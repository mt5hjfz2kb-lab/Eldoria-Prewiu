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
 public sealed class ValoriaEntryExperimentalRuntime : MonoBehaviour {
  public SlicePresenter Presenter;
  public string ExperimentalScene="Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/Valoria_SHARP_Entry_PlayableV1.unity";
  static ValoriaEntryExperimentalRuntime active;bool ownsRuntime;
  LocalGateway gateway;FileStateStore store;IClock clock;string priorProfile;int lastScene=-1;bool probe;
  static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
  sealed class ProbeClock:IClock {public long UtcTicks{get;private set;}=DateTime.UtcNow.Ticks;public void Step(int seconds){UtcTicks+=TimeSpan.FromSeconds(seconds).Ticks;}}
  void Awake(){
   if(!UnityEngine.Application.isPlaying)return;
   if(active!=null&&active!=this){Destroy(gameObject);return;}active=this;ownsRuntime=true;
   priorProfile=SliceContentProfiles.ActiveRuntimeProfile;
   SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
   probe=Environment.GetEnvironmentVariable("VALORIA_ENTRY_PROBE")=="1";
   clock=probe?(IClock)new ProbeClock():new SystemClock();
   string path=probe?Path.Combine(Path.GetTempPath(),"eldoria-entry-probe-"+Guid.NewGuid().ToString("N")+".json"):
    Path.Combine(UnityEngine.Application.persistentDataPath,"eldoria-entry-experimental-owner-i-ii.json");
   store=new FileStateStore(path);gateway=new LocalGateway(clock,store);
   if(Presenter==null)Presenter=GetComponent<SlicePresenter>();
   Presenter.Initialize(gateway);DontDestroyOnLoad(gameObject);SceneManager.sceneLoaded+=Loaded;
  }
  void Start(){if(!UnityEngine.Application.isPlaying)return;Loaded(SceneManager.GetActiveScene(),LoadSceneMode.Single);if(probe)StartCoroutine(Probe());}
  void Loaded(Scene scene,LoadSceneMode mode){
   if(Presenter==null||scene.name=="Bootstrap"||lastScene==scene.handle)return;
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
  [Serializable] sealed class ProbeResult {public string status,scene,profile,clock_mode,save_scope,error;public bool ui_build_sawmill,ui_build_barracks,bastion_i_ii,persistence,existing_presenter_picking,canonical_save_touched;public long revision;public int sawmill,barracks,bastion;}
  static void Check(bool ok,string why){if(!ok)throw new Exception("ENTRY_GAMEPLAY: "+why);}
  IEnumerator Probe(){
   yield return null;yield return null;
   var result=new ProbeResult{scene=ExperimentalScene,profile=SliceContentProfiles.ActiveRuntimeProfile,
    clock_mode="Existing IClock logical time advanced for QA; unchanged OWNER_I_II costs and rules",save_scope="Unique temporary FileStateStore, canonical save never opened",canonical_save_touched=false};
   try{
    var visual=UnityEngine.Object.FindFirstObjectByType<ValoriaParcelPresentation>();
    var view=UnityEngine.Object.FindFirstObjectByType<ValoriaUprightViewBridge>();Check(visual!=null&&view!=null,"Scene presentation missing");
    void Pick(string id,Collider target){view.Sync();var pixel=visual.ProductionCamera.WorldToScreenPoint(target.bounds.center);
     var selected=Invoke("ResolveHotspot",new Vector2(pixel.x,pixel.y)) as WorldHotspot;Check(selected!=null&&selected.Id==id,"Existing presenter picked wrong target: "+id);Invoke("Select",id);}
    var left=Array.Find(visual.Bindings,b=>b.BuildingId=="sawmill");var right=Array.Find(visual.Bindings,b=>b.BuildingId=="barracks");
    Check(left!=null&&right!=null,"Existing parcel bindings missing");
    Check(visual.LeftState==ParcelBuildingState.AVAILABLE&&visual.RightState==ParcelBuildingState.NOT_BUILT,"Initial parcel state drift");
    Pick("sawmill",left.Target);var action=Field<Button>("buildingAction");Check(action!=null&&action.interactable,"Real sawmill action disabled");action.onClick.Invoke();
    Check(visual.LeftState==ParcelBuildingState.UNDER_CONSTRUCTION,"Real UI did not start sawmill");
    Check(ParcelBuildingStates.For(new LocalGateway(clock,store).Snapshot(),"sawmill")==ParcelBuildingState.UNDER_CONSTRUCTION,"Construction persistence failed");
    ((ProbeClock)clock).Step(SliceRules.SawmillBuildSeconds+1);gateway.Advance();Invoke("Refresh");
    Check(visual.LeftState==ParcelBuildingState.BUILT&&left.Built.activeSelf,"Sawmill built presentation failed");result.ui_build_sawmill=true;
    void Command(string kind,string target){var state=gateway.Snapshot();var r=gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),state.PlayerId,kind,target,state.Revision));Check(r.Ok,kind+": "+r.Message);}
    void March(string kind,string target){Command(kind,target);((ProbeClock)clock).Step(120);gateway.Advance();}
    March("Gather","forest-valoria");March("Gather","forest-valoria");March("Gather","quarry-valoria");March("Fight","corrupt-scout");
    Command("AdvanceBastion","bastion");Invoke("Refresh");Check(visual.PresentedBastionLevel==2&&visual.RightState==ParcelBuildingState.AVAILABLE,"Bastion I-II or camp unlock failed");result.bastion_i_ii=true;
    Pick("barracks",right.Target);Check(action.interactable,"Real camp/barracks action disabled");action.onClick.Invoke();
    Check(visual.RightState==ParcelBuildingState.UNDER_CONSTRUCTION,"Real camp/barracks UI did not build");
    ((ProbeClock)clock).Step(SliceRules.BarracksBuildSeconds+1);gateway.Advance();Invoke("Refresh");
    Check(visual.RightState==ParcelBuildingState.BUILT&&right.Built.activeSelf,"Built camp/barracks presentation failed");result.ui_build_barracks=true;
    Pick("sawmill",left.Target);Pick("barracks",right.Target);result.existing_presenter_picking=true;
    var final=gateway.Snapshot();var reloaded=new LocalGateway(clock,new FileStateStore(store.PathName)).Snapshot();
    Check(final.Revision==reloaded.Revision&&final.BastionLevel==reloaded.BastionLevel&&final.SawmillLevel==reloaded.SawmillLevel&&final.BarracksLevel==reloaded.BarracksLevel&&final.Resources.Wood==reloaded.Resources.Wood&&final.Resources.Stone==reloaded.Resources.Stone,"Exact FileStateStore reload mismatch");
    result.persistence=true;result.revision=final.Revision;result.bastion=final.BastionLevel;result.sawmill=final.SawmillLevel;result.barracks=final.BarracksLevel;result.status="BOUNDED_EXPERIMENTAL_UI_I_II_AND_PERSISTENCE_PASS_NOT_FULL_RELEASE_QA";
   }catch(Exception e){result.status="FAIL";result.error=e.ToString();Debug.LogError(e);}
   string dir=Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..","docs/evidence/valoria-layered-construction-20261016/playable-entry-v1"));Directory.CreateDirectory(dir);
   File.WriteAllText(Path.Combine(dir,"gameplay-playmode.json"),JsonUtility.ToJson(result,true));
   if(result.status!="FAIL"){
    // Genuine playmode geometry capture; overlay HUD is verified through actual UI
    // commands above, but is not composited into this Camera.Render evidence.
    var view=UnityEngine.Object.FindFirstObjectByType<ValoriaUprightViewBridge>();view.Sync();var c=view.RenderCamera;
    var prev=c.targetTexture;var active=RenderTexture.active;float aspect=c.aspect;
    var rt=new RenderTexture(1536,1024,24){antiAliasing=4};var tex=new Texture2D(1536,1024,TextureFormat.RGB24,false);
    try{c.aspect=1.5f;c.targetTexture=rt;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1536,1024),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(dir,"playmode-built-i-ii-geometry.png"),tex.EncodeToPNG());}
    finally{c.targetTexture=prev;c.aspect=aspect;RenderTexture.active=active;rt.Release();Destroy(rt);Destroy(tex);}
   }
   yield return new WaitForSecondsRealtime(2);
#if UNITY_EDITOR
   Debug.Log("VALORIA_ENTRY_PLAYMODE_PROBE_COMPLETE "+result.status);UnityEditor.EditorApplication.Exit(result.status=="FAIL"?1:0);
#endif
  }
 }
}
