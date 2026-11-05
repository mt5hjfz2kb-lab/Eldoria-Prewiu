using System;
using Eldoria.Domain;
using NUnit.Framework;

namespace Eldoria.Tests.EditMode
{
    public class EconomyProductionV2Tests
    {
        [Test]
        public void EightHoursOffline_AtSawmillThree()
        {
            long start = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
            var result = EconomyProductionV2.Accrue(start, start + TimeSpan.FromHours(8).Ticks,
                0, 3, "wood");
            Assert.AreEqual(10368, result.WholeUnits);
        }

        [Test]
        public void ReopeningAtSameTimestampCannotPayAgain()
        {
            long start = DateTime.UtcNow.Ticks;
            var first = EconomyProductionV2.Accrue(start, start + TimeSpan.FromMinutes(1).Ticks,
                0, 1, "wood");
            var second = EconomyProductionV2.Accrue(first.SettledThroughUtcTicks,
                first.SettledThroughUtcTicks, first.FractionalMicrounits, 1, "wood");
            Assert.AreEqual(0, second.WholeUnits);
        }

        [Test]
        public void UpgradeDuringAbsenceSplitsProductionAtCompletion()
        {
            long start = DateTime.UtcNow.Ticks;
            long middle = start + TimeSpan.FromHours(1).Ticks;
            var result = EconomyProductionV2.Accrue(start,
                start + TimeSpan.FromHours(2).Ticks, 0, 2, "wood", middle, 1);
            Assert.AreEqual(1224, result.WholeUnits);
        }

        [Test]
        public void OneSecondIsFractionalAndTenSecondsPayOneUnit()
        {
            long start = DateTime.UtcNow.Ticks;
            var first = EconomyProductionV2.Accrue(start, start + TimeSpan.FromSeconds(1).Ticks,
                0, 1, "wood");
            Assert.AreEqual(0, first.WholeUnits);
            var second = EconomyProductionV2.Accrue(first.SettledThroughUtcTicks,
                start + TimeSpan.FromSeconds(10).Ticks, first.FractionalMicrounits, 1, "wood");
            Assert.AreEqual(1, second.WholeUnits);
            Assert.AreEqual(200000, second.FractionalMicrounits);
        }

        [Test]
        public void ClockRollbackDoesNotMintResources()
        {
            long now = DateTime.UtcNow.Ticks;
            var result = EconomyProductionV2.Accrue(now, now - TimeSpan.FromHours(2).Ticks,
                0, 2, "stone");
            Assert.AreEqual(0, result.WholeUnits);
            Assert.AreEqual(now, result.SettledThroughUtcTicks);
        }

        [Test]
        public void HighBastionTimingsAreDefinedButNotAutomaticallyUnlocked()
        {
            Assert.IsTrue(EconomyBastionTimingV2.TryGetSeconds(10, out int seconds));
            Assert.AreEqual(28800, seconds);
            Assert.IsFalse(EconomyBastionTimingV2.TryGetSeconds(25, out _));
        }
    }
}
