using System.Collections.Generic;
using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    public static class AssetLibraryReprocessingPassV1Gate
    {
        public static void Capture()
        {
            UnityEditor.ShaderUtil.allowAsyncCompilation=false;
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);

            var state=new PlayerState{BastionLevel=3,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true};
            VisualWorld.VisualIntegrationEnabled=true;
            AssetLibraryReprocessingPassV1.Enabled=false;
            VisualWorld.Create(true,state);

            var camera=Camera.main;
            if(camera==null)throw new System.Exception("Valoria camera missing for Asset Library Reprocessing v1 gate.");
            const string folder="AssetLibraryReprocessingCaptures";
            Directory.CreateDirectory(folder);
            var position=new Vector3(18.2f,14.6f,-25.8f);
            var target=new Vector3(0f,3.15f,5.8f);
            var baseline=ValoriaVisualFormulaGate.CollisionSignature();

            Save(camera,folder+"/before-19.png",position,target,19f,1280,720);
            Save(camera,folder+"/before-12.png",position,target,12f,1280,720);
            Save(camera,folder+"/before-9.png",position,target,9f,1280,720);
            Save(camera,folder+"/before-mobile-390x844.png",position,target,12f,390,844);
            var before=Metrics();

            AssetLibraryReprocessingPassV1.BuildForGate(state);
            Physics.SyncTransforms();
            var afterSignature=ValoriaVisualFormulaGate.CollisionSignature();
            if(afterSignature!=baseline)throw new System.Exception("Asset Library Reprocessing v1 altered certified colliders/hotspots.");

            Save(camera,folder+"/after-19.png",position,target,19f,1280,720);
            Save(camera,folder+"/after-12.png",position,target,12f,1280,720);
            Save(camera,folder+"/after-9.png",position,target,9f,1280,720);
            Save(camera,folder+"/after-mobile-390x844.png",position,target,12f,390,844);
            var after=Metrics();

            File.WriteAllText(folder+"/evidence.json",
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

            AssetLibraryReprocessingPassV1.Enabled=true;
            Debug.Log("ASSET_LIBRARY_REPROCESSING_PASS_V1_GATE=PASS");
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
                var renderer=mf.GetComponent<Renderer>();
                if(renderer!=null&&!renderer.enabled)continue;
                var mesh=mf.sharedMesh;if(mesh==null)continue;
                for(int s=0;s<mesh.subMeshCount;s++)triangles+=(long)mesh.GetIndexCount(s)/3L;
            }
            foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(l!=null&&l.enabled&&l.gameObject.activeInHierarchy)lights++;
            return "{\"active_renderers\":"+renderers+",\"unique_materials\":"+materials.Count+",\"scene_triangles\":"+triangles+",\"active_lights\":"+lights+"}";
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
                foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                    if(renderer.enabled)foreach(var material in renderer.sharedMaterials)
                        if(material!=null&&warmed.Add(material))
                            for(int pass=0;pass<material.passCount;pass++)
                                UnityEditor.ShaderUtil.CompilePass(material,pass,true);
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
