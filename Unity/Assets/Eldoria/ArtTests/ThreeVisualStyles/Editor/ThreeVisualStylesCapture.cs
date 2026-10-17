using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Eldoria.ArtTests.ThreeVisualStyles
{
    /// <summary>Use Unity itself, not an AI illustration, to capture and save the THREE isolated scenes.</summary>
    public static class ThreeVisualStylesCapture
    {
        private const string Folder = "Assets/Eldoria/ArtTests/ThreeVisualStyles/";
        private static readonly string[] SceneNames = {
            "01_FantasyStrategy", "02_DetailedMedieval", "03_ColorfulCastle"
        };

        [Serializable]
        private class TrialResult
        {
            public string engine;
            public string scene;
            public string pipeline;
            public string screenshot;
            public int meshRenderers;
            public int meshes;
            public int authenticSourceInstances;
            public bool vendorArtPass;
            public bool hasCamera;
            public bool hasBastion;
            public bool containsGameplayIntegration;
            public bool certifiedMobile;
            public string status;
        }

        // Imported CC0 Kenney GLBs reference sibling Textures/colormap.png.
        // Previous failed importer cache results must be invalidated explicitly
        // after the original external texture files are restored.
        private static void RefreshKenneyDependencies()
        {
            const string relative = "Assets/Eldoria/ArtTests/ThreeVisualStyles/Vendor/Kenney";
            string root = Path.Combine(UnityEngine.Application.dataPath,
                "Eldoria/ArtTests/ThreeVisualStyles/Vendor/Kenney");
            if (!Directory.Exists(root))
                throw new DirectoryNotFoundException(root);
            int attempted = 0;
            foreach (string full in Directory.GetFiles(root, "*.glb", SearchOption.AllDirectories))
            {
                string path = "Assets/" + Path.GetRelativePath(UnityEngine.Application.dataPath, full)
                    .Replace('\\','/');
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate |
                                                ImportAssetOptions.ForceSynchronousImport);
                attempted++;
            }
            Debug.Log("[ELDORIA LOOK DEV] Kenney GLB force-reimports completed: " + attempted);
        }

        [MenuItem("Eldoria/Art tests/Three visual styles/Capture all three PNGs")]
        public static void CaptureAll()
        {
            if (!UnityEngine.Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            string previous = SceneManager.GetActiveScene().path;
            string output = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,
                "../../docs/evidence/eldoria-three-visual-styles-20261017"));
            Directory.CreateDirectory(output);
            try
            {
                RefreshKenneyDependencies();
                foreach (string name in SceneNames)
                {
                    Open(name);
                    string file = Path.Combine(output, name + "_UNITY_REAL.png");
                    var root = GeneratedRoot();
                    var generator = root.GetComponentInParent<ValoriaThreeStylesScene>();
                    int genuine = generator != null ? generator.AuthenticSourceInstances : 0;
                    int required = name == "02_DetailedMedieval" ? 2 : 3;
                    var camera = root.GetComponentInChildren<Camera>(true);
                    if (camera == null) throw new InvalidOperationException(name + " missing main camera");
                    Render(camera, file);
                    // A closer true Unity perspective is necessary to evaluate model
                    // quality instead of judging only a far-away strategic overview.
                    float previousSize = camera.orthographicSize;
                    string closeFile = Path.Combine(output, name + "_UNITY_CLOSE.png");
                    camera.orthographicSize = 30f;
                    try { Render(camera, closeFile); }
                    finally { camera.orthographicSize = previousSize; }
                    var result = new TrialResult {
                        engine = UnityEngine.Application.unityVersion,
                        scene = name,
                        pipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null ?
                                   UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.name : "builtin",
                        screenshot = file,
                        meshRenderers = root.GetComponentsInChildren<MeshRenderer>(true).Length,
                        meshes = root.GetComponentsInChildren<MeshFilter>(true).Length,
                        authenticSourceInstances = genuine,
                        vendorArtPass = genuine >= required,
                        hasCamera = true,
                        hasBastion = ContainsChild(root, "Bastion main hall"),
                        containsGameplayIntegration = false,
                        certifiedMobile = false,
                        status = "UNITY_EDITOR_CAPTURE_ONLY__ART_AND_DEVICE_NOT_APPROVED"
                    };
                    File.WriteAllText(Path.Combine(output, name + ".json"),
                        JsonUtility.ToJson(result, true));
                    if (!result.hasBastion || result.meshRenderers < 100 || !result.vendorArtPass)
                        throw new InvalidOperationException(name + " failed content assertions");
                    Debug.Log("[ELDORIA LOOK DEV] Real Unity output: " + file);
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(previous) && File.Exists(previous))
                    EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
            }
        }

        [MenuItem("Eldoria/Art tests/Three visual styles/Bake three editable Unity scenes")]
        public static void BakeAll()
        {
            if (!UnityEngine.Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            string previous = SceneManager.GetActiveScene().path;
            try
            {
                RefreshKenneyDependencies();
                foreach (var name in SceneNames)
                {
                    Open(name);
                    var generator = Object.FindFirstObjectByType<ValoriaThreeStylesScene>();
                    int required = name == "02_DetailedMedieval" ? 2 : 3;
                    if (generator.AuthenticSourceInstances < required)
                        throw new InvalidOperationException(name + " lacks imported genuine models (found " +
                            generator.AuthenticSourceInstances + ", expected " + required + ")");
                    BakeCurrent(name);
                }
                AssetDatabase.SaveAssets();
                Debug.Log("[ELDORIA LOOK DEV] Three isolated Unity scenes baked to editable native scene assets.");
            }
            finally
            {
                if (!string.IsNullOrEmpty(previous) && File.Exists(previous))
                    EditorSceneManager.OpenScene(previous, OpenSceneMode.Single);
            }
        }

        private static void Open(string name)
        {
            var scene = EditorSceneManager.OpenScene(Folder + name + ".unity", OpenSceneMode.Single);
            if (!scene.IsValid()) throw new InvalidOperationException("Scene did not open: " + name);
            var generator = Object.FindFirstObjectByType<ValoriaThreeStylesScene>();
            if (generator == null) throw new InvalidOperationException("Generator missing: " + name);
            generator.Build();
            if (GeneratedRoot().GetComponentInChildren<Camera>(true) == null)
                throw new InvalidOperationException("No generated camera: " + name);
        }

        private static Transform GeneratedRoot()
        {
            var generator = Object.FindFirstObjectByType<ValoriaThreeStylesScene>();
            if (generator == null) throw new InvalidOperationException("No art trial root");
            var root = generator.transform.Find("__LOOK_DEV_GENERATED_NOT_SAVED");
            if (root == null) throw new InvalidOperationException("No generated trial geometry");
            return root;
        }

        private static bool ContainsChild(Transform root, string childName)
        {
            foreach (var item in root.GetComponentsInChildren<Transform>(true))
                if (item.name == childName) return true;
            return false;
        }

        private static void BakeCurrent(string name)
        {
            var generator = Object.FindFirstObjectByType<ValoriaThreeStylesScene>();
            if (generator == null) throw new InvalidOperationException("Generator missing");
            var root = generator.transform.Find("__LOOK_DEV_GENERATED_NOT_SAVED");
            if (root == null) throw new InvalidOperationException("No generated terrain and buildings");
            string folder = Folder + "Baked_" + name;
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder(Folder.TrimEnd('/'), "Baked_" + name);
            root.gameObject.hideFlags = HideFlags.None;
            var materials = new Dictionary<Material, Material>();
            var meshes = new Dictionary<Mesh, Mesh>();
            int matCount = 0, meshCount = 0;
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                var original = renderer.sharedMaterial;
                if (original == null || EditorUtility.IsPersistent(original)) continue;
                if (!materials.TryGetValue(original, out var persistent))
                {
                    persistent = new Material(original) { name = "VisualTrial_Mat_" + matCount };
                    persistent.hideFlags = HideFlags.None;
                    AssetDatabase.CreateAsset(persistent,
                        folder + "/PreviewMat_" + (matCount++) + ".mat");
                    materials.Add(original, persistent);
                }
                renderer.sharedMaterial = persistent;
            }
            foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                var original = filter.sharedMesh;
                if (original == null || EditorUtility.IsPersistent(original)) continue;
                if (!meshes.TryGetValue(original, out var persistent))
                {
                    persistent = Object.Instantiate(original);
                    persistent.hideFlags = HideFlags.None;
                    persistent.name = "TrialMesh_" + meshCount;
                    AssetDatabase.CreateAsset(persistent,
                        folder + "/GeneratedMesh_" + (meshCount++) + ".asset");
                    meshes.Add(original, persistent);
                }
                filter.sharedMesh = persistent;
            }
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            if (!EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), Folder + name + ".unity"))
                throw new IOException("Unable to save baked scene " + name);
        }

        private static void Render(Camera camera, string destination)
        {
            var rt = RenderTexture.GetTemporary(1600, 900, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            var previousTarget = camera.targetTexture;
            var texture = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = rt;
                RenderTexture.active = rt;
                camera.Render();
                texture.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0, false);
                texture.Apply(false);
                File.WriteAllBytes(destination, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
                Object.DestroyImmediate(texture);
            }
        }
    }
}
