using System;
using System.Collections.Generic;

namespace Eldoria.Domain
{
    // Persisted with the existing PlayerState/FileStateStore, never in an art asset.
    [Serializable] public sealed class WorldResourceLocation
    {
        public string TargetId;
        public string SiteId;
        public string ResourceKind;
        public int Remaining;
        public int Capacity;
        public float X, Y, Z;
        public int Generation;
        public long RespawnUtcTicks;
        public long DepletedUtcTicks;
        public WorldResourceLocation Copy() => (WorldResourceLocation)MemberwiseClone();
    }
    // World population is saved by the original owner store, alongside resources.
    // Bastion I-II story bosses retain their independent progression ledger.
    [Serializable] public sealed class WorldCreatureLocation
    {
        public string TargetId, SiteId, CreatureKind;
        public int Level, Generation;
        public float X,Y,Z;
        public long DefeatedUtcTicks, RespawnUtcTicks;
        public bool Alive => Generation>0&&DefeatedUtcTicks==0;
        public WorldCreatureLocation Copy() => (WorldCreatureLocation)MemberwiseClone();
    }
    public readonly struct WorldCreatureStats
    {
        public readonly int Health,Attack,Defense,Break,RewardWood,RewardStone;
        public WorldCreatureStats(int hp,int att,int def,int brk,int wood,int stone)
        {Health=hp;Attack=att;Defense=def;Break=brk;RewardWood=wood;RewardStone=stone;}
    }
    public static class WorldCreatureCatalog
    {
        // One original 3D model per family, five DATA levels per family.
        // Engendro 3 exactly matches Bastion II (HP760/ATK84/DEF72/BRK32).
        // Starting values require playtest balance approval, not art regeneration.
        static readonly WorldCreatureStats[] Boar={
            new WorldCreatureStats(260,35,28,8,35,10),
            new WorldCreatureStats(340,42,34,11,42,18),
            new WorldCreatureStats(420,49,40,14,55,25),
            new WorldCreatureStats(510,58,45,18,66,35),
            new WorldCreatureStats(600,68,52,22,78,45)};
        static readonly WorldCreatureStats[] Spawnling={
            new WorldCreatureStats(510,65,50,23,80,65),
            new WorldCreatureStats(620,76,59,28,95,75),
            new WorldCreatureStats(760,84,72,32,120,100),
            new WorldCreatureStats(890,96,82,36,140,110),
            new WorldCreatureStats(1050,112,96,40,165,135)};
        public static WorldCreatureStats For(string kind,int level)
        {
            if(level<1||level>5)throw new ArgumentOutOfRangeException(nameof(level));
            if(kind=="boar")return Boar[level-1];
            if(kind=="spawnling")return Spawnling[level-1];
            throw new ArgumentException("Unknown Tripo creature family",nameof(kind));
        }
    }
    [Serializable] public sealed class ResourceWallet { public int Wood; public int Stone; public int Food; }
    [Serializable] public sealed class ArmyRoster
    {
        public int ArcherT1;
        public int ArcherT2;
        public int ArcherT3;
        public int Total => ArcherT1 + ArcherT2 + ArcherT3;
        public ArmyRoster Copy() => new ArmyRoster { ArcherT1 = ArcherT1, ArcherT2 = ArcherT2, ArcherT3 = ArcherT3 };
    }
    [Serializable] public sealed class WorldRoutePoint { public float X,Y,Z; public WorldRoutePoint Copy()=>(WorldRoutePoint)MemberwiseClone(); }
    [Serializable] public sealed class MarchState
    {
        public List<WorldRoutePoint> Route = new List<WorldRoutePoint>();
        public string MarchId = "march-main";
        public string OwnerId = "player-local";
        public string TargetId = "";
        public string HeroId = "aldric";
        public ArmyRoster Troops = new ArmyRoster();
        public string Phase = "idle"; // idle, outbound, gathering, returning
        public long PhaseEndsUtcTicks;
        public int PendingFood;
        public int PendingWood;
        public int PendingStone;
        public MarchState Copy()
        {
            var copy=(MarchState)MemberwiseClone();
            copy.Troops=Troops?.Copy()??new ArmyRoster();
            copy.Route=Route?.ConvertAll(x=>x.Copy())??new List<WorldRoutePoint>();
            return copy;
        }
    }
    [Serializable] public sealed class BattleReportState
    {
        public string TargetId = "";
        public string HeroId = "aldric";
        public int Troops;
        public int PlayerPower;
        public int EnemyPower;
        public bool Won;
        public int Rounds;
        public int RemainingHealth;
        public int RewardWood;
        public int RewardStone;
        public int RewardFood;
        public string Reason = "";
    }
    [Serializable] public sealed class ChapterProgressState
    {
        public int GatheredWood;
        public int GatheredStone;
        public int TrainedArchers;
        public int ConfirmedExpeditionPower;
        public bool RouteCleared;
        public bool BastionTwoReached;
        public bool MarchConfirmed;
        public bool EngendroDefeated;
        public List<string> GrantedRewards = new List<string>();
        // Backward-compatible narrative ledger; rewards and chapter objectives remain separate.
        public List<string> SeenNarrative = new List<string>();
    }
    // Additive save data: existing code16 saves remain schema 1 and start with empty III progress.
    [Serializable] public sealed class BastionThreeState
    {
        public int GranaryLevel, Hunts, LyraXp;
        // Additive IV/V save fields: old schema-1 saves remain valid.
        public int StoneworksLevel;
        public string DevelopmentPriority = "";
        public bool AshStalkerDefeated;
        public long AshStalkerRespawnUtcTicks;
        public bool RankUnlocked, ExtractionOddity;
        public int MeatRemaining = 1100;
        public bool FissureExplored, LyraContacted, LyraRecruited, FissureClosed;
        public long InvestigationEndsUtcTicks, ClosingEndsUtcTicks, EconomyUtcTicks;
        public long WolfRespawnUtcTicks, BoarRespawnUtcTicks, MeatRespawnUtcTicks;
        public int WolfGeneration, BoarGeneration, MeatGeneration;
        public int PendingBuildingLevel;
        public List<string> Inventory = new List<string>();
        public List<WorldRoutePoint> InvestigationRoute = new List<WorldRoutePoint>();
        public BastionThreeState Copy() {
            var c=(BastionThreeState)MemberwiseClone();
            c.Inventory=new List<string>(Inventory??new List<string>());
            c.InvestigationRoute=new List<WorldRoutePoint>();foreach(var p in InvestigationRoute??new List<WorldRoutePoint>())c.InvestigationRoute.Add(new WorldRoutePoint{X=p.X,Y=p.Y,Z=p.Z});return c;
        }
    }
    [Serializable] public sealed class PlayerState
    {
        public int SchemaVersion = 1;
        public int RulesVersion = 1;
        public long Revision;
        public string PlayerId = "player-local";
        public string RealmId = "valoria-local";
        public string WorldId = "world-local";
        public ResourceWallet Resources = new ResourceWallet {
            Wood = SliceContentProfiles.Active.InitialWood,
            Stone = SliceContentProfiles.Active.InitialStone,
            Food = SliceContentProfiles.Active.InitialFood
        };
        public BastionThreeState ChapterThree = new BastionThreeState();
        // Additive optional progress: old schema-one saves initialize on load.
        public EarlyRetentionState Retention = new EarlyRetentionState();
        public int BastionLevel = 1;
        public int MissionPower;
        public int SawmillLevel;
        public int BarracksLevel;
        public bool CorruptionDiscovered;
        public bool ScoutDefeated;
        public bool JourneyComplete;
        public bool EngendroDefeated;
        public ArmyRoster Available = new ArmyRoster { ArcherT1 = SliceContentProfiles.Active.InitialArcherT1 };
        public ArmyRoster Wounded = new ArmyRoster();
        public MarchState March = new MarchState();
        // Additive schema-one compatible: two extra independent march slots.
        public List<MarchState> ExtraMarches = new List<MarchState>();
        public bool MarchConfigured;
        public string PreparedHeroId = "aldric";
        public ArmyRoster PreparedTroops = new ArmyRoster();
        public List<WorldResourceLocation> WorldResourceLocations = new List<WorldResourceLocation>();
        public List<WorldCreatureLocation> WorldCreatureLocations = new List<WorldCreatureLocation>();
        public int ForestRemaining = 1250;
        public int QuarryRemaining = 5200;
        public long BuildingCompletesUtcTicks;
        public string BuildingTaskId = "";
        public long BastionUpgradeStartedUtcTicks;
        public long BastionUpgradeCompletesUtcTicks;
        public string BastionUpgradeTaskId = "";
        public long RecruitmentCompletesUtcTicks;
        public string RecruitmentTaskId = "";
        public int PendingRecruitArchers;
        // Additive on the existing schema-1 save. Older in-flight training defaults to T1.
        public int PendingRecruitTier = 1;
        // Web chapter rewards: persistent one- and five-minute accelerators.
        public int SpeedupOneMinute, SpeedupFiveMinutes;
        public string LastBattleReason = "";
        public BattleReportState LastBattleReport = new BattleReportState();
        public ChapterProgressState ChapterProgress = new ChapterProgressState();
        public List<string> CompletedCommandIds = new List<string>();
        public List<string> CompletedTaskIds = new List<string>();
    }
    // Shared authoritative limit for all troop deployments, not only UI.
    // Existing legacy marches continue to return even when above a new limit.
    public static class MarchCapacityRules
    {
        public const int MaxConcurrentMarches = 3;
        public const int BaseCapacity = 40;
        public const int PerBarracksLevel = 20;
        public const int PerHeroLevel = 5;
        public const int HeroXpPerLevel = 80;
        public const int MaxHeroLevel = 20;

