using System;
using Eldoria.Domain;
using Eldoria.Application;
using Eldoria.Infrastructure;
using NUnit.Framework;
using System.IO;

namespace Eldoria.Tests
{
    public sealed class SliceCoreTests
    {
        sealed class Clock:IClock { public long UtcTicks {get;set;}=new DateTime(2026,9,24,0,0,0,DateTimeKind.Utc).Ticks;
            public void Add(int seconds){UtcTicks+=TimeSpan.FromSeconds(seconds).Ticks;} }
        sealed class Memory:IStateStore { public PlayerState Value;
            public PlayerState Load()=>Value; public void Save(PlayerState state){Value=state;} }
        static GameCommand Cmd(LocalGateway g,string id,string kind,string target)
        {var s=g.Snapshot();return new GameCommand(id,s.PlayerId,kind,target,s.Revision);}
        [Test] public void ReferenceNumbersDeriveFromOwnershipAndSingleMarch()
        {
            var s=new PlayerState();var p=SliceRules.TotalPower(s);
            Assert.That(p.Kingdom,Is.EqualTo(600));Assert.That(p.Army,Is.EqualTo(648));
            Assert.That(p.Heroes,Is.EqualTo(1204));Assert.That(p.Total,Is.EqualTo(2452));
            var e=SliceRules.Expedition(s.Available,"aldric");
            Assert.That(new[]{e.Attack,e.Defense,e.Health,e.Break,e.Power},Is.EqualTo(new[]{198,184,642,100,2176}));
            Assert.That(SliceRules.Fight(e).Won,Is.True);
        }
        [Test] public void FullJourneySpendsOnceSurvivesOfflineAndChangesCity()
        {
            var clock=new Clock();var store=new Memory();var g=new LocalGateway(clock,store);
            Assert.That(g.Execute(Cmd(g,"build-early","Build","sawmill")).Ok,Is.False);
            var gather=Cmd(g,"forest-1","Gather","forest-valoria");
            Assert.That(g.Execute(gather).Ok,Is.True);
            Assert.That(g.Snapshot().Available.Total,Is.Zero);
            Assert.That(SliceRules.TotalPower(g.Snapshot()).Total,Is.EqualTo(2452));
            Assert.That(g.Execute(gather).Ok,Is.True); // idempotent retry
            clock.Add(9);
            g=new LocalGateway(clock,store); // offline completion in constructor
            Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(390));
            Assert.That(g.Snapshot().Available.Total,Is.EqualTo(36));
            Assert.That(g.Snapshot().ForestRemaining,Is.EqualTo(890));
            g.Advance();Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(390));
            Assert.That(g.Execute(Cmd(g,"sawmill-1","Build","sawmill")).Ok,Is.True);
            Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(310));
            clock.Add(6);g=new LocalGateway(clock,store);
            Assert.That(g.Snapshot().SawmillLevel,Is.EqualTo(1));
            Assert.That(SliceRules.TotalPower(g.Snapshot()).Total,Is.EqualTo(2622));
            Assert.That(g.Snapshot().JourneyComplete,Is.True);
        }
        [Test] public void CombatUsesReservedMarchAndRejectsStaleRevision()
        {
            var clock=new Clock();var g=new LocalGateway(clock,new Memory());
            var fight=Cmd(g,"scout-1","Fight","corrupt-scout");
            Assert.That(g.Execute(fight).Ok,Is.True);
            Assert.That(g.Execute(new GameCommand("stale","player-local","Build","sawmill",0)).Ok,Is.False);
            Assert.That(g.Snapshot().March.Troops.ArcherT1,Is.EqualTo(36));
            clock.Add(4);g.Advance();
            Assert.That(g.Snapshot().ScoutDefeated,Is.True);
            Assert.That(g.Snapshot().Available.Total,Is.EqualTo(36));
            Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(110));
            Assert.That(g.Snapshot().Resources.Stone,Is.EqualTo(220));
            Assert.That(g.Snapshot().Wounded.Total,Is.Zero);
        }

        [Test] public void BastionTwoBuildRecruitAndEngendroArePersistentAndIdempotent()
        {
            var clock=new Clock();var store=new Memory();var g=new LocalGateway(clock,store);
            Assert.That(g.Execute(Cmd(g,"forest-b2","Gather","forest-valoria")).Ok,Is.True);
            clock.Add(9);g.Advance();
            Assert.That(g.Execute(Cmd(g,"sawmill-b2","Build","sawmill")).Ok,Is.True);
            clock.Add(6);g.Advance();
            Assert.That(g.Snapshot().JourneyComplete,Is.True);

            var ascend=Cmd(g,"ascend-b2","AdvanceBastion","bastion");
            Assert.That(g.Execute(ascend).Ok,Is.True);
            Assert.That(g.Execute(ascend).Ok,Is.True);
            Assert.That(g.Snapshot().BastionLevel,Is.EqualTo(2));
            Assert.That(SliceRules.TotalPower(g.Snapshot()).Total,Is.EqualTo(3222));

            Assert.That(g.Execute(Cmd(g,"barracks-b2","Build","barracks")).Ok,Is.True);
            clock.Add(8);g=new LocalGateway(clock,store);
            Assert.That(g.Snapshot().BarracksLevel,Is.EqualTo(1));

            var recruit=Cmd(g,"recruit-b2","Recruit","archer:t1");
            Assert.That(g.Execute(recruit).Ok,Is.True);
            Assert.That(g.Execute(recruit).Ok,Is.True);
            clock.Add(7);g=new LocalGateway(clock,store);
            Assert.That(g.Snapshot().Available.ArcherT1,Is.EqualTo(48));
            Assert.That(SliceRules.TotalPower(g.Snapshot()).Total,Is.EqualTo(3628));

            Assert.That(g.Execute(Cmd(g,"engendro-b2","Fight","engendro-valoria")).Ok,Is.True);
            clock.Add(4);g=new LocalGateway(clock,store);
            Assert.That(g.Snapshot().EngendroDefeated,Is.True);
            Assert.That(g.Snapshot().Available.ArcherT1,Is.EqualTo(48));
            Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(240));
            Assert.That(g.Snapshot().Resources.Stone,Is.EqualTo(160));
        }

        [Test] public void SnapshotCannotEditAuthoritativeState()
        {
            var g=new LocalGateway(new Clock(),new Memory());var outside=g.Snapshot();
            outside.Resources.Wood=999;outside.Available.ArcherT1=999;outside.CompletedCommandIds.Add("fake");
            Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(30));
            Assert.That(g.Snapshot().Available.Total,Is.EqualTo(36));
            Assert.That(g.Snapshot().CompletedCommandIds,Is.Empty);
        }
        [Test] public void DiskSaveReloadDoesNotDuplicateDelayedReward()
        {
            var path=Path.Combine(Path.GetTempPath(),"eldoria-test-"+Guid.NewGuid().ToString("N")+".json");
            try
            {
                var clock=new Clock();var store=new FileStateStore(path);
                var first=new LocalGateway(clock,store);
                Assert.That(first.Execute(Cmd(first,"gather-disk","Gather","forest-valoria")).Ok,Is.True);
                clock.Add(9);
                var second=new LocalGateway(clock,store);
                Assert.That(second.Snapshot().Resources.Wood,Is.EqualTo(390));
                var third=new LocalGateway(clock,store);
                Assert.That(third.Snapshot().Resources.Wood,Is.EqualTo(390));
                Assert.That(third.Snapshot().Available.Total,Is.EqualTo(36));
                Assert.That(third.Snapshot().CompletedTaskIds.Count,Is.EqualTo(1));
            }
            finally
            {
                foreach(var candidate in new[]{path,path+".bak",path+".tmp"})if(File.Exists(candidate))File.Delete(candidate);
            }
        }
    }
}
