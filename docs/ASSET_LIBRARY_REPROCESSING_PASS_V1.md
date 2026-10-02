# ASSET LIBRARY REPROCESSING PASS v1 — AUDIT + EXECUTION PLAN

Date: 2026-10-02  
Branch: `visual-proof/asset-library-reprocessing-pass-v1`  
Base main: `b1e4cd0673765608bfe3d9ce5797f320df54468e`

## Routing

Primary route: `environment_composition`  
Surface subroute: `environment_surface`  
`geometry_gap_proven=false`  
`allow_tripo=false`  
Authorized credit cost: 0.

This pass reuses existing production geometry only. No new Tripo generation is authorized or required for the first execution.

## Family audit

| Family / asset | Class | Decision |
| --- | --- | --- |
| Hero Bastion / current hero fortress visual | REFINE + REASSEMBLE | Strong silhouette already proved; highest return is seating it into terraces/retaining masonry and normalizing its surface response, not replacing geometry. |
| Hero District / upper terraces | REASSEMBLE | Proven recipe; reuse current terrain/stone vocabulary to make Bastion + approach read as one mass. |
| Mid-Tier Architecture Kit v1 Piece01–04 | REFINE + REASSEMBLE | Good source family and production-proven; compact reframe leaves D1 as the useful surviving parcel. Improve its terrain/base integration and surface coherence rather than stamping more parcels. |
| Stone Architecture: CornerWallL | KEEP / REASSEMBLE | Repaired and certified; use as retaining shoulder/corner mass. |
| Stone Architecture: RockToWallTransition | KEEP / REASSEMBLE | Certified strongest seam piece; high-return terrain/architecture connector. |
| Stone Architecture: HighStraightWall | KEEP / REASSEMBLE | Repaired and certified; useful rear retaining spine. |
| Stone Architecture pieces 03/04/06/07/08 | REPLACE | Historical visual rejects: disconnected/fused/protruding geometry. Do not spend more cleanup cycles in this pass. |
| Terrain & Terrace: SteppedRockTerrace | KEEP / REASSEMBLE | Certified when top-aligned/buried; use as support, never isolated plinth. |
| Terrain & Terrace: BroadRockPlatform | KEEP / REASSEMBLE | Certified when top-aligned/buried; useful for bases/shelves. |
| Terrain & Terrace groups 02–06 | REPLACE | Historical rejected geometry; no more automatic salvage in v1. |
| ResidentialTerraceRock | KEEP | Production-proven historical rescue. Compact reframe deliberately reduces lateral housing; no reason to duplicate it now. |
| RockTerrainSeamFiller | KEEP | Production-proven support geometry; already solves seam role when restrained. |
| TowerWallRock | KEEP | Useful hero flank/defense vocabulary; already production-proven. |
| TerraceStairRock | KEEP | Recovered and Surface-v1-ready; omit from current core because certified stairs already own the vertical transition. |
| StreetLandingTransition | KEEP | Function-certified historical connector; not needed in accepted compact core. |
| GateStreetRiseRock MV1 | KEEP | Function-certified; not required for this pass and must not disturb certified circulation. |
| Aserradero AP2 | KEEP | Dedicated production asset with authored PBR; no structural problem. |
| Cuartel AP2 | KEEP | Dedicated production asset with authored PBR; no structural problem. |
| Granero BIII | KEEP | Dedicated production asset, isolated and integrated visual pass. |
| Ground Kit v1 / StoneKit support | KEEP / REASSEMBLE | Existing circulation/support layer remains useful; do not let visuals own gameplay floors. |
| Urban props / firewood / crate / barrel / sack | KEEP | Useful secondary detail; low priority until architecture/composition reads correctly. |
| Compatible Slavic / Mega fantasy support | KEEP | Raw construction vocabulary only; use selectively and preserve coherent Valoria language. |
| Historical generic/procedural houses hidden by Compact Footprint Reframe | REPLACE for production presentation | Their role is superseded in the accepted compact core; do not restore lateral residential sprawl. |
| World Map historical inventory | DEFER | Lower visual impact than current Valoria frame; audit after Valoria pass closes. |

## High-return shortlist

### REASSEMBLE
1. Hero Bastion + hero approach + upper retaining shoulders.
2. Mid-Tier D1 surviving compact-core parcel.
3. Stone Architecture + Terrain/Terrace seam system around the central vertical core.
4. Ground/retaining support around those same areas without adding buildings.

### REFINE
1. Hero Bastion surviving renderers: restrained stone response / smoothness normalization.
2. Mid-Tier D1 surface response and shared-material coherence.
3. Stone Architecture reused in the new assemblies.
4. Terrain/Terrace reused in the new assemblies.

## Execution boundary

The first implementation intentionally does **not**:
- create or regenerate geometry;
- restore D2/D3/W1/W2/W3 lateral housing removed by Compact Footprint Reframe;
- alter gameplay topology, route floors, colliders, hotspots or progression;
- replace Aserradero, Cuartel or Granero;
- touch rejected Stone/Terrain groups.

## Required proof

Same-scene camera-matched BEFORE/AFTER:
- zoom 19
- zoom 12
- zoom 9
- mobile 390x844

The collision/hotspot signature must be byte-for-byte identical before/after. Visual verdict is separate from technical verdict.