        public static int HeroLevel(PlayerState s, string heroId)
        {
            if(s==null)return 1;
            int aldric=Math.Max(0,s.Retention?.AldricExperience??0);
            int lyra=Math.Max(Math.Max(0,s.Retention?.LyraExperience??0),Math.Max(0,s.ChapterThree?.LyraXp??0));
            int xp=heroId=="lyra"?lyra:heroId=="aldric+lyra"?Math.Max(aldric,lyra):aldric;
            return Math.Min(MaxHeroLevel,1+xp/HeroXpPerLevel);
        }
        public static int Capacity(PlayerState s, string heroId)
        {
            if(s==null)return 0;
            return checked(BaseCapacity+Math.Max(0,s.BarracksLevel)*PerBarracksLevel+
                (HeroLevel(s,heroId)-1)*PerHeroLevel);
        }
        public static System.Collections.Generic.IEnumerable<MarchState> WorldMarches(PlayerState s)
        {
            if(s==null)yield break;
            if(s.March!=null)yield return s.March;
            if(s.ExtraMarches!=null)foreach(var m in s.ExtraMarches)if(m!=null)yield return m;
        }
        public static int ActiveWorldCount(PlayerState s)
        {
            int count=0;
            foreach(var m in WorldMarches(s))if(m.Phase!="idle")count++;
            return count;
        }
        public static int ActiveCount(PlayerState s,long utcTicks)
        {
            if(s==null)return 0;
            int count=ActiveWorldCount(s);
            if(s.ChapterThree!=null&&s.ChapterThree.InvestigationEndsUtcTicks>utcTicks)count++;
            if(s.Retention?.Orders!=null)
                foreach(var order in s.Retention.Orders)
                    if(order!=null&&!order.Claimed&&order.StartedUtcTicks>0&&order.EndsUtcTicks>utcTicks)count++;
            return count;
        }
        public static bool CanStart(PlayerState s,long utcTicks)
            => s!=null && ActiveCount(s,utcTicks)<MaxConcurrentMarches;
        public static bool Fits(ArmyRoster troops,ArmyRoster available,int capacity)
            => troops!=null&&available!=null&&troops.Total>0&&troops.Total<=capacity&&
               troops.ArcherT1>=0&&troops.ArcherT2>=0&&troops.ArcherT3>=0&&
               troops.ArcherT1<=available.ArcherT1&&troops.ArcherT2<=available.ArcherT2&&
               troops.ArcherT3<=available.ArcherT3;
        public static ArmyRoster AutoSelect(ArmyRoster available,int capacity)
        {
            var result=new ArmyRoster();
            if(available==null||capacity<=0)return result;
            int remaining=capacity;
            result.ArcherT3=Math.Min(Math.Max(0,available.ArcherT3),remaining);
            remaining-=result.ArcherT3;
            result.ArcherT2=Math.Min(Math.Max(0,available.ArcherT2),remaining);
            remaining-=result.ArcherT2;
            result.ArcherT1=Math.Min(Math.Max(0,available.ArcherT1),remaining);
            return result;
        }
        public static void Reserve(PlayerState s,ArmyRoster troops)
        {
            s.Available.ArcherT1-=troops.ArcherT1;
            s.Available.ArcherT2-=troops.ArcherT2;
            s.Available.ArcherT3-=troops.ArcherT3;
        }
        public static void Return(PlayerState s,ArmyRoster troops)
        {
            s.Available.ArcherT1=checked(s.Available.ArcherT1+troops.ArcherT1);
            s.Available.ArcherT2=checked(s.Available.ArcherT2+troops.ArcherT2);
            s.Available.ArcherT3=checked(s.Available.ArcherT3+troops.ArcherT3);
        }
        public static MarchState AvailableSlot(PlayerState s)
        {
            if(s.March==null)s.March=new MarchState();
            if(s.March.Phase=="idle")return s.March;
            if(s.ExtraMarches==null)s.ExtraMarches=new List<MarchState>();
            foreach(var m in s.ExtraMarches)if(m!=null&&m.Phase=="idle")return m;
            if(s.ExtraMarches.Count>=MaxConcurrentMarches-1)return null;
            var next=new MarchState{MarchId="march-"+(s.ExtraMarches.Count+2)};
            s.ExtraMarches.Add(next);
            return next;
        }
    }

