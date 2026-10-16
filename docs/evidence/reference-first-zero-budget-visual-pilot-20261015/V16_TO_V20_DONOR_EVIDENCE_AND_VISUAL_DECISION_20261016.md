# VALORIA — Native Unity donor audit + controlled V20 composition comparison

**Owner's canonical image:** 1513x1039 SHA256 `de476e0516f744f9cd899419404971fa5e8d42bd49f2f7db7670d1cb325f8a4f`. Reference 10/10, required acceptance 8/10.

**Outcome: ART_FAIL; NO PROMOTION TO GAME.** This sequence isolates a material-compatibility issue, but none of the six inspected terrain modules replaces a coherent, artist-authored continuous meseta/plateau. Source original prefabs/GLBs are unchanged, production/public game untouched, all manifests retain `artApproved=false`.

## Real workflow runs and matching original frames

- [A/B 2 platform donors #38861107452](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38861107452): [oblique](terrain-donors/31-broad-vs-stepped-oblique.png) and [top](terrain-donors/32-broad-vs-stepped-high.png). BroadRockPlatform (left) is a stone-paved island module; SteppedRockTerrace (right) is a smaller stair fragment. Both use a real inherited GLB; neither is a 130m meseta.
- [Four original rock/road transition donors #38864541121](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38864541121): [oblique](transition-donors/33-rock-transition-oblique.png) and [high](transition-donors/34-rock-transition-high.png). A RockTerrainSeamFiller renders nearly black, B StreetLandingTransition and C TerraceStairRock render near-white, while D RockToWallTransition retains coherent textured stone. Rendering original as-is fails palette-coherence.
- [Four material-neutral geometry controls #38864994482](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38864994482): [oblique](transition-neutral/35-neutral-transition-oblique.png) and [high](transition-neutral/36-neutral-transition-high.png). Each original imported GLB mesh is visible when a neutral URP shader is assigned to its scene **instance only**. All 4 imported original material slots report `Shader Graphs/glTF-pbrMetallicRoughness`. This diagnoses material appearance, NOT original source geometry failure.
- [Stone-material experiment #38865388028](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38865388028): [oblique](transition-stone-study/37-stone-study-oblique.png), [high](transition-stone-study/38-stone-study-high.png). Original project rock/limestone textures with URP Lit correct extremes on A/B/C, D left unchanged as honest control. The result is still dark/flat/inconsistent; never call it a professional material finish.
- [Native Unity close-up #38865787439](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38865787439): [A RockTerrainSeamFiller close](transition-close-study/39-seam-filler-detail.png), [D RockToWallTransition close](transition-close-study/40-rock-to-wall-detail.png). A is low-contrast with limited recognizable stone texture. D has detailed irregular stone mass and defined old masonry courses; D is a plausible junction **donor** but too dark and vertical for long meadow cliffs.
- [Real scene V20 #38866182984](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38866182984): [overview](41-transition-integrated-overview.png), [close](42-transition-integrated-close.png). Reuses four D modules, two near the lower gate, two along the upper wall. Exact camera & scene V15 aside from these donors. **ART REJECT:** makes grey rock-pillar accents beside beige walls, not a connected geological surface; the water/background, forest, path and courtyard remain at low quality.

## Exact image-difference checkpoint

Using RGB `max(|V15 channel − V20 channel|)` per pixel at the same 1536x1024 camera:
- At 2+ intensity, ~**1.82% of pixels** differ.
- At 8+ intensity, ~**0.90%** differ.
- At 20+ intensity, ~**0.50%** differ.
These are measurable changed-image areas, **NOT** quality gains. The new donor's tiny image footprint explains why it cannot repair the major fidelity gap.

## Factual donor decision table

| Candidate | Import/geometry | Observed aesthetic/usage | Decision |
| --- | --- | --- | --- |
| BroadRockPlatform.glb | Unity PASS, 1 renderer | Pebbled, paved terrain island; scale/content inappropriate for coherent main plateau | Do not use as giant terrain |
| SteppedRockTerrace.glb | Unity PASS, 1 renderer | Smaller sculpted step/road fragment | Detail candidate only, no approval |
| RockTerrainSeamFiller.glb | Unity PASS; original near-black material | With project texture can serve narrow contacts, but close view still flat/dark | Do not promote |
| StreetLandingTransition.glb | Unity PASS; original white | Temporary stone material helps readability but differs from reference | Do not promote |
| TerraceStairRock.glb | Unity PASS; original white | Stair detail only and too small for main terrain | Do not promote |
| RockToWallTransition.glb | Unity PASS; original textured | Highest-quality junction mesh of four, but D at four points did not improve whole scene | Keep isolated candidate, **REJECT integration V20** |

**Next action:** review the unused preexisting `SM_Cliffs_01`, `SM_Cliffs_03`, `SM_Terrain_03` source prefabs as a single controlled original-material landscape family before selecting an actual geological backbone. If they are equally insufficient, stop duplicating lookdev and commission a cohesive modeled terrain base using Blender/manual source, not more isolated rock props.

No source ownership or licenses are presumed cleared; none of the GLBs or new render samples receives ART_APPROVED, and no player scene or release was changed.
