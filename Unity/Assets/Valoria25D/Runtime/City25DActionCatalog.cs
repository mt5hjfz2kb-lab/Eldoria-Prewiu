using System;
using Eldoria.Domain;
using Eldoria.Experimentos.Ciudad2D;

namespace Eldoria.Valoria25D
{
    /// <summary>
    /// Presentation-only projection of the EXISTING LocalGateway command contract.
    /// This never computes gameplay results, credits wallet or writes a save.
    /// The canonical gateway revalidates every command, cost and prerequisite.
    /// </summary>
    public static class City25DActionCatalog
    {
        public readonly struct Offer
        {
            public readonly string Title, Description, Kind, Target;
            public readonly int Wood, Stone, Food, Seconds;
            public readonly string Requirements;
            public bool CanAttempt => !string.IsNullOrEmpty(Kind);
            public Offer(string title,string description,string kind,string target,
                         int wood=0,int stone=0,int food=0,int seconds=0,string requirements="")
            {
                Title=title;Description=description;Kind=kind;Target=target;
                Wood=wood;Stone=stone;Food=food;Seconds=seconds;
                Requirements=requirements??"";
            }
            public string Cost => Wood==0&&Stone==0&&Food==0 ? "" :
                "Coste: "+Wood+" madera · "+Stone+" piedra · "+Food+" comida";
        }

