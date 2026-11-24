using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Eldoria.Branding
{
    // Intentionally opt-in: no startup hooks, no saves, no scene/prefab edits.
    [DisallowMultipleComponent]
    public sealed class EonscarBrandingTrialOverlay : MonoBehaviour
    {
        private const string LogoResource = "EonscarTrial/eonscar-logo";
        private GameObject ownedEventSystem;
        private bool dismissed;

        public static void Show()
        {
            if (!Application.isPlaying ||
                Object.FindFirstObjectByType<EonscarBrandingTrialOverlay>() != null)
                return;
            new GameObject("EONSCAR Branding Trial (opt-in)")
                .AddComponent<EonscarBrandingTrialOverlay>();
        }

        private void Start()
        {
            if (EventSystem.current == null)
                ownedEventSystem = new GameObject("EONSCAR Trial EventSystem",
                    typeof(EventSystem), typeof(StandaloneInputModule));
            BuildOverlay();
        }

        private void BuildOverlay()
        {
            var canvasGo = new GameObject("EONSCAR Trial Canvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30000;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;

            var backdrop = MakeRect("Input-blocking backdrop", canvasGo.transform,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var tint = backdrop.gameObject.AddComponent<Image>();
            tint.color = new Color(.035f, .028f, .075f, .94f);
            tint.raycastTarget = true;
            // The player's SlicePresenter treats a UI raycast hit as blocking
            // world selection only when an ancestor is Selectable. A plain
            // Image is NOT sufficient. This silent button consumes backdrop
            // taps and drags without executing commands or altering game state.
            var inputSink = backdrop.gameObject.AddComponent<Button>();
            inputSink.targetGraphic = tint;
            inputSink.transition = Selectable.Transition.None;
            inputSink.navigation = new Navigation { mode = Navigation.Mode.None };

            // Keep artwork and CTA within the device safe area, but preserve
            // full-screen input blocking on the background (notch/home gesture).
            Rect safeArea = Screen.safeArea;
            if (safeArea.width < 1f || safeArea.height < 1f)
                safeArea = new Rect(0f, 0f, Screen.width, Screen.height);
            var content = MakeRect("Safe-area branding content", backdrop,
                new Vector2(safeArea.xMin / Screen.width, safeArea.yMin / Screen.height),
                new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height),
                Vector2.zero, Vector2.zero);

            // CanvasScaler matchWidthOrHeight=.5 uses a geometric-mean scale.
            // Calculate the virtual canvas height before positioning the logo,
            // otherwise a short landscape screen clips the top of the emblem.
            float scale = Mathf.Sqrt((Screen.width / 1920f) * (Screen.height / 1080f));
            scale = Mathf.Max(.001f, scale);
            float virtualWidth = safeArea.width / scale;
            float virtualHeight = safeArea.height / scale;
            bool portrait = Screen.height > Screen.width;
            float logoCenter = portrait ? .54f : .64f;
            Vector2 maxLogo = new Vector2(
                Mathf.Min(1120f, virtualWidth * .84f),
                Mathf.Min(650f, virtualHeight * (portrait ? .37f : .50f)));
            var logoRect = MakeRect("EONSCAR logotype", content,
                new Vector2(.5f, logoCenter), new Vector2(.5f, logoCenter),
                Vector2.zero, maxLogo);
            var texture = Resources.Load<Texture2D>(LogoResource);
            if (texture != null)
            {
                // Explicit fit against the desired logo box. AspectRatioFitter.FitInParent
                // would expand against the *whole screen* and overlap the entry button.
                float ratio = (float)texture.width / texture.height;
                float height = Mathf.Min(maxLogo.y, maxLogo.x / ratio);
                logoRect.sizeDelta = new Vector2(height * ratio, height);
                var logo = logoRect.gameObject.AddComponent<RawImage>();
                logo.texture = texture;
                logo.raycastTarget = false;
            }
            else
            {
                Debug.LogWarning("EONSCAR_TRIAL_LOGO_MISSING: " + LogoResource);
                MakeText("EONSCAR", logoRect, 136, new Color(.88f, .83f, .72f));
            }

            float captionCenter = portrait ? .285f : .27f;
            var caption = MakeRect("Chapter caption", content,
                new Vector2(.5f, captionCenter), new Vector2(.5f, captionCenter),
                Vector2.zero, new Vector2(Mathf.Min(900f, virtualWidth * .88f), 64f));
            MakeText("LAS CENIZAS DE VALORIA", caption, 32,
                new Color(.84f, .76f, .58f));

            float buttonCenter = portrait ? .15f : .11f;
            var enter = MakeRect("Enter existing gameplay", content,
                new Vector2(.5f, buttonCenter), new Vector2(.5f, buttonCenter),
                Vector2.zero, new Vector2(
                    Mathf.Min(portrait ? 660f : 500f, virtualWidth * .72f),
                    Mathf.Max(portrait ? 128f : 112f, 48f / scale)));
            var buttonImage = enter.gameObject.AddComponent<Image>();
            buttonImage.color = new Color(.34f, .19f, .39f, 1f);
            var button = enter.gameObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(Dismiss);
            var label = MakeRect("Enter text", enter, Vector2.zero, Vector2.one,
                Vector2.zero, Vector2.zero);
            MakeText("ENTRAR EN VALORIA", label, 37,
                new Color(1f, .93f, .78f));
            Debug.Log("EONSCAR_TRIAL_OVERLAY_READY");
        }

        public void Dismiss()
        {
            if (dismissed) return;
            dismissed = true;
            Debug.Log("EONSCAR_TRIAL_DISMISSED: no gameplay or save changes");
            Object.Destroy(gameObject);
        }

        private static RectTransform MakeRect(string name, Transform parent,
            Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            return rect;
        }

        private static void MakeText(string value, RectTransform parent, int size, Color color)
        {
            var label = parent.gameObject.AddComponent<Text>();
            label.text = value;
            label.font = Resources.Load<Font>("EldoriaRetention/reference-serif");
            if (label.font == null)
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = size;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 15;
            label.resizeTextMaxSize = size;
            label.color = color;
            label.raycastTarget = false;
        }

        private void OnDestroy()
        {
            if (ownedEventSystem != null)
                Object.Destroy(ownedEventSystem);
        }
    }
}