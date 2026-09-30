using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    // Render a baked, original Blender art test without touching a game scene.
    public static class ValoriaOriginalHeroCapture
    {
        const string Root = "Assets/Eldoria/ArtTests/OriginalHero/";

        [MenuItem("Eldoria/Art gate/Capture original hero study")]
        public static void Capture()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "valoria_hero_fragment.fbx");
            if (asset == null) throw new Exception("Original hero FBX missing");
            var hero = UnityEngine.Object.Instantiate(asset);
            hero.name = "ORIGINAL HERO STUDY - NOT APPROVED FOR VALORIA";
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new Exception("URP Lit shader missing");
            var materials = new System.Collections.Generic.Dictionary<string, Material>();
            var textureNames = new System.Collections.Generic.Dictionary<string, string> {
                { "limestone", "limestone" }, { "carved limestone", "limestone" },
                { "ancestral stone", "oldstone" }, { "slate", "slate" },
                { "oxidized slate", "slate" }, { "weathered oak", "oak" },
                { "cliff strata", "rock" }
            };
            foreach (var renderer in hero.GetComponentsInChildren<Renderer>())
            {
                var originals = renderer.sharedMaterials;
                var adapted = new Material[originals.Length];
                for (int i = 0; i < adapted.Length; i++)
                {
                    string name = originals[i] != null ? originals[i].name : "unknown";
                    if (!materials.TryGetValue(name, out var material))
                    {
                        material = new Material(shader) { name = name };
                        foreach (var pair in textureNames)
                            if (name.IndexOf(pair.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Textures/" + pair.Value + ".png");
                                if (tex == null) throw new Exception("Missing original texture: " + pair.Value);
                                material.SetTexture("_BaseMap", tex);
                                break;
                            }
                        if (name.Contains("recess")) material.SetColor("_BaseColor", new Color(.07f, .10f, .12f));
                        if (name.Contains("dim window")) material.SetColor("_BaseColor", new Color(.12f, .19f, .23f));
                        if (name.Contains("warm window")) material.SetColor("_BaseColor", new Color(.90f, .53f, .22f));
                        if (name.Contains("heraldic blue")) material.SetColor("_BaseColor", new Color(.035f, .10f, .26f));
                        if (name.Contains("iron")) material.SetColor("_BaseColor", new Color(.19f, .23f, .24f));
                        if (name.Contains("worn brass")) material.SetColor("_BaseColor", new Color(.47f, .32f, .13f));
                        materials.Add(name, material);
                    }
                    adapted[i] = material;
                }
                renderer.sharedMaterials = adapted;
            }
            var count = hero.GetComponentsInChildren<Renderer>().Length;
            if (count < 7) throw new Exception("Incomplete original hero model: " + count + " renderers");
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Review ground (not exported)";
            ground.transform.position = new Vector3(0, -3.8f, 0);
            ground.transform.localScale = new Vector3(18, 1, 18);
            var terrainMaterial = new Material(shader);
            terrainMaterial.SetColor("_BaseColor", new Color(.32f, .35f, .31f));
            ground.GetComponent<Renderer>().sharedMaterial = terrainMaterial;
            var sun = new GameObject("Warm afternoon").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1, .81f, .66f);
            sun.intensity = 1.45f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(43, -29, 0);
            RenderSettings.ambientLight = new Color(.53f, .61f, .70f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.66f, .73f, .78f);
            RenderSettings.fogDensity = .004f;
            var camera = new GameObject("Review camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = RenderSettings.fogColor;
            camera.orthographic = true;
            camera.orthographicSize = 20f;
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 300f;
            Directory.CreateDirectory("OriginalHeroCaptures");
            // Blender's negative-Y front maps to positive-Z in the exported FBX.
            Save(camera, "OriginalHeroCaptures/hero-establishing.png", new Vector3(49, 40, 61), new Vector3(-1, 14, 0));
            Save(camera, "OriginalHeroCaptures/hero-approach.png", new Vector3(18, 31, 55), new Vector3(0, 13, 0));
            Save(camera, "OriginalHeroCaptures/hero-oblique.png", new Vector3(-53, 38, 43), new Vector3(-1, 13, -1));
            Debug.Log("Original hero test rendered: " + count + " renderers; no game scene modified.");
        }

        static void Save(Camera camera, string path, Vector3 position, Vector3 target)
        {
            camera.transform.position = position;
            camera.transform.LookAt(target);
            var rt = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            try
            {
                camera.targetTexture = rt;
                camera.Render();
                RenderTexture.active = rt;
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
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }
    }
}
