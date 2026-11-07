using System;
using System.Linq;
using Eldoria.Domain;
using NUnit.Framework;

namespace Eldoria.Tests
{
    /// <summary>Native Unity EditMode gates for the additive V4.1 T1-T5 candidate.
    /// The authoritative gateway, saves, casualty/upgrade ledger and PvE/PvP rollout
    /// require separate PlayMode tests before enabling production.</summary>
    public sealed class EldoriaV41EvolutionIntegrationTests
    {
        [Test] public void HistoricalSaveFieldsSurviveAdditiveRosterMigration()
        {
            var old=UnityEngine.JsonUtility.FromJson<ArmyRoster>("{\"ArcherT1\":12,\"ArcherT2\":8,\"ArcherT3\":4}");
            Assert.That(old.Total,Is.EqualTo(24));
            Assert.That(old.PaladinT5,Is.Zero);
            Assert.That(old.WarlockT1,Is.Zero);
            for(int f=0;f<3;f++)for(int t=1;t<=5;t++)old.Set(f,t,f*5+t);
            var restored=UnityEngine.JsonUtility.FromJson<ArmyRoster>(UnityEngine.JsonUtility.ToJson(old));
            for(int f=0;f<3;f++)for(int t=1;t<=5;t++)Assert.That(restored.Get(f,t),Is.EqualTo(old.Get(f,t)));
            var state=new PlayerState{Available=restored};
            var selected=MarchCapacityRules.AutoSelect(restored,40);
            int before=state.Available.Total;
            MarchCapacityRules.Reserve(state,selected);
            Assert.That(state.Available.Total+selected.Total,Is.EqualTo(before));
            MarchCapacityRules.Return(state,selected);
            Assert.That(state.Available.Total,Is.EqualTo(before));
        }
        static EldoriaV41CombatEvolution.Force Force(int family,int tier,int count)
            => new EldoriaV41CombatEvolution.Force().Add(family,tier,count);

        [Test] public void ExperimentalResolverNeverChangesProductionAuthority()
        {
            Assert.That(EldoriaV41Combat.ProductionApproved,Is.False);
            Assert.That(EldoriaV41CombatEvolution.ProductionApproved,Is.False);
            Assert.That(EldoriaV41CombatEvolution.RulesVersion,Does.StartWith("v4.1-"));
        }
        [Test] public void SuperiorNumberCanBeatAHighTierSingleUnit()
        {
            for(int f=0;f<3;f++)for(int e=0;e<3;e++)
            {
                var low=EldoriaV41CombatEvolution.Resolve(Force(f,1,1000),Force(e,5,1));
                Assert.That(low.Winner,Is.EqualTo(1),"1000 T1 vs one T5: "+f+" / "+e);
            }
        }
        [Test] public void SameCompositionUpgradeIsValuableButNeverAutomatic()
        {
            for(int t=1;t<=4;t++)for(int f=0;f<3;f++)
            {
                var low=Force(f,t,100);var high=Force(f,t+1,100);
                Assert.That(EldoriaV41CombatEvolution.Resolve(low,high).Winner,
                    Is.EqualTo(-1),"Equal family and number, tier "+t);
            }
        }
        [Test] public void HistoricalCounterCycleIsPreservedAtEveryTier()
        {
            for(int t=1;t<=5;t++)foreach(var pair in new[]
                { new[]{0,1},new[]{1,2},new[]{2,0} })
            {
                var r=EldoriaV41CombatEvolution.Resolve(Force(pair[0],t,100),Force(pair[1],t,100));
                Assert.That(r.Winner,Is.EqualTo(1),"Counter cycle tier "+t);
            }
        }
        [Test] public void RuptureContinuesToImprovePastT3()
        {
            double p3=EldoriaV41CombatEvolution.RuptureFraction(17*2.40);
            double p4=EldoriaV41CombatEvolution.RuptureFraction(17*3.72);
            double p5=EldoriaV41CombatEvolution.RuptureFraction(17*5.77);
            Assert.That(p4,Is.GreaterThan(p3));Assert.That(p5,Is.GreaterThan(p4));
            Assert.That(p5,Is.LessThan(1.0));
        }
        [Test] public void RoundLimitProducesActualDrawNotInventedVictory()
        {
            var a=Force(0,5,100);
            var result=EldoriaV41CombatEvolution.Resolve(a,Force(0,5,100),1);
            Assert.That(result.Winner,Is.Zero);
            Assert.That(result.ReasonCode,Is.EqualTo("round_limit"));
        }
        [Test] public void HeroesAreRealAndCannotBeDuplicated()
        {
            Assert.Throws<ArgumentException>(()=>EldoriaV41CombatEvolution.Resolve(
                Force(0,1,30).WithHeroes("aldric","aldric"),Force(1,1,30)));
            Assert.Throws<ArgumentException>(()=>EldoriaV41CombatEvolution.Resolve(
                Force(0,1,30).WithHeroes("aldric","lyra","unreleased"),Force(1,1,30)));
            Assert.That(EldoriaV41CombatEvolution.Resolve(
                Force(0,1,30).WithHeroes("aldric","lyra"),Force(1,1,30)).Rounds,
                Is.GreaterThan(0));
        }
        [Test] public void AddingLyraCannotEraseAldricsEarnedGuardian25Victory()
        {
            var state=new PlayerState();
            state.Retention.AldricExperience=560;
            state.Retention.LyraExperience=0;
            state.ChapterThree.LyraRecruited=true;
            int soloMastery=EarlyRetentionRules.HeroMasteryLevel(state,"aldric");
            int pairedMastery=EarlyRetentionRules.HeroMasteryLevel(state,"aldric+lyra");
            Assert.That(soloMastery,Is.EqualTo(7));
            Assert.That(pairedMastery,Is.EqualTo(soloMastery));

            var enemy=EarlyRetentionRules.Stage(25).Enemy;
            var alone=EldoriaV41CombatEvolution.ResolveCreature(
                Force(1,2,36),enemy,masteryLevel:soloMastery);
            var withLyra=EldoriaV41CombatEvolution.ResolveCreature(
                Force(1,2,36).WithHeroes("aldric","lyra"),enemy,
                masteryLevel:pairedMastery);

            Assert.That(alone.Winner,Is.EqualTo(1),"Aldric's existing earned victory is the control");
            Assert.That(withLyra.Winner,Is.EqualTo(1),
                "Adding an unlocked support hero must not invert the same Guardian 25 victory");
            Assert.That(withLyra.RemainingA,Is.GreaterThanOrEqualTo(alone.RemainingA),
                "Mastery multipliers must not shrink merely because Lyra joined");
        }

