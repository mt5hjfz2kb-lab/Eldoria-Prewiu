#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Eldoria.Tests
{
    // The released scene list belongs to the consolidated game. These retained
    // regression fixtures need two additional scenes only while the tests run.
    // Test Framework invokes cleanup even on test/setup errors; SessionState
    // keeps the original snapshot across the Enter/ExitPlayMode domain reloads.
    public sealed class RegressionSceneBuildSetup : IPrebuildSetup, IPostBuildCleanup
    {
        const string SnapshotKey = "Eldoria.Tests.RegressionSceneBuildSetup.v1";
        static readonly string[] Required = {
            "Assets/Eldoria/Scenes/Valoria.unity",
            "Assets/Eldoria/Scenes/Frontier.unity"
        };

        [Serializable] sealed class Entry
        {
            public string path, guid;
            public bool enabled;
        }
        [Serializable] sealed class Snapshot
        {
            public Entry[] scenes;
            public string settingsPath, settingsBytes;
        }

        public void Setup()
        {
            // Recover a prior unfinished setup in this Editor session before
            // taking a fresh snapshot; never replace the canonical backup.
            if (!string.IsNullOrEmpty(SessionState.GetString(SnapshotKey, ""))) Cleanup();
            var original = EditorBuildSettings.scenes;
            var first = original.FirstOrDefault(scene => scene.enabled);
            if (first == null) throw new InvalidOperationException("No canonical enabled scene to preserve.");
            foreach (var path in Required)
                if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(path))
                    throw new FileNotFoundException("Required regression scene is missing.", path);
            string settingsPath = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
                "../ProjectSettings/EditorBuildSettings.asset"));
            var snapshot = new Snapshot {
                scenes = original.Select(scene => new Entry {
                    path = scene.path, guid = scene.guid.ToString(), enabled = scene.enabled
                }).ToArray(),
                settingsPath = settingsPath,
                settingsBytes = Convert.ToBase64String(File.ReadAllBytes(settingsPath))
            };
            SessionState.SetString(SnapshotKey, JsonUtility.ToJson(snapshot));
            try
            {
                var included = original.Select(Clone).ToList();
                foreach (var path in Required)
                {
                    var scene = included.FirstOrDefault(item => item.path == path);
                    if (scene != null) scene.enabled = true;
                    else included.Add(new EditorBuildSettingsScene(path, true));
                }
                if (included.First(scene => scene.enabled).path != first.path)
                    throw new InvalidOperationException("Regression setup would replace the canonical entry scene.");
                EditorBuildSettings.scenes = included.ToArray();
                if (EditorBuildSettings.scenes.First(scene => scene.enabled).path != first.path)
                    throw new InvalidOperationException("Canonical entry scene changed during regression setup.");
            }
            catch
            {
                Cleanup();
                throw;
            }
        }

        public void Cleanup()
        {
            string json = SessionState.GetString(SnapshotKey, "");
            if (string.IsNullOrEmpty(json)) return;
            var snapshot = JsonUtility.FromJson<Snapshot>(json);
            if (snapshot == null || snapshot.scenes == null)
                throw new InvalidDataException("Regression scene backup is invalid; refusing to invent a scene list.");
            var originalBytes = Convert.FromBase64String(snapshot.settingsBytes);
            EditorBuildSettings.scenes = snapshot.scenes.Select(entry =>
                new EditorBuildSettingsScene(entry.path, entry.enabled) { guid = new GUID(entry.guid) }).ToArray();
            // Restore the on-disk serialization as well as the live Editor list.
            File.WriteAllBytes(snapshot.settingsPath, originalBytes);
            var restored = EditorBuildSettings.scenes;
            if (restored.Length != snapshot.scenes.Length || restored.Where((scene, i) =>
                scene.path != snapshot.scenes[i].path || scene.enabled != snapshot.scenes[i].enabled ||
                scene.guid.ToString() != snapshot.scenes[i].guid).Any() ||
                !File.ReadAllBytes(snapshot.settingsPath).SequenceEqual(originalBytes))
                throw new InvalidOperationException("Regression scene catalog did not restore exactly.");
            SessionState.EraseString(SnapshotKey);
        }

        static EditorBuildSettingsScene Clone(EditorBuildSettingsScene scene)
            => new EditorBuildSettingsScene(scene.path, scene.enabled) { guid = scene.guid };
    }
}
#endif
