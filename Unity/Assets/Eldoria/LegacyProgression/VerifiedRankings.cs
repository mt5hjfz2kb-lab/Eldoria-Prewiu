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

    public enum RankingDetailStatus { Unavailable, Loading, Ready, Error }

    // A server-owned, already authenticated READ snapshot. No local score is promoted to public rank.
    // The injected service is responsible for the world/period and identity authorization;
    // this value verifies the display contract without performing any network request.
    public sealed class RankingDetailSnapshot
    {
        public RankingKind Kind { get; }
        public RankingDetailStatus Status { get; }
        public string PeriodLabel { get; }
        public IReadOnlyList<RankingRow> Rows { get; }
        public RankingRow OwnRow { get; }
        public RankingDetailSnapshot(RankingKind kind,RankingDetailStatus status,string periodLabel="",IEnumerable<RankingRow> rows=null,RankingRow ownRow=null)
        {
            if(!Enum.IsDefined(typeof(RankingKind),kind)||!Enum.IsDefined(typeof(RankingDetailStatus),status))throw new ArgumentException("Invalid ranking detail");
            var list=(rows??Array.Empty<RankingRow>()).ToArray();
            if(status!=RankingDetailStatus.Ready&&(list.Length!=0||ownRow!=null))throw new ArgumentException("Unready ranking cannot show standings");
            if(status==RankingDetailStatus.Ready&&string.IsNullOrWhiteSpace(periodLabel))throw new ArgumentException("Ready ranking requires real period label");
            if(list.Any(r=>!Valid(r))||list.Select(r=>r.PlayerId).Distinct(StringComparer.Ordinal).Count()!=list.Length||
                list.Where((r,i)=>r.Position!=i+1).Any()||list.Zip(list.Skip(1),(a,b)=>a.Score<b.Score).Any(x=>x))throw new ArgumentException("Invalid first page ordering");
            if(ownRow!=null&&(!Valid(ownRow)||list.Length==0))throw new ArgumentException("Invalid own ranking");
            if(ownRow!=null)
            {
                var existing=list.FirstOrDefault(r=>r.PlayerId==ownRow.PlayerId);
                if(existing!=null&&(existing.Position!=ownRow.Position||existing.Score!=ownRow.Score||existing.DisplayName!=ownRow.DisplayName))throw new ArgumentException("Inconsistent own ranking");
                if(existing==null&&ownRow.Position<=list.Length)throw new ArgumentException("Own ranking conflicts with first page");
            }
            Kind=kind;Status=status;PeriodLabel=periodLabel??"";Rows=Array.AsReadOnly(list);OwnRow=ownRow;
        }
        static bool Valid(RankingRow r)=>r!=null&&r.Position>0&&r.Score>=0&&!string.IsNullOrWhiteSpace(r.PlayerId)&&!string.IsNullOrWhiteSpace(r.DisplayName);
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
