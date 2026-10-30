using System;
using System.Text;
using System.Linq;
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

        private RectTransform safe, headerPlate, objectiveCard, heraldicBanner, buildingCard, buildingMiniature, scrollContent, scrollViewport;
        private RoyalText chapterText, objectiveText, progressText, buildingTitle, buildingLevel, buildingBody, primaryLabel;
        private RoyalText objectiveActionLabel, objectivesButtonLabel;
        private RoyalButton objectiveAction, objectivesButton, chapterClose, buildingClose, buildingAction;
        private ScrollRect buildingScroll;
        private RoyalGoldV3BuildingEngravingGraphic miniatureArt;
        private CanvasGroup objectiveGroup, chapterCloseGroup, buildingGroup, buildingActionGroup;
        private bool buildingVisible;
        private bool ready, expanded, portrait, hasGeometry, compactObjective;
        private float lastWidth = -1f, lastHeight = -1f;
        private string lastDetails = "";
        private static Font chronicleFont;
        private static Font ChronicleFont => chronicleFont ? chronicleFont :
            (chronicleFont = Resources.Load<Font>("EldoriaRetention/reference-serif"));
        private static readonly Color Ivory = new Color(.945f, .913f, .852f);
        private static readonly Color Muted = new Color(.775f, .76f, .715f);
        private static readonly Color Gold = new Color(.886f, .73f, .45f);

        public bool ChapterExpanded => expanded;
        public bool ChronicleIllustratedBuildingVisible => buildingGroup && buildingGroup.alpha > .5f;
        public RoyalButton ChronicleBuildingAction => buildingAction;
        public RoyalButton ChronicleBuildingClose => buildingClose;
        public RectTransform ChronicleBuildingRect => buildingCard;
        public bool ChronicleBuildingActionVisible => ChronicleIllustratedBuildingVisible &&
            buildingActionGroup && buildingActionGroup.blocksRaycasts &&
            Source && Source.Building && !Source.Building.OperationActive;

        // A purchase can refresh the gateway between LateUpdate calls. Remove
        // the consumed action immediately without changing the operation or save.
        public void HideConsumedBuildingAction()
        {
            SetVisible(buildingActionGroup, false);
            if (buildingAction) buildingAction.interactable = false;
        }
        public bool BuildingVisible
        {
            get
            {
                var management = GetComponent<RoyalGoldV3BuildingScreens>();if(management&&management.IsOpen)return true;
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
                var management = GetComponent<RoyalGoldV3BuildingScreens>();if(management&&management.IsOpen)return management.VisibleAction;
                var v3 = GetComponent<RoyalGoldV3ContextHud>();
                return v3 && v3.IsConfigured ? v3.VisibleActionButton : buildingAction;
            }
        }
        public RoyalButton VisibleBuildingCloseButton
        {
            get
            {
                var management = GetComponent<RoyalGoldV3BuildingScreens>();if(management&&management.IsOpen)return management.VisibleClose;
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
                var management = GetComponent<RoyalGoldV3BuildingScreens>();if(management&&management.IsOpen)return management.VisibleAction;
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
            if (UnityEngine.Application.isPlaying) EnsureView();
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
            // Original approved footer has five real destinations visible even
            // before unlock. Blocked destinations remain disabled; no gameplay
            // permission, reward or automatic resource action is created.
            if (Source.Navigation) Source.Navigation.ChronicleLayout = true;

            // Approved Interface 1: the existing crest/name/power receive a
            // quiet midnight-blue royal plate INSIDE their existing bounds.
            // Decorative graphic only; the genuine realm button remains sole
            // hit target, and no extra overlay occupies city coordinates.
            var realm = Source.Realm ? Source.Realm.transform as RectTransform : null;
            if (realm && !FindDirect(realm, "V3 / subdued Valoria livery"))
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
                // Reuse only the REAL Valoria crest artwork already in Unity.
                // Invert the old beige shield to noble midnight-blue and make
                // the existing Bastion emblem antique gold; no new fake icon.
                var crest = realm.Find("Valoria crest");
                if (crest)
                {
                    var shield = crest.GetComponent<RoyalIconGraphic>();
                    if (shield) shield.color = new Color(.078f, .148f, .265f, 1f);
                    var bastionEmblem = crest.Find("Bastion emblem");
                    if (bastionEmblem)
                    {
                        var emblem = bastionEmblem.GetComponent<RoyalIconGraphic>();
                        if (emblem) emblem.color = new Color(.92f, .75f, .43f, 1f);
                    }
                }
                if (Source.Realm.Realm)
                    Source.Realm.Realm.color = new Color(.96f, .84f, .61f, 1f);
                if (Source.Realm.Power)
                    Source.Realm.Power.color = new Color(.80f, .82f, .81f, 1f);
            }

            // The approved original has ONE unbroken midnight-blue / antique
            // gold royal header across the *entire safe edge*. Keep the actual
            // V1 controls above it; this decoration never catches touch input.
            headerPlate = NewRect("V3 / continuous original royal header", safe);
            headerPlate.gameObject.AddComponent<CanvasRenderer>();
            var plateSkin = headerPlate.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
            plateSkin.Primary = false;
            plateSkin.DecorativeCorners = false;
            plateSkin.color = Color.white;
            plateSkin.raycastTarget = false;
            headerPlate.SetAsFirstSibling();

            objectiveCard = Panel("V2 / current objective", safe);
            // Original Crónica Real's royal heraldry hangs beneath the
            // illuminated parchment in landscape. It is hidden in portrait
            // to protect the usable view and genuine SafeArea controls.
            heraldicBanner = NewRect("V3 / royal hanging chapter banner", objectiveCard);
            heraldicBanner.gameObject.AddComponent<CanvasRenderer>();
            var bannerSkin = heraldicBanner.gameObject.AddComponent<RoyalGoldV3HeraldicBannerGraphic>();
            bannerSkin.raycastTarget = false;
            var pendantSigil = NewRect("V3 / genuine Valoria castle insignia", heraldicBanner);
            pendantSigil.gameObject.AddComponent<CanvasRenderer>();
            var standardIcon = pendantSigil.gameObject.AddComponent<RoyalIconGraphic>();
            standardIcon.Icon = 9; // same Bastión visual language as the REAL navigation
            standardIcon.color = new Color(.91f, .73f, .42f, .97f);
            standardIcon.raycastTarget = false;
            Top(pendantSigil, 8f, 75f, 32f, 35f);
            heraldicBanner.SetAsFirstSibling();
            // Restore the approved Crónica Real parchment with heraldries.
            // Text/mission/button objects are the SAME native gameplay ones.
            var oldSlateFrame = objectiveCard.GetComponent<RoyalGoldV2FrameGraphic>();
            if (oldSlateFrame) { oldSlateFrame.enabled = false; oldSlateFrame.raycastTarget = false; }
            var manuscript = NewRect("V3 / original parchment and engraved edges", objectiveCard);
            manuscript.gameObject.AddComponent<CanvasRenderer>();
            var paperSkin = manuscript.gameObject.AddComponent<RoyalGoldV3ObjectiveParchmentGraphic>();
            paperSkin.raycastTarget = false;
            Stretch(manuscript, 0f);
            manuscript.SetAsFirstSibling();
            // A decorative repetition of the REAL Valoria shield, not a new
            // faction, invented heraldic symbol, or second gameplay button.
            var originalCrest = Source.Realm ? Source.Realm.transform.Find("Valoria crest") : null;
            if (originalCrest)
            {
                var heraldry = UnityEngine.Object.Instantiate(originalCrest.gameObject, objectiveCard);
                heraldry.name = "V3 / original Valoria heraldic sigil";
                var shieldRect = heraldry.transform as RectTransform;
                if (shieldRect) Top(shieldRect, 11f, 18f, 47f, 67f);
                foreach (var graphic in heraldry.GetComponentsInChildren<Graphic>(true))
                    graphic.raycastTarget = false;
            }
            var manuscriptInk = new Color(.22f, .155f, .095f, 1f);
            chapterText = Text("Chapter", objectiveCard, 17, manuscriptInk, true);
            chapterText.fontStyle = FontStyle.Bold;
            objectiveText = Text("Current real objective", objectiveCard, 20, manuscriptInk, true);
            progressText = Text("Objective progress", objectiveCard, 16,
                new Color(.44f, .30f, .15f, 1f));
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
            // The approved manuscript has TWO circular medieval miniatures
            // BESIDE the parchment, not flat CTAs across its lower half.
            // Keep the actual mission and go-to-objective callbacks intact.
            StyleChronicleMedallion(objectiveAction, 12);
            StyleChronicleMedallion(objectivesButton, 6);
            objectiveActionLabel.raycastTarget = false;
            objectivesButtonLabel.raycastTarget = false;
            // Real actions now have their original pictogram treatment.
            // Their accessibility names and events survive, but duplicate
            // rectangular captions are not drawn over the medallions.
            objectiveActionLabel.enabled = false;
            objectivesButtonLabel.enabled = false;
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
            // The approved Crónica Real has an architectural illustration inside
            // its broad bottom building cartouche. Draw an actual live, authored
            // native miniature rather than a placeholder or static UI screenshot.
            buildingMiniature = Panel("V3 / engraved real building miniature", buildingCard);
            var miniSkin = buildingMiniature.GetComponent<RoyalGoldV2FrameGraphic>();
            miniSkin.raycastTarget = false;
            miniSkin.DecorativeCorners = true;
            var miniSurface = NewRect("V3 / illuminated architectural illustration", buildingMiniature);
            miniSurface.gameObject.AddComponent<CanvasRenderer>();
            miniatureArt = miniSurface.gameObject.AddComponent<RoyalGoldV3BuildingEngravingGraphic>();
            miniatureArt.raycastTarget = false;
            Stretch(miniSurface, 5f);
            buildingTitle = Text("Selected building", buildingCard, 30, Ivory, true);
            buildingLevel = Text("Real level", buildingCard, 18, Gold);
            buildingClose = Button("Close selected building", buildingCard, out var closeBuildingText, false);
            closeBuildingText.Put("×");
            closeBuildingText.fontSize = 24;
            Stretch(closeBuildingText.rectTransform, 2f);
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

        // Transform.Find interprets '/' as a hierarchy separator. Our approved
        // Crónica/Valoria decoration names contain literal slashes; match only
        // immediate-child names to prevent false negatives and duplicate art
        // every time an orientation/layout refresh runs.
        private static Transform FindDirect(Transform parent, string exactName)
        {
            if (!parent) return null;
            foreach (Transform child in parent)
                if (child.name == exactName) return child;
            return null;
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
            label.font = ChronicleFont ? ChronicleFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            frame.AntiqueGoldAction = primary;
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

        private static void StyleChronicleMedallion(RoyalButton button, int icon)
        {
            if (!button || FindDirect(button.transform, "Crónica / native round icon"))
                return;
            var rectangularFrame = button.GetComponent<RoyalGoldV2FrameGraphic>();
            if (rectangularFrame)
            {
                rectangularFrame.raycastTarget = false;
                rectangularFrame.enabled = false;
            }
            var ring = NewRect("Crónica / native round icon", button.transform);
            ring.gameObject.AddComponent<CanvasRenderer>();
            var enamel = ring.gameObject.AddComponent<RoyalFrameGraphic>();
            enamel.Round = true;
            enamel.Variant = 1;
            enamel.raycastTarget = true;
            Stretch(ring, 0f);
            ring.SetAsFirstSibling();
            button.targetGraphic = enamel;
            var symbol = NewRect("Crónica / real mission pictogram", ring);
            symbol.gameObject.AddComponent<CanvasRenderer>();
            var artwork = symbol.gameObject.AddComponent<RoyalIconGraphic>();
            artwork.Icon = icon;
            artwork.color = new Color(.95f, .79f, .46f, 1f);
            artwork.raycastTarget = false;
            symbol.anchorMin = symbol.anchorMax = symbol.pivot =
                new Vector2(.5f, .5f);
            symbol.anchoredPosition = Vector2.zero;
            symbol.sizeDelta = new Vector2(32f, 32f);
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
            float goalWidth = Mathf.Min(portrait ? 280f : 430f,
                width - 142f);
            // The genuine royal header is 104 units tall. Keep the
            // illuminated chapter *below* its bounds, as in Option 1.
            // The previous 88-unit top offset crossed the Valoria header.
            float goalTop = portrait ? 287f : 122f;
            // A shorter illuminated manuscript; the scene remains open.
            Top(objectiveCard, portrait ? 9f : 14f, goalTop,
                goalWidth, 113f);
            if (heraldicBanner)
            {
                heraldicBanner.gameObject.SetActive(!portrait);
                Top(heraldicBanner, 16f, 72f, 47f, 150f);
            }
            LayoutManuscriptContent(goalWidth);
            // Reflow selected objectives after every orientation/size change.
            compactObjective = false;
            // Explicitly expanded chapter panels are allowed to be larger; the
            // default HUD remains only one short objective card.
            Top((RectTransform)chapterClose.transform,
                portrait ? Mathf.Max(10, width - 75f) : 268f,
                goalTop - 8f, 48, 48);

            // Crónica Real: broad bottom cartouche, illustrated on the left,
            // details in the middle and the ORIGINAL action at the right.
            // Keep only the timer projected beside the actual building.
            // This cartouche owns the sole canonical purchase control.
            float cardWidth = Mathf.Min(width - (portrait ? 18f : 32f),
                portrait ? 720f : 1300f);
            float cardHeight = portrait ? 178f : 164f;
            var nav = Source.Navigation ? Source.Navigation.transform as RectTransform : null;
            float navTop = nav ? nav.anchoredPosition.y + nav.rect.height : 114f;
            float sheetBottom = Mathf.Max(8f, navTop + 8f);
            Place(buildingCard, new Vector2(.5f, 0), new Vector2(.5f, 0),
                0, sheetBottom, cardWidth, cardHeight);
            // The narrow phone keeps one illustrated thumbnail and a real
            // right-hand text/CTA column. Never squeeze its text into a 90px
            // sliver to fit a third decorative region.
            float picture = portrait ? 137f : cardHeight - 19f;
            Place(buildingMiniature, new Vector2(0, 1), new Vector2(0, 1),
                9f, -9f, picture, picture);
            float textLeft = picture + 24f;
            float actionWidth = portrait ? 0f : 194f;
            float middleWidth = Mathf.Max(115f, cardWidth - textLeft -
                actionWidth - (portrait ? 14f : 24f));
            Top(buildingTitle.rectTransform, textLeft, 10f, middleWidth, 35f);
            buildingTitle.fontSize = portrait ? 22 : 30;
            Top(buildingLevel.rectTransform, textLeft, 44f, middleWidth, 22f);
            Top((RectTransform)buildingClose.transform, cardWidth - 49f, 5f, 44f, 44f);
            Top((RectTransform)buildingScroll.transform, textLeft, 71f,
                middleWidth, portrait ? 50f : cardHeight - 79f);
            Stretch(scrollViewport, 0);
            scrollContent.anchorMin = new Vector2(0, 1);
            scrollContent.anchorMax = new Vector2(1, 1);
            scrollContent.pivot = new Vector2(.5f, 1);
            scrollContent.anchoredPosition = Vector2.zero;
            scrollContent.sizeDelta = new Vector2(0, Mathf.Max(85f, scrollViewport.rect.height));
            Top(buildingBody.rectTransform, 0, 0, middleWidth - 4f,
                scrollContent.rect.height);
            buildingBody.fontSize = portrait ? 15 : 19;
            if (portrait)
                Top((RectTransform)buildingAction.transform,
                    textLeft, cardHeight - 53f, middleWidth, 44f);
            else
                Top((RectTransform)buildingAction.transform,
                    cardWidth - actionWidth - 12f, cardHeight - 66f,
                    actionWidth, 51f);
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
            // Both compact and selected states preserve the same miniature
            // parchment. An expanded chapter is an explicit player action.
            compactObjective = compact;
            objectiveCard.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 113f);
            LayoutManuscriptContent(objectiveCard.rect.width);
        }

        private void LayoutManuscriptContent(float width)
        {
            // Match Option 1's 2:1 illuminated page: no full-height overlay.
            Top(chapterText.rectTransform, 65f, 10f, width - 78f, 23f);
            chapterText.fontSize = width < 300f ? 15 : 18;
            Top(progressText.rectTransform, width - 66f, 10f, 52f, 22f);
            progressText.fontSize = 13;
            progressText.alignment = TextAnchor.MiddleRight;
            Top(objectiveText.rectTransform, 65f, 36f, width - 79f, 64f);
            objectiveText.fontSize = width < 300f ? 17 : 20;
            // Real "go to objective" / "missions" actions, in two rings
            // to the RIGHT of the parchment like the selected reference.
            const float diameter = 54f;
            Top((RectTransform)objectiveAction.transform,
                width + 9f, 8f, diameter, diameter);
            Top((RectTransform)objectivesButton.transform,
                width + 66f, 8f, diameter, diameter);
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
            // V3 owns the in-world projected action/timer. This is a separate
            // art-only companion cartouche reading the SAME canonical offer;
            // it does not create a second clock or a second gameplay command.
            bool show = !narrativeOrModal && !string.IsNullOrEmpty(Source.SelectedBuildingId) &&
                source && source.Title && !string.IsNullOrWhiteSpace(source.Title.text);
            buildingVisible = show;
            SetVisible(buildingGroup, show);
            if (!show) return;
            buildingTitle.Put(Safe(source.Title));
            buildingLevel.Put(Safe(source.Level));
            if (miniatureArt)
            {
                string selectedId = Source.SelectedBuildingId ?? "";
                int kind = selectedId == "granary" ? 4 :
                    selectedId == "bastion" ? 3 :
                    selectedId == "barracks" || selectedId == "march-main" ? 2 :
                    selectedId == "sawmill" ? 1 : 0;
                if (miniatureArt.Kind != kind)
                {
                    miniatureArt.Kind = kind;
                    miniatureArt.SetVerticesDirty();
                }
            }

            var body = new StringBuilder(280);
            // Put purchase information before descriptive prose so the
            // compact viewport exposes the cost before the player commits.
            Append(body, Safe(source.Costs));
            Append(body, Safe(source.Requirements));
            Append(body, Safe(source.Time));
            Append(body, Safe(source.Status));
            Append(body, Safe(source.Function));
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
            bool offered = !source.OperationActive && source.Primary && source.Primary.Visible;
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
        private void LayoutHeaderAlongSafeTop()
        {
            // Original approved Crónica Real: a SINGLE continuous navy/gold
            // top band, rather than the modern phone's floating second strip.
            // All resource totals and Valoria identity are still V1 values.
            float w = safe.rect.width;
            bool showFood = Source.Resources && Source.Resources.FoodVisible;
            float realmWidth = portrait ? Mathf.Min(160f, w * .39f) : 275f;
            float headerHeight = portrait ? 62f : 78f;
            float settingsSize = portrait ? 56f : 76f;
            if (headerPlate)
            {
                // Real resources may wrap to a second line in narrow portrait.
                // Keep them within the same top band, never behind the quest.
                float resourceRoom = w - realmWidth - settingsSize - 17f;
                bool wrapped = portrait && Source.Resources &&
                    resourceRoom / (Source.Resources.FoodVisible ? 3f : 2f) < 59f;
                headerPlate.anchorMin = headerPlate.anchorMax =
                    headerPlate.pivot = new Vector2(0f, 1f);
                headerPlate.anchoredPosition = Vector2.zero;
                SetWidthAndHeight(headerPlate, w,
                    headerHeight + (wrapped ? 63f : 0f));
                headerPlate.SetAsFirstSibling();
            }


            var realm = Source.Realm ? Source.Realm.transform as RectTransform : null;
            if (realm)
            {
                realm.anchorMin = realm.anchorMax = new Vector2(0f, 1f);
                realm.pivot = new Vector2(0f, 1f);
                realm.anchoredPosition = new Vector2(3f, 0f);
                SetWidthAndHeight(realm, realmWidth, headerHeight);
                var royalCrest = realm.Find("Valoria crest") as RectTransform;
                float crestSize = portrait ? 54f : 70f;
                if (royalCrest)
                {
                    Top(royalCrest, 3f, 3f, crestSize, crestSize);
                    var bastion = royalCrest.Find("Bastion emblem") as RectTransform;
                    if (bastion)
                        Top(bastion, crestSize * .22f, crestSize * .16f,
                            crestSize * .59f, crestSize * .59f);
                }
                float titleX = crestSize + 11f;
                if (Source.Realm.Realm)
                {
                    Top(Source.Realm.Realm.rectTransform, titleX,
                        5f, realmWidth - titleX - 2f, portrait ? 28f : 36f);
                    Source.Realm.Realm.fontSize = portrait ? 20 : 27;
                }
                if (Source.Realm.Power)
                {
                    Top(Source.Realm.Power.rectTransform, titleX,
                        portrait ? 32f : 46f, realmWidth - titleX - 2f, 23f);
                    Source.Realm.Power.fontSize = portrait ? 15 : 19;
                }
            }

            var settings = Source.Settings ? Source.Settings.transform as RectTransform : null;
            if (settings)
            {
                var seal = Source.Settings.GetComponent<RoyalFrameGraphic>();
                if (seal && !seal.Round)
                {
                    seal.Round = true;
                    seal.SetVerticesDirty();
                }
                settings.anchorMin = settings.anchorMax = new Vector2(1f, 1f);
                settings.pivot = new Vector2(1f, 1f);
                settings.anchoredPosition = new Vector2(-3f, 0f);
                SetWidthAndHeight(settings, settingsSize, settingsSize);
                var icon = settings.Find("Symbol") as RectTransform;
                if (icon)
                {
                    icon.anchorMin = icon.anchorMax = icon.pivot =
                        new Vector2(.5f, .5f);
                    SetWidthAndHeight(icon, settingsSize * .66f,
                        settingsSize * .66f);
                    icon.anchoredPosition = Vector2.zero;
                }
            }

            var resources = Source.Resources ? Source.Resources.transform as RectTransform : null;
            if (!resources) return;
            int n = showFood ? 3 : 2;
            float slotMin = 59f;
            float room = w - realmWidth - settingsSize - 17f;
            bool secondRow = portrait && room / n < slotMin;
            float resourceWidth = secondRow
                ? Mathf.Min(w - 22f, n * 116f)
                : portrait ? room : n * 165f;
            resourceWidth = Mathf.Max(n * 40f, resourceWidth);
            resources.anchorMin = resources.anchorMax = new Vector2(0f, 1f);
            resources.pivot = new Vector2(0f, 1f);
            resources.anchoredPosition = new Vector2(
                secondRow ? 11f : portrait ? realmWidth + 8f :
                    Mathf.Max(realmWidth + 10f, (w - resourceWidth) * .5f),
                secondRow ? -headerHeight - 2f : 0f);
            SetWidthAndHeight(resources, resourceWidth,
                portrait ? 60f : headerHeight);
            float chip = resourceWidth / n;
            LayoutOriginalResource(resources, "Madera", Source.Resources.Wood,
                0f, chip, portrait);
            LayoutOriginalResource(resources, "Piedra", Source.Resources.Stone,
                chip, chip, portrait);
            if (showFood)
                LayoutOriginalResource(resources, "Comida", Source.Resources.Food,
                    chip * 2f, chip, portrait);

            if (portrait && objectiveCard)
            {
                // Keep the original quest parchment immediately BELOW the
                // royal top band. An unlocked third resource is a native
                // value, not a reason to bury the objective halfway down.
                float questY = secondRow ? headerHeight + 65f : 72f;
                objectiveCard.anchoredPosition = new Vector2(
                    objectiveCard.anchoredPosition.x, -questY);
                // Keep the expanded-chapter close action beside the ACTUAL
                // manuscript, not at the obsolete 287px portrait position.
                if (chapterClose)
                    Top((RectTransform)chapterClose.transform,
                        Mathf.Max(10f,w-58f),questY+5f,42f,42f);
            }
        }

        private static void LayoutOriginalResource(RectTransform parent,
            string name, RoyalText value, float x, float chip, bool portrait)
        {
            if (!value) return;
            // In the approved reference numbers have icons but NO extra
            // stacked "Madera/Piedra/Comida" names stealing vertical space.
            var word = parent.Find(name + " name");
            if (word && word.gameObject.activeSelf)
                word.gameObject.SetActive(false);
            float iconSize = portrait
                ? Mathf.Clamp(chip * .27f, 14f, 25f) : 36f;
            var glyph = parent.Find(name + " icon") as RectTransform;
            if (glyph)
                Top(glyph, x + 3f, portrait ? 17f : 17f,
                    iconSize, iconSize);
            float textX = x + iconSize + 4f;
            Top(value.rectTransform, textX, portrait ? 13f : 18f,
                Mathf.Max(12f, chip - iconSize - 5f), portrait ? 33f : 40f);
            value.fontSize = portrait
                ? Mathf.RoundToInt(Mathf.Clamp(chip * .20f, 12f, 19f)) : 23;
            value.alignment = TextAnchor.MiddleLeft;
            value.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        // First-frame layout creates the card before the live footer has been
        // reflowed to the current orientation. Always position the illustrated
        // card AFTER the footer layout, otherwise on a phone it covers the
        // very medallions the player needs to continue building.
        private void AlignBuildingCartoucheToNavigation()
        {
            if (!buildingCard || !Source || !Source.Navigation) return;
            var dock = Source.Navigation.transform as RectTransform;
            if (!dock) return;
            float safeBottom = Mathf.Max(8f,
                dock.anchoredPosition.y + dock.rect.height + 8f);
            var at = buildingCard.anchoredPosition;
            if (Mathf.Abs(at.y-safeBottom)>.5f)
                buildingCard.anchoredPosition = new Vector2(at.x,safeBottom);
        }

        private void LayoutNavigation()
        {
            if (!Source.Navigation || !safe) return;
            var nav = Source.Navigation;
            int count = 0;
            foreach (var item in nav.Items)
                if (item.Button && item.Button.gameObject.activeSelf) count++;
            if (count == 0) return;

            // CRÓNICA REAL APPROVED ART: ONE horizontal row of circular
            // medallions along the bottom edge in BOTH orientations. Only
            // extremely narrow devices may require a stacked accessible row;
            // never substitute a sidebar or invent locked destinations.
            bool vertical = safe.rect.height > safe.rect.width;
            float availableWidth = Mathf.Max(150f, safe.rect.width - 12f);
            float columnGap = vertical ? 4f : 9f;
            // A 390px portrait must NEVER squeeze five/six actions into
            // 51-unit targets with 10-point labels. Keep one row only while
            // each action retains at least 72 logical units; otherwise use
            // two compact edge-attached rows with the SAME live callbacks.
            int columns = vertical && count > 2 &&
                (availableWidth - 12f - columnGap * (count - 1)) / count < 72f
                ? Mathf.CeilToInt(count / 2f) : count;
            int rows = Mathf.CeilToInt((float)count / columns);
            float rowGap = rows > 1 ? 3f : 0f;
            // Original reference uses a wide bottom cartouche spanning most
            // of landscape. Spread the same genuine medallions across its
            // safe width, without enlarging or moving the game viewport.
            float cellWidth = vertical
                ? Mathf.Min(88f, (availableWidth - 12f -
                    columnGap * (columns - 1)) / columns)
                : Mathf.Clamp((availableWidth * .9f - 12f -
                    columnGap * (columns - 1)) / columns, 90f, 240f);
            // Never create a visual button that cannot fit its own safe width.
            cellWidth = Mathf.Max(1f, cellWidth);
            float cellHeight = rows > 1 ? 74f : vertical ? 105f : 106f;
            float contentHeight = rows * cellHeight + (rows - 1) * rowGap;
            float totalWidth = cellWidth * columns +
                columnGap * (columns - 1);
            float dockWidth = Mathf.Min(availableWidth,
                Mathf.Max(totalWidth + 12f, availableWidth * .90f));
            float dockHeight = contentHeight + 6f;

            var dock = nav.transform as RectTransform;
            if (!dock) return;
            dock.anchorMin = dock.anchorMax = new Vector2(.5f, 0f);
            dock.pivot = new Vector2(.5f, 0f);
            SetWidthAndHeight(dock, dockWidth, dockHeight);
            dock.anchoredPosition = Vector2.zero;

            // Reuse the original functional buttons. Only a separate,
            // non-raycasting blue and antique-gold plate is added behind them.
            var trim = FindDirect(dock, "V3 / Chronicle bottom frame") as RectTransform;
            if (!trim)
            {
                var frameObject = new GameObject("V3 / Chronicle bottom frame",
                    typeof(RectTransform), typeof(CanvasRenderer),
                    typeof(RoyalGoldV2FrameGraphic));
                frameObject.transform.SetParent(dock, false);
                frameObject.layer = dock.gameObject.layer;
                trim = (RectTransform)frameObject.transform;
                var frame = frameObject.GetComponent<RoyalGoldV2FrameGraphic>();
                frame.raycastTarget = false;
                frame.DecorativeCorners = true;
                frame.Primary = false;
                trim.SetAsFirstSibling();
            }
            trim.anchorMin = Vector2.zero;
            trim.anchorMax = Vector2.one;
            trim.offsetMin = Vector2.zero;
            trim.offsetMax = Vector2.zero;

            float medalSize = Mathf.Min(rows > 1 ? 46f : vertical ? 68f : 71f,
                cellWidth - 7f);
            float buttonHeight = cellHeight;
            int index = 0;
            foreach (var item in nav.Items.OrderBy(
                item => BottomNavigation.ChronicleOrder(item.Section)))
            {
                if (!item.Button || !item.Button.gameObject.activeSelf) continue;
                int row = index / columns;
                int column = index % columns;
                int inRow = Mathf.Min(columns, count - row * columns);
                float rowWidth = cellWidth * inRow +
                    columnGap * (inRow - 1);
                index++;
                var cell = (RectTransform)item.Button.transform;
                cell.anchorMin = cell.anchorMax = cell.pivot =
                    new Vector2(.5f, .5f);
                SetWidthAndHeight(cell, cellWidth, buttonHeight);
                // Child anchors are at the middle of the dock: add NO
                // half-dockHeight here. That old offset placed every button
                // partly OUTSIDE the bottom bar, visibly floating.
                cell.anchoredPosition = new Vector2(
                    -rowWidth * .5f + cellWidth * .5f +
                        column * (cellWidth + columnGap),
                    contentHeight * .5f - cellHeight * .5f -
                        row * (cellHeight + rowGap));
                if (cell.childCount > 0 &&
                    cell.GetChild(0) is RectTransform medal)
                {
                    medal.anchorMin = medal.anchorMax = medal.pivot =
                        new Vector2(.5f, .5f);
                    SetWidthAndHeight(medal, medalSize, medalSize);
                    medal.anchoredPosition = new Vector2(0f, 10f);
                    if (medal.childCount > 0 &&
                        medal.GetChild(0) is RectTransform glyph)
                    {
                        glyph.anchorMin = glyph.anchorMax = glyph.pivot =
                            new Vector2(.5f, .5f);
                        SetWidthAndHeight(glyph, medalSize * .67f,
                            medalSize * .67f);
                        glyph.anchoredPosition = Vector2.zero;
                        var icon = glyph.GetComponent<RoyalIconGraphic>();
                        int desired = item.Section == RoyalSection.Bastion ? 9 :
                            item.Section == RoyalSection.Inventory ? 15 :
                            item.Section == RoyalSection.Army ? 4 :
                            item.Section == RoyalSection.World ? 16 : 6;
                        if (icon && icon.Icon != desired)
                        {
                            icon.Icon = desired;
                            icon.SetVerticesDirty();
                        }
                    }
                }
                if (item.Label)
                {
                    var caption = item.Label.rectTransform;
                    caption.anchorMin = caption.anchorMax = caption.pivot =
                        new Vector2(.5f, .5f);
                    SetWidthAndHeight(caption, cellWidth - 2f, 20f);
                    caption.anchoredPosition =
                        new Vector2(0f, -buttonHeight * .33f);
                    item.Label.fontSize = Mathf.RoundToInt(Mathf.Clamp(
                        cellWidth * .19f, 13f, 17f));
                    if (ChronicleFont) item.Label.font = ChronicleFont;
                    item.Label.fontStyle = FontStyle.Normal;
                    item.Label.alignment = TextAnchor.MiddleCenter;
                    item.Label.color = Ivory;
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
            // Crónica Real's two REAL chapter buttons deliberately replace
            // their old captions with circular medieval pictograms. Do not
            // resurrect hidden rectangular labels after rotating the phone.
            // For any older caption still visible, keep the existing
            // uGUI depth recovery with its original enabled state.
            bool wasVisible = label.enabled;
            if (!wasVisible) return;
            label.enabled = false;
            label.enabled = true;
            label.SetAllDirty();
        }

        private void LateUpdate() => SynchronizeNow();

        /// <summary>Explicit native QA sync after temporary canvas resolution
        /// changes. Identical to the normal Unity LateUpdate rendering path.</summary>
        public void SynchronizeNow()
        {
            if (!UnityEngine.Application.isPlaying) return;
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
            LayoutHeaderAlongSafeTop();
            LayoutNavigation();
            AlignBuildingCartoucheToNavigation();
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
            // The original bottom dock already contributes its occupied
            // y-range in RoyalHUD.GetVisibleSceneViewport. Never reserve an
            // imaginary left rail that shifts the city/world away from centre.
            float width = Mathf.Max(1, safe.rect.width);
            float height = Mathf.Max(1, safe.rect.height);
            float padding = .025f;
            // Crónica Real's illustrated card is bottom-docked in BOTH
            // orientations. The former landscape right-sheet exclusion made
            // the free width negative on 4:3 displays, so automatic focus
            // returned without ever moving to the selected world resource.
            if (buildingVisible)
                free.yMin = Mathf.Max(free.yMin,
                    (buildingCard.anchoredPosition.y + buildingCard.rect.height) / height + padding);
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
                {
                    // The chapter occupies only the TOP-LEFT. Rejecting that
                    // entire horizontal strip even at the middle of the screen
                    // prevented genuine world POIs from being selected/framed.
                    // Exclude its top band instead; the edge-anchored bottom navigation is measured separately.
                    float fromTop = -objectiveCard.anchoredPosition.y +
                        objectiveCard.rect.height;
                    free.yMax = Mathf.Min(free.yMax, 1f - fromTop / height - padding);
                }
            }
            return free;
        }
    }
}
