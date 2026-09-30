# Eldoria — World Map 4X Functional Library v1

Status: ACTIVE DESIGN / PRODUCTION BASELINE  
Updated: 2026-09-30

## Purpose

Lock the world-map definition so future art work does not drift into building a decorative adventure scene.

**Eldoria's world map is a persistent mobile 4X board.**

The environment is only the geographic base. The actual map is the combination of:
- geography;
- economic resource nodes;
- PvE / neutral strategic content;
- player cities and armies;
- alliance / territorial structures;
- readable routes and interactable overlays.

The vertical slice web is the canonical functional seed for this map. Its current forest, quarry, hunt, Rift/Breach, ruins, march and route interactions are not disposable prototypes: they define gameplay families that must scale into the persistent 4X map.

Related sources:
- `docs/WORLD_MAP_VISUAL_BENCHMARK_V1.md`
- `v0220/` canonical web vertical slice
- `PROJECT_STATE.md`
- `SESSION_HANDOFF.md`

## 1. Non-negotiable world-map rule

**The terrain is not the map. The terrain is the background layer of the 4X board.**

Every world-map visual decision must first answer:
1. what 4X gameplay function does this object represent?
2. how important is it at far / normal / near zoom?
3. how does it compete visually with cities, armies, resources and POIs?
4. is the visual mesh independent from gameplay state / hotspot / ownership logic?

Do not measure world-map completion by percentage of terrain filled.

## 2. Canonical 4X layers

### A. Geographic base — Tier A
Purpose:
- create territorial mood and navigation context;
- define barriers and corridors;
- support biome readability without overpowering gameplay nodes.

Families:
- terrain surfaces;
- mountains and cliffs;
- large rock formations;
- forest masses / clearings;
- rivers, coasts and water;
- roads and march corridors;
- corruption / Breach territory.

Current usable inventory:
- current Frontier terrain/presentation;
- Holotna Mountain candidate: strong source for mountain/rock/natural geography under the 4X camera;
- `WorldRouteKit`;
- `WorldResourceKit` roadside rock / quarry compositions;
- selected Slavic hard-surface assets only at low salience.

Known gaps:
- water / river / coast system;
- stronger repeatable vegetation language;
- biome variants beyond the current temperate/mountain frontier.

### B. Economic nodes — Tier B gameplay
Purpose:
visible interactable targets that generate / gather strategic resources.

Canonical functional seed from web:
- wood / forest;
- stone / quarry;
- food / hunting / food sources;
- later iron/mineral and special resources.

Required library families:
- WoodNode: 3–4 visual variants + abundance/depleted state;
- StoneNode: 3–4 variants + abundance/depleted state;
- FoodNode: 3–4 variants + abundance/depleted state;
- IronOreNode: 3–4 variants + abundance/depleted state;
- SpecialResourceNode: corruption/relic/crystal/etc. defined only when gameplay requires it.

Rules:
- must be unmistakable at normal 4X zoom;
- environment may support the node but cannot be the hotspot;
- do not rely only on UI icons to explain type;
- repeated nodes require silhouette variation.

### C. Beasts / PvE entities — Tier B/C gameplay
Canonical seed:
- Lobo ceniciento;
- Jabalí de roca;
- Engendro de la Fisura;
- Acechador de Ceniza;
- Devorador de Éter;
- Heraldo de la Fisura;
- later bosses / event creatures.

Required visual hierarchy:
- common beast;
- stronger beast;
- elite;
- Breach/corruption enemy;
- world boss / event boss.

Rules:
- readable as entities, not scenery;
- scale/silhouette communicates threat tier;
- gameplay marker and mesh are separate;
- enough family variation to avoid clone armies of identical creatures.

### D. Neutral / hostile installations and POIs — Tier C
Canonical seed:
- Fisuras / Breach sites;
- corrupt camps;
- Nareth-style ruins;
- trial/event locations;
- resource installations;
- neutral forts / shrines / towers where gameplay later requires them.

Required basic families:
- small neutral ruin;
- medium ruins/monument;
- corrupted Rift/Fissure;
- enemy camp;
- neutral tower/watchpost;
- shrine/ritual site;
- mine/resource installation;
- event/boss arena landmark.

Existing useful inventory:
- Quaternius ruins for secondary architectural POIs after Eldoria material adaptation;
- selected bridge/ruin fragments from current candidate packs.

### E. Player-city layer — PRIMARY 4X
Purpose:
represent each player's persistent city/kingdom on the shared map.

Required Player City Kit:
- City T1 / early;
- City T2 / developing;
- City T3 / established;
- City T4 / high power;
- City T5 / prestige/late placeholder until progression tiers are locked;
- ruined/protected/teleported visual state only if gameplay requires it.

Rules:
- city silhouette must survive far and normal strategic zoom;
- city level/power should produce visible growth without becoming UI noise;
- cities need variation through banners, walls, district density, color/owner treatment or modular dressing so the map does not read as copy/paste;
- player city mesh never owns account/player state logic;
- Valoria may inspire architectural language, but world-map city representation is a strategic iconographic model, not a miniature full Valoria scene.

### F. March / army layer — PRIMARY 4X
Required families/states:
- own march;
- allied march;
- neutral/NPC march;
- hostile march;
- gathering march / stationed state if gameplay uses it.

