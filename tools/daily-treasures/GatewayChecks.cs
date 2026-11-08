using System;
using Eldoria.Application;
using Eldoria.Domain;
using Eldoria.DailyTreasures;
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
    static void SevenDayRealGatewayJourney()
    {
        var clock=new FakeClock{UtcTicks=At(8)};
        var store=new MemoryStore();
        var gateway=new LocalGateway(clock,store);
        var first=gateway.Snapshot();
        int wood=first.Resources.Wood,stone=first.Resources.Stone,
            food=first.Resources.Food,one=first.SpeedupOneMinute,
            five=first.SpeedupFiveMinutes;
        for(int day=1;day<=7;day++)
        {
            clock.UtcTicks=At(7+day);
            var current=gateway.Snapshot();
            var predicted=DailyTreasureRules.Preview(current.DailyTreasures,clock.UtcTicks);
            Assert(predicted.CycleDay==day,"real 7-day candidate order "+day);
            var receipt="week-"+day;
            Assert(Submit(gateway,receipt,"ClaimDailyTreasure").Ok,
                "real gateway daily payout "+day);
            wood+=predicted.Wood;
            stone+=predicted.Stone;
            food+=predicted.Food;
            one+=predicted.SpeedupOneMinute;
            five+=predicted.SpeedupFiveMinutes;
            var actual=gateway.Snapshot();
            Assert(actual.Resources.Wood==wood && actual.Resources.Stone==stone &&
                actual.Resources.Food==food,"7-day exact resource ledger "+day);
            Assert(actual.SpeedupOneMinute==one &&
                actual.SpeedupFiveMinutes==five,"7-day exact speedup ledger "+day);
            Assert(MailCount(actual)==day,"one receipt per real paid day "+day);
            Assert(!Submit(gateway,"repeat-"+day,"ClaimDailyTreasure").Ok,
                "repeated day does not pay twice "+day);
            gateway=new LocalGateway(clock,store);
            Assert(!Submit(gateway,"reloaded-"+day,"ClaimDailyTreasure").Ok,
                "reloaded day cannot pay twice "+day);
        }
        Assert(wood-first.Resources.Wood==330,"week total wood balanced baseline");
        Assert(stone-first.Resources.Stone==300,"week total stone balanced baseline");
        Assert(food-first.Resources.Food==230,"week total food includes only real streak bonuses");
        Assert(one-first.SpeedupOneMinute==3 && five-first.SpeedupFiveMinutes==1,
            "week accelerators exact");
        clock.UtcTicks=At(15);
        Assert(Submit(gateway,"week-eight","ClaimDailyTreasure").Ok,
            "calendar cycles without losing consecutive streak");
        var eighth=gateway.Snapshot();
        Assert(eighth.DailyTreasures.CycleDay==1 &&
            eighth.DailyTreasures.ConsecutiveDays==8 &&
            eighth.DailyTreasures.TotalClaims==8,
            "cycle resets after seven while consecutive streak continues");
    }
    static void RealMilestoneEligibility()
    {
        for(int kind=0;kind<4;kind++)
        {
            var before=new PlayerState();
            var after=new PlayerState();
            var id="test-scout-"+kind;
            before.Retention.Orders.Add(new ScoutOrder{Id=id,Kind=kind,Claimed=false});
            after.Retention.Orders.Add(new ScoutOrder{Id=id,Kind=kind,Claimed=true});
            after.Retention.ScoutDiscoveryMask=1<<kind;
            Assert(DailyTreasureMilestoneRules.FirstScoutFamily(before,after,id),
                "first discovery family earns one ceremony "+kind);
            Assert(!DailyTreasureMilestoneRules.FirstScoutFamily(before,after,"other"),
                "wrong scout is never celebrated "+kind);
            before.Retention.ScoutDiscoveryMask=1<<kind;
            Assert(!DailyTreasureMilestoneRules.FirstScoutFamily(before,after,id),
                "seen discovery family is never celebrated twice "+kind);
            before.Retention.ScoutDiscoveryMask=0;
            before.Retention.Orders[0].Claimed=true;
            Assert(!DailyTreasureMilestoneRules.FirstScoutFamily(before,after,id),
                "repeated report already claimed, no ceremony "+kind);
            before.Retention.Orders[0].Claimed=false;
            after.Retention.ScoutDiscoveryMask=0;
            Assert(!DailyTreasureMilestoneRules.FirstScoutFamily(before,after,id),
                "unearned discovery family has no ceremony "+kind);
        }
        var one=new PlayerState();
        var two=new PlayerState();
        Assert(!DailyTreasureMilestoneRules.NewlyCompletedBastion(one,two,out _),
            "existing Bastion must not play a fake upgrade");
        two.BastionLevel=2;
        Assert(DailyTreasureMilestoneRules.NewlyCompletedBastion(one,two,out var upgraded)
            && upgraded==2,"actual Bastion II transition qualifies");
        two.BastionUpgradeCompletesUtcTicks=At(9);
        Assert(!DailyTreasureMilestoneRules.NewlyCompletedBastion(one,two,out _),
            "pending Bastion timer is not a completed milestone");
        two.BastionUpgradeCompletesUtcTicks=0;
        one.BastionLevel=2;
        Assert(!DailyTreasureMilestoneRules.NewlyCompletedBastion(one,two,out _),
            "equal Bastion levels cannot be celebrated");
        two.BastionLevel=5;
        Assert(DailyTreasureMilestoneRules.NewlyCompletedBastion(one,two,out upgraded)
            && upgraded==5,"real Bastion V progress qualifies");
        two.BastionLevel=6;
        Assert(!DailyTreasureMilestoneRules.NewlyCompletedBastion(one,two,out _),
            "future Bastion VI not authored");
        Assert(!DailyTreasureMilestoneRules.NewlyRecruitedLyra(one,two),
            "hero not recruited must not get presentation");
        two.ChapterThree.LyraRecruited=true;
        Assert(DailyTreasureMilestoneRules.NewlyRecruitedLyra(one,two),
            "first authentic Lyra acquisition detected");
        one.ChapterThree.LyraRecruited=true;
        Assert(!DailyTreasureMilestoneRules.NewlyRecruitedLyra(one,two),
            "existing Lyra not announced repeatedly");
    }
    static void RealBastionMailboxMilestones()
    {
        foreach(var level in new[]{2,5})
        {
            var clock=new FakeClock{UtcTicks=At(10)};
            var pending=new PlayerState();
            pending.BastionLevel=level-1;
            pending.BastionUpgradeStartedUtcTicks=At(8);
            pending.BastionUpgradeCompletesUtcTicks=At(9);
            pending.BastionUpgradeTaskId="bastion"+level+":infrastructure-test";
            var store=new MemoryStore{Saved=pending};
            var g=new LocalGateway(clock,store);
            var result=g.Snapshot();
            Assert(result.BastionLevel==level,
                "actual Bastion "+level+" upgrade resolved when deadline elapsed");
            var eventId="milestone:bastion:"+level;
            var notices=result.Mailbox.Messages.FindAll(m=>
                m.EventId==eventId && m.Type=="milestone-bastion" &&
                m.Category=="narrativa");
            Assert(notices.Count==1,
                "exactly one real Bastion "+level+" announcement");
            Assert(notices[0].RewardReceiptId=="",
                "Bastion announcement is not a payable receipt");
            Assert(notices[0].OccurredAtUtcTicks==At(9),
                "Bastion announcement timestamp is the actual completion deadline");
            int woodAfter=result.Resources.Wood,stoneAfter=result.Resources.Stone,
                foodAfter=result.Resources.Food;
            g=new LocalGateway(clock,store);
            Assert(g.Snapshot().Mailbox.Messages.FindAll(m=>m.EventId==eventId).Count==1,
                "reloaded Bastion level "+level+" cannot duplicate announcement");
            Assert(Submit(g,"delete-bastion-"+level,"MailDelete",eventId).Ok,
                "Bastion announcement is removable");
            g=new LocalGateway(clock,store);
            var after=g.Snapshot();
            Assert(after.Mailbox.Messages.FindAll(m=>m.EventId==eventId).Count==0,
                "deleted Bastion announcement stays deleted across reload");
            Assert(after.Mailbox.ReceivedEventIds.Contains(eventId),
                "Bastion announcement tombstone remains after deletion");
            Assert(after.Resources.Wood==woodAfter &&
                   after.Resources.Stone==stoneAfter &&
                   after.Resources.Food==foodAfter,
                "reading/deleting Bastion announcements never mints resources");
        }
        var noUpgrade=new MemoryStore{Saved=new PlayerState()};
        var neutral=new LocalGateway(new FakeClock{UtcTicks=At(10)},noUpgrade);
        Assert(neutral.Snapshot().Mailbox.Messages.TrueForAll(m=>
            m.Type!="milestone-bastion"),
            "ordinary fresh game never receives a fake completed Bastion ceremony");
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
        SevenDayRealGatewayJourney();
        RealMilestoneEligibility();
        RealBastionMailboxMilestones();
        Console.WriteLine("PASS daily treasure real gateway: "+assertions+" assertions, store writes "+store.Writes);
    }
}
