using System;
using Eldoria.Application;
using Eldoria.Domain;
using NUnit.Framework;
using UnityEngine;

namespace Eldoria.Tests.EditMode
{
    public class EconomyProductionV2GatewayTests
    {
        sealed class TestClock : IClock
        {
            public long UtcTicks { get; set; } = new DateTime(2026,11,5,0,0,0,DateTimeKind.Utc).Ticks;
            public void Advance(int seconds) { UtcTicks += TimeSpan.FromSeconds(seconds).Ticks; }
        }
        sealed class JsonStore : IStateStore
        {
            public PlayerState Seed;
            string saved;
            public PlayerState Load() => saved == null ? Seed : JsonUtility.FromJson<PlayerState>(saved);
            public void Save(PlayerState state) { saved = JsonUtility.ToJson(state); }
        }
        string previousProfile;
        [SetUp] public void SetUp()
        {
            previousProfile=SliceContentProfiles.ActiveRuntimeProfile;
            SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
        }
        [TearDown] public void TearDown() => SliceContentProfiles.SetRuntimeProfileOverride(previousProfile);
        static PlayerState Ready() => new PlayerState
        {
            BastionLevel=3, SawmillLevel=3, BarracksLevel=2,
            JourneyComplete=true, EngendroDefeated=true,
            Resources=new ResourceWallet { Wood=50000, Stone=50000, Food=50000 },
            ChapterThree=new BastionThreeState { GranaryLevel=2, StoneworksLevel=1 }
        };
        [Test]
        public void EightHoursOfAllThreeResourcesAreCreditedOnceAndSaved()
        {
            var clock=new TestClock();
            var store=new JsonStore { Seed=Ready() };
            var gateway=new LocalGateway(clock,store);
            var start=gateway.Snapshot().Resources;
            clock.Advance(8*3600);
            gateway=new LocalGateway(clock,store);
            var after=gateway.Snapshot().Resources;
            Assert.That(after.Wood-start.Wood,Is.EqualTo(10368));
            Assert.That(after.Stone-start.Stone,Is.EqualTo(2304));
            Assert.That(after.Food-start.Food,Is.EqualTo(5472));
            gateway=new LocalGateway(clock,store);
            Assert.That(gateway.Snapshot().Resources.Wood,Is.EqualTo(after.Wood));
            Assert.That(gateway.Snapshot().Resources.Stone,Is.EqualTo(after.Stone));
            Assert.That(gateway.Snapshot().Resources.Food,Is.EqualTo(after.Food));
        }
        [Test]
        public void GranaryUpgradeWhileOfflineUsesBothRatesAndDoesNotRepay()
        {
            var clock=new TestClock();
            var seed=Ready();seed.ChapterThree.GranaryLevel=1;
            var store=new JsonStore { Seed=seed };
            var gateway=new LocalGateway(clock,store);
            var snapshot=gateway.Snapshot();
            Assert.That(gateway.Execute(new GameCommand("economy-upgrade",snapshot.PlayerId,
                "UpgradeBuilding","granary",snapshot.Revision)).Ok,Is.True);
            var beforeFood=gateway.Snapshot().Resources.Food;
            clock.Advance(7200);
            gateway=new LocalGateway(clock,store);
            Assert.That(gateway.Snapshot().ChapterThree.GranaryLevel,Is.EqualTo(2));
            Assert.That(gateway.Snapshot().Resources.Food-beforeFood,Is.EqualTo(1367));
            gateway=new LocalGateway(clock,store);
            Assert.That(gateway.Snapshot().Resources.Food-beforeFood,Is.EqualTo(1367));
        }
        [Test]
        public void FractionalProductionSurvivesReload()
        {
            var clock=new TestClock();
            var seed=Ready();seed.SawmillLevel=1;
            var store=new JsonStore { Seed=seed };
            var gateway=new LocalGateway(clock,store);
            int initialWood=gateway.Snapshot().Resources.Wood;
            clock.Advance(1);gateway.Advance();
            Assert.That(gateway.Snapshot().Resources.Wood,Is.EqualTo(initialWood));
            Assert.That(gateway.Snapshot().ChapterThree.WoodProductionMicro,Is.EqualTo(120000));
            gateway=new LocalGateway(clock,store);
            clock.Advance(9);gateway.Advance();
            Assert.That(gateway.Snapshot().Resources.Wood,Is.EqualTo(initialWood+1));
            Assert.That(gateway.Snapshot().ChapterThree.WoodProductionMicro,Is.EqualTo(200000));
        }
        [Test]
        public void LegacyOneHourEntitlementMigratesExactlyOnceWithoutResettingTheWallet()
        {
            var clock=new TestClock();
            var old=Ready();old.SawmillLevel=1;old.ChapterThree.GranaryLevel=0;old.ChapterThree.StoneworksLevel=0;
            old.ChapterThree.EconomyVersion=0;
            old.ChapterThree.EconomyUtcTicks=clock.UtcTicks-TimeSpan.FromHours(2).Ticks;
            var current=Ready();current.SawmillLevel=1;current.ChapterThree.GranaryLevel=0;current.ChapterThree.StoneworksLevel=0;
            current.ChapterThree.EconomyVersion=2;current.ChapterThree.EconomyUtcTicks=clock.UtcTicks;
            var oldStore=new JsonStore { Seed=old };
            var currentStore=new JsonStore { Seed=current };
            var upgraded=new LocalGateway(clock,oldStore);
            var control=new LocalGateway(clock,currentStore);
            Assert.That(upgraded.Snapshot().Resources.Wood-control.Snapshot().Resources.Wood,Is.EqualTo(3600));
            Assert.That(upgraded.Snapshot().ChapterThree.EconomyVersion,Is.EqualTo(2));
            int settled=upgraded.Snapshot().Resources.Wood;
            upgraded=new LocalGateway(clock,oldStore);
            Assert.That(upgraded.Snapshot().Resources.Wood,Is.EqualTo(settled));
            clock.Advance(10);upgraded.Advance();
            Assert.That(upgraded.Snapshot().Resources.Wood,Is.EqualTo(settled+1));
            Assert.That(upgraded.Snapshot().ChapterThree.WoodProductionMicro,Is.EqualTo(200000));
        }
        [Test]
        public void BastionIIIUnlockedDuringLongOfflineIntervalPaysWoodInFirstReopen()
        {
            PlayerState Seed() => new PlayerState {
                BastionLevel=2,SawmillLevel=1,BarracksLevel=1,JourneyComplete=true,
                Available=new ArmyRoster { ArcherT1=56 },
                Resources=new ResourceWallet { Wood=50000,Stone=50000,Food=50000 }
            };
            var controlClock=new TestClock();
            var controlStore=new JsonStore { Seed=Seed() };
            var control=new LocalGateway(controlClock,controlStore);
            var ready=control.Snapshot();
            Assert.That(control.Execute(new GameCommand("ascend",ready.PlayerId,"AdvanceBastion","bastion",ready.Revision)).Ok,Is.True);
            controlClock.Advance(240);
            control=new LocalGateway(controlClock,controlStore);
            int woodAtAscension=control.Snapshot().Resources.Wood;
            var offlineClock=new TestClock();
            var offlineStore=new JsonStore { Seed=Seed() };
            var offline=new LocalGateway(offlineClock,offlineStore);
            ready=offline.Snapshot();
            Assert.That(offline.Execute(new GameCommand("ascend",ready.PlayerId,"AdvanceBastion","bastion",ready.Revision)).Ok,Is.True);
            offlineClock.Advance(240+3600);
            offline=new LocalGateway(offlineClock,offlineStore);
            Assert.That(offline.Snapshot().BastionLevel,Is.EqualTo(3));
            Assert.That(offline.Snapshot().Resources.Wood,Is.EqualTo(woodAtAscension+432));
            Assert.That(offline.Snapshot().ChapterThree.EconomyUtcTicks,Is.EqualTo(offlineClock.UtcTicks));
            int credited=offline.Snapshot().Resources.Wood;
            offline=new LocalGateway(offlineClock,offlineStore);
            Assert.That(offline.Snapshot().Resources.Wood,Is.EqualTo(credited));
        }
        [Test]
        public void FiveDaysAwayCreditsOnlyTwoDaysAndReopeningCannotPayAgain()
        {
            var clock = new TestClock();
            var store = new JsonStore { Seed=Ready() };
            var gateway = new LocalGateway(clock,store);
            var before = gateway.Snapshot().Resources;
            clock.Advance(5*24*3600);
            gateway = new LocalGateway(clock,store);
            var after = gateway.Snapshot().Resources;
            Assert.That(after.Wood-before.Wood,Is.EqualTo(62208));
            Assert.That(after.Stone-before.Stone,Is.EqualTo(13824));
            Assert.That(after.Food-before.Food,Is.EqualTo(32832));
            Assert.That(gateway.Snapshot().ChapterThree.EconomyUtcTicks,Is.EqualTo(clock.UtcTicks));
            gateway = new LocalGateway(clock,store);
            Assert.That(gateway.Snapshot().Resources.Wood,Is.EqualTo(after.Wood));
            Assert.That(gateway.Snapshot().Resources.Stone,Is.EqualTo(after.Stone));
            Assert.That(gateway.Snapshot().Resources.Food,Is.EqualTo(after.Food));
        }

