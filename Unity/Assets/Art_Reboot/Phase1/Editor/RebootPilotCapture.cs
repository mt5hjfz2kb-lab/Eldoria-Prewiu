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
            var previousPipeline = QualitySettings.renderPipeline;
            string original = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
            byte[] originalBytes = File.ReadAllBytes(original);
            var requestedOutput = Environment.GetEnvironmentVariable("REBOOT_CAPTURE_OUTPUT");
            var output = Path.GetFullPath(string.IsNullOrEmpty(requestedOutput)
                ? Path.Combine("..", "artifacts", "visual-reboot", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"))
                : requestedOutput);
            Directory.CreateDirectory(output);
            try
            {
                QualitySettings.renderPipeline = PreparePilotPipeline();
                var scene = EditorSceneManager.OpenScene(RebootSandboxGuard.ScenePath, OpenSceneMode.Single);
                // The copied hierarchy stays archived, inactive and unmodified.
                foreach (var item in scene.GetRootGameObjects())
                {
                    if (item.name == Generated) Object.DestroyImmediate(item);
                    else if (item.activeSelf) throw new InvalidOperationException("Unexpected active original root: " + item.name);
                }
                PrepareModel(output);
                AssetDatabase.ImportAsset(Root + "/Materials/Reboot_Atlas.shadergraph", ImportAssetOptions.ForceSynchronousImport);
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(Root + "/Materials/Reboot_Atlas.shadergraph");
                Debug.Log("REBOOT_SHADER_IMPORT " + (shader == null ? "null" : shader.name));
                if (shader == null || ShaderUtil.ShaderHasError(shader))
                    throw new InvalidOperationException("Pilot Shader Graph failed import/compilation; no fallback shader accepted.");
                var atlas = PrepareAtlas(output);
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
                var terrainBounds = renderers.Single(r => r.name == "Terrain").bounds;
                Debug.Log("REBOOT_WORLD_BOUNDS " + terrainBounds);
                File.WriteAllText(Path.Combine(output, "model-world-bounds.txt"),
                    string.Join("\n", renderers.Select(r => r.name + " " + r.bounds + " scale=" + r.transform.lossyScale)));
                if (Mathf.Abs(terrainBounds.size.x - 48) > .1f || Mathf.Abs(terrainBounds.size.z - 48) > .1f)
                    throw new InvalidOperationException("Imported world metres differ from Blender's 48m terrain: " + terrainBounds);
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
            catch (Exception error)
            {
                // Preserve the primary failure even if editor-state cleanup fails too.
                Debug.LogException(error);
                File.WriteAllText(Path.Combine(output, "capture-failure.txt"), error.ToString());
                throw;
            }
            finally
            {
                QualitySettings.renderPipeline = previousPipeline;
                // Fresh batchmode projects can start with no loaded/active scene.
                // Unity rejects restoring that empty SceneSetup array.
                if (setup.Any(s => s.isLoaded) && setup.Count(s => s.isActive) == 1)
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
                else
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                if (!File.ReadAllBytes(original).SequenceEqual(originalBytes))
                    throw new InvalidOperationException("Original scene bytes changed during capture.");
            }
        }

        static void PrepareModel(string output)
        {
            const string path = Root + "/Models/Reboot_Pilot.fbx";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new InvalidOperationException("FBX ModelImporter unavailable.");
            File.WriteAllText(Path.Combine(output, "model-import-before.json"), EditorJsonUtility.ToJson(importer, true));
            Debug.Log($"REBOOT_MODEL_BEFORE global={importer.globalScale} file={importer.fileScale} useFileScale={importer.useFileScale} useFileUnits={importer.useFileUnits}");
            importer.globalScale = 1;
            importer.useFileScale = true;
            importer.useFileUnits = true;
            importer.bakeAxisConversion = true;
            importer.importAnimation = false;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.importNormals = ModelImporterNormals.Import;
            importer.SaveAndReimport();
            File.WriteAllText(Path.Combine(output, "model-import-after.json"), EditorJsonUtility.ToJson(importer, true));
            Debug.Log($"REBOOT_MODEL_AFTER global={importer.globalScale} file={importer.fileScale} useFileScale={importer.useFileScale} useFileUnits={importer.useFileUnits}");
        }

        static Texture2D PrepareAtlas(string output)
        {
            const string path = Root + "/Textures/Reboot_Atlas_256.png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Atlas TextureImporter unavailable.");
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            var before = AssetDatabase.LoadMainAssetAtPath(path);
            File.WriteAllText(Path.Combine(output, "atlas-import-before.json"),
                EditorJsonUtility.ToJson(importer, true));
            Debug.Log($"REBOOT_ATLAS_BEFORE source={width}x{height} type={before?.GetType().Name} max={importer.maxTextureSize} shape={importer.textureShape}");
            if (width != 256 || height != 256)
                throw new InvalidOperationException($"Authored atlas source differs: {width}x{height}");
            // The recovered .meta supplied only a GUID. Make the owned atlas's
            // intended import contract explicit; retain its GUID and PNG bytes.
            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.maxTextureSize = 256;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = false;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.ClearPlatformTextureSettings("Standalone");
            importer.SaveAndReimport();
            var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (atlas == null || atlas.width != 256 || atlas.height != 256)
                throw new InvalidOperationException($"Atlas import contract failed: {(atlas == null ? "null" : atlas.width + "x" + atlas.height)}");
            Debug.Log($"REBOOT_ATLAS_AFTER {atlas.width}x{atlas.height} {atlas.format}");
            File.WriteAllText(Path.Combine(output, "atlas-import-after.json"),
                EditorJsonUtility.ToJson(importer, true));
            return atlas;
        }

        static UniversalRenderPipelineAsset PreparePilotPipeline()
        {
            // The production renderer has no postProcessData. Clone it; never mutate it.
            const string pipelineSource = "Assets/Eldoria/Content/EldoriaMobileURP.asset";
            const string rendererSource = "Assets/Eldoria/Content/EldoriaForwardRenderer.asset";
            const string pipelinePath = Root + "/Materials/Reboot_URP.asset";
            const string rendererPath = Root + "/Materials/Reboot_Renderer.asset";
            if (!AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath)
                && !AssetDatabase.CopyAsset(pipelineSource, pipelinePath))
                throw new InvalidOperationException("Cannot clone original mobile URP asset.");
            if (!AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath)
                && !AssetDatabase.CopyAsset(rendererSource, rendererPath))
                throw new InvalidOperationException("Cannot clone original renderer.");
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            var post = AssetDatabase.LoadAssetAtPath<PostProcessData>(
                "Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
            if (post == null) throw new InvalidOperationException("Installed URP postprocess resources unavailable.");
            var rendererData = new SerializedObject(renderer);
            var postProperty = rendererData.FindProperty("postProcessData");
            if (postProperty == null) throw new InvalidOperationException("URP renderer schema differs; do not guess.");
            postProperty.objectReferenceValue = post;
            rendererData.ApplyModifiedPropertiesWithoutUndo();
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            var settings = new SerializedObject(pipeline);
            var renderers = settings.FindProperty("m_RendererDataList");
            if (renderers == null || renderers.arraySize != 1)
                throw new InvalidOperationException("Unexpected mobile renderer topology.");
            renderers.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
            settings.ApplyModifiedPropertiesWithoutUndo();
            return pipeline;
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
            var previousTarget = camera.targetTexture;
            float previousAspect = camera.aspect;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D pixels = null;
            try
            {
                if (!target.Create()) throw new InvalidOperationException("GPU allocation failed.");
                camera.aspect = (float)width / height;
                var request = new RenderPipeline.StandardRequest { destination = target };
                // Batch Edit Mode can reach this method before the newly selected SRP
                // has rendered its first frame. Bootstrap one genuine camera render.
                if (!RenderPipeline.SupportsRenderRequest(camera, request))
                {
                    camera.targetTexture = target;
                    camera.Render();
                    camera.targetTexture = previousTarget;
                }
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
                File.WriteAllBytes(path,pixels.EncodeToPNG());
                File.WriteAllText(path + ".metrics.txt", $"lumaMin={min} lumaMax={max} magenta={magenta} pixels={colors.Length}");
                if (max - min < 20 || magenta > colors.Length / 1000)
                    throw new InvalidOperationException($"Rejected render {width}x{height}: luma {min}..{max}, magenta {magenta}; diagnostic PNG retained, not approved.");
            }
            finally
            {
                camera.aspect = previousAspect; camera.targetTexture = previousTarget;
                RenderTexture.active = previous;
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
