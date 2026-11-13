using System;
using System.Collections.Generic;
using Eldoria.Application;
using Eldoria.Domain;

namespace Eldoria.Issue186Art
{
    // Real saved-state projection for the future PR173 bag/mail controls.
    // This presenter owns NO Canvas and credits NO wallet; the authorized
    // gateway is the only writer. Attaching it to the canonical HUD remains
    // a separate visual/native gate, never inferred from source tests.
    public sealed class ResourceUiMailRow
    {
        public string EventId = "", Title = "", Summary = "";
        public bool Unread, CanClaim;
        public int LockedPackets;
    }

    public sealed class ResourceUiPacketRow
    {
        public string ReceiptId = "", Kind = "", SpriteKey = "", AmountText = "";
        public int Amount;
        public bool New, CanUse;
    }

    public sealed class ResourceUiSnapshot
    {
        public readonly List<ResourceUiMailRow> Mail = new List<ResourceUiMailRow>();
        public readonly List<ResourceUiPacketRow> Bag = new List<ResourceUiPacketRow>();
        public AttentionLevel MailAttention, BagAttention;
        public bool ClaimTutorialPending, UseTutorialPending;
        public long Revision;
    }

    public sealed class ResourceExperienceUiBridge
    {
        readonly ICommandGateway gateway;
        public ResourceExperienceUiBridge(ICommandGateway gateway)
        {
            this.gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        }

        public ResourceUiSnapshot Refresh()
        {
            var s = gateway.Snapshot();
            var result = new ResourceUiSnapshot { Revision = s.Revision };
            var packets = s.ResourceRewards?.Packets;
            int unread = 0, claimable = 0, unseen = 0;
            foreach (var message in s.Mailbox?.Messages ??
                new List<EldoriaV41Message>())
            {
                if (message == null || string.IsNullOrWhiteSpace(message.EventId))
                    continue;
                int locked = 0;
                if (packets != null)
                    foreach (var packet in packets)
                        if (packet != null && !packet.Collected &&
                            packet.MailEventId == message.EventId) locked++;
                bool pending = locked > 0;
                if (!message.Read) unread++;
                if (pending) claimable++;
                result.Mail.Add(new ResourceUiMailRow {
                    EventId = message.EventId, Title = message.Title ?? "",
                    Summary = message.Subtitle ?? "", Unread = !message.Read,
                    CanClaim = pending, LockedPackets = locked
                });
            }
            if (packets != null)
                foreach (var packet in packets)
                {
                    if (packet == null || !packet.Collected || packet.Used ||
                        !ResourceRewardRules.IsResource(packet.Kind) || packet.Amount <= 0)
                        continue;
                    bool isNew = !packet.SeenInBag;
                    if (isNew) unseen++;
                    int balance = packet.Kind == "wood" ? s.Resources.Wood :
                        packet.Kind == "stone" ? s.Resources.Stone : s.Resources.Food;
                    result.Bag.Add(new ResourceUiPacketRow {
                        ReceiptId = packet.ReceiptId, Kind = packet.Kind,
                        SpriteKey = ResourceRewardRules.SpriteKey(packet.Kind,packet.Amount),
                        AmountText = ResourceRewardRules.AmountLabel(packet.Amount),
                        Amount = packet.Amount, New = isNew,
                        CanUse = balance >= 0 && balance <= int.MaxValue-packet.Amount
                    });
                }
            result.MailAttention = AttentionSignalRules.Mail(unread,claimable);
            result.BagAttention = AttentionSignalRules.Bag(unseen);
            // Only expose a request to show help. PR173's actual
            // GraphicRaycaster must separately confirm the target is visible,
            // frontmost and enabled before drawing a hand/halo.
            result.ClaimTutorialPending = claimable > 0 &&
                GuidedTutorialRules.ShouldPoint(s.ResourceGuidance,
                    ResourceRewardCommands.FirstMailClaimStep,
                    ResourceRewardCommands.ClaimMail,true,true);
            result.UseTutorialPending = result.Bag.Exists(p=>p.CanUse) &&
                GuidedTutorialRules.ShouldPoint(s.ResourceGuidance,
                    ResourceRewardCommands.FirstUseStep,
                    ResourceRewardCommands.UsePacket,true,true);
            return result;
        }

        CommandResult Send(string command, string target)
        {
            // The action is bound to a FRESH authoritative snapshot, never
            // to the potentially stale revision used when rendering a row.
            // No local optimistic counters, receipt edits or automatic retry
            // on a stale revision. The button must show the actual result.
            var s = gateway.Snapshot();
            if (string.IsNullOrWhiteSpace(target))
                return new CommandResult(false,"Destino no disponible",s.Revision);
            return gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),
                s.PlayerId,command,target,s.Revision));
        }
        public CommandResult ReadMail(string eventId) => Send("MailRead",eventId);
        public CommandResult ClaimMail(string eventId) =>
            Send(ResourceRewardCommands.ClaimMail,eventId);
        public CommandResult UsePacket(string receiptId) =>
            Send(ResourceRewardCommands.UsePacket,receiptId);
        public CommandResult MarkBagSeen(string receiptId) =>
            Send(ResourceRewardCommands.MarkBagSeen,receiptId);
        public CommandResult DismissTutorial(string stepId) =>
            Send(ResourceRewardCommands.DismissGuidance,stepId);
    }
}
