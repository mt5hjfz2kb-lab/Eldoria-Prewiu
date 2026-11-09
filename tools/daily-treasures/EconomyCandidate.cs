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
    readonly struct ThirtyDayEnvelope
    {
        public readonly int Claims,Wood,Stone,Food,One,Five;
        public int AccelerationMinutes => One+Five*5;
        public ThirtyDayEnvelope(int claims,int wood,int stone,int food,
            int one,int five)
        {Claims=claims;Wood=wood;Stone=stone;Food=food;One=one;Five=five;}
    }

    static ThirtyDayEnvelope SimulateRealClaims(Func<int,bool> attends)
    {
        // Call actual game rules through their saved state for EACH day,
        // not a fantasy percentage or a free-grant rule for absent players.
        var state=new DailyTreasureState();
        int claims=0,wood=0,stone=0,food=0,one=0,five=0;
        for(int d=1;d<=30;d++)
        {
            if(!attends(d))continue;
            long utc=new DateTime(2026,11,1,12,0,0,DateTimeKind.Utc)
                .AddDays(d-1).Ticks;
            Need(DailyTreasureRules.TryClaim(state,utc,out var earned),
                "Scheduled real daily grant must succeed on day "+d);
            claims++;
            wood+=earned.Wood;stone+=earned.Stone;food+=earned.Food;
            one+=earned.SpeedupOneMinute;
            five+=earned.SpeedupFiveMinutes;
            Need(state.TotalClaims==claims && state.CycleDay==((claims-1)%7+1),
                "Seven-day calendar progress depends on claims, never attendance streak");
            Need(!DailyTreasureRules.TryClaim(state,utc,out _),
                "Same-day double reward remains prohibited in all user patterns");
            Need(earned.Wood>=0 && earned.Stone>=0 && earned.Food>=0,
                "No negative resources on irregular attendance");
        }
        return new ThirtyDayEnvelope(claims,wood,stone,food,one,five);
    }

    static void AssertScenario(string name,ThirtyDayEnvelope a,
        int claims,int wood,int stone,int food,int acceleration)
    {
        Need(a.Claims==claims, name+": exact number of paid claims");
        Need(a.Wood==wood&&a.Stone==stone&&a.Food==food,
            name+": exact treasury resource envelope");
        Need(a.AccelerationMinutes==acceleration,
            name+": exact accelerator minutes");
        Console.WriteLine("CANDIDATE_30D "+name+": claims="+a.Claims+
            " wood="+a.Wood+" stone="+a.Stone+" food="+a.Food+
            " speedupMinutes="+a.AccelerationMinutes);
    }

    static void CompareAttendancePatterns()
    {
        var daily=SimulateRealClaims(d=>true);
        var five=SimulateRealClaims(d=>(d-1)%7<5);
        var three=SimulateRealClaims(d=>(d-1)%7==0 ||
            (d-1)%7==2||(d-1)%7==4);
        var two=SimulateRealClaims(d=>(d-1)%7==0||(d-1)%7==3);
        var breakAfterFirstWeek=SimulateRealClaims(d=>d!=8);
        AssertScenario("30_consecutive",daily,30,1380,1240,945,32);
        AssertScenario("5_per_week",five,22,1050,900,400,24);
        AssertScenario("3_per_week",three,13,480,480,200,11);
        AssertScenario("2_per_week",two,9,390,340,100,8);
        AssertScenario("miss_day_8",breakAfterFirstWeek,29,1380,1200,920,32);
        Need(five.Food<daily.Food && three.Food<five.Food,
            "Loyalty bonus increases with consecutive actual attendance");
        Need(three.Claims>=7 && three.Wood>=330 &&
             three.Stone>=300 && three.Five>=1,
            "Casual players still earn the seven-day chest without consecutive visits");
        Need(breakAfterFirstWeek.Food>=daily.Food-25 &&
             breakAfterFirstWeek.Claims==29,
            "Missing one day may lose a minor streak bonus but cannot reset calendar");
        Need(daily.AccelerationMinutes==32,
            "Fully engaged player cannot generate unchecked paid-skip acceleration");
        Console.WriteLine("ECONOMY_SCENARIOS_SOURCE_ONLY: "+
            "four attendance patterns + one skipped day, NOT an approved economy");
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
        CompareAttendancePatterns();
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
