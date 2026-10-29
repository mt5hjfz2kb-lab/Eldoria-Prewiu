using System;
using System.Linq;
using Eldoria.Application;
using Eldoria.Domain;
using NUnit.Framework;

namespace Eldoria.Tests
{
    public sealed class EarlyRetentionRulesTests
    {
        sealed class Clock:IClock
        {
            public long UtcTicks{get;set;}=new DateTime(2026,10,28,0,0,0,DateTimeKind.Utc).Ticks;
            public void AddSeconds(long seconds) => UtcTicks=checked(UtcTicks+seconds*TimeSpan.TicksPerSecond);
        }
        sealed class MemoryStateStore:IStateStore
        {
            public PlayerState Value;
            public PlayerState Load() => Value;
            public void Save(PlayerState state) => Value=state;
        }
        static GameCommand Cmd(LocalGateway g,string id,string kind,string target)
        {
            var s=g.Snapshot();return new GameCommand(id,s.PlayerId,kind,target,s.Revision);
        }
        static PlayerState Ready()
        {
            return new PlayerState
            {
                BastionLevel=2,SawmillLevel=1,BarracksLevel=1,
                ChapterProgress=new ChapterProgressState{MarchConfirmed=true},
                Available=new ArmyRoster{ArcherT1=60},
                Resources=new ResourceWallet{Wood=300,Stone=250,Food=100}
            };
        }
        [Test] public void UnlockingScoutsDoesNotInvalidateNextCanonicalFightCommand()
        {
            var time=new Clock();
            var state=Ready();
            state.MarchConfigured=false;
            state.ChapterProgress.MarchConfirmed=false;
            var store=new MemoryStateStore{Value=state};
            var g=new LocalGateway(time,store);
            Assert.That(g.Snapshot().Retention.Orders.Count,Is.Zero);
            var preparing=Cmd(g,"first-real-march","ConfigureMarch","march-main");
            Assert.That(g.Execute(preparing).Ok,Is.True);
            var afterPreparation=g.Snapshot();
            Assert.That(afterPreparation.ChapterProgress.MarchConfirmed,Is.True);
            Assert.That(afterPreparation.Retention.Orders.Count,Is.EqualTo(3),
                "Initial scout cards must be committed together with March preparation");
            var attack=Cmd(g,"next-valid-battle","Fight","corrupt-scout");
            var result=g.Execute(attack);
            Assert.That(result.Ok,Is.True,
                "Scout unlock must never insert an unannounced save revision before a real battle: "+result.Message);
            Assert.That(g.Snapshot().March.Phase,Is.EqualTo("outbound"));
        }

