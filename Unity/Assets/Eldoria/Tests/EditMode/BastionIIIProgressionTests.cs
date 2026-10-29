using System;
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
  [Test]public void Code16SaveAscendsOnceAfterFourteenSeconds(){var c=new Clock();var st=Ready(2);st.Value.ChapterThree=null;var g=new LocalGateway(c,st);int wood=g.Snapshot().Resources.Wood;Assert.That(Do(g,"AdvanceBastion","bastion","same").Ok);Assert.That(Do(g,"AdvanceBastion","bastion","same").Ok);Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(wood-650));c.Add(13);g.Advance();Assert.That(g.Snapshot().BastionLevel,Is.EqualTo(2));c.Add(1);g=new LocalGateway(c,st);Assert.That(g.Snapshot().BastionLevel,Is.EqualTo(3));Assert.That(g.Snapshot().Resources.Food,Is.EqualTo(400));}
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
   Assert.That(g.Snapshot().Resources.Stone,Is.EqualTo(stone+560));
   Assert.That(Do(g,"Fight","fissure-valoria").Ok,Is.False);
   g=new LocalGateway(c,st);
   Assert.That(g.Snapshot().ChapterThree.Inventory.FindAll(x=>x=="breach-manuscript").Count,Is.EqualTo(1));
   Assert.That(g.Snapshot().Resources.Stone,Is.EqualTo(stone+560));
  }
  [Test]public void ConstructionAndFoodAccrueAtActualOfflineDeadline(){var c=new Clock();var st=Ready();var g=new LocalGateway(c,st);Assert.That(Do(g,"Build","granary").Ok);int food=g.Snapshot().Resources.Food;c.Add(19);g=new LocalGateway(c,st);Assert.That(g.Snapshot().ChapterThree.GranaryLevel,Is.EqualTo(1));Assert.That(g.Snapshot().Resources.Food,Is.EqualTo(food+10));Assert.That(Do(g,"UpgradeBuilding","granary").Ok);c.Add(20);g=new LocalGateway(c,st);Assert.That(g.Snapshot().ChapterThree.GranaryLevel,Is.EqualTo(2));Assert.That(g.Snapshot().Resources.Food,Is.EqualTo(food+40));Assert.That(Do(g,"UpgradeBuilding","granary").Ok,Is.False);}
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
   c.Add(20);g=new LocalGateway(c,st);
   Assert.That(g.Snapshot().Available.ArcherT1,Is.EqualTo(initialT1));
   Assert.That(g.Snapshot().Available.ArcherT2,Is.EqualTo(5));
   Assert.That(g.Snapshot().PendingRecruitTier,Is.EqualTo(1));
   g=new LocalGateway(c,st);
   Assert.That(g.Snapshot().Available.ArcherT2,Is.EqualTo(5));
  }
  [Test]public void SnapshotDoesNotExposeMutableInventory(){var c=new Clock();var st=Ready();st.Value.ChapterThree.Inventory.Add("treated-hide");var g=new LocalGateway(c,st);g.Snapshot().ChapterThree.Inventory.Clear();Assert.That(g.Snapshot().ChapterThree.Inventory,Does.Contain("treated-hide"));}
  [Test]public void FullMeatReserveAndRespawnSurviveOneLongOfflineInterval(){var c=new Clock();var st=Ready();var g=new LocalGateway(c,st);int food=g.Snapshot().Resources.Food;Assert.That(Do(g,"Gather","meat-valoria").Ok);c.Add(60);g=new LocalGateway(c,st);Assert.That(g.Snapshot().March.Phase,Is.EqualTo("idle"));Assert.That(g.Snapshot().Resources.Food,Is.EqualTo(food+1100));Assert.That(g.Snapshot().ChapterThree.MeatRemaining,Is.EqualTo(1100));Assert.That(g.Snapshot().ChapterThree.MeatGeneration,Is.EqualTo(1));g=new LocalGateway(c,st);Assert.That(g.Snapshot().Resources.Food,Is.EqualTo(food+1100));Assert.That(g.Snapshot().ChapterThree.MeatGeneration,Is.EqualTo(1));}
  [Test]public void OfflineProductionCapsAtOneHourAcrossPendingUpgrade(){var c=new Clock();var st=Ready();st.Value.ChapterThree.GranaryLevel=1;var g=new LocalGateway(c,st);int food=g.Snapshot().Resources.Food;Assert.That(Do(g,"UpgradeBuilding","granary").Ok);c.Add(7200);g=new LocalGateway(c,st);Assert.That(g.Snapshot().ChapterThree.GranaryLevel,Is.EqualTo(2));Assert.That(g.Snapshot().Resources.Food-food,Is.EqualTo(7200));}

 }
}
