using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2D
{
    /// <summary>
    /// Subtle anchored activity on an EXISTING prop; no duplicated illustration.
    /// Conditional on the same read-only building stage as the parent workshop.
    /// All poses are explicit functions of evidence time, without simulation.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class City2DGroundedActivity : MonoBehaviour
    {
        [SerializeField] private CityBuilding owner;
        [SerializeField] private Vector3 anchorPosition;
        [SerializeField] private Quaternion anchorRotation;
        [SerializeField] private float period = 3f;
        [SerializeField] private float swayDegrees = 1f;
        [SerializeField] private float travelWorld = .015f;
        [SerializeField] private float phase;
        [SerializeField] private int originalOrder;
        [SerializeField] private bool configured;
        private SpriteRenderer visual;

        public bool IsPresentationOnly => true;
        public CityBuilding Owner => owner;

        public void Configure(CityBuilding linkedOwner, float cycle, float degrees,
            float travel, float startPhase)
        {
            if (linkedOwner == null || cycle < 1f || degrees < 0f ||
                degrees > 3f || travel < 0f || travel > .07f)
                throw new System.ArgumentException("Invalid grounded activity");
            owner = linkedOwner;
            visual = GetComponent<SpriteRenderer>();
            anchorPosition = transform.position;
            anchorRotation = transform.rotation;
            originalOrder = visual.sortingOrder;
            period = cycle;
            swayDegrees = degrees;
            travelWorld = travel;
            phase = startPhase;
            configured = true;
            EvaluateAtTime(0f);
        }

        public void EvaluateAtTime(float seconds)
        {
            if (!configured) return;
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            bool built = owner != null && owner.PlotPhase == City2DPlotPhase.Built &&
                         owner.Level > 0;
            float wave = built ? Mathf.Sin((seconds / period + phase) * Mathf.PI * 2f) : 0f;
            transform.position = anchorPosition + new Vector3(wave * travelWorld, 0f, 0f);
            transform.rotation = anchorRotation * Quaternion.Euler(0f, 0f, wave * swayDegrees);
            // Sorting uses the ground-contact anchor, not image centre or roof.
            visual.sortingOrder = originalOrder;
        }

        private void Update()
        {
            if (UnityEngine.Application.isPlaying) EvaluateAtTime(Time.unscaledTime);
        }

        private void OnDisable()
        {
            if (!configured) return;
            transform.position = anchorPosition;
            transform.rotation = anchorRotation;
        }
    }
}
