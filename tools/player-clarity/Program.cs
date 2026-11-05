using System;
using Eldoria.PlayerClarity;

int checks = 0;
void Assert(bool condition, string explanation)
{
    checks++;
    if (!condition) throw new Exception("PLAYER_CLARITY_SMOKE_FAIL: " + explanation);
}

Assert(PlayerNameRules.TryNormalize("  Álvaro_27  ", out var alias, out _)
       && alias == "Álvaro_27", "trim, accents and underscore");
Assert(PlayerNameRules.TryNormalize("Sir   Aldric", out alias, out _)
       && alias == "Sir Aldric", "collapse spaces");
Assert(PlayerNameRules.TryNormalize("123", out alias, out _)
       && alias == "123", "minimum length");
foreach (var invalid in new[] {"", "  ", "AB", "0123456789012345678",
    "<script>", "A\nB", "A\tB", "Luz🙂"})
    Assert(!PlayerNameRules.TryNormalize(invalid, out _, out _),
        "reject invalid nickname: " + invalid.Replace("\n", "NEWLINE"));

const long stamp = 638900000000000000L;
var delay = TimeSpan.FromSeconds(300).Ticks;
Assert(RenewableRespawnRules.Delay.Ticks == delay, "five minute constant");
Assert(RenewableRespawnRules.DueAt(stamp) == stamp + delay,
    "event time is the source, not login time");
Assert(!RenewableRespawnRules.CanRespawn(stamp,
    stamp + TimeSpan.FromSeconds(299).Ticks), "299 seconds is too early");
Assert(RenewableRespawnRules.CanRespawn(stamp, stamp + delay),
    "300 seconds is available");
Assert(RenewableRespawnRules.CanRespawn(stamp,
    stamp + TimeSpan.FromMinutes(20).Ticks), "offline elapsed period");
foreach (var minutes in new[] {15, 30})
    Assert(RenewableRespawnRules.MigrateExistingDue(stamp,
        stamp + TimeSpan.FromMinutes(minutes).Ticks) == stamp + delay,
        "legacy 15/30m migrated from real event");
Assert(RenewableRespawnRules.MigrateExistingDue(stamp, 0) == 0,
    "absent due does not invent a timer");
Assert(RenewableRespawnRules.MigrateExistingDue(0, stamp + delay) == stamp + delay,
    "absent event does not change due");
Assert(RenewableRespawnRules.MigrateExistingDue(stamp,
    stamp + TimeSpan.FromMinutes(2).Ticks) ==
    stamp + TimeSpan.FromMinutes(2).Ticks, "never lengthen shorter cooldown");
Console.WriteLine("PLAYER_CLARITY_DOMAIN_SMOKE_PASS checks=" + checks);
