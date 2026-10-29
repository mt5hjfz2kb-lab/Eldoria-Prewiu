using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.Domain;
using Eldoria.Presentation;
using Eldoria.World4XCanon;
using UnityEngine;
namespace Eldoria.ReferenceClean {
 [DefaultExecutionOrder(1300)] public sealed class BastionIVVWorldPresentation:MonoBehaviour {
  ValoriaIntegratedWorld host;GameObject cityRoot,stoneSite,stoneBase,worldFive;
  bool assetsLoaded;PlayerState cached;object cachedGateway;float nextStatePoll;Vector3 worldFiveScale,worldFiveFoot;
  readonly Dictionary<string,GameObject> models=new Dictionary<string,GameObject>();
  readonly List<(GameObject root,bool wasActive)> keepTorches=new List<(GameObject,bool)>(),quarryTrees=new List<(GameObject,bool)>();
  readonly List<(GameObject model,int level)> stoneStock=new List<(GameObject,int)>();
  public GameObject WorldModel=>worldFive;
  public void RotateCityForReview(float delta){foreach(var model in models.Values)if(model){var b=Bounds(model);model.transform.RotateAround(new Vector3(b.center.x,b.min.y,b.center.z),Vector3.up,delta);}}
  public GameObject ModelFor(string id)=>models.TryGetValue(id,out var model)?model:null;
  public void Initialize(ValoriaIntegratedWorld h){host=h;cityRoot=new GameObject("Bastion IV V architecture");cityRoot.transform.SetParent(transform,false);
   foreach(var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include,FindObjectsSortMode.None)){var p=light.transform.position;if(light.name=="Torch glow"&&p.z>27&&p.z<32&&Mathf.Abs(p.x)<4)keepTorches.Add((light.gameObject,light.gameObject.activeSelf));}
   Apply();
  }
  void LoadUnlockedAssets(){if(assetsLoaded)return;assetsLoaded=true;var h=host;
   Add("bastion","bastion-v",h.City.BastionTwo,28);
   Add("sawmill","aserradero-v",h.City.Parcels.LeftBuilt,12);
   Add("barracks","cuartel-v",h.City.Parcels.RightBuilt,13);
   models["granary"]=Load("granero-v",cityRoot.transform,new Vector3(-19,.56f,9),10.4f);
   stoneSite=new GameObject("Stoneworks site");stoneSite.transform.SetParent(cityRoot.transform,false);stoneSite.transform.position=new Vector3(19,.56f,9);stoneSite.layer=8;
   foreach(var r in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)){if(r.transform.IsChildOf(h.WorldRoot.transform)||r.transform.IsChildOf(cityRoot.transform))continue;string n=r.name.ToLowerInvariant();if(!n.Contains("tree")&&!n.Contains("pine")&&!n.Contains("conifer"))continue;var d=r.bounds.center-stoneSite.transform.position;d.y=0;if(d.sqrMagnitude<49&&!quarryTrees.Exists(x=>x.root==r.gameObject))quarryTrees.Add((r.gameObject,r.gameObject.activeSelf));}
   stoneSite.AddComponent<WorldHotspot>().Id="stoneworks";var box=stoneSite.AddComponent<BoxCollider>();box.size=new Vector3(11,6,10);box.center=new Vector3(0,3,0);
   // Reuse an existing authored workshop for I-IV, never the complete tier-V silhouette early.
   stoneBase=Load("stoneworks-base",stoneSite.transform,Vector3.zero,8.5f);
   models["stoneworks"]=Load("cantera-v",stoneSite.transform,Vector3.zero,11);
   for(int level=1;level<=4;level++){
    var prop=Resources.Load<GameObject>("BastionIII/Granary/StockCrate");if(!prop)continue;
    var stock=Instantiate(prop,stoneSite.transform);stock.name="Stoneworks processing stock "+level;
    Normalize(stock,new Vector3(-3+(level-1)*1.9f,0,-4),1.4f);stoneStock.Add((stock,level));
   }
   if(h.WorldBastionTwo){var b=Bounds(h.WorldBastionTwo);worldFive=Load("bastion-v",h.WorldBastionTwo.transform.parent,new Vector3(b.center.x,b.min.y,b.center.z),Mathf.Max(b.size.x,b.size.z),true);if(worldFive){worldFiveScale=worldFive.transform.localScale;var wb=Bounds(worldFive);worldFiveFoot=new Vector3(wb.center.x,wb.min.y,wb.center.z);}}
  }
  void Add(string id,string resource,GameObject old,float fallback){var b=Bounds(old);models[id]=Load(resource,cityRoot.transform,new Vector3(b.center.x,b.min.y,b.center.z),b.size.sqrMagnitude>.1f?Mathf.Max(b.size.x,b.size.z):fallback);}
  static Bounds Bounds(GameObject g){var rs=g.GetComponentsInChildren<Renderer>(true);if(rs.Length==0)return new Bounds(g.transform.position,Vector3.zero);var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
  static GameObject Load(string id,Transform parent,Vector3 point,float width,bool worldPoint=false){var source=Resources.Load<GameObject>("BastionIVV/Models/"+id);if(!source){Debug.LogError("BASTION_IV_V_MISSING_ART "+id);return null;}
   var model=Instantiate(source,parent);model.name="Tier V "+id;Normalize(model,worldPoint?parent.InverseTransformPoint(point):point,width);return model;
  }
  static void Normalize(GameObject model,Vector3 point,float width){model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.identity;model.transform.localScale=Vector3.one;model.SetActive(true);
   foreach(var r in model.GetComponentsInChildren<Renderer>(true)){for(var t=r.transform;t&&t!=model.transform.parent;t=t.parent)t.gameObject.SetActive(true);r.enabled=true;}
   foreach(var lod in model.GetComponentsInChildren<LODGroup>(true)){var levels=lod.GetLODs();if(levels.Length==0)continue;var first=new HashSet<Renderer>(levels[0].renderers);lod.enabled=false;foreach(var r in lod.GetComponentsInChildren<Renderer>(true))r.enabled=first.Contains(r);}
   foreach(var c in model.GetComponentsInChildren<Collider>(true))Destroy(c);
   var b=Bounds(model);model.transform.localScale*=width/Mathf.Max(.001f,Mathf.Max(b.size.x,b.size.z));b=Bounds(model);model.transform.localPosition+=point-model.transform.parent.InverseTransformPoint(new Vector3(b.center.x,b.min.y,b.center.z));
  }
  void LateUpdate(){if(!host||!host.Ready)return;if(cached==null||!ReferenceEquals(cachedGateway,host.City.Gateway)||Time.unscaledTime>=nextStatePoll)Apply();else Render(cached);}
  public void Apply(){if(!host||host.City.Gateway==null)return;cachedGateway=host.City.Gateway;cached=host.City.Gateway.Snapshot();nextStatePoll=Time.unscaledTime+.2f;Render(cached);}
  void Render(PlayerState s){foreach(var light in keepTorches)if(light.root)light.root.SetActive(s.BastionLevel<5&&light.wasActive);foreach(var tree in quarryTrees)if(tree.root)tree.root.SetActive(!(s.BastionLevel>=5&&s.ChapterThree.StoneworksLevel>0)&&tree.wasActive);cityRoot.SetActive(!host.InWorld&&s.BastionLevel>=5);
   if(s.BastionLevel<5){if(worldFive)worldFive.SetActive(false);return;}LoadUnlockedAssets();
   Set("bastion",s.BastionLevel);Set("sawmill",s.SawmillLevel);Set("barracks",s.BarracksLevel);Set("granary",s.ChapterThree.GranaryLevel);Set("stoneworks",s.ChapterThree.StoneworksLevel);
   // Keep authoritative hotspots, deadlines and the established I-IV source objects intact.
   if(ModelFor("bastion")&&s.BastionLevel>=5)host.City.BastionTwo.SetActive(false);
   if(ModelFor("sawmill")&&s.SawmillLevel>=5)host.City.Parcels.LeftBuilt.SetActive(false);
   if(ModelFor("barracks")&&s.BarracksLevel>=5)host.City.Parcels.RightBuilt.SetActive(false);
   stoneSite.SetActive(s.BastionLevel>=5);if(stoneBase)stoneBase.SetActive(s.ChapterThree.StoneworksLevel>0&&s.ChapterThree.StoneworksLevel<5);
   foreach(var stock in stoneStock)stock.model.SetActive((s.ChapterThree.StoneworksLevel==0&&stock.level<=2)||(s.ChapterThree.StoneworksLevel>=stock.level&&s.ChapterThree.StoneworksLevel<5));
   if(worldFive){if(worldFive.layer!=host.WorldRoot.layer)foreach(var t in worldFive.GetComponentsInChildren<Transform>(true))t.gameObject.layer=host.WorldRoot.layer;float zoom=host.WorldCamera.orthographicSize;float strategicScale=Mathf.Lerp(1f,1.8f,Mathf.InverseLerp(110f,280f,zoom))*Mathf.Max(1f,zoom/280f);worldFive.transform.localScale=worldFiveScale*strategicScale;var wb=Bounds(worldFive);worldFive.transform.position+=worldFiveFoot-new Vector3(wb.center.x,wb.min.y,wb.center.z);worldFive.SetActive(s.BastionLevel>=5);if(s.BastionLevel>=5)host.WorldBastionTwo.SetActive(false);if(zoom>=110){var target=host.FindTarget("valoria-map-city");var hit=target?target.GetComponent<BoxCollider>():null;if(hit){wb=Bounds(worldFive);hit.center=hit.transform.InverseTransformPoint(wb.center);var size=hit.transform.InverseTransformVector(wb.size);hit.size=new Vector3(Mathf.Abs(size.x),Mathf.Abs(size.y),Mathf.Abs(size.z))+Vector3.one;}}}
  }
  void Set(string id,int level){if(models.TryGetValue(id,out var model)&&model)model.SetActive(level>=5);}
 }
}
