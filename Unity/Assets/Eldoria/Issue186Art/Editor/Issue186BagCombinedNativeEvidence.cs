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
            public int realFirstHitClicks,actualIntegratedCityFrames,firstBuildingActionViewportFrames;
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
        static void RequireCleanSelfExplainingGuide(Canvas canvas,string state)
        {
            var plate=canvas.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(r=>r&&r.name=="First-use caption plate");
            Need(plate&&!plate.gameObject.activeInHierarchy,
                "The redundant coach caption covers the real "+state+" text");
        }
        static void RequireSelectedCityLabelUnpainted(Canvas canvas,
            string building,string viewport)
        {
            // After the real first click the label must neither paint over
            // the construction panel nor catch an invisible first touch.
            var label=canvas.GetComponentsInChildren<BuildingLabel>(true)
                .FirstOrDefault(v=>v && v.name=="Guidance "+building &&
                    v.gameObject.activeInHierarchy);
            if(!label)return; // The production UI may remove its selected label.
            var group=label.GetComponent<CanvasGroup>();
            Need(group && group.alpha<.001f && !group.blocksRaycasts,
                "Selected "+building+" label still paints/blocks the "+
                viewport+" actual construction panel");
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
        // GPU acceptance, not just active GameObjects / raycast handlers.
        // Real native review caught a false green: the guide action was
        // "clickable" in the component tree while its 390x844 screenshot
        // showed only the city, no visible building sheet or hand.
        static Color32[] RenderedPatch(Camera camera,RenderTexture target,
            RectTransform control)
        {
            Need(camera && target && control,"Native visible-control probe missing input");
            var center=RectTransformUtility.WorldToScreenPoint(camera,
                control.TransformPoint(control.rect.center));
            const int pw=40,ph=30;
            int x=Mathf.FloorToInt(center.x-pw*.5f);
            int y=Mathf.FloorToInt(center.y-ph*.5f);
            Need(x>=0 && y>=0 && x+pw<=target.width && y+ph<=target.height,
                "Actual guide target is outside the physical phone render: "+
                control.name+" center="+center+" output="+target.width+"x"+target.height);
            Draw(camera,target);
            var prior=RenderTexture.active;
            var tex=new Texture2D(pw,ph,TextureFormat.RGB24,false);
            try
            {
                RenderTexture.active=target;
                tex.ReadPixels(new Rect(x,y,pw,ph),0,0);
                tex.Apply(false,false);
                return tex.GetPixels32();
            }
            finally
            {
                RenderTexture.active=prior;
                Object.DestroyImmediate(tex);
            }
        }
        // Native debug geometry for the *painted* 390x844 output, not the
        // Editor host window. A RectTransform can be active and raycastable
        // while every visible pixel is outside the actual render target.
        static string ActualScreenRect(Camera camera,RectTransform control)
        {
            if(!camera || !control)return "<missing>";
            var corners=new Vector3[4];
            control.GetWorldCorners(corners);
            var min=new Vector2(float.PositiveInfinity,float.PositiveInfinity);
            var max=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
            foreach(var corner in corners)
            {
                var point=RectTransformUtility.WorldToScreenPoint(camera,corner);
                min=Vector2.Min(min,point);max=Vector2.Max(max,point);
            }
            return "["+min.x.ToString("F1")+","+min.y.ToString("F1")+
                   ".."+max.x.ToString("F1")+","+max.y.ToString("F1")+"]";
        }
        // Read the actual uGUI frontmost hit after a viewport reflow; unlike
        // the coach's cached Visible flag this cannot pass on stale geometry.
        // This probe does not click, change the saved state or increment the
        // 54 real gameplay/UI interactions.
        static void RequireActualFirstHitOnButton(Button action,Camera camera,
            RenderTexture target,string viewport)
        {
            Need(action && action.isActiveAndEnabled && action.IsInteractable(),
                "Rotated building CTA not interactable at "+viewport);
            Draw(camera,target);
            var system=EventSystem.current;
            Need(system && system.isActiveAndEnabled,
                "Real EventSystem absent at "+viewport);
            var rect=(RectTransform)action.transform;
            var point=RectTransformUtility.WorldToScreenPoint(camera,
                rect.TransformPoint(rect.rect.center));
            var data=new PointerEventData(system){position=point};
            var hits=new List<RaycastResult>();
            system.RaycastAll(data,hits);
            var handler=hits.Count>0
                ?ExecuteEvents.GetEventHandler<IPointerClickHandler>(
                    hits[0].gameObject):null;
            Need(handler==action.gameObject,
                "Real CONSTRUIR first-hit occluded at "+viewport+
                "; first="+(hits.Count>0?hits[0].gameObject.name:"none")+
                "; position="+point+
                "; CTA="+ActualScreenRect(camera,rect));
        }
        // Art QA: a hand painted near CONSTRUIR is not enough. Its actual
        // authored fingertip sits at the RectTransform pivot (.16,.696).
        // On every viewport, this pixel must lie inside the real button and
        // resolve to the SAME foremost click handler, without tapping it.
        static void RequirePaintedFingertipHit(Button action,Image hand,
            Camera camera,string viewport)
        {
            Need(action && hand && hand.sprite && hand.gameObject.activeInHierarchy,
                "Missing painted fingertip/button at "+viewport);
            var control=(RectTransform)action.transform;
            var corners=new Vector3[4];control.GetWorldCorners(corners);
            var lo=new Vector2(float.PositiveInfinity,float.PositiveInfinity);
            var hi=new Vector2(float.NegativeInfinity,float.NegativeInfinity);
            foreach(var corner in corners)
            {
                var p=RectTransformUtility.WorldToScreenPoint(camera,corner);
                lo=Vector2.Min(lo,p);hi=Vector2.Max(hi,p);
            }
            var tip=RectTransformUtility.WorldToScreenPoint(
                camera,hand.rectTransform.position);
            Need(tip.x>=lo.x+2f && tip.x<=hi.x-2f &&
                 tip.y>=lo.y+2f && tip.y<=hi.y-2f,
                "Painted fingertip outside actual CONSTRUIR at "+viewport+
                "; tip="+tip+"; CTA="+ActualScreenRect(camera,control));
            var events=EventSystem.current;
            Need(events && events.isActiveAndEnabled,
                "Missing real EventSystem for fingertip at "+viewport);
            var hits=new List<RaycastResult>();
            events.RaycastAll(new PointerEventData(events){position=tip},hits);
            var handler=hits.Count>0
                ?ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject)
                :null;
            Need(handler==action.gameObject,
                "Painted fingertip does not hit CONSTRUIR at "+viewport+
                "; first="+(hits.Count>0?hits[0].gameObject.name:"none")+
                "; tip="+tip);
        }
        static void RequirePaintedInRealFrame(Camera camera,RenderTexture target,
            RectTransform control,GameObject painted,string name)
        {
            Need(painted && painted.activeInHierarchy,
                name+" is not visibly active in its real Canvas");
            // Compare the SAME layout with only the candidate's pixels
            // removed. Deactivating the GameObject can rebuild layout/reflow
            // neighbouring UI, yielding a false GPU difference even when the
            // construction panel/hand itself was never painted.
            var actual=RenderedPatch(camera,target,control);
            var opacity=painted.GetComponent<CanvasGroup>();
            bool addedOpacity=!opacity;
            if(addedOpacity)opacity=painted.AddComponent<CanvasGroup>();
            float priorAlpha=opacity.alpha;
            try
            {
                opacity.alpha=0f;
                var absent=RenderedPatch(camera,target,control);
                int changed=0;
                for(int i=0;i<actual.Length;i++)
                {
                    int contrast=Mathf.Abs(actual[i].r-absent[i].r)+
                        Mathf.Abs(actual[i].g-absent[i].g)+
                        Mathf.Abs(actual[i].b-absent[i].b);
                    if(contrast>48)changed++;
                }
                Need(changed>35,
                    name+" contributes no visible pixels at its real touch target ("+
                    changed+"/"+actual.Length+"); unchanged-layout GPU alpha probe failed");
            }
            finally
            {
                if(opacity)
                {
                    opacity.alpha=priorAlpha;
                    if(addedOpacity)Object.DestroyImmediate(opacity);
                }
                Draw(camera,target);
            }
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
                // Headless native evidence has no automatic GameView render.
                // A Graphic's depth and GraphicRaycaster registry become live
                // only after the real independent Camera/Canvas has drawn.
                // Do not bypass EventSystem or grant a fake tutorial hit:
                // use the same GPU Draw that every subsequent real click uses.
                RenderUi(ui,390,844);
                Draw(camera,target);
                double limit=EditorApplication.timeSinceStartup+3.0;
                while(EditorApplication.timeSinceStartup<limit &&
                      !(coach.Visible&&coach.TargetId=="nickname-input"))
                    {RenderUi(ui,390,844);Draw(camera,target);yield return null;}
                Need(coach.Visible&&coach.TargetId=="nickname-input",
                    "First name input hand misses actual UI hit: "+coach.Rejection);
                RequireCleanSelfExplainingGuide(canvas,"nickname input");
                Photograph(camera,target,"first-nickname-input-guide",390,844);
                report.firstNicknameInputOnRealHit=true;
                input.text="Prueba Valoria";
                limit=EditorApplication.timeSinceStartup+3.0;
                while(EditorApplication.timeSinceStartup<limit &&
                      !(coach.Visible&&coach.TargetId=="nickname-confirm"))
                    {RenderUi(ui,390,844);Draw(camera,target);yield return null;}
                Need(coach.Visible&&coach.TargetId=="nickname-confirm",
                    "Name confirmation hand misses actual UI hit: "+coach.Rejection);
                RequireCleanSelfExplainingGuide(canvas,"nickname confirmation");
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
                    {RenderUi(ui,390,844);Draw(camera,target);yield return null;}
                Need(host.Hud.NarrativeOpen&&coach.Visible&&
                     coach.TargetId=="narrative-continue",
                    "Opening narrative hand misses actual continue hit: "+coach.Rejection);
                RequireCleanSelfExplainingGuide(canvas,"first narrative");
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
                // The original native probe failed before the first actual
                // camera draw but its failure Photograph showed the live,
                // player-visible "Construir aqui: Aserradero" label. Match
                // real player timing: render the production Canvas before
                // querying its first EventSystem hit, never fake the hit.
                Draw(camera,rt);
                double pointerDeadline=EditorApplication.timeSinceStartup+3.0;
                while(EditorApplication.timeSinceStartup<pointerDeadline &&
                      !(firstCoach.Visible&&firstCoach.TargetId=="sawmill"))
                    {RenderUi(ui,390,844);Draw(camera,rt);yield return null;}
                RenderUi(ui,390,844);
                Draw(camera,rt);
                if(!firstCoach.Visible)
                    Photograph(camera,rt,"FAILED-first-sawmill-guide",390,844);
                Need(firstCoach.Visible && firstCoach.TargetId=="sawmill",
                    "Actual Aserradero tutorial hand did not find a frontmost hit: "+
                    firstCoach.Rejection+"; target="+firstCoach.TargetId);
                RequireCleanSelfExplainingGuide(canvas,"Aserradero label");
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
                    {RenderUi(ui,390,844);Draw(camera,rt);yield return null;}
                // Do not accept a guide that exists in the hierarchy but is
                // missing from the picture actually shown to the player.
                // Preserve the GPU frame even when the hand never reaches
                // CONSTRUIR (previously this assertion escaped the screenshot
                // catch), not only when the later pixel contrast check fails.
                // Captures are evidence; first-hit/pixel gates remain strict.
                try
                {
                    // Always capture the actual layout before rejecting a
                    // missing hand: otherwise the error lost all evidence of
                    // whether the sheet was offscreen, disabled or occluded.
                    RenderUi(ui,390,844);
                    var actualPanel=(RectTransform)type.GetProperty("BuildingPanel")
                        .GetValue(ui);
                    var actualAction=(Button)type.GetProperty("BuildingAction").GetValue(ui);
                    var selectedField=type.GetField("selected",
                        BindingFlags.NonPublic|BindingFlags.Instance);
                    var widthField=type.GetField("lastWidth",
                        BindingFlags.NonPublic|BindingFlags.Instance);
                    var heightField=type.GetField("lastHeight",
                        BindingFlags.NonPublic|BindingFlags.Instance);
                    var diagnostic="canonical="+host.Selected+
                        "; hud="+host.Hud.SelectedBuildingId+
                        "; vigilia="+(selectedField?.GetValue(ui)??"<missing>")+
                        "; vigiliaReflow="+(widthField?.GetValue(ui)??"<missing>")+
                        "x"+(heightField?.GetValue(ui)??"<missing>")+
                        "; editorScreen="+Screen.width+"x"+Screen.height+
                        "; renderTarget="+rt.width+"x"+rt.height+
                        "; panelActive="+(actualPanel&&actualPanel.gameObject.activeInHierarchy)+
                        "; ctaActive="+(actualAction&&actualAction.gameObject.activeInHierarchy)+
                        "; canvasPixelRect="+canvas.pixelRect+
                        "; cameraPixelRect="+camera.pixelRect+
                        "; panelPhysicalRect="+ActualScreenRect(camera,actualPanel)+
                        "; ctaPhysicalRect="+ActualScreenRect(camera,
                            actualAction?(RectTransform)actualAction.transform:null);
                    Need(firstCoach.Visible&&firstCoach.TargetId=="building-action",
                        "The hand did not move to the real CONSTRUIR action: "+
                        firstCoach.Rejection+"; "+diagnostic);
                    Need(actualPanel && actualPanel.gameObject.activeInHierarchy &&
                         actualAction && actualAction.gameObject.activeInHierarchy &&
                         actualAction.IsInteractable(),
                        "Selected Aserradero does not show a real construction sheet and CTA: "+
                        diagnostic);
                    RequireSelectedCityLabelUnpainted(canvas,"sawmill","390x844");
                    var realHand=canvas.GetComponentsInChildren<Image>(true)
                        .SingleOrDefault(image=>image && image.name=="Nonblocking actual hand");
                    var handDiagnostic=diagnostic+
                        "; handPhysicalRect="+ActualScreenRect(camera,
                            realHand?realHand.rectTransform:null)+
                        "; handDepth="+(realHand?realHand.depth:-999);
                    Debug.Log("ISSUE186_VISIBLE_BUILD_GEOMETRY "+handDiagnostic);
                    Need(realHand&&realHand.gameObject.activeInHierarchy &&
                         realHand.sprite!=null,
                        "First Aserradero build hand is not visually active: "+handDiagnostic);
                    RequirePaintedInRealFrame(camera,rt,
                        (RectTransform)actualAction.transform,actualPanel.gameObject,
                        "Aserradero action sheet: "+diagnostic);
                    RequirePaintedInRealFrame(camera,rt,
                        realHand.rectTransform,realHand.gameObject,
                        "Aserradero action hand: "+diagnostic);
                    RequireActualFirstHitOnButton(actualAction,camera,rt,"390x844");
                    RequirePaintedFingertipHit(actualAction,realHand,camera,"390x844");
                }
                catch(Exception)
                {
                    // Preserve the unmodified real GPU result before exit(1)
                    // so a red native run remains diagnosable without another
                    // costly 20-minute Editor cycle. Never mask the original
                    // failure if screenshot encoding independently fails.
                    try { Photograph(camera,rt,"FAILED-first-sawmill-build-guide",390,844); }
                    catch(Exception capture) { Debug.LogWarning(
                        "ISSUE186_FAILED_BUILD_FRAME_CAPTURE: "+capture); }
                    throw;
                }
                Photograph(camera,rt,"first-sawmill-build-guide",390,844);
                report.firstBuildingActionViewportFrames++;
                report.firstBuildingActionHandOnRealHit=true;
                // The previous native PASS checked other orientations only
                // for city/bag/mail, not the first building CTA or its hand.
                // Keep the SAME real selected sawmill and run independent GPU
                // paint + live first-hit checks after each device-size reflow.
                // Do not dispatch a construction command or alter a save.
                foreach(var size in new[]{(900,1600),(844,390),(1600,900)})
                {
                    int width=size.Item1,height=size.Item2;
                    string viewport=width+"x"+height;
                    var rotated=new RenderTexture(width,height,24,
                        RenderTextureFormat.ARGB32);
                    rotated.Create();
                    Need(rotated.IsCreated(),"No physical GPU target for "+viewport);
                    camera.targetTexture=rotated;
                    if(rt){rt.Release();Object.DestroyImmediate(rt);}
                    rt=rotated;
                    camera.aspect=(float)width/height;
                    RenderUi(ui,width,height);
                    // The coach deliberately throttles pointer rechecks to
                    // 350 ms on iPhone. Exercise a genuine update after
                    // orientation change, not a stale 390x844 Visible flag.
                    double refreshUntil=EditorApplication.timeSinceStartup+.6;
                    do {Draw(camera,rt);yield return null;RenderUi(ui,width,height);}
                    while(EditorApplication.timeSinceStartup<refreshUntil);
                    try
                    {
                        var panel=(RectTransform)type.GetProperty("BuildingPanel")
                            .GetValue(ui);
                        var action=(Button)type.GetProperty("BuildingAction")
                            .GetValue(ui);
                        var hand=canvas.GetComponentsInChildren<Image>(true)
                            .SingleOrDefault(v=>v&&v.name=="Nonblocking actual hand");
                        string geometry="viewport="+viewport+
                            "; coach="+firstCoach.TargetId+
                            "; rejection="+firstCoach.Rejection+
                            "; panel="+ActualScreenRect(camera,panel)+
                            "; CTA="+ActualScreenRect(camera,
                                action?(RectTransform)action.transform:null)+
                            "; hand="+ActualScreenRect(camera,
                                hand?hand.rectTransform:null)+
                            "; editorScreen="+Screen.width+"x"+Screen.height;
                        Need(firstCoach.Visible&&firstCoach.TargetId=="building-action",
                            "Rotated first-use pointer missing: "+geometry);
                        Need(panel&&panel.gameObject.activeInHierarchy&&
                             action&&action.gameObject.activeInHierarchy&&
                             action.IsInteractable()&&hand&&hand.sprite&&
                             hand.gameObject.activeInHierarchy,
                            "Rotated building panel/CTA/hand absent: "+geometry);
                        RequireSelectedCityLabelUnpainted(canvas,"sawmill",viewport);
                        RequireActualFirstHitOnButton(action,camera,rt,viewport);
                        RequirePaintedFingertipHit(action,hand,camera,viewport);
                        RequirePaintedInRealFrame(camera,rt,
                            (RectTransform)action.transform,panel.gameObject,
                            "Aserradero action sheet: "+geometry);
                        RequirePaintedInRealFrame(camera,rt,
                            hand.rectTransform,hand.gameObject,
                            "Aserradero action hand: "+geometry);
                        Photograph(camera,rt,"first-sawmill-build-guide",width,height);
                        report.firstBuildingActionViewportFrames++;
                    }
                    catch(Exception)
                    {
                        // Preserve the rotated GPU evidence on a red test:
                        // never report success from only live hierarchy state.
                        try {Photograph(camera,rt,"FAILED-first-sawmill-build-guide",
                            width,height);}
                        catch(Exception capture){Debug.LogWarning(
                            "ISSUE186_ROTATED_BUILD_FRAME_CAPTURE: "+capture);}
                        throw;
                    }
                }
                Need(report.firstBuildingActionViewportFrames==4,
                    "First building CTA/hand did not pass all four real viewport captures");
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
