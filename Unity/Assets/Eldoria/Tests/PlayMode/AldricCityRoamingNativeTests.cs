using System;
using System.Collections;
using System.IO;
using System.Linq;
using Eldoria.AldricCity;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;
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
        // Real Unity GPU render, not a composited mockup. Overlay HUD is not
        // included: this proof is strictly the 3D character in Valoria.
        private static void CaptureCityCamera(Camera camera, string filename, int width, int height)
        {
            var directory = Environment.GetEnvironmentVariable("ALDRIC_CITY_SCREENSHOT_DIR");
            if (string.IsNullOrEmpty(directory) || !camera) return;
            Directory.CreateDirectory(directory);
            var target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            var previous = camera.targetTexture;
            var active = RenderTexture.active;
            Texture2D image = null;
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image = new Texture2D(width, height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                image.Apply(false);
                var pixels = image.GetPixels32();
                int darkest = 255, brightest = 0;
                for (int i = 0; i < pixels.Length; i += 97)
                {
                    int value = (pixels[i].r + pixels[i].g + pixels[i].b) / 3;
                    darkest = Math.Min(darkest, value);
                    brightest = Math.Max(brightest, value);
                }
                Assert.That(brightest - darkest, Is.GreaterThan(40),
                    "Actual city/world camera must produce nonempty native scene pixels: " + filename);
                File.WriteAllBytes(Path.Combine(directory, filename), image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previous;
                RenderTexture.active = active;
                RenderTexture.ReleaseTemporary(target);
                if (image) Object.Destroy(image);
            }
        }
        [UnityTest]
        public IEnumerator ApprovedFullBodyAldricWalksOnActualValoriaPaving()
        {
            if (Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE") != "1")
                Assert.Ignore("Only run with isolated VALORIA_GAMEPLAY_PROBE=1 save");

            SceneManager.LoadScene("Valoria_City_World_UI");
            ValoriaIntegratedWorld host = null;
            for (var readyDeadline = Time.realtimeSinceStartup + 60f; Time.realtimeSinceStartup < readyDeadline;)
            {
                yield return null;
                host = Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                if (host && host.Ready) break;
            }
            Assert.That(host, Is.Not.Null, "Canonical Valoria scene must be loaded");
            Assert.That(host.Ready, Is.True, "Canonical host is required; do not create a second one");
            Assert.That(host.City.Probe, Is.True, "Never modify a real player save");
            if (host.Guidance) host.Guidance.enabled = false;
            var fixture = host.City.Gateway.Snapshot();
            fixture.SawmillLevel = 1;
            fixture.BastionLevel = 2;
            fixture.BarracksLevel = 1;
            fixture.Available.ArcherT1 = 60;
            fixture.ChapterProgress.BastionTwoReached = true;
            fixture.ChapterProgress.MarchConfirmed = true;
            new FileStateStore(host.City.SaveFile).Save(fixture);
            host.City.ReloadForProbe();
            yield return null;
            var walker = host.GetComponent<AldricCityRoaming>();
            Assert.That(walker, Is.Not.Null, "One AldricCityRoaming must attach to existing host");
            var importDeadline = Time.realtimeSinceStartup + 90f;
            while (!walker.ApprovedVisualReady && Time.realtimeSinceStartup < importDeadline)
                yield return null;
            Assert.That(walker.ApprovedVisualReady, Is.True,
                "Original full-body skinned GLB, source hash, road route and animations must pass");
            Assert.That(walker.RoadWaypoints, Is.GreaterThanOrEqualTo(3));
            Assert.That(walker.MeasuredModelHeight, Is.InRange(1.70f, 2.0f),
                "Aldric must use human city metres; not asset units or a UI portrait");
            Assert.That(walker.IsCityActorVisible, Is.True,
                "Approved visual should be present in the actual Valoria city");

            var skins = walker.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var bones = skins.SelectMany(x=>x.bones).Where(x=>x).Distinct().ToArray();
            Assert.That(bones.Length, Is.EqualTo(26), "The real imported visual retains all26approved bones");
            Assert.That(skins.SelectMany(x=>x.sharedMaterials).All(x=>x && x.shader &&
                x.shader.isSupported && !x.shader.name.Contains("InternalError")), Is.True,
                "Approved body/cape must use supported real materials, never the pink error shader");
            var camera = host.City.PlayerCamera;
            Assert.That(camera, Is.Not.Null, "Use actual playable city camera");
            CaptureCityCamera(camera, "aldric-city-idle-landscape.png", 1280, 720);
            CaptureCityCamera(camera, "aldric-city-idle-portrait.png", 390, 844);

            var beforeWalking = JsonUtility.ToJson(host.City.Gateway.Snapshot());
            // Real time/real animation: initial idle pause is intentional.
            var first = walker.ActorGroundPosition;
            var travelled = 0f;
            for (var walkDeadline = Time.realtimeSinceStartup + 20f; travelled < 0.3f && Time.realtimeSinceStartup < walkDeadline;)
            {
                yield return null;
                travelled = Vector3.Distance(first, walker.ActorGroundPosition);
            }
            Assert.That(travelled, Is.GreaterThan(0.3f),
                "Aldric must actually walk on a valid road; a static render is not acceptance");
            Assert.That(walker.ActualWalkClipPlaying, Is.True,
                "Real imported 26-joint Walk animation must play, not only translate a static model");
            var priorPose = bones.Select(x=>x.localRotation).ToArray();
            yield return new WaitForSecondsRealtime(.22f);
            Assert.That(bones.Where((bone,i)=>Quaternion.Angle(priorPose[i],bone.localRotation)>1f).Any(), Is.True,
                "The imported walk clip actually deforms approved bones across rendered frames");
            CaptureCityCamera(camera, "aldric-city-walking-landscape.png", 1280, 720);
            CaptureCityCamera(camera, "aldric-city-walking-portrait.png", 390, 844);
            // Additional diagnostic front/back views retain the exact imported
            // actor and lighting; the four acceptance views above remain the
            // genuine playable camera. Never alter the production camera.
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ALDRIC_CITY_SCREENSHOT_DIR")))
            {
                var diagnosticObject = new GameObject("Probe-only Aldric four-angle detail camera");
                var diagnostic = diagnosticObject.AddComponent<Camera>();
                try
                {
                    diagnostic.CopyFrom(camera);
                    diagnostic.orthographic = true;
                    diagnostic.orthographicSize = 1.35f;
                    diagnostic.enabled = false;
                    var centre = walker.ActorGroundPosition + Vector3.up * .95f;
                    var facing = walker.transform.Find("Sir Aldric | city-only visual actor");
                    Assert.That(facing, Is.Not.Null);
                    // Four actor-relative angles expose face, cape and yaw
                    // without assuming the imported mesh's front axis.
                    var directions = new[]{facing.forward, facing.right, -facing.forward, -facing.right};
                    var names = new[]{"forward", "right", "backward", "left"};
                    for (int view=0; view<directions.Length; view++)
                    {
                        diagnostic.transform.position = centre + directions[view] * 4f + Vector3.up * .5f;
                        diagnostic.transform.LookAt(centre);
                        CaptureCityCamera(diagnostic, "aldric-detail-"+names[view]+".png", 720, 900);
                    }
                }
                finally { Object.Destroy(diagnosticObject); }
            }
            Assert.That(JsonUtility.ToJson(host.City.Gateway.Snapshot()), Is.EqualTo(beforeWalking),
                "Cosmetic walking must not mutate authoritative player progress");

            // A physical obstruction added AFTER route validation must be detected.
            var obstacle = new GameObject("Native probe temporary body obstruction");
            obstacle.transform.position = walker.ActorGroundPosition + Vector3.up * .68f;
            obstacle.AddComponent<BoxCollider>().size = new Vector3(.06f, .06f, .06f);
            Physics.SyncTransforms();
            try { Assert.That(walker.CurrentBodyClear, Is.False,
                "An actual narrow collider between old point samples blocks the entire body capsule"); }
            finally { Object.Destroy(obstacle); }
            yield return null;
            Physics.SyncTransforms();
            Assert.That(walker.CurrentBodyClear, Is.True, "Clearance recovers after removing the probe obstacle");

            Assert.That(host.TrySetWorld(true), Is.True, "Use real world navigation");
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(walker.IsCityActorVisible, Is.False, "No duplicate Aldric remains visible in World");
            CaptureCityCamera(host.WorldCamera, "aldric-away-world.png", 1280, 720);
            Assert.That(host.TrySetWorld(false), Is.True);
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(walker.IsCityActorVisible, Is.True, "Aldric returns with the actual city camera");

            var state = host.City.Gateway.Snapshot();
            var sent = host.City.Gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),
                state.PlayerId, "Gather", "forest-valoria", state.Revision));
            Assert.That(sent.Ok, Is.True, sent.Message);
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(walker.IsCityActorVisible, Is.False, "Aldric really leaves the city with his march");
            CaptureCityCamera(camera, "aldric-away-march.png", 1280, 720);
            var clock = host.City.Clock as ValoriaReferenceCleanGameplay.TestClock;
            Assert.That(clock, Is.Not.Null, "Advance only the disposable test clock");
            clock.Step(SliceRules.TravelSeconds * 2 + SliceRules.GatherSeconds + 2);
            host.City.Gateway.Advance();
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(host.City.Gateway.Snapshot().March.Phase, Is.EqualTo("idle"));
            Assert.That(walker.IsCityActorVisible, Is.True, "The returning march restores Aldric in Valoria");

            // Prepare only the disposable chapter-III fixture; initiate the
            // actual domain investigation command, never fake a visual flag.
            state = host.City.Gateway.Snapshot();
            state.BastionLevel = 3;
            state.ChapterThree.FissureClosed = false;
            state.ChapterThree.FissureExplored = false;
            new FileStateStore(host.City.SaveFile).Save(state);
            host.City.ReloadForProbe();
            yield return null;
            state = host.City.Gateway.Snapshot();
            var investigated = host.City.Gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),
                state.PlayerId, "InvestigateFissure", "fissure-valoria", state.Revision));
            Assert.That(investigated.Ok, Is.True, investigated.Message);
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(walker.IsCityActorVisible, Is.False, "Investigation hides the city actor");
            CaptureCityCamera(camera, "aldric-away-investigation.png", 1280, 720);
            host.City.ReloadForProbe();
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(walker.IsCityActorVisible, Is.False, "Reload preserves the actual ongoing investigation");
            clock.Step(20 * 60 + 1);
            host.City.Gateway.Advance();
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(host.City.Gateway.Snapshot().ChapterThree.FissureExplored, Is.True);
            Assert.That(walker.IsCityActorVisible, Is.True, "Aldric returns after the genuine investigation completes");
            CaptureCityCamera(camera, "aldric-city-returned.png", 1280, 720);
            Assert.That(host.City.Ready && host.Ready && !host.InWorld, Is.True);
            Assert.That(Object.FindObjectsByType<AldricCityRoaming>(FindObjectsSortMode.None).Length,
                Is.EqualTo(1), "Never instantiate a duplicate logical Aldric");
        }
    }
}
