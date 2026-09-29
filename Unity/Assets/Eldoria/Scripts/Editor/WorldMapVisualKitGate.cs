using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    public static class WorldMapVisualKitGate
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
            if(camera==null)throw new System.Exception("Frontier camera was not created");
            var forest=GameObject.Find("Frontier · forest resource kit");
            if(forest==null)throw new System.Exception("Forest Resource Kit v1 missing from Frontier.");
            foreach(var collider in forest.GetComponentsInChildren<Collider>(true))
                if(collider.enabled)throw new System.Exception("Forest Resource Kit visual must not own gameplay collision.");
            var hotspot=GameObject.Find("Bosque de Valoria · recolectar");
            if(hotspot==null||hotspot.GetComponent<WorldHotspot>()==null||hotspot.GetComponent<WorldHotspot>().Id!="forest-valoria")
                throw new System.Exception("Forest gameplay hotspot must remain independent and authoritative.");

            const string folder="WorldMapVisualKitCaptures";
            Directory.CreateDirectory(folder);
            var officialPosition=new Vector3(20f,24f,-21f);
            var officialTarget=new Vector3(0f,0f,1f);
            Save(camera,folder+"/world-overview-14.png",officialPosition,officialTarget,14f,1280,720);
            Save(camera,folder+"/world-overview-mobile.png",officialPosition,officialTarget,14f,390,844);

            var forestShift=new Vector3(-6.25f,-1.2f,0.6f);
            Save(camera,folder+"/forest-kit-10.png",officialPosition+forestShift,officialTarget+forestShift,10f,1280,720);
            Save(camera,folder+"/forest-kit-7.png",officialPosition+forestShift,officialTarget+forestShift,7f,1280,720);
            Save(camera,folder+"/forest-kit-mobile.png",officialPosition+forestShift,officialTarget+forestShift,10f,390,844);

            int renderers=0;
            foreach(var r in forest.GetComponentsInChildren<Renderer>(true))if(r.enabled)renderers++;
            File.WriteAllText(folder+"/forest-kit-metrics.json",
                "{\n"+
                "  \"schema_version\": 1,\n"+
                "  \"kit\": \"WorldNatureKit.ForestResourcePocket\",\n"+
                "  \"active_renderers\": "+renderers+",\n"+
                "  \"gameplay_hotspot\": \"forest-valoria\",\n"+
                "  \"visual_owns_gameplay_collision\": false,\n"+
                "  \"source_family\": \"Slavic World Free + Eldoria material modulation\",\n"+
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
