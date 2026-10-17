// Valoria: isolate macro-composition, never hide 3D-vs-SHARP mismatch with FOV.
// Player HOME and all 7 original semantic source rays measured, no gameplay/scene promotion.
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Eldoria.Presentation;
namespace Eldoria.ValoriaLayered.Editor {
 public static class ValoriaMacroScreenEnvelopeCandidateV1 {
   const string Root="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/";
   const string Input=Root+"Valoria_SHARP_SemanticContact_IsolatedV1.unity";
   const string Output=Root+"Valoria_SHARP_MacroEnvelope_CANDIDATE.unity";
   const int W=1536,H=1024,SW=1230,SH=845;
   static readonly string E=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..",
      "docs","evidence","valoria-layered-construction-20261016","macro-envelope"));
   [Serializable] class A{public string id; public float[] sharp_approved_normalized;}
   [Serializable] class F{public A[] anchors;}
   [Serializable] class InputModel{public F global_fit;}
   [Serializable] class Trial {
      public float dx,dy,dz,gate_width_percent,gate_height_percent,gate_center_error_px;
      public int semantic_contacts;public bool valid_macro_footprint,gate_entirely_visible;
      public float penalty;
   }
   [Serializable] class Result {
      public string camera_authority,source_scene,candidate_scene,visual_status,gameplay_status;
      public bool original_camera_untouched,source_geometry_untouched,canonical_modified,spatially_upright;
      public int total_trials,seven_contacts_count;
      public float approved_gate_width_percent,approved_gate_height_percent,approved_gate_center_x,approved_gate_center_y;
      public Trial baseline,best_all,best_seven_contacts;
      public Trial[] trials;
   }
   static void Check(bool x,string s){if(!x)throw new Exception("SHARP_MACRO_GATE: "+s);}
   static Bounds B(IEnumerable<Renderer> rs){
      var arr=rs.ToArray();Check(arr.Length>0,"missing gate arch/tower renderers");
      var b=arr[0].bounds;foreach(var a in arr.Skip(1))b.Encapsulate(a.bounds);return b;
   }
   static Vector3[] Corners(Bounds b){
      var a=new List<Vector3>();
      foreach(float x in new[]{b.min.x,b.max.x})
      foreach(float y in new[]{b.min.y,b.max.y})
      foreach(float z in new[]{b.min.z,b.max.z})a.Add(new Vector3(x,y,z));
      return a.ToArray();
   }
   static readonly string[] Names={"Bridge","LowerGate","MainRoad","CentralStair",
      "Bastion","LeftCabinParcel","RightCampParcel"};
   static bool IsSemantic(string id,string obj){
      switch(id){
       case "Bridge":return obj=="TEMPORARY bridge / continuous deck and collider";
       case "LowerGate":return obj=="SUPPORT/RESERVE Puerta · ir al mundo";
       case "MainRoad":return obj=="GREYBOX VPD · L0 main street";
       case "CentralStair":return obj.StartsWith("GREYBOX certified stair step ");
       case "Bastion":return obj=="GREYBOX Bastion hero reservation, silhouette only";
       case "LeftCabinParcel":return obj=="SUPPORT/RESERVE VPD · L0 west plot";
       case "RightCampParcel":return obj=="SUPPORT/RESERVE VPD · L0 east plot";
       default: return false;
      }
   }
   static float WidthTarget=273f/W,HeightTarget=211f/H;
   static Trial Test(Camera cam,Transform frame,Vector3 basePosition,Renderer[] sourceParts,
       A[] anchors,Vector3 offset){
      frame.position=basePosition+offset;Physics.SyncTransforms();
      var outp=new Trial{dx=offset.x,dy=offset.y,dz=offset.z};
      cam.aspect=(float)W/H;
      var bounds=B(sourceParts);
      var coords=Corners(bounds).Select(c=>cam.WorldToViewportPoint(c)).ToArray();
      if(coords.Any(v=>v.z<cam.nearClipPlane)){
          outp.penalty=100000;return outp;
      }
      float minx=coords.Min(x=>x.x),maxx=coords.Max(x=>x.x),
        miny=coords.Min(x=>x.y),maxy=coords.Max(x=>x.y);
      outp.gate_entirely_visible=minx>=0&&maxx<=1&&miny>=0&&maxy<=1;
      outp.gate_width_percent=maxx-minx;outp.gate_height_percent=maxy-miny;
      float targetX=(565f+838f)/2/W;
      float targetY=1-(526f+737f)/2/H;
      outp.gate_center_error_px=Mathf.Sqrt(
         Mathf.Pow(((minx+maxx)/2-targetX)*W,2)+
         Mathf.Pow(((miny+maxy)/2-targetY)*H,2));
      outp.valid_macro_footprint=outp.gate_width_percent>=.75f*WidthTarget &&
       outp.gate_width_percent<=1.25f*WidthTarget &&
       outp.gate_height_percent>=.75f*HeightTarget &&
       outp.gate_height_percent<=1.25f*HeightTarget &&
       outp.gate_center_error_px<=Mathf.Sqrt(W*W+H*H)*.025f &&
       outp.gate_entirely_visible;
      cam.aspect=(float)SW/SH;
      int matched=0;
      foreach(var a in anchors){
         var q=a.sharp_approved_normalized;
         Check(q!=null&&q.Length==2,"seven canonical viewport anchors required");
         var hits=Physics.RaycastAll(cam.ViewportPointToRay(new Vector3(q[0],q[1],0)),300f,~0,
             QueryTriggerInteraction.Collide);
         if(hits.Any(hit=>IsSemantic(a.id,hit.collider.name)))matched++;
      }
      outp.semantic_contacts=matched;
      // 7/7 actual ray coverage and source bbox are independent conditions.
      outp.penalty=(7-matched)*10000+
         Mathf.Abs(outp.gate_width_percent-WidthTarget)*W*4+
         Mathf.Abs(outp.gate_height_percent-HeightTarget)*H*2+
         outp.gate_center_error_px;
      return outp;
   }
   static void Shot(Camera c,string name,int w,int h){
       float oldAspect=c.aspect;var prev=c.targetTexture;var active=RenderTexture.active;
       var tex=new Texture2D(w,h,TextureFormat.RGB24,false);
       var rt=new RenderTexture(w,h,24){antiAliasing=4};
       try{
         c.aspect=(float)w/h;c.targetTexture=rt;c.Render();c.Render();
         RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();
         File.WriteAllBytes(Path.Combine(E,name+".png"),tex.EncodeToPNG());
       }finally{
         c.aspect=oldAspect;c.targetTexture=prev;RenderTexture.active=active;
         rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);
       }
   }
   [MenuItem("Eldoria/Valoria/Measure actual SHARP gate screen envelope with 3D relocation candidates")]
   public static void Build(){
      try{BuildCore();Debug.Log("VALORIA_MACRO_SCREEN_CANDIDATES_NATIVE_COMPLETE");EditorApplication.Exit(0);}
      catch(Exception e){Debug.LogError("VALORIA_MACRO_SCREEN_CANDIDATES_NATIVE_FAILED "+e);EditorApplication.Exit(1);}
   }
   static void BuildCore(){
       Check(File.Exists(Input),"No preserved SHARP source 7-click scene");
       var source=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..",
          "docs","evidence","valoria-layered-construction-20261016","sharp-camera-mapping",
          "sharp-camera-rigid-calibration.json"));
       Check(File.Exists(source),"No certified source anchors");
       var refFile=JsonUtility.FromJson<InputModel>(File.ReadAllText(source));
       Check(refFile.global_fit.anchors.Length==7,"Need exact 7 anchors");
       var scene=EditorSceneManager.OpenScene(Input,OpenSceneMode.Single);
       var frame=UnityEngine.Object.FindObjectsByType<Transform>(
          FindObjectsInactive.Include,FindObjectsSortMode.None)
          .SingleOrDefault(x=>x.name.StartsWith("DIAGNOSTIC / native 3D rigid world-to-SHARP"));
       Check(frame!=null,"No existing calibrated hierarchy");
       var controls=UnityEngine.Object.FindObjectsByType<ValoriaParcelPresentation>(
          FindObjectsInactive.Include,FindObjectsSortMode.None);
       Check(controls.Length==1,"Require one and only one real player camera");
       controls[0].Home();var camera=controls[0].ProductionCamera;
       Check(camera!=null&&!camera.orthographic&&
          Mathf.Abs(camera.fieldOfView-44.42281f)<.0001f,"Camera authority changed");
       var gate=UnityEngine.Object.FindObjectsByType<Transform>(
          FindObjectsInactive.Include,FindObjectsSortMode.None)
          .SingleOrDefault(x=>x.name=="LAYER D - modular lower gate from six real source GLBs");
       Check(gate!=null&&gate.IsChildOf(frame),"Expected original six-source gate assembly missing");
       // Include only the two hero towers and arch, NEVER the long side walls.
       var partRoots=gate.Cast<Transform>().Where(t=>t.name=="PRODUCTION_SOURCE_CANDIDATE__tower_square_short"||
            t.name=="PRODUCTION_SOURCE_CANDIDATE__arch_complete_major").ToArray();
       Check(partRoots.Length==3,"Two towers + arch are a single target family");
       var rs=partRoots.SelectMany(t=>t.GetComponentsInChildren<Renderer>(true)).ToArray();
       var baseP=frame.position;
       Directory.CreateDirectory(E);
       var all=new List<Trial>();
       // Finite controlled candidate placements. Native geometry/scale and actual camera FOV unchanged.
       foreach(float dz in new[]{0f,4f,8f,12f,16f,20f,24f,28f,32f,40f})
       foreach(float dx in new[]{-5f,-2.5f,0f,2.5f,5f})
       foreach(float dy in new[]{-5f,-2.5f,0f,2.5f,5f})
          all.Add(Test(camera,frame,baseP,rs,refFile.global_fit.anchors,new Vector3(dx,dy,dz)));
       var baseline=all.Single(t=>Mathf.Abs(t.dx)<.0001f&&Mathf.Abs(t.dy)<.0001f&&Mathf.Abs(t.dz)<.0001f);
       var good=all.Where(t=>t.semantic_contacts==7).OrderBy(t=>t.penalty).ToArray();
       var chosen=good.Length>0?good[0]:all.OrderBy(t=>t.penalty).First();
       frame.position=baseP;Physics.SyncTransforms();
       Shot(camera,"macro-source-home",W,H);
       frame.position=baseP+new Vector3(chosen.dx,chosen.dy,chosen.dz);
       Physics.SyncTransforms();
       Shot(camera,"macro-candidate-home",W,H);
       Shot(camera,"macro-candidate-portrait",390,844);
       // This candidate is geometrically reversible and cannot modify the canonical scene.
       Check(EditorSceneManager.SaveScene(scene,Output,true),"Cannot save separate macro candidate");
       var result=new Result{
         camera_authority="ValoriaParcelPresentation actual player perspective FOV 44.42281",
         source_scene=Input,candidate_scene=Output,
         original_camera_untouched=true,source_geometry_untouched=true,canonical_modified=false,
         spatially_upright=false,total_trials=all.Count,
         seven_contacts_count=all.Count(t=>t.semantic_contacts==7),
         approved_gate_width_percent=WidthTarget,
         approved_gate_height_percent=HeightTarget,
         approved_gate_center_x=(565f+838f)/2/W,
         approved_gate_center_y=1-(526f+737f)/2/H,
         baseline=baseline,best_all=all.OrderBy(x=>x.penalty).First(),
         best_seven_contacts=good.Length>0?good[0]:null,trials=all.ToArray(),
         visual_status=chosen.semantic_contacts==7&&chosen.valid_macro_footprint?
           "MACRO_GEOMETRIC_CANDIDATE_NOT_ART_APPROVED":"NO_MACRO_RECALIBRATION_APPROVED",
         gameplay_status="NO_PLAYERSTATE_OR_GLOBAL_GRAVITY_PASS"
       };
       File.WriteAllText(Path.Combine(E,"macro-screen-envelope-trials.json"),JsonUtility.ToJson(result,true));
       Debug.Log("VALORIA_MACRO_SEARCH_TRUE_SHARP "+all.Count+" trials 7/7="+result.seven_contacts_count+
        " baseline_width="+baseline.gate_width_percent+
        " best_width="+chosen.gate_width_percent+" target_width="+WidthTarget+
        " best_center_error_px="+chosen.gate_center_error_px+
        " macro_pass="+chosen.valid_macro_footprint);
    }
 }
}
