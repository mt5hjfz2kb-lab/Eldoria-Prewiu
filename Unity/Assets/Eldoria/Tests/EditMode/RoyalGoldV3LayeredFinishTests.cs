using Eldoria.UI.RoyalGold.V2;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Eldoria.Tests
{
    public sealed class RoyalGoldV3LayeredFinishTests
    {
        [TestCase(RoyalGoldV3LayeredFinish.Scene.Vigilants)]
        [TestCase(RoyalGoldV3LayeredFinish.Scene.Scouts)]
        [TestCase(RoyalGoldV3LayeredFinish.Scene.Training)]
        [TestCase(RoyalGoldV3LayeredFinish.Scene.March)]
        [TestCase(RoyalGoldV3LayeredFinish.Scene.Panel)]
        public void ForegroundCannotStealTouchOrCreateAdditionalCanvas(
            RoyalGoldV3LayeredFinish.Scene scene)
        {
            var parent=new GameObject("Royal accepted room",typeof(RectTransform));
            try
            {
                var rect=(RectTransform)parent.transform;
                var foreground=RoyalGoldV3LayeredFinish.Install(rect,scene,0);
                Assert.That(foreground,Is.Not.Null);
                Assert.That(foreground.raycastTarget,Is.False);
                Assert.That(parent.GetComponentsInChildren<Canvas>(true).Length,Is.Zero);
                Assert.That(parent.GetComponentsInChildren<EventSystem>(true).Length,Is.Zero);
                Assert.That(foreground.rectTransform.anchorMin,Is.EqualTo(Vector2.zero));
                Assert.That(foreground.rectTransform.anchorMax,Is.EqualTo(Vector2.one));
                foreground.SetScene(scene);
                foreground.SetVerticesDirty();
                Assert.That(foreground.gameObject.layer,Is.EqualTo(parent.layer));
            }
            finally
            {
                Object.DestroyImmediate(parent);
            }
        }
    }
}
