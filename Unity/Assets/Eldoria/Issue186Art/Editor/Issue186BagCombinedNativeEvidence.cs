#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.ReferenceClean;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Eldoria.Issue186Art.Editor
{
    // Uses an isolated, GUID-named gameplay-probe save in the OS temp folder.
    // All consuming UI taps go through the real first EventSystem hit.
    // Optional Vigilia is resolved at runtime, never a production dependency.
    [InitializeOnLoad]
    public static class Issue186BagCombinedNativeEvidence
    {
        const string Flag="Issue186.CombinedSavedBag.Native";
        const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        const string Folder="Issue186BagCombinedNativeEvidence";
        static IEnumerator routine;
        static double deadline;
        static readonly List<Shot> shots=new List<Shot>();
        static int clicks;
        [Serializable] sealed class Shot
        {
            public string file,sha256,phase;
            public int width,height,bytes;
        }
        [Serializable] sealed class Report
        {
            public string status,resourceSource,uiSource,error,limitation,nativeRun,nativeAttempt;
            public bool productionVigiliaInstalled, firstUseCoachPresent, firstSawmillHandOnRealHit;
            public bool firstSawmillLabelSelected, firstBuildingActionHandOnRealHit;
            public bool firstNicknameInputOnRealHit, firstNicknameConfirmOnRealHit,
                firstStoryContinueOnRealHit;
            public bool temporaryProbeOnly,actualBagOnVigilia,seenDoesNotUse,
                skipDoesNotComplete,useCreditsOnce,jsonReloadVerified,
                mailReadDoesNotClaim,mailClaimDoesNotCredit,artApproved,
                physicalIphoneVerified,fullMailboxUiFlowVerified;
            public int realFirstHitClicks,actualIntegratedCityFrames;
            public Shot[] images;
        }
        static Report report;
        static string Output=>Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath,"../"+Folder));
        static Issue186BagCombinedNativeEvidence()
        {
            EditorApplication.update-=Tick;EditorApplication.update+=Tick;
        }
        static void Need(bool ok,string message)
        {if(!ok)throw new InvalidOperationException("ISSUE186_COMBINED_BAG "+message);}
        static void VerifyPaintedReceiptBinding(Issue186BagOverlay bag)
        {
            const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
            var type=bag.GetType();
            var pageField=type.GetField("page",flags);
            var sizeField=type.GetField("perPage",flags);
            var action=type.GetMethod("PresentedPacket",flags);
            Need(action!=null,"Bag actions have no stable painted receipt binding");
            var painted=action.Invoke(bag,new object[]{0}) as ResourceUiPacketRow;
            Need(painted!=null,"First visible real saved receipt missing");
            int oldPage=(int)pageField.GetValue(bag),oldSize=(int)sizeField.GetValue(bag);
            try
            {
                // Reproduce the intermediate offset change made by Layout on
                // rotation, before the next paint. No gameplay command is sent.
                pageField.SetValue(bag,1);sizeField.SetValue(bag,1);
                var bound=action.Invoke(bag,new object[]{0}) as ResourceUiPacketRow;
                Need(bound!=null&&bound.ReceiptId==painted.ReceiptId,
                    "Resize redirected a painted cell to another saved reward");
            }
            finally {pageField.SetValue(bag,oldPage);sizeField.SetValue(bag,oldSize);}
        }
        public static void Capture()
        {
            Need(!EditorApplication.isPlaying,"Start from a fresh isolated Editor");
            Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
            Environment.SetEnvironmentVariable("ELDORIA_NEW_UI_ONLY","1");
            EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
            SessionState.SetBool(Flag,true);routine=null;deadline=0;
            EditorApplication.EnterPlaymode();
        }
        static void Tick()
        {
            if(!SessionState.GetBool(Flag,false)||!EditorApplication.isPlaying)return;
            try
            {
                if(deadline==0)deadline=EditorApplication.timeSinceStartup+360;
                Need(EditorApplication.timeSinceStartup<deadline,"Native probe timed out");
                if(routine==null)
                {
                    shots.Clear();clicks=0;
                    report=new Report{status="RUNNING",
                        resourceSource=Environment.GetEnvironmentVariable("GITHUB_SHA")??"",
                        uiSource=Environment.GetEnvironmentVariable("ISSUE186_VIGILIA_SOURCE")??"",
                        nativeRun=Environment.GetEnvironmentVariable("GITHUB_RUN_ID")??"",
                        nativeAttempt=Environment.GetEnvironmentVariable("GITHUB_RUN_ATTEMPT")??"",
                        limitation="Actual saved bag and complete letter-row read/claim/back UI on combined Vigilia probe; no art or physical device approval"};
                    // A failed prior run can leave PNGs that do not belong
                    // to this source. Preserve its complete output separately;
                    // never let stale pictures enter the current artifact.
                    if(Directory.Exists(Output))
                    {
                        Need((new DirectoryInfo(Output).Attributes&FileAttributes.ReparsePoint)==0,
                            "Refuse to move a linked native evidence directory");
                        Directory.Move(Output,Output+".prior-"+Guid.NewGuid().ToString("N"));
                    }
                    Directory.CreateDirectory(Output);routine=Run();
                }
                if(routine.MoveNext())return;
                report.status="REAL_VIGILIA_SAVED_BAG_PASS_ART_REVIEW_PENDING";
                Write();SessionState.SetBool(Flag,false);
                EditorApplication.Exit(0);
            }
            catch(Exception e)
            {
                SessionState.SetBool(Flag,false);
                try{(routine as IDisposable)?.Dispose();}catch(Exception cleanup){Debug.LogException(cleanup);}
                if(report==null)report=new Report();
                report.status="REAL_VIGILIA_SAVED_BAG_FAIL";report.error=e.ToString();
                Write();Debug.LogException(e);EditorApplication.Exit(1);
            }
        }
        static void Write()
        {
            report.images=shots.ToArray();report.realFirstHitClicks=clicks;
            Directory.CreateDirectory(Output);
            File.WriteAllText(Path.Combine(Output,"report.json"),JsonUtility.ToJson(report,true));
        }
        static Type VigiliaType()=>AppDomain.CurrentDomain.GetAssemblies()
            .Select(a=>a.GetType("Eldoria.BreachWatch.BreachWatchHud",false))
            .FirstOrDefault(t=>t!=null);
        static void RenderUi(Component ui,int width,int height)
        {
            ui.GetType().GetMethod("Reflow").Invoke(ui,new object[]{width,height});
            ui.GetType().GetMethod("RenderNow").Invoke(ui,null);
            Canvas.ForceUpdateCanvases();
        }
        static void Draw(Camera camera,RenderTexture target)
        {
            Canvas.ForceUpdateCanvases();
            if(GraphicsSettings.currentRenderPipeline!=null)
            {
                var request=new RenderPipeline.StandardRequest{destination=target};
                Need(RenderPipeline.SupportsRenderRequest(camera,request),"Actual camera render unsupported");
                RenderPipeline.SubmitRenderRequest(camera,request);
                RenderPipeline.SubmitRenderRequest(camera,request);
            }
            else {camera.Render();camera.Render();}
            Canvas.ForceUpdateCanvases();
        }
        static Button ButtonNamed(Component parent,string name)=>parent
            .GetComponentsInChildren<Button>(true)
            .Single(b=>b&&b.name==name&&b.gameObject.activeInHierarchy);
        static void Click(Button button,Camera camera,RenderTexture target)
        {
            Need(button&&button.isActiveAndEnabled&&button.IsInteractable(),"Actual button unavailable");
            Draw(camera,target);
            var system=EventSystem.current;
            Need(system&&system.isActiveAndEnabled,"Actual scene EventSystem unavailable");
            var r=(RectTransform)button.transform;
            var data=new PointerEventData(system){position=
                RectTransformUtility.WorldToScreenPoint(camera,r.TransformPoint(r.rect.center))};
            var hits=new List<RaycastResult>();system.RaycastAll(data,hits);
            Need(hits.Count>0&&ExecuteEvents.GetEventHandler<IPointerClickHandler>(
                hits[0].gameObject)==button.gameObject,"Actual target is not frontmost: "+button.name+
                "; first="+(hits.Count>0?hits[0].gameObject.name:"none")+
                "; point="+data.position+"; rect="+r.rect+
                "; depth="+(button.targetGraphic?button.targetGraphic.depth:-999)+
                "; graphic="+(button.targetGraphic?button.targetGraphic.name:"none"));
            // Match EventSystem routing to the first hit\'s click-handler ancestor.
            var handler=ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
            Need(ExecuteEvents.Execute<IPointerClickHandler>(
                handler,data,ExecuteEvents.pointerClickHandler),"First hit has no click handler");clicks++;
            Debug.Log("ISSUE186_ACTUAL_FIRST_HIT_CLICK target="+button.name+
                "; hit="+hits[0].gameObject.name+"; handler="+handler.name);
        }
        static void TouchRealWorldGuidance(RoyalButton actual,
            Camera camera,RenderTexture target)
        {
            Need(actual&&actual.isActiveAndEnabled&&actual.IsInteractable() &&
                actual.gameObject.activeInHierarchy,
                "The real city guidance RoyalButton is not available");
            Draw(camera,target);
            var system=EventSystem.current;
            Need(system&&system.isActiveAndEnabled,"Actual guidance EventSystem unavailable");
            var rect=(RectTransform)actual.transform;
            var pos=RectTransformUtility.WorldToScreenPoint(camera,
                rect.TransformPoint(rect.rect.center));
            var data=new PointerEventData(system){
                position=pos,pressPosition=pos,
                button=PointerEventData.InputButton.Left,
                pointerId=-1,clickCount=1,eligibleForClick=true};
            var hits=new List<RaycastResult>();
            system.RaycastAll(data,hits);
            var handler=hits.Count>0?
                ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject):null;
            Need(handler==actual.gameObject,
                "The visible city label is not the first real touch handler: "+
                (hits.Count>0?hits[0].gameObject.name:"none"));
            // RoyalButton deliberately rejects a click without pointer down
            // and up. The real mobile click sequence must be delivered.
            Need(ExecuteEvents.Execute<IPointerDownHandler>(handler,data,
                    ExecuteEvents.pointerDownHandler),
                "Guidance did not accept real pointer down");
            Need(ExecuteEvents.Execute<IPointerUpHandler>(handler,data,
                    ExecuteEvents.pointerUpHandler),
                "Guidance did not accept real pointer up");
            Need(ExecuteEvents.Execute<IPointerClickHandler>(handler,data,
                    ExecuteEvents.pointerClickHandler),
                "Guidance did not receive real pointer click");
            clicks++;
        }
        static bool SessionRebound(ValoriaIntegratedWorld host)
        {
            var field=typeof(ValoriaIntegratedWorld).GetField("boundGateway",
                BindingFlags.Instance|BindingFlags.NonPublic);
            Need(field!=null,"Canonical session authority field missing");
            var bag=host.Hud.GetComponent<Issue186BagOverlay>();
            var authority=typeof(Issue186BagOverlay).GetField("gateway",
                BindingFlags.Instance|BindingFlags.NonPublic);
            Need(authority!=null,"Saved bag authority identity missing");
            return ReferenceEquals(field.GetValue(host),host.City.Gateway)&&
                bag&&ReferenceEquals(authority.GetValue(bag),host.City.Gateway);
        }
        static CommandResult Send(ICommandGateway gateway,string kind,string target)
        {
            var s=gateway.Snapshot();
            return gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),
                s.PlayerId,kind,target,s.Revision));
        }
        static void Photograph(Camera camera,RenderTexture target,string phase,int width,int height)
        {
            Draw(camera,target);
            var prior=RenderTexture.active;
            var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,width,height),0,0);
                texture.Apply(false);var bytes=texture.EncodeToPNG();
                Need(bytes.Length>12000,"Actual bag picture is suspiciously blank");
                string file="real-vigilia-bag-"+phase+"-"+width+"x"+height+".png";
                File.WriteAllBytes(Path.Combine(Output,file),bytes);
                using(var hash=SHA256.Create())
                    shots.Add(new Shot{file=file,phase=phase,width=width,height=height,
                        bytes=bytes.Length,sha256=BitConverter.ToString(hash.ComputeHash(bytes))
                            .Replace("-","").ToLowerInvariant()});
            }
            finally{RenderTexture.active=prior;Object.DestroyImmediate(texture);}
        }
        // First entry is tested BEFORE fixture helpers auto-complete it.
        // Three production pointer hits are verified against the first real
        // EventSystem target; the nickname is committed by the genuine Button.
        static IEnumerator VerifyInitialEntryHand(ValoriaIntegratedWorld host)
        {
            var type=VigiliaType();Need(type!=null,"Vigilia type missing at first entry");
            var ui=(Component)type.GetMethod("Install",BindingFlags.Public|BindingFlags.Static)
                .Invoke(null,new object[]{host});
            Need(type.GetProperty("ProductionPresentation")?.GetValue(ui) is bool prod && prod,
                "First entry is not the real production-only Vigilia UI");
            var canvas=(Canvas)type.GetProperty("NativeCanvas").GetValue(ui);
            var coach=host.Hud.GetComponent<Issue186FirstUseCoach>();
            Need(canvas&&coach,"First entry has no live tutorial coach/canvas");
            var camera=host.PlayerCamera;Need(camera,"First entry has no player camera");
            var oldTarget=camera.targetTexture;
            float oldAspect=camera.aspect;int oldMask=camera.cullingMask;
            var oldCanvasCamera=canvas.worldCamera;
            var target=new RenderTexture(390,844,24,RenderTextureFormat.ARGB32);
            target.Create();
            try
            {
                camera.targetTexture=target;camera.aspect=390f/844f;
                camera.cullingMask|=1<<5;canvas.worldCamera=camera;
                var coordinator=host.Hud.GetComponent<Eldoria.PlayerClarity.RoyalPlayerEntryCoordinator>();
                Need(coordinator,"Canonical first-entry coordinator missing");
                coordinator.Refresh();
                var field=coordinator.GetType().GetField("prompt",
                    BindingFlags.Instance|BindingFlags.NonPublic);
                var prompt=field?.GetValue(coordinator) as Eldoria.PlayerClarity.RoyalPlayerNamePrompt;
                Need(prompt&&prompt.IsOpen,"Real first name prompt absent before tutorial");
                var input=prompt.GetComponentInChildren<InputField>(true);
                var confirm=prompt.GetComponentsInChildren<Button>(true)
                    .SingleOrDefault(b=>b.name=="Confirm your name");
                Need(input&&confirm,"Actual first-entry input/confirmation button missing");
                Need(string.IsNullOrWhiteSpace(host.City.Gateway.Snapshot().PlayerName),
                    "First-entry fixture unexpectedly already named");
                double limit=EditorApplication.timeSinceStartup+3.0;
                while(EditorApplication.timeSinceStartup<limit &&
                      !(coach.Visible&&coach.TargetId=="nickname-input"))
                    {RenderUi(ui,390,844);yield return null;}
                Need(coach.Visible&&coach.TargetId=="nickname-input",
                    "First name input hand misses actual UI hit: "+coach.Rejection);
                Photograph(camera,target,"first-nickname-input-guide",390,844);
                report.firstNicknameInputOnRealHit=true;
                input.text="Prueba Valoria";
                limit=EditorApplication.timeSinceStartup+3.0;
                while(EditorApplication.timeSinceStartup<limit &&
                      !(coach.Visible&&coach.TargetId=="nickname-confirm"))
                    {RenderUi(ui,390,844);yield return null;}
                Need(coach.Visible&&coach.TargetId=="nickname-confirm",
                    "Name confirmation hand misses actual UI hit: "+coach.Rejection);
                Photograph(camera,target,"first-nickname-confirm-guide",390,844);
                report.firstNicknameConfirmOnRealHit=true;
                // Physical hit testing and real Button handler, not Invoke().
                Click(confirm,camera,target);
                Need(host.City.Gateway.Snapshot().PlayerName=="Prueba Valoria",
                    "Real name confirmation did not persist canonical player name");
                limit=EditorApplication.timeSinceStartup+3.0;
                while(EditorApplication.timeSinceStartup<limit &&
                      !(host.Hud.NarrativeOpen&&coach.Visible&&
                        coach.TargetId=="narrative-continue"))
                    {RenderUi(ui,390,844);yield return null;}
                Need(host.Hud.NarrativeOpen&&coach.Visible&&
                     coach.TargetId=="narrative-continue",
                    "Opening narrative hand misses actual continue hit: "+coach.Rejection);
                Photograph(camera,target,"first-story-continue-guide",390,844);
                report.firstStoryContinueOnRealHit=true;
            }
            finally
            {
                camera.targetTexture=oldTarget;camera.aspect=oldAspect;
                camera.cullingMask=oldMask;canvas.worldCamera=oldCanvasCamera;
                target.Release();Object.DestroyImmediate(target);
            }
        }
        static IEnumerator Run()
        {
            ValoriaIntegratedWorld host=null;
            while(!host||!host.Ready||!host.City||!host.Hud)
            {host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();yield return null;}
            Need(host.City.Probe,"Refuse actual player campaign");
            // Actual input, confirmation and story targets are checked first.
            var firstEntry=VerifyInitialEntryHand(host);
            while(firstEntry.MoveNext())yield return firstEntry.Current;
            int pages=0;
            while(host.Hud.NarrativeOpen)
            {
                Need(pages++<30,"Canonical introductory setup cannot finish");
                // Setup only, excluded from all recorded UI-input proofs.
                // No resource claim/use can be driven through onClick.Invoke.
                host.Hud.Modal.Extra.onClick.Invoke();yield return null;
            }
            var file=Path.GetFullPath(host.City.SaveFile);
            Need(Path.GetDirectoryName(file).TrimEnd(Path.DirectorySeparatorChar)==
                Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar)&&
                Path.GetFileName(file).StartsWith("eldoria-valoria-gameplay-")&&File.Exists(file),
                "Save is not the isolated GUID-named OS-temp probe file");
            var originalBytes=File.ReadAllBytes(file);
            var originalState=JsonUtility.ToJson(host.City.Gateway.Snapshot());
            report.temporaryProbeOnly=true;
            var type=VigiliaType();Need(type!=null,"Exact owned Vigilia source overlay missing");
            var ui=(Component)type.GetMethod("Install",BindingFlags.Public|BindingFlags.Static)
                .Invoke(null,new object[]{host});
            var canvas=(Canvas)type.GetProperty("NativeCanvas").GetValue(ui);
            Need(type.GetProperty("ProductionPresentation")?.GetValue(ui) is bool exclusive && exclusive,
                "The first-use test must run the real replacement presentation, never Editor legacy paint");
            report.productionVigiliaInstalled=true;
            Need(canvas&&canvas!=host.Hud.GetComponent<Canvas>(),"Bag requires the actual independent canvas");
            var camera=host.PlayerCamera;Need(camera,"Canonical camera absent");
            var oldTarget=camera.targetTexture;float oldAspect=camera.aspect;int oldMask=camera.cullingMask;
            RenderTexture rt=null;
            try
            {
                // Real FIRST journey in the same disposable, named and
                // narratively acknowledged player, BEFORE advanced bag seeds.
                // This rejects the original defect where the approved hand
                // existed only for first packet USE and never appeared beside
                // the opening Aserradero interaction.
                var firstStep=SliceRules.CurrentObjectiveKey(host.City.Gateway.Snapshot());
                Need(firstStep=="b1.build-sawmill",
                    "Starting onboarding objective changed: "+firstStep);
                var firstCoach=host.Hud.GetComponent<Issue186FirstUseCoach>();
                Need(firstCoach!=null,"No actual first-journey hand installed on canonical HUD");
                report.firstUseCoachPresent=true;
                rt=new RenderTexture(390,844,24,RenderTextureFormat.ARGB32);rt.Create();
                camera.targetTexture=rt;camera.aspect=390f/844f;
                camera.cullingMask|=1<<5;canvas.worldCamera=camera;
                RenderUi(ui,390,844);
                // Let the real MonoBehaviour Update and EventSystem run, not
                // a synthetic onClick or our own direct pointer callback.
                double pointerDeadline=EditorApplication.timeSinceStartup+1.2;
                while(EditorApplication.timeSinceStartup<pointerDeadline)
                    {yield return null;}
                RenderUi(ui,390,844);
                if(!firstCoach.Visible)
                    Photograph(camera,rt,"FAILED-first-sawmill-guide",390,844);
                Need(firstCoach.Visible && firstCoach.TargetId=="sawmill",
                    "Actual Aserradero tutorial hand did not find a frontmost hit: "+
                    firstCoach.Rejection+"; target="+firstCoach.TargetId);
                Photograph(camera,rt,"first-sawmill-guide",390,844);
                report.firstSawmillHandOnRealHit=true;
                var realLabel=canvas.GetComponentsInChildren<BuildingLabel>(true)
                    .SingleOrDefault(v=>v&&v.name=="Guidance sawmill"&&
                        v.gameObject.activeInHierarchy);
                Need(realLabel&&realLabel.Select,
                    "No real, visible and selectable Aserradero guidance label");
                TouchRealWorldGuidance(realLabel.Select,camera,rt);
                Need(host.Selected=="sawmill"&&
                     host.Hud.SelectedBuildingId=="sawmill",
                    "The first label touch did not actually select the sawmill");
                report.firstSawmillLabelSelected=true;
                double actionDeadline=EditorApplication.timeSinceStartup+3.0;
                while(EditorApplication.timeSinceStartup<actionDeadline &&
                      !(firstCoach.Visible&&firstCoach.TargetId=="building-action"))
                    {RenderUi(ui,390,844);yield return null;}
                Need(firstCoach.Visible&&firstCoach.TargetId=="building-action",
                    "The hand did not move to the real CONSTRUIR action: "+
                    firstCoach.Rejection);
                Photograph(camera,rt,"first-sawmill-build-guide",390,844);
                report.firstBuildingActionHandOnRealHit=true;
                // Reset only the disposable view selection, not a save or
                // building action, before the existing bag/mail fixture.
                host.Hud.SelectBuilding("");
                camera.targetTexture=oldTarget;rt.Release();Object.DestroyImmediate(rt);rt=null;
                foreach(var size in new[]{(390,844),(900,1600),(844,390),(1600,900)})
                {
                    int width=size.Item1,height=size.Item2;
                    var state=JsonUtility.FromJson<PlayerState>(originalState);
                    state.BastionLevel=3;state.SawmillLevel=1;state.BarracksLevel=1;
                    state.ChapterProgress.MarchConfirmed=true;
                    foreach(var beat in new[]{"start","world","mark","barracks","secured",
                        "frontier-open","march-preparation","vigilants-introduction","scouts-introduction"})
                        if(!state.ChapterProgress.SeenNarrative.Contains(beat))
                            state.ChapterProgress.SeenNarrative.Add(beat);
                    state.Mailbox.Messages.Clear(); // Isolated fixture only; original file restored finally.
                    state.ResourceRewards=new ResourceRewardInventory();
                    state.ResourceGuidance=new GuidedTutorialProgress();
                    state.RewardAttention=new AttentionCueLedger();
                    string prefix="native-bag:"+width+"x"+height;
                    Need(ResourceRewardRules.TryIssue(state.ResourceRewards,prefix+":wood","mission","wood",100)&&
                        ResourceRewardRules.TryIssue(state.ResourceRewards,prefix+":stone","mission","stone",1000)&&
                        ResourceRewardRules.TryIssue(state.ResourceRewards,prefix+":food","mission","food",10000),
                        "Cannot seed the isolated package study");
                    string letter=prefix+":letter";
                    Need(ResourceRewardRules.TryIssueBundle(state.ResourceRewards,prefix+":mail","mission",
                        75,50,0,letter),"Cannot seed isolated actual pending mail");
                    state.Mailbox.Receive(new EldoriaV41Message{EventId=letter,Category="recompensas",
                        Type="mission",Title="Recompensa de la prueba aislada"});
                    new Eldoria.Infrastructure.FileStateStore(file).Save(state);host.City.ReloadForProbe();
                    yield return null;
                    while(!SessionRebound(host))yield return null;
                    var gateway=host.City.Gateway;host.City.Apply(gateway.Snapshot());
                    var bag=host.Hud.GetComponent<Issue186BagOverlay>();
                    Need(bag,"Canonical saved bag component missing");
                    if(rt){camera.targetTexture=oldTarget;rt.Release();Object.DestroyImmediate(rt);}
                    rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);rt.Create();
                    camera.targetTexture=rt;camera.aspect=width/(float)height;camera.cullingMask|=1<<5;
                    canvas.worldCamera=camera;RenderUi(ui,width,height);
                    yield return null;RenderUi(ui,width,height);
                    Need(canvas.isActiveAndEnabled && !host.Hud.GetComponent<Canvas>().enabled,
                        "Integrated city still exposes the obsolete HUD canvas");
                    Photograph(camera,rt,"actual-integrated-city",width,height);
                    report.actualIntegratedCityFrames++;
                    var wallet=gateway.Snapshot().Resources;
                    Click(ButtonNamed(ui,"Resource 0"),camera,rt);
                    Need(bag.IsOpen,"Actual resource capsule did not open saved bag; chronicle="+
                        (host.Hud.Navigation&&host.Hud.Navigation.ChronicleLayout)+
                        "; selected="+host.Hud.SelectedBuildingId+
                        "; narrative="+host.Hud.NarrativeOpen+
                        "; modal="+(host.Hud.Modal&&host.Hud.Modal.gameObject.activeSelf)+
                        "; management="+(host.Hud.GetComponent<RoyalGoldV3ManagementScreens>()?.IsOpen??false)+
                        "; retention="+(host.Hud.GetComponent<RoyalGoldV3EarlyRetentionScreens>()?.IsOpen??false));
                    var panel=ui.GetComponentsInChildren<RectTransform>(true)
                        .Single(t=>t.name=="Issue186 saved resource bag");
                    Need(panel.parent==canvas.transform&&!host.Hud.GetComponent<Canvas>().enabled,
                        "Bag is still under the disabled legacy canvas");
                    report.actualBagOnVigilia=true;bag.Refresh();
                    VerifyPaintedReceiptBinding(bag);
                    // In this detached offscreen batch the disabled player
                    // camera does not render newly opened Graphics by itself.
                    // Submit the actual open panel before checking its real
                    // GraphicRaycaster depth, as a player frame would do.
                    Draw(camera,rt);
                    // Let the real component's 160ms guide cadence run. A
                    // custom Editor iterator does not execute WaitForSeconds.
                    double guideReady=EditorApplication.timeSinceStartup+.30;
                    while(EditorApplication.timeSinceStartup<guideReady)yield return null;
                    var actualHand=panel.GetComponentsInChildren<Image>(true)
                        .Single(t=>t.name=="Issue186 authentic tutorial hand");
                    if(!actualHand.gameObject.activeInHierarchy)
                        Photograph(camera,rt,"FAILED-guide-target",width,height);
                    Need(actualHand.gameObject.activeInHierarchy,
                        "Actual combined first-use guide did not become visible on its real target; "+
                        Eldoria.Issue186Art.Issue186ArtKit.LastPointerRejectReason);
                    var renderedAmounts=panel.GetComponentsInChildren<Text>(false)
                        .Where(t=>t && t.isActiveAndEnabled &&
                            t.name=="Issue186 readable saved amount").ToArray();
                    Need(renderedAmounts.Length>=2,
                        "Actual saved amount labels are absent");
                    foreach(var amount in renderedAmounts)
                        Need(!string.IsNullOrEmpty(amount.text) &&
                             amount.cachedTextGenerator.characterCountVisible>=amount.text.Length,
                            "Actual saved amount glyphs are truncated or invisible: "+
                            amount.text+" in "+width+"x"+height);
                    Photograph(camera,rt,"new",width,height);
                    Click(panel.GetComponentsInChildren<Button>(true)
                        .First(b=>b.name=="Issue186 card - mark seen only"&&b.gameObject.activeInHierarchy),
                        camera,rt);
                    // Bag component stays on RoyalHUD while its panel belongs
                    // to Vigilia: locate actual controls from the owning panel.
                    bag.Refresh();
                    var seen=gateway.Snapshot();
                    Need(seen.ResourceRewards.Packets.Single(p=>p.ReceiptId==prefix+":wood").SeenInBag&&
                        !seen.ResourceRewards.Packets.Single(p=>p.ReceiptId==prefix+":wood").Used&&
                        seen.Resources.Wood==wallet.Wood,"Seeing a card consumed its reward");
                    report.seenDoesNotUse=true;
                    Click(panel.GetComponentsInChildren<Button>(true)
                        .Single(b=>b.name=="Issue186 dismiss first-use guidance"&&b.gameObject.activeInHierarchy),
                        camera,rt);
                    var skipped=gateway.Snapshot();
                    Need(skipped.ResourceGuidance.Dismissed.Contains(ResourceRewardCommands.FirstUseStep)&&
                        !skipped.ResourceGuidance.Completed.Contains(ResourceRewardCommands.FirstUseStep)&&
                        skipped.Resources.Wood==wallet.Wood,"Skip falsely completed or consumed first use");
                    report.skipDoesNotComplete=true;
                    Click(panel.GetComponentsInChildren<Button>(true)
                        .Single(b=>b.name=="Issue186 saved packet USE 0"&&b.gameObject.activeInHierarchy),
                        camera,rt);
                    var used=gateway.Snapshot();
                    Need(used.Resources.Wood==wallet.Wood+100&&
                        used.ResourceRewards.Packets.Single(p=>p.ReceiptId==prefix+":wood").Used,
                        "Actual USE did not credit the exact packet once");
                    long revision=used.Revision;
                    Need(!Send(gateway,ResourceRewardCommands.UsePacket,prefix+":wood").Ok&&
                        gateway.Snapshot().Revision==revision,"Used receipt was paid or saved twice");
                    report.useCreditsOnce=true;
                    Click(panel.GetComponentsInChildren<Button>(true)
                        .Single(b=>b.name=="Issue186 bag close"&&b.gameObject.activeInHierarchy),camera,rt);
                    // Re-enabling the real utility Graphic needs one normal
                    // player frame, not repeated same-tick synthetic renders.
                    RenderUi(ui,width,height);yield return null;RenderUi(ui,width,height);
                    var mailboxEntry=type.GetProperty("ActualMailboxEntry").GetValue(ui) as Button;
                    Click(mailboxEntry,camera,rt);RenderUi(ui,width,height);
                    yield return null;RenderUi(ui,width,height);
                    var mailWindow=ui.GetComponentsInChildren<RectTransform>(true)
                        .Single(t=>t.name=="V41 / Buzón destination"&&t.gameObject.activeInHierarchy);
                    Click(ButtonNamed(mailWindow,"Buzón recompensas"),camera,rt);
                    RenderUi(ui,width,height);yield return null;RenderUi(ui,width,height);
                    Click(ButtonNamed(mailWindow,"Buzón / mensaje 0"),camera,rt);
                    bag.Refresh();RenderUi(ui,width,height);yield return null;RenderUi(ui,width,height);
                    Need(gateway.Snapshot().Mailbox.Messages.Single(m=>m.EventId==letter).Read,
                        "Actual letter row did not mark its own message read");
                    Need(gateway.Snapshot().ResourceRewards.Packets
                        .Where(p=>p.MailEventId==letter).All(p=>!p.Collected),
                        "Opening the actual letter claimed locked mail packets");
                    Need(bag.ClaimableMailCount()==1,
                        "Reading erased actual claimable rewards before collection");
                    var visibleMailText=mailWindow.GetComponentsInChildren<Text>(false)
                        .Where(t=>t && t.isActiveAndEnabled).Select(t=>t.text??"").ToArray();
                    Need(visibleMailText.Any(t=>t.Contains("ARCHIVO REAL")) &&
                         !visibleMailText.Any(t=>t=="RONDAS" || t=="VIDA RESTANTE" ||
                             t.StartsWith("ENCUENTRO · ")),
                        "An empty deserialized battle shell turned genuine reward mail into a fabricated combat report");
                    Need(host.Hud.GetComponent<RoyalGoldV41MailboxScreen>().VisibleRewardAttachments==2,
                        "Actual unclaimed reward mail must show both saved original item attachments");
                    report.mailReadDoesNotClaim=true;
                    Photograph(camera,rt,"mail-read-unclaimed",width,height);
                    int beforeClaim=gateway.Snapshot().Resources.Wood;
                    Click(ButtonNamed(mailWindow,"Leer"),camera,rt);
                    bag.Refresh();RenderUi(ui,width,height);yield return null;RenderUi(ui,width,height);
                    Need(gateway.Snapshot().Resources.Wood==beforeClaim&&
                        gateway.Snapshot().ResourceRewards.Packets
                            .Where(p=>p.MailEventId==letter).All(p=>p.Collected&&!p.Used),
                        "Actual mailbox claim did not move exact components to bag without credit");
                    Need(bag.ClaimableMailCount()==0,
                        "Actual claimed mail still has a pending reward badge");
                    report.mailClaimDoesNotCredit=true;
                    Photograph(camera,rt,"mail-claimed-no-credit",width,height);
                    Click(ButtonNamed(mailWindow,"Volver"),camera,rt);
                    RenderUi(ui,width,height);yield return null;RenderUi(ui,width,height);
                    Click(ButtonNamed(mailWindow,"Volver"),camera,rt);
                    Need(!host.Hud.GetComponent<RoyalGoldV41MailboxScreen>().IsOpen,
                        "Actual mailbox Back did not close the letter destination");
                    RenderUi(ui,width,height);
                    Click(ButtonNamed(ui,"Resource 0"),camera,rt);
                    Need(bag.IsOpen,"Actual resource capsule could not reopen after mailbox claim");
                    report.fullMailboxUiFlowVerified=true;
                    host.City.ReloadForProbe();yield return null;
                    while(!SessionRebound(host))yield return null;
                    gateway=host.City.Gateway;bag=host.Hud.GetComponent<Issue186BagOverlay>();
                    bag.Refresh();RenderUi(ui,width,height);
                    Need(gateway.Snapshot().Resources.Wood==wallet.Wood+100&&
                        gateway.Snapshot().ResourceRewards.Packets.Single(p=>p.ReceiptId==prefix+":wood").Used&&
                        gateway.Snapshot().ResourceGuidance.Dismissed.Contains(ResourceRewardCommands.FirstUseStep),
                        "Actual JSON reload lost wallet, tombstone or saved help dismissal");
                    report.jsonReloadVerified=true;
                    Photograph(camera,rt,"used-reloaded",width,height);
                    Click(panel.GetComponentsInChildren<Button>(true)
                        .Single(b=>b.name=="Issue186 bag close"&&b.gameObject.activeInHierarchy),camera,rt);
                    Need(!bag.IsOpen,"Actual bag Back button failed");
                }
            }
            finally
            {
                camera.targetTexture=oldTarget;camera.aspect=oldAspect;camera.cullingMask=oldMask;
                if(rt){rt.Release();Object.DestroyImmediate(rt);}
                if(ui)Object.DestroyImmediate(ui.gameObject);
                File.WriteAllBytes(file,originalBytes);
                Need(File.ReadAllBytes(file).SequenceEqual(originalBytes),"Original isolated probe bytes were not restored");
                host.City.ReloadForProbe();
            }
        }
    }
}
#endif
