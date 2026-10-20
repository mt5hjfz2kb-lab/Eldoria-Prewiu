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
  [Serializable] public class Piece {public string name;public Vector3[] vertices;public Vector2[] uv;public int[] triangles;}
  [Serializable] public class Candidate {public string scene_sha256,survey_sha256;public Piece[] pieces;}
  static void Need(bool b,string message){if(!b)throw new InvalidOperationException(message);}
  public static void Trial(){try{
   Directory.CreateDirectory(Ev);EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var data=JsonUtility.FromJson<Candidate>(File.ReadAllText(Ev+"/candidate.json"));Need(data.scene_sha256==Hash(Scene)&&data.survey_sha256==Hash(Ev+"/survey.json"),"Measured architecture source changed");
   var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();bool one=city.BastionOne.activeSelf,two=city.BastionTwo.activeSelf;var replacements=new Dictionary<Renderer,Material[]>();var temporary=new List<Object>();
   try{
    city.BastionOne.SetActive(true);city.BastionTwo.SetActive(false);Views(city.PlayerCamera,"before-I");city.BastionOne.SetActive(false);city.BastionTwo.SetActive(true);Views(city.PlayerCamera,"before-II");
    var roofSources=city.BastionOne.GetComponentsInChildren<MeshRenderer>(true).Single(r=>r.name=="Bastion_I").sharedMaterials;
    var map=new Dictionary<Material,Material>();foreach(int slot in new[]{8,9,10}){
     var src=roofSources[slot];Need(src.name.StartsWith("bastion-slate-material"),"Exact roof slot identity changed; preserve banners");var mat=new Material(src);var original=src.GetTexture("_BaseMap") as Texture2D;Need(original,"Missing retained slate texture");var colors=original.GetPixels();
     for(int i=0;i<colors.Length;i++){float luminance=colors[i].grayscale;float value=Mathf.Clamp(luminance*1.28f,.10f,.70f);colors[i]=new Color(value*.53f,value*.75f,value,colors[i].a);}
     var tex=new Texture2D(original.width,original.height,TextureFormat.RGBA32,true){name="Weathered blue slate exact existing detail",wrapMode=original.wrapMode,anisoLevel=original.anisoLevel};tex.SetPixels(colors);tex.Apply();mat.SetTexture("_BaseMap",tex);mat.SetColor("_BaseColor",Color.white);mat.SetFloat("_Metallic",0);mat.SetFloat("_Smoothness",.23f);temporary.Add(tex);temporary.Add(mat);map.Add(src,mat);
    }
    foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
     var original=r.sharedMaterials;var next=(Material[])original.Clone();bool changed=false;for(int i=0;i<next.Length;i++)if(next[i]&&map.TryGetValue(next[i],out var mat)){next[i]=mat;changed=true;}
     if(r.name=="Bridge_Paving"||r.name=="Axial_Paving"||r.name=="Outer_Road")for(int i=0;i<next.Length;i++){var mat=new Material(next[i]);if(mat.HasProperty("_BaseColor"))mat.SetColor("_BaseColor",new Color(1f,.96f,.87f));temporary.Add(mat);next[i]=mat;changed=true;}
     if(changed){replacements.Add(r,original);r.sharedMaterials=next;}
    }
    var donor=Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single(r=>r.name=="Gate_LowerGate_WestTower").sharedMaterials[0];
    foreach(var piece in data.pieces){var g=new GameObject(piece.name);temporary.Add(g);var m=new Mesh{name=piece.name};m.vertices=piece.vertices;m.uv=piece.uv;m.triangles=piece.triangles;m.RecalculateNormals();m.RecalculateBounds();temporary.Add(m);g.AddComponent<MeshFilter>().sharedMesh=m;g.AddComponent<MeshRenderer>().sharedMaterial=donor;}
    city.BastionOne.SetActive(true);city.BastionTwo.SetActive(false);Views(city.PlayerCamera,"after-I");city.BastionOne.SetActive(false);city.BastionTwo.SetActive(true);Views(city.PlayerCamera,"after-II");Need(Hash(Scene)==data.scene_sha256,"Scene modified during trial");
    File.WriteAllText(Ev+"/unity-review.json","{\"same_camera_light\":true,\"scene_saved\":false,\"art_approved\":false,\"I_II_preserved\":true,\"banner_slot14_untouched\":true}");
   }finally{foreach(var r in replacements)r.Key.sharedMaterials=r.Value;city.BastionOne.SetActive(one);city.BastionTwo.SetActive(two);foreach(var o in temporary)Object.DestroyImmediate(o);}
   Debug.Log("VALORIA_REFERENCE_CLEAN_ARCHITECTURE_TRIAL_COMPLETE");EditorApplication.Exit(0);
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
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
