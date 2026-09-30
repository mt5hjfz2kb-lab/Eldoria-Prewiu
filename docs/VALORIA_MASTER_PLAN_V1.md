# Valoria Master Plan v1

Date: 2026-09-28  
Status: **ACTIVE DESIGN BASELINE — STRUCTURE BEFORE FINAL ART**

## Purpose

This plan converts the certified **Valoria Playable District Skeleton v1** into the durable spatial framework for the full city.

The current playable skeleton is **the kernel of Valoria, not the whole city**. Its certified circulation hierarchy is preserved:

**entry → Planta 0 street → physical stair → Planta 1 → Bastion / buildings**

Future growth extends around that kernel. It must not return to the rejected strategy of letting fused Tripo dioramas dictate city topology.

Related source-of-truth documents:
- `docs/VALORIA_PLAYABLE_DISTRICT_SKELETON_V1.md`
- `docs/VALORIA_MASTER_PLAN_AND_PROGRESSION_REQUIREMENTS.md`
- `docs/VALORIA_PROGRESSION_MAP_V1.md`
- `docs/VALORIA_CAMERA_EXPANSION_PLAN_V1.md`

## 1. Core principles

1. **One persistent Valoria, many states.** The player grows one city; the spatial identity remains recognizable across progression.
2. **Bastion I–X is the prologue/first arc, not the city ceiling.** The spatial plan must support the current long-range planning target of roughly Bastion 25–35.
3. **The city is larger than one mobile viewport.** A player explores it through bounded panning while keeping the authored isometric orientation.
4. **Terrain and circulation precede architecture.** Floors, streets, stairs, ramps, terraces and reserved plots are authored before production buildings.
5. **The city dictates assets.** Certified Tripo families are reusable art inventory, not topology.
6. **Growth must be physically visible.** Higher progression occupies more plots, restores ruins, increases density and opens additional districts.
7. **Do not overbuild early Valoria.** Empty, ruined or closed future zones are intentional progression space, not unfinished composition.
8. **A system does not imply a building.** UI/meta systems such as Códice and Relicario consume no physical plot by default. Reserve a building plot only when the gameplay contract actually requires a world-space structure or a later explicit decision adds one.

## 2. Durable spatial hierarchy

### 2.1 Kernel District — certified base
Preserve the certified skeleton as the initial structural nucleus:
- lower approach / gate;
- Planta 0 civic floor;
- Planta 0 main street;
- west lower economic plot;
- east lower military plot;
- dedicated 0→1 staircase;
- Planta 1 landing;
- upper plots;
- Bastion hero plot;
- retaining/support terrain.

This is the reference origin for every later expansion.

### 2.2 West Growth District
Primary long-term role:
- residential growth;
- workshops / secondary economy;
- civilian life;
- service buildings;
- later dense housing terraces.

Design intent:
- visually warmer and more inhabited than the military side;
- several smaller plots rather than one giant building;
- enough lateral width that later growth requires camera movement.

### 2.3 East Military / Production District
Primary long-term role:
- barracks expansion;
- training / military infrastructure;
- storage / logistics;
- later defensive or war-support buildings.

Design intent:
- larger clear footprints than residential plots;
- direct readable connection to the main route;
- avoid foreground masses that hide the Bastion or upper stair.

### 2.4 Upper Civic / Government District
Primary long-term role:
- government;
- ministers / civic representation;
- prestige civic structures;
- late-game administrative buildings.

Design intent:
- lives above or behind the early core without blocking the certified lower route;
- should read as increasing institutional power as Valoria matures;
- can become one of the strongest late-game visual areas after the Bastion.

### 2.5 Defensive Ring
Primary long-term role:
- gates;
- wall sections;
- towers;
- repaired / extended fortifications;
- edge-of-city silhouette.

Design intent:
- starts incomplete or ruined;
- closes and strengthens across progression;
- must never create an unreadable wall in front of clickable inner buildings from the official camera family.

### 2.6 Future Expansion Reservation
Reserve at least one major edge beyond current Arc I needs for systems not yet in the Unity slice.

Candidate uses include:
- large alliance/civic structures if city-local;
- advanced economy;
- post-X special structures;
- port/naval access **only if that design remains approved and the eventual terrain supports it**.

This reservation must exist spatially even if its final gameplay purpose is undecided.

## 3. Growth shape

Valoria should expand **primarily laterally** across the mobile view, with secondary depth and elevation.

Reason:
- left/right drag is natural on mobile;
- lateral growth makes the city feel larger without turning it into a second world map;
- the central Bastion remains a reliable orientation landmark.

