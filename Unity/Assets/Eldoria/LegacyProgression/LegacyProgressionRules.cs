using System;
using System.Collections.Generic;
using System.Linq;

namespace Eldoria.LegacyProgression
{
    // A projection of earned facts, not a new wallet, economy or save.
    public sealed class ProgressFacts
    {
        public int Bastion, TotalPower, KingdomPower, ArmyPower, VigilantStage,
            ScoutReports, DiscoveryMask, RecordedHunts;
        public bool JourneyComplete, RouteCleared, EngendroDefeated, LyraContacted,
            FissureExplored, FissureClosed, LyraRecruited, AshStalkerDefeated, HasWolfHide, HasBoarTusk;
    }

    public sealed class Trophy
    {
        public readonly string Id, Name;
        public readonly int Prestige, MinBastion;
        public readonly Func<ProgressFacts, bool> Earned;
        public Trophy(string id,string name,int prestige,int minBastion,Func<ProgressFacts,bool> earned)
        { Id=id; Name=name; Prestige=prestige; MinBastion=minBastion; Earned=earned; }
    }

    public sealed class Cosmetic
    {
        public readonly string Id, Name, TrophyId;
        public readonly int MinBastion;
        public Cosmetic(string id,string name,string trophyId,int minBastion)
        { Id=id; Name=name; TrophyId=trophyId; MinBastion=minBastion; }
    }

    public sealed class Discovery
    {
        public readonly string Id, Name;
        public readonly Func<ProgressFacts,bool> Found;
        public Discovery(string id,string name,Func<ProgressFacts,bool> found)
        { Id=id; Name=name; Found=found; }
    }

    public sealed class DiscoveryCollection
    {
        public readonly string Id, Name;
        public readonly int MinBastion;
        public readonly IReadOnlyList<Discovery> Entries;
        public DiscoveryCollection(string id,string name,int minBastion,params Discovery[] entries)
        { Id=id; Name=name; MinBastion=minBastion; Entries=Array.AsReadOnly(entries); }
        public int Count(ProgressFacts facts) => Entries.Count(e=>e.Found(facts));
    }

    public sealed class HonorTitle
    {
        public readonly string Id, Name;
        public readonly int Prestige;
        public HonorTitle(string id,string name,int prestige){Id=id;Name=name;Prestige=prestige;}
    }

    public enum RankingKind { TotalPower, Kingdom, Army, Vigilants, Hunters, Explorers }
    public enum RankingPeriod { Permanent, Weekly, Season }

    public static class LegacyProgressionRules
    {
        public static int DiscoveryCount(int mask)
        { int count=0; for(int i=0;i<4;i++)if((mask&(1<<i))!=0)count++;return count; }

        static Trophy Bastion(int level,string name,int points) =>
            new Trophy("bastion-"+level,name,points,level,f=>f.Bastion>=level);
        static Trophy Vigilant(int stage,int points) =>
            new Trophy("vigilant-"+stage,"Vigilante · etapa "+stage,points,2,f=>f.VigilantStage>=stage);
        static Trophy Hunter(int count,int points) =>
            new Trophy("hunt-"+count,"Cazador · "+count+" cacerías",points,3,f=>f.RecordedHunts>=count);
        static Trophy Scout(int count,int points) =>
            new Trophy("scout-"+count,"Explorador · "+count+" informes",points,3,f=>f.ScoutReports>=count);

