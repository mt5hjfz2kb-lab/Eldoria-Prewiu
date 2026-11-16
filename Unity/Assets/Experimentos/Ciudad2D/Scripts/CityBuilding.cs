using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Eldoria.Experimentos.Ciudad2D
{
    /// <summary>
    /// Presentación aislada. Nunca compra mejoras, cambia el guardado ni inventa niveles.
    /// Cada versión gráfica permanece estable entre hitos 1, 5, 10, 15...
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class CityBuilding : MonoBehaviour, IPointerClickHandler
    {
        [Serializable]
        public struct VisualTier
        {
            [Min(1)] public int MinLevel;
            public Sprite Sprite;
            [Min(0.01f)] public float Scale;

            public VisualTier(int minLevel, Sprite sprite, float scale)
            {
                MinLevel = minLevel;
                Sprite = sprite;
                Scale = scale;
            }
        }

        [SerializeField] private string buildingId = "bastion";
        [SerializeField] private string displayName = "Bastión";
        [SerializeField] private int level = 1;
        [SerializeField] private VisualTier[] visualTiers = Array.Empty<VisualTier>();
        [SerializeField, Range(0.06f, 0.30f)] private float bounceDuration = 0.16f;
        [SerializeField, Range(1.01f, 1.20f)] private float bounceMultiplier = 1.07f;

        private SpriteRenderer visual;
        private BoxCollider2D hitbox;
        private Coroutine bounce;
        private Vector3 baselineScale = Vector3.one;
        private int selectedArtLevel;
        [SerializeField] private City2DPlotPhase plotPhase = City2DPlotPhase.Built;
        [SerializeField] private SpriteRenderer availableMarker, constructionMarker;
        public City2DPlotPhase PlotPhase => plotPhase;

        public string BuildingId => buildingId;
        public string DisplayName => displayName;
        public int Level => level;
        public int SelectedArtLevel => selectedArtLevel;
        public event Action<CityBuilding> Selected;

        // Importante: no es 1, 6, 11... Los hitos son 1, 5, 10, 15.
        public static int PlannedMilestone(int actualLevel)
        {
            return actualLevel <= 0 ? 0 : actualLevel < 5 ? 1 : (actualLevel / 5) * 5;
        }

        public void Configure(string id, string title, int initialLevel, VisualTier[] tiers)
        {
            buildingId = id ?? string.Empty;
            displayName = title ?? string.Empty;
            visualTiers = tiers ?? Array.Empty<VisualTier>();
            CacheComponents();
            SetLevel(initialLevel);
        }

        private void Awake()
        {
            CacheComponents();
            // La versión inicial puede ser configurada durante la construcción de escena.
            if (visualTiers != null && visualTiers.Length != 0)
                SetAppearance(new City2DPlotView(plotPhase, level, 1));
        }

        private void CacheComponents()
        {
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            if (hitbox == null) hitbox = GetComponent<BoxCollider2D>();
        }

        public void ConfigurePlotMarkers(SpriteRenderer available, SpriteRenderer underConstruction)
        {
            availableMarker = available;
            constructionMarker = underConstruction;
            SetAppearance(new City2DPlotView(plotPhase, level, 1));
        }

        public void SetAppearance(City2DPlotView view)
        {
            plotPhase = view.Phase;
            SetLevel(view.Phase == City2DPlotPhase.Built ? view.BuiltLevel : 0);
            if (availableMarker != null) availableMarker.enabled = view.Phase == City2DPlotPhase.Available;
            if (constructionMarker != null) constructionMarker.enabled = view.Phase == City2DPlotPhase.UnderConstruction;
            if (hitbox != null) hitbox.enabled = view.Phase != City2DPlotPhase.Locked;
        }

        public void SetLevel(int nextLevel)
        {
            CacheComponents();
            if (bounce != null)
            {
                StopCoroutine(bounce);
                bounce = null;
            }
            level = Mathf.Max(0, nextLevel);
            bool present = level > 0;
            if (visual != null) visual.enabled = present;
            if (hitbox != null) hitbox.enabled = present;
            if (!present) { selectedArtLevel = 0; return; }

            // Máximo umbral con sprite real disponible <= nivel real.
            int bestLevel = -1;
            VisualTier best = default;
            if (visualTiers != null)
            {
                foreach (var tier in visualTiers)
                {
                    if (tier.Sprite == null || tier.MinLevel > level || tier.MinLevel <= bestLevel) continue;
                    bestLevel = tier.MinLevel;
                    best = tier;
                }
            }
            // No inventar arte de nivel 10/15 mientras no se haya producido.
            if (bestLevel < 0 && visualTiers != null)
            {
                foreach (var tier in visualTiers)
                {
                    if (tier.Sprite == null || (bestLevel >= 0 && tier.MinLevel >= bestLevel)) continue;
                    bestLevel = tier.MinLevel;
                    best = tier;
                }
            }
            if (bestLevel < 0)
            {
                selectedArtLevel = 0;
                if (visual != null) visual.enabled = false;
                if (hitbox != null) hitbox.enabled = false;
                return;
            }
            selectedArtLevel = bestLevel;
            visual.sprite = best.Sprite;
            baselineScale = Vector3.one * Mathf.Max(0.01f, best.Scale);
            transform.localScale = baselineScale;
            // Collision centrada en el edificio; no en el cielo o el humo.
            if (hitbox != null)
            {
                var size = best.Sprite.bounds.size;
                hitbox.size = new Vector2(size.x * 0.72f, size.y * 0.52f);
                hitbox.offset = new Vector2(0, size.y * 0.28f);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || plotPhase == City2DPlotPhase.Locked || eventData == null ||
                eventData.button != PointerEventData.InputButton.Left) return;
            Debug.Log($"[Ciudad2D] {displayName} — fase {plotPhase}, nivel {level}, arte hito {selectedArtLevel}", this);
            Selected?.Invoke(this);
            if (bounce != null) StopCoroutine(bounce);
            transform.localScale = baselineScale;
            bounce = StartCoroutine(Pulse());
        }

        private IEnumerator Pulse()
        {
            float elapsed = 0f;
            while (elapsed < bounceDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / bounceDuration);
                transform.localScale = baselineScale *
                    (1f + (bounceMultiplier - 1f) * Mathf.Sin(t * Mathf.PI));
                yield return null;
            }
            transform.localScale = baselineScale;
            bounce = null;
        }

        private void OnDisable()
        {
            if (bounce != null) StopCoroutine(bounce);
            bounce = null;
            transform.localScale = baselineScale;
        }
    }
}
