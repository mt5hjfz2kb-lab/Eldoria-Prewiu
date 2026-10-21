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

        [Serializable] public sealed class CityCameraProof
        {
            public string kind, stamp;
            public int width, height;
        }
        static readonly Dictionary<string,string> CityComparisonStamps = new Dictionary<string,string>();
        static void VerifyCityCameraStamp(Camera c, string prefix, string kind, int width, int height)
        {
            // Calibration uses a temporary camera; preserve full matrix precision.
            c.aspect = width / (float)height;
            c.ResetProjectionMatrix();
            string stamp = c.transform.position.ToString("R") + "|" +
                c.transform.rotation.ToString("R") + "|" +
                c.orthographicSize.ToString("R") + "|" +
                c.projectionMatrix.ToString("F7") + "|" +
                c.worldToCameraMatrix.ToString("F7");
            // Unity matrix string contains CR/LF. JsonUtility escapes these
            // properly, unlike the old hand-built JSON (native260 evidence).
            var proof = new CityCameraProof {
                kind = kind, stamp = stamp, width = width, height = height
            };
            File.WriteAllText(Path.Combine(Evidence, prefix + "-" + kind + "-camera.json"),
                JsonUtility.ToJson(proof, true));
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
            public string source_scene_sha256, tool_sha256, method, world_ground_source, artist_plan_sha256;
            public string[] original_poi_ids, after_poi_ids, images, image_sha256;
            public int source_world_triangles, candidate_world_triangles, sampled_conifer_renderers, added_visual_only_forest_instances;
            public int source_height_resolution, candidate_height_resolution, changed_city_height_samples;
            public bool previous_scene_untouched, city_terrain_collider_unchanged, world_source_mesh_unchanged;
            public bool native_unity_ab, art_approved, physical_mobile_profiled, published;
        }

        [Serializable] public class SculptParameters { public float x,z,radius_x,radius_z,height; }
        [Serializable] public class SculptAction { public string type,target; public SculptParameters parameters; }
        [Serializable] public class ArtistPlan {
            public string source_scene_sha256; public bool qwen_artist_authored; public SculptAction[] planned_actions;
        }
        static ArtistPlan activeArtistPlan;
        static string activeArtistPlanHash;
        static void LoadArtistPlan(string sceneHash)
        {
            activeArtistPlan=null;activeArtistPlanHash="";
            string path=Environment.GetEnvironmentVariable("VALORIA_GEOGRAPHY_PLAN");
            if(string.IsNullOrEmpty(path))return;
            Need(File.Exists(path),"Requested local artist plan missing");
            var plan=JsonUtility.FromJson<ArtistPlan>(File.ReadAllText(path));
            Need(plan!=null&&plan.qwen_artist_authored&&plan.source_scene_sha256==sceneHash,
                "Local artist decisions must match frozen actual scene");
            Need(plan.planned_actions!=null&&plan.planned_actions.Length>=4&&plan.planned_actions.Length<=8,
                "Expected four to eight bounded sculpt regions");
            foreach(string target in new[]{"city","world"})
                Need(plan.planned_actions.Count(a=>a.target==target)>=2&&
                    plan.planned_actions.Count(a=>a.target==target)<=4,"Two to four regions per landscape");
            foreach(var a in plan.planned_actions) {
                Need(a.type=="sculpt_region"&&(a.target=="city"||a.target=="world")&&a.parameters!=null,
                    "Only typed edits to existing terrain");
                var q=a.parameters;bool city=a.target=="city";
                Need(q.x>=(city?-80:895)&&q.x<=(city?80:1105)&&q.z>=(city?-75:-105)&&q.z<=(city?100:105)&&
                    q.radius_x>=(city?15:22)&&q.radius_x<=(city?40:65)&&
                    q.radius_z>=(city?18:25)&&q.radius_z<=(city?55:75)&&
                    q.height>=(city?1.5f:5)&&q.height<=(city?4:16),"Artist region outside safe metric domain");
            }
            activeArtistPlan=plan;activeArtistPlanHash=Sha(path);
        }
        static float ArtistHeight(string target,float x,float z)
        {
            float value=0;
            foreach(var a in activeArtistPlan.planned_actions.Where(a=>a.target==target)) {
                var q=a.parameters;
                value+=q.height*Mathf.Exp(-Mathf.Pow((x-q.x)/q.radius_x,2)-Mathf.Pow((z-q.z)/q.radius_z,2));
            }
            // Smooth, bounded height addition; no clipped plateau or new topology.
            return target=="city"?5f*(1f-Mathf.Exp(-value/5f)):24f*(1f-Mathf.Exp(-value/24f));
        }
        static MeshRenderer[] CityDecorativeProps(Terrain terrain)
        {
            Vector3 lo=terrain.transform.position,hi=lo+terrain.terrainData.size;
            return Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)
                .Where(r=>{
                    if(!r||!r.enabled||r.GetComponentInParent<Collider>()||
                       r.GetComponentInParent<WorldHotspot>()||
                       r.GetComponentsInChildren<Renderer>(true).Length!=1)return false;
                    string n=r.name.ToLowerInvariant();
                    bool natural=new[]{"pine","conifer","tree","rock","shrub","bush","grass","fern","ruin"}.Any(n.Contains);
                    return natural&&r.bounds.size.x<28&&r.bounds.size.z<28&&
                        r.bounds.center.x>lo.x&&r.bounds.center.x<hi.x&&r.bounds.center.z>lo.z&&r.bounds.center.z<hi.z;
                }).ToArray();
        }
        static Dictionary<Transform,Vector3> SeatCityDecoration(Terrain terrain,TerrainData original,TerrainData candidate)
        {
            var moved=new Dictionary<Transform,Vector3>();
            foreach(var r in CityDecorativeProps(terrain)) {
                Vector3 center=r.bounds.center;
                float u=(center.x-terrain.transform.position.x)/original.size.x;
                float v=(center.z-terrain.transform.position.z)/original.size.z;
                float delta=candidate.GetInterpolatedHeight(u,v)-original.GetInterpolatedHeight(u,v);
                if(Mathf.Abs(delta)<.001f)continue;
                moved[r.transform]=r.transform.position;
                r.transform.position+=Vector3.up*delta;
            }
            foreach(var group in Object.FindObjectsByType<LODGroup>(FindObjectsSortMode.None))group.RecalculateBounds();
            Debug.Log("VALORIA_GEOGRAPHY_CITY_DECORATION_SEATED "+moved.Count+" original visual props");
            return moved;
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
                // A road has NO shoulder outside its measured longitudinal
                // extent. Zero means touching the pavement: returning zero
                // here previously painted distant meadow as worn path and
                // suppressed real height sculpt outside the road.
                if (z < z0 || z > z1) return float.PositiveInfinity;
                var slice = section[Mathf.Clamp(Mathf.RoundToInt((z - z0) * 2), 0, section.Length - 1)];
                return Mathf.Max(slice.x - x, x - slice.y);
            }
            public bool OutsideDomainIsUnpainted()
            {
                return float.IsPositiveInfinity(Edge(0, z0 - 1f)) &&
                       float.IsPositiveInfinity(Edge(0, z1 + 1f));
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
        static TerrainData AuthorCityTerrain(ValoriaReferenceCleanGameplay city, Terrain terrain, out int authoredHeightSamples)
        {
            TerrainData data = Object.Instantiate(terrain.terrainData);
            data.name = "Valoria organic prairie / existing height and paths";
            var filters = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
            var roads = new[] { "Axial_Paving", "Outer_Road" }.Select(name =>
                new GeographicRoad(filters.Single(f => f.name == name && f.sharedMesh != null))).ToArray();
            Need(roads.All(r => r.OutsideDomainIsUnpainted()),
                "Measured road domain must not repaint distant meadow or block height sculpt");
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
                    float paintGrain = .85f + .15f * Mathf.PerlinNoise(x*.12f+49f,z*.13f+17f);
                     float paint = Mathf.Clamp01((.46f * shoulder + .28f * bank + .14f * contact) * paintGrain);
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
                        mix = Mathf.Clamp01(mix + detail * .26f);
                        alphas[iz, ix, 0] = green * (1f - mix);
                        alphas[iz, ix, 1] = green * mix;
                    }
                }
            data.SetAlphamaps(0, 0, alphas);

            // Source TerrainData still holds all original 257x257 samples. Low
            // meadow forms are authored only in open ground, not foundations,
            // road shoulders, playable river margins, rocks or tree roots.
            authoredHeightSamples = 0;
            int hn = data.heightmapResolution;
            var heights = data.GetHeights(0, 0, hn, hn);
            var movable = activeArtistPlan!=null ? new HashSet<MeshRenderer>(CityDecorativeProps(terrain)) : new HashSet<MeshRenderer>();
            var keepers = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Where(r => r != null && r.enabled && r.gameObject.activeSelf && !movable.Contains(r) &&
                    r.bounds.size.x < 28 && r.bounds.size.z < 28 &&
                    r.bounds.center.x >= terrain.transform.position.x &&
                    r.bounds.center.x <= terrain.transform.position.x + data.size.x &&
                    r.bounds.center.z >= terrain.transform.position.z &&
                    r.bounds.center.z <= terrain.transform.position.z + data.size.z)
                .Select(r => new Vector2(r.bounds.center.x, r.bounds.center.z)).ToArray();
            float maximumDelta = 0;
            for (int iz = 0; iz < hn; iz++)
                for (int ix = 0; ix < hn; ix++)
                {
                    float x = terrain.transform.position.x + data.size.x * ix / (hn - 1f);
                    float z = terrain.transform.position.z + data.size.z * iz / (hn - 1f);
                    // Outer city TerrainData bounds are shared with the original
                    // landscape. Never lift border vertices or create abrupt cuts
                    // between the authored meadow and untouched surroundings.
                    if (ix == 0 || iz == 0 || ix == hn-1 || iz == hn-1) continue;
                    float edgeDistance = Mathf.Min(
                        Mathf.Min(x - terrain.transform.position.x,
                                  terrain.transform.position.x + data.size.x - x),
                        Mathf.Min(z - terrain.transform.position.z,
                                  terrain.transform.position.z + data.size.z - z));
                    float smoothBorder = Smooth(2f, 16f, edgeDistance);
                    float roadSafe = 1f;
                    foreach (var road in roads)
                    {
                        float edge = road.Edge(x, z);
                        roadSafe = Mathf.Min(roadSafe, Smooth(3.6f, 9.5f, edge));
                    }
                    float river = -23f + 1.5f * Mathf.Sin(x * .065f) +
                        .85f * Mathf.Sin(x * .19f + .4f) * Smooth(6, 17, Mathf.Abs(x));
                    float waterSafe = Smooth(13f, 25f, Mathf.Abs(z - river));
                    float fortressSafe = Smooth(27f, 42f,
                        Mathf.Sqrt(x * x + (z - 30f) * (z - 30f)));
                    float sitesSafe = 1f;
                    foreach (var anchor in keepers)
                    {
                        float distance = Vector2.Distance(new Vector2(x, z), anchor);
                        if (distance < 11f)
                            sitesSafe = Mathf.Min(sitesSafe, Smooth(4.5f, 11f, distance));
                    }
                    // Protect the exact complete footprints of the sawmill and
                    // barracks even when they are locked/inactive at Bastion I.
                    // Parent inactive states must not make foundations float.
                    foreach (var footprint in buildings)
                    {
                        float dx = Mathf.Max(Mathf.Abs(x - footprint.center.x) - footprint.extents.x, 0f);
                        float dz = Mathf.Max(Mathf.Abs(z - footprint.center.z) - footprint.extents.z, 0f);
                        sitesSafe = Mathf.Min(sitesSafe,
                            Smooth(4f, 12f, Mathf.Sqrt(dx * dx + dz * dz)));
                    }
                    float westMeadow = 1.15f * Mathf.Exp(
                        -Mathf.Pow((x + 51f) / 32f, 2f) -
                        Mathf.Pow((z - 52f) / 55f, 2f));
                    float eastMeadow = 1.00f * Mathf.Exp(
                        -Mathf.Pow((x - 54f) / 39f, 2f) -
                        Mathf.Pow((z - 59f) / 56f, 2f));
                    float southwestMeadow = .76f * Mathf.Exp(
                        -Mathf.Pow((x + 57f) / 39f, 2f) -
                        Mathf.Pow((z + 57f) / 40f, 2f));
                    float grain = (Noise(x, z) - .5f) * .22f;
                    float authoredHeight=activeArtistPlan!=null?ArtistHeight("city",x,z):westMeadow+eastMeadow+southwestMeadow+grain;
                    float delta = authoredHeight *
                        smoothBorder * roadSafe * waterSafe * fortressSafe * sitesSafe;
                    if (delta < .012f) continue;
                    heights[iz, ix] = Mathf.Clamp01(heights[iz, ix] +
                        delta / data.size.y);
                    maximumDelta = Mathf.Max(maximumDelta, delta);
                    authoredHeightSamples++;
                }
            Need(authoredHeightSamples > 1000 && maximumDelta <= (activeArtistPlan!=null?5f:1.4f),
                "Open meadow relief must remain bounded and visibly geometric");
            data.SetHeights(0, 0, heights);
            data.SyncHeightmap();
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
            int resolution=Mathf.Min(2048,Mathf.Max(texture.width,texture.height));
            var rt = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            try
            {
                Graphics.Blit(texture, rt);
                RenderTexture.active = rt;
                var sample = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Repeat };
                sample.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                sample.Apply();
                return sample;
            }
            finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(rt); }
        }
        static Mesh AuthorWorldMesh(ValoriaIntegratedWorld host, MeshFilter source, WorldHotspot[] spots)
        {
            var original = source.sharedMesh;
            var next = Object.Instantiate(original);
            next.name = "Region1 continuous authored macrorelief / original 4X topology";
            var v = original.vertices;
            var warped = new Vector3[v.Length];
            var b = source.GetComponent<Renderer>().bounds;
            Need(b.size.x > 100 && b.size.z > 100, "Unexpected Region1 real terrain extent");
            // Preserve contact at the feet of existing mature trees. Their
            // meshes are NOT relocated by this visual-only landform authoring.
            // Candidate trees are grounded separately on the candidate mesh.
            var existingTreeBases = host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true)
                .Where(rr => rr.enabled &&
                    (rr.name.StartsWith("Tree01_LOD", StringComparison.OrdinalIgnoreCase) ||
                     rr.name == "Near branches" ||
                     rr.name.StartsWith("Forest retained", StringComparison.OrdinalIgnoreCase) ||
                     rr.name.StartsWith("Conifer", StringComparison.OrdinalIgnoreCase)) &&
                    rr.bounds.size.y > 3f && rr.bounds.size.y < 28f &&
                    rr.bounds.center.x >= b.min.x && rr.bounds.center.x <= b.max.x &&
                    rr.bounds.center.z >= b.min.z && rr.bounds.center.z <= b.max.z)
                .Select(rr => new Vector2(rr.bounds.center.x, rr.bounds.center.z))
                .Distinct().ToArray();
            Need(existingTreeBases.Length > 12, "Missing retained forest anchors for 4X terrain contact protection");
            var fixedProps=host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true)
                .Where(rr=>rr.enabled&&rr.bounds.size.x<65&&rr.bounds.size.z<65&&
                    rr.bounds.center.x>b.min.x&&rr.bounds.center.x<b.max.x&&
                    rr.bounds.center.z>b.min.z&&rr.bounds.center.z<b.max.z)
                .Select(rr=>new Vector3(rr.bounds.center.x,rr.bounds.center.z,
                    Mathf.Max(rr.bounds.extents.x,rr.bounds.extents.z))).ToArray();
            for (int i = 0; i < v.Length; i++)
            {
                Vector3 world = source.transform.TransformPoint(v[i]);
                float nearest = spots.Min(h => PlanarDistance(world, h.transform.position));
                float anchor = Smooth(10f, 28f, nearest);
                // Wide shallow ridges extend the existing eastern quarry geology
                // into surrounding open grass. Existing POIs and roads remain in place.
                // Author three low, connected macroforms around existing
                // geographic landmarks, rather than noise over the entire map.
                // One readable topographic shoulder joining the retained
                // quarry/existing rock formation. Unlike broad random noise,
                // this is a coherent landform, with its original quarry base
                // and sector edge untouched by the existing keep-outs.
                float quarryFoothill = 7.2f * Mathf.Exp(
                    -Mathf.Pow((world.x - 1092f) / 61f, 2f) -
                    Mathf.Pow((world.z + 73f) / 73f, 2f));
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
                float nearestTrunkSqr = float.PositiveInfinity;
                foreach (var tree in existingTreeBases)
                {
                    float dx = world.x - tree.x, dz = world.z - tree.y;
                    nearestTrunkSqr = Mathf.Min(nearestTrunkSqr, dx * dx + dz * dz);
                }
                float trunkSafe = Smooth(2.5f, 8f, Mathf.Sqrt(nearestTrunkSqr));
                if(activeArtistPlan!=null) {
                    float propDistance=float.PositiveInfinity;
                    foreach(var prop in fixedProps)
                        propDistance=Mathf.Min(propDistance,Vector2.Distance(
                            new Vector2(world.x,world.z),new Vector2(prop.x,prop.y))-prop.z);
                    trunkSafe=Mathf.Min(trunkSafe,Smooth(3f,18f,propDistance));
                }
                float geography=activeArtistPlan!=null?ArtistHeight("world",world.x,world.z):
                    quarryFoothill+northForestRise+corruptionValley+reliefGrain;
                float deltaY = edgeProtect * anchor * rockProtect * trunkSafe * geography;
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
            const int size = 2048;
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
                        // The same authored foothill controls material change:
                        // subtle rock/dry-soil only where the terrain actually
                        // rises toward the retained quarry formation.
                        float upland = Mathf.Exp(
                            -Mathf.Pow((wx - 1092f) / 62f, 2f) -
                            Mathf.Pow((wz + 73f) / 74f, 2f));
                        float grain = .65f + .35f * Mathf.PerlinNoise(wx*.14f+72,wz*.14f+33);
                        float earth = Mathf.Clamp01(upland * grain * .37f + quarry * .2f);
                        baseColor = Color.Lerp(baseColor,
                            new Color(baseColor.r * .94f, baseColor.g * .81f, baseColor.b * .68f),
                            earth);
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

        [Serializable] public class DonorDescription
        {
            public string name; public Vector3 center,size;
            public int mesh_triangles,source_layer;
        }
        [Serializable] public class DonorInventory {public DonorDescription[] sources;}
        static GameObject ExpandExistingForest(ValoriaIntegratedWorld host, MeshFilter worldGround, Mesh candidateSurface, out int additions)
        {
            var forest = Hotspots(host).Single(p => p.Id == "forest-valoria").transform.position;
            // The seven 'conifer' renderers found previously are saplings:
            // copying them 47 times produced tiny specks at 4X zoom.
            // Select only EXISTING mature trees by their actual world bounds,
            // excluding the nearby player city, quarry and scripted hotspots.
            Vector3 cityPosition=Hotspots(host).Single(p=>p.Id=="valoria-map-city").transform.position;
            var original = host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true)
                .Where(x => x.enabled && x.GetComponent<MeshFilter>() != null &&
                     PlanarDistance(x.bounds.center,forest)<49f &&
                     PlanarDistance(x.bounds.center,cityPosition)>22f &&
                     x.bounds.size.y >= 5f && x.bounds.size.y <= 32f &&
                     x.bounds.size.x >= 2f && x.bounds.size.x <= 19f &&
                     x.bounds.size.z >= 2f && x.bounds.size.z <= 19f)
                .OrderByDescending(x=>x.bounds.size.y).ThenBy(x=>x.name).ToArray();
            var inventory = original.Take(100).Select(x=> new DonorDescription{
                name=x.name, center=x.bounds.center, size=x.bounds.size,
                mesh_triangles=x.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3,
                source_layer=x.gameObject.layer
            }).ToArray();
            File.WriteAllText(Path.Combine(Evidence,"forest-source-inventory.json"),
                JsonUtility.ToJson(new DonorInventory{sources=inventory},true));
            Need(original.Length >= 4 && original.Length <= 150,
                "Measured mature source trees not found; reject rather than copying miniature understory: "+original.Length);
            var root = new GameObject("Forest ecological continuity / approved source meshes only");
            root.transform.SetParent(host.WorldRoot.transform, false);
            // Contact must be measured against the FINAL visual macroterrain,
            // not the retained original world mesh. Otherwise new canopy trunks
            // appear to float when the authored forest rise lifts the surface.
            Need(candidateSurface != null &&
                 candidateSurface.vertexCount == worldGround.sharedMesh.vertexCount,
                 "Forest contact requires topologically identical candidate ground");
            var terrainVerts = candidateSurface.vertices
                .Select(q => worldGround.transform.TransformPoint(q)).ToArray();
            var terrainTriangles = candidateSurface.triangles;
            float GroundHeight(float x,float z)
            {
                // Barycentric height from the EXACT candidate triangles:
                // nearest-vertex lookups can bury/floats trees on slopes.
                // This is read-only geometry mathematics, not a new collider.
                const float tolerance = .0002f;
                for(int i=0;i<terrainTriangles.Length;i+=3)
                {
                    Vector3 a=terrainVerts[terrainTriangles[i]],
                        b=terrainVerts[terrainTriangles[i+1]],
                        c=terrainVerts[terrainTriangles[i+2]];
                    float denominator=(b.z-c.z)*(a.x-c.x)+(c.x-b.x)*(a.z-c.z);
                    if(Mathf.Abs(denominator)<.0000001f)continue;
                    float aa=((b.z-c.z)*(x-c.x)+(c.x-b.x)*(z-c.z))/denominator;
                    float bb=((c.z-a.z)*(x-c.x)+(a.x-c.x)*(z-c.z))/denominator;
                    float cc=1f-aa-bb;
                    if(aa>=-tolerance&&bb>=-tolerance&&cc>=-tolerance)
                        return aa*a.y+bb*b.y+cc*c.y;
                }
                throw new InvalidOperationException(
                    "Tree site outside exact candidate macroterrain triangle coverage");
            }
            var placed = new List<Vector3>(original.Select(x => x.bounds.center));
            // Three irregular, geographically connected forest shoulders:
            // the current source has attractive trees but too much open board
            // between canopy groups. Extend existing canopy clusters, not POIs.
            var city = Hotspots(host).Single(p => p.Id == "valoria-map-city").transform.position;
            // Revise native261's isolated NE saplings: new adult crowns now
            // extend measured existing canopy clusters, not the map edge.
            Vector2[] lobes = {
                new Vector2(-17f,10f), new Vector2(-6f,22f),
                new Vector2(16f,14f), new Vector2(19f,-12f),
                new Vector2(-12f,-17f)
            };
            var playable = worldGround.GetComponent<Renderer>().bounds;
            var otherPOIs = Hotspots(host).Where(p => p.Id != "forest-valoria")
                .Select(p => p.transform.position).ToArray();
            // A retained open corridor from Valoria's gate to the lumber
            // destination remains visible and usable in the 4X camera.
            float DistanceToRoad(Vector3 q)
            {
                Vector2 a = new Vector2(city.x,city.z);
                Vector2 b = new Vector2(forest.x,forest.z);
                Vector2 c = new Vector2(q.x,q.z);
                float t=Mathf.Clamp01(Vector2.Dot(c-a,b-a)/Mathf.Max(.001f,(b-a).sqrMagnitude));
                return Vector2.Distance(c,Vector2.Lerp(a,b,t));
            }
            additions=0;
            int considered = 0;
            for(int l=0;l<lobes.Length;l++)
            // Search further along each existing ecological lobe when the
            // inner sites are occupied. Keep the 21-tree cap, 7.7m spacing,
            // road/POI buffers and exact ground contact unchanged.
            for(int j=0;j<64;j++)
            {
                int index=l*64+j;
                // Golden-angle sampling plus bounded jitter avoids rows,
                // perfect rings or repeated prefab silhouettes.
                float angle = index*2.3999632f + l*.7f;
                float spread=3.5f + Mathf.Sqrt(j+.4f)*2.95f;
                float jitter = (Mathf.PerlinNoise(index*.37f+14,l*.71f+29)-.5f)*5f;
                Vector2 offset=lobes[l] + new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(spread+jitter);
                Vector3 target=forest+new Vector3(offset.x,0,offset.y);
                if(target.x<playable.min.x+9||target.x>playable.max.x-9||
                   target.z<playable.min.z+9||target.z>playable.max.z-9)continue;
                if(DistanceToRoad(target)<10f)continue;
                if(otherPOIs.Any(q=>PlanarDistance(q,target)<15f))continue;
                if(placed.Any(v=>PlanarDistance(v,target)<7.7f))continue;
                float canopy = Mathf.PerlinNoise(target.x*.085f+21,target.z*.085f+67);
                if(canopy<.25f)continue; // irregular clearings, not solid wall
                if(additions>=21)continue;
                var donor=original[(l*3+j)%original.Length];
                considered++;
                var meshFilter=donor.GetComponent<MeshFilter>();
                if(!meshFilter||!meshFilter.sharedMesh)continue;
                float baseToCenter=donor.bounds.center.y-donor.bounds.min.y;
                target.y=GroundHeight(target.x,target.z)+baseToCenter+.045f;
                var obj=new GameObject("Existing conifer canopy / forest ecological lobe "+l+" tree "+j);
                obj.layer=donor.gameObject.layer;
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
                // Preserve the already-reviewed two-stage Unity LOD contract.
                // Copying Near branches or Strategic silhouette alone at every
                // distance needlessly cost mobile triangles or sacrifices detail.
                if((donor.name=="Near branches"||donor.name=="Strategic silhouette")
                    && donor.transform.parent!=null)
                {
                    string partnerName=donor.name=="Near branches"
                        ?"Strategic silhouette":"Near branches";
                    Transform partner=donor.transform.parent.Find(partnerName);
                    if(partner!=null)
                    {
                        var sourceRenderer=partner.GetComponent<MeshRenderer>();
                        var sourceMesh=partner.GetComponent<MeshFilter>();
                        Need(sourceRenderer!=null && sourceMesh!=null &&
                            sourceMesh.sharedMesh!=null,
                            "Retained near/far conifer partner lacks original mesh");
                        var other=new GameObject("Retained "+partnerName+" / mobile LOD");
                        other.layer=partner.gameObject.layer;
                        other.transform.SetParent(obj.transform,false);
                        other.transform.localPosition=Vector3.zero;
                        other.transform.localRotation=Quaternion.identity;
                        other.transform.localScale=Vector3.one;
                        other.AddComponent<MeshFilter>().sharedMesh=sourceMesh.sharedMesh;
                        var alternate=other.AddComponent<MeshRenderer>();
                        alternate.sharedMaterials=sourceRenderer.sharedMaterials;
                        alternate.shadowCastingMode=sourceRenderer.shadowCastingMode;
                        alternate.receiveShadows=sourceRenderer.receiveShadows;
                        alternate.lightProbeUsage=sourceRenderer.lightProbeUsage;
                        alternate.reflectionProbeUsage=sourceRenderer.reflectionProbeUsage;
                        alternate.renderingLayerMask=sourceRenderer.renderingLayerMask;
                        MeshRenderer near=donor.name=="Near branches"?rr:alternate;
                        MeshRenderer far=donor.name=="Strategic silhouette"?rr:alternate;
                        var nearMesh=near.GetComponent<MeshFilter>().sharedMesh;
                        var farMesh=far.GetComponent<MeshFilter>().sharedMesh;
                        Need(farMesh.triangles.Length<=nearMesh.triangles.Length,
                            "Repeated forest silhouette is more expensive than near variant");
                        var lod=obj.AddComponent<LODGroup>();
                        lod.SetLODs(new[]{new LOD(.16f,new Renderer[]{near}),
                            new LOD(.012f,new Renderer[]{far})});
                        lod.RecalculateBounds();
                    }
                }
                Vector3 actualCenter=rr.bounds.center;
                Need((host.WorldCamera.cullingMask & (1<<obj.layer)) != 0,
                     "Mature canopy not included in real strategic camera culling mask");
                Need(PlanarDistance(actualCenter,target)<3.5f &&
                     Mathf.Abs(rr.bounds.min.y-GroundHeight(target.x,target.z))<.75f,
                     "Cloned conifer must actually render grounded inside surveyed forest");
                // Absolutely no collider, WorldHotspot, army marker, or LOD
                // owner is cloned: only compatible visual source geometry.
                placed.Add(actualCenter);additions++;
            }
            Need(additions>=15&&additions<=21,"Forest silhouettes are too sparse or excessively expensive: "+additions+" mature trees from "+considered+" surveyed locations");
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
            var movedCityDecoration=new Dictionary<Transform,Vector3>();
            try
            {
                Directory.CreateDirectory(Evidence);
                string sceneHash = Sha(ScenePath);
                LoadArtistPlan(sceneHash);
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
                newCity = AuthorCityTerrain(city, terrain, out int cityHeightChanges);
                if(activeArtistPlan!=null)movedCityDecoration=SeatCityDecoration(terrain,originalTerrain,newCity);
                worldMesh = AuthorWorldMesh(host, ground, Hotspots(host));
                forestVisual = ExpandExistingForest(host, ground, worldMesh, out int forestAdditions);
                albedo = AuthorWorldAlbedo(host, ground, out int treeCount);
                var existingWorldMat = oldRenderer.sharedMaterial;
                Need(existingWorldMat && existingWorldMat.HasProperty("_BaseMap"), "Original URP world material unavailable");
                worldMat = new Material(existingWorldMat)
                    { name = "Region1 original roads/biomes retained plus organic dressing" };
                Need(worldMat.shader != null, "URP Lit shader unavailable");
                worldMat.SetTexture("_BaseMap", albedo);
                // Keep original tint/roughness/metallic/normal properties unchanged.
                terrain.terrainData = newCity;
                // The visible sculpt and its collision must use the SAME
                // cloned TerrainData while testing. Both revert on rejection.
                originalCollider.terrainData = newCity;
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
                var afterPOI = Hotspots(host).Select(s => s.Id + "=" + s.transform.position.ToString("R")).ToArray();
                Need(beforePOI.SequenceEqual(afterPOI), "Original POI identities or positions changed");
                Need(host.WorldRoot.GetComponentsInChildren<Collider>(true).Length == beforeColliders,
                    "New gameplay collider introduced");
                Need(originalCollider.terrainData == newCity && ground.sharedMesh == originalWorldMesh,
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
                    source_scene_sha256 = sceneHash, tool_sha256 = toolHash, artist_plan_sha256=activeArtistPlanHash,
                    method = "Open-meadow TerrainData sculpt with synced trial physics; Region1 retained-tree roots anchored to original ground, mature forest additions seated on the candidate visual macroterrain; original atlas/POIs intact",
                    world_ground_source = AssetDatabase.GetAssetPath(originalWorldMesh),
                    original_poi_ids = beforePOI, after_poi_ids = afterPOI,
                    images = imgs, image_sha256 = imgs.Select(x => Sha(Path.Combine(Evidence, x))).ToArray(),
                    source_world_triangles = beforeWorldTriangles,
                    candidate_world_triangles = worldMesh.triangles.Length / 3,
                    sampled_conifer_renderers = treeCount,
                    added_visual_only_forest_instances = forestAdditions,
                    source_height_resolution = originalTerrain.heightmapResolution,
                    candidate_height_resolution = newCity.heightmapResolution,
                    changed_city_height_samples = cityHeightChanges,
                    previous_scene_untouched = true,
                    city_terrain_collider_unchanged = false, // trial collider is safely rebound to the matching candidate asset
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
                    foreach(var entry in movedCityDecoration)if(entry.Key)entry.Key.position=entry.Value;
                    if (terrain && originalTerrain) {
                        var originalPhysics = terrain.GetComponent<TerrainCollider>();
                        if (originalPhysics) originalPhysics.terrainData = originalTerrain;
                        terrain.terrainData = originalTerrain; terrain.Flush();
                    }
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
