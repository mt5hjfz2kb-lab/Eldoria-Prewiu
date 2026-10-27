#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;
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
  static IEnumerator Probe(ValoriaIntegratedWorld h){
   for(int i=0;i<180&&!h.Ready;i++)yield return null;Need(h.Ready,"integrated production host ready");
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
    var target=h.FindTarget(id);Need(target&&target.gameObject.activeInHierarchy,id+" visible");
    h.WorldNavigation.RestoreView(target.transform.position,14);yield return null;Shot(h,id,h.WorldCamera);
   }
   var fissure=h.FindTarget("fissure-valoria");h.WorldNavigation.RestoreView(fissure.transform.position,14);yield return null;
   var offer=h.Read("fissure-valoria",false).Building.Action;Need(offer.Kind=="ContactLyra","contact offer before assault");h.Submit(offer);yield return null;
   Need(h.Hud.NarrativeOpen,"Lyra real dialogue opens");Need(!h.City.Gateway.Snapshot().ChapterThree.LyraContacted,"dialogue completion gates contact");
   for(int i=0;i<30&&h.Hud.NarrativeOpen;i++){h.Hud.Modal.Extra.onClick.Invoke();yield return null;}
   Need(h.City.Gateway.Snapshot().ChapterThree.LyraContacted,"Lyra dialogue completed");
   Shot(h,"fissure-A-open",h.WorldCamera);Command(h,"Fight","fissure-valoria");Need(h.City.Gateway.Snapshot().March.Route.Count>1,"real reachable fissure route");Step(h,2);yield return null;Shot(h,"fissure-B-scar",h.WorldCamera);Step(h,7);yield return null;Shot(h,"fissure-C-closed",h.WorldCamera);
   Need(!fissure.gameObject.activeInHierarchy,"private fissure fully disappears");h.City.ReloadForProbe();var victory=h.City.Gateway.Snapshot();Need(victory.ChapterThree.LyraRecruited&&victory.ChapterThree.Inventory.FindAll(x=>x=="breach-manuscript").Count==1,"persistent Lyra and unique manuscript");
   foreach(var id in new[]{"hunt-wolf","hunt-boar","meat-valoria"}){Command(h,id=="meat-valoria"?"Gather":"Fight",id);Need(h.City.Gateway.Snapshot().March.Route.Count>1,id+" real route");Step(h,20);}
   Need(h.City.Gateway.Snapshot().ChapterThree.Hunts==2,"both hunts resolved");Command(h,"ConfigureMarch","march-main:20:aldric+lyra");h.City.ReloadForProbe();Need(h.City.Gateway.Snapshot().PreparedHeroId=="aldric+lyra","recruited heroes persist in composition");
   h.TrySetWorld(false);yield return null;cityCam.transform.SetPositionAndRotation(cameraPosition,cameraRotation);
   Command(h,"UpgradeBuilding","granary");Step(h,12);yield return null;Shot(h,"E-granary-level3",cityCam);
   foreach(string building in new[]{"sawmill","barracks"})foreach(int level in new[]{2,3}){Command(h,"UpgradeBuilding",building);Step(h,8+(level-1)*2);yield return null;Shot(h,building+"-level"+level,cityCam);}
   h.City.ReloadForProbe();Need(h.City.Gateway.Snapshot().ChapterThree.GranaryLevel==3,"level3 save reload");
   File.WriteAllLines(Ev+"/functional-checks.txt",checks);File.WriteAllText(Ev+"/runtime-save-fixture.json",JsonUtility.ToJson(h.City.Gateway.Snapshot(),true));Debug.Log("BASTION_III_NATIVE_PROOF_PASS");EditorApplication.Exit(0);
  }
 }
}
#endif
