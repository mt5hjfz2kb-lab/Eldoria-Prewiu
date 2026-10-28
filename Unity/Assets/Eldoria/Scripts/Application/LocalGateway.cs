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
        private readonly IClock clock;
        private readonly IStateStore store;
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
                // Keep the return leg anchored to its original destination.
                if (item.RespawnUtcTicks > clock.UtcTicks || MarchTargetActive(id)) continue;
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
                if (item == null || string.IsNullOrEmpty(item.TargetId) || (item.TargetId != "forest-valoria" && item.TargetId != "quarry-valoria" && !(item.TargetId.StartsWith("region1:wood:",StringComparison.Ordinal)&&item.ResourceKind=="wood") && !(item.TargetId.StartsWith("region1:stone:",StringComparison.Ordinal)&&item.ResourceKind=="stone")) || !resourceIds.Add(item.TargetId))
                    throw new InvalidOperationException("Invalid or duplicate persisted resource identity");
            if(state.March==null)state.March=new MarchState();
            if(state.ExtraMarches==null)state.ExtraMarches=new System.Collections.Generic.List<MarchState>();
            if(state.ExtraMarches.Count>MarchCapacityRules.MaxConcurrentMarches-1 || state.ExtraMarches.Exists(x=>x==null))
                throw new InvalidOperationException("Invalid persistent march slots");
            if(state.Retention==null)state.Retention=new EarlyRetentionState();
            if(state.Retention.Orders==null)state.Retention.Orders=new System.Collections.Generic.List<ScoutOrder>();
            if(state.ChapterThree==null)state.ChapterThree=new BastionThreeState();
            if(state.ChapterThree.Inventory==null)state.ChapterThree.Inventory=new System.Collections.Generic.List<string>();
            if(state.ChapterThree.FissureClosed){state.ChapterThree.LyraRecruited=true;state.ChapterThree.InvestigationEndsUtcTicks=0;}
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
                MissionPower=s.MissionPower, ChapterThree=s.ChapterThree.Copy(),
                Retention=s.Retention?.Copy()??new EarlyRetentionState(),
                CorruptionDiscovered=s.CorruptionDiscovered, ScoutDefeated=s.ScoutDefeated,
                JourneyComplete=s.JourneyComplete, EngendroDefeated=s.EngendroDefeated,
                Available=s.Available.Copy(), Wounded=s.Wounded.Copy(),
                MarchConfigured=s.MarchConfigured, PreparedHeroId=s.PreparedHeroId,
                PreparedTroops=s.PreparedTroops.Copy(),
                ExtraMarches=s.ExtraMarches.ConvertAll(x=>x.Copy()),
                March=new MarchState { MarchId=s.March.MarchId, OwnerId=s.March.OwnerId,
                    TargetId=s.March.TargetId, HeroId=s.March.HeroId, Troops=s.March.Troops.Copy(),
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
            var early=EarlyRetentionRules.Apply(state,command.Kind,command.TargetId,
                clock.UtcTicks,out string earlyMessage);
            if(early.HasValue)
            {
                if(!early.Value)return Fail(earlyMessage);
                state.CompletedCommandIds.Add(command.Id);
                Commit();
                return new CommandResult(true,earlyMessage,state.Revision);
            }
            var chapterThreeResult=ExecuteChapterThree(command);
            if(chapterThreeResult!=null)return chapterThreeResult;
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
                    if(!MarchCapacityRules.CanStart(state,clock.UtcTicks))return Fail("Ya hay tres marchas activas");
                    // Legacy command plus explicit three-tier composition, all checked
                    // atomically against troops still at home and the selected hero cap.
                    var parts=(command.TargetId??"").Split(':');
                    if(parts[0]!="march-main" || (parts.Length!=1&&parts.Length!=2&&parts.Length!=3&&parts.Length!=5))
                        return Fail("Composición desconocida");
                    string marchHero=parts.Length==3?parts[2]:parts.Length==5?parts[4]:"aldric";
                    if(marchHero!="aldric" && (state.BastionLevel<3 || !state.ChapterThree.LyraRecruited || (marchHero!="lyra" && marchHero!="aldric+lyra")))
                        return Fail("Héroe no disponible");
                    var nextTroops=state.Available.Copy();
                    if(parts.Length>1)
                    {
                        if(!int.TryParse(parts[1],out nextTroops.ArcherT1))return Fail("Composición desconocida");
                        nextTroops.ArcherT2=0;nextTroops.ArcherT3=0;
                    }
                    if(parts.Length==5 &&
                        (!int.TryParse(parts[2],out nextTroops.ArcherT2)||
                         !int.TryParse(parts[3],out nextTroops.ArcherT3)))
                        return Fail("Composición desconocida");
                    if(!MarchCapacityRules.Fits(nextTroops,state.Available,MarchCapacityRules.Capacity(state,marchHero)))
                        return Fail("La marcha supera la capacidad del héroe o no tiene tropas disponibles");
                    state.PreparedHeroId=marchHero;
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
                    state.BastionUpgradeStartedUtcTicks = clock.UtcTicks;
                    state.BastionUpgradeCompletesUtcTicks = clock.UtcTicks +
                        TimeSpan.FromSeconds(next.Level==3?14:SliceContentProfiles.WebContract.Bastion2UpgradeSeconds).Ticks;
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
            // A real ConfigureMarch can unlock the Scout Board. Materialize
            // its first three contracts in THIS SAME accepted revision: if
            // deferred until the next Execute()->Advance(), the first genuine
            // attack after preparing a march would reject a fresh command as
            // stale when its own preflight silently created scout orders.
            EarlyRetentionRules.Synchronize(state,clock.UtcTicks);
            state.CompletedCommandIds.Add(command.Id);
            Commit();
            return new CommandResult(true, "Acción iniciada", state.Revision);
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
            string hero=usePrepared?state.PreparedHeroId:"aldric";
            var troops=usePrepared?state.PreparedTroops.Copy():
                MarchCapacityRules.AutoSelect(state.Available,MarchCapacityRules.Capacity(state,hero));
            if(!MarchCapacityRules.Fits(troops,state.Available,MarchCapacityRules.Capacity(state,hero)))return false;
            var m=MarchCapacityRules.AvailableSlot(state);
            if(m==null)return false;
            m.Route=planned;
            m.Troops=troops;
            m.HeroId=hero;
            MarchCapacityRules.Reserve(state,troops);
            m.TargetId = targetId; m.Phase = "outbound";
            m.PendingFood = m.PendingWood = m.PendingStone = 0;
            m.PhaseEndsUtcTicks = clock.UtcTicks + TimeSpan.FromSeconds(SliceRules.TravelSeconds).Ticks;
            return true;
        }
        public bool Advance()
        {
            bool changed = AdvanceChapterThree();
            changed |= EarlyRetentionRules.Synchronize(state,clock.UtcTicks);
            if (state.BastionUpgradeCompletesUtcTicks > 0 && state.BastionUpgradeCompletesUtcTicks <= clock.UtcTicks)
            {
                if (!state.CompletedTaskIds.Contains(state.BastionUpgradeTaskId))
                {
                    state.CompletedTaskIds.Add(state.BastionUpgradeTaskId);
                    state.BastionLevel = Math.Max(state.BastionUpgradeTaskId.StartsWith("bastion3:",StringComparison.Ordinal)?3:2, state.BastionLevel);
                    if(state.BastionLevel==3){state.ChapterThree.EconomyUtcTicks=state.BastionUpgradeCompletesUtcTicks;state.MissionPower+=300;}
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
                        var report=creature!=null?SliceRules.FightCreature(expedition,enemy):SliceRules.Fight(expedition,m.TargetId);
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
                            PlayerPower = expedition.Power,
                            EnemyPower = creature!=null?enemy.Health+enemy.Attack+enemy.Defense+enemy.Break:0,
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
                        MarchCapacityRules.Return(state,m.Troops);
                    }
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
            void Grant(string id, bool complete, int wood=0, int stone=0, int power=0, int food=0)
            {
                if (!complete || p.GrantedRewards.Contains(id)) return;
                p.GrantedRewards.Add(id);
                state.Resources.Wood += wood;
                state.Resources.Stone += stone;
                state.Resources.Food += food;
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
            Grant("c2-bastion",state.BastionLevel>=3,food:180);
            Grant("chapter-2",state.BastionLevel>=3,wood:240,stone:200,food:220,power:180);
            if(state.BastionLevel>=3) {
                var c=state.ChapterThree;
                Grant("c3-granary",c.GranaryLevel>0,wood:160,stone:110);
                Grant("c3-food",state.Resources.Food>=500,wood:120);
                Grant("c3-hunt",c.Hunts>=2,food:220);
                Grant("c3-fissure",c.FissureClosed,stone:260,power:100);
                Grant("c3-infra",state.SawmillLevel>=2&&state.BarracksLevel>=2&&c.GranaryLevel>=2,wood:220,stone:180);
            }
            return changed;
        }
        public static bool IsChapterThreeTarget(string id)=>id=="fissure-valoria"||id=="hunt-wolf"||id=="hunt-boar"||id=="meat-valoria";
        CommandResult ExecuteChapterThree(GameCommand command)
        {
            var c=state.ChapterThree;var id=command.TargetId;
            bool handles=command.Kind=="InvestigateFissure"||command.Kind=="ContactLyra"||command.Kind=="UpgradeBuilding"||
                (command.Kind=="Build"&&id=="granary")||((command.Kind=="Fight"||command.Kind=="Gather")&&IsChapterThreeTarget(id));
            if(!handles)return null;
            if(state.BastionLevel<3)return Fail("Requiere Bastión III");
            switch(command.Kind) {
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
                    int level=id=="sawmill"?state.SawmillLevel:id=="barracks"?state.BarracksLevel:id=="granary"?c.GranaryLevel:-1;
                    if(level<0||level>=state.BastionLevel||level>=3||(command.Kind=="Build"?level!=0:level==0))return Fail("Mejora no disponible");
                    if(level>=2&&id!="barracks"&&!c.Inventory.Contains(id=="sawmill"?"iron-brace":"treated-hide"))return Fail("Falta material especial de la frontera");
                    int wood=level==0?240:(id=="barracks"?140:120)*level;
                    int stone=level==0?180:(id=="barracks"?90:78)*level;
                    if(state.Resources.Wood<wood||state.Resources.Stone<stone)return Fail("Faltan recursos para la obra");
                    state.Resources.Wood-=wood;state.Resources.Stone-=stone;c.PendingBuildingLevel=level+1;
                    state.BuildingTaskId="c3:"+id+":"+command.Id;
                    state.BuildingCompletesUtcTicks=clock.UtcTicks+TimeSpan.FromSeconds(level==0?9:8+level*2).Ticks;break;
                case "Gather":case "Fight":
                    if(id=="fissure-valoria"&&(!c.LyraContacted||c.FissureClosed))return Fail("Investiga y conoce a Lyra antes del asalto");
                    if(id=="meat-valoria"&&(command.Kind!="Gather"||c.MeatRemaining<=0))return Fail("Reserva no disponible");
                    if(id!="meat-valoria"&&command.Kind!="Fight")return Fail("Acción no válida");
                    if(id=="hunt-wolf"&&c.WolfRespawnUtcTicks>clock.UtcTicks||id=="hunt-boar"&&c.BoarRespawnUtcTicks>clock.UtcTicks)return Fail("La caza aún no está disponible");
                    if(!CanDepart())return Fail("Marcha no disponible");
                    if(!Depart(id))return Fail("No hay ruta transitable");break;
            }
            GrantWebMissionRewards();state.CompletedCommandIds.Add(command.Id);Commit();
            return new CommandResult(true,"Acción iniciada",state.Revision);
        }
        void AccrueChapterThree(long until)
        {
            var c=state.ChapterThree;
            if(c.EconomyUtcTicks<=0){c.EconomyUtcTicks=until;return;}
            long seconds=Math.Min(3600,Math.Max(0,(until-c.EconomyUtcTicks)/TimeSpan.TicksPerSecond));
            state.Resources.Wood+=checked((int)seconds*state.SawmillLevel);
            state.Resources.Food+=checked((int)seconds*c.GranaryLevel);
            c.EconomyUtcTicks+=seconds*TimeSpan.TicksPerSecond;
            if(until-c.EconomyUtcTicks>=TimeSpan.TicksPerSecond)c.EconomyUtcTicks=until;
        }
        bool AdvanceFrontierRespawns()
        {
            if(state.BastionLevel<3)return false;
            var c=state.ChapterThree;bool changed=false;
            if(c.MeatRespawnUtcTicks>0&&clock.UtcTicks>=c.MeatRespawnUtcTicks){c.MeatRemaining=1100;c.MeatRespawnUtcTicks=0;c.MeatGeneration++;changed=true;}
            if(c.WolfRespawnUtcTicks>0&&clock.UtcTicks>=c.WolfRespawnUtcTicks){c.WolfRespawnUtcTicks=0;c.WolfGeneration++;changed=true;}
            if(c.BoarRespawnUtcTicks>0&&clock.UtcTicks>=c.BoarRespawnUtcTicks){c.BoarRespawnUtcTicks=0;c.BoarGeneration++;changed=true;}
            return changed;
        }
        bool AdvanceChapterThree()
        {
            if(state.BastionLevel<3)return false;
            var c=state.ChapterThree;bool changed=false;
            changed|=AdvanceFrontierRespawns();
            if(state.ScoutDefeated&&!c.Inventory.Contains("iron-brace")){c.Inventory.Add("iron-brace");changed=true;}
            if(c.EconomyUtcTicks>0&&clock.UtcTicks-c.EconomyUtcTicks>TimeSpan.FromHours(1).Ticks){c.EconomyUtcTicks=clock.UtcTicks-TimeSpan.FromHours(1).Ticks;changed=true;}
            if(c.InvestigationEndsUtcTicks>0&&c.InvestigationEndsUtcTicks<=clock.UtcTicks){c.FissureExplored=true;c.InvestigationEndsUtcTicks=0;changed=true;}
            if(state.BuildingTaskId.StartsWith("c3:",StringComparison.Ordinal)&&state.BuildingCompletesUtcTicks<=clock.UtcTicks&&state.BuildingCompletesUtcTicks>0) {
                AccrueChapterThree(Math.Max(c.EconomyUtcTicks,state.BuildingCompletesUtcTicks));
                if(!state.CompletedTaskIds.Contains(state.BuildingTaskId)) {
                    state.CompletedTaskIds.Add(state.BuildingTaskId);string id=state.BuildingTaskId.Split(':')[1];
                    int oldLevel=c.PendingBuildingLevel-1;
                    int gain=oldLevel==0?300:id=="barracks"?110+oldLevel*30:90+oldLevel*25;
                    state.MissionPower+=gain-(id=="sawmill"?170:id=="barracks"?190:0);
                    if(id=="sawmill")state.SawmillLevel=c.PendingBuildingLevel;
                    else if(id=="barracks")state.BarracksLevel=c.PendingBuildingLevel;
                    else c.GranaryLevel=c.PendingBuildingLevel;
                }
                c.PendingBuildingLevel=0;state.BuildingCompletesUtcTicks=0;state.BuildingTaskId="";changed=true;
            }
            if(c.EconomyUtcTicks==0||clock.UtcTicks-c.EconomyUtcTicks>=TimeSpan.TicksPerSecond){AccrueChapterThree(clock.UtcTicks);changed=true;}
            return changed;
        }
        void ResolveChapterThreeMarch(MarchState m,long due)
        {
            var c=state.ChapterThree;
            if(m.TargetId=="meat-valoria") {m.PendingFood=c.MeatRemaining;c.MeatRemaining=0;c.MeatRespawnUtcTicks=due+TimeSpan.FromSeconds(45).Ticks;return;}
            var power=SliceRules.Expedition(m.Troops,m.HeroId).Power;
            // The web's ordinary hunts resolve against their explicit expedition-power requirement.
            bool win=m.TargetId=="fissure-valoria"?c.LyraContacted&&!c.FissureClosed:power>=(m.TargetId=="hunt-wolf"?1450:2050);
            if(win&&m.TargetId=="fissure-valoria") {
                c.FissureClosed=true;c.LyraRecruited=true;c.LyraXp+=80;state.MissionPower+=430;c.ClosingEndsUtcTicks=due+TimeSpan.FromMilliseconds(6500).Ticks;
                if(!c.Inventory.Contains("breach-manuscript"))c.Inventory.Add("breach-manuscript");m.PendingStone=300;
            } else if(win) {
                c.Hunts++;bool wolf=m.TargetId=="hunt-wolf";m.PendingFood=wolf?140:190;
                if(wolf)c.WolfRespawnUtcTicks=due+TimeSpan.FromSeconds(45).Ticks;else c.BoarRespawnUtcTicks=due+TimeSpan.FromSeconds(45).Ticks;
                string item=wolf?"treated-hide":"stone-tusk";if(!c.Inventory.Contains(item))c.Inventory.Add(item);
            }
            state.LastBattleReport=new BattleReportState{TargetId=m.TargetId,HeroId=m.HeroId,Troops=m.Troops.Total,PlayerPower=power,Won=win,RewardStone=m.PendingStone,
                Reason=m.TargetId=="fissure-valoria"?"Aldric y Lyra cierran la fisura. Lyra reconoce el símbolo de su hermano y se une a Valoria.":win?"La caza proporciona alimento y materiales, no Poder.":"La expedición no alcanza el Poder necesario para esta cacería."};
            state.LastBattleReason=state.LastBattleReport.Reason;
        }
        // Narrative is acknowledged ONLY after the player finishes the last page.
        // No resources, tasks, timers or gameplay progression can be granted here.
        public bool AcknowledgeNarrative(string beat)
        {
            if (beat!="start" && beat!="world" && beat!="mark" &&
                beat!="barracks" && beat!="secured" && beat!="frontier-open" && beat!="fissure-closed") return false;
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
