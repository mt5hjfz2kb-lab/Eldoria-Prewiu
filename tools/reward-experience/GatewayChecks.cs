using System.Text.Json;
using Eldoria.Domain;
using Eldoria.Application;
int checks=0;
void Need(bool ok,string message){checks++;if(!ok)throw new Exception(message);}
var clock=new FixedClock{UtcTicks=new DateTime(2026,11,11,12,0,0,DateTimeKind.Utc).Ticks};
CommandResult Send(LocalGateway g,string id,string kind,string target){var s=g.Snapshot();return g.Execute(new GameCommand(id,s.PlayerId,kind,target,s.Revision));}
ResourceRewardInventory Packets(string mail="letter"){
 var r=new ResourceRewardInventory();Need(ResourceRewardRules.TryIssueBundle(r,"mission:chapter1","mission",75,50,0,mail),"seed exact compound reward");return r;
}
var initial=new PlayerState{ResourceRewards=Packets(),ResourceRewardsVersion=1};
initial.Mailbox.Receive(new EldoriaV41Message{EventId="letter",Category="recompensas",Type="mission",Title="Premio de misión"});
AttentionCueRules.Signal(initial.RewardAttention,"mail-reward:letter","mail",AttentionLevel.ActionReady);
var store=new JsonStore(initial);var gateway=new LocalGateway(clock,store);gateway.Advance();
var wallet=gateway.Snapshot().Resources;int wood=wallet.Wood,stone=wallet.Stone,food=wallet.Food;
Need(Send(gateway,"read","MailRead","letter").Ok,"real mail read command");
var read=gateway.Snapshot();Need(read.Mailbox.Messages.Single().Read,"read flag persisted through gateway");
Need(read.ResourceRewards.Packets.All(p=>!p.Collected),"reading cannot collect packets");
Need(read.Resources.Wood==wood&&read.Resources.Stone==stone,"reading cannot credit wallet");
Need(AttentionCueRules.At(read.RewardAttention,"mail")==AttentionLevel.ActionReady,"reading cannot remove pending claim attention");
int writes=store.Writes;
Need(!Send(gateway,"delete-locked","MailDelete","letter").Ok,"cannot delete pending-reward mail and strand its packages");
Need(store.Writes==writes&&gateway.Snapshot().Mailbox.Messages.Count==1,"rejected deletion neither saves nor removes letter");
Need(!Send(gateway,"use-locked",ResourceRewardCommands.UsePacket,"mission:chapter1:wood").Ok,"locked packet cannot be spent");
Need(!gateway.Snapshot().ResourceGuidance.Completed.Contains(ResourceRewardCommands.FirstUseStep),"failed use cannot complete tutorial");
writes=store.Writes;
Need(Send(gateway,"claim",ResourceRewardCommands.ClaimMail,"letter").Ok,"real claim command accepted");
var claimed=gateway.Snapshot();Need(store.Writes==writes+1,"claim is one authoritative saved transaction");
Need(claimed.ResourceRewards.Packets.All(p=>p.Collected&&!p.Used),"claim transfers all components to bag");
Need(claimed.Resources.Wood==wood&&claimed.Resources.Stone==stone&&claimed.Resources.Food==food,"claim does not credit wallet");
Need(AttentionCueRules.At(claimed.RewardAttention,"mail")==AttentionLevel.None,"successful claim resolves pending letter attention");
Need(AttentionCueRules.At(claimed.RewardAttention,"bag")==AttentionLevel.New,"new bag items are informational");
Need(claimed.ResourceGuidance.Completed.Contains(ResourceRewardCommands.FirstMailClaimStep),"only real successful claim completes claim tutorial");
writes=store.Writes;Need(Send(gateway,"claim",ResourceRewardCommands.ClaimMail,"letter").Ok,"same command ID remains idempotent");Need(store.Writes==writes,"same claim ID cannot save/grant again");
Need(!Send(gateway,"claim-again",ResourceRewardCommands.ClaimMail,"letter").Ok,"different command ID cannot claim spent mail twice");
Need(store.Writes==writes,"replayed logical claim cannot save again");
gateway=new LocalGateway(clock,store);Need(gateway.Snapshot().ResourceRewards.Packets.All(p=>p.Collected),"JSON reload retains collected packages");
writes=store.Writes;Need(Send(gateway,"use",ResourceRewardCommands.UsePacket,"mission:chapter1:wood").Ok,"real use command accepted");
var used=gateway.Snapshot();Need(store.Writes==writes+1,"wallet and used receipt save together");
Need(used.Resources.Wood==wood+75&&used.Resources.Stone==stone&&used.Resources.Food==food,"only actual packet amount/resource credits");
Need(used.ResourceRewards.Packets.Single(p=>p.Kind=="wood").Used,"used tombstone retained");
Need(used.ResourceGuidance.Completed.Contains(ResourceRewardCommands.FirstUseStep),"real successful use completes first-use tutorial");
writes=store.Writes;Need(Send(gateway,"use",ResourceRewardCommands.UsePacket,"mission:chapter1:wood").Ok,"same use command idempotent");
Need(!Send(gateway,"use-again",ResourceRewardCommands.UsePacket,"mission:chapter1:wood").Ok,"new command cannot reuse same item");Need(store.Writes==writes,"reuse cannot create saved duplicate credit");
gateway=new LocalGateway(clock,store);Need(!Send(gateway,"after-reload",ResourceRewardCommands.UsePacket,"mission:chapter1:wood").Ok,"used tombstone survives real JSON reload");Need(gateway.Snapshot().Resources.Wood==wood+75,"reload cannot recredit wallet");
var external=gateway.Snapshot();external.ResourceRewards.Packets.Single(p=>p.Kind=="stone").Amount=int.MaxValue;external.ResourceGuidance.Completed.Clear();external.RewardAttention.Records.Clear();external.Resources.Wood=0;
var isolated=gateway.Snapshot();Need(isolated.ResourceRewards.Packets.Single(p=>p.Kind=="stone").Amount==50,"snapshot cannot mutate packet authority");Need(isolated.ResourceGuidance.Completed.Count>0,"snapshot guidance is deep copied");Need(isolated.RewardAttention.Records.Count>0,"snapshot attention is deep copied");Need(isolated.Resources.Wood==wood+75,"snapshot wallet is isolated");
Need(Send(gateway,"delete-claimed","MailDelete","letter").Ok,"claimed informational letter can be deleted");Need(Send(gateway,"use-stone",ResourceRewardCommands.UsePacket,"mission:chapter1:stone").Ok,"deleting claimed letter cannot remove collected item");Need(gateway.Snapshot().Resources.Stone==stone+50,"stone credits exactly once");
foreach(string kind in new[]{"wood","stone","food"}){
 var s=new PlayerState();s.Resources.Wood=s.Resources.Stone=s.Resources.Food=int.MaxValue-5;Need(ResourceRewardRules.TryIssue(s.ResourceRewards,"overflow:"+kind,"mission",kind,10),"seed overflow packet");var st=new JsonStore(s);var g=new LocalGateway(clock,st);g.Advance();int before=st.Writes;
 Need(!Send(g,"overflow",ResourceRewardCommands.UsePacket,"overflow:"+kind).Ok,"overflow rejects "+kind);var actual=g.Snapshot();Need(actual.ResourceRewards.Packets.Single().Used==false,"overflow keeps packet "+kind);Need(!actual.ResourceGuidance.Completed.Contains(ResourceRewardCommands.FirstUseStep),"overflow does not fake tutorial "+kind);Need(st.Writes==before,"overflow has no saved partial mutation "+kind);
}
var seenState=new PlayerState{ResourceRewards=Packets("")};var seenStore=new JsonStore(seenState);var seenGateway=new LocalGateway(clock,seenStore);seenGateway.Advance();
Need(Send(seenGateway,"seen",ResourceRewardCommands.MarkBagSeen,"mission:chapter1:wood").Ok,"seeing packet uses real command");Need(!seenGateway.Snapshot().ResourceRewards.Packets.Single(p=>p.Kind=="wood").Used,"seeing is not forced consumption");
Need(Send(seenGateway,"skip",ResourceRewardCommands.DismissGuidance,ResourceRewardCommands.FirstUseStep).Ok,"explicit tutorial skip accepted");seenGateway=new LocalGateway(clock,seenStore);var progress=seenGateway.Snapshot().ResourceGuidance;Need(progress.Dismissed.Contains(ResourceRewardCommands.FirstUseStep)&&!progress.Completed.Contains(ResourceRewardCommands.FirstUseStep),"skip persists without fake completion");Need(!GuidedTutorialRules.ShouldPoint(progress,ResourceRewardCommands.FirstUseStep,"actual-use-button",true,true),"dismissed pointer does not return after reload");Need(!Send(seenGateway,"unknown-skip",ResourceRewardCommands.DismissGuidance,"unrelated-feature").Ok,"resource module cannot dismiss unrelated steps");
var legacy=new PlayerState{ResourceRewardsVersion=0,ResourceRewards=null,ResourceGuidance=null,RewardAttention=null};int oldWood=legacy.Resources.Wood;var oldStore=new JsonStore(legacy);var migrated=new LocalGateway(clock,oldStore).Snapshot();Need(migrated.ResourceRewardsVersion==1&&migrated.ResourceRewards.Packets.Count==0,"legacy null ledger migrates additively");Need(migrated.ResourceGuidance.Completed.Count==0&&migrated.RewardAttention.Records.Count==0,"legacy missing guidance initializes safely");Need(migrated.Resources.Wood==oldWood,"migration does not retroactively reissue/credit rewards");
try{new LocalGateway(clock,new JsonStore(new PlayerState{ResourceRewardsVersion=2}));Need(false,"future module save must not be overwritten");}catch(InvalidOperationException){checks++;}

