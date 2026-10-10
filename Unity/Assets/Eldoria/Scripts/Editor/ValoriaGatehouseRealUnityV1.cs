using System;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools
{
    // Isolated production-asset candidate. No modifications to Valoria's playable scene.
    public static class ValoriaGatehouseRealUnityV1
    {
        const string AssetPath = "Assets/Eldoria/ArtTests/ValoriaGatehouseV1/valoria_gate_reference.glb";
        const string PriorPath = "Assets/Eldoria/Resources/Valoria/ProductionArt/CoherentFamilyV1/Valoria_MainGate_CohV1.glb";
        const string Output = "ValoriaGatehouseRealUnityV1Captures";

        public static async void Capture()
        {
            try { await Execute(); EditorApplication.Exit(0); }
            catch(Exception ex) { Debug.LogException(ex); EditorApplication.Exit(1); }
        }

        static async Task Execute()
        {
            Directory.CreateDirectory(Output);
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root = new GameObject("Blender authored gatehouse candidate");
            var gltf = new GltfImport(null, new UninterruptedDeferAgent());
            var path = Path.GetFullPath(AssetPath);
            if(!File.Exists(path)) throw new FileNotFoundException(path);
            if(!await gltf.LoadFile(path)) throw new Exception("glTFast could not load exact Blender GLB");
            if(!await gltf.InstantiateMainSceneAsync(root.transform)) throw new Exception("glTFast scene instantiation failed");
            var renderers=root.GetComponentsInChildren<Renderer>(true);
            if(renderers.Length==0) throw new Exception("GLB instantiated without renderers");
            var bounds=renderers[0].bounds;
            foreach(var r in renderers) bounds.Encapsulate(r.bounds);
            if(bounds.size.x<0.001f) throw new Exception("GLB invalid width");
            root.transform.localScale*=17.3f/bounds.size.x;
            renderers=root.GetComponentsInChildren<Renderer>(true);
            bounds=renderers[0].bounds;
            foreach(var r in renderers) bounds.Encapsulate(r.bounds);
            root.transform.position-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
            var light=new GameObject("Valoria matched sunlight").AddComponent<Light>();
            light.type=LightType.Directional;light.intensity=1.2f;light.shadows=LightShadows.Soft;
            light.transform.rotation=Quaternion.Euler(42f,-35f,0f);
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.48f,.52f,.58f);
            var camera=new GameObject("Valoria strategic orthographic camera").AddComponent<Camera>();
            camera.orthographic=true;camera.orthographicSize=9f;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.27f,.32f,.38f);
            camera.transform.position=new Vector3(18.2f,14.6f,-25.8f);
            camera.transform.LookAt(new Vector3(0f,3.15f,5.8f));
            Save(camera,Output+"/unity-strategic-1280x720.png",1280,720);
            camera.transform.position=new Vector3(18.2f,14.6f,-31.6f);
            camera.transform.LookAt(new Vector3(0f,3.15f,0f));
            camera.orthographicSize=16f;
            Save(camera,Output+"/unity-mobile-390x844.png",390,844);
            long triangles=0;int materials=0;
            foreach(var mesh in root.GetComponentsInChildren<MeshFilter>(true))
                if(mesh.sharedMesh!=null) for(int i=0;i<mesh.sharedMesh.subMeshCount;i++)triangles+=mesh.sharedMesh.GetIndexCount(i)/3;
            foreach(var renderer in renderers)materials+=renderer.sharedMaterials.Length;
            File.WriteAllText(Output+"/metrics.json",
                "{\n  \"source\": \"exact authored Blender GLB\",\n  \"triangles_instantiated\": "+triangles+
                ",\n  \"renderer_count\": "+renderers.Length+",\n  \"material_slots\": "+materials+
                ",\n  \"reference_source\": \""+PriorPath+"\",\n  \"physical_mobile_tested\": false\n}\n");
            // A Unity snapshot is not an artistic approval or proof of runtime parity.
            Debug.Log("GATEHOUSE_UNITY_CAPTURE_COMPLETE "+Path.GetFullPath(Output));
        }
        static void Save(Camera camera,string name,int w,int h)
        {
            var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){antiAliasing=4};
            var old=RenderTexture.active;
            try {
                camera.targetTexture=rt;camera.Render();camera.Render();
                RenderTexture.active=rt;
                var pixels=new Texture2D(w,h,TextureFormat.RGB24,false);
                pixels.ReadPixels(new Rect(0,0,w,h),0,0);pixels.Apply();
                File.WriteAllBytes(name,pixels.EncodeToPNG());Object.DestroyImmediate(pixels);
            } finally {
                camera.targetTexture=null;RenderTexture.active=old;rt.Release();Object.DestroyImmediate(rt);
            }
        }
    }
}