#if UNITY_EDITOR
using System;
using System.IO;
using System.Security.Cryptography;
using Eldoria.BreachWatch;
using Eldoria.ReferenceClean;
using UnityEditor;
using UnityEngine;

namespace Eldoria.MigrationQA
{
    // Called at the original fixture's PNG write, while its camera and layout still match that PNG.
    public static class ValoriaMigrationCapture
    {
        [Serializable]
        sealed class Sample
        {
            public string suite, sourceCommit, state, screenshot, screenshotSha256, inventory;
            public int width, height, editorWidth, editorHeight, frame;
            public string unity, scene, camera, cameraTarget, graphicsDevice;
            public string mode = "Unity Editor Play Mode; canonical player presentation; fixture save";
        }

        public static void Record(string screenshot, Camera camera, int width, int height)
        {
            if (!EditorApplication.isPlaying || !camera || !File.Exists(screenshot))
                throw new InvalidOperationException("Capture requires the rendered Play Mode screenshot and camera.");
            var host = UnityEngine.Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
            var hud = UnityEngine.Object.FindFirstObjectByType<BreachWatchHud>();
            if (!host || !host.Ready || !host.City || !host.City.Probe || !hud ||
                !BreachWatchProductionPresentation.Required || !hud.IndependentCanvas ||
                !hud.LegacyPresentationHidden || !hud.ProductionPresentation)
                throw new InvalidOperationException("Disposable Valoria scene with the canonical player HUD required.");
            var rt = camera.targetTexture;
            if (camera != host.PlayerCamera || !rt || rt.width != width || rt.height != height)
                throw new InvalidOperationException("Fixture target differs from the canonical render camera.");
            var output = Environment.GetEnvironmentVariable("VALORIA_MIGRATION_OUTPUT");
            var suite = Environment.GetEnvironmentVariable("VALORIA_MIGRATION_SUITE");
            var sha = Environment.GetEnvironmentVariable("ELDORIA_QA_SOURCE_SHA");
            if (string.IsNullOrEmpty(output) || string.IsNullOrEmpty(suite))
                throw new InvalidOperationException("Missing capture output or suite name.");
            var folder = Path.Combine(output, suite);
            Directory.CreateDirectory(folder);
            var stem = Path.GetFileNameWithoutExtension(screenshot);
            var sample = new Sample {
                suite = suite, sourceCommit = sha, state = stem, screenshot = stem + ".png",
                inventory = stem + ".inventory.json", width = width, height = height,
                editorWidth = Screen.width, editorHeight = Screen.height, frame = Time.frameCount,
                unity = UnityEngine.Application.unityVersion, scene = host.gameObject.scene.path, camera = camera.name,
                cameraTarget = rt.name, graphicsDevice = SystemInfo.graphicsDeviceType.ToString()
            };
            var imagePath = Path.Combine(folder,sample.screenshot);
            File.Copy(screenshot,imagePath,false);
            using (var stream = File.OpenRead(imagePath))
            using (var hash = SHA256.Create())
                sample.screenshotSha256 = BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
            ValoriaLiveInventory.Capture(Path.Combine(folder,sample.inventory),sha,stem);
            bool cityFocus = (suite == "hud" && (stem == "real-unity-city-390x844" || stem == "real-unity-bastion-390x844")) ||
                (suite == "destinations" && stem == "real-unity-nav-advanced-390x844") ||
                (suite == "bag" && stem == "real-vigilia-bag-actual-integrated-city-390x844");
            if (cityFocus)
                global::EldoriaCityBaselineSnapshot.Capture(folder, stem);
            using (var stream = new FileStream(Path.Combine(folder,stem+".sample.json"), FileMode.CreateNew))
            using (var writer = new StreamWriter(stream))
                writer.Write(JsonUtility.ToJson(sample,true));
        }
    }
}
#endif