        // Stable IDs; award thresholds are initial tuning, not economy bonuses.
        public static readonly IReadOnlyList<Trophy> Trophies=Array.AsReadOnly(new[] {
            Bastion(2,"Primer renacer",10),Bastion(3,"La frontera se abre",20),
            Bastion(4,"Reino en expansión",30),Bastion(5,"Fundador de Valoria",50),
            Bastion(6,"El reino perdura",60),Bastion(7,"Custodio del legado",70),
            Bastion(8,"Más allá de Valoria",80),Bastion(9,"Señor de la frontera",100),
            Bastion(10,"El renacer del reino",150),
            Vigilant(1,10),Vigilant(5,20),Vigilant(10,30),Vigilant(25,50),Vigilant(50,75),Vigilant(100,150),
            Hunter(1,10),Hunter(10,30),Hunter(25,50),Hunter(50,100),
            Scout(1,10),Scout(3,20),Scout(10,50),Scout(25,100),
            new Trophy("discovery-1","Primer hallazgo",10,3,f=>DiscoveryCount(f.DiscoveryMask)>=1),
            new Trophy("discovery-2","Tras las huellas",25,3,f=>DiscoveryCount(f.DiscoveryMask)>=2),
            new Trophy("discovery-4","Cronista de la frontera",75,3,f=>DiscoveryCount(f.DiscoveryMask)==4),
            new Trophy("engendro","El primer guardián",30,2,f=>f.EngendroDefeated),
            new Trophy("fissure","La fisura cerrada",50,3,f=>f.FissureClosed),
            new Trophy("lyra","Una nueva aliada",25,3,f=>f.LyraRecruited),
            new Trophy("ash-stalker","Vencedor de la ceniza",50,4,f=>f.AshStalkerDefeated)
        });
        public static readonly IReadOnlyList<HonorTitle> Titles=Array.AsReadOnly(new[] {
            new HonorTitle("pioneer","Pionero",0),new HonorTitle("watchman","Vigía",50),
            new HonorTitle("guardian","Guardián",100),new HonorTitle("knight","Caballero",200),
            new HonorTitle("warden","Custodio",350),new HonorTitle("chronicler","Cronista",500),
            new HonorTitle("champion","Campeón",700),new HonorTitle("marshal","Mariscal",950),
            new HonorTitle("legend","Leyenda",1250),new HonorTitle("legacy","Legado de Eldoria",1500)
        });
        public static readonly IReadOnlyList<Cosmetic> Banners=Array.AsReadOnly(new[] {
            new Cosmetic("banner-founder","Fundador","bastion-5",5),
            new Cosmetic("banner-watch","Vigilia","vigilant-5",4),
            new Cosmetic("banner-guard","Guardia real","vigilant-25",4),
            new Cosmetic("banner-hunt","Cazadores","hunt-10",4),
            new Cosmetic("banner-frontier","Frontera","discovery-4",4),
            new Cosmetic("banner-fissure","La fisura","fissure",4),
            new Cosmetic("banner-lyra","Juramento de Lyra","lyra",4),
            new Cosmetic("banner-ash","Ceniza","ash-stalker",4)
        });
        public static readonly IReadOnlyList<Cosmetic> Monuments=Array.AsReadOnly(new[] {
            new Cosmetic("monument-founder","Estatua del fundador","bastion-5",5),
            new Cosmetic("monument-guard","Fuente de los Vigilantes","vigilant-25",5),
            new Cosmetic("monument-hunt","Trofeo de caza","hunt-25",5),
            new Cosmetic("monument-fissure","Memorial de la fisura","fissure",5),
            new Cosmetic("monument-ash","Memorial de ceniza","ash-stalker",5)
        });
        public static readonly IReadOnlyList<DiscoveryCollection> Collections=Array.AsReadOnly(new[] {
            new DiscoveryCollection("ashes","Las cenizas de Valoria",3,
                new Discovery("journey","Primer viaje",f=>f.JourneyComplete),
                new Discovery("route","Ruta despejada",f=>f.RouteCleared),
                new Discovery("engendro","El Engendro",f=>f.EngendroDefeated)),
            new DiscoveryCollection("fissure","Crónica de la fisura",3,
                new Discovery("investigation","Investigación",f=>f.FissureExplored),
                new Discovery("contact","Primer contacto",f=>f.LyraContacted||f.LyraRecruited),
                new Discovery("closure","Cierre de la fisura",f=>f.FissureClosed),
                new Discovery("alliance","Una nueva alianza",f=>f.LyraRecruited)),
            new DiscoveryCollection("frontier","Informes de la frontera",3,
                new Discovery("patrol","Guardia en la frontera",f=>(f.DiscoveryMask&1)!=0),
                new Discovery("travelers","Voces del camino",f=>(f.DiscoveryMask&2)!=0),
                new Discovery("supplies","Marcas en los suministros",f=>(f.DiscoveryMask&4)!=0),
                new Discovery("breach","Ecos de la Brecha",f=>(f.DiscoveryMask&8)!=0))
        });
        public static IReadOnlyList<Trophy> Earned(ProgressFacts facts) =>
            Trophies.Where(t=>facts.Bastion>=t.MinBastion&&t.Earned(facts)).ToArray();
        public static int Prestige(ProgressFacts facts) => Earned(facts).Sum(t=>t.Prestige);
        public static IReadOnlyList<HonorTitle> EarnedTitles(ProgressFacts facts) =>
            facts.Bastion<4 ? Array.Empty<HonorTitle>() : Titles.Where(t=>t.Prestige<=Prestige(facts)).ToArray();
        public static IReadOnlyList<Cosmetic> Unlocked(ProgressFacts facts,IReadOnlyList<Cosmetic> catalog)
        {
            var earned=new HashSet<string>(Earned(facts).Select(t=>t.Id),StringComparer.Ordinal);
            return catalog.Where(c=>facts.Bastion>=c.MinBastion&&earned.Contains(c.TrophyId)).ToArray();
        }

