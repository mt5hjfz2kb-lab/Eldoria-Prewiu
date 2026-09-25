using System.IO;
using Eldoria.Domain;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Eldoria.EditorTools
{
    // A deterministic, HUD-free art review of the actual runtime Valoria renderer.
    // Run without -nographics: Camera.Render needs a graphics device on the Windows runner.
    public static class ValoriaBenchmarkCapture
    {
        public static void Capture()
        {
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var state=new PlayerState { BastionLevel=2,SawmillLevel=1,BarracksLevel=1,CorruptionDiscovered=true };
            VisualWorld.Create(true,state);
            var camera=Camera.main;
            if(camera==null)throw new System.Exception("Valoria camera was not created");
            const string folder="BenchmarkCaptures";
            Directory.CreateDirectory(folder);
            Save(camera,folder+"/valoria-establishing.png",new Vector3(20.5f,16.8f,-25.8f),new Vector3(0,2.6f,4.0f),34f,1280,720);
            Save(camera,folder+"/valoria-gate.png",new Vector3(8.4f,10.6f,-18.8f),new Vector3(0,2.5f,3.8f),31f,1280,720);
            Save(camera,folder+"/valoria-districts.png",new Vector3(-16.2f,13.2f,-13.6f),new Vector3(0,2.15f,2.4f),32f,1280,720);
            Debug.Log("Valoria benchmark captures saved to "+Path.GetFullPath(folder));
        }

        static void Save(Camera camera,string path,Vector3 position,Vector3 target,float fieldOfView,int width,int height)
        {
            camera.transform.position=position;
            camera.transform.LookAt(target);
            camera.orthographic=false;
            camera.fieldOfView=fieldOfView;
            var targetTexture=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);
            var previous=RenderTexture.active;
            try
            {
                camera.targetTexture=targetTexture;
                camera.Render();
                RenderTexture.active=targetTexture;
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
                targetTexture.Release();
                Object.DestroyImmediate(targetTexture);
            }
        }
    }
}
