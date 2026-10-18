#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Eldoria.Application;
using Eldoria.Domain;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.World4XPrototype
{
    // Explicit Editor-only entry. Never starts PlayMode, SliceBoot or a real save store.
    public static class ZoneOneNativeCandidate
    {
        const string Root = "Assets/Eldoria/ArtTests/World4XZone1/NativeValidated";
        static readonly List<string> Errors = new List<string>();
        static void Log(string message, string stack, LogType type)
        { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) Errors.Add(message); }
        public static async void Build()
        {
            UnityEngine.Application.logMessageReceived += Log;
            string output = Environment.GetEnvironmentVariable("ELDORIA_ZONEONE_CAPTURE_OUT");
            try
            {
                string before = Environment.GetEnvironmentVariable("ELDORIA_ZONEONE_GROUND_BEFORE");
                string after = Environment.GetEnvironmentVariable("ELDORIA_ZONEONE_GROUND_AFTER");
                Need(File.Exists(before) && File.Exists(after), "Exact source pair missing");
                Need(!string.IsNullOrEmpty(output) && !Directory.Exists(output), "New explicit evidence folder required");
                Need(GraphicsSettings.defaultRenderPipeline != null, "Existing project URP required");
                Directory.CreateDirectory(output);
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var report = new Report { sourceCommit = Environment.GetEnvironmentVariable("GITHUB_SHA"), beforeSha256 = Hash(before), afterSha256 = Hash(after), engine = UnityEngine.Application.unityVersion };
                report.hostChecks = VerifyHost(); report.cameraChecks = VerifyCamera();
                Vector3 focus = new Vector3(-18, 0, 24);
                var captures = new List<Capture>();
                string savedScene = null;
                for (int version = 0; version < 2; version++)
                {
                    var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    var root = new GameObject("World4X retained source " + version);
                    using (var importer = new GltfImport(null, new UninterruptedDeferAgent()))
                    {
                        string input = version == 0 ? before : after;
                        Need(await importer.Load(new Uri(Path.GetFullPath(input)).AbsoluteUri), "GLB load failed");
                        Need(await importer.InstantiateMainSceneAsync(root.transform), "GLB instantiate failed");
                        foreach (var light in root.GetComponentsInChildren<Light>()) light.enabled = false;
                        foreach (var cam in root.GetComponentsInChildren<Camera>()) cam.enabled = false;
                        var renderers = root.GetComponentsInChildren<Renderer>();
                        Need(renderers.Length > 0, "No imported renderers");
                        foreach (var r in renderers)
                            foreach (var m in r.sharedMaterials)
                                Need(m != null && m.shader != null && !m.shader.name.Contains("InternalError"), "Invalid imported material");
                        Bounds bounds = renderers[0].bounds;
                        foreach (var r in renderers) bounds.Encapsulate(r.bounds);
                        Need(bounds.size.x > 200 && bounds.size.z > 200, "Authored ground axes or scale wrong");
                        var sun = new GameObject("World4X comparison sun").AddComponent<Light>();
                        sun.type = LightType.Directional; sun.intensity = 1.45f; sun.color = new Color(1, .94f, .84f);
                        sun.shadows = LightShadows.Soft; sun.transform.rotation = Quaternion.Euler(40, -35, 0);
                        RenderSettings.ambientMode = AmbientMode.Trilight;
                        RenderSettings.ambientSkyColor = new Color(.52f, .64f, .76f);
                        RenderSettings.ambientEquatorColor = new Color(.35f, .39f, .32f);
                        RenderSettings.ambientGroundColor = new Color(.18f, .17f, .13f);
                        RenderSettings.fog = false; RenderSettings.sun = sun;
                        var camera = new GameObject("World4X only player camera").AddComponent<Camera>();
                        camera.tag = "MainCamera"; camera.clearFlags = CameraClearFlags.SolidColor;
                        camera.backgroundColor = new Color(.20f, .23f, .25f); camera.allowHDR = false;
                        var nav = camera.gameObject.AddComponent<ZoneOneStrategicCamera>();
                        nav.minimum = new Vector2(bounds.min.x, bounds.min.z); nav.maximum = new Vector2(bounds.max.x, bounds.max.z);
                        nav.edgeMargin = 10;
                        var views = new[] { (0, "global", 1100, 800), (1, "regional", 1100, 800), (2, "local", 1100, 800), (1, "mobile", 540, 960) };
                        foreach (var view in views)
                        {
                            camera.aspect = view.Item3 / (float)view.Item4;
                            nav.PanTo(focus); nav.SetZoomLevel(view.Item1, true);
                            string name = (version == 0 ? "before-" : "after-") + view.Item2 + "-unity.png";
                            var c = CaptureImage(camera, Path.Combine(output, name), view.Item3, view.Item4);
                            c.name = name; c.level = view.Item2; c.sourceSha256 = Hash(input); c.renderers = renderers.Length;
                            captures.Add(c);
                        }
                        if (version == 1)
                        {
                            string generated = Root + "/run-" + Environment.GetEnvironmentVariable("GITHUB_RUN_ID");
                            Need(!Directory.Exists(generated), "Never overwrite an earlier native candidate");
                            Directory.CreateDirectory(generated);
                            PersistClones(root, generated);
                            foreach (var filter in root.GetComponentsInChildren<MeshFilter>())
                            { var col = filter.gameObject.GetComponent<MeshCollider>() ?? filter.gameObject.AddComponent<MeshCollider>(); col.sharedMesh = filter.sharedMesh; }
                            camera.aspect = 1100f / 800; nav.PanTo(focus); nav.SetZoomLevel(1, true);
                            PrefabUtility.SaveAsPrefabAsset(root, generated + "/ZoneOneGround.prefab");
                            AssetDatabase.SaveAssets(); savedScene = generated + "/ZoneOneGround_EditorReview.unity";
                            Need(EditorSceneManager.SaveScene(scene, savedScene), "Scene save failed");
                            report.prefab = generated + "/ZoneOneGround.prefab"; report.scene = savedScene;
                            report.triangles = root.GetComponentsInChildren<MeshFilter>().Sum(f => f.sharedMesh.triangles.Length / 3);
                        }
                        // Instances only use cloned/persisted resources by this point.
                        Object.DestroyImmediate(root);
                    }
                }
                for (int i = 0; i < 4; i++)
                {
                    Need(captures[i].width == captures[i + 4].width && captures[i].height == captures[i + 4].height, "A/B dimensions changed");
                    Need(MatrixClose(captures[i].camera, captures[i + 4].camera) && MatrixClose(captures[i].projection, captures[i + 4].projection), "A/B camera changed");
                }
                EditorSceneManager.OpenScene(savedScene, OpenSceneMode.Single);
                var loaded = Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
                Need(loaded.Length > 0 && loaded.All(f => f.sharedMesh != null && AssetDatabase.Contains(f.sharedMesh)), "Saved scene lost source geometry after importer disposal");
                Need(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Count(c => c.enabled) == 1, "Duplicate native camera");
                Need(Errors.Count == 0, "Unity errors: " + string.Join(" | ", Errors));
                report.captures = captures.ToArray(); report.savedSceneReopened = true;
                report.status = "NATIVE_IMPORT_CAPTURE_HOST_CAMERA_PASS_ART_REVIEW_REQUIRED";
                File.WriteAllText(Path.Combine(output, "native-result.json"), JsonUtility.ToJson(report, true));
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                if (!string.IsNullOrEmpty(output)) { Directory.CreateDirectory(output); File.WriteAllText(Path.Combine(output, "native-failure.txt"), e + "\n" + string.Join("\n", Errors)); }
                Debug.LogException(e); EditorApplication.Exit(1);
            }
        }
        static int VerifyHost()
        {
            var fake = new SpyGateway(); int returned = 0, refreshed = 0;
            var host = new ZoneOneGatewayHost(fake, () => returned++, () => 100);
            host.Refreshed += _ => refreshed++;
            Need(ReferenceEquals(fake.State, host.Snapshot()), "Host substituted its own state");
            Need(host.Inspect("forest-valoria").Enabled, "Forest hint incorrect");
            host.Execute("forest-valoria", "same-request");
            Need(fake.Last.Kind == "Gather" && fake.Last.TargetId == "forest-valoria" && fake.Last.Id == "same-request", "Command identity changed");
            Need(fake.Last.ActorId == fake.State.PlayerId && fake.Last.ExpectedRevision == fake.State.Revision, "Wrong session owner or revision");
            host.Execute("forest-valoria", "same-request"); Need(fake.Last.Id == "same-request", "Retry ID not preserved");
            int writes = fake.Commands; host.Execute("future-food-node", "unknown"); Need(fake.Commands == writes, "Unintegrated node wrote to canonical resources");
            host.Execute("valoria-map-city", "return"); Need(returned == 1 && fake.Commands == writes, "Return generated a gameplay write");
            host.Refresh(); Need(fake.Advances == 0, "Host silently advanced shared timers");
            host.Refresh(true); Need(fake.Advances == 1 && refreshed > 0, "Explicit timer advance missing");
            Need(Math.Abs(ZoneOneGatewayHost.TravelFraction(0, TimeSpan.TicksPerSecond * 5, 10) - .5) < 1e-6, "March interpolation incorrect");
            Need(ZoneOneGatewayHost.TravelFraction(100, 0, 10) == 1, "Late march not bounded");
            return 11;
        }
        static int VerifyCamera()
        {
            var obj = new GameObject("temporary camera test"); var camera = obj.AddComponent<Camera>();
            var nav = obj.AddComponent<ZoneOneStrategicCamera>(); int checks = 0;
            foreach (float aspect in new[] { 1100f / 800, 540f / 960 })
            {
                camera.aspect = aspect;
                foreach (int level in new[] { 0, 1, 2 }) foreach (int x in new[] { -1000, 1000 }) foreach (int z in new[] { -1000, 1000 })
                {
                    nav.PanTo(new Vector3(x, 0, z)); nav.SetZoomLevel(level, true);
                    foreach (var p in new[] { new Vector3(0, 0), new Vector3(1, 0), new Vector3(0, 1), new Vector3(1, 1) })
                    {
                        var ray = camera.ViewportPointToRay(p); var plane = new Plane(Vector3.up, Vector3.zero);
                        Need(plane.Raycast(ray, out float d), "Camera does not see ground"); var hit = ray.GetPoint(d);
                        Need(hit.x >= nav.minimum.x - .01f && hit.x <= nav.maximum.x + .01f && hit.z >= nav.minimum.y - .01f && hit.z <= nav.maximum.y + .01f, "Viewport escaped authored terrain"); checks++;
                    }
                }
                nav.PanTo(Vector3.zero); nav.SetZoomLevel(1, true);
                Vector2 screen = new Vector2(camera.pixelWidth * .52f, camera.pixelHeight * .53f);
                Need(nav.GroundPoint(screen, out var before), "Zoom anchor missing");
                nav.ZoomAt(screen, .75f, true); Need(nav.GroundPoint(screen, out var after) && Vector3.Distance(before, after) < .01f, "Zoom moved its world focus"); checks++;
            }
            Object.DestroyImmediate(obj); return checks;
        }
        static Capture CaptureImage(Camera camera, string path, int width, int height)
        {
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 2 };
            var previous = RenderTexture.active; var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0); pixels.Apply();
                var data = pixels.GetPixels32(); double min = 255, max = 0; int magenta = 0;
                foreach (var c in data) { double v = (c.r + c.g + c.b) / 3.0; min = Math.Min(min, v); max = Math.Max(max, v); if (c.r > 220 && c.g < 40 && c.b > 220) magenta++; }
                Need(max - min > 12 && magenta < data.Length / 100, "Blank or missing-shader native capture");
                File.WriteAllBytes(path, pixels.EncodeToPNG());
                return new Capture { width = width, height = height, camera = camera.cameraToWorldMatrix, projection = camera.projectionMatrix, sha256 = Hash(path), zoom = camera.orthographicSize };
            }
            finally { RenderTexture.active = previous; camera.targetTexture = null; Object.DestroyImmediate(pixels); rt.Release(); Object.DestroyImmediate(rt); }
        }
        static void PersistClones(GameObject root, string folder)
        {
            var meshes = new Dictionary<Mesh, Mesh>(); var materials = new Dictionary<Material, Material>(); var textures = new Dictionary<Texture, Texture>(); int index = 0;
            foreach (var f in root.GetComponentsInChildren<MeshFilter>())
            {
                if (!meshes.TryGetValue(f.sharedMesh, out var clone)) { clone = Object.Instantiate(f.sharedMesh); AssetDatabase.CreateAsset(clone, folder + "/mesh-" + index++ + ".asset"); meshes.Add(f.sharedMesh, clone); }
                f.sharedMesh = clone;
            }
            foreach (var r in root.GetComponentsInChildren<Renderer>())
            {
                var assigned = r.sharedMaterials;
                for (int i = 0; i < assigned.Length; i++)
                {
                    Material source = assigned[i];
                    if (!materials.TryGetValue(source, out var clone))
                    {
                        clone = new Material(source);
                        foreach (string property in source.GetTexturePropertyNames())
                        {
                            var t = source.GetTexture(property); if (t == null) continue;
                            if (!textures.TryGetValue(t, out var copy)) { copy = Object.Instantiate(t); AssetDatabase.CreateAsset(copy, folder + "/texture-" + index++ + ".asset"); textures.Add(t, copy); }
                            clone.SetTexture(property, copy);
                        }
                        AssetDatabase.CreateAsset(clone, folder + "/material-" + index++ + ".mat"); materials.Add(source, clone);
                    }
                    assigned[i] = clone;
                }
                r.sharedMaterials = assigned;
            }
        }
        static bool MatrixClose(Matrix4x4 a, Matrix4x4 b) { for (int i = 0; i < 16; i++) if (Mathf.Abs(a[i] - b[i]) > .0001f) return false; return true; }
        static void Need(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); }
        static string Hash(string file) { using (var s = SHA256.Create()) using (var f = File.OpenRead(file)) return BitConverter.ToString(s.ComputeHash(f)).Replace("-", "").ToLowerInvariant(); }
        sealed class SpyGateway : ICommandGateway
        {
            public PlayerState State = new PlayerState(); public GameCommand Last; public int Commands, Advances;
            public PlayerState Snapshot() => State;
            public CommandResult Execute(GameCommand command) { Last = command; Commands++; return new CommandResult(true, "test-only", State.Revision); }
            public bool Advance() { Advances++; return false; }
        }
        [Serializable] public class Capture { public string name, level, sourceSha256, sha256; public int width, height, renderers; public float zoom; public Matrix4x4 camera, projection; }
        [Serializable] public class Report { public string sourceCommit, beforeSha256, afterSha256, engine, status, prefab, scene; public int hostChecks, cameraChecks, triangles; public bool savedSceneReopened, physicalDeviceTested = false, artApproved = false, playModeTested = false; public Capture[] captures; }
    }
}
#endif
