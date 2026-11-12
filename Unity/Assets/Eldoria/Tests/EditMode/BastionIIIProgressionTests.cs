using System;
using System.Linq;
using Eldoria.Application;
using Eldoria.Domain;
using NUnit.Framework;
using UnityEngine;
namespace Eldoria.Tests {
 public sealed class BastionIIIProgressionTests {
  sealed class Clock:IClock {public long UtcTicks{get;set;}=DateTime.UtcNow.Ticks;public void Add(int seconds){UtcTicks+=TimeSpan.FromSeconds(seconds).Ticks;}}
  sealed class Store:IStateStore {public PlayerState Value;string json;public PlayerState Load()=>json==null?Value:JsonUtility.FromJson<PlayerState>(json);public void Save(PlayerState state){json=JsonUtility.ToJson(state);Value=JsonUtility.FromJson<PlayerState>(json);}}
  string previous;
  [SetUp]public void Setup(){previous=SliceContentProfiles.ActiveRuntimeProfile;SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);}
  [TearDown]public void Cleanup(){SliceContentProfiles.SetRuntimeProfileOverride(previous);}
  static CommandResult Do(LocalGateway g,string kind,string target,string id=null){var s=g.Snapshot();return g.Execute(new GameCommand(id??Guid.NewGuid().ToString(),s.PlayerId,kind,target,s.Revision));}
  static Store Ready(int level=3)=>new Store{Value=new PlayerState{BastionLevel=level,SawmillLevel=1,BarracksLevel=1,JourneyComplete=true,EngendroDefeated=true,Available=new ArmyRoster{ArcherT1=56},Resources=new ResourceWallet{Wood=5000,Stone=5000}}};
  [TestCase(40,false)][TestCase(41,true)]public void AscensionHonorsOriginalTroopBoundaryWithoutSpendingOnFailure(int troops,bool allowed){var c=new Clock();var st=Ready(2);st.Value.Available.ArcherT1=troops;var g=new LocalGateway(c,st);var before=g.Snapshot();Assert.That(Do(g,"AdvanceBastion","bastion").Ok,Is.EqualTo(allowed));Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(before.Resources.Wood-(allowed?650:0)));Assert.That(g.Snapshot().Resources.Stone,Is.EqualTo(before.Resources.Stone-(allowed?500:0)));}
  [Test]public void Code16SaveAscendsOnceAfterFourMinutes(){var c=new Clock();var st=Ready(2);st.Value.ChapterThree=null;var g=new LocalGateway(c,st);int wood=g.Snapshot().Resources.Wood;Assert.That(Do(g,"AdvanceBastion","bastion","same").Ok);Assert.That(Do(g,"AdvanceBastion","bastion","same").Ok);Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(wood-650));c.Add(239);g.Advance();Assert.That(g.Snapshot().BastionLevel,Is.EqualTo(2));c.Add(1);g=new LocalGateway(c,st);Assert.That(g.Snapshot().BastionLevel,Is.EqualTo(3));
#if UNITY_6000_0_OR_NEWER || REWARD_EXPERIENCE_FEATURE
   Assert.That(g.Snapshot().Resources.Food,Is.Zero,"Chapter rewards stay in the bag until used");
   var food=g.Snapshot().ResourceRewards.Packets.Where(p=>p.Kind=="food").ToArray();
   Assert.That(food.Sum(p=>p.Amount),Is.EqualTo(400));
   Assert.That(food.All(p=>p.Collected&&!p.Used),Is.True);
   foreach(var packet in food)Assert.That(Do(g,ResourceRewardCommands.UsePacket,packet.ReceiptId).Ok,Is.True);
   Assert.That(g.Snapshot().Resources.Food,Is.EqualTo(400));
   g=new LocalGateway(c,st);
   Assert.That(g.Snapshot().Resources.Food,Is.EqualTo(400),"Reload never repeats consumed chapter rewards");
#else
   Assert.That(g.Snapshot().Resources.Food,Is.EqualTo(400));
