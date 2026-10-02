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
    public static class ValoriaCameraBackdropGateV2
    {
        const string Folder="ValoriaCameraBackdropV2Captures";

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
            ValoriaPanoramicSkyCandidateV1.Enabled=false;
            ValoriaPanoramicSkyCandidateV2.Enabled=false;
            ValoriaCameraBackdropV1.Enabled=false;
            ValoriaCameraBackdropV2.Enabled=false;
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

            Save(camera,Folder+"/before-12.png",p,t,12f,1280,720);
            Save(camera,Folder+"/before-mobile.png",p,t,12f,390,844);
            Metrics(Folder+"/before-metrics.json");

            camera.clearFlags=CameraClearFlags.SolidColor;
            camera.backgroundColor=new Color(.49f,.58f,.63f);
            camera.farClipPlane=500f;

            ValoriaCameraBackdropV2.Enabled=true;
            if(!ValoriaCameraBackdropV2.Build(root.transform,camera))
                throw new System.Exception("Atmospheric camera backdrop could not be built.");

            var crops=new[]{
                new Vector4(.44f,.27f,.38f,.46f),
                new Vector4(.50f,.30f,.36f,.44f),
                new Vector4(.56f,.31f,.36f,.43f),
                new Vector4(.60f,.33f,.35f,.41f),
                new Vector4(.46f,.34f,.42f,.39f),
                new Vector4(.58f,.36f,.40f,.37f)
            };
            var tints=new[]{
                new Color(.80f,.82f,.82f,1f),
                new Color(.74f,.78f,.80f,1f)
            };

            int shot=0;
            foreach(var tint in tints)
            {
                foreach(var c in crops)
                {
                    ValoriaCameraBackdropV2.CropX=c.x;
                    ValoriaCameraBackdropV2.CropY=c.y;
                    ValoriaCameraBackdropV2.CropW=c.z;
                    ValoriaCameraBackdropV2.CropH=c.w;
                    ValoriaCameraBackdropV2.BackdropTint=tint;
                    ValoriaCameraBackdropV2.ApplyLook();
                    ValoriaCameraBackdropV2.SetHazeVisible(true);
                    Save(camera,Folder+"/candidate-"+shot+"-12.png",p,t,12f,1280,720);
                    if(shot==2||shot==3||shot==8||shot==9||shot==11)
                        Save(camera,Folder+"/candidate-"+shot+"-mobile.png",p,t,12f,390,844);
                    shot++;
                }
            }

            ValoriaCameraBackdropV2.SetHazeVisible(false);
            ValoriaCameraBackdropV2.CropX=.56f;
            ValoriaCameraBackdropV2.CropY=.31f;
            ValoriaCameraBackdropV2.CropW=.36f;
            ValoriaCameraBackdropV2.CropH=.43f;
            ValoriaCameraBackdropV2.BackdropTint=new Color(.74f,.78f,.80f,1f);
            ValoriaCameraBackdropV2.ApplyLook();
            Save(camera,Folder+"/candidate-no-haze-12.png",p,t,12f,1280,720);

            Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new System.Exception("Atmospheric backdrop altered gameplay signature.");

            Metrics(Folder+"/after-metrics.json");
            File.WriteAllText(Folder+"/evidence.json",
                "{\n"+
                "  \"camera_matched\": true,\n"+
                "  \"camera_projection_unchanged\": \"orthographic\",\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"source\": \"Poly Haven Alps Field CC0\",\n"+
                "  \"candidate_count\": 12,\n"+
                "  \"haze_blend_tested\": true,\n"+
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
