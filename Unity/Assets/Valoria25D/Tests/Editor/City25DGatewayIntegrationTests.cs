#if UNITY_EDITOR
using System;
using Eldoria.Application;
using Eldoria.Domain;
using NUnit.Framework;
using UnityEngine;

namespace Eldoria.Valoria25D.Tests
{
    /// <summary>
    /// Exercises the UNMODIFIED main LocalGateway using a JSON store and an
    /// advancing fake UTC clock. Seeded milestones are explicitly QA fixtures,
    /// not client-side cheats, altered rules or user saves.
    /// </summary>
    public sealed class City25DGatewayIntegrationTests
    {
        sealed class Clock : IClock
        {
            public long UtcTicks { get; private set; } =
                new DateTime(2026, 11, 17, 12, 0, 0, DateTimeKind.Utc).Ticks;
            public void NextSeconds(int seconds)
            {
                UtcTicks+=TimeSpan.FromSeconds(seconds).Ticks;
            }
        }
        sealed class Store : IStateStore
        {
            public PlayerState Initial;
            private string json;
            public PlayerState Load() =>
                json==null?Initial:JsonUtility.FromJson<PlayerState>(json);
            public void Save(PlayerState state)
            {
                json=JsonUtility.ToJson(state);
            }
        }
        private string prior;
        [SetUp] public void Setup()
        {
            prior=SliceContentProfiles.ActiveRuntimeProfile;
            SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
        }
        [TearDown] public void Cleanup() =>
            SliceContentProfiles.SetRuntimeProfileOverride(prior);
        static PlayerState Seed(int bastion, int sawmill=0, int barracks=0)
        {
            return new PlayerState
            {
                BastionLevel=bastion,
                SawmillLevel=sawmill,
                BarracksLevel=barracks,
                JourneyComplete=true,
                EngendroDefeated=true,
                Available=new ArmyRoster { ArcherT1=70 },
                Resources=new ResourceWallet { Wood=100000,Stone=100000,Food=100000 },
                ChapterThree=new BastionThreeState()
            };
        }
        static CommandResult Do(LocalGateway g,string kind,string id,string key=null)
        {
            var s=g.Snapshot();
            return g.Execute(new GameCommand(key??Guid.NewGuid().ToString("N"),
                s.PlayerId,kind,id,s.Revision));
        }

