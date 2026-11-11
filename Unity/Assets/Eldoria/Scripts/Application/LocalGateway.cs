using System;
using Eldoria.Domain;

namespace Eldoria.Application
{
    public interface IClock { long UtcTicks { get; } }
    public interface IRandomSource { int Next(int exclusiveMax); }
    public interface IStateStore { PlayerState Load(); void Save(PlayerState state); }
    public interface IContentCatalog { string DisplayName(string id); }
    public interface ICommandGateway
    {
        PlayerState Snapshot();
        CommandResult Execute(GameCommand command);
        bool Advance();
    }
    public sealed class GameCommand
    {
        public readonly string Id, ActorId, Kind, TargetId;
        public readonly long ExpectedRevision;
        public GameCommand(string id, string actorId, string kind, string targetId, long expectedRevision)
        { Id = id; ActorId = actorId; Kind = kind; TargetId = targetId; ExpectedRevision = expectedRevision; }
    }
    public sealed class CommandResult
    {
        public readonly bool Ok;
        public readonly string Message;
        public readonly long Revision;
        public CommandResult(bool ok, string message, long revision)
        { Ok = ok; Message = message; Revision = revision; }
    }
    // The local authority receives sites verified against the imported terrain. A
    // remote world must supply its own authoritative snapshot; clients never roll sites.
    public interface IWorldResourcePlacement
    {
        bool TryChoose(PlayerState snapshot, string targetId, string previousSiteId,
            int generation, out WorldResourceLocation location);
    }
    public interface IWorldCreaturePlacement
    {
        bool TryChoose(PlayerState snapshot,string targetId,string previousSiteId,int generation,out WorldCreatureLocation location);
    }
    public interface IWorldMarchRoutes
    {
        bool TryRoute(PlayerState snapshot,string targetId,out System.Collections.Generic.List<WorldRoutePoint> route);
    }
    public sealed class LocalGateway : ICommandGateway
    {
        string ReceiptOrigin(string id){
            if(IsWoodTarget(id))return "la expedición de madera";
            if(IsStoneTarget(id))return "la expedición de piedra";
            switch(id){case "meat-valoria":return "la reserva de carne";
                case "hunt-wolf":return "la caza de lobos";case "hunt-boar":return "la caza de jabalí";
                case "ash-stalker-valoria":return "el Acechador de Ceniza";
                case "fissure-valoria":return "la fisura";case "engendro-valoria":return "el Engendro";
                case "corrupt-scout":return "la ruta corrupta";default:return "la expedición";}
        }
        private readonly IClock clock;
        private readonly IStateStore store;
        private readonly IEldoriaVersionedBattleResolver battleResolver;
        private PlayerState state;
        private IWorldResourcePlacement resourcePlacement;
        private long resourceRespawnTicks;
        private IWorldMarchRoutes marchRoutes;
        private IWorldCreaturePlacement creaturePlacement;
        private long creatureRespawnTicks;
        public void ConfigureLocalMarchRoutes(IWorldMarchRoutes routes){if(state.WorldId!="world-local")throw new InvalidOperationException("Remote route authority required");marchRoutes=routes??throw new ArgumentNullException(nameof(routes));}
        public void ConfigureLocalResourceRespawn(IWorldResourcePlacement placement, TimeSpan delay)
        {
            if (placement == null) throw new ArgumentNullException(nameof(placement));
            if (delay <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delay));
            if (state.WorldId != "world-local")
                throw new InvalidOperationException("Shared worlds require a remote authoritative population snapshot.");
            resourcePlacement = placement;
            resourceRespawnTicks = delay.Ticks;
            Advance();
        }
        public void EnsureLocalRenewablePopulation(int woodCount, int stoneCount)
        {
            if(resourcePlacement==null||state.WorldId!="world-local")throw new InvalidOperationException("Local terrain authority required");
            if(woodCount<0||stoneCount<0||woodCount>100||stoneCount>100)throw new ArgumentOutOfRangeException("population");
            bool changed=false;
            foreach(string kind in new[]{"wood","stone"})
                for(int n=1;n<=(kind=="wood"?woodCount:stoneCount);n++)
                {
                    string id="region1:"+kind+":"+n.ToString("D3",System.Globalization.CultureInfo.InvariantCulture);
                    if(state.WorldResourceLocations.Exists(x=>x.TargetId==id))continue;
                    state.WorldResourceLocations.Add(new WorldResourceLocation{TargetId=id,ResourceKind=kind,Capacity=kind=="wood"?1250:5200,RespawnUtcTicks=clock.UtcTicks});changed=true;
                }
            changed|=AdvanceResourcePopulation();if(changed)Commit();
        }
        public void ConfigureLocalCreatureRespawn(IWorldCreaturePlacement placement,TimeSpan delay)
        {
            if(placement==null)throw new ArgumentNullException(nameof(placement));
            if(delay<=TimeSpan.Zero)throw new ArgumentOutOfRangeException(nameof(delay));
            if(state.WorldId!="world-local")throw new InvalidOperationException("Shared creatures require remote server authority");
            creaturePlacement=placement;creatureRespawnTicks=delay.Ticks;Advance();
        }
        public void EnsureLocalCreaturePopulation(int boarCount,int spawnlingCount)
        {
            if(creaturePlacement==null||state.WorldId!="world-local")throw new InvalidOperationException("Local creature site authority required");
            if(boarCount<0||spawnlingCount<0||boarCount>60||spawnlingCount>60)throw new ArgumentOutOfRangeException("creature population");
            bool changed=false;
            foreach(var kind in new[]{"boar","spawnling"})
                for(int n=1;n<=(kind=="boar"?boarCount:spawnlingCount);n++)
                {
                    var id="region1:"+kind+":"+n.ToString("D3",System.Globalization.CultureInfo.InvariantCulture);
                    if(state.WorldCreatureLocations.Exists(x=>x.TargetId==id))continue;
                    state.WorldCreatureLocations.Add(new WorldCreatureLocation{TargetId=id,CreatureKind=kind,Level=1,RespawnUtcTicks=clock.UtcTicks});
                    changed=true;
                }
            changed|=AdvanceCreaturePopulation();
            if(changed)Commit();
        }
        public static WorldCreatureLocation Creature(PlayerState snapshot,string targetId)
            =>snapshot.WorldCreatureLocations?.Find(x=>x.TargetId==targetId);
        private bool AdvanceCreaturePopulation()
        {
            if(creaturePlacement==null)return false;
            bool changed=false;
            foreach(var record in state.WorldCreatureLocations)
            {
                if (!record.Alive && record.DefeatedUtcTicks > 0 && record.RespawnUtcTicks > 0)
                {
                    // Saved 15-minute deadlines must become eligible five
                    // minutes after the ORIGINAL defeat, including offline.
                    long migrated=RenewablePopulationPolicy.MigrateExistingDue(
                        record.DefeatedUtcTicks,record.RespawnUtcTicks);
                    if (migrated != record.RespawnUtcTicks)
                    { record.RespawnUtcTicks=migrated;changed=true; }
                }
                if(record.Alive||record.RespawnUtcTicks>clock.UtcTicks||MarchTargetActive(record.TargetId))continue;
                if(!creaturePlacement.TryChoose(Snapshot(),record.TargetId,record.SiteId,record.Generation+1,out var chosen))continue;
                if(chosen==null||chosen.TargetId!=record.TargetId||chosen.CreatureKind!=record.CreatureKind||
                    chosen.Level<1||chosen.Level>5||string.IsNullOrEmpty(chosen.SiteId)||chosen.SiteId==record.SiteId||
                    float.IsNaN(chosen.X)||float.IsInfinity(chosen.X)||float.IsNaN(chosen.Y)||float.IsInfinity(chosen.Y)||
                    float.IsNaN(chosen.Z)||float.IsInfinity(chosen.Z))
                    throw new InvalidOperationException("Invalid authoritative creature site or level");
                record.SiteId=chosen.SiteId;record.X=chosen.X;record.Y=chosen.Y;record.Z=chosen.Z;
                record.Level=chosen.Level;record.Generation++;record.RespawnUtcTicks=0;record.DefeatedUtcTicks=0;changed=true;
            }
            return changed;
        }
        public static string ResourceKind(PlayerState snapshot,string targetId)
        {
            if(targetId=="forest-valoria")return "wood";
            if(targetId=="quarry-valoria")return "stone";
            var item=snapshot.WorldResourceLocations?.Find(x=>x.TargetId==targetId);
            return item?.ResourceKind=="wood"||item?.ResourceKind=="stone"?item.ResourceKind:null;
        }
        public static int ResourceRemaining(PlayerState snapshot,string targetId)
        {
            if(targetId=="forest-valoria")return snapshot.ForestRemaining;
            if(targetId=="quarry-valoria")return snapshot.QuarryRemaining;
            return snapshot.WorldResourceLocations?.Find(x=>x.TargetId==targetId)?.Remaining??0;
        }
        bool IsWoodTarget(string id)=>ResourceKind(state,id)=="wood";
        bool IsStoneTarget(string id)=>ResourceKind(state,id)=="stone";
        void SetResourceRemaining(string id,int amount)
        {
            if(id=="forest-valoria")state.ForestRemaining=amount;
            else if(id=="quarry-valoria")state.QuarryRemaining=amount;
            else state.WorldResourceLocations.Find(x=>x.TargetId==id).Remaining=amount;
        }
        // Only canonical wood/stone families participate. Unique campaign
        // encounters deliberately have no population record and never respawn here.
        private bool AdvanceResourcePopulation()
        {
            if (resourcePlacement == null) return false;
            bool changed = false;
            var ids=new System.Collections.Generic.List<string>{"forest-valoria","quarry-valoria"};
            foreach(var existing in state.WorldResourceLocations)
                if(existing.TargetId!="forest-valoria"&&existing.TargetId!="quarry-valoria")ids.Add(existing.TargetId);
            foreach (string id in ids)
            {
                bool depleted = ResourceRemaining(state,id)<=0;
                if (!depleted) continue;
                var item = state.WorldResourceLocations.Find(x => x.TargetId == id);
                if (item == null)
                {
                    item = new WorldResourceLocation { TargetId = id };
                    state.WorldResourceLocations.Add(item);
                }
                if (item.RespawnUtcTicks == 0)
                {
                    item.RespawnUtcTicks = checked((item.DepletedUtcTicks>0?item.DepletedUtcTicks:clock.UtcTicks) + resourceRespawnTicks);
                    changed = true;
                }
                else if (item.DepletedUtcTicks > 0)
                {
                    // Rebase a 30-minute legacy deadline to event+300s.
                    // Do not fabricate an event timestamp for old saves.
                    long migrated=RenewablePopulationPolicy.MigrateExistingDue(
                        item.DepletedUtcTicks,item.RespawnUtcTicks);
                    if(migrated!=item.RespawnUtcTicks)
                    {item.RespawnUtcTicks=migrated;changed=true;}
                }
                // Keep the return leg anchored to its original destination.
                if (item.RespawnUtcTicks > clock.UtcTicks || MarchTargetActive(id)) continue;
                if (!resourcePlacement.TryChoose(Snapshot(), id, item.SiteId, item.Generation + 1, out var next)) continue;
                if (next == null || next.TargetId != id || string.IsNullOrEmpty(next.SiteId) ||
                    next.SiteId == item.SiteId || float.IsNaN(next.X) || float.IsInfinity(next.X) ||
                    float.IsNaN(next.Y) || float.IsInfinity(next.Y) || float.IsNaN(next.Z) || float.IsInfinity(next.Z))
                    throw new InvalidOperationException("Invalid authoritative resource placement");
                item.SiteId = next.SiteId; item.X = next.X; item.Y = next.Y; item.Z = next.Z;
                int nextGeneration=item.Generation+1;
                string kind=IsWoodTarget(id)?"wood":"stone";
                int nodeLevel=id.StartsWith("region1:",StringComparison.Ordinal)
                    ?ResourceNodeTierPolicy.ForNewSite(id,next.SiteId,nextGeneration):1;
                item.Level=nodeLevel;
                item.Capacity=ResourceNodeTierPolicy.Capacity(kind,nodeLevel);
                item.Generation=nextGeneration;
                item.RespawnUtcTicks = 0; item.DepletedUtcTicks=0;
                SetResourceRemaining(id,item.Capacity);
                changed = true;
            }
            return changed;
        }
        public LocalGateway(IClock clock, IStateStore store, IEldoriaVersionedBattleResolver battleResolver=null)
        {
            this.clock = clock; this.store = store; this.battleResolver=battleResolver;
            state = store.Load() ?? new PlayerState();
            if (state.SchemaVersion != 1) throw new InvalidOperationException("Unsupported Eldoria save schema");
#if UNITY_6000_0_OR_NEWER || REWARD_EXPERIENCE_FEATURE
            if(state.ResourceRewardsVersion<0 || state.ResourceRewardsVersion>1)
                throw new InvalidOperationException("Unsupported resource reward save version");
            state.ResourceRewardsVersion=1;
            if(state.ResourceRewards==null)state.ResourceRewards=new ResourceRewardInventory();
            if(state.ResourceRewards.Packets==null)
                state.ResourceRewards.Packets=new System.Collections.Generic.List<ResourceRewardPacket>();
            if(state.ResourceGuidance==null)state.ResourceGuidance=new GuidedTutorialProgress();
            if(state.ResourceGuidance.Completed==null)state.ResourceGuidance.Completed=new System.Collections.Generic.List<string>();
            if(state.ResourceGuidance.Dismissed==null)state.ResourceGuidance.Dismissed=new System.Collections.Generic.List<string>();
            if(state.RewardAttention==null)state.RewardAttention=new AttentionCueLedger();
            if(state.RewardAttention.Records==null)state.RewardAttention.Records=new System.Collections.Generic.List<AttentionCueRecord>();
#endif
            // An absent nickname in schema-1 legacy saves is a normal first
            // entry. Never replace the stable save owner/player ID with it.
            if (state.PlayerName == null) state.PlayerName = "";
            if (state.PlayerName.Length > 0 &&
                (!PlayerIdentityPolicy.TryNormalize(state.PlayerName,out var savedName,out _) ||
                 !string.Equals(savedName,state.PlayerName,StringComparison.Ordinal)))
                throw new InvalidOperationException("Invalid persisted display name");
            if (state.WorldResourceLocations == null)
                state.WorldResourceLocations = new System.Collections.Generic.List<WorldResourceLocation>();
            if (state.WorldCreatureLocations == null)
                state.WorldCreatureLocations = new System.Collections.Generic.List<WorldCreatureLocation>();
            var creatureIds=new System.Collections.Generic.HashSet<string>();
            foreach(var creature in state.WorldCreatureLocations)
                if(creature==null||string.IsNullOrWhiteSpace(creature.TargetId)||
                   (creature.CreatureKind!="boar"&&creature.CreatureKind!="spawnling")||
                   !creature.TargetId.StartsWith("region1:"+creature.CreatureKind+":",StringComparison.Ordinal)||
                   !creatureIds.Add(creature.TargetId)||creature.Level<1||creature.Level>5)
                    throw new InvalidOperationException("Duplicate or invalid persistent creature identity");
            var resourceIds = new System.Collections.Generic.HashSet<string>();
            foreach (var item in state.WorldResourceLocations)
            {
                if (item == null || string.IsNullOrEmpty(item.TargetId) || (item.TargetId != "forest-valoria" && item.TargetId != "quarry-valoria" && !(item.TargetId.StartsWith("region1:wood:",StringComparison.Ordinal)&&item.ResourceKind=="wood") && !(item.TargetId.StartsWith("region1:stone:",StringComparison.Ordinal)&&item.ResourceKind=="stone")) || !resourceIds.Add(item.TargetId))
                    throw new InvalidOperationException("Invalid or duplicate persisted resource identity");
                // Preserve legacy partially mined stock and infer its tier from
                // the original site capacity, never from the remaining amount.
                if (item.TargetId.StartsWith("region1:",StringComparison.Ordinal))
                {
                    if (item.Level == 0 && item.Generation > 0)
                        item.Level = ResourceNodeTierPolicy.ExistingSaveLevel(item.ResourceKind,item.Capacity);
                    if (item.Level < 0 || item.Level > ResourceNodeTierPolicy.MaxLevel)
                        throw new InvalidOperationException("Invalid persisted resource tier");
                }
            }
            if(state.March==null)state.March=new MarchState();
            if(state.ExtraMarches==null)state.ExtraMarches=new System.Collections.Generic.List<MarchState>();
            if(state.ExtraMarches.Count>MarchCapacityRules.MaxConcurrentMarches-1 || state.ExtraMarches.Exists(x=>x==null))
                throw new InvalidOperationException("Invalid persistent march slots");
            foreach(var existingMarch in MarchCapacityRules.WorldMarches(state))
            {
                if(existingMarch.VersionedBattle?.IsLegacyPlaceholder==true)existingMarch.VersionedBattle=null;
                if(existingMarch.VersionedBattle!=null && existingMarch.Phase!="idle" &&
                    (battleResolver==null || existingMarch.VersionedBattle.RulesVersion!=battleResolver.RulesVersion))
                    throw new InvalidOperationException("The persisted march requires its original combat resolver");
            }
#if UNITY_6000_0_OR_NEWER || DAILY_TREASURE_FEATURE
            if(state.DailyTreasures==null)state.DailyTreasures=new DailyTreasureState();
#endif
            if(state.Retention==null)state.Retention=new EarlyRetentionState();
            if(state.Retention.Orders==null)state.Retention.Orders=new System.Collections.Generic.List<ScoutOrder>();
            if(state.Mailbox==null)state.Mailbox=new EldoriaV41Mailbox();
            if(state.Mailbox.Messages==null)state.Mailbox.Messages=new System.Collections.Generic.List<EldoriaV41Message>();
            if(state.Mailbox.ReceivedEventIds==null)state.Mailbox.ReceivedEventIds=new System.Collections.Generic.List<string>();
            if(state.PreparedHeroIds==null || state.PreparedHeroIds.Count!=3)
            {
                state.PreparedHeroIds=new System.Collections.Generic.List<string>{
                    state.PreparedHeroId=="lyra"?"lyra":"aldric",
                    state.PreparedHeroId=="aldric+lyra"?"lyra":null,null
                };
            }
            if(state.March!=null && state.March.HeroIds==null)
                state.March.HeroIds=new System.Collections.Generic.List<string>();
            if(state.ExtraMarches!=null)
                foreach(var m in state.ExtraMarches)if(m!=null && m.HeroIds==null)
                    m.HeroIds=new System.Collections.Generic.List<string>();
            if(state.ChapterThree==null)state.ChapterThree=new BastionThreeState();
            if(state.ChapterThree.Inventory==null)state.ChapterThree.Inventory=new System.Collections.Generic.List<string>();
            if(state.ChapterThree.FissureClosed){state.ChapterThree.LyraRecruited=true;state.ChapterThree.InvestigationEndsUtcTicks=0;}
            // Schema-1 saves may have confirmed an army before hero capacity existed.
            // Trim only that unreserved selection; never change home or travelling troops.
            bool migratedPrepared=state.MarchConfigured && state.PreparedTroops!=null &&
                state.PreparedTroops.Total>MarchCapacityRules.Capacity(state,state.PreparedHeroId);
            if(migratedPrepared)
            {
                state.PreparedTroops=MarchCapacityRules.AutoSelect(state.PreparedTroops,
                    MarchCapacityRules.Capacity(state,state.PreparedHeroId));
                if(state.ChapterProgress!=null)state.ChapterProgress.ConfirmedExpeditionPower=
                    SliceRules.Expedition(state.PreparedTroops,state.PreparedHeroId).Power;
            }
            NormalizeMissionProgress();
            if(!Advance() && migratedPrepared)Commit();
        }
        private void NormalizeMissionProgress()
        {
            if (state.ChapterProgress == null) state.ChapterProgress = new ChapterProgressState();
            if (state.ChapterProgress.GrantedRewards == null)
                state.ChapterProgress.GrantedRewards = new System.Collections.Generic.List<string>();
            if (state.ChapterProgress.SeenNarrative == null)
                state.ChapterProgress.SeenNarrative = new System.Collections.Generic.List<string>();
            if (state.ScoutDefeated) state.ChapterProgress.RouteCleared = true;
            if (state.BastionLevel >= 2) state.ChapterProgress.BastionTwoReached = true;
            if (state.MarchConfigured)
            {
                state.ChapterProgress.MarchConfirmed = true;
                if (state.ChapterProgress.ConfirmedExpeditionPower <= 0)
                    state.ChapterProgress.ConfirmedExpeditionPower =
                        SliceRules.Expedition(state.PreparedTroops,state.PreparedHeroId).Power;
            }
            if (state.EngendroDefeated) state.ChapterProgress.EngendroDefeated = true;
        }

