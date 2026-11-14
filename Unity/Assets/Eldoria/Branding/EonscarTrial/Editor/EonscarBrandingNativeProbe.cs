#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Eldoria.Branding;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.Branding.Editor
{
    // Batchmode-only opt-in source/import & UI hierarchy probe. It never saves scenes.
    // This is NOT a player-session or artistic screenshot certificate.
    public static class EonscarBrandingNativeProbe
    {
        private const string Resource = "EonscarTrial/eonscar-logo";
        private const string ExpectedSha256 =
            "c5ac3f2688196548daea6c61e7d0654f3b0cff0c613c78b44a12b4d72bbeab72";

        [Serializable]
        private sealed class Report
        {
            public string result;
            public string assetPath;
            public string pngSha256;
            public int textureWidth;
            public int textureHeight;
            public int canvasCount;
            public int logoImages;
            public int buttons;
            public int textLabels;
            public bool overlayInputBlocking;
            public bool savedSceneUntouched;
            public bool settingsUnchanged;
            public bool gamePlayed;
            public bool deviceTested;
        }

        [MenuItem("Eldoria/EONSCAR/Validar importación nativa (sin publicar)")]
        public static void Run()
        {
            var savedScenePaths = string.Join("|", EditorBuildSettings.scenes
                .Select(s => s.path + ":" + s.enabled));
            string savedProduct = PlayerSettings.productName;
            GameObject root = null;
            try
            {
                // Never open, modify or save Valoria; use only an empty unsaved
                // editor scene and inspect the same real runtime UI component.
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                AssetDatabase.Refresh();
                var texture = Resources.Load<Texture2D>(Resource);
                Require(texture != null, "Runtime Resources logo missing");
                Require(texture.width == 512 && texture.height == 384,
                    "Expected the 512x384 QA preview, not a renamed replacement");
                string assetPath = AssetDatabase.GetAssetPath(texture);
                Require(assetPath.EndsWith("/EonscarTrial/Resources/EonscarTrial/eonscar-logo.png",
                    StringComparison.Ordinal), "Imported texture is not from owned directory");
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                Require(importer != null && importer.alphaIsTransparency,
                    "Transparent PNG import settings invalid");
                string pngSha;
                string diskPath = Path.GetFullPath(Path.Combine(
                    UnityEngine.Application.dataPath, "..", assetPath));
                using (var sha = SHA256.Create())
                    pngSha = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(diskPath)))
                        .Replace("-", "").ToLowerInvariant();
                Require(pngSha == ExpectedSha256, "QA PNG content differs from selected art");

                root = new GameObject("EONSCAR isolated native import probe");
                var overlay = root.AddComponent<EonscarBrandingTrialOverlay>();
                var build = typeof(EonscarBrandingTrialOverlay).GetMethod("BuildOverlay",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Require(build != null, "Overlay build method unavailable");
                build.Invoke(overlay, null);

                var canvases = root.GetComponentsInChildren<Canvas>(true);
                var logos = root.GetComponentsInChildren<RawImage>(true);
                var buttons = root.GetComponentsInChildren<Button>(true);
                var labels = root.GetComponentsInChildren<Text>(true);
                var blockers = root.GetComponentsInChildren<Image>(true);
                bool blocked = blockers.Any(g =>
                    g.gameObject.name == "Input-blocking backdrop" && g.raycastTarget &&
                    g.GetComponent<Button>() != null &&
                    g.GetComponent<Button>().transition == Selectable.Transition.None);
                Require(canvases.Length == 1 &&
                    canvases[0].renderMode == RenderMode.ScreenSpaceOverlay &&
                    canvases[0].sortingOrder == 30000, "Unexpected Canvas hierarchy");
                Require(logos.Length == 1 && logos[0].texture == texture,
                    "Actual Resources PNG is not rendered by overlay");
                // Two UI buttons: the silent full-screen tap sink and the
                // one intentional CTA. No second gameplay action is introduced.
                Require(buttons.Length == 2 &&
                    buttons.Any(b => b.gameObject.name == "Enter existing gameplay") &&
                    labels.Any(t => t.text == "ENTRAR EN VALORIA") &&
                    labels.Any(t => t.text == "LAS CENIZAS DE VALORIA") && blocked,
                    "CTA, title caption or input blocking missing");

                Require(string.Join("|", EditorBuildSettings.scenes
                    .Select(s => s.path + ":" + s.enabled)) == savedScenePaths &&
                    PlayerSettings.productName == savedProduct,
                    "Product identity/build scenes unexpectedly changed");

                string dest = Environment.GetEnvironmentVariable("EONSCAR_QA_OUTPUT");
                if (string.IsNullOrWhiteSpace(dest))
                    dest = Path.Combine(Directory.GetCurrentDirectory(), "EonscarTrialOutput");
                Directory.CreateDirectory(dest);
                var report = new Report
                {
                    result = "PASS_EDITOR_IMPORT_AND_UI_HIERARCHY_ONLY",
                    assetPath = assetPath, pngSha256 = pngSha,
                    textureWidth = texture.width, textureHeight = texture.height,
                    canvasCount = canvases.Length, logoImages = logos.Length,
                    buttons = buttons.Length, textLabels = labels.Length,
                    overlayInputBlocking = blocked, savedSceneUntouched = true,
                    settingsUnchanged = true, gamePlayed = false, deviceTested = false
                };
                File.WriteAllText(Path.Combine(dest, "eonscar-native-probe.json"),
                    JsonUtility.ToJson(report, true));
                Debug.Log("EONSCAR_NATIVE_IMPORT_AND_UI_HIERARCHY_PASS (not PlayMode or final art)");
            }
            finally
            {
                if (root != null) UnityEngine.Object.DestroyImmediate(root);
                // Probe scene is unsaved. Never save to an Eldoria project scene.
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("EONSCAR_NATIVE_QA_FAIL " + message);
        }
    }
}
#endif
