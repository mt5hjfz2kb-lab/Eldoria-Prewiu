# Eldoria — Canonical Tripo Module Production Pipeline

This document is the canonical operational contract for validating any new Tripo-generated modular asset for Valoria.

## Core rule

The owner performs Tripo manually. Everything after export is automated through the Windows self-hosted Unity runner.

**Owner flow:**

`concept image → Tripo manual generation → export GLB to C:\Users\crist\Downloads`

**Automated flow:**

`request file in repo → GitHub Actions self-hosted runner → safe GLB selection → SHA/size inventory → Blender → ~50K tris → UV0/material sanity → Unity isolated gate → colliders/raycast → 19/12/9/oblique/cardinal captures → artifact → technical + visual verdict`

A chat must **not** ask the owner to upload the GLB manually if it has repository write access and the self-hosted runner is available. Manual upload is fallback only after a demonstrated runner/workflow failure.

## Source of truth

Canonical entry points:

- Request contract: `pipeline/tripo-module-request.json`
- Workflow: `.github/workflows/tripo-module-pipeline.yml`
- Blender processor: `tools/tripo_module_blender.py`
- Unity gate: `Unity/Assets/Eldoria/ArtTests/ImageTo3D/Editor/TripoGenericModuleReview.cs`

Older module-specific workflows remain historical evidence and compatibility references. New module validation must use the canonical pipeline unless a documented technical reason requires otherwise.

## How any chat starts a new module gate

1. Read `AGENTS.md`, `SESSION_HANDOFF.md`, this file, and live `main` HEAD.
2. Confirm the owner has exported the new GLB into runner Downloads.
3. Determine the candidate safely:
   - use exact filename + SHA-256 when already known;
   - otherwise use a narrow `name_regex` and recent time window;
   - include hashes of known previous/invalid variants in `exclude_sha256`.
4. Update `pipeline/tripo-module-request.json` with:
   - `enabled: true`
   - unique `request_id`
   - human-readable `module_label`
   - source selection fields
   - module-specific `visual_acceptance`.
5. Commit that request to `main`. The commit itself triggers the canonical workflow automatically.
6. Follow the resulting GitHub Actions run to completion.
7. Inspect the artifact and the actual captures before issuing a visual PASS/FAIL.
8. Record the result in the relevant specialized doc and `SESSION_HANDOFF.md`.
9. Never infer a new family from a filename/version alone. A family exists only when source + gate + evidence demonstrate a distinct functional asset.

## Safe source selection contract

The runner searches `C:\Users\crist\Downloads` and inventories recent `.glb` files.

Selection must resolve to **exactly one** candidate.

Supported request fields:

- `exact_name`: preferred when known.
- `exact_sha256`: strongest identity check; pair with exact name whenever possible.
- `name_regex`: fallback discovery filter.
- `max_age_hours`: bounds discovery to recent owner exports.
- `exclude_sha256`: rejects old/failed variants even if names are similar.
- `unity_yaw_degrees`: rotates the imported module for the official review cameras without altering source geometry. Default 0. If Tripo exports a different canonical front, use the cardinal diagnostics to choose the correct yaw and rerun before declaring a visual FAIL.

If selection produces zero or multiple candidates, the workflow must fail safely. It must never silently choose an ambiguous file.

## Canonical technical gate

Blender:
- measures raw objects / vertices / triangles / materials / UV / normals / bounds;
- preserves or creates at least one material;
- creates UV0 when absent;
- reduces to target 49,800 triangles;
- accepted range: **49,500–50,000**;
- exports GLB and report.

Unity 6000.3.23f1:
- imports the canonical optimized GLB in an isolated review scene;
- verifies mesh, material, UV0 and normals;
- adds MeshCollider(s);
- verifies positive raycast and empty-space miss;
- generates non-empty evidence at:
  - 19 strategic
  - 12 city
  - 9 detail
  - oblique
  - front diagnostic
  - rear diagnostic
  - left diagnostic
  - right diagnostic
- writes `metrics.json`.

Technical PASS never implies visual or functional PASS.

## Visual gate

Every request must state its own functional acceptance criterion in `visual_acceptance`.

The reviewer must inspect the actual images, not only workflow status or metrics.

Examples:
- TowerWallRock: tower/wall/rock silhouette survives official zooms.
- TerraceStairRock: readable elevation change and terrace/stair mass survives official zooms.
- GateStreetRiseRock: readable chain `lower entry → arch → visible ascent → upper landing`.

If a functional relation is hidden, ambiguous, internal-only, or visible only from a diagnostic angle that is not useful in official game cameras, visual gate is FAIL.

## Owner interaction rule

The owner should only need to:
1. create/generate the asset in Tripo manually;
2. export the GLB into Downloads;
3. tell the chat that export is done.

For direct visual inspection of the currently certified three-family composition, the owner can open Unity and use:

`Eldoria > Art Gate > Micro-Valoria > Rebuild and Open Certified Review`

That command verifies the known owner source files in Downloads, prepares local 50K review copies with the same canonical Blender processor when needed, rebuilds the isolated Micro-Valoria review scene and opens it in the Unity Editor. Local staging is ignored by Git. This is a convenience view only; GitHub Actions remains the certification source.

The chat/agent owns everything after that.

Do **not** ask the owner to:
- rename/move/process the GLB manually unless ambiguity cannot be resolved safely;
- run Blender;
- import the asset into Unity;
- execute local commands;
- upload the GLB to chat merely because the chat itself cannot browse Downloads.

A genuine owner blocker exists only when:
- the Windows self-hosted runner is unavailable;
- repo write/Actions access is unavailable in the current environment;
- Downloads contains ambiguous candidates that cannot be distinguished safely;
- or an external/manual application step truly cannot be automated.

## Protected production surfaces

All module gates are isolated.

Unless explicitly authorized, the pipeline must not modify:
- `Valoria.unity`
- `VisualWorld`
- gameplay
- canonical web runtime.

## Legacy workflows

The following are historical/specialized and must not be treated as the default for future modules:
- `tripo-module-gate.yml`
- `tripo-module3-pipeline.yml`
- `tripo-gate-street-pipeline.yml`
- GateStreet source-inventory workflow.

They may remain for reproducibility of old evidence, but the canonical reusable path is `tripo-module-pipeline.yml`.
