// VALORIA — isolate best V13, inspect every renderer/collider, remove ONLY proven
// identical active visual duplicates. Never mutate protected V13 or V3 original.
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Eldoria.Presentation;
namespace Eldoria.ValoriaLayered.Editor {
 public static class ValoriaSceneVisualOwnershipGate {
  const string Root="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/";
  const string Baseline=Root+"Valoria_SHARP_EntryZoneV13_CANDIDATE.unity";
  const string Candidate=Root+"Valoria_SHARP_EntryZoneV13_VISUAL_CLEAN_CANDIDATE.unity";
  static readonly string Ev=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..",
    "docs/evidence/valoria-layered-construction-20261016/visual-ownership-cleanup"));
  [Serializable] class Item {
   public string path,group,mesh,material,shadow,kind;
   public bool hierarchy_active,renderer_enabled,actually_visible,has_collider,has_hotspot;
   public int vertices,triangles;public Vector3 center,size;
  }
  [Serializable] class Group {
   public string group;public int visible,hidden,changed_pixels_if_hidden;public bool current_representation;
  }
  [Serializable] class Duplicate {
   public string kept,disabled,reason;public int changed_pixels_after_disabling;
  }
  [Serializable] class Report {
   public string source_scene=Baseline,candidate_scene,verdict,method;
   public int transforms,renderers,active_visible,active_hidden,colliders,hotspots,original_stair_colliders;
   public int original_interaction_hits,clean_interaction_hits,route_probes,original_route_passes,clean_route_passes;
   public int same_image_changed_pixels,visually_identical_duplicates,unresolved_overlap_hints,source_bastion_renderers;
   public int provisional_greybox_removed_pixels,provisional_greybox_changed_outside_bastion_roi;
   public bool provisional_greybox_renderer_disabled,provisional_greybox_collider_preserved;
   public bool camera_preserved,source_scene_modified,canonical_modified,gameplay_v3_probe_rerun,physical_mobile_tested,art_approved;
   public float camera_fov;public Item[] inventory;public Group[] groups;public Duplicate[] disabled_duplicates;
   public string[] overlap_review_hints;
  }
  static void Req(bool pass,string why){if(!pass)throw new Exception("VISUAL_OWNERSHIP: "+why);}
  static string PathName(Transform t){
   var n=new List<string>();while(t!=null){n.Add(t.name);t=t.parent;}
   n.Reverse();return string.Join("/",n);
  }
  static string GroupName(Transform t){
   var p=t;while(p!=null){
    if(p.name.StartsWith("ENTRY V13 /"))return "AUTHORITATIVE_V13_COMPOSITE";
    if(p.name.StartsWith("ENTRY V17 /"))return "REJECTED_V17";
    if(p.name.StartsWith("ENTRY V12 /"))return "LEGACY_V12";
    if(p.name.StartsWith("ENTRY V7 /")||p.name.StartsWith("ENTRY V6 /")||p.name.StartsWith("ENTRY V5 /")
      ||p.name.StartsWith("ENTRY V4 /")||p.name.StartsWith("ENTRY V3 /")||p.name.StartsWith("ENTRY V2 /")
      ||p.name.StartsWith("ENTRY V1 /"))return "OLDER_ENTRY";
    if(p.name.StartsWith("GREYBOX")||p.name.Contains("greybox"))return "GREYBOX_SUPPORT";
    if(p.name.StartsWith("SUPPORT")||p.name.StartsWith("LAYER"))return "SUPPORT_OR_LAYER";
    if(p.name.StartsWith("TEMPORARY")||p.name.StartsWith("PROVISIONAL"))return "TEMPORARY";
    p=p.parent;
   }return "OTHER_SCENE_RENDERERS";
  }
  static Mesh MeshOf(Renderer r){
   var f=r.GetComponent<MeshFilter>();if(f!=null)return f.sharedMesh;
   var s=r as SkinnedMeshRenderer;return s!=null?s.sharedMesh:null;
  }
  static bool SameMatrix(Matrix4x4 a,Matrix4x4 b){
   for(int i=0;i<16;i++)if(Mathf.Abs(a[i]-b[i])>0.00005f)return false;
   return true;
  }
  // Bounds intersection alone is NOT a duplicate; require identical full mesh and
  // material assignment, same exact placement and identical shadow configuration.
  static bool ProvenExactDuplicate(Renderer a,Renderer b){
   if(!a.enabled||!b.enabled||!a.gameObject.activeInHierarchy||!b.gameObject.activeInHierarchy)return false;
   var m=MeshOf(a);var n=MeshOf(b);
   if(m==null||n==null)return false;
   if(!SameMatrix(a.localToWorldMatrix,b.localToWorldMatrix))return false;
   var ma=a.sharedMaterials;var mb=b.sharedMaterials;
   if(ma.Length!=mb.Length||a.shadowCastingMode!=b.shadowCastingMode)return false;
   for(int i=0;i<ma.Length;i++)if(ma[i]!=mb[i])return false;
   if(m==n)return true;
   if(m.vertexCount!=n.vertexCount||m.subMeshCount!=n.subMeshCount)return false;
   var va=m.vertices;var vb=n.vertices;
   for(int i=0;i<va.Length;i++)if((va[i]-vb[i]).sqrMagnitude>0.0000000001f)return false;
   for(int k=0;k<m.subMeshCount;k++){
    var ta=m.GetIndices(k);var tb=n.GetIndices(k);
    if(ta.Length!=tb.Length)return false;
    for(int i=0;i<ta.Length;i++)if(ta[i]!=tb[i])return false;
   }
   return true;
  }
  static Color32[] Picture(Camera c,string file,int w=1536,int h=1024){
   var oldT=c.targetTexture;var oldA=RenderTexture.active;float aspect=c.aspect;
   var rt=new RenderTexture(w,h,24){antiAliasing=4};var img=new Texture2D(w,h,TextureFormat.RGB24,false);
   try{
    c.aspect=(float)w/h;c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;
    img.ReadPixels(new Rect(0,0,w,h),0,0);img.Apply();
    File.WriteAllBytes(System.IO.Path.Combine(Ev,file),img.EncodeToPNG());
    if(file=="BEFORE-home.png"||file=="AFTER-home.png"||file=="PROVISIONAL_OFF_CANDIDATE.png")
      File.WriteAllBytes(System.IO.Path.Combine(Ev,System.IO.Path.GetFileNameWithoutExtension(file)+".jpg"),img.EncodeToJPG(92));
    return img.GetPixels32();
   }finally{
    c.targetTexture=oldT;c.aspect=aspect;RenderTexture.active=oldA;rt.Release();
    UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(img);
   }
  }
  static int PixelDelta(Color32[] a,Color32[] b){
   Req(a.Length==b.Length,"Different size images");
   int pixels=0;
   for(int i=0;i<a.Length;i++)if(Mathf.Abs(a[i].r-b[i].r)+Mathf.Abs(a[i].g-b[i].g)+Mathf.Abs(a[i].b-b[i].b)>=30)pixels++;
   return pixels;
  }
  static int Targets(Camera camera,ValoriaParcelPresentation present,Transform[] all){
   string[] names={"TEMPORARY bridge / continuous deck and collider","SUPPORT/RESERVE Puerta · ir al mundo",
    "GREYBOX VPD · L0 main street","GREYBOX certified stair step 6",
    "GREYBOX Bastion hero reservation, silhouette only",
    "SUPPORT/RESERVE VPD · L0 west plot","SUPPORT/RESERVE VPD · L0 east plot"};
   int matches=0;
   foreach(var name in names){
    var obj=all.Single(t=>t.name==name);var col=obj.GetComponent<Collider>();
    Req(col!=null&&col.enabled,"Missing certified collider: "+name);
    var v=camera.WorldToViewportPoint(col.bounds.center);
    var u=present.ProductionCamera.WorldToViewportPoint(col.bounds.center);
    Req(Vector2.Distance(v,u)<.0001f,"SHARP picking and rendered camera differ: "+name);
    Req(v.z>0&&v.x>0&&v.x<1&&v.y>0&&v.y<1,"Source target out of view: "+name);
    if(Physics.RaycastAll(camera.ViewportPointToRay(v),500,~0,QueryTriggerInteraction.Collide).Any(hit=>hit.collider==col))matches++;
   }
   return matches;
  }
  static int Routes(Transform[] all){
   int pass=0;var steps=all.Where(t=>t.name.StartsWith("GREYBOX certified stair step ")).OrderBy(t=>t.position.z).ToArray();
   Req(steps.Length==12,"12 original physical stairs disappeared");
   void Probe(Vector3 point,string name){
    if(Physics.Raycast(point+Vector3.up*.10f,Vector3.down,out var hit,.22f,~0,QueryTriggerInteraction.Ignore)&&hit.collider.name==name)pass++;
   }
   var start=new Vector3(0,.38f,-8.7f);
   var end=new Vector3(steps[0].position.x,.30f,steps[0].position.z-.26f);
   for(int i=1;i<12;i++)Probe(Vector3.Lerp(start,end,i/12f),"GREYBOX VPD · L0 main street");
   foreach(var step in steps){var b=step.GetComponent<Collider>().bounds;Probe(new Vector3(b.center.x,b.max.y,b.center.z),step.name);}
   for(int i=1;i<6;i++)Probe(Vector3.Lerp(new Vector3(0,.38f,-19.65f),new Vector3(0,.38f,-9.35f),i/6f),"TEMPORARY bridge / continuous deck and collider");
   return pass;
  }
  [MenuItem("Eldoria/Valoria/Check and safely clean actual V13 visible ownership")]
  public static void Build(){
   try{Run();Debug.Log("VALORIA_VISUAL_OWNERSHIP_CLEANUP_NATIVE_COMPLETE");EditorApplication.Exit(0);}
   catch(Exception ex){Debug.LogError(ex);EditorApplication.Exit(1);}
  }
  static void Run(){
   Directory.CreateDirectory(Ev);
   var scene=EditorSceneManager.OpenScene(Baseline,OpenSceneMode.Single);
   var all=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
   var objects=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).OrderBy(r=>PathName(r.transform)).ToArray();
   var colliders=UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None);
   var present=UnityEngine.Object.FindObjectsByType<ValoriaParcelPresentation>(FindObjectsSortMode.None).Single();
   var bridge=UnityEngine.Object.FindObjectsByType<ValoriaUprightViewBridge>(FindObjectsSortMode.None).Single();
   present.Home();bridge.Sync();Camera camera=bridge.RenderCamera;
   Req(camera!=null&&!camera.orthographic,"Only real perspective SHARP player camera permitted");
   Req(Mathf.Abs(camera.fieldOfView-44.4228096f)<.0001f,"Certified camera FOV changed");
   Req(all.Count(t=>t.name.StartsWith("ENTRY V13 /"))==1,"Exactly one V13 visual composition required");
   int sourceBastion=objects.Count(r=>GroupName(r.transform)=="AUTHORITATIVE_V13_COMPOSITE"&&PathName(r.transform).Contains("ENTRY_V13_EXISTING_BASTION_")&&r.enabled&&r.gameObject.activeInHierarchy);
   Req(sourceBastion==9,"Nine Bastion modules are not the sole intended V13 visible source");
   var report=new Report {
    transforms=all.Length,renderers=objects.Length,active_visible=objects.Count(r=>r.enabled&&r.gameObject.activeInHierarchy),
    active_hidden=objects.Count(r=>!(r.enabled&&r.gameObject.activeInHierarchy)),
    colliders=colliders.Length,hotspots=UnityEngine.Object.FindObjectsByType<WorldHotspot>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length,
    original_stair_colliders=all.Count(t=>t.name.StartsWith("GREYBOX certified stair step ")),
    camera_fov=camera.fieldOfView,camera_preserved=true,source_bastion_renderers=sourceBastion,
    method="Full hierarchy + exact full mesh/transform/material duplicate test + reversible per-group native SHARP render + preserved source raycasts. No bbox-only deletion.",
    source_scene_modified=false,canonical_modified=false,art_approved=false,physical_mobile_tested=false,gameplay_v3_probe_rerun=false
   };
   report.inventory=objects.Select(r=>{
    var m=MeshOf(r);var col=r.GetComponent<Collider>();
    return new Item{path=PathName(r.transform),group=GroupName(r.transform),kind=r.GetType().Name,
      mesh=m==null?"NONE":m.name,material=string.Join("|",r.sharedMaterials.Select(x=>x==null?"NONE":x.name)),
      shadow=r.shadowCastingMode.ToString(),hierarchy_active=r.gameObject.activeInHierarchy,
      renderer_enabled=r.enabled,actually_visible=r.gameObject.activeInHierarchy&&r.enabled,
      has_collider=col!=null,has_hotspot=r.GetComponent<WorldHotspot>()!=null,
      vertices=m==null?0:m.vertexCount,triangles=m==null?0:m.triangles.Length/3,
      center=r.bounds.center,size=r.bounds.size};
   }).ToArray();
   Physics.SyncTransforms();report.original_interaction_hits=Targets(camera,present,all);
   report.original_route_passes=Routes(all);report.route_probes=28;
   Req(report.original_interaction_hits==7&&report.original_route_passes==28,"Preexisting V13 circulation/interaction was not certified, cannot safely clean");
   var baseline=Picture(camera,"BEFORE-home.png");
   present.Zoom(-2);bridge.Sync();Picture(camera,"BEFORE-zoom-in.png");
   present.Home();bridge.Sync();
   // Measure which named category contributes visible pixels, never infer from hierarchy.
   var gs=new List<Group>();
   foreach(var name in objects.Select(r=>GroupName(r.transform)).Distinct().OrderBy(x=>x)){
    var active=objects.Where(r=>GroupName(r.transform)==name&&r.enabled&&r.gameObject.activeInHierarchy).ToArray();
    var group=new Group{group=name,visible=active.Length,hidden=objects.Count(r=>GroupName(r.transform)==name)-active.Length,
      current_representation=name=="AUTHORITATIVE_V13_COMPOSITE"};
    if(active.Length>0){
     foreach(var r in active)r.enabled=false;
     var off=Picture(camera,"PROBE-"+name+".png");
     group.changed_pixels_if_hidden=PixelDelta(baseline,off);
     foreach(var r in active)r.enabled=true;
    }
    gs.Add(group);
   }
   report.groups=gs.ToArray();
   // Real observed case from previous complete inventory: visible obsolete
   // placeholder block behind the actual nine Bastion meshes. Keep the entire
   // GameObject and collider; remove only this renderer from an isolated V13 copy.
   var oldProxy=all.Single(t=>t.name=="GREYBOX upper fortified entry reservation");
   var proxyRenderer=oldProxy.GetComponent<MeshRenderer>();
   var proxyCollider=oldProxy.GetComponent<Collider>();
   var proxyMesh=oldProxy.GetComponent<MeshFilter>();
   Req(proxyRenderer!=null&&proxyRenderer.enabled&&oldProxy.gameObject.activeInHierarchy,
    "Observed obsolete Bastion placeholder no longer present as expected");
   Req(proxyCollider!=null&&proxyCollider.enabled&&proxyMesh!=null
     &&proxyMesh.sharedMesh!=null&&proxyMesh.sharedMesh.triangles.Length==36,
     "Provisional block identity no longer exact; do not automatically delete");
   Req(oldProxy.GetComponent<WorldHotspot>()==null,"Original placeholder now owns hotspot; do not hide");
   proxyRenderer.enabled=false;
   var proxyOff=Picture(camera,"PROVISIONAL_OFF_CANDIDATE.png");
   report.provisional_greybox_removed_pixels=PixelDelta(baseline,proxyOff);
   int outside=0;
   for(int y=0;y<1024;y++)for(int x=0;x<1536;x++){
     int p=y*1536+x;
     int delta=Mathf.Abs(baseline[p].r-proxyOff[p].r)+Mathf.Abs(baseline[p].g-proxyOff[p].g)+Mathf.Abs(baseline[p].b-proxyOff[p].b);
     if(delta>=30 && !(x>=608&&x<=1265&&y>=744))outside++;
   }
   report.provisional_greybox_changed_outside_bastion_roi=outside;
   Req(report.provisional_greybox_removed_pixels>=30&&report.provisional_greybox_removed_pixels<=4000&&outside<=4,
      "Removing upper placeholder unexpectedly changes protected frame outside Bastion region: "+outside+" pixels");
   report.provisional_greybox_renderer_disabled=true;
   report.provisional_greybox_collider_preserved=proxyCollider.enabled;
   Req(report.provisional_greybox_collider_preserved,"Provisional block collision accidentally removed");
   // Zero visual changes are not proof of duplication by themselves; only full
   // same-space same-material same-mesh pairs may be automatically disabled.
   var activeList=objects.Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();
   var pairs=new List<Duplicate>();
   for(int i=0;i<activeList.Length;i++)for(int j=i+1;j<activeList.Length;j++){
    Renderer a=activeList[i],b=activeList[j];
    if(!a.enabled||!b.enabled||!ProvenExactDuplicate(a,b))continue;
    var ar=GroupName(a.transform);var br=GroupName(b.transform);
    bool currentA=ar=="AUTHORITATIVE_V13_COMPOSITE",currentB=br=="AUTHORITATIVE_V13_COMPOSITE";
    var keep=currentB&&!currentA?b:a;var older=keep==a?b:a;
    older.enabled=false;
    var pixels=PixelDelta(proxyOff,Picture(camera,"EXACT_DUPLICATE_TEST.png"));
    // If removing an exact overlap changes appearance (including shadows), restore.
    if(pixels>8){older.enabled=true;continue;}
    pairs.Add(new Duplicate{kept=PathName(keep.transform),disabled=PathName(older.transform),
      reason="identical FULL mesh/transform/material/shadow and native renderer OFF has <=8 pixel changes; collider/scripts retained",
      changed_pixels_after_disabling=pixels});
   }
   report.disabled_duplicates=pairs.ToArray();
   report.visually_identical_duplicates=pairs.Count;
   report.clean_interaction_hits=Targets(camera,present,all);
   report.clean_route_passes=Routes(all);
   Req(report.clean_interaction_hits==7&&report.clean_route_passes==28,"Gameplay collision regression");
   var cleaned=Picture(camera,"AFTER-home.png");
   report.same_image_changed_pixels=PixelDelta(baseline,cleaned);
   present.Zoom(-2);bridge.Sync();Picture(camera,"AFTER-zoom-in.png");present.Home();bridge.Sync();
   Picture(camera,"AFTER-portrait.png",390,844);
   var hints=new List<string>();
   var activeHiddenOld=objects.Where(r=>r.enabled&&r.gameObject.activeInHierarchy&&
     (GroupName(r.transform)=="LEGACY_V12"||GroupName(r.transform)=="OLDER_ENTRY"||GroupName(r.transform)=="REJECTED_V17")).ToArray();
   foreach(var r in activeHiddenOld)hints.Add("Visible legacy root: "+PathName(r.transform));
   foreach(var g in gs.Where(g=>g.group=="GREYBOX_SUPPORT"||g.group=="TEMPORARY"||g.group=="SUPPORT_OR_LAYER"))
    if(g.visible>0&&g.changed_pixels_if_hidden>8)hints.Add(g.group+" genuinely contributes "+g.changed_pixels_if_hidden+" pixels; not automatically removed because may be intentional visible technical support");
   report.overlap_review_hints=hints.ToArray();report.unresolved_overlap_hints=hints.Count;
   if(pairs.Count>0||report.provisional_greybox_renderer_disabled){
    Req(report.same_image_changed_pixels<=4000&&report.provisional_greybox_changed_outside_bastion_roi<=4,
     "Source artistic frame changed beyond isolated old Bastion proxy; refuse promotion");
    Req(EditorSceneManager.SaveScene(scene,Candidate,true),"Failed to save isolated clean candidate");
    report.candidate_scene=Candidate;report.verdict="OLD_UPPER_BASTION_GREYBOX_RENDERER_DISABLED_COLLIDER_RETAINED_IN_ISOLATED_COPY_NOT_ART_APPROVED";
   } else {
    report.candidate_scene="NO_SCENE_WRITTEN_BEST_V13_REMAINS_UNCHANGED";
    report.verdict=activeHiddenOld.Length>0?"LEGACY_VISIBLE_NEEDS_RENDERER_SPECIFIC_ART_REVIEW":"NO_PROVEN_EXACT_DUPLICATES_OR_EXTRA_LEGACY_RENDERERS";
   }
   Req(report.same_image_changed_pixels<=4000&&report.provisional_greybox_changed_outside_bastion_roi<=4,"No changes permitted outside former Bastion placeholder");
   File.WriteAllText(System.IO.Path.Combine(Ev,"visual-ownership-native.json"),JsonUtility.ToJson(report,true));
   Debug.Log("VALORIA_OWNERSHIP_REPORT "+report.verdict+" visible="+report.active_visible+" exact_removed="+pairs.Count+
     " unresolved_hints="+hints.Count+" interactions="+report.clean_interaction_hits+" routes="+report.clean_route_passes);
  }
 }
}
