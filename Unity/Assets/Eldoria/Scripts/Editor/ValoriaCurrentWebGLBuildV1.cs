using System;
using System.IO;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    /// <summary>
    /// Builds the certified Valoria production runtime scene for browser playtesting.
    /// The original production scene is never regenerated or saved. For WebGL only, a temporary
    /// scene copy removes the embedded heavy SHARP assets and uses the exact external PLY variants.
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
            Directory.CreateDirectory(TempFolder);

            var scene = EditorSceneManager.OpenScene(ValoriaScene, OpenSceneMode.Single);
            var visual = UnityEngine.Object.FindFirstObjectByType<ValoriaParcelPresentation>();
            if (visual == null) throw new Exception("Current Valoria production presentation is missing.");
            if (visual.SceneSplats == null) throw new Exception("Current Valoria production SHARP renderer is missing.");

            // Web transport only: keep the certified source assets untouched on disk, but remove
            // their serialized references from the temporary scene so WebGL does not embed ~1.2 GB.
            visual.StateAssets = Array.Empty<Gsplat.GsplatAsset>();
            visual.SceneSplats.GsplatAsset = null;
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

        static void Require(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Required current-production asset missing", path);
        }
    }
}
