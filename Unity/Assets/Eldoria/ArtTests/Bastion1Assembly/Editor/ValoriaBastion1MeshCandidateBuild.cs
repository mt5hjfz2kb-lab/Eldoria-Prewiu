using System;
using System.IO;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.EditorTools
{
    public static class ValoriaBastion1MeshCandidateBuild
    {
        const string NativeScene="Assets/Eldoria/ProductionSlice/Runtime/Valoria.unity";
        const string WebScene="Assets/Eldoria/Bastion1MeshWebGL/ValoriaWebGL.unity";
        const string Frontier="Assets/Eldoria/Scenes/Frontier.unity";
        const string Output="Builds/Bastion1MeshCandidateWebGL";

        [MenuItem("Eldoria/Build Bastion I authored mesh candidate WebGL")]
        public static void BuildWebGL()
        {
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(NativeScene)==null)
                throw new Exception("Generate the authored Valoria scene first");
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(Frontier)==null)
                throw new Exception("World Region 1 scene missing");
            var scene=EditorSceneManager.OpenScene(NativeScene,OpenSceneMode.Single);
            var presentation=UnityEngine.Object.FindFirstObjectByType<ValoriaParcelPresentation>();
            if(presentation==null||!presentation.MeshMode||
               presentation.SceneSplats!=null||presentation.ProductionCamera==null)
                throw new Exception("Valoria candidate must be an authored mesh player scene");
            if(presentation.Bindings.Length<2||
               presentation.Bindings[0].Ground==null||
               presentation.Bindings[0].Construction==null||
               presentation.Bindings[0].Built==null||
               presentation.Bindings[1].Built==null)
                throw new Exception("Authored construction family is incomplete");
            if(UnityEngine.Object.FindObjectsByType<Gsplat.GsplatRenderer>(
                   FindObjectsInactive.Include,FindObjectsSortMode.None).Length!=0)
                throw new Exception("SHARP renderer leaked into mesh candidate");

            Directory.CreateDirectory(Path.GetDirectoryName(WebScene));
            if(!EditorSceneManager.SaveScene(scene,WebScene,true))
                throw new Exception("WebGL player scene copy failed");
            AssetDatabase.Refresh();
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback=true;
            if(Directory.Exists(Output))Directory.Delete(Output,true);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes=new[]{WebScene,Frontier},
                locationPathName=Output,
                target=BuildTarget.WebGL,
                options=BuildOptions.StrictMode
            });
            if(report.summary.result!=BuildResult.Succeeded)
                throw new Exception("Bastion I candidate WebGL build failed: "+report.summary.result);
            if(!File.Exists(Path.Combine(Output,"index.html")))
                throw new Exception("No WebGL index.html");
            long bytes=0;
            foreach(var file in new DirectoryInfo(Output).GetFiles("*",SearchOption.AllDirectories))
                bytes+=file.Length;
            Debug.Log("BASTION1_MESH_WEBGL_CANDIDATE_PASS output_bytes="+bytes+
                " engine_bytes="+report.summary.totalSize);
        }
    }
}
