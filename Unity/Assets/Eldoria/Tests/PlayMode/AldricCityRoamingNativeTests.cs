using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.Infrastructure;
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
        // The canonical host/walker belong to Assembly-CSharp. As with the
        // existing integrated UI tests, inspect their PUBLIC API by reflection
        // instead of making the PlayMode asmdef depend on a predefined assembly.
        private static T Read<T>(object target, string name)
        {
            Assert.That(target, Is.Not.Null, "Actual component for " + name + " missing");
            var type = target.GetType();
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (field != null) return (T)field.GetValue(target);
            var property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            Assert.That(property, Is.Not.Null, "Missing actual public " + type.Name + "." + name);
            return (T)property.GetValue(target);
        }
        private static object Call(object target, string name, params object[] args)
        {
            var method = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(x=>x.Name==name && x.GetParameters().Length==args.Length);
            Assert.That(method, Is.Not.Null, "Missing real " + name + " action");
            return method.Invoke(target, args);
        }
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
        private static void AssertActualBootGrounding(SkinnedMeshRenderer[] skins, object walker, float tolerance, string state)
        {
            float bottom = float.PositiveInfinity;
            foreach (var skin in skins)
            {
                var snapshot = new Mesh();
                try
                {
                    // true compensates Transform scale; convert actual posed
                    // vertices back through the live renderer Transform.
                    skin.BakeMesh(snapshot, true);
                    foreach (var vertex in snapshot.vertices)
                        bottom = Mathf.Min(bottom, skin.transform.TransformPoint(vertex).y);
                }
                finally { Object.Destroy(snapshot); }
            }
            var ground = Read<Vector3>(walker,"ActorGroundPosition").y;
            Debug.Log("ALDRIC_ACTUAL_SKIN_GROUND state=" + state + " bottom=" + bottom +
                " paving=" + ground + " offset=" + (bottom-ground));
            Assert.That(Mathf.Abs(bottom-ground), Is.LessThan(tolerance),
                "Actual posed boot vertices meet the real paving in " + state);
        }

        [UnityTest]
        public IEnumerator ApprovedFullBodyAldricWalksOnActualValoriaPaving()
        {
            if (Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE") != "1")
                Assert.Ignore("Only run with isolated VALORIA_GAMEPLAY_PROBE=1 save");

            SceneManager.LoadScene("Valoria_City_World_UI");
            Component host = null;
            for (var readyDeadline = Time.realtimeSinceStartup + 60f; Time.realtimeSinceStartup < readyDeadline;)
            {
                yield return null;
                host = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .FirstOrDefault(x=>x && x.GetType().FullName=="Eldoria.ReferenceClean.ValoriaIntegratedWorld");
                if (host && Read<bool>(host,"Ready")) break;
            }
            Assert.That(host, Is.Not.Null, "Canonical Valoria scene must be loaded");
            Assert.That(Read<bool>(host,"Ready"), Is.True, "Canonical host is required; do not create a second one");
            var city = Read<Component>(host,"City");
            Assert.That(Read<bool>(city,"Probe"), Is.True, "Never modify a real player save");
            var guidance = Read<Behaviour>(host,"Guidance");
            if (guidance) guidance.enabled = false;
            ICommandGateway Gateway()=>Read<ICommandGateway>(city,"Gateway");
            var fixture = Gateway().Snapshot();
            fixture.SawmillLevel = 1;
            fixture.BastionLevel = 2;
            fixture.BarracksLevel = 1;
            fixture.Available.ArcherT1 = 60;
            fixture.ChapterProgress.BastionTwoReached = true;
            fixture.ChapterProgress.MarchConfirmed = true;
            new FileStateStore(Read<string>(city,"SaveFile")).Save(fixture);
            Call(city,"ReloadForProbe");
            yield return null;
            var walker = host.GetComponent("AldricCityRoaming");
            Assert.That(walker, Is.Not.Null, "One AldricCityRoaming must attach to existing host");
            var importDeadline = Time.realtimeSinceStartup + 90f;
            while (!Read<bool>(walker,"ApprovedVisualReady") && Time.realtimeSinceStartup < importDeadline)
                yield return null;
            Assert.That(Read<bool>(walker,"ApprovedVisualReady"), Is.True,
                "Original full-body skinned GLB, source hash, road route and animations must pass");
            Assert.That(Read<string>(walker,"ImportedCompatibilitySha256"), Is.EqualTo(
                "d64f765625340365ad760e7e8d0639d640ebdd828d55c0635b3aefc0c974081b"),
                "Exact original geometry/UV/materials/rig/clips plus verified NORMAL streams only");
            Assert.That(Read<int>(walker,"RoadWaypoints"), Is.GreaterThanOrEqualTo(3));
            Assert.That(Read<float>(walker,"MeasuredModelHeight"), Is.InRange(1.70f, 2.0f),
                "Aldric must use human city metres; not asset units or a UI portrait");
            Assert.That(Read<bool>(walker,"IsCityActorVisible"), Is.True,
                "Approved visual should be present in the actual Valoria city");

            var skins = walker.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var bones = skins.SelectMany(x=>x.bones).Where(x=>x).Distinct().ToArray();
            Assert.That(bones.Length, Is.EqualTo(26), "The real imported visual retains all26approved bones");
            Assert.That(skins.SelectMany(x=>x.sharedMaterials).All(x=>x && x.shader &&
                x.shader.isSupported && !x.shader.name.Contains("InternalError")), Is.True,
                "Approved body/cape must use supported real materials, never the pink error shader");
            var camera = Read<Camera>(city,"PlayerCamera");
            Assert.That(camera, Is.Not.Null, "Use actual playable city camera");
            CaptureCityCamera(camera, "aldric-city-idle-landscape.png", 1280, 720);
            CaptureCityCamera(camera, "aldric-city-idle-portrait.png", 390, 844);
            AssertActualBootGrounding(skins, walker, .12f, "idle");
            foreach (var side in new[]{"L","R"})
            {
                var upper = bones.Single(x=>x.name=="Eldoria_Aldric_upperarm_"+side);
                var hand = bones.Single(x=>x.name=="Eldoria_Aldric_hand_"+side);
                var arm = hand.position-upper.position;
                Assert.That(-arm.y, Is.GreaterThan(Vector3.ProjectOnPlane(arm,Vector3.up).magnitude*1.05f),
                    "Actual city idle arm hangs naturally instead of retaining the source A-pose: " + side);
            }

            var beforeWalking = JsonUtility.ToJson(Gateway().Snapshot());
            // Real time/real animation: initial idle pause is intentional.
            var first = Read<Vector3>(walker,"ActorGroundPosition");
            var travelled = 0f;
            for (var walkDeadline = Time.realtimeSinceStartup + 20f; travelled < 0.3f && Time.realtimeSinceStartup < walkDeadline;)
            {
                yield return null;
                travelled = Vector3.Distance(first, Read<Vector3>(walker,"ActorGroundPosition"));
            }
            Assert.That(travelled, Is.GreaterThan(0.3f),
                "Aldric must actually walk on a valid road; a static render is not acceptance");
            Assert.That(Read<bool>(walker,"ActualWalkClipPlaying"), Is.True,
                "Real imported 26-joint Walk animation must play, not only translate a static model");
            var priorPose = bones.Select(x=>x.localRotation).ToArray();
            yield return new WaitForSecondsRealtime(.22f);
            Assert.That(bones.Where((bone,i)=>Quaternion.Angle(priorPose[i],bone.localRotation)>1f).Any(), Is.True,
                "The imported walk clip actually deforms approved bones across rendered frames");
            var cape = skins.FirstOrDefault(x=>x.name == "Cape_Main_ValoriaBlue_SeparateCloth_ForFutureRigging");
            var body = skins.FirstOrDefault(x=>x.name == "SirAldric_Tripo_Source_49800_VertexTopologyPreserved");
            Assert.That(cape && body, Is.True, "Inspect the actual approved body and separate blue cape");
            var travelDirection = Vector3.ProjectOnPlane(Read<Vector3>(walker,"CurrentWalkDestination") -
                Read<Vector3>(walker,"ActorGroundPosition"), Vector3.up).normalized;
            Assert.That(Vector3.Dot(Vector3.ProjectOnPlane(cape.bounds.center-body.bounds.center,Vector3.up),
                travelDirection), Is.LessThan(-.08f),
                "Approved blue cape is physically BEHIND the body in its actual direction of travel; no backwards walking");
            CaptureCityCamera(camera, "aldric-city-walking-landscape.png", 1280, 720);
            CaptureCityCamera(camera, "aldric-city-walking-portrait.png", 390, 844);
            AssertActualBootGrounding(skins, walker, .15f, "walking");
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
                    diagnostic.nearClipPlane = .05f;
                    diagnostic.enabled = false;
                    var centre = Read<Vector3>(walker,"ActorGroundPosition") + Vector3.up * .95f;
                    var facing = walker.transform.Find("Sir Aldric | city-only visual actor");
                    Assert.That(facing, Is.Not.Null);
                    // Four travel-relative angles expose actual front, cape and
                    // yaw after the independent cape/body alignment assertion.
                    var right = Vector3.Cross(Vector3.up, travelDirection).normalized;
                    var directions = new[]{travelDirection, right, -travelDirection, -right};
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
            Assert.That(JsonUtility.ToJson(Gateway().Snapshot()), Is.EqualTo(beforeWalking),
                "Cosmetic walking must not mutate authoritative player progress");

            // A physical obstruction added AFTER route validation must be detected.
            var obstacle = new GameObject("Native probe temporary body obstruction");
            obstacle.transform.position = Read<Vector3>(walker,"ActorGroundPosition") + Vector3.up * .68f;
            obstacle.AddComponent<BoxCollider>().size = new Vector3(.06f, .06f, .06f);
            Physics.SyncTransforms();
            try { Assert.That(Read<bool>(walker,"CurrentBodyClear"), Is.False,
                "An actual narrow collider between old point samples blocks the entire body capsule"); }
            finally { Object.Destroy(obstacle); }
            yield return null;
            Physics.SyncTransforms();
            Assert.That(Read<bool>(walker,"CurrentBodyClear"), Is.True, "Clearance recovers after removing the probe obstacle");

            var interruptedDestination = Read<Vector3>(walker,"CurrentWalkDestination");
            Assert.That((bool)Call(host,"TrySetWorld",true), Is.True, "Use real world navigation");
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(Read<bool>(walker,"IsCityActorVisible"), Is.False, "No duplicate Aldric remains visible in World");
            CaptureCityCamera(Read<Camera>(host,"WorldCamera"), "aldric-away-world.png", 1280, 720);
            Assert.That((bool)Call(host,"TrySetWorld",false), Is.True);
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(Read<bool>(walker,"IsCityActorVisible"), Is.True, "Aldric returns with the actual city camera");
            var resumedWaypoint = false;
            var nextRender = Time.realtimeSinceStartup;
            var nextDiagnostic = Time.realtimeSinceStartup;
            for (var resumeDeadline=Time.realtimeSinceStartup+15f; Time.realtimeSinceStartup<resumeDeadline;)
            {
                yield return null;
                // Batch-editor GameView rendering may stop after World. Wake
                // the genuine playable camera while observing its culled rig,
                // rather than requiring an invisible legacy loop to stop.
                if (Time.realtimeSinceStartup >= nextRender)
                { camera.Render(); nextRender = Time.realtimeSinceStartup+.2f; }
                var position = Read<Vector3>(walker,"ActorGroundPosition");
                if (Time.realtimeSinceStartup >= nextDiagnostic)
                {
                    Debug.Log("ALDRIC_WORLD_RESUME position="+position+" expected="+interruptedDestination+
                        " destination="+Read<Vector3>(walker,"CurrentWalkDestination")+
                        " moving="+Read<bool>(walker,"IsWalking")+
                        " walk_weight="+Read<float>(walker,"WalkClipWeight")+
                        " idle_weight="+Read<float>(walker,"IdleClipWeight")+
                        " time_scale="+Time.timeScale+" scaled_time="+Time.time+
                        " pause="+Read<float>(walker,"PauseSecondsRemaining")+
                        " body_clear="+Read<bool>(walker,"CurrentBodyClear")+
                        " leg_clear="+Read<bool>(walker,"CurrentLegClear"));
                    nextDiagnostic = Time.realtimeSinceStartup+1f;
                }
                if(Vector3.Distance(position,interruptedDestination)<.05f &&
                    !Read<bool>(walker,"IsWalking") && Read<bool>(walker,"ActualIdleClipPlaying") &&
                    !Read<bool>(walker,"ActualWalkClipPlaying")){resumedWaypoint=true;break;}
            }
            if (!resumedWaypoint) CaptureCityCamera(camera,"aldric-diagnostic-resume-failure.png",1280,720);
            Assert.That(resumedWaypoint, Is.True,
                "Returning from World resumes the interrupted leg and visibly pauses in Idle at its genuine destination");

            var state = Gateway().Snapshot();
            var sent = Gateway().Execute(new GameCommand(Guid.NewGuid().ToString("N"),
                state.PlayerId, "Gather", "forest-valoria", state.Revision));
            Assert.That(sent.Ok, Is.True, sent.Message);
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(Read<bool>(walker,"IsCityActorVisible"), Is.False, "Aldric really leaves the city with his march");
            CaptureCityCamera(camera, "aldric-away-march.png", 1280, 720);
            var clock = Read<IClock>(city,"Clock");
            Assert.That(clock, Is.Not.Null, "Advance only the disposable test clock");
            Assert.That(clock.GetType().Name, Is.EqualTo("TestClock"), "Only the actual disposable probe clock may advance");
            Call(clock,"Step",SliceRules.TravelSeconds * 2 + SliceRules.GatherSeconds + 2);
            Gateway().Advance();
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(Gateway().Snapshot().March.Phase, Is.EqualTo("idle"));
            Assert.That(Read<bool>(walker,"IsCityActorVisible"), Is.True, "The returning march restores Aldric in Valoria");

            // Prepare only the disposable chapter-III fixture; initiate the
            // actual domain investigation command, never fake a visual flag.
            state = Gateway().Snapshot();
            state.BastionLevel = 3;
            state.ChapterThree.FissureClosed = false;
            state.ChapterThree.FissureExplored = false;
            new FileStateStore(Read<string>(city,"SaveFile")).Save(state);
            Call(city,"ReloadForProbe");
            yield return null;
            state = Gateway().Snapshot();
            var investigated = Gateway().Execute(new GameCommand(Guid.NewGuid().ToString("N"),
                state.PlayerId, "InvestigateFissure", "fissure-valoria", state.Revision));
            Assert.That(investigated.Ok, Is.True, investigated.Message);
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(Read<bool>(walker,"IsCityActorVisible"), Is.False, "Investigation hides the city actor");
            CaptureCityCamera(camera, "aldric-away-investigation.png", 1280, 720);
            Call(city,"ReloadForProbe");
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(Read<bool>(walker,"IsCityActorVisible"), Is.False, "Reload preserves the actual ongoing investigation");
            Call(clock,"Step",20 * 60 + 1);
            Gateway().Advance();
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(Gateway().Snapshot().ChapterThree.FissureExplored, Is.True);
            Assert.That(Read<bool>(walker,"IsCityActorVisible"), Is.True, "Aldric returns after the genuine investigation completes");
            CaptureCityCamera(camera, "aldric-city-returned.png", 1280, 720);
            var returnedPosition = Read<Vector3>(walker,"ActorGroundPosition");
            var movedAfterReturn = 0f;
            for(var returnDeadline=Time.realtimeSinceStartup+15f;
                movedAfterReturn<.3f && Time.realtimeSinceStartup<returnDeadline;)
            {
                yield return null;
                movedAfterReturn=Vector3.Distance(returnedPosition,Read<Vector3>(walker,"ActorGroundPosition"));
            }
            Assert.That(movedAfterReturn, Is.GreaterThan(.3f),
                "After the actual investigation and reload Aldric resumes walking, not a stranded idle actor");
            Assert.That(Read<bool>(city,"Ready") && Read<bool>(host,"Ready") && !Read<bool>(host,"InWorld"), Is.True);
            Assert.That(Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .Count(x=>x && x.GetType().FullName=="Eldoria.AldricCity.AldricCityRoaming"),
                Is.EqualTo(1), "Never instantiate a duplicate logical Aldric");
        }
    }
}
