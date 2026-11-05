using System;
using Eldoria.Domain;

namespace Eldoria.PlayerClarity
{
    /// <summary>Presentation-side alias for canonical domain UTC policy.</summary>
    public static class RenewableRespawnRules
    {
        public static readonly TimeSpan Delay = RenewablePopulationPolicy.Delay;
        public static long DueAt(long eventUtcTicks) =>
            RenewablePopulationPolicy.DueAt(eventUtcTicks);
        public static bool CanRespawn(long eventUtcTicks, long nowUtcTicks) =>
            RenewablePopulationPolicy.CanRespawn(eventUtcTicks,nowUtcTicks);
        public static long MigrateExistingDue(long eventUtcTicks,long oldDueUtcTicks) =>
            RenewablePopulationPolicy.MigrateExistingDue(eventUtcTicks,oldDueUtcTicks);
    }
}
