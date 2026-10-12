using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    public static class ValoriaV5AndroidCandidate
    {
        const string ScenePath = "Assets/Eldoria/Experimental/ValoriaFinishV1/ValoriaFortification.unity";
        public static void Build()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
                throw new Exception("ANDROID_MODULE_MISSING: Unity 6000.3.23f1 Android Build Support required");
            if (!File.Exists(ScenePath)) throw new FileNotFoundException("Persisted V5 scene missing", ScenePath);
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var anchors = UnityEngine.Object.FindObjectsByType<ValoriaFirstZoneAnchor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var controls = UnityEngine.Object.FindObjectsByType<ValoriaFirstZonePreviewControls>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (anchors.Length < 8 || controls.Length != 1 || controls[0].GetComponent<Camera>() == null)
                throw new Exception("V5 scene missing source anchors or touch camera: " + anchors.Length + "/" + controls.Length);
            PlayerSettings.companyName = "Eldoria";
            PlayerSettings.productName = "Eldoria Valoria V5 Test";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.eldoria.valoria.v5test");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.useCustomKeystore = false;
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            Directory.CreateDirectory("Builds/Android");
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Android/Eldoria-Valoria-V5-Test.apk",
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.StrictMode
            });
            if (result.summary.result != BuildResult.Succeeded) throw new Exception("APK_BUILD_FAILED: " + result.summary.result);
            if (!File.Exists("Builds/Android/Eldoria-Valoria-V5-Test.apk")) throw new Exception("APK_MISSING");
            Debug.Log("VALORIA_V5_ANDROID_APK_PASS anchors=" + anchors.Length + " bytes=" + new FileInfo("Builds/Android/Eldoria-Valoria-V5-Test.apk").Length);
        }
    }
}
