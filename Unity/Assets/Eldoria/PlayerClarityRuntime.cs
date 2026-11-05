using System;
using System.Collections.Generic;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.PlayerClarity;
using Eldoria.Presentation;
using Eldoria.ReferenceClean;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using Eldoria.World4XCanon;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Eldoria
{
    /// <summary>
    /// Presentation-only scene adapter for the integrated Valoria gameplay.
    /// Does not replace the canonical HUD, pick colliders, terrain, economy,
    /// saved state or combat. Its icons and labels never intercept input.
    /// LocalGateway remains the sole authority for renewable populations.
    /// </summary>
    [DefaultExecutionOrder(3500)]
    public sealed class PlayerClarityRuntime : MonoBehaviour
    {
        private sealed class Marker
        {
            public RectTransform Rect;
            public Text Text;
            public RoyalResourcePictogram Icon;
            public string Id;
            public bool Touched;
        }

        private static readonly TimeSpan RenewableDelay = RenewableRespawnRules.Delay;
        private readonly Dictionary<string, Marker> markers = new Dictionary<string, Marker>();
        private readonly Dictionary<string, WorldHotspot> hotspots = new Dictionary<string, WorldHotspot>();
        private readonly HashSet<long> occupiedScreenCells = new HashSet<long>();
        private ValoriaIntegratedWorld host;
        private RoyalHUD hud;
        private RectTransform layer;
        private LocalGateway configuredGateway;
        private RegionOneResourcePlacement configuredTerrain;
        private bool creaturesConfigured;
        private float nextUpdate;
        private float nextScan;
        private Rect visibleViewport = new Rect(0, 0, 1, 1);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneLoaded += SceneLoaded;
        }

        private static void SceneLoaded(Scene scene, LoadSceneMode unused)
        {
            if (!scene.name.StartsWith("Valoria", StringComparison.OrdinalIgnoreCase))
                return;
            if (UnityEngine.Object.FindFirstObjectByType<PlayerClarityRuntime>() != null)
                return;
            var go = new GameObject("Eldoria / player clarity (canonical scene adapter)");
            DontDestroyOnLoad(go);
            go.AddComponent<PlayerClarityRuntime>();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
        }

        private void LateUpdate()
        {
            if (Time.unscaledTime < nextUpdate) return;
            nextUpdate = Time.unscaledTime + .20f;
            var candidate = UnityEngine.Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
            if (!candidate || !candidate.Ready || !candidate.Hud || candidate.City?.Gateway == null)
            {
                HideAll();
                return;
            }
            if (candidate != host || candidate.Hud != hud)
                Bind(candidate);
            ConfigureRenewableDelay();
            if (!layer || !host.PlayerCamera) return;
            // Calculate the canonical camera-safe UI viewport ONCE per sweep.
            // The V2 method forces Canvas geometry; calling it per node
            // would multiply layout work on 84+ renewable map targets.
            var v2 = hud.GetComponent<RoyalGoldV2Hud>();
            visibleViewport = v2 ? v2.GetFreeSceneViewport() : new Rect(0, 0, 1, 1);
            foreach (var marker in markers.Values) marker.Touched = false;
            occupiedScreenCells.Clear();

            var snapshot = host.City.Gateway.Snapshot();
            bool modal = hud.Modal && hud.Modal.gameObject.activeSelf;
            var management = hud.GetComponent<RoyalGoldV3ManagementScreens>();
            bool obstructed = modal || (management && management.IsOpen);
            if (!obstructed)
            {
                if (host.InWorld) RenderWorld(snapshot);
                else RenderCity(snapshot);
            }
            foreach (var marker in markers.Values)
                if (marker.Rect && !marker.Touched && marker.Rect.gameObject.activeSelf)
                    marker.Rect.gameObject.SetActive(false);
        }

        private void Bind(ValoriaIntegratedWorld candidate)
        {
            if (layer) Destroy(layer.gameObject);
            markers.Clear();
            hotspots.Clear();
            host = candidate;
            hud = candidate.Hud;
            configuredGateway = null;
            configuredTerrain = null;
            creaturesConfigured = false;
            nextScan = 0f;
            var parent = hud.transform as RectTransform;
            if (!parent) return;
            var go = new GameObject("Player clarity / non-interactive projected labels",
                typeof(RectTransform));
            layer = (RectTransform)go.transform;
            layer.SetParent(parent, false);
            layer.anchorMin = Vector2.zero;
            layer.anchorMax = Vector2.one;
            layer.offsetMin = layer.offsetMax = Vector2.zero;
            layer.SetAsLastSibling();
        }

        /// <summary>
        /// Only the renewable local population uses the five-minute setting.
        /// Story targets and multiplayer/server populations are untouched.
        /// Existing cooldown timestamps still need a separate authoritative
        /// save migration; never rewrite a snapshot from presentation code.
        /// </summary>
        private void ConfigureRenewableDelay()
        {
            if (!(host.City.Gateway is LocalGateway local) ||
                local.Snapshot().WorldId != "world-local" || !host.WorldRoot)
                return;
            var terrain = host.WorldRoot.GetComponentInChildren<RegionOneResourcePlacement>(true);
            if (!terrain) return;
            if (configuredGateway != local || configuredTerrain != terrain)
            {
                local.ConfigureLocalResourceRespawn(terrain, RenewableDelay);
                configuredGateway = local;
                configuredTerrain = terrain;
                creaturesConfigured = false;
            }
            if (!creaturesConfigured &&
                host.WorldRoot.GetComponentInChildren<RegionOneCreaturePopulation>(true))
            {
                local.ConfigureLocalCreatureRespawn(terrain, RenewableDelay);
                creaturesConfigured = true;
            }
        }

        private void ScanWorld()
        {
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 1f;
            hotspots.Clear();
            if (!host.WorldRoot) return;
            foreach (var spot in host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true))
                if (spot && !string.IsNullOrEmpty(spot.Id))
                    hotspots[spot.Id] = spot;
        }

        private bool TryHotspot(string id, out WorldHotspot target)
        {
            if (hotspots.TryGetValue(id, out target) && target) return true;
            target = host.FindTarget(id);
            if (target) { hotspots[id] = target; return true; }
            return false;
        }

        private void RenderWorld(PlayerState s)
        {
            if (!host.WorldCamera || !host.WorldCamera.enabled || !host.WorldNavigation)
                return;
            var camera = host.WorldCamera;
            if (RegionOneResourcePlacement.IsPanoramic(
                camera.orthographicSize, host.WorldNavigation.globalZoom)) return;
            ScanWorld();
            foreach (var resource in s.WorldResourceLocations)
            {
                if (resource.Generation < 1 || resource.Remaining <= 0 ||
                    !TryHotspot(resource.TargetId, out var hot) ||
                    !hot.gameObject.activeInHierarchy) continue;
                ResourcePictogramKind kind;
                if (resource.ResourceKind == "wood") kind = ResourcePictogramKind.Wood;
                else if (resource.ResourceKind == "stone") kind = ResourcePictogramKind.Stone;
                else continue;
                Place(resource.TargetId, "", kind, hot, camera, false, 0);
            }
            foreach (var id in new[] {"forest-valoria", "quarry-valoria"})
            {
                if (LocalGateway.ResourceRemaining(s, id) <= 0 ||
                    !TryHotspot(id, out var hot) || !hot.gameObject.activeInHierarchy) continue;
                Place(id, "", id == "forest-valoria"
                    ? ResourcePictogramKind.Wood : ResourcePictogramKind.Stone,
                    hot, camera, false, 0);
            }
            foreach (var creature in s.WorldCreatureLocations)
            {
                if (!creature.Alive || creature.Level < 1 ||
                    !TryHotspot(creature.TargetId, out var hot) ||
                    !hot.gameObject.activeInHierarchy) continue;
                Place(creature.TargetId, "Nv. " + creature.Level, null,
                    hot, camera, false, 0);
            }
        }

        private void RenderCity(PlayerState s)
        {
            var camera = host.City?.PlayerCamera;
            if (!camera || !camera.enabled) return;
            AddBuilding(s, "bastion", s.BastionLevel, camera);
            AddBuilding(s, "sawmill", s.SawmillLevel, camera);
            AddBuilding(s, "barracks", s.BarracksLevel, camera);
            if (s.ChapterThree != null)
            {
                AddBuilding(s, "granary", s.ChapterThree.GranaryLevel, camera);
                AddBuilding(s, "stoneworks", s.ChapterThree.StoneworksLevel, camera);
            }
        }

        private void AddBuilding(PlayerState state, string id, int level, Camera camera)
        {
            if (level < 1 || hud.SelectedBuildingId == id) return;
            GameObject model = null;
            var advanced = host.GetComponent<BastionIVVWorldPresentation>();
            if (advanced) model = advanced.ModelFor(id);
            if (!model || !model.activeInHierarchy)
            {
                if (id == "bastion")
                    model = state.BastionLevel >= 2 ? host.City.BastionTwo : host.City.BastionOne;
                else if (id == "sawmill" && host.City.Parcels)
                    model = host.City.Parcels.LeftBuilt;
                else if (id == "barracks" && host.City.Parcels)
                    model = host.City.Parcels.RightBuilt;
                else if (id == "granary" || id == "stoneworks")
                {
                    var target = host.FindTarget(id);
                    if (target) model = target.gameObject;
                }
            }
            if (!model || !model.activeInHierarchy) return;
            Place(id, "Nv. " + level, null, null, camera, true, level, model);
        }

        private static Vector3 TopOf(GameObject go, bool building)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(false);
            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);
                return new Vector3(bounds.center.x, bounds.max.y + (building ? 1.1f : .75f), bounds.center.z);
            }
            var collider = go.GetComponent<Collider>();
            return collider
                ? new Vector3(collider.bounds.center.x, collider.bounds.max.y + .7f, collider.bounds.center.z)
                : go.transform.position + Vector3.up * 4f;
        }

        private void Place(string id, string text, ResourcePictogramKind? icon,
            WorldHotspot hot, Camera camera, bool building, int level, GameObject model = null)
        {
            var go = model ? model : hot ? hot.gameObject : null;
            if (!go) return;
            var point = camera.WorldToScreenPoint(TopOf(go, building));
            if (point.z <= camera.nearClipPlane || !camera.pixelRect.Contains(point)) return;
            var norm = new Vector2(point.x / Mathf.Max(1, Screen.width),
                point.y / Mathf.Max(1, Screen.height));
            if (norm.x < visibleViewport.xMin + .005f ||
                norm.x > visibleViewport.xMax - .005f ||
                norm.y < visibleViewport.yMin + .005f ||
                norm.y > visibleViewport.yMax - .005f)
                return;
            // Prefer the real resource/node identity but prevent overlapping
            // labels from turning the mobile world into a wall of text.
            int cellX = Mathf.FloorToInt(point.x / 29f);
            int cellY = Mathf.FloorToInt(point.y / 29f);
            long cell = ((long)cellX << 32) | (uint)cellY;
            if (!occupiedScreenCells.Add(cell)) return;
            if (!markers.TryGetValue(id, out var marker))
            {
                marker = CreateMarker(id, icon.HasValue, building);
                markers.Add(id, marker);
            }
            marker.Touched = true;
            marker.Rect.gameObject.SetActive(true);
            // Keep the 21px icon/15px label legible on narrow phones even
            // when a 1200px reference Canvas is scaled down to 390px.
            var canvasScale = layer.GetComponentInParent<Canvas>();
            marker.Rect.localScale = Vector3.one /
                Mathf.Max(.1f, canvasScale ? canvasScale.scaleFactor : 1f);
            if (marker.Text && marker.Text.text != text) marker.Text.text = text;
            if (marker.Icon)
            {
                marker.Icon.gameObject.SetActive(icon.HasValue);
                if (icon.HasValue) marker.Icon.Kind = icon.Value;
            }
            var canvas = layer.GetComponentInParent<Canvas>();
            var uiCam = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                layer, point, uiCam, out var local))
                marker.Rect.anchoredPosition = local + (building ? new Vector2(24f, -2f) : Vector2.zero);
        }

        private Marker CreateMarker(string id, bool icon, bool building)
        {
            var root = new GameObject("Clarity / " + id, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)root.transform;
            rect.SetParent(layer, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = new Vector2(icon ? 26f : 67f, 25f);
            var background = root.GetComponent<Image>();
            background.color = new Color(.09f, .105f, .10f, .86f);
            background.raycastTarget = false;
            var rim = root.AddComponent<Outline>();
            rim.effectColor = building
                ? new Color(.74f, .55f, .29f, .74f)
                : new Color(.53f, .49f, .40f, .69f);
            rim.effectDistance = new Vector2(1f, -1f);
            var marker = new Marker { Rect = rect, Id = id };
            if (icon)
            {
                var iconGo = new GameObject("Readable resource silhouette",
                    typeof(RectTransform), typeof(RoyalResourcePictogram));
                var ir = (RectTransform)iconGo.transform;
                ir.SetParent(rect, false);
                ir.anchorMin = ir.anchorMax = ir.pivot = new Vector2(.5f, .5f);
                ir.sizeDelta = new Vector2(21f, 21f);
                marker.Icon = iconGo.GetComponent<RoyalResourcePictogram>();
                marker.Icon.color = Color.white;
                marker.Icon.raycastTarget = false;
            }
            else
            {
                var labelGo = new GameObject(building ? "Building level" : "Actual threat level",
                    typeof(RectTransform), typeof(Text));
                var t = (RectTransform)labelGo.transform;
                t.SetParent(rect, false);
                t.anchorMin = Vector2.zero;
                t.anchorMax = Vector2.one;
                t.offsetMin = t.offsetMax = Vector2.zero;
                marker.Text = labelGo.GetComponent<Text>();
                marker.Text.font = Resources.Load<Font>("EldoriaRetention/reference-serif");
                if (!marker.Text.font)
                    marker.Text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                marker.Text.fontSize = 15;
                marker.Text.alignment = TextAnchor.MiddleCenter;
                marker.Text.color = building
                    ? new Color(.97f, .80f, .55f) : new Color(1f, .85f, .75f);
                marker.Text.raycastTarget = false;
            }
            return marker;
        }

        private void HideAll()
        {
            foreach (var marker in markers.Values)
                if (marker.Rect) marker.Rect.gameObject.SetActive(false);
        }
    }
}
