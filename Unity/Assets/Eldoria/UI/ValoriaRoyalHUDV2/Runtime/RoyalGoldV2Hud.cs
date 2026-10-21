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
        private RoyalText objectiveActionLabel, objectivesButtonLabel;
        private RoyalButton objectiveAction, objectivesButton, chapterClose, buildingClose, buildingAction;
        private ScrollRect buildingScroll;
        private CanvasGroup objectiveGroup, chapterCloseGroup, buildingGroup, buildingActionGroup;
        private bool buildingVisible;
        private bool ready, expanded, portrait, hasGeometry, compactObjective;
        private float lastWidth = -1f, lastHeight = -1f;
        private string lastDetails = "";
        private static readonly Color Ivory = new Color(.945f, .913f, .852f);
        private static readonly Color Muted = new Color(.775f, .76f, .715f);
        private static readonly Color Gold = new Color(.886f, .73f, .45f);

        public bool ChapterExpanded => expanded;
        public bool BuildingVisible
        {
            get
            {
                var v3 = GetComponent<RoyalGoldV3ContextHud>();
                return v3 && v3.IsConfigured ? v3.CloseVisible : buildingVisible;
            }
        }
        public RoyalButton VisibleObjectiveButton => objectiveAction;
        public RoyalButton VisibleChapterButton => objectivesButton;
        public RoyalButton VisibleBuildingButton
        {
            get
            {
                var v3 = GetComponent<RoyalGoldV3ContextHud>();
                return v3 && v3.IsConfigured ? v3.VisibleActionButton : buildingAction;
            }
        }
        public RoyalButton VisibleBuildingCloseButton
        {
            get
            {
                var v3 = GetComponent<RoyalGoldV3ContextHud>();
                return v3 && v3.IsConfigured ? v3.VisibleCloseButton : buildingClose;
            }
        }
        public bool ObjectiveVisible => objectiveGroup && objectiveGroup.blocksRaycasts;
        public bool BuildingActionVisible
        {
            get
            {
                var v3 = GetComponent<RoyalGoldV3ContextHud>();
                // The established WebGL telemetry reads this boolean before
                // clicking the actual V3 button. Do not silently mask it by
                // the intentionally hidden V2 building sheet CanvasGroup.
                return v3 && v3.IsConfigured
                    ? v3.ContextVisible && v3.VisibleActionButton &&
                        v3.VisibleActionButton.gameObject.activeInHierarchy
                    : buildingVisible && buildingActionGroup && buildingActionGroup.blocksRaycasts;
            }
        }

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

            // Approved Interface 1: the existing crest/name/power receive a
            // quiet midnight-blue royal plate INSIDE their existing bounds.
            // Decorative graphic only; the genuine realm button remains sole
            // hit target, and no extra overlay occupies city coordinates.
            var realm = Source.Realm ? Source.Realm.transform as RectTransform : null;
            if (realm && !realm.Find("V3 / subdued Valoria livery"))
            {
                var relief = NewRect("V3 / subdued Valoria livery", realm);
                relief.gameObject.AddComponent<CanvasRenderer>();
                var reliefSkin = relief.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
                reliefSkin.Primary = false;
                reliefSkin.DecorativeCorners = false;
                reliefSkin.raycastTarget = false;
                relief.anchorMin = Vector2.zero;
                relief.anchorMax = Vector2.one;
                relief.offsetMin = Vector2.zero;
                relief.offsetMax = Vector2.zero;
                relief.SetAsFirstSibling();
            }

            objectiveCard = Panel("V2 / current objective", safe);
            chapterText = Text("Chapter", objectiveCard, 18, Gold, true);
            objectiveText = Text("Current real objective", objectiveCard, 25, Ivory, true);
            progressText = Text("Objective progress", objectiveCard, 17, Muted);
            objectiveAction = Button("Go to existing objective", objectiveCard, out objectiveActionLabel, true);
            objectivesButton = Button("Open current chapter", objectiveCard, out objectivesButtonLabel, false);
            objectiveActionLabel.Put("Ir al objetivo");
            objectivesButtonLabel.Put("Misiones");
            // Unity offscreen portrait captures exposed a uGUI canvas ordering
            // defect: the original Text children were clipped to blank blue
            // button frames even though both real Button raycasts worked.
            // Keep the SAME Text objects, but draw them as later, non-raycast
            // siblings of the objective buttons on the one existing canvas.
            // This adds no duplicate action, EventSystem or touch surface.
            objectiveActionLabel.rectTransform.SetParent(objectiveCard, false);
            objectivesButtonLabel.rectTransform.SetParent(objectiveCard, false);
            objectiveActionLabel.raycastTarget = false;
            objectivesButtonLabel.raycastTarget = false;
            objectiveActionLabel.fontSize = 18;
            objectivesButtonLabel.fontSize = 18;
            objectiveActionLabel.alignment = TextAnchor.MiddleCenter;
            objectivesButtonLabel.alignment = TextAnchor.MiddleCenter;
            objectiveAction.onClick.AddListener(() =>
            {
                if (Source && Source.Mission && Source.Mission.Open)
                    Source.Mission.Open.onClick.Invoke();
            });
            objectivesButton.onClick.AddListener(() =>
            {
                expanded = true;
                // Keep the original checklist projection and toggle state.
                if (Source.Mission.ListToggle && !Source.Mission.ListExpanded)
                    Source.Mission.ListToggle.onClick.Invoke();
            });

            chapterClose = Button("Close chapter overlay", safe, out var closeChapterText, false);
            closeChapterText.Put("×");
            closeChapterText.fontSize = 30;
            chapterClose.onClick.AddListener(() =>
            {
                expanded = false;
                if (Source.Mission.ListToggle && Source.Mission.ListExpanded)
                    Source.Mission.ListToggle.onClick.Invoke();
            });

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
            // Keep dynamically created uGUI graphics registered with the
            // parent canvas. Disabling the GameObject caused new buttons to
            // have Graphic.depth=-1 even after the building changed state.
            objectiveGroup = objectiveCard.gameObject.AddComponent<CanvasGroup>();
            buildingGroup = buildingCard.gameObject.AddComponent<CanvasGroup>();
            buildingActionGroup = buildingAction.gameObject.AddComponent<CanvasGroup>();
            chapterCloseGroup = chapterClose.gameObject.AddComponent<CanvasGroup>();
            SetVisible(buildingGroup, false);
            SetVisible(buildingActionGroup, false);
            SetVisible(chapterCloseGroup, false);
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
            var bg = r.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
            bg.Primary = false;
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
            var frame = r.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
            frame.Primary = primary;
            frame.DecorativeCorners = false;
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
            float goalWidth = Mathf.Min(portrait ? 382f : 330f, width - 26f);
            // Existing header/resources/feedback occupy up to 280 units
            // on a portrait handset; never place the chapter over them.
            float goalTop = portrait ? 291f : 136f;
            Top(objectiveCard, portrait ? 14 : 18, goalTop, goalWidth, 119f);
            Top(chapterText.rectTransform, 13, 9, goalWidth - 114, 22);
            chapterText.fontSize = 17;
            Top(progressText.rectTransform, goalWidth - 98, 9, 84, 22);
            progressText.fontSize = 16;
            progressText.alignment = TextAnchor.MiddleRight;
            Top(objectiveText.rectTransform, 13, 31, goalWidth - 26, 41);
            objectiveText.fontSize = 20;
            Top((RectTransform)objectiveAction.transform, 10, 76, (goalWidth - 32f) * .58f, 39);
            Top((RectTransform)objectivesButton.transform, 16 + (goalWidth - 32f) * .58f,
                76, (goalWidth - 32f) * .42f, 39);
            PositionObjectiveActionLabels();
            // Reflow selected objectives after every orientation/size change.
            compactObjective = false;
            // Explicitly expanded chapter panels are allowed to be larger; the
            // default HUD remains only one short objective card.
            Top((RectTransform)chapterClose.transform,
                portrait ? Mathf.Max(10, width - 75f) : 268f,
                goalTop - 8f, 48, 48);

            float cardWidth = portrait ? Mathf.Min(width - 24, 720f) : Mathf.Min(420f, width * .34f);
            float cardHeight = portrait ? Mathf.Min(340f, height * .30f) : Mathf.Min(474f, height * .67f);
            // The bottom navigation is the only entrance to the World after
            // Bastion I builds the sawmill. The context sheet MUST start above
            // that navigation instead of silently covering its hit targets.
            var nav = Source.Navigation ? Source.Navigation.transform as RectTransform : null;
            float navTop = nav ? nav.anchoredPosition.y + nav.rect.height : 114f;
            float sheetBottom = Mathf.Max(128f, navTop + 14f);
            if (portrait)
                Place(buildingCard, new Vector2(.5f, 0), new Vector2(.5f, 0), 0, sheetBottom, cardWidth, cardHeight);
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

        /// <summary>The next real objective remains actionable while a
        /// destination is selected. Its reduced height does not create
        /// another HUD or obscure the contextual building action.</summary>
        private void ReflowSelectedObjective(bool compact)
        {
            if (!hasGeometry || compactObjective == compact) return;
            // V3 never expands an objective simply because a building was selected.
            // The original expanded checklist remains behind the explicit Misiones action.
            compactObjective = compact;
            float w = objectiveCard.rect.width;
            objectiveCard.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 119f);
            Top(chapterText.rectTransform, 13, 9, w - 114f, 22f);
            Top(progressText.rectTransform, w - 98f, 9, 84f, 22f);
            Top(objectiveText.rectTransform, 13, 31, w - 26f, 41f);
            objectiveText.fontSize = 20;
            Top((RectTransform)objectiveAction.transform, 10, 76, (w - 32f) * .58f, 39f);
            Top((RectTransform)objectivesButton.transform,
                16 + (w - 32f) * .58f, 76, (w - 32f) * .42f, 39f);
            PositionObjectiveActionLabels();
        }

        /// <summary>
        /// One visible native uGUI label per original clickable button.
        /// The labels are non-interactive siblings to avoid the portrait
        /// offscreen MaskableGraphic ordering/clip regression.
        /// </summary>
        private void PositionObjectiveActionLabels()
        {
            if (!objectiveActionLabel || !objectivesButtonLabel ||
                !objectiveAction || !objectivesButton) return;
            PlaceObjectiveLabel(objectiveActionLabel, (RectTransform)objectiveAction.transform);
            PlaceObjectiveLabel(objectivesButtonLabel, (RectTransform)objectivesButton.transform);
            objectiveActionLabel.transform.SetAsLastSibling();
            objectivesButtonLabel.transform.SetAsLastSibling();
        }

        private static void PlaceObjectiveLabel(RoyalText label, RectTransform button)
        {
            var x = button.anchoredPosition.x + 7f;
            var top = -button.anchoredPosition.y + 3f;
            Top(label.rectTransform, x, top,
                Mathf.Max(40f, button.rect.width - 14f),
                Mathf.Max(30f, button.rect.height - 6f));
            label.fontSize = 18;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
        }

        private static string Safe(RoyalText value) => value ? value.text ?? "" : "";

        private void RenderBuilding(bool narrativeOrModal)
        {
            var source = Source.Building;
            // V3 attaches to the same HUD: the old, oversized V2 sheet is
            // display-only and must never compete with object-anchored controls.
            var contextual = GetComponent<RoyalGoldV3ContextHud>();
            if (contextual && contextual.IsConfigured)
            {
                buildingVisible = false;
                SetVisible(buildingGroup, false);
                SetVisible(buildingActionGroup, false);
                return;
            }
            bool show = !narrativeOrModal && !string.IsNullOrEmpty(Source.SelectedBuildingId) &&
                source && source.Title && !string.IsNullOrWhiteSpace(source.Title.text);
            buildingVisible = show;
            SetVisible(buildingGroup, show);
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
            SetVisible(buildingActionGroup, offered);
            if (offered)
            {
                primaryLabel.Put(Safe(source.Primary.Label));
                buildingAction.interactable =
                    source.Offer != null && source.Primary.Button && source.Primary.Button.interactable;
            }
        }

        private static void SetVisible(CanvasGroup group, bool show)
        {
            if (!group) return;
            float opacity = show ? 1f : 0f;
            if (!Mathf.Approximately(group.alpha, opacity)) group.alpha = opacity;
            if (group.interactable != show) group.interactable = show;
            if (group.blocksRaycasts != show) group.blocksRaycasts = show;
        }

        private static void Append(StringBuilder buffer, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            if (buffer.Length > 0) buffer.Append("\n\n");
            buffer.Append(value.Trim());
        }

        private static void SetWidthAndHeight(RectTransform r, float w, float h)
        {
            if (Mathf.Abs(r.rect.width - w) > .2f)
                r.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
            if (Mathf.Abs(r.rect.height - h) > .2f)
                r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        }

        /// <summary>
        /// The existing navigation logic uses six 140-unit cells (over 900
        /// logical units). This extends past portrait safe areas, even when
        /// all its buttons work. Reflow ONLY existing, authorized actions.
        /// Do not create dummy buttons or hide usable navigation categories.
        /// </summary>
        private void LayoutNavigation()
        {
            if (!Source.Navigation || !safe) return;
            var nav = Source.Navigation;
            int count = 0;
            foreach (var item in nav.Items)
                if (item.Button && item.Button.gameObject.activeSelf) count++;
            if (count == 0) return;
            float space = Mathf.Max(250f, safe.rect.width - 30f);
            float gap = portrait ? 7f : 9f;
            float width = Mathf.Min(portrait ? 108f : 110f, (space - gap * (count - 1)) / count);
            float total = width * count + gap * (count - 1);
            var navRect = nav.transform as RectTransform;
            if (navRect)
            {
                SetWidthAndHeight(navRect, total, 77f);
                if ((navRect.anchoredPosition - new Vector2(0, 10f)).sqrMagnitude > .1f)
                    navRect.anchoredPosition = new Vector2(0, 10f);
            }
            float icon = Mathf.Min(43f, width - 10f);
            int index = 0;
            foreach (var item in nav.Items)
            {
                if (!item.Button || !item.Button.gameObject.activeSelf) continue;
                var cell = (RectTransform)item.Button.transform;
                SetWidthAndHeight(cell, width, 77f);
                float x = -total * .5f + width * .5f + index++ * (width + gap);
                if ((cell.anchoredPosition - new Vector2(x, 0)).sqrMagnitude > .1f)
                    cell.anchoredPosition = new Vector2(x, 0);
                // The canonical prefab has one medallion child and one label.
                // Icons retain the existing medieval artwork and listeners.
                if (cell.childCount > 0 && cell.GetChild(0) is RectTransform medallion)
                {
                    SetWidthAndHeight(medallion, icon, icon);
                    Vector2 position = new Vector2((width - icon) * .5f, -2f);
                    if ((medallion.anchoredPosition - position).sqrMagnitude > .1f)
                        medallion.anchoredPosition = position;
                }
                if (item.Label)
                {
                    var label = item.Label.rectTransform;
                    SetWidthAndHeight(label, width, 22f);
                    Vector2 position = new Vector2(0, -49f);
                    if ((label.anchoredPosition - position).sqrMagnitude > .1f)
                        label.anchoredPosition = position;
                    if (item.Label.fontSize != 17) item.Label.fontSize = 17;
                    item.Label.alignment = TextAnchor.MiddleCenter;
                }
            }
        }

        /// <summary>
        /// Long live feedback must wrap inside the actual mobile safe area,
        /// rather than projecting beyond the right edge over the city.
        /// This only affects the existing text component; no new HUD/toast.
        /// </summary>
        private void ConfigureFeedbackReadability()
        {
            if (!Source || !Source.Feedback || !safe) return;
            var feedback = Source.Feedback;
            feedback.raycastTarget = false;
            feedback.alignment = TextAnchor.MiddleCenter;
            feedback.horizontalOverflow = HorizontalWrapMode.Wrap;
            feedback.verticalOverflow = VerticalWrapMode.Truncate;
            int size = portrait ? 19 : 20;
            if (feedback.fontSize != size) feedback.fontSize = size;
            var rect = feedback.rectTransform;
            float width = Mathf.Min(portrait ? safe.rect.width - 36f : 690f,
                safe.rect.width - 26f);
            SetWidthAndHeight(rect, Mathf.Max(240f, width), portrait ? 56f : 48f);
        }

        private static void RebindObjectiveLabel(RoyalText label)
        {
            if (!label) return;
            // Never remove/recreate a button or listener. UI.Graphic.OnEnable
            // re-registers this same child on the current canvas after the
            // orientation/camera switch (including the native capture path).
            label.enabled = false;
            label.enabled = true;
            label.SetAllDirty();
        }

        private void LateUpdate() => SynchronizeNow();

        /// <summary>Explicit native QA sync after temporary canvas resolution
        /// changes. Identical to the normal Unity LateUpdate rendering path.</summary>
        public void SynchronizeNow()
        {
            if (!Application.isPlaying) return;
            EnsureView();
            if (!ready) return;
            bool resized = !hasGeometry || Mathf.Abs(safe.rect.width - lastWidth) > .5f ||
                Mathf.Abs(safe.rect.height - lastHeight) > .5f;
            if (resized)
            {
                Layout();
                // Native portrait captures and live orientation switches alter
                // the stretched button rectangles while the text string stays
                // unchanged. Explicitly rebuild their uGUI vertices so these
                // genuine mission labels cannot render as empty gold frames.
                // A camera-backed offscreen portrait capture can leave the
                // dynamic child Text registered at Graphic.depth=-1 after
                // switching from landscape. Re-register its Graphic rather
                // than merely marking cached geometry dirty: without this,
                // the actual clickable mission buttons render WITHOUT text.
                RebindObjectiveLabel(objectiveActionLabel);
                RebindObjectiveLabel(objectivesButtonLabel);
            }
            if (!hasGeometry) return;
            LayoutNavigation();
            ConfigureFeedbackReadability();
            bool modalOpen = Source.Modal && Source.Modal.gameObject.activeSelf;

            chapterText.Put(Safe(Source.Mission.Chapter));
            objectiveText.Put(Safe(Source.Mission.Objective));
            progressText.Put(Safe(Source.Mission.ProgressText));

            // Closing the canonical checklist also restores the compact
            // current-objective action. Otherwise it remains invisible behind
            // a nominally closed V1 list in the V2 expanded overlay.
            if (expanded && !Source.Mission.ListExpanded) expanded = false;
            bool showExpanded = expanded && !modalOpen;
            bool chapterJustOpened = Source.Mission.gameObject.activeSelf != showExpanded;
            if (chapterJustOpened)
                Source.Mission.gameObject.SetActive(showExpanded);
            if (showExpanded)
            {
                Source.Mission.SetSelectionCompact(false);
                if (chapterJustOpened || resized)
                    Source.Mission.ApplyLayout(safe.rect.width);
                var legacy = (RectTransform)Source.Mission.transform;
                legacy.anchoredPosition = new Vector2(portrait ? 16f : 26f, -(portrait ? 245f : 145f));
            }
            // When a building is selected, prioritize its actions and the
            // scene itself. Never stack a full-width chapter card above the
            // bottom building sheet on a portrait handset.
            bool inspectingBuilding = !string.IsNullOrEmpty(Source.SelectedBuildingId);
            // Previously hiding all objectives during selection stranded
            // a player on the Forest resource after its first expedition.
            // Keep a compact, *genuine* next-objective action available.
            ReflowSelectedObjective(inspectingBuilding);
            SetVisible(objectiveGroup, !expanded && !modalOpen);
            SetVisible(chapterCloseGroup, showExpanded);

            RenderBuilding(modalOpen);

            // The V1 detail sheet is only a data/command source in this isolated
            // candidate. Hide it after each V1 refresh so it cannot intercept
            // input or obscure the actual Valoria scene.
            if (Source.Building && Source.Building.gameObject.activeSelf)
                Source.Building.gameObject.SetActive(false);
            // Existing native gameplay/capture harness calls this synchronously;
            // the reprojected V3 button must be the actual hittable button.
            var v3 = GetComponent<RoyalGoldV3ContextHud>();
            if (v3 && v3.IsConfigured) v3.SynchronizeNow();
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
            if (portrait && buildingVisible)
                free.yMin = Mathf.Max(free.yMin,
                    (buildingCard.anchoredPosition.y + buildingCard.rect.height) / height + padding);
            else if (!portrait && buildingVisible)
                free.xMax = Mathf.Min(free.xMax, 1f - buildingCard.rect.width / width - padding);
            // The V3 selected control follows the object itself and does not
            // reduce a whole side of the playable camera rectangle.
            if (objectiveGroup && objectiveGroup.blocksRaycasts)
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
