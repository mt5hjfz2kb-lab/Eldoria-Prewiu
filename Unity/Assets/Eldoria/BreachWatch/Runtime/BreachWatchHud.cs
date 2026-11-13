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
    // with exclusive Vigilia artwork in production. Canonical gameplay
    // controllers and their live input bindings remain authoritative.
    [DisallowMultipleComponent]
    public sealed class BreachWatchHud : MonoBehaviour
    {
        const string RootName = "VIGILIA / independent canvas";
        static readonly Color TextIvory = new Color(.98f,.94f,.84f,1f);
        static readonly Color Muted = new Color(.88f,.85f,.79f,1f);
        static readonly Color Copper = new Color(.92f,.70f,.40f,1f);
        static readonly Color Locked = new Color(.38f,.39f,.39f,1f);
        ValoriaIntegratedWorld host;
        Canvas canvas;
        BreachWatchDestinationBridge destinationBridge;
        BreachWatchMailboxBridge mailboxBridge;
        BreachWatchModalBridge modalBridge;
        BreachWatchHeroesBridge heroesBridge;
        BreachWatchRetentionBridge retentionBridge;
        BreachWatchResourceBagBridge resourceBagBridge;
        CanvasScaler scaler;
        GraphicRaycaster raycaster;
        Canvas legacyCanvas;
        bool productionPresentation;
        public bool ProductionPresentation => productionPresentation;
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
        int buildingNameMaxFont=18,buildingNameMinFont=16;
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
        Material singleCrestPaletteMaterial;
        BreachWatchNavFiligree navFiligree;
        readonly BreachWatchPlate[] approvedUtilitySeals = new BreachWatchPlate[3];
        readonly Button[] navButtons = new Button[5];
        Button mailUtilityButton;
        readonly BreachWatchPlate[] navPlates = new BreachWatchPlate[5];
        readonly Text[] navCaptions = new Text[5];
        readonly Outline[] navActiveOutlines = new Outline[5];
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
        // Staged bridge: management routes remain hidden until native
        // destination proof is accepted. No production-screen takeover.
        public BreachWatchDestinationBridge RealDestinations => destinationBridge;
        public BreachWatchMailboxBridge RealMailbox => mailboxBridge;
        public Button ActualMailboxEntry => mailUtilityButton;
        public BreachWatchHeroesBridge RealHeroes => heroesBridge;
        public BreachWatchRetentionBridge RealRetention => retentionBridge;
        public bool CanonicalModalVisibleOnIndependentCanvas => modalBridge &&
            modalBridge.Installed;
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
                    tablet.sprite.name=="mission" &&
                    tablet.type==Image.Type.Sliced &&
                    !tablet.preserveAspect;
            }
        }
        public bool HasSinglePremiumIdentity =>
            identity && identity.GetComponent<BreachWatchPlate>() &&
            identity.GetComponent<BreachWatchPlate>().sprite &&
            identity.GetComponent<BreachWatchPlate>().sprite.name=="profile" &&
            identity.GetComponent<BreachWatchPlate>().type==Image.Type.Sliced &&
            // The original painted profile already contains shading.
            // Do not require a second uGUI Shadow after the approved
            // single-surface unification.
            !identity.GetComponent<Shadow>() &&
            identity.GetComponentsInChildren<Graphic>(true).Length==3;

        public bool HasPremiumResourceSurfaces
        {
            get
            {
                for(int i=0;i<resourceChips.Length;i++)
                {
                    if(!resourceChips[i])return false;
                    var surface=resourceChips[i].GetComponent<BreachWatchPlate>();
                    if(!surface || !surface.sprite ||
                       surface.sprite.name!="resource" ||
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
            identity && !identityCrest &&
            identity.GetComponent<BreachWatchPlate>() &&
            identity.GetComponent<BreachWatchPlate>().sprite ==
                Resources.Load<Sprite>("VigiliaArt/CleanRoom/profile");
        public bool UsesOriginalPaintedMedallions =>
            navPlates[0] && navPlates[0].sprite &&
            navPlates[0].sprite.name=="nav_castle";
        public bool UsesPremiumSculptedMedallions
        {
            get
            {
                for(int i=0;i<navPlates.Length;i++)
                {
                    var requested=Resources.Load<Sprite>("VigiliaArt/CleanRoom/nav_"+ReferenceNavArt[i]);
                    if(!requested || !navPlates[i] || navPlates[i].sprite!=requested)
                        return false;
                }
                // Navigation uses five sculpted medallions, while profile
                // identity is the separately approved golden-lion crest.
                // The newly authored player name plate already includes
                // its own lion crest and golden ring, as ONE image.
                return UsesCompactRealmCrest;
            }
        }
        public bool UsesApprovedReferenceMedallions
        {
            get
            {
                for(int i=0;i<navPlates.Length;i++)
                    if(!navPlates[i] || !navPlates[i].UsesOwnerReference ||
                       !navPlates[i].sprite ||
                       navPlates[i].sprite.name!="nav_"+ReferenceNavArt[i])
                        return false;
                return true;
            }
        }
        // One new painted card, no older decorative layers underneath it.
        public bool UsesPremiumForgedPortraitFrame =>
            portraitFrame && !portraitFrame.GetComponent<Graphic>() &&
            portraitView && portraitView.transform.IsChildOf(portraitFrame);
        public bool UsesExactOwnerApprovedGoldBorder =>
            !exactOwnerGoldBorder && panel && panel.GetComponent<Image>() &&
            panel.GetComponent<Image>().sprite ==
                Resources.Load<Sprite>("VigiliaArt/CleanRoom/panel");
        public bool UsesSinglePremiumContext =>
            panel && panel.GetComponent<Image>() &&
            panel.GetComponent<Image>().sprite ==
                Resources.Load<Sprite>("VigiliaArt/CleanRoom/panel") &&
            panel.GetComponent<Image>().type==Image.Type.Simple &&
            !panel.GetComponent<BreachWatchPanelInk>() &&
            !panel.GetComponent<BreachWatchPlate>() &&
            UsesExactOwnerApprovedGoldBorder;
        public bool UsesPremiumBurgundyAction =>
            actionPlate && actionPlate.sprite &&
            actionPlate.sprite.name=="upgrade" &&
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
            if(BreachWatchProductionPresentation.Required)
                hud.EnableProductionPresentation();
            return hud;
        }

        public void EnableProductionPresentation()
        {
            if(productionPresentation)return;
            productionPresentation=true;
            var typography=GetComponent<BreachWatchTypography>();
            if(!typography)typography=gameObject.AddComponent<BreachWatchTypography>();
            typography.AllDestinations=true;
            if(!typography.SetCandidate(true))
                throw new InvalidOperationException("Production Vigilia font set missing");
            gameObject.AddComponent<BreachWatchProductionPresentation>().Initialize(legacyCanvas);
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
            // Stable 2K sprite-border conversion: Image divides sprite PPU
            // by the Canvas reference PPU when generating sliced geometry.
            scaler.referencePixelsPerUnit=100f;
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
            destinationBridge=gameObject.AddComponent<BreachWatchDestinationBridge>();
            destinationBridge.Initialize(host,canvas);
            mailboxBridge=gameObject.AddComponent<BreachWatchMailboxBridge>();
            mailboxBridge.Initialize(host,canvas);
            heroesBridge=gameObject.AddComponent<BreachWatchHeroesBridge>();
            heroesBridge.Initialize(host,canvas,destinationBridge);
            retentionBridge=gameObject.AddComponent<BreachWatchRetentionBridge>();
            retentionBridge.Initialize(host,canvas);
            // The legacy Canvas is disabled, but actual story/confirmation
            // callbacks still live on its canonical ModalWindow. Move and
            // restyle that ONE modal so it cannot become invisible in Vigilia.
            modalBridge=gameObject.AddComponent<BreachWatchModalBridge>();
            if(!modalBridge.Initialize(host,canvas))
                throw new InvalidOperationException(
                    "Canonical narrative/modal bridge missing for independent HUD");
            resourceBagBridge=gameObject.AddComponent<BreachWatchResourceBagBridge>();
            resourceBagBridge.Initialize(host,canvas,resourceChips,mailUtilityButton);
            created=true;Reflow(Screen.width,Screen.height);RenderNow();
            // Explicit candidate only; the ordinary UI keeps its exact fonts.
            if(BreachWatchTypography.IsOptedIn(UnityEngine.Application.absoluteURL))
                canvas.gameObject.AddComponent<BreachWatchTypography>().SetCandidate(true);
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
                name=="Real building level" ||
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
            hitSurface.SetPointerInput(true);
            var b=Button(rt,action);
            b.targetGraphic=hitSurface;
            var glyph=Glyph(rt,"Carved sigil",icon);
            Fill((RectTransform)glyph.transform,10);
            return rt;
        }
        void BuildIndependentComponents()
        {
            // Rebuild starts on a clear Canvas, without an atmospheric overlay.
            // Asymmetrical stone architecture instead of a header/footer re-skin.
            identity=Rect("Identity / basalt crest",uiRoot);
            // REPLACEMENT, not a bevel pasted over another panel: one
            // authoritative forged-metal plate from the approved art kit.
            var identitySurface=Plate(identity,BreachWatchPlate.PlateType.Basalt);
            var finalProfile=Resources.Load<Sprite>("VigiliaArt/CleanRoom/profile");
            if(!finalProfile)throw new InvalidOperationException(
                "Missing Vigilia's approved player profile art");
            identitySurface.ApplyIndependentArt(finalProfile,true);
            // A whole-image Image.color tint failed real native QA in
            // run 42954979987. Keep the original source sprite and white
            // UI vertex colors; only the painted violet pixels of its single
            // 2K image may be selectively shifted toward antique bronze.
            // One source Image, no added border/crest/control behind it.
            var crestShader=Resources.Load<Shader>(
                "VigiliaArt/CleanRoom/VigiliaAntiqueCrest");
            if(!crestShader || !crestShader.isSupported)
                throw new InvalidOperationException(
                    "Missing or unsupported original-image bronze crest shader");
            singleCrestPaletteMaterial=new Material(crestShader);
            singleCrestPaletteMaterial.name="Vigilia / one painted crest, local palette";
            singleCrestPaletteMaterial.hideFlags=HideFlags.DontSave;
            identitySurface.material=singleCrestPaletteMaterial;
            identitySurface.color=Color.white;
            // The entire visible account plate must prevent accidental
            // click-through to the city, without pretending to navigate.
            identitySurface.raycastTarget=true;
            // No additional uGUI Shadow: the genuine profile painting already
            // owns its falloff. Doubling it made the entire header float.
            // The forged lion medallion is baked into the independently
            // illustrated profile sprite; no second decorative child exists.
            identityCrest=null;
            identityName=Label(identity,"Player name",22,TextAnchor.MiddleLeft,TextIvory,true);
            identityPower=Label(identity,"Realm power",16,TextAnchor.MiddleLeft,Copper);
            // Keep actual gameplay identity ink fully inside a single
            // painted plate at 390 and 900px. The previous middle-aligned
            // second line hung BELOW its source rim. Overflow here prevents
            // false zero-glyph truncation but native pixel QA still applies.
            identityName.verticalOverflow=VerticalWrapMode.Overflow;
            identityPower.verticalOverflow=VerticalWrapMode.Overflow;
            resourceRow=Rect("Separate resource stones",uiRoot);
            var originalResourceStone=Resources.Load<Sprite>(
                "VigiliaArt/CleanRoom/resource");
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
                resourceSurface.color=Color.white;

                resourceSurface.raycastTarget=true;
                var picture=Rect("True illustrated resource",resourceChips[i]);
                resourceIllustrations[i]=picture.gameObject.AddComponent<Image>();
                resourceIllustrations[i].sprite=Resources.Load<Sprite>(
                    "VigiliaArt/FreshArt/"+(i==0?"wood":i==1?"stone":"food"));
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
                "VigiliaArt/CleanRoom/mission");
            if(!angledPlate)throw new InvalidOperationException(
                "Missing Vigilia's approved integral chapter tablet");
            // The actual tapered ornament is a single authored silhouette:
            // never silently fall back to a generic procedural dark panel.
            chapterStone.ApplyIndependentArt(angledPlate,true);
            chapterStone.color=Color.white;
            chapterStone.preserveAspect=false;

            // No extra shadow: the carved quest PNG already has a baked
            // shadow and finished outline. This is one plate, not two.
            // The approved stone already has an engraved ornamental
            // silhouette. Do not paste a bright vector tower ON TOP of it:
            // the live chapter title itself identifies the real quest.
            chapterName=Label(objective,"Actual chapter",15,TextAnchor.UpperLeft,Copper,true);
            chapterName.lineSpacing=.93f;
            chapterObjective=Label(objective,"Actual objective",17,TextAnchor.UpperLeft,TextIvory);
            chapterProgress=Label(objective,"Objective progress",14,TextAnchor.UpperLeft,Muted);
            // Genuine Unity 6000.3.23f1 run 42823264943 proved that the
            // new art's 12-unit objective row rendered ZERO glyphs with
            // default VerticalWrapMode.Truncate, despite source-only font
            // width checks passing. The fraction needed the same repair.
            // Overflow draws real glyphs without deleting or falsifying a
            // quest row; native GPU and line-count verification still enforce
            // one legible line and no overlap with the next real row.
            chapterObjective.verticalOverflow=VerticalWrapMode.Overflow;
            chapterProgress.verticalOverflow=VerticalWrapMode.Overflow;
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
                var seal=BuildButton(utilityRail,design.name,design.icon,()=>{
                    if(design.art=="bell")OpenActualMailbox();
                });
                var disabled=seal.GetComponent<Button>();
                disabled.interactable=false; // until this REAL source unlocks
                disabled.transition=Selectable.Transition.None;
                if(design.art=="bell")
                {
                    mailUtilityButton=disabled;
                    seal.name="CORREO / real introduced mailbox";
                }
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
            // Rebuilt from a blank canvas: one complete ORIGINAL 2K card.
            // All previous gold borders, slate slabs and inset frames are gone.
            var finishedPanel=Resources.Load<Sprite>("VigiliaArt/CleanRoom/panel");
            if(!finishedPanel)throw new InvalidOperationException("Missing original 2K selected-building panel");
            var singleSlate=panel.gameObject.AddComponent<Image>();
            singleSlate.sprite=finishedPanel;
            singleSlate.color=Color.white;
            singleSlate.type=Image.Type.Simple;
            singleSlate.preserveAspect=false;
            singleSlate.raycastTarget=true;
            exactOwnerGoldBorder=null;
            portraitFrame=Rect("One clean real building diorama viewport",panel);
            var liveView=Rect("Live 3D building portrait / no old HUD sprites",portraitFrame);
            Fill(liveView,5);
            portraitView=liveView.gameObject.AddComponent<RawImage>();
            portraitView.color=Color.white;
            portraitView.raycastTarget=false;
            buildingPortrait=gameObject.AddComponent<BreachWatchBuildingPortrait>();
            buildingPortrait.Initialize(portraitView);
            // The actual stone card has its own sculpted source contour.
            // A separate vector castle floated over the metal and the
            // live title redundantly said the same thing twice. Remove it;
            // reserve a single calm typographic header within the stone.
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
                image.sprite=Resources.Load<Sprite>("VigiliaArt/FreshArt/"+(i==0?"wood":"stone"));
                image.type=Image.Type.Simple;
                image.preserveAspect=true;
                image.raycastTarget=false;
                costIcons[i]=image;
                costQuantities[i]=Label(panel,i==0?"Actual wood cost":"Actual stone cost",16,
                    TextAnchor.MiddleLeft,TextIvory,true);
            }
            buildingState=Label(panel,"Real state and timer",15,TextAnchor.UpperLeft,Muted);
            // Never make the actual timer disappear because a tighter source
            // plate has a line box smaller than Legacy Text line metrics.
            buildingState.verticalOverflow=VerticalWrapMode.Overflow;
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
            var originalArt=Resources.Load<Sprite>("VigiliaArt/CleanRoom/upgrade");
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
                    "VigiliaArt/CleanRoom/nav_"+symbol);
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
                surface.color=Color.white;
                // True Unity 900x1600 GPU A/B cf73b49: source at .70
                // painted only ~45px while the approved 900px owner reference
                // is ~62-68px per medallion. Raise the ACTUAL independent
                // sculpted artwork (not click/hit bounds and not unused
                // future systems) inside the same 52u nav anchors. The
                // 6u edge inset and .86 fill yield ~60px painted diameter.
                surface.VisualScale=.86f;
                surface.raycastTarget=false;
                UseSourceArtInteraction(button,surface);
                var glow=surface.gameObject.AddComponent<Outline>();
                glow.effectColor=new Color(.98f,.78f,.41f,.48f);
                glow.effectDistance=new Vector2(1.35f,-1.35f);
                glow.useGraphicAlpha=true;
                glow.enabled=false;
                navActiveOutlines[i]=glow;
                var caption=Label(root,"Navigation / "+navLabels[i],13,
                    TextAnchor.MiddleCenter,TextIvory,true);
                caption.resizeTextForBestFit=false;
                navCaptions[i]=caption;
                navButtons[i]=button;
                navPlates[i]=surface;
            }
            feedbackPlate=Rect("Temporary real action requirement tablet",uiRoot);
            var feedbackStone=Plate(feedbackPlate,BreachWatchPlate.PlateType.Resource);
            var authoredStone=Resources.Load<Sprite>("VigiliaArt/CleanRoom/resource");
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
        // The two original city buttons always exist. Each other gilded
        // medallion appears ONLY at its authoritative narrative unlock.
        // Routes open the ONE canonical destination reparented into this
        // Canvas, never the older blue screen and never a fake placeholder.
        int NavigationMask()
        {
            int mask=(1<<0)|(1<<1);
            if(!host)return mask;
            if(destinationBridge && host.CanOpen(RoyalSection.Army) &&
               Eldoria.Domain.NarrativeCoherenceRules.Seen(host.City.Gateway.Snapshot(),"march-preparation"))
                mask|=1<<2;
            if(host.CanOpen(RoyalSection.World))
                mask|=1<<3;
            var hero=host.Hud
                ?host.Hud.GetComponent<Eldoria.HeroesUI.HeroesApprovedDestination>()
                :null;
            if(heroesBridge && hero && hero.CanOpenDestination!=null &&
               hero.CanOpenDestination())
                mask|=1<<4;
            return mask;
        }
        bool CanShowCanonicalMailbox()
        {
            if(!host || !host.Ready || !host.Hud || !host.City ||
               host.InWorld || !mailboxBridge || mailboxBridge.IsShowing)
                return false;
            var saved=host.City.Gateway?.Snapshot();
            var mailbox=saved?.Mailbox;
            if(mailbox==null || (mailbox.ReceivedEventIds?.Count??0)==0 &&
               (mailbox.Messages?.Count??0)==0)
                return false;
            var canonical=host.Hud.GetComponent<
                Eldoria.UI.RoyalGold.V2.RoyalGoldV41MailboxScreen>();
            if(!canonical || canonical.IsOpen ||
               host.Hud.NarrativeOpen ||
               (host.Hud.Modal && host.Hud.Modal.gameObject.activeSelf) ||
               (canonical.CanShowEntry!=null && !canonical.CanShowEntry()))
                return false;
            return !(destinationBridge && destinationBridge.IsShowing) &&
                !(heroesBridge && heroesBridge.IsShowing) &&
                !(retentionBridge && retentionBridge.IsShowing);
        }

        void UpdateMailSeal()
        {
            if(!utilityRail || !mailUtilityButton)return;
            bool show=CanShowCanonicalMailbox();
            // This pre-existing original bell medallion is now the one real
            // introduced-mail route. Gear and breach designs remain hidden,
            // not inert fake promises or overlapping lower nav controls.
            utilityRail.gameObject.SetActive(show);
            mailUtilityButton.gameObject.SetActive(show);
            mailUtilityButton.interactable=show;
            for(int i=1;i<utilityRail.childCount;i++)
                utilityRail.GetChild(i).gameObject.SetActive(false);
        }

        void OpenActualMailbox()
        {
            if(!CanShowCanonicalMailbox())return;
            if(mailboxBridge.TryOpen())
            {
                SetFeedback("");
                UpdateMailSeal();
            }
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
                case 2:
                    // Ejército is the REAL, source-bound march composition.
                    // It cannot appear before Bastion/Barracks narrative
                    // unlock, and has no independent army save or mock troops.
                    if((NavigationMask()&(1<<2))==0)return;
                    if(host.InWorld && !host.TrySetWorld(false))return;
                    string lastBuilding=selected;
                    SelectBuilding("");
                    if(destinationBridge && destinationBridge.TryOpen(
                        Eldoria.UI.RoyalGold.V2.RoyalGoldV3ManagementScreens.Destination.March))
                    {
                        selectedTab=2;
                        RenderNow();
                    }
                    else
                    {
                        if(!string.IsNullOrEmpty(lastBuilding))
                            SelectBuilding(lastBuilding);
                        SetFeedback("Todavía no está disponible la marcha.");
                    }
                    break;
                case 4:
                    // The canonical heroine recruitment gate is stronger than
                    // a static level check; no premature, hollow Heroes button.
                    if((NavigationMask()&(1<<4))==0)return;
                    if(host.InWorld && !host.TrySetWorld(false))return;
                    string returnBuilding=selected;
                    SelectBuilding("");
                    if(heroesBridge && heroesBridge.TryOpen())
                    {
                        selectedTab=4;
                        RenderNow();
                    }
                    else
                    {
                        if(!string.IsNullOrEmpty(returnBuilding))
                            SelectBuilding(returnBuilding);
                        SetFeedback("Recluta primero a Lyra.");
                    }
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
            }
        }
        void RefreshNavigationAppearance()
        {
            if(!created || !host)return;
            // The actual scene and destination controllers own active state.
            int current=host.InWorld?3:selectedTab;
            if(!host.InWorld)
            {
                if(destinationBridge && destinationBridge.IsShowing)current=2;
                else if(heroesBridge && heroesBridge.IsShowing)current=4;
                else if(selected=="bastion")current=0;
                else if(selected=="sawmill" || selected=="stoneworks" ||
                    selected=="granary" || selected=="barracks" ||
                    selected=="upgrade-barracks")current=1;
            }
            for(int i=0;i<navButtons.Length;i++)
            {
                bool active=i==current && navButtons[i] &&
                    navButtons[i].gameObject.activeInHierarchy;
                if(navActiveOutlines[i] && navActiveOutlines[i].enabled!=active)
                    navActiveOutlines[i].enabled=active;
                if(navCaptions[i])
                {
                    var ink=active?BreachWatchControlKit.Gold:
                        BreachWatchControlKit.Ivory;
                    if(navCaptions[i].color!=ink)navCaptions[i].color=ink;
                }
            }
        }
        void LateUpdate()
        {
            if(!created||!host)return;
            // A canonical modal handles its own Back/Close. When the actual
            // destination closes, restore the city navigation highlight;
            // never leave a selected emblem pointing at a hidden panel.
            if(selectedTab==2 && destinationBridge &&
               !destinationBridge.IsShowing)selectedTab=0;
            if(selectedTab==4 && heroesBridge &&
               !heroesBridge.IsShowing)selectedTab=0;
            if(feedbackUntil>0f && Time.unscaledTime>=feedbackUntil)
                SetFeedback("");
            // When the canonical world map returns to city it resets the
            // city camera. Restore the approved playable Bastion/bridge
            // composition exactly once, without stealing pan/zoom afterward.
            if(lastWorldMode && !host.InWorld && cityComposition)
                cityComposition.OnCityReentered(Screen.width,Screen.height);
            lastWorldMode=host.InWorld;
            RefreshNavigationAppearance();
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
                float identityHeight=smallPhone?58f:48f;
                At(identity,margin,smallPhone?11f:14f,182,identityHeight);
                float crestSize=smallPhone?45f:37f;
                // Engraved lion lives INSIDE the one painted player plaque:
                // no separate disc hanging over a second visible surface.
                // The single profile illustration already contains its lion.
                // Source-faithful physical gap after the FULL lion ring,
                // now preserved by a 35% non-stretching 9-slice border.
                float lionTextInset=crestSize+(smallPhone?8f:16f);
                // The real cf73 Unity screenshot had a completely blank
                // profile even though the live session supplies name and
                // power: Unity's legacy Text silently paints zero glyphs
                // when Truncate box is shorter than font line metrics.
                // Keep both live labels above the observed lower painted rim:
                // genuine 900x1600 Unity d88903ba showed the PODER descenders
                // touching it. Raise both rows by 4 authored units while
                // preserving their original heights, spacing and one Canvas.
                At(identityName.rectTransform,lionTextInset,smallPhone?6f:5f,
                    (smallPhone?174f:166f)-crestSize,smallPhone?21f:17f);
                At(identityPower.rectTransform,lionTextInset,
                    smallPhone?27f:23f,(smallPhone?174f:166f)-crestSize,
                    smallPhone?18f:17f);
                identityName.fontSize=smallPhone?14:12;
                identityPower.fontSize=smallPhone?13:11;

                float rightSpace=w-(margin+182+9+margin);
                // Three approved stone counters occupy predictable slots.
                // Right-align only on the narrow phone. Hiding unbuilt food
                // cannot push the two existing counters 100 pixels away from
                // their original positions on the owner's 864px reference.
                // Preserve the correctly centred badge; widen its capsule so
                // the amount has real space before the pointed right cap.
                float chip=smallPhone
                    ?(visibleResources==2
                        ?Mathf.Min(94f,Mathf.Max(72f,(rightSpace-4f)/2f))
                        :Mathf.Min(70f,Mathf.Max(58f,(rightSpace-8f)/3f)))
                    :82f;
                float chipGap=smallPhone?4f:12f;
                float resourcesWidth=visibleResources*chip+
                    chipGap*(visibleResources-1);
                float chipHeight=smallPhone?38f:28f;
                float symbolSize=smallPhone?23f:19f;
                float resourceLeft=smallPhone
                    ?w-margin-resourcesWidth
                    :Mathf.Min(w-margin-resourcesWidth,w*.52f);
                At(resourceRow,resourceLeft,smallPhone?18f:17f,
                    resourcesWidth,chipHeight);
                for(int i=0;i<3;i++)
                {
                    resourceChips[i].gameObject.SetActive(i<visibleResources);
                    if(i>=visibleResources)continue;
                    At(resourceChips[i],i*(chip+chipGap),0,chip,chipHeight);
                    LayoutResourceContents(i,chip,chipHeight,symbolSize);
                    resourceAmounts[i].fontSize=smallPhone?14:12;
                }

                At(centerMark,w*.50f-56,69,112,30);
                // Single carved plate taken from the owner's approved 864x1536
                // original: the castle insignia is PAINTED IN that one plate.
                // Keep live chapter, objective and progress editable on top.
                // The owner's carved quest plaque is wide and shallow. The
                // original 78u height made the illustration look like a poster.
                float missionWidth=Mathf.Min(smallPhone?248f:184f,
                    w*(smallPhone?.59f:.40f));
                float missionTop=smallPhone?80f:82f;
                float missionHeight=smallPhone?110f:68f;
                // The NEW independently illustrated mission asset has an
                // alpha-trimmed painted silhouette, unlike the older cropped
                // reference with 12u transparent padding. Align its true
                // pixels with the 13u profile inset; never clip the bevel.
                At(objective,margin,missionTop,missionWidth,
                    fullObjective?(smallPhone?130f:112f):missionHeight);
                // A genuine 390x844 phone showed 12*.91=10.92px, below
                // the minimum readable physical text. Use a 13u authored
                // serif chapter and reclaim a little space right of the
                // badge without overlaying the real emblem or metal contour.
                // On 900x1600 source art the live 12pt caption is 133.5u
                // across but the old 130u rect wrapped into TWO lines.
                // Shift ink 5u right of the same badge contour; 136u
                // now fits ONE true source-faithful line at both phone sizes.
                // Real Unity 900x1600 proves the old 14u row painted a
                // visible heading but failed the 15u native legibility guard.
                // Move the same live text 1u UP and make the row 1u taller,
                // preserving a full 2u gap to the actual objective at y21u.
                At(chapterName.rectTransform,smallPhone?54f:36f,
                    smallPhone?16f:13f,missionWidth-(smallPhone?64f:44f),
                    smallPhone?20f:15f);
                // Preserve three independent readable rows INSIDE the real
                // engraved plate. The former 15u progress row ended 1u
                // above the plate bottom on 900x1600, failing real GPU QA.
                // Genuine Unity screenshot 8a8bd8 (900x1600 and 390x844)
                // proved the live 0/1 fraction was below the painted lower
                // gold rim, while the preceding task row had extra line
                // spacing. Move ONLY live ink upward, with 2u minimum row
                // separation; keep the genuine 2K source and active button.
                At(chapterObjective.rectTransform,smallPhone?54f:36f,
                    smallPhone?40f:27f,missionWidth-(smallPhone?64f:44f),
                    fullObjective?51f:(smallPhone?36f:15f));
                At(chapterProgress.rectTransform,smallPhone?54f:36f,
                    fullObjective?(smallPhone?94f:87f):
                        (smallPhone?82f:43f),
                    missionWidth-(smallPhone?64f:44f),smallPhone?19f:14f);
                // Larger physical lettering and a genuine two-line task well
                // replace the ~10px objective visible on the native 390px run.
                chapterName.fontSize=smallPhone?15:11;
                chapterName.resizeTextForBestFit=true;
                chapterName.resizeTextMinSize=smallPhone?13:11;
                chapterName.resizeTextMaxSize=smallPhone?15:11;
                chapterObjective.fontSize=smallPhone?16:13;
                chapterObjective.resizeTextForBestFit=true;
                chapterObjective.resizeTextMinSize=smallPhone?14:12;
                chapterObjective.resizeTextMaxSize=smallPhone?16:13;
                chapterProgress.fontSize=smallPhone?15:12;

                // Secondary seals are disabled on this first screen:
                // keep their carved artwork discreet rather than occupying
                // three large touch footprints over the playable terrain.
                // One introduced-mail seal near the mission (not an extra
                // row of stacked lower buttons). Chapter-I remains untouched
                // until the real first delivery creates its event ledger.
                At(utilityRail,margin,missionTop+
                    (fullObjective&&smallPhone?130f:missionHeight)+16f,44f,49f);
                for(int i=0;i<utilityRail.childCount;i++)
                {
                    var utility=(RectTransform)utilityRail.GetChild(i);
                    utility.gameObject.SetActive(false);
                    At(utility,0,i*45f,37,37);
                }

                // Full owner-reference medallions; retain a restrained bronze
                // link rather than a second dark bottom toolbar.
                // Small independent gold medallions, reference pitch 107px
                // and footer y1493 on 900x1600; keep full invisible hitbox.
                // The original approved sprites stay untouched. On a narrow
                // phone, the painted medal was ~31px despite a ~47px tap
                // area. Expand the actual visible artwork, not its style.
                // Native 42865155787 measured 35.29px painted REINO
                // at 844x390 despite 52 source geometry checks passing.
                // Preserve owner-approved artwork, increase its real mesh
                // footprint and keep the complete row within the safe area.
                // Owner review: footer must remain visibly present in a full-screen mobile capture.
                // Larger original medallions, safe-area clearance and all five slots still fit.
                float navSize=75f,navGap=6f;
                float navWidth=visibleNav*navSize+Mathf.Max(0,visibleNav-1)*navGap;
                float navTop=h-(smallPhone?146f:127f);
                At(buttonRow,(w-navWidth)*.5f,navTop,navWidth,
                    100f);
                navFiligree.Configure(navSize+navGap,navSize,visibleNav);
                int navColumn=0;
                for(int i=0;i<5;i++)
                {
                    bool visible=(navigationMask & (1<<i))!=0;
                    navButtons[i].gameObject.SetActive(visible);
                    if(visible)
                    {
                        At((RectTransform)navButtons[i].transform,
                            navColumn++*(navSize+navGap),3,navSize,navSize+17f);
                        At(navPlates[i].rectTransform,smallPhone?4f:5f,0f,
                            navSize-(smallPhone?8f:10f),navSize-(smallPhone?8f:10f));
                        At(navCaptions[i].rectTransform,0f,navSize-1f,
                            navSize,16f);
                        navCaptions[i].fontSize=smallPhone?13:12;
                        navPlates[i].VisualScale=smallPhone?.92f:.90f;
                    }
                }

                // Approved portrait reference places the card across
                // roughly one third of the screen rather than nearly half.
                // Separate, fully readable live text fits in the compact
                // vertical stack; it is never flattened into one bitmap.
                // Match the approved 864×1536 premium reference: the card
                // occupies roughly one third of width and leaves more
                // unblocked river and city. Internal live text is rearranged
                // instead of baking it into an image.
                // Source-faithful owner cartouche: less dead header height,
                // more world visible and one sculpted material, not stacked cards.
                // At 900x1600 Unity the old card was 299px wide and
                // anchored 23px from the right edge; owner's approved card
                // is ~276px wide and set ~69px in. Shift the WHOLE real
                // panel once, do not build a second decorative panel.
                // Phones retain their 44px targets and own safe-area inset.
                float pw=smallPhone?Mathf.Min(171f,w*.42f):158f;
                // Exact real Unity 5b57bb0 390x844 failed: 65u photo forced
                // y=0.13..0.84 but genuine masonry occupied y=0.12..0.85.
                // In the original one-panel composition add 17u ONLY to the
                // portrait well up to the real painted rule, and overall card; move later live rows
                // with it. This gives the native 512x320 mesh an aspect-safe
                // 82u aperture instead of silently cropping turrets/stairs.
                float ph=297f;
                float cardRightInset=smallPhone?margin:39f;
                float left=w-cardRightInset-pw;
                // Genuine 900x1600 reference comparison at SHA 138416cf:
                // approved 864x1536 card sits about 65-75 physical pixels
                // LOWER than the Unity panel. Shift the ONE already live
                // context panel +40 authored units (~70px at 900-wide);
                // do not change its material or overlay a second card.
                // Small phones retain their separately verified safe layout.
                float top=navTop-ph-(smallPhone?34f:32f);
                At(panel,left,top,pw,ph);
                // Unity 6000.3 GPU proof 42824817856: the original
                // 49u second line wrapped NIVEL I into 29.7u of glyphs
                // while its 17u masked rect hid the actual level numeral.
                // The approved crest ends at x45; the 50u close button
                // starts x103. Both true text rows can safely occupy the
                // intervening 57u, with a 1u hard separation from close.
                // Keep both captions fully live, no painted placeholder.
                At(buildingName.rectTransform,45,21,pw-104,21);
                At(buildingLevel.rectTransform,45,43,pw-104,17);
                At(((RectTransform)closeButton.transform),pw-55,14,50,50);
                // Source-measured new card: illustration well ends at 129.7u,
                // description below it; one painted divider at 160.8u.
                // Never include live description inside the portrait frame.
                // The real source-painted border is at ~137u in this 297u
                // panel: finish the live portrait at y=135, BEFORE its gold rim.
                // Increasing the photo well would cover actual artwork.
                At(portraitFrame,15,61,pw-30,74);
                // The full live description remains below the portrait and well
                // above the existing gilded cost divider.
                At(buildingFunction.rectTransform,13,146,pw-26,22);
                descriptionDivider.gameObject.SetActive(false);
                At(costHeader.rectTransform,13,174,pw-26,17);
                At(buildingCosts.rectTransform,13,194,pw-26,20);
                At(costIcons[0].rectTransform,14,194,19,19);
                At(costQuantities[0].rectTransform,39,194,42,20);
                At(costIcons[1].rectTransform,89,194,19,19);
                // Unity native 42835121287 proved that the canonical 300
                // stone cost wrapped to 40u in its 33u-wide slot; the
                // screenshot falsely appeared to cost only 30. Never hide
                // or truncate a real price. Use the remaining true 43u
                // before the card's bronze rim, not a painted overlay.
                At(costQuantities[1].rectTransform,114,194,pw-119,20);
                costQuantities[0].fontSize=14;
                costQuantities[1].fontSize=14;
                At(buildingState.rectTransform,16,220,pw-32,17);
                // The full live Spanish action is 95px wide at 12pt: retain a
                // 32.5% hammer well AND widen the actual painted action, rather
                // than shrinking the gameplay text below legibility.
                At(((RectTransform)actionButton.transform),6,ph-53,pw-12,50);
                // Readable physical text survives the .91 scale of 390px
                // devices while the larger 900px design stays lightweight.
                buildingNameMaxFont=15;
                buildingNameMinFont=smallPhone?14:12;
                buildingName.fontSize=buildingNameMaxFont;
                buildingLevel.fontSize=smallPhone?13:11;
                buildingFunction.fontSize=smallPhone?14:11;
                buildingCosts.fontSize=smallPhone?14:12;
                buildingState.fontSize=smallPhone?14:12;
                costHeader.fontSize=smallPhone?13:10;
                actionLabel.fontSize=smallPhone?16:13;

                At(feedbackPlate,margin,h-160,Mathf.Max(120,left-margin-15),36);
                Fill(feedback.rectTransform,6);
                feedback.fontSize=13;
            }
            else
            {
                At(identity,margin,11,234,66);
                // The single profile illustration already contains its lion.
                // Native 1600x900 proves the name draws in the first row.
                // The power must remain >= 11.75 physical pixels even in
                // 844x390 (.86 scale), therefore 14pt is mandatory; reserve
                // its full Legacy Text line height without another panel.
                At(identityName.rectTransform,59,12,166,25);
                At(identityPower.rectTransform,59,39,165,21);
                identityName.fontSize=17;
                identityPower.fontSize=14;

                float chip=visibleResources==2
                    ?Mathf.Min(124f,Mathf.Max(94f,(w*.28f-7f)/2f))
                    :Mathf.Min(100f,Mathf.Max(82f,(w*.28f-12f)/3f));
                float resourcesWidth=visibleResources*chip+7f*(visibleResources-1);
                At(resourceRow,w-margin-resourcesWidth,16,resourcesWidth,43);
                for(int i=0;i<3;i++)
                {
                    resourceChips[i].gameObject.SetActive(i<visibleResources);
                    if(i>=visibleResources)continue;
                    At(resourceChips[i],i*(chip+7),0,chip,41);
                    LayoutResourceContents(i,chip,41f,23f);
                    resourceAmounts[i].fontSize=16;
                }
                At(centerMark,w*.49f-60,18,120,28);
                // On the real 844x390 Unity capture, the previous 61+19
                // progress position left only 2.6 physical pixels above
                // the bottom bevel. Keep true text visibly inside the stone.
                // The actual 1600x900 Unity first native screenshot showed
                // title and objective fraction too near the source-metal rim.
                // Shift live ink INSIDE the same authentic complete illustration.
                At(objective,margin,100,256,fullObjective?122:106);
                At(chapterName.rectTransform,66,20,180,18);
                // Native 844x390 measured only 2.6px between the real
                // progress ink and the lower gold contour. Reserve at least
                // 9px; no second panel, no hidden fake fraction or font shrink.
                // Native 844x390 (run 42865155787) shows 0/1 visibly
                // touching the bottom carved rim. Move BOTH live rows up
                // in the SAME approved plate; preserve title and text data.
                At(chapterObjective.rectTransform,66,42,176,fullObjective?50:19);
                At(chapterProgress.rectTransform,66,fullObjective?93:65,176,18);
                // 844x390 actual native pixels: 13 at scale .86 gives only
                // 11.18px. Enlarge the live caption rather than weaken QA.
                chapterName.fontSize=14;
                // Portrait Reflow leaves BestFit max 13 in the same Canvas.
                // Force the ACTUAL painted 14pt on short landscape, not only
                // its misleading public fontSize property.
                chapterName.resizeTextMinSize=14;
                chapterName.resizeTextMaxSize=14;
                chapterObjective.fontSize=15;
                // Reset the portrait best-fit limits when rotating the same Canvas.
                chapterObjective.resizeTextMinSize=14;
                chapterObjective.resizeTextMaxSize=15;
                chapterProgress.fontSize=14;

                // Landscape quest ends at y~206; the separate small mail
                // seal sits below, clear of the centered bottom medallions.
                At(utilityRail,margin,219f,44f,49f);
                for(int i=0;i<utilityRail.childCount;i++)
                {
                    var utility=(RectTransform)utilityRail.GetChild(i);
                    utility.gameObject.SetActive(false);
                    At(utility,0,i*44f,37,37);
                }

                // The landscape phone also needs visible medals, while 1600x900
                // keeps the approved desktop pitch and silhouettes.
                bool compactLandscape=h<530f;
                float navSize=compactLandscape?75f:55f,
                    navGap=compactLandscape?10f:15f;
                float navWidth=visibleNav*navSize+Mathf.Max(0,visibleNav-1)*navGap;
                float navTop=h-(compactLandscape?104f:94f);
                At(buttonRow,(w-navWidth)*.5f,navTop,navWidth,
                    compactLandscape?101f:85f);
                navFiligree.Configure(navSize+navGap,navSize,visibleNav);
                int navColumn=0;
                for(int i=0;i<5;i++)
                {
                    bool visible=(navigationMask & (1<<i))!=0;
                    navButtons[i].gameObject.SetActive(visible);
                    if(visible)
                    {
                        At((RectTransform)navButtons[i].transform,
                            navColumn++*(navSize+navGap),3,navSize,navSize+17f);
                        At(navPlates[i].rectTransform,compactLandscape?4f:6f,0f,
                            navSize-(compactLandscape?8f:12f),
                            navSize-(compactLandscape?8f:12f));
                        At(navCaptions[i].rectTransform,0f,navSize-1f,
                            navSize,16f);
                        navCaptions[i].fontSize=compactLandscape?13:12;
                        navPlates[i].VisualScale=compactLandscape?.92f:.86f;
                    }
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
                At(buildingName.rectTransform,60,h<530f?12f:22f,pw-121,h<530f?27f:22f);
                At(buildingLevel.rectTransform,60,h<530f?40f:45f,pw-121,21);
                At(((RectTransform)closeButton.transform),pw-57,3,52,52);
                buildingNameMaxFont=18;
                buildingNameMinFont=16;
                buildingName.fontSize=buildingNameMaxFont;
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
                    // The complete source card already paints its gold rule.
                    descriptionDivider.gameObject.SetActive(false);
                    At(descriptionDivider,14,197,pw-28,9);
                    At(costHeader.rectTransform,13,204,pw-26,19);
                    costHeader.fontSize=14;
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

            // BestFit limits set during construction otherwise override each
            // layout's fontSize, and survive orientation changes. Use the
            // measured fixed size for this short inscription in every layout.
            costHeader.resizeTextForBestFit=false;
            FitBuildingName();

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
                // The NEW independent 2K painting is 3.45:1 instead of the
                // old 246:60 sprite. Contain it in BOTH dimensions so its
                // real source gold border cannot protrude into the timer.
                float paintedHeight=Mathf.Min(plate.rect.height-4f,
                    plate.rect.width/Mathf.Max(1f,artAspect));
                float paintedWidth=paintedHeight*artAspect;
                float paintedLeft=(plate.rect.width-paintedWidth)*.5f;
                float paintedTop=(plate.rect.height-paintedHeight)*.5f;
                At(illustration,paintedLeft,paintedTop,paintedWidth,paintedHeight);
                // Caption is anchored to PAINTED pixels, not the larger
                // invisible accessible touch target. Hammers occupy ~28%.
                // Extra clearance after native GPU test showed the new 2K hammer
                // relief extends beyond 28% of the painted button.
                float captionLeft=paintedLeft+paintedWidth*.30f;
                // Keep eight source units clear of the right gilded edge.
                // The prior fourteen-unit inset truncated Bastion II with the
                // production font despite spare painted space on its right.
                float captionRight=paintedLeft+paintedWidth-8f;
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

            // The viewport matches the single illustration well painted into
            // the new panel. Do not letterbox it into a second smaller box.
            // CropPortraitToTrueGeometry preserves the model's proportions.
            if(portraitView && portraitFrame)
                Fill(portraitView.rectTransform,2f);
            lastPortrait=portrait;
            lastNavigationMask=navigationMask;
            lastResourceCount=visibleResources;
            UpdateMailSeal();
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
            // A tighter but strictly positive transparent gutter preserves the
            // entire renderer silhouette while enlarging the actual Bastion
            // within the painted gold rim. No texture stretching or masking.
            float wantedWidth=bounds.width+.045f;
            float wantedHeight=bounds.height+.045f;
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

        void LayoutResourceContents(int index,float chipWidth,float chipHeight,float iconSize)
        {
            // The authored capsule badge is centred at x=423/2593, not at
            // the centre of an arbitrary icon slot. Its centre lies inside
            // the unscaled left nine-slice border: 423px / (2200/100).
            const float badgeCenterX=423f/22f;
            float badgeCenterY=chipHeight*.49f;
            At(resourceIllustrations[index].rectTransform,
                badgeCenterX-iconSize*.5f,badgeCenterY-iconSize*.5f,
                iconSize,iconSize);
            float amountLeft=badgeCenterX+iconSize*.5f+4f;
            At(resourceAmounts[index].rectTransform,amountLeft,3f,
                chipWidth-amountLeft-3f,chipHeight-6f);
        }

        public void RenderNow()
        {
            if(!created||!host)return;
            try
            {
                snapshot=host.Read(selected,host.English);
                RefreshNavigationAppearance();
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
                    ? realChapter.ToUpperInvariant()+" / "+(host.English?"THE BREACH":"LA BRECHA")
                    : realChapter;
                // The approved first-chapter lettering fits the authored
                // bronze well at its fixed source-measured size. Unity 6000.3
                // BestFit silently painted font 12 in 844x390 despite nominal
                // size 14 and min/max 14. Do not use BestFit on Chapter I;
                // later variable chapter names may still fit dynamically.
                chapterName.resizeTextForBestFit=!chapterOne;
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
                    FitBuildingName();
                    buildingLevel.text="NIVEL "+b.Level;
                    if(buildingPortrait)
                    {
                        buildingPortrait.Show(ActualBuildingModel(b.Id),host.PlayerCamera,
                            b.Id+"/"+b.Level);
                        CropPortraitToTrueGeometry();
                    }
                    buildingFunction.text=(b.Id=="bastion" && b.Level==1)
                        ? (host.English?"Valoria's government."
                            :"Gobierno de Valoria.")
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
                        // The true 2K forged-iron art has a shorter text well on landscape;
                        // use the exact same real Bastion II action with concise text.
                        bool fullUpgradeCaption=lastPortrait && lastWidth>=800;
                        actionCaption=host.English
                            ?(fullUpgradeCaption?"Upgrade to Bastion II":"Upgrade II")
                            :(fullUpgradeCaption?"Mejorar a Bastión II":"Mejorar a II");
                    }
                    actionLabel.text=actionCaption;
                    // Fit the ACTUAL Unity font's preferred width into the
                    // carved red field before letting uGUI render it. Unity's
                    // BestFit+Overflow had passed CI while visibly painting
                    // "II" outside the medal on a 900x1600 phone.
                    actionLabel.resizeTextForBestFit=false;
                    actionLabel.horizontalOverflow=HorizontalWrapMode.Wrap;
                    actionLabel.verticalOverflow=VerticalWrapMode.Truncate;
                    float legalWidth=Mathf.Max(16f,
                        actionLabel.rectTransform.rect.width-2.5f);
                    for(int size=lastPortrait?14:15;size>=11;size--)
                    {
                        actionLabel.fontSize=size;
                        if(actionLabel.preferredWidth<=legalWidth)break;
                    }
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
                    // Keep the authored relief visible after every data refresh.
                    // Reflow previously used .86-.92, then RenderNow silently
                    // shrank the same artwork to .70-.76 in the next frame.
                    if(p)p.VisualScale=i==activeTab?.92f:.88f;
                    if(p)p.color=allowed
                        ? i==(host.InWorld?3:selectedTab)
                            ?new Color(1f,.97f,.87f,1f):Color.white
                        :new Color(.70f,.68f,.65f,.88f);
                }
                UpdateMailSeal();
                if(resourceBagBridge)resourceBagBridge.Synchronize();
            }
            catch(Exception e)
            {
                SetFeedback("Error en datos reales: "+e.GetType().Name);
                Debug.LogException(e);throw;
            }
        }
        void FitBuildingName()
        {
            // The first-screen Bastion fits at 15pt, but Aserradero measures
            // 65.9u in the same font against a 54u portrait well. Measure the
            // actual live title; retain the full name and readable lower bound.
            // Reset from the layout maximum on every selection so a long name
            // cannot leave subsequent short names permanently undersized.
            buildingName.resizeTextForBestFit=false;
            float available=Mathf.Max(1f,buildingName.rectTransform.rect.width);
            for(int size=buildingNameMaxFont;size>=buildingNameMinFont;size--)
            {
                buildingName.fontSize=size;
                if(buildingName.preferredWidth<=available)break;
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
            if(resourceBagBridge)resourceBagBridge.Restore();
            if(singleCrestPaletteMaterial)Destroy(singleCrestPaletteMaterial);
            if(host && host.Hud)host.Hud.SelectionChanged-=OnCanonicalSelection;
            foreach(var entry in originalCameraMasks)
                if(entry.Key && (entry.Value & (1<<5))==0)
                    entry.Key.cullingMask &= ~(1<<5);
            if(host && host.City && host.City.Presenter)
                host.City.Presenter.ExternalPointerBlocked=priorPointerBlocker;
            if(legacyCanvas)legacyCanvas.enabled=!productionPresentation && originalCanvas;
            if(legacyRaycaster)legacyRaycaster.enabled=!productionPresentation && originalRaycaster;
            if(legacyGroup){legacyGroup.alpha=productionPresentation?0:originalAlpha;legacyGroup.blocksRaycasts=!productionPresentation && originalBlocks;legacyGroup.interactable=!productionPresentation && originalInteractable;}
        }
    }
}
