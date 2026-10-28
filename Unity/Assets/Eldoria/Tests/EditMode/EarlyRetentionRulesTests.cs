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
            Assert.That(b.Name,Is.EqualTo("Guardián del umbral"));
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
        [Test] public void IdleExperienceAccruesOfflineWithEightHourCapAndNoDoubleClaim()
        {
            var time=new Clock();var store=new MemoryStateStore{Value=Ready()};
            var g=new LocalGateway(time,store);
            Assert.That(g.Execute(Cmd(g,"battle","VigilantBattle","1")).Ok,Is.True);
            time.AddSeconds(30*3600);
            g=new LocalGateway(time,store);
            var pending=EarlyRetentionRules.UnclaimedIdleXp(g.Snapshot().Retention,time.UtcTicks);
            Assert.That(pending,Is.EqualTo(8*12));
            var cmd=Cmd(g,"claim","ClaimVigilantXp","aldric");
            Assert.That(g.Execute(cmd).Ok,Is.True);
            var xp=g.Snapshot().Retention.AldricExperience;
            Assert.That(g.Execute(cmd).Ok,Is.True);
            Assert.That(g.Snapshot().Retention.AldricExperience,Is.EqualTo(xp));
            Assert.That(EarlyRetentionRules.UnclaimedIdleXp(g.Snapshot().Retention,time.UtcTicks),Is.Zero);
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
        [Test] public void FirstChapterHasAnActualPowerWallBeforeTheFinalBoss()
        {
            var boss=EarlyRetentionRules.Stage(25);
            Assert.That(boss.Name,Is.EqualTo("Guardián del umbral"));
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
