// Exact native Unity EditMode gates for the isolated playable I–V experiment.
// These are tests of real LocalGateway transitions and serialized Unity source.
// They do not claim iPhone performance, UI taps, or subjective visual approval.
using System;
using System.Linq;
using NUnit.Framework;
using Eldoria.Application;
using Eldoria.Domain;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eldoria.ExperimentalAshenCitadel.Tests
{
    [TestFixture]
    public sealed class AshenCitadelProgressionTests
    {
        private sealed class Clock : IClock
        {
            public long Now=DateTime.UtcNow.Ticks;
            public long UtcTicks => Now;
        }
        private sealed class MemoryStore : IStateStore
        {
            private PlayerState held;
            public MemoryStore(PlayerState state){held=state;}
            public PlayerState Load() => held;
            public void Save(PlayerState snapshot){held=snapshot;}
        }
        [SetUp]
        public void SetProfile() =>
            SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);

        [TearDown]
        public void ResetProfile() =>
            SliceContentProfiles.SetRuntimeProfileOverride(null);

        private static PlayerState Prepared(int level)
        {
            var state=new PlayerState
            {
                BastionLevel=level,
                JourneyComplete=true,
                SawmillLevel=2,
                BarracksLevel=2
            };
            state.Resources.Wood=10000;
            state.Resources.Stone=10000;
            state.Resources.Food=10000;
            state.Available.ArcherT1=100;
            state.ChapterThree.GranaryLevel=2;
            state.ChapterThree.FissureClosed=true;
            state.ChapterThree.DevelopmentPriority="production";
            return state;
        }

        [Test]
        public void FreshBastionCannotSkipChapterOne()
        {
            var current=new PlayerState();
            var clock=new Clock();
            var gateway=new LocalGateway(clock,new MemoryStore(current));
            var s=gateway.Snapshot();
            Assert.That(s.BastionLevel,Is.EqualTo(1));
            var command=new GameCommand(Guid.NewGuid().ToString("N"),
                s.PlayerId,"AdvanceBastion","bastion",s.Revision);
            var result=gateway.Execute(command);
            Assert.That(result.Ok,Is.False,"I→II must keep objective and cost gates");
            Assert.That(gateway.Snapshot().BastionLevel,Is.EqualTo(1));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void AllFourAscensionsUseOriginalCostsAndSavedTimers(int startingLevel)
        {
            var state=Prepared(startingLevel);
            var clock=new Clock();
            var save=new MemoryStore(state);
            var gateway=new LocalGateway(clock,save);
            var before=gateway.Snapshot();
            var next=BastionProgressionCatalog.ForLevel(startingLevel+1);
            Assert.That(BastionProgressionCatalog.RequirementsMet(before,next),Is.True,
                "The authentic progression catalog must recognize valid prerequisites");
            var result=gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),
                before.PlayerId,"AdvanceBastion","bastion",before.Revision));
            Assert.That(result.Ok,Is.True,result.Message);
            var during=gateway.Snapshot();
            Assert.That(during.BastionLevel,Is.EqualTo(startingLevel),
                "Level must not change before its real construction timer completes");
            Assert.That(during.Resources.Wood,Is.EqualTo(before.Resources.Wood-next.WoodCost));
            Assert.That(during.Resources.Stone,Is.EqualTo(before.Resources.Stone-next.StoneCost));
            Assert.That(during.Resources.Food,Is.EqualTo(before.Resources.Food-next.FoodCost));
            Assert.That(during.BastionUpgradeCompletesUtcTicks,
                Is.GreaterThan(clock.Now));
            clock.Now=during.BastionUpgradeCompletesUtcTicks+TimeSpan.TicksPerSecond;
            gateway.Advance();
            var after=gateway.Snapshot();
            Assert.That(after.BastionLevel,Is.EqualTo(startingLevel+1));
            Assert.That(after.BastionUpgradeCompletesUtcTicks,Is.Zero);
            Assert.That(after.BastionUpgradeTaskId,Is.Empty);
            var recovered=new LocalGateway(clock,save);
            Assert.That(recovered.Snapshot().BastionLevel,Is.EqualTo(startingLevel+1),
                "Reopening the same authoritative save must preserve the level");
        }

        [Test]
        public void BastionIVRejectsMissingFissureWithoutSpendingResources()
        {
            var state=Prepared(3);
            state.ChapterThree.FissureClosed=false;
            var clock=new Clock();
            var gateway=new LocalGateway(clock,new MemoryStore(state));
            var before=gateway.Snapshot();
            var response=gateway.Execute(new GameCommand(Guid.NewGuid().ToString("N"),
                before.PlayerId,"AdvanceBastion","bastion",before.Revision));
            Assert.That(response.Ok,Is.False);
            Assert.That(gateway.Snapshot().BastionLevel,Is.EqualTo(3));
            Assert.That(gateway.Snapshot().Resources.Wood,Is.EqualTo(before.Resources.Wood));
        }

        [Test]
        public void AuthoredSceneContainsAllStagesAndPhysicalMeshes()
        {
            const string expected="Assets/Eldoria/ExperimentalAshenCitadel/Scenes/AshenCitadel.unity";
            var scene=EditorSceneManager.OpenScene(expected,OpenSceneMode.Single);
            Assert.That(scene.IsValid(),Is.True);
            Assert.That(scene.path,Is.EqualTo(expected));
            var stageParent=scene.GetRootGameObjects().SelectMany(
                x=>x.GetComponentsInChildren<Transform>(true)).FirstOrDefault(
                x=>x.name=="Bastion stage variants");
            Assert.That(stageParent,Is.Not.Null);
            Assert.That(stageParent.childCount,Is.EqualTo(5));
            for(int level=1;level<=5;level++)
            {
                var stage=stageParent.Find("Stage_"+level);
                Assert.That(stage,Is.Not.Null,"Every architectural phase must have its own 3D source");
                var meshes=stage.GetComponentsInChildren<MeshFilter>(true);
                Assert.That(meshes.Length,Is.GreaterThan(0));
                Assert.That(meshes.Sum(m=>m.sharedMesh==null?0:m.sharedMesh.vertexCount),
                    Is.GreaterThan(100));
                var renderer=stage.GetComponentInChildren<MeshRenderer>(true);
                Assert.That(renderer,Is.Not.Null);
                foreach(var mat in renderer.sharedMaterials)
                {
                    Assert.That(mat,Is.Not.Null);
                    Assert.That(mat.shader.name,Does.Contain("Universal Render Pipeline"));
                }
                Assert.That(stage.GetComponent<BoxCollider>(),Is.Not.Null,
                    "The stage must be touch-addressable without cooked MeshColliders");
            }
            Assert.That(scene.GetRootGameObjects().Any(x=>x.name.StartsWith(
                "Ashen Frontier")),Is.True);
        }
    }
}
