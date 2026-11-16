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
    public static class RebootHeroCapture
    {
        const string Root = "Assets/Art_Reboot/Phase1";

        [MenuItem("Art Reboot/Capture hero alone")]
        public static void BuildAndCapture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Edit mode only");
            var previousPipeline = QualitySettings.renderPipeline;
            bool previousAsync = ShaderUtil.allowAsyncCompilation;
            var setup = EditorSceneManager.GetSceneManagerSetup();
            string output = Environment.GetEnvironmentVariable("REBOOT_CAPTURE_OUTPUT");
            if (string.IsNullOrEmpty(output)) output = Path.GetFullPath("../artifacts/hero-review");
            Directory.CreateDirectory(output);
            try
            {
                ShaderUtil.allowAsyncCompilation = false;
                QualitySettings.renderPipeline = PreparePilotPipeline();
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                string modelPath = Root + "/Models/Reboot_Hero.fbx";
                AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceSynchronousImport);
                var importer = (ModelImporter)AssetImporter.GetAtPath(modelPath);
                importer.globalScale = 1; importer.useFileScale = true;
                importer.importCameras = false; importer.importLights = false;
                importer.SaveAndReimport();
                string atlasPath = Root + "/Textures/Hero_Atlas_256.png";
                AssetDatabase.ImportAsset(atlasPath, ImportAssetOptions.ForceSynchronousImport);
                var ti = (TextureImporter)AssetImporter.GetAtPath(atlasPath);
                ti.textureType = TextureImporterType.Default; ti.textureShape = TextureImporterShape.Texture2D;
                ti.sRGBTexture = true; ti.mipmapEnabled = false; ti.maxTextureSize = 256;
                ti.textureCompression = TextureImporterCompression.Uncompressed; ti.SaveAndReimport();
                var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(Root + "/Materials/Reboot_Atlas.shadergraph");
                if (shader == null || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Hero shader invalid");
                var stone = MaterialAsset("Hero_Surface", shader, atlas, 0);
                var glow = MaterialAsset("Hero_Emission", shader, atlas, 1.2f);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                var hero = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                // Native evidence 43848005730 showed the FBX facade facing away from the fixed game camera.
                // Rotate the asset placement, not the camera; preserve the tested importer and mesh units.
                hero.transform.rotation = Quaternion.Euler(0,180,0) * hero.transform.rotation;
                var renderers = hero.GetComponentsInChildren<MeshRenderer>();
                if (renderers.Length == 0) throw new InvalidOperationException("No imported hero mesh");
                long triangles = 0;
                foreach (var renderer in renderers)
                {
                    var mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                    if (!mesh.HasVertexAttribute(VertexAttribute.Color) || !mesh.HasVertexAttribute(VertexAttribute.TexCoord0)) throw new InvalidOperationException("Hero missing color/UV");
                    for (int i=0;i<mesh.subMeshCount;i++) triangles += mesh.GetIndexCount(i)/3;
                    renderer.sharedMaterials = renderer.sharedMaterials.Select(m => m != null && m.name.Contains("Light") ? glow : stone).ToArray();
                }
                var expected=JsonUtility.FromJson<HeroGeometry>(File.ReadAllText(Root+"/Source~/HeroReview/geometry.json"));
                if(expected==null || triangles!=expected.triangles) throw new InvalidOperationException("Imported hero triangles differ from authored geometry");
                // Unity's 16-bit importer splits the color/UV-expanded mesh. Keep all parts.
                var bounds = renderers[0].bounds;
                foreach(var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
                if (bounds.size.y < 7 || bounds.size.y > 10) throw new InvalidOperationException("Hero metre scale incorrect: " + bounds);
                File.WriteAllText(Path.Combine(output,"model-world-bounds.txt"), bounds.ToString());
                RenderSettings.skybox = null; RenderSettings.fog = false;
                RenderSettings.ambientMode = AmbientMode.Custom;
                var sun = new GameObject("Warm shape key").AddComponent<Light>();
                sun.type = LightType.Directional; sun.transform.rotation = Quaternion.Euler(32,-35,0);
                sun.color = new Color(1,.84f,.65f); sun.intensity = 1.5f;
                sun.shadows = LightShadows.Soft; sun.shadowStrength = .65f; sun.shadowBias = .025f; sun.shadowNormalBias = .08f;
                RenderSettings.sun = sun;
                var fill = new GameObject("Cool skylight").AddComponent<Light>();
                fill.type=LightType.Directional;fill.transform.rotation=Quaternion.Euler(48,145,0);
                fill.color=new Color(.64f,.73f,1);fill.intensity=.35f;fill.shadows=LightShadows.None;
                var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);floor.name="Neutral review floor";
                floor.transform.localScale=new Vector3(20,1,20);floor.transform.position=new Vector3(0,bounds.min.y-.005f,0);
                var floorMat=new Material(Shader.Find("Universal Render Pipeline/Lit"));floorMat.SetColor("_BaseColor",new Color(.22f,.25f,.29f));
                AssetDatabase.CreateAsset(floorMat,Root+"/Materials/Hero_Floor.mat");floor.GetComponent<Renderer>().sharedMaterial=floorMat;
                var camera = new GameObject("Game camera 50 -13 close-up").AddComponent<Camera>();
                var rotation=Quaternion.Euler(50,-13,0);
                camera.transform.SetPositionAndRotation(bounds.center-rotation*Vector3.forward*30,rotation);
                camera.orthographic=true;camera.nearClipPlane=.1f;camera.farClipPlane=100;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.22f,.25f,.29f);camera.allowHDR=true;
                camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
                camera.GetUniversalAdditionalCameraData().antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                var volume=new GameObject("Hero neutral grading").AddComponent<Volume>();volume.isGlobal=true;
                var profile=ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile,Root+"/Materials/Hero_Profile.asset");
                profile.Add<Tonemapping>(true).mode.Override(TonemappingMode.ACES);
                var bloom=profile.Add<Bloom>(true);bloom.intensity.Override(.12f);bloom.threshold.Override(1.1f);
                foreach(var c in profile.components)AssetDatabase.AddObjectToAsset(c,profile);
                volume.sharedProfile=profile;AssetDatabase.SaveAssets();
                foreach(var size in new[]{new Vector2Int(1100,1250),new Vector2Int(390,844),new Vector2Int(1600,900),new Vector2Int(844,390)})
                {
                    FitHero(camera,bounds,size.x,size.y);
                    Capture(camera,size.x,size.y,Path.Combine(output,$"unity-{size.x}x{size.y}.png"));
                }
                FitHero(camera,bounds,1100,1250);
                EditorSceneManager.SaveScene(scene,Root+"/Scenes/Hero_Review.unity");
                // Additional actual silhouette proves the same 3D form without surface detail.
                var normalMaterials=renderers.Select(r=>r.sharedMaterials).ToArray();
                var silhouette=new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                silhouette.SetColor("_BaseColor",Color.black);
                var previousBackground=camera.backgroundColor;
                try
                {
                    floor.SetActive(false); volume.enabled=false;
                    camera.GetUniversalAdditionalCameraData().renderPostProcessing=false;
                    camera.backgroundColor=Color.white;
                    foreach(var renderer in renderers) renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>silhouette).ToArray();
                    Capture(camera,1100,1250,Path.Combine(output,"unity-hero-silhouette.png"));
                }
                finally
                {
                    for(int i=0;i<renderers.Length;i++)renderers[i].sharedMaterials=normalMaterials[i];
                    floor.SetActive(true);volume.enabled=true;camera.backgroundColor=previousBackground;
                    camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
                    Object.DestroyImmediate(silhouette);
                }
                File.WriteAllText(Path.Combine(output,"capture.json"),JsonUtility.ToJson(new Evidence{
                    unity=UnityEngine.Application.unityVersion,sourceSHA=Environment.GetEnvironmentVariable("GITHUB_SHA"),
                    scene=Root+"/Scenes/Hero_Review.unity",cameraTilt=50,triangles=triangles,
                    sunElevation=32,sunIntensity=1.5f,bloomIntensity=.12f,bloomThreshold=1.1f,
                    note="Hero alone, game camera rotation (50,-13,0), reframed for close-up. Asset placement yaw corrected 180 degrees after native rear-view diagnosis. Neutral review lighting. Scene assembly and derivatives awaiting owner approval. Not mobile performance proof."
                },true));
                Debug.Log("REBOOT_CAPTURE_OUTPUT "+output);
            }
            finally
            {
                QualitySettings.renderPipeline=previousPipeline;ShaderUtil.allowAsyncCompilation=previousAsync;
                if(setup.Any(x=>x.isLoaded)&&setup.Count(x=>x.isActive)==1)EditorSceneManager.RestoreSceneManagerSetup(setup);
                else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            }
        }
        [Serializable] sealed class HeroGeometry { public long triangles; }
        static void FitHero(Camera camera,Bounds bounds,int width,int height)
        {
            float horizontal=0,vertical=0;
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
            {
                var p=camera.transform.InverseTransformPoint(bounds.center+Vector3.Scale(bounds.extents,new Vector3(x,y,z)));
                horizontal=Mathf.Max(horizontal,Mathf.Abs(p.x));vertical=Mathf.Max(vertical,Mathf.Abs(p.y));
            }
            camera.orthographicSize=Mathf.Max(vertical,horizontal/((float)width/height))*1.12f;
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
