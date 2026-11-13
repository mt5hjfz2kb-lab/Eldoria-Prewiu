using System;
using System.Linq;
using Eldoria.Domain;
using Eldoria.Application;
static class GatewayEvolutionChecks
{
    sealed class Clock:IClock { public long UtcTicks{get;set;}=new DateTime(2026,11,5,0,0,0,DateTimeKind.Utc).Ticks; }
    sealed class Store:IStateStore { public PlayerState State;static readonly System.Text.Json.JsonSerializerOptions Options=new System.Text.Json.JsonSerializerOptions{IncludeFields=true};static PlayerState Clone(PlayerState state)=>System.Text.Json.JsonSerializer.Deserialize<PlayerState>(System.Text.Json.JsonSerializer.Serialize(state,Options),Options);public PlayerState Load()=>Clone(State);public void Save(PlayerState state)=>State=Clone(state); }
    static int checks;
    static void Check(bool value,string name){if(!value)throw new Exception("V41_GATEWAY_EVOLUTION_FAIL "+name);checks++;}
    static CommandResult Send(LocalGateway g,string id,string kind,string target){var s=g.Snapshot();return g.Execute(new GameCommand(id,s.PlayerId,kind,target,s.Revision));}
    public static void Run()
    {
        // Controlled test opponent only: not a definition for the production scout.
        var definition=new EldoriaV41BattleInput{RulesVersion=EldoriaV41CombatEvolution.RulesVersion,TargetId="corrupt-scout",EnemyTroops=new ArmyRoster{ArcherT1=1},EnemyHeroes=new[]{"aldric",null,null}};
        var resolver=new EldoriaV41GatewayResolver(new[]{definition});
        definition.EnemyTroops.ArcherT1=10000;
        var store=new Store{State=new PlayerState{BastionLevel=2,BarracksLevel=1,SawmillLevel=1,ChapterProgress=new ChapterProgressState{MarchConfirmed=true},Available=new ArmyRoster{ArcherT1=150}}};
        // Real Vigilant eligibility includes the already completed Engendro
        // and its actual taught narrative beat. Otherwise a busy-hero test
        // could pass merely because the entire destination was still locked.
        store.State.EngendroDefeated=true;
        store.State.ChapterProgress.SeenNarrative.Add("vigilants-introduction");
        var clock=new Clock();var gateway=new LocalGateway(clock,store,resolver);
        Check(EarlyRetentionRules.StageUnlocked(gateway.Snapshot(),1),
            "busy-hero fixture has the genuine introduced guardian unlocked");
        Check(Send(gateway,"v41-prepare","ConfigureMarch","march-main:40:0:0:aldric::").Ok,"prepare real march");
        Check(Send(gateway,"v41-launch","Fight","corrupt-scout").Ok,"launch real command");
        Check(!Send(gateway,"busy-guardian","VigilantBattle","1").Ok,"guardian cannot reuse hero already marching");
        Check(gateway.Snapshot().March.VersionedBattle.EnemyTroops.ArcherT1==1,"catalog copied before caller mutation");
        var copied=gateway.Snapshot();copied.March.VersionedBattle.EnemyTroops.ArcherT1=999;
        Check(gateway.Snapshot().March.VersionedBattle.EnemyTroops.ArcherT1==1,"save snapshot isolated");
        bool rejected=false;try{new LocalGateway(clock,store);}catch(InvalidOperationException){rejected=true;}
        Check(rejected,"missing resolver never falls back midflight");
        clock.UtcTicks+=TimeSpan.FromSeconds(30).Ticks;
        // Empty catalog still resolves the old immutable input after a release change.
        gateway=new LocalGateway(clock,store,new EldoriaV41GatewayResolver(Array.Empty<EldoriaV41BattleInput>()));
        var result=gateway.Snapshot();var mail=result.Mailbox.List("informes").Single();
        Check(mail.Battle.Outcome=="victory" && mail.Battle.PlayerComposition.Contains("40"),"C# resolver used by real gateway");
        Check(mail.Battle.OwnDamageByFamily.Length>0,"advanced own facts persisted");
        Check(new[]{mail.Battle.OwnDamageByFamily,mail.Battle.OwnReceivedByFamily,mail.Battle.OwnRemainingByFamily}
            .All(x=>x.Contains("Arqueros")&&!x.Contains("Paladines")&&!x.Contains("Brujos")),
            "archer-only report does not preview absent future troop families");
        Check(result.Available.ArcherT1==150 && result.March.Phase=="idle","pre-hospital all troops return once");
        Check(result.March.VersionedBattle==null,"completed snapshot released");
        var wood=result.Resources.Wood;
        gateway=new LocalGateway(clock,store,resolver);
        Check(gateway.Snapshot().Mailbox.List("informes").Count==1 && gateway.Snapshot().Resources.Wood==wood,"offline replay cannot duplicate battle rewards");
        Check(Send(gateway,"v41-read","MailRead",mail.EventId).Ok && Send(gateway,"v41-delete","MailDelete",mail.EventId).Ok,"read/delete through authority");
        gateway=new LocalGateway(clock,store,resolver);
        Check(gateway.Snapshot().Mailbox.List("informes").Count==0 && gateway.Snapshot().Resources.Wood==wood,"deleted battle not resurrected or repaid");
        var campaign=EldoriaV41GatewayResolver.Campaign();
        var veteran=new Store{State=new PlayerState{BastionLevel=2,BarracksLevel=5,SawmillLevel=1,
            ChapterProgress=new ChapterProgressState{MarchConfirmed=true},Available=new ArmyRoster{ArcherT1=200}}};
        veteran.State.EngendroDefeated=true;
        veteran.State.ChapterProgress.SeenNarrative.Add("vigilants-introduction");
        var journey=new LocalGateway(clock,veteran,campaign);
        for(int stage=1;stage<=25;stage++)
        {
            Check(Send(journey,"guard-"+stage,"VigilantBattle",stage.ToString()).Ok,"actual guardian command "+stage);
            var after=journey.Snapshot();
            Check(after.Retention.LastClearedStage==stage,"earned guardian progression "+stage);
            var report=after.Mailbox.List("informes").First(m=>m.EventId=="vigilant:guard-"+stage).Battle;
            Check(report.Outcome=="victory" && report.OwnDamageByFamily.Length>0,"evolved guardian report "+stage);
            Check(report.RewardWood==30+stage*2 && after.Available.ArcherT1==200,"reward facts and troop conservation "+stage);
            int paid=after.Resources.Wood;
            journey=new LocalGateway(clock,veteran,campaign);
            Check(Send(journey,"guard-"+stage,"VigilantBattle",stage.ToString()).Ok && journey.Snapshot().Resources.Wood==paid,"idempotent reload "+stage);
        }
        var low=campaign.PrepareCreature(new ArmyRoster{ArcherT1=36},new[]{"aldric",null,null},"vigilant:25",EarlyRetentionRules.Stage(25).Enemy);
        var high=low.Copy();high.OwnMasteryLevel=20;
        Check(campaign.Resolve(high).RemainingHealth>=campaign.Resolve(low).RemainingHealth,"earned mastery retains benefit");
        Check(high.OwnTroops.ArcherT1==36 && low.OwnMasteryLevel==0,"mastery does not invent troops or mutate snapshot");
        // Regression for the exact Guardian 25 failure discovered during the
        // pre-tester audit: Aldric won with 36 T2 and 560 XP, but adding a
        // newly recruited Lyra could reduce mastery and reverse that victory.
        var veteranPairState=new PlayerState {
            BastionLevel=5,BarracksLevel=5,SawmillLevel=1,
            ChapterProgress=new ChapterProgressState{MarchConfirmed=true},
            Available=new ArmyRoster{ArcherT2=36},
            Retention=new EarlyRetentionState{LastClearedStage=24,AldricExperience=560,LyraExperience=0},
            ChapterThree=new BastionThreeState{LyraRecruited=true}
        };
        veteranPairState.ChapterProgress.SeenNarrative.Add("vigilants-introduction");
        var pairStore=new Store{State=veteranPairState};
        var pairGateway=new LocalGateway(clock,pairStore,campaign);
        Check(Send(pairGateway,"pair-prepare","ConfigureMarch",
            "march-main:0:36:0:aldric:lyra:").Ok,"canonical pair march prepared with 36 T2");
        Check(pairGateway.Snapshot().PreparedHeroId=="aldric+lyra",
            "canonical gateway selects both real heroes");
        Check(EarlyRetentionRules.HeroMasteryLevel(pairGateway.Snapshot(),"aldric+lyra")==7,
            "pair retains its best earned mastery, not an average with new Lyra");
        var soloInput=campaign.PrepareCreature(new ArmyRoster{ArcherT2=36},
            new[]{"aldric",null,null},"vigilant:25",EarlyRetentionRules.Stage(25).Enemy);
        soloInput.OwnMasteryLevel=7;
        var soloReport=campaign.Resolve(soloInput);
        Check(soloReport.Won,"Aldric solo Guardian 25 is the verified winning control");
        Check(Send(pairGateway,"pair-vigilant-25","VigilantBattle","25").Ok,
            "pair Guardian 25 is a valid real gateway command");
        var pairAfter=pairGateway.Snapshot();
        Check(pairAfter.Retention.LastClearedStage==25 && pairAfter.LastBattleReport.Won,
            "adding Lyra never reverses earned Guardian 25 victory");
        Check(pairAfter.LastBattleReport.VersionedOutcome?.Outcome=="victory" &&
              pairAfter.LastBattleReport.RemainingHealth>=soloReport.RemainingHealth,
            "versioned combat report retains mastery and a non-regressed result");
        Check(pairAfter.Retention.AldricExperience==595 && pairAfter.Retention.LyraExperience==0,
            "normal victory XP remains credited once to original leading hero");
        int pairWood=pairAfter.Resources.Wood;
        pairGateway=new LocalGateway(clock,pairStore,campaign);
        Check(pairGateway.Snapshot().Retention.LastClearedStage==25 &&
              pairGateway.Snapshot().Resources.Wood==pairWood,
              "paired victory, report and rewards survive real gateway reload");
        Check(Send(pairGateway,"pair-vigilant-25","VigilantBattle","25").Ok &&
              pairGateway.Snapshot().Resources.Wood==pairWood,
              "repeating original battle ID cannot pay rewards twice");
        Check(!Send(pairGateway,"pair-vigilant-25-again","VigilantBattle","25").Ok,
              "new command cannot repeat the completed guardian");

        foreach(var target in new[]{"corrupt-scout","ash-stalker-valoria"})
        {
            // Saved controlled mirror battle exercises both real gateway report routes.
            // This fixture is never added to the production creature catalog.
            var input=new EldoriaV41BattleInput{RulesVersion=resolver.RulesVersion,TargetId=target,
                OwnTroops=new ArmyRoster{ArcherT1=40},EnemyTroops=new ArmyRoster{ArcherT1=40},
                OwnHeroes=new[]{"aldric",null,null},EnemyHeroes=new[]{"aldric",null,null}};
            var saved=new Store{State=new PlayerState{BastionLevel=3,Available=new ArmyRoster{ArcherT1=100},
                March=new MarchState{TargetId=target,HeroId="aldric",Troops=new ArmyRoster{ArcherT1=40},
                    Phase="outbound",PhaseEndsUtcTicks=clock.UtcTicks-TimeSpan.FromSeconds(30).Ticks,VersionedBattle=input}}};
            var game=new LocalGateway(clock,saved,resolver);var after=game.Snapshot();
            var drawn=after.Mailbox.List("informes").Single();
            Check(drawn.Battle.Outcome=="draw" && drawn.Title.Contains("Empate"),"draw title matches outcome "+target);
            Check(after.LastBattleReport.VersionedOutcome?.Outcome=="draw","last report preserves draw "+target);
            Check(drawn.Battle.RewardWood==0 && drawn.Battle.RewardStone==0 && drawn.Battle.RewardFood==0 &&
                !after.ChapterThree.AshStalkerDefeated,"draw never pays victory or clears encounter "+target);
            Check(after.Available.ArcherT1==140 && after.March.Phase=="idle","draw returns all reserved troops "+target);
            game=new LocalGateway(clock,saved,resolver);after=game.Snapshot();
            Check(after.LastBattleReport.VersionedOutcome?.Outcome=="draw" && after.Mailbox.List("informes").Count==1 &&
                after.Available.ArcherT1==140,"draw reload preserves outcome and idempotency "+target);
        }
        Console.WriteLine("V41_VERSIONED_GATEWAY_PASS "+checks);
    }
}
