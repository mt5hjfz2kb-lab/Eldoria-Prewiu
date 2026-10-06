using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        const int MaxProofSplats = 300000;
        const int FloatsPerVertex = 14;
        const int VertexStride = FloatsPerVertex * 4;

        struct PrepStats
        {
            public int OriginalCount;
            public int OutputCount;
            public int SanitizedCount;
            public int Step;
        }

        public static void Capture()
        {
            var output = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "ValoriaSharpGaussianUnityProofV1Captures"));
            if (Directory.Exists(output)) Directory.Delete(output, true);
            Directory.CreateDirectory(output);

            var sourcePly = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "SharpGaussianSource", "sharp-1.ply"));
            if (!File.Exists(sourcePly)) throw new FileNotFoundException("Missing SHARP PLY", sourcePly);

            // Keep downloaded canonical bytes untouched. Convergence proof operates on a bounded working copy.
            var workingPly = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "SharpGaussianSource", "sharp-proof-sanitized-300k.ply"));
            var prep = PrepareProofPly(sourcePly, workingPly, MaxProofSplats);
            Debug.Log($"[SHARP] Proof PLY prepared: source={prep.OriginalCount}, output={prep.OutputCount}, sanitized={prep.SanitizedCount}, step={prep.Step}");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SetupUrpFeature();

            Debug.Log("[SHARP] Loading bounded Gaussian asset");
            var asset = GsplatRuntimeLoader.LoadFile(
                workingPly,
                CompressionMode.Spark,
                SourceCoordinates.RUB,
                (stage, progress) => Debug.Log($"[SHARP] {stage} {progress:P0}")
            );
            if (asset == null || asset.SplatCount < 100000)
                throw new Exception("SHARP Gaussian asset did not load correctly");
            Debug.Log($"[SHARP] Asset loaded: splats={asset.SplatCount}, bounds={asset.Bounds}");

            Debug.Log("[SHARP] Creating renderer");
            var root = new GameObject("SHARP_Valoria_Gaussian");
            var gs = root.AddComponent<GsplatRenderer>();
            Debug.Log("[SHARP] Assigning asset to renderer");
            gs.GsplatAsset = asset;
            gs.SHDegree = 0;
            gs.GammaToLinear = true;
            gs.AsyncUpload = false;
            gs.RenderBeforeUploadComplete = false;
            Debug.Log("[SHARP] Renderer configured");

            // SHARP is OpenCV x-right/y-down/z-forward. UnitySplats converts RUB input to Unity RUF.
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            var substrate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            substrate.name = "Functional3D_Substrate_Collider";
            substrate.transform.position = new Vector3(0, -4f, 35f);
            substrate.transform.localScale = new Vector3(45f, .15f, 55f);
            substrate.GetComponent<Renderer>().enabled = false;

            var frontProbe = CreateProbe("OcclusionFront", new Color(.9f,.15f,.1f), new Vector3(-3.8f, 0.5f, 5f), new Vector3(1.1f,2.2f,1.1f));
            var backProbe  = CreateProbe("OcclusionBack", new Color(.15f,.9f,.25f), new Vector3(3.8f, 0.5f, 120f), new Vector3(2f,4f,2f));

            var camGo = new GameObject("ProofCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.02f,.025f,.035f,1f);
            cam.nearClipPlane = .01f;
            cam.farClipPlane = 500f;
            cam.fieldOfView = 43.58f;
            cam.transform.position = Vector3.zero;
            cam.transform.rotation = Quaternion.identity;

            Debug.Log("[SHARP] ForceRefresh begin");
            gs.ForceRefresh();
            Debug.Log("[SHARP] ForceRefresh complete");

            Debug.Log("[SHARP] Warmup render 1");
            cam.Render();
            Debug.Log("[SHARP] Warmup render 2");
            cam.Render();
            Debug.Log("[SHARP] Warmup complete");

            CaptureView(cam, output, "home", Vector3.zero, 43.58f);
            CaptureView(cam, output, "pan-left", new Vector3(-1.75f,0,0), 43.58f);
            CaptureView(cam, output, "pan-right", new Vector3(1.75f,0,0), 43.58f);
            CaptureView(cam, output, "zoom-in", Vector3.zero, 36f);
            CaptureView(cam, output, "zoom-out", Vector3.zero, 52f);

            frontProbe.SetActive(true); backProbe.SetActive(false);
            CaptureView(cam, output, "occlusion-front", Vector3.zero, 43.58f);

            frontProbe.SetActive(false); backProbe.SetActive(true);
            CaptureView(cam, output, "occlusion-behind", Vector3.zero, 43.58f);

            File.WriteAllText(Path.Combine(output, "evidence.json"),
                "{\n"+
                $"  \"source_splat_count\": {prep.OriginalCount},\n"+
                $"  \"proof_splat_count\": {asset.SplatCount},\n"+
                $"  \"sanitized_source_vertices\": {prep.SanitizedCount},\n"+
                $"  \"sampling_step\": {prep.Step},\n"+
                $"  \"bounds_center\": \"{asset.Bounds.center}\",\n"+
                $"  \"bounds_size\": \"{asset.Bounds.size}\",\n"+
                "  \"source\": \"SHARP full canonical Valoria PLY artifact 11404383856\",\n"+
                "  \"renderer\": \"UnitySplats 1.2.0 runtime PLY / URP\",\n"+
                "  \"proof_mode\": \"bounded_300k_uniform_sample\",\n"+
                "  \"functional_substrate_collider\": true,\n"+
                "  \"views\": [\"home\",\"pan-left\",\"pan-right\",\"zoom-in\",\"zoom-out\",\"occlusion-front\",\"occlusion-behind\"],\n"+
                "  \"paid_credits\": 0\n"+
                "}\n");

            gs.GsplatAsset = null;
            UnityEngine.Object.DestroyImmediate(asset);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Debug.Log("[SHARP] Unity convergence proof complete: " + output);
        }

        static PrepStats PrepareProofPly(string sourcePath, string outputPath, int maxSplats)
        {
            var data = File.ReadAllBytes(sourcePath);
            var needle = Encoding.ASCII.GetBytes("end_header\n");
            int end = -1;
            for (int i = 0; i <= data.Length - needle.Length; i++)
            {
                bool match = true;
                for (int k = 0; k < needle.Length; k++)
                    if (data[i + k] != needle[k]) { match = false; break; }
                if (match) { end = i + needle.Length; break; }
            }
            if (end < 0) throw new InvalidDataException("PLY end_header not found");

            var header = Encoding.ASCII.GetString(data, 0, end);
            var m = Regex.Match(header, @"element vertex (\d+)");
            if (!m.Success) throw new InvalidDataException("PLY vertex count not found");
            int count = int.Parse(m.Groups[1].Value);

            int vertexPayloadEnd = end + count * VertexStride;
            if (vertexPayloadEnd > data.Length) throw new EndOfStreamException("PLY vertex payload truncated");

            int step = Math.Max(1, (int)Math.Ceiling(count / (double)maxSplats));
            int outputCount = (count + step - 1) / step;
            var rewrittenHeader = new Regex(@"element vertex \d+").Replace(header, "element vertex " + outputCount, 1);
            var headerBytes = Encoding.ASCII.GetBytes(rewrittenHeader);

            int sanitized = 0;
            using (var ms = new MemoryStream(headerBytes.Length + outputCount * VertexStride + (data.Length - vertexPayloadEnd)))
            {
                ms.Write(headerBytes, 0, headerBytes.Length);
                var vertex = new byte[VertexStride];
                var vals = new float[FloatsPerVertex];

                for (int src = 0; src < count; src += step)
                {
                    int off = end + src * VertexStride;
                    Buffer.BlockCopy(data, off, vertex, 0, VertexStride);

                    bool invalid = false;
                    for (int k = 0; k < FloatsPerVertex; k++)
                    {
                        vals[k] = BitConverter.ToSingle(vertex, k * 4);
                        if (float.IsNaN(vals[k]) || float.IsInfinity(vals[k]))
                        {
                            vals[k] = 0f;
                            invalid = true;
                        }
                    }

                    if (invalid)
                    {
                        sanitized++;
                        // Make a corrupted source splat effectively invisible rather than inventing geometry.
                        vals[6] = -20f;
                        for (int k = 0; k < FloatsPerVertex; k++)
                        {
                            var bytes = BitConverter.GetBytes(vals[k]);
                            Buffer.BlockCopy(bytes, 0, vertex, k * 4, 4);
                        }
                    }

                    ms.Write(vertex, 0, vertex.Length);
                }

                ms.Write(data, vertexPayloadEnd, data.Length - vertexPayloadEnd);
                File.WriteAllBytes(outputPath, ms.ToArray());
            }

            return new PrepStats {
                OriginalCount = count,
                OutputCount = outputCount,
                SanitizedCount = sanitized,
                Step = step
            };
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

            Debug.Log("[SHARP] Capture " + name + " render 1");
            cam.Render();
            Debug.Log("[SHARP] Capture " + name + " render 2");
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
            Debug.Log("[SHARP] Capture complete: " + name);
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
