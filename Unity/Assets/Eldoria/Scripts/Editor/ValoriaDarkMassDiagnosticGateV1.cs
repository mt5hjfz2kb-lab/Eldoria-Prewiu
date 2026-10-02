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
    public static class ValoriaDarkMassDiagnosticGateV1
    {
        const string Folder="ValoriaDarkMassDiagnosticV1Captures";

        static readonly string[] Candidates={
            "Valoria · rescued hero flank",
            "Valoria · Bastion hero wall west",
            "Valoria · Bastion hero tower west",
            "Valoria · Bastion hero wall rear",
            "Valoria · Bastion hero tower crown",
            "Valoria · Bastion hero gate"
        };

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
            ValoriaWorldFrameMountainTerrainV1.Enabled=false;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);
            var c=Camera.main;
            if(c==null)throw new System.Exception("Valoria camera missing.");

            var p=new Vector3(18.2f,14.6f,-25.8f);
            var t=new Vector3(0f,3.15f,5.8f);
            Save(c,Folder+"/baseline.png",p,t,12f,1280,720);

            var report=new List<string>();
            foreach(var name in Candidates)
            {
                var renderers=FindChainRenderers(name);
                report.Add(name+" renderers="+renderers.Count);
                foreach(var r in renderers)r.enabled=false;
                Save(c,Folder+"/hide-"+Safe(name)+".png",p,t,12f,1280,720);
                foreach(var r in renderers)r.enabled=true;
            }

            // Also record screen-space centres for every renderer whose ancestor is one of the candidates.
            foreach(var name in Candidates)
            {
                foreach(var r in FindChainRenderers(name))
                {
                    var s=c.WorldToScreenPoint(r.bounds.center);
                    report.Add(name+" | "+r.gameObject.name+" | screen="+s.x.ToString("F1")+","+s.y.ToString("F1")+" | bounds="+r.bounds.size);
                }
            }
            File.WriteAllLines(Folder+"/report.txt",report);
            EditorApplication.Exit(0);
        }

        static List<Renderer> FindChainRenderers(string exact)
        {
            var list=new List<Renderer>();
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                for(var t=r.transform;t!=null;t=t.parent)
                {
                    if(t.name==exact){list.Add(r);break;}
                }
            }
            return list;
        }

        static string Safe(string s)
        {
            foreach(var c in Path.GetInvalidFileNameChars())s=s.Replace(c,'_');
            return s.Replace(' ','_').Replace('·','-');
        }

        static void Save(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h)
        {
            c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=size;
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;
            try{c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);}
            finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
        }
    }
}
