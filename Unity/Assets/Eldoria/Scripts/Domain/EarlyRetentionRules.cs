using System;
using System.Collections.Generic;
using System.Linq;

namespace Eldoria.Domain
{
    // Additive schema-one save fields. All clocks are UTC and every reward is committed
    // through the EXISTING LocalGateway; these systems never own a second state store.
    [Serializable] public sealed class ScoutOrder
    {
        public string Id="";
        public int Kind, Period, Slot;
        public long StartedUtcTicks, EndsUtcTicks;
        public bool Claimed;
        public ScoutOrder Copy() => (ScoutOrder)MemberwiseClone();
    }

    [Serializable] public sealed class EarlyRetentionState
    {
        public int LastClearedStage;
        public int AldricExperience, LyraExperience;
        public long LastIdleClaimUtcTicks;
        public List<ScoutOrder> Orders=new List<ScoutOrder>();
        public EarlyRetentionState Copy()
        {
            var clone=(EarlyRetentionState)MemberwiseClone();
            clone.Orders=(Orders??new List<ScoutOrder>()).Where(x=>x!=null)
                .Select(x=>x.Copy()).ToList();
            return clone;
        }
    }

    public readonly struct VigilantStage
    {
        public readonly int Absolute, Chapter, Step, Difficulty;
        public readonly string Name, Trait, Scene;
        public readonly WorldCreatureStats Enemy;
        public VigilantStage(int absolute, int chapter, int step,
            string name, string trait, string scene, WorldCreatureStats enemy)
        { Absolute=absolute; Chapter=chapter; Step=step; Name=name; Trait=trait;
          Scene=scene; Enemy=enemy; Difficulty=enemy.Health+enemy.Attack*2+enemy.Defense; }
    }

    public static class EarlyRetentionRules
    {
        public const int StagesPerChapter=25;
        public const int ScoutSlotsPerPeriod=3;
        public const int MaxUnclaimedOrders=6;
        public const int IdleCapHours=8;
        static readonly long PeriodTicks=TimeSpan.FromHours(12).Ticks;
        static readonly long IdleCapTicks=TimeSpan.FromHours(IdleCapHours).Ticks;

        public static bool VigilantsUnlocked(PlayerState s) =>
            s!=null && s.SawmillLevel>=1 && s.Available!=null &&
            (s.Available.Total>0 || s.BastionLevel>=2);
        public static bool ScoutsUnlocked(PlayerState s) =>
            s!=null && s.BastionLevel>=2 && s.BarracksLevel>=1 &&
            s.ChapterProgress!=null && s.ChapterProgress.MarchConfirmed;
        public static bool StageUnlocked(PlayerState s,int absolute) =>
            VigilantsUnlocked(s) && absolute>0 && absolute<=10000 &&
            (absolute==1 || s.BastionLevel>=2) &&
            (absolute<=25 || s.BastionLevel>=3) &&
            (absolute<=50 || s.BastionLevel>=4) &&
            (absolute<=75 || s.BastionLevel>=5);
        public static long Period(long now) => Math.Max(0,now/PeriodTicks);
        public static long NextPeriodUtcTicks(long now) => checked((Period(now)+1)*PeriodTicks);
        public static int UnclaimedIdleXp(EarlyRetentionState p,long now)
        {
            if(p==null||p.LastClearedStage<1||p.LastIdleClaimUtcTicks<=0) return 0;
            long elapsed=Math.Max(0,Math.Min(IdleCapTicks,now-p.LastIdleClaimUtcTicks));
            int perHour=12+Math.Min(48,p.LastClearedStage/5*3);
            return (int)(elapsed*perHour/TimeSpan.TicksPerHour);
        }
        public static VigilantStage Stage(int stage)
        {
            if(stage<1||stage>10000)throw new ArgumentOutOfRangeException(nameof(stage));
            int chapter=(stage-1)/StagesPerChapter+1;
            int level=(stage-1)%StagesPerChapter+1;
            bool boss=level%5==0;
            string[] locations={"Las Sendas de Ceniza","Bosque de los Centinelas",
                "Frontera Corrupta","Ruinas de los Vigilantes"};
            string[] normal={"Avanzadilla corrupta","Acechador ceniciento",
                "Custodio de piedra","Emboscada del bosque"};
            string[] traits={"Escudo pétreo","Emboscada","Armadura reforzada","Ataque veloz"};
            int variance=(stage-1)%4;
            int hp=200+stage*27+(boss?190:0);
            int atk=22+stage*4+(boss?22:0);
            int def=19+stage*3+(boss?18:0);
            int brk=12+stage*2+(boss?15:0);
            var enemy=new WorldCreatureStats(hp,atk,def,brk,0,0);
            return new VigilantStage(stage,chapter,level,
                boss?"Guardián del umbral":normal[variance],
                traits[variance],locations[(chapter-1)%locations.Length],enemy);
        }

