using System;
using System.Collections.Generic;
using System.Linq;

namespace Eldoria.Domain
{
    /// <summary>
    /// V4.1 production-ready, PURE selection rules. This file does not unlock future troop
    /// families, alter the six-round campaign resolver, or issue commands. Callers must
    /// validate the result again in the authoritative gateway before dispatch.
    /// </summary>
    public static class EldoriaV41Planning
    {
        public static readonly string[] Families = { "archer", "paladin", "warlock" };
        public const int MaxHeroSlots = 3;
        static int Safe(Dictionary<string,int> data, string key)
            => data != null && data.TryGetValue(key, out var count) ? Math.Max(0, count) : 0;
        static bool Known(string family) => Families.Contains(family);

        public static string[] SelectHero(string[] current, int slot, string heroId,
            ICollection<string> unlocked)
        {
            if (current == null || current.Length != MaxHeroSlots ||
                slot < 0 || slot >= MaxHeroSlots) throw new ArgumentException("Three hero slots required");
            var next = (string[])current.Clone();
            if (!string.IsNullOrEmpty(heroId) &&
                (unlocked == null || !unlocked.Contains(heroId) ||
                 next.Where((_,i) => i != slot).Contains(heroId)))
                throw new ArgumentException("Hero unavailable or already selected");
            next[slot] = heroId;
            if (string.IsNullOrEmpty(next[0]) && next.Any(h => !string.IsNullOrEmpty(h)))
                throw new ArgumentException("A march leader is required");
            return next;
        }

        /// <summary>
        /// Allocate integer troops using proportional weights and available stock.
        /// Equal remainders are resolved in family order; no hidden troops are created.
        /// </summary>
        static Dictionary<string,int> Allocate(int total, Dictionary<string,int> weights,
            Dictionary<string,int> available, ICollection<string> unlocked)
        {
            var keys = Families.Where(k => unlocked.Contains(k)).ToArray();
            if (total < 0 || keys.Sum(k => Safe(available,k)) < total)
                throw new ArgumentException("Insufficient unlocked troops");
            var output = Families.ToDictionary(k=>k,k=>0);
            int remaining=total;
            while (remaining>0)
            {
                var active=keys.Where(k=>output[k]<Safe(available,k)).ToArray();
                if (active.Length==0) throw new ArgumentException("Insufficient troops");
                double wsum=active.Sum(k=>(double)Safe(weights,k));
                var raw=active.Select(k=>new {
                    Key=k, Room=Safe(available,k)-output[k],
                    Desired=remaining * (wsum>0 ? Safe(weights,k)/wsum : 1.0/active.Length)
                }).ToArray();
                var capped=raw.Where(x=>x.Desired>=x.Room && x.Room>0).ToArray();
                if(capped.Length>0)
                {
                    foreach(var item in capped){output[item.Key]+=item.Room;remaining-=item.Room;}
                    continue;
                }
                foreach(var item in raw)
                {
                    int take=Math.Min(item.Room,(int)Math.Floor(item.Desired));
                    output[item.Key]+=take;remaining-=take;
                }
                foreach(var item in raw.OrderByDescending(x=>x.Desired-Math.Floor(x.Desired))
                         .ThenBy(x=>Array.IndexOf(Families,x.Key)))
                {
                    if(remaining==0)break;
                    if(output[item.Key]<Safe(available,item.Key))
                    {output[item.Key]++;remaining--;}
                }
            }
            return output;
        }

        public static Dictionary<string,int> SetTarget(int desiredTotal,int capacity,
            Dictionary<string,int> counts,Dictionary<string,int> available,ICollection<string> unlocked)
        {
            ValidateInputs(desiredTotal, capacity, available, unlocked);
            return Allocate(desiredTotal,counts,available,unlocked);
        }

        public static Dictionary<string,int> EditCount(string family,int desiredCount,
            int target,int capacity,Dictionary<string,int> counts,
            Dictionary<string,int> available,ICollection<string> unlocked)
        {
            ValidateInputs(target,capacity,available,unlocked);
            if(!Known(family) || !unlocked.Contains(family) || desiredCount<0 ||
                desiredCount>target || desiredCount>Safe(available,family))
                throw new ArgumentException("Invalid unit count");
            var remaining=target-desiredCount;
            var others=Families.Where(k=>k!=family && unlocked.Contains(k)).ToArray();
            if(remaining>0 && others.Length==0)throw new ArgumentException("No other unlocked family");
            var result=Allocate(remaining,counts,available,others);
            result[family]=desiredCount;
            return result;
        }

        public static Dictionary<string,int> EditPercent(string family,double percent,int target,
            int capacity,Dictionary<string,int> counts,Dictionary<string,int> available,
            ICollection<string> unlocked)
        {
            if(double.IsNaN(percent)||double.IsInfinity(percent)||percent<0||percent>100)
                throw new ArgumentOutOfRangeException(nameof(percent));
            // UX: 40 of 200 at 20%; standard midpoint rounding for nonnegative quantities.
            int count=(int)Math.Floor(target*percent/100.0+0.5);
            return EditCount(family,count,target,capacity,counts,available,unlocked);
        }

