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
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Eldoria.ReferenceClean.Editor
{
    // Real-player-camera proof. Absolutely never load or alter an owner save.
    [InitializeOnLoad] public static class EarlyRetentionEngagementNativeCapture
    {
        const string Flag="Eldoria.RetentionEngagement.Native";
        const string Scene="Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity";
        static string Output=>Path.GetFullPath(Environment.GetEnvironmentVariable("ENGAGEMENT_EVIDENCE")??
            Path.Combine(UnityEngine.Application.dataPath,"../../docs/early-retention/engagement-native"));
        static EarlyRetentionEngagementNativeCapture()=>EditorApplication.playModeStateChanged+=Entered;
        static readonly List<string> Checks=new List<string>();
        static void Need(bool value,string why)
        {
            if(!value)throw new InvalidOperationException("RETENTION_ENGAGEMENT_NATIVE_FAIL: "+why);
            Checks.Add(why);Debug.Log("RETENTION_ENGAGEMENT_NATIVE_PASS: "+why);
        }
        public static void Capture()
        {
            try
            {
                Environment.SetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE","1");
                Directory.CreateDirectory(Output);
                EditorSceneManager.OpenScene(Scene);
                SessionState.SetBool(Flag,true);
                EditorApplication.EnterPlaymode();
            }catch(Exception e){Fail(e);}
        }
        static void Entered(PlayModeStateChange phase)
        {
            if(phase!=PlayModeStateChange.EnteredPlayMode||!SessionState.GetBool(Flag,false))return;
            SessionState.SetBool(Flag,false);
            EditorApplication.delayCall+=()=>{
                var host=Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                if(!host){Fail(new Exception("Canonical Valoria scene missing"));return;}
                host.StartCoroutine(Guard(host));
            };
        }
        static IEnumerator Guard(ValoriaIntegratedWorld host)
        {
            var steps=Exercise(host);
            while(true)
            {
                bool more;object step=null;
                try{more=steps.MoveNext();if(more)step=steps.Current;}
                catch(Exception ex){Fail(ex);yield break;}
                if(!more)yield break;
                yield return step;
            }
        }
        static void Save(ValoriaIntegratedWorld host,PlayerState s)
        {
            Need(host.City.Probe,"Scene is using a disposable saved game, never owner progress");
            new FileStateStore(host.City.SaveFile).Save(s);
            host.City.ReloadForProbe();
        }
        static GameCommand Command(LocalGateway g,string key,string kind,string target)
        {
            var s=g.Snapshot();
            return new GameCommand(key,s.PlayerId,kind,target,s.Revision);
        }
        static void Shot(ValoriaIntegratedWorld host,string name,int w,int h)
        {
            var filename=Path.Combine(Output,name+".png");
            ValoriaIntegrationBuild.Capture(host,filename,w,h);
            Need(File.Exists(filename)&&new FileInfo(filename).Length>24000,
                "Authentic Unity camera PNG generated: "+name);
            var texture=new Texture2D(2,2,TextureFormat.RGB24,false);
            try
            {
                Need(texture.LoadImage(File.ReadAllBytes(filename))&&
                    texture.width==w&&texture.height==h,
                    "Correct real camera resolution: "+name);
            }
            finally{Object.Destroy(texture);}
        }
        static RoyalText FindText(RoyalHUD hud,string name)
            =>hud.GetComponentsInChildren<RoyalText>(true).FirstOrDefault(t=>t.name==name);
        static IEnumerator Exercise(ValoriaIntegratedWorld host)
        {
            for(int i=0;i<300&&!host.Ready;i++)yield return null;
            Need(host.Ready&&host.City.Probe,"Real game booted on throwaway save");
            if(host.Guidance)host.Guidance.enabled=false;
            var hud=host.Hud;Need(hud!=null,"Single canonical HUD loaded");
            for(int i=0;i<50&&hud.NarrativeOpen;i++)
            {
                hud.Modal.Extra.onClick.Invoke();
                yield return null;
            }
            if(hud.Modal.gameObject.activeSelf)hud.Modal.Hide();
            hud.SelectBuilding("");
            var state=host.City.Gateway.Snapshot();
            state.SawmillLevel=1;
            state.BastionLevel=2;
            state.BarracksLevel=1;
            state.ChapterProgress.MarchConfirmed=true;
            state.Available=new ArmyRoster{ArcherT1=100};
            state.Retention.LastClearedStage=4;
            Save(host,state);
            yield return new WaitForSecondsRealtime(.3f);
            var ui=hud.GetComponent<RoyalGoldV3EarlyRetentionScreens>();
            Need(ui!=null,"The same approved two-destination player interface remains");
            ui.Open(RoyalGoldV3EarlyRetentionScreens.Destination.Vigilants);
            yield return new WaitForSecondsRealtime(.25f);
            var subtitle=FindText(hud,"Chapter overline");
            Need(subtitle && subtitle.text.Contains("DEFENSA ALTA"),
                "Real stage-five player banner warns of the actual defense profile");
            var badge=FindText(hud,"Live progress / no decorative counters");
            Need(badge&&badge.text.Contains("4 ETAPAS"),
                "Gilded landscape plaque reflects canonical saved path progress");
            Shot(host,"vigilants-guardian5-landscape",1600,900);
            Shot(host,"vigilants-guardian5-portrait",900,1600);
            Shot(host,"vigilants-guardian5-phone",390,844);
            var g=host.City.Gateway;
            var attack=hud.GetComponentsInChildren<RoyalButton>(true)
                .FirstOrDefault(x=>x.name=="Battle against next unique stage");
            Need(attack&&attack.interactable,"Real UI has an actionable guardian combat button");
            attack.onClick.Invoke();
            var after=g.Snapshot();
            Need(after.Retention.LastClearedStage==5,
                "The REAL RoyalButton click defeats the fifth guardian and advances the path");
            var victory=FindText(hud,"Original victory heading");
            Need(victory&&victory.text=="VICTORIA"&&victory.gameObject.activeInHierarchy,
                "A genuine gold combat celebration is shown, not only a report in code");
            Need(after.LastBattleReport.TargetId=="vigilant:5",
                "Real combat report attached to the fifth guardian");
            Need(after.LastBattleReport.Rounds>0&&after.LastBattleReport.Troops<=
                MarchCapacityRules.Capacity(after,"aldric"),
                "Battle results contain genuine rounds and capacity-limited troop count");
            var lesson=EarlyRetentionRules.BattleReview(after.LastBattleReport,EarlyRetentionRules.Stage(5));
            Need(lesson.Contains("rondas")&&lesson.Contains("tropas"),
                "Useful battle review comes from the actual combat report");
            ui.SynchronizeLayout();
            Shot(host,"vigilants-battle-result-landscape",1600,900);
            // Stage 15's approved original creature remains present; only its
            // trait and combat numbers change.
            state=host.City.Gateway.Snapshot();
            state.Retention.LastClearedStage=14;
            Save(host,state);yield return new WaitForSecondsRealtime(.3f);
            ui.Open(RoyalGoldV3EarlyRetentionScreens.Destination.Vigilants);
            ui.SynchronizeLayout();
            Need(subtitle.text.Contains("RUPTURA ALTA"),
                "The canonical Engendro warns against its real Breach trait");
            Need(FindText(hud,"Real enemy and trait").text.Contains("Engendro de la Brecha"),
                "The original authored Spawnling lore is preserved");
            Shot(host,"vigilants-engendro15-portrait",900,1600);
            // A real losing army must keep the current stage and explain the defeat.
            state=host.City.Gateway.Snapshot();
            state.Available=new ArmyRoster{ArcherT1=1};
            state.MarchConfigured=false;
            state.PreparedTroops=new ArmyRoster();
            Save(host,state);yield return new WaitForSecondsRealtime(.3f);
            ui.SynchronizeLayout();
            attack.onClick.Invoke();
            var loss=host.City.Gateway.Snapshot();
            Need(loss.Retention.LastClearedStage==14&&!loss.LastBattleReport.Won&&
                loss.LastBattleReport.TargetId=="vigilant:15"&&loss.LastBattleReport.Rounds>0,
                "Actual one-troop defeat neither advances nor pays a false victory");
            Need(victory.text=="DERROTA"&&victory.gameObject.activeInHierarchy,
                "Losing combat presents a distinct real defeat banner");
            Shot(host,"vigilants-defeat-phone",390,844);
            host.City.ReloadForProbe();yield return new WaitForSecondsRealtime(.3f);
            ui.SynchronizeLayout();
            Need(host.City.Gateway.Snapshot().LastBattleReport.TargetId=="vigilant:15"&&
                FindText(hud,"Last action result").text.Contains("rondas"),
                "Current-stage defeat analysis survives actual saved-game reload");
            ui.Close();
            state=host.City.Gateway.Snapshot();
            state.Retention.LastClearedStage=0;
            state.Retention.Orders=new List<ScoutOrder>();
            state.Retention.LatestDiscoveryTitle="";
            state.Retention.LatestDiscoveryText="";
            state.Retention.TotalScoutReports=0;
            state.Retention.ScoutDiscoveryMask=0;
            state.Available=new ArmyRoster{ArcherT1=100};
            Save(host,state);yield return new WaitForSecondsRealtime(.3f);
            ui.Open(RoyalGoldV3EarlyRetentionScreens.Destination.Scouts);
            ui.SynchronizeLayout();
            Need(badge.text.Contains("0 INFORMES"),
                "Map begins without fabricated discoveries or seals");
            Shot(host,"scouts-before-discovery-landscape",1600,900);
            g=host.City.Gateway;
            var order=g.Snapshot().Retention.Orders.First(x=>x.Kind!=0);
            string story=EarlyRetentionRules.ScoutDiscoveryText(order);
            var visibleOrders=g.Snapshot().Retention.Orders
                .Where(x=>x!=null&&!x.Claimed)
                .OrderBy(x=>x.StartedUtcTicks>0?0:1)
                .ThenBy(x=>x.Period).ThenBy(x=>x.Slot).Take(3).ToList();
            int slot=visibleOrders.FindIndex(x=>x.Id==order.Id);
            Need(slot>=0,"The contract can be selected by its real visible board position");
            var dispatch=hud.GetComponentsInChildren<RoyalButton>(true)
                .FirstOrDefault(x=>x.name=="Dispatch or claim scout "+(slot+1));
            Need(dispatch&&dispatch.interactable,"Real blue SEND button is interactable");
            dispatch.onClick.Invoke();
            Need(g.Snapshot().Retention.Orders.Any(x=>x.Id==order.Id&&x.StartedUtcTicks>0),
                "Real blue UI action dispatches the selected contract");
            var clock=host.City.Clock as ValoriaReferenceCleanGameplay.TestClock;
            Need(clock!=null,"Only the disposable test clock can advance a report");
            clock.Step(EarlyRetentionRules.ScoutDurationSeconds(order.Kind)+1);
            g.Advance();
            ui.SynchronizeLayout();
            Need(dispatch.interactable,"Real blue CLAIM action unlocks after UTC completion");
            dispatch.onClick.Invoke();
            Need(g.Snapshot().Retention.Orders.Any(x=>x.Id==order.Id&&x.Claimed),
                "The same original UI button claims one completed contract");
            var discoveryTitle=FindText(hud,"Original victory heading");
            Need(discoveryTitle&&discoveryTitle.text.StartsWith("HALLAZGO"),
                "Earned expedition displays a celebratory narrative banner");
            host.City.ReloadForProbe();yield return new WaitForSecondsRealtime(.3f);
            var saved=host.City.Gateway.Snapshot();
            Need(saved.Retention.TotalScoutReports==1&&
                 saved.Retention.LatestDiscoveryText==story,
                 "One original lore discovery survives actual file reload");
            ui.Open(RoyalGoldV3EarlyRetentionScreens.Destination.Scouts);
            ui.SynchronizeLayout();
            Need(badge.text.Contains("1 INFORMES"),
                "The map's first real heraldic seal is earned");
            var journal=FindText(hud,"Real expedition report / persistent");
            Need(journal && journal.gameObject.activeInHierarchy &&
                journal.text.Contains(story),
                "The real narrative appears on the approved cartographer painting");
            Need(hud.GetComponentsInChildren<Image>(true).Any(x=>
                x.name=="Cartographer wax pin / 1"&&x.gameObject.activeInHierarchy),
                "The first wax pin is visible after an earned report");
            Shot(host,"scouts-discovery-landscape",1600,900);
            Shot(host,"scouts-discovery-portrait",900,1600);
            Shot(host,"scouts-discovery-phone",390,844);
            Need(saved.Retention.ScoutDiscoveryMask==(1<<order.Kind),
                "Only the genuinely earned scout family seal is discovered");
            for(int i=0;i<5;i++)
            {
                g=host.City.Gateway;
                var next=g.Snapshot().Retention.Orders.FirstOrDefault(x=>!x.Claimed&&x.StartedUtcTicks==0);
                if(next==null)
                {
                    clock.Step(12*3600);g.Advance();
                    next=g.Snapshot().Retention.Orders.First(x=>!x.Claimed&&x.StartedUtcTicks==0);
                }
                Need(g.Execute(Command(g,"family-send-"+i,"ScoutDispatch",next.Id)).Ok,
                    "Dispatch actual further discovery contract "+i);
                clock.Step(EarlyRetentionRules.ScoutDurationSeconds(next.Kind)+1);g.Advance();
                Need(g.Execute(Command(g,"family-claim-"+i,"ScoutClaim",next.Id)).Ok,
                    "Earn actual further discovery contract "+i);
            }
            host.City.ReloadForProbe();yield return new WaitForSecondsRealtime(.4f);
            saved=host.City.Gateway.Snapshot();
            Need(saved.Retention.TotalScoutReports==6&&saved.Retention.ScoutDiscoveryMask==15,
                "Six actual claims and all four discovered families persist independently");
            ui.SynchronizeLayout();
            var familySeals=hud.GetComponentInChildren<RoyalGoldV3ScoutDiscoverySeals>(true);
            Need(familySeals&&familySeals.DiscoveredMask==15&&familySeals.gameObject.activeInHierarchy,
                "Original map genuinely shows all four earned regional seals");
            Shot(host,"scouts-six-reports-four-families-landscape",1600,900);
            Shot(host,"scouts-six-reports-four-families-portrait",900,1600);
            Shot(host,"scouts-six-reports-four-families-phone",390,844);
            Need(Object.FindObjectsByType<RoyalHUD>(FindObjectsSortMode.None).Length==1,
                "No duplicate HUD, Canvas or game session created");
            File.WriteAllText(Path.Combine(Output,"result.txt"),
                "RETENTION_ENGAGEMENT_NATIVE_PASS\n"+
                string.Join("\n",Checks)+"\nowner-save-modified=false\n");
            Debug.Log("RETENTION_ENGAGEMENT_NATIVE_PASS");
            EditorApplication.Exit(0);
        }
        static void Fail(Exception ex)
        {
            Directory.CreateDirectory(Output);
            File.WriteAllText(Path.Combine(Output,"failure.txt"),ex.ToString());
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }
}
#endif
