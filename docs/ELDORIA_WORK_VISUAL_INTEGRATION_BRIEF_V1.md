# Eldoria — Work Visual Integration Brief v1

Status: EXECUTED — see `docs/ELDORIA_WORK_VISUAL_INTEGRATION_V1.md` for final verified evidence and remaining benchmark limits  
Updated: 2026-09-30

## Mission

Use the **real current repository state** to assemble the strongest visual production step possible from assets and systems Eldoria already owns or has already validated.

This is not a research task and not another isolated art experiment.

The goal is to create two visible integrated wedges:
1. **World Map 4X populated strategic wedge**
2. **Valoria production-city wedge**

The result must make progress visibly obvious to the owner and expose remaining asset-library gaps by direct comparison.

## Authority

Order of authority:
1. real `main` repository state;
2. canonical design/visual documents;
3. owner's approved visual reference image supplied with the Work task;
4. historical chat context only when it does not conflict with repo.

Read before editing:
- `AGENTS.md`
- `SESSION_HANDOFF.md`
- `PROJECT_STATE.md`
- `docs/ELDORIA_ASSET_LIBRARY_ROADMAP_V1.md`
- `docs/WORLD_MAP_4X_FUNCTIONAL_LIBRARY_V1.md`
- `docs/WORLD_MAP_VISUAL_BENCHMARK_V1.md`
- `docs/ELDORIA_VISUAL_BENCHMARK.md`
- `docs/VALORIA_LIBRARY_PRODUCTION_PLAN_V1.md`
- `docs/VALORIA_MASTER_PLAN_V1.md`
- `docs/VALORIA_BUILDING_PRODUCTION_INVENTORY_V1.md`
- `docs/VALORIA_FIRST_PRODUCTION_DISTRICT_V1.md`
- `docs/VALORIA_WEST_REBUILDERS_QUARTER_V1.md`
- `docs/VALORIA_MODULE_KIT.md`
- `docs/ELDORIA_ART_PIPELINE_INDEX.md`

## Non-negotiables

- The repo rules over chat.
- Do not rebuild project state from memory.
- Do not buy assets.
- Do not spend Tripo credits.
- Do not touch or replace certified gameplay topology merely to fit art.
- Do not let an asset dictate roads, interaction or game rules.
- Do not regress current I–II gameplay.
- Preserve fixed-direction isometric/mobile camera.
- No free-orbit solution.
- Avoid procedural cube-city / generic graybox final presentation.
- Keep gameplay hotspots/state independent from decorative meshes.
- Reuse/rescue existing material before creating replacements.
- If the supplied visual reference image is unavailable, do not invent or substitute a different reference; use canonical benchmark docs and record the missing reference.

## A. World Map 4X wedge

### Product meaning

The world map is a persistent 4X board.

Geography is background. The scene must visibly read as a strategic territory containing:
- resources;
- beasts/PvE;
- installations/POIs;
- player city representation;
- marches/armies;
- routes;
- environmental geography.

### Reuse first

Evaluate and integrate, where they actually improve the result:
- current Frontier;
- Holotna geographic candidates already validated/importable through existing comparison infrastructure;
- Quaternius ruin/POI candidates;
- NatureStarterKit2 only when useful;
- selected Slavic hard-surface pieces only;
- `WorldRouteKit`;
- `WorldResourceKit`;
- current terrain/corruption language;
- canonical web semantics for wood, stone, food/hunts, wolf, boar, Rift enemies, Fissure/Breach, ruins, march and Valoria/player kingdom.

Do not restore rejected Slavic foliage as the primary forest language.

### Required integrated scene

Create a coherent representative 4X territory containing at minimum:
- continuous geographic terrain;
- mountains/rock barriers;
- forest mass / clearings;
- readable route(s);
- several resource nodes;
- at least two PvE/beast node types;
- at least one hostile Rift/Breach installation;
- at least one neutral ruins/POI;
- one representative player city;
- one representative march/army;
- enough empty strategic space to preserve board readability.

When final art for a P0 family is still missing, create the **smallest deliberate visual placeholder** that represents the functional requirement without pretending it is final. Track it explicitly against the master library roadmap.

