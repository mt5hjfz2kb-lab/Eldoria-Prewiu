# Eldoria — Art Production Pipeline Index

Status: active organizational map.  
Updated: 2026-09-30.

The purpose of this file is to prevent experimental code from being mistaken for the current production route.

## ACTIVE — use by default

### Integrated production visual wedges v1
- Runtime assembly: `Unity/Assets/Eldoria/Scripts/Presentation/ProductionVisualIntegration.cs`, called by real `VisualWorld`.
- Verified record/cameras/gaps: `docs/ELDORIA_WORK_VISUAL_INTEGRATION_V1.md`.
- Exact before/after evidence: `docs/evidence/work-visual-integration-v1/`.
- Uses existing World/Valoria visual formula gates and the full Unity slice; no parallel art pipeline.
- GroundKit + six StoneKit pieces are now an integrated visual-only construction layer. Final architecture/reference benchmark remains open.


### Master asset-library roadmap
- Canonical inventory: `docs/ELDORIA_ASSET_LIBRARY_ROADMAP_V1.md`

Purpose:
separate what Eldoria already owns from the exact functional families still missing across the World Map 4X and Valoria. This is the default search/generation queue. Integrate current material into visible production wedges before broad new acquisition.


### World Map 4X functional production
- Canonical functional library / acquisition roadmap: `docs/WORLD_MAP_4X_FUNCTIONAL_LIBRARY_V1.md`
- Visual acceptance gate: `docs/WORLD_MAP_VISUAL_BENCHMARK_V1.md`
- Functional gameplay seed: canonical web vertical slice under `v0220/`

Purpose:
keep world-map production centered on a persistent mobile 4X board. Geography is only Tier A background. Production must visibly support resource nodes, beasts/PvE, neutral/hostile installations and POIs, player cities, marches/armies and later alliance/territorial structures. Search/generation work must target missing functional families rather than generic scenery packs. Immediate P0 queue: Player City Kit v1, Resource Node Kit v1, Beast Kit v1, Installation/POI Kit v1 and March Representation Kit v1.


### Visual convergence before production spend
- Contract: `docs/ELDORIA_VISUAL_CONVERGENCE_PIPELINE.md`

Purpose:
prove composition, look-dev and hero-fragment language at the cheapest useful fidelity before paid generation or broad city dressing; classify whether a failure is composition, surface or identity so downstream defects do not trigger unnecessary regeneration.


### Tripo module production
- Contract: `docs/TRIPO_MODULE_PIPELINE.md`
- Kit inventory: `docs/VALORIA_MODULE_KIT.md`
- Request: `pipeline/tripo-module-request.json`
- Workflow: `.github/workflows/tripo-module-pipeline.yml`
- Blender: `tools/tripo_module_blender.py`
- Unity gate: `Unity/Assets/Eldoria/ArtTests/ImageTo3D/Editor/TripoGenericModuleReview.cs`

Purpose:
`Tripo source (manual Downloads export OR remote CLI generation) → canonical staging → Blender ~50K/verification → Unity isolated gate → evidence → PASS/FAIL`.

Remote CLI source modes are handled inside the same canonical workflow; do not create a second parallel art pipeline.

### Micro-Valoria composition
- Workflow: `.github/workflows/micro-valoria-gate.yml`
- Unity composition: `Unity/Assets/Eldoria/ArtTests/ImageTo3D/Editor/MicroValoriaReview.cs`
- Owner access: `Unity/Assets/Eldoria/ArtTests/ImageTo3D/Editor/MicroValoriaOwnerReview.cs`
- Three-family precedent: `docs/VALORIA_MICRO_CITY_THREE_FAMILY_GATE.md`
- Six-family inhabited-district gate: `docs/VALORIA_MICRO_CITY_INHABITED_DISTRICT_GATE.md` (technical PASS / visual FAIL)
- Isolated six-family review: `.github/workflows/micro-valoria-2-gate.yml` + `MicroValoria2Review.cs`

Purpose:
historical/diagnostic composition evidence for certified families. After the 2026-09-28 direction change, Micro-Valoria no longer defines production city topology; use it only to judge whether a specific reusable art fragment composes cleanly.


