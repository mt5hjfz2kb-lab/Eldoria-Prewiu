using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Eldoria.Domain;
using Eldoria.Application;

namespace Eldoria.Tests
{
    public sealed class EldoriaV41ContractsTests
    {
        sealed class Clock : IClock
        {
            public long UtcTicks {get;set;}=new DateTime(2026,10,30,8,0,0,DateTimeKind.Utc).Ticks;
            public void Advance(int secs)=>UtcTicks+=TimeSpan.FromSeconds(secs).Ticks;
        }
        sealed class Store : IStateStore
        {
            public PlayerState State;
            public PlayerState Load()=>State;
            public void Save(PlayerState value)=>State=value;
        }
        static CommandResult Run(LocalGateway g,string id,string kind,string target)
        {
            var s=g.Snapshot();
            return g.Execute(new GameCommand(id,s.PlayerId,kind,target,s.Revision));
        }
        [Test] public void PercentShareRetainsIntegerTotalAndStockLimits()
        {
            var selected=new Dictionary<string,int>{{"archer",80},{"paladin",80},{"warlock",40}};
            var stock=new Dictionary<string,int>{{"archer",150},{"paladin",150},{"warlock",150}};
            var result=EldoriaV41Planning.EditPercent("archer",20,200,200,selected,stock,
                new[]{"archer","paladin","warlock"});
            Assert.That(result["archer"],Is.EqualTo(40));
            Assert.That(result.Values.Sum(),Is.EqualTo(200));
            Assert.That(EldoriaV41Planning.Percentages(result).Values.Sum(),Is.EqualTo(100));
        }
        [Test] public void NarrativeLockAndHeroDuplicationCannotBeBypassed()
        {
            var party=new[]{"aldric",(string)null,(string)null};
            Assert.Throws<ArgumentException>(()=>
                EldoriaV41Planning.SelectHero(party,1,"lyra",new[]{"aldric"}));
            var team=EldoriaV41Planning.SelectHero(party,1,"lyra",new[]{"aldric","lyra"});
            Assert.Throws<ArgumentException>(()=>
                EldoriaV41Planning.SelectHero(team,2,"lyra",new[]{"aldric","lyra"}));
        }
        [Test] public void RealGatewayKeepsMailAfterBattleWithoutReclaimingReward()
        {
            var clock=new Clock();
            var store=new Store{State=new PlayerState{
                BastionLevel=2,BarracksLevel=1,SawmillLevel=1,
                Available=new ArmyRoster{ArcherT1=150},
                ChapterProgress=new ChapterProgressState{MarchConfirmed=true}
            }};
            var gateway=new LocalGateway(clock,store);
            Assert.That(Run(gateway,"m1","ConfigureMarch","march-main:40:0:0:aldric::").Ok,Is.True);
            Assert.That(Run(gateway,"m2","Fight","corrupt-scout").Ok,Is.True);
            clock.Advance(20);
            gateway=new LocalGateway(clock,store);
            var reports=gateway.Snapshot().Mailbox.List("informes");
            Assert.That(reports.Count,Is.EqualTo(1));
            Assert.That(reports[0].Battle.TargetId,Is.EqualTo("corrupt-scout"));
            int wood=gateway.Snapshot().Resources.Wood;
            Assert.That(Run(gateway,"m3","MailRead",reports[0].EventId).Ok,Is.True);
            Assert.That(gateway.Snapshot().Mailbox.Unread("informes"),Is.Zero);
            Assert.That(Run(gateway,"m4","MailDelete",reports[0].EventId).Ok,Is.True);
            gateway=new LocalGateway(clock,store);
            Assert.That(gateway.Snapshot().Mailbox.List("informes").Count,Is.Zero);
            Assert.That(gateway.Snapshot().Resources.Wood,Is.EqualTo(wood));
        }
        [Test] public void TierStrengthComparisonIsNotEqualizedAway()
        {
            var low=new EldoriaV41Combat.Force().Add(0,1,30).Add(1,1,40).Add(2,1,30);
            var mid=new EldoriaV41Combat.Force().Add(0,2,30).Add(1,2,40).Add(2,2,30);
            var high=new EldoriaV41Combat.Force().Add(0,3,30).Add(1,3,40).Add(2,3,30);
            Assert.That(EldoriaV41Combat.Duel(low,mid).Winner,Is.EqualTo(-1));
            Assert.That(EldoriaV41Combat.Duel(mid,high).Winner,Is.EqualTo(-1));
            Assert.That(EldoriaV41Combat.ProductionApproved,Is.False);
        }
    }
}
