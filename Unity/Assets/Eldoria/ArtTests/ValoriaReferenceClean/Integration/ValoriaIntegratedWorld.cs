using System;
using System.Collections;
using System.Linq;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Presentation;
using Eldoria.UI.RoyalGold;
using Eldoria.World4XPrototype;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.ReferenceClean {
 // One host, one saved session, one input driver. Imported modules own no simulation.
 public sealed class ValoriaIntegratedWorld : MonoBehaviour,IPlayableSceneHost,IRoyalNavigation,IRoyalSession {
  public GameObject HudPrefab,WorldRoot;
  public Camera WorldCamera;
  public ZoneOneStrategicCamera WorldNavigation;
  public Vector3 WorldHome=new Vector3(982,0,24);
  public RoyalHUD Hud {get;private set;}
  public bool InWorld {get;private set;}
  public bool Ready {get;private set;}
  ValoriaReferenceCleanGameplay city; ExistingSliceBinding binding; ZoneOneGatewayHost world;
  ICommandGateway boundGateway; Canvas legacyCanvas; GraphicRaycaster legacyRaycaster;
  public Camera PlayerCamera=>InWorld?WorldCamera:city.PlayerCamera;
  IEnumerator Start(){
   city=GetComponent<ValoriaReferenceCleanGameplay>();
   yield return null;
   if(city==null||!city.Ready||HudPrefab==null||WorldRoot==null||WorldCamera==null){Debug.LogError("VALORIA_INTEGRATION_MISSING_BINDING");yield break;}
   var legacy=GameObject.Find("Eldoria HUD");
   legacyCanvas=legacy.GetComponent<Canvas>();legacyRaycaster=legacy.GetComponent<GraphicRaycaster>();
   Hud=Instantiate(HudPrefab).GetComponent<RoyalHUD>();Hud.name="Valoria Royal Gold HUD";
   BindSession();
   // Keep the original presenter running: it owns Advance, scene state and gestures.
   // Disable only its render/raycast surfaces after the replacement is bound.
   legacyCanvas.enabled=false;legacyRaycaster.enabled=false;
   city.Presenter.SceneHost=this;city.Presenter.ExternalSelection=Select;
   city.Presenter.ExternalPointerBlocked=RoyalInputGate.Blocks;
   WorldNavigation.enabled=false;WorldCamera.enabled=false;WorldRoot.SetActive(false);
   Ready=true;
  }
  void BindSession(){boundGateway=city.Gateway;binding=new ExistingSliceBinding(boundGateway,city.Clock);world=new ZoneOneGatewayHost(boundGateway,()=>SetWorld(false),()=>city.Clock.UtcTicks);Hud.Bind(this,this);}
  void LateUpdate(){if(Ready&&!ReferenceEquals(boundGateway,city.Gateway))BindSession();}
  void OnDestroy(){if(city!=null&&city.Presenter!=null){city.Presenter.SceneHost=city;city.Presenter.ExternalSelection=null;city.Presenter.ExternalPointerBlocked=null;}if(legacyCanvas)legacyCanvas.enabled=true;if(legacyRaycaster)legacyRaycaster.enabled=true;if(Hud)Destroy(Hud.gameObject);}
  public bool Select(string id){if(id=="gate"){SetWorld(true);return true;}if(id=="valoria-map-city"){SetWorld(false);return true;}Hud.SelectBuilding(id);return true;}
  public void SetWorld(bool open){
   if(!Ready)return;
   Hud.SelectBuilding("");Hud.Modal.Hide();city.Presenter.ClosePanel();
   InWorld=open;WorldRoot.SetActive(open);city.PlayerCamera.enabled=!open;WorldCamera.enabled=open;
   if(open){WorldNavigation.PanTo(WorldHome);WorldNavigation.SetZoomLevel(1,true);}
   Hud.Refresh();
  }
  public bool CanOpen(RoyalSection section)=>section==RoyalSection.World;
  public void Open(RoyalSection section){if(section==RoyalSection.World)SetWorld(!InWorld);}
  public void Navigate(string target)=>SetWorld(target=="Frontier");
  public void Apply(PlayerState state)=>city.Apply(state);
  public void Home(){if(InWorld){WorldNavigation.PanTo(WorldHome);WorldNavigation.SetZoomLevel(1,true);}else city.Home();}
  public void Pan(Vector2 delta){if(!InWorld){city.Pan(delta);return;}var p=new Vector2(Screen.width*.5f,Screen.height*.5f);if(WorldNavigation.GroundPoint(p,out var a)&&WorldNavigation.GroundPoint(p-delta,out var b))WorldNavigation.PanTo(WorldNavigation.target+b-a);}
  public void Zoom(float delta){if(!InWorld){city.Zoom(delta);return;}WorldNavigation.ZoomAt(new Vector2(Screen.width*.5f,Screen.height*.5f),Mathf.Exp(delta*.04f),true);}
  public void Focus(GameObject target){if(InWorld)WorldNavigation.PanTo(target.transform.position);else city.Focus(target);}
  public string TargetName(string id)=>InWorld?"Zone1 target "+id:city.TargetName(id);
  public WorldHotspot Pick(Vector2 point){
   if(!InWorld)return city.Pick(point);
   return Physics.RaycastAll(PlayerCamera.ScreenPointToRay(point),900,1<<8).OrderBy(h=>h.distance).Select(h=>h.collider.GetComponent<WorldHotspot>()).FirstOrDefault(h=>h!=null&&h.transform.IsChildOf(WorldRoot.transform));
  }
  public RoyalSnapshot Read(string selected,bool english){
   var data=binding.Read(selected,english);if(InWorld)data.Realm=english?"ZONE 1 · VALORIA":"ZONA 1 · VALORIA";
   if(selected=="forest-valoria"||selected=="quarry-valoria"||selected=="corrupt-scout"||selected=="engendro-valoria"||selected=="old-watch-ruin"){
    var action=world.Inspect(selected);var s=boundGateway.Snapshot();
    string name=selected=="forest-valoria"?"Bosque de Valoria":selected=="quarry-valoria"?"Cantera de Valoria":selected=="corrupt-scout"?"Ruta corrupta":selected=="engendro-valoria"?"Engendro":"Atalaya en ruinas";
    data.Building=new RoyalBuilding{Id=selected,Name=name,Function=data.March,State=action.Label,Requirements=action.Enabled?"":"Consulta el estado de la marcha y los objetivos.",Action=string.IsNullOrEmpty(action.Kind)?null:new RoyalAction(action.Kind,selected,action.Label,"",s.Revision,action.Enabled)};
   }
   return data;
  }
  public RoyalResult Submit(RoyalAction offer){var result=binding.Submit(offer);city.Presenter.RefreshState();return result;}
 }
}
