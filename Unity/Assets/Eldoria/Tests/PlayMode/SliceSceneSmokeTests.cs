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
            Assert.That(assets.MasonryWall,Is.Not.Null);
            Assert.That(assets.MasonryTower,Is.Not.Null);
            Assert.That(assets.MasonryGate,Is.Not.Null);
            Assert.That(assets.CobbleRoad,Is.Not.Null);
            Assert.That(assets.SlavicBush,Is.Not.Null);
            Assert.That(assets.SlavicCobbleRoad,Is.Not.Null);
            Assert.That(assets.SlavicStoneFence,Is.Not.Null);
            Assert.That(assets.SlavicRockGate,Is.Not.Null);
            Assert.That(assets.SlavicBoulder,Is.Not.Null);
            SceneManager.LoadScene("Valoria");
            yield return null;
            Assert.That(GameObject.Find("Aserradero · carpentry shed"),Is.Null,
                "The detached imported shed failed the visual capture review and must stay out");
            Assert.That(GameObject.Find("Sawmill · masonry"),Is.Not.Null);
            Assert.That(GameObject.Find("Cuartel · casa de guardia"),Is.Null,
                "The detached imported guardhouse failed the visual capture review and must stay out");
            Assert.That(GameObject.Find("Early barracks · masonry"),Is.Not.Null);
            Assert.That(GameObject.Find("Valoria · damaged outer tower"),Is.Null,
                "Loose ruin fragments must not return to the benchmark composition");
            Assert.That(GameObject.Find("Bastion · masonry gate"),Is.Null,
                "Detached modular masonry must not return until a coherent castle set is selected");
            Assert.That(GameObject.Find("Bastion · front curtain · wall"),Is.Not.Null);
            Assert.That(GameObject.Find("Bastion · dead palace arch · left pier"),Is.Not.Null);
            Assert.That(GameObject.Find("Valoria · lower stone gate"),Is.Not.Null);
            Assert.That(GameObject.Find("Valoria · authored cobble route"),Is.Not.Null);
            Assert.That(GameObject.Find("Valoria · authored cliff boulder"),Is.Not.Null);
            Assert.That(GameObject.Find("Ruined imperial arch · left pier"),Is.Not.Null);
            Assert.That(GameObject.Find("Ruined imperial causeway · deck 0"),Is.Null,
                "The oversized causeway failed capture review and must stay out of the city silhouette");
            Assert.That(GameObject.Find("Bastion · authored gate"),Is.Null,
                "The blue blockout castle should not be part of the art benchmark");
            Assert.That(GameObject.Find("Brecha · burned earth"),Is.Not.Null);
            Assert.That(GameObject.Find("Fractured Breach shard"),Is.Null);
            Assert.That(GameObject.Find("Valoria smoke · sawmill · wisp 0"),Is.Null,
                "Sphere smoke is placeholder art and should not be in the benchmark");
            var hotspot=GameObject.Find("Aserradero · interacción");
            Assert.That(hotspot.GetComponent<Collider>().enabled,Is.True);
            Assert.That(hotspot.GetComponent<WorldHotspot>().Id,Is.EqualTo("sawmill"));
        }
    }
}
