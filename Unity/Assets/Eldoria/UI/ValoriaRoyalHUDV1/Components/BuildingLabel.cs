using UnityEngine;
namespace Eldoria.UI.RoyalGold
{
    public sealed class BuildingLabel : MonoBehaviour
    {
        public RoyalText Label;
        public RoyalButton Select;
        public Transform Target;
        public Camera PlayerCamera;
        public RectTransform Layer;
        public Vector3 WorldOffset = new Vector3(0, 2, 0);

        public void Bind(string title, Transform target, Camera camera, RectTransform layer)
        {
            Target = target; PlayerCamera = camera; Layer = layer;
            Label.Put(title);
            if (!layer) return;
            var rect = (RectTransform)transform;
            rect.SetParent(layer, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        }
        private void LateUpdate() => RefreshPosition();
        public void RefreshPosition()
        {
            if (!Target || !PlayerCamera || !Layer) { SetVisible(false); return; }
            var screenPoint = PlayerCamera.WorldToScreenPoint(Target.position + WorldOffset);
            bool visible = screenPoint.z > 0 && PlayerCamera.pixelRect.Contains(screenPoint);
            SetVisible(visible);
            if (!visible) return;
            var canvas = Layer.GetComponentInParent<Canvas>();
            var uiCamera = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Layer, screenPoint, uiCamera, out var local))
                ((RectTransform)transform).anchoredPosition = local;
        }
        private void SetVisible(bool value) { if (Select) Select.gameObject.SetActive(value); }
    }
}
