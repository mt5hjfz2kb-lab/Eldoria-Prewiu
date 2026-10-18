using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
public static class ValoriaReferenceCleanContactTrial {
 const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean";
 static string Ev=Path.GetFullPath("../docs/evidence/valoria-reference-clean");
 static string Hash(string path){using(var s=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(s.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
 [Serializable]class Report{public string source_sha,scene_sha256,scope,documentation;public bool scene_unchanged,same_camera,same_sun,after_opaque_before,after_opaque_after,art_approved=false;public int triangles,renderers;}
 static string Stamp(Camera c)=>c.transform.localToWorldMatrix.ToString("R")+c.orthographicSize;
 static void Views(Camera c,string prefix){var p=c.transform.position;var r=c.transform.rotation;float z=c.orthographicSize;try{
  ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-main.png",1448,1086);
  c.transform.position=new Vector3(24,31,-8);c.transform.LookAt(new Vector3(0,4,35.5f));c.orthographicSize=18;
  ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-bastion.png",1448,1086);
  c.transform.position=new Vector3(25,25,-48);c.transform.LookAt(new Vector3(-3,0,-20));c.orthographicSize=17;
  ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-gate.png",1448,1086);
 }finally{c.transform.SetPositionAndRotation(p,r);c.orthographicSize=z;}}
 public static void Build(){
  ValoriaReferenceCleanRenderSettings guard=null;UniversalRenderPipelineAsset original=null,trial=null;ScriptableRendererData renderer=null;ScriptableRendererFeature feature=null;
  try{
   string scene=Root+"/Valoria_Reference_Clean.unity",hash=Hash(scene);EditorSceneManager.OpenScene(scene,OpenSceneMode.Single);
   guard=Object.FindFirstObjectByType<ValoriaReferenceCleanRenderSettings>();original=guard.scenePipeline;guard.Apply();
   var c=Camera.main;string camera=Stamp(c),sun=RenderSettings.sun.transform.rotation.ToString("R")+RenderSettings.sun.intensity;
   Views(c,"contact-before");
   trial=Object.Instantiate(original);var ps=new SerializedObject(trial);var list=ps.FindProperty("m_RendererDataList");int index=ps.FindProperty("m_DefaultRendererIndex").intValue;
   renderer=Object.Instantiate((ScriptableRendererData)list.GetArrayElementAtIndex(index).objectReferenceValue);
   var prior=renderer.rendererFeatures.Single(f=>f.GetType().Name=="ScreenSpaceAmbientOcclusion");
   feature=Object.Instantiate(prior);renderer.rendererFeatures[renderer.rendererFeatures.IndexOf(prior)]=feature;
   var settings=new SerializedObject(feature);var setting=settings.FindProperty("m_Settings.AfterOpaque");if(setting==null||!setting.boolValue)throw new Exception("Expected existing AfterOpaque baseline");
   setting.boolValue=false;settings.ApplyModifiedPropertiesWithoutUndo();feature.Create();renderer.SetDirty();
   list.GetArrayElementAtIndex(index).objectReferenceValue=renderer;ps.ApplyModifiedPropertiesWithoutUndo();
   guard.Restore();guard.scenePipeline=trial;guard.Apply();Views(c,"contact-after");
   var report=new Report{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),scene_sha256=hash,scope="Only existing SSAO AfterOpaque true to false; same native scene/geometry/materials/GI/sun/exposure/SSAO strength and radius; trial restored",
    documentation="Context7 official Unity manual plus exact Unity6000.3 SSAO renderer-feature reference; AfterOpaque can over-darken baked occlusion",
    scene_unchanged=Hash(scene)==hash,same_camera=Stamp(c)==camera,same_sun=sun==RenderSettings.sun.transform.rotation.ToString("R")+RenderSettings.sun.intensity,
    after_opaque_before=true,after_opaque_after=false,triangles=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Sum(f=>f.sharedMesh.triangles.Length/3),renderers=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Length};
   if(!report.scene_unchanged||!report.same_camera||!report.same_sun)throw new Exception("Contact comparison invariant failed");
   File.WriteAllText(Ev+"/contact-comparison.json",JsonUtility.ToJson(report,true));
   guard.Restore();guard.scenePipeline=original;
   Object.DestroyImmediate(trial);Object.DestroyImmediate(renderer);Object.DestroyImmediate(feature);
   Debug.Log("VALORIA_REFERENCE_CLEAN_CONTACT_TRIAL_COMPLETE");EditorApplication.Exit(0);
  }catch(Exception e){if(guard){guard.Restore();guard.scenePipeline=original;}Debug.LogException(e);EditorApplication.Exit(1);}
 }
}
}
