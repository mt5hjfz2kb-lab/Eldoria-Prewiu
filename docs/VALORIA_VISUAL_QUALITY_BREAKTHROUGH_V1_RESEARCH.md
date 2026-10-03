# VALORIA VISUAL QUALITY BREAKTHROUGH v1 — applied research and tool decision

Date: 2026-10-03
Status: active experimental block.

## Real baseline
Source is the closed Asset Coherence implementation 1071454 / run 37143754581 / artifact 11281586517. Real 19/12/9/mobile review shows three high-impact structural defects: repeated cube/merlon wall rhythm, secondary buildings sitting as isolated source models with weak foundations, and the Hero Bastion's fused rock/retaining interface reading as a separate pedestal. The exterior is contained but still too uniform.

## Web research -> applied decisions
- Unity URP decals can blend seams and add wear, but they require renderer configuration and have mobile trade-offs; screen-space decals are the mobile-friendlier mode. For this first breakthrough candidate they are **deferred**, because the dominant defects are silhouette/interface, not missing surface dirt. Sources: Unity URP Decal docs and renderer-feature reference.
- Unity Terrain Shader Graph supports triplanar projection, height-based blending and repetition breakup, but adding a new Terrain system would be unnecessary and risks reopening the locked macrocomposition. **Rejected for this block**; keep the contained low-relief/mesh support field.
- MeshRenderer additionalVertexStreams can carry vertex paint/masks but conflicts with GPU instancing. **Deferred**; current source-family defects can be tested first with replaceable authored modules and existing semantic world-space material response.
- Modular medieval environment breakdowns emphasize blockout silhouette, gatehouse hierarchy and readable areas of interest before fine detail. **Applied**: fewer longer curtain modules, deliberately unequal curtain spans/heights, stronger gate shoulder hierarchy and corner treatment instead of repeated full-height tower stamps.

## Toolchain evaluation
Current tools are not the ceiling here. Unity + the canonical library can test the biggest visual hypothesis without new software:
1. StoneArchitectureKit_v1 provides detailed authored wall/corner/rock-transition geometry.
2. Rescued RockTerrainSeamFiller / StreetLandingTransition / TerraceStairRock directly target the Bastion-city seam.
3. StoneKit piece_05/06 can create foundations and work-yard mass around functional buildings.
4. Existing CC0 dirt material + canonical foliage can author a bounded exterior edge.

Blender source segmentation remains the next escalation if the imported transition kit cannot sufficiently bury the fused Hero silhouette. No new plugin is adopted in iteration 1. Material Maker remains a future free pilot only if tiling-material authoring becomes the measured bottleneck; ArmorPaint/Substance remain unnecessary until geometry/interface quality is proven.

## Stop rule
This candidate is valid only if the 19/12/9/mobile frame changes immediately at wall rhythm, building grounding and Bastion integration. If the result mainly changes tint or adds clutter, revert. Maximum 2–3 attempts per technique family.


## Evidence-driven decisions after initial research

### Wall family
The accepted Art Consolidation wall base is retained. Replacing it wholesale with separated authored wall segments caused visible fragmentation and was rejected in run 37146098026 / artifact 11282360873.

A later structural pass replaced only the repetitive primitive merlon rhythm while retaining the continuous base, using canonical Stone_Wall spans with controlled unequal lengths/heights plus CornerWallL corner treatment. Run 37147632315 / artifact 11282847741 was reviewed at real 19/12/9/mobile and is retained as an improvement: stronger rhythm and less kit repetition without occupying XW/XE.

### Secondary architecture
A normalized Unity lineup compared canonical Aserradero/Cuartel/Granero against MidTier 01–04, Slavic town/administrative candidates and Mega House. Run 37147508623 / artifact 11282981323 showed that the three current functional GLBs are already among the strongest secondary sources in the library. Direct Slavic replacement was rejected because it weakens detail/material coherence; generic Stone wall/tower replacement was also rejected because it reads as fortification rather than functional architecture.

Blender source audit run 37147770128 / artifact 11283077627 proved all three canonical functional GLBs have full PBR inputs and healthy geometry, but only one material each:
- Aserradero: 49,800 tris; 2048 basecolor + 1024 RM + 1024 normal.
- Cuartel: 49,800 tris; 2048 basecolor + 1024 RM + 1024 normal.
- Granero: 49,800 tris; 2048 basecolor + 1024 RM + 2048 normal.
Therefore remeshing/replacement is not justified. Semantic material authoring is the correct intervention.

The semantic shader pass uses height, surface orientation, source luminance/chroma and existing PBR maps to infer roof/foundation/timber response without altering geometry. The first pass exposed partial roof masking; the second and final pass widened slope coverage and reduced color dominance. Run 37148245980 / artifact 11283083226 is retained.

### Hero Bastion source
Blender audit run 37146685234 / artifact 11283120541 proved the Hero is one 49,800-triangle mesh with one material but 2,792 disconnected geometric components. This justified a controlled segmentation experiment without regeneration.

Three zero-cost candidates were authored:
- low14: remove 5,125 tris / 10.29%;
- low20: remove 8,067 tris / 16.20%;
- low26: remove 11,852 tris / 23.80%.

Real Unity comparison showed low14/low20 barely change pedestal reading while low26 begins to open holes and remove valid architectural silhouette. Height-only segmentation is therefore closed and must not be continued with higher thresholds.

The current alternative keeps the canonical Hero completely intact and re-authors only its city interface using existing HighStraightWall / CornerWallL / RockToWallTransition pieces. This is a different technique family: architectural wrapping rather than source subtraction.

### Exterior edge decision
The first exterior-field candidate was rejected because large flat brown wedges read as texture patches rather than world structure. The next exterior pass will not reintroduce a large Terrain or opaque carpet.

Unity documentation confirms Terrain details are available in URP, but using a Terrain system would add unnecessary infrastructure for the locked Flat Citadel. URP screen-space decals are a viable mobile-oriented option for local wear/seams, but are not a substitute for edge silhouette and must remain sparse. The preferred next technique is a small number of opaque low-relief edge meshes, grouped low vegetation and dissolving road/ditch transitions with distance/culling limits. Avoid broad transparent foliage coverage because mobile fill-rate/overdraw remains a risk.

### Toolchain evolution result so far
- Unity + existing canonical GLBs: sufficient for accepted wall rhythm and semantic material uplift.
- Blender: materially useful for source diagnosis and deterministic geometry experiments. Keep it in the pipeline for evidence-led source work.
- Tripo/paid generation: not needed; spend remains 0.
- New Unity terrain/foliage plugin: not justified yet.
- Decal Renderer Feature: technically valid for a later sparse local-wear pilot, preferably screen-space on tile-based mobile GPUs; not adopted globally.
- Material Maker / Substance / ArmorPaint: no demonstrated bottleneck requiring adoption yet.
- Self-mutating GitHub Actions receipts were found to create non-fast-forward/cancellation fragility and have been removed from the main VQB workflow. Runs/artifacts are the authoritative execution evidence.

## Anti-loop ledger
Closed technique families that must not be silently retried:
1. wholesale wall replacement by separated curtain pieces;
2. flat exterior field wedges;
3. small MidTier annex placement as the main secondary-architecture solution;
4. direct Slavic functional replacement;
5. generic Stone_Wall/Stone_Tower functional replacement;
6. Hero height-only segmentation;
7. repeated tint-only material changes.

Retained techniques:
1. continuous wall base + authored non-uniform cap rhythm;
2. canonical functional GLBs + semantic PBR response;
3. canonical Hero intact + architectural interface wrapping (currently under validation).
