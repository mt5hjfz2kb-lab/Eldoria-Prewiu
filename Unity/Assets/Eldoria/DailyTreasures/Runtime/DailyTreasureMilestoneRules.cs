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
                o!=null && o.Id==orderId && o.Claimed);
            if(order==null || order.Kind<0 || order.Kind>3)return false;
            int mask=1<<order.Kind;
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
