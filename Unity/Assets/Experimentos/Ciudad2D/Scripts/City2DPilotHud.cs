using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.Experimentos.Ciudad2D
{
    /// <summary>
    /// Interfaz de laboratorio, aislada del HUD del juego.
    /// Tamaño tactil verificable en 390x844 y 844x390; sin panel permanente inferior.
    /// Nunca compra niveles ni altera la partida.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class City2DPilotHud : MonoBehaviour
    {
        [SerializeField] private City2DStateBridge bridge;
        private CanvasScaler scaler;
        private Text heading;
        private Text selection;
        private RectTransform selectionHolder;
        private Button first;
        private Button fifth;
        private Image firstImage;
        private Image fifthImage;
        private Font font;
        private CityBuilding selected;

        private static readonly Color Parchment = new Color(0.97f, 0.94f, 0.85f, 1f);
        private static readonly Color Gold = new Color(0.91f, 0.72f, 0.38f, 1f);
        private static readonly Color Ink = new Color(0.035f, 0.055f, 0.078f, 0.84f);
        private static readonly Color ButtonIdle = new Color(0.13f, 0.20f, 0.26f, 0.97f);
        private static readonly Color ButtonActive = new Color(0.37f, 0.27f, 0.14f, 1f);

        public void Configure(City2DStateBridge source) { bridge = source; }

        private void Start()
        {
            if (bridge == null)
            {
                Debug.LogWarning("[Ciudad2D] Falta StateBridge", this);
                return;
            }

            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 16);

            var canvasGo = new GameObject("Ciudad2D_HUD_Ligero", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;

            scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            UpdateScale();

            var top = MakeBox(canvasGo.transform, "HUD_Superior_68px", Ink);
            Position(top, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -68), Vector2.zero);

            var accent = MakeBox(top, "Filete_Oro", new Color(0.58f, 0.45f, 0.24f, 0.85f));
            Position(accent, new Vector2(0, 0), new Vector2(1, 0),
                Vector2.zero, new Vector2(0, 2));

            var title = MakeText(top, "VALORIA  /  CIUDAD 2D", 17, Parchment, TextAnchor.MiddleLeft);
            Position(title.rectTransform, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(16, 31), new Vector2(-153, -3));

            heading = MakeText(top, "BASTIÓN I  ·  NIVELES 1–4", 13, Gold, TextAnchor.MiddleLeft);
            Position(heading.rectTransform, new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(16, 5), new Vector2(-153, -34));

            first = MakeButton(top, "I–IV", new Vector2(-138, 11), new Vector2(60, 46));
            fifth = MakeButton(top, "V–IX", new Vector2(-72, 11), new Vector2(60, 46));
            firstImage = first.GetComponent<Image>();
            fifthImage = fifth.GetComponent<Image>();
            first.onClick.AddListener(() => bridge.SetDemoBastionLevel(1));
            fifth.onClick.AddListener(() => bridge.SetDemoBastionLevel(5));

            selectionHolder = MakeBox(canvasGo.transform, "Seleccion_Contextual_56px", Ink);
            Position(selectionHolder, Vector2.zero, new Vector2(1, 0),
                Vector2.zero, new Vector2(0, 56));

            selection = MakeText(selectionHolder, string.Empty, 16, Parchment, TextAnchor.MiddleCenter);
            Position(selection.rectTransform, Vector2.zero, Vector2.one,
                new Vector2(12, 3), new Vector2(-12, -3));
            selectionHolder.gameObject.SetActive(false);

            bridge.BuildingSelected += OnSelected;
            bridge.LevelsChanged += Refresh;
            Refresh();
        }

        private void Update()
        {
            if (scaler != null) UpdateScale();
        }

        private void UpdateScale()
        {
            // En 390x844 y 844x390, los botones permanecen >=44 px fisicos.
            // En iPhone retina se escalan sin ocupar mas puntos logicos.
            float byWidth = Screen.width / 390f;
            float byHeight = Screen.height / 844f;
            scaler.scaleFactor = Mathf.Clamp(Mathf.Min(byWidth, byHeight), 1f, 3f);
        }

        private void OnSelected(CityBuilding building)
        {
            selected = building;
            Refresh();
        }

        private void Refresh()
        {
            if (bridge == null || heading == null) return;
            bool live = bridge.IsLiveBound;
            int current = bridge.DemoBastionLevel;
            heading.text = live ? "PROGRESIÓN REAL  ·  SOLO LECTURA" :
                "BASTIÓN " + (current >= 5 ? "V" : "I") +
                "  ·  NIVELES " + (current >= 5 ? "5–9" : "1–4");
            first.interactable = !live;
            fifth.interactable = !live;
            if (firstImage != null) firstImage.color = current < 5 ? ButtonActive : ButtonIdle;
            if (fifthImage != null) fifthImage.color = current >= 5 ? ButtonActive : ButtonIdle;

            if (selectionHolder == null) return;
            selectionHolder.gameObject.SetActive(selected != null);
            if (selected != null && selection != null)
                selection.text = selected.DisplayName.ToUpperInvariant() +
                    "   ·   NIVEL " + selected.Level +
                    "   ·   ARTE " + selected.SelectedArtLevel +
                    (live ? "   ·   PARTIDA" : "   ·   DEMO");
        }

        private RectTransform MakeBox(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return (RectTransform)go.transform;
        }

        private Text MakeText(Transform parent, string value, int size, Color color, TextAnchor anchor)
        {
            var go = new GameObject("Rotulo", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = font;
            t.text = value;
            t.fontSize = size;
            t.color = color;
            t.alignment = anchor;
            t.raycastTarget = false;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = 11;
            t.resizeTextMaxSize = size;
            return t;
        }

        private Button MakeButton(RectTransform parent, string label, Vector2 position, Vector2 size)
        {
            var go = new GameObject("Hito_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(1, 0);
            rt.pivot = Vector2.zero;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;

            go.GetComponent<Image>().color = ButtonIdle;
            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(0.52f, 0.41f, 0.24f, 1f);
            colors.pressedColor = new Color(0.48f, 0.35f, 0.20f, 1f);
            button.colors = colors;
            var t = MakeText(rt, label, 16, Parchment, TextAnchor.MiddleCenter);
            Position(t.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private static void Position(RectTransform t, Vector2 lo, Vector2 hi, Vector2 min, Vector2 max)
        {
            t.anchorMin = lo;
            t.anchorMax = hi;
            t.offsetMin = min;
            t.offsetMax = max;
        }

        private void OnDestroy()
        {
            if (bridge == null) return;
            bridge.BuildingSelected -= OnSelected;
            bridge.LevelsChanged -= Refresh;
        }
    }
}
