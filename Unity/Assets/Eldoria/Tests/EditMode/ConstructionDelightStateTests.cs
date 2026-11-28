using Eldoria.Domain;
using Eldoria.Presentation;
using NUnit.Framework;

namespace Eldoria.Tests
{
    public sealed class ConstructionDelightStateTests
    {
        [Test] public void SawmillRequiresTrueCompletionRatherThanTimeExpiry()
        {
            var s=new PlayerState{SawmillLevel=0,BuildingTaskId="sawmill:job",BuildingCompletesUtcTicks=3210};
            Assert.IsTrue(ConstructionDelightState.TryRead(s,out var task));
            Assert.AreEqual("sawmill",task.BuildingId);
            Assert.AreEqual(0,task.LevelAtStart);
            Assert.IsFalse(ConstructionDelightState.JustCompleted(task,s));
            s.BuildingTaskId="";s.BuildingCompletesUtcTicks=0;
            Assert.IsFalse(ConstructionDelightState.JustCompleted(task,s));
            s.SawmillLevel=1;
            Assert.IsFalse(ConstructionDelightState.JustCompleted(task,s),
                "Uncorroborated level change must not simulate a completed construction.");
            s.CompletedTaskIds.Add("different:task");
            Assert.IsFalse(ConstructionDelightState.JustCompleted(task,s));
            s.CompletedTaskIds.Add(task.TaskKey);
            Assert.IsTrue(ConstructionDelightState.JustCompleted(task,s));
        }

        [Test] public void ChapterThreeUpgradeKeepsTheRealBuildingAndLevel()
        {
            var s=new PlayerState{SawmillLevel=2,BuildingTaskId="c3:sawmill:upgrade",BuildingCompletesUtcTicks=4321};
            Assert.IsTrue(ConstructionDelightState.TryRead(s,out var task));
            Assert.AreEqual(2,task.LevelAtStart);
            s.BuildingCompletesUtcTicks=0;s.BuildingTaskId="";s.SawmillLevel=3;
            s.CompletedTaskIds.Add(task.TaskKey);
            Assert.IsTrue(ConstructionDelightState.JustCompleted(task,s));
        }

        [Test] public void BastionAndEconomyBuildingsShareTheCorrectRoute()
        {
            var s=new PlayerState{BastionLevel=2,BastionUpgradeTaskId="bastion3:job",
                BastionUpgradeCompletesUtcTicks=9000};
            Assert.IsTrue(ConstructionDelightState.TryRead(s,out var t));
            Assert.AreEqual("bastion",t.BuildingId);
            s.BastionUpgradeTaskId="";s.BastionUpgradeCompletesUtcTicks=0;s.BastionLevel=3;
            s.CompletedTaskIds.Add(t.TaskKey);
            Assert.IsTrue(ConstructionDelightState.JustCompleted(t,s));
            s.BuildingTaskId="c3:granary:job";s.BuildingCompletesUtcTicks=50;
            Assert.IsTrue(ConstructionDelightState.TryRead(s,out t));
            Assert.AreEqual("granary",t.BuildingId);
            s.BuildingTaskId="c3:stoneworks:job";
            Assert.IsTrue(ConstructionDelightState.TryRead(s,out t));
            Assert.AreEqual("stoneworks",t.BuildingId);
        }

        [Test] public void TrainingOrUnrecognizedTaskCannotCreateVisualCelebration()
        {
            var s=new PlayerState{RecruitmentCompletesUtcTicks=500,
                RecruitmentTaskId="recruit:t1"};
            Assert.IsFalse(ConstructionDelightState.TryRead(s,out _));
            s.BuildingTaskId="c3:unknown:job";s.BuildingCompletesUtcTicks=100;
            Assert.IsFalse(ConstructionDelightState.TryRead(s,out _));
            s.BuildingTaskId="c3:granary";
            Assert.IsFalse(ConstructionDelightState.TryRead(s,out _));
        }
    }
}
