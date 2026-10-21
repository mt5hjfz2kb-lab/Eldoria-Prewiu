using System;
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
        private EldoriaV3VisualDirection visualDirection;
        private bool previousOrin;

        private void Awake()
        {
            if (!Source) Source = GetComponent<RoyalHUD>();
            // Owner-approved art finish is presentation-only and runs AFTER
            // the certified V2 narrative. No second HUD or story controller.
            var finish = GetComponent<RoyalGoldV3CinematicFinish>();
            if (!finish) finish = gameObject.AddComponent<RoyalGoldV3CinematicFinish>();
            finish.Source = Source;
        }

        private void Cache()
        {
            if (!Source || !Source.Modal) return;
            var a = Source.GetComponentInChildren<RoyalHudLayout>(true);
            safe = a ? a.transform as RectTransform : null;
            cached = safe;
            visualDirection = Source.GetComponent<EldoriaV3VisualDirection>();
        }

        private static void Top(RectTransform rect, float x, float top, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -top);
            rect.sizeDelta = new Vector2(width, height);
        }

        private void LateUpdate()
        {
            if (!UnityEngine.Application.isPlaying || !Source) return;
            if (!Source.NarrativeOpen)
            {
                // Do not leave Orín's supernatural treatment over normal gameplay.
                if (previousOrin && visualDirection) visualDirection.RestoreRoyal();
                previousOrin = false;
                return;
            }
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
            string speaker = Source.NarrativeSpeaker ?? "";
            bool aldric = string.Equals(speaker, "Sir Aldric", StringComparison.OrdinalIgnoreCase);
            bool orin = string.Equals(speaker, "Orín", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(speaker, "Orin", StringComparison.OrdinalIgnoreCase);
            bool lyra = string.Equals(speaker, "Lyra", StringComparison.OrdinalIgnoreCase);
            if (visualDirection)
            {
                if (orin && !previousOrin)
                    visualDirection.ApplyFromCanonicalEvent(EldoriaV3VisualDirection.NarrativeTreatment.Orin);
                else if (!orin && previousOrin)
                    visualDirection.RestoreRoyal();
            }
            previousOrin = orin;
            // A dialogue can contain up to 218 characters with explicit
            // paragraph breaks. A short fixed 222/298px text region clips
            // Aldric's very first speech when the font is enlarged.
            // Prioritise COMPLETE legibility over an artificially short box.
            float maxReadable = Mathf.Max(115f, availableHeight - bodyTop - 102f);
            float preferredRegion = portraitScreen ? 385f : 325f;
            float bodyHeight = Mathf.Min(maxReadable, preferredRegion);
            float actionTop = bodyTop + bodyHeight + 12f;
            frame.sizeDelta = new Vector2(frameWidth, actionTop + 82f);
            float portraitSize = Mathf.Clamp(frameWidth * .18f, 68f, 112f);
            float titleX = portraitSize + 38f;

            modal.Title.fontSize = frameWidth < 380f ? 25 : (portraitScreen ? 31 : 33);
            modal.Title.color = orin ? new Color(.94f, .82f, 1f) : new Color(.96f, .88f, .73f);
            modal.Title.fontStyle = FontStyle.Bold;
            Top(modal.Title.rectTransform, titleX, 27, frameWidth - titleX - 14f, 47);

            if (!role)
                role = frame.Find("Narrative character role") ?
                    frame.Find("Narrative character role").GetComponent<RoyalText>() : null;
            if (role)
            {
                role.fontSize = 20;
                role.color = orin ? new Color(.84f, .72f, .97f) : new Color(.80f, .79f, .75f);
                if (lyra || orin) role.Put(speaker);
                Top(role.rectTransform, titleX, 76, frameWidth - titleX - 14f, 32);
            }

            if (!portrait)
                portrait = frame.Find("Canonical Aldric portrait") as RectTransform;
            bool actualAldricPortrait = false;
            if (portrait)
            {
                var image = portrait.GetComponent<RawImage>();
                actualAldricPortrait = aldric && image && image.texture;
                portrait.gameObject.SetActive(actualAldricPortrait);
                Top(portrait, 24, 20, portraitSize, portraitSize);
            }
            if (!sigil) sigil = frame.Find("Narrator sigil") as RectTransform;
            if (sigil)
            {
                sigil.gameObject.SetActive(!actualAldricPortrait);
                Top(sigil, 24, 20, portraitSize, portraitSize);
                var label = sigil.GetComponent<RoyalText>();
                if (label)
                {
                    // Never show Aldric's portrait for a different canonical speaker.
                    label.Put(orin ? "◈" : lyra ? "✧" : speaker == "NARRADOR" ? "✦" : "♜");
                    label.fontSize = Mathf.RoundToInt(portraitSize * .54f);
                    label.color = orin ? new Color(.79f, .56f, .96f) : new Color(.89f, .75f, .45f);
                }
            }

            modal.Body.fontSize = availableHeight < 590f ? 25 : (portraitScreen ? 29 : 30);
            modal.Body.color = orin ? new Color(.97f, .92f, 1f) : new Color(.95f, .93f, .88f);
            modal.Body.lineSpacing = 1.18f;
            modal.Body.alignment = TextAnchor.UpperLeft;
            modal.Body.horizontalOverflow = HorizontalWrapMode.Wrap;
            modal.Body.verticalOverflow = VerticalWrapMode.Truncate;
            Top(modal.Body.rectTransform, 26, bodyTop, textWidth, bodyHeight);
            // Auto-extend for real measured content (including hard line breaks)
            // when space allows. Do NOT crop a dialogue to fit a mockup.
            float required = Mathf.Ceil(modal.Body.preferredHeight) + 12f;
            if (required > bodyHeight)
            {
                bodyHeight = Mathf.Min(maxReadable, required);
                // When the device is exceptionally short, adapt the type size
                // rather than clipping text behind the Continue button.
                while (modal.Body.preferredHeight + 12f > bodyHeight && modal.Body.fontSize > 21)
                {
                    modal.Body.fontSize--;
                    required = Mathf.Ceil(modal.Body.preferredHeight) + 12f;
                }
                if (required <= maxReadable) bodyHeight = Mathf.Max(bodyHeight, required);
                actionTop = bodyTop + bodyHeight + 12f;
                Top(modal.Body.rectTransform, 26, bodyTop, textWidth, bodyHeight);
                frame.sizeDelta = new Vector2(frameWidth, actionTop + 82f);
            }
            Top((RectTransform)modal.Extra.transform, 24, actionTop, frameWidth - 48f, 60);
            if (modal.ExtraLabel)
                modal.ExtraLabel.fontSize = 24;
            // The canonical presenter handles character-by-character reveal,
            // next page, next speaker, language changes and final acknowledgement.
        }
    }
}
