#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Eldoria.Experimentos.Ciudad2D;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.Experimentos.Ciudad2DDepthPilot.Editor
{
    /// <summary>
    /// Reproducible same-camera A/B native Unity motion proof, never a video of a static JPEG.
    /// A is the byte-identical archived Premium V6 Unity scene. B is a new opt-in scene.
    /// Outputs direct Camera.Render RenderTexture frames and source/frame SHA-256 manifests.
    /// </summary>
    public static class DepthPilotNativeCapture
    {
        private const string Root = "Assets/Experimentos/Ciudad2DDepthPilot/";
        private const string Reference =
            "Assets/Experimentos/Ciudad2D/QA/ArchivedScenes/Experimento_Ciudad2D_PremiumV6_44133747143.unity";
        private const string ReferenceSha256 =
            "aa566bf6a075868ad852ac865aa8a8cb867d1ade601ac3dd74e8e091f44f0b06";
        private const string PilotScene = Root + "Valoria_Depth_B.unity";
        private const string Output = "Ciudad2DDepthPilotEvidence";
        private const string Generated = Root + "Generated/";
        private const int FramesPerTrajectory = 18;
        private const float FixedFps = 12f;

        [Serializable]
        private sealed class FrameRecord
        {
            public string variant, trajectory, file, sha256;
            public int frame, width, height;
            public float zoom, focusX, focusY, seconds;
            public float actualCameraX, actualCameraY, actualOrthographicSize;
        }

        [Serializable]
        private sealed class Evidence
        {
            public string engine = "Unity";
            public string mode = "Editor_RenderTexture";
            public string comparison = "immutable Premium V6 source vs isolated depth pilot B";
            public string baseline_scene = Reference;
            public string baseline_scene_sha256 = ReferenceSha256;
            public string pilot_scene = PilotScene;
            public string motion_capture = "actual rendered frames; no interpolated stills";
            public string source_authority = "art/ciudad-2d-hitos-1-5-20261116@10dbc197d48ea8517c6a3dd6c22f1a2abf75d508";
            public string mobile_device_test = "NOT_PERFORMED";
            public string webgl_test = "NOT_PERFORMED";
            public string art_approved = "NO";
            public string gameplay_changed = "NO";
            public int fps = (int)FixedFps;
            public int frames_per_trajectory = FramesPerTrajectory;
            public FrameRecord[] frames;
        }

        private struct CameraPose
        {
            public float Zoom;
            public Vector2 Focus;
            public CameraPose(float zoom, Vector2 focus) { Zoom = zoom; Focus = focus; }
        }

        [MenuItem("Eldoria/Experimentos/Valoria profundidad/Crear escena B (no tocar V6)")]
        public static void BuildOnly()
        {
            ValidateArchive();
            BuildB();
        }

        [MenuItem("Eldoria/Experimentos/Valoria profundidad/Capturar movimiento A-B real")]
        public static void Capture()
        {
            if (!UnityEngine.Application.isBatchMode &&
                !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                ValidateArchive();
                CreateWorkerArt();
                EnsureContactArt();
                BuildB();
                var output = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", Output));
                if (Directory.Exists(output))
                    throw new InvalidOperationException("Evidence folder already exists; refuse to mix captures");
                Directory.CreateDirectory(output);
                var records = new List<FrameRecord>(120);
                CaptureVariant("A", Reference, output, records);
                CaptureVariant("B", PilotScene, output, records);
                ValidateArchive(); // post-render byte identity, not just preflight
                VerifyPairs(records);
                var manifest = new Evidence { frames = records.ToArray() };
                File.WriteAllText(Path.Combine(output, "evidence.json"),
                    JsonUtility.ToJson(manifest, true), new UTF8Encoding(false));
                Debug.Log("[VALORIA_DEPTH_PILOT] REAL_UNITY_A_B_FRAME_PAIRS " +
                          records.Count + " frames (Editor only), V6 reference preserved");
            }
            finally
            {
                if (!UnityEngine.Application.isBatchMode && setup != null && setup.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }

        private static string Physical(string assetPath) =>
            Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", assetPath));

        private static string Sha(byte[] bytes)
        {
            using (var h = SHA256.Create())
                return BitConverter.ToString(h.ComputeHash(bytes))
                    .Replace("-", "").ToLowerInvariant();
        }

        private static void ValidateArchive()
        {
            string source = Physical(Reference);
            if (!File.Exists(source)) throw new FileNotFoundException("Premium V6 source", source);
            string actual = Sha(File.ReadAllBytes(source));
            if (actual != ReferenceSha256)
                throw new InvalidOperationException("Premium V6 source changed: " + actual);
        }

        private static Dictionary<string, CityBuilding> Buildings(Scene scene)
        {
            var map = new Dictionary<string, CityBuilding>(StringComparer.Ordinal);
            foreach (var b in UnityEngine.Object.FindObjectsByType<CityBuilding>(
                FindObjectsSortMode.None))
                if (b.gameObject.scene == scene) map.Add(b.BuildingId, b);
            if (map.Count != 6 || !map.ContainsKey("bastion") ||
                !map.ContainsKey("aserradero") || !map.ContainsKey("granero"))
                throw new InvalidOperationException("Unexpected V6 building contract");
            return map;
        }

        private static void PrepareStage(Scene scene)
        {
            var bridge = UnityEngine.Object.FindObjectsByType<City2DStateBridge>(
                FindObjectsSortMode.None).SingleOrDefault(x => x.gameObject.scene == scene);
            if (bridge == null || bridge.IsLiveBound)
                throw new InvalidOperationException("Expected immutable read-only trial bridge");
            bridge.RebindSceneBuildings();
            bridge.SetDemoBastionLevel(5);
            foreach (var shadow in UnityEngine.Object.FindObjectsByType<City2DPremiumShadowGate>(
                FindObjectsSortMode.None))
                if (shadow.gameObject.scene == scene) shadow.Refresh();
            var map = Buildings(scene);
            if (map["aserradero"].PlotPhase != City2DPlotPhase.Built ||
                map["granero"].PlotPhase != City2DPlotPhase.Built ||
                map["bastion"].SelectedArtLevel != 5)
                throw new InvalidOperationException("Fixture is not at honest Bastion V stage");
        }

        private static void BuildB()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(PilotScene) != null)
            {
                // Deliberately never rewrite a previous B candidate.
                return;
            }
            CreateWorkerArt();
            EnsureContactArt();
            var sourceScene = EditorSceneManager.OpenScene(Reference, OpenSceneMode.Single);
            if (!EditorSceneManager.SaveScene(sourceScene, PilotScene, true))
                throw new InvalidOperationException("Cannot create an isolated V6 scene copy");
            var scene = EditorSceneManager.OpenScene(PilotScene, OpenSceneMode.Single);
            var buildings = Buildings(scene);
            var root = new GameObject("DepthPilot_B_OptIn_NoPersistentState");
            if (root.scene != scene)
                throw new InvalidOperationException("Opt-in root was not created in B scene");
            CorrectLocalOcclusion(scene, buildings);
            foreach (var id in new[] { "bastion", "aserradero", "granero" })
                InstallContact(root.transform, buildings[id]);
            InstallWorker(root.transform, buildings["aserradero"], "Leñador", "ToolAxe.png",
                new Vector3(0.53f, -0.26f, 0f), 0.38f, 0f, true);
            InstallWorker(root.transform, buildings["granero"], "Almacen", "ToolSack.png",
                new Vector3(-0.46f, -0.24f, 0f), 0.34f, 1.2f, false);
            PrepareStage(scene);
            if (!EditorSceneManager.SaveScene(scene, PilotScene))
                throw new InvalidOperationException("B candidate did not save");
            ValidateArchive();
        }

        private static void CorrectLocalOcclusion(Scene scene,
            IDictionary<string, CityBuilding> buildings)
        {
            foreach (var renderer in UnityEngine.Object.FindObjectsByType<SpriteRenderer>(
                FindObjectsSortMode.None))
            {
                if (renderer.gameObject.scene != scene) continue;
                string name = renderer.gameObject.name;
                if (name.StartsWith("LuzEmisiva_", StringComparison.Ordinal))
                {
                    // The baseline hard-codes 9000, forcing ambient light in front
                    // of ALL roofs and walls. Tie light to actual ground/world Y.
                    renderer.sortingOrder =
                        -Mathf.RoundToInt(renderer.transform.position.y * 100f) + 1;
                }
                if (!name.StartsWith("AmbienteVivo_smoke_", StringComparison.Ordinal)) continue;
                // A chimney plume must be behind the roof silhouette whenever it overlaps,
                // not forced to the front by an arbitrary large order.
                foreach (var id in new[] { "aserradero", "cuartel", "granero" })
                {
                    if (!name.Contains(id) || !buildings.TryGetValue(id, out var owner)) continue;
                    renderer.sortingOrder = owner.GetComponent<SpriteRenderer>().sortingOrder - 1;
                    break;
                }
            }
            // Keep the painted premium shadows untouched, but place them behind
            // the separately measurable inner contact that anchors to the alpha-foot.
            foreach (var id in new[] { "aserradero", "granero" })
            {
                var existing = GameObject.Find("SombraPremium_" + id);
                if (existing == null || existing.scene != scene)
                    throw new InvalidOperationException("Missing premium painted shadow " + id);
                var visual = existing.GetComponent<SpriteRenderer>();
                if (visual == null) throw new InvalidOperationException("Shadow renderer missing");
                visual.sortingOrder = buildings[id].GetComponent<SpriteRenderer>().sortingOrder - 2;
            }
            // No new geometry or altered textures: never hide plinths behind foliage.
        }


        private static void EnsureContactArt()
        {
            const string filename = "SupportContact.png";
            string path = Generated + filename;
            Directory.CreateDirectory(Physical(Generated));
            if (!File.Exists(Physical(path)))
            {
                const int w = 256, h = 96;
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                try
                {
                    for (int y = 0; y < h; ++y)
                        for (int x = 0; x < w; ++x)
                        {
                            float dx = (x + .5f - w * .5f) / (w * .47f);
                            float dy = (y + .5f - h * .5f) / (h * .41f);
                            float e = Mathf.Max(0f, 1f - dx * dx - dy * dy);
                            float alpha = .14f * e * e;
                            tex.SetPixel(x, y, new Color(.16f, .13f, .22f, alpha));
                        }
                    tex.Apply();
                    File.WriteAllBytes(Physical(path), tex.EncodeToPNG());
                }
                finally { UnityEngine.Object.DestroyImmediate(tex); }
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Contact texture import failed");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 160f;
            importer.spriteAlignment = (int)SpriteAlignment.Center;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static void InstallContact(Transform root, CityBuilding owner)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Generated + "SupportContact.png");
            if (sprite == null) throw new InvalidOperationException("Missing inner contact alpha");
            var built = owner.GetComponent<SpriteRenderer>();
            if (built == null || built.sprite == null)
                throw new InvalidOperationException("Contact owner has no premium sprite");

            // Measure actual lowest painted pixels; the PNG canvas rect is not the foot.
            // Use a support band rather than a guessed offset or a blurred rectangular matte.
            string source = Physical(AssetDatabase.GetAssetPath(built.sprite));
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            Vector3 foot;
            try
            {
                if (!ImageConversion.LoadImage(tex, File.ReadAllBytes(source), false))
                    throw new InvalidOperationException("Cannot inspect actual alpha support");
                var data = tex.GetPixels32();
                int bottom = tex.height;
                for (int y = 0; y < tex.height; ++y)
                    for (int x = 0; x < tex.width; ++x)
                        if (data[y * tex.width + x].a > 80)
                            bottom = Math.Min(bottom, y);
                if (bottom == tex.height) throw new InvalidOperationException("Empty building alpha");
                double total = 0, weightedX = 0;
                int top = Math.Min(tex.height - 1, bottom + Math.Max(4, tex.height / 28));
                for (int y = bottom; y <= top; ++y)
                    for (int x = 0; x < tex.width; ++x)
                    {
                        byte alpha = data[y * tex.width + x].a;
                        if (alpha < 80) continue;
                        total += alpha;
                        weightedX += x * alpha;
                    }
                float supportX = total > 0 ? (float)(weightedX / total) : tex.width * .5f;
                float localX = (supportX - built.sprite.pivot.x) /
                    built.sprite.pixelsPerUnit * owner.transform.lossyScale.x;
                float localY = (bottom - built.sprite.pivot.y) /
                    built.sprite.pixelsPerUnit * owner.transform.lossyScale.y;
                foot = new Vector3(localX, localY, 0f);
            }
            finally { UnityEngine.Object.DestroyImmediate(tex); }

            var obj = new GameObject("ContactoFisico_" + owner.BuildingId);
            obj.transform.SetParent(root, false);
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            float footprintWidth = built.bounds.size.x * .58f;
            obj.transform.localScale = new Vector3(
                footprintWidth / sprite.bounds.size.x,
                footprintWidth * .16f / sprite.bounds.size.y, 1f);
            obj.AddComponent<DepthPilotContact>().Configure(owner, renderer, foot);
            if (obj.GetComponent<Collider2D>() != null)
                throw new InvalidOperationException("Contact shadow must not block interaction");
        }

        private static void InstallWorker(Transform root, CityBuilding owner,
            string title, string toolFile, Vector3 offset, float scale, float phase, bool sawmill)
        {
            var bodySprite = AssetDatabase.LoadAssetAtPath<Sprite>(Generated + "Worker.png");
            var toolSprite = AssetDatabase.LoadAssetAtPath<Sprite>(Generated + toolFile);
            if (bodySprite == null || toolSprite == null)
                throw new InvalidOperationException("Procedural pilot sprite import failed");

            var go = new GameObject("Actividad_" + title + "_" + owner.BuildingId);
            go.transform.SetParent(root, false);
            go.transform.position = owner.transform.position + offset;
            go.transform.localScale = Vector3.one * scale;
            var body = new GameObject("WorkerBody");
            body.transform.SetParent(go.transform, false);
            var bodyRenderer = body.AddComponent<SpriteRenderer>();
            bodyRenderer.sprite = bodySprite;
            var tool = new GameObject("WorkTool");
            tool.transform.SetParent(go.transform, false);
            tool.transform.localPosition = new Vector3(0.14f, 0.35f, 0f);
            var toolRenderer = tool.AddComponent<SpriteRenderer>();
            toolRenderer.sprite = toolSprite;
            tool.transform.localScale = Vector3.one * 0.72f;
            go.AddComponent<DepthPilotActivity>().Configure(owner, bodyRenderer,
                toolRenderer, phase, sawmill);
            if (go.GetComponentsInChildren<Collider2D>().Length != 0)
                throw new InvalidOperationException("Pure visual worker cannot intercept taps");
        }

        private static void CaptureVariant(string variant, string path,
            string output, List<FrameRecord> records)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            PrepareStage(scene);
            var buildings = Buildings(scene);
            var camera = Camera.main;
            var fit = camera != null ? camera.GetComponent<City2DCameraFit>() : null;
            if (camera == null || fit == null || !camera.orthographic)
                throw new InvalidOperationException("Missing certified ortho city camera");

            var bastion = (Vector2)buildings["bastion"].transform.position;
            var sawmill = (Vector2)buildings["aserradero"].transform.position;
            // A and B derive exactly the same path from unmodified building transforms.
            for (int pathIndex = 0; pathIndex < 3; pathIndex++)
            {
                string trajectory = pathIndex == 0 ? "pan" :
                    pathIndex == 1 ? "approach" : "retreat";
                fit.SetEvidenceViewport(390, 844);
                for (int f = 0; f < FramesPerTrajectory; f++)
                {
                    float t = f / (float)(FramesPerTrajectory - 1);
                    CameraPose pose = MakePose(pathIndex, t, bastion, sawmill);
                    float seconds = (pathIndex * FramesPerTrajectory + f) / FixedFps;
                    SampleAmbience(scene, seconds);
                    ApplyPose(fit, pose);
                    AddCapture(variant, trajectory, f, 390, 844,
                        pose, seconds, camera, output, records);
                    if (f == FramesPerTrajectory / 2)
                    {
                        fit.SetEvidenceViewport(1170, 2532);
                        SampleAmbience(scene, seconds);
                        ApplyPose(fit, pose);
                        AddCapture(variant, trajectory, f, 1170, 2532,
                            pose, seconds, camera, output, records);
                        fit.SetEvidenceViewport(390, 844);
                    }
                }
            }
        }

        private static CameraPose MakePose(int index, float t, Vector2 bastion, Vector2 sawmill)
        {
            t = Mathf.Clamp01(t);
            float smooth = t * t * (3f - 2f * t);
            if (index == 0)
            {
                // Fixed isometric camera: genuine x AND y movement, no fake building parallax.
                return new CameraPose(1.52f,
                    bastion + Vector2.Lerp(new Vector2(-0.65f, -0.54f),
                        new Vector2(0.65f, 0.54f), smooth));
            }
            if (index == 1)
            {
                return new CameraPose(Mathf.Lerp(1.03f, 1.87f, smooth),
                    Vector2.Lerp(bastion, Vector2.Lerp(bastion, sawmill, 0.64f), smooth));
            }
            return new CameraPose(Mathf.Lerp(1.87f, 1f, smooth),
                Vector2.Lerp(Vector2.Lerp(bastion, sawmill, 0.64f), bastion, smooth));
        }

        private static void ApplyPose(City2DCameraFit fit, CameraPose pose)
        {
            fit.SetEvidenceZoom(pose.Zoom);
            fit.SetEvidenceFocus(pose.Focus);
        }

        private static void SampleAmbience(Scene scene, float time)
        {
            foreach (var motion in UnityEngine.Object.FindObjectsByType<City2DAmbientMotion>(
                FindObjectsSortMode.None))
                if (motion.gameObject.scene == scene) motion.EvaluateAtTime(time);
            foreach (var activity in UnityEngine.Object.FindObjectsByType<DepthPilotActivity>(
                FindObjectsSortMode.None))
                if (activity.gameObject.scene == scene) activity.Sample(time);
        }

        private static void AddCapture(string variant, string trajectory, int frame,
            int width, int height, CameraPose pose, float seconds, Camera camera,
            string folder, List<FrameRecord> records)
        {
            string file = variant + "_" + trajectory + "_" + frame.ToString("D2") +
                "_" + width + "x" + height + ".png";
            var bytes = Render(camera, width, height);
            File.WriteAllBytes(Path.Combine(folder, file), bytes);
            records.Add(new FrameRecord {
                variant = variant, trajectory = trajectory, frame = frame,
                file = file, sha256 = Sha(bytes),
                width = width, height = height,
                focusX = pose.Focus.x, focusY = pose.Focus.y,
                zoom = pose.Zoom, seconds = seconds,
                actualCameraX = camera.transform.position.x,
                actualCameraY = camera.transform.position.y,
                actualOrthographicSize = camera.orthographicSize
            });
        }

        private static byte[] Render(Camera camera, int width, int height)
        {
            RenderTexture oldActive = RenderTexture.active;
            RenderTexture oldTarget = camera.targetTexture;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D image = null;
            try
            {
                rt.Create();
                camera.targetTexture = rt;
                camera.aspect = (float)width / height;
                camera.Render();
                RenderTexture.active = rt;
                image = new Texture2D(width, height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply();
                var png = image.EncodeToPNG();
                if (png == null || png.Length < 8000)
                    throw new InvalidOperationException("Empty true Unity frame");
                return png;
            }
            finally
            {
                camera.targetTexture = oldTarget;
                RenderTexture.active = oldActive;
                if (image != null) UnityEngine.Object.DestroyImmediate(image);
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }

        private static void VerifyPairs(List<FrameRecord> frames)
        {
            if (frames.Count != 2 * 3 * (FramesPerTrajectory + 1))
                throw new InvalidOperationException("Expected 114 native A/B frames");
            var groups = frames.GroupBy(f => f.trajectory + ":" + f.frame + ":" +
                f.width + "x" + f.height);
            foreach (var group in groups)
            {
                var pair = group.OrderBy(x => x.variant).ToArray();
                if (pair.Length != 2 || pair[0].variant != "A" || pair[1].variant != "B" ||
                    pair[0].zoom != pair[1].zoom ||
                    pair[0].focusX != pair[1].focusX ||
                    pair[0].focusY != pair[1].focusY ||
                    pair[0].seconds != pair[1].seconds ||
                    Mathf.Abs(pair[0].actualCameraX - pair[1].actualCameraX) > 0.00001f ||
                    Mathf.Abs(pair[0].actualCameraY - pair[1].actualCameraY) > 0.00001f ||
                    Mathf.Abs(pair[0].actualOrthographicSize - pair[1].actualOrthographicSize) > 0.00001f)
                    throw new InvalidOperationException("Camera A/B mismatch at " + group.Key);
            }
        }

        private static void CreateWorkerArt()
        {
            Directory.CreateDirectory(Physical(Generated));
            EnsureArt("Worker.png", 0);
            EnsureArt("ToolAxe.png", 1);
            EnsureArt("ToolSack.png", 2);
        }

        private static void EnsureArt(string name, int which)
        {
            string assetPath = Generated + name;
            string file = Physical(assetPath);
            if (!File.Exists(file))
            {
                const int w = 96, h = 144;
                var canvas = new Texture2D(w, h, TextureFormat.RGBA32, false);
                try
                {
                    var px = new Color[w * h];
                    for (int i = 0; i < px.Length; i++) px[i] = Color.clear;
                    if (which == 0)
                    {
                        // Miniature adult medieval worker: boots, apron, leather, tunic,
                        // hood and sun-facing highlights, not an unrelated stock photo.
                        Stamp(px, w, h, 34, 20, 15, 9, Hex("302a2b"));
                        Stamp(px, w, h, 62, 20, 15, 9, Hex("302a2b"));
                        Stamp(px, w, h, 38, 43, 13, 28, Hex("343e43"));
                        Stamp(px, w, h, 60, 43, 13, 28, Hex("353d3f"));
                        Stamp(px, w, h, 48, 79, 32, 40, Hex("423d3a"));
                        Stamp(px, w, h, 43, 85, 24, 33, Hex("6a4933"));
                        Stamp(px, w, h, 45, 67, 19, 21, Hex("94734c"));
                        Stamp(px, w, h, 48, 56, 30, 6, Hex("2b292b"));
                        Stamp(px, w, h, 48, 118, 25, 24, Hex("343d43"));
                        Stamp(px, w, h, 47, 115, 16, 18, Hex("bc916d"));
                        Stamp(px, w, h, 43, 126, 25, 9, Hex("435060"));
                        Stamp(px, w, h, 31, 93, 6, 27, Hex("71523a"));
                        Stamp(px, w, h, 65, 93, 6, 27, Hex("71523a"));
                    }
                    else if (which == 1)
                    {
                        Stamp(px, w, h, 43, 56, 6, 48, Hex("3b2923"));
                        Stamp(px, w, h, 41, 58, 3, 46, Hex("a27a4d"));
                        Stamp(px, w, h, 49, 104, 24, 18, Hex("3f4a53"));
                        Stamp(px, w, h, 58, 111, 13, 9, Hex("8295a2"));
                    }
                    else
                    {
                        Stamp(px, w, h, 48, 77, 30, 34, Hex("534337"));
                        Stamp(px, w, h, 45, 82, 23, 27, Hex("8d7659"));
                        Stamp(px, w, h, 48, 104, 12, 7, Hex("473629"));
                    }
                    for (int y = 0; y < h; ++y)
                        for (int x = 0; x < w; ++x)
                            canvas.SetPixel(x, y, px[y * w + x]);
                    canvas.Apply();
                    File.WriteAllBytes(file, canvas.EncodeToPNG());
                }
                finally { UnityEngine.Object.DestroyImmediate(canvas); }
            }
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            var ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (ti == null) throw new InvalidOperationException("PNG import missing");
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 160f;
            ti.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            ti.alphaIsTransparency = true;
            ti.filterMode = FilterMode.Bilinear;
            ti.mipmapEnabled = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
            if (AssetDatabase.LoadAssetAtPath<Sprite>(assetPath) == null)
                throw new InvalidOperationException("Generated activity sprite not imported");
        }

        private static Color Hex(string code)
        {
            if (!ColorUtility.TryParseHtmlString("#" + code, out var color))
                throw new InvalidOperationException("Bad authored palette");
            return color;
        }

        private static void Stamp(Color[] pixels, int width, int height,
            float centerX, float centerY, float radiusX, float radiusY, Color shade)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(centerX - radiusX - 2));
            int maxX = Mathf.Min(width - 1, Mathf.CeilToInt(centerX + radiusX + 2));
            int minY = Mathf.Max(0, Mathf.FloorToInt(centerY - radiusY - 2));
            int maxY = Mathf.Min(height - 1, Mathf.CeilToInt(centerY + radiusY + 2));
            for (int y = minY; y <= maxY; ++y)
                for (int x = minX; x <= maxX; ++x)
                {
                    float dx = (x - centerX) / radiusX;
                    float dy = (y - centerY) / radiusY;
                    float radial = Mathf.Sqrt(dx * dx + dy * dy);
                    float coverage = Mathf.Clamp01((1f - radial) * 5.5f);
                    int i = y * width + x;
                    var previous = pixels[i];
                    float alpha = Mathf.Clamp01(previous.a + (1f - previous.a) * coverage);
                    if (alpha <= 0f) continue;
                    float weight = coverage / alpha;
                    pixels[i] = new Color(
                        Mathf.Lerp(previous.r, shade.r, weight),
                        Mathf.Lerp(previous.g, shade.g, weight),
                        Mathf.Lerp(previous.b, shade.b, weight),
                        alpha);
                }
        }
    }
}
#endif
