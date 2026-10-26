// ELDORIA READ-ONLY UNITY ASSETDATABASE AUDIT
// This source is staged in tools/, NOT compiled into main.
// To execute, copy into a temporary Assets/Editor folder on an isolated Unity
// checkout, call -executeMethod EldoriaAudit.NativeDependencyAudit.Run,
// then discard the temporary working tree. No asset moves or deletions.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EldoriaAudit
{
    public static class NativeDependencyAudit
    {
        [Serializable]
        public class Family
        {
            public string name;
            public string prefix;
            public int totalFiles;
            public int dependentFiles;
            public long totalBytes;
            public long dependentBytes;
            public string[] referencedSamples;
        }
        [Serializable]
        public class Result
        {
            public string status;
            public string[] enabledScenes;
            public int resourcesAndStreamingAssets;
            public int reachableAssets;
            public int totalAssetPaths;
            public string[] unresolvedGuids;
            public string[] importantReferences;
            public Family[] families;
            public bool noAssetsMoved = true;
            public bool noFilesDeleted = true;
            public string note;
        }

        public static void Run()
        {
            var sceneRoots = EditorBuildSettings.scenes
                .Where(x => x.enabled).Select(x => x.path).ToArray();
            const string gameplayScene =
                "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
            if (!sceneRoots.Contains(gameplayScene))
                throw new Exception("Audit aborted: integrated playable scene not enabled in build settings.");

            var all = AssetDatabase.GetAllAssetPaths()
                .Where(x => x.StartsWith("Assets/", StringComparison.Ordinal))
                .ToArray();
            var resources = all.Where(x =>
                x.Contains("/Resources/") || x.Contains("/StreamingAssets/")).ToArray();
            var roots = sceneRoots.Concat(resources).Distinct().ToArray();
            var reachable = new HashSet<string>(
                AssetDatabase.GetDependencies(roots, true),
                StringComparer.Ordinal);
            var invalid = new List<string>();
            var sampled = new List<string>();
            foreach (var path in reachable)
            {
                if (!path.StartsWith("Assets/", StringComparison.Ordinal)) continue;
                var guid = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrWhiteSpace(guid) ||
                    !string.Equals(path, AssetDatabase.GUIDToAssetPath(guid),
                        StringComparison.Ordinal))
                {
                    invalid.Add(path);
                }
                else if (sampled.Count < 100)
                {
                    sampled.Add(path);
                }
            }

            var familyPairs = new[]
            {
                new[] { "world_native_sources", "../docs/evidence/world4x-canon-20261022/sources/native-" },
                new[] { "layered_generated", "Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/GeneratedEntry" },
                new[] { "integration_generated", "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Generated-" },
                new[] { "layered_all", "Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/" },
                new[] { "reference_all", "Assets/Eldoria/ArtTests/ValoriaReferenceClean/" },
                new[] { "world_runtime", "Assets/Eldoria/World4XCanon/" },
                new[] { "valoria_resources", "Assets/Eldoria/Resources/Valoria/" }
            };

            var projectRoot = Directory.GetParent(Application.dataPath).FullName;
            Func<string, long> bytes = path =>
            {
                var abs = Path.GetFullPath(Path.Combine(projectRoot, path));
                return File.Exists(abs) ? new FileInfo(abs).Length : 0L;
            };
            var families = new List<Family>();
            foreach (var pair in familyPairs)
            {
                var prefix = pair[1];
                // Unity only enumerates Unity asset paths; Blender authoring and
                // evidence outside Assets are intentionally NOT certified.
                var matching = all.Where(x => x.StartsWith(prefix, StringComparison.Ordinal)
                    && !x.EndsWith(".meta", StringComparison.Ordinal)).ToArray();
                var deps = matching.Where(x => reachable.Contains(x)).ToArray();
                families.Add(new Family
                {
                    name = pair[0],
                    prefix = prefix,
                    totalFiles = matching.Length,
                    dependentFiles = deps.Length,
                    totalBytes = matching.Sum(bytes),
                    dependentBytes = deps.Sum(bytes),
                    referencedSamples = deps.Take(30).ToArray()
                });
            }

            var result = new Result
            {
                status = invalid.Count == 0 ? "UNITY_DEPENDENCIES_INSPECTED" : "REQUIRES_UNRESOLVED_REFERENCE_REVIEW",
                enabledScenes = sceneRoots,
                resourcesAndStreamingAssets = resources.Length,
                reachableAssets = reachable.Count,
                totalAssetPaths = all.Length,
                unresolvedGuids = invalid.ToArray(),
                importantReferences = sampled.ToArray(),
                families = families.ToArray(),
                note = "Unity AssetDatabase graph only; dynamic Resources.Load, procedural scene creation, Blender authoring, workflows, save games and historical replay require separate checks. Never infer that unreferenced means removable."
            };
            var outputFolder = Environment.GetEnvironmentVariable("ELDORIA_DEPENDENCY_AUDIT_OUT");
            if (string.IsNullOrWhiteSpace(outputFolder))
                outputFolder = Path.Combine(projectRoot, "Temp", "EldoriaDependencyAudit");
            Directory.CreateDirectory(outputFolder);
            File.WriteAllText(Path.Combine(outputFolder, "unity-native-dependencies.json"),
                JsonUtility.ToJson(result, true));
            Debug.Log("ELDORIA_DEPENDENCY_AUDIT_COMPLETE reachable=" + reachable.Count
                + " resources=" + resources.Length + " unresolved=" + invalid.Count);
        }
    }
}
