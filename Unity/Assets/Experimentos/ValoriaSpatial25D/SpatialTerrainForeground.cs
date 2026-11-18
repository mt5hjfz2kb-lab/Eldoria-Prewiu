using System;
using System.Collections.Generic;
using UnityEngine;

namespace Eldoria.Experimentos.ValoriaSpatial25D
{
    /// <summary>
    /// Re-uses EXACT source texels of the Premium V6 painted terrain to create
    /// separate foreground occlusion meshes. No image resampling, duplication,
    /// inpainting, grass replacement or second texture allocation.
    ///
    /// Areas are compact, convex polygons authored in the ONE stable 941x1672
    /// layout coordinate space. A wall can now sit in front of a visitor whose
    /// feet are behind it, and behind a visitor who walks ahead of its foot line.
    ///
    /// A primitive first gate mask is a technical experiment, NOT art approved.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SpatialTerrainForeground : MonoBehaviour
    {
        [Serializable] private struct Point
        {
            public float x, y;
        }
        [Serializable] private sealed class Region
        {
            public string id;
            public float foot_y;
            public Point[] contour;
        }
        [Serializable] private sealed class Layout
        {
            public int schema, coordinate_width, coordinate_height;
            public Region[] regions;
        }

        private readonly List<Mesh> meshes = new List<Mesh>();
        private readonly Dictionary<string, Renderer> parts = new Dictionary<string, Renderer>(StringComparer.Ordinal);
        private Material sharedCutoutMaterial;
        private SpriteRenderer ground;
        private GameObject graphics;
        public int RegionCount { get; private set; }
        public bool IsPurePresentation => true;
        public int OrderFor(string regionId)
        {
            return parts.TryGetValue(regionId, out var renderer) && renderer != null
                ? renderer.sortingOrder
                : throw new InvalidOperationException("Unknown original-terrace occluder: " + regionId);
        }

        public void Configure(SpriteRenderer source, TextAsset json, Shader shader)
        {
            if (source == null || source.sprite == null || json == null || shader == null)
                throw new InvalidOperationException("Occluder needs ground, layout and URP cutout shader");
            if (sharedCutoutMaterial != null)
                throw new InvalidOperationException("Configure called twice on same pilot");
            ground = source;
            var doc = JsonUtility.FromJson<Layout>(json.text);
            if (doc == null || doc.schema != 1 || doc.coordinate_width != 941 ||
                doc.coordinate_height != 1672 || doc.regions == null ||
                doc.regions.Length < 1 || doc.regions.Length > 12)
                throw new InvalidOperationException("Incorrect spatial foreground layout");
            var sprite = source.sprite;
            var texture = sprite.texture;
            if (texture == null) throw new InvalidOperationException("No source terrain texture");
            var rect = sprite.textureRect;
            // Importer must be FullRect and centred. The overlay has to
            // reproduce the exact original pixel-to-world mapping.
            if ((sprite.pivot - new Vector2(sprite.rect.width * .5f,
                                          sprite.rect.height * .5f)).sqrMagnitude > 1f)
                throw new InvalidOperationException("Source ground pivot must be centred");

            sharedCutoutMaterial = new Material(shader) { name = "B_SameSourceGroundFore" };
            sharedCutoutMaterial.hideFlags = HideFlags.DontSave;
            sharedCutoutMaterial.mainTexture = texture;
            graphics = new GameObject("B_MurallaPrimerPlano_OriginalTexels");
            graphics.hideFlags = HideFlags.DontSave;
            graphics.transform.SetParent(transform, false);
            graphics.transform.SetPositionAndRotation(source.transform.position,
                                                     source.transform.rotation);
            graphics.transform.localScale = source.transform.lossyScale;

            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var shape in doc.regions)
            {
                if (shape == null || string.IsNullOrWhiteSpace(shape.id) ||
                    !names.Add(shape.id) || shape.contour == null ||
                    shape.contour.Length < 3 || shape.contour.Length > 14 ||
                    shape.foot_y < 0 || shape.foot_y > 1672)
                    throw new InvalidOperationException("Invalid region declaration");
                ValidateConvex(shape.contour);
                var mesh = BuildMesh(shape, sprite, rect, texture);
                mesh.name = "B_Foreground_" + shape.id;
                mesh.hideFlags = HideFlags.DontSave;
                meshes.Add(mesh);
                var item = new GameObject("B_Region_" + shape.id);
                item.hideFlags = HideFlags.DontSave;
                item.transform.SetParent(graphics.transform, false);
                item.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = item.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = sharedCutoutMaterial;
                renderer.sortingLayerID = ground.sortingLayerID;
                float localY = (.5f - shape.foot_y / 1672f) * sprite.bounds.size.y;
                var worldFoot = graphics.transform.TransformPoint(new Vector3(0f, localY, 0f));
                renderer.sortingOrder = SpatialPilotBootstrap.FootOrder(worldFoot.y);
                parts.Add(shape.id, renderer);
                if (item.GetComponent<Collider2D>() != null || item.GetComponent<Collider>() != null)
                    throw new InvalidOperationException("Foreground mask may not block taps");
                RegionCount++;
            }
        }

        private static Mesh BuildMesh(Region region, Sprite sprite, Rect rect, Texture2D tex)
        {
            var contour = region.contour;
            var vertices = new Vector3[contour.Length];
            var uv = new Vector2[contour.Length];
            var indices = new int[(contour.Length - 2) * 3];
            for (int i = 0; i < contour.Length; i++)
            {
                float x = contour[i].x / 941f, y = contour[i].y / 1672f;
                vertices[i] = new Vector3(
                    (x - .5f) * sprite.bounds.size.x,
                    (.5f - y) * sprite.bounds.size.y, 0f);
                uv[i] = new Vector2(
                    (rect.x + x * rect.width) / tex.width,
                    (rect.y + (1f - y) * rect.height) / tex.height);
                if (i >= 2)
                {
                    int j = (i - 2) * 3;
                    indices[j] = 0;
                    indices[j + 1] = i - 1;
                    indices[j + 2] = i;
                }
            }
            var mesh = new Mesh { name = "B_ExactPaintedTerrainSegment" };
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = indices;
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void ValidateConvex(Point[] points)
        {
            float sign = 0f;
            for (int i = 0; i < points.Length; ++i)
            {
                var p = points[i];
                var q = points[(i + 1) % points.Length];
                var r = points[(i + 2) % points.Length];
                if (p.x < 0 || p.x > 941 || p.y < 0 || p.y > 1672)
                    throw new InvalidOperationException("Foreground polygon outside terrain");
                float cross = (q.x-p.x) * (r.y-q.y) - (q.y-p.y) * (r.x-q.x);
                if (Mathf.Abs(cross) < 1f) continue;
                float side = Mathf.Sign(cross);
                if (sign != 0f && side != sign)
                    throw new InvalidOperationException("Foreground region must be convex");
                sign = side;
            }
            if (sign == 0f) throw new InvalidOperationException("Foreground polygon has no area");
        }

        private void OnDestroy()
        {
            if (graphics != null)
            {
                if (UnityEngine.Application.isPlaying) Destroy(graphics);
                else DestroyImmediate(graphics);
            }
            foreach (var mesh in meshes)
            {
                if (mesh == null) continue;
                if (UnityEngine.Application.isPlaying) Destroy(mesh);
                else DestroyImmediate(mesh);
            }
            if (sharedCutoutMaterial != null)
            {
                if (UnityEngine.Application.isPlaying) Destroy(sharedCutoutMaterial);
                else DestroyImmediate(sharedCutoutMaterial);
            }
        }
    }
}
