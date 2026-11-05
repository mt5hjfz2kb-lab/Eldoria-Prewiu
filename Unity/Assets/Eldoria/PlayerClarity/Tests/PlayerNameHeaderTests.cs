using Eldoria.PlayerClarity;
using Eldoria.UI.RoyalGold;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.PlayerClarity.Tests
{
    public sealed class PlayerNameHeaderTests
    {
        private GameObject canvas;
        [SetUp]
        public void Before()
        {
            canvas = new GameObject("UI", typeof(RectTransform), typeof(Canvas));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        }

        [TearDown]
        public void After()
        {
            if (canvas) Object.DestroyImmediate(canvas);
        }

        private RoyalText Label(string name, Transform parent, Vector2 position)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(RoyalText));
            obj.transform.SetParent(parent, false);
            var label = obj.GetComponent<RoyalText>();
            label.rectTransform.anchorMin = label.rectTransform.anchorMax =
                label.rectTransform.pivot = new Vector2(0f, 1f);
            label.rectTransform.anchoredPosition = position;
            label.rectTransform.sizeDelta = new Vector2(212, name == "Realm" ? 34 : 28);
            return label;
        }

        [Test]
        public void NicknameUsesOriginalSingleHeaderLineWithoutMovingOrHidingPower()
        {
            var go = new GameObject("Realm header", typeof(RectTransform), typeof(RealmHeader));
            go.transform.SetParent(canvas.transform, false);
            var realm = go.GetComponent<RealmHeader>();
            realm.Realm = Label("Realm", go.transform, new Vector2(102, -18));
            realm.Power = Label("Power", go.transform, new Vector2(102, -57));
            realm.Realm.Put("Valoria");
            realm.Power.Put("Poder 123");
            var realmPosition = realm.Realm.rectTransform.anchoredPosition;
            var powerPosition = realm.Power.rectTransform.anchoredPosition;
            var children = go.transform.childCount;

            string selected = "";
            var display = go.AddComponent<PlayerNameHeader>();
            display.Initialize(realm, () => selected);
            Assert.That(realm.Realm.text, Is.EqualTo("Valoria"));
            Assert.That(realm.Power.text, Is.EqualTo("Poder 123"));

            selected = "Sir Alvar";
            display.Refresh();
            Assert.That(realm.Realm.text, Is.EqualTo(selected));
            Assert.That(realm.Power.text, Is.EqualTo("Poder 123"));
            Assert.That(realm.Realm.gameObject.activeSelf, Is.True);
            Assert.That(realm.Power.gameObject.activeSelf, Is.True);
            Assert.That(realm.Realm.rectTransform.anchoredPosition, Is.EqualTo(realmPosition));
            Assert.That(realm.Power.rectTransform.anchoredPosition, Is.EqualTo(powerPosition));
            Assert.That(go.transform.childCount, Is.EqualTo(children),
                "A third Valoria/name line must never be inserted over the resource bar");

            // Simulate the canonical RoyalHUD refresh, which renders "Valoria"
            // at 0.25-second intervals; the nameplate must restore the nickname.
            realm.Realm.Put("Valoria");
            display.Refresh();
            Assert.That(realm.Realm.text, Is.EqualTo("Sir Alvar"));

            selected = "WWWWWWWWWWWWWWWWWW";
            display.Refresh();
            Assert.That(realm.Realm.text, Is.EqualTo(selected));
            Assert.That(realm.Realm.resizeTextForBestFit, Is.True,
                "Long saved nicknames must fit inside the existing single-line slot");
            Assert.That(go.transform.childCount, Is.EqualTo(children));

            selected = "";
            display.Refresh();
            Assert.That(realm.Realm.text, Is.EqualTo("Valoria"));
            Assert.That(realm.Power.rectTransform.anchoredPosition, Is.EqualTo(powerPosition));
        }
    }
}
