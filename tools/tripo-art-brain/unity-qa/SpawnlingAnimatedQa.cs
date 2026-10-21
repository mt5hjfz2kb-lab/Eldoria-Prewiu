// Copied into a disposable Unity QA project only; never compiled by the game.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public static class SpawnlingAnimatedQa
{
    const string Expected = "86ec1c93a222d58b89b9f393d39c8e5e912b14516ff5298500e5aca80048f631";
    [Serializable] public class Result
    {
        public string status, source_sha256, unity, clip, error;
        public int skins, joints, vertices;
        public float duration, attack_delta, return_delta;
        public bool canonical_scene_changed, integrated, art_approved;
    }
    static void Need(bool condition,string message) { if(!condition)throw new Exception(message); }
    static string Hash(string path) { using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant(); }
    static Vector3[] Vertices(SkinnedMeshRenderer[] skins)
    {
        var all=new List<Vector3>();
        foreach(var skin in skins)
        {
            var mesh=new Mesh();
            try { skin.BakeMesh(mesh); all.AddRange(mesh.vertices.Select(v=>skin.transform.TransformPoint(v))); }
            finally { Object.DestroyImmediate(mesh); }
        }
        return all.ToArray();
    }
    static float Delta(Vector3[] a,Vector3[] b)
    {
        Need(a.Length==b.Length && a.Length>0,"Baked vertex topology changed");
        float max=0;for(int i=0;i<a.Length;i++)max=Mathf.Max(max,Vector3.Distance(a[i],b[i]));return max;
    }
    static void Capture(Camera camera,string file)
    {
        var previous=RenderTexture.active;
        var target=RenderTexture.GetTemporary(960,720,24,RenderTextureFormat.ARGB32);
        var image=new Texture2D(960,720,TextureFormat.RGB24,false);
        try
        {
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            image.ReadPixels(new Rect(0,0,960,720),0,0);image.Apply();
            File.WriteAllBytes(file,image.EncodeToPNG());
        }
        finally { camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(image);RenderTexture.ReleaseTemporary(target); }
    }
    public static async void Run()
    {
        string output=Environment.GetEnvironmentVariable("ELDORIA_RIG_QA_OUTPUT");
        string input=Environment.GetEnvironmentVariable("ELDORIA_RIG_QA_INPUT");
        var result=new Result{status="FAILED",unity=UnityEngine.Application.unityVersion};
        Directory.CreateDirectory(output);
        int exit=1;
        try
        {
            Need(File.Exists(input),"Exact archived V4 GLB missing");
            result.source_sha256=Hash(input);Need(result.source_sha256==Expected,"V4 source digest mismatch");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            RenderSettings.ambientMode=AmbientMode.Flat;
            RenderSettings.ambientLight=new Color(.52f,.55f,.62f);
            var light=new GameObject("QA key light").AddComponent<Light>();
            light.type=LightType.Directional;light.intensity=1.25f;light.transform.rotation=Quaternion.Euler(45,-35,0);
            var root=new GameObject("Exact V4 archive model");
            using(var gltf=new GltfImport(null,new UninterruptedDeferAgent()))
            {
                Need(await gltf.Load(new Uri(Path.GetFullPath(input)).AbsoluteUri,
                    new ImportSettings{AnimationMethod=AnimationMethod.Legacy,GenerateMipMaps=true}),"GLB load failed");
                Need(await gltf.InstantiateMainSceneAsync(root.transform),"GLB instantiation failed");
                var clips=gltf.GetAnimationClips();
                Need(clips!=null && clips.Length==1,"Expected one genuine imported clip");
                var clip=clips[0];result.clip=clip.name;result.duration=clip.length;
                Need(clip.length>2.6f && clip.length<2.9f,"Unexpected clip duration");
                var anim=root.GetComponentsInChildren<Animation>(true).Single();
                anim.Stop();anim.enabled=false;
                var skins=root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                Need(skins.Length>0,"No genuine SkinnedMeshRenderer");
                result.skins=skins.Length;
                result.joints=skins.SelectMany(s=>s.bones).Distinct().Count();
                Need(result.joints==12,"Expected twelve imported joints");
                foreach(var skin in skins)
                {
                    Need(skin.sharedMesh && skin.sharedMesh.bindposes.Length==skin.bones.Length,"Invalid bindposes");
                    Need(skin.bones.All(b=>b),"Missing imported bone");
                    skin.updateWhenOffscreen=true;
                    foreach(var material in skin.sharedMaterials)
                    {
                        Need(material && material.shader && !material.shader.name.Contains("InternalError"),"Invalid material");
                        Need(material.GetTexturePropertyNames().Any(p=>material.GetTexture(p)!=null),"PBR textures missing");
                    }
                }
                var times=new[]{1f/24f,27f/24f,65f/24f};
                var labels=new[]{"rest","swipe","return"};
                var poses=new List<Vector3[]>();
                var bounds=new Bounds();
                for(int i=0;i<times.Length;i++)
                {
                    clip.SampleAnimation(anim.gameObject,times[i]);
                    var vertices=Vertices(skins);poses.Add(vertices);
                    foreach(var v in vertices) {
                        Need(!float.IsNaN(v.x)&&!float.IsNaN(v.y)&&!float.IsNaN(v.z),"Nonfinite skin");
                        if(i==0 && v==vertices[0])bounds=new Bounds(v,Vector3.zero);else bounds.Encapsulate(v);
                    }
                }
                result.vertices=poses[0].Length;
                result.attack_delta=Delta(poses[0],poses[1]);result.return_delta=Delta(poses[0],poses[2]);
                Need(result.attack_delta>bounds.size.magnitude*.001f,"Clip does not deform the actual skin");
                Need(result.return_delta<bounds.size.magnitude*.001f,"Clip fails to return to rest");
                var camera=new GameObject("Fixed QA camera").AddComponent<Camera>();
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.08f,.10f,.14f);
                camera.orthographic=true;camera.orthographicSize=bounds.extents.magnitude*.85f;
                camera.nearClipPlane=.01f;camera.farClipPlane=1000;camera.aspect=4f/3f;
                camera.transform.position=bounds.center+new Vector3(1,.6f,-1.7f).normalized*bounds.size.magnitude*2;
                camera.transform.LookAt(bounds.center);
                for(int i=0;i<times.Length;i++) {
                    clip.SampleAnimation(anim.gameObject,times[i]);
                    Capture(camera,Path.Combine(output,labels[i]+".png"));
                }
                result.status="UNITY_SKIN_CLIP_IMPORT_PASS_VISUAL_REVIEW_REQUIRED";exit=0;
            }
        }
        catch(Exception ex) { result.error=ex.ToString();Debug.LogException(ex); }
        finally { File.WriteAllText(Path.Combine(output,"result.json"),JsonUtility.ToJson(result,true));EditorApplication.Exit(exit); }
    }
}
