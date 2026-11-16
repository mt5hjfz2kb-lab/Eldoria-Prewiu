using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.Experimentos.Ciudad2D
{
    /// <summary>
    /// Controles ligeros solo para inspeccionar el pase artístico. No simulan
    /// compra ni construcción. La UI definitiva de Vigilia permanece intacta.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class City2DPilotHud : MonoBehaviour
    {
        [SerializeField] private City2DStateBridge bridge;
        private CanvasScaler scaler;
        private Text heading;
        private Text selection;
        private Button first;
        private Button fifth;
        private Font font;
        private CityBuilding selected;
        private static readonly Color Parchment = new Color(0.95f, 0.91f, 0.80f, 1f);
        private static readonly Color Gold = new Color(0.78f, 0.64f, 0.38f, 1f);
        private static readonly Color Ink = new Color(0.055f, 0.083f, 0.12f, 0.92f);

        public void Configure(City2DStateBridge source) { bridge = source; }

        private void Start()
        {
            if (bridge == null) { Debug.LogWarning("[Ciudad2D] Falta StateBridge", this); return; }
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 22);

            var canvasGo = new GameObject("Ciudad2D_HUD_Demostracion", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
            scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(941, 1672);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            var top = MakeBox(canvasGo.transform, "CintaSuperior", Ink);
            Position(top, new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, -113), new Vector2(0, 0));
            var title = MakeText(top, "VALORIA   ·   ESTUDIO 2D", 29, Parchment, TextAnchor.MiddleLeft);
            Position(title.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(26, 36), new Vector2(-290, -2));
            heading = MakeText(top, "BASTIÓN I  /  Nivel 1–4", 21, Gold, TextAnchor.MiddleLeft);
            Position(heading.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(28, 2), new Vector2(-290, -48));

            first = MakeButton(top, "I–IV", new Vector2(-236, 16), new Vector2(108, 65));
            fifth = MakeButton(top, "V–IX", new Vector2(-118, 16), new Vector2(108, 65));
            first.onClick.AddListener(() => bridge.SetDemoBastionLevel(1));
            fifth.onClick.AddListener(() => bridge.SetDemoBastionLevel(5));

            var bottom = MakeBox(canvasGo.transform, "EstadoSeleccion", Ink);
            Position(bottom, new Vector2(0, 0), new Vector2(1, 0),
                new Vector2(0, 0), new Vector2(0, 92));
            selection = MakeText(bottom, "Toca un edificio    •    Comparador aislado, sin cambios en la partida", 22,
                Parchment, TextAnchor.MiddleLeft);
            Position(selection.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(30, 4), new Vector2(-18, -4));

            bridge.BuildingSelected += OnSelected;
            bridge.LevelsChanged += Refresh;
            Refresh();
        }

        private void Update()
        {
            if (scaler != null)
                scaler.matchWidthOrHeight = Screen.width > Screen.height ? 1f : 0f;
        }

        private void OnSelected(CityBuilding building)
        {
            selected = building;
            Refresh();
        }

        private void Refresh()
        {
            if (bridge == null || heading == null) return;
            string mode = bridge.IsLiveBound ? "LECTURA DE PARTIDA" : "PRUEBA VISUAL";
            heading.text = bridge.IsLiveBound ? mode :
                "BASTIÓN " + (bridge.DemoBastionLevel >= 5 ? "V" : "I") +
                "  /  Niveles " + (bridge.DemoBastionLevel >= 5 ? "5–9" : "1–4");
            first.interactable = !bridge.IsLiveBound;
            fifth.interactable = !bridge.IsLiveBound;
            if (selection != null)
                selection.text = selected == null ? "Toca un edificio    •    " + mode + " · sin modificar guardados" :
                    selected.DisplayName + "   ·   Nivel " + selected.Level +
                    "   ·   Ilustración hito " + selected.SelectedArtLevel + "   ·   " + mode;
        }

        private RectTransform MakeBox(Transform parent, string label, Color color)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image));
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
            t.resizeTextMinSize = 14;
            t.resizeTextMaxSize = size;
            return t;
        }

        private Button MakeButton(RectTransform parent, string label, Vector2 fromRight, Vector2 size)
        {
            var go = new GameObject("Hito_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0, 0);
            rt.anchoredPosition = fromRight;
            rt.sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.color = new Color(0.16f, 0.23f, 0.31f, 1f);
            var button = go.GetComponent<Button>();
            var cb = button.colors;
            cb.highlightedColor = new Color(0.75f, 0.72f, 0.61f, 1f);
            cb.pressedColor = new Color(0.45f, 0.39f, 0.29f, 1f);
            button.colors = cb;
            var txt = MakeText(rt, label, 25, Parchment, TextAnchor.MiddleCenter);
            Position(txt.GetComponent<RectTransform>(), new Vector2(0, 0), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(0, 0));
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
