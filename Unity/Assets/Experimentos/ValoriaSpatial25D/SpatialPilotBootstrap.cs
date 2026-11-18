using System;
using System.Collections.Generic;
using Eldoria.Experimentos.Ciudad2D;
using UnityEngine;

namespace Eldoria.Experimentos.ValoriaSpatial25D
{
    /// <summary>
    /// Reversible, presentation-only spatial pilot for the archived Premium V6 scene.
    /// It never reads or writes saves, alters buildings or executes gameplay commands.
    /// The control A has no component of this type; only sandbox B does.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class SpatialPilotBootstrap : MonoBehaviour
    {
        [SerializeField] private Sprite occlusionTree;
        [SerializeField, Range(0.20f, 1.2f)] private float walkingSpeed = 0.46f;
        [SerializeField] private bool drawOcclusionTree = true;

        private readonly Dictionary<string, CityBuilding> owners =
            new Dictionary<string, CityBuilding>(StringComparer.Ordinal);
        private readonly List<SpriteRenderer> grounded = new List<SpriteRenderer>(40);
        private readonly List<City2DPremiumShadowGate> shadows =
            new List<City2DPremiumShadowGate>(4);
        private readonly List<SpatialPilotWorker> actors = new List<SpatialPilotWorker>(3);
        private Transform generated;
        private SpriteRenderer testTree;
        [SerializeField] private TextAsset foregroundLayout;
        [SerializeField] private Shader foregroundShader;
        [SerializeField] private bool drawGroundForeground = true;
        private SpatialTerrainForeground foreground;
        public int ForegroundRegionCount => foreground != null ? foreground.RegionCount : 0;
        private bool ready;
        private float elapsed;

        public int WorkerCount => actors.Count;
        public bool IsReady => ready;
        public SpriteRenderer OcclusionTree => testTree;

        private void OnEnable() { ready = false; elapsed = 0f; }

        private void Update()
        {
            if (!ready) EnsureBuilt();
            if (!ready) return;

            if (UnityEngine.Application.isPlaying)
                elapsed += Time.deltaTime;
            // In edit mode choose an informative, but explicitly static pose.
            SampleAt(UnityEngine.Application.isPlaying ? elapsed : 3.6f);
        }

        public void EnsureBuilt()
        {
            if (ready || !gameObject.scene.IsValid() || !gameObject.scene.isLoaded) return;
            owners.Clear();
            grounded.Clear();
            shadows.Clear();
            actors.Clear();

            foreach (var owner in FindObjectsByType<CityBuilding>(FindObjectsSortMode.None))
                if (owner != null && owner.gameObject.scene == gameObject.scene)
                    owners[owner.BuildingId] = owner;

            // Fail closed until the archived scene has finished deserializing.
            if (owners.Count != 6 || !owners.ContainsKey("bastion") ||
                !owners.ContainsKey("cuartel") || !owners.ContainsKey("aserradero") ||
                !owners.ContainsKey("granero") || !owners.ContainsKey("cantera"))
                return;

            foreach (var root in gameObject.scene.GetRootGameObjects())
            foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (renderer == null) continue;
                var name = renderer.gameObject.name;
                if (name.StartsWith("Atrezzo_", StringComparison.Ordinal) ||
                    renderer.GetComponent<CityBuilding>() != null)
                    grounded.Add(renderer);
            }

            foreach (var gate in FindObjectsByType<City2DPremiumShadowGate>(FindObjectsSortMode.None))
                if (gate != null && gate.gameObject.scene == gameObject.scene)
                    shadows.Add(gate);

            var rootObject = new GameObject("B_ObjetosEspaciales_VisualOnly_Runtime");
            rootObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            rootObject.transform.SetParent(transform, false);
            generated = rootObject.transform;

            // Duplicate exact painted-wall texels in separate depth slots.
            // This is an occlusion pilot, not a new ground illustration.
            if (drawGroundForeground)
            {
                SpriteRenderer ground = null;
                foreach (var candidate in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
                    if (candidate.gameObject.scene == gameObject.scene &&
                        candidate.gameObject.name == "Terreno_Valoria")
                    {
                        ground = candidate;
                        break;
                    }
                if (ground == null)
                    throw new InvalidOperationException("V6 ground sprite missing");
                var cutout = new GameObject("B_PaintedWallForeground_OcclusionOnly");
                cutout.hideFlags = HideFlags.DontSave;
                cutout.transform.SetParent(generated, false);
                foreground = cutout.AddComponent<SpatialTerrainForeground>();
                foreground.Configure(ground, foregroundLayout, foregroundShader);
                if (foreground.RegionCount != 7)
                    throw new InvalidOperationException("Seven aligned wall regions expected");
            }

            // One independent foreground object, not another background photograph.
            // It must share world coordinates with actor feet and building pivots.
            if (drawOcclusionTree && occlusionTree != null)
            {
                var sawmill = owners["aserradero"];
                float radius = HalfWidth(sawmill) + 0.47f;
                var tree = new GameObject("B_ArbolOclusion_Anclado");
                tree.transform.SetParent(generated, false);
                tree.transform.position = sawmill.transform.position +
                    new Vector3(-radius, -0.12f, 0f);
                tree.transform.localScale = Vector3.one * 0.23f;
                testTree = tree.AddComponent<SpriteRenderer>();
                testTree.sprite = occlusionTree;
                testTree.sortingOrder = FootOrder(tree.transform.position.y);
                // No collider: this tree cannot intercept selection or change the gateway.
            }

            CreateWorker("aserradero", SpatialPilotWorker.Role.Woodcutter, -1f, 0.0f);
            CreateWorker("cuartel", SpatialPilotWorker.Role.Recruit, 1f, 1.6f);
            CreateWorker("granero", SpatialPilotWorker.Role.Porter, 1f, 2.8f);
            ready = actors.Count == 3;
        }

        private void CreateWorker(string id, SpatialPilotWorker.Role role, float side, float phase)
        {
            var owner = owners[id];
            float radius = HalfWidth(owner) + 0.47f;
            float x = radius * side;
            // U-shaped route at the OUTSIDE of the footprint: never crosses its center.
            // Its top leg passes behind the facade; its bottom leg passes in front.
            var waypoints = new[] {
                new Vector2(x, -0.58f), new Vector2(x + side * .22f, -0.52f),
                new Vector2(x + side * .22f, -0.10f),
                new Vector2(x + side * .22f,  0.28f),
                new Vector2(x, 0.36f), new Vector2(x + side * .22f, -0.10f)
            };
            var go = new GameObject("B_Figurante_" + id);
            go.transform.SetParent(generated, false);
            var worker = go.AddComponent<SpatialPilotWorker>();
            worker.Configure(owner, role, waypoints, walkingSpeed, phase);
            actors.Add(worker);
        }

        private static float HalfWidth(CityBuilding b)
        {
            var r = b.GetComponent<SpriteRenderer>();
            if (r == null || r.sprite == null) return 0.75f;
            return Mathf.Max(0.45f,
                r.sprite.bounds.extents.x * Mathf.Abs(b.transform.lossyScale.x));
        }

        public static int FootOrder(float worldFootY)
        {
            return Mathf.Clamp(-Mathf.RoundToInt(worldFootY * 100f), -8500, 8500);
        }

        /// <summary>
        /// Exact scene-space samples used by Editor RenderTexture A/B proof.
        /// Real gameplay uses Time.deltaTime, not scripted screenshot scrolling.
        /// </summary>
        public void SampleAt(float seconds)
        {
            if (!ready) EnsureBuilt();
            if (!ready) return;

            foreach (var render in grounded)
            {
                if (render == null) continue;
                // Original sprite importers use BottomCenter; world position is the foot.
                // No extra offset or artificial parallax for co-planar city objects.
                render.sortingOrder = FootOrder(render.transform.position.y);
            }

            foreach (var shade in shadows)
            {
                if (shade == null || shade.Owner == null) continue;
                var main = shade.Owner.GetComponent<SpriteRenderer>();
                var source = shade.GetComponent<SpriteRenderer>();
                if (main != null && source != null)
                    source.sortingOrder = main.sortingOrder - 1;
            }

            if (testTree != null)
                testTree.sortingOrder = FootOrder(testTree.transform.position.y);

            foreach (var worker in actors)
                if (worker != null) worker.SampleAt(seconds);
        }

        public SpatialPilotWorker WorkerFor(string id)
        {
            foreach (var actor in actors)
                if (actor != null && actor.Owner != null && actor.Owner.BuildingId == id)
                    return actor;
            return null;
        }

        private void OnDisable()
        {
            ready = false;
            if (generated == null) return;
            var go = generated.gameObject;
            generated = null;
            testTree = null;
            foreground = null;
            actors.Clear();
            if (UnityEngine.Application.isPlaying) Destroy(go);
            else DestroyImmediate(go);
        }
    }

}