        public static Dictionary<string,int> Percentages(Dictionary<string,int> counts)
        {
            if(counts==null || Families.Any(k=>counts.TryGetValue(k,out var count) && count<0))
                throw new ArgumentException("Invalid roster");
            int total=Families.Sum(k=>Safe(counts,k));
            var result=Families.ToDictionary(k=>k,k=>0);
            if(total==0)return result;
            var parts=Families.Select(k=>new {
                Key=k, Exact=100.0*Safe(counts,k)/total
            }).ToArray();
            foreach(var part in parts)result[part.Key]=(int)Math.Floor(part.Exact);
            int remaining=100-result.Values.Sum();
            foreach(var part in parts.OrderByDescending(p=>p.Exact-Math.Floor(p.Exact))
                     .ThenBy(p=>Array.IndexOf(Families,p.Key)))
            {
                if(remaining--<=0)break;
                result[part.Key]++;
            }
            return result;
        }

        static void ValidateInputs(int target,int capacity,Dictionary<string,int> available,
            ICollection<string> unlocked)
        {
            if(target<1 || target>capacity || available==null || unlocked==null ||
                unlocked.Any(k=>!Known(k)) ||
                Families.Sum(k=>unlocked.Contains(k)?Safe(available,k):0)<target)
                throw new ArgumentException("March exceeds authorized capacity or stock");
        }
    }

    /// <summary>Immutable-at-write combat summary kept with each inbox item;
    /// older reports must not depend on the mutable LastBattleReport field.</summary>
    [Serializable] public sealed class EldoriaV41CombatRecord
    {
        public string TargetId="",HeroId="",Reason="";
        public int Troops,PlayerPower,EnemyPower,Rounds,RemainingHealth;
        public int RewardWood,RewardStone,RewardFood;
        public bool Won;
        public EldoriaV41CombatRecord Copy() => (EldoriaV41CombatRecord)MemberwiseClone();
    }

    [Serializable] public sealed class EldoriaV41Message
    {
        public string EventId="", Category="", Type="", Title="", Subtitle="";
        public string ReportId="", RewardReceiptId="";
        public long OccurredAtUtcTicks;
        public EldoriaV41CombatRecord Battle;
        public bool Read;
    }

    /// <summary>
    /// Serializable owner-local mailbox. Never pays rewards; reward receipts are references
    /// to the existing authoritative ledger. Persist alongside the player snapshot.
    /// </summary>
    [Serializable] public sealed class EldoriaV41Mailbox
    {
        public int SchemaVersion=1, Revision;
        public List<EldoriaV41Message> Messages=new List<EldoriaV41Message>();
        // Keep tombstones after deletion so replaying an offline event cannot resurrect it.
        public List<string> ReceivedEventIds=new List<string>();

        public EldoriaV41Mailbox Copy()
        {
            var other=new EldoriaV41Mailbox{
                SchemaVersion=SchemaVersion,Revision=Revision,
                ReceivedEventIds=new List<string>(ReceivedEventIds??new List<string>()),
                Messages=new List<EldoriaV41Message>()
            };
            foreach(var m in Messages??new List<EldoriaV41Message>())
                if(m!=null)other.Messages.Add(new EldoriaV41Message{
                    EventId=m.EventId,Category=m.Category,Type=m.Type,
                    Title=m.Title,Subtitle=m.Subtitle,ReportId=m.ReportId,
                    RewardReceiptId=m.RewardReceiptId,OccurredAtUtcTicks=m.OccurredAtUtcTicks,
                    Battle=m.Battle?.Copy(),Read=m.Read
                });
            return other;
        }
        public bool Receive(EldoriaV41Message value)
        {
            if(value==null || string.IsNullOrWhiteSpace(value.EventId) ||
                string.IsNullOrWhiteSpace(value.Title) ||
                !new[]{"informes","recompensas","sistema","narrativa"}.Contains(value.Category))
                throw new ArgumentException("Invalid mailbox event");
            if(ReceivedEventIds.Contains(value.EventId))return false;
            ReceivedEventIds.Add(value.EventId);
            Messages.Add(new EldoriaV41Message {
                EventId=value.EventId, Category=value.Category, Type=value.Type,
                Title=value.Title, Subtitle=value.Subtitle, ReportId=value.ReportId,
                RewardReceiptId=value.RewardReceiptId, OccurredAtUtcTicks=value.OccurredAtUtcTicks,
                Battle=value.Battle?.Copy()
            });
            Revision++;return true;
        }
        public void MarkRead(string eventId)
        {
            var item=Messages.Find(m=>m.EventId==eventId);
            if(item!=null && !item.Read){item.Read=true;Revision++;}
        }
        public void Delete(string eventId)
        {
            int removed=Messages.RemoveAll(m=>m.EventId==eventId);
            if(removed>0)Revision++;
            // ReceivedEventIds intentionally retained.
        }
        public int Unread(string category=null)
            => Messages.Count(m=>!m.Read && (category==null || m.Category==category));
        public List<EldoriaV41Message> List(string category=null,int offset=0,int limit=20)
        {
            if(offset<0 || limit<1 || limit>100)throw new ArgumentOutOfRangeException();
            return Messages.Where(m=>category==null || m.Category==category)
                .OrderByDescending(m=>m.OccurredAtUtcTicks).ThenBy(m=>m.EventId,StringComparer.Ordinal)
                .Skip(offset).Take(limit).ToList();
        }
    }
}
