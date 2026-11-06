using System;
using System.Reflection;
using Eldoria.Domain;

namespace Eldoria.UI.RoyalGold
{
    /// <summary>Read-only seam for the old baseline and Valoria's timed upgrade. Never creates or advances a timer.</summary>
    public static class BastionTiming
    {
        private static readonly FieldInfo DeadlineField = typeof(PlayerState).GetField(
            "BastionUpgradeCompletesUtcTicks", BindingFlags.Instance | BindingFlags.Public);

        public static bool IsTimed => DeadlineField != null && DeadlineField.FieldType == typeof(long);
        public static long Deadline(PlayerState state) => state != null && IsTimed
            ? (long)DeadlineField.GetValue(state) : 0L;
        // Display the same authored timer that LocalGateway applies to Bastion II.
        public static int DurationSeconds => BastionProgressionCatalog.ForLevel(2).DurationSeconds;
    }
}
