// Isolated read-only production dependency audit; NOT a deletion or migration authority.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Eldoria.Audits
{
    public static class DependencyGuidAudit20261026
    {
        [Serializable]
        private class Record
        {
            public string path, guid, category, reason;
            public long bytes;
        }

        [Serializable]
        private class Totals
        {
            public string sourceCommit, activeScene;
            public int allUnityAssets, primaryRoots, productionRoots, historicalSceneRoots;
            public int candidates, activeSceneDependencies, otherProductionDependencies, historicalSceneDependencies, unresolvedCandidates;
            public long candidateBytes, activeSceneBytes, otherProductionBytes, historicalSceneBytes, unresolvedBytes;
            public bool completed;
            public string warning;
        }

        private static readonly string[] CandidatePrefixes =
        {
            "Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/GeneratedEntry",
            "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Generated-",
            "Assets/Eldoria/ArtTests/ValoriaReferenceClean/ArtVariants/"
        };

        private static bool IsCandidate(string path)
        {
            return CandidatePrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsResource(string path)
        {
            return path.IndexOf("/Resources/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsCodeOrTool(string path)
        {
            return new [] {".cs", ".asmdef", ".asmref", ".dll", ".shader", ".compute", ".hlsl", ".rsp" }
                .Any(s => path.EndsWith(s, StringComparison.OrdinalIgnoreCase));
        }

        private static HashSet<string> Dependencies(string[] roots)
        {
            if (roots.Length == 0) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return new HashSet<string>(AssetDatabase.GetDependencies(roots, true), StringComparer.OrdinalIgnoreCase);
        }

        public static void Run()
        {
            var destination = Environment.GetEnvironmentVariable("ELDORIA_DEPENDENCY_AUDIT_OUT");
            if (string.IsNullOrEmpty(destination)) throw new InvalidOperationException("Audit output destination missing.");
            Directory.CreateDirectory(destination);
            var report = new Totals { sourceCommit = Environment.GetEnvironmentVariable("GITHUB_SHA"), warning =
                "An unresolved GUID graph is NOT permission to move/delete; literal paths, native plugins, authoring scripts, and historic provenance need separate review." };
            try
            {
                // AssetDatabase reads the REAL Unity import graph. No asset is modified or reimported intentionally.
                var assets = AssetDatabase.GetAllAssetPaths()
                    .Where(p => p.StartsWith("Assets/", StringComparison.Ordinal) && !AssetDatabase.IsValidFolder(p))
                    .OrderBy(p => p, StringComparer.Ordinal).ToArray();
                report.allUnityAssets = assets.Length;
                var sceneRoots = EditorBuildSettings.scenes.Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
                    .Select(s => s.path).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                if (sceneRoots.Length == 0) throw new InvalidOperationException("No enabled player scene.");
                foreach (var scene in sceneRoots)
                    if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene) == null)
                        throw new InvalidOperationException("Active build scene is missing: " + scene);
                report.activeScene = string.Join(";", sceneRoots);

                var primary = sceneRoots.Concat(assets.Where(IsResource)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                var production = assets.Where(p => !p.StartsWith("Assets/Eldoria/ArtTests/", StringComparison.OrdinalIgnoreCase)
                                               && !IsCandidate(p)).ToArray();
                var otherScenes = assets.Where(p => p.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)
                    && !sceneRoots.Contains(p, StringComparer.OrdinalIgnoreCase)).ToArray();
                report.primaryRoots = primary.Length;
                report.productionRoots = production.Length;
                report.historicalSceneRoots = otherScenes.Length;
                var required = Dependencies(primary);
                var productionDeps = Dependencies(production);
                var otherSceneDeps = Dependencies(otherScenes);
                string unityRoot = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
                using (var writer = new StreamWriter(Path.Combine(destination, "dependency-asset-records.jsonl"), false))
                {
                    foreach (var path in assets.Where(IsCandidate))
                    {
                        var diskPath = Path.Combine(unityRoot, path.Replace('/', Path.DirectorySeparatorChar));
                        var bytes = File.Exists(diskPath) ? new FileInfo(diskPath).Length : 0L;
                        var item = new Record { path=path, guid=AssetDatabase.AssetPathToGUID(path), bytes=bytes };
                        report.candidates++; report.candidateBytes += bytes;
                        if (IsCodeOrTool(path) || IsResource(path))
                        {
                            item.category="PROTECT_CODE_OR_RESOURCE";item.reason="Compiled by Unity or Resources-loaded; never bulk-archive.";
                            report.otherProductionDependencies++;report.otherProductionBytes += bytes;
                        }
                        else if (required.Contains(path))
                        {
                            item.category="PROTECT_ACTIVE_PLAYER";item.reason="Reachable from enabled build scene or a Resources root.";
                            report.activeSceneDependencies++;report.activeSceneBytes += bytes;
                        }
                        else if (productionDeps.Contains(path))
                        {
                            item.category="PROTECT_PRODUCTION_REFERENCED";item.reason="Referenced by non-experimental Unity assets.";
                            report.otherProductionDependencies++;report.otherProductionBytes += bytes;
                        }
                        else if (otherSceneDeps.Contains(path))
                        {
                            item.category="RETAIN_HISTORICAL_TEST";item.reason="Referenced by another Unity scene; not a deletion candidate.";
                            report.historicalSceneDependencies++;report.historicalSceneBytes += bytes;
                        }
                        else
                        {
                            item.category="REVIEW_NO_UNITY_REFERENCE";item.reason="Not resolved in the imported Unity dependency graph. Still requires source, literal-path, editor, workflow and backup checks.";
                            report.unresolvedCandidates++;report.unresolvedBytes += bytes;
                        }
                        writer.WriteLine(JsonUtility.ToJson(item));
                    }
                }
                report.completed=true;
                File.WriteAllText(Path.Combine(destination,"summary.json"),JsonUtility.ToJson(report,true));
                Debug.Log("ELDORIA_DEPENDENCY_GUID_AUDIT_PASS " + JsonUtility.ToJson(report));
            }
            catch (Exception ex)
            {
                report.completed=false;
                File.WriteAllText(Path.Combine(destination,"summary.json"),JsonUtility.ToJson(report,true));
                Debug.LogException(ex);
                throw;
            }
        }
    }
}
