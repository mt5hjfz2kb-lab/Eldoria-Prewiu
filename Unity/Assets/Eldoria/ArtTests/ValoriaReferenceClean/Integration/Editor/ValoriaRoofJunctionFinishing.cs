#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
public static class ValoriaRoofJunctionFinishing {
 const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
 const string Tool="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/ValoriaRoofJunctionFinishing.cs";
 static string Ev=>Path.GetFullPath("../docs/evidence/valoria-reference-clean/roof-junction226");
 static string Survey=>Path.GetFullPath("../docs/evidence/valoria-reference-clean/world-contact222/roof-source-survey.json.gz");
 [Serializable] public class Indices {public int[] indices;}
 [Serializable] public class Part {public string name;public Vector3[] vertices,normals;public Vector2[] uv,uv2;public Indices[] submeshes;}
 [Serializable] public class Candidate {public string source_scene_sha256,survey_sha256;public Part[] parts;}
 [Serializable] public class Acceptance {public bool accepted;public string source_scene_sha256,candidate_sha256,tool_sha256;public string[] images,hashes;}
 static string Hash(string p){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p))).Replace("-","").ToLowerInvariant();}
 static string ReadGzip(string path){using(var file=File.OpenRead(path))using(var zip=new System.IO.Compression.GZipStream(file,System.IO.Compression.CompressionMode.Decompress))using(var reader=new StreamReader(zip))return reader.ReadToEnd();}
 static void Need(bool b,string s){if(!b)throw new InvalidOperationException(s);}
 static void Views(ValoriaReferenceCleanGameplay city,string prefix){
  var c=city.PlayerCamera;var pos=c.transform.position;float zoom=c.orthographicSize;
  var objects=new[]{city.BastionOne,city.BastionTwo,city.Parcels.LeftBuilt,city.Parcels.RightBuilt};var active=objects.Select(o=>o.activeSelf).ToArray();
  try{for(int state=0;state<2;state++){
   objects[0].SetActive(state==0);objects[1].SetActive(state==1);objects[2].SetActive(state==1);objects[3].SetActive(state==1);string name=state==0?"I":"II";
   c.transform.position=pos;c.orthographicSize=zoom;
   ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-"+name+"-landscape.png",1448,1086);
   ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-"+name+"-portrait.png",900,1600);
   var r=objects[state].GetComponentsInChildren<MeshRenderer>(true).Single(x=>x.name=="Bastion_"+name);
   c.transform.position=r.bounds.center-c.transform.forward*100;c.orthographicSize=11;
   ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-"+name+"-roof-detail.png",1448,1086);
  }}finally{c.transform.position=pos;c.orthographicSize=zoom;for(int i=0;i<objects.Length;i++)objects[i].SetActive(active[i]);}
 }
 public static void Trial(){Run(false);}
 public static void Integrate(){Run(true);}
 static void Run(bool integrate){try{
  Directory.CreateDirectory(Ev);string sourceHash=Hash(Scene);var data=JsonUtility.FromJson<Candidate>(ReadGzip(Ev+"/candidate.json.gz"));
  Need(data.source_scene_sha256==sourceHash&&data.survey_sha256==Hash(Survey),"Exact measured current architecture required");Need(data.parts.Length==2,"Both established Bastion stages required");
  var approved=new Dictionary<string,byte[]>();if(integrate){var a=JsonUtility.FromJson<Acceptance>(File.ReadAllText(Ev+"/accepted-candidate.json"));Need(a.accepted&&a.source_scene_sha256==sourceHash&&a.candidate_sha256==Hash(Ev+"/candidate.json.gz")&&a.tool_sha256==Hash(Tool),"Exact reviewed roof candidate required");Need(a.images.Length==6&&a.hashes.Length==6,"Both states, both orientations and roof detail required");for(int i=0;i<6;i++){Need(Hash(Ev+"/"+a.images[i])==a.hashes[i],"Reviewed roof image changed");approved.Add(a.images[i],File.ReadAllBytes(Ev+"/"+a.images[i]));}}
  EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();var original=new Dictionary<MeshFilter,Mesh>();var owned=new List<Mesh>();bool saved=false;
  var colliders=Object.FindObjectsByType<MeshCollider>(FindObjectsInactive.Include,FindObjectsSortMode.None);var collisionMeshes=colliders.Select(c=>c.sharedMesh).ToArray();
  try{
   Views(city,"before");foreach(var part in data.parts){
    var root=part.name=="Bastion_I"?city.BastionOne:city.BastionTwo;var f=root.GetComponentsInChildren<MeshFilter>(true).Single(x=>x.name==part.name);original.Add(f,f.sharedMesh);
    Need(part.submeshes.Length==f.sharedMesh.subMeshCount&&part.submeshes.Length==f.GetComponent<Renderer>().sharedMaterials.Length,"Material identities changed");
    var m=new Mesh{name=part.name+" / seated slate and retained tower geometry",indexFormat=IndexFormat.UInt32};m.vertices=part.vertices.Select(v=>f.transform.InverseTransformPoint(v)).ToArray();m.normals=part.normals.Select(n=>f.transform.InverseTransformDirection(n).normalized).ToArray();m.uv=part.uv;if(part.uv2.Length==part.vertices.Length)m.uv2=part.uv2;m.subMeshCount=part.submeshes.Length;for(int i=0;i<m.subMeshCount;i++)m.SetTriangles(part.submeshes[i].indices,i);m.RecalculateBounds();m.RecalculateTangents();owned.Add(m);f.sharedMesh=m;
   }
   for(int i=0;i<colliders.Length;i++)Need(colliders[i].sharedMesh==collisionMeshes[i],"Architecture collision changed");Views(city,"after");Need(Hash(Scene)==sourceHash,"Trial unexpectedly saved scene");
   File.WriteAllText(Ev+"/unity-review.json","{\"same_camera_light\":true,\"both_bastion_stages\":true,\"materials_retained\":true,\"colliders_unchanged\":true,\"scene_saved\":false,\"art_approved\":false}");
   if(integrate){var differences=new List<string>();foreach(var a in approved){differences.Add(ValoriaTerrainBankAuthoring.CompareRender(a.Value,Ev+"/"+a.Key));File.WriteAllBytes(Ev+"/approved-"+a.Key,a.Value);}File.WriteAllText(Ev+"/render-identity.json","["+string.Join(",",differences)+"]");const string folder="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/RoofJunction226";Need(!Directory.Exists(folder),"Refuse overwrite of retained architecture");Directory.CreateDirectory(folder);AssetDatabase.Refresh();for(int i=0;i<owned.Count;i++)AssetDatabase.CreateAsset(owned[i],folder+"/Bastion-"+i+".asset");AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(city.gameObject.scene);Need(EditorSceneManager.SaveScene(city.gameObject.scene,Scene),"Roof integration save failed");saved=true;File.WriteAllText(Ev+"/integrated.json","{\"scene_sha256\":\""+Hash(Scene)+"\",\"gameplay_gate\":\"PENDING_CURRENT_PROBE\",\"published\":false}");}
  }finally{if(!saved){foreach(var p in original)p.Key.sharedMesh=p.Value;foreach(var m in owned)if(m)Object.DestroyImmediate(m);}}
  if(integrate)ValoriaIntegrationBuild.ValidateCurrent();else{Debug.Log("VALORIA_ROOF_JUNCTION_TRIAL_COMPLETE");EditorApplication.Exit(0);}
 }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
}}
#endif
