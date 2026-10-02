using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Eldoria.EditorTools
{
    public static class ValoriaDarkMassScreenDiagnosticV2
    {
        const string Folder="ValoriaDarkMassScreenDiagnosticV2";
        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory(Folder);SceneSetup.SetupRenderPipeline();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ProductionVisualIntegration.ResetVisualCachesForGate();
            ProductionVisualIntegration.StoneArchitectureEnabled=true;ProductionVisualIntegration.TerrainTerraceEnabled=true;ProductionVisualIntegration.SurfaceCellEnabled=false;ProductionVisualIntegration.ProductionCellEnabled=false;ProductionVisualIntegration.CoherentCastleProofEnabled=false;ProductionVisualIntegration.SlavicDistrictProofEnabled=false;ProductionVisualIntegration.CompactFootprintReframeEnabled=true;
            AssetVisualUpliftPassV1.Enabled=false;AssetLibraryReprocessingPassV1.Enabled=true;MidTierDistrictProduction.Enabled=true;ValoriaFullFrameArchitectureBatchV1.Enabled=true;ValoriaFullFrameForegroundEdgePassV1.Enabled=true;ValoriaOpenValleyCompositionV1.Enabled=true;ValoriaReferenceConvergencePassV2.Enabled=false;ValoriaInCitySurfacePassV1.Enabled=false;ValoriaStairLandingIntegrationV1.Enabled=false;ValoriaFullFrameConvergenceIteration1.Enabled=false;ValoriaFullFrameConvergenceIteration2.Enabled=false;ValoriaWorldFrameMountainTerrainV1.Enabled=false;VisualWorld.VisualIntegrationEnabled=true;
            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};VisualWorld.Create(true,state);
            var c=Camera.main;if(c==null)throw new System.Exception("camera missing");
            var p=new Vector3(18.2f,14.6f,-25.8f);var t=new Vector3(0f,3.15f,5.8f);c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=12f;
            var lines=new List<string>();
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                var s=c.WorldToScreenPoint(r.bounds.center);
                if(s.z<=0)continue;
                // Target the right-of-bastion dark mass in the 1280x720 official frame.
                float nx=s.x/Screen.width, ny=s.y/Screen.height;
                // Screen.width is unreliable in batch render; use camera viewport instead.
                var v=c.WorldToViewportPoint(r.bounds.center);
                if(v.z<=0||v.x<.51f||v.x>.78f||v.y<.36f||v.y>.70f)continue;
                string chain="";for(var tr=r.transform;tr!=null;tr=tr.parent)chain+=" > "+tr.name;
                string mats="";foreach(var m in r.sharedMaterials)if(m!=null)mats+=m.name+";";
                lines.Add($"viewport={v.x:F3},{v.y:F3} depth={v.z:F2} bounds={r.bounds.size} renderer={r.gameObject.name} mats={mats} chain={chain}");
            }
            lines.Sort();
            File.WriteAllLines(Folder+"/screen-region-renderers.txt",lines);
            Debug.Log("VALORIA_DARK_MASS_SCREEN_DIAGNOSTIC_V2="+lines.Count);
            EditorApplication.Exit(0);
        }
    }
}
