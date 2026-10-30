using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Final owner-art pass applied AFTER the certified V2/V3 narrative layout.
    /// Reuses the single real RoyalHUD, ModalWindow and original Aldric texture.
    /// No fabricated character, new button, event, save, clock or game command.
    /// </summary>
    // Must run AFTER RoyalGoldV2Narrative (450), otherwise that
    // component overwrites the cinematic frame geometry each frame.
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(650)]
    public sealed class RoyalGoldV3CinematicFinish : MonoBehaviour
    {
        public RoyalHUD Source;
        private RectTransform safe, frame, manuscript, portrait, sigil;
        private RoyalGoldV3ParchmentGraphic parchment;
        private Graphic originalFrame;
        private RoyalText role;
        private Vector2 savedAnchorMin, savedAnchorMax, savedPivot;
        private Vector2 savedPosition, savedSize;
        private Color savedTitleColor, savedBodyColor, savedExtraLabelColor;
        private int savedTitleSize, savedBodySize, savedExtraLabelSize;
        private Vector2 labelAnchorMin, labelAnchorMax, labelPivot, labelPosition, labelSize;
        private TextAnchor labelAlignment;
        private bool captured;
        // Pre-rendered genuine 3D busts; zero real-time skinned hero renderers in mobile dialogue.
        // Default is neutral for every turn until canonical narrative authors assign a mood.
        private Texture2D[] aldricBustTextures;
        private int aldricMood;
        private int aldricLastTurn = -1;
        private static readonly string[] AldricBustPaths = {
            "UI/AldricDialogue/aldric_neutral",
            "UI/AldricDialogue/aldric_firme",
            "UI/AldricDialogue/aldric_preocupado",
            "UI/AldricDialogue/aldric_sereno"
        };

        // Optional, presentation-only authored override; never guesses emotion from dialogue text.
        public void SetAldricPortraitMood(int mood) => aldricMood = Mathf.Clamp(mood, 0, 3);

        private Texture2D GetAldricBust()
        {
            if (aldricBustTextures == null)
            {
                aldricBustTextures = new Texture2D[AldricBustPaths.Length];
                for (int i = 0; i < AldricBustPaths.Length; i++)
                    aldricBustTextures[i] = Resources.Load<Texture2D>(AldricBustPaths[i]);
            }
            return aldricBustTextures[aldricMood] ? aldricBustTextures[aldricMood] :
                   aldricBustTextures[0];
        }
        private static readonly Color Sepia = new Color(.19f, .15f, .11f, 1f);
        private static readonly Color Brown = new Color(.38f, .25f, .13f, 1f);
        private static readonly Color PurplePale = new Color(.95f, .86f, 1f, 1f);

        private void Awake()
        {
            if (!Source) Source = GetComponent<RoyalHUD>();
        }

        private static void Top(RectTransform r, float x, float top, float w, float h)
        {
            if (!r) return;
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0, 1);
            r.anchoredPosition = new Vector2(x, -top);
            r.sizeDelta = new Vector2(Mathf.Max(0, w), Mathf.Max(0, h));
        }

        // Canvas dimensions also change during offscreen native captures and
        // phone rotation between LateUpdate and rendering. Reflow from the
        // actual current canvas before it is drawn, not last frame's width.
        private void OnEnable() => Canvas.willRenderCanvases += SynchronizeNow;
        private void OnDisable()
        {
            Canvas.willRenderCanvases -= SynchronizeNow;
            Restore();
        }

        private void Restore()
        {
            if (!captured) return;
            if (originalFrame) originalFrame.enabled = true;
            if (manuscript) manuscript.gameObject.SetActive(false);
            if (frame)
            {
                frame.anchorMin = savedAnchorMin;
                frame.anchorMax = savedAnchorMax;
                frame.pivot = savedPivot;
                // The canonical narrative controller has already restored
                // ordinary menu geometry when its story ends. Never replace
                // that with a snapshot of the preceding narrative layout.
                if (Source && Source.NarrativeOpen)
                {
                    frame.anchoredPosition = savedPosition;
                    frame.sizeDelta = savedSize;
                }
            }
            if (Source && Source.Modal)
            {
                var modal = Source.Modal;
                if (modal.Title)
                {
                    modal.Title.color = savedTitleColor;
                    modal.Title.fontSize = savedTitleSize;
                }
                if (modal.Body)
                {
                    modal.Body.color = savedBodyColor;
                    if (Source.NarrativeOpen) modal.Body.fontSize = savedBodySize;
                }
                if (modal.ExtraLabel)
                {
                    modal.ExtraLabel.color = savedExtraLabelColor;
                    modal.ExtraLabel.fontSize = savedExtraLabelSize;
                    var label = modal.ExtraLabel.rectTransform;
                    label.anchorMin = labelAnchorMin;
                    label.anchorMax = labelAnchorMax;
                    label.pivot = labelPivot;
                    label.anchoredPosition = labelPosition;
                    label.sizeDelta = labelSize;
                    modal.ExtraLabel.alignment = labelAlignment;
                }
            }
            captured = false;
        }

        private bool Cache()
        {
            if (!Source || !Source.Modal || !Source.Modal.Title) return false;
            var layout = Source.GetComponentInChildren<RoyalHudLayout>(true);
            safe = layout ? layout.transform as RectTransform : null;
            frame = Source.Modal.Title.rectTransform.parent as RectTransform;
            if (!safe || !frame) return false;
            role = frame.Find("Narrative character role")
                ? frame.Find("Narrative character role").GetComponent<RoyalText>() : null;
            portrait = frame.Find("Canonical Aldric portrait") as RectTransform;
            sigil = frame.Find("Narrator sigil") as RectTransform;
            if (!manuscript)
            {
                var go = new GameObject("V3 / cinematic royal parchment",
                    typeof(RectTransform), typeof(CanvasRenderer),
                    typeof(RoyalGoldV3ParchmentGraphic));
                go.transform.SetParent(frame, false);
                go.transform.SetAsFirstSibling();
                go.layer = frame.gameObject.layer;
                manuscript = (RectTransform)go.transform;
                manuscript.anchorMin = Vector2.zero;
                manuscript.anchorMax = Vector2.one;
                manuscript.offsetMin = Vector2.zero;
                manuscript.offsetMax = Vector2.zero;
                parchment = go.GetComponent<RoyalGoldV3ParchmentGraphic>();
                parchment.raycastTarget = false;
            }
            if (!parchment) parchment = manuscript.GetComponent<RoyalGoldV3ParchmentGraphic>();
            originalFrame = frame.GetComponent<Graphic>();
            return true;
        }

        private void LateUpdate() => SynchronizeNow();

        public void SynchronizeNow()
        {
            if (!UnityEngine.Application.isPlaying || !Source) return;
            if (!Source.NarrativeOpen)
            {
                Restore();
                return;
            }
            var modal = Source.Modal;
            if (!modal || !modal.Title || !modal.Body || !modal.Extra) return;
            if (!Cache()) return;
            float availableWidth = safe.rect.width;
            float availableHeight = safe.rect.height;
            if (availableWidth < 280f || availableHeight < 320f) return;

            if (!captured)
            {
                savedAnchorMin = frame.anchorMin;
                savedAnchorMax = frame.anchorMax;
                savedPivot = frame.pivot;
                savedPosition = frame.anchoredPosition;
                savedSize = frame.sizeDelta;
                savedTitleColor = modal.Title.color;
                savedBodyColor = modal.Body.color;
                savedTitleSize = modal.Title.fontSize;
                savedBodySize = modal.Body.fontSize;
                if (modal.ExtraLabel)
                {
                    savedExtraLabelColor = modal.ExtraLabel.color;
                    savedExtraLabelSize = modal.ExtraLabel.fontSize;
                    var label = modal.ExtraLabel.rectTransform;
                    labelAnchorMin = label.anchorMin;
                    labelAnchorMax = label.anchorMax;
                    labelPivot = label.pivot;
                    labelPosition = label.anchoredPosition;
                    labelSize = label.sizeDelta;
                    labelAlignment = modal.ExtraLabel.alignment;
                }
                captured = true;
            }
            if (originalFrame) originalFrame.enabled = false;
            manuscript.gameObject.SetActive(true);

            bool arcane = Source.NarrativeSpeaker == "Orín" ||
                Source.NarrativeSpeaker == "Orin";
            if (parchment.Arcane != arcane)
            {
                parchment.Arcane = arcane;
                parchment.SetVerticesDirty();
            }

            bool portraitOrientation = availableHeight > availableWidth;
            float panelWidth = Mathf.Min(availableWidth - 26f,
                portraitOrientation ? 725f : 1120f);
            float panelHeight = Mathf.Min(availableHeight - 24f,
                portraitOrientation ? 440f : 356f);
            bool compactPortrait = portraitOrientation && panelWidth < 580f;
            float column = Mathf.Clamp(panelWidth * .29f, 132f, 360f);
            // Dialogues rest in the bottom cinematic strip. Unlike V2, they
            // never create a central opaque rectangle covering the whole city.
            frame.anchorMin = frame.anchorMax = new Vector2(.5f, 0f);
            frame.pivot = new Vector2(.5f, 0f);
            frame.sizeDelta = new Vector2(panelWidth, panelHeight);
            // Centre on the *safe-area* bottom, not the device's raw bottom.
            var modalParent = frame.parent as RectTransform;
            if (modalParent)
            {
                var corners = new Vector3[4];
                safe.GetWorldCorners(corners);
                var safeBottomLeft = modalParent.InverseTransformPoint(corners[0]);
                var safeBottomRight = modalParent.InverseTransformPoint(corners[3]);
                frame.anchoredPosition = new Vector2(
                    (safeBottomLeft.x + safeBottomRight.x) * .5f - modalParent.rect.center.x,
                    safeBottomLeft.y - modalParent.rect.yMin + (portraitOrientation ? 23f : 18f));
            }
            else frame.anchoredPosition = new Vector2(0f, portraitOrientation ? 23f : 18f);

            float left = column + 22f;
            float textWidth = panelWidth - left - 22f;
            float titleTop = 22f;
            // Narrow phones need the full paper width for long canonical story
            // turns. Keep portrait and speaker together in the top band,
            // then place narrative text BELOW them rather than squeezing it
            // into a 220px-wide column with silent truncation.
            float bodyTop = compactPortrait ? 140f :
                portraitOrientation ? 116f : 102f;
            float bodyLeft = compactPortrait ? 24f : left;
            float bodyWidth = compactPortrait ? panelWidth - 48f : textWidth;
            float bodyHeight = panelHeight - bodyTop - 78f;
            // Interface 1 concept places Aldric as a substantial cinematic
            // presence beside his real text, not a small account avatar.
            // Reuse the actual approved asset; never invent a different face.
            float portraitSize = compactPortrait
                ? Mathf.Clamp(column - 24f, 76f, 100f)
                : Mathf.Max(90f, Mathf.Min(column - 18f, panelHeight - 35f));
            float portraitLeft = 11f + (column - portraitSize) * .5f;
            float portraitTop = compactPortrait ? 12f :
                Mathf.Max(24f, (panelHeight - portraitSize) * .5f - 3f);

            modal.Title.fontSize = panelWidth < 465f ? 23 : 29;
            modal.Title.color = arcane ? PurplePale : Brown;
            modal.Title.fontStyle = FontStyle.Bold;
            modal.Title.alignment = TextAnchor.MiddleLeft;
            Top(modal.Title.rectTransform, left, titleTop, textWidth, 48f);
            if (role)
            {
                role.color = arcane ? new Color(.82f, .69f, .94f, 1f) : Brown;
                role.fontSize = panelWidth < 465f ? 17 : 20;
                Top(role.rectTransform, left + 2f, 70f, textWidth - 4f, 30f);
            }

            // The legacy narrative component currently activates Aldric for any non-narrator.
            // Correct that VISUALLY here, without changing the authoritative gameplay/UI.
            string speaker = Source.NarrativeSpeaker ?? string.Empty;
            bool showingAldric =
                string.Equals(speaker, "Sir Aldric", System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(speaker, "Aldric", System.StringComparison.OrdinalIgnoreCase);
            bool hasVisibleAldricPortrait = false;
            if (portrait)
            {
                // Only the authentic Aldric image may represent Aldric. Lyra,
                // narrator and Orín keep the canonical speaker-specific sigil,
                // never an invented portrait or Aldric misidentified as Lyra.
                Top(portrait, portraitLeft, portraitTop, portraitSize, portraitSize);
                var face = portrait.GetComponent<RawImage>();
                if (face)
                {
                    // Keep the existing canonical RawImage, speaker visibility and its fallback.
                    // Only Sir Aldric may receive Aldric's approved chest-up portrait.
                    if (showingAldric)
                    {
                        int turn = Source.NarrativeTurnIndex;
                        if (aldricLastTurn != turn)
                        {
                            aldricMood = 0; // new turn neutral by default
                            aldricLastTurn = turn;
                        }
                        var bust = GetAldricBust();
                        if (bust) face.texture = bust;
                    }
                    // Hide legacy Aldric JPEG on Lyra/Orín/narrator turns.
                    // The speaker-specific glyph appears instead; never mislabel Aldric.
                    hasVisibleAldricPortrait = showingAldric && face.texture != null;
                    face.gameObject.SetActive(hasVisibleAldricPortrait);
                    face.raycastTarget = false;
                    face.uvRect = new Rect(0f, 0f, 1f, 1f);
                }
            }
            if (sigil)
            {
                // If both the new bust and original JPEG are missing, use the
                // existing sigil rather than leaving an empty speaker column.
                sigil.gameObject.SetActive(!hasVisibleAldricPortrait);
                Top(sigil, portraitLeft, portraitTop, portraitSize, portraitSize);
                var glyph = sigil.GetComponent<RoyalText>();
                if (glyph)
                {
                    glyph.fontSize = Mathf.RoundToInt(portraitSize * .58f);
                    glyph.color = arcane
                        ? new Color(.78f, .60f, .92f) : new Color(.89f, .75f, .43f);
                }
            }

            // The paper uses ink, NOT white text on an oversized navy slab.
            // A real 218-character page must remain fully readable on phones.
            modal.Body.fontSize = panelWidth < 465f ? 21 :
                (portraitOrientation ? 25 : 27);
            modal.Body.color = arcane ? PurplePale : Sepia;
            modal.Body.lineSpacing = 1.15f;
            modal.Body.alignment = TextAnchor.UpperLeft;
            modal.Body.horizontalOverflow = HorizontalWrapMode.Wrap;
            modal.Body.verticalOverflow = VerticalWrapMode.Truncate;
            Top(modal.Body.rectTransform, bodyLeft, bodyTop, bodyWidth, bodyHeight);
            // Real measured story text, including Aldric's line breaks, must
            // fit. Prefer increasing only a dialogue that actually needs it
            // (within the device safe area) over shrinking every page.
            float needed = Mathf.Ceil(modal.Body.preferredHeight) + 12f;
            if (needed > bodyHeight)
            {
                float maximumHeight = Mathf.Min(availableHeight - 24f,
                    portraitOrientation ? 600f : 430f);
                float growth = Mathf.Min(Mathf.Max(0f, maximumHeight - panelHeight),
                    needed - bodyHeight);
                panelHeight += growth;
                bodyHeight += growth;
                frame.sizeDelta = new Vector2(panelWidth, panelHeight);
                Top(modal.Body.rectTransform, bodyLeft, bodyTop, bodyWidth, bodyHeight);
            }
            while (modal.Body.preferredHeight + 8f > bodyHeight &&
                   modal.Body.fontSize > 18)
                modal.Body.fontSize--;
            // Short devices may not fit the longest page at 18px; expose the
            // full available safe height rather than silently crop text.
            if (modal.Body.preferredHeight + 8f > bodyHeight &&
                panelHeight < availableHeight - 24f)
            {
                float growth = Mathf.Min(availableHeight - 24f - panelHeight,
                    modal.Body.preferredHeight + 12f - bodyHeight);
                if (growth > 0f)
                {
                    panelHeight += growth;
                    bodyHeight += growth;
                    frame.sizeDelta = new Vector2(panelWidth, panelHeight);
                    Top(modal.Body.rectTransform, bodyLeft, bodyTop, bodyWidth, bodyHeight);
                }
            }

            // Retain the actual original Continue/Reveal button and listener,
            // but present it as a compact action on the bottom-right of paper.
            float buttonWidth = Mathf.Clamp(panelWidth * .255f, 170f, 225f);
            Top((RectTransform)modal.Extra.transform,
                panelWidth - buttonWidth - 20f, panelHeight - 64f,
                buttonWidth, 48f);
            if (modal.ExtraLabel)
            {
                modal.ExtraLabel.fontSize = 21;
                modal.ExtraLabel.color = new Color(.96f, .91f, .81f);
                // The original label is a fixed 510px child of a 530px
                // button. Resize that SAME child with its compact button;
                // otherwise Continue renders outside the paper entirely.
                Top(modal.ExtraLabel.rectTransform, 8f, 3f, buttonWidth - 16f, 42f);
                modal.ExtraLabel.alignment = TextAnchor.MiddleCenter;
            }
        }
    }
}

