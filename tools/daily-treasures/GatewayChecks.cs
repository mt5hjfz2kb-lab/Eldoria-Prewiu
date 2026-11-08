using System;
using Eldoria.Application;
using Eldoria.Domain;
internal static class GatewayChecks
{
    sealed class FakeClock : IClock
    {
        public long UtcTicks {get;set;}
    }
    sealed class MemoryStore : IStateStore
    {
        public PlayerState Saved;
        public int Writes;
        public PlayerState Load() => Saved;
        public void Save(PlayerState state){Saved=state;Writes++;}
    }
    static int assertions;
    static void Assert(bool ok,string message)
    {
        assertions++;
        if(!ok)throw new Exception(message);
    }
    static long At(int day) => new DateTime(2026,11,day,12,0,0,DateTimeKind.Utc).Ticks;
    static CommandResult Submit(LocalGateway g,string id,string kind,string target="valoria")
    {
        var s=g.Snapshot();
        return g.Execute(new GameCommand(id,s.PlayerId,kind,target,s.Revision));
    }
    static int MailCount(PlayerState state)
    {
        int n=0;
        foreach(var mail in state.Mailbox.Messages)
            if(mail.Type=="daily-treasure")n++;
        return n;
    }
    public static void Main()
    {
        var clock=new FakeClock{UtcTicks=At(8)};
        var store=new MemoryStore();
        var gateway=new LocalGateway(clock,store);
        var first=gateway.Snapshot();
        Assert(first.DailyTreasures!=null,"daily state in snapshot");
        int wood=first.Resources.Wood,stone=first.Resources.Stone,food=first.Resources.Food;
        Assert(Submit(gateway,"claim-08","ClaimDailyTreasure").Ok,"claim day1 success");
        var paid=gateway.Snapshot();
        Assert(paid.Resources.Wood==wood+60,"exact wood paid");
        Assert(paid.Resources.Stone==stone && paid.Resources.Food==food,"other resources untouched");
        Assert(paid.DailyTreasures.CycleDay==1,"day one persisted");
        Assert(MailCount(paid)==1,"one informational receipt");
        Assert(paid.Mailbox.Messages.Exists(m=>m.Type=="daily-treasure" &&
            m.RewardReceiptId=="claim-08" && m.Subtitle.Contains("60 madera")),
            "receipt describes real amount");
        Assert(Submit(gateway,"claim-08","ClaimDailyTreasure").Ok,"replaying same command idempotent");
        Assert(!Submit(gateway,"claim-08-second","ClaimDailyTreasure").Ok,
            "second command same UTC day rejected");
        var afterReplay=gateway.Snapshot();
        Assert(afterReplay.Resources.Wood==wood+60,"no duplicate resources");
        Assert(MailCount(afterReplay)==1,"no duplicate mailbox");
        Assert(Submit(gateway,"read-08","MailRead",afterReplay.Mailbox.Messages[0].EventId).Ok,
            "mail read via gateway");
        Assert(gateway.Snapshot().Resources.Wood==wood+60,"reading mail never pays again");
        var id=afterReplay.Mailbox.Messages[0].EventId;
        Assert(Submit(gateway,"delete-08","MailDelete",id).Ok,"receipt can be deleted");
        Assert(MailCount(gateway.Snapshot())==0,"receipt removed");
        Assert(gateway.Snapshot().Mailbox.ReceivedEventIds.Contains(id),
            "mail tombstone retained after deletion");
        // Reload same canonical saved snapshot: not a second wallet.
        gateway=new LocalGateway(clock,store);
        Assert(!Submit(gateway,"after-reload","ClaimDailyTreasure").Ok,
            "reload does not regenerate today's reward");
        Assert(gateway.Snapshot().Resources.Wood==wood+60,"reload keeps exact balance");
        clock.UtcTicks=At(9);
        Assert(Submit(gateway,"claim-09","ClaimDailyTreasure").Ok,"next day is available");
        var second=gateway.Snapshot();
        Assert(second.Resources.Stone==stone+40,"day2 exact stone");
        Assert(second.DailyTreasures.ConsecutiveDays==2,"two-day streak");
        var copy=second;
        copy.DailyTreasures.CycleDay=7;
        Assert(gateway.Snapshot().DailyTreasures.CycleDay==2,"detached snapshot is safe");
        clock.UtcTicks=At(11);
        Assert(Submit(gateway,"claim-11","ClaimDailyTreasure").Ok,"after one missed day");
        var third=gateway.Snapshot();
        Assert(third.DailyTreasures.CycleDay==3,"seven-day calendar keeps progress");
        Assert(third.DailyTreasures.ConsecutiveDays==1,"miss resets streak bonus only");
        Assert(third.SpeedupOneMinute==1,"day3 speedup paid");
        clock.UtcTicks=At(10);
        Assert(!Submit(gateway,"rollback","ClaimDailyTreasure").Ok,
            "wall-clock rollback cannot claim again");
        Console.WriteLine("PASS daily treasure real gateway: "+assertions+" assertions, store writes "+store.Writes);
    }
}
