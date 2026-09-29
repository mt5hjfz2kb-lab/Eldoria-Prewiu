using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Eldoria.Tests
{
    public sealed class ReferenceUiArtPassTests
    {
        [UnityTest]
        public IEnumerator ApprovedReferenceSkinDecoratesPlayerHudWithoutReplacingControls()
        {
            SceneManager.LoadScene("Valoria");
            yield return null;
            yield return new WaitForSecondsRealtime(.65f);

            var top=GameObject.Find("Reference topbar");
            var quest=GameObject.Find("Quest panel");
            var dock=GameObject.Find("World objective dock");
            var nav=GameObject.Find("Bottom navigation");
            Assert.That(top,Is.Not.Null);
            Assert.That(quest,Is.Not.Null);
            Assert.That(dock,Is.Not.Null);
            Assert.That(nav,Is.Not.Null);
            Assert.That(top.transform.Find("ReferenceArtFrame"),Is.Not.Null);
            Assert.That(quest.transform.Find("ReferenceArtFrame"),Is.Not.Null);
            Assert.That(dock.transform.Find("ReferenceArtFrame"),Is.Not.Null);
            Assert.That(nav.transform.Find("ReferenceArtFrame"),Is.Not.Null);

            var primary=GameObject.Find("CONTINUAR");
            Assert.That(primary,Is.Not.Null);
            Assert.That(primary.GetComponent<Button>(),Is.Not.Null);
            Assert.That(primary.transform.Find("ReferenceButtonFrame"),Is.Not.Null);
            Assert.That(primary.GetComponent<Outline>(),Is.Not.Null);
        }
    }
}
