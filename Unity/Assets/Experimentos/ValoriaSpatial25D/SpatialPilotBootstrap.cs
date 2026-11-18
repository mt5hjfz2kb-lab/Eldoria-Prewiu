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

            if (Application.isPlaying)
                elapsed += Time.deltaTime;
            // In edit mode choose an informative, but explicitly static pose.
            SampleAt(Application.isPlaying ? elapsed : 3.6f);
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
            actors.Clear();
            if (Application.isPlaying) Destroy(go);
            else DestroyImmediate(go);
        }
    }

    /// <summary>Deterministic micro-activity, no pathfinding/gateway/economy/persistence.</summary>
    [DisallowMultipleComponent]
    public sealed class SpatialPilotWorker : MonoBehaviour
    {
        public enum Role { Woodcutter, Recruit, Porter }
        private CityBuilding owner;
        private Vector2[] waypoints;
        private Role role;
        private float speed, phase;
        private SpriteRenderer image, contact;
        public CityBuilding Owner => owner;
        public SpriteRenderer Image => image;
        public bool IsVisible => image != null && image.enabled;

        public void Configure(CityBuilding parent, Role occupation,
                              Vector2[] path, float unitsPerSecond, float phaseSeconds)
        {
            owner = parent;
            role = occupation;
            waypoints = path;
            speed = Mathf.Max(0.10f, unitsPerSecond);
            phase = phaseSeconds;
            image = gameObject.AddComponent<SpriteRenderer>();
            image.sprite = SpatialPilotActorArt.Frame(role, 0);
            transform.localScale = Vector3.one * .85f;
            var shade = new GameObject("SombraContacto_Figurante");
            shade.transform.SetParent(transform, false);
            shade.transform.localPosition = new Vector3(0f, 0f, .01f);
            contact = shade.AddComponent<SpriteRenderer>();
            contact.sprite = SpatialPilotActorArt.Shadow;
            contact.color = new Color(1f, 1f, 1f, .48f);
            SampleAt(0f);
        }

        public void SampleAt(float seconds)
        {
            if (image == null || contact == null || owner == null ||
                waypoints == null || waypoints.Length < 2) return;
            bool built = owner.Level > 0 && owner.PlotPhase == City2DPlotPhase.Built;
            image.enabled = contact.enabled = built;
            if (!built) return;

            // Constant-distance segment traversal and a short dwell for each task.
            float t = Mathf.Max(0f, seconds + phase) * speed;
            float perimeter = 0f;
            for (int i = 0; i < waypoints.Length; ++i)
                perimeter += Vector2.Distance(waypoints[i],
                    waypoints[(i + 1) % waypoints.Length]) + .16f;
            if (perimeter <= .01f) return;
            t = Mathf.Repeat(t, perimeter);

            Vector2 local = waypoints[0], direction = Vector2.right;
            bool working = false;
            for (int i = 0; i < waypoints.Length; ++i)
            {
                Vector2 from = waypoints[i];
                Vector2 to = waypoints[(i + 1) % waypoints.Length];
                float distance = Vector2.Distance(from, to);
                if (t <= distance)
                {
                    float u = distance > .0001f ? Mathf.Clamp01(t / distance) : 0f;
                    local = Vector2.Lerp(from, to, u);
                    direction = to - from;
                    break;
                }
                t -= distance;
                if (t <= .16f)
                {
                    local = to;
                    direction = to - from;
                    working = true;
                    break;
                }
                t -= .16f;
            }

            Vector3 p = owner.transform.position;
            transform.position = new Vector3(p.x + local.x, p.y + local.y, -.045f);
            // Visible legs/arms change with movement; stable pose during activity.
            int pose = working ? 2 : ((Mathf.FloorToInt(seconds * 3f + phase) & 1) == 0 ? 0 : 1);
            image.sprite = SpatialPilotActorArt.Frame(role, pose);
            image.flipX = direction.x < -.02f;
            image.sortingOrder = SpatialPilotBootstrap.FootOrder(transform.position.y) + 1;
            contact.sortingOrder = image.sortingOrder - 2;
        }
    }

    /// <summary>
    /// Reproducible small silhouette PROXIES for evaluating occlusion and scale.
    /// These are explicitly NOT artist-approved character art; replace them after
    /// spatial A/B has been checked in actual Unity. No sourced sprite copied.
    /// </summary>
    internal static class SpatialPilotActorArt
    {
        private static readonly Dictionary<int, Sprite> frames = new Dictionary<int, Sprite>();
        private static Sprite shadow;
        private const int W = 48, H = 72;
        public static Sprite Shadow
        {
            get
            {
                if (shadow != null) return shadow;
                var tex = new Texture2D(48, 20, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Bilinear;
                var pixels = new Color32[48 * 20];
                for (int y = 0; y < 20; y++)
                for (int x = 0; x < 48; x++)
                {
                    float d = Mathf.Pow((x - 23.5f) / 22f, 2f) +
                              Mathf.Pow((y - 9.5f) / 7f, 2f);
                    pixels[y * 48 + x] = new Color32(35, 27, 55,
                        (byte)Mathf.RoundToInt(Mathf.Clamp01(1f - d) * 115f));
                }
                tex.SetPixels32(pixels);
                tex.Apply(false, true);
                tex.hideFlags = HideFlags.HideAndDontSave;
                shadow = Sprite.Create(tex, new Rect(0, 0, 48, 20),
                    new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect);
                shadow.hideFlags = HideFlags.HideAndDontSave;
                return shadow;
            }
        }

        public static Sprite Frame(SpatialPilotWorker.Role role, int pose)
        {
            int key = (int)role * 3 + Mathf.Clamp(pose, 0, 2);
            if (frames.TryGetValue(key, out var cached) && cached != null) return cached;
            Color32[] pixels = new Color32[W * H];
            Color32 outline = new Color32(41, 35, 49, 255);
            Color32 skin = new Color32(196, 143, 100, 255);
            Color32 boot = new Color32(67, 45, 45, 255);
            Color32 coat = role == SpatialPilotWorker.Role.Recruit
                ? new Color32(57, 78, 122, 255)
                : role == SpatialPilotWorker.Role.Woodcutter
                    ? new Color32(111, 76, 47, 255)
                    : new Color32(84, 96, 63, 255);
            Color32 light = role == SpatialPilotWorker.Role.Recruit
                ? new Color32(100, 124, 163, 255)
                : role == SpatialPilotWorker.Role.Woodcutter
                    ? new Color32(171, 118, 64, 255)
                    : new Color32(140, 148, 94, 255);

            int stride = pose == 0 ? -3 : pose == 1 ? 3 : 0;
            Rect(pixels, 14 + stride, 5, 22 + stride, 19, outline);
            Rect(pixels, 25 - stride, 5, 33 - stride, 19, outline);
            Rect(pixels, 15 + stride, 12, 21 + stride, 25, boot);
            Rect(pixels, 26 - stride, 12, 32 - stride, 25, boot);
            Rect(pixels, 13, 25, 35, 48, outline);
            Rect(pixels, 16, 28, 32, 46, coat);
            Rect(pixels, 17, 33, 23, 46, light);
            Rect(pixels, 15, 29, 33, 33, new Color32(79, 54, 42, 255));
            int armSwing = pose == 0 ? -4 : pose == 1 ? 4 : -2;
            Rect(pixels, 9 + armSwing, 28, 15 + armSwing, 42, coat);
            Rect(pixels, 34 - armSwing, 28, 40 - armSwing, 42, coat);
            Ellipse(pixels, 11 + armSwing, 28, 3, 4, skin);
            Ellipse(pixels, 37 - armSwing, 28, 3, 4, skin);
            Ellipse(pixels, 24, 55, 11, 12, outline);
            Ellipse(pixels, 24, 55, 8, 9, skin);
            Rect(pixels, 16, 62, 32, 65, boot);
            Rect(pixels, 17, 64, 31, 67, light);
            // Each activity has a recognizable prop, but no UI icon or combat action.
            if (role == SpatialPilotWorker.Role.Woodcutter)
            {
                Rect(pixels, 38, 23, 40, 55, new Color32(128, 85, 53, 255));
                Rect(pixels, 35, 53, 44, 59, new Color32(141, 148, 160, 255));
            }
            else if (role == SpatialPilotWorker.Role.Recruit)
            {
                Rect(pixels, 39, 23, 41, 66, new Color32(134, 103, 72, 255));
                Rect(pixels, 37, 61, 43, 65, new Color32(160, 167, 176, 255));
            }
            else
            {
                Rect(pixels, 35, 20, 45, 33, new Color32(125, 83, 49, 255));
                Rect(pixels, 37, 25, 43, 30, new Color32(199, 158, 88, 255));
            }
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            tex.hideFlags = HideFlags.HideAndDontSave;
            var sprite = Sprite.Create(tex, new Rect(0, 0, W, H),
                new Vector2(.5f, .04f), 100f, 0, SpriteMeshType.FullRect);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            frames[key] = sprite;
            return sprite;
        }

        private static void Rect(Color32[] p, int x0, int y0, int x1, int y1, Color32 color)
        {
            for (int y = Mathf.Max(0, y0); y <= Mathf.Min(H - 1, y1); y++)
            for (int x = Mathf.Max(0, x0); x <= Mathf.Min(W - 1, x1); x++)
                p[y * W + x] = color;
        }
        private static void Ellipse(Color32[] p, int cx, int cy, int rx, int ry, Color32 color)
        {
            for (int y = Mathf.Max(0, cy - ry); y <= Mathf.Min(H - 1, cy + ry); y++)
            for (int x = Mathf.Max(0, cx - rx); x <= Mathf.Min(W - 1, cx + rx); x++)
            {
                float dx = (x - cx) / (float)Mathf.Max(1, rx);
                float dy = (y - cy) / (float)Mathf.Max(1, ry);
                if (dx * dx + dy * dy <= 1f) p[y * W + x] = color;
            }
        }
    }
}
