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
            bool previousAsync = ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation = false;
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
                var glow = MaterialAsset("Reboot_Emission", shader, atlas, 1.75f);
                var corruption = MaterialAsset("Reboot_Corruption", shader, atlas, 1.35f);
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
                        m != null && m.name.IndexOf("Corruption", StringComparison.OrdinalIgnoreCase) >= 0 ? corruption :
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
                var geometry = JsonUtility.FromJson<GeneratedGeometry>(
                    File.ReadAllText(Root + "/Evidence/geometry.json"));
                if (geometry == null || geometry.mesh_objects != 4 || geometry.triangles < 21000
                    || triangles != geometry.triangles)
                    throw new InvalidOperationException($"Authored FBX topology mismatch: Unity={triangles}, Blender={geometry?.triangles}");
                RenderSettings.ambientMode = AmbientMode.Custom;
                RenderSettings.ambientSkyColor = new Color(.42f, .40f, .42f);
                RenderSettings.ambientEquatorColor = new Color(.29f, .31f, .38f);
                RenderSettings.ambientGroundColor = new Color(.16f, .20f, .22f);
                RenderSettings.skybox = null;
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Linear;
                RenderSettings.fogColor = new Color(.36f, .38f, .43f);
                RenderSettings.fogStartDistance = 20f;
                RenderSettings.fogEndDistance = 58f;
                var sun = new GameObject("Warm sunset").AddComponent<Light>();
                sun.transform.SetParent(root.transform, false);
                sun.type = LightType.Directional;
                sun.transform.rotation = Quaternion.Euler(16f, -35f, 0f);
                sun.color = new Color(1f, .735f, .46f);
                sun.intensity = 1.65f;
                sun.shadows = LightShadows.Soft;
                sun.shadowStrength = .62f;
                sun.shadowBias = .028f;
                sun.shadowNormalBias = .17f;
                RenderSettings.sun = sun;
                var fill = new GameObject("Cool open-sky fill").AddComponent<Light>();
                fill.transform.SetParent(root.transform, false);
                fill.type = LightType.Directional;
                fill.transform.rotation = Quaternion.Euler(48, 145, 0);
                fill.color = new Color(.61f, .65f, .88f);
                fill.intensity = .23f;
                fill.shadows = LightShadows.None;
                // Exactly one realtime local light illuminates both emissive entrance torches.
                var flameFill = new GameObject("Single shared torch point light").AddComponent<Light>();
                flameFill.transform.SetParent(root.transform, false);
                flameFill.transform.localPosition = new Vector3(0f, 2.15f, -.85f);
                flameFill.type = LightType.Point;
                flameFill.color = new Color(1f, .65f, .34f);
                flameFill.intensity = .52f;
                flameFill.range = 3.5f;
                flameFill.shadows = LightShadows.None;
                BuildLowGroundMist(root);
                var corruptParticles = BuildCorruptionParticles(root);
                if (corruptParticles.particleCount < 2)
                    throw new InvalidOperationException("Corruption particles did not simulate in capture scene.");
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
                ConfigureDuskProfile(profile);
                volume.sharedProfile = profile;
                AssetDatabase.SaveAssets();
                if (!(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))
                    throw new InvalidOperationException("Real URP pipeline required; project settings will not be overwritten.");
                EditorSceneManager.SaveScene(scene, RebootSandboxGuard.ScenePath);
                foreach (var size in new[] { new Vector2Int(1100,1250), new Vector2Int(390,844), new Vector2Int(1600,900), new Vector2Int(844,390) })
                {
                    // Keep geometry and angle fixed. Fit all three buildings at narrow aspect ratios.
                    FitBuildings(camera, renderers, size.x, size.y);
                    Capture(camera, size.x, size.y, Path.Combine(output, $"unity-{size.x}x{size.y}.png"));
                }
                FitBuildings(camera, renderers, 1100, 1250);
                Capture(camera, 1100, 1250, Path.Combine(output, "unity-repeat-1100x1250.png"));
                File.WriteAllText(Path.Combine(output,"capture.json"), JsonUtility.ToJson(new Evidence
                {
                    unity = UnityEngine.Application.unityVersion, triangles = triangles,
                    sourceSHA = Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "local-unrecorded",
                    scene = RebootSandboxGuard.ScenePath, cameraTilt = 50,
                    sunElevation = 16f, sunIntensity = 1.65f, torchIntensity = .52f,
                    fogStart = 20f, fogEnd = 58f, bloomThreshold = 1.05f, bloomIntensity = .21f,
                    groundMistLowY = .19f, groundMistHighY = .55f,
                    shadowMapResolution = 2048, shadowCascades = 2, particleCount = corruptParticles.particleCount,
                    note = "Actual Edit Mode URP render requests. Particle emitter simulated in Editor; physical mobile, gameplay, runtime motion and performance remain unverified. Artistic review required."
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
                ShaderUtil.allowAsyncCompilation = previousAsync;
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


        // Separate 3D ground-hugging haze from the built-in distant linear fog.
        // Two vertex-alpha meshes, no large textures and no extra dynamic lights.
        static void BuildLowGroundMist(GameObject root)
        {
            const string shaderPath = Root + "/Materials/Reboot_Atmosphere.shader";
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
            if (shader == null || ShaderUtil.ShaderHasError(shader))
                throw new InvalidOperationException("URP vertex-alpha atmosphere shader unavailable.");
            const string materialPath = Root + "/Materials/Reboot_LowMist.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.shader = shader;
            material.SetFloat("_EmissionScale", 1f);
            EditorUtility.SetDirty(material);
            for (int layer = 0; layer < 2; layer++)
            {
                var meshPath = Root + "/Materials/Reboot_LowMist_" + layer + ".asset";
                var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                bool createMeshAsset = mesh == null;
                // Recompute every capture. Reusing a persisted Mesh must not freeze older mist colours.
                {
                    const int n = 13;
                    var verts = new Vector3[n * n];
                    var colors = new Color[n * n];
                    var tris = new int[(n - 1) * (n - 1) * 6];
                    for (int z = 0; z < n; z++)
                    for (int x = 0; x < n; x++)
                    {
                        float xx = -13f + x * (26f / (n - 1));
                        float zz = -11f + z * (25f / (n - 1));
                        float back = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-7f, 12f, zz));
                        float edge = Mathf.Sin(Mathf.PI * x / (n - 1)) * Mathf.Sin(Mathf.PI * z / (n - 1));
                        float density = (layer == 0 ? .17f : .09f) * (.35f + .65f * back) * edge;
                        int index = z * n + x;
                        verts[index] = new Vector3(xx, layer == 0 ? .19f : .55f, zz);
                        colors[index] = new Color(.47f, .44f, .52f, density);
                    }
                    int t = 0;
                    for (int z = 0; z < n - 1; z++)
                    for (int x = 0; x < n - 1; x++)
                    {
                        int i = z * n + x;
                        tris[t++] = i; tris[t++] = i + n; tris[t++] = i + 1;
                        tris[t++] = i + 1; tris[t++] = i + n; tris[t++] = i + n + 1;
                    }
                    if (mesh == null) mesh = new Mesh { name = "Low mist vertex alpha " + layer };
                    mesh.Clear();
                    mesh.vertices = verts;
                    mesh.colors = colors;
                    mesh.triangles = tris;
                    mesh.RecalculateBounds();
                    if (createMeshAsset) AssetDatabase.CreateAsset(mesh, meshPath);
                    else EditorUtility.SetDirty(mesh);
                }
                var go = new GameObject(layer == 0 ? "Low dense ground haze" : "Far hovering mist");
                go.transform.SetParent(root.transform, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.sortingOrder = layer;
            }
        }

        // Slow ascending particles are a real ParticleSystem, not painted spheres.
        // They remain local to the fractured watchtower and cast no extra shadows.
        static ParticleSystem BuildCorruptionParticles(GameObject root)
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(
                Root + "/Materials/Reboot_Atmosphere.shader");
            if (shader == null) throw new InvalidOperationException("Particle shader missing.");
            const string path = Root + "/Materials/Reboot_ArcaneParticles.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetFloat("_EmissionScale", 2.0f);
            EditorUtility.SetDirty(material);
            var go = new GameObject("Localized slowly rising arcane motes");
            go.transform.SetParent(root.transform, false);
            go.transform.localPosition = new Vector3(4.35f, .32f, -.85f);
            var particles = go.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.playOnAwake = false;
            main.loop = true;
            main.duration = 7f;
            main.maxParticles = 28;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3.8f, 6.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(.08f, .20f);
            main.startSize = new ParticleSystem.MinMaxCurve(.045f, .11f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(.72f, .32f, 1f, .72f));
            var emission = particles.emission;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(2.6f);
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(2.05f, .2f, 1.8f);
            var velocity = particles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.y = new ParticleSystem.MinMaxCurve(.20f, .36f);
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            particles.Simulate(6f, true, true, true);
            particles.Pause(true);
            Debug.Log("REBOOT_ARCANE_PARTICLES " + particles.particleCount);
            return particles;
        }

        static void ConfigureDuskProfile(VolumeProfile profile)
        {
            if (!profile.TryGet<Bloom>(out var bloom))
            {
                bloom = profile.Add<Bloom>(true);
                AssetDatabase.AddObjectToAsset(bloom, profile);
            }
            bloom.active = true;
            bloom.threshold.Override(1.05f);
            bloom.intensity.Override(.21f);
            bloom.scatter.Override(.66f);
            if (!profile.TryGet<Tonemapping>(out var tonemapping))
            {
                tonemapping = profile.Add<Tonemapping>(true);
                AssetDatabase.AddObjectToAsset(tonemapping, profile);
            }
            tonemapping.mode.Override(TonemappingMode.ACES);
            if (!profile.TryGet<ColorAdjustments>(out var grading))
            {
                grading = profile.Add<ColorAdjustments>(true);
                AssetDatabase.AddObjectToAsset(grading, profile);
            }
            grading.contrast.Override(3f);
            grading.saturation.Override(0f);
            if (!profile.TryGet<SplitToning>(out var split))
            {
                split = profile.Add<SplitToning>(true);
                AssetDatabase.AddObjectToAsset(split, profile);
            }
            split.shadows.Override(new Color(.43f, .40f, .53f));
            split.highlights.Override(new Color(1f, .81f, .59f));
            split.balance.Override(-8f);
            EditorUtility.SetDirty(profile);
        }

        [Serializable] sealed class GeneratedGeometry
        {
            public int triangles, mesh_objects;
        }

        static void FitBuildings(Camera camera, MeshRenderer[] renderers, int width, int height)
        {
            float horizontal = 0, vertical = 0;
            foreach (var renderer in renderers.Where(r => r.name != "Terrain"))
            {
                var bounds = renderer.bounds;
                for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                {
                    var world = bounds.center + Vector3.Scale(bounds.extents, new Vector3(x,y,z));
                    var point = camera.transform.InverseTransformPoint(world);
                    horizontal = Mathf.Max(horizontal, Mathf.Abs(point.x));
                    vertical = Mathf.Max(vertical, Mathf.Abs(point.y));
                }
            }
            camera.orthographicSize = Mathf.Max(10, vertical * 1.12f,
                horizontal * 1.12f / ((float)width / height));
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
            var softShadows = settings.FindProperty("m_SoftShadowsSupported");
            if (softShadows == null) throw new InvalidOperationException("URP soft-shadow schema differs.");
            softShadows.boolValue = true;
            // Mobile-minded, two near-field cascades with verified URP17 serialization names.
            var resolution = settings.FindProperty("m_MainLightShadowmapResolution");
            var cascades = settings.FindProperty("m_ShadowCascadeCount");
            var split = settings.FindProperty("m_Cascade2Split");
            var distance = settings.FindProperty("m_ShadowDistance");
            var softness = settings.FindProperty("m_SoftShadowQuality");
            if (resolution == null || cascades == null || split == null || distance == null || softness == null)
                throw new InvalidOperationException("URP shadow schema differs; do not silently downgrade.");
            resolution.intValue = 2048;
            cascades.intValue = 2;
            split.floatValue = .37f;
            distance.floatValue = 42f;
            softness.intValue = 2;
            settings.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pipeline);
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
                // Update the explicit sky probe on every request, avoiding an
                // inherited sky probe on the first batch Editor frame.
                for (int frame = 0; frame < 3; frame++)
                {
                    var probe = new SphericalHarmonicsL2();
                    probe.AddAmbientLight(new Color(.28f, .30f, .33f));
                    RenderSettings.ambientProbe = probe;
                    RenderPipeline.SubmitRenderRequest(camera, request);
                }
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
            public float sunElevation, sunIntensity, torchIntensity, fogStart, fogEnd,
                         bloomThreshold, bloomIntensity, groundMistLowY, groundMistHighY;
            public int shadowMapResolution, shadowCascades, particleCount;
            public long triangles;
            public float cameraTilt;
        }
    }
}
