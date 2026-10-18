using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold
{
    public sealed class MissionPanel : MonoBehaviour
    {
        public RoyalText Chapter, Objective, ProgressText;
        public RectTransform Fill, Details;
        public RoyalButton Toggle, Open;
        private RoyalText context, openLabel;
        private bool collapsed;
        private float width = 316f;
        public bool IsCollapsed => collapsed;
        public string ContextText => context ? context.text : "";

        private void Awake() { width = ((RectTransform)transform).rect.width; EnsureParts(); }
        private void OnEnable() { if (Toggle) Toggle.onClick.AddListener(Switch); }
        private void OnDisable() { if (Toggle) Toggle.onClick.RemoveListener(Switch); }

        private void EnsureParts()
        {
            if (!context && Objective && Details)
            {
                context = Instantiate(Objective, Details);
                context.name = "Objective hint";
                context.fontSize = 20;
                context.color = new Color(.77f, .81f, .82f);
                context.Put("");
            }
            if (!openLabel && Open && Objective)
            {
                // Reuse the existing button and its listeners; only replace the ambiguous arrow.
                foreach (var symbol in Open.GetComponentsInChildren<RoyalIconGraphic>(true))
                    symbol.gameObject.SetActive(false);
                openLabel = Instantiate(Objective, Open.transform);
                openLabel.name = "Objective action label";
                openLabel.alignment = TextAnchor.MiddleCenter;
                openLabel.fontSize = 23;
                openLabel.fontStyle = FontStyle.Bold;
                var r = openLabel.rectTransform;
                r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
                r.offsetMin = new Vector2(10, 4); r.offsetMax = new Vector2(-10, -4);
            }
        }

        private void Switch() { collapsed = !collapsed; Reflow(); }

        public void SetContext(string text)
        {
            EnsureParts();
            if (context) context.Put(text);
            Reflow();
        }

        public void ApplyLayout(float availableWidth)
        {
            width = availableWidth >= 1080 ? 316f : Mathf.Max(260f, availableWidth - 52f);
            Reflow();
        }

        public void Render(RoyalSnapshot state, bool en = false)
        {
            EnsureParts();
            Chapter.Put(state.Chapter); Objective.Put(state.Objective);
            ProgressText.Put(state.ObjectiveProgress);
            Fill.anchorMax = new Vector2(Mathf.Clamp01(state.MissionFraction), 1f);
            if (openLabel) openLabel.Put(RoyalFormatting.L(en, "Ir al objetivo", "Go to objective"));
            Reflow();
        }

        private void Reflow()
        {
            if (!Details || !Objective || !Chapter) return;
            var root = (RectTransform)transform;
            root.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            Chapter.fontSize = 19;
            Position(Chapter.rectTransform, 20, 18, width - 102, 28);
            if (Toggle)
            {
                var symbol = Toggle.GetComponentInChildren<RoyalIconGraphic>(true);
                if (symbol) { symbol.Icon = collapsed ? 14 : 13; symbol.SetVerticesDirty(); }
            }
            Details.gameObject.SetActive(!collapsed);
            if (collapsed) { root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 70f); return; }
            Details.anchorMin = Details.anchorMax = Details.pivot = new Vector2(0, 1);
            Details.anchoredPosition = new Vector2(0, -62);
            Details.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            float top = 0, inside = width - 40;
            top = TextRow(Objective, top, inside, 24, 58);
            if (context)
            {
                context.gameObject.SetActive(!string.IsNullOrWhiteSpace(context.text));
                if (context.gameObject.activeSelf) top = TextRow(context, top, inside, 20, 96);
            }
            ProgressText.gameObject.SetActive(!string.IsNullOrWhiteSpace(ProgressText.text));
            if (ProgressText.gameObject.activeSelf) top = TextRow(ProgressText, top, inside, 20, 26);
            if (Fill && Fill.parent is RectTransform track)
            {
                track.gameObject.SetActive(ProgressText.gameObject.activeSelf);
                if (track.gameObject.activeSelf) { Position(track, 20, top, inside, 6); top += 20; }
            }
            if (Open) { Position((RectTransform)Open.transform, 20, top, inside, 62); top += 76; }
            Details.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, top);
            root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 62 + top);
        }

        private static float TextRow(RoyalText text, float top, float width, int size, float maxHeight)
        {
            text.fontSize = size; text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            Position(text.rectTransform, 20, top, width, maxHeight);
            float height = Mathf.Clamp(text.preferredHeight + 3, size + 6, maxHeight);
            text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            return top + height + 8;
        }

        private static void Position(RectTransform r, float x, float top, float w, float h)
        {
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(x, -top); r.sizeDelta = new Vector2(w, h);
        }
    }
}
