using System;
using System.Collections.Generic;
using System.IO;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Eldoria.EditorTools
{
    // Dedicated candidate. Never touches canonical Valoria or its build settings.
    public static class Bastion1ReferenceCapture
    {
        const string Study="Assets/Eldoria/Experimental/Bastion1Assembly";
        static readonly Dictionary<UnityEngine.Object,string> Saved=new Dictionary<UnityEngine.Object,string>();
        [MenuItem("Eldoria/Art/Build Bastion I reference assembly candidate")]
        public static async void Capture()
        {
            var previousGraphics=GraphicsSettings.defaultRenderPipeline;
            var previousQuality=QualitySettings.renderPipeline;
            try
            {
                var original=previousGraphics as UniversalRenderPipelineAsset;
                if(original==null)throw new Exception("A configured URP project is required");
                var pipeline=UnityEngine.Object.Instantiate(original);
                pipeline.name="Bastion I comparative URP, unprofiled";
                pipeline.shadowDistance=180f;pipeline.shadowCascadeCount=4;
                pipeline.msaaSampleCount=4;pipeline.useSRPBatcher=true;
                Directory.CreateDirectory(Study+"/Generated");
                AssetDatabase.DeleteAsset(Study+"/Generated/StudyURP.asset");
                AssetDatabase.CreateAsset(pipeline,Study+"/Generated/StudyURP.asset");
                GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;
                var active=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                var source=Path.GetFullPath(Study+"/GeneratedSource/Bastion1ReferenceAssembly.glb");
                if(!File.Exists(source))throw new FileNotFoundException("Run the exact source assembler first",source);
                var importer=new GltfImport(null,new UninterruptedDeferAgent());
                if(!await importer.Load(new Uri(source).AbsoluteUri))throw new Exception("glTFast source import failed");
                var root=new GameObject("BastionI_ReferenceAssembly_CANDIDATE");
                if(!await importer.InstantiateMainSceneAsync(root.transform))throw new Exception("glTFast instantiate failed");
                var sun=new GameObject("Bastion I afternoon sun").AddComponent<Light>();
                sun.type=LightType.Directional;sun.color=new Color(1f,.86f,.72f);sun.intensity=1.25f;
                sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(38f,-38f,0f);
                RenderSettings.ambientMode=AmbientMode.Trilight;
                RenderSettings.ambientSkyColor=new Color(.37f,.46f,.61f);
                RenderSettings.ambientEquatorColor=new Color(.24f,.29f,.32f);
                RenderSettings.ambientGroundColor=new Color(.15f,.13f,.10f);
                RenderSettings.fog=false;
                var camera=new GameObject("Bastion I strategic camera").AddComponent<Camera>();
                camera.orthographic=true;camera.orthographicSize=38.5f;
                camera.nearClipPlane=.1f;camera.farClipPlane=350f;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.15f,.23f,.31f);
                // glTF exporter: Blender (x,y,z)->(x,z,-y); glTFast mirrors x.
                camera.transform.position=new Vector3(-48f,54f,70f);camera.transform.LookAt(Vector3.zero);
                var controls=camera.gameObject.AddComponent<Eldoria.Art.Bastion1AssemblyCamera>();
                controls.ConfigureHome();
                var directory=Path.GetFullPath("Bastion1ReferenceCaptures");Directory.CreateDirectory(directory);
                foreach(var stale in Directory.GetFiles(directory,"*.png"))File.Delete(stale);
                Render(camera,Path.Combine(directory,"bastion1-unity-landscape.png"),1440,1000);
                Render(camera,Path.Combine(directory,"bastion1-unity-mobile-landscape.png"),780,520);
                // Portrait shows complete construction envelope, without artificial close zoom.
                camera.orthographicSize=76f;
                Render(camera,Path.Combine(directory,"bastion1-unity-portrait.png"),390,844);
                camera.orthographicSize=38.5f;
                // Focalized source check: relative clamps cannot teleport this camera
                // to the old image-plane origin on the first player drag.
                var home=camera.transform.position;
                controls.PanByScreenDelta(new Vector2(100000f,-100000f));
                var moved=camera.transform.position-home;
                if(Mathf.Abs(moved.x)>12.001f||Mathf.Abs(moved.z)>12.001f||moved.sqrMagnitude<.01f)
                    throw new Exception("Candidate camera pan bounds invalid");
                controls.ZoomBy(-10000f);controls.Home();
                if(Vector3.Distance(camera.transform.position,home)>.001f)
                    throw new Exception("Candidate Home pose was not restored");
                var renderers=root.GetComponentsInChildren<Renderer>(true);
                var uniqueMeshes=new HashSet<Mesh>();var uniqueMaterials=new HashSet<Material>();var uniqueTextures=new HashSet<Texture>();long triangles=0;
                foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))
                {
                    if(filter.sharedMesh==null)continue;
                    triangles+=filter.sharedMesh.triangles.Length/3;uniqueMeshes.Add(filter.sharedMesh);Persist(filter.sharedMesh);
                }
                foreach(var renderer in renderers)foreach(var material in renderer.sharedMaterials)
                {
                    if(material==null)continue;uniqueMaterials.Add(material);
                    foreach(var property in material.GetTexturePropertyNames())
                    {var texture=material.GetTexture(property);if(texture!=null){uniqueTextures.Add(texture);Persist(texture);}}
                    Persist(material);
                }
                // Only selection anchors; neither coarse gate blockers nor invented buildings.
                var upper=Find(root.transform,"UpperBastion");
                if(upper!=null)
                {
                    var handle=new GameObject("bastion.selection.candidate");handle.transform.SetParent(upper,false);
                    var box=handle.AddComponent<BoxCollider>();box.center=new Vector3(0,3,0);box.size=new Vector3(10,6,6);box.isTrigger=true;
                }
                AssetDatabase.SaveAssets();
                EditorSceneManager.SaveScene(active,Study+"/Bastion1ReferenceAssembly.unity");
                string evidence="{\"engine\":\"Unity 6000.3.23f1\",\"renderers\":"+renderers.Length+
                    ",\"shared_meshes\":"+uniqueMeshes.Count+",\"shared_materials\":"+uniqueMaterials.Count+
                    ",\"shared_textures\":"+uniqueTextures.Count+",\"visible_triangles\":"+triangles+
                    ",\"art_pass\":false,\"mobile_performance_pass\":false,\"gameplay_pass\":false,\"main_promotion_allowed\":false}";
                File.WriteAllText(Path.Combine(directory,"unity-evidence.json"),evidence);
                GraphicsSettings.defaultRenderPipeline=previousGraphics;QualitySettings.renderPipeline=previousQuality;
                Debug.Log("BASTION1_REFERENCE_CAPTURE_COMPLETE "+evidence);
                EditorApplication.Exit(0);
            }
            catch(Exception ex)
            {
                GraphicsSettings.defaultRenderPipeline=previousGraphics;QualitySettings.renderPipeline=previousQuality;
                Debug.LogError("BASTION1_REFERENCE_CAPTURE_FAILED "+ex);EditorApplication.Exit(1);
            }
        }
        static Transform Find(Transform root,string name)
        {foreach(var item in root.GetComponentsInChildren<Transform>(true))if(item.name==name)return item;return null;}
        static void Persist(UnityEngine.Object obj)
        {
            if(obj==null||AssetDatabase.Contains(obj)||Saved.ContainsKey(obj))return;
            string path=Study+"/Generated/"+Saved.Count.ToString("D4")+".asset";
            AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(obj,path);Saved.Add(obj,path);
        }
        static void Render(Camera camera,string path,int width,int height)
        {
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){antiAliasing=4};
            var prior=RenderTexture.active;Texture2D pixels=null;
            try
            {
                camera.aspect=(float)width/height;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
                pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
                pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();
                var corner=pixels.GetPixel(0,0);bool nonempty=false;
                for(int y=12;y<height;y+=24)for(int x=12;x<width;x+=24)
                    if(Vector4.Distance(pixels.GetPixel(x,y),corner)>.03f)nonempty=true;
                if(!nonempty)throw new Exception("Empty rendered scene: "+path);
                File.WriteAllBytes(path,pixels.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture=null;RenderTexture.active=prior;
                if(pixels!=null)UnityEngine.Object.DestroyImmediate(pixels);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
            }
        }
    }
}
