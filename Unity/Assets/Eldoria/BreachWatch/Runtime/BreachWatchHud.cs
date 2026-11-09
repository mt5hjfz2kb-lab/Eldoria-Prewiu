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
        Font font, boldFont, bodyFont, bodyBoldFont;
        RectTransform uiRoot;
        RectTransform identity, identityCrest, resourceRow, objective, panel, portraitFrame, descriptionDivider, buttonRow, utilityRail, centerMark, feedbackPlate;
        RectTransform closePainted;
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
        BreachWatchCityComposition cityComposition;
        BreachWatchPlate actionPlate;
        Image exactOwnerGoldBorder;
        BreachWatchNavFiligree navFiligree;
        readonly BreachWatchPlate[] approvedUtilitySeals = new BreachWatchPlate[3];
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
        int lastNavigationMask=-1,lastResourceCount=-1;
        UnityEngine.Rect lastSafeArea=UnityEngine.Rect.zero;
        bool lastWorldMode;
        float nextRefresh;
        float feedbackUntil;
        bool created;
        RoyalSnapshot snapshot;
        readonly string[] navLabels = {"REINO", "RECURSOS", "EJÉRCITO", "EXPLORAR", "HÉROES"};
        // One canonical navigation artwork mapping. The medallion sprites
        // are complete original illustrations; NEVER create vector placeholder
        // icon components behind or on top of these five surfaces.

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
        public bool PortraitCityFramed => cityComposition && cityComposition.PortraitFramed;
        public float PortraitCityOrthographicSize => cityComposition?cityComposition.OrthographicSize:0f;
        public bool BuildingPortraitEdgeClipped =>
            buildingPortrait && buildingPortrait.PortraitGeometryClipped;
        public UnityEngine.Rect BuildingPortraitUvRect =>
            portraitView ? portraitView.uvRect : UnityEngine.Rect.zero;
        public UnityEngine.Rect BuildingPortraitMeshUvBounds =>
            buildingPortrait ? buildingPortrait.LastGeometryUvBounds :
                UnityEngine.Rect.zero;
        public string LastFeedback => feedback?feedback.text:"";
        public bool HasVisibleFeedbackPlate =>
            feedbackPlate && feedbackPlate.gameObject.activeInHierarchy;
        public bool BuildingPortraitExclusive => buildingPortrait && buildingPortrait.RenderedFromIsolatedRealMeshes;
        public float BuildingPortraitDrawnFraction => buildingPortrait ? buildingPortrait.NonBackgroundPixelFraction : 0f;
        public float BuildingPortraitTransparentBackground =>
            buildingPortrait ? buildingPortrait.TransparentBackdropFraction : 0f;
        public float BuildingPortraitOpaqueMasonry =>
            buildingPortrait ? buildingPortrait.OpaqueMasonryFraction : 0f;
        public bool HasNativePremiumTypography =>
            font && boldFont && bodyFont && bodyBoldFont &&
            font==Resources.Load<Font>("VigiliaArt/Fonts/CormorantGaramond-Regular") &&
            boldFont==Resources.Load<Font>("VigiliaArt/Fonts/CormorantGaramond-SemiBold") &&
            bodyFont==Resources.Load<Font>("VigiliaArt/Fonts/LibreBaskerville-Regular") &&
            bodyBoldFont==Resources.Load<Font>("VigiliaArt/Fonts/LibreBaskerville-Bold");
        public bool HasOwnerApprovedUtilityArtwork
        {
            get
            {
                string[] names={"bell","settings","breach"};
                for(int i=0;i<names.Length;i++)
                {
                    var image=approvedUtilitySeals[i];
                    if(!image || !image.sprite ||
                       image.sprite!=Resources.Load<Sprite>(
                           "VigiliaArt/OwnerUtilities/"+names[i]))return false;
                }
                return true;
            }
        }
        public bool HasPremiumCarvedQuest
        {
            get
            {
                var tablet=objective?objective.GetComponent<BreachWatchPlate>():null;
                return tablet && tablet.sprite &&
                    tablet.sprite.name=="chiseled_quest" &&
                    tablet.type==Image.Type.Simple &&
                    !tablet.preserveAspect;
            }
        }
        public bool HasSinglePremiumIdentity =>
            identity && identity.GetComponent<BreachWatchPlate>() &&
            identity.GetComponent<BreachWatchPlate>().sprite &&
            identity.GetComponent<BreachWatchPlate>().sprite.name=="profile_bar" &&
            identity.GetComponent<BreachWatchPlate>().type==Image.Type.Sliced &&
            // The original painted profile already contains shading.
            // Do not require a second uGUI Shadow after the approved
            // single-surface unification.
            !identity.GetComponent<Shadow>() &&
            identity.GetComponentsInChildren<Graphic>(true).Length==4;

        public bool HasPremiumResourceSurfaces
        {
            get
            {
                for(int i=0;i<resourceChips.Length;i++)
                {
                    if(!resourceChips[i])return false;
                    var surface=resourceChips[i].GetComponent<BreachWatchPlate>();
                    if(!surface || !surface.sprite ||
                       surface.sprite.name!="resource_capsule" ||
                       surface.type!=Image.Type.Sliced)return false;
                }
                return true;
            }
        }
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
            identityCrest.GetComponent<BreachWatchPlate>().sprite.name=="lion_crest";
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
                // Navigation uses five sculpted medallions, while profile
                // identity is the separately approved golden-lion crest.
                var crest=Resources.Load<Sprite>("VigiliaArt/OwnerReference/lion_crest");
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
        public bool UsesPremiumForgedPortraitFrame =>
            portraitFrame && portraitFrame.GetComponent<BreachWatchPlate>() &&
            portraitFrame.GetComponent<BreachWatchPlate>().sprite &&
            portraitFrame.GetComponent<BreachWatchPlate>().sprite.name=="brass_portrait" &&
            portraitFrame.GetComponent<BreachWatchPlate>().type==Image.Type.Sliced;
        public bool UsesExactOwnerApprovedGoldBorder =>
            exactOwnerGoldBorder && exactOwnerGoldBorder.sprite &&
            exactOwnerGoldBorder.sprite==
                Resources.Load<Sprite>("VigiliaArt/ApprovedContour/owner_gold_border") &&
            exactOwnerGoldBorder.type==Image.Type.Simple &&
            !exactOwnerGoldBorder.raycastTarget;
        // The authoritative context now has exactly one slate interior and
        // one owner-approved external gold contour. The superseded basalt
        // picture and extra interim dark overlays are NOT layered under it.
        public bool UsesSinglePremiumContext =>
            panel && panel.GetComponent<Image>() &&
            panel.GetComponent<Image>().sprite ==
                Resources.Load<Sprite>("VigiliaArt/PremiumSurfaces/panel_fill") &&
            panel.GetComponent<Image>().type==Image.Type.Simple &&
            !panel.GetComponent<BreachWatchPanelInk>() &&
            !panel.GetComponent<BreachWatchPlate>() &&
            UsesExactOwnerApprovedGoldBorder;
        public bool UsesPremiumBurgundyAction =>
            actionPlate && actionPlate.sprite &&
            actionPlate.sprite.name=="upgrade_button" &&
            actionPlate.type==Image.Type.Simple &&
            actionPlate.preserveAspect &&
            !actionPlate.raycastTarget &&
            actionPlate.transform.parent==actionButton.transform;
        public IReadOnlyList<Button> NavButtons => navButtons;
        // Native read-only proof: an unavailable third resource must be
        // physically absent, not an empty capsule with a fake dash.
        public int VisibleResourceCount
        {
            get
            {
                int count=0;
                foreach(var chip in resourceChips)
                    if(chip && chip.gameObject.activeInHierarchy)count++;
                return count;
            }
        }

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
            cityComposition=gameObject.AddComponent<BreachWatchCityComposition>();
            cityComposition.Initialize(source);
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
            bodyFont=Resources.Load<Font>("VigiliaArt/Fonts/LibreBaskerville-Regular");
            bodyBoldFont=Resources.Load<Font>("VigiliaArt/Fonts/LibreBaskerville-Bold");
            if(!font)font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if(!bodyFont)bodyFont=font;
            if(!bodyBoldFont)bodyBoldFont=boldFont;
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
        void AddContactShadow(RectTransform surface,float depth,float opacity)
        {
            // The carved metal is a physical, raised tablet. Give the actual
            // UI surface a restrained contact shadow that follows its true
            // outline, never a huge opaque rectangle covering the city.
            var shadow=surface.gameObject.AddComponent<Shadow>();
            shadow.effectColor=new Color(.01f,.008f,.006f,
                Mathf.Clamp01(opacity));
            shadow.effectDistance=new Vector2(1f,-depth);
            shadow.useGraphicAlpha=true;
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
            // Keep numerals and full gameplay requirements in legible
            // Baskerville. Use genuine, slightly narrower OFL Cormorant for
            // concise two-line inscriptions and the upgrade button: the
            // latter now fits without truncation at phone width.
            bool display=name=="Player name" || name=="Real building name" ||
                name=="Compact engraved cost heading" ||
                name=="Actual chapter" || name=="Actual objective" ||
                name=="Real effects" || name=="Real action title" ||
                name=="Real state and timer";
            Font regular=display?font:bodyFont;
            Font heavy=display?boldFont:bodyBoldFont;
            text.font=bold && heavy ? heavy : regular;
            text.fontSize=size;
            text.alignment=align;
            text.color=tint;
            text.fontStyle=bold && !heavy ? FontStyle.Bold : FontStyle.Normal;
            if(name=="Compact engraved cost heading" ||
               name=="Actual chapter" || name=="Real action title")
            {
                text.resizeTextForBestFit=true;
                text.resizeTextMinSize=12;
                text.resizeTextMaxSize=size;
            }
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
        // The painted sprite is also the visible input state. The invisible
        // full-size Image remains ONLY the 44px+ raycast surface; it never
        // displays a separate pressed plate, ring or hover graphic.
        static void UseSourceArtInteraction(Button button,Graphic paint)
        {
            if(!button || !paint)return;
            var colors=button.colors;
            colors.normalColor=Color.white;
            colors.highlightedColor=new Color(1f,.975f,.91f,1f);
            colors.selectedColor=new Color(1f,.975f,.91f,1f);
            colors.pressedColor=new Color(.78f,.73f,.67f,1f);
            // Keep source art fully readable while an authentic action
            // is blocked; explanation is handled by the input-only hint.
            colors.disabledColor=Color.white;
            colors.fadeDuration=.09f;
            button.colors=colors;
            button.targetGraphic=paint;
            button.transition=Selectable.Transition.ColorTint;
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
            // REPLACEMENT, not a bevel pasted over another panel: one
            // authoritative forged-metal plate from the approved art kit.
            var identitySurface=Plate(identity,BreachWatchPlate.PlateType.Basalt);
            var finalProfile=Resources.Load<Sprite>("VigiliaArt/OwnerControls/profile_bar");
            if(!finalProfile)throw new InvalidOperationException(
                "Missing Vigilia's approved player profile art");
            identitySurface.ApplyIndependentArt(finalProfile,true);
            identitySurface.color=new Color(.97f,.95f,.90f,1f);
            // The entire visible account plate must prevent accidental
            // click-through to the city, without pretending to navigate.
            identitySurface.raycastTarget=true;
            // No additional uGUI Shadow: the genuine profile painting already
            // owns its falloff. Doubling it made the entire header float.
            // The owner-approved compact reference has NO red top banner.
            // Independent round carved crest replaces the obsolete banner.
            identityCrest=Rect("Independent circular realm crest",identity);
            Plate(identityCrest,BreachWatchPlate.PlateType.Navigation);
            // The actual owner-approved design uses a *lion* personal crest,
            // not another castle. Keep it a standalone 56px sprite; never
            // place the approved full screenshot under gameplay controls.
            var crestImage=Resources.Load<Sprite>("VigiliaArt/OwnerReference/lion_crest");
            if(!crestImage)crestImage=Resources.Load<Sprite>("VigiliaArt/PremiumMedallions/castle");
            if(crestImage)identityCrest.GetComponent<BreachWatchPlate>()
                .ApplyIndependentArt(crestImage);
            identityCrest.GetComponent<BreachWatchPlate>().VisualScale=.94f;
            At(identityCrest,4,4,50,50);
            identityName=Label(identity,"Player name",22,TextAnchor.MiddleLeft,TextIvory,true);
            identityPower=Label(identity,"Realm power",16,TextAnchor.MiddleLeft,Muted);
            resourceRow=Rect("Separate resource stones",uiRoot);
            var originalResourceStone=Resources.Load<Sprite>(
                "VigiliaArt/OwnerControls/resource_capsule");
            if(!originalResourceStone)throw new InvalidOperationException(
                "Missing Vigilia's approved resource capsule art");
            for(int i=0;i<3;i++)
            {
                resourceChips[i]=Rect("Resource "+i,resourceRow);
                var resourceSurface=Plate(resourceChips[i],BreachWatchPlate.PlateType.Resource);
                // Single premium brass-edged 9-slice capsule. The earlier
                // procedural tile PLUS a second border produced an ugly
                // cross/X behind the actual resource amount in Unity.
                resourceSurface.ApplyIndependentArt(originalResourceStone,true);
                resourceSurface.color=new Color(.97f,.95f,.90f,1f);

                resourceSurface.raycastTarget=true;
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
            // One authentic transparent gold/orange break and chiseled
            // horizontal filigree from the owner's accepted screenshot.
            // This replaces, never overlays, the former generic X-shaped
            // procedural Breach; it is decoration, not a gameplay control.
            centerMark=Rect("One owner-approved transparent Breach signal",uiRoot);
            var approvedRift=Resources.Load<Sprite>(
                "VigiliaArt/OwnerControls/breach_signal");
            if(!approvedRift)
                throw new InvalidOperationException(
                    "Vigilia requires its original standalone orange Breach artwork");
            var crack=centerMark.gameObject.AddComponent<Image>();
            crack.sprite=approvedRift;
            crack.type=Image.Type.Simple;
            crack.preserveAspect=true;
            crack.raycastTarget=false;
            objective=Rect("Chapter side tablet",uiRoot);
            var chapterStone=Plate(objective,BreachWatchPlate.PlateType.Basalt);
            var angledPlate=Resources.Load<Sprite>(
                "VigiliaArt/PremiumSurfaces/chiseled_quest");
            if(!angledPlate)throw new InvalidOperationException(
                "Missing Vigilia's approved integral chapter tablet");
            // The actual tapered ornament is a single authored silhouette:
            // never silently fall back to a generic procedural dark panel.
            chapterStone.ApplyIndependentArt(angledPlate,false);
            chapterStone.color=new Color(.97f,.95f,.90f,1f);
            chapterStone.preserveAspect=false;

            // No extra shadow: the carved quest PNG already has a baked
            // shadow and finished outline. This is one plate, not two.
            // The chapter's approval reference has a tiny tower crest
            // attached to the tablet. The open-air Breach sigil is separate.
            var objectiveSeal=Glyph(objective,"Chapter stone rune",BreachWatchGlyph.Sigil.Castle);
            objectiveSeal.Ink=Copper;
            At((RectTransform)objectiveSeal.transform,9,9,20,20);
            chapterName=Label(objective,"Actual chapter",15,TextAnchor.MiddleLeft,Copper,true);
            chapterObjective=Label(objective,"Actual objective",17,TextAnchor.UpperLeft,TextIvory);
            chapterProgress=Label(objective,"Objective progress",14,TextAnchor.MiddleLeft,Muted);
            var questPlate=objective.GetComponent<BreachWatchPlate>();questPlate.raycastTarget=true;
            var questHit=Button(objective,()=>{fullObjective=!fullObjective;Reflow(lastWidth,lastHeight);});
            questHit.targetGraphic=questPlate;
            questHit.interactable=true;
            utilityRail=Rect("Freestanding secondary seals",uiRoot);
            // These are visual assets for future REAL destinations, not usable
            // Chapter-I controls. Keep their subtree dormant until a live
            // screen and route are implemented; disabled-looking fake buttons
            // still imply functionality and clutter the playable world.
            utilityRail.gameObject.SetActive(false);
            // No fake buttons: these destinations are not part of the first
            // proof. Keep their artistic seals, but make disabled state explicit.
            foreach(var design in new [] {
                (name:"Notices",icon:BreachWatchGlyph.Sigil.Bell,art:"bell"),
                (name:"Settings",icon:BreachWatchGlyph.Sigil.Settings,art:"settings"),
                (name:"Fissure",icon:BreachWatchGlyph.Sigil.Breach,art:"breach") })
            {
                var seal=BuildButton(utilityRail,design.name,design.icon,()=>{});
                var disabled=seal.GetComponent<Button>();
                disabled.interactable=false; // never fake an unbuilt destination
                disabled.transition=Selectable.Transition.None;
                var utilityPlate=seal.GetComponent<BreachWatchPlate>();
                var genuineArt=Resources.Load<Sprite>(
                    "VigiliaArt/OwnerUtilities/"+design.art);
                if(utilityPlate)
                {
                    utilityPlate.Type=BreachWatchPlate.PlateType.Navigation;
                    if(genuineArt)utilityPlate.ApplyIndependentArt(genuineArt);
                    utilityPlate.VisualScale=.94f;
                }
                approvedUtilitySeals[design.art=="bell"?0:
                    design.art=="settings"?1:2]=utilityPlate;
                var glyph=seal.GetComponentInChildren<BreachWatchGlyph>();
                if(glyph)
                {
                    // The uploaded approved artwork already includes the
                    // fully engraved icon. Do not draw a cheap secondary
                    // vector Bell/Gear/Breach on top of it.
                    glyph.enabled=!genuineArt;
                    if(!genuineArt)
                    {
                        glyph.Ink=new Color(.72f,.52f,.32f,1f);
                        glyph.SetVerticesDirty();
                    }
                }
            }
            panel=Rect("Independent selected-building cartouche",uiRoot);
            // ONE actual authored dark-stone surface owns this root canvas.
            // Delete the obsolete intermediate nine-slice panel and its
            // second dark child. The bright outer metal is a separate,
            // transparent approved border; neither competes with the other.
            // Actual warm stone from the owner's approved, BLANK texture
            // source. It REPLACES the featureless procedural PanelInk mesh:
            // exactly one painted surface behind the single transparent
            // outer-gold reference border, never another card layered on top.
            var realMineral=Resources.Load<Sprite>("VigiliaArt/PremiumSurfaces/panel_fill");
            if(!realMineral)
                throw new InvalidOperationException(
                    "Missing separately authored owner-reference mineral card material");
            var singleSlate=panel.gameObject.AddComponent<Image>();
            singleSlate.sprite=realMineral;
            singleSlate.color=new Color(.97f,.95f,.90f,1f);
            singleSlate.type=Image.Type.Simple;
            singleSlate.preserveAspect=false;
            // Passive native input shield prevents clicks leaking through.
            singleSlate.raycastTarget=true;
            // The mineral surface and the ONE owner rim carry all relief.
            // A second UI Shadow beneath them created a detached slab.
            // Most faithful finish: transparent embossed metal border
            // extracted solely from the exact image the owner approved.
            // Real building data and 3D meshes remain separate.
            var outlined=Resources.Load<Sprite>("VigiliaArt/ApprovedContour/owner_gold_border");
            if(outlined)
            {
                var ring=Rect("Exact owner-approved independent gold relief ring",panel);
                Fill(ring);
                // The reference's upper/lower diamonds sit OUTSIDE the
                // readable tablet. Enlarge only the independently rendered
                // ornamental ring, never the actual text/button surfaces.
                ring.offsetMin=new Vector2(-1.5f,-19f);
                ring.offsetMax=new Vector2(1.5f,19f);
                exactOwnerGoldBorder=ring.gameObject.AddComponent<Image>();
                exactOwnerGoldBorder.sprite=outlined;
                // One complete owner-approved border, including top and bottom
                // diamonds. No second bottom finial overlaid on the old one.
                exactOwnerGoldBorder.type=Image.Type.Simple;
                exactOwnerGoldBorder.raycastTarget=false;
                exactOwnerGoldBorder.color=new Color(.96f,.93f,.87f,.88f);
            }
            else throw new InvalidOperationException(
                "Missing Vigilia's single approved sculpted-gold contour");
            portraitFrame=Rect("Carved recess / live building illustration",panel);
            var brassFrame=Plate(portraitFrame,BreachWatchPlate.PlateType.Portrait);
            var premiumBrass=Resources.Load<Sprite>("VigiliaArt/PremiumSurfaces/brass_portrait");
            if(!premiumBrass)throw new InvalidOperationException(
                "Missing Vigilia's genuine brass picture recess");
            brassFrame.ApplyIndependentArt(premiumBrass,true);
            // The portrait must be a subtle recess of the SAME mineral
            // cartouche, not a second thick black/bronze plate perched on it.
            // Keep the original 9-slice ornament and a single live 3D image,
            // but let the shared mineral body read through the heavy center.
            // The actual 3D image now has a transparent background. The old
            // 55% opaque border becomes a SECOND golden frame below that
            // transparent texture, reintroducing the pasted-on-panel effect.
            // Keep a very soft relief at 18% and the ONE outer approved rim.
            // The brass source PNG has an entirely opaque CENTRAL slab.
            // Even at 18% opacity it rendered an extra rectangular card
            // behind the URP alpha-cut 3D Bastion. Retain its authored
            // asset for provenance, but do NOT paint the duplicate surface.
            brassFrame.enabled=false;
            brassFrame.raycastTarget=false;
            var liveView=Rect("Live 3D building portrait / no old HUD sprites",portraitFrame);
            Fill(liveView,5);
            portraitView=liveView.gameObject.AddComponent<RawImage>();
            portraitView.color=Color.white;
            portraitView.raycastTarget=false;
            buildingPortrait=gameObject.AddComponent<BreachWatchBuildingPortrait>();
            buildingPortrait.Initialize(portraitView);
            // Keep one SMALL engraved building identifier, not a bright
            // white pictogram visually floating above the owner's metalwork.
            // Typography and engraved icon now share the warm metal palette.
            var buildingSeal=Glyph(panel,"Building crest",BreachWatchGlyph.Sigil.Castle);
            buildingSeal.Ink=new Color(.76f,.62f,.42f,1f);
            At((RectTransform)buildingSeal.transform,18,16,29,29);
            buildingName=Label(panel,"Real building name",22,TextAnchor.MiddleLeft,TextIvory,true);
            buildingLevel=Label(panel,"Real building level",15,TextAnchor.MiddleLeft,Copper,true);
            buildingFunction=Label(panel,"Real effects",17,TextAnchor.UpperLeft,TextIvory);
            descriptionDivider=Rect("Original engraved narrative divider",panel);
            var divider=descriptionDivider.gameObject.AddComponent<BreachWatchSectionDivider>();
            divider.raycastTarget=false;
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
            // Source-faithful one-piece owner circular close X. Remove the
            // old black octagon, its vector cross and decorative double rim.
            // As with navigation/CTA, the root is a full-height *invisible*
            // touch target and the child is the ONE painted graphic.
            var closeRect=Rect("Close selected building",panel);
            var closeHit=closeRect.gameObject.AddComponent<Image>();
            closeHit.color=new Color(0f,0f,0f,0f);
            closeHit.raycastTarget=true;
            closeButton=Button(closeRect,()=>SelectBuilding(""));
            closeButton.targetGraphic=closeHit;
            closeButton.transition=Selectable.Transition.None;
            var goldenClose=Resources.Load<Sprite>(
                "VigiliaArt/OwnerControls/close_medallion");
            if(!goldenClose)
                throw new InvalidOperationException(
                    "Vigilia requires a real round golden X from the owner's reference");
            closePainted=Rect("Single owner-approved circular gold X",closeRect);
            var closeIcon=closePainted.gameObject.AddComponent<Image>();
            closeIcon.sprite=goldenClose;
            closeIcon.preserveAspect=true;
            closeIcon.raycastTarget=false;
            UseSourceArtInteraction(closeButton,closeIcon);
            // Actual owner-approved burgundy CTA is a COMPLETE 246x60
            // painted motif (including its hammers). Nine-slicing a narrow
            // 147-unit button with 78px left border exaggerated the hammer
            // half and squeezed/collided with the text. Render the illustration
            // ONCE at natural aspect in a shorter well; a separate completely
            // transparent, full-height hit target provides >=44px phone taps.
            var actionRect=Rect("Original gameplay action / new presentation",panel);
            var actionHit=actionRect.gameObject.AddComponent<Image>();
            actionHit.color=new Color(0f,0f,0f,0f);
            actionHit.raycastTarget=true;
            actionButton=Button(actionRect,SubmitSelectedBuilding);
            actionButton.targetGraphic=actionHit;
            actionButton.transition=Selectable.Transition.None;
            var originalArt=Resources.Load<Sprite>("VigiliaArt/OwnerControls/upgrade_button");
            if(!originalArt)
                throw new InvalidOperationException(
                    "Vigilia requires the owner's standalone upgrade image");
            var actionVisual=Rect("One owner-approved real hammered burgundy image",actionRect);
            actionPlate=Plate(actionVisual,BreachWatchPlate.PlateType.Action);
            actionPlate.ApplyIndependentArt(originalArt);
            actionPlate.type=Image.Type.Simple;
            actionPlate.preserveAspect=true;
            actionPlate.raycastTarget=false;
            UseSourceArtInteraction(actionButton,actionPlate);
            // A blocked native button retains pointer click feedback but no
            // fake upgrade command. Dynamic label is the ONLY overlay allowed.
            var explanation=actionRect.gameObject.AddComponent<BreachWatchUnavailableActionHint>();
            explanation.Initialize(this,actionButton);
            actionLabel=Label(actionButton.transform,"Real action title",17,
                TextAnchor.MiddleCenter,TextIvory,true);
            Fill(actionLabel.rectTransform,6);
            buttonRow=Rect("Five standalone stone navigation emblems",uiRoot);
            var navLine=Rect("Subtle sculpted copper linkwork / no bottom HUD bar",buttonRow);
            Fill(navLine);
            navFiligree=navLine.gameObject.AddComponent<BreachWatchNavFiligree>();
            navFiligree.raycastTarget=false;
            // Five single-image native controls. Every symbol is drawn
            // exactly once, with an invisible full-size touch surface; the
            // icon's 6-unit transparent inset replaces stacked rings and
            // vector placeholders. The only linkwork is behind the row.
            for(int i=0;i<5;i++)
            {
                int index=i;
                string symbol=ReferenceNavArt[i];
                var painted=Resources.Load<Sprite>(
                    "VigiliaArt/PremiumMedallions/"+symbol);
                if(!painted)
                    throw new InvalidOperationException(
                        "Vigilia requires the approved navigation sprite: "+symbol);
                var root=Rect(navLabels[i],buttonRow);
                var touch=root.gameObject.AddComponent<Image>();
                touch.color=new Color(0f,0f,0f,0f);
                touch.raycastTarget=true;
                var button=Button(root,()=>Navigate(index));
                button.targetGraphic=touch;
                button.transition=Selectable.Transition.None;
                var art=Rect("Single owner-approved "+symbol+" relief",root);
                Fill(art,6f);
                var surface=Plate(art,BreachWatchPlate.PlateType.Navigation);
                surface.ApplyIndependentArt(painted);
                surface.color=new Color(.97f,.95f,.90f,1f);
                surface.VisualScale=1f;
                surface.raycastTarget=false;
                UseSourceArtInteraction(button,surface);
                navButtons[i]=button;
                navPlates[i]=surface;
            }
            feedbackPlate=Rect("Temporary real action requirement tablet",uiRoot);
            var feedbackStone=Plate(feedbackPlate,BreachWatchPlate.PlateType.Resource);
            var authoredStone=Resources.Load<Sprite>("VigiliaArt/OwnerControls/resource_capsule");
            if(authoredStone)feedbackStone.ApplyIndependentArt(authoredStone,true);
            feedbackStone.raycastTarget=false;
            feedback=Label(feedbackPlate,"Action feedback / live or disabled",
                15,TextAnchor.MiddleCenter,TextIvory);
            Fill(feedback.rectTransform,5);
            feedbackPlate.gameObject.SetActive(false);
            feedback.text="";
        }
        void SetFeedback(string value)
        {
            if(feedback)feedback.text=value??"";
            bool show=!string.IsNullOrWhiteSpace(value);
            if(feedbackPlate)feedbackPlate.gameObject.SetActive(show);
            feedbackUntil=show?Time.unscaledTime+4.0f:0f;
        }
        public void ExplainUnavailableAction()
        {
            var b=snapshot?.Building;
            if(b==null || actionButton.interactable)return;
            string reason=b.Action?.Reason??"";
            if(string.IsNullOrWhiteSpace(reason))
                reason=b.OperationActive
                    ? (host.English?"Operation in progress":"Operación en curso")
                    : (host.English?"Action unavailable":"Acción no disponible");
            // Remove formatting breaks only, never invent different
            // mechanics, costs or requirements.
            SetFeedback(reason.Replace("\r"," ").Replace("\n"," "));
        }
        void OnCanonicalSelection(string id)
        {
            selected=id??"";
            SetFeedback("");
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
            SetFeedback("");
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
        // Do not expose an icon before its REAL gameplay destination exists.
        // Army/Heroes art remains authored but dormant in this first-screen
        // proof; World becomes visible only on canonical unlock.
        int NavigationMask()
        {
            int mask=(1<<0)|(1<<1);
            if(host && host.CanOpen(RoyalSection.World))mask|=1<<3;
            return mask;
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
            if(feedbackUntil>0f && Time.unscaledTime>=feedbackUntil)
                SetFeedback("");
            // When the canonical world map returns to city it resets the
            // city camera. Restore the approved playable Bastion/bridge
            // composition exactly once, without stealing pan/zoom afterward.
            if(lastWorldMode && !host.InWorld && cityComposition)
                cityComposition.OnCityReentered(Screen.width,Screen.height);
            lastWorldMode=host.InWorld;
            SyncLiveCamera();
            if(lastWidth!=Screen.width||lastHeight!=Screen.height||
               lastSafeArea!=Screen.safeArea)
                Reflow(Screen.width,Screen.height);
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
            // On real phones, notches, system gestures and immersive mode
            // can change the safe area without a size/orientation change.
            lastSafeArea=Screen.safeArea;

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
            int navigationMask=NavigationMask();
            int visibleNav=0;
            for(int i=0;i<navButtons.Length;i++)
                if((navigationMask & (1<<i))!=0)visibleNav++;
            int visibleResources=snapshot!=null && snapshot.ShowFood?3:2;
            // The owner's selected city view deliberately shows the WHOLE
            // road, bridge and Bastion instead of a giant castle closeup.
            // This belongs to the new UI only and never changes gameplay
            // presets, saves or the regional World camera.
            if(cityComposition)cityComposition.Layout(pixelWidth,pixelHeight);
            // The original 1.75 clamp made the entire interface tiny on
            // 1170/1440px Retina phone render targets, even though 900px
            // portrait screenshots looked right. Keep the approved 900px
            // design proportions across *physical* mobile resolutions.
            // The .91 floor preserves tappable 44px nav on 390px captures.
            float scale=portrait
                ?Mathf.Clamp(pixelWidth/514f,.91f,3.00f)
                :Mathf.Clamp(Mathf.Min(pixelWidth/1250f,pixelHeight/680f),.86f,1.30f);
            scaler.scaleFactor=scale;
            float w=pixelWidth/scale,h=pixelHeight/scale;
            float margin=portrait?13f:18f;

            // The owner specifically rejected the thick burgundy header and
            // the second huge title plate. A single small round realm crest,
            // compact counters and a free-standing Breach mark remain.
            if(portrait)
            {
                // The owner's reference is a lightweight TOP band, not
                // two tall rectangular headers. Compact at 900px, but
                // preserve comfortable actual phone lettering at 390px.
                bool smallPhone=pixelWidth<600;
                float identityHeight=smallPhone?52f:43f;
                At(identity,margin,smallPhone?11f:14f,182,identityHeight);
                float crestSize=smallPhone?45f:37f;
                // Engraved lion lives INSIDE the one painted player plaque:
                // no separate disc hanging over a second visible surface.
                At(identityCrest,4f,(identityHeight-crestSize)*.5f,crestSize,crestSize);
                At(identityName.rectTransform,crestSize+8f,smallPhone?6f:4f,
                    174f-crestSize,smallPhone?25f:21f);
                At(identityPower.rectTransform,crestSize+8f,
                    smallPhone?31f:25f,174f-crestSize,17f);
                identityName.fontSize=smallPhone?17:16;
                identityPower.fontSize=smallPhone?14:12;

                float rightSpace=w-(margin+182+9+margin);
                float chip=visibleResources==2
                    ?Mathf.Min(85f,Mathf.Max(72f,(rightSpace-4f)/2f))
                    :Mathf.Min(70f,Mathf.Max(58f,(rightSpace-8f)/3f));
                float resourcesWidth=visibleResources*chip+4f*(visibleResources-1);
                float chipHeight=smallPhone?38f:28f;
                float symbolSize=smallPhone?25f:19f;
                float symbolTop=(chipHeight-symbolSize)*.5f;
                At(resourceRow,w-margin-resourcesWidth,smallPhone?18f:17f,
                    resourcesWidth,chipHeight);
                for(int i=0;i<3;i++)
                {
                    resourceChips[i].gameObject.SetActive(i<visibleResources);
                    if(i>=visibleResources)continue;
                    At(resourceChips[i],i*(chip+4f),0,chip,chipHeight);
                    At(resourceIllustrations[i].rectTransform,
                        4f,symbolTop,symbolSize,symbolSize);
                    At(resourceAmounts[i].rectTransform,
                        symbolSize+8f,3f,chip-symbolSize-10f,chipHeight-6f);
                    resourceAmounts[i].fontSize=smallPhone?14:12;
                }

                At(centerMark,w*.52f-56,74,112,30);
                float missionWidth=Mathf.Min(185f,w*.46f);
                float missionTop=smallPhone?96f:82f;
                At(objective,margin,missionTop,missionWidth,
                    fullObjective?108f:78f);
                // Native 390px screenshot: the engraved chapter castle
                // ends at x=29, while title started at x=28. They touched,
                // making the mission feel like two pasted graphics. One
                // 2u breathing gap + font-safe width holds the real title.
                At(chapterName.rectTransform,31f,5f,missionWidth-45f,20f);
                At(chapterObjective.rectTransform,13f,24f,missionWidth-22f,
                    fullObjective?59f:31f);
                At(chapterProgress.rectTransform,13f,
                    fullObjective?83f:53f,missionWidth-22f,18f);
                chapterName.fontSize=smallPhone?13:12;
                chapterObjective.fontSize=smallPhone?15:14;
                chapterProgress.fontSize=smallPhone?14:13;

                // Secondary seals are disabled on this first screen:
                // keep their carved artwork discreet rather than occupying
                // three large touch footprints over the playable terrain.
                At(utilityRail,margin,h-306,42,138);
                for(int i=0;i<utilityRail.childCount;i++)
                {
                    var utility=(RectTransform)utilityRail.GetChild(i);
                    utility.gameObject.SetActive(false);
                    At(utility,0,i*45f,37,37);
                }

                float navSize=49,navGap=14;
                float navWidth=visibleNav*navSize+Mathf.Max(0,visibleNav-1)*navGap;
                float navTop=h-100f;
                At(buttonRow,(w-navWidth)*.5f,navTop,navWidth,64);
                navFiligree.Configure(navSize+navGap,navSize,visibleNav);
                int navColumn=0;
                for(int i=0;i<5;i++)
                {
                    bool visible=(navigationMask & (1<<i))!=0;
                    navButtons[i].gameObject.SetActive(visible);
                    if(visible)At((RectTransform)navButtons[i].transform,
                        navColumn++*(navSize+navGap),3,navSize,navSize);
                }

                // Approved portrait reference places the card across
                // roughly one third of the screen rather than nearly half.
                // Separate, fully readable live text fits in the compact
                // vertical stack; it is never flattened into one bitmap.
                // Match the approved 864×1536 premium reference: the card
                // occupies roughly one third of width and leaves more
                // unblocked river and city. Internal live text is rearranged
                // instead of baking it into an image.
                float pw=Mathf.Min(171f,w*.42f);
                float ph=303f;
                float left=w-margin-pw;
                float top=navTop-ph-45;
                At(panel,left,top,pw,ph);
                At(buildingName.rectTransform,51,7,pw-110,28);
                At(buildingLevel.rectTransform,51,36,pw-110,17);
                At(((RectTransform)closeButton.transform),pw-55,3,50,50);
                // This 90-unit live portrait well is at ~1.9:1 aspect
                // rather than the prior 2.4:1 slit. Together with the
                // separately rendered 1.6:1 texture, its center crop fits
                // all original Bastion parapets and gate without stretching
                // the real 3D mesh or enlarging the panel's width.
                At(portraitFrame,11,55,pw-22,83);
                At(buildingFunction.rectTransform,13,140,pw-26,42);
                descriptionDivider.gameObject.SetActive(true);
                At(descriptionDivider,14,183,pw-28,8);
                At(costHeader.rectTransform,13,191,pw-26,15);
                At(buildingCosts.rectTransform,13,208,pw-26,24);
                At(costIcons[0].rectTransform,14,208,23,23);
                At(costQuantities[0].rectTransform,41,208,43,24);
                At(costIcons[1].rectTransform,90,208,23,23);
                At(costQuantities[1].rectTransform,116,208,pw-129,24);
                At(buildingState.rectTransform,13,232,pw-26,18);
                At(((RectTransform)actionButton.transform),12,ph-53,pw-24,50);
                buildingName.fontSize=19;
                buildingLevel.fontSize=14;
                buildingFunction.fontSize=15;
                buildingCosts.fontSize=14;
                buildingState.fontSize=14;
                costHeader.fontSize=13;
                actionLabel.fontSize=16;

                At(feedbackPlate,margin,h-160,Mathf.Max(120,left-margin-15),36);
                Fill(feedback.rectTransform,6);
                feedback.fontSize=13;
            }
            else
            {
                At(identity,margin,11,234,58);
                At(identityCrest,4,5,48,48);
                At(identityName.rectTransform,59,6,166,31);
                At(identityPower.rectTransform,59,38,165,17);
                identityName.fontSize=20;
                identityPower.fontSize=14;

                float chip=visibleResources==2
                    ?Mathf.Min(110f,Mathf.Max(82f,(w*.28f-7f)/2f))
                    :Mathf.Min(100f,Mathf.Max(82f,(w*.28f-12f)/3f));
                float resourcesWidth=visibleResources*chip+7f*(visibleResources-1);
                At(resourceRow,w-margin-resourcesWidth,16,resourcesWidth,43);
                for(int i=0;i<3;i++)
                {
                    resourceChips[i].gameObject.SetActive(i<visibleResources);
                    if(i>=visibleResources)continue;
                    At(resourceChips[i],i*(chip+7),0,chip,41);
                    At(resourceIllustrations[i].rectTransform,5,5,30,30);
                    At(resourceAmounts[i].rectTransform,38,5,chip-41,30);
                    resourceAmounts[i].fontSize=16;
                }
                At(centerMark,w*.49f-60,18,120,28);
                // On the real 844x390 Unity capture, the previous 61+19
                // progress position left only 2.6 physical pixels above
                // the bottom bevel. Keep true text visibly inside the stone.
                At(objective,margin,102,225,fullObjective?118:83);
                At(chapterName.rectTransform,37,8,178,20);
                At(chapterObjective.rectTransform,14,31,197,fullObjective?57:24);
                At(chapterProgress.rectTransform,14,fullObjective?92:57,190,19);
                chapterName.fontSize=14;
                chapterObjective.fontSize=16;
                chapterProgress.fontSize=15;

                At(utilityRail,margin,h-165,44,145);
                for(int i=0;i<utilityRail.childCount;i++)
                {
                    var utility=(RectTransform)utilityRail.GetChild(i);
                    utility.gameObject.SetActive(false);
                    At(utility,0,i*44f,37,37);
                }

                float navSize=55,navGap=15;
                float navWidth=visibleNav*navSize+Mathf.Max(0,visibleNav-1)*navGap;
                float navTop=h-78;
                At(buttonRow,(w-navWidth)*.5f,navTop,navWidth,69);
                navFiligree.Configure(navSize+navGap,navSize,visibleNav);
                int navColumn=0;
                for(int i=0;i<5;i++)
                {
                    bool visible=(navigationMask & (1<<i))!=0;
                    navButtons[i].gameObject.SetActive(visible);
                    if(visible)At((RectTransform)navButtons[i].transform,
                        navColumn++*(navSize+navGap),3,navSize,navSize);
                }

                // Short landscape currently compresses the genuine 3-line
                // Bastion description into an 86u column beside the 3D
                // miniature. Add only 24u on short screens: this preserves
                // over 75% of the actual playable city while giving the
                // narrative, full authentic action and resources room.
                float pw=h<530f?234f:220f;
                float ph=Mathf.Min(330,h-174f);
                float top=h<530f?75f:118f;
                At(panel,w-margin-pw,top,pw,ph);
                At(buildingName.rectTransform,58,8,pw-119,31);
                At(buildingLevel.rectTransform,58,40,pw-119,17);
                At(((RectTransform)closeButton.transform),pw-57,3,52,52);
                buildingName.fontSize=21;
                buildingLevel.fontSize=15;
                if(ph<305)
                {
                    // Tight landscape phones: separate image and text columns
                    // followed by an independent full-width action row.
                    At(portraitFrame,12,60,95,83);
                    At(buildingFunction.rectTransform,114,62,pw-124,76);
                    descriptionDivider.gameObject.SetActive(false);
                    At(costHeader.rectTransform,12,141,pw-24,20);
                    At(buildingCosts.rectTransform,12,166,pw-24,24);
                    At(costIcons[0].rectTransform,14,165,26,26);
                    At(costQuantities[0].rectTransform,43,165,52,26);
                    At(costIcons[1].rectTransform,112,165,26,26);
                    At(costQuantities[1].rectTransform,140,165,pw-151,26);
                    At(buildingState.rectTransform,12,197,pw-24,20);
                    At(((RectTransform)actionButton.transform),12,ph-56,pw-24,53);
                    buildingFunction.fontSize=14;
                    buildingCosts.fontSize=14;
                    buildingState.fontSize=14;
                    // Native 844x390 screenshot exposed an 11.18px
                    // COSTE DE LA MEJORA caption at 13*.86. Keep readable
                    // type in the *real* small-landscape viewport.
                    costHeader.fontSize=14;
                    actionLabel.fontSize=15;
                }
                else
                {
                    // Real 1600x900 screenshot had an oversized dark void
                    // between the portrait and resource costs. Compact this
                    // as an intentional illustrated information hierarchy.
                    At(portraitFrame,12,61,pw-24,93);
                    At(buildingFunction.rectTransform,13,157,pw-26,40);
                    descriptionDivider.gameObject.SetActive(true);
                    At(descriptionDivider,14,197,pw-28,9);
                    At(costHeader.rectTransform,13,204,pw-26,19);
                    At(buildingCosts.rectTransform,13,227,pw-26,30);
                    At(costIcons[0].rectTransform,17,227,30,30);
                    At(costQuantities[0].rectTransform,52,227,53,30);
                    At(costIcons[1].rectTransform,121,227,30,30);
                    At(costQuantities[1].rectTransform,153,227,pw-162,30);
                    At(buildingState.rectTransform,13,261,pw-26,19);
                    At(((RectTransform)actionButton.transform),12,ph-48,pw-24,45);
                    buildingFunction.fontSize=15;
                    buildingCosts.fontSize=15;
                    buildingState.fontSize=14;
                    actionLabel.fontSize=16;
                }
                At(feedbackPlate,margin,h-124,
                    Mathf.Max(120,w-pw-3*margin),35);
                Fill(feedback.rectTransform,6);
                feedback.fontSize=14;
            }

            // A real set of crossed hammers is engraved at the LEFT of the
            // authored burgundy artwork. Keep the live Cormorant/Baskerville
            // caption strictly to its right: earlier native screenshots showed
            // the hammers cutting through "MEJORAR" despite a technically
            // valid clickable Button.
            if(actionLabel && actionButton)
            {
                var plate=(RectTransform)actionButton.transform;
                // One original source sprite contains the left-side
                // hammered ornament. Leave its width entirely clear for
                // the dynamic upgrade label, no procedural icon overlay.
                var illustration=(RectTransform)actionPlate.transform;
                // Render the owner's SINGLE source 246x60 burgundy asset
                // at its exact natural aspect while filling the button's
                // available WIDTH. A constant 36u height caused letterboxing
                // on large landscape screens: the word "II" escaped the
                // PAINTED red geometry even when all touch and text-Rect
                // bounds technically passed. Source-image proportions are
                // never distorted, and no second hammer layer is introduced.
                float artAspect=actionPlate.sprite
                    ?actionPlate.sprite.rect.width/
                        Mathf.Max(1f,actionPlate.sprite.rect.height)
                    :246f/60f;
                float paintedHeight=plate.rect.width/Mathf.Max(1f,artAspect);
                float paintedTop=(plate.rect.height-paintedHeight)*.5f;
                At(illustration,0f,paintedTop,plate.rect.width,paintedHeight);
                // The hammered engraving lives inside the first quarter of
                // the ORIGINAL painting, not the invisible wider hit target.
                // 25% left and 7u right clearances keep the full genuine
                // Spanish chapter action visible on every orientation.
                float captionLeft=plate.rect.width*.25f;
                float captionRight=plate.rect.width-7f;
                At(actionLabel.rectTransform,captionLeft,
                    paintedTop+3f,
                    Mathf.Max(20f,captionRight-captionLeft),
                    Mathf.Max(18f,paintedHeight-6f));
            }

            if(closePainted && closeButton)
            {
                var full=(RectTransform)closeButton.transform;
                float disk=Mathf.Min(30f,full.rect.width-16f);
                At(closePainted,(full.rect.width-disk)*.5f,
                    (full.rect.height-disk)*.5f,disk,disk);
            }

            // Render only the original selected building in its native
            // illustration well. Reproject the physical camera texture without
            // stretching stone, gold trim or actual building geometry.
            if(portraitView && portraitFrame)
            {
                float availableWidth=Mathf.Max(1f,portraitFrame.rect.width-10f);
                float availableHeight=Mathf.Max(1f,portraitFrame.rect.height-10f);
                float ratio=availableWidth/availableHeight;
                // Preserve the *actual* aspect of the separately rendered
                // 3D camera. Our earlier 2:1 constant both shrank the castle
                // and silently cropped towers on widescreen UI cards.
                float renderRatio=portraitView.texture
                    ?portraitView.texture.width/(float)portraitView.texture.height
                    :1.60f;
                if(ratio>2.0f)
                {
                    // Wide landscape well: letterbox a natural 1.6:1 image
                    // against the premium dark brass frame rather than
                    // cropping the 3D ramparts out of the live photograph.
                    float fittedWidth=availableHeight*renderRatio;
                    float inset=(portraitFrame.rect.width-fittedWidth)*.5f;
                    At(portraitView.rectTransform,inset,5f,
                        fittedWidth,availableHeight);
                    portraitView.uvRect=new Rect(0,0,1,1);
                }
                else
                {
                    Fill(portraitView.rectTransform,5f);
                    if(ratio<=renderRatio)
                    {
                        float uvW=ratio/renderRatio;
                        portraitView.uvRect=new Rect((1f-uvW)*.5f,0,uvW,1);
                    }
                    else
                    {
                        float uvH=renderRatio/ratio;
                        portraitView.uvRect=new Rect(0,(1f-uvH)*.5f,1,uvH);
                    }
                }
            }
            lastPortrait=portrait;
            lastNavigationMask=navigationMask;
            lastResourceCount=visibleResources;
            Canvas.ForceUpdateCanvases();
            CropPortraitToTrueGeometry();
        }

        // The real 3D building occupies only a portion of its isolated
        // RenderTexture. Fit its documented model silhouette into the
        // available viewport instead of photographing a tiny building
        // surrounded by vast empty stage. A padded equal-aspect UV crop
        // preserves authentic stone proportions and NEVER changes the
        // playable city's meshes or main camera.
        void CropPortraitToTrueGeometry()
        {
            if(!portraitView || !portraitView.texture || !buildingPortrait)return;
            var bounds=buildingPortrait.LastGeometryUvBounds;
            if(bounds.width<.02f || bounds.height<.02f)return;
            var shown=portraitView.rectTransform.rect;
            if(shown.width<5f || shown.height<5f)return;
            float textureRatio=portraitView.texture.width/
                (float)portraitView.texture.height;
            float uvRatio=(shown.width/shown.height)/textureRatio;
            uvRatio=Mathf.Clamp(uvRatio,.45f,2.8f);
            float wantedWidth=bounds.width+.075f;
            float wantedHeight=bounds.height+.07f;
            float cropHeight=Mathf.Max(wantedHeight,wantedWidth/uvRatio);
            float cropWidth=cropHeight*uvRatio;
            if(cropWidth>1f)
            {
                cropWidth=1f;
                cropHeight=cropWidth/uvRatio;
            }
            if(cropHeight>1f)
            {
                cropHeight=1f;
                cropWidth=Mathf.Min(1f,cropHeight*uvRatio);
            }
            var center=bounds.center;
            float x=Mathf.Clamp(center.x-cropWidth*.5f,0f,1f-cropWidth);
            float y=Mathf.Clamp(center.y-cropHeight*.5f,0f,1f-cropHeight);
            portraitView.uvRect=new Rect(x,y,cropWidth,cropHeight);
        }

        public void RenderNow()
        {
            if(!created||!host)return;
            try
            {
                snapshot=host.Read(selected,host.English);
                // Re-layout only at a genuine unlock boundary. Do not leave a
                // grey ghost medallion or empty food capsule on the first screen.
                if(lastNavigationMask!=NavigationMask() ||
                   lastResourceCount!=(snapshot.ShowFood?3:2))
                    Reflow(lastWidth>0?lastWidth:Screen.width,
                           lastHeight>0?lastHeight:Screen.height);
                var realName=host.City.Gateway.Snapshot().PlayerName;
                identityName.text=string.IsNullOrWhiteSpace(realName)?"VALORIA":realName;
                identityPower.text="PODER  "+RoyalFormatting.Amount(snapshot.Power);
                // The ornate objective tablet already bears the Breach seal.
                // A 20-character subtitle broke into two lines on real
                // 390px phones with Libre Baskerville; use true chapter only.
                // Chapter I's story identity is the Breach. Display its
                // compact subtitle without falsely altering the real chapter
                // number, objective or progress supplied by the game gateway.
                // Later bastions retain their real chapter string unchanged.
                var realChapter=snapshot.Chapter??"";
                bool chapterOne=host.City.Gateway.Snapshot().BastionLevel==1;
                chapterName.text=chapterOne && realChapter.Length>0
                    ? realChapter+(host.English?" / THE BREACH":" / LA BRECHA")
                    : realChapter;
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
                    {
                        buildingPortrait.Show(ActualBuildingModel(b.Id),host.PlayerCamera,
                            b.Id+"/"+b.Level);
                        CropPortraitToTrueGeometry();
                    }
                    buildingFunction.text=(b.Id=="bastion" && b.Level==1)
                        ? (host.English?"Valoria's government and progress."
                            :"Gobierno y progreso de Valoria.")
                        : b.Function??"";
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
                    // At real 390px widths a 21-character command runs into
                    // the gilded corners. Display a truthful short action
                    // name in this ONE compact context; the actual command,
                    // target, affordability and confirmation are unchanged.
                    // Real Unity review: the full long-action label ran beyond
                    // the carved CTA on a 1600x900 capture. The command and
                    // confirmation remain IDENTICAL; only its compact visual
                    // caption is compact enough for an honest single line at every viewport.
                    string actionCaption=b.Action==null
                        ?(host.English?"NO ACTION":"Sin acción")
                        :b.Action.Label;
                    if(b.Action!=null && b.Id=="bastion" && b.Level==1 &&
                       b.Action.Kind=="AdvanceBastion")
                    {
                        // Full title only where the ORIGINAL 246x60 picture
                        // has enough real horizontal room. Narrow landscape
                        // retains the authentic destination but avoids tiny
                        // text beyond the painted gilded edge.
                        bool fullUpgradeCaption=lastWidth>=800;
                        actionCaption=host.English
                            ?(fullUpgradeCaption?"Upgrade to Bastion II":"Upgrade II")
                            :(fullUpgradeCaption?"Mejorar a Bastión II":"Mejorar a II");
                    }
                    actionLabel.text=actionCaption;
                    actionLabel.resizeTextForBestFit=true;
                    actionLabel.resizeTextMinSize=11;
                    actionLabel.resizeTextMaxSize=lastWidth<1000?16:17;
                    actionLabel.horizontalOverflow=HorizontalWrapMode.Wrap;
                    actionLabel.verticalOverflow=VerticalWrapMode.Truncate;
                    actionLabel.color=can?TextIvory:new Color(.91f,.85f,.72f,1f);
                }
                // Accurate labels, artwork and routes: realm/castle, resource/sack,
                // army/swords, world/raven and heroes/helm. Only the genuine
                // first-screen routes are enabled; we never dispatch old panels.
                int activeTab=Mathf.Clamp(host.InWorld?3:selectedTab,0,navButtons.Length-1);
                for(int i=0;i<5;i++)
                {
                    navPlates[i].Type=i==(host.InWorld?3:selectedTab)
                        ?BreachWatchPlate.PlateType.Selected
                        :BreachWatchPlate.PlateType.Navigation;
                    bool allowed=(NavigationMask() & (1<<i))!=0;
                    navButtons[i].interactable=allowed;
                    // The single painted medallion is already the whole icon.
                    // An unavailable destination is absent, not a tappable or
                    // faded promise of a screen that does not yet exist.
                    var p=navPlates[i];
                    if(p)p.color=allowed
                        ? i==(host.InWorld?3:selectedTab)
                            ?new Color(1f,.97f,.87f,1f):Color.white
                        :new Color(.70f,.68f,.65f,.88f);
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