using System;
using System.Linq;
using System.Text.Json;
using Eldoria.Domain;
using Eldoria.Application;

// Continuous domain regression, NOT a native visual or real-device certificate.
// Starts from an empty store. All progression, resources and troops are earned
// through canonical commands. Only time and terrain placement are test adapters.
class ContinuousJourney
{
    sealed class Clock : IClock
    {
        public long UtcTicks { get; set; } = new DateTime(2026,10,30,0,0,0,DateTimeKind.Utc).Ticks;
        public void Tick() => UtcTicks += TimeSpan.TicksPerSecond;
    }
    sealed class Store : IStateStore
    {
        string json;
        static readonly JsonSerializerOptions Options = new() { IncludeFields = true };
        public PlayerState Load() => json == null ? null : JsonSerializer.Deserialize<PlayerState>(json, Options);
        public void Save(PlayerState state) => json = JsonSerializer.Serialize(state, Options);
    }
    sealed class Sites : IWorldResourcePlacement
    {
        public bool TryChoose(PlayerState state, string id, string previous, int generation, out WorldResourceLocation site)
        {
            site = new WorldResourceLocation { TargetId=id, SiteId=id+":"+generation, X=10+generation, Y=0, Z=20 };
            return true;
        }
    }
    static Clock clock = new();
    static Store store = new();
    static LocalGateway game;
    static int commands, reloads;
    static void Check(bool value, string text) { if (!value) throw new Exception(text); Console.WriteLine("PASS " + text); }
    static void Reload()
    {
        game = new LocalGateway(clock, store);
        // Same production gateway respawn API; no direct save edits.
        game.ConfigureLocalResourceRespawn(new Sites(), TimeSpan.FromSeconds(45));
        reloads++;
    }
    static void Do(string kind, string target)
    {
        var state=game.Snapshot();
        var result=game.Execute(new GameCommand("journey-"+(++commands), state.PlayerId, kind, target, state.Revision));
        Check(result.Ok, kind+" "+target+" — "+result.Message);
    }
    static void Seconds(int count) { for (int i=0;i<count;i++) { clock.Tick(); game.Advance(); } }
    static void Rest()
    {
        for(int i=0;i<4000;i++)
        {
            var s=game.Snapshot();
            if(MarchCapacityRules.ActiveCount(s,clock.UtcTicks)==0 && s.BuildingCompletesUtcTicks==0 && s.RecruitmentCompletesUtcTicks==0) return;
            Seconds(1);
        }
        throw new Exception("Canonical activity never completed");
    }
    static void Gather(string id)
    {
        Rest();
        for(int i=0;LocalGateway.ResourceRemaining(game.Snapshot(),id)==0 && i<120;i++) Seconds(1);
        Do("Gather",id);Rest();
    }
    static void Funds(int wood, int stone)
    {
        for(int i=0;game.Snapshot().Resources.Wood<wood && i<100;i++) Gather("forest-valoria");
        for(int i=0;game.Snapshot().Resources.Stone<stone && i<100;i++) Gather("quarry-valoria");
        var s=game.Snapshot();Check(s.Resources.Wood>=wood&&s.Resources.Stone>=stone,"resources earned by actual gathering");
    }
    static void Fight(string id) { Do("Fight",id); Rest(); Reload(); }
    static void Ascend(int level)
    {
        Do("AdvanceBastion","bastion");
        // Simulate closing the app with an active construction deadline.
        Reload();Rest();Reload();
        Check(game.Snapshot().BastionLevel==level,"continuous Bastion "+level+" reached and persisted");
        var s=game.Snapshot();var wallet=s.Resources;
        var grants=string.Join("|",s.ChapterProgress.GrantedRewards);
        Reload();var reopened=game.Snapshot();
        Check(reopened.Resources.Wood==wallet.Wood&&reopened.Resources.Stone==wallet.Stone&&reopened.Resources.Food==wallet.Food&&
            string.Join("|",reopened.ChapterProgress.GrantedRewards)==grants,"same-time reopen cannot duplicate chapter rewards "+level);
    }
    static void Main()
    {
        SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
        Reload();Check(game.Snapshot().BastionLevel==1,"fresh canonical save; no seeded chapter state");
        Do("Build","sawmill");Rest();
        Gather("forest-valoria");Gather("forest-valoria");Gather("quarry-valoria");
        Fight("corrupt-scout");Funds(450,300);Ascend(2);
        Funds(1000,1000);Do("Build","barracks");Rest();
        Do("Recruit","archer:t1:20");Reload();Rest();
        Do("ConfigureMarch","march-main:"+game.Snapshot().Available.ArcherT1+":aldric");
        Fight("engendro-valoria");
        if(game.Snapshot().Available.ArcherT1<41) { Funds(400,240);Do("Recruit","archer:t1:20");Rest(); }
        Funds(650,500);Ascend(3);
        Funds(240,180);Do("Build","granary");Rest();
        Do("InvestigateFissure","fissure-valoria");Reload();Seconds(1200);Reload();
        Check(game.Snapshot().ChapterThree.FissureExplored,"investigation persisted across absence");
        Do("ContactLyra","fissure-valoria");Fight("fissure-valoria");
        Check(game.Snapshot().ChapterThree.LyraRecruited,"Lyra recruited through continuous actual encounter");
        Fight("hunt-wolf");Fight("hunt-boar");
        Funds(3000,3000);
        foreach(var id in new[]{"sawmill","barracks","granary"}) { Do("UpgradeBuilding",id);Rest(); }
        Seconds(100);Funds(850,700);Ascend(4);
        Do("ChooseDevelopment","civil");Funds(1100,900);Ascend(5);
        Funds(500,0);Do("Build","stoneworks");Reload();Rest();
        Check(game.Snapshot().ChapterThree.StoneworksLevel==1,"quarry built after continuous V unlock");
        Check(!game.Execute(new GameCommand("no-vi",game.Snapshot().PlayerId,"AdvanceBastion","bastion",game.Snapshot().Revision)).Ok,"Bastion VI remains unavailable");
        Console.WriteLine("CONTINUOUS_I_V_DOMAIN_PASS commands="+commands+" reloads="+reloads+"; native visuals and device certification remain separate");
    }
}
