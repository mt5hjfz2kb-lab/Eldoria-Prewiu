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
    // Isolated camera/depth proof. Does not change production camera or gameplay.
    public static class ValoriaPerspectivePanoramaGateV1
    {
        const string Folder="ValoriaPerspectivePanoramaV1Captures";

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
            ValoriaPanoramicSkyCandidateV1.Enabled=false;
            ValoriaPanoramicSkyCandidateV2.Enabled=false;
            ValoriaWorldFrameMountainTerrainV1.Enabled=true;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);
            var c=Camera.main;
            var root=GameObject.Find("Valoria · integrated construction visual layer");
            if(c==null||root==null)throw new System.Exception("Valoria capture prerequisites missing.");

            var baseline=ValoriaVisualFormulaGate.CollisionSignature();
            var orthoPos=new Vector3(18.2f,14.6f,-25.8f);
            var target=new Vector3(0f,3.15f,5.8f);
            SaveOrtho(c,Folder+"/before-ortho-12.png",orthoPos,target,12f,1280,720);
            SaveOrtho(c,Folder+"/before-ortho-mobile.png",orthoPos,target,12f,390,844);
            Metrics(Folder+"/before-metrics.json");

            var heroValley=GameObject.Find("Valoria · Hero Frame valley terrain");
            if(heroValley!=null)Object.DestroyImmediate(heroValley);
            var worldFrame=GameObject.Find("Valoria · World Frame Mountain Terrain v1");
            if(worldFrame!=null)Object.DestroyImmediate(worldFrame);
            ValoriaWorldFrameMountainTerrainV1.Enabled=false;

            ValoriaPanoramicSkyCandidateV2.Enabled=true;
            float[] rotations={0f,90f,180f,270f};
            foreach(var rotation in rotations)
            {
                ValoriaPanoramicSkyCandidateV2.Rotation=rotation;
                if(!ValoriaPanoramicSkyCandidateV2.Apply())
                    throw new System.Exception("Alps Field panorama unavailable.");

                string tag=((int)rotation).ToString();
                var pos=new Vector3(20.0f,15.7f,-29.2f);
                var look=new Vector3(-.35f,3.55f,5.3f);
                SavePerspective(c,Folder+"/perspective-rot"+tag+"-fov28.png",pos,look,28f,1280,720);
                SavePerspective(c,Folder+"/perspective-rot"+tag+"-fov32.png",new Vector3(21.8f,16.4f,-31.4f),look,32f,1280,720);
                SavePerspective(c,Folder+"/perspective-rot"+tag+"-mobile.png",pos,look,30f,390,844);
            }

            Physics.SyncTransforms();
            if(ValoriaVisualFormulaGate.CollisionSignature()!=baseline)
                throw new System.Exception("Perspective/panorama proof altered gameplay signature.");

            Metrics(Folder+"/after-metrics.json");
            File.WriteAllText(Folder+"/evidence.json",
                "{\n"+
                "  \"baseline_camera\": \"orthographic\",\n"+
                "  \"candidate_camera\": \"perspective fov 28/32\",\n"+
                "  \"camera_change_production\": false,\n"+
                "  \"panorama\": \"Poly Haven Alps Field CC0\",\n"+
                "  \"hero_valley_removed_for_candidate\": true,\n"+
                "  \"world_frame_removed_for_candidate\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"tripo_credits\": 0\n"+
                "}\n");
            EditorApplication.Exit(0);
        }

        static void SaveOrtho(Camera c,string path,Vector3 p,Vector3 t,float size,int w,int h)
        {
            c.transform.position=p;c.transform.LookAt(t);c.orthographic=true;c.orthographicSize=size;Render(c,path,w,h);
        }
        static void SavePerspective(Camera c,string path,Vector3 p,Vector3 t,float fov,int w,int h)
        {
            c.transform.position=p;c.transform.LookAt(t);c.orthographic=false;c.fieldOfView=fov;c.nearClipPlane=.3f;c.farClipPlane=500f;Render(c,path,w,h);
        }
        static void Render(Camera c,string path,int w,int h)
        {
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
