// Owner camera gate: ACTUAL ValoriaParcelPresentation.Home/Pan/Zoom authority.
// Opens preserved historical 3D source and tests a single rigid native-metre embedding.
// Renders perspective diagnostic; NEVER claims real SHARP Gaussian beauty/gameplay or art approval.
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
 public static class ValoriaSharpPlayerCameraCorrespondence
 {
   const string Root="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction";
   const string InputScene=Root+"/Valoria_MasterPlan_Layers_ABCD_BridgeV1.unity";
   const string OutputScene=Root+"/Valoria_SHARP_Camera_Mapping_DIAGNOSTIC.unity";
   const string Filename="docs/evidence/valoria-layered-construction-20261016/sharp-camera-mapping/sharp-camera-rigid-calibration.json";
   static readonly string Dir=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
     "..","..","docs","evidence","valoria-layered-construction-20261016","sharp-camera-mapping"));
   const int W=1230,H=845;
   [Serializable] sealed class Anchor{
     public string id;
     public float[] legacy_world_xyz;
     public float[] sharp_approved_normalized;
     public float[] fitted_viewport;
     public float residual_px;
   }
   [Serializable] sealed class Calibration{
     public GlobalFit global_fit;
     public bool camera_to_world_mapping_pass;
     public string camera_authority;
     public float camera_fov_degrees;
   }
   [Serializable] sealed class GlobalFit{
     public float[] quaternion_xyzw;
     public float[] translation_xyz;
     public Anchor[] anchors;
   }
   [Serializable] sealed class Observation{
     public string id;
     public float target_x,target_y,actual_x,actual_y;
     public float error_px,python_projection_delta_px;
     public bool in_front,visible;
   }
   [Serializable] sealed class State{
     public string name;
     public float fov;
     public float[] position;
     public int screenshot_w,screenshot_h;
   }
   [Serializable] sealed class Evidence{
     public string authority,scene,source_scene,technical_gate,correspondence_gate,art_gate,gameplay_gate;
     public bool actual_player_camera_component,source_geometry_unchanged,visual_approval,canonical_scene_changed;
     public float camera_home_fov,maximum_semantic_error_px,maximum_python_unity_delta_px;
     public int semantic_anchors_measured;
     public Observation[] observations;
     public State[] states;
   }
   [MenuItem("Eldoria/Valoria/Calibrate genuine SHARP player camera vs structural city")]
   public static void Build(){
     try{BuildCore();Debug.Log("VALORIA_REAL_PLAYER_CAMERA_RIGID_DIAGNOSTIC_COMPLETE");EditorApplication.Exit(0);}
     catch(Exception ex){Debug.LogError("VALORIA_REAL_PLAYER_CAMERA_RIGID_DIAGNOSTIC_FAILED "+ex);EditorApplication.Exit(1);}
   }
   static void Check(bool ok,string m){if(!ok)throw new Exception("SHARP_PLAYER_CAMERA_GATE: "+m);}
   static Vector3 V(float[] a){Check(a!=null&&a.Length==3,"missing XYZ");return new Vector3(a[0],a[1],a[2]);}
   static Quaternion Q(float[] a){
     Check(a!=null&&a.Length==4,"missing XYZW");
     return new Quaternion(a[0],a[1],a[2],a[3]);
   }
   static void Shot(Camera cam,string label,int w,int h){
      var old=cam.targetTexture;var active=RenderTexture.active;
      var target=new RenderTexture(w,h,24){antiAliasing=4};
      var texture=new Texture2D(w,h,TextureFormat.RGB24,false);
      try{
        cam.targetTexture=target;cam.aspect=w/(float)h;cam.Render();
        RenderTexture.active=target;
        texture.ReadPixels(new Rect(0,0,w,h),0,0);texture.Apply();
        File.WriteAllBytes(Path.Combine(Dir,label+".png"),texture.EncodeToPNG());
      }finally{
        cam.targetTexture=old;RenderTexture.active=active;
        target.Release();UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(target);
      }
   }
   static void BuildCore(){
      Check(File.Exists(InputScene),"Preserved BridgeV1 scene missing");
      var path=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..",Filename));
      Check(File.Exists(path),"Missing Python real-anchor camera fit, cannot guess it");
      var cal=JsonUtility.FromJson<Calibration>(File.ReadAllText(path));
      Check(cal!=null&&cal.global_fit!=null&&cal.global_fit.anchors!=null&&
            cal.global_fit.anchors.Length==7,"Expected seven source-recorded semantic anchors");
      Check(Mathf.Abs(cal.camera_fov_degrees-44.42281f)<.0001f,"SHARP player FOV mismatched");
      Directory.CreateDirectory(Dir);
      var scene=EditorSceneManager.OpenScene(InputScene,OpenSceneMode.Single);
      // Hide the historical camera, but DO NOT delete or edit master plan or gameplay.
      foreach(var prior in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
         prior.enabled=false;
         if(prior.CompareTag("MainCamera")) prior.gameObject.tag="Untagged";
      }
      var frame=new GameObject("DIAGNOSTIC / native 3D rigid world-to-SHARP (NOT CERTIFIED)").transform;
      var sceneRoots=scene.GetRootGameObjects();
      foreach(var root in sceneRoots){
         if(root==frame.gameObject)continue;
         root.transform.SetParent(frame,true);
      }
      frame.SetPositionAndRotation(V(cal.global_fit.translation_xyz),Q(cal.global_fit.quaternion_xyzw));
      frame.localScale=Vector3.one;
      var cam=new GameObject("SHARP_ACTUAL_PLAYER_HOME_CAMERA_NOT_HISTORICAL_QA").AddComponent<Camera>();
      cam.tag="MainCamera";
      cam.orthographic=false;
      cam.clearFlags=CameraClearFlags.SolidColor;
      cam.backgroundColor=new Color(.16f,.20f,.23f);
      cam.nearClipPlane=.05f;cam.farClipPlane=600;
      cam.aspect=W/(float)H;
      var controller=cam.gameObject.AddComponent<ValoriaParcelPresentation>();
      controller.ProductionCamera=cam;
      // REAL game camera implementation, not hardcoded look-at adjustment.
      controller.Home();
      Check(!cam.orthographic &&
         Vector3.Distance(cam.transform.position,Vector3.zero)<.0001f &&
         Quaternion.Angle(cam.transform.rotation,Quaternion.identity)<.001f &&
         Mathf.Abs(cam.fieldOfView-44.42281f)<.0001f,"Actual player Home() contract failed");
      Physics.SyncTransforms();
      var observations=new List<Observation>();
      float maxError=0,maxDelta=0;
      foreach(var a in cal.global_fit.anchors){
         var canonical=frame.TransformPoint(V(a.legacy_world_xyz));
         Vector3 projected=cam.WorldToViewportPoint(canonical);
         float err=Mathf.Sqrt(Mathf.Pow((projected.x-a.sharp_approved_normalized[0])*W,2)+
                             Mathf.Pow((projected.y-a.sharp_approved_normalized[1])*H,2));
         float delta=Mathf.Sqrt(Mathf.Pow((projected.x-a.fitted_viewport[0])*W,2)+
                               Mathf.Pow((projected.y-a.fitted_viewport[1])*H,2));
         Check(delta<=.75f,"Python geometric fit and Unity real camera disagree at "+a.id+" by "+delta+" px");
         observations.Add(new Observation{
           id=a.id,actual_x=projected.x,actual_y=projected.y,
           target_x=a.sharp_approved_normalized[0],target_y=a.sharp_approved_normalized[1],
           error_px=err,python_projection_delta_px=delta,
           in_front=projected.z>0,visible=projected.z>0 && projected.x>=0 && projected.x<=1 &&
                   projected.y>=0&&projected.y<=1
         });
         maxError=Mathf.Max(maxError,err);maxDelta=Mathf.Max(maxDelta,delta);
      }
      // Visual marker spheres are diagnostic only; no colliders and no fake WorldHotspots.
      var gizmos=new GameObject("SUPPORT / SEMANTIC ANCHOR BALLS - DIAGNOSTIC ONLY");
      var mat=new Material(Shader.Find("Universal Render Pipeline/Unlit")??Shader.Find("Unlit/Color"));
      if(mat.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",new Color(.96f,.23f,.20f));
      else mat.color=new Color(.96f,.23f,.20f);
      foreach(var a in cal.global_fit.anchors){
         var ball=GameObject.CreatePrimitive(PrimitiveType.Sphere);
         ball.name="DIAGNOSTIC_RENDERED_"+a.id;ball.transform.SetParent(gizmos.transform,false);
         ball.transform.position=frame.TransformPoint(V(a.legacy_world_xyz));
         ball.transform.localScale=Vector3.one*.37f;
         ball.GetComponent<Renderer>().sharedMaterial=mat;
         UnityEngine.Object.DestroyImmediate(ball.GetComponent<Collider>());
      }
      var states=new List<State>();
      void Save(string label,int w,int h){
         Shot(cam,label,w,h);
         var p=cam.transform.position;
         states.Add(new State{name=label,fov=cam.fieldOfView,
            position=new[]{p.x,p.y,p.z},screenshot_w=w,screenshot_h=h});
      }
      Save("sharp-perspective-home-1230x845",W,H);
      controller.Pan(new Vector2(41.6667f,0));Save("sharp-perspective-pan-left",W,H);
      controller.Home();controller.Pan(new Vector2(-41.6667f,0));Save("sharp-perspective-pan-right",W,H);
      controller.Home();controller.Zoom(-2.0f);Save("sharp-perspective-zoom-in",W,H);
      controller.Home();controller.Zoom(2.0f);Save("sharp-perspective-zoom-out",W,H);
      controller.Home();Save("sharp-perspective-portrait-390x844",390,844);
      Check(states.Count==6,"Missing real camera states");
      Check(Mathf.Abs(states[1].position[0]+.5f)<.002f&&
            Mathf.Abs(states[2].position[0]-.5f)<.002f,"Real SHARP controller pan not valid");
      controller.Home();
      Check(EditorSceneManager.SaveScene(scene,OutputScene,true),"Cannot save isolated diagnostic scene");
      var evidence=new Evidence{
          authority="ValoriaParcelPresentation.Home/Pan/Zoom with SHARP perspective projection",
          scene=OutputScene,source_scene=InputScene,
          technical_gate="PLAYER_CAMERA_RUNTIME_COMPONENT_AND_PYTHON_UNITY_PROJECTION_PASS",
          correspondence_gate=maxError<=12f?"RIGID_MAPPING_SCREEN_ERROR_PASS":"RIGID_MAPPING_REJECTED_NO_FALSE_GAMEPLAY_PASS",
          art_gate="NOT_APPROVED_DIAGNOSTIC_ONLY",gameplay_gate="NOT_TESTED",
          actual_player_camera_component=true,source_geometry_unchanged=true,
          visual_approval=false,canonical_scene_changed=false,
          camera_home_fov=44.42281f,maximum_semantic_error_px=maxError,
          maximum_python_unity_delta_px=maxDelta,semantic_anchors_measured=observations.Count,
          observations=observations.ToArray(),states=states.ToArray()
      };
      File.WriteAllText(Path.Combine(Dir,"sharp-camera-native-evidence.json"),JsonUtility.ToJson(evidence,true));
      Debug.Log("VALORIA_SHARP_NATIVE_CAMERA_PROJECTION_PASS max_expected_error="+maxError+
                " max_python_unity_delta="+maxDelta+" mappingPASS="+(maxError<=12f));
   }
 }
}
