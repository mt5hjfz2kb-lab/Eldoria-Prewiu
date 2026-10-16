// Isolated next layer on top of the validated master plan ABC.
// Genuine six existing GLB sources (hash-pinned); no modification to their owner branch.
// Candidate is NOT ART_APPROVED, NOT CANONICAL, NOT GAMEPLAY CERTIFIED.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using GLTFast;

namespace Eldoria.ValoriaLayered.Editor
{
    public static class ValoriaModularGateLayerD
    {
        const string Root="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction";
        const string Input=Root+"/Sources";
        const string Generated=Root+"/GeneratedD";
        const string BaseScene=Root+"/Valoria_MasterPlan_Layers_ABC.unity";
        const string Output=Root+"/Valoria_MasterPlan_Layers_ABCD.unity";
        static readonly string Evidence=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
            "..","..","docs","evidence","valoria-layered-construction-20261016"));
        static readonly string[] Ids={"tower_square_short","wall_panel_crenellated_4m",
            "arch_complete_major","portcullis_large","reinforced_oak_gate","banner_royal_blue"};
        static readonly string[] Hashes={
            "a514d97fc3241477f4b98b7e4d04b1213d75a1718e53d662fca9e83be8dfd4ac",
            "476d4cd3bb9d220845489833b3386311c1994b6db55ab3353571cf564a6dd05f",
            "b2026a1046e8e8b64556b440e00aed8dd4a788aab599ca3c60582b02e8640bfa",
            "09492e60b841e27bf4b65c532b9b973ec0e2dd1cebb2738beafabcbde52f3c1d",
            "6b1ad6f41c0ce11341281caf6d9ba21438771119443ced089f6f5de691e94a98",
            "be95150ecaf59a92946974cdfe84e0b416df1ec1ae320ea129ceb38b39b8a4af"
        };
        static readonly Dictionary<string,GameObject> templates=new Dictionary<string,GameObject>();
        static readonly HashSet<int> persisted=new HashSet<int>();
        static int assetNumber;
        static Camera view;
        [Serializable] sealed class EvidenceRow
        {
            public string unity,scene,base_scene,classification,art_gate,gameplay_gate;
            public bool original_library_sources_immutable,is_canonical,full_scene_visual_pass,mobile_profiled;
            public string[] glb_sha256,placed_parts;
            public int genuine_glb_types,module_instances,renderer_count;
        }
        [MenuItem("Eldoria/Valoria/Build D authentic gate above validated ABC")]
        public static async void Build()
        {
            try
            {
                await BuildInternal();
                Debug.Log("VALORIA_LAYERED_AUTHENTIC_GATE_D_COMPLETE");
                EditorApplication.Exit(0);
            }
            catch(Exception error)
            {
                Debug.LogError("VALORIA_LAYERED_AUTHENTIC_GATE_D_FAILED "+error);
                EditorApplication.Exit(1);
            }
        }
        static void Verify(bool ok,string error){if(!ok)throw new Exception("VALORIA_GATE_D "+error);}
        static Bounds Extents(GameObject o)
        {
            var rs=o.GetComponentsInChildren<Renderer>(true);
            Verify(rs.Length>0,"No rendered GLB "+o.name);
            Bounds b=rs[0].bounds;
            foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);
            return b;
        }
        static async Task LoadModule(string id,Transform sources)
        {
            string p=Path.GetFullPath(Input+"/"+id+".glb");
            Verify(File.Exists(p),"Missing immutable input "+p);
            using(var f=File.OpenRead(p))using(var hash=SHA256.Create()){
                string got=BitConverter.ToString(hash.ComputeHash(f)).Replace("-","").ToLowerInvariant();
                Verify(got==Hashes[Array.IndexOf(Ids,id)],"GLB hash mismatch "+id+" "+got);
            }
            var gltf=new GltfImport(null,new UninterruptedDeferAgent());
            Verify(await gltf.Load(new Uri(p).AbsoluteUri,
                 new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=2}),
                 "gltf import failed "+id);
            var t=new GameObject("_ORIGINAL_LIBRARY_SOURCE_"+id);
            t.transform.SetParent(sources,false);
            Verify(await gltf.InstantiateMainSceneAsync(t.transform),"gltf instance failed "+id);
            Verify(t.GetComponentsInChildren<Renderer>(true).Length>0,"No imported renderer "+id);
            t.SetActive(false);
            templates.Add(id,t);
        }
        static GameObject Place(string id,Transform parent,Vector3 centre,float desiredSourceXWidth,float yaw=0)
        {
            var o=UnityEngine.Object.Instantiate(templates[id],parent);
            o.name="PRODUCTION_SOURCE_CANDIDATE__"+id;
            o.SetActive(true);
            o.transform.position=centre;
            o.transform.rotation=Quaternion.identity;
            var sourceBounds=Extents(o);
            float factor=desiredSourceXWidth/Mathf.Max(.03f,sourceBounds.size.x);
            Verify(factor>=.22f&&factor<=5.2f,"Nonuniform or implausible scale "+id+": "+factor);
            o.transform.localScale*=factor;
            o.transform.rotation=Quaternion.Euler(0,yaw,0);
            var newBounds=Extents(o);
            o.transform.position+=new Vector3(centre.x-newBounds.center.x,
                centre.y-newBounds.min.y,centre.z-newBounds.center.z);
            // No new gameplay interaction or collisions: existing gameplay authority is protected.
            return o;
        }
        static void Persist(UnityEngine.Object obj)
        {
            if(obj==null||AssetDatabase.Contains(obj)||!persisted.Add(obj.GetInstanceID()))return;
            if(!(obj is Mesh)&&!(obj is Material)&&!(obj is Texture2D))return;
            string ext=obj is Material?".mat":".asset";
            AssetDatabase.CreateAsset(obj,Generated+"/D"+(assetNumber++).ToString("D5")+ext);
        }
        static void Capture(string label,int width,int height,float ortho)
        {
            var pos=view.transform.position;float prior=view.orthographicSize;
            var target=view.targetTexture;var active=RenderTexture.active;
            var rt=new RenderTexture(width,height,24){antiAliasing=4};
            var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            try {
                view.orthographicSize=ortho;
                view.targetTexture=rt;view.Render();view.Render();
                RenderTexture.active=rt;
                texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();
                File.WriteAllBytes(Path.Combine(Evidence,label+".png"),texture.EncodeToPNG());
            } finally {
                view.transform.position=pos;view.orthographicSize=prior;view.targetTexture=target;
                RenderTexture.active=active;rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);
            }
        }
        static async Task BuildInternal()
        {
            Verify(File.Exists(BaseScene),"Cannot layer D before native ABC scene exists");
            foreach(string id in Ids)Verify(File.Exists(Input+"/"+id+".glb"),"Missing selected actual GLB "+id);
            EditorSceneManager.OpenScene(BaseScene,OpenSceneMode.Single);
            view=Camera.main;
            Verify(view!=null&&view.orthographic,"Missing correct historical QA camera");
            var layer=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
                .FirstOrDefault(t=>t.name.StartsWith("LAYER C - architectural blockout"));
            Verify(layer!=null,"Certified ABC layer C not present");
            // Keep the architectural placeholders as hidden SUPPORT and collision context.
            int hidden=0;
            foreach(Transform t in layer){
                if(t.name.StartsWith("GREYBOX lower gate")||
                   t.name.StartsWith("GREYBOX gate arch")||
                   t.name.StartsWith("GREYBOX gate base")||
                   t.name.StartsWith("TEMPORARY gate leaf")){
                    foreach(var renderer in t.GetComponentsInChildren<Renderer>(true)){
                        renderer.enabled=false;hidden++;
                    }
                }
            }
            Verify(hidden>=8,"Source greybox gate not found: refusing unrelated scene");
            if(AssetDatabase.IsValidFolder(Generated))AssetDatabase.DeleteAsset(Generated);
            Directory.CreateDirectory(Generated);Directory.CreateDirectory(Evidence);
            AssetDatabase.Refresh();
            Transform sources=new GameObject("_GLB_LIBRARY_INPUT_TEMPLATES_HIDDEN").transform;
            Transform gate=new GameObject("LAYER D - modular lower gate from six real source GLBs").transform;
            foreach(var id in Ids)await LoadModule(id,sources);
            sources.gameObject.SetActive(false);
            var placed=new List<string>();
            void Add(string id,Vector3 at,float width,float yaw=0){
                Place(id,gate,at,width,yaw);
                placed.Add(id);
            }
            // Exact certified lower gate landmark remains X=0, Z=-9.25.
            // Imported source bounds, UNIFORM scale and Y contact are measured before placement.
            Add("tower_square_short",new Vector3(-3.3f,.38f,-9.25f),2.65f);
            Add("tower_square_short",new Vector3(3.3f,.38f,-9.25f),2.65f);
            Add("arch_complete_major",new Vector3(0,.38f,-9.25f),4.1f);
            Add("portcullis_large",new Vector3(0,.38f,-9.50f),3.05f);
            Add("reinforced_oak_gate",new Vector3(0,.38f,-8.90f),2.95f);
            foreach(float x in new[]{-7.75f,-5.35f,5.35f,7.75f})
                Add("wall_panel_crenellated_4m",new Vector3(x,.38f,-9.25f),2.50f);
            Add("banner_royal_blue",new Vector3(-3.3f,2.8f,-9.75f),.70f);
            Add("banner_royal_blue",new Vector3(3.3f,2.8f,-9.75f),.70f);
            // No forced use of 63: this is one coherent architectural ensemble, not scattering.
            Verify(placed.Count==11,"Gate needs eleven selected module instances");
            Physics.SyncTransforms();
            foreach(var renderer in sources.GetComponentsInChildren<Renderer>(true).Concat(
                gate.GetComponentsInChildren<Renderer>(true))){
                foreach(var material in renderer.sharedMaterials){
                    if(material==null)continue;
                    foreach(var prop in material.GetTexturePropertyNames())Persist(material.GetTexture(prop));
                    Persist(material);
                }
            }
            foreach(var filter in sources.GetComponentsInChildren<MeshFilter>(true))Persist(filter.sharedMesh);
            foreach(var filter in gate.GetComponentsInChildren<MeshFilter>(true))Persist(filter.sharedMesh);
            foreach(var renderer in sources.GetComponentsInChildren<SkinnedMeshRenderer>(true))Persist(renderer.sharedMesh);
            foreach(var renderer in gate.GetComponentsInChildren<SkinnedMeshRenderer>(true))Persist(renderer.sharedMesh);
            AssetDatabase.SaveAssets();
            // Save prefab *after* all imported meshes/materials have persistent Unity asset identity.
            PrefabUtility.SaveAsPrefabAsset(gate.gameObject,Generated+"/LowerGate_Real6_Modular.prefab");
            Verify(EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),Output,true),"Save source scene D");
            Capture("master-gate-D-home-12",1536,1024,12);
            Capture("master-gate-D-home-19",1536,1024,19);
            Capture("master-gate-D-portrait-12",390,844,12);
            var evidence=new EvidenceRow {
                unity=UnityEngine.Application.unityVersion,scene=Output,base_scene=BaseScene,
                classification="PRODUCTION ART SOURCE CANDIDATE / GREYBOX ENVIRONMENT",
                art_gate="NOT_APPROVED_NO_INDEPENDENT_AB",gameplay_gate="NOT_CERTIFIED",
                original_library_sources_immutable=true,is_canonical=false,
                full_scene_visual_pass=false,mobile_profiled=false,
                glb_sha256=Ids.Select((id,i)=>id+" "+Hashes[i]).ToArray(),
                placed_parts=placed.ToArray(),genuine_glb_types=Ids.Length,module_instances=placed.Count,
                renderer_count=gate.GetComponentsInChildren<Renderer>(true).Length
            };
            File.WriteAllText(Path.Combine(Evidence,"lower-gate-D-evidence.json"),JsonUtility.ToJson(evidence,true));
            Debug.Log("VALORIA_LAYERED_D_REAL_MODULES loaded="+Ids.Length+" instances="+placed.Count);
        }
    }
}
