using System;
using System.Linq;
using Eldoria.Domain;
using Eldoria.Application;

static class Program
{
    sealed class Clock:IClock{
        public long UtcTicks {get;set;}=new DateTime(2026,10,28,0,0,0,DateTimeKind.Utc).Ticks;
        public void Add(int seconds)=>UtcTicks+=TimeSpan.FromSeconds(seconds).Ticks;
    }
    sealed class Store:IStateStore{
        public PlayerState State;
        public PlayerState Load()=>State;
        public void Save(PlayerState state)=>State=state;
    }
    static void Require(bool ok,string message)
    { if(!ok)throw new InvalidOperationException("RETENTION_SMOKE_FAIL "+message); }
    static GameCommand Command(LocalGateway g,string id,string kind,string target)
    { var s=g.Snapshot(); return new GameCommand(id,s.PlayerId,kind,target,s.Revision); }
    static void Main()
    {
        var bare=new PlayerState();
        Require(!EarlyRetentionRules.VigilantsUnlocked(bare),"No premature B1 shortcut");
        Require(!EarlyRetentionRules.ScoutsUnlocked(bare),"No premature B2 shortcut");
        bare.SawmillLevel=1;
        Require(EarlyRetentionRules.VigilantsUnlocked(bare),"B1 trial opens after real sawmill");
        Require(!EarlyRetentionRules.StageUnlocked(bare,2),"B1 trial does not grant chapter progression");
        for(int i=1;i<=100;i++){
            var stage=EarlyRetentionRules.Stage(i);
            Require(stage.Chapter==(i-1)/25+1&&stage.Step==(i-1)%25+1,"25-stage chapter math");
            if(i%5==0)Require(stage.Name=="Guardián del umbral","Guard every five");
        }
        var clock=new Clock();
        var state=new PlayerState{
            BastionLevel=2,SawmillLevel=1,BarracksLevel=1,
            Available=new ArmyRoster{ArcherT1=60},
            ChapterProgress=new ChapterProgressState{MarchConfirmed=true},
            Resources=new ResourceWallet{Wood=200,Stone=250,Food=50}
        };
        var store=new Store{State=state};
        var g=new LocalGateway(clock,store);
        Require(g.Snapshot().Retention.Orders.Count==3,"Three board orders at new period");
        Require(EarlyRetentionRules.ScoutsUnlocked(g.Snapshot()),"Scout unlock after first March");
        var battle=Command(g,"v1","VigilantBattle","1");
        Require(g.Execute(battle).Ok,"Real battle wins first stage");
        Require(g.Snapshot().Retention.LastClearedStage==1,"Stage advances");
        var loot=g.Snapshot().Resources.Wood;
        Require(g.Execute(battle).Ok&&g.Snapshot().Resources.Wood==loot,"Idempotent battle command");
        Require(!g.Execute(Command(g,"retry","VigilantBattle","1")).Ok,"Cannot repeat same stage");
        var stageCopy=g.Snapshot();stageCopy.Retention.LastClearedStage=900;
        Require(g.Snapshot().Retention.LastClearedStage==1,"Snapshot remains isolated");
        clock.Add(30*3600);g=new LocalGateway(clock,store);
        Require(EarlyRetentionRules.UnclaimedIdleXp(g.Snapshot().Retention,clock.UtcTicks)==96,"Eight-hour XP cap");
        var xp=Command(g,"xp","ClaimVigilantXp","aldric");
        Require(g.Execute(xp).Ok,"Claim offline XP");
        int amount=g.Snapshot().Retention.AldricExperience;
        Require(g.Execute(xp).Ok&&g.Snapshot().Retention.AldricExperience==amount,"Never duplicate hero XP");
        var order=g.Snapshot().Retention.Orders.First(x=>!x.Claimed);
        Require(g.Execute(Command(g,"dispatch","ScoutDispatch",order.Id)).Ok,"Scout dispatched by gateway");
        Require(!g.Execute(Command(g,"early","ScoutClaim",order.Id)).Ok,"No early reward");
        clock.Add(EarlyRetentionRules.ScoutDurationSeconds(order.Kind)+1);
        g=new LocalGateway(clock,store);
        var claim=Command(g,"claim","ScoutClaim",order.Id);
        Require(g.Execute(claim).Ok,"Real offline return claim");
        int after=g.Snapshot().Retention.AldricExperience;
        Require(g.Execute(claim).Ok&&g.Snapshot().Retention.AldricExperience==after,"No double scout claim");
        clock.Add(30*24*3600);g=new LocalGateway(clock,store);
        Require(g.Snapshot().Retention.Orders.Count(x=>!x.Claimed)<=6,"Max six pending orders offline");
        Console.WriteLine("EARLY_RETENTION_DOMAIN_SMOKE=PASS");
    }
}
