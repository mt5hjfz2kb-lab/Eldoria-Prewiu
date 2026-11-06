using System.Reflection;
using Eldoria.PlayerClarity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.PlayerClarity.Tests
{
    /// <summary>
    /// Reproduces the exact native HUD failure: existing Image occupies the
    /// resource slot. The new Graphic must be on a child GameObject.
    /// </summary>
    public sealed class RoyalResourcePictogramInstallerTests
    {
        private GameObject canvas;
        private RectTransform bar;
        private static MethodInfo apply;

        [SetUp]
        public void SetUp()
        {
            canvas = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            var root = new GameObject("Resources", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            bar = (RectTransform)root.transform;
            apply = typeof(RoyalResourcePictogramInstaller).GetMethod("Apply",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(apply, Is.Not.Null);
        }

        [TearDown]
        public void TearDown()
        {
            if (canvas) Object.DestroyImmediate(canvas);
        }

        private bool Apply(string name, ResourcePictogramKind kind)
        {
            return (bool)apply.Invoke(null, new object[] {bar, name, kind});
        }

        private Image Slot(string name, bool active = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(bar, false);
            go.SetActive(active);
            return go.GetComponent<Image>();
        }

        [TestCase("Madera icon", ResourcePictogramKind.Wood)]
        [TestCase("Piedra icon", ResourcePictogramKind.Stone)]
        [TestCase("Comida icon", ResourcePictogramKind.Food)]
        public void ExistingImageDoesNotConflictWithNewGraphic(string slotName,
            ResourcePictogramKind kind)
        {
            var oldImage = Slot(slotName);
            Assert.That(Apply(slotName, kind), Is.True);
            Assert.That(oldImage.enabled, Is.False);
            Assert.That(oldImage.GetComponent<RoyalResourcePictogram>(), Is.Null,
                "Never attach two UI Graphics to the same original slot");
            var replacement = oldImage.transform.Find("Eldoria resource pictogram");
            Assert.That(replacement, Is.Not.Null);
            var graphic = replacement.GetComponent<RoyalResourcePictogram>();
            Assert.That(graphic, Is.Not.Null);
            Assert.That(graphic.Kind, Is.EqualTo(kind));
            Assert.That(graphic.raycastTarget, Is.False);
            var rect = (RectTransform)replacement;
            Assert.That(rect.anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(rect.anchorMax, Is.EqualTo(Vector2.one));
            Assert.That(rect.offsetMin, Is.EqualTo(Vector2.zero));
            Assert.That(rect.offsetMax, Is.EqualTo(Vector2.zero));
            // LateUpdate can rerun; no duplicate art or unexpected error.
            Assert.That(Apply(slotName, kind), Is.True);
            Assert.That(oldImage.GetComponentsInChildren<RoyalResourcePictogram>().Length,
                Is.EqualTo(1));
        }

        [Test]
        public void InactiveFoodDoesNotUnlockOrBecomeVisibleDuringInstallation()
        {
            var oldImage = Slot("Comida icon", false);
            Assert.That(Apply("Comida icon", ResourcePictogramKind.Food), Is.True);
            Assert.That(oldImage.gameObject.activeSelf, Is.False);
            var icon = oldImage.GetComponentInChildren<RoyalResourcePictogram>(true);
            Assert.That(icon, Is.Not.Null);
            Assert.That(icon.gameObject.activeInHierarchy, Is.False);
        }

        [Test]
        public void NoSlotDoesNotCreateFakeResource()
        {
            Assert.That(Apply("Madera icon", ResourcePictogramKind.Wood), Is.False);
            Assert.That(bar.childCount, Is.EqualTo(0));
        }
    }
}
