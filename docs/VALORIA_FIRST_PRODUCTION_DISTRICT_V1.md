# Valoria — First Production District v1

Status: PRODUCTION INTEGRATED / VISUAL PASS  
Updated: 2026-09-29

## Purpose

Record the first small Valoria district that is no longer an isolated module demo. It uses the real playable VisualWorld topology, the frozen Valoria Visual Formula v1, dedicated production buildings and rescued certified historical geometry.

This record does not reopen the visual formula or historical traversal failures.

## Production composition

Real VisualWorld contains:

- dedicated Aserradero AP2 production asset;
- dedicated Cuartel AP2 production asset;
- rescued ResidentialTerraceRock on the certified upper civil plot;
- rescued RockTerrainSeamFiller used three times to bury/integrate seams around west, residential and east areas;
- the existing independent 12-step gameplay route;
- canonical neutral-overcast Valoria v1 environment.

ResidentialTerraceRock and RockTerrainSeamFiller are production Resources under `Unity/Assets/Eldoria/Resources/Valoria/Rescued/`.

Gameplay topology, colliders and hotspots remain independent of these visual meshes.

## TerraceStairRock decision

TerraceStairRock was recovered and Surface-v1-ready, then fit-tested in the district.

Result: omit it from this first district.

Reason:
- the certified 12-step route already owns the vertical transition;
- the rescued terrace mass was visually redundant/partly occluded;
- keeping it would add geometry without improving the official-camera read.

This is a composition decision only. The asset remains reusable for a later district or another vertical transition.

## Official evidence

### Visual Formula / production overview

- run: **36541695034**
- artifact: **11021096851**
- result: **SUCCESS**
- official evidence includes zoom 19 / 12 plus focused zoom 9 captures and mobile framing.
- measured real scene:
  - active renderers: **513**
  - unique materials: **72**
  - scene triangles: **460,236**
  - lights: **8**

### First production district

- run: **36543056731**
- artifact: **11021850361**
- result: **SUCCESS**
- topology: **REAL_VISUALWORLD_PRODUCTION_DISTRICT**
- official district captures: **19 / 12 / 9**
- measured district-focused scene:
  - active renderers: **428**
  - unique materials: **55**
  - scene triangles: **460,236**
  - lights: **8**

Only Bastion renderers/lights are suppressed in the district-focused evidence so the lower/upper production district can be judged cleanly. The rescued pieces shown are the real VisualWorld production instances.

### Runtime / interaction protection

- Unity slice run: **36541695042**
- result: **SUCCESS**
- EditMode: PASS
- PlayMode: PASS
- Windows build: PASS
- Valoria capture: PASS

The rescued visual layer therefore did not break the playable topology, building interaction or runtime gate.

### LookDev recheck

- run: **36541695060**
- result: **SUCCESS**

The canonical neutral-overcast Valoria v1 LookDev remains the production baseline.

## Visual verdict

**VISUAL PASS for first-production-district scope.**

The district now reads as a small real part of Valoria rather than a collection of isolated module tests:
- dedicated production/military buildings retain their own material identity;
- upper civil habitation is visible;
- architecture is buried into continuous terrain/rock rather than floating;
- rescued seam geometry is support-only and does not dictate circulation;
- warm inhabited accents survive the dark-fantasy palette;
- official zoom 19/12/9 evidence exists.

This is not a claim that all of Valoria is final art. Bastion architecture, broader district density, skyline and later expansion areas remain future production work.

## Formula/freeze rule

Valoria Visual Formula v1 remains **VALIDATED / FROZEN**.

Do not reopen broad LookDev/tool research because of local production defects. Route defects go to composition/gameplay, surface defects go to material/lighting integration, and measured performance defects go to role budgets/LOD/texture policy.

No paid geometry regeneration is justified for the rescued assets used here.

## Mobile scaling baseline

The existing `pipeline/mobile-visual-performance-gate.json` now contains the measured desktop/editor production baseline from this district.

Current measured geometry baseline is **460,236 scene triangles**. This is not a mobile limit.

Next scaling step:
1. build the same commit/scenario for a representative target phone;
2. measure zoom-12 overview, dense district pan, Aserradero+Cuartel together and Bastion/hero view;
3. record FPS/frame time, peak memory, texture memory where available, batches/draw calls, resolution and thermal/sustained-run notes;
4. derive SUPPORT / PRIMARY / HERO budgets from those measurements;
5. preserve current quality if the device is healthy; optimize only the measured bottleneck if it is not.

Do not pre-emptively degrade visual quality.

## Zero-credit statement

Historical rescue, Surface v1 treatment, composition tests, production promotion and validation in this block spent **0 Tripo credits**.
