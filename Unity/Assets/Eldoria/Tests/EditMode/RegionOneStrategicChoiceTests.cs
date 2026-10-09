using System;
using Eldoria.Application;
using Eldoria.Domain;
using NUnit.Framework;

namespace Eldoria.Tests
{
    public sealed class RegionOneStrategicChoiceTests
    {
        sealed class FixedClock : IClock { public long UtcTicks => new DateTime(2026, 10, 8, 12, 0, 0, DateTimeKind.Utc).Ticks; }
        sealed class Memory : IStateStore
        {
            public PlayerState Value;
            public PlayerState Load() => Value;
            public void Save(PlayerState state) => Value = state;
        }
        static GameCommand Order(LocalGateway g, string id, string option)
        {
            var state = g.Snapshot();
            return new GameCommand(id, state.PlayerId, "ChooseRegionOneForest", option, state.Revision);
        }
        [TestCase("forest-valoria:survey","survey",20,1250)]
        [TestCase("forest-valoria:harvest","harvest",40,1210)]
        public void ChoiceHasDistinctPersistentAndBoundedOutcome(string target, string expected, int reward, int remaining)
        {
            var memory = new Memory();
            var clock = new FixedClock();
            var game = new LocalGateway(clock, memory);
            int before = game.Snapshot().Resources.Wood;
            var order = Order(game,"choice-r2-b",target);
            Assert.That(game.Execute(order).Ok, Is.True);
            Assert.That(game.Snapshot().RegionOneForestChoice, Is.EqualTo(expected));
            Assert.That(game.Snapshot().Resources.Wood, Is.EqualTo(before+reward));
            Assert.That(game.Snapshot().ForestRemaining, Is.EqualTo(remaining));
            Assert.That(game.Execute(order).Ok, Is.True, "Same command is idempotent");
            Assert.That(game.Snapshot().Resources.Wood, Is.EqualTo(before+reward));
            Assert.That(game.Execute(Order(game,"second","forest-valoria:survey")).Ok,Is.False, "Choice can only happen once");
            var reloaded = new LocalGateway(clock,memory);
            Assert.That(reloaded.Snapshot().RegionOneForestChoice, Is.EqualTo(expected));
            Assert.That(reloaded.Snapshot().Resources.Wood, Is.EqualTo(before+reward));
        }
        [Test] public void InvalidAndStaleChoicesHaveNoSideEffects()
        {
            var game = new LocalGateway(new FixedClock(),new Memory());
            var before = game.Snapshot();
            Assert.That(game.Execute(Order(game,"invalid","forest-valoria:unknown")).Ok,Is.False);
            Assert.That(game.Execute(new GameCommand("stale","player-local","ChooseRegionOneForest","forest-valoria:harvest",before.Revision+1)).Ok,Is.False);
            Assert.That(game.Snapshot().RegionOneForestChoice,Is.Empty);
            Assert.That(game.Snapshot().Resources.Wood,Is.EqualTo(before.Resources.Wood));
            Assert.That(game.Snapshot().ForestRemaining,Is.EqualTo(before.ForestRemaining));
        }
        [Test] public void LegacySaveWithoutChoiceDefaultsToAvailable()
        {
            var state = new PlayerState();
            state.RegionOneForestChoice = null; // Simulates old JSON schema
            var memory = new Memory {Value=state};
            var game = new LocalGateway(new FixedClock(),memory);
            Assert.That(game.Execute(Order(game,"legacy","forest-valoria:survey")).Ok,Is.True);
            Assert.That(game.Snapshot().RegionOneForestChoice,Is.EqualTo("survey"));
        }
    }
}
