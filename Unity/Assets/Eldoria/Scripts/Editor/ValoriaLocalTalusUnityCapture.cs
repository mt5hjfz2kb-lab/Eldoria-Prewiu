using System;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldoria.EditorTools {
    public static class ValoriaLocalTalusUnityCapture {
        public static async void Capture() {
            try {
                SceneSetup.SetupRenderPipeline();
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                string folder=Path.Combine(UnityEngine.Application.streamingAssetsPath,"ValoriaLocalTalus");
                GameObject cliff=await Load(Path.Combine(folder,"cliff_rocks.glb"),"frozen_cliff_rocks");
                GameObject talus=await Load(Path.Combine(folder,"cliff_talus_transition_v1.glb"),"new_talus_transition_v1");
                cliff.transform.rotation=Quaternion.Euler(0,180,0);
                talus.transform.rotation=Quaternion.Euler(0,180,0);
                Bounds b=FindBounds(cliff);
                var sun=new GameObject("Sunlight").AddComponent<Light>();
                sun.type=LightType.Directional;sun.intensity=1.32f;sun.shadows=LightShadows.Soft;
                sun.transform.rotation=Quaternion.Euler(43f,-29f,0f);
                RenderSettings.ambientMode=AmbientMode.Flat;
                RenderSettings.ambientLight=new Color(.62f,.64f,.67f);
                RenderSettings.fog=false;
                var cam=new GameObject("Matched perspective camera").AddComponent<Camera>();
                cam.orthographic=true;cam.clearFlags=CameraClearFlags.SolidColor;
                cam.backgroundColor=new Color(.30f,.37f,.47f);
                cam.nearClipPlane=.05f;cam.farClipPlane=450f;
                float radius=Mathf.Max(b.extents.x,Mathf.Max(b.extents.y,b.extents.z));
                Vector3 dir=new Vector3(1.15f,.85f,-1.4f).normalized;
                cam.transform.position=b.center+dir*(radius*5f+9f);cam.transform.LookAt(b.center);
                cam.orthographicSize=Mathf.Max(2f,radius*1.9f);
                string output=Path.Combine(UnityEngine.Application.dataPath,"../ValoriaLocalTalusCaptures");
                Directory.CreateDirectory(output);
                talus.SetActive(false);
                Render(cam,Path.Combine(output,"before-frozen-cliff.png"));
                talus.SetActive(true);
                Render(cam,Path.Combine(output,"after-with-talusedge.png"));
                File.WriteAllText(Path.Combine(output,"capture-manifest.json"),
                  "{\"engine\":\"Unity 6000.3.23f1\",\"capture\":\"camera.Render matched perspective\",\"artist\":\"qwen3-vl:4b-instruct\",\"source\":\"frozen cliff V3\",\"asset\":\"cliff_talus_transition_v1\",\"visual_pass\":false,\"mobile_pass\":false}");
                Debug.Log("VALORIA_LOCAL_TALUS_UNITY_CAPTURE_COMPLETED "+output);
                Environment.Exit(0);
            } catch(Exception ex) {Debug.LogError("VALORIA_LOCAL_TALUS_UNITY_CAPTURE_FAILED "+ex);Environment.Exit(1);}
        }
        static async Task<GameObject> Load(string p,string name){
            if(!File.Exists(p))throw new FileNotFoundException(p);
            var importer=new GltfImport(null,new UninterruptedDeferAgent());
            if(!await importer.Load(new Uri(p).AbsoluteUri))throw new Exception("GLB load failed "+name);
            var go=new GameObject(name);
            if(!await importer.InstantiateMainSceneAsync(go.transform))throw new Exception("GLB instantiate failed "+name);
            if(go.GetComponentsInChildren<Renderer>(true).Length==0)throw new Exception("Empty renderer "+name);
            return go;
        }
        static Bounds FindBounds(GameObject go){
            var rs=go.GetComponentsInChildren<Renderer>(true);
            var b=rs[0].bounds;
            for(int i=1;i<rs.Length;i++)b.Encapsulate(rs[i].bounds);
            return b;
        }
        static void Render(Camera cam,string path){
            var rt=new RenderTexture(1200,800,24);
            cam.targetTexture=rt;var old=RenderTexture.active;RenderTexture.active=rt;
            cam.Render();
            var t=new Texture2D(1200,800,TextureFormat.RGB24,false);
            t.ReadPixels(new Rect(0,0,1200,800),0,0);t.Apply();
            File.WriteAllBytes(path,t.EncodeToPNG());
            RenderTexture.active=old;cam.targetTexture=null;
            UnityEngine.Object.DestroyImmediate(t);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
            if(new FileInfo(path).Length<20000)throw new Exception("Suspiciously small render "+path);
        }
    }
}
