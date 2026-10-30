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
            if(i%5==0)Require(stage.Name==((stage.Step==15)?
                "Engendro de la Brecha":EarlyRetentionRules.GuardianName(stage.Step)),
                "Every fifth stage has the lore-correct guardian, never a golem");
            if(i==3)Require(stage.Name=="Corrupto errante","No stone golems in Eldoria");
        }
        // Each guardian actually changes a different combat stat, not just the card title.
        var g5=EarlyRetentionRules.Stage(5);
        var g10=EarlyRetentionRules.Stage(10);
        var g15=EarlyRetentionRules.Stage(15);
        var g20=EarlyRetentionRules.Stage(20);
        var g25=EarlyRetentionRules.Stage(25);
        Require(g5.Trait.Contains("DEFENSA")&&
                g5.Enemy.Defense>EarlyRetentionRules.Stage(4).Enemy.Defense+20,
                "Guardian 5 has real armor counterplay");
        Require(g10.Trait.Contains("ATAQUE")&&
                g10.Enemy.Attack>EarlyRetentionRules.Stage(9).Enemy.Attack+20,
                "Guardian 10 has real attack counterplay");
        Require(g15.Name=="Engendro de la Brecha"&&g15.Trait.Contains("RUPTURA")&&
                g15.Enemy.Break>EarlyRetentionRules.Stage(14).Enemy.Break+25,
                "Stage15 is the real lore Spawnling with breach pressure");
        Require(g20.Trait.Contains("RESISTENCIA")&&
                g20.Enemy.Health>EarlyRetentionRules.Stage(19).Enemy.Health+240,
                "Guardian 20 has real endurance");
        Require(g25.Trait.Contains("EQUILIBRIO")&&
                g25.Enemy.Defense>EarlyRetentionRules.Stage(24).Enemy.Defense+10,
                "Guardian 25 tests combined stats");
        var starter=SliceRules.Expedition(new ArmyRoster{ArcherT1=36},"aldric");
        var developed=SliceRules.Expedition(new ArmyRoster{ArcherT1=100},"aldric");
        Require(!SliceRules.FightCreature(starter,g25.Enemy).Won,
            "Starter army must NOT trivialize the final guardian");
        Require(SliceRules.FightCreature(developed,g25.Enemy).Won,
            "Improving the army still overcomes the strengthened chapter guardian");
        var evidence=new BattleReportState{TargetId="vigilant:15",
            HeroId="lyra",Troops=55,Won=false,Rounds=5,PlayerPower=2200,
            EnemyPower=g15.Difficulty,RemainingHealth=0};
        var review=EarlyRetentionRules.BattleReview(evidence,g15);
        Require(review.Contains("5 rondas")&&review.Contains("55 tropas")&&
                review.Contains("Lyra")&&review.Contains("ruptura"),
                "Battle report uses authoritative stats and useful advice");
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
        var discovery=EarlyRetentionRules.ScoutDiscoveryText(order);
        Require(discovery.Length>20,"Real lore report is available after scouting");
        Require(g.Snapshot().Retention.LatestDiscoveryText==discovery,
            "Latest discovery persisted in canonical state");
        Require(g.Snapshot().Retention.TotalScoutReports==1,
            "Claim grants one narrative wax seal only");
        Require(g.Snapshot().Retention.ScoutDiscoveryMask==(1<<order.Kind),
            "Only the genuinely claimed expedition family earns a persistent seal");
        int after=g.Snapshot().Retention.AldricExperience;
        Require(g.Execute(claim).Ok&&g.Snapshot().Retention.AldricExperience==after,"No double scout claim");
        g=new LocalGateway(clock,store);
        Require(g.Snapshot().Retention.LatestDiscoveryText==discovery &&
                g.Snapshot().Retention.TotalScoutReports==1,
                "Reopening the game never re-rolls narrative or duplicates seals");
        clock.Add(30*24*3600);g=new LocalGateway(clock,store);
        Require(g.Snapshot().Retention.Orders.Count(x=>!x.Claimed)<=6,"Max six pending orders offline");
        Require(g.Snapshot().Retention.TotalScoutReports==1&&
                g.Snapshot().Retention.ScoutDiscoveryMask==(1<<order.Kind),
                "Count and family discoveries survive old-order expiry");
        var legacy=new PlayerState{BastionLevel=2,BarracksLevel=1,SawmillLevel=1,
            ChapterProgress=new ChapterProgressState{MarchConfirmed=true}};
        legacy.Retention.Orders.Add(new ScoutOrder{Id="old-1",Period=0,Kind=1,Claimed=true});
        legacy.Retention.Orders.Add(new ScoutOrder{Id="old-3",Period=0,Kind=3,Claimed=true});
        var legacyStore=new Store{State=legacy};var migrated=new LocalGateway(clock,legacyStore);
        Require(migrated.Snapshot().Retention.TotalScoutReports==2&&
                migrated.Snapshot().Retention.ScoutDiscoveryMask==10,
                "Actual legacy claims migrate before pruning, never inventing discoveries");
        migrated=new LocalGateway(clock,legacyStore);
        Require(migrated.Snapshot().Retention.TotalScoutReports==2&&
                migrated.Snapshot().Retention.ScoutDiscoveryMask==10,
                "Migration itself is idempotent on reload");
        var backlogClock=new Clock();
        var backlogStore=new Store{State=new PlayerState{BastionLevel=2,BarracksLevel=1,SawmillLevel=1,
            ChapterProgress=new ChapterProgressState{MarchConfirmed=true},Available=new ArmyRoster{ArcherT1=180}}};
        var backlog=new LocalGateway(backlogClock,backlogStore);
        backlogClock.Add(12*3600);backlog.Advance();
        Require(backlog.Snapshot().Retention.Orders.Count(x=>!x.Claimed)==6,"Actual two-period backlog reaches six orders");
        backlogClock.Add(12*3600);backlog.Advance();
        var queued=backlog.Snapshot().Retention.Orders.Where(x=>!x.Claimed).Take(3).ToArray();
        for(int i=0;i<queued.Length;i++)Require(backlog.Execute(Command(backlog,"backlog-send-"+i,"ScoutDispatch",queued[i].Id)).Ok,"Backlog dispatch "+i);
        // Exact UI flow: read the snapshot and claim immediately at the UTC
        // deadline, before another simulation tick has run.
        backlogClock.Add(1000);
        for(int i=queued.Length-1;i>=0;i--)
            Require(backlog.Execute(Command(backlog,"backlog-claim-"+i,"ScoutClaim",queued[i].Id)).Ok,
                "Consecutive real claims cannot stale themselves when the live board refills "+i);
        Require(backlog.Snapshot().Retention.TotalScoutReports==3,"All three backlog claims grant exactly one report each");
        var staleRevision=backlog.Snapshot().Revision;
        var nextOrder=backlog.Snapshot().Retention.Orders.First(x=>!x.Claimed&&x.StartedUtcTicks==0);
        Require(backlog.Execute(Command(backlog,"real-next-command","ScoutDispatch",nextOrder.Id)).Ok,"Another real command advances the saved revision");
        Require(!backlog.Execute(new GameCommand("genuinely-stale",backlog.Snapshot().PlayerId,
            "Gather","forest-valoria",staleRevision)).Ok,"A revision superseded by ANOTHER command is still rejected");
        // Owner-requested depth: three consequential troop/time scouting
        // strategies. A fresh game with no suffix keeps the original route.
        Require(EarlyRetentionRules.ScoutTroopsForStrategy(0)==12 &&
                EarlyRetentionRules.ScoutTroopsForStrategy(1)==8 &&
                EarlyRetentionRules.ScoutTroopsForStrategy(2)==20,
                "Strategy must reserve actual distinct detachment sizes");
        var strategyClock=new Clock();
        var strategyStore=new Store{State=new PlayerState{
            BastionLevel=2,BarracksLevel=1,SawmillLevel=1,
            ChapterProgress=new ChapterProgressState{MarchConfirmed=true},
            Available=new ArmyRoster{ArcherT1=80}}};
        var strategyGateway=new LocalGateway(strategyClock,strategyStore);
        var scoutTarget=strategyGateway.Snapshot().Retention.Orders
            .First(o=>o.Kind!=0).Id;
        Require(!strategyGateway.Execute(Command(strategyGateway,"strategy-bad",
            "ScoutDispatch",scoutTarget+"|5")).Ok,
            "Cannot forge unknown expedition strategies");
        Require(strategyGateway.Execute(Command(strategyGateway,"strategy-stealth",
            "ScoutDispatch",scoutTarget+"|1")).Ok,
            "Discreet expedition dispatches using real troops");
        var activeOrder=strategyGateway.Snapshot().Retention.Orders
            .First(o=>o.Id==scoutTarget);
        Require(activeOrder.StrategyMode==1 && activeOrder.Troops.Total==8 &&
                activeOrder.EndsUtcTicks-activeOrder.StartedUtcTicks==
                TimeSpan.FromSeconds(EarlyRetentionRules.ScoutDurationSeconds(
                    activeOrder.Kind,1)).Ticks,
                "Discreet expedition really spends fewer troops and more UTC time");
        Require(EarlyRetentionRules.ScoutDurationSeconds(activeOrder.Kind,1)>
                EarlyRetentionRules.ScoutDurationSeconds(activeOrder.Kind,0),
                "Discreet route must not be faster");
        strategyGateway=new LocalGateway(strategyClock,strategyStore);
        Require(strategyGateway.Snapshot().Retention.Orders.First(o=>o.Id==scoutTarget)
                .StrategyMode==1,"Chosen route survives real save and reload");
        strategyClock.Add(EarlyRetentionRules.ScoutDurationSeconds(activeOrder.Kind,1));
        strategyGateway=new LocalGateway(strategyClock,strategyStore);
        Require(strategyGateway.Execute(Command(strategyGateway,"strategy-claim",
            "ScoutClaim",scoutTarget)).Ok,"Discreet route can finish and pay");
        int scoutReports=strategyGateway.Snapshot().Retention.TotalScoutReports;
        Require(!strategyGateway.Execute(Command(strategyGateway,"strategy-repeat",
            "ScoutClaim",scoutTarget)).Ok &&
            strategyGateway.Snapshot().Retention.TotalScoutReports==scoutReports,
            "A second claim never duplicates scouting rewards");
        var fastClock=new Clock();
        var fastStore=new Store{State=new PlayerState{
            BastionLevel=2,BarracksLevel=1,SawmillLevel=1,
            ChapterProgress=new ChapterProgressState{MarchConfirmed=true},
            Available=new ArmyRoster{ArcherT1=80}}};
        var fastGateway=new LocalGateway(fastClock,fastStore);
        var fastTarget=fastGateway.Snapshot().Retention.Orders.First(o=>o.Kind!=0).Id;
        Require(fastGateway.Execute(Command(fastGateway,"strategy-fast","ScoutDispatch",
            fastTarget+"|2")).Ok,"Rapid expedition sends 20 actual troops");
        var fastOrder=fastGateway.Snapshot().Retention.Orders.First(o=>o.Id==fastTarget);
        Require(fastOrder.StrategyMode==2 && fastOrder.Troops.Total==20 &&
            EarlyRetentionRules.ScoutDurationSeconds(fastOrder.Kind,2)<
            EarlyRetentionRules.ScoutDurationSeconds(fastOrder.Kind,0),
            "Rapid route spends more troops and returns sooner");
        Console.WriteLine("SCOUT_STRATEGY_CONTRACT=PASS");

        Console.WriteLine("EARLY_RETENTION_DOMAIN_SMOKE=PASS");
    }
}
