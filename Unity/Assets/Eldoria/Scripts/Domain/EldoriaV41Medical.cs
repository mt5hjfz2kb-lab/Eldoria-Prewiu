using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Eldoria.Domain
{
    // Pure transaction planner. The gateway must persist State and Returned together.
    // No severity rates, hospital costs/unlocks, currency or production activation here.
    public static class EldoriaV41Medical
    {
        [Serializable] public sealed class Treatment
        {
            public string EventId;
            public long EndsUtcTicks;
            public ArmyRoster Troops=new ArmyRoster();
            public Treatment Copy()=>new Treatment{EventId=EventId,EndsUtcTicks=EndsUtcTicks,Troops=Troops.Copy()};
        }
        [Serializable] public sealed class Receipt
        {
            public string EventId,Payload;
            public long ArrivalUtcTicks;
            public ArmyRoster Ready,Admitted,DirectDeaths,OverflowDeaths;
            public Receipt Copy()=>new Receipt{EventId=EventId,Payload=Payload,ArrivalUtcTicks=ArrivalUtcTicks,
                Ready=Ready.Copy(),Admitted=Admitted.Copy(),DirectDeaths=DirectDeaths.Copy(),OverflowDeaths=OverflowDeaths.Copy()};
        }
        [Serializable] public sealed class Hospital
        {
            public int SchemaVersion=1,Capacity;
            // Set only by a validated, persistent hospital-unlock transition, not a visual building.
            public bool FeatureOperational;
            public long Revision,ThroughUtcTicks;
            public string LastArrivalId="";
            public List<Treatment> Treatments=new List<Treatment>();
            public List<Receipt> Receipts=new List<Receipt>();
            public Hospital Copy()=>new Hospital{SchemaVersion=SchemaVersion,Capacity=Capacity,
                FeatureOperational=FeatureOperational,Revision=Revision,ThroughUtcTicks=ThroughUtcTicks,
                LastArrivalId=LastArrivalId,Treatments=Treatments.ConvertAll(x=>x.Copy()),Receipts=Receipts.ConvertAll(x=>x.Copy())};
        }
        public sealed class Arrival
        {
            public string EventId,Mode;
            public long UtcTicks,TreatmentTicks;
            // Integer aftermath from an independently calibrated injury policy. Not HP values.
            public ArmyRoster Deployed,Graves,DirectDeaths;
        }
        public sealed class Plan
        {
            public Hospital State;
            public ArmyRoster Returned=new ArmyRoster();
            public List<Receipt> Applied=new List<Receipt>();
        }
        static void ValidateRoster(ArmyRoster r)
        {
            if(r==null || !r.IsNonNegative)throw new ArgumentException("Invalid medical roster");
            _=r.Total; // checked total; reject overflow before any state is changed
        }
        static void Add(ArmyRoster to,ArmyRoster from)
        {for(int f=0;f<3;f++)for(int t=1;t<=5;t++)to.Set(f,t,checked(to.Get(f,t)+from.Get(f,t)));}
        static string Payload(Arrival a)
        {
            if(a==null || string.IsNullOrWhiteSpace(a.EventId) || (a.Mode!="pve"&&a.Mode!="pvp") ||
                a.UtcTicks<=0 || a.TreatmentTicks<=0)throw new ArgumentException("Invalid medical event");
            ValidateRoster(a.Deployed);ValidateRoster(a.Graves);ValidateRoster(a.DirectDeaths);
            if(a.Mode=="pve" && a.DirectDeaths.Total!=0)throw new ArgumentException("PvE cannot cause direct deaths");
            var s=new StringBuilder(a.Mode).Append('|').Append(a.UtcTicks).Append('|').Append(a.TreatmentTicks);
            for(int f=0;f<3;f++)for(int t=1;t<=5;t++)
            {
                int n=a.Deployed.Get(f,t),g=a.Graves.Get(f,t),d=a.DirectDeaths.Get(f,t);
                if((long)g+d>n)throw new ArgumentException("Medical outcomes exceed deployed troops");
                s.Append('|').Append(n).Append(',').Append(g).Append(',').Append(d);
            }
            return s.ToString();
        }
        static void Discharge(Plan plan,long now)
        {
            foreach(var treatment in plan.State.Treatments.Where(x=>x.EndsUtcTicks<=now).ToArray())
            {Add(plan.Returned,treatment.Troops);plan.State.Treatments.Remove(treatment);}
        }
        static ArmyRoster Allocate(ArmyRoster graves,int beds)
        {
            var result=new ArmyRoster();int total=graves.Total,take=Math.Min(total,beds);
            if(total==0 || take==0)return result;
            var remainders=new List<(int family,int tier,long remainder)>();int assigned=0;
            for(int f=0;f<3;f++)for(int t=1;t<=5;t++)
            {
                long scaled=(long)graves.Get(f,t)*take;int n=(int)(scaled/total);
                result.Set(f,t,n);assigned+=n;remainders.Add((f,t,scaled%total));
            }
            foreach(var row in remainders.OrderByDescending(x=>x.remainder).ThenBy(x=>x.family).ThenBy(x=>x.tier))
            {if(assigned>=take)break;result.Set(row.family,row.tier,result.Get(row.family,row.tier)+1);assigned++;}
            return result;
        }
        public static Plan PlanReturns(Hospital current,IEnumerable<Arrival> arrivals,long throughUtcTicks)
        {
            if(current==null || current.Capacity<0 || current.Treatments==null || current.Receipts==null ||
                arrivals==null || throughUtcTicks<current.ThroughUtcTicks)throw new ArgumentException("Invalid hospital state/time");
            if(current.Receipts.Any(x=>x==null) || current.Receipts.Select(x=>x.EventId).Distinct(StringComparer.Ordinal).Count()!=current.Receipts.Count)
                throw new ArgumentException("Invalid medical receipts");
            long occupied=0;
            foreach(var t in current.Treatments)
            {
                if(t==null || t.EndsUtcTicks<=0)throw new ArgumentException("Invalid treatment");
                ValidateRoster(t.Troops);occupied+=t.Troops.Total;
            }
            if(occupied>current.Capacity)throw new ArgumentException("Hospital capacity below admitted troops");
            var list=arrivals.ToList();foreach(var a in list)Payload(a);
            var plan=new Plan{State=current.Copy()};
            foreach(var a in list.OrderBy(x=>x.UtcTicks).ThenBy(x=>x.EventId,StringComparer.Ordinal))
            {
                string payload=Payload(a);
                var prior=plan.State.Receipts.Find(x=>x.EventId==a.EventId);
                if(prior!=null)
                {if(prior.Payload!=payload)throw new ArgumentException("Medical event reused with different payload");continue;}
                if(a.UtcTicks<plan.State.ThroughUtcTicks || a.UtcTicks>throughUtcTicks ||
                    (a.UtcTicks==plan.State.ThroughUtcTicks && StringComparer.Ordinal.Compare(a.EventId,plan.State.LastArrivalId)<0))
                    throw new ArgumentException("Medical arrivals must be reconciled in chronological order");
                Discharge(plan,a.UtcTicks);
                int beds=plan.State.Capacity-plan.State.Treatments.Sum(x=>x.Troops.Total);
                var graves=plan.State.FeatureOperational?a.Graves.Copy():new ArmyRoster();
                var direct=plan.State.FeatureOperational?a.DirectDeaths.Copy():new ArmyRoster();
                var admitted=Allocate(graves,beds);var ready=new ArmyRoster();var overflow=new ArmyRoster();
                for(int f=0;f<3;f++)for(int t=1;t<=5;t++)
                {
                    ready.Set(f,t,a.Deployed.Get(f,t)-graves.Get(f,t)-direct.Get(f,t));
                    overflow.Set(f,t,graves.Get(f,t)-admitted.Get(f,t));
                }
                // Owner decision 2026-11-05: graves without beds die in both PvE and PvP.
                var receipt=new Receipt{EventId=a.EventId,Payload=payload,ArrivalUtcTicks=a.UtcTicks,
                    Ready=ready,Admitted=admitted,DirectDeaths=direct,OverflowDeaths=overflow};
                if(admitted.Total>0)plan.State.Treatments.Add(new Treatment{EventId=a.EventId,
                    EndsUtcTicks=checked(a.UtcTicks+a.TreatmentTicks),Troops=admitted.Copy()});
                plan.State.Receipts.Add(receipt);plan.Applied.Add(receipt.Copy());Add(plan.Returned,ready);
                plan.State.ThroughUtcTicks=a.UtcTicks;plan.State.LastArrivalId=a.EventId;
            }
            Discharge(plan,throughUtcTicks);
            if(throughUtcTicks>plan.State.ThroughUtcTicks)plan.State.LastArrivalId="";
            plan.State.ThroughUtcTicks=throughUtcTicks;plan.State.Revision=checked(current.Revision+1);
            return plan;
        }
    }
}