// Daily resources arrive as unopened MAIL packets, never as wallet
// deposits. Check the actual gateway, JSON storage and replay boundaries.
long utcDay=DailyTreasureRules.UtcDay(clock.UtcTicks);
string dailyLetter="daily-treasure:"+utcDay;
var dailyStore=new JsonStore(new PlayerState());
var daily=new LocalGateway(clock,dailyStore);daily.Advance();
int initialDailyWood=daily.Snapshot().Resources.Wood;
int dailyWrites=dailyStore.Writes;
Need(Send(daily,"day1-real-gateway","ClaimDailyTreasure","").Ok,"day one real daily claim");
var day1=daily.Snapshot();
Need(dailyStore.Writes==dailyWrites+1,"daily claim, mail, packets saved once");
Need(day1.Resources.Wood==initialDailyWood,"daily claim did not bypass the bag");
var day1Pack=day1.ResourceRewards.Packets.Single(p=>p.MailEventId==dailyLetter);
Need(day1Pack.Kind=="wood"&&day1Pack.Amount==60&&!day1Pack.Collected&&!day1Pack.Used,"day one exact locked 60 wood");
Need(day1.Mailbox.Messages.Any(m=>m.EventId==dailyLetter&&m.Subtitle.Contains("paquetes esperan")),"daily mail explains pending packets");
Need(AttentionCueRules.At(day1.RewardAttention,"mail")==AttentionLevel.ActionReady,"daily claim creates actionable mail cue");
dailyWrites=dailyStore.Writes;
Need(!Send(daily,"day1-second-id","ClaimDailyTreasure","").Ok,"day one replay cannot credit new packets");
Need(dailyStore.Writes==dailyWrites,"replayed daily claim does not save");
Need(Send(daily,"day1-mail-read","MailRead",dailyLetter).Ok,"daily mail read");
Need(!daily.Snapshot().ResourceRewards.Packets.Single().Collected,"read is not claim");
Need(!Send(daily,"day1-mail-delete","MailDelete",dailyLetter).Ok,"pending letter cannot be deleted");
Need(Send(daily,"day1-mail-claim",ResourceRewardCommands.ClaimMail,dailyLetter).Ok,"mail claim transfers daily packet to bag");
Need(daily.Snapshot().Resources.Wood==initialDailyWood,"mail claim does not credit wallet");
daily=new LocalGateway(clock,dailyStore);
Need(Send(daily,"day1-bag-use",ResourceRewardCommands.UsePacket,day1Pack.ReceiptId).Ok,"daily bag use after reload");
Need(daily.Snapshot().Resources.Wood==initialDailyWood+60,"daily credit only upon use");
Need(!Send(daily,"day1-use-again",ResourceRewardCommands.UsePacket,day1Pack.ReceiptId).Ok,"daily packet cannot be reused");
// Multi-resource rewards retain both components, zero partial replays.
var fourth=new PlayerState{DailyTreasures=new DailyTreasureState{
    LastClaimUtcDay=utcDay-1,CycleDay=3,ConsecutiveDays=3,TotalClaims=3}};
