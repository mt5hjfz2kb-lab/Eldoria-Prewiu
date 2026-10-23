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
  [Serializable] public class Site { public int index; public Vector3 position; public float slope; public bool wood,stone; }
  [Serializable] public class Instance { public string id,content; public int site,generation; public long due; public bool active=true; }
  [Serializable] public class Fixture { public string authority="TEST_ONLY_NOT_LIVE_WORLD"; public long revision; public List<Instance> instances=new List<Instance>(); }
  [Serializable] public class Report {
   public string scope="Disposable capacity and relocation fixture; NOT production respawn or network multiplayer";
   public int validSites,instances,citySpaces,routeSamples,respawned,duplicateCount,meshHits;
   public float maxContactError,maxCityRelief;public float testMarkerScale=.30f;public bool saveReloadStable,repeatedTickIdempotent,occupiedRejected,changedPosition,originalSaveUntouched,physicalDevice=false;
   public Site[] sites;public Point[] cities; public Fixture population;
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
  static int Choose(Site[] sites,string content,int bastion,HashSet<int> occupied,int old,int start) {
   for(int n=0;n<sites.Length;n++){int k=(start+n)%sites.Length;var s=sites[k];if(s.index!=old&&!occupied.Contains(s.index)&&Allowed(s,content,bastion))return s.index;}return -1;
  }
  static void TickFixture(Fixture f,Site[] sites,long now) {
   var occupied=new HashSet<int>(f.instances.Where(i=>i.active).Select(i=>i.site));
   foreach(var i in f.instances.Where(i=>!i.active&&i.due<=now).OrderBy(i=>i.id,StringComparer.Ordinal)) {
    int next=Choose(sites,i.content,2,occupied,i.site,(i.site+37+i.generation)%sites.Length);
    if(next<0)continue;i.site=next;i.generation++;i.active=true;i.due=0;occupied.Add(next);f.revision++;
   }
  }
  public static void Capture(Camera camera,GameObject root,Transform world,Vector3 home,string output) {
   var asset=Resources.Load<TextAsset>("World4X/Pilot/LivingSectorLayout");if(!asset)return;
   var layout=JsonUtility.FromJson<Layout>(asset.text);var dir=Path.Combine(output,"living-sector");Directory.CreateDirectory(dir);
   var temporary=new List<GameObject>();var surfaces=new List<MeshCollider>();var displays=new List<GameObject>();var meshes=new List<Mesh>();Material clearanceMaterial=null;Mesh enemyMesh=null;
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
    var sites=new List<Site>();
    for(float z=176;z<=246;z+=5)for(float x=-90;x<=90;x+=5) {
     if(layout.cities.Any(c=>Distance(x,z,c)<c.radius+4)||Road(layout,x,z)<4||layout.trees.Any(t=>Distance(x,z,t)<t.radius+2))continue;
     if(!Sample(x,z,out var hit)||Vector3.Angle(hit.normal,Vector3.up)>20)continue;
     bool footprint=true;foreach(var d in new[]{new Vector2(-1.5f,0),new Vector2(1.5f,0),new Vector2(0,-1.5f),new Vector2(0,1.5f)})if(!Sample(x+d.x,z+d.y,out var h)||Mathf.Abs(h.point.y-hit.point.y)>.6f)footprint=false;
     if(!footprint)continue;
     float treeDistance=layout.trees.Min(t=>Distance(x,z,t));
     sites.Add(new Site{index=sites.Count,position=hit.point,slope=Vector3.Angle(hit.normal,Vector3.up),wood=treeDistance<27,stone=x>20||treeDistance>22});
    }
    report.sites=sites.ToArray();report.validSites=sites.Count;
    if(sites.Count<150)throw new InvalidOperationException("Insufficient actual-mesh capacity: "+sites.Count);
    // City footprints are reserved empty ground, not decorative city models or player settlements.
    foreach(var c in layout.cities) {
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
     int steps=Mathf.CeilToInt(Vector2.Distance(a,b));for(int n=1;n<steps;n++)foreach(float offset in new[]{-1f,0f,1f}) {
      var p=Vector2.Lerp(a,b,n/(float)steps)+side*offset;if(p.x<=-95||p.x>=95||p.y<=171||p.y>=249)continue;
      if(!Sample(p.x,p.y,out var h)||Vector3.Angle(h.normal,Vector3.up)>25||layout.trees.Any(t=>Distance(p.x,p.y,t)<t.radius))throw new InvalidOperationException("Blocked march corridor");report.routeSamples++;
     }
    }
    var fstate=new Fixture();var occupied=new HashSet<int>();
    foreach(string content in new[]{"forest-valoria","quarry-valoria","engendro-valoria"})for(int n=0;n<40;n++) {
     int k=Choose(report.sites,content,2,occupied,-1,n*11%sites.Count);if(k<0)throw new InvalidOperationException("Insufficient biome capacity: "+content);
     occupied.Add(k);fstate.instances.Add(new Instance{id="qa:"+content+":"+n,content=content,site=k});
    }
    report.instances=fstate.instances.Count;
    if(Choose(report.sites,"engendro-valoria",1,new HashSet<int>(),-1,0)!=-1)throw new InvalidOperationException("Progression exclusion failed");
    report.occupiedRejected=Choose(report.sites,"corrupt-scout",2,new HashSet<int>(sites.Select(s=>s.index)),-1,0)==-1;
    var depleted=fstate.instances.Where((i,n)=>n%4==0).ToArray();var oldSites=depleted.ToDictionary(i=>i.id,i=>i.site);
    foreach(var i in depleted){i.active=false;i.due=100;}fstate.revision++;
    string saved=JsonUtility.ToJson(fstate);File.WriteAllText(Path.Combine(dir,"fixture-before-respawn.json"),saved);
    fstate=JsonUtility.FromJson<Fixture>(File.ReadAllText(Path.Combine(dir,"fixture-before-respawn.json")));report.saveReloadStable=JsonUtility.ToJson(fstate)==saved;
    TickFixture(fstate,report.sites,99);if(fstate.instances.Count(i=>!i.active)!=depleted.Length)throw new InvalidOperationException("Premature respawn");
    TickFixture(fstate,report.sites,100);report.respawned=fstate.instances.Count(i=>i.generation==1);report.changedPosition=fstate.instances.Where(i=>oldSites.ContainsKey(i.id)).All(i=>i.site!=oldSites[i.id]);
    string after=JsonUtility.ToJson(fstate);fstate=JsonUtility.FromJson<Fixture>(after);TickFixture(fstate,report.sites,100);report.repeatedTickIdempotent=after==JsonUtility.ToJson(fstate);
    report.duplicateCount=fstate.instances.Count-fstate.instances.Select(i=>i.id).Distinct().Count()+fstate.instances.Count(i=>i.active)-fstate.instances.Where(i=>i.active).Select(i=>i.site).Distinct().Count();
    if(report.respawned!=30||!report.changedPosition||!report.saveReloadStable||!report.repeatedTickIdempotent||report.duplicateCount!=0||!report.occupiedRejected)throw new InvalidOperationException("Capacity fixture state invariant failed");
    report.population=fstate;
    // Render mesh donors only; no scripts, colliders, hotspots, rewards or save IDs are cloned.
    var donors=world.GetComponentsInChildren<MeshRenderer>(true);
    foreach(var i in fstate.instances) {
     string prefix=i.content=="forest-valoria"?"Reference existing wood grove":i.content=="quarry-valoria"?"Reference existing quarry":null;
     var donor=prefix==null?null:donors.FirstOrDefault(r=>r.name.StartsWith(prefix,StringComparison.Ordinal));
     if(donor) {
      var mf=donor.GetComponent<MeshFilter>();var foot=donor.transform.TransformPoint(mf.sharedMesh.vertices[0]);
      var go=new GameObject("TEST ONLY "+i.id);displays.Add(go);go.layer=root.layer;go.transform.SetParent(root.transform,true);go.transform.SetPositionAndRotation(donor.transform.position+sites[i.site].position-foot+Vector3.up*.08f,donor.transform.rotation);go.transform.localScale=donor.transform.lossyScale*.30f;
      go.transform.position+=sites[i.site].position+Vector3.up*.08f-go.transform.TransformPoint(mf.sharedMesh.vertices[0]);
      var markerMesh=UnityEngine.Object.Instantiate(mf.sharedMesh);meshes.Add(markerMesh);markerMesh.uv2=markerMesh.uv2.Select(v=>v*.30f).ToArray();
      var vertices=markerMesh.vertices;
      for(int k=0;k<vertices.Length;k++){
       var point=go.transform.TransformPoint(vertices[k]);
       if(!Sample(point.x-home.x,point.z-home.z,out var support))throw new InvalidOperationException("Resource footprint outside terrain");
       point.y=support.point.y+.08f;vertices[k]=go.transform.InverseTransformPoint(point);
       report.maxContactError=Mathf.Max(report.maxContactError,Mathf.Abs(go.transform.TransformPoint(vertices[k]).y-support.point.y-.08f));
      }
      markerMesh.vertices=vertices;go.AddComponent<MeshFilter>().sharedMesh=markerMesh;go.AddComponent<MeshRenderer>().sharedMaterials=donor.sharedMaterials;
     } else {
      // Existing animated Spawnling's measured pose, baked once per fixture specimen; no new monster.
      var actor=world.GetComponentsInChildren<SkinnedMeshRenderer>(true).FirstOrDefault(r=>r.name.ToLowerInvariant().Contains("spawn")||r.transform.GetComponentsInParent<MonoBehaviour>(true).Any(c=>c&&c.GetType().Name=="SpawnlingEncounterVisual"));
      if(!actor)throw new InvalidOperationException("Existing enemy mesh unavailable for capacity display");
      if(!enemyMesh){enemyMesh=new Mesh();actor.BakeMesh(enemyMesh);meshes.Add(enemyMesh);}var mesh=enemyMesh;var go=new GameObject("TEST ONLY "+i.id);displays.Add(go);go.layer=root.layer;go.transform.SetParent(root.transform,true);go.transform.SetPositionAndRotation(actor.transform.position,actor.transform.rotation);go.transform.localScale=actor.transform.lossyScale*.30f;go.AddComponent<MeshFilter>().sharedMesh=mesh;var rr=go.AddComponent<MeshRenderer>();rr.sharedMaterials=actor.sharedMaterials;go.transform.position+=sites[i.site].position-new Vector3(rr.bounds.center.x,rr.bounds.min.y,rr.bounds.center.z)+Vector3.up*.08f;
      report.maxContactError=Mathf.Max(report.maxContactError,Mathf.Abs(rr.bounds.min.y-sites[i.site].position.y-.08f));
     }
    }
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
     Frame(camera,Path.Combine(dir,(populated?"TEST-population-":"geography-")+(portrait?"portrait-":"landscape-")+new[]{"close","regional","panorama"}[level]),portrait?720:1280,portrait?1280:720);
    }
    report.originalSaveUntouched=playerBefore==JsonUtility.ToJson(host.City.Gateway.Snapshot());if(!report.originalSaveUntouched)throw new InvalidOperationException("Population fixture changed player state");
    File.WriteAllText(Path.Combine(dir,"capacity.json"),JsonUtility.ToJson(report,true));
   } finally {foreach(var go in displays)UnityEngine.Object.DestroyImmediate(go);foreach(var go in temporary)UnityEngine.Object.DestroyImmediate(go);foreach(var m in meshes)UnityEngine.Object.DestroyImmediate(m);if(clearanceMaterial)UnityEngine.Object.DestroyImmediate(clearanceMaterial);camera.transform.SetPositionAndRotation(oldPosition,oldRotation);camera.orthographicSize=oldZoom;camera.aspect=oldAspect;camera.targetTexture=oldTarget;RenderTexture.active=oldActive;}
  }
  static void Frame(Camera camera,string path,int w,int h) {
   var rt=new RenderTexture(w,h,24);var image=new Texture2D(w,h,TextureFormat.RGB24,false);
   try{camera.aspect=w/(float)h;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,w,h),0,0);image.Apply();File.WriteAllBytes(path+".png",image.EncodeToPNG());}
   finally{camera.targetTexture=null;RenderTexture.active=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
  }
 }
}