### World acceptance

Capture:
- ortho 18;
- ortho 14;
- ortho 10;
- ortho 7;
- 390x844 mobile.

Pass only when:
- player city/resources/PvE/POIs/march are readable over terrain;
- environment does not dominate gameplay;
- territory reads continuous rather than prefab islands;
- no obvious neon/foreign-pack material mismatch;
- no obvious board-game strip roads;
- no catastrophic repetition at normal zoom;
- visual hierarchy approaches the supplied owner reference while preserving Eldoria-specific gameplay.

## B. Valoria wedge

### Reuse first

Use the current production city and library:
- current certified topology/camera;
- Ground Kit v1;
- Aserradero;
- Cuartel;
- Granero;
- current Bastion presentation;
- ResidentialTerraceRock;
- RockTerrainSeamFiller;
- useful rescued historical families where composition warrants them;
- West Rebuilders Quarter precedent;
- six-piece multipiece StoneKit proof as a reusable visual layer where it actually improves streets/edges/steps/courts.

### Required visual improvement

Build the strongest coherent current-Valoria presentation possible without altering gameplay contracts.

Priorities:
1. make Ground Kit + StoneKit read as one intentional urban construction language;
2. reduce remaining flat/provisional surface reads;
3. strengthen inhabited density without fortress-only repetition;
4. use stone edges/steps/courts to integrate buildings with terrain;
5. increase lived-in reconstruction cues using existing props/assets;
6. preserve hierarchy: Bastion HERO, dedicated buildings PRIMARY, housing/support subordinate;
7. make current west/core district visibly closer to the approved visual direction.

Do not invent missing Cantera/Forja/Hospital final art in this task. Reserve/represent their future need only if useful.

### Valoria acceptance

Capture:
- zoom 19;
- zoom 12;
- zoom 9;
- mobile portrait;
- at least one focused before/after angle.

Pass only when:
- current gameplay routes remain intact;
- building clicks/hotspots remain functional;
- city no longer reads as disconnected props/platforms;
- stone/ground/building language feels coherent;
- visual hierarchy survives mobile;
- no regression in Aserradero/Cuartel/Granero presentation.

## C. Before / after and gap output

Produce a concise final evidence package:

### 1. Before / after
For World and Valoria:
- matched camera before;
- matched camera after;
- mobile before/after where possible.

### 2. What was reused
List exact existing assets/systems used.

### 3. What remains visibly missing
Map each visible placeholder/gap to:
`docs/ELDORIA_ASSET_LIBRARY_ROADMAP_V1.md`

Do not create a generic wishlist. Only list gaps exposed by the integrated result.

### 4. Next three highest-leverage library families
Select based on the finished integrated scenes, not theory.

Expected candidates may include:
- Player City Kit v1;
- Resource Node Kit v1;
- Beast Kit v1;
- Stone Architecture Kit v1;
- Residential Support Kit v1;
- Urban Props Kit v1;

but the visual evidence should determine the immediate order.

## D. Engineering / CI discipline

- Work from current `main`; refresh before writes because other chats may advance it.
- Reuse existing workflows and gates.
- Avoid creating parallel redundant pipelines.
- Keep experiments isolated until visually accepted.
- Minimize self-hosted runner contention.
- Do not trigger full WebGL/Unity gates for editor-only changes when existing preflight rules can avoid it.
- Any production integration must finish with the relevant existing functional and visual gates green.
- Record evidence/artifact/run IDs in repo docs.
- Update `PROJECT_STATE.md` / `SESSION_HANDOFF.md` only with verified final state.
- If a candidate visually fails, revert/remove it rather than leaving speculative production clutter.

## E. Desired outcome

This task is successful when the owner can look at:
- one populated World 4X frame;
- one current Valoria frame;

and immediately see a substantial step toward the target game, while the remaining gaps are fewer, concrete and linked to the asset-library roadmap.

The task should **consume existing inventory before requesting new art**.

No report-only completion. Produce the integrated visual result, evidence, tests and final repo state.
