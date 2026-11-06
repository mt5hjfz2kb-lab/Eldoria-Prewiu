using System;

namespace Eldoria.Domain
{
    /// <summary>
    /// Resource node tier has a gameplay meaning: higher tiers have a larger
    /// extractable capacity, not faster passive city production. This pure
    /// policy does not apply economy balance or change an existing save.
    /// The canonical gateway will assign it on spawn/respawn.
    /// </summary>
    public static class ResourceNodeTierPolicy
    {
        public const int MaxLevel = 5;
        public const int BaseWoodCapacity = 1250;
        public const int BaseStoneCapacity = 5200;

        public static int Capacity(string kind, int level)
        {
            if (level < 1 || level > MaxLevel)
                throw new ArgumentOutOfRangeException(nameof(level));
            int basis;
            if (kind == "wood") basis = BaseWoodCapacity;
            else if (kind == "stone") basis = BaseStoneCapacity;
            else throw new ArgumentException("Only renewable wood and stone may carry tiers",nameof(kind));
            return checked((basis * (10 + level - 1)) / 10);
        }

        public static int ExistingSaveLevel(string kind, int capacity)
        {
            // An unversioned schema-1 node may not have a tier. All shipped
            // pre-tier nodes used the baseline capacity, so they remain L1
            // without a resource rebalance on a saved game.
            int initial = Capacity(kind, 1);
            if (capacity <= initial) return 1;
            int best = 1; int distance = int.MaxValue;
            for (int tier = 1; tier <= MaxLevel; tier++)
            {
                var delta = Math.Abs(Capacity(kind, tier) - capacity);
                if (delta < distance) { distance = delta; best = tier; }
            }
            return best;
        }

        public static int ForNewSite(string id, string siteId, int generation)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Target ID required",nameof(id));
            if (string.IsNullOrEmpty(siteId)) throw new ArgumentException("Verified site required",nameof(siteId));
            if (generation < 1) throw new ArgumentOutOfRangeException(nameof(generation));
            unchecked
            {
                uint hash = 2166136261;
                foreach (char c in id) hash = (hash ^ c) * 16777619;
                foreach (char c in siteId) hash = (hash ^ c) * 16777619;
                hash = (hash ^ (uint)generation) * 16777619;
                hash ^= hash >> 16;
                uint draw = hash % 100;
                // Most nodes should be accessible to new players; scarce,
                // richer sites reward travelling rather than walling off the map.
                if (draw < 45) return 1;
                if (draw < 72) return 2;
                if (draw < 88) return 3;
                if (draw < 97) return 4;
                return 5;
            }
        }
    }
}
