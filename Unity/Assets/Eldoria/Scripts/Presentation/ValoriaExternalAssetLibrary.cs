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

        public static ValoriaExternalAssetLibrary Load()
            => Resources.Load<ValoriaExternalAssetLibrary>("Valoria/ExternalAssetLibrary");
    }
}
