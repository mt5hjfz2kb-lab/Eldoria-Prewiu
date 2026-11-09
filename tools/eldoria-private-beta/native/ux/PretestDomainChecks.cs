using System;
using Eldoria.Domain;
using Eldoria.Application;
using Eldoria.UI.RoyalGold;
public static class PretestDomainChecks
{
 sealed class Clock:IClock { public long Now=new DateTime(2026,11,9,0,0,0,DateTimeKind.Utc).Ticks;public long UtcTicks=>Now; }
 sealed class Store:IStateStore {public PlayerState Value;public PlayerState Load()=>Value;public void Save(PlayerState value){Value=value;}}
 static int checks;
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);checks++;}
 static CommandResult Send(LocalGateway g,string kind,string target,string id=null){var s=g.Snapshot();return g.Execute(new GameCommand(id??Guid.NewGuid().ToString(),s.PlayerId,kind,target,s.Revision));}
 public static string Run()
 {
  checks=0;
  foreach(var building in new[]{"sawmill","barracks"})
  {
   var clock=new Clock();var store=new Store {Value=new PlayerState {BastionLevel=2,SawmillLevel=1,BarracksLevel=1}};
   store.Value.Resources.Wood=2000;store.Value.Resources.Stone=2000;store.Value.Resources.Food=2000;
   var g=new LocalGateway(clock,store);var before=g.Snapshot();
   var binding=new ExistingSliceBinding(g,clock);
   var offer=binding.Read(building=="barracks"?"upgrade-barracks":building,false).Building.Action;
   Check(offer!=null&&offer.Enabled&&offer.Kind=="UpgradeBuilding"&&offer.Target==building,"UI exposes actual level II upgrade");
   var result=Send(g,"UpgradeBuilding",building,"upgrade-"+building);
   Check(result.Ok,building+" level II accepted: "+result.Message);
   var pending=g.Snapshot();Check(pending.ChapterThree.PendingBuildingLevel==2,"pending level II");
   Check(pending.Resources.Wood==before.Resources.Wood-(building=="barracks"?140:120),"authoritative wood cost");
   Check(pending.Resources.Stone==before.Resources.Stone-(building=="barracks"?90:78),"authoritative stone cost");
   Check(!Send(g,"UpgradeBuilding",building).Ok,"concurrent upgrade blocked");
   g=new LocalGateway(clock,store);Check(g.Snapshot().BuildingCompletesUtcTicks>clock.Now,"pending work survives reload");
   clock.Now+=TimeSpan.FromMinutes(1).Ticks;g.Advance();
   var done=g.Snapshot();Check((building=="sawmill"?done.SawmillLevel:done.BarracksLevel)==2,"completion raises correct building");
   Check(done.BuildingCompletesUtcTicks==0&&done.ChapterThree.PendingBuildingLevel==0,"completion clears task");
   Check(done.ChapterThree.EconomyUtcTicks==0,"no premature chapter III production");
   g=new LocalGateway(clock,store);g.Advance();
   Check((building=="sawmill"?g.Snapshot().SawmillLevel:g.Snapshot().BarracksLevel)==2,"completed upgrade survives reload");
   Check(g.Snapshot().Resources.Wood==done.Resources.Wood,"reload does not duplicate payment or rewards");
   Check(!Send(g,"UpgradeBuilding",building).Ok,"cannot exceed Bastion II");
   Check(Send(g,"UpgradeBuilding",building,"upgrade-"+building).Ok,"retry of same command remains idempotent");
  }
  foreach(var level in new[]{1,2,3,4,5})
  {
   var clock=new Clock();var store=new Store{Value=new PlayerState{BastionLevel=level,SawmillLevel=level,BarracksLevel=level}};
   store.Value.ChapterThree.GranaryLevel=level;store.Value.ChapterThree.StoneworksLevel=level;
   store.Value.ChapterThree.Inventory.AddRange(new[]{"iron-brace","treated-hide","stone-tusk"});
   store.Value.Resources.Wood=10000;store.Value.Resources.Stone=10000;
   var g=new LocalGateway(clock,store);
   foreach(var id in new[]{"sawmill","barracks","granary","stoneworks"})Check(!Send(g,"UpgradeBuilding",id).Ok,"cap "+level+" "+id);
  }
  var state=new PlayerState{SawmillLevel=1};
  Check(!EarlyRetentionRules.VigilantsUnlocked(state),"sawmill alone does not reveal Vigilants");
  state.BastionLevel=2;state.BarracksLevel=1;state.ChapterProgress.TrainedArchers=20;
  state.ChapterProgress.SeenNarrative.AddRange(new[]{"world","barracks"});
  Check(NarrativeCoherenceRules.NextUnseenBeat(state)=="march-preparation","march contextual lesson");
  var teachingStore=new Store{Value=state};var teachingGateway=new LocalGateway(new Clock(),teachingStore);
  Check(teachingGateway.AcknowledgeNarrative("march-preparation"),"new introduction accepted by save authority");
  Check(NarrativeCoherenceRules.Seen(new LocalGateway(new Clock(),teachingStore).Snapshot(),"march-preparation"),"introduction survives reload");
  state=teachingGateway.Snapshot();state.EngendroDefeated=true;
  Check(!EarlyRetentionRules.VigilantsUnlocked(state),"eligible Vigilants hidden until taught");
  state.ChapterProgress.SeenNarrative.Add("vigilants-introduction");
  Check(EarlyRetentionRules.VigilantsUnlocked(state),"Vigilants visible after introduction");
  state.ChapterProgress.MarchConfirmed=true;
  Check(!EarlyRetentionRules.ScoutsUnlocked(state),"scouts hidden at Bastion II");
  state.BastionLevel=3;
  Check(!EarlyRetentionRules.ScoutsUnlocked(state),"scouts hidden until taught");
  state.ChapterProgress.SeenNarrative.Add("scouts-introduction");
  Check(EarlyRetentionRules.ScoutsUnlocked(state),"scouts visible after introduction");
  state.Available=new ArmyRoster();
  Check(EarlyRetentionRules.VigilantsUnlocked(state)&&EarlyRetentionRules.ScoutsUnlocked(state),"access persists while troops away");
  return "PASS "+checks+" domain assertions; no Unity visual certification";
 }
}
