using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.ArtTests.ImageTo3D;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;

namespace Eldoria.EditorTools
{
    // Isolated V4 appearance review. Never touches Valoria.unity, VisualWorld or gameplay.
    public static class TripoAppearanceV4Review
    {
        const string Root = "Assets/Eldoria/ArtTests/ImageTo3D/";
        const string Source = Root + "Source/Eldoria_Module_TowerWallRock_50K_SurfaceCleanupV4.glb";
        const string Scene = Root + "TripoAppearanceV4Review.unity";
        const string Output = "TripoAppearanceV4ReviewCaptures";

        [Serializable] class Report
        {
            public string source = Source;
            public string scene = Scene;
            public float unityScale;
            public Vector3 boundsSize;
            public int meshes, renderers, materials, textures, colliders;
            public long vertices, triangles, meshRuntimeBytes, textureRuntimeBytes;
            public bool uvPresent, normalsPresent, positiveRaycast, emptySpaceDoesNotSelect;
            public string[] materialNames, textureSizes;
            public long[] captureCpuMilliseconds;
        }

        public static void Capture()
        {
            var tuple = CreateScene();
            var camera = tuple.Item1; var module = tuple.Item2; var report = tuple.Item3;
            Directory.CreateDirectory(Output);
            var sizes = new[] { 19f, 12f, 9f };
            var names = new[] { "strategic", "city", "detail" };
            report.captureCpuMilliseconds = new long[4];
            for (int i = 0; i < sizes.Length; i++) {
                camera.orthographicSize = sizes[i];
                report.captureCpuMilliseconds[i] = Save(camera, Output + "/" + names[i] + ".png");
            }
            camera.orthographicSize = 12f;
            camera.transform.position = new Vector3(-19f, 16f, -22f);
            camera.transform.LookAt(new Vector3(0, 6f, 0));
            report.captureCpuMilliseconds[3] = Save(camera, Output + "/oblique.png");

            Physics.SyncTransforms();
            var bounds = BoundsOf(module);
            var ray = new Ray(camera.transform.position, (bounds.center - camera.transform.position).normalized);
            report.positiveRaycast = Physics.Raycast(ray, out var hit, 250f) && hit.transform.IsChildOf(module.transform);
            var emptyRay = camera.ScreenPointToRay(new Vector3(4, 716, 0));
            report.emptySpaceDoesNotSelect = !Physics.Raycast(emptyRay, out var emptyHit, 250f) || !emptyHit.transform.IsChildOf(module.transform);
            if (!report.positiveRaycast || !report.emptySpaceDoesNotSelect)
                throw new Exception("V4 appearance raycast acceptance failed.");
            if (report.triangles != 50000 || report.materials != 3 || report.textures != 3)
                throw new Exception($"V4 appearance metric mismatch: tris={report.triangles} mats={report.materials} textures={report.textures}");
            File.WriteAllText(Output + "/metrics.json", JsonUtility.ToJson(report, true));
            Debug.Log($"V4 appearance review complete: {report.triangles} tris, {report.materials} materials, {report.textures} textures.");
        }

