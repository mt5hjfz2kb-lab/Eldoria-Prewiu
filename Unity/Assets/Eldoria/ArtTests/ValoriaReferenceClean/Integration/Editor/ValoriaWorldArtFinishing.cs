#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Eldoria.World4XPrototype;
using Eldoria.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
 public static class ValoriaWorldArtFinishing {
  const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
  static string Ev=>Path.GetFullPath("../docs/evidence/valoria-reference-clean/world204");
  [Serializable] public class Node {public string id;public Vector3 position;}
  [Serializable] public class Part {public string name,path,mesh;public bool active;public Vector3 min,max,position,scale;public int triangles,vertices;public string[] materials;}
  [Serializable] public class Ground {public Vector3[] vertices;public int[] triangles;public Vector2[] uv;}
  [Serializable] public class Data {public string scene_sha256,reference_sha256;public Node[] nodes;public Part[] parts;public Ground ground;public Vector3 camera_euler;public float camera_zoom;}
  static string Hash(string p){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p))).Replace("-","").ToLowerInvariant();}
  static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
  static void CaptureViews(ValoriaIntegratedWorld host,string prefix){
   var c=host.WorldCamera;var pos=c.transform.position;var rot=c.transform.rotation;float zoom=c.orthographicSize;bool active=host.WorldRoot.activeSelf;host.WorldRoot.SetActive(true);
   try{foreach(string id in new[]{"forest-valoria","quarry-valoria","corrupt-scout","engendro-valoria"}){
    var point=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true).Single(n=>n.Id==id).transform.position;point.y=2;c.transform.position=point-c.transform.forward*110;
    foreach(float size in new[]{32f,14f}){c.orthographicSize=size;ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-"+id+"-"+size.ToString("0")+".png",1448,1086);}
    c.orthographicSize=14;ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-"+id+"-portrait.png",900,1600);
   }}finally{c.transform.SetPositionAndRotation(pos,rot);c.orthographicSize=zoom;host.WorldRoot.SetActive(active);}
  }
  public static void Survey(){try{
   Directory.CreateDirectory(Ev);EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();bool active=host.WorldRoot.activeSelf;host.WorldRoot.SetActive(true);
   var parts=new List<Part>();foreach(var f in host.WorldRoot.GetComponentsInChildren<MeshFilter>(true)){
    if(!f.sharedMesh)continue;var r=f.GetComponent<Renderer>();if(!r)continue;parts.Add(new Part{name=f.name,path=PathOf(f.transform),mesh=AssetDatabase.GetAssetPath(f.sharedMesh),active=f.gameObject.activeInHierarchy&&r.enabled,min=r.bounds.min,max=r.bounds.max,position=f.transform.position,scale=f.transform.lossyScale,vertices=f.sharedMesh.vertexCount,triangles=f.sharedMesh.triangles.Length/3,materials=r.sharedMaterials.Select(m=>m?AssetDatabase.GetAssetPath(m):"").ToArray()});
   }
   var ground=host.WorldRoot.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.name=="ZoneOne continuous prairie source");var gm=ground.sharedMesh;
   var data=new Data{scene_sha256=Hash(Scene),reference_sha256=Hash("../references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg"),nodes=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true).Select(n=>new Node{id=n.Id,position=n.transform.position}).ToArray(),parts=parts.ToArray(),ground=new Ground{vertices=gm.vertices.Select(v=>ground.transform.TransformPoint(v)).ToArray(),triangles=gm.triangles,uv=gm.uv},camera_euler=host.WorldCamera.transform.eulerAngles,camera_zoom=host.WorldCamera.orthographicSize};File.WriteAllText(Ev+"/survey.json",JsonUtility.ToJson(data));CaptureViews(host,"before");host.WorldRoot.SetActive(active);Debug.Log("VALORIA_REFERENCE_CLEAN_WORLD_ART_SURVEY_COMPLETE");EditorApplication.Exit(0);
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 }
}
#endif