var fourStore=new JsonStore(fourth);
var four=new LocalGateway(clock,fourStore);four.Advance();
Need(Send(four,"day4-real-gateway","ClaimDailyTreasure","").Ok,"day four accepts two resource components");
var day4=four.Snapshot();
Need(day4.ResourceRewards.Packets.Count(p=>p.MailEventId==dailyLetter)==2,"day four awards two locked packets");
Need(day4.ResourceRewards.Packets.Any(p=>p.Kind=="wood"&&p.Amount==90&&!p.Collected)&&
     day4.ResourceRewards.Packets.Any(p=>p.Kind=="stone"&&p.Amount==60&&!p.Collected),"day four actual amounts 90 and 60");
Need(!Send(four,"day4-second-id","ClaimDailyTreasure","").Ok,"day four replay cannot grant more resources");
Need(Send(four,"day4-mail-claim",ResourceRewardCommands.ClaimMail,dailyLetter).Ok,"multi-resource mail claim works");
Need(four.Snapshot().ResourceRewards.Packets.All(p=>p.Collected),"one claim collects entire two-part bundle");
// A day containing only acceleration generates no fake resource packet.
var fifth=new PlayerState{DailyTreasures=new DailyTreasureState{
    LastClaimUtcDay=utcDay-1,CycleDay=4,ConsecutiveDays=4,TotalClaims=4}};
