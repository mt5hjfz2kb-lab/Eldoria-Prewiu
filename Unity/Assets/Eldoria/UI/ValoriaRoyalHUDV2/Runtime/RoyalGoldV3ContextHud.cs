using System;
using System.Collections.Generic;
using Eldoria.UI.RoyalGold;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.UI.RoyalGold.V2
{
    /// <summary>
    /// A single, scene-anchored presentation over the canonical RoyalHUD.
    /// All offers, costs, requirements, construction deadlines and progress
    /// originate in the existing IRoyalSession. No additional Canvas, gateway,
    /// EventSystem, save, clock or in-game timer is created.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(500)]
    public sealed class RoyalGoldV3ContextHud : MonoBehaviour
    {
        public RoyalHUD Source;

        // Supplied by the canonical scene host after it creates the one HUD.
        // The UI assembly cannot own or modify the scene/camera/target logic.
        public Func<string, Transform> FindAnchor;
        public Func<Camera> ActiveCamera;
        public Func<string, RoyalBuilding> ReadBuilding;

        private RectTransform safe, card, cardButton;
        private RoyalText title, status, costs, requirements, buttonLabel;
        private RoyalButton actionButton;
        private CanvasGroup cardGroup;
        private readonly Dictionary<string, TimerView> timers = new Dictionary<string, TimerView>();
        private readonly Dictionary<string, RoyalBuilding> live = new Dictionary<string, RoyalBuilding>();
        private float nextRead;
        private bool ready;
        private static readonly string[] TimedTargets =
        {
            "sawmill", "barracks", "bastion", "forest-valoria",
            "quarry-valoria", "corrupt-scout", "engendro-valoria"
        };
        private static readonly Color Ivory = new Color(.95f, .925f, .865f);
        private static readonly Color Gold = new Color(.89f, .73f, .46f);
        private static readonly Color Muted = new Color(.78f, .82f, .84f);
        private static readonly Color Amber = new Color(.89f, .66f, .33f);
        private sealed class TimerView
        {
            public RectTransform Root, Progress;
            public RoyalText Label;
            public CanvasGroup Visibility;
            public Image Fill;
        }

        public bool ContextVisible => cardGroup && cardGroup.blocksRaycasts;
        public RectTransform ContextRect => card;
        public bool IsConfigured => FindAnchor != null && ActiveCamera != null && ReadBuilding != null;
        public int ActiveTimers
        {
            get
            {
                int count = 0;
                foreach (var t in timers.Values)
                    if (t.Visibility && t.Visibility.alpha > .5f) count++;
                return count;
            }
        }

        private void Awake() { if (!Source) Source = GetComponent<RoyalHUD>(); }
        private void OnDisable()
        {
            Visible(cardGroup, false, true);
            foreach (var t in timers.Values) Visible(t.Visibility, false, false);
        }
        private static RectTransform Rect(string name, Transform parent)
        {
            var g = new GameObject(name, typeof(RectTransform));
            g.transform.SetParent(parent, false);
            g.layer = parent.gameObject.layer;
            return (RectTransform)g.transform;
        }
        private static RoyalText Text(string name, RectTransform parent, int font, Color color, bool bold = false)
        {
            var r = Rect(name, parent);
            r.gameObject.AddComponent<CanvasRenderer>();
            var t = r.gameObject.AddComponent<RoyalText>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = font;
            t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            t.color = color;
            t.raycastTarget = false;
            t.alignment = TextAnchor.UpperLeft;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }
        private static void Place(RectTransform r, float x, float y, float w, float h)
        {
            r.anchorMin = r.anchorMax = r.pivot = new Vector2(0f, 1f);
            r.anchoredPosition = new Vector2(x, -y);
            r.sizeDelta = new Vector2(w, h);
        }
        private static void Visible(CanvasGroup g, bool visible, bool receivesPointer)
        {
            if (!g) return;
            g.alpha = visible ? 1f : 0f;
            g.interactable = visible && receivesPointer;
            g.blocksRaycasts = visible && receivesPointer;
        }
        private static RectTransform Frame(string name, RectTransform parent, bool raycast)
        {
            var r = Rect(name, parent);
            r.gameObject.AddComponent<CanvasRenderer>();
            var graphic = r.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
            graphic.DecorativeCorners = false;
            graphic.Primary = false;
            graphic.color = Color.white;
            graphic.raycastTarget = raycast;
            return r;
        }
        private void EnsureView()
        {
            if (ready || !Source) return;
            var anchor = Source.GetComponentInChildren<RoyalHudLayout>(true);
            safe = anchor ? anchor.transform as RectTransform : null;
            if (!safe) return;

            card = Frame("V3 / selected building contextual action", safe, true);
            cardGroup = card.gameObject.AddComponent<CanvasGroup>();
            title = Text("Building name", card, 20, Gold, true);
            status = Text("Authoritative building status", card, 17, Muted);
            costs = Text("Authoritative action cost", card, 17, Ivory);
            requirements = Text("Authoritative prerequisites", card, 17, Amber);
            cardButton = Frame("V3 / execute canonical offer", card, true);
            cardButton.GetComponent<RoyalGoldV2FrameGraphic>().Primary = true;
            actionButton = cardButton.gameObject.AddComponent<RoyalButton>();
            actionButton.targetGraphic = cardButton.GetComponent<RoyalGoldV2FrameGraphic>();
            buttonLabel = Text("Canonical action label", cardButton, 18, Ivory, true);
            buttonLabel.alignment = TextAnchor.MiddleCenter;
            buttonLabel.rectTransform.anchorMin = Vector2.zero;
            buttonLabel.rectTransform.anchorMax = Vector2.one;
            buttonLabel.rectTransform.offsetMin = new Vector2(8, 5);
            buttonLabel.rectTransform.offsetMax = new Vector2(-8, -5);
            actionButton.onClick.AddListener(() =>
            {
                // V1 owns the idempotency, stale-revision check and sole gateway.
                if (!Source || !Source.Building || Source.Building.Offer == null) return;
                var real = Source.Building.Primary.Button;
                if (real && real.interactable) real.onClick.Invoke();
            });
            Visible(cardGroup, false, true);
            foreach (var id in TimedTargets) timers.Add(id, CreateTimer(id));
            ready = true;
        }
        private TimerView CreateTimer(string id)
        {
            var r = Frame("V3 / real-time operation / " + id, safe, false);
            var label = Text("Live remaining time", r, 18, Ivory, true);
            label.alignment = TextAnchor.MiddleCenter;
            var track = Rect("Actual progress", r);
            track.gameObject.AddComponent<CanvasRenderer>();
            var bg = track.gameObject.AddComponent<Image>();
            bg.color = new Color(.09f, .115f, .15f, .97f);
            bg.raycastTarget = false;
            var fill = Rect("Gateway progress fill", track);
            fill.gameObject.AddComponent<CanvasRenderer>();
            var ink = fill.gameObject.AddComponent<Image>();
            ink.color = Gold;
            ink.raycastTarget = false;
            fill.anchorMin = new Vector2(0, 0);
            fill.anchorMax = new Vector2(0, 1);
            fill.pivot = new Vector2(0, .5f);
            fill.anchoredPosition = Vector2.zero;
            fill.sizeDelta = Vector2.zero;
            var group = r.gameObject.AddComponent<CanvasGroup>();
            Visible(group, false, false);
            return new TimerView { Root = r, Label = label, Progress = fill, Fill = ink, Visibility = group };
        }
        private void LateUpdate()
        {
            if (!Application.isPlaying || !IsConfigured || !Source) return;
            EnsureView();
            if (!ready || safe.rect.width < 200 || safe.rect.height < 260) return;
            if (Time.unscaledTime >= nextRead)
            {
                nextRead = Time.unscaledTime + .25f;
                live.Clear();
                foreach (var id in TimedTargets)
                {
                    var data = ReadBuilding(id);
                    if (data != null) live[id] = data;
                }
            }
            var camera = ActiveCamera();
            bool blocked = (Source.Modal && Source.Modal.gameObject.activeInHierarchy) ||
                (Source.Mission && Source.Mission.gameObject.activeInHierarchy);
            RenderSelection(camera, blocked);
            foreach (var id in TimedTargets) RenderTimer(id, camera, blocked);
        }
        private static bool CanProject(Transform target, Camera camera, RectTransform safe, out Vector2 local)
        {
            local = Vector2.zero;
            if (!target || !target.gameObject.activeInHierarchy || !camera || !camera.enabled) return false;
            Vector3 position = target.position;
            var hit = target.GetComponent<Collider>();
            if (!hit) hit = target.GetComponentInChildren<Collider>();
            if (hit && hit.enabled)
            {
                var b = hit.bounds;
                position = new Vector3(b.center.x, b.max.y + .2f, b.center.z);
            }
            var screen = camera.WorldToScreenPoint(position);
            if (screen.z <= camera.nearClipPlane || screen.z >= camera.farClipPlane) return false;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(safe, screen, null, out local)) return false;
            var bounds = safe.rect;
            return local.x >= bounds.xMin - 36 && local.x <= bounds.xMax + 36 &&
                local.y >= bounds.yMin - 36 && local.y <= bounds.yMax + 36;
        }
        private static Vector2 FitToSafe(Vector2 anchor, float width, float height,
            RectTransform safe, float topExclusion, float bottomExclusion, bool above)
        {
            var bounds = safe.rect;
            float margin = 10f;
            float x = Mathf.Clamp(anchor.x, bounds.xMin + width * .5f + margin,
                bounds.xMax - width * .5f - margin);
            float minBottom = bounds.yMin + bottomExclusion;
            float maxTop = bounds.yMax - topExclusion;
            float preferred = above ? anchor.y + height * .5f + 14f :
                anchor.y - height * .5f - 16f;
            // Prefer the vertical-slice relationship (action below, timer above).
            // Flip to the opposite side before clamping when that would
            // push a control over the navigation/heading safe zones.
            float other = above ? anchor.y - height * .5f - 16f :
                anchor.y + height * .5f + 14f;
            float minCenter = minBottom + height * .5f;
            float maxCenter = maxTop - height * .5f;
            if (maxCenter < minCenter) { minCenter = maxCenter = (minBottom + maxTop) * .5f; }
            if ((preferred < minCenter || preferred > maxCenter) &&
                other >= minCenter && other <= maxCenter) preferred = other;
            return new Vector2(x, Mathf.Clamp(preferred, minCenter, maxCenter));
        }
        private void RenderSelection(Camera camera, bool blocked)
        {
            string id = Source.SelectedBuildingId;
            if (blocked || string.IsNullOrEmpty(id))
            {
                Visible(cardGroup, false, true);
                return;
            }
            // The canonical host may expose additional read-only world POIs
            // such as old-watch-ruin; no extra actions or state are invented.
            if (!live.TryGetValue(id, out var selected)) selected = ReadBuilding(id);
            if (selected == null) { Visible(cardGroup, false, true); return; }
            RenderSelectedBuilding(id, selected, camera);
        }
        private void RenderSelectedBuilding(string id, RoyalBuilding value, Camera camera)
        {
            var target = FindAnchor(id == "march-main" ? "barracks" : id);
            if (!CanProject(target, camera, safe, out var screen))
            {
                Visible(cardGroup, false, true);
                return;
            }
            bool offered = value.Action != null && !value.OperationActive;
            // A running operation gets ONLY the anchored timer; the selected
            // object must not create a second, competing action panel.
            if (value.OperationActive)
            {
                Visible(cardGroup, false, true);
                return;
            }
            title.Put(value.Name);
            status.Put(value.State);
            costs.Put(offered ? value.Costs : value.Result);
            requirements.Put(value.Requirements);
            buttonLabel.Put(offered ? value.Action.Label : "");
            actionButton.interactable = offered && value.Action.Enabled &&
                Source.Building && Source.Building.Primary && Source.Building.Primary.Button &&
                Source.Building.Primary.Button.interactable && Source.Building.Offer != null;
            cardButton.gameObject.SetActive(offered);
            float width = Mathf.Min(290f, safe.rect.width - 24f);
            float contentWidth = width - 24f;
            float y = 10f;
            y = PositionRow(title, y, contentWidth, 27f, 3f);
            y = PositionRow(status, y, contentWidth, 20f, 5f);
            y = PositionRow(costs, y, contentWidth, 0f, 5f);
            y = PositionRow(requirements, y, contentWidth, 0f, 6f);
            if (offered) { Place(cardButton, 12f, y, width - 24f, 48f); y += 56f; }
            float height = Mathf.Max(62f, y + 6f);
            card.sizeDelta = new Vector2(width, height);
            // Keep the selection card below the real resources/feedback/compact
            // chapter instead of solving overlap with unreadable tiny text.
            var exclusionTop = safe.rect.height > safe.rect.width ? 421f : 266f;
            var center = FitToSafe(screen, width, height, safe, exclusionTop, 88f, false);
            card.anchorMin = card.anchorMax = card.pivot = new Vector2(.5f, .5f);
            card.anchoredPosition = center;
            Visible(cardGroup, true, true);
        }
        private static float PositionRow(RoyalText label, float y, float width, float minimum, float gap)
        {
            bool show = !string.IsNullOrWhiteSpace(label.text);
            label.gameObject.SetActive(show);
            if (!show) return y;
            // Measured real text: no cropping costs, requirements or conditions.
            label.rectTransform.sizeDelta = new Vector2(width, minimum);
            float h = Mathf.Max(minimum, Mathf.Ceil(label.preferredHeight) + 3f);
            Place(label.rectTransform, 12f, y, width, h);
            return y + h + gap;
        }
        private void RenderTimer(string id, Camera camera, bool blocked)
        {
            var timer = timers[id];
            if (blocked || !live.TryGetValue(id, out var data) ||
                !data.OperationActive || string.IsNullOrWhiteSpace(data.Time))
            {
                Visible(timer.Visibility, false, false);
                return;
            }
            var target = FindAnchor(id);
            if (!CanProject(target, camera, safe, out var screen))
            {
                Visible(timer.Visibility, false, false);
                return;
            }
            float width = Mathf.Min(254f, safe.rect.width - 26f);
            float height = 70f;
            timer.Root.sizeDelta = new Vector2(width, height);
            timer.Label.Put(data.Time);
            timer.Label.fontSize = 18;
            Place(timer.Label.rectTransform, 8f, 7f, width - 16f, 42f);
            Place(timer.Progress, 10f, 57f, width - 20f, 6f);
            timer.Fill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(data.Progress), 1f);
            var center = FitToSafe(screen, width, height, safe,
                safe.rect.height > safe.rect.width ? 421f : 266f, 88f, true);
            timer.Root.anchorMin = timer.Root.anchorMax = timer.Root.pivot = new Vector2(.5f, .5f);
            timer.Root.anchoredPosition = center;
            Visible(timer.Visibility, true, false);
        }
    }
}
