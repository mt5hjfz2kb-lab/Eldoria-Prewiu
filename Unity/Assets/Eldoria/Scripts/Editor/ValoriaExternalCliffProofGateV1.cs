using System;
using System.IO;
using System.Linq;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.EditorTools
{
    public static class ValoriaExternalCliffProofGateV1
    {
        const string Folder="ValoriaExternalCliffProofV1Captures";
        const string ExternalFolder="Assets/Eldoria/ExternalProof/RockFace02";

        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;
            Directory.CreateDirectory(Folder);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
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
            ValoriaLowerCityPlateauV1.Enabled=false;
            ValoriaWorldFrameMountainTerrainV1.Enabled=false;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);
            var camera=Camera.main;
            var root=GameObject.Find("Valoria · integrated construction visual layer");
            if(camera==null||root==null)throw new Exception("Valoria capture prerequisites missing.");

            var p=new Vector3(18.2f,14.6f,-25.8f);
            var t=new Vector3(0f,3.35f,5.6f);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            BuildStrongestBase(root.transform,state,camera);
            SaveSet(camera,"before",p,t);

            int suppressed=SuppressFragmentedLowerSupports();
            int cliffs=AddExternalCliffs(root.transform);

            Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new Exception("External cliff proof altered gameplay signature.");

            SaveSet(camera,"after",p,t);
            File.WriteAllText(Folder+"/evidence.json",$"{{\n"+
                $"  \"collider_hotspot_signature_equal\": true,\n"+
                $"  \"external_asset\": \"Poly Haven Rock Face 02\",\n"+
                $"  \"license\": \"CC0\",\n"+
                $"  \"external_cliff_instances\": {cliffs},\n"+
                $"  \"fragment_renderers_suppressed\": {suppressed},\n"+
                $"  \"runtime_persistence\": false,\n"+
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
                throw new Exception("Kiara 3 backplate unavailable.");
            ValoriaBackplateCandidateV1.FitAspect(1280f/720f);
            ValoriaCliffIslandCleanupV2.Enabled=true;ValoriaCliffIslandCleanupV2.Build(root,state);
            ValoriaResidualCleanupV1.Enabled=true;ValoriaResidualCleanupV1.Build(root,state);
            ValoriaMaterialResidueCleanupV2.Enabled=true;ValoriaMaterialResidueCleanupV2.Build(root,state);
            ValoriaFullFrameArtifactCleanupV1.Enabled=true;ValoriaFullFrameArtifactCleanupV1.Build(root,state);
            ValoriaCoherencePruneV1.Enabled=true;ValoriaCoherencePruneV1.Build(root,state);
        }

        static int AddExternalCliffs(Transform root)
        {
            if(!Directory.Exists(ExternalFolder))throw new Exception("External cliff staging folder missing.");
            var gltf=Directory.GetFiles(ExternalFolder,"*.gltf",SearchOption.AllDirectories)
                .FirstOrDefault();
            if(string.IsNullOrEmpty(gltf))throw new Exception("Rock Face 02 glTF was not staged.");
            gltf=gltf.Replace('\\','/');
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(gltf);
            if(prefab==null)throw new Exception("Rock Face 02 did not import as a GameObject: "+gltf);

            var holder=new GameObject("Valoria · External Cliff Proof v1").transform;
            holder.SetParent(root,true);
            var specs=new[]{
                (new Vector3(-7.9f,-1.10f,-5.8f),5.2f,3.2f,18f),
                (new Vector3(-3.0f,-1.32f,-8.1f),5.4f,3.1f,4f),
                (new Vector3(2.9f,-1.30f,-8.15f),5.4f,3.1f,182f),
                (new Vector3(7.8f,-1.05f,-5.9f),5.2f,3.2f,198f),
                (new Vector3(-8.7f,-.72f,-1.0f),4.3f,2.8f,82f),
                (new Vector3(8.7f,-.72f,-1.1f),4.3f,2.8f,278f)
            };
            int count=0;
            foreach(var s in specs)
            {
                var go=ValoriaKit.BenchmarkPiece("Valoria · Rock Face 02 · cliff "+count,prefab,s.Item1,s.Item2,s.Item3,
                    Quaternion.Euler(0f,s.Item4,0f));
                if(go==null)continue;
                go.transform.SetParent(holder,true);
                foreach(var col in go.GetComponentsInChildren<Collider>(true))col.enabled=false;
                foreach(var h in go.GetComponentsInChildren<WorldHotspot>(true))Object.DestroyImmediate(h);
                count++;
            }
            return count;
        }

        static int SuppressFragmentedLowerSupports()
        {
            int count=0;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var b=r.bounds;
                string chain=Chain(r.transform);
                if(chain.Contains("bastion")||chain.Contains("aserradero")||chain.Contains("cuartel")||
                   chain.Contains("granary")||chain.Contains("granero")||chain.Contains("backplate")||
                   chain.Contains("main street")||chain.Contains("processional"))continue;
                if(b.center.y>1.5f||b.center.z>5.6f)continue;
                bool fragment=chain.Contains("foreground edge")||chain.Contains("lower cliff authored rock")||
                    chain.Contains("terrainterrace")||chain.Contains("expansion edge geology")||
                    chain.Contains("environment uplift · rock")||chain.Contains("cliff island · lower")||
                    chain.Contains("cliff island · middle")||chain.Contains("broadrockplatform")||
                    chain.Contains("steppedrockterrace");
                if(fragment){r.enabled=false;count++;}
            }
            return count;
        }

        static string Chain(Transform t)
        {
            string s="";
            for(var p=t;p!=null;p=p.parent)s+="|"+p.name.ToLowerInvariant();
            return s;
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
                File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);
            } finally {
                c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);
            }
        }
    }
}
