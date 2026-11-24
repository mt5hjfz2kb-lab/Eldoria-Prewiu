using System;
using System.Collections.Generic;
using System.Linq;

namespace Eldoria.LegacyProgression
{
    // Future canonical save/server integration contract. This data is NOT written by the client
    // or used for public rankings until hooked into committed authoritative outcomes.
    public enum ReceiptKind { HuntVictory, GatherReturn, ScoutDiscovery, VigilantClear, StrategyClear, DuelResult }
    [Serializable] public sealed class ProgressReceipt
    {
        public string Id="", PlayerId="", WorldId="", SeasonId="", SubjectId="";
        public ReceiptKind Kind;
        public long UtcTicks, Amount;
        public int Level;
        public ProgressReceipt Copy()=>(ProgressReceipt)MemberwiseClone();
    }

    [Serializable] public sealed class LegacyReceiptLedger
    {
        public string PlayerId="", WorldId="";
        public List<ProgressReceipt> Receipts=new List<ProgressReceipt>();

        // Only call inside an accepted server transaction or canonical gateway commit.
        // This method validates identity, shape and uniqueness, NOT gameplay authenticity.
        public bool RecordCommitted(ProgressReceipt r)
        {
            if(r==null||string.IsNullOrWhiteSpace(PlayerId)||string.IsNullOrWhiteSpace(WorldId)||
                r.PlayerId!=PlayerId||r.WorldId!=WorldId||string.IsNullOrWhiteSpace(r.Id)||
                string.IsNullOrWhiteSpace(r.SubjectId)||r.UtcTicks<=0||r.UtcTicks>DateTime.MaxValue.Ticks||
                r.Amount<=0||!Enum.IsDefined(typeof(ReceiptKind),r.Kind))return false;
            if(r.Kind==ReceiptKind.HuntVictory&&(r.Amount!=1||r.Level<1||r.Level>5))return false;
            if(r.Kind==ReceiptKind.ScoutDiscovery&&(r.Amount!=1||r.Level!=0))return false;
            if(r.Kind==ReceiptKind.VigilantClear&&(r.Amount!=1||r.Level<1||r.Level>10000))return false;
            if(Receipts==null)Receipts=new List<ProgressReceipt>();
            if(Receipts.Any(x=>x!=null&&x.Id==r.Id))return false;
            Receipts.Add(r.Copy()); return true;
        }

        public long HuntPoints(long startInclusive,long endExclusive)
        {
            if(startInclusive>=endExclusive)throw new ArgumentException("Invalid UTC interval");
            long points=0;
            foreach(var r in Window(startInclusive,endExclusive).Where(x=>x.Kind==ReceiptKind.HuntVictory))
                points=checked(points+r.Level);
            return points;
        }

        public long Gathered(long startInclusive,long endExclusive)
        {
            if(startInclusive>=endExclusive)throw new ArgumentException("Invalid UTC interval");
            long total=0;
            foreach(var r in Window(startInclusive,endExclusive).Where(x=>x.Kind==ReceiptKind.GatherReturn))
                total=checked(total+r.Amount);
            return total;
        }

        public int UniqueDiscoveries(long startInclusive,long endExclusive)
        {
            if(startInclusive>=endExclusive)throw new ArgumentException("Invalid UTC interval");
            return Window(startInclusive,endExclusive).Where(x=>x.Kind==ReceiptKind.ScoutDiscovery)
                .Select(x=>x.SubjectId).Distinct(StringComparer.Ordinal).Count();
        }

        IEnumerable<ProgressReceipt> Window(long start,long end) =>
            (Receipts??new List<ProgressReceipt>()).Where(r=>r!=null&&r.PlayerId==PlayerId&&
                r.WorldId==WorldId&&r.UtcTicks>=start&&r.UtcTicks<end)
                .GroupBy(r=>r.Id,StringComparer.Ordinal).Select(g=>g.First());

        public static DateTime WeekStartUtc(DateTime utc)
        {
            if(utc.Kind!=DateTimeKind.Utc)throw new ArgumentException("UTC required");
            return utc.Date.AddDays(-((7+(int)utc.DayOfWeek-(int)DayOfWeek.Monday)%7));
        }
    }
}
