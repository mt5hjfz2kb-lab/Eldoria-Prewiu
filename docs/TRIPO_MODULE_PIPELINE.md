# Eldoria — Canonical Tripo Module Production Pipeline

This document is the canonical operational contract for validating any new Tripo-generated modular asset for Valoria.

## Core rule

There is one canonical Valoria module pipeline and two supported source-entry routes.

**Route A — manual Tripo source (compatible fallback/current owner choice):**

`concept image → Tripo manual generation → export GLB to C:\Users\crist\Downloads`

**Route B — remote Tripo CLI source:**

`canonical request → Windows self-hosted runner → Tripo CLI from text / one image / 2–4 multiview images → generated GLB`

**Common automated flow after either route:**

`canonical staging → source SHA/size trace → Blender verification/reduction to ~50K → UV0/material sanity → Unity isolated gate → colliders/raycast → 19/12/9/oblique/cardinal captures → artifact → technical + visual verdict`

Remote CLI generation is an extension of the existing canonical workflow, not a second pipeline.

A chat must **not** ask the owner to upload the GLB manually if it has repository write access and the self-hosted runner is available. Manual upload is fallback only after a demonstrated runner/workflow failure.

## Canonical exact-input staging from ChatGPT

For an owner-approved image attached in ChatGPT, the preferred pre-spend route is now:

`ChatGPT approved image → exact original bytes → repository base64 exact-input parts → Windows runner reconstruction → SHA-256 + byte-size verification → Tripo Studio stage_upload → staged screenshot + visible credit cost → explicit owner approval → generate`

Contract:

- The chat-side agent must verify the attachment's real format, dimensions, byte count and SHA-256 before publication. Do not re-export, recompress, redraw or transcode it.
- Serialize the unchanged bytes to base64. Store one or more lexically ordered parts under `pipeline/exact-inputs/<asset_name>/part_###.b64`.
- `pipeline/tripo-studio-request.json` points to those parts with `upload_base64_glob`, plus the approved `upload_sha256`, `upload_size_bytes`, `upload_file_name` and `asset_name`.
- The bridge reconstructs repository exact-input parts before considering Downloads/Desktop fallback. It fails before Tripo if base64 decode, SHA-256 or byte size differs.
- `stage_upload` is a zero-spend boundary: `allow_credit_spend=false`, `authorized_credit_cost=0`, no Generate click. The artifact must retain `tripo-studio-after-upload.png`, `tripo-studio-probe.json` and `tripo-exact-input-identity.txt`.
- Generation is a separate explicitly authorized step tied to the exact staged source SHA and the exact visible credit cost.
- Runner Downloads remains a compatible manual/fallback source route. It is not required when a valid repository exact-input exists.

This route reuses the existing Tripo Studio bridge and `upload_base64_glob`; it is not a parallel pipeline.

## Source of truth

Canonical entry points:

- Request contract: `pipeline/tripo-module-request.json`
- Workflow: `.github/workflows/tripo-module-pipeline.yml`
- Blender processor: `tools/tripo_module_blender.py`
- Unity gate: `Unity/Assets/Eldoria/ArtTests/ImageTo3D/Editor/TripoGenericModuleReview.cs`

Older module-specific workflows remain historical evidence and compatibility references. New module validation must use the canonical pipeline unless a documented technical reason requires otherwise.

## How any chat starts a new module gate

1. Read `AGENTS.md`, `SESSION_HANDOFF.md`, this file, and live `main` HEAD.
2. Choose the source route:
   - manual export: confirm the owner has exported the new GLB into runner Downloads;
   - remote CLI: declare `source.mode` plus the text/image/multiview input in the canonical request.
3. For manual Downloads sources, determine the candidate safely:
   - use exact filename + SHA-256 when already known;
   - otherwise use a narrow `name_regex` and recent time window;
   - include hashes of known previous/invalid variants in `exclude_sha256`.
