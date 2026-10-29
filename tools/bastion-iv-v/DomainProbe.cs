using System;
using Eldoria.Domain;
using Eldoria.Application;
using Eldoria.UI.RoyalGold;
class DomainProbe {
 sealed class Clock:IClock { public long UtcTicks{get;set;}=new DateTime(2026,10,29,0,0,0,DateTimeKind.Utc).Ticks;public void Add(int s)=>UtcTicks+=TimeSpan.FromSeconds(s).Ticks; }
 sealed class Store:IStateStore {string json;public PlayerState Value;static readonly System.Text.Json.JsonSerializerOptions Options=new(){IncludeFields=true};public PlayerState Load()=>json==null?Value:System.Text.Json.JsonSerializer.Deserialize<PlayerState>(json,Options);public void Save(PlayerState s){json=System.Text.Json.JsonSerializer.Serialize(s,Options);}}
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);Console.WriteLine("PASS "+name);}
 static CommandResult Do(LocalGateway g,string k,string t,string id=null){var s=g.Snapshot();return g.Execute(new GameCommand(id??Guid.NewGuid().ToString(),s.PlayerId,k,t,s.Revision));}
 static PlayerState Ready()=>new(){BastionLevel=3,SawmillLevel=2,BarracksLevel=2,JourneyComplete=true,ScoutDefeated=true,EngendroDefeated=true,Resources=new(){Wood=100000,Stone=100000,Food=100000},ChapterThree=new(){GranaryLevel=2,FissureClosed=true,Inventory=new(){"iron-brace","treated-hide","stone-tusk"}}};
 static void Main(){
  SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
  foreach(string choice in new[]{"production","defense","civil"}){
   var clock=new Clock();var store=new Store{Value=Ready()};var g=new LocalGateway(clock,store);var before=g.Snapshot();var binding=new ExistingSliceBinding(g,clock);Check(binding.Read("bastion",false).Building.Costs.Contains("300"),"IV UI shows food cost");int power=SliceRules.TotalPower(before).Total;
   Check(!Do(g,"ChooseDevelopment",choice).Ok,"no early choice");
   Check(!Do(g,"Build","stoneworks").Ok,"quarry locked before V");
   Check(Do(g,"AdvanceBastion","bastion","iv").Ok,"start IV");Check(Do(g,"AdvanceBastion","bastion","iv").Ok,"idempotent retry");
   var paid=g.Snapshot();Check(paid.Resources.Wood==before.Resources.Wood-850&&paid.Resources.Stone==before.Resources.Stone-700&&paid.Resources.Food==before.Resources.Food-300,"exact IV debit once");
   clock.Add(15);g.Advance();Check(g.Snapshot().BastionLevel==3,"IV not early");clock.Add(1);g=new(clock,store);Check(g.Snapshot().BastionLevel==4,"IV offline completion");Check(SliceRules.TotalPower(g.Snapshot()).Total==power+1150,"IV power parity");
   Check(!Do(g,"AdvanceBastion","bastion").Ok,"V requires choice");binding=new ExistingSliceBinding(g,clock);Check(!binding.Read("bastion",false).Building.Action.Enabled,"V UI disabled until priority");Check(!Do(g,"ChooseDevelopment","unknown").Ok,"reject invalid choice");before=g.Snapshot();power=SliceRules.TotalPower(before).Total;
   Check(Do(g,"ChooseDevelopment",choice,"choice").Ok,"choose "+choice);Check(Do(g,"ChooseDevelopment",choice,"choice").Ok,"choice retry safe");Check(!Do(g,"ChooseDevelopment","civil","other").Ok,"cannot choose again");g=new(clock,store);var chosen=g.Snapshot();
   Check(chosen.ChapterThree.DevelopmentPriority==choice,"choice persists");Check(chosen.Resources.Wood-before.Resources.Wood==(choice=="production"?260:choice=="civil"?180:0),"choice wood");Check(chosen.Resources.Stone-before.Resources.Stone==(choice=="production"?180:choice=="defense"?220:0),"choice stone");Check(chosen.Resources.Food-before.Resources.Food==(choice=="civil"?280:0),"choice food");Check(SliceRules.TotalPower(chosen).Total-power==(choice=="defense"?100:0),"choice power");
   before=g.Snapshot();power=SliceRules.TotalPower(before).Total;Check(Do(g,"AdvanceBastion","bastion").Ok,"start V");paid=g.Snapshot();Check(paid.Resources.Wood==before.Resources.Wood-1100&&paid.Resources.Stone==before.Resources.Stone-900&&paid.Resources.Food==before.Resources.Food-500,"exact V cost");clock.Add(18);g=new(clock,store);Check(g.Snapshot().BastionLevel==5&&g.Snapshot().SawmillLevel==2,"V never upgrades other buildings");Check(SliceRules.TotalPower(g.Snapshot()).Total==power+1500,"V power");
   before=g.Snapshot();Check(Do(g,"Build","stoneworks","quarry").Ok,"build quarry");paid=g.Snapshot();Check(paid.Resources.Wood==before.Resources.Wood-500&&paid.Resources.Food==before.Resources.Food-200&&paid.Resources.Stone==before.Resources.Stone,"quarry debit");clock.Add(11);g.Advance();Check(g.Snapshot().ChapterThree.StoneworksLevel==0,"quarry not early");clock.Add(1);g=new(clock,store);Check(g.Snapshot().ChapterThree.StoneworksLevel==1&&g.Snapshot().ChapterThree.RankUnlocked,"quarry completes and rank unlocks");
   int stone=g.Snapshot().Resources.Stone;clock.Add(10);g.Advance();Check(g.Snapshot().Resources.Stone==stone+20,"stone 2 per level/sec");
   foreach(var id in new[]{"sawmill","barracks","granary","stoneworks"}){
    int level=id=="stoneworks"?1:2;
    for(;level<5;level++){Check(Do(g,"UpgradeBuilding",id).Ok,id+" upgrade "+level);clock.Add(8+level*2);g=new(clock,store);}
    Check(!Do(g,"UpgradeBuilding",id).Ok,id+" cap V");
   }
   Check(!Do(g,"AdvanceBastion","bastion").Ok,"VI deliberately unavailable");binding=new ExistingSliceBinding(g,clock);Check(!binding.Read("stoneworks",false).Building.Action.Enabled,"V quarry UI cannot purchase VI");stone=g.Snapshot().Resources.Stone;clock.Add(7200);g=new(clock,store);Check(g.Snapshot().Resources.Stone==stone+36000,"offline stone cap 1 hour");g=new(clock,store);Check(g.Snapshot().Resources.Stone==stone+36000,"reload cannot double production");Check(g.Snapshot().ChapterThree.Inventory.Count==3,"materials not consumed");
  }
  foreach(string missing in new[]{"fissure","sawmill","barracks","granary","food"}){
   var s=Ready();if(missing=="fissure")s.ChapterThree.FissureClosed=false;if(missing=="sawmill")s.SawmillLevel=1;if(missing=="barracks")s.BarracksLevel=1;if(missing=="granary")s.ChapterThree.GranaryLevel=1;if(missing=="food")s.Resources.Food=0;
   Check(!BastionProgressionCatalog.RequirementsMet(s,BastionProgressionCatalog.ForLevel(4)),"IV requires "+missing);
  }
  Console.WriteLine("BASTION_IV_V_DOMAIN_PASS");
 }
}
