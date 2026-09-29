# Eldoria — Art Production Pipeline Index

Status: active organizational map.  
Updated: 2026-09-29.

The purpose of this file is to prevent experimental code from being mistaken for the current production route.

## ACTIVE — use by default

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

The following workflows/scripts remain for evidence and reproducibility of prior experiments. They are **not** the default route for new modules. The four superseded module/source workflows are now **manual-only** and prefixed `[LEGACY]` in GitHub Actions so they cannot be mistaken for the active route:

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
6. If composing certified modules, use Micro-Valoria.
7. Consult historical workflows only to reproduce or audit prior evidence.

If a chat starts creating another per-module Blender script, Unity capturer or workflow without a demonstrated requirement that the generic path cannot satisfy, stop and extend the canonical pipeline instead.
