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
    public static class ValoriaSharpRuntimeBudgetGateV1
    {
        const int W = 1280;
        const int H = 853;
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
            var output = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "ValoriaSharpRuntimeBudgetGateV1Captures"));
            if (Directory.Exists(output)) Directory.Delete(output, true);
            Directory.CreateDirectory(output);

            var sourcePly = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "SharpGaussianSource", "sharp-1.ply"));
            if (!File.Exists(sourcePly)) throw new FileNotFoundException("Missing SHARP PLY", sourcePly);

            var densities = new[] { 300000, 450000, 600000 };
            var records = new System.Collections.Generic.List<string>();
            bool allStable = true;

            foreach (var density in densities)
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                SetupUrpFeature();

                string tag = (density / 1000).ToString() + "k";
                var workingPly = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "SharpGaussianSource", "sharp-runtime-" + tag + ".ply"));

                var prepWatch = System.Diagnostics.Stopwatch.StartNew();
                var prep = PrepareProofPly(sourcePly, workingPly, density);
                prepWatch.Stop();

                long allocBefore = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
                long reserveBefore = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();

                var loadWatch = System.Diagnostics.Stopwatch.StartNew();
                var asset = GsplatRuntimeLoader.LoadFile(
                    workingPly,
                    CompressionMode.Spark,
                    SourceCoordinates.RDF,
                    (stage, progress) => Debug.Log($"[GATE4:{tag}] {stage} {progress:P0}")
                );
                loadWatch.Stop();

                if (asset == null || asset.SplatCount < 100000)
                    throw new Exception($"Gate4 {tag} asset did not load correctly");

                var root = new GameObject("SHARP_Runtime_" + tag);
                var gs = root.AddComponent<GsplatRenderer>();
                gs.GsplatAsset = asset;
                gs.SHDegree = 0;
                gs.GammaToLinear = true;
                gs.AsyncUpload = false;
                gs.RenderBeforeUploadComplete = false;
                gs.Update();

                var camGo = new GameObject("RuntimeBudgetCamera_" + tag);
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(.02f,.025f,.035f,1f);
                cam.nearClipPlane = .01f;
                cam.farClipPlane = 1000f;
                cam.fieldOfView = 43.58f;
                cam.aspect = W / (float)H;
                cam.transform.position = Vector3.zero;
                cam.transform.rotation = Quaternion.identity;

                var initWatch = System.Diagnostics.Stopwatch.StartNew();
                PrepareSplatFrame(gs, cam, "gate4-" + tag + "-warmup");
                cam.Render();
                initWatch.Stop();

                long allocAfterWarmup = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
                long reserveAfterWarmup = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();

                // One authoritative beauty capture for visual sanity at each density.
                CaptureView(gs, cam, output, "home-" + tag, Vector3.zero, Quaternion.identity, 43.58f);

                const int samples = 20;
                var ms = new double[samples];
                for (int i = 0; i < samples; i++)
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    gs.Update();
                    GsplatSorter.Instance.GatherGsplatsForCamera(cam);
                    GsplatSorter.Instance.Update();
                    cam.Render();
                    sw.Stop();
                    ms[i] = sw.Elapsed.TotalMilliseconds;
                }

                Array.Sort(ms);
                double sum = 0d;
                for (int i=0;i<samples;i++) sum += ms[i];
                double avg = sum / samples;
                double p95 = ms[Math.Min(samples-1, (int)Math.Ceiling(samples * .95) - 1)];
                double max = ms[samples-1];

                bool stable = gs.Valid &&
                              gs.SplatCount == asset.SplatCount &&
                              gs.RemainingCount > 0 &&
                              double.IsFinite(avg) &&
                              double.IsFinite(p95);
                allStable &= stable;

                double allocDeltaMb = (allocAfterWarmup - allocBefore) / (1024d * 1024d);
                double reserveDeltaMb = (reserveAfterWarmup - reserveBefore) / (1024d * 1024d);

                records.Add(
                    "    {" +
                    $"\"density_target\":{density}," +
                    $"\"source_count\":{prep.OriginalCount}," +
                    $"\"proof_splat_count\":{asset.SplatCount}," +
                    $"\"sampling_step\":{prep.Step}," +
                    $"\"sanitized_source_vertices\":{prep.SanitizedCount}," +
                    $"\"prep_ms\":{prepWatch.Elapsed.TotalMilliseconds.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}," +
                    $"\"load_ms\":{loadWatch.Elapsed.TotalMilliseconds.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}," +
                    $"\"warmup_ms\":{initWatch.Elapsed.TotalMilliseconds.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}," +
                    $"\"render_sort_avg_ms\":{avg.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)}," +
                    $"\"render_sort_p95_ms\":{p95.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)}," +
                    $"\"render_sort_max_ms\":{max.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)}," +
                    $"\"allocated_memory_delta_mb\":{allocDeltaMb.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}," +
                    $"\"reserved_memory_delta_mb\":{reserveDeltaMb.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}," +
                    $"\"stable\":{(stable ? "true" : "false")}," +
                    $"\"beauty\":\"home-{tag}.png\"" +
                    "}"
                );

                Debug.Log($"[GATE4:{tag}] stable={stable}, splats={asset.SplatCount}, load={loadWatch.Elapsed.TotalMilliseconds:F1}ms, avg={avg:F2}ms, p95={p95:F2}ms, allocDelta={allocDeltaMb:F1}MB, reserveDelta={reserveDeltaMb:F1}MB");

                gs.GsplatAsset = null;
                UnityEngine.Object.DestroyImmediate(asset);
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                GC.Collect();
            }

            File.WriteAllText(Path.Combine(output, "runtime-budget-evidence.json"),
                "{\n" +
                "  \"source_artifact\": 11408853949,\n" +
                "  \"visual_authority_artifact\": 11409079879,\n" +
                "  \"gate3_artifact\": 11412317590,\n" +
                "  \"benchmark_scope\": \"relative_windows_unity_batchmode_not_mobile_certification\",\n" +
                "  \"densities\": [\n" + string.Join(",\n", records) + "\n  ],\n" +
                $"  \"all_stable\": {(allStable ? "true" : "false")},\n" +
                "  \"paid_credits\": 0\n" +
                "}\n");

            if (!allStable)
                throw new Exception("Gate 4 runtime density benchmark had an unstable density.");

            Debug.Log("[GATE4] Relative runtime density benchmark complete: " + output);
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

        static void PrepareSplatFrame(GsplatRenderer gs, Camera cam, string label)
        {
            // Batchmode Camera.Render() does not advance UnitySplats' normal PlayerLoop hook.
            // Reproduce the required D3D11 CPU-fallback lifecycle explicitly.
            gs.Update();
            gs.ForceRefresh();
            if (gs.SorterResource != null)
                gs.SorterResource.Initialized = false;

            var deadline = DateTime.UtcNow.AddSeconds(20);
            int loops = 0;
            while (DateTime.UtcNow < deadline)
            {
                loops++;
                gs.Update();
                GsplatSorter.Instance.GatherGsplatsForCamera(cam);
                GsplatSorter.Instance.Update();

                bool ready = gs.Valid &&
                             gs.SplatCount == gs.GsplatAsset.SplatCount &&
                             gs.RemainingCount > 0 &&
                             gs.SorterResource != null &&
                             gs.SorterResource.Initialized;
                if (ready)
                {
                    Debug.Log($"[SHARP] Frame ready {label}: resident={gs.SplatCount}, remaining={gs.RemainingCount}, loops={loops}, cpuFallback={GsplatSorter.Instance.CpuFallbackEnabled}");
                    return;
                }

                System.Threading.Thread.Sleep(25);
            }

            throw new TimeoutException(
                $"SHARP sorter did not initialize for {label}. resident={gs.SplatCount}, remaining={gs.RemainingCount}, valid={gs.Valid}, sorter={(gs.SorterResource != null)}, initialized={(gs.SorterResource != null && gs.SorterResource.Initialized)}");
        }

        static void CaptureView(GsplatRenderer gs, Camera cam, string output, string name, Vector3 position, Quaternion rotation, float fov)
        {
            cam.transform.position = position;
            cam.transform.rotation = rotation;
            cam.fieldOfView = fov;

            var rt = new RenderTexture(W,H,24,RenderTextureFormat.ARGB32);
            rt.Create();
            cam.targetTexture = rt;

            PrepareSplatFrame(gs, cam, name);
            Debug.Log("[SHARP] Capture " + name + " render");
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
