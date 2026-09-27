using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;

namespace Eldoria.EditorTools
{
    // Canonical isolated review for any externally-generated Tripo module.
    // Never touches Valoria.unity, VisualWorld or gameplay.
    public static class TripoGenericModuleReview
    {
        const string Root = "Assets/Eldoria/ArtTests/ImageTo3D/";
        const string Source = Root + "Source/Eldoria_Module_UnderReview_50K.glb";
        const string Scene = Root + "TripoGenericModuleReview.unity";
        const string Output = "TripoGenericModuleReviewCaptures";
        const long MinTriangles = 49500;
        const long MaxTriangles = 50000;

        [Serializable] class Report
        {
            public string source = Source;
            public string scene = Scene;
            public float unityScale;
            public Vector3 boundsSize;
            public int meshes, renderers, materials, textures, colliders;
            public long vertices, triangles, meshRuntimeBytes, textureRuntimeBytes;
            public bool uvPresent, normalsPresent, positiveRaycast, emptySpaceDoesNotSelect;
            public bool[] captureNonEmpty;
            public string[] textureSizes;
            public long[] captureCpuMilliseconds;
        }

        public static void Capture()
        {
            var tuple = CreateScene();
            var camera = tuple.Item1;
            var module = tuple.Item2;
            var report = tuple.Item3;
            ApplyDiagnosticClay(module);
            Directory.CreateDirectory(Output);

            var sizes = new[] { 19f, 12f, 9f };
            var names = new[] { "strategic", "city", "detail" };
            report.captureCpuMilliseconds = new long[8];
            report.captureNonEmpty = new bool[8];

            for (int i = 0; i < sizes.Length; i++) {
                camera.orthographicSize = sizes[i];
                var saved = Save(camera, Output + "/" + names[i] + ".png");
                report.captureCpuMilliseconds[i] = saved.Item1;
                report.captureNonEmpty[i] = saved.Item2;
            }

            var bounds = BoundsOf(module);

            camera.orthographicSize = 12f;
            camera.transform.position = bounds.center + new Vector3(-22f, 15f, -25f);
            camera.transform.LookAt(bounds.center + Vector3.up * (bounds.extents.y * .15f));
            Store(report, 3, Save(camera, Output + "/oblique.png"));

            camera.orthographicSize = 9f;
            camera.transform.position = bounds.center + new Vector3(0f, 5f, -24f);
            camera.transform.LookAt(bounds.center + Vector3.up * (bounds.extents.y * .08f));
            Store(report, 4, Save(camera, Output + "/front-diagnostic.png"));

            camera.transform.position = bounds.center + new Vector3(0f, 5f, 24f);
            camera.transform.LookAt(bounds.center + Vector3.up * (bounds.extents.y * .08f));
            Store(report, 5, Save(camera, Output + "/rear-diagnostic.png"));

            camera.transform.position = bounds.center + new Vector3(-24f, 5f, 0f);
            camera.transform.LookAt(bounds.center + Vector3.up * (bounds.extents.y * .08f));
            Store(report, 6, Save(camera, Output + "/left-diagnostic.png"));

            camera.transform.position = bounds.center + new Vector3(24f, 5f, 0f);
            camera.transform.LookAt(bounds.center + Vector3.up * (bounds.extents.y * .08f));
            Store(report, 7, Save(camera, Output + "/right-diagnostic.png"));

            Physics.SyncTransforms();
            bounds = BoundsOf(module);
            var ray = new Ray(camera.transform.position, (bounds.center - camera.transform.position).normalized);
            report.positiveRaycast = Physics.Raycast(ray, out var hit, 300f) && hit.transform.IsChildOf(module.transform);
            var emptyRay = camera.ScreenPointToRay(new Vector3(4, 716, 0));
            report.emptySpaceDoesNotSelect = !Physics.Raycast(emptyRay, out var emptyHit, 300f) || !emptyHit.transform.IsChildOf(module.transform);

            if (!report.positiveRaycast || !report.emptySpaceDoesNotSelect)
                throw new Exception("Generic Tripo module raycast acceptance failed.");
            foreach (var ok in report.captureNonEmpty)
                if (!ok) throw new Exception("Generic Tripo module produced an empty/flat capture.");

            File.WriteAllText(Output + "/metrics.json", JsonUtility.ToJson(report, true));
            Debug.Log($"Generic Tripo module review complete: {report.triangles} tris, {report.vertices} verts, {report.meshes} meshes, UV={report.uvPresent}, materials={report.materials}");
        }

        static void Store(Report report, int index, Tuple<long,bool> saved)
        {
            report.captureCpuMilliseconds[index] = saved.Item1;
            report.captureNonEmpty[index] = saved.Item2;
        }

