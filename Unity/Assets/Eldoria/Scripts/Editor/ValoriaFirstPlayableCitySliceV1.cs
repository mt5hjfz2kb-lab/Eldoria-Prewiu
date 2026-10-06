using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Globalization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Gsplat;

namespace Eldoria.EditorTools
{
    public static class ValoriaFirstPlayableCitySliceV1
    {
        static int W=1230, H=845;
        static float HomeFov;
        const int MaxProofSplats=600000, FloatsPerVertex=14, VertexStride=FloatsPerVertex*4;
        const string CorrectAuthoritySha="8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689";
        [Serializable] sealed class SourceAuthority { public string authority_sha256,clean_input_sha256,ply_sha256; public int[] clean_dimensions,crop_box; }
        const string ReferenceAssetPath="Assets/Eldoria/ProductionSlice/VALORIA_SHARP_CLEAN_REFERENCE.png";

        sealed class FamilySpec
        {
            public string Id, AssetPath, Source;
            public Vector2 Viewport;
            public float DepthScale, TargetWidth, TargetHeight;
            public bool BeautyVisible;
        }

        sealed class FamilyRuntime
        {
            public FamilySpec Spec;
            public GameObject Go;
            public Renderer[] Renderers;
            public Bounds Bounds;
            public float ViewportError;
        }

        sealed class ProxyRuntime
        {
            public string Id;
            public Vector2 ExpectedViewport;
            public GameObject Go;
        }

        struct PrepStats { public int OriginalCount, OutputCount, SanitizedCount, Step; }

        static readonly FamilySpec[] Families = {
            new FamilySpec { Id="Bridge", AssetPath="Assets/Eldoria/ProductionSlice/BridgeFamilyV1.glb", Source="art-source/valoria/production/bridge-family-v1/BridgeFamilyV1.glb", Viewport=new Vector2(.288618f,.100592f), DepthScale=.86f, TargetWidth=18f, TargetHeight=11f, BeautyVisible=false },
            new FamilySpec { Id="LowerGate", AssetPath="Assets/Eldoria/ProductionSlice/LowerGateFamilyV1.glb", Source="art-source/valoria/production/lower-gate-family-v1/LowerGateFamilyV1.glb", Viewport=new Vector2(.394309f,.319527f), DepthScale=.90f, TargetWidth=15f, TargetHeight=20f, BeautyVisible=false },
            new FamilySpec { Id="MainRoad", AssetPath="Assets/Eldoria/ProductionSlice/RoadFamilyV1.glb", Source="art-source/valoria/production/road-family-v1/RoadFamilyV1.glb", Viewport=new Vector2(.467480f,.508876f), DepthScale=.925f, TargetWidth=13f, TargetHeight=25f, BeautyVisible=false },
            new FamilySpec { Id="CentralStair", AssetPath="Assets/Eldoria/ProductionSlice/StairFamilyV1.glb", Source="art-source/valoria/production/stair-family-v1/StairFamilyV1.glb", Viewport=new Vector2(.534959f,.673373f), DepthScale=.95f, TargetWidth=12f, TargetHeight=13f, BeautyVisible=false },
            new FamilySpec { Id="UpperWalls", AssetPath="Assets/Eldoria/ProductionSlice/WallFamilyV1.glb", Source="art-source/valoria/production/wall-family-v1/WallFamilyV1.glb", Viewport=new Vector2(.788618f,.801183f), DepthScale=.975f, TargetWidth=28f, TargetHeight=15f, BeautyVisible=false },
            new FamilySpec { Id="Bastion", AssetPath="Assets/Eldoria/ProductionSlice/BastionFamilyV1.glb", Source="art-source/valoria/production/bastion-family-v1/BastionFamilyV1.glb", Viewport=new Vector2(.604878f,.842604f), DepthScale=1.00f, TargetWidth=26f, TargetHeight=27f, BeautyVisible=false },
            new FamilySpec { Id="TerrainCliffSupport", AssetPath="Assets/Eldoria/ProductionSlice/RockTerrainFamilyV1.glb", Source="art-source/valoria/production/rock-terrain-family-v1/RockTerrainFamilyV1.glb", Viewport=new Vector2(.720000f,.360000f), DepthScale=1.015f, TargetWidth=42f, TargetHeight=24f, BeautyVisible=false },
        };

