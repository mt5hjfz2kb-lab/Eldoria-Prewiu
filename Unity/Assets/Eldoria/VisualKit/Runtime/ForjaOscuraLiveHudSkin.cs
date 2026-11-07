using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Eldoria.VisualKit
{
    /// <summary>
    /// Reversible, EDITOR-PREVIEW skin for the ONE EXISTING RoyalHUD.
    /// No Canvas, EventSystem, camera, SaveStore, gameplay Gateway or duplicate callbacks.
    /// The actual game's existing UI renders every value and executes every action.
    /// This class only changes decoration and color, never Button.interactable.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(20000)]
    public sealed class ForjaOscuraLiveHudSkin : MonoBehaviour
    {
        public bool NavigationSkinned { get; private set; }
        public bool ResourceSkinned { get; private set; }
        public bool HeaderSkinned { get; private set; }
        public bool ObjectiveSkinned { get; private set; }
        public bool BuildingSkinned { get; private set; }
        public bool ContextSkinned { get; private set; }
        public bool UsesExistingNavigation => hud && hud.Navigation;
        public bool SourceIsLive => hud && hud.isActiveAndEnabled;
        public bool UsesExistingBuildingAction => hud && hud.Building && hud.Building.Primary;

        private RoyalHUD hud;
        private RoyalGoldV2Hud v2;
        private RoyalGoldV3ContextHud context;
        private RoyalSection selected = RoyalSection.Bastion;
        private RectTransform lastContextRect;
        private struct OriginalButtonStyle { public Selectable.Transition Transition; public SpriteState SpriteState; public ColorBlock Colors; }
        private readonly Dictionary<Button, OriginalButtonStyle> originalButtonStyles = new Dictionary<Button, OriginalButtonStyle>();
        private float nextStyleRefresh;
        private readonly Dictionary<Behaviour, bool> previousEnabled = new Dictionary<Behaviour, bool>();
        private readonly Dictionary<Graphic, Color> previousColors = new Dictionary<Graphic, Color>();
        private readonly Dictionary<RoyalText, int> originalFontSizes = new Dictionary<RoyalText, int>();
        private readonly Dictionary<RectTransform, Vector2> originalCaptionSizes = new Dictionary<RectTransform, Vector2>();
        private readonly Dictionary<RectTransform, Vector2> originalNavigationSizes = new Dictionary<RectTransform, Vector2>();
        private readonly Dictionary<RectTransform, Vector2> originalNavigationPositions = new Dictionary<RectTransform, Vector2>();
        private readonly List<RectTransform> mobileNavigationButtons = new List<RectTransform>(5);
        private readonly Dictionary<RoyalButton, UnityAction> navigationListeners = new Dictionary<RoyalButton, UnityAction>();
        private readonly Dictionary<Button, Graphic> previousTargets = new Dictionary<Button, Graphic>();
        private readonly List<GameObject> created = new List<GameObject>();
        private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        private static readonly Color LightInk = new Color(.94f, .91f, .86f, 1f);
        private static readonly Color WarmGold = new Color(.98f, .75f, .42f, 1f);
        private static readonly Color MutedText = new Color(.78f, .79f, .81f, 1f);
        private static readonly Color Faded = new Color(.58f, .60f, .64f, 1f);

        private void Awake()
        {
            hud = GetComponent<RoyalHUD>();
            v2 = GetComponent<RoyalGoldV2Hud>();
            context = GetComponent<RoyalGoldV3ContextHud>();
            if (!hud)
            {
                Debug.LogWarning("[FORJA] Expected existing RoyalHUD component on this object. Skin disabled.");
                enabled = false;
            }
        }

        private void LateUpdate() => ApplyNow();

        /// <summary>Synchronize the SAME runtime presentation immediately before an
        /// offscreen native capture; no synthetic values or actions are created.</summary>
        public void ApplyNow(bool forceLayoutRefresh = false)
        {
            if (!hud || !hud.gameObject.activeInHierarchy) return;
            if (!v2) v2 = GetComponent<RoyalGoldV2Hud>();
            if (!context) context = GetComponent<RoyalGoldV3ContextHud>();
            if (!NavigationSkinned) TrySkinNavigation();
            if (!ResourceSkinned) TrySkinResources();
            if (!ObjectiveSkinned || !BuildingSkinned || !HeaderSkinned) TrySkinV2();
            if (!ContextSkinned || (context && context.ContextRect != lastContextRect))
                TrySkinV3Context();
            // V2 navigation layout can reset targetGraphic even after the
            // skin has been installed. Keep the illustrated real button
            // as a raycast/transition target in the same frame, not 50ms later.
            EnsureNavigationTargets();
            EnsureMobileNavigationTouchTargets();
            EnsureBuildingCartoucheClearsNavigation();
            EnsureMobileResourceIconReadability();
            ReflowLiveLabels();
            // Canonical RoyalGoldV2Hud.LayoutNavigation and header layout run
            // EVERY LateUpdate and reset caption sizes, colors and raycast
            // graphics. Restyle these eight small live controls EVERY frame:
            // a 50ms throttle caused rotating-frame flicker and tiny labels.
            // Keep the expensive deep panel hierarchy pass throttled.
            RefreshFastNavigationAndResources();
            // Avoid scanning full panel text hierarchy every frame.
            if (forceLayoutRefresh || Time.unscaledTime >= nextStyleRefresh)
            {
                nextStyleRefresh = Time.unscaledTime + .05f;
                RefreshExistingLabels();
            }
        }

        private Sprite Sprite(string group, string name)
        {
            // The retained original plate stays in the kit for provenance.
            // This authored raster keeps its corner metalwork inside fixed slices.
            if (group == "Surfaces" && name == "panel_dark") name = "panel_dark_painted";
            string key = "ForjaOscura/" + group + "/" + name;
            if (!spriteCache.TryGetValue(key, out var result))
            {
                result = Resources.Load<Sprite>(key);
                spriteCache.Add(key, result);
            }
            return result;
        }

        private static Transform ImmediateChild(Transform parent, string name)
        {
            if (!parent) return null;
            foreach (Transform t in parent)
                if (t.name == name) return t;
            return null;
        }

        private static void Fill(RectTransform rect, float inset = 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(.5f, .5f);
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        private Image ImageChild(Transform parent, string name, Sprite sprite, bool capturesPointer = false)
        {
            if (!parent || !sprite) return null;
            var existing = ImmediateChild(parent, name);
            if (existing)
            {
                var image = existing.GetComponent<Image>();
                if (image) return image;
            }
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            go.layer = parent.gameObject.layer;
            created.Add(go);
            var graphic = go.GetComponent<Image>();
            graphic.sprite = sprite;
            graphic.type = sprite.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
            graphic.raycastTarget = capturesPointer;
            Fill(go.GetComponent<RectTransform>());
            return graphic;
        }

        private void Hide(Behaviour graphic)
        {
            if (!graphic) return;
            if (!previousEnabled.ContainsKey(graphic)) previousEnabled.Add(graphic, graphic.enabled);
            graphic.enabled = false;
        }

        private void Tint(Graphic graphic, Color color)
        {
            if (!graphic) return;
            if (!previousColors.ContainsKey(graphic)) previousColors.Add(graphic, graphic.color);
            graphic.color = color;
        }

        // The native capture's CanvasScaler can reduce 24pt text below
        // usable mobile pixel sizes. Touch ONLY existing RoyalText presentation.
        // Preserve original font sizes for a faithful reversible A/B test.
        private void EnsureReadable(RoyalText label, float minimumPixels, bool navigationCaption = false)
        {
            if (!label || !label.isActiveAndEnabled) return;
            var canvas = hud.GetComponent<Canvas>();
            if (!canvas) return;
            var rect = label.rectTransform;
            if (!rect || rect.rect.width <= 8f || rect.rect.height <= 8f) return;
            float factor = Mathf.Max(.01f, canvas.scaleFactor);
            // Source-proven mobile geometry defect: the canonical V2 caption
            // is only 20 logical units high. On an 844x390 camera capture
            // scaleFactor=390/900, that leaves 8.7 physical pixels, so a
            // 12px label CANNOT fit even if its fontSize is increased.
            // Only stretch the existing caption, never its hit target or dock,
            // and restore the old rect on skin removal.
            if (navigationCaption)
            {
                float desiredHeight = (minimumPixels + 4f) / factor;
                if (rect.rect.height < desiredHeight)
                {
                    if (!originalCaptionSizes.ContainsKey(rect))
                        originalCaptionSizes.Add(rect, rect.sizeDelta);
                    rect.sizeDelta = new Vector2(rect.sizeDelta.x, desiredHeight);
                }
            }
            int needed = Mathf.CeilToInt(minimumPixels / factor);
            if (label.fontSize >= needed) return;
            int original = label.fontSize;
            if (!originalFontSizes.ContainsKey(label))
                originalFontSizes.Add(label, original);
            label.fontSize = needed;
            // Do not trade microscopic text for clipping. If it does not fit,
            // the native readability gate reports the unmet requirement.
            while (label.fontSize > original &&
                (label.preferredWidth > rect.rect.width - 4f ||
                 label.preferredHeight > rect.rect.height - 2f))
                label.fontSize--;
        }

        private void ReplaceButtonSurface(Button button, Sprite sprite, string elementName)
        {
            if (!button || !sprite) return;
            var skin = ImageChild(button.transform, elementName, sprite, true);
            if (!skin) return;
            skin.transform.SetAsFirstSibling();
            if (!previousTargets.ContainsKey(button))
            {
                previousTargets.Add(button, button.targetGraphic);
                // Keep original graphic enabled! V3 reads its depth to decide
                // whether to re-register the uGUI hierarchy after rotation.
                // Disabling it causes a repeated stale-depth rebind loop.
            }
            button.targetGraphic = skin;
            if (!originalButtonStyles.ContainsKey(button))
                originalButtonStyles.Add(button, new OriginalButtonStyle {
                    Transition = button.transition,
                    SpriteState = button.spriteState,
                    Colors = button.colors
                });
            bool primary = elementName.Contains("action");
            string prefix = primary ? "button_primary_" : "button_secondary_";
            var states = new SpriteState {
                highlightedSprite = Sprite("Surfaces", prefix + "hover"),
                pressedSprite = Sprite("Surfaces", primary ? "button_primary_pressed" : "button_secondary_hover"),
                selectedSprite = Sprite("Surfaces", prefix + "normal"),
                disabledSprite = Sprite("Surfaces", prefix + "disabled")
            };
            // All referenced states are existing, imported PNGs. No new input
            // handler: the original RoyalButton and its onClick delegates survive.
            if (states.highlightedSprite && states.pressedSprite && states.disabledSprite)
            {
                button.transition = Selectable.Transition.SpriteSwap;
                button.spriteState = states;
            }
            // Button continues to receive pointer events through its own image child.
            // Existing onClick handlers remain unchanged and are never copied.
        }

        private void TrySkinNavigation()
        {
            if (!hud.Navigation || hud.Navigation.Items == null) return;
            int decorated = 0;
            foreach (var item in hud.Navigation.Items)
            {
                if (item == null || !item.Button) continue;
                var name = NavName(item.Section);
                if (string.IsNullOrEmpty(name)) continue; // Do not expose locked/non-canonical destinations.
                var sprite = Sprite("Icons", name);
                if (!sprite) continue;

                // The V2 layout ASSUMES Button.child[0] = medallion and
                // medallion.child[0] = glyph. Never insert at either level.
                var button = item.Button;
                if (button.transform.childCount == 0) continue;
                var medallion = button.transform.GetChild(0) as RectTransform;
                if (!medallion || medallion.childCount == 0) continue;
                var glyph = medallion.GetChild(0) as RectTransform;
                if (!glyph || !glyph.GetComponent<RoyalIconGraphic>()) continue;
                var background = ImageChild(glyph, "Forja / navigation plate",
                    Sprite("Surfaces", "nav_tile_normal"), true);
                var icon = ImageChild(glyph, "Forja / navigation icon", sprite);
                if (background) background.transform.SetAsFirstSibling();
                if (icon) icon.transform.SetAsLastSibling();
                if (background)
                {
                    // V2 normally points Button.targetGraphic at its old
                    // icon. Hiding that icon without redirecting the
                    // target leaves the genuine button with a disabled
                    // transition/raycast graphic. Keep the original
                    // Button and every onClick delegate, but make the
                    // new illustrated hit surface its active target.
                    if (!previousTargets.ContainsKey(button))
                        previousTargets.Add(button, button.targetGraphic);
                    button.targetGraphic = background;
                    background.raycastTarget = true;
                }
                if (icon) icon.raycastTarget = false;
                Hide(glyph.GetComponent<RoyalIconGraphic>());

                if (!navigationListeners.ContainsKey(button))
                {
                    RoyalSection choice = item.Section;
                    UnityAction listener = () => selected = choice;
                    button.onClick.AddListener(listener);
                    navigationListeners.Add(button, listener);
                }
                decorated++;
            }
            // The approved direction needs one coherent premium dock, not
            // only five isolated icons. Draw a reusable nine-sliced backing
            // behind every ORIGINAL navigation action and label.
            var dock = hud.Navigation.transform as RectTransform;
            var surface = dock ? ImageChild(dock, "Forja / continuous bottom dock",
                Sprite("Surfaces", "panel_dark")) : null;
            if (surface)
            {
                surface.raycastTarget = false;
                surface.transform.SetAsFirstSibling();
                var oldTrim = ImmediateChild(dock, "V3 / Chronicle bottom frame");
                if (oldTrim) HideGraphicsByTypeName(oldTrim, "RoyalGoldV2FrameGraphic");
            }
            NavigationSkinned = decorated == 5 && surface;
            if (!NavigationSkinned) return;
            Debug.Log("[FORJA] Five live navigation buttons and continuous dock skinned, original actions retained.");
        }

        private void TrySkinResources()
        {
            // Original prefab owns the numeric labels and their resource
            // glyph RectTransforms. Only replace the icon drawings; never
            // create synthetic resource values or extra data bindings.
            if (!hud.Resources) return;
            var parent = hud.Resources.transform;
            string[] sourceNames = { "Madera icon", "Piedra icon", "Comida icon" };
            string[] spriteNames = { "res_madera", "res_piedra", "res_comida" };
            int done = 0;
            for (int i = 0; i < sourceNames.Length; i++)
            {
                var target = ImmediateChild(parent, sourceNames[i]);
                if (!target) continue;
                var newIcon = ImageChild(target, "Forja / live " + spriteNames[i], Sprite("Icons", spriteNames[i]));
                if (!newIcon) continue;
                newIcon.raycastTarget = false;
                newIcon.transform.SetAsLastSibling();
                // The original 256px icon sheet leaves safe alpha margins.
                // A 14-25 logical-unit resource slot otherwise turns the
                // illustrated wood/stone/food into a near-invisible speck.
                // Grow only this non-raycasting child by 3 units per side;
                // keep the real value and its layout unchanged.
                var cropRect = (RectTransform)newIcon.transform;
                cropRect.offsetMin = new Vector2(-3f, -3f);
                cropRect.offsetMax = new Vector2(3f, 3f);
                // LayoutOriginalResource continues to size/position the
                // existing outer icon. The new child fills its rectangle.
                var original = target.GetComponent<RoyalIconGraphic>();
                if (original) Hide(original);
                done++;
            }
            ResourceSkinned = done == sourceNames.Length;
        }

        /// <summary>
        /// The canonical header's real resource icons are only 25 logical
        /// units in portrait and 36 in landscape. At native 390/800 or
        /// 390/900 Canvas scale that is 12px/16px, too small to recognize
        /// wood/stone/food illustrations. Grow the same LIVE resource slots
        /// only when physically tiny. In portrait, where numeric chips have
        /// spare horizontal room, shift the existing value text so no icon
        /// can cover a genuine number. In landscape expand only the new,
        /// non-raycasting illustrated child mostly towards the free left edge.
        /// </summary>
        private void EnsureMobileResourceIconReadability()
        {
            if (!ResourceSkinned || !hud || !hud.Resources) return;
            var canvas = hud.GetComponent<Canvas>();
            var canvasRect = canvas ? canvas.transform as RectTransform : null;
            if (!canvas || !canvasRect || canvas.scaleFactor <= .01f) return;
            bool portrait = canvasRect.rect.height > canvasRect.rect.width;
            string[] labels = { "Madera", "Piedra", "Comida" };
            string[] sprites = { "res_madera", "res_piedra", "res_comida" };
            RoyalText[] values = { hud.Resources.Wood, hud.Resources.Stone, hud.Resources.Food };
            for (int i = 0; i < labels.Length; i++)
            {
                if (!values[i] || !values[i].gameObject.activeInHierarchy) continue;
                var source = ImmediateChild(hud.Resources.transform, labels[i] + " icon") as RectTransform;
                var icon = source ? ImmediateChild(source, "Forja / live " + sprites[i]) as RectTransform : null;
                if (!source || !icon) continue;
                float physicalWidth = source.rect.width * canvas.scaleFactor;
                if (physicalWidth >= 24f)
                {
                    icon.offsetMin = new Vector2(-3f, -3f);
                    icon.offsetMax = new Vector2(3f, 3f);
                    continue;
                }
                if (portrait)
                {
                    float required = Mathf.Min(50f, Mathf.CeilToInt(24f / canvas.scaleFactor));
                    if (required <= source.rect.width) continue;
                    float increase = required - source.rect.width;
                    var value = values[i].rectTransform;
                    // Presentation can NEVER throw on an otherwise playable
                    // 390px phone because a resource label is short. Retain at
                    // least 48 logical units for the real numeric value. The
                    // native icon-size QA fails closed if this compromise
                    // cannot show a legible illustrated resource icon.
                    increase = Mathf.Min(increase,
                        Mathf.Max(0f, value.rect.width - 48f));
                    if (increase < 1f) continue;
                    required = source.rect.width + increase;
                    RememberNavigationGeometry(source);
                    RememberNavigationGeometry(value);
                    source.sizeDelta = new Vector2(required, required);
                    source.anchoredPosition = new Vector2(source.anchoredPosition.x,
                        -(60f - required) * .5f);
                    value.anchoredPosition = new Vector2(
                        value.anchoredPosition.x + increase, value.anchoredPosition.y);
                    value.sizeDelta = new Vector2(value.sizeDelta.x - increase,
                        value.sizeDelta.y);
                    icon.offsetMin = new Vector2(-3f, -3f);
                    icon.offsetMax = new Vector2(3f, 3f);
                }
                else
                {
                    // Existing values retain their exact geometry; all extra
                    // sprite area is decorative and cannot intercept taps.
                    icon.offsetMin = new Vector2(-19f, -10f);
                    icon.offsetMax = new Vector2(2f, 10f);
                }
            }
        }

        private void EnsureNavigationTargets()
        {
            if (!NavigationSkinned || hud.Navigation == null || hud.Navigation.Items == null) return;
            foreach (var item in hud.Navigation.Items)
            {
                if (item == null || !item.Button || item.Button.transform.childCount < 1) continue;
                var medallion = item.Button.transform.GetChild(0);
                if (medallion.childCount < 1) continue;
                var glyph = medallion.GetChild(0);
                var plate = ImmediateChild(glyph, "Forja / navigation plate");
                var image = plate ? plate.GetComponent<Image>() : null;
                if (image && item.Button.targetGraphic != image)
                    item.Button.targetGraphic = image;
            }
        }

        private void RememberNavigationGeometry(RectTransform rect)
        {
            if (!rect) return;
            if (!originalNavigationSizes.ContainsKey(rect))
                originalNavigationSizes.Add(rect, rect.sizeDelta);
            if (!originalNavigationPositions.ContainsKey(rect))
                originalNavigationPositions.Add(rect, rect.anchoredPosition);
        }

        /// <summary>
        /// On the real 390px portrait, the native Canvas uses scale 390/800.
        /// A five-column V2 dock can thus have logically wide buttons but only
        /// about 66 physical pixels for the word CONSTRUIR/CRÓNICAS. This is
        /// too narrow for >=12px text. Only when the measured physical width is
        /// under 100px, reflow the ORIGINAL five controls into 3+2 rows,
        /// preserving their live click handlers and a 44.5px touch height.
        /// The existing V2 renderer still owns all other device layouts.
        /// </summary>
        private void EnsureMobileNavigationTouchTargets()
        {
            if (!NavigationSkinned || !hud || !hud.Navigation) return;
            var canvas = hud.GetComponent<Canvas>();
            var dock = hud.Navigation.transform as RectTransform;
            var canvasRect = canvas ? canvas.transform as RectTransform : null;
            if (!canvas || !dock || !canvasRect || canvas.scaleFactor <= .01f ||
                canvasRect.rect.height <= canvasRect.rect.width) return;
            mobileNavigationButtons.Clear();
            foreach (var item in hud.Navigation.Items)
            {
                if (item != null && item.Button && item.Button.gameObject.activeInHierarchy)
                {
                    var buttonRect = item.Button.transform as RectTransform;
                    if (buttonRect) mobileNavigationButtons.Add(buttonRect);
                }
            }
            if (mobileNavigationButtons.Count != 5) return;
            float minY = mobileNavigationButtons[0].anchoredPosition.y;
            float maxY = minY;
            foreach (var rect in mobileNavigationButtons)
            {
                minY = Mathf.Min(minY, rect.anchoredPosition.y);
                maxY = Mathf.Max(maxY, rect.anchoredPosition.y);
            }
            bool twoRows = maxY - minY > 3f;
            float requiredHeight = Mathf.CeilToInt(44.5f / canvas.scaleFactor);
            bool compactFiveColumns = !twoRows &&
                mobileNavigationButtons[0].rect.width * canvas.scaleFactor < 100f;
            if (!compactFiveColumns && !twoRows) return;

            const float columnGap = 4f;
            const float rowGap = 3f;
            float originalHeight = mobileNavigationButtons[0].rect.height;
            float buttonHeight = Mathf.Max(74f, requiredHeight);
            // Source already chose two rows: only extend when its 74-logical
            // unit buttons are too short on a low-density offscreen capture.
            if (twoRows && originalHeight >= requiredHeight) return;
            float cellWidth;
            if (compactFiveColumns)
            {
                float contentWidth = Mathf.Max(150f, dock.rect.width - 12f);
                cellWidth = Mathf.Max(60f, (contentWidth - 2f * columnGap) / 3f);
            }
            else
                cellWidth = mobileNavigationButtons[0].rect.width;

            float contentHeight = buttonHeight * 2f + rowGap;
            RememberNavigationGeometry(dock);
            dock.sizeDelta = new Vector2(dock.sizeDelta.x, contentHeight + 6f);
            int index = 0;
            for (int order = 0; order < 5; order++)
            foreach (var item in hud.Navigation.Items)
            {
                if (item == null || BottomNavigation.ChronicleOrder(item.Section) != order)
                    continue;
                if (item == null || !item.Button || !item.Button.gameObject.activeInHierarchy)
                    continue;
                int row = index / 3, column = index % 3;
                int inRow = row == 0 ? 3 : 2;
                float rowWidth = cellWidth * inRow + columnGap * (inRow - 1);
                var rect = item.Button.transform as RectTransform;
                RememberNavigationGeometry(rect);
                rect.sizeDelta = new Vector2(cellWidth, buttonHeight);
                rect.anchoredPosition = new Vector2(
                    -rowWidth * .5f + cellWidth * .5f + column * (cellWidth + columnGap),
                    contentHeight * .5f - buttonHeight * .5f -
                    row * (buttonHeight + rowGap));
                // Keep illustrated icons as actual glyphs, not tiny dots.
                // Each change is reversible, including the real caption.
                if (rect.childCount > 0 && rect.GetChild(0) is RectTransform medal)
                {
                    RememberNavigationGeometry(medal);
                    // 60x60 fits the reflowed 92-unit touch cell without
                    // covering the caption (both remain in the real cell).
                    medal.sizeDelta = new Vector2(60f, 60f);
                    medal.anchoredPosition = new Vector2(0f, 19f);
                    if (medal.childCount > 0 && medal.GetChild(0) is RectTransform glyph)
                    {
                        RememberNavigationGeometry(glyph);
                        glyph.sizeDelta = new Vector2(48f, 48f);
                    }
                }
                if (item.Label)
                {
                    var caption = item.Label.rectTransform;
                    RememberNavigationGeometry(caption);
                    caption.sizeDelta = new Vector2(cellWidth - 4f,
                        Mathf.Max(caption.rect.height, (12f + 4f) / canvas.scaleFactor));
                    caption.anchoredPosition = new Vector2(0f, -buttonHeight * .32f);
                }
                index++;
            }
        }

        /// <summary>
        /// RoyalGoldV2Hud.SynchronizeNow aligns the REAL building cartouche
        /// to its original navigation dock before this opt-in skin expands
        /// that dock to two readable portrait rows. Without this adjustment
        /// the card's lower edge would cover the upper row of touch controls.
        /// Move only the existing bottom-anchored card; never create a modal
        /// or invoke a building command. The original position is restored.
        /// </summary>
        private void EnsureBuildingCartoucheClearsNavigation()
        {
            if (!NavigationSkinned || !v2 || !v2.ChronicleBuildingRect ||
                !hud || !hud.Navigation) return;
            var dock = hud.Navigation.transform as RectTransform;
            if (!dock) return;
            // This is exclusively the mobile two-row variant. Do not alter
            // established landscape and wide-portrait placements.
            float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
            int active = 0;
            foreach (var item in hud.Navigation.Items)
            {
                if (item == null || !item.Button ||
                    !item.Button.gameObject.activeInHierarchy) continue;
                var rect = item.Button.transform as RectTransform;
                if (!rect) continue;
                active++;
                minY = Mathf.Min(minY, rect.anchoredPosition.y);
                maxY = Mathf.Max(maxY, rect.anchoredPosition.y);
            }
            if (active != 5 || maxY - minY < 3f) return;
            var card = v2.ChronicleBuildingRect;
            // The V2 cartouche uses the SAME bottom anchor as this dock.
            // Its y is the bottom edge, not the center or top.
            if (card.anchorMin.y > .01f || card.anchorMax.y > .01f) return;
            float requiredBottom = Mathf.Max(8f,
                dock.anchoredPosition.y + dock.rect.height + 8f);
            if (card.anchoredPosition.y >= requiredBottom - .5f) return;
            RememberNavigationGeometry(card);
            card.anchoredPosition = new Vector2(card.anchoredPosition.x,
                requiredBottom);
        }

        // Correct the actual text slots, not a screenshot overlay. V2 owns the
        // same top-left anchors; every size/position/font is restored on removal.
        private void SetTextSlot(Transform parent, string name, float x, float y,
            float width, float height, float minPixels)
        {
            var child = ImmediateChild(parent, name) as RectTransform;
            if (!child) return;
            RememberNavigationGeometry(child);
            child.anchoredPosition = new Vector2(x, -y);
            child.sizeDelta = new Vector2(width, height);
            var label = child.GetComponent<RoyalText>();
            if (label) EnsureReadable(label, minPixels);
        }

        private void ReflowLiveLabels()
        {
            if (!v2) return;
            var canvas = GetComponent<Canvas>();
            float scale = canvas && canvas.scaleFactor > .01f ? canvas.scaleFactor : 1f;
            if (ObjectiveSkinned && v2.ChronicleObjectiveRect)
            {
                var card = v2.ChronicleObjectiveRect;
                float pad = 16f;
                SetTextSlot(card, "Chapter", pad, 9f, card.rect.width-2f*pad, 27f, 13f);
                SetTextSlot(card, "Current real objective", pad, 38f,
                    card.rect.width-2f*pad, 48f, 12f);
                SetTextSlot(card, "Objective progress", card.rect.width-94f,
                    83f, 78f, 25f, 11f);
                var crest = ImmediateChild(card, "V3 / original Valoria heraldic sigil");
                if (crest)
                    foreach (var g in crest.GetComponentsInChildren<Graphic>(true)) Hide(g);
                HideGraphicsByTypeName(card, "RoyalGoldV3HeraldicBannerGraphic");
                var banner = card.GetComponentsInChildren<Graphic>(true);
                foreach (var g in banner)
                    if (g && g.transform.parent &&
                        g.transform.parent.name == "V3 / genuine Valoria castle insignia") Hide(g);
            }
            if (!BuildingSkinned || !v2.ChronicleBuildingRect) return;
            var building = v2.ChronicleBuildingRect;
            var action = ImmediateChild(building, "Execute existing authorised offer") as RectTransform;
            if (!action) return;
            // Landscape previously clipped 'Mejorar a Bastión II' after 'a'.
            // Widen the existing CTA into unused lower-right card space.
            var parent = building.parent as RectTransform;
            bool portrait = parent && parent.rect.height > parent.rect.width;
            if (!portrait)
            {
                RememberNavigationGeometry(action);
                float w = Mathf.Min(building.rect.width*.38f, Mathf.Max(290f, 220f/scale));
                action.sizeDelta = new Vector2(w, Mathf.Max(51f, 44f/scale));
                action.anchoredPosition = new Vector2(building.rect.width-w-14f,
                    -(building.rect.height-action.rect.height-14f));
            }
            foreach (var label in action.GetComponentsInChildren<RoyalText>(true))
                EnsureReadable(label, 12f);
        }

        private static string NavName(RoyalSection section)
        {
            switch (section)
            {
                case RoyalSection.Bastion: return "nav_reino";
                case RoyalSection.Inventory: return "nav_construir";
                case RoyalSection.Army: return "nav_unidades";
                case RoyalSection.World: return "nav_mundo";
                case RoyalSection.Missions: return "nav_cronicas";
                default: return null;
            }
        }

        private void TrySkinV2()
        {
            if (!v2) return;
            if (!ObjectiveSkinned && v2.ChronicleObjectiveRect)
            {
                var obj = v2.ChronicleObjectiveRect;
                var sprite = Sprite("Surfaces", "panel_quest");
                var image = ImageChild(obj, "Forja / live objective surface", sprite);
                if (image)
                {
                    image.transform.SetAsFirstSibling();
                    // Suppress ONLY the specific source parchment/background; do not
                    // change event handlers or original objective text objects.
                    HideGraphicsByTypeName(obj, "RoyalGoldV3ObjectiveParchmentGraphic");
                    ObjectiveSkinned = true;
                    DecorateMedallion(obj, "Go to existing objective", "ui_exploradores");
                    DecorateMedallion(obj, "Open current chapter", "ui_misiones");
                }
            }
            if (!BuildingSkinned && v2.ChronicleBuildingRect)
            {
                var card = v2.ChronicleBuildingRect;
                var image = ImageChild(card, "Forja / live building surface", Sprite("Surfaces", "panel_dark"));
                if (image)
                {
                    image.transform.SetAsFirstSibling();
                    HideGraphicsByTypeName(card, "RoyalGoldV3BuildingCartoucheFinish");
                    var oldRim = ImmediateChild(card, "Building / real nine-slice outline");
                    if (oldRim)
                        foreach (var g in oldRim.GetComponentsInChildren<Graphic>(true)) Hide(g);
                    // The real building miniature remains untouched: it contains
                    // the original Unity model/RenderTexture owned by the game.
                    var primary = ImmediateChild(card, "Execute existing authorised offer");
                    if (primary)
                        ReplaceButtonSurface(primary.GetComponent<RoyalButton>(),
                            Sprite("Surfaces", "button_primary_normal"), "Forja / live action surface");
                    BuildingSkinned = true;
                }
            }
            if (!HeaderSkinned)
            {
                var safe = v2.transform.GetComponentInChildren<RoyalHudLayout>(true);
                var top = safe ? ImmediateChild(safe.transform, "V3 / continuous original royal header") : null;
                if (top)
                {
                    var image = ImageChild(top, "Forja / existing header skin", Sprite("Surfaces", "panel_dark"));
                    if (image)
                    {
                        // Never draw a full-frame opaque panel ABOVE the
                        // resource text, crest or click targets. The original
                        // V2 header itself is already the earliest safe-edge
                        // sibling; the replacement is its rear-most child.
                        image.transform.SetAsFirstSibling();
                        HideGraphicsByTypeName(top, "RoyalGoldV2FrameGraphic");
                        HeaderSkinned = true;
                    }
                }
            }
        }

        private void TrySkinV3Context()
        {
            // In the actual canonical city the V3 contextual cartouche is
            // often visible while the older V2 building panel stays hidden.
            // Target the public V3 contract, never a guessed hierarchy.
            if (!context || !context.IsConfigured || !context.ContextRect) return;
            var card = context.ContextRect;
            if (v2 && card == v2.ChronicleBuildingRect)
            {
                ContextSkinned = BuildingSkinned;
                lastContextRect = ContextSkinned ? card : null;
                return;
            }
            var bg = ImageChild(card, "Forja / V3 canonical contextual card",
                Sprite("Surfaces", "panel_dark"));
            if (!bg) return;
            bg.transform.SetAsFirstSibling();
            // Keep the card's own RoyalGoldV2FrameGraphic enabled as the
            // original backdrop raycast blocker; overlay it visually only.
            // The real button still invokes the authoritative V1 offer.
            ReplaceButtonSurface(context.VisibleActionButton,
                Sprite("Surfaces", "button_primary_normal"), "Forja / V3 real action");
            ReplaceButtonSurface(context.VisibleCloseButton,
                Sprite("Surfaces", "button_secondary_normal"), "Forja / V3 real close");
            ContextSkinned = true;
            lastContextRect = card;
            Debug.Log("[FORJA] Actual V3 context card/offer skinned: retained real action, selection and close.");
        }

        private void DecorateMedallion(RectTransform panel, string buttonName, string iconName)
        {
            var target = ImmediateChild(panel, buttonName);
            var button = target ? target.GetComponent<RoyalButton>() : null;
            if (!button) return;
            var medal = target.childCount > 0 ? target.GetChild(0) : target;
            var ring = ImageChild(medal, "Forja / medallion surface", Sprite("Surfaces", "medallion_normal"), true);
            if (ring) ring.transform.SetAsFirstSibling();
            var icon = ImageChild(medal, "Forja / medallion symbol", Sprite("Icons", iconName));
            if (icon)
            {
                var rt = (RectTransform)icon.transform;
                rt.anchorMin = rt.anchorMax = new Vector2(.5f, .5f);
                rt.sizeDelta = new Vector2(34f, 34f);
                rt.anchoredPosition = Vector2.zero;
                icon.transform.SetAsLastSibling();
            }
            // The ring remains enabled for its original Canvas depth/raycast;
            // only the legacy pictogram is hidden (the new illustrated icon
            // replaces it, without affecting Button or Graphic registration).
            HideGraphicsByTypeName(medal, "RoyalIconGraphic");
            if (ring && !previousTargets.ContainsKey(button))
            {
                previousTargets.Add(button, button.targetGraphic);
                button.targetGraphic = ring;
            }
        }

        private void HideGraphicsByTypeName(Transform root, string typeName)
        {
            if (!root) return;
            foreach (var g in root.GetComponentsInChildren<Graphic>(true))
                if (g && g.GetType().Name == typeName) Hide(g);
        }

        private void RefreshFastNavigationAndResources()
        {
            if (hud.Navigation && NavigationSkinned)
            {
                foreach (var item in hud.Navigation.Items)
                {
                    if (item == null || !item.Button) continue;
                    var chosen = item.Section == selected;
                    if (item.Label)
                    {
                        EnsureReadable(item.Label, 12f, true);
                        Tint(item.Label, item.Button.interactable ? (chosen ? WarmGold : LightInk) : Faded);
                    }
                    if (item.Button.transform.childCount == 0) continue;
                    var medal = item.Button.transform.GetChild(0);
                    if (medal.childCount == 0) continue;
                    var glyph = medal.GetChild(0) as RectTransform;
                    if (!glyph) continue;
                    var back = ImmediateChild(glyph, "Forja / navigation plate");
                    if (back)
                    {
                        var graphic = back.GetComponent<Image>();
                        if (graphic)
                        {
                            // RoyalGoldV2Hud.LayoutNavigation may rebind the
                            // old RoyalIconGraphic during orientation changes.
                            // Restore our actual live target AFTER V2 layout
                            // (this skin has a later execution order).
                            if (item.Button.targetGraphic != graphic)
                                item.Button.targetGraphic = graphic;
                            var target = Sprite("Surfaces", chosen ? "nav_tile_selected" : "nav_tile_normal");
                            if (target && graphic.sprite != target) graphic.sprite = target;
                            graphic.color = item.Button.interactable ? Color.white : new Color(.55f,.55f,.55f,.85f);
                            var r = (RectTransform)back;
                            r.anchorMin = r.anchorMax = new Vector2(.5f,.5f);
                            // Center the full-size touch surface on the real
                            // button, not on its elevated illustration medallion.
                            r.anchoredPosition = new Vector2(0f,
                                -((RectTransform)medal).anchoredPosition.y);
                            var b = (RectTransform)item.Button.transform;
                            // The illustrated raycast graphic must cover the
                            // actual 44.5px target, not just the 36px medallion.
                            // Keep it inside the resized live button rectangle.
                            r.sizeDelta = new Vector2(Mathf.Max(54f, b.rect.width - 2f),
                                Mathf.Max(58f, b.rect.height));
                        }
                    }
                    var ic = ImmediateChild(glyph, "Forja / navigation icon");
                    if (ic)
                    {
                        var rect=(RectTransform)ic;
                        Fill(rect);
                        rect.offsetMin = new Vector2(-2f,-2f);
                        rect.offsetMax = new Vector2(2f,2f);
                    }
                }
            }
            if (hud.Resources)
            {
                EnsureReadable(hud.Resources.Wood, 13f);
                Tint(hud.Resources.Wood, LightInk);
                EnsureReadable(hud.Resources.Stone, 13f);
                Tint(hud.Resources.Stone, LightInk);
                if (hud.Resources.Food)
                {
                    EnsureReadable(hud.Resources.Food, 13f);
                    Tint(hud.Resources.Food, LightInk);
                }
            }
        }

        private void RefreshExistingLabels()
        {
            if (ContextSkinned && context && context.ContextRect &&
                (!v2 || context.ContextRect != v2.ChronicleBuildingRect))
            {
                var active = context.ContextRect;
                foreach (var text in active.GetComponentsInChildren<RoyalText>(true))
                    Tint(text, LightInk);
                ColorizeDirect(active, "Building name", WarmGold);
                ColorizeDirect(active, "Authoritative building status", MutedText);
                ColorizeDirect(active, "Authoritative action cost", LightInk);
                ColorizeDirect(active, "Authoritative prerequisites", WarmGold);
            }
            if (!v2) return;
            if (ObjectiveSkinned)
            {
                var card = v2.ChronicleObjectiveRect;
                var chapter = ImmediateChild(card, "Chapter");
                if (chapter) EnsureReadable(chapter.GetComponent<RoyalText>(), 13f);
                var objective = ImmediateChild(card, "Current real objective");
                if (objective) EnsureReadable(objective.GetComponent<RoyalText>(), 12f);
                ColorizeDirect(card, "Chapter", WarmGold);
                ColorizeDirect(card, "Current real objective", LightInk);
                ColorizeDirect(card, "Objective progress", LightInk);
            }
            if (BuildingSkinned)
            {
                var card = v2.ChronicleBuildingRect;
                // Dark metal replaces the former pale parchment: every real
                // interactive label (close, acceleration, council, details)
                // must stay readable even when not explicitly named below.
                foreach (var label in card.GetComponentsInChildren<RoyalText>(true))
                    Tint(label, LightInk);
                ColorizeDirect(card, "Selected building", LightInk);
                ColorizeDirect(card, "Real level", WarmGold);
                ColorizeDirect(card, "Building / genuine offer cost", LightInk);
                ColorizeDirect(card, "Building / live state or countdown", WarmGold);
                var scroll = ImmediateChild(card, "Scrollable real building details");
                if (scroll)
                {
                    foreach (var text in scroll.GetComponentsInChildren<RoyalText>(true)) Tint(text, MutedText);
                }
                var primary=ImmediateChild(card, "Execute existing authorised offer");
                if (primary)
                    foreach (var text in primary.GetComponentsInChildren<RoyalText>(true)) Tint(text, LightInk);
            }
        }

        private void ColorizeDirect(Transform card, string name, Color color)
        {
            var child = ImmediateChild(card, name);
            if (child)
            {
                var graphic = child.GetComponent<Graphic>();
                if (graphic) Tint(graphic, color);
            }
        }

        private void OnDestroy()
        {
            foreach (var pair in navigationListeners)
                if (pair.Key) pair.Key.onClick.RemoveListener(pair.Value);
            foreach (var pair in originalButtonStyles)
            {
                if (!pair.Key) continue;
                pair.Key.transition = pair.Value.Transition;
                pair.Key.spriteState = pair.Value.SpriteState;
                pair.Key.colors = pair.Value.Colors;
            }
            foreach (var pair in previousTargets)
                if (pair.Key) pair.Key.targetGraphic = pair.Value;
            foreach (var pair in previousEnabled)
                if (pair.Key) pair.Key.enabled = pair.Value;
            foreach (var pair in previousColors)
                if (pair.Key) pair.Key.color = pair.Value;
            foreach (var pair in originalFontSizes)
                if (pair.Key) pair.Key.fontSize = pair.Value;
            foreach (var pair in originalCaptionSizes)
                if (pair.Key) pair.Key.sizeDelta = pair.Value;
            foreach (var pair in originalNavigationSizes)
                if (pair.Key) pair.Key.sizeDelta = pair.Value;
            foreach (var pair in originalNavigationPositions)
                if (pair.Key) pair.Key.anchoredPosition = pair.Value;
            // The native A/B proof restores the skin and captures another frame
            // without advancing Unity's game loop. Destroy() would leave the
            // transient Image children alive through that same frame, causing
            // stale raycasts and a false "restored" result in the Editor.
            // Only objects CREATED by this skin are removed synchronously.
            foreach (var go in created)
            {
                if (!go) continue;
#if UNITY_EDITOR
                DestroyImmediate(go);
#else
                Destroy(go);
#endif
            }
            created.Clear();
        }
    }
}
