using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.Domain;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using Eldoria.World4XPrototype;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.ReferenceClean {
 // Read-only presentation of the ONE authoritative save and clock. No simulation or rewards here.
 public sealed class ValoriaMarchPresentation:MonoBehaviour {
  ValoriaIntegratedWorld host;
  RectTransform layer,objective,vigilantAccess,scoutAccess;
  readonly List<Button> buttons=new List<Button>();
  readonly List<RoyalText> hudTexts=new List<RoyalText>();
  readonly List<Rect> occupied=new List<Rect>();
  readonly RectTransform[] standards=new RectTransform[3];
  readonly RoyalText[] numbers=new RoyalText[3],labels=new RoyalText[3];
  readonly RoyalIconGraphic[] arrows=new RoyalIconGraphic[3];
  readonly ValoriaMarchRouteGraphic[] routes=new ValoriaMarchRouteGraphic[3];
  readonly Vector2[][] points={new Vector2[25],new Vector2[25],new Vector2[25]};
  readonly Vector2[] placed=new Vector2[3];
  readonly bool[] active=new bool[3];
  readonly MarchState[] operations=new MarchState[3];
  readonly Vector3[] corners=new Vector3[4];
  readonly RectTransform[] headers=new RectTransform[3];
  readonly MarchState investigationMarch=new MarchState{TargetId="fissure-valoria",HeroId="aldric",Phase="investigating"};
  readonly long[] labelSeconds={-1,-1,-1};
  readonly int[] labelTroops={-1,-1,-1};
  readonly string[] labelHeroes=new string[3],labelPhases=new string[3];
  static readonly Vector2[] Offsets={Vector2.zero,new Vector2(-200,0),new Vector2(200,0),
   new Vector2(0,120),new Vector2(0,-120),new Vector2(0,240),new Vector2(0,-240),new Vector2(-200,120),new Vector2(200,120),
   new Vector2(-200,-120),new Vector2(200,-120)};
  PlayerState sampled;float nextSample;
  string lastPhase="idle",lastTarget="";int lastPendingWood,lastPendingStone,lastTraining;
  bool observed,projecting;
  public Vector3 ObservedPosition {get;private set;}
  public string ObservedPhase {get;private set;}="idle";
  public bool MarkerVisible=>standards[0]&&standards[0].gameObject.activeInHierarchy;
  static RectTransform CreateRect(string name,Transform parent,int uiLayer){
   var r=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer)).GetComponent<RectTransform>();
   r.SetParent(parent,false);r.gameObject.layer=uiLayer;
   r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);
   r.anchoredPosition=Vector2.zero;r.localScale=Vector3.one;return r;
  }
  static RoyalText Text(string name,RectTransform parent,int size,Color color,Vector2 dimensions,Vector2 offset){
   var r=CreateRect(name,parent,parent.gameObject.layer);r.sizeDelta=dimensions;r.anchoredPosition=offset;
   var t=r.gameObject.AddComponent<RoyalText>();
   t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=size;
   t.fontStyle=FontStyle.Bold;t.color=color;t.alignment=TextAnchor.MiddleCenter;
   t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;
   t.raycastTarget=false;return t;
  }
  public void Initialize(ValoriaIntegratedWorld value){
   host=value;
   headers[0]=(RectTransform)host.Hud.Realm.transform;
   headers[1]=(RectTransform)host.Hud.Resources.transform;
   headers[2]=(RectTransform)host.Hud.Settings.transform;
   layer=CreateRect("Expedition route / authoritative clock",host.Hud.transform,host.Hud.gameObject.layer);
   layer.anchorMin=Vector2.zero;layer.anchorMax=Vector2.one;layer.offsetMin=layer.offsetMax=Vector2.zero;
   layer.SetAsFirstSibling();
   var hudRects=host.Hud.GetComponentsInChildren<RectTransform>(true);
   objective=hudRects.FirstOrDefault(x=>x.name=="V2 / current objective");
   vigilantAccess=hudRects.FirstOrDefault(x=>x.name=="Live Vigilants unlocked access");
   scoutAccess=hudRects.FirstOrDefault(x=>x.name=="Live Scouts unlocked access");
   for(int i=0;i<3;i++){
    var line=CreateRect("Authoritative expedition route "+(i+1),layer,layer.gameObject.layer);
    line.anchorMin=Vector2.zero;line.anchorMax=Vector2.one;line.offsetMin=line.offsetMax=Vector2.zero;
    routes[i]=line.gameObject.AddComponent<ValoriaMarchRouteGraphic>();routes[i].raycastTarget=false;
    routes[i].color=i==0?new Color(.96f,.76f,.39f,.94f):new Color(.52f,.84f,.92f,.90f);
    var marker=CreateRect(i==0?"Aldric expedition standard":"Expedition standard "+(i+1),layer,layer.gameObject.layer);
    marker.sizeDelta=new Vector2(58,58);
    var frame=marker.gameObject.AddComponent<RoyalGoldV2FrameGraphic>();
    frame.Primary=true;frame.DecorativeCorners=false;frame.raycastTarget=false;
    numbers[i]=Text("Expedition slot "+(i+1),marker,32,new Color(1,.87f,.54f),new Vector2(52,48),new Vector2(0,2));
    numbers[i].Put((i+1).ToString());
    var caption=CreateRect("Expedition caption backing "+(i+1),marker,layer.gameObject.layer);
    caption.sizeDelta=new Vector2(184,58);caption.anchoredPosition=new Vector2(0,-60);
    var back=caption.gameObject.AddComponent<Image>();back.color=new Color(.025f,.055f,.095f,.94f);back.raycastTarget=false;
    labels[i]=Text("Expedition hero troops and real deadline "+(i+1),caption,20,new Color(1,.94f,.76f),new Vector2(180,56),Vector2.zero);
    var direction=CreateRect("Offscreen expedition direction "+(i+1),marker,layer.gameObject.layer);
    direction.sizeDelta=new Vector2(20,20);direction.anchoredPosition=new Vector2(30,0);
    arrows[i]=direction.gameObject.AddComponent<RoyalIconGraphic>();arrows[i].Icon=12;
    arrows[i].color=new Color(1,.85f,.43f);arrows[i].raycastTarget=false;
    standards[i]=marker;marker.gameObject.SetActive(false);
   }
   Canvas.willRenderCanvases+=SynchronizeForCamera;
  }
  void Sample(){
   if(!host||!host.Ready)return;
   if(sampled==null||Time.unscaledTime>=nextSample){
    sampled=host.City.Gateway.Snapshot();nextSample=Time.unscaledTime+.1f;
    buttons.Clear();hudTexts.Clear();
    host.Hud.GetComponentsInChildren<Button>(true,buttons);
    host.Hud.GetComponentsInChildren<RoyalText>(true,hudTexts);
   }
  }
  void LateUpdate(){
   Sample();if(sampled==null)return;
   bool investigating=sampled.ChapterThree.InvestigationEndsUtcTicks>host.City.Clock.UtcTicks;
   ObservedPhase=investigating?"investigating":sampled.March.Phase;
   if(observed&&lastPhase!="idle"&&sampled.March.Phase=="idle"){
    string reward=host.Reward(lastPendingWood,lastPendingStone);
    host.Hud.Feedback.Put((host.English?"Expedition returned from ":"Expedición de regreso de ")+
     ValoriaJourneyGuidance.Name(lastTarget,host.English)+(reward.Length>0?" · "+reward:""));
   }
   if(observed&&sampled.ChapterProgress.TrainedArchers>lastTraining)
    host.Hud.Feedback.Put((sampled.ChapterProgress.TrainedArchers-lastTraining)+(host.English?" archers trained and available.":" arqueros entrenados y disponibles."));
   lastPhase=sampled.March.Phase;lastTarget=sampled.March.TargetId;
   lastPendingWood=sampled.March.PendingWood;lastPendingStone=sampled.March.PendingStone;
   lastTraining=sampled.ChapterProgress.TrainedArchers;observed=true;
  }
  Rect Bounds(RectTransform r){
   r.GetWorldCorners(corners);var a=layer.InverseTransformPoint(corners[0]);var b=layer.InverseTransformPoint(corners[2]);
   return Rect.MinMaxRect(a.x,a.y,b.x,b.y);
  }
  Rect FreeArea(float scale){
   // Read the final actual HUD rectangles WITHOUT recursively forcing canvas rebuilds.
   var free=layer.rect;
   foreach(var header in headers)
    if(header&&header.gameObject.activeInHierarchy)free.yMax=Mathf.Min(free.yMax,Bounds(header).yMin);
   var group=objective?objective.GetComponent<CanvasGroup>():null;
   if(objective&&objective.gameObject.activeInHierarchy&&(!group||group.alpha>.1f))free.yMax=Mathf.Min(free.yMax,Bounds(objective).yMin);
   if(host.Hud.Navigation&&host.Hud.Navigation.gameObject.activeInHierarchy)
    free.yMin=Mathf.Max(free.yMin,Bounds((RectTransform)host.Hud.Navigation.transform).yMax);
   if(vigilantAccess&&vigilantAccess.gameObject.activeInHierarchy)free.yMin=Mathf.Max(free.yMin,Bounds(vigilantAccess).yMax);
   if(scoutAccess&&scoutAccess.gameObject.activeInHierarchy)free.yMin=Mathf.Max(free.yMin,Bounds(scoutAccess).yMax);
   // Include the full caption below each standard, above shortcuts and navigation.
   free.xMin+=98*scale;free.xMax-=98*scale;free.yMin+=100*scale;free.yMax-=34*scale;
   return free;
  }
  Vector2 Project(Vector3 world){
   var p=host.WorldCamera.WorldToViewportPoint(world);var r=layer.rect;
   return new Vector2(r.xMin+p.x*r.width,r.yMin+p.y*r.height);
  }
  static Vector2 Clamp(Vector2 p,Rect r)=>new Vector2(Mathf.Clamp(p.x,r.xMin,r.xMax),Mathf.Clamp(p.y,r.yMin,r.yMax));
  void OccupiedAreas(){
   occupied.Clear();
   foreach(var button in buttons){
    if(!button||!button.gameObject.activeInHierarchy||button.transform.IsChildOf(layer))continue;
    if(button.GetComponentsInParent<CanvasGroup>().Any(g=>g.alpha<.1f))continue;
    occupied.Add(Bounds((RectTransform)button.transform));
   }
   foreach(var text in hudTexts){
    if(!text||!text.isActiveAndEnabled||text.transform.IsChildOf(layer)||
       string.IsNullOrEmpty(text.text)||text.text.Length>9||!text.text.Contains(":"))continue;
    if(text.GetComponentsInParent<CanvasGroup>().Any(g=>g.alpha<.1f))continue;
    // The existing world node timers also remain readable underneath their buttons.
    var r=Bounds(text.rectTransform);r.xMin-=7;r.xMax+=7;r.yMin-=7;r.yMax+=7;occupied.Add(r);
   }
  }
  Vector2 Place(Vector2 actual,Rect free,float scale,int index){
   var origin=Clamp(actual,free);var best=origin;float bestPenalty=float.MaxValue;
   foreach(var offset in Offsets){
    var candidate=Clamp(origin+offset*scale,free);
    var footprint=Rect.MinMaxRect(candidate.x-96*scale,candidate.y-90*scale,candidate.x+96*scale,candidate.y+31*scale);
    float penalty=0;
    foreach(var r in occupied)if(footprint.Overlaps(r))
     penalty+=(Mathf.Min(footprint.xMax,r.xMax)-Mathf.Max(footprint.xMin,r.xMin))*
      (Mathf.Min(footprint.yMax,r.yMax)-Mathf.Max(footprint.yMin,r.yMin));
    for(int j=0;j<index;j++)if(active[j]){
     var d=candidate-placed[j];
     if(Mathf.Abs(d.x)<190*scale&&Mathf.Abs(d.y)<116*scale)penalty+=1000000;
    }
    if(penalty==0)return candidate;
    if(penalty<bestPenalty){best=candidate;bestPenalty=penalty;}
   }
   return best;
  }
  public void SynchronizeForCamera(){
   if(projecting||!layer||!host||!host.Ready)return;
   projecting=true;
   try{
    Sample();if(sampled==null)return;
    bool investigation=sampled.ChapterThree.InvestigationEndsUtcTicks>host.City.Clock.UtcTicks;
    investigationMarch.Route=sampled.ChapterThree.InvestigationRoute;
    investigationMarch.PhaseEndsUtcTicks=sampled.ChapterThree.InvestigationEndsUtcTicks;
    operations[0]=investigation?investigationMarch:sampled.March;
    operations[1]=operations[2]=null;
    if(investigation){int k=1;foreach(var m in MarchCapacityRules.WorldMarches(sampled))if(m.Phase!="idle"&&k<3)operations[k++]=m;}
    else if(sampled.ExtraMarches!=null)for(int i=0;i<Math.Min(2,sampled.ExtraMarches.Count);i++)operations[i+1]=sampled.ExtraMarches[i];
    var canvas=layer.GetComponentInParent<Canvas>();float scale=Mathf.Clamp(.70f/Mathf.Max(.1f,canvas.scaleFactor),.70f,1.60f);
    var free=FreeArea(scale);OccupiedAreas();var origin=host.FindTarget("valoria-map-city");
    for(int i=0;i<3;i++){
     var m=operations[i];var target=m==null?null:host.FindTarget(m.TargetId);
     active[i]=host.InWorld&&m!=null&&m.Phase!="idle"&&origin&&target&&free.width>0&&free.height>0;
     standards[i].gameObject.SetActive(active[i]);
     // The player explicitly rejected the bright dashed trajectory. Preserve
     // the real march standards, position, countdown and saved waypoint data;
     // never draw diagnostic line segments across the authored landscape.
     routes[i].gameObject.SetActive(false);
     if(!active[i])continue;
     var a=origin.transform.position;var b=target.transform.position;
     float fraction=m.Phase=="gathering"?1:m.Phase=="investigating"?
      Mathf.Clamp01((float)((host.City.Clock.UtcTicks-(m.PhaseEndsUtcTicks-TimeSpan.FromMinutes(20).Ticks))/(double)TimeSpan.TicksPerSecond/SliceRules.TravelSeconds)):
      (float)ZoneOneGatewayHost.TravelFraction(host.City.Clock.UtcTicks,m.PhaseEndsUtcTicks,SliceRules.TravelSeconds);
     if(m.Phase=="returning")fraction=1-fraction;
     var position=RoutePosition(m,a,b,fraction)+Vector3.up*3;
     if(i==0)ObservedPosition=position;
     var actual=Project(position);placed[i]=Place(actual,free,scale,i);
     standards[i].anchoredPosition=placed[i];standards[i].localScale=Vector3.one*scale;
     long seconds=Math.Max(0,(m.PhaseEndsUtcTicks-host.City.Clock.UtcTicks)/TimeSpan.TicksPerSecond);
     if(labelSeconds[i]!=seconds||labelTroops[i]!=m.Troops.Total||labelHeroes[i]!=m.HeroId||labelPhases[i]!=m.Phase){
      string hero=m.HeroId=="lyra"?"Lyra":m.HeroId=="aldric+lyra"?"Aldric + Lyra":"Aldric";
      labels[i].Put(hero+"\n"+(m.Phase=="investigating"?(host.English?"Fissure":"Brecha"):m.Troops.Total+(host.English?" troops":" tropas"))+" · "+
       (seconds/60)+":"+(seconds%60).ToString("00"));
      labelSeconds[i]=seconds;labelTroops[i]=m.Troops.Total;labelHeroes[i]=m.HeroId;labelPhases[i]=m.Phase;
     }
     bool outside=!free.Contains(actual)||(actual-placed[i]).sqrMagnitude>16f*scale*scale;arrows[i].gameObject.SetActive(outside);
     if(outside){var d=actual-placed[i];arrows[i].rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg);}
     // Do not populate or submit the retired route mesh each canvas frame.
    }
   }finally{projecting=false;}
  }
  static Vector3 RoutePosition(MarchState march,Vector3 from,Vector3 to,float t){
   var route=march.Route;if(route==null||route.Count<2)return Vector3.Lerp(from,to,t);
   float total=0;for(int i=1;i<route.Count;i++)total+=Vector3.Distance(Point(route[i-1]),Point(route[i]));
   float remaining=Mathf.Clamp01(t)*total;
   for(int i=1;i<route.Count;i++){var a=Point(route[i-1]);var b=Point(route[i]);float length=Vector3.Distance(a,b);if(remaining<=length)return Vector3.Lerp(a,b,length>0?remaining/length:0);remaining-=length;}
   return Point(route[route.Count-1]);
  }
  static Vector3 Point(WorldRoutePoint p)=>new Vector3(p.X,p.Y,p.Z);
  void OnDestroy(){Canvas.willRenderCanvases-=SynchronizeForCamera;if(layer)Destroy(layer.gameObject);}
 }
 public sealed class ValoriaMarchRouteGraphic:MaskableGraphic {
  Vector2[] points=Array.Empty<Vector2>();
  public Rect Clip;
  public void SetPoints(Vector2[] value){points=value??Array.Empty<Vector2>();SetVerticesDirty();}
  static bool Edge(float p,float q,ref float min,ref float max){
   if(Mathf.Abs(p)<.00001f)return q>=0;
   float r=q/p;if(p<0){if(r>max)return false;min=Mathf.Max(min,r);}else{if(r<min)return false;max=Mathf.Min(max,r);}return true;
  }
  bool Visible(ref Vector2 a,ref Vector2 b){
   var d=b-a;float min=0,max=1;
   if(!Edge(-d.x,a.x-Clip.xMin,ref min,ref max)||!Edge(d.x,Clip.xMax-a.x,ref min,ref max)||
      !Edge(-d.y,a.y-Clip.yMin,ref min,ref max)||!Edge(d.y,Clip.yMax-a.y,ref min,ref max))return false;
   var start=a;a=start+d*min;b=start+d*max;return true;
  }
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();float half=Mathf.Clamp(1.2f/Mathf.Max(.1f,canvas?canvas.scaleFactor:1f),1.2f,3f);
   for(int i=1;i<points.Length;i++){
    if((i&1)==0)continue;
    var a=points[i-1];var b=points[i];if(!Visible(ref a,ref b))continue;
    var delta=b-a;if(delta.sqrMagnitude<.01f)continue;
    var n=new Vector2(-delta.y,delta.x).normalized*half;int start=vh.currentVertCount;
    vh.AddVert(a-n,color,Vector2.zero);vh.AddVert(a+n,color,Vector2.zero);vh.AddVert(b+n,color,Vector2.zero);vh.AddVert(b-n,color,Vector2.zero);
    vh.AddTriangle(start,start+1,start+2);vh.AddTriangle(start,start+2,start+3);
   }
  }
 }
}
