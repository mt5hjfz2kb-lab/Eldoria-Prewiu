# ASSET LIBRARY CANONICALIZATION PASS v1 — FINAL RESULT

Date: 2026-10-02  
Status: **PASS / CLOSED**  
Visual changes: **none**  
Gameplay changes: **none**  
Tripo credits: **0**

## Objective

Turn the already-reviewed Eldoria asset library into a self-contained canonical repository library: persist reusable certified historical GLBs, normalize production locations, commit stable Unity metadata, and reconcile stale inventory documentation.

## Canonical execution

Final maintenance workflow:
- run: **36997740541 — SUCCESS**
- artifact: **11221974353**
- artifact digest: `sha256:6fa846c1e8e6d5878e7bfd38caa02fbe65ff125cd9176e6a93618261351c0de2`
- audit: `pipeline/asset-library-canonicalization-audit.json`

The final execution uses GitHub-hosted infrastructure; it does not occupy the Windows Unity runner and performs no Tripo operation.

## Final library audit

- canonical GLBs: **26**
- canonical GLBs missing committed `.meta`: **0**
- duplicate generic Mid-Tier storage tree: **removed**
- Mid-Tier production location: `Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/`
- Player City production location: `Unity/Assets/Eldoria/Resources/WorldPlayerCity/PlayerCity_v1.glb`

Every GLB listed in the canonical audit records:
- repository path;
- byte size;
- SHA-256;
- committed metadata presence.

## Historical certified assets recovered into main

The following were previously recoverable primarily through historical workflow artifacts. They are now physically persisted in the canonical library after exact SHA verification:

1. `TerraceStairRock.glb`
   - path: `Unity/Assets/Eldoria/Resources/Valoria/Rescued/TerraceStairRock.glb`
   - SHA-256: `83fce93daeb5bb455ab89bb195c39f617b3ea69ff9bf29f938ae4744caff3e5e`
   - source artifact: `10936719852`

2. `StreetLandingTransition.glb`
   - path: `Unity/Assets/Eldoria/Resources/Valoria/Rescued/StreetLandingTransition.glb`
   - SHA-256: `ef367d9f0f671cd29e1b02e2d36a2dfdea3acd6e789087e4da5fb5e087e81b01`
   - source artifact: `10944255594`

3. `GateStreetRiseRock_MV1.glb`
   - path: `Unity/Assets/Eldoria/Resources/Valoria/HistoricalLandmarks/GateStreetRiseRock_MV1.glb`
   - SHA-256: `9d1a97ea385d557b77779eef7a7b65f8027bfc79a56b26081abc6b62f25a8302`
   - source artifact: `10941357361`
   - limitation preserved: **landmark/support only; historical traversal/interface failure remains valid**.

Historical artifacts remain provenance/recovery evidence, but are no longer the only place where these reusable GLBs exist.

## Unity metadata normalization

All canonical Valoria/WorldPlayerCity GLBs now have committed Unity `.meta` files.

Existing Mid-Tier GLB metadata was moved with the assets so its GUIDs remain unchanged.

For GLBs that had no committed metadata, canonical stable glTFast ScriptedImporter metadata was generated and committed. This removes the previous fresh-clone risk where Unity could create different GUIDs on different machines.

## Mid-Tier normalization

Before:
`Unity/Assets/Resources/Valoria/MidTierArchitectureKit_v1/`

After:
`Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/`

Piece01–04 and their existing metadata were moved together. Runtime resource identity is unchanged because the relative Resources path remains:

`Valoria/MidTierArchitectureKit_v1/<Piece>`

No gameplay, scene-layout or visual-composition change is introduced by this relocation.

## Documentation corrections

The canonical inventories were reconciled with current repository reality:

- Player City v1 is production and is no longer listed as a missing first-acquisition target.
- Granero BIII is production/PASS and is no longer listed as missing.
- Stone Architecture v1 is represented as a partial production-safe family rather than a wholly missing kit.
- historical rescue registry now records canonical physical repository paths and current physical SHA identities.
- future asset acquisition should use the canonical audit before assuming a model is absent.

## Relationship to Asset Library Reprocessing Pass v1

This pass does not replace `ASSET_LIBRARY_REPROCESSING_PASS_v1`.

- Reprocessing Pass v1 answered: **which existing assets are worth keeping/refining/reassembling/replacing, and can they improve the real frame?**
- Canonicalization Pass v1 answers: **are the reusable results actually persisted, consistently located and stable for future Unity work?**

Together they establish the intended workflow:
1. inspect existing library;
2. classify KEEP / REFINE / REASSEMBLE / REPLACE;
3. prove visual value in the real frame;
4. persist reusable accepted assets in the canonical repo library;
5. only then consider new geometry.

## Final verdict

**PASS / CLOSED.**

The Eldoria canonical GLB library is now materially more self-contained and auditable: 26 inventoried GLBs, zero missing committed metadata, historical reusable GLBs persisted in main, and the Mid-Tier family normalized under the Eldoria resource tree.
