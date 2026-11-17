using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2D
{
    /// <summary>Sprite-only atmospheric glow, never a gameplay light source.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class City2DTorchPulse : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float opacity = .3f;
        [SerializeField] private float phase;
        private SpriteRenderer graphic;

        public void Configure(float initialOpacity, float initialPhase)
        {
            opacity = Mathf.Clamp01(initialOpacity);
            phase = initialPhase;
            Apply(opacity);
        }

        private void Awake() { graphic = GetComponent<SpriteRenderer>(); Apply(opacity); }
        private void Update()
        {
            // Two low-amplitude, phase-offset harmonics avoid a synchronized
            // mechanical sine. No dynamic lights, new textures, or color filters.
            float t = Time.unscaledTime;
            float slow = Mathf.Sin(t * 3.1f + phase);
            float quick = Mathf.Sin(t * 8.7f + phase * 2.31f);
            float intensity = Mathf.Clamp(0.88f + 0.085f * slow + 0.035f * quick, .71f, 1f);
            Apply(opacity * intensity);
        }
        private void Apply(float alpha)
        {
            if (graphic == null) graphic = GetComponent<SpriteRenderer>();
            if (graphic != null) graphic.color = new Color(1, 1, 1, alpha);
        }
    }
}
