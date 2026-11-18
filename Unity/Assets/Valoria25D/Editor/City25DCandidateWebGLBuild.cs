#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Eldoria.Experimentos.Ciudad2D;
using Eldoria.Valoria25D;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.Valoria25D.Editor
{
    /// <summary>
    /// Exports only the new, actually interactive Premium V6 city scene.
    /// Explicit scene inclusion prevents production city/world scene replacement.
    /// No in-game economy changes, no asset edit, no main branch publication.
    /// </summary>
    public static class City25DCandidateWebGLBuild
    {
        public const string ScenePath =
            "Assets/Valoria25D/Scenes/Valoria25D_Gameplay.unity";
        public const string OutputDir="Valoria25D_Candidate_WebGL";

        [MenuItem("Eldoria/Valoria25D/Exportar candidata WebGL jugable aislada")]
        public static void Build()
        {
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.WebGL)
                throw new InvalidOperationException("Select WebGL build target first");
            var root=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,".."));
            var file=Path.Combine(root,ScenePath);
            if(!File.Exists(file))
                throw new InvalidOperationException("Playable scene missing, run City25DGameplaySceneBuilder first");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(ScenePath,ImportAssetOptions.ForceSynchronousImport);
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)==null)
                throw new InvalidOperationException("Unity did not import isolated playable scene");
            var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            if(!scene.IsValid()||!scene.isLoaded)
                throw new InvalidOperationException("Unable to open playable scene");
            if(UnityEngine.Object.FindObjectsByType<City25DPlayableController>(
                FindObjectsSortMode.None).Count(x=>x.gameObject.scene==scene)!=1)
                throw new InvalidOperationException("Must have exactly one canonical controller");
            if(UnityEngine.Object.FindObjectsByType<City2DStateBridge>(
                FindObjectsSortMode.None).Count(x=>x.gameObject.scene==scene)!=1)
                throw new InvalidOperationException("Must have exactly one read-only live bridge");
            if(UnityEngine.Object.FindObjectsByType<CityBuilding>(
                FindObjectsSortMode.None).Count(x=>x.gameObject.scene==scene)!=6 ||
               UnityEngine.Object.FindObjectsByType<City2DPremiumShadowGate>(
                   FindObjectsSortMode.None).Count(x=>x.gameObject.scene==scene)!=4)
                throw new InvalidOperationException("Premium V6 six plots and four shadows required");
            if(UnityEngine.Object.FindObjectsByType<Camera>(
                FindObjectsSortMode.None).Count(x=>x.gameObject.scene==scene)!=1)
                throw new InvalidOperationException("Scene must use the original single camera");
            if(UnityEngine.Object.FindObjectsByType<City2DPilotHud>(
                FindObjectsInactive.Include,FindObjectsSortMode.None).Length!=0)
                throw new InvalidOperationException("Demo HUD still present");
            var folder=Path.Combine(root,OutputDir);
            if(Directory.Exists(folder))
                throw new InvalidOperationException("Refusing to overwrite another WebGL candidate");
            Directory.CreateDirectory(folder);
            var previousCompression=PlayerSettings.WebGL.compressionFormat;
            var previousDiagnostics=PlayerSettings.WebGL.showDiagnostics;
            try
            {
                // Gzip content-type hosting is error-prone on bare static servers;
                // this QA candidate intentionally packages uncompressed WebGL files.
                PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
                PlayerSettings.WebGL.showDiagnostics=true;
                var options=new BuildPlayerOptions {
                    target=BuildTarget.WebGL,
                    locationPathName=folder,
                    scenes=new[]{ScenePath},
                    options=BuildOptions.None
                };
                BuildReport report=BuildPipeline.BuildPlayer(options);
                if(report==null||report.summary.result!=BuildResult.Succeeded)
                    throw new InvalidOperationException("Real WebGL build failed: "+
                        (report==null?"no BuildReport":report.summary.result.ToString()));
                if(!File.Exists(Path.Combine(folder,"index.html"))||
                   Directory.GetFiles(Path.Combine(folder,"Build"),"*.wasm").Length!=1||
                   Directory.GetFiles(Path.Combine(folder,"Build"),"*.data").Length!=1||
                   Directory.GetFiles(Path.Combine(folder,"Build"),"*.loader.js").Length!=1)
                    throw new InvalidOperationException("Build succeeded but playable WASM payload incomplete");
                using(var sha=SHA256.Create())
                {
                    var sceneHash=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(file)))
                        .Replace("-","").ToLowerInvariant();
                    var json="{"+
                        "\"engine\":\"Unity 6000.3.23f1\","+
                        "\"scene\":\""+ScenePath+"\","+
                        "\"scene_sha256\":\""+sceneHash+"\","+
                        "\"real_gateway\":\"LocalGateway\","+
                        "\"authoritative_save\":\"SliceBoot.OpenCity25DGateway\","+
                        "\"builds_and_recruits_are_real\":true,"+
                        "\"no_economy_or_combat_changes\":true,"+
                        "\"separate_main_and_art\":true,"+
                        "\"world_4x_rebuild\":false,"+
                        "\"wasm_distribution_ready\":true,"+
                        "\"iphone_hardware_qa\":\"PENDING\","+
                        "\"uncompressed_webgl_files\":true,"+
                        "\"total_build_bytes\":"+report.summary.totalSize+"}\n";
                    File.WriteAllText(Path.Combine(folder,"candidate_manifest.json"),
                        json,new UTF8Encoding(false));
                }
                Debug.Log("VALORIA25D_REAL_WEBGL_CANDIDATE_BUILD_OK "+
                          "scene="+ScenePath+" runtime_gateway=LocalGateway");
            }
            finally
            {
                PlayerSettings.WebGL.compressionFormat=previousCompression;
                PlayerSettings.WebGL.showDiagnostics=previousDiagnostics;
            }
        }
    }
}
#endif
