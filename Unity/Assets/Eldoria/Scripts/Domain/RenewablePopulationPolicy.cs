using System;

namespace Eldoria.Domain
{
    /// <summary>
    /// The local world's recurring resources and animals respawn 300 seconds
    /// after their actual depletion/defeat UTC event, even across save reloads.
    /// This is not a passive economy/accrual rule and does not affect
    /// one-time story enemies or remote multiplayer server authority.
    /// </summary>
    public static class RenewablePopulationPolicy
    {
        public const int RespawnSeconds = 300;
        public static readonly TimeSpan Delay = TimeSpan.FromSeconds(RespawnSeconds);

        public static long DueAt(long eventUtcTicks)
        {
            if (eventUtcTicks <= 0)
                throw new ArgumentOutOfRangeException(nameof(eventUtcTicks));
            return checked(eventUtcTicks + Delay.Ticks);
        }

        public static bool CanRespawn(long eventUtcTicks,long nowUtcTicks)
        {
            return eventUtcTicks > 0 && nowUtcTicks >= DueAt(eventUtcTicks);
        }

        public static long MigrateExistingDue(long eventUtcTicks,long previousDueUtcTicks)
        {
            // A missing event timestamp cannot prove when depletion occurred.
            // Don't fabricate one from login time or extend shorter deadlines.
            if (eventUtcTicks <= 0 || previousDueUtcTicks <= 0)
                return previousDueUtcTicks;
            return Math.Min(previousDueUtcTicks, DueAt(eventUtcTicks));
        }
    }
}
