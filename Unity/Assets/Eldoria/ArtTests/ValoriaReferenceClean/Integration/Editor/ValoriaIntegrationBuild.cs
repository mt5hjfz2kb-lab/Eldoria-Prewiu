#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Eldoria.Domain;
using Eldoria.Application;
using Eldoria.Presentation;
using Eldoria.SpawnlingBastion2;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
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
  static bool probeQueued;static double probeDeadline=-1;
  static ValoriaIntegrationBuild(){EditorApplication.playModeStateChanged+=Entered;EditorApplication.update+=StartQueuedProbe;}
  static void Need(bool pass,string why){if(!pass)throw new InvalidOperationException(why);}
  // Advanced native fixtures still pass through the real first-entry UI.
  // Keep this public entry point stable: the dialogue capture invokes it by
  // reflection. Post-merge UI certification must run it before selecting any
  // advanced screen, while the canonical main delivery lease is held.
  // The separate integration probe keeps its unnamed initial state and checks
  // invalid input, normalization, persistence and the first visible parchment.
  public static void CompleteProbePlayerName(ValoriaIntegratedWorld host){
   Need(host&&host.Ready&&host.City.Probe,"Only a disposable ready probe may complete fixture identity");
   var coordinator=host.Hud.GetComponent<Eldoria.PlayerClarity.RoyalPlayerEntryCoordinator>();
   Need(coordinator,"Native fixture requires the real first-entry coordinator");
   coordinator.Refresh();
   var before=host.City.Gateway.Snapshot();
   if(!string.IsNullOrWhiteSpace(before.PlayerName))return;
   var prompt=host.Hud.GetComponentInChildren<Eldoria.PlayerClarity.RoyalPlayerNamePrompt>(true);
   Need(prompt&&prompt.IsOpen,"Unnamed native fixture must expose the real name prompt");
   var input=prompt.GetComponentInChildren<InputField>(true);
   Need(input,"The real nickname input is required");
   input.text="Prueba Valoria";
   prompt.Confirm();
   coordinator.Refresh();
   var after=host.City.Gateway.Snapshot();
   Need(after.PlayerName=="Prueba Valoria"&&!coordinator.IsBlockingNarrative,
    "Native fixture name must persist through the real UI before advanced captures");
   Need(after.PlayerId==before.PlayerId&&JsonUtility.ToJson(after.Resources)==JsonUtility.ToJson(before.Resources),
    "Completing fixture identity must preserve player identity and resources");
  }
  public static async void Build(){try{
   Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
   var scene=EditorSceneManager.OpenScene("Assets/Eldoria/ArtTests/ValoriaReferenceClean/ArtVariants/SawmillCompleted/Valoria_SawmillCompleted.unity",OpenSceneMode.Single);
   var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();Need(city!=null,"Existing city gameplay missing");
   await ReplaceSawmill(city);
   var host=city.gameObject.AddComponent<ValoriaIntegratedWorld>();
   host.HudPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Eldoria/UI/ValoriaRoyalHUDV1/Prefabs/RoyalGoldHUD.prefab");Need(host.HudPrefab!=null,"Royal Gold prefab missing");
   host.LabelPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Eldoria/UI/ValoriaRoyalHUDV1/Prefabs/BuildingLabel.prefab");Need(host.LabelPrefab!=null,"Guidance label prefab missing");
   var plots=Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Plot_")).ToArray();
   Need(plots.Length>=4,"Expected authored plot slabs missing");foreach(var plot in plots)plot.enabled=false;
   foreach(var binding in city.Parcels.Bindings)foreach(var part in new[]{binding.Ground,binding.Available})if(part)foreach(var renderer in part.GetComponentsInChildren<Renderer>(true))renderer.enabled=false;
   city.Apply(new PlayerState{BastionLevel=2,SawmillLevel=1,BarracksLevel=1});
   ValoriaReferenceCleanBuilder.Capture(city.PlayerCamera,Path.Combine(Ev,"integration-city-art.png"),1600,900);city.Apply(new PlayerState());
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
  // Continue the committed integrated scene. The initial assembly path above is historical.
  public static async void Improve(){try{
   Directory.CreateDirectory(Ev);
   Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
   var scene=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
   var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();
   var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
   Need(city&&host&&host.HudPrefab&&host.WorldRoot,"Current consolidated scene missing");
   await ValoriaEnvironmentFinish.ApplyAsync(city,host,Ev);
   AssetDatabase.SaveAssets();Need(EditorSceneManager.SaveScene(scene,Scene),"Improved integrated scene save");
   BeginCurrentProbe();
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Path.Combine(Ev,"integration-failure.txt"),e.ToString());EditorApplication.Exit(1);}}
  public static async void VisualLeap(){try{
   Directory.CreateDirectory(Ev);Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
   var scene=EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
   var city=Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();
   var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
   Need(city&&host&&host.HudPrefab&&host.WorldRoot,"Consolidated playable source missing");
   ValoriaEnvironmentFinish.ApplyVisualLeap(city,host,Ev);
   ValoriaEnvironmentFinish.ApplyVisualPolish(city,host,Ev);
   ValoriaEnvironmentFinish.ApplyVisualPalette(city,host,Ev);
   ValoriaEnvironmentFinish.ApplyRoofSignature(city,host,Ev);
   ValoriaEnvironmentFinish.RestoreRoyalBanners(city,host,Ev);
   ValoriaEnvironmentFinish.ApplyWorldIdentityV10(city,host,Ev);
   ValoriaEnvironmentFinish.ApplyHostileCampIdentityV11(city,host,Ev);
   // Native165 compact building variant retained for historical visual review; ART_REJECTED.
   ValoriaEnvironmentFinish.ApplyCityCameraCompositionV14(city,host,Ev);
   // OWNER 2026-10-20: do not add a new urban precinct before its actual I-II gameplay unlock.
   // V16 retained as isolated historical .blend/.glb evidence, not a playable-scene addition.
   ValoriaEnvironmentFinish.ApplyPlayerFacingFidelityV17(city,host,Ev);
   // V13 soil-layer repaint rejected: genuine A/B at normal camera was imperceptible.
   // V12 road overlay rejected after genuine native A/B: light slabs and broken ground contact.
   // Retained V7–V9 visual experiments are archived and rejected; no promotion.
   AssetDatabase.SaveAssets();Need(EditorSceneManager.SaveScene(scene,Scene),"Visual leap scene save");
   BeginCurrentProbe();
  }catch(Exception e){Debug.LogException(e);File.WriteAllText(Path.Combine(Ev,"visual-leap-failure.txt"),e.ToString());EditorApplication.Exit(1);}}
  public static void ValidateCurrent(){try{
   Directory.CreateDirectory(Ev);Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
   EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);BeginCurrentProbe();
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}}
  static void BeginCurrentProbe(){
   foreach(string name in new[]{"eldoria-unity-slice-v1.json","eldoria-unity-slice-owner-i-ii-v1.json"})SessionState.SetString(Flag+name,FileHash(Path.Combine(UnityEngine.Application.persistentDataPath,name)));
   SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();
  }
  static Bounds ModelBounds(GameObject model){var rs=model.GetComponentsInChildren<Renderer>(true);Need(rs.Length>0,"Missing sawmill renderers");var b=rs[0].bounds;foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);return b;}
  static void SawmillViews(Camera c,string prefix){var position=c.transform.position;var rotation=c.transform.rotation;float size=c.orthographicSize;try{
   ValoriaReferenceCleanBuilder.Capture(c,Path.Combine(Ev,prefix+"-main.png"),1448,1086);
   c.transform.position=new Vector3(-42,20,9);c.transform.LookAt(new Vector3(-29,2,32));c.orthographicSize=11.5f;
   ValoriaReferenceCleanBuilder.Capture(c,Path.Combine(Ev,prefix+"-detail.png"),1448,1086);
  }finally{c.transform.SetPositionAndRotation(position,rotation);c.orthographicSize=size;}}
  static async System.Threading.Tasks.Task ReplaceSawmill(ValoriaReferenceCleanGameplay city){
   string source="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Sources/SawmillRefined104.glb";
   Need(File.Exists(source),"Reviewed local artist sawmill missing");
   Directory.CreateDirectory(Ev);city.Apply(new PlayerState{BastionLevel=2,SawmillLevel=1,BarracksLevel=1});
   var old=city.Parcels.LeftBuilt;var prior=ModelBounds(old);SawmillViews(city.PlayerCamera,"sawmill-refined-unity-before");
   var next=new GameObject("Sawmill / local AI authored workshop");next.transform.SetParent(old.transform.parent,false);
   using(var importer=new GltfImport(null,new UninterruptedDeferAgent())){
    Need(await importer.Load(new Uri(Path.GetFullPath(source)).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=4}),"Refined sawmill load");
    Need(await importer.InstantiateMainSceneAsync(next.transform),"Refined sawmill instantiate");
    foreach(var camera in next.GetComponentsInChildren<Camera>())Object.DestroyImmediate(camera);
    foreach(var light in next.GetComponentsInChildren<Light>())Object.DestroyImmediate(light);
    next.transform.localRotation=Quaternion.Euler(0,180,0);
    var bounds=ModelBounds(next);next.transform.localScale=Vector3.one*(Mathf.Max(prior.size.x,prior.size.z)/Mathf.Max(bounds.size.x,bounds.size.z));bounds=ModelBounds(next);
    next.transform.position+=new Vector3(prior.center.x-bounds.center.x,prior.min.y-bounds.min.y,prior.center.z-bounds.center.z);
    foreach(var r in next.GetComponentsInChildren<Renderer>()){GameObjectUtility.SetStaticEditorFlags(r.gameObject,0);r.lightmapIndex=-1;r.lightProbeUsage=UnityEngine.Rendering.LightProbeUsage.BlendProbes;r.reflectionProbeUsage=UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;}
    string folder=Root+"/Sawmill-"+Environment.GetEnvironmentVariable("GITHUB_RUN_ID");Directory.CreateDirectory(folder);Persist(next,folder);
   }
   var after=ModelBounds(next);Need(Mathf.Max(after.size.x,after.size.z)<10.8f&&after.size.y<10.4f,"Sawmill envelope");Need(Mathf.Abs(after.min.y-prior.min.y)<.005f,"Sawmill foundation floats");
   old.SetActive(false);SawmillViews(city.PlayerCamera,"sawmill-refined-unity-after");
   foreach(var binding in city.Parcels.Bindings)if(binding.BuildingId=="sawmill")binding.Built=next;
   city.Parcels.LeftBuilt=next;Object.DestroyImmediate(old);city.Apply(new PlayerState());
   File.WriteAllText(Path.Combine(Ev,"sawmill-refined-unity.json"),"{\"same_camera_light\":true,\"source_sha256\":\""+ValoriaContentPolicy.Hash(source)+"\",\"grounded\":true,\"art_approved\":false}");
  }
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
  static void Entered(PlayModeStateChange state){
   if(state!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Flag,false))return;
   SessionState.SetBool(Flag,false);probeQueued=true;probeDeadline=EditorApplication.timeSinceStartup+300;
  }
  // Editor Search can throw in delayCall before later callbacks execute. Use the
  // independent update path verified by the structural startup probe instead.
  static void StartQueuedProbe(){
   if(probeDeadline<0||!EditorApplication.isPlaying)return;
   try{
    Need(EditorApplication.timeSinceStartup<probeDeadline,"Canonical journey exceeded 300-second runtime deadline");
    if(!probeQueued)return;
    probeQueued=false;
    var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
    Need(host,"Canonical journey host missing");
    Debug.Log("VALORIA_INTEGRATION_PROBE_STARTED");
    host.StartCoroutine(GuardedProbe(host));
   }catch(Exception e){
    probeDeadline=-1;Debug.LogException(e);
    File.WriteAllText(Path.Combine(Ev,"integration-failure.txt"),e.ToString());
    EditorApplication.Exit(1);
   }
  }
  static IEnumerator GuardedProbe(ValoriaIntegratedWorld h){
   var probe=Probe(h);
   while(true){
    object current=null;bool moved=false;Exception failure=null;
    try{moved=probe.MoveNext();if(moved)current=probe.Current;}
    catch(Exception e){failure=e;}
    if(failure!=null){
     Debug.LogException(failure);
     File.WriteAllText(Path.Combine(Ev,"integration-failure.txt"),failure.ToString());
     EditorApplication.Exit(1);yield break;
    }
    if(!moved)yield break;
    yield return current;
   }
  }
  static IEnumerator Probe(ValoriaIntegratedWorld h){
   framingChecks.Clear();managementChecks.Clear();
   // Runtime initialization now spans frame boundaries. Test the real readiness
   // contract before issuing player actions; keep the existing journey assertions.
   float startupDeadline=Time.realtimeSinceStartup+90f;
   while(h&&!h.Ready&&Time.realtimeSinceStartup<startupDeadline)yield return null;
   Need(h&&h.Ready,"Integrated world initialization did not finish before journey validation");
   yield return null;yield return new WaitForSecondsRealtime(1);
   string failure=null;var checks=new List<string>();var city=h.GetComponent<ValoriaReferenceCleanGameplay>();
   bool Check(Action action,string name){if(failure!=null)return false;try{action();checks.Add(name);Debug.Log("VALORIA_INTEGRATION_CHECK_PASS "+name);return true;}catch(Exception e){failure=e.ToString();Debug.LogException(e);return false;}}
   Check(()=>{
    // Real first-entry on the original RoyalHUD, before the narrator.
    // Capture both orientations at the very first visible interaction.
    var naming=h.Hud.GetComponent<Eldoria.PlayerClarity.RoyalPlayerEntryCoordinator>();
    var parchment=h.Hud.GetComponentInChildren<Eldoria.PlayerClarity.RoyalPlayerNamePrompt>(true);
    Need(naming&&parchment&&parchment.IsOpen&&naming.IsBlockingNarrative,
      "First entry must open an actual editable nickname parchment");
    h.Guidance.PresentMilestone();
    Need(!h.Hud.NarrativeOpen,"The narrator must wait until the real name is saved");
    Capture(h,"integration-player-name-landscape.png",1600,900);
    Capture(h,"integration-player-name-portrait.png",900,1600);
    Capture(h,"integration-player-name-phone-landscape.png",844,390);
    Capture(h,"integration-player-name-phone-portrait.png",390,844);
    var editor=parchment.GetComponentInChildren<InputField>(true);
    var save=parchment.GetComponentInChildren<Button>(true);
    Need(editor&&save&&save.interactable&&save.targetGraphic&&save.targetGraphic.raycastTarget,
      "First entry needs a real raycastable UGUI input and confirmation");
    editor.text="AB";
    parchment.Confirm();
    Need(parchment.IsOpen&&string.IsNullOrEmpty(city.Gateway.Snapshot().PlayerName),
      "An invalid name must not dismiss or persist the prompt");
    editor.text="  Prueba   Valoria  ";
    parchment.Confirm();
    Need(!parchment.IsOpen&&city.Gateway.Snapshot().PlayerName=="Prueba Valoria",
      "Normalized name was not committed through the canonical gateway");
    var header=h.Hud.Realm.GetComponent<Eldoria.PlayerClarity.PlayerNameHeader>();
    Need(header,"Original realm header lacks the player name adapter");
    header.Refresh();
    // The original crest title is the persisted nickname; its original
    // second line is Power. No third label may overlap the portrait HUD.
    var nicknameLabel=h.Hud.Realm.Realm;
    var powerLabel=h.Hud.Realm.Power;
    Need(nicknameLabel&&nicknameLabel.gameObject.activeInHierarchy&&
      nicknameLabel.text=="Prueba Valoria",
      "The saved name must occupy the original crest title");
    Need(powerLabel&&powerLabel.gameObject.activeInHierarchy,
      "Power cannot disappear when the nameplate is displayed");
    Need(h.Hud.Realm.transform.Find("Character name")==null,
      "No duplicate third nameplate may cover the two-line crest");
    Canvas.ForceUpdateCanvases();
    var titleCorners=new Vector3[4];var powerCorners=new Vector3[4];
    nicknameLabel.rectTransform.GetWorldCorners(titleCorners);
    powerLabel.rectTransform.GetWorldCorners(powerCorners);
    Need(titleCorners[0].y>powerCorners[1].y+.4f,
      "Nickname must end above original Power text");
    Capture(h,"integration-player-name-saved-landscape.png",1600,900);
    Capture(h,"integration-player-name-saved-portrait.png",900,1600);
    Capture(h,"integration-player-name-saved-phone-landscape.png",844,390);
    Capture(h,"integration-player-name-saved-phone-portrait.png",390,844);
   },"editable first-entry parchment, invalid nick validation, saved display and real screenshots");
   // Native regression 42123550342: the parchment is removed and an
   // offscreen ScreenSpaceCamera proof restores ScreenSpaceOverlay within
   // one Editor frame. Without a real frame to re-register the visible
   // GraphicRaycaster targets, the next legitimate narrator button has
   // Graphic.depth=-1 and zero pointer hits (640x480). A player naturally
   // receives this rendered frame between name confirmation and dialogue.
   // Keep the strict real-raycast Click proof; NEVER invoke a hidden button.
   yield return null;
   yield return new WaitForSecondsRealtime(.12f);
   Canvas.ForceUpdateCanvases();
   Check(()=>{
    h.Guidance.PresentMilestone();
    Need(h.Hud.NarrativeOpen&&h.Hud.NarrativeSpeaker=="NARRADOR"&&h.Hud.NarrativeSourceText.Contains("La Brecha se abrió en la tierra junto a Valoria"),"Original introductory narrator missing");
    Need(h.Hud.NarrativeTurnCount==3,"Web narrator, Aldric and tutorial turns not preserved");
    // Native Editor startup may consume the typewriter's real-time budget before
    // this probe runs. Restart the SAME turn through the normal language API,
    // preserving source text, turn index and completion callback. Never change
    // runtime typewriter speed or weaken the real pointer/reveal assertion.
    string openingText=h.Hud.NarrativeSourceText;int openingTurn=h.Hud.NarrativeTurnIndex;
    Need(openingTurn==0,"Opening narrator already advanced before input probe");
    h.Hud.SetEnglish(true);h.Hud.SetEnglish(false);
    Need(h.Hud.NarrativeTurnIndex==openingTurn&&h.Hud.NarrativeSourceText==openingText&&h.Hud.NarrativeIsRevealing,"Opening reveal fixture did not preserve the canonical turn");
    Click(h.Hud.Modal.Extra);Need(h.Hud.NarrativeOpen&&!h.Hud.NarrativeIsRevealing&&h.Hud.NarrativeTurnIndex==openingTurn,"First touch must reveal, not skip a speaker");
    Capture(h,"integration-story.png",900,1600);
    Click(h.Hud.Modal.Extra);Need(h.Hud.NarrativeSpeaker=="Sir Aldric","Aldric did not follow narrator");
    Click(h.Hud.Modal.Extra);Need(h.Hud.NarrativeOpen,"Reveal skipped Aldric");
   },"original web opening narrator and Aldric appear through actual buttons");
   // Wait for the intentional .28s dialogueIn animation before judging the
   // panel's real visibility. Capturing at t=0 yielded a near-transparent
   // Aldric screenshot even though the dialogue passes its runtime checks.
   yield return new WaitForSecondsRealtime(.36f);
   Check(()=>{
    Need(h.Hud.Modal.Body.preferredHeight<=h.Hud.Modal.Body.rectTransform.rect.height,"Original Aldric introduction is clipped");
    Capture(h,"integration-story-aldric.png",900,1600);
    Click(h.Hud.Modal.Extra);Need(h.Hud.NarrativeSpeaker=="NARRADOR","Tutorial narrator missing");
    Click(h.Hud.Modal.Extra);
   },"Aldric portrait and text rendered at settled opacity");
   yield return new WaitForSecondsRealtime(.35f);
   Check(()=>{
    Capture(h,"integration-story-sawmill-tutorial.png",900,1600);
    Click(h.Hud.Modal.Extra);
    Need(!h.Hud.NarrativeOpen&&city.Gateway.Snapshot().ChapterProgress.SeenNarrative.Contains("start"),"Completion did not persist introduction");
    // General menus must not inherit portraits, oversized body geometry, or
    // the cinematic overlay after narrative return to ordinary Royal Gold.
    Click(h.Hud.Settings);
    Need(h.Hud.Modal.gameObject.activeSelf&&!h.Hud.NarrativeOpen,"Settings panel was replaced by narrator");
    var portraits=h.Hud.Modal.GetComponentsInChildren<RawImage>(true).Where(x=>x.name=="Canonical Aldric portrait");
    Need(portraits.All(x=>!x.gameObject.activeSelf),"Non-narrative menu inherited Aldric portrait");
   },"new player introduction saved and shared modal restored for settings");
   // After a cinematic closes, restoring Settings activates a previously hidden
   // button. In the same Editor call stack its Graphic has depth=-1 until the
   // next rendered frame; a human touch cannot arrive within that frame.
   // Keep the real EventSystem raycast and pointer handlers (no Invoke shortcut).
   yield return null;
   yield return new WaitForSecondsRealtime(.20f);
   Check(()=>{
    Need(h.Hud.Modal.gameObject.activeInHierarchy&&h.Hud.Modal.Close.gameObject.activeInHierarchy,"Settings modal or Close disappeared");
    Click(h.Hud.Modal.Close);
    Need(!h.Hud.Modal.gameObject.activeSelf,"Settings Close failed through real pointer");
   },"restored Settings closes through real rendered-button raycast");
   Check(()=>{Need(h.Ready,"Host not ready");Need(Object.FindObjectsByType<RoyalHUD>(FindObjectsSortMode.None).Length==1,"HUD count");Need(Camera.allCameras.Count(x=>x.enabled)==1,"City camera count");Need(!GameObject.Find("Eldoria HUD").GetComponent<Canvas>().enabled,"Legacy HUD still visible");Capture(h,"integration-city-16x9.png",1600,900);},"city HUD, one camera");
   Check(()=>{Need(h.Hud.Mission.ChapterStepCount==6&&h.Hud.Mission.CompletedStepCount==0,"New-player chapter objectives not projected from real state");Click(h.Hud.Mission.ListToggle);Need(h.Hud.Mission.ListExpanded,"Chapter list did not open through its visible button");Capture(h,"integration-chapter-list.png",1600,900);Click(h.Hud.Mission.ListToggle);Need(!h.Hud.Mission.ListExpanded,"Chapter list would not close");},"Royal Gold real chapter checklist and collapse");
   // The compact action becomes visible again AFTER a chapter closes.
   // Unity GraphicRegistry registers its raycast rectangle on the next
   // rendered frame; the real player cannot touch during the same stack.
   yield return null;
   yield return new WaitForSecondsRealtime(.15f);
   Check(()=>{Need(!h.CanOpen(RoyalSection.Heroes)&&!h.CanOpen(RoyalSection.Army),"Premature menus visible");Need(!string.IsNullOrWhiteSpace(h.Guidance.Story),"Original narrative missing");Need(Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Plot_")).All(r=>!r.enabled),"Plot slab remains visible");Click(h.Hud.Mission.Open);Need(h.Read("sawmill",false).Building!=null&&!h.InWorld,"Objective construction navigation failed");},"mechanic-gated menus, narrative, no slabs, objective navigation");
   yield return null;
   var initial=JsonUtility.ToJson(city.Gateway.Snapshot());
   Check(()=>{
    Need(!h.CanOpen(RoyalSection.World),"Initial world button unlocked");
    Need(!h.Select("gate")&&!h.InWorld,"Gate bypassed world unlock");
    h.Open(RoyalSection.World);Need(!h.InWorld,"Navigation bypassed world unlock");
    h.Navigate("Frontier");Need(!h.InWorld,"Context navigation bypassed world unlock");
    Need(!h.TrySetWorld(true)&&!h.InWorld,"SetWorld bypassed world unlock");
    Need(initial==JsonUtility.ToJson(city.Gateway.Snapshot()),"Blocked navigation mutated save");
    h.Hud.SelectBuilding("");
   },"all world entry points share progression gate");
   h.Select("sawmill");yield return new WaitForSecondsRealtime(.2f);
   Check(()=>{Capture(h,"integration-construction-landscape.png",1600,900);Capture(h,"integration-construction-portrait.png",900,1600);
    var preview=h.Hud.GetComponentInChildren<Eldoria.UI.RoyalGold.V2.RoyalGoldV3BuildingPreview>(true);
    Need(preview&&preview.HasModelPreview,"Sawmill card did not render the canonical building model");
   },"construction panel visible in both orientations");
   if(Check(()=>{Need(h.Hud.Building.Primary.Button.interactable,"Sawmill button unavailable");Click(h.Hud.Building.Primary.Button);Need(city.Gateway.Snapshot().BuildingCompletesUtcTicks>0,"Real Royal button did not build");},"Royal build command")){
    yield return new WaitForSecondsRealtime(.4f);
    Check(()=>{((ValoriaReferenceCleanGameplay.TestClock)city.Clock).Step(SliceRules.SawmillBuildSeconds+1);city.Gateway.Advance();city.Presenter.RefreshState();Need(city.Parcels.LeftBuilt.activeSelf,"Built scene model missing");h.Hud.Refresh();Capture(h,"integration-city-built.png",1600,900);},"shared timer and authored model");
   }
   // World-unlock narration can only exist once the Aserradero is BUILT.
   // This must run after its real gateway deadline completes, not before the
   // building purchase. Checking Sir Aldric while Beat is still "start"
   // incorrectly reports a runtime regression for the legitimate narrator.
   Check(()=>{
    h.Guidance.PresentMilestone();
    Need(!h.Hud.NarrativeOpen,"Automatic story must defer while the construction action is selected");
    h.Hud.SelectBuilding("");
   },"world introduction respects active construction context");
   yield return new WaitForSecondsRealtime(1.6f);
   Check(()=>{
    if(!h.Hud.NarrativeOpen)h.Guidance.PresentMilestone();
    Need(h.Hud.NarrativeOpen&&h.Hud.NarrativeSpeaker=="Sir Aldric",
      "Actual Aserradero-unlocked World narrative missing");
    for(int turn=0;turn<4&&h.Hud.NarrativeOpen;turn++)Click(h.Hud.Modal.Extra);
    Need(!h.Hud.NarrativeOpen,"World-unlock dialogue cannot be acknowledged");
    Need(city.Gateway.Snapshot().ChapterProgress.SeenNarrative.Contains("world"),
      "World-unlock narration was not persisted by its canonical callback");
   },"real World unlock narration after the built Aserradero");
   h.Select("sawmill");yield return null;
   Check(()=>{
    // Native regression for a real fault found on Windows Chromium in
    // native189: the V2 context sheet obscured the only World button.
    var nav=h.Hud.Navigation.Items.First(x=>x.Section==RoyalSection.World).Button;
    Need(nav&&nav.gameObject.activeInHierarchy&&nav.interactable,"World button not available after sawmill");
    h.Hud.GetComponent<Eldoria.UI.RoyalGold.V2.RoyalGoldV2Hud>().SynchronizeNow();
    Canvas.ForceUpdateCanvases();
    var corners=new Vector3[4];((RectTransform)nav.transform).GetWorldCorners(corners);
    var center=RectTransformUtility.WorldToScreenPoint(null,(corners[0]+corners[2])*.5f);
    var pointer=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=center};
    var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
    Need(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==nav.gameObject,
     "Context sheet obstructs real World button; screen="+Screen.width+"x"+Screen.height+
     "; point="+center+"; hits="+string.Join(",",hits.Select(x=>x.gameObject.name)));
   },"World navigation is touchable even while a building sheet is open");
   var before=JsonUtility.ToJson(city.Gateway.Snapshot());
   Check(()=>{Need(h.TrySetWorld(true),"Unlocked world remained blocked");var entryTarget=h.WorldNavigation.target;float entryZoom=h.WorldCamera.orthographicSize;var centre=new Vector3((h.WorldNavigation.minimum.x+h.WorldNavigation.maximum.x)/2,0,(h.WorldNavigation.minimum.y+h.WorldNavigation.maximum.y)/2);h.WorldNavigation.PanTo(centre);Debug.Log("ELDORIA_NATIVE_STRATEGIC_ENTRY size="+h.WorldCamera.orthographicSize+" aspect="+h.WorldCamera.aspect+" heading="+h.WorldNavigation.heading+" tilt="+h.WorldNavigation.tilt+" distance="+h.WorldNavigation.cameraDistance+" target="+h.WorldNavigation.target+" min="+h.WorldNavigation.minimum+" max="+h.WorldNavigation.maximum);var start=h.WorldNavigation.target;h.Pan(new Vector2(0,80));Debug.Log("ELDORIA_NATIVE_VERTICAL_PAN before="+start+" after="+h.WorldNavigation.target);Need(Mathf.Abs(h.WorldNavigation.target.z-start.z)>.5f,"Vertical map pan blocked");start=h.WorldNavigation.target;h.Pan(new Vector2(80,0));Need(Mathf.Abs(h.WorldNavigation.target.x-start.x)>.5f,"Horizontal map pan blocked");start=h.WorldNavigation.target;h.Pan(new Vector2(0,-80));Need(Mathf.Abs(h.WorldNavigation.target.z-start.z)>.5f,"Reverse vertical map pan blocked");start=h.WorldNavigation.target;h.Pan(new Vector2(-80,0));Need(Mathf.Abs(h.WorldNavigation.target.x-start.x)>.5f,"Reverse horizontal map pan blocked");h.Zoom(-3);h.SetWorld(false);h.SetWorld(true);Need(Vector3.Distance(entryTarget,h.WorldNavigation.target)<.02f&&Mathf.Abs(entryZoom-h.WorldCamera.orthographicSize)<.02f,"Returning world must restore the canonical Valoria near entry");h.Home();Need(Camera.allCameras.Count(x=>x.enabled)==1,"World camera count");Need(string.Equals(h.Hud.Navigation.Items.First(x=>x.Section==RoyalSection.World).Label.text,"Ciudad",StringComparison.OrdinalIgnoreCase),"World return label");Need(h.Read("",false).Realm=="ZONA 1","World realm label");Need(before==JsonUtility.ToJson(city.Gateway.Snapshot()),"Navigation mutated save");h.WorldNavigation.SetZoomLevel(1,true);float oldRegional=h.WorldCamera.orthographicSize;Capture(h,"integration-issue95-world-before-regional.png",1600,900);Capture(h,"integration-issue95-world-before-portrait.png",900,1600);h.WorldNavigation.EnterStrategicView(h.WorldHome);Need(h.WorldCamera.orthographicSize>oldRegional*1.45f,"World still lacks a genuinely larger strategic entry footprint");Capture(h,"integration-issue95-world-after-strategic.png",1600,900);Capture(h,"integration-issue95-world-after-portrait.png",900,1600);Capture(h,"integration-world-16x9.png",1600,900);Capture(h,"integration-world-portrait.png",900,1600);h.SetWorld(false);Need(before==JsonUtility.ToJson(city.Gateway.Snapshot()),"Return mutated save");},"world return restores near framing without mutating save");
   h.SetWorld(true);
   yield return new WaitForSecondsRealtime(.35f);
   Check(()=>CapturePlayerClarityNodes(h),"real resource and creature labels in four sizes without a modal");
   h.SetWorld(true);h.Select("forest-valoria");yield return null;
   Check(()=>{
    var v2=h.Hud.GetComponent<Eldoria.UI.RoyalGold.V2.RoyalGoldV2Hud>();
    v2.SynchronizeNow();Canvas.ForceUpdateCanvases();
    Need(v2.ObjectiveVisible&&v2.VisibleObjectiveButton.interactable,
     "A selected World resource hid the next real objective"+
     "; objectiveVisible="+v2.ObjectiveVisible+
     "; actionable="+v2.VisibleObjectiveButton.interactable+
     "; modalActive="+h.Hud.Modal.gameObject.activeSelf+
     "; narrative="+h.Hud.NarrativeOpen+
     "; selected="+h.Hud.SelectedBuildingId+
     "; missionExpanded="+h.Hud.Mission.ListExpanded+
     "; inWorld="+h.InWorld);
    var button=v2.VisibleObjectiveButton;
    var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);
    var point=RectTransformUtility.WorldToScreenPoint(null,(corners[0]+corners[2])*.5f);
    var evt=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=point};
    var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(evt,hits);
    Need(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==button.gameObject,
     "Next objective is masked by the World selection panel: "+string.Join(",",hits.Select(x=>x.gameObject.name)));
   },"Current objective is still genuinely clickable while a World resource is selected");
   // SynchronizeNow may activate a new V3 action graphic in the frame above.
   // Unity registers the raycast depth at the next rendered frame; a player
   // cannot physically press it in the same C# callback. Keep the genuine
   // EventSystem raycast and do not substitute a direct onClick.Invoke.
   yield return null;yield return new WaitForSecondsRealtime(.16f);
   Check(()=>{Need(h.Hud.Building.Primary.Button.interactable,"Forest command disabled");Click(h.Hud.Building.Primary.Button);Need(city.Gateway.Snapshot().March.Phase=="outbound","World gathering did not start");h.SetWorld(false);((ValoriaReferenceCleanGameplay.TestClock)city.Clock).Step(120);city.Gateway.Advance();city.Presenter.RefreshState();Need(city.Gateway.Snapshot().ChapterProgress.GatheredWood>0,"Shared world reward missing");var snapshot=JsonUtility.ToJson(city.Gateway.Snapshot());city.ReloadForProbe();Need(snapshot==JsonUtility.ToJson(city.Gateway.Snapshot()),"Reload changed rewards");},"world gathering, city return, saved reward");
   yield return new WaitForSecondsRealtime(1.6f);
   Check(()=>{
    h.Guidance.PresentMilestone();
    Need(h.Hud.NarrativeOpen&&h.Hud.NarrativeSpeaker=="NARRADOR"&&h.Hud.NarrativeSourceText.Contains("marca negra"),"Original gathering narrator missing");
    Click(h.Hud.Modal.Extra);
   },"narrator presents original forest mark without mixing voices");
   yield return new WaitForSecondsRealtime(.36f);
   Check(()=>{
    Capture(h,"integration-story-mark.png",900,1600);
    Need(h.Hud.Modal.Body.preferredHeight<=h.Hud.Modal.Body.rectTransform.rect.height,"Narrator text clipped");
    Click(h.Hud.Modal.Extra);Need(h.Hud.NarrativeSpeaker=="Sir Aldric","Original Aldric mark response missing");
    Click(h.Hud.Modal.Extra);
   },"forest mark narrator legible and next Aldric speaker acknowledged");
   yield return new WaitForSecondsRealtime(.36f);
   Check(()=>{
    Capture(h,"integration-story-mark-aldric.png",900,1600);
    Need(h.Hud.Modal.Body.preferredHeight<=h.Hud.Modal.Body.rectTransform.rect.height,"Aldric text clipped");
    Click(h.Hud.Modal.Extra);
    Need(city.Gateway.Snapshot().ChapterProgress.SeenNarrative.Contains("mark"),"Gathering dialogue not acknowledged");
   },"forest mark Aldric legible after animation and story ledger persisted");
   foreach(string node in new[]{"forest-valoria","quarry-valoria","corrupt-scout"}){
    h.SetWorld(true);h.Select(node);yield return null;
    // The contextual button was just reprojected from another target.
    // Allow its uGUI GraphicRegistry entry to become touchable.
    yield return new WaitForSecondsRealtime(.16f);
    Check(()=>{Need(h.Hud.Building.Primary.Button.interactable,"World command disabled: "+node);Click(h.Hud.Building.Primary.Button);Need(city.Gateway.Snapshot().March.Phase=="outbound","World march missing: "+node);},"Royal map action "+node);
    yield return new WaitForSecondsRealtime(.4f);
    Check(()=>{((ValoriaReferenceCleanGameplay.TestClock)city.Clock).Step(120);city.Gateway.Advance();city.Presenter.RefreshState();Need(city.Gateway.Snapshot().March.Phase=="idle","World march failed to return");},"World return "+node);
   }
   h.SetWorld(false);h.Select("bastion");yield return null;
   Check(()=>{Need(city.Gateway.Snapshot().JourneyComplete,"Chapter I incomplete");Click(h.Hud.Building.Primary.Button);Need(city.Gateway.Snapshot().BastionUpgradeCompletesUtcTicks>city.Clock.UtcTicks,"Upgrade did not begin");Capture(h,"integration-bastion-upgrade.png",1600,900);},"Royal Bastion upgrade");
   yield return new WaitForSecondsRealtime(.4f);
   Check(()=>{int seconds=BastionProgressionCatalog.ForLevel(2).DurationSeconds;((ValoriaReferenceCleanGameplay.TestClock)city.Clock).Step(seconds-1);city.Gateway.Advance();city.Presenter.RefreshState();Need(city.Gateway.Snapshot().BastionLevel==1,"Bastion II completed before authored deadline");((ValoriaReferenceCleanGameplay.TestClock)city.Clock).Step(1);city.Gateway.Advance();city.Presenter.RefreshState();Need(city.Gateway.Snapshot().BastionLevel==2&&city.BastionTwo.activeSelf&&!city.BastionOne.activeSelf,"Bastion II visual/progression mismatch");},"Bastion II visual");
   h.Select("barracks");yield return null;
   // Real 844x390 browser input exposed Barracks hidden behind the bottom
   // context sheet: the top-left quest left just 3% unobstructed scene height.
   // Capture the actual unbuilt plot before its normal construction command.
   // Capture itself checks full model framing against the genuine rendered HUD.
   Check(()=>{
    Capture(h,"integration-city-barracks-phone-landscape.png",844,390,()=>{
     var plot=h.FindTarget("barracks");Rect projected=default,visible=default;
     bool framed=plot&&h.IsTargetFramed(plot.gameObject,out projected,out visible);
     // The old available-plot renderers are intentionally hidden. In prior
     // native322 the collider geometry could pass framing while the player
     // saw no cue over the actual empty grass. Require the real existing
     // world-anchored label to remain visible for the selected unbuilt plot.
     var marker=h.Hud.GetComponentsInChildren<BuildingLabel>(true)
       .FirstOrDefault(item=>item&&item.name=="Guidance barracks");
     Need(marker&&marker.gameObject.activeInHierarchy&&
      marker.Select&&marker.Select.gameObject.activeInHierarchy&&
      marker.Label&&marker.Label.text.Contains("Cuartel"),
      "Selected unbuilt Barracks site is visually unmarked");
     Need(framed&&visible.height>.25f,
      "Landscape Barracks must fit beside the quest, above the bottom sheet"+
      " framed="+framed+" projected="+projected+" visible="+visible+
      " selected="+h.Selected+" aspect="+h.PlayerCamera.aspect+
      " zoom="+h.PlayerCamera.orthographicSize+
      " camera="+h.PlayerCamera.transform.position);
    });
    Capture(h,"integration-city-barracks-phone-portrait.png",390,844);
   },"Barracks construction plot fully visible in both phone orientations");
   yield return null;
   Check(()=>{Click(h.Hud.Building.Primary.Button);Need(city.Gateway.Snapshot().BuildingCompletesUtcTicks>0,"Barracks did not start");},"Royal barracks");
   yield return new WaitForSecondsRealtime(.4f);
   Check(()=>{((ValoriaReferenceCleanGameplay.TestClock)city.Clock).Step(SliceRules.BarracksBuildSeconds+1);city.Gateway.Advance();city.Presenter.RefreshState();Need(city.Parcels.RightBuilt.activeSelf,"Barracks model missing");h.Select("barracks");},"Royal barracks construction completed with native state");
   // The paid-building action only becomes visible after the finished
   // construction changes from a progress-only sheet to an actionable sheet.
   // A real finger cannot touch in that same immediate C# call stack.
   yield return null;
   yield return new WaitForSecondsRealtime(.20f);
   // The actual Bastion II narrative has priority over an independent
   // management destination. Finish its real pages before training, allowing
   // the suspended destination to reappear on the same city/camera.
   for(int page=0;page<12&&h.Hud.NarrativeOpen;page++)
   {
    Click(h.Hud.Modal.Extra);
    yield return new WaitForSecondsRealtime(.18f);
   }
   yield return null;
   yield return new WaitForSecondsRealtime(.18f);
   Check(()=>{
    var management=h.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
    Need(management!=null&&!management.IsOpen&&h.Selected=="upgrade-barracks","Barracks must offer a choice before entering training");
    var training=GameObject.Find("Barracks / enter training")?.GetComponent<RoyalButton>();
    Need(training&&training.interactable,"Barracks training choice absent");
    Capture(h,"pretest-barracks-choice-portrait.png",390,844);
    Capture(h,"pretest-barracks-choice-landscape.png",844,390);
    // Mission rewards are now collected consumables, never automatic wallet
    // credits. This disposable player deliberately uses its already-earned
    // packets before budgeting the complete first-training objective.
    int required=Math.Max(1,SliceContentProfiles.Active.Chapter2TrainArchers-
      city.Gateway.Snapshot().ChapterProgress.TrainedArchers);
    foreach(var packet in city.Gateway.Snapshot().ResourceRewards.Packets
      .Where(p=>p!=null&&p.Collected&&!p.Used&&(p.Kind=="wood"||p.Kind=="stone")).ToArray())
    {
     if(SliceRules.MaxAffordableArchers(city.Gateway.Snapshot())>=required)break;
     var beforeUse=city.Gateway.Snapshot();
     var used=city.Gateway.Execute(new GameCommand(Guid.NewGuid().ToString(),
       beforeUse.PlayerId,ResourceRewardCommands.UsePacket,packet.ReceiptId,beforeUse.Revision));
     Need(used.Ok,"Explicit use of an earned training-budget packet failed: "+packet.ReceiptId);
     Need(city.Gateway.Snapshot().ResourceRewards.Packets.Any(p=>
       p.ReceiptId==packet.ReceiptId&&p.Used),"Used training-budget packet not persisted");
    }
    Need(SliceRules.MaxAffordableArchers(city.Gateway.Snapshot())>=required,
      "Earned resources must fund the complete first-training objective");
    Click(training);
   },"barracks shows upgrade and training choices after deliberate packet use");
   yield return new WaitForSecondsRealtime(.2f);
   Check(()=>{
    var management=h.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
    Need(management!=null&&management.IsOpen&&
      management.CurrentDestination==RoyalGoldV3ManagementScreens.Destination.Training,
      "Built Barracks did not open the independent V3 training destination");
    var train=GameObject.Find("Confirm military command")?.GetComponent<RoyalButton>();
    Need(train&&train.interactable,"Real V3 train button unavailable");
    Click(train);
    Need(city.Gateway.Snapshot().RecruitmentCompletesUtcTicks>0,"Recruitment did not start");
    Need(city.Gateway.Snapshot().PendingRecruitArchers+
      city.Gateway.Snapshot().ChapterProgress.TrainedArchers>=
      SliceContentProfiles.Active.Chapter2TrainArchers,
      "Visible training command must queue the complete first-training objective");
    Need(h.Read("barracks",false).Building.OperationActive,"Training not identified as paid active operation");
    Need(!h.Read("barracks",false).Building.Requirements.Contains("Operación en curso"),"Duplicate training status remains");
    Capture(h,"integration-training.png",1600,900);
    Capture(h,"integration-training-portrait.png",900,1600);
    Capture(h,"integration-training-phone-landscape.png",844,390);
    Capture(h,"integration-training-phone-portrait.png",390,844);
   },"Royal recruitment via visible V3 primary button");
   yield return new WaitForSecondsRealtime(.4f);
   Check(()=>{
    ((ValoriaReferenceCleanGameplay.TestClock)city.Clock).Step(SliceRules.RecruitSecondsFor(SliceRules.RecruitArchers)+1);
    city.Gateway.Advance();city.Presenter.RefreshState();
    var management=h.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
    management.Close();h.Hud.SelectBuilding("");
    h.Guidance.FollowObjective();
    Need(h.Hud.NarrativeOpen,"First march needs its contextual explanation");
   },"march explanation precedes preparation");
   // Let the newly activated narrative canvas register its raycast graphics.
   yield return new WaitForSecondsRealtime(.2f);
   for(int page=0;page<12&&h.Hud.NarrativeOpen;page++){
    Click(h.Hud.Modal.Extra);yield return new WaitForSecondsRealtime(.18f);
   }
   yield return new WaitForSecondsRealtime(.3f);
   Check(()=>{
    var management=h.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
    Need(NarrativeCoherenceRules.Seen(city.Gateway.Snapshot(),"march-preparation"),"March introduction not saved");
    Need(management.IsOpen&&management.CurrentDestination==RoyalGoldV3ManagementScreens.Destination.March,"March did not open after explanation");
   },"Royal march action becomes available after introduction");
   // Canvas GraphicsRegistry must render its first active frame after the
   // recruitment panel was hidden. An immediate synthetic pointer in the same
   // call-stack cannot occur for a human finger and may have graphic.depth=-1.
   // Do NOT substitute Button.onClick.Invoke: the next step still tests uGUI
   // event-system raycasts and the actual primary action hit rectangle.
   yield return null;
   yield return new WaitForSecondsRealtime(.20f);
   Check(()=>{
    var management=h.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
    Need(management!=null&&management.IsOpen&&
      management.CurrentDestination==RoyalGoldV3ManagementScreens.Destination.March,
      "Native march preparation is not an independent management destination");
    Capture(h,"integration-march-landscape.png",1600,900);
    Capture(h,"integration-march-portrait.png",900,1600);
    Capture(h,"integration-march-phone-landscape.png",844,390);
    Capture(h,"integration-march-phone-portrait.png",390,844);
   },"Royal march management real independent landscape and portrait");
   Check(()=>{
    var prepare=GameObject.Find("Confirm military command")?.GetComponent<RoyalButton>();
    Need(prepare&&prepare.interactable,"Visible V3 march confirm unavailable");
    Click(prepare);
    Need(city.Gateway.Snapshot().MarchConfigured,"March configuration missing");
    var management=h.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
    if(management!=null&&management.IsOpen){
      var back=GameObject.Find("Return to Valoria")?.GetComponent<RoyalButton>();
      Need(back&&back.interactable,"V3 return-to-city button unavailable");
      Click(back);
      Need(!management.IsOpen,"Return to city failed to dismiss V3 management destination");
    }
   },"Royal march confirmation and real return-to-city button");
   yield return new WaitForSecondsRealtime(.4f);
   // Issue #95: preserve two controlled, authentic Unity frames of the
   // ALIVE Bastion II enemy, before the real existing combat/loot/clock probe.
   // Selection is restored before the existing authorization and march action.
   // No invented rival, shortcut, rewards or changed gameplay transitions.
   h.SetWorld(true);
   var spawnTarget=h.FindTarget("engendro-valoria");
   Need(spawnTarget!=null,"Authored Spawnling destination missing before battle");
   var animated=h.WorldRoot.GetComponentInChildren<SpawnlingEncounterVisual>(true);
   Need(animated&&animated.ModelRoot&&animated.ModelRoot.activeInHierarchy,
    "Real animated Spawnling not visible before its existing Bastion II fight");
   float nearPresence=animated.NearPresence,farPresence=animated.StrategicPresence;
   // Compare against the genuine world ground, not only the old mesh height.
   var prairie=h.WorldRoot.GetComponentsInChildren<MeshFilter>(true)
    .FirstOrDefault(f=>f.name=="ZoneOne continuous prairie source");
   Need(prairie&&prairie.sharedMesh,"Original world ground absent for Engendro foot test");
   var groundCollider=prairie.GetComponent<MeshCollider>();
   bool disposableGround=groundCollider==null;
   if(disposableGround)groundCollider=prairie.gameObject.AddComponent<MeshCollider>();
   var savedGroundMesh=groundCollider.sharedMesh;
   float groundY;
   try{
    groundCollider.sharedMesh=prairie.sharedMesh;Physics.SyncTransforms();
    Need(groundCollider.Raycast(new Ray(new Vector3(spawnTarget.transform.position.x,200,
      spawnTarget.transform.position.z),Vector3.down),out var hit,400),
      "No original terrain directly beneath Engendro");
    groundY=hit.point.y;
   }finally{
    if(disposableGround)Object.DestroyImmediate(groundCollider);
    else groundCollider.sharedMesh=savedGroundMesh;
   }
   Bounds CreatureBounds(){
    // The V4 rig has precomputed renderer animation culling bounds much
    // larger than the mesh itself. Measure actual transformed skin vertices,
    // exactly as original animated-model integration did. Otherwise a
    // false 160m bounding volume is mistaken for the creature's visible foot.
    Need(animated.TryGeometryBounds(out var geometry),"Animated V4 skin geometry not CPU-readable");
    Need(geometry.size.y>.05f,"Animated enemy mesh geometry is degenerate");
    return geometry;
   }
   void VerifyFoot(Bounds before,Bounds after,string level){
    Debug.Log("ISSUE95_B2_FOOT_SURVEY level="+level+" ground="+groundY.ToString("F4")+
     " beforeMin="+before.min.y.ToString("F4")+
     " afterMin="+after.min.y.ToString("F4")+
     " beforeXZ="+before.center.x.ToString("F3")+","+before.center.z.ToString("F3")+
     " afterXZ="+after.center.x.ToString("F3")+","+after.center.z.ToString("F3")+
     " hotspot="+spawnTarget.transform.position.ToString("F3")+
     " afterSize="+after.size.ToString("F3"));
    Need(Mathf.Abs(before.min.y-after.min.y)<.4f &&
     Vector2.Distance(new Vector2(before.center.x,before.center.z),
      new Vector2(after.center.x,after.center.z))<.6f,
      "Enlarged Engendro lost original foot/XY contact at "+level);
    Need(after.size.y>before.size.y*1.7f,"B2 visual growth insignificant at "+level);
    Need(Mathf.Abs(after.min.y-groundY)<.55f,
      "Engendro has lost CONTACT with actual existing terrain at "+level);
    Need(Vector2.Distance(new Vector2(after.center.x,after.center.z),
      new Vector2(spawnTarget.transform.position.x,spawnTarget.transform.position.z))<1.5f,
      "Engendro visual no longer aligned to real encounter hotspot at "+level);
   }
   h.Select("");
   h.WorldNavigation.EnterStrategicView(spawnTarget.transform.position);
   h.Focus(spawnTarget.gameObject);
   yield return null;
   animated.NearPresence=animated.StrategicPresence=1f;
   yield return null;
   var regionalBefore=CreatureBounds();
   Check(()=>Capture(h,"integration-art-engendro-before-strategic.png",1600,900),
    "real old-size model, fixed strategic camera before enlargement");
   animated.NearPresence=nearPresence;animated.StrategicPresence=farPresence;
   yield return null;
   Check(()=>{
    VerifyFoot(regionalBefore,CreatureBounds(),"strategic");
    Capture(h,"integration-art-engendro-prebattle-strategic.png",1600,900);
   },"real B2 strategic enlarged silhouette AND exact grounded anchor");
   h.WorldNavigation.SetZoomLevel(2,true);
   // This is an explicit local zoom (not a Focus() auto-fit back to macro).
   // Offset toward the screen's open centre while retaining the hotspot.
   h.WorldNavigation.PanTo(spawnTarget.transform.position+new Vector3(0,0,3f));
   Need(Mathf.Abs(h.WorldCamera.orthographicSize-h.WorldNavigation.localZoom)<.02f&&
    h.WorldCamera.orthographicSize<h.WorldNavigation.regionalZoom*.25f,
    "Local view zoom was silently lost: actual="+h.WorldCamera.orthographicSize+
    " configuredLocal="+h.WorldNavigation.localZoom);
   yield return null;
   animated.NearPresence=animated.StrategicPresence=1f;
   yield return null;
   var localBefore=CreatureBounds();
   Check(()=>Capture(h,"integration-art-engendro-before-close.png",1600,900),
    "real original 3m close-size comparison at identical camera");
   animated.NearPresence=nearPresence;animated.StrategicPresence=farPresence;
   yield return null;
   Check(()=>{
    VerifyFoot(localBefore,CreatureBounds(),"local");
    Capture(h,"integration-art-engendro-prebattle-close.png",1600,900);
   },"real B2 local enlarged silhouette AND exact grounded anchor");
   // The native316 "prebattle context" image still showed a genuine
   // pending Bastion-II dialogue. Advance it through the visible control,
   // not by hiding the overlay or mutating narrative state.
   for(int turn=0;turn<20&&h.Hud.NarrativeOpen;turn++){
    Click(h.Hud.Modal.Extra);
    yield return new WaitForSecondsRealtime(.35f);
   }
   Check(()=>Need(!h.Hud.NarrativeOpen&&!h.Hud.Modal.gameObject.activeSelf,
    "Unacknowledged dialogue obscures the selected Engendro"),
    "earned Bastion-II dialogue acknowledged with visible control");
   h.Select("engendro-valoria");yield return null;
   Check(()=>{
    Need(h.Selected=="engendro-valoria"&&!h.Hud.NarrativeOpen,
     "Prebattle proof must show real Engendro context, not dialogue");
    Capture(h,"integration-context-engendro-phone-landscape.png",844,390);
    Capture(h,"integration-context-engendro-phone-portrait.png",390,844);
   },
    "genuine selected Engendro context card in both mobile orientations");
   Check(()=>{Click(h.Hud.Building.Primary.Button);
    Need(city.Gateway.Snapshot().March.Phase=="outbound","Engendro march missing");
    var outbound=h.Read("engendro-valoria",false).Building;
    Need(outbound!=null&&outbound.OperationActive&&
     outbound.Function.Contains("La marcha ya ha salido")&&
     !outbound.Function.Contains("Confirma tu expedición antes de salir"),
     "The Engendro card must not ask to send a march that is already outbound");
    Capture(h,"integration-context-engendro-outbound-phone-landscape.png",844,390);
    Capture(h,"integration-context-engendro-outbound-phone-portrait.png",390,844,()=>{
     var viewport=h.Hud.GetComponentsInChildren<RectTransform>(true)
      .FirstOrDefault(x=>x.name=="Clipped text viewport");
     Need(viewport&&viewport.rect.height>=60f,
      "Portrait Engendro explanation must have a real multi-line scroll viewport");
    });
    ((ValoriaReferenceCleanGameplay.TestClock)city.Clock).Step(120);city.Gateway.Advance();city.Presenter.RefreshState();Need(SliceRules.CurrentObjectiveKey(city.Gateway.Snapshot())=="b2.complete","Bastion II content boundary not reached");Need(h.Read("",false).MissionSteps.Length==5&&h.Read("",false).MissionSteps.All(step=>step.Done),"Completed Bastion II checklist does not reflect actual saved progression");h.SetWorld(false);h.Hud.Refresh();Capture(h,"integration-chapter2-complete.png",1600,900);var saved=JsonUtility.ToJson(city.Gateway.Snapshot());city.ReloadForProbe();Need(saved==JsonUtility.ToJson(city.Gateway.Snapshot()),"Final save/rewards mismatch");},"Royal I-II chapter complete and reload");
   Check(()=>{foreach(string name in new[]{"eldoria-unity-slice-v1.json","eldoria-unity-slice-owner-i-ii-v1.json"})Need(SessionState.GetString(Flag+name,"")==FileHash(Path.Combine(UnityEngine.Application.persistentDataPath,name)),"Canonical save changed: "+name);},"canonical save byte hashes preserved");
   // Render the actual authored destinations through the same Royal UI and
   // production focus path that the player uses, after the progression probe.
   yield return null;yield return new WaitForSecondsRealtime(.5f);
   for(int turn=0;turn<16&&h.Hud.NarrativeOpen;turn++){Click(h.Hud.Modal.Extra);yield return new WaitForSecondsRealtime(.35f);}
   Check(()=>Need(!h.Hud.NarrativeOpen,"Closing narrative still obscures destination review"),"narrative closed through actual UI before art captures");
   foreach(string destination in new[]{"forest-valoria","quarry-valoria","corrupt-scout","engendro-valoria"}){
    h.SetWorld(true);h.Select(destination);yield return null;yield return new WaitForSecondsRealtime(.35f);
    for(int turn=0;turn<16&&h.Hud.NarrativeOpen;turn++){Click(h.Hud.Modal.Extra);yield return new WaitForSecondsRealtime(.35f);}
    Check(()=>{Need(!h.Hud.NarrativeOpen,"Narrative obscures destination at capture: "+destination);Capture(h,"integration-art-"+destination+"-landscape.png",1600,900);Capture(h,"integration-art-"+destination+"-portrait.png",900,1600);},"native authored destination capture "+destination);
   }
   h.SetWorld(false);h.Hud.Refresh();
   if(failure==null)yield return new JourneyContext(Check,()=>failure!=null).ContinueNativeJourneyToV(h);
   var report=new Report{source=Environment.GetEnvironmentVariable("GITHUB_SHA"),status=failure==null?"INTEGRATION_PASS_ART_REVIEW_REQUIRED":"FAIL",error=failure,checks=checks.ToArray(),scene=Scene,framing=framingChecks.ToArray(),management=managementChecks.ToArray()};
   if(failure==null) {
    File.WriteAllText(Path.Combine(Ev,"integration-input-manifest.txt"),ComputeInputManifest());
    File.WriteAllText(Path.Combine(Ev,"integration-input-signature.txt"),ComputeInputSignature());
   }
   File.WriteAllText(Path.Combine(Ev,"integration-result.json"),JsonUtility.ToJson(report,true));
   probeDeadline=-1;if(failure==null)Debug.Log("VALORIA_REFERENCE_CLEAN_INTEGRATION_COMPLETE");EditorApplication.Exit(failure==null?0:1);
  }
  sealed class JourneyContext {
   readonly Func<Action,string,bool> Check;
   readonly Func<bool> Failed;
   public JourneyContext(Func<Action,string,bool> check,Func<bool> failed){Check=check;Failed=failed;}
  // Continues the SAME saved I-II playthrough above. No prepared III/V save,
  // injected resources, direct progression flags, or bypassed command buttons.
  IEnumerator ReadJourneyNarrative(ValoriaIntegratedWorld h){
   yield return new WaitForSecondsRealtime(.3f);
   for(int turn=0;turn<40;turn++){
    var recruitment=h.Hud.GetComponent<Eldoria.HeroesUI.HeroesApprovedDestination>();
    if(recruitment&&recruitment.IsOpen){
     Check(()=>{
      Need(recruitment.CurrentHeroId=="lyra"&&h.City.Gateway.Snapshot().ChapterThree.LyraRecruited,
       "Recruitment presentation must show the actually recruited Lyra");
      bool wasInWorld=h.InWorld;
      Capture(h,"integration-lyra-recruitment-landscape.png",844,390);
      Capture(h,"integration-lyra-recruitment-portrait.png",390,844);
      Click(GameObject.Find("Close heroes").GetComponent<Button>());
      Need(!recruitment.IsOpen,"Recruitment close did not return to the journey");
      Need(h.InWorld==wasInWorld,"Recruitment close changed the player's underlying location");
     },"player reads and closes earned Lyra presentation");
     yield return new WaitForSecondsRealtime(.18f);
     if(Failed())yield break;
    }
    if(!h.Hud.NarrativeOpen){
     if(h.Hud.Modal.gameObject.activeSelf||!string.IsNullOrEmpty(h.Selected))yield break;
     yield return new WaitForSecondsRealtime(1.6f);
     h.Guidance.PresentMilestone();
     if(!h.Hud.NarrativeOpen)yield break;
    }
    if(h.Hud.NarrativeSpeaker=="Lyra")Check(()=>{
     Capture(h,"integration-continuous-lyra-landscape.png",1600,900);
     Capture(h,"integration-continuous-lyra-portrait.png",900,1600);
    },"continuous journey Lyra dialogue native captures");
    Check(()=>Click(h.Hud.Modal.Extra),"continuous journey reads canonical narrative");
    yield return new WaitForSecondsRealtime(.18f);
    if(Failed())yield break;
   }
  }
  IEnumerator FinishJourneyActivity(ValoriaIntegratedWorld h){
   var city=h.City;
   // Preserve all deadlines/rules; advance only the existing disposable QA clock.
   Check(()=>{
    ((ValoriaReferenceCleanGameplay.TestClock)city.Clock).Step(1800);
    city.Gateway.Advance();city.Presenter.RefreshState();h.Hud.Refresh();
    h.Hud.SelectBuilding("");
   },"continuous journey completes real deadlines");
   yield return new WaitForSecondsRealtime(.3f);
   yield return ReadJourneyNarrative(h);
  }
  IEnumerator JourneyAction(ValoriaIntegratedWorld h,string id,string kind){
   yield return ReadJourneyNarrative(h);
   var management=h.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
   if(management&&management.IsOpen)management.Close();
   h.SetWorld(id=="fissure-valoria"||id.StartsWith("hunt-")||id=="ash-stalker-valoria"||
    id=="forest-valoria"||id=="quarry-valoria"||id.StartsWith("region1:"));
   h.Select(id);yield return new WaitForSecondsRealtime(.25f);
   yield return ReadJourneyNarrative(h);
   if(Failed())yield break;
   long revision=h.City.Gateway.Snapshot().Revision;
   Check(()=>{
    var offer=h.Read(id,false).Building;
    Need(offer!=null&&offer.Action!=null&&offer.Action.Kind==kind&&offer.Action.Enabled,
     "Continuous actual offer missing: "+id+" / "+kind);
    Click(h.Hud.Building.Primary.Button);
   },"continuous native command "+kind+" "+id);
   if(Failed())yield break;
   // ContactLyra commits only after the player finishes its real dialogue.
   yield return ReadJourneyNarrative(h);
   Check(()=>Need(h.City.Gateway.Snapshot().Revision>revision,
    "Real button and narrative did not submit "+kind+" "+id),"continuous command committed "+kind+" "+id);
   if(Failed())yield break;
   yield return FinishJourneyActivity(h);
  }
  IEnumerator JourneyFunds(ValoriaIntegratedWorld h,int wood,int stone){
   for(int count=0;count<80;count++){
    var s=h.City.Gateway.Snapshot();
    if(s.Resources.Wood>=wood&&s.Resources.Stone>=stone)yield break;
    string kind=s.Resources.Wood<wood?"wood":"stone";
    string id=kind=="wood"?"forest-valoria":"quarry-valoria";
    if(LocalGateway.ResourceRemaining(s,id)==0){
     id=s.WorldResourceLocations.Where(x=>x.ResourceKind==kind&&x.Remaining>0&&h.FindTarget(x.TargetId))
      .Select(x=>x.TargetId).FirstOrDefault();
    }
    if(string.IsNullOrEmpty(id)){
     yield return FinishJourneyActivity(h);continue;
    }
    yield return JourneyAction(h,id,"Gather");
    if(Failed())yield break;
   }
   Check(()=>Need(false,"No canonical renewable resource met continuous journey budget"),"continuous real economy");
  }
  IEnumerator JourneyLevel(ValoriaIntegratedWorld h,int level,int wood,int stone){
   yield return JourneyFunds(h,wood,stone);
   if(Failed())yield break;
   yield return JourneyAction(h,"bastion","AdvanceBastion");
   if(Failed())yield break;
   Check(()=>{
    Need(h.City.Gateway.Snapshot().BastionLevel==level,"Continuous Bastion "+level+" not reached");
    var before=JsonUtility.ToJson(h.City.Gateway.Snapshot());h.City.ReloadForProbe();
    Need(before==JsonUtility.ToJson(h.City.Gateway.Snapshot()),"Continuous save/reward mismatch at "+level);
   },"continuous I-"+level+" native progression and reopened save");
   yield return new WaitForSecondsRealtime(.3f);
   h.Hud.SelectBuilding("");h.SetWorld(false);h.Home();
   Check(()=>{
    Capture(h,"integration-continuous-bastion-"+level+"-landscape.png",1600,900);
    Capture(h,"integration-continuous-bastion-"+level+"-portrait.png",900,1600);
   },"continuous Bastion "+level+" actual city in both orientations");
  }
  IEnumerator OpenJourneyTraining(ValoriaIntegratedWorld h){
   h.SetWorld(false);h.Hud.SelectBuilding("");yield return null;
   h.Select("barracks");yield return new WaitForSecondsRealtime(.3f);
   Check(()=>Click(GameObject.Find("Barracks / enter training")?.GetComponent<RoyalButton>()),
    "continuous journey opens training through actual barracks choice");
   yield return new WaitForSecondsRealtime(.3f);
  }
  IEnumerator VerifyHeroDestinations(ValoriaIntegratedWorld h){
   var before=JsonUtility.ToJson(h.City.Gateway.Snapshot());
   yield return OpenJourneyTraining(h);if(Failed())yield break;
   Check(()=>Click(GameObject.Find("Open canonical heroes").GetComponent<Button>()),"hero hall opens through actual units navigation");
   yield return new WaitForSecondsRealtime(.3f);
   var heroes=h.Hud.GetComponent<Eldoria.HeroesUI.HeroesApprovedDestination>();
   Check(()=>{
    Need(heroes&&heroes.IsOpen,"Integrated hero hall absent");
    Capture(h,"integration-heroes-hall-landscape.png",1600,900);
    Capture(h,"integration-heroes-hall-portrait.png",900,1600);
   },"hero hall actual controls and both orientations");
   // Captures rebuild the responsive hierarchy when restoring the real screen.
   // Let uGUI register its new graphics before sending genuine pointer input.
   yield return null;
   Check(()=>Click(GameObject.Find("Select hero aldric").GetComponent<Button>()),"select Aldric through actual hero card");
   yield return new WaitForSecondsRealtime(.3f);
   foreach(var page in new[]{"Resumen","Habilidades","Talentos","Equipamiento"}){
    Check(()=>Click(GameObject.Find("Hero tab "+page).GetComponent<Button>()),"genuine hero tab "+page);
    yield return new WaitForSecondsRealtime(.2f);
    Check(()=>{
     Need(heroes.CurrentAuthoritativeHero().Value.Health==SliceRules.HeroIntrinsicStats("aldric").Health,"Hero values do not match combat");
     Need(heroes.GetComponentInChildren<Eldoria.HeroesUI.HeroesModelSnapshot>(true).HasModel,"Aldric must use actual loaded 3D model");
     Capture(h,"integration-heroes-aldric-"+page.ToLowerInvariant()+"-landscape.png",1600,900);
     Capture(h,"integration-heroes-aldric-"+page.ToLowerInvariant()+"-portrait.png",900,1600);
    },"actual Aldric hero screen "+page);
    if(Failed())yield break;
    yield return null;
   }
   Check(()=>Click(GameObject.Find("Next available hero").GetComponent<Button>()),"real hero navigation selects narratively recruited Lyra");
   yield return new WaitForSecondsRealtime(.2f);
   Check(()=>{
    Need(heroes.CurrentHeroId=="lyra"&&h.City.Gateway.Snapshot().ChapterThree.LyraRecruited,"Lyra navigation violated narrative unlock");
    Capture(h,"integration-heroes-lyra-resumen-landscape.png",1600,900);
    Capture(h,"integration-heroes-lyra-resumen-portrait.png",900,1600);
   },"actual Lyra screen in both orientations");
   yield return null;
   Check(()=>{
    Click(GameObject.Find("Close heroes").GetComponent<Button>());
    Need(!heroes.IsOpen&&h.Hud.GetComponent<RoyalGoldV3ManagementScreens>().IsOpen,"Hero close did not return to units");
    h.Hud.GetComponent<RoyalGoldV3ManagementScreens>().Close();
    Need(before==JsonUtility.ToJson(h.City.Gateway.Snapshot()),"Hero browsing changed authoritative save");
   },"six actual hero screens preserve gameplay save and return navigation");
  }
  public IEnumerator ContinueNativeJourneyToV(ValoriaIntegratedWorld h){
   Check(()=>Need(h.City.Gateway.Snapshot().BastionLevel==2&&h.City.Gateway.Snapshot().EngendroDefeated,
    "Continuous III-V must start with the actual completed I-II state"),"continuous journey preserves original I-II lineage");
   yield return OpenJourneyTraining(h);if(Failed())yield break;
   Check(()=>{
    var heroes=h.Hud.GetComponent<Eldoria.HeroesUI.HeroesApprovedDestination>();
    Need(GameObject.Find("Open canonical heroes")==null,"Hero shortcut must stay hidden before Lyra recruitment");
    heroes.Open();
    Need(!heroes.IsOpen&&!heroes.OpenHero("lyra"),"Hero destination bypassed Bastion III narrative lock");
    h.Hud.GetComponent<RoyalGoldV3ManagementScreens>().Close();
   },"hero hall unlock waits for actual Lyra recruitment in Bastion III");
   if(Failed())yield break;
   yield return JourneyLevel(h,3,650,500);if(Failed())yield break;
   yield return JourneyFunds(h,240,180);if(Failed())yield break;
   yield return JourneyAction(h,"granary","Build");if(Failed())yield break;
   yield return JourneyAction(h,"fissure-valoria","InvestigateFissure");if(Failed())yield break;
   yield return JourneyAction(h,"fissure-valoria","ContactLyra");if(Failed())yield break;
   yield return JourneyAction(h,"fissure-valoria","Fight");if(Failed())yield break;
   yield return JourneyAction(h,"hunt-wolf","Fight");if(Failed())yield break;
   yield return JourneyAction(h,"hunt-boar","Fight");if(Failed())yield break;
   yield return JourneyFunds(h,3000,3000);if(Failed())yield break;
   foreach(var id in new[]{"sawmill","upgrade-barracks","granary"}){
    yield return JourneyAction(h,id,"UpgradeBuilding");if(Failed())yield break;
   }
   yield return JourneyLevel(h,4,850,700);if(Failed())yield break;
   h.Select("bastion");yield return new WaitForSecondsRealtime(.3f);
   yield return ReadJourneyNarrative(h);
   Check(()=>{
    // Build and council now share ONE city-local parchment. The
    // former full-canvas \"Choose civil\" modal was intentionally removed.
    var choice=h.Hud.GetComponentsInChildren<RoyalButton>(true)
        .FirstOrDefault(b=>b.name=="Building / council civil"&&
            b.gameObject.activeInHierarchy);
    var buildingUtility=h.Hud.GetComponent<RoyalGoldV3BuildingScreens>();
    Need(choice&&choice.interactable&&buildingUtility&&!buildingUtility.IsOpen,
        "Canonical IV council choice unavailable in the actual city cartouche");
    Click(choice);
    Need(h.City.Gateway.Snapshot().ChapterThree.DevelopmentPriority=="civil",
        "Inline royal council decision not saved");
   },"continuous IV real contextual council button");
   if(Failed())yield break;
   yield return JourneyLevel(h,5,1100,900);if(Failed())yield break;
   yield return JourneyFunds(h,500,0);if(Failed())yield break;
   yield return JourneyAction(h,"stoneworks","Build");if(Failed())yield break;
   Check(()=>Need(h.City.Gateway.Snapshot().ChapterThree.StoneworksLevel==1,
    "V quarry did not complete from continuous save"),"continuous V quarry real construction");
   // Finish the actual authored V objectives on this same uninterrupted save.
   foreach(var id in new[]{"sawmill","upgrade-barracks","granary","stoneworks"}){
    for(int level=id=="stoneworks"?1:2;level<5;level++){
     yield return JourneyFunds(h,4000,4000);if(Failed())yield break;
     yield return JourneyAction(h,id,"UpgradeBuilding");if(Failed())yield break;
    }
   }
   foreach(int tier in new[]{1,2}){
    yield return JourneyFunds(h,1000,600);if(Failed())yield break;
    yield return OpenJourneyTraining(h);if(Failed())yield break;
    yield return ReadJourneyNarrative(h);
    Check(()=>{
     var management=h.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
     Need(management&&management.IsOpen&&management.CurrentDestination==RoyalGoldV3ManagementScreens.Destination.Training,"V training unavailable");
     if(management.SelectedTier!=tier)Click(GameObject.Find("Select archer tier").GetComponent<RoyalButton>());
     GameObject.Find("Quantity slider").GetComponent<UnityEngine.UI.Slider>().value=tier==1?30:20;
    },"continuous V selects real training tier "+tier);
    yield return new WaitForSecondsRealtime(.2f);
    Check(()=>{
     Click(GameObject.Find("Confirm military command").GetComponent<RoyalButton>());
     Need(h.City.Gateway.Snapshot().RecruitmentCompletesUtcTicks>0,"V training command rejected");
    },"continuous V training real purchase "+tier);
    if(Failed())yield break;
    yield return FinishJourneyActivity(h);
    h.Hud.GetComponent<RoyalGoldV3ManagementScreens>().Close();
   }
   h.Select("march-main");yield return new WaitForSecondsRealtime(.3f);
   Check(()=>{
    var management=h.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
    Need(management&&management.IsOpen&&management.CurrentDestination==RoyalGoldV3ManagementScreens.Destination.March,"V preparation unavailable");
    // The second real hero slot adds the now narratively recruited Lyra.
    Click(GameObject.Find("Hero slot 1").GetComponent<RoyalButton>());
    if(management.SelectedTier!=1)Click(GameObject.Find("Select archer tier").GetComponent<RoyalButton>());
    GameObject.Find("Quantity slider").GetComponent<UnityEngine.UI.Slider>().value=20;
    Click(GameObject.Find("Select archer tier").GetComponent<RoyalButton>());
    GameObject.Find("Quantity slider").GetComponent<UnityEngine.UI.Slider>().value=20;
   },"continuous V actual two-hero mixed-tier preparation");
   yield return new WaitForSecondsRealtime(.2f);
   Check(()=>{
    Click(GameObject.Find("Confirm military command").GetComponent<RoyalButton>());
    var state=h.City.Gateway.Snapshot();
    Need(state.PreparedTroops.ArcherT1==20&&state.PreparedTroops.ArcherT2==20&&state.PreparedHeroId=="aldric+lyra","V prepared composition not persisted");
   },"continuous V preparation saved from genuine controls");
   if(Failed())yield break;
   yield return JourneyAction(h,"ash-stalker-valoria","Fight");if(Failed())yield break;
   Check(()=>Need(h.City.Gateway.Snapshot().ChapterThree.AshStalkerDefeated&&
    SliceRules.CurrentObjectiveKey(h.City.Gateway.Snapshot())=="b5.complete","Continuous V objectives not complete"),
    "continuous native I-V authored objectives complete");
   h.Hud.SelectBuilding("");h.SetWorld(false);h.Home();
   Check(()=>{
    var architecture=h.GetComponent<BastionIVVWorldPresentation>();
    foreach(var id in new[]{"bastion","sawmill","barracks","granary","stoneworks"}){
     var model=architecture.ModelFor(id);
     Need(model&&model.activeInHierarchy&&model.GetComponentsInChildren<Renderer>().Any(r=>r.enabled),
      "Completed V building invisible on actual city return: "+id);
    }
    Capture(h,"integration-continuous-bastion-5-complete-landscape.png",1600,900);
    Capture(h,"integration-continuous-bastion-5-complete-portrait.png",900,1600);
   },"continuous V completed city actual captures");
   var beforeFinal=JsonUtility.ToJson(h.City.Gateway.Snapshot());h.City.ReloadForProbe();
   Check(()=>Need(beforeFinal==JsonUtility.ToJson(h.City.Gateway.Snapshot()),"Continuous V final reopen changed state"),
    "continuous I-V final saved state exact");
   yield return VerifyHeroDestinations(h);
  }
  }

  static void CapturePlayerClarityNodes(ValoriaIntegratedWorld h){
   h.SetWorld(true);
   h.Hud.SelectBuilding("");h.Hud.Modal.Hide();
   var state=h.City.Gateway.Snapshot();
   var resources=state.WorldResourceLocations.Where(x=>x.Generation>=1&&x.Remaining>0).ToArray();
   var ids=resources.Where(x=>x.ResourceKind=="wood").Take(2).Select(x=>x.TargetId)
    .Concat(resources.Where(x=>x.ResourceKind=="stone").Take(2).Select(x=>x.TargetId))
    .Concat(state.WorldCreatureLocations.Where(x=>x.Alive&&x.Level>0).Take(1).Select(x=>x.TargetId)).ToArray();
   Need(ids.Length==5,"Clarity evidence requires two real wood nodes, two stone nodes and a live creature");
   Debug.Log("ELDORIA_PLAYER_CLARITY_DENSITY wood="+resources.Count(x=>x.ResourceKind=="wood")+
    " stone="+resources.Count(x=>x.ResourceKind=="stone")+" creatures="+state.WorldCreatureLocations.Count(x=>x.Alive)+
    " resource_quadrants="+string.Join(",",resources.GroupBy(x=>(x.X<h.WorldHome.x?"W":"E")+(x.Z<h.WorldHome.z?"S":"N"))
     .Select(group=>group.Key+":"+group.Count())));
   foreach(var id in ids){
    var target=h.FindTarget(id);
    Need(target&&target.gameObject.activeInHierarchy,"Live clarity target missing: "+id);
    foreach(var dimensions in new[]{new Vector2Int(844,390),new Vector2Int(390,844),new Vector2Int(1600,900),new Vector2Int(900,1600)}){
     string label=id.Replace(':','-').Replace('/','-');
     Action focus=()=>{
      h.WorldNavigation.localZoom=Eldoria.World4XCanon.RegionOneCanonicalLandscape.NearZoom(h.PlayerCamera.aspect);
      h.WorldNavigation.SetZoomLevel(2,true);
      h.WorldNavigation.PanTo(target.transform.position);
      // Camera-space Canvas matrices must follow this actual pan before
      // Focus reads its unobstructed viewport (runtime uses screen overlay).
      h.PlayerCamera.Render();Canvas.ForceUpdateCanvases();
      var before=h.WorldNavigation.target;
      h.Focus(target.gameObject);
      var method=typeof(ValoriaIntegratedWorld).GetMethod("FocusPoints",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
      var focusPoints=(System.Collections.Generic.List<Vector3>)method.Invoke(h,new object[]{target.gameObject});
      var projectedPoints=focusPoints.Select(p=>h.PlayerCamera.WorldToViewportPoint(p)).ToArray();
      Debug.Log("ELDORIA_FOCUS_DIAGNOSTIC "+id+" ready="+h.Ready+" spot="+target.Id+
       " targetBefore="+before+" targetAfter="+h.WorldNavigation.target+" limits="+h.WorldNavigation.minimum+"/"+h.WorldNavigation.maximum+
       " points="+projectedPoints.Length+" x="+projectedPoints.Min(p=>p.x)+".."+projectedPoints.Max(p=>p.x)+
       " y="+projectedPoints.Min(p=>p.y)+".."+projectedPoints.Max(p=>p.y)+
       " depth="+projectedPoints.Min(p=>p.z)+".."+projectedPoints.Max(p=>p.z)+
       " clip="+h.PlayerCamera.nearClipPlane+".."+h.PlayerCamera.farClipPlane+
       " anchor="+h.PlayerCamera.WorldToViewportPoint(Eldoria.PlayerClarityRuntime.LabelAnchor(target.gameObject,false)));
     };
     CapturePrepared(h,"integration-clarity-"+label+"-"+dimensions.x+"x"+dimensions.y+".png",dimensions.x,dimensions.y,()=>{
      Need(!h.Hud.Modal.gameObject.activeSelf&&!h.Hud.NarrativeOpen,"Node evidence cannot hide behind a modal");
      var marker=h.Hud.GetComponentsInChildren<RectTransform>().FirstOrDefault(x=>x.name=="Clarity / "+id);
      if(!marker||!marker.gameObject.activeInHierarchy){
       var runtime=Object.FindFirstObjectByType<Eldoria.PlayerClarityRuntime>();
       var all=h.Hud.GetComponentsInChildren<RectTransform>(true);
       var field=typeof(Eldoria.PlayerClarityRuntime).GetField("visibleViewport",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
       var targetBounds=target.GetComponent<Collider>().bounds;
       Debug.Log("ELDORIA_CLARITY_MISSING runtime="+(runtime?runtime.isActiveAndEnabled.ToString():"absent")+
        " world="+h.InWorld+" camera="+h.WorldCamera.enabled+" zoom="+h.WorldCamera.orthographicSize+"/"+h.WorldNavigation.globalZoom+
        " safe="+(runtime?field.GetValue(runtime).ToString():"none")+" target="+h.WorldCamera.WorldToScreenPoint(new Vector3(targetBounds.center.x,targetBounds.max.y+.7f,targetBounds.center.z))+
        " markers="+string.Join(",",all.Where(x=>x.name.StartsWith("Clarity / ")).Select(x=>x.name+":"+x.gameObject.activeInHierarchy))+
        " screens="+string.Join(",",h.Hud.GetComponents<MonoBehaviour>().Where(x=>x&&x.GetType().GetProperty("IsOpen")!=null).Select(x=>x.GetType().Name+":"+x.GetType().GetProperty("IsOpen").GetValue(x)))+
        " naming="+h.Hud.GetComponent<Eldoria.PlayerClarity.RoyalPlayerEntryCoordinator>()?.IsBlockingNarrative);
      }
      Need(marker&&marker.gameObject.activeInHierarchy,"Actual projected marker missing: "+id);
      var text=marker.GetComponentInChildren<Text>();
      int level=resources.FirstOrDefault(x=>x.TargetId==id)?.Level ??
       state.WorldCreatureLocations.Single(x=>x.TargetId==id).Level;
      Need(text&&text.text.Replace(" ","")=="Nv."+level,"Displayed tier disagrees with saved node: "+id);
      Need(marker.GetComponentsInChildren<Graphic>().All(x=>!x.raycastTarget),"Map labels intercept pointer input");
      if(resources.Any(x=>x.TargetId==id)){
       var pictogram=marker.GetComponentInChildren<Eldoria.PlayerClarity.RoyalResourcePictogram>();
       Need(pictogram&&pictogram.GetComponent<CanvasRenderer>()&&pictogram.canvasRenderer.materialCount>0,
        "Resource pictogram has no actual rendering component/material: "+id);
      }
      var collider=target.GetComponent<Collider>();
      Need(collider,"The resource must retain its actual clickable hotspot");
      var touch=h.PlayerCamera.WorldToScreenPoint(collider.bounds.center);
      Need(h.Pick(new Vector2(touch.x,touch.y))==target,"The real node is not selectable with its label visible: "+id);
      var camera=h.PlayerCamera;
      var corners=new Vector3[4];marker.GetWorldCorners(corners);
      Vector2 bottom=RectTransformUtility.WorldToScreenPoint(camera,corners[0]);
      Vector2 top=RectTransformUtility.WorldToScreenPoint(camera,corners[2]);
      Need(top.x-bottom.x>=65&&top.x-bottom.x<=80&&top.y-bottom.y>=24&&top.y-bottom.y<=32,
       "Projected label physical size invalid: "+id+" "+(top-bottom));
     },focus);
     // Matched presentation A/B: same saved state, model, camera and zoom.
     // Hide only this feature's non-interactive labels for the control image.
     var labelLayer=h.Hud.GetComponentsInChildren<RectTransform>(true)
      .FirstOrDefault(x=>x.name=="Player clarity / non-interactive projected labels");
     Need(labelLayer,"Native label layer is required for its matched control");
     try{
      labelLayer.gameObject.SetActive(false);
      CapturePrepared(h,"integration-clarity-"+label+"-"+dimensions.x+"x"+dimensions.y+"-without-labels.png",
       dimensions.x,dimensions.y,null,focus);
     }finally{labelLayer.gameObject.SetActive(true);}
    }
   }
   h.SetWorld(false);
  }

  public static string ComputeInputManifest(){
   var files=new HashSet<string>(AssetDatabase.GetDependencies(Scene,true).Where(File.Exists));
   foreach(var path in Directory.GetFiles("Assets/Eldoria","*.cs",SearchOption.AllDirectories))
    files.Add(path.Replace('\\','/'));
   foreach(var path in Directory.GetFiles("Assets/Eldoria","*.asmdef",SearchOption.AllDirectories))
    files.Add(path.Replace('\\','/'));
   foreach(var path in files.ToArray())if(File.Exists(path+".meta"))files.Add(path+".meta");
   return string.Join("\n",files.OrderBy(x=>x,StringComparer.Ordinal).Select(path=>path+" "+FileHash(path)));
  }
  public static string ComputeInputSignature(){
   using(var sha=System.Security.Cryptography.SHA256.Create())
    return BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(ComputeInputManifest())));
  }
  static string FileHash(string path){if(!File.Exists(path))return "ABSENT";using(var sha=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path)));}
  static void Click(Button button){
   var hud=Object.FindFirstObjectByType<RoyalHUD>();
   if(hud&&hud.NarrativeOpen&&button!=hud.Modal.Extra)
    for(int guard=0;guard<18&&hud.NarrativeOpen;guard++)Click(hud.Modal.Extra);
   Need(hud==null||!hud.NarrativeOpen||button==hud.Modal.Extra,"Narrative modal could not be read and closed");
   // Test the VISIBLE V2 button with a real uGUI raycast, never invoke a
   // hidden legacy control directly. Existing handlers still execute the
   // original commands and preserve save/progression assertions.
   var v2=hud?hud.GetComponent<RoyalGoldV2Hud>():null;
   var management=hud?hud.GetComponent<RoyalGoldV3ManagementScreens>():null;
   if(management&&management.IsOpen&&button==hud.Building.Primary.Button){
    // The canonical V3 destination covers the old contextual button. Test
    // the real visible full-screen confirmation with the same pointer raycast.
    button=GameObject.Find("Confirm military command")?.GetComponent<RoyalButton>();
   }
   else if(v2){
    v2.SynchronizeNow();
    if(button==hud.Mission.Open)button=v2.VisibleObjectiveButton;
    else if(button==hud.Mission.ListToggle&&!hud.Mission.ListExpanded)button=v2.VisibleChapterButton;
    else if(button==hud.Building.Primary.Button)button=v2.VisibleBuildingButton;
   }
   if(button==null||!button.interactable||!button.gameObject.activeInHierarchy){
    var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
    var site=host?host.FindTarget(hud.SelectedBuildingId):null;
    Debug.LogError("ELDORIA_ACTION_DIAGNOSTIC selected="+hud.SelectedBuildingId+
     " site="+(site?site.transform.position.ToString():"missing")+
     " active="+(site&&site.gameObject.activeInHierarchy)+
     " camera="+(host?host.PlayerCamera.transform.position.ToString():"missing")+
     " zoom="+(host?host.PlayerCamera.orthographicSize:0)+
     " viewport="+(site&&host?host.PlayerCamera.WorldToViewportPoint(site.transform.position).ToString():"missing"));
   }
   Need(button!=null&&button.interactable&&button.gameObject.activeInHierarchy,"Royal action not interactable: "+hud.SelectedBuildingId);Canvas.ForceUpdateCanvases();
   var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);var p=RectTransformUtility.WorldToScreenPoint(null,(corners[0]+corners[2])*.5f);
   var data=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=p};
   var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);Need(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==button.gameObject,"Royal action raycast blocked: "+button.name+" point="+p+" screen="+Screen.width+"x"+Screen.height+" buttonGraphicDepth="+button.targetGraphic.depth+" buttonGraphicCull="+button.targetGraphic.canvasRenderer.cull+" graphicEnabled="+button.targetGraphic.isActiveAndEnabled+" graphicRaycast="+button.targetGraphic.raycastTarget+" buttonRect="+((RectTransform)button.transform).rect+" graphicCanvas="+(button.targetGraphic.canvas?button.targetGraphic.canvas.name:"NONE")+" buttonPosition="+button.transform.position+" corners="+corners[0]+" to "+corners[2]+" parent="+button.transform.parent.name+" parentActive="+button.transform.parent.gameObject.activeInHierarchy+" hits="+string.Join(",",hits.Select(x=>x.gameObject.name+":"+x.depth)));
   ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerClickHandler);
  }
  // Preserve the original five-argument reflection API used by dialogue QA.
  public static void Capture(ValoriaIntegratedWorld h,string name,int width,int height,Action validateLayout=null){
   CapturePrepared(h,name,width,height,validateLayout,null);
  }
  static void CapturePrepared(ValoriaIntegratedWorld h,string name,int width,int height,Action validateLayout,Action prepareView){
   h.RefreshNavigation();
   bool managementOpen=h.Hud.GetComponent<RoyalGoldV3ManagementScreens>()?.IsOpen==true;
   if(managementOpen){
    var destination=GameObject.Find("V3 / Management destination");
    Need(destination!=null,"V3 management destination missing in native capture: "+name);
    var background=destination.GetComponent<Image>();
    var screen=destination.GetComponent<RectTransform>();
    Need(background!=null&&background.raycastTarget&&background.color.a>=.999f,
      "V3 management not opaque or touch-blocking: "+name);
    Need(screen.anchorMin==Vector2.zero&&screen.anchorMax==Vector2.one,
      "V3 management does not cover canvas: "+name);
    Need(!h.InWorld,"Management opened over regional world: "+name);
   }
   var rankings=h.Hud.GetComponent<RoyalGoldV3RankingScreen>();
   var buildings=h.Hud.GetComponent<RoyalGoldV3BuildingScreens>();
   var retention=h.Hud.GetComponent<RoyalGoldV3EarlyRetentionScreens>();
   bool buildingOpen=buildings&&buildings.IsOpen,rankingOpen=rankings&&rankings.IsOpen;
   var heroes=h.Hud.GetComponent<Eldoria.HeroesUI.HeroesApprovedDestination>();
   bool heroesOpen=heroes&&heroes.IsOpen;
   bool destinationOpen=managementOpen||buildingOpen||rankingOpen||heroesOpen;
   RectTransform destinationRoot=heroesOpen?heroes.VisibleRoot:rankingOpen?rankings.VisibleRoot:buildingOpen?buildings.VisibleRoot:managementOpen?GameObject.Find("V3 / Management destination").GetComponent<RectTransform>():null;
   var c=h.PlayerCamera;var canvas=h.Hud.GetComponent<Canvas>();var oldMode=canvas.renderMode;var oldCam=canvas.worldCamera;var oldDistance=canvas.planeDistance;var oldAspect=c.aspect;var oldMask=c.cullingMask;var oldRt=c.targetTexture;var oldActive=RenderTexture.active;var position=c.transform.position;float size=c.orthographicSize;var worldTarget=h.WorldNavigation.target;
   var scaler=h.Hud.GetComponent<RoyalScaler>();bool scalerEnabled=scaler.enabled;float scale=canvas.scaleFactor,canvasPixels=canvas.referencePixelsPerUnit;var scalerMode=scaler.uiScaleMode;var scalerResolution=scaler.referenceResolution;var scalerMatchMode=scaler.screenMatchMode;float scalerMatch=scaler.matchWidthOrHeight,scalerPixels=scaler.referencePixelsPerUnit;var areas=h.Hud.GetComponentsInChildren<RoyalSafeArea>(true);var areaEnabled=areas.Select(a=>a.enabled).ToArray();
   var rt=new RenderTexture(width,height,24);var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
   try{scaler.enabled=false;foreach(var a in areas)a.enabled=false;bool phone=Mathf.Min(width,height)<600;canvas.scaleFactor=width<height?width/(phone?600f:800f):height/(phone?600f:900f);foreach(var layout in h.Hud.GetComponentsInChildren<RoyalHudLayout>(true))layout.Apply(width/canvas.scaleFactor,true);c.targetTexture=rt;c.aspect=width/(float)height;
    // The world camera uses a different culling profile from the city camera.
    // An off-screen proof capture moves ScreenSpaceOverlay to ScreenSpaceCamera:
    // include Unity's existing UI layer (5) only during this capture or a real
    // Lyra dialog can appear in the game but disappear from the evidence PNG.
    c.cullingMask|=1<<5;
    if(h.InWorld)h.WorldNavigation.FitToAuthoredBounds();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=c;canvas.planeDistance=1;h.Guidance.Refresh();Canvas.ForceUpdateCanvases();var ui2=h.Hud.GetComponent<RoyalGoldV2Hud>();if(ui2)ui2.SynchronizeNow();Canvas.ForceUpdateCanvases();if(managementOpen)h.Hud.GetComponent<RoyalGoldV3ManagementScreens>().SynchronizeLayout();Canvas.ForceUpdateCanvases();if(retention&&retention.IsOpen)retention.SynchronizeLayout();if(buildings&&buildings.IsOpen)buildings.SynchronizeLayout();if(rankings&&rankings.IsOpen)rankings.SynchronizeLayout();if(heroesOpen)heroes.Refresh();Canvas.ForceUpdateCanvases();foreach(var label in h.Hud.GetComponentsInChildren<BuildingLabel>())label.RefreshPosition();
    // The normal game applies this after RoyalHUD.Refresh in LateUpdate.
    // Offscreen QA renders without advancing a frame, so use the SAME
    // existing title adapter after layout and before both real camera frames.
    var playerHeader=h.Hud.Realm?.GetComponent<Eldoria.PlayerClarity.PlayerNameHeader>();
    if(playerHeader)playerHeader.Refresh();
    // ScreenSpaceCamera Canvas matrices follow the camera during render.
    // After an offscreen pan, measuring them before that sync produced a
    // zero-width/inverted free viewport and hid every otherwise live marker.
    // Render the real camera once before measuring the same runtime overlay.
    c.Render();Canvas.ForceUpdateCanvases();
    // The target RenderTexture changes the camera-space Canvas matrices.
    // Apply the requested player focus only AFTER that first real render;
    // otherwise the free HUD viewport still belongs to the prior size.
    if(prepareView==null&&!destinationOpen&&!string.IsNullOrEmpty(h.Selected)){
     var selectedTarget=h.FindTarget(h.Selected=="march-main"?"barracks":h.Selected);
     if(selectedTarget)h.Focus(selectedTarget.gameObject);
     c.Render();Canvas.ForceUpdateCanvases();
    }
    if(prepareView!=null){
     Debug.Log("ELDORIA_CAPTURE_VIEW_READY "+name+" safe="+ui2.GetFreeSceneViewport());
     prepareView();c.Render();Canvas.ForceUpdateCanvases();
    }
    // Offscreen node evidence pans after the earlier label refresh. Reproject
    // existing guidance from that SAME final camera, never the previous shot.
    foreach(var label in h.Hud.GetComponentsInChildren<BuildingLabel>())label.RefreshPosition();
    var clarity=Object.FindFirstObjectByType<Eldoria.PlayerClarityRuntime>();
    if(clarity)clarity.RefreshForCapture();
    Canvas.ForceUpdateCanvases();// Two actual Unity camera renders allow the newly bound overlay canvas
    // to participate after its first SRP layout/batch registration.
    c.Render();c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Ev,name),tex.EncodeToPNG());validateLayout?.Invoke();
    foreach(var choice in h.Hud.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Building / council ",StringComparison.Ordinal)&&b.gameObject.activeInHierarchy)){
     var caption=choice.GetComponentInChildren<Text>();
     Need(caption&&caption.preferredHeight<=caption.rectTransform.rect.height+1f&&
      caption.preferredWidth<=caption.rectTransform.rect.width+1f,
      "Real council benefits must be fully readable before choosing: "+name+" / "+choice.name);
    }
    if(destinationOpen){
     Need(destinationRoot&&destinationRoot.gameObject.activeInHierarchy,"Actual management root not visible: "+name);
     var background=destinationRoot.GetComponent<Image>();var canvasRect=heroesOpen?(RectTransform)destinationRoot.parent:(RectTransform)canvas.transform;
     bool opaque=background&&background.color.a>=.999f;
     bool blocking=background&&background.raycastTarget;
     bool full=destinationRoot.anchorMin==Vector2.zero&&destinationRoot.anchorMax==Vector2.one&&Mathf.Abs(destinationRoot.rect.width-canvasRect.rect.width)<1f&&Mathf.Abs(destinationRoot.rect.height-canvasRect.rect.height)<1f;
     int controls=destinationRoot.GetComponentsInChildren<Button>().Count(b=>b.isActiveAndEnabled&&b.interactable&&((RectTransform)b.transform).rect.width>=20&&((RectTransform)b.transform).rect.height>=20);
     // Recruitment is earned at the fissure and overlays that world context.
     // Regular management still opens only in the city; all visual/input
     // checks apply to both destinations without bypassing the overlay.
     bool earnedWorldPresentation=heroesOpen&&heroes.IsRecruitmentPresentation&&
      heroes.CurrentHeroId=="lyra"&&h.City.Gateway.Snapshot().ChapterThree.LyraRecruited;
     Need(opaque&&blocking&&full&&controls>0&&(!h.InWorld||earnedWorldPresentation),
      "Native management must be opaque, full-canvas and interactive: "+name+
      " opaque="+opaque+" blocking="+blocking+" full="+full+" controls="+controls+" inWorld="+h.InWorld);
     if(buildingOpen&&!rankingOpen)Need(buildings.ReferenceTexture&&buildings.ReferenceTexture.width>100,"Native management reference not bound: "+name);
     managementChecks.Add(new ManagementCapture{capture=name,screen=heroesOpen?"heroes":rankingOpen?"rankings":buildingOpen?"building":"management",target=buildingOpen&&!rankingOpen?buildings.BuildingId:h.Selected,width=width,height=height,pass=true,opaque=opaque,touchBlocking=blocking,fullCanvas=full,activeControls=controls});
    }
    if(!destinationOpen&&!string.IsNullOrEmpty(h.Selected)){
     var focusTarget=h.FindTarget(h.Selected=="march-main"?"barracks":h.Selected);
     if(focusTarget){bool framed=h.IsTargetFramed(focusTarget.gameObject,out var projected,out var visible);
      framingChecks.Add(new Framing{capture=name,target=h.Selected,width=width,height=height,pass=framed,projected=Box(projected),visible=Box(visible)});
      Need(framed,"Selected model overlaps UI: "+name+" target="+h.Selected+" projected="+projected+" visible="+visible);
     }
    }
   }
   finally{scaler.enabled=scalerEnabled;scaler.uiScaleMode=scalerMode;scaler.referenceResolution=scalerResolution;scaler.screenMatchMode=scalerMatchMode;scaler.matchWidthOrHeight=scalerMatch;scaler.referencePixelsPerUnit=scalerPixels;canvas.scaleFactor=scale;canvas.referencePixelsPerUnit=canvasPixels;for(int i=0;i<areas.Length;i++)areas[i].enabled=areaEnabled[i];canvas.renderMode=oldMode;canvas.worldCamera=oldCam;canvas.planeDistance=oldDistance;c.targetTexture=oldRt;c.aspect=oldAspect;c.cullingMask=oldMask;c.transform.position=position;c.orthographicSize=size;if(h.InWorld)h.WorldNavigation.RestoreView(worldTarget,size);RenderTexture.active=oldActive;rt.Release();Object.Destroy(rt);Object.Destroy(tex);Canvas.ForceUpdateCanvases();foreach(var layout in h.Hud.GetComponentsInChildren<RoyalHudLayout>(true))layout.Apply(((RectTransform)layout.transform).rect.width);if(managementOpen)h.Hud.GetComponent<RoyalGoldV3ManagementScreens>().SynchronizeLayout();if(retention&&retention.IsOpen)retention.SynchronizeLayout();if(buildings&&buildings.IsOpen)buildings.SynchronizeLayout();if(rankings&&rankings.IsOpen)rankings.SynchronizeLayout();if(heroesOpen)heroes.Refresh();}
  }
  static readonly List<Framing> framingChecks=new List<Framing>();
  static readonly List<ManagementCapture> managementChecks=new List<ManagementCapture>();
  [Serializable] class ManagementCapture{public string capture,screen,target;public int width,height,activeControls;public bool pass,opaque,touchBlocking,fullCanvas;}
  static float[] Box(Rect value)=>new[]{value.xMin,value.yMin,value.xMax,value.yMax};
  [Serializable] class Framing{public string capture,target;public int width,height;public bool pass;public float[] projected,visible;}
  [Serializable] class Report{public string source,status,error,scene;public string[] checks;public Framing[] framing;public ManagementCapture[] management;public bool artApproved=false,physicalDeviceTested=false,timeAccelerated=true;}
 }
}
#endif