        [Test] public void UnlockButtonsOnlyWhenGameplayTrulyTeachesFeatures()
        {
            var a=new PlayerState();
            Assert.That(EarlyRetentionRules.VigilantsUnlocked(a),Is.False);
            Assert.That(EarlyRetentionRules.ScoutsUnlocked(a),Is.False);
            a.SawmillLevel=1;
            Assert.That(EarlyRetentionRules.VigilantsUnlocked(a),Is.True);
            Assert.That(EarlyRetentionRules.StageUnlocked(a,1),Is.True);
            Assert.That(EarlyRetentionRules.StageUnlocked(a,2),Is.False);
            Assert.That(EarlyRetentionRules.ScoutsUnlocked(a),Is.False);
            a.BastionLevel=2;a.BarracksLevel=1;
            Assert.That(EarlyRetentionRules.ScoutsUnlocked(a),Is.False);
            a.ChapterProgress.MarchConfirmed=true;
            Assert.That(EarlyRetentionRules.ScoutsUnlocked(a),Is.True);
            Assert.That(EarlyRetentionRules.StageUnlocked(a,2),Is.True);
            Assert.That(EarlyRetentionRules.StageUnlocked(a,26),Is.False);
        }
        [Test] public void TwentyFiveStagesPerChapterHaveAuthoredDistinctScenesAndBoss()
        {
            var a=EarlyRetentionRules.Stage(1);
            var b=EarlyRetentionRules.Stage(25);
            var c=EarlyRetentionRules.Stage(26);
            Assert.That(a.Chapter,Is.EqualTo(1));
            Assert.That(b.Step,Is.EqualTo(25));
            Assert.That(b.Name,Is.EqualTo(EarlyRetentionRules.GuardianName(b.Step)));
            Assert.That(c.Chapter,Is.EqualTo(2));
            Assert.That(c.Step,Is.EqualTo(1));
            Assert.That(c.Scene,Is.Not.EqualTo(a.Scene));
            Assert.That(b.Enemy.Health,Is.GreaterThan(a.Enemy.Health));
            Assert.That(EarlyRetentionRules.Stage(15).Name,
                Is.EqualTo("Engendro de la Brecha"),
                "The approved stage-15 art must depict Eldoria's corruption, not a stone golem");
            Assert.That(EarlyRetentionRules.Stage(3).Name,
                Is.EqualTo("Corrupto errante"),
                "Ordinary early challengers must stay within the established bestiary");
        }
        [Test] public void BattleUsesRealCombatAndNeverDuplicatesStageReward()
        {
            var time=new Clock();var save=new MemoryStateStore{Value=Ready()};
            var g=new LocalGateway(time,save);
            var before=g.Snapshot().Resources.Wood;
            var cmd=Cmd(g,"v-1","VigilantBattle","1");
            var first=g.Execute(cmd);
            Assert.That(first.Ok,Is.True,first.Message);
            Assert.That(g.Snapshot().Retention.LastClearedStage,Is.EqualTo(1));
            Assert.That(g.Snapshot().LastBattleReport.TargetId,Is.EqualTo("vigilant:1"));
            Assert.That(g.Snapshot().Resources.Wood,Is.GreaterThan(before));
            long revision=g.Snapshot().Revision;
            int loot=g.Snapshot().Resources.Wood;
            Assert.That(g.Execute(cmd).Ok,Is.True);
            Assert.That(g.Snapshot().Revision,Is.EqualTo(revision));
            Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(loot));
            Assert.That(g.Execute(Cmd(g,"repeat","VigilantBattle","1")).Ok,Is.False);
            g=new LocalGateway(time,save);
            Assert.That(g.Snapshot().Retention.LastClearedStage,Is.EqualTo(1));
        }
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(16)]
        [TestCase(30)]
        public void IdleExperienceAccruesOfflineWithEightHourCapAndNoDoubleClaim(int offlineHours)
        {
            var time=new Clock();var store=new MemoryStateStore{Value=Ready()};
            var g=new LocalGateway(time,store);
            Assert.That(g.Execute(Cmd(g,"battle","VigilantBattle","1")).Ok,Is.True);
            time.AddSeconds(offlineHours*3600);
            g=new LocalGateway(time,store);
            var pending=EarlyRetentionRules.UnclaimedIdleXp(g.Snapshot().Retention,time.UtcTicks);
            Assert.That(pending,Is.EqualTo(8*12));
            var cmd=Cmd(g,"claim","ClaimVigilantXp","aldric");
            Assert.That(g.Execute(cmd).Ok,Is.True);
            var xp=g.Snapshot().Retention.AldricExperience;
            Assert.That(g.Execute(cmd).Ok,Is.True);
            Assert.That(g.Snapshot().Retention.AldricExperience,Is.EqualTo(xp));
            Assert.That(EarlyRetentionRules.UnclaimedIdleXp(g.Snapshot().Retention,time.UtcTicks),Is.Zero);
            Assert.That(g.Execute(Cmd(g,"fresh-claim","ClaimVigilantXp","aldric")).Ok,Is.False);
            Assert.That(g.Snapshot().Retention.AldricExperience,Is.EqualTo(xp));
        }
        [Test] public void ScoutsRespectUnlockTimePersistenceAndSingleClaim()
        {
            var time=new Clock();var store=new MemoryStateStore{Value=Ready()};
            var g=new LocalGateway(time,store);
            var first=g.Snapshot().Retention.Orders.First();
            Assert.That(g.Snapshot().Retention.Orders.Count,Is.EqualTo(3));
            int initialStone=g.Snapshot().Resources.Stone;
            Assert.That(g.Execute(Cmd(g,"tooearly","ScoutClaim",first.Id)).Ok,Is.False);
            Assert.That(g.Execute(Cmd(g,"dispatch","ScoutDispatch",first.Id)).Ok,Is.True);
            time.AddSeconds(EarlyRetentionRules.ScoutDurationSeconds(first.Kind)+1);
            g=new LocalGateway(time,store);
            int initialXp=g.Snapshot().Retention.AldricExperience;
            var reward=Cmd(g,"reward","ScoutClaim",first.Id);
            Assert.That(g.Execute(reward).Ok,Is.True);
            int xp=g.Snapshot().Retention.AldricExperience;
            Assert.That(xp,Is.EqualTo(initialXp+20));
            Assert.That(g.Execute(reward).Ok,Is.True);
            Assert.That(g.Snapshot().Retention.AldricExperience,Is.EqualTo(xp));
            Assert.That(g.Snapshot().Retention.Orders.Single(x=>x.Id==first.Id).Claimed,Is.True);
            Assert.That(g.Snapshot().Resources.Stone,Is.GreaterThanOrEqualTo(initialStone));
        }
        [Test] public void ScoutBoardDoesNotManufactureInfiniteOfflineBacklog()
        {
            var time=new Clock();var store=new MemoryStateStore{Value=Ready()};
            var g=new LocalGateway(time,store);
            time.AddSeconds(30*24*3600);
            g=new LocalGateway(time,store);
            Assert.That(g.Snapshot().Retention.Orders.Count(x=>!x.Claimed),
                Is.LessThanOrEqualTo(EarlyRetentionRules.MaxUnclaimedOrders));
            Assert.That(g.Snapshot().Retention.Orders.Select(x=>x.Id).Distinct().Count(),
                Is.EqualTo(g.Snapshot().Retention.Orders.Count));
        }
        [Test] public void FiveGuardiansHaveRealDistinctStatStrengthsAndUsefulHints()
        {
            var armor=EarlyRetentionRules.Stage(5);
            var assault=EarlyRetentionRules.Stage(10);
            var breach=EarlyRetentionRules.Stage(15);
            var endurance=EarlyRetentionRules.Stage(20);
            var final=EarlyRetentionRules.Stage(25);
            Assert.That(armor.Trait,Does.Contain("DEFENSA"));
            Assert.That(assault.Trait,Does.Contain("ATAQUE"));
            Assert.That(breach.Trait,Does.Contain("RUPTURA"));
            Assert.That(endurance.Trait,Does.Contain("RESISTENCIA"));
            Assert.That(final.Trait,Does.Contain("EQUILIBRIO"));
            Assert.That(armor.Enemy.Defense,Is.GreaterThan(EarlyRetentionRules.Stage(4).Enemy.Defense+20));
            Assert.That(assault.Enemy.Attack,Is.GreaterThan(EarlyRetentionRules.Stage(9).Enemy.Attack+20));
            Assert.That(breach.Enemy.Break,Is.GreaterThan(EarlyRetentionRules.Stage(14).Enemy.Break+25));
            Assert.That(endurance.Enemy.Health,Is.GreaterThan(EarlyRetentionRules.Stage(19).Enemy.Health+240));
            Assert.That(final.Enemy.Defense,Is.GreaterThan(EarlyRetentionRules.Stage(24).Enemy.Defense+10));
            foreach(var boss in new[]{armor,assault,breach,endurance,final})
                Assert.That(EarlyRetentionRules.CombatAdvice(boss),Is.Not.Empty);
            Assert.That(breach.Name,Is.EqualTo("Engendro de la Brecha"));
        }

        [Test] public void BattleAnalysisUsesActualSavedRoundsTroopsAndHero()
        {
            var challenge=EarlyRetentionRules.Stage(15);
            var report=new BattleReportState{TargetId="vigilant:15",
                HeroId="lyra",Won=false,Rounds=6,Troops=37,PlayerPower=2260,
                EnemyPower=challenge.Difficulty,RemainingHealth=0};
            var text=EarlyRetentionRules.BattleReview(report,challenge);
            Assert.That(text,Does.Contain("6 rondas"));
            Assert.That(text,Does.Contain("37 tropas"));
            Assert.That(text,Does.Contain("Lyra"));
            Assert.That(text,Does.Contain("ruptura"));
            Assert.That(EarlyRetentionRules.BattleReview(report,EarlyRetentionRules.Stage(10)),
                Is.Empty,"Reports cannot be shown as an unrelated battle");
        }

        [Test] public void ScoutDiscoveriesAreDeterministicAndPersistAfterReloadWithoutDoubleClaim()
        {
            var time=new Clock();var store=new MemoryStateStore{
                Value=new PlayerState{BastionLevel=2,BarracksLevel=1,SawmillLevel=1,
                    ChapterProgress=new ChapterProgressState{MarchConfirmed=true},
                    Available=new ArmyRoster{ArcherT1=100}}
            };
            var gateway=new LocalGateway(time,store);
            var order=gateway.Snapshot().Retention.Orders.First(x=>x.Kind!=0);
            var expected=EarlyRetentionRules.ScoutDiscoveryText(order);
            Assert.That(expected,Is.Not.Empty);
            Assert.That(gateway.Execute(Cmd(gateway,"scout-story-send","ScoutDispatch",order.Id)).Ok,Is.True);
            time.AddSeconds(EarlyRetentionRules.ScoutDurationSeconds(order.Kind)+1);
            gateway=new LocalGateway(time,store);
            Assert.That(gateway.Execute(Cmd(gateway,"scout-story-claim","ScoutClaim",order.Id)).Ok,Is.True);
            var after=gateway.Snapshot();
            Assert.That(after.Retention.TotalScoutReports,Is.EqualTo(1));
            Assert.That(after.Retention.ScoutDiscoveryMask,Is.EqualTo(1<<order.Kind));
            Assert.That(after.Retention.LatestDiscoveryText,Is.EqualTo(expected));
            var title=after.Retention.LatestDiscoveryTitle;
            var claimCount=after.Retention.TotalScoutReports;
            gateway=new LocalGateway(time,store);
            Assert.That(gateway.Snapshot().Retention.LatestDiscoveryTitle,Is.EqualTo(title));
            Assert.That(gateway.Snapshot().Retention.LatestDiscoveryText,Is.EqualTo(expected));
            Assert.That(gateway.Execute(Cmd(gateway,"scout-story-claim-again","ScoutClaim",order.Id)).Ok,Is.False);
            Assert.That(gateway.Snapshot().Retention.TotalScoutReports,Is.EqualTo(claimCount));
            Assert.That(EarlyRetentionRules.RevealedScoutReports(gateway.Snapshot().Retention),Is.EqualTo(1));
        }

        [Test] public void FirstChapterHasAnActualPowerWallBeforeTheFinalBoss()
        {
            var boss=EarlyRetentionRules.Stage(25);
            Assert.That(boss.Name,Is.EqualTo("Vigía de las Cenizas"));
            var starter=SliceRules.Expedition(new ArmyRoster{ArcherT1=36},"aldric");
            var developed=SliceRules.Expedition(new ArmyRoster{ArcherT1=100},"aldric");
            Assert.That(SliceRules.FightCreature(starter,boss.Enemy).Won,Is.False,
                "Starter archers must not trivialize all twenty-five stages");
            Assert.That(SliceRules.FightCreature(developed,boss.Enemy).Won,Is.True,
                "Investing in an army must allow the player to surmount the chapter boss");
        }

        [Test] public void AccumulatedHeroXpImprovesGenuineEarlyPveButHasSafeCap()
        {
            var state=Ready();
            var original=SliceRules.Expedition(state.Available,"aldric");
            Assert.That(EarlyRetentionRules.HeroMasteryLevel(state,"aldric"),Is.Zero);
            state.Retention.AldricExperience=480;
            var upgraded=EarlyRetentionRules.ExpeditionWithMastery(state,
                state.Available,"aldric");
            Assert.That(EarlyRetentionRules.HeroMasteryLevel(state,"aldric"),Is.EqualTo(6));
            Assert.That(upgraded.Attack,Is.EqualTo(original.Attack+24));
            Assert.That(upgraded.Defense,Is.EqualTo(original.Defense+18));
            Assert.That(upgraded.Power,Is.EqualTo(original.Power+270));
            Assert.That(SliceRules.Expedition(state.Available,"aldric").Power,
                Is.EqualTo(original.Power),"The historic battle engine must retain the old baseline");
            state.Retention.AldricExperience=999999;
            Assert.That(EarlyRetentionRules.HeroMasteryLevel(state,"aldric"),Is.EqualTo(20));
        }

        [Test] public void LegacyScoutClaimsKeepCountAndFamiliesAfterUtcCleanupAndReload()
        {
            var time=new Clock();var old=Ready();
            old.Retention.Orders=new System.Collections.Generic.List<ScoutOrder>{
                new ScoutOrder{Id="legacy-wood",Period=0,Kind=1,Claimed=true},
                new ScoutOrder{Id="legacy-breach",Period=0,Kind=3,Claimed=true}};
            var save=new MemoryStateStore{Value=old};
            var gateway=new LocalGateway(time,save);
            Assert.That(gateway.Snapshot().Retention.TotalScoutReports,Is.EqualTo(2));
            Assert.That(gateway.Snapshot().Retention.ScoutDiscoveryMask,Is.EqualTo(10));
            gateway=new LocalGateway(time,save);
            Assert.That(gateway.Snapshot().Retention.TotalScoutReports,Is.EqualTo(2));
            Assert.That(gateway.Snapshot().Retention.ScoutDiscoveryMask,Is.EqualTo(10));
            Assert.That(gateway.Snapshot().Retention.Orders.Any(x=>x.Id.StartsWith("legacy-")),Is.False);
        }

        [Test] public void SnapshotDoesNotExposeMutableOrderObjects()
        {
            var clock=new Clock();var save=new MemoryStateStore{Value=Ready()};
            var g=new LocalGateway(clock,save);
            var before=g.Snapshot().Retention.Orders[0].Id;
            var tampered=g.Snapshot();
            tampered.Retention.Orders[0].Id="forged";
            tampered.Retention.LastClearedStage=250;
            Assert.That(g.Snapshot().Retention.Orders[0].Id,Is.EqualTo(before));
            Assert.That(g.Snapshot().Retention.LastClearedStage,Is.Zero);
        }
    }
}
