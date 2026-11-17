using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2D
{
    /// <summary>
    /// Purely visual built-only functional detail for the isolated 2.5D QA city.
    /// Never modifies PlayerState, colliders, progression, scene selection or saves.
    /// It follows exactly the already resolved CityBuilding plot phase.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class City2DFunctionalDetailGate : MonoBehaviour
    {
        [SerializeField] private CityBuilding owner;
        [SerializeField] private SpriteRenderer visual;
        public bool IsPurePresentation => true;
        public bool IsVisible => visual != null && visual.enabled;
        public CityBuilding Owner => owner;

        public void Configure(CityBuilding linked)
        {
            owner = linked != null ? linked :
                throw new System.ArgumentNullException(nameof(linked));
            visual = GetComponent<SpriteRenderer>();
            if (GetComponent<Collider2D>() != null)
                throw new System.InvalidOperationException(
                    "Functional art detail is never a selectable game object");
            Refresh();
        }
        public void Refresh()
        {
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            visual.enabled = owner != null &&
                             owner.PlotPhase == City2DPlotPhase.Built &&
                             owner.Level > 0;
        }
        private void LateUpdate()
        {
            if (UnityEngine.Application.isPlaying) Refresh();
        }
    }
}
