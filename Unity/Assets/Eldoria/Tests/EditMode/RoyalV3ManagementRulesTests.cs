using System;
using Eldoria.Application;
using Eldoria.Domain;
using NUnit.Framework;

namespace Eldoria.Tests
{
    public sealed class RoyalV3ManagementRulesTests
    {
        sealed class Clock : IClock
        {
            public long UtcTicks { get; set; } = new DateTime(2026,10,21,12,0,0,DateTimeKind.Utc).Ticks;
            public void Add(int seconds) { UtcTicks += TimeSpan.FromSeconds(seconds).Ticks; }
        }
        sealed class Memory : IStateStore
        {
            public PlayerState State;
            public PlayerState Load() => State;
            public void Save(PlayerState state) { State=state; }
        }
        static GameCommand Command(LocalGateway gateway,string id,string kind,string target)
        {
            var s=gateway.Snapshot();
            return new GameCommand(id,s.PlayerId,kind,target,s.Revision);
        }
        static PlayerState Ready()
        {
            return new PlayerState
            {
                BastionLevel=2,BarracksLevel=1,
                Resources=new ResourceWallet{Wood=9000,Stone=9000},
                Available=new ArmyRoster{ArcherT1=36},
                // An already-built Bastion II/Cuartel has received these rewards in a real save.
                ChapterProgress=new ChapterProgressState{
                    GrantedRewards=new System.Collections.Generic.List<string>{
                        "c1-bastion","chapter-1","c2-barracks"}}
            };
        }
        [SetUp] public void OwnerProfile()
        {
            SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
        }
        [TearDown] public void RestoreProfile()
        {
            SliceContentProfiles.SetRuntimeProfileOverride(null);
        }
        [Test] public void AcceleratorsOnlyReducePaidCompatibleTasksOnce()
        {
            var clock=new Clock();var state=Ready();state.BastionLevel=4;
            var store=new Memory{State=state};var g=new LocalGateway(clock,store);
            Assert.That(g.Execute(Command(g,"no-task","UseSpeedup","training:1")).Ok,Is.False);
            Assert.That(g.Execute(Command(g,"begin-training","Recruit","archer:t1:10")).Ok);
            var before=g.Snapshot();
            Assert.That(before.ChapterProgress.SpeedupOneMinute,Is.GreaterThanOrEqualTo(1),
                "Web first-compatible-task accelerator was not granted");
            var use=Command(g,"consume-one-minute","UseSpeedup","training:1");
            Assert.That(g.Execute(use).Ok);
            var accelerated=g.Snapshot();
            Assert.That(accelerated.RecruitmentCompletesUtcTicks,
                Is.EqualTo(before.RecruitmentCompletesUtcTicks-TimeSpan.FromMinutes(1).Ticks));
            Assert.That(accelerated.ChapterProgress.SpeedupOneMinute,
                Is.EqualTo(before.ChapterProgress.SpeedupOneMinute-1));
            Assert.That(g.Execute(use).Ok,"Replaying an accepted speedup cannot spend twice");
            Assert.That(g.Snapshot().RecruitmentCompletesUtcTicks,
                Is.EqualTo(accelerated.RecruitmentCompletesUtcTicks));
            clock.Add(240);
            g=new LocalGateway(clock,store);
            Assert.That(g.Snapshot().Available.ArcherT1,Is.EqualTo(46));
            Assert.That(g.Snapshot().PendingRecruitArchers,Is.Zero);
            Assert.That(g.Execute(Command(g,"no-gather-speedup","UseSpeedup","gather:1")).Ok,Is.False);
        }

        [Test] public void ChapterThreeFourFiveWebGrantsAreRetainedExactlyOnce()
        {
            var clock=new Clock();var state=Ready();
            state.BastionLevel=5;state.SawmillLevel=2;state.BarracksLevel=2;
            state.ChapterThree.GranaryLevel=2;
            state.ChapterThree.StoneworksLevel=1;
            state.ChapterThree.Hunts=2;
            state.ChapterThree.FissureClosed=true;
            state.ChapterThree.DevelopmentPriority="production";
            state.Resources.Food=1200;
            state.Resources.Stone=1600;
            state.Available.ArcherT1=60;
            state.ChapterProgress.GrantedRewards.Add("c3-food");
            var memory=new Memory{State=state};
            var gateway=new LocalGateway(clock,memory);
            var first=gateway.Snapshot();
            foreach(var id in new[]{"c3-bastion","chapter-3","c4-sawmill2",
                "c4-granary2","c4-barracks2","c4-priority","c4-bastion","chapter-4",
                "c5-stoneworks","c5-stone","c5-army"})
                Assert.That(first.ChapterProgress.GrantedRewards,
                    Has.Exactly(1).EqualTo(id),"Web grant "+id+" must be recorded once");
            gateway.Advance();
            gateway=new LocalGateway(clock,memory);
            var second=gateway.Snapshot();
            Assert.That(second.Resources.Wood,Is.EqualTo(first.Resources.Wood));
            Assert.That(second.Resources.Stone,Is.EqualTo(first.Resources.Stone));
            Assert.That(second.Resources.Food,Is.EqualTo(first.Resources.Food));
            Assert.That(second.MissionPower,Is.EqualTo(first.MissionPower));
            Assert.That(second.ChapterProgress.GrantedRewards,
                Is.EquivalentTo(first.ChapterProgress.GrantedRewards));
            Assert.That(second.ChapterProgress.GrantedRewards,Does.Not.Contain("chapter-5"),
                "Cannot declare chapter V complete without its enemy and Bastion VI");
        }

