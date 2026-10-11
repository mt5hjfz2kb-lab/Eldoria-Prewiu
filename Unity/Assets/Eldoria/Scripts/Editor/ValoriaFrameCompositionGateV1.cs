using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    public static class ValoriaFrameCompositionGateV1
    {
        const string Folder="ValoriaFrameCompositionV1Captures";

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
            ValoriaOpenValleyCompositionV1.Enabled=true;
            ValoriaReferenceConvergencePassV2.Enabled=false;
            ValoriaInCitySurfacePassV1.Enabled=false;
            ValoriaStairLandingIntegrationV1.Enabled=false;
            ValoriaFullFrameConvergenceIteration1.Enabled=false;
            ValoriaFullFrameConvergenceIteration2.Enabled=false;
            ValoriaBenchmarkCompositeV2.Enabled=false;
            ValoriaEnvironmentUpliftV1.Enabled=false;
            ValoriaArchitectureCoherenceV1.Enabled=false;
            ValoriaBackplateCandidateV1.Enabled=false;
            ValoriaCliffIslandReframeV1.Enabled=false;
            ValoriaCliffIslandCleanupV2.Enabled=false;
            ValoriaResidualCleanupV1.Enabled=false;
            ValoriaMaterialResidueCleanupV2.Enabled=false;
            ValoriaFullFrameArtifactCleanupV1.Enabled=false;
            ValoriaVisibleFrameRebalanceV1.Enabled=false;
            ValoriaCoherencePruneV1.Enabled=false;
            ValoriaWorldFrameMountainTerrainV1.Enabled=false;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);
            var c=Camera.main;
            var root=GameObject.Find("Valoria · integrated construction visual layer");
            if(c==null||root==null)throw new System.Exception("Valoria capture prerequisites missing.");
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            BuildBestCleanStack(root.transform,state,c);

            var pos=new Vector3(18.2f,14.6f,-25.8f);
            var variants=new[]{
                new Vector4(9.1f,3.35f,5.60f,0f),
                new Vector4(8.7f,3.60f,5.70f,0f),
                new Vector4(8.4f,3.80f,5.80f,0f),
                new Vector4(8.1f,3.95f,5.90f,0f),
                new Vector4(7.8f,4.05f,6.00f,0f)
            };
            for(int i=0;i<variants.Length;i++)
            {
                var v=variants[i];
                var target=new Vector3(0f,v.y,v.z);
                ValoriaBackplateCandidateV1.FitAspect(1280f/720f);
                Save(c,Folder+"/desktop-"+i+".png",pos,target,v.x,1280,720);
                ValoriaBackplateCandidateV1.FitAspect(390f/844f);
                Save(c,Folder+"/mobile-"+i+".png",pos,target,v.x,390,844);
            }

            Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new System.Exception("Frame composition proof altered gameplay signature.");

            File.WriteAllText(Folder+"/evidence.json",
                "{\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"projection\": \"orthographic\",\n"+
                "  \"sizes\": [9.1,8.7,8.4,8.1,7.8],\n"+
                "  \"target_y\": [3.35,3.60,3.80,3.95,4.05],\n"+
                "  \"target_z\": [5.60,5.70,5.80,5.90,6.00],\n"+
                "  \"production_camera_changed\": false,\n"+
                "  \"tripo_credits\": 0\n"+
                "}\n");
            EditorApplication.Exit(0);
        }

        static void BuildBestCleanStack(Transform root,PlayerState state,Camera c)
        {
            ValoriaBenchmarkCompositeV2.Enabled=true;ValoriaBenchmarkCompositeV2.Build(root,state);
            ValoriaEnvironmentUpliftV1.Enabled=true;ValoriaEnvironmentUpliftV1.Build(root,state);
            ValoriaArchitectureCoherenceV1.Enabled=true;ValoriaArchitectureCoherenceV1.Build(root,state);
            ValoriaCliffIslandReframeV1.Enabled=true;ValoriaCliffIslandReframeV1.Build(root,state);
            ValoriaBackplateCandidateV1.Enabled=true;
            if(!ValoriaBackplateCandidateV1.Build(root,c,"kiara3_1"))
                throw new System.Exception("Kiara 3 backplate unavailable.");
            ValoriaCliffIslandCleanupV2.Enabled=true;ValoriaCliffIslandCleanupV2.Build(root,state);
            ValoriaResidualCleanupV1.Enabled=true;ValoriaResidualCleanupV1.Build(root,state);
            ValoriaMaterialResidueCleanupV2.Enabled=true;ValoriaMaterialResidueCleanupV2.Build(root,state);
            ValoriaFullFrameArtifactCleanupV1.Enabled=true;ValoriaFullFrameArtifactCleanupV1.Build(root,state);
            ValoriaVisibleFrameRebalanceV1.Enabled=true;ValoriaVisibleFrameRebalanceV1.Build(root,state);
            ValoriaCoherencePruneV1.Enabled=true;ValoriaCoherencePruneV1.Build(root,state);
        }

        static void Save(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h)
        {
            c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=size;
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;
            try{
                c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;
                var im=new Texture2D(w,h,TextureFormat.RGB24,false);
                im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();
                File.WriteAllBytes(path,im.EncodeToPNG());UnityEngine.Object.DestroyImmediate(im);
            } finally {
                c.targetTexture=null;RenderTexture.active=prev;rt.Release();UnityEngine.Object.DestroyImmediate(rt);
            }
        }
    }
}
