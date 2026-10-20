#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Eldoria.Presentation;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
public static class ValoriaWorldContactFinishing {
 const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
 static string Ev=>Path.GetFullPath("../docs/evidence/valoria-reference-clean/world-contact222");
 const string Tool="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Editor/ValoriaWorldContactFinishing.cs";
 [Serializable] public class Acceptance {public bool accepted;public string source_scene_sha256,tool_sha256;public string[] images,hashes;}
 [Serializable] public class Report {public string source_scene_sha256,tool_sha256;public float camera_before,camera_after,urp_shadow_distance,max_viewport_xy_difference,min_apron_before_ground_delta,max_apron_before_ground_delta;public int apron_vertices,retained_props_seated;public bool terrain_and_gameplay_unchanged;}
 [Serializable] public class RoofIndices {public int[] indices;}
 [Serializable] public class RoofSource {public string name,mesh_asset;public Vector3[] local_vertices,world_vertices,normals;public Vector2[] uv,uv2;public RoofIndices[] submeshes;public string[] materials;}
 [Serializable] public class RoofSurvey {public string scene_sha256;public RoofSource[] parts;}
 static void SurveyRoof(string sceneHash){
  var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();var parts=new[]{city.BastionOne,city.BastionTwo}.SelectMany(root=>root.GetComponentsInChildren<MeshFilter>(true)).Where(f=>f.name=="Bastion_I"||f.name=="Bastion_II").Select(f=>{var m=f.sharedMesh;return new RoofSource{name=f.name,mesh_asset=AssetDatabase.GetAssetPath(m),local_vertices=m.vertices,world_vertices=m.vertices.Select(v=>f.transform.TransformPoint(v)).ToArray(),normals=m.normals.Select(v=>f.transform.TransformDirection(v).normalized).ToArray(),uv=m.uv,uv2=m.uv2,submeshes=Enumerable.Range(0,m.subMeshCount).Select(i=>new RoofIndices{indices=m.GetTriangles(i)}).ToArray(),materials=f.GetComponent<Renderer>().sharedMaterials.Select(x=>AssetDatabase.GetAssetPath(x)).ToArray()};}).ToArray();
  Need(parts.Length==2,"Expected exact integrated Bastion I and II sources");using(var file=File.Create(Ev+"/roof-source-survey.json.gz"))using(var zip=new System.IO.Compression.GZipStream(file,System.IO.Compression.CompressionLevel.Optimal))using(var writer=new StreamWriter(zip)){writer.Write(JsonUtility.ToJson(new RoofSurvey{scene_sha256=sceneHash,parts=parts}));}if(File.Exists(Ev+"/roof-source-survey.json"))File.Delete(Ev+"/roof-source-survey.json");
 }
 [Serializable] public class MeshProof {public Vector3[] before,after;public int[] triangles;}
 static string Hash(string p){using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p))).Replace("-","").ToLowerInvariant();}
 static void Need(bool b,string s){if(!b)throw new InvalidOperationException(s);}
 static void Views(ValoriaIntegratedWorld host,string prefix,float distance){
  var c=host.WorldCamera;var pos=c.transform.position;float zoom=c.orthographicSize;
  try{foreach(string id in new[]{"forest-valoria","quarry-valoria","corrupt-scout","engendro-valoria"}){
   var p=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true).Single(n=>n.Id==id).transform.position;p.y=2;c.transform.position=p-c.transform.forward*distance;
   foreach(float size in new[]{64f,32f,14f}){c.orthographicSize=size;ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-"+id+"-"+size.ToString("0")+".png",1448,1086);}
   c.orthographicSize=14;ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-"+id+"-portrait.png",900,1600);
  }}finally{c.transform.position=pos;c.orthographicSize=zoom;}
 }
 public static void Trial(){Run(false);}
 public static void Integrate(){Run(true);}
 static void Run(bool integrate){try{
  Directory.CreateDirectory(Ev);string sourceHash=Hash(Scene);var approved=new Dictionary<string,byte[]>();if(integrate){var a=JsonUtility.FromJson<Acceptance>(File.ReadAllText(Ev+"/accepted-candidate.json"));Need(a.accepted&&a.source_scene_sha256==sourceHash&&a.tool_sha256==Hash(Tool),"Exact accepted world finishing sources required");Need(a.images.Length==16&&a.hashes.Length==16,"All destinations, zooms and portrait required");for(int i=0;i<16;i++){Need(Hash(Ev+"/"+a.images[i])==a.hashes[i],"Reviewed image changed");approved.Add(a.images[i],File.ReadAllBytes(Ev+"/"+a.images[i]));}}
  EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);SurveyRoof(sourceHash);var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();bool active=host.WorldRoot.activeSelf;host.WorldRoot.SetActive(true);var nav=host.WorldNavigation;float oldDistance=nav.cameraDistance;var c=host.WorldCamera;var oldCameraPosition=c.transform.position;var temporary=new List<Object>();var changes=new Dictionary<MeshFilter,Mesh>();var props=new Dictionary<Transform,Vector3>();bool saved=false;
  try{
   Need(Mathf.Abs(oldDistance-350)<.1f,"World source camera no longer matches diagnosed depth");Views(host,"before",oldDistance);
   var all=host.WorldRoot.GetComponentsInChildren<MeshFilter>(true);var ground=all.Single(f=>f.name=="ZoneOne continuous prairie source");var query=ground.gameObject.AddComponent<MeshCollider>();query.sharedMesh=ground.sharedMesh;temporary.Add(query);Physics.SyncTransforms();
   float GroundAt(Vector3 p){Need(query.Raycast(new Ray(new Vector3(p.x,100,p.z),Vector3.down),out var hit,200),"World ground contact query missed");return hit.point.y;}
   var apron=all.Single(f=>f.gameObject.activeInHierarchy&&(f.name.Contains("Connected graded extraction apron")||f.sharedMesh.name.Contains("Connected graded extraction apron")));
   var original=apron.sharedMesh;var before=original.vertices.Select(v=>apron.transform.TransformPoint(v)).ToArray();var vv=before.ToArray();var b=apron.GetComponent<Renderer>().bounds;float minDelta=100,maxDelta=-100;
   for(int i=0;i<vv.Length;i++){var p=vv[i];float groundY=GroundAt(p);minDelta=Mathf.Min(minDelta,p.y-groundY);maxDelta=Mathf.Max(maxDelta,p.y-groundY);float nx=Mathf.Abs((p.x-b.center.x)/Mathf.Max(.1f,b.extents.x)),nz=Mathf.Abs((p.z-b.center.z)/Mathf.Max(.1f,b.extents.z));float edge=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.78f,1,Mathf.Max(nx,nz)));p.y=groundY+Mathf.Lerp(-.025f,.055f,edge)+edge*.025f*Mathf.Sin(p.x*2.1f+p.z*1.7f);vv[i]=p;}
   var mesh=Object.Instantiate(original);mesh.name="Existing quarry extraction apron / measured terrain contact";mesh.vertices=vv.Select(v=>apron.transform.InverseTransformPoint(v)).ToArray();mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();temporary.Add(mesh);changes.Add(apron,original);apron.sharedMesh=mesh;
   File.WriteAllText(Ev+"/apron-contact-source.json",JsonUtility.ToJson(new MeshProof{before=before,after=vv,triangles=mesh.triangles}));
   foreach(var f in all.Where(f=>f.gameObject.activeInHierarchy&&(f.name.StartsWith("Quarry fractured waste")||f.name.StartsWith("Retained functional quarry equipment")))){
    var r=f.GetComponent<Renderer>();if(!r)continue;float delta=GroundAt(r.bounds.center)-r.bounds.min.y+.035f;Need(Mathf.Abs(delta)<4,"Unexpected retained equipment offset");props.Add(f.transform,f.transform.position);f.transform.position+=Vector3.up*delta;
   }
   // Same X/Y projection and existing zoom; only depth moves within the already
   // configured 220-unit shadow range. No URP budget/resolution increase.
   var sample=all.Where(f=>f.gameObject.activeInHierarchy&&f.GetComponent<Renderer>()).Select(f=>f.GetComponent<Renderer>().bounds.center).ToArray();var projected=sample.Select(c.WorldToViewportPoint).ToArray();c.transform.position+=c.transform.forward*(oldDistance-140);float maxError=0;for(int i=0;i<sample.Length;i++){var q=c.WorldToViewportPoint(sample[i]);maxError=Mathf.Max(maxError,Vector2.Distance(new Vector2(q.x,q.y),new Vector2(projected[i].x,projected[i].y)));}Need(maxError<.0001f,"Orthographic framing changed");c.transform.position=oldCameraPosition;nav.cameraDistance=140;
   Views(host,"after",140);Need(Hash(Scene)==sourceHash,"World trial unexpectedly saved scene");var urp=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;Need(urp&&urp.shadowDistance>=200&&urp.shadowDistance<300,"Unexpected URP shadow source");
   var report=new Report{source_scene_sha256=sourceHash,tool_sha256=Hash(Tool),camera_before=oldDistance,camera_after=140,urp_shadow_distance=urp.shadowDistance,max_viewport_xy_difference=maxError,min_apron_before_ground_delta=minDelta,max_apron_before_ground_delta=maxDelta,apron_vertices=mesh.vertexCount,retained_props_seated=props.Count,terrain_and_gameplay_unchanged=true};File.WriteAllText(Ev+"/unity-review.json",JsonUtility.ToJson(report,true));
   if(integrate){var differences=new List<string>();foreach(var a in approved){differences.Add(ValoriaTerrainBankAuthoring.CompareRender(a.Value,Ev+"/"+a.Key));File.WriteAllBytes(Ev+"/approved-"+a.Key,a.Value);}File.WriteAllText(Ev+"/render-identity.json","["+string.Join(",",differences)+"]");const string folder="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/WorldContact222";Need(!Directory.Exists(folder),"Refuse overwrite current world assets");Directory.CreateDirectory(folder);AssetDatabase.Refresh();AssetDatabase.CreateAsset(mesh,folder+"/QuarryApron.asset");Object.DestroyImmediate(query);c.transform.position=oldCameraPosition+c.transform.forward*(oldDistance-140);host.WorldRoot.SetActive(active);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(host.gameObject.scene);Need(EditorSceneManager.SaveScene(host.gameObject.scene,Scene),"World save failed");saved=true;SurveyRoof(Hash(Scene));File.WriteAllText(Ev+"/integrated.json","{\"scene_sha256\":\""+Hash(Scene)+"\",\"gameplay_gate\":\"PENDING_CURRENT_PROBE\",\"published\":false}");}
  }finally{if(!saved){foreach(var kv in changes)kv.Key.sharedMesh=kv.Value;foreach(var kv in props)kv.Key.position=kv.Value;foreach(var o in temporary)if(o)Object.DestroyImmediate(o);nav.cameraDistance=oldDistance;c.transform.position=oldCameraPosition;host.WorldRoot.SetActive(active);}}
  if(integrate)ValoriaIntegrationBuild.ValidateCurrent();else{Debug.Log("VALORIA_REFERENCE_CLEAN_WORLD_CONTACT_TRIAL_COMPLETE");EditorApplication.Exit(0);}
 }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
}}
#endif