        public static int ScoutKind(long period,int slot) => (int)((period+slot)%4);
        public static string ScoutTitle(int kind)
        {
            switch(kind){case 0:return "Patrulla corrupta";case 1:return "Viajeros perdidos";
                case 2:return "Suministros extraviados";default:return "Rastros de la Brecha";}
        }
        public static int ScoutDurationSeconds(int kind) => 180+kind*90;
        public static string ScoutRewardText(int kind) =>
            kind==0?"45 madera · 20 piedra · 20 XP":
            kind==1?"35 madera · 30 comida · 20 XP":
            kind==2?"60 madera · 25 piedra · 20 XP":
            "25 piedra · 45 comida · 20 XP";

        static EarlyRetentionState Get(PlayerState s)
        {
            if(s.Retention==null)s.Retention=new EarlyRetentionState();
            if(s.Retention.Orders==null)s.Retention.Orders=new List<ScoutOrder>();
            return s.Retention;
        }
        // Load, menu refresh and UTC expiry are safe to replay or simulate offline.
        public static bool Synchronize(PlayerState s,long utcNow)
        {
            if(s==null)return false;
            var p=Get(s);bool changed=false;
            if(ScoutsUnlocked(s))
            {
                long period=Period(utcNow);
                int before=p.Orders.Count;
                p.Orders.RemoveAll(x=>x==null || (x.Claimed && x.Period<period-2));
                changed|=before!=p.Orders.Count;
                for(int slot=0;slot<ScoutSlotsPerPeriod;slot++)
                {
                    string id=period+":"+slot;
                    if(p.Orders.Any(x=>x.Id==id))continue;
                    if(p.Orders.Count(x=>!x.Claimed)>=MaxUnclaimedOrders)break;
                    p.Orders.Add(new ScoutOrder{Id=id,Period=(int)Math.Min(int.MaxValue,period),
                        Slot=slot,Kind=ScoutKind(period,slot)});
                    changed=true;
                }
            }
            return changed;
        }

