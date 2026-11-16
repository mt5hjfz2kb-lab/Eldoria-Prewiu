using UnityEngine;
using UnityEngine.InputSystem;

namespace Eldoria.Experimentos.Ciudad2D
{
    /// <summary>
    /// Cámara acotada. Retrato: vista completa; apaisado: encuadre utilizable,
    /// con arrastre y zoom limitados. Sin cambios a ProjectSettings.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class City2DCameraFit : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer ground;
        [SerializeField, Range(1f, 1.15f)] private float landscapeWidthAllowance = 1.04f;
        private Camera sceneCamera;
        private int lastWidth = -1, lastHeight = -1;
        private float zoom = 1f;
        private Vector2 pan;
        private Vector2 previousMouse;
        private bool wasDragging;
        private float previousPinchDistance = -1f;

        public void SetGround(SpriteRenderer renderer) { ground = renderer; Refit(); }
        private void Awake() { sceneCamera = GetComponent<Camera>(); Refit(); }
        private void OnEnable() => Refit();

        private void LateUpdate()
        {
            if (ground == null || ground.sprite == null) return;
            if (Screen.width != lastWidth || Screen.height != lastHeight) Refit();
            PollInput();
            ApplyCameraPosition();
        }

        private void Refit()
        {
            if (sceneCamera == null) sceneCamera = GetComponent<Camera>();
            if (sceneCamera == null || ground == null || ground.sprite == null) return;
            float aspect = Mathf.Max(0.1f, (float)Screen.width / Mathf.Max(1, Screen.height));
            bool nowLandscape = aspect > 1.2f;
            if (lastWidth < 0 || ((lastWidth > lastHeight) != (Screen.width > Screen.height)))
                pan = nowLandscape ? new Vector2(0f, 1.7f) : Vector2.zero;
            Vector2 size = ground.sprite.bounds.size;
            float contain = Mathf.Max(size.y * 0.5f, size.x / (2f * aspect));
            // Un terreno vertical NO puede contenerse entero en horizontal sin enormes
            // bandas laterales. Acercamos la cámara al ancho del terreno y permitimos
            // recorrerlo verticalmente con arrastre. El marco horizontal no se vacía.
            float desired = aspect > 1.2f ?
                size.x * landscapeWidthAllowance / (2f * aspect) : contain;
            sceneCamera.orthographicSize = Mathf.Clamp(desired / zoom, contain * 0.16f, contain);
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            ApplyCameraPosition();
        }

        private void PollInput()
        {
            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.1f)
                {
                    zoom = Mathf.Clamp(zoom * (scroll > 0 ? 1.09f : 0.92f), 1f, 1.95f);
                    Refit();
                }
                bool dragging = Mouse.current.middleButton.isPressed || Mouse.current.rightButton.isPressed;
                var pos = Mouse.current.position.ReadValue();
                if (dragging && wasDragging) PanPixels(pos - previousMouse);
                previousMouse = pos;
                wasDragging = dragging;
            }
            var screen = Touchscreen.current;
            if (screen == null) return;
            bool first = screen.touches[0].press.isPressed;
            bool second = screen.touches[1].press.isPressed;
            if (!first || !second)
            {
                previousPinchDistance = -1f;
                if (first && !second) PanPixels(screen.touches[0].delta.ReadValue());
                return;
            }
            var a = screen.touches[0].position.ReadValue();
            var b = screen.touches[1].position.ReadValue();
            float distance = Vector2.Distance(a, b);
            if (previousPinchDistance > 10f && distance > 10f)
            {
                zoom = Mathf.Clamp(zoom * distance / previousPinchDistance, 1f, 1.95f);
                Refit();
            }
            previousPinchDistance = distance;
        }

        private void PanPixels(Vector2 delta)
        {
            float worldPerPixel = sceneCamera.orthographicSize * 2f / Mathf.Max(1f, Screen.height);
            pan -= delta * worldPerPixel;
        }

        private void ApplyCameraPosition()
        {
            if (sceneCamera == null || ground == null || ground.sprite == null) return;
            Bounds bounds = ground.bounds;
            float halfHeight = sceneCamera.orthographicSize;
            float aspect = Mathf.Max(0.1f, (float)Screen.width / Mathf.Max(1, Screen.height));
            float xCap = Mathf.Max(0, bounds.extents.x - halfHeight * aspect);
            float yCap = Mathf.Max(0, bounds.extents.y - halfHeight);
            pan = new Vector2(Mathf.Clamp(pan.x, -xCap, xCap), Mathf.Clamp(pan.y, -yCap, yCap));
            transform.position = new Vector3(bounds.center.x + pan.x, bounds.center.y + pan.y, -10f);
        }
    }
}
