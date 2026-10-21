using System;
using Eldoria.Application;
using Eldoria.Domain;

internal static class Program
{
    sealed class Clock : IClock
    {
        public long UtcTicks { get; set; } =
            new DateTime(2026,10,21,12,0,0,DateTimeKind.Utc).Ticks;
        public void Add(int seconds) { UtcTicks += TimeSpan.FromSeconds(seconds).Ticks; }
    }
    sealed class Store : IStateStore
    {
        public PlayerState Value;
        public PlayerState Load() => Value;
        public void Save(PlayerState next) { Value = next; }
    }
    static int checks;
    static void Check(bool passed,string name)
    {
        checks++;
        if(!passed)throw new Exception("FAIL "+name);
        Console.WriteLine("PASS "+name);
    }
    static PlayerState Ready()
    {
        return new PlayerState{
            BastionLevel=2,BarracksLevel=1,
            Resources=new ResourceWallet{Wood=9000,Stone=9000},
            Available=new ArmyRoster{ArcherT1=36},
            ChapterProgress=new ChapterProgressState{
                GrantedRewards=new System.Collections.Generic.List<string>{
                    "c1-bastion","chapter-1","c2-barracks"}}};
    }
    static GameCommand Cmd(LocalGateway g,string id,string kind,string target)
    {
        var s=g.Snapshot();
        return new GameCommand(id,s.PlayerId,kind,target,s.Revision);
    }
    static void Main()
    {
        SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
        Check(SliceRules.RecruitSeconds==600,"legacy default batch uses the same true OWNER deadline");
        Check(SliceRules.RecruitSecondsFor(10)==300,"10 T1 = 5 minutes");
        Check(SliceRules.RecruitSecondsFor(20)==600,"20 T1 = 10 minutes");
        Check(SliceRules.RecruitSecondsFor(30)==900,"30 T1 = 15 minutes");
        Check(SliceRules.RecruitWoodCostFor(10)==200,"10 T1 wood");
        Check(SliceRules.RecruitStoneCostFor(30)==360,"30 T1 stone");
        var funds=Ready();funds.Resources.Wood=420;funds.Resources.Stone=250;
        Check(SliceRules.MaxAffordableArchers(funds)==20,"resources cap selection");
        var clock=new Clock();var store=new Store{Value=Ready()};
        var g=new LocalGateway(clock,store);
        var once=Cmd(g,"idempotent","Recruit","archer:t1:10");
        Check(g.Execute(once).Ok,"first training command accepted");
        var active=g.Snapshot();
        Check(active.Resources.Wood==8800&&active.Resources.Stone==8880,"one resource debit");
        Check(active.PendingRecruitArchers==10,"exact recruits pending");
        Check(active.RecruitmentCompletesUtcTicks-clock.UtcTicks==TimeSpan.FromMinutes(5).Ticks,"persistent deadline");
        Check(g.Execute(once).Ok&&g.Snapshot().Resources.Wood==8800,"duplicate command no second debit");
        Check(!g.Execute(Cmd(g,"too-soon","Recruit","archer:t1:1")).Ok,"overlapping training rejected");
        clock.Add(299);g=new LocalGateway(clock,store);
        Check(g.Snapshot().Available.ArcherT1==36,"offline pending until deadline");
        clock.Add(1);g=new LocalGateway(clock,store);
        Check(g.Snapshot().Available.ArcherT1==46,"offline completion grants exactly 10");
        Check(g.Snapshot().PendingRecruitArchers==0&&g.Snapshot().RecruitmentCompletesUtcTicks==0,"queue settles");
        Check(g.Snapshot().ChapterProgress.TrainedArchers==10,"tutorial credited");
        g.Advance();g=new LocalGateway(clock,store);
        Check(g.Snapshot().Available.ArcherT1==46,"reload never duplicates troops");
        var selected=Cmd(g,"selected-march","ConfigureMarch","march-main:7");
        Check(g.Execute(selected).Ok,"partial march accepted");
        var m=g.Snapshot();
        Check(m.PreparedTroops.ArcherT1==7&&m.Available.ArcherT1==46,"march reserves composition only, without debit");
        Check(m.PreparedHeroId=="aldric"&&m.ChapterProgress.ConfirmedExpeditionPower==SliceRules.Expedition(m.PreparedTroops,"aldric").Power,"Aldric power is real");
        Check(!g.Execute(Cmd(g,"march-invalid","ConfigureMarch","march-main:999")).Ok,"invalid march rejected");
        var trained=Ready();trained.ChapterProgress.TrainedArchers=20;trained.EngendroDefeated=true;
        var g2=new LocalGateway(new Clock(),new Store{Value=trained});
        Check(g2.Execute(Cmd(g2,"after-tutorial","Recruit","archer:t1:30")).Ok,"recruitment remains available after chapter II");
        var blocked=Ready();blocked.Resources.Wood=200;blocked.Resources.Stone=120;
        var g3=new LocalGateway(new Clock(),new Store{Value=blocked});
        Check(!g3.Execute(Cmd(g3,"too-many","Recruit","archer:t1:11")).Ok,"over-budget rejected");
        Check(!g3.Execute(Cmd(g3,"zero","Recruit","archer:t1:0")).Ok,"zero recruitment rejected");
        Check(!g3.Execute(Cmd(g3,"unknown","Recruit","archer:t1:abc")).Ok,"malformed recruitment rejected");
        Check(g3.Snapshot().Resources.Wood==200,"invalid requests spend nothing");
        var legacy=Ready();legacy.PendingRecruitArchers=20;legacy.RecruitmentTaskId="recruit:legacy";
        var lclock=new Clock();legacy.RecruitmentCompletesUtcTicks=lclock.UtcTicks+TimeSpan.FromSeconds(7).Ticks;
        var old=new Store{Value=legacy};lclock.Add(7);
        var existing=new LocalGateway(lclock,old);
        Check(existing.Snapshot().SchemaVersion==1,"save schema compatible");
        Check(existing.Snapshot().Available.ArcherT1==56,"legacy in-flight task recovered");
        Check(existing.Snapshot().CompletedTaskIds.Contains("recruit:legacy"),"legacy task marked exactly once");
        SliceContentProfiles.SetRuntimeProfileOverride(null);
        Console.WriteLine("ALL_PASS count="+checks);
    }
}
