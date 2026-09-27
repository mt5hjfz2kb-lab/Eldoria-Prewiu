using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;

namespace Eldoria.EditorTools
{
    // Composition study only. All geometry comes from the three supplied GLBs.
    public static class MicroValoriaReview
    {
        const string Root = "Assets/Eldoria/ArtTests/ImageTo3D/";
        const string Tower = Root + "Source/Eldoria_Module_TowerWallRock_50K.glb";
        const string Gate = Root + "Source/Eldoria_Module_GateWallRock_50K.glb";
        const string Terrace = Root + "Source/Eldoria_Module_TerraceStairRock_50K.glb";
        const string Scene = Root + "MicroValoriaReview.unity";
        const string Output = "MicroValoriaReviewCaptures";

        [Serializable] class Entry
        {
            public string name, source;
            public int instances, sourceMeshes, sourceMaterials;
            public long sourceTriangles, sourceVertices, sourceMeshBytes;
            public bool uv0, normals;
            public Vector3 sourceBounds;
            public bool raycastHit;
        }

        [Serializable] class Report
        {
            public string scene = Scene;
            public Entry[] modules;
            public long instanceTriangles, instanceMeshBytesWithoutSharing;
            public int meshColliders;
            public bool[] captureNonEmpty = new bool[4];
            public bool[] backgroundMiss = new bool[4];
            public long[] captureMilliseconds = new long[4];
            public Vector3 districtBounds;
        }

        public static void Capture()
        {
            SceneSetup.SetupRenderPipeline();
            var assets = new[] { Tower, Gate, Terrace };
            var prefabs = new GameObject[3];
            for (int i = 0; i < 3; i++) {
                if (!File.Exists(assets[i])) throw new FileNotFoundException("Missing certified source: " + assets[i]);
                AssetDatabase.ImportAsset(assets[i], ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(assets[i]);
                if (!prefabs[i]) throw new Exception("glTFast import failed: " + assets[i]);
            }
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Micro-Valoria | isolated district experiment");
            var report = new Report { modules = new Entry[3] };
            string[] labels = { "Tower wall rock", "Gate wall rock", "Terrace stair rock" };
            for (int i = 0; i < 3; i++) report.modules[i] = Measure(prefabs[i], labels[i], assets[i]);
            // L-shaped route: entry from the south, central gate, ascending terrace,
            // tower district on the west shoulder and a second back wall on the east.
            // Bounds normalization compensates for Tripo's arbitrary metre scale.
            Place(prefabs[1], report.modules[1], root.transform, "01 Gate / southern threshold", new Vector3(0, 0, -9), 0, 15);
            Place(prefabs[2], report.modules[2], root.transform, "02 Ascending rock terrace / main climb", new Vector3(0, 0, 3.5f), 0, 19);
            Place(prefabs[0], report.modules[0], root.transform, "03 Tower / western high shoulder", new Vector3(-10.5f, 1.0f, 7.5f), 25, 14);
            Place(prefabs[0], report.modules[0], root.transform, "04 Reused tower wall / rear skyline", new Vector3(9.0f, 1.0f, 11.0f), 180, 11);
            Place(prefabs[1], report.modules[1], root.transform, "05 Reused gate wall / eastern enclosure", new Vector3(11.0f, 0, -1), 90, 11);

            var terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
            terrain.name = "Neutral ground | context only";
            terrain.transform.position = new Vector3(0, -.16f, 0);
            terrain.transform.localScale = new Vector3(7, 1, 7);
            Colorize(terrain, new Color(.37f, .36f, .32f));
            var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Southern approach | context only";
            road.transform.position = new Vector3(0, -.07f, -22);
            road.transform.localScale = new Vector3(3.4f, .12f, 16);
            Colorize(road, new Color(.29f, .28f, .25f));
            var light = new GameObject("District light").AddComponent<Light>();
            light.type = LightType.Directional; light.intensity = 1.15f;
            light.transform.rotation = Quaternion.Euler(48, -42, 0);
            RenderSettings.ambientLight = new Color(.59f, .61f, .64f);

            var district = BoundsOf(root);
            report.districtBounds = district.size;
            var cam = new GameObject("District zoom review camera").AddComponent<Camera>();
            cam.tag = "MainCamera"; cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.65f, .72f, .79f);
            Directory.CreateDirectory(Output);
            Physics.SyncTransforms();
            float[] sizes = { 19, 12, 9, 12 };
            string[] names = { "strategic", "city", "detail", "oblique" };
            for (int i = 0; i < 4; i++) {
                cam.orthographicSize = sizes[i];
                cam.transform.position = district.center + (i == 3 ? new Vector3(-25, 22, -26) : new Vector3(20, 29, -32));
                cam.transform.LookAt(district.center + Vector3.up * 1.1f);
                var result = Save(cam, Output + "/" + names[i] + ".png");
                report.captureMilliseconds[i] = result.Item1;
                report.captureNonEmpty[i] = result.Item2;
                var empty = cam.ScreenPointToRay(new Vector3(3, 716, 0));
                report.backgroundMiss[i] = !Physics.Raycast(empty, out var hit, 150) || !hit.transform.IsChildOf(root.transform);
                if (!result.Item2 || !report.backgroundMiss[i]) throw new Exception("Capture or empty-space selection gate failed: " + names[i]);
            }
            foreach (var e in report.modules) {
                if (!e.uv0 || !e.normals || e.sourceTriangles < 45000 || e.sourceTriangles > 50000)
                    throw new Exception("Source geometry/UV gate failed: " + e.name + " " + e.sourceTriangles);
                var child = root.transform.Find(e.name == labels[0] ? "03 Tower / western high shoulder" :
                    e.name == labels[1] ? "01 Gate / southern threshold" : "02 Ascending rock terrace / main climb");
                var b = BoundsOf(child.gameObject);
                e.raycastHit = Physics.Raycast(new Ray(b.center + Vector3.up * 60, Vector3.down), out var hit, 120) && hit.transform.IsChildOf(child);
                if (!e.raycastHit) throw new Exception("Module raycast gate failed: " + e.name);
                report.instanceTriangles += e.sourceTriangles * e.instances;
                report.instanceMeshBytesWithoutSharing += e.sourceMeshBytes * e.instances;
            }
            report.meshColliders = root.GetComponentsInChildren<MeshCollider>().Length;
            if (report.meshColliders < 5) throw new Exception("Missing district MeshColliders.");
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Scene);
            File.WriteAllText(Output + "/metrics.json", JsonUtility.ToJson(report, true));
            Debug.Log("Micro-Valoria isolated technical gate passed; visual assessment requires capture inspection.");
        }

