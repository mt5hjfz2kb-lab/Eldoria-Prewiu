using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Eldoria.ReferenceClean;
using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Eldoria.BreachWatch
{
    // New, independent presentation object. The canonical game/session is reused,
    // but none of the old HUD's UI hierarchy, graphic assets or layout is reused.
    // Opt-in native proof only; it is NOT installed in the production scene/build.
    [DisallowMultipleComponent]
    public sealed class BreachWatchHud : MonoBehaviour
    {
        const string RootName = "VIGILIA / independent canvas";
        static readonly Color TextIvory = new Color(.98f,.94f,.84f,1f);
        static readonly Color Muted = new Color(.83f,.81f,.76f,1f);
        static readonly Color Copper = new Color(.92f,.70f,.40f,1f);
        static readonly Color Locked = new Color(.38f,.39f,.39f,1f);
        ValoriaIntegratedWorld host;
        Canvas canvas;
        CanvasScaler scaler;
        GraphicRaycaster raycaster;
        Canvas legacyCanvas;
        GraphicRaycaster legacyRaycaster;
        CanvasGroup legacyGroup;
        bool originalCanvas, originalRaycaster, originalBlocks, originalInteractable;
        Func<Vector2,bool> priorPointerBlocker;
        readonly List<RaycastResult> pointerHits = new List<RaycastResult>();
        readonly Dictionary<Camera,int> originalCameraMasks = new Dictionary<Camera,int>();
        float originalAlpha;
        Font font, boldFont;
        RectTransform uiRoot;
        RectTransform identity, identityCrest, resourceRow, objective, panel, portraitFrame, buttonRow, utilityRail, centerMark;
        RectTransform[] resourceChips = new RectTransform[3];
        Text identityName, identityPower, chapterName, chapterObjective, chapterProgress;
        Text[] resourceAmounts = new Text[3];
        readonly Image[] resourceIllustrations = new Image[3];
        Text buildingName, buildingLevel, buildingFunction, buildingCosts, buildingState, actionLabel, feedback, costHeader;
        readonly Image[] costIcons = new Image[2];
        readonly Text[] costQuantities = new Text[2];
        Button actionButton, closeButton;
        RawImage portraitView;
        BreachWatchBuildingPortrait buildingPortrait;
        BreachWatchPlate actionPlate;
        BreachWatchNavFiligree navFiligree;
        readonly Button[] navButtons = new Button[5];
        readonly BreachWatchPlate[] navPlates = new BreachWatchPlate[5];
        static readonly string[] ReferenceNavArt = {
            "castle", "provisions", "swords", "raven", "helm"
        };
        string selected="";
        int selectedTab;
        bool fullObjective;
        bool lastPortrait;
        int lastWidth=-1,lastHeight=-1;
        float nextRefresh;
        bool created;
        RoyalSnapshot snapshot;
        readonly string[] navLabels = {"REINO", "RECURSOS", "EJÉRCITO", "EXPLORAR", "HÉROES"};
        readonly BreachWatchGlyph.Sigil[] navIcons = {
            BreachWatchGlyph.Sigil.Castle,BreachWatchGlyph.Sigil.Build,
            BreachWatchGlyph.Sigil.Army,BreachWatchGlyph.Sigil.World,BreachWatchGlyph.Sigil.Chronicle
        };

        public bool IndependentCanvas => canvas && legacyCanvas && canvas!=legacyCanvas
            && canvas.transform.parent != legacyCanvas.transform;
        public bool LegacyPresentationHidden => legacyCanvas && !legacyCanvas.enabled
            && legacyGroup && legacyGroup.alpha<.001f;
        public string SelectedBuildingId => selected;
        public RoyalSnapshot CurrentSnapshot => snapshot;
        public bool HasRealSession => host && host.Read(selected,host.English)!=null;
        public Canvas NativeCanvas => canvas;
        public Button BuildingAction => actionButton;
        public Button BuildingClose => closeButton;
        public RectTransform BuildingPanel => panel;
        public bool HasLiveBuildingPortrait => buildingPortrait && buildingPortrait.HasLiveBuilding;
        public bool BuildingPortraitExclusive => buildingPortrait && buildingPortrait.RenderedFromIsolatedRealMeshes;
        public float BuildingPortraitDrawnFraction => buildingPortrait ? buildingPortrait.NonBackgroundPixelFraction : 0f;
        public bool HasNativePremiumTypography =>
            font && boldFont &&
            font==Resources.Load<Font>("VigiliaArt/Fonts/CormorantGaramond-Regular") &&
            boldFont==Resources.Load<Font>("VigiliaArt/Fonts/CormorantGaramond-SemiBold");
        public bool HasTrueResourceIllustrations =>
            resourceIllustrations[0] && resourceIllustrations[0].sprite &&
            resourceIllustrations[0].sprite.name=="wood" &&
            resourceIllustrations[1] && resourceIllustrations[1].sprite &&
            resourceIllustrations[1].sprite.name=="stone" &&
            resourceIllustrations[2] && resourceIllustrations[2].sprite &&
            resourceIllustrations[2].sprite.name=="food";
        public bool UsesCompactRealmCrest =>
            identityCrest && identityCrest.GetComponent<BreachWatchPlate>() &&
            identityCrest.GetComponent<BreachWatchPlate>().sprite &&
            identityCrest.GetComponent<BreachWatchPlate>().sprite.name=="castle";
        public bool UsesOriginalPaintedMedallions =>
            navPlates[0] && navPlates[0].sprite &&
            navPlates[0].sprite.name=="castle";
        public bool UsesPremiumSculptedMedallions
        {
            get
            {
                for(int i=0;i<navPlates.Length;i++)
                {
                    var requested=Resources.Load<Sprite>("VigiliaArt/PremiumMedallions/"+ReferenceNavArt[i]);
                    if(!requested || !navPlates[i] || navPlates[i].sprite!=requested)
                        return false;
                }
                var crest=Resources.Load<Sprite>("VigiliaArt/PremiumMedallions/castle");
                return crest && identityCrest &&
                    identityCrest.GetComponent<BreachWatchPlate>().sprite==crest;
            }
        }
        public bool UsesApprovedReferenceMedallions
        {
            get
            {
                for(int i=0;i<navPlates.Length;i++)
                    if(!navPlates[i] || !navPlates[i].UsesOwnerReference ||
                       !navPlates[i].sprite ||
                       navPlates[i].sprite.name!=ReferenceNavArt[i])
                        return false;
                return true;
            }
        }
        public bool UsesOriginalForgedPortraitFrame =>
            portraitFrame && portraitFrame.GetComponent<BreachWatchPlate>() &&
            portraitFrame.GetComponent<BreachWatchPlate>().sprite &&
            portraitFrame.GetComponent<BreachWatchPlate>().sprite.name=="portrait_forged_iron";
        public bool UsesOwnerApprovedCarvedContext
        {
            get
            {
                var surface=panel?panel.GetComponent<BreachWatchPlate>():null;
                return surface && surface.UsesOwnerReference && surface.sprite &&
                    surface.sprite.name=="carved_context" &&
                    surface.type==Image.Type.Sliced;
            }
        }
        public IReadOnlyList<Button> NavButtons => navButtons;

        public static BreachWatchHud Install(ValoriaIntegratedWorld source)
        {
            if(!source || !source.Ready || !source.Hud)throw new InvalidOperationException("BreachWatch needs a ready canonical Valoria session");
            var existing=FindFirstObjectByType<BreachWatchHud>();
            if(existing) return existing;
            var obj=new GameObject(RootName,typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            obj.layer=5; // Unity's UI layer, included in the real camera's capture mask.
            var hud=obj.AddComponent<BreachWatchHud>();
            hud.Initialize(source);
            return hud;
        }

        void Initialize(ValoriaIntegratedWorld source)
        {
            host=source;
            canvas=GetComponent<Canvas>();scaler=GetComponent<CanvasScaler>();raycaster=GetComponent<GraphicRaycaster>();
            canvas.renderMode=RenderMode.ScreenSpaceCamera;
            canvas.planeDistance=1f;canvas.overrideSorting=true;canvas.sortingOrder=5000;
            SyncLiveCamera();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;scaler.scaleFactor=1f;
            raycaster.enabled=true;
            // The old RoyalInputGate detects only descendants of RoyalHUD.
            // A new standalone Canvas must block world gestures itself.
            if(host.City && host.City.Presenter)
            {
                priorPointerBlocker=host.City.Presenter.ExternalPointerBlocked;
                host.City.Presenter.ExternalPointerBlocked=BlocksNativePointer;
            }
            // Old presentation is explicitly switched OFF. It is not a background
            // component or a graphics donor; its live bridge remains available
            // for world selection and gameplay while this proof is active.
            legacyCanvas=host.Hud.GetComponent<Canvas>();
            legacyRaycaster=host.Hud.GetComponent<GraphicRaycaster>();
            legacyGroup=host.Hud.GetComponent<CanvasGroup>();
            host.Hud.SelectionChanged+=OnCanonicalSelection;
            originalCanvas=legacyCanvas.enabled;
            originalRaycaster=legacyRaycaster && legacyRaycaster.enabled;
            originalAlpha=legacyGroup?legacyGroup.alpha:1;
            originalBlocks=legacyGroup && legacyGroup.blocksRaycasts;
            originalInteractable=legacyGroup && legacyGroup.interactable;
            legacyCanvas.enabled=false;
            if(legacyRaycaster)legacyRaycaster.enabled=false;
            if(legacyGroup){legacyGroup.alpha=0;legacyGroup.blocksRaycasts=false;legacyGroup.interactable=false;}
            // Original, redistributable OFL type belongs to Vigilia alone.
            // Avoid device-dependent Georgia/Arial substitutions on Android.
            font=Resources.Load<Font>("VigiliaArt/Fonts/CormorantGaramond-Regular");
            boldFont=Resources.Load<Font>("VigiliaArt/Fonts/CormorantGaramond-SemiBold");
            if(!font)font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if(!font)font=Font.CreateDynamicFontFromOSFont(new[]{"Georgia","Arial"},20);
            uiRoot=Rect("Safe composition",transform);
            Stretch(uiRoot);
            BuildIndependentComponents();
            created=true;Reflow(Screen.width,Screen.height);RenderNow();
        }

        static RectTransform Rect(string name,Transform parent)
        {
            // Native Unity GraphicRaycaster requires an actual CanvasRenderer for every Graphic.
            // Do not rely on the UI component being added after RectTransform.
            var obj=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer));obj.layer=5;obj.transform.SetParent(parent,false);
            return (RectTransform)obj.transform;
        }
        static void Stretch(RectTransform rect)
        {
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.pivot=new Vector2(.5f,.5f);
            rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
        }
        static void At(RectTransform rt,float x,float top,float width,float height)
        {
            rt.anchorMin=rt.anchorMax=new Vector2(0,1);
            rt.pivot=new Vector2(0,1);
            rt.anchoredPosition=new Vector2(x,-top);
            rt.sizeDelta=new Vector2(Mathf.Max(1,width),Mathf.Max(1,height));
        }
        static void Fill(RectTransform rt,float margin=0)
        {
            Stretch(rt);rt.offsetMin=new Vector2(margin,margin);rt.offsetMax=new Vector2(-margin,-margin);
        }
        BreachWatchPlate Plate(RectTransform rect,BreachWatchPlate.PlateType type)
        {
            var p=rect.gameObject.AddComponent<BreachWatchPlate>();p.Type=type;p.raycastTarget=false;return p;
        }
        void AddBevel(RectTransform parent,bool accent=false)
        {
            var carved=Rect("Independent forged copper surface trim",parent);
            Fill(carved);
            var trim=carved.gameObject.AddComponent<BreachWatchBevelTrim>();
            trim.Highlight=accent;
            trim.raycastTarget=false;
        }
        BreachWatchGlyph Glyph(Transform parent,string name,BreachWatchGlyph.Sigil symbol)
        {
            var rect=Rect(name,parent);var g=rect.gameObject.AddComponent<BreachWatchGlyph>();
            g.Type=symbol;g.Ink=TextIvory;g.raycastTarget=false;return g;
        }
        Text Label(Transform parent,string name,int size,TextAnchor align,Color tint,bool bold=false)
        {
            var rect=Rect(name,parent);
            var text=rect.gameObject.AddComponent<Text>();
            text.font=bold && boldFont ? boldFont : font;
            text.fontSize=size;
            text.alignment=align;
            text.color=tint;
            text.fontStyle=bold && !boldFont ? FontStyle.Bold : FontStyle.Normal;
            text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;
            text.supportRichText=false;text.raycastTarget=false;
            var shadow=rect.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(0,.01f,.01f,.65f);
            shadow.effectDistance=new Vector2(0,-1);
            return text;
        }
        Button Button(RectTransform rect,Action click)
        {
            var graphic=rect.GetComponent<BreachWatchPlate>();
            if(graphic)graphic.raycastTarget=true; // actual graphic participates in EventSystem raycasting
            var b=rect.gameObject.AddComponent<Button>();
            b.transition=Selectable.Transition.ColorTint;
            var tint=b.colors;
            tint.normalColor=Color.white;
            tint.highlightedColor=new Color(1f,.94f,.85f,1f);
            tint.pressedColor=new Color(.73f,.69f,.65f,1f);
            tint.disabledColor=new Color(.84f,.79f,.71f,1f);
            tint.fadeDuration=.08f;
            b.colors=tint;
            if(graphic)b.targetGraphic=graphic;
            b.onClick.AddListener(()=>click());return b;
        }
        RectTransform BuildButton(Transform parent,string name,BreachWatchGlyph.Sigil icon,Action action,
            BreachWatchPlate.PlateType kind=BreachWatchPlate.PlateType.Basalt)
        {
            var rt=Rect(name,parent);var hitSurface=Plate(rt,kind);
            // A visible Graphic must accept the pointer for Unity's GraphicRaycaster.
            // The parent Button then receives the normal pointerClick event.
            hitSurface.raycastTarget=true;
            var b=Button(rt,action);
            b.targetGraphic=hitSurface;
            var glyph=Glyph(rt,"Carved sigil",icon);
            Fill((RectTransform)glyph.transform,10);
            return rt;
        }
        void BuildIndependentComponents()
        {
            // A truly independent world-safe material wash, only 26% dark at
            // extreme edges and nearly transparent over the playable center.
            var atmosphere=Rect("Soft volcanic atmosphere / no world replacement",uiRoot);
            Fill(atmosphere);
            var veil=atmosphere.gameObject.AddComponent<BreachWatchAtmosphere>();
            veil.raycastTarget=false;
            // Asymmetrical stone architecture instead of a header/footer re-skin.
            identity=Rect("Identity / basalt crest",uiRoot);
            Plate(identity,BreachWatchPlate.PlateType.Basalt);
            AddBevel(identity,true);
            // The owner-approved compact reference has NO red top banner.
            // Independent round carved crest replaces the obsolete banner.
            identityCrest=Rect("Independent circular realm crest",identity);
            Plate(identityCrest,BreachWatchPlate.PlateType.Navigation);
            var crestImage=Resources.Load<Sprite>("VigiliaArt/PremiumMedallions/castle");
            if(!crestImage)crestImage=Resources.Load<Sprite>("VigiliaArt/OwnerReference/castle");
            if(crestImage)identityCrest.GetComponent<BreachWatchPlate>()
                .ApplyIndependentArt(crestImage);
            At(identityCrest,-3,-4,62,62);
            identityName=Label(identity,"Player name",22,TextAnchor.MiddleLeft,TextIvory,true);
            identityPower=Label(identity,"Realm power",16,TextAnchor.MiddleLeft,Muted);
            resourceRow=Rect("Separate resource stones",uiRoot);
            for(int i=0;i<3;i++)
            {
                resourceChips[i]=Rect("Resource "+i,resourceRow);
                Plate(resourceChips[i],BreachWatchPlate.PlateType.Resource);
                AddBevel(resourceChips[i]);
                var picture=Rect("True illustrated resource",resourceChips[i]);
                resourceIllustrations[i]=picture.gameObject.AddComponent<Image>();
                resourceIllustrations[i].sprite=Resources.Load<Sprite>(
                    "VigiliaArt/ResourceIcons/"+(i==0?"wood":i==1?"stone":"food"));
                resourceIllustrations[i].type=Image.Type.Simple;
                resourceIllustrations[i].preserveAspect=true;
                resourceIllustrations[i].raycastTarget=false;
                At(picture,7,5,36,35);
                resourceAmounts[i]=Label(resourceChips[i],"Actual quantity",17,TextAnchor.MiddleLeft,TextIvory,true);
                At(resourceAmounts[i].rectTransform,47,5,90,35);
            }
            centerMark=Rect("Breach witness sigil",uiRoot);Plate(centerMark,BreachWatchPlate.PlateType.Obsidian);
            var central=Glyph(centerMark,"Stone fracture",BreachWatchGlyph.Sigil.Breach);
            Fill((RectTransform)central.transform,4);central.Ink=Copper;
            objective=Rect("Chapter side tablet",uiRoot);
            Plate(objective,BreachWatchPlate.PlateType.Basalt);
            AddBevel(objective,true);
            var objectiveSeal=Glyph(objective,"Chapter stone rune",BreachWatchGlyph.Sigil.Breach);
            objectiveSeal.Ink=Copper;
            At((RectTransform)objectiveSeal.transform,12,7,24,24);
            chapterName=Label(objective,"Actual chapter",15,TextAnchor.MiddleLeft,Copper,true);
            chapterObjective=Label(objective,"Actual objective",17,TextAnchor.UpperLeft,TextIvory);
            chapterProgress=Label(objective,"Objective progress",14,TextAnchor.MiddleLeft,Muted);
            var questPlate=objective.GetComponent<BreachWatchPlate>();questPlate.raycastTarget=true;
            var questHit=Button(objective,()=>{fullObjective=!fullObjective;Reflow(lastWidth,lastHeight);});
            questHit.targetGraphic=questPlate;
            questHit.interactable=true;
            utilityRail=Rect("Freestanding secondary seals",uiRoot);
            // No fake buttons: these destinations are not part of the first
            // proof. Keep their artistic seals, but make disabled state explicit.
            foreach(var design in new [] {
                (name:"Notices",icon:BreachWatchGlyph.Sigil.Bell),
                (name:"Settings",icon:BreachWatchGlyph.Sigil.Settings),
                (name:"Fissure",icon:BreachWatchGlyph.Sigil.Breach) })
            {
                var seal=BuildButton(utilityRail,design.name,design.icon,()=>{});
                seal.GetComponent<Button>().interactable=false;
                var glyph=seal.GetComponentInChildren<BreachWatchGlyph>();
                if(glyph){glyph.Ink=Locked;glyph.SetVerticesDirty();}
            }
            panel=Rect("Independent selected-building cartouche",uiRoot);
            var basalt=Plate(panel,BreachWatchPlate.PlateType.Basalt);
            var approvedFrame=Resources.Load<Sprite>("VigiliaArt/OwnerReference/carved_context");
            if(approvedFrame)basalt.ApplyIndependentArt(approvedFrame,true);
            // The original artwork contains an irregular see-through center.
            // Make live text entirely readable on a separate dark mineral
            // surface, while preserving the approved carved outer border.
            var opaqueWell=Rect("Dense original slate ink / readable live data",panel);
            Fill(opaqueWell,12);
            var ink=opaqueWell.gameObject.AddComponent<BreachWatchPanelInk>();
            ink.raycastTarget=false;
            // Three-dimensional, resolution-independent fine gold metalwork
            // is a separate original Graphic, never a skin over RoyalHUD.
            var engravingRect=Rect("Original gilded Breach engraving",panel);
            Fill(engravingRect);
            var engraving=engravingRect.gameObject.AddComponent<BreachWatchFrameEngraving>();
            engraving.raycastTarget=false;
            portraitFrame=Rect("Carved recess / live building illustration",panel);
            Plate(portraitFrame,BreachWatchPlate.PlateType.Portrait);
            var liveView=Rect("Live 3D building portrait / no old HUD sprites",portraitFrame);
            Fill(liveView,8);
            portraitView=liveView.gameObject.AddComponent<RawImage>();
            portraitView.color=Color.white;
            portraitView.raycastTarget=false;
            buildingPortrait=gameObject.AddComponent<BreachWatchBuildingPortrait>();
            buildingPortrait.Initialize(portraitView);
            var buildingSeal=Glyph(panel,"Building crest",BreachWatchGlyph.Sigil.Castle);
            At((RectTransform)buildingSeal.transform,14,14,36,36);
            buildingName=Label(panel,"Real building name",22,TextAnchor.MiddleLeft,TextIvory,true);
            buildingLevel=Label(panel,"Real building level",15,TextAnchor.MiddleLeft,Copper,true);
            buildingFunction=Label(panel,"Real effects",17,TextAnchor.UpperLeft,TextIvory);
            costHeader=Label(panel,"Compact engraved cost heading",14,
                TextAnchor.MiddleCenter,Copper,true);
            costHeader.text="COSTE DE LA MEJORA";
            buildingCosts=Label(panel,"Real costs",15,TextAnchor.UpperLeft,TextIvory);
            for(int i=0;i<2;i++)
            {
                var icon=Rect(i==0?"Real wood icon":"Real stone icon",panel);
                var image=icon.gameObject.AddComponent<Image>();
                image.sprite=Resources.Load<Sprite>("VigiliaArt/ResourceIcons/"+(i==0?"wood":"stone"));
                image.type=Image.Type.Simple;
                image.preserveAspect=true;
                image.raycastTarget=false;
                costIcons[i]=image;
                costQuantities[i]=Label(panel,i==0?"Actual wood cost":"Actual stone cost",16,
                    TextAnchor.MiddleLeft,TextIvory,true);
            }
            buildingState=Label(panel,"Real state and timer",15,TextAnchor.UpperLeft,Muted);
            var closeRect=BuildButton(panel,"Close selected building",BreachWatchGlyph.Sigil.Cross,()=>SelectBuilding(""));
            closeButton=closeRect.GetComponent<Button>();
            var actionRect=Rect("Original gameplay action / new presentation",panel);
            actionPlate=Plate(actionRect,BreachWatchPlate.PlateType.Action);
            actionButton=Button(actionRect,SubmitSelectedBuilding);
            var actionPalette=actionButton.colors;
            actionPalette.disabledColor=new Color(.83f,.77f,.69f,1f);
            actionButton.colors=actionPalette;
            actionPlate.raycastTarget=true;
            actionButton.targetGraphic=actionPlate;
            actionLabel=Label(actionButton.transform,"Real action title",17,TextAnchor.MiddleCenter,TextIvory,true);
            Fill(actionLabel.rectTransform,6);
            buttonRow=Rect("Five standalone stone navigation emblems",uiRoot);
            var navLine=Rect("Subtle sculpted copper linkwork / no bottom HUD bar",buttonRow);
            Fill(navLine);
            navFiligree=navLine.gameObject.AddComponent<BreachWatchNavFiligree>();
            navFiligree.raycastTarget=false;
            for(int i=0;i<5;i++)
            {
                int index=i;var rt=BuildButton(buttonRow,navLabels[i],navIcons[i],()=>Navigate(index),BreachWatchPlate.PlateType.Navigation);
                navButtons[i]=rt.GetComponent<Button>();navPlates[i]=rt.GetComponent<BreachWatchPlate>();
                navPlates[i].Type=i==0?BreachWatchPlate.PlateType.Selected:BreachWatchPlate.PlateType.Navigation;
                var approved=Resources.Load<Sprite>("VigiliaArt/PremiumMedallions/"+ReferenceNavArt[i]);
                if(!approved)approved=Resources.Load<Sprite>("VigiliaArt/OwnerReference/"+ReferenceNavArt[i]);
                if(approved)
                {
                    navPlates[i].ApplyIndependentArt(approved);
                    // The reference image includes its own relief icon. Keep
                    // the separate raycast and button, but avoid double icons.
                    var oldPlaceholder=rt.GetComponentInChildren<BreachWatchGlyph>();
                    if(oldPlaceholder)oldPlaceholder.enabled=false;
                }
            }
            feedback=Label(uiRoot,"Action feedback / live or disabled",15,TextAnchor.MiddleCenter,Copper);
            feedback.text="";
        }
        void SetFeedback(string value){if(feedback)feedback.text=value;}
        void OnCanonicalSelection(string id)
        {
            selected=id??"";
            if(host && host.InWorld)selectedTab=3;
            else if(selected=="sawmill" || selected=="barracks" ||
                    selected=="upgrade-barracks" || selected=="granary" ||
                    selected=="stoneworks")selectedTab=1;
            else if(selected=="bastion")selectedTab=0;
            if(created)RenderNow();
        }

        // The actual scene mode changes which Unity camera is enabled.
        // Both cameras must render our NEW UI layer, including on real devices,
        // not only when a native capture temporarily modifies cullingMask.
        void SyncLiveCamera()
        {
            if(!host||!canvas)return;
            var active=host.PlayerCamera;
            if(!active)return;
            if(!originalCameraMasks.ContainsKey(active))
                originalCameraMasks.Add(active,active.cullingMask);
            active.cullingMask|=1<<5;
            if(canvas.worldCamera!=active)canvas.worldCamera=active;
        }

        public void SelectBuilding(string id)
        {
            if(!host)return;
            selected=id??"";
            // Reuse ONLY the existing real selection command and route, not UI.
            host.Hud.SelectBuilding(selected);
            RenderNow();
        }
        void SubmitSelectedBuilding()
        {
            if(!host||snapshot?.Building?.Action==null||!snapshot.Building.Action.Enabled)return;
            var action=snapshot.Building.Action;
            var result=host.Submit(action);SetFeedback(result.Message);
            RenderNow();
        }
        void Navigate(int index)
        {
            if(!host)return;
            switch(index)
            {
                case 0:
                    // City/Realm is a destination, not a tab label on the world.
                    // Return the canonical city camera before selecting Bastion.
                    if(host.InWorld && !host.TrySetWorld(false))return;
                    selectedTab=0;
                    SelectBuilding("bastion");
                    SyncLiveCamera();
                    break;
                case 1:
                    // Provisions belongs to the REAL city's resource building.
                    // Never show an inaccessible city card above the world.
                    if(host.InWorld && !host.TrySetWorld(false))return;
                    selectedTab=1;
                    SelectBuilding("sawmill");
                    SyncLiveCamera();
                    break;
                case 3:
                    // Explore is idempotent. A second tap cannot accidentally
                    // return the player to the city; Reino handles that.
                    if(host.CanOpen(RoyalSection.World) && host.TrySetWorld(true))
                    {
                        selectedTab=3;
                        SelectBuilding("");
                        SyncLiveCamera();
                    }
                    break;
                // Ejército and Héroes deliberately stay disabled until their
                // future independent screens exist; chapter progress is opened
                // using the actual objective tablet, not a falsely named route.
            }
        }
        void LateUpdate()
        {
            if(!created||!host)return;
            SyncLiveCamera();
            if(lastWidth!=Screen.width||lastHeight!=Screen.height)Reflow(Screen.width,Screen.height);
            if(Time.unscaledTime<nextRefresh)return;
            nextRefresh=Time.unscaledTime+.25f;RenderNow();
        }
        // Exact owner-approved compact composition, designed independently
        // of RoyalHUD and optimised for preserving visible playable world.
        // All geometry is uGUI: never paste a generated full-screen bitmap.
        public void Reflow(int pixelWidth,int pixelHeight)
        {
            if(!canvas || !uiRoot)return;
            pixelWidth=Mathf.Max(320,pixelWidth);
            pixelHeight=Mathf.Max(300,pixelHeight);
            lastWidth=pixelWidth;
            lastHeight=pixelHeight;

            bool physicalDevice=(Screen.width==pixelWidth && Screen.height==pixelHeight);
            if(physicalDevice)
            {
                Rect safe=Screen.safeArea;
                if(safe.width>0 && safe.height>0 &&
                   safe.width<=Screen.width && safe.height<=Screen.height)
                {
                    uiRoot.anchorMin=new Vector2(safe.xMin/Screen.width,safe.yMin/Screen.height);
                    uiRoot.anchorMax=new Vector2(safe.xMax/Screen.width,safe.yMax/Screen.height);
                    uiRoot.offsetMin=Vector2.zero;
                    uiRoot.offsetMax=Vector2.zero;
                    pixelWidth=Mathf.Max(320,Mathf.RoundToInt(safe.width));
                    pixelHeight=Mathf.Max(300,Mathf.RoundToInt(safe.height));
                }
            }
            else Stretch(uiRoot);

            bool portrait=pixelHeight>pixelWidth;
            float scale=portrait
                ?Mathf.Clamp(pixelWidth/430f,.86f,1.75f)
                :Mathf.Clamp(Mathf.Min(pixelWidth/1250f,pixelHeight/680f),.86f,1.30f);
            scaler.scaleFactor=scale;
            float w=pixelWidth/scale,h=pixelHeight/scale;
            float margin=portrait?13f:18f;

            // The owner specifically rejected the thick burgundy header and
            // the second huge title plate. A single small round realm crest,
            // compact counters and a free-standing Breach mark remain.
            if(portrait)
            {
                At(identity,margin,11,182,55);
                At(identityCrest,-4,-4,62,62);
                At(identityName.rectTransform,56,7,121,27);
                At(identityPower.rectTransform,56,31,121,18);
                identityName.fontSize=16;
                identityPower.fontSize=14;

                float rightSpace=w-(margin+182+9+margin);
                float chip=Mathf.Max(58,(rightSpace-8f)/3f);
                float resourcesWidth=3*chip+8f;
                At(resourceRow,w-margin-resourcesWidth,18,resourcesWidth,39);
                for(int i=0;i<3;i++)
                {
                    At(resourceChips[i],i*(chip+4f),0,chip,38);
                    var image=resourceIllustrations[i].rectTransform;
                    At(image,4,7,25,25);
                    At(resourceAmounts[i].rectTransform,30,7,chip-31,26);
                    resourceAmounts[i].fontSize=14;
                }

                At(centerMark,w*.52f-17,84,34,34);
                At(objective,margin,125,Mathf.Min(250,w*.62f),fullObjective?118:89);
                At(chapterName.rectTransform,37,6,Mathf.Min(205,w*.52f),21);
                At(chapterObjective.rectTransform,14,30,Mathf.Min(221,w*.53f),fullObjective?67:37);
                At(chapterProgress.rectTransform,14,fullObjective?101:69,Mathf.Min(221,w*.53f),15);
                chapterName.fontSize=13;
                chapterObjective.fontSize=16;
                chapterProgress.fontSize=13;

                At(utilityRail,margin,h-338,54,160);
                for(int i=0;i<utilityRail.childCount;i++)
                {
                    var utility=(RectTransform)utilityRail.GetChild(i);
                    utility.gameObject.SetActive(i<3);
                    At(utility,0,i*53f,49,49);
                }

                float navSize=56,navGap=11;
                float navWidth=5*navSize+4*navGap;
                float navTop=h-100f;
                At(buttonRow,(w-navWidth)*.5f,navTop,navWidth,64);
                navFiligree.Configure(navSize+navGap,navSize);
                for(int i=0;i<5;i++)
                    At((RectTransform)navButtons[i].transform,
                        i*(navSize+navGap),3,navSize,navSize);

                float pw=Mathf.Min(210,w*.48f);
                float ph=294f;
                float left=w-margin-pw;
                float top=navTop-ph-45;
                At(panel,left,top,pw,ph);
                At(buildingName.rectTransform,58,7,pw-115,29);
                At(buildingLevel.rectTransform,58,35,pw-115,19);
                At(((RectTransform)closeButton.transform),pw-57,3,52,52);
                At(portraitFrame,11,59,pw-22,81);
                At(buildingFunction.rectTransform,13,143,pw-26,43);
                At(costHeader.rectTransform,13,188,pw-26,14);
                At(buildingCosts.rectTransform,13,204,pw-26,24);
                At(costIcons[0].rectTransform,15,204,24,24);
                At(costQuantities[0].rectTransform,43,204,55,24);
                At(costIcons[1].rectTransform,105,204,24,24);
                At(costQuantities[1].rectTransform,133,204,pw-144,24);
                At(buildingState.rectTransform,13,230,pw-26,14);
                At(((RectTransform)actionButton.transform),12,ph-50,pw-24,49);
                buildingName.fontSize=20;
                buildingLevel.fontSize=14;
                buildingFunction.fontSize=14;
                buildingCosts.fontSize=14;
                buildingState.fontSize=13;
                actionLabel.fontSize=14;

                At(feedback.rectTransform,margin,h-122,Mathf.Max(90,left-margin-15),34);
                feedback.fontSize=13;
            }
            else
            {
                At(identity,margin,11,234,58);
                At(identityCrest,-3,-4,64,64);
                At(identityName.rectTransform,59,6,166,31);
                At(identityPower.rectTransform,59,35,165,18);
                identityName.fontSize=20;
                identityPower.fontSize=14;

                float chip=Mathf.Min(100,Mathf.Max(82f,(w*.28f-12f)/3f));
                float resourcesWidth=3*chip+14;
                At(resourceRow,w-margin-resourcesWidth,16,resourcesWidth,43);
                for(int i=0;i<3;i++)
                {
                    At(resourceChips[i],i*(chip+7),0,chip,41);
                    At(resourceIllustrations[i].rectTransform,5,5,30,30);
                    At(resourceAmounts[i].rectTransform,38,5,chip-41,30);
                    resourceAmounts[i].fontSize=16;
                }
                At(centerMark,w*.49f-17,11,34,34);
                At(objective,margin,102,225,fullObjective?110:83);
                At(chapterName.rectTransform,37,8,178,20);
                At(chapterObjective.rectTransform,14,32,197,fullObjective?60:31);
                At(chapterProgress.rectTransform,14,fullObjective?90:65,190,14);
                chapterName.fontSize=14;
                chapterObjective.fontSize=16;
                chapterProgress.fontSize=14;

                At(utilityRail,margin,h-179,49,162);
                for(int i=0;i<utilityRail.childCount;i++)
                {
                    var utility=(RectTransform)utilityRail.GetChild(i);
                    utility.gameObject.SetActive(i<3);
                    At(utility,0,i*48,43,43);
                }

                float navSize=55,navGap=15;
                float navWidth=5*navSize+4*navGap;
                float navTop=h-78;
                At(buttonRow,(w-navWidth)*.5f,navTop,navWidth,69);
                navFiligree.Configure(navSize+navGap,navSize);
                for(int i=0;i<5;i++)
                    At((RectTransform)navButtons[i].transform,
                        i*(navSize+navGap),3,navSize,navSize);

                float pw=h<530f?210f:220f;
                float ph=Mathf.Min(365,h-174f);
                float top=h<530f?75f:118f;
                At(panel,w-margin-pw,top,pw,ph);
                At(buildingName.rectTransform,58,8,pw-115,31);
                At(buildingLevel.rectTransform,58,37,pw-115,19);
                At(((RectTransform)closeButton.transform),pw-57,3,52,52);
                buildingName.fontSize=21;
                buildingLevel.fontSize=15;
                if(ph<305)
                {
                    // Tight landscape phones: separate image and text columns
                    // followed by an independent full-width action row.
                    At(portraitFrame,12,60,95,83);
                    At(buildingFunction.rectTransform,114,62,pw-124,76);
                    At(costHeader.rectTransform,12,142,pw-24,15);
                    At(buildingCosts.rectTransform,12,163,pw-24,25);
                    At(costIcons[0].rectTransform,14,162,26,26);
                    At(costQuantities[0].rectTransform,43,162,52,26);
                    At(costIcons[1].rectTransform,112,162,26,26);
                    At(costQuantities[1].rectTransform,140,162,pw-151,26);
                    At(buildingState.rectTransform,12,196,pw-24,18);
                    At(((RectTransform)actionButton.transform),12,ph-56,pw-24,53);
                    buildingFunction.fontSize=14;
                    buildingCosts.fontSize=14;
                    buildingState.fontSize=14;
                    actionLabel.fontSize=15;
                }
                else
                {
                    At(portraitFrame,12,61,pw-24,99);
                    At(buildingFunction.rectTransform,13,163,pw-26,46);
                    At(costHeader.rectTransform,13,211,pw-26,16);
                    At(buildingCosts.rectTransform,13,231,pw-26,26);
                    At(costIcons[0].rectTransform,17,228,30,30);
                    At(costQuantities[0].rectTransform,52,229,53,29);
                    At(costIcons[1].rectTransform,121,228,30,30);
                    At(costQuantities[1].rectTransform,153,229,pw-162,29);
                    At(buildingState.rectTransform,13,262,pw-26,18);
                    At(((RectTransform)actionButton.transform),12,ph-55,pw-24,49);
                    buildingFunction.fontSize=15;
                    buildingCosts.fontSize=15;
                    buildingState.fontSize=14;
                    actionLabel.fontSize=16;
                }
                At(feedback.rectTransform,margin,h-121,
                    Mathf.Max(120,w-pw-3*margin),30);
                feedback.fontSize=14;
            }

            // Render only the original selected building in its native
            // illustration well. Reproject the physical camera texture without
            // stretching stone, gold trim or actual building geometry.
            if(portraitView && portraitFrame)
            {
                float ratio=Mathf.Max(.1f,(portraitFrame.rect.width-16f)/
                    Mathf.Max(1f,portraitFrame.rect.height-16f));
                if(ratio<=2f)
                {
                    float uvW=ratio*.5f;
                    portraitView.uvRect=new Rect((1f-uvW)*.5f,0,uvW,1);
                }
                else
                {
                    float uvH=2f/ratio;
                    portraitView.uvRect=new Rect(0,(1f-uvH)*.5f,1,uvH);
                }
            }
            lastPortrait=portrait;
            Canvas.ForceUpdateCanvases();
        }
        public void RenderNow()
        {
            if(!created||!host)return;
            try
            {
                snapshot=host.Read(selected,host.English);
                var realName=host.City.Gateway.Snapshot().PlayerName;
                identityName.text=string.IsNullOrWhiteSpace(realName)?"VALORIA":realName;
                identityPower.text="PODER  "+RoyalFormatting.Amount(snapshot.Power);
                chapterName.text=snapshot.Chapter+"    /    LA BRECHA";
                chapterObjective.text=snapshot.Objective;
                chapterProgress.text=snapshot.ObjectiveProgress;
                resourceAmounts[0].text=RoyalFormatting.Amount(snapshot.Wood);
                resourceAmounts[1].text=RoyalFormatting.Amount(snapshot.Stone);
                resourceAmounts[2].text=snapshot.ShowFood?RoyalFormatting.Amount(snapshot.Food):"—";
                if(resourceIllustrations[2])
                    resourceIllustrations[2].color=snapshot.ShowFood
                        ?Color.white:new Color(.5f,.48f,.45f,.70f);
                var b=snapshot.Building;
                bool show=b!=null&&!string.IsNullOrEmpty(selected);
                panel.gameObject.SetActive(show);
                if(show)
                {
                    buildingName.text=b.Name;
                    buildingLevel.text="NIVEL "+b.Level;
                    if(buildingPortrait)
                        buildingPortrait.Show(ActualBuildingModel(b.Id),host.PlayerCamera,
                            b.Id+"/"+b.Level);
                    buildingFunction.text=b.Function??"";
                    buildingCosts.text=string.IsNullOrWhiteSpace(b.Costs)
                        ?(host.English?"No cost":"Sin coste")
                        :CompactRealCosts(b.Costs);
                    // Real numbers from the canonical offer, with new isolated
                    // illustrated resource icons. No mock costs are embedded.
                    costHeader.gameObject.SetActive(!string.IsNullOrWhiteSpace(b.Costs));
                    var wood=Regex.Match(b.Costs??"",
                        @"(?:Madera|Wood)\s+([\d.,]+)",RegexOptions.IgnoreCase);
                    var stone=Regex.Match(b.Costs??"",
                        @"(?:Piedra|Stone)\s+([\d.,]+)",RegexOptions.IgnoreCase);
                    bool realPair=wood.Success && stone.Success &&
                        !Regex.IsMatch(b.Costs??"",
                            @"(?:Comida|Food)\s+[\d.,]+",RegexOptions.IgnoreCase);
                    buildingCosts.gameObject.SetActive(!realPair);
                    for(int i=0;i<2;i++)
                    {
                        costIcons[i].gameObject.SetActive(realPair);
                        costQuantities[i].gameObject.SetActive(realPair);
                    }
                    if(realPair)
                    {
                        costQuantities[0].text=wood.Groups[1].Value;
                        costQuantities[1].text=stone.Groups[1].Value;
                    }
                    buildingState.text=CompactRealState(b.State,b.Time);
                    bool can=b.Action!=null && b.Action.Enabled && !b.OperationActive;
                    actionButton.interactable=can;
                    // Preserve the premium copper CTA even when gameplay
                    // legitimately blocks it; state/action binder stays honest.
                    actionPlate.Type=BreachWatchPlate.PlateType.Action;
                    actionPlate.SetVerticesDirty();
                    actionLabel.text=b.Action==null?"SIN ACCIÓN":b.Action.Label;
                    actionLabel.color=can?TextIvory:new Color(.91f,.85f,.72f,1f);
                }
                // Accurate labels, artwork and routes: realm/castle, resource/sack,
                // army/swords, world/raven and heroes/helm. Only the genuine
                // first-screen routes are enabled; we never dispatch old panels.
                for(int i=0;i<5;i++)
                {
                    navPlates[i].Type=i==(host.InWorld?3:selectedTab)
                        ?BreachWatchPlate.PlateType.Selected
                        :BreachWatchPlate.PlateType.Navigation;
                    bool allowed=(i==0||i==1||(i==3&&host.CanOpen(RoyalSection.World)));
                    navButtons[i].interactable=allowed;
                    var icon=navButtons[i].GetComponentInChildren<BreachWatchGlyph>();
                    if(icon && icon.enabled)
                    {
                        icon.Ink=allowed?TextIvory:Locked;
                        icon.SetVerticesDirty();
                    }
                    // Disabled destination is still visible, never presented as playable.
                    var p=navPlates[i];
                    if(p)p.color=allowed
                        ? i==(host.InWorld?3:selectedTab)?new Color(1f,.86f,.77f):Color.white
                        : new Color(.86f,.83f,.77f,.97f);
                }
            }
            catch(Exception e)
            {
                SetFeedback("Error en datos reales: "+e.GetType().Name);
                Debug.LogException(e);throw;
            }
        }
        static string CompactRealState(string state,string time)
        {
            // Preserve the real operation and timer; strip only duplicated
            // prose, not amounts or progress. This is essential on phone HUDs.
            state=state??"";
            time=time??"";
            foreach(var prefix in new[]{"Duración · ","Duration · ","Tiempo restante · ",
                "Time remaining · ","Duración: ","Duration: "})
                if(time.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))
                {time=time.Substring(prefix.Length);break;}
            return time.Length==0?state:state+" · "+time;
        }
        static string CompactRealCosts(string source)
        {
            // Keep exact authoritative amounts and names. Remove only the
            // redundant newline header so a phone panel cannot silently crop
            // a second row of real costs.
            if(string.IsNullOrEmpty(source))return source;
            if(source.StartsWith("Coste de la acción\n",StringComparison.Ordinal))
                return source.Substring("Coste de la acción\n".Length);
            if(source.StartsWith("Action cost\n",StringComparison.Ordinal))
                return source.Substring("Action cost\n".Length);
            if(source.StartsWith("Coste pagado\n",StringComparison.Ordinal))
                return "Pagado · "+source.Substring("Coste pagado\n".Length);
            if(source.StartsWith("Cost paid\n",StringComparison.Ordinal))
                return "Paid · "+source.Substring("Cost paid\n".Length);
            return source;
        }

        // Models from the real playable city, never a mocked building artwork
        // or a reused RoyalGold miniature/prefab.
        bool BlocksNativePointer(Vector2 position)
        {
            var eventSystem=EventSystem.current;
            if(eventSystem && raycaster && raycaster.enabled && canvas && canvas.enabled)
            {
                pointerHits.Clear();
                raycaster.Raycast(new PointerEventData(eventSystem) { position=position },pointerHits);
                bool blocked=pointerHits.Count>0;
                pointerHits.Clear();
                if(blocked)return true;
            }
            return priorPointerBlocker!=null && priorPointerBlocker(position);
        }

        GameObject ActualBuildingModel(string id)
        {
            if(!host || !host.City) return null;
            if(id=="bastion")
            {
                var level=host.City.Gateway.Snapshot().BastionLevel;
                if(level>=2 && host.City.BastionTwo &&
                    host.City.BastionTwo.activeInHierarchy) return host.City.BastionTwo;
                return host.City.BastionOne;
            }
            if(!host.City.Parcels) return null;
            if(id=="sawmill") return host.City.Parcels.LeftBuilt;
            if(id=="barracks" || id=="upgrade-barracks")
                return host.City.Parcels.RightBuilt;
            return null; // Never invent an illustration for other sites.
        }

        void OnDestroy()
        {
            if(host && host.Hud)host.Hud.SelectionChanged-=OnCanonicalSelection;
            foreach(var entry in originalCameraMasks)
                if(entry.Key && (entry.Value & (1<<5))==0)
                    entry.Key.cullingMask &= ~(1<<5);
            if(host && host.City && host.City.Presenter)
                host.City.Presenter.ExternalPointerBlocked=priorPointerBlocker;
            if(legacyCanvas)legacyCanvas.enabled=originalCanvas;
            if(legacyRaycaster)legacyRaycaster.enabled=originalRaycaster;
            if(legacyGroup){legacyGroup.alpha=originalAlpha;legacyGroup.blocksRaycasts=originalBlocks;legacyGroup.interactable=originalInteractable;}
        }
    }
}