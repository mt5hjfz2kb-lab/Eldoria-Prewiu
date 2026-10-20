using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Reuses Eldoria's tested GLB/2K isolated review strategy; never loads main Valoria scenes.
public static class TripoAssetMinimalReview
{
    [Serializable] sealed class Metrics
    {
        public string asset;
        public string unityVersion;
        public string source = "Assets/TripoSubject.glb";
        public int meshes, renderers, materials, textures;
        public long triangles, vertices;
        public int importedBaseColorWidth, importedBaseColorHeight;
        public bool uv0AllMeshes = true, normalsAllMeshes = true, fullResolutionTexturePass;
        public bool positiveSelection, emptySelectionMiss;
        public Vector3 worldBounds;
        public string[] captures;
        public string status = "UNITY_TECH_ONLY_ART_REVIEW_PENDING";
    }

    static Bounds BoundsOf(GameObject go)
    {
        var rr = go.GetComponentsInChildren<Renderer>();
        if (rr.Length == 0) throw new Exception("Imported model has no renderers");
        var b = rr[0].bounds;
        foreach (var r in rr) b.Encapsulate(r.bounds);
        return b;
    }

    static Texture BaseColorTexture(Material m)
    {
        if (m == null) return null;
        foreach (string name in m.GetTexturePropertyNames())
        {
            string k = name.ToLowerInvariant();
            if (k.Contains("basecolor") || k.Contains("basemap") || k.Contains("maintex") || k.Contains("diffuse"))
            {
                var t = m.GetTexture(name);
                if (t != null) return t;
            }
        }
        return m.mainTexture;
    }

    static void PreviewMaterials(GameObject go)
    {
        var shader = Shader.Find("Standard");
        if (shader == null) throw new Exception("Unity Standard shader missing in minimal QA");
        var changed = new Dictionary<Material, Material>();
        foreach (var rr in go.GetComponentsInChildren<Renderer>())
        {
            var ms = rr.sharedMaterials;
            for (int i = 0; i < ms.Length; i++)
            {
                var source = ms[i];
                if (source == null) throw new Exception("Null imported material");
                if (!changed.TryGetValue(source, out var target))
                {
                    var baseColor = BaseColorTexture(source);
                    if (baseColor == null || baseColor.width < 1024 || baseColor.height < 1024)
                        throw new Exception("2K material transport failed: " + source.name);
                    target = new Material(shader) { name = "Tripo_QA_" + source.name };
                    target.mainTexture = baseColor;
                    target.color = Color.white; // GLB baseColorFactor already white: no double darkening
                    if (target.HasProperty("_Glossiness")) target.SetFloat("_Glossiness", 0.13f);
                    changed[source] = target;
                    Debug.Log("TRIPO_MATERIAL_2K_IMPORTED=" + baseColor.name + ":" + baseColor.width + "x" + baseColor.height);
                }
                ms[i] = target;
            }
            rr.sharedMaterials = ms;
        }
    }