The long-term composition should therefore resemble an authored wide settlement:
- central stronghold / upper anchor;
- inhabited wings extending left and right;
- selective upper/lower terraces;
- bounded defensive edge;
- future reserve beyond early-game occupancy.

Exact metre dimensions are **not locked in v1**. They must be derived by a full-envelope graybox test using the certified camera orientation and real mobile aspect ratios.

## 4. Plot strategy

Every production building belongs to a plot class before art is generated.

### Hero plots
For:
- Bastion;
- major government/prestige structures;
- other rare skyline-defining structures.

Rules:
- generous click footprint;
- strong silhouette;
- must remain legible at zoom 19/12/9;
- never hidden behind disposable foreground detail.

### Standard functional plots
For:
- Aserradero;
- Cuartel;
- resource / military / civic buildings.

Rules:
- consistent interaction footprint;
- visual upgrades can grow vertically and modestly laterally;
- maximum upgrade envelope is reserved from the start.

### Small civilian/support plots
For:
- houses;
- workshops;
- decorative services;
- life-density structures.

Rules:
- can appear/disappear as progression dressing;
- should not consume critical gameplay interaction corridors.

### Reserved plots
For future systems.
Rules:
- may begin as ruins, rock, vegetation, scaffolds, closed courtyards or unused terrain;
- must not look like obvious empty developer slots;
- must remain structurally usable later.

## 5. Vertical structure

The certified Planta 0 → Planta 1 relationship remains the first proven layer.

Long-term Valoria may add more elevation bands only when they:
- create meaningful hierarchy;
- have explicit stairs/ramps/bridges;
- remain readable from the camera;
- do not bury lower gameplay;
- preserve clickability.

Do not add a new vertical level just to make the skyline busier.

Preferred hierarchy:
- **L0:** entry, economy, military, busy public circulation;
- **L1:** Bastion core and important upper plots;
- **L2 / high terraces (optional, later):** prestige/civic/monumental structures and skyline accents, introduced only after a graybox proves visibility.

## 6. Building upgrade envelopes

A building is not authored only for its current tier. Each functional plot must reserve the bounding envelope required by its likely late-game version.

Each upgrade family should define:
- base footprint;
- maximum footprint;
- maximum intended height;
- protected click zone;
- required clear space around entrances;
- visual growth axes;
- pieces that may change at each art tier.

This prevents a level-20 building from forcing roads or neighboring plots to move.

## 7. Ruin-to-city language

Early Valoria should communicate recovery:
- broken walls;
- unused terraces;
- damaged masonry;
- sparse habitation;
- scaffolds;
- incomplete roofs;
- subdued civic decoration.

Progression gradually converts those same places:
- repaired streets;
- occupied plots;
- stronger roofs/towers;
- banners and lighting;
- denser props;
- more vegetation management;
- more civilian life;
- completed walls;
- prestige decoration.

The same geography should become recognizable as prosperous rather than being replaced by a different city.

## 8. Art-library relationship

The six certified Tripo families remain valid library material:
- TowerWallRock;
- TerraceStairRock;
- GateStreetRiseRock MV1;
- ResidentialTerraceRock;
- StreetLandingTransition;
- RockTerrainSeamFiller.

They may be:
- used intact where they fit a defined plot;
- harvested for facades, towers, rocks, stairs or hero accents;
- adapted into production prefabs;
- retained as visual references.

They must **not** force road placement, elevation or district boundaries.

## 9. Approval gate for the master layout

Before final-art city production scales up, an isolated Unity graybox must demonstrate:

1. full long-term city envelope larger than one mobile viewport;
2. certified kernel retained;
3. west/east/upper/future districts reserved;
4. camera can reach every active/reserved area without changing authored orientation;
5. Bastion remains the primary orientation anchor;
6. no critical building zone becomes permanently occluded;
7. all major expansion corridors are continuous;
8. level 1–10 can remain compact without exposing ugly empty voids;
9. level 25–35 can occupy the envelope without relocating the original kernel;
10. real interaction remains viable after camera movement.

Until this gate passes, exact outer dimensions and late-game plot sizes are provisional.

## 10. Next production sequence

1. keep the certified playable skeleton;
2. graybox the full master envelope around it;
3. implement/validate bounded camera panning;
4. mark district and plot reservations;
5. preview representative progression states (early, Arc-I complete, mid, late, prestige);
6. only then begin broad production-art replacement;
7. create new Tripo/assets from the needs of approved plots;
8. finish materials/props/lighting;
9. performance/LOD/mobile pass.

This sequence is the production baseline unless a later validated gameplay requirement changes it.
