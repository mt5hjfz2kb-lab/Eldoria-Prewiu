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
