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
 [InitializeOnLoad] public static class BastionIVVNativeProof {
  const string Flag="Eldoria.BastionIVV.NativeProof";static string Ev=>Path.GetFullPath("../docs/bastion-iv-v/native");static readonly List<string> checks=new List<string>();
  static BastionIVVNativeProof(){EditorApplication.playModeStateChanged+=Entered;}
  static void Need(bool ok,string message){if(!ok)throw new Exception(message);checks.Add(message);Debug.Log("IVV_PASS "+message);}
  static void Fail(Exception e){Directory.CreateDirectory(Ev);File.WriteAllText(Ev+"/failure.txt",e.ToString());Debug.LogException(e);EditorApplication.Exit(1);}
  public static void Capture(){try{Directory.CreateDirectory(Ev);Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");EditorSceneManager.OpenScene("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity");SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();}catch(Exception e){Fail(e);}}
  static void Entered(PlayModeStateChange state){if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Flag,false))return;SessionState.SetBool(Flag,false);EditorApplication.delayCall+=()=>{var h=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();if(!h){Fail(new Exception("No host"));return;}h.StartCoroutine(Guard(h));};}
  static IEnumerator Guard(ValoriaIntegratedWorld h){var probe=Probe(h);while(true){object item=null;bool more;try{more=probe.MoveNext();if(more)item=probe.Current;}catch(Exception e){Fail(e);yield break;}if(!more)yield break;yield return item;}}
  static void Step(ValoriaIntegratedWorld h,int sec){((ValoriaReferenceCleanGameplay.TestClock)h.City.Clock).Step(sec);h.City.Gateway.Advance();h.City.Presenter.RefreshState();h.GetComponent<BastionIIIWorldPresentation>().Apply();h.GetComponent<BastionIVVWorldPresentation>().Apply();}
  static void Do(ValoriaIntegratedWorld h,string kind,string target){var s=h.City.Gateway.Snapshot();var result=h.City.Gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),s.PlayerId,kind,target,s.Revision));Need(result.Ok,kind+":"+target+" "+result.Message);}
  static void Shot(ValoriaIntegratedWorld h,string name){foreach(var size in new[]{(1448,1086,"horizontal"),(900,1600,"vertical")})ValoriaReferenceCleanBuilder.Capture(h.PlayerCamera,Ev+"/"+name+"-"+size.Item3+".png",size.Item1,size.Item2);}
  static void Ui(ValoriaIntegratedWorld h,string name){ValoriaIntegrationBuild.Capture(h,Ev+"/ui-"+name+"-vertical.png",900,1600);ValoriaIntegrationBuild.Capture(h,Ev+"/ui-"+name+"-horizontal.png",1600,900);}
  static IEnumerator Dismiss(ValoriaIntegratedWorld h){for(int i=0;i<40&&h.Hud.NarrativeOpen;i++){h.Hud.Modal.Extra.onClick.Invoke();yield return null;}}
  static IEnumerator Probe(ValoriaIntegratedWorld h){
   for(int i=0;i<240&&!h.Ready;i++)yield return null;Need(h.Ready,"canonical scene ready");
   var art=h.GetComponent<BastionIVVWorldPresentation>();Need(art!=null,"real tier presenter");
   foreach(string id in new[]{"bastion","sawmill","barracks","granary","stoneworks"}){
    Need(art.ModelFor(id)==null,id+" V assets not instantiated before unlock");
    string resource=id=="bastion"?"bastion-v":id=="sawmill"?"aserradero-v":id=="barracks"?"cuartel-v":id=="granary"?"granero-v":"cantera-v";
    var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Resources/BastionIVV/Models/"+resource+".glb");
    Need(model&&model.GetComponentsInChildren<Renderer>(true).Length>0,id+" exact GLB imported");
    foreach(var r in model.GetComponentsInChildren<Renderer>(true))foreach(var m in r.sharedMaterials)Need(m&&m.shader&&!m.shader.name.Contains("Error"),id+" valid native material");
   }
   var seed=h.City.Gateway.Snapshot();seed.BastionLevel=3;seed.SawmillLevel=2;seed.BarracksLevel=2;seed.ChapterThree.GranaryLevel=2;seed.ChapterThree.FissureClosed=true;seed.ChapterThree.LyraRecruited=true;seed.ScoutDefeated=true;seed.EngendroDefeated=true;seed.JourneyComplete=true;seed.Resources.Wood=100000;seed.Resources.Stone=100000;seed.Resources.Food=100000;seed.ChapterThree.Inventory.AddRange(new[]{"iron-brace","treated-hide","stone-tusk"});seed.ChapterProgress.SeenNarrative.AddRange(new[]{"start","world","mark","barracks","secured","frontier-open","fissure-closed"});
   new FileStateStore(h.City.SaveFile).Save(seed);h.City.ReloadForProbe();yield return new WaitForSecondsRealtime(.5f);yield return Dismiss(h);Step(h,0);
   h.City.Home();var pos=h.City.PlayerCamera.transform.position;var rot=h.City.PlayerCamera.transform.rotation;float zoom=h.City.PlayerCamera.orthographicSize;
   Shot(h,"city-III");Do(h,"AdvanceBastion","bastion");Step(h,16);yield return new WaitForSecondsRealtime(.3f);yield return Dismiss(h);Need(h.City.Gateway.Snapshot().BastionLevel==4,"III to IV real gateway");h.City.PlayerCamera.transform.SetPositionAndRotation(pos,rot);h.City.PlayerCamera.orthographicSize=zoom;Shot(h,"city-IV");
   h.Hud.SelectBuilding("bastion");yield return null;var screen=h.Hud.GetComponent<RoyalGoldV3BuildingScreens>();Need(screen.IsOpen,"independent Bastion screen");Ui(h,"council-IV");Do(h,"ChooseDevelopment","production");h.City.ReloadForProbe();yield return new WaitForSecondsRealtime(.3f);Need(h.City.Gateway.Snapshot().ChapterThree.DevelopmentPriority=="production","choice survives actual FileStateStore reload");Ui(h,"bastion-IV");screen.Close();
   Do(h,"AdvanceBastion","bastion");Step(h,18);yield return new WaitForSecondsRealtime(.3f);yield return Dismiss(h);Step(h,0);Need(art.ModelFor("bastion").activeInHierarchy&&!h.City.BastionTwo.activeSelf,"V replaces keep only");Need(!art.ModelFor("sawmill").activeInHierarchy,"sawmill does not auto-promote");h.City.PlayerCamera.transform.SetPositionAndRotation(pos,rot);h.City.PlayerCamera.orthographicSize=zoom;Shot(h,"city-V-keep-only");
   Do(h,"Build","stoneworks");Step(h,12);yield return null;Need(h.City.Gateway.Snapshot().ChapterThree.StoneworksLevel==1,"quarry built individually");
   foreach(string id in new[]{"sawmill","barracks","granary","stoneworks"}){
    int from=id=="stoneworks"?1:2;
    for(int level=from;level<5;level++){
     h.City.PlayerCamera.transform.SetPositionAndRotation(pos,rot);h.City.PlayerCamera.orthographicSize=zoom;Shot(h,id+"-level-"+level);
     Do(h,"UpgradeBuilding",id);Step(h,8+level*2);yield return null;
     Need(art.ModelFor(id).activeInHierarchy==(level+1>=5),id+" exact individual visual threshold "+(level+1));
    }
    Shot(h,id+"-level-5");h.Hud.SelectBuilding(id=="barracks"?"upgrade-barracks":id);yield return null;Ui(h,id+"-V");screen.Close();
   }
   h.City.ReloadForProbe();yield return new WaitForSecondsRealtime(.3f);Step(h,0);h.City.PlayerCamera.transform.SetPositionAndRotation(pos,rot);h.City.PlayerCamera.orthographicSize=zoom;Shot(h,"city-V-complete");
   foreach(string id in new[]{"bastion","sawmill","barracks","granary","stoneworks"})Need(art.ModelFor(id).activeInHierarchy,id+" V survives save reload");
   Need(h.TrySetWorld(true),"world navigation survives IVV");yield return new WaitForSecondsRealtime(.3f);h.WorldNavigation.RestoreView(h.WorldHome,32);yield return null;Shot(h,"world-city-V");Need(h.TrySetWorld(false),"return to city");
   File.WriteAllLines(Ev+"/functional-checks.txt",checks);Debug.Log("BASTION_IV_V_NATIVE_PASS_VISUAL_REVIEW_REQUIRED");EditorApplication.Exit(0);
  }
 }
}
#endif
