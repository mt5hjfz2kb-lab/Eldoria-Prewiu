using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.ArtReboot.Editor
{
    [InitializeOnLoad]
    public sealed class RebootSandboxGuard : IProcessSceneWithReport
    {
        public const string ScenePath = "Assets/Art_Reboot/Phase1/Scenes/Eldoria_Visual_Sandbox.unity";
        static RebootSandboxGuard()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.ExitingEditMode) return;
                for (int i = 0; i < SceneManager.sceneCount; i++)
                    if (SceneManager.GetSceneAt(i).path == ScenePath)
                    {
                        EditorApplication.isPlaying = false;
                        Debug.LogWarning("REBOOT: visual-only pilot. Use Art Reboot/Build and capture phase 1. Play is blocked to preserve saves and avoid V4.1 bootstrap.");
                        break;
                    }
            };
        }
        public int callbackOrder => -10000;
        public void OnProcessScene(Scene scene, BuildReport report)
        {
            if (report != null && scene.path == ScenePath)
                throw new BuildFailedException("Visual reboot is editor-only until the owner resolves the campaign/V4.1 boundary. No playable build is authorized by phase 1.");
        }
    }
}
