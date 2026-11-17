#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.Experimentos.Ciudad2D.Editor
{
    /// <summary>
    /// Unity 6000.3 test-only WebGL export. Its sole input is the SHA-locked
    /// archived PremiumV6 Unity scene, never Valoria production or Build Settings.
    /// No merge, no beta deployment, no changes to authoritative game data.
    /// </summary>
    public static class City2DWebPerfBuild
    {
        const string Folder="Assets/Experimentos/Ciudad2D/";
        const string Source=Folder+"QA/ArchivedScenes/Experimento_Ciudad2D_PremiumV6_44133747143.unity";
        const string SourceSha="aa566bf6a075868ad852ac865aa8a8cb867d1ade601ac3dd74e8e091f44f0b06";
        const string TestScene=Folder+"QA/WebPerf/Experimento_Ciudad2D_WebPerfV6.unity";
        const string Output="Ciudad2D_WebPerfV6_Build";

        [MenuItem("Eldoria/Experimentos/Ciudad2D/Exportar WebGL aislado de rendimiento V6")]
        public static void Build()
        {
            if (EditorUserBuildSettings.activeBuildTarget!=BuildTarget.WebGL)
                throw new InvalidOperationException("Invoke the Editor with -buildTarget WebGL before Build");
            string path=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..",Source));
            if(!File.Exists(path)||Sha(File.ReadAllBytes(path))!=SourceSha)
                throw new InvalidOperationException("Archived original PremiumV6 scene has invalid SHA");
            Directory.CreateDirectory(Path.GetFullPath(
                Path.Combine(UnityEngine.Application.dataPath,"..",Folder+"QA/WebPerf")));
            // The first WebGL trial proved the exact SHA is present but a
            // just-imported archived scene may not yet be registered by Unity.
            // Perform synchronous scene import before opening it.
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(Source,
                ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
            string sceneActuallyOpened=Source;
            Scene original=default;
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Source)!=null)
            {
                try { original=EditorSceneManager.OpenScene(Source,OpenSceneMode.Single); }
                catch(ArgumentException ex)
                {
                    Debug.LogWarning("[Ciudad2D WebGL] Archived Unity scene could not open: "+
                        ex.Message+"; regenerating read-only V6 in scratch");
                }
            }
            if(!original.IsValid()||!original.isLoaded)
            {
                // A fallback from the same premium source/art, never an unrelated scene.
                // The known passing V6 builder creates only sandbox scenes and
                // has no data/gameplay/save actions.
                City2DPremiumPreviewV6.Build();
                sceneActuallyOpened=City2DPremiumPreviewV6.PremiumScene;
                AssetDatabase.ImportAsset(sceneActuallyOpened,
                    ImportAssetOptions.ForceSynchronousImport);
                original=EditorSceneManager.OpenScene(sceneActuallyOpened,OpenSceneMode.Single);
                Debug.Log("[Ciudad2D WebGL] Runtime source regenerated from immutable V6 assets");
            }
            if(!original.IsValid()||!original.isLoaded)
                throw new InvalidOperationException("Neither archived nor rebuilt V6 scene could load");
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(TestScene)!=null)
                throw new InvalidOperationException("Do not overwrite an earlier separate WebPerf scene");
            if(!EditorSceneManager.SaveScene(original,TestScene,true))
                throw new InvalidOperationException("Could not create test-only scene");
            var trial=EditorSceneManager.OpenScene(TestScene,OpenSceneMode.Single);
            if(!trial.IsValid()||!trial.isLoaded)
                throw new InvalidOperationException("Trial scene did not load");
            var bridge=UnityEngine.Object.FindFirstObjectByType<City2DStateBridge>();
            if(bridge==null||bridge.IsLiveBound)
                throw new InvalidOperationException("Must not bind authoritative gameplay");
            var buildings=UnityEngine.Object.FindObjectsByType<CityBuilding>(FindObjectsSortMode.None)
                .Where(x=>x.gameObject.scene==trial).ToArray();
            var shadows=UnityEngine.Object.FindObjectsByType<City2DPremiumShadowGate>(
                FindObjectsSortMode.None).Where(x=>x.gameObject.scene==trial).ToArray();
            if(buildings.Length!=6 || shadows.Length!=4 ||
               UnityEngine.Camera.main==null ||
               buildings.Count(x=>x.BuildingId=="cantera")!=1)
                throw new InvalidOperationException("Not the verified V6 sandbox scene");
            var runtime=new GameObject("WEBPERF_V6_SoloPresentacion");
            runtime.AddComponent<City2DWebPerfProbe>();
            if(!EditorSceneManager.SaveScene(trial,TestScene))
                throw new InvalidOperationException("Cannot save isolated runtime probe");

            string output=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..",Output));
            if(Directory.Exists(output))
                throw new InvalidOperationException("Refusing to overwrite a prior WebPerf build");
            Directory.CreateDirectory(output);
            var previousCompression=PlayerSettings.WebGL.compressionFormat;
            bool previousDiagnostics=PlayerSettings.WebGL.showDiagnostics;
            try
            {
                // Disabled compression permits an ordinary static HTTP server,
                // but makes the initial load larger than a distribution build.
                // It never changes settings in main: CI builds a disposable SHA checkout.
                PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
                PlayerSettings.WebGL.showDiagnostics=true;
                var options=new BuildPlayerOptions
                {
                    scenes=new[]{TestScene},
                    locationPathName=output,
                    target=BuildTarget.WebGL,
                    options=BuildOptions.None
                };
                BuildReport report=BuildPipeline.BuildPlayer(options);
                if(report==null || report.summary.result!=BuildResult.Succeeded)
                    throw new InvalidOperationException("Unity WebGL BuildPipeline failed: "+
                        (report==null?"null report":report.summary.result.ToString()));
                if(!File.Exists(Path.Combine(output,"index.html")) ||
                   !Directory.Exists(Path.Combine(output,"Build")))
                    throw new InvalidOperationException("No playable Unity WebGL distribution");
                string manifest="{"+
                    "\"engine\":\"Unity 6000.3\","+
                    "\"source_Unity_scene\":\""+Source+"\","+
                    "\"runtime_scene_generated_from\":\""+sceneActuallyOpened+"\","+
                    "\"source_Unity_scene_sha256\":\""+SourceSha+"\","+
                    "\"trial_scene\":\""+TestScene+"\","+
                    "\"target\":\"WebGL\","+
                    "\"playable_city_trial_only\":true,"+
                    "\"native_FPS_measurement_in_Unity\":true,"+
                    "\"unity_web_diagnostics_enabled\":true,"+
                    "\"initial_loading_compressed\":false,"+
                    "\"source_build_settings_modified\":false,"+
                    "\"main_modified\":false,"+
                    "\"gameplay_modified\":false,"+
                    "\"quarry_built_only_visual_fixture\":true,"+
                    "\"target_hardware_iphone_test\":\"PENDING\","+
                    "\"unity_report_size_bytes\":"+report.summary.totalSize+
                    "}\n";
                File.WriteAllText(Path.Combine(output,"trial_manifest.json"),manifest,
                    new UTF8Encoding(false));
                Debug.Log("CIUDAD2D_WEBPERF_V6_WEBGL_BUILD_PASS only_scene="+TestScene+
                    " playable=true iPhone_physical=PENDING output="+output);
            }
            finally
            {
                PlayerSettings.WebGL.compressionFormat=previousCompression;
                PlayerSettings.WebGL.showDiagnostics=previousDiagnostics;
            }
        }
        static string Sha(byte[] raw)
        {
            using(var hash=SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(raw))
                    .Replace("-","").ToLowerInvariant();
        }
    }
}
#endif
