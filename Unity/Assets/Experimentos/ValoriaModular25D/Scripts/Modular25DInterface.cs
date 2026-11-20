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
    /// actions (inspect the sawmill and reset the camera). No invented systems.
    /// New 9-slice materials are created once as small in-memory textures;
    /// the protected source art, game rules and persistence stay unchanged.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed class Modular25DInterface : MonoBehaviour
    {
        public const string UiEdition = "Corona de la Brecha - Eldoria 2.5D";
        public const int FutureNavigationSlots = 6;
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

        private static Sprite slatePlate, parchmentPlate, actionPlate, resourcePlate, portraitPlate, burgundyPlate, identityPlate, navPlate;
        private static Sprite woodIcon, stoneIcon, foodIcon, hammerIcon, cityIcon, taskIcon;
        private static Texture2D originalIconAtlas;
        private Modular25DGameplay gateway;
        private GameObject buildingSheet;
        private Canvas hudCanvas;
        private RectTransform identity, resourceRow, inspect, center, statusDock, sheet, statusSheet, objective, noticeRoot;
        // World-anchored, tap-enabled nameplates: standard strategy-game
        // readability without hand-painted text embedded in the scenery.
        private RectTransform bastionTag, sawmillTag;
        private Text bastionTagText, sawmillTagText;
        private Modular25DWorld world;
        private RectTransform[] resourceChips;
        private RectTransform constructionProgressRoot,constructionProgressFill;
        private RectTransform worldProgressTrack,worldProgressFill;
        private Text title, level, woodValue, stoneValue, foodValue;
        private Text stage, cost, remaining, actionLabel, notice, objectiveText, stateSummary;
        private Button action;
        private Image woodAccent;
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
            InitializeArt();
            var root = new GameObject("CORONA DE LA BRECHA | HUD original",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            hudCanvas = root.GetComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 240;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390f, 844f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .3f;
            var rootTransform = root.transform;

            // Header is NOT one dark full-width strip. The game view remains
            // visible between a short identity seal and three resource chips.
            identity = Plate("Valoria identity", rootTransform, identityPlate,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, -12f),
                new Vector2(182f, 82f));
            var sigil = Plate("Sir Aldric portrait bezel", identity, portraitPlate,
                new Vector2(0f, .5f), new Vector2(.5f, .5f), new Vector2(34f, 0f),
                new Vector2(59f, 59f));
            var aldricAvatar=Resources.Load<Sprite>(
                "ValoriaModular25D/UI/UmbralII/aldric_avatar");
            if(aldricAvatar==null)
                throw new InvalidOperationException(
                    "Umbral II requires Eldoria's authentic Sir Aldric portrait.");
            var face=Plate("Authentic Sir Aldric character portrait",sigil,aldricAvatar,
                new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(44f,44f));
            face.GetComponent<Image>().type=Image.Type.Simple;
            face.GetComponent<Image>().preserveAspect=true;
            // Original 580x260 painted crest reserves its right inset for
            // the text, beginning at ~54% of image width. The previous label
            // began before this text-safe enamel and overprinted the jewel.
            title = Label("VALORIA", identity, 19, true, WarmWhite, TextAnchor.MiddleLeft,
                new Vector2(51f, 9f), new Vector2(78f, 29f));
            title.resizeTextForBestFit=true;
            title.resizeTextMinSize=16;
            title.resizeTextMaxSize=19;
            level = Label("BASTIÓN I", identity, 12, false, PaleGold, TextAnchor.MiddleLeft,
                new Vector2(51f, -17f), new Vector2(78f, 28f));
            // SourceSans line metrics exceed the 20px legacy plate: Truncate
            // silently discarded all small-glyph geometry in actual WebGL.
            // Let ascenders/descenders use the designed clear lower half.
            level.verticalOverflow = VerticalWrapMode.Overflow;

            resourceRow = Plate("Independent resources", rootTransform, null,
                new Vector2(.5f, 1f), new Vector2(.5f, 1f), Vector2.zero,
                new Vector2(365f, 52f));
            resourceChips = new RectTransform[3];
            resourceChips[0] = CreateResource(resourceRow, 0, "MADERA", woodIcon, out woodValue, out woodAccent);
            Image stoneAccent;
            resourceChips[1] = CreateResource(resourceRow, 1, "PIEDRA", stoneIcon, out stoneValue, out stoneAccent);
            Image foodAccent;
            resourceChips[2] = CreateResource(resourceRow, 2, "COMIDA", foodIcon, out foodValue, out foodAccent);

            // Each building now communicates its interactive status in world
            // space like a commercial 4X city. These labels MOVE with the
            // camera and point only to canonical, currently implemented UI.
            bastionTag=ButtonPlate("Bastion world nameplate",rootTransform,"BASTIÓN · I",
                slatePlate,new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                Vector2.zero,new Vector2(150f,34f),OpenStatus).GetComponent<RectTransform>();
            bastionTagText=bastionTag.GetComponentInChildren<Text>(true);
            bastionTagText.fontSize=14;
            bastionTagText.color=PaleGold;
            // Building nameplates are wayfinding, NOT a second enormous
            // turquoise Build CTA. The actual action stays in the real modal.
            // Keep the two original art plates short/readable in world-space.
            var keepGlyph=Plate("Bastion nameplate original icon",bastionTag,cityIcon,
                new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                new Vector2(-57f,0f),new Vector2(23f,23f));
            keepGlyph.GetComponent<Image>().preserveAspect=true;
            bastionTagText.rectTransform.anchoredPosition=new Vector2(10f,0f);
            bastionTagText.rectTransform.sizeDelta=new Vector2(120f,30f);
            sawmillTag=ButtonPlate("Sawmill world nameplate",rootTransform,"ASERRADERO",
                slatePlate,new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                Vector2.zero,new Vector2(180f,44f),OpenSawmill).GetComponent<RectTransform>();
            sawmillTagText=sawmillTag.GetComponentInChildren<Text>(true);
            sawmillTagText.fontSize=13;
            sawmillTagText.color=PaleGold;
            sawmillTagText.rectTransform.anchoredPosition=new Vector2(11f,0f);
            sawmillTagText.rectTransform.sizeDelta=new Vector2(146f,32f);
            var sawmillGlyph=Plate("Sawmill nameplate original icon",sawmillTag,hammerIcon,
                new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                new Vector2(-68f,0f),new Vector2(24f,24f));
            sawmillGlyph.GetComponent<Image>().preserveAspect=true;
            // Visible AFTER the successful Build auto-dismiss: real progress
            // belongs on the construction parcel, not a persistent modal.
            // This understated track has no new game clock or invented rewards.
            worldProgressTrack=Plate("World sawmill canonical progress track",sawmillTag,null,
                new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                new Vector2(11f,-15f),new Vector2(120f,6f));
            worldProgressTrack.GetComponent<Image>().color=new Color32(17,11,32,226);
            worldProgressFill=Plate("World sawmill elapsed canonical fill",worldProgressTrack,null,
                new Vector2(0f,.5f),new Vector2(0f,.5f),
                new Vector2(2f,0f),new Vector2(2f,4f));
            worldProgressFill.GetComponent<Image>().color=new Color32(76,211,192,255);
            worldProgressTrack.gameObject.SetActive(false);

            // Two contextual, accessible actions; no false shortcuts to
            // heroes/world/bag until the gameplay genuinely exposes them.
            // Commercial strategy-game grammar: three illustrated action tiles
            // and one honest contextual task, not an app-style navigation bar.
            // Every available action has a real behaviour from the sandbox.
            inspect = DockButton("Inspect sawmill",rootTransform,"OBRAS",hammerIcon,
                new Vector2(0f,0f),new Vector2(0f,0f),OpenSawmill);
            center = DockButton("Center camera",rootTransform,"CIUDAD",cityIcon,
                new Vector2(1f,0f),new Vector2(1f,0f),
                () => GetComponent<Modular25DCameraInput>().Home());
            statusDock = DockButton("Inspect city status",rootTransform,"ESTADO",taskIcon,
                new Vector2(.5f,0f),new Vector2(.5f,0f),OpenStatus);
            objective=ButtonPlate("Current real construction objective",rootTransform,"",
                burgundyPlate,new Vector2(1f,1f),new Vector2(1f,1f),
                new Vector2(-12f,-122f),new Vector2(172f,50f),OpenSawmill)
                .GetComponent<RectTransform>();
            // The premium objective_rune source ALREADY includes its own
            // sculpted hanging parchment insignia. Placing a second
            // illustrated task medallion on top produced a visually noisy
            // double icon, unlike commercial strategy quest plaques.
            // Its existing Button keeps the real OpenSawmill action.
            Label("PRÓXIMO PASO",objective,11,true,PaleGold,TextAnchor.MiddleLeft,
                new Vector2(19f,11f),new Vector2(118f,16f));
            objectiveText=Label("ASERRADERO · 0/1",objective,13,false,WarmWhite,
                TextAnchor.MiddleLeft,new Vector2(19f,-8f),new Vector2(118f,24f));

            // Context sheet uses warm architectural parchment, a real sprite
            // from the experiment, and original data. Not a generic modal.
            sheet = Plate("Sawmill parchment sheet", rootTransform, parchmentPlate,
                new Vector2(.5f, 0f), new Vector2(.5f, 0f), new Vector2(0f, 110f),
                new Vector2(355f, 244f));
            buildingSheet = sheet.gameObject;
            Label("ASERRADERO", sheet, 23, true, WarmWhite, TextAnchor.MiddleLeft,
                new Vector2(-30f, 84f), new Vector2(190f, 31f));
            Label("TALLER DE VALORIA", sheet, 11, false, PaleGold, TextAnchor.MiddleLeft,
                new Vector2(-30f, 61f), new Vector2(200f, 17f));
            AccentHairline("Divider", sheet, AntiqueBrass,
                new Vector2(0f, 44f), new Vector2(270f, 1.5f));
            var previewPlate = Plate("Architectural preview", sheet, portraitPlate,
                new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(-84f, -6f),
                new Vector2(96f, 99f));
            var view = Plate("Real illustrated sawmill", previewPlate, null,
                new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero,
                new Vector2(93f, 94f));
            var preview = view.GetComponent<Image>();
            preview.sprite = Modular25DArt.Sawmill();
            preview.preserveAspect = true;
            preview.type = Image.Type.Simple;
            preview.color = Color.white;
            preview.raycastTarget = false;
            stage = Label("", sheet, 13, false, WarmWhite, TextAnchor.MiddleLeft,
                new Vector2(56f, 26f), new Vector2(185f, 22f));
            cost = Label("", sheet, 13, false, MutedInk, TextAnchor.MiddleLeft,
                new Vector2(56f, 0f), new Vector2(185f, 19f));
            remaining = Label("", sheet, 13, false, WarmWhite, TextAnchor.MiddleLeft,
                new Vector2(56f, -28f), new Vector2(185f, 19f));

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
                new Vector2(0f, -80f), new Vector2(270f, 42f),
                () =>
                {
                    // Only a successful real LocalGateway construction
                    // dismisses the panel. Failed commands keep the panel
                    // open so the feedback is actionable. The worksite,
                    // progress clock and spend persist through the canonical
                    // gateway; no synthetic game state is introduced.
                    var result=gateway.BuildSawmill();
                    if(result.Ok)ClosePanels();
                });
            actionLabel = action.GetComponentInChildren<Text>();
            var close = ButtonPlate("Close sawmill", sheet, "×", navPlate,
                new Vector2(1f, 1f), new Vector2(.5f, .5f),
                new Vector2(-27f, -25f), new Vector2(46f, 46f),
                ClosePanels);
            var closeText = close.GetComponentInChildren<Text>();
            closeText.fontSize = 25;
            buildingSheet.SetActive(false);

            statusSheet=Plate("Valoria state parchment",rootTransform,parchmentPlate,
                new Vector2(.5f,0f),new Vector2(.5f,0f),
                new Vector2(0f,112f),new Vector2(330f,170f));
            Label("ESTADO DE VALORIA",statusSheet,23,true,WarmWhite,TextAnchor.MiddleCenter,
                new Vector2(0f,57f),new Vector2(278f,28f));
            AccentHairline("State divider",statusSheet,AntiqueBrass,
                new Vector2(0f,35f),new Vector2(281f,1.5f));
            stateSummary=Label("",statusSheet,14,false,WarmWhite,TextAnchor.MiddleLeft,
                new Vector2(0f,-15f),new Vector2(278f,83f));
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
            Refresh(gateway.Snapshot());
            Feedback(gateway.LastMessage);
            SyncWorldNameplates();
        }

        public void OpenSawmill()
        {
            if(buildingSheet==null)return;
            if(statusSheet!=null)statusSheet.gameObject.SetActive(false);
            buildingSheet.SetActive(true);
            SetModalChrome();
            Refresh(gateway.Snapshot());
        }

        public void OpenStatus()
        {
            if(statusSheet==null)return;
            if(buildingSheet!=null)buildingSheet.SetActive(false);
            statusSheet.gameObject.SetActive(true);
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
        // region. Showing the CIUDAD dock under it created overlapping hit
        // targets. Hide ALL three nav medallions while a contextual sheet is
        // open; they return immediately through the real close control.
        // No gameplay action or backend state changes here.
        private void SetModalChrome()
        {
            bool open=(buildingSheet!=null&&buildingSheet.activeSelf)||
                      (statusSheet!=null&&statusSheet.gameObject.activeSelf);
            if(inspect!=null)inspect.gameObject.SetActive(!open);
            if(center!=null)center.gameObject.SetActive(!open);
            if(statusDock!=null)statusDock.gameObject.SetActive(!open);
            if(objective!=null)objective.gameObject.SetActive(!open);
        }

        private void Update()
        {
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
            // The Aserradero worksite timer remains readable on the world
            // even though Build closes its panel. Query at 4 Hz only during
            // canonical construction or an explicitly opened detail sheet.
            if(gateway!=null && (cachedSawmillPhase==ParcelBuildingState.UNDER_CONSTRUCTION ||
                (buildingSheet!=null&&buildingSheet.activeSelf)))
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

        private void Refresh(PlayerState s)
        {
            if (woodValue == null) return;
            if (s == null)
            {
                woodValue.text = stoneValue.text = foodValue.text = "—";
                if (action != null) action.interactable = false;
                return;
            }
            level.text = "BASTIÓN " + s.BastionLevel;
            if(bastionTagText!=null)
                bastionTagText.text="BASTIÓN · "+s.BastionLevel;
            cachedSawmillPhase=ParcelBuildingStates.For(s,"sawmill");
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
                sawmillTagText.rectTransform.anchoredPosition=
                    new Vector2(11f,constructing?5f:0f);
                sawmillTagText.rectTransform.sizeDelta=
                    new Vector2(146f,constructing?23f:32f);
                if(worldProgressTrack!=null)
                {
                    worldProgressTrack.gameObject.SetActive(constructing);
                    if(constructing && worldProgressFill!=null)
                    {
                        float duration=Mathf.Max(1f,(float)SliceRules.SawmillBuildSeconds);
                        float elapsed=Mathf.Clamp01(1f-(float)timeLeft/duration);
                        worldProgressFill.sizeDelta=new Vector2(
                            Mathf.Max(2f,116f*elapsed),4f);
                    }
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
            woodValue.text = s.Resources.Wood.ToString("N0");
            stoneValue.text = s.Resources.Stone.ToString("N0");
            foodValue.text = s.Resources.Food.ToString("N0");
            // Preserve the artist's original full-color logs. Tinting the
            // *whole* premium illustration with AntiqueBrass destroyed its
            // wood/rope/material separation. Only numbers convey shortages.
            woodAccent.color=Color.white;
            woodValue.color=s.Resources.Wood<SliceRules.SawmillWoodCost
                ? Red : WarmWhite;
            if(objectiveText!=null)
                objectiveText.text=s.SawmillLevel>0?"ASERRADERO · 1/1":
                    s.BuildingCompletesUtcTicks>0?"OBRA EN CURSO":"ASERRADERO · 0/1";
            if(stateSummary!=null)
                stateSummary.text="Bastión " + s.BastionLevel + "  ·  Aserradero " + s.SawmillLevel +
                    "\nMadera: " + s.Resources.Wood.ToString("N0") +
                    "   Piedra: " + s.Resources.Stone.ToString("N0") +
                    "\nComida: " + s.Resources.Food.ToString("N0");
            if (buildingSheet == null || !buildingSheet.activeSelf) return;
            var phase = ParcelBuildingStates.For(s, "sawmill");
            if(constructionProgressRoot!=null)
                constructionProgressRoot.gameObject.SetActive(
                    phase==ParcelBuildingState.UNDER_CONSTRUCTION);
            cost.text = "MADERA · " + SliceRules.SawmillWoodCost.ToString("N0") + " necesarias";
            switch (phase)
            {
                case ParcelBuildingState.BUILT:
                    stage.text = "CONSTRUIDO · NIVEL " + s.SawmillLevel;
                    cost.text = "Producción activa";
                    remaining.text = "La obra está terminada";
                    actionLabel.text = "CONSTRUIDO";
                    action.interactable = false;
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
            // Actual three-state WebGL evidence: the unbuilt plot finishes
            // near screen y~570, while the completed sawmill occupies screen
            // y~450..625. One fixed plaque would overlay the built workshop.
            // Above the western keep tower, the Bastion label also avoids
            // the civic well and the quest overlay at the upper right.
            PlaceWorldNameplate(bastionTag,baseRect,
                new Vector3(-1.63f,3.05f,0f),hiddenByPanel);
            float millFootY=cachedSawmillPhase==ParcelBuildingState.BUILT
                ? -3.86f : cachedSawmillPhase==ParcelBuildingState.UNDER_CONSTRUCTION
                    ? -3.45f : -2.90f;
            PlaceWorldNameplate(sawmillTag,baseRect,
                new Vector3(.96f,millFootY,0f),hiddenByPanel);
        }

        private void PlaceWorldNameplate(RectTransform nameplate,RectTransform canvasRect,
            Vector3 worldPosition,bool modalOpen)
        {
            var cam=world.ViewCamera;
            var screen=cam.WorldToScreenPoint(worldPosition);
            var safe=Screen.safeArea;
            var inside=screen.z>0&&
                screen.x>safe.xMin+68f&&screen.x<safe.xMax-68f&&
                screen.y>safe.yMin+105f&&screen.y<safe.yMax-125f;
            if(nameplate.gameObject.activeSelf!=inside&&!modalOpen)
                nameplate.gameObject.SetActive(inside);
            if(modalOpen)
            {
                if(nameplate.gameObject.activeSelf)nameplate.gameObject.SetActive(false);
                return;
            }
            if(!inside)return;
            Vector2 local;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,new Vector2(screen.x,screen.y),null,out local))
            {
                nameplate.anchorMin=nameplate.anchorMax=new Vector2(.5f,.5f);
                nameplate.pivot=new Vector2(.5f,.5f);
                nameplate.anchoredPosition=local;
            }
        }


        // Six stable slots at the lower edge. Only slots 3, 4 and 5
        // currently have real actions (Estado, Ciudad, Obras). Slots
        // 0-2 are reserved positions, NOT fake or disabled buttons.
        public static Rect NavigationCell(int index,float logicalWidth,
            float leftInset,float rightInset,float bottomInset,bool wide)
        {
            if(index<0 || index>=FutureNavigationSlots)
                throw new ArgumentOutOfRangeException(nameof(index));
            float safeWidth=logicalWidth-leftInset-rightInset;
            float step=Mathf.Min(wide?54f:60f,
                Mathf.Max(49f,(safeWidth-12f)/FutureNavigationSlots));
            float diameter=Mathf.Max(44f,step-5f);
            float origin=logicalWidth-rightInset-8f-FutureNavigationSlots*step;
            return new Rect(
                origin+index*step+(step-diameter)*.5f,
                bottomInset+(wide?12f:14f),diameter,diameter+12f);
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
                caption.fontSize=wide?9:10;
                caption.alignment=TextAnchor.MiddleCenter;
                caption.rectTransform.anchoredPosition=
                    new Vector2(0f,-cell.width*.5f-9f);
                caption.rectTransform.sizeDelta=new Vector2(cell.width+5f,16f);
            }
        }

        private void Layout()
        {
            if(hudCanvas==null || identity==null)return;
            int w=Screen.width,h=Screen.height;
            if(w<=0 || h<=0)return;
            float scale=Mathf.Max(.01f,hudCanvas.scaleFactor);
            Rect safe=Screen.safeArea;
            float top=Mathf.Max(0f,(h-safe.yMax)/scale)+8f;
            float bottom=Mathf.Max(0f,safe.yMin/scale)+8f;
            float left=Mathf.Max(0f,safe.xMin/scale);
            float right=Mathf.Max(0f,(w-safe.xMax)/scale);
            float logicalWidth=w/scale;
            bool wide=w>h;

            // Economy touches the TOP safe area in portrait and landscape.
            // It is never delayed by a large empty gap under the identity.
            resourceRow.anchorMin=resourceRow.anchorMax=
                wide?new Vector2(1f,1f):new Vector2(.5f,1f);
            resourceRow.pivot=wide?new Vector2(1f,1f):new Vector2(.5f,1f);
            resourceRow.anchoredPosition=wide?
                new Vector2(-12f-right,-top):new Vector2(0f,-top);
            float available=logicalWidth-left-right;
            float economyWidth=wide
                ? Mathf.Min(365f,Mathf.Max(280f,available-214f))
                : Mathf.Min(365f,available-16f);
            resourceRow.sizeDelta=new Vector2(economyWidth,52f);
            float capsuleWidth=(economyWidth-12f)/3f;
            float sourceHeightPerWidth=resourcePlate.rect.height/
                Mathf.Max(1f,resourcePlate.rect.width);
            for(int i=0;i<resourceChips.Length;i++)
            {
                var chip=resourceChips[i];
                chip.anchorMin=chip.anchorMax=new Vector2(0f,.5f);
                chip.pivot=new Vector2(0f,.5f);
                chip.sizeDelta=new Vector2(capsuleWidth,
                    Mathf.Min(52f,capsuleWidth*sourceHeightPerWidth));
                chip.anchoredPosition=new Vector2(i*(capsuleWidth+6f),0f);
                chip.GetComponent<Image>().preserveAspect=true;
            }

            // Landscape: crest left, economy right; portrait: crest is
            // directly below the resource ledger, with no overlap.
            identity.anchorMin=identity.anchorMax=new Vector2(0f,1f);
            identity.pivot=new Vector2(0f,1f);
            identity.anchoredPosition=new Vector2(10f+left,
                wide?-top:-top-58f);
            identity.sizeDelta=new Vector2(182f,82f);

            // 3 active medallions in a scalable grid of six stable cells.
            // When systems unlock, use the spare left-hand cells.
            PlaceNavigation(statusDock,NavigationCell(3,logicalWidth,left,right,bottom,wide),wide);
            PlaceNavigation(center,NavigationCell(4,logicalWidth,left,right,bottom,wide),wide);
            PlaceNavigation(inspect,NavigationCell(5,logicalWidth,left,right,bottom,wide),wide);

            objective.anchorMin=objective.anchorMax=
                wide?new Vector2(0f,1f):new Vector2(0f,0f);
            objective.pivot=wide?new Vector2(0f,1f):new Vector2(0f,0f);
            objective.sizeDelta=new Vector2(185f,72f);
            objective.anchoredPosition=wide?
                new Vector2(12f+left,-top-85f):
                new Vector2(10f+left,bottom+88f);

            if(wide)
            {
                sheet.anchorMin=sheet.anchorMax=new Vector2(1f,.5f);
                sheet.pivot=new Vector2(1f,.5f);
                sheet.anchoredPosition=new Vector2(-12f-right,2f);
                sheet.sizeDelta=new Vector2(326f,240f);
                noticeRoot.anchorMin=noticeRoot.anchorMax=new Vector2(0f,1f);
                noticeRoot.pivot=new Vector2(0f,1f);
                noticeRoot.anchoredPosition=new Vector2(12f+left,-top-167f);
                noticeRoot.sizeDelta=new Vector2(290f,35f);
                statusSheet.anchorMin=statusSheet.anchorMax=new Vector2(1f,.5f);
                statusSheet.pivot=new Vector2(1f,.5f);
                statusSheet.anchoredPosition=new Vector2(-12f-right,2f);
                statusSheet.sizeDelta=new Vector2(328f,170f);
            }
            else
            {
                sheet.anchorMin=sheet.anchorMax=new Vector2(.5f,0f);
                sheet.pivot=new Vector2(.5f,0f);
                sheet.anchoredPosition=new Vector2(0f,bottom+95f);
                sheet.sizeDelta=new Vector2(
                    Mathf.Min(355f,logicalWidth-left-right-24f),244f);
                noticeRoot.anchorMin=noticeRoot.anchorMax=new Vector2(.5f,1f);
                noticeRoot.pivot=new Vector2(.5f,1f);
                noticeRoot.anchoredPosition=new Vector2(0f,-top-153f);
                noticeRoot.sizeDelta=new Vector2(324f,35f);
                statusSheet.anchorMin=statusSheet.anchorMax=new Vector2(.5f,0f);
                statusSheet.pivot=new Vector2(.5f,0f);
                statusSheet.anchoredPosition=new Vector2(0f,bottom+95f);
                statusSheet.sizeDelta=new Vector2(
                    Mathf.Min(330f,logicalWidth-left-right-28f),170f);
            }
            lastWidth=w;
            lastHeight=h;
            lastSafeArea=safe;
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
            word.fontSize=11;
            word.color=WarmWhite;
            word.alignment=TextAnchor.MiddleCenter;
            word.rectTransform.anchoredPosition=new Vector2(0f,-30f);
            word.rectTransform.sizeDelta=new Vector2(80f,21f);
            word.horizontalOverflow=HorizontalWrapMode.Overflow;
            word.verticalOverflow=VerticalWrapMode.Overflow;
            // WOS-style captions sit just below the original sculpted
            // medals, not over the metal/hammer itself. Four-way ivory
            // outline keeps the caption legible against animated grass.
            var outline=word.gameObject.AddComponent<Outline>();
            outline.effectColor=new Color(.075f,.045f,.12f,.92f);
            outline.effectDistance=new Vector2(1.25f,-1.25f);
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
            button.transition = Selectable.Transition.ColorTint;
            var colours = button.colors;
            colours.normalColor = Color.white;
            colours.highlightedColor = new Color(1f, .97f, .87f, 1f);
            colours.pressedColor = new Color(.74f, .82f, .77f, 1f);
            colours.disabledColor = new Color(.55f, .57f, .56f, .7f);
            colours.fadeDuration = .09f;
            button.colors = colours;
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
            var fontName = inscription ? "CormorantGaramond-SemiBold" : "SourceSans3-Semibold";
            label.font = Resources.Load<Font>("ValoriaModular25D/UI/Fonts/" + fontName);
            if (label.font == null)
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            identityPlate=NeedCorona("identity_crest");
            slatePlate=NeedCorona("world_rune");
            resourcePlate=NeedCorona("resource_siglet");
            portraitPlate=NeedCorona("portrait_ring");
            navPlate=NeedCorona("nav_medallion");
            parchmentPlate=NeedCorona("panel_obsidian");
            actionPlate=NeedCorona("action_arcane");
            burgundyPlate=NeedCorona("objective_rune");
            woodIcon=NeedCorona("icon_wood");
            stoneIcon=NeedCorona("icon_stone");
            foodIcon=NeedCorona("icon_food");
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
