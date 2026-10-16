using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace Eldoria.Breakthrough {
public static partial class SceneCapture {
 static void CloneSceneMaterials(){
  var copies=new Dictionary<Material,Material>();
  foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(UnityEngine.FindObjectsInactive.Include,FindObjectsSortMode.None)){
   var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)if(mats[i]!=null){if(!copies.ContainsKey(mats[i]))copies[mats[i]]=new Material(mats[i]){name=mats[i].name+" review copy"};mats[i]=copies[mats[i]];}r.sharedMaterials=mats;
  }
 }
 static void PlotSelection(GameObject group,string name){
  var b=BoundsOf(group);var o=new GameObject(name);o.transform.position=b.center;var c=o.AddComponent<BoxCollider>();c.isTrigger=true;c.size=b.size+Vector3.one*.2f;
 }
 static void RecordMetrics(string name){
  var renderers=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();
  var meshes=new HashSet<Mesh>();var mats=new HashSet<Material>();var textures=new HashSet<Texture>();long triangles=0;
  foreach(var r in renderers){var f=r.GetComponent<MeshFilter>();if(f!=null&&f.sharedMesh!=null){meshes.Add(f.sharedMesh);triangles+=f.sharedMesh.triangles.Length/3;}foreach(var m in r.sharedMaterials)if(m!=null){mats.Add(m);foreach(var property in m.GetTexturePropertyNames())if(m.GetTexture(property)!=null)textures.Add(m.GetTexture(property));}}
  var foreground=new System.Text.StringBuilder();foreach(var rr in renderers.OrderBy(x=>x.name)){if(rr.transform.IsChildOf(distant.transform)||rr.name.StartsWith("2.5D distant"))continue;var ff=rr.GetComponent<MeshFilter>();if(ff==null)continue;foreground.AppendLine(rr.name+" "+ff.sharedMesh.vertexCount+" "+ff.sharedMesh.triangles.Length+" "+rr.transform.localToWorldMatrix.ToString("R")+" "+String.Join(";",rr.sharedMaterials.Select(mm=>mm.GetColor("_BaseColor").ToString("R"))));}File.WriteAllText(Evidence+"/"+name+"-foreground-fingerprint.txt",foreground.ToString());
  File.WriteAllText(Evidence+"/"+name+"-metrics.json","{\"active_renderers\":"+renderers.Length+",\"instance_triangles\":"+triangles+",\"shared_meshes\":"+meshes.Count+",\"shared_materials\":"+mats.Count+",\"shared_textures\":"+textures.Count+",\"physical_device_performance_measured\":false}");
 }
 public static async void ReviewCandidate(){
  try{
   EditorSceneManager.OpenScene(Root+"/BastionI_Hybrid3D_Final.unity",OpenSceneMode.Single);cam=Camera.main;cam.aspect=1.5f;cam.orthographicSize=24;
   artRoot=GameObject.Find("HYBRID_3D_AUTHORED_ART");distant=GameObject.Find("DISTANT_3D_ENVIRONMENT_NONINTERACTIVE");distant.SetActive(true);
   artFolder=Root+"/GeneratedReview/"+(Environment.GetEnvironmentVariable("GITHUB_SHA")??"local000").Substring(0,8);
   CloneSceneMaterials();int retired=0;
   foreach(var p in artRoot.transform.Cast<Transform>().ToArray()){
    if(p.name.StartsWith("SOURCE_cliff",StringComparison.Ordinal)){UnityEngine.Object.DestroyImmediate(p.gameObject);retired++;continue;}
    if(p.name=="SOURCE_tower placement")p.localScale*=9.1f/BoundsOf(p.gameObject).size.y;
    if(p.name=="SOURCE_portal placement"){float height=cam.WorldToViewportPoint(p.position).x>.55f?6.8f:3.8f;p.localScale*=height/BoundsOf(p.gameObject).size.y;}
   }
   if(artRoot.GetComponentsInChildren<Transform>(true).Any(t=>t.name.StartsWith("SOURCE_cliff",StringComparison.Ordinal)))throw new Exception("Retired cliff overlay remains");
   var tinted=new HashSet<Material>();int pineIndex=0;
   foreach(var p in artRoot.transform.Cast<Transform>().Where(t=>t.name.StartsWith("SOURCE_pine")).ToArray()){
    if(pineIndex++%3==1){UnityEngine.Object.DestroyImmediate(p.gameObject);continue;}
    p.localScale*=(7.5f+(pineIndex%4)*.5f)/BoundsOf(p.gameObject).size.y;
    foreach(var r in p.GetComponentsInChildren<Renderer>())foreach(var m in r.sharedMaterials)if(tinted.Add(m)&&!m.name.ToLowerInvariant().Contains("trunk")&&!m.name.ToLowerInvariant().Contains("bark")){var c=m.GetColor("_BaseColor");m.SetColor("_BaseColor",new Color(c.r*.42f,c.g*.57f,c.b*.45f,c.a));}
   }
   foreach(var capName in new[]{"Main continuous buildable plateau","Upper terrace","Foreground bank"}){var cap=GameObject.Find(capName);cap.GetComponent<Renderer>().sharedMaterial.SetColor("_BaseColor",new Color(.43f,.49f,.30f));var mf=cap.GetComponent<MeshFilter>();mf.sharedMesh=UnityEngine.Object.Instantiate(mf.sharedMesh);mf.sharedMesh.RecalculateNormals();}
   foreach(var r in artRoot.GetComponentsInChildren<Renderer>())foreach(var m in r.sharedMaterials)if(m.name.IndexOf("fabric_",StringComparison.OrdinalIgnoreCase)>=0)m.SetColor("_BaseColor",new Color(.10f,.22f,.52f));
   foreach(var p in artRoot.transform.Cast<Transform>().ToArray())if(p.name=="SOURCE_cabin placement")UnityEngine.Object.DestroyImmediate(p.gameObject);
   var cabin=await Source("cabinpainted");Fit(cabin,P(451,398,7),6.7f);PlotSelection(cabin,"Cabin plot selection");
   var camp=artRoot.transform.Cast<Transform>().First(t=>t.name=="SOURCE_camp placement").gameObject;PlotSelection(camp,"Camp plot selection");
   int mountainsVisible=0;int layer=0;
   foreach(var placement in distant.transform.Cast<Transform>().Where(t=>t.name.StartsWith("Existing mountain")).ToArray()){
    var words=placement.name.Split(' ');int tier=int.Parse(words[2]);int side=int.Parse(words[3]);
    placement.position=Vector3.zero;placement.rotation=Quaternion.identity;
    var model=placement.GetChild(0).gameObject;var b=BoundsOf(model);
    model.transform.localScale=Vector3.Scale(model.transform.localScale,new Vector3((120+tier*20)/b.size.x,(40+tier*15)/b.size.y,25/b.size.z));
    b=BoundsOf(model);model.transform.position-=new Vector3(b.center.x,b.min.y,b.center.z);
    placement.rotation=Facing;placement.position=Forward*(35+tier*25)+Right*(side*(70+tier*10))+Vector3.up*(-20-tier*15);
    foreach(var r in model.GetComponentsInChildren<Renderer>())if(GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(cam),r.bounds))mountainsVisible++;
   }
   if(mountainsVisible==0)throw new Exception("Distant scenery outside player frustum");
   foreach(var o in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).ToArray())if(o.name.StartsWith("Bridge repaired"))UnityEngine.Object.DestroyImmediate(o.gameObject);
   var bridge=await Source("bridge");bridge.name="Canonical reused bridge geometry";bridge.transform.SetParent(artRoot.transform,false);bridge.transform.rotation=Quaternion.Euler(0,180,0);bridge.transform.position=new Vector3(2,3.2f,-30);
   foreach(var mf in bridge.GetComponentsInChildren<MeshFilter>()){var c=mf.gameObject.AddComponent<MeshCollider>();c.sharedMesh=mf.sharedMesh;}
   var paving=new Texture2D(2,2,TextureFormat.RGB24,false);paving.name="Existing authored stone paving";paving.LoadImage(File.ReadAllBytes("../art-source/valoria/production/bastion-family-v1/paving_albedo.png"));paving.wrapMode=TextureWrapMode.Repeat;
   var paved=Mat("Shared existing stone paving",Color.white);paved.SetTexture("_BaseMap",paving);
   foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))if(r.name.StartsWith("Corrected stair tread")||r.name=="Corrected road"||r.name=="Corrected upper landing"){r.sharedMaterial=paved;WorldUV(r.gameObject,3);}
   var outside=P(470,930,3.05f);var foot=P(540,843,3.2f);var end=P(670,713,7.05f);var gate=P(691,702,7.05f);
   Strip("Foreground access road",outside,foot,6,.15f,paved);WorldUV(GameObject.Find("Foreground access road"),3);
   Strip("Bridge gate threshold",end,gate,5,.15f,paved);WorldUV(GameObject.Find("Bridge gate threshold"),3);
   var walk=new GameObject("Canonical reused bridge walk support").AddComponent<BoxCollider>();walk.transform.position=(foot+end)*.5f-Vector3.up*.12f;walk.transform.rotation=Quaternion.LookRotation(end-foot,Vector3.up);walk.size=new Vector3(5.5f,.15f,Vector3.Distance(foot,end));
   var capOld=GameObject.Find("Foreground bank");var mats=capOld.GetComponent<Renderer>().sharedMaterials;var cliffMaterial=GameObject.Find("Foreground bank continuous escarpment").GetComponent<Renderer>().sharedMaterial;
   Deck("Foreground bank extension",new[]{new Vector2(410,940),new Vector2(570,970),new Vector2(635,875),new Vector2(445,815)},3.1f,-1,mats[0],cliffMaterial);
   GeologicalSides(GameObject.Find("Foreground bank extension"));
   foreach(var r in GameObject.Find("Foreground bank extension").GetComponentsInChildren<Renderer>())WorldUV(r.gameObject,5);
   // Real proxies and exact same foreground are retained in both variants.
   FunctionalChecks();File.Copy(Evidence+"/functional-checks.txt",Evidence+"/functional-checks-3d.txt",true);Suite("final3d");RecordMetrics("final3d");
   SaveArt();AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),Root+"/BastionI_Hybrid3D_Final.unity");
   BakeDistant();FunctionalChecks();File.Copy(Evidence+"/functional-checks.txt",Evidence+"/functional-checks-25d.txt",true);Suite("final25d");RecordMetrics("final25d");
   SaveArt();AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),Root+"/BastionI_Hybrid25D_Final.unity");
   File.WriteAllText(Evidence+"/global-correction-facts.json","{\"legacy_cliff_instances_destroyed\":"+retired+",\"distant_renderers_in_frustum\":"+mountainsVisible+",\"bridge\":\"immutable source canonical pivot and Y180, no bounding-box fit\",\"art_approved\":false}");
   var rotation=cam.transform.rotation;cam.transform.Rotate(Vector3.up,10,Space.World);Capture("final25d-orbit-stress-unsupported",1536,1024);cam.transform.rotation=rotation;
   Debug.Log("BREAKTHROUGH_CAPTURE_COMPLETE");EditorApplication.Exit(0);
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
 }
}
}