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
    public static class ValoriaCameraBackdropGateV3
    {
        const string Folder="ValoriaCameraBackdropV3Captures";

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
            ValoriaCameraBackdropV1.Enabled=false;
            ValoriaCameraBackdropV2.Enabled=false;
            ValoriaCameraBackdropV3.Enabled=false;
            ValoriaWorldFrameMountainTerrainV1.Enabled=false;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);

            var camera=Camera.main;
            var root=GameObject.Find("Valoria · integrated construction visual layer");
            if(camera==null||root==null)throw new System.Exception("Valoria capture prerequisites missing.");
            var p=new Vector3(18.2f,14.6f,-25.8f);
            var t=new Vector3(0f,3.15f,5.8f);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            Save(camera,Folder+"/before-12.png",p,t,12f,1280,720,null);
            Save(camera,Folder+"/before-mobile.png",p,t,12f,390,844,null);
            Metrics(Folder+"/before-metrics.json");

            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.49f,.58f,.63f);
            camera.farClipPlane=500f;
            ValoriaCameraBackdropV3.Enabled=true;
            if(!ValoriaCameraBackdropV3.Build(root.transform,camera))
                throw new System.Exception("Adaptive stylized backdrop could not be built.");

            var desktopCrops=new[]{
                new Vector4(.54f,.30f,.36f,.44f),
                new Vector4(.58f,.31f,.35f,.43f),
                new Vector4(.60f,.33f,.35f,.41f),
                new Vector4(.62f,.35f,.34f,.39f)
            };
            for(int i=0;i<desktopCrops.Length;i++)
            {
                ValoriaCameraBackdropV3.DesktopCrop=desktopCrops[i];
                Save(camera,Folder+"/desktop-"+i+"-12.png",p,t,12f,1280,720,16f/9f);
            }

            var mobileCrops=new[]{
                new Vector4(.12f,.24f,.44f,.50f),
                new Vector4(.18f,.25f,.44f,.49f),
                new Vector4(.24f,.27f,.42f,.47f),
                new Vector4(.30f,.28f,.40f,.46f),
                new Vector4(.36f,.29f,.38f,.45f),
                new Vector4(.42f,.30f,.36f,.44f)
            };
            for(int i=0;i<mobileCrops.Length;i++)
            {
                ValoriaCameraBackdropV3.MobileCrop=mobileCrops[i];
                Save(camera,Folder+"/mobile-"+i+".png",p,t,12f,390,844,390f/844f);
            }

            // Matched combined evidence using one desktop + one portrait crop.
            ValoriaCameraBackdropV3.DesktopCrop=new Vector4(.60f,.33f,.35f,.41f);
            ValoriaCameraBackdropV3.MobileCrop=new Vector4(.24f,.27f,.42f,.47f);
            Save(camera,Folder+"/selected-desktop-12.png",p,t,12f,1280,720,16f/9f);
            Save(camera,Folder+"/selected-mobile.png",p,t,12f,390,844,390f/844f);

            Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new System.Exception("Adaptive stylized backdrop altered gameplay signature.");

            Metrics(Folder+"/after-metrics.json");
            File.WriteAllText(Folder+"/evidence.json",
                "{\n"+
                "  \"camera_projection_unchanged\": \"orthographic\",\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"source\": \"Poly Haven Alps Field CC0\",\n"+
                "  \"source_preprocessed_for_distance\": true,\n"+
                "  \"stylized_resolution\": \"384x192\",\n"+
                "  \"aspect_adaptive_crop\": true,\n"+
                "  \"desktop_candidates\": 4,\n"+
                "  \"mobile_candidates\": 6,\n"+
                "  \"tripo_credits\": 0\n"+
                "}\n");
            EditorApplication.Exit(0);
        }

        static void Save(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h,float? aspect)
        {
            c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=size;
            if(aspect.HasValue)ValoriaCameraBackdropV3.ApplyForAspect(aspect.Value);
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;
            try{c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);}
            finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
        }

        static void Metrics(string path)
        {
            int rs=0,ls=0;long tri=0;var ms=new HashSet<string>();
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;rs++;foreach(var m in r.sharedMaterials)if(m!=null)ms.Add(m.name);}
            foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)){if(mf==null||mf.sharedMesh==null||!mf.gameObject.activeInHierarchy)continue;var rr=mf.GetComponent<Renderer>();if(rr!=null&&!rr.enabled)continue;for(int s=0;s<mf.sharedMesh.subMeshCount;s++)tri+=(long)mf.sharedMesh.GetIndexCount(s)/3L;}
            foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(l!=null&&l.enabled&&l.gameObject.activeInHierarchy)ls++;
            File.WriteAllText(path,$"{{\n  \"active_renderers\": {rs},\n  \"unique_materials\": {ms.Count},\n  \"scene_triangles\": {tri},\n  \"active_lights\": {ls}\n}}\n");
        }
    }
}