        [Test]
        public void UpgradeCompletingDuringFiveDayAbsenceSharesSameFortyEightHourWindow()
        {
            var clock = new TestClock();
            var seed=Ready();
            seed.ChapterThree.GranaryLevel=1;
            var store = new JsonStore { Seed=seed };
            var gateway = new LocalGateway(clock,store);
            var beforeFood=gateway.Snapshot().Resources.Food;
            var state=gateway.Snapshot();
            Assert.That(gateway.Execute(new GameCommand("economy-long-upgrade",
                state.PlayerId,"UpgradeBuilding","granary",state.Revision)).Ok,Is.True);
            // Simulate a save with a delayed construction completion and preserved original ledger.
            // The 48-hour entitlement must remain based on the original timestamp.
            // Do not grant five days at the new rate merely because construction completes.
            clock.Advance(5*24*3600);
            gateway=new LocalGateway(clock,store);
            Assert.That(gateway.Snapshot().ChapterThree.GranaryLevel,Is.EqualTo(2));
            Assert.That(gateway.Snapshot().Resources.Food-beforeFood,
                Is.EqualTo(32831));
            // First 10 seconds at 0.1/s, remaining 47h 59m 50s at 0.19/s.
            gateway=new LocalGateway(clock,store);
            Assert.That(gateway.Snapshot().Resources.Food-beforeFood,
                Is.EqualTo(32831));
        }

        [Test]
        public void TimingsApplyOnlyToAuthoredBastionTwoThroughFive()
        {
            Assert.That(BastionProgressionCatalog.ForLevel(2).DurationSeconds,Is.EqualTo(120));
            Assert.That(BastionProgressionCatalog.ForLevel(3).DurationSeconds,Is.EqualTo(240));
            Assert.That(BastionProgressionCatalog.ForLevel(4).DurationSeconds,Is.EqualTo(420));
            Assert.That(BastionProgressionCatalog.ForLevel(5).DurationSeconds,Is.EqualTo(600));
            Assert.That(EconomyBastionTimingV2.TryGetSeconds(10,out var seconds),Is.True);
            Assert.That(seconds,Is.EqualTo(28800));
            Assert.Throws<ArgumentOutOfRangeException>(()=>BastionProgressionCatalog.ForLevel(6));
        }
    }
}
