#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;
using Eldoria.ReferenceClean;
using Eldoria.ReferenceClean.Editor;
using Eldoria.UI.RoyalGold.V2;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Eldoria.BreachWatch.Editor
{
    // Production replacement for historical Royal-painter acceptance. Exercise
    // real Vigilia raycasts and gateway saves, never an owner's player file.
    [InitializeOnLoad]
    public static class BreachWatchRetentionInteractionEvidence
    {
        const string Flag="Vigilia.RetentionInteraction.Run";
        const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        static string Output=>Path.GetFullPath(Environment.GetEnvironmentVariable("EARLY_RETENTION_EVIDENCE")??
            Path.Combine(UnityEngine.Application.dataPath,"../BreachWatchRetentionInteractionEvidence"));
        static readonly List<string> checks=new List<string>();
        static BreachWatchRetentionInteractionEvidence()=>EditorApplication.playModeStateChanged+=Entered;
        static void Need(bool value,string message)
        {
            if(!value)throw new InvalidOperationException("VIGILIA_RETENTION_INTERACTION_FAIL: "+message);
            checks.Add(message);
        }
        public static void Capture()
        {
            Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
            Environment.SetEnvironmentVariable("ELDORIA_NEW_UI_ONLY","1");
            Directory.CreateDirectory(Output);checks.Clear();
            EditorSceneManager.OpenScene(Scene);SessionState.SetBool(Flag,true);
            EditorApplication.EnterPlaymode();
        }
        static void Entered(PlayModeStateChange state)
        {
            if(state!=PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Flag,false))return;
            SessionState.SetBool(Flag,false);
            EditorApplication.delayCall+=()=>{
                var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                if(!host){Fail(new Exception("Integrated host missing"));return;}
                host.StartCoroutine(Guard(host));
            };
        }
        static IEnumerator Guard(ValoriaIntegratedWorld host)
        {
            var steps=Probe(host);
            while(true)
            {
                bool more;object current=null;
                try{more=steps.MoveNext();if(more)current=steps.Current;}
                catch(Exception e){Fail(e);yield break;}
                if(!more){File.WriteAllText(Path.Combine(Output,"result.txt"),
                    "VIGILIA_RETENTION_INTERACTION_PASS\n"+string.Join("\n",checks)+"\nART_APPROVED=false\n");
                    File.WriteAllText(Path.Combine(Output,"source.txt"),Environment.GetEnvironmentVariable("GITHUB_SHA"));
                    Debug.Log("VIGILIA_RETENTION_INTERACTION_PASS");EditorApplication.Exit(0);yield break;}
                yield return current;
            }
        }
        static void Fail(Exception e)
        {
            Directory.CreateDirectory(Output);File.WriteAllText(Path.Combine(Output,"error.txt"),e.ToString());
            Debug.LogException(e);EditorApplication.Exit(1);
        }
        static void Save(ValoriaIntegratedWorld host,PlayerState state)
        {
            Need(host.City.Probe,"Only disposable probe save may be changed");
            new FileStateStore(host.City.SaveFile).Save(state);host.City.ReloadForProbe();
        }
        static void Render(ValoriaIntegratedWorld host,BreachWatchHud ui,string file,Button click=null)
        {
            var camera=host.PlayerCamera;var oldTarget=camera.targetTexture;
            var oldActive=RenderTexture.active;float aspect=camera.aspect;int mask=camera.cullingMask;
            var target=new RenderTexture(390,844,24);var pixels=new Texture2D(390,844,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target;camera.aspect=390f/844f;camera.cullingMask|=1<<5;
                ui.NativeCanvas.worldCamera=camera;ui.Reflow(390,844);ui.RenderNow();ui.RealRetention.SynchronizeVisual();
                Canvas.ForceUpdateCanvases();ui.GetComponent<BreachWatchProductionPresentation>().Enforce();
                camera.Render();
                Need(ui.LegacyPresentationHidden,"Retired Canvas stays hidden during real interaction");
                if(file!=null)
                {
                    RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,390,844),0,0);pixels.Apply();
                    File.WriteAllBytes(Path.Combine(Output,file+".png"),pixels.EncodeToPNG());
                }
                if(click)
                {
                    Need(click.isActiveAndEnabled && click.interactable,"Real command is visible and actionable: "+click.name);
                    var rect=(RectTransform)click.transform;
                    var position=RectTransformUtility.WorldToScreenPoint(camera,rect.TransformPoint(rect.rect.center));
                    Need(position.x>=0&&position.x<=390&&position.y>=0&&position.y<=844,"Command is inside actual mobile viewport");
                    var data=new PointerEventData(EventSystem.current){position=position,button=PointerEventData.InputButton.Left,pointerId=-1};
                    var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(data,hits);
                    Need(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==click,"Actual first raycast reaches "+click.name);
                    ExecuteEvents.Execute(click.gameObject,data,ExecuteEvents.pointerDownHandler);
                    ExecuteEvents.Execute(click.gameObject,data,ExecuteEvents.pointerUpHandler);
                    ExecuteEvents.Execute(click.gameObject,data,ExecuteEvents.pointerClickHandler);
                }
            }
            finally
            {
                camera.targetTexture=oldTarget;camera.aspect=aspect;camera.cullingMask=mask;RenderTexture.active=oldActive;
                target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(pixels);
            }
        }
        static Button Command(BreachWatchHud ui,string name)=>
            ui.RealRetention.LiveScreenRect.GetComponentsInChildren<Button>(true).Single(x=>x.name==name);
        static IEnumerator Probe(ValoriaIntegratedWorld host)
        {
            for(int i=0;i<600&&!host.Ready;i++)yield return null;
            Need(host.Ready&&host.City.Probe,"Canonical disposable scene ready");
            if(host.Guidance)host.Guidance.enabled=false;
            ValoriaIntegrationBuild.CompleteProbePlayerName(host);
            for(int i=0;i<50&&host.Hud.NarrativeOpen;i++){host.Hud.Modal.Extra.onClick.Invoke();yield return null;}
            Need(!host.Hud.NarrativeOpen,"Probe setup introduction completed");
            byte[] original=File.ReadAllBytes(host.City.SaveFile);
            var ui=BreachWatchHud.Install(host);
            try
            {
                var state=host.City.Gateway.Snapshot();state.BastionLevel=3;state.BarracksLevel=3;state.SawmillLevel=1;
                state.Available.ArcherT1=500;state.Available.ArcherT2=200;
                state.ChapterProgress.BastionTwoReached=true;state.ChapterProgress.MarchConfirmed=true;
                state.ChapterThree.LyraRecruited=true;
                foreach(var beat in new[]{"march-preparation","vigilants-introduction","scouts-introduction"})
                    if(!state.ChapterProgress.SeenNarrative.Contains(beat))state.ChapterProgress.SeenNarrative.Add(beat);
                state.Retention.LastClearedStage=0;state.Retention.Orders.Clear();
                state.Retention.TotalScoutReports=0;state.Retention.ScoutDiscoveryMask=0;
                state.Retention.LatestDiscoveryTitle="";state.Retention.LatestDiscoveryText="";
                var clock=host.City.Clock as ValoriaReferenceCleanGameplay.TestClock;
                Need(clock!=null,"Only disposable clock is advanced");
                EarlyRetentionRules.Synchronize(state,clock.UtcTicks);Save(host,state);yield return null;
                Need(ui.RealRetention.TryOpen(RoyalGoldV3EarlyRetentionScreens.Destination.Vigilants),"Actual earned Vigilants destination opens");
                yield return null;
                Render(host,ui,"vigilants-before-real-battle",Command(ui,"Battle against next unique stage"));yield return null;
                var won=host.City.Gateway.Snapshot();
                Need(won.Retention.LastClearedStage==1&&won.LastBattleReport.Won,"Real first-hit battle resolves one canonical stage");
                Render(host,ui,"vigilants-real-victory");ui.RealRetention.Close();
                // Match the historical approved guardian-5 and Engendro-15 compositions,
                // using only a disposable test state, never awarding these stages.
                foreach(int stage in new[]{5,15})
                {
                    var artState=JsonUtility.FromJson<PlayerState>(JsonUtility.ToJson(won));artState.Retention.LastClearedStage=stage-1;
                    Save(host,artState);yield return null;
                    Need(ui.RealRetention.TryOpen(RoyalGoldV3EarlyRetentionScreens.Destination.Vigilants),"Real guardian composition opens: "+stage);
                    yield return null;Render(host,ui,"vigilants-guardian-"+stage);
                    var portraits=ui.RealRetention.LiveScreenRect.GetComponentsInChildren<RawImage>(true);
                    Need(portraits.Any(x=>x.isActiveAndEnabled&&x.texture&&
                        x.texture.name==(stage==5?"vigilant-guardian-coraza-v5":"engendro-reference-v3")),"Actual canonical guardian portrait is visible: "+stage);
                    ui.RealRetention.Close();
                }
                var weak=JsonUtility.FromJson<PlayerState>(JsonUtility.ToJson(won));
                weak.Retention.LastClearedStage=14;weak.Available=new ArmyRoster{ArcherT1=1};
                weak.MarchConfigured=false;weak.PreparedTroops=new ArmyRoster();
                Save(host,weak);yield return null;
                Need(ui.RealRetention.TryOpen(RoyalGoldV3EarlyRetentionScreens.Destination.Vigilants),"Real losing stage opens");yield return null;
                Render(host,ui,null,Command(ui,"Battle against next unique stage"));yield return null;
                var lost=host.City.Gateway.Snapshot();
                Need(lost.Retention.LastClearedStage==14&&!lost.LastBattleReport.Won&&lost.LastBattleReport.TargetId=="vigilant:15"&&
                    lost.Resources.Wood==weak.Resources.Wood&&lost.Resources.Stone==weak.Resources.Stone&&
                    lost.Retention.AldricExperience==weak.Retention.AldricExperience,"Real one-troop defeat never advances or grants victory rewards");
                Render(host,ui,"vigilants-real-defeat");host.City.ReloadForProbe();yield return null;
                Need(host.City.Gateway.Snapshot().LastBattleReport.TargetId=="vigilant:15","Actual defeat report survives reload");
                ui.RealRetention.Close();
                Save(host,won);yield return null;
                Need(ui.RealRetention.TryOpen(RoyalGoldV3EarlyRetentionScreens.Destination.Scouts),"Actual earned Scouts destination opens");yield return null;
                Render(host,ui,null,Command(ui,"Prepare actual march or choose scout strategy"));yield return null;
                int troopsBefore=host.City.Gateway.Snapshot().Available.Total;
                for(int i=1;i<=3;i++)
                {
                    int count=host.City.Gateway.Snapshot().Retention.Orders.Count(x=>x.StartedUtcTicks>0);
                    Render(host,ui,null,Command(ui,"Dispatch or claim scout "+i));yield return null;
                    Need(host.City.Gateway.Snapshot().Retention.Orders.Count(x=>x.StartedUtcTicks>0)==count+1,"Exactly one real UTC order dispatched: "+i);
                }
                var sent=host.City.Gateway.Snapshot().Retention.Orders.Where(x=>x.StartedUtcTicks>0).ToArray();
                Need(sent.Length==3,"All three actual Scout contracts dispatched");
                Need(sent.All(x=>x.StrategyMode==1&&x.Troops.Total==EarlyRetentionRules.ScoutTroopsForStrategy(1))&&
                    host.City.Gateway.Snapshot().Available.Total==troopsBefore-sent.Sum(x=>x.Troops.Total),"Actual selected stealth route reserves only its required troops");
                host.City.ReloadForProbe();yield return null;
                Need(sent.All(x=>host.City.Gateway.Snapshot().Retention.Orders.Any(y=>y.Id==x.Id&&y.StrategyMode==1&&y.Troops.Total==x.Troops.Total)),"Dispatched strategy and troop reservations survive reload");Render(host,ui,"scouts-real-underway");
                yield return new WaitForSecondsRealtime(2.2f);
                clock.Step((int)Math.Ceiling((sent.Max(x=>x.EndsUtcTicks)-clock.UtcTicks)/(double)TimeSpan.TicksPerSecond)+1);
                host.City.Gateway.Advance();ui.RealRetention.SynchronizeVisual();yield return null;Render(host,ui,"scouts-real-ready");
                for(int i=3;i>=1;i--){Render(host,ui,null,Command(ui,"Dispatch or claim scout "+i));yield return null;}
                var discoveries=host.City.Gateway.Snapshot().Retention;
                int earnedMask=sent.Aggregate(0,(mask,order)=>mask|(1<<order.Kind));
                Need(discoveries.TotalScoutReports==3&&discoveries.ScoutDiscoveryMask==earnedMask&&
                    !string.IsNullOrWhiteSpace(discoveries.LatestDiscoveryText),"Only actual claims create saved reports and discovered families");
                ui.RealRetention.SynchronizeVisual();Render(host,ui,"scouts-three-real-discoveries");
                var recovered=ui.RealRetention.LiveScreenRect.GetComponentInChildren<BreachWatchRecoveredRetentionArt>(true);
                Need(recovered&&recovered.VisibleDiscoveryMask==earnedMask,"Vigilia displays exactly the genuinely discovered map families");
                var retired=ui.RealRetention.LiveScreenRect.GetComponentInChildren<RoyalGoldV3ScoutDiscoverySeals>(true);
                Need(retired&&!retired.enabled,"Original discovery painter remains retired");
                // New UTC periods create real contracts. Earn the remaining
                // families through actual first-hit UI commands, never set a mask.
                for(int n=0;n<3;n++)
                {
                    if(!host.City.Gateway.Snapshot().Retention.Orders.Any(x=>!x.Claimed&&x.StartedUtcTicks==0))
                    {clock.Step(12*3600);host.City.Gateway.Advance();}
                    ui.RealRetention.SynchronizeVisual();yield return null;
                    Render(host,ui,null,Command(ui,"Dispatch or claim scout 1"));yield return null;
                    var order=host.City.Gateway.Snapshot().Retention.Orders.Single(x=>!x.Claimed&&x.StartedUtcTicks>0);
                    clock.Step((int)Math.Ceiling((order.EndsUtcTicks-clock.UtcTicks)/(double)TimeSpan.TicksPerSecond)+1);
                    host.City.Gateway.Advance();ui.RealRetention.SynchronizeVisual();yield return null;
                    Render(host,ui,null,Command(ui,"Dispatch or claim scout 1"));yield return null;
                    Need(host.City.Gateway.Snapshot().Retention.Orders.Any(x=>x.Id==order.Id&&x.Claimed),"Further report really claimed: "+n);
                }
                host.City.ReloadForProbe();yield return null;
                ui.RealRetention.SynchronizeVisual();Render(host,ui,"scouts-six-real-reports-four-families");
                Need(host.City.Gateway.Snapshot().Retention.TotalScoutReports==6&&recovered.VisibleDiscoveryMask==15,
                    "Six actual saved reports display all four earned families after reload");
                ui.RealRetention.Close();
                Need(ui.RealRetention.TryOpen(RoyalGoldV3EarlyRetentionScreens.Destination.Vigilants),"Vigilants returns after actual scout claims");yield return null;
                var before=host.City.Gateway.Snapshot().Retention;
                int earned=EarlyRetentionRules.UnclaimedIdleXp(before,clock.UtcTicks);
                Need(earned>0,"Real elapsed probe time earned idle hero XP");
                Render(host,ui,null,Command(ui,"Claim earned hero experience"));yield return null;
                var claimed=host.City.Gateway.Snapshot();
                Need(claimed.Retention.AldricExperience==before.AldricExperience+earned&&
                    EarlyRetentionRules.UnclaimedIdleXp(claimed.Retention,clock.UtcTicks)==0,"Actual hero XP is collected once");
                Need(sent.All(order=>claimed.Retention.Orders.Any(x=>x.Id==order.Id&&x.Claimed)),"All three actual rewards claimed exactly once");
                Render(host,ui,null,Command(ui,"Return to actual kingdom"));yield return null;
                Need(!ui.RealRetention.IsShowing,"Real return control closes the destination");
                host.City.ReloadForProbe();yield return null;var loaded=host.City.Gateway.Snapshot();
                Need(loaded.Retention.LastClearedStage==1&&loaded.Retention.AldricExperience==claimed.Retention.AldricExperience&&
                    sent.All(order=>loaded.Retention.Orders.Any(x=>x.Id==order.Id&&x.Claimed)),"Reload preserves actual battle, scout claims and hero XP");
            }
            finally
            {
                ui.RealRetention.Close();File.WriteAllBytes(host.City.SaveFile,original);host.City.ReloadForProbe();
                Need(File.ReadAllBytes(host.City.SaveFile).SequenceEqual(original),"Original disposable file restored byte-for-byte");
            }
        }
    }
}
#endif
