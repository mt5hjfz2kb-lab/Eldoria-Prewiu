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
  [Serializable] sealed class CameraEvidence {public string name,source;public Vector3 position,angles;public float orthographicSize,aspect;public int width,height;}
  static void Shot(ValoriaIntegratedWorld h,string name){foreach(var size in new[]{(1448,1086,"horizontal"),(900,1600,"vertical")}){var c=h.PlayerCamera;float prior=c.aspect;try{c.aspect=(float)size.Item1/size.Item2;string path=Ev+"/"+name+"-"+size.Item3+".png";ValoriaReferenceCleanBuilder.Capture(c,path,size.Item1,size.Item2);File.WriteAllText(Path.ChangeExtension(path,".json"),JsonUtility.ToJson(new CameraEvidence{name=name,source=Environment.GetEnvironmentVariable("ELDORIA_COMPILED_SOURCE")??Environment.GetEnvironmentVariable("GITHUB_SHA"),position=c.transform.position,angles=c.transform.eulerAngles,orthographicSize=c.orthographicSize,aspect=c.aspect,width=size.Item1,height=size.Item2},true));}finally{c.aspect=prior;}}}
  static void VisibleDelta(string before,string after){
   var a=new Texture2D(2,2);var b=new Texture2D(2,2);
   try{Need(a.LoadImage(File.ReadAllBytes(Ev+"/"+before+"-horizontal.png"))&&b.LoadImage(File.ReadAllBytes(Ev+"/"+after+"-horizontal.png")),"decode actual upgrade captures");
    var x=a.GetPixels32();var y=b.GetPixels32();Need(x.Length==y.Length,"same camera resolution");int changed=0;
    for(int i=0;i<x.Length;i++)if(Math.Abs(x[i].r-y[i].r)>12||Math.Abs(x[i].g-y[i].g)>12||Math.Abs(x[i].b-y[i].b)>12)changed++;
    Need(changed>=120,before+" -> "+after+" visible pixels="+changed);
   }finally{Object.Destroy(a);Object.Destroy(b);}
  }
  static void Ui(ValoriaIntegratedWorld h,string name){ValoriaIntegrationBuild.Capture(h,Ev+"/ui-"+name+"-vertical.png",900,1600,()=>VerifyManagementLayout(h));ValoriaIntegrationBuild.Capture(h,Ev+"/ui-"+name+"-horizontal.png",1600,900,()=>VerifyManagementLayout(h));}
  static Rect LocalRect(RectTransform root,RectTransform child){var corners=new Vector3[4];child.GetWorldCorners(corners);var min=root.InverseTransformPoint(corners[0]);var max=root.InverseTransformPoint(corners[2]);return Rect.MinMaxRect(min.x,min.y,max.x,max.y);}
  static void VerifyManagementLayout(ValoriaIntegratedWorld h){
   var management=h.Hud.GetComponent<RoyalGoldV3BuildingScreens>();var ranking=h.Hud.GetComponent<RoyalGoldV3RankingScreen>();
   var root=ranking&&ranking.IsOpen?ranking.VisibleRoot:management.VisibleRoot;Need(root!=null,"actual management canvas visible");
   foreach(var button in root.GetComponentsInChildren<UnityEngine.UI.Button>()){
    var r=LocalRect(root,(RectTransform)button.transform);Need(r.xMin>=root.rect.xMin-.5f&&r.xMax<=root.rect.xMax+.5f&&r.yMin>=root.rect.yMin-.5f&&r.yMax<=root.rect.yMax+.5f,"visible touch target inside destination: "+button.name);
   }
   if(ranking&&ranking.IsOpen)return;
   Need(management.ReferenceTexture&&management.ReferenceTexture.width>100,"approved reference texture bound: "+management.BuildingId);
   foreach(var button in root.GetComponentsInChildren<UnityEngine.UI.Button>()){var text=button.GetComponentInChildren<UnityEngine.UI.Text>();Need(text&&!string.IsNullOrWhiteSpace(text.text),"visible button has a real label: "+button.name);}
   var choices=root.Find("Building management/Development council") as RectTransform;
   if(choices&&choices.gameObject.activeInHierarchy){var status=root.Find("Building management/Requirements and progress") as RectTransform;Need(!LocalRect(root,status).Overlaps(LocalRect(root,choices)),"council choices do not overlap explanation at actual runtime scale");}
  }
  static void VerifyWorldCityTouch(ValoriaIntegratedWorld h,BastionIVVWorldPresentation art){
   var rs=art.WorldModel.GetComponentsInChildren<Renderer>();var bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);
   Physics.SyncTransforms();var screen=h.WorldCamera.WorldToScreenPoint(bounds.center);
   var hits=Physics.RaycastAll(h.WorldCamera.ScreenPointToRay(screen),2000,1<<8,QueryTriggerInteraction.Collide);
   var actual=h.FindTarget("valoria-map-city");var collider=actual?actual.GetComponent<BoxCollider>():null;
   File.AppendAllText(Ev+"/world-hitbox-diagnostic.txt","zoom="+h.WorldCamera.orthographicSize+" model="+bounds+" ray="+h.WorldCamera.ScreenPointToRay(screen)+" collider="+(collider?collider.bounds.ToString():"missing")+" local="+(collider?collider.size.ToString():"missing")+" active="+(actual&&actual.gameObject.activeInHierarchy)+" enabled="+(collider&&collider.enabled)+" hits="+string.Join(",",hits.Select(x=>x.collider.name))+"\n");
   Need(hits.Any(hit=>{var target=hit.collider.GetComponentInParent<Eldoria.Presentation.WorldHotspot>();return target&&target.Id=="valoria-map-city";}),"actual V city is touchable at zoom "+h.WorldCamera.orthographicSize);
   float ground=h.WorldHome.y+Eldoria.World4XCanon.RegionOneCanonicalLandscape.SampleHeight(bounds.center.x-h.WorldHome.x,bounds.center.z-h.WorldHome.z);
   Need(bounds.min.y>=ground-.02f&&bounds.min.y<=ground+.25f,"V city foundation meets actual regional terrain at zoom "+h.WorldCamera.orthographicSize);
  }
  static IEnumerator Dismiss(ValoriaIntegratedWorld h){for(int i=0;i<40&&h.Hud.NarrativeOpen;i++){h.Hud.Modal.Extra.onClick.Invoke();yield return null;}}
  static void MilitaryUi(ValoriaIntegratedWorld h,string name){
   foreach(var size in new[]{(1600,900,"horizontal"),(900,1600,"vertical")})
    ValoriaIntegrationBuild.Capture(h,Ev+"/parity-"+name+"-"+size.Item3+".png",size.Item1,size.Item2,()=>{
     var root=h.Hud.GetComponent<RoyalGoldV3ManagementScreens>().VisibleRoot;
     foreach(var b in root.GetComponentsInChildren<UnityEngine.UI.Button>()){
      var r=LocalRect(root,(RectTransform)b.transform);
      Need(r.xMin>=root.rect.xMin-.5f&&r.xMax<=root.rect.xMax+.5f&&r.yMin>=root.rect.yMin-.5f&&r.yMax<=root.rect.yMax+.5f,"parity military control contained: "+b.name);
     }
    });
  }
  static IEnumerator MigrationParity(ValoriaIntegratedWorld h){
   var military=h.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
   h.Hud.SelectBuilding("barracks");yield return null;
   Need(military.IsOpen,"real training destination opens for parity");
   var tier=h.Hud.GetComponentsInChildren<UnityEngine.UI.Button>().Single(x=>x.name=="Select archer tier");
   Need(tier.interactable,"real T2 selector unlocked by CuartelV");tier.onClick.Invoke();yield return null;
   Need(military.SelectedTier==2,"real selector chooses T2");MilitaryUi(h,"training-t2");
   var before=h.City.Gateway.Snapshot();int quantity=military.SelectedQuantity;
   military.VisibleAction.onClick.Invoke();yield return null;
   Need(h.City.Gateway.Snapshot().PendingRecruitTier==2,"visible training button starts authoritative T2");
   Step(h,1);yield return new WaitForSecondsRealtime(.5f);
   MilitaryUi(h,"training-speedup-choice");
   // Both rewards exist. Choose the one-minute item explicitly rather than silently wasting5m.
   var label=tier.GetComponentInChildren<UnityEngine.UI.Text>();
   if(!label.text.StartsWith("1 MIN",StringComparison.Ordinal)){tier.onClick.Invoke();yield return null;}
   Need(label.text.StartsWith("1 MIN",StringComparison.Ordinal),"speedup duration is a visible player choice");
   long deadline=h.City.Gateway.Snapshot().RecruitmentCompletesUtcTicks;
   int five=h.City.Gateway.Snapshot().SpeedupFiveMinutes;
   military.VisibleAction.onClick.Invoke();yield return null;
   Need(h.City.Gateway.Snapshot().RecruitmentCompletesUtcTicks==deadline-TimeSpan.FromMinutes(1).Ticks,"real training UI subtracts exactly1min");
   Need(h.City.Gateway.Snapshot().SpeedupFiveMinutes==five,"one-minute choice preserves five-minute inventory");
   Step(h,SliceRules.RecruitSecondsFor(quantity));yield return null;
   Need(h.City.Gateway.Snapshot().Available.ArcherT2==before.Available.ArcherT2+quantity&&h.City.Gateway.Snapshot().Available.ArcherT1==before.Available.ArcherT1,"actual T2 completion preserves T1");
   military.Close();h.City.ReloadForProbe();yield return null;
   Need(h.City.Gateway.Snapshot().Available.ArcherT2==before.Available.ArcherT2+quantity,"T2 actual FileStateStore reload");
   h.City.Home();yield return null;
   var city=h.City.PlayerCamera;var cityPos=city.transform.position;var cityRot=city.transform.rotation;float citySize=city.orthographicSize;
   Need(h.TrySetWorld(true),"parity enter world");yield return new WaitForSecondsRealtime(.3f);
   Need(h.WorldCamera!=city&&h.WorldCamera.enabled&&!city.enabled,"distinct cameras with only world active");
   var landscape=Object.FindFirstObjectByType<Eldoria.World4XCanon.RegionOneCanonicalLandscape>();
   var node=h.FindTarget("ash-stalker-valoria");Need(node&&node.gameObject.activeInHierarchy,"reachable authored rare encounter visible");
   var anchor=node.transform.position;float aspect=h.WorldCamera.aspect;
   foreach(var size in new[]{(1600,900,"horizontal"),(900,1600,"vertical")}){
    h.WorldCamera.aspect=(float)size.Item1/size.Item2;landscape.RefreshForCapture();
    Need(h.WorldNavigation.localZoom<h.WorldNavigation.regionalZoom&&h.WorldNavigation.regionalZoom<h.WorldNavigation.globalZoom,"ordered actual geography presets "+size.Item3);
    float previous=0;
    foreach(int preset in new[]{2,1,0}){
     h.WorldNavigation.SetZoomLevel(preset,true);landscape.RefreshForCapture();
     Need(h.WorldCamera.orthographicSize>previous,"actual world zoom monotonically increases "+size.Item3+":"+preset);previous=h.WorldCamera.orthographicSize;
     Need(node.transform.position==anchor,"same geographic rare node at every scale");
     Need(city.transform.position==cityPos&&city.transform.rotation==cityRot&&Mathf.Abs(city.orthographicSize-citySize)<.001f,"world presets never change city pose");
     ValoriaReferenceCleanBuilder.Capture(h.WorldCamera,Ev+"/parity-world-"+size.Item3+"-preset"+preset+".png",size.Item1,size.Item2);
    }
    h.WorldNavigation.SetZoomLevel(1,true);float regional=h.WorldCamera.orthographicSize;
    h.WorldNavigation.ZoomAt(new Vector2(h.WorldCamera.pixelWidth*.5f,h.WorldCamera.pixelHeight*.5f),.9f,true);
    Need(h.WorldCamera.orthographicSize<regional&&h.WorldCamera.orthographicSize>h.WorldNavigation.localZoom,"continuous world zoom between presets "+size.Item3);
   }
   h.WorldCamera.aspect=aspect;landscape.RefreshForCapture();h.WorldNavigation.SetZoomLevel(2,true);h.Focus(node.gameObject);yield return null;
   Physics.SyncTransforms();var point=h.WorldCamera.WorldToScreenPoint(node.transform.position);
   Need(Physics.RaycastAll(h.WorldCamera.ScreenPointToRay(point),2000,1<<8,QueryTriggerInteraction.Collide).Any(x=>x.collider.GetComponentInParent<Eldoria.Presentation.WorldHotspot>()==node),"rare real world raycast selects encounter");
   Shot(h,"parity-rare-before-battle");Do(h,"Fight","ash-stalker-valoria");Step(h,2*SliceRules.TravelSeconds);yield return Dismiss(h);Step(h,0);
   Need(h.City.Gateway.Snapshot().ChapterThree.AshStalkerDefeated,"actual routed rare victory");
   Need(h.City.Gateway.Snapshot().LastBattleReport.RewardFood==120&&h.City.Gateway.Snapshot().LastBattleReport.RewardStone==110,"rare base loot is separate from mission reward");
   Need(h.City.Gateway.Snapshot().ChapterProgress.GrantedRewards.Contains("c5-rare"),"rare mission ledger paid once");
   Need(!node.gameObject.activeInHierarchy,"defeated rare hides during cooldown");
   Need(h.TrySetWorld(false),"parity returns to city");yield return null;
   Need(city.enabled&&!h.WorldCamera.enabled&&Vector3.Distance(city.transform.position,cityPos)<.01f&&Mathf.Abs(city.orthographicSize-citySize)<.01f,"return restores city camera only");
   h.City.ReloadForProbe();yield return null;Step(h,0);
   Need(h.City.Gateway.Snapshot().ChapterThree.AshStalkerDefeated,"rare victory persisted through FileStateStore reload");
   Step(h,60);Need(h.TrySetWorld(true),"reopen world after rare cooldown");yield return null;Step(h,0);Need(node.gameObject.activeInHierarchy,"same rare node respawns after60seconds without resetting mission");Need(h.TrySetWorld(false),"return after respawn verification");
  }
  static IEnumerator Probe(ValoriaIntegratedWorld h){
   for(int i=0;i<240&&!h.Ready;i++)yield return null;Need(h.Ready,"canonical scene ready");
   var art=h.GetComponent<BastionIVVWorldPresentation>();Need(art!=null,"real tier presenter");
   foreach(string id in new[]{"bastion","sawmill","barracks","granary","stoneworks"}){
    Need(art.ModelFor(id)==null,id+" V assets not instantiated before unlock");
    string resource=id=="bastion"?"bastion-v":id=="sawmill"?"aserradero-v":id=="barracks"?"cuartel-v":id=="granary"?"granero-v":"cantera-v";
    var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Resources/BastionIVV/Models/"+resource+".glb");
    Need(model&&model.GetComponentsInChildren<Renderer>(true).Length>0,id+" exact GLB imported");
    var reference=Resources.Load<Texture2D>("BastionIVV/Portraits/"+resource);Debug.Log("IVV_REFERENCE_RESOURCE "+resource+" texture="+(reference?reference.name+" "+reference.width+"x"+reference.height:"NULL")+" assetType="+AssetDatabase.GetMainAssetTypeAtPath("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Resources/BastionIVV/Portraits/"+resource+".jpeg"));
    foreach(var r in model.GetComponentsInChildren<Renderer>(true))foreach(var m in r.sharedMaterials)Need(m&&m.shader&&!m.shader.name.Contains("Error"),id+" valid native material");
   }
   var seed=h.City.Gateway.Snapshot();seed.BastionLevel=3;seed.SawmillLevel=2;seed.BarracksLevel=2;seed.ChapterThree.GranaryLevel=2;seed.ChapterThree.FissureClosed=true;seed.ChapterThree.LyraRecruited=true;seed.ScoutDefeated=true;seed.EngendroDefeated=true;seed.JourneyComplete=true;seed.Available.ArcherT1=56;seed.Resources.Wood=100000;seed.Resources.Stone=100000;seed.Resources.Food=100000;seed.ChapterThree.Inventory.AddRange(new[]{"iron-brace","treated-hide","stone-tusk"});seed.ChapterProgress.SeenNarrative.AddRange(new[]{"start","world","mark","barracks","secured","frontier-open","fissure-closed"});
   new FileStateStore(h.City.SaveFile).Save(seed);h.City.ReloadForProbe();yield return new WaitForSecondsRealtime(.5f);yield return Dismiss(h);Step(h,0);
   h.City.Home();var pos=h.City.PlayerCamera.transform.position;var rot=h.City.PlayerCamera.transform.rotation;float zoom=h.City.PlayerCamera.orthographicSize;
   Shot(h,"city-III");Do(h,"AdvanceBastion","bastion");Step(h,16);yield return new WaitForSecondsRealtime(.3f);yield return Dismiss(h);Need(h.City.Gateway.Snapshot().BastionLevel==4,"III to IV real gateway");h.City.PlayerCamera.transform.SetPositionAndRotation(pos,rot);h.City.PlayerCamera.orthographicSize=zoom;Shot(h,"city-IV");
   h.Hud.SelectBuilding("bastion");yield return null;var screen=h.Hud.GetComponent<RoyalGoldV3BuildingScreens>();Need(screen.IsOpen,"independent Bastion screen");Ui(h,"council-IV");var priorityButton=h.Hud.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(x=>x.gameObject.name=="Choose production");Need(priorityButton.isActiveAndEnabled&&priorityButton.interactable,"real council choice available");priorityButton.onClick.Invoke();Need(h.City.Gateway.Snapshot().ChapterThree.DevelopmentPriority=="production","real council UI persists choice");h.City.ReloadForProbe();yield return new WaitForSecondsRealtime(.3f);Need(h.City.Gateway.Snapshot().ChapterThree.DevelopmentPriority=="production","choice survives actual FileStateStore reload");Ui(h,"bastion-IV");screen.Close();
   Do(h,"AdvanceBastion","bastion");Step(h,18);yield return new WaitForSecondsRealtime(.3f);yield return Dismiss(h);Step(h,0);Need(art.ModelFor("bastion").activeInHierarchy&&!h.City.BastionTwo.activeSelf,"V replaces keep only");Need(!art.ModelFor("sawmill").activeInHierarchy,"sawmill does not auto-promote");h.City.PlayerCamera.transform.SetPositionAndRotation(pos,rot);h.City.PlayerCamera.orthographicSize=zoom;Shot(h,"city-V-keep-only");
   Do(h,"Build","stoneworks");Step(h,12);yield return null;Need(h.City.Gateway.Snapshot().ChapterThree.StoneworksLevel==1,"quarry built individually");
   foreach(string id in new[]{"sawmill","barracks","granary","stoneworks"}){
    int from=id=="stoneworks"?1:2;
    for(int level=from;level<5;level++){
     h.City.PlayerCamera.transform.SetPositionAndRotation(pos,rot);h.City.PlayerCamera.orthographicSize=zoom;Shot(h,id+"-level-"+level);
     h.Hud.SelectBuilding(id=="barracks"?"upgrade-barracks":id);yield return null;Need(screen.VisibleAction&&screen.VisibleAction.interactable,id+" real upgrade button available");screen.VisibleAction.onClick.Invoke();Need(h.City.Gateway.Snapshot().ChapterThree.PendingBuildingLevel==level+1,id+" UI dispatches authoritative upgrade");screen.Close();Step(h,8+level*2);yield return null;
     Need(art.ModelFor(id).activeInHierarchy==(level+1>=5),id+" exact individual visual threshold "+(level+1));
    }
    h.City.PlayerCamera.transform.SetPositionAndRotation(pos,rot);h.City.PlayerCamera.orthographicSize=zoom;Shot(h,id+"-level-5");h.Hud.SelectBuilding(id=="barracks"?"upgrade-barracks":id);yield return null;Ui(h,id+"-V");screen.Close();
   }
   foreach(string id in new[]{"sawmill","barracks","granary","stoneworks"})for(int level=id=="stoneworks"?1:2;level<5;level++)VisibleDelta(id+"-level-"+level,id+"-level-"+(level+1));
   VisibleDelta("city-III","city-IV");VisibleDelta("city-IV","city-V-keep-only");
   h.City.ReloadForProbe();yield return new WaitForSecondsRealtime(.3f);Step(h,0);h.City.PlayerCamera.transform.SetPositionAndRotation(pos,rot);h.City.PlayerCamera.orthographicSize=zoom;Shot(h,"city-V-complete");
   foreach(string id in new[]{"bastion","sawmill","barracks","granary","stoneworks"})Need(art.ModelFor(id).activeInHierarchy,id+" V survives save reload");
   // Capture actual runtime per-model headings and close-ups before any diagnostic extra rotation.
   var headings=new List<string>();
   foreach(string id in new[]{"bastion","sawmill","barracks","granary","stoneworks"}){
    var model=art.ModelFor(id);var renders=model.GetComponentsInChildren<Renderer>();var bounds=renders[0].bounds;foreach(var renderer in renders)bounds.Encapsulate(renderer.bounds);
    headings.Add(id+" yaw="+model.transform.eulerAngles.y+" center="+bounds.center+" size="+bounds.size);
    h.City.PlayerCamera.transform.position=bounds.center+new Vector3(12,21,-34);h.City.PlayerCamera.transform.LookAt(bounds.center);h.City.PlayerCamera.orthographicSize=Mathf.Max(bounds.size.x,bounds.size.z)*.7f;Shot(h,"facade-"+id);
   }
   File.WriteAllLines(Ev+"/actual-runtime-headings.txt",headings);
   h.City.PlayerCamera.transform.position=new Vector3(0,100,18);h.City.PlayerCamera.transform.rotation=Quaternion.Euler(90,0,0);h.City.PlayerCamera.orthographicSize=40;Shot(h,"city-V-plan-alignment");
   h.City.PlayerCamera.transform.SetPositionAndRotation(pos,rot);h.City.PlayerCamera.orthographicSize=zoom;
   h.Hud.SelectBuilding("bastion");yield return null;
   var rankings=h.Hud.GetComponent<RoyalGoldV3RankingScreen>();rankings.Open();yield return null;Need(rankings.IsOpen,"quarry unlocks independent simulated ranking screen");Ui(h,"rank-power");
   foreach(var tab in new[]{"CORRUPTOS","RELIQUIAS"}){var button=h.Hud.GetComponentsInChildren<UnityEngine.UI.Button>(true).Single(x=>x.gameObject.name==tab);button.onClick.Invoke();yield return null;Ui(h,"rank-"+tab.ToLowerInvariant());}rankings.Close();screen.Close();
   Need(h.TrySetWorld(true),"world navigation survives IVV");yield return new WaitForSecondsRealtime(.3f);h.WorldNavigation.RestoreView(h.WorldHome,32);yield return null;Shot(h,"world-city-V");VerifyWorldCityTouch(h,art);Need(art.WorldModel.activeInHierarchy,"world V active at canonical city");foreach(var renderer in art.WorldModel.GetComponentsInChildren<Renderer>())Need((h.WorldCamera.cullingMask&(1<<renderer.gameObject.layer))!=0,"world camera includes V renderer layer");h.WorldNavigation.RestoreView(h.WorldHome,280);yield return null;Shot(h,"world-panorama-V");VerifyWorldCityTouch(h,art);h.WorldNavigation.RestoreView(h.WorldHome,32);yield return null;
   var vein=h.City.Gateway.Snapshot().WorldResourceLocations.First(x=>x.TargetId.StartsWith("region1:stone:",StringComparison.Ordinal)&&x.Remaining>0);Do(h,"Gather",vein.TargetId);Step(h,120);yield return new WaitForSecondsRealtime(.4f);Need(h.City.Gateway.Snapshot().ChapterThree.ExtractionOddity,"regional extraction triggers persistent V narrative");yield return Dismiss(h);
   Need(h.TrySetWorld(false),"return to city");
   var parity=MigrationParity(h);while(parity.MoveNext())yield return parity.Current;
   var earlier=h.City.Gateway.Snapshot();earlier.BastionLevel=3;earlier.SawmillLevel=2;earlier.BarracksLevel=2;earlier.ChapterThree.GranaryLevel=2;earlier.ChapterThree.StoneworksLevel=0;new FileStateStore(h.City.SaveFile).Save(earlier);h.City.ReloadForProbe();yield return null;Step(h,0);
   Need(!art.WorldModel.activeSelf,"loading earlier progress hides world V");foreach(string id in new[]{"bastion","sawmill","barracks","granary","stoneworks"})Need(!art.ModelFor(id).activeInHierarchy,"loading earlier progress hides "+id+" V");
   File.WriteAllLines(Ev+"/functional-checks.txt",checks);Debug.Log("BASTION_IV_V_NATIVE_PASS_VISUAL_REVIEW_REQUIRED");EditorApplication.Exit(0);
  }
 }
}
#endif
