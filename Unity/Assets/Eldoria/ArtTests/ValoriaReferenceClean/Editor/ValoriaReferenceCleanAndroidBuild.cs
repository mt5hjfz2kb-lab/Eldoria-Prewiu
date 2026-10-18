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
 [Serializable]class Key{public string path,certificate_sha256;public bool matches_original;}
 [Serializable]class KeyReport{public Key[] keys;public string package;public int old_version_code;}
 [Serializable]class Result { public string source_sha,status,apk_sha256,package_id,graphics_apis,note;public long apk_bytes;public int errors,warnings;public bool physical_device_tested; }
 public static void Build(){
  var result=new Result{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),package_id="com.eldoria.valoria.v5test",graphics_apis="Vulkan, OpenGLES3",physical_device_tested=false,note="Update of recovered optimized APK38279800977: same package and verified existing signing certificate, versionCode5; single current Valoria scene, known source-only GLBs excluded/restored by existing packaging procedure. No data reset/deletion. Phone install/performance still requires owner test."};
  int exit=1;string settings="ProjectSettings/ProjectSettings.asset";byte[] original=File.ReadAllBytes(settings);
  try{
   if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))throw new Exception("Installed editor lacks Android Build Support");
   EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
   if(!UnityEngine.Object.FindFirstObjectByType<ValoriaReferenceCleanInteraction>())throw new Exception("Independent scene interaction missing");
   var host=UnityEngine.Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();
   if(!host||!host.BastionOne||!host.BastionTwo||!host.Presenter)throw new Exception("Validated Bastion I-II gameplay/visual bindings missing");
   var rendering=UnityEngine.Object.FindFirstObjectByType<ValoriaReferenceCleanRenderSettings>();if(!rendering||!rendering.scenePipeline)throw new Exception("Isolated scene URP asset missing");
   GraphicsSettings.defaultRenderPipeline=rendering.scenePipeline;QualitySettings.renderPipeline=rendering.scenePipeline;
   PlayerSettings.companyName="Eldoria";PlayerSettings.productName="Eldoria Valoria V5 Test";
   PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,result.package_id);
   PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
   PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
   PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel29;
   PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;
   var keys=JsonUtility.FromJson<KeyReport>(File.ReadAllText("../docs/evidence/valoria-reference-clean/android-existing-key-check.json").TrimStart('\uFEFF'));
   const string originalCertificate="f479ca92ccfa256a991936b23d0153ffad247b352a8a5ed569457db8274443d8";
   var key=System.Array.Find(keys.keys,k=>k.matches_original&&k.certificate_sha256==originalCertificate&&File.Exists(k.path));
   if(key==null||keys.package!=result.package_id||keys.old_version_code!=1)throw new Exception("Original optimized APK signing identity unavailable; refuse replacement app or reinstall");
   PlayerSettings.Android.useCustomKeystore=true;
   PlayerSettings.Android.keystoreName=key.path;PlayerSettings.Android.keystorePass="android";
   PlayerSettings.Android.keyaliasName="androiddebugkey";PlayerSettings.Android.keyaliasPass="android";
   PlayerSettings.bundleVersion="1.4";PlayerSettings.Android.bundleVersionCode=5;
   PlayerSettings.stripEngineCode=true;
   PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android,ManagedStrippingLevel.Medium);
   PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
   PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);
   PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.Vulkan,GraphicsDeviceType.OpenGLES3});
   EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
   const string apk="Builds/ValoriaReferenceClean/Valoria-Reference-Clean.apk";Directory.CreateDirectory(Path.GetDirectoryName(apk));
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=apk,target=BuildTarget.Android,options=BuildOptions.StrictMode});
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