var fiveStore=new JsonStore(fifth);
var five=new LocalGateway(clock,fiveStore);five.Advance();
int oldSpeedups=five.Snapshot().SpeedupOneMinute;
Need(Send(five,"day5-speedups","ClaimDailyTreasure","").Ok,"speedup-only day claims successfully");
var day5=five.Snapshot();
Need(day5.SpeedupOneMinute==oldSpeedups+2&&day5.ResourceRewards.Packets.Count==0,"day five retains direct speedups, zero fake packets");
Need(day5.Mailbox.Messages.Any(m=>m.EventId==dailyLetter),"informational day five mail exists");
Need(!Send(five,"day5-nothing",ResourceRewardCommands.ClaimMail,dailyLetter).Ok,"day five cannot fake packet claim");
Need(Send(five,"day5-mail-delete","MailDelete",dailyLetter).Ok,"no locked resource means mail may be deleted");
// A full wallet never forfeits an earned packet. Only USE is blocked.
var full=new PlayerState();full.Resources.Wood=int.MaxValue;
var fullStore=new JsonStore(full);
var fullGateway=new LocalGateway(clock,fullStore);fullGateway.Advance();
Need(Send(fullGateway,"day1-full-wallet","ClaimDailyTreasure","").Ok,"full wallet still receives packet");
var fullPack=fullGateway.Snapshot().ResourceRewards.Packets.Single();
Need(Send(fullGateway,"day1-full-mail",ResourceRewardCommands.ClaimMail,dailyLetter).Ok,"full wallet still allows collecting packet");
Need(!Send(fullGateway,"day1-use-overflow",ResourceRewardCommands.UsePacket,fullPack.ReceiptId).Ok,"overflow rejects use only");
Need(!fullGateway.Snapshot().ResourceRewards.Packets.Single().Used,"overflow keeps packet unspent");


// Historical mail tombstones survive deletion. In a mismatched/imported
// daily-cycle save, never issue packets with no visible letter to claim.
var tombState=new PlayerState();
tombState.Mailbox.ReceivedEventIds.Add(dailyLetter);
var tombStore=new JsonStore(tombState);
var tombGateway=new LocalGateway(clock,tombStore);tombGateway.Advance();
int tombWrites=tombStore.Writes;
Need(!Send(tombGateway,"daily-deleted-mail-tombstone","ClaimDailyTreasure","").Ok,
    "deleted mail event tombstone blocks orphan daily packets");
