#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Eldoria.ReferenceClean.Editor {
 public static class ValoriaIntegratedWebBuild {
  const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
  const string Output="Builds/ValoriaWebPlaytest";
  const string Ev="../docs/evidence/valoria-reference-clean";
  [Serializable] class Result {public string source_sha,status,scene,error;public long bytes;public bool physical_iphone_tested;public string[] files;}
  public static void Build(){
   var result=new Result{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),scene=Scene,status="WEB_BUILD_FAILED"};
   var libraries=new Dictionary<string,byte[]>();int exit=1;
   try{
    if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL,BuildTarget.WebGL))throw new Exception("Installed Unity WebGL module unavailable");
    var proof=File.ReadAllText(Ev+"/integration-result.json");
    if(!proof.Contains("INTEGRATION_PASS_ART_REVIEW_REQUIRED"))throw new Exception("Integrated I-II native proof missing");
    var expectedPath=Ev+"/integration-input-signature.txt";
    var manifestPath=Ev+"/integration-input-manifest.txt";
    var currentManifest=ValoriaIntegrationBuild.ComputeInputManifest();
    var currentSignature=ValoriaIntegrationBuild.ComputeInputSignature();
    File.WriteAllText(Ev+"/web-input-current-manifest.txt",currentManifest);
    File.WriteAllText(Ev+"/web-input-current-signature.txt",currentSignature);
    if(!File.Exists(expectedPath))throw new Exception("Integrated source input signature absent");
    var expectedSignature=File.ReadAllText(expectedPath).Trim();
    if(expectedSignature!=currentSignature){
     var oldLines=File.Exists(manifestPath)?File.ReadAllLines(manifestPath):Array.Empty<string>();
     var nowLines=currentManifest.Split(new[]{'\n'},StringSplitOptions.None);
     var oldMap=oldLines.ToDictionary(l=>l.Substring(0,Math.Max(0,l.LastIndexOf(' '))),l=>l);
     var newMap=nowLines.ToDictionary(l=>l.Substring(0,Math.Max(0,l.LastIndexOf(' '))),l=>l);
     var changed=oldMap.Keys.Union(newMap.Keys).Where(path=>
      !oldMap.TryGetValue(path,out var before)||!newMap.TryGetValue(path,out var after)||before!=after)
      .OrderBy(x=>x,StringComparer.Ordinal).ToArray();
     File.WriteAllLines(Ev+"/web-input-signature-differences.txt",changed);
     throw new Exception("Integrated WebGL input mismatch against current Win64 native proof. expected="+expectedSignature+" current="+currentSignature+" changed="+changed.Length+" paths="+string.Join(" ; ",changed.Take(12)));
    }
    EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
    var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
    if(!host||!host.HudPrefab||!host.WorldRoot||!host.WorldCamera)throw new Exception("Integrated city/UI/Zone1 bindings missing");
    var render=Object.FindFirstObjectByType<ValoriaReferenceCleanRenderSettings>();
    if(!render||!render.scenePipeline)throw new Exception("Validated scene pipeline missing");
    GraphicsSettings.defaultRenderPipeline=render.scenePipeline;QualitySettings.renderPipeline=render.scenePipeline;
    // Current scene contains authored mesh/material assets. Unused historical resource
    // libraries must not drag old production splat/source families into this playtest.
    var dependencies=new HashSet<string>(AssetDatabase.GetDependencies(Scene,true));
    foreach(var path in AssetDatabase.FindAssets("t:ValoriaExternalAssetLibrary").Select(AssetDatabase.GUIDToAssetPath)){
     if(dependencies.Contains(path))throw new Exception("Current scene unexpectedly depends on legacy resource library: "+path);
     libraries[path]=File.ReadAllBytes(path);
     var asset=AssetDatabase.LoadMainAssetAtPath(path);var serialized=new SerializedObject(asset);var it=serialized.GetIterator();
     while(it.Next(true))if(it.propertyType==SerializedPropertyType.ObjectReference&&it.name!="m_Script")it.objectReferenceValue=null;
     serialized.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(asset);
    }
    AssetDatabase.SaveAssets();
    File.WriteAllLines(Ev+"/web-scene-dependencies.txt",dependencies.OrderBy(x=>x));
    PlayerSettings.companyName="Eldoria";PlayerSettings.productName="Valoria · Ciudad y Zona 1";
    PlayerSettings.SetScriptingBackend(NamedBuildTarget.WebGL,ScriptingImplementation.IL2CPP);
    PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL,ManagedStrippingLevel.Medium);
    PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
    PlayerSettings.WebGL.decompressionFallback=true;
    PlayerSettings.WebGL.dataCaching=true;
    PlayerSettings.WebGL.template="APPLICATION:Minimal";
    PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL,false);
    PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL,new[]{GraphicsDeviceType.OpenGLES3});
    Directory.CreateDirectory(Output);
    var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Scene},locationPathName=Output,target=BuildTarget.WebGL,options=BuildOptions.StrictMode});
    if(report.summary.result!=BuildResult.Succeeded||!File.Exists(Output+"/index.html"))throw new Exception("Web build failed: "+report.summary.result);
    result.bytes=(long)report.summary.totalSize;result.files=Directory.GetFiles(Output,"*",SearchOption.AllDirectories);
    result.status="WEB_COMPILED_CURRENT_INTEGRATED_SCENE_NOT_PHYSICAL_IPHONE_TESTED";exit=0;
    Debug.Log("VALORIA_REFERENCE_CLEAN_WEB_COMPLETE bytes="+result.bytes);
   }catch(Exception e){result.error=e.ToString();Debug.LogException(e);}
   finally{foreach(var pair in libraries)File.WriteAllBytes(pair.Key,pair.Value);File.WriteAllText(Ev+"/web-build.json",JsonUtility.ToJson(result,true));}
   EditorApplication.Exit(exit);
  }
 }
}
#endif
