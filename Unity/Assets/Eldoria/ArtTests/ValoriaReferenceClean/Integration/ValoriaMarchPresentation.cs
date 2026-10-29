using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.Domain;
using Eldoria.UI.RoyalGold;
using Eldoria.World4XPrototype;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.ReferenceClean {
 // A visible expedition standard and route, interpolated from the existing saved deadlines.
 // This component never moves a clock, owns simulation, pays or grants resources.
 public sealed class ValoriaMarchPresentation:MonoBehaviour {
  ValoriaIntegratedWorld host;RectTransform layer,standard;ValoriaMarchRouteGraphic route;RoyalText label;
  readonly RectTransform[] otherStandards=new RectTransform[2];
  readonly ValoriaMarchRouteGraphic[] otherRoutes=new ValoriaMarchRouteGraphic[2];
  readonly RoyalText[] otherLabels=new RoyalText[2];
  string lastPhase="idle",lastTarget="";int lastPendingWood,lastPendingStone,lastTraining;
  bool observed;
  PlayerState sampled;float nextSample;
  public Vector3 ObservedPosition {get;private set;}
  public string ObservedPhase {get;private set;}="idle";
  public bool MarkerVisible=>standard&&standard.gameObject.activeInHierarchy;
  public void Initialize(ValoriaIntegratedWorld value){
   host=value;
   layer=new GameObject("Expedition route / authoritative clock",typeof(RectTransform)).GetComponent<RectTransform>();
   layer.SetParent(host.Hud.transform,false);layer.anchorMin=Vector2.zero;layer.anchorMax=Vector2.one;layer.offsetMin=layer.offsetMax=Vector2.zero;layer.SetAsFirstSibling();
   route=layer.gameObject.AddComponent<ValoriaMarchRouteGraphic>();route.raycastTarget=false;route.color=new Color(.91f,.72f,.36f,.8f);
   standard=new GameObject("Aldric expedition standard",typeof(RectTransform),typeof(Image),typeof(Outline)).GetComponent<RectTransform>();standard.SetParent(layer,false);standard.sizeDelta=new Vector2(40,44);
   standard.GetComponent<Image>().color=new Color(.025f,.09f,.17f,.94f);standard.GetComponent<Image>().raycastTarget=false;
   var outline=standard.GetComponent<Outline>();outline.effectColor=new Color(.9f,.72f,.38f);outline.effectDistance=new Vector2(1.5f,-1.5f);
   var icon=new GameObject("Expedition",typeof(RectTransform),typeof(RoyalIconGraphic)).GetComponent<RoyalIconGraphic>();icon.transform.SetParent(standard,false);icon.Icon=5;icon.color=new Color(1,.82f,.44f);icon.raycastTarget=false;icon.rectTransform.sizeDelta=new Vector2(34,34);
   label=Instantiate(host.Hud.Mission.Objective,standard);label.name="Expedition troop count";label.fontSize=15;label.alignment=TextAnchor.MiddleCenter;label.raycastTarget=false;label.color=new Color(1,.94f,.76f);label.rectTransform.anchorMin=label.rectTransform.anchorMax=new Vector2(.5f,.5f);label.rectTransform.pivot=new Vector2(.5f,.5f);label.rectTransform.anchoredPosition=new Vector2(0,-36);label.rectTransform.sizeDelta=new Vector2(160,24);
   standard.gameObject.SetActive(false);
   for(int i=0;i<2;i++){
    var extraLayer=new GameObject("Additional expedition route "+(i+2),typeof(RectTransform),typeof(ValoriaMarchRouteGraphic)).GetComponent<RectTransform>();
    extraLayer.SetParent(layer,false);extraLayer.anchorMin=Vector2.zero;extraLayer.anchorMax=Vector2.one;
    extraLayer.offsetMin=extraLayer.offsetMax=Vector2.zero;
    var line=extraLayer.GetComponent<ValoriaMarchRouteGraphic>();line.raycastTarget=false;line.color=new Color(.52f,.84f,.92f,.78f);
    var marker=Instantiate(standard,extraLayer);marker.name="Expedition standard "+(i+2);
    marker.gameObject.SetActive(false);
    otherStandards[i]=marker;otherRoutes[i]=line;
    otherLabels[i]=marker.GetComponentInChildren<RoyalText>(true);
   }
  }
  void UpdateAdditional(PlayerState state,bool investigating){
   // Investigation uses the primary visual; keep BOTH simultaneous world routes visible.
   var displayed=investigating?MarchCapacityRules.WorldMarches(state).Where(m=>m.Phase!="idle").ToList():state.ExtraMarches;
   var origin=host.FindTarget("valoria-map-city");
   var canvas=layer.GetComponentInParent<Canvas>();
   var uiCamera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
   for(int i=0;i<2;i++){
    var m=displayed!=null&&i<displayed.Count?displayed[i]:null;
    if(!host.InWorld||m==null||m.Phase=="idle"||!origin){
     otherStandards[i].gameObject.SetActive(false);
     otherRoutes[i].SetPoints(Array.Empty<Vector2>());
     continue;
    }
    var destination=host.FindTarget(m.TargetId);
    if(!destination){
     otherStandards[i].gameObject.SetActive(false);
     otherRoutes[i].SetPoints(Array.Empty<Vector2>());
     continue;
    }
    var from=origin.transform.position;var to=destination.transform.position;
    float fraction=m.Phase=="gathering"?1:(float)ZoneOneGatewayHost.TravelFraction(host.City.Clock.UtcTicks,m.PhaseEndsUtcTicks,SliceRules.TravelSeconds);
    if(m.Phase=="returning")fraction=1-fraction;
    var points=new List<Vector2>();
    for(int j=0;j<=24;j++){
     var sample=RoutePosition(m,from,to,j/24f)+Vector3.up*1.5f;
     var screen=host.WorldCamera.WorldToScreenPoint(sample);
     if(screen.z>0&&RectTransformUtility.ScreenPointToLocalPointInRectangle(layer,screen,uiCamera,out var local))points.Add(local);
    }
    otherRoutes[i].SetPoints(points.ToArray());
    var world=RoutePosition(m,from,to,fraction)+Vector3.up*3;
    var screenPosition=host.WorldCamera.WorldToScreenPoint(world);
    bool visible=screenPosition.z>0&&host.WorldCamera.pixelRect.Contains(screenPosition);
    otherStandards[i].gameObject.SetActive(visible);
    if(!visible)continue;
    if(RectTransformUtility.ScreenPointToLocalPointInRectangle(layer,screenPosition,uiCamera,out var position))
      otherStandards[i].anchoredPosition=position;
    otherLabels[i].Put((m.HeroId=="lyra"?"Lyra":m.HeroId=="aldric+lyra"?"Aldric + Lyra":"Aldric")+" · "+m.Troops.Total);
   }
  }
  void LateUpdate(){
   if(!host||!host.Ready||!layer)return;
   if(sampled==null||Time.unscaledTime>=nextSample){sampled=host.City.Gateway.Snapshot();nextSample=Time.unscaledTime+.1f;}
   var s=sampled;
   bool investigating=s.ChapterThree.InvestigationEndsUtcTicks>host.City.Clock.UtcTicks;
   UpdateAdditional(s,investigating);
   var march=investigating?new MarchState{TargetId="fissure-valoria",HeroId="aldric",Route=s.ChapterThree.InvestigationRoute,Phase="investigating"}:s.March;
   string phase=march.Phase;ObservedPhase=phase;
   if(observed&&lastPhase!="idle"&&phase=="idle"){
    string reward=host.Reward(lastPendingWood,lastPendingStone);
    host.Hud.Feedback.Put((host.English?"Expedition returned from ":"Expedición de regreso de ")+ValoriaJourneyGuidance.Name(lastTarget,host.English)+(reward.Length>0?" · "+reward:""));
   }
   if(observed&&s.ChapterProgress.TrainedArchers>lastTraining)host.Hud.Feedback.Put((s.ChapterProgress.TrainedArchers-lastTraining)+(host.English?" archers trained and available.":" arqueros entrenados y disponibles."));
   lastPhase=phase;lastTarget=march.TargetId;lastPendingWood=s.March.PendingWood;lastPendingStone=s.March.PendingStone;lastTraining=s.ChapterProgress.TrainedArchers;observed=true;
   bool visible=host.InWorld&&phase!="idle";standard.gameObject.SetActive(visible);
   if(!visible){route.SetPoints(Array.Empty<Vector2>());return;}
   var origin=host.FindTarget("valoria-map-city");var target=host.FindTarget(march.TargetId);if(!origin||!target){standard.gameObject.SetActive(false);route.SetPoints(Array.Empty<Vector2>());return;}
   Vector3 a=origin.transform.position,b=target.transform.position;
   float fraction=investigating?Mathf.Clamp01((float)((host.City.Clock.UtcTicks-(s.ChapterThree.InvestigationEndsUtcTicks-TimeSpan.FromMinutes(20).Ticks))/(double)TimeSpan.TicksPerSecond/SliceRules.TravelSeconds)):phase=="gathering"?1:(float)ZoneOneGatewayHost.TravelFraction(host.City.Clock.UtcTicks,s.March.PhaseEndsUtcTicks,SliceRules.TravelSeconds);
   if(phase=="returning")fraction=1-fraction;
   ObservedPosition=RoutePosition(march,a,b,fraction)+Vector3.up*3;
   var canvas=layer.GetComponentInParent<Canvas>();var uiCamera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
   var points=new List<Vector2>();
   for(int i=0;i<=24;i++){
    var world=RoutePosition(march,a,b,i/24f)+Vector3.up*1.5f;var screen=host.WorldCamera.WorldToScreenPoint(world);
    if(screen.z>0&&RectTransformUtility.ScreenPointToLocalPointInRectangle(layer,screen,uiCamera,out var local))points.Add(local);
   }
   route.SetPoints(points.ToArray());
   var marker=host.WorldCamera.WorldToScreenPoint(ObservedPosition);bool onScreen=marker.z>0&&host.WorldCamera.pixelRect.Contains(marker);
   standard.gameObject.SetActive(onScreen);if(!onScreen)return;
   if(RectTransformUtility.ScreenPointToLocalPointInRectangle(layer,marker,uiCamera,out var position))standard.anchoredPosition=position;
   standard.localScale=Vector3.one*(phase=="gathering"?1+Mathf.Sin(Time.unscaledTime*4)*.05f:1);
   label.Put(investigating?(host.English?"Aldric · investigating":"Aldric · investigando"):(s.March.HeroId=="lyra"?"Lyra · ":s.March.HeroId=="aldric+lyra"?"Aldric + Lyra · ":"Aldric · ")+s.March.Troops.Total);
  }
  static Vector3 RoutePosition(MarchState march,Vector3 from,Vector3 to,float t){
   var route=march.Route;if(route==null||route.Count<2)return Vector3.Lerp(from,to,t); // pre-migration saved march only
   float total=0;for(int i=1;i<route.Count;i++)total+=Vector3.Distance(Point(route[i-1]),Point(route[i]));
   float remaining=Mathf.Clamp01(t)*total;
   for(int i=1;i<route.Count;i++){var a=Point(route[i-1]);var b=Point(route[i]);float length=Vector3.Distance(a,b);if(remaining<=length)return Vector3.Lerp(a,b,length>0?remaining/length:0);remaining-=length;}
   return Point(route[route.Count-1]);
  }
  static Vector3 Point(WorldRoutePoint p)=>new Vector3(p.X,p.Y,p.Z);
  void OnDestroy(){if(layer)Destroy(layer.gameObject);}
 }
 public sealed class ValoriaMarchRouteGraphic:MaskableGraphic {
  Vector2[] points=Array.Empty<Vector2>();
  public void SetPoints(Vector2[] value){points=value??Array.Empty<Vector2>();SetVerticesDirty();}
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();for(int i=1;i<points.Length;i++){
    if((i&1)==0)continue;
    var a=points[i-1];var b=points[i];var delta=b-a;if(delta.sqrMagnitude<.01f)continue;
    var n=new Vector2(-delta.y,delta.x).normalized*1.5f;int start=vh.currentVertCount;
    vh.AddVert(a-n,color,Vector2.zero);vh.AddVert(a+n,color,Vector2.zero);vh.AddVert(b+n,color,Vector2.zero);vh.AddVert(b-n,color,Vector2.zero);vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);
   }
  }
 }
}
