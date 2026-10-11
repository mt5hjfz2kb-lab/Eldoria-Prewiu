using System.Collections.Generic;
using System.IO;
using System.Text;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    public static class ValoriaDarkRendererAuditV1
    {
        const string Folder="ValoriaDarkRendererAuditV1";

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

            var rows=new List<string>();
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                foreach(var m in r.sharedMaterials)
                {
                    if(m==null)continue;
                    Color c=Color.white;bool hasColor=false;
                    if(m.HasProperty("_BaseColor")){c=m.GetColor("_BaseColor");hasColor=true;}
                    else if(m.HasProperty("_Color")){c=m.GetColor("_Color");hasColor=true;}
                    float lum=.2126f*c.r+.7152f*c.g+.0722f*c.b;
                    bool hasTex=(m.HasProperty("_BaseMap")&&m.GetTexture("_BaseMap")!=null)||(m.HasProperty("_MainTex")&&m.GetTexture("_MainTex")!=null);
                    if((hasColor&&lum<.15f)||(!hasTex&&hasColor&&lum<.24f))
                    {
                        var b=r.bounds;
                        rows.Add($"{r.gameObject.name}\t{m.name}\t{lum:0.000}\t{(hasTex?"tex":"no_tex")}\tcenter=({b.center.x:0.00},{b.center.y:0.00},{b.center.z:0.00})\tsize=({b.size.x:0.00},{b.size.y:0.00},{b.size.z:0.00})");
                    }
                }
            }
            rows.Sort();
            var sb=new StringBuilder();
            sb.AppendLine("renderer\tmaterial\tluminance\ttexture\tbounds_center\tbounds_size");
            foreach(var row in rows)sb.AppendLine(row);
            File.WriteAllText(Path.Combine(Folder,"dark-renderers.tsv"),sb.ToString());
            File.WriteAllText(Path.Combine(Folder,"summary.txt"),$"dark_entries={rows.Count}\n");
            EditorApplication.Exit(0);
        }
    }
}
