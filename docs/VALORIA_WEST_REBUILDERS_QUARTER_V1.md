# Valoria — West Rebuilders Quarter v1

Status: PRODUCTION INTEGRATED / VISUAL PASS  
Updated: 2026-09-29

## Scope

This is the first real expansion beyond the certified first production district.

The west Master Envelope reserve is no longer only empty planning terrain. A bounded part of it now reads as an inhabited reconstruction/work quarter connected visually to the Aserradero side of Valoria.

The expansion does not move certified gameplay topology, the independent 12-step route, camera family, hotspots or Master Envelope reservations.

## Production content

Integrated directly in `VisualWorld`:

- west rebuilders terrace;
- west rebuilders upper shelf;
- six staggered stone/cobble route fragments;
- five lower rebuilders homes;
- two work courts with firewood/fence dressing;
- two upper-shelf dwellings;
- two reused `RockTerrainSeamFiller` instances used only as visual geology/burial;
- restrained banner and pine identity cues.

No new paid asset and no Tripo generation was used.

## Bastion Hero Pass correction

The same production block corrected the dominant Bastion silhouette defect visible at official zoom 12/9.

Changes:
- oversized detached gable masses were replaced by low grounded fortress crowns;
- the final visibly detached lantern cap was removed;
- the existing authored lantern tower remains the skyline marker;
- no gameplay/collider/hotspot contract changed.

Final art commit for the floating-crown fix:
- `d84b31811fe044dbc3901f3ecd6edde13a635cc6`

## Official visual evidence

Valoria Visual Formula:
- run: **36546445386**
- artifact: **11022647974**
- result: **SUCCESS**
- head: `d84b31811fe044dbc3901f3ecd6edde13a635cc6`
- official zoom family: 19 / 12 / 9
- final Bastion capture no longer shows the detached crown.

LookDev:
- run: **36546445341**
- result: **SUCCESS**

Measured scene complexity:
- active renderers: **644**
- unique materials: **80**
- scene triangles: **567,460**
- lights: **15**

Delta from the first production district:
- +107,224 triangles;
- +131 active renderers;
- +8 unique materials;
- +7 lights.

## Runtime / interaction evidence

Unity slice run **36546445337**, first attempt, executed the relevant production checks successfully before the workflow was later marked cancelled by concurrency/supersession from newer `main` commits:

- EditMode: **SUCCESS**
- PlayMode: **SUCCESS**
- Windows desktop build: **SUCCESS**
- Valoria art benchmark render: **SUCCESS**
- artifact upload: **SUCCESS**
- checkout cleanup: **SUCCESS**

The overall workflow conclusion is therefore not used as a clean certification label. The cancellation was not caused by a failing Eldoria test; all substantive first-attempt steps completed successfully before a newer concurrent run superseded it.

Later Bastion-I/II development continued advancing `main` without changing the west-quarter / Bastion visual implementation.

## Visual verdict

**VISUAL PASS for West Rebuilders Quarter v1 scope.**

The west side now reads as:
- a connected inhabited extension of Valoria rather than empty reserve;
- a reconstruction/work district related to the Aserradero;
- vertically layered instead of flat suburban sprawl;
- integrated through terrain/rock rather than isolated prefabs;
- coherent with the frozen Valoria Visual Formula v1.

The Bastion now has a substantially cleaner grounded skyline and remains the city focal anchor.

## Performance policy

The measured production baseline is recorded in:
`pipeline/mobile-visual-performance-gate.json`

Current desktop/editor baseline:
**567,460 scene triangles / 644 renderers / 80 materials / 15 lights**

This is not a mobile budget.

Before reducing quality:
1. build the same production state for a representative physical phone;
2. measure official zoom-12 overview;
3. measure west-quarter pan;
4. measure Aserradero + Cuartel together;
5. measure Bastion/hero view;
6. record FPS/frame time, peak memory, texture memory where available, batches/draw calls, resolution and sustained/thermal notes;
7. derive SUPPORT / PRIMARY / HERO budgets from measured bottlenecks only.

## Freeze / next production rule

Do not reopen Visual Formula v1 or broad tool research.

The next art expansion should follow the real building-production inventory and Bastion chronology. The west quarter is now a production precedent for expanding reserved districts without moving gameplay topology.
