using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools.ReferenceFirstPilot
{
    /// <summary>
    /// Isolated, reference-first Unity art study. NEVER modifies the game scene or routes.
    /// Invoke ONLY via Unity -batchmode -projectPath Unity
    /// -executeMethod Eldoria.EditorTools.ReferenceFirstPilot.ReferenceFirstPilotCapture.BuildAndCapture
    /// No claim of artistic approval is made by this tool.
    /// </summary>
    public static class ReferenceFirstPilotCapture
    {
        private const string Pilot = "Assets/Eldoria/ArtTests/ReferenceFirstPilot";
        private const string HeroPath = "Assets/Eldoria/ArtTests/OriginalHero/valoria_hero_fragment.fbx";
        private const string HeroTextures = "Assets/Eldoria/ArtTests/OriginalHero/Textures/";
        private const string TreePath = "Assets/Eldoria/Resources/WorldInventory/Tree01A.prefab";
        private const string CliffPath = "Assets/Eldoria/Resources/Valoria/SM_Cliffs_01.prefab";
        private const string RockPath = "Assets/Eldoria/Resources/WorldInventory/Rock01.prefab";
        private const string GatePath = "Assets/Eldoria/Resources/Valoria/Stone_Gate.prefab";
        private const string WallPath = "Assets/Eldoria/Resources/Valoria/Stone_Wall.prefab";
        private const int CaptureWidth = 1536;
        private const int CaptureHeight = 1024;

        [Serializable]
        private sealed class Report
        {
            public string stage = "ISOLATED_UNITY_LOOKDEV_NOT_ART_APPROVED";
            public string referencePath = "references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg";
            public string heroPath = HeroPath;
            public string treePath = TreePath;
            public string cliffPath = CliffPath;
            public string gatePath = GatePath;
            public string wallPath = WallPath;
            public int heroRenderers;
            public int treeInstances;
            public int cliffInstances;
            public int rockInstances;
            public int gatewayInstances;
            public int wallInstances;
            public int placeholderRoadTiles;
            public int width = CaptureWidth;
            public int height = CaptureHeight;
            public string[] captures;
            public bool licenseVerified = false;
            public bool artApproved = false;
            public string sourceCommit;
        }

        public static void BuildAndCapture()
        {
            if (!UnityEngine.Application.isBatchMode)
                throw new InvalidOperationException("Run in batchmode only: the current editor scene must not be replaced.");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var heroAsset = MustLoad(HeroPath);
            var treeAsset = MustLoad(TreePath);
            var cliffAsset = MustLoad(CliffPath);
            var rockAsset = MustLoad(RockPath);
            var gateAsset = MustLoad(GatePath);
            var wallAsset = MustLoad(WallPath);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                throw new InvalidOperationException("URP Lit unavailable. Do not modify global render settings as a workaround.");

            Directory.CreateDirectory(Pilot + "/Editor");
            Directory.CreateDirectory(Pilot + "/Materials");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ReferenceFirstValoriaLookdev";

            var group = new GameObject("REFERENCE FIRST — VISUAL STUDY ONLY");
            var geometryGroup = new GameObject("Existing source meshes, never edited");
            geometryGroup.transform.SetParent(group.transform);
            var stageGroup = new GameObject("BLOCKOUT terrain and road, not approved assets");
            stageGroup.transform.SetParent(group.transform);
            var vegetationGroup = new GameObject("Reusable candidate prefabs — license pending");
            vegetationGroup.transform.SetParent(group.transform);
            var fortificationGroup = new GameObject("Existing gate and wall prefabs — composition proof, license pending");
            fortificationGroup.transform.SetParent(group.transform);

            var hero = (GameObject)PrefabUtility.InstantiatePrefab(heroAsset);
            if (hero == null)
                throw new InvalidOperationException("Could not instantiate OriginalHero FBX.");
            hero.name = "OriginalHero source study — NOT visually approved";
            hero.transform.SetParent(geometryGroup.transform, true);
            int heroRenderers = CountRenderableMeshes(hero);
            if (heroRenderers < 7)
                throw new InvalidOperationException("Hero must expose 7+ actual renderable meshes, got " + heroRenderers);
            ApplyOriginalHeroMaterials(hero, shader);
            Bounds hb = RenderBounds(hero);
            float groundY = hb.min.y - 0.15f;

            var grassMaterial = StageMaterial("plateau-grass", shader, new Color(0.37f, 0.41f, 0.27f));
            var waterMaterial = StageMaterial("lake-water", shader, new Color(0.075f, 0.16f, 0.23f));
            var roadMaterial = StageMaterial("road-markers", shader, new Color(0.88f, 0.80f, 0.71f));
            var cliffRockMaterial = StageMaterial("cliff-rock", shader, new Color(.82f,.78f,.72f));
            var ground = Block("BLOCKOUT plateau (temporary)", stageGroup.transform,
                new Vector3(hb.center.x, groundY - 0.55f, hb.center.z + 7f),
                new Vector3(110f, 1.0f, 85f), grassMaterial);
            var water = Block("BLOCKOUT river/lake plane (temporary)", stageGroup.transform,
                new Vector3(hb.center.x, groundY - 7.7f, hb.center.z + 5f),
                new Vector3(220f, 0.25f, 200f), waterMaterial);
            ground.isStatic = false;
            water.isStatic = false;

            int roadCount = 0;
            // Temporary layout markers only; not claimed as authored medieval paving.
            for (int n = 0; n < 15; n++)
            {
                float z = hb.center.z + 42f - n * 5.1f;
                Block("BLOCKOUT stone route " + n, stageGroup.transform,
                    new Vector3(hb.center.x + 6.0f, groundY + 0.015f, z),
                    new Vector3(6.2f, 0.08f, 5.05f), roadMaterial);
                roadCount++;
            }

            int treeCount = 0;
            for (int i = 0; i < 24; i++)
            {
                float a = i * Mathf.PI * 2f / 24f;
                float x = hb.center.x + Mathf.Cos(a) * (45f + (i % 4) * 4f);
                float z = hb.center.z + Mathf.Sin(a) * (34f + (i % 3) * 5f);
                if (Mathf.Abs(x - hb.center.x) < 13f && z > hb.center.z) continue;
                PlaceRealAsset(treeAsset, vegetationGroup.transform, "Tree source " + i,
                    x, z, groundY, 11.5f + (i % 3) * 2.1f, i * 53f);
                treeCount++;
            }

            int cliffCount = 0;
            for (int i = 0; i < 14; i++)
            {
                bool onFront = i < 8;
                float x = onFront ? hb.center.x - 52f + i * 15.0f
                                  : hb.center.x + (i % 2 == 0 ? -53f : 53f);
                float z = onFront ? hb.center.z + 48f
                                  : hb.center.z - 39f + (i - 8) * 13f;
                var cliff = PlaceRealAsset(cliffAsset, vegetationGroup.transform, "Cliff source " + i,
                    x, z, groundY - 6.6f, 12.0f + i % 3, i * 21f);
                // Only the LOOKDEV instance receives the rock material; the source prefab is immutable.
                // The former bright-green/high-key imported hill texture overwhelms the medieval frame.
                foreach (var rr in cliff.GetComponentsInChildren<Renderer>(true))
                {
                    var slots = rr.sharedMaterials;
                    for (int k = 0; k < slots.Length; k++) slots[k] = cliffRockMaterial;
                    rr.sharedMaterials = slots;
                }
                cliffCount++;
            }

            int rockCount = 0;
            for (int i = 0; i < 12; i++)
            {
                float x = hb.center.x + ((i * 19) % 90) - 45f;
                float z = hb.center.z + ((i * 31) % 65) - 29f;
                if (Mathf.Abs(x - hb.center.x - 6.0f) < 8f) continue;
                PlaceRealAsset(rockAsset, vegetationGroup.transform, "Rock source " + i,
                    x, z, groundY, 3.5f + (i % 3), i * 37f);
                rockCount++;
            }
            if (treeCount < 12 || cliffCount < 10 || rockCount < 6)
                throw new InvalidOperationException("Real source environment did not reach pilot staging minimum.");

            // Make the fortress readable as a fortified settlement rather than an isolated townhouse.
            // Genuine existing gate/wall prefabs, not new meshes invented for the showcase.
            int gatewayCount = 0;
            int wallCount = 0;
            PlaceRealAsset(gateAsset, fortificationGroup.transform, "Lower gate — retained prefab source",
                hb.center.x + 6f, hb.center.z + 32f, groundY, 17f, 0f);
            gatewayCount++;
            PlaceRealAsset(gateAsset, fortificationGroup.transform, "Upper inner gate — retained prefab source",
                hb.center.x + 6f, hb.center.z - 24f, groundY, 12f, 0f);
            gatewayCount++;
            var wallPositions = new Vector3[] {
                new Vector3(hb.center.x - 20f,groundY,hb.center.z + 29f),
                new Vector3(hb.center.x + 32f,groundY,hb.center.z + 29f),
                new Vector3(hb.center.x - 24f,groundY,hb.center.z - 21f),
                new Vector3(hb.center.x + 31f,groundY,hb.center.z - 21f)
            };
            for (int w = 0; w < wallPositions.Length; w++)
            {
                var p = wallPositions[w];
                PlaceRealAsset(wallAsset, fortificationGroup.transform, "Stone curtain wall source " + w,
                    p.x, p.z, p.y, 9.8f, w < 2 ? 0f : 180f);
                wallCount++;
            }
            if (gatewayCount < 2 || wallCount < 4)
                throw new InvalidOperationException("Missing source fortification placement.");

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.48f, .53f, .62f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.51f, .61f, .69f);
            RenderSettings.fogDensity = .0032f;
            var sun = new GameObject("Warm late-afternoon direction").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1.0f, .80f, .63f);
            sun.intensity = 1.35f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(45f, -32f, 0f);

            var camera = new GameObject("Reference first camera — 1536 x 1024").AddComponent<Camera>();
            camera.orthographic = true;
            camera.nearClipPlane = .2f;
            camera.farClipPlane = 450f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = RenderSettings.fogColor;
            camera.allowHDR = true;

            string sceneFile = Pilot + "/ReferenceFirstValoriaLookdev.unity";
            if (!EditorSceneManager.SaveScene(scene, sceneFile))
                throw new IOException("Cannot save isolated pilot scene: " + sceneFile);
            AssetDatabase.SaveAssets();

            string output = Environment.GetEnvironmentVariable("ELDORIA_LOOKDEV_OUTPUT");
            if (string.IsNullOrWhiteSpace(output))
                output = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "../../artifacts/reference-first-pilot"));
            Directory.CreateDirectory(output);

            Vector3 look = hb.center + new Vector3(0f, 1.5f, 3f);
            SavePng(camera, output, "01-wide-camera.png", look + new Vector3(82f, 70f, 95f), look, 72f);
            SavePng(camera, output, "02-main-architecture.png", look + new Vector3(46f, 41f, 59f), look, 38f);
            var report = new Report
            {
                heroRenderers = heroRenderers,
                treeInstances = treeCount,
                cliffInstances = cliffCount,
                rockInstances = rockCount,
                gatewayInstances = gatewayCount,
                wallInstances = wallCount,
                placeholderRoadTiles = roadCount,
                captures = new[] {"01-wide-camera.png", "02-main-architecture.png"},
                sourceCommit = Environment.GetEnvironmentVariable("GITHUB_SHA") ?? "UNKNOWN_LOCAL_SOURCE"
            };
            File.WriteAllText(Path.Combine(output, "render-manifest.json"),
                JsonUtility.ToJson(report, true));
            Debug.Log("REFERENCE_FIRST_PILOT_RENDERED: " + output
                + " heroMeshes=" + heroRenderers + " trees=" + treeCount
                + " cliffs=" + cliffCount + " rocks=" + rockCount
                + " gates=" + gatewayCount + " curtainWalls=" + wallCount
                + " ART_APPROVED=false");
        }

        private static GameObject MustLoad(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) throw new FileNotFoundException("Required existing art source missing: " + path);
            return asset;
        }

        private static int CountRenderableMeshes(GameObject go)
        {
            int count = 0;
            foreach (var filter in go.GetComponentsInChildren<MeshFilter>(true))
                if (filter.sharedMesh != null && filter.sharedMesh.vertexCount > 0) count++;
            return count;
        }

        private static Bounds RenderBounds(GameObject go)
        {
            var rs = go.GetComponentsInChildren<Renderer>(true);
            bool started = false;
            Bounds b = new Bounds(go.transform.position, Vector3.zero);
            foreach (var r in rs)
            {
                if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
                if (!started) { b = r.bounds; started = true; }
                else b.Encapsulate(r.bounds);
            }
            if (!started || b.size.y < .001f)
                throw new InvalidOperationException("Imported prefab has no visible renderer bounds: " + go.name);
            return b;
        }

        private static GameObject PlaceRealAsset(GameObject source, Transform parent,
            string name, float x, float z, float baseY, float desiredHeight, float yaw)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(source);
            if (go == null || CountRenderableMeshes(go) < 1)
                throw new InvalidOperationException("Real mesh reference is broken: " + source.name);
            go.name = name;
            go.transform.SetParent(parent, true);
            var bound = RenderBounds(go);
            var scale = Mathf.Clamp(desiredHeight / bound.size.y, .005f, 20f);
            go.transform.localScale *= scale;
            go.transform.rotation = Quaternion.Euler(0, yaw, 0);
            go.transform.position = new Vector3(x, 0, z);
            bound = RenderBounds(go);
            go.transform.position += new Vector3(0, baseY - bound.min.y, 0);
            return go;
        }

        private static Material StageMaterial(string key, Shader shader, Color color)
        {
            string path = Pilot + "/Materials/BLOCKOUT_" + key + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.name = "BLOCKOUT only - " + key;
            material.shader = shader;
            material.SetColor("_BaseColor", color);
            string albedoName =
                key == "plateau-grass" ? "ground_albedo.png" :
                key == "road-markers" ? "stone_albedo.png" :
                key == "cliff-rock" ? "rock_albedo.png" : null;
            if (albedoName != null)
            {
                var texturePath = "Assets/Eldoria/ArtTests/GoldenSurfaceV2/Textures/" + albedoName;
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null) throw new FileNotFoundException("Existing authored PBR source texture missing: " + texturePath);
                material.SetTexture("_BaseMap", texture);
                material.SetTextureScale("_BaseMap", key == "cliff-rock" ? new Vector2(1.25f,1.25f) : new Vector2(2.7f,2.7f));
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject Block(string name, Transform parent,
            Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        private static void ApplyOriginalHeroMaterials(GameObject hero, Shader shader)
        {
            var cache = new Dictionary<string, Material>();
            int counter = 0;
            foreach (var renderer in hero.GetComponentsInChildren<Renderer>(true))
            {
                var originals = renderer.sharedMaterials;
                var updated = new Material[originals.Length];
                for (int i = 0; i < originals.Length; i++)
                {
                    var name = originals[i] == null ? "unknown" : originals[i].name;
                    if (!cache.TryGetValue(name, out var material))
                    {
                        var lower = name.ToLowerInvariant();
                        string texName = lower.Contains("oak") ? "oak" :
                            lower.Contains("slate") ? "slate" :
                            lower.Contains("cliff") ? "rock" :
                            lower.Contains("ancestral") ? "oldstone" : "limestone";
                        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(HeroTextures + texName + ".png");
                        if (tex == null) throw new FileNotFoundException("Original hero material texture: " + texName);
                        string matPath = Pilot + "/Materials/M_" + counter.ToString("D2") + ".mat";
                        material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                        if (material == null)
                        {
                            material = new Material(shader);
                            AssetDatabase.CreateAsset(material, matPath);
                        }
                        material.name = "ReferencePilot - " + name;
                        material.shader = shader;
                        material.SetTexture("_BaseMap", tex);
                        var color = lower.Contains("heraldic blue") ? new Color(.06f,.12f,.33f) :
                            lower.Contains("warm window") ? new Color(1f,.60f,.26f) :
                            lower.Contains("recess") ? new Color(.10f,.11f,.13f) : Color.white;
                        material.SetColor("_BaseColor", color);
                        EditorUtility.SetDirty(material);
                        cache[name] = material;
                        counter++;
                    }
                    updated[i] = material;
                }
                renderer.sharedMaterials = updated;
            }
        }

        private static void SavePng(Camera camera, string folder, string file,
            Vector3 position, Vector3 target, float orthographicSize)
        {
            camera.transform.position = position;
            camera.transform.LookAt(target);
            camera.orthographicSize = orthographicSize;
            var targetTexture = new RenderTexture(CaptureWidth, CaptureHeight, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            Texture2D tex = null;
            try
            {
                camera.targetTexture = targetTexture;
                camera.Render();
                RenderTexture.active = targetTexture;
                tex = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0,0,CaptureWidth,CaptureHeight), 0, 0);
                tex.Apply();
                File.WriteAllBytes(Path.Combine(folder,file), tex.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                Object.DestroyImmediate(tex);
                targetTexture.Release();
                Object.DestroyImmediate(targetTexture);
            }
        }
    }
}
