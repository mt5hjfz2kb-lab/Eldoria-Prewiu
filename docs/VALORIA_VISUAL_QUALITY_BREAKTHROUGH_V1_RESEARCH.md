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
