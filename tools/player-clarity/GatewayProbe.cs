using System;
using Eldoria.Domain;
using Eldoria.Application;

internal static class GatewayProbe
{
    private sealed class Clock : IClock { public long UtcTicks {get;set;} }
    private sealed class Store : IStateStore
    {
        public PlayerState Saved;
        public int Writes;
        public PlayerState Load()=>Saved;
        public void Save(PlayerState state){Saved=state;Writes++;}
    }
    private sealed class Placement : IWorldResourcePlacement,IWorldCreaturePlacement
    {
        public bool TryChoose(PlayerState state,string target,string previous,int gen,
            out WorldResourceLocation place)
        {
            place=new WorldResourceLocation{TargetId=target,
                ResourceKind=target.Contains(":wood:")?"wood":"stone",
                Generation=gen,SiteId="verified-wood-"+gen,X=160,Y=4,Z=190};
            return true;
        }
        public bool TryChoose(PlayerState state,string target,string previous,int gen,
            out WorldCreatureLocation place)
        {
            place=new WorldCreatureLocation{TargetId=target,CreatureKind="boar",
                Level=3,Generation=gen,SiteId="verified-boar-"+gen,
                X=260,Y=4,Z=320};
            return true;
        }
    }
    private static int count;
    private static void Check(bool pass,string note)
    {
        if(!pass)throw new Exception("GATEWAY_INTEGRATION_FAIL: "+note);
        count++;Console.WriteLine("PASS "+note);
    }
    public static void Main()
    {
        long depleted=new DateTime(2026,11,5,12,0,0,DateTimeKind.Utc).Ticks;
        var c=new Clock{UtcTicks=depleted+TimeSpan.FromMinutes(6).Ticks};
        var persisted=new PlayerState();
        persisted.PlayerName=null; // genuine legacy schema-1 JSON without name
        persisted.WorldResourceLocations.Add(new WorldResourceLocation{
            TargetId="region1:wood:001",SiteId="old-wood-site",
            ResourceKind="wood",Remaining=0,Capacity=1250,Level=0,Generation=1,
            DepletedUtcTicks=depleted,RespawnUtcTicks=depleted+TimeSpan.FromMinutes(30).Ticks
        });
        persisted.WorldCreatureLocations.Add(new WorldCreatureLocation{
            TargetId="region1:boar:001",SiteId="old-boar-site",CreatureKind="boar",
            Level=2,Generation=1,DefeatedUtcTicks=depleted,
            RespawnUtcTicks=depleted+TimeSpan.FromMinutes(15).Ticks
        });
        var store=new Store{Saved=persisted};
        var gateway=new LocalGateway(c,store);
        var legacy=gateway.Snapshot();
        Check(legacy.PlayerName=="","missing old nickname does not break old save");
        Check(legacy.WorldResourceLocations[0].Level==1,
            "legacy partially depleted node has accurate capacity-derived L1");
        var source=new Placement();
        gateway.ConfigureLocalResourceRespawn(source,RenewablePopulationPolicy.Delay);
        gateway.ConfigureLocalCreatureRespawn(source,RenewablePopulationPolicy.Delay);
        var renewed=gateway.Snapshot();
        var wood=renewed.WorldResourceLocations.Find(x=>x.TargetId=="region1:wood:001");
        var boar=renewed.WorldCreatureLocations.Find(x=>x.TargetId=="region1:boar:001");
        Check(wood.Generation==2 && wood.SiteId=="verified-wood-2" &&
            wood.RespawnUtcTicks==0 && wood.DepletedUtcTicks==0,
            "old 30-minute resource respawns at original event+5 minutes");
        Check(wood.Level>=1&&wood.Level<=5 &&
            wood.Capacity==ResourceNodeTierPolicy.Capacity("wood",wood.Level) &&
            wood.Remaining==wood.Capacity,
            "actual node tier and extractable capacity are persisted together");
        Check(boar.Generation==2&&boar.Level==3&&
            boar.DefeatedUtcTicks==0&&boar.RespawnUtcTicks==0,
            "old 15-minute animal respawns at original event+5 minutes");
        Check(gateway.Snapshot().Resources.Wood==persisted.Resources.Wood,
            "renewing nodes never grants resources directly to the wallet");

        var initial=gateway.Snapshot();
        var invalid=gateway.Execute(new GameCommand("name-invalid",initial.PlayerId,
            "ChoosePlayerName","x",initial.Revision));
        Check(!invalid.Ok && gateway.Snapshot().PlayerName=="",
            "invalid nickname does not mutate saved identity");
        initial=gateway.Snapshot();
        var chosen=gateway.Execute(new GameCommand("name-1",initial.PlayerId,
            "ChoosePlayerName","  Áurea_7  ",initial.Revision));
        Check(chosen.Ok&&gateway.Snapshot().PlayerName=="Áurea_7",
            "first nickname is normalized and committed by canonical GameCommand");
        var snapshot=gateway.Snapshot();
        snapshot.PlayerName="tampered";
        Check(gateway.Snapshot().PlayerName=="Áurea_7",
            "immutable snapshot cannot override authoritative player name");

        var replay=gateway.Execute(new GameCommand("name-1",initial.PlayerId,
            "ChoosePlayerName","  Áurea_7  ",initial.Revision));
        Check(replay.Ok && gateway.Snapshot().PlayerName=="Áurea_7",
            "same command ID replay is idempotent");
        var newer=gateway.Snapshot();
        var rename=gateway.Execute(new GameCommand("name-2",newer.PlayerId,
            "ChoosePlayerName","Another name",newer.Revision));
        Check(!rename.Ok&&gateway.Snapshot().PlayerName=="Áurea_7",
            "second different command cannot rename the first chosen identity");
        var reloaded=new LocalGateway(c,store);
        Check(reloaded.Snapshot().PlayerName=="Áurea_7",
            "nickname survives canonical store reload");
        Check(reloaded.Snapshot().WorldResourceLocations[0].Level==wood.Level,
            "tier survives canonical store reload");
        Check(reloaded.Snapshot().WorldCreatureLocations[0].Level==boar.Level,
            "animal level survives canonical store reload");
        Console.WriteLine("PLAYER_CLARITY_GATEWAY_PASS "+count+" exact state/command checks");
    }
}
