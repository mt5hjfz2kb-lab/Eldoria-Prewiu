using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Larger, legible presentation of the SAME canonical dialogue turns,
    /// portraits, reveal/continue buttons and completion callbacks.
    /// Does not replace RoyalNarrativePresentation or edit the narrative data.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(450)]
    public sealed class RoyalGoldV2Narrative : MonoBehaviour
    {
        public RoyalHUD Source;
        private RectTransform safe;
        private RoyalText role;
        private RectTransform portrait, sigil;
        private bool cached;

        private void Awake()
        {
            if (!Source) Source = GetComponent<RoyalHUD>();
        }

        private void Cache()
        {
            if (!Source || !Source.Modal) return;
            var a = Source.GetComponentInChildren<RoyalHudLayout>(true);
            safe = a ? a.transform as RectTransform : null;
            cached = safe;
        }

        private static void Top(RectTransform rect, float x, float top, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -top);
            rect.sizeDelta = new Vector2(width, height);
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying || !Source || !Source.NarrativeOpen) return;
            if (!cached) Cache();
            if (!cached) return;

            var modal = Source.Modal;
            if (!modal || !modal.Title || !modal.Body || !modal.Extra) return;
            var frame = modal.Title.rectTransform.parent as RectTransform;
            if (!frame) return;

            float availableWidth = safe.rect.width;
            float availableHeight = safe.rect.height;
            if (availableWidth < 260 || availableHeight < 330) return;
            bool portraitScreen = availableHeight > availableWidth;

            // Deliberately focus attention during a story beat. The permanent
            // HUD is compact in exploration; the dialogue gets readable space.
            float frameWidth = Mathf.Min(availableWidth - 34f, portraitScreen ? 646f : 760f);
            float textWidth = frameWidth - 52f;
            float bodyTop = portraitScreen ? 147f : 150f;
            float bodyHeight = portraitScreen ? Mathf.Min(298f, availableHeight * .32f) : 222f;
            float actionTop = bodyTop + bodyHeight + 12f;
            frame.sizeDelta = new Vector2(frameWidth, actionTop + 82f);

            modal.Title.fontSize = portraitScreen ? 31 : 33;
            modal.Title.color = new Color(.96f, .88f, .73f);
            modal.Title.fontStyle = FontStyle.Bold;
            Top(modal.Title.rectTransform, 136, 27, frameWidth - 155f, 47);

            if (!role)
                role = frame.Find("Narrative character role") ?
                    frame.Find("Narrative character role").GetComponent<RoyalText>() : null;
            if (role)
            {
                role.fontSize = 21;
                role.color = new Color(.80f, .79f, .75f);
                Top(role.rectTransform, 138, 76, frameWidth - 164f, 32);
            }

            if (!portrait)
                portrait = frame.Find("Canonical Aldric portrait") as RectTransform;
            if (portrait) Top(portrait, 25, 21, 98, 98);
            if (!sigil) sigil = frame.Find("Narrator sigil") as RectTransform;
            if (sigil)
            {
                Top(sigil, 25, 21, 98, 98);
                var label = sigil.GetComponent<RoyalText>();
                if (label) label.fontSize = 60;
            }

            modal.Body.fontSize = portraitScreen ? 29 : 30;
            modal.Body.color = new Color(.95f, .93f, .88f);
            modal.Body.lineSpacing = 1.18f;
            modal.Body.alignment = TextAnchor.UpperLeft;
            modal.Body.horizontalOverflow = HorizontalWrapMode.Wrap;
            modal.Body.verticalOverflow = VerticalWrapMode.Truncate;
            Top(modal.Body.rectTransform, 26, bodyTop, textWidth, bodyHeight);
            Top((RectTransform)modal.Extra.transform, 24, actionTop, frameWidth - 48f, 60);
            if (modal.ExtraLabel)
                modal.ExtraLabel.fontSize = 24;
            // The canonical presenter handles character-by-character reveal,
            // next page, next speaker, language changes and final acknowledgement.
        }
    }
}
