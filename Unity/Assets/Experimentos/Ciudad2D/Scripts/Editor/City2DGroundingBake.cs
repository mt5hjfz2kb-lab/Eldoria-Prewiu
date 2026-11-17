#if UNITY_EDITOR
using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace Eldoria.Experimentos.Ciudad2D.Editor
{
    /// <summary>
    /// NON-DESTRUCTIVE A/B ART TRIAL: soften only the silhouette at the bottom
    /// of a painted stone platform. No geometry, gameplay or file in main is
    /// changed. Result is not an approved asset until native screenshots pass.
    /// </summary>
    public static class City2DGroundingBake
    {
        private const string Sandbox = "Assets/Experimentos/Ciudad2D/Sprites/";
        private const string Output = Sandbox + "Generated/";
        private const string Algorithm = "contact-transition-bottom-v2-r18-y0.42-20261117";
        private static readonly string[] Names =
            { "Cuartel", "Aserradero", "Granero", "Cantera" };
        private static bool generating;

        public static void EnsureGenerated()
        {
            if (generating) return;
            generating = true;
            try
            {
                Directory.CreateDirectory(ToPhysical(Output));
                foreach (var name in Names)
                {
                    string source = ToPhysical(Sandbox + name + ".png");
                    string destinationPath = Output + name + "_Grounded.png";
                    string destination = ToPhysical(destinationPath);
                    byte[] sourceBytes = File.ReadAllBytes(source);
                    string digest;
                    using (var sha = SHA256.Create())
                    {
                        var algo = System.Text.Encoding.UTF8.GetBytes(Algorithm);
                        byte[] combined = new byte[algo.Length + sourceBytes.Length];
                        Buffer.BlockCopy(algo, 0, combined, 0, algo.Length);
                        Buffer.BlockCopy(sourceBytes, 0, combined, algo.Length, sourceBytes.Length);
                        digest = BitConverter.ToString(sha.ComputeHash(combined)).Replace("-", "");
                    }
                    if (File.Exists(destination) && File.Exists(destination + ".sha256") &&
                        File.ReadAllText(destination + ".sha256").Trim() == digest &&
                        AssetDatabase.LoadAssetAtPath<Sprite>(destinationPath) != null)
                        continue;
                    var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (!ImageConversion.LoadImage(tex, sourceBytes))
                        throw new InvalidOperationException("Unable to decode " + name);
                    int w = tex.width, h = tex.height;
                    var pixels = tex.GetPixels32();
                    int[] distance = new int[w * h];
                    const int Far = 9999;
                    // Manhattan distance to transparent silhouette, via two
                    // linear passes. No packages, no non-deterministic randomness.
                    for (int y = 0; y < h; ++y)
                    for (int x = 0; x < w; ++x)
                    {
                        int i = y * w + x;
                        if (pixels[i].a < 16) { distance[i] = 0; continue; }
                        int n = Far;
                        if (x > 0) n = Math.Min(n, distance[i - 1] + 1);
                        if (y > 0) n = Math.Min(n, distance[i - w] + 1);
                        distance[i] = n;
                    }
                    for (int y = h - 1; y >= 0; --y)
                    for (int x = w - 1; x >= 0; --x)
                    {
                        int i = y * w + x;
                        if (x < w - 1) distance[i] = Math.Min(distance[i], distance[i + 1] + 1);
                        if (y < h - 1) distance[i] = Math.Min(distance[i], distance[i + w] + 1);
                    }
                    var output = new Color32[pixels.Length];
                    for (int y = 0; y < h; ++y)
                    for (int x = 0; x < w; ++x)
                    {
                        int i = y * w + x;
                        var pixel = pixels[i];
                        if (pixel.a == 0) { output[i] = pixel; continue; }
                        // Imported image bottom is y=0 in Unity. Fade only
                        // the platform silhouette below roughly 42% height.
                        float bottomWeight = 1f - Mathf.SmoothStep(h * .28f, h * .42f, y);
                        float edge = Mathf.SmoothStep(0f, 18f, distance[i]);
                        float blend = Mathf.Lerp(1f, edge, bottomWeight);
                        pixel.a = (byte)Mathf.Clamp(Mathf.RoundToInt(pixel.a * blend), 0, 255);
                        output[i] = pixel;
                    }
                    // Reject an empty/mis-matted art replacement before Unity
                    // imports it. The previous Cuartel A/B failed this visual gate.
                    int sourceOpaque = 0, groundedOpaque = 0;
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        if (pixels[i].a >= 128) sourceOpaque++;
                        if (output[i].a >= 128) groundedOpaque++;
                    }
                    if (sourceOpaque < 1000 || groundedOpaque < sourceOpaque * .90f)
                        throw new InvalidOperationException("Grounded candidate erased " + name +
                            " opaque=" + groundedOpaque + "/" + sourceOpaque);
                    Debug.Log("[Ciudad2D] GROUNDED_ALPHA_COVERAGE " + name +
                        " " + groundedOpaque + "/" + sourceOpaque);
                    var image = new Texture2D(w, h, TextureFormat.RGBA32, false);
                    image.SetPixels32(output);
                    image.Apply(false, false);
                    File.WriteAllBytes(destination, image.EncodeToPNG());
                    File.WriteAllText(destination + ".sha256", digest);
                    UnityEngine.Object.DestroyImmediate(tex);
                    UnityEngine.Object.DestroyImmediate(image);
                    AssetDatabase.ImportAsset(destinationPath, ImportAssetOptions.ForceSynchronousImport);
                    Debug.Log("[Ciudad2D] GROUNDED_AB_BAKE " + name + " " + w + "x" + h);
                }
            }
            finally { generating = false; }
        }

        private static string ToPhysical(string asset)
        {
            if (!asset.StartsWith(Sandbox, StringComparison.Ordinal))
                throw new ArgumentException("Only isolated art files permitted");
            return Path.Combine(Application.dataPath,
                asset.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
#endif
