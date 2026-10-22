using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Eldoria.ReferenceClean;
using Eldoria.World4XPrototype;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Eldoria.World4XCanon
{
    /// <summary>
    /// Presentation-only Region 1 extension for the *integrated* Valoria world.
    /// Never makes WorldHotspots, commands, simulated cities, opponents or save data.
    /// Installed at scene load rather than editing the UI/Spawnling-owned integration scene.
    /// All three distances observe the same textured, deterministic mesh.
    /// </summary>
    public sealed class RegionOneCanonicalLandscape : MonoBehaviour
    {
        public const int LandscapeVersion = 1;
        // The strategic Region 1 fits in a landscape panorama. The much wider
        // textured continuation masks the outer bounds without forming an island.
        public const float CoreHalfWidth = 425f;
        public const float CoreHalfDepth = 260f;
        public const float LandscapeHalfWidth = 910f;
        public const float LandscapeHalfDepth = 780f;
        public const float BreachOffsetX = 18f;
        public const float BreachOffsetZ = 44f;
        public const float LocalThreshold = 41f;
        public const float RegionalThreshold = 152f;

        static readonly Color Grass = new Color(.51f, .52f, .30f);
        static readonly Color DryGrass = new Color(.62f, .56f, .33f);
        static readonly Color DarkForest = new Color(.25f, .39f, .23f);
        static readonly Color Road = new Color(.67f, .54f, .32f);
        static readonly Color Rock = new Color(.52f, .49f, .40f);
        static readonly Color Water = new Color(.19f, .43f, .58f);
        static readonly Color Rift = new Color(.21f, .15f, .22f);
        static readonly Vector2[] ForestCentres = {
            new Vector2(-310,230),new Vector2(-180,165),new Vector2(-390,-100),
            new Vector2(-235,-220),new Vector2(-105,-310),new Vector2(-25,230),
            new Vector2(130,270),new Vector2(325,210),new Vector2(385,55),
            new Vector2(270,-125),new Vector2(100,-240),new Vector2(-170,390),
            new Vector2(190,385),new Vector2(-410,340),new Vector2(-355,-340),
            new Vector2(385,-330),new Vector2(-65,465),new Vector2(340,440),
            // Scale-specific regional and near-horizon vegetation: keep the
            // gameplay corridor clear; don't populate its selectable centre.
            new Vector2(-115,65),new Vector2(105,80),new Vector2(-125,-75),
            new Vector2(118,-92),new Vector2(-20,125)
        };

        ValoriaIntegratedWorld host;
        ZoneOneStrategicCamera navigation;
        Vector3 origin;
        GameObject detailTrees;
        GameObject peripheralFog;
        readonly List<Renderer> originalGround = new List<Renderer>();
        readonly List<bool> originalGroundInitiallyEnabled = new List<bool>();
        GameObject distantForestSilhouettes;
        Mesh terrainMesh;
        Texture2D terrainTexture;
        Material surfaceMaterial, riftMaterial, fogMaterial;
        readonly List<Mesh> generatedMeshes = new List<Mesh>();
        bool built, farMode, fogMode;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            // AfterSceneLoad alone runs once per application, not on later
            // scene transitions. The real player can enter Valoria after a
            // bootstrap scene, and PlayMode tests also LoadScene dynamically.
            SceneManager.sceneLoaded -= WhenSceneLoaded;
            SceneManager.sceneLoaded += WhenSceneLoaded;
            AttachToCurrentIntegratedScene();
        }

        static void WhenSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            AttachToCurrentIntegratedScene();
        }

        static void AttachToCurrentIntegratedScene()
        {
            var host = UnityEngine.Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
            if (!host || UnityEngine.Object.FindFirstObjectByType<RegionOneCanonicalLandscape>()) return;
            var adapter = new GameObject("Region 1 canonical world · presentation adapter")
                .AddComponent<RegionOneCanonicalLandscape>();
            adapter.host = host;
        }

        IEnumerator Start()
        {
            if (!host) yield break;
            while (host && !host.Ready) yield return null;
            if (!host || !host.WorldRoot || !host.WorldNavigation || !host.WorldCamera) yield break;
            navigation = host.WorldNavigation;
            origin = host.WorldHome;
            if (host.WorldRoot.transform.Find("Region 1 canonical continuous geography"))
            {
                Debug.LogWarning("REGION1_CANON_LAYER_ALREADY_PRESENT");
                yield break;
            }
            InstallCameraLimits();
            Build(host.WorldRoot.transform);
            built = true;
            UpdateLevelOfDetail(true);
            Debug.Log("REGION1_CANON_WORLD_INSTALLED presentation_only=1 geography=1 save_changes=0");
        }

        void InstallCameraLimits()
        {
            // Bounds enclose a larger real terrain mesh, not a zoom onto empty sky.
            // The current integration sets regionalZoom=32 and localZoom=14 in Start.
            navigation.localZoom = 17f;
            navigation.regionalZoom = 84f;
            navigation.globalZoom = 244f;
            navigation.tilt = 66f;
            navigation.cameraDistance = Mathf.Max(navigation.cameraDistance, 540f);
            navigation.minimum = new Vector2(origin.x - 600f, origin.z - 535f);
            navigation.maximum = new Vector2(origin.x + 600f, origin.z + 535f);
            navigation.edgeMargin = 12f;
            navigation.FitToAuthoredBounds();
        }

        void Build(Transform existingWorld)
        {
            var root = new GameObject("Region 1 canonical continuous geography").transform;
            root.SetParent(existingWorld, true);
            // Existing hotspot transforms remain the only gameplay coordinate authority.
            surfaceMaterial = MakeMaterial("Region1 ground texture", Color.white, false);
            terrainTexture = MakeTerrainTexture(1024);
            if (surfaceMaterial.HasProperty("_BaseMap")) surfaceMaterial.SetTexture("_BaseMap", terrainTexture);
            if (surfaceMaterial.HasProperty("_MainTex")) surfaceMaterial.SetTexture("_MainTex", terrainTexture);
            var land = new GameObject("Region1 · one coherent terrain mesh");
            land.transform.SetParent(root, true);
            var filter = land.AddComponent<MeshFilter>();
            terrainMesh = MakeLandscapeMesh(80);
            filter.sharedMesh = terrainMesh;
            var renderer = land.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = surfaceMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            // Preserve all original hotspots/colliders. Render-only LOD turns off
            // the *legacy prairie sheet* at panorama scale, otherwise that
            // square visually cuts the same Region 1 into two maps.
            CollectLegacyGround(existingWorld);
            // No terrain collider: world node selection continues to hit original WorldHotspots.
            BuildBreach(root);
            BuildTreeDetail(root, existingWorld);
            BuildDistantForests(root);
            BuildEdgeFog(root);
        }

        void CollectLegacyGround(Transform existingWorld)
        {
            foreach (var renderer in existingWorld.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!renderer || renderer.transform.IsChildOf(transform)) continue;
                string name = renderer.name.ToLowerInvariant();
                bool isPrairie = name.Contains("continuous prairie") ||
                    (name.Contains("ground") && name.Contains("zoneone"));
                // Do not infer that the city, quarry, real trees or enemy rocks
                // are background; only the positively identified prairie.
                if (!isPrairie || renderer.bounds.size.x < 100f ||
                                  renderer.bounds.size.z < 100f) continue;
                originalGround.Add(renderer);
                originalGroundInitiallyEnabled.Add(renderer.enabled);
            }
            Debug.Log("REGION1_CANON_LEGACY_PRAIRIE_RENDERERS=" + originalGround.Count);
        }

        Mesh MakeLandscapeMesh(int resolution)
        {
            int row = resolution + 1;
            var vertices = new Vector3[row * row];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[resolution * resolution * 6];
            for (int z = 0; z <= resolution; z++)
                for (int x = 0; x <= resolution; x++)
                {
                    float u = x / (float)resolution, v = z / (float)resolution;
                    float localX = (u * 2 - 1) * LandscapeHalfWidth;
                    float localZ = (v * 2 - 1) * LandscapeHalfDepth;
                    int index = z * row + x;
                    vertices[index] = new Vector3(origin.x + localX, SampleHeight(localX, localZ), origin.z + localZ);
                    uv[index] = new Vector2(u, v);
                }
            int t = 0;
            for (int z = 0; z < resolution; z++)
                for (int x = 0; x < resolution; x++)
                {
                    int i = z * row + x;
                    triangles[t++] = i; triangles[t++] = i + row; triangles[t++] = i + 1;
                    triangles[t++] = i + 1; triangles[t++] = i + row; triangles[t++] = i + row + 1;
                }
            var mesh = new Mesh { name = "Region 1 single terrain + peripheral continuation",
                vertices = vertices, uv = uv, triangles = triangles };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            generatedMeshes.Add(mesh);
            return mesh;
        }

        public static float SampleHeight(float x, float z)
        {
            // Always below original near-field targets and authored 3D terrain.
            float near = Mathf.Clamp01((Mathf.Sqrt(x * x + z * z) - 62f) / 140f);
            float contour = Mathf.Sin(x * .017f) * Mathf.Cos(z * .014f) * .95f +
                            Mathf.Sin(z * .009f + x * .006f) * .55f;
            float distantRidge = Mathf.Clamp01((Mathf.Abs(x) - 330f) / 290f) * 3.5f;
            return -4.0f + near * (contour + distantRidge);
        }

        static float Noise(int x, int z)
        {
            unchecked
            {
                uint n = (uint)(x * 374761393 + z * 668265263 + 20261022);
                n = (n ^ (n >> 13)) * 1274126177u;
                return ((n ^ (n >> 16)) & 65535u) / 65535f;
            }
        }

        public static bool IsForest(float x, float z)
        {
            // Distinct, irregular authored-looking groves rather than circles
            // stamped at predictable radii across a strategic board.
            for (int i = 0; i < ForestCentres.Length; i++)
            {
                var centre = ForestCentres[i];
                float width = 24f + (i % 4) * 5.5f;
                float depth = 20f + (i % 3) * 5.8f;
                float dx = (x - centre.x) / width;
                float dz = (z - centre.y) / depth;
                float d = dx * dx + dz * dz;
                if (d >= 1.45f) continue;
                float irregularity = .19f * Mathf.Sin(x * .16f + i * 1.2f) *
                                    Mathf.Cos(z * .11f - i * .7f) +
                                    .13f * Mathf.Sin(x * .07f + z * .12f + i);
                if (d < 1f + irregularity) return true;
            }
            return false;
        }

        public static bool IsCoast(float x, float z)
        {
            // A narrow southeastern shore, never an island outline.
            float coast = 366f + 12f * Mathf.Sin(z * .032f) +
                          7f * Mathf.Sin(z * .083f);
            float southEdge = -68f - .22f * (x - 366f) +
                              10f * Mathf.Sin(x * .022f);
            return x > coast && z < southEdge;
        }

        static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / Mathf.Max(.001f, Vector2.Dot(ab, ab)));
            return Vector2.Distance(point, a + ab * t);
        }

        public static bool IsRoad(float x, float z)
        {
            // Curved road network in one shared coordinate system. The close
            // 3D route remains the authoritative interactable route surface.
            Vector2 p = new Vector2(x + 13f * Mathf.Sin(z * .017f) +
                                         5f * Mathf.Sin(z * .037f),
                                     z + 8f * Mathf.Sin(x * .013f));
            return DistanceToSegment(p, new Vector2(0,0), new Vector2(-365,310)) < 3.0f ||
                   DistanceToSegment(p, new Vector2(0,0), new Vector2(370,305)) < 3.0f ||
                   DistanceToSegment(p, new Vector2(0,0), new Vector2(-305,-370)) < 3.0f ||
                   DistanceToSegment(p, new Vector2(0,0), new Vector2(400,-262)) < 3.0f;
        }

        public static bool IsRiver(float x, float z)
        {
            float riverX = -104f + .25f * z +
                27f * Mathf.Sin(z * .016f) + 12f * Mathf.Sin(z * .042f);
            return Mathf.Abs(x - riverX) < 3.2f && z > -440f && z < 450f;
        }

        Texture2D MakeTerrainTexture(int size)
        {
            var image = new Texture2D(size, size, TextureFormat.RGB24, true, false);
            image.name = "Region1 continuous textured geography one shared space";
            var pixels = new Color32[size * size];
            Vector2 rift = new Vector2(BreachOffsetX, BreachOffsetZ);
            for (int z = 0; z < size; z++)
                for (int x = 0; x < size; x++)
                {
                    float worldX = ((x + .5f) / size * 2 - 1) * LandscapeHalfWidth;
                    float worldZ = ((z + .5f) / size * 2 - 1) * LandscapeHalfDepth;
                    // Smooth multiscale ground removes square pixel blocks.
                    float broad = Mathf.PerlinNoise(22f + worldX * .0065f, 55f + worldZ * .007f);
                    float mottling = Mathf.PerlinNoise(106f + worldX * .045f, 78f + worldZ * .05f);
                    float grain = Mathf.PerlinNoise(216f + worldX * .18f, 312f + worldZ * .17f);
                    float n = .46f * broad + .38f * mottling + .16f * grain;
                    Color c = Color.Lerp(Grass, DryGrass, Mathf.Clamp01(.10f + n * .85f));
                    // Colour continuity from the actual player-city biome to
                    // the surrounding region, without adding a separate map.
                    float radius = Mathf.Sqrt(worldX * worldX + worldZ * worldZ);
                    float meadowTransition = 1f - Mathf.SmoothStep(85f, 230f, radius);
                    c = Color.Lerp(c, new Color(.42f,.46f,.28f),
                        meadowTransition * .33f);
                    if (IsForest(worldX, worldZ))
                    {
                        float canopy = .42f + .20f * mottling;
                        c = Color.Lerp(c, DarkForest, canopy);
                        if (grain > .68f) c = Color.Lerp(c, Grass, .27f);
                    }
                    if (Mathf.Abs(worldX) > 430f && broad > .70f)
                        c = Color.Lerp(c, Rock, .12f + .12f * grain);
                    if (IsRoad(worldX, worldZ))
                        c = Color.Lerp(c, Road, .62f + .13f * grain);
                    float waterbank = .0f;
                    float riverX = -104f + .25f * worldZ +
                        27f * Mathf.Sin(worldZ * .016f) + 12f * Mathf.Sin(worldZ * .042f);
                    if (worldZ > -445f && worldZ < 455f)
                        waterbank = Mathf.Clamp01((5.4f - Mathf.Abs(worldX - riverX)) / 4f);
                    if (waterbank > 0) c = Color.Lerp(c, Water, waterbank * .91f);
                    if (IsCoast(worldX, worldZ))
                        c = Color.Lerp(Water, new Color(.25f,.48f,.61f), grain * .18f);
                    float d = Vector2.Distance(new Vector2(worldX, worldZ), rift);
                    if (d < 54f)
                    {
                        float strength = Mathf.Clamp01((54f - d) / 47f);
                        float cracks = Mathf.Abs(Mathf.Sin(
                            Mathf.Atan2(worldZ - rift.y, worldX - rift.x) * 12f + d * .035f));
                        c = Color.Lerp(c, Rift, strength * (.58f + .22f * cracks));
                        if (d < 10f) c = Color.Lerp(c, new Color(.60f,.18f,.68f),
                            Mathf.Clamp01((10f - d) / 8f) * .68f);
                    }
                    pixels[z * size + x] = c;
                }
            image.SetPixels32(pixels);
            image.Apply(true, true); // do not retain the CPU pixel array on mobile
            image.wrapMode = TextureWrapMode.Clamp;
            image.filterMode = FilterMode.Bilinear;
            return image;
        }

        void BuildBreach(Transform parent)
        {
            var fracture = new GameObject("Region1 · grounded geologic Breach (not a portal)").transform;
            fracture.SetParent(parent, true);
            riftMaterial = MakeMaterial("Region1 fracture violet", new Color(.51f, .16f, .56f), true);
            var points = new List<Vector3>();
            var indices = new List<int>();
            var centre = new Vector2(BreachOffsetX, BreachOffsetZ);
            // Thin branching cracks follow the terrain. No upright towers or enemy proxies.
            for (int branch = 0; branch < 28; branch++)
            {
                float angle = branch * Mathf.PI * 2f / 28f;
                float extent = 20f + (branch % 5) * 4.5f;
                Vector2 previous = centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 1.5f;
                for (int step = 1; step <= 3; step++)
                {
                    float distance = extent * step / 3f;
                    float jitter = Mathf.Sin((branch + 1) * step * 1.7f) * distance * .13f;
                    Vector2 next = centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance +
                                   new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle)) * jitter;
                    float halfWidth = .65f * (1f - .18f * step);
                    Vector2 perpendicular = (next - previous).normalized;
                    perpendicular = new Vector2(-perpendicular.y, perpendicular.x) * halfWidth;
                    int index = points.Count;
                    foreach (Vector2 location in new [] { previous - perpendicular, previous + perpendicular,
                                                           next - perpendicular, next + perpendicular })
                        points.Add(new Vector3(origin.x + location.x, SampleHeight(location.x, location.y) + .20f,
                                               origin.z + location.y));
                    indices.Add(index); indices.Add(index + 1); indices.Add(index + 2);
                    indices.Add(index + 1); indices.Add(index + 3); indices.Add(index + 2);
                    previous = next;
                }
            }
            var mesh = new Mesh { name = "Region1 surface fracture only", vertices = points.ToArray(), triangles = indices.ToArray() };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            generatedMeshes.Add(mesh);
            var go = new GameObject("Region1 · fracture mesh · no collider");
            go.transform.SetParent(fracture, true);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = riftMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        void BuildTreeDetail(Transform parent, Transform original)
        {
            detailTrees = new GameObject("Region1 · reusable midrange forest silhouettes");
            detailTrees.transform.SetParent(parent, true);
            // Clone ONLY shared tree meshes/materials from existing art. No scene prefabs,
            // colliders, selection or save system are duplicated.
            var donor = original.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(f =>
                f.sharedMesh != null && f.sharedMesh.vertexCount < 3500 &&
                f.GetComponent<MeshRenderer>() != null &&
                (f.name.IndexOf("tree", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 f.name.IndexOf("pine", StringComparison.OrdinalIgnoreCase) >= 0));
            if (!donor) return;
            var oldRenderer = donor.GetComponent<MeshRenderer>();
            for (int i = 0; i < ForestCentres.Length; i++)
                for (int j = 0; j < 5; j++)
                {
                    float theta = (j * 2.39f + i) * Mathf.PI;
                    var centre = ForestCentres[i];
                    float x = centre.x + Mathf.Cos(theta) * (11f + j * 6f);
                    float z = centre.y + Mathf.Sin(theta) * (9f + j * 4f);
                    var tree = new GameObject("Region1 · reused-tree " + i + "-" + j);
                    tree.transform.SetParent(detailTrees.transform, true);
                    tree.transform.position = new Vector3(origin.x + x, SampleHeight(x,z) + .2f, origin.z + z);
                    tree.transform.rotation = Quaternion.Euler(0, (i * 67 + j * 53) % 360, 0);
                    float originalHeight = Mathf.Max(.15f, donor.sharedMesh.bounds.size.y);
                    float height = 2.2f + .4f * ((i + j) % 4);
                    tree.transform.localScale = Vector3.one * Mathf.Clamp(height / originalHeight, .08f, 3.5f);
                    tree.AddComponent<MeshFilter>().sharedMesh = donor.sharedMesh;
                    var meshRenderer = tree.AddComponent<MeshRenderer>();
                    meshRenderer.sharedMaterials = oldRenderer.sharedMaterials;
                    meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                    meshRenderer.receiveShadows = false;
                }
        }

        readonly List<Material> farForestMaterials = new List<Material>();

        void BuildDistantForests(Transform parent)
        {
            distantForestSilhouettes = new GameObject("Region1 · batched distant organic forest canopies");
            distantForestSilhouettes.transform.SetParent(parent, true);
            // These are distant-scale tree silhouettes, never gameplay entities.
            // A handful of low-poly canopy batches keeps draw calls predictable,
            // unlike thousands of individual forest prefab renderers on iPhone.
            for (int band = 0; band < 3; band++)
            {
                var positions = new List<Vector3>();
                var indices = new List<int>();
                for (int cluster = band; cluster < ForestCentres.Length; cluster += 3)
                {
                    Vector2 centre = ForestCentres[cluster];
                    const int count = 18;
                    for (int j = 0; j < count; j++)
                    {
                        // Golden-angle positions form irregular groves, not stamped disks.
                        float radial = Mathf.Sqrt((j + .7f) / count);
                        float theta = j * 2.399963f + cluster * 1.37f;
                        float radiusX = 13f + (cluster % 4) * 3.2f;
                        float radiusZ = 12f + (cluster % 3) * 3.3f;
                        float x = centre.x + Mathf.Cos(theta) * radiusX * radial;
                        float z = centre.y + Mathf.Sin(theta) * radiusZ * radial;
                        if (Mathf.Sqrt(x * x + z * z) < 95f || IsRiver(x,z) || IsCoast(x,z)) continue;
                        float y = SampleHeight(x, z) + .1f;
                        float width = 1.65f + Noise(cluster * 31, j * 19) * 1.8f;
                        float height = 4.8f + Noise(j * 11, cluster * 43) * 3f;
                        // Triangular tapering spruce crown, with two independent
                        // fronds and no upright surface monument.
                        int begin = positions.Count;
                        positions.Add(new Vector3(origin.x+x, y+height, origin.z+z));
                        for (int k = 0; k < 6; k++)
                        {
                            float angle = k * Mathf.PI / 3f + .2f * cluster;
                            float radius = width * (k % 2 == 0 ? 1f : .80f);
                            positions.Add(new Vector3(origin.x+x + Mathf.Cos(angle)*radius,
                                y+1.05f, origin.z+z + Mathf.Sin(angle)*radius));
                        }
                        for (int k = 0; k < 6; k++)
                        {
                            indices.Add(begin); indices.Add(begin + 1 + (k + 1) % 6);
                            indices.Add(begin + 1 + k);
                        }
                    }
                }
                if (positions.Count == 0) continue;
                var mesh = new Mesh { name = "Region1 · distant spruce grove batch " + band,
                    vertices = positions.ToArray(), triangles = indices.ToArray() };
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                generatedMeshes.Add(mesh);
                var forest = new GameObject("Region1 · strategic tree batch " + band);
                forest.transform.SetParent(distantForestSilhouettes.transform, true);
                forest.AddComponent<MeshFilter>().sharedMesh = mesh;
                var material = MakeMaterial("Region1 strategic spruce " + band,
                    band == 0 ? new Color(.15f,.31f,.16f) :
                    band == 1 ? new Color(.22f,.36f,.17f) : new Color(.29f,.41f,.21f), false);
                farForestMaterials.Add(material);
                var renderer = forest.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.receiveShadows = false;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
            distantForestSilhouettes.SetActive(false);
        }

        void BuildEdgeFog(Transform parent)
        {
            peripheralFog = new GameObject("Region1 · panoramic border haze only");
            peripheralFog.transform.SetParent(parent, true);
            Shader shader = Shader.Find("Sprites/Default");
            if (!shader) { peripheralFog.SetActive(false); return; }
            fogMaterial = new Material(shader) { name = "Region1 peripheral alpha no-volume" };
            fogMaterial.renderQueue = 3000;
            AddFogStrip(peripheralFog.transform, -1, 0);
            AddFogStrip(peripheralFog.transform, 1, 0);
            AddFogStrip(peripheralFog.transform, 0, -1);
            AddFogStrip(peripheralFog.transform, 0, 1);
        }

        void AddFogStrip(Transform parent, int sx, int sz)
        {
            const float westEast = 425f, northSouth = 285f, thickness = 85f;
            var verts = new List<Vector3>();
            var colors = new List<Color>();
            var indices = new List<int>();
            Color baseColor = new Color(.81f, .86f, .88f, 1);
            for (int step = 0; step <= 8; step++)
            {
                float value = step / 8f;
                float distance = (sx != 0 ? westEast : northSouth) + value * thickness;
                float x = sx != 0 ? sx * distance : -LandscapeHalfWidth;
                float z = sz != 0 ? sz * distance : -LandscapeHalfDepth;
                float alpha = .22f * value * value;
                if (sx != 0)
                {
                    verts.Add(new Vector3(origin.x + x, -1f, origin.z - LandscapeHalfDepth));
                    verts.Add(new Vector3(origin.x + x, -1f, origin.z + LandscapeHalfDepth));
                }
                else
                {
                    verts.Add(new Vector3(origin.x - LandscapeHalfWidth, -1f, origin.z + z));
                    verts.Add(new Vector3(origin.x + LandscapeHalfWidth, -1f, origin.z + z));
                }
                colors.Add(new Color(baseColor.r,baseColor.g,baseColor.b,alpha));
                colors.Add(new Color(baseColor.r,baseColor.g,baseColor.b,alpha));
                if (step == 0) continue;
                int a = (step - 1) * 2, b = step * 2;
                indices.Add(a); indices.Add(b); indices.Add(a+1);
                indices.Add(a+1); indices.Add(b); indices.Add(b+1);
            }
            var mesh = new Mesh { name = "Region1 border haze strip", vertices = verts.ToArray(),
                                  colors = colors.ToArray(), triangles = indices.ToArray() };
            mesh.RecalculateBounds();
            generatedMeshes.Add(mesh);
            var go = new GameObject("Region1 · haze edge " + sx + " " + sz);
            go.transform.SetParent(parent,true);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = fogMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        static Material MakeMaterial(string name, Color color, bool emissive)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ??
                         Shader.Find("Unlit/Texture") ??
                         Shader.Find("Standard");
            var material = new Material(shader) { name = name, enableInstancing = true };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            if (emissive && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color);
            }
            return material;
        }

        // Explicitly synchronize representations before a manually rendered Unity capture.
        public void RefreshForCapture()
        {
            if (built) UpdateLevelOfDetail(true);
        }

        void LateUpdate()
        {
            if (built) UpdateLevelOfDetail(false);
        }

        void UpdateLevelOfDetail(bool force)
        {
            if (!navigation || !navigation.View) return;
            float size = navigation.View.orthographicSize;
            bool distant = size >= RegionalThreshold;
            bool showFog = size >= 190f && host.InWorld;
            if (force || distant != farMode)
            {
                farMode = distant;
                if (detailTrees) detailTrees.SetActive(!distant);
                if (distantForestSilhouettes) distantForestSilhouettes.SetActive(distant);
                for (int i = 0; i < originalGround.Count; i++)
                    if (originalGround[i])
                        originalGround[i].enabled = distant ? false : originalGroundInitiallyEnabled[i];
            }
            if (force || showFog != fogMode)
            {
                fogMode = showFog;
                if (peripheralFog) peripheralFog.SetActive(showFog);
            }
        }

        void OnDestroy()
        {
            for (int i = 0; i < originalGround.Count; i++)
                if (originalGround[i]) originalGround[i].enabled = originalGroundInitiallyEnabled[i];
            foreach (var mesh in generatedMeshes) if (mesh) Destroy(mesh);
            if (terrainTexture) Destroy(terrainTexture);
            if (surfaceMaterial) Destroy(surfaceMaterial);
            if (riftMaterial) Destroy(riftMaterial);
            if (fogMaterial) Destroy(fogMaterial);
            foreach (var mat in farForestMaterials) if (mat) Destroy(mat);
        }
    }
}
