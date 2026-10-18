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
 public static class ValoriaEntryZoneV6Integration {
  const string Root="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/";
  const string Resources=Root+"GeneratedEntryV6";
  static readonly string Ev=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..","docs/evidence/valoria-layered-construction-20261016/entry-zone-v6"));
  static Camera camera;static HashSet<int> saved=new HashSet<int>();static int index;
  [Serializable] class Manifest {public string result_glb_sha256;public int triangles;}
  [Serializable] class Result {public string scene,glb_sha256,art_status,gameplay_status;public int route_probes,route_passes;public float gate_width_percent,gate_height_percent,gate_center_error_px,walls_width_percent;public bool gate_macro_proportions_pass,pedestrian_clear;public int source_triangles,visible_renderers,physical_stairs,source_target_matches,camera_state_matches;public bool canonical_modified,physical_mobile_profiled,original_sources_modified;public float home_fov;}
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
  public static async void Build(){try{await Run();Debug.Log("VALORIA_ENTRY_ZONE_V6_NATIVE_COMPLETE");EditorApplication.Exit(0);}catch(Exception e){Debug.LogError(e);EditorApplication.Exit(1);}}
  static async Task Run(){
   Directory.CreateDirectory(Ev);var meta=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(Ev,"entry-source.json")));
   string glb=Path.Combine(Ev,"ValoriaEntryZoneV6.glb");
   using(var f=File.OpenRead(glb))using(var h=SHA256.Create())Req(BitConverter.ToString(h.ComputeHash(f)).Replace("-","").ToLowerInvariant()==meta.result_glb_sha256,"Source identity changed");
   var scene=EditorSceneManager.OpenScene(Root+"Valoria_SHARP_EntryZoneV4_CANDIDATE.unity",OpenSceneMode.Single);
   var transforms=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
   var frame=transforms.Single(t=>t.name.StartsWith("EXPERIMENTAL / upright native-metre"));
   var bridge=UnityEngine.Object.FindObjectsByType<Eldoria.ValoriaLayered.ValoriaUprightViewBridge>(FindObjectsSortMode.None).Single();
   var control=UnityEngine.Object.FindObjectsByType<ValoriaParcelPresentation>(FindObjectsSortMode.None).Single();control.Home();bridge.Sync();camera=bridge.RenderCamera;
   Req(Vector3.Dot(frame.up,Vector3.up)>.9999f&&!camera.orthographic,"Need genuine upright perspective candidate");
   Picture("entry-BEFORE-home");Picture("entry-BEFORE-portrait",390,844);
   Directory.CreateDirectory(Resources);AssetDatabase.Refresh();
   var entry=await Import(glb,frame,"ENTRY V6 / sculpted sloping banks and coherent outcrops / candidate");
   foreach(var t in transforms){
    if((t.name.StartsWith("ENTRY V1 /")||t.name.StartsWith("ENTRY V2 /")||t.name.StartsWith("ENTRY V3 /")||t.name.StartsWith("ENTRY V4 /")||t.name.StartsWith("ENTRY V5 /"))){t.gameObject.SetActive(false);continue;}
    if(t.name.StartsWith("LAYER BRIDGE V1 -"))foreach(var r in t.GetComponentsInChildren<Renderer>(true))r.enabled=false;
    bool hide=t.name.StartsWith("SUPPORT/RESERVE ")||t.name.StartsWith("GREYBOX VPD · continuous terrain")||t.name.Contains("terrain apron")||
     t.name=="GREYBOX continuous rocky substrate"||t.name.StartsWith("SUPPORT upper civic")||t.name=="SUPPORT civic link underfill"||
     t.name.StartsWith("GREYBOX VPD · L1 support")||t.name=="PROVISIONAL river under gate approach"||t.name.StartsWith("TEMPORARY far shore ground");
    if(t.name.StartsWith("GREYBOX")&&(t.name.Contains("side street")||t.name.Contains("civic link")||t.name.Contains("upper link")))hide=true;
    if(t.name=="GREYBOX VPD · L0 main street"||t.name.StartsWith("GREYBOX certified stair step ")||t.name.StartsWith("TEMPORARY far shore approach"))hide=true;
    if(hide)foreach(var r in t.GetComponentsInChildren<Renderer>(true))r.enabled=false;
    if(t.name=="LAYER D - modular lower gate from six real source GLBs")foreach(var r in t.GetComponentsInChildren<Renderer>(true))r.enabled=false;
    // Actual terrain replaces obsolete terrain-only collision below water. Landmark/route
    // colliders, all twelve steps and every source interaction volume remain in place.
    if(t.name=="GREYBOX continuous rocky substrate"||t.name=="GREYBOX VPD · continuous terrain"){
     var c=t.GetComponent<Collider>();if(c!=null)c.enabled=false;}
   }
   // New visible terrain has its true static mesh collider; never offset a collision surface.
   foreach(var filter in entry.GetComponentsInChildren<MeshFilter>(true)){
    if(filter.name.Contains("ValoriaEntryTerrainV6")){var mc=filter.gameObject.AddComponent<MeshCollider>();mc.sharedMesh=filter.sharedMesh;}}
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
   // Re-run physical circulation with the ACTUAL newly authored terrain collider.
   int routeTotal=0,routePass=0;
   void Probe(Vector3 point,string name){routeTotal++;if(Physics.Raycast(point+Vector3.up*.10f,Vector3.down,out var h,.22f,~0,QueryTriggerInteraction.Ignore)&&h.collider.name==name)routePass++;}
   var ordered=sourceSteps.OrderBy(t=>t.position.z).ToArray();
   var start=new Vector3(0,.38f,-8.7f);var end=new Vector3(ordered[0].position.x,.30f,ordered[0].position.z-.26f);
   for(int i=1;i<12;i++)Probe(Vector3.Lerp(start,end,i/12f),"GREYBOX VPD · L0 main street");
   foreach(var t in ordered){var b=t.GetComponent<Collider>().bounds;Probe(new Vector3(b.center.x,b.max.y,b.center.z),t.name);}
   for(int i=1;i<6;i++)Probe(Vector3.Lerp(new Vector3(0,.38f,-19.65f),new Vector3(0,.38f,-9.35f),i/6f),"TEMPORARY bridge / continuous deck and collider");
   bool clear=!Physics.SphereCast(start+Vector3.up*.8f,.3f,(end-start).normalized,out var blocked,(end-start).magnitude,~0,QueryTriggerInteraction.Ignore);
   Req(routePass==routeTotal,"New terrain masks a source route: "+routePass+"/"+routeTotal);Req(clear,"Pedestrian passage blocked by "+(blocked.collider!=null?blocked.collider.name:"unknown"));
   Vector4 Envelope(bool includeWalls){
    var fs=entry.GetComponentsInChildren<MeshFilter>().Where(f=>includeWalls?f.name.Contains("ENTRY_LowerGate_"):f.name=="ENTRY_LowerGate_Arch"||f.name=="ENTRY_LowerGate_EastTower"||f.name=="ENTRY_LowerGate_WestTower").ToArray();Req(fs.Length>0,"Gate model geometry missing");
    var points=fs.SelectMany(f=>f.sharedMesh.vertices.Select(v=>camera.WorldToViewportPoint(f.transform.TransformPoint(v)))).Where(v=>v.z>0).ToArray();
    return new Vector4(points.Min(v=>v.x),points.Min(v=>v.y),points.Max(v=>v.x),points.Max(v=>v.y));
   }
   var gate=Envelope(false);var walls=Envelope(true);float gw=gate.z-gate.x,gh=gate.w-gate.y;
   float ce=Vector2.Distance(new Vector2((gate.x+gate.z)*.5f*1536,(gate.y+gate.w)*.5f*1024),new Vector2(701.5f,392.5f));
   bool proportion=gw>=.75f*(273f/1536)&&gw<=1.25f*(273f/1536)&&gh>=.75f*(211f/1024)&&gh<=1.25f*(211f/1024)&&ce<46.2f;
   foreach(var r in entry.GetComponentsInChildren<Renderer>(true))foreach(var m in r.sharedMaterials){if(m==null)continue;foreach(var p in m.GetTexturePropertyNames())Persist(m.GetTexture(p));Persist(m);}
   foreach(var f in entry.GetComponentsInChildren<MeshFilter>(true))Persist(f.sharedMesh);
   AssetDatabase.SaveAssets();PrefabUtility.SaveAsPrefabAsset(entry,Resources+"/EntryZoneV1.prefab");
   string output=Root+"Valoria_SHARP_EntryZoneV6_CANDIDATE.unity";Req(EditorSceneManager.SaveScene(scene,output,true),"Save experimental scene");
   var report=new Result{scene=output,glb_sha256=meta.result_glb_sha256,source_triangles=meta.triangles,
    route_probes=routeTotal,route_passes=routePass,pedestrian_clear=clear,gate_width_percent=gw*100,gate_height_percent=gh*100,walls_width_percent=(walls.z-walls.x)*100,gate_center_error_px=ce,gate_macro_proportions_pass=proportion,
    visible_renderers=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Count(r=>r.enabled&&r.gameObject.activeInHierarchy),
    physical_stairs=12,source_target_matches=sourceMatches,camera_state_matches=matches,canonical_modified=false,physical_mobile_profiled=false,original_sources_modified=false,
    home_fov=camera.fieldOfView,art_status="REQUIRES_INDEPENDENT_GENUINE_UNITY_AB_REVIEW",gameplay_status="SOURCE_TARGETS_ONLY_NOT_PLAYERSTATE_PROGRESSION_OR_DEVICE_PERFORMANCE"};
   File.WriteAllText(Path.Combine(Ev,"entry-native.json"),JsonUtility.ToJson(report,true));
  }
 }
}
