using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;
using Eldoria.Presentation;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using GLTFast;

namespace Eldoria.ReferenceClean.Editor {
 [InitializeOnLoad]
 public static class ValoriaReferenceCleanGameplayBuild {
  const string ProbeFlag="ValoriaReferenceClean.GameplayProbe";
  static ValoriaReferenceCleanGameplayBuild(){EditorApplication.playModeStateChanged+=Entered;}
  const string Root="Assets/Eldoria/ArtTests/ValoriaReferenceClean/";
  const string ScenePath=Root+"Valoria_Reference_Clean.unity";
  const string AssetsPath=Root+"GameplayAssets";
  static string Ev=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..","docs/evidence/valoria-reference-clean"));
  static HashSet<int> saved=new HashSet<int>();static int index;
  static void Check(bool ok,string why){if(!ok)throw new Exception("VALORIA_GAMEPLAY: "+why);}
  static void Persist(UnityEngine.Object o){if(o==null||AssetDatabase.Contains(o)||!saved.Add(o.GetInstanceID()))return;if(!(o is Mesh)&&!(o is Material)&&!(o is Texture2D))return;AssetDatabase.CreateAsset(o,AssetsPath+"/Gameplay_"+(index++).ToString("D5")+(o is Material?".mat":".asset"));}
  static Bounds Bounds(GameObject o){var rs=o.GetComponentsInChildren<Renderer>(true);Check(rs.Length>0,"No renderers");var b=rs[0].bounds;foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);return b;}
  static async Task<GameObject> Model(string source,string name,Transform parent,Vector3 centre,float width){
   var g=new GltfImport(null,new UninterruptedDeferAgent());Check(await g.Load(new Uri(Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"..","..",source))).AbsoluteUri,new ImportSettings{GenerateMipMaps=true,AnisotropicFilterLevel=4}),"GLB "+source);
   var o=new GameObject(name);o.transform.SetParent(parent,false);Check(await g.InstantiateMainSceneAsync(o.transform),"Instantiate "+name);
   var b=Bounds(o);o.transform.localScale=Vector3.one*(width/Mathf.Max(b.size.x,b.size.z));b=Bounds(o);o.transform.position+=new Vector3(centre.x-b.center.x,.56f-b.min.y,centre.z-b.center.z);
   foreach(var r in o.GetComponentsInChildren<Renderer>(true)){
    GameObjectUtility.SetStaticEditorFlags(r.gameObject,0);r.lightmapIndex=-1;r.lightProbeUsage=UnityEngine.Rendering.LightProbeUsage.BlendProbes;r.reflectionProbeUsage=UnityEngine.Rendering.ReflectionProbeUsage.BlendProbes;
    foreach(var m in r.sharedMaterials){if(m==null)continue;foreach(var p in m.GetTexturePropertyNames())Persist(m.GetTexture(p));Persist(m);}
   }foreach(var f in o.GetComponentsInChildren<MeshFilter>(true))Persist(f.sharedMesh);
   return o;
  }
  public static async void Build(){try{await Run();Debug.Log("VALORIA_REFERENCE_CLEAN_GAMEPLAY_BUILD_COMPLETE");EditorApplication.Exit(0);}catch(Exception e){Debug.LogError(e);EditorApplication.Exit(1);}}
  static async Task Run(){
   Directory.CreateDirectory(Ev);Directory.CreateDirectory(AssetsPath);AssetDatabase.Refresh();var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
   Check(UnityEngine.Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>()==null,"Host already present: use validation without rebuilding");
   var controls=UnityEngine.Object.FindFirstObjectByType<ValoriaReferenceCleanInteraction>();Check(controls!=null,"Original interaction missing");
   var keep=GameObject.Find("Bastión I");var left=GameObject.Find("Parcela 1");var right=GameObject.Find("Parcela 3");Check(keep&&left&&right,"Current targets missing");
   foreach(var pair in new[]{(keep,"bastion"),(left,"sawmill"),(right,"barracks")}){var h=pair.Item1.GetComponent<WorldHotspot>()??pair.Item1.AddComponent<WorldHotspot>();h.Id=pair.Item2;}
   var go=new GameObject("Valoria gameplay / existing rules and UI");var visual=go.AddComponent<ValoriaParcelPresentation>();visual.ProductionCamera=controls.playerCamera;
   var built=await Model("art-source/valoria/production/sawmill-family-v1/state1/SawmillState1V1.glb","Sawmill built",go.transform,left.transform.position,10.4f);
   var works=await Model("art-source/valoria/production/sawmill-family-v1/state0/SawmillState0V1.glb","Sawmill reconstruction",go.transform,left.transform.position,10.4f);
   var camp=await Model("art-source/valoria/production/camp-family-v1/CampFamilyV1.glb","Barracks built",go.transform,right.transform.position,10.6f);
   visual.Bindings=new[]{new ValoriaParcelPresentation.Binding{ParcelId="Parcela 1",BuildingId="sawmill",VariantBit=1,Built=built,Construction=works},new ValoriaParcelPresentation.Binding{ParcelId="Parcela 3",BuildingId="barracks",VariantBit=2,Built=camp}};
   visual.LeftBuilt=built;visual.RightBuilt=camp;visual.LeftConstruction=works;
   var presenter=go.AddComponent<SlicePresenter>();var host=go.AddComponent<ValoriaReferenceCleanGameplay>();host.Controls=controls;host.Parcels=visual;host.Presenter=presenter;
   var prior=SliceContentProfiles.ActiveRuntimeProfile;SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);try{visual.Apply(new PlayerState());}finally{SliceContentProfiles.SetRuntimeProfileOverride(prior);}
   AssetDatabase.SaveAssets();Check(EditorSceneManager.SaveScene(scene),"Scene save");
   File.WriteAllText(Path.Combine(Ev,"gameplay78-integration.json"),"{\"source_sha\":\""+Environment.GetEnvironmentVariable("GITHUB_SHA")+"\",\"scene\":\""+ScenePath+"\",\"rules\":\"existing OWNER_I_II + recovered web Bastion2 10s task\",\"bastion2_visual\":\"PENDING_AUTHORING\",\"retained_art\":\"native74; rejected pine77 variant not integrated\",\"bake_unchanged\":true}");
  }
  public static void PlayProbe(){
   Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
   EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
   SessionState.SetBool(ProbeFlag,true);EditorApplication.EnterPlaymode();
  }
  static void Entered(PlayModeStateChange change){if(change!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(ProbeFlag,false))return;SessionState.SetBool(ProbeFlag,false);EditorApplication.delayCall+=()=>{var h=UnityEngine.Object.FindFirstObjectByType<ValoriaReferenceCleanGameplay>();if(h==null){Debug.LogError("Missing gameplay host");EditorApplication.Exit(1);return;}h.StartCoroutine(Probe(h));};}
  [Serializable]class Result {public string source_sha,status,error,save_scope,bastion2_visual;public bool visual_swap,b2_routes,real_picking;public bool ui_sawmill,ui_gather,ui_fight,upgrade_started,resources_once,upgrade_persistence,upgrade_offline_completion,unlock,persistence,canonical_save_untouched,one_camera;public long final_revision;}
  static string Hash(string p){if(!File.Exists(p))return "ABSENT";using(var h=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p)));}
  static void Capture(ValoriaReferenceCleanGameplay h,string name){
   var c=h.PlayerCamera;var canvas=GameObject.Find("Eldoria HUD").GetComponent<Canvas>();var priorMode=canvas.renderMode;var priorCam=canvas.worldCamera;float priorDistance=canvas.planeDistance;
   var old=c.targetTexture;var active=RenderTexture.active;float aspect=c.aspect;var pos=c.transform.position;float zoom=c.orthographicSize;
   var rt=new RenderTexture(1536,1024,24){antiAliasing=4};var tex=new Texture2D(1536,1024,TextureFormat.RGB24,false);
   try{c.targetTexture=rt;c.aspect=1.5f;h.Controls.FitToTerrain();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=c;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1536,1024),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Ev,name),tex.EncodeToPNG());}
   finally{canvas.renderMode=priorMode;canvas.worldCamera=priorCam;canvas.planeDistance=priorDistance;c.targetTexture=old;c.aspect=aspect;c.transform.position=pos;c.orthographicSize=zoom;RenderTexture.active=active;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(tex);Canvas.ForceUpdateCanvases();}
  }
  static IEnumerator Probe(ValoriaReferenceCleanGameplay h){
   yield return null;yield return null;yield return new WaitForSecondsRealtime(.5f);
   var result=new Result{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),status="IN_PROGRESS",save_scope="Unique temporary FileStateStore; canonical files guarded",bastion2_visual="AUTHORED_EVOLUTION_RUNTIME_TEST_PENDING"};
   string owner=SliceBoot.SavePath,legacy=Path.Combine(UnityEngine.Application.persistentDataPath,"eldoria-unity-slice-v1.json");string ownerHash=Hash(owner),legacyHash=Hash(legacy);
   bool Step(Action a){if(result.status=="FAIL")return false;try{a();return true;}catch(Exception e){result.status="FAIL";result.error=e.ToString();Debug.LogError(e);return false;}}
   void Click(){var b=(Button)typeof(SlicePresenter).GetField("buildingAction",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(h.Presenter);Check(b!=null&&b.interactable,"Existing action disabled");b.onClick.Invoke();}
   void Advance(int sec){((ValoriaReferenceCleanGameplay.TestClock)h.Clock).Step(sec);h.Gateway.Advance();h.Presenter.RefreshState();}
   void Expedition(string id){h.Presenter.ShowExpedition(id);Click();Check(h.Gateway.Snapshot().March.Phase=="outbound","March did not depart");Advance(120);Check(h.Gateway.Snapshot().March.Phase=="idle","March did not return");}
   Step(()=>{Check(h.Ready,"Host not ready");Check(h.Gateway.Snapshot().BastionLevel==1,"Initial level");Check(h.BastionTwo!=null&&h.BastionOne!=null,"Reviewed visual bindings missing");Check(!h.BastionTwo.activeSelf&&h.BastionOne.activeSelf,"I visual gate");
    var target=GameObject.Find(h.TargetName("bastion")).GetComponent<Collider>();var point=h.PlayerCamera.WorldToScreenPoint(target.bounds.center);var pick=h.Pick(new Vector2(point.x,point.y));Check(pick!=null&&pick.Id=="bastion","Actual scene host picking");result.real_picking=true;Check(h.Pick(new Vector2(-100,-100))==null,"Offscreen pick");h.Presenter.Inspect("bastion");Capture(h,"gameplay78-bastion-i-ui.png");
    h.Presenter.Inspect("sawmill");Click();Check(h.Gateway.Snapshot().BuildingCompletesUtcTicks>0,"Sawmill not started");h.ReloadForProbe();Check(h.Parcels.LeftState==ParcelBuildingState.UNDER_CONSTRUCTION,"Sawmill work lost");Advance(SliceRules.SawmillBuildSeconds+1);Check(h.Parcels.LeftBuilt.activeSelf,"Sawmill missing");result.ui_sawmill=true;
    Expedition("forest-valoria");Expedition("forest-valoria");Expedition("quarry-valoria");result.ui_gather=true;Expedition("corrupt-scout");result.ui_fight=true;
    Check(h.Gateway.Snapshot().JourneyComplete,"Chapter one incomplete");h.Presenter.Inspect("bastion");var before=h.Gateway.Snapshot();Click();var started=h.Gateway.Snapshot();
    Check(started.BastionLevel==1&&started.BastionUpgradeCompletesUtcTicks>h.Clock.UtcTicks,"Upgrade must stay at I during work");result.upgrade_started=true;
    Check(started.Resources.Wood==before.Resources.Wood-450&&started.Resources.Stone==before.Resources.Stone-300,"Wrong expense");
    string id=started.CompletedCommandIds.Last();var again=h.Gateway.Execute(new GameCommand(id,started.PlayerId,"AdvanceBastion","bastion",started.Revision));Check(again.Ok&&h.Gateway.Snapshot().Resources.Wood==started.Resources.Wood,"Replay charged twice");result.resources_once=true;
    Advance(4);h.ReloadForProbe();Check(h.Gateway.Snapshot().BastionLevel==1&&h.Gateway.Snapshot().BastionUpgradeCompletesUtcTicks==started.BastionUpgradeCompletesUtcTicks,"Mid-upgrade reload lost task");result.upgrade_persistence=true;
    h.Presenter.Inspect("bastion");Capture(h,"gameplay78-upgrade-ui.png");
    ((ValoriaReferenceCleanGameplay.TestClock)h.Clock).Step(7);h.ReloadForProbe();Check(h.Gateway.Snapshot().BastionLevel==2,"Offline completion failed");result.upgrade_offline_completion=true;Check(h.BastionTwo.activeSelf&&!h.BastionOne.activeSelf,"II geometry swap");result.visual_swap=true;result.bastion2_visual="AUTHORED_I_II_EXCLUSIVE_MODEL_SWAP_VERIFIED";Physics.SyncTransforms();
    int route=0,head=0,stairs=0;for(float z=-70;z<=25;z+=1){if(Physics.Raycast(new Vector3(ValoriaReferenceCleanArtBuilder.RoadX(z),1.8f,z),Vector3.down,out var hit,6,~(1<<8),QueryTriggerInteraction.Ignore)&&hit.point.y>-.1f&&hit.point.y<1.2f){route++;if(Physics.OverlapCapsule(hit.point+Vector3.up*.45f,hit.point+Vector3.up*1.55f,.28f,~(1<<8),QueryTriggerInteraction.Ignore).Length==0)head++;}}
    for(int i=0;i<6;i++)if(Physics.Raycast(new Vector3(0,20,25.75f+i*.46f),Vector3.down,out var hit,40,~(1<<8),QueryTriggerInteraction.Ignore)&&Mathf.Abs(hit.point.y-(.57f+i*.19f))<.12f)stairs++;
    Check(route==96&&head==96&&stairs==6,"II collision routes/headroom/stairs");result.b2_routes=true;Check(h.Parcels.RightState==ParcelBuildingState.AVAILABLE&&!h.Parcels.RightBuilt.activeSelf,"Barracks unlock gate incorrect");result.unlock=true;
    var final=JsonUtility.ToJson(h.Gateway.Snapshot());h.ReloadForProbe();Check(final==JsonUtility.ToJson(h.Gateway.Snapshot()),"Completion/rewards replayed on reload");result.persistence=true;h.Presenter.Inspect("bastion");Capture(h,"gameplay78-bastion-ii-ui.png");result.final_revision=h.Gateway.Snapshot().Revision;
    result.canonical_save_untouched=Hash(owner)==ownerHash&&Hash(legacy)==legacyHash;Check(result.canonical_save_untouched,"Player save changed");result.one_camera=UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Count(c=>c.enabled)==1;Check(result.one_camera,"Multiple cameras");result.status="EXISTING_UI_RULES_SAVE_I_II_PASS_AUTHORED_VISUAL_SWAP_VERIFIED";
   });
   File.WriteAllText(Path.Combine(Ev,"gameplay78-playmode.json"),JsonUtility.ToJson(result,true));
   Debug.Log("VALORIA_REFERENCE_CLEAN_GAMEPLAY_PROBE_COMPLETE "+result.status);
   yield return null;EditorApplication.Exit(result.status=="FAIL"?1:0);
  }
 }
}
