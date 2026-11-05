using System;
using System.Collections.Generic;

namespace Eldoria.Domain
{
    // Pure deterministic production model. No Unity dependencies or changes to saved game state.
    // Rates are fixed-point micro-units per second; each level is independently configured.
    public static class EconomyProductionV2
    {
        public const long Scale = 1000000L;
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

        // The ledger stores sub-second precision and prevents repeat payment.
        // A future level-completion timestamp can split the interval into two rates.
        public static ProductionResult Accrue(
            long lastUtcTicks, long nowUtcTicks, long fractionalMicrounits,
            int currentLevel, string resource, long upgradeCompletedUtcTicks = 0,
            int previousLevel = -1)
        {
            if (lastUtcTicks < 0 || nowUtcTicks < 0 ||
                fractionalMicrounits < 0 || fractionalMicrounits >= Scale)
                throw new ArgumentOutOfRangeException("Invalid production ledger");
            if (nowUtcTicks <= lastUtcTicks)
                return new ProductionResult(0, fractionalMicrounits, lastUtcTicks);
            long micro = fractionalMicrounits;
            if (upgradeCompletedUtcTicks > lastUtcTicks &&
                upgradeCompletedUtcTicks < nowUtcTicks && previousLevel >= 0)
            {
                micro = checked(micro + Integrate(lastUtcTicks, upgradeCompletedUtcTicks,
                    Rate(resource, previousLevel)));
                micro = checked(micro + Integrate(upgradeCompletedUtcTicks, nowUtcTicks,
                    Rate(resource, currentLevel)));
            }
            else micro = checked(micro + Integrate(lastUtcTicks, nowUtcTicks,
                Rate(resource, currentLevel)));
            long whole = micro / Scale;
            if (whole > int.MaxValue)
                throw new OverflowException("Resource wallet capacity exceeded");
            return new ProductionResult((int)whole, micro % Scale, nowUtcTicks);
        }

        static long Integrate(long from, long to, long microPerSecond)
        {
            // Preserve sub-second fractions using integer ticks.
            return checked((to - from) * microPerSecond / TimeSpan.TicksPerSecond);
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
    }
}
