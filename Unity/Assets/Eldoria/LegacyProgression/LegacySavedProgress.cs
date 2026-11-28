using System;
using System.Collections.Generic;
using System.Linq;

namespace Eldoria.LegacyProgression
{
    // Additive state for the canonical save. No PlayerPrefs, auxiliary file or wallet.
    [Serializable] public sealed class LegacySavedProgress
    {
        public int SchemaVersion=1;
        public List<string> EarnedTrophyIds=new List<string>();
        public string SelectedTitle="", SelectedBanner="", SelectedMonument="";
        public LegacyReceiptLedger Ledger=new LegacyReceiptLedger();

        public LegacySavedProgress Copy() => new LegacySavedProgress {
            SchemaVersion=SchemaVersion,
            EarnedTrophyIds=new List<string>(EarnedTrophyIds??new List<string>()),
            SelectedTitle=SelectedTitle, SelectedBanner=SelectedBanner,SelectedMonument=SelectedMonument,
            Ledger=new LegacyReceiptLedger{
                PlayerId=Ledger?.PlayerId??"",WorldId=Ledger?.WorldId??"",
                Receipts=(Ledger?.Receipts??new List<ProgressReceipt>()).Where(r=>r!=null).Select(r=>r.Copy()).ToList()
            }
        };

        public void BindIdentity(string playerId,string worldId)
        {
            if(SchemaVersion!=1)throw new InvalidOperationException("Unsupported legacy progression schema");
            if(string.IsNullOrWhiteSpace(playerId)||string.IsNullOrWhiteSpace(worldId))
                throw new ArgumentException("Player/world identity required");
            if(Ledger==null)Ledger=new LegacyReceiptLedger();
            if(!string.IsNullOrEmpty(Ledger.PlayerId)&&Ledger.PlayerId!=playerId ||
               !string.IsNullOrEmpty(Ledger.WorldId)&&Ledger.WorldId!=worldId)
                throw new InvalidOperationException("Legacy progression ownership mismatch");
            Ledger.PlayerId=playerId;Ledger.WorldId=worldId;
            if(EarnedTrophyIds==null)EarnedTrophyIds=new List<string>();
        }

        public bool Reconcile(ProgressFacts facts)
        {
            var known=new HashSet<string>(LegacyProgressionRules.Trophies.Select(t=>t.Id),StringComparer.Ordinal);
            var ids=new HashSet<string>((EarnedTrophyIds??new List<string>()).Where(known.Contains),StringComparer.Ordinal);
            foreach(var t in LegacyProgressionRules.Earned(facts))ids.Add(t.Id);
            var next=LegacyProgressionRules.Trophies.Where(t=>ids.Contains(t.Id)).Select(t=>t.Id).ToList();
            bool changed=EarnedTrophyIds==null||!EarnedTrophyIds.SequenceEqual(next);
            EarnedTrophyIds=next;
            if(!ValidSelection(facts,"title",SelectedTitle)){SelectedTitle="";changed=true;}
            if(!ValidSelection(facts,"banner",SelectedBanner)){SelectedBanner="";changed=true;}
            if(!ValidSelection(facts,"monument",SelectedMonument)){SelectedMonument="";changed=true;}
            return changed;
        }

        public IReadOnlyList<Trophy> Earned()
        {
            var ids=new HashSet<string>(EarnedTrophyIds??new List<string>(),StringComparer.Ordinal);
            return LegacyProgressionRules.Trophies.Where(t=>ids.Contains(t.Id)).ToArray();
        }
        public int Prestige => Earned().Sum(t=>t.Prestige);
        public IReadOnlyList<HonorTitle> Titles(ProgressFacts f) =>
            f.Bastion<4?Array.Empty<HonorTitle>():LegacyProgressionRules.Titles.Where(t=>t.Prestige<=Prestige).ToArray();
        public IReadOnlyList<Cosmetic> Cosmetics(ProgressFacts f,IReadOnlyList<Cosmetic> catalog)
        {
            var ids=new HashSet<string>(Earned().Select(t=>t.Id),StringComparer.Ordinal);
            return catalog.Where(c=>f.Bastion>=c.MinBastion&&ids.Contains(c.TrophyId)).ToArray();
        }
        bool ValidSelection(ProgressFacts f,string kind,string id)
        {
            if(string.IsNullOrEmpty(id))return true;
            if(kind=="title")return Titles(f).Any(t=>t.Id==id);
            if(kind=="banner")return Cosmetics(f,LegacyProgressionRules.Banners).Any(t=>t.Id==id);
            if(kind=="monument")return Cosmetics(f,LegacyProgressionRules.Monuments).Any(t=>t.Id==id);
            return false;
        }
        // Only invoke from the validated owner command path. Cosmetics confer no stats.
        public bool TrySelect(ProgressFacts f,string kind,string id)
        {
            if(id==null||!ValidSelection(f,kind,id))return false;
            switch(kind)
            {
                case "title":if(f.Bastion<4)return false;SelectedTitle=id;return true;
                case "banner":if(f.Bastion<4)return false;SelectedBanner=id;return true;
                case "monument":if(f.Bastion<5)return false;SelectedMonument=id;return true;
                default:return false;
            }
        }
    }
}
