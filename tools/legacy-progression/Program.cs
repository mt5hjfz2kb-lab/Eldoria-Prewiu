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
        var fresh=new LocalProfile("",new ProgressFacts{Bastion=1});
        Check(ProfilePresentation.Sections(fresh).SequenceEqual(new[]{ProfileSection.Overview}),"B1 no feature dump");
        Check(ProfilePresentation.Rows(fresh,ProfileSection.Rankings).Single().State==ProfileEntryState.Locked,"rankings locked B1");
        Check(!ProfilePresentation.Rows(fresh,ProfileSection.Overview).Any(x=>x.Key=="prestige"),"prestige hidden before B4");
        Check(ProfilePresentation.Sections(snapshot).Contains(ProfileSection.Discoveries)&&!ProfilePresentation.Sections(snapshot).Contains(ProfileSection.Heraldry),"B3 navigation");
        var rankView=ProfilePresentation.Rows(snapshot,ProfileSection.Rankings);
        Check(rankView.Count==7&&rankView.First().State==ProfileEntryState.Unavailable,"no fake leaderboard or positions");
        Check(rankView.Single(x=>x.Key=="ranking:Hunters").State==ProfileEntryState.Unavailable,"no fabricated hunter metric");
        Check(ProfilePresentation.DiscoveryEntries(snapshot,"frontier").Count(x=>x.State==ProfileEntryState.Earned)==2,"collection detail from facts");
        Check(ProfilePresentation.DiscoveryEntries(snapshot,"frontier").Count(x=>x.Title=="Hallazgo desconocido")==2,"unseen lore hidden");
        Check(ProfilePresentation.DiscoveryEntries(fresh,"frontier").Count==0,"hidden collection not inspectable");
        Check(ProfilePresentation.Rows(snapshot,ProfileSection.Trophies).Single(x=>x.Key=="fissure").Title=="Hazaña por descubrir","narrative trophy spoiler guard");
        Check(ProfilePresentation.Rows(p,ProfileSection.Monuments).Any(x=>x.Key=="monument-founder"&&x.State==ProfileEntryState.Earned),"monument earned but not installed");
        Check(ProfilePresentation.Rows(p,ProfileSection.Overview).Any(x=>x.Key=="next-title"),"next prestige goal shown");
        var saved=new LegacySavedProgress();
        saved.BindIdentity("p","w");
        Check(saved.Reconcile(f)&&!saved.Reconcile(f),"reconciliation idempotent");
        Check(saved.TrySelect(f,"banner","banner-founder")&&saved.TrySelect(f,"monument","monument-founder"),"earned cosmetics selectable");
        Check(!saved.TrySelect(f,"monument","monument-hunt")&&!saved.TrySelect(f,"banner","unknown"),"locked and unknown cosmetics rejected");
        var historicPrestige=saved.Prestige;f.AshStalkerDefeated=false;f.FissureClosed=false;
        Check(!saved.Reconcile(f)&&saved.Prestige==historicPrestige,"earned trophies never revoked by transient facts");
        var persisted=JsonSerializer.Deserialize<LegacySavedProgress>(JsonSerializer.Serialize(saved,options),options);
        persisted.BindIdentity("p","w");
        Check(persisted.Prestige==historicPrestige&&persisted.SelectedBanner=="banner-founder","same-save persistence");
        var local=new LocalProfile("Real",f,persisted);
        Check(local.Trophies.Any(x=>x.Id=="ash-stalker")&&local.Prestige==historicPrestige,"profile displays permanent prestige");
        Check(ProfilePresentation.Rows(local,ProfileSection.Trophies).Single(x=>x.Key=="ash-stalker").State==ProfileEntryState.Earned,"earned view preserved");
        var defensive=local.Saved;persisted.EarnedTrophyIds.Clear();
        Check(defensive.Prestige==historicPrestige,"snapshot does not alias source");
        bool wrongOwner=false;try{persisted.BindIdentity("another","w");}catch(InvalidOperationException){wrongOwner=true;}
        Check(wrongOwner,"save identity mismatch rejected");
        Check(ProfilePresentation.Rows(local,ProfileSection.Heraldry).Any(x=>x.Key=="title:pioneer"&&x.CommandKind=="SelectLegacyTitle"),"earned title command");
        Check(ProfilePresentation.Rows(local,ProfileSection.Monuments).Single(x=>x.Key=="monument-hunt").CommandKind=="","locked monument no command");
        Check(ProfilePresentation.Rows(local,ProfileSection.Heraldry).Any(x=>x.Key=="clear:SelectLegacyBanner"),"clear banner command");
        Check(ProfilePresentation.Rows(p,ProfileSection.Heraldry).All(x=>x.CommandKind==""),"unintegrated state no write controls");
        var recorded=new LegacySavedProgress();
        recorded.BindIdentity("p","w");
        recorded.Ledger.RecordCommitted(Receipt("regional",ReceiptKind.HuntVictory,"regional:boar",start,3));
        recorded.Ledger.RecordCommitted(Receipt("oldwolf",ReceiptKind.HuntVictory,"legacy:wolf",start,1));
        recorded.Ledger.RecordCommitted(Receipt("future",ReceiptKind.HuntVictory,"regional:boar",end,5));
        recorded.Ledger.RecordCommitted(Receipt("d1",ReceiptKind.ScoutDiscovery,"family:1",start));
        recorded.Ledger.RecordCommitted(Receipt("d2",ReceiptKind.ScoutDiscovery,"family:1",start));
        recorded.Ledger.RecordCommitted(Receipt("g1",ReceiptKind.GatherReturn,"wood",start,0,120));
        var codexFacts=new ProgressFacts{Bastion=7,EngendroDefeated=true};
        recorded.Reconcile(codexFacts);
        var codex=new LocalProfile("Codex",codexFacts,recorded);
        var weekly=ProfilePresentation.Rows(codex,ProfileSection.Rankings,week.AddDays(1));
        Check(weekly.Single(x=>x.Key=="ranking:Hunters").Detail.StartsWith("4 ·"),"weekly score excludes later receipt");
        Check(weekly.Single(x=>x.Key=="week:discoveries").Detail.StartsWith("1 ·"),"weekly unique discovery");
        Check(weekly.Single(x=>x.Key=="week:gathered").Detail.StartsWith("120 ·"),"weekly gathered");
        var beast=BestiaryProjection.Read(codex);
        Check(beast.Single(x=>x.Id=="boar").HighestRecordedTier==5,"actual recorded regional tier");
        Check(beast.Single(x=>x.Id=="wolf").HighestRecordedTier==0,"legacy scoring convention not creature tier");
        Check(beast.Single(x=>x.Id=="spawnling").Known&&beast.Single(x=>x.Id=="spawnling").RecordedVictories==0,"historic boss known, count not invented");
        Check(!beast.Single(x=>x.Id=="ash-stalker").Known&&beast.Single(x=>x.Id=="ash-stalker").Name=="Criatura por descubrir","unknown species hidden");
        Check(BestiaryProjection.Read(local).Count==0,"Códice gated B7");
        Console.WriteLine("PASS: "+checks+" legacy progression assertions");
    }
}