    public readonly struct PowerParts
    {
        public readonly int Kingdom, Army, Heroes, Collection, Equipment;
        public int Total => Kingdom + Army + Heroes + Collection + Equipment;
        public PowerParts(int kingdom, int army, int heroes, int collection, int equipment)
        { Kingdom = kingdom; Army = army; Heroes = heroes; Collection = collection; Equipment = equipment; }
    }
    public readonly struct CombatStats
    {
        public readonly int Attack, Defense, Health, Break, Power;
        public CombatStats(int attack, int defense, int health, int brk, int power)
        { Attack = attack; Defense = defense; Health = health; Break = brk; Power = power; }
    }
    public readonly struct CombatReport
    {
        public readonly bool Won;
        public readonly int RemainingHealth;
        public readonly int Rounds;
        public readonly string Reason;
        public CombatReport(bool won, int remainingHealth, int rounds, string reason)
        { Won = won; RemainingHealth = remainingHealth; Rounds = rounds; Reason = reason; }
    }
    public sealed class BastionProgressionDefinition
    {
        public readonly int Level, RequiredPreviousLevel, WoodCost, StoneCost, PowerReward, FoodCost, DurationSeconds;
        public readonly bool RequiresJourneyComplete;
        public readonly string[] Unlocks;
        public BastionProgressionDefinition(int level,int requiredPreviousLevel,int woodCost,int stoneCost,
            int powerReward,bool requiresJourneyComplete,string[] unlocks,int foodCost=0,int durationSeconds=10)
        {
            Level=level;RequiredPreviousLevel=requiredPreviousLevel;WoodCost=woodCost;StoneCost=stoneCost;
            FoodCost=foodCost;DurationSeconds=durationSeconds;
            PowerReward=powerReward;RequiresJourneyComplete=requiresJourneyComplete;Unlocks=unlocks??Array.Empty<string>();
        }
    }

