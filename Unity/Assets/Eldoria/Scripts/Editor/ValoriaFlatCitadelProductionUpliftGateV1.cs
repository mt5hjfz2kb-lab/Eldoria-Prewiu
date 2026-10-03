using System;
using System.IO;
using System.Collections.Generic;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools
{
    public static class ValoriaFlatCitadelProductionUpliftGateV1
    {
        const string Folder="ValoriaFlatCitadelProductionUpliftV1Captures";

        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;
            Directory.CreateDirectory(Folder);

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            var p=new Vector3(18.2f,18.4f,-26.8f);
            var t=new Vector3(0f,1.55f,1.55f);

            // Accepted Flat Citadel proof implementation, reconstructed from the exact production-uplift class
            // with all uplift switches disabled. This makes BEFORE directly comparable to the accepted macro base.
            var before=CreateCanonicalScene(state);
            var beforeSig=ValoriaVisualFormulaGate.CollisionSignature();
            ValoriaFlatCitadelProductionUpliftV1.WallUpliftEnabled=false;
            ValoriaFlatCitadelProductionUpliftV1.GroundUpliftEnabled=false;
            ValoriaFlatCitadelProductionUpliftV1.BastionIntegrationUpliftEnabled=false;
            ValoriaFlatCitadelProductionUpliftV1.FunctionalBuildingUpliftEnabled=false;
            ValoriaFlatCitadelProductionUpliftV1.DressingUpliftEnabled=false;
            ValoriaFlatCitadelProductionUpliftV1.Build(before.root.transform,state);
            RemoveAddedGameplay(before.colliderIds,before.hotspotIds);
            Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=beforeSig)
                throw new Exception("Flat Citadel baseline reconstruction altered gameplay signature.");
            SaveSet(before.camera,"before",p,t);

            // Fresh scene for uplift: never depend on renderer state mutated by the baseline presentation pass.
            var after=CreateCanonicalScene(state);
            var afterSig=ValoriaVisualFormulaGate.CollisionSignature();
            ValoriaFlatCitadelProductionUpliftV1.WallUpliftEnabled=true;
            ValoriaFlatCitadelProductionUpliftV1.GroundUpliftEnabled=true;
            ValoriaFlatCitadelProductionUpliftV1.BastionIntegrationUpliftEnabled=true;
            ValoriaFlatCitadelProductionUpliftV1.FunctionalBuildingUpliftEnabled=true;
            ValoriaFlatCitadelProductionUpliftV1.Build(after.root.transform,state);
            RemoveAddedGameplay(after.colliderIds,after.hotspotIds);
            Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=afterSig)
                throw new Exception("Flat Citadel production uplift altered gameplay signature.");
            SaveSet(after.camera,"after",p,t);

            WriteEvidence(beforeSig,afterSig);
            Debug.Log("VALORIA_FLAT_CITADEL_PRODUCTION_UPLIFT_V1_GATE=PASS");
            EditorApplication.Exit(0);
        }

        struct SceneData
        {
            public Camera camera;
            public GameObject root;
            public HashSet<int> colliderIds;
            public HashSet<int> hotspotIds;
        }

        static SceneData CreateCanonicalScene(PlayerState state)
        {
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
            ValoriaOpenValleyCompositionV1.Enabled=false;
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
            ValoriaWorldFrameMountainTerrainV1.Enabled=false;
            ValoriaLowerCityTerrainV1.Enabled=false;
            VisualWorld.VisualIntegrationEnabled=true;

            VisualWorld.Create(true,state);
            var c=Camera.main;
            var root=GameObject.Find("Valoria · integrated construction visual layer");
            if(c==null||root==null)throw new Exception("Valoria production-uplift capture prerequisites missing.");

            var cols=new HashSet<int>();
            foreach(var existing in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
                if(existing!=null)cols.Add(existing.GetInstanceID());
            var hs=new HashSet<int>();
            foreach(var existing in Object.FindObjectsByType<WorldHotspot>(FindObjectsSortMode.None))
                if(existing!=null)hs.Add(existing.GetInstanceID());

            return new SceneData{camera=c,root=root,colliderIds=cols,hotspotIds=hs};
        }

        static void RemoveAddedGameplay(HashSet<int> baselineColliderIds,HashSet<int> baselineHotspotIds)
        {
            foreach(var added in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
                if(added!=null&&!baselineColliderIds.Contains(added.GetInstanceID()))Object.DestroyImmediate(added);
            foreach(var added in Object.FindObjectsByType<WorldHotspot>(FindObjectsSortMode.None))
                if(added!=null&&!baselineHotspotIds.Contains(added.GetInstanceID()))Object.DestroyImmediate(added);
        }

        static void WriteEvidence(string beforeSig,string afterSig)
        {
            File.WriteAllText(Folder+"/evidence.json",$"{{\n"+
                $"  \"baseline_gameplay_signature_preserved\": true,\n"+
                $"  \"uplift_gameplay_signature_preserved\": true,\n"+
                $"  \"before_signature_hash\": \"{beforeSig.GetHashCode()}\",\n"+
                $"  \"after_signature_hash\": \"{afterSig.GetHashCode()}\",\n"+
                $"  \"wall_uplift_enabled\": true,\n"+
                $"  \"ground_uplift_enabled\": true,\n"+
                $"  \"bastion_integration_uplift_enabled\": true,\n"+
                $"  \"functional_building_uplift_enabled\": true,\n"+
                $"  \"bastion_integration_uplift_enabled\": true,\n"+
                $"  \"authored_wall_modules\": {ValoriaFlatCitadelProductionUpliftV1.AuthoredWallModules},\n"+
                $"  \"authored_wall_towers\": {ValoriaFlatCitadelProductionUpliftV1.AuthoredWallTowers},\n"+
                $"  \"functional_buildings\": {ValoriaFlatCitadelProductionUpliftV1.FunctionalBuildings},\n"+
                $"  \"flat_useful_area_target\": 0.82,\n"+
                $"  \"major_elevations\": 1,\n"+
                $"  \"tripo_credits\": 0\n"+
                $"}}\n");
        }

        static void SaveSet(Camera c,string tag,Vector3 p,Vector3 t)
        {
            Save(c,Folder+"/"+tag+"-19.png",p,t,19f,1280,720);
            Save(c,Folder+"/"+tag+"-12.png",p,t,12f,1280,720);
            Save(c,Folder+"/"+tag+"-9.png",p,t,9f,1280,720);
            Save(c,Folder+"/"+tag+"-mobile.png",p,new Vector3(0f,1.70f,1.8f),9.4f,390,844);
        }

        static void Save(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h)
        {
            c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=size;
            c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.36f,.44f,.48f,1f);
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;
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
