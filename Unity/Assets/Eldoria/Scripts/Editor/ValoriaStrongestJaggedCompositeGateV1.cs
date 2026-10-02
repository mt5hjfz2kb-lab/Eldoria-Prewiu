using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    public static class ValoriaStrongestJaggedCompositeGateV1
    {
        const string Folder="ValoriaStrongestJaggedCompositeV1Captures";

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
            ValoriaJaggedBackdropV1.Enabled=false;
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

            ValoriaBenchmarkCompositeV2.Enabled=true;ValoriaBenchmarkCompositeV2.Build(root.transform,state);
            ValoriaEnvironmentUpliftV1.Enabled=true;ValoriaEnvironmentUpliftV1.Build(root.transform,state);
            ValoriaArchitectureCoherenceV1.Enabled=true;ValoriaArchitectureCoherenceV1.Build(root.transform,state);
            ValoriaCliffIslandReframeV1.Enabled=true;ValoriaCliffIslandReframeV1.Build(root.transform,state);
            ValoriaCliffIslandCleanupV2.Enabled=true;ValoriaCliffIslandCleanupV2.Build(root.transform,state);
            ValoriaResidualCleanupV1.Enabled=true;ValoriaResidualCleanupV1.Build(root.transform,state);
            ValoriaMaterialResidueCleanupV2.Enabled=true;ValoriaMaterialResidueCleanupV2.Build(root.transform,state);
            ValoriaFullFrameArtifactCleanupV1.Enabled=true;ValoriaFullFrameArtifactCleanupV1.Build(root.transform,state);

            int normalizedDarkFamilies=NormalizeKnownDarkFamilies();

            // Capture current strongest composition without a photographic backdrop first.
            Save(c,Folder+"/baseline-no-backdrop.png",p,t,9.1f,1280,720);

            ValoriaJaggedBackdropV1.Enabled=true;
            float[] biases={-5f,-3f,-1f,1f,3f};
            for(int i=0;i<biases.Length;i++)
            {
                ValoriaJaggedBackdropV1.VerticalBias=biases[i];
                if(!ValoriaJaggedBackdropV1.Build(root.transform,state))
                    throw new System.Exception("Jagged backdrop unavailable in integrated composite.");
                Save(c,Folder+"/after-bias"+i+".png",p,t,9.1f,1280,720);
                Save(c,Folder+"/after-bias"+i+"-mobile.png",p,t,9.1f,390,844);
            }

            Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new System.Exception("Strongest Jagged Composite altered gameplay signature.");

            File.WriteAllText(Folder+"/evidence.json",
                "{\n"+
                "  \"composition\": \"strongest-current-main + residue-cleanup-v2 + artifact-cleanup-v1 + jagged-peaks\",\n"+
                "  \"camera_size\": 9.1,\n"+
                "  \"vertical_biases\": [-5,-3,-1,1,3],\n"+
                "  \"source\": \"Wikimedia Commons - Jagged peaks over a valley\",\n"+
                "  \"license\": \"CC0\",\n"+
                "  \"normalized_known_dark_family_slots\": "+normalizedDarkFamilies+",\n"+
                "  \"material_residue_environment_slots\": "+ValoriaMaterialResidueCleanupV2.NormalizedEnvironment+",\n"+
                "  \"artifact_rock_renderers\": "+ValoriaFullFrameArtifactCleanupV1.RockRenderersNormalized+",\n"+
                "  \"artifact_terrace_renderers\": "+ValoriaFullFrameArtifactCleanupV1.TerraceRenderersNormalized+",\n"+
                "  \"artifact_foliage_lifted\": "+ValoriaFullFrameArtifactCleanupV1.FoliageRenderersLifted+",\n"+
                "  \"artifact_dark_flat_lifted\": "+ValoriaFullFrameArtifactCleanupV1.DarkFlatRenderersLifted+",\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"tripo_credits\": 0\n"+
                "}\n");
            EditorApplication.Exit(0);
        }


        static int NormalizeKnownDarkFamilies()
        {
            int changed=0;
            var rockShader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");
            if(rockShader==null)return 0;

            Material rock=null,tree=null;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var mats=r.sharedMaterials;
                bool dirty=false;
                for(int i=0;i<mats.Length;i++)
                {
                    var m=mats[i];
                    if(m==null)continue;
                    string n=m.name??"";
                    bool isRock=n.Contains("Rock02",System.StringComparison.OrdinalIgnoreCase);
                    bool isTree=n.Contains("Tree01",System.StringComparison.OrdinalIgnoreCase);
                    if(!isRock&&!isTree)continue;

                    Texture tex=null;
                    if(m.HasProperty("_BaseMap"))tex=m.GetTexture("_BaseMap");
                    if(tex==null&&m.HasProperty("_MainTex"))tex=m.GetTexture("_MainTex");
                    if(tex!=null)continue;

                    Color col=Color.white;
                    if(m.HasProperty("_BaseColor"))col=m.GetColor("_BaseColor");
                    else if(m.HasProperty("_Color"))col=m.GetColor("_Color");
                    float lum=.2126f*col.r+.7152f*col.g+.0722f*col.b;
                    if(lum>=.16f)continue;

                    if(isRock)
                    {
                        if(rock==null)
                        {
                            rock=new Material(rockShader){name="Valoria Strongest · normalized Rock02"};
                            if(rock.HasProperty("_BaseColor"))rock.SetColor("_BaseColor",new Color(.39f,.38f,.34f,1f));
                            if(rock.HasProperty("_Color"))rock.SetColor("_Color",new Color(.39f,.38f,.34f,1f));
                            if(rock.HasProperty("_Smoothness"))rock.SetFloat("_Smoothness",.025f);
                            if(rock.HasProperty("_Metallic"))rock.SetFloat("_Metallic",0f);
                        }
                        mats[i]=rock;
                    }
                    else
                    {
                        if(tree==null)
                        {
                            tree=new Material(rockShader){name="Valoria Strongest · normalized Tree01"};
                            if(tree.HasProperty("_BaseColor"))tree.SetColor("_BaseColor",new Color(.17f,.27f,.18f,1f));
                            if(tree.HasProperty("_Color"))tree.SetColor("_Color",new Color(.17f,.27f,.18f,1f));
                            if(tree.HasProperty("_Smoothness"))tree.SetFloat("_Smoothness",.02f);
                            if(tree.HasProperty("_Metallic"))tree.SetFloat("_Metallic",0f);
                        }
                        mats[i]=tree;
                    }
                    changed++;dirty=true;
                }
                if(dirty)r.sharedMaterials=mats;
            }
            return changed;
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
