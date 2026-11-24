using System;
using System.Collections.Generic;
using System.Linq;

namespace Eldoria.LegacyProgression
{
    // The future server supplies scores calculated from its own canonical state.
    // This is a deterministic sorting/pagination contract, not an authentication mechanism.
    public sealed class RankingScope
    {
        public string WorldId="", PeriodId="";
        public RankingKind Kind;
        public RankingPeriod Period;
        public long StartUtcTicks, EndUtcTicks;
    }

    public sealed class VerifiedScore
    {
        public string PlayerId="", DisplayName="", WorldId="", PeriodId="";
        public RankingKind Kind;
        public long Score, AchievedUtcTicks;
    }

    public sealed class RankingRow
    {
        public readonly int Position;
        public readonly string PlayerId,DisplayName;
        public readonly long Score;
        public RankingRow(int position,VerifiedScore score)
        {Position=position;PlayerId=score.PlayerId;DisplayName=score.DisplayName;Score=score.Score;}
    }

    public static class VerifiedRankings
    {
        // Reject ambiguous duplicate current scores instead of silently taking the larger.
        public static IReadOnlyList<RankingRow> Build(RankingScope scope,IEnumerable<VerifiedScore> authoritativeScores)
        {
            if(scope==null||authoritativeScores==null||string.IsNullOrWhiteSpace(scope.WorldId)||
                string.IsNullOrWhiteSpace(scope.PeriodId)||scope.StartUtcTicks<0||
                scope.EndUtcTicks<=scope.StartUtcTicks||scope.EndUtcTicks>DateTime.MaxValue.Ticks||
                !Enum.IsDefined(typeof(RankingKind),scope.Kind)||!Enum.IsDefined(typeof(RankingPeriod),scope.Period))
                throw new ArgumentException("Invalid ranking scope");
            var rows=authoritativeScores.Where(s=>s!=null&&s.WorldId==scope.WorldId&&
                s.PeriodId==scope.PeriodId&&s.Kind==scope.Kind).ToArray();
            if(rows.Any(s=>string.IsNullOrWhiteSpace(s.PlayerId)||string.IsNullOrWhiteSpace(s.DisplayName)||
                s.Score<0||s.AchievedUtcTicks<scope.StartUtcTicks||s.AchievedUtcTicks>=scope.EndUtcTicks))
                throw new ArgumentException("Invalid authoritative score");
            if(rows.GroupBy(s=>s.PlayerId,StringComparer.Ordinal).Any(g=>g.Count()>1))
                throw new ArgumentException("Duplicate authoritative score");
            return rows.OrderByDescending(s=>s.Score).ThenBy(s=>s.AchievedUtcTicks)
                .ThenBy(s=>s.PlayerId,StringComparer.Ordinal).Select((s,i)=>new RankingRow(i+1,s)).ToArray();
        }
    }
}
