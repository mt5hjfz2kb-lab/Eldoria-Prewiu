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
    /// Actual integrated entry scene, HUD, file-backed gateway and UI controls.
    /// Tests run ONLY with the existing isolated VALORIA_GAMEPLAY_PROBE save;
    /// never reset, rename or alter a player's real save.
    /// Reflection avoids an Assembly-CSharp reference in the PlayMode asmdef.
    /// </summary>
    public sealed class PlayerClaritySceneTests
    {
        private static object Member(object obj, string name)
        {
            Assert.That(obj, Is.Not.Null, "Missing parent for " + name);
            var type = obj.GetType();
            var flags = BindingFlags.Public | BindingFlags.Instance;
            var property = type.GetProperty(name, flags);
            if (property != null) return property.GetValue(obj);
            var field = type.GetField(name, flags);
            Assert.That(field, Is.Not.Null, "Missing " + type.Name + "." + name);
            return field.GetValue(obj);
        }

        private static object Call(object obj, string name)
        {
            Assert.That(obj, Is.Not.Null);
            var method = obj.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.Instance,
                null, Type.EmptyTypes, null);
            Assert.That(method, Is.Not.Null, "Missing real gateway/presentation action " + name);
            return method.Invoke(obj, null);
        }

        [UnityTest]
        public IEnumerator FreshSaveShowsNameBeforeNarrativeAndReloadKeepsOriginalOwner()
        {
            if (Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE") != "1")
                Assert.Ignore("Only isolated disposable native QA saves may be renamed.");

            SceneManager.LoadScene("Valoria_City_World_UI");
            Component host = null, hud = null, city = null;
            for (int frame = 0; frame < 240; frame++)
            {
                yield return null;
                host = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .FirstOrDefault(x => x && x.GetType().Name == "ValoriaIntegratedWorld");
                if (!host || !(bool)Member(host, "Ready")) continue;
                hud = Member(host, "Hud") as Component;
                city = Member(host, "City") as Component;
                if (hud && city) break;
            }
            Assert.That(hud, Is.Not.Null, "The canonical RoyalHUD must load.");
            Assert.That(city, Is.Not.Null, "The original city gateway must load.");

            var originalGateway = Member(city, "Gateway");
            var first = Call(originalGateway, "Snapshot");
            var originalId = Member(first, "PlayerId") as string;
            Assert.That(Member(first, "PlayerName"), Is.EqualTo(""));
            Assert.That(Member(hud, "NarrativeOpen"), Is.EqualTo(false),
                "Aldric's introduction must never appear before choosing the name.");

            var prompt = hud.GetComponentsInChildren<MonoBehaviour>(true)
                .FirstOrDefault(x => x && x.GetType().Name == "RoyalPlayerNamePrompt");
            Assert.That(prompt, Is.Not.Null, "A fresh session needs the real parchment input.");
            Assert.That(prompt.gameObject.activeInHierarchy, Is.True);
            Assert.That(Member(prompt, "IsOpen"), Is.EqualTo(true));
            var editor = prompt.GetComponentInChildren<InputField>(true);
            Assert.That(editor, Is.Not.Null, "The nickname must use an editable mobile input.");
            editor.text = "Lucía_07";
            Call(prompt, "Confirm");

            var second = Call(Member(city, "Gateway"), "Snapshot");
            Assert.That(Member(second, "PlayerName"), Is.EqualTo("Lucía_07"));
            Assert.That(Member(second, "PlayerId"), Is.EqualTo(originalId),
                "A cosmetic nickname may never change the persisted save identity.");
            Assert.That(prompt.gameObject.activeSelf, Is.False);
            yield return null;
            Assert.That(hud.GetComponentsInChildren<Text>(true).Any(t =>
                    t && t.gameObject.activeInHierarchy && t.text == "Lucía_07"),
                Is.True, "The real crest must display the persisted nickname.");

            // Reload the same real file-backed gateway, not a mock PlayerPrefs
            // or a new player. This is the original city's probe-only API.
            Call(city, "ReloadForProbe");
            for (int i = 0; i < 5; i++) yield return null;
            var restored = Call(Member(city, "Gateway"), "Snapshot");
            Assert.That(Member(restored, "PlayerId"), Is.EqualTo(originalId));
            Assert.That(Member(restored, "PlayerName"), Is.EqualTo("Lucía_07"));
            Assert.That(prompt.gameObject.activeSelf, Is.False,
                "A returning player must not be asked to choose a nickname again.");
        }
    }
}
