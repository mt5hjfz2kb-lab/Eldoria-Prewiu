using System;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    public static class ValoriaMasterVisualRebuildV2Gate
    {
        const string Folder="ValoriaMasterVisualRebuildV2Captures";
        static readonly Vector3 CameraPosition=new Vector3(18.2f,14.6f,-25.8f);
        static readonly Vector3 CameraTarget=new Vector3(0f,3.15f,5.8f);

        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;
            SceneSetup.SetupRenderPipeline();
            Directory.CreateDirectory(Folder);

            ProductionVisualIntegration.MasterVisualRebuildV2Enabled=false;
            Build(); var camera=Camera.main;if(camera==null)throw new Exception("Baseline camera missing");
            string baseline=ValoriaVisualFormulaGate.CollisionSignature();
            Save(camera,Folder+"/before-19.png",19,1280,720);
            Save(camera,Folder+"/before-12.png",12,1280,720);
            Save(camera,Folder+"/before-9.png",9,1280,720);
            Save(camera,Folder+"/before-mobile.png",12,390,844);
            var before=Metrics();

            ProductionVisualIntegration.MasterVisualRebuildV2Enabled=true;
            Build(); camera=Camera.main;if(camera==null)throw new Exception("Rebuild camera missing");
            string afterSig=ValoriaVisualFormulaGate.CollisionSignature();
            if(afterSig!=baseline)throw new Exception("Master visual rebuild altered collider/hotspot signature.");
            Save(camera,Folder+"/after-19.png",19,1280,720);
            Save(camera,Folder+"/after-12.png",12,1280,720);
            Save(camera,Folder+"/after-9.png",9,1280,720);
            Save(camera,Folder+"/after-mobile.png",12,390,844);
            var after=Metrics();

            File.WriteAllText(Folder+"/evidence.json",
                "{\n"+
                "  \"phase\":\"VALORIA_MASTER_VISUAL_REBUILD_V2\",\n"+
                "  \"profile\":\"environment_composition\",\n"+
                "  \"tripo_credits\":0,\n"+
                "  \"collider_hotspot_signature_equal\":true,\n"+
                "  \"before\":"+before+",\n"+
                "  \"after\":"+after+"\n"+
                "}\n");
            EditorApplication.Exit(0);
        }

        static void Build()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            VisualWorld.VisualIntegrationEnabled=true;
            VisualWorld.Create(true,new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true});
        }

        static string Metrics()
        {
            int r=0,m=0,l=0;long t=0;
            foreach(var x in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(!x.enabled||!x.gameObject.activeInHierarchy)continue;
                r++;m+=x.sharedMaterials.Length;
                var mf=x.GetComponent<MeshFilter>();if(mf!=null&&mf.sharedMesh!=null)t+=mf.sharedMesh.triangles.LongLength/3;
            }
            foreach(var x in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(x.enabled&&x.gameObject.activeInHierarchy)l++;
            return "{\"triangles\":"+t+",\"renderers\":"+r+",\"materials\":"+m+",\"lights\":"+l+"}";
        }

        static void Save(Camera camera,string path,float zoom,int width,int height)
        {
            camera.transform.position=CameraPosition;camera.transform.LookAt(CameraTarget);camera.orthographic=true;camera.orthographicSize=zoom;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;
            UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);
        }
    }
}
