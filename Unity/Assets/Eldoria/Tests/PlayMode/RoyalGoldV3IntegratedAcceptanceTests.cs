using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Eldoria.Tests
{
    /// <summary>
    /// Validates the canonical integrated scene without coupling the existing
    /// Eldoria.Tests.PlayMode asmdef to the runtime Assembly-CSharp or V2
    /// assembly. Reflection is used only to inspect existing presentation
    /// components; no gameplay commands or save writes are issued here.
    /// </summary>
    public sealed class RoyalGoldV3IntegratedAcceptanceTests
    {
        private static object Member(object target, string name)
        {
            Assert.That(target, Is.Not.Null, "Target for " + name + " missing");
            var type = target.GetType();
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (field != null) return field.GetValue(target);
            var property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            Assert.That(property, Is.Not.Null, "No public member " + type.Name + "." + name);
            return property.GetValue(target);
        }
        private static object Call(object target, string name, params object[] args)
        {
            var method = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == name && m.GetParameters().Length == args.Length);
            Assert.That(method, Is.Not.Null, "Missing existing " + name + " action");
            return method.Invoke(target, args);
        }
        private static Component FindComponent(GameObject root, string typeName)
        {
            Assert.That(root, Is.Not.Null);
            var found = root.GetComponent(typeName);
            Assert.That(found, Is.Not.Null, "Required existing " + typeName + " not found");
            return found;
        }
        private static IEnumerator Boot(Action<Component, Component> ready)
        {
            SceneManager.LoadScene("Valoria_City_World_UI");
            for (int frame = 0; frame < 180; frame++)
            {
                yield return null;
                var host = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .FirstOrDefault(x => x && x.GetType().Name == "ValoriaIntegratedWorld");
                if (!host || !(bool)Member(host, "Ready")) continue;
                var hud = Member(host, "Hud") as Component;
                if (!hud) continue;
                ready(host, hud);
                yield break;
            }
            Assert.Fail("Consolidated Valoria scene did not create its one real HUD");
        }

        [UnityTest]
        public IEnumerator SingleHudCompactObjectiveAndNoOverlayAtRest()
        {
            Component host = null, hud = null;
            yield return Boot((h, v) => { host = h; hud = v; });
            Assert.That(host, Is.Not.Null);
            var v2 = FindComponent(hud.gameObject, "RoyalGoldV2Hud");
            var v3 = FindComponent(hud.gameObject, "RoyalGoldV3ContextHud");
            Assert.That(Member(v3, "Source"), Is.SameAs(hud));
            Assert.That(Member(v3, "IsConfigured"), Is.EqualTo(true));
            Assert.That(Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Count(x => x && x.GetType().Name == "RoyalHUD"), Is.EqualTo(1),
                "Never create a second authoritative game HUD");
            Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length,
                Is.LessThanOrEqualTo(1), "No second EventSystem");
            yield return null;
            Canvas.ForceUpdateCanvases();
            var objective = GameObject.Find("V2 / current objective");
            Assert.That(objective, Is.Not.Null);
            Assert.That(objective.GetComponent<RectTransform>().rect.height,
                Is.LessThanOrEqualTo(120f), "Objectives must be compact by default");
            var sheet = GameObject.Find("V2 / contextual building sheet");
            Assert.That(sheet, Is.Not.Null);
            Assert.That(sheet.GetComponent<CanvasGroup>().blocksRaycasts,
                Is.False, "Oversized V2 building sheet must not intercept the game");
            Assert.That(Member(v3, "ActiveTimers"), Is.EqualTo(0), "No fake idle timers");
            Assert.That(Member(v3, "ContextVisible"), Is.EqualTo(false));
            Assert.That(Member(host, "InWorld"), Is.EqualTo(false));
            var free = (Rect)Call(v2, "GetFreeSceneViewport");
            Assert.That(free.width * free.height, Is.GreaterThan(.25f));
        }

        [UnityTest]
        public IEnumerator BuildingSelectionRetainsAuthoritativeGatewayOffer()
        {
            Component host = null, hud = null;
            yield return Boot((h, v) => { host = h; hud = v; });
            var v3 = FindComponent(hud.gameObject, "RoyalGoldV3ContextHud");
            Assert.That(Member(v3, "IsConfigured"), Is.EqualTo(true));
            var sawmill = Call(host, "FindTarget", "sawmill") as Component;
            Assert.That(sawmill, Is.Not.Null, "Original sawmill hotspot must survive");
            var anchor = Member(v3, "FindAnchor") as Delegate;
            Assert.That(anchor, Is.Not.Null);
            Assert.That(anchor.DynamicInvoke("sawmill"), Is.SameAs(sawmill.transform));
            var english = (bool)Member(host, "English");
            var existing = Member(Call(host, "Read", "sawmill", english), "Building");
            Assert.That(existing, Is.Not.Null);
            var sourceState = Member(existing, "State");
            var read = Member(v3, "ReadBuilding") as Delegate;
            Assert.That(read, Is.Not.Null);
            var candidate = read.DynamicInvoke("sawmill");
            Assert.That(Member(candidate, "State"), Is.EqualTo(sourceState));
            Call(hud, "SelectBuilding", "sawmill");
            yield return null;
            yield return null; // allow the canonical camera focus coroutine to finish
            Canvas.ForceUpdateCanvases();
            Call(v3, "SynchronizeNow");
            Assert.That(Member(hud, "SelectedBuildingId"), Is.EqualTo("sawmill"));
            var v2 = FindComponent(hud.gameObject, "RoyalGoldV2Hud");
            Assert.That(Member(v3, "ContextVisible"), Is.EqualTo(true),
                "Selected building must have a visible contextual offer");
            Assert.That(Member(v2, "BuildingActionVisible"), Is.EqualTo(true),
                "Existing browser telemetry must discover the genuine V3 action");
            Assert.That(Member(v2, "VisibleBuildingButton"),
                Is.SameAs(Member(v3, "VisibleActionButton")));
            Assert.That(Member(v3, "CloseVisible"), Is.EqualTo(true),
                "The mobile player must be able to close selection without Escape");
            var actualClose = Member(v2, "VisibleBuildingCloseButton") as Button;
            Assert.That(actualClose, Is.Not.Null);
            Assert.That(actualClose, Is.SameAs(Member(v3, "VisibleCloseButton")));
            var closeCorners = new Vector3[4];
            ((RectTransform)actualClose.transform).GetWorldCorners(closeCorners);
            var closePoint = RectTransformUtility.WorldToScreenPoint(null,
                (closeCorners[0] + closeCorners[2]) * .5f);
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = closePoint
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits.Count, Is.GreaterThan(0));
            Assert.That(ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject),
                Is.SameAs(actualClose.gameObject), "V3 close must be the topmost touch target");
            var offer = Member(candidate, "Action");
            var actualOffer = Member(read.DynamicInvoke("sawmill"), "Action");
            Assert.That(actualOffer == null ? null : Member(actualOffer, "Kind"),
                Is.EqualTo(offer == null ? null : Member(offer, "Kind")),
                "V3 must use the actual gateway action and not an invented one");
            // Exercise the original single-HUD selection API through the
            // new actual visible Close button, not an invented dismiss command.
            actualClose.onClick.Invoke();
            yield return null;
            Assert.That(Member(hud, "SelectedBuildingId"), Is.EqualTo(""));
            Assert.That(Member(v3, "ContextVisible"), Is.EqualTo(false));
            Assert.That(Member(v3, "CloseVisible"), Is.EqualTo(false));
        }
    }
}
