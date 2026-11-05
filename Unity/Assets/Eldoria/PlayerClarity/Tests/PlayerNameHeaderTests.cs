using Eldoria.PlayerClarity;
using Eldoria.UI.RoyalGold;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.PlayerClarity.Tests
{
    public sealed class PlayerNameHeaderTests
    {
        GameObject canvas;
        [SetUp]
        public void Before()
        {
            canvas=new GameObject("UI",typeof(RectTransform),typeof(Canvas));
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        }
        [TearDown]
        public void After()
        {
            if(canvas)Object.DestroyImmediate(canvas);
        }

        private RoyalText Label(string label,Transform parent)
        {
            var go=new GameObject(label,typeof(RectTransform),typeof(RoyalText));
            go.transform.SetParent(parent,false);
            return go.GetComponent<RoyalText>();
        }

        [Test]
        public void RealmRetainsKingdomAndPowerWhileShowingPlayerName()
        {
            var root=new GameObject("Realm header",typeof(RectTransform),typeof(RealmHeader));
            root.transform.SetParent(canvas.transform,false);
            var header=root.GetComponent<RealmHeader>();
            header.Realm=Label("Realm",root.transform);
            header.Power=Label("Power",root.transform);
            header.Realm.Put("Valoria");
            header.Power.Put("Poder 123");
            header.Power.rectTransform.anchoredPosition=new Vector2(102,-57);

            string saved="";
            var adapter=root.AddComponent<PlayerNameHeader>();
            adapter.Initialize(header,()=>saved);
            Assert.IsTrue(header.Realm.gameObject.activeSelf);
            saved="Sir Alvar";
            adapter.Refresh();
            Assert.IsFalse(header.Realm.gameObject.activeSelf);
            Assert.AreEqual("Poder 123",header.Power.text);
            Assert.IsNotNull(root.transform.Find("Kingdom"));
            Assert.AreEqual("Valoria",root.transform.Find("Kingdom").GetComponent<RoyalText>().text);
            Assert.AreEqual("Sir Alvar",
                root.transform.Find("Character name").GetComponent<RoyalText>().text);
            saved="";
            adapter.Refresh();
            Assert.IsTrue(header.Realm.gameObject.activeSelf);
            Assert.AreEqual(new Vector2(102,-57),header.Power.rectTransform.anchoredPosition);
        }
    }
}
