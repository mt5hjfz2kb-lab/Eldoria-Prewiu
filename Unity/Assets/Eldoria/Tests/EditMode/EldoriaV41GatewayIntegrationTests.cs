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
            Assert.That(s.LastBattleReport.VersionedOutcome.EffectiveOutcome,Is.EqualTo(outcome));
            Assert.That(store.Load().LastBattleReport.VersionedOutcome.EffectiveOutcome,Is.EqualTo(outcome));
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
        [TestCase(false)]
        [TestCase(true)] public void UnityLegacyEmptyMarchPlaceholderRetainsOriginalResolver(bool newResolverInstalled)
        {
            var clock=new Clock();var store=new Store();
            store.Save(new PlayerState{BastionLevel=2,Available=new ArmyRoster{ArcherT1=100}});
            var game=new LocalGateway(clock,store);
            Assert.That(Send(game,"legacy-launch","Fight","corrupt-scout").Ok,Is.True);
            // Native serializer, including null nested class behavior.
            var resolver=newResolverInstalled?EldoriaV41GatewayResolver.Campaign():null;
            game=new LocalGateway(clock,store,resolver);
            Assert.That(game.Snapshot().March.VersionedBattle,Is.Null);
            clock.UtcTicks+=TimeSpan.FromSeconds(30).Ticks;
            game=new LocalGateway(clock,store,resolver);
            Assert.That(game.Snapshot().March.Phase,Is.EqualTo("idle"));
            Assert.That(game.Snapshot().Available.ArcherT1,Is.EqualTo(100));
        }
        [Test] public void RealCampaignCreatureSnapshotSurvivesUnitySerialization()
        {
            var clock=new Clock();var store=new Store();
            store.Save(new PlayerState{BastionLevel=2,Available=new ArmyRoster{ArcherT1=100}});
            var resolver=EldoriaV41GatewayResolver.Campaign();
            var game=new LocalGateway(clock,store,resolver);
            Assert.That(Send(game,"creature-launch","Fight","corrupt-scout").Ok,Is.True);
            var frozen=store.Load().March.VersionedBattle;
            Assert.That(frozen.EnemyCreature.Health,Is.EqualTo(620));
            clock.UtcTicks+=TimeSpan.FromSeconds(30).Ticks;
            game=new LocalGateway(clock,store,resolver);
            var battle=game.Snapshot().Mailbox.List("informes").Single().Battle;
            Assert.That(battle.Outcome,Is.EqualTo("victory"));
            Assert.That(battle.OwnDamageByFamily,Is.Not.Empty);
            Assert.That(game.Snapshot().Available.ArcherT1,Is.EqualTo(100));
        }
        [Test] public void NativeVigilantResultAndEarnedMasterySurviveReloadWithoutDoubleReward()
        {
            var clock=new Clock();var store=new Store();
            store.Save(new PlayerState{BastionLevel=2,BarracksLevel=5,SawmillLevel=1,EngendroDefeated=true,
                ChapterProgress=new ChapterProgressState{MarchConfirmed=true},Available=new ArmyRoster{ArcherT1=200}});
            var introduced=store.Load();
            introduced.ChapterProgress.SeenNarrative.Add("vigilants-introduction");
            store.Save(introduced);
            var resolver=EldoriaV41GatewayResolver.Campaign();
            var game=new LocalGateway(clock,store,resolver);
            Assert.That(Send(game,"guardian","VigilantBattle","1").Ok,Is.True);
            var before=game.Snapshot();
            Assert.That(before.Retention.LastClearedStage,Is.EqualTo(1));
            Assert.That(before.LastBattleReport.VersionedOutcome.Outcome,Is.EqualTo("victory"));
            game=new LocalGateway(clock,store,resolver);
            var mail=game.Snapshot().Mailbox.List("informes").Single();
            Assert.That(mail.Battle.OwnDamageByFamily,Is.Not.Empty);
            Assert.That(mail.Battle.RewardWood,Is.EqualTo(32));
            Assert.That(Send(game,"guardian","VigilantBattle","1").Ok,Is.True);
            Assert.That(game.Snapshot().Resources.Wood,Is.EqualTo(before.Resources.Wood));
            Assert.That(game.Snapshot().Retention.AldricExperience,Is.EqualTo(before.Retention.AldricExperience));
            Assert.That(game.Snapshot().Available.ArcherT1,Is.EqualTo(200));
        }
        [Test] public void ChapterThreeDrawKeepsTitleOutcomeAndNoRewardAfterNativeReload()
        {
            var clock=new Clock();var store=new Store();
            var input=new EldoriaV41BattleInput{RulesVersion=EldoriaV41CombatEvolution.RulesVersion,TargetId="ash-stalker-valoria",
                OwnTroops=new ArmyRoster{ArcherT1=40},EnemyTroops=new ArmyRoster{ArcherT1=40},
                OwnHeroes=new[]{"aldric",null,null},EnemyHeroes=new[]{"aldric",null,null}};
            store.Save(new PlayerState{BastionLevel=3,Available=new ArmyRoster{ArcherT1=100},
                March=new MarchState{TargetId=input.TargetId,HeroId="aldric",Troops=input.OwnTroops.Copy(),
                    Phase="outbound",PhaseEndsUtcTicks=clock.UtcTicks-TimeSpan.FromSeconds(30).Ticks,VersionedBattle=input}});
            var resolver=new EldoriaV41GatewayResolver(Array.Empty<EldoriaV41BattleInput>());
            var game=new LocalGateway(clock,store,resolver);var after=game.Snapshot();
            var mail=after.Mailbox.List("informes").Single();
            Assert.That(mail.Battle.Outcome,Is.EqualTo("draw"));
            Assert.That(mail.Title,Does.Contain("Empate"));
            Assert.That(mail.Battle.RewardWood+mail.Battle.RewardStone+mail.Battle.RewardFood,Is.Zero);
            Assert.That(after.ChapterThree.AshStalkerDefeated,Is.False);
            game=new LocalGateway(clock,store,resolver);after=game.Snapshot();
            Assert.That(after.LastBattleReport.VersionedOutcome.Outcome,Is.EqualTo("draw"));
            Assert.That(after.Mailbox.List("informes").Count,Is.EqualTo(1));
            Assert.That(after.Available.ArcherT1,Is.EqualTo(140));
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
