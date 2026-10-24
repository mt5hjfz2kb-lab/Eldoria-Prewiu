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
    public interface IWorldMarchRoutes
    {
        bool TryRoute(PlayerState snapshot,string targetId,out System.Collections.Generic.List<WorldRoutePoint> route);
    }
    public sealed class LocalGateway : ICommandGateway
    {
        private readonly IClock clock;
        private readonly IStateStore store;
        private PlayerState state;
        private IWorldResourcePlacement resourcePlacement;
        private long resourceRespawnTicks;
        private IWorldMarchRoutes marchRoutes;
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
                // Keep the return leg anchored to its original destination.
                if (item.RespawnUtcTicks > clock.UtcTicks || state.March.TargetId == id) continue;
                if (!resourcePlacement.TryChoose(Snapshot(), id, item.SiteId, item.Generation + 1, out var next)) continue;
                if (next == null || next.TargetId != id || string.IsNullOrEmpty(next.SiteId) ||
                    next.SiteId == item.SiteId || float.IsNaN(next.X) || float.IsInfinity(next.X) ||
                    float.IsNaN(next.Y) || float.IsInfinity(next.Y) || float.IsNaN(next.Z) || float.IsInfinity(next.Z))
                    throw new InvalidOperationException("Invalid authoritative resource placement");
                item.SiteId = next.SiteId; item.X = next.X; item.Y = next.Y; item.Z = next.Z;
                item.Generation++; item.RespawnUtcTicks = 0; item.DepletedUtcTicks=0;
                SetResourceRemaining(id,IsWoodTarget(id)?1250:5200);
                changed = true;
            }
            return changed;
        }
        public LocalGateway(IClock clock, IStateStore store)
        {
            this.clock = clock; this.store = store;
            state = store.Load() ?? new PlayerState();
            if (state.SchemaVersion != 1) throw new InvalidOperationException("Unsupported Eldoria save schema");
            if (state.WorldResourceLocations == null)
                state.WorldResourceLocations = new System.Collections.Generic.List<WorldResourceLocation>();
            var resourceIds = new System.Collections.Generic.HashSet<string>();
            foreach (var item in state.WorldResourceLocations)
                if (item == null || string.IsNullOrEmpty(item.TargetId) || (item.TargetId != "forest-valoria" && item.TargetId != "quarry-valoria" && !(item.TargetId.StartsWith("region1:wood:",StringComparison.Ordinal)&&item.ResourceKind=="wood") && !(item.TargetId.StartsWith("region1:stone:",StringComparison.Ordinal)&&item.ResourceKind=="stone")) || !resourceIds.Add(item.TargetId))
                    throw new InvalidOperationException("Invalid or duplicate persisted resource identity");
            NormalizeMissionProgress();
            Advance();
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
                PlayerId=s.PlayerId, RealmId=s.RealmId, WorldId=s.WorldId,
                Resources=new ResourceWallet { Wood=s.Resources.Wood, Stone=s.Resources.Stone, Food=s.Resources.Food },
                BastionLevel=s.BastionLevel, SawmillLevel=s.SawmillLevel, BarracksLevel=s.BarracksLevel,
                MissionPower=s.MissionPower,
                CorruptionDiscovered=s.CorruptionDiscovered, ScoutDefeated=s.ScoutDefeated,
                JourneyComplete=s.JourneyComplete, EngendroDefeated=s.EngendroDefeated,
                Available=s.Available.Copy(), Wounded=s.Wounded.Copy(),
                MarchConfigured=s.MarchConfigured, PreparedHeroId=s.PreparedHeroId,
                PreparedTroops=s.PreparedTroops.Copy(),
                March=new MarchState { MarchId=s.March.MarchId, OwnerId=s.March.OwnerId,
                    TargetId=s.March.TargetId, HeroId=s.March.HeroId, Troops=s.March.Troops.Copy(),
                    Route=s.March.Route?.ConvertAll(x=>x.Copy())??new System.Collections.Generic.List<WorldRoutePoint>(),
                    Phase=s.March.Phase, PhaseEndsUtcTicks=s.March.PhaseEndsUtcTicks,
                    PendingWood=s.March.PendingWood, PendingStone=s.March.PendingStone },
                WorldResourceLocations=s.WorldResourceLocations.ConvertAll(x=>x.Copy()),
                ForestRemaining=s.ForestRemaining, QuarryRemaining=s.QuarryRemaining, BuildingCompletesUtcTicks=s.BuildingCompletesUtcTicks,
                BuildingTaskId=s.BuildingTaskId,
                BastionUpgradeStartedUtcTicks=s.BastionUpgradeStartedUtcTicks,
                BastionUpgradeCompletesUtcTicks=s.BastionUpgradeCompletesUtcTicks,
                BastionUpgradeTaskId=s.BastionUpgradeTaskId,
                RecruitmentCompletesUtcTicks=s.RecruitmentCompletesUtcTicks,
                RecruitmentTaskId=s.RecruitmentTaskId, PendingRecruitArchers=s.PendingRecruitArchers,
                LastBattleReason=s.LastBattleReason,
                LastBattleReport=new BattleReportState {
                    TargetId=s.LastBattleReport?.TargetId??"",
                    HeroId=s.LastBattleReport?.HeroId??"aldric",
                    Troops=s.LastBattleReport?.Troops??0,
                    PlayerPower=s.LastBattleReport?.PlayerPower??0,
                    EnemyPower=s.LastBattleReport?.EnemyPower??0,
                    Won=s.LastBattleReport?.Won??false,
                    Rounds=s.LastBattleReport?.Rounds??0,
                    RemainingHealth=s.LastBattleReport?.RemainingHealth??0,
                    RewardWood=s.LastBattleReport?.RewardWood??0,
                    RewardStone=s.LastBattleReport?.RewardStone??0,
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
            Advance();
            if (command == null || string.IsNullOrEmpty(command.Id)) return Fail("Comando sin ID");
            if (state.CompletedCommandIds.Contains(command.Id)) return new CommandResult(true, "Ya ejecutado", state.Revision);
            if (command.ActorId != state.PlayerId) return Fail("Propietario incorrecto");
            if (command.ExpectedRevision != state.Revision) return Fail("Estado desactualizado");
            switch (command.Kind)
            {
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
                    if (state.March.Phase != "idle") return Fail("Espera al regreso de la Marcha");
                    int marchArchers=state.Available.ArcherT1;
                    if(command.TargetId != "march-main")
                    {
                        if(!command.TargetId.StartsWith("march-main:",StringComparison.Ordinal) ||
                            !int.TryParse(command.TargetId.Substring("march-main:".Length),out marchArchers))
                            return Fail("Composición desconocida");
                    }
                    if(marchArchers < 1 || marchArchers > state.Available.ArcherT1)
                        return Fail("No hay suficientes arqueros disponibles");
                    state.PreparedHeroId = "aldric";
                    state.PreparedTroops = state.Available.Copy();
                    state.PreparedTroops.ArcherT1 = marchArchers;
                    // Only the expressly selected unlocked T1 unit is assigned
                    // in the new composition editor. Legacy full-stack marches
                    // retain their historical behavior and save compatibility.
                    if(command.TargetId != "march-main")
                    {
                        state.PreparedTroops.ArcherT2 = 0;
                        state.PreparedTroops.ArcherT3 = 0;
                    }
                    state.MarchConfigured = true;
                    if (state.ChapterProgress == null) state.ChapterProgress = new ChapterProgressState();
                    state.ChapterProgress.MarchConfirmed = true;
                    state.ChapterProgress.ConfirmedExpeditionPower =
                        SliceRules.Expedition(state.PreparedTroops,state.PreparedHeroId).Power;
                    break;
                case "Fight":
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
                    state.BastionUpgradeStartedUtcTicks = clock.UtcTicks;
                    state.BastionUpgradeCompletesUtcTicks = clock.UtcTicks +
                        TimeSpan.FromSeconds(SliceContentProfiles.WebContract.Bastion2UpgradeSeconds).Ticks;
                    state.BastionUpgradeTaskId = "bastion2:" + command.Id;
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
                case "Recruit":
                    if (state.BastionLevel < 2 || state.BarracksLevel < 1)
                        return Fail("Reclutamiento no disponible");
                    int recruitCount=SliceRules.RecruitArchers;
                    if(command.TargetId != "archer:t1")
                    {
                        if(!command.TargetId.StartsWith("archer:t1:",StringComparison.Ordinal) ||
                           !int.TryParse(command.TargetId.Substring("archer:t1:".Length),out recruitCount))
                            return Fail("Tipo de unidad desconocido");
                    }
                    if(recruitCount<1 || recruitCount>500) return Fail("Cantidad de reclutas no válida");
                    if (state.RecruitmentCompletesUtcTicks > 0) return Fail("Ya hay reclutas entrenando");
                    int recruitWood=SliceRules.RecruitWoodCostFor(recruitCount);
                    int recruitStone=SliceRules.RecruitStoneCostFor(recruitCount);
                    if (state.Resources.Wood < recruitWood || state.Resources.Stone < recruitStone)
                        return Fail("Faltan recursos para equipar reclutas");
                    state.Resources.Wood -= recruitWood;
                    state.Resources.Stone -= recruitStone;
                    state.PendingRecruitArchers = recruitCount;
                    state.RecruitmentTaskId = "recruit:" + command.Id;
                    state.RecruitmentCompletesUtcTicks = clock.UtcTicks +
                        TimeSpan.FromSeconds(SliceRules.RecruitSecondsFor(recruitCount)).Ticks;
                    break;
                default: return Fail("Acción desconocida");
            }
            GrantWebMissionRewards();
            state.CompletedCommandIds.Add(command.Id);
            Commit();
            return new CommandResult(true, "Acción iniciada", state.Revision);
        }
        private bool CanDepart() => state.March.Phase == "idle" && state.Available.ArcherT1 > 0;
        private bool Depart(string targetId)
        {
            var planned=new System.Collections.Generic.List<WorldRoutePoint>();
            if(marchRoutes!=null&&!marchRoutes.TryRoute(Snapshot(),targetId,out planned))return false;
            if(planned==null)return false;
            var m = state.March;
            m.Route=planned;
            bool usePrepared = targetId == "engendro-valoria" && state.MarchConfigured;
            m.Troops = usePrepared ? state.PreparedTroops.Copy() : state.Available.Copy();
            m.HeroId = usePrepared ? state.PreparedHeroId : "aldric";
            state.Available.ArcherT1 -= m.Troops.ArcherT1;
            state.Available.ArcherT2 -= m.Troops.ArcherT2;
            state.Available.ArcherT3 -= m.Troops.ArcherT3;
            m.TargetId = targetId; m.Phase = "outbound";
            m.PendingWood = m.PendingStone = 0;
            m.PhaseEndsUtcTicks = clock.UtcTicks + TimeSpan.FromSeconds(SliceRules.TravelSeconds).Ticks;
            return true;
        }
        public bool Advance()
        {
            bool changed = false;
            if (state.BastionUpgradeCompletesUtcTicks > 0 && state.BastionUpgradeCompletesUtcTicks <= clock.UtcTicks)
            {
                if (!state.CompletedTaskIds.Contains(state.BastionUpgradeTaskId))
                {
                    state.CompletedTaskIds.Add(state.BastionUpgradeTaskId);
                    state.BastionLevel = Math.Max(2, state.BastionLevel);
                    if (state.ChapterProgress == null) state.ChapterProgress = new ChapterProgressState();
                    state.ChapterProgress.BastionTwoReached = true;
                }
                state.BastionUpgradeStartedUtcTicks = 0;
                state.BastionUpgradeCompletesUtcTicks = 0;
                state.BastionUpgradeTaskId = "";
                changed = true;
            }
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
                    state.Available.ArcherT1 += state.PendingRecruitArchers;
                    if (state.ChapterProgress == null) state.ChapterProgress = new ChapterProgressState();
                    state.ChapterProgress.TrainedArchers += state.PendingRecruitArchers;
                }
                state.RecruitmentCompletesUtcTicks = 0;
                state.RecruitmentTaskId = "";
                state.PendingRecruitArchers = 0;
                changed = true;
            }
            // Drive multiple phases in one call after a long offline interval using the ORIGINAL due time.
            for (int guard=0; guard<3 && state.March.Phase != "idle" &&
                state.March.PhaseEndsUtcTicks <= clock.UtcTicks; guard++)
            {
                var m = state.March;
                long due = m.PhaseEndsUtcTicks;
                if (m.Phase == "outbound")
                {
                    if (IsWoodTarget(m.TargetId) || IsStoneTarget(m.TargetId))
                    { m.Phase = "gathering"; m.PhaseEndsUtcTicks = due + TimeSpan.FromSeconds(SliceRules.GatherSeconds).Ticks; }
                    else
                    {
                        var expedition = SliceRules.Expedition(m.Troops, m.HeroId);
                        var report = SliceRules.Fight(expedition, m.TargetId);
                        state.LastBattleReason = report.Reason;
                        int rewardWood = 0, rewardStone = 0;
                        if (report.Won)
                        {
                            if (m.TargetId == "engendro-valoria")
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
                            PlayerPower = expedition.Power,
                            EnemyPower = 0,
                            Won = report.Won,
                            Rounds = report.Rounds,
                            RemainingHealth = report.RemainingHealth,
                            RewardWood = rewardWood,
                            RewardStone = rewardStone,
                            Reason = report.Reason
                        };
                        m.Phase = "returning"; m.PhaseEndsUtcTicks = due + TimeSpan.FromSeconds(SliceRules.TravelSeconds).Ticks;
                    }
                }
                else if (m.Phase == "gathering")
                {
                    if(IsStoneTarget(m.TargetId))
                    {
                        m.PendingStone = Math.Min(SliceRules.QuarryLoad,ResourceRemaining(state,m.TargetId));
                        SetResourceRemaining(m.TargetId,ResourceRemaining(state,m.TargetId)-m.PendingStone);
                    }
                    else
                    {
                        m.PendingWood = Math.Min(SliceRules.ForestLoad,ResourceRemaining(state,m.TargetId));
                        SetResourceRemaining(m.TargetId,ResourceRemaining(state,m.TargetId)-m.PendingWood);
                    }
                    if(ResourceRemaining(state,m.TargetId)==0){
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
                        state.Resources.Wood += m.PendingWood;
                        state.Resources.Stone += m.PendingStone;
                        if (IsWoodTarget(m.TargetId) && m.PendingWood > 0)
                        {
                            if (state.ChapterProgress == null) state.ChapterProgress = new ChapterProgressState();
                            state.ChapterProgress.GatheredWood += m.PendingWood;
                        }
                        if (IsStoneTarget(m.TargetId) && m.PendingStone > 0)
                        {
                            if (state.ChapterProgress == null) state.ChapterProgress = new ChapterProgressState();
                            state.ChapterProgress.GatheredStone += m.PendingStone;
                        }
                        state.Available.ArcherT1 += m.Troops.ArcherT1;
                        state.Available.ArcherT2 += m.Troops.ArcherT2;
                        state.Available.ArcherT3 += m.Troops.ArcherT3;
                    }
                    m.Troops = new ArmyRoster(); m.PendingWood = m.PendingStone = 0;
                    m.TargetId = ""; m.Phase = "idle"; m.PhaseEndsUtcTicks = 0;
                }
                changed = true;
            }
            if (!state.JourneyComplete && state.March.Phase == "idle")
            {
                if (state.ChapterProgress == null) state.ChapterProgress = new ChapterProgressState();
                bool chapterOneReady =
                    state.SawmillLevel > 0 &&
                    state.ChapterProgress.GatheredWood >= SliceContentProfiles.Active.Chapter1GatherWood &&
                    state.ChapterProgress.GatheredStone >= SliceContentProfiles.Active.Chapter1GatherStone &&
                    state.ChapterProgress.RouteCleared;
                if (chapterOneReady) { state.JourneyComplete = true; changed = true; }
            }
            if (GrantWebMissionRewards()) changed = true;
            changed |= AdvanceResourcePopulation();
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
            void Grant(string id, bool complete, int wood=0, int stone=0, int power=0)
            {
                if (!complete || p.GrantedRewards.Contains(id)) return;
                p.GrantedRewards.Add(id);
                state.Resources.Wood += wood;
                state.Resources.Stone += stone;
                state.MissionPower += power;
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
            // The web's c2-bastion and chapter-2 rewards require Bastion III. This slice
            // intentionally ends at II, so neither completion nor payout is fabricated.
            return changed;
        }
        // Narrative is acknowledged ONLY after the player finishes the last page.
        // No resources, tasks, timers or gameplay progression can be granted here.
        public bool AcknowledgeNarrative(string beat)
        {
            if (beat!="start" && beat!="world" && beat!="mark" &&
                beat!="barracks" && beat!="secured") return false;
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
