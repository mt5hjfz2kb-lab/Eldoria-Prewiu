using System;
using System.Linq;
using Eldoria.Domain;

namespace Eldoria.DailyTreasures
{
    /// <summary>
    /// Decides whether a genuine progression event merits a one-time visual
    /// ceremony. Never creates a reward, a timer, a battle or a mailbox message.
    /// Animation uses only result data already committed by the real gateway.
    /// </summary>
    public static class DailyTreasureMilestoneRules
    {
        public static bool FirstScoutFamily(PlayerState before,
            PlayerState after, string orderId)
        {
            if(before?.Retention==null || after?.Retention==null ||
               string.IsNullOrWhiteSpace(orderId))return false;
            var order=after.Retention.Orders?.FirstOrDefault(o=>
                o!=null && o.Id==orderId);
            int kind=-1;
            if(order!=null)
            {
                // An existing but unclaimed order must never be replaced by
                // an older report merely to produce a ceremony.
                if(!order.Claimed)return false;
                kind=order.Kind;
            }
            else
            {
                // LocalGateway records the immutable, claimed-order report
                // BEFORE EarlyRetentionRules.Synchronize trims older orders.
                // A delayed, genuinely first family claim can therefore
                // disappear from Orders in the very same saved revision.
                // Recover only that exact newly-claimed order from both the
                // pre-command snapshot and its authoritative mail receipt.
                var beforeOrder=before.Retention.Orders?.FirstOrDefault(o=>
                    o!=null && o.Id==orderId);
                var mail=after.Mailbox?.Messages?.FirstOrDefault(m=>
                    m!=null && m.Type=="exploration" &&
                    m.Category=="informes" && m.DiscoveryOrderId==orderId &&
                    !string.IsNullOrWhiteSpace(m.RewardReceiptId) &&
                    m.EventId=="exploration:"+m.RewardReceiptId);
                if(beforeOrder==null || beforeOrder.Claimed || mail==null ||
                   beforeOrder.Kind!=mail.DiscoveryKind)
                    return false;
                kind=mail.DiscoveryKind;
            }
            if(kind<0 || kind>3)return false;
            int mask=1<<kind;
            if((before.Retention.ScoutDiscoveryMask & mask)!=0)return false;
            if((after.Retention.ScoutDiscoveryMask & mask)==0)return false;
            // Only this specific newly completed order can start a ceremony.
            // An already-claimed order or another scout's cached result must
            // never regenerate the rare discovery animation.
            if(before.Retention.Orders!=null &&
               before.Retention.Orders.Any(o=>o!=null&&o.Id==orderId&&o.Claimed))
                return false;
            return true;
        }

        public static bool NewlyCompletedBastion(PlayerState before,
            PlayerState after, out int newLevel)
        {
            newLevel=0;
            if(before==null || after==null)return false;
            if(after.BastionLevel<=before.BastionLevel)return false;
            if(after.BastionLevel<2 || after.BastionLevel>5)return false;
            if(after.BastionUpgradeCompletesUtcTicks>0)return false;
            newLevel=after.BastionLevel;
            return true;
        }

        public static bool NewlyRecruitedLyra(PlayerState before,PlayerState after)
            => before?.ChapterThree!=null && after?.ChapterThree!=null &&
               !before.ChapterThree.LyraRecruited && after.ChapterThree.LyraRecruited;
    }
}
