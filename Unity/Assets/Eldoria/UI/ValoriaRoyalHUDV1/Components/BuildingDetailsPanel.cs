using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold
{
    public sealed class BuildingDetailsPanel : MonoBehaviour
    {
        public RoyalText Title, Level, Function, Status, Costs, Requirements, Time;
        public PrimaryActionButton Primary;
        public RoyalButton Close;
        public RoyalAction Offer { get; private set; }
        public bool IsBottomSheet { get; private set; }
        public bool OperationActive => current != null && current.OperationActive;
        private RoyalText resultText;
        private RectTransform progressTrack, progressFill;
        private RoyalBuilding current;
        private float panelWidth = 414f;

        public void Render(RoyalBuilding b, bool en, bool pending)
        {
            current = b;
            gameObject.SetActive(b != null);
            Offer = b != null && !b.OperationActive ? b.Action : null;
            if (b == null) return;
            EnsureParts();
            Title.Put(b.Name);
            bool structure = b.Id == "bastion" || b.Id == "sawmill" || b.Id == "barracks";
            Level.Put(structure && b.Level > 0 ? RoyalFormatting.L(en, "NIVEL ", "LEVEL ") + b.Level : "");
            Function.Put(b.Function); Status.Put(b.State); Costs.Put(b.Costs);
            Requirements.Put(b.Requirements); Time.Put(b.Time); resultText.Put(b.Result);
            Primary.Render(Offer, pending);
            progressTrack.gameObject.SetActive(b.OperationActive && !string.IsNullOrWhiteSpace(b.Time));
            progressFill.anchorMax = new Vector2(Mathf.Clamp01(b.Progress), 1);
            Reflow();
        }

        private void EnsureParts()
        {
            if (!resultText)
            {
                resultText = Instantiate(Function, transform);
                resultText.name = "Operation result";
                resultText.color = new Color(.78f, .85f, .76f);
            }
            if (!progressTrack)
            {
                progressTrack = new GameObject("Operation progress", typeof(RectTransform), typeof(CanvasRenderer), typeof(RoyalSolidGraphic)).GetComponent<RectTransform>();
                progressTrack.SetParent(transform, false); progressTrack.gameObject.layer = gameObject.layer;
                var track = progressTrack.GetComponent<RoyalSolidGraphic>();
                track.color = new Color(.16f, .19f, .23f); track.raycastTarget = false;
                progressFill = new GameObject("Completed", typeof(RectTransform), typeof(CanvasRenderer), typeof(RoyalSolidGraphic)).GetComponent<RectTransform>();
                progressFill.SetParent(progressTrack, false); progressFill.gameObject.layer = gameObject.layer;
                progressFill.anchorMin = Vector2.zero; progressFill.anchorMax = Vector2.one;
                progressFill.offsetMin = progressFill.offsetMax = Vector2.zero;
                var fill = progressFill.GetComponent<RoyalSolidGraphic>();
                fill.color = new Color(.77f, .61f, .32f); fill.raycastTarget = false;
            }
        }

        public void ApplyLayout(float availableWidth, bool compact = false)
        {
            IsBottomSheet = compact || availableWidth < 1080;
            panelWidth = IsBottomSheet ? Mathf.Max(260, availableWidth - 52) : 414;
            var r = (RectTransform)transform;
            r.anchorMin = r.anchorMax = r.pivot = IsBottomSheet ? new Vector2(.5f, 0) : new Vector2(1, 1);
            r.anchoredPosition = IsBottomSheet ? new Vector2(0, 162) : new Vector2(-24, -176);
            r.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);
            Reflow();
        }

        private void Reflow()
        {
            if (current == null || !resultText) return;
            float top = 24, inside = panelWidth - 48;
            top = Row(Title, top, panelWidth - 116, 30, 78, 7);
            // Keep the close control clear even when the selected title is short.
            top = Mathf.Max(top, 76);
            top = Row(Level, top, inside, 19, 27, 8);
            top = Row(Function, top, inside, 22, 82, 12);
            Status.fontStyle = FontStyle.Bold;
            top = Row(Status, top, inside, 24, 66, 8);
            Time.color = new Color(.88f, .77f, .53f);
            top = Row(Time, top, inside, 22, 58, 8);
            if (progressTrack.gameObject.activeSelf) { Position(progressTrack, 24, top, inside, 7); top += 19; }
            top = Row(resultText, top, inside, 21, 100, 10);
            top = Row(Costs, top, inside, 21, 85, 10);
            top = Row(Requirements, top, inside, 21, 110, 12);
            if (Primary.gameObject.activeSelf)
            {
                Position((RectTransform)Primary.transform, 22, top, panelWidth - 44, 64);
                var label = Primary.Label.rectTransform;
                label.anchorMin = Vector2.zero; label.anchorMax = Vector2.one;
                label.offsetMin = new Vector2(12, 6); label.offsetMax = new Vector2(-12, -6);
                Primary.Label.fontSize = 23;
                top += 76;
            }
            ((RectTransform)transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, top + 16);
        }

        private static float Row(RoyalText text, float top, float width, int fontSize, float maximumHeight, float gap)
        {
            bool visible = !string.IsNullOrWhiteSpace(text.text);
            text.gameObject.SetActive(visible);
            if (!visible) return top;
            text.fontSize = fontSize;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
            Position(text.rectTransform, 24, top, width, maximumHeight);
            float height = Mathf.Clamp(text.preferredHeight + 4, fontSize + 6, maximumHeight);
            text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            return top + height + gap;
        }

        private static void Position(RectTransform r, float x, float top, float width, float height)
        {
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(x, -top); r.sizeDelta = new Vector2(width, height);
        }
    }
}
