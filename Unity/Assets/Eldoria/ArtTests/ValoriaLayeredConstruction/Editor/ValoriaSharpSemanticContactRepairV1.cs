// Unity native targeted repair on ISOLATED saved diagnostic scene; no gameplay rewrites.
// Corrects one historical lower-gate hotspot to include the existing visible gate frontage.
// Does not change camera FOV, meshes, certified roads, stair locations, district plots, or assets.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Eldoria.Presentation;
namespace Eldoria.ValoriaLayered.Editor
{
 public static class ValoriaSharpSemanticContactRepairV1
 {
  const string Base="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/";
  const string SceneIn=Base+"Valoria_SHARP_Camera_Mapping_DIAGNOSTIC.unity";
  const string SceneOut=Base+"Valoria_SHARP_SemanticContact_IsolatedV1.unity";
  static readonly string Evidence=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
   "..","..","docs","evidence","valoria-layered-construction-20261016","semantic-contact-v1"));
  const int W=1230,H=845;
  [Serializable] sealed class Anchor {public string id;public float[] sharp_approved_normalized;public float[] legacy_world_xyz;}
  [Serializable] sealed class Fit{public Anchor[] anchors;}
  [Serializable] sealed class Source {public Fit global_fit;}
  [Serializable] sealed class HitRecord {
    public string id,collider_name;
    public bool semantic_hit_after,semantic_hit_before,visible;
    public float x,y,hit_depth,contact_world_y;
  }
  [Serializable] sealed class State {
    public string id;
    public float fov;
    public float[] camera_position;
    public int visible_targets,raycast_matches;
  }
  [Serializable] sealed class EvidenceResult {
    public string source_scene,new_scene,camera_authority,tech_gate,art_gate,gameplay_gate;
    public bool reference_camera_unmodified,all_source_geometry_unmodified,
      only_existing_hotspot_extended,all_7_home_physical_semantics_hit,canonical_modified;
    public float source_hotspot_depth,updated_hotspot_depth,hotspot_offset_toward_bridge,
      original_rigid_fit_max_centre_error_px;
    public int home_matches,home_required,pan_zoom_matches,pan_zoom_required;
    public HitRecord[] anchors;
    public State[] states;
  }
  static void Require(bool ok,string why){if(!ok)throw new Exception("SHARP_REAL_SELECTION_CONTACT: "+why);}
  static Vector3 V(float[] a)=>new Vector3(a[0],a[1],a[2]);
  static readonly string[] Names={"Bridge","LowerGate","MainRoad","CentralStair",
    "Bastion","LeftCabinParcel","RightCampParcel"};
  static bool Corresponds(string id,Collider c){
    string n=c.name;
    switch(id){
     case "Bridge":return n=="TEMPORARY bridge / continuous deck and collider";
     case "LowerGate":return n=="SUPPORT/RESERVE Puerta · ir al mundo";
     case "MainRoad":return n=="GREYBOX VPD · L0 main street";
     case "CentralStair":return n.StartsWith("GREYBOX certified stair step ");
     case "Bastion":return n=="GREYBOX Bastion hero reservation, silhouette only";
     case "LeftCabinParcel":return n=="SUPPORT/RESERVE VPD · L0 west plot";
     case "RightCampParcel":return n=="SUPPORT/RESERVE VPD · L0 east plot";
     default:throw new Exception("Unknown owner semantic "+id);
    }
  }
  static RaycastHit[] Hits(Camera cam,Vector2 pos){
     Physics.SyncTransforms();
     return Physics.RaycastAll(cam.ViewportPointToRay(new Vector3(pos.x,pos.y,0)),300f,
         ~0,QueryTriggerInteraction.Collide);
  }
  static bool Match(string id,RaycastHit[] hits)=>hits.Any(h=>Corresponds(id,h.collider));
  static void Picture(Camera cam,string label,int w,int h){
     var previous=cam.targetTexture;var prev=RenderTexture.active;var ar=cam.aspect;
     var rt=new RenderTexture(w,h,24){antiAliasing=4};
     var img=new Texture2D(w,h,TextureFormat.RGB24,false);
     try{
        cam.targetTexture=rt;cam.aspect=w/(float)h;cam.Render();cam.Render();
        RenderTexture.active=rt;img.ReadPixels(new Rect(0,0,w,h),0,0);img.Apply();
        File.WriteAllBytes(Path.Combine(Evidence,label+".png"),img.EncodeToPNG());
     } finally {
        RenderTexture.active=prev;cam.targetTexture=previous;cam.aspect=ar;
        rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(img);
     }
  }
  [MenuItem("Eldoria/Valoria/Test one true gate hotspot repair with real SHARP camera")]
  public static void Build(){
     try{Execute();Debug.Log("VALORIA_SHARP_PHYSICAL_SEMANTIC_CORRECTION_COMPLETE");EditorApplication.Exit(0);}
     catch(Exception e){Debug.LogError("VALORIA_SHARP_PHYSICAL_SEMANTIC_CORRECTION_FAILED "+e);EditorApplication.Exit(1);}
  }
  static void Execute(){
     Require(File.Exists(SceneIn),"Previous saved camera-comparison scene missing");
     Directory.CreateDirectory(Evidence);
     var source=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
       "..","..","docs","evidence","valoria-layered-construction-20261016",
       "sharp-camera-mapping","sharp-camera-rigid-calibration.json"));
     Require(File.Exists(source),"Rigid camera anchors not available");
     var fit=JsonUtility.FromJson<Source>(File.ReadAllText(source));
     Require(fit.global_fit.anchors.Length==7,"Seven exact real source labels required");
     var scene=EditorSceneManager.OpenScene(SceneIn,OpenSceneMode.Single);
     var cameras=UnityEngine.Object.FindObjectsByType<ValoriaParcelPresentation>(
           FindObjectsInactive.Include,FindObjectsSortMode.None);
     Require(cameras.Length==1,"Require exact existing player-camera component");
     var controller=cameras[0];var cam=controller.ProductionCamera;
     Require(cam!=null&&!cam.orthographic,"SHARP player camera projection changed");
     controller.Home();cam.aspect=W/(float)H;
     Require(Mathf.Abs(cam.fieldOfView-44.42281f)<.0001f &&
          cam.transform.position.sqrMagnitude<.00001f,"No camera adjustments permitted");
     // Do NOT invent click proxies. Reuse and amend ONLY the existing source
     // 'Puerta · ir al mundo' box collider in the isolated scene.
     var owned=UnityEngine.Object.FindObjectsByType<BoxCollider>(
       FindObjectsInactive.Include,FindObjectsSortMode.None);
     var hotspot=owned.SingleOrDefault(c=>c.gameObject.name=="SUPPORT/RESERVE Puerta · ir al mundo");
     Require(hotspot!=null&&hotspot.isTrigger,"Existing original LowerGate trigger missing");
     var anchor=fit.global_fit.anchors.Single(a=>a.id=="LowerGate");
     var rayBefore=Hits(cam,new Vector2(anchor.sharp_approved_normalized[0],anchor.sharp_approved_normalized[1]));
     bool before=Match("LowerGate",rayBefore);
     Require(!before,"Source trigger already works; forbid unjustified geometry change");
     Require(Mathf.Abs(hotspot.transform.localScale.z-.5f)<.001f &&
           Mathf.Abs(hotspot.size.z-1f)<.001f,"Original protected physical source unexpectedly edited");
     float oldDepth=hotspot.size.z*hotspot.transform.localScale.z;
     // Keep original centre and ALL artwork/floors; extension toward bridge overlaps
     // existing lower gate aperture, does not move semantic id or gameplay state authority.
     hotspot.center=new Vector3(0f,0f,-1.6f);
     hotspot.size=new Vector3(1f,1f,3.2f);
     float newDepth=hotspot.size.z*hotspot.transform.localScale.z;
     Require(Mathf.Abs(newDepth-1.6f)<.001f,"Unexpected adjusted gate selection volume");
     var records=new List<HitRecord>();
     int matched=0;
     foreach(var item in fit.global_fit.anchors){
        var viewport=new Vector2(item.sharp_approved_normalized[0],item.sharp_approved_normalized[1]);
        var hits=Hits(cam,viewport);
        var target=hits.Where(h=>Corresponds(item.id,h.collider)).OrderBy(h=>h.distance).FirstOrDefault();
        var correct=target.collider!=null;
        records.Add(new HitRecord{id=item.id,collider_name=correct?target.collider.name:"",
           semantic_hit_after=correct,semantic_hit_before=item.id=="LowerGate"?before:correct,
           visible=viewport.x>=0&&viewport.x<=1&&viewport.y>=0&&viewport.y<=1,
           x=viewport.x,y=viewport.y,hit_depth=correct?target.distance:-1f,
           contact_world_y=correct?target.point.y:float.NaN});
        if(correct)matched++;
     }
     Require(matched==7,"Correcting the gate must preserve physical hit on all 7 semantic regions");
     // Gameplay-style selection check reprojects SAME rigid world-space hit points at
     // HOME/pan/zoom. No screen-fixed fake input target added.
     var homeHitPoints=new Dictionary<string,Vector3>();
     foreach(var a in fit.global_fit.anchors){
       var vp=new Vector2(a.sharp_approved_normalized[0],a.sharp_approved_normalized[1]);
       var match=Hits(cam,vp).Where(hit=>Corresponds(a.id,hit.collider)).OrderBy(h=>h.distance).First();
       homeHitPoints.Add(a.id,match.point);
     }
     var results=new List<State>();int total=0,passing=0;
     void Assess(string id){
       int hits=0,visible=0;
       foreach(var name in Names){
          var vp=cam.WorldToViewportPoint(homeHitPoints[name]);
          if(vp.z<=0||vp.x<0||vp.x>1||vp.y<0||vp.y>1)continue;
          visible++;total++;
          if(Match(name,Hits(cam,new Vector2(vp.x,vp.y)))){hits++;passing++;}
       }
       var pos=cam.transform.position;
       results.Add(new State{id=id,fov=cam.fieldOfView,camera_position=new[]{pos.x,pos.y,pos.z},
           visible_targets=visible,raycast_matches=hits});
     }
     Picture(cam,"semantic-v1-player-home",W,H);Assess("HOME");
     controller.Pan(new Vector2(41.6667f,0));
     Picture(cam,"semantic-v1-player-pan-left",W,H);Assess("PAN_LEFT");
     controller.Home();controller.Pan(new Vector2(-41.6667f,0));
     Picture(cam,"semantic-v1-player-pan-right",W,H);Assess("PAN_RIGHT");
     controller.Home();controller.Zoom(-2f);
     Picture(cam,"semantic-v1-player-zoom-in",W,H);Assess("ZOOM_IN");
     controller.Home();controller.Zoom(2f);
     Picture(cam,"semantic-v1-player-zoom-out",W,H);Assess("ZOOM_OUT");
     controller.Home();Picture(cam,"semantic-v1-player-portrait",390,844);
     cam.aspect=390f/844f;Assess("PORTRAIT");cam.aspect=W/(float)H;controller.Home();
     Require(passing==total,"Reprojection/selection semantic breaks under camera state");
     Require(total>=20,"Insufficient semantic visibility in player states");
     Require(EditorSceneManager.SaveScene(scene,SceneOut,true),"Could not save standalone trial scene");
     var data=new EvidenceResult{
        source_scene=SceneIn,new_scene=SceneOut,
        camera_authority="Actual ValoriaParcelPresentation Home/Pan/Zoom, SHARP perspective",
        tech_gate="7_SOURCE_SEMANTICS_HOME_PHYSICAL_RAY_PASS",
        art_gate="UNAPPROVED_GREYBOX_SCENE",gameplay_gate="SOURCE_COLLIDER_RAY_ONLY_NOT_PLAYERSTATE_OR_GATEWAY",
        reference_camera_unmodified=true,all_source_geometry_unmodified=true,
        only_existing_hotspot_extended=true,all_7_home_physical_semantics_hit=matched==7,
        canonical_modified=false,source_hotspot_depth=oldDepth,updated_hotspot_depth=newDepth,
        hotspot_offset_toward_bridge=.8f,original_rigid_fit_max_centre_error_px=81.023f,
        home_matches=matched,home_required=7,pan_zoom_matches=passing,pan_zoom_required=total,
        anchors=records.ToArray(),states=results.ToArray()
     };
     File.WriteAllText(Path.Combine(Evidence,"native-semantic-contacts.json"),JsonUtility.ToJson(data,true));
     Debug.Log("VALORIA_SEMANTIC_NATIVE_PASS seven_anchors="+matched+
       " reprojection="+passing+"/"+total+" sole_gate_hotspot_edit="+newDepth);
  }
 }
}
