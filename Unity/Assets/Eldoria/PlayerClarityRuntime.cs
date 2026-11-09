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
            public float LastSizeMultiplier;
            public Transform Target;
            public Camera Camera;
            public Vector3 AnchorOffset;
            public bool Building;
        }

        private static readonly TimeSpan RenewableDelay = RenewableRespawnRules.Delay;
        private readonly Dictionary<string, Marker> markers = new Dictionary<string, Marker>();
        private readonly Dictionary<string, WorldHotspot> hotspots = new Dictionary<string, WorldHotspot>();
        private readonly List<Rect> occupiedScreenRects = new List<Rect>();
        private readonly PlayerClarityStoryRenewal storyRenewal = new PlayerClarityStoryRenewal();
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

        private void OnEnable() => Canvas.willRenderCanvases += RefreshMarkerPositions;
        private void OnDisable() => Canvas.willRenderCanvases -= RefreshMarkerPositions;

        private void RefreshMarkerPositions()
        {
            if (!layer) return;
            var canvas = layer.GetComponentInParent<Canvas>();
            var uiCamera = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            foreach (var marker in markers.Values)
            {
                if (!marker.Rect || !marker.Rect.gameObject.activeSelf || !marker.Target || !marker.Camera) continue;
                var point = marker.Camera.WorldToScreenPoint(marker.Target.position + marker.AnchorOffset);
                if (marker.Building) point.y -= 32f;
                if (point.z <= marker.Camera.nearClipPlane || !marker.Camera.pixelRect.Contains(point))
                { marker.Rect.gameObject.SetActive(false); continue; }
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(layer, point, uiCamera, out var local))
                {
                    local += marker.Building ? new Vector2(24f, -2f) : Vector2.zero;
                    marker.Rect.localPosition = new Vector3(local.x, local.y, 0f);
                }
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
        }

        // The real integrated Unity evidence pipeline changes targetTexture
        // sizes during its two orientation captures. Recompute exactly the
        // same live markers after camera/Canvas geometry is rebuilt, rather
        // than asserting visibility from a screenshot with stale GameView UI.
        public void RefreshForCapture()
        {
            if (!isActiveAndEnabled) return;
            nextUpdate = 0f;
            LateUpdate();
        }

        private void LateUpdate()
        {
            // Reproject cached labels every frame; keep world/state scans at 5 Hz.
            RefreshMarkerPositions();
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
            occupiedScreenRects.Clear();

            var snapshot = host.City.Gateway.Snapshot();
            // Relocate an original Chapter III hunt hotspot only after the
            // authoritative UTC regeneration increments its saved generation.
            storyRenewal.Sync(host, snapshot);
            bool modal = hud.Modal && hud.Modal.gameObject.activeSelf;
            var management = hud.GetComponent<RoyalGoldV3ManagementScreens>();
            var retention = hud.GetComponent<RoyalGoldV3EarlyRetentionScreens>();
            var mailbox = hud.GetComponent<RoyalGoldV41MailboxScreen>();
            var building = hud.GetComponent<RoyalGoldV3BuildingScreens>();
            var heroes = hud.GetComponent<Eldoria.HeroesUI.HeroesApprovedDestination>();
            var naming = hud.GetComponent<RoyalPlayerEntryCoordinator>();
            bool obstructed = modal || (management && management.IsOpen) ||
                (retention && retention.IsOpen) || (mailbox && mailbox.IsOpen) ||
                (building && building.IsOpen) || (heroes && heroes.IsOpen) ||
                (naming && naming.IsBlockingNarrative);
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
            // The canonical ValoriaIntegratedWorld.BindSession now owns the
            // first-entry coordinator and rebinds its gateway on save reset.
            // Do not initialize it a second time from the map-label adapter.
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
                // Until the schema-1 Level field is integrated, capacity
                // is already authoritative and permits a truthful legacy L1.
                int level = resource.Level > 0 ? resource.Level :
                    ResourceNodeTierPolicy.ExistingSaveLevel(
                        resource.ResourceKind, resource.Capacity);
                Place(resource.TargetId, "Nv." + level, kind, hot, camera, false, level);
            }
            foreach (var id in new[] {"forest-valoria", "quarry-valoria"})
            {
                if (LocalGateway.ResourceRemaining(s, id) <= 0 ||
                    !TryHotspot(id, out var hot) || !hot.gameObject.activeInHierarchy) continue;
                Place(id, "Nv.1", id == "forest-valoria"
                    ? ResourcePictogramKind.Wood : ResourcePictogramKind.Stone,
                    hot, camera, false, 1);
            }
            // The meat reserve is the third real resource category in the
            // Bastion III frontier. Use the same food pictogram as the HUD.
            // Its actual remaining amount and unlock gate come from PlayerState.
            if (s.BastionLevel >= 3 && s.ChapterThree != null &&
                s.ChapterThree.MeatRemaining > 0 &&
                TryHotspot("meat-valoria", out var meat) &&
                meat.gameObject.activeInHierarchy)
                Place("meat-valoria", "Nv.1", ResourcePictogramKind.Food,
                    meat, camera, false, 1);
            // Chapter III hunt difficulty comes from the genuine power
            // threshold currently used in combat, not from a decorative
            // creature counter. Hide a defeated hunt until it returns.
            if (s.BastionLevel >= 3 && s.ChapterThree != null)
            {
                if (s.ChapterThree.WolfRespawnUtcTicks == 0 &&
                    TryHotspot("hunt-wolf", out var wolf) &&
                    wolf.gameObject.activeInHierarchy)
                    Place("hunt-wolf",
                        "Nv. " + ChapterThreeHuntDifficultyPolicy.Level("hunt-wolf"),
                        null, wolf, camera, false, 0);
                if (s.ChapterThree.BoarRespawnUtcTicks == 0 &&
                    TryHotspot("hunt-boar", out var boar) &&
                    boar.gameObject.activeInHierarchy)
                    Place("hunt-boar",
                        "Nv. " + ChapterThreeHuntDifficultyPolicy.Level("hunt-boar"),
                        null, boar, camera, false, 0);
            }
            // Unique campaign enemies are NOT part of the renewable
            // population. Show a threat tier inferred directly from the
            // combat catalog, without changing their respawn/story state.
            if (!s.ScoutDefeated &&
                TryHotspot("corrupt-scout", out var scout) &&
                scout.gameObject.activeInHierarchy)
                Place("corrupt-scout",
                    "Nv. " + CampaignTier("corrupt-scout"), null,
                    scout, camera, false, 0);
            if (s.BastionLevel >= 2 && !s.EngendroDefeated &&
                TryHotspot("engendro-valoria", out var engendro) &&
                engendro.gameObject.activeInHierarchy)
                Place("engendro-valoria",
                    "Nv. " + CampaignTier("engendro-valoria"), null,
                    engendro, camera, false, 0);
            if (s.BastionLevel >= 5 && s.ChapterThree != null &&
                (s.ChapterThree.AshStalkerRespawnUtcTicks == 0 ||
                 host.City.Clock.UtcTicks >= s.ChapterThree.AshStalkerRespawnUtcTicks) &&
                TryHotspot("ash-stalker-valoria", out var stalker) &&
                stalker.gameObject.activeInHierarchy)
                Place("ash-stalker-valoria",
                    "Nv. " + ChapterThreeHuntDifficultyPolicy.LevelForPower(3600),
                    null, stalker, camera, false, 0);
            foreach (var creature in s.WorldCreatureLocations)
            {
                if (!creature.Alive || creature.Level < 1 ||
                    !TryHotspot(creature.TargetId, out var hot) ||
                    !hot.gameObject.activeInHierarchy) continue;
                Place(creature.TargetId, "Nv. " + creature.Level, null,
                    hot, camera, false, 0);
            }
        }

        private static int CampaignTier(string targetId)
        {
            var real = SliceRules.CampaignEnemy(targetId);
            int result = 1, lowest = int.MaxValue;
            for (int i = 1; i <= 5; i++)
            {
                var known = WorldCreatureCatalog.For("spawnling", i);
                int distance = Mathf.Abs(real.Health - known.Health) +
                    3 * Mathf.Abs(real.Attack - known.Attack) +
                    2 * Mathf.Abs(real.Defense - known.Defense) +
                    3 * Mathf.Abs(real.Break - known.Break);
                if (distance < lowest) { result = i; lowest = distance; }
            }
            return result;
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

        public static Vector3 LabelAnchor(GameObject go, bool building)
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
            var anchor = LabelAnchor(go, building);
            var point = camera.WorldToScreenPoint(anchor);
            // The operation timer occupies the space above the roof. Keep
            // the level beside the upper facade, with a physical-pixel gap.
            if(building)point.y-=32f;
            if (point.z <= camera.nearClipPlane || !camera.pixelRect.Contains(point)) return;
            // The active Unity camera can target a dedicated RenderTexture
            // during native portrait/landscape evidence captures. Its pixels
            // differ from Screen.width/height in the editor, so use the real
            // camera viewport and never incorrectly cull valid node icons.
            var pixels = camera.pixelRect;
            var norm = new Vector2((point.x - pixels.xMin) / Mathf.Max(1f, pixels.width),
                (point.y - pixels.yMin) / Mathf.Max(1f, pixels.height));
            if (norm.x < visibleViewport.xMin + .005f ||
                norm.x > visibleViewport.xMax - .005f ||
                norm.y < visibleViewport.yMin + .005f ||
                norm.y > visibleViewport.yMax - .005f)
                return;
            // Prefer the real resource/node identity but prevent overlapping
            // labels from turning the mobile world into a wall of text.
            // Adjacent grid cells can still hold overlapping 72px labels.
            // Compare their actual screen footprints, including a small gap.
            var ownerCanvas = layer.GetComponentInParent<Canvas>();
            float ownerScale = ownerCanvas ? ownerCanvas.scaleFactor : 1f;
            Vector2 center = new Vector2(point.x, point.y) +
                (building ? new Vector2(24f, -2f) * ownerScale : Vector2.zero);
            var footprint = new Rect(center.x - 38.5f, center.y - 16f, 77f, 32f);
            var safePixels = Rect.MinMaxRect(
                pixels.xMin + visibleViewport.xMin * pixels.width,
                pixels.yMin + visibleViewport.yMin * pixels.height,
                pixels.xMin + visibleViewport.xMax * pixels.width,
                pixels.yMin + visibleViewport.yMax * pixels.height);
            if (!safePixels.Contains(footprint.min) || !safePixels.Contains(footprint.max))
                return;
            foreach (var occupied in occupiedScreenRects)
                if (occupied.Overlaps(footprint)) return;
            occupiedScreenRects.Add(footprint);
            if (!markers.TryGetValue(id, out var marker))
            {
                marker = CreateMarker(id, icon.HasValue, building);
                markers.Add(id, marker);
            }
            marker.Target = go.transform;
            marker.Camera = camera;
            marker.AnchorOffset = anchor - go.transform.position;
            marker.Building = building;
            marker.Touched = true;
            marker.Rect.gameObject.SetActive(true);
            // Compensate Canvas scale in logical dimensions, not Transform
            // scale. Scaling a tiny dynamic-font raster back up blurred mobile
            // labels even though their final RectTransform size was correct.
            var canvas = layer.GetComponentInParent<Canvas>();
            float multiplier = 1f / Mathf.Max(.1f, canvas ? canvas.scaleFactor : 1f);
            marker.Rect.localScale = Vector3.one;
            if (marker.Text && marker.Text.text != text) marker.Text.text = text;
            if (marker.Icon)
            {
                marker.Icon.gameObject.SetActive(icon.HasValue);
                if (icon.HasValue) marker.Icon.Kind = icon.Value;
            }
            if (!Mathf.Approximately(marker.LastSizeMultiplier, multiplier))
            {
                marker.LastSizeMultiplier = multiplier;
                marker.Rect.sizeDelta = new Vector2(marker.Icon ? 72f : 73f, 28f) * multiplier;
                if (marker.Icon)
                    ((RectTransform)marker.Icon.transform).sizeDelta = Vector2.one * 23f * multiplier;
                if (marker.Text)
                    marker.Text.fontSize = Mathf.RoundToInt((marker.Icon ? 14f : 15f) * multiplier);
            }
            var uiCam = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                layer, point, uiCam, out var local))
            {
                local += building ? new Vector2(24f, -2f) : Vector2.zero;
                marker.Rect.localPosition = new Vector3(local.x, local.y, 0f);
            }
        }

        private Marker CreateMarker(string id, bool icon, bool building)
        {
            var root = new GameObject("Clarity / " + id, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)root.transform;
            rect.SetParent(layer, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = new Vector2(icon ? 72f : 67f, 28f);
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
                ir.anchorMin = ir.anchorMax = ir.pivot = new Vector2(.22f, .5f);
                ir.sizeDelta = new Vector2(23f, 23f);
                marker.Icon = iconGo.GetComponent<RoyalResourcePictogram>();
                marker.Icon.color = Color.white;
                marker.Icon.raycastTarget = false;
                // Short tier text uses the exact same persisted capacity as
                // gathering. Keep each icon recognizable at small phone size.
                var tierGo = new GameObject("Real resource tier",
                    typeof(RectTransform), typeof(Text));
                var tr = (RectTransform)tierGo.transform;
                tr.SetParent(rect, false);
                tr.anchorMin = new Vector2(.42f, 0f);
                tr.anchorMax = new Vector2(.99f, 1f);
                tr.offsetMin = tr.offsetMax = Vector2.zero;
                marker.Text = tierGo.GetComponent<Text>();
                marker.Text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                marker.Text.fontSize = 14;
                marker.Text.alignment = TextAnchor.MiddleCenter;
                marker.Text.color = new Color(.97f,.82f,.57f);
                marker.Text.raycastTarget = false;
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

