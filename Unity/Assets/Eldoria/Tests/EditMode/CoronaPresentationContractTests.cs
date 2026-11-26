using Eldoria.UI.RoyalGold;
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

        [Test]
        public void ExistingModalGetsSharedFontsWithoutChangingItsActionsOrCopy()
        {
            var root=new GameObject("Existing modal",typeof(RectTransform));
            root.SetActive(false);
            try
            {
                var modal=root.AddComponent<ModalWindow>();
                RoyalText Label(string name,string value)
                {
                    var g=new GameObject(name,typeof(RectTransform),typeof(RoyalText));
                    g.transform.SetParent(root.transform,false);
                    var text=g.GetComponent<RoyalText>();text.Put(value);return text;
                }
                RoyalButton Action(string name)
                {
                    var g=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(RoyalButton));
                    g.transform.SetParent(root.transform,false);
                    var button=g.GetComponent<RoyalButton>();button.targetGraphic=g.GetComponent<Image>();return button;
                }
                modal.Title=Label("Title","AJUSTES");
                modal.Body=Label("Body","Idioma / Language");
                modal.ExtraLabel=Label("Action","Cambiar a inglés");
                modal.SecondaryLabel=Label("Confirm","CONFIRM RESET");
                modal.Close=Action("Close");
                modal.Extra=Action("Extra");
                modal.Secondary=Action("Secondary");
                modal.Secondary.interactable=false;
                modal.Secondary.gameObject.SetActive(false);
                var originalTarget=modal.Extra.targetGraphic;
                var originalRect=modal.Body.rectTransform.sizeDelta;
                int invoked=0;
                modal.Extra.onClick.AddListener(()=>invoked++);
                CoronaFinish.ApplyModalTypography(modal);
                Assert.AreSame(CoronaFinish.HeadingFont,modal.Title.font);
                Assert.AreSame(CoronaFinish.BodyFont,modal.Body.font);
                Assert.AreSame(CoronaFinish.BodyFont,modal.ExtraLabel.font);
                Assert.AreSame(CoronaFinish.BodyFont,modal.SecondaryLabel.font);
                Assert.AreEqual("Idioma / Language",modal.Body.text);
                Assert.AreEqual("CONFIRM RESET",modal.SecondaryLabel.text);
                Assert.AreEqual(originalRect,modal.Body.rectTransform.sizeDelta);
                Assert.AreSame(originalTarget,modal.Extra.targetGraphic);
                Assert.IsFalse(modal.Secondary.interactable);
                Assert.IsFalse(modal.Secondary.gameObject.activeSelf);
                modal.Extra.onClick.Invoke();
                Assert.AreEqual(1,invoked);
            }
            finally {Object.DestroyImmediate(root);}
        }

        [TestCase(90f,128f,35f)]
        [TestCase(140f,140f,35f)]
        [TestCase(210f,210f,50f)]
        public void InventoryVisualBandsDoNotOverlap(float width,float height,float actionHeight)
        {
            CoronaFinish.InventoryBands(width,height,actionHeight,out Rect icon,out Rect caption,out Rect action);
            Assert.That(icon.yMax,Is.LessThanOrEqualTo(caption.yMin-2f));
            Assert.That(caption.yMax,Is.LessThanOrEqualTo(action.yMin-3f));
            Assert.That(action.yMax,Is.LessThanOrEqualTo(height));
            Assert.That(icon.xMin,Is.GreaterThanOrEqualTo(0f));
            Assert.That(icon.xMax,Is.LessThanOrEqualTo(width));
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
