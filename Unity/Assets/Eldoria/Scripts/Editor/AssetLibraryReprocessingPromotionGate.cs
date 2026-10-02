using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools
{
    public static class AssetLibraryReprocessingPromotionGate
    {
        const string Folder="AssetLibraryReprocessingPromotionCaptures";

        public static void Capture()
        {
            UnityEditor.ShaderUtil.allowAsyncCompilation=false;
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
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};

            AssetLibraryReprocessingPassV1.Enabled=false;
            VisualWorld.Create(true,state);
            var camera=Camera.main;
            if(camera==null)throw new System.Exception("Valoria camera missing.");
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();
            var before=Metrics();

            var position=new Vector3(18.2f,14.6f,-25.8f);
            var target=new Vector3(0f,3.15f,5.8f);
            Save(camera,Folder+"/before-19.png",position,target,19f,1280,720);
            Save(camera,Folder+"/before-12.png",position,target,12f,1280,720);
            Save(camera,Folder+"/before-9.png",position,target,9f,1280,720);
            Save(camera,Folder+"/before-mobile.png",position,target,12f,390,844);

            AssetLibraryReprocessingPassV1.BuildForGate(state);
            Physics.SyncTransforms();
            var afterSignature=ValoriaVisualFormulaGate.CollisionSignature();
            if(afterSignature!=baseline)throw new System.Exception("Gameplay collider/hotspot signature changed.");
            var after=Metrics();

            Save(camera,Folder+"/after-19.png",position,target,19f,1280,720);
            Save(camera,Folder+"/after-12.png",position,target,12f,1280,720);
            Save(camera,Folder+"/after-9.png",position,target,9f,1280,720);
            Save(camera,Folder+"/after-mobile.png",position,target,12f,390,844);

            File.WriteAllText(Folder+"/evidence.json",
                "{\n"+
                "  \"camera_matched\": true,\n"+
                "  \"same_scene_before_after\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"gameplay_topology_changed\": false,\n"+
                "  \"geometry_gap_proven\": false,\n"+
                "  \"tripo_credits\": 0,\n"+
                "  \"before\": "+before+",\n"+
                "  \"after\": "+after+"\n"+
                "}\n");
            Debug.Log("ASSET_LIBRARY_REPROCESSING_PROMOTION_GATE=PASS");
            UnityEditor.EditorApplication.Exit(0);
        }

        static string Metrics()
        {
            int renderers=0,lights=0;long triangles=0;var materials=new HashSet<string>();
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                renderers++;
                foreach(var m in r.sharedMaterials)if(m!=null)materials.Add(m.name);
            }
            foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
            {
                if(mf==null||!mf.gameObject.activeInHierarchy)continue;
                var rr=mf.GetComponent<Renderer>();if(rr!=null&&!rr.enabled)continue;
                var mesh=mf.sharedMesh;if(mesh==null)continue;
                for(int s=0;s<mesh.subMeshCount;s++)triangles+=(long)mesh.GetIndexCount(s)/3L;
            }
            foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if(l!=null&&l.enabled&&l.gameObject.activeInHierarchy)lights++;
            return "{\"active_renderers\":"+renderers+",\"unique_materials\":"+materials.Count+",\"scene_triangles\":"+triangles+",\"active_lights\":"+lights+"}";
        }

        static void Save(Camera camera,string path,Vector3 position,Vector3 target,float size,int width,int height)
        {
            camera.transform.position=position;camera.transform.LookAt(target);camera.orthographic=true;camera.orthographicSize=size;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);var prev=RenderTexture.active;
            try
            {
                camera.targetTexture=rt;
                var warmed=new HashSet<Material>();
                foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                    if(renderer.enabled)foreach(var material in renderer.sharedMaterials)
                        if(material!=null&&warmed.Add(material))
                            for(int pass=0;pass<material.passCount;pass++)UnityEditor.ShaderUtil.CompilePass(material,pass,true);
                camera.Render();camera.Render();RenderTexture.active=rt;
                var image=new Texture2D(width,height,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());Object.DestroyImmediate(image);
            }
            finally{camera.targetTexture=null;RenderTexture.active=prev;rt.Release();Object.DestroyImmediate(rt);}
        }
    }
}
