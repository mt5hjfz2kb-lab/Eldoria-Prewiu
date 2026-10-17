using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace Eldoria.ReferenceClean.Editor {
[InitializeOnLoad]
public static class ValoriaReferenceCleanPlayProbe {
 const string Flag="ValoriaReferenceClean.PlayProbe";static double entered;static int errors;
 static ValoriaReferenceCleanPlayProbe(){EditorApplication.playModeStateChanged+=Changed;UnityEngine.Application.logMessageReceived+=Log;}
 static void Log(string message,string stack,LogType type){if(SessionState.GetBool(Flag,false)&&(type==LogType.Exception||type==LogType.Error))errors++;}
 public static void Build(){SessionState.SetBool(Flag,true);SessionState.SetInt(Flag+"Result",0);EditorSceneManager.OpenScene("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Valoria_Reference_Clean.unity",OpenSceneMode.Single);EditorApplication.EnterPlaymode();}
 static void Changed(PlayModeStateChange state){if(!SessionState.GetBool(Flag,false))return;if(state==PlayModeStateChange.EnteredPlayMode){entered=EditorApplication.timeSinceStartup;errors=0;EditorApplication.update+=Tick;}if(state==PlayModeStateChange.EnteredEditMode){bool pass=SessionState.GetInt(Flag+"Result",0)==1;SessionState.SetBool(Flag,false);Debug.Log(pass?"VALORIA_REFERENCE_CLEAN_PLAY_COMPLETE":"VALORIA_REFERENCE_CLEAN_PLAY_FAILED");EditorApplication.Exit(pass?0:1);}}
 static void Tick(){if(EditorApplication.timeSinceStartup-entered<3)return;EditorApplication.update-=Tick;
 try {
 var interaction=UnityEngine.Object.FindFirstObjectByType<ValoriaReferenceCleanInteraction>();if(!interaction||!interaction.isActiveAndEnabled||!UnityEngine.Application.isPlaying)throw new Exception("Live PlayMode interaction missing");
 var cam=interaction.playerCamera;Physics.SyncTransforms();int count=0;
 foreach(var box in UnityEngine.Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None).Where(c=>c.gameObject.layer==8)){
 var point=cam.WorldToScreenPoint(box.bounds.center);if(!Physics.Raycast(cam.ScreenPointToRay(point),out var hit,500,1<<8)||hit.collider!=box)throw new Exception("PlayMode ray selection: "+box.name);interaction.Select(hit.collider.name);if(box.name.StartsWith("Parcela")&&interaction.plot<1)throw new Exception("PlayMode parcel state missing");count++;
 }
 float before=cam.orthographicSize;cam.orthographicSize=24;ValoriaReferenceCleanBuilder.Capture(cam,"../docs/evidence/valoria-reference-clean/playmode-zoom.png",1448,1086);cam.orthographicSize=before;
 var origin=cam.transform.position;cam.transform.position+=Vector3.right*6;ValoriaReferenceCleanBuilder.Capture(cam,"../docs/evidence/valoria-reference-clean/playmode-pan.png",1448,1086);cam.transform.position=origin;interaction.Select("Bastión I");
 ValoriaReferenceCleanBuilder.Capture(cam,"../docs/evidence/valoria-reference-clean/playmode-home.png",1448,1086);
 File.WriteAllText("../docs/evidence/valoria-reference-clean/playmode.json",JsonUtility.ToJson(new Report{playmode=true,selectable_targets=count,zoom_camera_changed=true,pan_camera_changed=true,exceptions=errors,editor_batches=UnityStats.batches,editor_setpass_calls=UnityStats.setPassCalls,editor_triangles=UnityStats.triangles,physical_mobile_tested=false},true));
 if(count!=5||errors!=0)throw new Exception("Live probes fail: targets="+count+", errors="+errors);SessionState.SetInt(Flag+"Result",1);
 }catch(Exception e){Debug.LogException(e);SessionState.SetInt(Flag+"Result",0);}EditorApplication.ExitPlaymode();
 }
 [Serializable]class Report{public bool playmode,zoom_camera_changed,pan_camera_changed,physical_mobile_tested;public int selectable_targets,exceptions,editor_batches,editor_setpass_calls,editor_triangles;}
}
}
