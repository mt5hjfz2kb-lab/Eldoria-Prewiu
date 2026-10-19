using UnityEngine;

namespace Eldoria.UI.RoyalGold
{
    [ExecuteAlways]
    public sealed class RoyalHudLayout : MonoBehaviour
    {
        public RectTransform Resources, Mission, Feedback;
        private float priorWidth = -1, priorHeight = -1, priorResourceWidth = -1;
        private int priorScreenWidth = -1, priorScreenHeight = -1;
        private BuildingDetailsPanel building;

        private void LateUpdate()
        {
            var r = (RectTransform)transform;
            if (Mathf.Abs(r.rect.width - priorWidth) < .5f && Mathf.Abs(r.rect.height - priorHeight) < .5f &&
                Resources && Mathf.Abs(Resources.rect.width - priorResourceWidth) < .5f &&
                priorScreenWidth == Screen.width && priorScreenHeight == Screen.height) return;
            Apply(r.rect.width);
        }

        public void Apply(float width, bool offscreenCapture = false)
        {
            if (!Resources || !Mission || !Feedback || width < 1) return;
            priorWidth = width; priorHeight = ((RectTransform)transform).rect.height;
            priorScreenWidth = Screen.width; priorScreenHeight = Screen.height;
            priorResourceWidth = Resources.rect.width;
            // Unity's offscreen 1600px capture must not use the 640px editor
            // GameView to choose its layout. Runtime 640px must remain compact
            // even when CanvasScaler reports 1200 logical reference units.
            // A 960x540 desktop/phone landscape canvas has 1600 logical
            // reference units after CanvasScaler. The previous *pixel-width*
            // cutoff incorrectly forced a portrait-style full-width bottom sheet
            // and overlapped the left mission sheet. The free world viewport
            // then had yMin > yMax, making forest/quarry impossible to frame.
            // Use the actual aspect ratio for live layout; retained offscreen
            // landscape captures explicitly select the wide composition.
            bool landscape = Screen.width > Screen.height * 1.08f;
            bool wide = width >= 1080 && (offscreenCapture || landscape);
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
            if (building) building.ApplyLayout(width, !wide);
        }
    }
}
