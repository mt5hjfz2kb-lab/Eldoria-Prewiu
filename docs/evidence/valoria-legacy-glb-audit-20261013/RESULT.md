# Valoria - independent visual audit of 36 assets (2026-10-13)

**Input:** 26 canonical historical GLBs + frozen Hero V3 and its 7 modules + prior bridge/stair. Owner-supplied visual target equals 10/10.

**Executed test:** [GitHub Actions #38393822753](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38393822753), output artifact ID 11809609192. Exactly 36/36 GLBs imported, 72 Blender material/clay renders, 0 meshes without UV and 0 non-finite vertices. The historical 26 were checked against their exact SHA256 manifest. No source modification or paid credits.

**Scoring:** independent visual judgement against the owner image, for each object's intended production role. Granularity 0.5 points. Fixed camera **orientation**, lighting, resolution and exposure; per-object framing normalized for model size. These scores do NOT certify Unity shader equivalence, gameplay use, real-device performance or aesthetic approval. Material-black candidates have lower grading confidence.

| # | Asset | Relative /10 | Decision |
|---:|---|---:|---|
| 1 | Valoria_HeroBastion_v1 | 7.0 | KEEP |
| 2 | GateStreetRiseRock_MV1 | 4.5 | SCENERY_ONLY |
| 3 | MidTier/Piece01 | 7.0 | REUSE |
| 4 | MidTier/Piece02 | 7.0 | REUSE |
| 5 | MidTier/Piece03 | 7.0 | REUSE |
| 6 | MidTier/Piece04 | 6.5 | REUSE |
| 7 | ResidentialTerraceRock | 4.0 | MATERIAL_DIAG |
| 8 | RockTerrainSeamFiller | 4.0 | MATERIAL_DIAG |
| 9 | StreetLandingTransition | 4.5 | SCENERY_ONLY |
| 10 | TerraceStairRock | 4.5 | SURFACE_FIX |
| 11 | TowerWallRock | 4.5 | MATERIAL_DIAG |
| 12 | CornerWallL | 7.0 | REUSE |
| 13 | HighStraightWall | 7.0 | REUSE |
| 14 | RockToWallTransition | 7.0 | REUSE |
| 15 | StoneKit_01 | 7.0 | REUSE |
| 16 | StoneKit_02 | 7.5 | REUSE |
| 17 | StoneKit_03 | 6.5 | REUSE |
| 18 | StoneKit_04 | 7.0 | REUSE |
| 19 | StoneKit_05 | 7.0 | REUSE |
| 20 | StoneKit_06 | 7.0 | REUSE |
| 21 | BroadRockPlatform | 6.5 | REUSE_SEAMS |
| 22 | SteppedRockTerrace | 6.5 | REUSE_SEAMS |
| 23 | Aserradero_AP2 | 7.0 | KEEP |
| 24 | Cuartel_AP2 | 7.5 | KEEP |
| 25 | Granero_BIII | 7.0 | KEEP |
| 26 | PlayerCity_v1 | 6.5 | WORLD_ONLY |
| 27 | HeroGate_V3 | 7.0 | BENCHMARK |
| 28 | StraightWall_V3 | 6.5 | REFINE |
| 29 | Watchtower_V3 | 6.5 | REFINE |
| 30 | Corner_V3 | 6.0 | REFINE |
| 31 | Connection_V3 | 5.5 | SUPPORT |
| 32 | StonePath_V3 | 6.0 | REFINE |
| 33 | CliffRocks_V3 | 4.0 | REAUTHOR |
| 34 | DefensiveProps_V3 | 5.5 | SUPPORT |
| 35 | Bridge_V1 | 5.5 | SUPPORT |
| 36 | Stairs_V1 | 5.5 | SUPPORT |

**Findings:** Older MidTier houses, Cuartel, Granero, Aserradero and several StoneKit / StoneArchitecture parts merit reuse. ResidentialTerraceRock, RockTerrainSeamFiller and TowerWallRock render almost black in standalone Blender but have surviving sculpted geometry; investigate import/material semantics before excluding them. GateStreetRiseRock, StreetLandingTransition and TerraceStairRock contain 0 embedded texture images. Hero V3 cliff has genuinely block-like cap/terminal ends; this needs geometry work beyond material tweaks.

**Independent earlier Unity evidence:** [30 actual Unity camera frames in run #38188882212](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38188882212) cover the Hero V3 family only.

**Outstanding required gate:** The requested 26-source identical-light/camera Unity 6 URP gallery is NOT executed. Active Bastion I workstream owns the windows-runner-heavy reservation and its player scene. Running another heavy Unity job would collide. The independent Ubuntu audit is complete, but a final Unity-specific per-piece 0-10 rating remains blocked until that resource is released by its owner.

**Status:** Independent Blender comparison delivered and exact sources verified; final Unity gate pending legitimate shared-runner ownership release.