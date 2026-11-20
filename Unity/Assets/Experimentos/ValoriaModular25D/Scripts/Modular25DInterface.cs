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
        private Text title, level, woodValue, stoneValue, foodValue;
        private Text stage, cost, remaining, actionLabel, notice, objectiveText, stateSummary;
        private Button action;
        private Image woodAccent;
        private float nextTick, noticeExpires;
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
                new Vector2(194f, 68f));
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
            title = Label("VALORIA", identity, 19, true, WarmWhite, TextAnchor.MiddleLeft,
                new Vector2(44f, 9f), new Vector2(100f, 29f));
            level = Label("BASTIÓN I", identity, 12, false, PaleGold, TextAnchor.MiddleLeft,
                new Vector2(44f, -17f), new Vector2(100f, 28f));
            // SourceSans line metrics exceed the 20px legacy plate: Truncate
            // silently discarded all small-glyph geometry in actual WebGL.
            // Let ascenders/descenders use the designed clear lower half.
            level.verticalOverflow = VerticalWrapMode.Overflow;

            resourceRow = Plate("Independent resources", rootTransform, null,
                new Vector2(.5f, 1f), new Vector2(.5f, 1f), Vector2.zero,
                new Vector2(365f, 44f));
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
            // A real progression prompt, not a fictional quest/reward.
            var missionIcon=Plate("Original real objective scroll",objective,taskIcon,
                new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                new Vector2(-64f,0f),new Vector2(28f,28f));
            missionIcon.GetComponent<Image>().type=Image.Type.Simple;
            missionIcon.GetComponent<Image>().preserveAspect=true;
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
                () => gateway.BuildSawmill());
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
            if (buildingSheet != null && buildingSheet.activeSelf)
                Refresh(gateway.Snapshot());
        }

        private void Feedback(string value)
        {
            if (noticeRoot == null) return;
            value = value ?? "";
            if (value.StartsWith("Partida experimental aislada", StringComparison.OrdinalIgnoreCase))
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
            if(sawmillTag!=null && sawmillTagText!=null)
            {
                var building=ParcelBuildingStates.For(s,"sawmill");
                sawmillTagText.text=building==ParcelBuildingState.BUILT
                    ? "ASERRADERO · "+s.SawmillLevel
                    : building==ParcelBuildingState.UNDER_CONSTRUCTION
                        ? "OBRA EN CURSO" : "ASERRADERO";
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
            woodAccent.color = s.Resources.Wood < SliceRules.SawmillWoodCost ? Red : AntiqueBrass;
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
            // On the genuine 390x844 build the old coordinates placed
            // BASTIÓN on top of the civic well and ASERRADERO far below the
            // actual footprint. Bind to the western keep tower and near the
            // sawmill parcel edge instead; these are world coordinates, not
            // fixed screenshot pixels, and continue to track pan/zoom.
            // The north-west anchor is also left of the quest overlay.
            PlaceWorldNameplate(bastionTag,baseRect,
                new Vector3(-1.63f,3.05f,0f),hiddenByPanel);
            PlaceWorldNameplate(sawmillTag,baseRect,
                new Vector3(.96f,-2.90f,0f),hiddenByPanel);
        }

        private void PlaceWorldNameplate(RectTransform nameplate,RectTransform canvasRect,
            Vector3 worldPosition,bool modalOpen)
        {
            var cam=world.ViewCamera;
            var screen=cam.WorldToScreenPoint(worldPosition);
            var inside=screen.z>0&&
                screen.x>75f&&screen.x<Screen.width-75f&&
                screen.y>110f&&screen.y<Screen.height-130f;
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

        private void Layout()
        {
            if (hudCanvas == null || identity == null) return;
            int w = Screen.width, h = Screen.height;
            if (w <= 0 || h <= 0) return;
            float scale = Mathf.Max(.01f, hudCanvas.scaleFactor);
            var safe = Screen.safeArea;
            float top = Mathf.Max(0f, (h - safe.yMax) / scale) + 8f;
            float bottom = Mathf.Max(0f, safe.yMin / scale) + 8f;
            bool wide = w > h;

            identity.anchorMin = identity.anchorMax = new Vector2(0f, 1f);
            identity.pivot = new Vector2(0f, 1f);
            identity.anchoredPosition = new Vector2(10f, -top);
            identity.sizeDelta = new Vector2(194f, 68f);

            resourceRow.anchorMin = resourceRow.anchorMax =
                wide ? new Vector2(1f, 1f) : new Vector2(.5f, 1f);
            resourceRow.pivot = wide ? new Vector2(1f, 1f) : new Vector2(.5f, 1f);
            resourceRow.anchoredPosition = wide ?
                new Vector2(-10f, -top) : new Vector2(0f, -top - 98f);
            resourceRow.sizeDelta = new Vector2(365f, 44f);
            for (int i = 0; i < resourceChips.Length; i++)
            {
                resourceChips[i].anchorMin = resourceChips[i].anchorMax =
                    new Vector2(0f, .5f);
                resourceChips[i].pivot = new Vector2(0f, .5f);
                float cw = 117f;
                resourceChips[i].sizeDelta = new Vector2(cw, 43f);
                resourceChips[i].anchoredPosition = new Vector2(i * (cw + 6f), 0f);
            }

            inspect.anchorMin=inspect.anchorMax=new Vector2(0f,0f);
            inspect.pivot=new Vector2(0f,0f);
            inspect.sizeDelta=new Vector2(86f,80f);
            inspect.anchoredPosition=new Vector2(12f,bottom+8f);
            center.anchorMin=center.anchorMax=new Vector2(1f,0f);
            center.pivot=new Vector2(1f,0f);
            center.sizeDelta=new Vector2(86f,80f);
            center.anchoredPosition=new Vector2(-12f,bottom+8f);
            statusDock.anchorMin=statusDock.anchorMax=new Vector2(.5f,0f);
            statusDock.pivot=new Vector2(.5f,0f);
            statusDock.sizeDelta=new Vector2(86f,80f);
            statusDock.anchoredPosition=new Vector2(0f,bottom+8f);
            objective.anchorMin=objective.anchorMax=
                wide?new Vector2(0f,1f):new Vector2(1f,1f);
            objective.pivot=wide?new Vector2(0f,1f):new Vector2(1f,1f);
            objective.sizeDelta=new Vector2(172f,50f);
            objective.anchoredPosition=wide?new Vector2(12f,-top-83f):
                new Vector2(-12f,-top-139f);

            if (wide)
            {
                // Never build a full-width drawer across a 390px-tall viewport.
                sheet.anchorMin = sheet.anchorMax = new Vector2(1f, .5f);
                sheet.pivot = new Vector2(1f, .5f);
                sheet.anchoredPosition = new Vector2(-12f, 2f);
                sheet.sizeDelta = new Vector2(326f, 240f);
                noticeRoot.anchorMin = noticeRoot.anchorMax = new Vector2(0f, 1f);
                noticeRoot.pivot = new Vector2(0f, 1f);
                noticeRoot.anchoredPosition = new Vector2(12f, -top - 146f);
                noticeRoot.sizeDelta = new Vector2(290f, 35f);
                statusSheet.anchorMin=statusSheet.anchorMax=new Vector2(1f,.5f);
                statusSheet.pivot=new Vector2(1f,.5f);
                statusSheet.anchoredPosition=new Vector2(-12f,2f);
                statusSheet.sizeDelta=new Vector2(328f,170f);
            }
            else
            {
                sheet.anchorMin = sheet.anchorMax = new Vector2(.5f, 0f);
                sheet.pivot = new Vector2(.5f, 0f);
                sheet.anchoredPosition = new Vector2(0f, bottom + 95f);
                sheet.sizeDelta = new Vector2(Mathf.Min(355f, w / scale - 24f), 244f);
                noticeRoot.anchorMin = noticeRoot.anchorMax = new Vector2(.5f, 1f);
                noticeRoot.pivot = new Vector2(.5f, 1f);
                noticeRoot.anchoredPosition = new Vector2(0f, -top - 221f);
                noticeRoot.sizeDelta = new Vector2(324f, 35f);
                statusSheet.anchorMin=statusSheet.anchorMax=new Vector2(.5f,0f);
                statusSheet.pivot=new Vector2(.5f,0f);
                statusSheet.anchoredPosition=new Vector2(0f,bottom+95f);
                statusSheet.sizeDelta=new Vector2(Mathf.Min(330f,w/scale-28f),170f);
            }
            lastWidth = w;
            lastHeight = h;
            lastSafeArea = safe;
        }

        private static RectTransform CreateResource(Transform parent, int index, string caption,
            Sprite art, out Text value, out Image icon)
        {
            var chip = Plate("Resource " + caption, parent, resourcePlate,
                new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(index * 123f, 0f), new Vector2(117f, 43f));
            var medallion = Plate(caption + " seal", chip, portraitPlate,
                new Vector2(0f, .5f), new Vector2(0f, .5f),
                new Vector2(19f, 0f), new Vector2(27f, 27f));
            icon = medallion.GetComponent<Image>();
            var symbol=Plate(caption + " icon", medallion, art,
                new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero, new Vector2(22f,22f));
            symbol.GetComponent<Image>().type=Image.Type.Simple;
            symbol.GetComponent<Image>().color=Color.white;
            // Native Chromium QA found completely EMPTY counters despite
            // correct canonical snapshots. 14/20px-high Text rects with
            // VerticalWrapMode.Truncate can drop the entire line for this
            // font's actual line-height at phone scale. Keep the two-line
            // ledger but allocate enough baseline room, and do not truncate
            // glyphs at the chip's own edge.
            var captionText = Label(caption, chip, 10, false, PaleGold,
                TextAnchor.MiddleLeft, new Vector2(23f, 10f), new Vector2(77f, 24f));
            captionText.verticalOverflow = VerticalWrapMode.Overflow;
            value = Label("0", chip, 16, false, WarmWhite,
                TextAnchor.MiddleLeft, new Vector2(23f, -8f), new Vector2(80f, 32f));
            value.verticalOverflow = VerticalWrapMode.Overflow;
            value.resizeTextForBestFit = true;
            value.resizeTextMinSize = 12;
            value.resizeTextMaxSize = 16;
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
            // A two-vertex outline improves small text on the sculpted lower
            // part of the medal without a second rectangle or game asset.
            var shadow=word.gameObject.AddComponent<Shadow>();
            shadow.effectColor=new Color(.055f,.035f,.09f,.9f);
            shadow.effectDistance=new Vector2(1f,-1f);
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
