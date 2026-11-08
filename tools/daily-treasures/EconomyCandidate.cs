using System;
using Eldoria.Domain;

// An explicit OWNER_I_II economic candidate check, not a QA_FAST assessment
// and not evidence of real human retention. This cannot publish or grant loot.
internal static class EconomyCandidate
{
    static int checks;
    static void Need(bool pass,string name)
    {
        checks++;
        if(!pass)throw new Exception("REWARD_ECONOMY_GUARD: "+name);
    }
    static void Main()
    {
        const int days=14;
        int wood=0,stone=0,food=0,one=0,five=0;
        int weekWood=0,weekStone=0,weekFood=0,weekOne=0,weekFive=0;
        for(int day=1;day<=days;day++)
        {
            var reward=DailyTreasureRules.ForDay((day-1)%7+1,day);
            wood+=reward.Wood;stone+=reward.Stone;food+=reward.Food;
            one+=reward.SpeedupOneMinute;five+=reward.SpeedupFiveMinutes;
            Need(reward.Wood>=0 && reward.Stone>=0 && reward.Food>=0,
                "No negative grant day "+day);
            Need(reward.SpeedupOneMinute>=0 && reward.SpeedupFiveMinutes>=0,
                "No negative time day "+day);
            if(day==1)
            {
                Need(reward.Wood*4<=SliceContentProfiles.OwnerIiiCandidate.Bastion2WoodCost,
                    "First-day wood does not bypass owner Bastion II progression");
                Need(reward.Stone==0 && reward.SpeedupFiveMinutes==0,
                    "First claim does not bypass stone or timed Bastion progress");
            }
            if(day==2)
                Need(reward.Stone*4<=SliceContentProfiles.OwnerIiiCandidate.Bastion2StoneCost,
                    "Second-day stone stays moderate");
            if(day==7)
            {
                weekWood=wood;weekStone=stone;weekFood=food;
                weekOne=one;weekFive=five;
                Need(wood<=SliceContentProfiles.OwnerIiiCandidate.Bastion2WoodCost,
                    "Week-one bonus wood below the early Bastion cost");
                Need(stone<=SliceContentProfiles.OwnerIiiCandidate.Bastion2StoneCost,
                    "Week-one bonus stone below the early Bastion cost");
                Need(one+five*5<=10,
                    "Week-one acceleration entitlement below ten minutes");
            }
        }
        Need(weekWood==330 && weekStone==300 && weekFood==230,
            "Document exact week-one provisional resource grants");
        Need(weekOne==3 && weekFive==1,"Document exact week-one speedups");
        Need(wood==660 && stone==600 && food==460,
            "Two full cycles with authentic consecutive-day bonuses");
        Need(one==6 && five==2,"Two cycles of accelerator ownership");
        Console.WriteLine("PASS Daily Treasure OWNER_I_II candidate arithmetic: "+
            checks+" checks, NOT a completed economy or retention approval");
        Console.WriteLine("WEEK_1: wood="+weekWood+
            " stone="+weekStone+" food="+weekFood+
            " accelerated-minutes="+(weekOne+weekFive*5));
        Console.WriteLine("WEEK_2_CUMULATIVE: wood="+wood+
            " stone="+stone+" food="+food+
            " accelerated-minutes="+(one+five*5));
        Console.WriteLine("OWNER_BASTION_II: wood="+
            SliceContentProfiles.OwnerIiiCandidate.Bastion2WoodCost+
            " stone="+SliceContentProfiles.OwnerIiiCandidate.Bastion2StoneCost);
    }
}
