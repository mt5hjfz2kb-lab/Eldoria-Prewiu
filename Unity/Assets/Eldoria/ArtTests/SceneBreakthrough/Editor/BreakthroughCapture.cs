using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Eldoria.Breakthrough {
public static class BreakthroughCapture {
 const string Study="Assets/Eldoria/ArtTests/SceneBreakthrough";
 const string Output="SceneBreakthroughCaptures";
 [Serializable] public class Item { public string name,family;public Vector3[] vertices;public int[] indices,color; }
 [Serializable] public class Input { public Item[] meshes; }
 static int saved;
 static readonly HashSet<Object> persisted=new HashSet<Object>();
 public static void Capture() {
  try {
   Directory.CreateDirectory(Study+"/Generated");Directory.CreateDirectory(Output);
   ShaderUtil.allowAsyncCompilation=false;
   DebugManager.instance.Reset();
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var root=new GameObject("BREAKTHROUGH_COMPLETE_COMPOSITION_GREYBOX");
   string project=Path.GetFullPath("..");
   var input=JsonUtility.FromJson<Input>(File.ReadAllText(Path.Combine(project,"docs/evidence/valoria-art-production-reset-v1/unity-blockout-input.json")));
   var mats=new Dictionary<string,Material>();
   foreach(var item in input.meshes) {
    var go=new GameObject(item.name);go.transform.SetParent(root.transform);
    var mesh=new Mesh{name=item.name+"_BLOCKOUT"};var verts=new Vector3[item.indices.Length];var tris=new int[verts.Length];
    for(int i=0;i<verts.Length;i++){int k=i%3;verts[i]=item.vertices[item.indices[i-k+(k==1?2:k==2?1:0)]];tris[i]=i;}
    mesh.vertices=verts;mesh.triangles=tris;mesh.RecalculateNormals();mesh.RecalculateBounds();
    go.AddComponent<MeshFilter>().sharedMesh=mesh;
    if(!mats.ContainsKey(item.family)){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.name="Blockout_"+item.family;m.SetColor("_BaseColor",new Color(item.color[0]/255f,item.color[1]/255f,item.color[2]/255f));m.SetFloat("_Smoothness",.08f);m.SetFloat("_Cull",0);mats.Add(item.family,m);}
    go.AddComponent<MeshRenderer>().sharedMaterial=mats[item.family];
    if(item.family=="ground" || item.family=="road" || item.name=="MainPlatform")go.AddComponent<MeshCollider>().sharedMesh=mesh;
   }
   var sun=new GameObject("Comparative sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.15f;sun.color=new Color(1,.9f,.79f);sun.transform.rotation=Quaternion.Euler(45,-35,0);sun.shadows=LightShadows.Soft;
   RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.45f,.53f,.65f);RenderSettings.ambientEquatorColor=new Color(.37f,.4f,.43f);RenderSettings.ambientGroundColor=new Color(.24f,.22f,.18f);RenderSettings.fog=false;
   var cam=new GameObject("Player camera").AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=24;cam.nearClipPlane=.1f;cam.farClipPlane=400;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.24f,.32f,.41f);
   cam.transform.rotation=Quaternion.Euler(35,-20,0);cam.transform.position=-cam.transform.forward*110;
   cam.gameObject.AddComponent<Eldoria.Art.BreakthroughCamera>().ConfigureHome();
   RenderSet(cam,"blockout");
   foreach(var m in root.GetComponentsInChildren<MeshFilter>())Persist(m.sharedMesh);
   foreach(var m in mats.Values)Persist(m);
   AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,Study+"/BastionI_Blockout.unity");
   File.WriteAllText(Output+"/status.json","{\"stage\":\"COMPLETE_BLOCKOUT\",\"art_approved\":false,\"credits\":0,\"new_scene\":true}");
   Debug.Log("BREAKTHROUGH_CAPTURE_COMPLETE");EditorApplication.Exit(0);
  }catch(Exception ex){Debug.LogException(ex);EditorApplication.Exit(1);}
 }
 static void Persist(Object obj){if(obj==null||AssetDatabase.Contains(obj)||!persisted.Add(obj))return;AssetDatabase.CreateAsset(obj,Study+"/Generated/blockout_"+(saved++).ToString("D4")+".asset");}
 public static void RenderSet(Camera c,string prefix){
  var pos=c.transform.position;float size=c.orthographicSize;
  Render(c,prefix+"-reference",1536,1024);Render(c,prefix+"-landscape",1280,720);Render(c,prefix+"-portrait",390,844);
  c.transform.position=pos+c.transform.right*12;Render(c,prefix+"-pan-right",1280,720);
  c.transform.position=pos-c.transform.right*12;Render(c,prefix+"-pan-left",1280,720);
  c.transform.position=pos;c.orthographicSize=size*.8f;Render(c,prefix+"-zoom",1280,720);
  c.orthographicSize=size; c.transform.position=pos;
 }
 [Serializable] class CameraRecord { public int width,height;public Vector3 position;public Quaternion rotation;public float orthographicSize;public Matrix4x4 worldToCamera,projection; }
 public static void Render(Camera c,string name,int w,int h){
  var rt=new RenderTexture(w,h,24,RenderTextureFormat.ARGB32){antiAliasing=4};var prev=RenderTexture.active;
  c.aspect=(float)w/h;c.targetTexture=rt;c.Render();c.Render();c.Render();RenderTexture.active=rt;
  var pixels=new Texture2D(w,h,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,w,h),0,0);pixels.Apply();
  File.WriteAllBytes(Output+"/"+name+".png",pixels.EncodeToPNG());
  File.WriteAllText(Output+"/"+name+".camera.json",JsonUtility.ToJson(new CameraRecord{width=w,height=h,position=c.transform.position,rotation=c.transform.rotation,orthographicSize=c.orthographicSize,worldToCamera=c.worldToCameraMatrix,projection=c.projectionMatrix},true));
  c.targetTexture=null;RenderTexture.active=prev;Object.DestroyImmediate(pixels);rt.Release();Object.DestroyImmediate(rt);
 }
}}
