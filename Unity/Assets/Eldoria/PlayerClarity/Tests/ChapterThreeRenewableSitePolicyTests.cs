using System.Collections.Generic;
using Eldoria.PlayerClarity;
using NUnit.Framework;
namespace Eldoria.PlayerClarity.Tests
{
    public sealed class ChapterThreeRenewableSitePolicyTests
    {
        [TestCase("hunt-wolf")]
        [TestCase("hunt-boar")]
        [TestCase("meat-valoria")]
        public void FirstGenerationPreservesExistingGeography(string id)
        {
            for(int i=0;i<ChapterThreeRenewableSitePolicy.SiteCount;i++)
            {
                Assert.AreEqual(i,ChapterThreeRenewableSitePolicy.CandidateIndex(id,0,i));
                Assert.That(ChapterThreeRenewableSitePolicy.RadiusAt(i),Is.InRange(72,168));
                Assert.That(ChapterThreeRenewableSitePolicy.BearingAt(i),Is.InRange(0,23));
            }
        }
        [TestCase("hunt-wolf")]
        [TestCase("hunt-boar")]
        [TestCase("meat-valoria")]
        public void NewGenerationTraversesAllValidCandidateIndicesWithoutRepetition(string id)
        {
            for(int generation=1;generation<=4;generation++)
            {
                var visited=new HashSet<int>();
                for(int i=0;i<ChapterThreeRenewableSitePolicy.SiteCount;i++)
                    Assert.IsTrue(visited.Add(
                        ChapterThreeRenewableSitePolicy.CandidateIndex(id,generation,i)));
                Assert.AreEqual(ChapterThreeRenewableSitePolicy.SiteCount,visited.Count);
            }
        }
        [Test]
        public void StoryTargetNeverMovesAndActiveMarchPreventsTeleport()
        {
            Assert.IsFalse(ChapterThreeRenewableSitePolicy.CanRelocate("fissure-valoria",0,1,false));
            Assert.IsFalse(ChapterThreeRenewableSitePolicy.CanRelocate("ash-stalker-valoria",0,1,false));
            Assert.IsFalse(ChapterThreeRenewableSitePolicy.CanRelocate("hunt-boar",0,1,true));
            Assert.IsFalse(ChapterThreeRenewableSitePolicy.CanRelocate("hunt-boar",1,1,false));
            Assert.IsTrue(ChapterThreeRenewableSitePolicy.CanRelocate("hunt-boar",0,1,false));
        }
    }
}
