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
            if (guidance) guidance.enabled = false;

            Assert.That(hud.gameObject.GetComponent("RoyalGoldV3CinematicFinish"),
                Is.Not.Null, "The actual one RoyalHUD must own the cinematic finish");

            var modal = Read(hud, "Modal") as Component;
            Assert.That(modal, Is.Not.Null);
            var title = Read(modal, "Title") as Text;
            var body = Read(modal, "Body") as Text;
            var button = Read(modal, "Extra") as Button;
            Assert.That(button, Is.Not.Null);
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

            // Use the *actual* original dialogue; no duplicate controller.
            // The prologue begins with a narrator turn before Sir Aldric.
            for (int turn = 0; turn < 16; turn++)
            {
                if (!(bool)Read(hud, "NarrativeOpen")) break;
                if (string.Equals((string)Read(hud, "NarrativeSpeaker"),
                    "Sir Aldric", StringComparison.OrdinalIgnoreCase)) break;
                button.onClick.Invoke(); // reveal
                button.onClick.Invoke(); // next canonical speaker/page
                yield return null;
            }
            if (!(bool)Read(hud, "NarrativeOpen"))
                Call(hud, "ShowNarrative", "Sir Aldric",
                    "Valoria sobrevivió a la noche. La Brecha sigue abierta. " +
                    "No dejaremos que destruya de nuevo aquello que hemos jurado proteger.");
            yield return null;
            yield return null;

            Assert.That((bool)Read(hud, "NarrativeOpen"), Is.True);
            var paper = frame.Find("V3 / cinematic royal parchment") as RectTransform;
            Assert.That(paper, Is.Not.Null);
            Assert.That(paper.gameObject.activeInHierarchy, Is.True);
            Assert.That(paper.GetComponent<Graphic>().raycastTarget, Is.False);
            Assert.That(original.enabled, Is.False, "Only the real parchment must be drawn");
            Assert.That(body.color.r, Is.LessThan(.6f),
                "Dialogue 1 must be sepia ink on paper, not white on navy");
            Assert.That(frame.anchorMin.y, Is.EqualTo(0f).Within(.001f),
                "Cinematic dialogue must sit below the city, not in the centre");
            Assert.That(frame.rect.height, Is.LessThanOrEqualTo(440f),
                "Dialogue must leave most of the city visible");

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
        }
    }
}
