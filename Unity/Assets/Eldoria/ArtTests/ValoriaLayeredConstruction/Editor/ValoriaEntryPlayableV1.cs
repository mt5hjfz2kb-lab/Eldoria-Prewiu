using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using GLTFast;
using Eldoria.Presentation;
using Eldoria.Domain;
namespace Eldoria.ValoriaLayered.Editor {
 public static class ValoriaEntryPlayableV1 {
  const string Root="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/";
  const string ScenePath=Root+"Valoria_SHARP_Entry_PlayableV1.unity";
  const string AssetsPath=Root+"GeneratedPlayableEntryV1";
  static readonly string Ev=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..","docs/evidence/valoria-layered-construction-20261016/playable-entry-v1"));
  static readonly HashSet<int> saved=new HashSet<int>();static int index;
  static void Req(bool ok,string why){if(!ok)throw new Exception("PLAYABLE_ENTRY: "+why);}
  static void Persist(UnityEngine.Object o){if(o==null||AssetDatabase.Contains(o)||!saved.Add(o.GetInstanceID()))return;if(!(o is Mesh)&&!(o is Material)&&!(o is Texture2D))return;AssetDatabase.CreateAsset(o,AssetsPath+"/Playable_"+(index++).ToString("D5")+(o is Material?".mat":".asset"));}
  static Bounds Bounds(GameObject go){var rs=go.GetComponentsInChildren<Renderer>(true);Req(rs.Length>0,"No model geometry");var b=rs[0].bounds;foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);return b;}
  static async Task<GameObject> Model(string path,Transform parent,string name,Collider target,float width){
   var g=new GltfImport(null,new UninterruptedDeferAgent());Req(await g.Load(new Uri(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..",path))).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=4}),"Import "+path);
   var o=new GameObject(name);o.transform.SetParent(parent,false);Req(await g.InstantiateMainSceneAsync(o.transform),"Instantiate "+path);
   var b=Bounds(o);float scale=width/Mathf.Max(b.size.x,b.size.z);o.transform.localScale=Vector3.one*scale;b=Bounds(o);
   Vector3 contact=target.bounds.center;float y=target.bounds.min.y;
   var hits=Physics.RaycastAll(new Vector3(contact.x,20,contact.z),Vector3.down,50,~0,QueryTriggerInteraction.Ignore);
   var ground=hits.Where(h=>h.collider is MeshCollider&&h.collider.name.Contains("ValoriaEntryTerrain")).OrderByDescending(h=>h.point.y).FirstOrDefault();if(ground.collider!=null)y=ground.point.y;
   o.transform.position+=new Vector3(contact.x-b.center.x,y-b.min.y,contact.z-b.center.z);
   foreach(var r in o.GetComponentsInChildren<Renderer>(true))foreach(var m in r.sharedMaterials){if(m==null)continue;foreach(var p in m.GetTexturePropertyNames())Persist(m.GetTexture(p));Persist(m);}foreach(var f in o.GetComponentsInChildren<MeshFilter>(true))Persist(f.sharedMesh);
   return o;
  }
  static GameObject Empty(Transform parent,string name){var o=new GameObject(name);o.transform.SetParent(parent,false);return o;}
  static Collider Hotspot(Transform[] ts,string source,string name,string id){var t=ts.Single(x=>x.name==source);var c=t.GetComponent<Collider>();Req(c!=null,"Missing real source contact "+source);t.name=name;var h=t.GetComponent<WorldHotspot>()??t.gameObject.AddComponent<WorldHotspot>();h.Id=id;return c;}
  [MenuItem("Eldoria/Valoria/Build isolated existing-system playable entry")]
  public static async void Build(){try{await Run();Debug.Log("VALORIA_ENTRY_PLAYABLE_SCENE_COMPLETE");EditorApplication.Exit(0);}catch(Exception e){Debug.LogError(e);EditorApplication.Exit(1);}}
  static async Task Run(){
   Directory.CreateDirectory(Ev);Directory.CreateDirectory(AssetsPath);AssetDatabase.Refresh();
   string source=Root+"Valoria_SHARP_EntryZoneV4_CANDIDATE.unity";Req(File.Exists(source),"V4 source missing");
   var scene=EditorSceneManager.OpenScene(source,OpenSceneMode.Single);
   var ts=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None);
   var frame=ts.Single(t=>t.name.StartsWith("EXPERIMENTAL / upright native-metre"));
   var visual=UnityEngine.Object.FindFirstObjectByType<ValoriaParcelPresentation>();Req(visual!=null,"Canonical parcel presenter missing");
   var left=Hotspot(ts,"SUPPORT/RESERVE VPD · L0 west plot","InteractiveProxy_LeftCabinParcel","sawmill");
   var right=Hotspot(ts,"SUPPORT/RESERVE VPD · L0 east plot","InteractiveProxy_RightCampParcel","barracks");
   Hotspot(ts,"GREYBOX Bastion hero reservation, silhouette only","InteractiveProxy_Bastion","bastion");
   Hotspot(ts,"SUPPORT/RESERVE Puerta · ir al mundo","Experimental original gate contact","gate");
   var states=Empty(frame,"EXPERIMENTAL / existing parcel progression visuals");
   Physics.SyncTransforms();
   var sawmill=await Model("art-source/valoria/production/sawmill-family-v1/state1/SawmillState1V1.glb",states.transform,"Existing authored sawmill / BUILT",left,5.1f);
   var works=await Model("art-source/valoria/production/sawmill-family-v1/state0/SawmillState0V1.glb",states.transform,"Existing authored sawmill / UNDER_CONSTRUCTION",left,5.1f);
   var camp=await Model("art-source/valoria/production/camp-family-v1/CampFamilyV1.glb",states.transform,"Existing authored camp / BUILT barracks state",right,6.5f);
   visual.LeftBuilt=sawmill;visual.RightBuilt=camp;visual.LeftTarget=left;visual.RightTarget=right;
   visual.LeftGround=Empty(states.transform,"Left empty parcel ground state");visual.RightGround=Empty(states.transform,"Right empty parcel ground state");
   visual.LeftConstruction=works;visual.RightConstruction=Empty(states.transform,"Camp construction state / art pending");
   visual.LeftAvailable=Empty(states.transform,"Sawmill available state");visual.RightAvailable=Empty(states.transform,"Camp available state");
   visual.Bindings=new[]{new ValoriaParcelPresentation.Binding{ParcelId="LeftCabinParcel",BuildingId="sawmill",VariantBit=1,Built=sawmill,Ground=visual.LeftGround,Construction=works,Available=visual.LeftAvailable,Target=left},new ValoriaParcelPresentation.Binding{ParcelId="RightCampParcel",BuildingId="barracks",VariantBit=2,Built=camp,Ground=visual.RightGround,Construction=visual.RightConstruction,Available=visual.RightAvailable,Target=right}};
   // State machine owns presentation; no fabricated final state in the saved scene.
   var prior=SliceContentProfiles.ActiveRuntimeProfile;SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
   try{var gateway=new Eldoria.Application.LocalGateway(new Eldoria.Infrastructure.SystemClock(),new InitialStore());visual.Apply(gateway.Snapshot());}
   finally{SliceContentProfiles.SetRuntimeProfileOverride(prior);}
   var runtime=Empty(null,"EXPERIMENTAL / existing LocalGateway and SlicePresenter with isolated save");
   var presenter=runtime.AddComponent<SlicePresenter>();var host=runtime.AddComponent<ValoriaEntryExperimentalRuntime>();host.Presenter=presenter;host.ExperimentalScene=ScenePath;
   visual.Home();UnityEngine.Object.FindFirstObjectByType<ValoriaUprightViewBridge>().Sync();AssetDatabase.SaveAssets();Req(EditorSceneManager.SaveScene(scene,ScenePath,true),"Scene save failed");
   File.WriteAllText(Path.Combine(Ev,"scene-integration.json"),"{\"scene\":\""+ScenePath+"\",\"gameplay\":\"EXISTING_LOCAL_GATEWAY_AND_PRESENTER\",\"save\":\"ISOLATED\",\"canonical_modified\":false,\"art_status\":\"CANDIDATE_NOT_APPROVED\",\"physical_mobile_profiled\":false}");
  }
  sealed class InitialStore:Eldoria.Application.IStateStore {PlayerState s;public PlayerState Load()=>s;public void Save(PlayerState state){s=state;}}
  public static void PlayProbe(){EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);EditorApplication.EnterPlaymode();}
 }
}
