using UnityEngine;

namespace Eldoria.UI.RoyalGold
{
    [ExecuteAlways]
    public sealed class RoyalHudLayout : MonoBehaviour
    {
        public RectTransform Resources, Mission, Feedback;
        private float priorWidth = -1, priorHeight = -1, priorResourceWidth = -1;
        private BuildingDetailsPanel building;

        private void LateUpdate()
        {
            var r = (RectTransform)transform;
            if (Mathf.Abs(r.rect.width - priorWidth) < .5f && Mathf.Abs(r.rect.height - priorHeight) < .5f &&
                Resources && Mathf.Abs(Resources.rect.width - priorResourceWidth) < .5f) return;
            Apply(r.rect.width);
        }

        public void Apply(float width)
        {
            if (!Resources || !Mission || !Feedback || width < 1) return;
            priorWidth = width; priorHeight = ((RectTransform)transform).rect.height;
            priorResourceWidth = Resources.rect.width;
            bool wide = width >= 1080;
            float stripWidth = Resources.rect.width;
            if (wide)
            {
                float left = Mathf.Clamp((width - stripWidth) / 2, 370, Mathf.Max(370, width - stripWidth - 142));
                Resources.anchoredPosition = new Vector2(left + stripWidth / 2 - width / 2, -22);
                Mission.anchoredPosition = new Vector2(26, -174);
                Feedback.anchoredPosition = new Vector2(0, -126);
            }
            else
            {
                Resources.anchoredPosition = new Vector2(0, -132);
                Mission.anchoredPosition = new Vector2(26, -288);
                Feedback.anchoredPosition = new Vector2(0, -232);
            }
            Feedback.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(650, width - 52));
            Feedback.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 48);
            var mission = Mission.GetComponent<MissionPanel>();
            if (mission) mission.ApplyLayout(width);
            if (!building) building = GetComponentInChildren<BuildingDetailsPanel>(true);
            if (building) building.ApplyLayout(width);
        }
    }
}
