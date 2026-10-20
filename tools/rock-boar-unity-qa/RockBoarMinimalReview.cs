using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Lightweight isolated Unity validation of the ONE Tripo H3.1 Rock Boar already paid for.
// Does not load the main Eldoria scene, call Tripo, edit saves, or modify gameplay.
public static class RockBoarMinimalReview
{
    [Serializable] private class Result
    {
        public string asset = "Eldoria_RockBoar_FirstHunt_v1";
        public string source = "Assets/RockBoar.glb";
        public string unityVersion;
        public int meshes;
        public int renderers;
        public int materials;
        public int textures;
        public long triangles;
        public long vertices;
        public bool uv0AllMeshes = true;
        public bool normalsAllMeshes = true;
        public bool positiveSelection;
        public bool emptySelectionMiss;
        public bool capturesValid;
        public Vector3 worldBounds;
        public string[] captureFiles;
    }

    private static Bounds GetBounds(GameObject root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) throw new Exception("Rock Boar has no renderers");
        var bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private static Texture GetBaseTexture(Material mat)
    {
        if (mat == null) return null;
        string[] names = mat.GetTexturePropertyNames();
        foreach (string name in names)
        {
            string n = name.ToLowerInvariant();
            if (n.Contains("basecolor") || n.Contains("_basemap") || n.Contains("maintex") || n.Contains("diffuse"))
            {
                var texture = mat.GetTexture(name);
                if (texture != null) return texture;
            }
        }
        return mat.mainTexture;
    }

    private static void ApplyPreviewMaterials(GameObject root)
    {
        var shader = Shader.Find("Standard");
        if (shader == null) throw new Exception("Standard shader is unavailable for isolated lit preview");
        var converted = new Dictionary<Material, Material>();
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
        {
            var slots = renderer.sharedMaterials;
            for (int i = 0; i < slots.Length; i++)
            {
                Material original = slots[i];
                if (original == null) continue;
                if (!converted.TryGetValue(original, out Material target))
                {
                    target = new Material(shader) { name = "BoarPreview_" + original.name };
                    var tex = GetBaseTexture(original);
                    if (tex == null) throw new Exception("Imported PBR base-color texture was not accessible: " + original.name);
                    // The GLB's glTF baseColorFactor is [1,1,1,1].
                    // Never multiply the imported albedo by a second (possibly dark)
                    // Unity shader color: this made the first Unity capture near-black.
                    target.mainTexture = tex;
                    target.color = Color.white;
                    if (target.HasProperty("_Glossiness")) target.SetFloat("_Glossiness", 0.12f);
                    Debug.Log("BOAR_MATERIAL_TRANSPORT: source_shader=" + original.shader.name +
                              " imported_base_texture=" + tex.name +
                              " resolution=" + tex.width + "x" + tex.height +
                              " preview_tint=white");
                    converted.Add(original, target);
                }
                slots[i] = target;
            }
            renderer.sharedMaterials = slots;
        }
    }

    private static string Save(Camera camera, string path, int width, int height)
    {
        RenderTexture target = new RenderTexture(width, height, 24);
        camera.targetTexture = target;
        RenderTexture.active = target;
        camera.Render();
        Texture2D png = new Texture2D(width, height, TextureFormat.RGB24, false);
        png.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        png.Apply();
        File.WriteAllBytes(path, png.EncodeToPNG());
        camera.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.DestroyImmediate(png);
        UnityEngine.Object.DestroyImmediate(target);
        if (new FileInfo(path).Length < 20000) throw new Exception("Unity capture is suspiciously small: " + path);
        return Path.GetFileName(path);
    }

