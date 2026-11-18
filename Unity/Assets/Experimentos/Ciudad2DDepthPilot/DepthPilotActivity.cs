using System;
using Eldoria.Experimentos.Ciudad2D;
using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2DDepthPilot
{
    /// <summary>
    /// Opt-in purely presentational human-scale activity in the isolated V6 depth experiment.
    /// Never reads/writes gameplay saves or changes the owner's collider or camera.
    /// All animation is deterministic and can be sampled identically in Editor and PlayMode.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DepthPilotActivity : MonoBehaviour
    {
        [SerializeField] private CityBuilding owner;
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private SpriteRenderer handTool;
        [SerializeField] private Vector3 restingPosition;
        [SerializeField] private Vector3 toolPosition;
        [SerializeField] private float phase;
        [SerializeField] private bool sawmill;
        [SerializeField] private bool configured;

        public bool PurePresentation => true;
        public bool Visible => body != null && body.enabled;
        public string BuildingId => owner != null ? owner.BuildingId : string.Empty;

        public void Configure(CityBuilding building, SpriteRenderer worker,
            SpriteRenderer tool, float timePhase, bool sawmillTask)
        {
            if (building == null || worker == null || tool == null)
                throw new ArgumentNullException(nameof(building));
            if (building.GetComponent<SpriteRenderer>() == null)
                throw new InvalidOperationException("Activity owner has no visual sprite");
            owner = building;
            body = worker;
            handTool = tool;
            phase = timePhase;
            sawmill = sawmillTask;
            restingPosition = worker.transform.localPosition;
            toolPosition = tool.transform.localPosition;
            configured = true;
            Sample(0f);
        }

        public void Sample(float seconds)
        {
            if (!configured || owner == null || body == null || handTool == null)
                return;

            bool built = owner.isActiveAndEnabled &&
                owner.PlotPhase == City2DPlotPhase.Built && owner.Level > 0;
            body.enabled = built;
            handTool.enabled = built;
            if (!built) return;

            float period = sawmill ? 3.2f : 4.8f;
            float t = Mathf.Repeat((seconds + phase) / period, 1f);
            // Short, subtle work cycle, then a true resting interval.
            float working = Mathf.Clamp01((t - 0.08f) / 0.54f);
            float activity = t < 0.65f ?
                Mathf.Sin(working * Mathf.PI * 2f) : 0f;
            float breathing = Mathf.Sin(seconds * 1.3f + phase) * 0.004f;

            body.transform.localPosition = restingPosition +
                new Vector3(0f, breathing, 0f);
            handTool.transform.localPosition = toolPosition +
                new Vector3(activity * 0.014f, activity * 0.014f, 0f);
            handTool.transform.localRotation = Quaternion.Euler(
                0f, 0f, (sawmill ? -27f : 12f) * activity);

            // World-foot based painter ordering; do not infer depth from camera pan.
            int footOrder = -Mathf.RoundToInt(transform.position.y * 100f);
            body.sortingOrder = footOrder;
            handTool.sortingOrder = footOrder + 1;
        }

        private void LateUpdate()
        {
            if (Application.isPlaying) Sample(Time.unscaledTime);
        }
    }
}