        [Test] public void ArcherT2UnlocksAtBarracksFourAndSurvivesOfflineTraining()
        {
            var clock=new Clock();var state=Ready();
            state.BastionLevel=4;state.BarracksLevel=3;
            var memory=new Memory{State=state};var gateway=new LocalGateway(clock,memory);
            Assert.That(gateway.Execute(Command(gateway,"locked-t2","Recruit","archer:t2:10")).Ok,Is.False);
            // Test the actual Barracks IV unlock without changing the owned T1 stock.
            memory.State.BarracksLevel=4;
            gateway=new LocalGateway(clock,memory);
            var before=gateway.Snapshot();
            var start=Command(gateway,"t2-unique","Recruit","archer:t2:10");
            Assert.That(gateway.Execute(start).Ok,Is.True);
            Assert.That(gateway.Execute(start).Ok,Is.True,"Duplicate command is idempotent");
            Assert.That(gateway.Snapshot().PendingRecruitTier,Is.EqualTo(2));
            Assert.That(gateway.Snapshot().Resources.Wood,Is.EqualTo(before.Resources.Wood-200));
            Assert.That(gateway.Snapshot().Resources.Stone,Is.EqualTo(before.Resources.Stone-120));
            clock.Add(300);
            gateway=new LocalGateway(clock,memory);
            Assert.That(gateway.Snapshot().Available.ArcherT1,Is.EqualTo(36));
            Assert.That(gateway.Snapshot().Available.ArcherT2,Is.EqualTo(10));
            Assert.That(gateway.Snapshot().PendingRecruitArchers,Is.Zero);
            Assert.That(gateway.Execute(Command(gateway,"t2-march","ConfigureMarch","march-main:0:10:0:aldric")).Ok,Is.True);
            Assert.That(gateway.Snapshot().PreparedTroops.ArcherT2,Is.EqualTo(10));
        }

