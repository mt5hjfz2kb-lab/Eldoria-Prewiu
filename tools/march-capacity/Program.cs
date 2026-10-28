using System;
using System.Linq;
using Eldoria.Domain;
using Eldoria.Application;

static class Program
{
    sealed class Clock:IClock {public long UtcTicks {get;set;}=new DateTime(2026,10,28,0,0,0,DateTimeKind.Utc).Ticks;
        public void Add(int seconds)=>UtcTicks+=TimeSpan.FromSeconds(seconds).Ticks;}
    sealed class Store:IStateStore {public PlayerState State;public PlayerState Load()=>State;public void Save(PlayerState state)=>State=state;}
    static void Assert(bool condition,string name){if(!condition)throw new Exception("MARCH_CAPACITY_FAIL: "+name);}
    static CommandResult Run(LocalGateway g,string id,string kind,string target)
    {var s=g.Snapshot();return g.Execute(new GameCommand(id,s.PlayerId,kind,target,s.Revision));}
    static void Main()
    {
        var state=new PlayerState{Available=new ArmyRoster{ArcherT1=150}};
        Assert(MarchCapacityRules.MaxConcurrentMarches==3,"Three march slots");
        Assert(MarchCapacityRules.Capacity(state,"aldric")==40,"Base forty troops");
        var clock=new Clock();var store=new Store{State=state};var g=new LocalGateway(clock,store);
        Assert(Run(g,"gather1","Gather","forest-valoria").Ok,"First march starts");
        Assert(g.Snapshot().March.Troops.Total==40,"First march has real cap");
        Assert(Run(g,"gather2","Gather","quarry-valoria").Ok,"Second march starts");
        Assert(Run(g,"fight3","Fight","corrupt-scout").Ok,"Third march starts");
        var snap=g.Snapshot();
        Assert(MarchCapacityRules.ActiveCount(snap,clock.UtcTicks)==3,"Three active count");
        Assert(snap.ExtraMarches.Count==2&&snap.ExtraMarches.All(m=>m.Phase=="outbound"),"Two persisted additional marches");
        Assert(snap.Available.Total==30,"Reserved three distinct forty-troop groups");
        Assert(!Run(g,"gather4","Gather","forest-valoria").Ok,"Fourth march prohibited with spare troops");
        snap.ExtraMarches[0].Troops.ArcherT1=0;
        Assert(g.Snapshot().ExtraMarches[0].Troops.ArcherT1==40,"Snapshot deep copy");
        clock.Add(20);
        g=new LocalGateway(clock,store);
        snap=g.Snapshot();
        Assert(MarchCapacityRules.ActiveCount(snap,clock.UtcTicks)==0,"All marches return independently offline");
        Assert(snap.Available.Total==150,"Every reserved troop returns once");
        int wood=snap.Resources.Wood,stone=snap.Resources.Stone;
        g.Advance();g=new LocalGateway(clock,store);
        Assert(g.Snapshot().Available.Total==150&&g.Snapshot().Resources.Wood==wood&&g.Snapshot().Resources.Stone==stone,"No double return or loot");
        state=g.Snapshot();
        state.BarracksLevel=2;
        state.Retention.AldricExperience=800;
        Assert(MarchCapacityRules.HeroLevel(state,"aldric")==11,"Hero levels driven by persistent experience");
        Assert(MarchCapacityRules.Capacity(state,"aldric")==130,"Cuartel plus hero progression capacity");
        Assert(MarchCapacityRules.Capacity(state,"lyra")==80,"Hero level changes capacity individually");
        state.Retention.AldricExperience=1000000;
        Assert(MarchCapacityRules.HeroLevel(state,"aldric")==20,"Hero level capped safely");

        var barracksState=new PlayerState{BastionLevel=2,BarracksLevel=1,
            Available=new ArmyRoster{ArcherT1=100}};
        var barracksStore=new Store{State=barracksState};
        var b=new LocalGateway(clock,barracksStore);
        Assert(!Run(b,"overflow","ConfigureMarch","march-main:61:aldric").Ok,"Overcapacity composition rejected");
        Assert(Run(b,"valid","ConfigureMarch","march-main:60:aldric").Ok,"Valid composition accepted");
        Assert(b.Snapshot().PreparedTroops.Total==60,"Prepared selection retained");

        var scoutState=new PlayerState{BastionLevel=2,BarracksLevel=1,
            Available=new ArmyRoster{ArcherT1=90},
            ChapterProgress=new ChapterProgressState{MarchConfirmed=true}};
        var scoutStore=new Store{State=scoutState};var scoutClock=new Clock();
        var scouts=new LocalGateway(scoutClock,scoutStore);
        var orders=scouts.Snapshot().Retention.Orders.Take(3).Select(x=>x.Id).ToArray();
        Assert(orders.Length==3,"Three real scout orders");
        for(int i=0;i<3;i++)Assert(Run(scouts,"send"+i,"ScoutDispatch",orders[i]).Ok,"Scout dispatch "+i);
        Assert(MarchCapacityRules.ActiveCount(scouts.Snapshot(),scoutClock.UtcTicks)==3,"Scouts also consume three global slots");
        Assert(scouts.Snapshot().Available.Total==54,"Scout troop reservations subtract once");
        Assert(!Run(scouts,"fourth","Gather","forest-valoria").Ok,"No fourth simultaneous world march");
        scoutClock.Add(1000);scouts=new LocalGateway(scoutClock,scoutStore);
        Assert(scouts.Snapshot().Available.Total==90,"Scout troops returned at deadline");
        Assert(MarchCapacityRules.ActiveCount(scouts.Snapshot(),scoutClock.UtcTicks)==0,"All scout slots free before claim");
        for(int i=0;i<3;i++)Assert(Run(scouts,"claim"+i,"ScoutClaim",orders[i]).Ok,"Each scout reward collected exactly once");
        scouts=new LocalGateway(scoutClock,scoutStore);
        Assert(scouts.Snapshot().Available.Total==90,"Claims do not return troops twice");
        var mixedClock=new Clock();
        var mixedStore=new Store{State=new PlayerState{
            BastionLevel=3,BarracksLevel=1,SawmillLevel=1,
            Available=new ArmyRoster{ArcherT1=190}
        }};
        var mixed=new LocalGateway(mixedClock,mixedStore);
        Assert(Run(mixed,"investigate-one","InvestigateFissure","fissure-valoria").Ok,
            "Real Breach investigation occupies exactly one slot");
        Assert(MarchCapacityRules.ActiveCount(mixed.Snapshot(),mixedClock.UtcTicks)==1,
            "Investigation counted without inventing a second world army");
        Assert(Run(mixed,"investigate-forest","Gather","forest-valoria").Ok,
            "First free slot remains usable during investigation");
        Assert(Run(mixed,"investigate-quarry","Gather","quarry-valoria").Ok,
            "Second free slot remains usable during investigation");
        Assert(MarchCapacityRules.ActiveCount(mixed.Snapshot(),mixedClock.UtcTicks)==3,
            "Investigation and two world marches fill exactly three slots");
        Assert(!Run(mixed,"investigate-fourth","Fight","corrupt-scout").Ok,
            "A fourth activity is denied while the Breach investigation is active");
        mixedClock.Add(25);mixed=new LocalGateway(mixedClock,mixedStore);
        Assert(MarchCapacityRules.ActiveCount(mixed.Snapshot(),mixedClock.UtcTicks)==1 &&
               mixed.Snapshot().Available.Total==190,
            "Only the Breach investigation remains after both world marches return");
        Console.WriteLine("GLOBAL_MARCH_CAPACITY=PASS; CONCURRENT_LIMIT=3; SAVE_COMPAT=PASS; SCOUT_SLOTS=PASS; INVESTIGATION_MIX=PASS");
    }
}
