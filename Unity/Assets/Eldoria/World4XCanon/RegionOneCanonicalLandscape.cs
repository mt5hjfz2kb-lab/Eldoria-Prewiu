using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Eldoria.ReferenceClean;
using Eldoria.World4XPrototype;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Eldoria.World4XCanon
{
 // One presentation owner. The former stacked terrain/ecology builders are not constructed.
 [DefaultExecutionOrder(1600)]
 public sealed class RegionOneCanonicalLandscape : MonoBehaviour
 {
  public const int LandscapeVersion=3;
  public const float LandscapeHalfWidth=910,LandscapeHalfDepth=1800;
  public const float BreachOffsetX=-260,BreachOffsetZ=150;
  public const float LandscapePanoramaZoom=500,PortraitPanoramaZoom=620;
  internal static readonly int[] ForestCorridorPairs=new int[0];
  ValoriaIntegratedWorld host; ZoneOneStrategicCamera navigation;Vector3 origin;
  bool built;
  RegionOneBlenderLandscape geography;RegionOneReferenceNodes nodes;
  RegionOneResourcePlacement placement;RegionOneResourceModels resourceModels;
  RegionOneCreaturePopulation creaturePopulation;RegionOneLookDevelopment lookDevelopment;
  Eldoria.SpawnlingBastion2.SpawnlingEncounterVisual originalTripoActor;
  bool previousPanorama;
  float renderedZoom=-1f,renderedAspect=-1f;
  readonly List<Renderer> originals=new List<Renderer>();
  readonly List<bool> states=new List<bool>();readonly HashSet<Renderer> background=new HashSet<Renderer>();
  readonly List<LODGroup> lods=new List<LODGroup>();readonly List<bool> lodStates=new List<bool>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            // AfterSceneLoad alone runs once per application, not on later
            // scene transitions. The real player can enter Valoria after a
            // bootstrap scene, and PlayMode tests also LoadScene dynamically.
            SceneManager.sceneLoaded -= WhenSceneLoaded;
            SceneManager.sceneLoaded += WhenSceneLoaded;
            AttachToCurrentIntegratedScene();
        }

        static void WhenSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            AttachToCurrentIntegratedScene();
        }

        static void AttachToCurrentIntegratedScene()
        {
            var host = UnityEngine.Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
            if (!host || UnityEngine.Object.FindFirstObjectByType<RegionOneCanonicalLandscape>()) return;
            var adapter = new GameObject("Region 1 canonical world · presentation adapter")
                .AddComponent<RegionOneCanonicalLandscape>();
            adapter.host = host;
        }

        IEnumerator Start()
        {
            if (!host) yield break;
            while (host && !host.Ready) yield return null;
            if (!host || !host.WorldRoot || !host.WorldNavigation || !host.WorldCamera) yield break;
            navigation = host.WorldNavigation;
            origin = host.WorldHome;
            if (host.WorldRoot.transform.Find("Region 1 canonical continuous geography"))
            {
                Debug.LogWarning("REGION1_CANON_LAYER_ALREADY_PRESENT");
                yield break;
            }
            InstallCameraLimits();
            Build(host.WorldRoot.transform);
            foreach(var renderer in host.WorldRoot.GetComponentsInChildren<Renderer>(true))renderer.gameObject.layer=29;
            host.WorldRoot.layer=29;
            host.WorldCamera.cullingMask=1<<29;
            host.WorldCamera.backgroundColor=new Color(.48f,.53f,.35f,1); // continental boundary haze, never implied surrounding sea
            ConfigureRegionCoverage();
            built = true;
            UpdateLevelOfDetail(true);
            Debug.Log("REGION1_CANON_WORLD_INSTALLED geography=1 population_authority=existing_LocalGateway persistence=existing_PlayerState");
        }

        void InstallCameraLimits()
        {
            // Bounds enclose a larger real terrain mesh, not a zoom onto empty sky.
            // The current integration sets regionalZoom=32 and localZoom=14 in Start.
            // The previous 17-unit close zoom filled the view with Valoria's
            // castle and prevented players from reading the surrounding 4X
            // territory. Widen both scales, retaining one real camera and
            // strong near -> regional -> panorama separation.
            navigation.localZoom = 36f;
            navigation.regionalZoom = Mathf.Max(280f,400f/navigation.View.aspect);
            navigation.globalZoom = navigation.View.aspect < .80f
                ? PortraitPanoramaZoom : LandscapePanoramaZoom;
            navigation.tilt = 66f;
            navigation.cameraDistance = Mathf.Max(navigation.cameraDistance, 540f);
            navigation.minimum = new Vector2(origin.x - 910f, origin.z - 780f);
            navigation.maximum = new Vector2(origin.x + 910f, origin.z + 780f);
            navigation.edgeMargin = 12f;
            navigation.FitToAuthoredBounds();
        }


  Bounds regionBounds;
  void ConfigureRegionCoverage()
  {
   var vertices=geography.WorldVertices;
   regionBounds=new Bounds(vertices[0],Vector3.zero);
   foreach(var p in vertices)regionBounds.Encapsulate(p);
   navigation.AllowWholeRegionOverview=true;
   navigation.minimum=new Vector2(regionBounds.min.x,regionBounds.min.z);
   navigation.maximum=new Vector2(regionBounds.max.x,regionBounds.max.z);
   navigation.cameraDistance=Mathf.Max(540,regionBounds.size.magnitude);
   navigation.View.farClipPlane=navigation.cameraDistance*2.5f;
   navigation.globalZoom=OverviewSize(navigation.View.aspect);
  }
  static float PanoramaHeading(float aspect)=>90*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.8f,1.2f,aspect));
  float OverviewSize(float aspect)
  {
   // Observe the same longitudinal continent along its long axis in landscape.
   // Fit all eight real bounds corners at the actual heading; never crop or scale geography.
   var rotation=Quaternion.Euler(64,PanoramaHeading(aspect),0);
   var right=rotation*Vector3.right;var up=rotation*Vector3.up;
   float horizontal=0,vertical=0;
   for(int i=0;i<8;i++){
    var p=new Vector3((i&1)==0?-regionBounds.extents.x:regionBounds.extents.x,
      (i&2)==0?-regionBounds.extents.y:regionBounds.extents.y,
      (i&4)==0?-regionBounds.extents.z:regionBounds.extents.z);
    horizontal=Mathf.Max(horizontal,Mathf.Abs(Vector3.Dot(p,right)));
    vertical=Mathf.Max(vertical,Mathf.Abs(Vector3.Dot(p,up)));
   }
   return Mathf.Max(horizontal/Mathf.Max(.1f,aspect),vertical)*1.04f;
  }
  void Build(Transform existingWorld)
  {
   // Capture existing scene integration state ONCE, before creating the clean presentation.
   foreach(var r in existingWorld.GetComponentsInChildren<Renderer>(true))
   {
    originals.Add(r);states.Add(r.enabled);
    string n=r.name.ToLowerInvariant();
    bool oldGround=(n.Contains("continuous prairie")||n.Contains("organic macrogeography")||(n.Contains("ground")&&n.Contains("zoneone")))&&r.bounds.size.x>100&&r.bounds.size.z>100;
    bool oldScenery=RegionOneDirectedGeography.IsTreeDonor(r);
    for(var p=r.transform;p&&p!=existingWorld;p=p.parent)
     oldScenery|=RegionOneDirectedGeography.IsTreeName(p.name)||p.name.StartsWith("Existing Holotna mountain",StringComparison.Ordinal)||p.name=="Enemy camp provisional existing canvas"||p.name=="Enemy camp existing shelter";
    if(oldGround||oldScenery)background.Add(r);
   }
   foreach(var lod in existingWorld.GetComponentsInChildren<LODGroup>(true))
    if(lod.GetComponentsInChildren<Renderer>(true).Any(background.Contains))
    {lods.Add(lod);lodStates.Add(lod.enabled);lod.enabled=false;}
   var root=new GameObject("Region 1 canonical continuous geography").transform;root.SetParent(existingWorld,true);
   geography=RegionOneBlenderLandscape.Install(root,host);lookDevelopment=root.GetComponentInChildren<RegionOneLookDevelopment>();
   nodes=RegionOneReferenceNodes.Install(root,host);
   RegionOneResourcePlacement.Install(root,host,geography);
   RegionOneResourceModels.Install(root,host);
   placement=root.GetComponent<RegionOneResourcePlacement>();
   resourceModels=root.GetComponent<RegionOneResourceModels>();
   RegionOneCreaturePopulation.Install(root,host,placement);
   creaturePopulation=root.GetComponent<RegionOneCreaturePopulation>();
   originalTripoActor=existingWorld.GetComponentInChildren<Eldoria.SpawnlingBastion2.SpawnlingEncounterVisual>(true);
  }
  void UpdateLevelOfDetail(bool force)
  {
   if(!navigation||!navigation.View)return;
   bool wasGlobal=navigation.View.orthographicSize>=navigation.globalZoom*.98f;
   float regional=Mathf.Max(280f,400f/navigation.View.aspect);
   if(Mathf.Abs(navigation.regionalZoom-regional)>.5f){bool was=Mathf.Abs(navigation.View.orthographicSize-navigation.regionalZoom)<1;navigation.regionalZoom=regional;if(was)navigation.SetZoomLevel(1,true);}
   float far=OverviewSize(navigation.View.aspect);
   if(Mathf.Abs(navigation.globalZoom-far)>.5f){navigation.globalZoom=far;if(wasGlobal){navigation.heading=PanoramaHeading(navigation.View.aspect);navigation.SetZoomLevel(0,true);}}
   float near=navigation.View.aspect<.8f?64:36;
   if(Mathf.Abs(navigation.localZoom-near)>.5f){bool was=navigation.View.orthographicSize<=navigation.localZoom+.5f;navigation.localZoom=near;if(was)navigation.SetZoomLevel(2,true);}
   float size=navigation.View.orthographicSize;
   navigation.cameraDistance=Mathf.Max(140,size*2.5f);navigation.View.farClipPlane=Mathf.Max(900,navigation.cameraDistance*2.5f);
   if(size>=navigation.globalZoom*.98f)navigation.PanTo(new Vector3(regionBounds.center.x,origin.y,regionBounds.center.z));
   float desired=Mathf.Lerp(42,64,Mathf.SmoothStep(0,1,Mathf.InverseLerp(36,244,size)));
   float tilt=force?desired:Mathf.Lerp(navigation.tilt,desired,1-Mathf.Exp(-7*Time.unscaledDeltaTime));
   float desiredHeading=PanoramaHeading(navigation.View.aspect)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(regional*1.15f,far*.85f,size));
   float heading=desiredHeading; // Actual zoom already interpolates; do not add a second lag to pointer anchoring.
   if(Mathf.Abs(tilt-navigation.tilt)>.04f||Mathf.Abs(Mathf.DeltaAngle(heading,navigation.heading))>.04f){navigation.SetOrientation(tilt,heading);}
   bool panorama=RegionOneResourcePlacement.IsPanoramic(size,navigation.globalZoom);
   // A target selected before zooming out must not leave an invisible combat
   // or resource selection marker on the map. This is the existing RoyalHUD
   // selection API, not new UI or a gameplay-state mutation.
   if(panorama&&host.InWorld&&!string.IsNullOrEmpty(host.Selected)&&
       (host.Selected.StartsWith("region1:",StringComparison.Ordinal)||
        host.Selected=="forest-valoria"||host.Selected=="quarry-valoria"||
        host.Selected=="corrupt-scout"||host.Selected=="engendro-valoria"))
       host.Select("");
   // The integrated Pick() has an additional creature geometry hit-test that
   // bypasses the hotspot collider. Deactivate the SAME source ModelRoot while
   // panoramic, otherwise an invisible engendro remains selectable.
   // This changes visibility only; its existing combat, rewards, actor pivot
   // and saved campaign defeat flag remain authoritative.
   if(originalTripoActor&&originalTripoActor.ModelRoot)
   {
    var model=originalTripoActor.ModelRoot;
    if(panorama&&model.activeSelf)model.SetActive(false);
    else if(!panorama&&previousPanorama&&!model.activeSelf&&
      host.City!=null&&host.City.Gateway!=null&&
      !host.City.Gateway.Snapshot().EngendroDefeated)model.SetActive(true);
   }
   // The existing Tripo spawnling remains visible in regional view. No second
   // creature, placeholder camp or gameplay authority is ever instantiated.
   for(int i=0;i<originals.Count;i++)if(originals[i])
    originals[i].enabled=states[i]&&!panorama&&
      originals[i].GetComponentInParent<Eldoria.SpawnlingBastion2.SpawnlingEncounterVisual>()!=null;
   // Change only visibility/picking. Coordinates, persistence and timers stay
   // with the same LocalGateway instance through the entire continuous zoom.
   if(force||previousPanorama!=panorama)
   {
    previousPanorama=panorama;
    if(placement)placement.RefreshForZoom();
    if(resourceModels)resourceModels.RefreshNow();
    if(creaturePopulation)creaturePopulation.RefreshNow();
   }
   if(nodes)nodes.Refresh();if(geography)geography.Refresh();if(lookDevelopment)lookDevelopment.RefreshForZoom();
  }
  // Render callbacks also cover direct capture cameras and a zoom changed after
  // LateUpdate. Refresh representation before drawing this actual world camera.
  void OnEnable(){renderedZoom=-1f;renderedAspect=-1f;UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering+=BeforeWorldRender;}
  void OnDisable(){UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering-=BeforeWorldRender;}
  void BeforeWorldRender(UnityEngine.Rendering.ScriptableRenderContext context,Camera camera)
  {
   if(!built||!host||camera!=host.WorldCamera)return;
   // Normal population timers handle state changes. Force only a changed view:
   // direct Camera.Render must be correct without cloning the gateway snapshot
   // again on every idle frame.
   if(Mathf.Abs(renderedZoom-camera.orthographicSize)<.0001f&&Mathf.Abs(renderedAspect-camera.aspect)<.0001f)return;
   UpdateLevelOfDetail(true);
   renderedZoom=camera.orthographicSize;renderedAspect=camera.aspect;
  }
  void LateUpdate(){if(built)UpdateLevelOfDetail(false);}
  public void RefreshForCapture(){if(built)UpdateLevelOfDetail(true);}
  public void SetBlockout(bool value){if(geography)geography.SetBlockout(value);}
  // Historical callers must not silently reconstruct old layers.
  public void CompareAtlas(bool baseline){if(baseline)throw new InvalidOperationException("Use the archived exact-commit baseline run; clean presentation has no legacy atlas.");}
  public void CompareDirectedLayout(bool before){if(before)throw new InvalidOperationException("Old composition is archived in Git, not built beneath this world.");}
  public static float SampleHeight(float x,float z)=>RegionOneBlenderLandscape.Height(x,z);
  public static bool IsCoast(float x,float z)=>RegionOneBlenderLandscape.Shore(x,z)<0;
  public static float CoastDistance(float x,float z)=>RegionOneBlenderLandscape.Shore(x,z);
  public static float CoastWaterMask(float x,float z)=>1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(-2,2,CoastDistance(x,z)));
  public static bool IsForest(float x,float z)=>RegionOneBlenderLandscape.Forest(x,z)>.1f;
  public static bool IsRiver(float x,float z)=>RegionOneBlenderLandscape.Water(x,z)>.1f;
  public static bool IsRoad(float x,float z)=>RegionOneBlenderLandscape.Road(x,z)>.1f;
  public static float RoadDistance(float x,float z)=>RegionOneBlenderLandscape.RoadDistance(x,z);
  public static float RiverCentre(float z)=>-104; // Legacy authoring API, not used by this presentation.
