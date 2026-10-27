using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.Domain;
using Eldoria.Presentation;
using Eldoria.World4XCanon;
using UnityEngine;
namespace Eldoria.ReferenceClean {
 // Presentation only: all unlocks, deadlines, inventory and rewards come from the existing gateway.
 public sealed class BastionIIIWorldPresentation:MonoBehaviour {
  ValoriaIntegratedWorld host;
  GameObject town,granary,granarySite,siteStock; readonly Dictionary<string,GameObject> nodes=new Dictionary<string,GameObject>();
  readonly List<(GameObject cue,string building,int level)> cues=new List<(GameObject,string,int)>();
  bool placed;float refresh;
  public void Initialize(ValoriaIntegratedWorld h){
   host=h;town=new GameObject("Bastion III city additions");town.transform.SetParent(transform,false);
   granarySite=new GameObject("Granary site");granarySite.transform.SetParent(town.transform,false);granarySite.transform.position=new Vector3(-19,.56f,9);
   Hotspot(granarySite,"granary",new Vector3(11,5,9));
   granary=Model("BastionIII/Granary/Granary",granarySite.transform,Vector3.zero,10.4f);
   siteStock=new GameObject("Granary construction supplies");siteStock.transform.SetParent(granarySite.transform,false);
   Model("Valoria/UrbanProps/Crate",siteStock.transform,new Vector3(-1,0,0),1.5f);
   Model("Valoria/UrbanProps/Sack",siteStock.transform,new Vector3(1,0,0),1.3f);
   // Existing, textured production stock denotes the small level increments.
   MakeStock("granary",granarySite.transform,new Vector3(0,0,-5),"Sack");
   if(host.City.Parcels){
    MakeStock("sawmill",town.transform,host.City.Parcels.LeftBuilt.transform.position+new Vector3(4,0,-4),"Crate");
    MakeStock("barracks",town.transform,host.City.Parcels.RightBuilt.transform.position+new Vector3(4,0,-4),"Barrel");
   }
   // III repairs and provisions the existing keep; its approved silhouette stays intact.
   MakeStock("bastion",town.transform,new Vector3(5,.56f,28),"Crate",3);
   foreach(string id in new[]{"hunt-wolf","hunt-boar","meat-valoria","fissure-valoria"}){
    var root=new GameObject(id);root.transform.SetParent(host.WorldRoot.transform,false);nodes.Add(id,root);
    Hotspot(root,id,id=="fissure-valoria"?new Vector3(9,2,4):new Vector3(7,4,5));
    if(id=="fissure-valoria")Model("BastionIII/PrivateFissure",root.transform,Vector3.zero,8);
    else if(id=="hunt-wolf"){
     Model("BastionIII/AshWolf",root.transform,new Vector3(-1.8f,0,0),3.8f);
     var second=Model("BastionIII/AshWolf",root.transform,new Vector3(1.5f,0,1),3.2f);if(second)second.transform.Rotate(0,24,0);
    }else{
     Model("World4X/Creatures/RockBoarNear",root.transform,Vector3.zero,id=="hunt-boar"?5:3.6f);
     if(id=="meat-valoria")Model("World4X/Creatures/RockBoarNear",root.transform,new Vector3(2,0,2.8f),2.6f);
    }
    root.SetActive(false);
   }
   Apply();
  }
  void MakeStock(string building,Transform parent,Vector3 point,string prop,int minimum=2){
   for(int level=minimum;level<=3;level++){
    var group=new GameObject(building+" level "+level+" stock");group.transform.SetParent(parent,false);group.transform.localPosition=point;
    float offset=(level-minimum)*1.8f;
    for(int i=0;i<2;i++)Model("Valoria/UrbanProps/"+prop,group.transform,new Vector3(offset+i*.7f,0,i*.65f),1.1f);
    cues.Add((group,building,level));
   }
  }
  static void Hotspot(GameObject root,string id,Vector3 size){root.layer=8;root.AddComponent<WorldHotspot>().Id=id;var box=root.AddComponent<BoxCollider>();box.size=size;box.center=Vector3.up*size.y*.5f;}
  static GameObject Model(string path,Transform parent,Vector3 position,float width){
   var source=Resources.Load<GameObject>(path);if(!source){Debug.LogError("BASTION_III_MISSING_ART "+path);return null;}
   var model=Instantiate(source,parent);model.name=path.Split('/').Last();model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.identity;model.transform.localScale=Vector3.one;
   foreach(var c in model.GetComponentsInChildren<Collider>(true))Destroy(c);
   if(path=="BastionIII/AshWolf"){
    var animation=model.GetComponentInChildren<Animation>(true);
    if(!animation)Debug.LogError("BASTION_III_WOLF_ANIMATION_MISSING");
    else{
     animation.Stop();bool idle=false;
     foreach(AnimationState state in animation)if(state.name=="Idle"){
      state.wrapMode=WrapMode.Loop;animation.clip=state.clip;animation.Play(state.name);
      state.normalizedTime=position.x<0?0:.4f;animation.Sample();idle=true;break;
     }
     if(!idle)Debug.LogError("BASTION_III_WOLF_IDLE_MISSING");
    }
   }
   var renderers=model.GetComponentsInChildren<Renderer>(true);if(renderers.Length==0){Debug.LogError("BASTION_III_EMPTY_ART "+path);return model;}
   var b=Bounds(renderers);model.transform.localScale*=width/Mathf.Max(b.size.x,b.size.z);b=Bounds(renderers);
   var local=parent.InverseTransformPoint(new Vector3(b.center.x,b.min.y,b.center.z));model.transform.localPosition+=position-local;return model;
  }
  static Bounds Bounds(Renderer[] rs){var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
  void PlaceNodes(PlayerState s){
   var service=host.WorldRoot.GetComponentInChildren<RegionOneResourcePlacement>(true);
   var routes=service?service.MarchRoutes as RegionOneTerrainRoutes:null;if(routes==null)return;
   var used=new List<Vector3>();int ordinal=0;
   foreach(var pair in nodes){bool found=false;
    // Deterministic sites on the actual terrain, reachable from the same city.
    for(int radius=72;radius<=176&&!found;radius+=16)for(int step=0;step<24&&!found;step++){
     float angle=(step*15+ordinal*87)*Mathf.Deg2Rad;float x=Mathf.Cos(angle)*radius,z=Mathf.Sin(angle)*radius;
     var p=host.WorldHome+new Vector3(x,RegionOneBlenderLandscape.Height(x,z),z);
     if(s.WorldCreatureLocations.Any(q=>q.Alive&&Vector3.Distance(new Vector3(q.X,q.Y,q.Z),p)<24)||used.Any(q=>Vector3.Distance(q,p)<30)||!routes.FootprintClear(p,8)||!routes.TryRouteTo(s,p,out var route))continue;
     // Use the reachable endpoint; no fabricated march path or floating fauna.
     var end=route[route.Count-1];p=new Vector3(end.X,end.Y,end.Z);
     if(used.Any(q=>Vector3.Distance(q,p)<30))continue;
     pair.Value.transform.position=p;used.Add(p);found=true;
    }
    if(!found){Debug.LogError("BASTION_III_NO_REACHABLE_SITE "+pair.Key);return;}ordinal++;
   }
   placed=true;
  }
  void Update(){if(host&&host.Ready&&Time.unscaledTime>=refresh){refresh=Time.unscaledTime+.2f;Apply();}}
  public void Apply(){
   if(!host||host.City.Gateway==null)return;var s=host.City.Gateway.Snapshot();var c=s.ChapterThree;
   bool unlocked=s.BastionLevel>=3;town.SetActive(unlocked&&!host.InWorld);
   if(granary)granary.SetActive(c.GranaryLevel>0);
   // A buildable site has actual stored supplies instead of an invisible target.
   siteStock.SetActive(c.GranaryLevel==0);
   foreach(var cue in cues){int level=cue.building=="granary"?c.GranaryLevel:cue.building=="sawmill"?s.SawmillLevel:cue.building=="barracks"?s.BarracksLevel:s.BastionLevel;cue.cue.SetActive(level>=cue.level);}
   if(!placed&&unlocked)PlaceNodes(s);
   bool near=!RegionOneResourcePlacement.IsPanoramic(host.WorldCamera.orthographicSize,host.WorldNavigation.globalZoom);
   foreach(var pair in nodes){bool visible=unlocked&&placed&&near;
    if(pair.Key=="fissure-valoria")visible&=!c.FissureClosed||host.City.Clock.UtcTicks<c.ClosingEndsUtcTicks;
    if(pair.Key=="hunt-wolf")visible&=host.City.Clock.UtcTicks>=c.WolfRespawnUtcTicks;
    if(pair.Key=="hunt-boar")visible&=host.City.Clock.UtcTicks>=c.BoarRespawnUtcTicks;
    if(pair.Key=="meat-valoria")visible&=c.MeatRemaining>0;
    pair.Value.SetActive(visible);
    pair.Value.GetComponent<Collider>().enabled=visible&&!(pair.Key=="fissure-valoria"&&c.FissureClosed);
    if(pair.Key=="fissure-valoria"&&c.FissureClosed){float left=Mathf.Clamp01((float)((c.ClosingEndsUtcTicks-host.City.Clock.UtcTicks)/(double)TimeSpan.TicksPerSecond/6.5));pair.Value.transform.localScale=new Vector3(1,Mathf.Max(.03f,left),Mathf.Max(.03f,left));}
   }
  }
 }
}
