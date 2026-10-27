using System;
using Eldoria.Application;
using Eldoria.Domain;
using NUnit.Framework;
namespace Eldoria.Tests {
 public sealed class BastionIIIProgressionTests {
  sealed class Clock:IClock {public long UtcTicks{get;set;}=DateTime.UtcNow.Ticks;public void Add(int seconds){UtcTicks+=TimeSpan.FromSeconds(seconds).Ticks;}}
  sealed class Store:IStateStore {public PlayerState Value;public PlayerState Load()=>Value;public void Save(PlayerState state){Value=state;}}
  string previous;
  [SetUp]public void Setup(){previous=SliceContentProfiles.ActiveRuntimeProfile;SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);}
  [TearDown]public void Cleanup(){SliceContentProfiles.SetRuntimeProfileOverride(previous);}
  static CommandResult Do(LocalGateway g,string kind,string target,string id=null){var s=g.Snapshot();return g.Execute(new GameCommand(id??Guid.NewGuid().ToString(),s.PlayerId,kind,target,s.Revision));}
  static Store Ready(int level=3)=>new Store{Value=new PlayerState{BastionLevel=level,SawmillLevel=1,BarracksLevel=1,JourneyComplete=true,EngendroDefeated=true,Available=new ArmyRoster{ArcherT1=56},Resources=new ResourceWallet{Wood=5000,Stone=5000}}};
  [Test]public void Code16SaveAscendsOnceAfterFourteenSeconds(){var c=new Clock();var st=Ready(2);st.Value.ChapterThree=null;var g=new LocalGateway(c,st);int wood=g.Snapshot().Resources.Wood;Assert.That(Do(g,"AdvanceBastion","bastion","same").Ok);Assert.That(Do(g,"AdvanceBastion","bastion","same").Ok);Assert.That(g.Snapshot().Resources.Wood,Is.EqualTo(wood-650));c.Add(13);g.Advance();Assert.That(g.Snapshot().BastionLevel,Is.EqualTo(2));c.Add(1);g=new LocalGateway(c,st);Assert.That(g.Snapshot().BastionLevel,Is.EqualTo(3));Assert.That(g.Snapshot().Resources.Food,Is.EqualTo(400));}
  [Test]public void InvestigationPersistsAndNeverRecruitsBeforeVictory(){var c=new Clock();var st=Ready();var g=new LocalGateway(c,st);Assert.That(Do(g,"InvestigateFissure","fissure-valoria").Ok);Assert.That(Do(g,"ContactLyra","fissure-valoria").Ok,Is.False);Assert.That(Do(g,"Fight","hunt-wolf").Ok,Is.False);c.Add(1199);g=new LocalGateway(c,st);Assert.That(g.Snapshot().ChapterThree.FissureExplored,Is.False);c.Add(1);g=new LocalGateway(c,st);Assert.That(Do(g,"ContactLyra","fissure-valoria").Ok);Assert.That(g.Snapshot().ChapterThree.LyraRecruited,Is.False);int stone=g.Snapshot().Resources.Stone;Assert.That(Do(g,"Fight","fissure-valoria").Ok);c.Add(4);g=new LocalGateway(c,st);Assert.That(g.Snapshot().ChapterThree.LyraRecruited);Assert.That(g.Snapshot().Resources.Stone,Is.EqualTo(stone+560));Assert.That(Do(g,"Fight","fissure-valoria").Ok,Is.False);g=new LocalGateway(c,st);Assert.That(g.Snapshot().ChapterThree.Inventory.FindAll(x=>x=="breach-manuscript").Count,Is.EqualTo(1));Assert.That(g.Snapshot().Resources.Stone,Is.EqualTo(stone+560));}
  [Test]public void ConstructionAndFoodAccrueAtActualOfflineDeadline(){var c=new Clock();var st=Ready();var g=new LocalGateway(c,st);Assert.That(Do(g,"Build","granary").Ok);int food=g.Snapshot().Resources.Food;c.Add(19);g=new LocalGateway(c,st);Assert.That(g.Snapshot().ChapterThree.GranaryLevel,Is.EqualTo(1));Assert.That(g.Snapshot().Resources.Food,Is.EqualTo(food+10));Assert.That(Do(g,"UpgradeBuilding","granary").Ok);c.Add(20);g=new LocalGateway(c,st);Assert.That(g.Snapshot().ChapterThree.GranaryLevel,Is.EqualTo(2));Assert.That(g.Snapshot().Resources.Food,Is.EqualTo(food+40));Assert.That(Do(g,"UpgradeBuilding","granary").Ok,Is.False);}
  [Test]public void SnapshotDoesNotExposeMutableInventory(){var c=new Clock();var st=Ready();st.Value.ChapterThree.Inventory.Add("treated-hide");var g=new LocalGateway(c,st);g.Snapshot().ChapterThree.Inventory.Clear();Assert.That(g.Snapshot().ChapterThree.Inventory,Does.Contain("treated-hide"));}
 }
}
