// Native Unity overlay of authored Blender terrain above certified Valoria structural supports.
// All source colliders and plot geometry preserved; independent scene candidate.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using GLTFast;
namespace Eldoria.ValoriaLayered.Editor
{
 public static class ValoriaTerrainRiverCliffV1
 {
    const string Root="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction";
    const string BaseScene=Root+"/Valoria_MasterPlan_Layers_ABCD_BridgeV1.unity";
    const string NewScene=Root+"/Valoria_MasterPlan_BridgeTerrainV1.unity";
    const string Source=Root+"/Sources/ValoriaTerrainRiverCliffV1.glb";
    const string Assets=Root+"/GeneratedTerrainV1";
    static readonly string Evidence=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..",
         "docs","evidence","valoria-layered-construction-20261016","terrain-v1"));
    static HashSet<int> persisted=new HashSet<int>();static int ix;
    [Serializable] sealed class Input {public string export_glb_sha256; public int triangles,mesh_count;}
    [Serializable] sealed class Result {
       public string scene,source_sha256,status,art_status,gameplay_status,camera_authority;
       public bool original_colliders_retained,original_plots_retained,bridge_variant_retained,public_scene_modified;
       public int source_triangles,source_renderers,bridge_rays,stair_rays;
    }
    [MenuItem("Eldoria/Valoria/Build authored riverside terrain above bridge")]
    public static async void Build(){
       try{await Execute();Debug.Log("VALORIA_TERRAIN_RIVER_CLIFF_NATIVE_COMPLETE");EditorApplication.Exit(0);}
       catch(Exception e){Debug.LogError("VALORIA_TERRAIN_RIVER_CLIFF_NATIVE_FAILED "+e);EditorApplication.Exit(1);}
    }
    static void Req(bool ok,string msg){if(!ok)throw new Exception("TERRAIN_RIVER_V1: "+msg);}
    static void Persist(UnityEngine.Object o) {
       if(o==null||AssetDatabase.Contains(o)||!persisted.Add(o.GetInstanceID()))return;
       if(!(o is Mesh)&&!(o is Material)&&!(o is Texture2D))return;
       AssetDatabase.CreateAsset(o,Assets+"/T"+(ix++).ToString("D5")+(o is Material?".mat":".asset"));
    }
    static void Snap(Camera view,string id,float zoom,int width,int height) {
       var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
       var rt=new RenderTexture(width,height,24){antiAliasing=4};
       var old=view.targetTexture;var act=RenderTexture.active;var orth=view.orthographicSize;
       try{
           view.orthographicSize=zoom;view.targetTexture=rt;view.Render();view.Render();
           RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();
           File.WriteAllBytes(Path.Combine(Evidence,id+".png"),tex.EncodeToPNG());
       }finally{view.targetTexture=old;view.orthographicSize=orth;RenderTexture.active=act;
           rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
    }
    static async Task Execute(){
       Req(File.Exists(BaseScene),"BridgeV1 native scene required first");
       Req(File.Exists(Source),"No Blender terrain GLB");
       var m=JsonUtility.FromJson<Input>(File.ReadAllText(Path.Combine(Evidence,"ValoriaTerrainRiverCliffV1-manifest.json")));
       using(var stream=File.OpenRead(Source))using(var hash=SHA256.Create()){
           var sha=BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
           Req(sha==m.export_glb_sha256,"Terrain mesh hash changed");
       }
       var scene=EditorSceneManager.OpenScene(BaseScene,OpenSceneMode.Single);
       var camera=Camera.main;Req(camera!=null&&camera.orthographic,"Historical QA camera missing");
       var all=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
       Req(all.Any(t=>t.name=="LAYER BRIDGE V1 - APPROVED FAMILY DERIVATIVE - TRUE SCALE"),
         "Approved bridge variant absent");
       var deck=all.FirstOrDefault(t=>t.name=="TEMPORARY bridge / continuous deck and collider");
       Req(deck!=null&&deck.GetComponent<BoxCollider>()!=null,"Certified bridge collider removed");
       var gltf=new GltfImport(null,new UninterruptedDeferAgent());
       Req(await gltf.Load(new Uri(Path.GetFullPath(Source)).AbsoluteUri,
           new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=2}),"Terrain GLB import failure");
       var root=new GameObject("LAYER TERRAIN V1 - AUTHORED ROCK SLOPES AND RIVER");
       Req(await gltf.InstantiateMainSceneAsync(root.transform),"Terrain source missing");
       root.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
       root.transform.localScale=Vector3.one;
       var renderers=root.GetComponentsInChildren<Renderer>(true);
       Req(renderers.Length>=4,"Terrain source meshes not present");
       // Preserve structural geometry, collider and certified route. Replace VISUAL support only.
       string[] hidePrefixes={"GREYBOX continuous rocky substrate","GREYBOX VPD · continuous terrain",
          "GREYBOX VPD · master west terrain apron","GREYBOX VPD · master east terrain apron",
          "TEMPORARY far shore ground","SUPPORT VPD · L1 support",
          "SUPPORT upper civic underfill","SUPPORT civic link underfill"};
       foreach(var t in all){
           if(hidePrefixes.Any(prefix=>t.name.StartsWith(prefix)))
              foreach(var rend in t.GetComponentsInChildren<Renderer>(true))rend.enabled=false;
       }
       if(AssetDatabase.IsValidFolder(Assets))AssetDatabase.DeleteAsset(Assets);
       Directory.CreateDirectory(Assets);AssetDatabase.Refresh();
       foreach(var rr in renderers)foreach(var mt in rr.sharedMaterials){
           if(mt==null)continue;
           foreach(var p in mt.GetTexturePropertyNames())Persist(mt.GetTexture(p));
           Persist(mt);
       }
       foreach(var f in root.GetComponentsInChildren<MeshFilter>(true))Persist(f.sharedMesh);
       AssetDatabase.SaveAssets();
       PrefabUtility.SaveAsPrefabAsset(root,Assets+"/ValoriaTerrainRiverCliffV1.prefab");
       Physics.SyncTransforms();
       int bridge=0,stairs=0;
       foreach(float z in new[]{-19f,-17f,-14.5f,-12f,-9.5f}){
           Req(Physics.Raycast(new Vector3(0,.58f,z),Vector3.down,out var hit,.8f,
                 ~0,QueryTriggerInteraction.Ignore),"Bridge original collider removed at "+z);
           Req(Mathf.Abs(hit.point.y-.38f)<.03f,"Bridge height changed "+z);
           bridge++;
       }
       for(int i=0;i<12;i++){
           var y=.39f+.18f*i+.09f;var z=.15f+.52f*i;
           Req(Physics.Raycast(new Vector3(0,y+2,z),Vector3.down,out var hit,4,
               ~0,QueryTriggerInteraction.Ignore),"Stair geometry missing at "+i);
           Req(Mathf.Abs(hit.point.y-y)<.30f,"Stair y compromised at "+i);
           stairs++;
       }
       Req(EditorSceneManager.SaveScene(scene,NewScene,true),"Cannot persist new layered scene");
       Directory.CreateDirectory(Evidence);
       Snap(camera,"terrain-v1-home-12",12,1536,1024);
       Snap(camera,"terrain-v1-home-19",19,1536,1024);
       Snap(camera,"terrain-v1-home-9",9,1536,1024);
       Snap(camera,"terrain-v1-portrait",12,390,844);
       var evidence=new Result{scene=NewScene,source_sha256=m.export_glb_sha256,
         status="UNITY_NATIVE_TERRAIN_GEOMETRY_PASS",art_status="REVIEW_REQUIRED",
         gameplay_status="NOT_CERTIFIED",camera_authority="LEGACY_QA_NOT_SHARP",
         original_colliders_retained=true,original_plots_retained=true,
         bridge_variant_retained=true,public_scene_modified=false,
         source_triangles=m.triangles,source_renderers=renderers.Length,
         bridge_rays=bridge,stair_rays=stairs};
       File.WriteAllText(Path.Combine(Evidence,"terrain-v1-unity.json"),JsonUtility.ToJson(evidence,true));
       Debug.Log("VALORIA_TERRAIN_RIVER_NATIVE_GATE_PASS bridgeRays="+bridge+" stairRays="+stairs);
    }
 }
}
