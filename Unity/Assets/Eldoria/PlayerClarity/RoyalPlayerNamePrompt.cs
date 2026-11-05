using System;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.PlayerClarity
{
    /// <summary>
    /// First-player-entry view prepared for the authoritative PlayerState
    /// integration. It MUST be shown before the narrator by the real scene
    /// coordinator, never enabled by a second save/PlayerPrefs flag.
    /// A successful callback is responsible for persisting and rereading
    /// the name in the canonical gateway before the view can close.
    /// </summary>
    public sealed class RoyalPlayerNamePrompt : MonoBehaviour
    {
        private RectTransform panel;
        private InputField editor;
        private Text title, question, error, confirmLabel, placeholder;
        private Button confirm;
        private Func<string, bool> persist;
        private bool english;
        private bool ready;

        public bool IsOpen => gameObject.activeInHierarchy && ready;

        public static RoyalPlayerNamePrompt Create(RectTransform canvasRoot)
        {
            if (!canvasRoot) throw new ArgumentNullException(nameof(canvasRoot));
            var obj = new GameObject("Eldoria / first entry name parchment",
                typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            var rect = (RectTransform)obj.transform;
            rect.SetParent(canvasRoot, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var view = obj.AddComponent<RoyalPlayerNamePrompt>();
            view.Construct();
            obj.SetActive(false);
            return view;
        }

        private static Font Serif()
        {
            var font = Resources.Load<Font>("EldoriaRetention/reference-serif");
            return font ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static Text Label(Transform parent, string name,
            Vector2 min, Vector2 max, int size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var r = (RectTransform)go.transform;
            r.SetParent(parent, false);
            r.anchorMin = min; r.anchorMax = max;
            r.offsetMin = r.offsetMax = Vector2.zero;
            var label = go.GetComponent<Text>();
            label.font = Serif();
            label.fontSize = size;
            label.alignment = TextAnchor.MiddleCenter;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = Mathf.Min(12, size);
            label.resizeTextMaxSize = size;
            label.color = color;
            label.raycastTarget = false;
            label.supportRichText = false;
            return label;
        }

        private static Image Plate(Transform parent, string name,
            Vector2 min, Vector2 max, Color fill, Color edge)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline));
            var r = (RectTransform)go.transform;
            r.SetParent(parent, false);
            r.anchorMin = min; r.anchorMax = max;
            r.offsetMin = r.offsetMax = Vector2.zero;
            var image = go.GetComponent<Image>();
            image.color = fill;
            image.raycastTarget = false;
            var outline = go.GetComponent<Outline>();
            outline.effectColor = edge;
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;
            return image;
        }

        private void Construct()
        {
            if (ready) return;
            var darkness = GetComponent<Image>();
            darkness.color = new Color(.015f, .022f, .025f, .83f);
            darkness.raycastTarget = true;

            var frame = Plate(transform, "Aged parchment",
                new Vector2(.08f, .20f), new Vector2(.92f, .80f),
                new Color(.80f, .70f, .51f, 1f),
                new Color(.97f, .80f, .50f, 1f));
            panel = (RectTransform)frame.transform;
            Plate(panel, "Inner vellum", new Vector2(.017f, .023f),
                new Vector2(.983f, .977f),
                new Color(.91f, .82f, .65f, 1f),
                new Color(.41f, .27f, .15f, .68f));
            Plate(panel, "Thin golden divider", new Vector2(.14f, .78f),
                new Vector2(.86f, .783f),
                new Color(.56f, .35f, .17f, .67f), Color.clear);

            title = Label(panel, "Opening question", new Vector2(.08f, .79f),
                new Vector2(.92f, .95f), 34, new Color(.27f, .16f, .09f));
            question = Label(panel, "Valoria's challenge",
                new Vector2(.11f, .48f), new Vector2(.89f, .77f), 24,
                new Color(.29f, .21f, .14f));

            var inputBack = Plate(panel, "Player's chosen name",
                new Vector2(.16f, .29f), new Vector2(.84f, .44f),
                new Color(.24f, .19f, .14f, 1f),
                new Color(.64f, .43f, .19f, 1f));
            inputBack.raycastTarget = true;
            editor = inputBack.gameObject.AddComponent<InputField>();
            editor.characterLimit = PlayerNameRules.MaximumLength;
            editor.lineType = InputField.LineType.SingleLine;
            editor.contentType = InputField.ContentType.Standard;
            editor.textComponent = Label(inputBack.transform, "Live input",
                new Vector2(.04f, 0), new Vector2(.96f, 1), 25,
                new Color(.98f, .93f, .82f));
            editor.textComponent.alignment = TextAnchor.MiddleLeft;
            editor.textComponent.resizeTextForBestFit = false;
            placeholder = Label(inputBack.transform, "Placeholder",
                new Vector2(.04f, 0), new Vector2(.96f, 1), 22,
                new Color(.76f, .68f, .53f, .85f));
            placeholder.alignment = TextAnchor.MiddleLeft;
            editor.placeholder = placeholder;
            editor.onEndEdit.AddListener(OnEndEdit);

            error = Label(panel, "Validation",
                new Vector2(.08f, .175f), new Vector2(.92f, .29f), 15,
                new Color(.47f, .16f, .12f));

            var actionImage = Plate(panel, "Confirm your name",
                new Vector2(.30f, .055f), new Vector2(.70f, .19f),
                new Color(.49f, .32f, .17f, 1f),
                new Color(.91f, .72f, .40f, 1f));
            actionImage.raycastTarget = true;
            confirm = actionImage.gameObject.AddComponent<Button>();
            confirm.targetGraphic = actionImage;
            confirm.onClick.AddListener(Confirm);
            confirmLabel = Label(actionImage.transform, "Continue",
                new Vector2(.02f, .02f), new Vector2(.98f, .98f), 21,
                new Color(.99f, .88f, .67f));
            ready = true;
        }

        public void Open(bool inEnglish, Func<string, bool> persistToPlayerState)
        {
            if (!ready) Construct();
            if (persistToPlayerState == null)
                throw new ArgumentNullException(nameof(persistToPlayerState));
            persist = persistToPlayerState;
            english = inEnglish;
            title.text = english ? "WHO ARE YOU?" : "¿QUIÉN ERES?";
            question.text = english
                ? "Have you come to finish off what little remains... or to help us save it?"
                : "¿Vienes a terminar con lo poco que queda… o a ayudarnos a salvarlo?";
            placeholder.text = english ? "Your name" : "Tu nombre";
            confirmLabel.text = english ? "CONTINUE" : "CONTINUAR";
            editor.text = "";
            error.text = "";
            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            editor.ActivateInputField();
        }

        private void OnEndEdit(string submitted)
        {
            // A physical Return/Done key should not steal focus from this
            // single-line field unless it contains an acceptable nickname.
            if (IsOpen && !string.IsNullOrWhiteSpace(submitted)) Confirm();
        }

        public void Confirm()
        {
            if (!IsOpen || persist == null) return;
            if (!PlayerNameRules.TryNormalize(editor.text, out var name, out var reason))
            {
                error.text = english ? "Use 3–18 letters/numbers, spaces, - or _." : reason;
                return;
            }

            // The actual gateway callback must commit and verify the same
            // PlayerState name. Failed/stale writes leave the screen open.
            if (!persist(name))
            {
                error.text = english
                    ? "The name could not be saved. Please try again."
                    : "No se ha podido guardar el nombre. Inténtalo de nuevo.";
                return;
            }
            persist = null;
            gameObject.SetActive(false);
        }
    }
}
