using System.Collections;
using Eldoria.ReferenceClean;
using Eldoria.UI.RoyalGold;
using Eldoria.UI.RoyalGold.V2;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Eldoria.Tests
{
    /// <summary>
    /// Integration checks over the actual consolidated game scene. These
    /// checks deliberately do not send commands or touch stored progression.
    /// Full I-II/save/reload and camera captures use the existing native gates.
    /// </summary>
    public sealed class RoyalGoldV3IntegratedAcceptanceTests
    {
        [UnityTest]
        public IEnumerator IntegratedSceneUsesSingleHudAndObjectAnchoredV3()
        {
            SceneManager.LoadScene("Valoria_City_World_UI");
            ValoriaIntegratedWorld host = null;
            for (int frame = 0; frame < 160; frame++)
            {
                yield return null;
                host = Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                if (host && host.Ready && host.Hud) break;
            }
            Assert.That(host, Is.Not.Null, "Canonical integrated host must exist");
            Assert.That(host.Ready, Is.True, "Cannot validate UI against an unready scene");
            var hud = host.Hud;
            Assert.That(hud, Is.Not.Null);
            Assert.That(Object.FindObjectsByType<RoyalHUD>(FindObjectsSortMode.None).Length,
                Is.EqualTo(1), "Do not create a second RoyalHUD");
            Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length,
                Is.LessThanOrEqualTo(1), "Do not create a second EventSystem");
            var v2 = hud.GetComponent<RoyalGoldV2Hud>();
            var v3 = hud.GetComponent<RoyalGoldV3ContextHud>();
            Assert.That(v2, Is.Not.Null);
            Assert.That(v3, Is.Not.Null);
            Assert.That(v3.Source, Is.SameAs(hud));
            Assert.That(v3.IsConfigured, Is.True);
            yield return null;
            Canvas.ForceUpdateCanvases();
            var objective = GameObject.Find("V2 / current objective");
            Assert.That(objective, Is.Not.Null);
            Assert.That(objective.GetComponent<RectTransform>().rect.height,
                Is.LessThanOrEqualTo(120f), "Expanded missions are optional, not permanent");
            var oldSheet = GameObject.Find("V2 / contextual building sheet");
            Assert.That(oldSheet, Is.Not.Null);
            Assert.That(oldSheet.GetComponent<CanvasGroup>().blocksRaycasts,
                Is.False, "V2 must not intercept city or building interactions");
            Assert.That(v3.ActiveTimers, Is.EqualTo(0),
                "An idle game must not fake construction timers");
            var context = v3.ContextRect;
            Assert.That(context, Is.Not.Null);
            Assert.That(context.GetComponent<CanvasGroup>().blocksRaycasts,
                Is.False, "No unselected floating action may cover the city");

            var originalWorld = host.InWorld;
            Assert.That(originalWorld, Is.False, "Integrated scene must start in Valoria");
            var area = v2.GetFreeSceneViewport();
            Assert.That(area.width * area.height, Is.GreaterThan(.25f),
                "HUD viewport must preserve playable scene area");
        }

        [UnityTest]
        public IEnumerator SelectionUsesCanonicalTargetsAndNoBackgroundActions()
        {
            SceneManager.LoadScene("Valoria_City_World_UI");
            ValoriaIntegratedWorld host = null;
            for (int frame = 0; frame < 160; frame++)
            {
                yield return null;
                host = Object.FindFirstObjectByType<ValoriaIntegratedWorld>();
                if (host && host.Ready && host.Hud) break;
            }
            Assert.That(host, Is.Not.Null);
            Assert.That(host.Ready, Is.True);
            var hud = host.Hud;
            var v3 = hud.GetComponent<RoyalGoldV3ContextHud>();
            Assert.That(v3, Is.Not.Null);
            var target = host.FindTarget("sawmill");
            Assert.That(target, Is.Not.Null, "Sawmill interaction hotspot must remain");
            var source = host.Read("sawmill", host.English).Building;
            Assert.That(source, Is.Not.Null);
            Assert.That(source.Id, Is.EqualTo("sawmill"));
            Assert.That(v3.FindAnchor("sawmill"), Is.SameAs(target.transform));
            Assert.That(v3.ReadBuilding("sawmill").State, Is.EqualTo(source.State));
            hud.SelectBuilding("sawmill");
            yield return null;
            Assert.That(hud.SelectedBuildingId, Is.EqualTo("sawmill"));
            Assert.That(v3.ReadBuilding("sawmill").Action?.Kind, Is.EqualTo(source.Action?.Kind),
                "The visible offer must come from the existing gateway");
            hud.SelectBuilding("");
            yield return null;
            Assert.That(v3.ContextVisible, Is.False,
                "Closing the selection must remove its contextual button");
        }
    }
}
