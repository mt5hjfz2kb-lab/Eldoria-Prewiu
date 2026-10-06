using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.Presentation;
using Gsplat;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.EditorTools
{
    /// <summary>
    /// Builds the certified Valoria production runtime scene for browser playtesting.
    /// The original production scene is never regenerated or saved. For WebGL only, a temporary
    /// scene copy strips every serialized GsplatAsset reference and loads the exact external PLY variants at runtime.
    /// </summary>
    public static class ValoriaCurrentWebGLBuildV1
    {
        const string ValoriaScene = "Assets/Eldoria/ProductionSlice/Runtime/Valoria.unity";
        const string TempFolder = "Assets/Eldoria/WebGLTemp";
        const string TempScene = TempFolder + "/ValoriaWebGL.unity";
        const string Output = "Builds/WebGL";

        [MenuItem("Eldoria/Build current Valoria production WebGL")]
        public static void Build()
        {
            Require(ValoriaScene);
            for (var i = 0; i < 4; i++)
                Require($"Assets/Eldoria/ProductionSlice/Runtime/state-{i}.asset");

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;

            if (Directory.Exists(Output))
                Directory.Delete(Output, true);
            Directory.CreateDirectory(Output);

            if (AssetDatabase.IsValidFolder(TempFolder))
                AssetDatabase.DeleteAsset(TempFolder);
            Directory.CreateDirectory(TempFolder);
            AssetDatabase.Refresh();

            var scene = EditorSceneManager.OpenScene(ValoriaScene, OpenSceneMode.Single);
            var visual = UnityEngine.Object.FindFirstObjectByType<ValoriaParcelPresentation>();
            if (visual == null) throw new Exception("Current Valoria production presentation is missing.");
            if (visual.SceneSplats == null) throw new Exception("Current Valoria production SHARP renderer is missing.");

            // Web transport only. The certified production scene/assets stay untouched.
            // Clear the known presentation references first, then strip *all* serialized
            // GsplatAsset references from the temporary scene so no hidden/prefab reference
            // can silently drag the ~GB production assets into the WebGL player.
            foreach (var presentation in UnityEngine.Object.FindObjectsByType<ValoriaParcelPresentation>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
                presentation.StateAssets = Array.Empty<GsplatAsset>();

            foreach (var renderer in UnityEngine.Object.FindObjectsByType<GsplatRenderer>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
                renderer.GsplatAsset = null;

            var stripped = StripSerializedGsplatReferences(scene);

            var loader = visual.GetComponent<ValoriaWebGLSplatStateLoader>();
            if (loader == null) loader = visual.gameObject.AddComponent<ValoriaWebGLSplatStateLoader>();
            loader.Presentation = visual;
            loader.Renderer = visual.SceneSplats;
            loader.FilePrefix = "splat-state-";

            EditorUtility.SetDirty(visual);
            EditorUtility.SetDirty(loader);
            EditorUtility.SetDirty(visual.SceneSplats);

            if (!EditorSceneManager.SaveScene(scene, TempScene, true))
                throw new Exception("Could not save temporary Valoria WebGL scene.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AssertNoEmbeddedGsplatDependencies();
            Debug.Log($"VALORIA_WEBGL_SPLAT_STRIP_PASS stripped_refs={stripped}");

            try
            {
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { TempScene },
                    locationPathName = Output,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None
                });

                if (report.summary.result != BuildResult.Succeeded)
                    throw new Exception("Current Valoria WebGL build failed: " + report.summary.result);

                if (!File.Exists(Path.Combine(Output, "index.html")))
                    throw new Exception("Current Valoria WebGL build did not produce index.html.");

                Debug.Log($"VALORIA_CURRENT_WEBGL_PASS size={report.summary.totalSize} bytes");
            }
            finally
            {
                AssetDatabase.DeleteAsset(TempFolder);
                AssetDatabase.Refresh();
            }
        }

        static int StripSerializedGsplatReferences(Scene scene)
        {
            var stripped = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var component in root.GetComponentsInChildren<Component>(true))
                {
                    if (component == null) continue;
                    var serialized = new SerializedObject(component);
                    var property = serialized.GetIterator();
                    var changed = false;

                    while (property.Next(true))
                    {
                        if (property.propertyType != SerializedPropertyType.ObjectReference)
                            continue;
                        if (property.objectReferenceValue is not GsplatAsset)
                            continue;

                        property.objectReferenceValue = null;
                        stripped++;
                        changed = true;
                    }

                    if (changed)
                    {
                        serialized.ApplyModifiedPropertiesWithoutUndo();
                        EditorUtility.SetDirty(component);
                    }
                }
            }
            return stripped;
        }

        static void AssertNoEmbeddedGsplatDependencies()
        {
            var bad = new List<string>();
            foreach (var dependency in AssetDatabase.GetDependencies(TempScene, true))
            {
                if (dependency == TempScene) continue;
                if (AssetDatabase.LoadAssetAtPath<GsplatAsset>(dependency) != null)
                    bad.Add(dependency);
            }

            if (bad.Count > 0)
                throw new Exception(
                    "Temporary WebGL scene still embeds GsplatAsset dependencies:\n" +
                    string.Join("\n", bad));

            Debug.Log("VALORIA_WEBGL_EXTERNAL_SPLAT_DEPENDENCY_PASS");
        }

        static void Require(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Required current-production asset missing", path);
        }
    }
}
