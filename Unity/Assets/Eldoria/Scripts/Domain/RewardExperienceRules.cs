using System;
using System.Collections.Generic;

namespace Eldoria.Domain
{
    // Issue #186 phase one. Kept separate from PlayerState/LocalGateway until
    // the PR #181 writer hands over those paths. All fields are Unity-serializable.
    [Serializable] public sealed class ResourceRewardPacket
    {
        public string ReceiptId = "";
        public string Origin = "";
        public string Kind = "";
        public string MailEventId = "";
        public int Amount;
        public bool Collected;
        public bool SeenInBag;
        public bool Used;
        public ResourceRewardPacket Copy() => (ResourceRewardPacket)MemberwiseClone();
    }

    [Serializable] public sealed class ResourceRewardInventory
    {
        // Used packets remain as receipt tombstones. Never reissue a spent ID.
        public List<ResourceRewardPacket> Packets = new List<ResourceRewardPacket>();
        public ResourceRewardInventory Copy()
        {
            var copy = new ResourceRewardInventory();
            foreach (var p in Packets ?? new List<ResourceRewardPacket>())
                if (p != null) copy.Packets.Add(p.Copy());
            return copy;
        }
    }

    public static class ResourceRewardRules
    {
        // Only the three owner-approved production/collection origins may be
        // credited directly by the existing authoritative gameplay systems.
        // Combat rewards, missions, explorations and daily prizes are NOT hunts.
        public static bool DirectToWallet(string origin) =>
            origin == "node" || origin == "hunt" || origin == "building";

        public static bool IsResource(string kind) =>
            kind == "wood" || kind == "stone" || kind == "food";

        // Owner-approved generic denominations. The game is free to adjust
        // reward tables later, but packets cannot promise unavailable art.
        public static bool IsStandardTier(int amount) =>
            amount == 100 || amount == 1000 ||
            amount == 10000 || amount == 100000;

        public static string TierLabel(int amount)
        {
            switch(amount)
            {
                case 100: return "100";
                case 1000: return "1K";
                case 10000: return "10K";
                case 100000: return "100K";
                default: return null;
            }
        }

        // Rewards may be 75, 250 or 2,500, not just art denominations.
        // NEVER discard a legitimate award because only four pictures exist.
        // Pick a visual tier while retaining the exact authoritative amount.
        public static int ArtworkTier(int amount) =>
            amount <= 0 ? 0 : amount < 1000 ? 100 :
            amount < 10000 ? 1000 : amount < 100000 ? 10000 : 100000;

        public static string AmountLabel(int amount) =>
            amount <= 0 ? null :
            TierLabel(amount) ?? amount.ToString(System.Globalization.CultureInfo.InvariantCulture);

        // The sixteen authored resources contain 12 generic packet pictures,
        // two badges and two pointers. Denomination is always LIVE text.
        public static string SpriteKey(string resourceKind, int amount)
        {
            var tier = TierLabel(ArtworkTier(amount));
            return IsResource(resourceKind) && tier != null ?
                "Issue186ResourceArt/pack_" + resourceKind + "_" + tier.ToLowerInvariant() :
                null;
        }

        // Receipt IDs must be stable and distinct per resource component:
        // e.g. "mission:c1-route:wood" and "mission:c1-route:stone".
        // mailEventId empty => collected directly into bag, never into wallet.
        // mailEventId nonempty => locked until explicit Mail Claim.
        public static bool TryIssue(ResourceRewardInventory inventory,
            string receiptId, string origin, string kind, int amount,
            string mailEventId = "")
        {
            if (inventory == null || string.IsNullOrWhiteSpace(receiptId) ||
                string.IsNullOrWhiteSpace(origin) || DirectToWallet(origin) ||
                !IsResource(kind) || amount <= 0 ||
                // A whitespace-only mail key would create an unclaimable
                // locked packet: TryIssue used IsNullOrEmpty while ClaimMail
                // rejects IsNullOrWhiteSpace. Fail closed before mutation.
                (mailEventId != null && mailEventId.Length > 0 &&
                    string.IsNullOrWhiteSpace(mailEventId))) return false;
            var packets = inventory.Packets;
            if (packets == null) return false;
            foreach (var p in packets)
                if (p != null && p.ReceiptId == receiptId) return false;
            packets.Add(new ResourceRewardPacket {
                ReceiptId = receiptId, Origin = origin, Kind = kind,
                Amount = amount, MailEventId = mailEventId ?? "",
                Collected = string.IsNullOrEmpty(mailEventId)
            });
            return true;
        }

