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
            // Older saved V5 scene may contain missing MonoScript refs from the CI
            // generator. Repair in memory using original authored objects rather
            // than rejecting usable geometry or changing production assets.
            if (anchors.Length < 8 || controls.Length != 1)
            {
                string[] modules = {
                    "wall_straight", "watchtower", "wall_corner", "wall_connections",
                    "stone_path_platform", "cliff_rocks", "defensive_props", "hero_gate_v3"
                };
                var transforms = UnityEngine.Object.FindObjectsByType<Transform>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                int attached = 0;
                foreach (var transform in transforms)
                {
                    var name = transform.gameObject.name.ToLowerInvariant();
                    bool matches = false;
                    foreach (var module in modules)
                        if (name == module || name.StartsWith(module + "_") ||
                            name.StartsWith(module + " (")) { matches = true; break; }
                    if (!matches) continue;
                    var anchor = transform.GetComponent<ValoriaFirstZoneAnchor>();
                    if (anchor == null)
                    {
                        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
                        anchor = transform.gameObject.AddComponent<ValoriaFirstZoneAnchor>();
                    }
                    anchor.ConfigureForIsolatedScene(
                        "valoria.android.v5." + attached.ToString("D2"), name);
                    if (transform.GetComponent<Collider>() == null)
                    {
                        var renderers = transform.GetComponentsInChildren<Renderer>(true);
                        if (renderers.Length > 0)
                        {
                            var bounds = renderers[0].bounds;
                            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                            var col = transform.gameObject.AddComponent<BoxCollider>();
                            col.center = transform.InverseTransformPoint(bounds.center);
                            var scale = transform.lossyScale;
                            col.size = new Vector3(
                                bounds.size.x / Mathf.Max(.001f, Mathf.Abs(scale.x)),
                                bounds.size.y / Mathf.Max(.001f, Mathf.Abs(scale.y)),
                                bounds.size.z / Mathf.Max(.001f, Mathf.Abs(scale.z)));
                            col.isTrigger = true;
                        }
                    }
                    attached++;
                }
                var cameras = UnityEngine.Object.FindObjectsByType<Camera>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                Camera selected = null;
                foreach (var camera in cameras)
                    if (camera.enabled && camera.gameObject.activeInHierarchy)
                    { selected = camera; break; }
                if (selected == null) throw new Exception("V5_ACTIVE_CAMERA_MISSING");
                if (selected.GetComponent<ValoriaFirstZonePreviewControls>() == null)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(selected.gameObject);
                    selected.gameObject.AddComponent<ValoriaFirstZonePreviewControls>();
                }
                Debug.Log("V5_ANDROID_PREVIEW_COMPONENTS_REBUILT attached=" + attached);
            }
            var repairedAnchors = UnityEngine.Object.FindObjectsByType<ValoriaFirstZoneAnchor>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            var repairedControls = UnityEngine.Object.FindObjectsByType<ValoriaFirstZonePreviewControls>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (repairedAnchors.Length < 8 || repairedControls.Length != 1 ||
                repairedControls[0].GetComponent<Camera>() == null)
                throw new Exception("V5_COMPONENT_REPAIR_FAILED: " +
                    repairedAnchors.Length + "/" + repairedControls.Length);
            const string TempScene = "Assets/Eldoria/Experimental/ValoriaFinishV1/ValoriaAndroidV5Temp.unity";
            if (!EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), TempScene, true))
                throw new Exception("V5_ANDROID_TEMP_SCENE_SAVE_FAILED");
            PlayerSettings.companyName = "Eldoria";
            PlayerSettings.productName = "Eldoria Valoria V5 Test";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.eldoria.valoria.v5test");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.useCustomKeystore = false;
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Medium);
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            Directory.CreateDirectory("Builds/Android");
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { TempScene },
                locationPathName = "Builds/Android/Eldoria-Valoria-V5-Test.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            });
            if (result.summary.result != BuildResult.Succeeded) throw new Exception("APK_BUILD_FAILED: " + result.summary.result);
            if (!File.Exists("Builds/Android/Eldoria-Valoria-V5-Test.apk")) throw new Exception("APK_MISSING");
            Debug.Log("VALORIA_V5_ANDROID_APK_PASS anchors=" + repairedAnchors.Length + " bytes=" + new FileInfo("Builds/Android/Eldoria-Valoria-V5-Test.apk").Length);
        }
    }
}
