using System;

namespace Eldoria.DailyTreasures
{
    /// <summary>
    /// Presentation-only event palette. Every ceremony uses the same authored
    /// iron/bronze UI. The premium chest belongs to credited daily rewards;
    /// genuine milestone scenery and emblems identify narrative events.
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
            // Bronze/charcoal is always the dominant identity. Environmental
            // hints are subtle tints, never neon fairytale glows.
            float strength=legendary?.33f:.22f;
            if(!string.IsNullOrEmpty(eventId))
            {
                if(eventId.StartsWith("milestone:hero:lyra",
                    StringComparison.Ordinal))
                    return new DailyTreasureCeremonyTheme(
                        .73f,.47f,.33f,strength,legendary);
                if(eventId.StartsWith("milestone:bastion:",
                    StringComparison.Ordinal))
                    return new DailyTreasureCeremonyTheme(
                        .84f,.63f,.35f,strength,legendary);
                if(eventId.StartsWith("scout:",
                    StringComparison.Ordinal) ||
                   eventId.StartsWith("exploration:",
                    StringComparison.Ordinal))
                    return new DailyTreasureCeremonyTheme(
                        .45f,.56f,.43f,strength,legendary);
                if(eventId.StartsWith("vigilant:",
                    StringComparison.Ordinal))
                    // Only the Breach owns violet. Victories of the real
                    // Vigilants are celebrated in forged iron and old bronze.
                    return new DailyTreasureCeremonyTheme(
                        .54f,.49f,.40f,strength,legendary);
            }
            // Daily chest and any unrecognized future event use the same
            // original bronze-and-amber reference, not an unknown color.
            return new DailyTreasureCeremonyTheme(
                .92f,.70f,.40f,strength,legendary);
        }
    }
}
