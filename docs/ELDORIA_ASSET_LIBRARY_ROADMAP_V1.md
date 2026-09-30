# Eldoria — Asset Library Roadmap v1

Status: ACTIVE MASTER LIBRARY ROADMAP  
Updated: 2026-09-30

## Purpose

Provide one canonical inventory for what Eldoria already owns, what is reusable now, and what must still be searched, rescued, generated or authored.

This document covers two different construction problems:
1. **World Map 4X** — persistent shared strategic board.
2. **Valoria** — persistent player city with progression and bounded mobile pan.

Never mix the two asset roles.

Related canonical sources:
- `docs/WORLD_MAP_4X_FUNCTIONAL_LIBRARY_V1.md`
- `docs/WORLD_MAP_VISUAL_BENCHMARK_V1.md`
- `docs/VALORIA_LIBRARY_PRODUCTION_PLAN_V1.md`
- `docs/VALORIA_BUILDING_PRODUCTION_INVENTORY_V1.md`
- `docs/VALORIA_MODULE_KIT.md`

---

# A. WORLD MAP 4X LIBRARY

## A1. What we have now

### Geographic / environmental
- Current Frontier terrain and presentation.
- Holotna Mountain: selected mountain, rock and two tree variants are now persisted and used in the real runtime wedge; official-camera evidence is in `docs/ELDORIA_WORK_VISUAL_INTEGRATION_V1.md`.
- NatureStarterKit2: available, lower-priority baseline.
- Selected Slavic hard-surface props: conditional only.
- Slavic foliage: REJECTED as primary forest language.
- Existing `WorldRouteKit`.
- Existing `WorldResourceKit` quarry/rock support compositions.
- Quaternius ruins: useful secondary POI architecture after Eldoria material adaptation.
- Existing route/corruption/frontier presentation from Unity + web semantics.

### Functional gameplay seed already defined in web
- wood / forest resource interaction;
- stone / quarry resource interaction;
- food / hunting;
- wolf;
- rock boar;
- Rift Spawn;
- Ash Stalker;
- Aether Devourer;
- Rift Herald / boss language;
- Fissure / Breach;
- ruins / Nareth;
- march / expedition;
- Valoria as player kingdom origin.

## A2. What is still missing

### P0 — makes the map actually read as a 4X
1. **Player City Kit v1**
   - at least 3 strategic city tiers;
   - readable silhouette progression;
   - owner/banner variation;
   - optional protected/ruined/teleport state only if gameplay requires it.

2. **Resource Node Kit v1**
   - wood;
   - stone;
   - food;
   - iron/mineral;
   - later special resource;
   - abundance/depleted states;
   - 3–4 shape variants per common resource where practical.

3. **Beast Kit v1**
   - wolf;
   - boar;
   - Rift common enemy;
   - elite enemy;
   - boss visual language.

4. **Installation / POI Kit v1**
   - Fissure/Breach;
   - corrupt camp;
   - neutral ruin;
   - watchtower;
   - resource installation/mine;
   - shrine/event landmark.

5. **March Representation Kit v1**
   - own march;
   - allied march;
   - hostile march;
   - readable heading/motion;
   - gathering/stationed state only if gameplay needs it.

### P1 — makes regions visually complete
6. **World Vegetation Kit v1**
   - 3–4 tree silhouettes;
   - shrubs;
   - undergrowth;
   - dead tree/stump;
   - repeatable clusters.

7. **World Water Kit v1**
   - river;
   - bank;
   - ford;
   - small waterfall;
   - shoreline;
   - water/rock and water/earth transitions.

8. **World Ruin Detail Kit v1**
   - broken wall;
   - small arch;
   - stairs;
   - platform;
   - pedestal;
   - debris.

9. **World Settlement Support Kit v1**
   - tent;
   - hut/store;
   - palisade;
   - gate;
   - campfire;
   - crates/sacks/cart.

### P2 — scale and late-game diversity
10. biome variants;
11. alliance structures;
12. territory markers;
13. teleport structures;
14. special event arenas;
15. advanced resource installations;
16. civilization/region-specific POI dressing.

## A3. Current readiness

Do not report terrain completion as map completion.

Approximate:
- first mountain/forest geographic layer: 60–70%;
- full functional 4X asset library: **25–35%**.

---

# B. VALORIA CITY LIBRARY

## B1. What we have now

### Production dedicated buildings
- Aserradero — PASS.
- Cuartel — PASS.
- Granero BIII — PASS.
- Bastion — active HERO line / existing production presentation with later refinement headroom.

