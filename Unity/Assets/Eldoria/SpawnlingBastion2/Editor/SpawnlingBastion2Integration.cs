#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Eldoria.ReferenceClean;
using Eldoria.Presentation;
using Eldoria.SpawnlingBastion2;
using Eldoria.World4XPrototype;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.SpawnlingBastion2.Editor
{
    /// <summary>Actual canonical I-II scene visual replacement; no new game rules.</summary>
    public static class SpawnlingBastion2Integration
    {
        const string ScenePath = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        const string FinishedSource = "../ArtSource/World4X/Spawnling_v1/Finished/";
        const string AssetRoot = "Assets/Eldoria/SpawnlingBastion2/Generated";
        const string VisualName = "Zone1 authored engendro-valoria";
        const string CanonicalId = "engendro-valoria";
        const string SourceSha = "2701e6f6a37920cbe6b434b903aa7b14283514f8276399d14bfffaece0edcc4a";
        static readonly string Evidence = Path.GetFullPath("../docs/evidence/spawnling-bastion2");
        // Near/far GLBs carry equivalent body materials; do not duplicate 2K
        // PBR textures in the mobile build just because they were loaded twice.
        static readonly Dictionary<string,Material> SharedPbrAcrossLods = new Dictionary<string,Material>();
        static void Need(bool ok,string message){if(!ok) throw new InvalidOperationException("Spawnling B2: "+message);}
        static string Hash(string path)
        {
            using(var sha=SHA256.Create())using(var stream=File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
        }
        static Transform Named(GameObject root,string exact)
        {
            return root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name==exact);
        }
        static Bounds VisibleBounds(GameObject obj)
        {
            var rr=obj.GetComponentsInChildren<Renderer>(true);
            Need(rr.Length>0,"No genuine renderers on "+obj.name);
            var b=rr[0].bounds;
            foreach(var r in rr.Skip(1))b.Encapsulate(r.bounds);
            return b;
        }
        static int Triangles(GameObject go)
        {
            return go.GetComponentsInChildren<MeshFilter>(true)
                .Sum(f=>f.sharedMesh?f.sharedMesh.triangles.Length/3:0);
        }
        static void SaveResources(GameObject root,string folder,ref int counter)
        {
            var meshes=new Dictionary<Mesh,Mesh>();
            var mats=new Dictionary<Material,Material>();
            var textures=new Dictionary<Texture,Texture2D>();
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                var source=filter.sharedMesh;
                Need(source!=null && source.vertexCount>3,"Source LOD mesh missing");
                if(!meshes.TryGetValue(source,out var saved))
                {
                    saved=Object.Instantiate(source);
                    saved.name="Engendro B2 / owned mesh";
                    AssetDatabase.CreateAsset(saved,folder+"/mesh-"+(counter++)+".asset");
                    meshes.Add(source,saved);
                }
                filter.sharedMesh=saved;
            }
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                var assigned=renderer.sharedMaterials;
                for(int index=0;index<assigned.Length;index++)
                {
                    var source=assigned[index];
                    Need(source&&source.shader&&!source.shader.name.Contains("InternalError"),
                        "Bad Tripo material or shader on "+renderer.name);
                    if(!mats.TryGetValue(source,out var saved))
                    {
                        if(!SharedPbrAcrossLods.TryGetValue(source.name,out saved))
                        {
                            saved=new Material(source){name="Engendro B2 / "+source.name};
                            foreach(string property in source.GetTexturePropertyNames())
                            {
                                Texture texture=source.GetTexture(property);
                                if(texture==null)continue;
                                if(!textures.TryGetValue(texture,out var copy))
                                {
                                    copy=ReadableTexture(texture);
                                    copy.name="Engendro B2 / retained PBR "+property;
                                    AssetDatabase.CreateAsset(copy,folder+"/tex-"+(counter++)+".asset");
                                    textures.Add(texture,copy);
                                }
                                saved.SetTexture(property,copy);
                            }
                            AssetDatabase.CreateAsset(saved,folder+"/mat-"+(counter++)+".mat");
                            SharedPbrAcrossLods.Add(source.name,saved);
                        }
                        mats.Add(source,saved);
                    }
                    assigned[index]=saved;
                }
                renderer.sharedMaterials=assigned;
                renderer.lightmapIndex=-1;
                renderer.lightProbeUsage=LightProbeUsage.BlendProbes;
                renderer.reflectionProbeUsage=ReflectionProbeUsage.BlendProbes;
                renderer.shadowCastingMode=ShadowCastingMode.On;
                GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,0);
            }
        }
        static Texture2D ReadableTexture(Texture texture)
        {
            bool linear=!UnityEngine.Experimental.Rendering.GraphicsFormatUtility
                .IsSRGBFormat(texture.graphicsFormat);
            var rt=RenderTexture.GetTemporary(texture.width,texture.height,0,
                RenderTextureFormat.ARGB32,linear?RenderTextureReadWrite.Linear:RenderTextureReadWrite.sRGB);
            var previous=RenderTexture.active;
            try
            {
                Graphics.Blit(texture,rt);RenderTexture.active=rt;
                var copy=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,true,linear)
                {
                    wrapMode=texture.wrapMode,filterMode=FilterMode.Trilinear,anisoLevel=2
                };
                copy.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);
                copy.Apply(true,false);
                return copy;
            }
            finally{RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);}
        }
        static async Task<GameObject> ImportAsync(string path,string name,Transform parent,
            string folder,Action<GameObject> persist)
        {
            Need(File.Exists(path),"Reviewed existing Tripo source not found: "+path);
            var root=new GameObject(name);
            root.transform.SetParent(parent,false);
            using(var importer=new GltfImport(null,new UninterruptedDeferAgent()))
            {
                Need(await importer.Load(new Uri(Path.GetFullPath(path)).AbsoluteUri,
                    new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=4}),
                    "glTFast load failed for "+path);
                Need(await importer.InstantiateMainSceneAsync(root.transform),
                    "glTFast scene missing "+path);
                foreach(var l in root.GetComponentsInChildren<Light>(true))
                    Object.DestroyImmediate(l.gameObject);
                foreach(var c in root.GetComponentsInChildren<Camera>(true))
                    Object.DestroyImmediate(c.gameObject);
                Need(Triangles(root)>0,"Imported model lacks real triangles");
                persist(root);
            }
            Need(root.GetComponentsInChildren<MeshFilter>(true).All(f=>
                f.sharedMesh && AssetDatabase.Contains(f.sharedMesh)),
                "Importer disposal would invalidate non-persisted mesh");
            return root;
        }
        static float GroundAt(ValoriaIntegratedWorld host,Vector3 point)
        {
            var ground=host.WorldRoot.GetComponentsInChildren<MeshFilter>(true)
                .FirstOrDefault(f=>f.name=="ZoneOne continuous prairie source");
            Need(ground && ground.sharedMesh,"Existing world ground missing");
            var coll=ground.GetComponent<MeshCollider>();
            bool temporary=coll==null;
            if(temporary)coll=ground.gameObject.AddComponent<MeshCollider>();
            Mesh previous=coll.sharedMesh;
            try
            {
                coll.sharedMesh=ground.sharedMesh;
                Physics.SyncTransforms();
                if(coll.Raycast(new Ray(new Vector3(point.x,200,point.z),Vector3.down),out var hit,400))
                    return hit.point.y;
                throw new InvalidOperationException("Spawnling B2: No ground under retained hotspot");
            }
            finally
            {
                if(temporary)Object.DestroyImmediate(coll);
                else coll.sharedMesh=previous;
            }
        }
        static void Capture(Camera camera,Vector3 center,string path,int width,int height,float zoom)
        {
            var previousTransform=camera.transform.localToWorldMatrix;
            var priorPos=camera.transform.position;
            var priorSize=camera.orthographicSize;
            var priorAspect=camera.aspect;
            var priorTarget=camera.targetTexture;
            var priorActive=RenderTexture.active;
            var priorEnabled=camera.enabled;
            var rt=new RenderTexture(width,height,24);
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                camera.enabled=true;
                camera.orthographicSize=zoom;
                camera.aspect=width/(float)height;
                camera.transform.position=center-camera.transform.forward*90f;
                camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
                tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();
                File.WriteAllBytes(path,tex.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture=priorTarget;camera.aspect=priorAspect;
                camera.orthographicSize=priorSize;camera.transform.position=priorPos;
                camera.enabled=priorEnabled;RenderTexture.active=priorActive;
                rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);
            }
        }
        [Serializable] class Report
        {
            public string scene_sha_before,scene_sha_after,source_glb_sha256,
                near_glb_sha256,far_glb_sha256,scene,unity_version,status;
            public int near_triangles,far_triangles,hotspot_count,gameplay_collider_count,
                visual_renderers,visual_colliders;
            public bool hotspot_preserved,gameplay_unchanged,save_untouched,physical_mobile_tested=false;
            public string[] captures;
        }
        public static async void Integrate()
        {
            try
            {
                Directory.CreateDirectory(Evidence);
                string nearPath=Path.GetFullPath(FinishedSource+"Spawnling_B2_Near.glb");
                string farPath=Path.GetFullPath(FinishedSource+"Spawnling_B2_Far.glb");
                string sourcePath=Path.GetFullPath("../ArtSource/World4X/Spawnling_v1/Optimized_Source.glb");
                Need(File.Exists(sourcePath) && Hash(sourcePath)==SourceSha,
                    "Immutable 20-credit Tripo original was replaced");
                var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
                var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                Need(host && host.WorldRoot && host.WorldCamera && host.City==null,
                    "Expected existing edit-mode canonical city/world host");
                Need(Named(host.WorldRoot,VisualName)==null,
                    "An engendro has already been integrated. Refuse duplicated model");
                var hotspots=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true);
                var targets=hotspots.Where(h=>h.Id==CanonicalId).ToArray();
                Need(targets.Length==1,"Original engendro hotspot duplicated/missing");
                var spot=targets[0];Vector3 location=spot.transform.position;
                int originalColliders=host.WorldRoot.GetComponentsInChildren<Collider>(true).Length;
                int originalHotspots=hotspots.Length;
                string beforeSha=Hash(ScenePath);
                bool active=host.WorldRoot.activeSelf;
                host.WorldRoot.SetActive(true);
                Vector3 focus=new Vector3(location.x,2,location.z);
                Capture(host.WorldCamera,focus,Path.Combine(Evidence,"before-local-900x1600.png"),900,1600,14);
                Capture(host.WorldCamera,focus,Path.Combine(Evidence,"before-region-1600x900.png"),1600,900,32);
                string folder=AssetRoot+"/Run-"+(Environment.GetEnvironmentVariable("GITHUB_RUN_ID")??"local");
                Need(!Directory.Exists(folder),"Never overwrite earlier generated imported art");
                Directory.CreateDirectory(folder);
                SharedPbrAcrossLods.Clear();
                AssetDatabase.Refresh();
                var root=new GameObject(VisualName);
                root.transform.SetParent(host.WorldRoot.transform,true);
                var animator=root.AddComponent<SpawnlingEncounterVisual>();
                animator.Host=host;
                var pivot=new GameObject("Rift idle pivot / visual only");
                pivot.transform.SetParent(root.transform,false);
                animator.MotionPivot=pivot.transform;
                animator.ModelRoot=pivot;
                int count=0;
                // GLTFast resources are runtime objects. Own them before importer's Dispose.
                var near=await ImportAsync(nearPath,"NEAR / paid Tripo source finished",
                    pivot.transform,folder,g=>SaveResources(g,folder,ref count));
                var far=await ImportAsync(farPath,"FAR / paid Tripo source reduced",
                    pivot.transform,folder,g=>SaveResources(g,folder,ref count));
                int nearTris=Triangles(near),farTris=Triangles(far);
                Need(nearTris>=4000&&nearTris<=13000&&farTris>=1000&&farTris<=3300,
                    "Unexpected finished LOD triangle count");
                float modelHeight=VisibleBounds(near).size.y;
                Need(modelHeight>.3f,"Imported original y-up model height implausible");
                root.transform.localScale=Vector3.one*(3.0f/modelHeight);
                // The genuine sculpt faces local +X (verified in Blender captures).
                // Turn that axis toward Valoria, preserving this enemy's original anchor.
                var playerCity=hotspots.FirstOrDefault(h=>h.Id=="valoria-map-city");
                Vector3 horizontal=(playerCity?playerCity.transform.position:
                    host.WorldCamera.transform.position)-location;
                horizontal.y=0;
                if(horizontal.sqrMagnitude>.1f)
                    root.transform.rotation=Quaternion.FromToRotation(Vector3.right,horizontal.normalized);
                Bounds fitted=VisibleBounds(near);
                float ground=GroundAt(host,location);
                root.transform.position+=new Vector3(location.x-fitted.center.x,ground-fitted.min.y,
                    location.z-fitted.center.z);
                var nearRs=near.GetComponentsInChildren<Renderer>(true);
                var farRs=far.GetComponentsInChildren<Renderer>(true);
                var lod=root.AddComponent<LODGroup>();
                lod.SetLODs(new[]{new LOD(.055f,nearRs),new LOD(.006f,farRs)});
                lod.RecalculateBounds();
                Need(root.GetComponentsInChildren<Collider>(true).Length==0,
                    "Visual replacement must not add its own trigger or collider");
                // Former speculative tent structures must remain hidden, not be recreated.
                foreach(string name in new[]{"Zone1 hostile camp engendro-valoria",
                    "Zone1 hostile camp rear shelter","Enemy camp existing shelter"})
                {
                    var old=Named(host.WorldRoot,name);
                    if(old)old.gameObject.SetActive(false);
                }
                Physics.SyncTransforms();
                Capture(host.WorldCamera,focus,Path.Combine(Evidence,"after-local-900x1600.png"),900,1600,14);
                Capture(host.WorldCamera,focus,Path.Combine(Evidence,"after-region-1600x900.png"),1600,900,32);
                Need(host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true).Length==originalHotspots
                    &&spot &&spot.Id==CanonicalId &&spot.transform.position==location,
                    "Existing interactable target or exact position changed");
                Need(host.WorldRoot.GetComponentsInChildren<Collider>(true).Length==originalColliders,
                    "Gameplay physics was modified");
                host.WorldRoot.SetActive(active);
                AssetDatabase.SaveAssets();
                Need(EditorSceneManager.SaveScene(scene,ScenePath),"Integrated player scene save failed");
                var report=new Report{
                    scene=ScenePath,scene_sha_before=beforeSha,scene_sha_after=Hash(ScenePath),
                    near_glb_sha256=Hash(nearPath),far_glb_sha256=Hash(farPath),
                    source_glb_sha256=SourceSha,unity_version=UnityEngine.Application.unityVersion,
                    near_triangles=nearTris,far_triangles=farTris,
                    hotspot_count=originalHotspots,gameplay_collider_count=originalColliders,
                    visual_renderers=nearRs.Length+farRs.Length,visual_colliders=0,
                    hotspot_preserved=true,gameplay_unchanged=true,save_untouched=true,
                    status="SCENE_INTEGRATED_NATIVE_GAMEPLAY_PROBE_REQUIRED",
                    captures=new[]{"before-local-900x1600.png","after-local-900x1600.png",
                        "before-region-1600x900.png","after-region-1600x900.png"}};
                File.WriteAllText(Path.Combine(Evidence,"native-integration.json"),JsonUtility.ToJson(report,true));
                Debug.Log("SPAWNLING_B2_SCENE_INTEGRATED original gameplay and hotspot preserved; before="+beforeSha);
                EditorApplication.Exit(0);
            }
            catch(Exception e)
            {
                Directory.CreateDirectory(Evidence);
                File.WriteAllText(Path.Combine(Evidence,"native-integration-error.txt"),e.ToString());
                Debug.LogException(e);EditorApplication.Exit(1);
            }
        }
    }
}
#endif