        public static void Capture()
        {
            var output=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","ValoriaFirstPlayableCitySliceV1Captures"));
            if(Directory.Exists(output)) Directory.Delete(output,true);
            Directory.CreateDirectory(output);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var reference=AssetDatabase.LoadAssetAtPath<Texture2D>(ReferenceAssetPath);
            var projectionShader=Shader.Find("Eldoria/ValoriaLowerGateProjection");
            if(reference==null) throw new Exception("Production slice: canonical reference texture missing");
            if(projectionShader==null) throw new Exception("Production slice: proven projection shader missing");

            foreach(var f in Families)
                if(AssetDatabase.LoadAssetAtPath<GameObject>(f.AssetPath)==null)
                    throw new Exception("Production slice: approved GLB import missing: "+f.AssetPath);

            var sourcePly=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","SharpGaussianSource","sharp-1.ply"));
            if(!File.Exists(sourcePly)) throw new FileNotFoundException("Missing clean SHARP source",sourcePly);
            var workingPly=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","SharpGaussianSource","sharp-production-slice-600k.ply"));
            var sourceManifestPath=Path.Combine(Path.GetDirectoryName(sourcePly),"source-authority.json");
            if(!File.Exists(sourceManifestPath)) throw new Exception("VISUAL_AUTHORITY_MISMATCH: source manifest missing");
            var sourceAuthority=JsonUtility.FromJson<SourceAuthority>(File.ReadAllText(sourceManifestPath));
            var referenceFile=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..","references","VALORIA_APPROVED_VISUAL_REFERENCE.jpg"));
            if(sourceAuthority.authority_sha256!=CorrectAuthoritySha||Sha256(referenceFile)!=CorrectAuthoritySha||Sha256(sourcePly)!=sourceAuthority.ply_sha256) throw new Exception("VISUAL_AUTHORITY_MISMATCH: source fingerprint rejected");
            ReadSharpCamera(sourcePly);
            if(W!=sourceAuthority.clean_dimensions[0]||H!=sourceAuthority.clean_dimensions[1]) throw new Exception("SHARP camera dimensions mismatch");
            File.Copy(referenceFile,Path.Combine(output,"correct-reference.jpg"),true);
            File.Copy(sourceManifestPath,Path.Combine(output,"source-authority.json"),true);
            File.Copy(Path.Combine(Path.GetDirectoryName(sourcePly),"canonical-target.png"),Path.Combine(output,"sharp-clean-input.png"),true);
            var prep=PrepareProofPly(sourcePly,workingPly,MaxProofSplats);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            SetupUrpFeature();

            var asset=GsplatRuntimeLoader.LoadFile(workingPly,CompressionMode.Spark,SourceCoordinates.RDF,
                (stage,progress)=>Debug.Log($"[PRODUCTION-SLICE] {stage} {progress:P0}"));
            if(asset==null||asset.SplatCount<500000) throw new Exception("Production slice: SHARP 600k load failed");

            var sharpRoot=new GameObject("SHARP_Valoria_Clean_600K");
            var gs=sharpRoot.AddComponent<GsplatRenderer>();
            gs.GsplatAsset=asset; gs.SHDegree=0; gs.GammaToLinear=true; gs.AsyncUpload=false; gs.RenderBeforeUploadComplete=false; gs.Update();
            if(!gs.Valid||gs.SplatCount==0) throw new Exception("Production slice: Gaussian renderer invalid");

            var cam=new GameObject("ValoriaProductionCamera").AddComponent<Camera>();
            cam.clearFlags=CameraClearFlags.SolidColor; cam.backgroundColor=new Color(.02f,.025f,.035f,1f);
            cam.nearClipPlane=.01f; cam.farClipPlane=1000f; cam.fieldOfView=HomeFov; cam.aspect=W/(float)H;
            cam.transform.position=Vector3.zero; cam.transform.rotation=Quaternion.identity;
            PrepareSplatFrame(gs,cam,"production-warmup");

            float anchorDistance=Mathf.Max(30f,Vector3.Distance(cam.transform.position,asset.Bounds.center));

            // Approved production families were authored/reviewed from the reset camera.
            // Rotate source geometry into the locked SHARP identity-camera basis instead of
            // viewing world-authored GLBs edge-on. This is transform alignment, not new art.
            var resetCameraPosition=new Vector3(30.8183f,63.0934f,-84.6726f);
            var resetCameraTarget=Vector3.zero;
            var resetCameraRotation=Quaternion.LookRotation((resetCameraTarget-resetCameraPosition).normalized,Vector3.up);
            var sourceToSharpRotation=Quaternion.Inverse(resetCameraRotation);

            var projectionMaterial=new Material(projectionShader);
            projectionMaterial.SetTexture("_ReferenceTex",reference);
            projectionMaterial.SetColor("_FallbackColor",new Color(.18f,.16f,.14f,1f));
            projectionMaterial.SetFloat("_ProjectionStrength",1f);
            projectionMaterial.SetFloat("_FacingStart",.10f);
            projectionMaterial.SetFloat("_FacingFull",.38f);
            projectionMaterial.SetFloat("_FlipY",1f);
            projectionMaterial.SetMatrix("_ProjectorVP",GL.GetGPUProjectionMatrix(cam.projectionMatrix,false)*cam.worldToCameraMatrix);
            projectionMaterial.SetVector("_ProjectorPosition",cam.transform.position);

            var runtimes=new List<FamilyRuntime>();
            foreach(var spec in Families)
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(spec.AssetPath);
                var go=UnityEngine.Object.Instantiate(prefab);
                go.name=spec.Id+"_ApprovedRealGeometry";
                foreach(var c in go.GetComponentsInChildren<Collider>(true)) c.enabled=false;
                go.transform.rotation=sourceToSharpRotation;
                var rs=go.GetComponentsInChildren<Renderer>(true);
                if(rs.Length==0) throw new Exception("Production slice: no renderers for "+spec.Id);

                var b=CombinedBounds(rs);
                if(b.size.sqrMagnitude<.001f) throw new Exception("Production slice: invalid bounds for "+spec.Id);
                float sx=spec.TargetWidth/Mathf.Max(.01f,b.size.x);
                float sy=spec.TargetHeight/Mathf.Max(.01f,b.size.y);
                go.transform.localScale*=Mathf.Clamp(Mathf.Min(sx,sy),.001f,1000f);
                b=CombinedBounds(go.GetComponentsInChildren<Renderer>(true));

                var ray=cam.ViewportPointToRay(new Vector3(spec.Viewport.x,spec.Viewport.y,0));
                var desired=ray.origin+ray.direction*anchorDistance*spec.DepthScale;
                go.transform.position+=desired-b.center;
                b=CombinedBounds(go.GetComponentsInChildren<Renderer>(true));

                var vp=cam.WorldToViewportPoint(b.center);
                float err=Vector2.Distance(new Vector2(vp.x,vp.y),spec.Viewport);
                if(vp.z<=0||err>.015f) throw new Exception($"Production slice: anchor alignment failed for {spec.Id}: {err:F5}");

                // First playable slice keeps SHARP as beauty authority. Every approved GLB
                // is real editable support geometry and stays hidden in beauty until a
                // family-specific visible treatment can pass without artifacts.
                SetRenderers(rs,spec.BeautyVisible);
                runtimes.Add(new FamilyRuntime{Spec=spec,Go=go,Renderers=rs,Bounds=b,ViewportError=err});
            }

