using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    // Owner-facing helper for inspecting the certified three-family Micro-Valoria
    // directly in the local Unity Editor. This is convenience tooling only;
    // certification still belongs to the canonical GitHub Actions gates.
    public static class MicroValoriaOwnerReview
    {
        const string SceneAsset = "Assets/Eldoria/ArtTests/ImageTo3D/MicroValoriaReview.unity";

        const string TowerName = "Eldoria_Module_TowerWallRock_50K.glb";
        const string TowerSha = "18785f7ba607cef1e7dcee45684d65c3166dc47b0f73b4bc4fb3d295c6f53a6a";

        const string TerraceRawName = "Eldoria_Module_TerraceStairRock.glb";
        const string TerraceRawSha = "75e9a955e0c3f7596fbb4a70ef76eccceff7a379a0437ea859c587d2a2653685";
        const string TerraceOptimizedName = "Eldoria_Module_TerraceStairRock_50K.glb";

        const string GateRawName = "Eldoria_Module_GateStreetRiseRock_MV1.glb";
        const string GateRawSha = "4d1c19978302643003600b0da5ed2f0ef14f7256c85da03077d36fa9f13e294e";
        const string GateOptimizedName = "Eldoria_Module_GateStreetRiseRock_50K.glb";

        [MenuItem("Eldoria/Art Gate/Micro-Valoria/Rebuild and Open Certified Review")]
        public static void RebuildAndOpen()
        {
            try
            {
                PrepareCertifiedSources();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                MicroValoriaReview.Capture();
                EditorSceneManager.OpenScene(SceneAsset);
                FrameDistrict();
                UnityEngine.Debug.Log("Micro-Valoria owner review is ready. Certification remains the GitHub Actions gate.");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                EditorUtility.DisplayDialog(
                    "Micro-Valoria review could not be prepared",
                    ex.Message +
                    "\n\nExpected owner exports live in your Downloads folder. " +
                    "The production runner remains the source of certification.",
                    "OK");
            }
        }

        [MenuItem("Eldoria/Art Gate/Micro-Valoria/Open Last Generated Review")]
        public static void OpenLastGenerated()
        {
            var full = Path.Combine(UnityProjectRoot(), "Assets", "Eldoria", "ArtTests", "ImageTo3D", "MicroValoriaReview.unity");
            if (!File.Exists(full))
            {
                EditorUtility.DisplayDialog(
                    "No local Micro-Valoria scene",
                    "There is no generated local review yet. Use 'Rebuild and Open Certified Review' first.",
                    "OK");
                return;
            }

            EditorSceneManager.OpenScene(SceneAsset);
            FrameDistrict();
        }

        [MenuItem("Eldoria/Art Gate/Micro-Valoria/Prepare Certified Sources Only")]
        public static void PrepareCertifiedSourcesOnly()
        {
            try
            {
                PrepareCertifiedSources();
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                EditorUtility.DisplayDialog(
                    "Certified sources prepared",
                    "The three certified Micro-Valoria source modules are staged locally in the isolated Source folder.",
                    "OK");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
                EditorUtility.DisplayDialog("Source preparation failed", ex.Message, "OK");
            }
        }

        static void PrepareCertifiedSources()
        {
            var downloads = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");
            if (!Directory.Exists(downloads))
                throw new DirectoryNotFoundException("Downloads folder not found: " + downloads);

            var sourceDir = Path.Combine(
                UnityEngine.Application.dataPath,
                "Eldoria", "ArtTests", "ImageTo3D", "Source");
            Directory.CreateDirectory(sourceDir);

            // TowerWallRock is already the certified 50K owner file.
            var tower = Path.Combine(downloads, TowerName);
            RequireExactSha(tower, TowerSha, "TowerWallRock certified 50K");
            File.Copy(tower, Path.Combine(sourceDir, TowerName), true);

            // Terrace and Gate are owner raw exports. Local review runs the same
            // canonical Blender processor used by CI. This prepares a visual-review
            // copy; the certified artifact hashes remain documented in the repo.
            var terraceRaw = Path.Combine(downloads, TerraceRawName);
            RequireExactSha(terraceRaw, TerraceRawSha, "TerraceStairRock raw source");
            RunCanonicalBlender(
                terraceRaw,
                Path.Combine(sourceDir, TerraceOptimizedName),
                "terrace-owner-review.json");

            var gateRaw = Path.Combine(downloads, GateRawName);
            RequireExactSha(gateRaw, GateRawSha, "GateStreetRiseRock MV1 raw source");
            RunCanonicalBlender(
                gateRaw,
                Path.Combine(sourceDir, GateOptimizedName),
                "gate-owner-review.json");
        }

        static void RunCanonicalBlender(string input, string output, string reportName)
        {
            var repoRoot = RepositoryRoot();
            var script = Path.Combine(repoRoot, "tools", "tripo_module_blender.py");
            if (!File.Exists(script))
                throw new FileNotFoundException("Canonical Blender script missing: " + script);

            var blender = FindBlender();
            var reportDir = Path.Combine(UnityProjectRoot(), "Library", "EldoriaOwnerReview");
            Directory.CreateDirectory(reportDir);
            var report = Path.Combine(reportDir, reportName);

            var psi = new ProcessStartInfo
            {
                FileName = blender,
                Arguments =
                    "--background --python " + Q(script) +
                    " -- --input " + Q(input) +
                    " --output " + Q(output) +
                    " --report " + Q(report),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(psi))
            {
                if (process == null)
                    throw new InvalidOperationException("Could not start Blender.");

                var stdout = process.StandardOutput.ReadToEnd();
                var stderr = process.StandardError.ReadToEnd();

                if (!process.WaitForExit(600000))
                {
                    try { process.Kill(); } catch { }
                    throw new TimeoutException("Blender owner-review preparation timed out.");
                }

                if (process.ExitCode != 0)
                    throw new InvalidOperationException(
                        "Blender failed while preparing " + Path.GetFileName(input) +
                        "\nExit code: " + process.ExitCode +
                        "\n\n" + Tail(stdout + "\n" + stderr, 5000));
            }

            if (!File.Exists(output))
                throw new FileNotFoundException("Blender did not produce: " + output);
        }

        static string FindBlender()
        {
            var candidates = new[]
            {
                @"C:\Program Files\Blender Foundation",
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Programs", "Blender Foundation")
            };

            foreach (var root in candidates)
            {
                if (!Directory.Exists(root)) continue;
                var found = Directory.GetFiles(root, "blender.exe", SearchOption.AllDirectories)
                    .OrderByDescending(x => x)
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(found)) return found;
            }

            throw new FileNotFoundException(
                "Blender was not found. Expected the same local Blender installation used by the Eldoria runner.");
        }

        static void RequireExactSha(string path, string expected, string label)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(label + " is missing from Downloads: " + path);

            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
            {
                var actual = BitConverter.ToString(sha.ComputeHash(stream))
                    .Replace("-", "")
                    .ToLowerInvariant();
                if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                        label + " SHA-256 does not match the certified source.\n" +
                        "Expected: " + expected + "\nActual:   " + actual);
            }
        }

        static void FrameDistrict()
        {
            var root = GameObject.Find("Micro-Valoria | isolated district experiment");
            if (root == null || SceneView.lastActiveSceneView == null) return;
            Selection.activeGameObject = root;
            SceneView.lastActiveSceneView.FrameSelected();
            SceneView.lastActiveSceneView.Repaint();
        }

        static string RepositoryRoot()
        {
            return Directory.GetParent(UnityProjectRoot()).FullName;
        }

        static string UnityProjectRoot()
        {
            return Directory.GetParent(UnityEngine.Application.dataPath).FullName;
        }

        static string Q(string value)
        {
            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }

        static string Tail(string value, int max)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max) return value;
            return value.Substring(value.Length - max);
        }
    }
}
