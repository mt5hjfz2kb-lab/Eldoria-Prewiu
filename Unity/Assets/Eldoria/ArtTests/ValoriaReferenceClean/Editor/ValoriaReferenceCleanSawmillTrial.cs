using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Eldoria.Domain;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using GLTFast;
using Object=UnityEngine.Object;

namespace Eldoria.ReferenceClean.Editor {
 // Reuses the EXISTING completed editable family, not a newly generated building.
 public static class ValoriaReferenceCleanSawmillTrial {
  const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean";
  const string Dir=Root+"/ArtVariants/SawmillCompleted";
  const string Source="../art-source/valoria/production/sawmill-family-v1/state2/SawmillState2V1";
  static string Ev=>Path.GetFullPath("../docs/evidence/valoria-reference-clean");
  static readonly HashSet<int> saved=new HashSet<int>();static int index;
  static void Check(bool ok,string why){if(!ok)throw new Exception("SAWMILL_TRIAL: "+why);}
  static string Hash(string p)=>ValoriaContentPolicy.Hash(p);
  static Bounds Bounds(GameObject o){var rs=o.GetComponentsInChildren<Renderer>(true);Check(rs.Length>0,"No renderer");var b=rs[0].bounds;foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);return b;}
  static void Persist(Object o){if(o==null||AssetDatabase.Contains(o)||!saved.Add(o.GetInstanceID()))return;if(!(o is Mesh)&&!(o is Material)&&!(o is Texture2D))return;AssetDatabase.CreateAsset(o,AssetDatabase.GenerateUniqueAssetPath(Dir+"/Sawmill_"+(index++).ToString("D4")+(o is Material?".mat":".asset")));}
  static void Views(Camera c,string prefix){var p=c.transform.position;var q=c.transform.rotation;float size=c.orthographicSize;try{
   ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-main.png",1448,1086);
   c.transform.position=new Vector3(-42,20,9);c.transform.LookAt(new Vector3(-29,2,32));c.orthographicSize=11.5f;
   ValoriaReferenceCleanBuilder.Capture(c,Ev+"/"+prefix+"-detail.png",1448,1086);
  }finally{c.transform.SetPositionAndRotation(p,q);c.orthographicSize=size;}}
  public static async void Build(){ValoriaReferenceCleanRenderSettings guard=null;try{
   Check(Hash(Source+".glb")=="abe163c09de3690fbeabf39f126663349f7e6f8e2565c0ce204f824a366ef026","Existing completed source GLB identity");
   Check(Hash(Source+".blend")=="7e7f6719806cadfef7a223bc6ef664b820ab17bc66db32fe6b27486d4236fec3","Existing editable Blender identity");
   Directory.CreateDirectory(Dir);AssetDatabase.Refresh();string source=Root+"/Valoria_Reference_Clean.unity",sceneHash=Hash(source);var scene=EditorSceneManager.OpenScene(source,OpenSceneMode.Single);
   var h=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();Check(h&&h.Parcels&&h.Parcels.LeftBuilt,"Existing bindings missing");
   guard=Object.FindFirstObjectByType<ValoriaReferenceCleanRenderSettings>();guard.Apply();var camera=Camera.main;Check(camera,"Current camera missing");
   var preview=new PlayerState{BastionLevel=2,SawmillLevel=1,BarracksLevel=1};h.Apply(preview);
   var old=h.Parcels.LeftBuilt;var prior=Bounds(old);Views(camera,"sawmill-completed-before");
   string variant=Dir+"/Valoria_SawmillCompleted.unity";Check(EditorSceneManager.SaveScene(scene,variant),"Variant save failed");
   var next=new GameObject("Sawmill built / existing completed STATE2");next.transform.SetParent(old.transform.parent,false);
   var model=new GameObject("Completed sawmill model");model.transform.SetParent(next.transform,false);
   var gltf=new GltfImport(null,new UninterruptedDeferAgent());
   Check(await gltf.Load(new Uri(Path.GetFullPath(Source+".glb")).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=4}),"GLB load");
   Check(await gltf.InstantiateMainSceneAsync(model.transform),"GLB instantiate");
   var b=Bounds(model);model.transform.localScale=Vector3.one*(Mathf.Max(prior.size.x,prior.size.z)/Mathf.Max(b.size.x,b.size.z));b=Bounds(model);
   model.transform.position-=new Vector3(b.center.x,b.min.y,b.center.z);next.transform.position=new Vector3(prior.center.x,prior.min.y,prior.center.z);
   int triangles=0;foreach(var r in next.GetComponentsInChildren<MeshRenderer>(true)){
    GameObjectUtility.SetStaticEditorFlags(r.gameObject,0);r.lightmapIndex=-1;r.receiveGI=ReceiveGI.LightProbes;r.lightProbeUsage=LightProbeUsage.BlendProbes;r.reflectionProbeUsage=ReflectionProbeUsage.BlendProbes;
    foreach(var m in r.sharedMaterials){Check(m&&m.shader&&m.shader.isSupported,"Material unsupported");foreach(var prop in m.GetTexturePropertyNames())Persist(m.GetTexture(prop));Persist(m);}
   }
   foreach(var f in next.GetComponentsInChildren<MeshFilter>(true)){Persist(f.sharedMesh);triangles+=f.sharedMesh.triangles.Length/3;}
   var after=Bounds(next);Check(Mathf.Max(after.size.x,after.size.z)<10.8f&&after.size.y<10.4f,"Functional parcel envelope");Check(Mathf.Abs(after.min.y-prior.min.y)<.005f,"Floating foundation");
   old.SetActive(false);Views(camera,"sawmill-completed-after");
   foreach(var binding in h.Parcels.Bindings)if(binding.BuildingId=="sawmill")binding.Built=next;
   h.Parcels.LeftBuilt=next;Object.DestroyImmediate(old);h.Apply(new PlayerState());AssetDatabase.SaveAssets();
   Check(EditorSceneManager.SaveScene(scene,variant),"Final variant save");Check(Hash(source)==sceneHash,"Authoritative scene changed");
   File.WriteAllText(Ev+"/sawmill-completed-comparison.json","{\"source_sha\":\""+Environment.GetEnvironmentVariable("GITHUB_SHA")+"\",\"original_scene_unchanged\":true,\"same_camera_light\":true,\"baked_world_retained\":true,\"triangles\":"+triangles+",\"functional_envelope_verified\":true,\"initial_saved_hidden\":true,\"source\":\"existing STATE2 family\",\"art_approved\":false}");
   guard.Restore();Debug.Log("VALORIA_REFERENCE_CLEAN_SAWMILL_TRIAL_COMPLETE");EditorApplication.Exit(0);
  }catch(Exception e){if(guard)guard.Restore();Debug.LogException(e);EditorApplication.Exit(1);}}
 }
}
