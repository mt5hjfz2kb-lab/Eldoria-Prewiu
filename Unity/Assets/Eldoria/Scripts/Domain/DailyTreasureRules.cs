using System;

namespace Eldoria.Domain
{
    // Saved within the ONE authoritative PlayerState; never use PlayerPrefs for claims.
    [Serializable]
    public sealed class DailyTreasureState
    {
        // UTC calendar-day index (ticks / TicksPerDay), not a wall-clock timer.
        // A skipped day ends the consecutive streak, never erases earned cycle progress.
        public long LastClaimUtcDay = -1;
        public int CycleDay;
        public int ConsecutiveDays;
        public int TotalClaims;

        public DailyTreasureState Copy() => (DailyTreasureState)MemberwiseClone();
    }

    public readonly struct DailyTreasureReward
    {
        public readonly int CycleDay, Streak, Wood, Stone, Food, BonusFood;
        public readonly int SpeedupOneMinute, SpeedupFiveMinutes;
        public readonly bool ShowChest;

        public DailyTreasureReward(int day, int streak, int wood, int stone,
            int food, int bonusFood, int oneMinute, int fiveMinutes, bool chest)
        {
            CycleDay=day; Streak=streak; Wood=wood; Stone=stone;
            Food=food; BonusFood=bonusFood; SpeedupOneMinute=oneMinute;
            SpeedupFiveMinutes=fiveMinutes; ShowChest=chest;
        }

        public bool IsValid => CycleDay>=1 && CycleDay<=7;
    }

    public static class DailyTreasureRules
    {
        public const int CycleLength=7;

        public static long UtcDay(long utcTicks)
        {
            if(utcTicks<0)throw new ArgumentOutOfRangeException(nameof(utcTicks));
            return utcTicks/TimeSpan.TicksPerDay;
        }

        // Player-facing reset countdown, independent of the device timezone.
        // It displays the actual UTC claim rollover instead of inaccurately
        // promising that rewards return at the player's local midnight.
        // 1..1440 minutes, rounded upward so we never show 00:00 while locked.
        public static int MinutesUntilNextUtcDay(long utcTicks)
        {
            if(utcTicks<0)throw new ArgumentOutOfRangeException(nameof(utcTicks));
            long remaining=TimeSpan.TicksPerDay-
                utcTicks%TimeSpan.TicksPerDay;
            return (int)((remaining+TimeSpan.TicksPerMinute-1)/
                TimeSpan.TicksPerMinute);
        }

        public static bool CanClaim(DailyTreasureState state,long utcTicks)
        {
            if(state==null)return false;
            return state.TotalClaims<int.MaxValue &&
                UtcDay(utcTicks)>state.LastClaimUtcDay;
        }

        public static int NextDay(DailyTreasureState state)
        {
            if(state==null)throw new ArgumentNullException(nameof(state));
            return state.CycleDay>=CycleLength || state.CycleDay<0
                ? 1 : state.CycleDay+1;
        }

        // UI hint only. This remains independent of a device's session
        // duration and never replaces authoritative PlayerState persistence.
        public static int NextConsecutiveBonusDay(int consecutiveDays)
        {
            int baseDay=Math.Min(1000000,Math.Max(0,consecutiveDays));
            for(int n=baseDay+1;n<=baseDay+7;n++)
                if(n%3==0 || n%7==0)return n;
            throw new InvalidOperationException("No loyalty bonus in the next seven days");
        }

        public static int BonusFoodForStreak(int streak)
            => streak>=7 && streak%7==0?80:
                streak>=3 && streak%3==0?25:0;

        public static DailyTreasureReward ForDay(int day,int streak)
        {
            if(day<1 || day>CycleLength)throw new ArgumentOutOfRangeException(nameof(day));
            int wood=0,stone=0,food=0,one=0,five=0;
            switch(day)
            {
                case 1: wood=60;break;
                case 2: stone=40;break;
                case 3: one=1;break;
                case 4: wood=90;stone=60;break;
                case 5: one=2;break;
                case 6: food=100;stone=80;break;
                case 7: wood=180;stone=120;five=1;break;
            }
            // A modest bonus for consecutive visits, without punishing skipped
            // days by resetting the seven-day reward calendar.
            int bonus=BonusFoodForStreak(streak);
            return new DailyTreasureReward(day,streak,wood,stone,
                food+bonus,bonus,one,five,day==4 || day==7);
        }

        public static DailyTreasureReward Preview(DailyTreasureState state,long utcTicks)
        {
            if(!CanClaim(state,utcTicks))return default(DailyTreasureReward);
            long day=UtcDay(utcTicks);
            // Cap BEFORE adding; int.MaxValue in an old or corrupted save
            // must never overflow back to a negative bonus counter.
            int streak=state.LastClaimUtcDay>=0 && day-state.LastClaimUtcDay==1
                ? Math.Min(999999,Math.Max(0,state.ConsecutiveDays))+1:1;
            return ForDay(NextDay(state),streak);
        }

        // Mutates the passed SAVE state only after all validations succeed.
        // Gateway checks wallet capacity, records command IDs and saves atomically.
        public static bool TryClaim(DailyTreasureState state,long utcTicks,
            out DailyTreasureReward reward)
        {
            reward=Preview(state,utcTicks);
            if(!reward.IsValid || state.TotalClaims==int.MaxValue)return false;
            state.CycleDay=reward.CycleDay;
            state.ConsecutiveDays=reward.Streak;
            state.LastClaimUtcDay=UtcDay(utcTicks);
            state.TotalClaims++;
            return true;
        }

        public static string Receipt(DailyTreasureReward reward)
        {
            if(!reward.IsValid)return "";
            var result="Ya recibidos: ";
            bool separated=false;
            if(reward.Wood>0){result+=reward.Wood+" madera";separated=true;}
            if(reward.Stone>0){result+=(separated?" · ":"")+reward.Stone+" piedra";separated=true;}
            if(reward.Food>0){result+=(separated?" · ":"")+reward.Food+" comida";separated=true;}
            if(reward.SpeedupOneMinute>0)
            {
                result+=(separated?" · ":"")+reward.SpeedupOneMinute+" acelerador(es) de 1 min";
                separated=true;
            }
            if(reward.SpeedupFiveMinutes>0)
                result+=(separated?" · ":"")+reward.SpeedupFiveMinutes+" acelerador(es) de 5 min";
            if(reward.BonusFood>0)result+=" (incluye "+reward.BonusFood+" comida por racha)";
            return result;
        }
    }
}
