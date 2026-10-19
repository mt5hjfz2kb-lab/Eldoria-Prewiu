#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eldoria.Domain;
using Eldoria.Presentation;
using GLTFast;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Eldoria.ReferenceClean.Editor {
 /// <summary>
 /// Finishes the currently open integrated scene. It never opens an older city
 /// scene or rebuilds gameplay. Model sources, parcel bindings and hotspot IDs
 /// remain the existing ones; only explicit visual copies are replaced.
 /// </summary>
 public static class ValoriaEnvironmentFinish {
  const string Sources = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources/EnvironmentFinish";
  const string AssetsRoot = "Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/EnvironmentFinish";
  const string Marker = "Environment finish / retained integrated base v1";
  static string folder;
  static int assetIndex;
  static readonly Dictionary<Object, Object> persisted = new Dictionary<Object, Object>();
  static readonly Dictionary<Material, Material> hostileMaterials = new Dictionary<Material, Material>();

  [Serializable] public sealed class ShoreGap { public float x, width, scale; }
  [Serializable] public sealed class ArtistPlan { public string rationale; public float city_path_width; public ShoreGap[] shore_gaps; }
  [Serializable] sealed class Frame { public string view, before, after; public int width, height; public float[] camera; }
  [Serializable] sealed class Report {
   public string source_sha, scene, source_scene_sha256, artist_plan_sha256, author_result_sha256;
   public bool current_scene_only, same_camera_light, terrain_heights_preserved, parcel_bindings_preserved,
    hotspot_centres_preserved, source_meshes_preserved, art_approved;
   public int city_painted_cells, world_painted_pixels, shoreline_stones_reused, world_trees_reused, replaced_world_destinations;
   public Frame[] frames;
   public string[] limitations;
  }
  struct CameraState {
   public Vector3 position; public Quaternion rotation; public float size, aspect; public bool enabled;
   public CameraState(Camera camera) { position=camera.transform.position; rotation=camera.transform.rotation; size=camera.orthographicSize; aspect=camera.aspect; enabled=camera.enabled; }
   public void Restore(Camera camera) { camera.transform.SetPositionAndRotation(position,rotation); camera.orthographicSize=size; camera.aspect=aspect; camera.enabled=enabled; }
  }
  static void Need(bool ok, string message) { if(!ok) throw new InvalidOperationException("Environment finish: "+message); }
  static string Hash(string path) { using(var h=System.Security.Cryptography.SHA256.Create()) return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant(); }
  static string LightStamp() { var sun=RenderSettings.sun; return (sun?sun.transform.localToWorldMatrix.ToString("R")+sun.intensity.ToString("R")+sun.color.ToString("R"):"none")+RenderSettings.ambientMode+RenderSettings.ambientSkyColor.ToString("R")+RenderSettings.ambientEquatorColor.ToString("R")+RenderSettings.ambientGroundColor.ToString("R"); }
  static Bounds BoundsOf(GameObject go) {
   var renderers=go.GetComponentsInChildren<Renderer>(true).Where(r=>r.GetComponent<MeshFilter>()!=null).ToArray();
   Need(renderers.Length>0,"No mesh in "+go.name); var result=renderers[0].bounds;
   foreach(var renderer in renderers.Skip(1)) result.Encapsulate(renderer.bounds); return result;
  }
  static Transform Find(GameObject root, string name) { return root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name==name); }
  static GameObject Target(ValoriaIntegratedWorld host,string id) {
   var value=Find(host.WorldRoot,"Zone1 target "+id); Need(value,"Retained destination missing: "+id); return value.gameObject;
  }

  public static async Task ApplyAsync(ValoriaReferenceCleanGameplay city, ValoriaIntegratedWorld host, string evidenceDir) {
   Need(city&&host&&host.WorldRoot&&host.WorldCamera&&city.PlayerCamera,"Current integrated bindings required");
   Need(city.BastionOne&&city.BastionTwo&&city.Parcels.LeftBuilt&&city.Parcels.RightBuilt,"Retained I–II models required");
   if(Find(host.WorldRoot,Marker)) { Debug.Log("VALORIA_ENVIRONMENT_ALREADY_APPLIED_CURRENT_SCENE"); return; }
   var planPath=Path.Combine(evidenceDir,"environment-finish-plan.json");
   var authorPath=Path.Combine(evidenceDir,"environment-finish-result.json");
   Need(File.Exists(planPath)&&File.Exists(authorPath),"Current local artist plan and execution evidence required");
   var plan=JsonUtility.FromJson<ArtistPlan>(File.ReadAllText(planPath));
   Need(plan!=null&&plan.city_path_width>=1.5f&&plan.city_path_width<=2.2f&&plan.shore_gaps!=null,"Invalid authored scene parameters");
   Need(File.Exists(Sources+"/EnvironmentForest.glb")&&File.Exists(Sources+"/EnvironmentQuarry.glb"),"Current artist exports missing");
   Directory.CreateDirectory(evidenceDir);
   folder=AssetsRoot+"/Run-"+(Environment.GetEnvironmentVariable("GITHUB_RUN_ID")??"local");
   Directory.CreateDirectory(folder); AssetDatabase.Refresh(); assetIndex=0; persisted.Clear(); hostileMaterials.Clear();

   var terrain=Object.FindObjectsByType<Terrain>(FindObjectsInactive.Include,FindObjectsSortMode.None)
    .FirstOrDefault(t=>!t.transform.IsChildOf(host.WorldRoot.transform));
   Need(terrain&&terrain.terrainData,"Current city terrain missing");
   var terrainBefore=terrain.terrainData;
   var leftBinding=city.Parcels.LeftBuilt; var rightBinding=city.Parcels.RightBuilt;
   var anchors=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true).ToDictionary(h=>h,h=>h.transform.position);
   var cityCameraState=new CameraState(city.PlayerCamera); var worldCameraState=new CameraState(host.WorldCamera);
   string beforeLight=LightStamp();
   bool wasWorldActive=host.WorldRoot.activeSelf;
   var frames=new Dictionary<string,Frame>();
   var report=new Report { source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"), scene=city.gameObject.scene.path,
    source_scene_sha256=Hash(city.gameObject.scene.path),
    artist_plan_sha256=Hash(planPath),author_result_sha256=Hash(authorPath),current_scene_only=true,
    source_meshes_preserved=true,art_approved=false,
    limitations=new[]{"Actual Unity images require independent visual review; this editor pass does not confer ART_APPROVED.",
     "Physical Android/iPhone performance is not measured by editor captures.","Existing light bake and scene heights retained; moved decorative models use realtime lighting and probes."} };
   try {
    city.Apply(new PlayerState { BastionLevel=2,SawmillLevel=1,BarracksLevel=1 });
    CaptureViews(city,host,plan,evidenceDir,"before",frames);
    host.WorldRoot.SetActive(true); Physics.SyncTransforms();
    var marker=new GameObject(Marker); marker.transform.SetParent(host.WorldRoot.transform,false);
    var ground=host.WorldRoot.GetComponentsInChildren<MeshFilter>(true).FirstOrDefault(f=>f.name=="ZoneOne continuous prairie source");
    Need(ground&&ground.sharedMesh,"Current authored world terrain missing");
    var query=ground.gameObject.AddComponent<MeshCollider>(); query.sharedMesh=ground.sharedMesh; Physics.SyncTransforms();
    float Ground(Vector3 p) { return query.Raycast(new Ray(new Vector3(p.x,100,p.z),Vector3.down),out var hit,200)?hit.point.y:p.y; }
    try {
     ReplaceMapValoria(city,host,marker.transform,Ground);
     var forest=await ReplaceAuthoredDestination(host,"forest-valoria","Wood resource existing timber stock",
      Sources+"/EnvironmentForest.glb",18,marker.transform,Ground);
     report.world_trees_reused=UnifyWorldVegetation(host,forest,marker.transform,Ground);
     await ReplaceAuthoredDestination(host,"quarry-valoria","Existing Holotna rock.001",
      Sources+"/EnvironmentQuarry.glb",20,marker.transform,Ground);
     ReplaceEncampments(city,host,marker.transform,Ground);
     report.replaced_world_destinations=5;
     // Supplies without a mechanic are scenery debt, not additional resources.
     foreach(string name in new[]{"Food resource existing supply","Food resource existing store"}) {
      var unused=Find(host.WorldRoot,name); if(unused) unused.gameObject.SetActive(false);
     }
     // Owner decision: no painted road corridors on the 4X world. Marches use\n     // a direct origin-to-target interpolation; preserve the existing prairie.\n     report.world_painted_pixels=0;
    } finally { Object.DestroyImmediate(query); }
    host.WorldRoot.SetActive(false);
    report.city_painted_cells=FinishCityGround(city,terrain,plan);
    // Do not scale embedded bank stones while their foam contact masks still\n    // refer to the old silhouettes. A mismatched shoreline is a visible regression.\n    report.shoreline_stones_reused=0;
    CaptureViews(city,host,plan,evidenceDir,"after",frames);
    report.same_camera_light=beforeLight==LightStamp(); Need(report.same_camera_light,"A/B light changed");
    report.terrain_heights_preserved=terrainBefore.GetHeights(0,0,terrainBefore.heightmapResolution,terrainBefore.heightmapResolution)
     .Cast<float>().SequenceEqual(terrain.terrainData.GetHeights(0,0,terrain.terrainData.heightmapResolution,terrain.terrainData.heightmapResolution).Cast<float>());
    report.parcel_bindings_preserved=city.Parcels.LeftBuilt==leftBinding&&city.Parcels.RightBuilt==rightBinding;
    report.hotspot_centres_preserved=anchors.All(pair=>pair.Key&&pair.Key.transform.position==pair.Value);
    Need(report.terrain_heights_preserved&&report.parcel_bindings_preserved&&report.hotspot_centres_preserved,"Protected current scene binding changed");
    report.frames=frames.Values.ToArray();
    AssetDatabase.SaveAssets();
    File.WriteAllText(Path.Combine(evidenceDir,"environment-finish-unity.json"),JsonUtility.ToJson(report,true));
    Debug.Log("VALORIA_ENVIRONMENT_FINISH_COMPLETE city_cells="+report.city_painted_cells+" shoreline_groups="+report.shoreline_stones_reused);
   } finally {
    city.Apply(new PlayerState());
    if(host.WorldBastionOne) host.WorldBastionOne.SetActive(true);
    if(host.WorldBastionTwo) host.WorldBastionTwo.SetActive(false);
    if(host.WorldSawmill) host.WorldSawmill.SetActive(false);
    host.WorldRoot.SetActive(wasWorldActive); cityCameraState.Restore(city.PlayerCamera); worldCameraState.Restore(host.WorldCamera);
   }
  }


  // Second, idempotent visual treatment of the actual playable I-II scene.
  // Retains all authored meshes and original gameplay, save and hotspot identities.
  public static void ApplyVisualLeap(ValoriaReferenceCleanGameplay city, ValoriaIntegratedWorld host, string evidenceDir) {
   const string leapMarker="Visual leap / integrated landscape v2";
   Need(city&&host&&host.WorldRoot&&host.WorldCamera&&city.PlayerCamera,"Consolidated playable bindings required");
   Need(Find(host.WorldRoot,Marker),"Existing native environment finishing must be retained");
   if(Find(host.WorldRoot,leapMarker)){Debug.Log("VALORIA_REFERENCE_CLEAN_VISUAL_LEAP_ALREADY_APPLIED_COMPLETE");return;}
   var planPath=Path.Combine(evidenceDir,"environment-finish-plan.json");
   Need(File.Exists(planPath),"Previously authored environment plan absent");
   var plan=JsonUtility.FromJson<ArtistPlan>(File.ReadAllText(planPath));
   Need(plan!=null&&plan.shore_gaps!=null&&plan.shore_gaps.Length>0,"Retained local artist plan invalid");
   Directory.CreateDirectory(evidenceDir);
   folder=AssetsRoot+"/VisualLeap-"+(Environment.GetEnvironmentVariable("GITHUB_RUN_ID")??"local");
   Directory.CreateDirectory(folder);AssetDatabase.Refresh();assetIndex=0;
   var terrain=Object.FindObjectsByType<Terrain>(FindObjectsInactive.Include,FindObjectsSortMode.None)
    .FirstOrDefault(t=>!t.transform.IsChildOf(host.WorldRoot.transform));
   Need(terrain&&terrain.terrainData,"Playable city terrain absent");
   var beforeTerrain=terrain.terrainData;
   var left=city.Parcels.LeftBuilt;var right=city.Parcels.RightBuilt;
   var hotspots=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true).ToDictionary(x=>x,x=>x.transform.position);
   var cityCamera=new CameraState(city.PlayerCamera);var worldCamera=new CameraState(host.WorldCamera);
   string light=LightStamp();bool worldWasActive=host.WorldRoot.activeSelf;
   var views=new Dictionary<string,Frame>();
   var report=new Report{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),
    scene=city.gameObject.scene.path,source_scene_sha256=Hash(city.gameObject.scene.path),
    artist_plan_sha256=Hash(planPath),current_scene_only=true,source_meshes_preserved=true,art_approved=false,
    limitations=new[]{"Independent before/after Unity-camera art review still required; counts are not art approval.",
     "Physical mobile GPU/memory performance not measured; unchanged terrain heights, gameplay, targets and state are checked."}};
   try{
    city.Apply(new PlayerState{BastionLevel=2,SawmillLevel=1,BarracksLevel=1});
    CaptureViews(city,host,plan,evidenceDir,"leap-before",views);
    host.WorldRoot.SetActive(true);Physics.SyncTransforms();
    var marker=new GameObject(leapMarker);marker.transform.SetParent(host.WorldRoot.transform,false);
    var ground=host.WorldRoot.GetComponentsInChildren<MeshFilter>(true)
     .FirstOrDefault(x=>x.name=="ZoneOne continuous prairie source");
    Need(ground&&ground.sharedMesh,"Current world prairie mesh absent");
    var collider=ground.GetComponent<MeshCollider>();
    bool temporaryCollider=collider==null;
    if(temporaryCollider)collider=ground.gameObject.AddComponent<MeshCollider>();
    var priorMesh=collider.sharedMesh;
    collider.sharedMesh=ground.sharedMesh;Physics.SyncTransforms();
    try {
     float Height(Vector3 p) {
      return collider.Raycast(new Ray(new Vector3(p.x,160,p.z),Vector3.down),out var hit,320)
       ?hit.point.y:p.y;
     }
     report.world_painted_pixels=FinishWorldSurface(host,ground);
     report.world_trees_reused=DeepenExistingForest(host,marker.transform,Height);
    }finally{
     if(temporaryCollider)Object.DestroyImmediate(collider);
     else collider.sharedMesh=priorMesh;
    }
    host.WorldRoot.SetActive(false);
    report.city_painted_cells=RefineCityLandscape(city,terrain);
    CaptureViews(city,host,plan,evidenceDir,"leap-after",views);
    report.same_camera_light=light==LightStamp();
    var oldHeights=beforeTerrain.GetHeights(0,0,beforeTerrain.heightmapResolution,beforeTerrain.heightmapResolution);
    var newHeights=terrain.terrainData.GetHeights(0,0,terrain.terrainData.heightmapResolution,terrain.terrainData.heightmapResolution);
    report.terrain_heights_preserved=oldHeights.Cast<float>().SequenceEqual(newHeights.Cast<float>());
    report.parcel_bindings_preserved=city.Parcels.LeftBuilt==left&&city.Parcels.RightBuilt==right;
    report.hotspot_centres_preserved=hotspots.All(t=>t.Key&&t.Key.transform.position==t.Value);
    Need(report.same_camera_light&&report.terrain_heights_preserved&&report.parcel_bindings_preserved&&report.hotspot_centres_preserved,
     "Light, terrain heights, parcels or gameplay hotspot positions changed");
    Need(report.city_painted_cells>0&&report.world_painted_pixels>0,"No perceptible ground treatment authored");
    report.frames=views.Values.ToArray();
    AssetDatabase.SaveAssets();
    File.WriteAllText(Path.Combine(evidenceDir,"environment-visual-leap-unity.json"),JsonUtility.ToJson(report,true));
    Debug.Log("VALORIA_REFERENCE_CLEAN_VISUAL_LEAP_COMPLETE city_cells="+report.city_painted_cells+
      " world_pixels="+report.world_painted_pixels+" forest_clumps="+report.world_trees_reused);
   }finally{
    city.Apply(new PlayerState());
    if(host.WorldBastionOne)host.WorldBastionOne.SetActive(true);
    if(host.WorldBastionTwo)host.WorldBastionTwo.SetActive(false);
    if(host.WorldSawmill)host.WorldSawmill.SetActive(false);
    host.WorldRoot.SetActive(worldWasActive);
    cityCamera.Restore(city.PlayerCamera);worldCamera.Restore(host.WorldCamera);
   }
  }


  [Serializable] sealed class PolishReport {
   public string scene,source_sha,source_scene_sha256,original_world_material_guid;
   public bool same_camera_light,terrain_heights_preserved,parcel_bindings_preserved,
    hotspot_centres_preserved,visual_only,art_approved,physical_mobile_tested;
   public int world_pixels_reauthored,city_tree_clumps,city_foundation_stones,quarry_stones,slate_roof_renderers;
   public Frame[] frames;
   public string[] limitations;
  }

  // A second independent, camera-compared art pass; existing I-II gameplay
  // and original artist assets stay untouched and protected.
  public static void ApplyVisualPolish(ValoriaReferenceCleanGameplay city,ValoriaIntegratedWorld host,string evidenceDir){
   const string name="Visual polish / integrated environment v3";
   const string prairieGuid="95a193ec5fe65874189128c68029ccb3";
   Need(city&&host&&host.WorldRoot&&city.PlayerCamera&&host.WorldCamera,"Canonical city/world missing");
   Need(Find(host.WorldRoot,"Visual leap / integrated landscape v2"),"Visual leap stage must precede polish");
   if(Find(host.WorldRoot,name)){Debug.Log("VALORIA_REFERENCE_CLEAN_VISUAL_POLISH_ALREADY_APPLIED_COMPLETE");return;}
   var authorPlan=Path.Combine(evidenceDir,"environment-finish-plan.json");
   Need(File.Exists(authorPlan),"Previously retained local Qwen/Blender plan missing");
   var plan=JsonUtility.FromJson<ArtistPlan>(File.ReadAllText(authorPlan));
   Need(plan!=null&&plan.shore_gaps!=null&&plan.shore_gaps.Length>0,"Artist plan missing");
   var path=AssetDatabase.GUIDToAssetPath(prairieGuid);
   var originalPrairie=AssetDatabase.LoadAssetAtPath<Material>(path);
   Need(originalPrairie!=null&&!string.IsNullOrEmpty(path),"Retained original prairie material unavailable; refuse a replacement");
   var terrain=Object.FindObjectsByType<Terrain>(FindObjectsInactive.Include,FindObjectsSortMode.None)
    .FirstOrDefault(t=>!t.transform.IsChildOf(host.WorldRoot.transform));
   Need(terrain&&terrain.terrainData,"City ground missing");
   var priorTerrain=terrain.terrainData;var left=city.Parcels.LeftBuilt;var right=city.Parcels.RightBuilt;
   var anchors=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true).ToDictionary(h=>h,h=>h.transform.position);
   var originalLight=LightStamp();var cityCam=new CameraState(city.PlayerCamera);var worldCam=new CameraState(host.WorldCamera);
   var wasWorld=host.WorldRoot.activeSelf;
   var frames=new Dictionary<string,Frame>();
   folder=AssetsRoot+"/VisualPolish-"+(Environment.GetEnvironmentVariable("GITHUB_RUN_ID")??"local");
   Directory.CreateDirectory(folder);Directory.CreateDirectory(evidenceDir);AssetDatabase.Refresh();
   assetIndex=0;
   var report=new PolishReport{scene=city.gameObject.scene.path,source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),
    source_scene_sha256=Hash(city.gameObject.scene.path),original_world_material_guid=prairieGuid,
    visual_only=true,art_approved=false,physical_mobile_tested=false,
    limitations=new[]{"Native capture review is distinct from physical Android/iPhone performance.",
     "No new economy, mechanics, scene root, paid art asset or gameplay bindings."}};
   try{
    city.Apply(new PlayerState{BastionLevel=2,SawmillLevel=1,BarracksLevel=1});
    CaptureViews(city,host,plan,evidenceDir,"polish-before",frames);
    host.WorldRoot.SetActive(true);Physics.SyncTransforms();
    var marker=new GameObject(name);marker.transform.SetParent(host.WorldRoot.transform,false);
    var ground=host.WorldRoot.GetComponentsInChildren<MeshFilter>(true)
     .FirstOrDefault(x=>x.name=="ZoneOne continuous prairie source");
    Need(ground&&ground.sharedMesh,"Existing prairie geometry missing");
    var groundRenderer=ground.GetComponent<MeshRenderer>();
    Need(groundRenderer&&groundRenderer.sharedMaterials.Length==1,"Original single-material world terrain missing");
    // Restore the exact pre-leap material, then repaint once at art-approved
    // earth values; avoids compounding the overly dark radial streak.
    groundRenderer.sharedMaterial=originalPrairie;
    var collider=ground.GetComponent<MeshCollider>();
    bool removeCollider=collider==null;
    if(removeCollider)collider=ground.gameObject.AddComponent<MeshCollider>();
    var earlierMesh=collider.sharedMesh;
    collider.sharedMesh=ground.sharedMesh;Physics.SyncTransforms();
    try {
     float Elevation(Vector3 p){
      return collider.Raycast(new Ray(new Vector3(p.x,180,p.z),Vector3.down),out var hit,380)?hit.point.y:p.y;
     }
     report.world_pixels_reauthored=FinishWorldSurface(host,ground);
     report.quarry_stones=ReuseQuarryRimStones(host,marker.transform,Elevation);
     report.slate_roof_renderers=RetuneCurrentRoofs(city,host);
    }finally{if(removeCollider)Object.DestroyImmediate(collider);else collider.sharedMesh=earlierMesh;}
    host.WorldRoot.SetActive(false);
    var cityDecor=new GameObject("Visual polish / city environmental details v3");
    cityDecor.transform.SetParent(city.transform,false);
    var counts=ReuseCityPlantsAndStone(city,host,terrain,cityDecor.transform);
    report.city_tree_clumps=counts.x;report.city_foundation_stones=counts.y;
    CaptureViews(city,host,plan,evidenceDir,"polish-after",frames);
    report.same_camera_light=LightStamp()==originalLight;
    report.terrain_heights_preserved=priorTerrain.GetHeights(0,0,priorTerrain.heightmapResolution,priorTerrain.heightmapResolution)
     .Cast<float>().SequenceEqual(terrain.terrainData.GetHeights(0,0,terrain.terrainData.heightmapResolution,terrain.terrainData.heightmapResolution).Cast<float>());
    report.parcel_bindings_preserved=city.Parcels.LeftBuilt==left&&city.Parcels.RightBuilt==right;
    report.hotspot_centres_preserved=anchors.All(p=>p.Key&&p.Key.transform.position==p.Value);
    Need(report.same_camera_light&&report.terrain_heights_preserved&&report.parcel_bindings_preserved&&report.hotspot_centres_preserved,
     "Protected Unity gameplay bindings, camera light or terrain elevations changed");
    Need(report.world_pixels_reauthored>1500&&report.quarry_stones>=4&&report.city_tree_clumps>=6&&report.city_foundation_stones>=6,
     "Insufficient retained-art physical environment improvement");
    report.frames=frames.Values.ToArray();AssetDatabase.SaveAssets();
    File.WriteAllText(Path.Combine(evidenceDir,"environment-visual-polish-unity.json"),JsonUtility.ToJson(report,true));
    Debug.Log("VALORIA_REFERENCE_CLEAN_VISUAL_POLISH_COMPLETE world_pixels="+report.world_pixels_reauthored+
     " city_trees="+report.city_tree_clumps+" city_stones="+report.city_foundation_stones+
     " quarry_rocks="+report.quarry_stones+" slates="+report.slate_roof_renderers);
   }finally{
    city.Apply(new PlayerState());
    if(host.WorldBastionOne)host.WorldBastionOne.SetActive(true);
    if(host.WorldBastionTwo)host.WorldBastionTwo.SetActive(false);
    if(host.WorldSawmill)host.WorldSawmill.SetActive(false);
    host.WorldRoot.SetActive(wasWorld);cityCam.Restore(city.PlayerCamera);worldCam.Restore(host.WorldCamera);
   }
  }

  static int ReuseQuarryRimStones(ValoriaIntegratedWorld host,Transform parent,Func<Vector3,float> ground){
   var stones=host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true)
    .Where(r=>r.name.StartsWith("Quarry retained embedded rim rock")).ToArray();
   Need(stones.Length>0,"Qwen/Blender-authored limestone donor missing");
   var centre=Target(host,"quarry-valoria").transform.position;
   int count=0;
   for(int i=0;i<13;i++){
    float a=(i*.382f+.17f)*2f*Mathf.PI;
    float d=7.1f+Mathf.PerlinNoise(i*.37f,13.2f)*6.4f;
    var p=centre+new Vector3(Mathf.Cos(a)*d,0,Mathf.Sin(a)*d*.7f);
    p.y=ground(p);
    var donor=stones[i%stones.Length];
    float width=1.1f+Mathf.PerlinNoise(i*.41f+7f,.2f)*1.25f;
    var item=CloneVisual(donor.gameObject,"Zone1 quarry / reclaimed extracted stone "+i,parent,p,width);
    item.transform.rotation=Quaternion.Euler(0,i*131f%360f,0);
    count++;
   }
   return count;
  }

  static Vector2Int ReuseCityPlantsAndStone(ValoriaReferenceCleanGameplay city,ValoriaIntegratedWorld host,Terrain terrain,Transform parent){
   // Native scene uses aggregated "Fir_tree_group" and "River_bank_rocks"
   // as one combined mesh each. Do not mistake the old inactive world LOD
   // children for city models. Reuse the proven individual Qwen/Blender pine
   // and extracted quarry stone already visible in the integrated world.
   var worldRenderers=host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true);
   var tree=worldRenderers.FirstOrDefault(r=>r.name.StartsWith("Forest retained Valoria pine"));
   var rock=worldRenderers.FirstOrDefault(r=>r.name.StartsWith("Quarry retained embedded rim rock"));
   Need(tree&&rock,"Retained integrated single-mesh pine or limestone donor missing");
   var cityBase=BoundsOf(city.BastionTwo).center;
   var work=new[]{BoundsOf(city.Parcels.LeftBuilt),BoundsOf(city.Parcels.RightBuilt)};
   int countTree=0,countRock=0;
   var start=terrain.transform.position;var end=start+terrain.terrainData.size;
   bool Fits(Vector3 p,float clearance){
    if(p.x<start.x+3||p.z<start.z+3||p.x>end.x-3||p.z>end.z-3)return false;
    if(Mathf.Abs(p.x-ValoriaReferenceCleanArtBuilder.RoadX(p.z))<clearance)return false;
    foreach(var b in work)if(Mathf.Abs(p.x-b.center.x)<b.extents.x+clearance&&
     Mathf.Abs(p.z-b.center.z)<b.extents.z+clearance)return false;
    if(Mathf.Abs(p.z-RiverCenter(p.x))<RiverWidth(p.x)+clearance)return false;
    return true;
   }
   Vector3 OnLand(Vector3 p){
    p.y=terrain.SampleHeight(p)+terrain.transform.position.y;
    return p;
   }
   // Cluster at the outskirts, leaving fortress, side-workshop selection and road clear.
   for(int i=0;i<82&&countTree<18;i++){
    float a=(i*.618034f+.11f)*Mathf.PI*2f;
    float rad=23f+Mathf.PerlinNoise(i*.31f,7.6f)*29f;
    var p=cityBase+new Vector3(Mathf.Cos(a)*rad,0,Mathf.Sin(a)*rad*.84f);
    if(!Fits(p,5.8f))continue;
    p=OnLand(p);
    var item=CloneVisual(tree.gameObject,"Valoria wild conifers / reclaimed "+i,parent,p,2.6f+Mathf.PerlinNoise(i*.52f,2.4f)*2.1f);
    item.transform.rotation=Quaternion.Euler(0,(i*137.508f)%360f,0);countTree++;
   }
   // Broken stone / natural boulders adjacent to existing working-yards; no foundations or colliders.
   for(int i=0;i<70&&countRock<22;i++){
    var yard=work[i%2];
    float a=(i*.618034f+.23f)*Mathf.PI*2f;
    float radius=1.15f+Mathf.PerlinNoise(i*.37f,2.8f)*.4f;
    var p=new Vector3(yard.center.x+Mathf.Cos(a)*(yard.extents.x+3.0f)*radius,0,
     yard.center.z+Mathf.Sin(a)*(yard.extents.z+3.0f)*radius);
    if(!Fits(p,2.0f))continue;
    p=OnLand(p);
    var item=CloneVisual(rock.gameObject,"Valoria reclaimed natural stone "+i,parent,p,.85f+Mathf.PerlinNoise(i*.27f,6.6f)*1.3f);
    item.transform.rotation=Quaternion.Euler(0,(i*173f)%360f,0);
    countRock++;
   }
   return new Vector2Int(countTree,countRock);
  }

  static int RetuneCurrentRoofs(ValoriaReferenceCleanGameplay city,ValoriaIntegratedWorld host){
   var roots=new[]{city.BastionOne,city.BastionTwo,city.Parcels.LeftBuilt,city.Parcels.RightBuilt,
    host.WorldBastionOne,host.WorldBastionTwo,host.WorldSawmill};
   var cache=new Dictionary<Material,Material>();int adjusted=0;
   foreach(var root in roots.Where(g=>g!=null)){
    foreach(var mesh in root.GetComponentsInChildren<MeshRenderer>(true)){
     var materials=mesh.sharedMaterials;bool edited=false;
     for(int i=0;i<materials.Length;i++){
      var material=materials[i];if(!material)continue;
      string key=(mesh.name+" "+material.name).ToLowerInvariant();
      if(!new[]{"roof","slate","shingle","tile"}.Any(key.Contains)||
       new[]{"flag","banner","crest","cloth","canvas","wall","floor","stone","foundation"}.Any(key.Contains))continue;
      if(!cache.TryGetValue(material,out var tinted)){
       string property=BaseTextureProperty(material);
       if(property==null)continue;
       var source=ReadableCopy(material.GetTexture(property),1024);
       var pixels=source.GetPixels();int shifted=0;
       for(int j=0;j<pixels.Length;j++){
        Color pixel=pixels[j];Color.RGBToHSV(pixel,out float hue,out float sat,out float value);
        if(sat<.20f||hue<.51f||hue>.72f)continue;
        var slate=new Color(value*.50f,value*.64f,value*.79f,pixel.a);
        pixels[j]=Color.Lerp(pixel,slate,.68f);shifted++;
       }
       if(shifted<pixels.Length*.01f)continue;
       source.SetPixels(pixels);source.Apply(true);Put(source,"recovered-roof-slate-v3");
       tinted=new Material(material){name="Valoria weathered slate / "+material.name};
       tinted.SetTexture(property,source);Put(tinted,"recovered-roof-material-v3");
       cache[material]=tinted;
      }
      materials[i]=tinted;edited=true;
     }
     if(edited){mesh.sharedMaterials=materials;adjusted++;}
    }
   }
   return adjusted;
  }


  [Serializable] sealed class VisualPaletteReport {
   public string scene,source_sha,source_scene_sha256,baseline_water;
   public bool same_camera_light,terrain_heights_preserved,parcel_bindings_preserved,
    hotspot_centres_preserved,visual_only,art_approved,physical_mobile_tested;
   public int slate_materials_reused,slate_renderers_modified,blue_roof_pixels_regraded,water_materials_reused;
   public Frame[] frames;
   public string[] limitations;
  }

  // V4 works on the integrated game only, regrading existing real source materials.
  // It creates no new geometry, changes no light, and touches no game progression.
  public static void ApplyVisualPalette(ValoriaReferenceCleanGameplay city,ValoriaIntegratedWorld host,string evidenceDir){
   const string markerName="Visual palette / slate and river v4";
   Need(city&&host&&host.WorldRoot&&host.WorldCamera&&city.PlayerCamera,"Integrated player scene missing");
   Need(Find(host.WorldRoot,"Visual polish / integrated environment v3"),"Prior validated visual pass missing");
   if(Find(host.WorldRoot,markerName)){Debug.Log("VALORIA_REFERENCE_CLEAN_VISUAL_PALETTE_ALREADY_APPLIED_COMPLETE");return;}
   string planFile=Path.Combine(evidenceDir,"environment-finish-plan.json");
   Need(File.Exists(planFile),"Previously retained local artist plan missing");
   var plan=JsonUtility.FromJson<ArtistPlan>(File.ReadAllText(planFile));
   Need(plan!=null&&plan.shore_gaps!=null&&plan.shore_gaps.Length>0,"Artist plan invalid");
   var terrain=Object.FindObjectsByType<Terrain>(FindObjectsInactive.Include,FindObjectsSortMode.None)
    .FirstOrDefault(t=>!t.transform.IsChildOf(host.WorldRoot.transform));
   Need(terrain&&terrain.terrainData,"Retained city terrain missing");
   var heightSource=terrain.terrainData;
   var cityLeft=city.Parcels.LeftBuilt;var cityRight=city.Parcels.RightBuilt;
   var hotspots=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true).ToDictionary(t=>t,t=>t.transform.position);
   var cityCamera=new CameraState(city.PlayerCamera);var worldCamera=new CameraState(host.WorldCamera);
   bool wasWorldActive=host.WorldRoot.activeSelf;string lightSource=LightStamp();
   Directory.CreateDirectory(evidenceDir);
   folder=AssetsRoot+"/VisualPalette-"+(Environment.GetEnvironmentVariable("GITHUB_RUN_ID")??"local");
   Directory.CreateDirectory(folder);AssetDatabase.Refresh();assetIndex=0;
   var frames=new Dictionary<string,Frame>();
   var report=new VisualPaletteReport {scene=city.gameObject.scene.path,
    source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),
    source_scene_sha256=Hash(city.gameObject.scene.path),
    visual_only=true,art_approved=false,physical_mobile_tested=false,
    limitations=new[]{"Artistic acceptance needs inspection of the genuine before/after images at the player camera.",
     "Physical iOS/Android device performance and publication are NOT implied by a native editor success."}};
   try{
    city.Apply(new PlayerState {BastionLevel=2,SawmillLevel=1,BarracksLevel=1});
    CaptureViews(city,host,plan,evidenceDir,"palette-before",frames);
    host.WorldRoot.SetActive(true);Physics.SyncTransforms();
    var marker=new GameObject(markerName);marker.transform.SetParent(host.WorldRoot.transform,false);
    RegradeExistingSlate(city,host,report);
    var water=Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)
     .FirstOrDefault(r=>r.name=="River water"&&!r.transform.IsChildOf(host.WorldRoot.transform));
    Need(water&&water.sharedMaterials.Length==1&&water.sharedMaterial,"Retained river material unavailable");
    var originalWater=water.sharedMaterial;
    Need(originalWater.HasProperty("_BaseColor")&&originalWater.HasProperty("_ShallowColor"),
     "Retained URP water shader colour interface changed");
    report.baseline_water=originalWater.name;
    var river=new Material(originalWater){name="Valoria river / shaded deep teal v4"};
    river.SetColor("_BaseColor",new Color(.036f,.207f,.265f,1));
    river.SetColor("_ShallowColor",new Color(.068f,.315f,.355f,1));
    if(river.HasProperty("_ReflectionStrength"))river.SetFloat("_ReflectionStrength",.69f);
    Put(river,"retained-river-deep-teal");
    water.sharedMaterial=river;report.water_materials_reused=1;
    host.WorldRoot.SetActive(false);
    CaptureViews(city,host,plan,evidenceDir,"palette-after",frames);
    report.same_camera_light=LightStamp()==lightSource;
    report.terrain_heights_preserved=heightSource.GetHeights(0,0,heightSource.heightmapResolution,heightSource.heightmapResolution)
     .Cast<float>().SequenceEqual(terrain.terrainData.GetHeights(0,0,terrain.terrainData.heightmapResolution,terrain.terrainData.heightmapResolution).Cast<float>());
    report.parcel_bindings_preserved=city.Parcels.LeftBuilt==cityLeft&&city.Parcels.RightBuilt==cityRight;
    report.hotspot_centres_preserved=hotspots.All(k=>k.Key&&k.Key.transform.position==k.Value);
    Need(report.same_camera_light&&report.terrain_heights_preserved&&report.parcel_bindings_preserved&&report.hotspot_centres_preserved,
     "Game bindings, terrain elevation, destination centres or comparison illumination changed");
    Need(report.water_materials_reused==1&&report.slate_materials_reused>0,
     "Native targeted water or roof material finishing not established");
    report.frames=frames.Values.ToArray();AssetDatabase.SaveAssets();
    File.WriteAllText(Path.Combine(evidenceDir,"environment-visual-palette-unity.json"),JsonUtility.ToJson(report,true));
    Debug.Log("VALORIA_REFERENCE_CLEAN_VISUAL_PALETTE_COMPLETE slate_materials="+report.slate_materials_reused+
     " roof_pixels="+report.blue_roof_pixels_regraded+" river="+report.water_materials_reused);
   }finally{
    city.Apply(new PlayerState());
    if(host.WorldBastionOne)host.WorldBastionOne.SetActive(true);
    if(host.WorldBastionTwo)host.WorldBastionTwo.SetActive(false);
    if(host.WorldSawmill)host.WorldSawmill.SetActive(false);
    host.WorldRoot.SetActive(wasWorldActive);
    cityCamera.Restore(city.PlayerCamera);worldCamera.Restore(host.WorldCamera);
   }
  }

  static void RegradeExistingSlate(ValoriaReferenceCleanGameplay city,ValoriaIntegratedWorld host,VisualPaletteReport report){
   var roots=new[]{city.BastionOne,city.BastionTwo,city.Parcels.LeftBuilt,
    city.Parcels.RightBuilt,host.WorldBastionOne,host.WorldBastionTwo,host.WorldSawmill};
   var cache=new Dictionary<Material,Material>();
   foreach(var root in roots.Where(r=>r!=null)){
    foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>(true)){
     var materials=renderer.sharedMaterials;bool modified=false;
     for(int j=0;j<materials.Length;j++){
      var source=materials[j];if(!source)continue;
      string name=source.name.ToLowerInvariant();
      bool slate=name.Contains("blue slate")||name.Contains("weathered slate")||
       (name.Contains("roof")&&!name.Contains("wood"));
      bool herald=name.Contains("cloth")||name.Contains("banner")||name.Contains("flag")||
       name.Contains("herald")||name.Contains("crest")||name.Contains("gold");
      if(!slate||herald)continue;
      if(!cache.TryGetValue(source,out var copy)){
       copy=new Material(source){name="Valoria muted stone slate / "+source.name};
       bool changed=false;
       string textureProperty=BaseTextureProperty(source);
       if(textureProperty!=null){
        var texture=ReadableCopy(source.GetTexture(textureProperty),1024);
        var pixels=texture.GetPixels();int coloured=0;
        for(int k=0;k<pixels.Length;k++){
         Color old=pixels[k];Color.RGBToHSV(old,out float hue,out float sat,out float value);
         if(old.a<.04f||sat<.19f||hue<.49f||hue>.74f)continue;
         // Keep the original tile detail; remove the strong cyan-blue saturation.
         var weathered=new Color(value*.57f,value*.62f,value*.65f,old.a);
         pixels[k]=Color.Lerp(old,weathered,.92f);coloured++;
        }
        if(coloured>pixels.Length*.003f){
         texture.SetPixels(pixels);texture.Apply(true);Put(texture,"muted-slate-texture");
         copy.SetTexture(textureProperty,texture);
         report.blue_roof_pixels_regraded+=coloured;changed=true;
        }else Object.DestroyImmediate(texture);
       }
       // Some of the original Blender glTF roof materials are plain colour
       // factors rather than painted albedo textures. Cover those as well.
       if(copy.HasProperty("baseColorFactor")&&(!changed||!source.GetTexture("baseColorTexture"))){
        copy.SetColor("baseColorFactor",new Color(.31f,.36f,.41f,1f));changed=true;
       }else if(copy.HasProperty("_BaseColor")&&(!changed||textureProperty==null)){
        copy.SetColor("_BaseColor",new Color(.31f,.36f,.41f,1f));changed=true;
       }
       if(!changed){Object.DestroyImmediate(copy);continue;}
       Put(copy,"muted-slate-material");cache[source]=copy;report.slate_materials_reused++;
      }
      materials[j]=copy;modified=true;
     }
     if(modified){renderer.sharedMaterials=materials;report.slate_renderers_modified++;}
    }
   }
  }


  [Serializable] sealed class RoofSignatureReport{
   public string scene,source_sha,scene_sha256;
   public bool original_light_preserved,terrain_heights_preserved,parcels_preserved,hotspots_preserved,
    renderer_colliders_preserved,art_approved,physical_mobile_tested;
   public int bastion_renderers,identified_blue_materials,weathered_roof_materials,roof_pixels_regraded;
   public string[] material_diagnosis;
   public Frame[] frames;
  }
  // Corrects the still-bright roof visible in the real Bastion II player image.
  // Material identities are diagnosed from the live integrated renderer slots:
  // historic NativeGI_Material_* names are not reliable slate identifiers.
  public static void ApplyRoofSignature(ValoriaReferenceCleanGameplay city,ValoriaIntegratedWorld host,string evidenceDir){
   const string markerName="Roof signature / Bastion weathered stone slate v5";
   Need(city&&host&&host.WorldRoot&&city.PlayerCamera&&host.WorldCamera,"Current I-II city/world missing");
   Need(Find(host.WorldRoot,"Visual palette / slate and river v4"),"Visual palette finishing not integrated");
   if(Find(host.WorldRoot,markerName)){Debug.Log("VALORIA_REFERENCE_CLEAN_BASTION_ROOF_ALREADY_APPLIED_COMPLETE");return;}
   string planPath=Path.Combine(evidenceDir,"environment-finish-plan.json");
   Need(File.Exists(planPath),"Original Qwen/Blender environment plan missing");
   var plan=JsonUtility.FromJson<ArtistPlan>(File.ReadAllText(planPath));
   Need(plan!=null&&plan.shore_gaps!=null&&plan.shore_gaps.Length>0,"Artist plan lost");
   var terrain=Object.FindObjectsByType<Terrain>(FindObjectsInactive.Include,FindObjectsSortMode.None)
    .FirstOrDefault(t=>!t.transform.IsChildOf(host.WorldRoot.transform));
   Need(terrain&&terrain.terrainData,"Retained city landscape missing");
   var oldTerrain=terrain.terrainData;
   var left=city.Parcels.LeftBuilt;var right=city.Parcels.RightBuilt;
   var hotspots=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true).ToDictionary(h=>h,h=>h.transform.position);
   var camCity=new CameraState(city.PlayerCamera);var camWorld=new CameraState(host.WorldCamera);
   bool worldActive=host.WorldRoot.activeSelf;var lightBefore=LightStamp();
   folder=AssetsRoot+"/RoofSignature-"+(Environment.GetEnvironmentVariable("GITHUB_RUN_ID")??"local");
   Directory.CreateDirectory(folder);Directory.CreateDirectory(evidenceDir);AssetDatabase.Refresh();assetIndex=0;
   var frames=new Dictionary<string,Frame>();var diagnostic=new List<string>();
   var report=new RoofSignatureReport{scene=city.gameObject.scene.path,
    source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),scene_sha256=Hash(city.gameObject.scene.path),
    art_approved=false,physical_mobile_tested=false};
   try{
    city.Apply(new PlayerState {BastionLevel=2,SawmillLevel=1,BarracksLevel=1});
    CaptureViews(city,host,plan,evidenceDir,"roof-before",frames);
    host.WorldRoot.SetActive(true);
    var marker=new GameObject(markerName);marker.transform.SetParent(host.WorldRoot.transform,false);
    var roots=new[]{city.BastionOne,city.BastionTwo,host.WorldBastionOne,host.WorldBastionTwo};
    var cache=new Dictionary<Material,Material>();
    foreach(var root in roots.Where(g=>g)){
     foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>(true)){
      // Bastion large mesh only. Keep royal banners, limestone, glass and
      // all non-roof materials unaltered even when names are generic.
      if(!renderer.name.StartsWith("Bastion_"))continue;
      report.bastion_renderers++;
      var mats=renderer.sharedMaterials;bool anyChanged=false;
      for(int slot=0;slot<mats.Length;slot++){
       var current=mats[slot];if(!current)continue;
       var key=current.name.ToLowerInvariant();
       string textureProperty=BaseTextureProperty(current);
       bool exclude=new[]{"cloth","banner","flag","herald","crest","oak","gold",
        "window","limestone","sandstone","wood","iron","mortar"}.Any(key.Contains);
       bool namedSlate=key.Contains("slate")||key.Contains("roof");
       int pixelsForSlot=0;
       Texture2D copyTexture=null;Color[] pixels=null;
       if(textureProperty!=null){
        copyTexture=ReadableCopy(current.GetTexture(textureProperty),768);
        pixels=copyTexture.GetPixels();
        for(int k=0;k<pixels.Length;k++){
         Color p=pixels[k];Color.RGBToHSV(p,out float h,out float sat,out float value);
         // Roof blue is medium slate: do not target the darker royal cloth.
         if(p.a>.05f&&h>.51f&&h<.715f&&sat>.19f&&sat<.83f&&
          p.b>p.g*1.06f&&p.g>p.r*1.13f&&p.g>.10f)pixelsForSlot++;
        }
       }
       float coverage=pixels==null?0:pixelsForSlot/(float)pixels.Length;
       string factorKey=current.HasProperty("baseColorFactor")?"baseColorFactor":
        current.HasProperty("_BaseColor")?"_BaseColor":null;
       Color color=factorKey!=null?current.GetColor(factorKey):Color.white;
       Color.RGBToHSV(color,out float hue,out float saturation,out float valueFactor);
       bool blueFactor=hue>.52f&&hue<.72f&&saturation>.24f&&color.g>.12f&&color.b>color.g*1.07f;
       diagnostic.Add(renderer.name+" / slot "+slot+" / "+current.name+" / blue_texture="+
        coverage.ToString("F3")+" / blue_factor="+blueFactor);
       // Do not mistake the flat cobalt colour factor of royal cloth for a roof.
       bool candidate=!exclude&&(namedSlate||coverage>.045f);
       if(!candidate){
        if(copyTexture)Object.DestroyImmediate(copyTexture);
        continue;
       }
       report.identified_blue_materials++;
       if(!cache.TryGetValue(current,out var replacement)){
        replacement=new Material(current){name="Bastion II / reclaimed weathered slate / "+current.name};
        bool changed=false;
        if(copyTexture&&pixelsForSlot>0){
         int modified=0;
         for(int k=0;k<pixels.Length;k++){
          Color p=pixels[k];Color.RGBToHSV(p,out float h,out float sat,out float bright);
          if(p.a<.05f||h<=.50f||h>=.73f||sat<.17f||p.b<p.g*1.05f)continue;
          pixels[k]=Color.Lerp(p,new Color(bright*.51f,bright*.56f,bright*.59f,p.a),.93f);
          modified++;
         }
         if(modified>0){
          copyTexture.SetPixels(pixels);copyTexture.Apply(true);
          Put(copyTexture,"bastion-weathered-slate");replacement.SetTexture(textureProperty,copyTexture);
          report.roof_pixels_regraded+=modified;changed=true;
         }
        }
        if(!changed&&(namedSlate||blueFactor)&&factorKey!=null){
         replacement.SetColor(factorKey,new Color(.29f,.34f,.37f,color.a));changed=true;
        }
        if(!changed){Object.DestroyImmediate(replacement);if(copyTexture)Object.DestroyImmediate(copyTexture);continue;}
        Put(replacement,"bastion-slate-material");cache[current]=replacement;report.weathered_roof_materials++;
       }else if(copyTexture)Object.DestroyImmediate(copyTexture);
       mats[slot]=replacement;anyChanged=true;
      }
      if(anyChanged)renderer.sharedMaterials=mats;
     }
    }
    host.WorldRoot.SetActive(false);
    CaptureViews(city,host,plan,evidenceDir,"roof-after",frames);
    report.original_light_preserved=LightStamp()==lightBefore;
    report.terrain_heights_preserved=oldTerrain.GetHeights(0,0,oldTerrain.heightmapResolution,oldTerrain.heightmapResolution)
     .Cast<float>().SequenceEqual(terrain.terrainData.GetHeights(0,0,terrain.terrainData.heightmapResolution,terrain.terrainData.heightmapResolution).Cast<float>());
    report.parcels_preserved=city.Parcels.LeftBuilt==left&&city.Parcels.RightBuilt==right;
    report.hotspots_preserved=hotspots.All(p=>p.Key&&p.Key.transform.position==p.Value);
    report.renderer_colliders_preserved=true; // This pass never edits or creates a Collider.
    Need(report.original_light_preserved&&report.terrain_heights_preserved&&report.parcels_preserved&&report.hotspots_preserved,
     "Protected native I-II world state changed in roof finishing");
    report.frames=frames.Values.ToArray();report.material_diagnosis=diagnostic.ToArray();
    File.WriteAllText(Path.Combine(evidenceDir,"environment-roof-signature-unity.json"),JsonUtility.ToJson(report,true));
    Need(report.weathered_roof_materials>=1&&report.roof_pixels_regraded>250,
     "Bastion roof regrade not established: actual renderer/material diagnostics retained");
    AssetDatabase.SaveAssets();
    Debug.Log("VALORIA_REFERENCE_CLEAN_ROOF_SIGNATURE_COMPLETE mats="+report.weathered_roof_materials+
     " pixels="+report.roof_pixels_regraded+" model_renderers="+report.bastion_renderers);
   }finally{
    city.Apply(new PlayerState());
    if(host.WorldBastionOne)host.WorldBastionOne.SetActive(true);
    if(host.WorldBastionTwo)host.WorldBastionTwo.SetActive(false);
    if(host.WorldSawmill)host.WorldSawmill.SetActive(false);
    host.WorldRoot.SetActive(worldActive);camCity.Restore(city.PlayerCamera);camWorld.Restore(host.WorldCamera);
   }
  }


  [Serializable] sealed class HeraldicRestoreReport {
   public string scene,source_sha,original_banner_material_guid;
   public bool same_camera_light,terrain_heights_preserved,parcels_preserved,hotspots_preserved,
    art_approved,physical_mobile_tested;
   public int checked_bastion_renderers,royal_material_slots_restored,material_slots_already_original;
   public Frame[] frames;
  }
  // The original large stone roof in V5 is good. Its generic-colour material
  // selection also caught the heraldic banners; restore only that donor slot.
  public static void RestoreRoyalBanners(ValoriaReferenceCleanGameplay city,ValoriaIntegratedWorld host,string evidenceDir){
   const string marker="Heraldic original royal blue / roof correction v6";
   const string bannerGuid="93d5684611f84b34693282ec9c3c3649";
   Need(city&&host&&host.WorldRoot&&city.PlayerCamera&&host.WorldCamera,"Retained player scene missing");
   Need(Find(host.WorldRoot,"Roof signature / Bastion weathered stone slate v5"),"V5 roof model finishing absent");
   if(Find(host.WorldRoot,marker)){Debug.Log("VALORIA_REFERENCE_CLEAN_BANNERS_ALREADY_RESTORED_COMPLETE");return;}
   var oldMat=AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(bannerGuid));
   Need(oldMat&&oldMat.name=="NativeGI_Material_026","Exact historic royal banner source material missing");
   string path=Path.Combine(evidenceDir,"environment-finish-plan.json");
   Need(File.Exists(path),"Original environment evidence missing");
   var plan=JsonUtility.FromJson<ArtistPlan>(File.ReadAllText(path));
   Need(plan!=null&&plan.shore_gaps!=null,"Original artist plan invalid");
   var terrain=Object.FindObjectsByType<Terrain>(FindObjectsInactive.Include,FindObjectsSortMode.None)
    .FirstOrDefault(t=>!t.transform.IsChildOf(host.WorldRoot.transform));
   Need(terrain&&terrain.terrainData,"City terrain missing");
   var data=terrain.terrainData;
   var left=city.Parcels.LeftBuilt;var right=city.Parcels.RightBuilt;
   var nodes=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true).ToDictionary(h=>h,h=>h.transform.position);
   var cc=new CameraState(city.PlayerCamera);var wc=new CameraState(host.WorldCamera);
   var lighting=LightStamp();bool wasWorld=host.WorldRoot.activeSelf;
   var frames=new Dictionary<string,Frame>();
   var result=new HeraldicRestoreReport{scene=city.gameObject.scene.path,
    source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),
    original_banner_material_guid=bannerGuid,art_approved=false,physical_mobile_tested=false};
   try{
    city.Apply(new PlayerState {BastionLevel=2,SawmillLevel=1,BarracksLevel=1});
    CaptureViews(city,host,plan,evidenceDir,"banner-before",frames);
    host.WorldRoot.SetActive(true);
    var markerObject=new GameObject(marker);markerObject.transform.SetParent(host.WorldRoot.transform,false);
    foreach(var root in new[]{city.BastionOne,city.BastionTwo,host.WorldBastionOne,host.WorldBastionTwo}.Where(x=>x)){
     foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>(true)){
      if(!renderer.name.StartsWith("Bastion_"))continue;
      result.checked_bastion_renderers++;
      var mats=renderer.sharedMaterials;
      Need(mats.Length>14&&mats[14],"Original 19-slot Bastion materials changed");
      if(mats[14]==oldMat){result.material_slots_already_original++;continue;}
      // Unity AssetDatabase.CreateAsset renames the previous tinted material
      // to its generated filename, so the Blender source name is not retained.
      // Check the exact generated artifact GUID instead of a mutable name.
      string pathOfPrevious=AssetDatabase.GetAssetPath(mats[14]);
      string guidOfPrevious=string.IsNullOrEmpty(pathOfPrevious)?"":AssetDatabase.AssetPathToGUID(pathOfPrevious);
      Need(guidOfPrevious=="9195b0acd9a2d524695f719dcfee0a7b",
       "Expected exact v5 regraded banner material identity in slot14, got "+
       renderer.name+"/"+pathOfPrevious+"/"+guidOfPrevious);
      mats[14]=oldMat;renderer.sharedMaterials=mats;result.royal_material_slots_restored++;
     }
    }
    host.WorldRoot.SetActive(false);
    CaptureViews(city,host,plan,evidenceDir,"banner-after",frames);
    result.same_camera_light=lighting==LightStamp();
    result.terrain_heights_preserved=data.GetHeights(0,0,data.heightmapResolution,data.heightmapResolution)
     .Cast<float>().SequenceEqual(terrain.terrainData.GetHeights(0,0,terrain.terrainData.heightmapResolution,terrain.terrainData.heightmapResolution).Cast<float>());
    result.parcels_preserved=city.Parcels.LeftBuilt==left&&city.Parcels.RightBuilt==right;
    result.hotspots_preserved=nodes.All(x=>x.Key&&x.Key.transform.position==x.Value);
    Need(result.same_camera_light&&result.terrain_heights_preserved&&result.parcels_preserved&&result.hotspots_preserved,
     "Heraldic material correction changed protected playable state");
    Need(result.checked_bastion_renderers>=2&&
     result.material_slots_already_original+result.royal_material_slots_restored==result.checked_bastion_renderers,
     "Incomplete Bastion banner recovery");
    result.frames=frames.Values.ToArray();
    AssetDatabase.SaveAssets();Directory.CreateDirectory(evidenceDir);
    File.WriteAllText(Path.Combine(evidenceDir,"environment-heraldic-restoration-unity.json"),JsonUtility.ToJson(result,true));
    Debug.Log("VALORIA_REFERENCE_CLEAN_ROYAL_BANNERS_COMPLETE restored="+result.royal_material_slots_restored);
   }finally{
    city.Apply(new PlayerState());
    if(host.WorldBastionOne)host.WorldBastionOne.SetActive(true);
    if(host.WorldBastionTwo)host.WorldBastionTwo.SetActive(false);
    if(host.WorldSawmill)host.WorldSawmill.SetActive(false);
    host.WorldRoot.SetActive(wasWorld);cc.Restore(city.PlayerCamera);wc.Restore(host.WorldCamera);
   }
  }

  static void ReplaceMapValoria(ValoriaReferenceCleanGameplay city,ValoriaIntegratedWorld host,Transform parent,Func<Vector3,float> ground) {
   var old=Find(host.WorldRoot,"Existing medieval player city"); Need(old,"Original map city missing");
   var target=Target(host,"valoria-map-city"); var origin=target.transform.position; origin.y=ground(origin);
   // The actual two current Bastion meshes share their city materials. There is
   // no substitute miniature castle family and no inference of a restored wall.
   host.WorldBastionOne=CloneVisual(city.BastionOne,"Valoria map / actual Bastion I",parent,origin+new Vector3(0,0,3),20);
   host.WorldBastionTwo=CloneVisual(city.BastionTwo,"Valoria map / actual Bastion II",parent,origin+new Vector3(0,0,3),20);
   host.WorldBastionOne.SetActive(false); host.WorldBastionTwo.SetActive(true);
   host.WorldSawmill=CloneVisual(city.Parcels.LeftBuilt,"Valoria map / actual timber workshop",parent,AtGround(origin+new Vector3(-11,0,-3),ground),7);
   // Only city-derived ruined fragments are reused at the border. No completed
   // defensive ring is shown before the player has actually restored it.
   var ruins=Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None)
    .Where(r=>r.name.StartsWith("Ruin_")&&!r.transform.IsChildOf(host.WorldRoot.transform)).Take(2).ToArray();
   for(int i=0;i<ruins.Length;i++) CloneVisual(ruins[i].gameObject,"Valoria map / retained ruins "+i,parent,
    AtGround(origin+new Vector3(i==0?-10:10,0,-8),ground),5.4f);
   old.gameObject.SetActive(false);
  }
  static Vector3 AtGround(Vector3 value,Func<Vector3,float> ground) { value.y=ground(value); return value; }
  static GameObject CloneVisual(GameObject source,string name,Transform parent,Vector3 destination,float width) {
   var root=new GameObject(name); root.transform.SetParent(parent,true);
   var bounds=BoundsOf(source); var centre=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
   float scale=width/Mathf.Max(bounds.size.x,bounds.size.z);
   foreach(var old in source.GetComponentsInChildren<MeshRenderer>(true)) {
    var filter=old.GetComponent<MeshFilter>(); if(!filter||!filter.sharedMesh) continue;
    var part=new GameObject(old.name); part.transform.SetParent(root.transform,false);
    // Flatten the visual hierarchy without copying behaviours, colliders or
    // baked lightmap indices from a different world position.
    var matrix=Matrix4x4.Translate(-centre)*old.transform.localToWorldMatrix;
    part.transform.localPosition=matrix.GetColumn(3); part.transform.localRotation=matrix.rotation;
    part.transform.localScale=matrix.lossyScale;
    part.AddComponent<MeshFilter>().sharedMesh=filter.sharedMesh;
    var renderer=part.AddComponent<MeshRenderer>(); renderer.sharedMaterials=old.sharedMaterials;
    PrepareRenderer(renderer);
   }
   root.transform.localScale=Vector3.one*scale; root.transform.position=destination;
   return root;
  }
  static void PrepareRenderer(MeshRenderer renderer) {
   GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,0); renderer.lightmapIndex=-1;
   renderer.receiveGI=ReceiveGI.LightProbes; renderer.lightProbeUsage=LightProbeUsage.BlendProbes;
   renderer.reflectionProbeUsage=ReflectionProbeUsage.BlendProbes; renderer.shadowCastingMode=ShadowCastingMode.On;
  }
  static async Task<GameObject> ReplaceAuthoredDestination(ValoriaIntegratedWorld host,string id,string original,string path,float width,Transform parent,Func<Vector3,float> ground) {
   var old=Find(host.WorldRoot,original); Need(old,"Original destination source missing: "+original);
   var target=Target(host,id); var destination=AtGround(target.transform.position,ground);
   var root=new GameObject("Zone1 authored "+id); root.transform.SetParent(parent,true);
   using(var loader=new GltfImport(null,new UninterruptedDeferAgent())) {
    Need(await loader.Load(new Uri(Path.GetFullPath(path)).AbsoluteUri,new ImportSettings {GenerateMipMaps=true,AnisotropicFilterLevel=4}),"GLB load: "+id);
    Need(await loader.InstantiateMainSceneAsync(root.transform),"GLB scene: "+id);
    // Match the authored north/front convention already used by the city GLBs.
    root.transform.rotation=Quaternion.Euler(0,180,0);
    var b=BoundsOf(root); root.transform.localScale=Vector3.one*(width/Mathf.Max(b.size.x,b.size.z)); b=BoundsOf(root);
    root.transform.position+=destination-new Vector3(b.center.x,b.min.y,b.center.z);
    foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>()) PrepareRenderer(renderer);
    PersistImported(root);
   }
   old.gameObject.SetActive(false);
   if(id=="quarry-valoria") foreach(var nearby in host.WorldRoot.GetComponentsInChildren<Transform>(true)
    .Where(t=>t.name.StartsWith("Existing Holotna rock.")).ToArray()) {
    var b=BoundsOf(nearby.gameObject);var delta=new Vector2(b.center.x-destination.x,b.center.z-destination.z);
    if(delta.magnitude<13)nearby.gameObject.SetActive(false);
   }
   return root;
  }
  static int UnifyWorldVegetation(ValoriaIntegratedWorld host,GameObject forest,Transform parent,Func<Vector3,float> ground) {
   var donor=forest.GetComponentsInChildren<MeshRenderer>(true).FirstOrDefault(r=>r.name.StartsWith("Forest retained Valoria pine"));
   Need(donor,"Authored forest has no retained pine donor");var donorBounds=BoundsOf(donor.gameObject);
   int count=0;
   // Keep the actual world woodland distribution. Replace only the repeated
   // planar tree representation, sharing the same retained pine mesh/materials.
   foreach(var old in host.WorldRoot.GetComponentsInChildren<Transform>(true)
    .Where(t=>t.name.StartsWith("Existing Holotna tree.")).ToArray()) {
    var bounds=BoundsOf(old.gameObject);var position=AtGround(new Vector3(bounds.center.x,bounds.min.y,bounds.center.z),ground);
    float width=Mathf.Clamp(bounds.size.y,4.3f,9f)*Mathf.Max(donorBounds.size.x,donorBounds.size.z)/Mathf.Max(1,donorBounds.size.y);
    var tree=CloneVisual(donor.gameObject,"Zone1 retained pine "+count,parent,position,width);
    tree.transform.rotation=Quaternion.Euler(0,(count*137.508f)%360,0);
    old.gameObject.SetActive(false);count++;
   }
   return count;
  }
  static void ReplaceEncampments(ValoriaReferenceCleanGameplay city,ValoriaIntegratedWorld host,Transform parent,Func<Vector3,float> ground) {
   foreach(var tuple in new[]{("corrupt-scout","Enemy camp provisional existing canvas",10f),("engendro-valoria","Enemy camp existing shelter",13f)}) {
    var old=Find(host.WorldRoot,tuple.Item2); Need(old,"Original enemy destination missing");
    var position=AtGround(Target(host,tuple.Item1).transform.position,ground);
    var clone=CloneVisual(city.Parcels.RightBuilt,"Zone1 hostile camp "+tuple.Item1,parent,position,tuple.Item3);
    foreach(var renderer in clone.GetComponentsInChildren<MeshRenderer>())
     renderer.sharedMaterials=renderer.sharedMaterials.Select(HostileCanvas).ToArray();
    if(tuple.Item1=="engendro-valoria") {
     var second=CloneVisual(city.Parcels.RightBuilt,"Zone1 hostile camp rear shelter",parent,
      AtGround(position+new Vector3(7,0,5),ground),6.5f);
     foreach(var renderer in second.GetComponentsInChildren<MeshRenderer>())
      renderer.sharedMaterials=renderer.sharedMaterials.Select(HostileCanvas).ToArray();
    }
    old.gameObject.SetActive(false);
   }
  }
  static Material HostileCanvas(Material old) {
   if(!old||!new[]{"canvas","cloth","fabric"}.Any(key=>old.name.ToLowerInvariant().Contains(key))) return old;
   if(hostileMaterials.TryGetValue(old,out var cached)) return cached;
   var copy=new Material(old) {name="Hostile ochre canvas / "+old.name};
   string property=BaseTextureProperty(copy);
   if(property!=null&&copy.GetTexture(property)) {
    var texture=ReadableCopy(copy.GetTexture(property)); var colors=texture.GetPixels();
    float average=Mathf.Max(.001f,colors.Average(c=>c.grayscale));
    for(int i=0;i<colors.Length;i++) { float detail=Mathf.Clamp(colors[i].grayscale/average,.25f,2.0f);float alpha=colors[i].a; colors[i]=new Color(.23f,.071f,.038f,1)*detail;colors[i].a=alpha; }
    texture.SetPixels(colors); texture.Apply(true); Put(texture,"hostile-canvas"); copy.SetTexture(property,texture);
   }
   if(copy.HasProperty("baseColorFactor")) copy.SetColor("baseColorFactor",Color.white);
   if(copy.HasProperty("_BaseColor")) copy.SetColor("_BaseColor",Color.white);
   Put(copy,"hostile-canvas"); hostileMaterials.Add(old,copy); return copy;
  }

  static int FinishCityGround(ValoriaReferenceCleanGameplay city,Terrain terrain,ArtistPlan plan) {
   var original=terrain.terrainData;
   var data=Object.Instantiate(original); data.name="Current city terrain / organic working routes";
   var layers=data.terrainLayers;
   int loam=Array.FindLastIndex(layers,l=>l&&l.name.ToLowerInvariant().Contains("loam")); if(loam<0) loam=Mathf.Min(2,layers.Length-1);
   Need(layers.Length>=3,"Current terrain material family missing");
   int n=original.alphamapWidth,m=original.alphamapHeight;
   var map=original.GetAlphamaps(0,0,n,m);
   // Clone explicitly from the original splat weights. Unity's TerrainData
   // asset cloning does not always retain packed alphamap texture contents.
   // Starting from the clone can recolour the entire river/grass outside the
   // tiny edited work-yard mask.
   data.SetAlphamaps(0,0,map);
   Put(data,"city-terrain"); terrain.terrainData=data;
   var collider=terrain.GetComponent<TerrainCollider>(); if(collider) collider.terrainData=data;
   var left=BoundsOf(city.Parcels.LeftBuilt); var right=BoundsOf(city.Parcels.RightBuilt);
   Vector2[] Access(Bounds b) {
    var end=new Vector2(b.center.x,b.min.z+.9f); var start=new Vector2(ValoriaReferenceCleanArtBuilder.RoadX(15),15);
    var control=new Vector2(b.center.x*.48f,14.5f);
    return Curve(start,control,end,16);
   }
   var paths=new[]{Access(left),Access(right)}; var workingBuildings=new[]{left,right}; int changed=0;
   for(int z=0;z<m;z++) for(int x=0;x<n;x++) {
    var p=new Vector2(terrain.transform.position.x+x/(float)(n-1)*data.size.x,terrain.transform.position.z+z/(float)(m-1)*data.size.z);
    float noise=(Mathf.PerlinNoise(p.x*.31f+31,p.y*.31f+57)-.5f)*.62f;
    float distance=Mathf.Min(DistanceToPath(p,paths[0]),DistanceToPath(p,paths[1]));
    float access=1-Smooth(plan.city_path_width-.3f,plan.city_path_width+1.0f,distance+noise);
    float yard=0;
    foreach(var b in workingBuildings) {
     // Elliptical work yards follow model occupancy, with an irregular feathered
     // edge. They have no raised geometry and never outline the old parcel slab.
     float dx=(p.x-b.center.x)/(b.extents.x+2.0f), dz=(p.y-(b.center.z-.7f))/(b.extents.z+1.9f);
     float radius=Mathf.Sqrt(dx*dx+dz*dz);
     yard=Mathf.Max(yard,(1-Smooth(.63f,1.19f,radius+noise*.07f))*.78f);
    }
    float wear=Mathf.Clamp01(Mathf.Max(access*.90f,yard));
    // An unbroken soft material transition along river banks replaces a uniform
    // bright grass lip while preserving the exact terrain heightfield.
    float river=Mathf.Abs(p.y-RiverCenter(p.x));
    float bank=(1-Smooth(1.0f,4.5f,Mathf.Abs(river-RiverWidth(p.x))-noise));
    if(Mathf.Abs(p.x)>18) wear=Mathf.Max(wear,bank*.29f);
    if(wear<.018f) continue;
    for(int layer=0;layer<layers.Length;layer++) map[z,x,layer]*=1-wear;
    map[z,x,loam]+=wear; changed++;
   }
   data.SetAlphamaps(0,0,map); terrain.Flush(); EditorUtility.SetDirty(data); return changed;
  }
  static float Smooth(float a,float b,float v) { return Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,v)); }
  // Exact retained river plan used for material placement only; no height or
  // source geometry is regenerated by these two functions.
  static float RiverCenter(float x) { return -23+1.5f*Mathf.Sin(x*.065f)+.85f*Mathf.Sin(x*.19f+.4f)*Smooth(6,17,Mathf.Abs(x)); }
  static float RiverWidth(float x) { return 7.1f+.75f*Mathf.Sin(x*.095f+.7f)+.48f*Mathf.Sin(x*.23f); }
  static Vector2[] Curve(Vector2 a,Vector2 b,Vector2 c,int count) {
   var result=new Vector2[count+1]; for(int i=0;i<=count;i++) {float t=i/(float)count; result[i]=(1-t)*(1-t)*a+2*(1-t)*t*b+t*t*c;} return result;
  }
  static float DistanceToPath(Vector2 point,Vector2[] path) {
   float best=float.MaxValue;
   for(int i=1;i<path.Length;i++) { var delta=path[i]-path[i-1]; float t=Mathf.Clamp01(Vector2.Dot(point-path[i-1],delta)/Mathf.Max(.0001f,delta.sqrMagnitude)); best=Mathf.Min(best,(point-path[i-1]-delta*t).sqrMagnitude); }
   return Mathf.Sqrt(best);
  }


  // Reuses the actual Qwen/Blender-authored pine, not new primitive trees.
  // Decorative clones have no collider: selectable routes and hotspots are untouched.
  static int DeepenExistingForest(ValoriaIntegratedWorld host,Transform parent,Func<Vector3,float> ground){
   var pine=host.WorldRoot.GetComponentsInChildren<MeshRenderer>(true)
    .FirstOrDefault(r=>r.name.StartsWith("Forest retained Valoria pine"));
   Need(pine&&pine.GetComponent<MeshFilter>()&&pine.GetComponent<MeshFilter>().sharedMesh,"Retained authored pine unavailable");
   Vector3 centre=Target(host,"forest-valoria").transform.position;
   Vector3 city=Target(host,"valoria-map-city").transform.position;
   var other=host.WorldRoot.GetComponentsInChildren<WorldHotspot>(true)
    .Where(h=>h.Id!="forest-valoria").Select(h=>h.transform.position).ToArray();
   int count=0;
   // A natural edge rather than a geometric ring, keeping an open arrival lane.
   for(int i=0;i<26;i++){
    float angle=(i*.618034f+.19f)*Mathf.PI*2f;
    float distance=10f+Mathf.PerlinNoise(i*.43f+.23f,11f)*20f;
    var direction=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
    var fromCity=new Vector2(city.x-centre.x,city.z-centre.z).normalized;
    if(Vector2.Dot(direction,fromCity)>.68f&&distance<22f)continue;
    var candidate=centre+new Vector3(direction.x*distance,0,direction.y*distance);
    if(other.Any(p=>(new Vector2(p.x-candidate.x,p.z-candidate.z)).sqrMagnitude<11f*11f))continue;
    candidate.y=ground(candidate);
    float width=3.9f+Mathf.PerlinNoise(i*.36f+4f,.71f)*2.1f;
    var tree=CloneVisual(pine.gameObject,"Zone1 woodland edge / retained pine "+i,parent,candidate,width);
    tree.transform.rotation=Quaternion.Euler(0,(i*137.508f)%360f,0);
    count++;
   }
   Need(count>=9,"Unable to create a visually legible woodland edge");
   return count;
  }

  // A broad, restrained terrain-layer treatment: soft vegetation/earth variation,
  // weathered foundations and river contact, with NO new mesh or visible plot grids.
  static int RefineCityLandscape(ValoriaReferenceCleanGameplay city,Terrain terrain){
   var source=terrain.terrainData;
   var data=Object.Instantiate(source);
   data.name="Valoria retained landscape / organic variation v2";
   var layers=data.terrainLayers;Need(layers!=null&&layers.Length>=3,"Original terrain materials absent");
   int loam=Array.FindLastIndex(layers,l=>l&&l.name.ToLowerInvariant().Contains("loam"));
   if(loam<0)loam=Mathf.Min(2,layers.Length-1);
   int grit=Array.FindIndex(layers,l=>l&&(l.name.ToLowerInvariant().Contains("stone")||
    l.name.ToLowerInvariant().Contains("gravel")||l.name.ToLowerInvariant().Contains("rock")));
   if(grit<0||grit==loam)grit=(loam+1)%layers.Length;
   int w=source.alphamapWidth,h=source.alphamapHeight;
   var map=source.GetAlphamaps(0,0,w,h);
   // Preserve packed terrain splats explicitly when cloning an existing TerrainData.
   data.SetAlphamaps(0,0,map);
   var workplaces=new[]{BoundsOf(city.Parcels.LeftBuilt),BoundsOf(city.Parcels.RightBuilt)};
   int changed=0;
   for(int z=0;z<h;z++)for(int x=0;x<w;x++){
    var p=new Vector2(terrain.transform.position.x+x/(float)(w-1)*data.size.x,
     terrain.transform.position.z+z/(float)(h-1)*data.size.z);
    float broad=Mathf.PerlinNoise(p.x*.039f+3.7f,p.y*.039f+27.6f);
    float detail=Mathf.PerlinNoise(p.x*.18f+13.3f,p.y*.18f+52.1f);
    float irregular=(broad-.47f)*.60f+(detail-.5f)*.28f;
    float variation=Mathf.Clamp01(irregular)*.37f;
    float yard=0f;
    foreach(var b in workplaces){
     float dx=(p.x-b.center.x)/(b.extents.x+3.8f);
     float dz=(p.y-b.center.z)/(b.extents.z+3.2f);
     yard=Mathf.Max(yard,1-Smooth(.65f,1.6f,Mathf.Sqrt(dx*dx+dz*dz)+irregular*.11f));
    }
    float riverEdge=Mathf.Abs(Mathf.Abs(p.y-RiverCenter(p.x))-RiverWidth(p.x));
    float bank=(1-Smooth(.7f,5.6f,riverEdge+irregular*.6f))*.18f;
    float wear=Mathf.Clamp01(Mathf.Max(variation,Mathf.Max(yard*.29f,bank)));
    // Original darker road/cobble splats remain the authority; do not repaint.
    wear*=1-Smooth(.51f,.94f,map[z,x,loam]);
    if(wear<.025f)continue;
    int target=(yard>.5f&&detail>.60f)?grit:loam;
    for(int i=0;i<layers.Length;i++)map[z,x,i]*=1-wear;
    map[z,x,target]+=wear;
    changed++;
   }
   data.SetAlphamaps(0,0,map);Put(data,"city-landscape-v2");
   terrain.terrainData=data;
   var terrainCollider=terrain.GetComponent<TerrainCollider>();
   if(terrainCollider)terrainCollider.terrainData=data;
   terrain.Flush();EditorUtility.SetDirty(data);return changed;
  }

  static int FinishWorldSurface(ValoriaIntegratedWorld host,MeshFilter ground) {
   var renderer=ground.GetComponent<MeshRenderer>(); Need(renderer&&renderer.sharedMaterials.Length==1,"World ground material layout changed");
   var old=renderer.sharedMaterial; string property=BaseTextureProperty(old); Need(property!=null&&old.GetTexture(property),"World ground base texture missing");
   var texture=ReadableCopy(old.GetTexture(property),1536); var pixels=texture.GetPixels();
   var mesh=ground.sharedMesh; var uv=mesh.uv; var vertices=mesh.vertices;
   Need(uv.Length==vertices.Length,"Current world ground UVs missing");
   int a=0,b=-1,c=-1;
   for(int i=1;i<uv.Length;i++)if((uv[i]-uv[a]).sqrMagnitude>.00000001f){b=i;break;}
   Need(b>=0,"World UV map lacks a distinct second coordinate");
   var ab=uv[b]-uv[a];
   for(int i=b+1;i<uv.Length;i++)if(Mathf.Abs(ab.x*(uv[i].y-uv[a].y)-ab.y*(uv[i].x-uv[a].x))>.000001f){c=i;break;}
   Need(c>=0,"World texture coordinate map degenerate");
   var ac=uv[c]-uv[a]; float determinant=ab.x*ac.y-ab.y*ac.x;
   var pa=ground.transform.TransformPoint(vertices[a]); var pb=ground.transform.TransformPoint(vertices[b])-pa; var pc=ground.transform.TransformPoint(vertices[c])-pa;
   var u=(pb*ac.y-pc*ab.y)/determinant; var v=(pc*ab.x-pb*ac.x)/determinant; var origin=pa-u*uv[a].x-v*uv[a].y;
   for(int i=0;i<vertices.Length;i+=Mathf.Max(1,vertices.Length/100)) {
    var expected=origin+u*uv[i].x+v*uv[i].y; var actual=ground.transform.TransformPoint(vertices[i]);
    Need(new Vector2(expected.x-actual.x,expected.z-actual.z).magnitude<.02f,"Non-affine world UV map requires explicit authoring");
   }
   Vector2 Point(string id) {var position=Target(host,id).transform.position;return new Vector2(position.x,position.z);}
   var home=Point("valoria-map-city"); var wood=Point("forest-valoria"); var quarry=Point("quarry-valoria");
   // Only two well-travelled resource routes. No five-spoke road star and no mesh strip.
   var destinations=new[]{wood,quarry};
   var routes=destinations.Select((destination,index)=> {
    var delta=destination-home;
    var bend=new Vector2(-delta.y,delta.x).normalized*Mathf.Min(9f,delta.magnitude*.12f)*(index==0?1:-1);
    return Curve(home,(home+destination)*.5f+bend,destination,12);
   }).ToArray();
   var breachA=Point("corrupt-scout");var breachB=Point("engendro-valoria");
   int changed=0;
   for(int y=0;y<texture.height;y++) for(int x=0;x<texture.width;x++) {
    var world=origin+u*(x/(float)(texture.width-1))+v*(y/(float)(texture.height-1)); var p=new Vector2(world.x,world.z);
    float edge=(Mathf.PerlinNoise(p.x*.19f+13,p.y*.19f+43)-.5f)*1.3f;
    float path=float.MaxValue;for(int route=0;route<routes.Length;route++)path=Mathf.Min(path,DistanceToPath(p,routes[route]));
    float wear=(1-Smooth(.9f,3.4f,path+edge))*.27f;
    float homeApron=(1-Smooth(9,19,Vector2.Distance(p,home)+edge))*.11f;
    float forestFloor=(1-Smooth(8,20,Vector2.Distance(p,wood)+edge))*.38f;
    float quarryFloor=(1-Smooth(7,17,Vector2.Distance(p,quarry)+edge))*.53f;
    float corruptedDistance=Mathf.Min(Vector2.Distance(p,breachA),Vector2.Distance(p,breachB));
    float breach=(1-Smooth(10,30,corruptedDistance+edge*2f))*.46f;
    float strength=Mathf.Max(Mathf.Max(wear,homeApron),Mathf.Max(forestFloor,Mathf.Max(quarryFloor,breach)));
    if(strength<.014f)continue;
    var soil=Color.Lerp(new Color(.38f,.34f,.24f),new Color(.46f,.405f,.29f),
     Mathf.PerlinNoise(p.x*.38f+6f,p.y*.38f+12f));
    if(forestFloor>wear)soil=Color.Lerp(soil,new Color(.24f,.29f,.20f),.46f);
    if(quarryFloor>wear)soil=Color.Lerp(soil,new Color(.37f,.34f,.28f),.57f);
    if(breach>.05f)soil=Color.Lerp(soil,new Color(.26f,.22f,.23f),Mathf.Clamp01(breach*.95f));
    int i=y*texture.width+x; pixels[i]=Color.Lerp(pixels[i],soil,strength); pixels[i].a=1; changed++;
   }
   texture.SetPixels(pixels); texture.Apply(true); texture.wrapMode=TextureWrapMode.Clamp;
   Put(texture,"world-ground-connected-routes"); var material=new Material(old) {name="Retained prairie / connected working routes"}; material.SetTexture(property,texture); Put(material,"world-ground"); renderer.sharedMaterial=material;
   return changed;
  }

  static int BreakShoreRepetition(ValoriaIntegratedWorld host,ArtistPlan plan) {
   int changed=0;
   foreach(var filter in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include,FindObjectsSortMode.None)
    .Where(f=>f.name.StartsWith("River_bank_rocks")&&!f.transform.IsChildOf(host.WorldRoot.transform)).ToArray()) {
    var old=filter.sharedMesh; if(!old||!old.isReadable) continue;
    var positions=old.vertices; var world=positions.Select(p=>filter.transform.TransformPoint(p)).ToArray();
    var welded=new Dictionary<Vector3Int,int>(); var parent=new int[positions.Length];
    int Root(int id) {while(parent[id]!=id){parent[id]=parent[parent[id]];id=parent[id];}return id;}
    for(int i=0;i<positions.Length;i++) {var key=Vector3Int.RoundToInt(world[i]*1000);if(welded.TryGetValue(key,out var other))parent[i]=other;else {parent[i]=i;welded.Add(key,i);}}
    for(int s=0;s<old.subMeshCount;s++) {var t=old.GetTriangles(s);for(int i=0;i<t.Length;i+=3){int r=Root(t[i]);parent[Root(t[i+1])]=r;parent[Root(t[i+2])]=r;}}
    var groups=Enumerable.Range(0,positions.Length).GroupBy(Root).ToArray(); bool edited=false;
    foreach(var group in groups) {
     var ids=group.ToArray(); if(ids.Length<12)continue;var bounds=new Bounds(world[ids[0]],Vector3.zero);foreach(int id in ids)bounds.Encapsulate(world[id]);
     if(Mathf.Abs(bounds.center.x)<18||bounds.size.y<.45f||bounds.size.x>9||bounds.size.z>9)continue;
     float scale=1;
     foreach(var gap in plan.shore_gaps) {
      if(Mathf.Abs(gap.x)<18)continue;
      float distance=Mathf.Abs(bounds.center.x-gap.x);float weight=1-Smooth(gap.width*.24f,gap.width*.65f,distance);
      scale=Mathf.Min(scale,Mathf.Lerp(1,gap.scale,weight));
     }
     if(scale>.93f)continue;
     foreach(int id in ids) {var p=world[id];p.x=bounds.center.x+(p.x-bounds.center.x)*scale;p.z=bounds.center.z+(p.z-bounds.center.z)*scale;p.y=bounds.min.y+(p.y-bounds.min.y)*scale;positions[id]=filter.transform.InverseTransformPoint(p);}
     changed++;edited=true;
    }
    if(!edited)continue;
    var mesh=Object.Instantiate(old); mesh.name=old.name+" / embedded retained shore groups"; mesh.vertices=positions;mesh.RecalculateBounds();Put(mesh,"river-bank-reused-stone-groups");filter.sharedMesh=mesh;
    // Selection and navigation do not rely on these decorative stones. Any
    // existing physical collider follows the same copied mesh, outside the
    // protected bridge corridor; terrain and hotspot colliders are untouched.
    var collider=filter.GetComponent<MeshCollider>(); if(collider&&collider.sharedMesh==old)collider.sharedMesh=mesh;
   }
   return changed;
  }

  static void CaptureViews(ValoriaReferenceCleanGameplay city,ValoriaIntegratedWorld host,ArtistPlan plan,string evidence,string phase,Dictionary<string,Frame> frames) {
   var cityState=new CameraState(city.PlayerCamera);var worldState=new CameraState(host.WorldCamera);bool active=host.WorldRoot.activeSelf;
   try {
    host.WorldRoot.SetActive(false); city.PlayerCamera.enabled=true; host.WorldCamera.enabled=false;
    void Capture(Camera camera,string view,int width,int height) {
     string name="environment-"+view+"-"+phase+".png";var matrix=camera.transform.localToWorldMatrix;
     var stamp=new float[18];for(int i=0;i<16;i++)stamp[i]=matrix[i];stamp[16]=camera.orthographicSize;stamp[17]=camera.fieldOfView;
     if(phase=="before"||phase=="leap-before"||phase=="polish-before"||phase=="palette-before"||phase=="roof-before"||phase=="banner-before")frames.Add(view,new Frame{view=view,before=name,width=width,height=height,camera=stamp});
     else {Need(frames.ContainsKey(view)&&frames[view].camera.SequenceEqual(stamp),"A/B camera moved: "+view);frames[view].after=name;}
     float previousAspect=camera.aspect;camera.aspect=width/(float)height;
     try {ValoriaReferenceCleanBuilder.Capture(camera,Path.Combine(evidence,name),width,height);}
     finally {camera.aspect=previousAspect;}
    }
    Capture(city.PlayerCamera,"city-wide",1600,900); Capture(city.PlayerCamera,"city-portrait",900,1600);
    foreach(var tuple in new[]{("sawmill",city.Parcels.LeftBuilt,-1f),("barracks",city.Parcels.RightBuilt,1f)}) {
     var bounds=BoundsOf(tuple.Item2);city.PlayerCamera.transform.position=bounds.center+new Vector3(tuple.Item3*13,17,-23);
     city.PlayerCamera.transform.LookAt(bounds.center-new Vector3(0,1.2f,2));city.PlayerCamera.orthographicSize=12.5f;
     Capture(city.PlayerCamera,tuple.Item1,1448,1086);
    }
    // Frame the actual authored shore stretch in both phases; the former fixed
    // central view missed all three chosen stretches outside the bridge.
    float shoreX=plan.shore_gaps.OrderBy(g=>Mathf.Abs(g.x)).First().x;
    var shorePoint=new Vector3(shoreX,-.3f,RiverCenter(shoreX));
    city.PlayerCamera.transform.position=shorePoint+new Vector3(14,32,-34);city.PlayerCamera.transform.LookAt(shorePoint);city.PlayerCamera.orthographicSize=23;
    Capture(city.PlayerCamera,"river-bank",1448,1086);
    city.PlayerCamera.enabled=false;host.WorldRoot.SetActive(true);host.WorldCamera.enabled=true;
    if(host.WorldBastionOne)host.WorldBastionOne.SetActive(false);if(host.WorldBastionTwo)host.WorldBastionTwo.SetActive(true);
    if(host.WorldSawmill)host.WorldSawmill.SetActive(true);
    void WorldFrame(string id,float size) {var point=Target(host,id).transform.position; point.y=2;host.WorldCamera.transform.rotation=worldState.rotation;host.WorldCamera.transform.position=point-host.WorldCamera.transform.forward*110;host.WorldCamera.orthographicSize=size;}
    WorldFrame("valoria-map-city",32);Capture(host.WorldCamera,"world-wide",1600,900);Capture(host.WorldCamera,"world-portrait",900,1600);
    foreach(string id in new[]{"forest-valoria","quarry-valoria","corrupt-scout","engendro-valoria"}) {WorldFrame(id,17);Capture(host.WorldCamera,"world-"+id,1448,1086);}
   } finally {host.WorldRoot.SetActive(active);cityState.Restore(city.PlayerCamera);worldState.Restore(host.WorldCamera);}
  }
  static string BaseTextureProperty(Material material) {
   foreach(string property in new[]{"baseColorTexture","_BaseMap","_MainTex"})if(material.HasProperty(property)&&material.GetTexture(property))return property;return null;
  }
  static Texture2D ReadableCopy(Texture source,int maximum=2048) {
   float scale=Mathf.Min(1,maximum/(float)Mathf.Max(source.width,source.height));int width=Mathf.Max(1,Mathf.RoundToInt(source.width*scale)),height=Mathf.Max(1,Mathf.RoundToInt(source.height*scale));
   var rt=RenderTexture.GetTemporary(width,height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);var previous=RenderTexture.active;
   try {Graphics.Blit(source,rt);RenderTexture.active=rt;var result=new Texture2D(width,height,TextureFormat.RGBA32,true,true){name=source.name+" finish copy",wrapMode=source.wrapMode,filterMode=FilterMode.Trilinear,anisoLevel=4};result.ReadPixels(new Rect(0,0,width,height),0,0);result.Apply(true,false);return result;}
   finally {RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);}
  }
  static void Put(Object value,string label) {AssetDatabase.CreateAsset(value,AssetDatabase.GenerateUniqueAssetPath(folder+"/"+label+"-"+(assetIndex++)+(value is Material?".mat":".asset")));}
  static void PersistImported(GameObject root) {
   foreach(var filter in root.GetComponentsInChildren<MeshFilter>()) {
    var source=filter.sharedMesh;if(!persisted.TryGetValue(source,out var copy)){copy=Object.Instantiate(source);Put(copy,"authored-mesh");persisted.Add(source,copy);}filter.sharedMesh=(Mesh)copy;
   }
   foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>()) {
    var materials=renderer.sharedMaterials;
    for(int i=0;i<materials.Length;i++) {
     var source=materials[i];Need(source&&source.shader&&!source.shader.name.Contains("InternalError"),"Authored material missing");
     if(!persisted.TryGetValue(source,out var material)) {
      var copy=new Material(source);
      foreach(string property in source.GetTexturePropertyNames()) {
       var texture=source.GetTexture(property);if(!texture)continue;
       if(!persisted.TryGetValue(texture,out var saved)){saved=ReadableCopy(texture);Put(saved,"authored-texture");persisted.Add(texture,saved);}copy.SetTexture(property,(Texture)saved);
      }
      Put(copy,"authored-material");persisted.Add(source,copy);material=copy;
     }
     materials[i]=(Material)material;
    }
    renderer.sharedMaterials=materials;
   }
  }
 }
}
#endif
