using System;
using System.Collections.Generic;
using System.Linq;

namespace Eldoria.Domain
{
    /// <summary>
    /// Isolated deterministic port of the V4.1 3-family/3-tier laboratory rules.
    /// DOES NOT replace SliceRules.Fight / FightCreature, hero roster authority,
    /// campaign saves, or any production balancing. PvP and death rules remain absent.
    /// </summary>
    public static class EldoriaV41Combat
    {
        public const string RulesVersion = "v4.1-laboratory";
        public const bool ProductionApproved = false;
        const double Epsilon=1e-9, CounterBonus=1.40, Tactical=0.65;
        const double Guard=0.24, GuardBudget=0.35, WarlockBypass=0.75;
        static readonly double[] Tier={1.0,1.15,1.33};
        static readonly double[] Attack={9,13,11};
        static readonly double[] Defense={16,7,8};
        static readonly double[] Health={72,42,49};
        static readonly double[] Rupture={3,7,17};
        // Indexed P,A,B: Paladin > Archer > Warlock > Paladin.
        static readonly int[,] Counter={
            {0,1,-1},
            {-1,0,1},
            {1,-1,0}
        };
        public sealed class Force
        {
            /// <summary>Three families P,A,B and three tiers T1,T2,T3 each.</summary>
            public int[,] Troops=new int[3,3];
            public string HeroId="aldric";
            public Force Add(int family,int tier,int count)
            {
                if(family<0||family>2||tier<1||tier>3||count<0)
                    throw new ArgumentOutOfRangeException();
                Troops[family,tier-1]=checked(Troops[family,tier-1]+count);
                return this;
            }
        }
        public sealed class Result
        {
            public int Winner; // +1 attacker A, -1 defender B, 0 draw
            public int Rounds;
            public double RemainingA,RemainingB;
            public double[] DamageDealtA=new double[3],DamageDealtB=new double[3];
            public double[] DamageReceivedA=new double[3],DamageReceivedB=new double[3];
            public double ShieldBlockedA,ShieldBlockedB,PiercingAddedA,PiercingAddedB;
        }
        sealed class Side
        {
            public string Hero;
            public double[] Hp=new double[9],Initial=new double[9],HpPer=new double[9],
                Atk=new double[9],Def=new double[9],Rup=new double[9];
            public double[] Dealt=new double[3],Taken=new double[3];
            public double ShieldBlocked,PiercingAdded;
            public double TotalHp=>Hp.Sum();
            public double InitialHp=>Initial.Sum();
        }
        static Side Build(Force f)
        {
            if(f==null || (f.HeroId!="aldric" && f.HeroId!="lyra"))
                throw new ArgumentException("Unknown hero");
            var s=new Side{Hero=f.HeroId};int total=0;
            for(int family=0;family<3;family++)for(int tier=0;tier<3;tier++)
            {
                int count=f.Troops[family,tier];
                if(count<0)throw new ArgumentException("Negative units");
                total=checked(total+count);
                int i=family*3+tier;
                s.HpPer[i]=Health[family]*Tier[tier];
                s.Hp[i]=s.Initial[i]=count*s.HpPer[i];
                s.Atk[i]=Attack[family]*Tier[tier]*
                    (f.HeroId=="lyra" && family==1?1.03:1.0);
                s.Def[i]=Defense[family]*Tier[tier]*
                    (f.HeroId=="aldric" && family==0?1.03:1.0);
                s.Rup[i]=Rupture[family]*Tier[tier];
            }
            if(total<1)throw new ArgumentException("Empty army");
            return s;
        }
        static double CounterFactor(int attackingFamily,int defendingFamily)
            => Counter[attackingFamily,defendingFamily]==1?CounterBonus:1;
        static double Coherence(Force a,Force b)
        {
            double[] x=new double[3],y=new double[3];
            for(int family=0;family<3;family++)
                for(int tier=0;tier<3;tier++)
                {x[family]+=a.Troops[family,tier];y[family]+=b.Troops[family,tier];}
            double sx=x.Sum(),sy=y.Sum(),coherence=0;
            for(int i=0;i<3;i++)for(int j=0;j<3;j++)
                coherence+=(x[i]/sx)*Counter[i,j]*(y[j]/sy);
            return coherence;
        }
        static double[,] Outgoing(Side source,Side target,double tactical)
        {
            var matrix=new double[9,9];
            for(int i=0;i<9;i++)
            {
                double living=source.Hp[i]/source.HpPer[i];
                if(living<=Epsilon)continue;
                double denom=0;
                for(int j=0;j<9;j++)
                    denom+=Math.Pow(CounterFactor(i/3,j/3),0.6)*
                        target.Hp[j]/target.HpPer[j];
                if(denom<=Epsilon)continue;
                for(int j=0;j<9;j++)
                    matrix[i,j]=living*source.Atk[i]*
                        Math.Pow(CounterFactor(i/3,j/3),0.6)*
                        (target.Hp[j]/target.HpPer[j])/denom;
            }
            // Interception is bounded by actual Paladin HP, to avoid one-unit decoys.
            double desired=0;
            for(int i=0;i<9;i++)for(int j=3;j<9;j++)
                desired+=matrix[i,j]*Guard*(j>=6?1-WarlockBypass:1);
            double palHp=target.Hp.Take(3).Sum();
            double applied=desired>Epsilon?Math.Min(desired,palHp*GuardBudget)/desired:0;
            if(palHp>Epsilon && applied>0)
                for(int i=0;i<9;i++)for(int j=3;j<9;j++)
                {
                    double moved=matrix[i,j]*Guard*(j>=6?1-WarlockBypass:1)*applied;
                    matrix[i,j]-=moved;
                    for(int k=0;k<3;k++)matrix[i,k]+=moved*target.Hp[k]/palHp;
                }
            for(int i=0;i<9;i++)for(int j=0;j<9;j++)
            {
                double ruptureFraction=Math.Min(0.5,source.Rup[i]/(source.Rup[i]+35.0));
                double mitigation=100.0/(100.0+4*target.Def[j]*(1-ruptureFraction));
                matrix[i,j]*=CounterFactor(i/3,j/3)*tactical*mitigation;
            }
            return matrix;
        }
        static double Sum(double[,] values)
        {
            double s=0;
            foreach(double d in values)s+=d;
            return s;
        }
        static void Scale(double[,] matrix,double ratio)
        {
            for(int i=0;i<9;i++)for(int j=0;j<9;j++)matrix[i,j]*=ratio;
        }
        static void Apply(Side attack,Side defend,double[,] damage)
        {
            // Simultaneous turn; only count effective damage (not overkill).
            for(int j=0;j<9;j++)
            {
                double incoming=0;
                for(int i=0;i<9;i++)incoming+=damage[i,j];
                if(incoming<Epsilon)continue;
                double cap=Math.Min(1.0,defend.Hp[j]/incoming);
                for(int i=0;i<9;i++)
                {
                    double dealt=damage[i,j]*cap;
                    attack.Dealt[i/3]+=dealt;defend.Taken[j/3]+=dealt;
                }
                defend.Hp[j]=Math.Max(0,defend.Hp[j]-incoming);
            }
        }
        public static Result Duel(Force a,Force b,int maxRounds=20,bool heroSkills=true)
        {
            if(maxRounds<1||maxRounds>100)throw new ArgumentOutOfRangeException(nameof(maxRounds));
            var x=Build(a);var y=Build(b);double coherence=Coherence(a,b);
            int turns=0;
            while(turns<maxRounds && x.TotalHp>Epsilon && y.TotalHp>Epsilon)
            {
                turns++;
                var xy=Outgoing(x,y,1+Tactical*coherence);
                var yx=Outgoing(y,x,1-Tactical*coherence);
                if(heroSkills && turns==1)
                {
                    if(x.Hero=="lyra")
                    {
                        double old=Sum(xy),increase=Math.Min(.025*y.InitialHp,.25*old);
                        if(old>Epsilon){Scale(xy,1+increase/old);x.PiercingAdded+=increase;}
                    }
                    if(y.Hero=="lyra")
                    {
                        double old=Sum(yx),increase=Math.Min(.025*x.InitialHp,.25*old);
                        if(old>Epsilon){Scale(yx,1+increase/old);y.PiercingAdded+=increase;}
                    }
                    if(x.Hero=="aldric")
                    {
                        double old=Sum(yx),block=Math.Min(.45*old,.028*x.InitialHp);
                        if(old>Epsilon){Scale(yx,1-block/old);x.ShieldBlocked+=block;}
                    }
                    if(y.Hero=="aldric")
                    {
                        double old=Sum(xy),block=Math.Min(.45*old,.028*y.InitialHp);
                        if(old>Epsilon){Scale(xy,1-block/old);y.ShieldBlocked+=block;}
                    }
                }
                Apply(x,y,xy);Apply(y,x,yx);
            }
            double leftA=x.TotalHp/x.InitialHp,leftB=y.TotalHp/y.InitialHp;
            return new Result{
                Winner=leftA-leftB>1e-7?1:leftB-leftA>1e-7?-1:0,
                Rounds=turns,RemainingA=leftA,RemainingB=leftB,
                DamageDealtA=x.Dealt,DamageDealtB=y.Dealt,
                DamageReceivedA=x.Taken,DamageReceivedB=y.Taken,
                ShieldBlockedA=x.ShieldBlocked,ShieldBlockedB=y.ShieldBlocked,
                PiercingAddedA=x.PiercingAdded,PiercingAddedB=y.PiercingAdded
            };
        }
    }
}