        public PlayerState Snapshot()
        {
            var s = state;
            var c = new PlayerState {
                SchemaVersion=s.SchemaVersion, RulesVersion=s.RulesVersion, Revision=s.Revision,
                PlayerId=s.PlayerId, PlayerName=s.PlayerName, RealmId=s.RealmId, WorldId=s.WorldId,
                Resources=new ResourceWallet { Wood=s.Resources.Wood, Stone=s.Resources.Stone, Food=s.Resources.Food },
#if UNITY_6000_0_OR_NEWER || REWARD_EXPERIENCE_FEATURE
                ResourceRewardsVersion=s.ResourceRewardsVersion,
                ResourceRewards=s.ResourceRewards?.Copy()??new ResourceRewardInventory(),
                ResourceGuidance=s.ResourceGuidance?.Copy()??new GuidedTutorialProgress(),
                RewardAttention=s.RewardAttention?.Copy()??new AttentionCueLedger(),
#endif
                BastionLevel=s.BastionLevel, SawmillLevel=s.SawmillLevel, BarracksLevel=s.BarracksLevel,
                MissionPower=s.MissionPower, ChapterThree=s.ChapterThree.Copy(),
                Retention=s.Retention?.Copy()??new EarlyRetentionState(),
#if UNITY_6000_0_OR_NEWER || DAILY_TREASURE_FEATURE
                DailyTreasures=s.DailyTreasures?.Copy()??new DailyTreasureState(),
#endif
                CorruptionDiscovered=s.CorruptionDiscovered, ScoutDefeated=s.ScoutDefeated,
                JourneyComplete=s.JourneyComplete, EngendroDefeated=s.EngendroDefeated,
                Available=s.Available.Copy(), Wounded=s.Wounded.Copy(),
                MarchConfigured=s.MarchConfigured, PreparedHeroId=s.PreparedHeroId,
                PreparedHeroIds=new System.Collections.Generic.List<string>(s.PreparedHeroIds??new System.Collections.Generic.List<string>()),
                PreparedTroops=s.PreparedTroops.Copy(),
                Mailbox=s.Mailbox?.Copy()??new EldoriaV41Mailbox(),
                ExtraMarches=s.ExtraMarches.ConvertAll(x=>x.Copy()),
                March=new MarchState { MarchId=s.March.MarchId, OwnerId=s.March.OwnerId,
                    TargetId=s.March.TargetId, HeroId=s.March.HeroId, Troops=s.March.Troops.Copy(),
                    VersionedBattle=s.March.VersionedBattle?.Copy(),
                    HeroIds=new System.Collections.Generic.List<string>(s.March.HeroIds??new System.Collections.Generic.List<string>()),
                    Route=s.March.Route?.ConvertAll(x=>x.Copy())??new System.Collections.Generic.List<WorldRoutePoint>(),
                    Phase=s.March.Phase, PhaseEndsUtcTicks=s.March.PhaseEndsUtcTicks,
                    PendingFood=s.March.PendingFood, PendingWood=s.March.PendingWood, PendingStone=s.March.PendingStone },
                WorldResourceLocations=s.WorldResourceLocations.ConvertAll(x=>x.Copy()),
                WorldCreatureLocations=s.WorldCreatureLocations.ConvertAll(x=>x.Copy()),
                ForestRemaining=s.ForestRemaining, QuarryRemaining=s.QuarryRemaining, BuildingCompletesUtcTicks=s.BuildingCompletesUtcTicks,
                BuildingTaskId=s.BuildingTaskId,
                BastionUpgradeStartedUtcTicks=s.BastionUpgradeStartedUtcTicks,
                BastionUpgradeCompletesUtcTicks=s.BastionUpgradeCompletesUtcTicks,
                BastionUpgradeTaskId=s.BastionUpgradeTaskId,
                RecruitmentCompletesUtcTicks=s.RecruitmentCompletesUtcTicks,
                RecruitmentTaskId=s.RecruitmentTaskId, PendingRecruitArchers=s.PendingRecruitArchers,
                PendingRecruitTier=s.PendingRecruitTier,
                SpeedupOneMinute=s.SpeedupOneMinute, SpeedupFiveMinutes=s.SpeedupFiveMinutes,
                LastBattleReason=s.LastBattleReason,
                LastBattleReport=new BattleReportState {
                    TargetId=s.LastBattleReport?.TargetId??"",
                    HeroId=s.LastBattleReport?.HeroId??"aldric",
                    Troops=s.LastBattleReport?.Troops??0,
                    OwnRoster=s.LastBattleReport?.OwnRoster?.Copy(),
                    VersionedOutcome=s.LastBattleReport?.VersionedOutcome?.Copy(),
                    PlayerPower=s.LastBattleReport?.PlayerPower??0,
                    EnemyPower=s.LastBattleReport?.EnemyPower??0,
                    Won=s.LastBattleReport?.Won??false,
                    Rounds=s.LastBattleReport?.Rounds??0,
                    RemainingHealth=s.LastBattleReport?.RemainingHealth??0,
                    RewardWood=s.LastBattleReport?.RewardWood??0,
                    RewardStone=s.LastBattleReport?.RewardStone??0,
                    RewardFood=s.LastBattleReport?.RewardFood??0,
                    Reason=s.LastBattleReport?.Reason??""
                },
                ChapterProgress=new ChapterProgressState {
                    GatheredWood=s.ChapterProgress?.GatheredWood??0,
                    GatheredStone=s.ChapterProgress?.GatheredStone??0,
                    TrainedArchers=s.ChapterProgress?.TrainedArchers??0,
                    ConfirmedExpeditionPower=s.ChapterProgress?.ConfirmedExpeditionPower??0,
                    RouteCleared=s.ChapterProgress?.RouteCleared??false,
                    BastionTwoReached=s.ChapterProgress?.BastionTwoReached??false,
                    MarchConfirmed=s.ChapterProgress?.MarchConfirmed??false,
                    EngendroDefeated=s.ChapterProgress?.EngendroDefeated??false,
                    GrantedRewards=new System.Collections.Generic.List<string>(s.ChapterProgress?.GrantedRewards??new System.Collections.Generic.List<string>()),
                    SeenNarrative=new System.Collections.Generic.List<string>(s.ChapterProgress?.SeenNarrative??new System.Collections.Generic.List<string>())
                },
                CompletedCommandIds=new System.Collections.Generic.List<string>(s.CompletedCommandIds),
                CompletedTaskIds=new System.Collections.Generic.List<string>(s.CompletedTaskIds)
            };
            return c;
        }
        public CommandResult Execute(GameCommand command)
        {
            // Reject revisions superseded by another command. Elapsed-time
            // processing performed by THIS command must not invalidate its
            // own fresh snapshot (for example a just-returned Scout claim).
            var observedRevision=state.Revision;
            Advance();
            if (command == null || string.IsNullOrEmpty(command.Id)) return Fail("Comando sin ID");
            if (state.CompletedCommandIds.Contains(command.Id)) return new CommandResult(true, "Ya ejecutado", state.Revision);
            if (command.ActorId != state.PlayerId) return Fail("Propietario incorrecto");
            if (command.ExpectedRevision != observedRevision) return Fail("Estado desactualizado");
            if (command.Kind == "ChoosePlayerName")
            {
                if (!string.IsNullOrWhiteSpace(state.PlayerName))
                    return Fail("Ya has elegido tu nombre");
                if (!PlayerIdentityPolicy.TryNormalize(command.TargetId,out var chosenName,out var nameError))
                    return Fail(nameError);
                state.PlayerName = chosenName;
                state.CompletedCommandIds.Add(command.Id);
                Commit();
                return new CommandResult(true,"Nombre guardado",state.Revision);
            }
            var early=EarlyRetentionRules.Apply(state,command.Kind,command.TargetId,
                clock.UtcTicks,out string earlyMessage,battleResolver as IEldoriaCampaignBattleResolver);
            if(early.HasValue)
            {
                if(!early.Value)return Fail(earlyMessage);
                if(command.Kind=="VigilantBattle" && state.LastBattleReport!=null &&
                    (state.LastBattleReport.TargetId??"").StartsWith("vigilant:",StringComparison.Ordinal))
                    state.Mailbox.Receive(new EldoriaV41Message {
                        EventId="vigilant:"+command.Id,Category="informes",Type="battle",
                        Title=state.LastBattleReport.VersionedOutcome?.Outcome=="draw"?"Empate en los Vigilantes":state.LastBattleReport.Won?"Victoria en los Vigilantes":"Derrota en los Vigilantes",
                        Subtitle=state.LastBattleReport.Reason,
                        ReportId="vigilant:"+command.Id,OccurredAtUtcTicks=clock.UtcTicks,
                        Battle=state.LastBattleReport.VersionedOutcome?.Copy()??new EldoriaV41CombatRecord {
                            TargetId=state.LastBattleReport.TargetId,HeroId=state.LastBattleReport.HeroId,
                            Troops=state.LastBattleReport.Troops,
                            OwnRoster=state.LastBattleReport.OwnRoster?.Copy(),
                            Won=state.LastBattleReport.Won,
                            PlayerPower=state.LastBattleReport.PlayerPower,
                            EnemyPower=state.LastBattleReport.EnemyPower,
                            Rounds=state.LastBattleReport.Rounds,
                            RemainingHealth=state.LastBattleReport.RemainingHealth,
                            RewardWood=state.LastBattleReport.RewardWood,
                            RewardStone=state.LastBattleReport.RewardStone,
                            RewardFood=state.LastBattleReport.RewardFood,
                            Reason=state.LastBattleReport.Reason
                        }
                    });
                if(command.Kind=="ScoutClaim")
                {
                    // Bind the historical message to the actually claimed order.
                    // Do not read a mutable "latest discovery" when rebuilding an
                    // older message; never reroll narrative or grant another reward.
                    var claimed=state.Retention?.Orders?.Find(o=>
                        o!=null&&o.Id==command.TargetId&&o.Claimed);
                    state.Mailbox.Receive(new EldoriaV41Message {
                        EventId="exploration:"+command.Id,Category="informes",Type="exploration",
                        Title="Informe de exploración",
                        Subtitle=claimed!=null?EarlyRetentionRules.ScoutDiscoveryTitle(claimed):
                            state.Retention?.LatestDiscoveryTitle??"Exploración concluida",
                        DiscoveryText=claimed!=null?EarlyRetentionRules.ScoutDiscoveryText(claimed):"",
                        DiscoveryKind=claimed?.Kind??-1,
                        DiscoveryOrderId=claimed?.Id??"",
                        RewardReceiptId=command.Id,OccurredAtUtcTicks=clock.UtcTicks
                    });
                }
                // A claim frees a board slot. Refill/migration belongs to THIS
                // accepted revision, not the next command's Advance preflight.
                // Otherwise the next fresh UI claim can reject itself as stale.
                EarlyRetentionRules.Synchronize(state,clock.UtcTicks);
                state.CompletedCommandIds.Add(command.Id);
                Commit();
                return new CommandResult(true,earlyMessage,state.Revision);
            }
            var chapterThreeResult=ExecuteChapterThree(command);
            if(chapterThreeResult!=null)return chapterThreeResult;
            switch (command.Kind)
            {
#if UNITY_6000_0_OR_NEWER || DAILY_TREASURE_FEATURE
                case "ClaimDailyTreasure":
                    {
                        // Claim daily progress, but never credit resource wallet yet.
                        // Packages are attached to a claimable letter, then bag use
                        // separately credits the actual amount exactly once.
                        var preview=DailyTreasureRules.Preview(state.DailyTreasures,clock.UtcTicks);
                        if(!preview.IsValid)return Fail("El tributo de hoy ya fue reclamado");
                        var mailEventId="daily-treasure:"+DailyTreasureRules.UtcDay(clock.UtcTicks);
                        // Tombstones persist after mail deletion. Refuse a replay
                        // even when the visible letter has disappeared.
                        if(state.Mailbox.ReceivedEventIds.Contains(mailEventId) ||
                           state.Mailbox.Messages.Exists(m=>m!=null&&m.EventId==mailEventId))
                            return Fail("El tributo de este día ya tiene un correo registrado");
#if UNITY_6000_0_OR_NEWER || REWARD_EXPERIENCE_FEATURE
                        bool hasPackets=preview.Wood>0||preview.Stone>0||preview.Food>0;
                        ResourceRewardInventory pendingInventory=null;
                        if(hasPackets)
                        {
                            // Validate a COPY before changing state: failed or
                            // replayed grants cannot advance a streak or save.
                            pendingInventory=state.ResourceRewards.Copy();
                            if(!ResourceRewardRules.TryIssueBundle(pendingInventory,
                                mailEventId,"daily",preview.Wood,preview.Stone,
                                preview.Food,mailEventId))
                                return Fail("El tributo ya tiene paquetes registrados");
                        }
#else
                        // Historical daily-only smoke without resource feature.
                        if(state.Resources.Wood>int.MaxValue-preview.Wood ||
                           state.Resources.Stone>int.MaxValue-preview.Stone ||
                           state.Resources.Food>int.MaxValue-preview.Food)
                            return Fail("No hay capacidad para recibir el tributo");
#endif
                        if(state.SpeedupOneMinute>int.MaxValue-preview.SpeedupOneMinute ||
                           state.SpeedupFiveMinutes>int.MaxValue-preview.SpeedupFiveMinutes)
                            return Fail("No hay capacidad para recibir el tributo");
                        if(!DailyTreasureRules.TryClaim(state.DailyTreasures,clock.UtcTicks,
                            out var received))return Fail("El tributo ya no está disponible");
#if UNITY_6000_0_OR_NEWER || REWARD_EXPERIENCE_FEATURE
                        if(pendingInventory!=null)state.ResourceRewards=pendingInventory;
#else
                        state.Resources.Wood+=received.Wood;
                        state.Resources.Stone+=received.Stone;
                        state.Resources.Food+=received.Food;
#endif
                        state.SpeedupOneMinute+=received.SpeedupOneMinute;
                        state.SpeedupFiveMinutes+=received.SpeedupFiveMinutes;
                        string summary=DailyTreasureRules.Receipt(received);
#if UNITY_6000_0_OR_NEWER || REWARD_EXPERIENCE_FEATURE
                        if(hasPackets)
                            summary="Los paquetes esperan en este correo. Reclámalos y úsalos en la mochila. "+
                                summary.Replace("Ya recibidos: ","Contenido: ");
#endif
                        state.Mailbox.Receive(new EldoriaV41Message {
                            EventId=mailEventId,Category="recompensas",Type="daily-treasure",
                            Title="Tributo de Valoria · día "+received.CycleDay,
                            Subtitle=summary,RewardReceiptId=command.Id,
                            OccurredAtUtcTicks=clock.UtcTicks
                        });
#if UNITY_6000_0_OR_NEWER || REWARD_EXPERIENCE_FEATURE
                        if(hasPackets)
                            AttentionCueRules.Signal(state.RewardAttention,
                                "mail-reward:"+mailEventId,"mail",AttentionLevel.ActionReady);
#endif
                        break;
                    }
#endif
#if UNITY_6000_0_OR_NEWER || REWARD_EXPERIENCE_FEATURE
                case ResourceRewardCommands.ClaimMail:
                    if(!state.Mailbox.Messages.Exists(m=>m!=null&&m.EventId==command.TargetId))
                        return Fail("Correo no encontrado");
                    if(ResourceRewardRules.ClaimMail(state.ResourceRewards,command.TargetId)<=0)
                        return Fail("Este correo no tiene paquetes pendientes");
                    state.Mailbox.MarkRead(command.TargetId);
                    AttentionCueRules.Resolve(state.RewardAttention,"mail-reward:"+command.TargetId,true);
                    foreach(var packet in state.ResourceRewards.Packets)
                        if(packet!=null&&packet.MailEventId==command.TargetId&&packet.Collected&&!packet.Used&&!packet.SeenInBag)
                            AttentionCueRules.Signal(state.RewardAttention,"bag-reward:"+packet.ReceiptId,"bag",AttentionLevel.New);
                    GuidedTutorialRules.ConfirmAction(state.ResourceGuidance,
                        ResourceRewardCommands.FirstMailClaimStep,ResourceRewardCommands.ClaimMail,command.Kind,true);
                    break;
                case ResourceRewardCommands.UsePacket:
                    if(!ResourceRewardRules.TryUse(state.ResourceRewards,state.Resources,command.TargetId,out var packetGranted))
                        return Fail("Paquete no disponible o capacidad insuficiente");
                    AttentionCueRules.Resolve(state.RewardAttention,"bag-reward:"+command.TargetId,true);
                    GuidedTutorialRules.ConfirmAction(state.ResourceGuidance,
                        ResourceRewardCommands.FirstUseStep,ResourceRewardCommands.UsePacket,command.Kind,true);
                    break;
                case ResourceRewardCommands.MarkBagSeen:
                    if(!ResourceRewardRules.MarkBagSeen(state.ResourceRewards,command.TargetId))
                        return Fail("Paquete nuevo no encontrado");
                    AttentionCueRules.MarkViewed(state.RewardAttention,"bag-reward:"+command.TargetId);
                    break;
                case ResourceRewardCommands.DismissGuidance:
                    if(command.TargetId!=ResourceRewardCommands.FirstUseStep&&
                       command.TargetId!=ResourceRewardCommands.FirstMailClaimStep)
                        return Fail("Paso de ayuda desconocido");
                    GuidedTutorialRules.Dismiss(state.ResourceGuidance,command.TargetId);
                    break;
#endif
                case "MailRead":
                    if(state.Mailbox.Messages==null ||
                       !state.Mailbox.Messages.Exists(m=>m!=null&&m.EventId==command.TargetId))
                        return Fail("Correo no encontrado");
                    state.Mailbox.MarkRead(command.TargetId);
#if UNITY_6000_0_OR_NEWER || REWARD_EXPERIENCE_FEATURE
                    AttentionCueRules.MarkViewed(state.RewardAttention,"mail-reward:"+command.TargetId);
#endif
                    break;
                case "MailDelete":
                    if(state.Mailbox.Messages==null ||
                       !state.Mailbox.Messages.Exists(m=>m!=null&&m.EventId==command.TargetId))
                        return Fail("Correo no encontrado");
#if UNITY_6000_0_OR_NEWER || REWARD_EXPERIENCE_FEATURE
                    if(state.ResourceRewards.Packets.Exists(p=>p!=null&&p.MailEventId==command.TargetId&&!p.Collected))
                        return Fail("Reclama los paquetes antes de eliminar el correo");
                    AttentionCueRules.Resolve(state.RewardAttention,"mail-reward:"+command.TargetId,true);
#endif
                    state.Mailbox.Delete(command.TargetId);
                    break;
                case "Gather":
                    if (!IsWoodTarget(command.TargetId) && !IsStoneTarget(command.TargetId))
                        return Fail("Nodo desconocido");
                    if (IsWoodTarget(command.TargetId) && ResourceRemaining(state,command.TargetId)<=0)
                        return Fail("El bosque está agotado");
                    if (IsStoneTarget(command.TargetId) && ResourceRemaining(state,command.TargetId)<=0)
                        return Fail("La cantera está agotada");
                    if (!CanDepart()) return Fail("Marcha no disponible");
                    if(!Depart(command.TargetId))return Fail("No hay una ruta transitable hasta el objetivo");
                    if(IsWoodTarget(command.TargetId)) state.CorruptionDiscovered=true;
                    break;
                case "ConfigureMarch":
                    if (state.BastionLevel < 2 || state.BarracksLevel < 1)
                        return Fail("Marcha no disponible");
                    if(!MarchCapacityRules.CanStart(state,clock.UtcTicks))return Fail("Ya hay tres marchas activas");
                    // Legacy command plus explicit three-tier composition and optional
                    // three hero slots, validated atomically against the current unlocks.
                    var parts=(command.TargetId??"").Split(':');
                    if(parts[0]!="march-main" || (parts.Length!=1&&parts.Length!=2&&parts.Length!=3&&parts.Length!=5&&parts.Length!=7))
                        return Fail("Composición desconocida");
                    string marchHero=parts.Length==3?parts[2]:parts.Length>=5?parts[4]:"aldric";
                    var heroSlots=new System.Collections.Generic.List<string>();
                    if(parts.Length==7)
                    {
                        heroSlots.Add(parts[4]);
                        heroSlots.Add(string.IsNullOrEmpty(parts[5])?null:parts[5]);
                        heroSlots.Add(string.IsNullOrEmpty(parts[6])?null:parts[6]);
                        var unlocked=new System.Collections.Generic.List<string>{"aldric"};
                        if(state.BastionLevel>=3 && state.ChapterThree.LyraRecruited)unlocked.Add("lyra");
                        var checkedSlots=new string[3];
                        try
                        {
                            for(int i=0;i<3;i++)
                                checkedSlots=EldoriaV41Planning.SelectHero(checkedSlots,i,heroSlots[i],unlocked);
                        }
                        catch(ArgumentException){return Fail("Héroe no disponible o duplicado");}
                        marchHero=checkedSlots[1]!=null || checkedSlots[2]!=null?"aldric+lyra":checkedSlots[0];
                        // Two heroes (Aldric+Lyra) are supported today; third is visually reserved,
                        // never used for future story characters without an approved combat contract.
                    }
                    else
                    {
                        if(marchHero!="aldric" && (state.BastionLevel<3 || !state.ChapterThree.LyraRecruited || (marchHero!="lyra" && marchHero!="aldric+lyra")))
                            return Fail("Héroe no disponible");
                        heroSlots=new System.Collections.Generic.List<string>{
                            marchHero=="lyra"?"lyra":"aldric",
                            marchHero=="aldric+lyra"?"lyra":null,null};
                    }
                    if(string.IsNullOrEmpty(heroSlots[0]))return Fail("Selecciona un líder");
                    foreach(var heroId in heroSlots)
                        if(!MarchCapacityRules.HeroAvailable(state,heroId))
                            return Fail("El héroe ya está en otra marcha");
                    var nextTroops=MarchCapacityRules.AutoSelect(state.Available,
                        MarchCapacityRules.Capacity(state,marchHero));
                    if(parts.Length>1)
                    {
                        nextTroops=new ArmyRoster(); // Explicit legacy composition must not retain auto-selected new families.
                        if(!int.TryParse(parts[1],out nextTroops.ArcherT1))return Fail("Composición desconocida");
                        nextTroops.ArcherT2=0;nextTroops.ArcherT3=0;
                    }
                    if(parts.Length>=5 &&
                        (!int.TryParse(parts[2],out nextTroops.ArcherT2)||
                         !int.TryParse(parts[3],out nextTroops.ArcherT3)))
                        return Fail("Composición desconocida");
                    if(!MarchCapacityRules.Fits(nextTroops,state.Available,MarchCapacityRules.Capacity(state,marchHero)))
                        return Fail("La marcha supera la capacidad del héroe o no tiene tropas disponibles");
                    state.PreparedHeroId=marchHero;
                    state.PreparedHeroIds=heroSlots;
                    state.PreparedTroops=nextTroops;
                    state.MarchConfigured = true;
                    if (state.ChapterProgress == null) state.ChapterProgress = new ChapterProgressState();
                    state.ChapterProgress.MarchConfirmed = true;
                    state.ChapterProgress.ConfirmedExpeditionPower =
                        SliceRules.Expedition(state.PreparedTroops,state.PreparedHeroId).Power;
                    break;
                case "Fight":
                    var creature=Creature(state,command.TargetId);
                    if(creature!=null)
                    {
                        if(!creature.Alive)return Fail("La criatura está derrotada o reapareciendo");
                        if(!CanDepart())return Fail("Marcha no disponible");
                        if(!Depart(command.TargetId))return Fail("No hay una ruta transitable hasta la criatura");
                        break;
                    }
                    if (command.TargetId == "corrupt-scout")
                    {
                        if (state.ScoutDefeated) return Fail("La ruta ya está despejada");
                    }
                    else if (command.TargetId == "engendro-valoria")
                    {
                        if (state.BastionLevel < 2 || state.BarracksLevel < 1) return Fail("Refuerza primero el Cuartel");
                        if (state.EngendroDefeated) return Fail("El Engendro ya ha sido derrotado");
                        if (!state.MarchConfigured) return Fail("Prepara y confirma la Marcha en Valoria antes de atacar");
                        if ((state.ChapterProgress?.TrainedArchers ?? 0) < SliceContentProfiles.Active.Chapter2TrainArchers)
                            return Fail("Completa primero el entrenamiento de arqueros del Bastión II");
                        var preparedPower = SliceRules.Expedition(state.PreparedTroops,state.PreparedHeroId).Power;
                        if (preparedPower < SliceContentProfiles.Active.Chapter2ExpeditionPower)
                            return Fail("La Marcha preparada no alcanza el Poder de expedición requerido");
                        if (state.PreparedTroops.ArcherT1 > state.Available.ArcherT1 ||
                            state.PreparedTroops.ArcherT2 > state.Available.ArcherT2 ||
                            state.PreparedTroops.ArcherT3 > state.Available.ArcherT3)
                            return Fail("La composición preparada ya no está disponible; vuelve a confirmar la Marcha");
                    }
                    else return Fail("Amenaza desconocida");
                    if (!CanDepart()) return Fail("Marcha no disponible");
                    if(!Depart(command.TargetId))return Fail("No hay una ruta transitable hasta el objetivo"); break;
                case "AdvanceBastion":
                    if (command.TargetId != "bastion") return Fail("El Bastión todavía no puede ascender");
                    BastionProgressionDefinition next;
                    try { next = BastionProgressionCatalog.ForLevel(state.BastionLevel + 1); }
                    catch (ArgumentOutOfRangeException) { return Fail("No hay un siguiente nivel de Bastión disponible"); }
                    if (!BastionProgressionCatalog.RequirementsMet(state,next))
                    {
                        var missing=BastionProgressionCatalog.MissingSummary(state,next);
                        return Fail(string.IsNullOrEmpty(missing)?"El Bastión todavía no puede ascender":"Falta: "+missing);
                    }
                    state.Resources.Wood -= next.WoodCost;
                    state.Resources.Stone -= next.StoneCost;
                    state.Resources.Food -= next.FoodCost;
                    state.BastionUpgradeStartedUtcTicks = clock.UtcTicks;
                    state.BastionUpgradeCompletesUtcTicks = clock.UtcTicks +
                        TimeSpan.FromSeconds(next.DurationSeconds).Ticks;
                    state.BastionUpgradeTaskId = "bastion"+next.Level+":" + command.Id;
                    break;
                case "Build":
                    if (state.BuildingCompletesUtcTicks > 0 || state.BastionUpgradeCompletesUtcTicks > 0) return Fail("Ya hay una obra en curso");
                    if (command.TargetId == "sawmill")
                    {
                        if (state.SawmillLevel != 0) return Fail("Obra no disponible");
                        if (state.Resources.Wood < SliceRules.SawmillWoodCost) return Fail("Falta madera: visita el bosque");
                        state.Resources.Wood -= SliceRules.SawmillWoodCost;
                        state.BuildingTaskId = "sawmill:" + command.Id;
                        state.BuildingCompletesUtcTicks = clock.UtcTicks + TimeSpan.FromSeconds(SliceRules.SawmillBuildSeconds).Ticks;
                    }
                    else if (command.TargetId == "barracks")
                    {
                        if (state.BastionLevel < 2 || state.BarracksLevel != 0) return Fail("Cuartel no disponible");
                        if (state.Resources.Wood < SliceRules.BarracksWoodCost || state.Resources.Stone < SliceRules.BarracksStoneCost)
                            return Fail("Faltan recursos para el Cuartel");
                        state.Resources.Wood -= SliceRules.BarracksWoodCost;
                        state.Resources.Stone -= SliceRules.BarracksStoneCost;
                        state.BuildingTaskId = "barracks:" + command.Id;
                        state.BuildingCompletesUtcTicks = clock.UtcTicks + TimeSpan.FromSeconds(SliceRules.BarracksBuildSeconds).Ticks;
                    }
                    else return Fail("Obra no disponible");
                    break;
                case "UseSpeedup":
                    {
                        string[] speed=command.TargetId.Split(':');
                        if(speed.Length!=2||(speed[0]!="building"&&speed[0]!="bastion"&&speed[0]!="training")||
                            (speed[1]!="1m"&&speed[1]!="5m"))return Fail("Aceleración desconocida");
                        bool five=speed[1]=="5m";
                        if(five?state.SpeedupFiveMinutes<=0:state.SpeedupOneMinute<=0)return Fail("No quedan aceleradores");
                        long deadline=speed[0]=="building"?state.BuildingCompletesUtcTicks:
                            speed[0]=="bastion"?state.BastionUpgradeCompletesUtcTicks:state.RecruitmentCompletesUtcTicks;
                        if(deadline<=clock.UtcTicks)return Fail("No hay tarea activa que acelerar");
                        long advanced=Math.Max(clock.UtcTicks,deadline-TimeSpan.FromMinutes(five?5:1).Ticks);
                        if(speed[0]=="building")state.BuildingCompletesUtcTicks=advanced;
                        else if(speed[0]=="bastion")state.BastionUpgradeCompletesUtcTicks=advanced;
                        else state.RecruitmentCompletesUtcTicks=advanced;
                        if(five)state.SpeedupFiveMinutes--;else state.SpeedupOneMinute--;
                        break;
                    }
                case "Recruit":
                    if (state.BastionLevel < 2 || state.BarracksLevel < 1)
                        return Fail("Reclutamiento no disponible");
                    int recruitCount=SliceRules.RecruitArchers;
                    int recruitTier=1;
                    if(command.TargetId != "archer:t1")
                    {
                        if(command.TargetId.StartsWith("archer:t1:",StringComparison.Ordinal))
                        {
                            if(!int.TryParse(command.TargetId.Substring("archer:t1:".Length),out recruitCount))
                                return Fail("Cantidad de reclutas no válida");
                        }
                        else if(command.TargetId.StartsWith("archer:t2:",StringComparison.Ordinal))
                        {
                            recruitTier=2;
                            if(!int.TryParse(command.TargetId.Substring("archer:t2:".Length),out recruitCount))
                                return Fail("Cantidad de reclutas no válida");
                        }
                        else return Fail("Tipo de unidad desconocido");
                    }
                    if(recruitTier==2 && (state.BarracksLevel<4||state.BastionLevel<4))
                        return Fail("Arqueros T2 requieren Cuartel nivel IV");
                    if(recruitCount<1 || recruitCount>500) return Fail("Cantidad de reclutas no válida");
                    if (state.RecruitmentCompletesUtcTicks > 0) return Fail("Ya hay reclutas entrenando");
                    int recruitWood=SliceRules.RecruitWoodCostFor(recruitCount);
                    int recruitStone=SliceRules.RecruitStoneCostFor(recruitCount);
                    if (state.Resources.Wood < recruitWood || state.Resources.Stone < recruitStone)
                        return Fail("Faltan recursos para equipar reclutas");
                    state.Resources.Wood -= recruitWood;
                    state.Resources.Stone -= recruitStone;
                    state.PendingRecruitArchers = recruitCount;
                    state.PendingRecruitTier = recruitTier;
                    state.RecruitmentTaskId = "recruit:" + command.Id;
                    state.RecruitmentCompletesUtcTicks = clock.UtcTicks +
                        TimeSpan.FromSeconds(SliceRules.RecruitSecondsFor(recruitCount)).Ticks;
                    break;
                default: return Fail("Acción desconocida");
            }
            GrantWebMissionRewards();
            // A real ConfigureMarch can unlock the Scout Board. Materialize
            // its first three contracts in THIS SAME accepted revision: if
            // deferred until the next Execute()->Advance(), the first genuine
            // attack after preparing a march would reject a fresh command as
            // stale when its own preflight silently created scout orders.
            EarlyRetentionRules.Synchronize(state,clock.UtcTicks);
            state.CompletedCommandIds.Add(command.Id);
            Commit();
#if UNITY_6000_0_OR_NEWER || DAILY_TREASURE_FEATURE
            return new CommandResult(true, command.Kind=="ClaimDailyTreasure" ?
                "Tributo registrado; revisa el correo" : "Acción iniciada", state.Revision);
#else
            return new CommandResult(true,"Acción iniciada",state.Revision);
#endif
        }
        private bool MarchTargetActive(string targetId)
        {
            foreach(var active in MarchCapacityRules.WorldMarches(state))
                if(active.Phase!="idle"&&active.TargetId==targetId)return true;
            return false;
        }
        // An investigation occupies ONE of three global slots, not the entire kingdom.
        private bool CanDepart() => MarchCapacityRules.CanStart(state,clock.UtcTicks) &&
            state.Available.Total>0;
        private bool Depart(string targetId)
        {
            var planned=new System.Collections.Generic.List<WorldRoutePoint>();
            if(marchRoutes!=null&&!marchRoutes.TryRoute(Snapshot(),targetId,out planned))return false;
            if(planned==null)return false;
            if(!MarchCapacityRules.CanStart(state,clock.UtcTicks))return false;
            // Never attack or resolve the same unique creature twice in parallel.
            if(MarchTargetActive(targetId)&&!IsWoodTarget(targetId)&&!IsStoneTarget(targetId))return false;
            bool special=targetId=="engendro-valoria"||IsChapterThreeTarget(targetId);
            bool usePrepared=state.MarchConfigured&&
                MarchCapacityRules.Fits(state.PreparedTroops,state.Available,
                    MarchCapacityRules.Capacity(state,state.PreparedHeroId));
            if(special&&state.MarchConfigured&&!usePrepared)return false;
            bool gathering=IsWoodTarget(targetId)||IsStoneTarget(targetId)||targetId=="meat-valoria";
            // Automatic supply marches need no named hero. Explicit compositions
            // reserve their heroes until return; a second departure revalidates them.
            string hero=usePrepared?state.PreparedHeroId:gathering?"":
                MarchCapacityRules.HeroAvailable(state,"aldric")?"aldric":
                state.ChapterThree.LyraRecruited&&MarchCapacityRules.HeroAvailable(state,"lyra")?"lyra":"";
            var party=usePrepared?new System.Collections.Generic.List<string>(state.PreparedHeroIds):
                new System.Collections.Generic.List<string>{hero,null,null};
            if(!gathering && string.IsNullOrEmpty(hero))return false;
            foreach(var id in party)if(!MarchCapacityRules.HeroAvailable(state,id))return false;
            var troops=usePrepared?state.PreparedTroops.Copy():
                MarchCapacityRules.AutoSelect(state.Available,MarchCapacityRules.Capacity(state,hero));
            if(!MarchCapacityRules.Fits(troops,state.Available,MarchCapacityRules.Capacity(state,hero)))return false;
            var versioned=gathering||IsChapterThreeTarget(targetId)?null:battleResolver?.Prepare(troops.Copy(),new System.Collections.Generic.List<string>(party),targetId);
            if(versioned==null && !gathering && (!IsChapterThreeTarget(targetId)||targetId=="ash-stalker-valoria") &&
                battleResolver is IEldoriaCampaignBattleResolver campaign && campaign.UsesCampaignCreatures)
            {
                var creature=Creature(state,targetId);
                var stats=targetId=="ash-stalker-valoria"?new WorldCreatureStats(920,132,68,54,0,0):creature!=null?WorldCreatureCatalog.For(creature.CreatureKind,creature.Level):SliceRules.CampaignEnemy(targetId);
                versioned=campaign.PrepareCreature(troops,party,targetId,stats);
                versioned.EnemyAmbush=targetId=="ash-stalker-valoria";
            }
            if(versioned!=null && (versioned.RulesVersion!=battleResolver.RulesVersion || versioned.TargetId!=targetId))
                throw new InvalidOperationException("Combat definition identity mismatch");
            if(versioned!=null)
            {
                if(versioned.OwnTroops==null || versioned.OwnHeroes==null || versioned.OwnHeroes.Length!=party.Count)
                    throw new InvalidOperationException("Incomplete own battle snapshot");
                for(int f=0;f<3;f++)for(int t=1;t<=5;t++)
                    if(versioned.OwnTroops.Get(f,t)!=troops.Get(f,t))throw new InvalidOperationException("Combat cannot change reserved troops");
                for(int h=0;h<party.Count;h++)if(versioned.OwnHeroes[h]!=party[h])throw new InvalidOperationException("Combat cannot change reserved heroes");
            }
            var m=MarchCapacityRules.AvailableSlot(state);
            if(m==null)return false;
            m.Route=planned;
            m.Troops=troops;
            m.VersionedBattle=versioned?.Copy();
            m.HeroId=hero;
            m.HeroIds=party;
            MarchCapacityRules.Reserve(state,troops);
            m.TargetId = targetId; m.Phase = "outbound";
            m.PendingFood = m.PendingWood = m.PendingStone = 0;
            m.PhaseEndsUtcTicks = clock.UtcTicks + TimeSpan.FromSeconds(SliceRules.TravelSeconds).Ticks;
            return true;
        }
        public bool Advance()
        {
            bool changed = AdvanceChapterThree();
            bool newlyOpenedEconomy = false;
            changed |= EarlyRetentionRules.Synchronize(state,clock.UtcTicks);
            if (state.BastionUpgradeCompletesUtcTicks > 0 && state.BastionUpgradeCompletesUtcTicks <= clock.UtcTicks)
            {
                if (!state.CompletedTaskIds.Contains(state.BastionUpgradeTaskId))
                {
                    state.CompletedTaskIds.Add(state.BastionUpgradeTaskId);
                    int targetLevel=2;
                    var prefix=state.BastionUpgradeTaskId.Split(':')[0];
                    if(prefix.StartsWith("bastion",StringComparison.Ordinal)&&int.TryParse(prefix.Substring(7),out var parsed)&&parsed>=2&&parsed<=5)targetLevel=parsed;
                    if(targetLevel>state.BastionLevel){
                        state.BastionLevel=targetLevel;
                        // Only two meaningful reconstruction milestones deserve
                        // an extra narrative letter. These DO NOT pay a reward:
                        // the normal upgrade already settled all real effects.
                        if(targetLevel==2 || targetLevel==5)
                            state.Mailbox.Receive(new EldoriaV41Message {
                                EventId="milestone:bastion:"+targetLevel,
                                Category="narrativa",Type="milestone-bastion",
                                Title="El Bastión alcanza el nivel "+targetLevel,
                                Subtitle=targetLevel==2
                                    ? "Valoria se fortalece. El Cuartel y el siguiente capítulo quedan a tu alcance."
                                    : "La reconstrucción alcanza una nueva etapa. Valoria extiende sus fronteras.",
                                OccurredAtUtcTicks=state.BastionUpgradeCompletesUtcTicks
                            });
                        // TotalPower already awards 600 per Bastion level.
                        state.MissionPower+=BastionProgressionCatalog.ForLevel(targetLevel).PowerReward-600;
                        if(targetLevel==3)
                        {
                            // Newly unlocked production starts at the actual offline completion deadline.
                            state.ChapterThree.EconomyVersion=2;
                            state.ChapterThree.EconomyUtcTicks=state.BastionUpgradeCompletesUtcTicks;
                            state.ChapterThree.WoodProductionMicro=0;
                            state.ChapterThree.StoneProductionMicro=0;
                            state.ChapterThree.FoodProductionMicro=0;
                            newlyOpenedEconomy=true;
                        }
                    }
                    if (state.ChapterProgress == null) state.ChapterProgress = new ChapterProgressState();
                    state.ChapterProgress.BastionTwoReached = true;
                }
                state.BastionUpgradeStartedUtcTicks = 0;
                state.BastionUpgradeCompletesUtcTicks = 0;
                state.BastionUpgradeTaskId = "";
                changed = true;
            }
            // Pay production accrued after the III unlock even on the FIRST offline load.
            // AdvanceChapterThree at method entry ran while this was still Bastion II.
            if(newlyOpenedEconomy)changed |= AdvanceChapterThree();
            if (state.BuildingCompletesUtcTicks > 0 && state.BuildingCompletesUtcTicks <= clock.UtcTicks)
            {
                if (!state.CompletedTaskIds.Contains(state.BuildingTaskId))
                {
                    state.CompletedTaskIds.Add(state.BuildingTaskId);
                    if (state.BuildingTaskId.StartsWith("barracks:")) state.BarracksLevel = 1;
                    else state.SawmillLevel = 1;
                }
                state.BuildingCompletesUtcTicks = 0; state.BuildingTaskId = ""; changed = true;
            }
            if (state.RecruitmentCompletesUtcTicks > 0 && state.RecruitmentCompletesUtcTicks <= clock.UtcTicks)
            {
                if (!state.CompletedTaskIds.Contains(state.RecruitmentTaskId))
                {
                    state.CompletedTaskIds.Add(state.RecruitmentTaskId);
                    if(state.PendingRecruitTier==2)state.Available.ArcherT2+=state.PendingRecruitArchers;
                    else state.Available.ArcherT1+=state.PendingRecruitArchers;
                    if (state.ChapterProgress == null) state.ChapterProgress = new ChapterProgressState();
                    state.ChapterProgress.TrainedArchers += state.PendingRecruitArchers;
                }
                state.RecruitmentCompletesUtcTicks = 0;
                state.RecruitmentTaskId = "";
                state.PendingRecruitArchers = 0;
                state.PendingRecruitTier = 1;
                changed = true;
            }
            // Every one of up to three persisted marches advances independently.
            // A long offline interval may complete all three phases in one call.
            foreach(var m in MarchCapacityRules.WorldMarches(state))
            for (int guard=0; guard<3 && m.Phase != "idle" &&
                m.PhaseEndsUtcTicks <= clock.UtcTicks; guard++)
            {
                long due = m.PhaseEndsUtcTicks;
                if (m.Phase == "outbound")
                {
                    if(m.TargetId=="meat-valoria") {
                        m.Phase="gathering";
                        int seconds=Math.Max(7,Math.Min(35,(state.ChapterThree.MeatRemaining+119)/120));
                        m.PhaseEndsUtcTicks=due+TimeSpan.FromSeconds(seconds).Ticks;
                    }
                    else if(IsChapterThreeTarget(m.TargetId)) {
                        ResolveChapterThreeMarch(m,due);
                        m.Phase="returning";m.PhaseEndsUtcTicks=due+TimeSpan.FromSeconds(SliceRules.TravelSeconds).Ticks;
                    }
                    else if (IsWoodTarget(m.TargetId) || IsStoneTarget(m.TargetId))
                    { m.Phase = "gathering"; m.PhaseEndsUtcTicks = due + TimeSpan.FromSeconds(SliceRules.GatherSeconds).Ticks; }
                    else
                    {
                        var expedition = SliceRules.Expedition(m.Troops, m.HeroId);
                        var creature=Creature(state,m.TargetId);
                        var enemy=creature!=null?WorldCreatureCatalog.For(creature.CreatureKind,creature.Level):default(WorldCreatureStats);
                        var evolved=m.VersionedBattle==null?null:battleResolver.Resolve(m.VersionedBattle.Copy());
                        if(m.VersionedBattle!=null && (evolved==null || evolved.TargetId!=m.TargetId ||
                            (evolved.Outcome!="victory" && evolved.Outcome!="defeat" && evolved.Outcome!="draw")))
                            throw new InvalidOperationException("Invalid versioned battle result");
                        var report=evolved!=null?new CombatReport(evolved.Outcome=="victory",evolved.RemainingHealth,evolved.Rounds,evolved.Reason):
                            creature!=null?SliceRules.FightCreature(expedition,enemy):SliceRules.Fight(expedition,m.TargetId);
                        state.LastBattleReason = report.Reason;
                        int rewardWood = 0, rewardStone = 0;
                        if (report.Won)
                        {
                            if (creature!=null)
                            {
                                rewardWood=enemy.RewardWood;rewardStone=enemy.RewardStone;
                                m.PendingWood=rewardWood;m.PendingStone=rewardStone;
                                creature.DefeatedUtcTicks=due;
                                creature.RespawnUtcTicks=checked(due+creatureRespawnTicks);
                            }
                            else if (m.TargetId == "engendro-valoria")
                            {
                                rewardWood = 120; rewardStone = 100;
                                m.PendingWood = rewardWood; m.PendingStone = rewardStone; state.EngendroDefeated = true;
                                if (state.ChapterProgress == null) state.ChapterProgress = new ChapterProgressState();
                                state.ChapterProgress.EngendroDefeated = true;
                            }
                            else
                            {
                                rewardWood = 80; rewardStone = 70;
                                m.PendingWood = rewardWood; m.PendingStone = rewardStone; state.ScoutDefeated = true;
                                if (state.ChapterProgress == null) state.ChapterProgress = new ChapterProgressState();
                                state.ChapterProgress.RouteCleared = true;
                            }
                        }
                        state.LastBattleReport = new BattleReportState {
                            TargetId = m.TargetId,
                            HeroId = m.HeroId,
                            Troops = m.Troops.Total,
                            OwnRoster = m.Troops?.Copy(),
                            PlayerPower = expedition.Power,
                            EnemyPower = creature!=null?enemy.Health+enemy.Attack+enemy.Defense+enemy.Break:0,
                            Won = report.Won,
                            Rounds = report.Rounds,
                            RemainingHealth = report.RemainingHealth,
                            RewardWood = rewardWood,
                            RewardStone = rewardStone,
                            Reason = report.Reason
                        };
                        if(evolved!=null)
                        {
                            evolved.RewardWood=rewardWood;evolved.RewardStone=rewardStone;
                            evolved.OwnRoster=m.Troops.Copy();
                            state.LastBattleReport.VersionedOutcome=evolved.Copy();
                        }
                        state.Mailbox.Receive(new EldoriaV41Message {
                            EventId="battle:"+m.MarchId+":"+m.TargetId+":"+due,
                            Category="informes",Type="battle",
                            Title=evolved!=null&&evolved.Outcome=="draw"?"Empate en batalla":report.Won?"Victoria en batalla":"Derrota en batalla",
                            Subtitle="Informe de "+m.TargetId,
                            ReportId="battle:"+m.MarchId+":"+m.TargetId+":"+due,
                            OccurredAtUtcTicks=due,
                            Battle=evolved??new EldoriaV41CombatRecord {
                                TargetId=state.LastBattleReport.TargetId,HeroId=state.LastBattleReport.HeroId,
                                Troops=state.LastBattleReport.Troops,
                            OwnRoster=state.LastBattleReport.OwnRoster?.Copy(),
                            Won=state.LastBattleReport.Won,
                                PlayerPower=state.LastBattleReport.PlayerPower,
                                EnemyPower=state.LastBattleReport.EnemyPower,
                                Rounds=state.LastBattleReport.Rounds,
                                RemainingHealth=state.LastBattleReport.RemainingHealth,
                                RewardWood=state.LastBattleReport.RewardWood,
                                RewardStone=state.LastBattleReport.RewardStone,
                                RewardFood=state.LastBattleReport.RewardFood,
                                Reason=state.LastBattleReport.Reason
                            }
                        });
                        m.Phase = "returning"; m.PhaseEndsUtcTicks = due + TimeSpan.FromSeconds(SliceRules.TravelSeconds).Ticks;
                    }
                }
                else if (m.Phase == "gathering")
                {
                    if(m.TargetId=="meat-valoria")
                    {
                        ResolveChapterThreeMarch(m,due);
                    }
                    else if(IsStoneTarget(m.TargetId))
                    {
                        m.PendingStone = Math.Min(SliceRules.QuarryLoad,ResourceRemaining(state,m.TargetId));
                        SetResourceRemaining(m.TargetId,ResourceRemaining(state,m.TargetId)-m.PendingStone);
                    }
                    else
                    {
                        m.PendingWood = Math.Min(SliceRules.ForestLoad,ResourceRemaining(state,m.TargetId));
                        SetResourceRemaining(m.TargetId,ResourceRemaining(state,m.TargetId)-m.PendingWood);
                    }
                    if(m.TargetId!="meat-valoria" && ResourceRemaining(state,m.TargetId)==0){
                        var depleted=state.WorldResourceLocations.Find(x=>x.TargetId==m.TargetId);
                        if(depleted==null){depleted=new WorldResourceLocation{TargetId=m.TargetId};state.WorldResourceLocations.Add(depleted);}
                        depleted.DepletedUtcTicks=due; // actual gathering deadline, including offline advancement
                    }
                    m.Phase = "returning";
                    m.PhaseEndsUtcTicks = due + TimeSpan.FromSeconds(SliceRules.TravelSeconds).Ticks;
                }
                else if (m.Phase == "returning")
                {
                    string id = "return:" + m.MarchId + ":" + due;
                    if (!state.CompletedTaskIds.Contains(id))
                    {
                        state.CompletedTaskIds.Add(id);
                        state.Resources.Food += m.PendingFood;
                        state.Resources.Wood += m.PendingWood;
                        state.Resources.Stone += m.PendingStone;
                        if(m.PendingFood>0 || m.PendingWood>0 || m.PendingStone>0)
                            state.Mailbox.Receive(new EldoriaV41Message {
                                EventId=id,Category="recompensas",Type="march",
                                Title="Regreso de "+ReceiptOrigin(m.TargetId),
                                Subtitle="Ya recibidos: "+m.PendingWood+" madera · "+m.PendingStone+" piedra · "+m.PendingFood+" comida",
                                RewardReceiptId=id,OccurredAtUtcTicks=due
                            });
                        if (IsWoodTarget(m.TargetId) && m.PendingWood > 0)
                        {
                            if (state.ChapterProgress == null) state.ChapterProgress = new ChapterProgressState();
                            state.ChapterProgress.GatheredWood += m.PendingWood;
                        }
                        if (IsStoneTarget(m.TargetId) && m.PendingStone > 0)
                        {
                            if (state.ChapterProgress == null) state.ChapterProgress = new ChapterProgressState();
                            state.ChapterProgress.GatheredStone += m.PendingStone;
                            // Web quarry3/4 are the distant veins. Region1 replaces those
                            // fixed DOM sites with persistent regional stone nodes.
                            if(state.BastionLevel>=5&&m.TargetId.StartsWith("region1:stone:",StringComparison.Ordinal))state.ChapterThree.ExtractionOddity=true;
                        }
                        MarchCapacityRules.Return(state,m.Troops);
                    }
                    m.VersionedBattle=null;
                    m.Troops = new ArmyRoster(); m.PendingFood = m.PendingWood = m.PendingStone = 0;
                    m.TargetId = ""; m.Phase = "idle"; m.PhaseEndsUtcTicks = 0;
                }
                changed = true;
            }
            if (!state.JourneyComplete && MarchCapacityRules.ActiveWorldCount(state)==0)
            {
                if (state.ChapterProgress == null) state.ChapterProgress = new ChapterProgressState();
                bool chapterOneReady =
                    state.SawmillLevel > 0 &&
                    state.ChapterProgress.GatheredWood >= SliceContentProfiles.Active.Chapter1GatherWood &&
                    state.ChapterProgress.GatheredStone >= SliceContentProfiles.Active.Chapter1GatherStone &&
                    state.ChapterProgress.RouteCleared;
                if (chapterOneReady) { state.JourneyComplete = true; changed = true; }
            }
            changed |= AdvanceFrontierRespawns();
            if (GrantWebMissionRewards()) changed = true;
            changed |= AdvanceResourcePopulation();
            changed |= AdvanceCreaturePopulation();
            if (changed) Commit();
            return changed;
        }
        private bool GrantWebMissionRewards()
        {
            // Web chapter rewards belong to the owner profile only. The ledger is saved with
            // progress so offline completion, reload and replay cannot pay a mission twice.
            if (SliceContentProfiles.ActiveRuntimeProfile != SliceContentProfiles.OwnerIiiId) return false;
            var p = state.ChapterProgress;
            if (p == null) return false;
            if (p.GrantedRewards == null) p.GrantedRewards = new System.Collections.Generic.List<string>();
            bool changed = false;
            void Grant(string id, bool complete, int wood=0, int stone=0, int power=0, int food=0, int speedup1=0, int speedup5=0)
            {
                if (!complete || p.GrantedRewards.Contains(id)) return;
                // Validate ALL effects before advancing the chapter receipt.
                // In particular, a failed packet replay or numeric overflow
                // must not mark a mission paid and strand a legitimate reward.
                if (wood < 0 || stone < 0 || food < 0 ||
                    power < 0 || speedup1 < 0 || speedup5 < 0 ||
                    state.MissionPower > int.MaxValue-power ||
                    state.SpeedupOneMinute > int.MaxValue-speedup1 ||
                    state.SpeedupFiveMinutes > int.MaxValue-speedup5) return;
#if UNITY_6000_0_OR_NEWER || REWARD_EXPERIENCE_FEATURE
                // Except for real nodes, hunts and productive buildings,
                // mission resources are durable, voluntarily consumed BAG
                // packets. No wallet credit on mission completion.
                // Receipt identity is a stable chapter milestone, never a
                // transient command ID or mutable UI message.
                string receipt = "mission:" + id;
                if (wood > 0 || stone > 0 || food > 0)
                {
                    if (!ResourceRewardRules.TryIssueBundle(state.ResourceRewards,
                        receipt, "mission", wood, stone, food)) return;
                    foreach (var packet in state.ResourceRewards.Packets)
                        if (packet != null && packet.Origin == "mission" &&
                            (packet.ReceiptId == receipt+":wood" ||
                             packet.ReceiptId == receipt+":stone" ||
                             packet.ReceiptId == receipt+":food"))
                            AttentionCueRules.Signal(state.RewardAttention,
                                "bag-reward:"+packet.ReceiptId,"bag",AttentionLevel.New);
                }
#else
                // Historical non-reward module smoke retains its original
                // wallet-only contract; Unity and the reward-enabled gateway
                // compile the actual durable packet path above.
                if (state.Resources.Wood > int.MaxValue-wood ||
                    state.Resources.Stone > int.MaxValue-stone ||
                    state.Resources.Food > int.MaxValue-food) return;
                state.Resources.Wood += wood;
                state.Resources.Stone += stone;
                state.Resources.Food += food;
#endif
                p.GrantedRewards.Add(id);
                state.MissionPower += power;
                state.SpeedupOneMinute += speedup1;
                state.SpeedupFiveMinutes += speedup5;
                changed = true;
            }
            Grant("c1-sawmill", state.SawmillLevel>0, wood:120);
            Grant("c1-wood", p.GatheredWood>=SliceContentProfiles.OwnerIiiCandidate.Chapter1GatherWood, stone:80);
            Grant("c1-stone", p.GatheredStone>=SliceContentProfiles.OwnerIiiCandidate.Chapter1GatherStone, wood:120);
            Grant("c1-route", p.RouteCleared, wood:90, stone:55);
            Grant("c1-bastion", state.BastionLevel>=2, power:80);
            Grant("chapter-1", state.BastionLevel>=2, wood:180, stone:140, power:120);
            Grant("c2-barracks", state.BarracksLevel>0, wood:140, stone:90);
            Grant("c2-train", p.TrainedArchers>=SliceContentProfiles.OwnerIiiCandidate.Chapter2TrainArchers, wood:220, stone:150);
            Grant("c2-power", p.ConfirmedExpeditionPower>=SliceContentProfiles.OwnerIiiCandidate.Chapter2ExpeditionPower, power:100);
            Grant("c2-spawnling", p.EngendroDefeated, wood:120, stone:120);
            Grant("c2-bastion",state.BastionLevel>=3,food:180);
            Grant("chapter-2",state.BastionLevel>=3,wood:240,stone:200,food:220,power:180);
            if(state.BastionLevel>=3) {
                var c=state.ChapterThree;
                Grant("c3-granary",c.GranaryLevel>0,wood:160,stone:110);
                Grant("c3-food",state.Resources.Food>=500,wood:120);
                Grant("c3-hunt",c.Hunts>=2,food:220);
                Grant("c3-fissure",c.FissureClosed,stone:260,power:100);
                Grant("c3-infra",state.SawmillLevel>=2&&state.BarracksLevel>=2&&c.GranaryLevel>=2,wood:220,stone:180);
                bool infrastructure=state.SawmillLevel>=2&&state.BarracksLevel>=2&&c.GranaryLevel>=2;
                Grant("c3-bastion",state.BastionLevel>=4,food:180);
                Grant("chapter-3",state.BastionLevel>=4&&infrastructure&&c.FissureClosed&&c.Hunts>=2&&p.GrantedRewards.Contains("c3-food"),
                    wood:280,stone:240,food:260,power:220);
                if(state.BastionLevel>=4){
                    Grant("c4-sawmill2",state.SawmillLevel>=2,wood:180);
                    Grant("c4-granary2",c.GranaryLevel>=2,food:220);
                    Grant("c4-barracks2",state.BarracksLevel>=2,stone:180);
                    Grant("c4-priority",!string.IsNullOrEmpty(c.DevelopmentPriority),power:80);
                    Grant("c4-bastion",state.BastionLevel>=5,power:220);
                    Grant("chapter-4",state.BastionLevel>=5&&infrastructure&&!string.IsNullOrEmpty(c.DevelopmentPriority),
                        wood:360,stone:300,food:280,power:260,speedup5:1);
                }
                // Web gives one introductory one-minute speedup for the first
                // compatible chapter-IV task; gathering is never accelerated.
                Grant("intro-speedup1",state.BastionLevel>=4 &&
                    (state.BuildingCompletesUtcTicks>clock.UtcTicks||
                     state.BastionUpgradeCompletesUtcTicks>clock.UtcTicks||
                     state.RecruitmentCompletesUtcTicks>clock.UtcTicks),speedup1:1);
                if(state.BastionLevel>=5){
                    Grant("c5-stoneworks",c.StoneworksLevel>0,wood:220,food:180);
                    Grant("c5-stone",state.Resources.Stone>=1200,wood:240);
                    int troops=state.Available.Total+state.Wounded.Total;
                    foreach(var march in MarchCapacityRules.WorldMarches(state))
                        if(march!=null&&march.Phase!="idle")troops+=march.Troops.Total;
                    Grant("c5-army",troops>=60,wood:260,stone:180);
                    Grant("c5-rare",c.AshStalkerDefeated,food:260,stone:180);
                    // The chapter-V conclusion requires Bastion VI; not authored yet.
                }
            }
            return changed;
        }
        public static bool IsChapterThreeTarget(string id)=>id=="fissure-valoria"||id=="hunt-wolf"||id=="hunt-boar"||id=="meat-valoria"||id=="ash-stalker-valoria";
        CommandResult ExecuteChapterThree(GameCommand command)
        {
            var c=state.ChapterThree;var id=command.TargetId;
            bool handles=command.Kind=="ChooseDevelopment"||command.Kind=="InvestigateFissure"||command.Kind=="ContactLyra"||command.Kind=="UpgradeBuilding"||
                (command.Kind=="Build"&&(id=="granary"||id=="stoneworks"))||((command.Kind=="Fight"||command.Kind=="Gather")&&IsChapterThreeTarget(id));
            if(!handles)return null;
            if(state.BastionLevel<3 && !(state.BastionLevel==2&&command.Kind=="UpgradeBuilding"&&(id=="sawmill"||id=="barracks")))return Fail("Requiere Bastión III");
            switch(command.Kind) {
                case "ChooseDevelopment":
                    if(state.BastionLevel<4||!string.IsNullOrEmpty(c.DevelopmentPriority))return Fail("Prioridad no disponible");
                    if(id!="production"&&id!="defense"&&id!="civil")return Fail("Prioridad desconocida");
                    // A chapter development bonus is NOT building production.
                    // One irreversible priority decision grants one persistent
                    // multi-resource bundle to the bag, without immediate
                    // wallet credit; power remains an immediate progression stat.
                    int bonusWood=id=="production"?260:id=="civil"?180:0;
                    int bonusStone=id=="production"?180:id=="defense"?220:0;
                    int bonusFood=id=="civil"?280:0;
                    int bonusPower=id=="defense"?100:0;
                    if(state.MissionPower>int.MaxValue-bonusPower)
                        return Fail("No hay capacidad para recibir el poder");
#if UNITY_6000_0_OR_NEWER || REWARD_EXPERIENCE_FEATURE
                    if(!ResourceRewardRules.TryIssueBundle(state.ResourceRewards,
                        "development:priority","development",
                        bonusWood,bonusStone,bonusFood))
                        return Fail("El bono de desarrollo ya fue registrado");
                    foreach(var packet in state.ResourceRewards.Packets)
                        if(packet!=null&&packet.Origin=="development"&&
                            (packet.ReceiptId=="development:priority:wood"||
                             packet.ReceiptId=="development:priority:stone"||
                             packet.ReceiptId=="development:priority:food"))
                            AttentionCueRules.Signal(state.RewardAttention,
                                "bag-reward:"+packet.ReceiptId,"bag",AttentionLevel.New);
#else
                    if(state.Resources.Wood>int.MaxValue-bonusWood||
                       state.Resources.Stone>int.MaxValue-bonusStone||
                       state.Resources.Food>int.MaxValue-bonusFood)
                        return Fail("No hay capacidad para recibir el bono");
                    state.Resources.Wood+=bonusWood;
                    state.Resources.Stone+=bonusStone;
                    state.Resources.Food+=bonusFood;
#endif
                    c.DevelopmentPriority=id;
                    state.MissionPower+=bonusPower;
                    break;
                case "InvestigateFissure":
                    if(id!="fissure-valoria"||c.FissureClosed||c.FissureExplored||c.InvestigationEndsUtcTicks>0)return Fail("Investigación no disponible");
                    if(!CanDepart())return Fail("Aldric debe regresar antes de investigar");
                    var investigationRoute=new System.Collections.Generic.List<WorldRoutePoint>();
                    if(marchRoutes!=null&&!marchRoutes.TryRoute(Snapshot(),id,out investigationRoute))return Fail("No hay ruta transitable hasta la fisura");
                    c.InvestigationRoute=investigationRoute??new System.Collections.Generic.List<WorldRoutePoint>();
                    c.InvestigationEndsUtcTicks=clock.UtcTicks+TimeSpan.FromMinutes(20).Ticks;break;
                case "ContactLyra":
                    if(id!="fissure-valoria"||!c.FissureExplored||c.FissureClosed||c.LyraContacted)return Fail("Encuentro no disponible");
                    c.LyraContacted=true;break;
                case "Build":case "UpgradeBuilding":
                    if(state.BuildingCompletesUtcTicks>0||state.BastionUpgradeCompletesUtcTicks>0)return Fail("Ya hay una obra en curso");
                    int level=id=="sawmill"?state.SawmillLevel:id=="barracks"?state.BarracksLevel:id=="granary"?c.GranaryLevel:id=="stoneworks"?c.StoneworksLevel:-1;
                    if(id=="stoneworks"&&state.BastionLevel<5)return Fail("Requiere Bastión V");
                    if(level<0||level>=state.BastionLevel||level>=5||(command.Kind=="Build"?level!=0:level==0))return Fail("Mejora no disponible");
                    if(level>=2&&id!="barracks"&&!c.Inventory.Contains(id=="sawmill"?"iron-brace":id=="stoneworks"?"stone-tusk":"treated-hide"))return Fail("Falta material especial de la frontera");
                    int wood=level==0?(id=="stoneworks"?500:240):(id=="barracks"?140:120)*level;
                    int stone=level==0?(id=="stoneworks"?0:180):(id=="barracks"?90:78)*level;
                    int food=level==0&&id=="stoneworks"?200:0;
                    if(state.Resources.Wood<wood||state.Resources.Stone<stone||state.Resources.Food<food)return Fail("Faltan recursos para la obra");
                    state.Resources.Food-=food;
                    state.Resources.Wood-=wood;state.Resources.Stone-=stone;c.PendingBuildingLevel=level+1;
                    state.BuildingTaskId="c3:"+id+":"+command.Id;
                    state.BuildingCompletesUtcTicks=clock.UtcTicks+TimeSpan.FromSeconds(level==0?(id=="stoneworks"?12:9):8+level*2).Ticks;break;
                case "Gather":case "Fight":
                    if(id=="fissure-valoria"&&(!c.LyraContacted||c.FissureClosed))return Fail("Investiga y conoce a Lyra antes del asalto");
                    if(id=="ash-stalker-valoria"&&(state.BastionLevel<5||c.AshStalkerRespawnUtcTicks>clock.UtcTicks))return Fail("Acechador no disponible");
                    if(id=="meat-valoria"&&(command.Kind!="Gather"||c.MeatRemaining<=0))return Fail("Reserva no disponible");
                    if(id!="meat-valoria"&&command.Kind!="Fight")return Fail("Acción no válida");
                    if(id=="hunt-wolf"&&c.WolfRespawnUtcTicks>clock.UtcTicks||id=="hunt-boar"&&c.BoarRespawnUtcTicks>clock.UtcTicks)return Fail("La caza aún no está disponible");
                    if(!CanDepart())return Fail("Marcha no disponible");
                    if(!Depart(id))return Fail("No hay ruta transitable");break;
            }
            GrantWebMissionRewards();state.CompletedCommandIds.Add(command.Id);Commit();
            return new CommandResult(true,"Acción iniciada",state.Revision);
        }
        void AccrueChapterThree(long until, long productionDeadlineUtcTicks = long.MaxValue)
        {
            var c=state.ChapterThree;
            if(c.EconomyUtcTicks<=0){c.EconomyUtcTicks=until;return;}
            if(until<=c.EconomyUtcTicks)return; // Clock rollback must not pay or rewind a settled ledger.
            if(c.EconomyVersion<2)
            {
                // Pay the remaining legacy entitlement once, with the legacy 1h limit.
                // A pending building completion is still split by AdvanceChapterThree.
                long seconds=Math.Min(3600,Math.Max(0,(until-c.EconomyUtcTicks)/TimeSpan.TicksPerSecond));
                state.Resources.Wood=checked(state.Resources.Wood+checked((int)seconds*state.SawmillLevel));
                state.Resources.Food=checked(state.Resources.Food+checked((int)seconds*c.GranaryLevel));
                state.Resources.Stone=checked(state.Resources.Stone+checked((int)seconds*2*c.StoneworksLevel));
                c.EconomyUtcTicks+=seconds*TimeSpan.TicksPerSecond;
                if(until-c.EconomyUtcTicks>=TimeSpan.TicksPerSecond)c.EconomyUtcTicks=until;
                return;
            }
            // Calculate every wallet balance before writing any ledger fields.
            // A failed checked overflow cannot partially credit resources.
            var wood=EconomyProductionV2.Accrue(c.EconomyUtcTicks,until,c.WoodProductionMicro,state.SawmillLevel,"wood",productionDeadlineUtcTicks:productionDeadlineUtcTicks);
            var stone=EconomyProductionV2.Accrue(c.EconomyUtcTicks,until,c.StoneProductionMicro,c.StoneworksLevel,"stone",productionDeadlineUtcTicks:productionDeadlineUtcTicks);
            var food=EconomyProductionV2.Accrue(c.EconomyUtcTicks,until,c.FoodProductionMicro,c.GranaryLevel,"food",productionDeadlineUtcTicks:productionDeadlineUtcTicks);
            int nextWood=checked(state.Resources.Wood+wood.WholeUnits);
            int nextStone=checked(state.Resources.Stone+stone.WholeUnits);
            int nextFood=checked(state.Resources.Food+food.WholeUnits);
            state.Resources.Wood=nextWood;state.Resources.Stone=nextStone;state.Resources.Food=nextFood;
            c.WoodProductionMicro=wood.FractionalMicrounits;
            c.StoneProductionMicro=stone.FractionalMicrounits;
            c.FoodProductionMicro=food.FractionalMicrounits;
            c.EconomyUtcTicks=wood.SettledThroughUtcTicks;
        }
        bool AdvanceFrontierRespawns()
        {
            if(state.BastionLevel<3)return false;
            var c=state.ChapterThree;bool changed=false;
            if(c.MeatRespawnUtcTicks>0&&clock.UtcTicks>=c.MeatRespawnUtcTicks){c.MeatRemaining=1100;c.MeatRespawnUtcTicks=0;c.MeatGeneration++;changed=true;}
            if(c.WolfRespawnUtcTicks>0&&clock.UtcTicks>=c.WolfRespawnUtcTicks){c.WolfRespawnUtcTicks=0;c.WolfGeneration++;changed=true;}
            if(c.BoarRespawnUtcTicks>0&&clock.UtcTicks>=c.BoarRespawnUtcTicks){c.BoarRespawnUtcTicks=0;c.BoarGeneration++;changed=true;}
            if(c.AshStalkerRespawnUtcTicks>0&&clock.UtcTicks>=c.AshStalkerRespawnUtcTicks){c.AshStalkerRespawnUtcTicks=0;c.AshStalkerGeneration++;changed=true;}
            return changed;
        }
        bool CompleteEarlyInfrastructure()
        {
            var c=state.ChapterThree;
            if(state.BastionLevel!=2||c==null||state.BuildingCompletesUtcTicks<=0||state.BuildingCompletesUtcTicks>clock.UtcTicks)return false;
            string task=state.BuildingTaskId??"";
            bool sawmill=task.StartsWith("c3:sawmill:",StringComparison.Ordinal);
            bool barracks=task.StartsWith("c3:barracks:",StringComparison.Ordinal);
            if(!sawmill&&!barracks)return false;
            if(!state.CompletedTaskIds.Contains(task))
            {
                state.CompletedTaskIds.Add(task);
                int oldLevel=c.PendingBuildingLevel-1;
                state.MissionPower+=(barracks?110+oldLevel*30-190:90+oldLevel*25-170);
                if(sawmill)state.SawmillLevel=c.PendingBuildingLevel;
                else state.BarracksLevel=c.PendingBuildingLevel;
            }
            c.PendingBuildingLevel=0;state.BuildingCompletesUtcTicks=0;state.BuildingTaskId="";
            return true;
        }
        bool AdvanceChapterThree()
        {
            if(state.BastionLevel<3)return CompleteEarlyInfrastructure();
            var c=state.ChapterThree;bool changed=false;
            // One 48h entitlement for the entire offline period. A building upgrade
            // must not restart the clock and silently credit another 48 hours.
            long offlineDeadline = c.EconomyVersion >= 2 && c.EconomyUtcTicks > 0
                ? EconomyProductionV2.OfflineWindowDeadline(c.EconomyUtcTicks) : long.MaxValue;
            changed|=AdvanceFrontierRespawns();
            if(state.ScoutDefeated&&!c.Inventory.Contains("iron-brace")){c.Inventory.Add("iron-brace");changed=true;}
            if(c.EconomyVersion<2&&c.EconomyUtcTicks>0&&clock.UtcTicks-c.EconomyUtcTicks>TimeSpan.FromHours(1).Ticks){c.EconomyUtcTicks=clock.UtcTicks-TimeSpan.FromHours(1).Ticks;changed=true;}
            if(c.InvestigationEndsUtcTicks>0&&c.InvestigationEndsUtcTicks<=clock.UtcTicks){c.FissureExplored=true;c.InvestigationEndsUtcTicks=0;changed=true;}
            if(state.BuildingTaskId.StartsWith("c3:",StringComparison.Ordinal)&&state.BuildingCompletesUtcTicks<=clock.UtcTicks&&state.BuildingCompletesUtcTicks>0) {
                AccrueChapterThree(Math.Max(c.EconomyUtcTicks,state.BuildingCompletesUtcTicks),offlineDeadline);
                if(!state.CompletedTaskIds.Contains(state.BuildingTaskId)) {
                    state.CompletedTaskIds.Add(state.BuildingTaskId);string id=state.BuildingTaskId.Split(':')[1];
                    int oldLevel=c.PendingBuildingLevel-1;
                    int gain=oldLevel==0?(id=="stoneworks"?420:300):id=="barracks"?110+oldLevel*30:90+oldLevel*25;
                    state.MissionPower+=gain-(id=="sawmill"?170:id=="barracks"?190:0);
                    if(id=="sawmill")state.SawmillLevel=c.PendingBuildingLevel;
                    else if(id=="barracks")state.BarracksLevel=c.PendingBuildingLevel;
                    else if(id=="stoneworks"){c.StoneworksLevel=c.PendingBuildingLevel;c.RankUnlocked=true;}
                    else c.GranaryLevel=c.PendingBuildingLevel;
                }
                c.PendingBuildingLevel=0;state.BuildingCompletesUtcTicks=0;state.BuildingTaskId="";changed=true;
            }
            if(c.EconomyUtcTicks==0||clock.UtcTicks-c.EconomyUtcTicks>=TimeSpan.TicksPerSecond){AccrueChapterThree(clock.UtcTicks,offlineDeadline);changed=true;}
            if(c.EconomyVersion<2)
            {
                // Legacy payout has completed under its original rules. Never replay it.
                c.EconomyVersion=2;
                c.EconomyUtcTicks=Math.Max(c.EconomyUtcTicks,clock.UtcTicks);
                c.WoodProductionMicro=0;c.StoneProductionMicro=0;c.FoodProductionMicro=0;
                changed=true;
            }
            return changed;
        }
        void RecordChapterThreeCombatMail(MarchState m,long due)
        {
            var report=state.LastBattleReport;
            if(report==null || state.Mailbox==null)return;
            string id="battle:"+m.MarchId+":"+m.TargetId+":"+due;
            state.Mailbox.Receive(new EldoriaV41Message {
                EventId=id,Category="informes",Type="battle",
                Title=report.VersionedOutcome?.Outcome=="draw"?"Empate en batalla":report.Won?"Victoria en batalla":"Derrota en batalla",
                Subtitle=report.Reason,ReportId=id,OccurredAtUtcTicks=due,
                Battle=report.VersionedOutcome?.Copy()??new EldoriaV41CombatRecord {
                    TargetId=report.TargetId,HeroId=report.HeroId,Troops=report.Troops,
                    OwnRoster=report.OwnRoster?.Copy(),PlayerPower=report.PlayerPower,EnemyPower=report.EnemyPower,Won=report.Won,
                    Rounds=report.Rounds,RemainingHealth=report.RemainingHealth,
                    RewardWood=report.RewardWood,RewardStone=report.RewardStone,
                    RewardFood=report.RewardFood,Reason=report.Reason
                }
            });
        }
        void ResolveChapterThreeMarch(MarchState m,long due)
        {
            var c=state.ChapterThree;
            if(m.TargetId=="ash-stalker-valoria")
            {
                var stats=SliceRules.Expedition(m.Troops,m.HeroId);
                var evolved=m.VersionedBattle==null?null:battleResolver.Resolve(m.VersionedBattle.Copy());
                var report=evolved==null?SliceRules.FightCreature(stats,new WorldCreatureStats(920,132,68,54,0,0),ambush:true):
                    new CombatReport(evolved.Won,evolved.RemainingHealth,evolved.Rounds,evolved.Reason);
                if(report.Won){c.AshStalkerDefeated=true;c.AshStalkerRespawnUtcTicks=checked(due+RenewablePopulationPolicy.Delay.Ticks);m.PendingFood=120;m.PendingStone=110;}
                if(evolved!=null){evolved.RewardFood=m.PendingFood;evolved.RewardStone=m.PendingStone;}
                state.LastBattleReport=new BattleReportState{
                    TargetId=m.TargetId,HeroId=m.HeroId,Troops=m.Troops.Total,
                    OwnRoster=m.Troops?.Copy(),VersionedOutcome=evolved?.Copy(),PlayerPower=stats.Power,EnemyPower=3600,Won=report.Won,
                    Rounds=report.Rounds,RemainingHealth=report.RemainingHealth,
                    RewardFood=m.PendingFood,RewardStone=m.PendingStone,
                    Reason=report.Reason+(report.Won?" Botín: 120 comida y 110 piedra al regresar.":"")
                };
                state.LastBattleReason=state.LastBattleReport.Reason;
                RecordChapterThreeCombatMail(m,due);
                return;
            }
            if(m.TargetId=="meat-valoria") {m.PendingFood=c.MeatRemaining;c.MeatRemaining=0;c.MeatRespawnUtcTicks=checked(due+RenewablePopulationPolicy.Delay.Ticks);return;}
            var power=SliceRules.Expedition(m.Troops,m.HeroId).Power;
            // The web's ordinary hunts resolve against their explicit expedition-power requirement.
            bool win=m.TargetId=="fissure-valoria"?c.LyraContacted&&!c.FissureClosed:power>=(m.TargetId=="hunt-wolf"?1450:2050);
            if(win&&m.TargetId=="fissure-valoria") {
                c.FissureClosed=true;c.LyraRecruited=true;c.LyraXp+=80;state.MissionPower+=430;c.ClosingEndsUtcTicks=due+TimeSpan.FromMilliseconds(6500).Ticks;
                if(!c.Inventory.Contains("breach-manuscript"))c.Inventory.Add("breach-manuscript");m.PendingStone=300;
                // Distinct one-time narrative welcome for a genuine recruit.
                // All combat loot and Lyra's acquisition stay in the existing
                // battle contract; the mailbox letter never pays again.
                state.Mailbox.Receive(new EldoriaV41Message {
                    EventId="milestone:hero:lyra",Category="narrativa",
                    Type="hero-unlocked",Title="Lyra se une a Valoria",
                    Subtitle="La exploradora ha llegado tras el cierre de la fisura. Puede participar en nuevas marchas.",
                    OccurredAtUtcTicks=due
                });
            } else if(win) {
                c.Hunts++;bool wolf=m.TargetId=="hunt-wolf";m.PendingFood=wolf?140:190;
                if(wolf)c.WolfRespawnUtcTicks=checked(due+RenewablePopulationPolicy.Delay.Ticks);else c.BoarRespawnUtcTicks=checked(due+RenewablePopulationPolicy.Delay.Ticks);
                string item=wolf?"treated-hide":"stone-tusk";if(!c.Inventory.Contains(item))c.Inventory.Add(item);
            }
            state.LastBattleReport=new BattleReportState{TargetId=m.TargetId,HeroId=m.HeroId,Troops=m.Troops.Total,
                OwnRoster=m.Troops?.Copy(),PlayerPower=power,Won=win,RewardStone=m.PendingStone,
                Reason=m.TargetId=="fissure-valoria"?"Aldric y Lyra cierran la fisura. Lyra reconoce el símbolo de su hermano y se une a Valoria.":win?"La caza proporciona alimento y materiales, no Poder.":"La expedición no alcanza el Poder necesario para esta cacería."};
            state.LastBattleReason=state.LastBattleReport.Reason;
            RecordChapterThreeCombatMail(m,due);
        }
        // Narrative is acknowledged ONLY after the player finishes the last page.
        // No resources, tasks, timers or gameplay progression can be granted here.
        public bool AcknowledgeNarrative(string beat)
        {
            if (beat!="start" && beat!="world" && beat!="mark" &&
                beat!="march-preparation" && beat!="vigilants-introduction" && beat!="scouts-introduction" && beat!="barracks" && beat!="secured" && beat!="frontier-open" && beat!="fissure-closed" && beat!="consolidate-valoria" && beat!="productive-valoria" && beat!="extraction-oddity") return false;
            if (state.ChapterProgress == null) state.ChapterProgress = new ChapterProgressState();
            if (state.ChapterProgress.SeenNarrative == null)
                state.ChapterProgress.SeenNarrative = new System.Collections.Generic.List<string>();
            if (state.ChapterProgress.SeenNarrative.Contains(beat)) return false;
            state.ChapterProgress.SeenNarrative.Add(beat);
            Commit();
            return true;
        }
        private void Commit() { state.Revision++; store.Save(state); }
        private CommandResult Fail(string reason) => new CommandResult(false, reason, state.Revision);
    }
}

