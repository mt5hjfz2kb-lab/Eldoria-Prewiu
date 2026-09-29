# Valoria — Library Production Plan v1

Status: ACTIVE PRODUCTION BASELINE  
Updated: 2026-09-29

## Purpose

Turn Valoria production into a reusable construction library rather than a sequence of one-off scenes.

Canonical rule:

**city topology first; reusable production library second; scene assembly third.**

The library must let later districts, Bastion states and functional buildings be assembled faster without forcing roads, floors, plots or progression to fit an asset.

## 1. Library layers

### A. Ground / circulation kit — P1
This is the shared visual foundation used beneath every district.

Required reusable families:
- worn stone street straight;
- street bend / widening;
- plaza / courtyard surface;
- terrace floor;
- stair/landing surface skin that preserves the certified physical route;
- parcel edge / retaining masonry;
- rock-to-floor seam;
- damaged/early-state variants and repaired/later-state variants.

Rules:
- these are visual skins over approved topology, not gameplay floors;
- they must tile/overlap without obvious seams at zoom 19/12/9;
- geometry should be much lighter than dedicated buildings;
- material language must inherit VALORIA_VISUAL_FORMULA_v1;
- support multiple progression states without moving circulation.

Existing useful inventory:
- RockTerrainSeamFiller — seam/burial support;
- StreetLandingTransition — limited visual transition reference;
- TerraceStairRock — later transition reference when not redundant;
- authored cobble/stone materials already used in VisualWorld.

Current gap:
**no production-certified reusable Ground Kit exists yet.**

### B. Residential / support architecture kit — P1
Reusable inhabited architecture for districts that should not require one bespoke hero asset per house.

Required families:
- small Valoria house;
- medium/two-volume house;
- small workshop/storage shed;
- food/storage support mass;
- low wall / courtyard edge;
- roof/facade variants;
- props: sacks, crates, barrels, firewood, carts, fences.

Existing useful inventory:
- ResidentialTerraceRock — larger inhabited mass;
- RockTerrainSeamFiller — terrain integration;
- procedural ValoriaKit.House — blockout/reference only, not final support quality.

Current gap:
**no final-quality small residential/support kit exists yet.**

### C. Dedicated functional buildings — PRIMARY
Unique world-space buildings tied to gameplay.

Production:
- Aserradero — PASS;
- Cuartel — PASS.

Next chronology:
1. Granero — MISSING;
2. Cantera — MISSING;
3. Forja — MISSING;
4. Hospital — MISSING.

Each dedicated building reserves a maximum upgrade envelope and may reuse Ground/Support kit pieces around it.

### D. Hero / landmark library
Skyline-defining structures and rare monumental accents.

Existing:
- Bastion — active HERO line;
- GateStreetRiseRock MV1 — visual landmark fragment only;
- TowerWallRock — defensive support/skyline;
- selected ruined architecture fragments.

Hero assets must not become mandatory circulation.

### E. District-state dressing
Reusable visual state layers used to show progression without replacing the city:
- reconstruction materials;
- defense cues;
- shelter/population cues;
- food/survival cues;
- stone/masonry recovery cues;
- forge/craft cues;
- civic/prestige cues.

These should be reversible/composable and tied to Bastion progression.

## 2. Correct production order

For a new playable district or Bastion stage:

1. verify approved floor/route/plot topology;
2. assemble Ground Kit skins over it;
3. place reusable Support architecture;
4. place the required Dedicated/Hero building;
5. add district-state dressing/props;
6. validate zoom 19/12/9;
7. only then promote the scene as visually finished.

Do not call a district final while important Ground/Support surfaces are still procedural placeholders.

## 3. Immediate next asset decision

The next **library family to create should be Ground Kit v1**, before the Granero asset.

Reason:
- every future district needs ground/circulation;
- the current visual benchmark explicitly rejects flat board/test-platform reads;
- the existing skeleton already has approved topology, so the ground can be upgraded without gameplay risk;
- one successful Ground Kit improves the current kernel, west quarter and every future district at once;
- it reduces the risk of putting a high-quality Granero on visibly provisional terrain.

After Ground Kit v1 passes:
- produce **Granero** as the next dedicated functional building in gameplay chronology;
- in the same Bastion III block, produce **Residential/Food Support Kit v1** so the Granero district and west quarter can share final-quality support pieces.

## 4. Ground Kit v1 scope

Minimum first production set:
1. **StreetStraight** — modular worn-stone/cobble strip;
2. **StreetBlend/Widening** — irregular widening/plaza transition;
3. **TerraceFloor** — irregular stone/earth terrace skin;
4. **RetainingEdge** — low masonry/rock parcel edge;
5. **GroundSeam** — rock/earth/stone overlap piece.

Optional only if needed after first assembly:
- corner/bend;
- damaged version;
- repaired/prosperous variant;
- drainage/steps detail strip.

Acceptance:
- visually coherent beside Aserradero and Cuartel;
- no rectangular-board read;
- no obvious repeating tile pattern at zoom 12;
- supports L0 and L1;
- no collision/gameplay ownership;
- no visible cracks when overlapped;
- works with neutral-overcast formula;
- reusable in west district and future Granero district.

## 5. What not to generate yet

Do not spend Tripo credits on:
- generic houses before Ground Kit v1 proves the base language;
- decorative buildings with no gameplay/support role;
- another fused district/diorama;
- another circulation module intended to dictate topology.

No paid generation is authorized by this plan.

## 6. Success condition

The construction library is working when a new district can be assembled mainly by combining:
- approved topology;
- Ground Kit;
- Support Kit;
- one or two dedicated/hero buildings;
- progression dressing;

without rebuilding materials, terrain language or generic architecture from scratch.

## 7. Ground Kit v1 implementation status — 2026-09-29

Implemented in `Unity/Assets/Eldoria/Scripts/Presentation/ValoriaGroundKit.cs` and assembled into the real Valoria kernel in `VisualWorld.cs`.

Current reusable functions:
- StreetStraight;
- StreetBlendWidening;
- TerraceFloor;
- RetainingEdge;
- GroundSeam.

Rules preserved:
- visual-only;
- certified gameplay floors/routes/hotspots remain authoritative underneath;
- no Tripo generation and no paid credit spend;
- validation must be integrated at official cameras before marking production PASS.

Implementation commits:
- `49e8548e92b8383d456b34b193d3cdea4834dbcc` — reusable Ground Kit class;
- `ff34e38c20a920d55cf1e78cc7c9969fd9fee335` — integrated into the real Valoria kernel.

The first Unity slice run for the integration was superseded/cancelled after concurrent `main` work changed only `SliceBoot.cs`; that cancellation is not evidence of a Ground Kit defect. Visual Formula / LookDev evidence for the art commit must decide visual acceptance.
