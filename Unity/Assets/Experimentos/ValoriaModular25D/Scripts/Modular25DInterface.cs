using System;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Eldoria.Experimentos.ValoriaModular25D
{
    /// <summary>
    /// CORONA DE LA BRECHA UI: illustrated arcane medallions, identity and 9-slice surfaces.
    /// One dynamic Canvas, the canonical LocalGateway, and only two real player
    /// actions exposed by the canonical game. No invented systems.
    /// New 9-slice materials are created once as small in-memory textures;
    /// the protected source art, game rules and persistence stay unchanged.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed partial class Modular25DInterface : MonoBehaviour
    {
        public const string UiEdition = "Corona de la Brecha - Eldoria 2.5D";
        public const int FutureNavigationSlots = 8;
        private static readonly Color32 Slate = new Color32(27, 20, 43, 242);
        private static readonly Color32 SlateLight = new Color32(55, 40, 72, 238);
        private static readonly Color32 Parchment = new Color32(33, 25, 48, 250);
        private static readonly Color32 ParchmentLight = new Color32(65, 48, 83, 252);
        private static readonly Color32 AntiqueBrass = new Color32(201, 156, 84, 255);
        private static readonly Color32 PaleGold = new Color32(245, 213, 156, 255);
        private static readonly Color32 WarmWhite = new Color32(248, 238, 220, 255);
        private static readonly Color32 DarkInk = new Color32(26, 20, 40, 255);
        private static readonly Color32 MutedInk = new Color32(203, 181, 217, 255);
        private static readonly Color32 Green = new Color32(80, 209, 195, 255);
        private static readonly Color32 Red = new Color32(184, 84, 120, 255);

        private static Sprite slatePlate, actionPlate, resourcePlate, portraitPlate, activityStandard, navPlate, topVelvetPlate, activityPanelPlate;
        private static Sprite woodIcon, stoneIcon, foodIcon, hammerIcon, cityIcon, taskIcon;
        private static Texture2D originalIconAtlas;
        private Modular25DGameplay gateway;
        private GameObject buildingSheet;
        private Canvas hudCanvas;
        private RectTransform mailboxDock, sheet, statusSheet, objective, noticeRoot;
        private RectTransform activityHandle, activityDrawer, activityFill, activityTrack;
        private Text activityTitle, activityProgress;
        private bool activityExpanded;
        // World-anchored, tap-enabled nameplates: standard strategy-game
        // readability without hand-painted text embedded in the scenery.
        private RectTransform bastionTag, sawmillTag;
        private Text bastionTagText, sawmillTagText;
        private Modular25DWorld world;
        private PlayerRibbon playerRibbon;
        private RectTransform constructionProgressRoot,constructionProgressFill;
        private RectTransform worldProgressTrack,worldProgressFill;

        private Text stage, cost, remaining, actionLabel, notice, objectiveText, stateSummary;
        private Button action;
        private RectTransform firstBuildGuide, districtClock;
        private Text districtClockText;
        private SpriteRenderer districtClockBuilding;
        private bool hasConstructionDeadline;
        private RectTransform regionDock;
        private bool worldUnlocked, enteringWorld;

        private float nextTick, noticeExpires;
        // Read-only presentation cache of the EXISTING canonical construction
        // phase. Avoid Snapshot() per frame in world-space nameplate tracking.
        private ParcelBuildingState cachedSawmillPhase=ParcelBuildingState.NOT_BUILT;
        private int lastWidth = -1, lastHeight = -1;
        private Rect lastSafeArea;

        private void Awake()
        {
            gateway = GetComponent<Modular25DGameplay>();
            world = GetComponent<Modular25DWorld>();
            if (gateway == null || world == null) throw new InvalidOperationException("Umbral requires the canonical modular gateway");
            if (EventSystem.current == null)
                new GameObject("ValoriaModular25D Events", typeof(EventSystem), typeof(InputSystemUIInputModule));
#if UNITY_WEBGL && !UNITY_EDITOR
            localJourneyProbe = Uri.TryCreate(UnityEngine.Application.absoluteURL, UriKind.Absolute, out var url)
                && url.IsLoopback && url.Query == "?journey-qa";
#endif
            InitializeArt();
            var root = new GameObject("CORONA DE LA BRECHA | HUD original",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            hudCanvas = root.GetComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 240;
            hudCanvas.pixelPerfect=true;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390f, 390f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            var rootTransform = root.transform;

            playerRibbon=new PlayerRibbon(rootTransform);

            // Each building now communicates its interactive status in world
            // space like a commercial 4X city. These labels MOVE with the
            // camera and point only to canonical, currently implemented UI.
            bastionTag=ButtonPlate("Bastion world nameplate",rootTransform,"BASTIÓN · I",
                slatePlate,new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                Vector2.zero,new Vector2(115f,27f),OpenBastion).GetComponent<RectTransform>();
            bastionTag.GetComponent<Image>().raycastPadding=new Vector4(-3f,-9f,-3f,-9f);
            bastionTagText=bastionTag.GetComponentInChildren<Text>(true);
            bastionTagText.fontSize=14;
            bastionTagText.color=PaleGold;
            // Building nameplates are wayfinding, NOT a second enormous
            // turquoise Build CTA. The actual action stays in the real modal.
            // Keep the two original art plates short/readable in world-space.
            var keepGlyph=Plate("Bastion nameplate original icon",bastionTag,cityIcon,
                new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                new Vector2(-37f,0f),new Vector2(17f,17f));
            keepGlyph.GetComponent<Image>().preserveAspect=true;
            bastionTagText.rectTransform.anchoredPosition=new Vector2(10f,0f);
            bastionTagText.rectTransform.sizeDelta=new Vector2(100f,28f);
            sawmillTag=ButtonPlate("Sawmill world nameplate",rootTransform,"ASERRADERO",
                slatePlate,new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                Vector2.zero,new Vector2(115f,27f),OpenSawmill).GetComponent<RectTransform>();
            sawmillTag.GetComponent<Image>().raycastPadding=new Vector4(-3f,-9f,-3f,-9f);
            sawmillTagText=sawmillTag.GetComponentInChildren<Text>(true);
            sawmillTagText.fontSize=14;
            sawmillTagText.color=PaleGold;
            sawmillTagText.rectTransform.anchoredPosition=new Vector2(12f,0f);
            sawmillTagText.rectTransform.sizeDelta=new Vector2(100f,30f);
            var sawmillGlyph=Plate("Sawmill nameplate original icon",sawmillTag,hammerIcon,
                new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                new Vector2(-37f,0f),new Vector2(17f,17f));
            sawmillGlyph.GetComponent<Image>().preserveAspect=true;
            // Visible AFTER the successful Build auto-dismiss: real progress
            // belongs on the construction parcel, not a persistent modal.
            // This understated track has no new game clock or invented rewards.
            worldProgressTrack=Plate("World sawmill canonical progress track",sawmillTag,null,
                new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                new Vector2(0f,-11f),new Vector2(86f,4f));
            worldProgressTrack.GetComponent<Image>().color=new Color32(17,11,32,226);
            worldProgressFill=Plate("World sawmill elapsed canonical fill",worldProgressTrack,null,
                new Vector2(0f,.5f),new Vector2(0f,.5f),
                new Vector2(2f,0f),new Vector2(2f,2f));
            worldProgressFill.GetComponent<Image>().color=new Color32(76,211,192,255);
            worldProgressTrack.gameObject.SetActive(false);

            // Main's Chronicle bottom navigation contains only the two live
            // destinations World and Mailbox. Contextual city actions remain
            // on the existing Bastion/Sawmill world labels and objective card.
            mailboxDock = DockButton("OpenSystem/mail",rootTransform,"CORREO",NeedCorona("icon_supplies"),
                Vector2.zero,Vector2.zero,() => OpenCanonicalRoom("mail"));
            if (Eldoria.ReferenceClean.ValoriaTravelSession.CombinedPlayer)
            {
                regionDock = DockButton("Visitar Región 1", rootTransform, "MUNDO", NeedCorona("icon_world"),
                    Vector2.zero, Vector2.zero, EnterRegion);
                regionDock.gameObject.SetActive(false);
            }
            objective=ButtonPlate("Current real construction objective",rootTransform,"",
                null,new Vector2(1f,1f),new Vector2(1f,1f),
                new Vector2(-12f,-122f),new Vector2(172f,50f),OpenCouncil)
                .GetComponent<RectTransform>();
            // The activity drawer is the entry point to the current objective.
            var workshop=Plate("Activity workshop illustration",objective,
                Modular25DArt.Sawmill(),new Vector2(0f,.5f),new Vector2(0f,.5f),
                new Vector2(1f,0f),new Vector2(49f,46f));
            workshop.GetComponent<Image>().preserveAspect=true;
            objectiveText=Label("Consejo",objective,17,false,WarmWhite,
                TextAnchor.MiddleLeft,new Vector2(14f,0f),new Vector2(105f,31f));
            Label("›",objective,21,false,PaleGold,TextAnchor.MiddleRight,
                new Vector2(77f,0f),new Vector2(18f,32f));

            // Hidden LEFT activity drawer: reuse the existing real quest
            // Button and live construction clock without adding a new
            // command or changing any resource/mission rules.
            activityDrawer=Plate("Left activity drawer",rootTransform,activityPanelPlate,
                new Vector2(0f,.5f),new Vector2(0f,.5f),
                new Vector2(54f,0f),new Vector2(246f,185f));
            activityTitle=Label("TAREAS",activityDrawer,16,true,PaleGold,
                TextAnchor.MiddleCenter,new Vector2(0f,49f),new Vector2(198f,25f));
            objective.SetParent(activityDrawer,false);
            objective.anchorMin=objective.anchorMax=new Vector2(.5f,.5f);
            objective.pivot=new Vector2(.5f,.5f);
            objective.anchoredPosition=new Vector2(0f,9f);
            activityProgress=Label("ASERRADERO",activityDrawer,13,false,WarmWhite,
                TextAnchor.MiddleCenter,new Vector2(0f,-39f),new Vector2(198f,24f));
            activityProgress.verticalOverflow=VerticalWrapMode.Overflow;
            activityTrack=Plate("Activity real construction track",activityDrawer,null,
                new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                new Vector2(0f,-59f),new Vector2(178f,7f));
            activityTrack.GetComponent<Image>().color=new Color32(20,13,38,232);
            activityFill=Plate("Activity canonical construction fill",activityTrack,null,
                new Vector2(0f,.5f),new Vector2(0f,.5f),
                new Vector2(2f,0f),new Vector2(2f,5f));
            activityFill.GetComponent<Image>().color=new Color32(75,214,199,255);
            activityTrack.gameObject.SetActive(false);
            activityHandle=ButtonPlate("Toggle left activities",rootTransform,"",activityStandard,
                new Vector2(0f,.5f),new Vector2(0f,.5f),Vector2.zero,
                new Vector2(46f,62f),ToggleActivities).GetComponent<RectTransform>();
            activityHandle.GetComponent<Image>().type=Image.Type.Simple;
            activityHandle.GetComponent<Image>().preserveAspect=true;
            activityDrawer.gameObject.SetActive(false);
            activityExpanded=false;

            // Context sheet uses warm architectural parchment, a real sprite
            // from the experiment, and original data. Not a generic modal.
            sheet = Plate("Sawmill parchment sheet", rootTransform, activityPanelPlate,
                new Vector2(.5f, 0f), new Vector2(.5f, 0f), new Vector2(0f, 110f),
                new Vector2(355f, 244f));
            buildingSheet = sheet.gameObject;
            Label("ASERRADERO", sheet, 23, true, WarmWhite, TextAnchor.MiddleLeft,
                new Vector2(-30f, 84f), new Vector2(190f, 31f));
            Label("TALLER DE VALORIA", sheet, 11, false, PaleGold, TextAnchor.MiddleLeft,
                new Vector2(-30f, 61f), new Vector2(200f, 17f));
            AccentHairline("Divider", sheet, AntiqueBrass,
                new Vector2(0f, 44f), new Vector2(270f, 1.5f));
            var previewPlate = Plate("Architectural preview", sheet, null,
                new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(-88f, -2f),
                new Vector2(96f, 99f));
            var view = Plate("Real illustrated sawmill", previewPlate, null,
                new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero,
                new Vector2(103f, 100f));
            var preview = view.GetComponent<Image>();
            preview.sprite = Modular25DArt.Sawmill();
            preview.preserveAspect = true;
            preview.type = Image.Type.Simple;
            preview.color = Color.white;
            preview.raycastTarget = false;
            stage = Label("", sheet, 14, false, WarmWhite, TextAnchor.MiddleLeft,
                new Vector2(56f, 26f), new Vector2(185f, 22f));
            cost = Label("", sheet, 14, false, MutedInk, TextAnchor.MiddleLeft,
                new Vector2(56f, 0f), new Vector2(185f, 23f));
            remaining = Label("", sheet, 14, false, WarmWhite, TextAnchor.MiddleLeft,
                new Vector2(56f, -28f), new Vector2(185f, 23f));

            stage.verticalOverflow=VerticalWrapMode.Overflow;
            cost.verticalOverflow=VerticalWrapMode.Overflow;
            remaining.verticalOverflow=VerticalWrapMode.Overflow;

            // Commercial construction feedback, but backed solely by the
            // canonical action clock. No invented production, new save or
            // decorative continuously animated fake progress.
            constructionProgressRoot=Plate("Canonical construction progress frame",sheet,null,
                new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                new Vector2(54f,-50f),new Vector2(190f,10f));
            constructionProgressRoot.GetComponent<Image>().color=AntiqueBrass;
            var track=Plate("Dark progress track",constructionProgressRoot,null,
                new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                Vector2.zero,new Vector2(185f,6f));
            track.GetComponent<Image>().color=Slate;
            constructionProgressFill=Plate("Real sawmill elapsed time",track,null,
                new Vector2(0f,.5f),new Vector2(0f,.5f),
                new Vector2(2f,0f),new Vector2(181f,4f));
            constructionProgressFill.GetComponent<Image>().color=
                new Color32(119,188,141,255);
            constructionProgressRoot.gameObject.SetActive(false);
            action = ButtonPlate("Execute canonical Build command", sheet, "CONSTRUIR",
                actionPlate, new Vector2(.5f, .5f), new Vector2(.5f, .5f),
                new Vector2(0f, -80f), new Vector2(actionPlate.rect.width * 44f / actionPlate.rect.height, 44f),
                () =>
                {
                    // Only a successful real LocalGateway construction
                    // dismisses the panel. Failed commands keep the panel
                    // open so the feedback is actionable. The worksite,
                    // progress clock and spend persist through the canonical
                    // gateway; no synthetic game state is introduced.
                    var result=cachedSawmillPhase==ParcelBuildingState.BUILT
                        ? gateway.Issue("UpgradeBuilding","sawmill") : gateway.BuildSawmill();
                    if(result.Ok)ClosePanels();
                });
            actionLabel = action.GetComponentInChildren<Text>();
            actionLabel.color=WarmWhite;
            actionLabel.fontSize=15;
            actionLabel.resizeTextForBestFit=true;
            actionLabel.resizeTextMinSize=10;
            actionLabel.resizeTextMaxSize=15;
            actionLabel.verticalOverflow=VerticalWrapMode.Overflow;
            var close = ButtonPlate("Close sawmill", sheet, "×", navPlate,
                new Vector2(1f, 1f), new Vector2(.5f, .5f),
                new Vector2(-27f, -25f), new Vector2(46f, 46f),
                ClosePanels);
            var closeText = close.GetComponentInChildren<Text>();
            closeText.fontSize = 25;
            buildingSheet.SetActive(false);

            statusSheet=Plate("Valoria state parchment",rootTransform,activityPanelPlate,
                new Vector2(.5f,0f),new Vector2(.5f,0f),
                new Vector2(0f,112f),new Vector2(330f,170f));
            councilTitle = Label("CONSEJO DE VALORIA",statusSheet,23,true,WarmWhite,TextAnchor.MiddleCenter,
                new Vector2(0f,0f),new Vector2(248f,28f));
            councilTitle.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top,10f,28f);

            stateSummary=Label("",statusSheet,14,false,WarmWhite,TextAnchor.MiddleLeft,
                new Vector2(0f,-15f),new Vector2(278f,83f));
            InitializeCouncil();
            var closeStatus=ButtonPlate("Close state",statusSheet,"×",navPlate,
                new Vector2(1f,1f),new Vector2(.5f,.5f),new Vector2(-27f,-22f),
                new Vector2(45f,45f),ClosePanels);
            closeStatus.GetComponentInChildren<Text>().fontSize=23;
            statusSheet.gameObject.SetActive(false);

            noticeRoot = Plate("Temporary canonical feedback", rootTransform, slatePlate,
                new Vector2(.5f, 1f), new Vector2(.5f, 1f), Vector2.zero,
                new Vector2(324f, 35f));
            notice = Label("", noticeRoot, 12, false, WarmWhite, TextAnchor.MiddleCenter,
                Vector2.zero, new Vector2(300f, 30f));
            noticeRoot.gameObject.SetActive(false);
            Layout();
        }

        private void OnEnable()
        {
            if (gateway == null) return;
            gateway.StateChanged += Refresh;
            gateway.MessageChanged += Feedback;
        }

        private void OnDisable()
        {
            if (gateway == null) return;
            gateway.StateChanged -= Refresh;
            gateway.MessageChanged -= Feedback;
        }

        private void Start()
        {
            Canvas.ForceUpdateCanvases();
            Layout();
            if (Eldoria.ReferenceClean.ValoriaTravelSession.CombinedPlayer)
            {
                districtClock = Plate("Plazo de la obra", hudCanvas.transform, slatePlate,
                    new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero, new Vector2(158f,38f));
                districtClock.GetComponent<Image>().color=new Color(.61f,.66f,.73f,1f);
                districtClockText = Label("", districtClock, 17, false, WarmWhite, TextAnchor.MiddleCenter,
                    Vector2.zero, new Vector2(148f,32f));
                districtClock.gameObject.SetActive(false);
                var hand = Resources.Load<Sprite>("ValoriaModular25D/UI/CoronaBrecha/guante_guia");
                if (hand)
                {
                    firstBuildGuide = Plate("Guía de la primera obra",sawmillTag,hand,new Vector2(.68f,.18f),
                        new Vector2(.21f,.86f),Vector2.zero,new Vector2(74f,74f));
                    var guideImage = firstBuildGuide.GetComponent<Image>();
                    guideImage.preserveAspect = true;
                    guideImage.raycastTarget = false;
                    var edge=firstBuildGuide.gameObject.AddComponent<Shadow>();
                    edge.effectColor=new Color(.085f,.06f,.055f,.90f);
                    edge.effectDistance=new Vector2(1.7f,-2.1f);
                    firstBuildGuide.gameObject.SetActive(false);
                }
            }
            Refresh(gateway.Snapshot());
            Feedback(gateway.LastMessage);
            OpenNamesIfNeeded();
            Eldoria.ReferenceClean.ValoriaTravelCurtain.Arrived(Eldoria.ReferenceClean.ValoriaTravelSession.CityScene);
            SyncWorldNameplates();
        }

        void OpenNamesIfNeeded()
        {
            if (!Eldoria.ReferenceClean.ValoriaTravelSession.CombinedPlayer) return;
            var state = gateway.Snapshot();
            if (state == null || !string.IsNullOrWhiteSpace(state.PlayerName)) return;
            var prompt = Eldoria.PlayerClarity.RoyalPlayerNamePrompt.Create((RectTransform)hudCanvas.transform);
            var frame = prompt.transform.Find("Aged parchment");
            var inner = frame.Find("Inner vellum").GetComponent<Image>();
            inner.sprite = activityPanelPlate;
            inner.color = Color.white;
            inner.type = Image.Type.Sliced;
            frame.GetComponent<Image>().enabled = false;
            var scene=Resources.Load<Texture2D>("DailyTreasureCeremonies/bastion_stage");
            if(scene)
            {
                var illustration=new GameObject("Valoria at the naming ceremony",typeof(RectTransform),typeof(RawImage));
                var painting=illustration.GetComponent<RawImage>();
                painting.transform.SetParent(prompt.transform,false);
                painting.transform.SetAsFirstSibling();
                var bounds=painting.rectTransform;
                bounds.anchorMin=Vector2.zero; bounds.anchorMax=Vector2.one;
                bounds.offsetMin=Vector2.zero; bounds.offsetMax=Vector2.zero;
                painting.texture=scene;
                var fit=illustration.AddComponent<AspectRatioFitter>();
                fit.aspectRatio=scene.width/(float)scene.height;
                fit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;
                painting.color=new Color(.62f,.66f,.66f);
                painting.raycastTarget=false;
            }
            foreach (var text in inner.transform.parent.GetComponentsInChildren<Text>(true))
            {
                text.font=Eldoria.UI.RoyalGold.V2.CoronaFinish.ResolveFont(text.name=="Opening question");
                text.color = WarmWhite;
                if (text.name == "Opening question") text.color = PaleGold;
            }
            // The gate remains visible above the player's inscription. Fractional
            // anchors let the same panel contract if the keyboard resizes the view.
            var plaque = (RectTransform)frame;
            plaque.anchorMin = new Vector2(.07f,.13f);
            plaque.anchorMax = new Vector2(.93f,.74f);
            var inscription = frame.Find("Player's chosen name").GetComponent<Image>();
            // Reuse the same quiet painted inset that holds the HUD wallet.
            // The InputField and its clipping, placeholder and keyboard stay intact.
            inscription.sprite=resourcePlate;
            inscription.type=resourcePlate.border.sqrMagnitude>0?Image.Type.Sliced:Image.Type.Simple;
            inscription.color=new Color(.80f,.79f,.86f,.92f);
            var rim = inscription.GetComponent<Outline>();
            rim.effectColor = new Color(.59f,.51f,.35f,.86f);
            var editor = inscription.GetComponent<InputField>();
            editor.textComponent.alignment = TextAnchor.MiddleCenter;
            var hint = (Text)editor.placeholder;
            hint.alignment = TextAnchor.MiddleCenter;
            hint.color = new Color(.81f,.78f,.69f);
            var confirm = frame.Find("Confirm your name").GetComponent<Image>();
            confirm.sprite = actionPlate;
            confirm.color = Color.white;
            var confirmControl=confirm.GetComponent<Button>();
            if(confirmControl)Eldoria.UI.RoyalGold.V2.CoronaFinish.ApplyButtonFeedback(confirmControl);
            confirm.rectTransform.anchorMin = new Vector2(.22f,.055f);
            confirm.rectTransform.anchorMax = new Vector2(.78f,.19f);
            prompt.Open(false, name => gateway.Issue("ChoosePlayerName", name).Ok &&
                string.Equals(gateway.Snapshot().PlayerName, name, StringComparison.Ordinal));
        }

        public void ToggleActivities()
        {
            if(activityDrawer==null)return;
            activityExpanded=!activityExpanded;
            activityDrawer.gameObject.SetActive(activityExpanded);
            if(activityExpanded&&gateway!=null)Refresh(gateway.Snapshot());
        }

        public void OpenSawmill()
        {
            // The activities shortcut and a map tap converge on the same plot.
            var camera=world.ViewCamera;
            Vector3 center=world.SawmillRenderer.bounds.center;
            camera.transform.position=new Vector3(center.x,center.y,camera.transform.position.z);
            if(buildingSheet==null)return;
            if(activityDrawer!=null)activityDrawer.gameObject.SetActive(false);
            activityExpanded=false;
            if(statusSheet!=null)statusSheet.gameObject.SetActive(false);
            buildingSheet.SetActive(true);
            SetModalChrome();
            Refresh(gateway.Snapshot());
        }

        void EnterRegion()
        {
            if (enteringWorld || !Eldoria.ReferenceClean.ValoriaIntegratedWorld.CanEnterWorld(gateway.Snapshot()))
                return;
            enteringWorld = true;
            regionDock.GetComponent<Button>().interactable = false;
            Feedback("Preparando Región 1…");
            Eldoria.ReferenceClean.ValoriaTravelCurtain.Travel(Eldoria.ReferenceClean.ValoriaTravelSession.RegionScene);
        }

        public void OpenBastion() => OpenDistrict("bastion");

        void OpenCouncil() => ShowCityPanel(null);

        private void ShowCityPanel(string building)
        {
            if(statusSheet==null)return;
            selectedBuilding = building;
            orderList.anchoredPosition = Vector2.zero;
            if(activityDrawer!=null)activityDrawer.gameObject.SetActive(false);
            activityExpanded=false;
            if(buildingSheet!=null)buildingSheet.SetActive(false);
            statusSheet.gameObject.SetActive(true);
            orderList.GetComponentInParent<ScrollRect>().StopMovement();
            SetModalChrome();
            Refresh(gateway.Snapshot());
        }

        public void ClosePanels()
        {
            if(buildingSheet!=null)buildingSheet.SetActive(false);
            if(statusSheet!=null)statusSheet.gameObject.SetActive(false);
            SetModalChrome();
        }

        // The 844x390 construction panel occupies the bottom-right camera
        // region. Showing bottom navigation under it creates overlapping hit
        // targets. Hide both canonical destinations while a contextual sheet
        // is open; they return immediately through the real close control.
        // No gameplay action or backend state changes here.
        private void SetModalChrome()
        {
            bool open=(buildingSheet!=null&&buildingSheet.activeSelf)||
                      (statusSheet!=null&&statusSheet.gameObject.activeSelf);
            if(mailboxDock)mailboxDock.gameObject.SetActive(!open);
            if (regionDock) regionDock.gameObject.SetActive(!open && worldUnlocked);
            if(objective!=null)objective.gameObject.SetActive(!open);
            if(activityHandle!=null)activityHandle.gameObject.SetActive(!open);
            if(open && activityDrawer!=null)
            {
                activityDrawer.gameObject.SetActive(false);
                activityExpanded=false;
            }
        }

        private void Update()
        {
            if(firstBuildGuide)
            {
                bool atSite = cachedSawmillPhase == ParcelBuildingState.AVAILABLE;
                bool show = atSite && !roomHost && !statusSheet.gameObject.activeSelf &&
                    (!buildingSheet.activeSelf || action.interactable);
                RectTransform target = buildingSheet.activeSelf ? (RectTransform)action.transform :
                    activityExpanded ? objective : sawmillTag.gameObject.activeInHierarchy ? sawmillTag : activityHandle;
                if(firstBuildGuide.parent != target) firstBuildGuide.SetParent(target,false);
                if(firstBuildGuide.gameObject.activeSelf != show) firstBuildGuide.gameObject.SetActive(show);
                if (show)
                {
                    bool parcel = target == sawmillTag;
                    // The mill is west of the keep. Place the palm over the open field
                    // while its fingertip stays anchored to the mill label. On the left
                    // screen edge mirror it onto the visible side of the same target.
                    bool fromWest = false;
                    if (parcel)
                    {
                        Vector2 westTip = RectTransformUtility.WorldToScreenPoint(null,
                            target.TransformPoint(new Vector2(target.rect.xMin+target.rect.width*.24f,
                                target.rect.yMax)));
                        float clearance = firstBuildGuide.rect.width*hudCanvas.scaleFactor;
                        fromWest = westTip.x-Screen.safeArea.xMin >= clearance;
                    }
                    Vector2 anchor = parcel ? new Vector2(fromWest?.24f:.76f,.86f) : new Vector2(.68f,.18f);
                    firstBuildGuide.anchorMin = firstBuildGuide.anchorMax = anchor;
                    firstBuildGuide.localScale = new Vector3(fromWest?-1f:1f,1f,1f);
                    firstBuildGuide.localRotation = parcel ?
                        Quaternion.Euler(0f,0f,fromWest?-85f:85f) : Quaternion.identity;
                    float pulse = 3f * Mathf.Sin(Time.unscaledTime * 3f);
                    firstBuildGuide.anchoredPosition = new Vector2(fromWest?-2f:2f, parcel ? 3f + pulse : -2f - pulse);
                }
            }
            if (roomHost && roomIsOpen != null && !roomIsOpen()) CloseCanonicalRoom();
            if (Screen.width != lastWidth || Screen.height != lastHeight ||
                Screen.safeArea != lastSafeArea)
                Layout();
            if (noticeRoot != null && noticeRoot.gameObject.activeSelf &&
                Time.unscaledTime > noticeExpires)
                noticeRoot.gameObject.SetActive(false);
            // Keep labels attached to their actual buildings through pan and
            // pinch; never leave fixed fake nameplates at screenshot positions.
            SyncWorldNameplates();
            if (Time.unscaledTime < nextTick) return;
            nextTick = Time.unscaledTime + .25f;
#if UNITY_WEBGL && !UNITY_EDITOR
            if (localJourneyProbe) ReportJourneyControls();
#endif
            // The Aserradero worksite timer remains readable on the world
            // even though Build closes its panel. Query at 4 Hz only during
            // canonical construction or an explicitly opened detail sheet.
            if(gateway!=null && (hasConstructionDeadline || cachedSawmillPhase==ParcelBuildingState.UNDER_CONSTRUCTION ||
                (buildingSheet!=null&&buildingSheet.activeSelf) || statusSheet.gameObject.activeSelf))
                Refresh(gateway.Snapshot());
        }

        private void Feedback(string value)
        {
            if (noticeRoot == null) return;
            value = value ?? "";
            if (value.StartsWith("Partida experimental aislada", StringComparison.OrdinalIgnoreCase))
                value = "";
            // Real WebGL 390x844 showed a giant "Acción iniciada" banner
            // painted straight across the keep after Build, hiding the
            // centre of the scene. Successful construction now closes the
            // sheet and exposes its real timer on the world tag. Duplicating
            // that with a generic success toast is unnecessary obstruction.
            // Keep genuine failure/errors visible for user feedback.
            if (value.Equals("Acción iniciada",StringComparison.OrdinalIgnoreCase))
                value = "";
            notice.text = value;
            var show = value.Length > 0;
            noticeRoot.gameObject.SetActive(show);
            if (show) noticeExpires = Time.unscaledTime + 2.8f;
        }

        public static string PlayerDisplayName(string savedName)
        {
            if(string.IsNullOrWhiteSpace(savedName))return "VALORIA";
            return savedName.Trim();
        }

        // WOS/Kingshot-sized resource indicators must remain legible
        // even when the canonical wallet grows past four or six digits.
        // The small HUD uses compact display only; status/gameplay/Save
        // always retain the unrounded authoritative resource amount.
        public static string CompactResource(long amount)
        {
            if(amount<1000L)return amount.ToString("N0");
            double v=amount;
            int tier=0;
            while(v>=1000d && tier<3){v/=1000d;tier++;}
            string suffix=tier==1?"K":tier==2?"M":"B";
            string mask=v>=100d?"0":v>=10d?"0.#":"0.#";
            return v.ToString(mask,System.Globalization.CultureInfo.CurrentCulture)+suffix;
        }

        private void Refresh(PlayerState s)
        {
            if (playerRibbon == null) return;
            playerRibbon.Refresh(s);
            if (s == null)
            {
                if (action != null) action.interactable = false;
                return;
            }
            if (regionDock)
            {
                bool unlocked = Eldoria.ReferenceClean.ValoriaIntegratedWorld.CanEnterWorld(s);
                bool navigationChanged = unlocked != worldUnlocked;
                worldUnlocked = unlocked;
                regionDock.gameObject.SetActive(worldUnlocked && !buildingSheet.activeSelf && !statusSheet.gameObject.activeSelf);
                // The initial layout has one centred Mailbox cell. When the
                // real unlock arrives, reflow immediately to main's two-cell
                // World/Mailbox order instead of stacking both icons.
                if(navigationChanged)Layout();
            }
            if(mailboxDock)mailboxDock.gameObject.SetActive(!buildingSheet.activeSelf&&!statusSheet.gameObject.activeSelf);

            if(bastionTagText!=null)
                bastionTagText.text="BASTIÓN · "+s.BastionLevel;
            cachedSawmillPhase=ParcelBuildingStates.For(s,"sawmill");
            long deadline = Math.Max(s.BuildingCompletesUtcTicks,s.BastionUpgradeCompletesUtcTicks);
            hasConstructionDeadline = deadline > DateTime.UtcNow.Ticks;
            districtClockBuilding = null;
            if (hasConstructionDeadline && districtClockText)
            {
                string task = s.BuildingTaskId ?? "";
                string name = "";
                if (s.BastionUpgradeCompletesUtcTicks > 0) { districtClockBuilding = world.BastionRenderer; name = "BASTIÓN"; }
                else if (task.Contains("barracks")) { districtClockBuilding = world.BarracksRenderer; name = "CUARTEL"; }
                else if (task.Contains("granary")) { districtClockBuilding = world.GranaryRenderer; name = "GRANERO"; }
                else if (task.Contains("stoneworks")) { districtClockBuilding = world.StoneworksRenderer; name = "CANTERA"; }
                else if (task.Contains("sawmill") && cachedSawmillPhase != ParcelBuildingState.UNDER_CONSTRUCTION)
                { districtClockBuilding = world.SawmillRenderer; name = "ASERRADERO"; }
                long seconds = Math.Max(0L,(long)Math.Ceiling((deadline-DateTime.UtcNow.Ticks)/(double)TimeSpan.TicksPerSecond));
                districtClockText.text = name + "  " + (seconds/60L) + ":" + (seconds%60L).ToString("00");
            }
            if(sawmillTag!=null && sawmillTagText!=null)
            {
                var building=cachedSawmillPhase;
                long timeLeft=building==ParcelBuildingState.UNDER_CONSTRUCTION
                    ? gateway.SawmillSecondsRemaining() : 0L;
                sawmillTagText.text=building==ParcelBuildingState.BUILT
                    ? "ASERRADERO · "+s.SawmillLevel
                    : building==ParcelBuildingState.UNDER_CONSTRUCTION
                        ? "OBRA  "+(timeLeft/60L).ToString()+":"+(timeLeft%60L).ToString("00")
                        : "ASERRADERO";
                bool constructing=building==ParcelBuildingState.UNDER_CONSTRUCTION;
                // The gilded end caps and hammer sit inside the painted width.
                // Measure the localized caption so a completed mill can show its
                // level without projecting past the right cap.
                float captionWidth=Mathf.Max(74f,Mathf.Ceil(sawmillTagText.preferredWidth));
                float plateWidth=captionWidth+43f;
                sawmillTag.sizeDelta=new Vector2(plateWidth,27f);
                sawmillTagText.rectTransform.anchoredPosition=new Vector2(9f,constructing?3f:0f);
                sawmillTagText.rectTransform.sizeDelta=new Vector2(captionWidth,constructing?23f:30f);
                if(worldProgressTrack!=null)
                    worldProgressTrack.gameObject.SetActive(constructing);
                if (constructing && worldProgressFill)
                    worldProgressFill.sizeDelta = new Vector2(82f*Mathf.Clamp01(1f-timeLeft/(float)Math.Max(1L,(long)SliceRules.SawmillBuildSeconds)),2f);
                if(activityTitle!=null)
                    activityTitle.text=constructing?"OBRA EN CURSO":"ACTIVIDADES";
                if(activityProgress!=null)
                    activityProgress.text=constructing
                        ? "Finaliza en "+(timeLeft/60L).ToString()+":"+
                            (timeLeft%60L).ToString("00")
                        : building==ParcelBuildingState.BUILT
                            ? "Construcción terminada":"Parcela disponible";
                if(activityTrack!=null)activityTrack.gameObject.SetActive(constructing);
                if(constructing && activityFill!=null)
                {
                    float duration=Mathf.Max(1f,(float)SliceRules.SawmillBuildSeconds);
                    float elapsed=Mathf.Clamp01(1f-(float)timeLeft/duration);
                    activityFill.sizeDelta=new Vector2(Mathf.Max(2f,174f*elapsed),5f);
                }
                // Arcane turquoise is reserved for the actual BUILD command.
                // The world label stays the same readable deep obsidian plaque
                // in every phase and only its text state changes.
                var image=sawmillTag.GetComponent<Image>();
                image.sprite=slatePlate;
                image.type=Image.Type.Simple;
                sawmillTagText.color=building==ParcelBuildingState.UNDER_CONSTRUCTION
                    ? WarmWhite : PaleGold;
            }
            if(stateSummary!=null)
                stateSummary.text="Jugador: " +
                    (string.IsNullOrWhiteSpace(s.PlayerName)?"SIN NOMBRE":s.PlayerName.Trim())+
                    "\nBastión " + s.BastionLevel + "  ·  Aserradero " + s.SawmillLevel +
                    "\nMadera: " + s.Resources.Wood.ToString("N0") +
                    "   Piedra: " + s.Resources.Stone.ToString("N0") +
                    "\nComida: " + s.Resources.Food.ToString("N0");
            if(statusSheet.gameObject.activeSelf) RefreshCouncil(s);
            if (buildingSheet == null || !buildingSheet.activeSelf) return;
            var phase = ParcelBuildingStates.For(s, "sawmill");
            if(constructionProgressRoot!=null)
                constructionProgressRoot.gameObject.SetActive(
                    phase==ParcelBuildingState.UNDER_CONSTRUCTION);
            cost.text = "Coste · " + SliceRules.SawmillWoodCost.ToString("N0") + " de madera";
            switch (phase)
            {
                case ParcelBuildingState.BUILT:
                    stage.text = "CONSTRUIDO · NIVEL " + s.SawmillLevel;
                    cost.text = "Producción activa";
                    remaining.text = "La obra está terminada";
                    var offer = gateway.BuildingOffer("sawmill",s.Revision);
                    bool canUpgrade = offer?.Action != null && offer.Action.Enabled;
                    actionLabel.text = canUpgrade ? "MEJORAR" : "CONSTRUIDO";
                    action.interactable = canUpgrade;
                    if(canUpgrade) { cost.text=offer.Costs; remaining.text=offer.Time; }
                    break;
                case ParcelBuildingState.UNDER_CONSTRUCTION:
                    stage.text = "RECONSTRUCCIÓN EN CURSO";
                    var secondsLeft=gateway.SawmillSecondsRemaining();
                    remaining.text = "Finaliza en " + secondsLeft + " s";
                    if(constructionProgressFill!=null)
                    {
                        float duration=Mathf.Max(1f,(float)SliceRules.SawmillBuildSeconds);
                        float elapsed=Mathf.Clamp01(1f-secondsLeft/duration);
                        constructionProgressFill.sizeDelta=
                            new Vector2(Mathf.Max(2f,181f*elapsed),4f);
                    }
                    actionLabel.text = "EN CONSTRUCCIÓN";
                    action.interactable = false;
                    break;
                case ParcelBuildingState.AVAILABLE:
                    stage.text = "PARCELA DISPONIBLE";
                    remaining.text = s.Resources.Wood < SliceRules.SawmillWoodCost
                        ? "Recursos insuficientes" : "Listo para comenzar";
                    actionLabel.text = "INICIAR OBRA";
                    action.interactable = s.Resources.Wood >= SliceRules.SawmillWoodCost;
                    break;
                default:
                    stage.text = "PENDIENTE DE DESBLOQUEO";
                    remaining.text = "Continúa la progresión";
                    actionLabel.text = "BLOQUEADO";
                    action.interactable = false;
                    break;
            }
        }

        private void SyncWorldNameplates()
        {
            if(world==null||world.ViewCamera==null||hudCanvas==null||
               bastionTag==null||sawmillTag==null)return;
            var baseRect=hudCanvas.GetComponent<RectTransform>();
            bool hiddenByPanel=(buildingSheet!=null&&buildingSheet.activeSelf)||
                               (statusSheet!=null&&statusSheet.gameObject.activeSelf);
            PlaceBuildingNameplate(bastionTag,baseRect,world.BastionRenderer,true,hiddenByPanel || districtClockBuilding==world.BastionRenderer);
            PlaceBuildingNameplate(sawmillTag,baseRect,world.SawmillRenderer,true,
                hiddenByPanel || districtClockBuilding==world.SawmillRenderer);
            if (districtClock && districtClockBuilding)
            {
                if (districtClockBuilding.gameObject.activeInHierarchy)
                    PlaceBuildingNameplate(districtClock,baseRect,districtClockBuilding,true,hiddenByPanel);
                else
                    PlaceWorldNameplate(districtClock,baseRect,districtClockBuilding.transform.position,hiddenByPanel,32f);
            }
            else if (districtClock && districtClock.gameObject.activeSelf) districtClock.gameObject.SetActive(false);
        }

        private void PlaceBuildingNameplate(RectTransform label,RectTransform canvas,
            SpriteRenderer building,bool above,bool hidden)
        {
            if(building==null)return;
            Bounds bounds=building.bounds;
            Vector3 anchor=new Vector3(bounds.center.x+(building==world.SawmillRenderer?bounds.size.x*.10f:0f),
                above?bounds.max.y:bounds.min.y,building.transform.position.z);
            // Keep a constant logical gap as the architecture changes size with zoom.
            float gap=label.rect.height*.5f+2f;
            PlaceWorldNameplate(label,canvas,anchor,hidden,above?gap:-gap);
        }

        private void PlaceWorldNameplate(RectTransform nameplate,RectTransform canvasRect,
            Vector3 worldPosition,bool modalOpen,float verticalOffset)
        {
            var cam=world.ViewCamera;
            var screen=cam.WorldToScreenPoint(worldPosition);
            screen.y+=verticalOffset*hudCanvas.scaleFactor;
            var safe=Screen.safeArea;
            float scale=hudCanvas.scaleFactor;
            var size=nameplate.rect.size*scale;
            var bounds=new Rect((Vector2)screen-size*.5f,size);
            // In compact landscape, a small lateral clearance keeps the keep title
            // beside the ribbon instead of suppressing it at the home camera.
            if(OverlapsControl(bounds,playerRibbon.PlateRect))
            {
                float ribbonLeft=RectTransformUtility.WorldToScreenPoint(null,
                    playerRibbon.PlateRect.TransformPoint(playerRibbon.PlateRect.rect.min)).x;
                float candidate=ribbonLeft-size.x*.5f-5f*scale;
                if(Mathf.Abs(candidate-screen.x)<=size.x*.5f &&
                    candidate-size.x*.5f>=safe.xMin)
                {
                    screen.x=candidate;
                    bounds=new Rect((Vector2)screen-size*.5f,size);
                }
            }
            bool visible=!modalOpen && screen.z>0f &&
                safe.Contains(bounds.min) && safe.Contains(bounds.max) &&
                !OverlapsControl(bounds,playerRibbon.PlateRect) && !OverlapsControl(bounds,activityHandle) &&
                !OverlapsControl(bounds,mailboxDock) && !OverlapsControl(bounds,regionDock) &&
                !OverlapsControl(bounds,activityDrawer);
            if(nameplate.gameObject.activeSelf!=visible)
                nameplate.gameObject.SetActive(visible);
            if(visible && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,screen,null,out var local))
                nameplate.anchoredPosition=local;
        }

        // All controls share the overlay canvas. Test their on-screen bounds
        // so rotating or using a Retina framebuffer keeps the same clearance.
        private static bool OverlapsControl(Rect bounds,RectTransform control)
        {
            if(!control || !control.gameObject.activeInHierarchy)return false;
            Rect rect=control.rect;
            Vector2 min=RectTransformUtility.WorldToScreenPoint(null,control.TransformPoint(rect.min));
            Vector2 max=RectTransformUtility.WorldToScreenPoint(null,control.TransformPoint(rect.max));
            return bounds.Overlaps(Rect.MinMaxRect(min.x,min.y,max.x,max.y));
        }

        // Eight genuine future positions; absent systems never create buttons.
        // Reserve a full caption width per column. Portrait uses two stable rows.
        public static Rect NavigationCell(int index,float logicalWidth,
            float leftInset,float rightInset,float bottomInset,bool wide)
        {
            if(index<0 || index>=FutureNavigationSlots)
                throw new ArgumentOutOfRangeException(nameof(index));
            float safeWidth=logicalWidth-leftInset-rightInset;
            int columns=safeWidth>=492f?8:4;
            float step=Mathf.Min(60f,(safeWidth-12f)/columns);
            float diameter=Mathf.Clamp(step-3f,44f,46f);
            float origin=logicalWidth-rightInset-6f-columns*step;
            int column=index%columns;
            int row=(FutureNavigationSlots-1-index)/columns;
            return new Rect(origin+column*step+(step-diameter)*.5f,
                bottomInset+12f+row*64f,diameter,diameter+12f);
        }

        private static void PlaceNavigation(RectTransform rect,Rect cell,bool wide)
        {
            rect.anchorMin=rect.anchorMax=new Vector2(0f,0f);
            rect.pivot=new Vector2(0f,0f);
            rect.anchoredPosition=cell.position;
            rect.sizeDelta=cell.size;
            var caption=rect.GetComponentInChildren<Text>(true);
            if(caption!=null)
            {
                caption.fontSize=13;
                caption.alignment=TextAnchor.MiddleCenter;
                caption.rectTransform.anchoredPosition=
                    new Vector2(0f,-cell.width*.5f-9f);
                caption.rectTransform.sizeDelta=new Vector2(cell.width+8f,16f);
                caption.horizontalOverflow=HorizontalWrapMode.Wrap;
                caption.resizeTextForBestFit=true;
                caption.resizeTextMinSize=11;
                caption.resizeTextMaxSize=13;
            }
        }

        private void Layout()
        {
            if(hudCanvas==null||playerRibbon==null)return;
            int w=Screen.width,h=Screen.height;
            if(w<=0||h<=0)return;
            float scale=Mathf.Max(.01f,hudCanvas.scaleFactor);
            Rect safe=Screen.safeArea;
            float top=Mathf.Max(0f,(h-safe.yMax)/scale)+2f;
            float bottom=Mathf.Max(0f,safe.yMin/scale)+8f;
            float left=Mathf.Max(0f,safe.xMin/scale);
            float right=Mathf.Max(0f,(w-safe.xMax)/scale);
            float width=w/scale;
            float height=h/scale;
            float safeWidth=width-left-right;
            bool wide=w>h;

            playerRibbon.Layout(width,left,safeWidth,top);

            // Match main's live Chronicle navigation exactly: Mailbox and,
            // once the canonical rule unlocks it, World. No district shortcuts.
            int canonicalCount=regionDock&&regionDock.gameObject.activeSelf?2:1;
            float canonicalWidth=78f,canonicalGap=16f;
            float canonicalTotal=canonicalCount*canonicalWidth+(canonicalCount-1)*canonicalGap;
            Rect CanonicalCell(int index)=>new Rect(
                left+(safeWidth-canonicalTotal)*.5f+index*(canonicalWidth+canonicalGap),
                bottom+8f,canonicalWidth,84f);
            // ChronicleOrder in main is World=0, Mailbox=1. Before World is
            // unlocked, Mailbox occupies the single centred cell.
            if(regionDock&&regionDock.gameObject.activeSelf)
            {
                PlaceNavigation(regionDock,CanonicalCell(0),wide);
                PlaceNavigation(mailboxDock,CanonicalCell(1),wide);
            }
            else
            {
                PlaceNavigation(mailboxDock,CanonicalCell(0),wide);
                if(regionDock)PlaceNavigation(regionDock,CanonicalCell(0),wide);
            }

            // The progress/quest card is tucked behind ONE tiny LEFT tab
            // by default. Opening it never changes the game state.
            activityHandle.anchorMin=activityHandle.anchorMax=
                new Vector2(0f,.5f);
            activityHandle.pivot=new Vector2(0f,.5f);
            activityHandle.anchoredPosition=new Vector2(left,wide?-5f:0f);
            activityHandle.sizeDelta=new Vector2(44f,54f);
            activityDrawer.anchorMin=activityDrawer.anchorMax=
                new Vector2(0f,.5f);
            activityDrawer.pivot=new Vector2(0f,.5f);
            activityDrawer.anchoredPosition=new Vector2(left+51f,wide?-4f:0f);
            activityDrawer.sizeDelta=wide?new Vector2(243f,173f):
                new Vector2(246f,171f);
            objective.anchorMin=objective.anchorMax=new Vector2(.5f,.5f);
            objective.pivot=new Vector2(.5f,.5f);
            objective.anchoredPosition=new Vector2(0f,wide?8f:9f);
            objective.sizeDelta=new Vector2(186f,65f);
            objective.GetComponent<Image>().preserveAspect=true;

            if(wide)
            {
                sheet.anchorMin=sheet.anchorMax=new Vector2(1f,.5f);
                sheet.pivot=new Vector2(1f,.5f);
                sheet.anchoredPosition=new Vector2(-12f-right,2f);
                sheet.sizeDelta=new Vector2(346f,240f);
                noticeRoot.anchorMin=noticeRoot.anchorMax=new Vector2(0f,1f);
                noticeRoot.pivot=new Vector2(0f,1f);
                noticeRoot.anchoredPosition=new Vector2(left+12f,-top-67f);
                noticeRoot.sizeDelta=new Vector2(286f,35f);
                statusSheet.anchorMin=statusSheet.anchorMax=new Vector2(1f,.5f);
                statusSheet.pivot=new Vector2(1f,.5f);
                statusSheet.anchoredPosition=new Vector2(-12f-right,2f);
                statusSheet.sizeDelta=new Vector2(328f, Mathf.Min(300f,height-top-bottom-12f));
            }
            else
            {
                sheet.anchorMin=sheet.anchorMax=new Vector2(.5f,0f);
                sheet.pivot=new Vector2(.5f,0f);
                sheet.anchoredPosition=new Vector2(0f,bottom+95f);
                sheet.sizeDelta=new Vector2(
                    Mathf.Min(355f,safeWidth-24f),244f);
                noticeRoot.anchorMin=noticeRoot.anchorMax=new Vector2(.5f,1f);
                noticeRoot.pivot=new Vector2(.5f,1f);
                noticeRoot.anchoredPosition=new Vector2(0f,-top-66f);
                noticeRoot.sizeDelta=new Vector2(324f,35f);
                statusSheet.anchorMin=statusSheet.anchorMax=new Vector2(.5f,0f);
                statusSheet.pivot=new Vector2(.5f,0f);
                statusSheet.anchoredPosition=new Vector2(0f,bottom+95f);
                statusSheet.sizeDelta=new Vector2(
                    Mathf.Min(330f,safeWidth-28f),Mathf.Min(440f,height-top-bottom-112f));
            }
            lastWidth=w;
            lastHeight=h;
            lastSafeArea=safe;
        }

        public sealed class PlayerRibbon
        {
            RectTransform topRibbon, identity, resourceRow;
            RectTransform[] resourceChips;
            Text title,level,woodValue,stoneValue,foodValue;
            Image woodAccent;
            Vector4 lastLayout;
            public RectTransform PlateRect => topRibbon;

            public PlayerRibbon(Transform rootTransform)
            {
                InitializeArt();
                // One continuous 46-point ribbon shares player name and wallet.
                topRibbon=Plate("Shared player and resource ribbon",rootTransform,topVelvetPlate,
                    new Vector2(.5f,1f),new Vector2(.5f,1f),Vector2.zero,
                    new Vector2(378f,46f));
                topRibbon.GetComponent<Image>().color=Color.white;
                topRibbon.GetComponent<Image>().preserveAspect=false;
                identity = Plate("Valoria identity", rootTransform, null,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(6f, -6f),
                    new Vector2(102f, 44f));
                var sigil = Plate("Sir Aldric portrait bezel", identity, portraitPlate,
                    new Vector2(0f, .5f), new Vector2(.5f, .5f), new Vector2(19f,0f),
                    new Vector2(35f,35f));
                var aldricAvatar=Resources.Load<Sprite>(
                    "ValoriaModular25D/UI/UmbralII/aldric_avatar");
                if(aldricAvatar==null)
                    throw new InvalidOperationException(
                        "Umbral II requires Eldoria's authentic Sir Aldric portrait.");
                var face=Plate("Authentic Sir Aldric character portrait",sigil,aldricAvatar,
                    new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(44f,44f));
                face.GetComponent<Image>().type=Image.Type.Simple;
                face.GetComponent<Image>().preserveAspect=true;
                face.GetComponent<RectTransform>().sizeDelta=new Vector2(28f,28f);
                // Real PlayerState.PlayerName is the user-selected name. Valoria
                // remains a fallback only for an older unnamed test save.
                title = Label("VALORIA", identity, 13, false, WarmWhite, TextAnchor.MiddleLeft,
                    new Vector2(18f,6f),new Vector2(60f,19f));
                title.resizeTextForBestFit=true;
                title.resizeTextMinSize=10;
                title.resizeTextMaxSize=13;
                title.horizontalOverflow=HorizontalWrapMode.Wrap;
                level = Label("Bastión 1", identity, 11, false, PaleGold, TextAnchor.MiddleLeft,
                    new Vector2(18f,-8f),new Vector2(60f,16f));
                // SourceSans line metrics exceed the 20px legacy plate: Truncate
                // silently discarded all small-glyph geometry in actual WebGL.
                // Let ascenders/descenders use the designed clear lower half.
                level.verticalOverflow = VerticalWrapMode.Overflow;

                resourceRow = Plate("Independent resources", rootTransform, null,
                    new Vector2(1f,1f),new Vector2(1f,1f),Vector2.zero,
                    new Vector2(266f,42f));
                resourceChips = new RectTransform[3];
                resourceChips[0] = CreateResource(resourceRow, 0, "MADERA", woodIcon, out woodValue, out woodAccent);
                Image stoneAccent;
                resourceChips[1] = CreateResource(resourceRow, 1, "PIEDRA", stoneIcon, out stoneValue, out stoneAccent);
                Image foodAccent;
                resourceChips[2] = CreateResource(resourceRow, 2, "COMIDA", foodIcon, out foodValue, out foodAccent);

            }

            public void Layout(float width,float left,float safeWidth,float top)
            {
                // Exactly ONE 46pt player economy strip. Its hairline and all
                // three independently-painted resource plates are aligned to
                // physical pixels and the entire strip hugs the safe top edge.
                topRibbon.anchorMin=topRibbon.anchorMax=new Vector2(.5f,1f);
                topRibbon.pivot=new Vector2(.5f,1f);
                float stripWidth=Mathf.Min(390f,safeWidth);
                float stripLeft=left+safeWidth-stripWidth;
                topRibbon.anchoredPosition=new Vector2(stripLeft+stripWidth*.5f-width*.5f,-top);
                topRibbon.sizeDelta=new Vector2(stripWidth-6f,46f);

                identity.anchorMin=identity.anchorMax=new Vector2(0f,1f);
                identity.pivot=new Vector2(0f,1f);
                identity.anchoredPosition=new Vector2(stripLeft+6f,-top-1f);
                lastLayout=new Vector4(width,left,safeWidth,top);
                // Protect all three economy cells before allocating extra nickname width.
                // Full nickname remains in PlayerState; this is only HUD geometry.
                float economyFloor=Mathf.Min(234f,Mathf.Max(0f,stripWidth-119f));
                float identityMax=Mathf.Max(102f,stripWidth-economyFloor-17f);
                float identityWidth=Mathf.Clamp(title.preferredWidth+46f,102f,identityMax);
                identity.sizeDelta=new Vector2(identityWidth,44f);
                title.rectTransform.sizeDelta=new Vector2(identityWidth-44f,22f);
                level.rectTransform.sizeDelta=new Vector2(identityWidth-44f,16f);
                resourceRow.anchorMin=resourceRow.anchorMax=new Vector2(1f,1f);
                resourceRow.pivot=new Vector2(1f,1f);
                resourceRow.anchoredPosition=new Vector2(stripLeft+stripWidth-width-6f,-top-3f);
                // The real 1280x720 Unity PlayMode found the old landscape
                // 335pt resource row stretched 580x260 source capsules by
                // 27%. Cap all orientations at 270pt. This is SMALLER,
                // preserves premium artist proportions and liberates map.
                float economyWidth=Mathf.Min(270f,
                    Mathf.Max(15f,stripWidth-identityWidth-17f));
                resourceRow.sizeDelta=new Vector2(economyWidth,40f);
                float chipWidth=(economyWidth-12f)/3f;
                float sourceAspect=resourcePlate.rect.height/
                    Mathf.Max(1f,resourcePlate.rect.width);
                for(int i=0;i<resourceChips.Length;i++)
                {
                    var chip=resourceChips[i];
                    chip.anchorMin=chip.anchorMax=new Vector2(0f,.5f);
                    chip.pivot=new Vector2(0f,.5f);
                    // Exact painted Sprite ratio, no artificial 38pt cap
                    // which created the 27% landscape distortion in actual
                    // source-pinned Unity test 44833570082.
                    chip.sizeDelta=new Vector2(chipWidth,chipWidth*sourceAspect);
                    chip.anchoredPosition=new Vector2(i*(chipWidth+6f),0f);
                    chip.GetComponent<Image>().preserveAspect=true;
                    // Reserve disjoint icon and value intervals, with an explicit gap.
                    const float iconInset=5f, valueInset=5f, contentGap=4f;
                    float d=Mathf.Clamp(chipWidth*.30f,18f,26f);
                    var art=chip.Find((i==0?"MADERA":i==1?"PIEDRA":"COMIDA")+
                        " original resource art") as RectTransform;
                    if(art!=null)
                    {
                        art.anchoredPosition=new Vector2(iconInset,0f);
                        art.sizeDelta=new Vector2(d,d);
                    }
                    var number=i==0?woodValue:i==1?stoneValue:foodValue;
                    if(number!=null)
                    {
                        number.rectTransform.anchoredPosition=new Vector2(-valueInset,0f);
                        float valueWidth=Mathf.Max(1f,chipWidth-iconInset-d-contentGap-valueInset);
                        number.rectTransform.sizeDelta=new Vector2(valueWidth,31f);
                        int preferred=chipWidth<82f?16:18;
                        Eldoria.UI.RoyalGold.V2.CoronaFinish.FitSingleLine(number,preferred,10);
                    }
                }

            }

            private static void SetAmount(Text label,string amount,float chipWidth)
            {
                if(label.text==amount)return;
                label.text=amount;
                Eldoria.UI.RoyalGold.V2.CoronaFinish.FitSingleLine(label,chipWidth<82f?16:18,10);
            }

            public void Refresh(PlayerState s)
            {
                if(s==null){woodValue.text=stoneValue.text=foodValue.text="—";return;}
                string name=PlayerDisplayName(s.PlayerName);
                if(title.text!=name){
                    title.text=name;
                    if(lastLayout.x>0)Layout(lastLayout.x,lastLayout.y,lastLayout.z,lastLayout.w);
                }
                level.text = "Bastión " + s.BastionLevel;
                SetAmount(woodValue,CompactResource(s.Resources.Wood),resourceChips[0].rect.width);
                SetAmount(stoneValue,CompactResource(s.Resources.Stone),resourceChips[1].rect.width);
                SetAmount(foodValue,CompactResource(s.Resources.Food),resourceChips[2].rect.width);
                // Preserve the artist's original full-color logs. Tinting the
                // *whole* premium illustration with AntiqueBrass destroyed its
                // wood/rope/material separation. Only numbers convey shortages.
                woodAccent.color=Color.white;
                woodValue.color=s.Resources.Wood<SliceRules.SawmillWoodCost
                    ? Red : WarmWhite;
            }
        }

        private static RectTransform CreateResource(Transform parent,int index,string caption,
            Sprite art,out Text value,out Image icon)
        {
            // Strategy-game resource counter: an instantly recognizable,
            // fully illustrated material and one readable live number. The
            // older two-line mini-labels sat on the gemstone/rim and looked
            // like debug text on a phone. Three clearly different icons
            // already identify wood, stone and grain.
            var chip=Plate("Resource "+caption,parent,resourcePlate,
                new Vector2(0f,.5f),new Vector2(0f,.5f),
                new Vector2(index*123f,0f),new Vector2(117f,52f));
            // The common velvet ribbon supplies the dark ground. Keep the
            // small metal insets subordinate to the original resource and value.
            chip.GetComponent<Image>().color = new Color(.83f,.83f,.87f,.52f);
            var image=Plate(caption+" original resource art",chip,art,
                new Vector2(0f,.5f),new Vector2(0f,.5f),
                new Vector2(10f,0f),new Vector2(42f,42f));
            icon=image.GetComponent<Image>();
            icon.type=Image.Type.Simple;
            icon.preserveAspect=true;
            icon.color=Color.white;
            value=Label("0",chip,18,false,WarmWhite,TextAnchor.MiddleRight,
                new Vector2(25f,0f),new Vector2(60f,33f));
            // Right-edge anchoring keeps amounts out of painted wood/stone
            // icons as landscape capsules compress to fit notch-safe area.
            value.rectTransform.anchorMin=value.rectTransform.anchorMax=new Vector2(1f,.5f);
            value.rectTransform.pivot=new Vector2(1f,.5f);
            value.rectTransform.anchoredPosition=new Vector2(-6f,0f);
            value.rectTransform.sizeDelta=new Vector2(62f,33f);
            value.verticalOverflow=VerticalWrapMode.Overflow;
            value.horizontalOverflow=HorizontalWrapMode.Overflow;
            value.resizeTextForBestFit=true;
            value.resizeTextMinSize=14;
            value.resizeTextMaxSize=18;
            // Two-point UI shadow increases readability on polished violet
            // enamel without a solid text box covering the source artwork.
            var shadow=value.gameObject.AddComponent<Shadow>();
            shadow.effectColor=new Color(.07f,.03f,.13f,.9f);
            shadow.effectDistance=new Vector2(1f,-1f);
            return chip;
        }

        private static void AccentHairline(string name, Transform parent, Color color,
            Vector2 at, Vector2 size)
        {
            var line = Plate(name, parent, null, new Vector2(.5f, .5f),
                new Vector2(.5f, .5f), at, size);
            var image = line.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static RectTransform Plate(string name, Transform parent, Sprite skin,
            Vector2 anchor, Vector2 pivot, Vector2 at, Vector2 size)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);
            var rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = at;
            rect.sizeDelta = size;
            var image = obj.GetComponent<Image>();
            image.raycastTarget = false;
            if (skin != null)
            {
                image.sprite = skin;
                // These are complete painted ornaments, not tileable border strips.
                // Slicing them shrinks the purple center to a line at 390px.
                image.type = Image.Type.Simple;
                image.color = Color.white;
            }
            else image.color = Color.clear;
            return rect;
        }

        private static RectTransform DockButton(string name,Transform parent,string caption,
            Sprite symbol,Vector2 anchor,Vector2 pivot,UnityEngine.Events.UnityAction click)
        {
            // Art review: each illustrated premium icon already CONTAINS its
            // complete sculpted medallion. Nesting it inside nav_medallion
            // rendered two rings and reduced its meaningful artwork to 42px.
            // Use the original artist sprite as the entire button face.
            var control=ButtonPlate(name,parent,caption,symbol,anchor,pivot,
                Vector2.zero,new Vector2(86f,80f),click);
            var rect=control.GetComponent<RectTransform>();
            var art=control.GetComponent<Image>();
            art.type=Image.Type.Simple;
            art.preserveAspect=true;
            var word=control.GetComponentInChildren<Text>();
            word.font=Eldoria.UI.RoyalGold.V2.CoronaFinish.ResolveFont(false);
            word.fontSize=14;
            word.color=WarmWhite;
            word.alignment=TextAnchor.MiddleCenter;
            word.rectTransform.anchoredPosition=new Vector2(0f,-30f);
            word.rectTransform.sizeDelta=new Vector2(74f,21f);
            Eldoria.UI.RoyalGold.V2.CoronaFinish.FitSingleLine(word,14,11);
            // WOS-style captions sit just below the original sculpted
            // medals, not over the metal/hammer itself. Four-way ivory
            // outline keeps the caption legible against animated grass.
            var outline=word.gameObject.AddComponent<Outline>();
            outline.effectColor=new Color(.075f,.045f,.12f,.92f);
            outline.effectDistance=new Vector2(.65f,-.65f);
            return rect;
        }

        private static Button ButtonPlate(string name, Transform parent, string caption, Sprite skin,
            Vector2 anchor, Vector2 pivot, Vector2 at, Vector2 size,
            UnityEngine.Events.UnityAction click)
        {
            var rect = Plate(name, parent, skin, anchor, pivot, at, size);
            var target = rect.GetComponent<Image>();
            target.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = target;
            Eldoria.UI.RoyalGold.V2.CoronaFinish.ApplyButtonFeedback(button);
            Label(caption, rect, 14, false,
                name == "Execute canonical Build command" ? DarkInk : PaleGold,
                TextAnchor.MiddleCenter, Vector2.zero, new Vector2(size.x - 12f, size.y - 5f));
            button.onClick.AddListener(click);
            return button;
        }

        private static Text Label(string content, Transform parent, int fontSize, bool inscription,
            Color color, TextAnchor align, Vector2 at, Vector2 size)
        {
            var go = new GameObject("Text " + content, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = at;
            rect.sizeDelta = size;
            var label = go.GetComponent<Text>();
            label.font = Eldoria.UI.RoyalGold.V2.CoronaFinish.ResolveFont(inscription);
            label.text = content;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = align;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.raycastTarget = false;
            return label;
        }

        /// <summary>
        /// Four tiny, original material plates. No screenshot crops or baked text.
        /// Reusable 9-slice allows responsive resizing without enlarged corners.
        /// </summary>
        private static Sprite NeedCorona(string key)
        {
            var sprite=Resources.Load<Sprite>("ValoriaModular25D/UI/CoronaBrecha/"+key);
            if(sprite==null)
                throw new InvalidOperationException("Missing original Corona de la Brecha source sprite: "+key+
                    ". Generate kit via tools/valoria-corona-brecha/bake.py; never silently replace it with Umbral II.");
            return sprite;
        }

        // Each illustrated texture has a stable GUID and independent editable
        // source; uses authentic, new UI art rather than old Umbral colors.
        private static void InitializeArt()
        {
            if(slatePlate!=null)return;
            topVelvetPlate=NeedCorona("top_velvet_ribbon");
            activityPanelPlate=NeedCorona("activity_panel_wide");
            slatePlate=NeedCorona("world_rune");
            resourcePlate=NeedCorona("wallet_inset");
            portraitPlate=NeedCorona("portrait_ring");
            navPlate=NeedCorona("nav_medallion");
            actionPlate=NeedCorona("action_arcane");
            activityStandard=NeedCorona("activity_standard");
            woodIcon=Eldoria.UI.RoyalGold.V2.CoronaFinish.ResourceIcon("wood")??NeedCorona("icon_wood");
            stoneIcon=Eldoria.UI.RoyalGold.V2.CoronaFinish.ResourceIcon("stone")??NeedCorona("icon_stone");
            foodIcon=Eldoria.UI.RoyalGold.V2.CoronaFinish.ResourceIcon("food")??NeedCorona("icon_food");
            hammerIcon=NeedCorona("icon_hammer");
            cityIcon=NeedCorona("icon_city");
            taskIcon=NeedCorona("icon_task");
        }

        private static Sprite MaterialSprite(string name, Color32 bottom, Color32 top,
            Color32 trim, byte opaque, int variant)
        {
            const int n = 64;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false, false);
            tex.name = name + " source texture";
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    int edge = Mathf.Min(Mathf.Min(x, n - 1 - x), Mathf.Min(y, n - 1 - y));
                    int corner = Mathf.Min(x, n - 1 - x) + Mathf.Min(y, n - 1 - y);
                    if (corner < 4)
                    {
                        px[y * n + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }
                    Color mixed = Color.Lerp(bottom, top, y / 63f);
                    int noise = (x * 37 + y * 71 + x * y * 11 + variant * 23) & 7;
                    float fleck = (noise - 3.5f) / 255f;
                    if (edge < 2)
                        mixed = Color.Lerp(mixed, trim, .82f);
                    else if (edge == 3)
                        mixed = Color.Lerp(mixed, trim, variant == 2 ? .92f : .63f);
                    else if (edge < 6)
                        mixed *= variant == 2 ? .94f : .77f;
                    if (variant == 2 && y > 8 && y < 56 && x > 8 && x < 56)
                        mixed *= .985f;
                    px[y * n + x] = new Color(
                        Mathf.Clamp01(mixed.r + fleck), Mathf.Clamp01(mixed.g + fleck),
                        Mathf.Clamp01(mixed.b + fleck), opaque / 255f);
                }
            }
            tex.SetPixels32(px);
            tex.Apply(false, false);
            var sprite = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(.5f, .5f),
                100f, 0, SpriteMeshType.FullRect, new Vector4(10, 10, 10, 10));
            sprite.name = name;
            return sprite;
        }
    }
}




