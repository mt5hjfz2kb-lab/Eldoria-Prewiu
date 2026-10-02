using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools
{
    // Dedicated closeout gate for ASSET LIBRARY REPROCESSING PASS v1.
    // It intentionally avoids unrelated historical proof cells.
    public static class AssetLibraryReprocessingGateV1
    {
        const string Folder="AssetLibraryReprocessingV1Captures";

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
            AssetLibraryReprocessingPassV1.Enabled=false;
            VisualWorld.VisualIntegrationEnabled=true;

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.Create(true,state);
            var camera=Camera.main;
            if(camera==null)throw new System.Exception("Valoria camera missing.");

            var position=new Vector3(18.2f,14.6f,-25.8f);
            var target=new Vector3(0f,3.15f,5.8f);
            var beforeSignature=ValoriaVisualFormulaGate.CollisionSignature();
            WriteMetrics(Folder+"/asset-reprocess-before-metrics.json");

            Save(camera,Folder+"/asset-reprocess-before-19.png",position,target,19f,1280,720);
            Save(camera,Folder+"/asset-reprocess-before-12.png",position,target,12f,1280,720);
            Save(camera,Folder+"/asset-reprocess-before-9.png",position,target,9f,1280,720);
            Save(camera,Folder+"/asset-reprocess-before-mobile.png",position,target,12f,390,844);

            AssetLibraryReprocessingPassV1.BuildForGate(state);
            var afterSignature=ValoriaVisualFormulaGate.CollisionSignature();
            if(afterSignature!=beforeSignature)
                throw new System.Exception("ASSET LIBRARY REPROCESSING PASS v1 altered certified colliders/hotspots.");

            WriteMetrics(Folder+"/asset-reprocess-after-metrics.json");
            Save(camera,Folder+"/asset-reprocess-after-19.png",position,target,19f,1280,720);
            Save(camera,Folder+"/asset-reprocess-after-12.png",position,target,12f,1280,720);
            Save(camera,Folder+"/asset-reprocess-after-9.png",position,target,9f,1280,720);
            Save(camera,Folder+"/asset-reprocess-after-mobile.png",position,target,12f,390,844);

            File.WriteAllText(Folder+"/asset-reprocess-evidence.json",
                "{\n"+
                "  \"camera_matched\": true,\n"+
                "  \"same_scene_before_after\": true,\n"+
                "  \"collider_hotspot_signature_equal\": true,\n"+
                "  \"gameplay_signature_sha256\": \""+Sha(beforeSignature)+"\",\n"+
                "  \"gameplay_topology_changed\": false,\n"+
                "  \"profile\": \"environment_composition + environment_surface\",\n"+
                "  \"geometry_gap_proven\": false,\n"+
                "  \"tripo_credits\": 0\n"+
                "}\n");

            Debug.Log("ASSET_LIBRARY_REPROCESSING_GATE_V1=PASS");
            EditorApplication.Exit(0);
        }

        static string Sha(string value)
        {
            using var sha=SHA256.Create();
            var bytes=sha.ComputeHash(Encoding.UTF8.GetBytes(value??""));
            var sb=new StringBuilder(bytes.Length*2);
            foreach(var b in bytes)sb.Append(b.ToString("x2"));
            return sb.ToString();
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
                var mesh=mf.sharedMesh;
                for(int s=0;s<mesh.subMeshCount;s++)triangles+=(long)mesh.GetIndexCount(s)/3L;
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
                var warmed=new HashSet<Material>();
                foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                    if(r!=null&&r.enabled)foreach(var m in r.sharedMaterials)
                        if(m!=null&&warmed.Add(m))
                            for(int p=0;p<m.passCount;p++)UnityEditor.ShaderUtil.CompilePass(m,p,true);
                camera.Render();camera.Render();
                RenderTexture.active=rt;
                var image=new Texture2D(width,height,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
                File.WriteAllBytes(path,image.EncodeToPNG());
                Object.DestroyImmediate(image);
            }
            finally
            {
                camera.targetTexture=null;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(rt);
            }
        }
    }
}
