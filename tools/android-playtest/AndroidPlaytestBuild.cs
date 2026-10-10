using System;
using System.IO;
using System.Collections.Generic;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools
{
    // Staged into the Editor folder by Android CI only; canonical Unity source remains unchanged.
    public static class AndroidPlaytestBuild
    {
        const string City = "Assets/Eldoria/ProductionSlice/Runtime/Valoria.unity";
        const string World = "Assets/Eldoria/Scenes/Frontier.unity";
        const string TempFolder = "Assets/Eldoria/AndroidPlaytestTemp";
        const string TempCity = TempFolder + "/Valoria.unity";
        public static void Build()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
                throw new Exception("Android Build Support is missing from Unity 6000.3.23f1.");
            foreach (var path in new[] { City, World })
                if (!File.Exists(path)) throw new FileNotFoundException("Native scene missing", path);
            EditorSceneManager.OpenScene(City, OpenSceneMode.Single);
            var visual = UnityEngine.Object.FindFirstObjectByType<ValoriaParcelPresentation>();
            if (visual == null || visual.SceneSplats == null || visual.StateAssets == null || visual.StateAssets.Length != 4)
                throw new Exception("Native production scene must retain its real SHARP renderer and four parcel variants.");
            foreach (var state in visual.StateAssets)
                if (state == null) throw new Exception("Native parcel asset missing.");

            // Older production artifacts contain scene-local MonoScripts. Rebind the
            // same hotspot identities in a temporary copy, as the browser builder does.
            var ids = new Dictionary<string, string>();
            foreach (var hotspot in UnityEngine.Object.FindObjectsByType<WorldHotspot>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var path = TransformPath(hotspot.transform);
                var id = hotspot.Id;
                if (string.IsNullOrEmpty(id)) throw new Exception("Native hotspot missing Id: " + path);
                ids.Add(path, id);
                var go = hotspot.gameObject;
                var enabled = hotspot.enabled;
                UnityEngine.Object.DestroyImmediate(hotspot);
                var stable = go.AddComponent<WorldHotspot>();
                stable.Id = id;
                stable.enabled = enabled;
            }
            if (AssetDatabase.IsValidFolder(TempFolder)) AssetDatabase.DeleteAsset(TempFolder);
            Directory.CreateDirectory(TempFolder);
            AssetDatabase.Refresh();
            if (!EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), TempCity, true))
                throw new Exception("Cannot save native temporary scene.");
            EditorSceneManager.OpenScene(TempCity, OpenSceneMode.Single);
            var count = 0;
            foreach (var hotspot in UnityEngine.Object.FindObjectsByType<WorldHotspot>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (!ids.TryGetValue(TransformPath(hotspot.transform), out var id) || id != hotspot.Id ||
                    AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(hotspot)) != "Assets/Eldoria/Scripts/Presentation/WorldHotspot.cs")
                    throw new Exception("Native hotspot serialization failed.");
                count++;
            }
            if (count != ids.Count) throw new Exception("Native hotspot count changed.");

            PlayerSettings.companyName = "Eldoria";
            PlayerSettings.productName = "Eldoria Playtest";
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android, "com.eldoria.playtest");
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.useCustomKeystore = false;
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            Directory.CreateDirectory("Builds/Android");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { TempCity, World },
                locationPathName = "Builds/Android/Eldoria-Playtest.apk",
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.StrictMode
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("Android build failed: " + report.summary.result);
            if (!File.Exists("Builds/Android/Eldoria-Playtest.apk"))
                throw new Exception("Android build reported success without an APK.");
            Debug.Log("ANDROID_APK_BUILD_PASS bytes=" + report.summary.totalSize + "; device gameplay/visual/performance acceptance still required");
        }
        static string TransformPath(Transform t) => t.parent == null ? t.name : TransformPath(t.parent) + "/" + t.name;
    }
}
