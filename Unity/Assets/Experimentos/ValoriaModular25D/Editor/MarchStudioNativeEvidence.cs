#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Eldoria.Experimentos.ValoriaModular25D.Editor
{
    // The exact production prefab and live LocalGateway, with disposable
    // deterministic state only. No city rendering, no owner save, no mock UI.
    [InitializeOnLoad]
    public static class MarchStudioNativeEvidence
    {
        const string Flag="Eldoria.MarchStudio.Native";
        const string City="Assets/Experimentos/ValoriaModular25D/Scenes/BastionI_Modular25D.unity";
        const string Region="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        static string Out=>Path.GetFullPath(Environment.GetEnvironmentVariable("MARCH_STUDIO_NATIVE_OUT")??"Artifacts/MarchStudio");
        static string Source=>Environment.GetEnvironmentVariable("MARCH_STUDIO_SOURCE_SHA")??"LOCAL";
        static readonly List<Frame> Frames=new List<Frame>();
        static readonly List<Control> Controls=new List<Control>();
        static readonly List<string> Checks=new List<string>();
        static readonly List<string> Defects=new List<string>();
        static int excludedRenderers,excludedCanvases;
        static bool uiOnlyNativeCamera;
        [Serializable] sealed class Frame {public string file,sha256,fixture,variant;public int width,height;public bool actualCamera,playMode;public int t1,t2,selectedQuantity,selectedTier;public string selectedHero;public long revision;public string[] metrics;}
        [Serializable] sealed class Control {public string frame,name;public float width,height;public bool interactable,hit;public string topHit;}
        [Serializable] sealed class Report {public string sourceSha,harnessSha,engine,engineVersion,prefabPath,assetSha;public bool actualPrefab,actualGateway,artApproved,physicalIphone,fullBrowserJourney,uiOnlyNativeCamera;public int excludedRenderers,excludedCanvases;public Frame[] frames;public Control[] controls;public string[] checks,defects;}
        sealed class Clock:IClock {public long UtcTicks{get;set;}=new DateTime(2026,11,26,12,0,0,DateTimeKind.Utc).Ticks;}
        sealed class Memory:IStateStore {public PlayerState Stored;public PlayerState Load()=>Stored;public void Save(PlayerState s){Stored=s;}}
        static IEnumerator routine;
        static double lastHeartbeat;
        static MarchStudioNativeEvidence()
        {
            EditorApplication.update-=Tick;
            EditorApplication.update+=Tick;
        }
        static void Need(bool condition,string message)
        {
            if(!condition)throw new InvalidOperationException(message);
            Checks.Add(message);Debug.Log("MARCH_STUDIO_CHECK "+message);
        }
        static void Defect(bool ok,string message){if(!ok){Defects.Add(message);Debug.LogWarning("MARCH_STUDIO_DEFECT "+message);}}
        static string Digest(byte[] b){using(var s=SHA256.Create())return BitConverter.ToString(s.ComputeHash(b)).Replace("-","").ToLowerInvariant();}
        public static void Capture()
        {
            try
            {
                Directory.CreateDirectory(Out);
                var scene=EditorSceneManager.OpenScene(City,OpenSceneMode.Single);
                var world=scene.GetRootGameObjects().Select(r=>r.GetComponentInChildren<Modular25DWorld>(true)).FirstOrDefault(x=>x);
                Need(world&&world.ManagementHudPrefab,"Canonical city provides the real ManagementHudPrefab");
                var path=AssetDatabase.GetAssetPath(world.ManagementHudPrefab);
                Need(!string.IsNullOrEmpty(path),"Exact prefab resolved: "+path);
                SessionState.SetString(Flag+"Prefab",path);
                // Only this scratch project's build settings. Exactly the
                // combined player's existing Corona activation condition.
                EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(City,true),new EditorBuildSettingsScene(Region,true)};
                // A saved scratch scene avoids the prior unsaved Temp/Backup scene.
                // Arm first; enter only on a later Editor update after project startup.
                var scratch=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
                const string probe="Assets/Experimentos/ValoriaModular25D/Scenes/MarchStudioDisposableProbe.unity";
                if(File.Exists(probe))throw new InvalidOperationException("Refuse to overwrite a probe scene");
                Need(EditorSceneManager.SaveScene(scratch,probe),"Saved isolated disposable probe scene");
                SessionState.SetBool(Flag,true);
                SessionState.SetString(Flag+"Phase","armed");
                SessionState.SetFloat(Flag+"Deadline",(float)(EditorApplication.timeSinceStartup+420));
                Debug.Log("MARCH_STUDIO_ARMED saved scene; editor update will enter PlayMode");
            }
            catch(Exception ex){Fail(ex);}
        }
        static void Tick()
        {
            if(!SessionState.GetBool(Flag,false))return;
            try
            {
                var now=EditorApplication.timeSinceStartup;
                var phase=SessionState.GetString(Flag+"Phase","");
                if(now>=SessionState.GetFloat(Flag+"Deadline",0f))
                    throw new TimeoutException("Native March editor-update watchdog: phase="+phase+
                        ", playing="+EditorApplication.isPlaying+", compiling="+EditorApplication.isCompiling);
                if(now-lastHeartbeat>=20)
                {
                    lastHeartbeat=now;
                    Debug.Log("MARCH_STUDIO_HEARTBEAT phase="+phase+" playing="+EditorApplication.isPlaying);
                }
                if(!EditorApplication.isPlaying)
                {
                    if(phase=="armed"&&!EditorApplication.isPlayingOrWillChangePlaymode&&
                        !EditorApplication.isCompiling&&!EditorApplication.isUpdating)
                    {
                        SessionState.SetString(Flag+"Phase","entering");
                        Debug.Log("MARCH_STUDIO_ENTER_REQUEST");
                        EditorApplication.EnterPlaymode();
                    }
                    return;
                }
                if(routine==null)
                {
                    SessionState.SetString(Flag+"Phase","rendering");
                    Debug.Log("MARCH_STUDIO_PLAYMODE_ENTERED via Editor update");
                    Need(RoyalGoldV2Hud.UseCoronaArt,"Real combined-player Corona condition is active");
                    SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
                    Need(SliceContentProfiles.ActiveRuntimeProfile==SliceContentProfiles.OwnerIiiId,"Actual combined-player content profile, disposable fixture only");
                    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(SessionState.GetString(Flag+"Prefab",""));
                    Need(prefab&&prefab.GetComponent<RoyalHUD>(),"Actual source-pinned room prefab exists");
                    // Do not instantiate an unused duplicate HUD: its
                    // runtime children/canvases can pollute the capture.
                    routine=Exercise(prefab);
                }
                // This existing enumerator only yields null; one real frame per Editor update.
                if(routine.MoveNext())return;
                SessionState.SetBool(Flag,false);
                EditorApplication.Exit(0);
            }
            catch(Exception ex){Fail(ex);}
        }
        static PlayerState Fixture(bool mixed)
        {
            var s=new PlayerState{BastionLevel=5,BarracksLevel=mixed?4:2,
                Resources=new ResourceWallet{Wood=9000,Stone=9000,Food=9000},
                Available=new ArmyRoster{ArcherT1=56,ArcherT2=mixed?18:0}};
            s.ChapterThree.LyraRecruited=true;
            s.ChapterProgress.SeenNarrative.Add("march-preparation");
            return s;
        }
        static Button FindButton(RoyalGoldV3ManagementScreens manager,string name)
        {
            return manager.VisibleRoot.GetComponentsInChildren<Button>(true).FirstOrDefault(x=>x.name==name);
        }
        static IEnumerator Exercise(GameObject prefab)
        {
            var cameraGo=new GameObject("March native actual UI camera");
            var cam=cameraGo.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=5;
            cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;
            cam.backgroundColor=new Color(.02f,.035f,.045f,1);cam.enabled=false;
            cam.cullingMask=1<<5;cam.nearClipPlane=.05f;cam.farClipPlane=3f;
            if(EventSystem.current==null)new GameObject("Native actual event system",typeof(EventSystem));
            var originalAsset=Path.Combine(UnityEngine.Application.dataPath,"Experimentos/ValoriaModular25D/Resources/ValoriaModular25D/UI/CoronaBrecha/march_command_table.png");
            Need(File.Exists(originalAsset)&&Digest(File.ReadAllBytes(originalAsset))=="a42b20c2ca7bd57a8b09d95a7442ccc81bb33c9f23f2e1869112b10e4071cab7","Exact original studio artist material imported");
            foreach(bool mixed in new[]{false,true})
            {
                foreach(bool pilot in new[]{false,true})
                {
                    RoyalGoldV3ManagementScreens.StudioMarchPilotForEvidence=pilot;
                    var room=Object.Instantiate(prefab);
                    var hud=room.GetComponent<RoyalHUD>();
                    var canvas=hud.GetComponent<Canvas>();
                    var scale=hud.GetComponent<RoyalScaler>();if(scale)scale.enabled=false;
                    foreach(var a in hud.GetComponentsInChildren<RoyalSafeArea>(true))a.enabled=false;
                    var clock=new Clock();var memory=new Memory{Stored=Fixture(mixed)};
                    var gateway=new LocalGateway(clock,memory);
                    hud.Bind(new ExistingSliceBinding(gateway,clock,true));
                    hud.enabled=false;
                    if(hud.Modal.gameObject.activeSelf)hud.Modal.Hide();
                    var safe=hud.GetComponentInChildren<RoyalHudLayout>(true).transform;
                    foreach(Transform child in safe)child.gameObject.SetActive(false);
                    var manager=hud.GetComponent<RoyalGoldV3ManagementScreens>()??room.AddComponent<RoyalGoldV3ManagementScreens>();
                    manager.Initialize(hud,gateway,()=>clock.UtcTicks,()=>{});
                    // A render target before Open matches the real responsive
                    // canvas geometry rather than the desktop GameView.
                    var target=new RenderTexture(390,844,24,RenderTextureFormat.ARGB32);target.Create();
                    cam.targetTexture=target;cam.aspect=390f/844f;
                    canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=cam;canvas.planeDistance=1;
                    canvas.scaleFactor=390f/600f;
                    Canvas.ForceUpdateCanvases();cam.Render();Canvas.ForceUpdateCanvases();
                    manager.Open(RoyalGoldV3ManagementScreens.Destination.March);
                    yield return null;Canvas.ForceUpdateCanvases();manager.SynchronizeLayout();
                    Need(manager.IsOpen,"Actual March room opens via real local gateway "+mixed+"/"+pilot);
                    // Mixed fixture exercises the real support selector and
                    // tier selector; use same clicks for BEFORE and AFTER.
                    if(mixed)
                    {
                        var support=FindButton(manager,"Hero slot 1");
                        Need(support&&support.interactable,"Support selector is real and available");
                        support.onClick.Invoke();
                        var tier=FindButton(manager,"Select archer tier");
                        Need(tier&&tier.gameObject.activeInHierarchy&&tier.interactable,"Real T2 selector is available");
                        tier.onClick.Invoke();
                        var slider=manager.VisibleRoot.GetComponentInChildren<Slider>(true);
                        slider.value=12;manager.SynchronizeLayout();
                    }
                    var fixture=mixed?"mixed":"t1";
                    var mode=pilot?"after":"before";
                    CaptureFrame(manager,gateway,cam,target,fixture,mode,390,844,pilot);
                    var locked=FindButton(manager,"Hero slot 2");
                    Need(locked&&!locked.interactable,"Third hero remains locked "+fixture+"/"+mode);
                    if(pilot)
                    {
                        var portraitViews=manager.VisibleRoot.GetComponentsInChildren<RectTransform>(false).Where(r=>r.name=="Hero portrait viewport").ToArray();
                        Defect(portraitViews.Length==3&&portraitViews.All(r=>r.GetComponentsInChildren<Image>(false).Any(i=>i.enabled&&i.sprite&&i.name.StartsWith("Approved source /",StringComparison.Ordinal))),"An original hero portrait/silhouette is hidden "+fixture);
                        var visible=manager.VisibleRoot.GetComponentsInChildren<Text>(false);
                        var values=visible.Where(x=>x.name.StartsWith("March / live metric ")).ToArray();
                        var labels=visible.Where(x=>x.name.StartsWith("Studio / real metric label ")).ToArray();
                        Defect(values.Length==6&&labels.Length==6,"Six exact labels and values visible "+fixture);
                        foreach(var t in values.Concat(labels))
                        {
                            Defect(!string.IsNullOrWhiteSpace(t.text),"Missing live metric "+t.name);
                            Defect(t.preferredWidth<=t.rectTransform.rect.width+2f&&t.preferredHeight<=t.rectTransform.rect.height+2f,"Clipped live metric "+fixture+"/"+t.name);
                        }
                        // Verify native actions on the same controls, then
                        // canonical persisted composition; no debug command.
                        var slider=manager.VisibleRoot.GetComponentInChildren<Slider>(true);
                        slider.value=7;manager.SynchronizeLayout();
                        var count=FindButton(manager,"Increase quantity");
                        var initialCount=manager.SelectedQuantity;
                        bool increased=count&&count.interactable&&ClickNative(count,cam);
                        Defect(increased&&manager.SelectedQuantity==initialCount+1,"Increment down-up-click did not change quantity "+fixture);
                        var reduce=FindButton(manager,"Reduce quantity");
                        var increasedCount=manager.SelectedQuantity;
                        bool reduced=reduce&&reduce.interactable&&ClickNative(reduce,cam);
                        Defect(reduced&&manager.SelectedQuantity==increasedCount-1,"Decrement down-up-click did not change quantity "+fixture);
                        var modeButton=FindButton(manager,"Edit march by percent");
                        if(modeButton&&modeButton.interactable)
                        {
                            var modeLabel=modeButton.GetComponentInChildren<RoyalText>(true);
                            var initialMode=modeLabel?modeLabel.text:"";
                            bool changed=ClickNative(modeButton,cam);
                            Defect(changed&&modeLabel&&modeLabel.text!=initialMode,"Percentage mode down-up-click not proven "+fixture);
                            bool restored=ClickNative(modeButton,cam);
                            Defect(restored&&modeLabel&&modeLabel.text==initialMode,"Quantity mode restoration not proven "+fixture);
                        }
                        else Defect(false,"Quantity mode control unavailable "+fixture);
                        manager.SynchronizeLayout();
                        var expected=manager.SelectedQuantity;
                        var action=manager.VisibleAction;
                        Defect(action&&action.interactable,"Real confirm command unavailable "+fixture);
                        bool confirmed=action&&action.interactable&&ClickNative(action,cam);
                        var saved=gateway.Snapshot();
                        Defect(confirmed&&saved.MarchConfigured&&(mixed?saved.PreparedTroops.ArcherT2:saved.PreparedTroops.ArcherT1)==expected,
                            "Confirm down-up-click did not persist chosen quantity "+fixture+" expected="+expected+" actual="+(mixed?saved.PreparedTroops.ArcherT2:saved.PreparedTroops.ArcherT1));
                        // Do not bypass event handlers. Collect the other
                        // authentic frames even when the interaction fails. 
                    }
                    cam.targetTexture=null;target.Release();Object.Destroy(target);
                    room.SetActive(false);Object.Destroy(room);
                    yield return null;
                }
            }
            // A/B landscape, with identical independent fixture. Verify the
            // portrait-only art is absent and the original combined text path remains.
            foreach(bool pilot in new[]{false,true})
            {
                RoyalGoldV3ManagementScreens.StudioMarchPilotForEvidence=pilot;
                var room=Object.Instantiate(prefab);var hud=room.GetComponent<RoyalHUD>();var canvas=hud.GetComponent<Canvas>();
                var scale=hud.GetComponent<RoyalScaler>();if(scale)scale.enabled=false;
                foreach(var a in hud.GetComponentsInChildren<RoyalSafeArea>(true))a.enabled=false;
                var clock=new Clock();var memory=new Memory{Stored=Fixture(false)};var gateway=new LocalGateway(clock,memory);
                hud.Bind(new ExistingSliceBinding(gateway,clock,true));hud.enabled=false;
                if(hud.Modal.gameObject.activeSelf)hud.Modal.Hide();
                foreach(Transform child in hud.GetComponentInChildren<RoyalHudLayout>(true).transform)child.gameObject.SetActive(false);
                var manager=hud.GetComponent<RoyalGoldV3ManagementScreens>()??room.AddComponent<RoyalGoldV3ManagementScreens>();
                manager.Initialize(hud,gateway,()=>clock.UtcTicks,()=>{});
                var target=new RenderTexture(844,390,24,RenderTextureFormat.ARGB32);target.Create();cam.targetTexture=target;cam.aspect=844f/390f;
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=cam;canvas.planeDistance=1;canvas.scaleFactor=390f/600f;
                Canvas.ForceUpdateCanvases();cam.Render();Canvas.ForceUpdateCanvases();
                manager.Open(RoyalGoldV3ManagementScreens.Destination.March);yield return null;manager.SynchronizeLayout();
                Need(manager.IsOpen,"Landscape existing March room still opens");
                CaptureFrame(manager,gateway,cam,target,"landscape",pilot?"after":"before",844,390,false);
                Need(!manager.VisibleRoot.GetComponentsInChildren<Text>(false).Any(t=>t.name.StartsWith("Studio / real metric label ")),"Landscape retains original metric path");
                room.SetActive(false);Object.Destroy(room);cam.targetTexture=null;target.Release();Object.Destroy(target);yield return null;
            }
            var report=new Report{sourceSha=Source,harnessSha=Environment.GetEnvironmentVariable("MARCH_STUDIO_HARNESS_SHA")??Source,engine="Unity",engineVersion=UnityEngine.Application.unityVersion,prefabPath=SessionState.GetString(Flag+"Prefab",""),assetSha=Digest(File.ReadAllBytes(originalAsset)),
                actualPrefab=true,actualGateway=true,artApproved=false,physicalIphone=false,fullBrowserJourney=false,
                uiOnlyNativeCamera=uiOnlyNativeCamera,excludedRenderers=excludedRenderers,excludedCanvases=excludedCanvases,
                frames=Frames.ToArray(),controls=Controls.ToArray(),checks=Checks.ToArray(),defects=Defects.ToArray()};
            File.WriteAllText(Path.Combine(Out,"evidence.json"),JsonUtility.ToJson(report,true));
            Debug.Log("MARCH_STUDIO_CAPTURE_COMPLETE frames="+Frames.Count+" defects="+Defects.Count);
        }
        // Scratch scene only. The actual production HUD normally renders as
        // overlay; camera mode must not render bootstrap 3D meshes or other
        // canvases in front of it. Do not change a game asset or production scene.
        static void IsolateNativeRoom(RoyalGoldV3ManagementScreens manager,Camera cam)
        {
            var root=manager.transform;
            foreach(var child in root.GetComponentsInChildren<Transform>(true))child.gameObject.layer=5;
            foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
                if(renderer.enabled){renderer.enabled=false;excludedRenderers++;}
            foreach(var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
                if(canvas.enabled&&canvas.transform!=root&&!canvas.transform.IsChildOf(root))
                {canvas.enabled=false;excludedCanvases++;}
            cam.cullingMask=1<<5;
            uiOnlyNativeCamera=Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude,FindObjectsSortMode.None)
                .All(r=>!r.enabled);
            Need(uiOnlyNativeCamera,"Native fixture excludes non-UI scene renderers");
        }
        static bool PointerTarget(Button button,Camera cam,out string first)
        {
            var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);
            var pos=(RectTransformUtility.WorldToScreenPoint(cam,corners[0])+RectTransformUtility.WorldToScreenPoint(cam,corners[2]))*.5f;
            var ev=new PointerEventData(EventSystem.current){position=pos,button=PointerEventData.InputButton.Left};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(ev,hits);
            first=hits.Count>0?hits[0].gameObject.name:"none";
            return hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)==button.gameObject;
        }
        static bool ClickNative(Button button,Camera cam)
        {
            if(!PointerTarget(button,cam,out string first)){Debug.LogWarning("MARCH_STUDIO_POINTER_MISS "+button.name+" -> "+first);return false;}
            var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);
            var pos=(RectTransformUtility.WorldToScreenPoint(cam,corners[0])+RectTransformUtility.WorldToScreenPoint(cam,corners[2]))*.5f;
            // RoyalButton accepts a click only after matching Down -> Up;
            // invoking OnPointerClick alone cannot exercise its drag guard.
            var ev=new PointerEventData(EventSystem.current){position=pos,pressPosition=pos,pointerId=-1,button=PointerEventData.InputButton.Left,eligibleForClick=true};
            ExecuteEvents.Execute(button.gameObject,ev,ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(button.gameObject,ev,ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(button.gameObject,ev,ExecuteEvents.pointerClickHandler);
            return true;
        }
        static void CaptureFrame(RoyalGoldV3ManagementScreens manager,LocalGateway gateway,Camera cam,RenderTexture rt,string fixture,string variant,int width,int height,bool checkTargets)
        {
            IsolateNativeRoom(manager,cam);
            Canvas.ForceUpdateCanvases();cam.Render();Canvas.ForceUpdateCanvases();
            manager.SynchronizeLayout();Canvas.ForceUpdateCanvases();cam.Render();cam.Render();
            var tex=new Texture2D(width,height,TextureFormat.RGB24,false);var prev=RenderTexture.active;
            byte[] bytes;
            try{RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();bytes=tex.EncodeToPNG();}
            finally{RenderTexture.active=prev;Object.Destroy(tex);}
            string file=fixture+"-"+variant+".png";File.WriteAllBytes(Path.Combine(Out,file),bytes);
            Need(bytes.Length>12000,"Actual native camera PNG "+file);
            var s=gateway.Snapshot();
            var allText=manager.VisibleRoot.GetComponentsInChildren<Text>(true);
            var metricText=new string[6];
            for(int i=0;i<6;i++)
            {
                var value=allText.First(t=>t.name=="March / live metric "+i);
                var caption=allText.FirstOrDefault(t=>t.name=="Studio / real metric label "+i&&t.gameObject.activeInHierarchy);
                metricText[i]=caption?caption.text+"\n"+value.text:value.text;
            }
            Frames.Add(new Frame{file=file,sha256=Digest(bytes),fixture=fixture,variant=variant,width=width,height=height,actualCamera=true,playMode=EditorApplication.isPlaying,t1=s.Available.ArcherT1,t2=s.Available.ArcherT2,selectedHero=s.PreparedHeroId,revision=s.Revision,selectedQuantity=manager.SelectedQuantity,selectedTier=manager.SelectedTier,metrics=metricText});
            foreach(var button in manager.VisibleRoot.GetComponentsInChildren<Button>(false))
            {
                if(!button.gameObject.activeInHierarchy)continue;
                var rect=(RectTransform)button.transform;var corners=new Vector3[4];rect.GetWorldCorners(corners);
                var a=RectTransformUtility.WorldToScreenPoint(cam,corners[0]);var b=RectTransformUtility.WorldToScreenPoint(cam,corners[2]);var center=(a+b)*.5f;
                bool hit=PointerTarget(button,cam,out string topHit);
                Controls.Add(new Control{frame=file,name=button.name,width=Mathf.Abs(b.x-a.x),height=Mathf.Abs(b.y-a.y),interactable=button.interactable,hit=hit,topHit=topHit});
                if(checkTargets&&button.interactable)
                {
                    Defect(Mathf.Abs(b.x-a.x)>=43.5f&&Mathf.Abs(b.y-a.y)>=43.5f,"Touch target below 44px "+file+"/"+button.name);
                    Defect(hit,"Native pointer raycast not proven "+file+"/"+button.name);
                }
            }
            var slider=manager.VisibleRoot.GetComponentInChildren<Slider>(false);
            if(slider&&checkTargets){var corners=new Vector3[4];slider.GetComponent<RectTransform>().GetWorldCorners(corners);var a=RectTransformUtility.WorldToScreenPoint(cam,corners[0]);var b=RectTransformUtility.WorldToScreenPoint(cam,corners[2]);Defect(Mathf.Abs(b.y-a.y)>=43.5f,"Slider native touch height below 44px "+file);}
        }
        static void Fail(Exception ex)
        {
            SessionState.SetBool(Flag,false);
            Directory.CreateDirectory(Out);File.WriteAllText(Path.Combine(Out,"failure.txt"),ex.ToString());
            Debug.LogException(ex);EditorApplication.Exit(1);
        }
    }
}
#endif
