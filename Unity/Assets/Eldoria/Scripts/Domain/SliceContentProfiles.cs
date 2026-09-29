namespace Eldoria.Domain
{
    /// <summary>
    /// Explicitly separates fast technical-slice values from the validated web product contract.
    /// QA_FAST remains the active runtime profile until OWNER_I_II is human-paced and explicitly promoted.
    /// Runtime code must consume Active so one profile is selected as a whole; never mix individual values.
    /// </summary>
    public static class SliceContentProfiles
    {
        public const string QaFastId = "QA_FAST";
        public const string OwnerIiiId = "OWNER_I_II";
        public const string ActiveRuntimeProfile = QaFastId;

        public static class QaFast
        {
            public const int InitialWood = 30;
            public const int InitialStone = 150;
            public const int InitialFood = 0;
            public const int InitialArcherT1 = 36;

            public const int SawmillWoodCost = 80;
            public const int Bastion2WoodCost = 0;
            public const int Bastion2StoneCost = 0;
            public const int BarracksWoodCost = 140;
            public const int BarracksStoneCost = 90;
            public const int RecruitWoodCost = 50;
            public const int RecruitStoneCost = 0;
            public const int RecruitArchers = 12;

            public const int SawmillBuildSeconds = 6;
            public const int BarracksBuildSeconds = 8;
            public const int RecruitSeconds = 7;
            public const int TravelSeconds = 2;
            public const int GatherSeconds = 5;
            public const int ForestLoad = 360;
            public const int QuarryLoad = 700;

            public const int Chapter1GatherWood = 360;
            public const int Chapter1GatherStone = 500;
            public const int Chapter2TrainArchers = 12;
            public const int Chapter2ExpeditionPower = 2500;
        }

        /// <summary>
        /// Candidate values for the first owner-facing Bastion I-II playtest.
        /// This profile is intentionally NOT active yet. Contract values come from the web.
        /// Travel/gather/recruit cadence remains provisional until the first human pacing pass,
        /// but is fully self-contained here so OWNER_I_II can never fall through to QA_FAST values.
        /// </summary>
        public static class OwnerIiiCandidate
        {
            public const int InitialWood = 230;
            public const int InitialStone = 150;
            public const int InitialFood = 0;
            public const int InitialArcherT1 = 36;

            public const int SawmillWoodCost = 80;
            public const int Bastion2WoodCost = 450;
            public const int Bastion2StoneCost = 300;
            public const int BarracksWoodCost = 180;
            public const int BarracksStoneCost = 120;
            public const int RecruitArchers = 20;
            public const int RecruitWoodCost = RecruitArchers * 20;
            public const int RecruitStoneCost = RecruitArchers * 12;

            public const int SawmillBuildSeconds = 6;
            public const int BarracksBuildSeconds = 8;
            public const int RecruitSeconds = 7; // candidate only; human pacing not frozen
            public const int TravelSeconds = 2;  // candidate only; human pacing not frozen
            public const int GatherSeconds = 5;  // candidate only; human pacing not frozen
            public const int ForestLoad = 360;   // candidate only; human pacing not frozen
            public const int QuarryLoad = 700;   // candidate only; human pacing not frozen

            public const int Chapter1GatherWood = 600;
            public const int Chapter1GatherStone = 500;
            public const int Chapter2TrainArchers = 20;
            public const int Chapter2ExpeditionPower = 2250;
        }

        /// <summary>
        /// Single runtime facade. Changing ActiveRuntimeProfile is the only supported profile switch.
        /// Save files are profile-scoped by SliceBoot so an OWNER_I_II test cannot reuse QA_FAST state.
        /// </summary>
        public static class Active
        {
            private static bool Owner => ActiveRuntimeProfile == OwnerIiiId;
            public static int InitialWood => Owner ? OwnerIiiCandidate.InitialWood : QaFast.InitialWood;
            public static int InitialStone => Owner ? OwnerIiiCandidate.InitialStone : QaFast.InitialStone;
            public static int InitialFood => Owner ? OwnerIiiCandidate.InitialFood : QaFast.InitialFood;
            public static int InitialArcherT1 => Owner ? OwnerIiiCandidate.InitialArcherT1 : QaFast.InitialArcherT1;
            public static int SawmillWoodCost => Owner ? OwnerIiiCandidate.SawmillWoodCost : QaFast.SawmillWoodCost;
            public static int Bastion2WoodCost => Owner ? OwnerIiiCandidate.Bastion2WoodCost : QaFast.Bastion2WoodCost;
            public static int Bastion2StoneCost => Owner ? OwnerIiiCandidate.Bastion2StoneCost : QaFast.Bastion2StoneCost;
            public static int BarracksWoodCost => Owner ? OwnerIiiCandidate.BarracksWoodCost : QaFast.BarracksWoodCost;
            public static int BarracksStoneCost => Owner ? OwnerIiiCandidate.BarracksStoneCost : QaFast.BarracksStoneCost;
            public static int RecruitWoodCost => Owner ? OwnerIiiCandidate.RecruitWoodCost : QaFast.RecruitWoodCost;
            public static int RecruitStoneCost => Owner ? OwnerIiiCandidate.RecruitStoneCost : QaFast.RecruitStoneCost;
            public static int RecruitArchers => Owner ? OwnerIiiCandidate.RecruitArchers : QaFast.RecruitArchers;
            public static int SawmillBuildSeconds => Owner ? OwnerIiiCandidate.SawmillBuildSeconds : QaFast.SawmillBuildSeconds;
            public static int BarracksBuildSeconds => Owner ? OwnerIiiCandidate.BarracksBuildSeconds : QaFast.BarracksBuildSeconds;
            public static int RecruitSeconds => Owner ? OwnerIiiCandidate.RecruitSeconds : QaFast.RecruitSeconds;
            public static int TravelSeconds => Owner ? OwnerIiiCandidate.TravelSeconds : QaFast.TravelSeconds;
            public static int GatherSeconds => Owner ? OwnerIiiCandidate.GatherSeconds : QaFast.GatherSeconds;
            public static int ForestLoad => Owner ? OwnerIiiCandidate.ForestLoad : QaFast.ForestLoad;
            public static int QuarryLoad => Owner ? OwnerIiiCandidate.QuarryLoad : QaFast.QuarryLoad;
            public static int Chapter1GatherWood => Owner ? OwnerIiiCandidate.Chapter1GatherWood : QaFast.Chapter1GatherWood;
            public static int Chapter1GatherStone => Owner ? OwnerIiiCandidate.Chapter1GatherStone : QaFast.Chapter1GatherStone;
            public static int Chapter2TrainArchers => Owner ? OwnerIiiCandidate.Chapter2TrainArchers : QaFast.Chapter2TrainArchers;
            public static int Chapter2ExpeditionPower => Owner ? OwnerIiiCandidate.Chapter2ExpeditionPower : QaFast.Chapter2ExpeditionPower;
        }

        /// <summary>
        /// Product-contract values extracted from the canonical web vertical slice.
        /// These are reference requirements, not yet the active Unity runtime balance profile.
        /// </summary>
        public static class WebContract
        {
            public const int InitialWood = 230;
            public const int InitialStone = 150;
            public const int InitialArcherT1 = 36;
            public const int Chapter1GatherWood = 600;
            public const int Chapter1GatherStone = 500;
            public const int Bastion2WoodCost = 450;
            public const int Bastion2StoneCost = 300;
            public const int SawmillWoodCost = 80;
            public const int SawmillBuildSeconds = 6;
            public const int BarracksWoodCost = 180;
            public const int BarracksStoneCost = 120;
            public const int BarracksBuildSeconds = 8;
            public const int Chapter2TrainArchers = 20;
            public const int Chapter2ExpeditionPower = 2250;
            public const int RecruitWoodPerArcher = 20;
            public const int RecruitStonePerArcher = 12;
        }
    }
}
