#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.World4XPrototype
{
    /// <summary>Only invoke on an available Unity runner; never alters Valoria or its assets.</summary>
    public static class ZoneOneSectorScene
    {
        const string Root = "Assets/Eldoria/ArtTests/World4XZone1";
        [MenuItem("Eldoria/Zone One/Open authored sector")]
        public static async void Open() { await Create(false); }
        public static async void BuildAndCapture()
        {
            try { await Create(true); EditorApplication.Exit(0); }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }
        /// <summary>Source terrain gate, with matching real URP cameras. Requires legitimate runner access.</summary>
        public static async void CaptureGroundPair()
        {
            try
            {
                string before = Environment.GetEnvironmentVariable("ELDORIA_ZONEONE_GROUND_BEFORE");
                string after = Environment.GetEnvironmentVariable("ELDORIA_ZONEONE_GROUND_AFTER");
                string output = Environment.GetEnvironmentVariable("ELDORIA_ZONEONE_CAPTURE_OUT");
                if (string.IsNullOrEmpty(output) || string.IsNullOrEmpty(before) || string.IsNullOrEmpty(after))
                    throw new InvalidOperationException("Explicit source pair and fresh capture output required");
                if (Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any())
                    throw new InvalidOperationException("Never replace earlier evidence");
                if (GraphicsSettings.defaultRenderPipeline == null)
                    throw new InvalidOperationException("Existing URP configuration required");
                Directory.CreateDirectory(output);
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var sun = new GameObject("Ground gate sun").AddComponent<Light>();
                sun.type = LightType.Directional; sun.intensity = 1.4f;
                sun.color = new Color(1, .92f, .78f); sun.shadows = LightShadows.Soft;
                sun.transform.rotation = Quaternion.Euler(42,-35,0);
                RenderSettings.ambientMode = AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = new Color(.52f,.64f,.76f);
                RenderSettings.ambientEquatorColor = new Color(.36f,.40f,.32f);
                RenderSettings.ambientGroundColor = new Color(.18f,.17f,.13f);
                RenderSettings.fog = false;
                var camera = new GameObject("Fixed ground gate camera").AddComponent<Camera>();
                camera.orthographic = true; camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.44f,.51f,.56f);
                Vector3 focus = Vector3.zero;
                int version = 0;
                foreach (string source in new[]{before,after})
                {
                    string absolute = Path.GetFullPath(source);
                    if (!File.Exists(absolute)) throw new FileNotFoundException("Ground source absent",absolute);
                    var root = new GameObject("Ground pair " + version);
                    using (var importer = new GltfImport(null,new UninterruptedDeferAgent()))
                    {
                        if (!await importer.Load(new Uri(absolute).AbsoluteUri) ||
                            !await importer.InstantiateMainSceneAsync(root.transform))
                            throw new InvalidOperationException("Ground GLB import failed");
                        var terrain = root.GetComponentsInChildren<Transform>().FirstOrDefault(
                            t=>t.name.Contains("ZoneOne continuous prairie source"));
                        if (terrain == null) throw new InvalidOperationException("Named continuous terrain missing");
                        var ground = terrain.GetComponentsInChildren<Renderer>();
                        if (ground.Length == 0) throw new InvalidOperationException("Ground renderers missing");
                        foreach (var renderer in root.GetComponentsInChildren<Renderer>())
                            renderer.enabled = ground.Contains(renderer);
                        foreach (var light in root.GetComponentsInChildren<Light>()) light.enabled = false;
                        foreach (var importedCamera in root.GetComponentsInChildren<Camera>()) importedCamera.enabled = false;
                        // Freeze focus from original import; never recenter candidate to hide deformation.
                        if (version == 0) focus = new Vector3(ground[0].bounds.center.x,0,ground[0].bounds.center.z);
                        foreach (var view in new[]{(1280,900,80f,"regional"),(390,844,80f,"mobile"),(1280,900,40f,"local")})
                        {
                            camera.orthographicSize = view.Item3;
                            camera.transform.position = focus + new Vector3(0,145,-130);
                            camera.transform.LookAt(focus);
                            camera.aspect = (float)view.Item1/view.Item2;
                            var rt = new RenderTexture(view.Item1,view.Item2,24);
                            var previous = RenderTexture.active;
                            var pixels = new Texture2D(view.Item1,view.Item2,TextureFormat.RGB24,false);
                            try
                            {
                                camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                                pixels.ReadPixels(new Rect(0,0,view.Item1,view.Item2),0,0); pixels.Apply();
                                string stem = (version==0?"before-":"after-") + view.Item4;
                                File.WriteAllBytes(Path.Combine(output,stem+"-unity.png"),pixels.EncodeToPNG());
                                File.WriteAllText(Path.Combine(output,stem+"-camera.json"),JsonUtility.ToJson(new CaptureIdentity{
                                    source=absolute,sourceSha256=HashFile(absolute),zoom=camera.orthographicSize,
                                    target=focus,cameraToWorld=camera.cameraToWorldMatrix,projection=camera.projectionMatrix,
                                    rendererCount=ground.Length,physicalDevice=false},true));
                            }
                            finally
                            {
                                RenderTexture.active=previous; camera.targetTexture=null;
                                UnityEngine.Object.DestroyImmediate(pixels); rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
                            }
                        }
                        UnityEngine.Object.DestroyImmediate(root);
                    }
                    version++;
                }
                EditorApplication.Exit(0);
            }
            catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
        }
        static string HashFile(string path)
        {
            using (var sha=SHA256.Create()) using (var stream=File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
        }
        static async Task Create(bool capture)
        {
            var source = Path.GetFullPath(Root + "/Models/ZoneOneSectorCandidate.glb");
            if (!File.Exists(source)) throw new FileNotFoundException("Authored sector GLB absent", source);
            if (GraphicsSettings.defaultRenderPipeline == null)
                throw new InvalidOperationException("Existing project URP pipeline must be configured; no built-in substitute.");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var world = new GameObject("Zone One authored reference sector — candidate");
            var importer = new GltfImport(null, new UninterruptedDeferAgent());
            if (!await importer.Load(new Uri(source).AbsoluteUri)) throw new Exception("Sector GLB import failed");
            if (!await importer.InstantiateMainSceneAsync(world.transform)) throw new Exception("Sector instantiation failed");
            var renderers = world.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) throw new Exception("Sector has no renderers");
            foreach (var renderer in renderers)
                foreach (var material in renderer.sharedMaterials)
                    if (material != null) material.enableInstancing = true;
            var sun = new GameObject("Zone One warm sun").AddComponent<Light>();
            sun.type = LightType.Directional; sun.intensity = 1.4f;
            sun.color = new Color(1, .92f, .78f); sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(42, -35, 0);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.52f, .64f, .76f);
            RenderSettings.ambientEquatorColor = new Color(.36f, .40f, .32f);
            RenderSettings.ambientGroundColor = new Color(.18f, .17f, .13f);
            RenderSettings.fog = false;
            var camera = new GameObject("Single strategic world camera").AddComponent<Camera>();
            camera.tag = "MainCamera"; camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.44f, .51f, .56f);
            var navigation = camera.gameObject.AddComponent<ZoneOneStrategicCamera>();
            var city = world.GetComponentsInChildren<Transform>().FirstOrDefault(
                x=>x.name.Contains("Existing medieval player city"));
            if (city == null) throw new Exception("Named source city was not retained by importer");
            var cityRenderers = city.GetComponentsInChildren<Renderer>();
            if (cityRenderers.Length == 0) throw new Exception("Imported city has no visible geometry");
            Bounds cityBounds = cityRenderers[0].bounds;
            foreach (var renderer in cityRenderers) cityBounds.Encapsulate(renderer.bounds);
            // Derive location after actual glTF handedness conversion, never assume an axis flip.
            navigation.PanTo(cityBounds.center);
            navigation.SetZoomLevel(1, true);
            Directory.CreateDirectory(Root + "/Scenes");
            // Persist importer-created meshes/materials before saving the scene.
            string generated = Root + "/NativeCandidate";
            Directory.CreateDirectory(generated);
            int index = 0;
            foreach (var mesh in world.GetComponentsInChildren<MeshFilter>().Select(x=>x.sharedMesh).Distinct())
                if (mesh != null && !AssetDatabase.Contains(mesh))
                    AssetDatabase.CreateAsset(mesh, AssetDatabase.GenerateUniqueAssetPath(generated + "/mesh-" + index++ + ".asset"));
            index = 0;
            foreach (var mat in renderers.SelectMany(x=>x.sharedMaterials).Distinct())
            {
                if (mat == null || AssetDatabase.Contains(mat)) continue;
                foreach (string property in mat.GetTexturePropertyNames())
                {
                    var texture = mat.GetTexture(property);
                    if (texture != null && !AssetDatabase.Contains(texture))
                        AssetDatabase.CreateAsset(texture, AssetDatabase.GenerateUniqueAssetPath(generated + "/texture-" + index++ + ".asset"));
                }
                AssetDatabase.CreateAsset(mat, AssetDatabase.GenerateUniqueAssetPath(generated + "/material-" + index++ + ".mat"));
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, Root + "/Scenes/ZoneOneReferenceSector.unity");
            if (!capture) return;
            string output = Environment.GetEnvironmentVariable("ELDORIA_ZONEONE_CAPTURE_OUT");
            if (string.IsNullOrEmpty(output)) throw new Exception("Exact capture output path must be supplied");
            if (Directory.Exists(output) && Directory.EnumerateFileSystemEntries(output).Any())
                throw new Exception("Capture output must be empty to preserve earlier evidence");
            Directory.CreateDirectory(output);
            foreach (var pair in new[]{(0,"global"),(1,"regional"),(2,"local")})
            {
                navigation.SetZoomLevel(pair.Item1, true);
                RenderTexture rt = new RenderTexture(1280, 900, 24);
                camera.targetTexture = rt; camera.Render();
                var old = RenderTexture.active; RenderTexture.active = rt;
                var image = new Texture2D(1280,900,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,1280,900),0,0); image.Apply();
                File.WriteAllBytes(Path.Combine(output, pair.Item2+"-unity.png"),image.EncodeToPNG());
                RenderTexture.active=old; camera.targetTexture=null;
                UnityEngine.Object.DestroyImmediate(image); rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
                File.WriteAllText(Path.Combine(output,pair.Item2+"-camera.json"),JsonUtility.ToJson(new CaptureIdentity{
                    source=source,zoom=camera.orthographicSize,target=navigation.target,
                    cameraToWorld=camera.cameraToWorldMatrix,projection=camera.projectionMatrix,
                    rendererCount=renderers.Length,physicalDevice=false},true));
            }
        }
        [Serializable] sealed class CaptureIdentity
        {
            public string source, sourceSha256; public float zoom; public Vector3 target;
            public Matrix4x4 cameraToWorld, projection; public int rendererCount; public bool physicalDevice;
        }
    }
}
#endif