        // A multi-resource mission/treasure prize is ONE logical grant.
        // Validate the whole bundle before adding anything so an already
        // consumed wood receipt cannot leave a newly duplicated stone item.
        // The real gateway must persist all packets in its single state commit.
        public static bool TryIssueBundle(ResourceRewardInventory inventory,
            string rewardReceiptId, string origin, int wood, int stone, int food,
            string mailEventId = "")
        {
            if (inventory?.Packets == null ||
                string.IsNullOrWhiteSpace(rewardReceiptId) ||
                string.IsNullOrWhiteSpace(origin) || DirectToWallet(origin) ||
                wood < 0 || stone < 0 || food < 0 ||
                (wood == 0 && stone == 0 && food == 0) ||
                (mailEventId != null && mailEventId.Length > 0 &&
                 string.IsNullOrWhiteSpace(mailEventId)))
                return false;

            var candidate = new List<ResourceRewardPacket>(3);
            void Add(string kind, int amount)
            {
                if (amount <= 0) return;
                candidate.Add(new ResourceRewardPacket {
                    ReceiptId = rewardReceiptId + ":" + kind,
                    Origin = origin, Kind = kind, Amount = amount,
                    MailEventId = mailEventId ?? "",
                    Collected = string.IsNullOrEmpty(mailEventId)
                });
            }
            Add("wood", wood);
            Add("stone", stone);
            Add("food", food);
            // Deduplicate by the logical reward event, not only by the
            // currently nonzero resource components. A reconnection may
            // replay the same event with a changed/reduced payload, e.g.
            // wood-only first and stone-only later. Comparing candidates
            // alone would wrongly grant the second payload. Existing receipt
            // tombstones cover this even after all items were spent/saved,
            // without introducing a second ledger or migrating old saves.
            // Match all THREE possible component receipts exactly, even if
            // the replay changes which resource amounts are nonzero. A raw
            // prefix would also swallow distinct, valid colon-delimited IDs
            // such as mission:c1 and mission:c1:bonus in either grant order.
            string woodReceipt = rewardReceiptId + ":wood";
            string stoneReceipt = rewardReceiptId + ":stone";
            string foodReceipt = rewardReceiptId + ":food";
            foreach (var existing in inventory.Packets)
            {
                if (existing != null &&
                    (existing.ReceiptId == woodReceipt ||
                     existing.ReceiptId == stoneReceipt ||
                     existing.ReceiptId == foodReceipt))
                    return false;
            }
            inventory.Packets.AddRange(candidate);
            return true;
        }

        // Reading/deleting a mail is intentionally NOT a claim. Caller must
        // invoke this only after a real explicit Claim action for that mail.
        public static int ClaimMail(ResourceRewardInventory inventory, string mailEventId)
        {
            if (inventory?.Packets == null || string.IsNullOrWhiteSpace(mailEventId))
                return 0;
            int count = 0;
            foreach (var p in inventory.Packets)
                if (p != null && p.MailEventId == mailEventId && !p.Collected)
                {
                    p.Collected = true;
                    count++;
                }
            return count;
        }

        // The same authoritative saved transaction must persist both the wallet
        // increment and the Used flag. No separate UI-side credit is allowed.
        public static bool TryUse(ResourceRewardInventory inventory, ResourceWallet wallet,
            string receiptId, out int granted)
        {
            granted = 0;
            if (inventory?.Packets == null || wallet == null ||
                string.IsNullOrWhiteSpace(receiptId)) return false;
            ResourceRewardPacket item = null;
            foreach (var p in inventory.Packets)
                if (p != null && p.ReceiptId == receiptId) { item = p; break; }
            if (item == null || !item.Collected || item.Used ||
                !IsResource(item.Kind) || item.Amount <= 0) return false;

            int current = item.Kind == "wood" ? wallet.Wood :
                item.Kind == "stone" ? wallet.Stone : wallet.Food;
            if (current < 0 || current > int.MaxValue - item.Amount) return false;
            int updated = current + item.Amount;
            if (item.Kind == "wood") wallet.Wood = updated;
            else if (item.Kind == "stone") wallet.Stone = updated;
            else wallet.Food = updated;
            item.Used = true;
            item.SeenInBag = true;
            granted = item.Amount;
            return true;
        }

