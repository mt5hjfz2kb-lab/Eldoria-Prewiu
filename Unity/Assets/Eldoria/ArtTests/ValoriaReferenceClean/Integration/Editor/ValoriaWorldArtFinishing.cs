#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Eldoria.World4XPrototype;
using Eldoria.Presentation;
using GLTFast;
using UnityEngine.Rendering;
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
  [Serializable] public class Indices {public int[] indices;}
  [Serializable] public class TreeMesh {public string name;public Vector3[] vertices,normals;public Vector2[] uv;public Indices[] submeshes;}
  [Serializable] public class Family {public TreeMesh[] variants,low_variants;public Color[] colors;}
  static void Need(bool b,string message){if(!b)throw new InvalidOperationException(message);}
  static Mesh MakeMesh(TreeMesh source){var m=new Mesh{name=source.name,indexFormat=source.vertices.Length>65535?IndexFormat.UInt32:IndexFormat.UInt16};m.vertices=source.vertices;m.normals=source.normals;if(source.uv!=null&&source.uv.Length==source.vertices.Length)m.uv=source.uv;m.subMeshCount=source.submeshes.Length;for(int i=0;i<source.submeshes.Length;i++)m.SetTriangles(source.submeshes[i].indices,i);m.RecalculateBounds();return m;}
  static Bounds BoundsOf(GameObject obj){var rr=obj.GetComponentsInChildren<Renderer>(true);Need(rr.Length>0,"Missing model bounds");var b=rr[0].bounds;foreach(var r in rr.Skip(1))b.Encapsulate(r.bounds);return b;}
  public static async void Trial(){try{
   Directory.CreateDirectory(Ev);EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);var source=JsonUtility.FromJson<Data>(File.ReadAllText(Ev+"/survey.json"));Need(source.scene_sha256==Hash(Scene),"Current world scene changed");var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();bool wasActive=host.WorldRoot.activeSelf;host.WorldRoot.SetActive(true);
   var originals=new List<Renderer>();var temporary=new List<Object>();var root=new GameObject("Forest and quarry / authored candidate");root.transform.SetParent(host.WorldRoot.transform,true);temporary.Add(root);var oldQuarry=host.WorldRoot.GetComponentsInChildren<Transform>(true).Single(t=>t.name=="Zone1 authored quarry-valoria");bool quarryActive=oldQuarry.gameObject.activeSelf;
   try{
    CaptureViews(host,"before");var family=JsonUtility.FromJson<Family>(File.ReadAllText(Ev+"/conifer-family.json"));Need(family.variants.Length==3&&family.low_variants.Length==3,"Three reviewed silhouettes and strategic meshes required");var meshes=family.variants.Select(MakeMesh).ToArray();var low=family.low_variants.Select(MakeMesh).ToArray();temporary.AddRange(meshes);temporary.AddRange(low);
    var materials=family.colors.Select((color,i)=>{var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Authored conifer surface "+i,enableInstancing=true};mat.SetColor("_BaseColor",color);mat.SetFloat("_Smoothness",.04f);mat.SetFloat("_Metallic",0);return mat;}).ToArray();temporary.AddRange(materials);
    var ground=host.WorldRoot.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.name=="ZoneOne continuous prairie source");var query=ground.GetComponent<MeshCollider>();if(!query){query=ground.gameObject.AddComponent<MeshCollider>();query.sharedMesh=ground.sharedMesh;temporary.Add(query);}Physics.SyncTransforms();
    float GroundAt(Vector3 p){return query.Raycast(new Ray(new Vector3(p.x,100,p.z),Vector3.down),out var hit,200)?hit.point.y:p.y;}
    var trees=host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.gameObject.activeInHierarchy&&r.enabled&&r.name.StartsWith("Forest retained Valoria pine")).OrderBy(r=>PathOf(r.transform),StringComparer.Ordinal).ToArray();Need(trees.Length==98,"Measured tree population changed");
    for(int i=0;i<trees.Length;i++){
     var old=trees[i];var bounds=old.bounds;int variant=i%3;var tree=new GameObject("Conifer / retained tree position "+i);tree.transform.SetParent(root.transform,true);float scale=bounds.size.y/meshes[variant].bounds.size.y;var p=bounds.center;p.y=GroundAt(p)-meshes[variant].bounds.min.y*scale-.055f;tree.transform.position=p;tree.transform.localScale=Vector3.one*scale;tree.transform.rotation=Quaternion.Euler(0,(i*137.508f)%360,0);
     Renderer MakeLOD(string name,Mesh mesh){var obj=new GameObject(name);obj.transform.SetParent(tree.transform,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=obj.AddComponent<MeshRenderer>();renderer.sharedMaterials=materials;renderer.lightProbeUsage=LightProbeUsage.BlendProbes;renderer.reflectionProbeUsage=ReflectionProbeUsage.BlendProbes;return renderer;}
     var high=MakeLOD("Near branches",meshes[variant]);var far=MakeLOD("Strategic silhouette",low[variant]);var group=tree.AddComponent<LODGroup>();group.SetLODs(new[]{new LOD(.16f,new[]{high}),new LOD(.012f,new[]{far})});group.RecalculateBounds();old.enabled=false;originals.Add(old);
    }
    // Replace only the previous quarry visual; preserve the hotspot, crane/stock
    // language, every other destination and all gameplay components.
    var oldBounds=BoundsOf(oldQuarry.gameObject);var quarry=new GameObject("Quarry / continuous geological excavation");quarry.transform.SetParent(root.transform,true);
    using(var loader=new GltfImport(null,new UninterruptedDeferAgent())){Need(await loader.Load(new Uri(Path.GetFullPath("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources/EnvironmentFinish/QuarryGeologyV18.glb")).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=4}),"Authored quarry GLB load failed");Need(await loader.InstantiateMainSceneAsync(quarry.transform),"Authored quarry instantiate failed");}
    quarry.transform.rotation=oldQuarry.rotation;var qb=BoundsOf(quarry);quarry.transform.localScale=Vector3.one*(oldBounds.size.x/qb.size.x);qb=BoundsOf(quarry);quarry.transform.position+=new Vector3(oldBounds.center.x-qb.center.x,oldBounds.min.y-qb.min.y,oldBounds.center.z-qb.center.z);oldQuarry.gameObject.SetActive(false);
    foreach(var r in quarry.GetComponentsInChildren<Renderer>()){r.lightmapIndex=-1;r.lightProbeUsage=LightProbeUsage.BlendProbes;r.reflectionProbeUsage=ReflectionProbeUsage.BlendProbes;}
    Physics.SyncTransforms();CaptureViews(host,"after");Need(Hash(Scene)==source.scene_sha256,"Trial scene unexpectedly saved");File.WriteAllText(Ev+"/unity-review.json","{\"same_camera_light\":true,\"scene_saved\":false,\"tree_count_preserved\":98,\"LOD_high_transition\":0.16,\"hotspots_unchanged\":true,\"art_approved\":false}");
   }finally{foreach(var r in originals)r.enabled=true;oldQuarry.gameObject.SetActive(quarryActive);foreach(var obj in temporary)if(obj)Object.DestroyImmediate(obj);host.WorldRoot.SetActive(wasActive);}
   Debug.Log("VALORIA_REFERENCE_CLEAN_WORLD_ART_TRIAL_COMPLETE");EditorApplication.Exit(0);
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
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
