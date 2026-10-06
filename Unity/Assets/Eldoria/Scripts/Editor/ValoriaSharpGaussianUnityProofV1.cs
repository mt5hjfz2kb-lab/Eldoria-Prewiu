using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Gsplat;

namespace Eldoria.EditorTools
{
    public static class ValoriaSharpGaussianUnityProofV1
    {
        const int W = 1280;
        const int H = 853;

        public static void Capture()
        {
            var output = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "ValoriaSharpGaussianUnityProofV1Captures"));
            if (Directory.Exists(output)) Directory.Delete(output, true);
            Directory.CreateDirectory(output);

            var ply = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "SharpGaussianSource", "sharp-1.ply"));
            if (!File.Exists(ply)) throw new FileNotFoundException("Missing SHARP PLY", ply);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SetupUrpFeature();

            var asset = GsplatRuntimeLoader.LoadFile(
                ply,
                CompressionMode.Spark,
                SourceCoordinates.RUB,
                (stage, progress) => Debug.Log($"[SHARP] {stage} {progress:P0}")
            );
            if (asset == null || asset.SplatCount < 100000)
                throw new Exception("SHARP Gaussian asset did not load correctly");

            var root = new GameObject("SHARP_Valoria_Gaussian");
            var gs = root.AddComponent<GsplatRenderer>();
            gs.GsplatAsset = asset;
            gs.SHDegree = 0;
            gs.GammaToLinear = true;
            gs.AsyncUpload = false;
            gs.RenderBeforeUploadComplete = true;
            gs.SplatDownscaleFactor = 0f;

            // SHARP is OpenCV x-right/y-down/z-forward. UnitySplats converts RUB input to Unity RUF.
            // The prediction camera is identity, so HOME remains at origin looking +Z.
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            // Functional 3D substrate / interaction proxy.
            var substrate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            substrate.name = "Functional3D_Substrate_Collider";
            substrate.transform.position = new Vector3(0, -4f, 35f);
            substrate.transform.localScale = new Vector3(45f, .15f, 55f);
            var subRenderer = substrate.GetComponent<Renderer>();
            subRenderer.enabled = false; // collider/logical substrate remains, visual comes from SHARP.

            // Explicit visible 3D occlusion probes.
            var frontProbe = CreateProbe("OcclusionFront", new Color(.9f,.15f,.1f), new Vector3(-3.8f, 0.5f, 5f), new Vector3(1.1f,2.2f,1.1f));
            var backProbe  = CreateProbe("OcclusionBack", new Color(.15f,.9f,.25f), new Vector3(3.8f, 0.5f, 120f), new Vector3(2f,4f,2f));

            var camGo = new GameObject("ProofCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.02f,.025f,.035f,1f);
            cam.nearClipPlane = .01f;
            cam.farClipPlane = 500f;
            cam.fieldOfView = 43.58f; // derived from SHARP intrinsics fy=1066.538, h=853.
            cam.transform.position = Vector3.zero;
            cam.transform.rotation = Quaternion.identity;

            // Allow package registration/upload to settle deterministically in editor.
            gs.ForceRefresh();
            for (int i=0;i<4;i++) cam.Render();

            CaptureView(cam, output, "home", Vector3.zero, 43.58f);
            CaptureView(cam, output, "pan-left", new Vector3(-1.75f,0,0), 43.58f);
            CaptureView(cam, output, "pan-right", new Vector3(1.75f,0,0), 43.58f);
            CaptureView(cam, output, "zoom-in", Vector3.zero, 36f);
            CaptureView(cam, output, "zoom-out", Vector3.zero, 52f);

            // Occlusion A: front red probe must visibly cover splats.
            frontProbe.SetActive(true); backProbe.SetActive(false);
            CaptureView(cam, output, "occlusion-front", Vector3.zero, 43.58f);

            // Occlusion B: green probe is behind the scene and should be largely/fully hidden by nearer splats.
            frontProbe.SetActive(false); backProbe.SetActive(true);
            CaptureView(cam, output, "occlusion-behind", Vector3.zero, 43.58f);

            File.WriteAllText(Path.Combine(output, "evidence.json"),
                "{\n"+
                $"  \"splat_count\": {asset.SplatCount},\n"+
                $"  \"bounds_center\": \"{asset.Bounds.center}\",\n"+
                $"  \"bounds_size\": \"{asset.Bounds.size}\",\n"+
                "  \"source\": \"SHARP full canonical Valoria PLY artifact 11404383856\",\n"+
                "  \"renderer\": \"UnitySplats 1.2.0 runtime PLY / URP\",\n"+
                "  \"functional_substrate_collider\": true,\n"+
                "  \"views\": [\"home\",\"pan-left\",\"pan-right\",\"zoom-in\",\"zoom-out\",\"occlusion-front\",\"occlusion-behind\"],\n"+
                "  \"paid_credits\": 0\n"+
                "}\n");

            UnityEngine.Object.DestroyImmediate(asset);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Debug.Log("[SHARP] Unity proof complete: " + output);
        }

        static GameObject CreateProbe(string name, Color color, Vector3 pos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            go.SetActive(false);
            return go;
        }

        static void CaptureView(Camera cam, string output, string name, Vector3 position, float fov)
        {
            cam.transform.position = position;
            cam.transform.rotation = Quaternion.identity;
            cam.fieldOfView = fov;

            var rt = new RenderTexture(W,H,24,RenderTextureFormat.ARGB32);
            rt.Create();
            cam.targetTexture = rt;

            // Multiple renders let CPU/GPU sort paths stabilize after camera change.
            cam.Render();
            cam.Render();

            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(W,H,TextureFormat.RGBA32,false);
            tex.ReadPixels(new Rect(0,0,W,H),0,0);
            tex.Apply();
            File.WriteAllBytes(Path.Combine(output,name+".png"), tex.EncodeToPNG());
            RenderTexture.active = prev;
            cam.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(tex);
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
        }

        static void SetupUrpFeature()
        {
            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Eldoria/Content/EldoriaMobileURP.asset");
            var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Eldoria/Content/EldoriaForwardRenderer.asset");
            if (urp == null || data == null) throw new Exception("Missing Eldoria URP assets");

            var featureType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Gsplat.GsplatURPFeature", false))
                .FirstOrDefault(t => t != null);
            if (featureType == null) throw new Exception("UnitySplats URP feature type not found");

            if (!data.rendererFeatures.Any(f => f != null && f.GetType() == featureType))
            {
                var feature = ScriptableObject.CreateInstance(featureType) as ScriptableRendererFeature;
                if (feature == null) throw new Exception("Could not instantiate GsplatURPFeature");
                feature.name = "SHARP Proof Gsplat URP Feature";
                data.rendererFeatures.Add(feature);
                feature.Create();
            }

            GraphicsSettings.defaultRenderPipeline = urp;
            QualitySettings.renderPipeline = urp;
        }
    }
}