4. Update `pipeline/tripo-module-request.json` with:
   - `enabled: true`
   - unique `request_id`
   - human-readable `module_label`
   - `source.mode` and the fields for that mode;
   - module-specific `visual_acceptance`.
5. Commit that request to `main`. The commit itself triggers the canonical workflow automatically.
6. Follow the resulting GitHub Actions run to completion.
7. Inspect the artifact and the actual captures before issuing a visual PASS/FAIL.
8. Record the result in the relevant specialized doc and `SESSION_HANDOFF.md`.
9. Never infer a new family from a filename/version alone. A family exists only when source + gate + evidence demonstrate a distinct functional asset.

## Safe source selection contract

Supported source modes:

- `downloads_glb` (default when `source.mode` is omitted): existing safe Downloads selection by exact name/SHA/regex.
- `tripo_text`: generate remotely from `source.prompt`.
- `tripo_single_image`: generate remotely from `source.input_path`.
- `tripo_multiview`: generate remotely from `source.input_paths` containing 2–4 images.

For remote Tripo modes, paths may be absolute paths already present on the Windows runner or repository-relative paths checked out by Actions.

Remote paid generation has **no implicit model default**. The request must explicitly declare:
- `source.tripo_model` — chosen for the asset role, never inferred by the workflow;
- `credit_authorization.approved = true`;
- `credit_authorization.authorized_credit_cost` — the exact owner-authorized spend for this request;
- for `tripo_single_image`, `source.input_sha256`;
- for `tripo_multiview`, one SHA-256 in `source.input_sha256` for every input path.

H3.1 and P-series are not interchangeable defaults: H3.1 is the current flagship fidelity route; P2 is a low-poly Preview route up to the game-oriented face range. Choose intentionally and retain the same Blender/Unity evidence gates.

Example remote request source:

```json
"credit_authorization": {
  "approved": true,
  "authorized_credit_cost": 40
},
"source": {
  "mode": "tripo_single_image",
  "input_path": "pipeline/art-inputs/example/front.png",
  "input_sha256": "<exact-approved-image-sha256>",
  "tripo_model": "tripo-v3.1",
  "face_limit": 50000
}
```

The numeric example is illustrative only; it is not standing authorization for any generation. The request must be tied to the owner's current explicit approval.

The Tripo CLI result, task id, credits consumed, generated source SHA and source bytes are recorded in the workflow artifact.

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
- uses a request-configurable triangle profile;
- legacy/default validation remains target **49,800**, accepted **49,500–50,000** when no profile is supplied;
- production assets may declare `optimization.target_triangles`, `min_triangles` and `max_triangles` according to asset role and measured mobile needs;
- exports GLB and report.

The old ~50K value is a reproducible validation default, not a universal final-art budget. Do not invent lower budgets solely for tidiness: establish them with official-camera visual comparison and device profiling, then use LOD where it produces a measurable benefit.

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

The owner should not need to sit at the PC for canonical processing.

For the manual source route, the owner only needs to:
1. create/generate the asset in Tripo;
2. export the GLB into Downloads;
3. tell the chat that export is done.

For the remote CLI route, the owner may simply provide/approve the artistic goal and input source. The agent can update the canonical request from chat/mobile and let the Windows self-hosted runner execute Tripo → Blender → Unity without the owner operating the desktop, provided the PC and runner are online and the Tripo API account has sufficient CLI/API credits.

For direct visual inspection of the currently certified three-family composition, the owner can open Unity and use:

`Eldoria > Art Gate > Micro-Valoria > Rebuild and Open Certified Review`

That command verifies the known owner source files in Downloads, prepares local 50K review copies with the same canonical Blender processor when needed, rebuilds the isolated Micro-Valoria review scene and opens it in the Unity Editor. Local staging is ignored by Git. This is a convenience view only; GitHub Actions remains the certification source.

The chat/agent owns everything after that.

Do **not** ask the owner to:
- rename/move/process the GLB manually unless ambiguity cannot be resolved safely;
- run Blender;
- import the asset into Unity;
- execute local commands when the canonical runner can do the same work;
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
