using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.ReferenceClean.Editor {
// Reuses the project's proven Android playtest build procedure, scoped to this scene.
public static class ValoriaReferenceCleanAndroidBuild {
 const string ScenePath="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Valoria_Reference_Clean.unity";
 const string Evidence="../docs/evidence/valoria-reference-clean/android-build.json";
 [Serializable]class Result { public string source_sha,status,apk_sha256,package_id,graphics_apis,note;public long apk_bytes;public int errors,warnings;public bool physical_device_tested; }
 public static void Build(){
  var result=new Result{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),package_id="com.eldoria.valoriareferenceclean",graphics_apis="Vulkan, OpenGLES3",physical_device_tested=false,note="ARM64 IL2CPP development APK compilation only; no physical visual or performance certification."};
  int exit=1;string settings="ProjectSettings/ProjectSettings.asset";byte[] original=File.ReadAllBytes(settings);
  try{
   if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))throw new Exception("Installed editor lacks Android Build Support");
   EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
   if(!UnityEngine.Object.FindFirstObjectByType<ValoriaReferenceCleanInteraction>())throw new Exception("Independent scene interaction missing");
   PlayerSettings.companyName="Eldoria";PlayerSettings.productName="Valoria Reference Clean";
   PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,result.package_id);
   PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
   PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
   PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel29;
   PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;
   PlayerSettings.Android.useCustomKeystore=false;
   PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
   PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);
   PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.Vulkan,GraphicsDeviceType.OpenGLES3});
   EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
   const string apk="Builds/ValoriaReferenceClean/Valoria-Reference-Clean.apk";Directory.CreateDirectory(Path.GetDirectoryName(apk));
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=apk,target=BuildTarget.Android,options=BuildOptions.Development|BuildOptions.StrictMode});
   result.errors=(int)report.summary.totalErrors;result.warnings=(int)report.summary.totalWarnings;
   if(report.summary.result!=BuildResult.Succeeded||!File.Exists(apk))throw new Exception("Android build failed: "+report.summary.result);
   result.apk_bytes=new FileInfo(apk).Length;
   using(var sha=System.Security.Cryptography.SHA256.Create())using(var stream=File.OpenRead(apk))result.apk_sha256=BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
   result.status="APK_COMPILED_NOT_DEVICE_VALIDATED";exit=0;Debug.Log("VALORIA_REFERENCE_CLEAN_ANDROID_COMPLETE bytes="+result.apk_bytes);
  }catch(Exception e){result.status="ANDROID_BUILD_FAILED";result.note=e.ToString();Debug.LogException(e);}
  finally{File.WriteAllText(Evidence,JsonUtility.ToJson(result,true));File.WriteAllBytes(settings,original);}
  EditorApplication.Exit(exit);
 }
}
}
