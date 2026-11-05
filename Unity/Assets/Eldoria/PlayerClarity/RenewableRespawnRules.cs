using System;

namespace Eldoria.PlayerClarity
{
    /// <summary>
    /// Deterministic, save-compatible policy for renewable local map entities.
    /// Old cooldowns should be rebased from the actual recorded event, never
    /// from login time. Unique enemies must never call these rules.
    /// </summary>
    public static class RenewableRespawnRules
    {
        public static readonly TimeSpan Delay = TimeSpan.FromSeconds(300);

        public static long DueAt(long eventUtcTicks)
        {
            if (eventUtcTicks <= 0) throw new ArgumentOutOfRangeException(nameof(eventUtcTicks));
            return checked(eventUtcTicks + Delay.Ticks);
        }

        public static bool CanRespawn(long eventUtcTicks, long nowUtcTicks)
        {
            return eventUtcTicks > 0 && nowUtcTicks >= DueAt(eventUtcTicks);
        }

        /// <summary>
        /// Reduces a legacy 15/30-minute countdown to five minutes measured
        /// from the saved defeat/depletion. Never lengthens a shorter countdown.
        /// </summary>
        public static long MigrateExistingDue(long eventUtcTicks, long oldDueUtcTicks)
        {
            if (eventUtcTicks <= 0 || oldDueUtcTicks <= 0) return oldDueUtcTicks;
            return Math.Min(oldDueUtcTicks, DueAt(eventUtcTicks));
        }
    }
}