        static Tuple<Camera, GameObject, Report> CreateScene()
        {
            if (!File.Exists(Source)) throw new FileNotFoundException("Canonical Tripo source missing at " + Source);
            SceneSetup.SetupRenderPipeline();
            AssetDatabase.ImportAsset(Source, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Source);
            if (prefab == null) throw new Exception("glTFast did not import " + Source);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var module = UnityEngine.Object.Instantiate(prefab);
            module.name = "Eldoria Tripo Module Under Review 50K";

            var b = BoundsOf(module);
            var span = Mathf.Max(b.size.x, b.size.z);
            if (span <= .001f) throw new Exception("Imported module has no usable bounds.");
            float scale = 18f / span;
            module.transform.localScale *= scale;
            b = BoundsOf(module);
            module.transform.position += new Vector3(-b.center.x, -b.min.y, -b.center.z);

            int colliders = 0;
            foreach (var f in module.GetComponentsInChildren<MeshFilter>()) {
                if (f.sharedMesh == null) continue;
                var c = f.gameObject.AddComponent<MeshCollider>();
                c.sharedMesh = f.sharedMesh;
                colliders++;
            }

            var terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
            terrain.name = "Review terrain only";
            terrain.transform.localScale = new Vector3(5f, 1f, 5f);
            terrain.transform.position = new Vector3(0, -.08f, 0);
            ApplyColor(terrain, new Color(.38f, .36f, .29f));

            var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Review road only";
            road.transform.position = new Vector3(0, .015f, -13f);
            road.transform.localScale = new Vector3(3.6f, .08f, 14f);
            ApplyColor(road, new Color(.28f, .27f, .24f));

            var sun = new GameObject("Review light").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.05f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(42, -38, 0);
            RenderSettings.ambientLight = new Color(.58f, .61f, .64f);

            var bounds = BoundsOf(module);
            var camera = new GameObject("Official zoom review camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 12f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.64f, .72f, .8f);
            camera.transform.position = bounds.center + new Vector3(18f, 14f, -25f);
            camera.transform.LookAt(bounds.center + Vector3.up * (bounds.extents.y * .10f));

            var report = Measure(module, scale);
            report.colliders = colliders;
            if (report.triangles < MinTriangles || report.triangles > MaxTriangles)
                throw new Exception($"Generic Tripo triangle gate failed: {report.triangles}; expected {MinTriangles}-{MaxTriangles}.");

            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Scene);
            return Tuple.Create(camera, module, report);
        }

        static void ApplyColor(GameObject go, Color color)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.color = color;
            go.GetComponent<Renderer>().sharedMaterial = m;
        }

        static void ApplyDiagnosticClay(GameObject go)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(.48f, .47f, .43f);
            foreach (var renderer in go.GetComponentsInChildren<Renderer>()) {
                var slots = renderer.sharedMaterials;
                for (int i = 0; i < slots.Length; i++) slots[i] = material;
                renderer.sharedMaterials = slots;
            }
        }

        static Bounds BoundsOf(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) throw new Exception("Imported module has no renderer.");
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        static Report Measure(GameObject module, float scale)
        {
            var report = new Report { unityScale = scale, boundsSize = BoundsOf(module).size };
            var mats = new HashSet<Material>();
            var texs = new HashSet<Texture>();
            var sizes = new List<string>();

            foreach (var f in module.GetComponentsInChildren<MeshFilter>()) {
                var mesh = f.sharedMesh;
                if (mesh == null) continue;
                report.meshes++;
                report.vertices += mesh.vertexCount;
                report.uvPresent |= mesh.uv != null && mesh.uv.Length == mesh.vertexCount;
                report.normalsPresent |= mesh.normals != null && mesh.normals.Length == mesh.vertexCount;
                for (int s = 0; s < mesh.subMeshCount; s++)
                    report.triangles += (long)mesh.GetIndexCount(s) / 3;
                report.meshRuntimeBytes += Profiler.GetRuntimeMemorySizeLong(mesh);
            }

            foreach (var r in module.GetComponentsInChildren<Renderer>()) {
                report.renderers++;
                foreach (var m in r.sharedMaterials) {
                    if (m == null || !mats.Add(m)) continue;
                    foreach (var prop in m.GetTexturePropertyNames()) {
                        var t = m.GetTexture(prop);
                        if (t == null || !texs.Add(t)) continue;
                        sizes.Add($"{t.name}: {t.width}x{t.height}");
                        report.textureRuntimeBytes += Profiler.GetRuntimeMemorySizeLong(t);
                    }
                }
            }

            report.materials = mats.Count;
            report.textures = texs.Count;
            report.textureSizes = sizes.ToArray();
            if (report.triangles == 0 || !report.uvPresent || !report.normalsPresent || report.materials == 0)
                throw new Exception("GLB import lost required geometry/UV0/normals/material.");
            return report;
        }

        static Tuple<long,bool> Save(Camera camera, string path)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var rt = new RenderTexture(1280, 720, 24);
            var previous = RenderTexture.active;
            bool nonEmpty = false;
            try {
                camera.targetTexture = rt;
                camera.Render();
                RenderTexture.active = rt;
                var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0,0,1280,720), 0, 0);
                image.Apply();

                var pixels = image.GetPixels32();
                byte min = 255, max = 0;
                for (int i = 0; i < pixels.Length; i += 257) {
                    var p = pixels[i];
                    byte l = (byte)((p.r + p.g + p.b) / 3);
                    if (l < min) min = l;
                    if (l > max) max = l;
                }
                nonEmpty = (max - min) >= 8;
                var png = image.EncodeToPNG();
                if (png.Length < 10000) nonEmpty = false;
                File.WriteAllBytes(path, png);
                UnityEngine.Object.DestroyImmediate(image);
            }
            finally {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }

            sw.Stop();
            return Tuple.Create(sw.ElapsedMilliseconds, nonEmpty);
        }
    }
}
