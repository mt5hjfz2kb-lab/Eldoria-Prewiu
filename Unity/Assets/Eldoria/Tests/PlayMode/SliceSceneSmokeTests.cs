using System.Collections;
using Eldoria.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Eldoria.Tests
{
    public sealed class SliceSceneSmokeTests
    {
        [UnityTest]
        public IEnumerator ValoriaAndFrontierExposeVisibleSceneAndActions()
        {
            SceneManager.LoadScene("Valoria");
            yield return null;
            Assert.That(Camera.main,Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<SlicePresenter>(),Is.Not.Null);
            Assert.That(Object.FindObjectsByType<WorldHotspot>(FindObjectsSortMode.None).Length,Is.GreaterThan(1));
            SceneManager.LoadScene("Frontier");
            yield return null;
            Assert.That(Camera.main,Is.Not.Null);
            Assert.That(Object.FindObjectsByType<WorldHotspot>(FindObjectsSortMode.None).Length,Is.GreaterThan(1));
        }

        [UnityTest]
        public IEnumerator ValoriaPlayableDistrictSkeletonHasReadableLayersAndRealBuildingPanels()
        {
            SceneManager.LoadScene("Valoria");
            yield return null;
            var camera=Camera.main;
            var presenter=Object.FindFirstObjectByType<SlicePresenter>();
            Assert.That(camera,Is.Not.Null);
            Assert.That(presenter,Is.Not.Null);

            Assert.That(GameObject.Find("VPD · continuous terrain"),Is.Not.Null);
            Assert.That(GameObject.Find("VPD · L0 civic floor"),Is.Not.Null);
            Assert.That(GameObject.Find("VPD · L0 main street"),Is.Not.Null);
            Assert.That(GameObject.Find("VPD · vertical stair 1"),Is.Not.Null);
            Assert.That(GameObject.Find("VPD · vertical stair 12"),Is.Not.Null);
            Assert.That(GameObject.Find("VPD · L1 landing"),Is.Not.Null);
            Assert.That(GameObject.Find("VPD · L1 west plot"),Is.Not.Null);
            Assert.That(GameObject.Find("VPD · L1 east plot"),Is.Not.Null);

            var select=typeof(SlicePresenter).GetMethod("Select",
                System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            Assert.That(select,Is.Not.Null);

            foreach(var pair in new[]{("Aserradero · target","sawmill","ASERRADERO"),
                                      ("Cuartel · target","barracks","CUARTEL"),
                                      ("Bastion · target","bastion","BASTIÓN")})
            {
                var target=GameObject.Find(pair.Item1);
                Assert.That(target,Is.Not.Null,pair.Item1+" missing");
                var hotspot=target.GetComponent<WorldHotspot>();
                Assert.That(hotspot,Is.Not.Null);
                Assert.That(hotspot.Id,Is.EqualTo(pair.Item2));
                var aim=target.GetComponent<Collider>().bounds.center;
                var ray=new Ray(camera.transform.position,(aim-camera.transform.position).normalized);
                Assert.That(Physics.Raycast(ray,out var hit,100f),Is.True,pair.Item1+" is not raycastable");
                var resolved=hit.collider.GetComponent<WorldHotspot>();
                Assert.That(resolved,Is.Not.Null,pair.Item1+" click path hit non-interactive visible geometry");
                Assert.That(resolved.Id,Is.EqualTo(pair.Item2),
                    pair.Item1+" click path resolved the wrong building");
                select.Invoke(presenter,new object[]{pair.Item2});
                yield return null;
                var panel=GameObject.Find("Building interaction panel");
                Assert.That(panel,Is.Not.Null);
                Assert.That(panel.activeInHierarchy,Is.True);
                var title=GameObject.Find("Building title").GetComponent<UnityEngine.UI.Text>();
                Assert.That(title.text,Does.Contain(pair.Item3));
                panel.SetActive(false);
            }
        }

    }
}