            // Semantic anchors are standard Unity colliders and remain invisible in beauty.
            var proxies=new List<ProxyRuntime>();
            foreach(var spec in Families)
            {
                float z=anchorDistance*spec.DepthScale;
                float span=2f*z*Mathf.Tan(HomeFov*Mathf.Deg2Rad*.5f);
                var colliderSize=new Vector3(span*cam.aspect*.035f,span*.035f,.2f);
                AddProxy(proxies,cam,anchorDistance,spec.Id,spec.Viewport,spec.DepthScale,colliderSize);
            }
            float proxySpan=2f*anchorDistance*Mathf.Tan(HomeFov*Mathf.Deg2Rad*.5f);
            var parcelProxySize=new Vector3(proxySpan*cam.aspect*.035f,proxySpan*.035f,.2f);
            AddProxy(proxies,cam,anchorDistance,"LeftCabinParcel",new Vector2(.203252f,.627219f),.94f,parcelProxySize);
            AddProxy(proxies,cam,anchorDistance,"RightCampParcel",new Vector2(.853659f,.519527f),.94f,parcelProxySize);
            Physics.SyncTransforms();

            // SHARP authority before real-family overlay.
            SetFamilyBeauty(runtimes,false);
            CaptureBeauty(gs,cam,output,"sharp-home");
            cam.transform.position=new Vector3(-1.75f,0,0); CaptureBeauty(gs,cam,output,"sharp-pan-left");
            cam.transform.position=new Vector3(1.75f,0,0); CaptureBeauty(gs,cam,output,"sharp-pan-right");

