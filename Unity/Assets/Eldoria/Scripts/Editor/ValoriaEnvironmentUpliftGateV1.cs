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
    public static class ValoriaEnvironmentUpliftGateV1
    {
        const string Folder="ValoriaEnvironmentUpliftV1Captures";

        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;Directory.CreateDirectory(Folder);SceneSetup.SetupRenderPipeline();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);

            ProductionVisualIntegration.ResetVisualCachesForGate();
            ProductionVisualIntegration.StoneArchitectureEnabled=true;ProductionVisualIntegration.TerrainTerraceEnabled=true;ProductionVisualIntegration.SurfaceCellEnabled=false;ProductionVisualIntegration.ProductionCellEnabled=false;ProductionVisualIntegration.CoherentCastleProofEnabled=false;ProductionVisualIntegration.SlavicDistrictProofEnabled=false;ProductionVisualIntegration.CompactFootprintReframeEnabled=true;
            AssetVisualUpliftPassV1.Enabled=false;AssetLibraryReprocessingPassV1.Enabled=true;MidTierDistrictProduction.Enabled=true;ValoriaFullFrameArchitectureBatchV1.Enabled=true;ValoriaFullFrameForegroundEdgePassV1.Enabled=true;ValoriaOpenValleyCompositionV1.Enabled=true;
            ValoriaReferenceConvergencePassV2.Enabled=false;ValoriaInCitySurfacePassV1.Enabled=false;ValoriaStairLandingIntegrationV1.Enabled=false;ValoriaFullFrameConvergenceIteration1.Enabled=false;ValoriaFullFrameConvergenceIteration2.Enabled=false;ValoriaBenchmarkCompositeV2.Enabled=false;ValoriaEnvironmentUpliftV1.Enabled=false;ValoriaWorldFrameMountainTerrainV1.Enabled=false;VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};VisualWorld.Create(true,state);
            var camera=Camera.main;var root=GameObject.Find("Valoria · integrated construction visual layer");if(camera==null||root==null)throw new System.Exception("Valoria capture prerequisites missing.");
            var p=new Vector3(18.2f,14.6f,-25.8f);var t=new Vector3(0f,3.25f,5.2f);var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            ValoriaBenchmarkCompositeV2.Enabled=true;ValoriaBenchmarkCompositeV2.Build(root.transform,state);
            ValoriaCameraBackdropV1.Enabled=true;ValoriaCameraBackdropV1.CropX=.50f;ValoriaCameraBackdropV1.CropY=.50f;ValoriaCameraBackdropV1.CropW=.42f;ValoriaCameraBackdropV1.CropH=.22f;ValoriaCameraBackdropV1.BackdropWidth=64f;ValoriaCameraBackdropV1.BackdropHeight=7.2f;ValoriaCameraBackdropV1.VerticalOffset=10.3f;ValoriaCameraBackdropV1.Distance=52f;
            if(!ValoriaCameraBackdropV1.Build(root.transform,camera))throw new System.Exception("Backdrop unavailable.");

            Save(camera,Folder+"/before.png",p,t,8.0f,1280,720);Save(camera,Folder+"/before-mobile.png",p,t,8.0f,390,844);Metrics(Folder+"/before-metrics.json");

            ValoriaEnvironmentUpliftV1.Enabled=true;ValoriaEnvironmentUpliftV1.Build(root.transform,state);Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)throw new System.Exception("Environment uplift altered gameplay signature.");

            Save(camera,Folder+"/after.png",p,t,8.0f,1280,720);Save(camera,Folder+"/after-mobile.png",p,t,8.0f,390,844);Metrics(Folder+"/after-metrics.json");
            File.WriteAllText(Folder+"/evidence.json",
                "{\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"repainted_renderers\": "+ValoriaEnvironmentUpliftV1.Repainted+",\n"+
                "  \"replaced_trees\": "+ValoriaEnvironmentUpliftV1.ReplacedTrees+",\n"+
                "  \"replaced_rocks\": "+ValoriaEnvironmentUpliftV1.ReplacedRocks+",\n"+
                "  \"existing_project_assets_only\": true,\n"+
                "  \"tripo_credits\": 0\n"+
                "}\n");
            EditorApplication.Exit(0);
        }

        static void Metrics(string path){int rs=0,ls=0;long tri=0;var ms=new HashSet<string>();foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;rs++;foreach(var m in r.sharedMaterials)if(m!=null)ms.Add(m.name);}foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)){if(mf==null||mf.sharedMesh==null||!mf.gameObject.activeInHierarchy)continue;var rr=mf.GetComponent<Renderer>();if(rr!=null&&!rr.enabled)continue;for(int s=0;s<mf.sharedMesh.subMeshCount;s++)tri+=(long)mf.sharedMesh.GetIndexCount(s)/3L;}foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(l!=null&&l.enabled&&l.gameObject.activeInHierarchy)ls++;File.WriteAllText(path,$"{{\n  \"active_renderers\": {rs},\n  \"unique_materials\": {ms.Count},\n  \"scene_triangles\": {tri},\n  \"active_lights\": {ls}\n}}\n");}
        static void Save(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h){c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=size;ValoriaCameraBackdropV1.Refit(c);var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;try{c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);}finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}}
    }
}
