using System;
using System.Text.Json;
using Eldoria.Domain;

internal static class Program
{
    private static int checks;
    private static void Check(bool condition, string name)
    {
        checks++;
        if (!condition) throw new InvalidOperationException("ISSUE186_FAIL " + name);
    }

    public static void Main()
    {
        var p = new ResourceRewardInventory();
        var wallet = new ResourceWallet { Wood = 250, Stone = 40, Food = 5 };
        Check(ResourceRewardRules.DirectToWallet("node"), "node direct");
        Check(ResourceRewardRules.DirectToWallet("hunt"), "hunt direct");
        Check(ResourceRewardRules.DirectToWallet("building"), "building direct");
        Check(!ResourceRewardRules.DirectToWallet("combat"), "combat is NOT hunting");
        Check(!ResourceRewardRules.DirectToWallet("mission"), "mission to bag");
        Check(!ResourceRewardRules.DirectToWallet("daily"), "daily to bag");
        Check(!ResourceRewardRules.DirectToWallet("unknown"), "default deny direct");

        Check(!ResourceRewardRules.TryIssue(p, "node:1", "node", "wood", 100),
            "direct sources must not create bogus packets");
        Check(!ResourceRewardRules.TryIssue(p, "mission:0", "mission", "wood", 0),
            "reject zero");
        Check(!ResourceRewardRules.TryIssue(p, "mission:-1", "mission", "wood", -2),
            "reject negative");
        Check(!ResourceRewardRules.TryIssue(p, "mission:ore", "mission", "ore", 100),
            "unsupported resource");
        foreach (var resource in new[] {"wood", "stone", "food"})
        foreach (var amount in new[] {100, 1000, 10000, 100000})
        {
            string expected = "Issue186ResourceArt/pack_" + resource + "_" +
                ResourceRewardRules.TierLabel(amount).ToLowerInvariant();
            Check(ResourceRewardRules.SpriteKey(resource, amount) == expected,
                "real package sprite address: " + expected);
        }
        Check(ResourceRewardRules.SpriteKey("invalid", 100) == null &&
            ResourceRewardRules.SpriteKey("wood", 30) == null,
            "no fabricated sprite for unsupported denomination");
        Check(!ResourceRewardRules.TryIssue(p, "mission:99", "mission", "wood", 99),
            "reject nonstandard resource denominations");
        Check(ResourceRewardRules.IsStandardTier(100) &&
            ResourceRewardRules.IsStandardTier(1000) &&
            ResourceRewardRules.IsStandardTier(10000) &&
            ResourceRewardRules.IsStandardTier(100000) &&
            !ResourceRewardRules.IsStandardTier(250) &&
            ResourceRewardRules.TierLabel(100000) == "100K",
            "all four UI tiers map to correct authoritative amounts");
        Check(ResourceRewardRules.TryIssue(p, "mission:wood", "mission", "wood", 100),
            "mission package received");
        Check(!ResourceRewardRules.TryIssue(p, "mission:wood", "mission", "wood", 100),
            "replay cannot issue packet twice");
        Check(wallet.Wood == 250, "award never credits wallet");
        Check(ResourceRewardRules.UnseenBagItems(p) == 1, "new bag attention");
        Check(AttentionSignalRules.Bag(ResourceRewardRules.UnseenBagItems(p)) == AttentionLevel.New,
            "bag signal is NEW not forced action");
        Check(ResourceRewardRules.MarkBagSeen(p, "mission:wood"), "bag marked seen");
        Check(ResourceRewardRules.UnseenBagItems(p) == 0, "seen dismisses badge not item");
        Check(!ResourceRewardRules.MarkBagSeen(p, "mission:wood"), "seen only once");
        Check(ResourceRewardRules.TryUse(p, wallet, "mission:wood", out int first) &&
            first == 100 && wallet.Wood == 350, "player voluntarily credits 100 wood");
        Check(!ResourceRewardRules.TryUse(p, wallet, "mission:wood", out _) &&
            wallet.Wood == 350, "spent package never pays twice");
        Check(!ResourceRewardRules.TryIssue(p, "mission:wood", "mission", "wood", 100),
            "spent packet receipt cannot be replayed");

        Check(ResourceRewardRules.TryIssue(p, "daily:wood", "daily", "wood", 1000, "mail:daily") &&
              ResourceRewardRules.TryIssue(p, "daily:stone", "daily", "stone", 100, "mail:daily"),
            "a mail can hold a mixed reward");
        Check(ResourceRewardRules.ClaimableMailMessages(p) == 1,
            "mail badge counts messages, not resource items");
        Check(!ResourceRewardRules.TryUse(p, wallet, "daily:wood", out _) &&
              wallet.Wood == 350, "opening or reading mail does not pay");
        Check(AttentionSignalRules.Mail(0, ResourceRewardRules.ClaimableMailMessages(p)) ==
              AttentionLevel.ActionReady, "claimable outranks unread");
        Check(ResourceRewardRules.ClaimMail(p, "mail:other") == 0, "different mail cannot claim");
        Check(ResourceRewardRules.ClaimMail(p, "mail:daily") == 2,
            "explicit claim unlocks both resource objects");
        Check(ResourceRewardRules.ClaimMail(p, "mail:daily") == 0,
            "claim replay idempotent");
        Check(wallet.Wood == 350 && wallet.Stone == 40, "claim moves to bag, not counters");
        Check(ResourceRewardRules.ClaimableMailMessages(p) == 0,
            "mail action badge disappears after real claim");
        Check(ResourceRewardRules.TryUse(p, wallet, "daily:stone", out int stone) &&
            stone == 100 && wallet.Stone == 140, "independent stone usage");
        Check(ResourceRewardRules.TryUse(p, wallet, "daily:wood", out int wood) &&
            wood == 1000 && wallet.Wood == 1350, "independent wood usage");
        Check(AttentionSignalRules.Mail(1, 0) == AttentionLevel.New, "unread mail badge");
        Check(AttentionSignalRules.Mail(0, 0) == AttentionLevel.None, "no phantom mail badge");
        Check(AttentionSignalRules.DailyTreasure(true) == AttentionLevel.ActionReady,
            "daily ready badge");
        Check(AttentionSignalRules.DailyTreasure(false) == AttentionLevel.None,
            "daily empty badge");

        Check(ResourceRewardRules.TryIssue(p, "overflow:food", "event", "food", 100),
            "overflow test package");
        wallet.Food = int.MaxValue - 3;
        Check(!ResourceRewardRules.TryUse(p, wallet, "overflow:food", out _) &&
            wallet.Food == int.MaxValue - 3 && !p.Packets[p.Packets.Count-1].Used,
            "overflow rejects atomically and keeps package");
        wallet.Food = 5;
        Check(ResourceRewardRules.TryUse(p, wallet, "overflow:food", out _) &&
            wallet.Food == 105, "package remains usable after overflow prevention");

        var saved = p.Copy();
        saved.Packets[0].Used = false;
        Check(p.Packets[0].Used, "snapshot deep copies receipt tombstones");
        var json = JsonSerializer.Serialize(p, new JsonSerializerOptions { IncludeFields=true });
        var restored = JsonSerializer.Deserialize<ResourceRewardInventory>(json,
            new JsonSerializerOptions { IncludeFields=true });
        Check(restored.Packets.Count == p.Packets.Count &&
            !ResourceRewardRules.TryIssue(restored, "mission:wood", "mission", "wood", 100),
            "persisted receipts survive serialize/load");

        var guide = new GuidedTutorialProgress();
        Check(!GuidedTutorialRules.ShouldPoint(guide, "first-mail", "mail-button", false, true),
            "locked features cannot point");
        Check(!GuidedTutorialRules.ShouldPoint(guide, "first-mail", "mail-button", true, false),
            "pointer never floats at invisible control");
        Check(GuidedTutorialRules.ShouldPoint(guide, "first-mail", "mail-button", true, true),
            "real visible control can be indicated");
        Check(!GuidedTutorialRules.ConfirmAction(guide, "first-mail", "MailRead", "MailRead", false),
            "tap is not authoritative success");
        Check(!GuidedTutorialRules.ConfirmAction(guide, "first-mail", "MailRead", "MailOpen", true),
            "wrong action cannot complete tutorial");
        Check(GuidedTutorialRules.ConfirmAction(guide, "first-mail", "MailRead", "MailRead", true),
            "real mail read completes tutorial");
        Check(!GuidedTutorialRules.ShouldPoint(guide, "first-mail", "mail-button", true, true),
            "completed tutorial does not replay");
        Check(!GuidedTutorialRules.ConfirmAction(guide, "first-mail", "MailRead", "MailRead", true),
            "tutorial completion idempotent");
        Check(GuidedTutorialRules.Dismiss(guide, "daily-intro"), "optional tutorial skip");
        Check(!GuidedTutorialRules.ShouldPoint(guide, "daily-intro", "treasure-button", true, true) &&
            !guide.Completed.Contains("daily-intro"), "skip never claims player learned");
        var g2 = guide.Copy();
        g2.Completed.Add("extra");
        Check(!guide.Completed.Contains("extra"), "tutorial progress copies independently");
        var serializedGuide = JsonSerializer.Serialize(guide,
            new JsonSerializerOptions { IncludeFields=true });
        var restoredGuide = JsonSerializer.Deserialize<GuidedTutorialProgress>(serializedGuide,
            new JsonSerializerOptions { IncludeFields=true });
        Check(restoredGuide.Completed.Contains("first-mail") &&
            restoredGuide.Dismissed.Contains("daily-intro"), "tutorial persistence survives load");
        Console.WriteLine("ISSUE186_DOMAIN_PASS checks=" + checks);
    }
}
