using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Eldoria.EditorTools
{
    /// <summary>
    /// Builds the already-certified Valoria production runtime scene for browser playtesting.
    /// It deliberately does NOT call SceneSetup.Regenerate(), because that would replace the
    /// production SHARP scene with the legacy generated slice.
    /// </summary>
    public static class ValoriaCurrentWebGLBuildV1
    {
        const string ValoriaScene = "Assets/Eldoria/ProductionSlice/Runtime/Valoria.unity";
        const string FrontierScene = "Assets/Eldoria/Scenes/Frontier.unity";
        const string Output = "Builds/WebGL";

        [MenuItem("Eldoria/Build current Valoria production WebGL")]
        public static void Build()
        {
            Require(ValoriaScene);
            Require(FrontierScene);
            for (var i = 0; i < 4; i++)
                Require($"Assets/Eldoria/ProductionSlice/Runtime/state-{i}.asset");

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;

            if (Directory.Exists(Output))
                Directory.Delete(Output, true);
            Directory.CreateDirectory(Output);

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ValoriaScene, FrontierScene },
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

        static void Require(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Required current-production asset missing", path);
        }
    }
}
