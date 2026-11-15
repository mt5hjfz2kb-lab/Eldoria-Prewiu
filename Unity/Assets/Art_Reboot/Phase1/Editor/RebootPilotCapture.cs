using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Eldoria.ArtReboot.Editor
{
    // Editor-only assembly: never enters Play Mode or resolves a game gateway.
    public static class RebootPilotCapture
    {
        const string Root = "Assets/Art_Reboot/Phase1";
        const string Generated = "Reboot_Phase1_Authored";

        [MenuItem("Art Reboot/Build and capture phase 1")]
        public static void BuildAndCapture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Capture requires Edit Mode.");
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save your open scenes before the isolated capture.");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            string original = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
            byte[] originalBytes = File.ReadAllBytes(original);
            var output = Path.GetFullPath(Path.Combine("..", "artifacts", "visual-reboot", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")));
            Directory.CreateDirectory(output);
            try
            {
                var scene = EditorSceneManager.OpenScene(RebootSandboxGuard.ScenePath, OpenSceneMode.Single);
                // The copied hierarchy stays archived, inactive and unmodified.
                foreach (var item in scene.GetRootGameObjects())
                {
                    if (item.name == Generated) Object.DestroyImmediate(item);
                    else if (item.activeSelf) throw new InvalidOperationException("Unexpected active original root: " + item.name);
                }
                AssetDatabase.ImportAsset(Root + "/Models/Reboot_Pilot.fbx", ImportAssetOptions.ForceSynchronousImport);
                AssetDatabase.ImportAsset(Root + "/Materials/Reboot_Atlas.shadergraph", ImportAssetOptions.ForceSynchronousImport);
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(Root + "/Materials/Reboot_Atlas.shadergraph");
                if (shader == null || ShaderUtil.ShaderHasError(shader))
                    throw new InvalidOperationException("Pilot Shader Graph failed import/compilation; no fallback shader accepted.");
                var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "/Textures/Reboot_Atlas_256.png");
                if (atlas == null || atlas.width != 256 || atlas.height != 256)
                    throw new InvalidOperationException("Expected the authored 256x256 atlas.");
                var stone = MaterialAsset("Reboot_Surface", shader, atlas, 0);
                var glow = MaterialAsset("Reboot_Emission", shader, atlas, 2.4f);
                var root = new GameObject(Generated);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Models/Reboot_Pilot.fbx");
                if (model == null) throw new InvalidOperationException("FBX is not imported.");
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(model, scene);
                instance.transform.SetParent(root.transform, false);
                var renderers = instance.GetComponentsInChildren<MeshRenderer>();
                if (renderers.Length != 4) throw new InvalidOperationException("Expected four authored mesh families.");
                long triangles = 0;
                foreach (var renderer in renderers)
                {
                    var mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                    if (!mesh.HasVertexAttribute(VertexAttribute.Color) || !mesh.HasVertexAttribute(VertexAttribute.TexCoord0))
                        throw new InvalidOperationException("Imported vertex colors or UV0 absent: " + renderer.name);
                    for (int i = 0; i < mesh.subMeshCount; i++) triangles += (long)mesh.GetIndexCount(i) / 3;
                    renderer.sharedMaterials = renderer.sharedMaterials.Select(m =>
                        m != null && m.name.IndexOf("Light", StringComparison.OrdinalIgnoreCase) >= 0 ? glow : stone).ToArray();
                    renderer.shadowCastingMode = ShadowCastingMode.On;
                    renderer.receiveShadows = true;
                }
                if (triangles != 21308) throw new InvalidOperationException("FBX topology differs from authored source: " + triangles);
                RenderSettings.ambientMode = AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = new Color(.24f, .31f, .44f);
                RenderSettings.ambientEquatorColor = new Color(.15f, .19f, .22f);
                RenderSettings.ambientGroundColor = new Color(.08f, .09f, .08f);
                RenderSettings.skybox = null;
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Linear;
                RenderSettings.fogColor = new Color(.20f, .27f, .34f);
                RenderSettings.fogStartDistance = 25;
                RenderSettings.fogEndDistance = 72;
                var sun = new GameObject("Warm sunset").AddComponent<Light>();
                sun.transform.SetParent(root.transform, false);
                sun.type = LightType.Directional;
                sun.transform.rotation = Quaternion.Euler(32, -35, 0);
                sun.color = new Color(1, .76f, .50f);
                sun.intensity = 2.2f;
                sun.shadows = LightShadows.Soft;
                sun.shadowBias = .04f;
                sun.shadowNormalBias = .25f;
                RenderSettings.sun = sun;
                var camera = new GameObject("Strategic 50 degree camera").AddComponent<Camera>();
                camera.transform.SetParent(root.transform, false);
                var rotation = Quaternion.Euler(50, -13, 0);
                camera.transform.SetPositionAndRotation(new Vector3(0, 1, .8f) - rotation * Vector3.forward * 30, rotation);
                camera.orthographic = true;
                camera.orthographicSize = 10;
                camera.nearClipPlane = .1f;
                camera.farClipPlane = 100;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = RenderSettings.fogColor;
                camera.allowHDR = true;
                camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
                camera.GetUniversalAdditionalCameraData().antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                var volume = new GameObject("Restrained dusk grading").AddComponent<Volume>();
                volume.transform.SetParent(root.transform, false);
                volume.isGlobal = true;
                const string profilePath = Root + "/Materials/Reboot_Dusk.asset";
                var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
                if (profile == null)
                {
                    profile = ScriptableObject.CreateInstance<VolumeProfile>();
                    AssetDatabase.CreateAsset(profile, profilePath);
                    var bloom = profile.Add<Bloom>(true);
                    bloom.intensity.Override(.18f); bloom.threshold.Override(1.2f);
                    var tone = profile.Add<Tonemapping>(true); tone.mode.Override(TonemappingMode.ACES);
                    foreach (var component in profile.components) AssetDatabase.AddObjectToAsset(component, profile);
                }
                volume.sharedProfile = profile;
                AssetDatabase.SaveAssets();
                if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))
                    throw new InvalidOperationException("Real URP pipeline required; project settings will not be overwritten.");
                EditorSceneManager.SaveScene(scene, RebootSandboxGuard.ScenePath);
                foreach (var size in new[] { new Vector2Int(1100,1250), new Vector2Int(390,844), new Vector2Int(1600,900), new Vector2Int(844,390) })
                {
                    // Keep geometry and angle fixed. Fit all three buildings at narrow aspect ratios.
                    camera.orthographicSize = Mathf.Max(10, 8.8f / ((float)size.x / size.y));
                    Capture(camera, size.x, size.y, Path.Combine(output, $"unity-{size.x}x{size.y}.png"));
                }
                File.WriteAllText(Path.Combine(output,"capture.json"), JsonUtility.ToJson(new Evidence
                {
                    unity = UnityEngine.Application.unityVersion, triangles = triangles,
                    sourceSHA = Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "local-unrecorded",
                    scene = RebootSandboxGuard.ScenePath, cameraTilt = 50,
                    note = "Actual Edit Mode URP render requests. No gameplay, physical mobile, ParticleSystem or performance certification. Artistic review required."
                }, true));
                Debug.Log("REBOOT_CAPTURE_OUTPUT " + output);
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(setup);
                if (!File.ReadAllBytes(original).SequenceEqual(originalBytes))
                    throw new InvalidOperationException("Original scene bytes changed during capture.");
            }
        }

        static Material MaterialAsset(string name, Shader shader, Texture atlas, float emission)
        {
            var path = Root + "/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material, path); }
            material.shader = shader;
            material.SetTexture("_Atlas", atlas); material.SetFloat("_Glow", emission);
            EditorUtility.SetDirty(material);
            return material;
        }

        static void Capture(Camera camera, int width, int height, string path)
        {
            var previous = RenderTexture.active;
            float previousAspect = camera.aspect;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D pixels = null;
            try
            {
                if (!target.Create()) throw new InvalidOperationException("GPU allocation failed.");
                camera.aspect = (float)width / height;
                var request = new RenderPipeline.StandardRequest { destination = target };
                if (!RenderPipeline.SupportsRenderRequest(camera, request))
                    throw new InvalidOperationException("URP render request unavailable.");
                RenderPipeline.SubmitRenderRequest(camera, request);
                RenderTexture.active = target;
                pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
                pixels.ReadPixels(new Rect(0,0,width,height),0,0); pixels.Apply();
                var colors = pixels.GetPixels32();
                int min = 255, max = 0, magenta = 0;
                foreach (var c in colors)
                {
                    int luma = (c.r + c.g + c.b) / 3; min = Math.Min(min,luma); max = Math.Max(max,luma);
                    if (c.r > 220 && c.b > 220 && c.g < 40) magenta++;
                }
                if (max - min < 20 || magenta > colors.Length / 1000)
                    throw new InvalidOperationException("Blank/flat render or shader-error pixels; no PASS.");
                File.WriteAllBytes(path,pixels.EncodeToPNG());
            }
            finally
            {
                camera.aspect = previousAspect; RenderTexture.active = previous;
                if (pixels != null) Object.DestroyImmediate(pixels);
                target.Release(); Object.DestroyImmediate(target);
            }
        }

        [Serializable] sealed class Evidence
        {
            public string unity, sourceSHA, scene, note;
            public long triangles;
            public float cameraTilt;
        }
    }
}
