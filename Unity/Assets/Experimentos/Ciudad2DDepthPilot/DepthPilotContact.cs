using System;
using Eldoria.Experimentos.Ciudad2D;
using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2DDepthPilot
{
    /// <summary>
    /// Localized, low-opacity inner contact shadow tied to measured alpha support.
    /// It does not replace the premium painted cast shadow or modify the source PNG.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DepthPilotContact : MonoBehaviour
    {
        [SerializeField] private CityBuilding owner;
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private Vector3 worldFootOffset;

        public bool PurePresentation => true;
        public bool Visible => visual != null && visual.enabled;

        public void Configure(CityBuilding source, SpriteRenderer image,
            Vector3 measuredFootOffset)
        {
            if (source == null || image == null)
                throw new ArgumentNullException(nameof(source));
            owner = source;
            visual = image;
            worldFootOffset = measuredFootOffset;
            Refresh();
        }

        public void Refresh()
        {
            if (owner == null || visual == null) return;
            bool built = owner.isActiveAndEnabled &&
                owner.Level > 0 && owner.PlotPhase == City2DPlotPhase.Built;
            visual.enabled = built;
            if (!built) return;
            transform.position = owner.transform.position + worldFootOffset;
            var building = owner.GetComponent<SpriteRenderer>();
            if (building != null) visual.sortingOrder = building.sortingOrder - 1;
        }

        private void LateUpdate()
        {
            if (UnityEngine.Application.isPlaying) Refresh();
        }
    }
}