        // Personal observations only. Chapter hunt counter is not a global hunting score.
        // No public standings can be inferred from a local save.
        public static long? PersonalMetric(ProgressFacts f,RankingKind kind)
        {
            switch(kind)
            {
                case RankingKind.TotalPower:return f.TotalPower;
                case RankingKind.Kingdom:return f.KingdomPower;
                case RankingKind.Army:return f.ArmyPower;
                case RankingKind.Vigilants:return f.Bastion>=2?(long?)f.VigilantStage:null;
                case RankingKind.Hunters:return null;
                case RankingKind.Explorers:return f.Bastion>=3?(long?)DiscoveryCount(f.DiscoveryMask):null;
                default:throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }

    public sealed class LocalProfile
    {
        public readonly string Name;
        public readonly ProgressFacts Facts;
        public readonly LegacySavedProgress Saved;
        public LocalProfile(string name,ProgressFacts facts,LegacySavedProgress saved=null)
        { Name=name??""; Facts=facts??throw new ArgumentNullException(nameof(facts)); Saved=saved?.Copy(); }
        public int Prestige => Saved?.Prestige??LegacyProgressionRules.Prestige(Facts);
        public bool ProfileUnlocked => Facts.Bastion>=4;
        public bool RankingsVisible => Facts.Bastion>=2;
        public IReadOnlyList<Trophy> Trophies => Saved?.Earned()??LegacyProgressionRules.Earned(Facts);
        public IReadOnlyList<HonorTitle> Titles => Saved?.Titles(Facts)??LegacyProgressionRules.EarnedTitles(Facts);
        public IReadOnlyList<Cosmetic> Banners => Saved?.Cosmetics(Facts,LegacyProgressionRules.Banners)??LegacyProgressionRules.Unlocked(Facts,LegacyProgressionRules.Banners);
        public IReadOnlyList<Cosmetic> Monuments => Saved?.Cosmetics(Facts,LegacyProgressionRules.Monuments)??LegacyProgressionRules.Unlocked(Facts,LegacyProgressionRules.Monuments);
        public IReadOnlyList<DiscoveryCollection> Discoveries =>
            LegacyProgressionRules.Collections.Where(c=>Facts.Bastion>=c.MinBastion).ToArray();
        // Distinguish "not connected" from an empty real ranking; never invent rows.
        public bool PublicRankingsAvailable => false;
    }
}
