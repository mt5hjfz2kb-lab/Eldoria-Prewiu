#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Eldoria.Presentation;
using UnityEngine;
using UnityEditor;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
 public static class ValoriaFinalEnvironmentFinishing {
  const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
  static string Ev=>Path.GetFullPath("../docs/evidence/valoria-reference-clean/finish211");
  [Serializable] public class Part {public string name,path;public Vector3 min,max;public bool active;public string[] materials;public int vertices,triangles;}
  [Serializable] public class Data {public string scene_sha256;public Part[] parts;public Vector3 sawmill_min,sawmill_max,barracks_min,barracks_max;}
  static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
  static string Hash(string p){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p))).Replace("-","").ToLowerInvariant();}
  static Bounds BoundsOf(GameObject g){var rr=g.GetComponentsInChildren<Renderer>(true);var b=rr[0].bounds;foreach(var r in rr.Skip(1))b.Encapsulate(r.bounds);return b;}
  public static void SurveyCurrent(){
   Directory.CreateDirectory(Ev);var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();var parts=new List<Part>();
   foreach(var f in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
    var r=f.GetComponent<Renderer>();if(!r||!f.sharedMesh||f.transform.IsChildOf(host.WorldRoot.transform))continue;
    parts.Add(new Part{name=f.name,path=PathOf(f.transform),min=r.bounds.min,max=r.bounds.max,active=f.gameObject.activeInHierarchy&&r.enabled,materials=r.sharedMaterials.Select(m=>m?m.name:"").ToArray(),vertices=f.sharedMesh.vertexCount,triangles=f.sharedMesh.triangles.Length/3});
   }
   var saw=BoundsOf(city.Parcels.LeftBuilt);var barr=BoundsOf(city.Parcels.RightBuilt);
   File.WriteAllText(Ev+"/survey.json",JsonUtility.ToJson(new Data{scene_sha256=Hash(Scene),parts=parts.ToArray(),sawmill_min=saw.min,sawmill_max=saw.max,barracks_min=barr.min,barracks_max=barr.max},true));
   var targets=new[]{city.BastionOne,city.BastionTwo,city.Parcels.LeftBuilt,city.Parcels.RightBuilt};var states=targets.Select(t=>t.activeSelf).ToArray();
   try{Views(city.PlayerCamera,"before-city-initial");city.BastionOne.SetActive(false);city.BastionTwo.SetActive(true);city.Parcels.LeftBuilt.SetActive(true);city.Parcels.RightBuilt.SetActive(true);Views(city.PlayerCamera,"before-city-built");}
   finally{for(int i=0;i<targets.Length;i++)targets[i].SetActive(states[i]);}
  }
  static void Views(Camera camera,string prefix){ValoriaReferenceCleanBuilder.Capture(camera,Ev+"/"+prefix+"-landscape.png",1448,1086);ValoriaReferenceCleanBuilder.Capture(camera,Ev+"/"+prefix+"-portrait.png",900,1600);}
 }
}
#endif