#endif
  }
  [Test]public void InvestigationPersistsAndNeverRecruitsBeforeVictory(){
   var c=new Clock();var st=Ready();st.Value.Available.ArcherT1=190;var g=new LocalGateway(c,st);
   Assert.That(Do(g,"InvestigateFissure","fissure-valoria").Ok);
   Assert.That(Do(g,"ContactLyra","fissure-valoria").Ok,Is.False);
   Assert.That(Do(g,"Fight","hunt-wolf").Ok,Is.True,"Investigation occupies one shared slot and leaves two available");
   Assert.That(Do(g,"Gather","forest-valoria").Ok,Is.True);
   Assert.That(MarchCapacityRules.ActiveCount(g.Snapshot(),c.UtcTicks),Is.EqualTo(3));
   Assert.That(Do(g,"Gather","quarry-valoria").Ok,Is.False,"A fourth mixed operation is rejected despite spare troops");
   c.Add(1199);g=new LocalGateway(c,st);
   Assert.That(g.Snapshot().ChapterThree.FissureExplored,Is.False);
   Assert.That(MarchCapacityRules.ActiveCount(g.Snapshot(),c.UtcTicks),Is.EqualTo(1));
   c.Add(1);g=new LocalGateway(c,st);
   Assert.That(Do(g,"ContactLyra","fissure-valoria").Ok);
   Assert.That(g.Snapshot().ChapterThree.LyraRecruited,Is.False);
   int stone=g.Snapshot().Resources.Stone;
   Assert.That(Do(g,"Fight","fissure-valoria").Ok);
   c.Add(4);g=new LocalGateway(c,st);
   Assert.That(g.Snapshot().ChapterThree.LyraRecruited);
#if UNITY_6000_0_OR_NEWER || REWARD_EXPERIENCE_FEATURE
   Assert.That(g.Snapshot().Resources.Stone,Is.EqualTo(stone),"Victory alone never spends the reward packets");
   var loot=g.Snapshot().ResourceRewards.Packets.Single(p=>p.Origin=="combat"&&p.Kind=="stone"&&!p.Used);
   var mission=g.Snapshot().ResourceRewards.Packets.Single(p=>p.ReceiptId=="mission:c3-fissure:stone");
   Assert.That(loot.Amount+mission.Amount,Is.EqualTo(560));
   Assert.That(loot.Collected,Is.False,"Combat return awaits its explicit mail claim");
   Assert.That(mission.Collected&&!mission.Used,Is.True);
   Assert.That(Do(g,ResourceRewardCommands.ClaimMail,loot.MailEventId).Ok,Is.True);
   Assert.That(g.Snapshot().Resources.Stone,Is.EqualTo(stone),"Mail claim only transfers the packet to the bag");
   Assert.That(Do(g,ResourceRewardCommands.UsePacket,loot.ReceiptId).Ok,Is.True);
   Assert.That(Do(g,ResourceRewardCommands.UsePacket,mission.ReceiptId).Ok,Is.True);
#endif
   Assert.That(g.Snapshot().Resources.Stone,Is.EqualTo(stone+560));
   Assert.That(Do(g,"Fight","fissure-valoria").Ok,Is.False);
   g=new LocalGateway(c,st);
   Assert.That(g.Snapshot().ChapterThree.Inventory.FindAll(x=>x=="breach-manuscript").Count,Is.EqualTo(1));
   Assert.That(g.Snapshot().Resources.Stone,Is.EqualTo(stone+560));
  }
  [Test]public void ConstructionAndFoodAccrueAtActualOfflineDeadline(){var c=new Clock();var st=Ready();var g=new LocalGateway(c,st);Assert.That(Do(g,"Build","granary").Ok);int food=g.Snapshot().Resources.Food;c.Add(19);g=new LocalGateway(c,st);Assert.That(g.Snapshot().ChapterThree.GranaryLevel,Is.EqualTo(1));Assert.That(g.Snapshot().Resources.Food,Is.EqualTo(food+1));Assert.That(Do(g,"UpgradeBuilding","granary").Ok);c.Add(20);g=new LocalGateway(c,st);Assert.That(g.Snapshot().ChapterThree.GranaryLevel,Is.EqualTo(2));Assert.That(g.Snapshot().Resources.Food,Is.EqualTo(food+3));Assert.That(Do(g,"UpgradeBuilding","granary").Ok,Is.False);}
  [Test]public void ArcherTierTwoRequiresBarracksFourAndPreservesExistingTierOne(){
   var c=new Clock();var st=Ready(4);st.Value.BarracksLevel=3;st.Value.Available.ArcherT1=60;
   var g=new LocalGateway(c,st);
   Assert.That(Do(g,"Recruit","archer:t2:5").Ok,Is.False);
   Assert.That(g.Snapshot().Available.ArcherT2,Is.Zero);
   // A fresh authoritative store avoids altering the cached JSON outside a command.
   var upgraded=Ready(4);upgraded.Value.BarracksLevel=4;upgraded.Value.Available.ArcherT1=60;
   st=upgraded;g=new LocalGateway(c,st);
   int initialT1=g.Snapshot().Available.ArcherT1;
   Assert.That(Do(g,"Recruit","archer:t2:5").Ok,Is.True);
   Assert.That(g.Snapshot().PendingRecruitTier,Is.EqualTo(2));
   c.Add(SliceRules.RecruitSecondsFor(5)-1);g=new LocalGateway(c,st);
   Assert.That(g.Snapshot().Available.ArcherT2,Is.Zero);
   c.Add(1);g=new LocalGateway(c,st);
   Assert.That(g.Snapshot().Available.ArcherT1,Is.EqualTo(initialT1));
   Assert.That(g.Snapshot().Available.ArcherT2,Is.EqualTo(5));
   Assert.That(g.Snapshot().PendingRecruitTier,Is.EqualTo(1));
   g=new LocalGateway(c,st);
   Assert.That(g.Snapshot().Available.ArcherT2,Is.EqualTo(5));
  }
  [Test]public void BastionFiveStoryRareEnemyPersistsAndPaysItsMissionOnce(){
   var c=new Clock();var st=Ready(5);
   st.Value.BarracksLevel=4;st.Value.SawmillLevel=2;st.Value.Available.ArcherT1=150;
   st.Value.ChapterThree.GranaryLevel=2;st.Value.ChapterThree.StoneworksLevel=1;
   st.Value.ChapterThree.FissureClosed=true;st.Value.ChapterThree.Hunts=2;
   st.Value.ChapterThree.DevelopmentPriority="production";
   st.Value.Resources.Food=3000;
   var g=new LocalGateway(c,st);
   Assert.That(Do(g,"Fight","ash-stalker-valoria").Ok,Is.True);
   c.Add(20);g=new LocalGateway(c,st);
   Assert.That(g.Snapshot().ChapterThree.AshStalkerDefeated,Is.True);
   Assert.That(g.Snapshot().ChapterProgress.GrantedRewards,Does.Contain("c5-rare"));
   var before=g.Snapshot().Resources.Stone;
   Assert.That(Do(g,"Fight","ash-stalker-valoria").Ok,Is.False);
   g=new LocalGateway(c,st);
   Assert.That(g.Snapshot().Resources.Stone,Is.EqualTo(before));
  }
  [Test]public void SpeedupConsumesOneSavedChargeAndLeavesTheOtherQueuesIntact(){
   var c=new Clock();var st=Ready(4);st.Value.SpeedupFiveMinutes=1;
   var g=new LocalGateway(c,st);
   Assert.That(Do(g,"Build","granary").Ok,Is.True);
   Assert.That(Do(g,"UseSpeedup","building:5m").Ok,Is.True);
   g.Advance();
   Assert.That(g.Snapshot().ChapterThree.GranaryLevel,Is.EqualTo(1));
   Assert.That(g.Snapshot().SpeedupFiveMinutes,Is.Zero);
   Assert.That(Do(g,"UseSpeedup","building:5m").Ok,Is.False);
   g=new LocalGateway(c,st);
   Assert.That(g.Snapshot().ChapterThree.GranaryLevel,Is.EqualTo(1));
  }
  [Test]public void SnapshotDoesNotExposeMutableInventory(){var c=new Clock();var st=Ready();st.Value.ChapterThree.Inventory.Add("treated-hide");var g=new LocalGateway(c,st);g.Snapshot().ChapterThree.Inventory.Clear();Assert.That(g.Snapshot().ChapterThree.Inventory,Does.Contain("treated-hide"));}
  [Test]
  public void MeatReserveOnlyRespawnsAtActualFiveMinuteUtcDeadlineAcrossOfflineReloads()
  {
   var c=new Clock();var st=Ready();var g=new LocalGateway(c,st);
   int originalFood=g.Snapshot().Resources.Food;
   Assert.That(Do(g,"Gather","meat-valoria").Ok,Is.True);
   // The march pays its ONE harvest reward. It must not also regenerate
   // the physical food stock after the obsolete 45-second deadline.
   c.Add(60);
   g=new LocalGateway(c,st);
   var afterHarvest=g.Snapshot();
   Assert.That(afterHarvest.March.Phase,Is.EqualTo("idle"));
   Assert.That(afterHarvest.Resources.Food,Is.EqualTo(originalFood+1100));
   Assert.That(afterHarvest.ChapterThree.MeatRemaining,Is.Zero);
   Assert.That(afterHarvest.ChapterThree.MeatGeneration,Is.Zero);
   long deadline=afterHarvest.ChapterThree.MeatRespawnUtcTicks;
   Assert.That(deadline,Is.GreaterThan(c.UtcTicks));
   Assert.That(deadline-c.UtcTicks,Is.InRange(1L,RenewablePopulationPolicy.Delay.Ticks),
    "Expiry must still be in the future but within the actual five-minute cooldown");
   // Reopening the old schema-one save at t-1 may not prepay a harvest.
   c.UtcTicks=deadline-1;
   g=new LocalGateway(c,st);
   Assert.That(g.Snapshot().ChapterThree.MeatRemaining,Is.Zero);
   Assert.That(g.Snapshot().ChapterThree.MeatGeneration,Is.Zero);
   Assert.That(g.Snapshot().Resources.Food,Is.EqualTo(originalFood+1100));
   // Offline at t=300s: only the reserve returns, never a second reward.
   c.UtcTicks=deadline;
   g=new LocalGateway(c,st);
   Assert.That(g.Snapshot().ChapterThree.MeatRemaining,Is.EqualTo(1100));
   Assert.That(g.Snapshot().ChapterThree.MeatGeneration,Is.EqualTo(1));
   Assert.That(g.Snapshot().Resources.Food,Is.EqualTo(originalFood+1100));
   g=new LocalGateway(c,st);
   Assert.That(g.Snapshot().ChapterThree.MeatGeneration,Is.EqualTo(1));
   Assert.That(g.Snapshot().Resources.Food,Is.EqualTo(originalFood+1100));
  }
  [Test]public void OfflineProductionPaysBothLevelsAcrossTwoHoursWithoutRepeat(){var c=new Clock();var st=Ready();st.Value.ChapterThree.GranaryLevel=1;var g=new LocalGateway(c,st);int food=g.Snapshot().Resources.Food;Assert.That(Do(g,"UpgradeBuilding","granary").Ok);c.Add(7200);g=new LocalGateway(c,st);Assert.That(g.Snapshot().ChapterThree.GranaryLevel,Is.EqualTo(2));Assert.That(g.Snapshot().Resources.Food-food,Is.EqualTo(1367));g=new LocalGateway(c,st);Assert.That(g.Snapshot().Resources.Food-food,Is.EqualTo(1367));}

  [Test]public void ModernEmptyLedgerNeverImportsAnotherPlaythroughGlobalFlags(){
   Assert.That(NarrativeCoherenceRules.LegacyAcknowledgements(null,beat=>true),Is.Empty);
   Assert.That(NarrativeCoherenceRules.LegacyAcknowledgements("{\"ChapterProgress\":{\"SeenNarrative\":[]}}",beat=>true),Is.Empty);
   var old=NarrativeCoherenceRules.LegacyAcknowledgements("{\"SchemaVersion\":1}",beat=>beat=="start"||beat=="world");
   CollectionAssert.AreEqual(new[]{"start","world"},old);
   var clock=new Clock();var store=Ready(1);store.Value.SawmillLevel=1;store.Value.ChapterProgress.GatheredWood=130;
   var gateway=new LocalGateway(clock,store);
   foreach(var beat in old)gateway.AcknowledgeNarrative(beat);
   gateway=new LocalGateway(clock,store);
   Assert.That(NarrativeCoherenceRules.NextUnseenBeat(gateway.Snapshot()),Is.EqualTo("mark"));
   Assert.That(gateway.Snapshot().ChapterProgress.SeenNarrative.Count,Is.EqualTo(2));
  }
  [Test]public void FissureDescriptionNeverSpoilsLyraBeforeContact(){
   var s=Ready(3).Value;
   Assert.That(NarrativeCoherenceRules.FissureKnowledgeStage(s),Is.Zero);
   s.ChapterThree.InvestigationEndsUtcTicks=DateTime.UtcNow.Ticks+TimeSpan.FromMinutes(20).Ticks;
   Assert.That(NarrativeCoherenceRules.FissureKnowledgeStage(s),Is.Zero);
   s.ChapterThree.InvestigationEndsUtcTicks=0;s.ChapterThree.FissureExplored=true;
   Assert.That(NarrativeCoherenceRules.FissureKnowledgeStage(s),Is.EqualTo(1));
   s.ChapterThree.LyraContacted=true;
   Assert.That(NarrativeCoherenceRules.FissureKnowledgeStage(s),Is.EqualTo(2));
  }
  [Test]public void StoryQueueShowsFissureConclusionBeforeCouncilAndKeepsExtractionAfterLevelFive(){
   var s=Ready(3).Value;s.ChapterThree.FissureClosed=true;
   s.ChapterProgress.SeenNarrative.Add("frontier-open");
   Assert.That(NarrativeCoherenceRules.NextUnseenBeat(s),Is.EqualTo("fissure-closed"));
   s.BastionLevel=4;
   Assert.That(NarrativeCoherenceRules.NextUnseenBeat(s),Is.EqualTo("fissure-closed"));
   s.ChapterProgress.SeenNarrative.Add("fissure-closed");
   Assert.That(NarrativeCoherenceRules.NextUnseenBeat(s),Is.EqualTo("consolidate-valoria"));
   s.BastionLevel=5;s.ChapterThree.ExtractionOddity=true;
   Assert.That(NarrativeCoherenceRules.NextUnseenBeat(s),Is.EqualTo("productive-valoria"));
   s.ChapterProgress.SeenNarrative.Add("productive-valoria");
   Assert.That(NarrativeCoherenceRules.NextUnseenBeat(s),Is.EqualTo("extraction-oddity"));
  }
  [Test]public void ClosingFissureAtBastionThreeSkipsObsoleteFrontierIntro(){
   var s=Ready(3).Value;s.ChapterThree.FissureClosed=true;
   Assert.That(NarrativeCoherenceRules.NextUnseenBeat(s),Is.EqualTo("fissure-closed"));
  }
  [Test]public void UnreadLyraConclusionSurvivesTransitionToBastionFive(){
   var s=Ready(5).Value;s.ChapterThree.FissureClosed=true;
   s.ChapterThree.ExtractionOddity=true;
   Assert.That(NarrativeCoherenceRules.NextUnseenBeat(s),Is.EqualTo("fissure-closed"));
   s.ChapterProgress.SeenNarrative.Add("fissure-closed");
   Assert.That(NarrativeCoherenceRules.NextUnseenBeat(s),Is.EqualTo("productive-valoria"));
   s.ChapterProgress.SeenNarrative.Add("productive-valoria");
   Assert.That(NarrativeCoherenceRules.NextUnseenBeat(s),Is.EqualTo("extraction-oddity"));
  }
  [Test]public void VigilantsFirstUseGuideEndsOnlyAfterRealVictory(){
   var s=Ready(1).Value;s.SawmillLevel=0;
   Assert.That(EarlyRetentionFirstUseTutorial.Vigilants(s),Is.EqualTo(FirstUseTutorialPhase.Locked));
   s.SawmillLevel=1;
   Assert.That(EarlyRetentionFirstUseTutorial.Vigilants(s),Is.EqualTo(FirstUseTutorialPhase.Locked));
   s.ChapterProgress.SeenNarrative.Add("vigilants-introduction");
   Assert.That(EarlyRetentionFirstUseTutorial.Vigilants(s),Is.EqualTo(FirstUseTutorialPhase.FirstAction));
   s.Retention.LastClearedStage=1;
   Assert.That(EarlyRetentionFirstUseTutorial.Vigilants(s),Is.EqualTo(FirstUseTutorialPhase.Learned));
  }
  [Test]public void ScoutsFirstUseGuideTracksRealOrdersAndReports(){
   var s=Ready(2).Value;
   Assert.That(EarlyRetentionFirstUseTutorial.Explorers(s),Is.EqualTo(FirstUseTutorialPhase.Locked));
   s.ChapterProgress.MarchConfirmed=true;
   Assert.That(EarlyRetentionFirstUseTutorial.Explorers(s),Is.EqualTo(FirstUseTutorialPhase.Locked));
   s.BastionLevel=3;
   Assert.That(EarlyRetentionFirstUseTutorial.Explorers(s),Is.EqualTo(FirstUseTutorialPhase.Locked));
   s.ChapterProgress.SeenNarrative.Add("scouts-introduction");
   Assert.That(EarlyRetentionFirstUseTutorial.Explorers(s),Is.EqualTo(FirstUseTutorialPhase.FirstAction));
   s.Retention.Orders.Add(new ScoutOrder{Id="real-order",StartedUtcTicks=1,EndsUtcTicks=2});
   Assert.That(EarlyRetentionFirstUseTutorial.Explorers(s),Is.EqualTo(FirstUseTutorialPhase.AwaitingResult));
   s.Retention.TotalScoutReports=1;
   Assert.That(EarlyRetentionFirstUseTutorial.Explorers(s),Is.EqualTo(FirstUseTutorialPhase.Learned));
  }
  [Test]public void ContextHelpDoesNotIntroduceLyraBeforeTheMeeting(){
   Assert.That(UnityContextTutorial.ForObjective("b3.lyra",false),Does.Not.Contain("Lyra"));
   Assert.That(UnityContextTutorial.ForObjective("b3.lyra",true),Does.Not.Contain("Lyra"));
   Assert.That(UnityContextTutorial.ForObjective("b3.fissure",false),Does.Contain("Lyra"));
  }
  [Test]public void ContextHelpTeachesRealTrainingAndMarchActions(){
   Assert.That(UnityContextTutorial.ForObjective("b2.train-archers",false),Does.Contain("Cuartel"));
   Assert.That(UnityContextTutorial.ForObjective("b2.prepare-march",false),Does.Contain("no envía"));
   Assert.That(UnityContextTutorial.ForObjective("b2.raise-expedition-power",false),Does.Contain("no del Bastión"));
  }
  [Test]public void AutomaticNarrativeNeverInterruptsAnotherGameSurface(){
   Assert.That(NarrativeCoherenceRules.CanPresent(false,false,false,false,false),Is.True);
   Assert.That(NarrativeCoherenceRules.CanPresent(true,false,false,false,false),Is.False);
   Assert.That(NarrativeCoherenceRules.CanPresent(false,true,false,false,false),Is.False);
   Assert.That(NarrativeCoherenceRules.CanPresent(false,false,true,false,false),Is.False);
   Assert.That(NarrativeCoherenceRules.CanPresent(false,false,false,true,false),Is.False);
   Assert.That(NarrativeCoherenceRules.CanPresent(false,false,false,false,true),Is.False);
  }
  [Test]public void IncompleteNarrativeIsNotRecordedAndAcknowledgementIsIdempotent(){
   var clock=new Clock();var store=Ready(3);var gateway=new LocalGateway(clock,store);
   Assert.That(NarrativeCoherenceRules.NextUnseenBeat(gateway.Snapshot()),Is.EqualTo("frontier-open"));
   Assert.That(gateway.AcknowledgeNarrative("frontier-open"),Is.True);
   Assert.That(gateway.AcknowledgeNarrative("frontier-open"),Is.False);
   gateway=new LocalGateway(clock,store);
   Assert.That(gateway.Snapshot().ChapterProgress.SeenNarrative.FindAll(x=>x=="frontier-open").Count,Is.EqualTo(1));
  }

 }
}

