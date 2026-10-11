using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

// Runner arbitration checkpoint v5: Editor-only convergence changes must not reserve the heavy Unity slice runner.
// Runner arbitration checkpoint v3: convergence gate changes are owned by the dedicated lightweight proof workflow.
namespace Eldoria.EditorTools
{
    public static class ValoriaReferenceConvergenceGateV2
    {
        const string Folder="ValoriaReferenceConvergenceV2Captures";

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
            ValoriaReferenceConvergencePassV2.Enabled=false;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);
            var camera=Camera.main;
            if(camera==null)throw new System.Exception("Valoria camera missing.");

            var position=new Vector3(18.2f,14.6f,-25.8f);
            var target=new Vector3(0f,3.15f,5.8f);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();
            WriteMetrics(Folder+"/before-metrics.json");

            Save(camera,Folder+"/before-19.png",position,target,19f,1280,720);
            Save(camera,Folder+"/before-12.png",position,target,12f,1280,720);
            Save(camera,Folder+"/before-9.png",position,target,9f,1280,720);
            Save(camera,Folder+"/before-mobile.png",position,target,12f,390,844);

            var root=GameObject.Find("Valoria · integrated construction visual layer");
            if(root==null)throw new System.Exception("Valoria production visual root missing.");
            ValoriaReferenceConvergencePassV2.Enabled=true;
            ValoriaReferenceConvergencePassV2.Build(root.transform,state);
            Physics.SyncTransforms();

            var afterSignature=ValoriaVisualFormulaGate.CollisionSignature();
            if(afterSignature!=baseline)
                throw new System.Exception("Reference Convergence v2 altered gameplay collider/hotspot signature.");

            WriteMetrics(Folder+"/after-metrics.json");
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
                "  \"tripo_credits\": 0\n"+
                "}\n");

            Debug.Log("VALORIA_REFERENCE_CONVERGENCE_V2_GATE=PASS");
            EditorApplication.Exit(0);
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
                // Synchronous rendering is sufficient here. Avoid compiling every pass for every
                // material before each of the eight captures; that made full-frame convergence gates
                // scale with the entire library rather than with the visible frame.
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
