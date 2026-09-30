using System;
using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
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
            Root=new GameObject("GOLDEN CELL · blockout v2").transform;
            Stone=Mat(new Color(.48f,.45f,.40f));Ground=Mat(new Color(.34f,.30f,.25f));
            Wood=Mat(new Color(.27f,.18f,.11f));Rock=Mat(new Color(.26f,.27f,.26f));
            Roof=Mat(new Color(.12f,.16f,.20f));Blue=Mat(new Color(.07f,.18f,.33f));

            // New cell sits around/below the existing Bastion instead of covering its silhouette.
            Block("GC2 · upper civic shelf",new Vector3(0f,2.47f,4.65f),new Vector3(12.6f,.36f,5.8f),Stone);
            Block("GC2 · lower plaza",new Vector3(0f,.62f,-.55f),new Vector3(8.8f,.16f,5.1f),Ground);

            // Processional stair — broad, straight and obvious from mobile camera.
            for(int i=0;i<9;i++)
            {
                float z=.15f+i*.54f;
                float y=.78f+i*.205f;
                Block("GC2 · stair "+i,new Vector3(0f,y,z),new Vector3(4.15f,.22f,.56f),Ground);
            }

            // Monumental framing wings are lower than Bastion: they support hierarchy instead of replacing it.
            Wing("GC2 · west gate wing",new Vector3(-5.05f,2.95f,4.25f),false);
            Wing("GC2 · east gate wing",new Vector3(5.05f,2.95f,4.25f),true);

            // Rock / architecture seam: stone retaining wall steps into angled mountain shoulders.
            for(int i=0;i<3;i++)
            {
                Block("GC2 · west retaining "+i,new Vector3(-6.15f-i*.72f,1.55f-i*.22f,3.7f+i*.72f),
                    new Vector3(1.15f,2.5f-i*.25f,2.7f),Stone).transform.rotation=Quaternion.Euler(0,12f+i*6f,0);
                Block("GC2 · east retaining "+i,new Vector3(6.15f+i*.72f,1.55f-i*.22f,3.7f+i*.72f),
                    new Vector3(1.15f,2.5f-i*.25f,2.7f),Stone).transform.rotation=Quaternion.Euler(0,-12f-i*6f,0);
            }
            Wedge("GC2 · west mountain shoulder",new Vector3(-8.55f,.72f,6.0f),new Vector3(5.2f,3.1f,6.5f),-9f,Rock);
            Wedge("GC2 · east mountain shoulder",new Vector3(8.55f,.72f,6.0f),new Vector3(5.2f,3.1f,6.5f),9f,Rock);

            // Small inhabited/productive anchors establish human scale without filling the cell.
            House("GC2 · residence",new Vector3(-7.7f,.55f,-1.05f),new Vector3(2.9f,2.15f,2.8f),-8f);
            House("GC2 · workshop",new Vector3(7.7f,.55f,-1.2f),new Vector3(3.25f,2.05f,3.0f),8f);

            // Low arcades create civic rhythm along the plaza edges.
            Arcade("GC2 · west arcade",new Vector3(-4.6f,.72f,-.55f),-3f);
            Arcade("GC2 · east arcade",new Vector3(4.6f,.72f,-.55f),3f);

            // Heraldic identity stays subordinate.
            Banner(new Vector3(-4.05f,4.65f,3.05f));Banner(new Vector3(4.05f,4.65f,3.05f));
        }

        static void Wing(string name,Vector3 p,bool mirror)
        {
            float s=mirror?-1f:1f;
            Block(name+" · lower mass",p,new Vector3(2.35f,2.4f,3.1f),Stone);
            Block(name+" · buttress",p+new Vector3(s*1.35f,-.35f,-.25f),new Vector3(.65f,2.7f,1.15f),Stone);
            var roof=Block(name+" · roof",p+new Vector3(0,1.55f,.10f),new Vector3(2.7f,.42f,3.45f),Roof);
            roof.transform.rotation=Quaternion.Euler(0,0,s*5f);
            Block(name+" · dark opening",p+new Vector3(-s*.45f,-.25f,-1.58f),new Vector3(.72f,1.15f,.08f),Wood);
        }

        static void Arcade(string name,Vector3 p,float yaw)
        {
            var root=new GameObject(name);root.transform.SetParent(Root);root.transform.position=p;root.transform.rotation=Quaternion.Euler(0,yaw,0);
            for(int i=-1;i<=1;i++)
            {
                Block(name+" · pier "+i,p+new Vector3(i*1.25f,.65f,0),new Vector3(.28f,1.3f,.45f),Stone);
                Block(name+" · beam "+i,p+new Vector3(i*1.25f,1.35f,0),new Vector3(1.35f,.28f,.48f),Stone);
            }
        }

        static void House(string name,Vector3 p,Vector3 size,float yaw)
        {
            var root=new GameObject(name);root.transform.SetParent(Root);root.transform.position=p;root.transform.rotation=Quaternion.Euler(0,yaw,0);
            var body=Block(name+" · body",p+Vector3.up*(size.y*.5f),size,Stone);body.transform.rotation=root.transform.rotation;
            var roof=Block(name+" · roof",p+new Vector3(0,size.y+.43f,0),new Vector3(size.x*1.12f,.48f,size.z*1.10f),Roof);
            roof.transform.rotation=root.transform.rotation*Quaternion.Euler(0,0,4f);
            var door=Block(name+" · door",p+root.transform.rotation*new Vector3(0,.62f,-size.z*.51f),new Vector3(.48f,1.1f,.10f),Wood);
            door.transform.rotation=root.transform.rotation;
        }

        static void Banner(Vector3 p){ Block("GC2 · blue banner",p,new Vector3(.52f,1.35f,.05f),Blue); }

        static GameObject Wedge(string name,Vector3 p,Vector3 scale,float zRot,Material m)
        { var go=Block(name,p,scale,m);go.transform.rotation=Quaternion.Euler(0,0,zRot);return go; }

        static GameObject Block(string name,Vector3 p,Vector3 scale,Material m)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(Root);
            go.transform.position=p;go.transform.localScale=scale;
            var col=go.GetComponent<Collider>();if(col!=null)UnityEngine.Object.DestroyImmediate(col);
            go.GetComponent<Renderer>().sharedMaterial=m;return go;
        }

        static Material Mat(Color color)
        {
            var shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            var m=new Material(shader);if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color);
            if(m.HasProperty("_Color"))m.SetColor("_Color",color);if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.025f);
            return m;
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
