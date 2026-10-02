# Eldoria — Asset Library Roadmap v1

Status: ACTIVE MASTER LIBRARY ROADMAP  
Updated: 2026-10-02

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
- **Player City v1 — PRODUCTION / PASS:** `Unity/Assets/Eldoria/Resources/WorldPlayerCity/PlayerCity_v1.glb`; one universal strategic city mesh for v1, with player/alliance/state differences handled outside the mesh.
- Quaternius ruins: useful secondary POI architecture after Eldoria material adaptation.
- Existing route/corruption/frontier presentation from Unity + web semantics.
- **Player City v1 universal strategic city mesh — PRODUCTION / PASS**, persisted at `Unity/Assets/Eldoria/Resources/WorldPlayerCity/PlayerCity_v1.glb`; player/alliance/protection identity remains UI/state-driven.

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
1. **Resource Node Kit v1**
   - wood;
   - stone;
   - food;
   - iron/mineral;
   - later special resource;
   - abundance/depleted states;
   - 3–4 shape variants per common resource where practical.

2. **Beast Kit v1**
   - wolf;
   - boar;
   - Rift common enemy;
   - elite enemy;
   - boss visual language.

3. **Installation / POI Kit v1**
   - Fissure/Breach;
   - corrupt camp;
   - neutral ruin;
   - watchtower;
   - resource installation/mine;
   - shrine/event landmark.

4. **March Representation Kit v1**
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
10. optional Player City visual tiers/skins if later gameplay justifies them;
11. biome variants;
12. alliance structures;
13. territory markers;
14. teleport structures;
15. special event arenas;
16. advanced resource installations;
17. civilization/region-specific POI dressing.

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
- Hero Bastion v1 — PASS / production hero anchor after Asset Library Reprocessing v1.

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
- GateStreetRiseRock MV1 — landmark/support only, not certified traversal; canonical GLB persisted under `HistoricalLandmarks`.
- ResidentialTerraceRock;
- StreetLandingTransition — canonical GLB persisted under `Rescued`;
- RockTerrainSeamFiller.
- Mid-Tier Architecture Kit v1 — 4 production GLBs, now normalized under `Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/`.
- Stone Architecture Kit v1 — 3 production-safe GLBs: CornerWallL / HighStraightWall / RockToWallTransition.

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
1. **Stone Architecture expansion / remaining masonry vocabulary**
   - current production-safe core already exists: CornerWallL / HighStraightWall / RockToWallTransition;
   - remaining need is selective expansion only where a proven city-composition gap requires it;
   - do not regenerate the rejected v1 pieces merely to increase asset count.

2. **Residential Support Kit v1**
   - small house;
   - medium/two-volume house;
   - stepped/terraced house;
   - workshop/shed;
   - storage/support structure.

2. **Roof / Facade Variation Kit v1**
   - roof straight;
   - roof corner/end;
   - chimney;
   - door;
   - window;
   - balcony/overhang;
   - awning/sign where appropriate.

3. **Urban Props Kit v1**
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
**Residential Support Kit + Roof/Facade Variation + Urban Props Kit**, while extending the already-promoted Stone Architecture family only for proven missing masonry roles.

---

# C. SEARCH / GENERATION PRIORITY

When looking for free assets, using Tripo, or authoring in Unity/Blender, work in this order unless a current gameplay milestone overrides it:

## Immediate
1. Resource Node Kit v1
2. Beast Kit v1
3. Installation/POI Kit v1
4. March Representation Kit v1
5. Stone Architecture expansion only when a proven gap remains
6. Residential Support Kit v1
7. Urban Props Kit v1

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

Visible world gaps now start after the promoted universal Player City v1: resource-node variations/depleted states, wolf/boar/Engendro art, final installation identity/states, and march representation; **B2.1** provisional Bastion/retaining masonry; **B2.2–3** civil bodies/repeated roofs; **B2.4** partial inhabited dressing and symbolic workers. Cantera/Forja/Hospital stay future **B2.5–7** gaps.

The three-family priority remains the governing sequence, but the first generation block is now closed:
1. **Stone Architecture Kit v1 — CLOSED AS GENERATED: TECH PASS / overall VISUAL KIT FAIL.** Strict per-piece review selectively promoted only **RockToWallTransition**; the remaining seven generated groups stay cleanup/reject inventory and do not constitute a production kit.
2. **Player City Kit v1 — NEXT NEW ACQUISITION PRIORITY** once Stone Architecture closeout is accepted.
3. **Beast Kit v1 — NEXT AFTER PLAYER CITY**; visible PvE silhouettes remain provisional.

Current shots make Resource/Installation/March/Residential/Props gaps concrete, but this entry does not authorize a wider acquisition wishlist or paid generation.

## G. CANONICAL LIBRARY STORAGE — 2026-10-02

Asset Library Canonicalization Pass v1 is complete. Canonical audit: `pipeline/asset-library-canonicalization-audit.json`.

- 26 canonical GLBs inventoried across Valoria + WorldPlayerCity.
- 0 canonical GLBs missing committed Unity `.meta` files.
- TerraceStairRock and StreetLandingTransition were recovered from their exact certified historical artifacts and persisted under `Resources/Valoria/Rescued/`.
- GateStreetRiseRock MV1 was recovered by exact SHA and persisted separately under `Resources/Valoria/HistoricalLandmarks/` to preserve its landmark-only / interface-fail limitation.
- Mid-Tier Architecture Kit Piece01–04 was moved from the generic `Unity/Assets/Resources/` tree into `Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/` with its existing GLB metadata/GUIDs preserved.
- Player City v1 and Granero BIII are existing production assets and must not be listed as missing acquisition targets.
- Historical artifacts remain provenance/recovery evidence, not the only storage location for reusable certified GLBs.


## G. ASSET LIBRARY CANONICALIZATION — 2026-10-02

Canonical audit: `pipeline/asset-library-canonicalization-audit.json`.

- **26 canonical GLBs** are persisted across Valoria + WorldPlayerCity.
- **0 canonical GLBs lack a committed Unity `.meta`**.
- Historical certified `TerraceStairRock`, `StreetLandingTransition` and `GateStreetRiseRock MV1` were recovered by exact SHA from their certified Actions artifacts and are now persisted in `main`.
- Mid-Tier Architecture Kit Piece01–04 moved from the generic `Unity/Assets/Resources/Valoria/` tree into `Unity/Assets/Eldoria/Resources/Valoria/MidTierArchitectureKit_v1/`, preserving the existing Unity metadata/GUIDs and the same `Resources.Load("Valoria/MidTierArchitectureKit_v1/...")` address.
- Production/rescue GLBs now live under the Eldoria-owned Resources tree; rejected historical source pieces remain evidence, not production-library entries.
- Canonicalization changed library persistence/organization only; it did not authorize new geometry, alter visuals, or spend Tripo credits.

Full closeout: `docs/ASSET_LIBRARY_CANONICALIZATION_PASS_V1_RESULT.md`.
