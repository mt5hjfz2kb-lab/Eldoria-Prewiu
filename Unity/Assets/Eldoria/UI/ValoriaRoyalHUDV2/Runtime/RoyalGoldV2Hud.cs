using System;
using System.Text;
using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// Presentation-only companion for the existing RoyalHUD.
    /// Every displayed value and action comes from V1's authoritative binding.
    /// Does not create a session, camera, EventSystem, save, cost or game command.
    /// Attach only to the isolated V2 prefab until owner approves integration.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(400)]
    public sealed class RoyalGoldV2Hud : MonoBehaviour
    {
        public RoyalHUD Source;

        private RectTransform safe, objectiveCard, buildingCard, scrollContent, scrollViewport;
        private RoyalText chapterText, objectiveText, progressText, buildingTitle, buildingLevel, buildingBody, primaryLabel;
        private RoyalButton objectiveAction, objectivesButton, chapterClose, buildingClose, buildingAction;
        private ScrollRect buildingScroll;
        private bool ready, expanded, portrait, hasGeometry;
        private float lastWidth = -1f, lastHeight = -1f;
        private string lastDetails = "";
        private static readonly Color Ivory = new Color(.945f, .913f, .852f);
        private static readonly Color Muted = new Color(.775f, .76f, .715f);
        private static readonly Color Gold = new Color(.886f, .73f, .45f);

        public bool ChapterExpanded => expanded;
        public bool BuildingVisible => buildingCard && buildingCard.gameObject.activeSelf;

        private void Awake()
        {
            if (!Source) Source = GetComponent<RoyalHUD>();
            if (Application.isPlaying) EnsureView();
        }

        private void OnEnable()
        {
            if (!Source) Source = GetComponent<RoyalHUD>();
            if (Source) Source.SelectionChanged += OnSelectionChanged;
        }

        private void OnDisable()
        {
            if (Source) Source.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(string unused)
        {
            // Opening a contextual sheet always closes a previously expanded
            // chapter list. Never reset a genuine mission or selected building.
            if (!string.IsNullOrEmpty(unused)) expanded = false;
        }

        private void EnsureView()
        {
            if (ready || !Source || !Source.Mission || !Source.Building) return;
            var anchor = Source.GetComponentInChildren<RoyalHudLayout>(true);
            if (!anchor) return;
            safe = anchor.transform as RectTransform;
            if (!safe) return;

            objectiveCard = Panel("V2 / current objective", safe);
            chapterText = Text("Chapter", objectiveCard, 18, Gold, true);
            objectiveText = Text("Current real objective", objectiveCard, 25, Ivory, true);
            progressText = Text("Objective progress", objectiveCard, 17, Muted);
            objectiveAction = Button("Go to existing objective", objectiveCard, out var goText, true);
            objectivesButton = Button("Open current chapter", objectiveCard, out var moreText, false);
            goText.Put("Ir al objetivo");
            moreText.Put("Misiones");
            objectiveAction.onClick.AddListener(() =>
            {
                if (Source && Source.Mission && Source.Mission.Open)
                    Source.Mission.Open.onClick.Invoke();
            });
            objectivesButton.onClick.AddListener(() => expanded = true);

            chapterClose = Button("Close chapter overlay", safe, out var closeChapterText, false);
            closeChapterText.Put("×");
            closeChapterText.fontSize = 30;
            chapterClose.onClick.AddListener(() => expanded = false);

            buildingCard = Panel("V2 / contextual building sheet", safe);
            buildingTitle = Text("Selected building", buildingCard, 30, Ivory, true);
            buildingLevel = Text("Real level", buildingCard, 18, Gold);
            buildingClose = Button("Close selected building", buildingCard, out var closeBuildingText, false);
            closeBuildingText.Put("×");
            closeBuildingText.fontSize = 30;
            buildingClose.onClick.AddListener(() => { if (Source) Source.SelectBuilding(""); });

            var scrollRoot = NewRect("Scrollable real building details", buildingCard);
            buildingScroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
            buildingScroll.horizontal = false;
            buildingScroll.vertical = true;
            buildingScroll.scrollSensitivity = 24f;
            buildingScroll.movementType = ScrollRect.MovementType.Clamped;
            scrollViewport = NewRect("Clipped text viewport", scrollRoot);
            scrollViewport.gameObject.AddComponent<CanvasRenderer>();
            var hitArea = scrollViewport.gameObject.AddComponent<Image>();
            hitArea.color = new Color(0f, 0f, 0f, 0f);
            hitArea.raycastTarget = true;
            scrollViewport.gameObject.AddComponent<RectMask2D>();
            scrollContent = NewRect("Building details content", scrollViewport);
            buildingBody = Text("Real status costs and requirements", scrollContent, 22, Ivory);
            buildingBody.alignment = TextAnchor.UpperLeft;
            buildingBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            buildingBody.verticalOverflow = VerticalWrapMode.Overflow;
            buildingScroll.viewport = scrollViewport;
            buildingScroll.content = scrollContent;

            buildingAction = Button("Execute existing authorised offer", buildingCard, out primaryLabel, true);
            buildingAction.onClick.AddListener(() =>
            {
                if (!Source || !Source.Building || Source.Building.Offer == null) return;
                var sourceButton = Source.Building.Primary.Button;
                if (sourceButton && sourceButton.interactable) sourceButton.onClick.Invoke();
            });
            buildingCard.gameObject.SetActive(false);
            chapterClose.gameObject.SetActive(false);
            ready = true;
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var g = new GameObject(name, typeof(RectTransform));
            g.transform.SetParent(parent, false);
            g.layer = parent.gameObject.layer;
            return (RectTransform)g.transform;
        }

        private static RectTransform Panel(string name, Transform parent)
        {
            var r = NewRect(name, parent);
            r.gameObject.AddComponent<CanvasRenderer>();
            var bg = r.gameObject.AddComponent<RoyalFrameGraphic>();
            bg.Variant = 0;
            bg.color = Color.white;
            bg.raycastTarget = true;
            return r;
        }

        private static RoyalText Text(string name, Transform parent, int fontSize, Color color, bool bold = false)
        {
            var r = NewRect(name, parent);
            r.gameObject.AddComponent<CanvasRenderer>();
            var label = r.gameObject.AddComponent<RoyalText>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = color;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        private static RoyalButton Button(string name, Transform parent, out RoyalText label, bool primary)
        {
            var r = NewRect(name, parent);
            r.gameObject.AddComponent<CanvasRenderer>();
            var frame = r.gameObject.AddComponent<RoyalFrameGraphic>();
            frame.Variant = primary ? 1 : 0;
            frame.color = Color.white;
            frame.raycastTarget = true;
            var button = r.gameObject.AddComponent<RoyalButton>();
            button.targetGraphic = frame;
            label = Text("Label", r, 21, primary ? Ivory : Gold, true);
            label.alignment = TextAnchor.MiddleCenter;
            Stretch(label.rectTransform, 8f);
            return button;
        }

        private static void Place(RectTransform r, Vector2 anchor, Vector2 pivot, float x, float y, float w, float h)
        {
            r.anchorMin = r.anchorMax = anchor;
            r.pivot = pivot;
            r.anchoredPosition = new Vector2(x, y);
            r.sizeDelta = new Vector2(w, h);
        }

        private static void Top(RectTransform r, float x, float top, float w, float h)
        {
            Place(r, new Vector2(0, 1), new Vector2(0, 1), x, -top, w, h);
        }

        private static void Stretch(RectTransform r, float margin)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.pivot = new Vector2(.5f, .5f);
            r.offsetMin = new Vector2(margin, margin);
            r.offsetMax = new Vector2(-margin, -margin);
        }

        private void Layout()
        {
            float width = safe.rect.width, height = safe.rect.height;
            if (width < 120 || height < 120) return;
            portrait = height > width;
            float goalWidth = Mathf.Min(portrait ? width - 36f : 446f, width - 32f);
            float goalTop = portrait ? Mathf.Min(248f, height * .23f) : 146f;
            Top(objectiveCard, portrait ? 18 : 24, goalTop, goalWidth, 156f);
            Top(chapterText.rectTransform, 17, 13, goalWidth - 130, 25);
            Top(progressText.rectTransform, goalWidth - 108, 13, 93, 25);
            progressText.alignment = TextAnchor.MiddleRight;
            Top(objectiveText.rectTransform, 17, 43, goalWidth - 34, 51);
            Top((RectTransform)objectiveAction.transform, 14, 102, (goalWidth - 40f) * .59f, 42);
            Top((RectTransform)objectivesButton.transform, 20 + (goalWidth - 40f) * .59f,
                102, (goalWidth - 40f) * .41f, 42);
            // Explicitly expanded chapter panels are allowed to be larger; the
            // default HUD remains only one short objective card.
            Top((RectTransform)chapterClose.transform,
                portrait ? Mathf.Max(10, width - 75f) : 268f,
                goalTop - 8f, 48, 48);

            float cardWidth = portrait ? Mathf.Min(width - 24, 720f) : Mathf.Min(420f, width * .34f);
            float cardHeight = portrait ? Mathf.Min(420f, height * .37f) : Mathf.Min(474f, height * .67f);
            if (portrait)
                Place(buildingCard, new Vector2(.5f, 0), new Vector2(.5f, 0), 0, 14, cardWidth, cardHeight);
            else
                Place(buildingCard, Vector2.one, Vector2.one, -22, -152, cardWidth, cardHeight);
            Top(buildingTitle.rectTransform, 19, 18, cardWidth - 91, 42);
            Top(buildingLevel.rectTransform, 20, 57, cardWidth - 44, 24);
            Top((RectTransform)buildingClose.transform, cardWidth - 58, 17, 42, 42);
            Top((RectTransform)buildingScroll.transform, 20, 92, cardWidth - 40, Mathf.Max(92, cardHeight - 170));
            Stretch(scrollViewport, 0);
            scrollContent.anchorMin = new Vector2(0, 1);
            scrollContent.anchorMax = new Vector2(1, 1);
            scrollContent.pivot = new Vector2(.5f, 1);
            scrollContent.anchoredPosition = Vector2.zero;
            scrollContent.sizeDelta = new Vector2(0, Mathf.Max(200, scrollViewport.rect.height));
            Top(buildingBody.rectTransform, 2, 0, cardWidth - 47, scrollContent.rect.height);
            Top((RectTransform)buildingAction.transform, 18, cardHeight - 70, cardWidth - 36, 54);
            hasGeometry = true;
            lastWidth = width;
            lastHeight = height;
            lastDetails = "";
        }

        private static string Safe(RoyalText value) => value ? value.text ?? "" : "";

        private void RenderBuilding()
        {
            var source = Source.Building;
            bool show = !string.IsNullOrEmpty(Source.SelectedBuildingId) &&
                source && source.Title && !string.IsNullOrWhiteSpace(source.Title.text);
            if (buildingCard.gameObject.activeSelf != show) buildingCard.gameObject.SetActive(show);
            if (!show) return;
            buildingTitle.Put(Safe(source.Title));
            buildingLevel.Put(Safe(source.Level));

            var body = new StringBuilder(280);
            Append(body, Safe(source.Function));
            Append(body, Safe(source.Status));
            Append(body, Safe(source.Time));
            Append(body, Safe(source.Costs));
            Append(body, Safe(source.Requirements));
            // All costs, requirements and state above are sourced directly from
            // the current authoritative offer. Do not synthesize extras.
            string details = body.ToString();
            if (details != lastDetails)
            {
                lastDetails = details;
                buildingBody.Put(details);
                float w = buildingBody.rectTransform.rect.width;
                if (w > 10)
                {
                    float naturalHeight = Mathf.Max(scrollViewport.rect.height,
                        buildingBody.preferredHeight + 20f);
                    scrollContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, naturalHeight);
                    buildingBody.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, naturalHeight);
                    buildingScroll.verticalNormalizedPosition = 1f;
                }
            }
            bool offered = source.Primary && source.Primary.Visible;
            buildingAction.gameObject.SetActive(offered);
            if (offered)
            {
                primaryLabel.Put(Safe(source.Primary.Label));
                buildingAction.interactable =
                    source.Offer != null && source.Primary.Button && source.Primary.Button.interactable;
            }
        }

        private static void Append(StringBuilder buffer, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            if (buffer.Length > 0) buffer.Append("\n\n");
            buffer.Append(value.Trim());
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying) return;
            EnsureView();
            if (!ready) return;
            if (!hasGeometry || Mathf.Abs(safe.rect.width - lastWidth) > .5f ||
                Mathf.Abs(safe.rect.height - lastHeight) > .5f) Layout();
            if (!hasGeometry) return;
            bool modalOpen = Source.Modal && Source.Modal.gameObject.activeSelf;

            chapterText.Put(Safe(Source.Mission.Chapter));
            objectiveText.Put(Safe(Source.Mission.Objective));
            progressText.Put(Safe(Source.Mission.ProgressText));

            bool showExpanded = expanded && !modalOpen;
            if (Source.Mission.gameObject.activeSelf != showExpanded)
                Source.Mission.gameObject.SetActive(showExpanded);
            if (showExpanded)
            {
                Source.Mission.SetSelectionCompact(false);
                Source.Mission.ApplyLayout(safe.rect.width);
                var legacy = (RectTransform)Source.Mission.transform;
                legacy.anchoredPosition = new Vector2(portrait ? 16f : 26f, -(portrait ? 245f : 145f));
            }
            objectiveCard.gameObject.SetActive(!expanded && !modalOpen);
            chapterClose.gameObject.SetActive(showExpanded);

            RenderBuilding();
            if (buildingCard.gameObject.activeSelf && modalOpen) buildingCard.gameObject.SetActive(false);

            // The V1 detail sheet is only a data/command source in this isolated
            // candidate. Hide it after each V1 refresh so it cannot intercept
            // input or obscure the actual Valoria scene.
            if (Source.Building && Source.Building.gameObject.activeSelf)
                Source.Building.gameObject.SetActive(false);
        }

        /// <summary>
        /// A camera-framing helper for the eventual safe integration. The V1
        /// method does not know the new V2 cards. No camera is changed here.
        /// </summary>
        public Rect GetFreeSceneViewport()
        {
            Rect free = Source ? Source.GetVisibleSceneViewport() : new Rect(.07f, .12f, .86f, .78f);
            if (!safe || !hasGeometry) return free;
            float width = Mathf.Max(1, safe.rect.width);
            float height = Mathf.Max(1, safe.rect.height);
            float padding = .025f;
            if (portrait && BuildingVisible)
                free.yMin = Mathf.Max(free.yMin, buildingCard.rect.height / height + padding);
            else if (!portrait && BuildingVisible)
                free.xMax = Mathf.Min(free.xMax, 1f - buildingCard.rect.width / width - padding);
            if (objectiveCard && objectiveCard.gameObject.activeInHierarchy)
            {
                if (portrait)
                {
                    float fromTop = -objectiveCard.anchoredPosition.y + objectiveCard.rect.height;
                    free.yMax = Mathf.Min(free.yMax, 1f - fromTop / height - padding);
                }
                else
                    free.xMin = Mathf.Max(free.xMin, objectiveCard.rect.width / width + padding);
            }
            return free;
        }
    }
}