    static string CapturePng(Camera camera, string output, string name, float orthographicSize, Vector3 direction, Bounds b)
    {
        camera.orthographicSize = orthographicSize;
        camera.transform.position = b.center + direction;
        camera.transform.LookAt(b.center);
        var target = new RenderTexture(1024, 768, 24);
        var previous = RenderTexture.active;
        try
        {
            camera.targetTexture = target;
            RenderTexture.active = target;
            camera.Render();
            var png = new Texture2D(1024, 768, TextureFormat.RGB24, false);
            png.ReadPixels(new Rect(0, 0, 1024, 768), 0, 0);
            png.Apply();
            var file = Path.Combine(output, name);
            File.WriteAllBytes(file, png.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(png);
            if (new FileInfo(file).Length < 15000) throw new Exception("Empty or suspicious Unity capture: " + name);
        }
        finally
        {
            camera.targetTexture = null;
            RenderTexture.active = previous;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
        }
        return name;
    }

    public static void Capture()
    {
        string output = Environment.GetEnvironmentVariable("ELDORIA_TRIPO_QA_OUTPUT");
        string name = Environment.GetEnvironmentVariable("ELDORIA_TRIPO_QA_ASSET");
        if (String.IsNullOrWhiteSpace(output) || String.IsNullOrWhiteSpace(name))
            throw new Exception("Asset and output environment required");
        if (name != "Eldoria_Spawnling_Bastion2_v1" && name != "Eldoria_RiftFocus_v1")
            throw new Exception("Unexpected QA asset");
        Directory.CreateDirectory(output);
        QualitySettings.globalTextureMipmapLimit = 0;
        AssetDatabase.ImportAsset("Assets/TripoSubject.glb", ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        GameObject imported = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TripoSubject.glb");
        if (imported == null) throw new Exception("glTFast did not import TripoSubject.glb");

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject obj = UnityEngine.Object.Instantiate(imported);
        obj.name = name;
        var data = new Metrics { asset = name, unityVersion = Application.unityVersion };
        var mats = new HashSet<Material>();
        var texs = new HashSet<Texture>();
        foreach (var f in obj.GetComponentsInChildren<MeshFilter>())
        {
            var mesh = f.sharedMesh;
            if (mesh == null) continue;
            data.meshes++;
            data.vertices += mesh.vertexCount;
            for (int i = 0; i < mesh.subMeshCount; i++) data.triangles += mesh.GetIndexCount(i) / 3;
            data.uv0AllMeshes &= mesh.uv != null && mesh.uv.Length == mesh.vertexCount;
            data.normalsAllMeshes &= mesh.normals != null && mesh.normals.Length == mesh.vertexCount;
        }
        foreach (var rr in obj.GetComponentsInChildren<Renderer>())
        {
            data.renderers++;
            foreach (var m in rr.sharedMaterials)
            {
                if (m == null) continue;
                mats.Add(m);
                foreach (var prop in m.GetTexturePropertyNames())
                {
                    var t = m.GetTexture(prop);
                    if (t != null) texs.Add(t);
                }
            }
        }
        data.materials = mats.Count;
        data.textures = texs.Count;
        foreach (var m in mats)
        {
            var t = BaseColorTexture(m);
            if (t == null) continue;
            data.importedBaseColorWidth = Math.Max(data.importedBaseColorWidth, t.width);
            data.importedBaseColorHeight = Math.Max(data.importedBaseColorHeight, t.height);
        }
        data.fullResolutionTexturePass = data.importedBaseColorWidth >= 1024 && data.importedBaseColorHeight >= 1024;
        if (data.triangles < 500 || data.triangles > 150000 || !data.uv0AllMeshes || !data.normalsAllMeshes || !data.fullResolutionTexturePass || data.textures == 0)
            throw new Exception("Import quality gate failed: tri=" + data.triangles + " uv=" + data.uv0AllMeshes + " tex=" + data.textures + " albedo=" + data.importedBaseColorWidth);
        foreach (var t in texs)
            if (t.width < 512 || t.height < 512) throw new Exception("Embedded texture degraded to " + t.width + "x" + t.height);
        PreviewMaterials(obj);

        var b = BoundsOf(obj);
        float horizontal = Mathf.Max(b.size.x, b.size.z);
        if (horizontal <= 0.001f) throw new Exception("Invalid raw bounds");
        float desired = name.Contains("Spawnling") ? 1.6f : 2.6f;
        obj.transform.localScale *= desired / horizontal;
        b = BoundsOf(obj);
        obj.transform.position += new Vector3(-b.center.x, -b.min.y, -b.center.z);
        b = BoundsOf(obj);
        data.worldBounds = b.size;

        var box = obj.AddComponent<BoxCollider>();
        box.center = obj.transform.InverseTransformPoint(b.center);
        var ls = obj.transform.lossyScale;
        box.size = new Vector3(b.size.x / Mathf.Max(.00001f, Mathf.Abs(ls.x)), b.size.y / Mathf.Max(.00001f, Mathf.Abs(ls.y)), b.size.z / Mathf.Max(.00001f, Mathf.Abs(ls.z)));
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "QA ground (not included in model)";
        ground.transform.position = new Vector3(0, -0.035f, 0);
        ground.transform.localScale = new Vector3(2.5f, 1f, 2.5f);
        ground.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("Standard")) { color = new Color(.32f, .34f, .31f) };
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.73f,.76f,.79f);
        RenderSettings.fog = false;
        var light = new GameObject("Sun QA").AddComponent<Light>();
        light.type = LightType.Directional; light.intensity=1.15f; light.shadows=LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(48f, -40f, 0);
        var cam = new GameObject("QA camera").AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.26f,.30f,.34f);
        cam.nearClipPlane = .01f; cam.farClipPlane=100f;
        cam.transform.position = b.center + new Vector3(3f,2.5f,-3.5f);
        cam.transform.LookAt(b.center);
        Physics.SyncTransforms();
        var ray = new Ray(cam.transform.position, (b.center - cam.transform.position).normalized);
        data.positiveSelection = Physics.Raycast(ray, out var hit, 40f) && (hit.collider == box || hit.transform.IsChildOf(obj.transform));
        data.emptySelectionMiss = !Physics.Raycast(cam.ScreenPointToRay(new Vector3(2,766,0)), out var empty, 40f) || !empty.transform.IsChildOf(obj.transform);
        if (!data.positiveSelection || !data.emptySelectionMiss)
            throw new Exception("Raycast failed: positive=" + data.positiveSelection + " empty=" + data.emptySelectionMiss);
        data.captures = new [] {
            CapturePng(cam, output, "iso.png", 1.85f, new Vector3(3.1f, 2.55f, -3.6f), b),
            CapturePng(cam, output, "side.png", 1.85f, new Vector3(0f, 1.8f, 4.5f), b),
            CapturePng(cam, output, "front.png", 2.2f, new Vector3(4.5f, 1.65f, 0f), b),
            CapturePng(cam, output, "strategic.png", 5.2f, new Vector3(3.1f, 2.55f, -3.6f), b)
        };
        File.WriteAllText(Path.Combine(output,"tripo-source-metrics.json"), JsonUtility.ToJson(data,true));
        Debug.Log("TRIPO_ASSET_QA_PASS " + JsonUtility.ToJson(data));
    }
}
