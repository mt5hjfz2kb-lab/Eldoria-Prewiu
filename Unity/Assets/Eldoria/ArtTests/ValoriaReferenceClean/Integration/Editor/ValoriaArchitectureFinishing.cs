#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
 public static class ValoriaArchitectureFinishing {
  const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
  static string Ev=>Path.GetFullPath("../docs/evidence/valoria-reference-clean/architecture201");
  [Serializable] public class Slot {public int index;public string name,asset,shader,texture;public Color color;public Vector3 min,max;public int triangles;}
  [Serializable] public class Part {public string name,path;public Vector3 min,max;public Vector3[] vertices;public Vector2[] uv;public int[] triangles;public Slot[] slots;}
  [Serializable] public class Data {public string scene_sha256,reference_sha256;public Part[] parts;}
  static string Hash(string p){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p))).Replace("-","").ToLowerInvariant();}
  static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
  static void Views(Camera c,string name){ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+name+"-main.png",1448,1086);ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+name+"-portrait.png",900,1600);}
  public static void Survey(){try{
   Directory.CreateDirectory(Ev);EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
   var parts=new List<Part>();
   foreach(var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
    if(!f.sharedMesh||f.transform.IsChildOf(host.WorldRoot.transform)||!(f.name.StartsWith("Bridge_")||f.name.StartsWith("Gate_")||f.name.StartsWith("Bastion_")))continue;
    var m=f.sharedMesh;var r=f.GetComponent<Renderer>();if(!r)continue;var vv=m.vertices.Select(v=>f.transform.TransformPoint(v)).ToArray();var slots=new List<Slot>();
    for(int i=0;i<m.subMeshCount;i++){var inds=m.GetTriangles(i);if(inds.Length==0)continue;var b=new Bounds(vv[inds[0]],Vector3.zero);foreach(int ix in inds)b.Encapsulate(vv[ix]);var mat=r.sharedMaterials[i];string tp=mat.HasProperty("_BaseMap")?"_BaseMap":mat.HasProperty("baseColorTexture")?"baseColorTexture":"_MainTex";string cp=mat.HasProperty("_BaseColor")?"_BaseColor":mat.HasProperty("baseColorFactor")?"baseColorFactor":"_Color";slots.Add(new Slot{index=i,name=mat.name,asset=AssetDatabase.GetAssetPath(mat),shader=mat.shader.name,texture=mat.HasProperty(tp)?AssetDatabase.GetAssetPath(mat.GetTexture(tp)):"",color=mat.HasProperty(cp)?mat.GetColor(cp):Color.white,min=b.min,max=b.max,triangles=inds.Length/3});}
    bool structure=!f.name.StartsWith("Bastion_");parts.Add(new Part{name=f.name,path=PathOf(f.transform),min=r.bounds.min,max=r.bounds.max,vertices=structure?vv:null,uv=structure?m.uv:null,triangles=structure?m.triangles:null,slots=slots.ToArray()});
   }
   var data=new Data{scene_sha256=Hash(Scene),reference_sha256=Hash("../references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg"),parts=parts.ToArray()};File.WriteAllText(Ev+"/survey.json",JsonUtility.ToJson(data));
   bool one=city.BastionOne.activeSelf,two=city.BastionTwo.activeSelf;city.BastionOne.SetActive(true);city.BastionTwo.SetActive(false);Views(city.PlayerCamera,"before-I");city.BastionOne.SetActive(false);city.BastionTwo.SetActive(true);Views(city.PlayerCamera,"before-II");city.BastionOne.SetActive(one);city.BastionTwo.SetActive(two);
   Debug.Log("VALORIA_REFERENCE_CLEAN_ARCHITECTURE_SURVEY_COMPLETE");EditorApplication.Exit(0);
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 }
}
#endif
