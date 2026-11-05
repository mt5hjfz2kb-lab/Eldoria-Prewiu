using System;
using System.Text.Json;
using Eldoria.Application;
using Eldoria.Domain;

internal static class EconomyV2IntegratedProbe
{
    sealed class Clock : IClock
    {
        public long UtcTicks { get; set; } = new DateTime(2026,11,5,12,0,0,DateTimeKind.Utc).Ticks;
        public void Add(int seconds) => UtcTicks += TimeSpan.FromSeconds(seconds).Ticks;
    }
    sealed class Store : IStateStore
    {
        public PlayerState Seed;
        private string save;
        static readonly JsonSerializerOptions Opt=new() {IncludeFields=true};
        public PlayerState Load() => save==null?Seed:JsonSerializer.Deserialize<PlayerState>(save,Opt);
        public void Save(PlayerState state) => save=JsonSerializer.Serialize(state,Opt);
    }
    static void Check(bool value,string why)
    {
        if(!value)throw new Exception("ECONOMY_V2_FAIL: "+why);
        Console.WriteLine("PASS "+why);
    }
    static PlayerState Ready()=>new()
    {
        BastionLevel=3,SawmillLevel=3,BarracksLevel=2,JourneyComplete=true,EngendroDefeated=true,
        Resources=new(){Wood=50000,Stone=50000,Food=50000},
        ChapterThree=new(){GranaryLevel=2,StoneworksLevel=1}
    };
    static CommandResult Do(LocalGateway g,string kind,string target)
    {
        var s=g.Snapshot();
        return g.Execute(new GameCommand(Guid.NewGuid().ToString(),s.PlayerId,kind,target,s.Revision));
    }
    static PlayerState ReadyToAscend()=>new()
    {
        BastionLevel=2,SawmillLevel=1,BarracksLevel=1,JourneyComplete=true,
        Available=new(){ArcherT1=56},Resources=new(){Wood=50000,Stone=50000,Food=50000}
    };
    static void LongOfflineProduction()
    {
        var clock=new Clock();var store=new Store{Seed=Ready()};var game=new LocalGateway(clock,store);
        var wallet=game.Snapshot().Resources;
        Check(game.Snapshot().ChapterThree.EconomyVersion==2,"fresh chapter III initializes V2 ledger");
        clock.Add(8*3600);game=new LocalGateway(clock,store);var after=game.Snapshot();
        Check(after.Resources.Wood-wallet.Wood==10368,"8h wood at level3");
        Check(after.Resources.Stone-wallet.Stone==2304,"8h stone at level1");
        Check(after.Resources.Food-wallet.Food==5472,"8h food at level2");
        game=new LocalGateway(clock,store);var reloaded=game.Snapshot();
        Check(reloaded.Resources.Wood==after.Resources.Wood&&reloaded.Resources.Stone==after.Resources.Stone&&reloaded.Resources.Food==after.Resources.Food,"8h reload has no duplicate production");
    }
    static void FractionAndRollback()
    {
        var clock=new Clock();var seed=Ready();seed.SawmillLevel=1;
        var store=new Store{Seed=seed};var game=new LocalGateway(clock,store);
        var wood=game.Snapshot().Resources.Wood;clock.Add(1);game.Advance();
        Check(game.Snapshot().Resources.Wood==wood&&game.Snapshot().ChapterThree.WoodProductionMicro==120000,"fraction after first second");
        game=new LocalGateway(clock,store);clock.Add(9);game.Advance();
        Check(game.Snapshot().Resources.Wood==wood+1&&game.Snapshot().ChapterThree.WoodProductionMicro==200000,"fraction survives save and reload");
        var before=game.Snapshot();clock.Add(-20);game.Advance();
        var after=game.Snapshot();
        Check(after.Resources.Wood==before.Resources.Wood&&after.ChapterThree.WoodProductionMicro==before.ChapterThree.WoodProductionMicro&&after.ChapterThree.EconomyUtcTicks==before.ChapterThree.EconomyUtcTicks,"clock rollback never pays or rewinds");
        clock.Add(20);game=new LocalGateway(clock,store);
        Check(game.Snapshot().Resources.Wood==wood+1,"clock recovering cannot repay elapsed time");
    }
    static void OfflineUpgrade()
    {
        var clock=new Clock();var seed=Ready();seed.ChapterThree.GranaryLevel=1;
        var store=new Store{Seed=seed};var game=new LocalGateway(clock,store);
        Check(Do(game,"UpgradeBuilding","granary").Ok,"start real granary upgrade");
        int before=game.Snapshot().Resources.Food;
        clock.Add(7200);game=new LocalGateway(clock,store);
        Check(game.Snapshot().ChapterThree.GranaryLevel==2,"granary upgrade completed offline");
        Check(game.Snapshot().Resources.Food-before==1367,"2h offline credit split at actual level-change deadline");
        game=new LocalGateway(clock,store);Check(game.Snapshot().Resources.Food-before==1367,"reload does not duplicate split");
    }
    static void LegacyMigration()
    {
        var clock=new Clock();var old=Ready();old.SawmillLevel=1;old.ChapterThree.GranaryLevel=0;old.ChapterThree.StoneworksLevel=0;
        old.ChapterThree.EconomyUtcTicks=clock.UtcTicks-TimeSpan.FromHours(2).Ticks;
        old.ChapterThree.EconomyVersion=0;
        var modern=Ready();modern.SawmillLevel=1;modern.ChapterThree.GranaryLevel=0;modern.ChapterThree.StoneworksLevel=0;
        modern.ChapterThree.EconomyUtcTicks=clock.UtcTicks;modern.ChapterThree.EconomyVersion=2;
        var oldStore=new Store{Seed=old};var currentStore=new Store{Seed=modern};
        var migrated=new LocalGateway(clock,oldStore);var control=new LocalGateway(clock,currentStore);
        var before=migrated.Snapshot();
        Check(before.Resources.Wood-control.Snapshot().Resources.Wood==3600,"legacy pending production capped once at original one hour");
        Check(before.ChapterThree.EconomyVersion==2,"legacy save migrated without resetting wallet");
        migrated=new LocalGateway(clock,oldStore);Check(migrated.Snapshot().Resources.Wood==before.Resources.Wood,"legacy migration is idempotent");
        clock.Add(10);migrated.Advance();
        Check(migrated.Snapshot().Resources.Wood==before.Resources.Wood+1&&migrated.Snapshot().ChapterThree.WoodProductionMicro==200000,"V2 fractional production continues after migration");
    }
    static void AscensionWhileOffline()
    {
        var clock=new Clock();var store=new Store{Seed=ReadyToAscend()};
        var game=new LocalGateway(clock,store);
        Check(Do(game,"AdvanceBastion","bastion").Ok,"ascend from canonical Bastion II");
        clock.Add(240);game=new LocalGateway(clock,store);
        int woodAtAscension=game.Snapshot().Resources.Wood;
        var awayClock=new Clock();var awayStore=new Store{Seed=ReadyToAscend()};
        var away=new LocalGateway(awayClock,awayStore);
        Check(Do(away,"AdvanceBastion","bastion").Ok,"start offline ascension");
        awayClock.Add(240+3600);
        away=new LocalGateway(awayClock,awayStore);var paid=away.Snapshot();
        Check(paid.BastionLevel==3,"Bastion III unlocks at exact historical deadline");
        Check(paid.Resources.Wood==woodAtAscension+432,"first offline load credits production after unlock, not before");
        Check(paid.ChapterThree.EconomyUtcTicks==awayClock.UtcTicks,"entire post-unlock period settled");
        away=new LocalGateway(awayClock,awayStore);
        Check(away.Snapshot().Resources.Wood==paid.Resources.Wood,"offline ascension reload cannot repay production");
    }
    static void TimingsAndLock()
    {
        foreach(var (tier,seconds) in new[]{(2,120),(3,240),(4,420),(5,600)})
          Check(BastionProgressionCatalog.ForLevel(tier).DurationSeconds==seconds,"Bastion "+tier+" V2 time");
        bool rejects=false;
        try{BastionProgressionCatalog.ForLevel(6);}catch(ArgumentOutOfRangeException){rejects=true;}
        Check(rejects,"planned VI-X times do not unlock new Bastion levels");
    }
    public static void Main()
    {
        SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
        LongOfflineProduction();FractionAndRollback();OfflineUpgrade();LegacyMigration();AscensionWhileOffline();TimingsAndLock();
        Console.WriteLine("ECONOMY_V2_GATEWAY_DOMAIN_PASS");
    }
}