    public static void Capture()
    {
        string output = Environment.GetEnvironmentVariable("ELDORIA_BOAR_QA_OUTPUT");
        if (String.IsNullOrEmpty(output)) throw new Exception("ELDORIA_BOAR_QA_OUTPUT is required");
        Directory.CreateDirectory(output);
        const string asset = "Assets/RockBoar.glb";
        AssetDatabase.ImportAsset(asset, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(asset);
        if (source == null) throw new Exception("glTFast did not import the existing RockBoar.glb");

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject boar = UnityEngine.Object.Instantiate(source);
        boar.name = "Eldoria Rock Boar — First Hunt";
        var result = new Result { unityVersion = Application.unityVersion };

        var originals = new HashSet<Material>();
        var textures = new HashSet<Texture>();
        MeshFilter[] filters = boar.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter f in filters)
        {
            Mesh mesh = f.sharedMesh;
            if (mesh == null) continue;
            result.meshes++;
            result.vertices += mesh.vertexCount;
            for (int sub = 0; sub < mesh.subMeshCount; sub++) result.triangles += (long)mesh.GetIndexCount(sub) / 3;
            result.uv0AllMeshes &= mesh.uv != null && mesh.uv.Length == mesh.vertexCount;
            result.normalsAllMeshes &= mesh.normals != null && mesh.normals.Length == mesh.vertexCount;
        }
        foreach (Renderer renderer in boar.GetComponentsInChildren<Renderer>())
        {
            result.renderers++;
            foreach (Material m in renderer.sharedMaterials)
            {
                if (m == null) continue;
                originals.Add(m);
                foreach (string prop in m.GetTexturePropertyNames())
                {
                    Texture t = m.GetTexture(prop);
                    if (t != null) textures.Add(t);
                }
            }
        }
        result.materials = originals.Count;
        result.textures = textures.Count;
        if (result.triangles < 48000 || result.triangles > 51000 || !result.uv0AllMeshes || !result.normalsAllMeshes || result.textures == 0)
            throw new Exception("Boar import acceptance failed triangles=" + result.triangles + " uv=" + result.uv0AllMeshes + " normals=" + result.normalsAllMeshes + " textures=" + result.textures);

        ApplyPreviewMaterials(boar);
        Bounds rawBounds = GetBounds(boar);
        float maxHorizontal = Mathf.Max(rawBounds.size.x, rawBounds.size.z);
        boar.transform.localScale *= 1.70f / maxHorizontal;
        Bounds bounds = GetBounds(boar);
        boar.transform.position += new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
        bounds = GetBounds(boar);
        result.worldBounds = bounds.size;

        BoxCollider collider = boar.AddComponent<BoxCollider>();
        collider.center = boar.transform.InverseTransformPoint(bounds.center);
        collider.size = boar.transform.InverseTransformVector(bounds.size);
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "QA ground, not part of the GLB";
        ground.transform.position = new Vector3(0f, -0.03f, 0f);
        ground.transform.localScale = new Vector3(2.5f, 1f, 2.5f);
        var groundShader = Shader.Find("Standard");
        ground.GetComponent<Renderer>().sharedMaterial = new Material(groundShader) { color = new Color(.36f, .36f, .32f) };

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.76f, .77f, .80f);
        var sunlight = new GameObject("QA sun").AddComponent<Light>();
        sunlight.type = LightType.Directional;
        sunlight.intensity = 1.15f;
        sunlight.shadows = LightShadows.Soft;
        sunlight.transform.rotation = Quaternion.Euler(52f, -35f, 0f);

        var cam = new GameObject("Isometric QA camera").AddComponent<Camera>();
        cam.tag = "MainCamera";
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.25f, .29f, .34f);
        cam.nearClipPlane = .01f;
        cam.farClipPlane = 100f;
        cam.orthographicSize = 1.6f;
        cam.transform.position = bounds.center + new Vector3(2.6f, 2.2f, -3.4f);
        cam.transform.LookAt(bounds.center);
        Physics.SyncTransforms();
        Ray ray = new Ray(cam.transform.position, (bounds.center - cam.transform.position).normalized);
        result.positiveSelection = Physics.Raycast(ray, out RaycastHit hit, 30f) && (hit.collider == collider || hit.transform.IsChildOf(boar.transform));
        Ray miss = cam.ScreenPointToRay(new Vector3(2f, 766f, 0f));
        result.emptySelectionMiss = !Physics.Raycast(miss, out RaycastHit empty, 30f) || !empty.transform.IsChildOf(boar.transform);

        var captures = new List<string>();
        captures.Add(Save(cam, Path.Combine(output, "boar-unity-isometric-textured.png"), 1024, 768));
        cam.transform.position = bounds.center + new Vector3(0f, 1.5f, 3.7f);
        cam.transform.LookAt(bounds.center);
        captures.Add(Save(cam, Path.Combine(output, "boar-unity-side-textured.png"), 1024, 768));
        cam.orthographicSize = 2.1f;
        cam.transform.position = bounds.center + new Vector3(3.7f, 1.6f, 0f);
        cam.transform.LookAt(bounds.center);
        captures.Add(Save(cam, Path.Combine(output, "boar-unity-front-textured.png"), 1024, 768));
        cam.orthographicSize = 4.8f;
        cam.transform.position = bounds.center + new Vector3(2.6f, 2.2f, -3.4f);
        cam.transform.LookAt(bounds.center);
        captures.Add(Save(cam, Path.Combine(output, "boar-unity-strategic-textured.png"), 1024, 768));
        result.captureFiles = captures.ToArray();
        result.capturesValid = true;
        File.WriteAllText(Path.Combine(output, "boar-unity-metrics.json"), JsonUtility.ToJson(result, true));
        Debug.Log("BOAR_UNITY_ISOLATED_QA=" + JsonUtility.ToJson(result));
        if (!result.positiveSelection || !result.emptySelectionMiss)
            throw new Exception("Rock Boar raycast gate failed, positive=" + result.positiveSelection + ", miss=" + result.emptySelectionMiss);
    }
}
