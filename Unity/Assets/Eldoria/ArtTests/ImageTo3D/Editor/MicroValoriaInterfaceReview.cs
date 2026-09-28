using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;

namespace Eldoria.EditorTools
{
    // Composition study only. Architecture comes from exactly six certified GLB families.
    public static class MicroValoriaInterfaceReview
    {
        const string Root = "Assets/Eldoria/ArtTests/ImageTo3D/";
        const string Tower = Root + "Source/Eldoria_Module_TowerWallRock_InterfaceV1.glb";
        const string Terrace = Root + "Source/Eldoria_Module_TerraceStairRock_InterfaceV1.glb";
        const string Gate = Root + "Source/Eldoria_Module_GateStreetRiseRock_InterfaceV1.glb";
        const string Residential = Root + "Source/Eldoria_Module_ResidentialTerraceRock_InterfaceV1.glb";
        const string Street = Root + "Source/Eldoria_Module_StreetLandingTransition_InterfaceV1.glb";
        const string Filler = Root + "Source/Eldoria_Module_RockTerrainSeamFiller_InterfaceV1.glb";
        const string Scene = Root + "MicroValoriaInterfaceReview.unity";
        const string Output = "MicroValoriaInterfaceReviewCaptures";

        [Serializable] class Entry
        {
            public string name, source;
            public int instances, sourceMeshes, sourceMaterials, socketCount;
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
            public float gateToStreetError, streetToTerraceError;
        }

        public static void Capture()
        {
            SceneSetup.SetupRenderPipeline();
            var assets = new[] { Tower, Terrace, Gate, Residential, Street, Filler };
            var prefabs = new GameObject[6];
            for (int i = 0; i < assets.Length; i++) {
                if (!File.Exists(assets[i])) throw new FileNotFoundException("Missing certified source: " + assets[i]);
                AssetDatabase.ImportAsset(assets[i], ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(assets[i]);
                if (!prefabs[i]) throw new Exception("glTFast import failed: " + assets[i]);
            }
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Micro-Valoria 2 | interface standard v1 review");
            var report = new Report { modules = new Entry[6] };
            string[] labels = { "Tower wall rock", "Terrace stair rock", "Gate street rise rock MV1", "Residential terrace rock", "Street landing transition", "Rock terrain seam filler" };
            for (int i = 0; i < assets.Length; i++) report.modules[i] = Measure(prefabs[i], labels[i], assets[i]);

            // Align measured walk surfaces; base bottoms are not functional pivots.
            // Source +Z is the lower end on the circulation pieces, so yaw 180
            // makes the route ascend from the southern camera toward +Z world.
            var gate = PlaceAtSocket(prefabs[2], report.modules[2], root.transform, "01 Southern gate", "Street_In", new Vector3(0, .15f, -13.4f), 180, 15.0f);
            var gateOut = Socket(gate.transform, "Street_Out");
            var street = PlaceAtSocket(prefabs[4], report.modules[4], root.transform, "02 Street landing", "Street_In", gateOut.position + new Vector3(0, 0, .25f), 180, 8.2f);
            var streetOut = Socket(street.transform, "Street_Out");
            var rise = PlaceAtSocket(prefabs[1], report.modules[1], root.transform, "04 Eastern rising terrace", "Street_In", streetOut.position + new Vector3(0, 0, .20f), 180, 11.3f);
            PlaceAtSocket(prefabs[3], report.modules[3], root.transform, "03 Western dwellings", "Frontage_L1", new Vector3(-3.7f, streetOut.position.y - .2f, 2.6f), 180, 11.5f);
            PlaceAtSocket(prefabs[3], report.modules[3], root.transform, "05 Upper eastern dwellings", "Frontage_L1", new Vector3(3.7f, Socket(rise.transform, "Terrace_L2").position.y - .2f, 9.2f), 180, 10.8f);
            PlaceAtSocket(prefabs[1], report.modules[1], root.transform, "06 Northwestern terrace", "Street_In", new Vector3(-2.8f, streetOut.position.y, 8.4f), 180, 10.5f);
            Place(prefabs[0], report.modules[0], root.transform, "07 Rear skyline tower", new Vector3(-4.4f, 4.5f, 14.5f), 55, 7.2f);
            Place(prefabs[5], report.modules[5], root.transform, "08 Central supporting rock", new Vector3(0, .05f, .9f), 180, 11.8f);
            Place(prefabs[5], report.modules[5], root.transform, "09 West supporting rock", new Vector3(-4.1f, 1.4f, 5.5f), 75, 11.2f);
            Place(prefabs[5], report.modules[5], root.transform, "10 Rear supporting rock", new Vector3(1.6f, 3.0f, 10.2f), 190, 10.5f);
            report.gateToStreetError = Vector3.Distance(gateOut.position + new Vector3(0, 0, .25f), Socket(street.transform, "Street_In").position);
            report.streetToTerraceError = Vector3.Distance(streetOut.position + new Vector3(0, 0, .20f), Socket(rise.transform, "Street_In").position);
            if (report.gateToStreetError > .01f || report.streetToTerraceError > .01f)
                throw new Exception("Functional sockets failed to align.");

            var terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
            terrain.name = "Neutral ground | context only";
            terrain.transform.position = new Vector3(0, -.16f, 0);
            terrain.transform.localScale = new Vector3(7, 1, 7);
            Colorize(terrain, new Color(.37f, .36f, .32f));
            var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Southern approach | context only";
            road.transform.position = new Vector3(0, -.07f, -16);
            road.transform.localScale = new Vector3(3.2f, .12f, 14);
            Colorize(road, new Color(.29f, .28f, .25f));
            var light = new GameObject("District light").AddComponent<Light>();
            light.type = LightType.Directional; light.intensity = .68f; light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(48, -42, 0);
            RenderSettings.ambientLight = new Color(.28f, .30f, .32f);

            ApplyDistrictClay(root);

            var district = BoundsOf(root);
            report.districtBounds = district.size;
            var cam = new GameObject("District zoom review camera").AddComponent<Camera>();
            cam.tag = "MainCamera"; cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.65f, .72f, .79f);
            Directory.CreateDirectory(Output);
            Physics.SyncTransforms();
            float[] sizes = { 19, 12, 9, 16 };
            string[] names = { "strategic", "city", "detail", "oblique" };
            for (int i = 0; i < 4; i++) {
                cam.orthographicSize = sizes[i];
                var target = i == 0 ? district.center + Vector3.up :
                    i == 1 ? district.center :
                    i == 2 ? new Vector3(0, district.center.y - .8f, 2) :
                    district.center + Vector3.up;
                cam.transform.position = target + (i == 3 ? new Vector3(-25, 20, -26) :
                    i == 0 ? new Vector3(19, 27, -30) : new Vector3(17, 25, -27));
                cam.transform.LookAt(target);
                var result = Save(cam, Output + "/" + names[i] + ".png");
                report.captureMilliseconds[i] = result.Item1;
                report.captureNonEmpty[i] = result.Item2;
                var empty = cam.ScreenPointToRay(new Vector3(3, 716, 0));
                report.backgroundMiss[i] = !Physics.Raycast(empty, out var hit, 150) || !hit.transform.IsChildOf(root.transform);
                if (!result.Item2 || !report.backgroundMiss[i]) throw new Exception("Capture or empty-space selection gate failed: " + names[i]);
            }
            string[] probeNames = {
                "07 Rear skyline tower", "04 Eastern rising terrace", "01 Southern gate",
                "03 Western dwellings", "02 Street landing", "08 West buried seam"
            };
            for (int i = 0; i < report.modules.Length; i++) {
                var e = report.modules[i];
                if (!e.uv0 || !e.normals || e.sourceTriangles < 45000 || e.sourceTriangles > 50000)
                    throw new Exception("Source geometry/UV gate failed: " + e.name + " " + e.sourceTriangles);
                var child = root.transform.Find(probeNames[i]);
                e.raycastHit = ProbeActualMesh(child);
                if (!e.raycastHit) throw new Exception("Module raycast gate failed: " + e.name);
                report.instanceTriangles += e.sourceTriangles * e.instances;
                report.instanceMeshBytesWithoutSharing += e.sourceMeshBytes * e.instances;
            }
            report.meshColliders = root.GetComponentsInChildren<MeshCollider>().Length;
            if (report.meshColliders < 10) throw new Exception("Missing district MeshColliders.");
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Scene);
            File.WriteAllText(Output + "/metrics.json", JsonUtility.ToJson(report, true));
            Debug.Log("Interface v1 isolated technical gate passed; visual assessment requires capture inspection.");
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
            foreach (var t in prefab.GetComponentsInChildren<Transform>(true))
                if (t.name.StartsWith("Socket_", StringComparison.Ordinal)) e.socketCount++;
            if (e.socketCount < 2) throw new Exception("No explicit interface sockets: " + name);
            return e;
        }

