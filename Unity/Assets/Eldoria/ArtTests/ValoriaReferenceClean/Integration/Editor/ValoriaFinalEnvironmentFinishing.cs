#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Eldoria.Presentation;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
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
   CityViews(city,"before-city");
  }
  static void CityViews(ValoriaReferenceCleanGameplay city,string prefix){
   var targets=new[]{city.BastionOne,city.BastionTwo,city.Parcels.LeftBuilt,city.Parcels.RightBuilt};var states=targets.Select(t=>t.activeSelf).ToArray();
   try{Views(city.PlayerCamera,prefix+"-initial");city.BastionOne.SetActive(false);city.BastionTwo.SetActive(true);city.Parcels.LeftBuilt.SetActive(true);city.Parcels.RightBuilt.SetActive(true);Views(city.PlayerCamera,prefix+"-built");}
   finally{for(int i=0;i<targets.Length;i++)targets[i].SetActive(states[i]);}
  }
  static Mesh SeatWoodlandRocks(MeshFilter filter,MeshFilter trees,Terrain terrain){
   var original=filter.sharedMesh;var v=original.vertices;var triangles=original.triangles;var parent=Enumerable.Range(0,v.Length).ToArray();var keys=new Dictionary<Vector3Int,int>();
   int Root(int i){while(parent[i]!=i){parent[i]=parent[parent[i]];i=parent[i];}return i;}
   void Join(int a,int b){parent[Root(a)]=Root(b);}
   for(int i=0;i<v.Length;i++){var key=new Vector3Int(Mathf.RoundToInt(v[i].x*10000),Mathf.RoundToInt(v[i].y*10000),Mathf.RoundToInt(v[i].z*10000));if(keys.TryGetValue(key,out int prior))Join(i,prior);else keys.Add(key,i);}
   for(int i=0;i<triangles.Length;i+=3){Join(triangles[i],triangles[i+1]);Join(triangles[i],triangles[i+2]);}
   float Ground(Vector3 p)=>terrain.SampleHeight(p)+terrain.transform.position.y;
   var anchors=new Dictionary<Vector2Int,Vector3>();foreach(var local in trees.sharedMesh.vertices){var p=trees.transform.TransformPoint(local);float delta=p.y-Ground(p);if(delta<-.15f||delta>.38f)continue;var key=new Vector2Int(Mathf.RoundToInt(p.x/2),Mathf.RoundToInt(p.z/2));if(!anchors.ContainsKey(key))anchors.Add(key,p);}
   Need(anchors.Count>10,"Tree-ground anchors missing");int moved=0;
   foreach(var component in Enumerable.Range(0,v.Length).GroupBy(Root)){
    var indices=component.ToArray();var b=new Bounds(filter.transform.TransformPoint(v[indices[0]]),Vector3.zero);foreach(int ix in indices)b.Encapsulate(filter.transform.TransformPoint(v[ix]));if(b.size.x>3.2f||b.size.z>3.2f)continue;var c=b.center;var anchor=anchors.Values.OrderBy(p=>(new Vector2(p.x-c.x,p.z-c.z)).sqrMagnitude).First();float distance=Vector2.Distance(new Vector2(c.x,c.z),new Vector2(anchor.x,anchor.z));
    if(distance<2||distance>18)continue;var dest=Vector3.Lerp(c,anchor,.80f);var shift=dest-c;shift.y=Ground(dest)-Ground(c)-Mathf.Min(.12f,b.size.y*.18f);foreach(int ix in indices)v[ix]=filter.transform.InverseTransformPoint(filter.transform.TransformPoint(v[ix])+shift);moved++;
   }
   Need(moved>5,"Woodland stone grouping produced no visible intervention");var next=Object.Instantiate(original);next.name="Woodland rocks / rigid natural tree-foot clusters";next.vertices=v;next.RecalculateBounds();File.WriteAllText(Ev+"/rock-grouping.json","{\"moved_components\":"+moved+",\"topology_unchanged\":true,\"collision_unchanged\":true,\"plot_footprints_created\":false}");return next;
  }
  static void Need(bool test,string reason){if(!test)throw new InvalidOperationException(reason);}
  static Mesh Make(ValoriaWorldArtFinishing.TreeMesh data){var mesh=new Mesh{name=data.name};mesh.vertices=data.vertices;mesh.normals=data.normals;mesh.uv=data.uv;mesh.subMeshCount=data.submeshes.Length;for(int i=0;i<data.submeshes.Length;i++)mesh.SetTriangles(data.submeshes[i].indices,i);mesh.RecalculateBounds();return mesh;}
  static void ThreatViews(ValoriaIntegratedWorld host,string prefix){
   var camera=host.WorldCamera;var pos=camera.transform.position;float zoom=camera.orthographicSize;bool active=host.WorldRoot.activeSelf;host.WorldRoot.SetActive(true);
   try{foreach(string id in new[]{"corrupt-scout","engendro-valoria"}){var p=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true).Single(n=>n.Id==id).transform.position;p.y=2;camera.transform.position=p-camera.transform.forward*110;foreach(float size in new[]{32f,14f}){camera.orthographicSize=size;ValoriaReferenceCleanBuilder.Capture(camera,Ev+"/"+prefix+"-"+id+"-"+size.ToString("0")+".png",1448,1086);}camera.orthographicSize=14;ValoriaReferenceCleanBuilder.Capture(camera,Ev+"/"+prefix+"-"+id+"-portrait.png",900,1600);}}
   finally{camera.transform.position=pos;camera.orthographicSize=zoom;host.WorldRoot.SetActive(active);}
  }
  [Serializable] public class Acceptance {public bool accepted;public string source_scene_sha256,candidate_sha256;public string[] image_names,image_hashes;}
  public static void Trial(){Run(false);}
  public static void Integrate(){Run(true);}
  static void Run(bool integrate){try{
   EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);Directory.CreateDirectory(Ev);var source=JsonUtility.FromJson<Data>(File.ReadAllText(Ev+"/survey.json"));Need(source.scene_sha256==Hash(Scene),"Finishing source scene changed");var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();bool saved=false;var approvedImages=new Dictionary<string,byte[]>();if(integrate){var a=JsonUtility.FromJson<Acceptance>(File.ReadAllText(Ev+"/accepted-candidate.json"));Need(a.accepted&&a.source_scene_sha256==Hash(Scene)&&a.candidate_sha256==Hash(Ev+"/breach-ground.json"),"Exact reviewed finishing sources required");Need(a.image_names.Length==10&&a.image_hashes.Length==10,"Ten matching city/threat views required");for(int i=0;i<a.image_names.Length;i++){string n=a.image_names[i];Need(Hash(Ev+"/"+n)==a.image_hashes[i],"Reviewed image changed");approvedImages.Add(n,File.ReadAllBytes(Ev+"/"+n));}}
   var temporary=new List<Object>();var changedMeshes=new Dictionary<MeshFilter,Mesh>();var replacedTrees=new List<Renderer>();var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();
   try{
    ThreatViews(host,"before");CityViews(city,"before-city");Need(!host.WorldRoot.GetComponentsInChildren<Transform>(true).Any(t=>t.gameObject.activeSelf&&(t.name.StartsWith("Zone1 hostile camp")||t.name.StartsWith("Enemy camp"))&&t.GetComponentsInChildren<Renderer>().Any(r=>r.enabled)),"Unauthorised camp unexpectedly active");
    var data=JsonUtility.FromJson<ValoriaWorldArtFinishing.Family>(File.ReadAllText(Ev+"/breach-ground.json"));Need(data.variants.Length==4,"Two existing threats and two functional contact aprons required");
    var rockMap=new Texture2D(2,2,TextureFormat.RGBA32,true,false){name="Retained rock3 Breach material",wrapMode=TextureWrapMode.Repeat,anisoLevel=4};Need(rockMap.LoadImage(File.ReadAllBytes("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources/MaterialInputs/rock_3/rock_3_diff_1k.png")),"Retained rock texture missing");temporary.Add(rockMap);
    var materials=data.colors.Select((c,i)=>{var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name="Breach altered matter "+i};m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",.08f);m.SetFloat("_Metallic",0);m.SetFloat("_Cull",0);if(i<3)m.SetTexture("_BaseMap",rockMap);if(i==3||i==4){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*.65f);}if(i>=5){var t=Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None).Single();m.SetTexture("_BaseMap",t.terrainData.terrainLayers[2].diffuseTexture);m.SetColor("_BaseColor",Color.white);}return m;}).ToArray();temporary.AddRange(materials);
    foreach(var model in data.variants){bool functional=model.name=="sawmill"||model.name=="barracks";Need(functional||model.name=="corrupt-scout"||model.name=="engendro-valoria","Unknown destination");if(!functional)Need(!host.WorldRoot.GetComponentsInChildren<Transform>(true).Any(t=>t.name=="Zone1 authored "+model.name),"Existing threat art must be retained/reviewed");
     var go=new GameObject(functional?model.name+" / irregular workground contact":"Zone1 authored "+model.name);var parent=functional?(model.name=="sawmill"?city.Parcels.LeftBuilt.transform:city.Parcels.RightBuilt.transform):host.WorldRoot.transform;go.transform.SetParent(parent,true);temporary.Add(go);var mesh=Make(model);
     if(functional){var t=Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None).Single();var vv=mesh.vertices;float baseline=model.name=="sawmill"?source.sawmill_min.y:source.barracks_min.y;for(int i=0;i<vv.Length;i++){float offset=vv[i].y-baseline;vv[i].y=t.SampleHeight(vv[i])+t.transform.position.y+offset;}mesh.vertices=vv;mesh.RecalculateNormals();mesh.RecalculateBounds();}
     temporary.Add(mesh);go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterials=materials;r.shadowCastingMode=functional?ShadowCastingMode.Off:ShadowCastingMode.On;
    }
    var natural=Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f=>f.gameObject.activeInHierarchy&&!f.transform.IsChildOf(host.WorldRoot.transform)).ToArray();var rock=natural.Single(f=>f.name=="Woodland_ground_rocks");var tree=natural.Single(f=>f.name=="Fir_tree_group");var terrain=Object.FindObjectsByType<Terrain>(FindObjectsSortMode.None).Single();changedMeshes.Add(rock,rock.sharedMesh);var grouped=SeatWoodlandRocks(rock,tree,terrain);temporary.Add(grouped);rock.sharedMesh=grouped;
    // Reuse the three exact reviewed world meshes only for the 18 earlier
    // lobed conifer clones. Preserve the better original Fir_tree_group.
    var oldTrees=Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.gameObject.activeInHierarchy&&r.enabled&&!r.transform.IsChildOf(host.WorldRoot.transform)&&r.name.StartsWith("Forest retained Valoria pine")).OrderBy(r=>PathOf(r.transform),StringComparer.Ordinal).ToArray();Need(oldTrees.Length==18,"Measured city conifer candidates changed");
    var worldTrees=host.WorldRoot.GetComponentsInChildren<Transform>(true);for(int i=0;i<oldTrees.Length;i++){
     var old=oldTrees[i];int variant=i%3;var sourceTree=worldTrees.Single(t=>t.name=="Conifer / retained tree position "+variant);var near=sourceTree.Find("Near branches").GetComponent<MeshFilter>();var far=sourceTree.Find("Strategic silhouette").GetComponent<MeshFilter>();var mats=near.GetComponent<MeshRenderer>().sharedMaterials;var b=old.bounds;float scale=b.size.y/near.sharedMesh.bounds.size.y;var pos=b.center;pos.y=terrain.SampleHeight(pos)+terrain.transform.position.y-near.sharedMesh.bounds.min.y*scale-.055f;
     var g=new GameObject("Reused reviewed conifer / city "+i);g.transform.position=pos;g.transform.localScale=Vector3.one*scale;g.transform.rotation=Quaternion.Euler(0,i*137.508f,0);g.transform.SetParent(old.transform,true);temporary.Add(g);
     Renderer Child(string name,Mesh mesh){var c=new GameObject(name);c.transform.SetParent(g.transform,false);c.AddComponent<MeshFilter>().sharedMesh=mesh;var r=c.AddComponent<MeshRenderer>();r.sharedMaterials=mats;return r;}
     var hi=Child("Near branches",near.sharedMesh);var low=Child("Strategic silhouette",far.sharedMesh);var lod=g.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.16f,new[]{hi}),new LOD(.012f,new[]{low})});lod.RecalculateBounds();old.enabled=false;replacedTrees.Add(old);
    }
    ThreatViews(host,"after");CityViews(city,"after-city");Need(source.scene_sha256==Hash(Scene),"Trial unexpectedly saved");if(integrate){
     var differences=new List<string>();foreach(var im in approvedImages){differences.Add(ValoriaTerrainBankAuthoring.CompareRender(im.Value,Ev+"/"+im.Key));File.WriteAllBytes(Ev+"/approved-"+im.Key,im.Value);}File.WriteAllText(Ev+"/render-identity.json","["+string.Join(",",differences)+"]");
     const string folder="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/FinalEnvironment212";Need(!Directory.Exists(folder),"Refuse overwrite existing final environment assets");Directory.CreateDirectory(folder);AssetDatabase.Refresh();int ix=0;foreach(var o in temporary.OfType<Texture2D>())AssetDatabase.CreateAsset(o,folder+"/texture-"+(ix++)+".asset");foreach(var o in temporary.OfType<Material>())AssetDatabase.CreateAsset(o,folder+"/material-"+(ix++)+".mat");foreach(var o in temporary.OfType<Mesh>())AssetDatabase.CreateAsset(o,folder+"/mesh-"+(ix++)+".asset");AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(host.gameObject.scene);Need(EditorSceneManager.SaveScene(host.gameObject.scene,Scene),"Final environment save failed");saved=true;
     File.WriteAllText(Ev+"/integrated.json","{\"scene_sha256\":\""+Hash(Scene)+"\",\"gameplay_gate\":\"PENDING_CURRENT_PROBE\",\"reused_city_trees\":18,\"new_enemy_camps\":false,\"published\":false}");
    }
    if(!integrate)File.WriteAllText(Ev+"/unity-review.json","{\"same_camera_light\":true,\"scene_saved\":false,\"art_approved\":false,\"hotspots_preserved\":true,\"no_camps\":true}");
   }finally{if(!saved){foreach(var r in replacedTrees)r.enabled=true;foreach(var kv in changedMeshes)kv.Key.sharedMesh=kv.Value;foreach(var o in temporary)if(o)Object.DestroyImmediate(o);}}
   if(integrate)ValoriaIntegrationBuild.ValidateCurrent();else{Debug.Log("VALORIA_REFERENCE_CLEAN_FINAL_ENVIRONMENT_TRIAL_COMPLETE");EditorApplication.Exit(0);}
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
  static void Views(Camera camera,string prefix){ValoriaReferenceCleanBuilder.Capture(camera,Ev+"/"+prefix+"-landscape.png",1448,1086);ValoriaReferenceCleanBuilder.Capture(camera,Ev+"/"+prefix+"-portrait.png",900,1600);}
 }
}
#endif
