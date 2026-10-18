// Isolated scene adapter. Uses the game's existing gateway, presenter and parcel
// state machine. Canonical save files, gameplay source and published scenes untouched.
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;
using Eldoria.Presentation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace Eldoria.ValoriaLayered {
 [DefaultExecutionOrder(-900)]
 public sealed class ValoriaEntryExperimentalRuntimeV2 : MonoBehaviour {
  public SlicePresenter Presenter;
  public string ExperimentalScene="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/Valoria_SHARP_Entry_PlayableV2.unity";
  static ValoriaEntryExperimentalRuntimeV2 active;bool ownsRuntime;
  LocalGateway gateway;FileStateStore store;IClock clock;string priorProfile;int lastScene=-1;bool probe;
  string canonicalBefore;
  static string Fingerprint(string path){if(!File.Exists(path))return "ABSENT";using(var h=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path)));}
  static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
  sealed class ProbeClock:IClock {public long UtcTicks{get;private set;}=DateTime.UtcNow.Ticks;public void Step(int seconds){UtcTicks+=TimeSpan.FromSeconds(seconds).Ticks;}}
  void Awake(){
   if(!UnityEngine.Application.isPlaying)return;
   if(active!=null&&active!=this){Destroy(gameObject);return;}active=this;ownsRuntime=true;
   priorProfile=SliceContentProfiles.ActiveRuntimeProfile;
   SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
   canonicalBefore=Fingerprint(SliceBoot.SavePath);
   probe=Environment.GetEnvironmentVariable("VALORIA_ENTRY_PROBE")=="1";
   clock=probe?(IClock)new ProbeClock():new SystemClock();
   string path=probe?Path.Combine(Path.GetTempPath(),"eldoria-entry-probe-"+Guid.NewGuid().ToString("N")+".json"):
    Path.Combine(UnityEngine.Application.persistentDataPath,"eldoria-entry-experimental-v2-owner-i-ii.json");
   store=new FileStateStore(path);gateway=new LocalGateway(clock,store);
   if(Presenter==null)Presenter=GetComponent<SlicePresenter>();
   Presenter.Initialize(gateway);DontDestroyOnLoad(gameObject);SceneManager.sceneLoaded+=Loaded;
  }
  void Start(){if(!UnityEngine.Application.isPlaying||!ownsRuntime)return;Loaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);if(probe)StartCoroutine(Probe());}
  void Loaded(Scene scene,LoadSceneMode mode){
   if(!ownsRuntime||Presenter==null||scene.name=="Bootstrap"||lastScene==scene.handle)return;
   lastScene=scene.handle;Presenter.OnSceneLoaded(scene,mode);ProtectExperimentalNavigation();
  }
  T Field<T>(string name)where T:class=>typeof(SlicePresenter).GetField(name,Private)?.GetValue(Presenter) as T;
  object Invoke(string name,params object[] args)=>typeof(SlicePresenter).GetMethod(name,Private).Invoke(Presenter,args);
  void ProtectExperimentalNavigation(){
   var reset=Field<Button>("resetButton");if(reset!=null){reset.onClick.RemoveAllListeners();reset.onClick.AddListener(ResetThisExperiment);}
   var label=Field<Text>("resetButtonText");if(label!=null)label.text="Restablecer prueba";
   var city=Field<Button>("cityNavButton");if(city!=null){city.onClick.RemoveAllListeners();city.onClick.AddListener(LoadExperiment);}
  }
  void ResetThisExperiment(){store.DeleteLocalState();gateway=new LocalGateway(clock,store);Presenter.Initialize(gateway);LoadExperiment();}
  void LoadExperiment(){
#if UNITY_EDITOR
   UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(ExperimentalScene,new LoadSceneParameters(LoadSceneMode.Single));
#else
   Debug.LogError("Experimental scene is editor-only until its publication gates pass.");
#endif
  }
  void OnDestroy(){if(!ownsRuntime)return;active=null;SceneManager.sceneLoaded-=Loaded;SliceContentProfiles.SetRuntimeProfileOverride(priorProfile);}
  [Serializable] sealed class ProbeResult {public string status,scene,profile,clock_mode,save_scope,error;public bool recruit_ui,march_ui,chapter_ii_complete,world_navigation,return_to_entry,isolated_reset;public bool ui_build_sawmill,ui_build_barracks,bastion_i_ii,persistence,existing_presenter_picking,canonical_save_touched;public long revision;public int sawmill,barracks,bastion;}
  static void Check(bool ok,string why){if(!ok)throw new Exception("ENTRY_GAMEPLAY: "+why);}
  void CaptureBuilt(string dir){
   var view=UnityEngine.Object.FindFirstObjectByType<ValoriaUprightViewBridge>();view.Sync();var c=view.RenderCamera;
   var prev=c.targetTexture;var active=RenderTexture.active;float aspect=c.aspect;
   var rt=new RenderTexture(1536,1024,24){antiAliasing=4};var tex=new Texture2D(1536,1024,TextureFormat.RGB24,false);
   try{c.aspect=1.5f;c.targetTexture=rt;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1536,1024),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(dir,"playmode-built-i-ii-geometry.png"),tex.EncodeToPNG());}
   finally{c.targetTexture=prev;c.aspect=aspect;RenderTexture.active=active;rt.Release();Destroy(rt);Destroy(tex);}
  }
  IEnumerator Probe(){
   yield return null;yield return null;
   string dir=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..","docs/evidence/valoria-layered-construction-20261016/playable-entry-v2"));Directory.CreateDirectory(dir);
   var result=new ProbeResult{scene=ExperimentalScene,profile=SliceContentProfiles.ActiveRuntimeProfile,
    clock_mode="Existing IClock logical time for QA; unchanged OWNER_I_II rules",save_scope="Unique temporary FileStateStore; canonical save hash guarded",status="IN_PROGRESS"};
   bool Phase(Action action){if(result.status=="FAIL")return false;try{action();return true;}catch(Exception e){result.status="FAIL";result.error=e.ToString();Debug.LogError(e);return false;}}
   void Command(string kind,string target){var state=gateway.Snapshot();var r=gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),state.PlayerId,kind,target,state.Revision));Check(r.Ok,kind+": "+r.Message);}
   void March(string kind,string target){Command(kind,target);((ProbeClock)clock).Step(120);gateway.Advance();}
   void Pick(string id,Collider target){
    var visual=UnityEngine.Object.FindFirstObjectByType<ValoriaParcelPresentation>();var view=UnityEngine.Object.FindFirstObjectByType<ValoriaUprightViewBridge>();view.Sync();
    var p=visual.ProductionCamera.WorldToScreenPoint(target.bounds.center);var selected=Invoke("ResolveHotspot",new Vector2(p.x,p.y)) as WorldHotspot;
    Check(selected!=null&&selected.Id==id,"Existing presenter picked wrong source target: "+id);Invoke("Select",id);
   }
   void Click(){var b=Field<Button>("buildingAction");Check(b!=null&&b.interactable,"Real action disabled");b.onClick.Invoke();}
   Phase(()=>{
    var visual=UnityEngine.Object.FindFirstObjectByType<ValoriaParcelPresentation>();Check(visual!=null,"No parcel presentation");
    var left=Array.Find(visual.Bindings,b=>b.BuildingId=="sawmill");var right=Array.Find(visual.Bindings,b=>b.BuildingId=="barracks");
    Check(visual.LeftState==ParcelBuildingState.AVAILABLE&&visual.RightState==ParcelBuildingState.NOT_BUILT,"Initial state drift");
    Pick("sawmill",left.Target);Click();Check(visual.LeftState==ParcelBuildingState.UNDER_CONSTRUCTION,"Sawmill UI did not start");
    Check(ParcelBuildingStates.For(new LocalGateway(clock,store).Snapshot(),"sawmill")==ParcelBuildingState.UNDER_CONSTRUCTION,"Construction persistence failed");
    ((ProbeClock)clock).Step(SliceRules.SawmillBuildSeconds+1);gateway.Advance();Invoke("Refresh");Check(visual.LeftState==ParcelBuildingState.BUILT&&left.Built.activeSelf,"Sawmill visual state");result.ui_build_sawmill=true;
    March("Gather","forest-valoria");March("Gather","forest-valoria");March("Gather","quarry-valoria");March("Fight","corrupt-scout");
    Pick("bastion",GameObject.Find("InteractiveProxy_Bastion").GetComponent<Collider>());Click();Check(gateway.Snapshot().BastionLevel==2&&visual.RightState==ParcelBuildingState.AVAILABLE,"Bastion UI I-II failed");result.bastion_i_ii=true;
    Pick("barracks",right.Target);Click();Check(visual.RightState==ParcelBuildingState.UNDER_CONSTRUCTION,"Camp UI did not start");
    ((ProbeClock)clock).Step(SliceRules.BarracksBuildSeconds+1);gateway.Advance();Invoke("Refresh");Check(visual.RightState==ParcelBuildingState.BUILT&&right.Built.activeSelf,"Camp visual state");result.ui_build_barracks=true;
    if(gateway.Snapshot().Resources.Wood<SliceRules.RecruitWoodCost)March("Gather","forest-valoria");
    if(gateway.Snapshot().Resources.Stone<SliceRules.RecruitStoneCost)March("Gather","quarry-valoria");
    Pick("barracks",right.Target);Click();Check(gateway.Snapshot().RecruitmentCompletesUtcTicks>0,"Recruit UI failed");
    ((ProbeClock)clock).Step(SliceRules.RecruitSeconds+1);gateway.Advance();Invoke("Refresh");Check(gateway.Snapshot().ChapterProgress.TrainedArchers>=SliceContentProfiles.Active.Chapter2TrainArchers,"Training missing");result.recruit_ui=true;
    Pick("barracks",right.Target);Click();Click();Check(gateway.Snapshot().MarchConfigured,"March UI confirmation failed");result.march_ui=true;
    Pick("sawmill",left.Target);Pick("barracks",right.Target);result.existing_presenter_picking=true;
    Invoke("CloseBuildingPanel");var gate=Array.Find(UnityEngine.Object.FindObjectsByType<WorldHotspot>(FindObjectsSortMode.None),h=>h.Id=="gate");Check(gate!=null,"Original gate contact missing");Pick("gate",gate.GetComponent<Collider>());
   });
   yield return null;yield return null;
   Phase(()=>{
    Check(SceneManager.GetActiveScene().name=="Frontier","Gate did not navigate to existing world");result.world_navigation=true;
    Invoke("Select","engendro-valoria");Click();((ProbeClock)clock).Step(120);gateway.Advance();Invoke("Refresh");
    Check(gateway.Snapshot().EngendroDefeated&&SliceRules.CurrentObjectiveKey(gateway.Snapshot())=="b2.complete","Existing chapter II not complete");result.chapter_ii_complete=true;
    var city=Field<Button>("cityNavButton");Check(city!=null,"No existing city navigation button");city.onClick.Invoke();
   });
   yield return null;yield return null;
   Phase(()=>{
    Check(SceneManager.GetActiveScene().path==ExperimentalScene,"World return escaped experimental scene");result.return_to_entry=true;
    var visual=UnityEngine.Object.FindFirstObjectByType<ValoriaParcelPresentation>();Check(visual.LeftState==ParcelBuildingState.BUILT&&visual.RightState==ParcelBuildingState.BUILT,"Visual state lost on return");
    var final=gateway.Snapshot();var reloaded=new LocalGateway(clock,new FileStateStore(store.PathName)).Snapshot();Check(JsonUtility.ToJson(final)==JsonUtility.ToJson(reloaded),"Exact save reload mismatch");result.persistence=true;
    result.revision=final.Revision;result.bastion=final.BastionLevel;result.sawmill=final.SawmillLevel;result.barracks=final.BarracksLevel;
    File.WriteAllText(Path.Combine(dir,"complete-playerstate-before-reset.json"),JsonUtility.ToJson(final,true));CaptureBuilt(dir);
    var reset=Field<Button>("resetButton");Check(reset!=null,"No existing reset button");reset.onClick.Invoke();
   });
   yield return null;yield return null;
   Phase(()=>{
    var initial=gateway.Snapshot();Check(SceneManager.GetActiveScene().path==ExperimentalScene&&initial.BastionLevel==1&&initial.SawmillLevel==0&&initial.BarracksLevel==0,"Isolated UI reset failed");result.isolated_reset=true;
    result.canonical_save_touched=Fingerprint(SliceBoot.SavePath)!=canonicalBefore;Check(!result.canonical_save_touched,"Canonical save hash changed");
    result.status="EXPERIMENTAL_EXISTING_UI_I_II_OBJECTIVE_NAVIGATION_PERSISTENCE_RESET_PASS_NOT_DEVICE_QA";
   });
   File.WriteAllText(Path.Combine(dir,"gameplay-playmode.json"),JsonUtility.ToJson(result,true));
   yield return new WaitForSecondsRealtime(1);
#if UNITY_EDITOR
   Debug.Log("VALORIA_ENTRY_PLAYMODE_PROBE_COMPLETE "+result.status);UnityEditor.EditorApplication.Exit(result.status=="FAIL"?1:0);
#endif
  }
 }
}
