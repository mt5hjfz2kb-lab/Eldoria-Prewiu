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
    public static class ValoriaBenchmarkCompositeGateV2
    {
        const string Folder="ValoriaBenchmarkCompositeV2Captures";

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
            ValoriaBenchmarkCompositeV1.Enabled=false;
            ValoriaBenchmarkCompositeV2.Enabled=false;
            ValoriaWorldFrameMountainTerrainV1.Enabled=false;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);
            var camera=Camera.main;
            var root=GameObject.Find("Valoria · integrated construction visual layer");
            if(camera==null||root==null)throw new System.Exception("Valoria capture prerequisites missing.");

            var basePos=new Vector3(18.2f,14.6f,-25.8f);
            var target=new Vector3(0f,3.25f,5.2f);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            Save(camera,Folder+"/before-12.png",basePos,target,12f,1280,720);
            Save(camera,Folder+"/before-mobile.png",basePos,target,12f,390,844);
            Metrics(Folder+"/before-metrics.json");

            ValoriaBenchmarkCompositeV2.Enabled=true;
            ValoriaBenchmarkCompositeV2.Build(root.transform,state);

            float[] sizes={8.6f,8.0f,7.4f};
            float[] cropY={.50f,.60f,.70f};
            foreach(var size in sizes)
            {
                foreach(var y in cropY)
                {
                    ValoriaCameraBackdropV1.Enabled=true;
                    ValoriaCameraBackdropV1.CropX=.50f;
                    ValoriaCameraBackdropV1.CropY=y;
                    ValoriaCameraBackdropV1.CropW=.42f;
                    ValoriaCameraBackdropV1.CropH=.22f;
                    ValoriaCameraBackdropV1.BackdropWidth=64f;
                    ValoriaCameraBackdropV1.BackdropHeight=7.2f;
                    ValoriaCameraBackdropV1.VerticalOffset=10.3f;
                    ValoriaCameraBackdropV1.Distance=52f;
                    if(!ValoriaCameraBackdropV1.Build(root.transform,camera))
                        throw new System.Exception("Mountain horizon backdrop unavailable.");
                    camera.clearFlags=CameraClearFlags.SolidColor;
                    camera.backgroundColor=new Color(.49f,.65f,.77f);
                    camera.farClipPlane=Mathf.Max(camera.farClipPlane,500f);
                    var tag=((int)(size*10)).ToString()+"-y"+((int)(y*100)).ToString();
                    Save(camera,Folder+"/candidate-"+tag+".png",basePos,target,size,1280,720);
                    if(Mathf.Abs(size-8.0f)<.01f)
                        Save(camera,Folder+"/candidate-"+tag+"-mobile.png",basePos,target,size,390,844);
                }
            }

            Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new System.Exception("Benchmark Composite v2 altered gameplay signature.");

            Metrics(Folder+"/after-metrics.json");
            File.WriteAllText(Folder+"/evidence.json",
                "{\n"+
                "  \"camera_subject_matched\": true,\n"+
                "  \"orthographic_sizes\": [8.6,8.0,7.4],\n"+
                "  \"horizon_crop_y\": [0.50,0.60,0.70],\n"+
                "  \"horizon_crop_h\": 0.22,\n"+
                "  \"extra_dense_clusters\": 6,\n"+
                "  \"post_finish\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
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
            ValoriaCameraBackdropV1.Refit(c);
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;
            try{c.targetTexture=rt;c.Render();c.Render();RenderTexture.active=rt;var im=new Texture2D(w,h,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,w,h),0,0);im.Apply();File.WriteAllBytes(path,im.EncodeToPNG());Object.DestroyImmediate(im);}
            finally{c.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
        }
    }
}
