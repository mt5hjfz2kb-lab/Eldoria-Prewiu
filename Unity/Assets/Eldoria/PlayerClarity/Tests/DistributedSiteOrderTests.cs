using System;
using System.Collections.Generic;
using Eldoria.PlayerClarity;
using NUnit.Framework;

namespace Eldoria.PlayerClarity.Tests
{
    public sealed class DistributedSiteOrderTests
    {
        [TestCase(1)]
        [TestCase(7)]
        [TestCase(16)]
        [TestCase(97)]
        [TestCase(512)]
        [TestCase(1003)]
        public void VisitsEverySiteOnceEvenForCompositeCount(int count)
        {
            var seen = new HashSet<int>();
            for (int i = 0; i < count; i++)
            {
                var value = DistributedSiteOrder.Candidate(count,"region1:wood:007",5,i);
                Assert.That(value, Is.InRange(0,count-1));
                Assert.That(seen.Add(value),Is.True,"Duplicate candidate at attempt "+i);
            }
            Assert.That(seen.Count,Is.EqualTo(count));
        }

        [Test]
        public void SameIdentityAndGenerationAreStableAndIndependentOfClock()
        {
            const int n = 499;
            var actual = new List<int>();
            var repeated = new List<int>();
            for (int i = 0; i < n; i++)
            {
                actual.Add(DistributedSiteOrder.Candidate(n,"region1:boar:009",3,i));
                repeated.Add(DistributedSiteOrder.Candidate(n,"region1:boar:009",3,i));
            }
            CollectionAssert.AreEqual(actual,repeated);
        }

        [Test]
        public void MultipleEntitiesAndGenerationsDoNotShareOneFixedWalk()
        {
            const int n = 509;
            var a = new int[8];var b = new int[8];var c = new int[8];
            for (int i = 0; i < 8; i++)
            {
                a[i] = DistributedSiteOrder.Candidate(n,"region1:stone:003",1,i);
                b[i] = DistributedSiteOrder.Candidate(n,"region1:stone:003",2,i);
                c[i] = DistributedSiteOrder.Candidate(n,"region1:stone:019",1,i);
            }
            Assert.That(string.Join(",",a),Is.Not.EqualTo(string.Join(",",b)));
            Assert.That(string.Join(",",a),Is.Not.EqualTo(string.Join(",",c)));
        }

        [Test]
        public void RejectsInvalidRequests()
        {
            Assert.Throws<ArgumentOutOfRangeException>(()=>DistributedSiteOrder.Candidate(0,"x",1,0));
            Assert.Throws<ArgumentException>(()=>DistributedSiteOrder.Candidate(4,"",1,0));
            Assert.Throws<ArgumentOutOfRangeException>(()=>DistributedSiteOrder.Candidate(4,"x",-1,0));
            Assert.Throws<ArgumentOutOfRangeException>(()=>DistributedSiteOrder.Candidate(4,"x",1,4));
        }
    }
}