### Valoria playable district integration
- Current reference: `docs/VALORIA_PLAYABLE_DISTRICT_SKELETON_V1.md`
- Production topology: `Unity/Assets/Eldoria/Scripts/Presentation/VisualWorld.cs`
- Interaction/UI integration: `Unity/Assets/Eldoria/Scripts/Presentation/SlicePresenter.cs`
- PlayMode gate: `Unity/Assets/Eldoria/Tests/PlayMode/SliceSceneSmokeTests.cs`
- Unity workflow: `.github/workflows/unity-slice.yml`

Purpose:
build Valoria bottom-up from functional layers (terrain -> L0 -> circulation -> vertical transition -> L1 -> plots/supports -> buildings -> art) and validate it from the fixed official camera with real gameplay clicks.

This is now the **production-direction reference for city topology**. The six certified Tripo families remain reusable art/reference material; they must not be treated as a required topology graph.

### First production district v1
- Record: `docs/VALORIA_FIRST_PRODUCTION_DISTRICT_V1.md`
- Visual Formula gate: run **36541695034**, artifact **11021096851**
- District gate: run **36543056731**, artifact **11021850361**
- Full Unity slice: run **36541695042**
- Production Resources: `Unity/Assets/Eldoria/Resources/Valoria/Rescued/`

Purpose:
the first production-scale proof that certified historical geometry can be rescued into real `VisualWorld` without becoming topology. Dedicated Aserradero/Cuartel remain primary buildings; `ResidentialTerraceRock` and `RockTerrainSeamFiller` are real production visual instances; gameplay/colliders/hotspots remain independent. `TerraceStairRock` is deliberately deferred from this district after a real fit test.

Measured production scene baseline: **460,236 triangles**, with device FPS/memory thresholds still intentionally TBD until representative mobile profiling.


### West rebuilders quarter v1
- Record: `docs/VALORIA_WEST_REBUILDERS_QUARTER_V1.md`
- Visual Formula gate: run **36546445386**, artifact **11022647974**
- LookDev recheck: run **36546445341**
- Art commit: `d84b31811fe044dbc3901f3ecd6edde13a635cc6`

Purpose:
first production expansion into the reserved west Master Envelope. It demonstrates that Valoria can grow beyond the original kernel as a connected inhabited city while keeping gameplay topology independent. The same block closes the bounded Bastion floating-crown Hero Pass.

Measured expanded production baseline: **567,460 triangles / 644 renderers / 80 materials / 15 lights**. These are scene-complexity measurements, not mobile budgets. Physical-device profiling remains required before any visual-quality reduction.


### Reusable Valoria construction library v1
- Plan: `docs/VALORIA_LIBRARY_PRODUCTION_PLAN_V1.md`
- **Ground Kit v1** is certified and integrated; the six StoneKit pieces now extend it in the verified Work wedge.
- **Granero BIII** is already dedicated production art. Remaining future dedicated gaps: Cantera, Forja, Hospital.
- **Stone Architecture Kit v1 generation/gate is closed: TECH PASS / overall VISUAL KIT FAIL.** Canonical closeout: `docs/STONE_ARCHITECTURE_KIT_V1_FINAL_GATE.md`. A separate zero-credit strict per-piece gate selectively promoted only **RockToWallTransition** to `Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/`; all other generated pieces remain cleanup/reject inventory. After this closed block, the next new acquisition priority is **Player City Kit v1**, then **Beast Kit v1**.
- **Terrain & Terrace Kit v1 is closed: isolated technical PARTIAL, West Rebuilders pilot VISUAL FAIL.** Canonical closeout: `docs/TERRAIN_TERRACE_KIT_V1_FINAL_GATE.md`. The exact approved input was generated once; only `SteppedRockTerrace` and `BroadRockPlatform` remain as source-traceable production inventory. The two trial placements were removed after same-camera 19/12/9/mobile comparison failed to demonstrate a credible mountain-city leap. Final Valoria instance count: zero.

