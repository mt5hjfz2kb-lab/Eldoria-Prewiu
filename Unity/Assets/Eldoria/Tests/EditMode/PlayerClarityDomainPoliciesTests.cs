using System;
using System.Collections.Generic;
using Eldoria.Domain;
using NUnit.Framework;

namespace Eldoria.Tests
{
    public sealed class PlayerClarityDomainPoliciesTests
    {
        [TestCase("  Áurea   2  ", "Áurea 2")]
        [TestCase("Sir_Aldric", "Sir_Aldric")]
        [TestCase("Valoria-01", "Valoria-01")]
        public void DisplayNameIsStableAndUnicodeAware(string raw, string expected)
        {
            Assert.IsTrue(PlayerIdentityPolicy.TryNormalize(raw,out var name,out var error),error);
            Assert.AreEqual(expected,name);
        }

        [TestCase("")]
        [TestCase("X")]
        [TestCase("012345678901234567890")]
        [TestCase("A\nB")]
        [TestCase("<malicious>")]
        public void InvalidNamesCannotBeCommitted(string raw)
        {
            Assert.IsFalse(PlayerIdentityPolicy.TryNormalize(raw,out var name,out var error));
            Assert.AreEqual("",name);
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void TiersHaveIncreasingActualExtractionCapacity()
        {
            Assert.AreEqual(1250, ResourceNodeTierPolicy.Capacity("wood",1));
            Assert.AreEqual(5200, ResourceNodeTierPolicy.Capacity("stone",1));
            for(int tier=2;tier<=ResourceNodeTierPolicy.MaxLevel;tier++)
            {
                Assert.Greater(ResourceNodeTierPolicy.Capacity("wood",tier),
                    ResourceNodeTierPolicy.Capacity("wood",tier-1));
                Assert.Greater(ResourceNodeTierPolicy.Capacity("stone",tier),
                    ResourceNodeTierPolicy.Capacity("stone",tier-1));
            }
            Assert.AreEqual(1,ResourceNodeTierPolicy.ExistingSaveLevel("wood",1250));
            Assert.AreEqual(1,ResourceNodeTierPolicy.ExistingSaveLevel("stone",5200));
            Assert.AreEqual(4,ResourceNodeTierPolicy.ExistingSaveLevel("wood",
                ResourceNodeTierPolicy.Capacity("wood",4)));
        }

        [Test]
        public void EachLevelAppearsWithinNumerousVerifiedSites()
        {
            var hits = new int[ResourceNodeTierPolicy.MaxLevel+1];
            for(int i=1;i<=500;i++)
                hits[ResourceNodeTierPolicy.ForNewSite("region1:wood:"+i,
                    "region1:verified:"+((i*13)%127),i%7+1)]++;
            for(int tier=1;tier<hits.Length;tier++)
                Assert.Greater(hits[tier],0,"Tier "+tier+" never generated");
        }

        [Test]
        public void SameSiteAndGenerationReproduceSavedTier()
        {
            int a=ResourceNodeTierPolicy.ForNewSite("region1:stone:004","region1:23:31",3);
            int b=ResourceNodeTierPolicy.ForNewSite("region1:stone:004","region1:23:31",3);
            Assert.AreEqual(a,b);
            Assert.That(a,Is.InRange(1,5));
        }

        [Test]
        public void InvalidTiersAndResourceTypesNeverSilentlyProduceCapacity()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()=>ResourceNodeTierPolicy.Capacity("wood",0));
            Assert.Throws<ArgumentOutOfRangeException>(()=>ResourceNodeTierPolicy.Capacity("stone",6));
            Assert.Throws<ArgumentException>(()=>ResourceNodeTierPolicy.Capacity("food",1));
        }
    }
}
