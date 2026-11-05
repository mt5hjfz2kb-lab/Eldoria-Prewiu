using System;

namespace Eldoria.Domain
{
    /// <summary>
    /// Chapter III recurring hunts have fixed real power requirements in the
    /// existing combat. The displayed level is a category of that requirement,
    /// not a fabricated health or enemy tier. Hook these shared values into
    /// the canonical fight validator when the economy gate is released.
    /// Unique Ash Stalker is deliberately NOT a recurring hunt.
    /// </summary>
    public static class ChapterThreeHuntDifficultyPolicy
    {
        public static int RequiredPower(string target)
        {
            switch(target)
            {
                case "hunt-wolf": return 1450;
                case "hunt-boar": return 2050;
                default: throw new ArgumentException("Not a renewable Chapter III hunt", nameof(target));
            }
        }

        public static int LevelForPower(int power)
        {
            if (power <= 0) throw new ArgumentOutOfRangeException(nameof(power));
            if (power >= 4000) return 5;
            if (power >= 2600) return 4;
            if (power >= 1800) return 3;
            if (power >= 950) return 2;
            return 1;
        }

        public static int Level(string target) => LevelForPower(RequiredPower(target));
    }
}
