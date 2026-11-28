using System;
using System.Collections.Generic;
using System.Linq;

namespace Eldoria.LegacyProgression
{
    public sealed class BestiaryRecord
    {
        public readonly string Id,Name;
        public readonly bool Known;
        public readonly int RecordedVictories,HighestRecordedTier;
        public BestiaryRecord(string id,string name,bool known,int victories,int tier)
        {Id=id;Name=known?name:"Criatura por descubrir";Known=known;RecordedVictories=victories;HighestRecordedTier=tier;}
    }
    // Consumer for the future Códice VII. Do not invent sightings or historical counts.
    public static class BestiaryProjection
    {
        public static IReadOnlyList<BestiaryRecord> Read(LocalProfile p)
        {
            if(p.Facts.Bastion<7)return Array.Empty<BestiaryRecord>();
            var receipts=p.Saved?.Ledger?.Receipts??new List<ProgressReceipt>();
            var ledger=p.Saved?.Ledger;
            var victories=receipts.Where(r=>r!=null&&r.Kind==ReceiptKind.HuntVictory&&
                r.PlayerId==ledger?.PlayerId&&r.WorldId==ledger?.WorldId)
                .GroupBy(r=>r.Id,StringComparer.Ordinal).Select(g=>g.First()).ToArray();
            BestiaryRecord Entry(string id,string label,bool historic,params string[] keys)
            {
                var records=victories.Where(r=>keys.Contains(r.SubjectId)).ToArray();
                var tiers=records.Where(r=>r.SubjectId.StartsWith("regional:",StringComparison.Ordinal)).Select(r=>r.Level);
                return new BestiaryRecord(id,label,historic||records.Length>0,records.Length,tiers.DefaultIfEmpty(0).Max());
            }
            return new[]{
                Entry("wolf","Lobo",p.Facts.HasWolfHide,"legacy:wolf"),
                Entry("boar","Jabalí de roca",p.Facts.HasBoarTusk,"legacy:boar","regional:boar"),
                Entry("spawnling","Engendro de la Brecha",p.Facts.EngendroDefeated||p.Trophies.Any(t=>t.Id=="engendro"),"regional:spawnling"),
                Entry("ash-stalker","Acechador de Ceniza",p.Facts.AshStalkerDefeated||p.Trophies.Any(t=>t.Id=="ash-stalker"))
            };
        }
    }
}
