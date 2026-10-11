using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.EditorTools
{
    public static class ValoriaStudioV3UnityProof
    {
        const string Output = "ValoriaStudioV3Proof";
        const string ScenePath = "Assets/Eldoria/ArtTests/ValoriaGatehouseV1/ValoriaVisualSliceV1.unity";
        const string V3Path = "Assets/Eldoria/ArtTests/ValoriaGatehouseV1/ValoriaRockAndGroundV3-ENV.glb";
        const string ProofScene = "Assets/Eldoria/ArtTests/ValoriaGatehouseV1/ValoriaStudioV3Proof.unity";

        public static async void Capture()
        {
            try
            {
                Directory.CreateDirectory(Output);
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                var camera = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault();
                if (camera == null) throw new Exception("Existing Valoria camera missing");
                Save(camera, Output + "/before.png");
                var world = GameObject.Find("Valoria Bastion I - visual vertical slice");
                if (world == null) throw new Exception("Existing Valoria environment missing");
                var old = world.GetComponentsInChildren<MeshFilter>(true)
                    .Where(x => x.sharedMesh != null && x.sharedMesh.name.Contains("Valoria authored geological mesh"))
                    .Select(x => x.GetComponent<Renderer>()).Where(x => x != null).ToArray();
                if (old.Length == 0) throw new Exception("Old geology unidentified: no replacement performed");
                foreach (var renderer in old) renderer.enabled = false;
                var path = Path.GetFullPath(V3Path);
                if (!File.Exists(path)) throw new FileNotFoundException(path);
                var gltf = new GltfImport(null, new UninterruptedDeferAgent());
                if (!await gltf.LoadFile(path)) throw new Exception("V3 GLB load failed");
                var candidate = new GameObject("Valoria V3 CC0 terrain");
                candidate.transform.SetParent(world.transform, false);
                if (!await gltf.InstantiateMainSceneAsync(candidate.transform)) throw new Exception("V3 instantiation failed");
                var meshes = candidate.GetComponentsInChildren<MeshFilter>(true).Where(x => x.sharedMesh != null).ToArray();
                if (meshes.Length == 0) throw new Exception("V3 contains no meshes");
                Save(camera, Output + "/after.png");
                PersistRuntimeAssets();
                StabilizeConiferMaterials();
                StabilizeCurtainWallMaterials();
                Save(camera, Output + "/after-persistence.png");
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ProofScene);
                var triangles = meshes.Sum(x => x.sharedMesh.triangles.Length / 3L);
                File.WriteAllText(Output + "/metrics.txt", "v3_triangles=" + triangles +
                    "\nv3_meshes=" + meshes.Length + "\nold_geology_hidden=" + old.Length +
                    "\nvisual_approved=false\nmobile_device_tested=false\n");
                EditorApplication.Exit(0);
            }
            catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(1); }
        }
        public static void Verify()
        {
            try
            {
                Directory.CreateDirectory(Output);
                EditorSceneManager.OpenScene(ProofScene, OpenSceneMode.Single);
                if (GameObject.Find("Valoria V3 CC0 terrain") == null)
                    throw new Exception("V3 terrain not preserved in proof scene");
                var camera = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).First();
                Save(camera, Output + "/reloaded.png");
                EditorApplication.Exit(0);
            }
            catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(1); }
        }
        static void PersistRuntimeAssets()
        {
            const string folder = "Assets/Eldoria/ArtTests/ValoriaGatehouseV1/V3PersistentAssets";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Eldoria/ArtTests/ValoriaGatehouseV1", "V3PersistentAssets");
            var meshes = UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int next = 0;
            foreach (var mf in meshes)
            {
                var mesh = mf.sharedMesh;
                if (mesh != null && !AssetDatabase.Contains(mesh))
                {
                    var clone = UnityEngine.Object.Instantiate(mesh);
                    clone.name = "Persisted mesh " + next;
                    AssetDatabase.CreateAsset(clone, folder + "/mesh-" + next++ + ".asset");
                    mf.sharedMesh = clone;
                }
            }
            var materials = new System.Collections.Generic.Dictionary<Material,Material>();
            var textures = new System.Collections.Generic.Dictionary<Texture,Texture>();
            foreach (var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var source = renderer.sharedMaterials;
                var persisted = new Material[source.Length];
                for (int i = 0; i < source.Length; i++)
                {
                    var mat = source[i];
                    if (mat == null || AssetDatabase.Contains(mat)) { persisted[i] = mat; continue; }
                    if (!materials.TryGetValue(mat, out var copy))
                    {
                        copy = UnityEngine.Object.Instantiate(mat);
                        if (mat.HasProperty("baseColorTexture") && mat.HasProperty("baseColorFactor"))
                        {
                            var standard = Shader.Find("Universal Render Pipeline/Lit");
                            if (standard == null) throw new Exception("URP Lit shader missing");
                            copy = new Material(standard);
                            copy.name = "Valoria persistent PBR " + materials.Count;
                            copy.SetColor("_BaseColor", mat.GetColor("baseColorFactor"));
                            copy.SetFloat("_Cull", 0f);
                            copy.doubleSidedGI = true;
                            copy.SetTexture("_BaseMap", mat.GetTexture("baseColorTexture"));
                            if (mat.HasProperty("normalTexture") && mat.GetTexture("normalTexture") != null)
                            {
                                // Native glTF normals require a correctly packed Unity normal-map import.\n                                // Keep stable albedo-only URP material until that path is verified.
                            }
                            if (mat.HasProperty("alphaCutoff") && mat.GetFloat("alphaCutoff") > 0.001f)
                            {
                                copy.SetFloat("_AlphaClip", 1f);
                                copy.SetFloat("_Cutoff", mat.GetFloat("alphaCutoff"));
                                copy.EnableKeyword("_ALPHATEST_ON");
                                copy.renderQueue = 2450;
                            }
                        }
                        if (copy.shader != null)
                            for (int p = 0; p < copy.shader.GetPropertyCount(); p++)
                            {
                                if (copy.shader.GetPropertyType(p) != UnityEngine.Rendering.ShaderPropertyType.Texture) continue;
                                var prop = copy.shader.GetPropertyName(p);
                                var tex = copy.GetTexture(prop);
                                if (tex == null || AssetDatabase.Contains(tex)) continue;
                                if (!textures.TryGetValue(tex, out var stored))
                                {
                                    stored = PersistTexture(tex, folder, textures.Count, prop);
                                    textures[tex] = stored;
                                }
                                copy.SetTexture(prop, stored);
                            }
                        AssetDatabase.CreateAsset(copy, folder + "/material-" + materials.Count + ".mat");
                        materials[mat] = copy;
                    }
                    persisted[i] = copy;
                }
                renderer.sharedMaterials = persisted;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("V3_PERSISTENCE meshes=" + next + " materials=" + materials.Count + " textures=" + textures.Count);
        }
        static void StabilizeConiferMaterials()
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) throw new Exception("URP Unlit missing");
            const string folder = "Assets/Eldoria/ArtTests/ValoriaGatehouseV1/V3PersistentAssets";
            var world = GameObject.Find("Valoria Bastion I - visual vertical slice");
            int count = 0;
            foreach (Transform child in world.transform)
            {
                if (!child.name.StartsWith("Existing FBX conifer variant")) continue;
                foreach (var renderer in child.GetComponentsInChildren<Renderer>(true))
                {
                    var original = renderer.sharedMaterials;
                    var replacement = new Material[original.Length];
                    for (int i = 0; i < original.Length; i++)
                    {
                        var mat = original[i];
                        if (mat == null) continue;
                        var source = mat.HasProperty("_BaseMap") ? mat.GetTexture("_BaseMap") : mat.mainTexture;
                        var stable = new Material(shader);
                        stable.name = "Valoria conifer stable unlit " + count;
                        stable.SetColor("_BaseColor", new Color(.16f, .34f, .22f, 1f));
                        if (source != null) stable.SetTexture("_BaseMap", source);
                        stable.SetFloat("_AlphaClip", 1f);
                        stable.SetFloat("_Cutoff", 0.4f);
                        stable.SetFloat("_Cull", 0f);
                        stable.renderQueue = 2450;
                        AssetDatabase.CreateAsset(stable, folder + "/stable-tree-" + count++ + ".mat");
                        replacement[i] = stable;
                    }
                    renderer.sharedMaterials = replacement;
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("VALORIA_STABLE_CONIFER_MATERIALS=" + count);
        }
        static void StabilizeCurtainWallMaterials()
        {
            const string folder = "Assets/Eldoria/ArtTests/ValoriaGatehouseV1/V3PersistentAssets";
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new Exception("URP Lit missing");
            var material = new Material(shader);
            material.name = "Valoria restored wall stone";
            material.SetColor("_BaseColor", new Color(.59f, .55f, .49f, 1f));
            AssetDatabase.CreateAsset(material, folder + "/stable-stone-wall.mat");
            foreach (string name in new [] { "Original wall segment left", "Original wall segment right", "Upper western curtain wall", "Upper eastern curtain wall" })
            {
                var root = GameObject.Find(name);
                if (root == null) continue;
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    var arr = renderer.sharedMaterials;
                    for (int i=0; i<arr.Length; i++) arr[i] = material;
                    renderer.sharedMaterials = arr;
                }
            }
            AssetDatabase.SaveAssets();
        }
        static Texture PersistTexture(Texture source, string folder, int index, string property)
        {
            var rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            var previous = RenderTexture.active;
            var readback = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            try
            {
                Graphics.Blit(source, rt);
                RenderTexture.active = rt;
                readback.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
                readback.Apply();
                var path = folder + "/tex-" + index + ".png";
                File.WriteAllBytes(Path.GetFullPath(path), readback.EncodeToPNG());
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    bool isNormal = property.ToLowerInvariant().Contains("normal") || property.Contains("Bump");
                    importer.sRGBTexture = !isNormal;
                    importer.alphaIsTransparency = property.ToLowerInvariant().Contains("base") || property.Contains("MainTex");
                    importer.SaveAndReimport();
                }
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(readback);
            }
        }
        static void Save(Camera camera, string path)
        {
            var target = new RenderTexture(1280, 720, 24);
            var prior = camera.targetTexture;
            var active = RenderTexture.active;
            var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = prior;
                RenderTexture.active = active;
                UnityEngine.Object.DestroyImmediate(texture);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }
    }
}