        [Test] public void MailboxAnalysisNamesOnlyFamiliesActuallySent()
        {
            var labels=new[]{"Paladines","Arqueros","Brujos"};
            for(int family=0;family<3;family++)
            {
                var player=Force(family,1,40);var enemy=Force(1,1,1);
                var result=EldoriaV41CombatEvolution.Resolve(player,enemy);
                var report=EldoriaV41CombatEvolution.ToMailbox(result,player,enemy,
                    "own-family:"+family,"corrupt-scout",637000000000000000L).Battle;
                foreach(var facts in new[]{report.OwnDamageByFamily,report.OwnReceivedByFamily,report.OwnRemainingByFamily})
                {
                    Assert.That(facts,Does.Contain(labels[family]));
                    for(int absent=0;absent<3;absent++)if(absent!=family)
                        Assert.That(facts,Does.Not.Contain(labels[absent]));
                }
            }
        }
        [Test] public void MailboxPreservesV41SimpleAndAdvancedWithoutSpoilersOrPayment()
        {
            var a=new EldoriaV41CombatEvolution.Force().Add(0,3,40).Add(1,3,35).Add(2,3,25);
            var enemy=Force(1,3,100);
            var battle=EldoriaV41CombatEvolution.Resolve(a,enemy);
            var mailbox=new EldoriaV41Mailbox();
            const long when=637000000000000000L;
            var msg=EldoriaV41CombatEvolution.ToMailbox(battle,a,enemy,
                "v41:test:event","vigilant:25",when);
            Assert.That(msg.Battle.Outcome,Is.EqualTo("victory").Or.EqualTo("defeat").Or.EqualTo("draw"));
            Assert.That(msg.Battle.PlayerComposition,Does.Contain("Paladines"));
            Assert.That(msg.Battle.OwnDamageByFamily,Does.Contain("Arqueros"));
            Assert.That(msg.Battle.OwnReceivedByFamily,Does.Contain("Brujos"));
            Assert.That(msg.Battle.OwnRemainingByFamily,Does.Contain("Paladines"));
            Assert.That(msg.Battle.Reason,Does.Not.Contain("Counter"));
            Assert.That(msg.Battle.Reason,Does.Not.Contain("usa brujos"));
            Assert.That(msg.Battle.RewardWood,Is.Zero);
            Assert.That(msg.Battle.RewardStone,Is.Zero);
            Assert.That(mailbox.Receive(msg),Is.True);
            Assert.That(mailbox.Receive(msg),Is.False);
            var restored=mailbox.Copy();
            Assert.That(restored.Messages.Count,Is.EqualTo(1));
            Assert.That(restored.Messages[0].Battle.OwnDamageByFamily,Is.EqualTo(msg.Battle.OwnDamageByFamily));
            restored.Delete(msg.EventId);
            Assert.That(restored.Receive(msg),Is.False,"Tombstone is permanent");
        }
    }
}
