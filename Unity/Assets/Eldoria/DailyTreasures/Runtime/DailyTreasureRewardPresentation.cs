using System;
using System.Collections.Generic;
using Eldoria.Domain;

namespace Eldoria.DailyTreasures
{
    // Visual descriptors of REWARDS ALREADY CREDITED by LocalGateway.
    // Nothing here can mint inventory items or change the player state.
    public readonly struct DailyTreasureDisplayItem
    {
        public readonly string Icon,Name,Quantity;
        public readonly bool LoyaltyBonus;

        public DailyTreasureDisplayItem(string icon,string name,
            string quantity,bool loyaltyBonus=false)
        {
            Icon=icon;Name=name;Quantity=quantity;LoyaltyBonus=loyaltyBonus;
        }
    }

    public static class DailyTreasureRewardPresentation
    {
        public const int MaxSimultaneousItems=4;

        public static DailyTreasureDisplayItem[] FromCommittedReward(
            DailyTreasureReward received)
        {
            if(!received.IsValid)return Array.Empty<DailyTreasureDisplayItem>();
            var items=new List<DailyTreasureDisplayItem>(MaxSimultaneousItems);
            if(received.Wood>0)
                items.Add(new DailyTreasureDisplayItem(
                    "resource_wood","MADERA","x "+received.Wood));
            if(received.Stone>0)
                items.Add(new DailyTreasureDisplayItem(
                    "resource_stone","PIEDRA","x "+received.Stone));
            if(received.Food>0)
                items.Add(new DailyTreasureDisplayItem(
                    "resource_food",received.BonusFood>0?"COMIDA · RACHA":"COMIDA",
                    "x "+received.Food,received.BonusFood>0));
            if(received.SpeedupOneMinute>0)
                items.Add(new DailyTreasureDisplayItem(
                    "resource_hourglass","ACELERADOR 1 MIN",
                    "x "+received.SpeedupOneMinute));
            if(received.SpeedupFiveMinutes>0)
                items.Add(new DailyTreasureDisplayItem(
                    "resource_hourglass","ACELERADOR 5 MIN",
                    "x "+received.SpeedupFiveMinutes));
            if(items.Count>MaxSimultaneousItems)
                throw new InvalidOperationException(
                    "Daily reward has more items than the authored premium reveal layout");
            return items.ToArray();
        }
    }
}
