using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2D
{
    /// <summary>
    /// Lightweight sprite-only ambience for the isolated 2.5D city.
    /// No particles, collider, Instantiate in Update, gameplay action or save write.
    /// All movement is derived from time and fixed authored parameters.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class City2DAmbientMotion : MonoBehaviour
    {
        public enum MotionKind { Smoke, Ember }

        private SpriteRenderer visual;
        // CRITICAL: these values are authored in Editor and MUST survive Unity
        // scene serialization, domain reload and real PlayMode. Unserialized
        // fields caused the failed genuine PlayMode run #44052522821.
        [SerializeField] private CityBuilding owner;
        [SerializeField] private string effectId = "";
        [SerializeField] private MotionKind kind;
        [SerializeField] private Vector3 anchor;
        [SerializeField] private Vector3 sourceScale;
        [SerializeField] private float opacity, risePixels, driftPixels, period, phase;
        [SerializeField] private int minimumBastion = 1;
        [SerializeField] private int currentStage = 1;
        [SerializeField] private bool configured;
        private bool activeForStage;

        public string EffectId => effectId;
        public MotionKind Kind => kind;
        public bool HasBuildingOwner => owner != null;
        public bool ActiveForStage => activeForStage;
        public bool VisualEnabled => visual != null && visual.enabled;
        public bool IsPurePresentation => true;

        public void Configure(string id, MotionKind motionKind, CityBuilding attachedOwner,
            int firstBastionStage, float alpha, float rise, float drift, float cycleSeconds,
            float initialPhase)
        {
            if (string.IsNullOrEmpty(id) || cycleSeconds <= 0.4f ||
                rise < 0f || drift < 0f || alpha < 0f || alpha > 1f)
                throw new System.ArgumentException("Invalid ambience effect parameters");
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            effectId = id;
            kind = motionKind;
            owner = attachedOwner;
            minimumBastion = Mathf.Max(1, firstBastionStage);
            opacity = alpha;
            risePixels = rise;
            driftPixels = drift;
            period = cycleSeconds;
            phase = initialPhase;
            anchor = transform.localPosition;
            sourceScale = transform.localScale;
            configured = true;
            ApplyStage(1);
        }

        public void ApplyStage(int bastionStage)
        {
            currentStage = Mathf.Clamp(bastionStage, 1, 5);
            EvaluateAtTime(Time.unscaledTime);
        }

        /// <summary>
        /// Reproducible editor proof: same scene rendered at different animation
        /// instants without a clock, touching domain state, or restarting Unity.
        /// </summary>
        public void EvaluateAtTime(float seconds)
        {
            if (!configured) return;
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            activeForStage = currentStage >= minimumBastion &&
                (owner == null || (owner.PlotPhase == City2DPlotPhase.Built && owner.Level > 0));
            visual.enabled = activeForStage;
            if (!activeForStage) return;

            float t = Mathf.Repeat((seconds + phase) / period, 1f);
            float envelope = Mathf.Pow(Mathf.Sin(Mathf.PI * t), kind == MotionKind.Smoke ? 1.25f : 1.7f);
            float sway = Mathf.Sin(6.283185f * t + phase * .71f) * driftPixels;
            float up = t * risePixels;
            transform.localPosition = anchor + new Vector3(sway / 100f, up / 100f, 0f);
            float expansion = kind == MotionKind.Smoke ? 1f + .28f * t : 1f + .12f * t;
            transform.localScale = sourceScale * expansion;
            float flicker = kind == MotionKind.Smoke ?
                0.90f + .10f * Mathf.Sin(seconds * 1.32f + phase) :
                0.87f + .13f * Mathf.Sin(seconds * 8.1f + phase);
            float alpha = opacity * envelope * flicker;
            visual.color = kind == MotionKind.Smoke
                ? new Color(.87f, .89f, .93f, alpha)
                : new Color(1f, .64f, .29f, alpha);
        }

        private void Update()
        {
            if (UnityEngine.Application.isPlaying)
                EvaluateAtTime(Time.unscaledTime);
        }
    }
}
