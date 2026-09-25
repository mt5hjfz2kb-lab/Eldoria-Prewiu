using System;
using Eldoria.Domain;

namespace Eldoria.Application
{
    public interface IClock { long UtcTicks { get; } }
    public interface IRandomSource { int Next(int exclusiveMax); }
    public interface IStateStore { PlayerState Load(); void Save(PlayerState state); }
    public interface IContentCatalog { string DisplayName(string id); }
    public interface ICommandGateway
    {
        PlayerState Snapshot();
        CommandResult Execute(GameCommand command);
        bool Advance();
    }
    public sealed class GameCommand
    {
        public readonly string Id, ActorId, Kind, TargetId;
        public readonly long ExpectedRevision;
        public GameCommand(string id, string actorId, string kind, string targetId, long expectedRevision)
        { Id = id; ActorId = actorId; Kind = kind; TargetId = targetId; ExpectedRevision = expectedRevision; }
    }
    public sealed class CommandResult
    {
        public readonly bool Ok;
        public readonly string Message;
        public readonly long Revision;
        public CommandResult(bool ok, string message, long revision)
        { Ok = ok; Message = message; Revision = revision; }
    }
    public sealed class LocalGateway : ICommandGateway
    {
        private readonly IClock clock;
        private readonly IStateStore store;
        private PlayerState state;
        public LocalGateway(IClock clock, IStateStore store)
        {
            this.clock = clock; this.store = store;
            state = store.Load() ?? new PlayerState();
            if (state.SchemaVersion != 1) throw new InvalidOperationException("Unsupported Eldoria save schema");
            Advance();
        }
        public PlayerState Snapshot()
        {
            var s = state;
            var c = new PlayerState {
                SchemaVersion=s.SchemaVersion, RulesVersion=s.RulesVersion, Revision=s.Revision,
                PlayerId=s.PlayerId, RealmId=s.RealmId, WorldId=s.WorldId,
                Resources=new ResourceWallet { Wood=s.Resources.Wood, Stone=s.Resources.Stone, Food=s.Resources.Food },
                BastionLevel=s.BastionLevel, SawmillLevel=s.SawmillLevel, BarracksLevel=s.BarracksLevel,
                CorruptionDiscovered=s.CorruptionDiscovered, ScoutDefeated=s.ScoutDefeated,
                JourneyComplete=s.JourneyComplete, EngendroDefeated=s.EngendroDefeated,
                Available=s.Available.Copy(), Wounded=s.Wounded.Copy(),
                March=new MarchState { MarchId=s.March.MarchId, OwnerId=s.March.OwnerId,
                    TargetId=s.March.TargetId, HeroId=s.March.HeroId, Troops=s.March.Troops.Copy(),
                    Phase=s.March.Phase, PhaseEndsUtcTicks=s.March.PhaseEndsUtcTicks,
                    PendingWood=s.March.PendingWood, PendingStone=s.March.PendingStone },
                ForestRemaining=s.ForestRemaining, BuildingCompletesUtcTicks=s.BuildingCompletesUtcTicks,
                BuildingTaskId=s.BuildingTaskId, RecruitmentCompletesUtcTicks=s.RecruitmentCompletesUtcTicks,
                RecruitmentTaskId=s.RecruitmentTaskId, PendingRecruitArchers=s.PendingRecruitArchers,
                LastBattleReason=s.LastBattleReason,
                CompletedCommandIds=new System.Collections.Generic.List<string>(s.CompletedCommandIds),
                CompletedTaskIds=new System.Collections.Generic.List<string>(s.CompletedTaskIds)
            };
            return c;
        }
        public CommandResult Execute(GameCommand command)
        {
            Advance();
            if (command == null || string.IsNullOrEmpty(command.Id)) return Fail("Comando sin ID");
            if (state.CompletedCommandIds.Contains(command.Id)) return new CommandResult(true, "Ya ejecutado", state.Revision);
            if (command.ActorId != state.PlayerId) return Fail("Propietario incorrecto");
            if (command.ExpectedRevision != state.Revision) return Fail("Estado desactualizado");
            switch (command.Kind)
            {
                case "Gather":
                    if (command.TargetId != "forest-valoria") return Fail("Nodo desconocido");
                    if (state.ForestRemaining <= 0) return Fail("El bosque está agotado");
                    if (!CanDepart()) return Fail("Marcha no disponible");
                    Depart(command.TargetId); state.CorruptionDiscovered = true; break;
                case "Fight":
                    if (command.TargetId == "corrupt-scout")
                    {
                        if (state.ScoutDefeated) return Fail("La ruta ya está despejada");
                    }
                    else if (command.TargetId == "engendro-valoria")
                    {
                        if (state.BastionLevel < 2 || state.BarracksLevel < 1) return Fail("Refuerza primero el Cuartel");
                        if (state.EngendroDefeated) return Fail("El Engendro ya ha sido derrotado");
                        if (state.Available.Total < 48) return Fail("Necesitas 48 arqueros: recluta refuerzos");
                    }
                    else return Fail("Amenaza desconocida");
                    if (!CanDepart()) return Fail("Marcha no disponible");
                    Depart(command.TargetId); break;
                case "AdvanceBastion":
                    if (command.TargetId != "bastion" || !state.JourneyComplete || state.BastionLevel != 1)
                        return Fail("El Bastión todavía no puede ascender");
                    state.BastionLevel = 2;
                    break;
                case "Build":
                    if (state.BuildingCompletesUtcTicks > 0) return Fail("Ya hay una obra en curso");
                    if (command.TargetId == "sawmill")
                    {
                        if (state.SawmillLevel != 0) return Fail("Obra no disponible");
                        if (state.Resources.Wood < SliceRules.SawmillWoodCost) return Fail("Falta madera: visita el bosque");
                        state.Resources.Wood -= SliceRules.SawmillWoodCost;
                        state.BuildingTaskId = "sawmill:" + command.Id;
                        state.BuildingCompletesUtcTicks = clock.UtcTicks + TimeSpan.FromSeconds(SliceRules.SawmillBuildSeconds).Ticks;
                    }
                    else if (command.TargetId == "barracks")
                    {
                        if (state.BastionLevel < 2 || state.BarracksLevel != 0) return Fail("Cuartel no disponible");
                        if (state.Resources.Wood < SliceRules.BarracksWoodCost || state.Resources.Stone < SliceRules.BarracksStoneCost)
                            return Fail("Faltan recursos para el Cuartel");
                        state.Resources.Wood -= SliceRules.BarracksWoodCost;
                        state.Resources.Stone -= SliceRules.BarracksStoneCost;
                        state.BuildingTaskId = "barracks:" + command.Id;
                        state.BuildingCompletesUtcTicks = clock.UtcTicks + TimeSpan.FromSeconds(SliceRules.BarracksBuildSeconds).Ticks;
                    }
                    else return Fail("Obra no disponible");
                    break;
                case "Recruit":
                    if (command.TargetId != "archer:t1" || state.BastionLevel < 2 || state.BarracksLevel < 1)
                        return Fail("Reclutamiento no disponible");
                    if (state.RecruitmentCompletesUtcTicks > 0) return Fail("Ya hay reclutas entrenando");
                    if (state.Resources.Wood < SliceRules.RecruitWoodCost) return Fail("Falta madera para equipar reclutas");
                    state.Resources.Wood -= SliceRules.RecruitWoodCost;
                    state.PendingRecruitArchers = SliceRules.RecruitArchers;
                    state.RecruitmentTaskId = "recruit:" + command.Id;
                    state.RecruitmentCompletesUtcTicks = clock.UtcTicks + TimeSpan.FromSeconds(SliceRules.RecruitSeconds).Ticks;
                    break;
                default: return Fail("Acción desconocida");
            }
            state.CompletedCommandIds.Add(command.Id);
            Commit();
            return new CommandResult(true, "Acción iniciada", state.Revision);
        }
        private bool CanDepart() => state.March.Phase == "idle" && state.Available.ArcherT1 > 0;
        private void Depart(string targetId)
        {
            var m = state.March;
            m.Troops = state.Available.Copy();
            state.Available.ArcherT1 = state.Available.ArcherT2 = state.Available.ArcherT3 = 0;
            m.TargetId = targetId; m.HeroId = "aldric"; m.Phase = "outbound";
            m.PendingWood = m.PendingStone = 0;
            m.PhaseEndsUtcTicks = clock.UtcTicks + TimeSpan.FromSeconds(SliceRules.TravelSeconds).Ticks;
        }
        public bool Advance()
        {
            bool changed = false;
            if (state.BuildingCompletesUtcTicks > 0 && state.BuildingCompletesUtcTicks <= clock.UtcTicks)
            {
                if (!state.CompletedTaskIds.Contains(state.BuildingTaskId))
                {
                    state.CompletedTaskIds.Add(state.BuildingTaskId);
                    if (state.BuildingTaskId.StartsWith("barracks:")) state.BarracksLevel = 1;
                    else state.SawmillLevel = 1;
                }
                state.BuildingCompletesUtcTicks = 0; state.BuildingTaskId = ""; changed = true;
            }
            if (state.RecruitmentCompletesUtcTicks > 0 && state.RecruitmentCompletesUtcTicks <= clock.UtcTicks)
            {
                if (!state.CompletedTaskIds.Contains(state.RecruitmentTaskId))
                {
                    state.CompletedTaskIds.Add(state.RecruitmentTaskId);
                    state.Available.ArcherT1 += state.PendingRecruitArchers;
                }
                state.RecruitmentCompletesUtcTicks = 0;
                state.RecruitmentTaskId = "";
                state.PendingRecruitArchers = 0;
                changed = true;
            }
            // Drive multiple phases in one call after a long offline interval using the ORIGINAL due time.
            for (int guard=0; guard<3 && state.March.Phase != "idle" &&
                state.March.PhaseEndsUtcTicks <= clock.UtcTicks; guard++)
            {
                var m = state.March;
                long due = m.PhaseEndsUtcTicks;
                if (m.Phase == "outbound")
                {
                    if (m.TargetId == "forest-valoria")
                    { m.Phase = "gathering"; m.PhaseEndsUtcTicks = due + TimeSpan.FromSeconds(SliceRules.GatherSeconds).Ticks; }
                    else
                    {
                        var report = SliceRules.Fight(SliceRules.Expedition(m.Troops, m.HeroId), m.TargetId);
                        state.LastBattleReason = report.Reason;
                        if (report.Won)
                        {
                            if (m.TargetId == "engendro-valoria")
                            {
                                m.PendingWood = 120; m.PendingStone = 100; state.EngendroDefeated = true;
                            }
                            else
                            {
                                m.PendingWood = 80; m.PendingStone = 70; state.ScoutDefeated = true;
                            }
                        }
                        m.Phase = "returning"; m.PhaseEndsUtcTicks = due + TimeSpan.FromSeconds(SliceRules.TravelSeconds).Ticks;
                    }
                }
                else if (m.Phase == "gathering")
                {
                    m.PendingWood = Math.Min(SliceRules.ForestLoad, state.ForestRemaining);
                    state.ForestRemaining -= m.PendingWood;
                    m.Phase = "returning";
                    m.PhaseEndsUtcTicks = due + TimeSpan.FromSeconds(SliceRules.TravelSeconds).Ticks;
                }
                else if (m.Phase == "returning")
                {
                    string id = "return:" + m.MarchId + ":" + due;
                    if (!state.CompletedTaskIds.Contains(id))
                    {
                        state.CompletedTaskIds.Add(id);
                        state.Resources.Wood += m.PendingWood;
                        state.Resources.Stone += m.PendingStone;
                        state.Available.ArcherT1 += m.Troops.ArcherT1;
                        state.Available.ArcherT2 += m.Troops.ArcherT2;
                        state.Available.ArcherT3 += m.Troops.ArcherT3;
                    }
                    m.Troops = new ArmyRoster(); m.PendingWood = m.PendingStone = 0;
                    m.TargetId = ""; m.Phase = "idle"; m.PhaseEndsUtcTicks = 0;
                }
                changed = true;
            }
            if (state.SawmillLevel > 0 && state.CorruptionDiscovered && !state.JourneyComplete && state.March.Phase == "idle")
            { state.JourneyComplete = true; changed = true; }
            if (changed) Commit();
            return changed;
        }
        private void Commit() { state.Revision++; store.Save(state); }
        private CommandResult Fail(string reason) => new CommandResult(false, reason, state.Revision);
    }
}
