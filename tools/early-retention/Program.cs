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
            if(i%5==0)Require(stage.Name==EarlyRetentionRules.GuardianName(stage.Step),
                "Every fifth stage has a unique lore-correct tactical guardian");
            if(i==3)Require(stage.Name=="Corrupto errante","No stone golems in Eldoria");
        }
        var bosses=new[]{5,10,15,20,25}.Select(EarlyRetentionRules.Stage).ToArray();
        Require(bosses.Select(b=>b.Name).Distinct().Count()==5,"Five distinctive guardian identities");
        Require(bosses.Select(b=>b.Trait).Distinct().Count()==5,"Five different guardian strengths");
        Require(bosses.All(b=>EarlyRetentionRules.GuardianHint(b.Step).Length>20),
            "Every guardian has a meaningful counter-strategy");
        int attack10=22+10*4+(int)Math.Round(10d*10*.12,MidpointRounding.AwayFromZero)+22;
        int baseDefense5=19+5*3+(int)Math.Round(5d*5*.09,MidpointRounding.AwayFromZero)+18;
        Require(EarlyRetentionRules.Stage(10).Enemy.Attack>attack10,"Fast assault has genuine extra attack");
        Require(EarlyRetentionRules.Stage(5).Enemy.Defense>baseDefense5,"Armor guardian has genuine defense");
        Require(EarlyRetentionRules.Stage(15).Enemy.Break>12+15*2+15,
            "Spawnling guardian has genuine extra rupture");
        Require(EarlyRetentionRules.Stage(20).Enemy.Health>200+20*27+20*20*.85+190,
            "Endurance guardian has genuine extra health");
        var weakened=EarlyRetentionRules.Stage(5);
        var insight=EarlyRetentionRules.CombatInsight(weakened,new CombatStats(20,20,100,5,150),
            new CombatReport(false,0,6,""));
        Require(insight.Contains("DERROTA")&&insight.Contains("Ruptura")&&insight.Contains("150"),
            "Report references true outcome, commander power and counter");
        var seen=new System.Collections.Generic.HashSet<string>();
        for(int kind=0;kind<4;kind++)
        for(int variant=0;variant<3;variant++){
            var heading=EarlyRetentionRules.ScoutReportHeading(kind,variant);
            Require(seen.Add(heading)&&EarlyRetentionRules.ScoutReportStory(kind,variant).Length>40,
                "Twelve distinct meaningful expedition discoveries");
        }
        // Regression guard for the REAL Bastion II sequence. Scout unlock
        // must share ConfigureMarch's save revision; it cannot invalidate
        // the player's very next legitimate Fight command.
        var firstClock=new Clock();
        var firstState=new PlayerState{
            BastionLevel=2,SawmillLevel=1,BarracksLevel=1,
            Available=new ArmyRoster{ArcherT1=60},
            ChapterProgress=new ChapterProgressState{MarchConfirmed=false},
            Resources=new ResourceWallet{Wood=500,Stone=400,Food=120}
        };
        var firstStore=new Store{State=firstState};
        var firstGateway=new LocalGateway(firstClock,firstStore);
        Require(firstGateway.Snapshot().Retention.Orders.Count==0,
            "No scout cards before first real configured March");
        var firstMarch=Command(firstGateway,"first-march","ConfigureMarch","march-main");
        Require(firstGateway.Execute(firstMarch).Ok,"First march prepares normally");
        Require(firstGateway.Snapshot().Retention.Orders.Count==3,
            "Scout unlock is atomically part of the first march revision");
        var nextFight=Command(firstGateway,"fight-after-first-march","Fight","corrupt-scout");
        var nextResult=firstGateway.Execute(nextFight);
        Require(nextResult.Ok,"First fight cannot fail after unlock: "+nextResult.Message);
        Require(firstGateway.Snapshot().March.Phase=="outbound",
            "The authentic world battle started after Scout Board initialization");

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
