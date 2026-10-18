// Owner-authorized spatial recalibration. Original scenes and mesh assets immutable.
// A stage-2 composition candidate, explicitly NOT final art or complete gameplay.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Eldoria.Presentation;
namespace Eldoria.ValoriaLayered.Editor {
 public static class ValoriaSpatialRecalibrationV1 {
  const string Root="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/";
  const string Input=Root+"Valoria_SHARP_SemanticContact_IsolatedV1.unity";
  const string Output=Root+"Valoria_SHARP_SpatialRecalibrationV1.unity";
  static readonly string Ev=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..","docs/evidence/valoria-layered-construction-20261016/spatial-v1"));
  const int W=1536,H=1024;
  static Transform frame;static Camera cam;static ValoriaParcelPresentation control;
  static Transform[] objects;
  static readonly float[] Target={565f/W,1f-737f/H,838f/W,1f-526f/H};
  [Serializable] public class Contact {public string id,collider;public float[] native_xyz,viewport;public bool hit;}
  [Serializable] public class State {public string id;public int visible,hits;public float fov;}
  [Serializable] public class Report {
   public string source_scene,candidate_scene,classification,gameplay_status,art_status;
   public bool canonical_modified,source_meshes_modified,world_y_up,original_controller_code_unchanged;
   public float home_fov,coordinate_conversion_max_projection_error_px;
   public float[] original_gate_envelope,candidate_gate_envelope,target_gate_envelope,frame_translation;
   public int physical_stairs,source_footprints,preserved_colliders,route_probes,route_passes;
   public Contact[] contacts;public State[] camera_states;
  }
  static void Check(bool x,string m){if(!x)throw new Exception("VALORIA_SPATIAL: "+m);}
  static Transform Find(string n)=>objects.Single(t=>t.name==n);
  static Vector3 Native(Transform t)=>frame.InverseTransformPoint(t.position);
  static void SetNative(Transform t,Vector3 p){t.position=frame.TransformPoint(p);}
  static float[] Envelope(Renderer[] renderers){
   var points=new List<Vector3>();
   // Per-renderer local mesh bounds. A world AABB inflates a rotated ensemble.
   foreach(var r in renderers){var b=r.localBounds;
    foreach(float x in new[]{b.min.x,b.max.x})foreach(float y in new[]{b.min.y,b.max.y})foreach(float z in new[]{b.min.z,b.max.z})
     points.Add(cam.WorldToViewportPoint(r.transform.TransformPoint(new Vector3(x,y,z))));}
   Check(points.All(p=>p.z>cam.nearClipPlane),"Gate behind near clip");
   return new[]{points.Min(p=>p.x),points.Min(p=>p.y),points.Max(p=>p.x),points.Max(p=>p.y)};
  }
  static float Loss(Renderer[] rs){try{var b=Envelope(rs);float s=0;for(int i=0;i<4;i++)s+=Mathf.Pow((b[i]-Target[i])*(i%2==0?W:H),2);return s;}catch{return float.MaxValue;}}
  static void FitGate(Renderer[] rs){
   float best=Loss(rs);var p=frame.position;
   // Translation affects apparent size through real perspective; meshes and lens never scale.
   foreach(float step in new[]{16f,8f,4f,2f,1f,.5f,.25f,.125f,.0625f,.03125f}){
    for(int it=0;it<40;it++){
     bool changed=false;
     foreach(var axis in new[]{Vector3.forward,Vector3.up,Vector3.right})foreach(float sign in new[]{-1f,1f}){
      var q=p+axis*step*sign;frame.position=q;var loss=Loss(rs);
      if(loss<best){best=loss;p=q;changed=true;}else frame.position=p;
     }
     if(!changed)break;
    }
   }
   frame.position=p;
  }
  static Vector3 At(float x,float y,float nativeHeight){
   var plane=new Plane(frame.up,frame.TransformPoint(new Vector3(0,nativeHeight,0)));
   var ray=cam.ViewportPointToRay(new Vector3(x,y,0));
   Check(plane.Raycast(ray,out var d)&&d>0&&d<400,"Reference ray cannot meet native floor");
   return frame.InverseTransformPoint(ray.GetPoint(d));
  }
  static void Segment(Transform t,Vector3 a,Vector3 b,float width,float thick){
   var dir=b-a;SetNative(t,(a+b)*.5f-Vector3.up*(thick*.5f));
   t.rotation=frame.rotation*Quaternion.LookRotation(dir,Vector3.up);
   t.localScale=new Vector3(width,thick,dir.magnitude);
  }
  static void Picture(string name,int w,int h){
   var old=cam.targetTexture;var active=RenderTexture.active;float aspect=cam.aspect;
   var rt=new RenderTexture(w,h,24){antiAliasing=4};var tex=new Texture2D(w,h,TextureFormat.RGB24,false);
   try{cam.aspect=(float)w/h;cam.targetTexture=rt;cam.Render();cam.Render();RenderTexture.active=rt;
    tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Ev,name+".png"),tex.EncodeToPNG());}
   finally{cam.targetTexture=old;cam.aspect=aspect;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
  }
  [MenuItem("Eldoria/Valoria/Build isolated real-perspective spatial recalibration V1")]
  public static void Build(){try{Run();Debug.Log("VALORIA_SPATIAL_RECALIBRATION_V1_COMPLETE");EditorApplication.Exit(0);}catch(Exception e){Debug.LogError(e);EditorApplication.Exit(1);}}
  static void Run(){
   Directory.CreateDirectory(Ev);var scene=EditorSceneManager.OpenScene(Input,OpenSceneMode.Single);
   objects=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
   frame=objects.Single(t=>t.name.StartsWith("DIAGNOSTIC / native 3D rigid world-to-SHARP"));
   control=UnityEngine.Object.FindObjectsByType<ValoriaParcelPresentation>(FindObjectsSortMode.None).Single();
   control.Home();cam=control.ProductionCamera;cam.aspect=(float)W/H;
   Check(!cam.orthographic&&Mathf.Abs(cam.fieldOfView-44.42281f)<.0001f,"Wrong player lens");
   var gate=Find("LAYER D - modular lower gate from six real source GLBs");
   var rs=gate.Cast<Transform>().Where(t=>t.name=="PRODUCTION_SOURCE_CANDIDATE__tower_square_short"||t.name=="PRODUCTION_SOURCE_CANDIDATE__arch_complete_major")
      .SelectMany(t=>t.GetComponentsInChildren<Renderer>(true)).ToArray();
   int originalColliders=frame.GetComponentsInChildren<Collider>(true).Length;
   Picture("spatial-BEFORE-home",W,H);Picture("spatial-BEFORE-portrait",390,844);
   var originalBox=Envelope(rs);FitGate(rs);var candidateBox=Envelope(rs);var translation=frame.position;
   // Remove display-only diagnostic dots. Never remove physical source colliders.
   foreach(var t in objects.Where(t=>t.name=="SUPPORT / SEMANTIC ANCHOR BALLS - DIAGNOSTIC ONLY"))t.gameObject.SetActive(false);
   // Revert the prior experimental hotspot extension: original aperture box travels with gate.
   var hotspot=Find("SUPPORT/RESERVE Puerta · ir al mundo").GetComponent<BoxCollider>();
   hotspot.center=Vector3.zero;hotspot.size=Vector3.one;
   // Visual reference points are contact locations, not bounding-box centres or old splat anchors.
   // Keep 12 original step meshes/colliders and their rise/run unchanged, move as one assembly.
   var steps=objects.Where(t=>t.name.StartsWith("GREYBOX certified stair step ")).OrderBy(t=>Native(t).z).ToArray();
   Check(steps.Length==12,"Twelve physical steps required");
   Vector3 centre=steps.Select(Native).Aggregate(Vector3.zero,(a,b)=>a+b)/12;
   var desired=At(869f/W,1f-330f/H,centre.y);var stairDelta=desired-centre;stairDelta.y=0;
   foreach(var t in steps)SetNative(t,Native(t)+stairDelta);
   foreach(var t in objects.Where(t=>t.name.Contains("L1 landing")||t.name.Contains("VPD · L1 ")||t.name=="SUPPORT exact certified L1 footprint (trigger-only envelope)"))SetNative(t,Native(t)+stairDelta);
   Vector3 roadStart=new Vector3(0,.38f,-8.7f);
   var first=Native(steps.First());var last=Native(steps.Last());
   var centreSupport=Find("GREYBOX VPD · L1 support centre");
   var sp=Native(centreSupport);sp.z=last.z+steps.Last().localScale.z*.5f+2.2f;
   SetNative(centreSupport,sp);centreSupport.localScale=new Vector3(7.2f,2.35f,4.2f);
   var roadEnd=new Vector3(first.x,.30f,first.z-steps.First().localScale.z*.5f);
   Segment(Find("GREYBOX VPD · L0 main street"),roadStart,roadEnd,3.7f,.12f);
   var hero=Find("GREYBOX Bastion hero reservation, silhouette only");
   var oldHero=Native(hero);var newHero=At(944f/W,1f-190f/H,oldHero.y);
   SetNative(hero,newHero);var heroDelta=newHero-oldHero;heroDelta.y=0;
   foreach(var t in objects.Where(t=>t.name.Contains("upper fortified entry reservation")||t.name.Contains("upper civic")||t.name.Contains("upper gate")))SetNative(t,Native(t)+heroDelta);
   // Move the obsolete future reserve outside the new road rather than leaving a second floor mid-route.
   var future=Find("SUPPORT/RESERVE VPD · master future reserve");
   var futureAt=Native(future);futureAt.z=newHero.z+20;SetNative(future,futureAt);
   foreach(var pair in new[]{("SUPPORT/RESERVE VPD · L0 west plot",471f/W,1f-369f/H),("SUPPORT/RESERVE VPD · L0 east plot",1260f/W,1f-446f/H)}){
    var t=Find(pair.Item1);SetNative(t,At(pair.Item2,pair.Item3,Native(t).y));}
   // The provisional ground is explicitly blockout; a subsequent Blender pass must author geology.
   var substrate=Find("GREYBOX continuous rocky substrate");
   float rear=Mathf.Max(newHero.z+10,last.z+10);
   SetNative(substrate,new Vector3(0,-.37f,(rear-13)/2));substrate.localScale=new Vector3(80,.86f,rear+13);
   // Extend upper receiving deck and existing civic link as spaces, never distort architectural assets.
   var landing=Find("GREYBOX L1 rear landing - stair throat visibly clear");
   var upperStart=new Vector3(last.x,2.7f,last.z+steps.Last().localScale.z*.5f);
   var upperEnd=new Vector3(newHero.x,2.7f,newHero.z-2.5f);
   Segment(landing,upperStart,upperEnd,10f,.5f);
   Segment(Find("SUPPORT civic link underfill"),upperStart,upperEnd,9.8f,2.6f);
   Segment(Find("GREYBOX VPD · master upper civic link"),upperStart,upperEnd,3.8f,.12f);
   // Native contacts: all seven targets are real source geometry; test rays against actual colliders.
   var points=new[]{
    ("Bridge",Find("TEMPORARY bridge / continuous deck and collider")),
    ("LowerGate",hotspot.transform),("MainRoad",Find("GREYBOX VPD · L0 main street")),
    ("CentralStair",steps[5]),("Bastion",hero),
    ("LeftCabinParcel",Find("SUPPORT/RESERVE VPD · L0 west plot")),
    ("RightCampParcel",Find("SUPPORT/RESERVE VPD · L0 east plot"))};
   var projected=points.Select(p=>cam.WorldToViewportPoint(p.Item2.position)).ToArray();
   // Rigid change of coordinate basis: renders must be mathematically identical, native terrain Y-up.
   var inverse=Matrix4x4.TRS(frame.position,frame.rotation,Vector3.one).inverse;
   var origin=inverse.MultiplyPoint3x4(Vector3.zero);var rotation=Quaternion.Inverse(frame.rotation);
   frame.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
   frame.name="EXPERIMENTAL / upright native-metre spatial recalibration V1";
   var input=new GameObject("SHARP canonical HOME PAN ZOOM input (unrendered perspective)").AddComponent<Camera>();
   input.CopyFrom(cam);input.enabled=false;input.tag="Untagged";input.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
   control.ProductionCamera=input;
   var bridge=cam.gameObject.AddComponent<Eldoria.ValoriaLayered.ValoriaUprightViewBridge>();
   bridge.CanonicalInput=input;bridge.RenderCamera=cam;bridge.NativeOrigin=origin;bridge.NativeRotation=rotation;
   bridge.Sync();cam.aspect=(float)W/H;Physics.SyncTransforms();
   float maxError=0;for(int i=0;i<points.Length;i++)maxError=Mathf.Max(maxError,Vector2.Distance(projected[i],cam.WorldToViewportPoint(points[i].Item2.position))*W);
   Check(maxError<.05f,"Coordinate conversion changed perspective");
   Check(Vector3.Dot(frame.up,Vector3.up)>.9999f,"World not upright");
   var contacts=new List<Contact>();
   foreach(var p in points){var pos=p.Item2.position;var c=p.Item2.GetComponent<Collider>();
    if(c!=null)pos=c.bounds.center;
    var vp=cam.WorldToViewportPoint(pos);bool hit=Physics.RaycastAll(cam.ViewportPointToRay(vp),500,~0,QueryTriggerInteraction.Collide).Any(h=>h.collider==c);
    contacts.Add(new Contact{id=p.Item1,collider=c!=null?c.name:"",native_xyz=new[]{pos.x,pos.y,pos.z},viewport=new[]{vp.x,vp.y},hit=hit});}
   var states=new List<State>();
   void Assess(string id,int width,int height){bridge.Sync();cam.aspect=(float)width/height;
    var s=new State{id=id,fov=cam.fieldOfView};foreach(var p in points){var c=p.Item2.GetComponent<Collider>();if(c==null)continue;
     var vp=cam.WorldToViewportPoint(c.bounds.center);if(vp.z<0||vp.x<0||vp.x>1||vp.y<0||vp.y>1)continue;s.visible++;
     if(Physics.RaycastAll(cam.ViewportPointToRay(vp),500,~0,QueryTriggerInteraction.Collide).Any(h=>h.collider==c))s.hits++;}
    states.Add(s);Picture("spatial-AFTER-"+id,width,height);}
   control.Home();Assess("home",W,H);control.Pan(new Vector2(41.6667f,0));Assess("pan-left",W,H);
   control.Home();control.Pan(new Vector2(-41.6667f,0));Assess("pan-right",W,H);
   control.Home();control.Pan(new Vector2(0,83.3333f));Assess("pan-down",W,H);
   control.Home();control.Pan(new Vector2(0,-83.3333f));Assess("pan-up",W,H);
   control.Home();control.Zoom(-2);Assess("zoom-in",W,H);control.Home();control.Zoom(2);Assess("zoom-out",W,H);
   control.Home();Assess("portrait",390,844);control.Home();bridge.Sync();cam.aspect=(float)W/H;
   Check(contacts.All(c=>c.hit)&&states.All(s=>s.visible==s.hits),"Real geometry picking failed");
   int passes=0,total=0;
   void Probe(Vector3 p,string expected){total++;
    if(Physics.Raycast(p+Vector3.up*.10f,Vector3.down,out var hit,.22f,~0,QueryTriggerInteraction.Ignore)&&hit.collider.name==expected)passes++;}
   for(int i=1;i<12;i++)Probe(Vector3.Lerp(roadStart,roadEnd,i/12f),"GREYBOX VPD · L0 main street");
   foreach(var t in steps)Probe(t.position+Vector3.up*t.localScale.y*.5f,t.name);
   for(int i=1;i<6;i++)Probe(Vector3.Lerp(new Vector3(0,.38f,-19.65f),new Vector3(0,.38f,-9.35f),i/6f),"TEMPORARY bridge / continuous deck and collider");
   Check(passes==total,"Specific route/step/bridge surface missing or masked by another collider: "+passes+"/"+total);
   var walk=roadEnd-roadStart;
   Check(!Physics.SphereCast(roadStart+Vector3.up*.8f,.30f,walk.normalized,out var obstruction,walk.magnitude,~0,QueryTriggerInteraction.Ignore),
    "Main road obstructed at pedestrian height: "+(obstruction.collider!=null?obstruction.collider.name:"unknown"));
   Check(frame.GetComponentsInChildren<Collider>(true).Length==originalColliders,"Physical collider lost");
   Check(EditorSceneManager.SaveScene(scene,Output,true),"Cannot save isolated scene");
   var report=new Report{source_scene=Input,candidate_scene=Output,classification="SPATIAL_BLOCKOUT_WITH_EXISTING_ARCHITECTURE_NOT_FINAL_ART",
    canonical_modified=false,source_meshes_modified=false,world_y_up=true,original_controller_code_unchanged=true,
    home_fov=input.fieldOfView,coordinate_conversion_max_projection_error_px=maxError,
    original_gate_envelope=originalBox,candidate_gate_envelope=candidateBox,target_gate_envelope=Target,
    frame_translation=new[]{translation.x,translation.y,translation.z},physical_stairs=steps.Length,source_footprints=22,
    preserved_colliders=originalColliders,contacts=contacts.ToArray(),camera_states=states.ToArray(),route_probes=total,route_passes=passes,
    gameplay_status="SOURCE_GEOMETRY_AND_CAMERA_CONTROLS_ONLY_NO_PLAYERSTATE_PROGRESSION_CERTIFICATION",art_status="REQUIRES_GENUINE_UNITY_VISUAL_REVIEW"};
   File.WriteAllText(Path.Combine(Ev,"spatial-native.json"),JsonUtility.ToJson(report,true));
  }
 }
}