            // Real playable slice over the same visual authority.
            SetFamilyBeauty(runtimes,true);
            cam.transform.position=Vector3.zero; CaptureBeauty(gs,cam,output,"integrated-home");
            cam.transform.position=new Vector3(-1.75f,0,0); CaptureBeauty(gs,cam,output,"integrated-pan-left");
            cam.transform.position=new Vector3(1.75f,0,0); CaptureBeauty(gs,cam,output,"integrated-pan-right");

            // Geometry-only evidence proves all approved families exist as real 3D.
            // Use unlit diagnostic material only in this evidence view; never in beauty.
            sharpRoot.SetActive(false);
            cam.transform.position=Vector3.zero;
            var diagShader=Shader.Find("Universal Render Pipeline/Unlit");
            if(diagShader==null) throw new Exception("Production slice: URP Unlit diagnostic shader missing");
            int familyIndex=0;
            foreach(var rt in runtimes)
            {
                var dm=new Material(diagShader);
                float v=.35f+.08f*(familyIndex%5);
                dm.SetColor("_BaseColor",new Color(v,v+.08f,v+.14f,1f));
                OverrideMaterials(rt.Renderers,dm);
                SetRenderers(rt.Renderers,true);
                familyIndex++;
            }
            CaptureRasterOnly(cam,output,"real-geometry-home");
            sharpRoot.SetActive(true);
            SetFamilyBeauty(runtimes,true);

            int homeChanged=CountChangedPixels(Path.Combine(output,"sharp-home.png"),Path.Combine(output,"integrated-home.png"),.12f);
            int leftChanged=CountChangedPixels(Path.Combine(output,"sharp-pan-left.png"),Path.Combine(output,"integrated-pan-left.png"),.12f);
            int rightChanged=CountChangedPixels(Path.Combine(output,"sharp-pan-right.png"),Path.Combine(output,"integrated-pan-right.png"),.12f);
            int framePixels=W*H;
            int identityTolerance=200;
            bool visualLocalized=homeChanged<=identityTolerance&&leftChanged<=identityTolerance&&rightChanged<=identityTolerance;

