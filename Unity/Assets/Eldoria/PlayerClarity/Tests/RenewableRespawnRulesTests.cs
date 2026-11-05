using System;
using Eldoria.PlayerClarity;
using NUnit.Framework;
namespace Eldoria.PlayerClarity.Tests
{
    public sealed class RenewableRespawnRulesTests
    {
        [Test]
        public void FiveMinutesAndOfflineTiming()
        {
            var due = new DateTime(2026, 11, 5, 18, 0, 0, DateTimeKind.Utc).Ticks;
            Assert.IsFalse(RenewableRespawnRules.CanRespawn(due, due + TimeSpan.FromSeconds(299).Ticks));
            Assert.IsTrue(RenewableRespawnRules.CanRespawn(due, due + TimeSpan.FromSeconds(300).Ticks));
            Assert.IsTrue(RenewableRespawnRules.CanRespawn(due, due + TimeSpan.FromMinutes(20).Ticks));
        }
        [TestCase(15)]
        [TestCase(30)]
        public void LegacyCooldownDoesNotRestartOnLogin(int minutes)
        {
            const long eventTicks = 638900000000000000L;
            var oldDue = eventTicks + TimeSpan.FromMinutes(minutes).Ticks;
            Assert.AreEqual(eventTicks + TimeSpan.FromMinutes(5).Ticks,
                RenewableRespawnRules.MigrateExistingDue(eventTicks, oldDue));
        }
        [Test]
        public void AlreadyShorterAndUnrecordedCooldownsRemainUnchanged()
        {
            const long ticks=638900000000000000L;
            Assert.AreEqual(123L, RenewableRespawnRules.MigrateExistingDue(0L,123L));
            Assert.AreEqual(0L, RenewableRespawnRules.MigrateExistingDue(ticks,0L));
            var early=ticks+TimeSpan.FromMinutes(2).Ticks;
            Assert.AreEqual(early, RenewableRespawnRules.MigrateExistingDue(ticks,early));
        }
    }
}
