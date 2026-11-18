using System;
using System.Linq;
using System.Text.Json;
using Eldoria.Domain;
using Eldoria.Application;

internal static class GatewayConstructionSmoke
{
    sealed class Clock : IClock
    {
        public long UtcTicks { get; set; } = new DateTime(2026,11,18,21,0,0,DateTimeKind.Utc).Ticks;
        public void Advance(int seconds) => UtcTicks += TimeSpan.FromSeconds(seconds).Ticks;
    }
    // True serialization round trips for the canonical IStateStore contract.
    // This is NOT a Unity FileStateStore or a physical-device test.
    sealed class Store : IStateStore
    {
        static readonly JsonSerializerOptions O = new JsonSerializerOptions { IncludeFields=true };
        string json;
        public int Saves { get; private set; }
        public Store(PlayerState seed=null) { if(seed!=null)Save(seed); }
        public PlayerState Load() => json==null?null:JsonSerializer.Deserialize<PlayerState>(json,O);
        public void Save(PlayerState state) { json=JsonSerializer.Serialize(state,O);Saves++; }
    }
    static int checks;
    static void Check(bool ok,string what)
    {
        if(!ok)throw new Exception("MODULAR25D_DOMAIN_FAIL "+what);
        checks++;Console.WriteLine("PASS "+what);
    }
    static CommandResult Send(LocalGateway g,string id,string kind="Build",string target="sawmill")
    {
        var s=g.Snapshot();
        return g.Execute(new GameCommand(id,s.PlayerId,kind,target,s.Revision));
    }
    public static void Main()
    {
        SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
        var clock=new Clock();
        var store=new Store();
        var gateway=new LocalGateway(clock,store);
        var before=gateway.Snapshot();
        int wood=before.Resources.Wood,cost=SliceRules.SawmillWoodCost,seconds=SliceRules.SawmillBuildSeconds;
        Check(before.BastionLevel==1&&before.SawmillLevel==0,"real Bastion I state");
        Check(ParcelBuildingStates.For(before,"sawmill")==ParcelBuildingState.AVAILABLE,
            "genuine parcel AVAILABLE");
        Check(wood>=cost&&cost>0&&seconds>0,"single valid OWNER_I_II content profile");
        Check(Send(gateway,"sawmill-event-1").Ok,"canonical Build:sawmill accepted");
        var during=gateway.Snapshot();
        Check(during.Resources.Wood==wood-cost&&during.SawmillLevel==0,
            "one exact canonical wood debit");
        Check(during.BuildingTaskId=="sawmill:sawmill-event-1"&&
            during.BuildingCompletesUtcTicks-clock.UtcTicks==TimeSpan.FromSeconds(seconds).Ticks,
            "canonical UTC deadline and persistent task id");
        Check(ParcelBuildingStates.For(during,"sawmill")==ParcelBuildingState.UNDER_CONSTRUCTION,
            "genuine parcel UNDER_CONSTRUCTION");
        Check(store.Saves>0,"gateway persisted state through IStateStore");
        Check(Send(gateway,"sawmill-event-1").Ok,"same command ID is idempotent");
        Check(gateway.Snapshot().Resources.Wood==wood-cost,"duplicate spends no wood");
        Check(!Send(gateway,"sawmill-event-2").Ok,"overlapping work rejected");
        Check(gateway.Snapshot().Resources.Wood==wood-cost,"rejected work spends no wood");
        clock.Advance(seconds-1);
        gateway=new LocalGateway(clock,store);
        Check(gateway.Snapshot().SawmillLevel==0&&
            ParcelBuildingStates.For(gateway.Snapshot(),"sawmill")==ParcelBuildingState.UNDER_CONSTRUCTION,
            "JSON-reloaded construction remains active until deadline");
        clock.Advance(1);
        gateway=new LocalGateway(clock,store);
        var complete=gateway.Snapshot();
        Check(complete.SawmillLevel==1&&
            ParcelBuildingStates.For(complete,"sawmill")==ParcelBuildingState.BUILT,
            "offline completion builds the actual sawmill");
        Check(complete.BuildingCompletesUtcTicks==0&&string.IsNullOrEmpty(complete.BuildingTaskId)&&
            complete.CompletedTaskIds.Contains("sawmill:sawmill-event-1"),
            "completed task cleared and ledger persisted");
        Check(complete.Resources.Wood==wood-cost,"completion never repays or double-charges");
        gateway.Advance();
        gateway=new LocalGateway(clock,store);
        Check(gateway.Snapshot().CompletedTaskIds.Count(x=>x=="sawmill:sawmill-event-1")==1,
            "reload never duplicates completion");
        Check(!Send(gateway,"sawmill-event-3").Ok,"already-built sawmill cannot be rebuilt");
        Check(gateway.Snapshot().Resources.Wood==wood-cost,"already-built request spends nothing");
        var low=new LocalGateway(clock,new Store(new PlayerState {
            Resources=new ResourceWallet {Wood=cost-1,Stone=150,Food=0}
        }));
        Check(!Send(low,"sawmill-low").Ok,"insufficient wood rejected by real gateway");
        Check(low.Snapshot().SawmillLevel==0&&low.Snapshot().Resources.Wood==cost-1&&
            low.Snapshot().BuildingCompletesUtcTicks==0,"insufficient funds do not mutate construction");
        var invalid=new LocalGateway(clock,new Store());
        Check(!Send(invalid,"unknown-1","Build","unknown").Ok,"unknown building ID rejected");
        SliceContentProfiles.SetRuntimeProfileOverride(null);
        Console.WriteLine("MODULAR25D_REAL_GATEWAY_PASS checks="+checks);
    }
}
