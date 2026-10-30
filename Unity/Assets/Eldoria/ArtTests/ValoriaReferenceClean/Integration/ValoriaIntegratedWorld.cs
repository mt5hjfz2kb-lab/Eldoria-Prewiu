using System;
using System.Collections;
using System.Linq;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Presentation;
using Eldoria.SpawnlingBastion2;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using Eldoria.World4XPrototype;
using Eldoria.World4XCanon;
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
  bool cityFog,englishInterface; int layoutWidth,layoutHeight,focusRequest,observedFocusStage;
  float nextVisualSync;
  // Motion is measured in screen pixels, so easing is consistent in both camera modes.
  Vector2 unappliedDragPixels;
  const float DragResponse=20f;
  void ApplyCameraPan(Vector2 portion){
   if(!InWorld){city.Pan(portion);return;}
   var center=new Vector2(Screen.width*.5f,Screen.height*.5f);
   if(WorldNavigation.GroundPoint(center,out var from)&&
      WorldNavigation.GroundPoint(center-portion,out var to))
    WorldNavigation.PanTo(WorldNavigation.target+to-from);
  }
  void SmoothCameraDrag(){
   if(unappliedDragPixels.sqrMagnitude<.0025f){unappliedDragPixels=Vector2.zero;return;}
   var portion=unappliedDragPixels*(1f-Mathf.Exp(-DragResponse*Mathf.Min(.05f,Time.unscaledDeltaTime)));
   unappliedDragPixels-=portion;
   ApplyCameraPan(portion);
  }
  Rect observedVisibleArea;
  WorldHotspot[] targets;
  public Camera PlayerCamera=>InWorld?WorldCamera:city.PlayerCamera;
  Rect UIVisibleSceneViewport(){
   if(!Hud)return default;
   var v2=Hud.GetComponent<RoyalGoldV2Hud>();
   return v2?v2.GetFreeSceneViewport():Hud.GetVisibleSceneViewport();
  }
  IEnumerator Start(){
   city=GetComponent<ValoriaReferenceCleanGameplay>();
   yield return null;
   if(city==null||!city.Ready||HudPrefab==null||WorldRoot==null||WorldCamera==null){Debug.LogError("VALORIA_INTEGRATION_MISSING_BINDING");yield break;}
   var legacy=GameObject.Find("Eldoria HUD");
   if(!legacy){Debug.LogError("VALORIA_INTEGRATION_MISSING_PRESENTER_HUD");yield break;}
   legacyCanvas=legacy.GetComponent<Canvas>();legacyRaycaster=legacy.GetComponent<GraphicRaycaster>();
   cityFog=RenderSettings.fog;
   Hud=Instantiate(HudPrefab).GetComponent<RoyalHUD>();Hud.name="Valoria Royal Gold HUD";
   // The playtest Settings dialog requests a guarded backup-and-fresh-save from the real city host.
   Hud.ResetPlaytestProgress=()=>city.ResetPlaytestProgress();
   // One authoritative Royal Gold session; V2 alters presentation only.
   if(!Hud.GetComponent<RoyalGoldV2Hud>())Hud.gameObject.AddComponent<RoyalGoldV2Hud>();
   Hud.GetComponent<RoyalGoldV2Hud>().BuildingModelFor=id=>{
    if(id=="upgrade-barracks"||id=="march-main")id="barracks";
    var advanced=GetComponent<BastionIVVWorldPresentation>();
    var current=advanced?advanced.ModelFor(id):null;
    if(current&&current.activeInHierarchy)return current;
    if(id=="bastion")return boundGateway!=null&&boundGateway.Snapshot().BastionLevel>=2?city.BastionTwo:city.BastionOne;
    if(city.Parcels&&id=="sawmill")return city.Parcels.LeftBuilt;
    if(city.Parcels&&id=="barracks")return city.Parcels.RightBuilt;
    if(id=="granary")return Resources.Load<GameObject>("BastionIII/Granary/Granary");
    if(id=="stoneworks")return Resources.Load<GameObject>("BastionIII/Granary/StoneWorkshop");
    return null;
   };
   if(!Hud.GetComponent<RoyalGoldV2Narrative>())Hud.gameObject.AddComponent<RoyalGoldV2Narrative>();
   // V3 shares the one RoyalHUD and receives only read-only target/camera
   // projections and the exact existing IRoyalSession building snapshot.
   // No second Canvas, controller, timer, EventSystem or gameplay rules.
   var context=Hud.GetComponent<RoyalGoldV3ContextHud>();
   if(!context)context=Hud.gameObject.AddComponent<RoyalGoldV3ContextHud>();
   context.Source=Hud;
   context.FindAnchor=id=>{
     // Barracks upgrading is an action against the existing Barracks model,
     // never a separate full-screen or an imaginary world target.
     if(id=="upgrade-barracks")id="barracks";
    // Recruitment belongs visually to the built barracks, not the logical
    // parcel hotspot, which can lie outside the portrait camera frame.
    // Selection/raycast/authoritative gameplay still use FindTarget unchanged.
    if(!InWorld&&id=="barracks"&&city.Parcels!=null&&
       city.Parcels.RightBuilt&&city.Parcels.RightBuilt.activeInHierarchy)
      return city.Parcels.RightBuilt.transform;
    var spot=FindTarget(id);return spot?spot.transform:null;
   };
   context.ActiveCamera=()=>PlayerCamera;
   context.ReadBuilding=id=>Read(id,English).Building;
   if(!Hud.GetComponent<EldoriaV3VisualDirection>())
     Hud.gameObject.AddComponent<EldoriaV3VisualDirection>();
   BindSession();Hud.SelectionChanged+=SelectionChanged;
   // Keep the original presenter ticking the authoritative gateway and gestures.
   legacyCanvas.enabled=false;legacyRaycaster.enabled=false;
   city.Presenter.ExternalNavigationOwned=true;
   city.Presenter.SceneHost=this;city.Presenter.ExternalSelection=Select;
   city.Presenter.ExternalPointerBlocked=RoyalInputGate.Blocks;
   WorldNavigation.enabled=false;WorldCamera.enabled=false;WorldRoot.SetActive(false);
   // Use the canonical world scale from the first frame. The landscape adapter
   // owns whole-region coverage from actual terrain bounds, not a fixed 280 preset.
   WorldNavigation.localZoom=RegionOneCanonicalLandscape.NearZoom(WorldCamera.aspect);
   WorldNavigation.regionalZoom=RegionOneCanonicalLandscape.RegionalZoom(WorldCamera.aspect);
   gameObject.AddComponent<BastionIIIWorldPresentation>().Initialize(this);
   gameObject.AddComponent<BastionIVVWorldPresentation>().Initialize(this);
   targets=Object.FindObjectsByType<WorldHotspot>(FindObjectsInactive.Include,FindObjectsSortMode.None);
   // One modestly wider authored entry view (about 10%), without changing near limits or world scale.
   city.Controls.homeZoom=Mathf.Min(65f,city.PlayerCamera.orthographicSize*1.08f);
   city.Controls.ResetView();
   Ready=true;Guidance=gameObject.AddComponent<ValoriaJourneyGuidance>();Guidance.Initialize(this,city);
   gameObject.AddComponent<ValoriaMarchPresentation>().Initialize(this);
   gameObject.AddComponent<ValoriaPlaytestTelemetry>().Initialize(this);
  }
  void BindSession(){boundGateway=city.Gateway;binding=new ExistingSliceBinding(boundGateway,city.Clock);world=new ZoneOneGatewayHost(boundGateway,()=>SetWorld(false),()=>city.Clock.UtcTicks);Hud.Bind(this,this);
   var buildings=Hud.GetComponent<RoyalGoldV3BuildingScreens>();if(!buildings)buildings=Hud.gameObject.AddComponent<RoyalGoldV3BuildingScreens>();
   buildings.Initialize(Hud,boundGateway,id=>Read(id,English).Building,()=>{city.Presenter.RefreshState();Hud.Refresh();});
    // Rebind to the one canonical gateway after resets; never create another session.
    var management=Hud.GetComponent<RoyalGoldV3ManagementScreens>();
    if(!management)management=Hud.gameObject.AddComponent<RoyalGoldV3ManagementScreens>();
    management.Initialize(Hud,boundGateway,()=>city.Clock.UtcTicks,()=>city.Presenter.RefreshState());
    management.ReturnToCity=()=>{if(InWorld)TrySetWorld(false);};
    // The Camino de los Vigilantes and Mesa de Exploradores are independent
    // top-level destinations. They are NOT inside Crónicas/Códice; their two
    // access ribbons only appear after progression actually unlocks each.
    var retention=Hud.GetComponent<RoyalGoldV3EarlyRetentionScreens>();
    if(!retention)retention=Hud.gameObject.AddComponent<RoyalGoldV3EarlyRetentionScreens>();
    retention.Initialize(Hud,boundGateway,()=>city.Clock.UtcTicks,
      ()=>{Hud.Refresh();city.Presenter.RefreshState();});
    // The saved LocalGateway is replaced on ReloadForProbe/load. Reconnect the
    // existing terrain route/population service in the very same session
    // rebind, before the first post-reload hunting or gathering command.
    // This never writes a save, creates a second gateway or changes gameplay.
    if(Ready && WorldRoot){
      var resources=WorldRoot.GetComponentInChildren<Eldoria.World4XCanon.RegionOneResourcePlacement>(true);
      if(resources)resources.RefreshForZoom();
    }
   }
  public void RefreshNavigation(){if(Hud==null)return;foreach(var item in Hud.Navigation.Items)if(item.Section==RoyalSection.World)item.Label.Put(InWorld?L("Ciudad","City"):L("Mundo","World"));}
  void LateUpdate(){
   if(!Ready)return;
   SmoothCameraDrag();
   if(!ReferenceEquals(boundGateway,city.Gateway))BindSession();
   RefreshNavigation();if(Time.unscaledTime>=nextVisualSync){
    nextVisualSync=Time.unscaledTime+.25f;SyncWorldCity();
    if(!string.IsNullOrEmpty(Selected)){
     var area=UIVisibleSceneViewport();
     if(Mathf.Abs(area.xMin-observedVisibleArea.xMin)+Mathf.Abs(area.yMin-observedVisibleArea.yMin)+Mathf.Abs(area.xMax-observedVisibleArea.xMax)+Mathf.Abs(area.yMax-observedVisibleArea.yMax)>.005f||FocusStage(Selected)!=observedFocusStage){observedVisibleArea=area;SelectionChanged(Selected);}
    }
   }
   if(layoutWidth!=Screen.width||layoutHeight!=Screen.height){
    layoutWidth=Screen.width;layoutHeight=Screen.height;var scaler=Hud.GetComponent<RoyalScaler>();
    scaler.referenceResolution=new Vector2(layoutWidth<layoutHeight?800:1600,900);scaler.matchWidthOrHeight=layoutWidth<layoutHeight?0:1;
    SelectionChanged(Selected);
   }
  }
  void SyncWorldCity(){var state=city.Gateway.Snapshot();if(WorldBastionOne)WorldBastionOne.SetActive(state.BastionLevel<2);if(WorldBastionTwo)WorldBastionTwo.SetActive(state.BastionLevel>=2&&state.BastionLevel<5);if(WorldSawmill)WorldSawmill.SetActive(state.SawmillLevel>0);}
  void OnDestroy(){if(Hud)Hud.SelectionChanged-=SelectionChanged;if(city!=null&&city.Presenter!=null){city.Presenter.SceneHost=city;city.Presenter.ExternalSelection=null;city.Presenter.ExternalPointerBlocked=null;city.Presenter.ExternalNavigationOwned=false;}if(legacyCanvas)legacyCanvas.enabled=true;if(legacyRaycaster)legacyRaycaster.enabled=true;if(Hud)Destroy(Hud.gameObject);}
  public static bool CanEnterWorld(PlayerState s)=>s!=null&&(s.SawmillLevel>0||s.Resources.Wood<SliceRules.SawmillWoodCost);
  public bool Select(string id){
   if(!Ready)return false;
   if(id=="gate")return TrySetWorld(true);
   if(id=="valoria-map-city")return TrySetWorld(false);
   if(IsWorldTarget(id)&&!TrySetWorld(true))return false;
   Hud.SelectBuilding(id);return true;
  }
  public static bool IsWorldTarget(string id)=>LocalGateway.IsChapterThreeTarget(id)||(id!=null&&id.StartsWith("region1:",StringComparison.Ordinal))||id=="forest-valoria"||id=="quarry-valoria"||id=="corrupt-scout"||id=="engendro-valoria"||id=="old-watch-ruin";
  public void SetWorld(bool open){TrySetWorld(open);}
  public bool TrySetWorld(bool open){
   if(!Ready)return false;
   if(open&&!CanEnterWorld(city.Gateway.Snapshot())){
    Hud.Feedback.Put(L("Construye primero el aserradero. Pulsa «Ir al objetivo» para encontrar la parcela.","Build the sawmill first. Choose Go to objective to find its plot."));
    Hud.SelectBuilding("sawmill");return false;
   }
   if(InWorld==open)return true;
   Hud.SelectBuilding("");Hud.Modal.Hide();city.Presenter.ClosePanel();
   unappliedDragPixels=Vector2.zero;
   InWorld=open;WorldRoot.SetActive(open);city.PlayerCamera.enabled=!open;WorldCamera.enabled=open;
   if(!open)city.Controls.ResetView();
   RenderSettings.fog=open?false:cityFog;
   if(open){
    // Every visit starts in playable near view, including after panorama or a saved-session reload.
    // EnterStrategicView resets bearing and focal point; the near preset then overrides its
    // strategic zoom. One existing camera, one WorldRoot and one persistent gateway.
    // Match the canonical portrait near scale before the same-frame render,
    // rather than creating a visible 36 -> 64 camera jump after entering.
    WorldNavigation.localZoom=RegionOneCanonicalLandscape.NearZoom(WorldCamera.aspect);
    WorldNavigation.EnterStrategicView(WorldHome);
    WorldNavigation.SetZoomLevel(2,true);
    // Strategic bounds may clamp the anchor before changing presets;
    // bring the near camera back to the genuine Valoria start point.
    WorldNavigation.PanTo(WorldHome);
   }
   SyncWorldCity();Hud.Refresh();RefreshNavigation();Guidance?.Refresh();return true;
  }
  public bool CanOpen(RoyalSection section){
   if(city==null||city.Gateway==null)return false;var s=city.Gateway.Snapshot();
   if(section==RoyalSection.World)return InWorld||CanEnterWorld(s);
   if(section==RoyalSection.Inventory)return s.ChapterThree.Inventory.Count>0;
   if(section==RoyalSection.Army)return s.BastionLevel>=2&&s.BarracksLevel>0;
   if(section==RoyalSection.Bastion)return s.BastionLevel<=2&&s.JourneyComplete;
   return false;
  }
  public void Open(RoyalSection section){if(section==RoyalSection.World)TrySetWorld(!InWorld);else if(section==RoyalSection.Inventory){var s=boundGateway.Snapshot();Hud.ShowNarrative(L("ARCÓN","CHEST"),string.Join("\n\n",s.ChapterThree.Inventory.OrderBy(x=>x=="breach-manuscript"?0:1).Select(x=>x=="breach-manuscript"?L("Manuscrito de la Fisura · Misterio sin descifrar. No utilizable todavía.","Fissure Manuscript · An unsolved mystery. Not usable yet."):x=="treated-hide"?L("Piel curtida de ceniza · Material del Granero","Ash-treated hide · Granary material"):x=="iron-brace"?L("Abrazadera de hierro antiguo · Material del Aserradero","Ancient iron brace · Sawmill material"):L("Colmillo pétreo · Material de construcción","Stone tusk · Building material"))));}}
  public void Navigate(string target){TrySetWorld(target=="Frontier");}
  public void Apply(PlayerState state)=>city.Apply(state);
  public void Home(){
   // Do not replay old gesture momentum after an explicit home command.
   unappliedDragPixels=Vector2.zero;
   if(!InWorld){city.Home();return;}
   // The player's world home is Valoria in playable near view, not a forced
   // strategic zoom that contradicts the default when entering from the city.
   WorldNavigation.EnterStrategicView(WorldHome);
   WorldNavigation.SetZoomLevel(2,true);
   WorldNavigation.PanTo(WorldHome);
  }
  public void Pan(Vector2 delta){
   // React to each touch immediately; ease the remaining gesture rather than
   // teleporting the whole camera in a single frame. This keeps real input
   // responsive and preserves the existing native immediate-pan assertions.
   // Resume spikes cannot move more than 180 screen pixels.
   unappliedDragPixels=Vector2.ClampMagnitude(unappliedDragPixels+delta,180f);
   var leading=unappliedDragPixels*.38f;
   unappliedDragPixels-=leading;
   ApplyCameraPan(leading);
  }
  public void Zoom(float delta){
   // A deliberate pinch/wheel zoom takes priority over queued drag easing.
   unappliedDragPixels=Vector2.zero;
   if(!InWorld){city.Zoom(delta);return;}
   WorldNavigation.ZoomAt(new Vector2(Screen.width*.5f,Screen.height*.5f),Mathf.Exp(delta*.04f),true);
  }
  public WorldHotspot FindTarget(string id)=>id=="stoneworks"?GetComponentsInChildren<WorldHotspot>(true).FirstOrDefault(x=>x.Id==id):FindTargetExact(id=="upgrade-barracks"?"barracks":id);
  WorldHotspot FindTargetExact(string id)=>(id!=null&&id.StartsWith("region1:",StringComparison.Ordinal)?WorldRoot.GetComponentsInChildren<WorldHotspot>(true):(targets??Object.FindObjectsByType<WorldHotspot>(FindObjectsInactive.Include,FindObjectsSortMode.None))).FirstOrDefault(x=>x.Id==id&&(IsWorldTarget(id)||id=="valoria-map-city"?x.transform.IsChildOf(WorldRoot.transform):!x.transform.IsChildOf(WorldRoot.transform)));
  void SelectionChanged(string id){int version=++focusRequest;if(!Ready||string.IsNullOrEmpty(id))return;
    // A management destination is not a request to move the world camera.
    var management=Hud?Hud.GetComponent<RoyalGoldV3ManagementScreens>():null;
    if(management&&management.IsOpen)return;
    observedFocusStage=FocusStage(id);StartCoroutine(FocusAfterLayout(id,version));}
  int FocusStage(string id){
   // These are the visual states already applied to the real scene. A completed
   // model can need a different frame even when the landscape HUD rect is unchanged.
   // Camera movement alone never changes this key, so manual panning stays put.
   if(id=="bastion")return city.Gateway.Snapshot().BastionLevel;
   if(city.Parcels){if(id=="sawmill")return (int)city.Parcels.LeftState;if(id=="barracks"||id=="march-main")return (int)city.Parcels.RightState;}
   return 0;
  }
  IEnumerator FocusAfterLayout(string id,int version){yield return null;if(version!=focusRequest||id!=Selected)yield break;Canvas.ForceUpdateCanvases();var target=FindTarget(id=="march-main"?"barracks":id);if(target)Focus(target.gameObject);}
  public void Focus(GameObject target){
   if(!target||!Ready||!Hud)return;
   var camera=PlayerCamera;
   var safe=UIVisibleSceneViewport();
   if(!camera||safe.width<=.08f||safe.height<=.08f)return;
   var points=FocusPoints(target);
   if(!ProjectFocus(camera,points,out var projected)){
    // A selected world site can lie behind the current near-view camera.
    // Move to its real site before fitting; rejecting projection here made
    // distant quarry/forest actions impossible to reveal from the objective.
    if(!InWorld)return;
    WorldNavigation.PanTo(target.transform.position);
    if(!ProjectFocus(camera,points,out projected))return;
   }
   // Focus against the actual unobstructed HUD rectangle, not the whole screen.
   // Terrain-wide camera clamps can reverse the final adjustment when the
   // selected Bastion roof is close to the upper chapter panel.
   var padded=Rect.MinMaxRect(safe.xMin+.012f,safe.yMin+.012f,safe.xMax-.012f,safe.yMax-.012f);
   for(int attempt=0;attempt<5;attempt++){
    if(!ProjectFocus(camera,points,out projected))break;
    if(projected.xMin>=padded.xMin&&projected.xMax<=padded.xMax&&projected.yMin>=padded.yMin&&projected.yMax<=padded.yMax){var context=Hud.GetComponent<RoyalGoldV3ContextHud>();if(context)context.SynchronizeNow();return;}
    if(camera.orthographic){
     float required=Mathf.Max(projected.width/padded.width,projected.height/padded.height);
     if(required>1.001f){
      float size=camera.orthographicSize*required*1.05f;
      if(InWorld)WorldNavigation.RestoreView(WorldNavigation.target,Mathf.Min(size,WorldNavigation.globalZoom));
      else city.Controls.ZoomBy(size-camera.orthographicSize);
      if(!ProjectFocus(camera,points,out projected))break;
     }
     // Translate parallel to the picture plane. Using an arbitrary horizontal
     // ground-plane ray and immediately clamping to a full-terrain footprint
     // previously caused the selected model to re-enter an occluded HUD area.
     Vector2 desired=padded.center;
     float x=(projected.center.x-desired.x)*2f*camera.orthographicSize*camera.aspect;
     float y=(projected.center.y-desired.y)*2f*camera.orthographicSize;
     var shift=camera.transform.right*x+camera.transform.up*y;
     if(InWorld)WorldNavigation.PanTo(WorldNavigation.target+new Vector3(shift.x,0,shift.z));
     else camera.transform.position+=shift;
    }else break;
   }
   // Reproject once after a native synchronous camera adjustment; the
   // existing capture harness renders in the same frame without LateUpdate.
   var v3=Hud.GetComponent<RoyalGoldV3ContextHud>();if(v3)v3.SynchronizeNow();
  }
  /// <summary>Checks every visible model-bound corner against the actual unoccluded UI viewport.</summary>
  public bool IsTargetFramed(GameObject target,out Rect projected,out Rect visible){
   projected=default;visible=Hud?UIVisibleSceneViewport():default;
   if(!target||!Ready||!PlayerCamera)return false;
   bool inFront=ProjectFocus(PlayerCamera,FocusPoints(target),out projected);
   return inFront&&visible.width>0&&visible.height>0&&projected.xMin>=visible.xMin&&projected.xMax<=visible.xMax&&projected.yMin>=visible.yMin&&projected.yMax<=visible.yMax;
  }
  System.Collections.Generic.List<Vector3> FocusPoints(GameObject target){
   var points=new System.Collections.Generic.List<Vector3>();var spot=target.GetComponent<WorldHotspot>();string id=spot?spot.Id:"";
   if(!target.transform.IsChildOf(WorldRoot.transform)){
    var progression=GetComponent<BastionIVVWorldPresentation>();if(progression)AddFocusRenderers(progression.ModelFor(id),points);
    if(points.Count>0)return points;
    if(id=="bastion"){AddFocusRenderers(city.BastionOne,points);AddFocusRenderers(city.BastionTwo,points);}
    else if((id=="sawmill"||id=="barracks")&&city.Parcels){
     var p=city.Parcels;bool left=id=="sawmill";
     AddFocusRenderers(left?p.LeftBuilt:p.RightBuilt,points);
     AddFocusRenderers(left?p.LeftConstruction:p.RightConstruction,points);
     AddFocusRenderers(left?p.LeftAvailable:p.RightAvailable,points);
    }
   }else{
    // Production resource models follow their saved logical sites; older
    // authored scenery is no longer the selection's rendered geometry.
    if(id=="forest-valoria"||id=="quarry-valoria"||
       id.StartsWith("region1:wood:",StringComparison.Ordinal)||
       id.StartsWith("region1:stone:",StringComparison.Ordinal)){
     var liveVisual=WorldRoot.GetComponentsInChildren<Transform>(true)
      .FirstOrDefault(x=>x.name=="Production resource visual "+id);
     // Selection can arrive before LateUpdate enables the near LOD after
     // leaving panorama. Its real mesh still supplies the correct geometry;
     // falling back to retired scenery points the camera away from the site.
     if(liveVisual)
      foreach(var renderer in liveVisual.GetComponentsInChildren<Renderer>(true))
       if(renderer is MeshRenderer||renderer is SkinnedMeshRenderer)
        AddFocusBox(renderer.localBounds,renderer.localToWorldMatrix,points);
     var hit=target.GetComponent<Collider>();
     if(points.Count==0&&hit)AddFocusBox(hit.bounds,Matrix4x4.identity,points);
     if(points.Count>0)return points;
    }
    if(id=="valoria-map-city"){
     var progression=GetComponent<BastionIVVWorldPresentation>();if(progression)AddFocusRenderers(progression.WorldModel,points);
     AddFocusRenderers(WorldBastionOne,points);AddFocusRenderers(WorldBastionTwo,points);AddFocusRenderers(WorldSawmill,points);
    }else{
     if(id=="engendro-valoria"){
      // Follow the actual scaled animated silhouette rather than its hidden
      // camp or tiny original hotspot. Keep gameplay physics untouched.
      var actor=WorldRoot.GetComponentInChildren<SpawnlingEncounterVisual>(true);
      if(actor&&actor.ModelRoot&&actor.ModelRoot.activeInHierarchy){
       // V4's renderer bounds include a huge baked animation culling
       // envelope. Only true skinned geometry may steer the player camera.
       if(actor.TryGeometryBounds(out var visible))AddFocusBox(visible,Matrix4x4.identity,points);
       else AddFocusRenderers(actor.ModelRoot,points);
      }
     }
     if(points.Count==0){
      var authored=WorldRoot.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name=="Zone1 authored "+id||t.name=="Zone1 hostile camp "+id);
      if(authored)AddFocusRenderers(authored.gameObject,points);
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
  public string TargetName(string id)=>id!=null&&id.StartsWith("region1:wood:",StringComparison.Ordinal)?L("Bosque de madera","Timber grove"):id!=null&&id.StartsWith("region1:stone:",StringComparison.Ordinal)?L("Afloramiento de piedra","Stone outcrop"):ValoriaJourneyGuidance.Name(id,englishInterface);
  public WorldHotspot Pick(Vector2 point){
   if(!InWorld)return city.Pick(point);
   // Keep canonical hotspot physics as first authority (including resource
   // gathering, navigation and quests). The larger visual-only creature may
   // project beyond its pre-existing, deliberately untouched hotspot collider.
   var ray=PlayerCamera.ScreenPointToRay(point);
   var hit=Physics.RaycastAll(ray,PlayerCamera.farClipPlane,1<<8).OrderBy(h=>h.distance)
    .Select(h=>h.collider.GetComponent<WorldHotspot>())
    .FirstOrDefault(h=>h!=null&&h.transform.IsChildOf(WorldRoot.transform));
   if(hit)return hit;
   var actor=WorldRoot.GetComponentInChildren<SpawnlingEncounterVisual>(true);
   if(!actor||!actor.ModelRoot||!actor.ModelRoot.activeInHierarchy)return null;
   // Picking the 150m culling envelope would select this monster while
   // the user touches unrelated forest, bridge or quarry terrain.
   if(!actor.TryGeometryBounds(out var b))return null;
   var min=b.min;var max=b.max;
   float left=float.PositiveInfinity,right=float.NegativeInfinity;
   float bottom=float.PositiveInfinity,top=float.NegativeInfinity;
   for(int i=0;i<8;i++){
    Vector3 p=new Vector3((i&1)==0?min.x:max.x,(i&2)==0?min.y:max.y,(i&4)==0?min.z:max.z);
    var v=PlayerCamera.WorldToScreenPoint(p);
    if(v.z<=PlayerCamera.nearClipPlane||v.z>=PlayerCamera.farClipPlane)return null;
    left=Mathf.Min(left,v.x);right=Mathf.Max(right,v.x);
    bottom=Mathf.Min(bottom,v.y);top=Mathf.Max(top,v.y);
   }
   // The projected silhouette is a selection fallback, not new gameplay
   // geometry or a second encounter. A generous box enables touch on mobile.
   if(point.x>=left-8&&point.x<=right+8&&point.y>=bottom-8&&point.y<=top+8)
    return FindTarget("engendro-valoria");
   return null;
  }
  public RoyalSnapshot Read(string selected,bool english){
   englishInterface=english;var data=binding.Read(selected,english);if(InWorld)data.Realm=L("ZONA 1","ZONE 1");
   if(IsWorldTarget(selected)&&!LocalGateway.IsChapterThreeTarget(selected)){
    var action=world.Inspect(selected);var s=boundGateway.Snapshot();
    MarchState selectedMarch=null;
    foreach(var march in MarchCapacityRules.WorldMarches(s))
     if(march.Phase!="idle"&&march.TargetId==selected){selectedMarch=march;break;}
    bool thisMarch=selectedMarch!=null;
    bool active=MarchCapacityRules.ActiveCount(s,city.Clock.UtcTicks)>=MarchCapacityRules.MaxConcurrentMarches;
    var b=new RoyalBuilding{Id=selected,Name=TargetName(selected),Function=DestinationSummary(selected,s),State=action.Label};
    if(thisMarch){b.OperationActive=true;b.State=MarchStatus(selectedMarch);b.Time=RoyalFormatting.Remaining(selectedMarch.PhaseEndsUtcTicks,city.Clock.UtcTicks,english);b.Result=selectedMarch.Phase=="returning"?Reward(selectedMarch.PendingWood,selectedMarch.PendingStone)+L(" al llegar a Valoria"," on arrival at Valoria"):ExpectedReward(selected,s);b.Progress=(float)ZoneOneGatewayHost.TravelFraction(city.Clock.UtcTicks,selectedMarch.PhaseEndsUtcTicks,selectedMarch.Phase=="gathering"?SliceRules.GatherSeconds:SliceRules.TravelSeconds);}
    else if(active)b.Requirements=L("Tres marchas activas. Espera a que regrese una.","Three active marches. Wait for one to return.");
    else if(selected=="engendro-valoria"&&!action.Enabled&&!s.EngendroDefeated)b.Requirements=L("Necesitas Bastión II, cuartel y una marcha confirmada.","Requires Bastion II, barracks and a confirmed march.");
    else if(!action.Enabled&&s.Available.Total<1)b.Requirements=L("No hay tropas disponibles para salir.","No troops are available to depart.");
    if(!thisMarch&&!string.IsNullOrEmpty(action.Kind))b.Action=new RoyalAction(action.Kind,selected,action.Label,b.Requirements,s.Revision,action.Enabled);
    if(!thisMarch&&(selected=="forest-valoria"||selected=="quarry-valoria"))b.Time=L("Ida + trabajo + regreso: ","Travel + work + return: ")+(2*SliceRules.TravelSeconds+SliceRules.GatherSeconds)+" s";
    data.Building=b;
   }
   return data;
  }
  public string MarchStatus(PlayerState s)=>MarchStatus(s.March);
  public string MarchStatus(MarchState m){string phase=m.Phase;return phase=="outbound"?L("En camino a ","Travelling to ")+TargetName(m.TargetId):phase=="gathering"?L("Recolectando en ","Gathering at ")+TargetName(m.TargetId):phase=="returning"?L("Regresando a Valoria","Returning to Valoria"):L("Expedición disponible","Expedition ready");}
  public string Reward(int wood,int stone){var parts=new System.Collections.Generic.List<string>();if(wood>0)parts.Add("+"+wood+L(" madera"," wood"));if(stone>0)parts.Add("+"+stone+L(" piedra"," stone"));return string.Join(" · ",parts);}
  string ExpectedReward(string id,PlayerState s){if(LocalGateway.ResourceKind(s,id)=="wood")return Reward(Math.Min(LocalGateway.ResourceRemaining(s,id),SliceRules.ForestLoad),0);if(LocalGateway.ResourceKind(s,id)=="stone")return Reward(0,Math.Min(LocalGateway.ResourceRemaining(s,id),SliceRules.QuarryLoad));return L("El botín vuelve con las tropas.","Loot returns with the troops.");}
  string DestinationSummary(string id,PlayerState s){
   if(LocalGateway.ResourceKind(s,id)=="wood")return L("Madera para reconstruir Valoria.\nCada expedición: ","Timber to rebuild Valoria.\nPer expedition: ")+ExpectedReward(id,s);
   if(LocalGateway.ResourceKind(s,id)=="stone")return L("Piedra para edificios y defensas.\nCada expedición: ","Stone for buildings and defences.\nPer expedition: ")+ExpectedReward(id,s);
   if(id=="old-watch-ruin")return L("Una antigua atalaya junto al camino. Por ahora sirve como referencia del terreno.","An old watchtower beside the road. It currently serves as a landmark.");
   if(id=="corrupt-scout")return s.ScoutDefeated?L("La ruta está despejada. Tus tropas pueden volver a Valoria.","The route is clear. Your troops can return to Valoria."):L("Aldric y tus arqueros deben despejar este acceso. Botín si vencen: +80 madera · +70 piedra.","Aldric and your archers must clear this route. Victory loot: +80 wood · +70 stone.");
   return s.EngendroDefeated?L("El Engendro ha caído. Bastión II está asegurado.","The Spawn has fallen. Bastion II is secured."):L("El enemigo del capítulo II. Confirma tu expedición antes de salir. Botín si vence: +120 madera · +100 piedra.","Chapter II's enemy. Confirm your expedition before departing. Victory loot: +120 wood · +100 stone.");
  }
  public RoyalResult Submit(RoyalAction offer){
   if(offer!=null&&offer.Kind=="ContactLyra"){
    Hud.ShowNarrativeSequence(new[]{
     new RoyalNarrativePresentation.Line("Lyra","Soy Lyra. Busco a mi hermano. Su compañía desapareció la noche de la Brecha. Encontré su insignia junto a esta fisura; no sé qué significa.","I am Lyra. I seek my brother. His company vanished on the night of the Breach. I found his insignia beside this fissure; I do not know what it means."),
     new RoyalNarrativePresentation.Line("Sir Aldric","También hemos perdido gente. Podemos seguir esas huellas juntos.","We have lost people too. We can follow these tracks together."),
     new RoyalNarrativePresentation.Line("Lyra","Ayudaré en el asalto. Después hablaremos de mi lugar en Valoria.","I will help with the assault. Afterwards we can discuss my place in Valoria.")
    },()=>{boundGateway.Advance();var current=boundGateway.Snapshot();binding.Submit(new RoyalAction("ContactLyra","fissure-valoria","","",current.Revision,true));city.Presenter.RefreshState();});
    return new RoyalResult(true,L("Encuentro con Lyra","Meeting Lyra"));
   }
   var result=binding.Submit(offer);city.Presenter.RefreshState();return result;}
  string L(string es,string en)=>englishInterface?en:es;
 }
}

