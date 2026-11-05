using System;
using System.Linq;
using Eldoria.Application;
using Eldoria.Domain;
using NUnit.Framework;
using UnityEngine;

namespace Eldoria.Tests
{
    public sealed class EldoriaV41GatewayIntegrationTests
    {
        sealed class Clock:IClock { public long UtcTicks{get;set;}=new DateTime(2026,11,5,0,0,0,DateTimeKind.Utc).Ticks; }
        sealed class Store:IStateStore
        {
            public string Json;
            public PlayerState Load()=>JsonUtility.FromJson<PlayerState>(Json);
            public void Save(PlayerState state)=>Json=JsonUtility.ToJson(state);
        }
        static CommandResult Send(LocalGateway gateway,string id,string kind,string target)
        {var s=gateway.Snapshot();return gateway.Execute(new GameCommand(id,s.PlayerId,kind,target,s.Revision));}
        [TestCase(1,"victory")]
        [TestCase(40,"draw")]
        [TestCase(1000,"defeat")]
        public void NativeSaveResolvesFrozenBattleOnce(int opponents,string outcome)
        {
            var clock=new Clock();var store=new Store();
            store.Save(new PlayerState{BastionLevel=2,BarracksLevel=1,SawmillLevel=1,ChapterProgress=new ChapterProgressState{MarchConfirmed=true},Available=new ArmyRoster{ArcherT1=150}});
            // Fixture only; never registered as production campaign content.
            var resolver=new EldoriaV41GatewayResolver(new[]{new EldoriaV41BattleInput{
                RulesVersion=EldoriaV41CombatEvolution.RulesVersion,TargetId="corrupt-scout",
                EnemyTroops=new ArmyRoster{ArcherT1=opponents},EnemyHeroes=new[]{"aldric",null,null}}});
            var gateway=new LocalGateway(clock,store,resolver);
            Assert.That(Send(gateway,"prepare","ConfigureMarch","march-main:40:0:0:aldric::").Ok,Is.True);
            Assert.That(Send(gateway,"launch","Fight","corrupt-scout").Ok,Is.True);
            Assert.That(store.Load().March.VersionedBattle.OwnTroops.ArcherT1,Is.EqualTo(40));
            Assert.Throws<InvalidOperationException>(()=>new LocalGateway(clock,store));
            clock.UtcTicks+=TimeSpan.FromSeconds(30).Ticks;
            gateway=new LocalGateway(clock,store,new EldoriaV41GatewayResolver(Array.Empty<EldoriaV41BattleInput>()));
            var s=gateway.Snapshot();var mail=s.Mailbox.List("informes").Single();
            Assert.That(mail.Battle.EffectiveOutcome,Is.EqualTo(outcome));
            Assert.That(mail.Battle.OwnRoster.ArcherT1,Is.EqualTo(40));
            Assert.That(s.Available.ArcherT1,Is.EqualTo(150));
            Assert.That(s.March.Phase,Is.EqualTo("idle"));
            int wood=s.Resources.Wood;
            Assert.That(Send(gateway,"read","MailRead",mail.EventId).Ok,Is.True);
            Assert.That(Send(gateway,"delete","MailDelete",mail.EventId).Ok,Is.True);
            gateway=new LocalGateway(clock,store,resolver);
            Assert.That(gateway.Snapshot().Mailbox.List("informes"),Is.Empty);
            Assert.That(gateway.Snapshot().Resources.Wood,Is.EqualTo(wood));
            if(outcome=="draw")Assert.That(mail.Title,Does.Contain("Empate"));
        }
        [Test] public void HistoricalReportsRetainOutcomeWithoutNewFields()
        {
            Assert.That(JsonUtility.FromJson<EldoriaV41CombatRecord>("{\"Won\":true}").EffectiveOutcome,Is.EqualTo("victory"));
            Assert.That(JsonUtility.FromJson<EldoriaV41CombatRecord>("{\"Won\":false}").EffectiveOutcome,Is.EqualTo("defeat"));
            var draw=new EldoriaV41CombatRecord{Outcome="draw"};
            Assert.That(JsonUtility.FromJson<EldoriaV41CombatRecord>(JsonUtility.ToJson(draw)).EffectiveOutcome,Is.EqualTo("draw"));
        }
    }
}
