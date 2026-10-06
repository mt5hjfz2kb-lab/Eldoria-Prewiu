using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace Eldoria.EditorTools
{
    public static class ValoriaCameraFirstResetTargetShellGateV1
    {
        const string Folder="ValoriaCameraFirstResetTargetShellGateV1Captures";
        const string RootName="Valoria · Camera-First Reset Target Shell v1";

        static readonly int TargetWidth=1536;
        static readonly int TargetHeight=1024;
        static readonly int CropX0=300;
        static readonly int CropY0=420;
        static readonly int CropX1=1000;
        static readonly int CropY1=960;
        static readonly float FullVerticalSpan=48f;
        static readonly float Yaw=20f;
        static readonly float Pitch=35f;
        static readonly float Distance=110f;

        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;
            Directory.CreateDirectory(Folder);
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);

            // Stable isolated runtime world. No production scene is opened or saved.
            ProductionVisualIntegration.ResetVisualCachesForGate();
            ProductionVisualIntegration.StoneArchitectureEnabled=true;
            ProductionVisualIntegration.TerrainTerraceEnabled=true;
            ProductionVisualIntegration.SurfaceCellEnabled=false;
            ProductionVisualIntegration.ProductionCellEnabled=false;
            ProductionVisualIntegration.CoherentCastleProofEnabled=false;
            ProductionVisualIntegration.SlavicDistrictProofEnabled=false;
            ProductionVisualIntegration.CompactFootprintReframeEnabled=true;
            AssetVisualUpliftPassV1.Enabled=false;
            AssetLibraryReprocessingPassV1.Enabled=true;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);
            var camera=Camera.main;
            if(camera==null)throw new Exception("Valoria main camera missing.");

            ConfigureCamera(camera,Vector3.zero,FullVerticalSpan);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            Save(camera,Folder+"/before-integrated-1536x1024.png",TargetWidth,TargetHeight);

            var shell=BuildShell(camera);
            Physics.SyncTransforms();
            var afterSignature=ValoriaVisualFormulaGate.CollisionSignature();
            if(afterSignature!=baseline)throw new Exception("Camera-first shell changed gameplay collider/hotspot signature.");
            if(shell.GetComponentsInChildren<Collider>(true).Length!=0)throw new Exception("Visual shell must have zero colliders.");
            if(shell.GetComponentsInChildren<WorldHotspot>(true).Length!=0)throw new Exception("Visual shell must have zero hotspots.");

            // Integrated base: the rectangle boundary remains visible if it does not blend with the real world.
            ConfigureCamera(camera,Vector3.zero,FullVerticalSpan);
            Save(camera,Folder+"/after-integrated-base-1536x1024.png",TargetWidth,TargetHeight);

            // Shell-only capture verifies exact Unity-side image reconstruction independently of the inherited world.
            var allRenderers=new List<Renderer>(Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None));
            var shellRenderers=new HashSet<Renderer>(shell.GetComponentsInChildren<Renderer>(true));
            var changed=new List<Renderer>();
            foreach(var r in allRenderers)
            {
                if(r==null||shellRenderers.Contains(r)||!r.enabled)continue;
                r.enabled=false;changed.Add(r);
            }
            Save(camera,Folder+"/shell-only-base-1536x1024.png",TargetWidth,TargetHeight);
            foreach(var r in changed)if(r!=null)r.enabled=true;

            // Actual locked Golden-close camera from current request.
            ConfigureCamera(camera,new Vector3(3f,0f,-21.5f),22f);
            Save(camera,Folder+"/after-golden-close-1280x720.png",1280,720);

            // Mobile landscape at the baseline camera.
            ConfigureCamera(camera,Vector3.zero,FullVerticalSpan);
            Save(camera,Folder+"/after-mobile-landscape-1280x720.png",1280,720);

            // Dynamic depth tests: no collider is kept on either probe.
            var frontProbe=Probe("FrontDepthProbe",camera,Color.magenta,-20f);
            Save(camera,Folder+"/depth-probe-front.png",TargetWidth,TargetHeight);
            Object.DestroyImmediate(frontProbe);

            var middleProbe=Probe("MiddleDepthProbe",camera,Color.cyan,-15f);
            Save(camera,Folder+"/depth-probe-middle.png",TargetWidth,TargetHeight);
            Object.DestroyImmediate(middleProbe);

            Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new Exception("Depth probes or shell changed gameplay signature.");

            var report=
                "{\n"+
                "  \"authority\": \"docs/evidence/valoria-art-production-reset-v1/canonical-target.jpeg\",\n"+
                "  \"authority_sha256\": \"8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689\",\n"+
                "  \"shell_source\": \"art-source/valoria/lookdev/golden-slice-v1/camera-first-reset-target-v1/layer-0..3.png\",\n"+
                "  \"camera\": \"ORTHOGRAPHIC yaw20 pitch35\",\n"+
                "  \"base_vertical_span\": 48,\n"+
                "  \"golden_close_vertical_span\": 22,\n"+
                "  \"crop_pixels\": [300,420,1000,960],\n"+
                "  \"crop_world_width\": "+CropWorldWidth().ToString("F6",System.Globalization.CultureInfo.InvariantCulture)+",\n"+
                "  \"crop_world_height\": "+CropWorldHeight().ToString("F6",System.Globalization.CultureInfo.InvariantCulture)+",\n"+
                "  \"gameplay_signature_preserved\": true,\n"+
                "  \"shell_colliders\": 0,\n"+
                "  \"shell_hotspots\": 0,\n"+
                "  \"production_scene_opened\": false,\n"+
                "  \"production_scene_saved\": false,\n"+
                "  \"paid_credits\": 0,\n"+
                "  \"tripo_credits\": 0,\n"+
                "  \"visual_review_required\": true\n"+
                "}\n";
            File.WriteAllText(Folder+"/evidence.json",report);
            Debug.Log("CAMERA_FIRST_RESET_TARGET_UNITY_TECH_PASS; VISUAL_REVIEW_REQUIRED");
            EditorApplication.Exit(0);
        }

        static void ConfigureCamera(Camera camera,Vector3 center,float verticalSpan)
        {
            float pitch=Pitch*Mathf.Deg2Rad,yaw=Yaw*Mathf.Deg2Rad;
            var vector=new Vector3(Mathf.Sin(yaw)*Mathf.Cos(pitch),Mathf.Sin(pitch),-Mathf.Cos(yaw)*Mathf.Cos(pitch));
            camera.transform.position=center+vector*Distance;
            camera.transform.LookAt(center);
            camera.orthographic=true;
            camera.orthographicSize=verticalSpan*.5f;
            camera.nearClipPlane=.1f;
            camera.farClipPlane=300f;
        }

        static GameObject BuildShell(Camera camera)
        {
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            var root=new GameObject(RootName);

            float screenX=((CropX0+CropX1)*.5f/TargetWidth-.5f)*(FullVerticalSpan*((float)TargetWidth/TargetHeight));
            float screenY=(.5f-(CropY0+CropY1)*.5f/TargetHeight)*FullVerticalSpan;
            float w=CropWorldWidth(),h=CropWorldHeight();

            // All planes are in front of the inherited scene, but retain depth ordering within the shell.
            // camera.forward points from camera into the world; negative offsets move toward the camera.
            float[] depthOffsets={-18f,-16f,-14f,-12f};
            var projectRoot=Path.GetFullPath(Path.Combine(Application.dataPath,"../.."));
            for(int band=0;band<4;band++)
            {
                string path=Path.Combine(projectRoot,"art-source","valoria","lookdev","golden-slice-v1","camera-first-reset-target-v1","layer-"+band+".png");
                if(!File.Exists(path))throw new Exception("Missing reset-target layer: "+path);
                var tex=LoadTexture(path,"ResetTargetLayer"+band);
                var mat=LayerMaterial(tex,band);
                var center=camera.transform.right*screenX+camera.transform.up*screenY+camera.transform.forward*depthOffsets[band];
                var go=Quad("ResetTargetDepthLayer"+band,center,camera.transform.right,camera.transform.up,w,h,mat);
                go.transform.SetParent(root.transform,true);
            }
            return root;
        }

        static float CropWorldWidth()
        {
            float fullWidth=FullVerticalSpan*((float)TargetWidth/TargetHeight);
            return (CropX1-CropX0)/(float)TargetWidth*fullWidth;
        }

        static float CropWorldHeight()=> (CropY1-CropY0)/(float)TargetHeight*FullVerticalSpan;

        static Texture2D LoadTexture(string path,string name)
        {
            var tex=new Texture2D(2,2,TextureFormat.RGBA32,false,false){name=name,wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
            if(!ImageConversion.LoadImage(tex,File.ReadAllBytes(path),false))throw new Exception("LoadImage failed: "+path);
            tex.alphaIsTransparency=true;
            return tex;
        }

        static Material LayerMaterial(Texture2D tex,int band)
        {
            var shader=Shader.Find("Universal Render Pipeline/Unlit");
            if(shader==null)throw new Exception("URP Unlit shader missing.");
            var mat=new Material(shader){name="ResetTargetLayerMat"+band};
            mat.SetTexture("_BaseMap",tex);
            mat.SetColor("_BaseColor",Color.white);
            if(mat.HasProperty("_AlphaClip"))mat.SetFloat("_AlphaClip",1f);
            if(mat.HasProperty("_Cutoff"))mat.SetFloat("_Cutoff",.5f);
            if(mat.HasProperty("_Surface"))mat.SetFloat("_Surface",0f);
            if(mat.HasProperty("_ZWrite"))mat.SetFloat("_ZWrite",1f);
            mat.EnableKeyword("_ALPHATEST_ON");
            mat.SetOverrideTag("RenderType","TransparentCutout");
            mat.renderQueue=(int)RenderQueue.AlphaTest;
            return mat;
        }

        static GameObject Quad(string name,Vector3 center,Vector3 right,Vector3 up,float width,float height,Material mat)
        {
            var go=new GameObject(name);
            var mf=go.AddComponent<MeshFilter>();
            var mr=go.AddComponent<MeshRenderer>();
            var r=right.normalized*width*.5f;
            var u=up.normalized*height*.5f;
            var mesh=new Mesh{name=name+"Mesh"};
            mesh.vertices=new[]{center-r-u,center+r-u,center+r+u,center-r+u};
            mesh.uv=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)};
            mesh.triangles=new[]{0,2,1,0,3,2};
            mesh.RecalculateNormals();mesh.RecalculateBounds();
            mf.sharedMesh=mesh;mr.sharedMaterial=mat;
            mr.shadowCastingMode=ShadowCastingMode.Off;mr.receiveShadows=false;
            return go;
        }

        static GameObject Probe(string name,Camera camera,Color color,float forwardOffset)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);go.name=name;
            var collider=go.GetComponent<Collider>();if(collider!=null)Object.DestroyImmediate(collider);
            go.transform.localScale=Vector3.one*2.5f;
            // Probe sits at the screen center of the Golden crop, but at a chosen shell depth.
            float screenX=((CropX0+CropX1)*.5f/TargetWidth-.5f)*(FullVerticalSpan*((float)TargetWidth/TargetHeight));
            float screenY=(.5f-(CropY0+CropY1)*.5f/TargetHeight)*FullVerticalSpan;
            go.transform.position=camera.transform.right*screenX+camera.transform.up*screenY+camera.transform.forward*forwardOffset;
            var shader=Shader.Find("Universal Render Pipeline/Unlit");
            var mat=new Material(shader);mat.SetColor("_BaseColor",color);
            go.GetComponent<Renderer>().sharedMaterial=mat;
            return go;
        }

        static void Save(Camera c,string path,int w,int h)
        {
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){antiAliasing=1};
            var prev=RenderTexture.active;
            try
            {
                c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;
                var im=new Texture2D(w,h,TextureFormat.RGB24,false);
                im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();
                File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);
            }
            finally
            {
                c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);
            }
        }
    }
}
