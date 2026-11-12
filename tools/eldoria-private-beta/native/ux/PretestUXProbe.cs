#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Eldoria.Domain;
using Eldoria.ReferenceClean;
using Eldoria.ReferenceClean.Editor;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using Object=UnityEngine.Object;

[InitializeOnLoad]
public static class EldoriaPretestUXProbe
{
 const string Flag="Eldoria.PretestUX.Active";
 static bool started;
 static readonly List<string> checks=new List<string>();
 static readonly List<string> errors=new List<string>();
 static string Out=>Environment.GetEnvironmentVariable("ELDORIA_UX_OUT");
 static readonly string[] Saves={"eldoria-unity-slice-v1.json","eldoria-unity-slice-owner-i-ii-v1.json"};
 static EldoriaPretestUXProbe(){EditorApplication.update+=Tick;Application.logMessageReceived+=Log;}
 static void Log(string message,string stack,LogType type){
  if(!SessionState.GetBool(Flag,false)||(type!=LogType.Exception&&type!=LogType.Error))return;
  // Same observed Editor-only diagnostic policy as StructuralStartupProbe.
  // Preserve its exact stack; every player/game exception remains fatal.
  if(message.StartsWith("ArgumentOutOfRangeException:")&&stack.Contains("UnityEditor.Search.SearchDatabase")&&stack.Contains("UnityEditor.Search.SearchInit.IndexationOnStartup")&&!stack.Contains("Eldoria.")){
   if(!string.IsNullOrEmpty(Out)){Directory.CreateDirectory(Out);File.AppendAllText(Path.Combine(Out,"editor-only-diagnostics.txt"),message+"\n"+stack+"\n");}return;
  }
  errors.Add(message+"\n"+stack);
 }
 static string Hash(string path){if(!File.Exists(path))return "absent";using(var stream=File.OpenRead(path))using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(stream));}
 static void Need(bool yes,string message){if(!yes)throw new InvalidOperationException(message);checks.Add(message);}
 public static void Run(){
  Directory.CreateDirectory(Out);
  foreach(var save in Saves)SessionState.SetString(Flag+save,Hash(Path.Combine(Application.persistentDataPath,save)));
  Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
  EditorSceneManager.OpenScene("Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity",OpenSceneMode.Single);
  SessionState.SetBool(Flag,true);EditorApplication.EnterPlaymode();
 }
 static void Tick(){if(!SessionState.GetBool(Flag,false)||!EditorApplication.isPlaying||started)return;var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();if(!host)return;started=true;host.StartCoroutine(Guard(host));}
 static IEnumerator Guard(ValoriaIntegratedWorld host){
  var run=Verify(host);
  while(true){object current;try{if(!run.MoveNext())break;current=run.Current;}catch(Exception e){File.WriteAllText(Path.Combine(Out,"checks.txt"),string.Join("\n",checks));File.WriteAllText(Path.Combine(Out,"failure.txt"),e.ToString());SessionState.SetBool(Flag,false);EditorApplication.Exit(1);yield break;}yield return current;}
  try{
   foreach(var save in Saves)Need(Hash(Path.Combine(Application.persistentDataPath,save))==SessionState.GetString(Flag+save,"missing-hash"),"canonical save unchanged: "+save);
   File.WriteAllText(Path.Combine(Out,"checks.txt"),string.Join("\n",checks));
   File.WriteAllText(Path.Combine(Out,"source.txt"),Environment.GetEnvironmentVariable("GITHUB_SHA"));
   File.WriteAllText(Path.Combine(Out,"complete.txt"),"PRETEST_UX_NATIVE_PASS_REQUIRES_VISUAL_REVIEW");
   SessionState.SetBool(Flag,false);EditorApplication.Exit(0);
  }catch(Exception e){File.WriteAllText(Path.Combine(Out,"checks.txt"),string.Join("\n",checks));File.WriteAllText(Path.Combine(Out,"failure.txt"),e.ToString());SessionState.SetBool(Flag,false);EditorApplication.Exit(1);}
 }
 static void Click(Button button){
  Need(button&&button.gameObject.activeInHierarchy&&button.IsInteractable(),"visible enabled button: "+(button?button.name:"absent"));
  var rt=(RectTransform)button.transform;var canvas=button.GetComponentInParent<Canvas>();
  var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
  var point=RectTransformUtility.WorldToScreenPoint(camera,rt.TransformPoint(rt.rect.center));
  var pointer=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,position=point};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
  Need(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==button,"button is actual top raycast: "+button.name);
  ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerDownHandler);
  ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerUpHandler);
  ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
 }
 static PlayerState Fixture(int level){
  var s=new PlayerState{PlayerName="Prueba Valoria",BastionLevel=level,SawmillLevel=level>1?1:0,BarracksLevel=level>1?1:0};
  s.Resources.Wood=5000;s.Resources.Stone=5000;s.Resources.Food=5000;
  s.ChapterProgress.SeenNarrative.AddRange(new[]{"start","world","mark","barracks","secured","frontier-open","fissure-closed","consolidate-valoria","productive-valoria","extraction-oddity"});return s;
 }
 static void Seed(ValoriaIntegratedWorld h,PlayerState s){
  Need(h.City.Probe&&Path.GetFullPath(h.City.SaveFile).StartsWith(Path.GetFullPath(Path.GetTempPath()),StringComparison.OrdinalIgnoreCase),"fixture restricted to disposable probe save");
  File.WriteAllText(h.City.SaveFile,JsonUtility.ToJson(s));h.City.ReloadForProbe();
 }
 static void Capture(ValoriaIntegratedWorld host,string key){ValoriaIntegrationBuild.Capture(host,"pretest-"+key+"-portrait.png",390,844);ValoriaIntegrationBuild.Capture(host,"pretest-"+key+"-landscape.png",844,390);}
 static void CheckClarityProjection(ValoriaIntegratedWorld h,bool buildings,string requiredId=null){
  var runtime=Object.FindFirstObjectByType<Eldoria.PlayerClarityRuntime>();Need(runtime,"actual level-label runtime exists");
  var field=typeof(Eldoria.PlayerClarityRuntime).GetField("markers",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
  var markers=(IDictionary)field.GetValue(runtime);int count=0;bool foundRequired=requiredId==null;
  foreach(DictionaryEntry item in markers){
   var marker=item.Value;var type=marker.GetType();
   var rect=(RectTransform)type.GetField("Rect").GetValue(marker);
   if(!rect||!rect.gameObject.activeInHierarchy||(bool)type.GetField("Building").GetValue(marker)!=buildings)continue;
   var target=(Transform)type.GetField("Target").GetValue(marker);var camera=(Camera)type.GetField("Camera").GetValue(marker);
   var expected=camera.WorldToScreenPoint(target.position+(Vector3)type.GetField("AnchorOffset").GetValue(marker));
   var canvas=rect.GetComponentInParent<Canvas>();
   if(buildings){expected.y-=32f;expected+=(Vector3)(new Vector2(24,-2)*canvas.scaleFactor);}
   var actual=RectTransformUtility.WorldToScreenPoint(canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,rect.position);
   Need(Vector2.Distance(expected,actual)<2f,"level label follows current camera frame: "+item.Key);count++;if((string)item.Key==requiredId)foundRequired=true;
  }
  Need(count>0,"measured real "+(buildings?"building":"world")+" level labels");
  Need(foundRequired,"visible enemy level measured across pan: "+requiredId);
 }
 static IEnumerator Verify(ValoriaIntegratedWorld h){
  float deadline=Time.realtimeSinceStartup+120;
  while(!h.Ready&&Time.realtimeSinceStartup<deadline)yield return null;
  Need(h.Ready&&h.City.Probe,"isolated ready probe");
  ValoriaIntegrationBuild.CompleteProbePlayerName(h);
  Seed(h,Fixture(1));yield return new WaitForSecondsRealtime(.7f);
  for(int page=0;page<20&&h.Hud.NarrativeOpen;page++){h.Hud.Modal.Extra.onClick.Invoke();yield return null;}
  h.Hud.SelectBuilding("");h.Hud.Refresh();yield return null;
  var nav=h.Hud.Navigation;
  Need(nav.Items.Where(i=>i.Button.gameObject.activeSelf).All(i=>i.Section==RoyalSection.Mailbox),"new player has mail and no premature bottom destinations");
  Click(nav.Items.Single(i=>i.Section==RoyalSection.Mailbox).Button);yield return new WaitForSecondsRealtime(.3f);
  var mailbox=h.Hud.GetComponent<RoyalGoldV41MailboxScreen>();Need(mailbox.IsOpen,"HUD opens actual mailbox");Capture(h,"mail");mailbox.Close();
  Seed(h,Fixture(2));yield return new WaitForSecondsRealtime(.7f);h.Hud.Refresh();yield return null;
  var world=nav.Items.Single(i=>i.Section==RoyalSection.World);Click(world.Button);yield return new WaitForSecondsRealtime(.4f);h.Hud.Refresh();
  Need(h.InWorld&&world.Label.text.IndexOf("CIUDAD",StringComparison.OrdinalIgnoreCase)>=0,"same world control becomes city");
  // The initial world overview deliberately hides node labels. Use the same
  // real near-view/focus path as canonical creature screenshot verification.
  var creature=h.City.Gateway.Snapshot().WorldCreatureLocations.FirstOrDefault(x=>x.Alive&&x.Level>0);
  string creatureId=creature?.TargetId??"corrupt-scout";
  var creatureTarget=h.FindTarget(creatureId);Need(creatureTarget,"live creature target exists");
  h.WorldNavigation.localZoom=Eldoria.World4XCanon.RegionOneCanonicalLandscape.NearZoom(h.PlayerCamera.aspect);
  h.WorldNavigation.SetZoomLevel(2,true);h.WorldNavigation.PanTo(creatureTarget.transform.position);
  h.PlayerCamera.Render();Canvas.ForceUpdateCanvases();h.Focus(creatureTarget.gameObject);
  yield return new WaitForSecondsRealtime(.6f);
  for(int frame=0;frame<3;frame++){
   h.WorldNavigation.PanTo(h.WorldNavigation.target+new Vector3(.4f,0,.2f));
   yield return null;yield return null;Canvas.ForceUpdateCanvases();CheckClarityProjection(h,false,creatureId);
  }
  Click(world.Button);yield return new WaitForSecondsRealtime(.4f);h.Hud.Refresh();
  Need(!h.InWorld&&world.Label.text.IndexOf("MUNDO",StringComparison.OrdinalIgnoreCase)>=0,"same city control becomes world");
  for(int frame=0;frame<3;frame++){
   h.City.PlayerCamera.transform.position+=new Vector3(.1f,0,.1f);
   yield return null;yield return null;Canvas.ForceUpdateCanvases();CheckClarityProjection(h,true);
  }
  h.Select("barracks");yield return new WaitForSecondsRealtime(.5f);
  var management=h.Hud.GetComponent<RoyalGoldV3ManagementScreens>();
  Need(h.Selected=="upgrade-barracks"&&!management.IsOpen,"barracks selection offers choice");
  var realTrainingButton=GameObject.Find("Barracks / enter training")?.GetComponent<Button>();
  Need(realTrainingButton,"actual barracks training button exists");
  var realTrainingCaption=realTrainingButton.GetComponentInChildren<RoyalText>(true);
  Need(realTrainingCaption && realTrainingCaption.text=="Entrenar","actual training caption retains its authored label");
  foreach(var viewport in new[]{new Vector2Int(390,844),new Vector2Int(844,390)}){
   ValoriaIntegrationBuild.Capture(h,"pretest-barracks-caption-"+viewport.x+"x"+viewport.y+".png",viewport.x,viewport.y);
   Canvas.ForceUpdateCanvases();
   Need(realTrainingCaption.enabled &&
        realTrainingCaption.cachedTextGenerator.characterCountVisible>=realTrainingCaption.text.Length &&
        realTrainingCaption.preferredHeight<=realTrainingCaption.rectTransform.rect.height+1f,
        "actual rendered barracks training caption fits every glyph at"+viewport.x+"x"+viewport.y);
  }
  Capture(h,"barracks-choice");yield return null;
  Click(GameObject.Find("Barracks / enter training")?.GetComponent<Button>());yield return new WaitForSecondsRealtime(.4f);
  Need(management.IsOpen&&management.CurrentDestination==RoyalGoldV3ManagementScreens.Destination.Training,"training choice opens training");
  Need(GameObject.Find("Switch destination")==null&&GameObject.Find("Open canonical heroes")==null,"training hides unexplained march and locked heroes");Capture(h,"training-initial");
  management.Close();h.Hud.SelectBuilding("");h.Select("barracks");yield return new WaitForSecondsRealtime(.4f);
  Click(h.Hud.GetComponent<RoyalGoldV2Hud>().VisibleBuildingButton);yield return new WaitForSecondsRealtime(.4f);
  Need(h.City.Gateway.Snapshot().BuildingCompletesUtcTicks>0,"barracks upgrade choice starts paid work");
  Capture(h,"barracks-upgrade");yield return null;
  var duplicate=h.Hud.GetComponentsInChildren<CanvasGroup>(true).FirstOrDefault(g=>g.name=="V3 / real-time operation / barracks");
  Need(!duplicate||duplicate.alpha<.1f,"selected barracks upgrade has no duplicate timer");
  Seed(h,Fixture(2));h.Hud.SelectBuilding("");h.Select("sawmill");yield return new WaitForSecondsRealtime(.4f);
  Click(h.Hud.GetComponent<RoyalGoldV2Hud>().VisibleBuildingButton);yield return new WaitForSecondsRealtime(.4f);
  Need(h.City.Gateway.Snapshot().BuildingCompletesUtcTicks>0,"visible upgrade starts paid work");Capture(h,"compact-selected-timer");h.Hud.SelectBuilding("");yield return new WaitForSecondsRealtime(.4f);Capture(h,"compact-timer");yield return null;Canvas.ForceUpdateCanvases();
  // Regression for the real owner-reported bug: city construction must
  // follow its world building, never stay parked in a right-hand HUD dock.
  var context=h.Hud.GetComponent<RoyalGoldV3ContextHud>();
  Need(context&&context.IsConfigured,"real scene anchor/camera contract available");
  var timerGroup=h.Hud.GetComponentsInChildren<CanvasGroup>(true)
   .FirstOrDefault(g=>g.name=="V3 / real-time operation / sawmill");
  Need(timerGroup&&timerGroup.alpha>.5f,"paid sawmill work has a visible countdown");
  var timerRect=(RectTransform)timerGroup.transform;
  var timerCanvas=timerRect.GetComponentInParent<Canvas>();
  var timerCamera=timerCanvas.renderMode==RenderMode.ScreenSpaceOverlay?null:timerCanvas.worldCamera;
  var building=context.FindAnchor("sawmill");
  Need(building&&building.gameObject.activeInHierarchy,"countdown has an active sawmill transform");
  var modelCollider=building.GetComponent<Collider>();
  if(!modelCollider)modelCollider=building.GetComponentInChildren<Collider>();
  Need(modelCollider&&modelCollider.enabled,"real sawmill collider available for projection");
  Func<float> modelCentreX=()=>{
   var bounds=modelCollider.bounds;float left=float.PositiveInfinity,right=float.NegativeInfinity;
   for(int i=0;i<8;i++){
    var corner=new Vector3((i&1)==0?bounds.min.x:bounds.max.x,
     (i&2)==0?bounds.min.y:bounds.max.y,(i&4)==0?bounds.min.z:bounds.max.z);
    var projected=h.City.PlayerCamera.WorldToScreenPoint(corner);
    if(projected.z<=h.City.PlayerCamera.nearClipPlane||
       projected.z>=h.City.PlayerCamera.farClipPlane)continue;
    left=Mathf.Min(left,projected.x);right=Mathf.Max(right,projected.x);
   }
   return float.IsInfinity(left)?float.NaN:(left+right)*.5f;
  };
  Func<Vector2> timerScreen=()=>RectTransformUtility.WorldToScreenPoint(timerCamera,timerRect.position);
  var modelXBefore=modelCentreX();var timerBefore=timerScreen();
  Need(!float.IsNaN(modelXBefore)&&
   Mathf.Abs(modelXBefore-timerBefore.x)<=42f*timerCanvas.scaleFactor,
   "sawmill timer is horizontally attached to its real building");
  var originalCameraPosition=h.City.PlayerCamera.transform.position;
  h.City.PlayerCamera.transform.position+=new Vector3(2f,0f,1f);
  yield return null;yield return null;Canvas.ForceUpdateCanvases();context.SynchronizeNow();
  var modelXAfter=modelCentreX();var timerAfter=timerScreen();
  Need(timerGroup.alpha>.5f,"building countdown remains visible after pan");
  Need(Vector2.Distance(timerBefore,timerAfter)>1f,
   "construction timer moves on-screen with the camera pan");
  Need(Mathf.Abs((timerAfter.x-timerBefore.x)-(modelXAfter-modelXBefore))
   <=42f*timerCanvas.scaleFactor,
   "construction timer tracks real projected building rather than fixed HUD X");
  h.City.PlayerCamera.transform.position+=new Vector3(2000f,0f,0f);
  yield return null;yield return null;Canvas.ForceUpdateCanvases();context.SynchronizeNow();
  Need(timerGroup.alpha<.5f,"timer hides when its building leaves the camera");
  h.City.PlayerCamera.transform.position=originalCameraPosition;
  yield return null;yield return null;Canvas.ForceUpdateCanvases();context.SynchronizeNow();
  Need(timerGroup.alpha>.5f,"countdown reappears beside the building on camera return");
  int labels=0;
  foreach(var label in Object.FindObjectsByType<BuildingLabel>(FindObjectsSortMode.None)){
   if(!label.Target||!label.PlayerCamera||!label.Select.gameObject.activeInHierarchy)continue;
   var expected=label.PlayerCamera.WorldToScreenPoint(label.Target.position+label.WorldOffset);
   var canvas=label.Layer.GetComponentInParent<Canvas>();
   var actual=RectTransformUtility.WorldToScreenPoint(canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera,label.transform.position);
   Need(Vector2.Distance(expected,actual)<2f,"label tracks target after camera movement: "+label.name);labels++;
  }
  Need(labels>0,"tested visible target labels");
  h.Hud.SelectBuilding("");yield return new WaitForSecondsRealtime(.3f);
  var hud=h.Hud.GetComponent<RoyalGoldV2Hud>();Click(hud.VisibleChapterButton);yield return new WaitForSecondsRealtime(.3f);
  Need(h.Hud.Mission.ListExpanded,"missions expand actual checklist");Capture(h,"missions-open");yield return null;
  var close=GameObject.Find("Close chapter overlay")?.GetComponent<Button>();Need(close.GetComponentsInChildren<RoyalIconGraphic>().Any(x=>x.Icon==11),"mission close has actual X geometry");Click(close);
  Need(!h.Hud.Mission.ListExpanded,"mission close collapses checklist");
  var mature=Fixture(3);mature.EngendroDefeated=true;mature.ChapterProgress.MarchConfirmed=true;mature.ChapterProgress.TrainedArchers=20;
  mature.ChapterProgress.SeenNarrative.AddRange(new[]{"march-preparation","vigilants-introduction","scouts-introduction"});
  Seed(h,mature);yield return new WaitForSecondsRealtime(.7f);
  var retention=h.Hud.GetComponent<RoyalGoldV3EarlyRetentionScreens>();retention.Open(RoyalGoldV3EarlyRetentionScreens.Destination.Vigilants);yield return new WaitForSecondsRealtime(.7f);
  Need(retention.IsOpen,"introduced Vigilants opens");Capture(h,"vigilants-static");yield return new WaitForSecondsRealtime(.25f);Canvas.ForceUpdateCanvases();
  var parallax=Object.FindFirstObjectByType<RoyalGoldV3VigilantsParallax>();Need(parallax,"Vigilants artwork exists");
  var planes=parallax.GetComponentsInChildren<RectTransform>();var positions=planes.Select(p=>p.anchoredPosition).ToArray();
  var scenic=parallax.transform.parent.GetComponentsInChildren<MaskableGraphic>().Where(g=>g is RoyalGoldV3LayeredFinish finish&&finish.CurrentScene==RoyalGoldV3LayeredFinish.Scene.Vigilants||g is RoyalGoldV3VigilantsParallax).ToArray();
  Need(scenic.Length>=2,"Vigilants painting and foreground both measured");
  var meshes=new List<Vector3[]>();
  foreach(var graphic in scenic){var mesh=graphic.canvasRenderer.GetMesh();Need(mesh,"rendered scenic mesh exists");meshes.Add(mesh.vertices);}
  yield return new WaitForSecondsRealtime(2);
  for(int i=0;i<scenic.Length;i++){var mesh=scenic[i].canvasRenderer.GetMesh();Need(mesh&&mesh.vertices.SequenceEqual(meshes[i]),"Vigilants scenic mesh stationary: "+scenic[i].name);}
  for(int i=0;i<planes.Length;i++)Need(Vector2.Distance(positions[i],planes[i].anchoredPosition)<.01f,"Vigilants artwork stationary: "+planes[i].name);
  Need(errors.Count==0,"no Unity errors: "+string.Join(" | ",errors));
 }
}
#endif