        // null = unrelated command; false = refused without mutation; true = accepted.
        // The host verifies PlayerId, Revision and idempotent command ID FIRST.
        public static bool? Apply(PlayerState s,string kind,string target,long utcNow,
            out string message)
        {
            message="";
            if(kind!="VigilantBattle" && kind!="ClaimVigilantXp" &&
               kind!="ScoutDispatch" && kind!="ScoutClaim")return null;
            if(s==null){message="Partida no disponible";return false;}
            var p=Get(s);
            if(kind=="VigilantBattle")
            {
                if(!int.TryParse(target,out int stage) || stage!=p.LastClearedStage+1 ||
                   !StageUnlocked(s,stage)){message="Etapa aún no desbloqueada";return false;}
                var available=s.Available?.Copy();
                if(available==null||available.Total<1){message="Necesitas tropas disponibles";return false;}
                string hero=s.ChapterThree!=null&&s.ChapterThree.LyraRecruited&&
                    (s.PreparedHeroId=="lyra"||s.PreparedHeroId=="aldric+lyra")
                    ?s.PreparedHeroId:"aldric";
                var enemy=Stage(stage);
                var battle=SliceRules.FightCreature(SliceRules.Expedition(available,hero),enemy.Enemy);
                s.LastBattleReport=new BattleReportState{TargetId="vigilant:"+stage,
                    HeroId=hero,Troops=available.Total,PlayerPower=SliceRules.Expedition(available,hero).Power,
                    EnemyPower=enemy.Difficulty,Won=battle.Won,Rounds=battle.Rounds,
                    RemainingHealth=battle.RemainingHealth,
                    Reason=battle.Won?"El ejército superó "+enemy.Trait.ToLowerInvariant()+".":
                        "Refuerza las tropas y vuelve a intentarlo; "+enemy.Trait.ToLowerInvariant()+"."};
                s.LastBattleReason=s.LastBattleReport.Reason;
                if(!battle.Won){message="La etapa resistió. Mejora tu ejército.";return true;}
                p.LastClearedStage=stage;
                if(p.LastIdleClaimUtcTicks==0)p.LastIdleClaimUtcTicks=utcNow;
                int xp=20+stage/5*3;
                if(hero=="lyra")p.LyraExperience=checked(p.LyraExperience+xp);
                else p.AldricExperience=checked(p.AldricExperience+xp);
                s.Resources.Wood=checked(s.Resources.Wood+30+stage*2);
                s.Resources.Stone=checked(s.Resources.Stone+10+stage);
                message="Etapa "+stage+" superada · "+xp+" XP · recompensa entregada";
                return true;
            }
            if(kind=="ClaimVigilantXp")
            {
                if(!VigilantsUnlocked(s)||p.LastClearedStage<1){message="Completa una etapa primero";return false;}
                int earned=UnclaimedIdleXp(p,utcNow);
                if(earned<1){message="Todavía no hay experiencia acumulada";return false;}
                p.AldricExperience=checked(p.AldricExperience+earned);
                // Preserve fractional accrual rather than discarding sub-hour progress.
                int rate=12+Math.Min(48,p.LastClearedStage/5*3);
                long consumed=earned*TimeSpan.TicksPerHour/rate;
                p.LastIdleClaimUtcTicks=Math.Min(utcNow,p.LastIdleClaimUtcTicks+consumed);
                if(utcNow-p.LastIdleClaimUtcTicks>IdleCapTicks)p.LastIdleClaimUtcTicks=utcNow-IdleCapTicks+consumed;
                message=earned+" XP de héroe recogida";
                return true;
            }
            if(!ScoutsUnlocked(s)){message="Exploradores se desbloquea al preparar la primera Marcha";return false;}
            if(string.IsNullOrEmpty(target)){message="Encargo no identificado";return false;}
            var order=p.Orders.FirstOrDefault(x=>x.Id==target);
            if(order==null||order.Claimed){message="Encargo inexistente o ya reclamado";return false;}
            if(kind=="ScoutDispatch")
            {
                if(order.StartedUtcTicks!=0){message="Encargo ya enviado";return false;}
                if(s.Available==null||s.Available.Total<1){message="No hay tropas disponibles";return false;}
                if(order.Kind==0)
                {
                    var fight=SliceRules.FightCreature(SliceRules.Expedition(s.Available,"aldric"),
                        new WorldCreatureStats(430,48,43,18,0,0));
                    if(!fight.Won){message="La patrulla es demasiado fuerte; refuerza tropas";return false;}
                }
                order.StartedUtcTicks=utcNow;
                order.EndsUtcTicks=checked(utcNow+TimeSpan.FromSeconds(ScoutDurationSeconds(order.Kind)).Ticks);
                message="Exploradores enviados; volverán con su informe";
                return true;
            }
            if(order.StartedUtcTicks<=0||order.EndsUtcTicks>utcNow){message="Los exploradores aún están en camino";return false;}
            if(order.Kind==0){s.Resources.Wood=checked(s.Resources.Wood+45);s.Resources.Stone=checked(s.Resources.Stone+20);}
            else if(order.Kind==1){s.Resources.Wood=checked(s.Resources.Wood+35);s.Resources.Food=checked(s.Resources.Food+30);}
            else if(order.Kind==2){s.Resources.Wood=checked(s.Resources.Wood+60);s.Resources.Stone=checked(s.Resources.Stone+25);}
            else{s.Resources.Stone=checked(s.Resources.Stone+25);s.Resources.Food=checked(s.Resources.Food+45);}
            p.AldricExperience=checked(p.AldricExperience+20);
            order.Claimed=true;
            message="Encargo completado · "+ScoutRewardText(order.Kind);
            return true;
        }
    }
}
