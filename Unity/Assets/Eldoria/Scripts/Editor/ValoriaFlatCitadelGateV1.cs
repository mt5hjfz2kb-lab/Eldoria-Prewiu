using System;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools
{
    public static class ValoriaFlatCitadelGateV1
    {
        const string Folder="ValoriaFlatCitadelProofV1Captures";

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
            ValoriaVisualShellV2.Enabled=false;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);
            var c=Camera.main;
            var root=GameObject.Find("Valoria · integrated construction visual layer");
            if(c==null||root==null)throw new Exception("Valoria flat-citadel capture prerequisites missing.");

            var p=new Vector3(18.2f,18.4f,-26.8f);
            var t=new Vector3(0f,1.55f,1.55f);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            SaveSet(c,"before",p,t);

            ValoriaFlatCitadelProofV1.Enabled=true;
            ValoriaFlatCitadelProofV1.Build(root.transform,state);

            Physics.SyncTransforms();
            var after=ValoriaVisualFormulaGate.CollisionSignature();
            if(after!=baseline)throw new Exception("Flat Citadel proof altered gameplay collider/hotspot signature.");

            SaveSet(c,"after",p,t);
            WriteEvidence(baseline==after);

            Debug.Log("VALORIA_FLAT_CITADEL_PROOF_V1_GATE=PASS");
            EditorApplication.Exit(0);
        }

        static void WriteEvidence(bool signatureEqual)
        {
            File.WriteAllText(Folder+"/evidence.json",$"{{\n"+
                $"  \"collider_hotspot_signature_equal\": {(signatureEqual?"true":"false")},\n"+
                $"  \"hidden_legacy_renderers\": {ValoriaFlatCitadelProofV1.HiddenLegacyRenderers},\n"+
                $"  \"functional_buildings\": {ValoriaFlatCitadelProofV1.FunctionalBuildings},\n"+
                $"  \"outer_wall_pieces\": {ValoriaFlatCitadelProofV1.WallPieces},\n"+
                $"  \"nature_pieces\": {ValoriaFlatCitadelProofV1.NaturePieces},\n"+
                $"  \"flat_useful_area_target\": 0.82,\n"+
                $"  \"major_elevations\": 1,\n"+
                $"  \"background_strategy\": \"camera-contained / no mountain world-frame\",\n"+
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
            c.transform.position=p;
            c.transform.LookAt(t);
            c.orthographic=true;
            c.orthographicSize=size;
            c.clearFlags=CameraClearFlags.SolidColor;
            c.backgroundColor=new Color(.36f,.44f,.48f,1f);

            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);
            var prev=RenderTexture.active;
            try
            {
                c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;
                var im=new Texture2D(w,h,TextureFormat.RGB24,false);
                im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();
                File.WriteAllBytes(path,im.EncodeToPNG());
                Object.DestroyImmediate(im);
            }
            finally
            {
                c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);
            }
        }
    }
}
