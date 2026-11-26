using Eldoria.UI.RoyalGold.V2;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Eldoria.Tests
{
    public class CoronaPresentationContractTests
    {
        [TestCase("wood")]
        [TestCase("stone")]
        [TestCase("food")]
        public void LooseResourcesUseTheCanonicalSprite(string kind)
        {
            var expected=Resources.Load<Sprite>("ValoriaModular25D/UI/CoronaBrecha/icon_"+kind);
            Assert.IsNotNull(expected);
            Assert.AreSame(expected,CoronaFinish.ResourceIcon(kind));
        }

        [Test] public void UnknownResourceDoesNotInventAnIcon()
        {
            Assert.IsNull(CoronaFinish.ResourceIcon("unknown"));
        }

        [Test] public void BothOriginalFontsAreAvailable()
        {
            Assert.IsNotNull(CoronaFinish.HeadingFont);
            Assert.IsNotNull(CoronaFinish.BodyFont);
            Assert.AreNotSame(CoronaFinish.HeadingFont,CoronaFinish.BodyFont);
        }

        [Test] public void ButtonFeedbackPreservesInteractionAndTarget()
        {
            var go=new GameObject("Contract",typeof(RectTransform),typeof(Image),typeof(Button));
            try {
                var button=go.GetComponent<Button>();
                button.targetGraphic=go.GetComponent<Image>();
                button.interactable=false;
                int called=0;
                button.onClick.AddListener(()=>called++);
                CoronaFinish.ApplyButtonFeedback(button);
                Assert.IsFalse(button.interactable);
                Assert.AreSame(go.GetComponent<Image>(),button.targetGraphic);
                Assert.AreEqual(Selectable.Transition.ColorTint,button.transition);
                Assert.AreNotEqual(button.colors.normalColor,button.colors.pressedColor);
                Assert.AreNotEqual(button.colors.normalColor,button.colors.disabledColor);
                button.onClick.Invoke();
                Assert.AreEqual(1,called);
            } finally { Object.DestroyImmediate(go); }
        }

        [TestCase("12.4K",64)]
        [TestCase("999",46)]
        [TestCase("MARCHAS",100)]
        public void SingleLinePreservesStringAndFitsGlyphs(string value,int width)
        {
            var go=new GameObject("Contract",typeof(RectTransform),typeof(Text));
            try {
                var label=go.GetComponent<Text>();
                label.font=CoronaFinish.BodyFont;
                Assert.IsNotNull(label.font);
                label.rectTransform.sizeDelta=new Vector2(width,32);
                label.text=value;
                CoronaFinish.FitSingleLine(label,20,10);
                Assert.AreEqual(value,label.text);
                Assert.That(label.fontSize,Is.InRange(10,20));
                Assert.That(label.preferredWidth,Is.LessThanOrEqualTo(width));
            } finally {Object.DestroyImmediate(go);}
        }
    }
}
