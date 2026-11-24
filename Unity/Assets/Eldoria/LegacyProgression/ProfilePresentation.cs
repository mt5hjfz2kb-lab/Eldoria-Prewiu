using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Eldoria.LegacyProgression
{
    public enum ProfileSection { Overview, Rankings, Trophies, Discoveries, Heraldry, Monuments }
    public enum ProfileEntryState { Information, Earned, Available, Locked, Unavailable }
    public sealed class ProfileEntry
    {
        public readonly string Key, Title, Detail;
        public readonly ProfileEntryState State;
        public ProfileEntry(string key,string title,string detail,ProfileEntryState state)
        { Key=key;Title=title;Detail=detail;State=state; }
    }

    // All display decisions are independent of art and UI layout, and read the existing save.
    public static class ProfilePresentation
    {
        static readonly CultureInfo Es=CultureInfo.GetCultureInfo("es-ES");
        static string N(long v)=>v.ToString("N0",Es);
        static string Ratio(int v,int total)=>N(v)+" / "+N(total);
        static ProfileEntry Entry(string key,string title,string detail,ProfileEntryState state=ProfileEntryState.Information)
            =>new ProfileEntry(key,title,detail,state);
        public static IReadOnlyList<ProfileSection> Sections(LocalProfile p)
        {
            if(p==null)throw new ArgumentNullException(nameof(p));
            var list=new List<ProfileSection>{ProfileSection.Overview};
            if(p.RankingsVisible)list.AddRange(new[]{ProfileSection.Rankings,ProfileSection.Trophies});
            if(p.Facts.Bastion>=3)list.Add(ProfileSection.Discoveries);
            if(p.ProfileUnlocked)list.Add(ProfileSection.Heraldry);
            if(p.Facts.Bastion>=5)list.Add(ProfileSection.Monuments);
            return list.ToArray();
        }
        public static string Label(ProfileSection s)
        {
            switch(s)
            {
                case ProfileSection.Overview:return "Perfil";
                case ProfileSection.Rankings:return "Clasificaciones";
                case ProfileSection.Trophies:return "Trofeos";
                case ProfileSection.Discoveries:return "Hallazgos";
                case ProfileSection.Heraldry:return "Heráldica";
                case ProfileSection.Monuments:return "Monumentos";
                default:throw new ArgumentOutOfRangeException(nameof(s));
            }
        }
        public static string RankingLabel(RankingKind k)
        {
            switch(k)
            {
                case RankingKind.TotalPower:return "Poder total";
                case RankingKind.Kingdom:return "Reino";
                case RankingKind.Army:return "Ejército";
                case RankingKind.Vigilants:return "Vigilantes";
                case RankingKind.Hunters:return "Cazadores";
                case RankingKind.Explorers:return "Exploradores";
                default:throw new ArgumentOutOfRangeException(nameof(k));
            }
        }
        public static IReadOnlyList<ProfileEntry> Rows(LocalProfile p,ProfileSection section)
        {
            if(!Sections(p).Contains(section))
                return new[]{Entry("locked","Todavía no disponible","Avanza en Valoria para desbloquearlo.",ProfileEntryState.Locked)};
            var f=p.Facts;
            switch(section)
            {
                case ProfileSection.Overview:
                    var rows=new List<ProfileEntry>{
                        Entry("name",string.IsNullOrWhiteSpace(p.Name)?"Tu reino":p.Name,"Bastión "+N(f.Bastion)),
                        Entry("power","Poder total",N(f.TotalPower)),
                        Entry("kingdom","Reino",N(f.KingdomPower)),
                        Entry("army","Ejército",N(f.ArmyPower))};
                    if(f.Bastion>=2)rows.Add(Entry("trophies","Trofeos obtenidos",N(p.Trophies.Count)));
                    if(f.Bastion>=3)rows.Add(Entry("discoveries","Hallazgos de Exploradores",N(LegacyProgressionRules.DiscoveryCount(f.DiscoveryMask))));
                    if(p.ProfileUnlocked)
                    {
                        var title=p.Titles.LastOrDefault();
                        rows.Add(Entry("title","Título",title?.Name??"Sin título"));
                        rows.Add(Entry("prestige","Prestigio",N(p.Prestige)));
                        var next=LegacyProgressionRules.Titles.FirstOrDefault(t=>t.Prestige>p.Prestige);
                        if(next!=null)rows.Add(Entry("next-title","Próximo título",next.Name+" · "+N(next.Prestige-p.Prestige)+" puntos restantes"));
                    }
                    return rows;
                case ProfileSection.Rankings:
                    var rankingRows=new List<ProfileEntry>{
                        Entry("offline","Clasificaciones del reino","Servicio competitivo todavía no conectado. Se muestran únicamente tus estadísticas.",ProfileEntryState.Unavailable)};
                    foreach(RankingKind k in Enum.GetValues(typeof(RankingKind)))
                    {
                        var value=LegacyProgressionRules.PersonalMetric(f,k);
                        rankingRows.Add(Entry("ranking:"+k,RankingLabel(k),value.HasValue?N(value.Value)+" · marca personal":
                            k==RankingKind.Hunters?"Registro completo de caza pendiente":"Todavía no disponible",
                            value.HasValue?ProfileEntryState.Information:ProfileEntryState.Unavailable));
                    }
                    return rankingRows;
                case ProfileSection.Trophies:
                    // Avoid revealing future lore or naming unearned narrative achievements.
                    return LegacyProgressionRules.Trophies.Where(t=>t.MinBastion<=f.Bastion)
                        .Select(t=>{bool earned=t.Earned(f);bool secret=t.Id=="engendro"||t.Id=="fissure"||t.Id=="lyra"||t.Id=="ash-stalker";
                            return Entry(t.Id,!earned&&secret?"Hazaña por descubrir":t.Name,
                                earned?"+ "+N(t.Prestige)+" prestigio":secret?"Continúa tu aventura":"Pendiente de completar",
                                earned?ProfileEntryState.Earned:ProfileEntryState.Locked);}).ToArray();
                case ProfileSection.Discoveries:
                    return p.Discoveries.Select(c=>Entry(c.Id,c.Name,Ratio(c.Count(f),c.Entries.Count),
                        c.Count(f)==c.Entries.Count?ProfileEntryState.Earned:ProfileEntryState.Information)).ToArray();
                case ProfileSection.Heraldry:
                    return CosmeticRows(p,LegacyProgressionRules.Banners);
                case ProfileSection.Monuments:
                    return CosmeticRows(p,LegacyProgressionRules.Monuments);
                default:throw new ArgumentOutOfRangeException(nameof(section));
            }
        }
        public static IReadOnlyList<ProfileEntry> DiscoveryEntries(LocalProfile p,string collectionId)
        {
            var c=p.Discoveries.FirstOrDefault(x=>x.Id==collectionId);
            if(c==null)return Array.Empty<ProfileEntry>();
            return c.Entries.Select(e=>{bool found=e.Found(p.Facts);return Entry(e.Id,found?e.Name:"Hallazgo desconocido",
                found?"Descubierto":"Por descubrir",found?ProfileEntryState.Earned:ProfileEntryState.Locked);}).ToArray();
        }
        static IReadOnlyList<ProfileEntry> CosmeticRows(LocalProfile p,IReadOnlyList<Cosmetic> catalog)
        {
            var unlocked=new HashSet<string>(LegacyProgressionRules.Unlocked(p.Facts,catalog).Select(c=>c.Id),StringComparer.Ordinal);
            return catalog.Where(c=>c.MinBastion<=p.Facts.Bastion).Select(c=>Entry(c.Id,
                unlocked.Contains(c.Id)?c.Name:"Distinción por descubrir",
                unlocked.Contains(c.Id)?"Conseguida · presentación pendiente":"Completa la hazaña asociada",
                unlocked.Contains(c.Id)?ProfileEntryState.Earned:ProfileEntryState.Locked)).ToArray();
        }
    }
}
