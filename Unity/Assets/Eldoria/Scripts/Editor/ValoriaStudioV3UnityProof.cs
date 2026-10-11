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
                        if (copy.shader != null)
                            for (int p = 0; p < copy.shader.GetPropertyCount(); p++)
                            {
                                if (copy.shader.GetPropertyType(p) != UnityEngine.Rendering.ShaderPropertyType.Texture) continue;
                                var prop = copy.shader.GetPropertyName(p);
                                var tex = copy.GetTexture(prop);
                                if (tex == null || AssetDatabase.Contains(tex)) continue;
                                if (!textures.TryGetValue(tex, out var stored))
                                {
                                    stored = UnityEngine.Object.Instantiate(tex);
                                    AssetDatabase.CreateAsset(stored, folder + "/tex-" + textures.Count + ".asset");
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
