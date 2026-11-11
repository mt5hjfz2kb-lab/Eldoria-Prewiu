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
        Check(!ResourceRewardRules.TryIssue(p, "mail:whitespace", "daily",
                "wood", 100, "  ") &&
              p.Packets.Count == 0,
            "whitespace-only mail ID must not create permanently locked packet");
        Check(ResourceRewardRules.TryIssue(p, "mail:null", "daily",
                "wood", 100, null) &&
              p.Packets[0].Collected &&
              ResourceRewardRules.ClaimableMailMessages(p) == 0,
            "null mail event means immediate bag packet, not an orphaned mail");
        Check(ResourceRewardRules.TryUse(p, wallet, "mail:null", out int noMailGrant) &&
              noMailGrant == 100,
            "direct bag packet may be used once after explicit Use");
        Check(!ResourceRewardRules.TryUse(p, wallet, "mail:null", out _),
            "direct bag packet cannot be used twice");
        // Subsequent historical wallet assertions are based on original
        // baseline. Return to a fresh inventory and original wallet before
        // running the existing full progression suite.
        p = new ResourceRewardInventory();
        wallet = new ResourceWallet { Wood = 250, Stone = 40, Food = 5 };
        foreach (var resource in new[] {"wood", "stone", "food"})
        foreach (var amount in new[] {100, 1000, 10000, 100000})
        {
            string expected = "Issue186ResourceArt/pack_" + resource + "_" +
                ResourceRewardRules.TierLabel(amount).ToLowerInvariant();
            Check(ResourceRewardRules.SpriteKey(resource, amount) == expected,
                "real package sprite address: " + expected);
        }
        Check(ResourceRewardRules.SpriteKey("invalid", 100) == null &&
            ResourceRewardRules.SpriteKey("wood", 0) == null,
            "no sprite for invalid kind or nonpositive quantity");
        Check(ResourceRewardRules.SpriteKey("wood", 30) ==
                "Issue186ResourceArt/pack_wood_100" &&
              ResourceRewardRules.SpriteKey("stone", 2500) ==
                "Issue186ResourceArt/pack_stone_1k" &&
              ResourceRewardRules.SpriteKey("food", int.MaxValue) ==
                "Issue186ResourceArt/pack_food_100k" &&
              ResourceRewardRules.AmountLabel(30) == "30" &&
              ResourceRewardRules.AmountLabel(2500) == "2500" &&
              ResourceRewardRules.AmountLabel(1000) == "1K",
            "nonstandard legitimate amounts retain exact text with nearest available picture");
        Check(!ResourceRewardRules.TryIssue(p, "mission:99", "mission", "wood", -99),
            "reject invalid negative amount, not a valid 99-piece award");
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

        Check(ResourceRewardRules.TryIssue(p, "mission:99", "mission", "wood", 99) &&
            wallet.Wood == 1350, "valid odd-sized reward is kept in inventory without credit");
        Check(ResourceRewardRules.TryUse(p, wallet, "mission:99", out int odd) &&
            odd == 99 && wallet.Wood == 1449,
            "99-wood packet credits exactly 99 only when explicitly used");
        Check(!ResourceRewardRules.TryUse(p, wallet, "mission:99", out _) &&
            wallet.Wood == 1449, "nonstandard packet cannot pay twice");

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
        var walletSaved = JsonSerializer.Serialize(wallet,
            new JsonSerializerOptions { IncludeFields=true });
        var walletRestored = JsonSerializer.Deserialize<ResourceWallet>(walletSaved,
            new JsonSerializerOptions { IncludeFields=true });
        int replayWood = walletRestored.Wood;
        Check(!ResourceRewardRules.TryUse(restored, walletRestored, "mission:wood", out _) &&
            !ResourceRewardRules.TryUse(restored, walletRestored, "mission:99", out _) &&
            ResourceRewardRules.ClaimMail(restored, "mail:daily") == 0 &&
            !ResourceRewardRules.TryUse(restored, walletRestored, "daily:wood", out _) &&
            walletRestored.Wood == replayWood,
            "reload/reconnect cannot replay previously spent packets or claimed mail");
        Check(ResourceRewardRules.TryIssue(restored, "gift:pending", "gift", "stone",
              275, "mail:pending") && ResourceRewardRules.ClaimMail(restored,"mail:other")==0,
            "unclaimed gift remains locked until matching explicit mail action");
        var secondLoad = JsonSerializer.Deserialize<ResourceRewardInventory>(
            JsonSerializer.Serialize(restored,new JsonSerializerOptions { IncludeFields=true }),
            new JsonSerializerOptions { IncludeFields=true });
        Check(!ResourceRewardRules.TryUse(secondLoad,walletRestored,"gift:pending",out _) &&
            ResourceRewardRules.ClaimMail(secondLoad,"mail:pending")==1 &&
            ResourceRewardRules.ClaimMail(secondLoad,"mail:pending")==0 &&
            ResourceRewardRules.TryUse(secondLoad,walletRestored,"gift:pending",out int gift) &&
            gift==275 && !ResourceRewardRules.TryUse(secondLoad,walletRestored,
                "gift:pending",out _),
            "pending mail survives reload; claim and use each commit once");


        // Multi-resource prizes must be all-or-nothing when reconnecting.
        var bundleInventory = new ResourceRewardInventory();
        var bundleWallet = new ResourceWallet { Wood=9, Stone=7, Food=5 };
        Check(!ResourceRewardRules.TryIssueBundle(bundleInventory,
            "bundle:empty","daily",0,0,0) && bundleInventory.Packets.Count==0,
            "a zero-value bundle creates no phantom inventory");
        Check(!ResourceRewardRules.TryIssueBundle(bundleInventory,
            "bundle:negative","daily",100,-1,0) &&
            !ResourceRewardRules.TryIssueBundle(bundleInventory,
            "bundle:hunt","hunt",100,100,0) &&
            !ResourceRewardRules.TryIssueBundle(bundleInventory,
            "bundle:whitespace","daily",100,100,0,"   ") &&
            bundleInventory.Packets.Count==0,
            "invalid bundle input cannot partially grant a resource");
        Check(ResourceRewardRules.TryIssueBundle(bundleInventory,
            "bundle:daily7","daily",120,250,0,"mail:day7") &&
            bundleInventory.Packets.Count==2 &&
            ResourceRewardRules.ClaimableMailMessages(bundleInventory)==1 &&
            bundleWallet.Wood==9 && bundleWallet.Stone==7,
            "mixed 120+250 reward is one unread mail, not wallet credit");
        Check(!ResourceRewardRules.TryIssueBundle(bundleInventory,
            "bundle:daily7","daily",120,250,0,"mail:day7") &&
            bundleInventory.Packets.Count==2,
            "replayed bundle cannot duplicate any of its component receipts");
        Check(!ResourceRewardRules.TryUse(bundleInventory,bundleWallet,
            "bundle:daily7:wood",out _) &&
            ResourceRewardRules.ClaimMail(bundleInventory,"mail:wrong")==0 &&
            ResourceRewardRules.ClaimableMailMessages(bundleInventory)==1,
            "mail read/incorrect claim never unlocks bundle");
        Check(ResourceRewardRules.ClaimMail(bundleInventory,"mail:day7")==2 &&
            ResourceRewardRules.ClaimMail(bundleInventory,"mail:day7")==0 &&
            bundleWallet.Wood==9 && bundleWallet.Stone==7,
            "claim unlocks two packets without touching resource totals");
        Check(ResourceRewardRules.TryUse(bundleInventory,bundleWallet,
            "bundle:daily7:wood",out int bundleWood) && bundleWood==120 &&
            ResourceRewardRules.TryUse(bundleInventory,bundleWallet,
            "bundle:daily7:stone",out int bundleStone) && bundleStone==250 &&
            bundleWallet.Wood==129 && bundleWallet.Stone==257,
            "optional use credits exact amounts instead of artwork tiers");
        Check(!ResourceRewardRules.TryUse(bundleInventory,bundleWallet,
            "bundle:daily7:stone",out _) && bundleWallet.Stone==257,
            "consumed mixed packet cannot repay on replay");
        var partialInventory = new ResourceRewardInventory();
        Check(ResourceRewardRules.TryIssue(partialInventory,
            "bundle:partial:wood","mission","wood",50) &&
            !ResourceRewardRules.TryIssueBundle(partialInventory,
            "bundle:partial","mission",50,75,100) &&
            partialInventory.Packets.Count==1 &&
            !partialInventory.Packets.Exists(x=>x.Kind=="stone") &&
            !partialInventory.Packets.Exists(x=>x.Kind=="food"),
            "colliding first receipt blocks ALL remaining bundle components");
        var restoredBundle = JsonSerializer.Deserialize<ResourceRewardInventory>(
            JsonSerializer.Serialize(bundleInventory,
                new JsonSerializerOptions{IncludeFields=true}),
            new JsonSerializerOptions{IncludeFields=true});
        Check(restoredBundle.Packets.Count==2 &&
            !ResourceRewardRules.TryIssueBundle(restoredBundle,
                "bundle:daily7","daily",120,250,0,"mail:day7") &&
            ResourceRewardRules.ClaimMail(restoredBundle,"mail:day7")==0,
            "reconnected saved bundle cannot be replayed or re-claimed");

        // Exact queued events, not a perpetual red dot on each menu button.
        var attentionLedger = new AttentionCueLedger();
        Check(!AttentionCueRules.Signal(attentionLedger,"","mail",AttentionLevel.New) &&
              !AttentionCueRules.Signal(attentionLedger,"mail:none","",AttentionLevel.New) &&
              !AttentionCueRules.Signal(attentionLedger,"mail:none","mail",AttentionLevel.None),
              "invalid/empty notification cannot create a phantom badge");
        Check(AttentionCueRules.Signal(attentionLedger,"mail:7","mail",AttentionLevel.New),
              "real new mail event activates unread cue");
        Check(AttentionCueRules.At(attentionLedger,"mail")==AttentionLevel.New,
              "new mail badge shown");
        Check(AttentionCueRules.MarkViewed(attentionLedger,"mail:7") &&
              AttentionCueRules.At(attentionLedger,"mail")==AttentionLevel.None,
              "reading information clears only the new badge");
        Check(!AttentionCueRules.Signal(attentionLedger,"mail:7","mail",AttentionLevel.New),
              "reloading a read mail does not create an unread badge");
        Check(AttentionCueRules.Signal(attentionLedger,"reward:mail7","mail",
                AttentionLevel.ActionReady),
              "actual pending reward produces action-required badge");
        Check(AttentionCueRules.At(attentionLedger,"mail")==AttentionLevel.ActionReady,
              "claim outranks read notifications");
        Check(AttentionCueRules.MarkViewed(attentionLedger,"reward:mail7") &&
              AttentionCueRules.At(attentionLedger,"mail")==AttentionLevel.ActionReady,
              "viewing or opening claimable mail does not clear pending claim");
        Check(!AttentionCueRules.Resolve(attentionLedger,"reward:mail7",false) &&
              AttentionCueRules.At(attentionLedger,"mail")==AttentionLevel.ActionReady,
              "failed or simulated claim does not clear pending award");
        Check(!AttentionCueRules.Signal(attentionLedger,"reward:mail7","daily",
                AttentionLevel.New),
              "event IDs cannot be reused across different destinations");
        Check(AttentionCueRules.Resolve(attentionLedger,"reward:mail7",true) &&
              AttentionCueRules.At(attentionLedger,"mail")==AttentionLevel.None,
              "successful authority claim clears pending badge");
        Check(!AttentionCueRules.Resolve(attentionLedger,"reward:mail7",true) &&
              !AttentionCueRules.Signal(attentionLedger,"reward:mail7","mail",
                AttentionLevel.ActionReady),
              "replay cannot reactivate claimed reward");
        Check(AttentionCueRules.Signal(attentionLedger,"daily:20261111","daily",
                AttentionLevel.New) &&
              AttentionCueRules.Signal(attentionLedger,"daily:20261111","daily",
                AttentionLevel.ActionReady),
              "new daily reward upgrades from information to action state");
        Check(!AttentionCueRules.Signal(attentionLedger,"daily:20261111","daily",
                AttentionLevel.New) &&
              AttentionCueRules.At(attentionLedger,"daily")==AttentionLevel.ActionReady,
              "daily claimable cannot be silently downgraded to informational");
        Check(AttentionCueRules.Expire(attentionLedger,"daily:20261111",true) &&
              AttentionCueRules.At(attentionLedger,"daily")==AttentionLevel.None,
              "authoritative expired event cannot remain red forever");
        Check(AttentionCueRules.Signal(attentionLedger,"daily:20261112","daily",
                AttentionLevel.ActionReady) &&
              AttentionCueRules.At(attentionLedger,"daily")==AttentionLevel.ActionReady,
              "new day uses its own unique event and displays new claim");
        var attentionSnapshot=attentionLedger.Copy();
        Check(AttentionCueRules.Resolve(attentionSnapshot,"daily:20261112",true) &&
              AttentionCueRules.At(attentionLedger,"daily")==AttentionLevel.ActionReady,
              "notification snapshots are deep copied");
        var attentionJson=JsonSerializer.Serialize(attentionLedger,
            new JsonSerializerOptions { IncludeFields=true });
        var restoredAttention=JsonSerializer.Deserialize<AttentionCueLedger>(
            attentionJson,new JsonSerializerOptions { IncludeFields=true });
        Check(AttentionCueRules.At(restoredAttention,"daily")==AttentionLevel.ActionReady &&
              AttentionCueRules.At(restoredAttention,"mail")==AttentionLevel.None &&
              !AttentionCueRules.Signal(restoredAttention,"reward:mail7","mail",
                AttentionLevel.ActionReady),
              "notification save/load retains pending actions and resolved receipts");

        var nestedAttention=new AttentionCueLedger();
        Check(AttentionCueRules.Signal(nestedAttention,"unread:1","hud/mail",
                AttentionLevel.New) &&
              AttentionCueRules.AtOrDescendants(nestedAttention,"hud")==AttentionLevel.New &&
              AttentionCueRules.AtOrDescendants(nestedAttention,"hud/mail")==AttentionLevel.New,
              "nested new-mail alert bubbles to its real HUD navigation");
        Check(AttentionCueRules.AtOrDescendants(nestedAttention,"hu")==AttentionLevel.None &&
              AttentionCueRules.AtOrDescendants(nestedAttention,"hud2")==AttentionLevel.None,
              "prefix collision cannot create phantom navigation indicators");
        Check(AttentionCueRules.Signal(nestedAttention,"daily:claim","hud/rewards/daily",
                AttentionLevel.ActionReady) &&
              AttentionCueRules.AtOrDescendants(nestedAttention,"hud")==AttentionLevel.ActionReady &&
              AttentionCueRules.AtOrDescendants(nestedAttention,"hud/rewards")==AttentionLevel.ActionReady,
              "pending action outranks unread child in parent navigation");
        Check(AttentionCueRules.Resolve(nestedAttention,"daily:claim",true) &&
              AttentionCueRules.AtOrDescendants(nestedAttention,"hud")==AttentionLevel.New,
              "parent falls back to remaining unread information");
        Check(AttentionCueRules.MarkViewed(nestedAttention,"unread:1") &&
              AttentionCueRules.AtOrDescendants(nestedAttention,"hud")==AttentionLevel.None,
              "parent clears only once all children have no outstanding attention");

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
