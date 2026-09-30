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

        sealed class FlowClock : Eldoria.Application.IClock
        {
            public long UtcTicks { get; set; } = System.DateTime.UtcNow.Ticks;
        }
        sealed class FlowStore : Eldoria.Application.IStateStore
        {
            public Eldoria.Domain.PlayerState State;
            public Eldoria.Domain.PlayerState Load() => State;
            public void Save(Eldoria.Domain.PlayerState state) { State=state; }
        }

        [UnityTest]
        public IEnumerator OwnerFreshSaveCompletesIAndIIThroughGuidedHud()
        {
            SceneManager.LoadScene("Valoria");
            yield return null;
            var presenter=Object.FindFirstObjectByType<Eldoria.Presentation.SlicePresenter>();
            var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            var field=typeof(Eldoria.Presentation.SlicePresenter).GetField("gateway",flags);
            var previous=(Eldoria.Application.ICommandGateway)field.GetValue(presenter);
            var previousProfile=Eldoria.Domain.SliceContentProfiles.ActiveRuntimeProfile;
            Eldoria.Domain.SliceContentProfiles.SetRuntimeProfileOverride(Eldoria.Domain.SliceContentProfiles.OwnerIiiId);
            var clock=new FlowClock();
            var gateway=new Eldoria.Application.LocalGateway(clock,new FlowStore());
            presenter.Initialize(gateway);
            try
            {
                SceneManager.LoadScene("Valoria");
                yield return null;
                for(int step=0;step<40;step++)
                {
                    var state=gateway.Snapshot();
                    if(Eldoria.Domain.SliceRules.CurrentObjectiveKey(state)=="b2.complete"&&
                       state.March.Phase=="idle"&&SceneManager.GetActiveScene().name=="Valoria")break;
                    var primary=GameObject.Find("CONTINUAR").GetComponent<Button>();
                    Assert.That(primary.interactable,Is.True,"Guided action stalled at "+Eldoria.Domain.SliceRules.CurrentObjectiveKey(state));
                    primary.onClick.Invoke();
                    yield return null;
                    var panel=GameObject.Find("Building interaction panel");
                    if(panel!=null&&panel.activeInHierarchy)
                    {
                        Assert.That(panel.transform.parent.name,Is.EqualTo("Safe area"));
                        var action=GameObject.Find("Building action").GetComponent<Button>();
                        Assert.That(action.interactable,Is.True,"Building action must be usable at the guided step.");
                        action.onClick.Invoke();
                    }
                    clock.UtcTicks+=System.TimeSpan.FromSeconds(30).Ticks;
                    gateway.Advance();
                    typeof(Eldoria.Presentation.SlicePresenter).GetMethod("Refresh",flags).Invoke(presenter,null);
                    yield return null;
                }
                var final=gateway.Snapshot();
                Assert.That(final.BastionLevel,Is.EqualTo(2));
                Assert.That(final.SawmillLevel,Is.EqualTo(1));
                Assert.That(final.BarracksLevel,Is.EqualTo(1));
                Assert.That(final.ChapterProgress.GatheredWood,Is.GreaterThanOrEqualTo(600));
                Assert.That(final.ChapterProgress.GatheredStone,Is.GreaterThanOrEqualTo(500));
                Assert.That(final.ChapterProgress.TrainedArchers,Is.EqualTo(20));
                Assert.That(final.MarchConfigured,Is.True);
                Assert.That(final.EngendroDefeated,Is.True);
                Assert.That(final.March.Phase,Is.EqualTo("idle"));
                Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("Valoria"));
            }
            finally
            {
                Eldoria.Domain.SliceContentProfiles.SetRuntimeProfileOverride(previousProfile);
                presenter.Initialize(previous);
                SceneManager.LoadScene("Valoria");
            }
        }

        [UnityTest]
        public IEnumerator ResourceStarvedSawmillGuidanceDepartsAndActuallyGathers()
        {
            SceneManager.LoadScene("Valoria");
            yield return null;
            var presenter=Object.FindFirstObjectByType<Eldoria.Presentation.SlicePresenter>();
            var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            var previous=(Eldoria.Application.ICommandGateway)typeof(Eldoria.Presentation.SlicePresenter).GetField("gateway",flags).GetValue(presenter);
            var previousProfile=Eldoria.Domain.SliceContentProfiles.ActiveRuntimeProfile;
            Eldoria.Domain.SliceContentProfiles.SetRuntimeProfileOverride(Eldoria.Domain.SliceContentProfiles.QaFastId);
            var gateway=new Eldoria.Application.LocalGateway(new FlowClock(),new FlowStore());
            presenter.Initialize(gateway);
            try
            {
                SceneManager.LoadScene("Valoria");
                yield return null;
                GameObject.Find("CONTINUAR").GetComponent<Button>().onClick.Invoke();
                yield return null;
                Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("Frontier"));
                var primary=GameObject.Find("CONTINUAR").GetComponent<Button>();
                Assert.That(primary.GetComponentInChildren<UnityEngine.UI.Text>().text,Is.EqualTo("RECOLECTAR MADERA"));
                primary.onClick.Invoke();
                Assert.That(gateway.Snapshot().March.TargetId,Is.EqualTo("forest-valoria"));
                Assert.That(gateway.Snapshot().March.Phase,Is.EqualTo("outbound"));
            }
            finally
            {
                Eldoria.Domain.SliceContentProfiles.SetRuntimeProfileOverride(previousProfile);
                presenter.Initialize(previous);
                SceneManager.LoadScene("Valoria");
            }
        }
    }
}