Rules:
- armies sit above terrain dressing in visual priority;
- direction and movement must read at normal zoom;
- do not require full troop simulation at map scale;
- hero/troop identity can be abstracted to standards, formations, mounts, silhouettes or small representative groups.

### G. Alliance / territorial layer — future PRIMARY 4X
Reserve visual language for:
- alliance fortress/headquarters;
- alliance structures;
- territory markers/borders;
- teleports;
- shared resource installations;
- strategic towers/flags.

Do not build these before gameplay contracts are locked, but leave them in the library roadmap.

## 3. Current world-map coverage assessment

This is a functional-library assessment, not a terrain-fill percentage.

Approximate current readiness:
- geographic base for a first mountain/forest region: **60–70%**;
- routes / quarry / rock-support language: **65–80%**;
- ruins / secondary POI source material: **50–65%**;
- economic node library: **25–35%**;
- beasts / PvE visual families: **15–25%**;
- neutral/hostile installation library: **20–30%**;
- player-city library: **0–10%**;
- march/army representation: **0–10%**;
- alliance/territorial structures: **0–5%**;
- water/coast library: **10–20%**.

Overall functional 4X world-map asset-library readiness:
**approximately 25–35%**.

Do not replace this number with a higher terrain-only estimate.

## 4. Missing library — production/search queue

### P0 — required to make the map read as 4X
1. **Player City Kit v1**
   - 3 initial strategic city tiers minimum;
   - one neutral/friendly owner-variation mechanism;
   - official camera proof 18/14/10/7 + mobile.

2. **Resource Node Kit v1**
   - wood;
   - stone;
   - food;
   - iron/mineral;
   - abundance/depletion variants.

3. **Beast Kit v1**
   - wolf;
   - boar;
   - one Rift common enemy;
   - one elite silhouette;
   - boss language placeholder.

4. **Installation / POI Kit v1**
   - Fissure/Breach;
   - corrupt camp;
   - neutral ruin;
   - tower/watchpost;
   - resource installation.

5. **March Representation Kit v1**
   - own/allied/hostile visual states;
   - readable direction;
   - no expensive individual-troop requirement.

### P1 — required to make regions convincing
6. **World Vegetation Kit v1**
   - 3–4 tree silhouettes;
   - 2–3 shrubs/undergrowth;
   - dead tree/stump;
   - cluster prefabs;
   - repetition-resistant distribution.

7. **World Water Kit v1**
   - river;
   - shore;
   - ford;
   - small waterfall;
   - water/rock and water/earth transitions.

8. **World Ruin Detail Kit v1**
   - small broken wall;
   - arch;
   - broken stairs;
   - platform;
   - pedestal/statue;
   - debris.

9. **World Settlement Support Kit v1**
   - tent/camp;
   - palisade;
   - gate;
   - small hut/store;
   - fire;
   - crates/sacks/cart.

### P2 — later diversity and scale
10. biome variants;
11. alliance structure kit;
12. special event/boss arenas;
13. advanced resource installations;
14. region-specific civilization/POI dressing.

## 5. Search / acquisition rule

When searching free packs or generating new art, do not search for “a world-map pack”.

Search for missing functional families from the queue above.

Every candidate must be classified before import as:
- geographic;
- economic node;
- beast;
- installation/POI;
- player city;
- march/army;
- alliance;
- dressing.

A candidate that does not fill a missing function is not a production priority even if visually attractive.

## 6. Implementation rule

Existing material should now be assembled into a real 4X visual wedge before broad new asset acquisition.

The next integrated world-map production proof should contain at minimum:
- continuous geographic base;
- readable route network;
- several resource nodes;
- at least two beast/PvE node types;
- at least one Rift/hostile installation;
- at least one ruins/neutral POI;
- one representative player-city placeholder/kit candidate;
- one march representation;
- mobile 4X camera proof.

This wedge should reuse current Frontier, Holotna/Quaternius candidates where appropriate, existing WorldRouteKit/WorldResourceKit, and canonical web gameplay semantics.

## 7. Relationship to the web vertical slice

The web vertical slice is a functional reference, not a final art layout.

Preserve its meaning:
- forest = wood/resource interaction;
- quarry/vein = stone/resource interaction;
- wolf/boar = hunt entities;
- Rift enemies = PvE threat family;
- Fissure/Breach = hostile strategic installation;
- Nareth/ruins = POI/narrative location;
- march = army preparation/deployment;
- Valoria = the player's kingdom/city origin.

Scale these concepts into the persistent 4X map instead of replacing them with decorative equivalents.

## 8. Visual direction

All world-map work must follow `WORLD_MAP_VISUAL_BENCHMARK_V1`:
- fixed isometric/top-down direction;
- strategic readability first;
- dark epic-fantasy Eldoria palette;
- terrain as coherent masses, not isolated prefabs;
- POIs/cities/resources/armies dominate over dressing;
- avoid neon foliage, low-poly toy-board appearance and obvious asset-pack seams;
- validate at 18/14/10/7 and 390x844 mobile.

## 9. Definition of progress

A visible increase in terrain density is not automatically progress.

World-map production progress is measured by:
- number of canonical 4X functions visually represented;
- readability at gameplay camera;
- reusability and variation;
- integration with gameplay semantics;
- reduced dependence on placeholders;
- ability to assemble a populated strategic territory without bespoke work for every node.
