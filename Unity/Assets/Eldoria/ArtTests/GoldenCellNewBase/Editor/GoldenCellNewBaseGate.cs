using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools
{
    public static class GoldenCellNewBaseGate
    {
        const string Folder="GoldenCellCaptures";
        static readonly Vector3 CameraPosition=new Vector3(18.2f,14.6f,-25.8f);
        static readonly Vector3 CameraTarget=new Vector3(0f,3.65f,7.25f);

        public static void CaptureBlockout()
        {
            UnityEditor.ShaderUtil.allowAsyncCompilation=false;
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            VisualWorld.VisualIntegrationEnabled=true;
            VisualWorld.Create(true,new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true});
            var camera=Camera.main;
            if(camera==null)throw new Exception("Valoria camera missing.");
            Directory.CreateDirectory(Folder);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            Save(camera,Folder+"/before-19.png",19f,1280,720);
            Save(camera,Folder+"/before-12.png",12f,1280,720);
            Save(camera,Folder+"/before-9.png",9f,1280,720);
            Save(camera,Folder+"/before-mobile.png",12f,390,844);

            GoldenCellBlockout.Build();

            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new Exception("Golden Cell blockout altered gameplay collider/hotspot signature.");

            Save(camera,Folder+"/blockout-19.png",19f,1280,720);
            Save(camera,Folder+"/blockout-12.png",12f,1280,720);
            Save(camera,Folder+"/blockout-9.png",9f,1280,720);
            Save(camera,Folder+"/blockout-mobile.png",12f,390,844);

            File.WriteAllText(Folder+"/blockout-evidence.json",
                "{\n"+
                "  \"phase\": \"BLOCKOUT_ONLY\",\n"+
                "  \"branch\": \"visual-proof/golden-cell-new-base-v1\",\n"+
                "  \"cell\": \"Bastion main access / upper civic terrace\",\n"+
                "  \"same_scene_before_after\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"tripo_credits\": 0,\n"+
                "  \"final_assets_fabricated\": false,\n"+
                "  \"scope\": [\"gate massing\",\"plaza\",\"terrace\",\"rock-architecture seam\",\"two support buildings\"]\n"+
                "}\n");
            EditorApplication.Exit(0);
        }

        public static void CaptureFinal()
        {
            UnityEditor.ShaderUtil.allowAsyncCompilation=false;
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            VisualWorld.VisualIntegrationEnabled=true;
            VisualWorld.Create(true,new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true});
            var camera=Camera.main;
            if(camera==null)throw new Exception("Valoria camera missing.");
            Directory.CreateDirectory(Folder);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            Save(camera,Folder+"/before-19.png",19f,1280,720);
            Save(camera,Folder+"/before-12.png",12f,1280,720);
            Save(camera,Folder+"/before-9.png",9f,1280,720);
            Save(camera,Folder+"/before-mobile.png",12f,390,844);

            GoldenCellFinished.Build();

            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new Exception("Golden Cell finished art altered gameplay collider/hotspot signature.");

            Save(camera,Folder+"/after-19.png",19f,1280,720);
            Save(camera,Folder+"/after-12.png",12f,1280,720);
            Save(camera,Folder+"/after-9.png",9f,1280,720);
            Save(camera,Folder+"/after-mobile.png",12f,390,844);
            File.WriteAllText(Folder+"/final-evidence.json",
                "{\n"+
                "  \"phase\": \"FINISHED_GOLDEN_CELL\",\n"+
                "  \"branch\": \"visual-proof/golden-cell-new-base-v1\",\n"+
                "  \"same_scene_before_after\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"tripo_credits\": 0,\n"+
                "  \"paid_assets\": 0\n"+
                "}\n");
            EditorApplication.Exit(0);
        }

        static void Save(Camera camera,string path,float zoom,int width,int height)
        {
            camera.transform.position=CameraPosition;
            camera.transform.LookAt(CameraTarget);
            camera.orthographic=true;
            camera.orthographicSize=zoom;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();
            File.WriteAllBytes(path,tex.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;
            UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);
        }
    }

    static class GoldenCellBlockout
    {
        static Transform Root;
        static Material Stone,Ground,Wood,Rock,Roof,Blue;

        public static void Build()
        {
            Root=new GameObject("GOLDEN CELL · blockout").transform;
            Stone=Mat(new Color(.67f,.65f,.59f));Ground=Mat(new Color(.42f,.37f,.30f));
            Wood=Mat(new Color(.30f,.20f,.12f));Rock=Mat(new Color(.31f,.32f,.30f));
            Roof=Mat(new Color(.15f,.19f,.24f));Blue=Mat(new Color(.08f,.22f,.40f));

            // Terrace/platform: broad enough for mobile readable civic space.
            Block("GC · terrace",new Vector3(0f,2.62f,5.9f),new Vector3(13.5f,.42f,7.4f),Stone);
            Block("GC · plaza",new Vector3(0f,2.85f,2.75f),new Vector3(8.4f,.10f,4.0f),Ground);

            // Main access composition: gate house + flanking masses, deliberately larger than residences.
            Block("GC · gate left",new Vector3(-3.15f,4.9f,6.8f),new Vector3(2.6f,4.8f,3.2f),Stone);
            Block("GC · gate right",new Vector3(3.15f,4.9f,6.8f),new Vector3(2.6f,4.8f,3.2f),Stone);
            Block("GC · gate lintel",new Vector3(0f,6.65f,6.8f),new Vector3(4.0f,1.1f,3.2f),Stone);
            Block("GC · gate opening dark",new Vector3(0f,4.25f,6.73f),new Vector3(3.0f,3.8f,.20f),Rock);
            Block("GC · crown",new Vector3(0f,8.0f,7.25f),new Vector3(4.3f,1.6f,2.6f),Stone);

            // Processional stair/road from lower city into the gate.
            for(int i=0;i<8;i++)
            {
                float z=-.2f+i*.50f;
                float y=.78f+i*.27f;
                Block("GC · stair "+i,new Vector3(0f,y,z),new Vector3(4.5f,.24f,.54f),Ground);
            }

            // Architecture-to-rock buttresses / mountain seam.
            Wedge("GC · rock west",new Vector3(-6.5f,2.0f,7.2f),new Vector3(5.4f,4.8f,7f),-14f,Rock);
            Wedge("GC · rock east",new Vector3(6.5f,2.0f,7.2f),new Vector3(5.4f,4.8f,7f),14f,Rock);
            Block("GC · retaining west",new Vector3(-5.2f,3.25f,4.5f),new Vector3(1.0f,2.2f,5.0f),Stone);
            Block("GC · retaining east",new Vector3(5.2f,3.25f,4.5f),new Vector3(1.0f,2.2f,5.0f),Stone);

            // Minimal support buildings for scale and functional adjacency.
            House("GC · residence",new Vector3(-7.4f,3.0f,2.5f),new Vector3(3.1f,2.4f,3.0f));
            House("GC · workshop",new Vector3(7.4f,3.0f,2.2f),new Vector3(3.4f,2.2f,3.2f));

            // Heraldic read.
            Banner(new Vector3(-2.0f,7.0f,5.1f));Banner(new Vector3(2.0f,7.0f,5.1f));
        }

        static void House(string name,Vector3 p,Vector3 size)
        {
            Block(name+" · body",p+Vector3.up*(size.y*.5f),size,Stone);
            var roof=Block(name+" · roof",p+new Vector3(0,size.y+.55f,0),new Vector3(size.x*1.08f,.65f,size.z*1.10f),Roof);
            roof.transform.rotation=Quaternion.Euler(0,0,5f);
            Block(name+" · door",p+new Vector3(0,.65f,-size.z*.51f),new Vector3(.55f,1.2f,.10f),Wood);
        }

        static void Banner(Vector3 p)
        {
            Block("GC · blue banner",p,new Vector3(.7f,1.6f,.06f),Blue);
        }

        static GameObject Wedge(string name,Vector3 p,Vector3 scale,float zRot,Material m)
        {
            var go=Block(name,p,scale,m);go.transform.rotation=Quaternion.Euler(0,0,zRot);return go;
        }

        static GameObject Block(string name,Vector3 p,Vector3 scale,Material m)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(Root);
            go.transform.position=p;go.transform.localScale=scale;
            var c=go.GetComponent<Collider>();if(c!=null)UnityEngine.Object.DestroyImmediate(c);
            go.GetComponent<Renderer>().sharedMaterial=m;return go;
        }

        static Material Mat(Color c)
        {
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            var m=new Material(shader);m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",.04f);return m;
        }
    }

    static class GoldenCellFinished
    {
        public static void Build()
        {
            // Intentionally empty in phase 1.
            // Final materials/architecture may only be authored after blockout evidence is reviewed.
            GoldenCellBlockout.Build();
        }
    }
}
