using System;
using System.Linq;
using Eldoria.LegacyProgression;

namespace Eldoria.Domain
{
    // Candidate for canonical integration, compiled against real copied domain.
    public static class CanonicalLegacyFacts
    {
        public static ProgressFacts Read(PlayerState s)
        {
            var power=SliceRules.TotalPower(s);
            var c=s.ChapterThree;var r=s.Retention;
            // Chapter hunt counter already contains legacy wolf/boar victories.
            // Add only new repeatable regional fauna; never count those twice.
            int regional=s.LegacyProgression?.Ledger?.Receipts?.Count(e=>e!=null&&
                e.Kind==ReceiptKind.HuntVictory&&e.SubjectId=="regional:boar")??0;
            return new ProgressFacts{
                Bastion=s.BastionLevel,TotalPower=power.Total,KingdomPower=power.Kingdom,
                ArmyPower=power.Army,VigilantStage=r?.LastClearedStage??0,
                ScoutReports=r?.TotalScoutReports??0,DiscoveryMask=r?.ScoutDiscoveryMask??0,
                RecordedHunts=checked((c?.Hunts??0)+regional),JourneyComplete=s.JourneyComplete,
                RouteCleared=s.ChapterProgress?.RouteCleared??false,
                EngendroDefeated=s.EngendroDefeated||s.ChapterProgress?.EngendroDefeated==true,
                LyraContacted=c?.LyraContacted??false,FissureExplored=c?.FissureExplored??false,
                FissureClosed=c?.FissureClosed??false,LyraRecruited=c?.LyraRecruited??false,
                AshStalkerDefeated=c?.AshStalkerDefeated??false,
                HasWolfHide=c?.Inventory?.Contains("treated-hide")??false,HasBoarTusk=c?.Inventory?.Contains("stone-tusk")??false
            };
        }
        public static bool Reconcile(PlayerState s) => s.LegacyProgression.Reconcile(Read(s));
        public static void Record(PlayerState s,string id,ReceiptKind kind,string subject,long ticks,long amount,int level)
        {
            if(!s.LegacyProgression.Ledger.RecordCommitted(new ProgressReceipt{
                Id=id,Kind=kind,SubjectId=subject,PlayerId=s.PlayerId,WorldId=s.WorldId,
                UtcTicks=ticks,Amount=amount,Level=level}))
                throw new InvalidOperationException("Invalid or duplicate progression receipt: "+id);
        }
    }
}
