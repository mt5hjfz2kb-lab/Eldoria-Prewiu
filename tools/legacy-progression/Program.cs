using System;
using System.Linq;
using System.Text.Json;
using Eldoria.LegacyProgression;
using Eldoria.Domain;
using Eldoria.Application;

static class Program
{
    static int checks;
    static void Check(bool condition,string name)
    { if(!condition)throw new Exception(name);checks++; }
    static ProgressReceipt Receipt(string id,ReceiptKind kind,string subject,long ticks,int level=0,long amount=1) =>
        new ProgressReceipt{Id=id,PlayerId="p",WorldId="w",Kind=kind,SubjectId=subject,UtcTicks=ticks,Level=level,Amount=amount};
    static void Main()
    {
        var r=LegacyProgressionRules.Trophies;
        Check(r.Count==30&&r.Select(t=>t.Id).Distinct().Count()==30,"30 unique trophies");
        Check(LegacyProgressionRules.Titles.Count==10,"10 titles");
        Check(LegacyProgressionRules.Banners.Count==8,"8 banners");
        Check(LegacyProgressionRules.Monuments.Count==5,"5 monuments");
        Check(LegacyProgressionRules.Collections.Count==3,"3 collections");
        Check(LegacyProgressionRules.Banners.Concat(LegacyProgressionRules.Monuments).All(c=>r.Any(t=>t.Id==c.TrophyId)),"all cosmetics have a trophy");
        var f=new ProgressFacts{Bastion=1,TotalPower=2452,KingdomPower=600,ArmyPower=648};
        var p=new LocalProfile("Tester",f);
        Check(p.Trophies.Count==0&&p.Prestige==0&&!p.ProfileUnlocked&&!p.RankingsVisible,"fresh profile");
        Check(!p.PublicRankingsAvailable,"local never public");
        Check(LegacyProgressionRules.PersonalMetric(f,RankingKind.Hunters)==null,"no fabricated hunt score");
        Check(LegacyProgressionRules.PersonalMetric(f,RankingKind.TotalPower)==2452,"canonical score");
        f.Bastion=2;f.VigilantStage=5;
        Check(p.Trophies.Count==3&&p.Prestige==40&&p.RankingsVisible,"BII trophy projection");
        f.Bastion=3;f.DiscoveryMask=15;f.ScoutReports=3;f.RecordedHunts=1;
        Check(LegacyProgressionRules.DiscoveryCount(31)==4,"ignore unknown mask bits");
        Check(p.Discoveries.Count==3&&p.Discoveries.Single(c=>c.Id=="frontier").Count(f)==4,"actual discovery collection");
        Check(LegacyProgressionRules.PersonalMetric(f,RankingKind.Explorers)==4,"unique discovery score");
        Check(p.Banners.Count==0&&p.Monuments.Count==0,"cosmetic gates");
        f.Bastion=5;f.FissureClosed=true;f.LyraRecruited=true;f.AshStalkerDefeated=true;
        Check(p.ProfileUnlocked&&p.Titles.Count>0,"profile BIV");
        Check(p.Banners.Any(c=>c.Id=="banner-founder")&&p.Monuments.Any(c=>c.Id=="monument-founder"),"founder unlock");
        Check(!p.Monuments.Any(c=>c.Id=="monument-hunt"),"locked monument");
        var prestige=p.Prestige;
        Check(p.Prestige==prestige,"read idempotence");
        var week=LegacyReceiptLedger.WeekStartUtc(new DateTime(2026,11,24,13,0,0,DateTimeKind.Utc));
        Check(week==new DateTime(2026,11,23,0,0,0,DateTimeKind.Utc),"UTC week");
        long start=week.Ticks,end=week.AddDays(7).Ticks;
        var ledger=new LegacyReceiptLedger{PlayerId="p",WorldId="w"};
        var hunt=Receipt("h1",ReceiptKind.HuntVictory,"boar",start,3);
        Check(ledger.RecordCommitted(hunt)&&!ledger.RecordCommitted(hunt),"unique event");
        hunt.Level=5;Check(ledger.HuntPoints(start,end)==3,"copy on ingest");
        Check(!ledger.RecordCommitted(Receipt("bad",ReceiptKind.HuntVictory,"boar",start,0)),"bad level");
        Check(ledger.RecordCommitted(Receipt("h2",ReceiptKind.HuntVictory,"boar",end,5)),"boundary next week");
        Check(ledger.HuntPoints(start,end)==3,"exclusive period end");
        var wrong=Receipt("wrong",ReceiptKind.HuntVictory,"boar",start,1);wrong.PlayerId="other";
        Check(!ledger.RecordCommitted(wrong),"player isolation");
        Check(ledger.RecordCommitted(Receipt("d1",ReceiptKind.ScoutDiscovery,"breach",start))&&ledger.RecordCommitted(Receipt("d2",ReceiptKind.ScoutDiscovery,"breach",start)),"distinct receipts");
        Check(ledger.UniqueDiscoveries(start,end)==1,"unique discovery not repeated reports");
        Check(ledger.RecordCommitted(Receipt("g1",ReceiptKind.GatherReturn,"wood",start,0,120))&&ledger.Gathered(start,end)==120,"actual return quantity");
        var options=new JsonSerializerOptions{IncludeFields=true};
        var restored=JsonSerializer.Deserialize<LegacyReceiptLedger>(JsonSerializer.Serialize(ledger,options),options);
        Check(restored.HuntPoints(start,end)==3&&!restored.RecordCommitted(hunt),"serialization and replay");
        Check(LegacyProgressionRules.Titles.Max(t=>t.Prestige)<=r.Sum(t=>t.Prestige),"highest title attainable");
        var state=new PlayerState{BastionLevel=3,PlayerName="Real player"};
        state.Retention.LastClearedStage=5;
        state.Retention.ScoutDiscoveryMask=3;
        state.ChapterThree.Hunts=1;
        var snapshot=LegacyPlayerAdapter.Project(state);
        Check(snapshot.Name=="Real player"&&snapshot.Facts.VigilantStage==5&&snapshot.Facts.DiscoveryMask==3,"actual state adapter");
        Check(snapshot.Facts.TotalPower==SliceRules.TotalPower(state).Total&&state.Retention.LastClearedStage==5,"canonical power and read-only projection");
        var scope=new RankingScope{WorldId="w",PeriodId="2026-W48",Kind=RankingKind.Hunters,
            Period=RankingPeriod.Weekly,StartUtcTicks=start,EndUtcTicks=end};
        VerifiedScore Score(string id,long value,long when,string world="w")=>
            new VerifiedScore{PlayerId=id,DisplayName=id,WorldId=world,PeriodId=scope.PeriodId,
                Kind=scope.Kind,Score=value,AchievedUtcTicks=when};
        var scores=new[]{Score("b",10,start+1),Score("a",10,start+1),Score("c",10,start),
            Score("d",20,start),Score("wrong-world",999,start,"other")};
        var rows=VerifiedRankings.Build(scope,scores);
        Check(rows.Select(x=>x.PlayerId).SequenceEqual(new[]{"d","c","a","b"}),"ranking sorted, tie break, world isolated");
        Check(rows.Select(x=>x.Position).SequenceEqual(new[]{1,2,3,4}),"deterministic positions");
        Check(VerifiedRankings.Build(scope,Array.Empty<VerifiedScore>()).Count==0,"empty ranking never creates rows");
        bool duplicateRejected=false;
        try { VerifiedRankings.Build(scope,new[]{scores[0],scores[0]}); }
        catch(ArgumentException){duplicateRejected=true;}
        Check(duplicateRejected,"duplicate score rejected");
        bool boundaryRejected=false;
        try { VerifiedRankings.Build(scope,new[]{Score("end",1,end)}); }
        catch(ArgumentException){boundaryRejected=true;}
        Check(boundaryRejected,"end time rejected");
        Console.WriteLine("PASS: "+checks+" legacy progression assertions");
    }
}
