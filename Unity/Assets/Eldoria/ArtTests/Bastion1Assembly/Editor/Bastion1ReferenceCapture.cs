using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Eldoria.EditorTools
{
    // Dedicated candidate. Never touches canonical Valoria or its build settings.
    public static class Bastion1ReferenceCapture
    {
        const string Study="Assets/Eldoria/ArtTests/Bastion1Assembly";
        static readonly Dictionary<UnityEngine.Object,string> Saved=new Dictionary<UnityEngine.Object,string>();
        [MenuItem("Eldoria/Art/Build Bastion I reference assembly candidate")]
        public static async void Capture()
        {
            var previousGraphics=GraphicsSettings.defaultRenderPipeline;
            var previousQuality=QualitySettings.renderPipeline;
            try
            {
                // The preceding batch capture can serialize its temporary StudyURP
                // into GraphicsSettings. Deleting that generated asset on the next
                // capture otherwise leaves a null pipeline. Resolve the immutable
                // project URP asset by its committed GUID for every matched frame.
                var original=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(
                    AssetDatabase.GUIDToAssetPath("48e3a09b57f028a4bb2e693d044ea6ca"));
                if(original==null)throw new Exception("Canonical committed URP asset unavailable");
                var pipeline=UnityEngine.Object.Instantiate(original);
                pipeline.name="Bastion I comparative URP, unprofiled";
                pipeline.shadowDistance=180f;pipeline.shadowCascadeCount=1;
                pipeline.msaaSampleCount=1;pipeline.useSRPBatcher=true;
                var settings=new SerializedObject(pipeline);
                var shadowResolution=settings.FindProperty("m_MainLightShadowmapResolution");
                if(shadowResolution==null)throw new Exception("URP main shadow resolution unavailable");
                shadowResolution.intValue=1024;settings.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("BASTION1_MOBILE_PROFILE shadow_distance=180 cascades=1 shadow_atlas=1024 msaa=1 geometry_unchanged=true");
                Directory.CreateDirectory(Study+"/Generated");
                AssetDatabase.DeleteAsset(Study+"/Generated/StudyURP.asset");
                AssetDatabase.CreateAsset(pipeline,Study+"/Generated/StudyURP.asset");
                GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;
                DebugManager.instance.Reset();
                var active=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var source=Path.GetFullPath(Study+"/GeneratedSource/Bastion1ReferenceAssembly.glb");
                if(!File.Exists(source))throw new FileNotFoundException("Run the exact source assembler first",source);
                var importer=new GltfImport(null,new UninterruptedDeferAgent());
                if(!await importer.Load(new Uri(source).AbsoluteUri,new ImportSettings { GenerateMipMaps=true,AnisotropicFilterLevel=2 }))throw new Exception("glTFast source import failed");
                var root=new GameObject("BastionI_ReferenceAssembly_CANDIDATE");
                if(!await importer.InstantiateMainSceneAsync(root.transform))throw new Exception("glTFast instantiate failed");
                Bastion1MaterialBindings.Apply(root,importer,source);
                Bastion1RoadClearance.Apply(root);
                await Bastion1ExistingRoadIntegration.Apply(root);
                await StageSelectedLegacyRockContacts(root);
                InspectRoadContacts(root);
                var sun=new GameObject("Bastion I afternoon sun").AddComponent<Light>();
                sun.type=LightType.Directional;sun.color=new Color(1f,.86f,.72f);sun.intensity=1.25f;
                sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(38f,-38f,0f);
                RenderSettings.ambientMode=AmbientMode.Trilight;
                RenderSettings.ambientSkyColor=new Color(.37f,.46f,.61f);
                RenderSettings.ambientEquatorColor=new Color(.38f,.40f,.38f);
                RenderSettings.ambientGroundColor=new Color(.31f,.27f,.22f);
                RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.0028f;
                RenderSettings.fogColor=new Color(.30f,.38f,.45f);
                foreach(var p in new[]{new Vector3(-4,2,20),new Vector3(4,2,20),new Vector3(-4,5.5f,-21),new Vector3(4,5.5f,-21)})
                {
                    var warm=new GameObject("Bastion I amber practical").AddComponent<Light>();
                    warm.type=LightType.Point;warm.color=new Color(1,.42f,.13f);warm.intensity=2.4f;warm.range=7f;
                    warm.shadows=LightShadows.None;warm.transform.position=p;
                }
                var camera=new GameObject("Bastion I strategic camera").AddComponent<Camera>();
                camera.orthographic=true;camera.orthographicSize=26f;
                camera.nearClipPlane=.1f;camera.farClipPlane=350f;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.25f,.34f,.42f);
                // glTF exporter: Blender (x,y,z)->(x,z,-y); glTFast mirrors x.
                camera.transform.position=new Vector3(-32f,49f,76f);camera.transform.LookAt(Vector3.zero);
                var controls=camera.gameObject.AddComponent<Eldoria.Art.Bastion1AssemblyCamera>();
                controls.ConfigureHome();
                // Existing authored world-asset depth; no generated geometry or future buildings.
                var directory=Path.GetFullPath("Bastion1ReferenceCaptures");Directory.CreateDirectory(directory);
                foreach(var stale in Directory.GetFiles(directory,"*.png"))File.Delete(stale);
                Render(camera,Path.Combine(directory,"bastion1-unity-landscape.png"),1440,1000);
                Render(camera,Path.Combine(directory,"bastion1-unity-mobile-landscape.png"),780,520);
                // Portrait shows complete construction envelope, without artificial close zoom.
                camera.orthographicSize=76f;
                Render(camera,Path.Combine(directory,"bastion1-unity-portrait.png"),390,844);
                camera.orthographicSize=26f;
                // Focalized source check: relative clamps cannot teleport this camera
                // to the old image-plane origin on the first player drag.
                var home=camera.transform.position;
                controls.PanByScreenDelta(new Vector2(100000f,-100000f));
                var moved=camera.transform.position-home;
                if(Mathf.Abs(moved.x)>12.001f||Mathf.Abs(moved.z)>12.001f||moved.sqrMagnitude<.01f)
                    throw new Exception("Candidate camera pan bounds invalid");
                controls.ZoomBy(-10000f);controls.Home();
                if(Vector3.Distance(camera.transform.position,home)>.001f)
                    throw new Exception("Candidate Home pose was not restored");
                var renderers=root.GetComponentsInChildren<Renderer>(true);
                var uniqueMeshes=new HashSet<Mesh>();var uniqueMaterials=new HashSet<Material>();var uniqueTextures=new HashSet<Texture>();long triangles=0;
                AssetDatabase.StartAssetEditing();
                try
                {
                foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))
                {
                    if(filter.sharedMesh==null)continue;
                    triangles+=filter.sharedMesh.triangles.Length/3;uniqueMeshes.Add(filter.sharedMesh);Persist(filter.sharedMesh);
                }
                foreach(var renderer in renderers)foreach(var material in renderer.sharedMaterials)
                {
                    if(material==null)continue;uniqueMaterials.Add(material);
                    foreach(var property in material.GetTexturePropertyNames())
                    {var texture=material.GetTexture(property);if(texture!=null){uniqueTextures.Add(texture);Persist(texture);}}
                    Persist(material);
                }
                }
                finally { AssetDatabase.StopAssetEditing(); }
                // Only selection anchors; neither coarse gate blockers nor invented buildings.
                var upper=Find(root.transform,"UpperBastion");
                if(upper!=null)
                {
                    var handle=new GameObject("bastion.selection.candidate");handle.transform.SetParent(upper,false);
                    var box=handle.AddComponent<BoxCollider>();box.center=new Vector3(0,3,0);box.size=new Vector3(10,6,6);box.isTrigger=true;
                }
                AssetDatabase.SaveAssets();
                EditorSceneManager.SaveScene(active,Study+"/Bastion1ReferenceAssembly.unity");
                await ValoriaBastion1MeshPlayerScene.Build(active,root,camera);
                string evidence="{\"engine\":\"Unity 6000.3.23f1\",\"renderers\":"+renderers.Length+
                    ",\"shared_meshes\":"+uniqueMeshes.Count+",\"shared_materials\":"+uniqueMaterials.Count+
                    ",\"shared_textures\":"+uniqueTextures.Count+",\"visible_triangles\":"+triangles+
                    ",\"art_pass\":false,\"mobile_performance_pass\":false,\"gameplay_pass\":false,\"main_promotion_allowed\":false}";
                File.WriteAllText(Path.Combine(directory,"unity-evidence.json"),evidence);
                GraphicsSettings.defaultRenderPipeline=previousGraphics;QualitySettings.renderPipeline=previousQuality;
                Debug.Log("BASTION1_REFERENCE_CAPTURE_COMPLETE "+evidence);
                EditorApplication.Exit(0);
            }
            catch(Exception ex)
            {
                GraphicsSettings.defaultRenderPipeline=previousGraphics;QualitySettings.renderPipeline=previousQuality;
                Debug.LogError("BASTION1_REFERENCE_CAPTURE_FAILED "+ex);EditorApplication.Exit(1);
            }
        }
        // Exact existing PR51 models; preserve source meshes and all gameplay objects.
        static async Task StageSelectedLegacyRockContacts(GameObject assembly)
        {
            var terrain=Find(assembly.transform,"Valoria natural playable hilltop and rock scarps");
            if(terrain==null)throw new Exception("Original playable terrain missing");
            var colliders=new List<MeshCollider>();
            var allowed=new HashSet<Collider>();
            var target=new GameObject("Bastion I two recovered rock details");
            target.transform.SetParent(assembly.transform,false);
            try
            {
                foreach(var mf in terrain.GetComponentsInChildren<MeshFilter>(true))
                {
                    if(mf.sharedMesh==null)continue;
                    var c=mf.gameObject.AddComponent<MeshCollider>();
                    c.sharedMesh=mf.sharedMesh;colliders.Add(c);allowed.Add(c);
                }
                if(colliders.Count==0)throw new Exception("Grounding mesh missing");
                Physics.SyncTransforms();
                await ImportSelectedGrounded(target.transform,"RockToWallTransition",
                    "116adcc98a676c249cf4ff0f4a33b5a7fc59f2f0469ba8a10e0513cb021326e0",
                    new Vector2(-23f,14f),4.8f,112f,allowed);
                await ImportSelectedGrounded(target.transform,"BroadRockPlatform",
                    "832d42f03c73495ce4d0eab248b9db555b9836e5fe443eeed19fff670d6f2520",
                    new Vector2(24f,12f),2.1f,198f,allowed);
            }
            finally
            {
                foreach(var c in colliders)UnityEngine.Object.DestroyImmediate(c);
                Physics.SyncTransforms();
            }
            Debug.Log("BASTION1_EXISTING_SELECTED_DETAIL_COMPLETE pieces=2 gameplay_colliders=0 new_geometry=0");
        }
        static async Task ImportSelectedGrounded(Transform parent,string id,string sha256,
            Vector2 site,float height,float rotation,HashSet<Collider> groundColliders)
        {
            var file=Path.GetFullPath(Study+"/GeneratedSource/"+id+".glb");
            if(!File.Exists(file))throw new FileNotFoundException("Selected existing GLB not staged",file);
            using(var hash=SHA256.Create())
            using(var stream=File.OpenRead(file))
            {
                var real=BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
                if(real!=sha256)throw new Exception("Frozen source SHA mismatch "+id);
            }
            var importer=new GltfImport(null,new UninterruptedDeferAgent());
            if(!await importer.Load(new Uri(file).AbsoluteUri,new ImportSettings
                {GenerateMipMaps=true,AnisotropicFilterLevel=2}))
                throw new Exception("Original selected GLB import failed "+id);
            var visual=new GameObject("Selected authored environmental piece "+id);
            visual.transform.SetParent(parent,false);
            if(!await importer.InstantiateMainSceneAsync(visual.transform))
                throw new Exception("Original selected mesh instantiate failed "+id);
            Bastion1MaterialBindings.Apply(visual,importer,file);
            foreach(var collider in visual.GetComponentsInChildren<Collider>(true))
                UnityEngine.Object.DestroyImmediate(collider);
            foreach(var script in visual.GetComponentsInChildren<MonoBehaviour>(true))
                UnityEngine.Object.DestroyImmediate(script);
            var renderers=visual.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0)throw new Exception("Selected mesh has no renderer "+id);
            var bounds=renderers[0].bounds;
            foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            if(bounds.size.y<=.001f)throw new Exception("Selected mesh dimension invalid "+id);
            visual.transform.localScale*=height/bounds.size.y;
            visual.transform.rotation=Quaternion.Euler(0f,rotation,0f);
            bounds=renderers[0].bounds;
            foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            var hits=Physics.RaycastAll(new Vector3(site.x,90f,site.y),Vector3.down,
                145f,~0,QueryTriggerInteraction.Ignore);
            bool found=false;Vector3 contact=Vector3.zero;
            foreach(var hit in hits)
            {
                if(!groundColliders.Contains(hit.collider)||hit.normal.y<.68f)continue;
                if(!found||hit.point.y>contact.y){contact=hit.point;found=true;}
            }
            if(!found)throw new Exception("No real terrain at selected model socket "+id);
            visual.transform.position+=new Vector3(contact.x-bounds.center.x,
                contact.y-bounds.min.y-.06f,contact.z-bounds.center.z);
            foreach(var r in renderers)r.shadowCastingMode=ShadowCastingMode.Off;
            Debug.Log("BASTION1_SELECTED_REAL_CONTACT id="+id+" xyz="+visual.transform.position+
                " source="+sha256+" height="+height+" renderers="+renderers.Length);
        }

        static Transform Find(Transform root,string name)
        {foreach(var item in root.GetComponentsInChildren<Transform>(true))if(item.name==name)return item;return null;}
        internal static void Persist(UnityEngine.Object obj)
        {
            if(obj==null||AssetDatabase.Contains(obj)||Saved.ContainsKey(obj))return;
            string path=Study+"/Generated/"+Saved.Count.ToString("D4")+".asset";
            AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(obj,path);Saved.Add(obj,path);
        }
        internal static void Render(Camera camera,string path,int width,int height)
        {
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){antiAliasing=1};
            var prior=RenderTexture.active;Texture2D pixels=null;
            bool priorAsync=ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation=false;
            try
            {
                camera.aspect=(float)width/height;camera.targetTexture=rt;
                // Compile exact variants synchronously; discard GPU setup frames.
                camera.Render();camera.Render();camera.Render();RenderTexture.active=rt;
                pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
                pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();
                var corner=pixels.GetPixel(0,0);bool nonempty=false;
                for(int y=12;y<height;y+=24)for(int x=12;x<width;x+=24)
                    if(Vector4.Distance(pixels.GetPixel(x,y),corner)>.03f)nonempty=true;
                if(!nonempty)throw new Exception("Empty rendered scene: "+path);
                File.WriteAllBytes(path,pixels.EncodeToPNG());
                File.WriteAllText(path+".camera.json",JsonUtility.ToJson(new CameraEvidence
                {
                    width=width,height=height,position=camera.transform.position,
                    rotation=camera.transform.rotation,orthographicSize=camera.orthographicSize,
                    worldToCamera=camera.worldToCameraMatrix,projection=camera.projectionMatrix,
                    colorSpace=QualitySettings.activeColorSpace.ToString()
                },true));
            }
            finally
            {
                ShaderUtil.allowAsyncCompilation=priorAsync;
                camera.targetTexture=null;RenderTexture.active=prior;
                if(pixels!=null)UnityEngine.Object.DestroyImmediate(pixels);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
            }
        }
        [Serializable] class CameraEvidence
        {
            public int width,height;
            public Vector3 position;
            public Quaternion rotation;
            public float orthographicSize;
            public Matrix4x4 worldToCamera,projection;
            public string colorSpace;
        }
        static void InspectRoadContacts(GameObject root)
        {
            var temporary=new List<MeshCollider>();
            try
            {
                foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))
                {
                    string name=filter.name.ToLowerInvariant();
                    if(!name.Contains("stone_path")&&!name.Contains("mainroad")&&!name.Contains("stairs")&&!name.Contains("playable hilltop")&&
                       !name.Contains("cliff plateau"))continue;
                    if(filter.sharedMesh==null)continue;
                    var collider=filter.gameObject.AddComponent<MeshCollider>();
                    collider.sharedMesh=filter.sharedMesh;temporary.Add(collider);
                }
                Physics.SyncTransforms();
                foreach(float z in new[]{-12f,-10f,-8f,-6f,-5f,-2f,2f,6f,10f,14f})
                {
                    var hits=Physics.RaycastAll(new Vector3(0,30,z),Vector3.down,80f);
                    Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
                    foreach(var hit in hits)
                        Debug.Log("BASTION1_REAL_ROAD_CONTACT z="+z+" surface="+hit.collider.name+
                            " parent="+hit.collider.transform.parent?.name+" y="+hit.point.y);
                }
            }
            finally
            {
                foreach(var collider in temporary)UnityEngine.Object.DestroyImmediate(collider);
                Physics.SyncTransforms();
            }
        }
    }
}
