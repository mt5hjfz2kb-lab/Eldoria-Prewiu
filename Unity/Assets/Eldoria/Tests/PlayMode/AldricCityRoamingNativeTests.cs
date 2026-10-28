using System;
using System.Collections;
using Eldoria.AldricCity;
using Eldoria.ReferenceClean;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Eldoria.Tests
{
    /// <summary>
    /// Real native Unity integration proof, never a mockup or simulated screenshot.
    /// Requires approved GLB in StreamingAssets and disposable player state.
    /// </summary>
    public sealed class AldricCityRoamingNativeTests
    {
        [UnityTest]
        public IEnumerator ApprovedFullBodyAldricWalksOnActualValoriaPavingWithoutWritingNewGameState()
        {
            if (Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE") != "1")
                Assert.Ignore("Only run with isolated VALORIA_GAMEPLAY_PROBE=1 save");

            SceneManager.LoadScene("Valoria_City_World_UI");
            ValoriaIntegratedWorld host = null;
            for (int i = 0; i < 600; ++i)
            {
                yield return null;
                host = Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                if (host && host.Ready) break;
            }
            Assert.That(host, Is.Not.Null, "Canonical Valoria scene must be loaded");
            Assert.That(host.Ready, Is.True, "Canonical host is required; do not create a second one");
            var walker = host.GetComponent<AldricCityRoaming>();
            Assert.That(walker, Is.Not.Null, "One AldricCityRoaming must attach to existing host");
            for (int i = 0; i < 900 && !walker.ApprovedVisualReady; ++i)
                yield return null;
            Assert.That(walker.ApprovedVisualReady, Is.True,
                "Original full-body skinned GLB, source hash, road route and animations must pass");
            Assert.That(walker.RoadWaypoints, Is.GreaterThanOrEqualTo(3));
            Assert.That(walker.MeasuredModelHeight, Is.InRange(1.70f, 2.0f),
                "Aldric must use human city metres; not asset units or a UI portrait");
            Assert.That(walker.IsCityActorVisible, Is.True,
                "Approved visual should be present in the actual Valoria city");

            // Real time/real animation: initial idle pause is intentional.
            var first = walker.ActorGroundPosition;
            var travelled = 0f;
            for (int i = 0; i < 900 && travelled < 0.3f; ++i)
            {
                yield return null;
                travelled = Vector3.Distance(first, walker.ActorGroundPosition);
            }
            Assert.That(travelled, Is.GreaterThan(0.3f),
                "Aldric must actually walk on a valid road; a static render is not acceptance");
            Assert.That(host.City.Ready && host.Ready && !host.InWorld, Is.True);
            Assert.That(Object.FindObjectsByType<AldricCityRoaming>(FindObjectsSortMode.None).Length,
                Is.EqualTo(1), "Never instantiate a duplicate logical Aldric");
        }
    }
}
