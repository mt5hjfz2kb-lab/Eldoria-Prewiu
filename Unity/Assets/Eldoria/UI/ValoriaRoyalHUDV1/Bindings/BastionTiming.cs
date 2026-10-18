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
        private static readonly FieldInfo DurationField = typeof(SliceContentProfiles.WebContract).GetField(
            "Bastion2UpgradeSeconds", BindingFlags.Static | BindingFlags.Public);

        public static bool IsTimed => DeadlineField != null && DeadlineField.FieldType == typeof(long);
        public static long Deadline(PlayerState state) => state != null && IsTimed
            ? (long)DeadlineField.GetValue(state) : 0L;
        public static int DurationSeconds => DurationField != null && DurationField.IsLiteral
            && DurationField.FieldType == typeof(int) ? (int)DurationField.GetRawConstantValue() : 0;
    }
}
