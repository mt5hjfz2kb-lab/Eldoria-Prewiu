# Asset Library Canonicalization Pass v1 — Final Result

Date: 2026-10-02  
Status: **TECH PASS / LIBRARY PASS**  
Visual changes: **none**  
Gameplay changes: **none**  
Tripo credits: **0**

## Objective

Turn Eldoria's accumulated 3D library into a stable canonical repository state instead of leaving reusable assets split across production folders, historical GitHub Actions artifacts, stale documentation, and Unity imports without committed metadata.

This block follows the completed `ASSET LIBRARY REPROCESSING PASS v1`. It does **not** re-grade the visual quality of every asset; it canonicalizes the assets that current production and rescue documentation says Eldoria owns.

## Canonical execution

Successful canonicalization:
- run: **36997740541 — SUCCESS**
- artifact: **11221974353**
- promotion commit: **3626b635a98a05df62dfde6ddbc3c784355326b6**
- audit: `pipeline/asset-library-canonicalization-audit.json`

Final audit:
- canonical GLBs: **26**
- canonical GLBs missing committed `.meta`: **0**
- Tripo credits: **0**
- new geometry generated: **0**

A later maintenance rerun `36997838861` reproduced the same 26/0 audit but failed only in its post-audit git staging because the old `Unity/Assets/Resources/Valoria` directory had already been removed by the successful migration. No asset/data validation failed. The maintenance script was subsequently made idempotent and the workflow was parked as manual-only.

## Historical GLBs recovered into main

The following certified historical assets previously depended on Actions artifacts. They were recovered by **exact SHA-256 match** and are now persisted in the canonical Unity library:

| Asset | Canonical path | Certified SHA-256 | Source artifact |
| --- | --- | --- | ---: |
| TerraceStairRock | `Unity/Assets/Eldoria/Resources/Valoria/Rescued/TerraceStairRock.glb` | `83fce93daeb5bb455ab89bb195c39f617b3ea69ff9bf29f938ae4744caff3e5e` | 10936719852 |
| StreetLandingTransition | `Unity/Assets/Eldoria/Resources/Valoria/Rescued/StreetLandingTransition.glb` | `ef367d9f0f671cd29e1b02e2d36a2dfdea3acd6e789087e4da5fb5e087e81b01` | 10944255594 |
| GateStreetRiseRock MV1 | `Unity/Assets/Eldoria/Resources/Valoria/HistoricalLandmarks/GateStreetRiseRock_MV1.glb` | `9d1a97ea385d557b77779eef7a7b65f8027bfc79a56b26081abc6b62f25a8302` | 10941357361 |

`GateStreetRiseRock MV1` remains **landmark/support only**. Canonical persistence does not erase its documented traversal/interface failure.

## Mid-Tier library normalization

The four production Mid-Tier GLBs were moved from:

`Unity/Assets/Resources/Valoria/MidTierArchitectureKit_v1/`

to:

`Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/`

The existing `.meta` files moved with the assets, preserving their Unity GUIDs.

The runtime address remains unchanged because both locations resolve to the same Resources-relative key:

`Valoria/MidTierArchitectureKit_v1/Piece01..04`

This removes the second generic production-library root without changing runtime loading semantics.

## Stable Unity metadata

Every canonical GLB under:
- `Unity/Assets/Eldoria/Resources/Valoria/`
- `Unity/Assets/Eldoria/Resources/WorldPlayerCity/`

now has a committed Unity `.meta`.

Existing metadata/GUIDs were preserved where present. Missing metadata received deterministic stable GUIDs using the existing glTFast ScriptedImporter contract.

This includes Hero Bastion, Aserradero, Cuartel, Granero, Player City, Stone Architecture, Terrain & Terrace, StoneKit, rescued historical modules, and Mid-Tier Piece01–04.

## Canonical inventory shape

The 26 audited GLBs comprise:

- Hero Bastion: 1
- Historical landmark: 1
- Mid-Tier Architecture Kit: 4
- Rescued support family: 5
- Stone Architecture production subset: 3
- StoneKit: 6
- Terrain & Terrace production subset: 2
- dedicated Valoria buildings: 3
- World Player City: 1

Rejected source groups/pieces are **not** promoted merely to increase the count. They remain historical evidence where documented.

## Documentation reconciliation

This block also corrects stale inventory statements:
- Granero BIII is production/PASS, not missing.
- Hero Bastion v1 is the production hero anchor after the reprocessing pass.
- Player City v1 already exists as the universal world-map city mesh; multiple city tiers are future expansion rather than a v1 blocker.
- Stone Architecture already has a three-piece production subset; the library gap is no longer “create Stone Architecture Kit from zero”.
- the remaining dedicated Arc-I building gaps are Cantera, Forja and Hospital.

Updated sources:
- `docs/ELDORIA_ASSET_LIBRARY_ROADMAP_V1.md`
- `docs/VALORIA_BUILDING_PRODUCTION_INVENTORY_V1.md`
- `pipeline/historical-module-rescue.json`
- `SESSION_HANDOFF.md`

## Maintenance rule

The reusable maintenance entry points remain:
- `tools/canonicalize-asset-library.sh`
- `.github/workflows/asset-library-canonicalization.yml`

The workflow is manual-only after this migration. It validates exact historical identities, canonical placement and `.meta` completeness without consuming the Windows Unity runner or Tripo credits.

## Final verdict

**PASS.**

The production/reusable GLB library is now materially more self-contained:
- reusable certified historical geometry no longer depends solely on expiring Actions artifacts;
- production GLBs are consolidated under the Eldoria-owned Resources tree;
- Unity metadata is stable and committed;
- the canonical inventory is machine-auditable;
- stale “missing asset” documentation has been reconciled.

This is a persistence/organization pass, not a claim that all 26 GLBs have equal visual quality or should all be placed in the current scene.