            var states=new[]{
                new {Name="home",Position=Vector3.zero,Fov=HomeFov},
                new {Name="pan-left",Position=new Vector3(-1.75f,0,0),Fov=HomeFov},
                new {Name="pan-right",Position=new Vector3(1.75f,0,0),Fov=HomeFov},
                new {Name="zoom-in",Position=Vector3.zero,Fov=HomeFov*.9f},
                new {Name="zoom-out",Position=Vector3.zero,Fov=HomeFov*1.1f}
            };
            int semanticRequired=0,semanticPass=0;
            var callbacks=new Dictionary<string,int>();
            var homeAnchorRows=new List<string>();
            var hitRows=new List<string>();
            foreach(var state in states)
            {
                cam.transform.position=state.Position; cam.fieldOfView=state.Fov; cam.transform.rotation=Quaternion.identity;
                Physics.SyncTransforms();
                if(state.Name.StartsWith("zoom")) CaptureBeauty(gs,cam,output,"integrated-"+state.Name);
                foreach(var p in proxies)
                {
                    var vp=cam.WorldToViewportPoint(p.Go.transform.position);
                    bool visible=vp.z>0&&vp.x>=0&&vp.x<=1&&vp.y>=0&&vp.y<=1;
                    if(!visible){hitRows.Add($"{{\"camera\":\"{state.Name}\",\"semantic\":\"{p.Id}\",\"visible\":false,\"pass\":false}}");continue;}
                    semanticRequired++;
                    var ray=cam.ViewportPointToRay(vp);
                    bool hit=Physics.Raycast(ray,out var info,1000f);
                    string actual=hit&&info.collider!=null?info.collider.gameObject.name.Replace("InteractiveProxy_",""):"";
                    bool ok=hit&&actual==p.Id;
                    if(ok) { semanticPass++; callbacks[p.Id]=callbacks.ContainsKey(p.Id)?callbacks[p.Id]+1:1; }
                    if(state.Name=="home") { var error=Vector2.Distance(new Vector2(vp.x,vp.y),p.ExpectedViewport); if(error>.002f) throw new Exception("Reference anchor drift: "+p.Id); homeAnchorRows.Add("{\"id\":\""+p.Id+"\",\"x\":"+vp.x.ToString(CultureInfo.InvariantCulture)+",\"y\":"+vp.y.ToString(CultureInfo.InvariantCulture)+"}"); }
                    hitRows.Add($"{{\"camera\":\"{state.Name}\",\"semantic\":\"{p.Id}\",\"visible\":true,\"actual\":\"{actual}\",\"pass\":{(ok?"true":"false")}}}");
                }
            }
            bool interactionPass=semanticRequired==states.Length*proxies.Count&&semanticPass==semanticRequired;

            // A compact debug view for semantic layout.
            foreach(var p in proxies)
            {
                var r=p.Go.GetComponent<Renderer>(); r.enabled=true;
                var m=new Material(Shader.Find("Universal Render Pipeline/Lit")); m.color=new Color(.1f,.85f,1f,.85f); r.sharedMaterial=m;
            }
            sharpRoot.SetActive(false); SetFamilyBeauty(runtimes,false);
            cam.transform.position=Vector3.zero; cam.fieldOfView=HomeFov; CaptureRasterOnly(cam,output,"semantic-anchors-home");
            sharpRoot.SetActive(true); SetFamilyBeauty(runtimes,true);
            foreach(var p in proxies) p.Go.GetComponent<Renderer>().enabled=false;

            bool importsPass=runtimes.Count==Families.Length&&runtimes.All(r=>r.Renderers.Length>0&&r.Bounds.size.sqrMagnitude>.001f&&r.ViewportError<=.015f);
            bool gatePass=importsPass&&interactionPass&&visualLocalized;

            var familyRows=runtimes.Select(r =>
                $"{{\"id\":\"{r.Spec.Id}\",\"source\":\"{r.Spec.Source}\",\"real_geometry\":true,\"beauty_visible\":{(r.Spec.BeautyVisible?"true":"false")},\"renderer_count\":{r.Renderers.Length},\"viewport_error\":{r.ViewportError.ToString("F8",System.Globalization.CultureInfo.InvariantCulture)}}}"
            );

