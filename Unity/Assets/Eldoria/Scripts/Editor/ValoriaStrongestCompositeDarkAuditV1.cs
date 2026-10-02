using System.Collections.Generic;
using System.IO;
using System.Text;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.EditorTools
{
    public static class ValoriaStrongestCompositeDarkAuditV1
    {
        const string Folder="ValoriaStrongestCompositeDarkAuditV1";

        public static void Run()
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
            ValoriaCliffIslandReframeV1.Enabled=false;
            ValoriaCliffIslandCleanupV2.Enabled=false;
            ValoriaResidualCleanupV1.Enabled=false;
            ValoriaWorldFrameMountainTerrainV1.Enabled=false;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);
            var root=GameObject.Find("Valoria · integrated construction visual layer");
            var c=Camera.main;
            if(root==null||c==null)throw new System.Exception("Valoria prerequisites missing.");

            ValoriaBenchmarkCompositeV2.Enabled=true;ValoriaBenchmarkCompositeV2.Build(root.transform,state);
            ValoriaEnvironmentUpliftV1.Enabled=true;ValoriaEnvironmentUpliftV1.Build(root.transform,state);
            ValoriaArchitectureCoherenceV1.Enabled=true;ValoriaArchitectureCoherenceV1.Build(root.transform,state);
            ValoriaCliffIslandReframeV1.Enabled=true;ValoriaCliffIslandReframeV1.Build(root.transform,state);
            ValoriaCliffIslandCleanupV2.Enabled=true;ValoriaCliffIslandCleanupV2.Build(root.transform,state);
            ValoriaResidualCleanupV1.Enabled=true;ValoriaResidualCleanupV1.Build(root.transform,state);

            c.transform.position=new Vector3(18.2f,14.6f,-25.8f);
            c.transform.LookAt(new Vector3(0f,3.35f,5.6f));
            c.orthographic=true;c.orthographicSize=9.1f;

            var rows=new List<string>();
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var b=r.bounds;
                // Focus on objects that can materially create the lower/side black masses in the official camera.
                if(b.size.x<.45f&&b.size.y<.45f&&b.size.z<.45f)continue;
                foreach(var m in r.sharedMaterials)
                {
                    if(m==null)continue;
                    Color col=Color.white;bool hasColor=false;
                    if(m.HasProperty("_BaseColor")){col=m.GetColor("_BaseColor");hasColor=true;}
                    else if(m.HasProperty("_Color")){col=m.GetColor("_Color");hasColor=true;}
                    else if(m.HasProperty("_BaseColorFactor")){col=m.GetColor("_BaseColorFactor");hasColor=true;}
                    float lum=.2126f*col.r+.7152f*col.g+.0722f*col.b;
                    bool hasTex=(m.HasProperty("_BaseMap")&&m.GetTexture("_BaseMap")!=null)||
                                (m.HasProperty("_MainTex")&&m.GetTexture("_MainTex")!=null)||
                                (m.HasProperty("_Albedo")&&m.GetTexture("_Albedo")!=null);
                    if(!hasColor||lum>=.23f)continue;
                    var chain=new StringBuilder();
                    for(var t=r.transform;t!=null;t=t.parent){if(chain.Length>0)chain.Append(" <- ");chain.Append(t.name);}
                    rows.Add($"{lum:0.000}\t{(hasTex?"tex":"no_tex")}\t{r.gameObject.name}\t{m.name}\tcenter=({b.center.x:0.00},{b.center.y:0.00},{b.center.z:0.00})\tsize=({b.size.x:0.00},{b.size.y:0.00},{b.size.z:0.00})\t{chain}");
                }
            }
            rows.Sort();
            var sb=new StringBuilder("lum\ttex\trenderer\tmaterial\tcenter\tsize\tchain\n");
            foreach(var row in rows)sb.AppendLine(row);
            File.WriteAllText(Path.Combine(Folder,"composite-dark-renderers.tsv"),sb.ToString());
            File.WriteAllText(Path.Combine(Folder,"summary.txt"),$"dark_entries={rows.Count}\n");
            Debug.Log("VALORIA_STRONGEST_COMPOSITE_DARK_AUDIT="+rows.Count);
            EditorApplication.Exit(0);
        }
    }
}
