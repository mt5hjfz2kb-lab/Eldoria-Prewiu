namespace Eldoria.Domain
{
    /// <summary>
    /// Explicitly separates fast technical-slice values from the validated web product contract.
    /// QA_FAST remains the active runtime profile until OWNER_I_II is implemented and human-paced.
    /// Do not silently promote QA values into production balance.
    /// </summary>
    public static class SliceContentProfiles
    {
        public const string ActiveRuntimeProfile = "QA_FAST";

        public static class QaFast
        {
            public const int InitialWood = 30;
            public const int InitialStone = 150;
            public const int InitialFood = 0;
            public const int InitialArcherT1 = 36;

            public const int SawmillWoodCost = 80;
            public const int BarracksWoodCost = 140;
            public const int BarracksStoneCost = 90;
            public const int RecruitWoodCost = 50;
            public const int RecruitArchers = 12;

            public const int SawmillBuildSeconds = 6;
            public const int BarracksBuildSeconds = 8;
            public const int RecruitSeconds = 7;
            public const int TravelSeconds = 2;
            public const int GatherSeconds = 5;
            public const int ForestLoad = 360;
            public const int QuarryLoad = 700;

            public const int EngendroRequiredArchers = 48;
            public const int Chapter1GatherWood = 360;
            public const int Chapter1GatherStone = 500; // one QA quarry trip clears the canonical early-stone lesson
            public const int Chapter2TrainArchers = 12;
            public const int Chapter2ExpeditionPower = 2500;
        }

        /// <summary>
        /// Product-contract values extracted from the canonical web vertical slice.
        /// These are reference requirements, not yet the active Unity runtime balance profile.
        /// Human pacing validation is required before OWNER_I_II becomes production/default.
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
