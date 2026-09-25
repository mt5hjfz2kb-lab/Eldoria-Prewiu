using System;
using System.Collections.Generic;

namespace Eldoria.Domain
{
    [Serializable] public sealed class ResourceWallet { public int Wood; public int Stone; public int Food; }
    [Serializable] public sealed class ArmyRoster
    {
        public int ArcherT1;
        public int ArcherT2;
        public int ArcherT3;
        public int Total => ArcherT1 + ArcherT2 + ArcherT3;
        public ArmyRoster Copy() => new ArmyRoster { ArcherT1 = ArcherT1, ArcherT2 = ArcherT2, ArcherT3 = ArcherT3 };
    }
    [Serializable] public sealed class MarchState
    {
        public string MarchId = "march-main";
        public string OwnerId = "player-local";
        public string TargetId = "";
        public string HeroId = "aldric";
        public ArmyRoster Troops = new ArmyRoster();
        public string Phase = "idle"; // idle, outbound, gathering, returning
        public long PhaseEndsUtcTicks;
        public int PendingWood;
        public int PendingStone;
    }
    [Serializable] public sealed class PlayerState
    {
        public int SchemaVersion = 1;
        public int RulesVersion = 1;
        public long Revision;
        public string PlayerId = "player-local";
        public string RealmId = "valoria-local";
        public string WorldId = "world-local";
        public ResourceWallet Resources = new ResourceWallet { Wood = 30, Stone = 150, Food = 0 };
        public int BastionLevel = 1;
        public int SawmillLevel;
        public int BarracksLevel;
        public bool CorruptionDiscovered;
        public bool ScoutDefeated;
        public bool JourneyComplete;
        public bool EngendroDefeated;
        public ArmyRoster Available = new ArmyRoster { ArcherT1 = 36 };
        public ArmyRoster Wounded = new ArmyRoster();
        public MarchState March = new MarchState();
        public int ForestRemaining = 1250;
        public long BuildingCompletesUtcTicks;
        public string BuildingTaskId = "";
        public long RecruitmentCompletesUtcTicks;
        public string RecruitmentTaskId = "";
        public int PendingRecruitArchers;
        public string LastBattleReason = "";
        public List<string> CompletedCommandIds = new List<string>();
        public List<string> CompletedTaskIds = new List<string>();
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
    public static class SliceRules
    {
        public const int SawmillWoodCost = 80;
        public const int BarracksWoodCost = 140;
        public const int BarracksStoneCost = 90;
        public const int RecruitWoodCost = 50;
        public const int RecruitArchers = 12;
        public const int BarracksBuildSeconds = 8;
        public const int RecruitSeconds = 7;
        public const int ForestLoad = 360;
        public const int SawmillBuildSeconds = 6;
        public const int TravelSeconds = 2;
        public const int GatherSeconds = 5;
        public static PowerParts TotalPower(PlayerState s)
            => new PowerParts(600 * s.BastionLevel + 170 * s.SawmillLevel + 190 * s.BarracksLevel,
                18 * (s.Available.Total + s.Wounded.Total + (s.March.Phase == "idle" ? 0 : s.March.Troops.Total)), 1204, 0, 0);
        // All deployment, preview, combat and casualties use this same tiered snapshot.
        public static CombatStats Expedition(ArmyRoster troops, string heroId)
        {
            if (heroId != "aldric" || troops == null || troops.Total < 1) return new CombatStats(0, 0, 0, 0, 0);
            double a = 78, d = 112, hp = 420, brk = 34; int pow = 1204;
            int[] counts = { troops.ArcherT1, troops.ArcherT2, troops.ArcherT3 };
            int[] tiers = { 1, 4, 10 }; // T1/T2/T3 profiles inherited from hero-army.js
            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] <= 0) continue;
                int t = tiers[i]; double scale = Math.Sqrt(counts[i]);
                a += (18 + t * 2) * scale;
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
        private static int Hit(int attack, int defense, int brk)
            => Math.Max(12, (int)Math.Round(attack * (1 + Math.Min(.6, brk / 500.0)) - defense * .48,
                MidpointRounding.AwayFromZero));
    }
}
