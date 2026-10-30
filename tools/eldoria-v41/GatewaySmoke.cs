using System;
using System.Linq;
using Eldoria.Domain;
using Eldoria.Application;

static class V41GatewaySmoke
{
    sealed class Clock:IClock
    {
        public long UtcTicks{get;set;}=new DateTime(2026,10,30,8,0,0,DateTimeKind.Utc).Ticks;
        public void Add(int seconds)=>UtcTicks+=TimeSpan.FromSeconds(seconds).Ticks;
    }
    sealed class Store:IStateStore
    {
        public PlayerState State;
        public PlayerState Load()=>State;
        public void Save(PlayerState next)=>State=next;
    }
    static void Check(bool condition,string name)
    {
        if(!condition)throw new Exception("V41_GATEWAY_FAIL "+name);
        Console.WriteLine("PASS "+name);
    }
    static CommandResult Send(LocalGateway g,string id,string kind,string target)
    {
        var s=g.Snapshot();return g.Execute(new GameCommand(id,s.PlayerId,kind,target,s.Revision));
    }
    static void Main()
    {
        var store=new Store{State=new PlayerState{
            BastionLevel=2,BarracksLevel=1,SawmillLevel=1,
            ChapterProgress=new ChapterProgressState{MarchConfirmed=true},
            Available=new ArmyRoster{ArcherT1=150}
        }};
        var clock=new Clock();var g=new LocalGateway(clock,store);
        Check(g.Snapshot().Mailbox!=null,"old player save gets mailbox");
        Check(Send(g,"select1","ConfigureMarch","march-main:40:0:0:aldric::").Ok,
            "three-slot command accepts one unlocked leader");
        Check(g.Snapshot().PreparedHeroIds.Count==3&&
              g.Snapshot().PreparedHeroIds[0]=="aldric"&&
              g.Snapshot().PreparedHeroIds[2]==null,
              "three hero slots persist without fake recruits");
        Check(!Send(g,"duplicate","ConfigureMarch","march-main:40:0:0:aldric:aldric:").Ok,
              "duplicate hero rejected");
        Check(!Send(g,"locked","ConfigureMarch","march-main:40:0:0:aldric:lyra:").Ok,
              "Lyra locked before narrative recruitment");
        Check(Send(g,"fight","Fight","corrupt-scout").Ok,"battle launched");
        clock.Add(15);g=new LocalGateway(clock,store);
        var inbox=g.Snapshot().Mailbox;
        Check(inbox.List("informes").Count==1,"battle creates one persistent mail");
        var battle=inbox.List("informes")[0];
        Check(battle.Battle!=null && battle.Battle.Troops>0 &&
              battle.Battle.TargetId=="corrupt-scout","battle mail retains original combat facts");
        Check(inbox.Unread("informes")==1,"unread count reflects actual combat");
        var originalWood=g.Snapshot().Resources.Wood;
        Check(Send(g,"read1","MailRead",battle.EventId).Ok,"mark mail read through authority");
        Check(g.Snapshot().Mailbox.Unread()==0,"mark read persists");
        Check(Send(g,"delete1","MailDelete",battle.EventId).Ok,"delete message through authority");
        g=new LocalGateway(clock,store);
        Check(g.Snapshot().Mailbox.List("informes").Count==0 &&
              g.Snapshot().Mailbox.ReceivedEventIds.Contains(battle.EventId),
              "delete and tombstone survive reload");
        Check(g.Snapshot().Resources.Wood==originalWood,"read/delete never re-grant rewards");

        var storyStore=new Store{State=new PlayerState{
            BastionLevel=3,BarracksLevel=1,
            Available=new ArmyRoster{ArcherT1=150}
        }};
        storyStore.State.ChapterThree.LyraRecruited=true;
        var story=new LocalGateway(clock,storyStore);
        Check(Send(story,"heroes2","ConfigureMarch","march-main:40:0:0:aldric:lyra:").Ok,
              "Aldric plus recruited Lyra selected");
        Check(story.Snapshot().PreparedHeroId=="aldric+lyra" &&
              story.Snapshot().PreparedHeroIds.Count==3,
              "existing two-hero combat resolver remains canonical");
        Console.WriteLine("ELDORIA_V41_GATEWAY_PASS");
    }
}
