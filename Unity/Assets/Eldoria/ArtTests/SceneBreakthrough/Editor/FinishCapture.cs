using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
namespace Eldoria.Breakthrough {
public static partial class SceneCapture {
 static GameObject distant;
 static void RestoreTerrainMeshes() {
  foreach(var obj in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
   if(obj.name!="Main continuous buildable plateau"&&obj.name!="Upper terrace"&&obj.name!="Foreground bank")continue;
   foreach(var f in Directory.GetFiles(Root+"/Generated","*.asset")){
    var original=AssetDatabase.LoadAssetAtPath<Mesh>(f);if(original==null||original.name!=obj.name)continue;obj.sharedMesh=original;WorldUV(obj.gameObject,5);break;
   }
   if(obj.sharedMesh==null)throw new Exception("Terrain mesh recovery failed "+obj.name);
  }
 }
static void GeologicalSides(GameObject cap) {
  var mf=cap.GetComponent<MeshFilter>();var original=mf.sharedMesh;int n=(original.vertexCount-1)/2;
  var top=original.vertices.Take(n).ToArray();var center=Vector3.zero;foreach(var p in top)center+=p;center/=n;
  var verts=new List<Vector3>();var uvs=new List<Vector2>();var tris=new List<int>();float u=0;
  for(int i=0;i<n;i++){
   var a=top[i];var b=top[(i+1)%n];float length=Vector3.Distance(a,b);int segments=Mathf.Max(2,Mathf.CeilToInt(length/2));
   for(int j=0;j<segments;j++){
    var lo=Vector3.Lerp(a,b,j/(float)segments);var hi=Vector3.Lerp(a,b,(j+1)/(float)segments);
    var radial=(Vector3.ProjectOnPlane((lo+hi)*.5f-center,Vector3.up)).normalized;
    int start=verts.Count;
    foreach(var p in new[]{lo,hi})for(int band=0;band<4;band++){
     float t=band/3f;float wobble=Mathf.PerlinNoise(p.x*.37f+31,p.z*.31f+72)*.8f;
     float floor=cap.name=="Upper terrace"?7:cap.name=="Foreground bank"?-2:0;
     var point=p+radial*(t*(1.1f+wobble));point.y=Mathf.Lerp(p.y,floor,t)+(band==0||band==3?0:(wobble-.4f)*.65f);
     verts.Add(point);uvs.Add(new Vector2((u+(p==hi?length/segments:0))/4,point.y/4));
    }
    for(int band=0;band<3;band++){int aa=start+band,bb=start+4+band;tris.AddRange(new[]{aa,aa+1,bb,bb,aa+1,bb+1});}
    u+=length/segments;
   }
  }
  var mesh=new Mesh{name=cap.name+" continuous authored geological bands"};mesh.SetVertices(verts);mesh.SetUVs(0,uvs);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
  var geo=new GameObject(cap.name+" continuous escarpment");geo.AddComponent<MeshFilter>().sharedMesh=mesh;geo.AddComponent<MeshRenderer>().sharedMaterial=cap.GetComponent<Renderer>().sharedMaterials[1];geo.transform.SetParent(artRoot.transform,true);
  var copy=UnityEngine.Object.Instantiate(original);copy.subMeshCount=1;mf.sharedMesh=copy;cap.GetComponent<Renderer>().sharedMaterials=new[]{cap.GetComponent<Renderer>().sharedMaterials[0]};
 }
 static async System.Threading.Tasks.Task RepairGlobal(){
  // Correct true source orientation: the legacy GLB contract uses Y=180, without a camera-facing extra yaw.
  foreach(var p in artRoot.transform.Cast<Transform>().ToArray())if(p.name.EndsWith(" placement"))p.rotation=Quaternion.Euler(0,180,0);
  foreach(var p in artRoot.transform.Cast<Transform>().ToArray())if(p.name=="SOURCE_upper placement"||p.name=="SOURCE_cabin placement"||p.name=="SOURCE_bridge placement"||p.name=="SOURCE_cliff placement")UnityEngine.Object.DestroyImmediate(p.gameObject);
  // One dominant keep, two portal subsets and authored curtain wings; no old visual remains underneath.
  Fit(await Source("portal"),P(952,235,11),7.2f);
  Fit(await Source("tower"),P(870,218,11),4.6f);
  Fit(await Source("portal"),P(790,210,11),4.2f);
  Fit(await Source("wall"),P(685,225,11),8);
  Fit(await Source("wall"),P(1110,248,11),10);
  Fit(await Source("wall"),P(1250,268,11),5.2f);
  Fit(await Source("cabin"),P(451,398,7),6.8f);
  foreach(var r in artRoot.GetComponentsInChildren<Renderer>())foreach(var m in r.sharedMaterials)
   if(m.name.IndexOf("fabric_main",StringComparison.OrdinalIgnoreCase)>=0||m.name.IndexOf("fabric_secondary",StringComparison.OrdinalIgnoreCase)>=0)m.SetColor("_BaseColor",new Color(.08f,.2f,.48f));
  // The literal old screen-height assumption made an almost vertical staircase.
  // Lengthen its approach to retain a usable 4m rise and ~5.5m horizontal run.
  foreach(var o in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
   if(o.name.StartsWith("Stair tread")||o.name=="Stair traversable ramp"||o.name=="Road functional surface"||o.name=="Upper landing")o.gameObject.SetActive(false);
  stone=GameObject.Find("West partial wall")!=null?GameObject.Find("West partial wall").GetComponent<Renderer>().sharedMaterial:AssetDatabase.LoadAssetAtPath<Material>(Root+"/Generated/001.asset");
  path=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Generated/008.asset");foreach(var guid in AssetDatabase.FindAssets("t:Material",new[]{Root+"/GeneratedArt"})){var m=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));if(m!=null&&m.name.IndexOf("paving",StringComparison.OrdinalIgnoreCase)>=0){path=m;break;}}if(path==null)path=stone;
  var foot=P(850,405,7.05f);var top=P(905,270,11.05f);var rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(top-foot,Vector3.up),Vector3.up);
  for(int i=0;i<24;i++){var point=Vector3.Lerp(foot,top,(i+.5f)/24);var o=Box("Corrected stair tread "+i,point,new Vector3(6,.2f,Vector3.ProjectOnPlane(top-foot,Vector3.up).magnitude/24+.02f),stone);o.transform.rotation=rotation;WorldUV(o,3);}
  Strip("Corrected road",P(691,702,7.05f),foot,4.7f,.1f,path);Strip("Corrected upper landing",top,P(952,235,11.05f),4.7f,.1f,path);
  WorldUV(GameObject.Find("Corrected road"),3);WorldUV(GameObject.Find("Corrected upper landing"),3);
  var ramp=new GameObject("Corrected invisible walk support").AddComponent<BoxCollider>();ramp.transform.position=(foot+top)*.5f;ramp.transform.rotation=Quaternion.LookRotation(top-foot,Vector3.up);ramp.size=new Vector3(5,.1f,Vector3.Distance(foot,top));
  foreach(var c in new[]{"Main continuous buildable plateau","Upper terrace","Foreground bank"}){var cap=GameObject.Find(c);GeologicalSides(cap);var m=cap.GetComponent<Renderer>().sharedMaterial;m.SetColor("_BaseColor",new Color(.43f,.49f,.30f));}
 }

