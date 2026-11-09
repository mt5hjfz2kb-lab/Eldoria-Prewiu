using System;
using System.Collections.Generic;
using Eldoria.Domain;

namespace Eldoria.DailyTreasures
{
    // A player-experience queue, not a wallet or alternate game state.
    // It only announces milestones which the authoritative gateway has
    // already committed AND recorded in the player's existing mailbox.
    // The first snapshot is a baseline: offline events go to the mailbox,
    // not an unsolicited opening ceremony during a fresh session.
    public readonly struct DailyTreasureMilestoneNotice
    {
        public readonly string EventId;
        public readonly string Title;
        public readonly string Detail;
        public readonly bool Legendary;

        public DailyTreasureMilestoneNotice(string eventId,string title,
            string detail,bool legendary=true)
        {
            EventId=eventId;Title=title;Detail=detail;Legendary=legendary;
        }
    }

    public sealed class DailyTreasureMilestoneQueue
    {
        readonly Queue<DailyTreasureMilestoneNotice> pending=
            new Queue<DailyTreasureMilestoneNotice>();
        readonly HashSet<string> seen=new HashSet<string>(StringComparer.Ordinal);
        PlayerState previous;

        public int PendingCount => pending.Count;

        // Called once per player/gateway binding. Do not call when a panel is
        // merely closed or when UI gets a redraw; that would lose transitions.
        public void Reset(PlayerState snapshot)
        {
            previous=snapshot;
            seen.Clear();
            pending.Clear();
        }

        static EldoriaV41Message FindCommittedNotice(PlayerState s,string id,string kind)
        {
            if(s?.Mailbox?.Messages==null)return null;
            return s.Mailbox.Messages.Find(m=>
                m!=null&&m.EventId==id&&m.Type==kind);
        }

        void AddIfCommitted(PlayerState after,string id,string kind)
        {
            if(seen.Contains(id))return;
            var mail=FindCommittedNotice(after,id,kind);
            if(mail==null || string.IsNullOrWhiteSpace(mail.Title))return;
            // No invented items or reward amounts. The existing report
            // explains the genuine achievement to the player.
            pending.Enqueue(new DailyTreasureMilestoneNotice(
                id,mail.Title,mail.Subtitle??""));
            seen.Add(id);
        }

        // Do not infer an earned chapter just because a battle-mail ID exists.
        // The saved report contains the actual outcome and target. Requiring
        // both prevents a loss, draw or mid-chapter victory from being shown
        // as a grand finale by a stale or mismatched UI call.
        static bool IsCommittedChapterVictory(PlayerState state,
            EldoriaV41Message report,string receiptId,string appearanceId)
        {
            const string prefix="vigilant:chapter:";
            if(state?.Retention==null || report?.Battle==null ||
               report.ReportId!=receiptId ||
               !report.Battle.Won ||
               report.Battle.EffectiveOutcome!="victory" ||
               !appearanceId.StartsWith(prefix,StringComparison.Ordinal))
                return false;
            if(!int.TryParse(appearanceId.Substring(prefix.Length),out int stage) ||
               stage<1 || stage>10000 ||
               report.Battle.TargetId!="vigilant:"+stage ||
               state.Retention.LastClearedStage<stage)
                return false;
            return EarlyRetentionRules.Stage(stage).Step==
                   EarlyRetentionRules.StagesPerChapter;
        }

        // A first discovery is not simply an exploration-themed popup.
        // Anchor it to the exact claimed order, family and command receipt
        // already saved by LocalGateway; no additional reward is paid.
        static bool IsCommittedFirstDiscovery(PlayerState state,
            EldoriaV41Message report,string receiptId,string appearanceId)
        {
            const string prefix="scout:first:";
            const string reportPrefix="exploration:";
            if(state?.Retention?.Orders==null || report==null ||
               !appearanceId.StartsWith(prefix,StringComparison.Ordinal) ||
               !receiptId.StartsWith(reportPrefix,StringComparison.Ordinal) ||
               report.DiscoveryKind<0 || report.DiscoveryKind>3 ||
               report.RewardReceiptId!=receiptId.Substring(reportPrefix.Length))
                return false;
            string orderId=appearanceId.Substring(prefix.Length);
            if(string.IsNullOrWhiteSpace(orderId) ||
               report.DiscoveryOrderId!=orderId ||
               (state.Retention.ScoutDiscoveryMask&
                    (1<<report.DiscoveryKind))==0)
                return false;
            var order=state.Retention.Orders.Find(o=>
                o!=null && o.Id==orderId);
            // A real claimed, old order can be pruned on the same command
            // that creates the immutable mail. Its receipt, family and saved
            // discovery mask remain authoritative, even without an Order row.
            // If an order still exists, it MUST be confirmed as claimed.
            return order==null ||
                   (order.Claimed && order.Kind==report.DiscoveryKind);
        }

        // The UI may only queue a chapter victory or first family discovery
        // when the SAME gateway command has already written its real mail.
        // This retains the ceremony during a room/tutorial, without issuing
        // another reward command or inventing a persisted presentation state.
        public bool AddConfirmedReport(PlayerState state,string receiptId,
            string receiptType,string appearanceId,string title,string detail,
            bool legendary)
        {
            bool valid=(receiptType=="battle" &&
                    receiptId!=null &&
                    receiptId.StartsWith("vigilant:",StringComparison.Ordinal) &&
                    appearanceId!=null &&
                    appearanceId.StartsWith("vigilant:chapter:",StringComparison.Ordinal)) ||
                (receiptType=="exploration" &&
                    receiptId!=null &&
                    receiptId.StartsWith("exploration:",StringComparison.Ordinal) &&
                    appearanceId!=null &&
                    appearanceId.StartsWith("scout:first:",StringComparison.Ordinal));
            if(!valid || string.IsNullOrWhiteSpace(title) ||
               seen.Contains(receiptId))return false;
            var committed=FindCommittedNotice(state,receiptId,receiptType);
            if(committed==null || committed.Category!="informes" ||
               (receiptType=="battle" &&
                   !IsCommittedChapterVictory(state,committed,receiptId,appearanceId)) ||
               (receiptType=="exploration" &&
                   !IsCommittedFirstDiscovery(state,committed,receiptId,appearanceId)))
                return false;
            pending.Enqueue(new DailyTreasureMilestoneNotice(
                appearanceId,title,detail??"",legendary));
            seen.Add(receiptId);
            return true;
        }

        public void Observe(PlayerState now)
        {
            if(now==null)return;
            if(previous!=null)
            {
                if(DailyTreasureMilestoneRules.NewlyCompletedBastion(
                    previous,now,out int bastion) &&
                    (bastion==2 || bastion==5))
                    AddIfCommitted(now,"milestone:bastion:"+bastion,"milestone-bastion");

                if(DailyTreasureMilestoneRules.NewlyRecruitedLyra(previous,now))
                    AddIfCommitted(now,"milestone:hero:lyra","hero-unlocked");
            }
            previous=now;
        }

        // Peek before showing the presentation, then acknowledge only
        // if DailyTreasurePanel.PresentCommittedMilestone returned true.
        public bool TryPeek(out DailyTreasureMilestoneNotice notice)
        {
            if(pending.Count==0)
            {
                notice=default(DailyTreasureMilestoneNotice);
                return false;
            }
            notice=pending.Peek();
            return true;
        }

        public bool ConfirmPresented(string eventId)
        {
            if(pending.Count==0||pending.Peek().EventId!=eventId)return false;
            pending.Dequeue();
            return true;
        }
    }
}
