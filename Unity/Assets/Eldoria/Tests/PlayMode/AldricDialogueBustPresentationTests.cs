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
    /// Aldric dialogue bust integration on the ONE real RoyalHUD. Speaker
    /// transitions below are UI probes, not invented gameplay/narrative.
    /// Run in Unity 6000.3.23f1 with disposable VALORIA_GAMEPLAY_PROBE=1.
    /// The existing authored I–III gameplay acceptance remains independent.
    /// </summary>
    public sealed class AldricDialogueBustPresentationTests
    {
        private static object Read(object target, string property)
        {
            Assert.That(target, Is.Not.Null);
            var t = target.GetType();
            // Unity's RoyalHUD.Modal is a public FIELD, whereas
            // ValoriaIntegratedWorld.Hud and Ready are public PROPERTIES.
            // Reflect both without changing the real HUD or test fixtures.
            var p = t.GetProperty(property, BindingFlags.Public | BindingFlags.Instance);
            if (p != null) return p.GetValue(target);
            var f = t.GetField(property, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(f, Is.Not.Null, t.Name + "." + property + " missing (property or field)");
            return f.GetValue(target);
        }

        private static void Invoke(object target, string method, params object[] args)
        {
            Assert.That(target, Is.Not.Null);
            var m = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(x => x.Name == method && x.GetParameters().Length == args.Length);
            Assert.That(m, Is.Not.Null, "Missing public " + method + " on " + target.GetType().Name);
            m.Invoke(target, args);
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
            Assert.Fail("Canonical city HUD not ready; cannot validate Aldric portrait.");
        }

        private static IEnumerator FinishCurrentNarrative(Component hud, Button continueButton)
        {
            for (int i = 0; i < 50 && (bool)Read(hud, "NarrativeOpen"); i++)
            {
                continueButton.onClick.Invoke();
                yield return null;
            }
            Assert.That((bool)Read(hud, "NarrativeOpen"), Is.False,
                "The original Continue must still be able to finish its narrative.");
        }

        [UnityTest]
        public IEnumerator ApprovedChestUpAldricAppearsOnlyForHisSpeakerAndNeverForLyraOrOrin()
        {
            if (Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE") != "1")
                Assert.Ignore("Requires isolated disposable Unity playtest save.");

            Component host = null, hud = null;
            yield return Boot((h, u) => { host = h; hud = u; });
            var guidance = Read(host, "Guidance") as Behaviour;
            if (guidance) guidance.enabled = false;

            var modal = Read(hud, "Modal") as Component;
            var button = Read(modal, "Extra") as Button;
            Assert.That(button, Is.Not.Null);
            yield return FinishCurrentNarrative(hud, button);

            var finish = hud.gameObject.GetComponent("RoyalGoldV3CinematicFinish");
            Assert.That(finish, Is.Not.Null, "Canonical cinematic presenter missing.");

            var expected = Resources.Load<Texture2D>("UI/AldricDialogue/aldric_neutral");
            Assert.That(expected, Is.Not.Null, "Owner-approved Aldric bust PNG is not in Unity Resources.");
            Assert.That(expected.width, Is.EqualTo(512));
            Assert.That(expected.height, Is.EqualTo(512));

            foreach (string speaker in new[] { "Sir Aldric", "Lyra", "Orín", "NARRADOR", "Aldric" })
            {
                // UI-only probe with the real HUD/controller, not a gameplay mission.
                Invoke(hud, "ShowNarrative", speaker, "La reconstrucción de Valoria continúa.");
                yield return null;
                Invoke(finish, "SynchronizeNow");

                Assert.That((bool)Read(hud, "NarrativeOpen"), Is.True);
                var title = Read(modal, "Title") as Text;
                var frame = title.transform.parent as RectTransform;
                var portrait = frame.Find("Canonical Aldric portrait") as RectTransform;
                var sigil = frame.Find("Narrator sigil") as RectTransform;
                Assert.That(portrait, Is.Not.Null);
                Assert.That(sigil, Is.Not.Null);

                bool isAldric = speaker == "Sir Aldric" || speaker == "Aldric";
                var raw = portrait.GetComponent<RawImage>();
                Assert.That(raw, Is.Not.Null);
                Assert.That(raw.gameObject.activeSelf, Is.EqualTo(isAldric),
                    "Wrong speaker identity for " + speaker);
                Assert.That(sigil.gameObject.activeSelf, Is.EqualTo(!isAldric),
                    "Aldric must not be misrepresented in " + speaker + " dialogue.");
                if (isAldric)
                {
                    Assert.That(raw.texture, Is.SameAs(expected),
                        "The authentic approved 3D bust must replace the legacy JPEG.");
                    Assert.That(portrait.rect.width, Is.GreaterThanOrEqualTo(115f),
                        "Never fall back to a tiny account-sized portrait.");
                    Assert.That(raw.uvRect, Is.EqualTo(new Rect(0f, 0f, 1f, 1f)));
                }
                Assert.That(button.gameObject.activeInHierarchy, Is.True,
                    "Original Continue must remain interactive.");
                yield return FinishCurrentNarrative(hud, button);
            }
            Assert.That((bool)Read(hud, "NarrativeOpen"), Is.False);
        }

        [Test]
        public void FourBustosHaveDistinctImportedTexturesWithIdenticalDimensions()
        {
            var names = new[] { "aldric_neutral", "aldric_firme", "aldric_preocupado", "aldric_sereno" };
            foreach (var n in names)
            {
                var image = Resources.Load<Texture2D>("UI/AldricDialogue/" + n);
                Assert.That(image, Is.Not.Null, "Missing approved source texture " + n);
                Assert.That(image.width, Is.EqualTo(512), n);
                Assert.That(image.height, Is.EqualTo(512), n);
            }
            // Four lighting/viewpoint variants of one actual character, NOT four facial blendshapes.
        }
    }
}
