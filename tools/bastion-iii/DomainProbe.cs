using System;
using Eldoria.Domain;
using Eldoria.Application;
class DomainProbe {
 sealed class Clock:IClock { public long UtcTicks{get;set;}=new DateTime(2026,10,27,0,0,0,DateTimeKind.Utc).Ticks;public void Add(int s)=>UtcTicks+=TimeSpan.FromSeconds(s).Ticks; }
 sealed class Store:IStateStore { public PlayerState Value;public PlayerState Load()=>Value;public void Save(PlayerState s)=>Value=s; }
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);Console.WriteLine("PASS "+name);}
 static CommandResult Do(LocalGateway g,string k,string t,string id=null){var s=g.Snapshot();return g.Execute(new GameCommand(id??Guid.NewGuid().ToString(),s.PlayerId,k,t,s.Revision));}
 static void Main(){
  SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
  var clock=new Clock();var store=new Store{Value=new PlayerState{BastionLevel=2,SawmillLevel=1,BarracksLevel=1,JourneyComplete=true,EngendroDefeated=true,Available=new ArmyRoster{ArcherT1=56},Resources=new ResourceWallet{Wood=5000,Stone=5000}}};
  var g=new LocalGateway(clock,store);var before=g.Snapshot().Resources.Wood;
  Check(Do(g,"AdvanceBastion","bastion","ascend").Ok,"start III");Check(Do(g,"AdvanceBastion","bastion","ascend").Ok,"retry same command");
  Check(g.Snapshot().Resources.Wood==before-650,"debit once");clock.Add(13);g.Advance();Check(g.Snapshot().BastionLevel==2,"not early");clock.Add(1);g=new LocalGateway(clock,store);Check(g.Snapshot().BastionLevel==3,"offline III completion");
  Check(g.Snapshot().Resources.Food==400,"chapter II food rewards");
  Check(Do(g,"Build","granary").Ok,"build granary");clock.Add(9);g=new LocalGateway(clock,store);Check(g.Snapshot().ChapterThree.GranaryLevel==1,"offline granary");
  var food=g.Snapshot().Resources.Food;clock.Add(10);g.Advance();Check(g.Snapshot().Resources.Food==food+10,"food rate");
  Check(Do(g,"InvestigateFissure","fissure-valoria").Ok,"20 minute investigation");Check(!Do(g,"InvestigateFissure","fissure-valoria").Ok,"no duplicate investigation");Check(!Do(g,"ContactLyra","fissure-valoria").Ok,"no early contact");Check(!Do(g,"Fight","hunt-wolf").Ok,"Aldric reserved");clock.Add(1199);g.Advance();Check(!g.Snapshot().ChapterThree.FissureExplored,"full investigation duration");clock.Add(1);g=new LocalGateway(clock,store);Check(g.Snapshot().ChapterThree.FissureExplored,"offline investigation");Check(!g.Snapshot().ChapterThree.LyraRecruited,"no premature recruitment");
  Check(!Do(g,"ConfigureMarch","march-main:20:lyra").Ok,"Lyra unavailable before victory");Check(Do(g,"ContactLyra","fissure-valoria").Ok,"contact Lyra");Check(!g.Snapshot().ChapterThree.LyraRecruited,"contact is not recruitment");
  var stone=g.Snapshot().Resources.Stone;Check(Do(g,"Fight","fissure-valoria").Ok,"assault");clock.Add(4);g=new LocalGateway(clock,store);var won=g.Snapshot();Check(won.ChapterThree.FissureClosed&&won.ChapterThree.LyraRecruited,"closure recruits Lyra");Check(won.Resources.Stone==stone+300+260,"original loot and mission reward");Check(won.ChapterThree.Inventory.FindAll(x=>x=="breach-manuscript").Count==1,"unique manuscript");Check(!Do(g,"Fight","fissure-valoria").Ok,"closed fissure cannot be farmed");g=new LocalGateway(clock,store);Check(g.Snapshot().Resources.Stone==won.Resources.Stone,"reload never duplicates loot");
  Check(Do(g,"Fight","hunt-wolf").Ok,"wolf hunt");clock.Add(4);g.Advance();Check(g.Snapshot().ChapterThree.Hunts==1,"hunt counter");Check(!Do(g,"Fight","hunt-wolf").Ok,"hunt cooldown");Check(Do(g,"Fight","hunt-boar").Ok,"boar hunt");clock.Add(4);g.Advance();Check(g.Snapshot().ChapterThree.Hunts==2,"two hunts");
  Check(Do(g,"UpgradeBuilding","granary").Ok,"granary level2");clock.Add(10);g.Advance();Check(g.Snapshot().ChapterThree.GranaryLevel==2,"level2 completes");Check(Do(g,"UpgradeBuilding","granary").Ok,"hide permits level3");clock.Add(12);g.Advance();Check(g.Snapshot().ChapterThree.GranaryLevel==3,"level3 completes");Check(!Do(g,"UpgradeBuilding","granary").Ok,"building cannot exceed Bastion");
  Check(Do(g,"Gather","meat-valoria").Ok,"depart for meat");clock.Add(2);g.Advance();Check(g.Snapshot().March.Phase=="gathering","meat requires gathering");clock.Add(9);g.Advance();Check(g.Snapshot().ChapterThree.MeatRemaining==1100,"no early meat debit");clock.Add(1);g=new LocalGateway(clock,store);Check(g.Snapshot().March.PendingFood==300&&g.Snapshot().ChapterThree.MeatRemaining==800,"persistent meat load");clock.Add(2);g.Advance();Check(g.Snapshot().March.Phase=="idle","meat returned");
  Check(Do(g,"ConfigureMarch","march-main:20:aldric+lyra").Ok,"recruited dual hero composition");g=new LocalGateway(clock,store);Check(g.Snapshot().PreparedHeroId=="aldric+lyra","hero composition persists");clock.Add(45);g.Advance();Check(Do(g,"Fight","hunt-wolf").Ok,"prepared hunt");Check(g.Snapshot().March.Troops.ArcherT1==20&&g.Snapshot().March.HeroId=="aldric+lyra","dispatch honors composition");clock.Add(4);g.Advance();
  var copy=g.Snapshot();copy.ChapterThree.Inventory.Clear();Check(g.Snapshot().ChapterThree.Inventory.Count>0,"snapshot inventory isolated");
  Console.WriteLine("BASTION_III_DOMAIN_PROBE_PASS");
 }
}
