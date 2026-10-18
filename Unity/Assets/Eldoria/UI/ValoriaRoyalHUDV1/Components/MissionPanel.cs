using UnityEngine;
namespace Eldoria.UI.RoyalGold
{
    public sealed class MissionPanel : MonoBehaviour
    {
        public RoyalText Chapter, Objective, ProgressText;
        public RectTransform Fill, Details;
        public RoyalButton Toggle, Open;
        private bool collapsed;
        private float expandedHeight;
        private void Awake() { expandedHeight = ((RectTransform)transform).rect.height; }
        private void OnEnable() { if (Toggle) Toggle.onClick.AddListener(Switch); }
        private void OnDisable() { if (Toggle) Toggle.onClick.RemoveListener(Switch); }
        private void Switch()
        {
            var rect = (RectTransform)transform;
            if (!collapsed) expandedHeight = rect.rect.height;
            collapsed = !collapsed;
            if (Details) Details.gameObject.SetActive(!collapsed);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
                collapsed ? 70f : Mathf.Max(208f, expandedHeight));
        }
        public void Render(RoyalSnapshot state)
        {
            Chapter.Put(state.Chapter); Objective.Put(state.Objective);
            ProgressText.Put(state.ObjectiveProgress);
            Fill.anchorMax = new Vector2(Mathf.Clamp01(state.MissionFraction), 1f);
        }
    }
}
