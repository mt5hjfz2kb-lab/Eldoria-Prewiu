using System;
using System.Linq;
using Eldoria.Domain;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.ReferenceClean {
 // Opt-in observations for the local browser playtest. No command, clock or save setters.
 public sealed class ValoriaPlaytestTelemetry:MonoBehaviour {
  ValoriaIntegratedWorld host;bool enabledForPlaytest;float next;long frameSequence;
  public void Initialize(ValoriaIntegratedWorld value){host=value;enabledForPlaytest=(UnityEngine.Application.absoluteURL??"").Contains("eldoriaPlaytest=1");}
  void LateUpdate(){if(!enabledForPlaytest||!host||!host.Ready||Time.unscaledTime<next)return;next=Time.unscaledTime+.35f;Emit();}
  void Emit(){
   var s=host.City.Gateway.Snapshot();var hud=host.Hud;Canvas.ForceUpdateCanvases();
   var f=new Frame{ready=true,width=Screen.width,height=Screen.height,inWorld=host.InWorld,selected=host.Selected,objective=SliceRules.CurrentObjectiveKey(s),narrative=hud.NarrativeOpen,revision=s.Revision,wood=s.Resources.Wood,stone=s.Resources.Stone,sawmill=s.SawmillLevel,barracks=s.BarracksLevel,bastion=s.BastionLevel,trained=s.ChapterProgress.TrainedArchers,configured=s.MarchConfigured,scout=s.ScoutDefeated,engendro=s.EngendroDefeated,march=s.March.Phase,target=s.March.TargetId,busy=s.BuildingCompletesUtcTicks>0||s.RecruitmentCompletesUtcTicks>0||s.BastionUpgradeCompletesUtcTicks>0,save=JsonUtility.ToJson(s),clock=host.City.Clock.GetType().Name,timeScale=Time.timeScale,utc=DateTime.UtcNow.ToString("O")};
   // Native V2 buttons are not the hidden V1 view controls. Report
   // only the actual visible hit targets so browser QA exercises the same
   // uGUI path a player taps. All actions still go through RoyalHUD.
   var v2=hud.GetComponent<RoyalGoldV2Hud>();
   f.controls=new Controls{
    objective=v2?Point(v2.VisibleObjectiveButton,v2.ObjectiveVisible):Point(hud.Mission.Open),
    primary=v2?Point(v2.VisibleBuildingButton,v2.BuildingActionVisible):Point(hud.Building.Primary.Button),
    @continue=Point(hud.Modal.Extra),
    close=v2?Point(v2.VisibleBuildingCloseButton,v2.BuildingVisible):Point(hud.Building.Close)
   };
   var nav=hud.Navigation.Items.FirstOrDefault(x=>x.Section==RoyalSection.World);if(nav!=null)f.controls.world=Point(nav.Button);
   // Observe the independent building destination a player actually sees.
   // This opt-in diagnostic reads controls only; it exposes no command/save setter.
   var building=hud.GetComponent<RoyalGoldV3BuildingScreens>();
   if(building&&building.IsOpen){
    f.management=new Management{destination="Building:"+building.BuildingId,bounds=ScreenBounds(building.VisibleRoot)};
    f.controls.primary=Point(building.VisibleAction);
    f.controls.close=Point(building.VisibleClose);
    f.controls.objective.enabled=false;
    if(f.controls.world!=null)f.controls.world.enabled=false;
   }
   var management=hud.GetComponent<RoyalGoldV3ManagementScreens>();
   if(management&&management.IsOpen){
    f.management=new Management{destination=management.CurrentDestination.ToString(),quantity=management.SelectedQuantity,
     trainingSeconds=management.SelectedTrainingSeconds,bounds=ScreenBounds(management.VisibleRoot)};
    f.controls.primary=Point(management.VisibleAction);
    f.controls.close=Point(management.VisibleBack);
    f.controls.objective.enabled=false;
    if(f.controls.world!=null)f.controls.world.enabled=false;
   }
   f.camera=new View{x=host.PlayerCamera.transform.position.x,y=host.PlayerCamera.transform.position.y,z=host.PlayerCamera.transform.position.z,zoom=host.PlayerCamera.orthographicSize};
   var expedition=host.GetComponent<ValoriaMarchPresentation>();
   if(expedition){var p=host.PlayerCamera.WorldToScreenPoint(expedition.ObservedPosition);f.marchVisual=new Hotspot{id=expedition.ObservedPhase,x=p.x,y=p.y,visible=expedition.MarkerVisible};}
   if(!string.IsNullOrEmpty(host.Selected)){
    var selectedTarget=host.FindTarget(host.Selected=="march-main"?"barracks":host.Selected);
    if(selectedTarget){bool framed=host.IsTargetFramed(selectedTarget.gameObject,out var projected,out var visible);f.framing=new Framing{pass=framed,projected=new[]{projected.xMin,projected.yMin,projected.xMax,projected.yMax},visible=new[]{visible.xMin,visible.yMin,visible.xMax,visible.yMax}};}
   }
   f.hotspots=new[]{"gate","sawmill","barracks","bastion","valoria-map-city","forest-valoria","quarry-valoria","corrupt-scout","engendro-valoria"}.Select(id=>{
    var target=host.FindTarget(id);var point=target?host.PlayerCamera.WorldToScreenPoint(target.GetComponent<Collider>()?target.GetComponent<Collider>().bounds.center:target.transform.position):new Vector3(-1,-1,-1);
    return new Hotspot{id=id,x=point.x,y=point.y,visible=target&&target.gameObject.activeInHierarchy&&point.z>0&&host.PlayerCamera.pixelRect.Contains(point)};
   }).ToArray();
   // WebGL's native log buffer truncates long lines. Preserve the complete
   // observation/save as ordered, bounded ASCII chunks; no state is modified.
   var encoded=Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(f)));
   const int chunkSize=4000;int count=(encoded.Length+chunkSize-1)/chunkSize;long sequence=++frameSequence;
   for(int part=0;part<count;part++)Debug.Log("VALORIA_PLAYTEST_CHUNK "+sequence+" "+part+" "+count+" "+encoded.Substring(part*chunkSize,Math.Min(chunkSize,encoded.Length-part*chunkSize)));
  }
  static float[] ScreenBounds(RectTransform rect){
   var corners=new Vector3[4];rect.GetWorldCorners(corners);var canvas=rect.GetComponentInParent<Canvas>();
   var camera=canvas&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null;
   var points=corners.Select(p=>RectTransformUtility.WorldToScreenPoint(camera,p)).ToArray();
   return new[]{points.Min(p=>p.x)/Screen.width,points.Min(p=>p.y)/Screen.height,
    points.Max(p=>p.x)/Screen.width,points.Max(p=>p.y)/Screen.height};
  }
  [Serializable] class Management{public string destination;public int quantity,trainingSeconds;public float[] bounds;}
  static Control Point(Button button,bool onScreen=true){
   var value=new Control();if(!button)return value;
   value.enabled=onScreen&&button.isActiveAndEnabled&&button.gameObject.activeInHierarchy&&button.interactable;
   var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);var canvas=button.GetComponentInParent<Canvas>();
   var p=RectTransformUtility.WorldToScreenPoint(canvas&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null,(corners[0]+corners[2])*.5f);value.x=p.x;value.y=p.y;return value;
  }
  [Serializable] class Frame{public bool ready,inWorld,narrative,busy,configured,scout,engendro;public int width,height,wood,stone,sawmill,barracks,bastion,trained;public long revision;public string selected,objective,march,target,save,clock,utc;public float timeScale;public Controls controls;public View camera;public Hotspot[] hotspots;public Hotspot marchVisual;public Framing framing;public Management management;}
  [Serializable] class Framing{public bool pass;public float[] projected,visible;}
  [Serializable] class Controls{public Control objective,primary,@continue,close,world;}
  [Serializable] class Control{public float x,y;public bool enabled;}
  [Serializable] class Hotspot{public string id;public float x,y;public bool visible;}
  [Serializable] class View{public float x,y,z,zoom;}
 }
}
