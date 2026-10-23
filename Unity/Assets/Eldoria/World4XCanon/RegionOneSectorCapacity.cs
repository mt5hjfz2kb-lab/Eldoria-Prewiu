using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Eldoria.World4XCanon
{
 // Explicit native QA fixture. Never bootstrapped and never writes a player save.
 // It consumes the existing four content IDs; it does not grant gameplay commands/rewards.
 public static class RegionOneSectorCapacity
 {
  [Serializable] public class Point { public float x,z,radius; }
  [Serializable] public class Route { public Point[] points; }
  [Serializable] public class Layout { public string stage; public float minX,maxX,minZ,maxZ; public Point[] cities,trees; public Route[] routes; }
  [Serializable] public class Site { public int index; public Vector3 position; public float slope,clearRadius; public bool wood,stone; }
  [Serializable] public class Instance { public string id,content; public int site,generation; public long due; public bool active=true; }
  [Serializable] public class Fixture { public string authority="TEST_ONLY_NOT_LIVE_WORLD"; public long revision; public List<Instance> instances=new List<Instance>(); }
  [Serializable] public class Report {
   public string scope="Disposable capacity and relocation fixture; NOT production respawn or network multiplayer";
   public int packableInstances,reservedRelocationSites;public int validSites,instances,citySpaces,routeSamples,respawned,duplicateCount,meshHits;
   public float maxContactError,maxCityRelief,minimumCityRadius;public float testMarkerScale=1f;public float conservativeFootprintRadius,woodRadius,stoneRadius,enemyRadius;public int woodInstances,stoneInstances,enemyInstances;public bool saveReloadStable,repeatedTickIdempotent,occupiedRejected,changedPosition,originalSaveUntouched,physicalDevice=false;
   public int walkableSamples,reachableSamples,reachableCities,dynamicNearTriangles,dynamicFarTriangles;public bool fullSizeNavigation;public Site[] sites;public Point[] cities; public Fixture population;
  }
  static float Segment(float x,float z,Point a,Point b) {
   var p=new Vector2(x,z);var aa=new Vector2(a.x,a.z);var d=new Vector2(b.x-a.x,b.z-a.z);
   return Vector2.Distance(p,aa+d*Mathf.Clamp01(Vector2.Dot(p-aa,d)/d.sqrMagnitude));
  }
  static float Road(Layout l,float x,float z)=>l.routes.Min(r=>Enumerable.Range(1,r.points.Length-1).Min(i=>Segment(x,z,r.points[i-1],r.points[i])));
  static float Distance(float x,float z,Point p)=>Vector2.Distance(new Vector2(x,z),new Vector2(p.x,p.z));
  static bool Allowed(Site p,string content,int bastion) {
   if(content=="forest-valoria")return p.wood;
   if(content=="quarry-valoria")return p.stone;
   if(content=="corrupt-scout")return bastion>=1;
   if(content=="engendro-valoria")return bastion>=2;
   return false;
  }
  // Stateless placement: callers must supply a common authoritative occupancy snapshot.
  // No client-local timer, save schema or second gameplay gateway is introduced here.
  static int Choose(Site[] sites,string content,int bastion,Dictionary<int,float> occupied,Dictionary<string,float> radii,int old,int start) {
   for(int n=0;n<sites.Length;n++){int k=(start+n)%sites.Length;var s=sites[k];if(s.index!=old&&!occupied.ContainsKey(s.index)&&s.clearRadius>=radii[content]&&!sites.Any(other=>occupied.ContainsKey(other.index)&&Vector2.Distance(new Vector2(other.position.x,other.position.z),new Vector2(s.position.x,s.position.z))<radii[content]+occupied[other.index])&&Allowed(s,content,bastion))return s.index;}return -1;
  }
  static void TickFixture(Fixture f,Site[] sites,Dictionary<string,float> radii,long now) {
   var occupied=f.instances.Where(i=>i.active).ToDictionary(i=>i.site,i=>radii[i.content]);
   foreach(var i in f.instances.Where(i=>!i.active&&i.due<=now).OrderBy(i=>sites.Count(site=>Allowed(site,i.content,2)&&site.clearRadius>=radii[i.content])).ThenBy(i=>i.id,StringComparer.Ordinal)) {
    int next=Choose(sites,i.content,2,occupied,radii,i.site,(i.site+37+i.generation)%sites.Length);
    if(next<0)continue;i.site=next;i.generation++;i.active=true;i.due=0;occupied.Add(next,radii[i.content]);f.revision++;
   }
  }
  public static void Capture(Camera camera,GameObject root,Transform world,Vector3 home,string output) {
   var asset=Resources.Load<TextAsset>("World4X/Pilot/LivingSectorLayout");if(!asset)return;
   var layout=JsonUtility.FromJson<Layout>(asset.text);var dir=Path.Combine(output,"living-sector");Directory.CreateDirectory(dir);
   var temporary=new List<GameObject>();var surfaces=new List<MeshCollider>();var displays=new List<GameObject>();var meshes=new List<Mesh>();Material clearanceMaterial=null;Mesh enemyMesh=null,enemyFarMesh=null;Material[] enemyMaterials=null,enemyFarMaterials=null;var enemyDisplays=new List<GameObject>();
   var oldPosition=camera.transform.position;var oldRotation=camera.transform.rotation;float oldZoom=camera.orthographicSize,oldAspect=camera.aspect;
   var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
   var report=new Report{cities=layout.cities};
   var host=UnityEngine.Object.FindFirstObjectByType<Eldoria.ReferenceClean.ValoriaIntegratedWorld>();
   string playerBefore=JsonUtility.ToJson(host.City.Gateway.Snapshot());
   try {
    foreach(var f in root.GetComponentsInChildren<MeshFilter>().Where(f=>f.name.StartsWith("Pilot_Terrain"))) {
     var go=new GameObject("QA only actual surface");temporary.Add(go);go.layer=31;go.transform.SetPositionAndRotation(f.transform.position,f.transform.rotation);go.transform.localScale=f.transform.lossyScale;
     var c=go.AddComponent<MeshCollider>();c.sharedMesh=f.sharedMesh;surfaces.Add(c);
    }
    Physics.SyncTransforms();
    bool Sample(float x,float z,out RaycastHit result) {
     result=new RaycastHit();bool found=false;var ray=new Ray(home+new Vector3(x,250,z),Vector3.down);
     foreach(var c in surfaces)if(c.Raycast(ray,out var hit,500)&&(!found||hit.distance<result.distance)){result=hit;found=true;}
     if(found)report.meshHits++;return found;
    }
    // Measure imported visual footprints BEFORE placement, including every part of the enemy.
    var component=world.GetComponentsInChildren<MonoBehaviour>(true).FirstOrDefault(c=>c&&c.GetType().Name=="SpawnlingEncounterVisual");
    var model=component?.GetType().GetField("ModelRoot")?.GetValue(component) as GameObject;
    if(!model)throw new InvalidOperationException("Existing enemy model unavailable");
    enemyMesh=BakeVisual(model.GetComponentsInChildren<SkinnedMeshRenderer>(true),out enemyMaterials);meshes.Add(enemyMesh);
    var lod=component.GetComponent<LODGroup>();if(!lod||lod.GetLODs().Length<2)throw new InvalidOperationException("Existing enemy LOD missing");
    enemyFarMesh=BakeVisual(lod.GetLODs()[1].renderers,out enemyFarMaterials);meshes.Add(enemyFarMesh);
    float MarkerRadius(string prefix){var renderer=world.GetComponentsInChildren<MeshRenderer>(true).First(r=>r.name.StartsWith(prefix,StringComparison.Ordinal));var mesh=renderer.GetComponent<MeshFilter>().sharedMesh;var foot=renderer.transform.TransformPoint(mesh.vertices[0]);return mesh.vertices.Max(v=>{var p=renderer.transform.TransformPoint(v)-foot;return new Vector2(p.x,p.z).magnitude;})+mesh.uv2.Max(v=>Mathf.Abs(v.x))+.5f;}
    report.woodRadius=MarkerRadius("Reference existing wood grove");report.stoneRadius=MarkerRadius("Reference existing quarry");report.enemyRadius=Mathf.Max(enemyMesh.vertices.Max(v=>new Vector2(v.x,v.z).magnitude),enemyFarMesh.vertices.Max(v=>new Vector2(v.x,v.z).magnitude))+.5f;
    var radii=new Dictionary<string,float>{{"forest-valoria",report.woodRadius},{"quarry-valoria",report.stoneRadius},{"engendro-valoria",report.enemyRadius},{"corrupt-scout",report.enemyRadius}};report.conservativeFootprintRadius=radii.Values.Max();
    var sites=new List<Site>();
    for(float z=layout.minZ+6;z<=layout.maxZ-4;z+=5)for(float x=layout.minX+6;x<=layout.maxX-6;x+=5) {
     if(layout.cities.Any(c=>Distance(x,z,c)<c.radius+4)||Road(layout,x,z)<4||layout.trees.Any(t=>Distance(x,z,t)<t.radius+2))continue;
     if(!Sample(x,z,out var hit)||Vector3.Angle(hit.normal,Vector3.up)>20)continue;
     bool footprint=true;foreach(var d in new[]{new Vector2(-1.5f,0),new Vector2(1.5f,0),new Vector2(0,-1.5f),new Vector2(0,1.5f)})if(!Sample(x+d.x,z+d.y,out var h)||Mathf.Abs(h.point.y-hit.point.y)>.6f)footprint=false;
     if(!footprint)continue;
     float treeDistance=layout.trees.Min(t=>Distance(x,z,t));
     sites.Add(new Site{index=sites.Count,position=hit.point,slope=Vector3.Angle(hit.normal,Vector3.up),clearRadius=Mathf.Min(Mathf.Min(Mathf.Min(x-layout.minX,layout.maxX-x),Mathf.Min(z-layout.minZ,layout.maxZ-z)),Mathf.Min(Road(layout,x,z)-3,Mathf.Min(layout.cities.Min(c=>Distance(x,z,c)-c.radius),layout.trees.Min(t=>Distance(x,z,t)-t.radius)))),wood=treeDistance<35,stone=x>20||treeDistance>22});
    }
    report.sites=sites.ToArray();report.validSites=sites.Count;
    if(sites.Count<150)throw new InvalidOperationException("Insufficient actual-mesh capacity: "+sites.Count);
    // City footprints are reserved empty ground, not decorative city models or player settlements.
    var cityCollider=host.FindTarget("valoria-map-city").GetComponent<Collider>();if(!cityCollider)throw new InvalidOperationException("Existing city footprint missing");report.minimumCityRadius=new Vector2(cityCollider.bounds.extents.x,cityCollider.bounds.extents.z).magnitude+.5f;
    foreach(var c in layout.cities) {
     if(c.radius<report.minimumCityRadius)throw new InvalidOperationException("Reserved city pad smaller than existing city interaction footprint");
     var heights=new List<float>();for(int z=-1;z<=1;z++)for(int x=-1;x<=1;x++) {
      float px=c.x+x*c.radius*.7f,pz=c.z+z*c.radius*.7f;
      if(!Sample(px,pz,out var h))throw new InvalidOperationException("City footprint outside terrain");heights.Add(h.point.y);
      if(Road(layout,px,pz)<2||layout.trees.Any(t=>Distance(px,pz,t)<t.radius))throw new InvalidOperationException("City footprint occupied");
     }
     float relief=heights.Max()-heights.Min();report.maxCityRelief=Mathf.Max(report.maxCityRelief,relief);if(relief>.3f)throw new InvalidOperationException("City footprint too steep");report.citySpaces++;
    }
    // March corridor sampled at 1 m spacing, with a 2 m clear width and actual slope.
    foreach(var r in layout.routes)for(int i=1;i<r.points.Length;i++) {
     var a=new Vector2(r.points[i-1].x,r.points[i-1].z);var b=new Vector2(r.points[i].x,r.points[i].z);var side=new Vector2(-(b-a).y,(b-a).x).normalized;
     int steps=Mathf.CeilToInt(Vector2.Distance(a,b));for(int n=0;n<=steps;n++)foreach(float offset in new[]{-1f,0f,1f}) {
      var p=Vector2.Lerp(a,b,n/(float)steps)+side*offset;if(p.x<=layout.minX+1||p.x>=layout.maxX-1||p.y<=layout.minZ+1||p.y>=layout.maxZ-1)continue;
      if(!Sample(p.x,p.y,out var h)||Vector3.Angle(h.normal,Vector3.up)>25||layout.trees.Any(t=>Distance(p.x,p.y,t)<t.radius))throw new InvalidOperationException("Blocked march corridor");report.routeSamples++;
     }
    }
    var fstate=new Fixture();var occupied=new Dictionary<int,float>();
    for(int n=0;n<40;n++)foreach(string content in new[]{"forest-valoria","quarry-valoria","engendro-valoria"}) {
     int k=Choose(report.sites,content,2,occupied,radii,-1,n*11%sites.Count);if(k<0)continue;
     occupied.Add(k,radii[content]);fstate.instances.Add(new Instance{id="qa:"+content+":"+n,content=content,site=k});
    }
    report.packableInstances=fstate.instances.Count;fstate.instances=fstate.instances.Where((i,n)=>n%4!=3).ToList();report.reservedRelocationSites=report.packableInstances-fstate.instances.Count;
    report.instances=fstate.instances.Count;report.woodInstances=fstate.instances.Count(i=>i.content=="forest-valoria");report.stoneInstances=fstate.instances.Count(i=>i.content=="quarry-valoria");report.enemyInstances=fstate.instances.Count(i=>i.content=="engendro-valoria");if(report.instances<12||report.woodInstances==0||report.stoneInstances==0||report.enemyInstances==0)throw new InvalidOperationException("Insufficient measured population capacity: "+report.instances+" wood="+report.woodInstances+" stone="+report.stoneInstances+" enemy="+report.enemyInstances+" radii="+report.woodRadius+","+report.stoneRadius+","+report.enemyRadius);
    if(Choose(report.sites,"engendro-valoria",1,new Dictionary<int,float>(),radii,-1,0)!=-1)throw new InvalidOperationException("Progression exclusion failed");
    report.occupiedRejected=Choose(report.sites,"corrupt-scout",2,sites.ToDictionary(site=>site.index,site=>1f),radii,-1,0)==-1;
    var depleted=fstate.instances.Where((i,n)=>n%4==0).ToArray();var oldSites=depleted.ToDictionary(i=>i.id,i=>i.site);
    foreach(var i in depleted){i.active=false;i.due=100;}fstate.revision++;
    string saved=JsonUtility.ToJson(fstate);File.WriteAllText(Path.Combine(dir,"fixture-before-respawn.json"),saved);
    fstate=JsonUtility.FromJson<Fixture>(File.ReadAllText(Path.Combine(dir,"fixture-before-respawn.json")));report.saveReloadStable=JsonUtility.ToJson(fstate)==saved;
    TickFixture(fstate,report.sites,radii,99);if(fstate.instances.Count(i=>!i.active)!=depleted.Length)throw new InvalidOperationException("Premature respawn");
    TickFixture(fstate,report.sites,radii,100);report.respawned=fstate.instances.Count(i=>i.generation==1);report.changedPosition=fstate.instances.Where(i=>oldSites.ContainsKey(i.id)).All(i=>i.site!=oldSites[i.id]);
    string after=JsonUtility.ToJson(fstate);fstate=JsonUtility.FromJson<Fixture>(after);TickFixture(fstate,report.sites,radii,100);report.repeatedTickIdempotent=after==JsonUtility.ToJson(fstate);
    report.duplicateCount=fstate.instances.Count-fstate.instances.Select(i=>i.id).Distinct().Count()+fstate.instances.Count(i=>i.active)-fstate.instances.Where(i=>i.active).Select(i=>i.site).Distinct().Count();
    if(report.respawned!=depleted.Length||!report.changedPosition||!report.saveReloadStable||!report.repeatedTickIdempotent||report.duplicateCount!=0||!report.occupiedRejected)throw new InvalidOperationException("Capacity fixture state invariant failed: respawned="+report.respawned+" expected="+depleted.Length+" changed="+report.changedPosition+" duplicates="+report.duplicateCount);
    report.population=fstate;
    // Actual-mesh flood fill of the populated extension, with one metre agent clearance.
    // This measures geometry connectivity; it is deliberately not a replacement march pathfinder.
    var free=new Dictionary<Vector2Int,Vector3>();
    for(int iz=0;iz<=(layout.maxZ-layout.minZ-4)/2;iz++)for(int ix=0;ix<=(layout.maxX-layout.minX-4)/2;ix++){
     float x=layout.minX+2+ix*2,z=layout.minZ+2+iz*2;
     if(layout.trees.Any(t=>Distance(x,z,t)<t.radius+1)||layout.cities.Any(c=>Distance(x,z,c)<c.radius+1)||fstate.instances.Any(i=>i.active&&Vector2.Distance(new Vector2(x+home.x,z+home.z),new Vector2(sites[i.site].position.x,sites[i.site].position.z))<radii[i.content]+1))continue;
     if(Sample(x,z,out var h)&&Vector3.Angle(h.normal,Vector3.up)<=25)free.Add(new Vector2Int(ix,iz),h.point);
    }
    var entry=home+new Vector3(layout.routes[0].points[0].x,0,172);var start=free.OrderBy(p=>new Vector2(p.Value.x-entry.x,p.Value.z-entry.z).sqrMagnitude).First().Key;
    var reached=new HashSet<Vector2Int>{start};var queue=new Queue<Vector2Int>();queue.Enqueue(start);
    while(queue.Count>0){var at=queue.Dequeue();foreach(var offset in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right}){var next=at+offset;if(free.ContainsKey(next)&&!reached.Contains(next)&&Mathf.Abs(free[next].y-free[at].y)<.94f){reached.Add(next);queue.Enqueue(next);}}}
    report.walkableSamples=free.Count;report.reachableSamples=reached.Count;
    foreach(var city in layout.cities)if(reached.Any(key=>Vector2.Distance(new Vector2(free[key].x-home.x,free[key].z-home.z),new Vector2(city.x,city.z))<=city.radius+4))report.reachableCities++;
    report.fullSizeNavigation=report.reachableCities==layout.cities.Length;if(!report.fullSizeNavigation){File.WriteAllText(Path.Combine(dir,"capacity-failed.json"),JsonUtility.ToJson(report,true));throw new InvalidOperationException("Populated terrain cuts off a reserved city approach: "+report.reachableCities+"/"+layout.cities.Length+", reached "+reached.Count+"/"+free.Count);}
    // Render mesh donors only; no scripts, colliders, hotspots, rewards or save IDs are cloned.
    var donors=world.GetComponentsInChildren<MeshRenderer>(true);
    foreach(var i in fstate.instances) {
     string prefix=i.content=="forest-valoria"?"Reference existing wood grove":i.content=="quarry-valoria"?"Reference existing quarry":null;
     var donor=prefix==null?null:donors.FirstOrDefault(r=>r.name.StartsWith(prefix,StringComparison.Ordinal));
     if(donor) {
      var mf=donor.GetComponent<MeshFilter>();var foot=donor.transform.TransformPoint(mf.sharedMesh.vertices[0]);
      var go=new GameObject("TEST ONLY "+i.id);displays.Add(go);go.layer=root.layer;go.transform.SetParent(root.transform,true);go.transform.SetPositionAndRotation(donor.transform.position+sites[i.site].position-foot+Vector3.up*.08f,donor.transform.rotation);go.transform.localScale=donor.transform.lossyScale;
      go.transform.position+=sites[i.site].position+Vector3.up*.08f-go.transform.TransformPoint(mf.sharedMesh.vertices[0]);
      var markerMesh=UnityEngine.Object.Instantiate(mf.sharedMesh);meshes.Add(markerMesh);
      var vertices=markerMesh.vertices;
      for(int k=0;k<vertices.Length;k++){
       var point=go.transform.TransformPoint(vertices[k]);
       if(!Sample(point.x-home.x,point.z-home.z,out var support))throw new InvalidOperationException("Resource footprint outside terrain");
       point.y=support.point.y+.08f;vertices[k]=go.transform.InverseTransformPoint(point);
       report.maxContactError=Mathf.Max(report.maxContactError,Mathf.Abs(go.transform.TransformPoint(vertices[k]).y-support.point.y-.08f));
      }
      markerMesh.vertices=vertices;go.AddComponent<MeshFilter>().sharedMesh=markerMesh;go.AddComponent<MeshRenderer>().sharedMaterials=donor.sharedMaterials;
     } else {
      // Bake the actual approved skeleton in world coordinates, matching TryGeometryBounds.
      // BakeMesh on an arbitrary renderer included a flattened non-model donor in the rejected trial.
      var go=new GameObject("TEST ONLY "+i.id);displays.Add(go);go.layer=root.layer;go.transform.SetParent(root.transform,true);go.transform.position=sites[i.site].position+Vector3.up*.08f;go.AddComponent<MeshFilter>().sharedMesh=enemyMesh;var rr=go.AddComponent<MeshRenderer>();rr.sharedMaterials=enemyMaterials;enemyDisplays.Add(go);
      float required=float.NegativeInfinity;
      foreach(var vertex in enemyMesh.vertices.Where(v=>v.y<enemyMesh.bounds.min.y+.25f)){var foot=go.transform.TransformPoint(vertex);if(!Sample(foot.x-home.x,foot.z-home.z,out var ground))throw new InvalidOperationException("Enemy foot outside terrain");required=Mathf.Max(required,ground.point.y+.08f-foot.y);}
      go.transform.position+=Vector3.up*required;
      float contact=float.PositiveInfinity;foreach(var vertex in enemyMesh.vertices.Where(v=>v.y<enemyMesh.bounds.min.y+.25f)){var foot=go.transform.TransformPoint(vertex);Sample(foot.x-home.x,foot.z-home.z,out var ground);contact=Mathf.Min(contact,foot.y-ground.point.y-.08f);}report.maxContactError=Mathf.Max(report.maxContactError,Mathf.Abs(contact));
     }
    }
    report.dynamicNearTriangles=displays.Sum(go=>go.GetComponent<MeshFilter>()?go.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3:0);report.dynamicFarTriangles=report.dynamicNearTriangles-enemyDisplays.Count*(enemyMesh.triangles.Length-enemyFarMesh.triangles.Length)/3;
    clearanceMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit"));clearanceMaterial.color=new Color(.2f,.8f,.9f,1);
    foreach(var city in layout.cities){
     var go=new GameObject("TEST ONLY city clearance");displays.Add(go);go.layer=root.layer;go.transform.SetParent(root.transform,true);var line=go.AddComponent<LineRenderer>();line.sharedMaterial=clearanceMaterial;line.widthMultiplier=.16f;line.loop=true;line.positionCount=48;line.useWorldSpace=true;
     for(int k=0;k<48;k++){float a=k*Mathf.PI*2/48;float x=city.x+Mathf.Cos(a)*city.radius,z=city.z+Mathf.Sin(a)*city.radius;if(!Sample(x,z,out var hit))throw new InvalidOperationException("City perimeter outside terrain");line.SetPosition(k,hit.point+Vector3.up*.10f);}
    }
    if(report.maxContactError>.03f)throw new InvalidOperationException("Population floats above actual mesh");
    foreach(bool populated in new[]{false,true})foreach(bool portrait in new[]{false,true})for(int level=0;level<3;level++) {
     foreach(var go in displays)go.SetActive(populated);
     var target=home+new Vector3(0,4,level==0?200:level==1?(portrait?170:180):(portrait?135:160));camera.transform.position=target+new Vector3(0,130,-145);camera.transform.LookAt(target);camera.orthographicSize=(portrait?new[]{32f,52f,74f}:new[]{24f,38f,52f})[level];
     float fade=Mathf.SmoothStep(0,1,Mathf.InverseLerp(37,47,camera.orthographicSize));
     foreach(var rr in root.GetComponentsInChildren<MeshRenderer>())if(rr.name=="Pilot_NearBatch"||rr.name=="Pilot_FarBatch"){rr.enabled=rr.name=="Pilot_NearBatch"?fade<.999f:fade>0;foreach(var material in rr.sharedMaterials)material.SetFloat("_Fade",fade);}
     foreach(var enemy in enemyDisplays){enemy.GetComponent<MeshFilter>().sharedMesh=level==2?enemyFarMesh:enemyMesh;enemy.GetComponent<MeshRenderer>().sharedMaterials=level==2?enemyFarMaterials:enemyMaterials;}
     Frame(camera,Path.Combine(dir,(populated?"TEST-population-":"geography-")+(portrait?"portrait-":"landscape-")+new[]{"close","regional","panorama"}[level]),portrait?720:1280,portrait?1280:720);
    }
    report.originalSaveUntouched=playerBefore==JsonUtility.ToJson(host.City.Gateway.Snapshot());if(!report.originalSaveUntouched)throw new InvalidOperationException("Population fixture changed player state");
    File.WriteAllText(Path.Combine(dir,"capacity.json"),JsonUtility.ToJson(report,true));
   } catch(Exception){File.WriteAllText(Path.Combine(dir,"capacity-failed.json"),JsonUtility.ToJson(report,true));throw;} finally {foreach(var go in displays)UnityEngine.Object.DestroyImmediate(go);foreach(var go in temporary)UnityEngine.Object.DestroyImmediate(go);foreach(var m in meshes)UnityEngine.Object.DestroyImmediate(m);if(clearanceMaterial)UnityEngine.Object.DestroyImmediate(clearanceMaterial);camera.transform.SetPositionAndRotation(oldPosition,oldRotation);camera.orthographicSize=oldZoom;camera.aspect=oldAspect;camera.targetTexture=oldTarget;RenderTexture.active=oldActive;}
  }
  static Mesh BakeVisual(Renderer[] renderers,out Material[] materials) {
   var pieces=new List<Mesh>();var combines=new List<CombineInstance>();var mats=new List<Material>();var vertices=new List<Vector3>();
   try {
    foreach(var renderer in renderers){
     var skin=renderer as SkinnedMeshRenderer;var source=skin?skin.sharedMesh:renderer.GetComponent<MeshFilter>().sharedMesh;var clone=UnityEngine.Object.Instantiate(source);pieces.Add(clone);var points=source.vertices;
     if(skin){var weights=source.boneWeights;var matrices=skin.bones.Select((b,k)=>b.localToWorldMatrix*source.bindposes[k]).ToArray();for(int k=0;k<points.Length;k++){var w=weights[k];var v=points[k];points[k]=matrices[w.boneIndex0].MultiplyPoint3x4(v)*w.weight0+matrices[w.boneIndex1].MultiplyPoint3x4(v)*w.weight1+matrices[w.boneIndex2].MultiplyPoint3x4(v)*w.weight2+matrices[w.boneIndex3].MultiplyPoint3x4(v)*w.weight3;}}
     else for(int k=0;k<points.Length;k++)points[k]=renderer.transform.TransformPoint(points[k]);
     clone.vertices=points;vertices.AddRange(points);
     for(int sub=0;sub<source.subMeshCount;sub++){combines.Add(new CombineInstance{mesh=clone,subMeshIndex=sub,transform=Matrix4x4.identity});mats.Add(renderer.sharedMaterials[Mathf.Min(sub,renderer.sharedMaterials.Length-1)]);}
    }
    if(vertices.Count==0)throw new InvalidOperationException("No approved visual geometry");var bounds=new Bounds(vertices[0],Vector3.zero);foreach(var v in vertices)bounds.Encapsulate(v);var foot=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
    var result=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};result.CombineMeshes(combines.ToArray(),false,true);result.vertices=result.vertices.Select(v=>v-foot).ToArray();result.RecalculateBounds();result.RecalculateNormals();materials=mats.ToArray();return result;
   }finally{foreach(var p in pieces)UnityEngine.Object.DestroyImmediate(p);}
  }
  [Serializable] class CaptureMetadata {public Vector3 position;public Quaternion rotation;public float orthographicSize;public int width,height;public bool playMode;public string scope="TEST ONLY, actual Unity Camera.Render";}
  static void Frame(Camera camera,string path,int w,int h) {
   var rt=new RenderTexture(w,h,24);var image=new Texture2D(w,h,TextureFormat.RGB24,false);
   try{camera.aspect=w/(float)h;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,w,h),0,0);image.Apply();File.WriteAllBytes(path+".png",image.EncodeToPNG());File.WriteAllText(path+".json",JsonUtility.ToJson(new CaptureMetadata{position=camera.transform.position,rotation=camera.transform.rotation,orthographicSize=camera.orthographicSize,width=w,height=h,playMode=UnityEngine.Application.isPlaying},true));}
   finally{camera.targetTexture=null;RenderTexture.active=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
  }
 }
}
