using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using GLTFast;
using Eldoria.Presentation;
namespace Eldoria.ValoriaLayered.Editor {
 public static class ValoriaEntryZoneV3Integration {
  const string Root="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/";
  const string Resources=Root+"GeneratedEntryV3";
  static readonly string Ev=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..","docs/evidence/valoria-layered-construction-20261016/entry-zone-v3"));
  static Camera camera;static HashSet<int> saved=new HashSet<int>();static int index;
  [Serializable] class Manifest {public string result_glb_sha256;public int triangles;}
  [Serializable] class Result {public string scene,glb_sha256,art_status,gameplay_status;public int source_triangles,visible_renderers,physical_stairs,source_target_matches,camera_state_matches;public bool canonical_modified,physical_mobile_profiled,original_sources_modified;public float home_fov;}
  static void Req(bool yes,string why){if(!yes)throw new Exception("ENTRY_ZONE: "+why);}
  static void Persist(UnityEngine.Object o){if(o==null||AssetDatabase.Contains(o)||!saved.Add(o.GetInstanceID()))return;
   if(!(o is Mesh)&&!(o is Material)&&!(o is Texture2D))return;
   AssetDatabase.CreateAsset(o,Resources+"/Entry_"+(index++).ToString("D5")+(o is Material?".mat":".asset"));}
  static void Picture(string name,int w=1536,int h=1024){
   var prev=camera.targetTexture;var active=RenderTexture.active;float aspect=camera.aspect;
   var rt=new RenderTexture(w,h,24){antiAliasing=4};var tex=new Texture2D(w,h,TextureFormat.RGB24,false);
   try{camera.aspect=(float)w/h;camera.targetTexture=rt;camera.Render();camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Ev,name+".png"),tex.EncodeToPNG());}
   finally{camera.targetTexture=prev;camera.aspect=aspect;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
  }
  static async Task<GameObject> Import(string path,Transform parent,string name){
   var gltf=new GltfImport(null,new UninterruptedDeferAgent());Req(await gltf.Load(new Uri(Path.GetFullPath(path)).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=4}),"GLB import "+path);
   var o=new GameObject(name);o.transform.SetParent(parent,false);Req(await gltf.InstantiateMainSceneAsync(o.transform),"GLB geometry "+path);return o;}
  [MenuItem("Eldoria/Valoria/Integrate source-derived entry zone V1")]
  public static async void Build(){try{await Run();Debug.Log("VALORIA_ENTRY_ZONE_V3_NATIVE_COMPLETE");EditorApplication.Exit(0);}catch(Exception e){Debug.LogError(e);EditorApplication.Exit(1);}}
  static async Task Run(){
   Directory.CreateDirectory(Ev);var meta=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(Ev,"entry-source.json")));
   string glb=Path.Combine(Ev,"ValoriaEntryZoneV3.glb");
   using(var f=File.OpenRead(glb))using(var h=SHA256.Create())Req(BitConverter.ToString(h.ComputeHash(f)).Replace("-","").ToLowerInvariant()==meta.result_glb_sha256,"Source identity changed");
   var scene=EditorSceneManager.OpenScene(Root+"Valoria_SHARP_EntryZoneV2_CANDIDATE.unity",OpenSceneMode.Single);
   var transforms=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
   var frame=transforms.Single(t=>t.name.StartsWith("EXPERIMENTAL / upright native-metre"));
   var bridge=UnityEngine.Object.FindObjectsByType<Eldoria.ValoriaLayered.ValoriaUprightViewBridge>(FindObjectsSortMode.None).Single();
   var control=UnityEngine.Object.FindObjectsByType<ValoriaParcelPresentation>(FindObjectsSortMode.None).Single();control.Home();bridge.Sync();camera=bridge.RenderCamera;
   Req(Vector3.Dot(frame.up,Vector3.up)>.9999f&&!camera.orthographic,"Need genuine upright perspective candidate");
   Picture("entry-BEFORE-home");Picture("entry-BEFORE-portrait",390,844);
   Directory.CreateDirectory(Resources);AssetDatabase.Refresh();
   var entry=await Import(glb,frame,"ENTRY V3 / continuous geographic surfaces and recovered CC0 cliffs / candidate");
   foreach(var t in transforms){
    if((t.name.StartsWith("ENTRY V1 /")||t.name.StartsWith("ENTRY V2 /"))){t.gameObject.SetActive(false);continue;}
    if(t.name.StartsWith("LAYER BRIDGE V1 -"))foreach(var r in t.GetComponentsInChildren<Renderer>(true))r.enabled=false;
    bool hide=t.name.StartsWith("SUPPORT/RESERVE ")||t.name.StartsWith("GREYBOX VPD · continuous terrain")||t.name.Contains("terrain apron")||
     t.name=="GREYBOX continuous rocky substrate"||t.name.StartsWith("SUPPORT upper civic")||t.name=="SUPPORT civic link underfill"||
     t.name.StartsWith("GREYBOX VPD · L1 support")||t.name=="PROVISIONAL river under gate approach"||t.name.StartsWith("TEMPORARY far shore ground");
    if(hide)foreach(var r in t.GetComponentsInChildren<Renderer>(true))r.enabled=false;
    if(t.name=="LAYER D - modular lower gate from six real source GLBs")foreach(var r in t.GetComponentsInChildren<Renderer>(true))r.enabled=false;
    // Actual terrain replaces obsolete terrain-only collision below water. Landmark/route
    // colliders, all twelve steps and every source interaction volume remain in place.
    if(t.name=="GREYBOX continuous rocky substrate"||t.name=="GREYBOX VPD · continuous terrain"){
     var c=t.GetComponent<Collider>();if(c!=null)c.enabled=false;}
   }
   // New visible terrain has its true static mesh collider; never offset a collision surface.
   foreach(var filter in entry.GetComponentsInChildren<MeshFilter>(true)){
    if(filter.name.Contains("ValoriaEntryTerrainV3")){var mc=filter.gameObject.AddComponent<MeshCollider>();mc.sharedMesh=filter.sharedMesh;}}
   var sourceSteps=transforms.Where(t=>t.name.StartsWith("GREYBOX certified stair step ")).ToArray();Req(sourceSteps.Length==12,"12 steps lost");
   // Lighting is the original scene sun, with a material-compatible warm daylight setting.
   var sun=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).FirstOrDefault(l=>l.type==LightType.Directional);
   if(sun!=null){sun.color=new Color(1f,.87f,.70f);sun.intensity=1.25f;sun.shadows=LightShadows.Soft;}
   RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;
   RenderSettings.ambientSkyColor=new Color(.40f,.48f,.57f);RenderSettings.ambientEquatorColor=new Color(.31f,.32f,.30f);RenderSettings.ambientGroundColor=new Color(.19f,.18f,.15f);
   Physics.SyncTransforms();int sourceMatches=0;
   var targets=new[]{"TEMPORARY bridge / continuous deck and collider","SUPPORT/RESERVE Puerta · ir al mundo","GREYBOX VPD · L0 main street","GREYBOX certified stair step 6","GREYBOX Bastion hero reservation, silhouette only","SUPPORT/RESERVE VPD · L0 west plot","SUPPORT/RESERVE VPD · L0 east plot"};
   int Test(){int hit=0;foreach(string name in targets){var c=transforms.Single(t=>t.name==name).GetComponent<Collider>();var vp=camera.WorldToViewportPoint(c.bounds.center);
    if(vp.z<0||vp.x<0||vp.x>1||vp.y<0||vp.y>1)continue;
    if(Physics.RaycastAll(camera.ViewportPointToRay(vp),500,~0,QueryTriggerInteraction.Collide).Any(r=>r.collider==c))hit++;}return hit;}
   camera.aspect=1.5f;bridge.Sync();sourceMatches=Test();Req(sourceMatches==7,"Source seven interactions failed");
   // Test the exact camera property consumed by existing SlicePresenter, not just rendered camera.
   foreach(string name in targets){
    var col=transforms.Single(t=>t.name==name).GetComponent<Collider>();var vp=camera.WorldToViewportPoint(col.bounds.center);
    var inputVp=control.ProductionCamera.WorldToViewportPoint(col.bounds.center);
    Req(Vector2.Distance(vp,inputVp)<.0001f,"Existing presenter projects a different point");
    Req(Physics.RaycastAll(control.ProductionCamera.ViewportPointToRay(vp),500,~0,QueryTriggerInteraction.Collide).Any(r=>r.collider==col),"Existing presenter ray differs from physical rendered ray");
   }
   int matches=0;void Capture(string name,int w=1536,int h=1024){camera.aspect=(float)w/h;bridge.Sync();matches+=Test();Picture(name,w,h);}
   control.Home();Capture("entry-AFTER-home");control.Pan(new Vector2(41.6667f,0));Capture("entry-AFTER-pan-left");
   control.Home();control.Pan(new Vector2(-41.6667f,0));Capture("entry-AFTER-pan-right");
   control.Home();control.Zoom(-2);Capture("entry-AFTER-zoom-in");control.Home();control.Zoom(2);Capture("entry-AFTER-zoom-out");
   control.Home();Capture("entry-AFTER-portrait",390,844);control.Home();camera.aspect=1.5f;bridge.Sync();
   foreach(var r in entry.GetComponentsInChildren<Renderer>(true))foreach(var m in r.sharedMaterials){if(m==null)continue;foreach(var p in m.GetTexturePropertyNames())Persist(m.GetTexture(p));Persist(m);}
   foreach(var f in entry.GetComponentsInChildren<MeshFilter>(true))Persist(f.sharedMesh);
   AssetDatabase.SaveAssets();PrefabUtility.SaveAsPrefabAsset(entry,Resources+"/EntryZoneV1.prefab");
   string output=Root+"Valoria_SHARP_EntryZoneV3_CANDIDATE.unity";Req(EditorSceneManager.SaveScene(scene,output,true),"Save experimental scene");
   var report=new Result{scene=output,glb_sha256=meta.result_glb_sha256,source_triangles=meta.triangles,
    visible_renderers=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Count(r=>r.enabled&&r.gameObject.activeInHierarchy),
    physical_stairs=12,source_target_matches=sourceMatches,camera_state_matches=matches,canonical_modified=false,physical_mobile_profiled=false,original_sources_modified=false,
    home_fov=camera.fieldOfView,art_status="REQUIRES_INDEPENDENT_GENUINE_UNITY_AB_REVIEW",gameplay_status="SOURCE_TARGETS_ONLY_NOT_PLAYERSTATE_PROGRESSION_OR_DEVICE_PERFORMANCE"};
   File.WriteAllText(Path.Combine(Ev,"entry-native.json"),JsonUtility.ToJson(report,true));
  }
 }
}