Purpose:
make new districts an assembly problem rather than a one-off art problem. Ground/Support modules are visual skins over approved topology; they never dictate circulation or gameplay floors.

## HISTORICAL ASSET RESCUE — reuse before regeneration

- Registry: `pipeline/historical-module-rescue.json`
- Inventory workflow: `.github/workflows/historical-module-rescue-inventory.yml`
- Surface profile: `pipeline/valoria-surface-v1.json`

Certified historical families may be re-evaluated under the current Surface v1 / official-camera formula without spending credits. Rescue does **not** erase documented geometry/interface failures: a surface pass may improve coherence, materials and integration, but it cannot certify traversal or topology that previously failed physically. Prefer reuse in an already-approved gameplay topology before requesting a new paid generation.

## SUPPORT — valid infrastructure, not the primary entry point

- `Unity/Assets/Eldoria/ArtTests/ImageTo3D/BastionSelectionProbe.cs`
- glTFast import dependencies in `Unity/Packages/`
- common SceneSetup / render-pipeline helpers
- GitHub self-hosted runner `DESKTOP-R10PE55`
- Unity 6000.3.23f1
- Blender 5.2.2 LTS on runner

These may be reused by active gates but do not define the production flow by themselves.

## HISTORICAL / DIAGNOSTIC — do not start new work here

The following workflows/scripts remain for evidence and reproducibility of prior experiments. They are **not** the default route for new modules. Historical/diagnostic workflows are now **manual-only** and prefixed `[LEGACY]` where appropriate so they cannot compete with production work or be mistaken for the active route:

- `.github/workflows/tripo-module-gate.yml`
- `.github/workflows/tripo-module3-pipeline.yml`
- `.github/workflows/tripo-gate-street-pipeline.yml`
- `.github/workflows/gate-street-source-inventory.yml`
- `.github/workflows/tripo-appearance-v3-compare.yml`
- `.github/workflows/tripo-appearance-v4-gate.yml`
- `.github/workflows/tripo-embedded-png-modules-test.yml`
- `.github/workflows/tripo-texture-import-debug.yml`

Historical art-test families include:
- full-Bastion image-to-3D trials;
- layered illustration / 2.5D trial;
- TowerWallRock V2/V3/V4/V5 appearance experiments;
- GateStreetRiseRock V1/V2/V3 failed whole-piece iterations.

Do not delete them solely for tidiness while they are still referenced by certified docs. Treat them as archived evidence.

## PRODUCTION SURFACES — protected

These are not art-test sandboxes:
- `Valoria.unity`
- `VisualWorld`
- gameplay
- canonical web runtime

No art-gate experiment may edit them unless the owner explicitly promotes a validated result into production.

## Decision rule for future chats

When continuing Valoria art development:

1. Read `AGENTS.md`.
2. Read current `SESSION_HANDOFF.md`.
3. Read this index.
4. Read `docs/VALORIA_MODULE_KIT.md`.
5. If validating a new Tripo module, use the ACTIVE canonical pipeline only.
6. Use Micro-Valoria only when an explicit diagnostic/reproduction task requires it; production city topology stays in the real playable district/VisualWorld path.
7. Consult historical workflows only to reproduce or audit prior evidence.

If a chat starts creating another per-module Blender script, Unity capturer or workflow without a demonstrated requirement that the generic path cannot satisfy, stop and extend the canonical pipeline instead.

## Toolchain orchestration v2 — experimental zero-spend controller
- Contract: `docs/ELDORIA_TOOLCHAIN_AUTOMATION_V2.md`
- Request: `pipeline/art-production-request.json`
- Capability registry: `pipeline/toolchain-capabilities.json`
- Planner: `tools/plan-art-production.mjs`
- Lightweight workflow: `.github/workflows/art-production-plan.yml`

Purpose:
route visual work by defect type before waking heavy tooling. Composition/surface problems stay in Unity/Blender zero-spend paths; Tripo is permitted only for a proven geometry gap and retains the existing explicit credit gate. The registry distinguishes tool capability from Eldoria-certified automation so future chats do not pretend an unverified feature is already operational.
