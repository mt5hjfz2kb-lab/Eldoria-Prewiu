using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools
{
    // Canonical zero-credit visual formula gate. It renders the real runtime Valoria,
    // never a replacement mockup, and records lightweight scene-complexity evidence.
    public static class ValoriaVisualFormulaGate
    {
        public static void Capture()
        {
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var state = new PlayerState
            {
                BastionLevel = 2,
                SawmillLevel = 1,
                BarracksLevel = 1,
                CorruptionDiscovered = true
            };
            VisualWorld.Create(true, state);

            var camera = Camera.main;
            if (camera == null) throw new System.Exception("Valoria camera was not created");

            const string folder = "VisualFormulaCaptures";
            Directory.CreateDirectory(folder);

            var officialPosition = new Vector3(18.2f, 14.6f, -25.8f);
            var officialTarget = new Vector3(0, 3.15f, 5.8f);
            var sawmillShift = new Vector3(-7f, -1.55f, -8.6f);
            var barracksShift = new Vector3(7f, -1.55f, -9.8f);
            var bastionShift = new Vector3(0f, 1.5f, 3.2f);

            Save(camera, folder + "/formula-overview-19.png", officialPosition, officialTarget, 19f, 1280, 720);
            Save(camera, folder + "/formula-overview-12.png", officialPosition, officialTarget, 12f, 1280, 720);
            Save(camera, folder + "/formula-overview-mobile.png", officialPosition, officialTarget, 12f, 390, 844);

            Save(camera, folder + "/formula-sawmill-12.png", officialPosition + sawmillShift, officialTarget + sawmillShift, 12f, 1280, 720);
            Save(camera, folder + "/formula-sawmill-9.png", officialPosition + sawmillShift, officialTarget + sawmillShift, 9f, 1280, 720);
            Save(camera, folder + "/formula-barracks-12.png", officialPosition + barracksShift, officialTarget + barracksShift, 12f, 1280, 720);
            Save(camera, folder + "/formula-barracks-9.png", officialPosition + barracksShift, officialTarget + barracksShift, 9f, 1280, 720);
            Save(camera, folder + "/formula-bastion-12.png", officialPosition + bastionShift, officialTarget + bastionShift, 12f, 1280, 720);
            Save(camera, folder + "/formula-bastion-9.png", officialPosition + bastionShift, officialTarget + bastionShift, 9f, 1280, 720);

            WriteMetrics(folder + "/formula-metrics.json");
            WriteMaterialEvidence(folder + "/formula-materials.json");
            Debug.Log("Valoria Visual Formula gate saved to " + Path.GetFullPath(folder));
            UnityEditor.EditorApplication.Exit(0);
        }

        public static void CaptureRescueDistrict()
        {
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var state = new PlayerState
            {
                BastionLevel = 2,
                SawmillLevel = 1,
                BarracksLevel = 1,
                CorruptionDiscovered = true
            };
            VisualWorld.Create(true, state);
            HideVisualFamilyForDistrictEvidence("Bastion");

            var residential = InstallRescueModule(
                "Assets/Eldoria/ArtTests/ImageTo3D/Source/ValoriaRescueDistrict/ResidentialTerraceRock.glb",
                "Production rescue · ResidentialTerraceRock",
                new Vector3(-1.75f, .40f, 1.10f), 0f, 5.45f);
            var seamWest = InstallRescueModule(
                "Assets/Eldoria/ArtTests/ImageTo3D/Source/ValoriaRescueDistrict/RockTerrainSeamFiller.glb",
                "Production rescue · Seam west",
                new Vector3(-6.15f, .20f, -2.55f), 28f, 3.10f);
            var seamResidential = InstallRescueModule(
                "Assets/Eldoria/ArtTests/ImageTo3D/Source/ValoriaRescueDistrict/RockTerrainSeamFiller.glb",
                "Production rescue · Seam residential",
                new Vector3(-1.70f, .18f, .95f), 112f, 4.20f);
            var seamEast = InstallRescueModule(
                "Assets/Eldoria/ArtTests/ImageTo3D/Source/ValoriaRescueDistrict/RockTerrainSeamFiller.glb",
                "Production rescue · Seam east",
                new Vector3(5.85f, .20f, -3.75f), 205f, 2.95f);

            if (residential == null || seamWest == null || seamResidential == null || seamEast == null)
                throw new System.Exception("Valoria rescue district did not stage all required composed modules.");

            var camera = Camera.main;
            if (camera == null) throw new System.Exception("Valoria camera was not created");

            const string folder = "VisualFormulaCaptures";
            Directory.CreateDirectory(folder);
            var districtShift = new Vector3(0f, -1.75f, -5.75f);
            var officialPosition = new Vector3(18.2f, 14.6f, -25.8f) + districtShift;
            var officialTarget = new Vector3(0, 3.15f, 5.8f) + districtShift;
            Save(camera, folder + "/production-district-19.png", officialPosition, officialTarget, 19f, 1280, 720);
            Save(camera, folder + "/production-district-12.png", officialPosition, officialTarget, 12f, 1280, 720);
            Save(camera, folder + "/production-district-9.png", officialPosition, officialTarget, 9f, 1280, 720);
            Save(camera, folder + "/production-district-mobile.png", officialPosition, officialTarget, 12f, 390, 844);
            WriteMetrics(folder + "/production-district-metrics.json");
            File.WriteAllText(folder + "/production-district-evidence.json",
                "{\n" +
                "  \"schema_version\": 1,\n" +
                "  \"topology\": \"REAL_VISUALWORLD_ISOLATED_EDITOR_SCENE\",\n" +
                "  \"gameplay_mesh_dependency\": false,\n" +
                "  \"rescued_assets\": [\"ResidentialTerraceRock\",\"RockTerrainSeamFiller\",\"TerraceStairRock\"],\n" +
                "  \"composed_assets\": [\"ResidentialTerraceRock\",\"RockTerrainSeamFiller x3\"],\n" +
                "  \"deferred_after_fit_test\": [\"TerraceStairRock: rescued and Surface-v1-ready, but omitted from this district because its large terrace mass duplicates the local vertical transition and overcrowds the parcel\"],\n" +
                "  \"canonical_assets\": [\"Aserradero\",\"Cuartel\",\"Bastion\"],\n" +
                "  \"official_zooms\": [19,12,9],\n" +
                "  \"surface_policy\": \"VALORIA_VISUAL_FORMULA_v1\",\n" +
                "  \"district_focus_visual_suppression\": \"Bastion renderers and Bastion-named lights hidden only in this isolated evidence scene so the lower production district can be judged; canonical VisualWorld/runtime is unchanged\",\n" +
                "  \"promotion\": \"EVIDENCE_ONLY_DO_NOT_MUTATE_VISUALWORLD\"\n" +
                "}\n");
            Debug.Log("Valoria production rescue district evidence saved to " + Path.GetFullPath(folder));
            UnityEditor.EditorApplication.Exit(0);
        }

        static void HideVisualFamilyForDistrictEvidence(string token)
        {
            foreach (var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                if (renderer.gameObject.name.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    renderer.enabled = false;
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (light.gameObject.name.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    light.enabled = false;
        }

        static GameObject InstallRescueModule(string assetPath, string label, Vector3 groundAnchor, float yaw, float targetSpan)
        {
            if (!File.Exists(assetPath))
                throw new FileNotFoundException("Rescue district asset missing", assetPath);

            UnityEditor.AssetDatabase.ImportAsset(assetPath, UnityEditor.ImportAssetOptions.ForceSynchronousImport | UnityEditor.ImportAssetOptions.ForceUpdate);
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null) throw new System.Exception("glTFast failed to import rescue asset " + assetPath);

            var root = Object.Instantiate(prefab);
            root.name = label;
            root.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            var bounds = BoundsOf(root);
            var span = Mathf.Max(bounds.size.x, bounds.size.z);
            if (span <= .001f) throw new System.Exception("Rescue asset has unusable bounds: " + assetPath);
            root.transform.localScale *= targetSpan / span;
            bounds = BoundsOf(root);
            root.transform.position += groundAnchor - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            NormalizeRescueMaterials(root);
            return root;
        }

        static Bounds BoundsOf(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) throw new System.Exception("Rescue asset contains no renderers: " + root.name);
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        static void NormalizeRescueMaterials(GameObject root)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new System.Exception("URP/Lit unavailable for rescue district.");

            var cache = new Dictionary<string, Material>();
            bool residential = root.name.IndexOf("ResidentialTerraceRock", System.StringComparison.OrdinalIgnoreCase) >= 0;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>())
            {
                var slots = renderer.sharedMaterials;
                for (int i = 0; i < slots.Length; i++)
                {
                    var source = slots[i];
                    if (source == null) continue;

                    // ResidentialTerraceRock is authored by the rescue step with stable semantic
                    // material slot order: 0 Stone, 1 Rock, 2 Roof, 3 Timber. glTF import may
                    // rename material assets, so production evidence must not depend on names.
                    string semantic = residential
                        ? (i == 0 ? "stone" : i == 1 ? "rock" : i == 2 ? "roof" : i == 3 ? "timber" : "neutral")
                        : (source.name ?? "").ToLowerInvariant();
                    string key = semantic + "|" + source.name;
                    if (!cache.TryGetValue(key, out var normalized))
                    {
                        Texture baseColorTexture = null;
                        foreach (var prop in source.GetTexturePropertyNames())
                        {
                            var lowerProp = prop.ToLowerInvariant();
                            if (lowerProp.Contains("basecolor") || lowerProp == "_basemap" || lowerProp == "_maintex")
                            {
                                baseColorTexture = source.GetTexture(prop);
                                if (baseColorTexture != null) break;
                            }
                        }

                        var color = baseColorTexture != null
                            ? (semantic.Contains("stone")
                                ? new Color(1.15f, 1.08f, .96f, 1f)
                                : semantic.Contains("rock")
                                    ? new Color(.78f, .82f, .78f, 1f)
                                    : semantic.Contains("timber")
                                        ? new Color(1.18f, 1.02f, .84f, 1f)
                                        : semantic.Contains("roof")
                                            ? new Color(.72f, .78f, .82f, 1f)
                                            : Color.white)
                            : (semantic.Contains("stone")
                                ? new Color(.43f, .36f, .27f, 1f)
                                : semantic.Contains("rock")
                                    ? new Color(.15f, .16f, .15f, 1f)
                                    : semantic.Contains("timber")
                                        ? new Color(.34f, .18f, .075f, 1f)
                                        : semantic.Contains("roof")
                                            ? new Color(.085f, .095f, .105f, 1f)
                                            : new Color(.25f, .24f, .21f, 1f));

                        normalized = new Material(shader) { name = "Valoria v1 · " + semantic + " · " + source.name };
                        if (baseColorTexture != null) normalized.SetTexture("_BaseMap", baseColorTexture);
                        normalized.SetColor("_BaseColor", color);
                        normalized.SetFloat("_Metallic", 0f);
                        normalized.SetFloat("_Smoothness", semantic.Contains("rock") ? .03f : .08f);
                        cache[key] = normalized;
                    }
                    slots[i] = normalized;
                }
                renderer.sharedMaterials = slots;
            }
        }

        static string TextureName(Material material, string property)
        {
            if (material == null || !material.HasProperty(property)) return "";
            var texture = material.GetTexture(property);
            return texture != null ? texture.name : "";
        }

        static string FloatValue(Material material, string property)
        {
            if (material == null || !material.HasProperty(property)) return "null";
            return material.GetFloat(property).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        static string TexturePropertiesJson(Material material)
        {
            if (material == null || material.shader == null) return "[]";
            var shader = material.shader;
            var rows = new List<string>();
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                if (shader.GetPropertyType(i) != ShaderPropertyType.Texture) continue;
                var property = shader.GetPropertyName(i);
                if (!material.HasProperty(property)) continue;
                var texture = material.GetTexture(property);
                if (texture == null) continue;
                rows.Add("{\"property\":\"" + Escape(property) +
                         "\",\"texture\":\"" + Escape(texture.name) + "\"}");
            }
            return "[" + string.Join(",", rows) + "]";
        }

        static string Escape(string value)
        {
            return (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        static void WriteMaterialEvidence(string path)
        {
            var rows = new List<string>();
            foreach (var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if (!renderer.gameObject.activeInHierarchy) continue;
                var lower = renderer.gameObject.name.ToLowerInvariant();
                string target = lower.Contains("aserradero") ? "sawmill" :
                                lower.Contains("cuartel") ? "barracks" :
                                lower.Contains("bastion") ? "bastion" : "";
                if (string.IsNullOrEmpty(target)) continue;

                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null) continue;
                    var baseMap = TextureName(material, "_BaseMap");
                    if (string.IsNullOrEmpty(baseMap)) baseMap = TextureName(material, "_MainTex");
                    var normalMap = TextureName(material, "_BumpMap");
                    var metallicMap = TextureName(material, "_MetallicGlossMap");
                    var occlusionMap = TextureName(material, "_OcclusionMap");
                    rows.Add(
                        "    {\"target\":\"" + target +
                        "\",\"renderer\":\"" + Escape(renderer.gameObject.name) +
                        "\",\"material\":\"" + Escape(material.name) +
                        "\",\"shader\":\"" + Escape(material.shader != null ? material.shader.name : "") +
                        "\",\"base_map\":\"" + Escape(baseMap) +
                        "\",\"normal_map\":\"" + Escape(normalMap) +
                        "\",\"metallic_gloss_map\":\"" + Escape(metallicMap) +
                        "\",\"occlusion_map\":\"" + Escape(occlusionMap) +
                        "\",\"texture_properties\":" + TexturePropertiesJson(material) +
                        ",\"metallic\":" + FloatValue(material, "_Metallic") +
                        ",\"smoothness\":" + FloatValue(material, "_Smoothness") + "}"
                    );
                }
            }

            File.WriteAllText(path,
                "{\n  \"schema_version\": 1,\n  \"materials\": [\n" +
                string.Join(",\n", rows) +
                "\n  ]\n}\n");
        }

        static void WriteMetrics(string path)
        {
            int renderers = 0;
            long triangles = 0;
            var materialNames = new HashSet<string>();
            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
                renderers++;
                foreach (var m in r.sharedMaterials)
                    if (m != null) materialNames.Add(m.name);
            }

            foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
            {
                var mesh = mf.sharedMesh;
                if (mesh == null) continue;
                for (int s = 0; s < mesh.subMeshCount; s++)
                    triangles += (long)mesh.GetIndexCount(s) / 3L;
            }

            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            var json =
                "{\n" +
                "  \"schema_version\": 1,\n" +
                "  \"active_renderers\": " + renderers + ",\n" +
                "  \"unique_materials\": " + materialNames.Count + ",\n" +
                "  \"scene_triangles\": " + triangles + ",\n" +
                "  \"lights\": " + lights.Length + ",\n" +
                "  \"targets\": [\"overview\",\"sawmill\",\"barracks\",\"bastion\"],\n" +
                "  \"official_zooms\": [19,12,9]\n" +
                "}\n";
            File.WriteAllText(path, json);
        }

        static void Save(Camera camera, string path, Vector3 position, Vector3 target, float size, int width, int height)
        {
            camera.transform.position = position;
            camera.transform.LookAt(target);
            camera.orthographic = true;
            camera.orthographicSize = size;

            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            try
            {
                camera.targetTexture = rt;
                camera.Render();
                RenderTexture.active = rt;
                var image = new Texture2D(width, height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
                Object.DestroyImmediate(image);
            }
            finally
            {
                camera.targetTexture = null;
                RenderTexture.active = previous;
                rt.Release();
                Object.DestroyImmediate(rt);
            }
        }
    }
}