### Certified/reusable ground
Ground Kit v1:
- StreetStraight;
- StreetBlendWidening;
- TerraceFloor;
- RetainingEdge;
- GroundSeam.

### Certified/rescued architecture / terrain support
- TowerWallRock;
- TerraceStairRock;
- GateStreetRiseRock MV1 — landmark/support only, not certified traversal;
- ResidentialTerraceRock;
- StreetLandingTransition;
- RockTerrainSeamFiller.

### Multipiece StoneKit proof
One exact Tripo generation proved a six-piece sheet can be converted into six independent reusable GLBs:
- broad stone/losa family;
- irregular surface;
- long edge/curb-like family;
- corner/edge family;
- step/low-block family;
- stone block family.

Final proof:
- 6 independent pieces;
- 49,791 triangles total;
- UV/normals/materials preserved;
- isolated Unity multipiece gate PASS;
- this proves the **production method**, not automatic production promotion of every piece.

### Existing inhabited/dressing precedent
- First Production District v1.
- West Rebuilders Quarter v1.
- housing/work-court/route/dressing patterns in VisualWorld.
- selected Slavic hard-surface props can be reused after camera validation.

## B2. What is still missing

### P0 — city construction multiplier
1. **Stone Architecture Kit v1**
   Best next multipiece-sheet candidate:
   - low straight wall;
   - high wall;
   - interior/exterior corner;
   - arch/opening;
   - pillar;
   - parapet/cap;
   - rock-to-wall transition where sheet capacity allows.

   Goal:
   turn the successful multipiece method into a real urban masonry kit.

2. **Residential Support Kit v1**
   - small house;
   - medium/two-volume house;
   - stepped/terraced house;
   - workshop/shed;
   - storage/support structure.

3. **Roof / Facade Variation Kit v1**
   - roof straight;
   - roof corner/end;
   - chimney;
   - door;
   - window;
   - balcony/overhang;
   - awning/sign where appropriate.

4. **Urban Props Kit v1**
   - barrels;
   - crates;
   - sacks;
   - firewood;
   - cart;
   - bench;
   - lantern/post;
   - tools;
   - fences;
   - small merchandise/material piles.

### P1 — gameplay chronology
5. **Cantera dedicated building/family**
6. **Forja dedicated building/family**
7. **Hospital dedicated building/family**

These are the remaining known Arc-I dedicated functional art gaps after Granero.

8. **Craft / Industry Dressing Kit**
   - anvil;
   - furnace/oven;
   - brazier;
   - coal/fuel;
   - racks;
   - cut stone;
   - processed logs;
   - pulley/crane details.

9. **Food / Survival Dressing Kit**
   - grain sacks;
   - baskets;
   - food crates;
   - small market/storage stalls;
   - hay/grain;
   - carts;
   - civilian utility props.

### P2 — city identity / progression
10. repaired vs damaged masonry variants;
11. scaffold/reconstruction kit;
12. banners/standards;
13. civic/prestige props;
14. vegetation/planter/courtyard greenery;
15. defensive-ring modular support;
16. upper-civic/government support architecture;
17. late-game prestige variants.

## B3. What the current StoneKit changes

The multipiece StoneKit does **not** mean the whole city is nearly complete.

It means the most repeated construction layer is becoming tractable.

Combined with Ground Kit v1, current stone assets can already cover much of:
- streets;
- terrace surfaces;
- borders;
- small steps;
- small courts;
- foundation transitions;
- stone clutter/seams.

Approximate readiness:
- ground/circulation/masonry-support layer: **75–85%**;
- total visual city library including architecture/buildings/props: roughly **35–45%**, depending on how aggressively current VisualWorld housing/support patterns are reused.

The next large gain comes from:
**Stone Architecture Kit + Residential Support Kit + Urban Props Kit**, not another isolated hero building.

---

# C. SEARCH / GENERATION PRIORITY

When looking for free assets, using Tripo, or authoring in Unity/Blender, work in this order unless a current gameplay milestone overrides it:

## Immediate
1. Player City Kit v1
2. Resource Node Kit v1
3. Beast Kit v1
4. Installation/POI Kit v1
5. March Representation Kit v1
6. Stone Architecture Kit v1
7. Residential Support Kit v1
8. Urban Props Kit v1

## Then
9. World Vegetation Kit v1
10. World Water Kit v1
11. Cantera
12. Forja
13. Hospital
14. Roof/Facade Variation Kit
15. Craft/Industry Dressing Kit
16. World Settlement Support Kit

