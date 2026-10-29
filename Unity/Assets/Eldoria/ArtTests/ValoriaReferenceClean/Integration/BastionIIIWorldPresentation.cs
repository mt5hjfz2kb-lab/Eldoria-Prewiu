using System;
using System.Collections.Generic;
using System.Linq;
using Eldoria.Domain;
using Eldoria.Presentation;
using Eldoria.World4XCanon;
using UnityEngine;
using UnityEngine.Rendering;
namespace Eldoria.ReferenceClean {
 // Presentation only: all unlocks, deadlines, inventory and rewards come from the existing gateway.
 public sealed class BastionIIIWorldPresentation:MonoBehaviour {
  ValoriaIntegratedWorld host;
  GameObject town,granary,granarySite,siteStock; readonly Dictionary<string,GameObject> nodes=new Dictionary<string,GameObject>();
  readonly List<(GameObject cue,string building,int level)> cues=new List<(GameObject,string,int)>();
  readonly Dictionary<string,GameObject> priorities=new Dictionary<string,GameObject>();
  readonly List<Mesh> terrainMeshes=new List<Mesh>();
  static readonly string[] VisibleAuthoredEncounterIds={"hunt-wolf","fissure-valoria"};
  bool placed;float refresh;
  public void Initialize(ValoriaIntegratedWorld h){
   host=h;town=new GameObject("Bastion III city additions");town.transform.SetParent(transform,false);
   granarySite=new GameObject("Granary site");granarySite.transform.SetParent(town.transform,false);granarySite.transform.position=new Vector3(-19,.56f,9);
   Hotspot(granarySite,"granary",new Vector3(11,5,9));
   granary=Model("BastionIII/Granary/Granary",granarySite.transform,Vector3.zero,10.4f);
   siteStock=new GameObject("Granary construction supplies");siteStock.transform.SetParent(granarySite.transform,false);
   Model("BastionIII/Granary/StockCrate",siteStock.transform,new Vector3(-1,0,0),1.5f);
   Model("BastionIII/Granary/StockSack",siteStock.transform,new Vector3(1,0,0),1.3f);
   // Existing, textured production stock denotes the small level increments.
   MakeStock("granary",granarySite.transform,new Vector3(0,0,-5),"Sack");
   if(host.City.Parcels){
    MakeStock("sawmill",town.transform,host.City.Parcels.LeftBuilt.transform.position+new Vector3(4,0,-4),"Crate");
    MakeStock("barracks",town.transform,host.City.Parcels.RightBuilt.transform.position+new Vector3(4,0,-4),"Barrel");
   }
   // III repairs and provisions the existing keep; its approved silhouette stays intact.
   MakeStock("bastion",town.transform,new Vector3(9,.56f,17),"Crate",3);
   // One restrained, authored supply arrangement reflects the permanent IV choice.
   // Existing textured props preserve the city style and avoid a new floating label.
   foreach(var option in new[]{("production","Crate"),("defense","Barrel"),("civil","Sack")}){
    var group=new GameObject("Valoria priority "+option.Item1);group.transform.SetParent(town.transform,false);priorities.Add(option.Item1,group);
    var origin=new Vector3(-7,.56f,16);
    for(int i=0;i<3;i++)Model("BastionIII/Granary/Stock"+option.Item2,group.transform,origin+new Vector3((i%2)*1.25f,0,(i/2)*1.2f),1.7f);
    group.SetActive(false);
   }
   foreach(string id in new[]{"hunt-wolf","hunt-boar","meat-valoria","fissure-valoria"}){
    var root=new GameObject(id);root.transform.SetParent(host.WorldRoot.transform,false);nodes.Add(id,root);
    Hotspot(root,id,id=="fissure-valoria"?new Vector3(9,2,4):new Vector3(7,4,5));
    if(id=="fissure-valoria")Model("BastionIII/PrivateFissure",root.transform,Vector3.zero,8);
    else if(id=="hunt-wolf"){
     // Match the boar encounter\u0027s actual rendered world width (5m), not its imported prefab scale.
     // Keep the two wolves separated so equal-sized animated silhouettes do not collide.
     Model("BastionIII/AshWolf",root.transform,new Vector3(-3.0f,0,-.6f),5f);
     var second=Model("BastionIII/AshWolf",root.transform,new Vector3(3.0f,0,1.5f),5f);if(second)second.transform.Rotate(0,24,0);
    }else{
     Model("World4X/Creatures/RockBoarNear",root.transform,Vector3.zero,id=="hunt-boar"?5:3.6f);
     if(id=="meat-valoria")Model("World4X/Creatures/RockBoarNear",root.transform,new Vector3(2,0,2.8f),2.6f);
    }
    root.SetActive(false);
   }
   Apply();
  }
  void MakeStock(string building,Transform parent,Vector3 point,string prop,int minimum=2){
   for(int level=minimum;level<=4;level++){
    var group=new GameObject(building+" level "+level+" stock");group.transform.SetParent(parent,false);group.transform.localPosition=point;
    float offset=(level-minimum)*1.8f;
    for(int i=0;i<2;i++)Model("BastionIII/Granary/Stock"+prop,group.transform,new Vector3(offset+i*.7f,0,i*.65f),1.1f);
    cues.Add((group,building,level));
   }
  }
  static void Hotspot(GameObject root,string id,Vector3 size){root.layer=8;root.AddComponent<WorldHotspot>().Id=id;var box=root.AddComponent<BoxCollider>();box.size=size;box.center=Vector3.up*size.y*.5f;}
  static GameObject Model(string path,Transform parent,Vector3 position,float width){
   var source=Resources.Load<GameObject>(path);if(!source){Debug.LogError("BASTION_III_MISSING_ART "+path);return null;}
   var model=Instantiate(source,parent);model.name=path.Split('/').Last();model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.identity;model.transform.localScale=Vector3.one;
   // The source GLB/prefab is imported as disabled presentation geometry.
   // Creating an active WorldHotspot does not activate these children: native
   // art-frustum evidence showed wolf meshes correctly framed but INACTIVE.
   // Enable the ACTUAL imported renderers along their prefab ancestry, not
   // placeholder cubes or extra copied artwork. The encounter parent retains
   // its original progression / panorama visibility gate.
   if(path=="BastionIII/AshWolf"||path=="BastionIII/PrivateFissure"||path.StartsWith("BastionIII/Granary/Stock",StringComparison.Ordinal)){
    model.SetActive(true);
    foreach(var renderer in model.GetComponentsInChildren<Renderer>(true)){
     for(var node=renderer.transform;node&&node!=parent;node=node.parent)
      if(!node.gameObject.activeSelf)node.gameObject.SetActive(true);
     if(!renderer.enabled)renderer.enabled=true;
    }
   }
   // These original EA03 crate/sack/barrel prefabs use 3 LOD meshes. At the
   // city camera's 29.5 orthographic scale their stock LODGroup can cull all
   // meshes, producing identical II/III and 1/2 A-B captures despite the
   // gameplay level changing. Select the authored highest-detail LOD0 on
   // small CITY props only; keep the same textured source, not duplicate
   // overlapping LOD meshes. Never change Region1's world LOD pipeline.
   if(path.StartsWith("BastionIII/Granary/Stock",StringComparison.Ordinal)){
    foreach(var lod in model.GetComponentsInChildren<LODGroup>(true)){
     var detail=lod.GetLODs();
     if(detail.Length==0)continue;
     var keep=new HashSet<Renderer>(detail[0].renderers.Where(r=>r!=null));
     lod.enabled=false;
     foreach(var renderer in lod.GetComponentsInChildren<Renderer>(true))
      renderer.enabled=keep.Contains(renderer);
    }
   }
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
   var local=parent.InverseTransformPoint(new Vector3(b.center.x,b.min.y,b.center.z));if(path=="BastionIII/PrivateFissure")local.y=0;model.transform.localPosition+=position-local;return model;
  }
  // glTFast may restore disabled nested skinned meshes when the gameplay
  // encounter toggles from inactive to active. Keep ONLY existing authored
  // renderers of an encounter actually unlocked/visible by the real gateway.
  // A raycastable hotspot without active renderers is not playable artwork.
  static void EnsureVisibleImportedArt(GameObject encounter,bool closingScar=false){
   foreach(var renderer in encounter.GetComponentsInChildren<Renderer>(true)){
    for(var node=renderer.transform;node&&node!=encounter.transform;node=node.parent)
     if(!node.gameObject.activeSelf)node.gameObject.SetActive(true);
    // After the ONE-TIME encounter closes, the void and its sparse ether
    // illumination must disappear immediately, leaving a short-lived low
    // soil/stone scar. Do not ever reactivate a closed private fissure.
    bool extinguished=closingScar&&(renderer.name=="Dark split floor"||
       renderer.name.StartsWith("Faint isolated ether vein",StringComparison.Ordinal));
    if(extinguished){renderer.enabled=false;continue;}
    if(!renderer.enabled)renderer.enabled=true;
   }
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
   // Match the existing geological Breach technique: fit the private split to
   // real terrain triangles while preserving its shallow authored strata.
   var fissure=nodes["fissure-valoria"];
   foreach(var filter in fissure.GetComponentsInChildren<MeshFilter>(true)){
    var mesh=Instantiate(filter.sharedMesh);terrainMeshes.Add(mesh);var vertices=mesh.vertices;
    for(int i=0;i<vertices.Length;i++){
     var p=filter.transform.TransformPoint(vertices[i]);float relief=p.y-fissure.transform.position.y;
     p.y=host.WorldHome.y+RegionOneBlenderLandscape.Height(p.x-host.WorldHome.x,p.z-host.WorldHome.z)+relief;
     vertices[i]=filter.transform.InverseTransformPoint(p);
    }
    mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();filter.sharedMesh=mesh;
   }
   placed=true;
  }
  void OnDestroy(){foreach(var mesh in terrainMeshes)if(mesh)Destroy(mesh);}
  void Update(){if(host&&host.Ready&&Time.unscaledTime>=refresh){refresh=Time.unscaledTime+.2f;Apply();}}
  // Animated glTF imports may restore their authored disabled renderer flags
  // AFTER Update(), between the 0.2s presentation refreshes. The native
  // camera proof confirmed in-frustum wolf meshes with renderers INACTIVE.
  // Reassert existing imported art ONLY for currently visible encounters
  // after Animation evaluation and before the camera renders this frame.
  // Never enable the inactive chapter, respawning hunts, closed fissure,
  // inaccessible zoom, or other global resources.
  // Imported glTF animation may reset MeshRenderer.enabled late in the frame.
  // Apply the existing unlocked encounter visibility policy at the actual
  // camera render boundary as well as Update/LateUpdate. URP receives the SRP
  // callback; Camera.onPreCull covers the editor's explicit Camera.Render.
  // This never creates proxy meshes or revives a defeated / locked target.
  void OnEnable(){RenderPipelineManager.beginCameraRendering+=BeforeWorldRender;Camera.onPreCull+=BeforeLegacyWorldRender;}
  void OnDisable(){RenderPipelineManager.beginCameraRendering-=BeforeWorldRender;Camera.onPreCull-=BeforeLegacyWorldRender;}
  void BeforeLegacyWorldRender(Camera camera){BeforeWorldRender(default(ScriptableRenderContext),camera);}
  void BeforeWorldRender(ScriptableRenderContext context,Camera camera){
   if(!host||!host.Ready||!host.InWorld||camera!=host.WorldCamera)return;
   foreach(var id in VisibleAuthoredEncounterIds)
    if(nodes.TryGetValue(id,out var encounter)&&encounter.activeInHierarchy)
     EnsureVisibleImportedArt(encounter,id=="fissure-valoria"&&host.City.Gateway.Snapshot().ChapterThree.FissureClosed);
  }
  void LateUpdate(){
   if(!host||!host.Ready||!host.InWorld)return;
   foreach(var id in VisibleAuthoredEncounterIds){
    if(nodes.TryGetValue(id,out var encounter)&&encounter.activeInHierarchy)
     EnsureVisibleImportedArt(encounter,id=="fissure-valoria"&&host.City.Gateway.Snapshot().ChapterThree.FissureClosed);
   }
  }
  public void Apply(){
   if(!host||host.City.Gateway==null)return;var s=host.City.Gateway.Snapshot();var c=s.ChapterThree;
   bool unlocked=s.BastionLevel>=3;town.SetActive(unlocked&&!host.InWorld);
   if(granary)granary.SetActive(c.GranaryLevel>0&&c.GranaryLevel<5);
   // A buildable site has actual stored supplies instead of an invisible target.
   siteStock.SetActive(c.GranaryLevel==0);
   foreach(var cue in cues){int level=cue.building=="granary"?c.GranaryLevel:cue.building=="sawmill"?s.SawmillLevel:cue.building=="barracks"?s.BarracksLevel:s.BastionLevel;cue.cue.SetActive(level>=cue.level&&level<5);}
   foreach(var priority in priorities)priority.Value.SetActive(s.BastionLevel>=4&&c.DevelopmentPriority==priority.Key);
   if(!placed&&unlocked)PlaceNodes(s);
   bool near=!RegionOneResourcePlacement.IsPanoramic(host.WorldCamera.orthographicSize,host.WorldNavigation.globalZoom);
   foreach(var pair in nodes){bool visible=unlocked&&placed&&near;
    if(pair.Key=="fissure-valoria")visible&=!c.FissureClosed||host.City.Clock.UtcTicks<c.ClosingEndsUtcTicks;
    if(pair.Key=="hunt-wolf")visible&=host.City.Clock.UtcTicks>=c.WolfRespawnUtcTicks;
    if(pair.Key=="hunt-boar")visible&=host.City.Clock.UtcTicks>=c.BoarRespawnUtcTicks;
    if(pair.Key=="meat-valoria")visible&=c.MeatRemaining>0;
    pair.Value.SetActive(visible);
    if(visible)EnsureVisibleImportedArt(pair.Value,pair.Key=="fissure-valoria"&&c.FissureClosed);
    pair.Value.GetComponent<Collider>().enabled=visible&&!(pair.Key=="fissure-valoria"&&c.FissureClosed);
    if(pair.Key=="fissure-valoria"&&c.FissureClosed){float left=Mathf.Clamp01((float)((c.ClosingEndsUtcTicks-host.City.Clock.UtcTicks)/(double)TimeSpan.TicksPerSecond/6.5));pair.Value.transform.localScale=new Vector3(1,Mathf.Max(.03f,left),Mathf.Max(.03f,left));}
   }
  }
 }
}
