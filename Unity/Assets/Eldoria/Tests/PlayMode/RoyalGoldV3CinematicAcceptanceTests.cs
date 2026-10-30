using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Eldoria.Tests
{
    /// <summary>
    /// Owner's cinematic Dialogue 1 acceptance on the ONE integrated game HUD.
    /// A local isolated probe is used; this suite never needs a player's save.
    /// Compile-time decoupled from the UI assembly like the existing I-II suite.
    /// </summary>
    public sealed class RoyalGoldV3CinematicAcceptanceTests
    {
        private static object Read(object target, string name)
        {
            Assert.That(target, Is.Not.Null);
            var type = target.GetType();
            var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (property != null) return property.GetValue(target);
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null, type.Name + "." + name + " missing");
            return field.GetValue(target);
        }

        private static void Call(object target, string method, params object[] args)
        {
            var match = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == method && m.GetParameters().Length == args.Length);
            Assert.That(match, Is.Not.Null, method + " missing");
            match.Invoke(target, args);
        }

        private static IEnumerator Boot(Action<Component, Component> ready)
        {
            SceneManager.LoadScene("Valoria_City_World_UI");
            for (int i = 0; i < 180; i++)
            {
                yield return null;
                var host = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .FirstOrDefault(x => x && x.GetType().Name == "ValoriaIntegratedWorld");
                if (!host || !(bool)Read(host, "Ready")) continue;
                var hud = Read(host, "Hud") as Component;
                if (!hud) continue;
                ready(host, hud);
                yield break;
            }
            Assert.Fail("The integrated city and its canonical HUD did not boot");
        }

        [UnityTest]
        public IEnumerator CinematicManuscriptRetainsCanonicalButtonsAndRestoresRegularModal()
        {
            if (Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE") != "1")
                Assert.Ignore("Requires disposable native probe save");

            Component host = null, hud = null;
            yield return Boot((h, u) => { host = h; hud = u; });
            var guidance = Read(host, "Guidance") as Behaviour;
            if (guidance)
            {
                guidance.enabled = false;
                // Ready precedes the guidance LateUpdate in a freshly loaded
                // scene. Start its real authored prologue explicitly, exactly
                // as the existing native integration probe does.
                Call(guidance, "PresentMilestone");
            }

            Assert.That(hud.gameObject.GetComponent("RoyalGoldV3CinematicFinish"),
                Is.Not.Null, "The actual one RoyalHUD must own the cinematic finish");

            var modal = Read(hud, "Modal") as Component;
            Assert.That(modal, Is.Not.Null);
            var title = Read(modal, "Title") as Text;
            var body = Read(modal, "Body") as Text;
            var button = Read(modal, "Extra") as Button;
            Assert.That(button, Is.Not.Null);
            string canonicalAldricText = null;
            // Boot can already be inside the genuine prologue. Finish it
            // before taking the NORMAL modal baseline; a cinematic snapshot
            // cannot be the expected appearance of an ordinary menu.
            for (int turn = 0; turn < 40 && (bool)Read(hud, "NarrativeOpen"); turn++)
            {
                if (string.Equals((string)Read(hud, "NarrativeSpeaker"),
                    "Sir Aldric", StringComparison.OrdinalIgnoreCase))
                    canonicalAldricText = (string)Read(hud, "NarrativeSourceText");
                button.onClick.Invoke();
                yield return null;
            }
            Assert.That((bool)Read(hud, "NarrativeOpen"), Is.False);
            Assert.That(canonicalAldricText, Is.Not.Null.And.Not.Empty,
                "Use the genuine existing Aldric introduction, never invented product text");
            yield return null;
            var frame = title.transform.parent as RectTransform;
            var original = frame.GetComponent<Graphic>();
            Assert.That(original, Is.Not.Null, "Original reusable modal frame missing");
            var beforeAnchorMin = frame.anchorMin;
            var beforeAnchorMax = frame.anchorMax;
            var beforePivot = frame.pivot;
            var beforePosition = frame.anchoredPosition;
            var beforeSize = frame.sizeDelta;
            var beforeTitleColor = title.color;
            var beforeBodyColor = body.color;
            var beforeTitleSize = title.fontSize;
            var beforeBodySize = body.fontSize;
            var extraLabel = Read(modal, "ExtraLabel") as Text;
            Assert.That(extraLabel, Is.Not.Null);
            var beforeLabelPosition = extraLabel.rectTransform.anchoredPosition;
            var beforeLabelSize = extraLabel.rectTransform.sizeDelta;

            // The same genuine dialogue and original controller/button.
            Call(hud, "ShowNarrative", "Sir Aldric", canonicalAldricText);
            yield return null;
            yield return null;

            Assert.That((bool)Read(hud, "NarrativeOpen"), Is.True);
            // Transform.Find treats '/' as a hierarchy path, even when a
            // single child object's literal name contains those characters.
            var paper = frame.Cast<Transform>().SingleOrDefault(x =>
                x.name == "V3 / cinematic royal parchment") as RectTransform;
            Assert.That(paper, Is.Not.Null);
            Assert.That(paper.gameObject.activeInHierarchy, Is.True);
            Assert.That(paper.GetComponent<Graphic>().raycastTarget, Is.False);
            Assert.That(original.enabled, Is.False, "Only the real parchment must be drawn");
            Assert.That(body.color.r, Is.LessThan(.6f),
                "Dialogue 1 must be sepia ink on paper, not white on navy");
            Assert.That(frame.anchorMin.y, Is.EqualTo(0f).Within(.001f),
                "Cinematic dialogue must sit below the city, not in the centre");
            Assert.That(frame.rect.height, Is.LessThanOrEqualTo(600f),
                "Dialogue must use a bounded cinematic strip rather than cover the world");
            Assert.That(body.preferredHeight, Is.LessThanOrEqualTo(
                body.rectTransform.rect.height + 12f),
                "The current typewriter prefix must fit");
            // Before Reveal has completed, measure the real entire page.
            // A typewriter-only assertion used to pass while later sentences
            // were clipped or forced a sudden oversized modal jump.
            var unrevealed = body.text;
            var fullPage = Read(hud, "NarrativePageText") as string;
            Assert.That(fullPage, Is.Not.Null.And.Not.Empty);
            body.text = fullPage;
            float fullPageHeight = body.preferredHeight;
            body.text = unrevealed;
            Assert.That(fullPageHeight, Is.LessThanOrEqualTo(
                body.rectTransform.rect.height + 8f),
                "All dialogue lines must fit before typewriter reveal on the real canvas");

            var portrait = frame.Find("Canonical Aldric portrait") as RectTransform;
            Assert.That(portrait, Is.Not.Null);
            if (string.Equals((string)Read(hud, "NarrativeSpeaker"),
                "Sir Aldric", StringComparison.OrdinalIgnoreCase))
            {
                Assert.That(portrait.gameObject.activeSelf, Is.True);
                Assert.That(portrait.rect.width, Is.GreaterThanOrEqualTo(115f),
                    "Do not retain the old tiny 98px icon as the story portrait");
            }
            Assert.That(body.rectTransform.rect.width, Is.GreaterThan(170f));
            Assert.That(button.gameObject.activeInHierarchy, Is.True);
            Assert.That(button.interactable, Is.True);
            Assert.That(extraLabel.transform.parent, Is.SameAs(button.transform));
            var labelCorners = new Vector3[4];
            extraLabel.rectTransform.GetWorldCorners(labelCorners);
            var buttonRect = (RectTransform)button.transform;
            foreach (var corner in labelCorners)
                Assert.That(buttonRect.rect.Contains(buttonRect.InverseTransformPoint(corner)),
                    Is.True, "The real Continue label must remain fully inside its real button");
            button.onClick.Invoke(); // reveal the full canonical page
            yield return null;
            Assert.That(body.preferredHeight, Is.LessThanOrEqualTo(body.rectTransform.rect.height),
                "The genuine full Aldric text must fit, not merely its first typed characters");

            // Click the real button, never a fake narrative acknowledgement.
            for (int i = 0; i < 40 && (bool)Read(hud, "NarrativeOpen"); i++)
            {
                button.onClick.Invoke();
                yield return null;
            }
            Assert.That((bool)Read(hud, "NarrativeOpen"), Is.False,
                "All original pages must still advance and complete");
            yield return null;
            Assert.That(paper.gameObject.activeSelf, Is.False,
                "Cinematic parchment must disappear for ordinary gameplay");
            Assert.That(original.enabled, Is.True,
                "Original frame must be restored for ordinary non-story modals");
            Assert.That(frame.anchorMin, Is.EqualTo(beforeAnchorMin));
            Assert.That(frame.anchorMax, Is.EqualTo(beforeAnchorMax));
            Assert.That(frame.pivot, Is.EqualTo(beforePivot));
            Assert.That(frame.anchoredPosition, Is.EqualTo(beforePosition));
            Assert.That(frame.sizeDelta, Is.EqualTo(beforeSize));
            Assert.That(title.color, Is.EqualTo(beforeTitleColor));
            Assert.That(body.color, Is.EqualTo(beforeBodyColor));
            Assert.That(title.fontSize, Is.EqualTo(beforeTitleSize));
            Assert.That(body.fontSize, Is.EqualTo(beforeBodySize));
            Assert.That(extraLabel.rectTransform.anchoredPosition, Is.EqualTo(beforeLabelPosition));
            Assert.That(extraLabel.rectTransform.sizeDelta, Is.EqualTo(beforeLabelSize));
        }
    }
}

