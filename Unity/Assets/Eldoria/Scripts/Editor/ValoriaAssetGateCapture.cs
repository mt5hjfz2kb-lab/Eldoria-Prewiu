using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    // Deliberately isolated asset gate. It does not edit Valoria or enter the player build.
    public static class ValoriaAssetGateCapture
    {
        const string Fort = "Assets/Eldoria/ArtTests/PolyHavenFort/modular_fort_01_1k.fbx";
        const string Rocks = "Assets/Eldoria/ArtTests/PolyHavenRocks/rock_moss_set_01_1k.fbx";
        const string Town = "Assets/EmaceArt/Slavic World Free/Prefabs/Town/Building/";
        const string Trees = "Assets/Tree_Packs/URP_Tree_Pack/Prefabs/";

        [MenuItem("Eldoria/Art gate/Capture isolated asset study")]
        public static void Capture()
        {
            // Downloaded test media is intentionally untracked; import it synchronously
            // on this Editor invocation instead of relying on a previous batch run.
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var stone = MaterialFor("Stone (Poly Haven CC0)", "Assets/Eldoria/ArtTests/PolyHavenFort/modular_fort_01_wall_diff_1k.jpg", "Assets/Eldoria/ArtTests/PolyHavenFort/modular_fort_01_wall_nor_gl_1k.jpg");
            var plaster = MaterialFor("Plaster (Poly Haven CC0)", "Assets/Eldoria/ArtTests/PolyHavenFort/modular_fort_01_plaster_diff_1k.jpg", "Assets/Eldoria/ArtTests/PolyHavenFort/modular_fort_01_plaster_nor_gl_1k.jpg");
            var rock = MaterialFor("Rock (Poly Haven CC0)", "Assets/Eldoria/ArtTests/PolyHavenRocks/rock_moss_set_01_diff_1k.jpg", "Assets/Eldoria/ArtTests/PolyHavenRocks/rock_moss_set_01_nor_gl_1k.jpg");
            var terrain = Terrain.CreateTerrainGameObject(MakeTerrain());
            terrain.name = "Art gate terrain (test only)";
            terrain.transform.position = new Vector3(-30, -2.2f, -30);
            var terrainComponent = terrain.GetComponent<Terrain>();
            terrainComponent.drawInstanced = true;

            var fort = Place(Fort, "CC0 weathered fortress", new Vector3(0, 0, 3), 20, terrainComponent, stone, plaster);
            var rockModel = Place(Rocks, "CC0 geological strata", new Vector3(-15, 0, 5), 10, terrainComponent, rock, null);
            Place(Town + "EA03_Town_House_Comp_01a_PRE.prefab", "Existing town house A", new Vector3(-12, 0, -10), 6, terrainComponent, null, null);
            Place(Town + "EA03_Town_House_Comp_03a_PRE.prefab", "Existing town house B", new Vector3(11, 0, -7), 6, terrainComponent, null, null);
            for (int i = 0; i < 14; i++)
            {
                float a = i * 2.39996f;
                float radius = 17 + (i % 5) * 1.65f;
                float x = Mathf.Cos(a) * radius, z = Mathf.Sin(a) * radius;
                Place(Trees + "URP_Tree_" + (1 + i % 3) + ".prefab", "Tree " + i,
                    new Vector3(x, 0, z), 5.4f + (i % 4) * .8f, terrainComponent, null, null);
            }
            var sun = new GameObject("Warm directional sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1, .86f, .69f);
            sun.intensity = 1.55f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(39, -32, 0);
            RenderSettings.ambientLight = new Color(.53f, .58f, .67f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.67f, .74f, .82f);
            RenderSettings.fogDensity = .008f;
            var camera = new GameObject("Art test camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = RenderSettings.fogColor;
            camera.orthographic = true;
            camera.orthographicSize = 18;
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 250;
            camera.allowHDR = true;
            Directory.CreateDirectory("AssetGateCaptures");
            Save(camera, "AssetGateCaptures/asset-gate-establishing.png", new Vector3(39, 33, -47), new Vector3(0, 5, 1));
            Save(camera, "AssetGateCaptures/asset-gate-oblique.png", new Vector3(-36, 26, -38), new Vector3(0, 5, 1));
            Debug.Log("Asset gate complete; fort renderers=" + fort.GetComponentsInChildren<Renderer>().Length +
                      "; rock renderers=" + rockModel.GetComponentsInChildren<Renderer>().Length +
                      "; screenshots in " + Path.GetFullPath("AssetGateCaptures"));
        }

        static TerrainData MakeTerrain()
        {
            var data = new TerrainData { heightmapResolution = 129, size = new Vector3(60, 12, 60) };
            float[,] heights = new float[129, 129];
            for (int z = 0; z < 129; z++) for (int x = 0; x < 129; x++)
            {
                float worldX = (x / 128f - .5f) * 60, worldZ = (z / 128f - .5f) * 60;
                float rise = 3.8f * Mathf.Exp(-.003f * (worldX * worldX + (worldZ - 3) * (worldZ - 3)));
                float ridge = 2.2f * Mathf.Exp(-.025f * (worldX + 17) * (worldX + 17));
                float waves = .45f * Mathf.PerlinNoise(x * .065f, z * .057f);
                heights[z, x] = Mathf.Clamp01((rise + ridge + waves) / 12f);
            }
            data.SetHeights(0, 0, heights);
            var layer = new TerrainLayer { diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/Eldoria/ArtTests/PolyHavenRocks/rock_moss_set_01_diff_1k.jpg"), tileSize = new Vector2(10, 10) };
            data.terrainLayers = new[] { layer };
            return data;
        }

        static Material MaterialFor(string name, string path, string normalPath)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null) throw new Exception("Missing CC0 texture: " + path);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new Exception("URP Lit shader unavailable");
            var normalImporter = AssetImporter.GetAtPath(normalPath) as TextureImporter;
            if (normalImporter == null) throw new Exception("Missing CC0 normal map: " + normalPath);
            if (normalImporter.textureType != TextureImporterType.NormalMap)
            {
                normalImporter.textureType = TextureImporterType.NormalMap;
                normalImporter.SaveAndReimport();
            }
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            var material = new Material(shader) { name = name, mainTexture = texture };
            material.SetTexture("_BumpMap", normal);
            material.EnableKeyword("_NORMALMAP");
            return material;
        }

        static GameObject Place(string path, string label, Vector3 position, float targetWidth, Terrain ground,
            Material primary, Material secondary)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) throw new Exception("Missing art gate model: " + path);
            var instance = UnityEngine.Object.Instantiate(asset);
            instance.name = label;
            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) throw new Exception("Asset has no renderers: " + path);
            if (primary != null) foreach (var renderer in renderers)
            {
                var original = renderer.sharedMaterials;
                var adapted = new Material[original.Length];
                for (int i = 0; i < original.Length; i++)
                {
                    var name = original[i] != null ? original[i].name.ToLowerInvariant() : "";
                    adapted[i] = secondary != null && name.Contains("plaster") ? secondary : primary;
                }
                renderer.sharedMaterials = adapted;
            }
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            if (bounds.size.x > .001f)
            {
                float scale = targetWidth / bounds.size.x;
                instance.transform.localScale *= scale;
            }
            renderers = instance.GetComponentsInChildren<Renderer>();
            bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            instance.transform.position += position - new Vector3(bounds.center.x, 0, bounds.center.z);
            bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            instance.transform.position += Vector3.up * (ground.SampleHeight(position) + ground.transform.position.y - bounds.min.y);
            return instance;
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
