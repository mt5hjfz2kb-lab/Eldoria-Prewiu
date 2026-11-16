using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Eldoria.Experimentos.Ciudad2D
{
    /// <summary>
    /// Camara local del sandbox. No modifica ProjectSettings ni escenas de produccion.
    /// Pan tactil con umbral, exclusion real de UI y pinch; encuadre por resolucion
    /// para capturas nativas comparables sin alterar la pantalla del editor.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class City2DCameraFit : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer ground;
        [SerializeField, Range(1f, 1.15f)] private float landscapeWidthAllowance = 1.04f;
        [SerializeField, Range(8f, 30f)] private float touchPanThresholdPixels = 14f;

        private Camera sceneCamera;
        private int lastWidth = -1, lastHeight = -1;
        private int evidenceWidth, evidenceHeight;
        private float zoom = 1f;
        private Vector2 pan;
        private Vector2 previousMouse;
        private bool wasDragging;
        private float previousPinchDistance = -1f;

        private int singleTouchId = -1;
        private Vector2 singleStart, previousSingle;
        private bool startedOverUi, singleDragging;
        private readonly List<RaycastResult> uiHits = new List<RaycastResult>(8);

        private int ViewWidth => evidenceWidth > 0 ? evidenceWidth : Screen.width;
        private int ViewHeight => evidenceHeight > 0 ? evidenceHeight : Screen.height;

        public void SetGround(SpriteRenderer renderer) { ground = renderer; Refit(); }
        public void SetEvidenceViewport(int width, int height)
        {
            // Exclusivo de capturas: en juego siempre se usan Screen.width/height.
            evidenceWidth = Mathf.Max(1, width);
            evidenceHeight = Mathf.Max(1, height);
            lastWidth = -1;
            Refit();
        }
        public void ClearEvidenceViewport()
        {
            evidenceWidth = evidenceHeight = 0;
            lastWidth = -1;
            Refit();
        }

        private void Awake() { sceneCamera = GetComponent<Camera>(); Refit(); }
        private void OnEnable() => Refit();

        private void LateUpdate()
        {
            if (ground == null || ground.sprite == null) return;
            if (ViewWidth != lastWidth || ViewHeight != lastHeight) Refit();
            // La captura del editor no debe consumir gestos de un dispositivo real.
            if (evidenceWidth == 0) PollInput();
            ApplyCameraPosition();
        }

        private void Refit()
        {
            if (sceneCamera == null) sceneCamera = GetComponent<Camera>();
            if (sceneCamera == null || ground == null || ground.sprite == null) return;
            float aspect = Mathf.Max(0.1f, (float)ViewWidth / Mathf.Max(1, ViewHeight));
            bool nowLandscape = aspect > 1.2f;
            if (lastWidth < 0 || ((lastWidth > lastHeight) != (ViewWidth > ViewHeight)))
                pan = nowLandscape ? new Vector2(0f, 1.7f) : Vector2.zero;

            Vector2 size = ground.sprite.bounds.size;
            float contain = Mathf.Max(size.y * 0.5f, size.x / (2f * aspect));
            // En retrato se prioriza llenar la pantalla, conservando las seis
            // parcelas: un recorte lateral moderado evita bandas vacias.
            // La camara admite desplazamiento X acotado para ver las murallas.
            float desired = nowLandscape
                ? size.x * landscapeWidthAllowance / (2f * aspect)
                : size.y * 0.51f;
            sceneCamera.orthographicSize = Mathf.Clamp(desired / zoom, contain * 0.16f, contain);
            lastWidth = ViewWidth;
            lastHeight = ViewHeight;
            ApplyCameraPosition();
        }

        private void PollInput()
        {
            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.1f)
                {
                    zoom = Mathf.Clamp(zoom * (scroll > 0f ? 1.09f : 0.92f), 1f, 1.95f);
                    Refit();
                }
                bool dragging = Mouse.current.middleButton.isPressed || Mouse.current.rightButton.isPressed;
                Vector2 pos = Mouse.current.position.ReadValue();
                if (dragging && wasDragging) PanPixels(pos - previousMouse);
                previousMouse = pos;
                wasDragging = dragging;
            }

            var touchscreen = Touchscreen.current;
            if (touchscreen == null || touchscreen.touches.Count == 0) return;
            bool first = touchscreen.touches[0].press.isPressed;
            bool second = touchscreen.touches.Count > 1 && touchscreen.touches[1].press.isPressed;
            if (!first)
            {
                ResetTouchGesture();
                return;
            }

            if (second)
            {
                singleTouchId = -1;
                singleDragging = false;
                Vector2 a = touchscreen.touches[0].position.ReadValue();
                Vector2 b = touchscreen.touches[1].position.ReadValue();
                float distance = Vector2.Distance(a, b);
                if (previousPinchDistance > 10f && distance > 10f)
                {
                    zoom = Mathf.Clamp(zoom * distance / previousPinchDistance, 1f, 1.95f);
                    Refit();
                }
                previousPinchDistance = distance;
                return;
            }

            previousPinchDistance = -1f;
            int id = touchscreen.touches[0].touchId.ReadValue();
            Vector2 position = touchscreen.touches[0].position.ReadValue();
            if (singleTouchId != id)
            {
                singleTouchId = id;
                singleStart = previousSingle = position;
                singleDragging = false;
                startedOverUi = IsOverUi(position);
                return;
            }

            if (startedOverUi) return;
            if (!singleDragging && Vector2.Distance(position, singleStart) >= touchPanThresholdPixels)
            {
                singleDragging = true;
                previousSingle = position;
                return; // No desplazar al superar el umbral: evita saltos bruscos.
            }
            if (singleDragging) PanPixels(position - previousSingle);
            previousSingle = position;
        }

        private bool IsOverUi(Vector2 screenPosition)
        {
            if (EventSystem.current == null) return false;
            var data = new PointerEventData(EventSystem.current) { position = screenPosition };
            uiHits.Clear();
            EventSystem.current.RaycastAll(data, uiHits);
            foreach (var hit in uiHits)
                if (hit.module is GraphicRaycaster) return true;
            return false;
        }

        private void ResetTouchGesture()
        {
            singleTouchId = -1;
            singleDragging = false;
            startedOverUi = false;
            previousPinchDistance = -1f;
        }

        private void PanPixels(Vector2 delta)
        {
            float worldPerPixel = sceneCamera.orthographicSize * 2f / Mathf.Max(1f, ViewHeight);
            pan -= delta * worldPerPixel;
        }

        private void ApplyCameraPosition()
        {
            if (sceneCamera == null || ground == null || ground.sprite == null) return;
            Bounds bounds = ground.bounds;
            float halfHeight = sceneCamera.orthographicSize;
            float aspect = Mathf.Max(0.1f, (float)ViewWidth / Mathf.Max(1, ViewHeight));
            float xCap = Mathf.Max(0f, bounds.extents.x - halfHeight * aspect);
            float yCap = Mathf.Max(0f, bounds.extents.y - halfHeight);
            pan = new Vector2(Mathf.Clamp(pan.x, -xCap, xCap), Mathf.Clamp(pan.y, -yCap, yCap));
            transform.position = new Vector3(bounds.center.x + pan.x, bounds.center.y + pan.y, -10f);
        }
    }
}
