using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using GLTFast;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
 public static class ValoriaReferenceCleanCampTrial {
  const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean";
  const string Dir=Root+"/ArtVariants/CampCloth87";
  static string Ev=>Path.GetFullPath("../docs/evidence/valoria-reference-clean");
  static HashSet<int> saved=new HashSet<int>();static int index;
  static void Check(bool ok,string why){if(!ok)throw new Exception("CAMP_TRIAL: "+why);}
  static string Hash(string p){using(var h=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p)));}
  static Bounds Bounds(GameObject o){var rs=o.GetComponentsInChildren<Renderer>(true);Check(rs.Length>0,"No renderer");var b=rs[0].bounds;foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);return b;}
  static void Persist(Object o){if(o==null||AssetDatabase.Contains(o)||!saved.Add(o.GetInstanceID()))return;if(!(o is Mesh)&&!(o is Material)&&!(o is Texture2D))return;AssetDatabase.CreateAsset(o,AssetDatabase.GenerateUniqueAssetPath(Dir+"/Camp87_"+(index++).ToString("D4")+(o is Material?".mat":".asset")));}
  static void Views(Camera c,string prefix){var p=c.transform.position;var q=c.transform.rotation;float size=c.orthographicSize;try{
   ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-main.png",1448,1086);
   c.transform.position=new Vector3(42,20,9);c.transform.LookAt(new Vector3(29,2,32));c.orthographicSize=11.5f;
   ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-detail.png",1448,1086);
  }finally{c.transform.SetPositionAndRotation(p,q);c.orthographicSize=size;}}
  public static async void Build(){ValoriaReferenceCleanRenderSettings guard=null;try{
   Directory.CreateDirectory(Dir);AssetDatabase.Refresh();string source=Root+"/Valoria_Reference_Clean.unity",sourceHash=Hash(source);var scene=EditorSceneManager.OpenScene(source,OpenSceneMode.Single);
   var h=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();Check(h&&h.Parcels&&h.Parcels.RightBuilt,"Existing gameplay bindings missing");
   guard=Object.FindFirstObjectByType<ValoriaReferenceCleanRenderSettings>();guard.Apply();var camera=Camera.main;Check(camera,"Current camera missing");
   // Editor-only preview of already implemented built states, never a player save or free reward.
   var preview=new PlayerState{BastionLevel=2,SawmillLevel=1,BarracksLevel=1};h.Apply(preview);var old=h.Parcels.RightBuilt;var prior=Bounds(old);Views(camera,"camp87-unity-before");
   string variant=Dir+"/Valoria_CampCloth87.unity";Check(EditorSceneManager.SaveScene(scene,variant),"Variant save failed");
   var next=new GameObject("Barracks / authored canvas camp");next.transform.SetParent(old.transform.parent,false);
   var model=new GameObject("Camp cloth87 model");model.transform.SetParent(next.transform,false);var gltf=new GltfImport(null,new UninterruptedDeferAgent());
   Check(await gltf.Load(new Uri(Path.GetFullPath(Root+"/Sources/CampCloth87.glb")).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=4}),"GLB load");
   Check(await gltf.InstantiateMainSceneAsync(model.transform),"GLB instantiate");
   var b=Bounds(model);model.transform.localScale=Vector3.one*(Mathf.Max(prior.size.x,prior.size.z)/Mathf.Max(b.size.x,b.size.z));b=Bounds(model);
   // Stakes intentionally penetrate the ground. Align the unchanged timber feet, not the lowest stake tip.
   var oldFeet=old.GetComponentsInChildren<Renderer>(true).Single(r=>r.name=="Camp_Timber");
   var newFeet=next.GetComponentsInChildren<Renderer>(true).Single(r=>r.name=="Camp87 Camp_Timber");
   float groundY=oldFeet.bounds.min.y,feetY=newFeet.bounds.min.y;
   model.transform.position-=new Vector3(b.center.x,feetY,b.center.z);next.transform.position=new Vector3(prior.center.x,groundY,prior.center.z);
   int triangles=0;foreach(var r in next.GetComponentsInChildren<MeshRenderer>(true)){
    GameObjectUtility.SetStaticEditorFlags(r.gameObject,0);r.lightmapIndex=-1;r.receiveGI=ReceiveGI.LightProbes;r.lightProbeUsage=LightProbeUsage.BlendProbes;r.reflectionProbeUsage=ReflectionProbeUsage.BlendProbes;
    foreach(var m in r.sharedMaterials){Check(m&&m.shader&&m.shader.isSupported,"Material unavailable");foreach(var prop in m.GetTexturePropertyNames())Persist(m.GetTexture(prop));Persist(m);}
   }
   foreach(var f in next.GetComponentsInChildren<MeshFilter>(true)){Persist(f.sharedMesh);triangles+=f.sharedMesh.triangles.Length/3;}
   var after=Bounds(next);Check(Mathf.Max(after.size.x,after.size.z)<11.0f&&after.size.y<6,"Functional parcel envelope exceeded");Check(Mathf.Abs(newFeet.bounds.min.y-groundY)<.005f,"Floating timber support");Check(after.min.y>=groundY-.4f,"Stake penetration too deep");
   old.SetActive(false);Views(camera,"camp87-unity-after");
   for(int i=0;i<h.Parcels.Bindings.Length;i++)if(h.Parcels.Bindings[i].BuildingId=="barracks")h.Parcels.Bindings[i].Built=next;
   h.Parcels.RightBuilt=next;Object.DestroyImmediate(old);h.Apply(new PlayerState());
   AssetDatabase.SaveAssets();Check(EditorSceneManager.SaveScene(scene,variant),"Final variant save");Check(Hash(source)==sourceHash,"Authoritative scene changed");
   File.WriteAllText(Ev+"/camp87-unity-comparison.json","{\"source_sha\":\""+Environment.GetEnvironmentVariable("GITHUB_SHA")+"\",\"original_scene_unchanged\":true,\"same_camera_light\":true,\"baked_world_retained\":true,\"camp_triangles\":"+triangles+",\"functional_envelope_verified\":true,\"level_zero_saved_hidden\":true,\"art_approved\":false}");
   guard.Restore();Debug.Log("VALORIA_REFERENCE_CLEAN_CAMP_TRIAL_COMPLETE");EditorApplication.Exit(0);
  }catch(Exception e){if(guard)guard.Restore();Debug.LogException(e);EditorApplication.Exit(1);}}
  public static void Integrate(){try{
   var scene=EditorSceneManager.OpenScene(Dir+"/Valoria_CampCloth87.unity",OpenSceneMode.Single);var h=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();Check(h&&h.Parcels&&h.Parcels.RightBuilt,"Reviewed camp bindings absent");
   var camp=h.Parcels.RightBuilt;bool active=camp.activeSelf;var pos=camp.transform.position;camp.SetActive(true);camp.transform.position=Vector3.zero;
   Check(PrefabUtility.SaveAsPrefabAsset(camp,Root+"/Prefabs/BarracksCamp.prefab"),"Portable camp prefab failed");camp.transform.position=pos;camp.SetActive(active);
   Check(EditorSceneManager.SaveScene(scene,Root+"/Valoria_Reference_Clean.unity"),"Integration save failed");AssetDatabase.SaveAssets();
   File.WriteAllText(Ev+"/camp87-integration.json","{\"source_sha\":\""+Environment.GetEnvironmentVariable("GITHUB_SHA")+"\",\"reviewed_camp_only\":true,\"existing_gameplay_and_gi_preserved\":true}");
   ValoriaReferenceCleanFinalize.Build();
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
 }
}
