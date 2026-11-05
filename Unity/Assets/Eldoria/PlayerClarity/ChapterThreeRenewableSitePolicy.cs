using System;

namespace Eldoria.PlayerClarity
{
    /// <summary>
    /// Chapter III's authored one-off geography is immutable. Only the wolf,
    /// boar and meat reserve re-select a safe routed site after each real
    /// generation increment. Generation zero preserves historical placement
    /// for existing saves; live marches must finish before relocation.
    /// </summary>
    public static class ChapterThreeRenewableSitePolicy
    {
        public const int RadialBands = 7;
        public const int BearingsPerBand = 24;
        public const int SiteCount = RadialBands * BearingsPerBand;

        public static bool IsRenewable(string id)
        {
            return id == "hunt-wolf" || id == "hunt-boar" || id == "meat-valoria";
        }

        public static bool CanRelocate(string id, int generationBefore,
            int generationAfter, bool activeMarchToTarget)
        {
            if (!IsRenewable(id)) return false;
            return generationAfter > generationBefore && !activeMarchToTarget;
        }

        public static int CandidateIndex(string id, int generation, int attempt)
        {
            if (!IsRenewable(id))
                throw new ArgumentException("Only Chapter III renewable hunt and meat nodes", nameof(id));
            if (generation < 0) throw new ArgumentOutOfRangeException(nameof(generation));
            if (attempt < 0 || attempt >= SiteCount)
                throw new ArgumentOutOfRangeException(nameof(attempt));
            // The initial placement is already part of previous players' world
            // layout; never teleport it just to change the algorithm.
            if (generation == 0) return attempt;
            return DistributedSiteOrder.Candidate(SiteCount, id, generation, attempt);
        }

        public static int RadiusAt(int candidateIndex)
        {
            if (candidateIndex < 0 || candidateIndex >= SiteCount)
                throw new ArgumentOutOfRangeException(nameof(candidateIndex));
            return 72 + 16 * (candidateIndex / BearingsPerBand);
        }

        public static int BearingAt(int candidateIndex)
        {
            if (candidateIndex < 0 || candidateIndex >= SiteCount)
                throw new ArgumentOutOfRangeException(nameof(candidateIndex));
            return candidateIndex % BearingsPerBand;
        }
    }
}
