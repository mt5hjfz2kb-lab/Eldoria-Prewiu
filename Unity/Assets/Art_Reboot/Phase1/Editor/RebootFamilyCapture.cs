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
    public static class RebootFamilyCapture
    {
        const string Root="Assets/Art_Reboot/Phase1";
        [Serializable] sealed class Geometry { public long triangles; }
        [Serializable] sealed class Proof
        {
            public string unity,sourceSHA,scene,note;
            public long heroTriangles,ruinTriangles,corruptTriangles,environmentTriangles;
            public float cameraTilt=50,cameraYaw=-13,sunElevation=16,sunIntensity=1.65f,fogStart=32,fogEnd=78,bloomIntensity=.21f,bloomThreshold=1.05f;
            public int shadowResolution=2048,shadowCascades=2,particles;
            public bool approvedHeroBytesPreserved=true,originalScenesPreserved=true,physicalMobileTested=false;
        }
        [MenuItem("Art Reboot/Capture approved hero family")]
        public static void BuildAndCapture()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Edit mode only");
            for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save scenes before capture");
            var protectedPaths=new[]{Root+"/Models/Reboot_Hero.fbx",Root+"/Textures/Hero_Atlas_256.png",Root+"/Scenes/Hero_Review.unity",Root+"/Scenes/Eldoria_Visual_Sandbox.unity"};
            var protectedBytes=protectedPaths.Select(File.ReadAllBytes).ToArray();
            var setup=EditorSceneManager.GetSceneManagerSetup();var previousPipeline=QualitySettings.renderPipeline;bool previousAsync=ShaderUtil.allowAsyncCompilation;
            string output=Environment.GetEnvironmentVariable("REBOOT_CAPTURE_OUTPUT")??Path.GetFullPath("../artifacts/hero-family");Directory.CreateDirectory(output);
            try
            {
                ShaderUtil.allowAsyncCompilation=false;QualitySettings.renderPipeline=PreparePilotPipeline();
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var root=new GameObject("Phase1_ApprovedHero_Family");
                var shader=AssetDatabase.LoadAssetAtPath<Shader>(Root+"/Materials/Reboot_Atlas.shadergraph");
                if(shader==null||ShaderUtil.ShaderHasError(shader))throw new InvalidOperationException("Family shader error");
                var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/Hero_Atlas_256.png");
                if(atlas==null||atlas.width!=256)throw new InvalidOperationException("Approved atlas unavailable");
                var surface=MaterialAsset("Family_Surface",shader,atlas,0);
                var warm=MaterialAsset("Family_WarmLight",shader,atlas,1.75f);
                var corruption=MaterialAsset("Family_Corruption",shader,atlas,3.2f);
                var names=new[]{"Hero","Ruin","Corrupt","Environment"};
                var positions=new[]{new Vector3(0,0,7),new Vector3(-10,0,-2),new Vector3(10,0,0),Vector3.zero};
                var objects=new GameObject[4];var bounds=new Bounds[4];var triangles=new long[4];
                for(int k=0;k<4;k++)
                {
                    string path=Root+"/Models/Reboot_"+names[k]+".fbx";
                    AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                    var importer=(ModelImporter)AssetImporter.GetAtPath(path);
                    importer.globalScale=1;importer.useFileScale=true;importer.useFileUnits=true;
                    importer.bakeAxisConversion=false;importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;
                    importer.meshCompression=ModelImporterMeshCompression.Off;importer.importNormals=ModelImporterNormals.Import;importer.SaveAndReimport();
                    objects[k]=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path),scene);
                    objects[k].name="Family_"+names[k];objects[k].transform.SetParent(root.transform,false);
                    objects[k].transform.rotation=Quaternion.Euler(0,180,0)*objects[k].transform.rotation;
                    objects[k].transform.position=positions[k];
                    var renderers=objects[k].GetComponentsInChildren<MeshRenderer>();
                    if(renderers.Length==0)throw new InvalidOperationException("No imported meshes: "+names[k]);
                    bounds[k]=renderers[0].bounds;
                    foreach(var renderer in renderers)
                    {
                        bounds[k].Encapsulate(renderer.bounds);
                        var mesh=renderer.GetComponent<MeshFilter>().sharedMesh;
                        if(!mesh.HasVertexAttribute(VertexAttribute.Color)||!mesh.HasVertexAttribute(VertexAttribute.TexCoord0))throw new InvalidOperationException("Missing UV/color: "+names[k]);
                        for(int s=0;s<mesh.subMeshCount;s++)triangles[k]+=mesh.GetIndexCount(s)/3;
                        renderer.sharedMaterials=renderer.sharedMaterials.Select(m=>m!=null&&m.name.Contains("Corruption")?corruption:m!=null&&m.name.Contains("Light")?warm:surface).ToArray();
                    }
                    string geometry=k==0?Root+"/Source~/HeroReview/geometry.json":Root+"/Source~/Family/"+names[k].ToLowerInvariant()+".json";
                    var expected=JsonUtility.FromJson<Geometry>(File.ReadAllText(geometry));
                    if(expected==null||triangles[k]!=expected.triangles)throw new InvalidOperationException("Triangle mismatch: "+names[k]);
                }
                if(Mathf.Abs(bounds[3].size.x-48)>.1f)throw new InvalidOperationException("Terrain scale error");
                File.WriteAllText(Path.Combine(output,"model-world-bounds.txt"),string.Join("\n",names.Select((n,i)=>n+": "+bounds[i]+" triangles="+triangles[i])));
                RenderSettings.skybox=null;RenderSettings.ambientMode=AmbientMode.Custom;
                RenderSettings.ambientSkyColor=new Color(.42f,.40f,.42f);RenderSettings.ambientEquatorColor=new Color(.29f,.31f,.38f);RenderSettings.ambientGroundColor=new Color(.16f,.20f,.22f);
                RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=new Color(.36f,.38f,.43f);RenderSettings.fogStartDistance=32;RenderSettings.fogEndDistance=78;
                var sun=new GameObject("Low amber dusk sun").AddComponent<Light>();sun.transform.SetParent(root.transform,false);sun.type=LightType.Directional;
                sun.transform.rotation=Quaternion.Euler(16,-35,0);sun.color=new Color(1,.735f,.46f);sun.intensity=1.65f;sun.shadows=LightShadows.Soft;sun.shadowStrength=.60f;sun.shadowBias=.028f;sun.shadowNormalBias=.12f;RenderSettings.sun=sun;
                var fill=new GameObject("Cool open sky").AddComponent<Light>();fill.transform.SetParent(root.transform,false);fill.type=LightType.Directional;fill.transform.rotation=Quaternion.Euler(48,145,0);fill.color=new Color(.61f,.65f,.88f);fill.intensity=.30f;fill.shadows=LightShadows.None;
                var torch=new GameObject("One shared entrance torch light").AddComponent<Light>();torch.transform.SetParent(root.transform,false);torch.type=LightType.Point;torch.transform.position=new Vector3(-.55f,1.9f,4.75f);torch.color=new Color(1,.65f,.34f);torch.intensity=.52f;torch.range=3.5f;torch.shadows=LightShadows.None;
                var atmosphere=new GameObject("Low layered atmosphere");atmosphere.transform.SetParent(root.transform,false);BuildLowGroundMist(atmosphere);
                var particles=BuildCorruptionParticles(root);if(particles.particleCount<2)throw new InvalidOperationException("No corruption particles");
                var camera=new GameObject("Game camera 50 minus13").AddComponent<Camera>();camera.transform.SetParent(root.transform,false);camera.orthographic=true;camera.nearClipPlane=.1f;camera.farClipPlane=120;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=RenderSettings.fogColor;camera.allowHDR=true;
                camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;camera.GetUniversalAdditionalCameraData().antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                var volume=new GameObject("Warm dusk grading").AddComponent<Volume>();volume.transform.SetParent(root.transform,false);volume.isGlobal=true;
                var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Root+"/Materials/Family_Dusk.asset");
                if(profile==null){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,Root+"/Materials/Family_Dusk.asset");}
                ConfigureDuskProfile(profile);volume.sharedProfile=profile;AssetDatabase.SaveAssets();
                var allBounds=bounds[0];allBounds.Encapsulate(bounds[1]);allBounds.Encapsulate(bounds[2]);
                foreach(var size in new[]{new Vector2Int(1100,1250),new Vector2Int(390,844),new Vector2Int(1600,900),new Vector2Int(844,390)})
                {
                    Fit(camera,allBounds,size.x,size.y,45);Capture(camera,size.x,size.y,Path.Combine(output,$"unity-{size.x}x{size.y}.png"));
                }
                Fit(camera,allBounds,1100,1250,45);EditorSceneManager.SaveScene(scene,Root+"/Scenes/Phase1_HeroFamily.unity");
                for(int k=1;k<=2;k++)
                {
                    for(int j=0;j<4;j++)objects[j].SetActive(j==k);
                    atmosphere.SetActive(false);particles.gameObject.SetActive(k==2);RenderSettings.fog=false;
                    Fit(camera,bounds[k],1100,1250,30);Capture(camera,1100,1250,Path.Combine(output,names[k].ToLowerInvariant()+"-closeup.png"));
                }
                for(int j=0;j<4;j++)objects[j].SetActive(true);atmosphere.SetActive(true);particles.gameObject.SetActive(true);RenderSettings.fog=true;
                File.WriteAllText(Path.Combine(output,"capture.json"),JsonUtility.ToJson(new Proof{unity=UnityEngine.Application.unityVersion,sourceSHA=Environment.GetEnvironmentVariable("GITHUB_SHA"),scene=Root+"/Scenes/Phase1_HeroFamily.unity",heroTriangles=triangles[0],ruinTriangles=triangles[1],corruptTriangles=triangles[2],environmentTriangles=triangles[3],particles=particles.particleCount,note="Actual Unity render. Hero form approved by owner; ruin and corrupt derived from that exact source. Phase1 scene candidate; Phase2 not started. Physical mobile performance untested."},true));
                Debug.Log("REBOOT_CAPTURE_OUTPUT "+output);
            }
            finally
            {
                QualitySettings.renderPipeline=previousPipeline;ShaderUtil.allowAsyncCompilation=previousAsync;
                if(setup.Any(s=>s.isLoaded)&&setup.Count(s=>s.isActive)==1)EditorSceneManager.RestoreSceneManagerSetup(setup);else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                for(int i=0;i<protectedPaths.Length;i++)if(!File.ReadAllBytes(protectedPaths[i]).SequenceEqual(protectedBytes[i]))throw new InvalidOperationException("Protected approved hero/scene bytes changed: "+protectedPaths[i]);
            }
        }
        static void Fit(Camera camera,Bounds bounds,int width,int height,float distance)
        {
            var rotation=Quaternion.Euler(50,-13,0);camera.transform.SetPositionAndRotation(bounds.center-rotation*Vector3.forward*distance,rotation);
            float horizontal=0,vertical=0;
            for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)
            {
                var p=camera.transform.InverseTransformPoint(bounds.center+Vector3.Scale(bounds.extents,new Vector3(x,y,z)));
                horizontal=Mathf.Max(horizontal,Mathf.Abs(p.x));vertical=Mathf.Max(vertical,Mathf.Abs(p.y));
            }
            camera.orthographicSize=Mathf.Max(vertical,horizontal/((float)width/height))*1.10f;
        }
        static void BuildLowGroundMist(GameObject root)
        {
            const string shaderPath = Root + "/Materials/Reboot_Atmosphere.shader";
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
            if (shader == null || ShaderUtil.ShaderHasError(shader))
                throw new InvalidOperationException("URP vertex-alpha atmosphere shader unavailable.");
            const string materialPath = Root + "/Materials/Family_LowMist.mat";
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
                var meshPath = Root + "/Materials/Family_LowMist_" + layer + ".asset";
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
                        float xx = -20f + x * (40f / (n - 1));
                        float zz = -16f + z * (36f / (n - 1));
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
            const string path = Root + "/Materials/Family_ArcaneParticles.mat";
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
            go.transform.localPosition = new Vector3(12.7f, 1.3f, -.2f);
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
            shape.scale = new Vector3(2.1f, 1.6f, 1.8f);
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

        static UniversalRenderPipelineAsset PreparePilotPipeline()
        {
            // The production renderer has no postProcessData. Clone it; never mutate it.
            const string pipelineSource = "Assets/Eldoria/Content/EldoriaMobileURP.asset";
            const string rendererSource = "Assets/Eldoria/Content/EldoriaForwardRenderer.asset";
            const string pipelinePath = Root + "/Materials/Family_URP.asset";
            const string rendererPath = Root + "/Materials/Family_Renderer.asset";
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
            distance.floatValue = 64f;
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

    }
}