    /// <summary>
    /// Data contract for Bastion growth. Future Bastion levels extend this catalog instead of adding
    /// one-off gateway/UI branches. Costs remain sourced from the active content profile.
    /// </summary>
    public static class BastionProgressionCatalog
    {
        public static BastionProgressionDefinition ForLevel(int level)
        {
            if(level==2) return new BastionProgressionDefinition(
                2,1,SliceContentProfiles.Active.Bastion2WoodCost,SliceContentProfiles.Active.Bastion2StoneCost,
                600,true,new[]{"barracks","chapter-ii","world-engendro","march-preparation"});
            if(level==3) return new BastionProgressionDefinition(
                3,2,650,500,900,false,new[]{"granary","food","hunting","fissure","chapter-iii"},0,14);
            if(level==4) return new BastionProgressionDefinition(4,3,850,700,1150,false,new[]{"development-priority","chapter-iv"},300,16);
            if(level==5) return new BastionProgressionDefinition(5,4,1100,900,1500,false,new[]{"stoneworks","chapter-v"},500,18);
            throw new ArgumentOutOfRangeException(nameof(level),"Bastion level is not authored in the current progression catalog.");
        }

        public static bool RequirementsMet(PlayerState state,BastionProgressionDefinition definition)
        {
            if(state==null||definition==null)return false;
            if(state.BastionLevel!=definition.RequiredPreviousLevel)return false;
            if(state.BastionUpgradeCompletesUtcTicks>0||state.BuildingCompletesUtcTicks>0)return false;
            if(definition.RequiresJourneyComplete&&!state.JourneyComplete)return false;
            if(definition.Level==3&&(state.BarracksLevel<1||state.Available.Total+state.March.Troops.Total<41))return false;
            if(definition.Level==4&&(!state.ChapterThree.FissureClosed||state.SawmillLevel<2||state.BarracksLevel<2||state.ChapterThree.GranaryLevel<2))return false;
            if(definition.Level==5&&string.IsNullOrEmpty(state.ChapterThree.DevelopmentPriority))return false;
            return state.Resources.Wood>=definition.WoodCost&&state.Resources.Stone>=definition.StoneCost&&state.Resources.Food>=definition.FoodCost;
        }