        public static string CanonicalId(string visualId)
        {
            switch (visualId)
            {
                case "bastion": return "bastion";
                case "aserradero": return "sawmill";
                case "cuartel": return "barracks";
                case "granero": return "granary";
                case "cantera": return "stoneworks";
                default: return null; // Salón de Héroes has no build command.
            }
        }
        public static string Display(string id)
        {
            switch (id)
            {
                case "bastion": return "Bastión";
                case "aserradero": return "Aserradero";
                case "cuartel": return "Cuartel";
                case "granero": return "Granero";
                case "cantera": return "Cantera";
                case "salon-heroes": return "Salón de Héroes";
                default: return id??"";
            }
        }
        public static int BuildingLevel(PlayerState state,string visualId)
        {
            if (state==null) return 0;
            switch(visualId)
            {
                case "bastion":return state.BastionLevel;
                case "aserradero":return state.SawmillLevel;
                case "cuartel":return state.BarracksLevel;
                case "granero":return state.ChapterThree?.GranaryLevel??0;
                case "cantera":return state.ChapterThree?.StoneworksLevel??0;
                default:return 0;
            }
        }
        public static bool IsBuildingInProgress(PlayerState state,string id)
        {
            if(state==null)return false;
            if(id=="bastion")return state.BastionUpgradeCompletesUtcTicks>0;
            var task=state.BuildingTaskId??"";
            if(state.BuildingCompletesUtcTicks<=0)return false;
            string target=CanonicalId(id);
            if(target==null)return false;
            return task.StartsWith(target+":",StringComparison.Ordinal) ||
                   task.StartsWith("c3:"+target+":",StringComparison.Ordinal);
        }
        public static long TaskEnd(PlayerState state,string id)
        {
            return IsBuildingInProgress(state,id)
                ?(id=="bastion"?state.BastionUpgradeCompletesUtcTicks:state.BuildingCompletesUtcTicks)
                :0L;
        }
        public static string Remaining(long utcTicks,long now)
        {
            if(utcTicks<=now)return "Finalizando...";
            var seconds=Math.Max(0,(long)Math.Ceiling((utcTicks-now)/(double)TimeSpan.TicksPerSecond));
            var time=TimeSpan.FromSeconds(seconds);
            return time.TotalHours>=1
                ?((int)time.TotalHours)+" h "+time.Minutes+" m"
                :time.Minutes+" m "+time.Seconds+" s";
        }
        public static Offer Building(PlayerState state,string id,long now)
        {
            if(state==null) return new Offer("Sin partida","No hay estado del jugador",null,null);
            var canonical=CanonicalId(id);
            if(canonical==null)return new Offer(Display(id),
                "Edificio informativo: no existe una orden de construcción canónica.",null,null);
            int level=BuildingLevel(state,id);
            string heading=Display(id)+" · nivel "+level;
            int required=City2DProgressionPolicy.RequiredBastion(id);
            if(state.BastionLevel<required)
                return new Offer(heading,"Parcela bloqueada hasta Bastión "+required,null,null,
                    requirements:"Requiere Bastión "+required);
            long until=TaskEnd(state,id);
            if(until>0)
                return new Offer(heading,"Obra real en curso: "+Remaining(until,now),null,null);
            if(state.BastionUpgradeCompletesUtcTicks>0 || state.BuildingCompletesUtcTicks>0)
                return new Offer(heading,"Ya hay otra construcción en curso.",null,null);
            if(id=="bastion")
            {
                if(level>=5)return new Offer(heading,
                    "Bastión V alcanzado. No hay nuevas mejoras en el catálogo I–V.",null,null);
                var next=BastionProgressionCatalog.ForLevel(level+1);
                return new Offer("Bastión "+level+" → "+next.Level,
                    "Mejora de Bastión "+next.Level+" con temporizador y desbloqueos reales.",
                    "AdvanceBastion","bastion",next.WoodCost,next.StoneCost,
                    next.FoodCost,next.DurationSeconds,
                    BastionProgressionCatalog.MissingSummary(state,next));
            }
            if(level>=5) return new Offer(heading,"Límite canónico de construcción I–V",null,null);
            if(id=="aserradero" && level==0)
                return new Offer(heading,"Reconstruir el Aserradero con la orden principal.",
                    "Build","sawmill",SliceRules.SawmillWoodCost,0,0,SliceRules.SawmillBuildSeconds);
            if(id=="cuartel" && level==0)
                return new Offer(heading,"Construir el Cuartel usando el contrato existente.",
                    "Build","barracks",SliceRules.BarracksWoodCost,
                    SliceRules.BarracksStoneCost,0,SliceRules.BarracksBuildSeconds);
            // All remaining buildings and post-level-I upgrades use the C3 command.
            // These exact values come directly from ExecuteChapterThree in main.
            // The gateway decides if an offer is valid, and errors are displayed unchanged.
            int wood=level==0?(id=="cantera"?500:240):
                (id=="cuartel"?140:120)*level;
            int stone=level==0?(id=="cantera"?0:180):
                (id=="cuartel"?90:78)*level;
            int food=level==0&&id=="cantera"?200:0;
            int seconds=level==0?(id=="cantera"?12:9):8+level*2;
            string requirement="";
            if(level>=state.BastionLevel)
                requirement="Necesitas un nivel superior de Bastión";
            else if(level>=2 && id!="cuartel")
            {
                string item=id=="aserradero"?"iron-brace":
                    id=="cantera"?"stone-tusk":"treated-hide";
                if(state.ChapterThree==null||
                   !state.ChapterThree.Inventory.Contains(item))
                    requirement="Falta material de la frontera: "+item;
            }
            return new Offer(heading,level==0?"Construir parcela":"Mejorar instalación a nivel "+(level+1),
                level==0?"Build":"UpgradeBuilding",canonical,
                wood,stone,food,seconds,requirement);
        }
        public static Offer Recruit(PlayerState state,long now)
        {
            if(state==null)return new Offer("Reclutamiento","Sin partida",null,null);
            if(state.BastionLevel<2 || state.BarracksLevel<1)
                return new Offer("Reclutar","Requiere Cuartel construido en Bastión II.",null,null);
            if(state.RecruitmentCompletesUtcTicks>0)
                return new Offer("Reclutamiento en curso",
                    "Quedan "+Remaining(state.RecruitmentCompletesUtcTicks,now)+
                    " · "+state.PendingRecruitArchers+" arqueros",null,null);
            var count=SliceRules.RecruitArchers;
            return new Offer("Reclutar "+count+" arqueros T1",
                "Se incorporan cuando finalice el temporizador del sistema existente.",
                "Recruit","archer:t1",SliceRules.RecruitWoodCostFor(count),
                SliceRules.RecruitStoneCostFor(count),0,SliceRules.RecruitSecondsFor(count));
        }
    }
}
