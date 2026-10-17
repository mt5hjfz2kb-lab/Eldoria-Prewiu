# Third-party 3D models for isolated Eldoria look-development

**Scope:** only branch `experiment/eldoria-three-visual-styles-20261017`, and only
`Unity/Assets/Eldoria/ArtTests/ThreeVisualStyles/Vendor/`. Not merged to
production. No external images or paid model generation.

## Quaternius — Ultimate Fantasy RTS, CC0

- Original author: Quaternius
- Official pack: https://quaternius.com/packs/ultimatefantasyrts.html
- Original download: https://drive.google.com/drive/folders/1h7sztlZyavWla-JDk3jp6KiWDdMh08yd
- Redistribution source repository (with author's source notice):
  https://github.com/ranzevor/Latticefolk/tree/main/public/assets/quaternius/ultimate-fantasy-rts
- Source model format: self-contained glTF files (.gltf, embedded binary data)
- License: Creative Commons Zero 1.0 Universal, CC0
- The source Git blob hashes were confirmed when copying into this independent
  Eldoria experimental Git tree.

Models copied verbatim (11):
`Barracks_FirstAge_Level3.gltf`, `Farm_SecondAge_Level3.gltf`,
`Houses_SecondAge_1_Level3.gltf`, `Houses_SecondAge_2_Level3.gltf`,
`Market_FirstAge_Level3.gltf`, `Mine.gltf`,
`Storage_FirstAge_Leve3.gltf`, `TownCenter_FirstAge_Level3.gltf`,
`Windmill_FirstAge.gltf`, `Crate.gltf`, `Barrel.gltf`.

## Kenney — Castle Kit and Fantasy Town Kit, CC0

- Original creator: Kenney (https://kenney.nl/)
- Official pack pages: https://kenney.nl/assets/castle-kit and
  https://kenney.nl/assets/fantasy-town-kit
- Redistribution source: https://github.com/shorepine/kenney/tree/main/3d
- Official CC0 status also appears on both official pack pages. The public source
  repository includes Kenney's CC0 `LICENSE.txt`; the retrieved license explicitly
  permits commercial use and does not require attribution.
- Source model format: single-file glTF binary (.glb), already optimized small
  meshes with material data. Imported byte-for-byte (SHA-1 Git blob checked).

Castle Kit (12):
`gate`, `tower-square-base`, `tower-square-mid`,
`tower-square-top-roof`, `wall`, `wall-corner`,
`bridge-straight`, `flag-banner-long`, `stairs-stone`,
`tree-small`, `rocks-large`, `tower-square-top`.

Fantasy Town Kit (9):
`fountain-square`, `cart`, `roof-gable`,
`roof-high-window`, `road`, `lantern`,
`planks`, `fence`, `wall`.

## Existing Eldoria source (not copied)

Scene 02 also attempts to load `Valoria_MainGate_v1.glb`,
`Valoria_CivicHouse_v1.glb`, `Valoria_Workshop_v1.glb` from the
already-existing `Resources/Valoria/ProductionArt/StarterFamily/` folder
in the same Unity project. Their original provenance and permitted use are
governed by Eldoria production art records and **this trial does not change
or relabel their license**. If these models cannot be imported or aligned,
the scene explicitly falls back to locally built geometry.

## Verification still necessary

All model imports must be inspected *in Unity* for expected colors, textures,
origin, bounds, ground contact and renderer scale. Creative Commons permission
and Git object integrity are **not the same as** a successful Unity render or
a high-quality scene. Do not interpret these copies as an ART PASS.
