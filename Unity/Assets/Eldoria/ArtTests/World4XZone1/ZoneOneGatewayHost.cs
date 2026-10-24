using System;
using Eldoria.Application;
using Eldoria.Domain;

namespace Eldoria.World4XPrototype
{
    /// <summary>
    /// Opt-in bridge to the SAME authoritative session supplied by Valoria.
    /// Never constructs a gateway, wallet, clock, state store or a second player save.
    /// Map coordinates and artwork cannot grant rewards or override command validation.
    /// </summary>
    public sealed class ZoneOneGatewayHost
    {
        readonly ICommandGateway gateway;
        readonly Action returnToCity;
        readonly Func<long> utcTicks;
        public event Action<PlayerState> Refreshed;
        public ZoneOneGatewayHost(ICommandGateway gateway, Action returnToCity, Func<long> utcTicks)
        {
            this.gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
            this.returnToCity = returnToCity;
            this.utcTicks = utcTicks ?? throw new ArgumentNullException(nameof(utcTicks));
        }
        public PlayerState Snapshot() => gateway.Snapshot();
        public long UtcTicks => utcTicks();

        // The active scene owner chooses whether to call this or to tick the common session
        // elsewhere. Advance is the EXISTING authoritative timer routine, not map simulation.
        public PlayerState Refresh(bool advanceAuthoritativeTimers = false)
        {
            if (advanceAuthoritativeTimers) gateway.Advance();
            var state = gateway.Snapshot();
            Refreshed?.Invoke(state);
            return state;
        }
        public ZoneOneTargetAction Inspect(string targetId)
        {
            var state = gateway.Snapshot();
            if (state == null) return new ZoneOneTargetAction(targetId, "", "Partida no disponible", false);
            bool idle = state.March != null && state.March.Phase == "idle";
            bool troops = state.Available != null && state.Available.ArcherT1 > 0;
            if(targetId!=null&&targetId.StartsWith("region1:",StringComparison.Ordinal))
            {
                string kind=LocalGateway.ResourceKind(state,targetId);int remaining=LocalGateway.ResourceRemaining(state,targetId);
                if(kind=="wood"||kind=="stone")return new ZoneOneTargetAction(targetId,"Gather",remaining<=0?"Recurso agotado":kind=="wood"?"Recoger madera":"Recoger piedra",idle&&troops&&remaining>0);
            }
            // Existing Inspect -> Execute -> original LocalGateway route handles
            // every renewable encounter; level is legible upon selection.
            var creature=LocalGateway.Creature(state,targetId);
            if(creature!=null)
            {
                var name=creature.CreatureKind=="boar"?"Jabalí":"Engendro";
                return new ZoneOneTargetAction(targetId,"Fight",
                    creature.Alive?"Atacar "+name+" · nivel "+creature.Level:
                        name+" · reapareciendo",
                    idle&&troops&&creature.Alive);
            }
            switch (targetId)
            {
                case "valoria-map-city":
                    return new ZoneOneTargetAction(targetId, "ReturnToCity", "Entrar en Valoria", returnToCity != null);
                case "forest-valoria":
                    return new ZoneOneTargetAction(targetId, "Gather", state.ForestRemaining > 0 ? "Recoger madera" : "Bosque agotado", idle && troops && state.ForestRemaining > 0);
                case "quarry-valoria":
                    return new ZoneOneTargetAction(targetId, "Gather", state.QuarryRemaining > 0 ? "Recoger piedra" : "Cantera agotada", idle && troops && state.QuarryRemaining > 0);
                case "corrupt-scout":
                    return new ZoneOneTargetAction(targetId, "Fight", state.ScoutDefeated ? "Ruta despejada" : "Atacar explorador", idle && troops && !state.ScoutDefeated);
                case "engendro-valoria":
                    return new ZoneOneTargetAction(targetId, "Fight", state.EngendroDefeated ? "Engendro derrotado" : "Atacar Engendro", idle && troops && !state.EngendroDefeated && state.BastionLevel >= 2 && state.BarracksLevel > 0 && state.MarchConfigured);
                case "old-watch-ruin":
                    return new ZoneOneTargetAction(targetId, "", "Atalaya en ruinas", false);
                default:
                    // A future wood/food/camp entity MUST NOT be silently mapped onto a
                    // current canonical resource. It needs a real command contract first.
                    return new ZoneOneTargetAction(targetId, "", "Sin acción integrada", false);
            }
        }
        public CommandResult Execute(string targetId, string requestId)
        {
            if (string.IsNullOrWhiteSpace(requestId)) throw new ArgumentException("A stable command request ID is required", nameof(requestId));
            var action = Inspect(targetId);
            var state = gateway.Snapshot();
            if (state == null) return new CommandResult(false, "Partida no disponible", -1);
            if (action.Kind == "ReturnToCity")
            {
                if (returnToCity == null) return new CommandResult(false, "Regreso no conectado", state.Revision);
                returnToCity();
                return new CommandResult(true, "Regreso a Valoria solicitado", state.Revision);
            }
            if (string.IsNullOrEmpty(action.Kind)) return new CommandResult(false, action.Label, state.Revision);
            // Always let the common gateway decide; Inspect supplies UI hints only.
            // The caller reuses requestId for retries, preserving existing idempotency.
            var result = gateway.Execute(new GameCommand(requestId, state.PlayerId, action.Kind, targetId, state.Revision));
            Refresh();
            return result;
        }
        public static double TravelFraction(long nowUtcTicks, long phaseEndsUtcTicks, double durationSeconds)
        {
            if (double.IsNaN(durationSeconds) || double.IsInfinity(durationSeconds) || durationSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            double remaining = ((double)phaseEndsUtcTicks - nowUtcTicks) / TimeSpan.TicksPerSecond;
            return Math.Max(0, Math.Min(1, 1 - remaining / durationSeconds));
        }
    }
    public readonly struct ZoneOneTargetAction
    {
        public readonly string TargetId, Kind, Label;
        public readonly bool Enabled;
        public ZoneOneTargetAction(string targetId, string kind, string label, bool enabled)
        { TargetId = targetId; Kind = kind; Label = label; Enabled = enabled; }
    }
}
