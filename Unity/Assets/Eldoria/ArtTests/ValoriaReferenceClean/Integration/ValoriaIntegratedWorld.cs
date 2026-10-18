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
using Object=UnityEngine.Object;

namespace Eldoria.ReferenceClean {
 // One host and saved session. Every entry point uses the same World access rule.
 public sealed class ValoriaIntegratedWorld : MonoBehaviour,IPlayableSceneHost,IRoyalNavigation,IRoyalSession {
  public GameObject HudPrefab,WorldRoot,LabelPrefab,WorldBastionOne,WorldBastionTwo,WorldSawmill;
  public ValoriaJourneyGuidance Guidance {get;private set;}
  public Camera WorldCamera;
  public ZoneOneStrategicCamera WorldNavigation;
  public Vector3 WorldHome=new Vector3(982,0,24);
  public RoyalHUD Hud {get;private set;}
  public bool InWorld {get;private set;}
  public bool Ready {get;private set;}
  public bool English=>englishInterface;
  public string Selected=>Hud?Hud.SelectedBuildingId:"";
  public ValoriaReferenceCleanGameplay City=>city;
  ValoriaReferenceCleanGameplay city; ExistingSliceBinding binding; ZoneOneGatewayHost world;
  ICommandGateway boundGateway; Canvas legacyCanvas; GraphicRaycaster legacyRaycaster;
  bool cityFog,englishInterface,worldVisited; int layoutWidth,layoutHeight,focusRequest,observedFocusStage;
  float nextVisualSync;
  Rect observedVisibleArea;
  WorldHotspot[] targets;
  public Camera PlayerCamera=>InWorld?WorldCamera:city.PlayerCamera;
  IEnumerator Start(){
   city=GetComponent<ValoriaReferenceCleanGameplay>();
   yield return null;
   if(city==null||!city.Ready||HudPrefab==null||WorldRoot==null||WorldCamera==null){Debug.LogError("VALORIA_INTEGRATION_MISSING_BINDING");yield break;}
   var legacy=GameObject.Find("Eldoria HUD");
   if(!legacy){Debug.LogError("VALORIA_INTEGRATION_MISSING_PRESENTER_HUD");yield break;}
   legacyCanvas=legacy.GetComponent<Canvas>();legacyRaycaster=legacy.GetComponent<GraphicRaycaster>();
   cityFog=RenderSettings.fog;
   Hud=Instantiate(HudPrefab).GetComponent<RoyalHUD>();Hud.name="Valoria Royal Gold HUD";
   BindSession();Hud.SelectionChanged+=SelectionChanged;
   // Keep the original presenter ticking the authoritative gateway and gestures.
   legacyCanvas.enabled=false;legacyRaycaster.enabled=false;
   city.Presenter.SceneHost=this;city.Presenter.ExternalSelection=Select;
   city.Presenter.ExternalPointerBlocked=RoyalInputGate.Blocks;
   WorldNavigation.enabled=false;WorldCamera.enabled=false;WorldRoot.SetActive(false);
   WorldNavigation.regionalZoom=32;WorldNavigation.localZoom=14;
   targets=Object.FindObjectsByType<WorldHotspot>(FindObjectsInactive.Include,FindObjectsSortMode.None);
   Ready=true;Guidance=gameObject.AddComponent<ValoriaJourneyGuidance>();Guidance.Initialize(this,city);
   gameObject.AddComponent<ValoriaMarchPresentation>().Initialize(this);
   gameObject.AddComponent<ValoriaPlaytestTelemetry>().Initialize(this);
  }
  void BindSession(){boundGateway=city.Gateway;binding=new ExistingSliceBinding(boundGateway,city.Clock);world=new ZoneOneGatewayHost(boundGateway,()=>SetWorld(false),()=>city.Clock.UtcTicks);Hud.Bind(this,this);}
  public void RefreshNavigation(){if(Hud==null)return;foreach(var item in Hud.Navigation.Items)if(item.Section==RoyalSection.World)item.Label.Put(InWorld?L("Ciudad","City"):L("Mundo","World"));}
  void LateUpdate(){
   if(!Ready)return;
   if(!ReferenceEquals(boundGateway,city.Gateway))BindSession();
   RefreshNavigation();if(Time.unscaledTime>=nextVisualSync){
    nextVisualSync=Time.unscaledTime+.25f;SyncWorldCity();
    if(!string.IsNullOrEmpty(Selected)){
     var area=Hud.GetVisibleSceneViewport();
     if(Mathf.Abs(area.xMin-observedVisibleArea.xMin)+Mathf.Abs(area.yMin-observedVisibleArea.yMin)+Mathf.Abs(area.xMax-observedVisibleArea.xMax)+Mathf.Abs(area.yMax-observedVisibleArea.yMax)>.005f||FocusStage(Selected)!=observedFocusStage){observedVisibleArea=area;SelectionChanged(Selected);}
    }
   }
   if(layoutWidth!=Screen.width||layoutHeight!=Screen.height){
    layoutWidth=Screen.width;layoutHeight=Screen.height;var scaler=Hud.GetComponent<RoyalScaler>();
    scaler.referenceResolution=new Vector2(layoutWidth<layoutHeight?800:1600,900);scaler.matchWidthOrHeight=layoutWidth<layoutHeight?0:1;
    SelectionChanged(Selected);
   }
  }
  void SyncWorldCity(){var state=city.Gateway.Snapshot();if(WorldBastionOne)WorldBastionOne.SetActive(state.BastionLevel<2);if(WorldBastionTwo)WorldBastionTwo.SetActive(state.BastionLevel>=2);if(WorldSawmill)WorldSawmill.SetActive(state.SawmillLevel>0);}
  void OnDestroy(){if(Hud)Hud.SelectionChanged-=SelectionChanged;if(city!=null&&city.Presenter!=null){city.Presenter.SceneHost=city;city.Presenter.ExternalSelection=null;city.Presenter.ExternalPointerBlocked=null;}if(legacyCanvas)legacyCanvas.enabled=true;if(legacyRaycaster)legacyRaycaster.enabled=true;if(Hud)Destroy(Hud.gameObject);}
  public static bool CanEnterWorld(PlayerState s)=>s!=null&&(s.SawmillLevel>0||s.Resources.Wood<SliceRules.SawmillWoodCost);
  public bool Select(string id){
   if(!Ready)return false;
   if(id=="gate")return TrySetWorld(true);
   if(id=="valoria-map-city")return TrySetWorld(false);
   if(IsWorldTarget(id)&&!TrySetWorld(true))return false;
   Hud.SelectBuilding(id);return true;
  }
  public static bool IsWorldTarget(string id)=>id=="forest-valoria"||id=="quarry-valoria"||id=="corrupt-scout"||id=="engendro-valoria"||id=="old-watch-ruin";
  public void SetWorld(bool open){TrySetWorld(open);}
  public bool TrySetWorld(bool open){
   if(!Ready)return false;
   if(open&&!CanEnterWorld(city.Gateway.Snapshot())){
    Hud.Feedback.Put(L("Construye primero el aserradero. Pulsa «Ir al objetivo» para encontrar la parcela.","Build the sawmill first. Choose Go to objective to find its plot."));
    Hud.SelectBuilding("sawmill");return false;
   }
   if(InWorld==open)return true;
   Hud.SelectBuilding("");Hud.Modal.Hide();city.Presenter.ClosePanel();
   InWorld=open;WorldRoot.SetActive(open);city.PlayerCamera.enabled=!open;WorldCamera.enabled=open;
   RenderSettings.fog=open?false:cityFog;
   if(open&&!worldVisited){WorldNavigation.PanTo(WorldHome);WorldNavigation.SetZoomLevel(1,true);worldVisited=true;}
   if(open)WorldNavigation.FitToAuthoredBounds();
   SyncWorldCity();Hud.Refresh();RefreshNavigation();Guidance?.Refresh();return true;
  }
  public bool CanOpen(RoyalSection section){
   if(city==null||city.Gateway==null)return false;var s=city.Gateway.Snapshot();
   if(section==RoyalSection.World)return InWorld||CanEnterWorld(s);
   if(section==RoyalSection.Army)return s.BastionLevel>=2&&s.BarracksLevel>0;
   if(section==RoyalSection.Bastion)return s.BastionLevel==1&&s.JourneyComplete;
   return false;
  }
  public void Open(RoyalSection section){if(section==RoyalSection.World)TrySetWorld(!InWorld);}
  public void Navigate(string target){TrySetWorld(target=="Frontier");}
  public void Apply(PlayerState state)=>city.Apply(state);
  public void Home(){if(InWorld){WorldNavigation.PanTo(WorldHome);WorldNavigation.SetZoomLevel(1,true);}else city.Home();}
  public void Pan(Vector2 delta){if(!InWorld){city.Pan(delta);return;}var p=new Vector2(Screen.width*.5f,Screen.height*.5f);if(WorldNavigation.GroundPoint(p,out var a)&&WorldNavigation.GroundPoint(p-delta,out var b))WorldNavigation.PanTo(WorldNavigation.target+b-a);}
  public void Zoom(float delta){if(!InWorld){city.Zoom(delta);return;}WorldNavigation.ZoomAt(new Vector2(Screen.width*.5f,Screen.height*.5f),Mathf.Exp(delta*.04f),true);}
  public WorldHotspot FindTarget(string id)=>(targets??Object.FindObjectsByType<WorldHotspot>(FindObjectsInactive.Include,FindObjectsSortMode.None)).FirstOrDefault(x=>x.Id==id&&(IsWorldTarget(id)||id=="valoria-map-city"?x.transform.IsChildOf(WorldRoot.transform):!x.transform.IsChildOf(WorldRoot.transform)));
  void SelectionChanged(string id){int version=++focusRequest;if(!Ready||string.IsNullOrEmpty(id))return;observedFocusStage=FocusStage(id);StartCoroutine(FocusAfterLayout(id,version));}
  int FocusStage(string id){
   // These are the visual states already applied to the real scene. A completed
   // model can need a different frame even when the landscape HUD rect is unchanged.
   // Camera movement alone never changes this key, so manual panning stays put.
   if(id=="bastion")return city.BastionTwo&&city.BastionTwo.activeSelf?2:1;
   if(city.Parcels){if(id=="sawmill")return (int)city.Parcels.LeftState;if(id=="barracks"||id=="march-main")return (int)city.Parcels.RightState;}
   return 0;
  }
  IEnumerator FocusAfterLayout(string id,int version){yield return null;if(version!=focusRequest||id!=Selected)yield break;Canvas.ForceUpdateCanvases();var target=FindTarget(id=="march-main"?"barracks":id);if(target)Focus(target.gameObject);}
  public void Focus(GameObject target){
   if(!target||!Ready||!Hud)return;
   var camera=PlayerCamera;var safe=Hud.GetVisibleSceneViewport();
   // An occluded layout is a real failure for the native probe, not a rectangle to repair by clamping.
   if(!camera||safe.width<=0||safe.height<=0)return;
   var points=FocusPoints(target);
   if(!ProjectFocus(camera,points,out var projected))return;
   if(camera.orthographic){
    float ratio=Mathf.Max(projected.width/safe.width,projected.height/safe.height);
    if(ratio>1){
     // Leave a small breathing margin only when zooming out is actually needed.
     float size=camera.orthographicSize*ratio*1.04f;
     if(InWorld)WorldNavigation.RestoreView(WorldNavigation.target,Mathf.Min(size,WorldNavigation.globalZoom));
     else city.Controls.ZoomBy(size-camera.orthographicSize);
     if(!ProjectFocus(camera,points,out projected))return;
    }
   }
   // Use the projected model envelope, including the roof, rather than the ground hotspot centre.
   // Two rays at the same height retain the existing ground-plane camera movement and pitch.
   var from=camera.ViewportPointToRay(new Vector3(projected.center.x,projected.center.y,0));
   var to=camera.ViewportPointToRay(new Vector3(safe.center.x,safe.center.y,0));
   var plane=new Plane(Vector3.up,points[0]);
   if(plane.Raycast(from,out var a)&&plane.Raycast(to,out var b)){
    var offset=from.GetPoint(a)-to.GetPoint(b);
    if(InWorld)WorldNavigation.PanTo(WorldNavigation.target+offset);
    else{camera.transform.position+=offset;city.Controls.FitToTerrain();}
   }
  }
  /// <summary>Checks every visible model-bound corner against the actual unoccluded UI viewport.</summary>
  public bool IsTargetFramed(GameObject target,out Rect projected,out Rect visible){
   projected=default;visible=Hud?Hud.GetVisibleSceneViewport():default;
   if(!target||!Ready||!PlayerCamera)return false;
   bool inFront=ProjectFocus(PlayerCamera,FocusPoints(target),out projected);
   return inFront&&visible.width>0&&visible.height>0&&projected.xMin>=visible.xMin&&projected.xMax<=visible.xMax&&projected.yMin>=visible.yMin&&projected.yMax<=visible.yMax;
  }
  System.Collections.Generic.List<Vector3> FocusPoints(GameObject target){
   var points=new System.Collections.Generic.List<Vector3>();var spot=target.GetComponent<WorldHotspot>();string id=spot?spot.Id:"";
   if(!target.transform.IsChildOf(WorldRoot.transform)){
    if(id=="bastion"){AddFocusRenderers(city.BastionOne,points);AddFocusRenderers(city.BastionTwo,points);}
    else if((id=="sawmill"||id=="barracks")&&city.Parcels){
     var p=city.Parcels;bool left=id=="sawmill";
     AddFocusRenderers(left?p.LeftBuilt:p.RightBuilt,points);
     AddFocusRenderers(left?p.LeftConstruction:p.RightConstruction,points);
     AddFocusRenderers(left?p.LeftAvailable:p.RightAvailable,points);
    }
   }else{
    if(id=="valoria-map-city"){
     AddFocusRenderers(WorldBastionOne,points);AddFocusRenderers(WorldBastionTwo,points);AddFocusRenderers(WorldSawmill,points);
    }else{
     var authored=WorldRoot.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name=="Zone1 authored "+id||t.name=="Zone1 hostile camp "+id);
     if(authored)AddFocusRenderers(authored.gameObject,points);
     if(id=="engendro-valoria"){
      var rear=WorldRoot.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name=="Zone1 hostile camp rear shelter");
      if(rear)AddFocusRenderers(rear.gameObject,points);
     }
    }
    if(points.Count==0){
     string name=id=="valoria-map-city"?"Existing medieval player city":id=="forest-valoria"?"Wood resource existing timber stock":id=="quarry-valoria"?"Existing Holotna rock.001":id=="corrupt-scout"?"Enemy camp provisional existing canvas":id=="engendro-valoria"?"Enemy camp existing shelter":id=="old-watch-ruin"?"Recovered neutral ruin Arch_Gothic.fbx":"";
     var retained=WorldRoot.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name==name);
     if(retained)AddFocusRenderers(retained.gameObject,points);
    }
   }
   if(points.Count==0)AddFocusRenderers(target,points);
   if(points.Count==0){
    var collider=target.GetComponent<Collider>();
    if(collider is BoxCollider box)AddFocusBox(new Bounds(box.center,box.size),box.transform.localToWorldMatrix,points);
    else if(collider)AddFocusBox(collider.bounds,Matrix4x4.identity,points);
    else points.Add(target.transform.position);
   }
   return points;
  }
  static void AddFocusRenderers(GameObject root,System.Collections.Generic.List<Vector3> points){
   if(!root||!root.activeInHierarchy)return;
   foreach(var renderer in root.GetComponentsInChildren<Renderer>())
    if(renderer.enabled&&!renderer.forceRenderingOff&&(renderer is MeshRenderer||renderer is SkinnedMeshRenderer))
     AddFocusBox(renderer.localBounds,renderer.localToWorldMatrix,points);
  }
  static void AddFocusBox(Bounds bounds,Matrix4x4 matrix,System.Collections.Generic.List<Vector3> points){
   var min=bounds.min;var max=bounds.max;
   for(int i=0;i<8;i++)points.Add(matrix.MultiplyPoint3x4(new Vector3((i&1)==0?min.x:max.x,(i&2)==0?min.y:max.y,(i&4)==0?min.z:max.z)));
  }
  static bool ProjectFocus(Camera camera,System.Collections.Generic.List<Vector3> points,out Rect projected){
   float xmin=float.PositiveInfinity,ymin=xmin,xmax=float.NegativeInfinity,ymax=xmax;bool inFront=points.Count>0;
   foreach(var point in points){var p=camera.WorldToViewportPoint(point);inFront&=p.z>=camera.nearClipPlane&&p.z<=camera.farClipPlane;xmin=Mathf.Min(xmin,p.x);xmax=Mathf.Max(xmax,p.x);ymin=Mathf.Min(ymin,p.y);ymax=Mathf.Max(ymax,p.y);}
   projected=points.Count>0?Rect.MinMaxRect(xmin,ymin,xmax,ymax):default;return inFront;
  }
  public string TargetName(string id)=>ValoriaJourneyGuidance.Name(id,englishInterface);
  public WorldHotspot Pick(Vector2 point){
   if(!InWorld)return city.Pick(point);
   return Physics.RaycastAll(PlayerCamera.ScreenPointToRay(point),900,1<<8).OrderBy(h=>h.distance).Select(h=>h.collider.GetComponent<WorldHotspot>()).FirstOrDefault(h=>h!=null&&h.transform.IsChildOf(WorldRoot.transform));
  }
  public RoyalSnapshot Read(string selected,bool english){
   englishInterface=english;var data=binding.Read(selected,english);if(InWorld)data.Realm=L("ZONA 1","ZONE 1");
   if(IsWorldTarget(selected)){
    var action=world.Inspect(selected);var s=boundGateway.Snapshot();bool active=s.March.Phase!="idle",thisMarch=active&&s.March.TargetId==selected;
    var b=new RoyalBuilding{Id=selected,Name=TargetName(selected),Function=DestinationSummary(selected,s),State=action.Label};
    if(thisMarch){b.OperationActive=true;b.State=MarchStatus(s);b.Time=RoyalFormatting.Remaining(s.March.PhaseEndsUtcTicks,city.Clock.UtcTicks,english);b.Result=s.March.Phase=="returning"?Reward(s.March.PendingWood,s.March.PendingStone)+L(" al llegar a Valoria"," on arrival at Valoria"):ExpectedReward(selected,s);b.Progress=(float)ZoneOneGatewayHost.TravelFraction(city.Clock.UtcTicks,s.March.PhaseEndsUtcTicks,s.March.Phase=="gathering"?SliceRules.GatherSeconds:SliceRules.TravelSeconds);}
    else if(active)b.Requirements=L("Tu expedición está en ","Your expedition is at ")+TargetName(s.March.TargetId)+". "+L("Espera a su regreso.","Wait for its return.");
    else if(selected=="engendro-valoria"&&!action.Enabled&&!s.EngendroDefeated)b.Requirements=L("Necesitas Bastión II, cuartel y una marcha confirmada.","Requires Bastion II, barracks and a confirmed march.");
    else if(!action.Enabled&&s.Available.Total<1)b.Requirements=L("No hay tropas disponibles para salir.","No troops are available to depart.");
    if(!thisMarch&&!string.IsNullOrEmpty(action.Kind))b.Action=new RoyalAction(action.Kind,selected,action.Label,b.Requirements,s.Revision,action.Enabled);
    if(!active&&(selected=="forest-valoria"||selected=="quarry-valoria"))b.Time=L("Ida + trabajo + regreso: ","Travel + work + return: ")+(2*SliceRules.TravelSeconds+SliceRules.GatherSeconds)+" s";
    data.Building=b;
   }
   return data;
  }
  public string MarchStatus(PlayerState s){string phase=s.March.Phase;return phase=="outbound"?L("En camino a ","Travelling to ")+TargetName(s.March.TargetId):phase=="gathering"?L("Recolectando en ","Gathering at ")+TargetName(s.March.TargetId):phase=="returning"?L("Regresando a Valoria","Returning to Valoria"):L("Expedición disponible","Expedition ready");}
  public string Reward(int wood,int stone){var parts=new System.Collections.Generic.List<string>();if(wood>0)parts.Add("+"+wood+L(" madera"," wood"));if(stone>0)parts.Add("+"+stone+L(" piedra"," stone"));return string.Join(" · ",parts);}
  string ExpectedReward(string id,PlayerState s){if(id=="forest-valoria")return Reward(Math.Min(s.ForestRemaining,SliceRules.ForestLoad),0);if(id=="quarry-valoria")return Reward(0,Math.Min(s.QuarryRemaining,SliceRules.QuarryLoad));return L("El botín vuelve con las tropas.","Loot returns with the troops.");}
  string DestinationSummary(string id,PlayerState s){
   if(id=="forest-valoria")return L("Madera para reconstruir Valoria.\nCada expedición: ","Timber to rebuild Valoria.\nPer expedition: ")+ExpectedReward(id,s);
   if(id=="quarry-valoria")return L("Piedra para edificios y defensas.\nCada expedición: ","Stone for buildings and defences.\nPer expedition: ")+ExpectedReward(id,s);
   if(id=="old-watch-ruin")return L("Una antigua atalaya junto al camino. Por ahora sirve como referencia del terreno.","An old watchtower beside the road. It currently serves as a landmark.");
   if(id=="corrupt-scout")return s.ScoutDefeated?L("La ruta está despejada. Tus tropas pueden volver a Valoria.","The route is clear. Your troops can return to Valoria."):L("Aldric y tus arqueros deben despejar este acceso. Botín si vencen: +80 madera · +70 piedra.","Aldric and your archers must clear this route. Victory loot: +80 wood · +70 stone.");
   return s.EngendroDefeated?L("El Engendro ha caído. Bastión II está asegurado.","The Spawn has fallen. Bastion II is secured."):L("El enemigo del capítulo II. Confirma tu expedición antes de salir. Botín si vence: +120 madera · +100 piedra.","Chapter II's enemy. Confirm your expedition before departing. Victory loot: +120 wood · +100 stone.");
  }
  public RoyalResult Submit(RoyalAction offer){var result=binding.Submit(offer);city.Presenter.RefreshState();return result;}
  string L(string es,string en)=>englishInterface?en:es;
 }
}
