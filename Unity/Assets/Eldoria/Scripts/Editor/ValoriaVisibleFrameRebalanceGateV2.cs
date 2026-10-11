using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    public static class ValoriaVisibleFrameRebalanceGateV2
    {
        const string Folder="ValoriaVisibleFrameRebalanceV2Captures";

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
            var p=new Vector3(18.2f,14.6f,-25.8f);
            var t=new Vector3(0f,3.35f,5.6f);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            BuildStrongestBase(root.transform,state,c);
            ValoriaVisibleFrameRebalanceV1.Enabled=true;
            ValoriaVisibleFrameRebalanceV1.Build(root.transform,state);
            SaveSet(c,"before",p,t);

            ValoriaCoherencePruneV1.Enabled=true;
            ValoriaCoherencePruneV1.Build(root.transform,state);
            Physics.SyncTransforms();

            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new System.Exception("Coherence-pruned rebalance altered gameplay signature.");

            SaveSet(c,"after",p,t);
            File.WriteAllText(Folder+"/evidence.json",$"{{\n"+
                $"  \"collider_hotspot_signature_equal\": true,\n"+
                $"  \"baseline\": \"Strongest + VisibleFrameRebalance v1\",\n"+
                $"  \"coherence_prune_suppressed\": {ValoriaCoherencePruneV1.Suppressed},\n"+
                $"  \"foreground_suppressed_before_prune\": {ValoriaVisibleFrameRebalanceV1.ForegroundSuppressed},\n"+
                $"  \"retinted_before_prune\": {ValoriaVisibleFrameRebalanceV1.RenderersRetinted},\n"+
                $"  \"geometry_added\": false,\n"+
                $"  \"tripo_credits\": 0\n"+
                $"}}\n");
            EditorApplication.Exit(0);
        }

        static void BuildStrongestBase(Transform root,PlayerState state,Camera c)
        {
            ValoriaBenchmarkCompositeV2.Enabled=true;ValoriaBenchmarkCompositeV2.Build(root,state);
            ValoriaEnvironmentUpliftV1.Enabled=true;ValoriaEnvironmentUpliftV1.Build(root,state);
            ValoriaArchitectureCoherenceV1.Enabled=true;ValoriaArchitectureCoherenceV1.Build(root,state);
            ValoriaCliffIslandReframeV1.Enabled=true;ValoriaCliffIslandReframeV1.Build(root,state);
            ValoriaBackplateCandidateV1.Enabled=true;
            if(!ValoriaBackplateCandidateV1.Build(root,c,"kiara3_1"))
                throw new System.Exception("Kiara 3 backplate unavailable.");
            ValoriaBackplateCandidateV1.FitAspect(1280f/720f);
            ValoriaCliffIslandCleanupV2.Enabled=true;ValoriaCliffIslandCleanupV2.Build(root,state);
            ValoriaResidualCleanupV1.Enabled=true;ValoriaResidualCleanupV1.Build(root,state);
            ValoriaMaterialResidueCleanupV2.Enabled=true;ValoriaMaterialResidueCleanupV2.Build(root,state);
            ValoriaFullFrameArtifactCleanupV1.Enabled=true;ValoriaFullFrameArtifactCleanupV1.Build(root,state);
        }

        static void SaveSet(Camera c,string tag,Vector3 p,Vector3 t)
        {
            ValoriaBackplateCandidateV1.FitAspect(1280f/720f);
            Save(c,Folder+"/"+tag+"-19.png",p,t,19f,1280,720);
            Save(c,Folder+"/"+tag+"-12.png",p,t,12f,1280,720);
            Save(c,Folder+"/"+tag+"-9.png",p,t,9f,1280,720);
            Save(c,Folder+"/"+tag+"-production.png",p,t,9.1f,1280,720);
            ValoriaBackplateCandidateV1.FitAspect(390f/844f);
            Save(c,Folder+"/"+tag+"-mobile.png",p,t,9.1f,390,844);
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
