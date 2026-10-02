using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    public static class ValoriaBackdropHorizonCropGateV2
    {
        const string Folder="ValoriaBackdropHorizonCropV2Captures";
        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory(Folder);SceneSetup.SetupRenderPipeline();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            ProductionVisualIntegration.StoneArchitectureEnabled=true;ProductionVisualIntegration.TerrainTerraceEnabled=true;ProductionVisualIntegration.SurfaceCellEnabled=false;ProductionVisualIntegration.ProductionCellEnabled=false;ProductionVisualIntegration.CoherentCastleProofEnabled=false;ProductionVisualIntegration.SlavicDistrictProofEnabled=false;ProductionVisualIntegration.CompactFootprintReframeEnabled=true;
            AssetVisualUpliftPassV1.Enabled=false;AssetLibraryReprocessingPassV1.Enabled=true;MidTierDistrictProduction.Enabled=true;ValoriaFullFrameArchitectureBatchV1.Enabled=true;ValoriaFullFrameForegroundEdgePassV1.Enabled=true;ValoriaOpenValleyCompositionV1.Enabled=true;ValoriaReferenceConvergencePassV2.Enabled=false;ValoriaInCitySurfacePassV1.Enabled=false;ValoriaStairLandingIntegrationV1.Enabled=false;ValoriaFullFrameConvergenceIteration1.Enabled=false;ValoriaFullFrameConvergenceIteration2.Enabled=false;ValoriaWorldFrameMountainTerrainV1.Enabled=false;VisualWorld.VisualIntegrationEnabled=true;
            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};VisualWorld.Create(true,state);
            var c=Camera.main;var root=GameObject.Find("Valoria · integrated construction visual layer");if(c==null||root==null)throw new System.Exception("prereqs missing");
            var p=new Vector3(18.2f,14.6f,-25.8f);var t=new Vector3(0f,3.15f,5.8f);
            c.farClipPlane=500f;c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.47f,.56f,.61f);
            ValoriaCameraBackdropV1.Enabled=true;
            ValoriaCameraBackdropV1.CropX=.55f;ValoriaCameraBackdropV1.CropW=.34f;
            ValoriaCameraBackdropV1.BackdropHeight=15f;ValoriaCameraBackdropV1.VerticalOffset=7.5f;
            if(!ValoriaCameraBackdropV1.Build(root.transform,c))throw new System.Exception("backdrop unavailable");
            var specs=new[]{
                new Vector2(.28f,.34f),new Vector2(.34f,.30f),new Vector2(.40f,.26f),
                new Vector2(.46f,.24f),new Vector2(.52f,.22f),new Vector2(.58f,.20f)
            };
            for(int i=0;i<specs.Length;i++)
            {
                ValoriaCameraBackdropV1.CropY=specs[i].x;ValoriaCameraBackdropV1.CropH=specs[i].y;ValoriaCameraBackdropV1.ApplyCrop();
                Save(c,Folder+$"/crop-{i}-12.png",p,t,12f,1280,720);
                Save(c,Folder+$"/crop-{i}-mobile.png",p,t,12f,390,844);
            }
            EditorApplication.Exit(0);
        }
        static void Save(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h){c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=size;var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;try{c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);}finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}}
    }
}
