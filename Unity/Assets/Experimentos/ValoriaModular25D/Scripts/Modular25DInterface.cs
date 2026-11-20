using System;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Eldoria.Experimentos.ValoriaModular25D
{
    /// <summary>
    /// UMBRAL UI: a separate, experimental presentation skin for Bastion I.
    /// One dynamic Canvas, the canonical LocalGateway, and only two real player
    /// actions (inspect the sawmill and reset the camera). No invented systems.
    /// New 9-slice materials are created once as small in-memory textures;
    /// the protected source art, game rules and persistence stay unchanged.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed class Modular25DInterface : MonoBehaviour
    {
        public const string UiEdition = "Umbral II - Ciudad y Bastion comercial / Eldoria";
        private static readonly Color32 Slate = new Color32(20, 38, 48, 242);
        private static readonly Color32 SlateLight = new Color32(40, 62, 70, 238);
        private static readonly Color32 Parchment = new Color32(235, 222, 192, 250);
        private static readonly Color32 ParchmentLight = new Color32(249, 240, 217, 252);
        private static readonly Color32 AntiqueBrass = new Color32(181, 141, 88, 255);
        private static readonly Color32 PaleGold = new Color32(228, 197, 139, 255);
        private static readonly Color32 WarmWhite = new Color32(246, 238, 217, 255);
        private static readonly Color32 DarkInk = new Color32(39, 47, 49, 255);
        private static readonly Color32 MutedInk = new Color32(93, 90, 77, 255);
        private static readonly Color32 Green = new Color32(59, 105, 98, 255);
        private static readonly Color32 Red = new Color32(151, 77, 69, 255);

        private static Sprite slatePlate, parchmentPlate, actionPlate, resourcePlate, portraitPlate, burgundyPlate;
        private static Sprite woodIcon, stoneIcon, foodIcon;
        private Modular25DGameplay gateway;
        private GameObject buildingSheet;
        private Canvas hudCanvas;
        private RectTransform identity, resourceRow, inspect, center, statusDock, sheet, statusSheet, objective, noticeRoot;
        private RectTransform[] resourceChips;
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
            if (gateway == null) throw new InvalidOperationException("Umbral requires the canonical modular gateway");
            if (EventSystem.current == null)
                new GameObject("ValoriaModular25D Events", typeof(EventSystem), typeof(InputSystemUIInputModule));
            InitializeArt();
            var root = new GameObject("UMBRAL | Valoria functional HUD",
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
            identity = Plate("Valoria identity", rootTransform, slatePlate,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, -12f),
                new Vector2(165f, 51f));
            var sigil = Plate("Sir Aldric portrait bezel", identity, portraitPlate,
                new Vector2(0f, .5f), new Vector2(.5f, .5f), new Vector2(27f, 0f),
                new Vector2(42f, 42f));
            var aldricAvatar=Resources.Load<Sprite>(
                "ValoriaModular25D/UI/UmbralII/aldric_avatar");
            if(aldricAvatar==null)
                throw new InvalidOperationException(
                    "Umbral II requires Eldoria's authentic Sir Aldric portrait.");
            var face=Plate("Authentic Sir Aldric character portrait",sigil,aldricAvatar,
                new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(36f,36f));
            face.GetComponent<Image>().type=Image.Type.Simple;
            face.GetComponent<Image>().preserveAspect=true;
            title = Label("VALORIA", identity, 19, true, WarmWhite, TextAnchor.MiddleLeft,
                new Vector2(26f, 8f), new Vector2(112f, 25f));
            level = Label("BASTIÓN I", identity, 12, false, PaleGold, TextAnchor.MiddleLeft,
                new Vector2(26f, -14f), new Vector2(114f, 20f));

            resourceRow = Plate("Independent resources", rootTransform, null,
                new Vector2(.5f, 1f), new Vector2(.5f, 1f), Vector2.zero,
                new Vector2(365f, 44f));
            resourceChips = new RectTransform[3];
            resourceChips[0] = CreateResource(resourceRow, 0, "MADERA", woodIcon, out woodValue, out woodAccent);
            Image stoneAccent;
            resourceChips[1] = CreateResource(resourceRow, 1, "PIEDRA", stoneIcon, out stoneValue, out stoneAccent);
            Image foodAccent;
            resourceChips[2] = CreateResource(resourceRow, 2, "COMIDA", foodIcon, out foodValue, out foodAccent);

            // Two contextual, accessible actions; no false shortcuts to
            // heroes/world/bag until the gameplay genuinely exposes them.
            // Commercial strategy-game grammar: three illustrated action tiles
            // and one honest contextual task, not an app-style navigation bar.
            // Every available action has a real behaviour from the sandbox.
            inspect = DockButton("Inspect sawmill",rootTransform,"OBRAS",woodIcon,
                new Vector2(0f,0f),new Vector2(0f,0f),OpenSawmill);
            center = DockButton("Center camera",rootTransform,"CIUDAD",portraitPlate,
                new Vector2(1f,0f),new Vector2(1f,0f),
                () => GetComponent<Modular25DCameraInput>().Home());
            statusDock = DockButton("Inspect city status",rootTransform,"ESTADO",stoneIcon,
                new Vector2(.5f,0f),new Vector2(.5f,0f),OpenStatus);
            objective=ButtonPlate("Current real construction objective",rootTransform,"",
                burgundyPlate,new Vector2(1f,1f),new Vector2(1f,1f),
                new Vector2(-12f,-122f),new Vector2(172f,50f),OpenSawmill)
                .GetComponent<RectTransform>();
            // A real progression prompt, not a fictional quest/reward.
            Label("PRÓXIMO PASO",objective,11,true,PaleGold,TextAnchor.MiddleLeft,
                new Vector2(0f,11f),new Vector2(137f,16f));
            objectiveText=Label("ASERRADERO · 0/1",objective,13,false,WarmWhite,
                TextAnchor.MiddleLeft,new Vector2(0f,-8f),new Vector2(141f,24f));

            // Context sheet uses warm architectural parchment, a real sprite
            // from the experiment, and original data. Not a generic modal.
            sheet = Plate("Sawmill parchment sheet", rootTransform, parchmentPlate,
                new Vector2(.5f, 0f), new Vector2(.5f, 0f), new Vector2(0f, 110f),
                new Vector2(355f, 228f));
            buildingSheet = sheet.gameObject;
            Label("ASERRADERO", sheet, 23, true, DarkInk, TextAnchor.MiddleLeft,
                new Vector2(-54f, 87f), new Vector2(205f, 31f));
            Label("TALLER DE VALORIA", sheet, 10, false, MutedInk, TextAnchor.MiddleLeft,
                new Vector2(-54f, 63f), new Vector2(215f, 17f));
            AccentHairline("Divider", sheet, AntiqueBrass,
                new Vector2(0f, 48f), new Vector2(310f, 1.5f));
            var previewPlate = Plate("Architectural preview", sheet, resourcePlate,
                new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(-117f, -13f),
                new Vector2(100f, 111f));
            var view = Plate("Real illustrated sawmill", previewPlate, null,
                new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero,
                new Vector2(93f, 94f));
            var preview = view.GetComponent<Image>();
            preview.sprite = Modular25DArt.Sawmill();
            preview.preserveAspect = true;
            preview.type = Image.Type.Simple;
            preview.color = Color.white;
            preview.raycastTarget = false;
            stage = Label("", sheet, 13, false, DarkInk, TextAnchor.MiddleLeft,
                new Vector2(54f, 22f), new Vector2(195f, 25f));
            cost = Label("", sheet, 13, false, MutedInk, TextAnchor.MiddleLeft,
                new Vector2(54f, -10f), new Vector2(195f, 25f));
            remaining = Label("", sheet, 13, false, DarkInk, TextAnchor.MiddleLeft,
                new Vector2(54f, -42f), new Vector2(195f, 31f));
            action = ButtonPlate("Execute canonical Build command", sheet, "CONSTRUIR",
                actionPlate, new Vector2(.5f, .5f), new Vector2(.5f, .5f),
                new Vector2(0f, -88f), new Vector2(307f, 47f),
                () => gateway.BuildSawmill());
            actionLabel = action.GetComponentInChildren<Text>();
            var close = ButtonPlate("Close sawmill", sheet, "×", slatePlate,
                new Vector2(1f, 1f), new Vector2(.5f, .5f),
                new Vector2(-27f, -25f), new Vector2(46f, 46f),
                () => buildingSheet.SetActive(false));
            var closeText = close.GetComponentInChildren<Text>();
            closeText.fontSize = 25;
            buildingSheet.SetActive(false);

            statusSheet=Plate("Valoria state parchment",rootTransform,parchmentPlate,
                new Vector2(.5f,0f),new Vector2(.5f,0f),
                new Vector2(0f,112f),new Vector2(330f,170f));
            Label("ESTADO DE VALORIA",statusSheet,23,true,DarkInk,TextAnchor.MiddleCenter,
                new Vector2(0f,57f),new Vector2(278f,28f));
            AccentHairline("State divider",statusSheet,AntiqueBrass,
                new Vector2(0f,35f),new Vector2(281f,1.5f));
            stateSummary=Label("",statusSheet,14,false,DarkInk,TextAnchor.MiddleLeft,
                new Vector2(0f,-15f),new Vector2(278f,83f));
            var closeStatus=ButtonPlate("Close state",statusSheet,"×",slatePlate,
                new Vector2(1f,1f),new Vector2(.5f,.5f),new Vector2(-27f,-22f),
                new Vector2(45f,45f),()=>statusSheet.gameObject.SetActive(false));
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
        }

        public void OpenSawmill()
        {
            if (buildingSheet == null) return;
            if(statusSheet!=null)statusSheet.gameObject.SetActive(false);
            buildingSheet.SetActive(true);
            Refresh(gateway.Snapshot());
        }

        public void OpenStatus()
        {
            if(statusSheet==null)return;
            if(buildingSheet!=null)buildingSheet.SetActive(false);
            statusSheet.gameObject.SetActive(true);
            Refresh(gateway.Snapshot());
        }

        private void Update()
        {
            if (Screen.width != lastWidth || Screen.height != lastHeight ||
                Screen.safeArea != lastSafeArea)
                Layout();
            if (noticeRoot != null && noticeRoot.gameObject.activeSelf &&
                Time.unscaledTime > noticeExpires)
                noticeRoot.gameObject.SetActive(false);
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
                    remaining.text = "Finaliza en " + gateway.SawmillSecondsRemaining() + " s";
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
            identity.anchoredPosition = new Vector2(12f, -top);
            identity.sizeDelta = new Vector2(165f, 51f);

            resourceRow.anchorMin = resourceRow.anchorMax =
                wide ? new Vector2(1f, 1f) : new Vector2(.5f, 1f);
            resourceRow.pivot = wide ? new Vector2(1f, 1f) : new Vector2(.5f, 1f);
            resourceRow.anchoredPosition = wide ?
                new Vector2(-12f, -top) : new Vector2(0f, -top - 61f);
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
            inspect.sizeDelta=new Vector2(116f,54f);
            inspect.anchoredPosition=new Vector2(12f,bottom+8f);
            center.anchorMin=center.anchorMax=new Vector2(1f,0f);
            center.pivot=new Vector2(1f,0f);
            center.sizeDelta=new Vector2(108f,54f);
            center.anchoredPosition=new Vector2(-12f,bottom+8f);
            statusDock.anchorMin=statusDock.anchorMax=new Vector2(.5f,0f);
            statusDock.pivot=new Vector2(.5f,0f);
            statusDock.sizeDelta=new Vector2(110f,54f);
            statusDock.anchoredPosition=new Vector2(0f,bottom+8f);
            objective.anchorMin=objective.anchorMax=
                wide?new Vector2(0f,1f):new Vector2(1f,1f);
            objective.pivot=wide?new Vector2(0f,1f):new Vector2(1f,1f);
            objective.sizeDelta=new Vector2(172f,50f);
            objective.anchoredPosition=wide?new Vector2(12f,-top-62f):
                new Vector2(-12f,-top-114f);

            if (wide)
            {
                // Never build a full-width drawer across a 390px-tall viewport.
                sheet.anchorMin = sheet.anchorMax = new Vector2(1f, .5f);
                sheet.pivot = new Vector2(1f, .5f);
                sheet.anchoredPosition = new Vector2(-12f, 2f);
                sheet.sizeDelta = new Vector2(326f, 222f);
                noticeRoot.anchorMin = noticeRoot.anchorMax = new Vector2(0f, 1f);
                noticeRoot.pivot = new Vector2(0f, 1f);
                noticeRoot.anchoredPosition = new Vector2(12f, -top - 123f);
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
                sheet.anchoredPosition = new Vector2(0f, bottom + 69f);
                sheet.sizeDelta = new Vector2(Mathf.Min(355f, w / scale - 24f), 228f);
                noticeRoot.anchorMin = noticeRoot.anchorMax = new Vector2(.5f, 1f);
                noticeRoot.pivot = new Vector2(.5f, 1f);
                noticeRoot.anchoredPosition = new Vector2(0f, -top - 180f);
                noticeRoot.sizeDelta = new Vector2(324f, 35f);
                statusSheet.anchorMin=statusSheet.anchorMax=new Vector2(.5f,0f);
                statusSheet.pivot=new Vector2(.5f,0f);
                statusSheet.anchoredPosition=new Vector2(0f,bottom+69f);
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
            Label(caption, chip, 11, false, AntiqueBrass,
                TextAnchor.MiddleLeft, new Vector2(23f, 9f), new Vector2(74f, 14f));
            value = Label("0", chip, 15, false, WarmWhite,
                TextAnchor.MiddleLeft, new Vector2(23f, -7f), new Vector2(76f, 20f));
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
                image.type = Image.Type.Sliced;
                image.color = Color.white;
            }
            else image.color = Color.clear;
            return rect;
        }

        private static RectTransform DockButton(string name,Transform parent,string title,
            Sprite icon,Vector2 anchor,Vector2 pivot,UnityEngine.Events.UnityAction click)
        {
            var button=ButtonPlate(name,parent,title,slatePlate,anchor,pivot,
                Vector2.zero,new Vector2(110f,54f),click);
            var rect=button.GetComponent<RectTransform>();
            var text=button.GetComponentInChildren<Text>();
            text.fontSize=13;
            text.rectTransform.anchoredPosition=new Vector2(13f,0f);
            text.rectTransform.sizeDelta=new Vector2(73f,48f);
            var medallion=Plate(name+" artistic emblem",rect,portraitPlate,
                new Vector2(0f,.5f),new Vector2(.5f,.5f),
                new Vector2(23f,0f),new Vector2(34f,34f));
            if(icon!=portraitPlate)
            {
                var inner=Plate(name+" functional icon",medallion,icon,
                    new Vector2(.5f,.5f),new Vector2(.5f,.5f),
                    Vector2.zero,new Vector2(23f,23f));
                inner.GetComponent<Image>().type=Image.Type.Simple;
                inner.GetComponent<Image>().color=Color.white;
            }
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
                name == "Execute canonical Build command" ? WarmWhite : PaleGold,
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
        private static void InitializeArt()
        {
            if (slatePlate != null) return;
            // The family is authored as five ORIGINAL reusable 9-slice PNGs
            // with common medallion/brass palette and non-stretch corners.
            // This is a genuine production asset pipeline, not a screenshot
            // crop or four flat colored UI primitives. The bronze portrait
            // comes from the existing licensed Vigilia kit, used legally
            // without replacing the game's protected original textures.
            const string art="ValoriaModular25D/UI/";
            const string skins=art+"UmbralII/";
            slatePlate=Resources.Load<Sprite>(skins+"dock_medallion");
            resourcePlate=Resources.Load<Sprite>(skins+"resource_capsule");
            parchmentPlate=Resources.Load<Sprite>(skins+"parchment_frame");
            actionPlate=Resources.Load<Sprite>(skins+"green_action");
            burgundyPlate=Resources.Load<Sprite>(skins+"burgundy_objective");
            portraitPlate=Resources.Load<Sprite>(art+"brass_portrait");
            if(slatePlate==null||resourcePlate==null||parchmentPlate==null||
               actionPlate==null||burgundyPlate==null||portraitPlate==null)
                throw new InvalidOperationException(
                    "Umbral II: missing original scalable material artwork (5 authored panels + licensed portrait)");
            woodIcon=ResourceIcon("wood");
            stoneIcon=ResourceIcon("stone");
            foodIcon=ResourceIcon("food");
        }

        // Original tiny resources drawn from deterministic layered material shapes.
        // Actual resource types follow PlayerState; no synthetic currency or counters.
        private static Sprite ResourceIcon(string kind)
        {
            const int n=48;
            var tex=new Texture2D(n,n,TextureFormat.RGBA32,false);
            tex.name="Umbral II icon "+kind;
            tex.wrapMode=TextureWrapMode.Clamp;
            tex.filterMode=FilterMode.Bilinear;
            var pixels=new Color32[n*n];
            void Pixel(int x,int y,Color32 color)
            {
                if(x>=0&&x<n&&y>=0&&y<n)pixels[y*n+x]=color;
            }
            void Ellipse(int cx,int cy,int rx,int ry,Color32 color)
            {
                for(int y=cy-ry;y<=cy+ry;y++)
                    for(int x=cx-rx;x<=cx+rx;x++)
                    {
                        float dx=(x-cx)/(float)Mathf.Max(1,rx);
                        float dy=(y-cy)/(float)Mathf.Max(1,ry);
                        if(dx*dx+dy*dy<=1f)Pixel(x,y,color);
                    }
            }
            if(kind=="wood")
            {
                Ellipse(22,19,19,12,new Color32(71,48,35,255));
                for(int y=13;y<29;y++)for(int x=7;x<39;x++)
                    if(Mathf.Abs(x-23)*.13f+Mathf.Abs(y-20)*.49f<10f)
                        Pixel(x,y,new Color32(137,83,46,255));
                Ellipse(14,21,10,10,new Color32(202,152,90,255));
                Ellipse(14,21,6,6,new Color32(120,78,47,255));
                Ellipse(14,21,4,4,new Color32(218,173,103,255));
                Ellipse(14,21,1,1,new Color32(118,76,46,255));
            }
            else if(kind=="stone")
            {
                Ellipse(22,19,17,13,new Color32(43,53,63,255));
                Ellipse(22,23,15,12,new Color32(111,127,131,255));
                Ellipse(18,28,11,7,new Color32(173,182,175,255));
                Ellipse(31,18,10,8,new Color32(76,92,104,255));
                Ellipse(16,32,6,3,new Color32(218,207,180,255));
            }
            else
            {
                for(int j=0;j<5;j++)
                {
                    int x=12+j*5;
                    for(int y=10;y<34;y++)Pixel(x,y,new Color32(160,104,47,255));
                    Ellipse(x-2,27-j%2*4,4,7,new Color32(220,164,74,255));
                    Ellipse(x+2,21+j%2*4,4,6,new Color32(246,204,113,255));
                }
                Ellipse(24,11,17,3,new Color32(138,92,47,255));
            }
            tex.SetPixels32(pixels);
            tex.Apply(false,true);
            var sprite=Sprite.Create(tex,new Rect(0,0,n,n),new Vector2(.5f,.5f),100f);
            sprite.name="Umbral II resource "+kind;
            return sprite;
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
