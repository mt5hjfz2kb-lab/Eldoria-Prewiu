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
            var officialPosition=new Vector3(18.2f,14.6f,-25.8f);
            var officialTarget=new Vector3(0,3.15f,5.8f);
            Save(camera,folder+"/valoria-establishing.png",officialPosition,officialTarget,19f,1280,720);
            Save(camera,folder+"/valoria-gate.png",officialPosition,officialTarget,12f,1280,720);
            Save(camera,folder+"/valoria-districts.png",officialPosition,officialTarget,9f,1280,720);
            Debug.Log("Valoria benchmark captures saved to "+Path.GetFullPath(folder));
        }

        static void Save(Camera camera,string path,Vector3 position,Vector3 target,float size,int width,int height)
        {
            camera.transform.position=position;
            camera.transform.LookAt(target);
            camera.orthographic=true;
            camera.orthographicSize=size;
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
