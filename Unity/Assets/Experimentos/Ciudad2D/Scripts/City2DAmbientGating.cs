using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2D
{
    /// <summary>
    /// Background-set dressing is independently gated so a developed district
    /// cannot appear in early tutorial chapters. No gameplay colliders or data.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class City2DAmbientGating : MonoBehaviour
    {
        [SerializeField, Min(1)] private int minBastion = 1;
        [SerializeField, Min(0)] private int maxBastion;
        private SpriteRenderer visual;
        public void Configure(int minLevel, int maxLevel = 0)
        {
            minBastion = Mathf.Max(1, minLevel);
            maxBastion = Mathf.Max(0, maxLevel);
            Apply(1);
        }
        public void Apply(int currentBastionLevel)
        {
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            if (visual != null) visual.enabled = currentBastionLevel >= minBastion &&
                (maxBastion == 0 || currentBastionLevel <= maxBastion);
        }
    }
}
