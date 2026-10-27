#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Eldoria.ReferenceClean.Editor {
 [InitializeOnLoad] public static class BastionIIINativeProof {
  const string Flag="Eldoria.BastionIII.NativeProof";
  static string Ev=>Path.GetFullPath("../docs/bastion-iii/native");
  static readonly List<string> checks=new List<string>();
  static BastionIIINativeProof(){EditorApplication.playModeStateChanged+=Entered;}
  static void Need(bool value,string name){if(!value)throw new Exception(name);checks.Add(name);Debug.Log("BASTION_III_PASS "+name);}
  static void Fail(Exception e){Directory.CreateDirectory(Ev);File.WriteAllText(Ev+"/failure.txt",e.ToString());Debug.LogException(e);EditorApplication.Exit(1);}
  public static void Capture(){try{Directory.CreateDirectory(Ev);Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");EditorSceneManager.OpenScene("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity");SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();}catch(Exception e){Fail(e);}}
  static void Entered(PlayModeStateChange s){if(s!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Flag,false))return;SessionState.SetBool(Flag,false);EditorApplication.delayCall+=()=>{var h=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();if(!h){Fail(new Exception("Host missing"));return;}h.StartCoroutine(Guard(h));};}
  static IEnumerator Guard(ValoriaIntegratedWorld h){var p=Probe(h);while(true){object value=null;bool more;try{more=p.MoveNext();if(more)value=p.Current;}catch(Exception e){Fail(e);yield break;}if(!more)yield break;yield return value;}}
  static void Step(ValoriaIntegratedWorld h,int sec){((ValoriaReferenceCleanGameplay.TestClock)h.City.Clock).Step(sec);h.City.Gateway.Advance();h.City.Presenter.RefreshState();h.GetComponent<BastionIIIWorldPresentation>().Apply();}
  static void Command(ValoriaIntegratedWorld h,string kind,string target){var s=h.City.Gateway.Snapshot();var r=h.City.Gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),s.PlayerId,kind,target,s.Revision));Need(r.Ok,kind+":"+target+":"+r.Message);}
  [Serializable] class CaptureInfo {public string source,name;public Vector3 cameraPosition,cameraEuler;public float zoom;public int level,width,height;public bool fixture=true,artApproved=false;}
  static void Shot(ValoriaIntegratedWorld h,string name,Camera c){foreach(var size in new[]{(1448,1086,"horizontal"),(900,1600,"vertical")}){
   var path=Ev+"/"+name+"-"+size.Item3+".png";float aspect=c.aspect;try{c.aspect=(float)size.Item1/size.Item2;ValoriaReferenceCleanBuilder.Capture(c,path,size.Item1,size.Item2);}finally{c.aspect=aspect;}
   File.WriteAllText(Path.ChangeExtension(path,".json"),JsonUtility.ToJson(new CaptureInfo{source=Environment.GetEnvironmentVariable("GITHUB_SHA"),name=name,cameraPosition=c.transform.position,cameraEuler=c.transform.eulerAngles,zoom=c.orthographicSize,level=h.City.Gateway.Snapshot().BastionLevel,width=size.Item1,height=size.Item2},true));
  }}
  static void UiShot(ValoriaIntegratedWorld h,string name){
   ValoriaIntegrationBuild.Capture(h,Ev+"/ui-"+name+"-vertical.png",900,1600);
   ValoriaIntegrationBuild.Capture(h,Ev+"/ui-"+name+"-horizontal.png",1600,900);
  }
  // Strict artistic integration gate: real rendered models must occupy the
  // gameplay camera frustum. A visible *hotspot* alone is not artwork.
  static void RequireRealWorldModel(ValoriaIntegratedWorld h,string id)
  {
   var target=h.FindTarget(id);
   Need(target!=null&&target.gameObject.activeInHierarchy,id+" has an active target");
   var camera=h.WorldCamera;
   var planes=GeometryUtility.CalculateFrustumPlanes(camera);
   var renderers=target.GetComponentsInChildren<Renderer>(true);
   int visible=0, active=0;
   foreach(var renderer in renderers){
    var b=renderer.bounds;
    var pos=camera.WorldToViewportPoint(b.center);
    bool layer=((camera.cullingMask&(1<<renderer.gameObject.layer))!=0);
    bool frustum=GeometryUtility.TestPlanesAABB(planes,b);
    bool enabled=renderer.enabled&&renderer.gameObject.activeInHierarchy;
    var local=b.center-h.WorldHome;
    float terrain=h.WorldHome.y+Eldoria.World4XCanon.RegionOneBlenderLandscape.Height(local.x,local.z);
    Debug.Log("BASTION_III_ART_FRUSTUM id="+id+" renderer="+renderer.name+
      " world="+b.center+" bounds="+b.size+" target="+target.transform.position+
      " viewport="+pos+" ground="+terrain+" layer="+renderer.gameObject.layer+
      " cull="+layer+" enabled="+enabled+" frustum="+frustum);
    if(enabled&&layer)active++;
    if(enabled&&layer&&frustum&&pos.z>0f)visible++;
   }
   Need(renderers.Length>0&&active>0,id+" has live graphical renderers");
   Need(visible>0,id+" actual mesh must be visible inside world camera frustum");
  }
  static IEnumerator Probe(ValoriaIntegratedWorld h){
   for(int i=0;i<180&&!h.Ready;i++)yield return null;Need(h.Ready,"integrated production host ready");
   var mainBreach=h.WorldRoot.GetComponentsInChildren<MeshFilter>(true).Single(x=>x.name=="BLD_BreachFloor");
   var mainBreachMesh=mainBreach.sharedMesh;var mainBreachPosition=mainBreach.transform.position;var mainBreachScale=mainBreach.transform.localScale;
   var initial=h.City.Gateway.Snapshot();Need(initial.BastionLevel==1,"fresh probe starts at I");
   Need(!h.FindTarget("granary").gameObject.activeInHierarchy,"granary hidden before III");Need(!h.FindTarget("hunt-wolf").gameObject.activeInHierarchy,"wolf hidden before III");
   // Explicit disposable completed-II fixture. I-II regression is the separate existing native suite.
   var seed=initial;seed.BastionLevel=2;seed.SawmillLevel=1;seed.BarracksLevel=1;seed.JourneyComplete=true;seed.ScoutDefeated=true;seed.EngendroDefeated=true;seed.Available.ArcherT1=56;seed.Resources.Wood=6000;seed.Resources.Stone=6000;
   seed.ChapterProgress.SeenNarrative.AddRange(new[]{"start","world","mark","barracks","secured","frontier-open","fissure-closed"});
   new FileStateStore(h.City.SaveFile).Save(seed);h.City.ReloadForProbe();yield return new WaitForSecondsRealtime(.5f);yield return null;
   // Clear a fresh-probe prologue via its real Continue button, never by replacing its UI.
   for(int i=0;i<40&&h.Hud.NarrativeOpen;i++){h.Hud.Modal.Extra.onClick.Invoke();yield return null;}
   Need(!h.Hud.NarrativeOpen,"narrative dismisses using real button");
   var cityCam=h.City.PlayerCamera;var cameraPosition=cityCam.transform.position;var cameraRotation=cityCam.transform.rotation;
   Shot(h,"A-bastion-II",cityCam);Command(h,"AdvanceBastion","bastion");Step(h,14);h.City.ReloadForProbe();yield return new WaitForSecondsRealtime(.5f);
   h.GetComponent<BastionIIIWorldPresentation>().Apply();cityCam.transform.SetPositionAndRotation(cameraPosition,cameraRotation);Shot(h,"B-bastion-III-site",cityCam);
   Need(h.FindTarget("granary").gameObject.activeInHierarchy,"granary III unlock");
   Command(h,"Build","granary");Step(h,9);yield return null;Shot(h,"C-granary-level1",cityCam);
   Command(h,"UpgradeBuilding","granary");h.TrySetWorld(true);yield return new WaitForSecondsRealtime(.5f);h.GetComponent<BastionIIIWorldPresentation>().Apply();Command(h,"InvestigateFissure","fissure-valoria");Need(h.City.Gateway.Snapshot().ChapterThree.InvestigationRoute.Count>1,"investigation uses real terrain route");
   h.WorldNavigation.RestoreView(h.FindTarget("fissure-valoria").transform.position,14);yield return null;Shot(h,"fissure-investigation",h.WorldCamera);h.TrySetWorld(false);yield return null;cityCam.transform.SetPositionAndRotation(cameraPosition,cameraRotation);Step(h,1199);h.City.ReloadForProbe();Need(!h.City.Gateway.Snapshot().ChapterThree.FissureExplored,"persistent investigation not early");Step(h,1);yield return null;
   Need(!h.City.Gateway.Snapshot().ChapterThree.LyraRecruited,"investigation never recruits Lyra");Shot(h,"D-granary-level2",cityCam);
   h.TrySetWorld(true);yield return null;yield return null;h.GetComponent<BastionIIIWorldPresentation>().Apply();
   foreach(var id in new[]{"hunt-wolf","hunt-boar","meat-valoria","fissure-valoria"}){
    var target=h.FindTarget(id);Need(target!=null,id+" exists");
    h.WorldNavigation.RestoreView(target.transform.position,14);h.GetComponent<BastionIIIWorldPresentation>().Apply();yield return null;
    Need(target.gameObject.activeInHierarchy,id+" visible in near view");Shot(h,id,h.WorldCamera);
    RequireRealWorldModel(h,id);
   }
   var wolf=h.FindTarget("hunt-wolf").GetComponentInChildren<Animation>(true);
   Need(wolf&&wolf.IsPlaying("Idle"),"wolf uses its authored idle animation");
   var fissure=h.FindTarget("fissure-valoria");
   var splitFloor=fissure.GetComponentsInChildren<MeshFilter>(true).Single(x=>x.name=="Dark split floor");float gap=0;
   foreach(var vertex in splitFloor.sharedMesh.vertices){var p=splitFloor.transform.TransformPoint(vertex);float ground=h.WorldHome.y+Eldoria.World4XCanon.RegionOneBlenderLandscape.Height(p.x-h.WorldHome.x,p.z-h.WorldHome.z);gap=Mathf.Max(gap,Mathf.Abs(p.y-ground));}
   Need(gap<.02f,"private split floor follows actual terrain within two centimetres");h.WorldNavigation.RestoreView(fissure.transform.position,14);yield return null;
   var offer=h.Read("fissure-valoria",false).Building.Action;Need(offer.Kind=="ContactLyra","contact offer before assault");h.Submit(offer);yield return null;
   var aldricFace=h.Hud.GetComponentsInChildren<UnityEngine.UI.RawImage>(true).FirstOrDefault(x=>x.name=="Canonical Aldric portrait");
   Need(aldricFace&&!aldricFace.gameObject.activeInHierarchy,"Lyra dialogue never impersonates Aldric portrait");
   UiShot(h,"lyra-contact");Need(h.Hud.NarrativeOpen,"Lyra real dialogue opens");Need(!h.City.Gateway.Snapshot().ChapterThree.LyraContacted,"dialogue completion gates contact");
   for(int i=0;i<30&&h.Hud.NarrativeOpen;i++){h.Hud.Modal.Extra.onClick.Invoke();yield return null;}
   Need(h.City.Gateway.Snapshot().ChapterThree.LyraContacted,"Lyra dialogue completed");
   Shot(h,"fissure-A-open",h.WorldCamera);Command(h,"Fight","fissure-valoria");Need(h.City.Gateway.Snapshot().March.Route.Count>1,"real reachable fissure route");Step(h,2);yield return null;Shot(h,"fissure-B-scar",h.WorldCamera);Step(h,7);yield return null;Shot(h,"fissure-C-closed",h.WorldCamera);
   Need(mainBreach&&mainBreach.sharedMesh==mainBreachMesh&&mainBreach.transform.position==mainBreachPosition&&mainBreach.transform.localScale==mainBreachScale,"main Breach geometry preserved after private closure");
   Need(!fissure.gameObject.activeInHierarchy,"private fissure fully disappears");
   h.City.ReloadForProbe();
   // Exercise the real next-frame session rebind after an offline load.
   // The previous test invoked a new hunt in the exact same frame as a
   // replacement LocalGateway, before its map route service could reattach.
   yield return null;yield return null;
   var routeService=h.WorldRoot.GetComponentInChildren<Eldoria.World4XCanon.RegionOneResourcePlacement>(true);
   Need(routeService!=null&&routeService.MarchRoutes!=null,
      "world route service survives genuine save reload");
   var victory=h.City.Gateway.Snapshot();Need(victory.ChapterThree.LyraRecruited&&victory.ChapterThree.Inventory.FindAll(x=>x=="breach-manuscript").Count==1,"persistent Lyra and unique manuscript");
   foreach(var id in new[]{"hunt-wolf","hunt-boar","meat-valoria"}){
    Need(routeService.MarchRoutes.TryRoute(h.City.Gateway.Snapshot(),id,out var planned)&&planned!=null&&planned.Count>1,
      id+" route certified after save reload");
    Command(h,id=="meat-valoria"?"Gather":"Fight",id);
    var underway=h.City.Gateway.Snapshot().March;
    Need(underway.TargetId==id&&underway.Phase=="outbound"&&underway.Route.Count>1,
      id+" actual dispatched terrain route");
    Step(h,20);
   }
   Need(h.City.Gateway.Snapshot().ChapterThree.Hunts==2,"both hunts resolved");Command(h,"ConfigureMarch","march-main:20:aldric+lyra");h.City.ReloadForProbe();Need(h.City.Gateway.Snapshot().PreparedHeroId=="aldric+lyra","recruited heroes persist in composition");
   h.TrySetWorld(false);yield return null;cityCam.transform.SetPositionAndRotation(cameraPosition,cameraRotation);
   Command(h,"UpgradeBuilding","granary");Step(h,12);yield return null;Shot(h,"E-granary-level3",cityCam);
   foreach(string building in new[]{"sawmill","barracks"})foreach(int level in new[]{2,3}){Command(h,"UpgradeBuilding",building);Step(h,8+(level-1)*2);yield return null;Shot(h,building+"-level"+level,cityCam);}
   h.City.ReloadForProbe();Need(h.City.Gateway.Snapshot().ChapterThree.GranaryLevel==3,"level3 save reload");
   h.Select("granary");yield return null;UiShot(h,"granary-level3");
   h.Open(RoyalSection.Inventory);yield return null;UiShot(h,"unique-manuscript-chest");
   for(int i=0;i<10&&h.Hud.NarrativeOpen;i++){h.Hud.Modal.Extra.onClick.Invoke();yield return null;}
   var management=h.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
   Need(management!=null,"V3 management host exists");
   management.Open(RoyalGoldV3ManagementScreens.Destination.March);yield return null;UiShot(h,"recruited-hero-composition");management.Close();
   File.WriteAllLines(Ev+"/functional-checks.txt",checks);File.WriteAllText(Ev+"/runtime-save-fixture.json",JsonUtility.ToJson(h.City.Gateway.Snapshot(),true));Debug.Log("BASTION_III_NATIVE_PROOF_PASS");EditorApplication.Exit(0);
  }
 }
}
#endif