        public static string MissingSummary(PlayerState state,BastionProgressionDefinition definition)
        {
            if(state==null||definition==null)return "";
            var missing=new List<string>();
            if(state.BastionUpgradeCompletesUtcTicks>0)missing.Add("mejora del Bastión en curso");
            else if(state.BuildingCompletesUtcTicks>0)missing.Add("espera a que termine la obra actual");
            if(definition.RequiresJourneyComplete&&!state.JourneyComplete)missing.Add("completa los objetivos de Bastión I");
            if(definition.Level==3&&state.BarracksLevel<1)missing.Add("construye el Cuartel");
            if(definition.Level==3&&state.Available.Total+state.March.Troops.Total<41)missing.Add("reúne 41 tropas");
            if(definition.Level==4){
                if(!state.ChapterThree.FissureClosed)missing.Add("cierra la fisura");
                if(state.SawmillLevel<2||state.BarracksLevel<2||state.ChapterThree.GranaryLevel<2)missing.Add("aserradero, cuartel y granero a nivel 2");
            }
            if(definition.Level==5&&string.IsNullOrEmpty(state.ChapterThree.DevelopmentPriority))missing.Add("elige la prioridad de desarrollo");
            if(state.Resources.Food<definition.FoodCost)missing.Add("comida "+state.Resources.Food+"/"+definition.FoodCost);
            if(state.Resources.Wood<definition.WoodCost)missing.Add("madera "+state.Resources.Wood+"/"+definition.WoodCost);
            if(state.Resources.Stone<definition.StoneCost)missing.Add("piedra "+state.Resources.Stone+"/"+definition.StoneCost);
            return string.Join(" · ",missing);
        }
    }

