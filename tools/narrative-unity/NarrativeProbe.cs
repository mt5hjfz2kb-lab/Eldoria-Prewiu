using System;
using System.Text.Json;
using Eldoria.Application;
using Eldoria.Domain;

public static class NarrativeProbe {
 sealed class Clock : IClock {
  public long UtcTicks { get; set; } = new DateTime(2026,11,5,12,0,0,DateTimeKind.Utc).Ticks;
 }
 sealed class Store : IStateStore {
  public PlayerState Value;
  string saved;
  static readonly JsonSerializerOptions Options = new JsonSerializerOptions { IncludeFields=true };
  public PlayerState Load() => saved==null ? Value : JsonSerializer.Deserialize<PlayerState>(saved,Options);
  public void Save(PlayerState s) {
   saved=JsonSerializer.Serialize(s,Options);
   Value=JsonSerializer.Deserialize<PlayerState>(saved,Options);
  }
 }
 static void Check(bool truth,string message) {
  if(!truth)throw new Exception("FAIL: "+message);
  Console.WriteLine("PASS: "+message);
 }
 static PlayerState State(int level) => new PlayerState { BastionLevel=level, SawmillLevel=1, BarracksLevel=1, JourneyComplete=true,
  EngendroDefeated=level>=3, Available=new ArmyRoster{ArcherT1=56},Resources=new ResourceWallet{Wood=5000,Stone=5000} };
 static void Main() {
  var first=State(1);
  Check(NarrativeCoherenceRules.NextUnseenBeat(first)=="start","Fresh Bastion I begins with the actual prologue");
  first.ChapterProgress.SeenNarrative.Add("start");
  Check(NarrativeCoherenceRules.NextUnseenBeat(first)=="world","Sawmill opens world only after the prologue");
  first.ChapterProgress.SeenNarrative.Add("world");
  first.ChapterProgress.GatheredWood=130;
  Check(NarrativeCoherenceRules.NextUnseenBeat(first)=="mark","First recovered wood introduces the unknown mark");
  first.ChapterProgress.SeenNarrative.Add("mark");
  first.BastionLevel=2;
  Check(NarrativeCoherenceRules.NextUnseenBeat(first)=="barracks","Bastion II introduces the barracks before combat closure");
  first.ChapterProgress.SeenNarrative.Add("barracks");
  first.EngendroDefeated=true;
  Check(NarrativeCoherenceRules.NextUnseenBeat(first)=="secured","Bastion II completes after the actual creature victory");

  var c3=State(3);
  Check(NarrativeCoherenceRules.NextUnseenBeat(c3)=="frontier-open","Bastion III announces unexplored frontier");
  Check(NarrativeCoherenceRules.FissureKnowledgeStage(c3)==0,"Undiscovered fissure does not reveal Lyra");
  c3.ChapterThree.InvestigationEndsUtcTicks=DateTime.UtcNow.AddMinutes(20).Ticks;
  Check(NarrativeCoherenceRules.FissureKnowledgeStage(c3)==0,"Active 20-minute investigation does not spoil the archer");
  c3.ChapterThree.InvestigationEndsUtcTicks=0;
  c3.ChapterThree.FissureExplored=true;
  Check(NarrativeCoherenceRules.FissureKnowledgeStage(c3)==1,"Finished investigation reveals human tracks only");
  c3.ChapterThree.LyraContacted=true;
  Check(NarrativeCoherenceRules.FissureKnowledgeStage(c3)==2,"Lyra becomes known only after contact");
  c3.ChapterThree.FissureClosed=true;
  Check(NarrativeCoherenceRules.NextUnseenBeat(c3)=="fissure-closed","Completed fissure cannot replay stale exploration intro");

  c3.BastionLevel=4;
  Check(NarrativeCoherenceRules.NextUnseenBeat(c3)=="fissure-closed","Lyra conclusion precedes Bastion IV council");
  c3.BastionLevel=5;
  c3.ChapterThree.ExtractionOddity=true;
  Check(NarrativeCoherenceRules.NextUnseenBeat(c3)=="fissure-closed","Late saved dialogue survives crossing to Bastion V");
  c3.ChapterProgress.SeenNarrative.Add("fissure-closed");
  Check(NarrativeCoherenceRules.NextUnseenBeat(c3)=="productive-valoria","Bastion V introduces its production after Lyra's ending");
  c3.ChapterProgress.SeenNarrative.Add("productive-valoria");
  Check(NarrativeCoherenceRules.NextUnseenBeat(c3)=="extraction-oddity","Anomaly is revealed after actual stone discovery");
  c3.ChapterProgress.SeenNarrative.Add("extraction-oddity");
  Check(NarrativeCoherenceRules.NextUnseenBeat(c3)=="","No repeated automatic lore after acknowledgment");

  var store=new Store{Value=State(3)};
  var clock=new Clock();
  var gateway=new LocalGateway(clock,store);
  Check(gateway.AcknowledgeNarrative("frontier-open"),"Finished dialogue records once");
  Check(!gateway.AcknowledgeNarrative("frontier-open"),"Repeating completed dialogue creates no duplicate");
  gateway=new LocalGateway(clock,store);
  Check(gateway.Snapshot().ChapterProgress.SeenNarrative.FindAll(x=>x=="frontier-open").Count==1,
      "Dialogue ledger persists across real gateway reload");
  Console.WriteLine("UNITY_NARRATIVE_DOMAIN_PROBE_PASS");
 }
}
