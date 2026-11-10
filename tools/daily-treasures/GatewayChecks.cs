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
        // Gateway first saves the report and can immediately remove a
        // claimed, expired order in its post-command Synchronize(). The
        // original UI only inspected Orders, so a legitimate first discovery
        // silently lost its celebration in that very same revision.
        var delayedBefore=new PlayerState();
        var delayedAfter=new PlayerState();
        const string oldOrderId="expired:first:1";
        delayedBefore.Retention.Orders.Add(new ScoutOrder{
            Id=oldOrderId,Kind=1,Claimed=false,Period=0
        });
        delayedAfter.Retention.ScoutDiscoveryMask=1<<1;
        Assert(!DailyTreasureMilestoneRules.FirstScoutFamily(
                delayedBefore,delayedAfter,oldOrderId),
            "a missing order WITHOUT its exact report is not celebrated");
        delayedAfter.Mailbox.Receive(new EldoriaV41Message{
            EventId="exploration:delayed-claim",Type="exploration",
            Category="informes",Title="Informe de exploración",
            DiscoveryOrderId=oldOrderId,DiscoveryKind=1,
            RewardReceiptId="delayed-claim"
        });
        Assert(DailyTreasureMilestoneRules.FirstScoutFamily(
                delayedBefore,delayedAfter,oldOrderId),
            "a pruned, genuinely claimed first family keeps its mail-backed ceremony");
        delayedBefore.Retention.ScoutDiscoveryMask=1<<1;
        Assert(!DailyTreasureMilestoneRules.FirstScoutFamily(
                delayedBefore,delayedAfter,oldOrderId),
            "old family mask cannot replay an expired scout discovery");
        delayedBefore.Retention.ScoutDiscoveryMask=0;
        delayedBefore.Retention.Orders[0].Claimed=true;
        Assert(!DailyTreasureMilestoneRules.FirstScoutFamily(
                delayedBefore,delayedAfter,oldOrderId),
            "previously claimed scout cannot use an old mail to replay a ceremony");

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
    static void RealMilestoneSessionQueue()
    {
        var q=new DailyTreasureMilestoneQueue();
        var baseline=new PlayerState();
        q.Reset(baseline);
        Assert(q.PendingCount==0,"no initial ceremony on a fresh game");
        var missingMail=new PlayerState();
        missingMail.BastionLevel=2;
        q.Observe(missingMail);
        Assert(q.PendingCount==0,"Bastion level without verified mailbox cannot invent ceremony");

        q.Reset(baseline);
        var earned=new PlayerState();
        earned.BastionLevel=2;
        earned.Mailbox.Receive(new EldoriaV41Message {
            EventId="milestone:bastion:2",Type="milestone-bastion",
            Category="narrativa",Title="Bastión II completado",
            Subtitle="El cuartel ya está a tu alcance"
        });
        q.Observe(earned);
        Assert(q.PendingCount==1,"committed Bastion II becomes a queued in-session celebration");
        Assert(q.TryPeek(out var first) && first.EventId=="milestone:bastion:2" &&
            first.Detail.Contains("cuartel"),"queue uses only real mail title/description");
        Assert(q.TryPeek(out var later) && later.EventId==first.EventId,
            "unavailable UI does not consume deferred ceremony");
        Assert(!q.ConfirmPresented("milestone:bastion:5"),
            "wrong confirmation cannot consume a ceremony");
        Assert(q.ConfirmPresented(first.EventId),"real displayed milestone is acknowledged");
        Assert(q.PendingCount==0 && !q.TryPeek(out _),"no replays after confirmed display");

        q.Observe(earned);
        Assert(q.PendingCount==0,"re-observing same state cannot celebrate Bastion twice");

        var recruited=new PlayerState();
        recruited.BastionLevel=2;
        recruited.ChapterThree.LyraRecruited=true;
        recruited.Mailbox.Receive(new EldoriaV41Message {
            EventId="milestone:hero:lyra",Type="hero-unlocked",
            Category="narrativa",Title="Lyra se une a Valoria",
            Subtitle="La exploradora puede participar en marchas"
        });
        q.Observe(recruited);
        Assert(q.PendingCount==1 && q.TryPeek(out var hero) &&
            hero.EventId=="milestone:hero:lyra","authentic Lyra recruitment is celebrated");
        Assert(!q.ConfirmPresented("fake"),"hero announcement cannot disappear through wrong ack");
        Assert(q.ConfirmPresented("milestone:hero:lyra"),
            "recruited Lyra presentation consumed only once");
        Assert(q.PendingCount==0,"hero announcement not replayed after acknowledgement");

        var boot=new DailyTreasureMilestoneQueue();
        boot.Reset(recruited);
        boot.Observe(recruited);
        Assert(boot.PendingCount==0,"offline Lyra arrives via mailbox without unwanted autoplay");
        q.Reset(recruited);
        Assert(q.PendingCount==0,"re-binding player resets only ephemeral screen queue");
        var next=new PlayerState();
        next.BastionLevel=5;
        next.ChapterThree.LyraRecruited=true;
        next.Mailbox.Receive(new EldoriaV41Message {
            EventId="milestone:bastion:5",Type="milestone-bastion",
            Category="narrativa",Title="Valoria se reconstruye",
            Subtitle="Bastión V completado"
        });
        q.Observe(next);
        Assert(q.PendingCount==1 && q.TryPeek(out var fifth) &&
            fifth.EventId=="milestone:bastion:5",
            "real late Bastion upgrade is queued correctly");

        var anonymous=new PlayerState();
        anonymous.BastionLevel=3;
        anonymous.ChapterThree.LyraRecruited=true;
        q.Reset(recruited);
        q.Observe(anonymous);
        Assert(q.PendingCount==0,"future unrecorded events do not create fictitious premium rewards");
    }

    static void RealReportSessionQueue()
    {
        var queue=new DailyTreasureMilestoneQueue();
        queue.Reset(new PlayerState());
        var realBattle=new PlayerState();
        realBattle.Retention.LastClearedStage=25;
        realBattle.Mailbox.Receive(new EldoriaV41Message{
            EventId="vigilant:completed-command",Type="battle",
            Category="informes",Title="Victoria en los Vigilantes",
            Subtitle="Capítulo cerrado",ReportId="vigilant:completed-command",
            Battle=new EldoriaV41CombatRecord{
                TargetId="vigilant:25",Won=true,Outcome="victory"
            }
        });
        Assert(!queue.AddConfirmedReport(realBattle,
                "vigilant:completed-command","battle","vigilant:chapter:24",
                "Capítulo falso","Resultado",true),
            "winning a non-final stage cannot claim a completed chapter");
        Assert(!queue.AddConfirmedReport(realBattle,
                "vigilant:completed-command","battle","vigilant:chapter:50",
                "Capítulo equivocado","Resultado",true),
            "a victory at stage 25 cannot pretend to be stage 50");
        var lostBattle=new PlayerState();
        lostBattle.Retention.LastClearedStage=24;
        lostBattle.Mailbox.Receive(new EldoriaV41Message{
            EventId="vigilant:lost-command",Type="battle",
            Category="informes",Title="Derrota en los Vigilantes",
            ReportId="vigilant:lost-command",
            Battle=new EldoriaV41CombatRecord{
                TargetId="vigilant:25",Won=false,Outcome="defeat"
            }
        });
        Assert(!queue.AddConfirmedReport(lostBattle,
                "vigilant:lost-command","battle","vigilant:chapter:25",
                "Capítulo falso","Derrota",true),
            "a persisted defeat cannot become a premium chapter victory");
        Assert(!queue.AddConfirmedReport(realBattle,
                "vigilant:invented","battle","vigilant:chapter:25",
                "Capítulo 1 completado","Resultado",true),
            "unpersisted battle cannot invent a premium ceremony");
        Assert(!queue.AddConfirmedReport(realBattle,
                "vigilant:completed-command","exploration","scout:first:order",
                "Capítulo 1 completado","Resultado",true),
            "exploration style cannot hijack a battle report");
        Assert(queue.AddConfirmedReport(realBattle,
                "vigilant:completed-command","battle","vigilant:chapter:25",
                "Capítulo 1 completado","Resultado confirmado",true),
            "an actually recorded chapter battle defers its ceremony");
        Assert(!queue.AddConfirmedReport(realBattle,
                "vigilant:completed-command","battle","vigilant:chapter:25",
                "Capítulo 1 completado","Resultado confirmado",true),
            "repeated command report cannot enqueue a duplicate ceremony");
        Assert(queue.TryPeek(out var battle) &&
               battle.Legendary && battle.EventId=="vigilant:chapter:25",
            "chapter queue retains the genuine victory visual tier");
        Assert(queue.TryPeek(out _) && queue.PendingCount==1,
            "unavailable or open narrative room cannot consume a queued battle");
        Assert(!queue.ConfirmPresented("scout:first:order"),
            "unrelated UI may not acknowledge a chapter battle");
        Assert(queue.ConfirmPresented(battle.EventId) &&
               queue.PendingCount==0,"chapter is consumed only after presentation");

        var discovery=new PlayerState();
        discovery.Retention.ScoutDiscoveryMask|=1<<2;
        discovery.Retention.Orders.Add(new ScoutOrder{
            Id="order42",Kind=2,Claimed=true
        });
        discovery.Mailbox.Receive(new EldoriaV41Message{
            EventId="exploration:actual-order-claim",Type="exploration",
            Category="informes",Title="Informe de exploración",
            Subtitle="Nuevo hallazgo auténtico",
            DiscoveryOrderId="order42",DiscoveryKind=2,
            RewardReceiptId="actual-order-claim"
        });
        Assert(!queue.AddConfirmedReport(discovery,
                "exploration:actual-order-claim","exploration","scout:first:other",
                "Hallazgo falso","Informe",false),
            "a real report cannot masquerade as another explorer order");
        var unclaimedOrder=new PlayerState();
        unclaimedOrder.Retention.ScoutDiscoveryMask|=1<<2;
        unclaimedOrder.Retention.Orders.Add(new ScoutOrder{
            Id="order42",Kind=2,Claimed=false
        });
        unclaimedOrder.Mailbox.Receive(new EldoriaV41Message{
            EventId="exploration:order-unclaimed",Type="exploration",
            Category="informes",Title="Informe parcial",
            DiscoveryOrderId="order42",DiscoveryKind=2,
            RewardReceiptId="order-unclaimed"
        });
        Assert(!queue.AddConfirmedReport(unclaimedOrder,
                "exploration:order-unclaimed","exploration","scout:first:order42",
                "Hallazgo falso","Informe",false),
            "unfinished explorer orders never trigger a first-find celebration");
        Assert(!queue.AddConfirmedReport(discovery,
                "exploration:unknown-claim","exploration","scout:first:order42",
                "Primer descubrimiento","Informe",false),
            "unpaid or missing scout report cannot create a celebration");
        Assert(queue.AddConfirmedReport(discovery,
                "exploration:actual-order-claim","exploration","scout:first:order42",
                "Primer descubrimiento","Informe confirmado",false),
            "first family discovery is queued after its real mailbox report");
        Assert(queue.TryPeek(out var scout) && !scout.Legendary &&
               scout.EventId=="scout:first:order42",
            "explorer keeps the special tier rather than false legendary loot");
        Assert(queue.ConfirmPresented(scout.EventId) &&
               queue.PendingCount==0,"first discovery ceremony displays once");
        // No retained order required after canonical post-claim cleanup:
        // the completed report still identifies the exact paid command and
        // discovery family, while an unclaimed retained order is rejected.
        var pruned=new PlayerState();
        pruned.Retention.ScoutDiscoveryMask=1<<1;
        pruned.Mailbox.Receive(new EldoriaV41Message{
            EventId="exploration:pruned-delayed",Type="exploration",
            Category="informes",Title="Primer hallazgo registrado",
            DiscoveryOrderId="expired:first:1",DiscoveryKind=1,
            RewardReceiptId="pruned-delayed"
        });
        Assert(queue.AddConfirmedReport(pruned,
                "exploration:pruned-delayed","exploration",
                "scout:first:expired:first:1","Primer hallazgo",
                "Informe auténtico",false),
            "pruned but saved scout report still permits a legitimate ceremony");
        Assert(queue.TryPeek(out var older) &&
               older.EventId=="scout:first:expired:first:1",
            "pruned first-family queue keeps exact historical order identity");
        Assert(queue.ConfirmPresented(older.EventId),
            "old first family only consumed after real optional presentation");
        queue.Reset(discovery);
        Assert(queue.PendingCount==0,
            "relaunch does not force an older offline discovery ceremony");
    }

    static void RealRewardEconomyEnvelope()
    {
        // A deterministic reference estimate, not a human retention result.
        // We use actual authored Bastion owner-profile values, never QA_FAST's
        // zero-cost developer shortcut. Revisit amounts after genuine testers.
        var total=new DailyTreasureState();
        int wood=0,stone=0,food=0,one=0,five=0;
        for(int d=1;d<=30;d++)
        {
            long ticks=new DateTime(2026,11,1,12,0,0,DateTimeKind.Utc)
                .AddDays(d-1).Ticks;
            Assert(DailyTreasureRules.TryClaim(total,ticks,out var reward),
                "daily economic simulation claim "+d);
            wood+=reward.Wood;
            stone+=reward.Stone;
            food+=reward.Food;
            one+=reward.SpeedupOneMinute;
            five+=reward.SpeedupFiveMinutes;
            if(d==7)
            {
                Assert(wood==330 && stone==300 && food==230 &&
                       one==3 && five==1,
                       "week one amounts and loyalty bonus exact");
                var level2=BastionProgressionCatalog.ForLevel(2);
                Assert(wood<SliceContentProfiles.OwnerIiiCandidate.Bastion2WoodCost &&
                       stone<=SliceContentProfiles.OwnerIiiCandidate.Bastion2StoneCost,
                       "first 7-day reward alone cannot finance Bastion II wood/stone");
                Assert(level2.PowerReward>0 &&
                       SliceContentProfiles.OwnerIiiCandidate.Bastion2WoodCost==450,
                       "balance review is tied to the real owner Bastion II cost");
            }
        }
        Assert(wood==1380 && stone==1240 && food==945 &&
               one==12 && five==4,
               "30-day generosity remains measurable and capped");
        int totalUpgradeWood=SliceContentProfiles.OwnerIiiCandidate.Bastion2WoodCost;
        int totalUpgradeStone=SliceContentProfiles.OwnerIiiCandidate.Bastion2StoneCost;
        int totalUpgradeFood=0;
        for(int level=3;level<=5;level++)
        {
            var cost=BastionProgressionCatalog.ForLevel(level);
            totalUpgradeWood+=cost.WoodCost;
            totalUpgradeStone+=cost.StoneCost;
            totalUpgradeFood+=cost.FoodCost;
        }
        Assert(totalUpgradeWood==3050 && totalUpgradeStone==2400 &&
               totalUpgradeFood==800,
               "real Bastion I-V authored cost envelope unchanged");
        Assert(wood*100/totalUpgradeWood<50 &&
               stone*100/totalUpgradeStone<55,
               "30-day resource entitlements below half of full major ascension cost");
        Console.WriteLine("DAILY_TREASURE_OWNER_BALANCE_7D: 330 madera / 300 piedra / 230 comida / 3x1min / 1x5min");
        Console.WriteLine("DAILY_TREASURE_OWNER_BALANCE_30D: 1380 madera / 1240 piedra / 945 comida / 12x1min / 4x5min");
        Console.WriteLine("DAILY_TREASURE_BASTION_II_V_COSTS: 3050 madera / 2400 piedra / 800 comida; balance candidate, not validated with people");
    }


    // Main's beta10 pretester fix must survive merging the independent
    // daily gift gateway. The real level-two sawmill/barracks upgrade has a
    // persisted timer; it must complete once (including after reload) while
    // the optional daily claim is still awarded by the same LocalGateway.
    static void MainlineBastionTwoAndDailyRewardCoexist()
    {
        foreach(var kind in new[]{"sawmill","barracks"})
        {
            var clock=new FakeClock{UtcTicks=At(20)};
            var saved=new PlayerState {
                BastionLevel=2,SawmillLevel=1,BarracksLevel=1
            };
            saved.Resources.Wood=5000;
            saved.Resources.Stone=5000;
            saved.Resources.Food=5000;
            var store=new MemoryStore{Saved=saved};
            var gateway=new LocalGateway(clock,store);
            Assert(Submit(gateway,"beta10-build-"+kind,
                "UpgradeBuilding",kind).Ok,
                "Main beta10 Bastion II "+kind+" still starts");
            var pending=gateway.Snapshot();
            Assert(pending.BuildingCompletesUtcTicks>clock.UtcTicks &&
                   pending.ChapterThree.PendingBuildingLevel==2 &&
                   pending.BuildingTaskId.StartsWith("c3:"+kind+":",
                       StringComparison.Ordinal),
                "Main beta10 early infrastructure timer persisted: "+kind);
            Assert(!Submit(gateway,"beta10-double-"+kind,
                "UpgradeBuilding",kind).Ok,
                "Main beta10 refuses two simultaneous infrastructure jobs: "+kind);
            clock.UtcTicks=pending.BuildingCompletesUtcTicks+
                TimeSpan.TicksPerSecond;
            Assert(gateway.Advance(),"Main beta10 completes timed job: "+kind);
            var finished=gateway.Snapshot();
            Assert((kind=="sawmill"?finished.SawmillLevel:
                    finished.BarracksLevel)==2 &&
                   finished.BuildingCompletesUtcTicks==0 &&
                   finished.BuildingTaskId=="" &&
                   finished.ChapterThree.PendingBuildingLevel==0,
                "Main beta10 settles actual level and clears timer: "+kind);
            long power=finished.MissionPower;
            int completed=finished.CompletedTaskIds.Count;
            gateway=new LocalGateway(clock,store);
            Assert(!gateway.Advance() &&
                   gateway.Snapshot().MissionPower==power &&
                   gateway.Snapshot().CompletedTaskIds.Count==completed,
                "Main beta10 early upgrade never pays twice after reload: "+kind);
            Assert(Submit(gateway,"daily-after-beta10-"+kind,
                "ClaimDailyTreasure").Ok,
                "Daily tributes still work after canonical beta10 upgrade: "+kind);
            var credited=gateway.Snapshot();
            Assert(credited.DailyTreasures.TotalClaims==1 &&
                   credited.Mailbox.Messages.Exists(m=>
                       m.Type=="daily-treasure") &&
                   !Submit(gateway,"daily-duplicate-beta10-"+kind,
                       "ClaimDailyTreasure").Ok,
                "Daily awards exactly once beside early infrastructure: "+kind);
        }
        var narrativeClock=new FakeClock{UtcTicks=At(20)};
        var narrative=new LocalGateway(narrativeClock,new MemoryStore());
        foreach(var beat in new[]{
            "march-preparation","vigilants-introduction","scouts-introduction"})
        {
            Assert(narrative.AcknowledgeNarrative(beat),
                "Canonical beta10 tutorial milestone remains accepted: "+beat);
            Assert(narrative.Snapshot().ChapterProgress.SeenNarrative.Contains(beat),
                "Canonical beta10 tutorial progress saved: "+beat);
            Assert(!narrative.AcknowledgeNarrative(beat),
                "Canonical beta10 tutorial cannot repeat: "+beat);
        }
    }

    static void NarrativeGatedScoutMarchCapacity()
    {
        // The old foreign march-capacity smoke configured only Bastion II
        // with an unopened tutorial. Main's beta10 narrative correctly
        // forbids Scouts until the Bastion III introduction. Reproduce the
        // *real* three-march invariants here, with a story-valid saved player,
        // rather than editing a foreign-owned test or bypassing its guard.
        var clock=new FakeClock{UtcTicks=At(8)};
        var player=new PlayerState {
            BastionLevel=3,BarracksLevel=1,SawmillLevel=1,
            Available=new ArmyRoster{ArcherT1=150},
            ChapterProgress=new ChapterProgressState {
                MarchConfirmed=true,
                SeenNarrative=new System.Collections.Generic.List<string>{
                    "scouts-introduction"
                }
            }
        };
        Assert(player.ChapterProgress.SeenNarrative.Contains("scouts-introduction"),
            "Scouts introduced through canonical saved narrative ledger");
        Assert(EarlyRetentionRules.ScoutsUnlocked(player),
            "Story-valid Scouts really become available");
        var store=new MemoryStore{Saved=player};
        var gateway=new LocalGateway(clock,store);
        var created=gateway.Snapshot();
        Assert(created.Retention.Orders.Count>=3,
            "Real LocalGateway creates three dispatchable scout orders");
        string[] orderIds={
            created.Retention.Orders[0].Id,
            created.Retention.Orders[1].Id,
            created.Retention.Orders[2].Id
        };
        for(int i=0;i<3;i++)
            Assert(Submit(gateway,"intro-scout-"+i,"ScoutDispatch",orderIds[i]).Ok,
                "Narrative-gated scout occupies shared world-march slot "+i);
        var inFlight=gateway.Snapshot();
        Assert(MarchCapacityRules.ActiveCount(inFlight,clock.UtcTicks)==3,
            "Three genuinely started Scouts fill the global march capacity");
        Assert(inFlight.Available.Total==114,
            "Scout capacity reserves three distinct twelve-archer detachments");
        Assert(!Submit(gateway,"intro-scout-fourth","Gather",
                "forest-valoria").Ok,
            "A fourth march cannot share already reserved troops");
        clock.UtcTicks+=TimeSpan.FromSeconds(1000).Ticks;
        gateway=new LocalGateway(clock,store);
        var returned=gateway.Snapshot();
        Assert(MarchCapacityRules.ActiveCount(returned,clock.UtcTicks)==0 &&
               returned.Available.Total==150,
            "All three Scout detachments return exactly once offline");
        for(int i=0;i<3;i++)
            Assert(Submit(gateway,"intro-scout-claim-"+i,
                "ScoutClaim",orderIds[i]).Ok,
                "Honest completed Scout report claimed once "+i);
        gateway=new LocalGateway(clock,store);
        Assert(gateway.Snapshot().Available.Total==150,
            "Claiming scout reports never returns the same troops twice");
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
        MainlineBastionTwoAndDailyRewardCoexist();
        RealMilestoneSessionQueue();
        RealReportSessionQueue();
        RealRewardEconomyEnvelope();
        NarrativeGatedScoutMarchCapacity();
        Console.WriteLine("PASS daily treasure real gateway: "+assertions+" assertions, store writes "+store.Writes);
    }
}
