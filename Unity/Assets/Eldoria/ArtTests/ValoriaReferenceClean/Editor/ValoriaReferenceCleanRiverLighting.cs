using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
public static class ValoriaReferenceCleanRiverLighting {
 const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean";
 const string Scene=Root+"/Valoria_Reference_Clean.unity";
 static readonly string Ev=Path.GetFullPath("../docs/evidence/valoria-reference-clean");
 [Serializable]class Report {public string source_sha,method="One existing baked128 cubemap / rough environment reflection / remove deep-water repeated caustic lattice";public bool same_camera,same_lighting,same_mesh,no_realtime_reflection=true,art_approved=false;public int cube_resolution,lightmaps,lightprobes,reflection_probes;public long active_lightmap_bytes,active_reflection_bytes;public string memory_note="Windows native referenced lighting textures only; not mobile process memory or FPS";}
 static string Stamp(Camera c)=>c.transform.position.ToString("R")+c.transform.rotation.ToString("R")+c.orthographicSize;
 static string LightStamp()=>RenderSettings.sun.transform.rotation.ToString("R")+RenderSettings.sun.color.ToString("R")+RenderSettings.sun.intensity+RenderSettings.ambientIntensity+RenderSettings.reflectionIntensity;
 static void Views(Camera cam,string prefix){var p=cam.transform.position;var q=cam.transform.rotation;var z=cam.orthographicSize;try{ValoriaReferenceCleanBuilder.Capture(cam,Ev+"/"+prefix+"-main.png",1448,1086);cam.transform.position=new Vector3(25,25,-48);cam.transform.LookAt(new Vector3(-3,0,-20));cam.orthographicSize=17;ValoriaReferenceCleanBuilder.Capture(cam,Ev+"/"+prefix+"-detail.png",1448,1086);}finally{cam.transform.SetPositionAndRotation(p,q);cam.orthographicSize=z;}}
 public static void Build(){try{
  var scene=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var guard=Object.FindFirstObjectByType<ValoriaReferenceCleanRenderSettings>();guard.Apply();var cam=Camera.main;var water=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Single(r=>r.name=="River water");var probe=Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None).Single(p=>p.name=="River baked reflection");if(!probe.bakedTexture)throw new Exception("Existing baked river cubemap missing");
  var home=Stamp(cam);var illumination=LightStamp();var mesh=water.GetComponent<MeshFilter>().sharedMesh;var original=water.sharedMaterial;var baseline=new Material(original);baseline.shader=Shader.Find("Eldoria/ReferenceClean/Comparison/RiverBeforeReflection");if(!baseline.shader)throw new Exception("Actual baseline shader missing");water.sharedMaterial=baseline;Views(cam,"river-lighting-before");
  var finish=new Material(original){name="River integrated with baked environment"};finish.SetTexture("_RiverReflection",probe.bakedTexture);finish.SetVector("_RiverReflection_HDR",probe.textureHDRDecodeValues);finish.SetFloat("_ReflectionStrength",1);const string asset=Root+"/GeneratedLighting/EnvironmentRiver.mat";if(AssetDatabase.LoadMainAssetAtPath(asset))AssetDatabase.DeleteAsset(asset);AssetDatabase.CreateAsset(finish,asset);water.sharedMaterial=finish;
  Views(cam,"river-lighting-after");ValoriaReferenceCleanBuilder.Capture(cam,Ev+"/D-final.png",1448,1086);ValoriaReferenceCleanBuilder.Capture(cam,Ev+"/C-art-scene.png",1448,1086);
  var p=cam.transform.position;var q=cam.transform.rotation;var z=cam.orthographicSize;cam.transform.position=new Vector3(24,31,-8);cam.transform.LookAt(new Vector3(0,4,35.5f));cam.orthographicSize=18;ValoriaReferenceCleanBuilder.Capture(cam,Ev+"/detail-bastion.png",1448,1086);cam.transform.position=new Vector3(25,25,-48);cam.transform.LookAt(new Vector3(-3,0,-20));cam.orthographicSize=17;ValoriaReferenceCleanBuilder.Capture(cam,Ev+"/detail-gate-bridge.png",1448,1086);cam.transform.SetPositionAndRotation(p,q);cam.orthographicSize=z;
  var maps=LightmapSettings.lightmaps.SelectMany(m=>new Texture[]{m.lightmapColor,m.lightmapDir,m.shadowMask}).Where(t=>t).Distinct().ToArray();var reflections=Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None);var cubes=reflections.Select(r=>r.bakedTexture).Where(t=>t).Distinct().ToArray();
  var report=new Report{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),same_camera=home==Stamp(cam),same_lighting=illumination==LightStamp(),same_mesh=mesh==water.GetComponent<MeshFilter>().sharedMesh,cube_resolution=probe.resolution,lightmaps=LightmapSettings.lightmaps.Length,lightprobes=LightmapSettings.lightProbes.count,reflection_probes=reflections.Length,active_lightmap_bytes=maps.Sum(t=>UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t)),active_reflection_bytes=cubes.Sum(t=>UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t))};File.WriteAllText(Ev+"/river-lighting-comparison.json",JsonUtility.ToJson(report,true));if(!report.same_camera||!report.same_lighting||!report.same_mesh||report.reflection_probes!=4)throw new Exception("River lighting isolation failed");
  Object.DestroyImmediate(baseline);EditorSceneManager.SaveScene(scene,Scene);AssetDatabase.SaveAssets();AssetDatabase.ExportPackage(new[]{Scene,Root+"/Prefabs"},Ev+"/Valoria_Reference_Clean.unitypackage",ExportPackageOptions.Recurse|ExportPackageOptions.IncludeDependencies);guard.Restore();Debug.Log("VALORIA_REFERENCE_CLEAN_RIVER_LIGHTING_COMPLETE");EditorApplication.Exit(0);
 }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
}
}
