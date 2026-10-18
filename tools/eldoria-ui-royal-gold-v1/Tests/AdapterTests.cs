using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.UI.RoyalGold;
static class AdapterTests {
 sealed class Clock:IClock{public long Now=DateTime.UtcNow.Ticks;public long UtcTicks=>Now;}
 sealed class Memory:IStateStore{public PlayerState State;public int Saves;public Memory(PlayerState s){State=s;}public PlayerState Load()=>State;public void Save(PlayerState s){Saves++;State=s;}}
 static readonly List<string> passed=new List<string>();
 static void Check(bool yes,string name){if(!yes)throw new Exception("FAIL: "+name);passed.Add(name);Console.WriteLine("PASS "+name);}
 static PlayerState Seed()=>new PlayerState{Resources=new ResourceWallet{Wood=5000,Stone=3000,Food=7}};
 static void Main(){
  var clock=new Clock();var memory=new Memory(Seed());var gateway=new LocalGateway(clock,memory);var ui=new ExistingSliceBinding(gateway,clock,true);
  var before=gateway.Snapshot();int saves=memory.Saves;var d=ui.Read("sawmill",false);
  Check(d.Wood==before.Resources.Wood&&d.Stone==before.Resources.Stone&&d.Food==7,"wallet is actual snapshot");
  Check(d.Power==SliceRules.TotalPower(before).Total,"power uses authoritative rules");Check(d.IsTest,"fixture clearly marked TEST");
  Check(d.Building.Action.Enabled&&d.Building.Action.Kind=="Build"&&d.Building.Action.Target=="sawmill","sawmill real offer");
  Check(d.Building.Costs.Contains(SliceRules.SawmillWoodCost.ToString()),"cost uses active rules");
  Check(memory.Saves==saves&&gateway.Snapshot().Revision==before.Revision,"reading does not save or mutate revision");
  var offer=d.Building.Action;Check(ui.Submit(offer).Ok,"real gateway accepts valid construction");
  var after=gateway.Snapshot();Check(after.Resources.Wood==before.Resources.Wood-SliceRules.SawmillWoodCost,"only gateway charges resources");
  Check(!ui.Submit(offer).Ok&&gateway.Snapshot().Revision==after.Revision,"duplicate offer rejected without extra command");
  Check(!ui.Read("sawmill",false).Building.Action.Enabled,"own construction locked");
  clock.Now=after.BuildingCompletesUtcTicks;Check(ui.Read("sawmill",false).Building.Time=="Finalizando…","elapsed deadline not fabricated completion");
  Check(gateway.Snapshot().SawmillLevel==0,"read does not advance simulation");gateway.Advance();
  Check(ui.Read("sawmill",false).Building.Level==1,"external host advancement reflected");Check(ui.Read("sawmill",false).Building.Action==null,"no invented sawmill upgrade");
  Check(ui.Read("unknown-building",false).Building==null,"unknown target hidden");Check(ui.Read("",false).Building==null,"cleared selection hides panel");
  Check(ui.Read("barracks",false).Building.Requirements.Contains("Bastión II"),"barracks requirement presented");
  Check(!ui.Read("barracks",false).Building.Action.Enabled,"barracks stays blocked below level two");
  var locked=Seed();locked.Resources.Wood=0;var lg=new LocalGateway(clock,new Memory(locked));var lu=new ExistingSliceBinding(lg,clock);
  Check(!lu.Read("sawmill",false).Building.Action.Enabled,"insufficient resources disabled");Check(!lu.Read("",false).IsTest,"production not marked TEST");
  var s=Seed();var sg=new LocalGateway(clock,new Memory(s));var su=new ExistingSliceBinding(sg,clock);var stale=su.Read("sawmill",false).Building.Action;
  var st=sg.Snapshot();sg.Execute(new GameCommand("external:build",st.PlayerId,"Build","sawmill",st.Revision));int charged=sg.Snapshot().Resources.Wood;
  Check(!su.Submit(stale).Ok&&sg.Snapshot().Resources.Wood==charged,"stale view rejected without retry or charge");
  var b2=Seed();b2.BastionLevel=2;b2.BarracksLevel=1;b2.JourneyComplete=true;b2.SawmillLevel=1;
  var bg=new LocalGateway(clock,new Memory(b2));var bu=new ExistingSliceBinding(bg,clock);
  Check(bu.Read("bastion",false).Building.Action==null,"no unauthored Bastion III");
  var recruit=bu.Read("barracks",false).Building.Action;Check(recruit.Enabled&&recruit.Kind=="Recruit"&&recruit.Target=="archer:t1","real recruitment offer");
  int troops=bg.Snapshot().Available.Total;Check(bu.Submit(recruit).Ok,"recruit dispatched");
  Check(!bu.Read("barracks",false).Building.Action.Enabled,"recruitment pending is disabled");clock.Now=bg.Snapshot().RecruitmentCompletesUtcTicks;bg.Advance();
  Check(bu.Read("",false).Available==troops+SliceRules.RecruitArchers,"troops updated only after real gateway completion");
  var march=bu.Read("march-main",false).Building.Action;Check(march.Enabled&&march.Kind=="ConfigureMarch","real march configuration offered");Check(bu.Submit(march).Ok&&bg.Snapshot().MarchConfigured,"real march confirmed");
  Check(bu.Read("barracks",true).Building.Name=="Barracks","English building label");Check(bu.Read("barracks",true).Building.Costs.Contains("Wood:"),"English cost text");
  Check(RoyalFormatting.Remaining(0,0)=="","no invented timer");Check(RoyalFormatting.Remaining(10,11,true)=="Completing…","expired English timer");
  Check(RoyalFormatting.Amount(1234)=="1.234"&&RoyalFormatting.Amount(1234,true)=="1,234","Spanish and English numeric formatting");
  Check(!bu.Submit(null).Ok,"null offer guarded");
  string report=JsonSerializer.Serialize(new{source_sha=Environment.GetEnvironmentVariable("GITHUB_SHA"),run_id=Environment.GetEnvironmentVariable("GITHUB_RUN_ID"),engine="DOTNET_WITH_ACTUAL_DOMAIN_AND_APPLICATION_SOURCES",native_unity=false,total=passed.Count,passed},new JsonSerializerOptions{WriteIndented=true});
  Directory.CreateDirectory("docs/ui-royal-gold-v1");File.WriteAllText("docs/ui-royal-gold-v1/adapter-tests.json",report);Console.WriteLine("ROYAL_ADAPTER_TESTS_PASS "+passed.Count);
 }
}
