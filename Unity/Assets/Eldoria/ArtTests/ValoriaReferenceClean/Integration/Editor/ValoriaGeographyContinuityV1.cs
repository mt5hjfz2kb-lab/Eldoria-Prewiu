#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Eldoria.Presentation;
using Eldoria.World4XPrototype;
using Object = UnityEngine.Object;

namespace Eldoria.ReferenceClean.Editor
{
    /// <summary>
    /// Same-source visual baseline and geographic authoring for the one integrated city/world scene.
    /// Camera positions, POI IDs, gameplay/save data and published artifacts are never changed.
    /// A survey is read-only. Trial and integration are separate explicit gates.
    /// </summary>
    public static class ValoriaGeographyContinuityV1
    {
        internal const string ScenePath = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        internal const string AssetFolder = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/GeographyV1";
        internal static string Evidence => Path.GetFullPath("../docs/evidence/valoria-reference-clean/geography-v1");
        [Serializable] public class Spot { public string id; public Vector3 position; public Vector3 bounds; }
        [Serializable] public class SurveyRecord
        {
            public string source_scene_sha256, tool_sha256, region_ground_name;
            public Vector3 terrain_origin, terrain_size, world_ground_min, world_ground_max;
            public int city_height_resolution, city_splat_resolution, city_layers;
            public int world_vertices, world_triangles, world_uvs, world_renderers;
            public string[] world_materials;
            public Spot[] poi;
            public float world_home_zoom, world_tilt, world_camera_distance;
            public bool preexisting_city_terrain_collider, world_ground_mesh_collider;
            public bool same_scene_bytes, camera_pose_restored, art_approved, physical_mobile_tested;
        }
        internal static void Need(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); }
        internal static string Sha(string path)
        {
            using (var h = SHA256.Create())
                return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
        }
        internal static ValoriaIntegratedWorld Load(out ValoriaReferenceCleanGameplay city)
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            city = Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();
            var host = Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
            Need(city != null && host != null && city.PlayerCamera != null && host.WorldCamera != null && host.WorldRoot != null, "Canonical city/world scene host unavailable");
            return host;
        }
        internal static MeshFilter WorldGround(ValoriaIntegratedWorld host)
        {
            var candidates = host.WorldRoot.GetComponentsInChildren<MeshFilter>(true).Where(f => f.name == "ZoneOne continuous prairie source" && f.sharedMesh != null).ToArray();
            Need(candidates.Length == 1, "Expected exactly one existing continuous Region1 ground mesh");
            return candidates[0];
        }
        internal static WorldHotspot[] Hotspots(ValoriaIntegratedWorld host)
        {
            return host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true).OrderBy(h => h.Id).ToArray();
        }
        static void CameraShot(Camera c, string name, int width, int height)
        {
            ValoriaReferenceCleanBuilder.Capture(c, Path.Combine(Evidence, name), width, height);
        }

        static readonly Dictionary<string,string> CityComparisonStamps = new Dictionary<string,string>();
        static void VerifyCityCameraStamp(Camera c, string prefix, string kind, int width, int height)
        {
            // Calibration uses an independent temporary Unity camera.
            c.aspect = width / (float)height;
            c.ResetProjectionMatrix();
            string stamp = c.transform.position.ToString("R") + "|" +
                c.transform.rotation.ToString("R") + "|" +
                c.orthographicSize.ToString("R") + "|" +
                c.projectionMatrix.ToString("F7") + "|" +
                c.worldToCameraMatrix.ToString("F7");
            File.WriteAllText(Path.Combine(Evidence,
                prefix + "-" + kind + "-camera.json"),
                "{\"kind\":\"" + kind + "\",\"width\":" + width +
                ",\"height\":" + height + ",\"stamp\":\"" +
                stamp.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n") + "\"}");
            if (prefix == "before") CityComparisonStamps[kind] = stamp;
            else if (prefix == "after")
            {
                Need(CityComparisonStamps.ContainsKey(kind) &&
                    CityComparisonStamps[kind] == stamp,
                    "CITY_AB_CAMERA_MATRIX_MISMATCH " + kind);
            }
        }
        internal static void CityViews(ValoriaReferenceCleanGameplay city, string prefix)
        {
            // Only the review copy renders. The actual gameplay camera is never
            // allowed to re-fit itself to terrain/aspect between two A/B captures.
            var actual = city.PlayerCamera;
            var originalPos = actual.transform.position;
            var originalRot = actual.transform.rotation;
            float originalZoom = actual.orthographicSize;
            var obj = new GameObject("Temporary city A-B audit camera / not saved");
            obj.hideFlags = HideFlags.HideAndDontSave;
            var cam = obj.AddComponent<Camera>();
            cam.CopyFrom(actual);
            cam.enabled = false;
            cam.transform.SetPositionAndRotation(originalPos, originalRot);
            cam.orthographicSize = originalZoom;
            var groups = new[] { city.BastionOne, city.BastionTwo, city.Parcels.LeftBuilt, city.Parcels.RightBuilt };
            Need(groups.All(o => o != null), "Missing protected Bastion I–II or unlocked buildings");
            bool[] originalActive = groups.Select(g => g.activeSelf).ToArray();
            try
            {
                for (int stage = 0; stage < 2; stage++)
                {
                    groups[0].SetActive(stage == 0);
                    groups[1].SetActive(stage == 1);
                    groups[2].SetActive(stage == 1);
                    groups[3].SetActive(stage == 1);
                    string suffix = stage == 0 ? "I" : "II";
                    cam.transform.SetPositionAndRotation(originalPos, originalRot);
                    cam.orthographicSize = originalZoom;
                    VerifyCityCameraStamp(cam, prefix, "city-" + suffix + "-wide", 1600, 900);
                    CameraShot(cam, prefix + "-city-" + suffix + "-wide.png", 1600, 900);
                    cam.transform.SetPositionAndRotation(originalPos, originalRot);
                    cam.orthographicSize = originalZoom;
                    VerifyCityCameraStamp(cam, prefix, "city-" + suffix + "-mobile", 900, 1600);
                    CameraShot(cam, prefix + "-city-" + suffix + "-mobile.png", 900, 1600);
                }
                Need(actual.transform.position == originalPos &&
                    actual.transform.rotation == originalRot &&
                    Mathf.Abs(actual.orthographicSize - originalZoom) < .0001f,
                    "Original player camera changed during isolated comparison");
            }
            finally
            {
                for (int i = 0; i < groups.Length; i++) groups[i].SetActive(originalActive[i]);
                Object.DestroyImmediate(obj);
            }
        }
        internal static void WorldViews(ValoriaIntegratedWorld host, string prefix, bool extended)
        {
            var c = host.WorldCamera;
            var cameraPos = c.transform.position;
            var cameraRot = c.transform.rotation;
            float zoom = c.orthographicSize;
            float aspect = c.aspect;
            bool originalRootState = host.WorldRoot.activeSelf;
            var nav = host.WorldNavigation;
            Vector3 navTarget = nav.target;
            try
            {
                host.WorldRoot.SetActive(true);
                var ground = WorldGround(host);
                var renderer = ground.GetComponent<Renderer>();
                Need(renderer != null, "Region1 surface renderer missing");
                var box = renderer.bounds;
                float left = nav.minimum.x, right = nav.maximum.x;
                float near = nav.minimum.y, far = nav.maximum.y;
                // These authored limits come from the real ground renderer, not imaginary territory.
                Need(left <= box.max.x && right >= box.min.x && near <= box.max.z && far >= box.min.z, "Authored world camera limits inconsistent with source mesh");
                Vector3 center = new Vector3((left + right) * .5f, (box.min.y + box.max.y) * .5f, (near + far) * .5f);
                c.transform.rotation = cameraRot;
                c.transform.position = center - c.transform.forward * Mathf.Max(140f, nav.cameraDistance);
                c.aspect = 2048f / 1152f;
                float projectedZ = Mathf.Abs(far - near) * Mathf.Sin(nav.tilt * Mathf.Deg2Rad);
                float projectedY = (box.max.y - box.min.y) * Mathf.Cos(nav.tilt * Mathf.Deg2Rad);
                c.orthographicSize = Mathf.Max((right - left) / (2f * c.aspect), (projectedZ + projectedY) * .5f) * 1.16f + 2;
                CameraShot(c, prefix + "-world-REGION1-ENTIRE-2048x1152.png", 2048, 1152);
                // Actual gameplay home view, without moving or replacing the gameplay camera asset.
                c.transform.SetPositionAndRotation(cameraPos, cameraRot);
                c.orthographicSize = zoom;
                c.aspect = 1600f / 900f;
                CameraShot(c, prefix + "-world-strategic-wide.png", 1600, 900);
                c.aspect = 900f / 1600f;
                CameraShot(c, prefix + "-world-strategic-mobile.png", 900, 1600);
                if (!extended) return;
                foreach (string id in new[] { "forest-valoria", "quarry-valoria", "corrupt-scout", "engendro-valoria" })
                {
                    var poi = Hotspots(host).Single(h => h.Id == id);
                    c.transform.position = new Vector3(poi.transform.position.x, 2, poi.transform.position.z) - c.transform.forward * Mathf.Max(140f, nav.cameraDistance);
                    c.orthographicSize = 27;
                    c.aspect = 1600f / 900f;
                    CameraShot(c, prefix + "-world-" + id + "-wide.png", 1600, 900);
                    c.orthographicSize = 19;
                    c.aspect = 900f / 1600f;
                    CameraShot(c, prefix + "-world-" + id + "-mobile.png", 900, 1600);
                }
            }
            finally
            {
                c.transform.SetPositionAndRotation(cameraPos, cameraRot);
                c.orthographicSize = zoom;
                c.aspect = aspect;
                nav.target = navTarget;
                host.WorldRoot.SetActive(originalRootState);
            }
        }


        // The native survey is immutable evidence. A trial only changes objects in
        // memory and always restores the original scene. Integration requires a
        // reviewer-signed acceptance file linked to its exact A/B image bytes.
        [Serializable] public class Approval
        {
            public bool accepted;
            public string source_scene_sha256, tool_sha256, reviewer, review_path;
            public string[] image_names, image_sha256;
        }
        [Serializable] public class Candidate
        {
            public string source_scene_sha256, tool_sha256, method, world_ground_source;
            public string[] original_poi_ids, after_poi_ids, images, image_sha256;
            public int source_world_triangles, candidate_world_triangles, sampled_conifer_renderers, added_visual_only_forest_instances;
            public int source_height_resolution, candidate_height_resolution, changed_city_height_samples;
            public bool previous_scene_untouched, city_terrain_collider_unchanged, world_source_mesh_unchanged;
            public bool native_unity_ab, art_approved, physical_mobile_profiled, published;
        }
        sealed class GeographicRoad
        {
            readonly float z0, z1;
            readonly Vector3[] section;
            public GeographicRoad(MeshFilter f)
            {
                var v = f.sharedMesh.vertices.Select(q => f.transform.TransformPoint(q)).ToArray();
                z0 = v.Min(q => q.z); z1 = v.Max(q => q.z);
                int n = Mathf.CeilToInt((z1 - z0) * 2) + 1;
                section = new Vector3[n];
                for (int i = 0; i < n; i++)
                {
                    var range = v.Where(q => Mathf.Abs(q.z - (z0 + i * .5f)) < .6f).ToArray();
                    Need(range.Length > 0, "Existing paving sampling failed");
                    section[i] = new Vector3(range.Min(q => q.x), range.Max(q => q.x), 0);
                }
            }
            public float Edge(float x, float z)
            {
                if (z < z0 || z > z1) return 0;
                var slice = section[Mathf.Clamp(Mathf.RoundToInt((z - z0) * 2), 0, section.Length - 1)];
                return Mathf.Max(slice.x - x, x - slice.y);
            }
        }
        static float Smooth(float lo, float hi, float v)
        {
            float a = Mathf.Clamp01((v - lo) / Mathf.Max(.0001f, hi - lo));
            return a * a * (3 - 2 * a);
        }
        static float Noise(float x, float z)
        {
            return .63f * Mathf.PerlinNoise(x * .024f + 16, z * .024f + 8) +
                .26f * Mathf.PerlinNoise(x * .063f + 3, z * .063f + 23) +
                .11f * Mathf.PerlinNoise(x * .19f + 91, z * .19f + 52);
        }
        static float PlanarDistance(Vector3 a, Vector3 b)
        {
            float x = a.x - b.x, z = a.z - b.z;
            return Mathf.Sqrt(x * x + z * z);
        }
        static TerrainData AuthorCityTerrain(ValoriaReferenceCleanGameplay city, Terrain terrain)
        {
            TerrainData data = Object.Instantiate(terrain.terrainData);
            data.name = "Valoria organic prairie / existing height and paths";
            var filters = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
            var roads = new[] { "Axial_Paving", "Outer_Road" }.Select(name =>
                new GeographicRoad(filters.Single(f => f.name == name && f.sharedMesh != null))).ToArray();
            int size = data.alphamapResolution;
            var alphas = data.GetAlphamaps(0, 0, size, size);
            Need(alphas.GetLength(2) >= 3, "Existing soil/grass blend layers unavailable");
            var buildings = new[] { city.Parcels.LeftBuilt, city.Parcels.RightBuilt }
                .Where(g => g != null).Select(g =>
                {
                    var rs = g.GetComponentsInChildren<Renderer>(true);
                    if (rs.Length == 0) return new Bounds(g.transform.position, Vector3.one);
                    var box = rs[0].bounds;
                    foreach (var rr in rs.Skip(1)) box.Encapsulate(rr.bounds);
                    return box;
                }).ToArray();
            for (int iz = 0; iz < size; iz++)
                for (int ix = 0; ix < size; ix++)
                {
                    float x = terrain.transform.position.x + data.size.x * ix / (size - 1f);
                    float z = terrain.transform.position.z + data.size.z * iz / (size - 1f);
                    float warp = (Mathf.PerlinNoise(x * .19f + 14, z * .19f + 37) - .5f) * 1.1f;
                    float shoulder = 0;
                    foreach (var road in roads)
                    {
                        float edge = road.Edge(x, z);
                        shoulder = Mathf.Max(shoulder, 1f - Smooth(-.4f, 2.5f + warp, edge));
                    }
                    // Only worn shoulders of EXISTING paving, never four plot imprints.
                    float river = -23f + 1.5f * Mathf.Sin(x * .065f) +
                        .85f * Mathf.Sin(x * .19f + .4f) * Smooth(6, 17, Mathf.Abs(x));
                    float riverEdge = Mathf.Abs(z - river) - 7.2f;
                    float bank = 1f - Smooth(.7f, 6.2f + warp, riverEdge);
                    float contact = 0;
                    foreach (var b in buildings)
                    {
                        float dx = Mathf.Max(Mathf.Abs(x - b.center.x) - b.extents.x, 0);
                        float dz = Mathf.Max(Mathf.Abs(z - b.center.z) - b.extents.z, 0);
                        contact = Mathf.Max(contact, 1f - Smooth(.2f, 2.3f + warp, Mathf.Sqrt(dx * dx + dz * dz)));
                    }
                    float paint = Mathf.Clamp01(.33f * shoulder + .20f * bank + .12f * contact);
                    // No erosion that changes navigation, terrain height or hidden parcel geometry.
                    float available = alphas[iz, ix, 0] + alphas[iz, ix, 1];
                    float transferred = available * paint;
                    if (available > .0001f)
                    {
                        float r = alphas[iz, ix, 0] / available;
                        alphas[iz, ix, 0] -= transferred * r;
                        alphas[iz, ix, 1] -= transferred * (1f - r);
                        alphas[iz, ix, 2] += transferred;
                    }
                    // Broad interwoven grass families rather than tiny random spots.
                    // Preserve the original combined grass weight and all other layers.
                    float green = alphas[iz, ix, 0] + alphas[iz, ix, 1];
                    if (green > .0001f)
                    {
                        float mix = alphas[iz, ix, 1] / green;
                        float detail = Noise(x + 73, z - 121) - .5f;
                        mix = Mathf.Clamp01(mix + detail * .22f);
                        alphas[iz, ix, 0] = green * (1f - mix);
                        alphas[iz, ix, 1] = green * mix;
                    }
                }
            data.SetAlphamaps(0, 0, alphas);
            return data;
        }
        static float[,] Density(ValoriaIntegratedWorld host, Bounds bounds, out int trees)
        {
            const int size = 128;
            float[,] result = new float[size, size];
            var sources = host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true)
                .Where(m => m.enabled &&
                (m.name.IndexOf("Conifer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 m.name.IndexOf("pine", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 m.name.IndexOf("understory", StringComparison.OrdinalIgnoreCase) >= 0))
                .Select(r => r.bounds.center).ToArray();
            trees = sources.Length;
            if (trees < 8)
            {
                // Measured forest POI provides a conservative seed if mesh-family
                // naming changed; the existing individual trees stay untouched.
                sources = Hotspots(host).Where(h => h.Id == "forest-valoria")
                    .Select(h => h.transform.position).ToArray();
            }
            foreach (var p in sources)
            {
                int centerX = Mathf.RoundToInt((p.x - bounds.min.x) / bounds.size.x * (size - 1));
                int centerZ = Mathf.RoundToInt((p.z - bounds.min.z) / bounds.size.z * (size - 1));
                float pxRadius = 17f * (size - 1) / bounds.size.x;
                float pzRadius = 17f * (size - 1) / bounds.size.z;
                for (int z = Mathf.Max(0, Mathf.FloorToInt(centerZ - pzRadius)); z <= Mathf.Min(size - 1, Mathf.CeilToInt(centerZ + pzRadius)); z++)
                    for (int x = Mathf.Max(0, Mathf.FloorToInt(centerX - pxRadius)); x <= Mathf.Min(size - 1, Mathf.CeilToInt(centerX + pxRadius)); x++)
                    {
                        float distance = Mathf.Sqrt(Mathf.Pow((x - centerX) / pxRadius, 2f) +
                                                    Mathf.Pow((z - centerZ) / pzRadius, 2f));
                        float weight = 1f - Smooth(.15f, 1f, distance);
                        result[z, x] = Mathf.Min(2.2f, result[z, x] + .36f * weight);
                    }
            }
            return result;
        }
        static float ReadDensity(float[,] mask, float u, float v)
        {
            int size = mask.GetLength(0);
            float x = Mathf.Clamp01(u) * (size - 1), z = Mathf.Clamp01(v) * (size - 1);
            int ix = Mathf.FloorToInt(x), iz = Mathf.FloorToInt(z);
            int ax = Mathf.Min(ix + 1, size - 1), az = Mathf.Min(iz + 1, size - 1);
            return Mathf.Lerp(Mathf.Lerp(mask[iz, ix], mask[iz, ax], x - ix),
                              Mathf.Lerp(mask[az, ix], mask[az, ax], x - ix), z - iz);
        }
        static Texture2D ReadableTexture(Texture texture)
        {
            if (!texture) return null;
            var rt = RenderTexture.GetTemporary(512, 512, 0, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            try
            {
                Graphics.Blit(texture, rt);
                RenderTexture.active = rt;
                var sample = new Texture2D(512, 512, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat };
                sample.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
                sample.Apply();
                return sample;
            }
            finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(rt); }
        }
        static Mesh AuthorWorldMesh(MeshFilter source, WorldHotspot[] spots)
        {
            var original = source.sharedMesh;
            var next = Object.Instantiate(original);
            next.name = "Region1 continuous authored macrorelief / original 4X topology";
            var v = original.vertices;
            var warped = new Vector3[v.Length];
            var b = source.GetComponent<Renderer>().bounds;
            Need(b.size.x > 100 && b.size.z > 100, "Unexpected Region1 real terrain extent");
            for (int i = 0; i < v.Length; i++)
            {
                Vector3 world = source.transform.TransformPoint(v[i]);
                float nearest = spots.Min(h => PlanarDistance(world, h.transform.position));
                float anchor = Smooth(10f, 28f, nearest);
                // Wide shallow ridges extend the existing eastern quarry geology
                // into surrounding open grass. Existing POIs and roads remain in place.
                // Author three low, connected macroforms around existing
                // geographic landmarks, rather than noise over the entire map.
                float quarryFoothill = 3.7f * Mathf.Exp(
                    -Mathf.Pow((world.x - 1092f) / 54f, 2f) -
                    Mathf.Pow((world.z + 73f) / 67f, 2f));
                float northForestRise = 1.65f * Mathf.Exp(
                    -Mathf.Pow((world.x - 1051f) / 76f, 2f) -
                    Mathf.Pow((world.z - 78f) / 58f, 2f));
                float corruptionValley = -1.55f * Mathf.Exp(
                    -Mathf.Pow((world.x - 933f) / 45f, 2f) -
                    Mathf.Pow((world.z + 26f) / 65f, 2f));
                float reliefGrain = (Noise(world.x, world.z) - .5f) * .6f;
                float edgeDist = Mathf.Min(
                    Mathf.Min(world.x - b.min.x, b.max.x - world.x),
                    Mathf.Min(world.z - b.min.z, b.max.z - world.z));
                float edgeProtect = Smooth(1.5f, 16f, edgeDist);
                // Preserve the original geology base directly under its source
                // meshes; the new hillside must not swallow authored rocks.
                float rockDist = Mathf.Sqrt(Mathf.Pow((world.x - 1098f) / 35f, 2f) +
                                            Mathf.Pow((world.z + 83f) / 39f, 2f));
                float rockProtect = Smooth(.8f, 1.8f, rockDist);
                float deltaY = edgeProtect * anchor * rockProtect *
                    (quarryFoothill + northForestRise + corruptionValley + reliefGrain);
                Vector3 altered = world + Vector3.up * deltaY;
                warped[i] = source.transform.InverseTransformPoint(altered);
            }
            next.vertices = warped;
            Need(next.uv.SequenceEqual(original.uv), "Region1 source UV atlas changed");
            next.RecalculateNormals();
            next.RecalculateBounds();
            Need(next.triangles.Length == original.triangles.Length &&
                next.vertexCount == original.vertexCount, "World topology changed");
            return next;
        }

        static Vector3[] OriginalUvWorldMapping(MeshFilter filter, Material originalMaterial)
        {
            var uv = filter.sharedMesh.uv;
            var verts = filter.sharedMesh.vertices;
            Need(uv.Length == verts.Length && verts.Length >= 100,
                "Real world UV/source mesh cardinality mismatch");
            Vector2 tiling = originalMaterial.GetTextureScale("_BaseMap");
            Vector2 offset = originalMaterial.GetTextureOffset("_BaseMap");
            Need((tiling - Vector2.one).sqrMagnitude < .0001f && offset.sqrMagnitude < .0001f,
                "Repeating UV atlas cannot carry geographic masks without road displacement");
            int n = uv.Length;
            double su=0,sv=0,sx=0,sz=0;
            float umin=float.MaxValue,umax=float.MinValue,vmin=float.MaxValue,vmax=float.MinValue;
            for(int i=0;i<n;i++)
            {
                Vector3 p=filter.transform.TransformPoint(verts[i]);
                su+=uv[i].x;sv+=uv[i].y;sx+=p.x;sz+=p.z;
                umin=Mathf.Min(umin,uv[i].x);umax=Mathf.Max(umax,uv[i].x);
                vmin=Mathf.Min(vmin,uv[i].y);vmax=Mathf.Max(vmax,uv[i].y);
            }
            Need(umin>=-.005f&&umax<=1.005f&&vmin>=-.005f&&vmax<=1.005f,
                "UVs exceed one unique world atlas; reject rather than move original paths");
            double mu=su/n,mv=sv/n,mx=sx/n,mz=sz/n;
            double uu=0,vv=0,uvv=0,ux=0,vx=0,uz=0,vz=0;
            for(int i=0;i<n;i++)
            {
                Vector3 p=filter.transform.TransformPoint(verts[i]);
                double u=uv[i].x-mu,v=uv[i].y-mv;
                uu+=u*u;vv+=v*v;uvv+=u*v;
                ux+=u*(p.x-mx);vx+=v*(p.x-mx);
                uz+=u*(p.z-mz);vz+=v*(p.z-mz);
            }
            double det=uu*vv-uvv*uvv;
            Need(det>.0000001,"Degenerate world UV coverage");
            double ax=(ux*vv-vx*uvv)/det,bx=(vx*uu-ux*uvv)/det;
            double az=(uz*vv-vz*uvv)/det,bz=(vz*uu-uz*uvv)/det;
            double squared=0;
            for(int i=0;i<n;i++)
            {
                Vector3 p=filter.transform.TransformPoint(verts[i]);
                double dx=ax*uv[i].x+bx*uv[i].y+mx-ax*mu-bx*mv-p.x;
                double dz=az*uv[i].x+bz*uv[i].y+mz-az*mu-bz*mv-p.z;
                squared+=dx*dx+dz*dz;
            }
            double rmse=Math.Sqrt(squared/n);
            Need(rmse<1.5,"Actual source UV not geographic/affine. Reject road/corruption remapping. RMS="+rmse);
            Debug.Log("ELDORIA_REGION1_UV_WORLD_FIT_PASS rms="+rmse);
            return new[] {
                new Vector3((float)ax,(float)bx,(float)(mx-ax*mu-bx*mv)),
                new Vector3((float)az,(float)bz,(float)(mz-az*mu-bz*mv))
            };
        }
        static Texture2D AuthorWorldAlbedo(ValoriaIntegratedWorld host, MeshFilter source, out int trees)
        {
            var renderer = source.GetComponent<Renderer>();
            var bounds = renderer.bounds;
            var ids = Hotspots(host).ToDictionary(h => h.Id, h => h.transform.position);
            var forest = Density(host, bounds, out trees);
            var existing = renderer.sharedMaterial;
            Texture diffuse = existing && existing.HasProperty("_BaseMap") ? existing.GetTexture("_BaseMap") :
                existing && existing.HasProperty("_MainTex") ? existing.GetTexture("_MainTex") : null;
            Texture2D sample = ReadableTexture(diffuse);
            Need(sample != null, "Original baked World4X roads/corruption albedo missing");
            Vector3[] atlas = OriginalUvWorldMapping(source, existing);
            const int size = 1024;
            var result = new Texture2D(size, size, TextureFormat.RGB24, true, false)
            {
                name = "Region1 continuous soil / forest / quarry / corrupted ecology",
                wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear, anisoLevel = 4
            };
            var pixels = new Color[size * size];
            try
            {
                for (int iz = 0; iz < size; iz++)
                    for (int ix = 0; ix < size; ix++)
                    {
                        float u = ix / (size - 1f), v = iz / (size - 1f);
                        float wx = atlas[0].x * u + atlas[0].y * v + atlas[0].z;
                        float wz = atlas[1].x * u + atlas[1].y * v + atlas[1].z;
                        float wu = (wx - bounds.min.x) / bounds.size.x;
                        float wv = (wz - bounds.min.z) / bounds.size.z;
                        float micro = Mathf.PerlinNoise(wx * .59f + 9, wz * .59f + 37);
                        float warpX = (Mathf.PerlinNoise(wx * .075f + 8, wz * .075f + 10) - .5f) * 7f;
                        float warpZ = (Mathf.PerlinNoise(wx * .075f + 46, wz * .075f + 26) - .5f) * 7f;
                        Vector3 warped = new Vector3(wx + warpX, 0, wz + warpZ);
                        float quarry = 1f - Smooth(13f, 43f, PlanarDistance(warped, ids["quarry-valoria"]));
                        float corruption = Mathf.Max(1f - Smooth(18f, 42f, PlanarDistance(warped, ids["corrupt-scout"])),
                                                     1f - Smooth(16f, 40f, PlanarDistance(warped, ids["engendro-valoria"])));
                        float treesFloor = Smooth(.18f, 1.1f, ReadDensity(forest, wu, wv));
                        Color baseColor = sample.GetPixelBilinear(u, v);
                        float variation = (Noise(wx, wz) - .5f) * .16f + (micro - .5f) * .025f;
                        baseColor *= 1f + variation;
                        // Author region-scale environmental continuity rather than a
                        // ring, flat blob, overlay decal or extra artificial prefabs.
                        baseColor = Color.Lerp(baseColor,
                            new Color(baseColor.r * .79f, baseColor.g * .91f, baseColor.b * .71f),
                            treesFloor * .48f);
                        baseColor = Color.Lerp(baseColor,
                            new Color(baseColor.r * 1.03f, baseColor.g * .91f, baseColor.b * .79f),
                            quarry * .36f);
                        baseColor = Color.Lerp(baseColor,
                            new Color(baseColor.r * .80f, baseColor.g * .80f, baseColor.b * .84f),
                            corruption * .18f);
                        baseColor.a = 1f;
                        pixels[iz * size + ix] = baseColor;
                    }
                result.SetPixels(pixels);
                result.Apply(true, false);
                return result;
            }
            finally { if (sample) Object.DestroyImmediate(sample); }
        }

        static GameObject ExpandExistingForest(ValoriaIntegratedWorld host, MeshFilter worldGround, out int additions)
        {
            var forest = Hotspots(host).Single(p => p.Id == "forest-valoria").transform.position;
            var original = host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true)
                .Where(x => x.enabled && x.GetComponent<MeshFilter>() != null &&
                    (x.name.IndexOf("Conifer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     x.name.IndexOf("pine", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     x.name.IndexOf("understory", StringComparison.OrdinalIgnoreCase) >= 0))
                .OrderBy(x => x.name).ToArray();
            Need(original.Length >= 3 && original.Length <= 60,
                "Existing validated forest family not found; refuse synthetic prefabs");
            var root = new GameObject("Forest ecological continuity / approved source meshes only");
            root.transform.SetParent(host.WorldRoot.transform, false);
            var terrainVerts = worldGround.sharedMesh.vertices
                .Select(q => worldGround.transform.TransformPoint(q)).ToArray();
            float GroundHeight(float x,float z)
            {
                float nearest = float.MaxValue, height = 0;
                foreach(var p in terrainVerts)
                {
                    float d=(p.x-x)*(p.x-x)+(p.z-z)*(p.z-z);
                    if(d<nearest){nearest=d;height=p.y;}
                }
                Need(nearest<11f,"Tree site lies beyond surveyed ground resolution");
                return height;
            }
            var placed = new List<Vector3>(original.Select(x => x.bounds.center));
            Vector2[] offsets = {
                new Vector2(-29f,9f), new Vector2(-22f,24f),
                new Vector2(-8f,31f), new Vector2(16f,29f),
                new Vector2(30f,15f), new Vector2(28f,-9f),
                new Vector2(9f,-24f), new Vector2(-16f,-23f),
                new Vector2(-35f,-8f), new Vector2(3f,17f)
            };
            additions=0;
            for(int i=0;i<offsets.Length;i++)
            {
                Vector3 target = forest + new Vector3(offsets[i].x,0,offsets[i].y);
                float nearest = placed.Min(v=>PlanarDistance(v,target));
                if(nearest<8.0f)continue;  // retained natural clearings
                var donor=original[i%original.Length];
                var meshFilter=donor.GetComponent<MeshFilter>();
                if(!meshFilter||!meshFilter.sharedMesh)continue;
                float baseToCenter=donor.bounds.center.y-donor.bounds.min.y;
                target.y=GroundHeight(target.x,target.z)+baseToCenter+.045f;
                var obj=new GameObject("Existing conifer canopy / distributed cluster "+i);
                obj.transform.SetParent(root.transform,true);
                // Preserve the original mesh's pivot, even if source vertices
                // are stored in world coordinates relative to its donor.
                Vector3 displacement=target-donor.bounds.center;
                obj.transform.position=donor.transform.position+displacement;
                obj.transform.rotation=donor.transform.rotation;
                obj.transform.localScale=donor.transform.lossyScale;
                obj.AddComponent<MeshFilter>().sharedMesh=meshFilter.sharedMesh;
                var rr=obj.AddComponent<MeshRenderer>();
                rr.sharedMaterials=donor.sharedMaterials;
                rr.shadowCastingMode=donor.shadowCastingMode;
                rr.receiveShadows=donor.receiveShadows;
                rr.lightProbeUsage=donor.lightProbeUsage;
                rr.reflectionProbeUsage=donor.reflectionProbeUsage;
                rr.renderingLayerMask=donor.renderingLayerMask;
                Vector3 actualCenter=rr.bounds.center;
                Need(PlanarDistance(actualCenter,target)<3.5f &&
                     Mathf.Abs(rr.bounds.min.y-GroundHeight(target.x,target.z))<2.5f,
                     "Cloned conifer must actually render grounded inside surveyed forest");
                // Absolutely no collider, WorldHotspot, army marker, or LOD
                // owner is cloned: only compatible visual source geometry.
                placed.Add(actualCenter);additions++;
            }
            Need(additions>=3&&additions<=10,"Forest expansion did not produce a useful silhouette");
            return root;
        }
        static string[] ImageNames(string prefix)
        {
            return new[] { "city-I-wide", "city-I-mobile", "city-II-wide", "city-II-mobile",
                "world-REGION1-ENTIRE-2048x1152", "world-strategic-wide", "world-strategic-mobile",
                "world-forest-valoria-wide", "world-forest-valoria-mobile",
                "world-quarry-valoria-wide", "world-quarry-valoria-mobile",
                "world-corrupt-scout-wide", "world-corrupt-scout-mobile",
                "world-engendro-valoria-wide", "world-engendro-valoria-mobile" }
                .Select(s => prefix + "-" + s + ".png").ToArray();
        }
        public static void Trial() { AuthorAndCapture(false); }
        public static void Integrate() { AuthorAndCapture(true); }
        static void AuthorAndCapture(bool integrate)
        {
            TerrainData newCity = null;
            Mesh worldMesh = null;
            Texture2D albedo = null;
            Material worldMat = null;
            GameObject terrainVisual = null;
            GameObject forestVisual = null;
            Terrain terrain = null;
            TerrainData originalTerrain = null;
            Renderer oldRenderer = null;
            bool oldEnabled = false;
            bool saved = false;
            try
            {
                Directory.CreateDirectory(Evidence);
                string sceneHash = Sha(ScenePath);
                string toolHash = Sha("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/ValoriaGeographyContinuityV1.cs");
                var surveyFile = Path.Combine(Evidence, "survey.json");
                Need(File.Exists(surveyFile), "Real current Unity source survey required");
                var survey = JsonUtility.FromJson<SurveyRecord>(File.ReadAllText(surveyFile));
                Need(survey.source_scene_sha256 == sceneHash, "Canonical scene differs from measured source");
                Approval approval = null;
                if (integrate)
                {
                    var approvalPath = Path.Combine(Evidence, "accepted-candidate.json");
                    Need(File.Exists(approvalPath), "Independent review approval missing");
                    approval = JsonUtility.FromJson<Approval>(File.ReadAllText(approvalPath));
                    Need(approval.accepted && approval.source_scene_sha256 == sceneHash &&
                        approval.tool_sha256 == toolHash &&
                        !string.IsNullOrEmpty(approval.reviewer) &&
                        !string.IsNullOrEmpty(approval.review_path),
                        "Independent source-pinned art acceptance required");
                }
                var host = Load(out var city);
                // Editor rendering and Terrain.Flush may re-frame the live camera.
                // The A/B must restore the exact initial pose and projection on
                // both legs; a different beauty angle is not valid evidence.
                var initialCityPos = city.PlayerCamera.transform.position;
                var initialCityRot = city.PlayerCamera.transform.rotation;
                float initialCityZoom = city.PlayerCamera.orthographicSize;
                float initialCityAspect = city.PlayerCamera.aspect;
                var initialWorldPos = host.WorldCamera.transform.position;
                var initialWorldRot = host.WorldCamera.transform.rotation;
                float initialWorldZoom = host.WorldCamera.orthographicSize;
                float initialWorldAspect = host.WorldCamera.aspect;
                terrain = Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None).Single();
                originalTerrain = terrain.terrainData;
                var originalCollider = terrain.GetComponent<TerrainCollider>();
                Need(originalCollider != null, "Existing city terrain collider missing");
                var originalPhysicsData = originalCollider.terrainData;
                MeshFilter ground = WorldGround(host);
                Mesh originalWorldMesh = ground.sharedMesh;
                oldRenderer = ground.GetComponent<Renderer>();
                oldEnabled = oldRenderer.enabled;
                int beforeColliders = host.WorldRoot.GetComponentsInChildren<Collider>(true).Length;
                var beforePOI = Hotspots(host).Select(s => s.Id + "=" + s.transform.position.ToString("R")).ToArray();
                var beforeWorldTriangles = originalWorldMesh.triangles.Length / 3;
                string[] imgs = ImageNames("after");
                CityViews(city, "before");
                WorldViews(host, "before", true);
                newCity = AuthorCityTerrain(city, terrain);
                worldMesh = AuthorWorldMesh(ground, Hotspots(host));
                albedo = AuthorWorldAlbedo(host, ground, out int treeCount);
                var existingWorldMat = oldRenderer.sharedMaterial;
                Need(existingWorldMat && existingWorldMat.HasProperty("_BaseMap"), "Original URP world material unavailable");
                worldMat = new Material(existingWorldMat)
                    { name = "Region1 original roads/biomes retained plus organic dressing" };
                Need(worldMat.shader != null, "URP Lit shader unavailable");
                worldMat.SetTexture("_BaseMap", albedo);
                // Keep original tint/roughness/metallic/normal properties unchanged.
                terrain.terrainData = newCity;
                terrain.Flush();
                terrainVisual = new GameObject("Region1 organic macrogeography / visual only");
                terrainVisual.layer = ground.gameObject.layer;
                terrainVisual.transform.SetParent(ground.transform, false);
                terrainVisual.transform.localPosition = Vector3.zero;
                terrainVisual.transform.localRotation = Quaternion.identity;
                terrainVisual.transform.localScale = Vector3.one;
                terrainVisual.AddComponent<MeshFilter>().sharedMesh = worldMesh;
                var newRenderer = terrainVisual.AddComponent<MeshRenderer>();
                newRenderer.sharedMaterial = worldMat;
                newRenderer.shadowCastingMode = oldRenderer.shadowCastingMode;
                newRenderer.receiveShadows = oldRenderer.receiveShadows;
                newRenderer.lightmapIndex = -1;
                oldRenderer.enabled = false;
                forestVisual = ExpandExistingForest(host, ground, out int forestAdditions);
                var afterPOI = Hotspots(host).Select(s => s.Id + "=" + s.transform.position.ToString("R")).ToArray();
                Need(beforePOI.SequenceEqual(afterPOI), "Original POI identities or positions changed");
                Need(host.WorldRoot.GetComponentsInChildren<Collider>(true).Length == beforeColliders,
                    "New gameplay collider introduced");
                Need(originalCollider.terrainData == originalPhysicsData && ground.sharedMesh == originalWorldMesh,
                    "Existing city/world collision or underlying mesh changed");
                city.PlayerCamera.transform.SetPositionAndRotation(initialCityPos, initialCityRot);
                city.PlayerCamera.orthographicSize = initialCityZoom;
                city.PlayerCamera.aspect = initialCityAspect;
                host.WorldCamera.transform.SetPositionAndRotation(initialWorldPos, initialWorldRot);
                host.WorldCamera.orthographicSize = initialWorldZoom;
                host.WorldCamera.aspect = initialWorldAspect;
                CityViews(city, "after");
                WorldViews(host, "after", true);
                Need(city.PlayerCamera.transform.position == initialCityPos &&
                    city.PlayerCamera.transform.rotation == initialCityRot &&
                    Mathf.Abs(city.PlayerCamera.orthographicSize - initialCityZoom) < .0001f &&
                    host.WorldCamera.transform.position == initialWorldPos &&
                    host.WorldCamera.transform.rotation == initialWorldRot &&
                    Mathf.Abs(host.WorldCamera.orthographicSize - initialWorldZoom) < .0001f,
                    "Native A/B camera pose changed; no artistic acceptance possible");
                Candidate candidate = new Candidate
                {
                    source_scene_sha256 = sceneHash, tool_sha256 = toolHash,
                    method = "Retained-source conifer silhouette masses, original forest POI and safe clearings; three site-specific smooth macroforms and protected quarry rock base; original UV/roads/6 POIs preserved",
                    world_ground_source = AssetDatabase.GetAssetPath(originalWorldMesh),
                    original_poi_ids = beforePOI, after_poi_ids = afterPOI,
                    images = imgs, image_sha256 = imgs.Select(x => Sha(Path.Combine(Evidence, x))).ToArray(),
                    source_world_triangles = beforeWorldTriangles,
                    candidate_world_triangles = worldMesh.triangles.Length / 3,
                    sampled_conifer_renderers = treeCount,
                    added_visual_only_forest_instances = forestAdditions,
                    source_height_resolution = originalTerrain.heightmapResolution,
                    candidate_height_resolution = newCity.heightmapResolution,
                    changed_city_height_samples = 0,
                    previous_scene_untouched = true,
                    city_terrain_collider_unchanged = originalCollider.terrainData == originalPhysicsData,
                    world_source_mesh_unchanged = ground.sharedMesh == originalWorldMesh,
                    native_unity_ab = true, art_approved = false, physical_mobile_profiled = false, published = false
                };
                Need(sceneHash == Sha(ScenePath), "Trial unexpectedly modified scene bytes");
                if (integrate)
                {
                    Need(approval.image_names != null && approval.image_sha256 != null &&
                        approval.image_names.Length == imgs.Length &&
                        approval.image_names.SequenceEqual(imgs) &&
                        approval.image_sha256.SequenceEqual(candidate.image_sha256),
                        "Accepted original Unity A/B bytes do not match integrated result");
                    Need(!AssetDatabase.IsValidFolder(AssetFolder), "Refuse overwriting previously integrated terrain assets");
                    Directory.CreateDirectory(AssetFolder);
                    AssetDatabase.Refresh();
                    AssetDatabase.CreateAsset(newCity, AssetFolder + "/ValoriaSurface.asset");
                    AssetDatabase.CreateAsset(worldMesh, AssetFolder + "/Region1Macroterrain.asset");
                    AssetDatabase.CreateAsset(albedo, AssetFolder + "/Region1Biomes.asset");
                    AssetDatabase.CreateAsset(worldMat, AssetFolder + "/Region1Biomes.mat");
                    AssetDatabase.SaveAssets();
                    EditorSceneManager.MarkSceneDirty(host.gameObject.scene);
                    Need(EditorSceneManager.SaveScene(host.gameObject.scene, ScenePath), "Canonical Unity save refused");
                    saved = true;
                    candidate.art_approved = false; // independent acceptance remains in the separate signed report
                    File.WriteAllText(Path.Combine(Evidence, "integrated.json"), JsonUtility.ToJson(candidate, true));
                    Debug.Log("VALORIA_REFERENCE_CLEAN_GEOGRAPHY_INTEGRATE_COMPLETE " + candidate.images.Length + " real images");
                }
                else
                {
                    File.WriteAllText(Path.Combine(Evidence, "trial.json"), JsonUtility.ToJson(candidate, true));
                    Debug.Log("VALORIA_REFERENCE_CLEAN_GEOGRAPHY_TRIAL_COMPLETE " + candidate.images.Length + " matched Unity A/B images");
                }
                EditorApplication.Exit(0);
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                EditorApplication.Exit(1);
            }
            finally
            {
                // Never allow a failed trial to mutate the native source scene.
                if (!saved)
                {
                    if (terrain && originalTerrain) { terrain.terrainData = originalTerrain; terrain.Flush(); }
                    if (oldRenderer) oldRenderer.enabled = oldEnabled;
                    if (forestVisual) Object.DestroyImmediate(forestVisual);
                    if (terrainVisual) Object.DestroyImmediate(terrainVisual);
                    if (worldMat) Object.DestroyImmediate(worldMat);
                    if (albedo) Object.DestroyImmediate(albedo);
                    if (worldMesh) Object.DestroyImmediate(worldMesh);
                    if (newCity) Object.DestroyImmediate(newCity);
                }
            }
        }

        public static void Survey()
        {
            try
            {
                Directory.CreateDirectory(Evidence);
                string before = Sha(ScenePath);
                var host = Load(out var city);
                var t = Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None).Single();
                var ground = WorldGround(host);
                bool rootActive = host.WorldRoot.activeSelf;
                var camPos = host.WorldCamera.transform.position;
                var camRot = host.WorldCamera.transform.rotation;
                float camSize = host.WorldCamera.orthographicSize;
                var b = ground.GetComponent<Renderer>().bounds;
                var d = t.terrainData;
                var rec = new SurveyRecord
                {
                    source_scene_sha256 = before, tool_sha256 = Sha("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/ValoriaGeographyContinuityV1.cs"),
                    region_ground_name = ground.name, terrain_origin = t.transform.position, terrain_size = d.size,
                    city_height_resolution = d.heightmapResolution, city_splat_resolution = d.alphamapResolution,
                    city_layers = d.terrainLayers.Length,
                    world_ground_min = b.min, world_ground_max = b.max,
                    world_vertices = ground.sharedMesh.vertexCount,
                    world_triangles = ground.sharedMesh.triangles.Length / 3,
                    world_uvs = ground.sharedMesh.uv.Length,
                    world_renderers = host.WorldRoot.GetComponentsInChildren<Renderer>(true).Length,
                    world_materials = ground.GetComponent<Renderer>().sharedMaterials.Select(m => m ? m.name : "<null>").ToArray(),
                    poi = Hotspots(host).Select(h => new Spot { id = h.Id, position = h.transform.position, bounds = h.GetComponent<BoxCollider>() ? h.GetComponent<BoxCollider>().bounds.size : Vector3.zero }).ToArray(),
                    world_home_zoom = camSize, world_tilt = host.WorldNavigation.tilt, world_camera_distance = host.WorldNavigation.cameraDistance,
                    preexisting_city_terrain_collider = t.GetComponent<TerrainCollider>() != null,
                    world_ground_mesh_collider = ground.GetComponent<MeshCollider>() != null,
                    physical_mobile_tested = false, art_approved = false
                };
                CityViews(city, "original");
                WorldViews(host, "original", true);
                rec.same_scene_bytes = before == Sha(ScenePath);
                rec.camera_pose_restored = camPos == host.WorldCamera.transform.position &&
                    camRot == host.WorldCamera.transform.rotation &&
                    Mathf.Abs(camSize - host.WorldCamera.orthographicSize) < 1e-4f && rootActive == host.WorldRoot.activeSelf;
                Need(rec.same_scene_bytes && rec.camera_pose_restored, "Survey modified scene or player camera");
                File.WriteAllText(Path.Combine(Evidence, "survey.json"), JsonUtility.ToJson(rec, true));
                Debug.Log("VALORIA_REFERENCE_CLEAN_GEOGRAPHY_SURVEY_COMPLETE " + rec.world_vertices + " vertices; " + rec.world_triangles + " world-ground triangles; " + rec.poi.Length + " POIs");
                EditorApplication.Exit(0);
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                EditorApplication.Exit(1);
            }
        }
    }
}
#endif
