using System;
using System.Text.Json;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.UI.RoyalGold;

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

  var legacy=State(1);legacy.ChapterProgress.GatheredWood=130;
  Func<string,bool> legacyRead=beat=>beat=="start"||beat=="world";
  foreach(var beat in NarrativeCoherenceRules.LegacyAcknowledgements("{\"SchemaVersion\":1}",legacyRead))
   legacy.ChapterProgress.SeenNarrative.Add(beat);
  Check(NarrativeCoherenceRules.NextUnseenBeat(legacy)=="mark",
    "Imported legacy acknowledgements cannot starve the next unread narrative");
  var freshUnseen=State(1);
  Check(NarrativeCoherenceRules.NextUnseenBeat(freshUnseen)=="start"&&freshUnseen.ChapterProgress.SeenNarrative.Count==0,
    "Selecting a new story neither acknowledges it nor mutates the save");
  Check(NarrativeCoherenceRules.LegacyAcknowledgements(null,beat=>true).Length==0,
    "Fresh game never imports global dialogue flags");
  Check(NarrativeCoherenceRules.LegacyAcknowledgements("{\"ChapterProgress\":{\"SeenNarrative\":[]}}",beat=>true).Length==0,
    "Reloading an unread modern save never imports another game's flags");
  var legacyBeats=NarrativeCoherenceRules.LegacyAcknowledgements("{\"SchemaVersion\":1}",legacyRead);
  Check(legacyBeats.Length==2&&legacyBeats[0]=="start"&&legacyBeats[1]=="world",
    "Only actual read flags are imported for a pre-ledger save");
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
  Check(NarrativeCoherenceRules.NextUnseenBeat(c3)=="vigilants-introduction",
        "Unread feature introduction survives acknowledgment of the main story");
  c3.ChapterProgress.SeenNarrative.Add("vigilants-introduction");
  Check(NarrativeCoherenceRules.NextUnseenBeat(c3)=="","No repeated automatic lore after acknowledgment");

  long clockTicks=new DateTime(2026,11,5,12,0,0,DateTimeKind.Utc).Ticks;
  var firstV=State(1);firstV.SawmillLevel=0;
  Check(EarlyRetentionFirstUseTutorial.Vigilants(firstV)==FirstUseTutorialPhase.Locked,
        "Vigilants lesson is locked before sawmill");
  firstV.SawmillLevel=1;
  Check(EarlyRetentionFirstUseTutorial.Vigilants(firstV)==FirstUseTutorialPhase.Locked,
        "Sawmill alone cannot reveal Vigilants before the creature victory");
  firstV.EngendroDefeated=true;
  Check(EarlyRetentionFirstUseTutorial.Vigilants(firstV)==FirstUseTutorialPhase.Locked,
        "Vigilants stay locked until their introduction is read");
  firstV.ChapterProgress.SeenNarrative.Add("vigilants-introduction");
  Check(EarlyRetentionFirstUseTutorial.Vigilants(firstV)==FirstUseTutorialPhase.FirstAction,
        "Vigilants first battle remains visible before a win");
  firstV.Retention.LastClearedStage=1;
  Check(EarlyRetentionFirstUseTutorial.Vigilants(firstV)==FirstUseTutorialPhase.Learned,
        "Vigilants guidance disappears after an earned stage");

  var firstS=State(2);
  Check(EarlyRetentionFirstUseTutorial.Explorers(firstS)==FirstUseTutorialPhase.Locked,
        "Scouts are not introduced until the first March is confirmed");
  firstS.ChapterProgress.MarchConfirmed=true;
  Check(EarlyRetentionFirstUseTutorial.Explorers(firstS)==FirstUseTutorialPhase.Locked,
        "A confirmed march does not reveal Scouts before Bastion III");
  firstS.BastionLevel=3;
  Check(EarlyRetentionFirstUseTutorial.Explorers(firstS)==FirstUseTutorialPhase.Locked,
        "Scouts stay locked until their introduction is read");
  firstS.ChapterProgress.SeenNarrative.Add("scouts-introduction");
  Check(EarlyRetentionFirstUseTutorial.Explorers(firstS)==FirstUseTutorialPhase.FirstAction,
        "Scouts teach how to send the first real order");
  firstS.Retention.Orders.Add(new ScoutOrder{Id="first",StartedUtcTicks=clockTicks,EndsUtcTicks=clockTicks+TimeSpan.FromMinutes(3).Ticks});
  Check(EarlyRetentionFirstUseTutorial.Explorers(firstS)==FirstUseTutorialPhase.AwaitingResult,
        "Scouts change instruction while the first order is underway");
  firstS.Retention.TotalScoutReports=1;
  Check(EarlyRetentionFirstUseTutorial.Explorers(firstS)==FirstUseTutorialPhase.Learned,
        "Scouts stop repeating after an earned report");
  var firstUseObjectives=new[]{
   "b2.train-archers","b2.prepare-march","b2.raise-expedition-power",
   "b3.food","b3.hunt","b3.investigate","b3.lyra","b3.fissure",
   "b3.infrastructure","b4.priority","b5.stoneworks","b5.stone","b5.army","b5.rare"
  };
  foreach(var objective in firstUseObjectives) {
   var spanish=UnityContextTutorial.ForObjective(objective,false);
   var english=UnityContextTutorial.ForObjective(objective,true);
   Check(!string.IsNullOrWhiteSpace(spanish)&&!string.IsNullOrWhiteSpace(english)
     &&spanish.Length<=140&&english.Length<=140,
      "Localized and concise tutorial for "+objective);
  }
  Check(UnityContextTutorial.ForObjective("b2.train-archers",false).Contains("Cuartel"),
        "Training first action is explained");
  Check(UnityContextTutorial.ForObjective("b2.prepare-march",false).Contains("no envía"),
        "Preparing a march is distinct from sending it");
  Check(UnityContextTutorial.ForObjective("b2.raise-expedition-power",false).Contains("no del Bastión"),
        "Expedition power is not Bastion power");
  Check(!UnityContextTutorial.ForObjective("b3.lyra",false).Contains("Lyra"),
        "The mysterious frontier contact remains unknown");
  Check(UnityContextTutorial.ForObjective("b3.investigate",false).Contains("20 minutos"),
        "Fissure investigation keeps the canonical duration");
  Check(UnityContextTutorial.ForObjective("b3.fissure",false).Contains("Lyra"),
        "Lyra is named after contact");
  Check(UnityContextTutorial.ForObjective("b4.priority",true).Contains("does not lock"),
        "Governance choice never closes alternatives");
  Check(UnityContextTutorial.ForObjective("unknown",false)==null,
        "No fabricated lesson for an unknown action");
  Check(NarrativeCoherenceRules.CanPresent(false,false,false,false,false),
        "Narrative can start when no management or report surface is active");
  Check(!NarrativeCoherenceRules.CanPresent(true,false,false,false,false),
        "Existing narrative is never replaced by another");
  Check(!NarrativeCoherenceRules.CanPresent(false,true,false,false,false),
        "Ordinary modal prevents automatic story interruption");
  Check(!NarrativeCoherenceRules.CanPresent(false,false,true,false,false),
        "Training and march preparation are uninterrupted");
  Check(!NarrativeCoherenceRules.CanPresent(false,false,false,true,false),
        "Vigilants and Scouts are uninterrupted");
  Check(!NarrativeCoherenceRules.CanPresent(false,false,false,false,true),
        "Reading mailbox reports is uninterrupted");
  Check(!NarrativeCoherenceRules.CanPresent(false,false,false,false,false,true),
        "A selected building's contextual action is not interrupted by automatic lore");
  var inbox=State(3);
  Check(!UnityContextTutorial.NeedsMailboxOpening(inbox),
        "Mailbox is not introduced before a genuine delivery");
  Check(inbox.Mailbox.Receive(new EldoriaV41Message {
    EventId="first-real-mail",Category="informes",Title="Exploración"
  }),"A genuine first mail event is delivered");
  Check(UnityContextTutorial.NeedsMailboxOpening(inbox),
        "First unread message teaches the Mailbox");
  Check(UnityContextTutorial.MailboxFirstUse(false).Contains("Correo") &&
        UnityContextTutorial.MailboxFirstUse(true).Contains("Mail"),
        "Mailbox explanation is available in both languages");
  inbox.Mailbox.MarkRead("first-real-mail");
  Check(!UnityContextTutorial.NeedsMailboxOpening(inbox),
        "Reading the first message completes the guidance");
  inbox.Mailbox.Receive(new EldoriaV41Message {
    EventId="second-mail",Category="informes",Title="Segundo mensaje"
  });
  Check(!UnityContextTutorial.NeedsMailboxOpening(inbox),
        "New reports do not repeat a lesson after it was learned");
  var store=new Store{Value=State(3)};
  var clock=new Clock();
  var gateway=new LocalGateway(clock,store);
  Check(gateway.AcknowledgeNarrative("frontier-open"),"Finished dialogue records once");
  Check(!gateway.AcknowledgeNarrative("frontier-open"),"Repeating completed dialogue creates no duplicate");
  gateway=new LocalGateway(clock,store);
  Check(gateway.Snapshot().ChapterProgress.SeenNarrative.FindAll(x=>x=="frontier-open").Count==1,
      "Dialogue ledger persists across real gateway reload");
  var actionStore=new Store{Value=State(3)};
  actionStore.Value.Resources.Food=5000;
  var actionClock=new Clock();
  var actions=new LocalGateway(actionClock,actionStore);
  var binding=new ExistingSliceBinding(actions,actionClock);
  var granary=binding.Read("granary",false).Building.Action;
  Check(granary!=null&&granary.Enabled,"Player sees a valid Granary offer");
  actionClock.UtcTicks+=TimeSpan.FromSeconds(60).Ticks;
  actions.Advance();
  Check(actions.Snapshot().Revision!=granary.Revision,"Passive progression really changes the displayed revision");
  Check(binding.Submit(granary).Ok,"An unchanged Granary offer survives passive production");
  var paid=actions.Snapshot().Resources.Wood;
  Check(!binding.Submit(granary).Ok&&actions.Snapshot().Resources.Wood==paid,
      "Double submission cannot charge the same building twice");
  var staleStore=new Store{Value=State(3)};staleStore.Value.Resources.Food=5000;
  var staleGateway=new LocalGateway(new Clock(),staleStore);
  var staleBinding=new ExistingSliceBinding(staleGateway,new Clock());
  var old=staleBinding.Read("granary",false).Building.Action;
  var other=staleBinding.Read("granary",false).Building.Action;
  Check(staleBinding.Submit(other).Ok,"A second view may start the originally offered building");
  var afterPayment=staleGateway.Snapshot().Resources.Wood;
  Check(!staleBinding.Submit(old).Ok&&staleGateway.Snapshot().Resources.Wood==afterPayment,
      "Stale offer cannot start an already paid operation");
  var fissureState=State(3);fissureState.ChapterThree.FissureExplored=true;
  fissureState.ChapterThree.LyraContacted=true;
  var fissureBinding=new ExistingSliceBinding(new LocalGateway(new Clock(),new Store{Value=fissureState}),new Clock());
  Check(fissureBinding.Read("fissure-valoria",false).Building.Time=="",
      "The Lyra assault does not advertise the finished investigation's twenty minutes");
  Console.WriteLine("UNITY_NARRATIVE_DOMAIN_PROBE_PASS");
 }
}


