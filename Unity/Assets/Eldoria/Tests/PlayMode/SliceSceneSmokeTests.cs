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
        public IEnumerator ValoriaBenchmarkArtKeepsGameplayHotspots()
        {
            var assets=ValoriaExternalAssetLibrary.Load();
            Assert.That(assets,Is.Not.Null);
            Assert.That(assets.SlavicShed,Is.Not.Null,"Shed prefab reference did not survive import");
            Assert.That(assets.SlavicHouse,Is.Not.Null,"Guardhouse prefab reference did not survive import");
            Assert.That(assets.SlavicTree,Is.Not.Null);
            Assert.That(assets.RuinedTower,Is.Not.Null);
            Assert.That(assets.Firewood,Is.Not.Null);
            SceneManager.LoadScene("Valoria");
            yield return null;
            var art=GameObject.Find("Aserradero · carpentry shed");
            Assert.That(art,Is.Not.Null,"Visual fallback was used despite imported assets");
            Assert.That(art.GetComponentsInChildren<Renderer>().Length,Is.GreaterThan(0));
            foreach(var collider in art.GetComponentsInChildren<Collider>())
                Assert.That(collider.enabled,Is.False,"Visual prefab must not intercept taps");
            foreach(var renderer in art.GetComponentsInChildren<Renderer>())
                foreach(var material in renderer.sharedMaterials)
                    Assert.That(material!=null&&material.shader!=null&&material.shader.isSupported,Is.True,
                        "Benchmark material has an unsupported shader");
            var ruinedTower=GameObject.Find("Valoria · damaged outer tower");
            Assert.That(ruinedTower,Is.Not.Null);
            foreach(var renderer in ruinedTower.GetComponentsInChildren<Renderer>())
                foreach(var material in renderer.sharedMaterials)
                    Assert.That(material.shader.name,Does.StartWith("Universal Render Pipeline/"));
            var hotspot=GameObject.Find("Aserradero · interacción");
            Assert.That(hotspot.GetComponent<Collider>().enabled,Is.True);
            Assert.That(hotspot.GetComponent<WorldHotspot>().Id,Is.EqualTo("sawmill"));
        }
    }
}