        public static bool MarkBagSeen(ResourceRewardInventory inventory, string receiptId)
        {
            if (inventory?.Packets == null) return false;
            foreach (var p in inventory.Packets)
                if (p != null && p.ReceiptId == receiptId && p.Collected && !p.SeenInBag)
                {
                    p.SeenInBag = true;
                    return true;
                }
            return false;
        }

        public static int UnseenBagItems(ResourceRewardInventory inventory)
        {
            int count = 0;
            if (inventory?.Packets == null) return 0;
            foreach (var p in inventory.Packets)
                if (p != null && p.Collected && !p.SeenInBag && !p.Used) count++;
            return count;
        }

        public static int ClaimableMailMessages(ResourceRewardInventory inventory)
        {
            if (inventory?.Packets == null) return 0;
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var p in inventory.Packets)
                if (p != null && !p.Collected && !string.IsNullOrEmpty(p.MailEventId))
                    ids.Add(p.MailEventId);
            return ids.Count;
        }
    }

    public enum AttentionLevel { None = 0, New = 1, ActionReady = 2 }

    // Semantic signals only. PR #173 owns their visual representation.
    public static class AttentionSignalRules
    {
        public static AttentionLevel Mail(int unreadMessages, int claimableMessages) =>
            claimableMessages > 0 ? AttentionLevel.ActionReady :
            unreadMessages > 0 ? AttentionLevel.New : AttentionLevel.None;
        public static AttentionLevel DailyTreasure(bool canClaim) =>
            canClaim ? AttentionLevel.ActionReady : AttentionLevel.None;
        // A bag packet is optional, not an obligatory "todo" forever.
        public static AttentionLevel Bag(int unseenPackets) =>
            unseenPackets > 0 ? AttentionLevel.New : AttentionLevel.None;
    }


    // Durable attention is separate from a decorative red dot. Every cue
    // identifies an actual source event and access, not a permanent menu item.
    // A new daily cycle/report uses a NEW stable ID; replay cannot revive one
    // that was seen, completed or expired. Saved with PlayerState in the future.
    [Serializable] public sealed class AttentionCueRecord
    {
        public string EventId = "";
        public string Destination = "";
        public AttentionLevel Level;
        public bool Viewed;
        public bool Resolved;
        public AttentionCueRecord Copy() => (AttentionCueRecord)MemberwiseClone();
    }

    [Serializable] public sealed class AttentionCueLedger
    {
        public List<AttentionCueRecord> Records = new List<AttentionCueRecord>();
        public AttentionCueLedger Copy()
        {
            var copy = new AttentionCueLedger();
            foreach(var cue in Records ?? new List<AttentionCueRecord>())
                if(cue != null) copy.Records.Add(cue.Copy());
            return copy;
        }
    }

    // This stores notification semantics, not visual layout. It never
    // awards items and cannot call LocalGateway or clear a pending reward.
    public static class AttentionCueRules
    {
        public static bool Signal(AttentionCueLedger ledger,
            string eventId, string destination, AttentionLevel level)
        {
            if(ledger?.Records == null ||
                string.IsNullOrWhiteSpace(eventId) ||
                string.IsNullOrWhiteSpace(destination) ||
                (level != AttentionLevel.New && level != AttentionLevel.ActionReady))
                return false;
            foreach(var cue in ledger.Records)
            {
                if(cue == null || cue.EventId != eventId) continue;
                // ID reuse across destinations is a programming error.
                // Never silently move a reward to a different menu.
                if(cue.Destination != destination || cue.Resolved) return false;
                // ActionReady may supersede informational New, never reverse.
                if(cue.Level == AttentionLevel.New &&
                   level == AttentionLevel.ActionReady)
                {
                    cue.Level = level;
                    return true;
                }
                return false;
            }
            ledger.Records.Add(new AttentionCueRecord {
                EventId = eventId, Destination = destination, Level = level
            });
            return true;
        }

        // Reading a letter dismisses "new" only. The unpaid envelope remains
        // ActionReady until an actual authoritative Claim succeeds.
        public static bool MarkViewed(AttentionCueLedger ledger, string eventId)
        {
            if(ledger?.Records == null) return false;
            foreach(var cue in ledger.Records)
                if(cue != null && cue.EventId == eventId &&
                   !cue.Resolved && !cue.Viewed)
                {
                    cue.Viewed = true;
                    return true;
                }
            return false;
        }

        // Caller must pass the result of the REAL gateway command. A pointer
        // animation, opening a screen or marking a mail as read is not success.
        public static bool Resolve(AttentionCueLedger ledger, string eventId,
            bool authoritativeActionSucceeded)
        {
            if(ledger?.Records == null || !authoritativeActionSucceeded)
                return false;
            foreach(var cue in ledger.Records)
                if(cue != null && cue.EventId == eventId && !cue.Resolved)
                {
                    cue.Resolved = true;
                    return true;
                }
            return false;
        }

        // Expire only from a confirmed authoritative expiry/cancellation.
        public static bool Expire(AttentionCueLedger ledger, string eventId,
            bool authoritativeExpiry)
            => Resolve(ledger, eventId, authoritativeExpiry);

        // Aggregate nested notifications into a parent navigation access;
        // the real UI decides how to paint one badge without duplication.
        public static AttentionLevel At(AttentionCueLedger ledger,
            string destination)
        {
            if(ledger?.Records == null || string.IsNullOrWhiteSpace(destination))
                return AttentionLevel.None;
            AttentionLevel current = AttentionLevel.None;
            foreach(var cue in ledger.Records)
            {
                if(cue == null || cue.Resolved || cue.Destination != destination)
                    continue;
                if(cue.Level == AttentionLevel.ActionReady)
                    return AttentionLevel.ActionReady;
                if(cue.Level == AttentionLevel.New && !cue.Viewed)
                    current = AttentionLevel.New;
            }
            return current;
        }

        // Nested features may have their real icon in a parent navigation
        // group. Only true path descendants propagate a signal: "hud/mail"
        // belongs to "hud", but "hud2" does not. No permanent generic badge
        // is created and pending actions still outrank informational ones.
        public static AttentionLevel AtOrDescendants(AttentionCueLedger ledger,
            string destination)
        {
            if(ledger?.Records == null || string.IsNullOrWhiteSpace(destination))
                return AttentionLevel.None;
            string childPrefix = destination + "/";
            AttentionLevel result = AttentionLevel.None;
            foreach(var cue in ledger.Records)
            {
                if(cue == null || cue.Resolved ||
                   !(cue.Destination == destination ||
                     (cue.Destination != null && cue.Destination.StartsWith(
                         childPrefix,StringComparison.Ordinal))))continue;
                if(cue.Level == AttentionLevel.ActionReady)
                    return AttentionLevel.ActionReady;
                if(cue.Level == AttentionLevel.New && !cue.Viewed)
                    result = AttentionLevel.New;
            }
            return result;
        }
    }

    [Serializable] public sealed class GuidedTutorialProgress
    {
        public List<string> Completed = new List<string>();
        public List<string> Dismissed = new List<string>();
        public GuidedTutorialProgress Copy() => new GuidedTutorialProgress {
            Completed = new List<string>(Completed ?? new List<string>()),
            Dismissed = new List<string>(Dismissed ?? new List<string>())
        };
    }

    // A UI view must resolve controlId to an actual visible clickable control.
    // A successful authoritative action, not a tap animation, completes a step.
    public static class GuidedTutorialRules
    {
        public static bool ShouldPoint(GuidedTutorialProgress progress,
            string stepId, string controlId, bool featureUnlocked, bool controlVisible)
        {
            if (progress == null || string.IsNullOrWhiteSpace(stepId) ||
                string.IsNullOrWhiteSpace(controlId) || !featureUnlocked ||
                !controlVisible) return false;
            return !(progress.Completed?.Contains(stepId) ?? false) &&
                   !(progress.Dismissed?.Contains(stepId) ?? false);
        }

        public static bool ConfirmAction(GuidedTutorialProgress progress,
            string stepId, string requiredActionId, string actualActionId,
            bool authoritativeSuccess)
        {
            if (progress?.Completed == null || string.IsNullOrWhiteSpace(stepId) ||
                string.IsNullOrWhiteSpace(requiredActionId) || !authoritativeSuccess ||
                requiredActionId != actualActionId ||
                progress.Completed.Contains(stepId)) return false;
            progress.Completed.Add(stepId);
            return true;
        }

        // Explicit skip suppresses repetitive prompting but NEVER claims
        // that the real action was learned or performed.
        public static bool Dismiss(GuidedTutorialProgress progress, string stepId)
        {
            if (progress?.Dismissed == null || string.IsNullOrWhiteSpace(stepId) ||
                progress.Dismissed.Contains(stepId)) return false;
            progress.Dismissed.Add(stepId);
            return true;
        }
    }
}

