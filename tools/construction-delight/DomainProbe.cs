using System;
using Eldoria.Presentation;

namespace Eldoria.Domain
{
    // Minimal test-only stand-in. The production ConstructionDelightState.cs
    // is linked verbatim from Unity and is never modified or copied here.
    public sealed class BastionThreeState { public int GranaryLevel, StoneworksLevel; }
    public sealed class PlayerState
    {
        public int SawmillLevel, BarracksLevel, BastionLevel=1;
        public string BuildingTaskId="", BastionUpgradeTaskId="", RecruitmentTaskId="";
        public long BuildingCompletesUtcTicks, BastionUpgradeCompletesUtcTicks, RecruitmentCompletesUtcTicks;
        public BastionThreeState ChapterThree=new BastionThreeState();
        public System.Collections.Generic.List<string> CompletedTaskIds=new System.Collections.Generic.List<string>();
    }
}

internal static class DomainProbe
{
    static int passed;
    static void Expect(bool condition, string message)
    {
        if(!condition) throw new InvalidOperationException("CONSTRUCTION_DELIGHT_DOMAIN_FAIL: "+message);
        passed++;
    }

    static void Main()
    {
        var s=new Eldoria.Domain.PlayerState();
        Expect(!ConstructionDelightState.TryRead(s,out _),"fresh save must have no active building");
        Expect(ConstructionDelightState.Level(s,"not-real")==0,"invalid building has no level");
        Expect(ConstructionDelightState.Level(null,"sawmill")==0,"null state safe");
        Expect(!ConstructionDelightState.TryRead(null,out _),"null snapshot safe");

        s.BuildingTaskId="sawmill:original";
        s.BuildingCompletesUtcTicks=1000;
        Expect(ConstructionDelightState.TryRead(s,out var original),"real sawmill task");
        Expect(original.BuildingId=="sawmill"&&original.LevelAtStart==0,"sawmill key and level");
        Expect(!ConstructionDelightState.JustCompleted(original,s),"timer active cannot complete");
        s.BuildingCompletesUtcTicks=800;
        Expect(ConstructionDelightState.TryRead(s,out var accelerated),"speedup retains task");
        Expect(ConstructionDelightState.SameTask(original,accelerated),"speedup not a new build");
        s.BuildingTaskId="";s.BuildingCompletesUtcTicks=0;
        Expect(!ConstructionDelightState.JustCompleted(original,s),"time expiry without level not celebrated");
        s.SawmillLevel=1;
        Expect(!ConstructionDelightState.JustCompleted(original,s),
            "level increase without canonical task receipt must not be celebrated");
        s.CompletedTaskIds.Add("different-sawmill:job");
        Expect(!ConstructionDelightState.JustCompleted(original,s),
            "wrong task receipt cannot celebrate original");
        s.CompletedTaskIds.Add(original.TaskKey);
        Expect(ConstructionDelightState.JustCompleted(original,s),
            "canonical completion receipt plus level increase celebrated");

        s.BuildingTaskId="barracks:build";s.BuildingCompletesUtcTicks=1200;
        Expect(ConstructionDelightState.TryRead(s,out var barracks),"real barracks build task");
        Expect(barracks.BuildingId=="barracks","barracks route retained");
        Expect(!ConstructionDelightState.SameTask(original,barracks),"two buildings not same task");
        s.BuildingTaskId="";s.BuildingCompletesUtcTicks=0;
        s.BarracksLevel=1;
        s.CompletedTaskIds.Add(barracks.TaskKey);
        Expect(ConstructionDelightState.JustCompleted(barracks,s),"barracks completion");

        s.BuildingTaskId="c3:sawmill:upgrade-2";s.BuildingCompletesUtcTicks=1400;
        Expect(ConstructionDelightState.TryRead(s,out var sawUpgrade),"chapter III sawmill upgrade");
        Expect(sawUpgrade.LevelAtStart==1,"upgrade remembers real level");
        s.BuildingTaskId="";s.BuildingCompletesUtcTicks=0;s.SawmillLevel=2;
        s.CompletedTaskIds.Add(sawUpgrade.TaskKey);
        Expect(ConstructionDelightState.JustCompleted(sawUpgrade,s),"sawmill upgrade completes on real level");

        foreach(string id in new[]{"granary","stoneworks","barracks"})
        {
            s.BuildingTaskId="c3:"+id+":job";s.BuildingCompletesUtcTicks=1500;
            Expect(ConstructionDelightState.TryRead(s,out var building),"chapter III "+id);
            Expect(building.BuildingId==id,"chapter III id "+id);
            s.BuildingTaskId="";s.BuildingCompletesUtcTicks=0;
            if(id=="granary")s.ChapterThree.GranaryLevel++;
            if(id=="stoneworks")s.ChapterThree.StoneworksLevel++;
            if(id=="barracks")s.BarracksLevel++;
            s.CompletedTaskIds.Add(building.TaskKey);
            Expect(ConstructionDelightState.JustCompleted(building,s),"chapter III increase "+id);
        }

        s.BastionUpgradeTaskId="bastion3:real-job";s.BastionUpgradeCompletesUtcTicks=2000;
        Expect(ConstructionDelightState.TryRead(s,out var bastion),"bastion upgrade");
        Expect(bastion.BuildingId=="bastion"&&bastion.LevelAtStart==1,"bastion base level");
        s.BastionUpgradeTaskId="";s.BastionUpgradeCompletesUtcTicks=0;s.BastionLevel=2;
        s.CompletedTaskIds.Add(bastion.TaskKey);
        Expect(ConstructionDelightState.JustCompleted(bastion,s),"bastion true completion");

        s.RecruitmentTaskId="recruit:t1";s.RecruitmentCompletesUtcTicks=2200;
        Expect(!ConstructionDelightState.TryRead(s,out _),"training never a construction");
        s.BuildingTaskId="c3:unknown:job";s.BuildingCompletesUtcTicks=3000;
        Expect(!ConstructionDelightState.TryRead(s,out _),"unknown id ignored");
        s.BuildingTaskId="c3:granary"; 
        Expect(!ConstructionDelightState.TryRead(s,out _),"malformed chapter III key ignored");
        s.BuildingTaskId="c3:granary:job";
        s.BuildingCompletesUtcTicks=0;
        Expect(!ConstructionDelightState.TryRead(s,out _),"stale timer ignored");
        Console.WriteLine("CONSTRUCTION_DELIGHT_DOMAIN_PASS assertions="+passed+" linked_source=true");
    }
}