        [Test] public void Sawmill_RealCostTimerCompletionAndReload()
        {
            var clock=new Clock();var store=new Store { Initial=Seed(1) };
            var gateway=new LocalGateway(clock,store);
            var state=gateway.Snapshot();
            int wood=state.Resources.Wood;
            var result=Do(gateway,"Build","sawmill","valoria25d-sawmill");
            Assert.That(result.Ok,Is.True,result.Message);
            Assert.That(gateway.Snapshot().Resources.Wood,Is.EqualTo(wood-SliceRules.SawmillWoodCost));
            Assert.That(gateway.Snapshot().SawmillLevel,Is.EqualTo(0));
            Assert.That(gateway.Snapshot().BuildingTaskId,
                Does.StartWith("sawmill:"));
            Assert.That(gateway.Snapshot().BuildingCompletesUtcTicks,
                Is.GreaterThan(clock.UtcTicks));
            // Reopen before completion, then settle once after the real deadline.
            gateway=new LocalGateway(clock,store);
            Assert.That(gateway.Snapshot().SawmillLevel,Is.Zero);
            clock.NextSeconds(SliceRules.SawmillBuildSeconds+2);
            gateway=new LocalGateway(clock,store);
            Assert.That(gateway.Snapshot().SawmillLevel,Is.EqualTo(1));
            Assert.That(gateway.Snapshot().BuildingCompletesUtcTicks,Is.Zero);
            Assert.That(gateway.Snapshot().Resources.Wood,Is.EqualTo(wood-SliceRules.SawmillWoodCost));
        }
        [Test] public void InsufficientResources_CanonicalCommandDoesNotSpend()
        {
            var clock=new Clock();var seed=Seed(1);
            seed.Resources.Wood=0;
            var store=new Store { Initial=seed };
            var g=new LocalGateway(clock,store);
            var before=g.Snapshot();
            var result=Do(g,"Build","sawmill");
            Assert.That(result.Ok,Is.False);
            var after=g.Snapshot();
            Assert.That(after.Resources.Wood,Is.EqualTo(before.Resources.Wood));
            Assert.That(after.SawmillLevel,Is.Zero);
            Assert.That(after.BuildingTaskId,Is.Empty);
        }
        [Test] public void Bastion_UnauthorizedAscensionBlockedThenRealTimerPersists()
        {
            var clock=new Clock();var seed=Seed(1);
            seed.JourneyComplete=false;seed.ScoutDefeated=false;
            var blocked=new LocalGateway(clock,new Store { Initial=seed });
            Assert.That(Do(blocked,"AdvanceBastion","bastion").Ok,Is.False,
                "Never bypass Chapter I world milestones with cosmetic buttons");
            var store=new Store { Initial=Seed(1) };
            var g=new LocalGateway(clock,store);
            var cost=BastionProgressionCatalog.ForLevel(2);
            int startWood=g.Snapshot().Resources.Wood;
            Assert.That(Do(g,"AdvanceBastion","bastion").Ok,Is.True);
            Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(startWood-cost.WoodCost));
            Assert.That(g.Snapshot().BastionLevel,Is.EqualTo(1));
            Assert.That(g.Snapshot().BastionUpgradeTaskId,Does.StartWith("bastion2:"));
            clock.NextSeconds(cost.DurationSeconds+1);
            g=new LocalGateway(clock,store);
            Assert.That(g.Snapshot().BastionLevel,Is.EqualTo(2));
            Assert.That(g.Snapshot().BastionUpgradeCompletesUtcTicks,Is.Zero);
        }
        [Test] public void BarracksAndArchers_UseRealTimersAndOneAcceptedRecruitment()
        {
            var clock=new Clock();var store=new Store { Initial=Seed(2,1,0) };
            var g=new LocalGateway(clock,store);
            Assert.That(Do(g,"Build","barracks").Ok,Is.True);
            var built=g.Snapshot();
            Assert.That(built.BuildingTaskId,Does.StartWith("barracks:"));
            clock.NextSeconds(SliceRules.BarracksBuildSeconds+1);
            g=new LocalGateway(clock,store);
            Assert.That(g.Snapshot().BarracksLevel,Is.EqualTo(1));
            int archers=g.Snapshot().Available.ArcherT1;
            int beforeWood=g.Snapshot().Resources.Wood;
            Assert.That(Do(g,"Recruit","archer:t1","recruit01").Ok,Is.True);
            Assert.That(g.Snapshot().Resources.Wood,
                Is.EqualTo(beforeWood-SliceRules.RecruitWoodCostFor(SliceRules.RecruitArchers)));
            Assert.That(Do(g,"Recruit","archer:t1","recruit02").Ok,Is.False,
                "A second action cannot double-spend or duplicate the active batch");
            clock.NextSeconds(SliceRules.RecruitSecondsFor(SliceRules.RecruitArchers)+1);
            g=new LocalGateway(clock,store);
            Assert.That(g.Snapshot().Available.ArcherT1,
                Is.EqualTo(archers+SliceRules.RecruitArchers));
            Assert.That(g.Snapshot().RecruitmentTaskId,Is.Empty);
        }
        [Test] public void GranaryAndQuarry_AuthoritativeChapterGatesAndProduction()
        {
            var clock=new Clock();var low=new LocalGateway(clock,new Store { Initial=Seed(3,1,1) });
            Assert.That(Do(low,"Build","stoneworks").Ok,Is.False);
            var store=new Store { Initial=Seed(3,1,1) };
            var g=new LocalGateway(clock,store);
            Assert.That(Do(g,"Build","granary").Ok,Is.True);
            clock.NextSeconds(11);g=new LocalGateway(clock,store);
            Assert.That(g.Snapshot().ChapterThree.GranaryLevel,Is.EqualTo(1));
            var before=g.Snapshot().Resources.Food;
            clock.NextSeconds(180);
            g=new LocalGateway(clock,store);
            Assert.That(g.Snapshot().Resources.Food,Is.GreaterThan(before));
            // Bastion V state in this QA fixture follows the SAME gateway commands.
            var highStore=new Store { Initial=Seed(5,2,2) };
            var high=new LocalGateway(clock,highStore);
            Assert.That(Do(high,"Build","stoneworks").Ok,Is.True);
            Assert.That(high.Snapshot().BuildingTaskId,Does.StartWith("c3:stoneworks:"));
            clock.NextSeconds(14);
            high=new LocalGateway(clock,highStore);
            Assert.That(high.Snapshot().ChapterThree.StoneworksLevel,Is.EqualTo(1));
        }
        [Test] public void Replay_CompletedCommandCannotDoubleSpendOrGrant()
        {
            var clock=new Clock();var store=new Store { Initial=Seed(1) };
            var g=new LocalGateway(clock,store);
            int wood=g.Snapshot().Resources.Wood;
            Assert.That(Do(g,"Build","sawmill","one").Ok,Is.True);
            var before=g.Snapshot().Resources.Wood;
            Assert.That(Do(g,"Build","sawmill","one").Ok,Is.True);
            Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(before));
            Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(wood-SliceRules.SawmillWoodCost));
        }

        [Test] public void MarchPreparation_CanonicalCommandUsesExistingTroopsAndSave()
        {
            var clock=new Clock();
            var store=new Store { Initial=Seed(2,1,1) };
            var game=new LocalGateway(clock,store);
            Assert.That(game.Snapshot().MarchConfigured,Is.False);
            var command=Do(game,"ConfigureMarch","march-main","march-qa-real");
            Assert.That(command.Ok,Is.True,command.Message);
            var state=game.Snapshot();
            Assert.That(state.MarchConfigured,Is.True);
            Assert.That(state.PreparedTroops.Total,Is.GreaterThan(0));
            Assert.That(state.PreparedTroops.Total,Is.LessThanOrEqualTo(state.Available.Total));
            Assert.That(state.ChapterProgress.MarchConfirmed,Is.True);
            game=new LocalGateway(clock,store);
            Assert.That(game.Snapshot().PreparedTroops.Total,Is.EqualTo(state.PreparedTroops.Total));
            Assert.That(game.Snapshot().MarchConfigured,Is.True);
        }

        [Test] public void ChapterThree_FissureInvestigationAndLyraUseAuthoritativeUTC()
        {
            var clock=new Clock();var store=new Store { Initial=Seed(3,1,1) };
            var game=new LocalGateway(clock,store);
            Assert.That(Do(game,"ContactLyra","fissure-valoria").Ok,Is.False,
                "No UI may unlock Lyra before the real investigation");
            var start=Do(game,"InvestigateFissure","fissure-valoria","fissure-qa");
            Assert.That(start.Ok,Is.True,start.Message);
            var pending=game.Snapshot();
            Assert.That(pending.ChapterThree.FissureExplored,Is.False);
            Assert.That(pending.ChapterThree.InvestigationEndsUtcTicks,
                Is.EqualTo(clock.UtcTicks+TimeSpan.FromMinutes(20).Ticks));
            game=new LocalGateway(clock,store);
            Assert.That(game.Snapshot().ChapterThree.InvestigationEndsUtcTicks,
                Is.EqualTo(pending.ChapterThree.InvestigationEndsUtcTicks));
            clock.NextSeconds(1201);
            game=new LocalGateway(clock,store);
            Assert.That(game.Snapshot().ChapterThree.FissureExplored,Is.True);
            Assert.That(game.Snapshot().ChapterThree.InvestigationEndsUtcTicks,Is.Zero);
            Assert.That(Do(game,"ContactLyra","fissure-valoria").Ok,Is.True);
            game=new LocalGateway(clock,store);
            Assert.That(game.Snapshot().ChapterThree.LyraContacted,Is.True);
        }

        [Test] public void BastionFourToFive_UsesIrreversibleDevelopmentChoiceAndRealDeadline()
        {
            var clock=new Clock();var seed=Seed(4,2,2);
            seed.ChapterThree.GranaryLevel=2;
            var store=new Store { Initial=seed };
            var game=new LocalGateway(clock,store);
            Assert.That(Do(game,"AdvanceBastion","bastion").Ok,Is.False,
                "Cannot skip the real chapter IV development choice");
            var choice=Do(game,"ChooseDevelopment","production","priority-real");
            Assert.That(choice.Ok,Is.True,choice.Message);
            Assert.That(game.Snapshot().ChapterThree.DevelopmentPriority,
                Is.EqualTo("production"));
            Assert.That(Do(game,"ChooseDevelopment","defense").Ok,Is.False,
                "A second UI press cannot replace or regrant a priority bonus");
            game=new LocalGateway(clock,store);
            Assert.That(game.Snapshot().ChapterThree.DevelopmentPriority,
                Is.EqualTo("production"));
            var next=BastionProgressionCatalog.ForLevel(5);
            Assert.That(BastionProgressionCatalog.RequirementsMet(game.Snapshot(),next),
                Is.True,BastionProgressionCatalog.MissingSummary(game.Snapshot(),next));
            Assert.That(Do(game,"AdvanceBastion","bastion").Ok,Is.True);
            var running=game.Snapshot();
            Assert.That(running.BastionLevel,Is.EqualTo(4));
            Assert.That(running.BastionUpgradeCompletesUtcTicks,
                Is.EqualTo(clock.UtcTicks+TimeSpan.FromSeconds(next.DurationSeconds).Ticks));
            clock.NextSeconds(next.DurationSeconds+1);
            game=new LocalGateway(clock,store);
            Assert.That(game.Snapshot().BastionLevel,Is.EqualTo(5));
            Assert.That(game.Snapshot().BastionUpgradeCompletesUtcTicks,Is.Zero);
            Assert.That(game.Snapshot().ChapterThree.DevelopmentPriority,
                Is.EqualTo("production"));
        }
    }
}
#endif