    public static class SliceRules
    {
        public static int SawmillWoodCost => SliceContentProfiles.Active.SawmillWoodCost;
        public static int Bastion2WoodCost => SliceContentProfiles.Active.Bastion2WoodCost;
        public static int Bastion2StoneCost => SliceContentProfiles.Active.Bastion2StoneCost;
        public static int BarracksWoodCost => SliceContentProfiles.Active.BarracksWoodCost;
        public static int BarracksStoneCost => SliceContentProfiles.Active.BarracksStoneCost;
        public static int RecruitWoodCost => SliceContentProfiles.Active.RecruitWoodCost;
        public static int RecruitStoneCost => SliceContentProfiles.Active.RecruitStoneCost;
        public static int RecruitArchers => SliceContentProfiles.Active.RecruitArchers;
        public static int BarracksBuildSeconds => SliceContentProfiles.Active.BarracksBuildSeconds;
        // Legacy callers and native I-II probes must see the same duration as
        // the canonical Recruit command for its default batch size.
        public static int RecruitSeconds => SliceContentProfiles.ActiveRuntimeProfile ==
            SliceContentProfiles.OwnerIiiId ? RecruitSecondsFor(RecruitArchers) :
            SliceContentProfiles.Active.RecruitSeconds;
        // Production pacing: 10 T1 = 300 s, 20 = 600 s, 30 = 900 s.
        // QA_FAST preserves its accelerated per-batch profile for existing tests.
        public static int RecruitSecondsFor(int count)
        {
            if(count < 1 || count > 500) throw new ArgumentOutOfRangeException(nameof(count));
            if(SliceContentProfiles.ActiveRuntimeProfile == SliceContentProfiles.OwnerIiiId)
                return checked(count * 30);
            return (int)Math.Ceiling((double)SliceContentProfiles.Active.RecruitSeconds *
                count / SliceContentProfiles.Active.RecruitArchers);
        }
        public static int RecruitWoodCostFor(int count)
        {
            if(count < 1 || count > 500) throw new ArgumentOutOfRangeException(nameof(count));
            return (int)Math.Ceiling((double)RecruitWoodCost * count / RecruitArchers);
        }
        public static int RecruitStoneCostFor(int count)
        {
            if(count < 1 || count > 500) throw new ArgumentOutOfRangeException(nameof(count));
            return (int)Math.Ceiling((double)RecruitStoneCost * count / RecruitArchers);
        }
        public static int MaxAffordableArchers(PlayerState state)
        {
            if(state == null || state.Resources == null) return 0;
            int lo=0,hi=500;
            while(lo<hi)
            {
                int mid=(lo+hi+1)/2;
                if(RecruitWoodCostFor(mid)<=state.Resources.Wood &&
                   RecruitStoneCostFor(mid)<=state.Resources.Stone) lo=mid;
                else hi=mid-1;
            }
            return lo;
        }
        public static int ForestLoad => SliceContentProfiles.Active.ForestLoad;
        public static int QuarryLoad => SliceContentProfiles.Active.QuarryLoad;
        public static int SawmillBuildSeconds => SliceContentProfiles.Active.SawmillBuildSeconds;
        public static int TravelSeconds => SliceContentProfiles.Active.TravelSeconds;
        public static int GatherSeconds => SliceContentProfiles.Active.GatherSeconds;
        public static string CurrentObjectiveKey(PlayerState s)
        {
            if (s == null) return "invalid";
            var p = s.ChapterProgress ?? new ChapterProgressState();
            if (s.BastionLevel <= 1)
            {
                if (s.JourneyComplete) return "b1.ascend";
                if (s.SawmillLevel == 0) return "b1.build-sawmill";
                if (p.GatheredWood < SliceContentProfiles.Active.Chapter1GatherWood) return "b1.gather-wood";
                if (p.GatheredStone < SliceContentProfiles.Active.Chapter1GatherStone) return "b1.gather-stone";
                if (!p.RouteCleared) return "b1.clear-route";
                return "b1.return";
            }
            if(s.BastionLevel>=5){
                var c=s.ChapterThree??new BastionThreeState();
                if(c.StoneworksLevel==0)return "b5.stoneworks";
                if(!(s.ChapterProgress?.GrantedRewards?.Contains("c5-stone")??false))return "b5.stone";
                int archers=s.Available.Total+s.Wounded.Total;
                foreach(var m in MarchCapacityRules.WorldMarches(s))if(m!=null&&m.Phase!="idle")archers+=m.Troops.Total;
                if(!(s.ChapterProgress?.GrantedRewards?.Contains("c5-army")??false)&&archers<60)return "b5.army";
                return c.AshStalkerDefeated?"b5.complete":"b5.rare";
            }
            if(s.BastionLevel==4)return string.IsNullOrEmpty(s.ChapterThree.DevelopmentPriority)?"b4.priority":"b4.ascend";
            if(s.BastionLevel>=3) {
                var c=s.ChapterThree??new BastionThreeState();
                if(c.GranaryLevel==0)return "b3.granary";
                if(s.Resources.Food<500&&!p.GrantedRewards.Contains("c3-food"))return "b3.food";
                if(c.Hunts<2)return "b3.hunt";
                if(!c.FissureClosed)return !c.FissureExplored?"b3.investigate":!c.LyraContacted?"b3.lyra":"b3.fissure";
                if(s.SawmillLevel<2||s.BarracksLevel<2||c.GranaryLevel<2)return "b3.infrastructure";
                return "b3.complete";
            }
            if (s.EngendroDefeated || p.EngendroDefeated) return "b2.complete";
            if (s.BarracksLevel == 0) return "b2.build-barracks";
            if (p.TrainedArchers < SliceContentProfiles.Active.Chapter2TrainArchers) return "b2.train-archers";
            if (!p.MarchConfirmed || !s.MarchConfigured) return "b2.prepare-march";
            if (p.ConfirmedExpeditionPower < SliceContentProfiles.Active.Chapter2ExpeditionPower) return "b2.raise-expedition-power";
            return "b2.defeat-engendro";
        }

