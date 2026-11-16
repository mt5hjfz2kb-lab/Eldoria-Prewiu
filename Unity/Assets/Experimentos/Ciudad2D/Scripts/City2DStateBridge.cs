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
        private float nextReadTime;

        public bool IsLiveBound => gateway != null;
        public int DemoBastionLevel => demoBastionLevel;
        public event Action<CityBuilding> BuildingSelected;
        public event Action LevelsChanged;

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
            nextReadTime = 0f;
            if (gateway != null) ReadLiveSnapshot();
        }

        public void SetDemoBastionLevel(int selectedLevel)
        {
            if (IsLiveBound) return;
            demoBastionLevel = selectedLevel >= 5 ? 5 : 1;
            if (buildings.TryGetValue("bastion", out var bastion) && bastion != null)
                bastion.SetLevel(demoBastionLevel);
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
            Apply("bastion", snapshot.BastionLevel);
            Apply("aserradero", snapshot.SawmillLevel);
            Apply("cuartel", snapshot.BarracksLevel);
            Apply("granero", snapshot.ChapterThree != null ? snapshot.ChapterThree.GranaryLevel : 0);
            Apply("cantera", snapshot.ChapterThree != null ? snapshot.ChapterThree.StoneworksLevel : 0);
            // Salón de Héroes: arte de muestra. No existe nivel persistido separado.
            // No inferir una mejora del edificio a partir de los niveles de héroes.
            LevelsChanged?.Invoke();
        }

        private void Apply(string id, int level)
        {
            if (buildings.TryGetValue(id, out var building) && building != null)
                building.SetLevel(level);
        }

        private void OnDestroy()
        {
            foreach (var pair in buildings)
                if (pair.Value != null) pair.Value.Selected -= OnSelected;
        }
    }
}
