using System;
using Eldoria.Domain;
using Eldoria.DailyTreasures;
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
        var initial=DailyTreasureRevealTimeline.Sample(0f);
        Eq(initial.Closed,1f,"chest starts fully closed");
        Eq(initial.Opened,0f,"open image initially invisible");
        for(int i=0;i<=500;i++)
        {
            float time=i*DailyTreasureRevealTimeline.DurationSeconds/500f;
            var frame=DailyTreasureRevealTimeline.Sample(time);
            True(frame.Closed>=0f && frame.Closed<=1f,"closed alpha bounded "+i);
            True(frame.Opened>=0f && frame.Opened<=1f,"opened alpha bounded "+i);
            True(Math.Abs(frame.Closed+frame.Opened-1f)<.0001f,
                "two original images form a single opaque silhouette "+i);
            True(frame.Glow>=0f && frame.Glow<=1f,"warm flash bounded "+i);
            True(frame.Embers>=0f && frame.Embers<=1f,"ember alpha bounded "+i);
            True(frame.Scale>=.95f && frame.Scale<=1.04f,"subtle premium scale "+i);
            True(Math.Abs(frame.Shake)<=.007f,"controlled hinge shake "+i);
            True(frame.Text>=0f && frame.Text<=1f,"caption visibility bounded "+i);
        }
        var peak=DailyTreasureRevealTimeline.Sample(
            DailyTreasureRevealTimeline.RevealPeakSeconds);
        True(peak.Glow>.35f,"flash genuinely disguises image pose transition");
        True(peak.Closed>.1f && peak.Opened>.1f,
            "pose transition takes place under peak flash");
        var final=DailyTreasureRevealTimeline.Sample(
            DailyTreasureRevealTimeline.DurationSeconds+.1f);
        True(final.Finished && final.Opened==1f && final.Closed==0f,
            "duration completed with one open asset");
        Eq(DailyTreasureRules.NextConsecutiveBonusDay(0),3,
            "new account sees first three-day loyalty target");
        Eq(DailyTreasureRules.NextConsecutiveBonusDay(2),3,
            "two-day streak previews bonus tomorrow");
        Eq(DailyTreasureRules.NextConsecutiveBonusDay(3),6,
            "after first bonus the next is day six");
        Eq(DailyTreasureRules.NextConsecutiveBonusDay(6),7,
            "seventh consecutive day gets the special loyalty reward");
        Eq(DailyTreasureRules.NextConsecutiveBonusDay(7),9,
            "loyalty milestones continue across reward cycles");
        Eq(DailyTreasureRules.NextConsecutiveBonusDay(13),14,
            "fourteen-day consecutive milestone remains special");
        Eq(DailyTreasureRules.BonusFoodForStreak(3),25,
            "three-day extra food");
        Eq(DailyTreasureRules.BonusFoodForStreak(7),80,
            "seventh-day extra food");
        Eq(DailyTreasureRules.BonusFoodForStreak(14),80,
            "next week loyalty stays limited");
        for(int i=0;i<=500;i++)
        {
            float t=i*DailyTreasureRevealTimeline.DurationSeconds/500f;
            var quiet=DailyTreasureRevealTimeline.Sample(
                t,DailyTreasureCeremonyTier.Quiet);
            var special=DailyTreasureRevealTimeline.Sample(
                t,DailyTreasureCeremonyTier.Special);
            var legendary=DailyTreasureRevealTimeline.Sample(
                t,DailyTreasureCeremonyTier.Legendary);
            True(quiet.Glow<=special.Glow+0.0001f &&
                 special.Glow<=legendary.Glow+0.0001f,
                 "legendary lighting must never be weaker "+i);
            True(quiet.Embers<=special.Embers+0.0001f &&
                 special.Embers<=legendary.Embers+0.0001f,
                 "legendary embers must never be weaker "+i);
            True(legendary.Scale>=special.Scale &&
                 legendary.Scale<=1.05f,
                 "rare ceremony stays within premium mobile scale "+i);
            True(legendary.Closed==special.Closed &&
                 legendary.Opened==special.Opened,
                 "different tiers must not make inconsistent lid poses "+i);
            True(legendary.Finished==special.Finished &&
                 quiet.Finished==special.Finished,
                 "all reward ceremonies must finish at the same duration "+i);
        }
        var normalPeak=DailyTreasureRevealTimeline.Sample(
            DailyTreasureRevealTimeline.RevealPeakSeconds,
            DailyTreasureCeremonyTier.Special);
        var grandPeak=DailyTreasureRevealTimeline.Sample(
            DailyTreasureRevealTimeline.RevealPeakSeconds,
            DailyTreasureCeremonyTier.Legendary);
        True(grandPeak.Glow>normalPeak.Glow,
            "real seventh-day grand coffer gets a distinct cinematic flash");
        // The luxurious reward cards must show what the gateway actually
        // credited, not a different visual-only loot table.
        int[] expectedCards={1,1,1,2,1,2,4};
        for(int day=1;day<=7;day++)
        {
            var received=DailyTreasureRules.ForDay(day,day);
            var art=DailyTreasureRewardPresentation.FromCommittedReward(received);
            Eq(art.Length,expectedCards[day-1],
                "authored separate reveal cards for real paid day "+day);
            True(art.Length<=DailyTreasureRewardPresentation.MaxSimultaneousItems,
                "every paid reward fits mobile reveal layout "+day);
            for(int i=0;i<art.Length;i++)
            {
                True(!string.IsNullOrWhiteSpace(art[i].Icon),
                    "earned item has an original PNG identity "+day+"/"+i);
                True(!string.IsNullOrWhiteSpace(art[i].Name) &&
                     !string.IsNullOrWhiteSpace(art[i].Quantity),
                    "earned item caption is understandable "+day+"/"+i);
                True(art[i].Quantity.StartsWith("x "),
                    "quantities are accurate numbers, not fake random loot "+day+"/"+i);
            }
        }
        var grand=DailyTreasureRewardPresentation.FromCommittedReward(
            DailyTreasureRules.ForDay(7,7));
        Eq(grand[0].Name,"MADERA","grand chest wood card");
        Eq(grand[0].Quantity,"x 180","grand chest exactly 180 actual wood");
        Eq(grand[1].Quantity,"x 120","grand chest exactly 120 actual stone");
        Eq(grand[2].Name,"COMIDA · RACHA","seventh-day streak is an explicit bonus");
        Eq(grand[2].Quantity,"x 80","grand chest never invents more than eighty food");
        True(grand[2].LoyaltyBonus,"real paid loyalty bonus has its own label");
        Eq(grand[3].Name,"ACELERADOR 5 MIN","grand chest earned timer is 5 min");
        Eq(grand[3].Quantity,"x 1","grand chest exactly one 5-min timer");
        var dayFourItems=DailyTreasureRewardPresentation.FromCommittedReward(
            DailyTreasureRules.ForDay(4,4));
        Eq(dayFourItems.Length,2,"fourth-day special chest has only two items");
        True(dayFourItems[0].Quantity=="x 90" && dayFourItems[1].Quantity=="x 60",
            "fourth-day chest matches exact paid wood and stone");
        Eq(DailyTreasureRewardPresentation.FromCommittedReward(
            default(DailyTreasureReward)).Length,0,
            "invalid reward cannot display fabricated cards");
        Console.WriteLine("PASS daily treasures domain: "+checks+" assertions");
    }
}