## Later
17. alliance structures;
18. biome expansion;
19. late-game city prestige;
20. region/civilization-specific world POIs.

---

# D. WORK IMPLEMENTATION TARGET

Before spending substantial effort acquiring more assets, use the inventory already available to build a visible integrated production wedge.

## World-map wedge
Must show, in one coherent 4X composition:
- geographic base;
- mountains/rocks;
- forest mass;
- route network;
- several distinct resource nodes, using current placeholders where final art is unavailable;
- at least two beast/PvE types, placeholders allowed only if clearly tracked as library gaps;
- Rift/hostile installation;
- neutral ruins/POI;
- representative player city placeholder/kit candidate;
- representative march;
- official camera 18/14/10/7 + mobile.

## Valoria wedge
Must use:
- current production city;
- Ground Kit v1;
- production Aserradero/Cuartel/Granero;
- rescued ResidentialTerraceRock / RockTerrainSeamFiller where appropriate;
- the six-piece StoneKit as an isolated reusable construction layer only where visual review supports it;
- current west rebuilders precedent;
- no change to certified gameplay topology/hotspots;
- official camera 19/12/9 + mobile.

## Goal

Create an immediately visible before/after that:
- proves how much of Eldoria can already be assembled;
- exposes the exact remaining library gaps;
- preserves the established dark epic-fantasy direction;
- avoids making new generic procedural/cube art;
- does not let assets dictate gameplay topology;
- gives the owner a concrete visual path of progress.

---

# E. ACCEPTANCE RULE

A new asset family enters this roadmap as “available” only when:
- it fills a named missing function;
- it survives the relevant official camera;
- it has a reuse/variation strategy;
- it does not own gameplay logic unless explicitly designed to;
- it matches or can be adapted to Eldoria’s visual formula;
- its evidence is stored in the repo.

Pretty but functionless assets do not improve library completion.


# F. VERIFIED WORK WEDGE — 2026-09-30

Record and exact images: `docs/ELDORIA_WORK_VISUAL_INTEGRATION_V1.md`; source `616d5f8e1e969e917f885c970562135e45fc2191`. World visual **36675166099**, Valoria visual **36675166037**, full Unity **36675166031** all SUCCESS. Zero purchases/Tripo spend. The final reference benchmark remains OPEN; do not equate these incremental wedges with final asset-family completion.

| Existing inventory now used | Verified production role |
| --- | --- |
| Holotna + selected Quaternius | Mountains/rock/forest geography; neutral ruin and hostile secondary masonry |
| Frontier / WorldRouteKit / WorldResourceKit | Preserve real targets/route semantics, integrate continuous geography and resource access strips |
| GroundKit + all six StoneKit GLBs | Streets, courts, borders, corners, twelve exact visual tread skins, transitions and small bases; no gameplay collision |
| Current Aserradero / Cuartel / Granero / Bastion | Existing functional plots, visibility and dedicated source art preserved |
| ResidentialTerraceRock / RockTerrainSeamFiller / TowerWallRock | Retained civil/seam support plus one decorative recovered flank |
| West Rebuilders + selected hard surface | Roof/work-frontage/stock support; no new dedicated functional buildings |

Visible gaps remain exact: **A2.1** compact city/tier silhouette; **A2.2** food/common node variations and depleted states; **A2.3** wolf/boar/Engendro primitive art; **A2.4** final installation identity/states; **A2.5** capsule/banner march; **B2.1** provisional Bastion/retaining masonry; **B2.2–3** civil bodies/repeated roofs; **B2.4** partial inhabited dressing and symbolic workers. Cantera/Forja/Hospital stay future **B2.5–7** gaps.

The three-family priority remains the governing sequence, but the first generation block is now closed:
1. **Stone Architecture Kit v1 — CLOSED AS GENERATED: TECH PASS / overall VISUAL KIT FAIL.** Strict per-piece review selectively promoted only **RockToWallTransition**; the remaining seven generated groups stay cleanup/reject inventory and do not constitute a production kit.
2. **Player City Kit v1 — NEXT NEW ACQUISITION PRIORITY** once Stone Architecture closeout is accepted.
3. **Beast Kit v1 — NEXT AFTER PLAYER CITY**; visible PvE silhouettes remain provisional.

Current shots make Resource/Installation/March/Residential/Props gaps concrete, but this entry does not authorize a wider acquisition wishlist or paid generation.
