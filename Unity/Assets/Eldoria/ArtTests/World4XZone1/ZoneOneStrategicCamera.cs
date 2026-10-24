using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace Eldoria.World4XPrototype
{
    /// <summary>One bounded camera, one coordinate space. No scene or gameplay writes.</summary>
    [RequireComponent(typeof(Camera))]
    public sealed class ZoneOneStrategicCamera : MonoBehaviour
    {
        public Vector3 target = new Vector3(-18, 0, 30);
        public float localZoom = 32, regionalZoom = 60, globalZoom = 110;
        [Range(35, 80)] public float tilt = 55;
        [Range(-180, 180)] public float heading;
        public bool AllowWholeRegionOverview;
        public float edgeMargin = 2, dragThresholdPixels = 8;
        // Orthographic framing depends on zoom, not this depth offset.
        // Integrated Valoria sets 140 to keep visible ground inside URP shadows.
        [Min(80)] public float cameraDistance = 350;
        public Vector2 minimum = new Vector2(-120, -120), maximum = new Vector2(120, 120);
        // The UI owner injects its hit test. This component never creates a second UI.
        public Func<Vector2, bool> PointerBlocked;
        Camera view;
        float desiredZoom = 60;
        Vector2 previousPointer, pressPointer, zoomFocus;
        bool pressed, blockedGesture, zoomAnchored;
        public bool WasDraggedThisGesture { get; private set; }
        public Camera View => view != null ? view : (view = GetComponent<Camera>());

        void Awake()
        {
            View.orthographic = true; View.nearClipPlane = .3f; View.farClipPlane = Mathf.Max(900, cameraDistance * 2.5f);
            desiredZoom = regionalZoom; View.orthographicSize = desiredZoom;
            ApplyPose(); FitToAuthoredBounds();
        }
        float MaximumFittingZoom()
        {
            float width = Mathf.Max(.2f, maximum.x - minimum.x - edgeMargin * 2);
            float depth = Mathf.Max(.2f, maximum.y - minimum.y - edgeMargin * 2);
            float c = Mathf.Abs(Mathf.Cos(heading * Mathf.Deg2Rad)), s = Mathf.Abs(Mathf.Sin(heading * Mathf.Deg2Rad));
            float ground = 1f / Mathf.Sin(tilt * Mathf.Deg2Rad);
            return Mathf.Max(.1f, .5f * Mathf.Min(width / (View.aspect * c + ground * s), depth / (View.aspect * s + ground * c)));
        }
        /// <summary>
        /// Wide, bounded first entry. Leave maneuvering margin rather than
        /// filling the entire map and making 4-way panning impossible.
        /// Existing terrain bounds (not invented world geometry) are authoritative.
        /// </summary>
        public float StrategicEntryZoom()
        {
            // Whole-region overview may exceed the navigable footprint, but
            // first entry must retain room to pan on BOTH ground axes. The
            // panorama's aspect/heading-specific zoom is not that limit.
            if (AllowWholeRegionOverview)
                return Mathf.Max(.1f, Mathf.Min(globalZoom, MaximumFittingZoom()) * .82f);
            float max = AllowWholeRegionOverview ? globalZoom : Mathf.Min(globalZoom, MaximumFittingZoom());
            float minimumWide = Mathf.Min(max, Mathf.Max(localZoom, regionalZoom * 1.65f));
            return Mathf.Max(minimumWide, max * .82f);
        }

        public void EnterStrategicView(Vector3 anchor)
        {
            target = anchor;
            desiredZoom = StrategicEntryZoom();
            zoomAnchored = false;
            View.orthographicSize = desiredZoom;
            ApplyPose();
            FitToAuthoredBounds();
        }

        public void SetZoomLevel(int level, bool immediate = false)
        {
            if (level < 0 || level > 2) throw new ArgumentOutOfRangeException(nameof(level));
            View.orthographic = true; View.nearClipPlane = .3f; View.farClipPlane = Mathf.Max(900, cameraDistance * 2.5f);
            desiredZoom = AllowWholeRegionOverview ? (level == 0 ? globalZoom : level == 1 ? regionalZoom : localZoom) : Mathf.Min(level == 0 ? globalZoom : level == 1 ? regionalZoom : localZoom, MaximumFittingZoom());
            zoomAnchored = false;
            if (immediate) View.orthographicSize = desiredZoom;
            ApplyPose(); FitToAuthoredBounds();
        }
        public bool GroundPoint(Vector2 screen, out Vector3 point)
        {
            var plane = new Plane(Vector3.up, new Vector3(0, target.y, 0));
            var ray = View.ScreenPointToRay(screen);
            if (plane.Raycast(ray, out float distance)) { point = ray.GetPoint(distance); return true; }
            point = default; return false;
        }
        public void ZoomAt(Vector2 screen, float factor, bool immediate = false)
        {
            if (factor <= 0 || float.IsNaN(factor) || float.IsInfinity(factor)) throw new ArgumentOutOfRangeException(nameof(factor));
            float max = AllowWholeRegionOverview ? globalZoom : Mathf.Min(globalZoom, MaximumFittingZoom());
            desiredZoom = Mathf.Clamp(desiredZoom * factor, Mathf.Min(localZoom, max), max);
            zoomFocus = screen; zoomAnchored = true;
            if (immediate) ApplyZoom(desiredZoom);
        }
        void ApplyZoom(float size)
        {
            Vector3 before = default;
            bool anchor = zoomAnchored && GroundPoint(zoomFocus, out before);
            View.orthographicSize = size; ApplyPose();
            if (anchor && GroundPoint(zoomFocus, out Vector3 after)) target += before - after;
            ApplyPose(); FitToAuthoredBounds();
        }
        public void SetOrientation(float pitch, float bearing)
        {
            Vector3 before = default;
            bool anchor = zoomAnchored && GroundPoint(zoomFocus, out before);
            tilt = pitch; heading = bearing; ApplyPose();
            if (anchor && GroundPoint(zoomFocus, out Vector3 after)) target += before - after;
            FitToAuthoredBounds();
        }
        public void PanTo(Vector3 point) { target = point; ApplyPose(); FitToAuthoredBounds(); }
        // Restore an observed pose after a temporary render or an orientation check.
        public void RestoreView(Vector3 point, float zoom)
        {
            target = point; desiredZoom = zoom; View.orthographicSize = zoom;
            zoomAnchored = false; ApplyPose(); FitToAuthoredBounds();
        }
        public void FitToAuthoredBounds()
        {
            float fit = AllowWholeRegionOverview ? globalZoom : MaximumFittingZoom();
            View.orthographicSize = Mathf.Min(View.orthographicSize, fit);
            desiredZoom = Mathf.Min(desiredZoom, fit);
            float screenHalf = View.orthographicSize * View.aspect;
            float groundHalf = View.orthographicSize / Mathf.Sin(tilt * Mathf.Deg2Rad);
            float c = Mathf.Abs(Mathf.Cos(heading * Mathf.Deg2Rad)), s = Mathf.Abs(Mathf.Sin(heading * Mathf.Deg2Rad));
            float halfX = screenHalf * c + groundHalf * s;
            float halfZ = screenHalf * s + groundHalf * c;
            float x0 = minimum.x + edgeMargin + halfX, x1 = maximum.x - edgeMargin - halfX;
            float z0 = minimum.y + edgeMargin + halfZ, z1 = maximum.y - edgeMargin - halfZ;
            target.x = x0 <= x1 ? Mathf.Clamp(target.x, x0, x1) : (minimum.x + maximum.x) * .5f;
            target.z = z0 <= z1 ? Mathf.Clamp(target.z, z0, z1) : (minimum.y + maximum.y) * .5f;
            ApplyPose();
        }
        void LateUpdate()
        {
            Vector2 movement = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                movement.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0);
                movement.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0);
                if (keyboard.digit1Key.wasPressedThisFrame) SetZoomLevel(0);
                if (keyboard.digit2Key.wasPressedThisFrame) SetZoomLevel(1);
                if (keyboard.digit3Key.wasPressedThisFrame) SetZoomLevel(2);
            }
            TouchControl first = null, second = null;
            if (Touchscreen.current != null)
                foreach (var touch in Touchscreen.current.touches)
                    if (touch.press.isPressed) { if (first == null) first = touch; else { second = touch; break; } }
            if (second != null)
            {
                var a = first.position.ReadValue(); var b = second.position.ReadValue();
                if (!(PointerBlocked?.Invoke(a) ?? false) && !(PointerBlocked?.Invoke(b) ?? false))
                {
                    float oldDistance = Vector2.Distance(a - first.delta.ReadValue(), b - second.delta.ReadValue());
                    ZoomAt((a + b) * .5f, Mathf.Clamp(oldDistance / Mathf.Max(1, Vector2.Distance(a, b)), .8f, 1.2f));
                }
                pressed = false; WasDraggedThisGesture = true;
            }
            else if (first != null) Drag(true, first.position.ReadValue());
            else if (Mouse.current != null)
            {
                var pointer = Mouse.current.position.ReadValue();
                float scroll = Mouse.current.scroll.ReadValue().y / 120f;
                if (Mathf.Abs(scroll) > .001f && !(PointerBlocked?.Invoke(pointer) ?? false)) ZoomAt(pointer, Mathf.Exp(-scroll * .12f));
                Drag(Mouse.current.leftButton.isPressed, pointer);
            }
            else pressed = false;
