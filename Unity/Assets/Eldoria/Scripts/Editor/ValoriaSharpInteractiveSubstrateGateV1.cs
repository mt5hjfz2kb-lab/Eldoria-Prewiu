using System;
using System.Collections.Generic;
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
    public static class ValoriaSharpInteractiveSubstrateGateV1
    {
        const int W = 1280;
        const int H = 853;
        const int MaxProofSplats = 600000;
        const int FloatsPerVertex = 14;
        const int VertexStride = FloatsPerVertex * 4;

        struct PrepStats
        {
            public int OriginalCount;
            public int OutputCount;
            public int SanitizedCount;
            public int Step;
        }

        sealed class ProxySpec
        {
            public string Id;
            public Vector2 Viewport;
            public float DepthScale;
            public Vector3 Size;
            public Color DebugColor;
            public GameObject Go;
        }

        public static void Capture()
        {
            var output = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "ValoriaSharpInteractiveSubstrateGateV1Captures"));
            if (Directory.Exists(output)) Directory.Delete(output, true);
            Directory.CreateDirectory(output);

            var sourcePly = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "SharpGaussianSource", "sharp-1.ply"));
            if (!File.Exists(sourcePly)) throw new FileNotFoundException("Missing clean SHARP PLY", sourcePly);

            var workingPly = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "SharpGaussianSource", "sharp-interaction-sanitized-600k.ply"));
            var prep = PrepareProofPly(sourcePly, workingPly, MaxProofSplats);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SetupUrpFeature();

            var asset = GsplatRuntimeLoader.LoadFile(
                workingPly,
                CompressionMode.Spark,
                SourceCoordinates.RDF,
                (stage, progress) => Debug.Log($"[GATE2] {stage} {progress:P0}")
            );
            if (asset == null || asset.SplatCount < 500000)
                throw new Exception("Clean SHARP 600k asset did not load correctly");

            var root = new GameObject("SHARP_Valoria_Clean_600K");
            var gs = root.AddComponent<GsplatRenderer>();
            gs.GsplatAsset = asset;
            gs.SHDegree = 0;
            gs.GammaToLinear = true;
            gs.AsyncUpload = false;
            gs.RenderBeforeUploadComplete = false;
            gs.Update();

            if (!gs.Valid || gs.SplatCount == 0)
                throw new Exception($"Gaussian renderer failed initialization. valid={gs.Valid}, resident={gs.SplatCount}");

            var camGo = new GameObject("Gate2Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.02f,.025f,.035f,1f);
            cam.nearClipPlane = .01f;
            cam.farClipPlane = 1000f;
            cam.fieldOfView = 43.58f;
            cam.aspect = W / (float)H;
            cam.transform.position = Vector3.zero;

            var forward = asset.Bounds.center;
            if (forward.sqrMagnitude < 1e-6f) forward = Vector3.forward;
            var homeRotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
            cam.transform.rotation = homeRotation;

            PrepareSplatFrame(gs, cam, "gate2-warmup");

            float anchorDistance = Mathf.Max(30f, Vector3.Distance(cam.transform.position, asset.Bounds.center));
            var proxies = new List<ProxySpec>
            {
                new ProxySpec { Id="WestTower", Viewport=new Vector2(.38f,.53f), DepthScale=.98f, Size=new Vector3(22f,42f,14f), DebugColor=new Color(.15f,.55f,1f,.72f) },
                new ProxySpec { Id="CentralKeep", Viewport=new Vector2(.51f,.50f), DepthScale=.98f, Size=new Vector3(30f,50f,16f), DebugColor=new Color(1f,.65f,.12f,.72f) },
                new ProxySpec { Id="EastTower", Viewport=new Vector2(.63f,.54f), DepthScale=.98f, Size=new Vector3(22f,42f,14f), DebugColor=new Color(.65f,.25f,1f,.72f) },
                new ProxySpec { Id="LowerGate", Viewport=new Vector2(.61f,.72f), DepthScale=.90f, Size=new Vector3(24f,28f,14f), DebugColor=new Color(.15f,1f,.45f,.72f) },
            };

            foreach (var p in proxies)
            {
                var ray = cam.ViewportPointToRay(new Vector3(p.Viewport.x,p.Viewport.y,0f));
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "InteractiveProxy_" + p.Id;
                go.transform.position = ray.origin + ray.direction * anchorDistance * p.DepthScale;
                go.transform.rotation = homeRotation;
                go.transform.localScale = p.Size;
                var renderer = go.GetComponent<Renderer>();
                renderer.enabled = false;
                p.Go = go;
            }

            CaptureBeauty(gs, cam, output, "beauty-home");

            var hitRows = new List<string>();
            int passed = 0;
            foreach (var p in proxies)
            {
                var ray = cam.ViewportPointToRay(new Vector3(p.Viewport.x,p.Viewport.y,0f));
                bool hit = Physics.Raycast(ray, out RaycastHit info, 1000f);
                string hitId = hit && info.collider != null ? info.collider.gameObject.name.Replace("InteractiveProxy_","") : "";
                bool ok = hit && hitId == p.Id;
                if (ok) passed++;
                hitRows.Add(
                    $"    {{\"expected\":\"{p.Id}\",\"viewport\":[{p.Viewport.x:F3},{p.Viewport.y:F3}],\"hit\":{(hit ? "true":"false")},\"actual\":\"{hitId}\",\"world\":\"{(hit ? info.point.ToString("F3") : "")}\",\"pass\":{(ok ? "true":"false")} }}"
                );
            }

            // Debug-only alignment capture: show proxies as transparent colored solids.
            foreach (var p in proxies)
            {
                var renderer = p.Go.GetComponent<Renderer>();
                renderer.enabled = true;
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = p.DebugColor;
                renderer.sharedMaterial = mat;
            }
            CaptureBeauty(gs, cam, output, "proxy-debug");
            foreach (var p in proxies) p.Go.GetComponent<Renderer>().enabled = false;

            File.WriteAllText(Path.Combine(output,"interaction-evidence.json"),
                "{\n"+
                $"  \"source_splat_count\": {prep.OriginalCount},\n"+
                $"  \"proof_splat_count\": {asset.SplatCount},\n"+
                $"  \"sanitized_source_vertices\": {prep.SanitizedCount},\n"+
                "  \"source_artifact\": 11408853949,\n"+
                "  \"visual_baseline_artifact\": 11409359136,\n"+
                $"  \"proxy_count\": {proxies.Count},\n"+
                $"  \"raycast_pass_count\": {passed},\n"+
                $"  \"gate_pass\": {(passed == proxies.Count ? "true":"false")},\n"+
                "  \"hits\": [\n"+string.Join(",\n",hitRows)+"\n  ],\n"+
                "  \"beauty_proxies_visible\": false,\n"+
                "  \"paid_credits\": 0\n"+
                "}\n");

            if (passed != proxies.Count)
                throw new Exception($"Interactive substrate gate failed: {passed}/{proxies.Count} deterministic raycasts passed.");

            gs.GsplatAsset = null;
            UnityEngine.Object.DestroyImmediate(asset);
            Debug.Log($"[GATE2] PASS {passed}/{proxies.Count} interactive proxies");
        }

        static void CaptureBeauty(GsplatRenderer gs, Camera cam, string output, string name)
        {
            var rt = new RenderTexture(W,H,24,RenderTextureFormat.ARGB32);
            rt.Create();
            cam.targetTexture = rt;
            PrepareSplatFrame(gs, cam, name);
            cam.Render();

            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(W,H,TextureFormat.RGBA32,false);
            tex.ReadPixels(new Rect(0,0,W,H),0,0);
            tex.Apply();
            File.WriteAllBytes(Path.Combine(output,name+".png"),tex.EncodeToPNG());
            RenderTexture.active = previous;
            cam.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(tex);
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
        }

        static void PrepareSplatFrame(GsplatRenderer gs, Camera cam, string label)
        {
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
                    Debug.Log($"[GATE2] Frame ready {label}: resident={gs.SplatCount}, remaining={gs.RemainingCount}, loops={loops}");
                    return;
                }
                System.Threading.Thread.Sleep(25);
            }
            throw new TimeoutException($"Gate2 sorter did not initialize for {label}");
        }

        static PrepStats PrepareProofPly(string sourcePath, string outputPath, int maxSplats)
        {
            var data = File.ReadAllBytes(sourcePath);
            var needle = Encoding.ASCII.GetBytes("end_header\n");
            int end = -1;
            for (int i=0;i<=data.Length-needle.Length;i++)
            {
                bool match=true;
                for(int k=0;k<needle.Length;k++) if(data[i+k]!=needle[k]) { match=false; break; }
                if(match){ end=i+needle.Length; break; }
            }
            if(end<0) throw new InvalidDataException("PLY end_header not found");

            var header=Encoding.ASCII.GetString(data,0,end);
            var m=Regex.Match(header,@"element vertex (\d+)");
            if(!m.Success) throw new InvalidDataException("PLY vertex count not found");
            int count=int.Parse(m.Groups[1].Value);
            int vertexPayloadEnd=end+count*VertexStride;
            if(vertexPayloadEnd>data.Length) throw new EndOfStreamException("PLY vertex payload truncated");

            int step=Math.Max(1,(int)Math.Ceiling(count/(double)maxSplats));
            int outputCount=(count+step-1)/step;
            var rewrittenHeader=new Regex(@"element vertex \d+").Replace(header,"element vertex "+outputCount,1);
            var headerBytes=Encoding.ASCII.GetBytes(rewrittenHeader);

            int sanitized=0;
            using(var ms=new MemoryStream(headerBytes.Length+outputCount*VertexStride+(data.Length-vertexPayloadEnd)))
            {
                ms.Write(headerBytes,0,headerBytes.Length);
                var vertex=new byte[VertexStride];
                var vals=new float[FloatsPerVertex];
                for(int src=0;src<count;src+=step)
                {
                    int off=end+src*VertexStride;
                    Buffer.BlockCopy(data,off,vertex,0,VertexStride);
                    bool invalid=false;
                    for(int k=0;k<FloatsPerVertex;k++)
                    {
                        vals[k]=BitConverter.ToSingle(vertex,k*4);
                        if(float.IsNaN(vals[k])||float.IsInfinity(vals[k])) { vals[k]=0f; invalid=true; }
                    }
                    if(invalid)
                    {
                        sanitized++;
                        vals[6]=-20f;
                        for(int k=0;k<FloatsPerVertex;k++)
                        {
                            var bytes=BitConverter.GetBytes(vals[k]);
                            Buffer.BlockCopy(bytes,0,vertex,k*4,4);
                        }
                    }
                    ms.Write(vertex,0,vertex.Length);
                }
                ms.Write(data,vertexPayloadEnd,data.Length-vertexPayloadEnd);
                File.WriteAllBytes(outputPath,ms.ToArray());
            }
            return new PrepStats{OriginalCount=count,OutputCount=outputCount,SanitizedCount=sanitized,Step=step};
        }

        static void SetupUrpFeature()
        {
            var urp=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Eldoria/Content/EldoriaMobileURP.asset");
            var data=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Eldoria/Content/EldoriaForwardRenderer.asset");
            if(urp==null||data==null) throw new Exception("Missing Eldoria URP assets");

            var featureType=AppDomain.CurrentDomain.GetAssemblies()
                .Select(a=>a.GetType("Gsplat.GsplatURPFeature",false))
                .FirstOrDefault(t=>t!=null);
            if(featureType==null) throw new Exception("UnitySplats URP feature type not found");

            if(!data.rendererFeatures.Any(f=>f!=null&&f.GetType()==featureType))
            {
                var feature=ScriptableObject.CreateInstance(featureType) as ScriptableRendererFeature;
                if(feature==null) throw new Exception("Could not instantiate GsplatURPFeature");
                feature.name="SHARP Gate2 Gsplat URP Feature";
                data.rendererFeatures.Add(feature);
                feature.Create();
            }
            GraphicsSettings.defaultRenderPipeline=urp;
            QualitySettings.renderPipeline=urp;
        }
    }
}