var tombResult=tombGateway.Snapshot();
Need(tombStore.Writes==tombWrites&&tombResult.DailyTreasures.TotalClaims==0,
    "rejected tombstone daily claim does not advance or save");
Need(tombResult.ResourceRewards.Packets.Count==0,
    "deleted mail tombstone cannot strand new unclaimable reward");

// Real chapter producers, not manually seeded packets. Owner-runtime mission
// milestones grant to the saved BAG only, including legacy non-tier amounts.
// The existing authoritative node/hunt/building paths remain direct wallets.
SliceContentProfiles.SetRuntimeProfileOverride(SliceContentProfiles.OwnerIiiId);
try
{
 var missionSeed=new PlayerState{BastionLevel=1,SawmillLevel=1};
 int missionWood=missionSeed.Resources.Wood,missionStone=missionSeed.Resources.Stone;
 var missionStore=new JsonStore(missionSeed);
 var missionGateway=new LocalGateway(clock,missionStore);
 var earned=missionGateway.Snapshot();
 Need(earned.ChapterProgress.GrantedRewards.Contains("c1-sawmill"),
     "real chapter milestone is durably recorded");
 var woodPrize=earned.ResourceRewards.Packets.Single(x=>x.ReceiptId=="mission:c1-sawmill:wood");
 Need(woodPrize.Origin=="mission"&&woodPrize.Kind=="wood"&&woodPrize.Amount==120&&woodPrize.Collected&&!woodPrize.Used,
     "real owner chapter source emits exact saved 120-wood packet");
 Need(earned.Resources.Wood==missionWood&&earned.Resources.Stone==missionStone,
     "receiving real mission prize never deposits directly into wallet");
 Need(AttentionCueRules.At(earned.RewardAttention,"bag")==AttentionLevel.New,
     "real mission producer raises bag novelty, not forced consumption");
 int missionWrites=missionStore.Writes;
 missionGateway.Advance();
 Need(missionStore.Writes==missionWrites&&missionGateway.Snapshot().ResourceRewards.Packets.Count==1,
     "repeated mission advancement cannot duplicate packet or commit");
 missionGateway=new LocalGateway(clock,missionStore);
 Need(Send(missionGateway,"real-mission-use",ResourceRewardCommands.UsePacket,woodPrize.ReceiptId).Ok,
     "real earned mission packet can be spent through the saved gateway");
 Need(missionGateway.Snapshot().Resources.Wood==missionWood+120,
     "mission wallet receives exactly 120 only after real Use");
 Need(!Send(missionGateway,"real-mission-replay",ResourceRewardCommands.UsePacket,woodPrize.ReceiptId).Ok,
     "earned mission packet cannot be re-used under another command ID");
 missionGateway=new LocalGateway(clock,missionStore);
 Need(missionGateway.Snapshot().Resources.Wood==missionWood+120 &&
      missionGateway.Snapshot().ResourceRewards.Packets.Single().Used,
     "mission reward and spent receipt survive authoritative JSON reload");
 var routeSeed=new PlayerState();
 routeSeed.ChapterProgress.RouteCleared=true;
 int routeWood=routeSeed.Resources.Wood,routeStone=routeSeed.Resources.Stone;
 var routeStore=new JsonStore(routeSeed);
 var routeGateway=new LocalGateway(clock,routeStore);
 var route=routeGateway.Snapshot();
 Need(route.ChapterProgress.GrantedRewards.Contains("c1-route"),
     "real two-resource route milestone paid once");
 Need(route.ResourceRewards.Packets.Count(p=>p.Origin=="mission")==2 &&
      route.ResourceRewards.Packets.Any(p=>p.ReceiptId=="mission:c1-route:wood"&&p.Amount==90) &&
      route.ResourceRewards.Packets.Any(p=>p.ReceiptId=="mission:c1-route:stone"&&p.Amount==55),
     "route milestone pays indivisible exact 90-wood and 55-stone bundle");
 Need(route.Resources.Wood==routeWood&&route.Resources.Stone==routeStone,
     "two-resource mission remains outside wallet until player uses each packet");
 routeGateway=new LocalGateway(clock,routeStore);
 Need(Send(routeGateway,"real-route-stone",ResourceRewardCommands.UsePacket,"mission:c1-route:stone").Ok,
     "one mission bundle resource may be used independently");
 Need(routeGateway.Snapshot().Resources.Stone==routeStone+55 &&
      routeGateway.Snapshot().Resources.Wood==routeWood,
     "using stone does not secretly consume wood");
 Need(routeGateway.Snapshot().ResourceRewards.Packets.Count==2 &&
      !routeGateway.Snapshot().ResourceRewards.Packets.Single(p=>p.Kind=="wood").Used,
     "other mission packet remains available after partial player consumption");
}
finally { SliceContentProfiles.SetRuntimeProfileOverride(null); }

