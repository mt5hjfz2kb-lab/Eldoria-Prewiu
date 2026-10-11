using System;
using System.IO;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools
{
    // Build and capture an independent 2.5D feasibility scene, leaving Valoria.unity untouched.
    public static class LayeredValoriaArtTestCapture
    {
        const string Assets = "Assets/Eldoria/ArtTests/LayeredValoria/";
        const string Scene = Assets + "LayeredValoriaArtTest.unity";
        const string Folder = "LayeredValoriaCaptures";

        [MenuItem("Eldoria/Art gate/Open isolated layered art test")]
        public static void OpenInteractive()
        {
            CreateScene();
            EditorSceneManager.OpenScene(Scene);
        }

        public static void Capture()
        {
            CreateScene();
            var camera = Camera.main;
            var controller = UnityEngine.Object.FindFirstObjectByType<LayeredValoriaArtTest>();
            if (camera == null || controller == null) throw new Exception("Art test scene is incomplete");
            Directory.CreateDirectory(Folder);
            var names = new[] { "strategic", "city", "detail" };
            var sizes = new[] { 19f, 12f, 9f };
            for (int stage = 1; stage <= 2; stage++)
            {
                controller.Bastion.sharedMaterial = stage == 1 ? controller.InitialStage : controller.RebuiltStage;
                for (int zoom = 0; zoom < 3; zoom++)
                {
                    camera.orthographicSize = sizes[zoom];
                    Save(camera, Folder + "/stage-" + stage + "-" + names[zoom] + ".png");
                }
            }
            Debug.Log("Isolated layered Valoria captures: " + Path.GetFullPath(Folder));
        }

        static void CreateScene()
        {
            SceneSetup.SetupRenderPipeline();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = new GameObject("Eldoria equivalent isometric camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 12f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.49f, .61f, .70f);
            camera.transform.position = new Vector3(18.2f, 14.6f, -25.8f);
            camera.transform.LookAt(new Vector3(0, 3.15f, 5.8f));

            var distant = Material("valley-backdrop.png", false, "Valley");
            var initial = Material("bastion-stage-1.png", true, "Initial Bastion");
            var rebuilt = Material("bastion-stage-2.png", true, "Rebuilt Bastion");
            Quad("Illustrated valley (camera aligned distant layer)", camera, distant, 90f, 82f, 0f, 1f, false);
            var hero = Quad("Bastion original illustration (interactive)", camera, initial, 20f, 35f, 0f, -2f, true);
            var control = new GameObject("Art test controls; independent from gameplay").AddComponent<LayeredValoriaArtTest>();
            control.ReviewCamera = camera;
            control.Bastion = hero;
            control.InitialStage = initial;
            control.RebuiltStage = rebuilt;
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Scene);
        }

        static Material Material(string filename, bool transparent, string name)
        {
            var importer = AssetImporter.GetAtPath(Assets + filename) as TextureImporter;
            if (importer == null) throw new Exception("Texture importer missing: " + filename);
            if (importer.alphaIsTransparency != transparent)
            {
                importer.alphaIsTransparency = transparent;
                importer.SaveAndReimport();
            }
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Assets + filename);
            if (texture == null) throw new Exception("Missing art-test texture: " + filename);
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) throw new Exception("URP Unlit shader not available");
            var path = Assets + name.Replace(' ', '-') + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.shader = shader;
            mat.SetTexture("_BaseMap", texture);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Cull", (float)CullMode.Off);
            mat.SetFloat("_Surface", transparent ? 1f : 0f);
            mat.SetInt("_SrcBlend", transparent ? (int)BlendMode.SrcAlpha : (int)BlendMode.One);
            mat.SetInt("_DstBlend", transparent ? (int)BlendMode.OneMinusSrcAlpha : (int)BlendMode.Zero);
            mat.SetInt("_ZWrite", transparent ? 0 : 1);
            mat.renderQueue = transparent ? (int)RenderQueue.Transparent : (int)RenderQueue.Geometry;
            mat.SetOverrideTag("RenderType", transparent ? "Transparent" : "Opaque");
            if (transparent) mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            else mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            return mat;
        }

        static Renderer Quad(string name, Camera camera, Material mat, float distance,
            float width, float right, float up, bool interactive)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.rotation = camera.transform.rotation;
            go.transform.position = camera.transform.position + camera.transform.forward * distance
                + camera.transform.right * right + camera.transform.up * up;
            go.transform.localScale = new Vector3(width, width * 1024f / 1536f, 1);
            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = mat;
            if (!interactive) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            return renderer;
        }

        static void Save(Camera camera, string path)
        {
            var target = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            var old = RenderTexture.active;
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image);
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = old;
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }
    }
}
