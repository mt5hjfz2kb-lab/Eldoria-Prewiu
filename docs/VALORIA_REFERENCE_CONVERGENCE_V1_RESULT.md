# VALORIA REFERENCE CONVERGENCE v1 — RESULT

Date: 2026-10-02
Status: **TECH PASS / VISUAL FAIL / GEOMETRY GAP PROVEN**
Branch: `visual-proof/valoria-reference-convergence-v1`
Base composition: `VALORIA MASTER VISUAL REBUILD v2`

## Objective

Drive the blank-canvas Valoria composition toward the owner-approved reference: a monumental inhabited fortress-city carved into cliffs, framed by colossal ancient ruins, dense depth layers and visible reconstruction.

## Toolchain

- profile: `environment_composition`
- Tripo credits spent: **0**
- gameplay/collider/hotspot authority preserved
- official captures: zoom 19 / 12 / 9 / mobile
- existing library only during the proof

## Existing-library routes tested

The pass tested, integrated and visually rejected or limited the following existing routes at official camera:

1. MegaWall / MegaTower monumental framing — rejected: coarse rectangular module read.
2. ReferenceKit bridge / tower / wall family — rejected: large brick-box read in the real frame.
3. Arch_Gothic / Wall_Broken — rejected: slab/panel silhouette at required scale.
4. Stone_Gate / Stone_Tower — rejected: oversized dark gate blocks, visually foreign.
5. TowerWallRock + Stone Architecture + RockTerrainSeamFiller — cleanest existing-library result, but still insufficient for the benchmark's colossal broken-arch/cliff language.

The cleanest iteration also reused:
- Hero Bastion;
- Aserradero / Cuartel / Granero dedicated assets;
- Terrain & Terrace;
- Stone Architecture;
- RockTerrainSeamFiller;
- vegetation, props, flags, warm lights;
- continuous visual terrain.

## Final evidence

Latest accepted technical evidence reviewed:
- run: `36993896223` — SUCCESS
- artifact: `11220507506`
- digest: `sha256:fb72725e53fbc2896e74824204833d02797f07cb6fd4780b0666ca30cb2a3ebb`

The frame is cleaner and the functional third depth tier is better, but it remains materially below the approved visual benchmark.

## Visual diagnosis

What now works:
- compact vertical city logic;
- Bastion hierarchy;
- central ascent;
- dedicated functional buildings;
- visible future growth;
- rock/terrace integration is better than the legacy city;
- reconstruction cues and warm/cool separation.

What still fails against the reference:
- no convincing colossal ancient arch/aqueduct silhouette;
- insufficient vertical cliff architecture around the city;
- side framing remains too sparse;
- skyline does not communicate a much larger ruined civilization;
- environment still reads as a compact game diorama rather than a city occupying a monumental mountain ruin;
- existing ruin families become visibly modular when scaled to benchmark importance.

## Proven geometry gap

A new asset is justified, but only for the missing capability.

Required family: **Valoria Monumental Ruin & Cliff Kit v1**.

Minimum geometry:
1. **Colossal Broken Arch** — tall asymmetric ancient arch/aqueduct fragment, open negative space, broken crown, rock-grown foundations.
2. **Ruin Tower / Wall-Rock Hybrid** — vertical fractured masonry mass that can flank the Bastion without reading as a rectangular wall prefab.
3. **Cliff Architecture Connector** — stepped rock + masonry transition that visually carries terraces down a vertical cliff and accepts stairs/roads beside it.

These are environment modules, not houses and not gameplay buildings. They must be designed for partial burial, overlap and official isometric camera use.

## Decision

Do not promote this pass to `main` as benchmark-complete.

Keep the Blank Canvas master composition.

Keep all existing library material.

The next high-return action is to create the exact Tripo input for **Valoria Monumental Ruin & Cliff Kit v1**, stage it through the canonical exact-input pipeline, show exact input + visible Tripo cost, and stop for fresh owner authorization before Generate.
