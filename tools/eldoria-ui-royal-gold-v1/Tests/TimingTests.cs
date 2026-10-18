using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Eldoria.Domain;
using Eldoria.Application;
using Eldoria.UI.RoyalGold;

static class TimingTests
{
    sealed class Clock : IClock { public long Now = DateTime.UtcNow.Ticks; public long UtcTicks => Now; }
    sealed class Memory : IStateStore { public PlayerState State; public Memory(PlayerState state) { State = state; } public PlayerState Load() => State; public void Save(PlayerState state) { State = state; } }
    static readonly List<string> checks = new List<string>();
    static void Require(bool ok, string label) { if (!ok) throw new Exception(label); checks.Add(label); Console.WriteLine("PASS " + label); }
    static void Main()
    {
        var clock = new Clock();
        var seed = new PlayerState { Resources = new ResourceWallet { Wood = 4000, Stone = 3000 }, JourneyComplete = true, SawmillLevel = 1 };
        var gateway = new LocalGateway(clock, new Memory(seed));
        var ui = new ExistingSliceBinding(gateway, clock);
        var prior = gateway.Snapshot(); var action = ui.Read("bastion", false).Building.Action;
        Require(action.Enabled, "completed chapter offers real advancement");
        Require(ui.Submit(action).Ok, "advancement accepted by actual gateway");
        var after = gateway.Snapshot();
        Require(!ui.Submit(action).Ok, "same upgrade command not repeated");
        if (BastionTiming.IsTimed)
        {
            Require(after.BastionLevel == 1 && BastionTiming.Deadline(after) > clock.UtcTicks, "new contract retains level until deadline");
            Require(BastionTiming.DurationSeconds > 0, "duration read from actual contract");
            Require(!ui.Read("bastion", false).Building.Action.Enabled, "upgrade locked while in progress");
            long revision = after.Revision; clock.Now = BastionTiming.Deadline(after);
            Require(ui.Read("bastion", false).Building.Time == "Finalizando…", "UI does not complete expired upgrade");
            Require(gateway.Snapshot().Revision == revision && gateway.Snapshot().BastionLevel == 1, "UI read cannot advance newer contract");
            gateway.Advance(); Require(ui.Read("bastion", false).Building.Level == 2, "host completion updates new contract");
        }
        else
        {
            Require(BastionTiming.Deadline(after) == 0 && BastionTiming.DurationSeconds == 0, "old contract has no fabricated timing");
            Require(after.BastionLevel == 2, "old immediate upgrade still respected");
        }
        Require(ui.Read("bastion", false).Building.Action == null, "Bastion III is not invented");
        var report = new { source_sha = Environment.GetEnvironmentVariable("GITHUB_SHA"), contracts = Environment.GetEnvironmentVariable("ROYAL_CONTRACT_LABEL"), timed_contract = BastionTiming.IsTimed, native_unity = false, count = checks.Count, checks };
        Directory.CreateDirectory("docs/ui-royal-gold-v1");
        File.WriteAllText("docs/ui-royal-gold-v1/timing-tests.json", JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    }
}
