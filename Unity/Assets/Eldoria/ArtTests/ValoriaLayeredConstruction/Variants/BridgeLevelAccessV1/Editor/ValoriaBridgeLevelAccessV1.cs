// Authentic editable Blender bridge variant from approved BridgeFamilyV1, no canonical edits.
// Build from pre-existing verified ABCD; original Blender .blend/GLB immutable.
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
 public static class ValoriaBridgeLevelAccessV1
 {
   const string Root="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction";
   const string BaseScene=Root+"/Valoria_MasterPlan_Layers_ABCD.unity";
   const string Output=Root+"/Valoria_MasterPlan_Layers_ABCD_BridgeV1.unity";
   const string Generated=Root+"/GeneratedBridgeV1";
   const string Source=Root+"/Sources/BridgeLevelAccessV1.glb";
   static readonly string Evidence=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
       "..","..","docs","evidence","valoria-layered-construction-20261016","bridge-v1"));
   static string ExpectedHash;
   static HashSet<int> persisted=new HashSet<int>();static int num;
   static Camera view;
   [Serializable] class SourceManifest{
     public string generated_glb_sha256;
     public float[] unity_bounds_min,unity_bounds_max;
     public int authored_triangles,pavers;
   }
   [Serializable] class Result{
     public string source_sha256,scene,base_scene,visual_status,gameplay_status,camera_status;
     public bool source_hash_verified,unity_scale_one,deck_collision_original_preserved;
     public float bbox_width,bbox_length,walk_surface_y,deck_collider_y;
     public int actual_mesh_renderers,triangle_count;
   }
   [MenuItem("Eldoria/Valoria/Build bridge derived from Blender at true geometry")]
   public static async void Build(){
     try {await Execute();Debug.Log("VALORIA_REAL_BLENDER_BRIDGE_V1_NATIVE_COMPLETE");EditorApplication.Exit(0);}
     catch(Exception e){Debug.LogError("VALORIA_REAL_BLENDER_BRIDGE_V1_NATIVE_FAILED "+e);EditorApplication.Exit(1);}
   }
   static void Check(bool a,string s){if(!a)throw new Exception("BRIDGE_VARIANT_F: "+s);}
   static void Persist(UnityEngine.Object o){
     if(o==null||AssetDatabase.Contains(o)||!persisted.Add(o.GetInstanceID()))return;
     if(!(o is Mesh)&&!(o is Material)&&!(o is Texture2D))return;
     AssetDatabase.CreateAsset(o,Generated+"/Bridge"+(num++).ToString("D5")+(o is Material?".mat":".asset"));
   }
   static void Capture(string id,int w,int h,float zoom) {
     var old=view.orthographicSize;var rtOld=view.targetTexture;var active=RenderTexture.active;
     var rt=new RenderTexture(w,h,24){antiAliasing=4};
     var im=new Texture2D(w,h,TextureFormat.RGB24,false);
     try{
       view.orthographicSize=zoom;view.targetTexture=rt;view.Render();view.Render();
       RenderTexture.active=rt;im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();
       File.WriteAllBytes(Path.Combine(Evidence,id+".png"),im.EncodeToPNG());
     }finally{view.orthographicSize=old;view.targetTexture=rtOld;RenderTexture.active=active;
       rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(im);}
   }
   static async Task Execute(){
     Check(File.Exists(BaseScene),"No verified ABCD scene");
     Check(File.Exists(Source),"Missing derived glb; original GLB cannot be substituted");
     var manifestPath=Path.Combine(Evidence,"BridgeLevelAccessV1-manifest.json");
     Check(File.Exists(manifestPath),"Missing Blender authored source manifest");
     var meta=JsonUtility.FromJson<SourceManifest>(File.ReadAllText(manifestPath));
     ExpectedHash=meta.generated_glb_sha256;
     using(var stream=File.OpenRead(Source))using(var hash=SHA256.Create()){
       var digest=BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
       Check(digest==ExpectedHash,"Variant source hash mismatch");
     }
     var s=EditorSceneManager.OpenScene(BaseScene,OpenSceneMode.Single);
     view=Camera.main;Check(view!=null&&view.orthographic,"Historical orthographic QA camera missing");
     var deck=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
       .FirstOrDefault(t=>t.name=="TEMPORARY bridge / continuous deck and collider");
     Check(deck!=null && deck.GetComponent<BoxCollider>()!=null,"Original validated bridge deck lost");
     // Independent hidden collider and all gate/scaffold remain as tested. Never change PlayerState or LocalGateway.
     Check(Mathf.Abs(deck.position.y+deck.localScale.y*.5f-.38f)<.001f,"Grade changed");
     var gltf=new GltfImport(null,new UninterruptedDeferAgent());
     Check(await gltf.Load(new Uri(Path.GetFullPath(Source)).AbsoluteUri,
         new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=2}),"GLB import failed");
     var root=new GameObject("LAYER BRIDGE V1 - APPROVED FAMILY DERIVATIVE - TRUE SCALE");
     Check(await gltf.InstantiateMainSceneAsync(root.transform),"No variant instantiated");
     var renderers=root.GetComponentsInChildren<Renderer>(true);
     Check(renderers.Length>=4,"Real mesh renderers missing");
     // New Blender variant uses absolute world geometry; import unit 1 and origin 0 are mandatory.
     root.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
     root.transform.localScale=Vector3.one;
     Bounds b=renderers[0].bounds;foreach(var renderer in renderers.Skip(1))b.Encapsulate(renderer.bounds);
     Check(b.min.z<=-19.5f&&b.max.z>=-9.45f,"Variant does not span full existing bridge endpoints");
     Check(b.size.x>=4.35f&&b.size.x<=5.30f,"Native bridge width incompatible");
     Check(b.max.y>1.0f&&b.min.y<-.65f,"Model lacks parapets or real supports");
     // Replace visual scaffold only when native bounds are good. No impact on gameplay collision.
     deck.GetComponent<Renderer>().enabled=false;
     foreach(var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
       if(t.name.StartsWith("GREYBOX bridge parapet")||t.name.StartsWith("GREYBOX bridge pier"))
         foreach(var rr in t.GetComponentsInChildren<Renderer>(true))rr.enabled=false;
     Directory.CreateDirectory(Evidence);if(AssetDatabase.IsValidFolder(Generated))AssetDatabase.DeleteAsset(Generated);
     Directory.CreateDirectory(Generated);AssetDatabase.Refresh();
     foreach(var rr in renderers){
       foreach(var material in rr.sharedMaterials){
         if(material==null)continue;
         foreach(var prop in material.GetTexturePropertyNames())Persist(material.GetTexture(prop));
         Persist(material);
       }
     }
     foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))Persist(filter.sharedMesh);
     foreach(var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))Persist(renderer.sharedMesh);
     AssetDatabase.SaveAssets();
     PrefabUtility.SaveAsPrefabAsset(root,Generated+"/BridgeLevelAccessV1.prefab");
     Physics.SyncTransforms();
     foreach(float z in new[]{-19.4f,-17f,-14.5f,-12f,-9.5f}){
       Check(Physics.Raycast(new Vector3(0,.58f,z),Vector3.down,out var hit,.8f,~0,QueryTriggerInteraction.Ignore),
        "Lost bridge contact at "+z);
       Check(Mathf.Abs(hit.point.y-.38f)<.025f,"Collision road height changed at "+z);
     }
     Check(EditorSceneManager.SaveScene(s,Output,true),"Scene save failed");
     Capture("bridge-v1-zoom12",1536,1024,12);
     Capture("bridge-v1-zoom19",1536,1024,19);
     Capture("bridge-v1-zoom9",1536,1024,9);
     Capture("bridge-v1-portrait",390,844,12);
     var ev=new Result{source_sha256=ExpectedHash,scene=Output,base_scene=BaseScene,
       visual_status="CANDIDATE_A_B_REVIEW_PENDING",gameplay_status="NOT_CERTIFIED",
       camera_status="LEGACY_ORTHOGRAPHIC_NOT_CANONICAL_SHARP",
       source_hash_verified=true,unity_scale_one=true,deck_collision_original_preserved=true,
       bbox_width=b.size.x,bbox_length=b.size.z,walk_surface_y=.38f,deck_collider_y=.38f,
       actual_mesh_renderers=renderers.Length,triangle_count=meta.authored_triangles};
     File.WriteAllText(Path.Combine(Evidence,"bridge-v1-unity.json"),JsonUtility.ToJson(ev,true));
     Debug.Log("VALORIA_BRIDGE_VARIANT_F_NATIVE_PASS width="+b.size.x+" length="+b.size.z+
       " renderers="+renderers.Length);
   }
 }
}
