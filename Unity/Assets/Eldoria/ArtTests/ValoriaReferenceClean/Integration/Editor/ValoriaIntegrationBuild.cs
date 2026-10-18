#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Eldoria.Domain;
using Eldoria.Presentation;
using Eldoria.UI.RoyalGold;
using Eldoria.World4XPrototype;
using GLTFast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Object=UnityEngine.Object;

namespace Eldoria.ReferenceClean.Editor {
 [InitializeOnLoad] public static class ValoriaIntegrationBuild {
  const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration";
  const string Scene=Root+"/Valoria_City_World_UI.unity";
  const string Flag="Valoria.IntegrationProbe";
  static string Ev=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../../docs/evidence/valoria-reference-clean"));
  static ValoriaIntegrationBuild(){EditorApplication.playModeStateChanged+=Entered;}
  static void Need(bool pass,string why){if(!pass)throw new InvalidOperationException(why);}
  public static async void Build(){try{
   Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
   var scene=EditorSceneManager.OpenScene("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Valoria_Reference_Clean.unity",OpenSceneMode.Single);
   var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();Need(city!=null,"Existing city gameplay missing");
   var host=city.gameObject.AddComponent<ValoriaIntegratedWorld>();
   host.HudPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Eldoria/UI/ValoriaRoyalHUDV1/Prefabs/RoyalGoldHUD.prefab");Need(host.HudPrefab!=null,"Royal Gold prefab missing");
   string source=Path.GetFullPath("Assets/Eldoria/ArtTests/World4XZone1/Models/ZoneOneSectorCandidate.glb");
   host.WorldRoot=new GameObject("Zone 1 / retained authored world");
   using(var importer=new GltfImport(null,new UninterruptedDeferAgent())){
    Need(await importer.Load(new Uri(source).AbsoluteUri),"World source load");Need(await importer.InstantiateMainSceneAsync(host.WorldRoot.transform),"World instantiate");
    host.WorldRoot.transform.position=new Vector3(1000,0,0);
    foreach(var c in host.WorldRoot.GetComponentsInChildren<Camera>())Object.DestroyImmediate(c);
    foreach(var l in host.WorldRoot.GetComponentsInChildren<Light>())Object.DestroyImmediate(l);
    var rs=host.WorldRoot.GetComponentsInChildren<Renderer>();Need(rs.Length>0,"World has no geometry");
    Directory.CreateDirectory(Ev);File.WriteAllLines(Path.Combine(Ev,"integration-world-object-names.txt"),rs.Select(x=>x.name));
    var ground=rs.OrderByDescending(x=>x.bounds.size.x*x.bounds.size.z).First();var bounds=ground.bounds;
    Need(bounds.size.x>200&&bounds.size.z>200,"World terrain axes/bounds");
    string folder=Root+"/Generated-"+Environment.GetEnvironmentVariable("GITHUB_RUN_ID");Directory.CreateDirectory(folder);Persist(host.WorldRoot,folder);
    // Explicit bindings only: additional generated nodes grant no resources.
    var names=new[]{("valoria-map-city","Existing medieval player city"),("forest-valoria","Wood resource existing timber stock"),("quarry-valoria","Existing Holotna rock.001"),("corrupt-scout","Enemy camp provisional existing canvas"),("engendro-valoria","Enemy camp existing shelter"),("old-watch-ruin","Recovered neutral ruin Arch_Gothic.fbx")};
    foreach(var pair in names){
     var authored=host.WorldRoot.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name==pair.Item2);Need(authored!=null,"Original world anchor missing: "+pair.Item2);
     var matched=authored.GetComponentsInChildren<Renderer>();Need(matched.Length>0,"World anchor has no renderer: "+pair.Item2);
     var anchor=matched[0].bounds;foreach(var renderer in matched)anchor.Encapsulate(renderer.bounds);var target=new GameObject("Zone1 target "+pair.Item1);target.transform.SetParent(host.WorldRoot.transform,true);target.transform.position=anchor.center;
     target.layer=8;target.AddComponent<WorldHotspot>().Id=pair.Item1;var box=target.AddComponent<BoxCollider>();box.isTrigger=true;box.size=new Vector3(Mathf.Clamp(anchor.size.x,8,20),Mathf.Max(5,anchor.size.y),Mathf.Clamp(anchor.size.z,8,20));
     if(pair.Item1=="valoria-map-city")host.WorldHome=new Vector3(anchor.center.x,0,anchor.center.z);
    }
    var camera=new GameObject("Zone 1 player camera").AddComponent<Camera>();camera.CopyFrom(city.Controls.playerCamera);camera.farClipPlane=900;camera.tag="MainCamera";
    camera.transform.SetParent(host.WorldRoot.transform,true);host.WorldCamera=camera;host.WorldNavigation=camera.gameObject.AddComponent<ZoneOneStrategicCamera>();
    host.WorldNavigation.minimum=new Vector2(bounds.min.x,bounds.min.z);host.WorldNavigation.maximum=new Vector2(bounds.max.x,bounds.max.z);host.WorldNavigation.enabled=false;
    host.WorldNavigation.PanTo(host.WorldHome);host.WorldNavigation.SetZoomLevel(1,true);camera.enabled=false;host.WorldRoot.SetActive(false);
    AssetDatabase.SaveAssets();Need(EditorSceneManager.SaveScene(scene,Scene),"Integration candidate save");
   }
   EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
   foreach(string name in new[]{"eldoria-unity-slice-v1.json","eldoria-unity-slice-owner-i-ii-v1.json"})SessionState.SetString(Flag+name,FileHash(Path.Combine(UnityEngine.Application.persistentDataPath,name)));
   SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Path.Combine(Ev,"integration-failure.txt"),e.ToString());EditorApplication.Exit(1);}}
  static void Persist(GameObject root,string folder){
   var meshes=new Dictionary<Mesh,Mesh>();var mats=new Dictionary<Material,Material>();var textures=new Dictionary<Texture,Texture>();int index=0;
   foreach(var f in root.GetComponentsInChildren<MeshFilter>()){if(!meshes.TryGetValue(f.sharedMesh,out var copy)){copy=Object.Instantiate(f.sharedMesh);AssetDatabase.CreateAsset(copy,folder+"/mesh-"+(index++)+".asset");meshes.Add(f.sharedMesh,copy);}f.sharedMesh=copy;}
   foreach(var r in root.GetComponentsInChildren<Renderer>()){var assigned=r.sharedMaterials;for(int i=0;i<assigned.Length;i++){var original=assigned[i];Need(original!=null&&original.shader!=null&&!original.shader.name.Contains("InternalError"),"World material invalid");if(!mats.TryGetValue(original,out var copy)){copy=new Material(original);foreach(var property in original.GetTexturePropertyNames()){var t=original.GetTexture(property);if(t==null)continue;if(!textures.TryGetValue(t,out var cloned)){cloned=ReadableCopy(t);AssetDatabase.CreateAsset(cloned,folder+"/texture-"+(index++)+".asset");textures.Add(t,cloned);}copy.SetTexture(property,cloned);}AssetDatabase.CreateAsset(copy,folder+"/material-"+(index++)+".mat");mats.Add(original,copy);}assigned[i]=copy;}r.sharedMaterials=assigned;}
  }
  static Texture2D ReadableCopy(Texture source){
   bool linear=!UnityEngine.Experimental.Rendering.GraphicsFormatUtility.IsSRGBFormat(source.graphicsFormat);
   var rt=RenderTexture.GetTemporary(source.width,source.height,0,RenderTextureFormat.ARGB32,linear?RenderTextureReadWrite.Linear:RenderTextureReadWrite.sRGB);var active=RenderTexture.active;
   try{Graphics.Blit(source,rt);RenderTexture.active=rt;var copy=new Texture2D(source.width,source.height,TextureFormat.RGBA32,true,linear){name=source.name,wrapMode=source.wrapMode,filterMode=source.filterMode,anisoLevel=source.anisoLevel};copy.ReadPixels(new Rect(0,0,source.width,source.height),0,0);copy.Apply(true,false);return copy;}
   finally{RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);}
  }
  static void Entered(PlayModeStateChange state){if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Flag,false))return;SessionState.SetBool(Flag,false);EditorApplication.delayCall+=()=>{var h=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();if(h==null){EditorApplication.Exit(1);return;}h.StartCoroutine(Probe(h));};}
  static IEnumerator Probe(ValoriaIntegratedWorld h){
   yield return null;yield return new WaitForSecondsRealtime(1);
   string failure=null;var checks=new List<string>();var city=h.GetComponent<ValoriaReferenceCleanGameplay>();
   bool Check(Action action,string name){if(failure!=null)return false;try{action();checks.Add(name);return true;}catch(Exception e){failure=e.ToString();Debug.LogException(e);return false;}}
   Check(()=>{Need(h.Ready,"Host not ready");Need(Object.FindObjectsByType<RoyalHUD>(FindObjectsSortMode.None).Length==1,"HUD count");Need(Camera.allCameras.Count(x=>x.enabled)==1,"City camera count");Need(!GameObject.Find("Eldoria HUD").GetComponent<Canvas>().enabled,"Legacy HUD still visible");Capture(h,"integration-city-16x9.png",1600,900);},"city HUD, one camera");
   var before=JsonUtility.ToJson(city.Gateway.Snapshot());
   Check(()=>{h.SetWorld(true);Need(Camera.allCameras.Count(x=>x.enabled)==1,"World camera count");Need(before==JsonUtility.ToJson(city.Gateway.Snapshot()),"Navigation mutated save");Capture(h,"integration-world-16x9.png",1600,900);Capture(h,"integration-world-portrait.png",900,1600);h.SetWorld(false);Need(before==JsonUtility.ToJson(city.Gateway.Snapshot()),"Return mutated save");},"world return preserves session");
   h.Select("sawmill");yield return null;
   if(Check(()=>{Need(h.Hud.Building.Primary.Button.interactable,"Sawmill button unavailable");Click(h.Hud.Building.Primary.Button);Need(city.Gateway.Snapshot().BuildingCompletesUtcTicks>0,"Real Royal button did not build");},"Royal build command")){
    yield return new WaitForSecondsRealtime(.4f);
    Check(()=>{((ValoriaReferenceCleanGameplay.TestClock)city.Clock).Step(SliceRules.SawmillBuildSeconds+1);city.Gateway.Advance();city.Presenter.RefreshState();Need(city.Parcels.LeftBuilt.activeSelf,"Built scene model missing");h.Hud.Refresh();Capture(h,"integration-city-built.png",1600,900);},"shared timer and authored model");
   }
   h.SetWorld(true);h.Select("forest-valoria");yield return null;
   Check(()=>{Need(h.Hud.Building.Primary.Button.interactable,"Forest command disabled");Click(h.Hud.Building.Primary.Button);Need(city.Gateway.Snapshot().March.Phase=="outbound","World gathering did not start");h.SetWorld(false);((ValoriaReferenceCleanGameplay.TestClock)city.Clock).Step(120);city.Gateway.Advance();city.Presenter.RefreshState();Need(city.Gateway.Snapshot().ChapterProgress.GatheredWood>0,"Shared world reward missing");var snapshot=JsonUtility.ToJson(city.Gateway.Snapshot());city.ReloadForProbe();Need(snapshot==JsonUtility.ToJson(city.Gateway.Snapshot()),"Reload changed rewards");},"world gathering, city return, saved reward");
   yield return new WaitForSecondsRealtime(.5f);
   foreach(string node in new[]{"forest-valoria","quarry-valoria","corrupt-scout"}){
    h.SetWorld(true);h.Select(node);yield return null;
    Check(()=>{Need(h.Hud.Building.Primary.Button.interactable,"World command disabled: "+node);Click(h.Hud.Building.Primary.Button);Need(city.Gateway.Snapshot().March.Phase=="outbound","World march missing: "+node);},"Royal map action "+node);
    yield return new WaitForSecondsRealtime(.4f);
    Check(()=>{((ValoriaReferenceCleanGameplay.TestClock)city.Clock).Step(120);city.Gateway.Advance();city.Presenter.RefreshState();Need(city.Gateway.Snapshot().March.Phase=="idle","World march failed to return");},"World return "+node);
   }
   h.SetWorld(false);h.Select("bastion");yield return null;
   Check(()=>{Need(city.Gateway.Snapshot().JourneyComplete,"Chapter I incomplete");Click(h.Hud.Building.Primary.Button);Need(city.Gateway.Snapshot().BastionUpgradeCompletesUtcTicks>city.Clock.UtcTicks,"Upgrade did not begin");Capture(h,"integration-bastion-upgrade.png",1600,900);},"Royal Bastion upgrade");
   yield return new WaitForSecondsRealtime(.4f);
   Check(()=>{((ValoriaReferenceCleanGameplay.TestClock)city.Clock).Step(11);city.Gateway.Advance();city.Presenter.RefreshState();Need(city.Gateway.Snapshot().BastionLevel==2&&city.BastionTwo.activeSelf&&!city.BastionOne.activeSelf,"Bastion II visual/progression mismatch");},"Bastion II visual");
   h.Select("barracks");yield return null;
   Check(()=>{Click(h.Hud.Building.Primary.Button);Need(city.Gateway.Snapshot().BuildingCompletesUtcTicks>0,"Barracks did not start");},"Royal barracks");
   yield return new WaitForSecondsRealtime(.4f);
   Check(()=>{((ValoriaReferenceCleanGameplay.TestClock)city.Clock).Step(SliceRules.BarracksBuildSeconds+1);city.Gateway.Advance();city.Presenter.RefreshState();Need(city.Parcels.RightBuilt.activeSelf,"Barracks model missing");h.Select("barracks");Click(h.Hud.Building.Primary.Button);Need(city.Gateway.Snapshot().RecruitmentCompletesUtcTicks>0,"Recruitment did not start");Capture(h,"integration-training.png",1600,900);},"Royal recruitment");
   yield return new WaitForSecondsRealtime(.4f);
   Check(()=>{((ValoriaReferenceCleanGameplay.TestClock)city.Clock).Step(SliceRules.RecruitSeconds+1);city.Gateway.Advance();city.Presenter.RefreshState();h.Select("march-main");Click(h.Hud.Building.Primary.Button);Need(city.Gateway.Snapshot().MarchConfigured,"March configuration missing");},"Royal march confirmation");
   yield return new WaitForSecondsRealtime(.4f);
   h.SetWorld(true);h.Select("engendro-valoria");yield return null;
   Check(()=>{Click(h.Hud.Building.Primary.Button);Need(city.Gateway.Snapshot().March.Phase=="outbound","Engendro march missing");((ValoriaReferenceCleanGameplay.TestClock)city.Clock).Step(120);city.Gateway.Advance();city.Presenter.RefreshState();Need(SliceRules.CurrentObjectiveKey(city.Gateway.Snapshot())=="b2.complete","Chapter II incomplete");h.SetWorld(false);h.Hud.Refresh();Capture(h,"integration-chapter2-complete.png",1600,900);var saved=JsonUtility.ToJson(city.Gateway.Snapshot());city.ReloadForProbe();Need(saved==JsonUtility.ToJson(city.Gateway.Snapshot()),"Final save/rewards mismatch");},"Royal I-II chapter complete and reload");
   Check(()=>{foreach(string name in new[]{"eldoria-unity-slice-v1.json","eldoria-unity-slice-owner-i-ii-v1.json"})Need(SessionState.GetString(Flag+name,"")==FileHash(Path.Combine(UnityEngine.Application.persistentDataPath,name)),"Canonical save changed: "+name);},"canonical save byte hashes preserved");
   var report=new Report{source=Environment.GetEnvironmentVariable("GITHUB_SHA"),status=failure==null?"INTEGRATION_PASS_ART_REVIEW_REQUIRED":"FAIL",error=failure,checks=checks.ToArray(),scene=Scene};
   File.WriteAllText(Path.Combine(Ev,"integration-result.json"),JsonUtility.ToJson(report,true));
   if(failure==null)Debug.Log("VALORIA_REFERENCE_CLEAN_INTEGRATION_COMPLETE");EditorApplication.Exit(failure==null?0:1);
  }
  static string FileHash(string path){if(!File.Exists(path))return "ABSENT";using(var sha=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path)));}
  static void Click(RoyalButton button){
   Need(button!=null&&button.interactable&&button.gameObject.activeInHierarchy,"Royal action not interactable");Canvas.ForceUpdateCanvases();
   var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);var p=RectTransformUtility.WorldToScreenPoint(null,(corners[0]+corners[2])*.5f);
   var data=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=p};
   var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);Need(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==button.gameObject,"Royal action raycast blocked: "+button.name+" point="+p+" screen="+Screen.width+"x"+Screen.height+" hits="+string.Join(",",hits.Select(x=>x.gameObject.name+":"+x.depth)));
   ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerClickHandler);
  }
  static void Capture(ValoriaIntegratedWorld h,string name,int width,int height){
   var c=h.PlayerCamera;var canvas=h.Hud.GetComponent<Canvas>();var oldMode=canvas.renderMode;var oldCam=canvas.worldCamera;var oldDistance=canvas.planeDistance;var oldAspect=c.aspect;var oldRt=c.targetTexture;var oldActive=RenderTexture.active;var position=c.transform.position;float size=c.orthographicSize;
   var scaler=h.Hud.GetComponent<RoyalScaler>();bool scalerEnabled=scaler.enabled;float scale=canvas.scaleFactor;var areas=h.Hud.GetComponentsInChildren<RoyalSafeArea>(true);var areaEnabled=areas.Select(a=>a.enabled).ToArray();
   var rt=new RenderTexture(width,height,24);var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
   try{scaler.enabled=false;foreach(var a in areas)a.enabled=false;canvas.scaleFactor=width<height?width/800f:height/900f;foreach(var layout in h.Hud.GetComponentsInChildren<RoyalHudLayout>(true))layout.Apply(width/canvas.scaleFactor);c.targetTexture=rt;c.aspect=width/(float)height;if(h.InWorld)h.WorldNavigation.FitToAuthoredBounds();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=c;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Ev,name),tex.EncodeToPNG());}
   finally{scaler.enabled=scalerEnabled;canvas.scaleFactor=scale;for(int i=0;i<areas.Length;i++)areas[i].enabled=areaEnabled[i];canvas.renderMode=oldMode;canvas.worldCamera=oldCam;canvas.planeDistance=oldDistance;c.targetTexture=oldRt;c.aspect=oldAspect;c.transform.position=position;c.orthographicSize=size;RenderTexture.active=oldActive;rt.Release();Object.Destroy(rt);Object.Destroy(tex);Canvas.ForceUpdateCanvases();foreach(var layout in h.Hud.GetComponentsInChildren<RoyalHudLayout>(true))layout.Apply(((RectTransform)layout.transform).rect.width);}
  }
  [Serializable] class Report{public string source,status,error,scene;public string[] checks;public bool artApproved=false,physicalDeviceTested=false;}
 }
}
#endif
