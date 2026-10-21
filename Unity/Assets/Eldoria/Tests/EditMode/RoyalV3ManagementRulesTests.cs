using System;
using Eldoria.Application;
using Eldoria.Domain;
using NUnit.Framework;

namespace Eldoria.Tests
{
    [NonParallelizable]
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
            Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(9000));
        }
    }
}
