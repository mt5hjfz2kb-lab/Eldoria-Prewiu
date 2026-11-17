using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2D
{
    /// <summary>
    /// One separate contact shadow in the isolated premium preview.
    /// A shadow is visible ONLY when the authoritative building is completed;
    /// it has no collider, gameplay component, camera control or save access.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class City2DPremiumShadowGate : MonoBehaviour
    {
        [SerializeField] private CityBuilding owner;
        [SerializeField] private SpriteRenderer shadowRenderer;
        public bool IsPurePresentation => true;
        public bool IsVisible => shadowRenderer != null && shadowRenderer.enabled;
        public CityBuilding Owner => owner;

        public void Configure(CityBuilding linked)
        {
            owner = linked != null ? linked :
                throw new System.ArgumentNullException(nameof(linked));
            shadowRenderer = GetComponent<SpriteRenderer>();
            if (GetComponent<Collider2D>() != null)
                throw new System.InvalidOperationException("A contact shadow may not block a touch");
            Refresh();
        }
        public void Refresh()
        {
            if (shadowRenderer == null) shadowRenderer = GetComponent<SpriteRenderer>();
            bool built = owner != null && owner.PlotPhase == City2DPlotPhase.Built &&
                         owner.Level > 0;
            shadowRenderer.enabled = built;
            if (owner != null) transform.position = owner.transform.position;
        }
        private void LateUpdate()
        {
            if (Application.isPlaying) Refresh();
        }
    }
}
