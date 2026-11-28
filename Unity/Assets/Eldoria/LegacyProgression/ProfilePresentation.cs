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
        public readonly string Key, Title, Detail, CommandKind, CommandTarget;
        public readonly ProfileEntryState State;
        public readonly int RankingPosition;
        public ProfileEntry(string key,string title,string detail,ProfileEntryState state,string commandKind="",string commandTarget="",int rankingPosition=0)
        { Key=key;Title=title;Detail=detail;State=state;CommandKind=commandKind;CommandTarget=commandTarget;RankingPosition=rankingPosition; }
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
        public static string RankingMetricLabel(RankingKind kind)
        {
            switch(kind)
            {
                case RankingKind.TotalPower:return "Poder total";
                case RankingKind.Kingdom:return "Poder del reino";
                case RankingKind.Army:return "Poder militar";
                case RankingKind.Vigilants:return "Etapa superada";
                case RankingKind.Hunters:return "Puntos de caza";
                case RankingKind.Explorers:return "Hallazgos";
                default:throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
        // Only exact category keys are navigation targets; notices and local weekly summaries are not.
        public static bool TryRankingKind(string key,out RankingKind kind)
        {
            foreach(RankingKind k in Enum.GetValues(typeof(RankingKind)))
                if(string.Equals(key,"ranking:"+k,StringComparison.Ordinal)){kind=k;return true;}
            kind=default(RankingKind);return false;
        }
        public static IReadOnlyList<ProfileEntry> RankingDetailRows(LocalProfile p,RankingKind kind,RankingDetailSnapshot data)
        {
            if(p==null)throw new ArgumentNullException(nameof(p));
            if(!p.RankingsVisible)return new[]{Entry("locked","Todavía no disponible","Avanza en Valoria para desbloquearlo.",ProfileEntryState.Locked)};
            RankingLabel(kind); // reject undefined enum values
            if(data==null||data.Kind!=kind||data.Status==RankingDetailStatus.Unavailable)
                return new[]{Entry("status","Clasificación no disponible","El servicio competitivo todavía no está conectado o no está disponible.",ProfileEntryState.Unavailable)};
            if(data.Status==RankingDetailStatus.Loading)return new[]{Entry("status","Cargando clasificación","Consultando las posiciones del reino.")};
            if(data.Status==RankingDetailStatus.Error)return new[]{Entry("status","No se pudo cargar","Inténtalo de nuevo más tarde.",ProfileEntryState.Unavailable)};
            var result=new List<ProfileEntry>{Entry("period",data.PeriodLabel,"Posición · Jugador · "+RankingMetricLabel(kind))};
            if(data.Rows.Count==0)result.Add(data.Offset==0?Entry("empty","Aún no hay jugadores clasificados","Las posiciones aparecerán cuando haya resultados válidos."):Entry("empty-page","No hay más resultados","Regresa a la página anterior."));
            foreach(var row in data.Rows)
                result.Add(new ProfileEntry("place:"+N(row.Position),N(row.Position)+". "+row.DisplayName,N(row.Score),ProfileEntryState.Information,"","",row.Position));
            if(data.OwnRow!=null)result.Add(Entry("own-position","Tu posición",N(data.OwnRow.Position)+" · "+N(data.OwnRow.Score)));
            else result.Add(Entry("own-position","Tu posición","Todavía sin clasificación"));
            return result;
        }
        public static IReadOnlyList<ProfileEntry> Rows(LocalProfile p,ProfileSection section,DateTime? localUtc=null)
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
                        var title=p.Titles.FirstOrDefault(t=>t.Id==p.Saved?.SelectedTitle)??p.Titles.LastOrDefault();
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
                        string qualifier=" · marca personal";
                        if(k==RankingKind.Hunters&&f.Bastion>=3&&p.Saved?.Ledger!=null&&localUtc.HasValue)
                        {
                            long start=LegacyReceiptLedger.WeekStartUtc(localUtc.Value).Ticks;
                            value=p.Saved.Ledger.HuntPoints(start,Math.Min(start+TimeSpan.FromDays(7).Ticks,localUtc.Value.Ticks+1));
                            qualifier=" · puntos esta semana (local)";
                        }
                        rankingRows.Add(Entry("ranking:"+k,RankingLabel(k),value.HasValue?N(value.Value)+qualifier:
                            k==RankingKind.Hunters?"Registro completo de caza pendiente":"Todavía no disponible",
                            value.HasValue?ProfileEntryState.Information:ProfileEntryState.Unavailable));
                    }
                    if(f.Bastion>=3&&p.Saved?.Ledger!=null&&localUtc.HasValue)
                    {
                        long start=LegacyReceiptLedger.WeekStartUtc(localUtc.Value).Ticks;
                        long end=Math.Min(start+TimeSpan.FromDays(7).Ticks,localUtc.Value.Ticks+1);
                        rankingRows.Add(Entry("week:discoveries","Exploración semanal",N(p.Saved.Ledger.UniqueDiscoveries(start,end))+" · familias únicas (local)"));
                        rankingRows.Add(Entry("week:gathered","Recolección semanal",N(p.Saved.Ledger.Gathered(start,end))+" · recursos de nodos (local)"));
                    }
                    return rankingRows;
                case ProfileSection.Trophies:
                    // Avoid revealing future lore or naming unearned narrative achievements.
                    return LegacyProgressionRules.Trophies.Where(t=>t.MinBastion<=f.Bastion)
                        .Select(t=>{bool earned=p.Trophies.Any(x=>x.Id==t.Id);bool secret=t.Id=="engendro"||t.Id=="fissure"||t.Id=="lyra"||t.Id=="ash-stalker";
                            return Entry(t.Id,!earned&&secret?"Hazaña por descubrir":t.Name,
                                earned?"+ "+N(t.Prestige)+" prestigio":secret?"Continúa tu aventura":"Pendiente de completar",
                                earned?ProfileEntryState.Earned:ProfileEntryState.Locked);}).ToArray();
                case ProfileSection.Discoveries:
                    return p.Discoveries.Select(c=>Entry(c.Id,c.Name,Ratio(c.Count(f),c.Entries.Count),
                        c.Count(f)==c.Entries.Count?ProfileEntryState.Earned:ProfileEntryState.Information)).ToArray();
                case ProfileSection.Heraldry:
                    return TitleRows(p).Concat(CosmeticRows(p,LegacyProgressionRules.Banners)).ToArray();
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
            var unlocked=new HashSet<string>((catalog==LegacyProgressionRules.Banners?p.Banners:p.Monuments).Select(c=>c.Id),StringComparer.Ordinal);
            bool banner=catalog==LegacyProgressionRules.Banners;
            string selected=banner?p.Saved?.SelectedBanner:p.Saved?.SelectedMonument;
            string command=banner?"SelectLegacyBanner":"SelectLegacyMonument";
            var rows=catalog.Where(c=>c.MinBastion<=p.Facts.Bastion).Select(c=>{
                bool earned=unlocked.Contains(c.Id);
                bool active=earned&&selected==c.Id;
                return new ProfileEntry(c.Id,earned?c.Name:"Distinción por descubrir",
                    active?"Seleccionada · arte pendiente":earned?"Conseguida · arte pendiente":"Completa la hazaña asociada",
                    earned?ProfileEntryState.Earned:ProfileEntryState.Locked,
                    earned&&!active&&p.Saved!=null?command:"",c.Id);
            }).ToList();
            if(!string.IsNullOrEmpty(selected))rows.Insert(0,new ProfileEntry("clear:"+command,"Retirar selección",
                "No afecta al poder ni al progreso",ProfileEntryState.Available,command,""));
            return rows;
        }
        static IEnumerable<ProfileEntry> TitleRows(LocalProfile p)
        {
            var unlocked=new HashSet<string>(p.Titles.Select(t=>t.Id),StringComparer.Ordinal);
            foreach(var t in LegacyProgressionRules.Titles)
            {
                bool earned=unlocked.Contains(t.Id), selected=p.Saved?.SelectedTitle==t.Id;
                yield return new ProfileEntry("title:"+t.Id,t.Name,
                    selected?"Título seleccionado":earned?"Título conseguido":"Requiere "+N(t.Prestige)+" prestigio",
                    earned?ProfileEntryState.Earned:ProfileEntryState.Locked,
                    earned&&!selected&&p.Saved!=null?"SelectLegacyTitle":"",t.Id);
            }
        }
    }
}
