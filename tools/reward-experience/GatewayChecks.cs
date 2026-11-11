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
Console.WriteLine("RESOURCE_REAL_GATEWAY_PASS checks="+checks+"; actual JSON save/reload; NOT Unity/UI/mobile proof");
sealed class FixedClock:IClock{public long UtcTicks{get;set;}}
sealed class JsonStore:IStateStore{
 static readonly JsonSerializerOptions options=new JsonSerializerOptions{IncludeFields=true};string json;public int Writes;
 public JsonStore(PlayerState state){json=JsonSerializer.Serialize(state,options);}public PlayerState Load()=>JsonSerializer.Deserialize<PlayerState>(json,options);
 public void Save(PlayerState state){json=JsonSerializer.Serialize(state,options);Writes++;}
}
