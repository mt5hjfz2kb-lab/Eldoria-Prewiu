using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Eldoria.World4XPrototype
{
    /// <summary>One camera and one coordinate space for the authored sector. No gameplay writes.</summary>
    [RequireComponent(typeof(Camera))]
    public sealed class ZoneOneStrategicCamera : MonoBehaviour
    {
        public Vector3 target = new Vector3(-18, 0, 30);
        public float localZoom = 32, regionalZoom = 60, globalZoom = 110;
        public Vector2 minimum = new Vector2(-120, -120), maximum = new Vector2(120, 120);
        Camera view;
        float desiredZoom = 60;
        Vector2 previousPointer;
        bool dragging;
        public Camera View => view != null ? view : GetComponent<Camera>();

        void Awake()
        {
            view = GetComponent<Camera>();
            view.orthographic = true;
            view.nearClipPlane = .3f;
            view.farClipPlane = 900;
            view.orthographicSize = regionalZoom;
            ApplyPose();
        }
        public void SetZoomLevel(int level, bool immediate = false)
        {
            View.orthographic = true;
            View.nearClipPlane = .3f;
            View.farClipPlane = 900;
            desiredZoom = level == 0 ? globalZoom : level == 1 ? regionalZoom : localZoom;
            if (immediate) View.orthographicSize = desiredZoom;
            ApplyPose();
        }
        public void PanTo(Vector3 point)
        {
            target = new Vector3(Mathf.Clamp(point.x, minimum.x, maximum.x), 0,
                Mathf.Clamp(point.z, minimum.y, maximum.y));
            ApplyPose();
        }
        void LateUpdate()
        {
            float scroll = 0;
            Vector2 movement = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                movement.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0)
                    - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0);
                movement.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0)
                    - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0);
                if (keyboard.digit1Key.wasPressedThisFrame) SetZoomLevel(0);
                if (keyboard.digit2Key.wasPressedThisFrame) SetZoomLevel(1);
                if (keyboard.digit3Key.wasPressedThisFrame) SetZoomLevel(2);
            }
            var touch = Touchscreen.current;
            bool activeTouch = touch != null && touch.touches[0].press.isPressed;
            if (Mouse.current != null && !activeTouch)
            {
                scroll = Mouse.current.scroll.ReadValue().y / 120f;
                Drag(Mouse.current.leftButton.isPressed, Mouse.current.position.ReadValue());
            }
            else if (!activeTouch) dragging = false;
            if (activeTouch)
            {
                var a = touch.touches[0]; var b = touch.touches[1];
                if (a.press.isPressed && b.press.isPressed)
                {
                    var pa = a.position.ReadValue(); var pb = b.position.ReadValue();
                    float oldDistance = Vector2.Distance(pa - a.delta.ReadValue(), pb - b.delta.ReadValue());
                    float newDistance = Vector2.Distance(pa, pb);
                    desiredZoom *= Mathf.Clamp(oldDistance / Mathf.Max(1, newDistance), .8f, 1.2f);
                    dragging = false;
                }
                else Drag(a.press.isPressed, a.position.ReadValue());
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            scroll = Input.mouseScrollDelta.y;
            Drag(Input.GetMouseButton(0), Input.mousePosition);
#endif
            target += new Vector3(movement.x, 0, movement.y) * View.orthographicSize * Time.unscaledDeltaTime;
            desiredZoom = Mathf.Clamp(desiredZoom * Mathf.Exp(-scroll * .12f), localZoom, globalZoom);
            View.orthographicSize = Mathf.Lerp(View.orthographicSize, desiredZoom, 1 - Mathf.Exp(-12 * Time.unscaledDeltaTime));
            PanTo(target);
        }
        void Drag(bool down, Vector2 pointer)
        {
            if (down && dragging)
            {
                // Intersections with a common ground plane keep pan aligned at every zoom.
                Plane ground = new Plane(Vector3.up, Vector3.zero);
                Ray previous = View.ScreenPointToRay(previousPointer), current = View.ScreenPointToRay(pointer);
                if (ground.Raycast(previous, out float p) && ground.Raycast(current, out float c))
                    target += previous.GetPoint(p) - current.GetPoint(c);
            }
            previousPointer = pointer; dragging = down;
        }
        void ApplyPose()
        {
            transform.rotation = Quaternion.Euler(52, 0, 0);
            transform.position = target - transform.forward * 350;
        }
    }
}