        static Entry Measure(GameObject prefab, string name, string source)
        {
            var e = new Entry { name = name, source = source };
            var materials = new HashSet<Material>();
            foreach (var f in prefab.GetComponentsInChildren<MeshFilter>()) {
                var m = f.sharedMesh; if (!m) continue;
                e.sourceMeshes++; e.sourceVertices += m.vertexCount;
                e.uv0 |= m.uv.Length == m.vertexCount;
                e.normals |= m.normals.Length == m.vertexCount;
                for (int j = 0; j < m.subMeshCount; j++) e.sourceTriangles += (long)m.GetIndexCount(j) / 3;
                e.sourceMeshBytes += Profiler.GetRuntimeMemorySizeLong(m);
            }
            foreach (var r in prefab.GetComponentsInChildren<Renderer>())
                foreach (var m in r.sharedMaterials) if (m) materials.Add(m);
            e.sourceMaterials = materials.Count;
            if (e.sourceMeshes == 0 || e.sourceMaterials == 0) throw new Exception("No usable source mesh/material: " + source);
            e.sourceBounds = BoundsOf(prefab).size;
            return e;
        }

        static void Place(GameObject prefab, Entry entry, Transform root, string name, Vector3 anchor, float yaw, float width)
        {
            var go = UnityEngine.Object.Instantiate(prefab, root);
            go.name = name;
            var b = BoundsOf(go);
            var span = Mathf.Max(b.size.x, b.size.z);
            if (span < .001f) throw new Exception("Zero-sized module: " + name);
            go.transform.localScale *= width / span;
            go.transform.rotation = Quaternion.Euler(0, yaw, 0);
            b = BoundsOf(go);
            go.transform.position += anchor - new Vector3(b.center.x, b.min.y, b.center.z);
            foreach (var f in go.GetComponentsInChildren<MeshFilter>()) {
                if (!f.sharedMesh) continue;
                var collider = f.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = f.sharedMesh;
            }
            entry.instances++;
        }

        static Bounds BoundsOf(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) throw new Exception("No renderers: " + go.name);
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            return b;
        }

        static void Colorize(GameObject go, Color color)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.color = color;
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        static Tuple<long, bool> Save(Camera camera, string path)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var rt = new RenderTexture(1280, 720, 24);
            var previous = RenderTexture.active;
            try {
                camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); image.Apply();
                byte min = 255, max = 0;
                var pixels = image.GetPixels32();
                for (int i = 0; i < pixels.Length; i += 257) {
                    var p = pixels[i]; var lum = (byte)((p.r + p.g + p.b) / 3);
                    if (lum < min) min = lum; if (lum > max) max = lum;
                }
                var bytes = image.EncodeToPNG(); File.WriteAllBytes(path, bytes);
                UnityEngine.Object.DestroyImmediate(image);
                return Tuple.Create(watch.ElapsedMilliseconds, max - min >= 8 && bytes.Length >= 10000);
            } finally {
                camera.targetTexture = null; RenderTexture.active = previous;
                rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
            }
        }
    }
}