#if UNITY_EDITOR
  [Serializable] sealed class AuthoringSnapshot
  {public Vector3 origin;public Vector3[] vertices;public Vector2[] uv;public int[] triangles;public bool[] protectedWater;public string context;}
  public void ExportAuthoringSnapshot(string directory)
  {
   if(!built||!geography)throw new InvalidOperationException("Clean geography must be built before export.");
   string file=System.IO.Path.Combine(directory,"terrain-source.json");
   if(System.IO.File.Exists(file))throw new InvalidOperationException("Do not overwrite frozen evidence.");
   System.IO.Directory.CreateDirectory(directory);var mesh=geography.DirectedTerrain;var positions=geography.WorldVertices;
   var water=positions.Select(p=>IsCoast(p.x-origin.x,p.z-origin.z)||IsRiver(p.x-origin.x,p.z-origin.z)).ToArray();
   System.IO.File.WriteAllBytes(System.IO.Path.Combine(directory,"terrain-atlas.png"),geography.GeographicMask.EncodeToPNG());
   System.IO.File.WriteAllText(file,JsonUtility.ToJson(new AuthoringSnapshot{origin=origin,vertices=positions,uv=mesh.uv,triangles=mesh.triangles,protectedWater=water,context="Imported Blender GLB mesh transformed to actual Unity world coordinates. Semantic masks exported by Blender. This is verification, not authorship."}));
  }
#endif
  void OnDestroy(){for(int i=0;i<originals.Count;i++)if(originals[i])originals[i].enabled=states[i];for(int i=0;i<lods.Count;i++)if(lods[i])lods[i].enabled=lodStates[i];}
 }
}

