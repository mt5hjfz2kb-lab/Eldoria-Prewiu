// Eldoria V4.1 evolutionary candidate (pure domain, no combat scene / no Unity UI).
// ADDITIVE to EldoriaV41Combat; DOES NOT activate the experimental resolver.
// Production must validate roster/tiers/heroes in LocalGateway before calling Resolve.
// Source parameters are proposals. Preserve old V4.1 for save/version compatibility.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Eldoria.Domain
{
    public static class EldoriaV41CombatEvolution
    {
        public const string RulesVersion = "v4.1-evolution-candidate-t1-t5";
        public const bool ProductionApproved = false;
        public const int Families = 3, Tiers = 5;
        const double Epsilon = 1e-9;
        const double CounterBonus = 1.40, TacticalBonus = .65;
        const double Guard = .24, GuardBudget = .35, WarlockBypass = .75;
        static readonly double[] Tier = { 1.0, 1.55, 2.40, 3.72, 5.77 };
        static readonly double[] Attack = { 9.0, 13.0, 11.0 };
        static readonly double[] Defense = { 16.0, 7.0, 8.0 };
        static readonly double[] Health = { 72.0, 42.0, 49.0 };
        static readonly double[] Rupture = { 3.0, 7.0, 17.0 };
        // Same V4.1 cycle: Paladines > Arqueros > Brujos > Paladines.
        static readonly int[,] Counter = {
            { 0, 1, -1 }, { -1, 0, 1 }, { 1, -1, 0 }
        };
        static readonly string[] FamilyLabels = { "Paladines", "Arqueros", "Brujos" };
        public sealed class Force
        {
            public int[,] Troops = new int[Families,Tiers];
            // Real slots only. 3rd hero is empty until there is an approved real hero.
            public string[] HeroIds = { "aldric", null, null };
            public Force Add(int family, int tier, int amount)
            {
                if (family < 0 || family >= Families || tier < 1 || tier > Tiers || amount < 0)
                    throw new ArgumentOutOfRangeException("Invalid family, tier or amount");
                Troops[family,tier-1] = checked(Troops[family,tier-1] + amount);
                return this;
            }
            public Force WithHeroes(string leader, string second = null, string third = null)
            { HeroIds = new [] { leader, second, third }; return this; }
            public int Count
            {
                get { int total=0; foreach (int x in Troops) total=checked(total+x); return total; }
            }
        }
        // Mapping only: the existing ArmyRoster remains the single persisted authority.
        public static Force FromRoster(ArmyRoster roster, string leader, string second=null)
        {
            if(roster==null || !roster.IsNonNegative)throw new ArgumentException("Invalid authoritative roster");
            var force=new Force().WithHeroes(leader,second);
            for(int f=0;f<Families;f++)for(int t=1;t<=Tiers;t++)force.Add(f,t,roster.Get(f,t));
            Validate(force);
            return force;
        }
        static ArmyRoster ToRoster(Force force)
        {
            var roster=new ArmyRoster();
            for(int f=0;f<Families;f++)for(int t=1;t<=Tiers;t++)roster.Set(f,t,force.Troops[f,t-1]);
            return roster;
        }
        public sealed class Turn
        {
            public int Number;
            public double DamageA, DamageB, HpA, HpB;
        }
        public sealed class Result
        {
            public string Rules = RulesVersion;
            public int Winner; // 1 A, -1 B, 0 mathematical draw
            public string ReasonCode;
            public int Rounds;
            public double RemainingA, RemainingB;
            public double[] DealtA, DealtB;
            public double[,] DamagedHpA, DamagedHpB;
            public double[] InitialHpA;
            public double AldricBlockedA, AldricBlockedB;
            public double CounterAdvantage;
            public double LyraFirstStrikeA, LyraFirstStrikeB;
            public List<Turn> Turns = new List<Turn>();
        }
        sealed class Side
        {
            public string[] Heroes;
            public bool Creature;
            public double[] Hp = new double[Families*Tiers], Initial = new double[Families*Tiers],
                HpPer = new double[Families*Tiers], Atk = new double[Families*Tiers],
                Def = new double[Families*Tiers], Rup = new double[Families*Tiers];
            public double[] Damage = new double[Families];
            public double ShieldBlocked, FirstStrike;
            public double CurrentHp { get { return Hp.Sum(); } }
            public double InitialHp { get { return Initial.Sum(); } }
        }
        public static void ValidateForce(Force force) => Validate(force);
        static void Validate(Force f)
        {
            if (f == null || f.Troops == null || f.Troops.GetLength(0) != Families ||
                f.Troops.GetLength(1) != Tiers || f.HeroIds == null || f.HeroIds.Length != 3)
                throw new ArgumentException("V4.1 requires 3 families, 5 tiers and 3 hero slots");
            if (f.HeroIds[0] != "aldric" && f.HeroIds[0] != "lyra")
                throw new ArgumentException("Unavailable march leader");
            var used = new HashSet<string>(StringComparer.Ordinal);
            foreach (string h in f.HeroIds)
                if (!string.IsNullOrEmpty(h) && ((h != "aldric" && h != "lyra") || !used.Add(h)))
                    throw new ArgumentException("Unknown or duplicated hero");
            foreach (int count in f.Troops) if (count < 0 || count > 1000000)
                throw new ArgumentException("Invalid troop count");
            if (f.Count < 1) throw new ArgumentException("Empty army");
        }
        static Side Build(Force f)
        {
            Validate(f);
            var s=new Side { Heroes=(string[])f.HeroIds.Clone() };
            bool hasLyra=Array.IndexOf(s.Heroes,"lyra")>=0;
            bool hasAldric=Array.IndexOf(s.Heroes,"aldric")>=0;
            for (int family=0;family<Families;family++) for (int tier=0;tier<Tiers;tier++)
            {
                int i=family*Tiers+tier;
                s.HpPer[i]=Health[family]*Tier[tier];
                s.Hp[i]=s.Initial[i]=f.Troops[family,tier]*s.HpPer[i];
                s.Atk[i]=Attack[family]*Tier[tier]*(hasLyra && family==1?1.03:1.0);
                s.Def[i]=Defense[family]*Tier[tier]*(hasAldric && family==0?1.03:1.0);
                s.Rup[i]=Rupture[family]*Tier[tier];
            }
            return s;
        }
        // No abrupt 50% saturation: Warlock T3/T4/T5 progressively pierce armor.
        public static double RuptureFraction(double rupture)
        {
            if (double.IsNaN(rupture) || double.IsInfinity(rupture) || rupture < 0)
                throw new ArgumentOutOfRangeException(nameof(rupture));
            return .64*rupture/(rupture+45.0);
        }
        static bool Has(Side s, string id) => Array.IndexOf(s.Heroes,id)>=0;
        static double CounterFactor(int from, int to)
            => Counter[from,to] == 1 ? CounterBonus : 1.0;
        static double Coherence(Force a, Force b)
        {
            double[] x=new double[Families],y=new double[Families];
            for (int f=0;f<Families;f++) for(int t=0;t<Tiers;t++)
            { x[f]+=a.Troops[f,t]; y[f]+=b.Troops[f,t]; }
            double sx=x.Sum(), sy=y.Sum(), coherence=0;
            for(int i=0;i<Families;i++) for(int j=0;j<Families;j++)
                coherence+=(x[i]/sx)*Counter[i,j]*(y[j]/sy);
            return coherence;
        }
        static double[,] Outgoing(Side src,Side dst,double tactical)
        {
            int size=Families*Tiers;
            var damage=new double[size,size];
            for (int i=0;i<size;i++)
            {
                double living=src.Hp[i]/src.HpPer[i];
                if(living<=Epsilon) continue;
                double denom=0;
                for(int j=0;j<size;j++)
                    denom+=Math.Pow((src.Creature || dst.Creature ? 1.0 : CounterFactor(i/Tiers,j/Tiers)),.6)*dst.Hp[j]/dst.HpPer[j];
                if(denom<=Epsilon)continue;
                for (int j=0;j<size;j++)
                    damage[i,j]=living*src.Atk[i]*
                        Math.Pow((src.Creature || dst.Creature ? 1.0 : CounterFactor(i/Tiers,j/Tiers)),.6)*
                        (dst.Hp[j]/dst.HpPer[j])/denom;
            }
            double paladinHp=0,desired=0;
            for(int k=0;k<Tiers;k++)paladinHp+=dst.Hp[k];
            for(int i=0;i<size;i++)for(int j=Tiers;j<size;j++)
                desired+=damage[i,j]*Guard*(j>=2*Tiers?1-WarlockBypass:1);
            double ratio=desired>Epsilon?Math.Min(1,paladinHp*GuardBudget/desired):0;
            if(paladinHp>Epsilon && ratio>0)
                for(int i=0;i<size;i++)for(int j=Tiers;j<size;j++)
                {
                    double amount=damage[i,j]*Guard*(j>=2*Tiers?1-WarlockBypass:1)*ratio;
                    damage[i,j]-=amount;
                    for(int k=0;k<Tiers;k++)damage[i,k]+=amount*dst.Hp[k]/paladinHp;
                }
            for(int i=0;i<size;i++)for(int j=0;j<size;j++)
            {
                double penetration=RuptureFraction(src.Rup[i]);
                double mitigation=100.0/(100.0+4*dst.Def[j]*(1-penetration));
                damage[i,j]*=(src.Creature || dst.Creature ? 1.0 : CounterFactor(i/Tiers,j/Tiers))*tactical*mitigation;
            }
            return damage;
        }
        static double Sum(double[,] matrix)
        {
            double d=0;foreach(double value in matrix)d+=value;return d;
        }
        static void Scale(double[,] matrix,double factor)
        {
            for(int i=0;i<matrix.GetLength(0);i++)for(int j=0;j<matrix.GetLength(1);j++)
                matrix[i,j]*=factor;
        }
        static double Apply(Side src, Side dst, double[,] damage)
        {
            double effectiveTotal=0;
            int size=Families*Tiers;
            for(int j=0;j<size;j++)
            {
                double raw=0;
                for(int i=0;i<size;i++)raw+=damage[i,j];
                if(raw<=Epsilon)continue;
                double factor=Math.Min(1,dst.Hp[j]/raw);
                for(int i=0;i<size;i++)
                {
                    double dealt=damage[i,j]*factor;
                    effectiveTotal+=dealt;src.Damage[i/Tiers]+=dealt;
                }
                dst.Hp[j]=Math.Max(0,dst.Hp[j]-raw);
            }
            return effectiveTotal;
        }
        static double Lyra(Side s,Side foe,double[,] outgoing)
        {
            if(!Has(s,"lyra"))return 0;
            double raw=Sum(outgoing),add=Math.Min(.025*foe.InitialHp,.25*raw);
            if(raw>Epsilon)Scale(outgoing,1+add/raw);
            return raw>Epsilon?add:0;
        }
        static double Aldric(Side s,double[,] incoming)
        {
            if(!Has(s,"aldric"))return 0;
            double raw=Sum(incoming),block=Math.Min(.45*raw,.028*s.InitialHp);
            if(raw>Epsilon)Scale(incoming,1-block/raw);
            return raw>Epsilon?block:0;
        }
        static double[,] Damaged(Side side)
        {
            var diff=new double[Families,Tiers];
            for(int f=0;f<Families;f++)for(int t=0;t<Tiers;t++)
            {int i=f*Tiers+t;diff[f,t]=Math.Max(0,side.Initial[i]-side.Hp[i]);}
            return diff;
        }
        // Never renders any combat. A turn is a calculation step, not a visual frame.
        public static Result Resolve(Force a,Force b,int maxRounds=120,bool heroSkills=true)
        {
            if(maxRounds<1 || maxRounds>1000)throw new ArgumentOutOfRangeException(nameof(maxRounds));
            return ResolveSides(Build(a),Build(b),Coherence(a,b),maxRounds,heroSkills);
        }
        // Creatures retain their authored finite stats. They are not assigned a
        // fictitious troop family, tier, hero, or counter advantage.
        public static Result ResolveCreature(Force player, WorldCreatureStats creature, int maxRounds=120, int masteryLevel=0, bool ambush=false)
        {
            if(creature.Health<=0 || creature.Attack<0 || creature.Defense<0 || creature.Break<0 || maxRounds<1 || maxRounds>1000)
                throw new ArgumentException("Invalid real creature profile");
            var enemy=new Side{Heroes=Array.Empty<string>(),Creature=true};
            for(int i=0;i<Families*Tiers;i++)enemy.HpPer[i]=1;
            enemy.Hp[0]=enemy.Initial[0]=enemy.HpPer[0]=creature.Health;
            enemy.Atk[0]=creature.Attack;enemy.Def[0]=creature.Defense;enemy.Rup[0]=creature.Break;
            var own=Build(player);
            if(masteryLevel<0 || masteryLevel>20)throw new ArgumentOutOfRangeException(nameof(masteryLevel));
            if(masteryLevel>0)
            {
                // Preserve the existing progression's proportional stat benefit.
                // Soldier profiles remain immutable; no new XP curve or coefficients.
                // Scale earned mastery from Aldric's original solo baseline
                // when he fights alongside Lyra. Using both heroes' combined
                // baseline would dilute the same mastery levels upon pairing.
                var hero=Has(own,"aldric")?"aldric":player.HeroIds[0];
                var baseline=SliceRules.Expedition(ToRoster(player),hero);
                double attack=(baseline.Attack+masteryLevel*4.0)/baseline.Attack;
                double defense=(baseline.Defense+masteryLevel*3.0)/baseline.Defense;
                double health=(baseline.Health+masteryLevel*15.0)/baseline.Health;
                double rupture=(baseline.Break+masteryLevel*2.0)/baseline.Break;
                for(int i=0;i<Families*Tiers;i++)
                {
                    own.Atk[i]*=attack;own.Def[i]*=defense;own.Rup[i]*=rupture;
                    own.HpPer[i]*=health;own.Hp[i]*=health;own.Initial[i]*=health;
                }
            }
            if(ambush)
            {
                var opening=Outgoing(enemy,own,1);
                Scale(opening,1.42);Apply(enemy,own,opening);
                enemy.Def[0]=Math.Round(enemy.Def[0]*.82,MidpointRounding.AwayFromZero);
            }
            return ResolveSides(own,enemy,0,maxRounds,true,ambush);
        }
        static Result ResolveSides(Side x,Side y,double coherence,int maxRounds,bool heroSkills,bool ambushRecovery=false)
        {
            var turns=new List<Turn>();
            for(int round=1;round<=maxRounds && x.CurrentHp>Epsilon && y.CurrentHp>Epsilon;round++)
            {
                var xy=Outgoing(x,y,1+TacticalBonus*coherence);
                var yx=Outgoing(y,x,1-TacticalBonus*coherence);
                if(ambushRecovery && round==1)Scale(xy,1.08);
                if(heroSkills && round==1)
                {
                    x.FirstStrike=Lyra(x,y,xy);y.FirstStrike=Lyra(y,x,yx);
                    x.ShieldBlocked=Aldric(x,yx);y.ShieldBlocked=Aldric(y,xy);
                }
                // Both attack matrices are computed BEFORE either HP changes.
                double dx=Apply(x,y,xy),dy=Apply(y,x,yx);
                turns.Add(new Turn {Number=round,DamageA=dx,DamageB=dy,HpA=x.CurrentHp,HpB=y.CurrentHp});
            }
            double leftA=x.CurrentHp,leftB=y.CurrentHp;
            int winner=leftA>Epsilon && leftB<=Epsilon?1:leftB>Epsilon && leftA<=Epsilon?-1:0;
            string cause=winner==1?"enemy_eliminated":winner==-1?"army_eliminated":
                leftA<=Epsilon&&leftB<=Epsilon?"mutual_elimination":"round_limit";
            return new Result {
                Winner=winner,ReasonCode=cause,Rounds=turns.Count,
                RemainingA=leftA/x.InitialHp,RemainingB=leftB/y.InitialHp,
                DealtA=(double[])x.Damage.Clone(),DealtB=(double[])y.Damage.Clone(),
                CounterAdvantage=coherence,
                DamagedHpA=Damaged(x),DamagedHpB=Damaged(y),InitialHpA=(double[])x.Initial.Clone(),
                AldricBlockedA=x.ShieldBlocked,AldricBlockedB=y.ShieldBlocked,
                LyraFirstStrikeA=x.FirstStrike,LyraFirstStrikeB=y.FirstStrike,Turns=turns
            };
        }
        // LocalGateway calls this ONCE for every authorised battle, witnessed or not.
        // A single authoritative state transaction must also persist the command ID
        // and any EXISTING ledger receipt. Replays return false and never pay twice.
        public static bool AppendCombatReport(EldoriaV41Mailbox mailbox, Result result,
            Force player, Force opponent, string eventId, string targetId,
            long occurredUtcTicks, string alreadyCommittedReceiptId = "")
        {
            if (mailbox == null) throw new ArgumentNullException(nameof(mailbox));
            return mailbox.Receive(ToMailbox(result,player,opponent,eventId,targetId,
                occurredUtcTicks,alreadyCommittedReceiptId));
        }

        // Only player-observable facts. These fields are for the optional
        // "Mi marcha" panel approved in V4.1, never shown in the mail list.
        static bool OwnFamilyPresent(Force player,int family)
        { return Enumerable.Range(0,Tiers).Any(t=>player.Troops[family,t]>0); }
        static string OwnFamilyDamage(double[] totals,Force player)
        {
            if(totals==null || totals.Length!=Families)throw new ArgumentException("Bad own damage");
            return string.Join(" | ",Enumerable.Range(0,Families).Where(f=>OwnFamilyPresent(player,f)).Select(f=>
                FamilyLabels[f]+": "+totals[f].ToString("F1",CultureInfo.InvariantCulture)).ToArray());
        }
        static string OwnReceivedDamage(Result result,Force player)
        {
            var values=new double[Families];
            for(int f=0;f<Families;f++)for(int t=0;t<Tiers;t++)
                values[f]+=result.DamagedHpA[f,t];
            return OwnFamilyDamage(values,player);
        }
        static string OwnFamilyRemaining(Result result,Force player)
        {
            var values=new List<string>();
            for(int f=0;f<Families;f++)
            {
                if(!OwnFamilyPresent(player,f))continue;
                double initial=0,lost=0;
                for(int t=0;t<Tiers;t++)
                { initial+=result.InitialHpA==null?player.Troops[f,t]*Health[f]*Tier[t]:result.InitialHpA[f*Tiers+t];lost+=result.DamagedHpA[f,t]; }
                values.Add(FamilyLabels[f]+": "+(initial>0?
                    (100*Math.Max(0,initial-lost)/initial).ToString("F1",CultureInfo.InvariantCulture)+"%": "sin tropas"));
            }
            return string.Join(" | ",values);
        }

        // Build a V4.1-compatible mailbox report; existing mailbox is the only mail store.
        // This method DOES NOT grant currency, recover troops or write a save.
        // Gateway must provide an event ID derived from the authoritative battle identity.
        public static EldoriaV41Message ToMailbox(Result r,Force player,Force opponent,
            string eventId,string targetId,long occurredUtcTicks,string committedRewardReceiptId="")
        {
            if(opponent==null)throw new ArgumentNullException(nameof(opponent));
            Validate(opponent);
            return PublicReport(r,player,eventId,targetId,occurredUtcTicks,committedRewardReceiptId);
        }
        public static EldoriaV41Message ToCreatureMailbox(Result r,Force player,string eventId,string targetId,long occurredUtcTicks)
            => PublicReport(r,player,eventId,targetId,occurredUtcTicks,"");
        static EldoriaV41Message PublicReport(Result r,Force player,string eventId,string targetId,long occurredUtcTicks,string committedRewardReceiptId)
        {
            if(r==null || player==null || string.IsNullOrEmpty(eventId) ||
                string.IsNullOrEmpty(targetId) || occurredUtcTicks<=0)
                throw new ArgumentException("Authoritative battle identity is required");
            Validate(player);
            // PLAYER-VISIBLE information only: keep mathematical internals in Result.
            // Never expose opponent troop matrices, tactical coefficients, per-family
            // damage dealt, ability multipliers or raw turn traces in the mailbox.
            // A useful report describes observations, not the optimal strategy.
            var lineup=new StringBuilder();
            for(int family=0;family<Families;family++)
                for(int tier=0;tier<Tiers;tier++)
                    if(player.Troops[family,tier]>0)
                    {
                        if(lineup.Length>0)lineup.Append(" · ");
                        lineup.Append(FamilyLabels[family]).Append(" T")
                            .Append(tier+1).Append(" ×").Append(player.Troops[family,tier]);
                    }
            var observed=new StringBuilder();
            for(int family=0;family<Families;family++)
                for(int tier=0;tier<Tiers;tier++)
                {
                    int count=player.Troops[family,tier];
                    if(count==0)continue;
                    double initial=r.InitialHpA==null?count*Health[family]*Tier[tier]:r.InitialHpA[family*Tiers+tier];
                    double damaged=Math.Max(0,Math.Min(initial,r.DamagedHpA[family,tier]));
                    double fraction=initial>0?damaged/initial:0;
                    string wear=fraction>.80?"muy castigados":
                        fraction>.45?"castigados":fraction>.10?"daños moderados":"desgaste leve";
                    if(observed.Length>0)observed.Append(" · ");
                    observed.Append(FamilyLabels[family]).Append(" T").Append(tier+1)
                        .Append(": ").Append(wear);
                }
            // Do not call HP loss 'dead' or 'wounded': casualties require a
            // separately authorized integer ledger and cannot be inferred here.
            string publicReason=r.Winner==1?"La expedición consiguió su objetivo.":
                r.Winner==-1?"La expedición no consiguió su objetivo.":
                "Ningún ejército logró imponerse.";
            string title=r.Winner==1?"Victoria en batalla":r.Winner==-1?"Derrota en batalla":"Empate en batalla";
            return new EldoriaV41Message {
                EventId=eventId,Category="informes",Type="battle",Title=title,
                Subtitle="Enfrentamiento frente a "+targetId,
                ReportId=eventId,
                RewardReceiptId=committedRewardReceiptId??"",OccurredAtUtcTicks=occurredUtcTicks,
                Battle=new EldoriaV41CombatRecord {
                    TargetId=targetId,HeroId=string.Join("+",player.HeroIds.Where(x=>!string.IsNullOrEmpty(x)).ToArray()),
                    Troops=player.Count,OwnRoster=ToRoster(player),Won=r.Winner==1,Rounds=r.Rounds,
                    Outcome=r.Winner==1?"victory":r.Winner==-1?"defeat":"draw",
                    RemainingHealth=(int)Math.Round(100*r.RemainingA,MidpointRounding.AwayFromZero),
                    PlayerComposition=lineup.ToString(),
                    ObservedAttrition=observed.ToString(),
                    EnemyObservedResistance=(Math.Max(0,Math.Min(100,(int)Math.Round(100*r.RemainingB,MidpointRounding.AwayFromZero)))).ToString(CultureInfo.InvariantCulture)+"%",
                    ObservedTotalDamageDealt=r.DealtA.Sum().ToString("F0",CultureInfo.InvariantCulture),
                    ObservedTotalDamageReceived=r.DealtB.Sum().ToString("F0",CultureInfo.InvariantCulture),
                    // V4.1 original optional OWN-family analysis: never a counter recommendation.
                    OwnDamageByFamily=OwnFamilyDamage(r.DealtA,player),
                    OwnReceivedByFamily=OwnReceivedDamage(r,player),
                    OwnRemainingByFamily=OwnFamilyRemaining(r,player),
                    Reason=publicReason
                    // Reward amounts MUST be copied from authoritative ledger, not generated here.
                }
            };
        }
    }
}
