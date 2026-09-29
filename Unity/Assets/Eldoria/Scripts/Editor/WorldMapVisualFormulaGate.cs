using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    public static class WorldMapVisualFormulaGate
    {
        public static void Capture()
        {
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var state=new PlayerState
            {
                BastionLevel=2,
                SawmillLevel=1,
                BarracksLevel=1,
                CorruptionDiscovered=true
            };
            VisualWorld.Create(false,state);
            var camera=Camera.main;
            if(camera==null)throw new System.Exception("Frontier camera missing.");

            var forestHotspot=GameObject.Find("Bosque de Valoria · recolectar");
            if(forestHotspot==null||forestHotspot.GetComponent<WorldHotspot>()==null)
                throw new System.Exception("Forest gameplay hotspot missing.");
            var quarry=GameObject.Find("Frontier · quarry resource kit");
            if(quarry==null)throw new System.Exception("Quarry Resource Kit v1 missing.");
            foreach(var c in quarry.GetComponentsInChildren<Collider>(true))
                if(c.enabled)throw new System.Exception("Quarry visual kit must not own gameplay collision.");
            var route=GameObject.Find("Frontier · march route kit");
            if(route==null)throw new System.Exception("World Route Kit v1 missing.");
            foreach(var c in route.GetComponentsInChildren<Collider>(true))
                if(c.enabled)throw new System.Exception("Route visual kit must not own gameplay collision.");

            const string folder="WorldMapVisualFormulaCaptures";
            Directory.CreateDirectory(folder);
            var officialPosition=new Vector3(20f,24f,-21f);
            var officialTarget=new Vector3(0f,0f,1f);

            Save(camera,folder+"/frontier-overview-14.png",officialPosition,officialTarget,14f,1280,720);
            Save(camera,folder+"/frontier-overview-mobile.png",officialPosition,officialTarget,14f,390,844);

            var quarryShift=new Vector3(6.2f,-1.1f,-3.0f);
            Save(camera,folder+"/quarry-kit-10.png",officialPosition+quarryShift,officialTarget+quarryShift,10f,1280,720);
            Save(camera,folder+"/quarry-kit-7.png",officialPosition+quarryShift,officialTarget+quarryShift,7f,1280,720);
            Save(camera,folder+"/quarry-kit-mobile.png",officialPosition+quarryShift,officialTarget+quarryShift,10f,390,844);

            var routeShift=new Vector3(0f,-.35f,-2.2f);
            Save(camera,folder+"/route-kit-10.png",officialPosition+routeShift,officialTarget+routeShift,10f,1280,720);
            Save(camera,folder+"/route-kit-7.png",officialPosition+routeShift,officialTarget+routeShift,7f,1280,720);
            Save(camera,folder+"/route-kit-mobile.png",officialPosition+routeShift,officialTarget+routeShift,10f,390,844);

            int quarryRenderers=0;
            foreach(var r in quarry.GetComponentsInChildren<Renderer>(true))if(r.enabled)quarryRenderers++;
            int routeRenderers=0;
            foreach(var r in route.GetComponentsInChildren<Renderer>(true))if(r.enabled)routeRenderers++;
            File.WriteAllText(folder+"/world-map-metrics.json",
                "{\n"+
                "  \"schema_version\": 2,\n"+
                "  \"quarry_kit\": \"WorldResourceKit.QuarryResourcePocket\",\n"+
                "  \"quarry_active_renderers\": "+quarryRenderers+",\n"+
                "  \"route_kit\": \"WorldRouteKit.MarchRoute\",\n"+
                "  \"route_active_renderers\": "+routeRenderers+",\n"+
                "  \"visual_owns_gameplay_collision\": false,\n"+
                "  \"forest_hotspot_preserved\": true,\n"+
                "  \"source_family\": \"Slavic hard-surface subset + Eldoria procedural terrain\",\n"+
                "  \"tripo_credits\": 0\n"+
                "}\n");
            UnityEditor.EditorApplication.Exit(0);
        }

        static void Save(Camera camera,string path,Vector3 position,Vector3 target,float size,int width,int height)
        {
            camera.transform.position=position;
            camera.transform.LookAt(target);
            camera.orthographic=true;
            camera.orthographicSize=size;
            camera.backgroundColor=RenderSettings.fogColor;
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var previous=RenderTexture.active;
            try
            {
                camera.targetTexture=rt;
                camera.Render();
                RenderTexture.active=rt;
                var image=new Texture2D(width,height,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,width,height),0,0);
                image.Apply();
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
