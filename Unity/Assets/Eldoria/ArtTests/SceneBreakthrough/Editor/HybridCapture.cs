using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
namespace Eldoria.Breakthrough {
public static partial class SceneCapture {
 static readonly HashSet<UnityEngine.Object> savedArt=new HashSet<UnityEngine.Object>();
 static readonly List<string> importFacts=new List<string>();
 static GameObject artRoot;
 static Bounds BoundsOf(GameObject g){var rr=g.GetComponentsInChildren<Renderer>(true);if(rr.Length==0)throw new Exception("Empty source "+g.name);var b=rr[0].bounds;foreach(var r in rr)b.Encapsulate(r.bounds);return b;}
 static async Task<GameObject> Source(string key) {
  string file=Path.GetFullPath(Root+"/Sources/"+key+"/"+key+".glb");
  var loader=new GltfImport(null,new UninterruptedDeferAgent());
  if(!await loader.Load(new Uri(file).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=2}))throw new Exception("Import failed "+key);
  var group=new GameObject("Authored source "+key);
  if(!await loader.InstantiateMainSceneAsync(group.transform))throw new Exception("Instantiate failed "+key);
  SceneMaterials.Apply(group,loader,file);
  // Preserve immutable authored geometry, combine only render partitions by material.
  var original=group.GetComponentsInChildren<MeshRenderer>(true);var bins=new Dictionary<Material,List<CombineInstance>>();
  foreach(var r in original) {
   var filter=r.GetComponent<MeshFilter>();if(filter==null||filter.sharedMesh==null)continue;
   var m=r.sharedMaterials;for(int i=0;i<Math.Min(m.Length,filter.sharedMesh.subMeshCount);i++){
    if(!bins.ContainsKey(m[i]))bins[m[i]]=new List<CombineInstance>();
    bins[m[i]].Add(new CombineInstance{mesh=filter.sharedMesh,subMeshIndex=i,transform=group.transform.worldToLocalMatrix*r.transform.localToWorldMatrix});
   }
  }
  var sourceBounds=BoundsOf(group);importFacts.Add(key+" bounds="+sourceBounds.ToString("F3"));var combined=new GameObject("SOURCE_"+key);
  foreach(var bin in bins) {
   var mesh=new Mesh{name=key+" "+bin.Key.name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(bin.Value.ToArray(),true,true);
   var o=new GameObject(bin.Key.name);o.transform.SetParent(combined.transform,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=bin.Key;
  }
  UnityEngine.Object.DestroyImmediate(group);importFacts.Add(key+" authored_renderers="+original.Length+" combined="+bins.Count);
  return combined;
 }
 static void Fit(GameObject g,Vector3 foot,float width,float depth=0) {
   var b=BoundsOf(g);float s=depth>0?depth/b.size.y:width/b.size.x;g.transform.localScale*=s;
   b=BoundsOf(g);g.transform.position-=new Vector3(b.center.x,b.min.y,b.center.z);
   var parent=new GameObject(g.name+" placement");g.transform.SetParent(parent.transform,true);
   parent.transform.rotation=Facing*Quaternion.Euler(0,180,0);parent.transform.position=foot;
   parent.transform.SetParent(artRoot.transform,true);g.SetActive(true);
 }
 static void RemoveVisualPrefix(params string[] prefixes) {
   foreach(var o in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
    if(prefixes.Any(p=>o.name.StartsWith(p,StringComparison.Ordinal)))o.gameObject.SetActive(false);
 }
 static void WorldUV(GameObject o,float tile) {
   var filter=o.GetComponent<MeshFilter>();if(filter==null)return;
   var mesh=UnityEngine.Object.Instantiate(filter.sharedMesh);mesh.name=o.name+" metric UV";
   var uv=new Vector2[mesh.vertexCount];var verts=mesh.vertices;
   for(int i=0;i<uv.Length;i++){var p=o.transform.TransformPoint(verts[i]);uv[i]=new Vector2(p.x/tile,p.z/tile);}
   mesh.uv=uv;filter.sharedMesh=mesh;
 }
 static void PersistArt(UnityEngine.Object obj){
   if(obj==null||AssetDatabase.Contains(obj)||savedArt.Contains(obj))return;savedArt.Add(obj);
   string file=Root+"/GeneratedArt/"+savedArt.Count.ToString("D4")+".asset";AssetDatabase.CreateAsset(obj,file);
 }
 static void SaveArt() {
  Directory.CreateDirectory(Root+"/GeneratedArt");
  foreach(var f in artRoot.GetComponentsInChildren<MeshFilter>(true))PersistArt(f.sharedMesh);
  foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))foreach(var m in r.sharedMaterials) {
   if(m==null)continue;foreach(var p in m.GetTexturePropertyNames())PersistArt(m.GetTexture(p));PersistArt(m);
  }
 }
 public static async void Hybrid() {
  try {
   EditorSceneManager.OpenScene(Root+"/BastionI_Blockout.unity",OpenSceneMode.Single);cam=Camera.main;
   cam.aspect=1.5f;cam.orthographicSize=24;
   artRoot=new GameObject("HYBRID_3D_AUTHORED_ART");Directory.CreateDirectory(Evidence);
   var oldBridge=GameObject.Find("Bridge functional deck");if(oldBridge!=null)UnityEngine.Object.DestroyImmediate(oldBridge);
   foreach(var side in new[]{-1,1}){var oldRail=GameObject.Find("Bridge parapet "+side);if(oldRail!=null)UnityEngine.Object.DestroyImmediate(oldRail);}
   path=GameObject.Find("Road functional surface").GetComponent<Renderer>().sharedMaterial;
   stone=GameObject.Find("Stair tread 0").GetComponent<Renderer>().sharedMaterial;
   var bridgeStart=P(420,924,3.05f);var bridgeEnd=P(691,702,7.05f);
   Strip("Bridge repaired connected deck",bridgeStart,bridgeEnd,6,.25f,path);
   foreach(var side in new[]{-1,1})Strip("Bridge repaired parapet "+side,bridgeStart+Right*3*side,bridgeEnd+Right*3*side,.45f,.9f,stone);
   var ramp=GameObject.Find("Stair traversable ramp");if(ramp!=null)ramp.GetComponent<Renderer>().enabled=false;
   Capture("blockout-repaired-main",1536,1024);
   RemoveVisualPrefix("Lower gate west","Lower gate east","Lower gate lintel","Upper citadel west","Upper citadel east","Upper citadel lintel","Upper citadel keep","Cabin","Camp tent","West partial wall","East partial wall","Blockout forest mass","Blockout trunk");
   Fit(await Source("gate"),P(691,702,7),12);
   Fit(await Source("upper"),P(950,222,11),30);
   Fit(await Source("cabin"),P(451,398,7),6);
   Fit(await Source("camp"),P(1261,457,7),9);
   Fit(await Source("wall"),P(411,536,7),8);
   Fit(await Source("wall"),P(1361,590,7),10);
   // Reuse the authored bridge for its real arches; keep exact traversable deck.
   var bridgeArt=await Source("bridge");Fit(bridgeArt,(bridgeStart+bridgeEnd)*.5f-Vector3.up*3.3f,6);
   var grassTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/NatureStarterKit2/Textures/grass01.tga");
   var grassMat=Mat("Existing grass texture in isolated lookdev",new Color(.65f,.61f,.43f));grassMat.SetTexture("_BaseMap",grassTexture);
   var rockTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Holotna/Mountain/Textures/Rock01_ALB.png");
   var rockMat=Mat("Existing natural rock in isolated lookdev",new Color(.66f,.65f,.64f));rockMat.SetTexture("_BaseMap",rockTexture);
   foreach(var o in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)) {
    if(o.name.Contains("plateau")||o.name=="Upper terrace"||o.name=="Foreground bank"){o.sharedMaterials=new[]{grassMat,rockMat};WorldUV(o.gameObject,5);}
   }
   var pine=await Source("pine");
   var rng=new System.Random(741);int count=0;
   foreach(int side in new[]{0,1})for(int i=0;i<16;i++){
    float x=(side==0?95+i*17:1455+i*2)+(float)(rng.NextDouble()-.5)*42;float y=(side==0?535-i*25:695-i*27)+(float)(rng.NextDouble()-.5)*30;
    float h=4.5f+(float)rng.NextDouble()*3;
    var copy=UnityEngine.Object.Instantiate(pine);Fit(copy,P(x,y,side==0&&i>11?11:7),h*.45f);count++;
   }
   for(int i=0;i<13;i++){var copy=UnityEngine.Object.Instantiate(pine);Fit(copy,P(430+i*76,172+(i%3)*7,11),2.3f+(float)rng.NextDouble());count++;}
   UnityEngine.Object.DestroyImmediate(pine);
   // Rock scan groups are seated along the full continuous geological support, with varied footprints.
   var cliff=await Source("cliff");var platform=GameObject.Find("Main continuous buildable plateau").GetComponent<MeshFilter>().sharedMesh.vertices;
   for(int i=0;i<26;i++){var a=platform[i];var b=platform[(i+1)%26];float len=Vector3.Distance(a,b);int n=Mathf.Max(1,Mathf.CeilToInt(len/4));
    for(int j=0;j<n;j++){var p=Vector3.Lerp(a,b,(j+.5f)/n);var copy=UnityEngine.Object.Instantiate(cliff);Fit(copy,new Vector3(p.x,.2f,p.z),4.5f,6.8f+(float)rng.NextDouble()*.6f);}
   }UnityEngine.Object.DestroyImmediate(cliff);
   Physics.SyncTransforms();
   Capture("hybrid3d-main",1536,1024);Capture("hybrid3d-landscape",1280,720);Capture("hybrid3d-portrait",390,844);
   cam.transform.position+=Right*7;Capture("hybrid3d-pan",1536,1024);cam.transform.position-=Right*7;
   cam.orthographicSize=17;Capture("hybrid3d-zoom",1536,1024);cam.orthographicSize=24;
   long triangles=0;var meshes=new HashSet<Mesh>();var mats=new HashSet<Material>();foreach(var f in artRoot.GetComponentsInChildren<MeshFilter>()){triangles+=f.sharedMesh.triangles.Length/3;meshes.Add(f.sharedMesh);}foreach(var r in artRoot.GetComponentsInChildren<Renderer>())foreach(var m in r.sharedMaterials)mats.Add(m);importFacts.Add("instances_triangles="+triangles+" shared_meshes="+meshes.Count+" shared_materials="+mats.Count+" art_renderers="+artRoot.GetComponentsInChildren<Renderer>().Length);SaveArt();AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),Root+"/BastionI_Hybrid3D.unity");
   File.WriteAllLines(Evidence+"/hybrid-import-facts.txt",importFacts);
   File.WriteAllText(Evidence+"/hybrid-result.json","{\"stage\":\"ART_REVIEW_REQUIRED\",\"paid_credits\":0,\"art_approved\":false,\"gameplay_pass\":false,\"mobile_performance_pass\":false}");
   Debug.Log("BREAKTHROUGH_CAPTURE_COMPLETE");EditorApplication.Exit(0);
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
 }
}
}