        [Test] public void RecruitmentIsProportionalAndCostsAreAuthoritative()
        {
            Assert.That(SliceRules.RecruitSecondsFor(10),Is.EqualTo(300));
            Assert.That(SliceRules.RecruitSecondsFor(20),Is.EqualTo(600));
            Assert.That(SliceRules.RecruitSecondsFor(30),Is.EqualTo(900));
            Assert.That(SliceRules.RecruitWoodCostFor(10),Is.EqualTo(200));
            Assert.That(SliceRules.RecruitWoodCostFor(20),Is.EqualTo(400));
            Assert.That(SliceRules.RecruitStoneCostFor(30),Is.EqualTo(360));
            var s=Ready();
            s.Resources.Wood=420;s.Resources.Stone=250;
            Assert.That(SliceRules.MaxAffordableArchers(s),Is.EqualTo(20));
        }
        [Test] public void TenArchersSurviveSaveReloadAndAreGrantedOnlyOnce()
        {
            var clock=new Clock();var store=new Memory{State=Ready()};
            var gateway=new LocalGateway(clock,store);
            var cmd=Command(gateway,"royal-v3:train-10","Recruit","archer:t1:10");
            var before=gateway.Snapshot();
            Assert.That(gateway.Execute(cmd).Ok,Is.True);
            var begun=gateway.Snapshot();
            Assert.That(begun.Resources.Wood,Is.EqualTo(before.Resources.Wood-200));
            Assert.That(begun.Resources.Stone,Is.EqualTo(before.Resources.Stone-120));
            Assert.That(begun.PendingRecruitArchers,Is.EqualTo(10));
            Assert.That(begun.RecruitmentCompletesUtcTicks-clock.UtcTicks,
                Is.EqualTo(TimeSpan.FromMinutes(5).Ticks));
            Assert.That(gateway.Execute(cmd).Ok,Is.True);
            Assert.That(gateway.Snapshot().Resources.Wood,Is.EqualTo(begun.Resources.Wood));
            clock.Add(299);
            gateway=new LocalGateway(clock,store);
            Assert.That(gateway.Snapshot().Available.ArcherT1,Is.EqualTo(36));
            Assert.That(gateway.Snapshot().PendingRecruitArchers,Is.EqualTo(10));
            clock.Add(1);
            gateway=new LocalGateway(clock,store);
            Assert.That(gateway.Snapshot().Available.ArcherT1,Is.EqualTo(46));
            Assert.That(gateway.Snapshot().PendingRecruitArchers,Is.Zero);
            Assert.That(gateway.Snapshot().RecruitmentCompletesUtcTicks,Is.Zero);
            Assert.That(gateway.Snapshot().ChapterProgress.TrainedArchers,Is.EqualTo(10));
            gateway.Advance();
            gateway=new LocalGateway(clock,store);
            Assert.That(gateway.Snapshot().Available.ArcherT1,Is.EqualTo(46));
            Assert.That(gateway.Snapshot().ChapterProgress.TrainedArchers,Is.EqualTo(10));
        }
        [Test] public void TrainingWorksAfterTutorialProgressCompleted()
        {
            var s=Ready();s.ChapterProgress.TrainedArchers=20;s.EngendroDefeated=true;
            var clock=new Clock();var g=new LocalGateway(clock,new Memory{State=s});
            Assert.That(g.Execute(Command(g,"after-chapter","Recruit","archer:t1:30")).Ok,Is.True);
            Assert.That(g.Snapshot().PendingRecruitArchers,Is.EqualTo(30));
            Assert.That(g.Snapshot().RecruitmentCompletesUtcTicks-clock.UtcTicks,
                Is.EqualTo(TimeSpan.FromMinutes(15).Ticks));
        }
        [Test] public void OverbudgetBusyMalformedAndStaleOrdersCannotSpend()
        {
            var s=Ready();s.Resources.Wood=200;s.Resources.Stone=120;
            var clock=new Clock();var g=new LocalGateway(clock,new Memory{State=s});
            Assert.That(g.Execute(Command(g,"too-many","Recruit","archer:t1:11")).Ok,Is.False);
            Assert.That(g.Execute(Command(g,"zero","Recruit","archer:t1:0")).Ok,Is.False);
            Assert.That(g.Execute(Command(g,"negative","Recruit","archer:t1:-2")).Ok,Is.False);
            Assert.That(g.Execute(Command(g,"malformed","Recruit","archer:t1:abc")).Ok,Is.False);
            Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(200));
            Assert.That(g.Execute(Command(g,"valid","Recruit","archer:t1:10")).Ok,Is.True);
            Assert.That(g.Execute(Command(g,"busy","Recruit","archer:t1:1")).Ok,Is.False);
            Assert.That(g.Execute(new GameCommand("stale",s.PlayerId,"Recruit","archer:t1:1",0)).Ok,Is.False);
            Assert.That(g.Snapshot().Resources.Wood,Is.Zero);
        }
        [Test] public void MarchSelectionUsesRealArchersAndAldricPower()
        {
            var clock=new Clock();var g=new LocalGateway(clock,new Memory{State=Ready()});
            Assert.That(g.Execute(Command(g,"march-7","ConfigureMarch","march-main:7")).Ok,Is.True);
            var s=g.Snapshot();
            Assert.That(s.PreparedHeroId,Is.EqualTo("aldric"));
            Assert.That(s.PreparedTroops.ArcherT1,Is.EqualTo(7));
            Assert.That(s.Available.ArcherT1,Is.EqualTo(36));
            Assert.That(s.ChapterProgress.ConfirmedExpeditionPower,
                Is.EqualTo(SliceRules.Expedition(s.PreparedTroops,"aldric").Power));
            Assert.That(g.Execute(Command(g,"march-over","ConfigureMarch","march-main:99")).Ok,Is.False);
            Assert.That(g.Execute(Command(g,"march-zero","ConfigureMarch","march-main:0")).Ok,Is.False);
            Assert.That(g.Snapshot().PreparedTroops.ArcherT1,Is.EqualTo(7));
        }
        [Test] public void ExistingSaveAndLegacyRecruitmentStayCompatible()
        {
            var s=Ready();s.RecruitmentTaskId="recruit:legacy";
            s.PendingRecruitArchers=20;
            var clock=new Clock();
            s.RecruitmentCompletesUtcTicks=clock.UtcTicks+TimeSpan.FromSeconds(7).Ticks;
            var store=new Memory{State=s};
            clock.Add(9);
            var g=new LocalGateway(clock,store);
            Assert.That(g.Snapshot().SchemaVersion,Is.EqualTo(1));
            Assert.That(g.Snapshot().Available.ArcherT1,Is.EqualTo(56));
            Assert.That(g.Snapshot().CompletedTaskIds,Does.Contain("recruit:legacy"));
            // OWNER_I_II awards its Chapter-II training mission once when the
            // old in-flight 20-archer task completes; this is NOT a second debit.
            Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(9220));
            g=new LocalGateway(clock,store);
            Assert.That(g.Snapshot().Available.ArcherT1,Is.EqualTo(56));
            Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(9220));
        }
    }
}
