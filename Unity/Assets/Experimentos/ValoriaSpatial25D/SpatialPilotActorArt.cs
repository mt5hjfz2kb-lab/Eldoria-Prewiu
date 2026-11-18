using System.Collections.Generic;
using UnityEngine;

namespace Eldoria.Experimentos.ValoriaSpatial25D
{
    /// <summary>
    /// Reproducible small silhouette PROXIES for evaluating occlusion and scale.
    /// These are explicitly NOT artist-approved character art; replace them after
    /// spatial A/B has been checked in actual Unity. No sourced sprite copied.
    /// </summary>
    internal static class SpatialPilotActorArt
    {
        private static readonly Dictionary<int, Sprite> frames = new Dictionary<int, Sprite>();
        private static Sprite shadow;
        private const int W = 48, H = 72;
        public static Sprite Shadow
        {
            get
            {
                if (shadow != null) return shadow;
                var tex = new Texture2D(48, 20, TextureFormat.RGBA32, false);
                tex.filterMode = FilterMode.Bilinear;
                var pixels = new Color32[48 * 20];
                for (int y = 0; y < 20; y++)
                for (int x = 0; x < 48; x++)
                {
                    float d = Mathf.Pow((x - 23.5f) / 22f, 2f) +
                              Mathf.Pow((y - 9.5f) / 7f, 2f);
                    pixels[y * 48 + x] = new Color32(35, 27, 55,
                        (byte)Mathf.RoundToInt(Mathf.Clamp01(1f - d) * 115f));
                }
                tex.SetPixels32(pixels);
                tex.Apply(false, true);
                tex.hideFlags = HideFlags.HideAndDontSave;
                shadow = Sprite.Create(tex, new Rect(0, 0, 48, 20),
                    new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect);
                shadow.hideFlags = HideFlags.HideAndDontSave;
                return shadow;
            }
        }

        public static Sprite Frame(SpatialPilotWorker.Role role, int pose)
        {
            int key = (int)role * 3 + Mathf.Clamp(pose, 0, 2);
            if (frames.TryGetValue(key, out var cached) && cached != null) return cached;
            Color32[] pixels = new Color32[W * H];
            Color32 outline = new Color32(41, 35, 49, 255);
            Color32 skin = new Color32(196, 143, 100, 255);
            Color32 boot = new Color32(67, 45, 45, 255);
            Color32 coat = role == SpatialPilotWorker.Role.Recruit
                ? new Color32(57, 78, 122, 255)
                : role == SpatialPilotWorker.Role.Woodcutter
                    ? new Color32(111, 76, 47, 255)
                    : new Color32(84, 96, 63, 255);
            Color32 light = role == SpatialPilotWorker.Role.Recruit
                ? new Color32(100, 124, 163, 255)
                : role == SpatialPilotWorker.Role.Woodcutter
                    ? new Color32(171, 118, 64, 255)
                    : new Color32(140, 148, 94, 255);

            int stride = pose == 0 ? -3 : pose == 1 ? 3 : 0;
            Rect(pixels, 14 + stride, 5, 22 + stride, 19, outline);
            Rect(pixels, 25 - stride, 5, 33 - stride, 19, outline);
            Rect(pixels, 15 + stride, 12, 21 + stride, 25, boot);
            Rect(pixels, 26 - stride, 12, 32 - stride, 25, boot);
            Rect(pixels, 13, 25, 35, 48, outline);
            Rect(pixels, 16, 28, 32, 46, coat);
            Rect(pixels, 17, 33, 23, 46, light);
            Rect(pixels, 15, 29, 33, 33, new Color32(79, 54, 42, 255));
            int armSwing = pose == 0 ? -4 : pose == 1 ? 4 : -2;
            Rect(pixels, 9 + armSwing, 28, 15 + armSwing, 42, coat);
            Rect(pixels, 34 - armSwing, 28, 40 - armSwing, 42, coat);
            Ellipse(pixels, 11 + armSwing, 28, 3, 4, skin);
            Ellipse(pixels, 37 - armSwing, 28, 3, 4, skin);
            Ellipse(pixels, 24, 55, 11, 12, outline);
            Ellipse(pixels, 24, 55, 8, 9, skin);
            Rect(pixels, 16, 62, 32, 65, boot);
            Rect(pixels, 17, 64, 31, 67, light);
            // Each activity has a recognizable prop, but no UI icon or combat action.
            if (role == SpatialPilotWorker.Role.Woodcutter)
            {
                Rect(pixels, 38, 23, 40, 55, new Color32(128, 85, 53, 255));
                Rect(pixels, 35, 53, 44, 59, new Color32(141, 148, 160, 255));
            }
            else if (role == SpatialPilotWorker.Role.Recruit)
            {
                Rect(pixels, 39, 23, 41, 66, new Color32(134, 103, 72, 255));
                Rect(pixels, 37, 61, 43, 65, new Color32(160, 167, 176, 255));
            }
            else
            {
                Rect(pixels, 35, 20, 45, 33, new Color32(125, 83, 49, 255));
                Rect(pixels, 37, 25, 43, 30, new Color32(199, 158, 88, 255));
            }
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            tex.hideFlags = HideFlags.HideAndDontSave;
            var sprite = Sprite.Create(tex, new Rect(0, 0, W, H),
                new Vector2(.5f, .04f), 100f, 0, SpriteMeshType.FullRect);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            frames[key] = sprite;
            return sprite;
        }

        private static void Rect(Color32[] p, int x0, int y0, int x1, int y1, Color32 color)
        {
            for (int y = Mathf.Max(0, y0); y <= Mathf.Min(H - 1, y1); y++)
            for (int x = Mathf.Max(0, x0); x <= Mathf.Min(W - 1, x1); x++)
                p[y * W + x] = color;
        }
        private static void Ellipse(Color32[] p, int cx, int cy, int rx, int ry, Color32 color)
        {
            for (int y = Mathf.Max(0, cy - ry); y <= Mathf.Min(H - 1, cy + ry); y++)
            for (int x = Mathf.Max(0, cx - rx); x <= Mathf.Min(W - 1, cx + rx); x++)
            {
                float dx = (x - cx) / (float)Mathf.Max(1, rx);
                float dy = (y - cy) / (float)Mathf.Max(1, ry);
                if (dx * dx + dy * dy <= 1f) p[y * W + x] = color;
            }
        }
    }
}
