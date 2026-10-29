using System;
using System.Collections.Generic;
using System.Text.Json;
using Eldoria.Domain;
using Eldoria.Application;
using Eldoria.UI.RoyalGold;
static class MigrationParityProbe {
 sealed class Clock:IClock {public long UtcTicks{get;set;}=new DateTime(2026,10,29,0,0,0,DateTimeKind.Utc).Ticks;public void Add(int s)=>UtcTicks+=TimeSpan.FromSeconds(s).Ticks;}
 sealed class Store:IStateStore {
  public string Json;public PlayerState Initial;
  static readonly JsonSerializerOptions Options=new(){IncludeFields=true};
  public PlayerState Load()=>Json==null?Initial:JsonSerializer.Deserialize<PlayerState>(Json,Options);
  public void Save(PlayerState s)=>Json=JsonSerializer.Serialize(s,Options);
 }
 static void Check(bool ok,string label){if(!ok)throw new Exception("MIGRATION_PARITY: "+label);Console.WriteLine("PASS "+label);}
 static CommandResult Do(LocalGateway g,string kind,string target,string id=null){var s=g.Snapshot();return g.Execute(new GameCommand(id??Guid.NewGuid().ToString(),s.PlayerId,kind,target,s.Revision));}
 static PlayerState Ready(int level=5)=>new(){BastionLevel=level,SawmillLevel=2,BarracksLevel=4,
  JourneyComplete=true,ScoutDefeated=true,EngendroDefeated=true,Available=new(){ArcherT1=60},
  Resources=new(){Wood=5000,Stone=5000,Food=5000},ChapterThree=new(){GranaryLevel=2,StoneworksLevel=level>=5?1:0,
   FissureClosed=true,Hunts=2,DevelopmentPriority="production"},ChapterProgress=new(){GrantedRewards=new(){
    "c1-sawmill","c1-wood","c1-stone","c1-route","c1-bastion","chapter-1",
    "c2-barracks","c2-train","c2-power","c2-spawnling","c2-bastion","chapter-2",
    "c3-granary","c3-food","c3-hunt","c3-fissure","c3-infra"}}};
 public static void Run(){
  SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
  var clock=new Clock();var store=new Store{Initial=Ready()};var g=new LocalGateway(clock,store);var s=g.Snapshot();
  // Independent totals of the missing canonical web rewards, not copied gateway expressions.
  Check(s.Resources.Wood==6540&&s.Resources.Stone==5900&&s.Resources.Food==6120,"advanced schema1 save backfills exact III-IV-V reward totals");
  Check(s.MissionPower==780&&s.SpeedupFiveMinutes==1,"backfill grants exact780 mission power and one5min charge");
  foreach(string key in new[]{"c3-bastion","chapter-3","c4-sawmill2","c4-granary2","c4-barracks2","c4-priority","c4-bastion","chapter-4","c5-stoneworks","c5-stone","c5-army"})
   Check(s.ChapterProgress.GrantedRewards.FindAll(x=>x==key).Count==1,"unique ledger "+key);
  Check(!s.ChapterProgress.GrantedRewards.Contains("chapter-5")&&!s.ChapterProgress.GrantedRewards.Contains("c5-bastion"),"VI and final chapterV reward remain locked");
  string saved=store.Json;g=new LocalGateway(clock,store);Check(store.Json==saved,"repeat load cannot duplicate reward backfill");
  Check(SliceRules.CurrentObjectiveKey(g.Snapshot())=="b5.rare","levelV optional building upgrades do not replace rare mission");
  var damaged=g.Snapshot();damaged.Available.ArcherT1=1;Check(SliceRules.CurrentObjectiveKey(damaged)=="b5.rare","completed army milestone survives later troop loss");
  var projection=new ExistingSliceBinding(g,clock).Read("ash-stalker-valoria",false);
  Check(projection.Building.Name=="Acechador de Ceniza"&&projection.Building.Action.Enabled,"real binding exposes rare encounter and action");
  foreach(var fixture in new[]{(180,140,700,50,2000,true,193,6),(350,190,1000,110,4200,true,812,3),(50,20,100,1,200,false,0,0)}){
   var r=SliceRules.FightCreature(new CombatStats(fixture.Item1,fixture.Item2,fixture.Item3,fixture.Item4,fixture.Item5),new WorldCreatureStats(920,132,68,54,0,0),true);
   Check(r.Won==fixture.Item6&&r.RemainingHealth==fixture.Item7&&r.Rounds==fixture.Item8,"actual v0220 ambush fixture "+fixture.Item5);
   Check(r.Reason.Contains("Emboscada"),"ambush is explained by persisted report");
  }
  Check(Do(g,"Fight","ash-stalker-valoria","rare").Ok,"rare march departs");clock.Add(20);g=new LocalGateway(clock,store);
  Check(g.Snapshot().ChapterThree.AshStalkerDefeated,"rare victory survives offline return");
  Check(g.Snapshot().ChapterProgress.GrantedRewards.Contains("c5-rare"),"rare mission paid after real victory");
  saved=store.Json;Check(!Do(g,"Fight","ash-stalker-valoria").Ok,"rare cannot pay twice");g=new LocalGateway(clock,store);Check(store.Json==saved,"rare replay/reload does not mutate reward ledger");
  Check(g.Snapshot().LastBattleReport.Reason.Contains("Emboscada"),"actual battle report preserves ambush reason");
  var low=Ready(4);low.BarracksLevel=3;g=new LocalGateway(clock,new Store{Initial=low});s=g.Snapshot();
  Check(!Do(g,"Recruit","archer:t2:5").Ok&&g.Snapshot().Resources.Wood==s.Resources.Wood,"lockedT2 does not spend");
  store=new Store{Initial=Ready(4)};g=new LocalGateway(clock,store);s=g.Snapshot();
  Check(Do(g,"Recruit","archer:t2:5","t2").Ok&&Do(g,"Recruit","archer:t2:5","t2").Ok,"T2 recruitment retry is idempotent");
  Check(g.Snapshot().Resources.Wood==s.Resources.Wood-100&&g.Snapshot().Resources.Stone==s.Resources.Stone-60,"T2 exact web20wood12stone per troop");
  clock.Add(149);g=new LocalGateway(clock,store);Check(g.Snapshot().Available.ArcherT2==0,"T2 respects real150seconds");
  clock.Add(1);g=new LocalGateway(clock,store);Check(g.Snapshot().Available.ArcherT2==5&&g.Snapshot().Available.ArcherT1==60,"T2 completion does not convertT1");
  Check(Do(g,"ConfigureMarch","march-main:0:5:0:aldric").Ok,"existing gateway accepts preparedT2 march");g=new LocalGateway(clock,store);Check(g.Snapshot().PreparedTroops.ArcherT2==5,"T2 march composition persists");
  var old=Ready(4);old.PendingRecruitArchers=3;old.PendingRecruitTier=0;old.RecruitmentTaskId="old-t1";old.RecruitmentCompletesUtcTicks=clock.UtcTicks+TimeSpan.TicksPerSecond;
  store=new Store{Initial=old};g=new LocalGateway(clock,store);clock.Add(1);g=new LocalGateway(clock,store);Check(g.Snapshot().Available.ArcherT1==63&&g.Snapshot().Available.ArcherT2==0,"old in-flight training remainsT1");
  var boost=Ready(4);boost.ChapterThree.GranaryLevel=0;boost.SpeedupFiveMinutes=1;boost.SpeedupOneMinute=1;boost.ChapterProgress.GrantedRewards.Add("intro-speedup1");
  store=new Store{Initial=boost};g=new LocalGateway(clock,store);Check(Do(g,"Recruit","archer:t1:20").Ok&&Do(g,"Build","granary").Ok,"independent build and training queues active");
  var trainingEnd=g.Snapshot().RecruitmentCompletesUtcTicks;
  Check(Do(g,"UseSpeedup","building:5m","boost").Ok&&Do(g,"UseSpeedup","building:5m","boost").Ok,"accelerator duplicate command consumes once");
  Check(g.Snapshot().SpeedupFiveMinutes==0&&g.Snapshot().ChapterThree.GranaryLevel==1&&g.Snapshot().RecruitmentCompletesUtcTicks==trainingEnd,"building acceleration leaves training deadline intact");
  Check(!Do(g,"UseSpeedup","gathering:1m").Ok&&g.Snapshot().SpeedupOneMinute==1,"gathering acceleration rejected without spend");
  Check(Do(g,"UseSpeedup","training:1m").Ok&&g.Snapshot().RecruitmentCompletesUtcTicks==trainingEnd-TimeSpan.FromMinutes(1).Ticks,"training accelerator subtracts exactly60seconds");
  g=new LocalGateway(clock,store);Check(g.Snapshot().SpeedupOneMinute==0&&g.Snapshot().SpeedupFiveMinutes==0,"accelerator inventory persists");
  Console.WriteLine("MIGRATION_PARITY_DOMAIN_PASS");
 }
}
