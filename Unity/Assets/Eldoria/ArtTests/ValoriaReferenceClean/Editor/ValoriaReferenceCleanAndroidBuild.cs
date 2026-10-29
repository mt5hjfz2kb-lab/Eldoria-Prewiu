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

namespace Eldoria.ReferenceClean.Editor {
// Reuses the project's proven Android playtest build procedure, scoped to this scene.
public static class ValoriaReferenceCleanAndroidBuild {
 const string ScenePath="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
 const string Evidence="../docs/evidence/valoria-reference-clean/android-build.json";
 [Serializable]class Key{public string path,certificate_sha256;public bool matches_original;}
 [Serializable]class KeyReport{public Key[] keys;public string package;public int old_version_code;}
 [Serializable]class Request{public bool content_updates;public int revision,android_version_code;public string android_version_name;}
 [Serializable]class Result { public string source_sha,status,apk_sha256,package_id,graphics_apis,note,scene;public long apk_bytes;public int errors,warnings;public bool physical_device_tested; }
 public static void Build(){
  var request=JsonUtility.FromJson<Request>(File.ReadAllText("../tools/valoria-reference-clean/request.json"));
  var result=new Result{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),package_id="com.eldoria.valoria.v5test",graphics_apis="Vulkan, OpenGLES3",scene=ScenePath,physical_device_tested=false,note="Owner update: same package and verified existing signing certificate, one current Valoria city/world/Royal Gold scene, source-only GLBs optimized. No forced data reset or uninstall. Physical Android install/performance has not been observed."};
  var libraries=new Dictionary<string,byte[]>();
  int exit=1;string settings="ProjectSettings/ProjectSettings.asset";byte[] original=File.ReadAllBytes(settings);
  try{
   if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))throw new Exception("Installed editor lacks Android Build Support");
   EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
   if(!UnityEngine.Object.FindFirstObjectByType<ValoriaReferenceCleanInteraction>())throw new Exception("Integrated scene player camera/input missing");
   var host=UnityEngine.Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();
   var integrated=UnityEngine.Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
   if(!host||!host.BastionOne||!host.BastionTwo||!host.Presenter||!integrated||!integrated.HudPrefab||!integrated.WorldRoot||!integrated.WorldCamera)throw new Exception("Current integrated Bastion I-II city/world/RoyalGold runtime bindings missing");
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
   PlayerSettings.bundleVersion=request.android_version_name??"1.4";PlayerSettings.Android.bundleVersionCode=request.android_version_code>0?request.android_version_code:5;
   if(request.content_updates&&PlayerSettings.Android.bundleVersionCode<6)throw new Exception("Content bootstrap requires versionCode6 or higher");
   PlayerSettings.Android.forceInternetPermission=true;
   PlayerSettings.stripEngineCode=true;
   PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android,ManagedStrippingLevel.Medium);
   PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;
   PlayerSettings.allowedAutorotateToPortrait=true;
   PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
   PlayerSettings.allowedAutorotateToLandscapeLeft=true;
   PlayerSettings.allowedAutorotateToLandscapeRight=true;
   PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);
   PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.Vulkan,GraphicsDeviceType.OpenGLES3});
   EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
   const string apk="Builds/ValoriaReferenceClean/Valoria-Reference-Clean.apk";Directory.CreateDirectory(Path.GetDirectoryName(apk));
   var scenes=new[]{ScenePath};string bundleManifest=null;
   if(request.content_updates)throw new InvalidOperationException(
    "The old isolated-scene remote bootstrap is not valid for the integrated city/world scene. Ship a signed full-scene APK without resetting data.");
   // Reuse the integrated WebGL packager's proven exclusion of the unused
   // legacy source catalog. Scene dependencies are retained; source assets
   // and the catalog's exact bytes are restored in finally, even on failure.
   var dependencies=new HashSet<string>(AssetDatabase.GetDependencies(ScenePath,true));
   foreach(var path in AssetDatabase.FindAssets("t:ValoriaExternalAssetLibrary").Select(AssetDatabase.GUIDToAssetPath)){
    if(dependencies.Contains(path))throw new Exception("Current scene unexpectedly depends on legacy resource library: "+path);
    libraries[path]=File.ReadAllBytes(path);
    var asset=AssetDatabase.LoadMainAssetAtPath(path);var serialized=new SerializedObject(asset);var it=serialized.GetIterator();
    while(it.Next(true))if(it.propertyType==SerializedPropertyType.ObjectReference&&it.name!="m_Script")it.objectReferenceValue=null;
    serialized.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(asset);
   }
   AssetDatabase.SaveAssets();
   File.WriteAllLines("../docs/evidence/valoria-reference-clean/android-scene-dependencies.txt",dependencies.OrderBy(x=>x));
   var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=apk,target=BuildTarget.Android,options=BuildOptions.StrictMode,assetBundleManifestPath=bundleManifest});
   result.errors=(int)report.summary.totalErrors;result.warnings=(int)report.summary.totalWarnings;
   if(report.summary.result!=BuildResult.Succeeded||!File.Exists(apk))throw new Exception("Android build failed: "+report.summary.result);
   result.apk_bytes=new FileInfo(apk).Length;
   if(result.apk_bytes>450L*1024*1024)throw new Exception("APK exceeds the installed updater 450 MiB limit: "+result.apk_bytes);
   using(var sha=System.Security.Cryptography.SHA256.Create())using(var stream=File.OpenRead(apk))result.apk_sha256=BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
   result.status="APK_COMPILED_NOT_DEVICE_VALIDATED";exit=0;Debug.Log("VALORIA_REFERENCE_CLEAN_ANDROID_COMPLETE bytes="+result.apk_bytes);
  }catch(Exception e){result.status="ANDROID_BUILD_FAILED";result.note=e.ToString();Debug.LogException(e);}
  finally{foreach(var pair in libraries)File.WriteAllBytes(pair.Key,pair.Value);File.WriteAllText(Evidence,JsonUtility.ToJson(result,true));File.WriteAllBytes(settings,original);}
  EditorApplication.Exit(exit);
 }
}
}

