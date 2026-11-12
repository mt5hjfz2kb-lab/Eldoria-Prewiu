using System;

namespace Eldoria.DailyTreasures
{
    // Same premium artwork/animation grammar, with measured intensity for
    // ordinary special chests versus rare chapter/hero/weekly ceremonies.
    // No separate gameplay payout, loot rarity, or pay-to-win multiplier.
    public enum DailyTreasureCeremonyTier { Quiet, Special, Legendary }

    /// <summary>
    /// Purely visual cinematic timings for the actual saved reward. Works with
    /// two aligned illustrated assets and masks their pose change with a warm
    /// light bloom. No game state, audio API, or RNG calls.
    /// </summary>
    public readonly struct DailyTreasureRevealFrame
    {
        public readonly float Closed,Opened,Glow,Embers,Scale,Shake,Text;
        public readonly bool Finished;
        public DailyTreasureRevealFrame(float closed,float opened,float glow,
            float embers,float scale,float shake,float text,bool finished)
        {
            Closed=closed; Opened=opened; Glow=glow; Embers=embers;
            Scale=scale; Shake=shake; Text=text; Finished=finished;
        }
    }

    public static class DailyTreasureRevealTimeline
    {
        public const float DurationSeconds=2.35f;
        public const float RevealPeakSeconds=1.04f;
        static float C(float v) => v<0f?0f:v>1f?1f:v;
        static float Smooth(float a,float b,float time)
        {
            float t=C((time-a)/(b-a));
            return t*t*(3f-2f*t);
        }

        public static DailyTreasureRevealFrame Sample(float elapsed)
        {
            if(float.IsNaN(elapsed) || float.IsInfinity(elapsed))
                throw new ArgumentOutOfRangeException(nameof(elapsed));
            float t=Math.Max(0f,elapsed);
            float reveal=Smooth(.94f,1.16f,t);
            float glow=Smooth(.32f,.97f,t)*(1f-Smooth(1.15f,2.25f,t));
            float amberSpike=(float)Math.Exp(-Math.Pow((t-RevealPeakSeconds)/.16f,2));
            float embers=Smooth(.69f,1.22f,t)*(1f-Smooth(1.65f,2.35f,t));
            float shake=Smooth(.45f,.65f,t)*(1f-Smooth(.90f,1.12f,t));
            // Restrained, rapidly damped anticipation. It never over-scales
            // the asset enough to clip the mobile context panel.
            float tremor=(float)Math.Sin(t*48.0)*shake*.006f;
            float scale=.96f+.04f*Smooth(0f,.30f,t)+
                .035f*Smooth(.98f,1.20f,t)*(1f-Smooth(1.22f,1.85f,t));
            float caption=Smooth(1.33f,1.83f,t);
            return new DailyTreasureRevealFrame(
                1f-reveal,reveal,C(glow*.50f+amberSpike*.40f),
                C(embers),scale,tremor,C(caption),t>=DurationSeconds);
        }

        public static DailyTreasureRevealFrame Sample(float elapsed,
            DailyTreasureCeremonyTier tier)
        {
            var baseFrame=Sample(elapsed);
            float energy=tier==DailyTreasureCeremonyTier.Legendary?1.26f:
                tier==DailyTreasureCeremonyTier.Special?1f:.55f;
            float scale=baseFrame.Scale+
                (tier==DailyTreasureCeremonyTier.Legendary?
                    Math.Min(.008f,baseFrame.Glow*.008f):0f);
            return new DailyTreasureRevealFrame(
                baseFrame.Closed,baseFrame.Opened,
                C(baseFrame.Glow*energy),
                C(baseFrame.Embers*energy),
                scale,baseFrame.Shake,baseFrame.Text,baseFrame.Finished);
        }
    }
}
