# Asset Visual Uplift Pass v1 — Result

Date: 2026-10-02  
Status: **IN PROGRESS — visual evidence gate pending**  
Branch: `visual-proof/asset-visual-uplift-v1`  
Tripo credits: **0**  
New source geometry: **0**  
Gameplay topology changes: **0 intended; gate enforces collider/hotspot signature identity**

## Objective

Improve the perceived quality of the highest-return assets already present in the canonical Valoria library. This is a selective surface/material/integration pass, not a blanket library rewrite and not a new-asset generation block.

The pass starts from the production result of `ASSET LIBRARY REPROCESSING PASS v1` and the canonicalized 26-GLB library. It does not alter canonical GLB bytes.

## Concurrency boundary

At start, `valoria-reference-convergence-v2` owned `ProductionVisualIntegration.cs`, `pipeline/art-production-request.json`, Valoria production composition and the Windows Unity runner.

This workstream therefore narrowed itself to non-conflicting surface/material treatment and its own validation harness. It did not mutate the concurrent composition workstream or consume its runner while the lock remained active.

## Selection

### 1. Hero Bastion — SELECTED / high return

Canonical source:
`Unity/Assets/Eldoria/Resources/Valoria/HeroBastionGenerated/Valoria_HeroBastion_v1.glb`

Reason:
- dominant focal asset at every official zoom;
- geometry/silhouette already certified;
- remaining leverage is surface/value hierarchy rather than regeneration.

Executed treatment:
- preserve existing authored textures/normals;
- role-sensitive stone / rock / roof / metal / accent separation;
- restrained roughness and normal-response normalization;
- no GLB mutation, no scale/topology/collider/hotspot change.

### 2. Dedicated production buildings — SELECTED / high return

Sources:
- `Valoria_Aserradero_AP2_v1.glb`
- `Valoria_Cuartel_AP2_v1.glb`
- `Valoria_Granero_BIII_v1.glb`

Reason:
- visible functional landmarks;
- already production-valid geometry;
- strong benefit from clearer stone/timber/roof separation and consistent Valoria roughness.

Executed treatment:
- renderer-local MaterialPropertyBlock treatment;
- functional palette distinction for production / military / granary roles;
- existing maps and source materials remain authoritative;
- no geometry or gameplay ownership changed.

### 3. Stone Architecture production subset — SELECTED / highest surface return

Sources:
- `CornerWallL.glb`
- `HighStraightWall.glb`
- `RockToWallTransition.glb`

Reason:
The previous reprocessing pass deliberately replaced all sub-materials on these instances with one flat architectural-stone material. That solved coherence but discarded potentially useful source base/normal/AO information and collapsed material separation.

Executed treatment:
- retain source base map where present;
- retain normal map where present;
- retain occlusion/mask information where representable in URP Lit;
- normalize roughness/metallicity to Valoria Stone;
- role-sensitive fallback tint rather than one universal flat material.

### 4. Terrain & Terrace production subset — SELECTED / highest surface return

Sources:
- `BroadRockPlatform.glb`
- `SteppedRockTerrace.glb`

Reason:
These are large screen-space support masses under Hero/core architecture. The old flat replacement material suppresses their surface information and makes rock/terrace depth read weaker than the geometry allows.

Executed treatment:
- preserve existing texture/normal/AO inputs;
- stronger but controlled rock normal response;
- low-gloss rock/earth family;
- no geometry/regeneration.

### 5. Mid-Tier Architecture Piece01–04 — SELECTED / medium-high return

Reason:
The geometry is already accepted for compact-core support. Previous treatment largely applied one fallback tint per piece; the current pass separates readable roof/timber/stone/rock roles while leaving original source materials and textures intact.

Executed treatment:
- per-submaterial classification;
- darker roof;
- warmer/desaturated timber;
- medium architectural stone;
- darker rock/base;
- restrained accent handling;
- MaterialPropertyBlock only.

### 6. Rescued support — SELECTIVE

Current canonical family:
- ResidentialTerraceRock
- RockTerrainSeamFiller
- StreetLandingTransition
- TerraceStairRock
- TowerWallRock

Selected in-frame treatment:
- currently visible rescued renderers receive restrained support-family surface normalization.

Practical-ceiling / deferred cases:
- `StreetLandingTransition` and `TerraceStairRock` are not primary current compact-core visual drivers; forcing new placement merely to show an uplift would violate the selective-pass rule.
- `ResidentialTerraceRock` is retained where already useful but is not expanded laterally.
- `TowerWallRock` remains useful as a visual-only defensive support mass.
- `RockTerrainSeamFiller` remains a seam/burial support rather than a hero asset.

## Explicit exclusions

- `GateStreetRiseRock_MV1`: no primary treatment. Its historical traversal/interface failure remains unchanged; visual-only landmark status remains.
- Stone Architecture historical rejected pieces 03/04/06/07/08: no salvage attempt.
- Terrain & Terrace historical rejected groups 02–06: no salvage attempt.
- no new GLB;
- no Tripo generation;
- no paid operation;
- no Blender processing because no geometry/cleanup defect has yet justified it.

## Implementation

New runtime surface layer:
- `Unity/Assets/Eldoria/Scripts/Presentation/AssetVisualUpliftPassV1.cs`

Integration point:
- `AssetLibraryReprocessingPassV1.cs` now routes its existing Stone/Terrain instances through the uplift material-preservation path when enabled;
- Mid-Tier instances receive sub-material separation;
- already-instantiated Hero/dedicated/rescued assets receive renderer-local surface treatment.

Validation:
- `Unity/Assets/Eldoria/Scripts/Editor/AssetVisualUpliftGateV1.cs`
- `.github/workflows/asset-visual-uplift-v1.yml`

The gate rebuilds deterministic BEFORE and AFTER scenes, captures zoom 19 / 12 / 9 / mobile, records scene metrics, and requires identical gameplay collider/hotspot signatures.

## Technical invariants

The uplift code:
- does not create or move gameplay colliders;
- does not create or move hotspots;
- does not edit GLB bytes;
- does not add source geometry;
- uses renderer materials / MaterialPropertyBlock only for the selected surface changes;
- keeps Tripo at 0 credits.

## Visual evidence

Pending the owned Unity runner becoming available under the canonical workstream protocol.

No VISUAL PASS is claimed until the matched 19/12/9/mobile evidence is inspected.

## Final verdict

**PENDING VISUAL GATE.**

This document must be updated with run/artifact IDs, before/after metrics, actual frame review and the final PASS/FAIL/ceiling classification before promotion.
