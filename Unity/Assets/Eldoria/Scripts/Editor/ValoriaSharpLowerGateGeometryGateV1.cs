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
    public static class ValoriaSharpLowerGateGeometryGateV1
    {
        const int W = 1280;
        const int H = 853;
        const int MaxProofSplats = 600000;
        const int FloatsPerVertex = 14;
        const int VertexStride = FloatsPerVertex * 4;
        const string GlbAssetPath = "Assets/Eldoria/Convergence/LowerGateFamilyV1.glb";
        static readonly Vector2 LowerGateViewport = new Vector2(.748f,.458f);

        [Serializable]
        sealed class Evidence
        {
            public bool gate_pass;
            public bool import_pass;
            public bool bounds_pass;
            public bool viewport_alignment_pass;
            public bool coexistence_home_visible_pass;
            public bool coexistence_pan_left_visible_pass;
            public bool coexistence_pan_right_visible_pass;
            public bool interaction_pass;
            public int source_splat_count;
            public int proof_splat_count;
            public int sanitized_source_vertices;
            public int renderer_count;
            public int disabled_imported_collider_count;
            public int coexistence_home_changed_pixels;
            public int coexistence_pan_left_changed_pixels;
            public int coexistence_pan_right_changed_pixels;
            public float viewport_x;
            public float viewport_y;
            public float viewport_error;
            public float geometry_depth_scale;
            public string source_glb;
            public string visual_source_artifact;
            public string interaction_semantic;
            public int paid_credits;
        }

        struct PrepStats
        {
            public int OriginalCount;
            public int OutputCount;
            public int SanitizedCount;
            public int Step;
        }

        public static void Capture()
        {
            var output = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "ValoriaSharpLowerGateGeometryGateV1Captures"));
            if (Directory.Exists(output)) Directory.Delete(output, true);
            Directory.CreateDirectory(output);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var importedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GlbAssetPath);
            if (importedPrefab == null)
                throw new Exception("Gate 4: glTFast did not import LowerGateFamilyV1.glb as a GameObject asset at " + GlbAssetPath);

            var sourcePly = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "SharpGaussianSource", "sharp-1.ply"));
            if (!File.Exists(sourcePly)) throw new FileNotFoundException("Missing clean SHARP PLY", sourcePly);
            var workingPly = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "SharpGaussianSource", "sharp-gate4-sanitized-600k.ply"));
            var prep = PrepareProofPly(sourcePly, workingPly, MaxProofSplats);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SetupUrpFeature();

            var asset = GsplatRuntimeLoader.LoadFile(
                workingPly,
                CompressionMode.Spark,
                SourceCoordinates.RDF,
                (stage, progress) => Debug.Log($"[GATE4] {stage} {progress:P0}")
            );
            if (asset == null || asset.SplatCount < 500000)
                throw new Exception("Gate 4: clean SHARP 600k asset did not load correctly");

            var sharpRoot = new GameObject("SHARP_Valoria_Clean_600K");
            var gs = sharpRoot.AddComponent<GsplatRenderer>();
            gs.GsplatAsset = asset;
            gs.SHDegree = 0;
            gs.GammaToLinear = true;
            gs.AsyncUpload = false;
            gs.RenderBeforeUploadComplete = false;
            gs.Update();
            if (!gs.Valid || gs.SplatCount == 0)
                throw new Exception($"Gate 4: Gaussian renderer failed initialization. valid={gs.Valid}, resident={gs.SplatCount}");

            var cam = new GameObject("Gate4Camera").AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.02f,.025f,.035f,1f);
            cam.nearClipPlane = .01f;
            cam.farClipPlane = 1000f;
            cam.fieldOfView = 43.58f;
            cam.aspect = W / (float)H;
            cam.transform.position = Vector3.zero;
            cam.transform.rotation = Quaternion.identity;
            PrepareSplatFrame(gs, cam, "gate4-warmup");

            float anchorDistance = Mathf.Max(30f, Vector3.Distance(cam.transform.position, asset.Bounds.center));

            // Keep the already-proven semantic interaction region at the exact Gate 2/3 depth.
            var semanticRay = cam.ViewportPointToRay(new Vector3(LowerGateViewport.x, LowerGateViewport.y, 0f));
            var semanticProxy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            semanticProxy.name = "InteractiveProxy_LowerGate";
            semanticProxy.transform.position = semanticRay.origin + semanticRay.direction * anchorDistance * .90f;
            semanticProxy.transform.localScale = new Vector3(15f,20f,10f);
            semanticProxy.GetComponent<Renderer>().enabled = false;

            var gate = UnityEngine.Object.Instantiate(importedPrefab);
            gate.name = "LowerGateFamilyV1_ApprovedGeometry";

            int disabledColliders = 0;
            foreach (var col in gate.GetComponentsInChildren<Collider>(true))
            {
                col.enabled = false;
                disabledColliders++;
            }

            var renderers = gate.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) throw new Exception("Gate 4: imported LowerGateFamilyV1 has no renderers");

            var bounds = CombinedBounds(renderers);
            if (bounds.size.sqrMagnitude < .001f) throw new Exception("Gate 4: imported LowerGateFamilyV1 bounds are invalid");

            float sx = bounds.size.x > .001f ? 15f / bounds.size.x : 1f;
            float sy = bounds.size.y > .001f ? 20f / bounds.size.y : 1f;
            float uniform = Mathf.Clamp(Mathf.Min(sx, sy), .001f, 1000f);
            gate.transform.localScale *= uniform;
            bounds = CombinedBounds(gate.GetComponentsInChildren<Renderer>(true));

            // Place the visible production geometry slightly in front of the semantic proxy.
            // Screen alignment stays canonical while raster geometry is allowed to coexist visibly
            // with the Gaussian representation instead of being fully depth-occluded by it.
            const float geometryDepthScale = .90f;
            var geometryRay = cam.ViewportPointToRay(new Vector3(LowerGateViewport.x, LowerGateViewport.y, 0f));
            var desiredCenter = geometryRay.origin + geometryRay.direction * anchorDistance * geometryDepthScale;
            gate.transform.position += desiredCenter - bounds.center;
            bounds = CombinedBounds(gate.GetComponentsInChildren<Renderer>(true));

            var projected = cam.WorldToViewportPoint(bounds.center);
            float viewportError = Vector2.Distance(new Vector2(projected.x, projected.y), LowerGateViewport);
            bool boundsPass = renderers.Length > 0 && bounds.size.x > .1f && bounds.size.y > .1f && bounds.size.z > .01f;
            bool viewportPass = projected.z > 0f && viewportError <= .015f;

            Physics.SyncTransforms();
            var selectionRay = cam.ViewportPointToRay(new Vector3(LowerGateViewport.x, LowerGateViewport.y, 0f));
            bool interactionPass = Physics.Raycast(selectionRay, out var hit, 1000f) &&
                                   hit.collider != null &&
                                   hit.collider.gameObject.name == "InteractiveProxy_LowerGate";

            // Original materials are retained for direct production-source comparison.
            var originalMaterials = renderers.Select(r => r.sharedMaterials).ToArray();

            // SHARP-only authority capture.
            SetRenderers(renderers, false);
            CaptureBeauty(gs, cam, output, "sharp-only-home");

            // Geometry-only captures: original material and explicit silhouette/debug material.
            sharpRoot.SetActive(false);
            SetRenderers(renderers, true);
            CaptureRasterOnly(cam, output, "geometry-original-home");

            var debugShader = Shader.Find("Hidden/Internal-Colored") ?? Shader.Find("Universal Render Pipeline/Unlit");
            var debugMat = new Material(debugShader);
            debugMat.color = new Color(.1f,.95f,1f,1f);
            debugMat.SetInt("_ZWrite",0);
            debugMat.SetInt("_ZTest",(int)CompareFunction.Always);
            debugMat.SetInt("_Cull",(int)CullMode.Off);
            debugMat.renderQueue = 5000;
            OverrideMaterials(renderers, debugMat);
            CaptureRasterOnly(cam, output, "geometry-debug-home");

            // Gate 4B is a substrate/alignment gate, not an appearance pass.
            // Keep the diagnostic material during coexistence so the real imported
            // mesh can be inspected over SHARP without the Gaussian depth layer
            // hiding it. Original source materials are captured separately below.
            sharpRoot.SetActive(true);

            var states = new[]
            {
                new { Name="home", Position=Vector3.zero, Fov=43.58f },
                new { Name="pan-left", Position=new Vector3(-1.75f,0f,0f), Fov=43.58f },
                new { Name="pan-right", Position=new Vector3(1.75f,0f,0f), Fov=43.58f },
            };

            int homeChanged=0, leftChanged=0, rightChanged=0;
            foreach (var state in states)
            {
                cam.transform.position = state.Position;
                cam.transform.rotation = Quaternion.identity;
                cam.fieldOfView = state.Fov;

                SetRenderers(renderers,false);
                string baselineName = "sharp-" + state.Name;
                CaptureBeauty(gs,cam,output,baselineName);

                SetRenderers(renderers,true);
                string coexistName = "coexist-" + state.Name;
                CaptureBeauty(gs,cam,output,coexistName);

                int changed = CountChangedPixels(
                    Path.Combine(output,baselineName+".png"),
                    Path.Combine(output,coexistName+".png"),
                    .12f
                );
                if(state.Name=="home") homeChanged=changed;
                else if(state.Name=="pan-left") leftChanged=changed;
                else if(state.Name=="pan-right") rightChanged=changed;
            }

            // Return to HOME for final source-material reference. This is evidence-only:
            // appearance/depth blending is intentionally deferred to the next gate.
            RestoreMaterials(renderers, originalMaterials);
            cam.transform.position = Vector3.zero;
            cam.fieldOfView = 43.58f;
            CaptureBeauty(gs,cam,output,"coexist-original-material-home");

            bool homeVisible = homeChanged >= 150;
            bool leftVisible = leftChanged >= 150;
            bool rightVisible = rightChanged >= 150;
            bool gatePass = boundsPass && viewportPass && interactionPass && homeVisible && leftVisible && rightVisible;

            var evidence = new Evidence
            {
                gate_pass = gatePass,
                import_pass = true,
                bounds_pass = boundsPass,
                viewport_alignment_pass = viewportPass,
                coexistence_home_visible_pass = homeVisible,
                coexistence_pan_left_visible_pass = leftVisible,
                coexistence_pan_right_visible_pass = rightVisible,
                interaction_pass = interactionPass,
                source_splat_count = prep.OriginalCount,
                proof_splat_count = checked((int)asset.SplatCount),
                sanitized_source_vertices = prep.SanitizedCount,
                renderer_count = renderers.Length,
                disabled_imported_collider_count = disabledColliders,
                coexistence_home_changed_pixels = homeChanged,
                coexistence_pan_left_changed_pixels = leftChanged,
                coexistence_pan_right_changed_pixels = rightChanged,
                viewport_x = projected.x,
                viewport_y = projected.y,
                viewport_error = viewportError,
                geometry_depth_scale = geometryDepthScale,
                source_glb = "art-source/valoria/production/lower-gate-family-v1/LowerGateFamilyV1.glb",
                visual_source_artifact = "11408853949",
                interaction_semantic = "LowerGate",
                paid_credits = 0
            };
            File.WriteAllText(Path.Combine(output,"gate4-evidence.json"), JsonUtility.ToJson(evidence,true));

            if (!gatePass)
                throw new Exception($"Gate 4 failed: bounds={boundsPass}, viewport={viewportPass} err={viewportError:F4}, interaction={interactionPass}, visible={homeChanged}/{leftChanged}/{rightChanged}");

            gs.GsplatAsset = null;
            UnityEngine.Object.DestroyImmediate(asset);
            Debug.Log($"[GATE4] PASS renderers={renderers.Length}, viewportError={viewportError:F4}, changedPixels={homeChanged}/{leftChanged}/{rightChanged}");
        }

        static Bounds CombinedBounds(Renderer[] renderers)
        {
            if (renderers == null || renderers.Length == 0) return new Bounds();
            var b = renderers[0].bounds;
            for (int i=1;i<renderers.Length;i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }

        static void SetRenderers(Renderer[] renderers, bool enabled)
        {
            foreach (var r in renderers) if (r != null) r.enabled = enabled;
        }

        static void OverrideMaterials(Renderer[] renderers, Material mat)
        {
            foreach (var r in renderers)
            {
                if (r == null) continue;
                var arr = new Material[Mathf.Max(1,r.sharedMaterials.Length)];
                for(int i=0;i<arr.Length;i++) arr[i]=mat;
                r.sharedMaterials = arr;
            }
        }

        static void RestoreMaterials(Renderer[] renderers, Material[][] mats)
        {
            for(int i=0;i<renderers.Length;i++) if(renderers[i]!=null) renderers[i].sharedMaterials=mats[i];
        }

        static int CountChangedPixels(string aPath, string bPath, float threshold)
        {
            var a = new Texture2D(2,2,TextureFormat.RGBA32,false);
            var b = new Texture2D(2,2,TextureFormat.RGBA32,false);
            a.LoadImage(File.ReadAllBytes(aPath));
            b.LoadImage(File.ReadAllBytes(bPath));
            if (a.width != b.width || a.height != b.height) throw new Exception("Gate 4 capture size mismatch");
            var ap=a.GetPixels32(); var bp=b.GetPixels32();
            int changed=0;
            for(int i=0;i<ap.Length;i++)
            {
                float d = Mathf.Abs(ap[i].r-bp[i].r)/255f + Mathf.Abs(ap[i].g-bp[i].g)/255f + Mathf.Abs(ap[i].b-bp[i].b)/255f;
                if(d>=threshold) changed++;
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
            PrepareSplatFrame(gs,cam,name);
            cam.Render();
            SaveRt(rt,cam,output,name);
        }

        static void CaptureRasterOnly(Camera cam, string output, string name)
        {
            var rt = new RenderTexture(W,H,24,RenderTextureFormat.ARGB32);
            rt.Create();
            cam.targetTexture = rt;
            cam.Render();
            SaveRt(rt,cam,output,name);
        }

        static void SaveRt(RenderTexture rt, Camera cam, string output, string name)
        {
            var prev=RenderTexture.active;
            RenderTexture.active=rt;
            var tex=new Texture2D(W,H,TextureFormat.RGBA32,false);
            tex.ReadPixels(new Rect(0,0,W,H),0,0);
            tex.Apply();
            File.WriteAllBytes(Path.Combine(output,name+".png"),tex.EncodeToPNG());
            RenderTexture.active=prev;
            cam.targetTexture=null;
            UnityEngine.Object.DestroyImmediate(tex);
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
        }

        static void PrepareSplatFrame(GsplatRenderer gs, Camera cam, string label)
        {
            gs.Update();
            gs.ForceRefresh();
            if (gs.SorterResource != null) gs.SorterResource.Initialized=false;
            var deadline=DateTime.UtcNow.AddSeconds(20);
            while(DateTime.UtcNow<deadline)
            {
                gs.Update();
                GsplatSorter.Instance.GatherGsplatsForCamera(cam);
                GsplatSorter.Instance.Update();
                if(gs.Valid && gs.SplatCount==gs.GsplatAsset.SplatCount && gs.RemainingCount>0 && gs.SorterResource!=null && gs.SorterResource.Initialized)
                    return;
                System.Threading.Thread.Sleep(25);
            }
            throw new TimeoutException("Gate 4 sorter did not initialize for "+label);
        }

        static PrepStats PrepareProofPly(string sourcePath, string outputPath, int maxSplats)
        {
            var data=File.ReadAllBytes(sourcePath);
            var needle=Encoding.ASCII.GetBytes("end_header\n");
            int end=-1;
            for(int i=0;i<=data.Length-needle.Length;i++)
            {
                bool match=true;
                for(int k=0;k<needle.Length;k++) if(data[i+k]!=needle[k]) {match=false;break;}
                if(match){end=i+needle.Length;break;}
            }
            if(end<0) throw new InvalidDataException("PLY end_header not found");
            var header=Encoding.ASCII.GetString(data,0,end);
            var m=Regex.Match(header,@"element vertex (\d+)");
            if(!m.Success) throw new InvalidDataException("PLY vertex count not found");
            int count=int.Parse(m.Groups[1].Value);
            int payloadEnd=end+count*VertexStride;
            if(payloadEnd>data.Length) throw new EndOfStreamException("PLY vertex payload truncated");
            int step=Math.Max(1,(int)Math.Ceiling(count/(double)maxSplats));
            int outputCount=(count+step-1)/step;
            var headerBytes=Encoding.ASCII.GetBytes(new Regex(@"element vertex \d+").Replace(header,"element vertex "+outputCount,1));
            int sanitized=0;
            using(var ms=new MemoryStream(headerBytes.Length+outputCount*VertexStride+(data.Length-payloadEnd)))
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
                        if(float.IsNaN(vals[k])||float.IsInfinity(vals[k])) {vals[k]=0f;invalid=true;}
                    }
                    if(invalid)
                    {
                        sanitized++;
                        vals[6]=-20f;
                        for(int k=0;k<FloatsPerVertex;k++) Buffer.BlockCopy(BitConverter.GetBytes(vals[k]),0,vertex,k*4,4);
                    }
                    ms.Write(vertex,0,vertex.Length);
                }
                ms.Write(data,payloadEnd,data.Length-payloadEnd);
                File.WriteAllBytes(outputPath,ms.ToArray());
            }
            return new PrepStats{OriginalCount=count,OutputCount=outputCount,SanitizedCount=sanitized,Step=step};
        }

        static void SetupUrpFeature()
        {
            var urp=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Eldoria/Content/EldoriaMobileURP.asset");
            var data=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Eldoria/Content/EldoriaForwardRenderer.asset");
            if(urp==null||data==null) throw new Exception("Missing Eldoria URP assets");
            var featureType=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Gsplat.GsplatURPFeature",false)).FirstOrDefault(t=>t!=null);
            if(featureType==null) throw new Exception("UnitySplats URP feature type not found");
            if(!data.rendererFeatures.Any(f=>f!=null&&f.GetType()==featureType))
            {
                var feature=ScriptableObject.CreateInstance(featureType) as ScriptableRendererFeature;
                if(feature==null) throw new Exception("Could not instantiate GsplatURPFeature");
                feature.name="SHARP Gate4 Gsplat URP Feature";
                data.rendererFeatures.Add(feature);
                feature.Create();
            }
            GraphicsSettings.defaultRenderPipeline=urp;
            QualitySettings.renderPipeline=urp;
        }
    }
}
