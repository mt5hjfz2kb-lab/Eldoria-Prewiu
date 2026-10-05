using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Eldoria.EditorTools
{
    // Generic, editor-only primitive mesh review. Never opens or saves production scenes.
    public static class PreproductionSceneCapture
    {
        [Serializable] public class MeshInput { public string name; public string family; public Vector3[] vertices; public int[] indices; public int[] color; }
        [Serializable] public class Input { public string classification; public MeshInput[] meshes; }
        [Serializable] public class View { public string name; public int width; public int height; public float pitch; public float yaw; public float span; public Vector3 center; public bool perspective; public float distance; }
        [Serializable] public class Request { public string mode; public string input; public View[] views; }
        [Serializable] public class Bound { public string name; public Vector4 bbox; }
        [Serializable] public class ViewResult { public string name; public int width; public int height; public Bound[] bounds; }
        [Serializable] public class Evidence { public string engine=UnityEngine.Application.unityVersion; public string classification="GREYBOX_ONLY"; public int meshes; public int triangles; public int colliders; public int tripo_credits=0; public bool production_scene_opened=false; public bool production_scene_saved=false; public ViewResult[] views; }
        const string Folder="ValoriaProductionArtResetV1Captures";

        public static void Capture()
        {
            var root=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../.."));
            var request=JsonUtility.FromJson<Request>(File.ReadAllText(Path.Combine(root,"pipeline/valoria-production-art-reset-run-request.json")));
            if(request.mode!="preproduction_mesh_scene") throw new Exception("Wrong preproduction mode");
            var input=JsonUtility.FromJson<Input>(File.ReadAllText(Path.Combine(root,request.input)));
            if(input.classification!="GREYBOX_ONLY") throw new Exception("Final geometry prohibited");
            SceneSetup.SetupRenderPipeline();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            ShaderUtil.allowAsyncCompilation=false;
            var materials=new Dictionary<string,Material>();
            var renderers=new List<Renderer>();
            int triangles=0;
            foreach(var item in input.meshes)
            {
                var go=new GameObject(item.name);
                var vertices=new Vector3[item.indices.Length];
                var indices=new int[item.indices.Length];
                // Flat face normals, no shading polish. Both sides survive coordinate handedness conversion.
                for(int i=0;i<indices.Length;i++)
                {
                    // X/Z/Y conversion reflects handedness: reverse triangle winding.
                    int corner=i%3;int source=i-corner+(corner==1?2:corner==2?1:0);
                    vertices[i]=item.vertices[item.indices[source]];indices[i]=i;
                }
                var mesh=new Mesh{name=item.name+"_GREYBOX"};mesh.vertices=vertices;mesh.triangles=indices;mesh.RecalculateNormals();mesh.RecalculateBounds();
                go.AddComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=go.AddComponent<MeshRenderer>();
                if(!materials.TryGetValue(item.family,out var material))
                {
                    material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    material.SetColor("_BaseColor",new Color(item.color[0]/255f,item.color[1]/255f,item.color[2]/255f));
                    material.SetFloat("_Smoothness",0);material.SetFloat("_Cull",0);
                    materials.Add(item.family,material);
                }
                renderer.sharedMaterial=material;renderers.Add(renderer);triangles+=indices.Length/3;
            }
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.7f,.7f,.7f);RenderSettings.fog=false;
            var light=new GameObject("Greybox directional light").AddComponent<Light>();
            light.type=LightType.Directional;light.intensity=.7f;light.transform.rotation=Quaternion.Euler(48,-35,0);light.shadows=LightShadows.None;
            var camera=new GameObject("Preproduction camera").AddComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.53f,.57f,.60f);
            camera.nearClipPlane=.1f;camera.farClipPlane=400f;
            Directory.CreateDirectory(Folder);
            var results=new List<ViewResult>();
            foreach(var view in request.views)
            {
                float pitch=view.pitch*Mathf.Deg2Rad,yaw=view.yaw*Mathf.Deg2Rad;
                var vector=new Vector3(Mathf.Sin(yaw)*Mathf.Cos(pitch),Mathf.Sin(pitch),-Mathf.Cos(yaw)*Mathf.Cos(pitch));
                camera.transform.position=view.center+vector*view.distance;camera.transform.LookAt(view.center);
                camera.orthographic=!view.perspective;camera.orthographicSize=view.span/2f;
                camera.fieldOfView=2f*Mathf.Atan(view.span/2f/view.distance)*Mathf.Rad2Deg;
                camera.aspect=(float)view.width/view.height;
                var bounds=new List<Bound>();
                foreach(var renderer in renderers)
                {
                    var mesh=renderer.GetComponent<MeshFilter>().sharedMesh;
                    float xmin=float.PositiveInfinity,ymin=xmin,xmax=float.NegativeInfinity,ymax=xmax;
                    foreach(var vertex in mesh.vertices)
                    {
                        var p=camera.WorldToViewportPoint(vertex);
                        xmin=Mathf.Min(xmin,p.x*view.width);xmax=Mathf.Max(xmax,p.x*view.width);
                        ymin=Mathf.Min(ymin,(1-p.y)*view.height);ymax=Mathf.Max(ymax,(1-p.y)*view.height);
                    }
                    bounds.Add(new Bound{name=renderer.name,bbox=new Vector4(xmin,ymin,xmax,ymax)});
                }
                Save(camera,Folder+"/"+view.name+".png",view.width,view.height);
                results.Add(new ViewResult{name=view.name,width=view.width,height=view.height,bounds=bounds.ToArray()});
            }
            int colliders=Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length;
            if(colliders!=0) throw new Exception("Preproduction scene must have zero gameplay colliders");
            File.WriteAllText(Folder+"/evidence.json",JsonUtility.ToJson(new Evidence{meshes=input.meshes.Length,triangles=triangles,colliders=colliders,views=results.ToArray()},true));
            Debug.Log("PREPRODUCTION_CAPTURE_TECH_PASS; VISUAL_REVIEW_REQUIRED");
            EditorApplication.Exit(0);
        }

        static void Save(Camera camera,string path,int width,int height)
        {
            var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){antiAliasing=1};
            var previous=RenderTexture.active;
            try
            {
                camera.targetTexture=rt;camera.Render();camera.Render();RenderTexture.active=rt;
                var image=new Texture2D(width,height,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());Object.DestroyImmediate(image);
            }
            finally{camera.targetTexture=null;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(rt);}
        }
    }
}
