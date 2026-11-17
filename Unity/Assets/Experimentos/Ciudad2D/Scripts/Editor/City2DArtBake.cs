#if UNITY_EDITOR
using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2D.Editor
{
    /// <summary>
    /// Experimental non-destructive local-contrast bake of existing RGBA sprites.
    /// Stores derived files only under Ciudad2D/Sprites/Generated in the isolated
    /// Unity scratch project. Never edits the source PNG, game save or main scene.
    /// This does not create source detail that is absent from the original artwork.
    /// </summary>
    public static class City2DArtBake
    {
        private const string Folder = "Assets/Experimentos/Ciudad2D/Sprites/";
        private const string Generated = Folder + "Generated/";
        private const string Algorithm = "city2d-edge-bake-v1-20261117";
        private static readonly string[] Sources = {
            "Cuartel.png", "Aserradero.png", "Granero.png", "Cantera.png"
        };
        private static bool busy;

        [MenuItem("Eldoria/Experimentos/Ciudad 2D/Regenerar sprites de contraste")]
        public static void Regenerate()
        {
            foreach (var source in Sources)
            {
                string marker = ToPhysical(Generated + Path.GetFileNameWithoutExtension(source) + "_Crisp.png.sha256");
                if (File.Exists(marker)) File.Delete(marker);
            }
            EnsureGenerated();
        }

        public static void EnsureGenerated()
        {
            if (busy) return;
            busy = true;
            try
            {
                Directory.CreateDirectory(ToPhysical(Generated));
                foreach (var name in Sources)
                {
                    string path = Folder + name;
                    string raw = ToPhysical(path);
                    if (!File.Exists(raw))
                        throw new FileNotFoundException("Ciudad2D source sprite missing", raw);
                    byte[] bytes = File.ReadAllBytes(raw);
                    string fingerprint;
                    using (var hash = SHA256.Create())
                    {
                        byte[] salt = System.Text.Encoding.UTF8.GetBytes(Algorithm);
                        var joined = new byte[salt.Length + bytes.Length];
                        Buffer.BlockCopy(salt, 0, joined, 0, salt.Length);
                        Buffer.BlockCopy(bytes, 0, joined, salt.Length, bytes.Length);
                        fingerprint = BitConverter.ToString(hash.ComputeHash(joined)).Replace("-", "");
                    }

                    string targetAsset = Generated + Path.GetFileNameWithoutExtension(name) + "_Crisp.png";
                    string target = ToPhysical(targetAsset);
                    string marker = target + ".sha256";
                    if (File.Exists(target) && File.Exists(marker) &&
                        string.Equals(File.ReadAllText(marker).Trim(), fingerprint, StringComparison.Ordinal) &&
                        AssetDatabase.LoadAssetAtPath<Sprite>(targetAsset) != null)
                        continue;

                    var rawTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (!ImageConversion.LoadImage(rawTex, bytes, false))
                        throw new InvalidOperationException("Could not decode " + path);
                    int width = rawTex.width, height = rawTex.height;
                    Color32[] original = rawTex.GetPixels32();
                    Color32[] output = new Color32[original.Length];

                    for (int y = 0; y < height; ++y)
                    for (int x = 0; x < width; ++x)
                    {
                        int index = y * width + x;
                        Color32 p = original[index];
                        if (p.a < 24) { output[index] = p; continue; }
                        float red = 0f, green = 0f, blue = 0f, sum = 0f;
                        int[] ax = { -1, 1, 0, 0 }, ay = { 0, 0, -1, 1 };
                        for (int k = 0; k < 4; ++k)
                        {
                            int nx = Mathf.Clamp(x + ax[k], 0, width - 1);
                            int ny = Mathf.Clamp(y + ay[k], 0, height - 1);
                            Color32 q = original[ny * width + nx];
                            // Alpha-weighted neighbor average prevents bright
                            // transparent pixels contaminating outline colors.
                            float weight = q.a / 255f;
                            if (weight < 0.12f) continue;
                            red += q.r * weight; green += q.g * weight;
                            blue += q.b * weight; sum += weight;
                        }
                        if (sum < 0.1f) { output[index] = p; continue; }
                        float strength = 0.65f * Mathf.SmoothStep(0.0f, 1.0f, p.a / 255f);
                        output[index] = new Color32(
                            (byte)Mathf.Clamp(Mathf.RoundToInt(p.r + strength * (p.r - red / sum)), 0, 255),
                            (byte)Mathf.Clamp(Mathf.RoundToInt(p.g + strength * (p.g - green / sum)), 0, 255),
                            (byte)Mathf.Clamp(Mathf.RoundToInt(p.b + strength * (p.b - blue / sum)), 0, 255),
                            p.a);
                    }
                    var polished = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    polished.SetPixels32(output);
                    polished.Apply(false, false);
                    File.WriteAllBytes(target, polished.EncodeToPNG());
                    File.WriteAllText(marker, fingerprint);
                    UnityEngine.Object.DestroyImmediate(rawTex);
                    UnityEngine.Object.DestroyImmediate(polished);
                    AssetDatabase.ImportAsset(targetAsset, ImportAssetOptions.ForceSynchronousImport);
                    Debug.Log("[Ciudad2D] CRISP_BAKE " + targetAsset + " (" + width + "x" + height + ")");
                }
            }
            finally { busy = false; }
        }

        public static Sprite ForBuilding(string id, string original)
        {
            if (id == "cuartel" || id == "aserradero" || id == "granero" || id == "cantera")
            {
                var derived = AssetDatabase.LoadAssetAtPath<Sprite>(
                    Generated + Path.GetFileNameWithoutExtension(original) + "_Crisp.png");
                if (derived != null) return derived;
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(Folder + original);
        }

        private static string ToPhysical(string asset)
        {
            if (!asset.StartsWith("Assets/", StringComparison.Ordinal))
                throw new ArgumentException("Only sandbox Assets paths are permitted");
            return Path.Combine(UnityEngine.Application.dataPath,
                asset.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
#endif
