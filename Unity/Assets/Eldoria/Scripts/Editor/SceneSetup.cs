using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Eldoria.EditorTools
{
    // Regenerate proper Unity YAML after first editor import; scene geometry is intentionally generated at runtime.
    public static class SceneSetup
    {
        const string Root="Assets/Eldoria/Scenes/";
        [MenuItem("Eldoria/Regenerate slice scenes and build settings")]
        public static void Regenerate()
        {
            Directory.CreateDirectory(Root);
            foreach (string name in new[] { "Bootstrap", "Valoria", "Frontier" })
            {
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                new GameObject("Eldoria " + name + " scene marker");
                EditorSceneManager.SaveScene(scene,Root+name+".unity");
            }
            EditorBuildSettings.scenes=new[] {
                new EditorBuildSettingsScene(Root+"Bootstrap.unity",true),
                new EditorBuildSettingsScene(Root+"Valoria.unity",true),
                new EditorBuildSettingsScene(Root+"Frontier.unity",true) };
            SetupRenderPipeline();
            EditorSceneManager.OpenScene(Root+"Bootstrap.unity");
            AssetDatabase.SaveAssets();
        }
        [MenuItem("Eldoria/Configure URP for this project")]
        public static void SetupRenderPipeline()
        {
            const string rendererPath="Assets/Eldoria/Content/EldoriaForwardRenderer.asset";
            const string pipelinePath="Assets/Eldoria/Content/EldoriaMobileURP.asset";
            Directory.CreateDirectory("Assets/Eldoria/Content");
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if(renderer==null){renderer=ScriptableObject.CreateInstance<UniversalRendererData>();AssetDatabase.CreateAsset(renderer,rendererPath);}
            var pipeline=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if(pipeline==null)
            {
                pipeline=ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
                var serialized=new SerializedObject(pipeline);
                var list=serialized.FindProperty("m_RendererDataList");
                if(list==null)
                { Debug.LogError("URP internal renderer property changed: create URP Pipeline Asset via Assets > Create and assign it manually."); return; }
                list.arraySize=1;list.GetArrayElementAtIndex(0).objectReferenceValue=renderer;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(pipeline,pipelinePath);
            }
            GraphicsSettings.defaultRenderPipeline=pipeline;
            QualitySettings.renderPipeline=pipeline;
            EditorUtility.SetDirty(pipeline);
        }
        public static void BuildLinux()
        {
            Regenerate();
            Directory.CreateDirectory("Builds/Linux");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                scenes=new[] {Root+"Bootstrap.unity",Root+"Valoria.unity",Root+"Frontier.unity"},
                locationPathName="Builds/Linux/Eldoria.x86_64",target=BuildTarget.StandaloneLinux64,
                options=BuildOptions.None});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new System.Exception("Unity build failed: "+report.summary.result);
        }
    }
}
