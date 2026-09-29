using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools
{
    // Reusable isolated review for Tripo sheets that are split into connected components by Blender.
    // It never edits Valoria.unity, VisualWorld or gameplay.
    public static class TripoMultipieceReview
    {
        const string SourceFolder = "Assets/Eldoria/ArtTests/ImageTo3D/Source/MultiPiece";
        const string ScenePath = "Assets/Eldoria/ArtTests/ImageTo3D/TripoMultipieceReview.unity";
        const string OutputFolder = "TripoMultipieceReviewCaptures";

        [Serializable] class PieceMetric
        {
            public string file;
            public string name;
            public Vector3 bounds;
            public long triangles;
            public long vertices;
            public int meshes;
            public int renderers;
            public int materials;
            public int colliders;
            public bool uvPresent;
            public bool normalsPresent;
        }

        [Serializable] class Report
        {
            public int pieceCount;
            public long totalTriangles;
            public long totalVertices;
            public PieceMetric[] pieces;
            public bool overviewNonEmpty;
            public bool frontNonEmpty;
            public bool closeNonEmpty;
        }

        public static void Capture()
        {
            var files = Directory.Exists(SourceFolder)
                ? Directory.GetFiles(SourceFolder, "*.glb", SearchOption.TopDirectoryOnly)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray()
                : Array.Empty<string>();
            if (files.Length < 2)
                throw new Exception("Multipiece review requires at least two separated GLBs; found " + files.Length);

            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Directory.CreateDirectory(OutputFolder);

            var roots = new List<GameObject>();
            var metrics = new List<PieceMetric>();
            float maxHorizontal = 0f;
            float maxHeight = 0f;

            foreach (var path in files) {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) throw new Exception("glTFast did not import multipiece GLB: " + path);
                var go = UnityEngine.Object.Instantiate(prefab);
                go.name = Path.GetFileNameWithoutExtension(path);
                roots.Add(go);

                var b = BoundsOf(go);
                maxHorizontal = Mathf.Max(maxHorizontal, b.size.x, b.size.z);
                maxHeight = Mathf.Max(maxHeight, b.size.y);
            }

            if (maxHorizontal <= .0001f) throw new Exception("Multipiece assets have no usable horizontal bounds.");
            var commonScale = 4.2f / maxHorizontal;
            var cell = 6.2f;
            var cols = Mathf.Min(3, roots.Count);
            var rows = Mathf.CeilToInt(roots.Count / (float)cols);

            for (int i = 0; i < roots.Count; i++) {
                var go = roots[i];
                go.transform.localScale *= commonScale;
                var b = BoundsOf(go);
                var col = i % cols;
                var row = i / cols;
                var x = (col - (cols - 1) * .5f) * cell;
                var z = ((rows - 1) * .5f - row) * cell;
                go.transform.position += new Vector3(x - b.center.x, -b.min.y, z - b.center.z);

                int colliders = 0;
                foreach (var filter in go.GetComponentsInChildren<MeshFilter>()) {
                    if (filter.sharedMesh == null) continue;
                    var mc = filter.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = filter.sharedMesh;
                    colliders++;
                }
                metrics.Add(Measure(go, files[i], colliders));
            }

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Multipiece review floor only";
            floor.transform.localScale = new Vector3(Mathf.Max(2f, cols * .9f), 1f, Mathf.Max(2f, rows * .9f));
            floor.transform.position = new Vector3(0, -.07f, 0);
            ApplyColor(floor, new Color(.42f, .40f, .36f));

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.66f, .67f, .67f);
            RenderSettings.fog = false;
            var sun = new GameObject("Multipiece review sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.0f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = .52f;
            sun.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            var all = BoundsOfMany(roots);
            var cam = new GameObject("Multipiece official review camera").AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.62f, .65f, .67f);
            cam.transform.position = all.center + new Vector3(18f, 16f, -22f);
            cam.transform.LookAt(all.center + Vector3.up * (all.extents.y * .1f));
            cam.orthographicSize = Mathf.Max(8f, Mathf.Max(all.extents.x, all.extents.z) * 1.25f);
            var overview = Save(cam, OutputFolder + "/overview-textured.png");

            cam.transform.position = all.center + new Vector3(0f, 8f, -24f);
            cam.transform.LookAt(all.center + Vector3.up * (all.extents.y * .1f));
            cam.orthographicSize = Mathf.Max(8f, all.extents.x * 1.25f);
            var front = Save(cam, OutputFolder + "/front-diagnostic.png");

            cam.transform.position = all.center + new Vector3(14f, 10f, -18f);
            cam.transform.LookAt(all.center);
            cam.orthographicSize = Mathf.Max(5f, Mathf.Min(10f, Mathf.Max(all.extents.x, all.extents.z) * .8f));
            var close = Save(cam, OutputFolder + "/close-oblique.png");

            var report = new Report {
                pieceCount = metrics.Count,
                totalTriangles = metrics.Sum(x => x.triangles),
                totalVertices = metrics.Sum(x => x.vertices),
                pieces = metrics.ToArray(),
                overviewNonEmpty = overview,
                frontNonEmpty = front,
                closeNonEmpty = close
            };
            if (!overview || !front || !close) throw new Exception("Multipiece review produced an empty/flat capture.");
            File.WriteAllText(OutputFolder + "/metrics.json", JsonUtility.ToJson(report, true));
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);
            Debug.Log($"Tripo multipiece review complete: {report.pieceCount} pieces, {report.totalTriangles} tris");
        }

        static PieceMetric Measure(GameObject go, string file, int colliders)
        {
            var meshes = go.GetComponentsInChildren<MeshFilter>().Where(x => x.sharedMesh != null).ToArray();
            var renderers = go.GetComponentsInChildren<Renderer>();
            long tris = 0, verts = 0;
            bool uv = true, normals = true;
            var mats = new HashSet<Material>();
            foreach (var f in meshes) {
                var m = f.sharedMesh;
                verts += m.vertexCount;
                tris += m.triangles.LongLength / 3;
                uv &= m.uv != null && m.uv.Length == m.vertexCount;
                normals &= m.normals != null && m.normals.Length == m.vertexCount;
            }
            foreach (var r in renderers)
                foreach (var m in r.sharedMaterials)
                    if (m != null) mats.Add(m);
            return new PieceMetric {
                file = file.Replace('\\','/'),
                name = go.name,
                bounds = BoundsOf(go).size,
                triangles = tris,
                vertices = verts,
                meshes = meshes.Length,
                renderers = renderers.Length,
                materials = mats.Count,
                colliders = colliders,
                uvPresent = uv,
                normalsPresent = normals
            };
        }

        static Bounds BoundsOf(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
            var b = rs[0].bounds;
            for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
            return b;
        }

        static Bounds BoundsOfMany(List<GameObject> roots)
        {
            var b = BoundsOf(roots[0]);
            for (int i = 1; i < roots.Count; i++) b.Encapsulate(BoundsOf(roots[i]));
            return b;
        }

        static void ApplyColor(GameObject go, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(shader);
            mat.color = color;
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        static bool Save(Camera camera, string path)
        {
            const int width = 1280, height = 720;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            camera.targetTexture = rt;
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            camera.Render();
            tex.ReadPixels(new Rect(0,0,width,height),0,0);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            var pixels = tex.GetPixels32();
            byte min = 255, max = 0;
            for (int i = 0; i < pixels.Length; i += 97) {
                var p = pixels[i];
                var l = (byte)((p.r + p.g + p.b) / 3);
                if (l < min) min = l;
                if (l > max) max = l;
            }
            camera.targetTexture = null;
            RenderTexture.active = prev;
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(tex);
            return new FileInfo(path).Length > 10000 && (max - min) > 8;
        }
    }
}
