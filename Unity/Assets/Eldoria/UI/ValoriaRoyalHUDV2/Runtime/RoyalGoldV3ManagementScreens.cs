using System;
using System.Collections.Generic;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// V3 management destinations on the existing Royal HUD canvas. Owns no save,
    /// clock, camera, gameplay scene or resource balance. All purchases use LocalGateway.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RoyalGoldV3ManagementScreens : MonoBehaviour
    {
        public enum Destination { Training, March }
        public RoyalHUD Hud { get; private set; }
        public bool IsOpen => root != null && root.gameObject.activeSelf;
        public Destination CurrentDestination { get; private set; }
        // Read-only observations of the real destination for native/browser QA.
        public RectTransform VisibleRoot => IsOpen ? root : null;
        public Button VisibleAction => IsOpen ? action : null;
        public Button VisibleBack => IsOpen ? back : null;
        public int SelectedQuantity => selectedCount;
        public int SelectedTrainingSeconds => SliceRules.RecruitSecondsFor(selectedCount);
        public int SelectedTier => selectedTier;
        ICommandGateway gateway;
        RoyalGoldV41MailboxScreen mailbox;
        Image approvedArcher,approvedTroopRow,trainingEnvironment,marchEnvironment;
        RectTransform marchHeadingSurface,marchTroopSurface;
        readonly RectTransform[] heroPortraitViewports=new RectTransform[3];
        readonly Dictionary<RectTransform,Image> paneSurfaces=new Dictionary<RectTransform,Image>();
        readonly Image[] approvedHeroes=new Image[3];
        Func<long> utcTicks;
        Action onCommand;
        // Optional host navigation; same scene, preserving the city camera state.
        public Action ReturnToCity { get; set; }
        public Action HeroesRequested { get; set; }
        public Func<bool> CanOpenHeroes { get; set; }
        RoyalButton heroesButton;
        Sprite trainingCardPortrait;
        readonly RoyalButton[] trainingTierChoices=new RoyalButton[2];
        readonly RoyalText[] trainingTierLabels=new RoyalText[2];
        readonly Image[] trainingTierPortraits=new Image[2];
        RectTransform root, layoutRoot, header, left, right, footer, quantityRow;
        bool compactLandscape;
        struct TextDensity { public int Font; public Vector2 Size; public Vector3 Scale; }
        readonly Dictionary<RoyalText,TextDensity> textDensity=new Dictionary<RoyalText,TextDensity>();
        RoyalGoldV3ManagementBackdropGraphic destinationArtwork, featuredUnitArtwork, statusArtwork;
        RoyalGoldV3LayeredFinish militaryDepth;
        RectTransform trainingProgress;
        RectTransform trainingProgressFill;
        RoyalText heading, subheading, unitTitle, unitDetail, selectedText, summary, status, feedback;
        RoyalButton minus, plus, action, back, switchDestination, chapterAction, tierButton;
        RoyalText chapterActionLabel;
        string selectedHero="aldric";
        readonly string[] heroSlots={"aldric",null,null};
        readonly RoyalButton[] heroSlotButtons=new RoyalButton[3];
        readonly RoyalText[] heroSlotLabels=new RoyalText[3];
        RoyalButton percentModeButton;RoyalText percentModeLabel;
        bool percentMode;
        int selectedT1,selectedT2;
        RoyalText actionLabel, switchLabel, tierLabel;
        Slider slider;
        int selectedCount = 10;
        int selectedTier = 1, speedMinutes = 1;
        int maxCount;
        float nextRefresh, lastWidth = -1, lastHeight = -1, submitAfter;
        string notice = "";
        string observedTask = "";
        int observedCount;
        bool deferredForModal;
        Destination deferredDestination;
        static readonly Color Ivory = new Color(.95f,.925f,.865f);
        static readonly Color Gold = new Color(.89f,.73f,.46f);
        static readonly Color Muted = new Color(.78f,.82f,.84f);

        public void Initialize(RoyalHUD hud, ICommandGateway authoritativeGateway,
            Func<long> authoritativeUtcTicks, Action refreshed)
        {
            if(Hud != hud && Hud != null)
            {
                Hud.SelectionChanged -= OnSelection;
                Hud.NarrativeOpening -= SuspendForNarrative;
            }
            if(Hud != hud && hud != null)
            {
                hud.SelectionChanged += OnSelection;
                hud.NarrativeOpening += SuspendForNarrative;
            }
            Hud = hud;
            gateway = authoritativeGateway;
            utcTicks = authoritativeUtcTicks;
            onCommand = refreshed;
            // One existing Royal HUD/Canvas, no parallel gameplay scene or save.
            if(hud!=null)
            {
                if(mailbox==null)mailbox=GetComponent<RoyalGoldV41MailboxScreen>();
                if(mailbox==null)mailbox=gameObject.AddComponent<RoyalGoldV41MailboxScreen>();
                mailbox.Initialize(hud,authoritativeGateway);
            }
        }

        void SuspendForNarrative()
        {
            // Invoked before the RoyalHUD narrative draws: the actual dialogue
            // Continue button must outrank every management destination.
            if(!IsOpen)return;
            deferredDestination=CurrentDestination;
            deferredForModal=true;
            root.gameObject.SetActive(false);
        }

        void OnSelection(string id)
        {
            if(gateway == null) return;
            if(string.IsNullOrEmpty(id))
            {
                deferredForModal=false;
                if(IsOpen){root.gameObject.SetActive(false);notice="";}
                return;
            }
            if(Hud != null && (Hud.NarrativeOpen ||
                (Hud.Modal != null && Hud.Modal.gameObject.activeSelf)))
            {
                if(id=="barracks" || id=="march-main")
                {
                    deferredDestination=id=="barracks"?Destination.Training:Destination.March;
                    deferredForModal=true;
                    if(IsOpen)root.gameObject.SetActive(false);
                }
                return;
            }
            // Canonical navigation clears selection before switching world/city.
            // Never leave a full-screen management destination over the world.
            if(IsOpen)
            {
                if(string.IsNullOrEmpty(id))
                {
                    root.gameObject.SetActive(false);
                    notice="";
                    return;
                }
                if(id=="march-main" && CurrentDestination!=Destination.March)
                    Open(Destination.March);
                else if(id=="barracks" && CurrentDestination!=Destination.Training)
                    Open(Destination.Training);
                return;
            }
            var s = gateway.Snapshot();
            if(s.BastionLevel < 2 || s.BarracksLevel < 1) return;
            if(id == "barracks") Open(Destination.Training);
            else if(id == "march-main") Open(Destination.March);
        }
        void OnDestroy()
        {
            if(trainingCardPortrait)Destroy(trainingCardPortrait);
            if(Hud != null)
            {
                Hud.SelectionChanged -= OnSelection;
                Hud.NarrativeOpening -= SuspendForNarrative;
            }
        }

        // Invoked from the canonical IRoyalSession, not by a duplicate HUD or modal.
        public bool TryOpen(RoyalAction offer)
        {
            if (gateway == null || offer == null) return false;
            var s = gateway.Snapshot();
            if (offer.Kind == "Recruit" && offer.Target == "archer:t1" &&
                s.BarracksLevel > 0 && s.BastionLevel >= 2)
            {
                Open(Destination.Training);
                return true;
            }
            if (offer.Kind == "ConfigureMarch" && offer.Target == "march-main" &&
                s.BarracksLevel > 0 && s.BastionLevel >= 2)
            {
                Open(Destination.March);
                return true;
            }
            return false;
        }

        public void Open(Destination destination)
        {
            if (gateway == null || Hud == null) return;
            var s = gateway.Snapshot();
            if (s.BastionLevel < 2 || s.BarracksLevel < 1) return;
            if(Hud.NarrativeOpen || (Hud.Modal != null && Hud.Modal.gameObject.activeSelf))
            {
                deferredDestination=destination;
                deferredForModal=true;
                if(IsOpen)root.gameObject.SetActive(false);
                return;
            }
            deferredForModal=false;
            if(mailbox!=null)mailbox.Close();
            EnsureView();
            CurrentDestination = destination;
            militaryDepth?.SetScene(destination==Destination.March?
                RoyalGoldV3LayeredFinish.Scene.March:
                RoyalGoldV3LayeredFinish.Scene.Training);
            destinationArtwork.MarchTheme = destination == Destination.March;
            destinationArtwork.SetVerticesDirty();
            featuredUnitArtwork.MarchTheme = destination == Destination.March;
            featuredUnitArtwork.SetVerticesDirty();
            statusArtwork.MarchTheme = destination == Destination.March;
            statusArtwork.SetVerticesDirty();
            submitAfter = 0f;
            selectedHero=s.MarchConfigured?s.PreparedHeroId:"aldric";
            for(int i=0;i<3;i++)heroSlots[i]=s.PreparedHeroIds!=null&&
                s.PreparedHeroIds.Count==3?s.PreparedHeroIds[i]:
                i==0?(selectedHero=="lyra"?"lyra":"aldric"):
                i==1&&selectedHero=="aldric+lyra"?"lyra":null;
            percentMode=false;
            int cap=MarchCapacityRules.Capacity(s,selectedHero);
            selectedT1=destination==Destination.March?
                (s.MarchConfigured?s.PreparedTroops.ArcherT1:Mathf.Min(s.Available.ArcherT1,cap)):0;
            selectedT2=destination==Destination.March?
                (s.MarchConfigured?s.PreparedTroops.ArcherT2:0):0;
            selectedTier=destination==Destination.Training&&s.RecruitmentCompletesUtcTicks>0&&s.PendingRecruitTier==2?2:
                destination==Destination.March&&s.MarchConfigured&&s.PreparedTroops.ArcherT2>0&&s.PreparedTroops.ArcherT1==0?2:1;
            int available=selectedTier==2?s.Available.ArcherT2:s.Available.ArcherT1;
            int prepared=selectedTier==2?s.PreparedTroops.ArcherT2:s.PreparedTroops.ArcherT1;
            selectedCount=destination==Destination.Training
                ?Mathf.Min(SliceRules.RecruitArchers,Math.Max(1,SliceRules.MaxAffordableArchers(s)))
                :Mathf.Min(Math.Max(1,available),s.MarchConfigured?Mathf.Max(1,prepared):Math.Max(1,available));
            notice = "";
            root.gameObject.SetActive(true);
            root.SetAsLastSibling();
            lastWidth = -1;
            Reflow();
            Refresh();
        }

        public void Close()
        {
            deferredForModal=false;
            if (root != null) root.gameObject.SetActive(false);
            notice = "";
            // World -> city is a canonical navigation request, never a scene reload.
            ReturnToCity?.Invoke();
            if (Hud != null) Hud.SelectBuilding("");
        }

        // Called by the native acceptance capture after switching the existing
        // canvas between real landscape/portrait render targets in one frame.
        // No scene/camera/gameplay mutation; runtime Update still handles resizing.
        public void SynchronizeLayout()
        {
            if(!IsOpen) return;
            Canvas.ForceUpdateCanvases();
            Reflow();
            Refresh();
        }

        void Update()
        {
            if(gateway == null || Hud == null)return;
            if(Hud.NarrativeOpen || (Hud.Modal != null && Hud.Modal.gameObject.activeSelf))
            {
                SuspendForNarrative();
                return;
            }
            if(deferredForModal && !IsOpen)
            {
                deferredForModal=false;
                string selected=Hud.SelectedBuildingId;
                if(selected=="barracks" || selected=="march-main")
                    Open(deferredDestination);
            }
            if(!IsOpen)return;
            root.SetAsLastSibling(); // Ensure the city and every other HUD layer stay behind.
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) { Close(); return; }
            if (Mathf.Abs(lastWidth-root.rect.width) > .5f ||
                Mathf.Abs(lastHeight-root.rect.height) > .5f) Reflow();
            if (Time.unscaledTime >= nextRefresh)
            {
                nextRefresh = Time.unscaledTime + .22f;
                Refresh();
            }
        }

        void EnsureView()
        {
            if (root != null) return;
            var go = new GameObject("V3 / Management destination", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image));
            root = (RectTransform)go.transform;
            root.SetParent(Hud.transform, false);
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;
            // Opaque, raycast-blocking royal midnight: the city is NOT visible through it.
            var bg = go.GetComponent<Image>();
            bg.color = new Color(.026f,.045f,.075f,1f);
            bg.raycastTarget = true;
            var artwork=Rect("V3 / Royal military hall artwork",root);
            artwork.anchorMin=Vector2.zero;
            artwork.anchorMax=Vector2.one;
            artwork.offsetMin=artwork.offsetMax=Vector2.zero;
            artwork.gameObject.AddComponent<CanvasRenderer>();
            var backdrop=artwork.gameObject.AddComponent<RoyalGoldV3ManagementBackdrop>();
            backdrop.color=Color.white;
            backdrop.raycastTarget=false;
            var themeArt=Rect("V3 / Training bow and marching compass",root);
            themeArt.anchorMin=Vector2.zero;
            themeArt.anchorMax=Vector2.one;
            themeArt.offsetMin=themeArt.offsetMax=Vector2.zero;
            themeArt.gameObject.AddComponent<CanvasRenderer>();
            destinationArtwork=themeArt.gameObject.AddComponent<RoyalGoldV3ManagementBackdropGraphic>();
            destinationArtwork.color=Color.white;
            destinationArtwork.raycastTarget=false;
            // Military atmosphere is a separate low-cost mesh behind every
            // genuine control rather than a second flat background image.
            militaryDepth=RoyalGoldV3LayeredFinish.Install(root,
                RoyalGoldV3LayeredFinish.Scene.Training,2);
            trainingEnvironment=RoyalGoldV41SourceArt.Surface(root,"training-courtyard-layer-v1");
            trainingEnvironment.rectTransform.SetAsLastSibling();
            var courtyardFit=trainingEnvironment.gameObject.AddComponent<AspectRatioFitter>();
            courtyardFit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;
            courtyardFit.aspectRatio=1.5f;
            marchEnvironment=RoyalGoldV41SourceArt.Surface(root,"training-courtyard-layer-v1");
            marchEnvironment.rectTransform.SetAsLastSibling();
            var marchFit=marchEnvironment.gameObject.AddComponent<AspectRatioFitter>();
            marchFit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;
            marchFit.aspectRatio=1.5f;
            marchEnvironment.gameObject.SetActive(false);
            layoutRoot=Rect("V3 / Responsive military layout",root);
            header = Frame("V3 / Destination header",layoutRoot,false);
            left = Frame("V3 / Featured military unit",layoutRoot,false);
            right = Frame("V3 / Authoritative status and cost",layoutRoot,false);
            footer = Frame("V3 / Destination actions",layoutRoot,false);
            // The existing status panel was a large empty blue rectangle
            // even while the real expedition/cost labels were visible.
            // Reuse the APPROVED military ornament at subdued opacity,
            // underneath the live counts and timers. Decorative only:
            // never create a second button, canvas, or pointer target.
            var statusDecoration=Rect("V3 / Subdued royal military status engraving",right);
            statusDecoration.anchorMin=Vector2.zero;
            statusDecoration.anchorMax=Vector2.one;
            statusDecoration.offsetMin=statusDecoration.offsetMax=Vector2.zero;
            statusDecoration.gameObject.AddComponent<CanvasRenderer>();
            statusArtwork=statusDecoration.gameObject.AddComponent<RoyalGoldV3ManagementBackdropGraphic>();
            statusArtwork.color=new Color(1f,1f,1f,.22f);
            statusArtwork.raycastTarget=false;
            statusDecoration.SetAsFirstSibling();
            // The world-sized tapestry is obscured by opaque readable cards.
            // Repeat its actual authored gold military heraldry INSIDE the
            // unit card, behind real controls: no placeholder empty expanse,
            // no new unit, external texture, canvas or paid image asset.
            var featured=Rect("V3 / Large unit heraldry",left);
            featured.anchorMin=Vector2.zero;
            featured.anchorMax=Vector2.one;
            featured.offsetMin=featured.offsetMax=Vector2.zero;
            featured.gameObject.AddComponent<CanvasRenderer>();
            featuredUnitArtwork=featured.gameObject.AddComponent<RoyalGoldV3ManagementBackdropGraphic>();
            featuredUnitArtwork.color=Color.white;
            featuredUnitArtwork.raycastTarget=false;
            heroesButton=Button("Open canonical heroes",header,"HÉROES",()=>{if(CanOpenHeroes!=null&&!CanOpenHeroes())return;Close();HeroesRequested?.Invoke();},false,out _);
            heading = Text("Destination title",header,27,Gold,true);
            subheading = Text("Royal gold description",header,15,Muted);
            unitTitle = Text("Unit or hero title",left,26,Ivory,true);
            tierButton=Button("Select archer tier",left,"ARQUEROS T1",()=>{if(CurrentDestination==Destination.Training&&gateway.Snapshot().RecruitmentCompletesUtcTicks>0)speedMinutes=speedMinutes==1?5:1;else selectedTier=selectedTier==1?2:1;Refresh();},false,out tierLabel);
            unitDetail = Text("Unit profile and composition",left,18,Muted);
            var rosterSource=RoyalGoldV41SourceArt.Get("archer-row");
            if(rosterSource)
            {
                var area=rosterSource.rect;
                trainingCardPortrait=Sprite.Create(rosterSource.texture,
                    new Rect(area.x+area.width*.22f,area.y,area.width*.58f,area.height),
                    new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            }
            for(int i=0;i<2;i++)
            {
                int tier=i+1;
                trainingTierChoices[i]=Button("Training unit card T"+tier,left,"T"+tier,()=>{
                    var state=gateway.Snapshot();
                    if(CurrentDestination!=Destination.Training||state.RecruitmentCompletesUtcTicks>0||
                        (tier==2&&state.BarracksLevel<4))return;
                    selectedTier=tier;Refresh();
                },false,out trainingTierLabels[i]);
                trainingTierPortraits[i]=RoyalGoldV41SourceArt.Surface(
                    (RectTransform)trainingTierChoices[i].transform,"archer-row");
                trainingTierPortraits[i].sprite=trainingCardPortrait;
                trainingTierPortraits[i].preserveAspect=true;
            }
            quantityRow = Rect("Quantity controls",left);
            minus = Button("Reduce quantity",quantityRow,"−",()=>SetQuantity(CurrentDestination==Destination.March&&percentMode?CurrentDisplayedPercent-1:selectedCount-1),false,out _);
            plus = Button("Increase quantity",quantityRow,"+",()=>SetQuantity(CurrentDestination==Destination.March&&percentMode?CurrentDisplayedPercent+1:selectedCount+1),false,out _);
            selectedText = Text("Authoritative quantity",quantityRow,26,Gold,true);
            selectedText.alignment = TextAnchor.MiddleCenter;
            var sliderGo = Rect("Quantity slider",left);
            sliderGo.gameObject.AddComponent<CanvasRenderer>();
            sliderGo.gameObject.AddComponent<Image>().color = new Color(.11f,.16f,.20f,1f);
            slider = sliderGo.gameObject.AddComponent<Slider>();
            slider.wholeNumbers = true;
            slider.minValue = 1f; slider.maxValue = 30f;
            var fillArea = Rect("Fill Area",sliderGo);
            fillArea.anchorMin = new Vector2(0,.28f);fillArea.anchorMax = new Vector2(1,.72f);
            fillArea.offsetMin = new Vector2(6,0);fillArea.offsetMax = new Vector2(-6,0);
            var fill = Rect("Fill",fillArea);
            fill.anchorMin = Vector2.zero;fill.anchorMax = Vector2.one;
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            fill.gameObject.AddComponent<CanvasRenderer>();
            var fillImage=fill.gameObject.AddComponent<Image>();
            fillImage.color=Gold;slider.fillRect=fill;
            var handle = Rect("Handle",sliderGo);
            handle.anchorMin = new Vector2(.5f,0);handle.anchorMax = new Vector2(.5f,1);
            handle.sizeDelta = new Vector2(20,0);
            handle.gameObject.AddComponent<CanvasRenderer>();
            var handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color=Ivory;slider.handleRect=handle;
            slider.targetGraphic=handleImage;
            slider.onValueChanged.AddListener(v=>SetQuantity(Mathf.RoundToInt(v)));
            // All animation and progress derive from the persisted UTC
            // deadline; closing the screen does not reset the training.
            trainingProgress=Rect("V3 / Training countdown progress",right);
            trainingProgress.gameObject.AddComponent<CanvasRenderer>();
            var progressBack=trainingProgress.gameObject.AddComponent<Image>();
            progressBack.color=new Color(.13f,.20f,.26f,.93f);
            progressBack.raycastTarget=false;
            trainingProgressFill=Rect("V3 / Real elapsed training progress",trainingProgress);
            trainingProgressFill.anchorMin=new Vector2(0,0);
            trainingProgressFill.anchorMax=new Vector2(0,1);
            trainingProgressFill.pivot=new Vector2(0,.5f);
            trainingProgressFill.anchoredPosition=Vector2.zero;
            trainingProgressFill.sizeDelta=Vector2.zero;
            trainingProgressFill.gameObject.AddComponent<CanvasRenderer>();
            var progressInk=trainingProgressFill.gameObject.AddComponent<Image>();
            progressInk.color=Gold; progressInk.raycastTarget=false;
            summary=Text("Training or expedition breakdown",right,20,Ivory);
            status=Text("Available and pending troops",right,16,Muted);
            feedback=Text("Command feedback",footer,15,Gold);
            back=Button("Return to Valoria",footer,"VOLVER A LA CIUDAD",Close,false,out _);
            action=Button("Confirm military command",footer,"CONFIRMAR",Confirm,true,out actionLabel);
            chapterAction=Button("Chapter III management",header,"MEJORAR CUARTEL",ChapterAction,false,out chapterActionLabel);
            switchDestination=Button("Switch destination",header,"MARCHAS",Switch,true,out switchLabel);
            for(int i=0;i<3;i++)
            {
                int slot=i;
                heroSlotButtons[i]=Button("Hero slot "+i,left,"",()=>CycleHeroSlot(slot),
                    false,out heroSlotLabels[i]);
            }
            percentModeButton=Button("Edit march by percent",left,"CANTIDAD / %",TogglePercentMode,
                false,out percentModeLabel);
            // Restore the reusable, authored military-hall architecture,
            // bow and compass BEHIND the approved manuscript textures.
            // The old implementation disabled these entirely and flattened
            // the whole room into four oversized rectangles.
            backdrop.enabled=true;
            destinationArtwork.enabled=true;
            destinationArtwork.color=new Color(1f,1f,1f,.52f);
            featuredUnitArtwork.enabled=true;
            featuredUnitArtwork.color=new Color(1f,1f,1f,.12f);
            statusArtwork.enabled=true;
            statusArtwork.color=new Color(1f,1f,1f,.12f);
            foreach(var pane in new[]{header,left,right,footer})
            {
                pane.GetComponent<RoyalGoldV2FrameGraphic>().enabled=false;
                // Semi-opaque approved parchment admits only a restrained
                // amount of scene depth; real numbers retain high contrast.
                var paper=RoyalGoldV41SourceArt.Surface(pane,"blue-panel",14);
                paper.color=new Color(1f,1f,1f,CurrentDestination==Destination.March?.78f:.94f);
                paneSurfaces[pane]=paper;
                // Antique rim and candlelight are independent of the texture.
                RoyalGoldV3LayeredFinish.Install(pane,
                    RoyalGoldV3LayeredFinish.Scene.Panel,1);
            }
            marchHeadingSurface=Rect("March / hero section cartouche",left);
            RoyalGoldV41SourceArt.Surface(marchHeadingSurface,"blue-panel",14);
            marchHeadingSurface.SetAsFirstSibling();
            marchTroopSurface=Rect("March / live troop order cartouche",left);
            RoyalGoldV41SourceArt.Surface(marchTroopSurface,"blue-panel",14);
            marchTroopSurface.SetAsFirstSibling();
            approvedArcher=RoyalGoldV41SourceArt.Surface(left,"training-archer-isolated-v1");
            approvedArcher.preserveAspect=true;approvedArcher.rectTransform.SetAsLastSibling();
            approvedTroopRow=RoyalGoldV41SourceArt.Surface(left,"archer-row");
            approvedTroopRow.preserveAspect=true;approvedTroopRow.rectTransform.SetAsLastSibling();
            for(int i=0;i<3;i++)
            {
                heroPortraitViewports[i]=Rect("Hero portrait viewport",heroSlotButtons[i].transform);
                heroPortraitViewports[i].gameObject.AddComponent<RectMask2D>();
                approvedHeroes[i]=RoyalGoldV41SourceArt.Surface(heroPortraitViewports[i],"aldric-face");
                approvedHeroes[i].preserveAspect=true;
            }
            var goldAction=RoyalGoldV41SourceArt.Surface((RectTransform)action.transform,"gold-button",14);
            action.GetComponent<RoyalGoldV2FrameGraphic>().enabled=false;
            goldAction.raycastTarget=true;action.targetGraphic=goldAction;
            actionLabel.color=new Color(.08f,.06f,.025f);
            root.gameObject.SetActive(false);
        }

        void Switch()
        {
            Open(CurrentDestination == Destination.Training ? Destination.March : Destination.Training);
        }
        void CycleHeroSlot(int slot)
        {
            if(CurrentDestination!=Destination.March || gateway==null)return;
            var s=gateway.Snapshot();
            bool hasLyra=s.ChapterThree!=null&&s.ChapterThree.LyraRecruited;
            if(slot==2)return; // A third combat hero has not been unlocked in the current story.
            if(slot==0)
            {
                string candidate=hasLyra&&heroSlots[0]=="aldric"?"lyra":"aldric";
                if(!MarchCapacityRules.HeroAvailable(s,candidate)){notice="El héroe está en otra marcha.";Refresh();return;}
                heroSlots[0]=candidate;
                if(heroSlots[1]==heroSlots[0])heroSlots[1]=null;
            }
            else if(hasLyra)
            {
                string candidate=string.IsNullOrEmpty(heroSlots[1])?(heroSlots[0]=="aldric"?"lyra":"aldric"):null;
                if(!MarchCapacityRules.HeroAvailable(s,candidate)){notice="El héroe está en otra marcha.";Refresh();return;}
                heroSlots[1]=candidate;
            }
            selectedHero=!string.IsNullOrEmpty(heroSlots[1])?"aldric+lyra":heroSlots[0];
            Reflow();Refresh();
        }
        void TogglePercentMode()
        {
            if(CurrentDestination!=Destination.March)return;
            percentMode=!percentMode;Refresh();
        }
        int SelectedFamilyCount=>selectedTier==2?selectedT2:selectedT1;
        int CurrentDisplayedPercent=>selectedT1+selectedT2>0?
            Mathf.RoundToInt(100f*SelectedFamilyCount/(selectedT1+selectedT2)):0;
        void SetFamilyCount(int value)
        {
            if(selectedTier==2)selectedT2=value;
            else selectedT1=value;
            selectedCount=value;
        }
        void SetQuantity(int requested)
        {
            if(CurrentDestination==Destination.Training)
            {
                selectedCount=Mathf.Clamp(requested,1,Mathf.Max(1,maxCount));
                slider.SetValueWithoutNotify(selectedCount);Refresh();return;
            }
            var s=gateway.Snapshot();
            int total=Mathf.Max(1,selectedT1+selectedT2);
            int availableHere=selectedTier==2?s.Available.ArcherT2:s.Available.ArcherT1;
            int availableOther=selectedTier==2?s.Available.ArcherT1:s.Available.ArcherT2;
            if(percentMode)
            {
                int desired=Mathf.RoundToInt(total*Mathf.Clamp(requested,0,100)/100f);
                int focus=Mathf.Clamp(desired,Mathf.Max(0,total-availableOther),
                    Mathf.Min(availableHere,total));
                SetFamilyCount(focus);
                if(selectedTier==2)selectedT1=total-focus;
                else selectedT2=total-focus;
            }
            else SetFamilyCount(Mathf.Clamp(requested,0,maxCount));
            Refresh();
        }
        void Refresh()
        {
            if (!IsOpen || gateway == null) return;
            var s=gateway.Snapshot();
            if(s.BastionLevel<2 || s.BarracksLevel<1){Close();return;}
            bool training=CurrentDestination==Destination.Training;
            approvedArcher.gameObject.SetActive(training);
            bool trainingActive=training && s.RecruitmentCompletesUtcTicks>0;
            bool tier2Unlocked=s.BarracksLevel>=4 && (training || s.Available.ArcherT2>0);
            for(int i=0;i<2;i++)
            {
                trainingTierChoices[i].gameObject.SetActive(training);
                bool unlocked=i==0||s.BarracksLevel>=4;
                trainingTierChoices[i].interactable=training&&!trainingActive&&unlocked;
                trainingTierPortraits[i].color=unlocked?Color.white:new Color(.45f,.45f,.45f,.75f);
                trainingTierLabels[i].color=selectedTier==i+1?Gold:Muted;
            }
            if(!tier2Unlocked && selectedTier==2)selectedTier=1;
            if(trainingActive)selectedTier=s.PendingRecruitTier==2?2:1;
            int minutes=SpeedMinutes(s);
            tierButton.gameObject.SetActive(trainingActive?s.SpeedupOneMinute>0&&s.SpeedupFiveMinutes>0:tier2Unlocked);
            tierLabel.Put(trainingActive?minutes+" MIN · CAMBIAR":"ARQUEROS T"+selectedTier+" · CAMBIAR");
            int selectedAvailable=selectedTier==2?s.Available.ArcherT2:s.Available.ArcherT1;
            int capacity=MarchCapacityRules.Capacity(s,selectedHero);
            if(!training)
            {
                selectedT1=Mathf.Clamp(selectedT1,0,s.Available.ArcherT1);
                selectedT2=Mathf.Clamp(selectedT2,0,s.Available.ArcherT2);
                if(selectedT1+selectedT2>capacity)
                {
                    int overflow=selectedT1+selectedT2-capacity;
                    int reduceT2=Mathf.Min(overflow,selectedT2);
                    selectedT2-=reduceT2;
                    selectedT1=Mathf.Max(0,selectedT1-(overflow-reduceT2));
                }
                selectedCount=SelectedFamilyCount;
            }
            maxCount=training?Mathf.Min(500,SliceRules.MaxAffordableArchers(s)):
                Mathf.Min(selectedAvailable,Mathf.Max(0,capacity-
                    (selectedTier==2?selectedT1:selectedT2)));
            // A pending operation has already spent its resources. Show the
            // real saved quantity rather than a newly affordable quantity.
            if(trainingActive)
            {
                selectedCount=Mathf.Clamp(s.PendingRecruitArchers,1,500);
                maxCount=Mathf.Max(maxCount,selectedCount);
            }
            else if(training && maxCount>0)selectedCount=Mathf.Clamp(selectedCount,1,maxCount);
            else if(training)selectedCount=1;
            slider.minValue=training?1:0;
            slider.maxValue=!training&&percentMode?100:Mathf.Max(1,maxCount);
            slider.SetValueWithoutNotify(!training&&percentMode?CurrentDisplayedPercent:selectedCount);
            slider.interactable=training?maxCount>1&&s.RecruitmentCompletesUtcTicks==0:
                (maxCount>0 || percentMode)&&!s.MarchConfigured; // Existing config remains amendable through reopen.
            if(!training)slider.interactable=maxCount>0 || (percentMode&&selectedT1+selectedT2>0);
            minus.interactable=(training?selectedCount>1:
                percentMode?CurrentDisplayedPercent>0:selectedCount>0)&&slider.interactable;
            plus.interactable=(training?selectedCount<maxCount:
                percentMode?CurrentDisplayedPercent<100:selectedCount<maxCount)&&slider.interactable;
            selectedText.Put(!training&&percentMode?CurrentDisplayedPercent+" %":
                maxCount>0?selectedCount.ToString():"—");
            heading.Put(training?"ENTRENAMIENTO":"SALA DE MARCHAS");
            subheading.Put(training?"VALORIA · RECLUTAMIENTO DE TROPAS":"VALORIA · PREPARACIÓN DE EXPEDICIONES");
            unitTitle.Put(training?"ARQUERO · T"+selectedTier:"HÉROES DE LA MARCHA");
            chapterAction.gameObject.SetActive(training&&s.BastionLevel>=3);
            chapterActionLabel.Put("MEJORAR CUARTEL");
            for(int i=0;i<3;i++)
            {
                heroSlotButtons[i].gameObject.SetActive(!training);
                heroSlotLabels[i].Put(string.IsNullOrEmpty(heroSlots[i])?
                    (i==2?"HÉROE 3 · BLOQUEADO":"HÉROE "+(i+1)+" · +"):
                    (i==0?"LÍDER · ":"APOYO · ")+HeroName(heroSlots[i]));
                heroSlotButtons[i].interactable=i<2 &&
                    (i==0||s.ChapterThree.LyraRecruited);
                approvedHeroes[i].sprite=string.IsNullOrEmpty(heroSlots[i])?
                    RoyalGoldV41SourceArt.Get(i==2?"hero-locked":"hero-support"):
                    RoyalGoldV41SourceArt.HeroPortrait(heroSlots[i]);
                if(!approvedHeroes[i].sprite)
                {
                    // Honest transient loading state: never show a white Image or another hero.
                    approvedHeroes[i].sprite=RoyalGoldV41SourceArt.Get("hero-support");
                    heroSlotLabels[i].Put((i==0?"LÍDER · ":"APOYO · ")+HeroName(heroSlots[i])+" · CARGANDO RETRATO");
                }
                approvedHeroes[i].gameObject.SetActive(!training);
            }
            approvedTroopRow.gameObject.SetActive(!training);
            percentModeButton.gameObject.SetActive(!training);
            percentModeLabel.Put(percentMode?"EDITAR POR %":"EDITAR CANTIDAD");
            unitDetail.Put(training?(compactLandscape?"":"Arqueros · nivel de tropa T"+selectedTier+"\n")+(selectedTier==2?"Desbloqueados por Cuartel IV":"Unidad inicial del reino"):
                "TROPAS · ARQUEROS T"+selectedTier+"\nAjusta cantidad o porcentaje.");
            switchLabel.Put(training?"PREPARAR MARCHA":"ENTRENAR TROPAS");
            switchDestination.interactable=true;
            bool busy=s.RecruitmentCompletesUtcTicks>0;
            if(training)
            {
                if(busy)
                {
                    string task=s.RecruitmentTaskId??"";
                    if(task!=observedTask){observedTask=task;observedCount=s.PendingRecruitArchers;}
                }
                else if(observedTask.Length>0)
                {
                    if(s.CompletedTaskIds.Contains(observedTask))notice="+"+observedCount+" arqueros incorporados a Valoria.";
                    observedTask="";
                }
                int wood=SliceRules.RecruitWoodCostFor(selectedCount);
                int stone=SliceRules.RecruitStoneCostFor(selectedCount);
                int seconds=SliceRules.RecruitSecondsFor(selectedCount);
                long leftSeconds=busy?Math.Max(0,(long)Math.Ceiling(
                    (s.RecruitmentCompletesUtcTicks-utcTicks())/(double)TimeSpan.TicksPerSecond)):0;
                summary.Put(
                    "Madera "+wood+" · Piedra "+stone+"\nTiempo total  "+Clock(seconds));
                trainingProgress.gameObject.SetActive(busy);
                if(busy)
                {
                    var elapsed=Mathf.Clamp01(1f-leftSeconds/(float)Math.Max(1,
                        SliceRules.RecruitSecondsFor(s.PendingRecruitArchers)));
                    trainingProgressFill.sizeDelta=new Vector2(
                        trainingProgress.rect.width*elapsed,0f);
                }
                status.Put("Disponibles T"+selectedTier+"  "+selectedAvailable+" · Entrenando "+s.PendingRecruitArchers+
                    (busy?"\nCUENTA ATRÁS  "+Clock(leftSeconds):"\nSin entrenamiento activo"));
                actionLabel.Put(busy?((s.SpeedupFiveMinutes>0||s.SpeedupOneMinute>0)?"ACELERAR · "+minutes+" MIN ("+(minutes==5?s.SpeedupFiveMinutes:s.SpeedupOneMinute)+")":
                    "ENTRENANDO T"+s.PendingRecruitTier+" · "+Clock(leftSeconds)):
                    "ENTRENAR "+selectedCount+" ARQUEROS T"+selectedTier);
                action.interactable=(busy?(s.SpeedupFiveMinutes>0||s.SpeedupOneMinute>0):maxCount>0)&&Time.unscaledTime>=submitAfter;
            }
            else
            {
                trainingProgress.gameObject.SetActive(false);
                var troops=new ArmyRoster{ArcherT1=selectedT1,ArcherT2=selectedT2};
                var power=SliceRules.Expedition(troops,selectedHero);
                int total=troops.Total;
                int pctT1=total>0?Mathf.RoundToInt(100f*selectedT1/total):0;
                summary.Put("TROPAS DE LA MARCHA\n"+
                    "Arqueros T1  "+selectedT1+" · "+pctT1+" %"+
                    "\nArqueros T2  "+selectedT2+" · "+(total>0?100-pctT1:0)+" %"+
                    "\n\nPODER  "+power.Power+
                    "\nATAQUE  "+power.Attack+"   DEFENSA  "+power.Defense+
                    "\nVIDA  "+power.Health+"   RUPTURA  "+power.Break);
                int occupied=MarchCapacityRules.ActiveCount(s,utcTicks());
                status.Put("DISPONIBLES T1  "+s.Available.ArcherT1+" · T2  "+s.Available.ArcherT2+
                    "\nCAPACIDAD DE "+HeroName(selectedHero)+"  "+MarchCapacityRules.Capacity(s,selectedHero)+
                    "\nNIVEL HÉROE  "+MarchCapacityRules.HeroLevel(s,selectedHero)+
                    " · MARCHAS ACTIVAS  "+occupied+"/"+MarchCapacityRules.MaxConcurrentMarches+
                    "\nYA PREPARADOS  "+(s.MarchConfigured?s.PreparedTroops.Total:0));
                bool allowed=MarchCapacityRules.CanStart(s,utcTicks()) && troops.Total>0 &&
                    MarchCapacityRules.Fits(troops,s.Available,capacity);
                foreach(var hero in heroSlots)if(!MarchCapacityRules.HeroAvailable(s,hero))allowed=false;
                actionLabel.Put(allowed?"CONFIRMAR COMPOSICIÓN":"MARCHA NO DISPONIBLE");
                action.interactable=allowed && Time.unscaledTime>=submitAfter;
            }
            feedback.Put(notice);
        }
        static string HeroName(string id)=>id=="lyra"?"LYRA":id=="aldric+lyra"?"SIR ALDRIC · LYRA":"SIR ALDRIC";
        void ChapterAction()
        {
            if(CurrentDestination==Destination.Training){root.gameObject.SetActive(false);Hud.SelectBuilding("upgrade-barracks");}
            else CycleHeroSlot(0);
        }
        int SpeedMinutes(PlayerState s){if(speedMinutes==5&&s.SpeedupFiveMinutes==0)speedMinutes=1;if(speedMinutes==1&&s.SpeedupOneMinute==0)speedMinutes=5;return speedMinutes;}
        void Confirm()
        {
            if(!IsOpen||Time.unscaledTime<submitAfter)return;
            var s=gateway.Snapshot();
            bool training=CurrentDestination==Destination.Training;
            if(training&&s.RecruitmentCompletesUtcTicks>0){
                if(s.SpeedupFiveMinutes<=0&&s.SpeedupOneMinute<=0)return;
                submitAfter=Time.unscaledTime+.45f;
                var accelerated=gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),
                    s.PlayerId,"UseSpeedup",SpeedMinutes(s)==5?"training:5m":"training:1m",s.Revision));
                notice=accelerated.Message;onCommand?.Invoke();Hud.Refresh();Refresh();return;
            }
            if(training&&selectedCount>SliceRules.MaxAffordableArchers(s))return;
            var selectedTroops=new ArmyRoster{ArcherT1=selectedT1,ArcherT2=selectedT2};
            if(!training&&(!MarchCapacityRules.CanStart(s,utcTicks()) ||
                !MarchCapacityRules.Fits(selectedTroops,s.Available,
                    MarchCapacityRules.Capacity(s,selectedHero))))return;
            // The gateway independently validates all three slots and every reserved unit.
            // This interface reveals only narratively unlocked heroes and troop tiers.
            string target=training?"archer:t"+selectedTier+":"+selectedCount:
                "march-main:"+selectedT1+":"+selectedT2+":0:"+
                (heroSlots[0]??"")+":"+(heroSlots[1]??"")+":"+(heroSlots[2]??"");
            string kind=training?"Recruit":"ConfigureMarch";
            submitAfter=Time.unscaledTime+.45f;
            var result=gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),
                s.PlayerId,kind,target,s.Revision));
            notice=result.Ok?(training?"Entrenamiento iniciado. Puedes regresar a la ciudad.":
                "Marcha preparada y guardada."):result.Message;
            onCommand?.Invoke();
            Hud.Refresh();
            Refresh();
        }
        static string Clock(long seconds)
        {
            long h=seconds/3600,m=(seconds/60)%60,s=seconds%60;
            return h>0?h+":"+m.ToString("00")+":"+s.ToString("00"):
                m.ToString("00")+":"+s.ToString("00");
        }

        void Reflow()
        {
            if(root==null)return;
            foreach(var item in textDensity)
            {
                if(!item.Key)continue;
                item.Key.fontSize=item.Value.Font;
                item.Key.rectTransform.sizeDelta=item.Value.Size;
                item.Key.rectTransform.localScale=item.Value.Scale;
            }
            textDensity.Clear();
            lastWidth=root.rect.width;lastHeight=root.rect.height;
            float w=lastWidth,h=lastHeight;
            if(w<1||h<1)return;
            bool landscape=w>h;
            // Production RoyalScaler matches height; native portrait probes
            // historically matched width. Normalize only this room so both
            // use the same 800-unit design and real physical text sizes.
            compactLandscape=landscape&&w/h>=1.5f&&
                h*(Hud.GetComponentInParent<Canvas>()?.scaleFactor??1f)<650f;
            float roomScale=landscape?(compactLandscape?h/600f:1f):Mathf.Min(1f,w/800f);
            w/=roomScale;h/=roomScale;
            layoutRoot.anchorMin=layoutRoot.anchorMax=layoutRoot.pivot=new Vector2(.5f,.5f);
            layoutRoot.anchoredPosition=Vector2.zero;
            layoutRoot.sizeDelta=new Vector2(w,h);
            layoutRoot.localScale=Vector3.one*roomScale;
            foreach(var button in root.GetComponentsInChildren<Button>(true))
                foreach(var label in button.GetComponentsInChildren<RoyalText>(true))label.fontSize=17;
            float canvasScale=(Hud.GetComponentInParent<Canvas>()?.scaleFactor??1f)*roomScale;
            Rect area=Screen.safeArea;
            float insetLeft=Screen.width>0?area.xMin/Mathf.Max(.01f,canvasScale):0;
            float insetRight=Screen.width>0?(Screen.width-area.xMax)/Mathf.Max(.01f,canvasScale):0;
            float side=Mathf.Max(18,w*.027f);
            float leftSide=Mathf.Max(side,insetLeft+12),rightSide=Mathf.Max(side,insetRight+12);
            float usable=w-leftSide-rightSide;
            float safeTop=Screen.height>0?(Screen.height-area.yMax)/Mathf.Max(.01f,canvasScale):0;
            float safeBottom=Screen.height>0?area.yMin/Mathf.Max(.01f,canvasScale):0;
            float top=Mathf.Max(Mathf.Max(10,h*.024f),safeTop+8);
            float bottom=Mathf.Max(Mathf.Max(10,h*.024f),safeBottom+8);
            float headerH=landscape?Mathf.Clamp(h*.205f,88,95):196;
            float footH=landscape?Mathf.Clamp(h*.21f,70,92):Mathf.Clamp(h*.155f,102,132);
            float bodyY=top+headerH+8;
            float bodyH=Mathf.Max(120,h-bodyY-footH-bottom-8);
            float gap=landscape?Mathf.Clamp(w*.018f,12,25):10;
            Place(header,leftSide,top,usable,headerH);
            if(landscape)
            {
                float lw=(usable-gap)*.49f;
                Place(left,leftSide,bodyY,lw,bodyH);
                Place(right,leftSide+lw+gap,bodyY,usable-lw-gap,bodyH);
            }
            else
            {
                float lh=(bodyH-gap)*.48f;
                Place(left,leftSide,bodyY,usable,lh);
                Place(right,leftSide,bodyY+lh+gap,usable,bodyH-lh-gap);
            }
            Place(footer,leftSide,h-bottom-footH,usable,footH);
            float hw=header.rect.width;
            // Keep the destination discoverable while its narrative prerequisite is locked.
            // Interactability and both destination guards still enforce Lyra recruitment.
            heroesButton.gameObject.SetActive(HeroesRequested!=null);
            heroesButton.interactable=HeroesRequested!=null&&(CanOpenHeroes==null||CanOpenHeroes());
            Place(heroesButton.transform as RectTransform,hw*.50f,landscape?7:9,hw*.19f,landscape?52:60);
            Place(heading.rectTransform,landscape?14:15,7,hw*.47f,landscape?40:37);
            Place(subheading.rectTransform,landscape?14:15,landscape?49:47,hw*.47f,26);
            Place(switchDestination.transform as RectTransform,hw*.71f,landscape?4:7,hw*.27f,landscape?30:34);
            Place(chapterAction.transform as RectTransform,hw*.71f,landscape?36:43,hw*.27f,landscape?27:31);
            float uw=left.rect.width,uh=left.rect.height;
            Place(unitTitle.rectTransform,landscape?15:18,landscape?7:18,uw*.61f,landscape?32:40);
            Place(tierButton.transform as RectTransform,uw*.64f,landscape?7:18,uw*.33f,landscape?32:40);
            Place(unitDetail.rectTransform,landscape?15:18,landscape?46:69,uw-35,landscape?42:60);
            // New controls stay entirely in existing left pane; never allocate a second
            // canvas. These are functional placements, NOT final visual approval.
            float heroY=landscape?Mathf.Min(100,uh*.31f):Mathf.Min(90,uh*.42f);
            float slotW=(uw-30-12)/3f;
            for(int i=0;i<3;i++)
                Place(heroSlotButtons[i].transform as RectTransform,
                    15+i*(slotW+6),heroY,slotW,landscape?37:32);
            float percentY=Mathf.Min(heroY+42,Mathf.Max(48,uh-110));
            Place(percentModeButton.transform as RectTransform,
                15,percentY,uw-30,landscape?29:27);
            float qy=landscape?Mathf.Max(90,uh-95):uh-129;
            Place(quantityRow,landscape?14:20,qy,uw-(landscape?28:40),landscape?39:48);
            float qw=quantityRow.rect.width;
            float ctrl=landscape?43:50;
            Place(minus.transform as RectTransform,0,0,ctrl,landscape?39:48);
            Place(plus.transform as RectTransform,qw-ctrl,0,ctrl,landscape?39:48);
            Place(selectedText.rectTransform,ctrl+8,0,qw-2*(ctrl+8),landscape?39:48);
            Place(slider.transform as RectTransform,landscape?19:25,uh-(landscape?21:45),uw-(landscape?38:50),landscape?14:19);
            float rw=right.rect.width,rh=right.rect.height;
            // March capacity/hero/global slots add five live lines. Allocate their
            // actual text height rather than clipping them into the old three-line box.
            float statusH=landscape?Mathf.Min(CurrentDestination==Destination.March?118f:90f,rh*.44f):
                Mathf.Min(CurrentDestination==Destination.March?132f:108f,rh*.40f);
            Place(summary.rectTransform,landscape?14:19,landscape?10:18,rw-(landscape?28:38),
                rh-statusH-(landscape?18:22));
            Place(status.rectTransform,landscape?14:19,rh-statusH-4,rw-(landscape?28:38),statusH);
            Place(trainingProgress,landscape?21:23,rh-statusH-(landscape?44:49),
                rw-(landscape?42:46),13);
            float fw=footer.rect.width;
            Place(feedback.rectTransform,14,4,fw-28,landscape?20:25);
            float bw=Mathf.Max(90,(fw-42)*.48f);
            float by=landscape?25:footH-82;
            Place(back.transform as RectTransform,12,by,bw,landscape?42:76);
            Place(action.transform as RectTransform,fw-12-bw,by,bw,landscape?42:76);
            // Crónica Real: a military manuscript must be read without zoom,
            // including genuine counts, costs and countdowns on landscape.
            // The earlier 12-13pt landscape type was effectively illegible.
            heading.fontSize=landscape?Mathf.RoundToInt(Mathf.Clamp(h*.064f,23,29)):24;
            subheading.fontSize=landscape?15:16;
            unitTitle.fontSize=landscape?23:25;
            unitDetail.fontSize=landscape?16:18;
            selectedText.fontSize=landscape?24:25;
            summary.fontSize=landscape?Mathf.RoundToInt(Mathf.Clamp(h*.046f,17,23)):19;
            status.fontSize=landscape?15:16;
            actionLabel.fontSize=landscape?16:17;
            switchLabel.fontSize=landscape?15:16;
            feedback.fontSize=landscape?14:15;
            if(!landscape)
            {
                // Two independent header rows: long Spanish titles must never
                // wrap into the subtitle or underneath navigation controls.
                Place(heading.rectTransform,15,8,hw-30,44);
                Place(subheading.rectTransform,15,58,hw-30,38);
                float navWidth=(hw-42)/3f;
                Place(heroesButton.transform as RectTransform,12,110,navWidth,72);
                Place(switchDestination.transform as RectTransform,21+navWidth,110,navWidth,72);
                Place(chapterAction.transform as RectTransform,30+2*navWidth,110,navWidth,72);
                heading.fontSize=32;
                switchLabel.fontSize=14;
                chapterActionLabel.fontSize=14;
            }
            if(compactLandscape)
            {
                Place(switchDestination.transform as RectTransform,hw*.71f,4,hw*.27f,34);
                Place(chapterAction.transform as RectTransform,hw*.71f,42,hw*.27f,34);
            }
            ReflowApprovedSurfaces(leftSide,top,usable,bodyY,bodyH,gap,landscape);
            ApplyMilitaryComposition(w,h,leftSide,rightSide,top,bottom,landscape);
            if(!landscape)
            {
                // Portrait captures use 800 canvas units: 26 units remain
                // readable at 390 px. Keep the same live controls and art.
                foreach(var label in root.GetComponentsInChildren<RoyalText>(true))
                    label.fontSize=Mathf.Max(26,label.fontSize);
                if(CurrentDestination!=Destination.Training)
                    Place(feedback.rectTransform,14,4,fw-28,44);
            }
            if(compactLandscape)
            {
                foreach(var label in layoutRoot.GetComponentsInChildren<RoyalText>(true))
                {
                    label.fontSize=Mathf.Max(20,label.fontSize);
                    var r=label.rectTransform;
                    textDensity[label]=new TextDensity{Font=label.fontSize,Size=r.sizeDelta,Scale=r.localScale};
                    var size=r.rect.size;
                    label.fontSize=Mathf.CeilToInt(label.fontSize*roomScale);
                    r.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,size.x*roomScale);
                    r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,size.y*roomScale);
                    r.localScale/=roomScale;
                }
            }

        }
        // Reference-led composition: the character occupies the courtyard, while
        // only the actual order controls receive a compact material surface.
        // No domain command, price, clock, save or troop availability is replaced.
        void ApplyMilitaryComposition(float w,float h,float x,float insetRight,float top,float bottom,bool wide)
        {
            bool training=CurrentDestination==Destination.Training;
            trainingEnvironment.gameObject.SetActive(training);
            marchEnvironment.gameObject.SetActive(!training);
            marchHeadingSurface.gameObject.SetActive(!training);
            marchTroopSurface.gameObject.SetActive(!training);
            destinationArtwork.enabled=false;
            featuredUnitArtwork.enabled=false;
            statusArtwork.enabled=false;
            militaryDepth.gameObject.SetActive(false);
            foreach(var pane in new[]{header,left,right,footer})
            {
                paneSurfaces[pane].enabled=pane==header||pane==right;
                pane.GetComponent<RoyalGoldV2FrameGraphic>().enabled=
                    pane==header||pane==right;
                foreach(var finish in pane.GetComponentsInChildren<RoyalGoldV3LayeredFinish>(true))
                    finish.gameObject.SetActive(false);
            }
            unitTitle.color=training?new Color(.10f,.08f,.055f):Ivory;
            unitDetail.color=training?new Color(.16f,.13f,.10f):Muted;
            action.transform.SetParent(training?right:footer,false);
            if(!training)return;
            tierButton.transform.SetParent(right,false);
            float width=w-x-insetRight;
            float hh=wide?86:188;
            float foot=wide?54:90;
            float bodyY=top+hh+12, bodyH=h-bodyY-bottom-foot;
            Place(header,x,top,width,hh);
            Place(heading.rectTransform,16,8,width*.48f,40);
            Place(subheading.rectTransform,16,56,width*.48f,26);
            if(!wide)
            {
                Place(heading.rectTransform,16,8,width-32,44);
                Place(subheading.rectTransform,16,55,width-32,32);
                float n=(width-48)/3;
                Place(heroesButton.transform as RectTransform,12,99,n,76);
                Place(switchDestination.transform as RectTransform,24+n,99,n,76);
                Place(chapterAction.transform as RectTransform,36+2*n,99,n,76);
            }
            // The approved composition places a full-height archer on the
            // courtyard floor and the order below the unit identity at right.
            // Narrow portrait keeps those same layers, stacking only the order.
            float portraitOrderH=Mathf.Min(540,bodyH*.52f);
            float artH=wide?bodyH:bodyH-portraitOrderH-12;
            Place(left,x,bodyY,wide?width*.54f:width,artH);
            float archerH=Mathf.Min(artH, (wide?width*.51f:width*.65f)*1.5f);
            float archerW=archerH/1.5f;
            Place(approvedArcher.rectTransform,width*(wide?.10f:.04f),artH-archerH,archerW,archerH);
            float rosterW=Mathf.Clamp(width*.14f,110f,150f);
            float rosterH=wide?Mathf.Min(150f,artH*.29f):160f;
            for(int i=0;i<2;i++)
            {
                Place((RectTransform)trainingTierChoices[i].transform,8,artH*.18f+i*(rosterH+12),rosterW,rosterH);
                Place(trainingTierPortraits[i].rectTransform,6,6,rosterW-12,rosterH-48);
                Place(trainingTierLabels[i].rectTransform,6,rosterH-40,rosterW-12,34);
            }
            unitTitle.rectTransform.SetParent(left,false);
            unitDetail.rectTransform.SetParent(left,false);
            bool stackedIdentity=wide&&!compactLandscape;
            // Reserve a separate identity column: the previous compact title crossed the face.
            float titleX=stackedIdentity?width*.56f:wide?width*.36f:width*.67f;
            float titleW=stackedIdentity?width*.44f:wide?left.rect.width-titleX:width-titleX-12;
            Place(unitTitle.rectTransform,titleX,artH*.04f,titleW,wide?72:90);
            Place(unitDetail.rectTransform,titleX,artH*.04f+(wide?82:100),titleW,wide?160:230);
            unitTitle.fontSize=wide?28:34;
            unitDetail.fontSize=wide?20:28;
            float orderW=wide?width*.44f:width;
            float orderH=wide?(compactLandscape?bodyH:Mathf.Min(bodyH,Mathf.Max(390,bodyH*.60f))):portraitOrderH;
            Place(right,wide?x+width-orderW:x,wide?bodyY+bodyH-orderH:bodyY+artH+12,orderW,orderH);
            float pad=wide?22:26;
            float qh=wide?48:78;
            Place(quantityRow,pad,orderH*.04f,orderW-pad*2,qh);
            Place(slider.transform as RectTransform,pad+6,orderH*.04f+qh+10,orderW-pad*2-12,wide?16:24);
            Place(summary.rectTransform,pad,orderH*.28f,orderW-pad*2,orderH*.20f);
            Place(tierButton.transform as RectTransform,pad,orderH*.51f,orderW-pad*2,wide?34:52);
            Place(status.rectTransform,pad,orderH*.63f,orderW-pad*2,wide?48:70);
            Place(trainingProgress,pad,orderH*.61f-8,orderW-pad*2,8);
            Place(action.transform as RectTransform,pad,orderH-(wide?65:94),orderW-pad*2,wide?52:80);
            summary.fontSize=wide?22:28;status.fontSize=wide?17:26;
            actionLabel.fontSize=wide?19:26;
            float qw=quantityRow.rect.width;
            Place(minus.transform as RectTransform,0,0,qh,qh);
            Place(plus.transform as RectTransform,qw-qh,0,qh,qh);
            Place(selectedText.rectTransform,qh+4,0,qw-2*qh-8,qh);
            Place(footer,x,h-bottom-foot,width,foot);
            Place(back.transform as RectTransform,0,8,wide?width*.30f:width*.40f,foot-12);
            Place(feedback.rectTransform,wide?width*.32f:width*.43f,8,wide?width*.68f:width*.57f,foot-12);
            if(!wide)
            {
                // The courtyard must share the character's viewport. Fitting it
                // to the entire tall phone put its floor underneath the order,
                // leaving the otherwise bottom-aligned character in the sky.
                trainingEnvironment.rectTransform.SetParent(left,false);
                Place(quantityRow,pad,20,orderW-pad*2,qh);
                Place(slider.transform as RectTransform,pad+6,110,orderW-pad*2-12,24);
                Place(summary.rectTransform,pad,154,orderW-pad*2,72);
                Place(tierButton.transform as RectTransform,pad,240,orderW-pad*2,52);
                Place(trainingProgress,pad,312,orderW-pad*2,8);
                Place(status.rectTransform,pad,340,orderW-pad*2,70);
            }
            else
            {
                trainingEnvironment.rectTransform.SetParent(root,false);
                trainingEnvironment.rectTransform.SetSiblingIndex(Mathf.Max(0,layoutRoot.GetSiblingIndex()-1));
            }
            // Explicit stacking retains live controls above source surfaces.
            approvedArcher.rectTransform.SetAsFirstSibling();
            if(!wide)trainingEnvironment.rectTransform.SetAsFirstSibling();
        }

        void ReflowApprovedSurfaces(float x,float top,float width,float y,float height,float gap,bool wide)
        {
            bool training=CurrentDestination==Destination.Training;
            unitTitle.rectTransform.SetParent(training?right:left,false);
            unitDetail.rectTransform.SetParent(training?right:left,false);
            quantityRow.SetParent(training?right:left,false);
            slider.transform.SetParent(training?right:left,false);
            tierButton.transform.SetParent(training?right:left,false);
            if(training)
            {
                if(!wide)
                {
                    // A portrait phone needs a compact illustration and unit
                    // identity above a full-width order, not two very tall columns.
                    float artH=Mathf.Min(height*.40f,360f);
                    Place(left,x,y,width,artH);
                    Place(right,x,y+artH+gap,width,height-artH-gap);
                    float artW=Mathf.Min(width*.40f,artH*184f/476f);
                    Place(approvedArcher.rectTransform,18,0,artW,artH);
                    unitTitle.rectTransform.SetParent(left,false);
                    unitDetail.rectTransform.SetParent(left,false);
                    tierButton.transform.SetParent(left,false);
                    float textX=Mathf.Max(width*.43f,artW+36),textW=width-textX-18;
                    Place(unitTitle.rectTransform,textX,24,textW,50);
                    Place(unitDetail.rectTransform,textX,90,textW,116);
                    Place(tierButton.transform as RectTransform,textX,artH-86,textW,72);
                    float rh=right.rect.height;
                    Place(summary.rectTransform,20,20,width-40,260);
                    Place(quantityRow,20,rh*.46f,width-40,88);
                    Place(slider.transform as RectTransform,24,rh*.46f+104,width-48,24);
                    Place(status.rectTransform,20,rh*.70f,width-40,rh*.30f-14);
                    Place(trainingProgress,20,rh*.70f-22,width-40,10);
                    summary.fontSize=18;status.fontSize=16;unitDetail.fontSize=17;
                }
                else
                {
                float artWidth=width*.43f;
                Place(left,x,y,artWidth,height);
                Place(right,x+artWidth+gap,y,width-artWidth-gap,height);
                float portraitWidth=Mathf.Min(artWidth,height*184f/476f);
                Place(approvedArcher.rectTransform,(artWidth-portraitWidth)*.5f,0,portraitWidth,height);
                float rw=right.rect.width;
                Place(unitTitle.rectTransform,14,12,rw-28,34);
                Place(unitDetail.rectTransform,14,50,rw-28,height*.18f);
                Place(tierButton.transform as RectTransform,14,height*.23f,rw-28,34);
                Place(summary.rectTransform,14,height*.33f,rw-28,height*.29f);
                Place(quantityRow,14,height*.64f,rw-28,42);
                Place(slider.transform as RectTransform,20,height*.64f+50,rw-40,18);
                Place(status.rectTransform,14,height*.82f,rw-28,height*.18f-5);
                Place(trainingProgress,14,height*.79f,rw-28,8);
                summary.fontSize=wide?18:16;status.fontSize=wide?14:13;
                unitDetail.fontSize=wide?17:15;
                if(compactLandscape)
                {
                    Place(unitDetail.rectTransform,14,50,rw-28,30);
                    Place(summary.rectTransform,14,height*.33f,rw-28,height*.23f);
                    Place(quantityRow,14,height*.60f,rw-28,68);
                    Place(slider.transform as RectTransform,20,height*.60f+74,rw-40,12);
                    Place(status.rectTransform,14,height*.84f,rw-28,height*.16f-4);
                    Place(trainingProgress,14,height-9,rw-28,5);
                }
                }
            }
            else
            {
                float lw=wide?(width-gap)*.60f:width;
                float lh=wide?height:height*.62f;
                Place(left,x,y,lw,lh);
                Place(right,wide?x+lw+gap:x,wide?y:y+lh+gap,
                    wide?width-lw-gap:width,wide?height:height-lh-gap);
                Place(marchHeadingSurface,8,0,lw-16,wide?38:58);
                Place(unitTitle.rectTransform,14,8,lw-28,wide?30:50);
                float cardsY=wide?43:65,cardsH=Mathf.Max(60,lh*(compactLandscape?.36f:wide?.48f:.42f)),cardW=(lw-40)/3f;
                for(int i=0;i<3;i++)
                {
                    Place((RectTransform)heroSlotButtons[i].transform,14+i*(cardW+6),cardsY,cardW,cardsH);
                    float portraitH=compactLandscape?Mathf.Max(40,cardsH-70):cardsH*.70f;
                    var portraitSprite=approvedHeroes[i].sprite;
                    float ratio=portraitSprite?portraitSprite.rect.width/Mathf.Max(1f,portraitSprite.rect.height):1f;
                    // Fill the portrait aperture with authentic pixels, crop only
                    // inside its mask, and keep live names outside that aperture.
                    float apertureW=cardW-16;
                    Place(heroPortraitViewports[i],8,6,apertureW,portraitH);
                    float portraitW=Mathf.Max(apertureW,portraitH*ratio);
                    float portraitHeight=portraitW/ratio;
                    // Faces sit near the upper fifth of these original portraits.
                    // A top-aligned cover crop showed only foreheads on 844x390.
                    float cropOffset=string.IsNullOrEmpty(heroSlots[i])?.5f:.18f;
                    Place(approvedHeroes[i].rectTransform,(apertureW-portraitW)*.5f,
                        -(portraitHeight-portraitH)*cropOffset,portraitW,portraitHeight);
                    Place(heroSlotLabels[i].rectTransform,4,compactLandscape?cardsH-60:cardsH*(wide?.74f:.72f),cardW-8,compactLandscape?54:cardsH*(wide?.24f:.26f));
                    heroSlotLabels[i].fontSize=wide?14:13;
                    if(compactLandscape)
                    {
                        // A horizontal phone cannot support a short panoramic
                        // portrait strip: retain the complete original image
                        // beside the actual hero name in the same touch target.
                        float imageAreaW=cardW*.40f,imageAreaH=cardsH-12;
                        Place(heroPortraitViewports[i],6,6,imageAreaW,imageAreaH);
                        float imageW=Mathf.Min(imageAreaW,imageAreaH*ratio),imageH=imageW/ratio;
                        Place(approvedHeroes[i].rectTransform,(imageAreaW-imageW)*.5f,
                            (imageAreaH-imageH)*.5f,imageW,imageH);
                        Place(heroSlotLabels[i].rectTransform,cardW*.45f,8,cardW*.52f,cardsH-16);
                    }
                }
                float rowY=cardsY+cardsH+8;
                Place(marchTroopSurface,8,rowY-4,lw-16,lh-rowY-4);
                Place(approvedTroopRow.rectTransform,14,rowY,lw*.29f,lh-rowY-18);
                Place(unitDetail.rectTransform,lw*.34f,rowY,lw*.63f,wide?40:76);
                Place(tierButton.transform as RectTransform,lw*.34f,rowY+(wide?42:84),lw*.63f,wide?32:80);
                Place(percentModeButton.transform as RectTransform,lw*.34f,rowY+(wide?82:172),lw*(wide?.28f:.24f),wide?32:88);
                Place(quantityRow,lw*(wide?.65f:.61f),rowY+(wide?82:172),lw*(wide?.32f:.36f),wide?38:88);
                Place(slider.transform as RectTransform,lw*.34f,rowY+(wide?129:270),lw*.63f,wide?16:24);
                float rw=right.rect.width,rh=right.rect.height;
                Place(summary.rectTransform,14,8,rw-28,rh*.58f);
                Place(status.rectTransform,14,rh*.60f,rw-28,rh*.38f);
                summary.fontSize=wide?18:16;status.fontSize=wide?14:13;
                unitDetail.fontSize=14;
                if(compactLandscape)
                {
                    Place(unitDetail.rectTransform,lw*.34f,rowY,lw*.63f,52);
                    Place(tierButton.transform as RectTransform,lw*.34f,rowY+56,lw*.63f,34);
                    Place(percentModeButton.transform as RectTransform,lw*.34f,rowY+96,lw*.20f,68);
                    Place(quantityRow,lw*.57f,rowY+96,lw*.40f,68);
                    Place(slider.transform as RectTransform,lw*.34f,rowY+174,lw*.63f,10);
                }
            }
            // Quantity buttons remain genuine controls within their moved container.
            float qw=quantityRow.rect.width;
            float control=compactLandscape?68:wide?36:88,controlHeight=compactLandscape?68:wide?38:88;
            Place(minus.transform as RectTransform,0,0,control,controlHeight);
            Place(plus.transform as RectTransform,qw-control,0,control,controlHeight);
            Place(selectedText.rectTransform,control+2,0,qw-2*(control+2),controlHeight);
            selectedText.fontSize=wide?20:32;
        }

        static RectTransform Rect(string name,Transform parent)
        {
            var g=new GameObject(name,typeof(RectTransform));
            g.transform.SetParent(parent,false);
            return (RectTransform)g.transform;
        }
        static RectTransform Frame(string name,RectTransform parent,bool primary)
        {
            var r=Rect(name,parent);
            r.gameObject.AddComponent<CanvasRenderer>();
            var graphic=r.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
            graphic.DecorativeCorners=true;
            graphic.Primary=primary;
            graphic.color=Color.white;
            graphic.raycastTarget=false;
            return r;
        }
        static RoyalText Text(string name,RectTransform parent,int size,Color color,bool bold=false)
        {
            var r=Rect(name,parent);
            r.gameObject.AddComponent<CanvasRenderer>();
            var t=r.gameObject.AddComponent<RoyalText>();
            t.font=RoyalGoldV41SourceArt.Serif;
            t.fontSize=size;t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;
            t.color=color;t.raycastTarget=false;
            t.alignment=TextAnchor.UpperLeft;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;
            t.verticalOverflow=VerticalWrapMode.Overflow;
            return t;
        }
        static RoyalButton Button(string name,RectTransform parent,string caption,
            Action click,bool primary,out RoyalText text)
        {
            var r=Frame(name,parent,primary);
            var graphic=r.GetComponent<RoyalGoldV2FrameGraphic>();
            graphic.raycastTarget=true;
            var button=r.gameObject.AddComponent<RoyalButton>();
            button.targetGraphic=graphic;
            text=Text("Button label",r,17,Ivory,true);
            text.Put(caption);text.alignment=TextAnchor.MiddleCenter;
            text.rectTransform.anchorMin=Vector2.zero;text.rectTransform.anchorMax=Vector2.one;
            text.rectTransform.offsetMin=new Vector2(6,3);text.rectTransform.offsetMax=new Vector2(-6,-3);
            button.onClick.AddListener(()=>click());
            return button;
        }
        static void Place(RoyalText label,float x,float y,float w,float h)
        {
            Place(label.rectTransform,x,y,w,h);
        }
        static void Place(RectTransform r,float x,float y,float w,float h)
        {
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);
            r.anchoredPosition=new Vector2(x,-y);
            r.sizeDelta=new Vector2(Mathf.Max(1,w),Mathf.Max(1,h));
        }
    }
}
