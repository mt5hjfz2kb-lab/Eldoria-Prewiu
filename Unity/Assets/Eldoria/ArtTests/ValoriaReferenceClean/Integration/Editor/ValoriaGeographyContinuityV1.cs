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
        internal static void CityViews(ValoriaReferenceCleanGameplay city, string prefix)
        {
            var cam = city.PlayerCamera;
            var origPos = cam.transform.position;
            var origRot = cam.transform.rotation;
            float origZoom = cam.orthographicSize;
            float origAspect = cam.aspect;
            var groups = new[] { city.BastionOne, city.BastionTwo, city.Parcels.LeftBuilt, city.Parcels.RightBuilt };
            Need(groups.All(o => o != null), "Missing protected Bastion I–II or unlocked buildings");
            bool[] origActive = groups.Select(g => g.activeSelf).ToArray();
            try
            {
                for (int stage = 0; stage < 2; stage++)
                {
                    groups[0].SetActive(stage == 0);
                    groups[1].SetActive(stage == 1);
                    groups[2].SetActive(stage == 1);
                    groups[3].SetActive(stage == 1);
                    cam.transform.SetPositionAndRotation(origPos, origRot);
                    cam.orthographicSize = origZoom;
                    string suffix = stage == 0 ? "I" : "II";
                    CameraShot(cam, prefix + "-city-" + suffix + "-wide.png", 1600, 900);
                    CameraShot(cam, prefix + "-city-" + suffix + "-mobile.png", 900, 1600);
                }
            }
            finally
            {
                cam.transform.SetPositionAndRotation(origPos, origRot);
                cam.orthographicSize = origZoom;
                cam.aspect = origAspect;
                for (int i = 0; i < groups.Length; i++) groups[i].SetActive(origActive[i]);
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
