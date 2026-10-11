using System;

namespace Eldoria.Domain
{
    public enum ParcelBuildingState { NOT_BUILT, AVAILABLE, UNDER_CONSTRUCTION, BUILT }

    // Derived presentation of the EXISTING authoritative LocalGateway state.
    // No second timer, level, unlock, wallet, save or transition machine exists here.
    public static class ParcelBuildingStates
    {
        public static ParcelBuildingState For(PlayerState state, string buildingId)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            bool sawmill = buildingId == "sawmill";
            if (!sawmill && buildingId != "barracks") throw new ArgumentException("Unknown parcel building");
            int level = sawmill ? state.SawmillLevel : state.BarracksLevel;
            if (level > 0) return ParcelBuildingState.BUILT;
            if (state.BuildingCompletesUtcTicks > 0 &&
                (state.BuildingTaskId ?? "").StartsWith(buildingId + ":", StringComparison.Ordinal))
                return ParcelBuildingState.UNDER_CONSTRUCTION;
            if (state.BastionLevel < (sawmill ? 1 : 2)) return ParcelBuildingState.NOT_BUILT;
            return ParcelBuildingState.AVAILABLE;
        }
    }
}
