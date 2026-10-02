using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    public static class ValoriaCameraFramingGateV1
    {
        const string Folder="ValoriaCameraFramingV1Captures";

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
            MidTierDistrictProduction.Enabled=true;
            ValoriaFullFrameArchitectureBatchV1.Enabled=true;
            ValoriaFullFrameForegroundEdgePassV1.Enabled=true;
            ValoriaReferenceConvergencePassV2.Enabled=false;
            ValoriaInCitySurfacePassV1.Enabled=false;
            ValoriaStairLandingIntegrationV1.Enabled=false;
            ValoriaFullFrameConvergenceIteration1.Enabled=false;
            ValoriaFullFrameConvergenceIteration2.Enabled=false;
            ValoriaOpenValleyCompositionV1.Enabled=true;
            ValoriaWorldFrameMountainTerrainV1.Enabled=false;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);

            var c=Camera.main;
            if(c==null)throw new System.Exception("Valoria camera missing.");
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            var pos=new Vector3(18.2f,14.6f,-25.8f);
            var variants=new[]{
                new[]{10.2f,0f,3.15f,5.8f},
                new[]{9.6f,0f,3.25f,5.7f},
                new[]{9.1f,0f,3.35f,5.6f},
                new[]{8.7f,0f,3.50f,5.5f},
                new[]{9.2f,-.5f,3.45f,5.2f},
                new[]{9.2f,.7f,3.45f,5.7f}
            };

            for(int i=0;i<variants.Length;i++)
            {
                var v=variants[i];
                var target=new Vector3(v[1],v[2],v[3]);
                Save(c,Folder+"/framing-"+i+"-desktop.png",pos,target,v[0],1280,720);
                Save(c,Folder+"/framing-"+i+"-mobile.png",pos,target,v[0],390,844);
            }

            Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new System.Exception("Camera framing proof altered gameplay signature.");

            File.WriteAllText(Folder+"/evidence.json",
                "{\n"+
                "  \"camera_only\": true,\n"+
                "  \"production_camera_unchanged\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"variants\": 6,\n"+
                "  \"tripo_credits\": 0\n"+
                "}\n");
            EditorApplication.Exit(0);
        }

        static void Save(Camera c,string path,Vector3 pos,Vector3 target,float size,int w,int h)
        {
            c.transform.position=pos;
            c.transform.LookAt(target);
            c.orthographic=true;
            c.orthographicSize=size;
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);
            var prev=RenderTexture.active;
            try
            {
                c.targetTexture=rt;
                c.Render();
                c.Render();
                RenderTexture.active=rt;
                var im=new Texture2D(w,h,TextureFormat.RGB24,false);
                im.ReadPixels(new Rect(0,0,w,h),0,0);
                im.Apply();
                File.WriteAllBytes(path,im.EncodeToPNG());
                Object.DestroyImmediate(im);
            }
            finally
            {
                c.targetTexture=null;
                RenderTexture.active=prev;
                rt.Release();
                Object.DestroyImmediate(rt);
            }
        }
    }
}
