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
    public static class ValoriaMatteBackdropGateV1
    {
        const string Folder="ValoriaMatteBackdropV1Captures";

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
            ValoriaReferenceConvergencePassV2.Enabled=false;
            ValoriaInCitySurfacePassV1.Enabled=false;
            ValoriaStairLandingIntegrationV1.Enabled=false;
            ValoriaFullFrameConvergenceIteration1.Enabled=false;
            ValoriaFullFrameConvergenceIteration2.Enabled=false;
            ValoriaOpenValleyCompositionV1.Enabled=false;
            ValoriaMatteBackdropV1.Enabled=false;
            ValoriaWorldFrameMountainTerrainV1.Enabled=true;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);
            var camera=Camera.main;
            var root=GameObject.Find("Valoria · integrated construction visual layer");
            if(camera==null||root==null)throw new System.Exception("Valoria capture prerequisites missing.");

            var p=new Vector3(18.2f,14.6f,-25.8f);
            var t=new Vector3(0f,3.15f,5.8f);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            Save(camera,Folder+"/before-12.png",p,t,12f,1280,720);
            Save(camera,Folder+"/before-mobile.png",p,t,12f,390,844);
            Metrics(Folder+"/before-metrics.json");

            ValoriaOpenValleyCompositionV1.Enabled=true;
            ValoriaOpenValleyCompositionV1.Build(root.transform,state);
            ValoriaMatteBackdropV1.Enabled=true;

            var variants=new[]{
                new Rect(0.00f,.24f,.50f,.50f),
                new Rect(.125f,.24f,.50f,.50f),
                new Rect(.25f,.24f,.50f,.50f),
                new Rect(.375f,.24f,.50f,.50f),
                new Rect(.00f,.34f,.50f,.42f),
                new Rect(.25f,.34f,.50f,.42f)
            };

            for(int i=0;i<variants.Length;i++)
            {
                ValoriaMatteBackdropV1.UvRect=variants[i];
                if(!ValoriaMatteBackdropV1.Build(root.transform,state))
                    throw new System.Exception("Matte backdrop could not load Alps Field.");
                Save(camera,Folder+"/matte-"+i+"-12.png",p,t,12f,1280,720);
                Save(camera,Folder+"/matte-"+i+"-mobile.png",p,t,12f,390,844);
            }

            Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new System.Exception("Matte backdrop proof altered gameplay signature.");

            Metrics(Folder+"/after-metrics.json");
            File.WriteAllText(Folder+"/evidence.json",
                "{\n"+
                "  \"camera_matched\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"open_valley_enabled\": true,\n"+
                "  \"source\": \"Poly Haven Alps Field\",\n"+
                "  \"license\": \"CC0\",\n"+
                "  \"mode\": \"camera_facing_2_5d_matte\",\n"+
                "  \"variants\": 6,\n"+
                "  \"tripo_credits\": 0\n"+
                "}\n");

            EditorApplication.Exit(0);
        }

        static void Metrics(string path)
        {
            int rs=0,ls=0;long tri=0;var ms=new HashSet<string>();
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;rs++;foreach(var m in r.sharedMaterials)if(m!=null)ms.Add(m.name);}
            foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)){if(mf==null||mf.sharedMesh==null||!mf.gameObject.activeInHierarchy)continue;var rr=mf.GetComponent<Renderer>();if(rr!=null&&!rr.enabled)continue;for(int s=0;s<mf.sharedMesh.subMeshCount;s++)tri+=(long)mf.sharedMesh.GetIndexCount(s)/3L;}
            foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(l!=null&&l.enabled&&l.gameObject.activeInHierarchy)ls++;
            File.WriteAllText(path,$"{{\n  \"active_renderers\": {rs},\n  \"unique_materials\": {ms.Count},\n  \"scene_triangles\": {tri},\n  \"active_lights\": {ls}\n}}\n");
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
