using System;
using System.IO;
using Eldoria;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.EditorTools
{
    /// <summary>
    /// Candidate WebGL build of the already rendered/persisted Valoria 3D sector.
    /// It is not the shipping Bastion I-II game. Never edits main or production scenes.
    /// </summary>
    public static class ValoriaFirstZoneCandidateWebGL
    {
        const string ScenePath =
            "Assets/Eldoria/Experimental/ValoriaFinishV1/ValoriaFortification.unity";
        const string Output="Builds/ValoriaFirstZoneWebGL";
        public static void Build()
        {
            if(!File.Exists("Assets/../Unity/Assets/Eldoria/Experimental/ValoriaFinishV1/ValoriaFortification.unity") &&
               !File.Exists(Path.Combine(Application.dataPath,
                    "Eldoria/Experimental/ValoriaFinishV1/ValoriaFortification.unity")))
                throw new Exception("Persisted ValoriaFortification scene unavailable");
            var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var anchors=UnityEngine.Object.FindObjectsByType<ValoriaFirstZoneAnchor>(
                FindObjectsInactive.Include,FindObjectsSortMode.None);
            if(anchors.Length<8)throw new Exception("Isolated first zone must contain 8+ source selection anchors");
            var cameras=UnityEngine.Object.FindObjectsByType<ValoriaFirstZonePreviewControls>(
                FindObjectsInactive.Include,FindObjectsSortMode.None);
            if(cameras.Length!=1)throw new Exception("Touch preview controls missing or duplicated");
            var cam=cameras[0].GetComponent<Camera>();
            if(cam==null||!cam.enabled)throw new Exception("First-zone camera not ready");
            if(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath)==null)
                throw new Exception("Persisted scene has missing asset metadata");
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback=true;
            Directory.CreateDirectory("Builds");
            if(Directory.Exists(Output))Directory.Delete(Output,true);
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=new[]{ScenePath},
                locationPathName=Output,
                target=BuildTarget.WebGL,
                options=BuildOptions.StrictMode
            });
            if(result.summary.result!=BuildResult.Succeeded)
                throw new Exception("First-zone candidate build failed: "+result.summary.result);
            if(!File.Exists(Path.Combine(Output,"index.html")))
                throw new Exception("No candidate WebGL index.html");
            Debug.Log("ELDORIA_VALORIA_FIRST_ZONE_WEBGL_CANDIDATE_PASS anchors="+anchors.Length+
                " size="+result.summary.totalSize);
        }
    }
}