        static Tuple<Camera, GameObject, Report> CreateScene()
        {
            if (!File.Exists(Source)) throw new FileNotFoundException("Owner V4 module missing at " + Source);
            SceneSetup.SetupRenderPipeline();
            AssetDatabase.ImportAsset(Source, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Source);
            if (prefab == null) throw new Exception("glTFast did not import " + Source);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var module = UnityEngine.Object.Instantiate(prefab);
            module.name = "Eldoria Tower Wall Rock 50K SurfaceCleanupV4";
            var b = BoundsOf(module);
            var span = Mathf.Max(b.size.x, b.size.z);
            if (span <= .001f) throw new Exception("Imported V4 module has no usable bounds.");
            float scale = 14f / span;
            module.transform.localScale *= scale;
            b = BoundsOf(module);
            module.transform.position += new Vector3(-b.center.x, -b.min.y, -b.center.z);

            int colliders = 0;
            foreach (var f in module.GetComponentsInChildren<MeshFilter>()) {
                if (f.sharedMesh == null) continue;
                var c = f.gameObject.AddComponent<MeshCollider>(); c.sharedMesh = f.sharedMesh; colliders++;
            }

            var terrain = GameObject.CreatePrimitive(PrimitiveType.Plane);
            terrain.name = "Review terrain only";
            terrain.transform.localScale = new Vector3(4f, 1f, 4f);
            terrain.transform.position = new Vector3(0, -.08f, 0);
            ApplyColor(terrain, new Color(.38f, .36f, .29f));

            var road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = "Review road only";
            road.transform.position = new Vector3(0, .015f, -10f);
            road.transform.localScale = new Vector3(3.2f, .08f, 12f);
            ApplyColor(road, new Color(.28f, .27f, .24f));

            var sun = new GameObject("Review light").AddComponent<Light>();
            sun.type = LightType.Directional; sun.intensity = 1.35f;
            sun.transform.rotation = Quaternion.Euler(42, -38, 0);
            RenderSettings.ambientLight = new Color(.58f, .61f, .64f);

            var camera = new GameObject("Official zoom review camera").AddComponent<Camera>();
            camera.tag = "MainCamera"; camera.orthographic = true; camera.orthographicSize = 12f;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.64f, .72f, .8f);
            camera.transform.position = new Vector3(16f, 13f, -22f);
            camera.transform.LookAt(new Vector3(0, 6f, 0));
            var probe = camera.gameObject.AddComponent<BastionSelectionProbe>();
            probe.ReviewCamera = camera; probe.BastionRoot = module.transform;

            var report = Measure(module, scale); report.colliders = colliders;
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Scene);
            return Tuple.Create(camera, module, report);
        }

        static void ApplyColor(GameObject go, Color color) {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit")); m.color = color;
            go.GetComponent<Renderer>().sharedMaterial = m;
        }

        static Bounds BoundsOf(GameObject go) {
            var rs = go.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) throw new Exception("Imported V4 module has no renderer.");
            var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds); return b;
        }

        static Report Measure(GameObject module, float scale)
        {
            var report = new Report { unityScale = scale, boundsSize = BoundsOf(module).size };
            var mats = new HashSet<Material>(); var texs = new HashSet<Texture>();
            var sizes = new List<string>(); var matNames = new List<string>();
            foreach (var f in module.GetComponentsInChildren<MeshFilter>()) {
                var mesh = f.sharedMesh; if (mesh == null) continue;
                report.meshes++; report.vertices += mesh.vertexCount;
                report.uvPresent |= mesh.uv != null && mesh.uv.Length == mesh.vertexCount;
                report.normalsPresent |= mesh.normals != null && mesh.normals.Length == mesh.vertexCount;
                for (int s = 0; s < mesh.subMeshCount; s++) report.triangles += (long)mesh.GetIndexCount(s) / 3;
                report.meshRuntimeBytes += Profiler.GetRuntimeMemorySizeLong(mesh);
            }
            foreach (var r in module.GetComponentsInChildren<Renderer>()) {
                report.renderers++;
                foreach (var m in r.sharedMaterials) {
                    if (m == null || !mats.Add(m)) continue;
                    matNames.Add(m.name);
                    foreach (var prop in m.GetTexturePropertyNames()) {
                        var t = m.GetTexture(prop); if (t == null || !texs.Add(t)) continue;
                        sizes.Add($"{t.name}: {t.width}x{t.height}");
                        if (t.name.ToLowerInvariant().Contains("basecolor") && (t.width != 1024 || t.height != 1024))
                            throw new Exception($"V4 BaseColor texture import regression: {t.name} is {t.width}x{t.height}, expected 1024x1024.");
                        report.textureRuntimeBytes += Profiler.GetRuntimeMemorySizeLong(t);
                    }
                }
            }
            report.materials = mats.Count; report.textures = texs.Count;
            report.materialNames = matNames.ToArray(); report.textureSizes = sizes.ToArray();
            if (report.triangles == 0 || !report.uvPresent || !report.normalsPresent || report.materials == 0)
                throw new Exception("V4 GLB import lost required geometry/UV0/normals/material.");
            return report;
        }

        static long Save(Camera camera, string path)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var rt = new RenderTexture(1280,720,24); var previous = RenderTexture.active;
            try {
                camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                var image = new Texture2D(1280,720,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,1280,720),0,0); image.Apply();
                File.WriteAllBytes(path,image.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(image);
            } finally {
                camera.targetTexture=null; RenderTexture.active=previous; rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
            }
            sw.Stop(); return sw.ElapsedMilliseconds;
        }
    }
}
