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
            float intensity = 0.85f + 0.15f * Mathf.Sin(Time.unscaledTime * 4.4f + phase);
            Apply(opacity * intensity);
        }
        private void Apply(float alpha)
        {
            if (graphic == null) graphic = GetComponent<SpriteRenderer>();
            if (graphic != null) graphic.color = new Color(1, 1, 1, alpha);
        }
    }
}
