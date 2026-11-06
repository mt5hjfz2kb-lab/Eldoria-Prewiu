using System;

namespace Eldoria.PlayerClarity
{
    /// <summary>
    /// Stable pseudo-random walk over any verified site list. Every candidate
    /// is visited at most once. Used by the canonical placement authority
    /// after biome, terrain, route and occupied-footprint checks.
    /// This does not choose positions or mutate game state on the client.
    /// </summary>
    public static class DistributedSiteOrder
    {
        private static uint Hash(string id, int generation)
        {
            unchecked
            {
                uint h = 2166136261u;
                foreach (char ch in id)
                    h = (h ^ ch) * 16777619u;
                h = (h ^ (uint)generation) * 16777619u;
                h ^= h >> 16;
                h *= 0x7feb352du;
                h ^= h >> 15;
                h *= 0x846ca68bu;
                return h ^ (h >> 16);
            }
        }

        private static int Gcd(int a, int b)
        {
            while (b != 0) { int r = a % b; a = b; b = r; }
            return a;
        }

        public static int Candidate(int count, string targetId, int generation, int attempt)
        {
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
            if (string.IsNullOrEmpty(targetId)) throw new ArgumentException("Stable resource or creature ID required", nameof(targetId));
            if (generation < 0) throw new ArgumentOutOfRangeException(nameof(generation));
            if (attempt < 0 || attempt >= count) throw new ArgumentOutOfRangeException(nameof(attempt));
            uint h = Hash(targetId, generation);
            int start = (int)(h % (uint)count);
            int step = 1 + (int)((h >> 8) % (uint)count);
            while (Gcd(step, count) != 1) step = step == count ? 1 : step + 1;
            return (int)(((long)start + (long)step * attempt) % count);
        }
    }
}
