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

        public static ValoriaExternalAssetLibrary Load()
            => Resources.Load<ValoriaExternalAssetLibrary>("Valoria/ExternalAssetLibrary");
    }
}
