using System;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    public static class ValoriaCompactFootprintReframeGate
    {
        const string Folder="ValoriaCompactFootprintReframeCaptures";
        static readonly Vector3 CameraPosition=new Vector3(18.2f,14.6f,-25.8f);
        static readonly Vector3 CameraTarget=new Vector3(0f,3.65f,7.25f);

        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;SceneSetup.SetupRenderPipeline();Directory.CreateDirectory(Folder);
            ProductionVisualIntegration.CompactFootprintReframeEnabled=false;
            Build();var camera=Camera.main;if(camera==null)throw new Exception("BEFORE camera missing");
            string before=ValoriaVisualFormulaGate.CollisionSignature();
            Save(camera,Folder+"/before-19.png",19,1280,720);Save(camera,Folder+"/before-12.png",12,1280,720);
            Save(camera,Folder+"/before-9.png",9,1280,720);Save(camera,Folder+"/before-mobile.png",12,390,844);
            var beforeMetrics=Metrics();var beforeWidth=UrbanWidth();

            ProductionVisualIntegration.CompactFootprintReframeEnabled=true;
            Build();camera=Camera.main;if(camera==null)throw new Exception("AFTER camera missing");
            string after=ValoriaVisualFormulaGate.CollisionSignature();
            if(before!=after)throw new Exception("Compact footprint composition altered gameplay collider/hotspot signature.");
            var afterMetrics=Metrics();var afterWidth=UrbanWidth();
            Save(camera,Folder+"/after-19.png",19,1280,720);Save(camera,Folder+"/after-12.png",12,1280,720);
            Save(camera,Folder+"/after-9.png",9,1280,720);Save(camera,Folder+"/after-mobile.png",12,390,844);
            File.WriteAllText(Folder+"/evidence.json",
                "{\n  \"phase\":\"VALORIA_COMPACT_FOOTPRINT_REFRAME_V1\",\n"+
                "  \"profile\":\"environment_composition\",\n  \"geometry_gap_proven\":false,\n  \"allow_tripo\":false,\n  \"tripo_credits\":0,\n"+
                "  \"collider_hotspot_signature_equal\":true,\n"+
                "  \"before_urban_width\":"+beforeWidth.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+",\n"+
                "  \"after_urban_width\":"+afterWidth.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+",\n"+
                "  \"before\":"+beforeMetrics+",\n  \"after\":"+afterMetrics+"\n}\n");
            EditorApplication.Exit(0);
        }
        static void Build(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);ProductionVisualIntegration.ResetVisualCachesForGate();VisualWorld.VisualIntegrationEnabled=true;VisualWorld.Create(true,new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true});}
        static float UrbanWidth(){bool any=false;float min=0,max=0;foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;var b=r.bounds;string chain="";for(var t=r.transform;t!=null;t=t.parent)chain+="|"+t.name.ToLowerInvariant();bool urban=chain.Contains("mid-tier")||chain.Contains("civil house")||chain.Contains("inhabited roofline")||chain.Contains("bastion hero")||chain.Contains("cuartel")||chain.Contains("aserradero");if(!urban)continue;if(!any){min=b.min.x;max=b.max.x;any=true;}else{min=Mathf.Min(min,b.min.x);max=Mathf.Max(max,b.max.x);}}return any?max-min:0f;}
        static string Metrics(){int r=0,m=0,l=0;long t=0;foreach(var x in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){if(!x.enabled||!x.gameObject.activeInHierarchy)continue;r++;m+=x.sharedMaterials.Length;var mf=x.GetComponent<MeshFilter>();if(mf!=null&&mf.sharedMesh!=null)t+=mf.sharedMesh.triangles.LongLength/3;}foreach(var x in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(x.enabled&&x.gameObject.activeInHierarchy)l++;return "{\"triangles\":"+t+",\"renderers\":"+r+",\"materials\":"+m+",\"lights\":"+l+"}";}
        static void Save(Camera camera,string path,float zoom,int width,int height){camera.transform.position=CameraPosition;camera.transform.LookAt(CameraTarget);camera.orthographic=true;camera.orthographicSize=zoom;var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);var tex=new Texture2D(width,height,TextureFormat.RGB24,false);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);}
    }
}
