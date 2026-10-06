using System.Collections;
using System.Reflection;
using Eldoria.Domain;
using Eldoria.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Eldoria.Tests
{
    public sealed class WorldRegion1RuntimeTests
    {
        [UnityTest]
        public IEnumerator DedicatedWorldScreenBuildsCanonicalRegion1Layers()
        {
            SceneManager.LoadScene("Frontier");
            yield return null;
            yield return null;

            Assert.That(GameObject.Find("World Region 1 · root"),Is.Not.Null);
            Assert.That(GameObject.Find("Frontier · integrated 4X visual layer"),Is.Null,
                "Legacy Frontier presentation must not remain the player-facing world.");
            Assert.That(GameObject.Find("World Region 1 · Valoria"),Is.Not.Null);
            Assert.That(GameObject.Find("World Region 1 · Player City v1") ?? GameObject.Find("World Region 1 · Valoria fallback keep"),Is.Not.Null);
            Assert.That(GameObject.Find("World Region 1 · Valoria main route"),Is.Not.Null);
            Assert.That(GameObject.Find("World Region 1 · old watch ruin"),Is.Not.Null);

            AssertHotspot("World Region 1 · Valoria target","valoria-map-city");
            AssertHotspot("World Region 1 · forest target","forest-valoria");
            AssertHotspot("World Region 1 · quarry target","quarry-valoria");
            AssertHotspot("World Region 1 · ruin target","old-watch-ruin");
            AssertHotspot("World Region 1 · corrupt scout target","corrupt-scout");

            var camera=GameObject.Find("Isometric camera")?.GetComponent<Camera>();
            Assert.That(camera,Is.Not.Null);
            Assert.That(camera.orthographic,Is.True);
            Assert.That(camera.orthographicSize,Is.EqualTo(14f).Within(.01f));

            var cityVisual=GameObject.Find("World Region 1 · Player City v1");
            if(cityVisual!=null)
                foreach(var collider in cityVisual.GetComponentsInChildren<Collider>(true))
                    Assert.That(collider.enabled,Is.False,
                        "Player City v1 is visual-only; interaction belongs to the independent map target.");
        }

        [UnityTest]
        public IEnumerator WorldPanIsBoundedAndKeepsFixedCameraDirection()
        {
            SceneManager.LoadScene("Frontier");
            yield return null;
            yield return null;

            var presenter=Object.FindFirstObjectByType<SlicePresenter>();
            var camera=GameObject.Find("Isometric camera")?.GetComponent<Camera>();
            Assert.That(presenter,Is.Not.Null);
            Assert.That(camera,Is.Not.Null);
            var pan=typeof(SlicePresenter).GetMethod("PanCameraByScreenDelta",BindingFlags.Instance|BindingFlags.NonPublic);
            var home=typeof(SlicePresenter).GetMethod("RecenterCamera",BindingFlags.Instance|BindingFlags.NonPublic);
            Assert.That(pan,Is.Not.Null);
            Assert.That(home,Is.Not.Null);

            var origin=camera.transform.position;
            var rotation=camera.transform.rotation;
            pan.Invoke(presenter,new object[]{new Vector2(5000f,5000f)});
            var moved=camera.transform.position;
            Assert.That(Vector3.Distance(origin,moved),Is.GreaterThan(.1f));
            Assert.That(Mathf.Abs(moved.x-origin.x),Is.LessThanOrEqualTo(10.01f));
            Assert.That(Mathf.Abs(moved.z-origin.z),Is.LessThanOrEqualTo(8.01f));
            Assert.That(Quaternion.Angle(rotation,camera.transform.rotation),Is.LessThan(.01f));

            var zoom=typeof(SlicePresenter).GetMethod("Zoom",BindingFlags.Instance|BindingFlags.NonPublic);
            Assert.That(zoom,Is.Not.Null);
            zoom.Invoke(presenter,new object[]{-100f});
            Assert.That(camera.orthographicSize,Is.EqualTo(10f).Within(.01f));
            zoom.Invoke(presenter,new object[]{100f});
            Assert.That(camera.orthographicSize,Is.EqualTo(18f).Within(.01f));

            home.Invoke(presenter,null);
            Assert.That(Vector3.Distance(camera.transform.position,origin),Is.LessThan(.01f));
            Assert.That(camera.orthographicSize,Is.EqualTo(14f).Within(.01f));
        }

        [UnityTest]
        public IEnumerator SelectingWorldResourceOpensContextBeforeDispatchingMarch()
        {
            SceneManager.LoadScene("Frontier");
            yield return null;
            yield return null;

            var presenter=Object.FindFirstObjectByType<SlicePresenter>();
            var select=typeof(SlicePresenter).GetMethod("Select",BindingFlags.Instance|BindingFlags.NonPublic);
            Assert.That(presenter,Is.Not.Null);
            Assert.That(select,Is.Not.Null);
            Assert.That(GameObject.Find("World Region 1 · active march"),Is.Null);

            select.Invoke(presenter,new object[]{"forest-valoria"});
            yield return null;

            var panelField=typeof(SlicePresenter).GetField("buildingPanel",BindingFlags.Instance|BindingFlags.NonPublic);
            Assert.That(panelField,Is.Not.Null);
            var panel=panelField.GetValue(presenter) as GameObject;
            Assert.That(panel,Is.Not.Null);
            Assert.That(panel.activeInHierarchy,Is.True);
            Text title=null;
            foreach(var label in panel.GetComponentsInChildren<Text>(true))
                if(label.name=="Building title"){title=label;break;}
            Assert.That(title,Is.Not.Null);
            Assert.That(title.text,Does.Contain("BOSQUE"));
            Assert.That(GameObject.Find("World Region 1 · active march"),Is.Null,
                "Selecting a 4X node must not silently execute the command.");
        }

        [UnityTest]
        public IEnumerator ValoriaMapCityReturnsToCityScreen()
        {
            SceneManager.LoadScene("Frontier");
            yield return null;
            yield return null;

            var presenter=Object.FindFirstObjectByType<SlicePresenter>();
            var select=typeof(SlicePresenter).GetMethod("Select",BindingFlags.Instance|BindingFlags.NonPublic);
            Assert.That(presenter,Is.Not.Null);
            select.Invoke(presenter,new object[]{"valoria-map-city"});
            yield return null;
            Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("Valoria"));
        }

        [UnityTest]
        public IEnumerator MarchVisualReflectsAuthoritativeMarchPhaseWithoutOwningReward()
        {
            var scene=SceneManager.CreateScene("WorldRegion1TestScene");
            SceneManager.SetActiveScene(scene);
            var state=new PlayerState();
            state.March.Phase="outbound";
            state.March.TargetId="forest-valoria";
            state.March.Troops.ArcherT1=10;

            WorldRegion1Runtime.Create(state);
            yield return null;
            var march=GameObject.Find("World Region 1 · active march");
            Assert.That(march,Is.Not.Null);
            var outbound=march.transform.position;

            state.March.Phase="returning";
            WorldRegion1Runtime.Refresh(state);
            yield return null;
            march=GameObject.Find("World Region 1 · active march");
            Assert.That(march,Is.Not.Null);
            Assert.That(Vector3.Distance(outbound,march.transform.position),Is.GreaterThan(.1f));

            state.March.Phase="idle";
            WorldRegion1Runtime.Refresh(state);
            yield return null;
            Assert.That(GameObject.Find("World Region 1 · active march"),Is.Null);
        }

        [UnityTest]
        public IEnumerator RebuildingRegionDoesNotLeakLooseWorldVisuals()
        {
            SceneManager.LoadScene("Frontier");
            yield return null;
            yield return null;

            int CountWorldParts()
            {
                int count=0;
                foreach(var tr in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                    if(tr.name.StartsWith("World Region 1 ·"))count++;
                return count;
            }

            var state=new PlayerState();
            WorldRegion1Runtime.Create(state);
            yield return null;
            int first=CountWorldParts();
            Assert.That(first,Is.GreaterThan(20));

            WorldRegion1Runtime.Create(state);
            yield return null;
            int second=CountWorldParts();
            Assert.That(second,Is.EqualTo(first),
                "Recreating Region 1 must replace its visual hierarchy rather than duplicate helper-created pieces.");

            int roots=0;
            foreach(var tr in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if(!tr.name.StartsWith("World Region 1 ·"))continue;
                if(tr.name=="World Region 1 · root"){roots++;continue;}
                Assert.That(tr.parent,Is.Not.Null,
                    tr.name+" escaped the Region 1 lifecycle root and would survive a rebuild.");
            }
            Assert.That(roots,Is.EqualTo(1));
        }

        static void AssertHotspot(string objectName,string id)
        {
            var go=GameObject.Find(objectName);
            Assert.That(go,Is.Not.Null,objectName+" missing");
            var hotspot=go.GetComponent<WorldHotspot>();
            Assert.That(hotspot,Is.Not.Null);
            Assert.That(hotspot.Id,Is.EqualTo(id));
            Assert.That(go.GetComponent<Collider>(),Is.Not.Null);
        }
    }
}