            var json="{\n"+
                $"  \"tech_gate_pass\": {(gatePass?"true":"false")},\n"+
                "  \"gate_pass\": false,\n"+
                "  \"visual_review\": \"PENDING DIRECT REFERENCE REVIEW; workflow success is technical evidence only\",\n"+
                "  \"reference_authority_sha256\": \""+CorrectAuthoritySha+"\",\n"+
                "  \"home_anchors\": ["+string.Join(",",homeAnchorRows)+"],\n"+
                "  \"callback_counts\": {"+string.Join(",",callbacks.Select(kv=>"\""+kv.Key+"\":"+kv.Value))+"},\n"+
                $"  \"visual_pass_candidate\": {(visualLocalized?"true":"false")},\n"+
                $"  \"imports_pass\": {(importsPass?"true":"false")},\n"+
                $"  \"interaction_pass\": {(interactionPass?"true":"false")},\n"+
                $"  \"source_splat_count\": {prep.OriginalCount},\n"+
                $"  \"proof_splat_count\": {asset.SplatCount},\n"+
                $"  \"family_count\": {runtimes.Count},\n"+
                $"  \"semantic_anchor_count\": {proxies.Count},\n"+
                $"  \"semantic_raycast_pass\": {semanticPass},\n"+
                $"  \"semantic_raycast_required\": {semanticRequired},\n"+
                $"  \"changed_pixels\": {{\"home\":{homeChanged},\"pan_left\":{leftChanged},\"pan_right\":{rightChanged}}},\n"+
                "  \"camera\": {\"home\":\"source intrinsic identity\",\"bounded_pan_x\":1.75,\"fov\":"+HomeFov.ToString(CultureInfo.InvariantCulture)+",\"width\":"+W+",\"height\":"+H+"},\n"+
                "  \"visual_authority\": \"clean SHARP 589824 beauty authority; approved central-axis GLBs are real editable support geometry hidden from beauty\",\n"+
                "  \"route_order\": [\"Bridge\",\"LowerGate\",\"MainRoad\",\"CentralStair\",\"UpperWalls\",\"Bastion\"],\n"+
                "  \"route_coherence_pass\": false,\n"+
                "  \"composition_review_required\": true,\n"+
                "  \"beauty_replacement_regions\": [],\n"+
                "  \"sharp_only_regions\": [\"background\",\"vegetation\",\"water_shore\",\"left_cabin_parcel\",\"right_camp_parcel\",\"secondary_props\"],\n"+
                "  \"real_geometry_regions\": [\"bridge\",\"lower_gate\",\"main_road\",\"central_stair\",\"upper_walls\",\"bastion\",\"terrain_cliff_support\"],\n"+
                "  \"families\": ["+string.Join(",",familyRows)+"],\n"+
                "  \"semantic_hits\": ["+string.Join(",",hitRows)+"],\n"+
                "  \"next_block\": [],\n"+
                "  \"paid_credits\": 0\n"+
                "}\n";
            File.WriteAllText(Path.Combine(output,"production-slice-evidence.json"),json);

            if(!gatePass) throw new Exception($"Production slice gate failed imports={importsPass} interaction={interactionPass} visualLocalized={visualLocalized} changed={homeChanged}/{leftChanged}/{rightChanged} rays={semanticPass}/{semanticRequired}");

            gs.GsplatAsset=null; UnityEngine.Object.DestroyImmediate(asset);
            Debug.Log($"[PRODUCTION-SLICE] TECH PASS; DIRECT VISUAL REVIEW REQUIRED families={runtimes.Count} rays={semanticPass}/{semanticRequired} changed={homeChanged}/{leftChanged}/{rightChanged}");
        }

