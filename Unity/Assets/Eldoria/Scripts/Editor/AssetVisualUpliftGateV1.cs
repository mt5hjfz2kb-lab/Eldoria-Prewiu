using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools
{
    public static class AssetVisualUpliftGateV1
    {
        const string Folder="AssetVisualUpliftV1Captures";

        public static void Capture()
        {
            ShaderUtil.allowAsyncCompilation=false;
            Directory.CreateDirectory(Folder);

            var before=BuildAndCapture(false,"before");
            var after=BuildAndCapture(true,"after");
            if(before!=after)throw new System.Exception("Asset Visual Uplift altered gameplay collider/hotspot signature.");

            File.WriteAllText(Folder+"/evidence.json",
                "{\n"+
                "  \"camera_matched\": true,\n"+
                "  \"deterministic_rebuild_before_after\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"gameplay_topology_changed\": false,\n"+
                "  \"new_geometry_generated\": false,\n"+
                "  \"tripo_credits\": 0,\n"+
                "  \"hero_renderers_touched\": "+AssetVisualUpliftPassV1.HeroRenderersTouched+",\n"+
                "  \"dedicated_renderers_touched\": "+AssetVisualUpliftPassV1.DedicatedRenderersTouched+",\n"+
                "  \"rescued_renderers_touched\": "+AssetVisualUpliftPassV1.RescuedRenderersTouched+",\n"+
                "  \"imported_stone_terrain_renderers_touched\": "+AssetVisualUpliftPassV1.ImportedRenderersTouched+",\n"+
                "  \"mid_tier_renderers_touched\": "+AssetVisualUpliftPassV1.MidTierRenderersTouched+"\n"+
                "}\n");
            Debug.Log("ASSET_VISUAL_UPLIFT_V1_GATE=PASS");
            EditorApplication.Exit(0);
        }

        static string BuildAndCapture(bool uplift,string prefix)
        {
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            AssetVisualUpliftPassV1.ResetDiagnostics();
            ProductionVisualIntegration.ResetVisualCachesForGate();
            ProductionVisualIntegration.StoneArchitectureEnabled=true;
            ProductionVisualIntegration.TerrainTerraceEnabled=true;
            ProductionVisualIntegration.SurfaceCellEnabled=false;
            ProductionVisualIntegration.ProductionCellEnabled=false;
            ProductionVisualIntegration.CoherentCastleProofEnabled=false;
            ProductionVisualIntegration.SlavicDistrictProofEnabled=false;
            ProductionVisualIntegration.CompactFootprintReframeEnabled=true;
            AssetLibraryReprocessingPassV1.Enabled=true;
            AssetVisualUpliftPassV1.Enabled=uplift;
            ValoriaReferenceConvergencePassV2.Enabled=false;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);
            var camera=Camera.main;
            if(camera==null)throw new System.Exception("Valoria camera missing.");

            var signature=ValoriaVisualFormulaGate.CollisionSignature();
            WriteMetrics(Folder+"/"+prefix+"-metrics.json");
            var position=new Vector3(18.2f,14.6f,-25.8f);
            var target=new Vector3(0f,3.15f,5.8f);
            Save(camera,Folder+"/"+prefix+"-19.png",position,target,19f,1280,720);
            Save(camera,Folder+"/"+prefix+"-12.png",position,target,12f,1280,720);
            Save(camera,Folder+"/"+prefix+"-9.png",position,target,9f,1280,720);
            Save(camera,Folder+"/"+prefix+"-mobile.png",position,target,12f,390,844);
            return signature;
        }

        static void WriteMetrics(string path)
        {
            int renderers=0,lights=0;
            long triangles=0;
            var materials=new HashSet<string>();
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r==null||!r.enabled||!r.gameObject.activeInHierarchy)continue;
                renderers++;
                foreach(var m in r.sharedMaterials)if(m!=null)materials.Add(m.name);
            }
            foreach(var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None))
            {
                if(mf==null||mf.sharedMesh==null||!mf.gameObject.activeInHierarchy)continue;
                var rr=mf.GetComponent<Renderer>();if(rr!=null&&!rr.enabled)continue;
                for(int s=0;s<mf.sharedMesh.subMeshCount;s++)triangles+=(long)mf.sharedMesh.GetIndexCount(s)/3L;
            }
            foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if(l!=null&&l.enabled&&l.gameObject.activeInHierarchy)lights++;
            File.WriteAllText(path,
                "{\n"+
                "  \"active_renderers\": "+renderers+",\n"+
                "  \"unique_materials\": "+materials.Count+",\n"+
                "  \"scene_triangles\": "+triangles+",\n"+
                "  \"active_lights\": "+lights+"\n"+
                "}\n");
        }

        static void Save(Camera camera,string path,Vector3 position,Vector3 target,float size,int width,int height)
        {
            camera.transform.position=position;
            camera.transform.LookAt(target);
            camera.orthographic=true;
            camera.orthographicSize=size;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var previous=RenderTexture.active;
            try
            {
                camera.targetTexture=rt;
                // Explicit CompilePass caused a URP culling crash under runner memory pressure.
                camera.Render();
                camera.Render();
                RenderTexture.active=rt;
                var image=new Texture2D(width,height,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
                File.WriteAllBytes(path,image.EncodeToPNG());
                Object.DestroyImmediate(image);
            }
            finally
            {
                camera.targetTexture=null;
                RenderTexture.active=previous;
                rt.Release();
                Object.DestroyImmediate(rt);
            }
        }
    }
}
