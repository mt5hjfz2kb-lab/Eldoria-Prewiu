using Eldoria.PlayerClarity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.PlayerClarity.Tests
{
    public sealed class RoyalPlayerNamePromptTests
    {
        private GameObject canvas;

        [SetUp]
        public void Setup()
        {
            canvas = new GameObject("Name prompt test canvas",
                typeof(RectTransform),typeof(Canvas));
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        }

        [TearDown]
        public void Cleanup()
        {
            if (canvas) Object.DestroyImmediate(canvas);
        }

        [Test]
        public void InvalidNameCannotCloseFirstEntryOrInvokeSave()
        {
            var prompt=RoyalPlayerNamePrompt.Create((RectTransform)canvas.transform);
            int writes=0;
            prompt.Open(false,name=>{writes++;return true;});
            var field=prompt.GetComponentInChildren<InputField>(true);
            Assert.That(field,Is.Not.Null);
            field.text="AB";
            prompt.Confirm();
            Assert.IsTrue(prompt.IsOpen);
            Assert.AreEqual(0,writes);
        }

        [Test]
        public void ValidNameClosesOnlyAfterAuthoritativeSaveReturnsSuccess()
        {
            var prompt=RoyalPlayerNamePrompt.Create((RectTransform)canvas.transform);
            string saved="";
            bool allow=false;
            prompt.Open(false,name=>{if (!allow) return false;saved=name;return true;});
            var field=prompt.GetComponentInChildren<InputField>(true);
            field.text="  Áurea_27  ";
            prompt.Confirm();
            Assert.IsTrue(prompt.IsOpen,"Save failure must not silently lose the nickname");
            allow=true;
            prompt.Confirm();
            Assert.IsFalse(prompt.IsOpen);
            Assert.AreEqual("Áurea_27",saved);
        }

        [Test]
        public void PromptUsesActualInputFieldAndRealButtonsNotAnImageMockup()
        {
            var prompt=RoyalPlayerNamePrompt.Create((RectTransform)canvas.transform);
            Assert.IsNotNull(prompt.GetComponentInChildren<InputField>(true));
            Assert.IsNotNull(prompt.GetComponentInChildren<Button>(true));
            prompt.Open(false, _=>true);
            Assert.IsTrue(prompt.IsOpen);
            Assert.That(prompt.GetComponent<Image>().raycastTarget,Is.True,
                "Onboarding must block accidental map touches until name is saved");
        }
    }
}