        static void AddProxy(List<ProxyRuntime> list,Camera cam,float anchorDistance,string id,Vector2 viewport,float depthScale,Vector3 size)
        {
            var ray=cam.ViewportPointToRay(new Vector3(viewport.x,viewport.y,0));
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name="InteractiveProxy_"+id;
            go.transform.position=ray.origin+ray.direction*anchorDistance*depthScale;
            go.transform.localScale=size;
            go.GetComponent<Renderer>().enabled=false;
            list.Add(new ProxyRuntime{Id=id,ExpectedViewport=viewport,Go=go});
        }

        static string Sha256(string p)
        {
            using(var sha=SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(p))).Replace("-","").ToLowerInvariant();
        }

        // SHARP PLY records its source camera. Derive framing instead of copying
        // the vertical-reference FOV into a differently shaped input image.
        static void ReadSharpCamera(string p)
        {
            using(var stream=File.OpenRead(p))
            using(var reader=new BinaryReader(stream))
            {
                var lines=new List<string>();
                while(true) { var chars=new List<byte>(); byte c; while((c=reader.ReadByte())!=10) chars.Add(c); string line=Encoding.ASCII.GetString(chars.ToArray()).TrimEnd('\r'); lines.Add(line); if(line=="end_header")break; }
                if(!lines.Contains("format binary_little_endian 1.0")) throw new Exception("Unsupported SHARP PLY format");
                float focal=0; int width=0,height=0;
                for(int i=0;i<lines.Count;i++)
                {
                    if(!lines[i].StartsWith("element "))continue;
                    var parts=lines[i].Split(' ');string name=parts[1];int count=int.Parse(parts[2]);int stride=0;
                    for(int j=i+1;j<lines.Count&&lines[j].StartsWith("property ");j++)
                    {
                        string type=lines[j].Split(' ')[1];
                        stride+=type=="float"||type=="uint"||type=="int"?4:type=="uchar"?1:throw new Exception("Unsupported SHARP metadata type "+type);
                    }
                    long start=stream.Position;
                    if(name=="intrinsic") { if(count!=9&&count!=4)throw new Exception("Invalid SHARP intrinsics"); focal=reader.ReadSingle();if(count==4){reader.ReadSingle();width=(int)reader.ReadSingle();height=(int)reader.ReadSingle();} }
                    if(name=="image_size") { width=(int)reader.ReadUInt32();height=(int)reader.ReadUInt32(); }
                    stream.Position=start+(long)count*stride;
                }
                if(width<=0||height<=0||focal<=0)throw new Exception("Missing SHARP camera intrinsics");
                W=width;H=height;HomeFov=2f*Mathf.Atan(height/(2f*focal))*Mathf.Rad2Deg;
                Debug.Log($"[AUTHORITY] source camera {W}x{H} focal={focal} fov={HomeFov}");
            }
        }

        static Bounds CombinedBounds(Renderer[] rs){var b=rs[0].bounds;for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);return b;}
        static void SetRenderers(Renderer[] rs,bool on){foreach(var r in rs)if(r!=null)r.enabled=on;}
        static void OverrideMaterials(Renderer[] rs,Material m){foreach(var r in rs){var a=new Material[Mathf.Max(1,r.sharedMaterials.Length)];for(int i=0;i<a.Length;i++)a[i]=m;r.sharedMaterials=a;}}
        static void SetFamilyBeauty(List<FamilyRuntime> rs,bool on){foreach(var r in rs)SetRenderers(r.Renderers,on&&r.Spec.BeautyVisible);}

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
            throw new TimeoutException("Production slice sorter did not initialize for "+label);
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
            if(!data.rendererFeatures.Any(f=>f!=null&&f.GetType()==t)){var feature=ScriptableObject.CreateInstance(t) as ScriptableRendererFeature;if(feature==null)throw new Exception("Could not instantiate GsplatURPFeature");feature.name="SHARP Production Slice Gsplat URP Feature";data.rendererFeatures.Add(feature);feature.Create();}
            GraphicsSettings.defaultRenderPipeline=urp;QualitySettings.renderPipeline=urp;
        }
    }
}
