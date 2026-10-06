using System;
using System.Collections.Generic;
using System.Globalization;
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
    public static class ValoriaCameraFirstDepthShellGateV1
    {
        const string Folder="ValoriaCameraFirstDepthShellGateV1Captures";
        const string RootName="Valoria · Camera-First 2.5D Depth Shell v1";
        const int TargetWidth=1536, TargetHeight=1024;
        const int CropX0=300,CropY0=420,CropX1=1000,CropY1=960;
        const float FullVerticalSpan=48f,Yaw=20f,Pitch=35f,Distance=110f;

        sealed class LayerState
        {
            public string Name;
            public GameObject Go;
            public Vector3 BasePosition;
            public float Compensation;
        }

        static readonly List<LayerState> Layers=new();

        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;
            Directory.CreateDirectory(Folder);
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);

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
            var camera=Camera.main??throw new Exception("Valoria main camera missing.");

            ConfigureCamera(camera,Vector3.zero,FullVerticalSpan);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();
            Save(camera,Folder+"/before-home-1536x1024.png",1536,1024);

            var shell=BuildShell(camera);
            Physics.SyncTransforms();
            var after=ValoriaVisualFormulaGate.CollisionSignature();
            if(after!=baseline)throw new Exception("Depth shell changed gameplay collider/hotspot signature.");
            if(shell.GetComponentsInChildren<Collider>(true).Length!=0)throw new Exception("Depth shell must have zero colliders.");
            if(shell.GetComponentsInChildren<WorldHotspot>(true).Length!=0)throw new Exception("Depth shell must have zero hotspots.");

            // HOME: exact registration, no artificial differential parallax.
            ResetParallax();
            ConfigureCamera(camera,Vector3.zero,FullVerticalSpan);
            Save(camera,Folder+"/after-home-1536x1024.png",1536,1024);

            // Golden close is the main visual inspection pose. Keep target registration.
            var golden=new Vector3(3f,0f,-21.5f);
            ResetParallax();
            ConfigureCamera(camera,golden,22f);
            Save(camera,Folder+"/after-golden-close-1280x720.png",1280,720);

            // Bounded local pan around Golden close. Camera keeps official yaw/pitch/orthographic contract.
            float pan=4.0f;
            var right=camera.transform.right;
            ConfigureCamera(camera,golden-right*pan,22f);
            ApplyParallax(-pan,camera);
            Save(camera,Folder+"/after-pan-left-1280x720.png",1280,720);

            ConfigureCamera(camera,golden+right*pan,22f);
            ApplyParallax(+pan,camera);
            Save(camera,Folder+"/after-pan-right-1280x720.png",1280,720);

            // Zoom-out around Golden center: no differential parallax, validates finite organic boundary.
            ResetParallax();
            ConfigureCamera(camera,golden,30f);
            Save(camera,Folder+"/after-zoom-out-1280x720.png",1280,720);

            // Mobile baseline.
            ResetParallax();
            ConfigureCamera(camera,Vector3.zero,FullVerticalSpan);
            Save(camera,Folder+"/after-mobile-landscape-1280x720.png",1280,720);

            // Relevant portrait entry view from canonical request.
            ResetParallax();
            ConfigureCamera(camera,new Vector3(1.9469909754850867f,0f,-15.902420247865848f),36f);
            Save(camera,Folder+"/after-portrait-entry-390x844.png",390,844);

            // Shell-only Golden close for direct structural read.
            ResetParallax();
            ConfigureCamera(camera,golden,22f);
            var shellRenderers=new HashSet<Renderer>(shell.GetComponentsInChildren<Renderer>(true));
            var changed=new List<Renderer>();
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||shellRenderers.Contains(r)||!r.enabled)continue;
                r.enabled=false;changed.Add(r);
            }
            Save(camera,Folder+"/shell-only-golden-close-1280x720.png",1280,720);
            foreach(var r in changed)if(r!=null)r.enabled=true;

            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new Exception("Depth-shell capture changed gameplay signature.");

            var report=
                "{\n"+
                "  \"authority\": \"docs/evidence/valoria-art-production-reset-v1/canonical-target.jpeg\",\n"+
                "  \"source_family\": \"art-source/valoria/lookdev/golden-slice-v1/camera-first-depth-shell-v1\",\n"+
                "  \"camera\": \"ORTHOGRAPHIC yaw20 pitch35\",\n"+
                "  \"home_span\": 48,\n"+
                "  \"golden_close_span\": 22,\n"+
                "  \"local_pan_world\": 4.0,\n"+
                "  \"layer_count\": "+Layers.Count+",\n"+
                "  \"parallax_mode\": \"depth-dependent compensation on fixed-orientation orthographic camera\",\n"+
                "  \"gate_thickness_mode\": \"co-registered dark back card with lower compensation\",\n"+
                "  \"bridge_thickness_mode\": \"co-registered dark back card with lower compensation\",\n"+
                "  \"far_edge_mode\": \"organic feathered support over inherited 3D world\",\n"+
                "  \"gameplay_signature_preserved\": true,\n"+
                "  \"shell_colliders\": 0,\n"+
                "  \"shell_hotspots\": 0,\n"+
                "  \"production_scene_saved\": false,\n"+
                "  \"paid_credits\": 0,\n"+
                "  \"visual_review_required\": true\n"+
                "}\n";
            File.WriteAllText(Folder+"/evidence.json",report);
            Debug.Log("CAMERA_FIRST_DEPTH_SHELL_TECH_PASS; VISUAL_REVIEW_REQUIRED");
            EditorApplication.Exit(0);
        }

        static GameObject BuildShell(Camera camera)
        {
            var old=GameObject.Find(RootName);if(old!=null)Object.DestroyImmediate(old);
            Layers.Clear();
            var root=new GameObject(RootName);
            float screenX=((CropX0+CropX1)*.5f/TargetWidth-.5f)*(FullVerticalSpan*((float)TargetWidth/TargetHeight));
            float screenY=(.5f-(CropY0+CropY1)*.5f/TargetHeight)*FullVerticalSpan;
            float w=CropWorldWidth(),h=CropWorldHeight();
            var projectRoot=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../.."));
            var src=Path.Combine(projectRoot,"art-source","valoria","lookdev","golden-slice-v1","camera-first-depth-shell-v1");

            AddLayer(root,camera,src,"far-environment",screenX,screenY,w,h,-12.0f,0.34f,true);
            AddLayer(root,camera,src,"cliff-ground-shore",screenX,screenY,w,h,-13.4f,0.22f,false);
            AddLayer(root,camera,src,"vegetation-mid",screenX,screenY,w,h,-14.6f,0.16f,false);
            AddLayer(root,camera,src,"gate-back",screenX,screenY,w,h,-15.5f,0.12f,false);
            AddLayer(root,camera,src,"gate-front",screenX,screenY,w,h,-15.8f,0.07f,false);
            AddLayer(root,camera,src,"bridge-back",screenX,screenY,w,h,-16.6f,0.07f,false);
            AddLayer(root,camera,src,"bridge-front",screenX,screenY,w,h,-16.9f,0.02f,false);
            AddLayer(root,camera,src,"vegetation-foreground",screenX,screenY,w,h,-17.8f,-0.04f,false);
            return root;
        }

        static void AddLayer(GameObject root,Camera camera,string src,string name,float screenX,float screenY,float w,float h,float forwardOffset,float compensation,bool feather)
        {
            string path=Path.Combine(src,name+".png");
            if(!File.Exists(path))throw new Exception("Missing depth-shell layer: "+path);
            var tex=LoadTexture(path,name);
            var mat=LayerMaterial(tex,name,feather);
            var pos=camera.transform.right*screenX+camera.transform.up*screenY+camera.transform.forward*forwardOffset;
            var go=Quad(name,pos,camera.transform.right,camera.transform.up,w,h,mat);
            go.transform.SetParent(root.transform,true);
            Layers.Add(new LayerState{Name=name,Go=go,BasePosition=pos,Compensation=compensation});
        }

        static void ResetParallax()
        {
            foreach(var l in Layers)if(l.Go!=null)l.Go.transform.position=l.BasePosition;
        }

        static void ApplyParallax(float panWorld,Camera camera)
        {
            foreach(var l in Layers)
            {
                // Move in the same direction as camera pan; far layers compensate more and therefore
                // move less on screen, while near layers retain stronger apparent motion.
                l.Go.transform.position=l.BasePosition+camera.transform.right*(panWorld*l.Compensation);
            }
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
            tex.alphaIsTransparency=true;return tex;
        }

        static Material LayerMaterial(Texture2D tex,string name,bool feather)
        {
            var shader=Shader.Find("Universal Render Pipeline/Unlit");
            if(shader==null)throw new Exception("URP Unlit shader missing.");
            var mat=new Material(shader){name="DepthShell_"+name};
            mat.SetTexture("_BaseMap",tex);mat.SetColor("_BaseColor",Color.white);
            if(feather)
            {
                if(mat.HasProperty("_Surface"))mat.SetFloat("_Surface",1f);
                if(mat.HasProperty("_Blend"))mat.SetFloat("_Blend",0f);
                if(mat.HasProperty("_AlphaClip"))mat.SetFloat("_AlphaClip",0f);
                if(mat.HasProperty("_ZWrite"))mat.SetFloat("_ZWrite",0f);
                if(mat.HasProperty("_SrcBlend"))mat.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);
                if(mat.HasProperty("_DstBlend"))mat.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);
                if(mat.HasProperty("_SrcBlendAlpha"))mat.SetFloat("_SrcBlendAlpha",(float)BlendMode.One);
                if(mat.HasProperty("_DstBlendAlpha"))mat.SetFloat("_DstBlendAlpha",(float)BlendMode.OneMinusSrcAlpha);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.SetOverrideTag("RenderType","Transparent");
                mat.renderQueue=(int)RenderQueue.Transparent;
            }
            else
            {
                if(mat.HasProperty("_AlphaClip"))mat.SetFloat("_AlphaClip",1f);
                if(mat.HasProperty("_Cutoff"))mat.SetFloat("_Cutoff",.5f);
                if(mat.HasProperty("_Surface"))mat.SetFloat("_Surface",0f);
                if(mat.HasProperty("_ZWrite"))mat.SetFloat("_ZWrite",1f);
                mat.EnableKeyword("_ALPHATEST_ON");
                mat.SetOverrideTag("RenderType","TransparentCutout");
                mat.renderQueue=(int)RenderQueue.AlphaTest;
            }
            return mat;
        }

        static GameObject Quad(string name,Vector3 center,Vector3 right,Vector3 up,float width,float height,Material mat)
        {
            var go=new GameObject("DepthShell · "+name);
            var mf=go.AddComponent<MeshFilter>();var mr=go.AddComponent<MeshRenderer>();
            var r=right.normalized*width*.5f;var u=up.normalized*height*.5f;
            var mesh=new Mesh{name=name+"Mesh"};
            mesh.vertices=new[]{center-r-u,center+r-u,center+r+u,center-r+u};
            mesh.uv=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)};
            mesh.triangles=new[]{0,2,1,0,3,2};mesh.RecalculateNormals();mesh.RecalculateBounds();
            mf.sharedMesh=mesh;mr.sharedMaterial=mat;
            mr.shadowCastingMode=ShadowCastingMode.Off;mr.receiveShadows=false;
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
            finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
        }
    }
}
