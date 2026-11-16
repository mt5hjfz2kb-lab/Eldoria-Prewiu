using System;
using Eldoria.Domain;

namespace Eldoria.Experimentos.Ciudad2D
{
    /// <summary>
    /// Read-only view of the authoritative I-V construction rules in LocalGateway.
    /// This policy never grants an unlock or starts a building. Reconcile it with
    /// LocalGateway when that canonical command contract changes.
    /// </summary>
    public enum City2DPlotPhase { Locked, Available, UnderConstruction, Built }

    public readonly struct City2DPlotView
    {
        public readonly City2DPlotPhase Phase;
        public readonly int BuiltLevel;
        public readonly int RequiredBastion;
        public City2DPlotView(City2DPlotPhase phase, int level, int requires)
        { Phase = phase; BuiltLevel = Math.Max(0, level); RequiredBastion = requires; }
    }

    public static class City2DProgressionPolicy
    {
        public static int RequiredBastion(string id)
        {
            switch (id)
            {
                case "bastion": return 1;
                case "aserradero": return 1;
                case "cuartel": return 2;
                case "granero": return 3;
                case "cantera": return 5;
                // The Hero Hall has no independent persisted construction level
                // or build command. It must not be displayed as an invented building.
                default: return int.MaxValue;
            }
        }

        public static City2DPlotView FromSnapshot(PlayerState state, string id)
        {
            if (state == null) return new City2DPlotView(City2DPlotPhase.Locked, 0, RequiredBastion(id));
            int required = RequiredBastion(id);
            if (required == int.MaxValue)
                return new City2DPlotView(City2DPlotPhase.Locked, 0, required);
            int level = 0;
            var c = state.ChapterThree;
            switch (id)
            {
                case "bastion": level = state.BastionLevel; break;
                case "aserradero": level = state.SawmillLevel; break;
                case "cuartel": level = state.BarracksLevel; break;
                case "granero": level = c != null ? c.GranaryLevel : 0; break;
                case "cantera": level = c != null ? c.StoneworksLevel : 0; break;
            }
            if (level > 0) return new City2DPlotView(City2DPlotPhase.Built, level, required);
            if (state.BastionLevel < required)
                return new City2DPlotView(City2DPlotPhase.Locked, 0, required);
            string task = state.BuildingTaskId ?? string.Empty;
            string prefix = id == "aserradero" ? "sawmill:" :
                id == "cuartel" ? "barracks:" :
                id == "granero" ? "c3:granary:" :
                id == "cantera" ? "c3:stoneworks:" : string.Empty;
            bool building = state.BuildingCompletesUtcTicks > 0 && prefix.Length > 0 &&
                task.StartsWith(prefix, StringComparison.Ordinal);
            return new City2DPlotView(building ? City2DPlotPhase.UnderConstruction :
                City2DPlotPhase.Available, 0, required);
        }

        /// <summary>
        /// Deterministic storyboard for five *separate* snapshots, not a save,
        /// no rewards, no fabricated 3D art. Stage 1 begins with an unbuilt
        /// sawmill; each following stage samples a plausible completed chapter.
        /// </summary>
        public static City2DPlotView DemoStage(int bastion, string id)
        {
            bastion = Math.Max(1, Math.Min(5, bastion));
            int required = RequiredBastion(id);
            if (required == int.MaxValue)
                return new City2DPlotView(City2DPlotPhase.Locked, 0, required);
            if (id == "bastion") return new City2DPlotView(City2DPlotPhase.Built, bastion, 1);
            if (bastion < required) return new City2DPlotView(City2DPlotPhase.Locked, 0, required);
            int built = 0;
            switch (id)
            {
                case "aserradero": built = bastion == 1 ? 0 : bastion < 3 ? 1 : 2; break;
                case "cuartel": built = bastion < 3 ? 0 : bastion < 4 ? 1 : 2; break;
                case "granero": built = bastion < 4 ? 0 : bastion < 5 ? 1 : 2; break;
                case "cantera": built = 0; break;
            }
            return new City2DPlotView(built > 0 ? City2DPlotPhase.Built : City2DPlotPhase.Available,
                built, required);
        }
    }
}
