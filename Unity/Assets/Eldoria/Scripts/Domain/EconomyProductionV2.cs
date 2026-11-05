using System;
using System.Collections.Generic;

namespace Eldoria.Domain
{
    // Pure deterministic production model. No Unity dependencies or changes to saved game state.
    // Rates are fixed-point micro-units per second; each level is independently configured.
    public static class EconomyProductionV2
    {
        public const long Scale = 1000000L;
        // Production credited while away stops after 48h. A new active session
        // starts a fresh window; previously stored resources are unaffected.
        public const int MaxOfflineProductionHours = 48;
        public const long MaxOfflineProductionTicks = MaxOfflineProductionHours * TimeSpan.TicksPerHour;

        public static long OfflineWindowDeadline(long lastUtcTicks)
        {
            if (lastUtcTicks < 0) throw new ArgumentOutOfRangeException(nameof(lastUtcTicks));
            return lastUtcTicks > long.MaxValue - MaxOfflineProductionTicks
                ? long.MaxValue : lastUtcTicks + MaxOfflineProductionTicks;
        }
        public static readonly long[] WoodPerSecond = { 0, 120000, 220000, 360000, 540000, 750000 };
        public static readonly long[] StonePerSecond = { 0, 80000, 150000, 250000, 380000, 540000 };
        public static readonly long[] FoodPerSecond = { 0, 100000, 190000, 310000, 460000, 640000 };

        public static long Rate(string resource, int level)
        {
            var rates = resource == "wood" ? WoodPerSecond :
                resource == "stone" ? StonePerSecond :
                resource == "food" ? FoodPerSecond :
                throw new ArgumentException("Unknown resource", nameof(resource));
            if (level < 0 || level >= rates.Length)
                throw new ArgumentOutOfRangeException(nameof(level));
            return rates[level];
        }

        // Production is capped at the first 48 hours of a disconnected interval.
        // productionDeadlineUtcTicks lets a gateway preserve that same window
        // across a building upgrade and its two separate accrual calls.
        // Always settle through "now", including skipped time, to prevent backpay.
        public static ProductionResult Accrue(
            long lastUtcTicks, long nowUtcTicks, long fractionalMicrounits,
            int currentLevel, string resource, long upgradeCompletedUtcTicks = 0,
            int previousLevel = -1, long productionDeadlineUtcTicks = long.MaxValue)
        {
            if (lastUtcTicks < 0 || nowUtcTicks < 0 ||
                fractionalMicrounits < 0 || fractionalMicrounits >= Scale ||
                productionDeadlineUtcTicks < 0)
                throw new ArgumentOutOfRangeException("Invalid production ledger");
            if (nowUtcTicks <= lastUtcTicks)
                return new ProductionResult(0, fractionalMicrounits, lastUtcTicks);
            long payableUntil = Math.Min(nowUtcTicks,
                Math.Min(OfflineWindowDeadline(lastUtcTicks), productionDeadlineUtcTicks));
            if (payableUntil <= lastUtcTicks)
                return new ProductionResult(0, fractionalMicrounits, nowUtcTicks);
            long micro = fractionalMicrounits;
            if (upgradeCompletedUtcTicks > lastUtcTicks && previousLevel >= 0)
            {
                long oldLevelUntil = Math.Min(payableUntil, upgradeCompletedUtcTicks);
                micro = checked(micro + Integrate(lastUtcTicks, oldLevelUntil,
                    Rate(resource, previousLevel)));
                if (payableUntil > upgradeCompletedUtcTicks)
                    micro = checked(micro + Integrate(upgradeCompletedUtcTicks, payableUntil,
                        Rate(resource, currentLevel)));
            }
            else micro = checked(micro + Integrate(lastUtcTicks, payableUntil,
                Rate(resource, currentLevel)));
            long whole = micro / Scale;
            if (whole > int.MaxValue)
                throw new OverflowException("Resource wallet capacity exceeded");
            return new ProductionResult((int)whole, micro % Scale, nowUtcTicks);
        }

        static long Integrate(long from, long to, long microPerSecond)
        {
            // Divide before multiplying: multiplying full .NET ticks by the rate
            // overflows Int64 after ~14 days at the highest wood level, even
            // when the resulting resource payout still fits in Int32.
            long elapsedTicks = checked(to - from);
            long fullSeconds = elapsedTicks / TimeSpan.TicksPerSecond;
            long remainderTicks = elapsedTicks % TimeSpan.TicksPerSecond;
            long wholeSecondsMicro = checked(fullSeconds * microPerSecond);
            long remainderMicro = checked(remainderTicks * microPerSecond) / TimeSpan.TicksPerSecond;
            return checked(wholeSecondsMicro + remainderMicro);
        }
    }

    public readonly struct ProductionResult
    {
        public readonly int WholeUnits;
        public readonly long FractionalMicrounits;
        public readonly long SettledThroughUtcTicks;
        public ProductionResult(int wholeUnits, long fractionalMicrounits, long settledThroughUtcTicks)
        {
            WholeUnits = wholeUnits;
            FractionalMicrounits = fractionalMicrounits;
            SettledThroughUtcTicks = settledThroughUtcTicks;
        }
    }

    public static class EconomyBastionTimingV2
    {
        static readonly Dictionary<int, int> SecondsByTargetLevel = new Dictionary<int, int>
        {
            { 2, 120 }, { 3, 240 }, { 4, 420 }, { 5, 600 },
            { 6, 1200 }, { 7, 2400 }, { 8, 4500 },
            { 9, 12600 }, { 10, 28800 }
        };
        public static bool TryGetSeconds(int targetLevel, out int seconds)
            => SecondsByTargetLevel.TryGetValue(targetLevel, out seconds);
        public static int GetSeconds(int targetLevel)
        {
            if (!TryGetSeconds(targetLevel, out var seconds))
                throw new ArgumentOutOfRangeException(nameof(targetLevel));
            return seconds;
        }
    }
}
