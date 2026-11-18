#if UNITY_EDITOR
using System;
using Eldoria.Application;
using Eldoria.Domain;
using UnityEditor;
using UnityEngine;

namespace Eldoria.Experimentos.ValoriaModular25D.Editor
{
    /// <summary>
    /// Runs a deterministic domain-only regression against the real LocalGateway.
    /// Opt-in, no real save files and no owner-beta state. This is NOT a PlayMode,
    /// touch, performance or artistic test.
    /// </summary>
    public static class Modular25DContractSmoke
    {
        sealed class FakeClock : IClock
        {
            public long UtcTicks { get; set; }
        }
        sealed class MemoryJsonStore : IStateStore
        {
            string saved;
            public PlayerState Load()
            {
                return string.IsNullOrEmpty(saved)?null:JsonUtility.FromJson<PlayerState>(saved);
            }
            public void Save(PlayerState state)
            {
                saved=JsonUtility.ToJson(state);
            }
        }
        static void Require(bool condition,string reason)
        {
            if(!condition)throw new InvalidOperationException("[Modular25D] "+reason);
        }
        static GameCommand Build(PlayerState s,string id)
        {
            return new GameCommand(id,s.PlayerId,"Build","sawmill",s.Revision);
        }

        [MenuItem("Eldoria/Experimentos/Valoria modular 2.5D/Probar economia + guardado canonico")]
        public static void Execute()
        {
            SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
            try
            {
                var clock=new FakeClock { UtcTicks=new DateTime(2026,11,18,12,0,0,DateTimeKind.Utc).Ticks };
                var store=new MemoryJsonStore();
                var gateway=new LocalGateway(clock,store);
                var start=gateway.Snapshot();
                Require(start.BastionLevel==1&&start.SawmillLevel==0,"initial Bastion I state");
                Require(start.Resources.Wood>=SliceRules.SawmillWoodCost,"Owner profile has sufficient start wood");
                var command=Build(start,"modular25d-contract-build-1");
                var accepted=gateway.Execute(command);
                Require(accepted.Ok,"canonical Build:sawmill rejected unexpectedly: "+accepted.Message);
                var during=gateway.Snapshot();
                Require(during.Resources.Wood==start.Resources.Wood-SliceRules.SawmillWoodCost,"wood is not deducted exactly once");
                Require(ParcelBuildingStates.For(during,"sawmill")==ParcelBuildingState.UNDER_CONSTRUCTION,
                    "canonical parcel must be under construction");
                Require(during.BuildingCompletesUtcTicks==clock.UtcTicks+
                    TimeSpan.FromSeconds(SliceRules.SawmillBuildSeconds).Ticks,
                    "deadline must be exact canonical UTC duration");

                var duplicate=gateway.Execute(command);
                Require(duplicate.Ok,"duplicate command receipt must be idempotently handled");
                Require(gateway.Snapshot().Resources.Wood==during.Resources.Wood,
                    "duplicate build paid twice");

                // Reload DURING construction to prove UTC deadline is persisted.
                var reloaded=new LocalGateway(clock,store);
                var pending=reloaded.Snapshot();
                Require(pending.SawmillLevel==0&&pending.BuildingCompletesUtcTicks==during.BuildingCompletesUtcTicks,
                    "construction did not persist through a fresh gateway");
                clock.UtcTicks+=TimeSpan.FromSeconds(SliceRules.SawmillBuildSeconds+1).Ticks;
                reloaded.Advance();
                var finished=reloaded.Snapshot();
                Require(finished.SawmillLevel==1,"offline completion did not award level 1");
                Require(finished.BuildingCompletesUtcTicks==0,"deadline not cleared");
                Require(ParcelBuildingStates.For(finished,"sawmill")==ParcelBuildingState.BUILT,
                    "built state mismatch");
                var loadedFinal=new LocalGateway(clock,store).Snapshot();
                Require(loadedFinal.SawmillLevel==1&&loadedFinal.Resources.Wood==during.Resources.Wood,
                    "finished building/load changed levels or credits");

                var emptyStore=new MemoryJsonStore();
                var insufficient=new PlayerState();
                insufficient.Resources.Wood=SliceRules.SawmillWoodCost-1;
                emptyStore.Save(insufficient);
                var poor=new LocalGateway(clock,emptyStore);
                int woodBefore=poor.Snapshot().Resources.Wood;
                var rejected=poor.Execute(Build(poor.Snapshot(),"modular25d-contract-poor-1"));
                Require(!rejected.Ok,"resource insufficiency should be rejected");
                Require(poor.Snapshot().Resources.Wood==woodBefore &&
                    poor.Snapshot().SawmillLevel==0,"rejected action mutated wealth/levels");
                Debug.Log("[Modular25D] CANONICAL ECONOMY SMOKE PASS: build, single payment, " +
                    "in-flight reload, UTC completion, persisted finished state, insufficient wood.");
            }
            finally
            {
                SliceContentProfiles.SetRuntimeProfileOverride(null);
            }
        }
    }
}
#endif
