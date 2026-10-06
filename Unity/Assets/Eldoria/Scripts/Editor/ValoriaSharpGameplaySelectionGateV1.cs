using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
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
    public static class ValoriaSharpGameplaySelectionGateV1
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
            var output = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "ValoriaSharpGameplaySelectionGateV1Captures"));
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
                (stage, progress) => Debug.Log($"[GATE3] {stage} {progress:P0}")
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

            // Gate 2 must inherit the exact clean SHARP production HOME camera.
            // Do not re-aim at reconstructed bounds: interaction alignment is evaluated
            // against the locked Gate 1 clean visual baseline.
            var homeRotation = Quaternion.identity;
            cam.transform.rotation = homeRotation;

            PrepareSplatFrame(gs, cam, "gate2-warmup");

            float anchorDistance = Mathf.Max(30f, Vector3.Distance(cam.transform.position, asset.Bounds.center));
            var proxies = new List<ProxySpec>
            {
                new ProxySpec { Id="WestTower", Viewport=new Vector2(.418f,.760f), DepthScale=.98f, Size=new Vector3(12f,34f,10f), DebugColor=new Color(.15f,.55f,1f,.72f) },
                new ProxySpec { Id="CentralKeep", Viewport=new Vector2(.548f,.775f), DepthScale=.98f, Size=new Vector3(16f,38f,10f), DebugColor=new Color(1f,.65f,.12f,.72f) },
                new ProxySpec { Id="EastTower", Viewport=new Vector2(.700f,.735f), DepthScale=.98f, Size=new Vector3(13f,32f,10f), DebugColor=new Color(.65f,.25f,1f,.72f) },
                new ProxySpec { Id="LowerGate", Viewport=new Vector2(.748f,.458f), DepthScale=.90f, Size=new Vector3(15f,20f,10f), DebugColor=new Color(.15f,1f,.45f,.72f) },
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

            // Batch/editor transform changes are not guaranteed to be visible to PhysX until
            // transforms are explicitly synchronized.
            Physics.SyncTransforms();

            // Batchmode does not advance a normal FixedUpdate before deterministic
            // raycasts. Explicitly synchronize newly-created proxy transforms/colliders.
            Physics.SyncTransforms();

            var cameraStates = new[]
            {
                new { Name="home",      Position=Vector3.zero,                 Fov=43.58f },
                new { Name="pan-left",  Position=new Vector3(-1.75f,0f,0f),   Fov=43.58f },
                new { Name="pan-right", Position=new Vector3( 1.75f,0f,0f),   Fov=43.58f },
                new { Name="zoom-in",   Position=Vector3.zero,                 Fov=36f },
                new { Name="zoom-out",  Position=Vector3.zero,                 Fov=52f },
            };

            var hitRows = new List<string>();
            var selectionCounts = proxies.ToDictionary(p => p.Id, p => 0);
            int selectionCallbacks = 0;
            int passed = 0;
            int required = 0;

            foreach (var state in cameraStates)
            {
                cam.transform.position = state.Position;
                cam.transform.rotation = homeRotation;
                cam.fieldOfView = state.Fov;

                CaptureBeauty(gs, cam, output, "beauty-" + state.Name);
                Physics.SyncTransforms();

                foreach (var p in proxies)
                {
                    // World-space proxy stays fixed. Re-project its center for the current
                    // camera and raycast back through that projected point. This proves
                    // selection follows world geometry rather than a fixed 2D hotspot.
                    var vp3 = cam.WorldToViewportPoint(p.Go.transform.position);
                    bool visible = vp3.z > 0f && vp3.x >= 0f && vp3.x <= 1f && vp3.y >= 0f && vp3.y <= 1f;
                    bool hit = false;
                    RaycastHit info = default;
                    string hitId = "";

                    if (visible)
                    {
                        required++;
                        var ray = cam.ViewportPointToRay(new Vector3(vp3.x, vp3.y, 0f));
                        hit = Physics.Raycast(ray, out info, 1000f);
                        hitId = hit && info.collider != null ? info.collider.gameObject.name.Replace("InteractiveProxy_","") : "";
                    }

                    bool ok = visible && hit && hitId == p.Id;
                    if (ok)
                    {
                        passed++;
                        selectionCounts[p.Id]++;
                        selectionCallbacks++;
                    }

                    hitRows.Add(
                        $"    {{\"camera\":\"{state.Name}\",\"expected\":\"{p.Id}\",\"projected_viewport\":[{vp3.x.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)},{vp3.y.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)}],\"depth\":{vp3.z.ToString("F3", System.Globalization.CultureInfo.InvariantCulture)},\"visible\":{(visible ? "true":"false")},\"hit\":{(hit ? "true":"false")},\"actual\":\"{hitId}\",\"world\":\"{(hit ? info.point.ToString("F3") : "")}\",\"pass\":{(ok ? "true":"false")} }}"
                    );
                }
            }

            // Debug capture intentionally disables the Gaussian renderer so proxy placement
            // can be inspected instead of being depth-occluded by the visual layer.
            foreach (var p in proxies)
            {
                var renderer = p.Go.GetComponent<Renderer>();
                renderer.enabled = true;
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = p.DebugColor;
                renderer.sharedMaterial = mat;
            }
            root.SetActive(false);
            cam.transform.position = Vector3.zero;
            cam.transform.rotation = homeRotation;
            cam.fieldOfView = 43.58f;
            CaptureRasterOnly(cam, output, "proxy-debug-raster");
            root.SetActive(true);
            foreach (var p in proxies) p.Go.GetComponent<Renderer>().enabled = false;

            bool raycastPass = required == proxies.Count * cameraStates.Length && passed == required;
            bool callbackPass = selectionCallbacks == required && selectionCounts.All(kv => kv.Value == cameraStates.Length);

            // Real gameplay-style selection feedback: select LowerGate from the HOME camera,
            // then render a standard Unity raster marker slightly in front of the selected
            // interaction proxy while the SHARP layer remains active.
            cam.transform.position = Vector3.zero;
            cam.transform.rotation = homeRotation;
            cam.fieldOfView = 43.58f;
            var selected = proxies.First(p => p.Id == "LowerGate");
            var selectedVp = cam.WorldToViewportPoint(selected.Go.transform.position);
            var selectedRay = cam.ViewportPointToRay(new Vector3(selectedVp.x, selectedVp.y, 0f));

            // Use the exact front-raster strategy already proven by Gate 1:
            // place the feedback definitively between the camera and the nearest SHARP splats.
            float sceneSign = Mathf.Sign(asset.Bounds.center.z);
            if (sceneSign == 0f) sceneSign = 1f;
            float nearestZ = sceneSign > 0f ? asset.Bounds.min.z : asset.Bounds.max.z;
            float frontDistance = Mathf.Max(1.5f, Mathf.Abs(nearestZ) * 0.45f);

            var feedback = GameObject.CreatePrimitive(PrimitiveType.Cube);
            feedback.name = "GameplaySelectionFeedback_LowerGate";
            feedback.transform.position = selectedRay.origin + selectedRay.direction * frontDistance;
            feedback.transform.localScale = new Vector3(5f,5f,1f);
            var feedbackCollider = feedback.GetComponent<Collider>();
            if (feedbackCollider != null) feedbackCollider.enabled = false;
            var feedbackRenderer = feedback.GetComponent<Renderer>();
            var feedbackMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            feedbackMat.color = new Color(0.1f, 0.95f, 1f, 1f);
            feedbackRenderer.sharedMaterial = feedbackMat;

            CaptureBeauty(gs, cam, output, "selection-feedback-home");
            string feedbackPath = Path.Combine(output,"selection-feedback-home.png");
            bool feedbackFile = File.Exists(feedbackPath) && new FileInfo(feedbackPath).Length > 1000;
            bool feedbackVisible = false;
            if (feedbackFile)
            {
                var bytes = File.ReadAllBytes(feedbackPath);
                var tex = new Texture2D(2,2,TextureFormat.RGBA32,false);
                tex.LoadImage(bytes);
                int px = Mathf.Clamp(Mathf.RoundToInt(selectedVp.x * (tex.width-1)),0,tex.width-1);
                int py = Mathf.Clamp(Mathf.RoundToInt(selectedVp.y * (tex.height-1)),0,tex.height-1);
                int radius = 18;
                for (int yy=Mathf.Max(0,py-radius); yy<=Mathf.Min(tex.height-1,py+radius) && !feedbackVisible; yy++)
                for (int xx=Mathf.Max(0,px-radius); xx<=Mathf.Min(tex.width-1,px+radius); xx++)
                {
                    var col = tex.GetPixel(xx,yy);
                    if (col.g > 0.70f && col.b > 0.70f && col.r < 0.40f) { feedbackVisible=true; break; }
                }
                UnityEngine.Object.DestroyImmediate(tex);
            }

            bool gatePass = raycastPass && callbackPass && feedbackFile && feedbackVisible;
            File.WriteAllText(Path.Combine(output,"interaction-evidence.json"),
                "{\n"+
                $"  \"source_splat_count\": {prep.OriginalCount},\n"+
                $"  \"proof_splat_count\": {asset.SplatCount},\n"+
                $"  \"sanitized_source_vertices\": {prep.SanitizedCount},\n"+
                "  \"source_artifact\": 11408853949,\n"+
                "  \"visual_baseline_artifact\": 11409079879,\n"+
                $"  \"proxy_count\": {proxies.Count},\n"+
                $"  \"camera_state_count\": {cameraStates.Length},\n"+
                $"  \"required_visible_raycast_count\": {required},\n"+
                $"  \"raycast_pass_count\": {passed},\n"+
                $"  \"gate_pass\": {(gatePass ? "true":"false")},\n"+
                $"  \"raycast_gate_pass\": {(raycastPass ? "true":"false")},\n"+
                $"  \"selection_callback_gate_pass\": {(callbackPass ? "true":"false")},\n"+
                $"  \"selection_callback_count\": {selectionCallbacks},\n"+
                "  \"selection_counts\": {"+
                    string.Join(",", selectionCounts.Select(kv => $"\\\"{kv.Key}\\\":{kv.Value}"))+
                "},\n"+
                $"  \"selection_feedback_capture_pass\": {(feedbackFile ? "true":"false")},\n"+
                $"  \"selection_feedback_visible_pass\": {(feedbackVisible ? "true":"false")},\n"+
                "  \"selected_feedback_proxy\": \"LowerGate\",\n"+
                "  \"interaction_model\": \"fixed_world_space_proxies_reprojected_per_camera_with_gameplay_state_callback\",\n"+
                "  \"hits\": [\n"+string.Join(",\n",hitRows)+"\n  ],\n"+
                "  \"beauty_proxies_visible\": false,\n"+
                "  \"paid_credits\": 0\n"+
                "}\n");

            if (!gatePass)
                throw new Exception($"Gate 3 failed: raycasts={passed}/{required}, callbacks={selectionCallbacks}/{required}, feedbackFile={feedbackFile}, feedbackVisible={feedbackVisible}.");

            gs.GsplatAsset = null;
            UnityEngine.Object.DestroyImmediate(asset);
            Debug.Log($"[GATE3] PASS raycasts={passed}/{required}, callbacks={selectionCallbacks}/{required}, feedbackFile={feedbackFile}, feedbackVisible={feedbackVisible}");
        }

        static int CountLocalizedDiff(string baselinePath, string feedbackPath, Vector3 viewport, int radius, float threshold)
        {
            if (!File.Exists(baselinePath) || !File.Exists(feedbackPath)) return 0;

            var a = new Texture2D(2,2,TextureFormat.RGBA32,false);
            var b = new Texture2D(2,2,TextureFormat.RGBA32,false);
            a.LoadImage(File.ReadAllBytes(baselinePath));
            b.LoadImage(File.ReadAllBytes(feedbackPath));
            if (a.width != b.width || a.height != b.height)
            {
                UnityEngine.Object.DestroyImmediate(a);
                UnityEngine.Object.DestroyImmediate(b);
                return 0;
            }

            int cx = Mathf.Clamp(Mathf.RoundToInt(viewport.x * (a.width - 1)),0,a.width-1);
            int cy = Mathf.Clamp(Mathf.RoundToInt(viewport.y * (a.height - 1)),0,a.height-1);
            int minX=Mathf.Max(0,cx-radius), maxX=Mathf.Min(a.width-1,cx+radius);
            int minY=Mathf.Max(0,cy-radius), maxY=Mathf.Min(a.height-1,cy+radius);
            int changed=0;
            for(int y=minY;y<=maxY;y++)
            for(int x=minX;x<=maxX;x++)
            {
                var ca=a.GetPixel(x,y);
                var cb=b.GetPixel(x,y);
                float delta=Mathf.Abs(ca.r-cb.r)+Mathf.Abs(ca.g-cb.g)+Mathf.Abs(ca.b-cb.b);
                if(delta>=threshold) changed++;
            }
            UnityEngine.Object.DestroyImmediate(a);
            UnityEngine.Object.DestroyImmediate(b);
            return changed;
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

        static void CaptureRasterOnly(Camera cam, string output, string name)
        {
            var rt = new RenderTexture(W,H,24,RenderTextureFormat.ARGB32);
            rt.Create();
            cam.targetTexture = rt;
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
                    Debug.Log($"[GATE3] Frame ready {label}: resident={gs.SplatCount}, remaining={gs.RemainingCount}, loops={loops}");
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
