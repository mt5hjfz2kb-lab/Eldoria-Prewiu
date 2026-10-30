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
        // Unity Transform.Find treats '/' as a hierarchy path delimiter.
        // Crónica Real's historic presentation names include literal slashes.
        private static Transform DirectChild(Transform parent, string exactName)
        {
            if (!parent) return null;
            foreach (Transform child in parent)
                if (child.name == exactName) return child;
            return null;
        }
        private static int DirectChildCount(Transform parent, string exactName)
        {
            if (!parent) return 0;
            int count = 0;
            foreach (Transform child in parent)
                if (child.name == exactName) count++;
            return count;
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
        public IEnumerator ChronicleRealKeepsNativeBuildingIllustrationWithCanonicalOffer()
        {
            Component host = null, hud = null;
            yield return Boot((h, v) => { host = h; hud = v; });
            var modal = Member(hud, "Modal") as Component;
            // The authentic prologue is scheduled after host readiness and
            // its transitions use real time. Thirty immediate frames could
            // finish before it opened, leaving the intended modal over the card.
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(modal, Is.Not.Null);
            var extra = Member(modal, "Extra") as Button;
            for (int i = 0; i < 80 && modal.gameObject.activeSelf; ++i)
            {
                Assert.That(extra, Is.Not.Null);
                extra.onClick.Invoke();
                yield return new WaitForSecondsRealtime(.2f);
            }
            Assert.That(modal.gameObject.activeSelf, Is.False,
                "Read and acknowledge the real opening prologue before inspecting the building card");
            Call(hud, "SelectBuilding", "bastion");
            yield return null;
            var v2 = FindComponent(hud.gameObject, "RoyalGoldV2Hud");
            Call(v2, "SynchronizeNow");
            Canvas.ForceUpdateCanvases();
            Assert.That(Member(v2,"ChronicleIllustratedBuildingVisible"),Is.EqualTo(true),
                "Crónica Real must actually DISPLAY its illustrated building card, not just instantiate a hidden placeholder");
            // GameObject.Find interprets slash-bearing scene names as
            // hierarchy paths. Look up the actual existing HUD child by
            // exact name instead, preventing a false-negative native gate.
            var realCard = hud.GetComponentsInChildren<RectTransform>(true)
                .FirstOrDefault(x => x.name=="V2 / contextual building sheet");
            Assert.That(realCard,Is.Not.Null);
            var freeViewport = (Rect)Call(v2, "GetFreeSceneViewport");
            Assert.That(freeViewport.width, Is.GreaterThan(.5f),
                "The bottom card must preserve horizontal camera space in landscape and portrait");
            Assert.That(freeViewport.height, Is.GreaterThan(.1f),
                "The selected building must retain a usable camera viewport");
            var cardRect = realCard;
            var bottomNav = Member(hud,"Navigation") as Component;
            Assert.That(bottomNav,Is.Not.Null);
            // Both use safe-bottom anchored coordinates: an illustrated card
            // should be just above the live dock, never overlaid on its buttons.
            var navRect = bottomNav.transform as RectTransform;
            Assert.That(cardRect.anchoredPosition.y,
                Is.GreaterThanOrEqualTo(navRect.anchoredPosition.y+navRect.rect.height),
                "The approved illustrated card must not cover the navigation dock");
            var art = hud.GetComponentsInChildren<Graphic>(true)
                .FirstOrDefault(g => g.GetType().Name ==
                    "RoyalGoldV3BuildingEngravingGraphic");
            Assert.That(art, Is.Not.Null,
                "Crónica Real requires a genuine native illustrated selected-building sheet");
            Assert.That(art.raycastTarget, Is.False,
                "Decorative preview must not eat building or world input");
            var preview = hud.GetComponentsInChildren<MonoBehaviour>(true)
                .FirstOrDefault(x => x && x.GetType().Name == "RoyalGoldV3BuildingPreview");
            Assert.That(preview, Is.Not.Null);
            Assert.That(Member(preview, "HasModelPreview"), Is.EqualTo(true),
                "The building card must render the canonical model, not the generic house fallback");
            var thumbnail = preview.GetComponent<RawImage>();
            Assert.That(thumbnail.raycastTarget, Is.False);
            Assert.That(thumbnail.texture.width, Is.EqualTo(256),
                "One bounded cached thumbnail; no full-screen render texture per building");
            var previousTarget = RenderTexture.active;
            var pixels = new Texture2D(256, 256, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = (RenderTexture)thumbnail.texture;
                pixels.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
                pixels.Apply();
                Assert.That(pixels.GetPixels32().Select(c => ((int)c.r << 16) | ((int)c.g << 8) | c.b)
                    .Distinct().Count(), Is.GreaterThan(32),
                    "The render texture must contain real rendered geometry, not just a cleared background");
            }
            finally { RenderTexture.active = previousTarget; Object.Destroy(pixels); }
            var context = hud.GetComponentsInChildren<MonoBehaviour>(true)
                .FirstOrDefault(x => x && x.GetType().Name == "RoyalGoldV3ContextHud");
            Assert.That(context, Is.Not.Null,
                "The original gameplay-projected action/timer must remain in the same HUD");
            Assert.That(hud.GetComponentsInChildren<Canvas>(true).Length, Is.EqualTo(1),
                "Do not create a second gameplay HUD or separate Canvas");
        }

        [UnityTest]
        public IEnumerator SettingsResetRequiresSecondExplicitTap()
        {
            Component host = null, hud = null;
            yield return Boot((h, v) => { host = h; hud = v; });
            Assert.That(Member(hud, "ResetPlaytestProgress"), Is.Not.Null,
                "Integrated Settings must expose a reset while family testing");
            var settings = Member(hud, "Settings") as Button;
            var modal = Member(hud, "Modal") as Component;
            Assert.That(settings, Is.Not.Null);
            Assert.That(modal, Is.Not.Null);
            // The callback is a legitimate developer/tester action, not a
            // forced save deletion or a replay of a synthetic UI.
            settings.onClick.Invoke();
            yield return null;
            var reset = Member(modal, "Secondary") as Button;
            Assert.That(reset && reset.gameObject.activeInHierarchy, Is.True,
                "Test save reset belongs in the Settings panel");
            var initialRevision = ((Eldoria.Domain.PlayerState)Call(
                Member(Member(host, "City"), "Gateway"), "Snapshot")).Revision;
            reset.onClick.Invoke(); // FIRST tap only: arm, NEVER destroy a save.
            yield return null;
            var unchanged = ((Eldoria.Domain.PlayerState)Call(
                Member(Member(host, "City"), "Gateway"), "Snapshot")).Revision;
            Assert.That(unchanged,Is.EqualTo(initialRevision),
                "Merely arming playtest reset must not write or delete player progress");
            var body = Member(modal, "Body") as Text;
            Assert.That(body, Is.Not.Null);
            StringAssert.Contains("CONFIRM", body.text.ToUpperInvariant());
            // Simulate expiry without sleeping eight seconds or ever touching
            // the real player's save. The visible label MUST disarm too.
            var armedUntil=hud.GetType().GetField("resetArmedUntil",
                BindingFlags.Instance|BindingFlags.NonPublic);
            Assert.That(armedUntil,Is.Not.Null);
            armedUntil.SetValue(hud,-1f);
            yield return null;
            var resetCaption=Member(modal,"SecondaryLabel") as Text;
            Assert.That(resetCaption,Is.Not.Null);
            StringAssert.Contains("Reiniciar",resetCaption.text);
            Assert.That(((Eldoria.Domain.PlayerState)Call(
                Member(Member(host,"City"),"Gateway"),"Snapshot")).Revision,
                Is.EqualTo(initialRevision),
                "Expired reset confirmation cannot delete or rewrite the save");
            // Deliberately never issue the destructive second tap in shared QA.
        }

        [UnityTest]
        public IEnumerator BastionIIIWolfBodyMatchesBoarBodyScale()
        {
            Component host=null,hud=null;
            yield return Boot((h,v)=>{host=h;hud=v;});
            var world=Member(host,"WorldRoot") as GameObject;
            Assert.That(world,Is.Not.Null);
            var wolves=world.transform.Find("hunt-wolf");
            var boars=world.transform.Find("hunt-boar");
            Assert.That(wolves&&boars,Is.True,
                "Both original real world encounters must exist");
            var wolfModel=wolves.Find("AshWolf");
            var boarModel=boars.Find("RockBoarNear");
            Assert.That(wolfModel&&boarModel,Is.True,
                "Source-authored wolf and boar models must be instantiated");
            var wolfRenderers=wolfModel.GetComponentsInChildren<Renderer>(true);
            var boarRenderers=boarModel.GetComponentsInChildren<Renderer>(true);
            Assert.That(wolfRenderers.Length,Is.GreaterThan(0));
            Assert.That(boarRenderers.Length,Is.GreaterThan(0));
            var wolf=wolfRenderers[0].bounds;
            foreach(var renderer in wolfRenderers.Skip(1))wolf.Encapsulate(renderer.bounds);
            var boar=boarRenderers[0].bounds;
            foreach(var renderer in boarRenderers.Skip(1))boar.Encapsulate(renderer.bounds);
            float wolfWidth=Mathf.Max(wolf.size.x,wolf.size.z);
            float boarWidth=Mathf.Max(boar.size.x,boar.size.z);
            Assert.That(wolfWidth,Is.InRange(boarWidth*.89f,boarWidth*1.12f),
                "A single Ash Wolf must be approximately the same physical size as a Rock Boar");
        }

        [UnityTest]
        public IEnumerator NativeProbeResetStartsFreshWithoutDeletingAnyRealDeviceSave()
        {
            if(Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE")!="1")
                Assert.Ignore("The destructive path is only tested against a disposable probe file");
            Component host=null,hud=null;
            yield return Boot((h,v)=>{host=h;hud=v;});
            var city=Member(host,"City") as Component;
            Assert.That(city,Is.Not.Null);
            Assert.That((bool)Member(city,"Probe"),Is.True);
            string disposable=(string)Member(city,"SaveFile");
            Assert.That(disposable.StartsWith(System.IO.Path.GetTempPath(),
                StringComparison.OrdinalIgnoreCase),Is.True,
                "Never reset a non-probe save from this native test");
            var gateway=Member(city,"Gateway") as Eldoria.Application.ICommandGateway;
            Assert.That(gateway,Is.Not.Null);
            var fixture=gateway.Snapshot();
            fixture.BastionLevel=2;
            fixture.SawmillLevel=1;
            new Eldoria.Infrastructure.FileStateStore(disposable).Save(fixture);
            Assert.That(System.IO.File.Exists(disposable),Is.True);
            var originalBytes=System.IO.File.ReadAllBytes(disposable);
            var ok=(bool)Call(city,"ResetPlaytestProgress");
            Assert.That(ok,Is.True,"Reset did not accept its own disposable save");
            var backups=System.IO.Directory.GetFiles(
                System.IO.Path.GetDirectoryName(disposable),
                System.IO.Path.GetFileName(disposable)+".before-playtest-reset-*.bak");
            Assert.That(backups.Length,Is.EqualTo(1),
                "The reset must preserve exactly one recoverable backup of this unique probe save");
            CollectionAssert.AreEqual(originalBytes,System.IO.File.ReadAllBytes(backups[0]),
                "The backup must preserve the complete original save byte-for-byte");
            Assert.That(System.IO.File.Exists(disposable),Is.False,
                "Only the disposable file should be removed before fresh load");
            Component freshHost=null,freshHud=null;
            for(int frame=0;frame<220;frame++)
            {
                yield return null;
                var candidate=Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                    .FirstOrDefault(x=>x&&x.GetType().Name=="ValoriaIntegratedWorld" &&
                        (bool)Member(x,"Ready"));
                if(!candidate||ReferenceEquals(candidate,host))continue;
                freshHost=candidate;
                freshHud=Member(candidate,"Hud") as Component;
                if(freshHud)break;
            }
            Assert.That(freshHost,Is.Not.Null,
                "A real scene reload must create a fresh playable session");
            var freshCity=Member(freshHost,"City") as Component;
            var freshGateway=Member(freshCity,"Gateway") as Eldoria.Application.ICommandGateway;
            Assert.That(freshGateway.Snapshot().BastionLevel,Is.EqualTo(1));
            Assert.That(freshGateway.Snapshot().SawmillLevel,Is.Zero);
            Assert.That((string)Member(freshCity,"SaveFile"),Is.Not.EqualTo(disposable),
                "Fresh native probe must have a new isolated save path");
        }

        [UnityTest]
        public IEnumerator AuthoredWorldOpensWithMateriallyWiderStrategicFraming()
        {
            Component host = null, hud = null;
            yield return Boot((h, v) => { host = h; hud = v; });
            var navigation = Member(host, "WorldNavigation") as Component;
            var camera = Member(host, "WorldCamera") as Camera;
            Assert.That(navigation, Is.Not.Null);
            Assert.That(camera, Is.Not.Null);
            var worldHome = (Vector3)Member(host, "WorldHome");
            // Only view controls; do not purchase the sawmill, unlock an
            // objective, reset the save or mutate the canonical gateway.
            Call(navigation, "SetZoomLevel", 1, true);
            float oldRegion = camera.orthographicSize;
            Call(navigation, "EnterStrategicView", worldHome);
            float overview = camera.orthographicSize;
            Assert.That(overview, Is.GreaterThan(oldRegion * 1.45f),
                "The new entry must expose >2.1x regional map AREA, not a 10% zoom tweak.");
            Assert.That((float)Call(navigation, "StrategicEntryZoom"), Is.GreaterThan(0f));
            // Nearby world details must remain reachable by deliberate zoom.
            Call(navigation, "SetZoomLevel", 2, true);
            Assert.That(camera.orthographicSize, Is.LessThan(oldRegion));
            // Returning to the overview uses the existing bounded terrain.
            Call(navigation, "EnterStrategicView", worldHome);
            Assert.That(camera.orthographicSize, Is.EqualTo(overview).Within(.01f));
        }


        [UnityTest]
        public IEnumerator ActualPanoramaHasNoTinyUiMarkersButRegionalKeepsTheCityObjective()
        {
            if (Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE") != "1")
                Assert.Ignore("A disposable gameplay probe is required before entering the true world.");
            Component host=null,hud=null;
            yield return Boot((h,v)=>{host=h;hud=v;});
            bool opened=(bool)Call(host,"TrySetWorld",true);
            if(!opened)
            {
                var offer=Member(Member(Call(host,"Read","sawmill",false),"Building"),"Action");
                Assert.That(offer,Is.Not.Null,"Actual progression must authorize world entry.");
                Call(host,"Submit",offer);
                var city=Member(host,"City");
                Call(Member(city,"Clock"),"Step",Eldoria.Domain.SliceRules.SawmillBuildSeconds+1);
                Call(Member(city,"Gateway"),"Advance");
                yield return null;
                opened=(bool)Call(host,"TrySetWorld",true);
            }
            Assert.That(opened,Is.True);
            var nav=Member(host,"WorldNavigation") as Component;
            var guidance=Member(host,"Guidance") as Component;
            Assert.That(nav,Is.Not.Null);
            Assert.That(guidance,Is.Not.Null);
            var field=guidance.GetType().GetField("labels",BindingFlags.Instance|BindingFlags.NonPublic);
            Assert.That(field,Is.Not.Null);
            var labels=field.GetValue(guidance) as System.Collections.IDictionary;
            Assert.That(labels,Is.Not.Null);
            Assert.That(labels.Count,Is.GreaterThan(0),"The real world must retain its existing labels.");
            var gateway=Member(Member(host,"City"),"Gateway");
            string saved=JsonUtility.ToJson(Call(gateway,"Snapshot"));
            Call(nav,"SetZoomLevel",0,true);
            Call(guidance,"Refresh");
            int worldLabels=0;
            foreach(System.Collections.DictionaryEntry item in labels)
            {
                string id=item.Key.ToString();
                if(id=="sawmill"||id=="barracks")continue;
                var marker=item.Value as Component;
                Assert.That(marker,Is.Not.Null);
                Assert.That(marker.gameObject.activeSelf,Is.False,
                    "Panoramic continent cannot show a small HUD/POI label: "+id);
                worldLabels++;
            }
            Assert.That(worldLabels,Is.GreaterThan(0));
            Call(nav,"SetZoomLevel",1,true);
            Call(guidance,"Refresh");
            var valoria=labels["valoria-map-city"] as Component;
            Assert.That(valoria,Is.Not.Null);
            Assert.That(valoria.gameObject.activeSelf,Is.True,
                "The real Valoria marker should return in regional gameplay.");
            Assert.That(JsonUtility.ToJson(Call(gateway,"Snapshot")),Is.EqualTo(saved),
                "Presentation-only label changes must never write saved game state.");
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
                Is.LessThanOrEqualTo(136f), "Crónica Real manuscript must remain compact");
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
        public IEnumerator ApprovedCronicaRealUsesAuthenticParchmentAndSafeEdgeNavigation()
        {
            Component host = null, hud = null;
            yield return Boot((h, v) => { host = h; hud = v; });
            var v2 = FindComponent(hud.gameObject, "RoyalGoldV2Hud");
            yield return null;
            Call(v2, "SynchronizeNow");
            Canvas.ForceUpdateCanvases();

            var objective = GameObject.Find("V2 / current objective");
            Assert.That(objective, Is.Not.Null, "Missing live objective");
            var safeRoot = (Member(hud, "Realm") as Component)?.transform.parent;
            Assert.That(safeRoot, Is.Not.Null, "Real safe-edge root must exist");
            var royalPlate = DirectChild(safeRoot, "V3 / continuous original royal header");
            // Do not pass just because three independent header buttons are at y=0:
            // the owner-approved illustration has one unbroken royal-gold bar.
            Assert.That(royalPlate, Is.Not.Null,
                "Crónica Real needs one continuous full-safe-width top band");
            var plateGraphic = royalPlate.GetComponent<Graphic>();
            Assert.That(plateGraphic, Is.Not.Null);
            Assert.That(plateGraphic.raycastTarget, Is.False);
            var rootRect = safeRoot as RectTransform;
            Assert.That(((RectTransform)royalPlate).rect.width,
                Is.EqualTo(rootRect.rect.width).Within(1f),
                "The top band must cover the entire device safe edge");
            Assert.That(DirectChildCount(safeRoot, "V3 / continuous original royal header"),
                Is.EqualTo(1), "No duplicate royal headers after layout");

            var paper = DirectChild(objective.transform, "V3 / original parchment and engraved edges");
            Assert.That(paper, Is.Not.Null, "Actual original manuscript treatment lost");
            Assert.That(DirectChildCount(objective.transform,
                "V3 / original parchment and engraved edges"), Is.EqualTo(1),
                "The manuscript must not be duplicated on later UI reflows");
            var paperGraphic = paper.GetComponent<Graphic>();
            Assert.That(paperGraphic, Is.Not.Null);
            Assert.That(paperGraphic.GetType().Name,
                Is.EqualTo("RoyalGoldV3ObjectiveParchmentGraphic"));
            Assert.That(paperGraphic.raycastTarget, Is.False,
                "Decorative parchment may never intercept objective actions");
            var objectiveButton = Member(v2, "VisibleObjectiveButton") as Button;
            Assert.That(objectiveButton, Is.Not.Null,
                "The quest pictogram must keep its existing live command");
            var roundObjective = DirectChild(objectiveButton.transform, "Crónica / native round icon");
            Assert.That(roundObjective, Is.Not.Null,
                "Crónica Real's objective command must use a circular medieval medallion");
            Assert.That(objectiveButton.targetGraphic,
                Is.SameAs(roundObjective.GetComponent<Graphic>()),
                "The real button must retain its native circular hit graphic");
            var caption = objective.GetComponentsInChildren<Text>(true)
                .FirstOrDefault(x => x.name == "Label");
            if (caption != null)
                Assert.That(caption.enabled, Is.False,
                    "Rotating the device must not resurrect the old rectangular CTA label");
            Assert.That(DirectChild(objective.transform, "V3 / original Valoria heraldic sigil"),
                Is.Not.Null, "Reuse Valoria's actual established crest");

            var nav = Member(hud, "Navigation") as Component;
            Assert.That(nav, Is.Not.Null);
            var dock = nav.transform as RectTransform;
            var safe = dock.parent as RectTransform;
            Assert.That(safe, Is.Not.Null);
            Assert.That(dock.anchorMin.x, Is.EqualTo(.5f).Within(.01f));
            Assert.That(dock.anchorMin.y, Is.EqualTo(0f).Within(.01f),
                "Left or floating navigation violates approved original");
            Assert.That(dock.anchorMax.y, Is.EqualTo(0f).Within(.01f));
            Assert.That(dock.pivot.y, Is.EqualTo(0f).Within(.01f));
            Assert.That(dock.anchoredPosition.y, Is.EqualTo(0f).Within(1f),
                "Footer must be flush with the DEVICE safe area");
            Assert.That(dock.rect.width, Is.LessThanOrEqualTo(safe.rect.width + 1f),
                "All navigation must fit within the current safe area");
            var trim = DirectChild(dock, "V3 / Chronicle bottom frame");
            Assert.That(trim, Is.Not.Null);
            // Two layout recalculations must never stack opaque frames over
            // other interactive content; this was caused by slash names.
            Call(v2, "SynchronizeNow");
            Call(v2, "SynchronizeNow");
            Assert.That(DirectChildCount(dock,
                "V3 / Chronicle bottom frame"), Is.EqualTo(1),
                "Repeated layout must retain exactly one native footer frame");
            Assert.That(trim.GetComponent<Graphic>().raycastTarget, Is.False);
            var entries = Member(nav, "Items") as System.Array;
            Assert.That(entries, Is.Not.Null);
            Assert.That((bool)Member(nav, "ChronicleLayout"), Is.True,
                "The original five-entry Crónica Real art must be active");
            // Five genuine destinations at the safe-bottom edge. Locked
            // actions can be visible but must never become interactable.
            var visible = entries.Cast<object>().Where(entry =>
            {
                var liveButton = Member(entry, "Button") as Button;
                return liveButton && liveButton.gameObject.activeSelf;
            }).ToArray();
            const int expectedVisible = 5; // Stable approved navigation; locks remain real
            foreach (var entry in entries)
            {
                string category = Member(entry, "Section").ToString();
                if(category != "Army" && category != "World") continue;
                bool trulyUnlocked = (bool)Call(host, "CanOpen", Member(entry, "Section"));
                var entryButton=Member(entry, "Button") as Button;
                Assert.That(entryButton.gameObject.activeSelf, Is.True,
                    "All five approved destinations remain visible");
                Assert.That(entryButton.interactable, Is.EqualTo(trulyUnlocked),
                    "Visible locked Army/World controls must not bypass narrative unlocks");
            }
            Assert.That(visible.Length, Is.EqualTo(expectedVisible),
                "Only coherent Crónica Real destinations may appear at this Bastion level");
            foreach (var entry in visible)
            {
                string section = Member(entry, "Section").ToString();
                if (section == "Army" || section == "World") continue;
                var button = Member(entry, "Button") as Button;
                Assert.That(button.interactable, Is.True,
                    "Only an existing real kingdom, build or chapter destination can be enabled");
            }
            int active = 0;
            foreach (var entry in entries)
            {
                var button = Member(entry, "Button") as Button;
                Assert.That(button, Is.Not.Null);
                if (!button.gameObject.activeSelf) continue;
                active++;
                var rect = button.transform as RectTransform;
                Assert.That(rect.anchorMin.y, Is.EqualTo(.5f).Within(.01f));
                Assert.That(rect.rect.height, Is.GreaterThanOrEqualTo(72f));
                Assert.That(Mathf.Abs(rect.anchoredPosition.x) + rect.rect.width * .5f,
                    Is.LessThanOrEqualTo(dock.rect.width * .5f + 1f),
                    "No genuine native medallion can leave the bottom dock");
                // In uGUI an anchor at the middle of a bottom-pivot dock
                // already includes half its height. The previous layout
                // accidentally ADDED that half-height again: buttons hovered
                // above the actual footer despite a valid dock anchor.
                Assert.That(Mathf.Abs(rect.anchoredPosition.y) + rect.rect.height * .5f,
                    Is.LessThanOrEqualTo(dock.rect.height * .5f + 1f),
                    "Every actual clickable medallion must remain INSIDE the bottom dock");
            }
            // Bastion I begins with locked sections. A zero-count state is
            // legitimate; never unlock or fabricate actions to satisfy art.
            if (active == 0)
                foreach (var entry in entries)
                    Assert.That((bool)Call(host, "CanOpen", Member(entry, "Section")),
                        Is.False, "Locked navigation must match genuine progression");
            var realm = Member(hud, "Realm") as Component;
            var settings = Member(hud, "Settings") as Button;
            Assert.That(((RectTransform)realm.transform).anchoredPosition.y,
                Is.EqualTo(0f).Within(1f), "Top crest floats below safe edge");
            Assert.That(((RectTransform)settings.transform).anchoredPosition.y,
                Is.EqualTo(0f).Within(1f), "Top icon floats below safe edge");
            var headerCorners = new Vector3[4];
            var questCorners = new Vector3[4];
            ((RectTransform)realm.transform).GetWorldCorners(headerCorners);
            objective.GetComponent<RectTransform>().GetWorldCorners(questCorners);
            Assert.That(questCorners[1].y, Is.LessThan(headerCorners[0].y),
                "The approved parchment must begin BELOW the real 104-unit royal header");
        }

        [UnityTest]
        public IEnumerator ClosingActualPrologueKeepsIntegratedNavigationOwnership()
        {
            if (Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE") != "1")
                Assert.Ignore("Requires disposable native probe save");
            Component host = null, hud = null;
            yield return Boot((h, v) => { host = h; hud = v; });
            var city = Member(host, "City");
            var presenter = Member(city, "Presenter");
            Assert.That(Member(presenter, "ExternalNavigationOwned"), Is.EqualTo(true));
            Assert.That(Member(presenter, "LegacyWebNavigationAllowed"), Is.EqualTo(false));
            var guidance = Member(host, "Guidance") as Behaviour;
            Assert.That(guidance, Is.Not.Null);
            guidance.enabled = false;
            Call(guidance, "PresentMilestone");
            Assert.That(Member(hud, "NarrativeOpen"), Is.EqualTo(true));
            var modal = Member(hud, "Modal");
            var next = Member(modal, "Extra") as Button;
            for (int i = 0; i < 40 && (bool)Member(hud, "NarrativeOpen"); i++)
            {
                next.onClick.Invoke();
                // Ownership must still hold in the release frame, before a
                // deferred WebGL heuristic can interpret this same touch.
                Assert.That(Member(presenter, "LegacyWebNavigationAllowed"), Is.EqualTo(false));
                yield return null;
            }
            Assert.That(Member(hud, "NarrativeOpen"), Is.EqualTo(false));
            Assert.That(Member(host, "InWorld"), Is.EqualTo(false));
            Assert.That(Member(presenter, "LegacyWebNavigationAllowed"), Is.EqualTo(false));
        }

        [UnityTest]
        public IEnumerator BuildingSelectionRetainsAuthoritativeGatewayOffer()
        {
            Component host = null, hud = null;
            yield return Boot((h, v) => { host = h; hud = v; });
            var v3 = FindComponent(hud.gameObject, "RoyalGoldV3ContextHud");
            Assert.That(Member(v3, "IsConfigured"), Is.EqualTo(true));
            // Selection UI is intentionally hidden during the genuine opening
            // cinematic. Isolate interaction presentation without acknowledging
            // narrative, modifying gameplay or writing the owner's save.
            var guidance = Member(host, "Guidance") as Behaviour;
            if (guidance) guidance.enabled = false;
            var modal = Member(hud, "Modal") as Component;
            if (modal && modal.gameObject.activeInHierarchy) Call(modal, "Hide");
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
            var management = FindComponent(hud.gameObject, "RoyalGoldV3BuildingScreens");
            Assert.That(Member(management, "IsOpen"), Is.EqualTo(false),
                "Selecting a normal building must not open a new page");
            Assert.That(Member(v3, "ContextVisible"), Is.EqualTo(true),
                "City-bound building selection must remain contextual");
            Assert.That(Member(v2, "BuildingActionVisible"), Is.EqualTo(true),
                "Existing browser telemetry must see the genuine contextual action");
            Assert.That(Member(v2, "VisibleBuildingButton"),
                Is.SameAs(Member(v3, "VisibleActionButton")));
            Assert.That(Member(v2, "VisibleBuildingButton"),
                Is.SameAs(Member(v2, "ChronicleBuildingAction")),
                "The illustrated city cartouche must own the only build command");
            var floating = hud.GetComponentsInChildren<CanvasGroup>(true)
                .First(x => x.name == "V3 / selected building contextual action");
            Assert.That(floating.blocksRaycasts, Is.False,
                "No duplicate floating action may intercept the city");
            Assert.That(floating.alpha, Is.EqualTo(0f));
            var actualClose = Member(v2, "VisibleBuildingCloseButton") as Button;
            Assert.That(actualClose, Is.Not.Null);
            Assert.That(actualClose, Is.SameAs(Member(v3, "VisibleCloseButton")),
                "Closing the city selection must not depend on an independent page");
            var closeCorners = new Vector3[4];
            ((RectTransform)actualClose.transform).GetWorldCorners(closeCorners);
            var closePoint = RectTransformUtility.WorldToScreenPoint(null,
                (closeCorners[0] + closeCorners[2]) * .5f);
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = closePoint
            };
            foreach (var legacy in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(x=>x.name=="Eldoria HUD"))
            {
                Assert.That(legacy.enabled,Is.False,"Integrated scene must own every visible HUD");
                var legacyRaycaster=legacy.GetComponent<GraphicRaycaster>();
                Assert.That(!legacyRaycaster||!legacyRaycaster.enabled,Is.True,"Legacy HUD cannot intercept actual player touches");
            }
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits.Count, Is.GreaterThan(0));
            Assert.That(ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject),
                Is.SameAs(actualClose.gameObject), "V3 close must be the topmost touch target; close depth="+actualClose.targetGraphic.depth+"; hits="+string.Join(", ",hits.Select(x=>x.gameObject.name+" depth="+x.depth)));
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
        [UnityTest]
        public IEnumerator RealSawmillPurchaseProjectsTimerInTheSameRenderedFrame()
        {
            // Never execute a building purchase on an owner's persisted save.
            // The existing native CI sets the probe variable and the real
            // ValoriaReferenceCleanGameplay creates a disposable file store.
            if (Environment.GetEnvironmentVariable("VALORIA_GAMEPLAY_PROBE") != "1")
                Assert.Ignore("Timer purchase test requires the disposable Valoria native probe save");

            Component host = null, hud = null;
            yield return Boot((h, v) => { host = h; hud = v; });

            var guidance = Member(host, "Guidance") as Behaviour;
            if (guidance) guidance.enabled = false;
            var modal = Member(hud, "Modal") as Component;
            if (modal && modal.gameObject.activeInHierarchy) Call(modal, "Hide");

            var v3 = FindComponent(hud.gameObject, "RoyalGoldV3ContextHud");
            Call(hud, "SelectBuilding", "sawmill");
            // FocusAfterLayout has its own yield; wait for its projection
            // BEFORE the purchase. The timer assertion below stays same-frame.
            yield return null;
            yield return null;
            Canvas.ForceUpdateCanvases();
            Call(v3, "SynchronizeNow");

            var management = FindComponent(hud.gameObject, "RoyalGoldV3BuildingScreens");
            Assert.That(Member(management, "IsOpen"), Is.False);
            var command = Member(v3, "VisibleActionButton") as Button;
            Assert.That(command, Is.Not.Null);
            Assert.That(command.interactable, Is.True, "Actual sawmill Build must be actionable");
            var offer = Member(Member(Call(host, "Read", "sawmill", false),
                "Building"), "Action");
            Assert.That(offer, Is.Not.Null);
            Assert.That(Member(offer, "Kind"), Is.EqualTo("Build"));

            // The V3 button itself forwards to the one existing RoyalHUD
            // command handler. Its old callback persists the real operation.
            command.onClick.Invoke();
            var operation = Member(Call(host, "Read", "sawmill", false), "Building");
            Assert.That(Member(operation, "OperationActive"), Is.EqualTo(true),
                "Real gateway must own the construction state and deadline");

            // No wait/yield: the first UI sync after payment must project
            // the same authoritative timer, not a stale pre-purchase offer.
            Call(v3, "SynchronizeNow");
            Assert.That(Member(management, "IsOpen"), Is.EqualTo(false));
            Assert.That(Member(v3, "ActiveTimers"), Is.EqualTo(1),
                "The paid work must show one anchored timer immediately");
            Assert.That(Member(v3, "ContextVisible"), Is.EqualTo(false),
                "The paid offer is never duplicated while work is active");
            Assert.That(command.interactable, Is.False,
                "Paid construction cannot be purchased twice");
            var operationTime = Member(operation, "Time") as string;
            Assert.That(hud.GetComponentsInChildren<Text>(true).Any(x =>
                x.gameObject.name=="Live remaining time" &&
                x.gameObject.activeInHierarchy && x.text.Contains(operationTime)),
                Is.True,"The genuine city timer must show the gateway countdown");
            Call(hud, "SelectBuilding", "");
            Call(v3, "SynchronizeNow");
            Assert.That(Member(v3, "ActiveTimers"), Is.EqualTo(1),
                "Closing the contextual selection must preserve the real timer");

        }

        [UnityTest]
        public IEnumerator ReducedWorldForestPreservesExistingHudAndNoninteractiveDecorations()
        {
            // Thinning decorative crowns cannot change HUD, game entities or saves.
            Component host=null,hud=null;
            yield return Boot((h,v)=>{host=h;hud=v;});
            var world=Member(host,"WorldRoot") as GameObject;
            Assert.That(world,Is.Not.Null);
            for(int i=0;i<180&&world.transform.Find("Region 1 canonical continuous geography")==null;i++)
                yield return null;
            Assert.That(world.transform.Find("Region 1 canonical continuous geography"),Is.Not.Null);
            var renderers=world.GetComponentsInChildren<MeshRenderer>(true);
            var crowns=renderers.Where(r=>r.name.StartsWith("BLD_ForestMass_",StringComparison.Ordinal)).ToArray();
            Assert.That(crowns.Length,Is.EqualTo(5047),"All authored sources remain available for reversal.");
            int hidden=crowns.Count(r=>r.forceRenderingOff);
            Assert.That(hidden,Is.EqualTo(3251),"Remove the inventoried rejected crowns and infill only.");
            Assert.That(crowns.Count(r=>!r.forceRenderingOff),Is.GreaterThan(1500));
            Assert.That(crowns.All(r=>r.gameObject.activeSelf&&r.GetComponent<Collider>()==null),Is.True);
            foreach(var near in crowns)
            {
                var far=renderers.Single(r=>r.name==near.name.Replace("BLD_ForestMass_","BLD_ForestFar_"));
                Assert.That(far.forceRenderingOff,Is.EqualTo(near.forceRenderingOff),near.name+" must stay coherent across zoom.");
                if(near.forceRenderingOff)
                {
                    // Boot starts in the city: the preserved world hierarchy is inactive.
                    var group=near.GetComponentInParent<LODGroup>(true);
                    Assert.That(group,Is.Not.Null,near.name+" must retain its paired LOD source.");
                    Assert.That(group.enabled,Is.False);
                }
            }
            var protectedPines=renderers.Where(r=>r.name.StartsWith("Pilot_Pine",StringComparison.Ordinal)).ToArray();
            Assert.That(protectedPines.Length,Is.EqualTo(462));
            Assert.That(protectedPines.All(r=>!r.forceRenderingOff),Is.True,"Keep original AF/AG near geometry.");
            Assert.That(crowns.Where(r=>r.sharedMaterials.Any(m=>m&&m.name.StartsWith("Player broadleaf crown shade ",StringComparison.Ordinal)))
                .All(r=>r.forceRenderingOff),Is.True,"Rejected artificial rounded model must not remain active.");
            Assert.That(Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Count(x=>x&&x.GetType().Name=="RoyalHUD"),Is.EqualTo(1),
                "Forest decoration cannot create a second HUD or interaction system.");
        }

    }
}

