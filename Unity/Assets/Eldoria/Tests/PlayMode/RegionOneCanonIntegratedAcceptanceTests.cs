using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Eldoria.Tests
{
    /// <summary>
    /// End-to-end structural assertions in the playable integrated scene.
    /// No save writes, constructed fake targets or forced gameplay unlocks.
    /// This is not visual-approval or mobile hardware evidence.
    /// </summary>
    public sealed class RegionOneCanonIntegratedAcceptanceTests
    {
        static object Member(object component, string name)
        {
            Assert.That(component, Is.Not.Null);
            var t = component.GetType();
            var field = t.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (field != null) return field.GetValue(component);
            var property = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            Assert.That(property, Is.Not.Null, "Missing integrated member: " + name);
            return property.GetValue(component);
        }

        static object Call(object component, string method, params object[] args)
        {
            var m = component.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(x => x.Name == method && x.GetParameters().Length == args.Length);
            Assert.That(m, Is.Not.Null, "Missing strategy camera API: " + method);
            return m.Invoke(component, args);
        }

        static IEnumerator Boot(Action<MonoBehaviour, GameObject, Component, Camera> ready)
        {
            SceneManager.LoadScene("Valoria_City_World_UI");
            for (int i = 0; i < 240; i++)
            {
                yield return null;
                var host = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .FirstOrDefault(x => x != null && x.GetType().Name == "ValoriaIntegratedWorld");
                if (!host || !(bool)Member(host, "Ready")) continue;
                var world = Member(host, "WorldRoot") as GameObject;
                var camera = Member(host, "WorldCamera") as Camera;
                var navigation = Member(host, "WorldNavigation") as Component;
                if (!world || !camera || !navigation) continue;
                if (!world.transform.Find("Region 1 canonical continuous geography")) continue;
                ready(host, world, navigation, camera);
                yield break;
            }
            Assert.Fail("Canonical adapter did not attach to the actual integrated scene");
        }

        [UnityTest]
        public IEnumerator OneGeographyAndNoInventedGameplayHotspots()
        {
            MonoBehaviour host = null; GameObject world = null; Component navigation = null; Camera camera = null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            var geo = world.transform.Find("Region 1 canonical continuous geography");
            Assert.That(geo, Is.Not.Null);
            Assert.That(geo.GetComponentsInChildren<MeshFilter>(true)
                .Count(f => f && f.name == "Region1 · one coherent terrain mesh"), Is.EqualTo(1));
            Assert.That(geo.GetComponentsInChildren<Collider>(true), Is.Empty,
                "Art layer must never steal taps or replace gameplay colliders");
            Assert.That(geo.GetComponentsInChildren<MonoBehaviour>(true)
                .Any(x=>x && x.GetType().Name == "WorldHotspot"), Is.False,
                "No decorative resource/city/enemy node may impersonate gameplay");
            var colliders = world.GetComponentsInChildren<MonoBehaviour>(true)
                .Where(x => x && x.GetType().Name == "WorldHotspot").ToArray();
            Assert.That(colliders.Length, Is.GreaterThanOrEqualTo(3));
            Assert.That(colliders.Any(x => (string)Member(x, "Id") == "forest-valoria"), Is.True);
            Assert.That(colliders.Any(x => (string)Member(x, "Id") == "quarry-valoria"), Is.True);
        }

        [UnityTest]
        public IEnumerator ThreeScalesUseSameWorldHomeWithoutCameraResetOrFakeBounds()
        {
            MonoBehaviour host = null; GameObject world = null; Component navigation = null; Camera camera = null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            Vector3 originalTarget = (Vector3)Member(navigation, "target");
            float originalZoom = camera.orthographicSize;
            var home = (Vector3)Member(host, "WorldHome");
            try
            {
                Call(navigation, "SetZoomLevel", 2, true);
                float near = camera.orthographicSize;
                Call(navigation, "SetZoomLevel", 1, true);
                float regional = camera.orthographicSize;
                Call(navigation, "SetZoomLevel", 0, true);
                float panorama = camera.orthographicSize;
                Assert.That(near, Is.GreaterThan(10));
                Assert.That(regional / near, Is.GreaterThan(3f));
                Assert.That(panorama / regional, Is.GreaterThan(2f));
                Assert.That((float)Member(navigation, "globalZoom"), Is.GreaterThan(200));
                Assert.That(camera.orthographic, Is.True);
                Call(navigation, "PanTo", home);
                Assert.That(Vector3.Distance((Vector3)Member(navigation, "target"),home),
                    Is.LessThan(2f), "Strategic home remains on the same geographical XYZ.");
                Assert.That(world.transform.Find("Region 1 canonical continuous geography"), Is.Not.Null);
            }
            finally
            {
                Call(navigation, "RestoreView", originalTarget, originalZoom);
            }
        }

        [UnityTest]
        public IEnumerator BreachIsGeologicalGroundGeometryNotTargetOrVerticalPortal()
        {
            MonoBehaviour host = null; GameObject world = null; Component navigation = null; Camera camera = null;
            yield return Boot((a,b,c,d)=>{host=a;world=b;navigation=c;camera=d;});
            var geo = world.transform.Find("Region 1 canonical continuous geography");
            var fracture = geo.Find("Region1 · grounded geologic Breach (not a portal)");
            Assert.That(fracture, Is.Not.Null);
            var filter = fracture.GetComponentInChildren<MeshFilter>(true);
            Assert.That(filter, Is.Not.Null);
            Assert.That(filter.sharedMesh.vertexCount, Is.GreaterThan(80));
            var b = filter.sharedMesh.bounds;
            Assert.That(b.size.y, Is.LessThan(8f), "Fracture is drawn along the ground, not a tower.");
            Assert.That(b.size.x, Is.GreaterThan(25f));
            Assert.That(b.size.z, Is.GreaterThan(25f));
            Assert.That(fracture.GetComponentsInChildren<Collider>(true), Is.Empty);
        }
    }
}
