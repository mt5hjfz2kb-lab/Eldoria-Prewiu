using System;

namespace Eldoria.DailyTreasures
{
    /// <summary>
    /// Presentation-only event palette. Every ceremony uses the same authored
    /// iron/bronze UI and the same high-detail chest art; only the atmospheric
    /// light identifies WHY a real event was important.
    /// No currency, loot table, progression state, time or Unity dependency.
    /// </summary>
    public readonly struct DailyTreasureCeremonyTheme
    {
        public readonly float R,G,B,Strength;
        public readonly bool Legendary;
        public DailyTreasureCeremonyTheme(float r,float g,float b,
            float strength,bool legendary)
        {
            R=r;G=g;B=b;Strength=strength;Legendary=legendary;
        }
        public static DailyTreasureCeremonyTheme ForEvent(string eventId,
            bool legendary)
        {
            float strength=legendary?.43f:.29f;
            if(!string.IsNullOrEmpty(eventId))
            {
                if(eventId.StartsWith("milestone:hero:lyra",
                    StringComparison.Ordinal))
                    return new DailyTreasureCeremonyTheme(
                        .75f,.45f,.29f,strength,legendary);
                if(eventId.StartsWith("milestone:bastion:",
                    StringComparison.Ordinal))
                    return new DailyTreasureCeremonyTheme(
                        .93f,.63f,.25f,strength,legendary);
                if(eventId.StartsWith("scout:",
                    StringComparison.Ordinal) ||
                   eventId.StartsWith("exploration:",
                    StringComparison.Ordinal))
                    return new DailyTreasureCeremonyTheme(
                        .29f,.63f,.51f,strength,legendary);
                if(eventId.StartsWith("vigilant:",
                    StringComparison.Ordinal))
                    return new DailyTreasureCeremonyTheme(
                        .53f,.39f,.78f,strength,legendary);
            }
            // Daily chest and any unrecognized future event use the same
            // original bronze-and-amber reference, not an unknown color.
            return new DailyTreasureCeremonyTheme(
                .99f,.71f,.32f,strength,legendary);
        }
    }
}