        public static PowerParts TotalPower(PlayerState s)
        {
            int deployed=0;
            foreach(var march in MarchCapacityRules.WorldMarches(s))
                if(march.Phase!="idle")deployed=checked(deployed+march.Troops.Total);
            if(s.Retention?.Orders!=null)
                foreach(var scout in s.Retention.Orders)
                    if(scout!=null&&!scout.TroopsReturned&&scout.StartedUtcTicks>0&&scout.Troops!=null)
                        deployed=checked(deployed+scout.Troops.Total);
            return new PowerParts(600*s.BastionLevel+170*s.SawmillLevel+
                190*s.BarracksLevel+s.MissionPower,
                18*checked(s.Available.Total+s.Wounded.Total+deployed),1204,0,0);
        }
        // All deployment, preview, combat and casualties use this same tiered snapshot.
        public static CombatStats Expedition(ArmyRoster troops, string heroId)
        {
            if ((heroId != "aldric" && heroId != "lyra" && heroId != "aldric+lyra") || troops == null || troops.Total < 1) return new CombatStats(0, 0, 0, 0, 0);
            bool lyra=heroId=="lyra"||heroId=="aldric+lyra", aldric=heroId!="lyra";
            double a = (aldric?78:0)+(lyra?116:0), d = (aldric?112:0)+(lyra?62:0), hp = (aldric?420:0)+(lyra?300:0), brk = (aldric?34:0)+(lyra?82:0); int pow = (aldric?1204:0)+(lyra?1278:0);
            int[] counts = { troops.ArcherT1, troops.ArcherT2, troops.ArcherT3 };
            int[] tiers = { 1, 4, 10 }; // T1/T2/T3 profiles inherited from hero-army.js
            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] <= 0) continue;
                int t = tiers[i]; double scale = Math.Sqrt(counts[i]);
                a += (18 + t * 2) * scale * (lyra?1.03:1);
                d += (11 + t) * scale;
                hp += (34 + t * 3) * scale;
                brk += (9 + t * 2) * scale;
                pow += (24 + t * 3) * counts[i];
            }
            return new CombatStats((int)Math.Round(a, MidpointRounding.AwayFromZero),
                (int)Math.Round(d, MidpointRounding.AwayFromZero),
                (int)Math.Round(hp, MidpointRounding.AwayFromZero),
                (int)Math.Round(brk, MidpointRounding.AwayFromZero), pow);
        }
        public static CombatReport Fight(CombatStats player) => Fight(player, "corrupt-scout");
        public static CombatReport Fight(CombatStats player, string enemyId)
        {
            int enemyHp, enemyDefense, enemyAttack, enemyBreak;
            if (enemyId == "engendro-valoria")
            {
                enemyHp = 760; enemyDefense = 72; enemyAttack = 84; enemyBreak = 32;
            }
            else
            {
                enemyHp = 620; enemyDefense = 64; enemyAttack = 76; enemyBreak = 28;
            }
            int playerHp = player.Health, rounds = 0;
            while (rounds < 6 && enemyHp > 0 && playerHp > 0)
            {
                rounds++;
                enemyHp -= Hit(player.Attack, enemyDefense, player.Break);
                if (enemyHp > 0) playerHp -= Hit(enemyAttack, player.Defense, enemyBreak);
            }
            bool win = enemyHp <= 0 && playerHp > 0;
            string reason = win
                ? (enemyId == "engendro-valoria"
                    ? "La nueva línea de arqueros sostuvo el frente y Aldric rompió la defensa del Engendro."
                    : "Tus arqueros y Aldric abrieron la defensa corrupta. Ataque y Ruptura superaron su resistencia.")
                : "El enemigo resistió seis intercambios o agotó la Vida de la marcha. Refuerza las tropas.";
            return new CombatReport(win, Math.Max(0, playerHp), rounds, reason);
        }
        // Use the same original six-round damage equation, without a second battle engine.
        public static CombatReport FightCreature(CombatStats player,WorldCreatureStats enemy,bool ambush=false)
        {
            int enemyHp=enemy.Health,playerHp=player.Health,rounds=0;
            int opening=ambush?Hit(enemy.Attack,player.Defense,enemy.Break,1.42):0;
            playerHp-=opening;
            int defense=ambush?(int)Math.Round(enemy.Defense*.82,MidpointRounding.AwayFromZero):enemy.Defense;
            while(rounds<6&&enemyHp>0&&playerHp>0)
            {
                rounds++;enemyHp-=Hit(player.Attack,defense,player.Break,ambush&&rounds==1?1.08:1);
                if(enemyHp>0)playerHp-=Hit(enemy.Attack,player.Defense,enemy.Break);
            }
            bool win=enemyHp<=0&&playerHp>0;
            return new CombatReport(win,Math.Max(0,playerHp),rounds,
                (win?"La expedición ha vencido al objetivo de Región 1.":
                    "La criatura resistió o agotó la Vida de la marcha. Mejora las tropas.")+
                (ambush?" Emboscada: "+opening+" daño inicial; después el Acechador pierde un 18 % de Defensa.":""));
        }
        private static int Hit(int attack, int defense, int brk,double multiplier=1)
            => Math.Max(12, (int)Math.Round((attack * (1 + Math.Min(.6, brk / 500.0)) - defense * .48)*multiplier,
                MidpointRounding.AwayFromZero));
    }
}
