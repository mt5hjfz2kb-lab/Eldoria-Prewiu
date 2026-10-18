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
        private string priorTitle;
        private int priorSize;

        public void Bind(string title, Transform target, Camera camera, RectTransform layer)
        {
            Target = target; PlayerCamera = camera; Layer = layer;
            Label.Put(title);
            Compact();
            if (!layer) return;
            var rect = (RectTransform)transform;
            rect.SetParent(layer, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        }
        private void LateUpdate() => RefreshPosition();
        public void RefreshPosition()
        {
            if (!Target || !PlayerCamera || !Layer) { SetVisible(false); return; }
            Compact();
            var screenPoint = PlayerCamera.WorldToScreenPoint(Target.position + WorldOffset);
            bool visible = screenPoint.z > 0 && PlayerCamera.pixelRect.Contains(screenPoint);
            SetVisible(visible);
            if (!visible) return;
            var canvas = Layer.GetComponentInParent<Canvas>();
            var uiCamera = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Layer, screenPoint, uiCamera, out var local))
                ((RectTransform)transform).anchoredPosition = local;
        }
        private void Compact()
        {
            if (!Label || !Select || (Label.text == priorTitle && Label.fontSize == priorSize)) return;
            priorTitle = Label.text; priorSize = Label.fontSize;
            var root = (RectTransform)transform;
            float width = Mathf.Clamp(Label.preferredWidth + 28, 112, 230);
            var text = Label.rectTransform;
            text.anchorMin = Vector2.zero; text.anchorMax = Vector2.one;
            text.offsetMin = new Vector2(10, 5); text.offsetMax = new Vector2(-10, -5);
            root.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            float height = Mathf.Clamp(Label.preferredHeight + 12, 46, 60);
            root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            var button = (RectTransform)Select.transform;
            button.anchorMin = Vector2.zero; button.anchorMax = Vector2.one;
            button.offsetMin = button.offsetMax = Vector2.zero;
        }
        private void SetVisible(bool value) { if (Select) Select.gameObject.SetActive(value); }
    }
}
