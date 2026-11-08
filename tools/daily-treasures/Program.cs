using System;
using Eldoria.Domain;
internal static class Program
{
    static int checks;
    static void Eq<T>(T actual,T expected,string name)
    {
        checks++;
        if(!object.Equals(actual,expected))
            throw new InvalidOperationException(name+": expected "+expected+", got "+actual);
    }
    static void True(bool value,string name) => Eq(value,true,name);
    static long Date(int day,int hour=12,int minute=0)
        => new DateTime(2026,11,day,hour,minute,0,DateTimeKind.Utc).Ticks;
    static void Main()
    {
        var s=new DailyTreasureState();
        Eq(s.CycleDay,0,"new calendar");
        True(DailyTreasureRules.CanClaim(s,Date(8)),"first day available");
        int[] wood={60,0,0,90,0,0,180};
        int[] stone={0,40,0,60,0,80,120};
        int[] food={0,0,25,0,0,125,80};
        for(int day=1;day<=7;day++)
        {
            long clock=Date(7+day);
            var preview=DailyTreasureRules.Preview(s,clock);
            Eq(preview.CycleDay,day,"preview cycle day "+day);
            Eq(preview.Wood,wood[day-1],"wood "+day);
            Eq(preview.Stone,stone[day-1],"stone "+day);
            Eq(preview.Food,food[day-1],"food "+day);
            Eq(preview.ShowChest,day==4 || day==7,"ceremony tier "+day);
            True(DailyTreasureRules.TryClaim(s,clock,out var award),"claim "+day);
            Eq(award.CycleDay,day,"awarded day "+day);
            Eq(s.ConsecutiveDays,day,"streak "+day);
            Eq(s.TotalClaims,day,"total "+day);
            True(!DailyTreasureRules.TryClaim(s,clock,out _),"no second claim "+day);
            Eq(DailyTreasureRules.Preview(s,clock).IsValid,false,"no second preview "+day);
        }
        True(DailyTreasureRules.TryClaim(s,Date(15),out var eighth),"cycle rollover");
        Eq(eighth.CycleDay,1,"cycle restarts after seven");
        Eq(s.ConsecutiveDays,8,"streak continues across cycle");
        Eq(eighth.Wood,60,"day one reward unchanged");
        var copy=s.Copy();
        copy.CycleDay=6;
        Eq(s.CycleDay,1,"snapshot is independent");
        True(!DailyTreasureRules.TryClaim(s,Date(14),out _),"clock rollback safe");
        var skipped=new DailyTreasureState();
        True(DailyTreasureRules.TryClaim(skipped,Date(8),out _),"skip test day1");
        True(DailyTreasureRules.TryClaim(skipped,Date(11),out var second),"skip test day2");
        Eq(second.CycleDay,2,"missing days do not lose calendar progress");
        Eq(second.Streak,1,"missing days reset streak only");
        True(DailyTreasureRules.TryClaim(skipped,Date(12),out var third),"streak increment");
        Eq(third.CycleDay,3,"third claim");
        Eq(third.BonusFood,0,"no false bonus after missed days");
        var boundary=new DailyTreasureState();
        True(DailyTreasureRules.TryClaim(boundary,Date(8,23,59),out _),"near UTC midnight");
        True(!DailyTreasureRules.TryClaim(boundary,Date(8,23,59),out _),"same UTC date");
        True(DailyTreasureRules.TryClaim(boundary,Date(9,0,0),out _),"new UTC date");
        Eq(DailyTreasureRules.ForDay(3,3).BonusFood,25,"three-day bonus");
        Eq(DailyTreasureRules.ForDay(7,7).BonusFood,80,"seven-day bonus");
        Eq(DailyTreasureRules.ForDay(7,6).BonusFood,25,"six-day mini-bonus");
        True(DailyTreasureRules.Receipt(DailyTreasureRules.ForDay(7,7)).Contains("5 min"),
            "accurate acceleration receipt");
        True(!DailyTreasureRules.TryClaim(null,Date(8),out _),"null save safe");
        var extreme=new DailyTreasureState{
            CycleDay=3,LastClaimUtcDay=DailyTreasureRules.UtcDay(Date(8)),
            ConsecutiveDays=int.MaxValue,TotalClaims=1000
        };
        var extremePreview=DailyTreasureRules.Preview(extreme,Date(9));
        Eq(extremePreview.Streak,1000000,"extreme streak saturates safely");
        True(DailyTreasureRules.TryClaim(extreme,Date(9),out _),
            "old invalid high streak must not wrap to negative");
        Eq(extreme.ConsecutiveDays,1000000,"saturated consecutive days are saved");
        var exhausted=new DailyTreasureState{
            CycleDay=6,ConsecutiveDays=6,TotalClaims=int.MaxValue,
            LastClaimUtcDay=DailyTreasureRules.UtcDay(Date(8))
        };
        True(!DailyTreasureRules.CanClaim(exhausted,Date(9)),
            "fully exhausted total-claims counter never offers a false button");
        Eq(DailyTreasureRules.Preview(exhausted,Date(9)).IsValid,false,
            "exhausted streak preview is invalid too");
        Console.WriteLine("PASS daily treasures domain: "+checks+" assertions");
    }
}
