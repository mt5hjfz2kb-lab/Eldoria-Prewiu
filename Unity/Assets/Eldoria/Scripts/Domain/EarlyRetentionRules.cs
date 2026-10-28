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
        // Actual reserved troops return at the UTC deadline, not when the prize is claimed.
        public string HeroId="aldric";
        public ArmyRoster Troops=new ArmyRoster();
        public bool TroopsReturned;
        public ScoutOrder Copy()
        {
            var copy=(ScoutOrder)MemberwiseClone();
            copy.Troops=Troops?.Copy()??new ArmyRoster();
            return copy;
        }
    }

    [Serializable] public sealed class EarlyRetentionState
    {
        public int LastClearedStage;
        // Saved, deterministic expedition discoveries. No duplicate rewards or new currencies.
        public int ScoutDiscoveries;
        // Four small map seals; bits track actual completed families only.
        public int ScoutDiscoveryMask;
        public int LatestScoutKind=-1, LatestScoutVariant=-1;
        public string LatestScoutReportId="";
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

        // Once taught, this destination must not disappear just because
        // the archers are temporarily away on a march. Entry stays visible;
        // COMBATIR itself is disabled while no troops are available.
        public static bool VigilantsUnlocked(PlayerState s) =>
            s!=null && s.SawmillLevel>=1;
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
            // All early challengers must belong to Eldoria's documented
            // corruption/bestiary. There are no stone golems in this story.
            string[] normal={"Avanzadilla corrupta","Acechador ceniciento",
                "Corrupto errante","Emboscada del bosque"};
            string[] traits={"Resistencia corrupta","Emboscada","Piel corrupta","Ataque veloz"};
            int variance=(stage-1)%4;
            // A quadratic ramp is deliberate: a starter roster reaches
            // several milestones but cannot clear the whole first chapter
            // without improving its troops/hero. Bosses are real walls,
            // not just a different title over trivial six-round combat.
            int hp=200+stage*27+(int)Math.Round(stage*(double)stage*.85,
                MidpointRounding.AwayFromZero)+(boss?190:0);
            int atk=22+stage*4+(int)Math.Round(stage*(double)stage*.12,
                MidpointRounding.AwayFromZero)+(boss?22:0);
            int def=19+stage*3+(int)Math.Round(stage*(double)stage*.09,
                MidpointRounding.AwayFromZero)+(boss?18:0);
            int brk=12+stage*2+(boss?15:0);
            if(boss)
            {
                switch(level)
                {
                    case 5: def+=Math.Max(15,def/2);break; // armored
                    case 10: atk+=Math.Max(15,atk/3);def=Math.Max(1,def-def/8);break; // assault
                    case 15: brk+=Math.Max(20,brk/2);hp+=Math.Max(25,hp/5);break; // Breach
                    case 20: hp+=Math.Max(40,hp/2);def+=Math.Max(10,def/8);break; // endurance
                    case 25: atk+=Math.Max(15,atk/4);def+=Math.Max(15,def/4);break; // commander
                }
            }
            var enemy=new WorldCreatureStats(hp,atk,def,brk,0,0);
            return new VigilantStage(stage,chapter,level,
                boss?GuardianName(level):normal[variance],
                boss?GuardianTrait(level):traits[variance],
                locations[(chapter-1)%locations.Length],enemy);
        }

        // Author-authored profiles; each boss changes real combat stats, not just labels.
        // Names and counters remain personal PvE; world creatures are unchanged.
        public static string GuardianName(int step)
        {
            switch(step){case 5:return "Guardián de la Coraza";
                case 10:return "Acechador del Umbral";
                case 15:return "Engendro de la Brecha";
                case 20:return "Centinela Inquebrantable";
                case 25:return "Vigía de las Cenizas";
                default:return "Guardián del umbral";}
        }
        public static string GuardianTrait(int step)
        {
            switch(step){case 5:return "Coraza reforzada";
                case 10:return "Asalto fulminante";
                case 15:return "Ruptura corrupta";
                case 20:return "Resistencia tenaz";
                case 25:return "Mando corrupto";
                default:return "Resistencia corrupta";}
        }
        public static string GuardianHint(int step)
        {
            switch(step){case 5:return "Defensa elevada: prioriza Ataque y Ruptura.";
                case 10:return "Golpea rápido: protege la Vida de tu marcha.";
                case 15:return "Ruptura peligrosa: refuerza Defensa y Vida.";
                case 20:return "Combate prolongado: aumenta el daño sostenido.";
                case 25:return "Poder equilibrado: prepara tu mejor héroe y tropas.";
                default:return "Estudia su rasgo y ajusta tu formación.";}
        }
        public static string CombatInsight(VigilantStage stage,CombatStats army,CombatReport result)
        {
            string outcome=result.Won?"VICTORIA":"DERROTA";
            string rounds=result.Rounds+" asaltos";
            if(result.Won)
                return outcome+" · "+rounds+" · "+result.RemainingHealth+
                    " Vida restante. "+(stage.Step%5==0?"Guardián superado. ":"Camino despejado. ")+
                    "Tu marcha venció con "+army.Power+" de Poder.";
            if(stage.Step%5==0)
                return outcome+" · "+rounds+". "+GuardianHint(stage.Step)+
                    " Tu marcha: "+army.Power+" Poder.";
            return outcome+" · "+rounds+". Revisa Poder, Ataque, Ruptura y tropas de la marcha.";
        }
        public static int ScoutReportVariant(ScoutOrder order)
        {
            if(order==null)return 0;
            // Stable after UTC expiry, refresh and reopen; no runtime randomness.
            return (int)((((long)order.Period+order.Slot*7L+order.Kind*11L)%3+3)%3);
        }
        public static string ScoutReportHeading(int kind,int variant)
        {
            int v=((variant%3)+3)%3;
            switch(kind){
                case 0:return new[]{"Señales en la frontera","Ecos del paso oscuro","Guardia en la niebla"}[v];
                case 1:return new[]{"La hoguera encendida","Un camino de regreso","Voces entre los pinos"}[v];
                case 2:return new[]{"El carro abandonado","Marcas junto al río","El almacén olvidado"}[v];
                default:return new[]{"Huellas violetas","Ceniza sobre la piedra","Un rumor bajo la Brecha"}[v];
            }
        }
        public static string ScoutReportStory(int kind,int variant)
        {
            int v=((variant%3)+3)%3;
            switch(kind){
                case 0:return new[]{
                    "La patrulla halló escudos marcados por la corrupción. La ruta vuelve a ser transitable.",
                    "Las antorchas se extinguieron antes del amanecer. Algo se retiró hacia el norte.",
                    "Un vigía reconoció los pasos enemigos entre la bruma. Valoria conoce ahora esa senda."}[v];
                case 1:return new[]{
                    "Los viajeros seguían una luz entre los árboles. Los exploradores los guiaron a casa.",
                    "Una familia había perdido el sendero. Un antiguo mojón mostró la ruta segura.",
                    "Las voces resultaron ser campesinos refugiados. Traen noticias de las tierras fronterizas."}[v];
                case 2:return new[]{
                    "Un carro oculto entre zarzas conservaba materiales útiles para Valoria.",
                    "Los rastros de ruedas conducían a un depósito intacto cerca del cauce.",
                    "Las provisiones estaban bajo una lona antigua. El destacamento recuperó lo aprovechable."}[v];
                default:return new[]{
                    "Encontraron huellas violáceas junto a unas rocas. La Brecha sigue extendiendo su influencia.",
                    "La ceniza formaba un símbolo desconocido. Los exploradores lo anotaron en su mapa.",
                    "Un murmullo surgía de las grietas. El informe recomienda vigilar el próximo guardián."}[v];
            }
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
            foreach(var order in p.Orders)
            {
                if(order==null||order.StartedUtcTicks<=0||order.EndsUtcTicks>utcNow||order.TroopsReturned)continue;
                if(order.Troops!=null&&order.Troops.Total>0)
                    MarchCapacityRules.Return(s,order.Troops);
                order.TroopsReturned=true;
                changed=true;
            }
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

        // Early-retention XP is a real combat progression rather than a
        // decorative counter. Apply its small capped mastery bonus only to
        // these new personal PvE destinations until the canonical hero-level
        // system is implemented; existing world PvE/PvP balance stays intact.
        public static int HeroMasteryLevel(PlayerState s,string heroId)
        {
            var p=s?.Retention;
            if(p==null)return 0;
            int earned=heroId=="lyra"?p.LyraExperience:
                heroId=="aldric+lyra"?(p.AldricExperience+p.LyraExperience)/2:
                p.AldricExperience;
            return Math.Max(0,Math.Min(20,earned/80));
        }
        public static CombatStats ExpeditionWithMastery(PlayerState s,
            ArmyRoster troops,string heroId)
        {
            var baseStats=SliceRules.Expedition(troops,heroId);
            int level=HeroMasteryLevel(s,heroId);
            return new CombatStats(
                checked(baseStats.Attack+level*4),
                checked(baseStats.Defense+level*3),
                checked(baseStats.Health+level*15),
                checked(baseStats.Break+level*2),
                checked(baseStats.Power+level*45));
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
                if(!MarchCapacityRules.CanStart(s,utcNow)){message="Las tres marchas están ocupadas";return false;}
                string hero=s.ChapterThree!=null&&s.ChapterThree.LyraRecruited&&
                    (s.PreparedHeroId=="lyra"||s.PreparedHeroId=="aldric+lyra")
                    ?s.PreparedHeroId:"aldric";
                var available=s.MarchConfigured&&s.PreparedTroops!=null&&s.PreparedTroops.Total>0?
                    s.PreparedTroops.Copy():
                    MarchCapacityRules.AutoSelect(s.Available,MarchCapacityRules.Capacity(s,hero));
                if(!MarchCapacityRules.Fits(available,s.Available,MarchCapacityRules.Capacity(s,hero)))
                {message="La composición supera la capacidad de marcha o no está disponible";return false;}
                var enemy=Stage(stage);
                var army=ExpeditionWithMastery(s,available,hero);
                var battle=SliceRules.FightCreature(army,enemy.Enemy);
                s.LastBattleReport=new BattleReportState{TargetId="vigilant:"+stage,
                    HeroId=hero,Troops=available.Total,PlayerPower=army.Power,
                    EnemyPower=enemy.Difficulty,Won=battle.Won,Rounds=battle.Rounds,
                    RemainingHealth=battle.RemainingHealth,
                    Reason=CombatInsight(enemy,army,battle)};
                s.LastBattleReason=s.LastBattleReport.Reason;
                if(!battle.Won){message=s.LastBattleReason;return true;}
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
                // Discard time beyond the cap before consuming the paid interval.
                // Otherwise a 9-hour absence pays 8 hours and leaves another hour
                // immediately claimable through a fresh command.
                long accrualStart=Math.Max(p.LastIdleClaimUtcTicks,utcNow-IdleCapTicks);
                p.LastIdleClaimUtcTicks=Math.Min(utcNow,accrualStart+consumed);
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
                if(!MarchCapacityRules.CanStart(s,utcNow)){message="Las tres marchas están ocupadas";return false;}
                // Twelve troops is a provisional scout detachment, not a balance reward.
                // It reserves REAL units so three orders cannot share the same army.
                var party=MarchCapacityRules.AutoSelect(s.Available,
                    Math.Min(12,MarchCapacityRules.Capacity(s,"aldric")));
                if(!MarchCapacityRules.Fits(party,s.Available,MarchCapacityRules.Capacity(s,"aldric")))
                {message="No hay tropas disponibles";return false;}
                if(order.Kind==0)
                {
                    var fight=SliceRules.FightCreature(ExpeditionWithMastery(s,party,"aldric"),
                        new WorldCreatureStats(430,48,43,18,0,0));
                    if(!fight.Won){message="La patrulla es demasiado fuerte; refuerza tropas";return false;}
                }
                MarchCapacityRules.Reserve(s,party);
                order.HeroId="aldric";
                order.Troops=party;
                order.TroopsReturned=false;
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
            if(p.LatestScoutReportId!=order.Id)
            {
                p.LatestScoutReportId=order.Id;
                p.LatestScoutKind=order.Kind;
                p.LatestScoutVariant=ScoutReportVariant(order);
                p.ScoutDiscoveries=checked(p.ScoutDiscoveries+1);
                p.ScoutDiscoveryMask|=1<<(order.Kind&3);
            }
            message="Hallazgo: "+ScoutReportHeading(order.Kind,p.LatestScoutVariant)+
                " · "+ScoutRewardText(order.Kind);
            return true;
        }
    }
}