#elif ENABLE_LEGACY_INPUT_MANAGER
            movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector2 pointer = Input.mousePosition;
            if (Mathf.Abs(Input.mouseScrollDelta.y) > .001f && !(PointerBlocked?.Invoke(pointer) ?? false)) ZoomAt(pointer, Mathf.Exp(-Input.mouseScrollDelta.y * .12f));
            Drag(Input.GetMouseButton(0), pointer);
#endif
            target += Quaternion.Euler(0, heading, 0) * new Vector3(movement.x, 0, movement.y) * View.orthographicSize * Time.unscaledDeltaTime;
            ApplyPose(); ApplyZoom(Mathf.Lerp(View.orthographicSize, desiredZoom, 1 - Mathf.Exp(-12 * Time.unscaledDeltaTime)));
            if (Mathf.Abs(View.orthographicSize - desiredZoom) < .001f) zoomAnchored = false;
        }
        void Drag(bool down, Vector2 pointer)
        {
            if (down && !pressed)
            {
                pressPointer = previousPointer = pointer; WasDraggedThisGesture = false;
                blockedGesture = PointerBlocked?.Invoke(pointer) ?? false;
            }
            else if (down && !blockedGesture)
            {
                if ((pointer - pressPointer).sqrMagnitude >= dragThresholdPixels * dragThresholdPixels) WasDraggedThisGesture = true;
                if (WasDraggedThisGesture && GroundPoint(previousPointer, out var before) && GroundPoint(pointer, out var after)) target += before - after;
            }
            previousPointer = pointer; pressed = down;
        }
        void ApplyPose()
        {
            transform.rotation = Quaternion.Euler(tilt, heading, 0);
            transform.position = target - transform.forward * cameraDistance;
        }
    }
}
