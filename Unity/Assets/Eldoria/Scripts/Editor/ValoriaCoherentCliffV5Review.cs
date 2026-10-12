using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools
{
    // Replacement-comparison, NOT a layer added to the original cliff.
    public static class ValoriaCoherentCliffV5Review
    {
        public static async void Capture()
        {
            try {
                SceneSetup.SetupRenderPipeline();
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                string dir=Path.Combine(UnityEngine.Application.streamingAssetsPath,"ValoriaCoherentCliffV5");
                GameObject frozen=await Load(Path.Combine(dir,"cliff_rocks.glb"),"frozen_cliff");
                GameObject candidate=await Load(Path.Combine(dir,"cliff_coherent_v5.glb"),"coherent_geological_candidate");
                frozen.transform.rotation=Quaternion.Euler(0,180,0);
                candidate.transform.rotation=Quaternion.Euler(0,180,0);
                Bounds source=BoundsFor(frozen), proposed=BoundsFor(candidate);
                // Match the original module pivot and roofline, never rescale the
                // result to hide geometric defects or change a 12m source seam.
                candidate.transform.position+=new Vector3(source.center.x-proposed.center.x,
                    source.max.y-proposed.max.y,source.center.z-proposed.center.z);
                var light=new GameObject("Matched sunlight").AddComponent<Light>();
                light.type=LightType.Directional;light.intensity=1.25f;light.shadows=LightShadows.Soft;
                light.transform.rotation=Quaternion.Euler(43,-29,0);
                RenderSettings.ambientMode=AmbientMode.Flat;
                RenderSettings.ambientLight=new Color(.62f,.64f,.67f);
                RenderSettings.fog=false;
                var camera=new GameObject("Matched strategy camera").AddComponent<Camera>();
                camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;
                camera.backgroundColor=new Color(.3f,.37f,.47f);
                camera.nearClipPlane=.05f;camera.farClipPlane=450f;
                float radius=Mathf.Max(source.extents.x,Mathf.Max(source.extents.y,source.extents.z));
                Vector3 direction=new Vector3(1.15f,.85f,-1.4f).normalized;
                camera.transform.position=source.center+direction*(radius*5+9);
                camera.transform.LookAt(source.center);
                camera.orthographicSize=Mathf.Max(2,radius*1.9f);
                string outDir=Path.Combine(UnityEngine.Application.dataPath,"../ValoriaCoherentCliffV5Captures");
                Directory.CreateDirectory(outDir);
                candidate.SetActive(false);
                Render(camera,Path.Combine(outDir,"before-textured.png"));
                candidate.SetActive(true);frozen.SetActive(false);
                Render(camera,Path.Combine(outDir,"after-textured.png"));
                var shader=Shader.Find("Universal Render Pipeline/Lit");
                if(shader==null)throw new Exception("URP Lit unavailable");
                var neutral=new Material(shader);
                neutral.SetColor("_BaseColor",new Color(.56f,.53f,.48f));
                neutral.SetFloat("_Metallic",0f);neutral.SetFloat("_Smoothness",.12f);
                foreach(var obj in new[]{frozen,candidate})
                foreach(var rd in obj.GetComponentsInChildren<Renderer>(true)) {
                    var m=new Material[rd.sharedMaterials.Length];
                    for(int i=0;i<m.Length;i++)m[i]=neutral;
                    rd.sharedMaterials=m;
                }
                candidate.SetActive(false);frozen.SetActive(true);
                Render(camera,Path.Combine(outDir,"before-neutral.png"));
                frozen.SetActive(false);candidate.SetActive(true);
                Render(camera,Path.Combine(outDir,"after-neutral.png"));
                var manifest="{\"source\":\"frozen V3 exact sha validation in CI\",\"candidate\":\"Qwen directed Blender authored seamless cliff_coherent_v5\",\"camera_matched\":true,\"art_pass\":false,\"mobile_pass\":false}";
                File.WriteAllText(Path.Combine(outDir,"manifest.json"),manifest);
                Debug.Log("ELDORIA_COHERENT_CLIFF_V5_UNITY_COMPLETED");
                Environment.Exit(0);
            } catch(Exception e) {Debug.LogError("ELDORIA_COHERENT_CLIFF_V4_FAILED "+e);Environment.Exit(1);}
        }
        static async Task<GameObject> Load(string path,string name) {
            if(!File.Exists(path))throw new FileNotFoundException(path);
            var importer=new GltfImport(null,new UninterruptedDeferAgent());
            if(!await importer.Load(new Uri(path).AbsoluteUri))throw new Exception("Import failed "+name);
            var go=new GameObject(name);
            if(!await importer.InstantiateMainSceneAsync(go.transform))throw new Exception("Empty scene "+name);
            if(go.GetComponentsInChildren<Renderer>(true).Length<1)throw new Exception("No renderers "+name);
            return go;
        }
        static Bounds BoundsFor(GameObject o) {
            var r=o.GetComponentsInChildren<Renderer>(true);
            Bounds b=r[0].bounds;
            for(int i=1;i<r.Length;i++)b.Encapsulate(r[i].bounds);
            return b;
        }
        static void Render(Camera c,string path) {
            var rt=new RenderTexture(1200,800,24);
            var old=RenderTexture.active;
            try {
                c.targetTexture=rt;RenderTexture.active=rt;c.Render();
                var t=new Texture2D(1200,800,TextureFormat.RGB24,false);
                t.ReadPixels(new Rect(0,0,1200,800),0,0);t.Apply();
                File.WriteAllBytes(path,t.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(t);
                if(new FileInfo(path).Length<20000)throw new Exception("Invalid frame "+path);
            } finally {
                c.targetTexture=null;RenderTexture.active=old;rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }
    }
}
