using System;
using System.Collections.Generic;
using Eldoria.Application;
using Eldoria.Domain;
using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2D
{
    /// <summary>
    /// Adaptador de SOLO LECTURA. La fuente de verdad sigue siendo el ICommandGateway.
    /// El modo de demostración cambia solo sprites; no ejecuta GameCommand ni guarda.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class City2DStateBridge : MonoBehaviour
    {
        private readonly Dictionary<string, CityBuilding> buildings = new Dictionary<string, CityBuilding>();
        private ICommandGateway gateway;
        private int demoBastionLevel = 1;
        private readonly List<City2DAmbientGating> ambientGates = new List<City2DAmbientGating>();
        private float nextReadTime;

        public bool IsLiveBound => gateway != null;
        public int DemoBastionLevel => demoBastionLevel;
        public event Action<CityBuilding> BuildingSelected;
        public event Action LevelsChanged;

        // Unity serializes objects, not dictionaries or C# event subscriptions.
        public int RegisteredBuildingCount => buildings.Count;
        private void Awake() => RebindSceneBuildings();
        private void Start()
        {
            RebindSceneBuildings();
            if (!IsLiveBound) SetDemoBastionLevel(demoBastionLevel);
        }

        public void RebindSceneBuildings()
        {
            foreach (var b in UnityEngine.Object.FindObjectsByType<CityBuilding>(FindObjectsSortMode.None))
                if (b.gameObject.scene == gameObject.scene) Register(b);
            ambientGates.Clear();
            foreach (var prop in UnityEngine.Object.FindObjectsByType<City2DAmbientGating>(FindObjectsSortMode.None))
                if (prop.gameObject.scene == gameObject.scene) ambientGates.Add(prop);
        }

        public void Register(CityBuilding building)
        {
            if (building == null || string.IsNullOrEmpty(building.BuildingId)) return;
            if (buildings.TryGetValue(building.BuildingId, out var previous) && previous != null)
                previous.Selected -= OnSelected;
            buildings[building.BuildingId] = building;
            building.Selected -= OnSelected;
            building.Selected += OnSelected;
        }

        private void OnSelected(CityBuilding building) => BuildingSelected?.Invoke(building);

        public void BindGateway(ICommandGateway authoritativeGateway)
        {
            gateway = authoritativeGateway;
            if (buildings.Count == 0) RebindSceneBuildings();
            nextReadTime = 0f;
            if (gateway != null) ReadLiveSnapshot();
        }

        public void SetDemoBastionLevel(int selectedLevel)
        {
            if (IsLiveBound) return;
            demoBastionLevel = Mathf.Clamp(selectedLevel, 1, 5);
            foreach (var pair in buildings)
                if (pair.Value != null)
                    pair.Value.SetAppearance(City2DProgressionPolicy.DemoStage(demoBastionLevel, pair.Key));
            foreach (var prop in ambientGates) if (prop != null) prop.Apply(demoBastionLevel);
            LevelsChanged?.Invoke();
        }

        private void Update()
        {
            if (gateway == null || Time.unscaledTime < nextReadTime) return;
            nextReadTime = Time.unscaledTime + 0.5f;
            ReadLiveSnapshot();
        }

        private void ReadLiveSnapshot()
        {
            if (gateway == null) return;
            try { ApplySnapshot(gateway.Snapshot()); }
            catch (Exception ex)
            {
                // Una captura fallida nunca modifica el guardado ni inventa un nivel.
                Debug.LogWarning("[Ciudad2D] Snapshot no disponible: " + ex.Message, this);
            }
        }

        public void ApplySnapshot(PlayerState snapshot)
        {
            if (snapshot == null) return;
            foreach (var pair in buildings)
                if (pair.Value != null)
                    pair.Value.SetAppearance(City2DProgressionPolicy.FromSnapshot(snapshot, pair.Key));
            foreach (var prop in ambientGates)
                if (prop != null) prop.Apply(snapshot.BastionLevel);
            LevelsChanged?.Invoke();
        }

        private void OnDestroy()
        {
            foreach (var pair in buildings)
                if (pair.Value != null) pair.Value.Selected -= OnSelected;
        }
    }
}
