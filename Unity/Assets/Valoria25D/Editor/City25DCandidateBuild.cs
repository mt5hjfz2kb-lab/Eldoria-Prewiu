#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.Valoria25D.Editor
{
    /// <summary>
    /// Candidate WebGL output with only the integrated City25D gameplay scene.
    /// It is NOT an Eldoria public beta, release or main branch mutation.
    /// </summary>
    public static class City25DCandidateBuild
    {
        public const string Folder="Valoria25D_CandidateWebGL";
        public static void WebGL()
        {
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.WebGL)
                throw new InvalidOperationException("Invoke with -buildTarget WebGL");
            string scene=City25DGameplaySceneBuilder.Playable;
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(scene,ImportAssetOptions.ForceSynchronousImport);
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(scene)==null)
                throw new InvalidOperationException("Integrated City25D scene absent");
            var opened=EditorSceneManager.OpenScene(scene,OpenSceneMode.Single);
            if(!opened.IsValid()||!opened.isLoaded)
                throw new InvalidOperationException("Candidate scene invalid");
            if(UnityEngine.Object.FindObjectsByType<City25DPlayableController>(
                FindObjectsSortMode.None).Length!=1)
                throw new InvalidOperationException("Missing unique gameplay root");
            if(UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(
                FindObjectsSortMode.None).Length!=1)
                throw new InvalidOperationException("Duplicated input EventSystem");
            var output=Path.GetFullPath(Path.Combine(
                UnityEngine.Application.dataPath,"..",Folder));
            if(Directory.Exists(output))
                throw new InvalidOperationException("Previous candidate build cannot be overwritten");
            var prev=PlayerSettings.WebGL.compressionFormat;
            bool prevDiagnostics=PlayerSettings.WebGL.showDiagnostics;
            try
            {
                // Candidate-only: plain wasm/data for local HTTP and private QA.
                PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
                PlayerSettings.WebGL.showDiagnostics=true;
                var options=new BuildPlayerOptions {
                    scenes=new[]{scene},
                    locationPathName=output,
                    target=BuildTarget.WebGL,
                    options=BuildOptions.None
                };
                var report=BuildPipeline.BuildPlayer(options);
                if(report==null||report.summary.result!=BuildResult.Succeeded)
                    throw new InvalidOperationException("Real Unity WebGL build failure: "+
                        (report==null?"no report":report.summary.result.ToString()));
                var build=Path.Combine(output,"Build");
                if(!File.Exists(Path.Combine(output,"index.html")) ||
                   !Directory.Exists(build) ||
                   !Directory.GetFiles(build,"*.wasm").Any() ||
                   !Directory.GetFiles(build,"*.data").Any() ||
                   !Directory.GetFiles(build,"*.loader.js").Any())
                    throw new InvalidOperationException("Incomplete real Unity WebGL candidate");
                File.WriteAllText(Path.Combine(output,"integration_manifest.json"),
                    "{"+
                    "\"engine\":\"Unity 6000.3.23f1\","+
                    "\"main_base\":\"904ff183ffb9053f68b9c0f359b2e9d3d40ce805\","+
                    "\"source_v6_scene_sha256\":\"aa566bf6a075868ad852ac865aa8a8cb867d1ade601ac3dd74e8e091f44f0b06\","+
                    "\"scene\":\""+scene+"\","+
                    "\"target\":\"WebGL\","+
                    "\"canonical_LocalGateway\":true,"+
                    "\"canonical_save_key\":true,"+
                    "\"no_main_modification\":true,"+
                    "\"world_reconstructed\":false,"+
                    "\"physical_iPhone_test\":\"PENDING\","+
                    "\"art_approval\":\"V6 candidate unchanged\","+
                    "\"build_size_bytes\":"+report.summary.totalSize+
                    "}\n",new UTF8Encoding(false));
                Debug.Log("VALORIA25D_CANDIDATE_WEBGL_BUILD_SUCCESS"+
                    " from_main=true real_gateway=true no_public_beta=true");
            }
            finally
            {
                PlayerSettings.WebGL.compressionFormat=prev;
                PlayerSettings.WebGL.showDiagnostics=prevDiagnostics;
            }
        }
    }
}
#endif