 static void BuildDistant() {
  distant=new GameObject("DISTANT_3D_ENVIRONMENT_NONINTERACTIVE");
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Holotna/Mountain/Models/Mountain01.fbx");
  if(prefab==null)throw new Exception("Existing mountain source missing");
  for(int layer=0;layer<3;layer++)for(int side=-1;side<=1;side++){
   var o=UnityEngine.Object.Instantiate(prefab);o.name="Existing mountain "+layer+" "+side;
   foreach(var c in o.GetComponentsInChildren<Collider>())UnityEngine.Object.DestroyImmediate(c);
   var b=BoundsOf(o);o.transform.localScale=new Vector3((120+layer*20)/b.size.x,(105+layer*40)/b.size.y,25/b.size.z);
   b=BoundsOf(o);o.transform.position-=new Vector3(b.center.x,b.min.y,b.center.z);
   var placement=new GameObject(o.name+" placement");o.transform.SetParent(placement.transform,true);
   placement.transform.rotation=Facing;placement.transform.position=Forward*(135+layer*55)+Right*(side*(75+layer*15));placement.transform.SetParent(distant.transform,true);
   var tint=Color.Lerp(new Color(.22f,.3f,.31f),new Color(.34f,.43f,.51f),layer*.4f);
   var m=Mat("Distant mountain tier "+layer,tint);m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Holotna/Mountain/Textures/Rock01_ALB.png"));
   foreach(var r in o.GetComponentsInChildren<Renderer>())r.sharedMaterial=m;
  }
  var waterObject=GameObject.Find("Water");if(waterObject!=null)waterObject.transform.SetParent(distant.transform,true);
  RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=145;RenderSettings.fogEndDistance=310;RenderSettings.fogColor=new Color(.28f,.37f,.46f);
 }
 static void Suite(string prefix) {
  var pose=cam.transform.position;float size=cam.orthographicSize;
  Capture(prefix+"-main",1536,1024);Capture(prefix+"-landscape",1280,720);Capture(prefix+"-portrait",390,844);
  cam.transform.position=pose+Right*10;Capture(prefix+"-pan-right",1536,1024);
  cam.transform.position=pose-Right*10;Capture(prefix+"-pan-left",1536,1024);
  cam.transform.position=pose;cam.orthographicSize=17;Capture(prefix+"-zoom-in",1536,1024);
  cam.orthographicSize=28;Capture(prefix+"-zoom-out",1280,720);
  cam.orthographicSize=size;cam.transform.position=pose;cam.aspect=1.5f;
 }
 static void FunctionalChecks() {
  Physics.SyncTransforms();var notes=new List<string>();
  // The continuous walk support must exist along bridge-road-stair-upper gate.
  var waypoints=new[]{P(420,924,3.05f),P(691,702,7.05f),P(850,405,7),P(905,270,11),P(952,235,11)};
  int count=0;
  for(int k=0;k<waypoints.Length-1;k++)for(int i=0;i<=20;i++){
   var p=Vector3.Lerp(waypoints[k],waypoints[k+1],i/20f);
   var hits=Physics.RaycastAll(p+Vector3.up*30,Vector3.down,70,~0,QueryTriggerInteraction.Ignore);
   if(!hits.Any(h=>Mathf.Abs(h.point.y-p.y)<.65f))throw new Exception("Uncovered access sample "+k+" "+i+" expectedHeight="+p.y);
   count++;
  }
  var before=cam.transform.position;var control=cam.GetComponent<SceneCamera>();
  control.Pan(new Vector2(50,20));if(Vector3.Distance(cam.transform.position,before)<.01)throw new Exception("Pan ineffective");control.Home();
  control.Zoom(-10000);if(Mathf.Abs(cam.orthographicSize-14)>.01)throw new Exception("Zoom lower bound failed");control.Home();
  foreach(var name in new[]{"Lower gate selection","Upper citadel selection","Bastion II expansion reserve"}) {
   var objects=UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Where(x=>x.name==name).ToArray();
   if(objects.Length==0)throw new Exception("Selection anchor absent "+name);
   foreach(var c in objects){var pixel=cam.WorldToScreenPoint(c.bounds.center);bool hit=control.SelectAt(pixel);notes.Add(name+" rayhit="+hit+" target="+control.Selected);if(!hit||control.Selected!=c.name)throw new Exception("Selection ray wrong target "+name+" actual="+control.Selected);}
  }
  notes.Add("route_support_samples="+count+"; actual raycasts, not NavMesh or device certification");
  File.WriteAllLines(Evidence+"/functional-checks.txt",notes);cam.transform.position=before;cam.orthographicSize=24;cam.aspect=1.5f;
 }
 static void BaselinePreviews() {
  var folder=Environment.GetEnvironmentVariable("RUNNER_TEMP")+"/breakthrough-baseline";
  if(!Directory.Exists(folder))throw new Exception("Frozen native baseline artifact missing");
  foreach(var name in new[]{"before-landscape","after-landscape","lookdev-landscape","after-portrait","after-mobile"}){
   var files=Directory.GetFiles(folder,name+".png",SearchOption.AllDirectories);if(files.Length==0)continue;
   var t=new Texture2D(2,2,TextureFormat.RGB24,false);if(!t.LoadImage(File.ReadAllBytes(files[0])))throw new Exception("Invalid baseline PNG");
   File.Copy(files[0],Evidence+"/frozen-"+name+".png",true);File.WriteAllBytes(Evidence+"/frozen-"+name+".jpg",t.EncodeToJPG(85));UnityEngine.Object.DestroyImmediate(t);
  }
 }
 static GameObject BakeDistant() {
  var hidden=new List<Renderer>();foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
   if(!r.transform.IsChildOf(distant.transform)&&r.enabled){r.enabled=false;hidden.Add(r);}
  var oldSize=cam.orthographicSize;var oldAspect=cam.aspect;var oldColor=cam.backgroundColor;cam.orthographicSize=48;cam.aspect=16f/9;cam.backgroundColor=Color.clear;
  var rt=new RenderTexture(2048,1152,24,RenderTextureFormat.ARGB32){antiAliasing=4};var tex=new Texture2D(2048,1152,TextureFormat.RGBA32,false);var previous=RenderTexture.active;
  bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
  try{cam.targetTexture=rt;cam.Render();cam.Render();cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,2048,1152),0,0);tex.Apply();
   Directory.CreateDirectory(Root+"/Horizon");File.WriteAllBytes(Root+"/Horizon/native-distant-bake.png",tex.EncodeToPNG());}
  finally{ShaderUtil.allowAsyncCompilation=async;cam.targetTexture=null;RenderTexture.active=previous;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);cam.orthographicSize=oldSize;cam.aspect=oldAspect;cam.backgroundColor=oldColor;foreach(var r in hidden)r.enabled=true;}
  AssetDatabase.ImportAsset(Root+"/Horizon/native-distant-bake.png",ImportAssetOptions.ForceSynchronousImport);
  var settings=(TextureImporter)AssetImporter.GetAtPath(Root+"/Horizon/native-distant-bake.png");settings.mipmapEnabled=true;settings.alphaIsTransparency=true;settings.wrapMode=TextureWrapMode.Clamp;settings.SaveAndReimport();
  var card=GameObject.CreatePrimitive(PrimitiveType.Quad);card.name="2.5D distant native bake - never reference";UnityEngine.Object.DestroyImmediate(card.GetComponent<Collider>());
  card.transform.position=cam.transform.position+cam.transform.forward*240;card.transform.rotation=cam.transform.rotation;card.transform.localScale=new Vector3(96*16/9,96,1);
  var material=new Material(Shader.Find("Eldoria/Breakthrough/NativeDistantBake"));material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Horizon/native-distant-bake.png"));card.GetComponent<Renderer>().sharedMaterial=material;
  distant.SetActive(false);return card;
 }
 public static async void Finish(){
  try{
   EditorSceneManager.OpenScene(Root+"/BastionI_Hybrid3D.unity",OpenSceneMode.Single);cam=Camera.main;cam.aspect=1.5f;cam.orthographicSize=24;
   artFolder=Root+"/GeneratedFinal/"+(Environment.GetEnvironmentVariable("GITHUB_SHA")??"local000").Substring(0,8);artRoot=GameObject.Find("HYBRID_3D_AUTHORED_ART");if(artRoot==null)throw new Exception("Hybrid scene missing");
   RestoreTerrainMeshes();await RepairGlobal();BuildDistant();FunctionalChecks();BaselinePreviews();Suite("final3d");
   SaveArt();AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),Root+"/BastionI_Hybrid3D_Final.unity");
   var card=BakeDistant();FunctionalChecks();Suite("final25d");SaveArt();AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),Root+"/BastionI_Hybrid25D_Final.unity");
   // Explicit orientation stress: fixed orientation is required; this is evidence, never hidden.
   var pose=cam.transform.rotation;cam.transform.Rotate(Vector3.up,10,Space.World);Capture("final25d-orbit-stress-unsupported",1536,1024);cam.transform.rotation=pose;
   File.WriteAllText(Evidence+"/comparison-result.json","{\"stage\":\"INDEPENDENT_VISUAL_REVIEW_REQUIRED\",\"foreground_3d\":true,\"distant_bake_source\":\"native existing 3D environment, never reference image\",\"orientation\":\"fixed, free orbit unsupported\",\"art_approved\":false,\"mobile_performance_pass\":false,\"source_commit\":\""+Environment.GetEnvironmentVariable("GITHUB_SHA")+"\"}");
   Debug.Log("BREAKTHROUGH_CAPTURE_COMPLETE");EditorApplication.Exit(0);
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
 }
}
}