// The player's irreversible Bastion IV priority also produces a durable
// bag bonus. These three alternatives are not normal building production.
foreach(var choice in new[]{"production","defense","civil"})
{
 var choiceSeed=new PlayerState{BastionLevel=4};
 var choiceStore=new JsonStore(choiceSeed);
 var choiceGateway=new LocalGateway(clock,choiceStore);
 var before=choiceGateway.Snapshot();
 int baseWood=before.Resources.Wood,baseStone=before.Resources.Stone,baseFood=before.Resources.Food;
 int basePower=before.MissionPower;
 Need(Send(choiceGateway,"choice-"+choice,"ChooseDevelopment",choice).Ok,
     "Bastion IV real development choice accepted: "+choice);
 var awarded=choiceGateway.Snapshot();
 var bundles=awarded.ResourceRewards.Packets.Where(p=>p.Origin=="development").ToList();
 Need(awarded.ChapterThree.DevelopmentPriority==choice&&bundles.Count==2,
     "development choice persisted and issued exactly two resource packets");
 int expectWood=choice=="production"?260:choice=="civil"?180:0;
 int expectStone=choice=="production"?180:choice=="defense"?220:0;
 int expectFood=choice=="civil"?280:0;
 Need(bundles.All(p=>p.Collected&&!p.Used&&p.MailEventId=="") &&
      bundles.Where(p=>p.Kind=="wood").Sum(p=>p.Amount)==expectWood &&
      bundles.Where(p=>p.Kind=="stone").Sum(p=>p.Amount)==expectStone &&
      bundles.Where(p=>p.Kind=="food").Sum(p=>p.Amount)==expectFood,
     "development choice produces exact durable bonus components");
 Need(awarded.Resources.Wood==baseWood&&awarded.Resources.Stone==baseStone&&
      awarded.Resources.Food==baseFood,
     "choosing development never invisibly credits wallet");
 Need(awarded.MissionPower==basePower+(choice=="defense"?100:0),
     "defense choice still grants immediate non-resource mission power");
 choiceGateway=new LocalGateway(clock,choiceStore);
 Need(!Send(choiceGateway,"choice-repeat-"+choice,"ChooseDevelopment",choice).Ok,
     "priority cannot pay again after JSON reload");
 Need(choiceGateway.Snapshot().ResourceRewards.Packets.Count(p=>p.Origin=="development")==2,
     "reload and replay cannot duplicate development bonus");
}

Console.WriteLine("RESOURCE_REAL_GATEWAY_PASS checks="+checks+"; actual JSON save/reload; NOT Unity/UI/mobile proof");
sealed class FixedClock:IClock{public long UtcTicks{get;set;}}
sealed class JsonStore:IStateStore{
 static readonly JsonSerializerOptions options=new JsonSerializerOptions{IncludeFields=true};string json;public int Writes;
 public JsonStore(PlayerState state){json=JsonSerializer.Serialize(state,options);}public PlayerState Load()=>JsonSerializer.Deserialize<PlayerState>(json,options);
 public void Save(PlayerState state){json=JsonSerializer.Serialize(state,options);Writes++;}
}
