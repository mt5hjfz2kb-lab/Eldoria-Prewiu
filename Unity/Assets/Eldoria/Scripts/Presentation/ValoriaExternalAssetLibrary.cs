using UnityEngine;

namespace Eldoria.Presentation
{
    // Runtime bridge to licensed environment prefabs imported into the Unity project.
    // Keeping references in one Resources asset lets builds use the original prefabs
    // without editor-only AssetDatabase calls.
    public sealed class ValoriaExternalAssetLibrary : ScriptableObject
    {
        public GameObject StoneTower;
        public GameObject StoneWall;
        public GameObject StoneGate;
        public GameObject LumberMill;
        // Approved benchmark candidates. Keep source prefabs intact; presentation adapts
        // their scale, colliders and materials when it instantiates them.
        public GameObject SlavicShed;
        public GameObject SlavicHouse;
        public GameObject SlavicTree;
        public GameObject RuinedTower;
        public GameObject Firewood;
        public GameObject MasonryWall;
        public GameObject MasonryGate;
        public GameObject MasonryTower;
        public GameObject CobbleRoad;
        public GameObject SlavicTreeTall;
        public GameObject SlavicBush;
        // Coherent Slavic environment subset promoted after visual review.
        public GameObject SlavicCobbleRoad;
        public GameObject SlavicStoneFence;
        public GameObject SlavicRockGate;
        public GameObject SlavicBoulder;
        public GameObject SlavicFlatRock;
        public GameObject SlavicMudFlat;
        public GameObject SlavicMoss;
        public GameObject MegaTower;
        public GameObject MegaHalfGate;
        public GameObject MegaDestroyedTower;
        public GameObject MegaWallPassage;

        public static ValoriaExternalAssetLibrary Load()
            => Resources.Load<ValoriaExternalAssetLibrary>("Valoria/ExternalAssetLibrary");
    }
}
