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
    // Isolated GLB review; the source GLB is imported by glTFast's ScriptedImporter.
    public static class BastionImageTo3DReview
    {
        const string Root = "Assets/Eldoria/ArtTests/ImageTo3D/";
        const string Source = Root + "Source/Bastion_Optimized_v1.glb";
        const string Scene = Root + "BastionImageTo3DReview.unity";
        const string Output = "ImageTo3DReviewCaptures";

        [Serializable]
        class Report
        {
            public string source = Source;
            public string scene = Scene;
            public float unityScale;
            public Vector3 boundsSizeMeters;
            public int meshes;
            public int renderers;
            public long vertices;
            public long triangles;
            public int materials;
            public int textures;
            public long meshRuntimeBytes;
            public long textureRuntimeBytes;
            public bool uvPresent;
            public bool normalsPresent;
            public bool positiveRaycast;
            public bool emptySpaceDoesNotSelect;
            public string[] textureSizes;
            public long[] captureCpuMilliseconds;
        }

        [MenuItem("Eldoria/Art gate/Open isolated image-to-3D Bastion review")]
        public static void Open()
        {
            CreateScene();
            EditorSceneManager.OpenScene(Scene);
        }

        public static void Capture()
        {
            var (camera, bastion, report) = CreateScene();
            Directory.CreateDirectory(Output);
            var sizes = new[] { 19f, 12f, 9f };
            var names = new[] { "strategic", "city", "detail" };
            report.captureCpuMilliseconds = new long[4];
            for (int i = 0; i < sizes.Length; i++)
            {
                camera.orthographicSize = sizes[i];
                report.captureCpuMilliseconds[i] = Save(camera, Output + "/" + names[i] + ".png");
            }
            camera.orthographicSize = 12f;
            camera.transform.position = new Vector3(-23, 18, -26);
            camera.transform.LookAt(new Vector3(0, 7, 0));
            report.captureCpuMilliseconds[3] = Save(camera, Output + "/oblique.png");

            Physics.SyncTransforms();
            var bounds = BoundsOf(bastion);
            var ray = new Ray(camera.transform.position, (bounds.center - camera.transform.position).normalized);
            report.positiveRaycast = Physics.Raycast(ray, out var hit, 250f) && hit.transform.IsChildOf(bastion.transform);
            var emptyRay = camera.ScreenPointToRay(new Vector3(0, 700, 0));
            report.emptySpaceDoesNotSelect = !Physics.Raycast(emptyRay, out var emptyHit, 250f) ||
                !emptyHit.transform.IsChildOf(bastion.transform);
            if (!report.positiveRaycast || !report.emptySpaceDoesNotSelect)
                throw new Exception("Bastion 3D raycast acceptance failed: " +
                    report.positiveRaycast + "/" + report.emptySpaceDoesNotSelect);
            File.WriteAllText(Output + "/metrics.json", JsonUtility.ToJson(report, true));
            Debug.Log("Isolated Tripo GLB review completed: " + Path.GetFullPath(Output) +
                " triangles=" + report.triangles + " textures=" + report.textures);
        }

        static (Camera, GameObject, Report) CreateScene()
        {
            if (!File.Exists(Source)) throw new FileNotFoundException("The owner's optimized GLB must be at " + Source);
            SceneSetup.SetupRenderPipeline();
            AssetDatabase.ImportAsset(Source, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Source);
            if (prefab == null) throw new Exception("glTFast did not import a GameObject from " + Source);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bastion = UnityEngine.Object.Instantiate(prefab);
            bastion.name = "Bastion Optimized v1 (real Tripo geometry)";
            var b = BoundsOf(bastion);
            var span = Mathf.Max(b.size.x, b.size.z);
            if (span <= .001f) throw new Exception("GLB has no usable mesh bounds.");
            float scale = 22f / span;
            bastion.transform.localScale *= scale;
            b = BoundsOf(bastion);
            bastion.transform.position += new Vector3(-b.center.x, -b.min.y, -b.center.z);

            int colliderCount = 0;
            foreach (var filter in bastion.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh == null) continue;
                var collider = filter.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = filter.sharedMesh;
                colliderCount++;
            }
            if (colliderCount == 0) throw new Exception("No real mesh available for MeshCollider.");
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Review ground (not a Bastion replacement)";
            ground.transform.localScale = new Vector3(4, 1, 4);
            ground.transform.position = new Vector3(0, -.06f, 0);
            var groundMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            groundMaterial.color = new Color(.43f, .42f, .36f);
            ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;

            var sun = new GameObject("Review light").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.4f;
            sun.transform.rotation = Quaternion.Euler(40, -35, 0);
            RenderSettings.ambientLight = new Color(.6f, .63f, .68f);
            var camera = new GameObject("Three zoom review camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 12;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.64f, .72f, .8f);
            camera.transform.position = new Vector3(18.2f, 14.6f, -25.8f);
            camera.transform.LookAt(new Vector3(0, 7, 0));
            var probe = camera.gameObject.AddComponent<BastionSelectionProbe>();
            probe.ReviewCamera = camera;
            probe.BastionRoot = bastion.transform;
            var report = Measure(bastion, scale);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Scene);
            return (camera, bastion, report);
        }

        static Bounds BoundsOf(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) throw new Exception("Imported GLB has no renderer.");
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        static Report Measure(GameObject bastion, float scale)
        {
            var report = new Report { unityScale = scale, boundsSizeMeters = BoundsOf(bastion).size };
            var materialSet = new HashSet<Material>();
            var textureSet = new HashSet<Texture>();
            var textureSizes = new List<string>();
            foreach (var filter in bastion.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = filter.sharedMesh;
                if (mesh == null) continue;
                report.meshes++;
                report.vertices += mesh.vertexCount;
                report.uvPresent |= mesh.uv != null && mesh.uv.Length == mesh.vertexCount;
                report.normalsPresent |= mesh.normals != null && mesh.normals.Length == mesh.vertexCount;
                for (int sub = 0; sub < mesh.subMeshCount; sub++) report.triangles += (long)mesh.GetIndexCount(sub) / 3;
                report.meshRuntimeBytes += Profiler.GetRuntimeMemorySizeLong(mesh);
            }
            foreach (var renderer in bastion.GetComponentsInChildren<Renderer>())
            {
                report.renderers++;
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null || !materialSet.Add(material)) continue;
                    foreach (var name in material.GetTexturePropertyNames())
                    {
                        var tex = material.GetTexture(name);
                        if (tex == null || !textureSet.Add(tex)) continue;
                        textureSizes.Add(tex.name + ": " + tex.width + "x" + tex.height);
                        report.textureRuntimeBytes += Profiler.GetRuntimeMemorySizeLong(tex);
                    }
                }
            }
            report.materials = materialSet.Count;
            report.textures = textureSet.Count;
            report.textureSizes = textureSizes.ToArray();
            if (report.triangles == 0 || !report.uvPresent || !report.normalsPresent || report.materials == 0)
                throw new Exception("GLB import lost geometry, UV, normals or materials.");
            return report;
        }

        static long Save(Camera camera, string path)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var render = new RenderTexture(1280, 720, 24);
            var previous = RenderTexture.active;
            try
            {
                camera.targetTexture = render;
                camera.Render();
                RenderTexture.active = render;
                var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image);
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                render.Release();
                UnityEngine.Object.DestroyImmediate(render);
            }
            watch.Stop();
            return watch.ElapsedMilliseconds; // Editor capture wall time, not device FPS.
        }
    }
}
