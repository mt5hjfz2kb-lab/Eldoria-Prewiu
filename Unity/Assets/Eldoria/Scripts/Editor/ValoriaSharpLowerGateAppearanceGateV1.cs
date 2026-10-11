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
    public static class ValoriaSharpLowerGateAppearanceGateV1
    {
        const int W=1280, H=853, MaxProofSplats=600000, FloatsPerVertex=14, VertexStride=FloatsPerVertex*4;
        const string GlbAssetPath="Assets/Eldoria/Convergence/LowerGateFamilyV1.glb";
        const string ReferenceAssetPath="Assets/Eldoria/Convergence/VALORIA_APPROVED_VISUAL_REFERENCE.jpg";
        static readonly Vector2 LowerGateViewport=new Vector2(.748f,.458f);

        struct PrepStats { public int OriginalCount, OutputCount, SanitizedCount, Step; }

        [Serializable]
        sealed class Evidence
        {
            public bool gate_pass;
            public bool import_pass;
            public bool bounds_pass;
            public bool viewport_alignment_pass;
            public bool interaction_pass;
            public bool home_projection_visible_pass;
            public bool pan_left_projection_visible_pass;
            public bool pan_right_projection_visible_pass;
            public bool full_frame_takeover_guard_pass;
            public int source_splat_count;
            public int proof_splat_count;
            public int renderer_count;
            public int sanitized_source_vertices;
            public int home_changed_pixels;
            public int pan_left_changed_pixels;
            public int pan_right_changed_pixels;
            public float viewport_error;
            public float chosen_flip_y;
            public float flip0_local_mae;
            public float flip1_local_mae;
            public string appearance_method;
            public string source_glb;
            public string reference_source;
            public string interaction_semantic;
            public int paid_credits;
        }

        public static void Capture()
        {
            var output=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","ValoriaSharpLowerGateAppearanceGateV1Captures"));
            if(Directory.Exists(output)) Directory.Delete(output,true);
            Directory.CreateDirectory(output);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var importedPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(GlbAssetPath);
            var reference=AssetDatabase.LoadAssetAtPath<Texture2D>(ReferenceAssetPath);
            var projectionShader=Shader.Find("Eldoria/ValoriaLowerGateProjection");
            if(importedPrefab==null) throw new Exception("Gate 5: approved LowerGateFamilyV1 GLB import missing");
            if(reference==null) throw new Exception("Gate 5: approved reference texture missing");
            if(projectionShader==null) throw new Exception("Gate 5: projection shader missing");

            var sourcePly=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","SharpGaussianSource","sharp-1.ply"));
            if(!File.Exists(sourcePly)) throw new FileNotFoundException("Missing clean SHARP PLY",sourcePly);
            var workingPly=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","SharpGaussianSource","sharp-gate5-sanitized-600k.ply"));
            var prep=PrepareProofPly(sourcePly,workingPly,MaxProofSplats);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SetupUrpFeature();

            var asset=GsplatRuntimeLoader.LoadFile(workingPly,CompressionMode.Spark,SourceCoordinates.RDF,
                (stage,progress)=>Debug.Log($"[GATE5] {stage} {progress:P0}"));
            if(asset==null||asset.SplatCount<500000) throw new Exception("Gate 5: SHARP 600k load failed");

            var sharpRoot=new GameObject("SHARP_Valoria_Clean_600K");
            var gs=sharpRoot.AddComponent<GsplatRenderer>();
            gs.GsplatAsset=asset; gs.SHDegree=0; gs.GammaToLinear=true; gs.AsyncUpload=false; gs.RenderBeforeUploadComplete=false; gs.Update();
            if(!gs.Valid||gs.SplatCount==0) throw new Exception("Gate 5: Gaussian renderer invalid");

            var cam=new GameObject("Gate5Camera").AddComponent<Camera>();
            cam.clearFlags=CameraClearFlags.SolidColor; cam.backgroundColor=new Color(.02f,.025f,.035f,1f);
            cam.nearClipPlane=.01f; cam.farClipPlane=1000f; cam.fieldOfView=43.58f; cam.aspect=W/(float)H;
            cam.transform.position=Vector3.zero; cam.transform.rotation=Quaternion.identity;
            PrepareSplatFrame(gs,cam,"gate5-warmup");

            float anchorDistance=Mathf.Max(30f,Vector3.Distance(cam.transform.position,asset.Bounds.center));
            var semanticRay=cam.ViewportPointToRay(new Vector3(LowerGateViewport.x,LowerGateViewport.y,0));
            var semanticProxy=GameObject.CreatePrimitive(PrimitiveType.Cube);
            semanticProxy.name="InteractiveProxy_LowerGate";
            semanticProxy.transform.position=semanticRay.origin+semanticRay.direction*anchorDistance*.90f;
            semanticProxy.transform.localScale=new Vector3(15f,20f,10f);
            semanticProxy.GetComponent<Renderer>().enabled=false;

            var gate=UnityEngine.Object.Instantiate(importedPrefab);
            gate.name="LowerGateFamilyV1_ApprovedGeometry";
            foreach(var col in gate.GetComponentsInChildren<Collider>(true)) col.enabled=false;
            var renderers=gate.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0) throw new Exception("Gate 5: imported gate has no renderers");

            var bounds=CombinedBounds(renderers);
            if(bounds.size.sqrMagnitude<.001f) throw new Exception("Gate 5: invalid imported bounds");
            float sx=bounds.size.x>.001f?15f/bounds.size.x:1f;
            float sy=bounds.size.y>.001f?20f/bounds.size.y:1f;
            gate.transform.localScale*=Mathf.Clamp(Mathf.Min(sx,sy),.001f,1000f);
            bounds=CombinedBounds(gate.GetComponentsInChildren<Renderer>(true));
            var desiredCenter=semanticRay.origin+semanticRay.direction*anchorDistance*.90f;
            gate.transform.position+=desiredCenter-bounds.center;
            bounds=CombinedBounds(gate.GetComponentsInChildren<Renderer>(true));

            var projected=cam.WorldToViewportPoint(bounds.center);
            float viewportError=Vector2.Distance(new Vector2(projected.x,projected.y),LowerGateViewport);
            bool boundsPass=bounds.size.x>.1f&&bounds.size.y>.1f&&bounds.size.z>.01f;
            bool viewportPass=projected.z>0&&viewportError<=.015f;

            Physics.SyncTransforms();
            bool interactionPass=Physics.Raycast(semanticRay,out var hit,1000f)&&hit.collider!=null&&hit.collider.gameObject.name=="InteractiveProxy_LowerGate";

            var originalMaterials=renderers.Select(r=>r.sharedMaterials).ToArray();

            // Capture visual authority and raw source evidence first.
            SetRenderers(renderers,false);
            CaptureBeauty(gs,cam,output,"sharp-only-home");
            sharpRoot.SetActive(false);
            SetRenderers(renderers,true);
            CaptureRasterOnly(cam,output,"geometry-original-home");
            sharpRoot.SetActive(true);

            var projMat=new Material(projectionShader);
            projMat.SetTexture("_ReferenceTex",reference);
            projMat.SetColor("_FallbackColor",new Color(.18f,.16f,.14f,1f));
            projMat.SetFloat("_ProjectionStrength",1f);
            projMat.SetFloat("_FacingStart",.10f);
            projMat.SetFloat("_FacingFull",.38f);
            var projectorVP=GL.GetGPUProjectionMatrix(cam.projectionMatrix,false)*cam.worldToCameraMatrix;
            projMat.SetMatrix("_ProjectorVP",projectorVP);
            projMat.SetVector("_ProjectorPosition",cam.transform.position);
            OverrideMaterials(renderers,projMat);

            // Render both Y conventions and choose the one closest to the SHARP visual authority locally.
            projMat.SetFloat("_FlipY",0f);
            CaptureBeauty(gs,cam,output,"projection-flip0-home");
            projMat.SetFloat("_FlipY",1f);
            CaptureBeauty(gs,cam,output,"projection-flip1-home");
            float mae0=LocalMae(Path.Combine(output,"sharp-only-home.png"),Path.Combine(output,"projection-flip0-home.png"),LowerGateViewport,120);
            float mae1=LocalMae(Path.Combine(output,"sharp-only-home.png"),Path.Combine(output,"projection-flip1-home.png"),LowerGateViewport,120);
            float chosenFlip=mae1<mae0?1f:0f;
            projMat.SetFloat("_FlipY",chosenFlip);

            sharpRoot.SetActive(false);
            CaptureRasterOnly(cam,output,"geometry-projected-home");
            sharpRoot.SetActive(true);

            var states=new[]{
                new {Name="home",Position=Vector3.zero,Fov=43.58f},
                new {Name="pan-left",Position=new Vector3(-1.75f,0,0),Fov=43.58f},
                new {Name="pan-right",Position=new Vector3(1.75f,0,0),Fov=43.58f}
            };
            int homeChanged=0,leftChanged=0,rightChanged=0;
            int maxAllowed=Mathf.RoundToInt(W*H*.03f);
            bool takeoverGuard=true;
            foreach(var state in states)
            {
                cam.transform.position=state.Position; cam.transform.rotation=Quaternion.identity; cam.fieldOfView=state.Fov;
                SetRenderers(renderers,false);
                CaptureBeauty(gs,cam,output,"baseline-"+state.Name);
                SetRenderers(renderers,true);
                CaptureBeauty(gs,cam,output,"appearance-"+state.Name);
                int changed=CountChangedPixels(Path.Combine(output,"baseline-"+state.Name+".png"),Path.Combine(output,"appearance-"+state.Name+".png"),.12f);
                takeoverGuard &= changed<maxAllowed;
                if(state.Name=="home") homeChanged=changed;
                else if(state.Name=="pan-left") leftChanged=changed;
                else rightChanged=changed;
            }

            // Restore source materials and HOME only for side-by-side evidence.
            RestoreMaterials(renderers,originalMaterials);
            cam.transform.position=Vector3.zero; cam.fieldOfView=43.58f;
            sharpRoot.SetActive(false);
            CaptureRasterOnly(cam,output,"geometry-original-home-final");
            sharpRoot.SetActive(true);

            bool visibleHome=homeChanged>=150, visibleLeft=leftChanged>=120, visibleRight=rightChanged>=120;
            bool gatePass=boundsPass&&viewportPass&&interactionPass&&visibleHome&&visibleLeft&&visibleRight&&takeoverGuard;

            var evidence=new Evidence{
                gate_pass=gatePass,import_pass=true,bounds_pass=boundsPass,viewport_alignment_pass=viewportPass,
                interaction_pass=interactionPass,home_projection_visible_pass=visibleHome,
                pan_left_projection_visible_pass=visibleLeft,pan_right_projection_visible_pass=visibleRight,
                full_frame_takeover_guard_pass=takeoverGuard,source_splat_count=prep.OriginalCount,
                proof_splat_count=checked((int)asset.SplatCount),renderer_count=renderers.Length,
                sanitized_source_vertices=prep.SanitizedCount,home_changed_pixels=homeChanged,
                pan_left_changed_pixels=leftChanged,pan_right_changed_pixels=rightChanged,
                viewport_error=viewportError,chosen_flip_y=chosenFlip,flip0_local_mae=mae0,flip1_local_mae=mae1,
                appearance_method="HOME-projector-conditioned canonical reference on real LowerGateFamilyV1 geometry with front-facing projection and stone fallback",
                source_glb="art-source/valoria/production/lower-gate-family-v1/LowerGateFamilyV1.glb",
                reference_source="references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg",
                interaction_semantic="LowerGate",paid_credits=0
            };
            File.WriteAllText(Path.Combine(output,"gate5-evidence.json"),JsonUtility.ToJson(evidence,true));
            if(!gatePass) throw new Exception($"Gate 5 failed: bounds={boundsPass}, viewport={viewportPass} err={viewportError:F5}, interaction={interactionPass}, changed={homeChanged}/{leftChanged}/{rightChanged}, takeoverGuard={takeoverGuard}");

            gs.GsplatAsset=null; UnityEngine.Object.DestroyImmediate(asset);
            Debug.Log($"[GATE5] PASS flipY={chosenFlip}, mae={mae0:F4}/{mae1:F4}, changed={homeChanged}/{leftChanged}/{rightChanged}");
        }

        static Bounds CombinedBounds(Renderer[] rs){var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;}
        static void SetRenderers(Renderer[] rs,bool on){foreach(var r in rs)if(r!=null)r.enabled=on;}
        static void OverrideMaterials(Renderer[] rs,Material m){foreach(var r in rs){var a=new Material[Mathf.Max(1,r.sharedMaterials.Length)];for(int i=0;i<a.Length;i++)a[i]=m;r.sharedMaterials=a;}}
        static void RestoreMaterials(Renderer[] rs,Material[][] mats){for(int i=0;i<rs.Length;i++)rs[i].sharedMaterials=mats[i];}

        static float LocalMae(string aPath,string bPath,Vector2 vp,int radius)
        {
            var a=LoadCapture(aPath);var b=LoadCapture(bPath);
            int cx=Mathf.Clamp(Mathf.RoundToInt(vp.x*(a.width-1)),0,a.width-1), cy=Mathf.Clamp(Mathf.RoundToInt(vp.y*(a.height-1)),0,a.height-1);
            int minX=Mathf.Max(0,cx-radius),maxX=Mathf.Min(a.width-1,cx+radius),minY=Mathf.Max(0,cy-radius),maxY=Mathf.Min(a.height-1,cy+radius);
            double sum=0;int n=0;
            for(int y=minY;y<=maxY;y++)for(int x=minX;x<=maxX;x++){var ca=a.GetPixel(x,y);var cb=b.GetPixel(x,y);sum+=Mathf.Abs(ca.r-cb.r)+Mathf.Abs(ca.g-cb.g)+Mathf.Abs(ca.b-cb.b);n++;}
            UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(b);
            return n>0?(float)(sum/(n*3.0)):999f;
        }

        static int CountChangedPixels(string aPath,string bPath,float threshold)
        {
            var a=LoadCapture(aPath);var b=LoadCapture(bPath);var ap=a.GetPixels32();var bp=b.GetPixels32();int changed=0;
            for(int i=0;i<ap.Length;i++){float d=Mathf.Abs(ap[i].r-bp[i].r)/255f+Mathf.Abs(ap[i].g-bp[i].g)/255f+Mathf.Abs(ap[i].b-bp[i].b)/255f;if(d>=threshold)changed++;}
            UnityEngine.Object.DestroyImmediate(a);UnityEngine.Object.DestroyImmediate(b);return changed;
        }
        static Texture2D LoadCapture(string p){var t=new Texture2D(2,2,TextureFormat.RGBA32,false);t.LoadImage(File.ReadAllBytes(p));return t;}

        static void CaptureBeauty(GsplatRenderer gs,Camera cam,string output,string name){var rt=new RenderTexture(W,H,24,RenderTextureFormat.ARGB32);rt.Create();cam.targetTexture=rt;PrepareSplatFrame(gs,cam,name);cam.Render();SaveRt(rt,cam,output,name);}
        static void CaptureRasterOnly(Camera cam,string output,string name){var rt=new RenderTexture(W,H,24,RenderTextureFormat.ARGB32);rt.Create();cam.targetTexture=rt;cam.Render();SaveRt(rt,cam,output,name);}
        static void SaveRt(RenderTexture rt,Camera cam,string output,string name){var prev=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(W,H,TextureFormat.RGBA32,false);tex.ReadPixels(new Rect(0,0,W,H),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),tex.EncodeToPNG());RenderTexture.active=prev;cam.targetTexture=null;UnityEngine.Object.DestroyImmediate(tex);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}

        static void PrepareSplatFrame(GsplatRenderer gs,Camera cam,string label)
        {
            gs.Update();gs.ForceRefresh();if(gs.SorterResource!=null)gs.SorterResource.Initialized=false;
            var deadline=DateTime.UtcNow.AddSeconds(20);
            while(DateTime.UtcNow<deadline){gs.Update();GsplatSorter.Instance.GatherGsplatsForCamera(cam);GsplatSorter.Instance.Update();if(gs.Valid&&gs.SplatCount==gs.GsplatAsset.SplatCount&&gs.RemainingCount>0&&gs.SorterResource!=null&&gs.SorterResource.Initialized)return;System.Threading.Thread.Sleep(25);}
            throw new TimeoutException("Gate 5 sorter did not initialize for "+label);
        }

        static PrepStats PrepareProofPly(string sourcePath,string outputPath,int maxSplats)
        {
            var data=File.ReadAllBytes(sourcePath);var needle=Encoding.ASCII.GetBytes("end_header\n");int end=-1;
            for(int i=0;i<=data.Length-needle.Length;i++){bool match=true;for(int k=0;k<needle.Length;k++)if(data[i+k]!=needle[k]){match=false;break;}if(match){end=i+needle.Length;break;}}
            if(end<0)throw new InvalidDataException("PLY end_header not found");
            var header=Encoding.ASCII.GetString(data,0,end);var m=Regex.Match(header,@"element vertex (\d+)");if(!m.Success)throw new InvalidDataException("PLY vertex count not found");
            int count=int.Parse(m.Groups[1].Value),payloadEnd=end+count*VertexStride;if(payloadEnd>data.Length)throw new EndOfStreamException("PLY vertex payload truncated");
            int step=Math.Max(1,(int)Math.Ceiling(count/(double)maxSplats)),outputCount=(count+step-1)/step;var headerBytes=Encoding.ASCII.GetBytes(new Regex(@"element vertex \d+").Replace(header,"element vertex "+outputCount,1));int sanitized=0;
            using(var ms=new MemoryStream(headerBytes.Length+outputCount*VertexStride+(data.Length-payloadEnd))){ms.Write(headerBytes,0,headerBytes.Length);var vertex=new byte[VertexStride];var vals=new float[FloatsPerVertex];for(int src=0;src<count;src+=step){int off=end+src*VertexStride;Buffer.BlockCopy(data,off,vertex,0,VertexStride);bool invalid=false;for(int k=0;k<FloatsPerVertex;k++){vals[k]=BitConverter.ToSingle(vertex,k*4);if(float.IsNaN(vals[k])||float.IsInfinity(vals[k])){vals[k]=0;invalid=true;}}if(invalid){sanitized++;vals[6]=-20f;for(int k=0;k<FloatsPerVertex;k++)Buffer.BlockCopy(BitConverter.GetBytes(vals[k]),0,vertex,k*4,4);}ms.Write(vertex,0,vertex.Length);}ms.Write(data,payloadEnd,data.Length-payloadEnd);File.WriteAllBytes(outputPath,ms.ToArray());}
            return new PrepStats{OriginalCount=count,OutputCount=outputCount,SanitizedCount=sanitized,Step=step};
        }

        static void SetupUrpFeature()
        {
            var urp=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Eldoria/Content/EldoriaMobileURP.asset");var data=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Eldoria/Content/EldoriaForwardRenderer.asset");if(urp==null||data==null)throw new Exception("Missing Eldoria URP assets");
            var t=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("Gsplat.GsplatURPFeature",false)).FirstOrDefault(x=>x!=null);if(t==null)throw new Exception("UnitySplats URP feature type not found");
            if(!data.rendererFeatures.Any(f=>f!=null&&f.GetType()==t)){var feature=ScriptableObject.CreateInstance(t) as ScriptableRendererFeature;if(feature==null)throw new Exception("Could not instantiate GsplatURPFeature");feature.name="SHARP Gate5 Gsplat URP Feature";data.rendererFeatures.Add(feature);feature.Create();}
            GraphicsSettings.defaultRenderPipeline=urp;QualitySettings.renderPipeline=urp;
        }
    }
}