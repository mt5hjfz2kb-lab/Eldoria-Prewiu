using System;
using System.Linq;
using System.Text.Json;
using Eldoria.Domain;
using Eldoria.Application;
using Eldoria.LegacyProgression;

static class IntegrationChecks
{
    sealed class Clock:IClock
    {
        public long UtcTicks{get;set;}=new DateTime(2026,11,24,10,0,0,DateTimeKind.Utc).Ticks;
        public void Add(int seconds)=>UtcTicks+=TimeSpan.FromSeconds(seconds).Ticks;
    }
    sealed class Store:IStateStore
    {
        static readonly JsonSerializerOptions Options=new JsonSerializerOptions{IncludeFields=true};
        public string Bytes;
        public PlayerState Load()=>Bytes==null?null:JsonSerializer.Deserialize<PlayerState>(Bytes,Options);
        public void Save(PlayerState s)=>Bytes=JsonSerializer.Serialize(s,Options);
    }
    static int checks;
    static void Require(bool condition,string message)
    {if(!condition)throw new Exception(message);checks++;}
    static GameCommand Command(LocalGateway g,string id,string kind,string target)
    {var s=g.Snapshot();return new GameCommand(id,s.PlayerId,kind,target,s.Revision);}
    static void Main()
    {
        var clock=new Clock();var store=new Store();
        var original=new PlayerState{BastionLevel=5,SawmillLevel=1,BarracksLevel=1,
            Available=new ArmyRoster{ArcherT1=60},ChapterProgress=new ChapterProgressState{MarchConfirmed=true},
            Resources=new ResourceWallet{Wood=500,Stone=500,Food=500}};
        original.ChapterProgress.SeenNarrative.AddRange(new[]{"vigilants-introduction","scouts-introduction"});
        original.ChapterThree.AshStalkerDefeated=true;
        // Serialize pre-feature state (missing field) to exercise additive migration.
        store.Save(original);store.Bytes=store.Bytes.Replace("\"LegacyProgression\":null,","");
        var gateway=new LocalGateway(clock,store);
        var migrated=gateway.Snapshot();
        Require(migrated.LegacyProgression!=null&&migrated.LegacyProgression.EarnedTrophyIds.Contains("bastion-5"),"migrate verified B5 milestone");
        Require(migrated.LegacyProgression.Ledger.Receipts.Count==0,"no invented historical events");
        Require(store.Load().LegacyProgression!=null,"migration persisted");
        var initialPower=SliceRules.TotalPower(migrated).Total;
        var selection=Command(gateway,"select","SelectLegacyBanner","banner-founder");
        Require(gateway.Execute(selection).Ok,"real owner selects earned banner");
        var selectedRevision=gateway.Snapshot().Revision;
        Require(gateway.Execute(selection).Ok&&gateway.Snapshot().Revision==selectedRevision,"selection replay no second commit");
        Require(!gateway.Execute(Command(gateway,"locked","SelectLegacyMonument","monument-hunt")).Ok,"reject locked item");
        var current=gateway.Snapshot();
        Require(!gateway.Execute(new GameCommand("other","imposter","SelectLegacyBanner","",current.Revision)).Ok,"reject other owner");
        Require(SliceRules.TotalPower(gateway.Snapshot()).Total==initialPower,"cosmetics do not alter power");
        gateway=new LocalGateway(clock,store);
        Require(gateway.Snapshot().LegacyProgression.SelectedBanner=="banner-founder","selection survives serialized reload");
        var stage=Command(gateway,"v1","VigilantBattle","1");
        Require(gateway.Execute(stage).Ok&&gateway.Snapshot().Retention.LastClearedStage==1,"real guardian battle won");
        Require(gateway.Snapshot().LegacyProgression.Ledger.Receipts.Count(r=>r.Kind==ReceiptKind.VigilantClear)==1,"one guardian receipt");
        Require(gateway.Execute(stage).Ok&&gateway.Snapshot().LegacyProgression.Ledger.Receipts.Count(r=>r.Kind==ReceiptKind.VigilantClear)==1,"battle replay no receipt duplication");
        var order=gateway.Snapshot().Retention.Orders.First(o=>!o.Claimed);
        Require(gateway.Execute(Command(gateway,"dispatch","ScoutDispatch",order.Id)).Ok,"dispatch real scout");
        Require(!gateway.Execute(Command(gateway,"too-early","ScoutClaim",order.Id)).Ok,"early claim rejected");
        clock.Add(EarlyRetentionRules.ScoutDurationSeconds(order.Kind)+1);
        gateway=new LocalGateway(clock,store);
        var claim=Command(gateway,"claim","ScoutClaim",order.Id);
        Require(gateway.Execute(claim).Ok,"completed scout claim");
        var after=gateway.Snapshot();
        Require(after.LegacyProgression.Ledger.Receipts.Count(r=>r.Kind==ReceiptKind.ScoutDiscovery)==1,"only completed discovery counts");
        Require(after.LegacyProgression.EarnedTrophyIds.Contains("scout-1"),"first scout trophy");
        Require(gateway.Execute(claim).Ok&&gateway.Snapshot().LegacyProgression.Ledger.Receipts.Count(r=>r.Kind==ReceiptKind.ScoutDiscovery)==1,"scout replay");
        int prestige=after.LegacyProgression.Prestige;
        after.LegacyProgression.EarnedTrophyIds.Clear();after.LegacyProgression.Ledger.Receipts.Clear();
        gateway=new LocalGateway(clock,store);
        Require(gateway.Snapshot().LegacyProgression.Prestige==prestige,"snapshot cannot mutate saved achievements");
        Require(LegacyPlayerAdapter.Read(gateway).Prestige==prestige,"profile reads canonical earned prestige");
        var beforeGather=gateway.Snapshot();
        // The authoritative return path, including offline deadlines and the
        // same receipt that credits resources. No new direct wallet command.
        beforeGather.March=new MarchState{MarchId="test-return",TargetId="forest-valoria",
            Phase="returning",PhaseEndsUtcTicks=clock.UtcTicks-1,PendingWood=75,Troops=new ArmyRoster{ArcherT1=1}};
        store.Save(beforeGather);
        gateway=new LocalGateway(clock,store);
        var collected=gateway.Snapshot();
        Require(collected.LegacyProgression.Ledger.Receipts.Count(r=>r.Kind==ReceiptKind.GatherReturn&&r.Amount==75)==1,"offline gather actual credited quantity");
        var total=collected.Resources.Wood;
        gateway=new LocalGateway(clock,store);
        Require(gateway.Snapshot().Resources.Wood==total&&gateway.Snapshot().LegacyProgression.Ledger.Receipts.Count(r=>r.Kind==ReceiptKind.GatherReturn)==1,"offline return replay no double reward or score");
        Console.WriteLine("PASS: "+checks+" canonical integration assertions on temporary copy");
    }
}