        static Transform Socket(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == "Socket_" + name) return t;
            throw new Exception("Missing socket " + name + " on " + root.name);
        }

        static GameObject PlaceAtSocket(GameObject prefab, Entry entry, Transform root, string name,
                                        string socketName, Vector3 worldTarget, float yaw, float width)
        {
            var go = Place(prefab, entry, root, name, Vector3.zero, yaw, width);
            go.transform.position += worldTarget - Socket(go.transform, socketName).position;
            return go;
        }

        static GameObject Place(GameObject prefab, Entry entry, Transform root, string name, Vector3 anchor, float yaw, float width)
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
            return go;
        }

        static Bounds BoundsOf(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) throw new Exception("No renderers: " + go.name);
            var b = renderers[0].bounds;
            foreach (var r in renderers) b.Encapsulate(r.bounds);
            return b;
        }

        static void ApplyDistrictClay(GameObject root)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(.50f, .49f, .46f);
            mat.SetFloat("_Smoothness", .18f);
            foreach (var renderer in root.GetComponentsInChildren<Renderer>()) {
                var slots = renderer.sharedMaterials;
                for (int i = 0; i < slots.Length; i++) slots[i] = mat;
                renderer.sharedMaterials = slots;
            }
        }

        static void Colorize(GameObject go, Color color)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.color = color;
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        static bool ProbeActualMesh(Transform child)
        {
            foreach (var collider in child.GetComponentsInChildren<MeshCollider>()) {
                var mesh = collider.sharedMesh;
                if (!mesh || mesh.triangles.Length < 3) continue;
                var vertices = mesh.vertices;
                var triangles = mesh.triangles;
                for (int i = 0; i + 2 < Mathf.Min(triangles.Length, 120); i += 3) {
                    var a = collider.transform.TransformPoint(vertices[triangles[i]]);
                    var b = collider.transform.TransformPoint(vertices[triangles[i + 1]]);
                    var c = collider.transform.TransformPoint(vertices[triangles[i + 2]]);
                    var n = Vector3.Cross(b - a, c - a).normalized;
                    if (n.sqrMagnitude < .5f) continue;
                    var center = (a + b + c) / 3;
                    if (collider.Raycast(new Ray(center + n * .3f, -n), out var hit, .6f)) return true;
                    if (collider.Raycast(new Ray(center - n * .3f, n), out hit, .6f)) return true;
                }
            }
            return false;
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
