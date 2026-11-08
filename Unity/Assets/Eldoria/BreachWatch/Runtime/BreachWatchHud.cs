using System;
using System.Collections.Generic;
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
        static readonly Color TextIvory = new Color(.90f,.85f,.75f,1f);
        static readonly Color Muted = new Color(.66f,.67f,.64f,1f);
        static readonly Color Copper = new Color(.82f,.48f,.31f,1f);
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
        Font font;
        RectTransform uiRoot;
        RectTransform identity, resourceRow, objective, panel, portraitFrame, buttonRow, utilityRail, centerMark;
        RectTransform[] resourceChips = new RectTransform[3];
        Text identityName, identityPower, chapterName, chapterObjective, chapterProgress;
        Text[] resourceAmounts = new Text[3];
        readonly Image[] resourceIllustrations = new Image[3];
        Text buildingName, buildingLevel, buildingFunction, buildingCosts, buildingState, actionLabel, feedback;
        Button actionButton, closeButton;
        RawImage portraitView;
        BreachWatchBuildingPortrait buildingPortrait;
        BreachWatchPlate actionPlate;
        readonly Button[] navButtons = new Button[5];
        readonly BreachWatchPlate[] navPlates = new BreachWatchPlate[5];
        string selected="";
        int selectedTab;
        bool fullObjective;
        bool lastPortrait;
        int lastWidth=-1,lastHeight=-1;
        float nextRefresh;
        bool created;
        RoyalSnapshot snapshot;
        readonly string[] navLabels = {"REINO", "CONSTRUIR", "UNIDADES", "MUNDO", "CRÓNICAS"};
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
        public RectTransform BuildingPanel => panel;
        public bool HasLiveBuildingPortrait => buildingPortrait && buildingPortrait.HasLiveBuilding;
        public bool BuildingPortraitExclusive => buildingPortrait && buildingPortrait.RenderedFromIsolatedRealMeshes;
        public bool HasTrueResourceIllustrations =>
            resourceIllustrations[0] && resourceIllustrations[0].sprite &&
            resourceIllustrations[0].sprite.name=="wood" &&
            resourceIllustrations[1] && resourceIllustrations[1].sprite &&
            resourceIllustrations[1].sprite.name=="stone" &&
            resourceIllustrations[2] && resourceIllustrations[2].sprite &&
            resourceIllustrations[2].sprite.name=="food";
        public bool UsesOriginalPaintedMedallions =>
            navPlates[0] && navPlates[0].sprite &&
            navPlates[0].sprite.name=="nav_sculpted_medallion";
        public bool UsesOriginalForgedPortraitFrame =>
            portraitFrame && portraitFrame.GetComponent<BreachWatchPlate>() &&
            portraitFrame.GetComponent<BreachWatchPlate>().sprite &&
            portraitFrame.GetComponent<BreachWatchPlate>().sprite.name=="portrait_forged_iron";
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
            font=Resources.Load<Font>("EldoriaRetention/reference-serif");
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
        BreachWatchGlyph Glyph(Transform parent,string name,BreachWatchGlyph.Sigil symbol)
        {
            var rect=Rect(name,parent);var g=rect.gameObject.AddComponent<BreachWatchGlyph>();
            g.Type=symbol;g.Ink=TextIvory;g.raycastTarget=false;return g;
        }
        Text Label(Transform parent,string name,int size,TextAnchor align,Color tint,bool bold=false)
        {
            var rect=Rect(name,parent);
            var text=rect.gameObject.AddComponent<Text>();text.font=font;text.fontSize=size;text.alignment=align;
            text.color=tint;text.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;
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
            tint.disabledColor=new Color(.48f,.48f,.48f,1f);
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
            // Asymmetrical stone architecture instead of a header/footer re-skin.
            identity=Rect("Identity / basalt crest",uiRoot);Plate(identity,BreachWatchPlate.PlateType.Basalt);
            var seal=Glyph(identity,"Breached crest",BreachWatchGlyph.Sigil.Breach);At((RectTransform)seal.transform,10,9,56,58);seal.Ink=Copper;
            identityName=Label(identity,"Player name",22,TextAnchor.MiddleLeft,TextIvory,true);
            identityPower=Label(identity,"Realm power",16,TextAnchor.MiddleLeft,Muted);
            resourceRow=Rect("Separate resource stones",uiRoot);
            for(int i=0;i<3;i++)
            {
                resourceChips[i]=Rect("Resource "+i,resourceRow);Plate(resourceChips[i],BreachWatchPlate.PlateType.Resource);
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
            objective=Rect("Chapter side tablet",uiRoot);Plate(objective,BreachWatchPlate.PlateType.Basalt);
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
            panel=Rect("Independent selected-building cartouche",uiRoot);Plate(panel,BreachWatchPlate.PlateType.Basalt);
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
            At((RectTransform)buildingSeal.transform,14,10,53,53);
            buildingName=Label(panel,"Real building name",22,TextAnchor.MiddleLeft,TextIvory,true);
            buildingLevel=Label(panel,"Real building level",15,TextAnchor.MiddleLeft,Copper,true);
            buildingFunction=Label(panel,"Real effects",17,TextAnchor.UpperLeft,Muted);
            buildingCosts=Label(panel,"Real costs",15,TextAnchor.UpperLeft,TextIvory);
            buildingState=Label(panel,"Real state and timer",15,TextAnchor.UpperLeft,Muted);
            var closeRect=BuildButton(panel,"Close selected building",BreachWatchGlyph.Sigil.Cross,()=>SelectBuilding(""));
            closeButton=closeRect.GetComponent<Button>();
            var actionRect=Rect("Original gameplay action / new presentation",panel);
            actionPlate=Plate(actionRect,BreachWatchPlate.PlateType.Action);
            actionButton=Button(actionRect,SubmitSelectedBuilding);
            actionPlate.raycastTarget=true;
            actionButton.targetGraphic=actionPlate;
            actionLabel=Label(actionButton.transform,"Real action title",17,TextAnchor.MiddleCenter,TextIvory,true);
            Fill(actionLabel.rectTransform,6);
            buttonRow=Rect("Five standalone stone navigation emblems",uiRoot);
            for(int i=0;i<5;i++)
            {
                int index=i;var rt=BuildButton(buttonRow,navLabels[i],navIcons[i],()=>Navigate(index),BreachWatchPlate.PlateType.Navigation);
                navButtons[i]=rt.GetComponent<Button>();navPlates[i]=rt.GetComponent<BreachWatchPlate>();
                navPlates[i].Type=i==0?BreachWatchPlate.PlateType.Selected:BreachWatchPlate.PlateType.Navigation;
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
                case 0:selectedTab=0;SelectBuilding("bastion");break;
                case 1:
                    selectedTab=1;
                    var s=host.Read("",host.English);
                    SelectBuilding(s.BastionLevel<2?"sawmill":s.BastionLevel<3?"barracks":"bastion");break;
                case 3:
                    if(host.CanOpen(RoyalSection.World))
                    {selectedTab=3;host.Open(RoyalSection.World);SelectBuilding("");SyncLiveCamera();}
                    break;
                case 4:selectedTab=4;fullObjective=!fullObjective;Reflow(lastWidth,lastHeight);RenderNow();break;
                // Unidades is deliberately noninteractive in this first-screen proof:
                // do not silently open an old UI panel or claim the route is ported.
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
        public void Reflow(int pixelWidth,int pixelHeight)
        {
            if(!canvas||!created && !uiRoot)return;
            pixelWidth=Mathf.Max(320,pixelWidth);pixelHeight=Mathf.Max(300,pixelHeight);
            lastWidth=pixelWidth;lastHeight=pixelHeight;
            // Device safe area is part of the NEW Canvas' own layout contract.
            // Offscreen evidence at synthetic resolutions uses its full target
            // instead of accidentally inheriting the host monitor's cut-outs.
            if(Screen.width==pixelWidth && Screen.height==pixelHeight)
            {
                Rect safe=Screen.safeArea;
                if(safe.width>0 && safe.height>0)
                {
                    uiRoot.anchorMin=new Vector2(safe.xMin/Screen.width,safe.yMin/Screen.height);
                    uiRoot.anchorMax=new Vector2(safe.xMax/Screen.width,safe.yMax/Screen.height);
                    uiRoot.offsetMin=Vector2.zero;uiRoot.offsetMax=Vector2.zero;
                    pixelWidth=Mathf.Max(320,Mathf.RoundToInt(safe.width));
                    pixelHeight=Mathf.Max(300,Mathf.RoundToInt(safe.height));
                }
            }
            else Stretch(uiRoot);
            bool portrait=pixelHeight>pixelWidth;
            float scale=portrait?pixelWidth/430f:Mathf.Max(.86f,Mathf.Min(pixelWidth/1180f,pixelHeight/650f));
            scaler.scaleFactor=scale;
            float w=pixelWidth/scale,h=pixelHeight/scale;
            float margin=portrait?13:17;
            // Correct in the camera target (native offscreen captures included).
            if(portrait)
            {
                buildingFunction.fontSize=17;
                buildingCosts.fontSize=15;
                buildingState.fontSize=15;
                At(identity,margin,10,w-margin*2,79);
                At(identityName.rectTransform,71,13,w-154,29);
                At(identityPower.rectTransform,71,45,w-154,23);
                At(centerMark,w-margin*2-61,14,52,52);centerMark.SetParent(uiRoot,false);
                centerMark.SetAsLastSibling();
                At(resourceRow,margin,99,w-margin*2,49);
                float chip=(w-2*margin-8)/3f;
                for(int i=0;i<3;i++)
                {
                    At(resourceChips[i],i*(chip+4),0,chip,44);
                    At(resourceAmounts[i].rectTransform,42,3,chip-45,38);
                    resourceAmounts[i].fontSize=14;
                }
                At(objective,margin,159,w-margin*2,fullObjective?121:89);
                At(chapterName.rectTransform,16,7,w-40,21);
                At(chapterObjective.rectTransform,16,29,w-40,41);
                At(chapterProgress.rectTransform,16,66,w-40,18);
                At(utilityRail,margin,fullObjective?300:267,56,173);
                for(int i=0;i<3;i++)
                {
                    var item=(RectTransform)utilityRail.GetChild(i);At(item,0,i*58,53,53);
                }
                float navHeight=69,navTop=h-navHeight-11f;
                At(buttonRow,margin,navTop,w-2*margin,navHeight);
                float bw=(w-margin*2-4*6)/5f;
                for(int i=0;i<5;i++)
                {
                    At((RectTransform)navButtons[i].transform,i*(bw+6)+(bw-Mathf.Min(bw,65))*.5f,1,Mathf.Min(bw,65),65);
                    navPlates[i].Type=i==0?BreachWatchPlate.PlateType.Selected:BreachWatchPlate.PlateType.Navigation;
                }
                float panelHeight=Mathf.Min(260,Mathf.Max(232,h-485));
                At(panel,margin,navTop-panelHeight-13,w-margin*2,panelHeight);
                At(buildingName.rectTransform,70,8,w-132,32);
                At(buildingLevel.rectTransform,70,37,w-132,19);
                At(((RectTransform)closeButton.transform),w-margin*2-51,10,39,40);
                float portraitHeight=panelHeight<250?62:84;
                At(portraitFrame,15,63,139,portraitHeight);
                At(buildingFunction.rectTransform,166,68,w-2*margin-183,portraitHeight-6);
                float costTop=69+portraitHeight;
                At(buildingCosts.rectTransform,16,costTop,w-2*margin-34,27);
                At(buildingState.rectTransform,16,costTop+29,w-2*margin-34,18);
                At(((RectTransform)actionButton.transform),16,panelHeight-49,w-margin*2-32,44);
                // Portrait tablets and the side rail may share Y only when
                // enough genuinely unobscured city area remains. 900x1600
                // used to overlap the lower disabled seal with the building.
                float railTop=fullObjective?300:267;
                float clearRailSpace=navTop-panelHeight-13-railTop-12;
                for(int i=0;i<utilityRail.childCount;i++)
                    utilityRail.GetChild(i).gameObject.SetActive((i+1)*58<=clearRailSpace);
                At(feedback.rectTransform,69,fullObjective?310:277,w-100,43);
            }
            else
            {
                At(identity,margin,12,Mathf.Min(338,w*.29f),76);
                At(identityName.rectTransform,72,10,256,31);
                At(identityPower.rectTransform,72,43,256,23);
                At(centerMark,w*.49f-26,15,52,52);
                float chipW=Mathf.Min(124, (w*.40f-14)/3f);
                At(resourceRow,w-3*chipW-3*7-margin,17,3*chipW+2*7,46);
                for(int i=0;i<3;i++)
                {
                    At(resourceChips[i],i*(chipW+7),0,chipW,43);
                    At(resourceAmounts[i].rectTransform,43,3,chipW-46,38);resourceAmounts[i].fontSize=16;
                }
                At(objective,margin,111,Mathf.Min(280,w*.23f),fullObjective?134:109);
                float questWidth=Mathf.Min(280,w*.23f)-30f;
                At(chapterName.rectTransform,15,9,questWidth,20);
                At(chapterObjective.rectTransform,15,34,questWidth,46);
                At(chapterProgress.rectTransform,15,85,questWidth,20);
                At(utilityRail,margin,fullObjective?266:238,59,183);
                for(int i=0;i<3;i++)
                {
                    utilityRail.GetChild(i).gameObject.SetActive(true);
                    At((RectTransform)utilityRail.GetChild(i),0,i*62,56,56);
                }
                float pw=Mathf.Min(338,w*(h<500f?.32f:.285f)),ph=Mathf.Min(335,h*.50f);
                At(panel,w-pw-margin,115,pw,ph);
                At(buildingName.rectTransform,72,11,pw-130,31);
                At(buildingLevel.rectTransform,72,42,pw-130,22);
                At((RectTransform)closeButton.transform,pw-52,10,40,40);
                if(ph<282f)
                {
                    // One-line genuine state below the columns; no clipping
                    // or overlap even at the actual phone 844x390 viewport.
                    At(portraitFrame,14,65,104,88);
                    At(buildingFunction.rectTransform,124,65,pw-138,50);
                    At(buildingCosts.rectTransform,124,117,pw-138,33);
                    At(buildingState.rectTransform,17,153,pw-34,19);
                    At((RectTransform)actionButton.transform,15,ph-51,pw-30,45);
                    buildingFunction.fontSize=15;
                    buildingCosts.fontSize=14;
                    buildingState.fontSize=14;
                }
                else
                {
                    buildingFunction.fontSize=17;
                    buildingCosts.fontSize=15;
                    buildingState.fontSize=15;
                    float portraitHeight=Mathf.Clamp(ph-232,44,102);
                    At(portraitFrame,17,70,pw-34,portraitHeight);
                    At(buildingFunction.rectTransform,18,77+portraitHeight,pw-36,35);
                    At(buildingCosts.rectTransform,18,118+portraitHeight,pw-34,27);
                    At(buildingState.rectTransform,18,150+portraitHeight,pw-36,20);
                    At((RectTransform)actionButton.transform,17,ph-58,pw-34,49);
                }
                float count=5,bw=Mathf.Min(93,(w-190)/count),gap=13;
                float navWidth=bw*count+gap*(count-1);
                At(buttonRow,(w-navWidth)*.5f,h-89,navWidth,81);
                for(int i=0;i<5;i++)At((RectTransform)navButtons[i].transform,i*(bw+gap)+(bw-74)*.5f,2,74,74);
                At(feedback.rectTransform,margin,h-135,w-margin*2,31);
            }
            // Preserve the true 2:1 camera aspect when the illustrated well
            // has a different physical aspect in portrait/landscape.
            if(portraitView && portraitFrame)
            {
                float frameRatio=Mathf.Max(.1f,(portraitFrame.rect.width-16f) /
                    Mathf.Max(1f,portraitFrame.rect.height-16f));
                if(frameRatio<=2f)
                {
                    float wUv=frameRatio*.5f;
                    portraitView.uvRect=new Rect((1f-wUv)*.5f,0f,wUv,1f);
                }
                else
                {
                    float hUv=2f/frameRatio;
                    portraitView.uvRect=new Rect(0f,(1f-hUv)*.5f,1f,hUv);
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
                    buildingCosts.text=string.IsNullOrWhiteSpace(b.Costs)?"Sin coste":b.Costs;
                    buildingState.text=(b.State??"")+(string.IsNullOrWhiteSpace(b.Time)?"":"  ·  "+b.Time);
                    bool can=b.Action!=null && b.Action.Enabled && !b.OperationActive;
                    actionButton.interactable=can;
                    actionPlate.Type=can?BreachWatchPlate.PlateType.Action:BreachWatchPlate.PlateType.Dangerous;
                    actionPlate.SetVerticesDirty();
                    actionLabel.text=b.Action==null?"SIN ACCIÓN":b.Action.Label;
                    actionLabel.color=can?TextIvory:Locked;
                }
                // A screen slice is not allowed to lie about unsupported destinations.
                // Reino, Construir, Mundo (when unlocked), and Crónicas are live;
                // the future independent Unidades screen remains explicitly disabled.
                for(int i=0;i<5;i++)
                {
                    navPlates[i].Type=i==(host.InWorld?3:selectedTab)
                        ?BreachWatchPlate.PlateType.Selected
                        :BreachWatchPlate.PlateType.Navigation;
                    bool allowed=i!=2&&(i!=3||host.CanOpen(RoyalSection.World));
                    navButtons[i].interactable=allowed;
                    var icon=navButtons[i].GetComponentInChildren<BreachWatchGlyph>();
                    if(icon){icon.Ink=allowed?TextIvory:Locked;icon.SetVerticesDirty();}
                }
            }
            catch(Exception e)
            {
                SetFeedback("Error en datos reales: "+e.GetType().Name);
                Debug.LogException(e);throw;
            }
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