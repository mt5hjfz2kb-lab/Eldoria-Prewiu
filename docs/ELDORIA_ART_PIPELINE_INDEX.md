# Eldoria — Art Production Pipeline Index

Status: active organizational map.  
Updated: 2026-09-27.

The purpose of this file is to prevent experimental code from being mistaken for the current production route.

## ACTIVE — use by default

### Tripo module production
- Contract: `docs/TRIPO_MODULE_PIPELINE.md`
- Kit inventory: `docs/VALORIA_MODULE_KIT.md`
- Request: `pipeline/tripo-module-request.json`
- Workflow: `.github/workflows/tripo-module-pipeline.yml`
- Blender: `tools/tripo_module_blender.py`
- Unity gate: `Unity/Assets/Eldoria/ArtTests/ImageTo3D/Editor/TripoGenericModuleReview.cs`

Purpose:
`Tripo export in Downloads → safe source selection → Blender ~50K → Unity isolated gate → evidence → PASS/FAIL`.

### Micro-Valoria composition
- Workflow: `.github/workflows/micro-valoria-gate.yml`
- Unity composition: `Unity/Assets/Eldoria/ArtTests/ImageTo3D/Editor/MicroValoriaReview.cs`
- Owner access: `Unity/Assets/Eldoria/ArtTests/ImageTo3D/Editor/MicroValoriaOwnerReview.cs`
- Current verdict: `docs/VALORIA_MICRO_CITY_THREE_FAMILY_GATE.md`

Purpose:
prove that certified families compose into a coherent district before production Valoria is touched.

## SUPPORT — valid infrastructure, not the primary entry point

- `Unity/Assets/Eldoria/ArtTests/ImageTo3D/BastionSelectionProbe.cs`
- glTFast import dependencies in `Unity/Packages/`
- common SceneSetup / render-pipeline helpers
- GitHub self-hosted runner `DESKTOP-R10PE55`
- Unity 6000.3.23f1
- Blender 5.2.2 LTS on runner

These may be reused by active gates but do not define the production flow by themselves.

## HISTORICAL / DIAGNOSTIC — do not start new work here

The following workflows/scripts remain for evidence and reproducibility of prior experiments. They are **not** the default route for new modules:

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
