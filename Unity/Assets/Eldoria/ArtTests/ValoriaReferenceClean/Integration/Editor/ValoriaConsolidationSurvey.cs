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
 // Read-only source survey. No scene, asset, save or gameplay writes.
 public static class ValoriaConsolidationSurvey {
  const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
  [Serializable] public class Part {public string name,path,mesh;public bool enabled,active;public Vector3 min,max;public Vector3[] vertices,normals;public Vector2[] uv;public Slot[] slots;}
  [Serializable] public class Slot {public string material,asset,shader,texture;public Color color;public int[] triangles;}
  [Serializable] public class Report {public string sourceCommit,sceneSha256,referenceSha256;public Vector3 cameraPosition,cameraEuler;public float zoom;public Part[] parts;}
  static string Hash(string path){using(var s=SHA256.Create())return BitConverter.ToString(s.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
  static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
  public static void Survey(){try{
   var ev=Path.GetFullPath("../docs/evidence/valoria-reference-clean/consolidation-20261027/"+(Environment.GetEnvironmentVariable("GITHUB_RUN_ID")??"local"));
   if(Directory.Exists(ev)&&Directory.GetFiles(ev).Length>0)throw new InvalidOperationException("Preserve existing survey; use a fresh run");
   Directory.CreateDirectory(ev);var original=Hash(Scene);
   EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
   var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
   if(!city||!host||!city.BastionOne||!city.BastionTwo)throw new InvalidOperationException("Current I-II city bindings required");
   var parts=new List<Part>();
   foreach(var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
    var r=f.GetComponent<Renderer>();if(!r||!f.sharedMesh||f.transform.IsChildOf(host.WorldRoot.transform))continue;
    if(!(f.name.StartsWith("Bridge_")||f.name.StartsWith("Gate_")||f.name.StartsWith("Bastion_")||f.name.IndexOf("bank",StringComparison.OrdinalIgnoreCase)>=0||f.name.IndexOf("rock",StringComparison.OrdinalIgnoreCase)>=0))continue;
    var m=f.sharedMesh;var slots=new List<Slot>();
    for(int i=0;i<m.subMeshCount;i++){
     var mat=i<r.sharedMaterials.Length?r.sharedMaterials[i]:null;
     var tp=mat&&mat.HasProperty("_BaseMap")?"_BaseMap":mat&&mat.HasProperty("baseColorTexture")?"baseColorTexture":"_MainTex";
     var cp=mat&&mat.HasProperty("_BaseColor")?"_BaseColor":mat&&mat.HasProperty("baseColorFactor")?"baseColorFactor":"_Color";
     slots.Add(new Slot{material=mat?mat.name:"",asset=AssetDatabase.GetAssetPath(mat),shader=mat?mat.shader.name:"",texture=mat&&mat.HasProperty(tp)?AssetDatabase.GetAssetPath(mat.GetTexture(tp)):"",color=mat&&mat.HasProperty(cp)?mat.GetColor(cp):Color.white,triangles=m.GetTriangles(i)});
    }
    parts.Add(new Part{name=f.name,path=PathOf(f.transform),mesh=AssetDatabase.GetAssetPath(m),enabled=r.enabled,active=r.gameObject.activeInHierarchy,min=r.bounds.min,max=r.bounds.max,vertices=m.vertices.Select(f.transform.TransformPoint).ToArray(),normals=m.normals.Select(n=>f.transform.TransformDirection(n).normalized).ToArray(),uv=m.uv,slots=slots.ToArray()});
   }
   var cam=city.PlayerCamera;
   File.WriteAllText(ev+"/survey.json",JsonUtility.ToJson(new Report{sourceCommit=Environment.GetEnvironmentVariable("GITHUB_SHA"),sceneSha256=original,referenceSha256=Hash("../references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg"),cameraPosition=cam.transform.position,cameraEuler=cam.transform.eulerAngles,zoom=cam.orthographicSize,parts=parts.ToArray()}));
   bool one=city.BastionOne.activeSelf,two=city.BastionTwo.activeSelf;
   try{foreach(int level in new[]{1,2}){
    city.BastionOne.SetActive(level==1);city.BastionTwo.SetActive(level==2);
    ValoriaReferenceCleanBuilder.Capture(cam,ev+"/city-"+level+"-horizontal.png",1448,1086);
    ValoriaReferenceCleanBuilder.Capture(cam,ev+"/city-"+level+"-vertical.png",900,1600);
   }}finally{city.BastionOne.SetActive(one);city.BastionTwo.SetActive(two);}
   if(Hash(Scene)!=original)throw new InvalidOperationException("Survey changed scene bytes");
   Debug.Log("VALORIA_REFERENCE_CLEAN_CONSOLIDATION_SURVEY_COMPLETE parts="+parts.Count+" scene_unchanged=1");EditorApplication.Exit(0);
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 }
}
#endif
