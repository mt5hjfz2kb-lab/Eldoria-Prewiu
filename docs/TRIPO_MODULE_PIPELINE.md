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

## Exact-input transport invariant (2026-09-30)

Production staging must **not** depend on a logged-in ChatGPT browser session on the Windows runner.

Canonical sequence:

`approved image in chat → persist exact bytes immediately in repo → validate repo payload → stage from repo_exact_input → visible Tripo cost → stop before Generate`

Rules:
- `chatgpt_exact_input` is deprecated for production `stage_upload` and must fail immediately rather than waiting for browser login.
- The approving chat owns persistence of the exact bytes during the same work block. Do not defer byte recovery to a later chat.
- If exact bytes are not present in `pipeline/exact-inputs/<asset>/`, the request stays disabled and no Windows runner time is consumed.
- Never replace a missing approved image with a visually similar export, screenshot, recompression, regenerated variant, Downloads candidate, or browser-session reconstruction.
- A valid `repo_exact_input` request must provide `upload_base64_glob`, expected SHA-256, expected byte size and filename.
- Run the exact-input validator before enabling staging. Missing or mismatched bytes fail before Tripo in seconds, not after a login wait.

## Canonical exact-input staging from ChatGPT

For an owner-approved image attached in ChatGPT, the preferred pre-spend route is now:

`ChatGPT approved image → exact original bytes → repository base64 exact-input parts → Windows runner reconstruction → SHA-256 + byte-size verification → Tripo Studio stage_upload → staged screenshot + visible credit cost → explicit owner approval → generate`

Contract:

- The chat-side agent must verify the attachment's real format, dimensions, byte count and SHA-256 before publication. Do not re-export, recompress, redraw or transcode it.
- Serialize the unchanged bytes to base64. Store one or more lexically ordered parts under `pipeline/exact-inputs/<asset_name>/part_###.b64`.
- `pipeline/tripo-studio-request.json` points to those parts with `upload_base64_glob`, plus the approved `upload_sha256`, `upload_size_bytes`, `upload_file_name` and `asset_name`.
- For chat-approved bytes, set `input_transport: "repo_exact_input"`. This is fail-fast: missing/invalid repository parts stop before Tripo and **must not** silently search Downloads, visual-match candidates or another upload path.
- The bridge reconstructs repository exact-input parts before considering any manual fallback. It fails before Tripo if base64 decode, SHA-256 or byte size differs.
- `stage_upload` is a zero-spend boundary: `allow_credit_spend=false`, `authorized_credit_cost=0`, no Generate click.
- Staging is only accepted after the UI reaches a stable state: upload reflected in Tripo, expected image dimensions observed, Generate cost visible, no upload/generation in progress. When the staged preview exposes local blob/data bytes, their SHA/size are compared with the source as additional evidence.
- The artifact must retain `tripo-studio-before-upload.png`, `tripo-studio-after-upload.png`, `tripo-studio-probe.json`, `tripo-upload-verification.json`, `tripo-flow-state.json` and `tripo-exact-input-identity.txt` when available.
- Generation is a separate explicitly authorized step tied to the exact staged source SHA and the exact visible credit cost.
- Runner Downloads remains a compatible manual/fallback source route. It is not required when a valid repository exact-input exists.

This route reuses the existing Tripo Studio bridge and `upload_base64_glob`; it is not a parallel pipeline.


### Upload/reception hardening rule

The bridge now treats **file transport** and **Tripo receipt** as separate gates:

1. exact bytes verified on the runner;
2. file input set with bounded retries;
3. Tripo UI must reflect the expected image and settle;
4. visible Generate cost must be readable;
5. `stage_upload` must still report zero spend / no Generate;
6. a resumable `tripo-flow-state.json` records the verified boundary.

A successful `setInputFiles` call alone is **not** a staging PASS.

### Exact-input preflight before Tripo

Before enabling a repo-backed `stage_upload`, run:

`node tools/validate-tripo-exact-input.mjs pipeline/exact-inputs/<asset>/manifest.json`

The validator must PASS the expected SHA-256 and byte count before Tripo is touched. If the repo payload is incomplete, keep the request disabled and do not fall back to Downloads for a ChatGPT-approved exact input.



### Recovery of the owner's existing Tripo browser

The canonical Studio bridge supports opt-in recovery after a refused CDP connection. This avoids requiring the owner at the PC when the dedicated browser has closed.

`browser_recovery` configuration:
- `enabled: true`;
- `executable_path: C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe`;
- `user_data_dir: C:\\Users\\crist\\Eldoria-Edge-Remote`;
- `start_timeout_ms: 30000`.

Recovery is restricted to Windows, `http://127.0.0.1:9222`, an existing Edge executable, and the existing dedicated profile with `Local State` and `Default/Preferences`. These paths correspond to the owner's original setup command; do not guess a different profile, copy credentials, create a substitute profile, or kill an existing browser. The original profile is reopened with the same remote-debugging arguments and Tripo URL. Connection retries are bounded, and `browser_recovery` evidence is included in the probe report. The browser is excluded from Actions child cleanup so it remains available to the owner and future bridge jobs.

This does not authorize generation, solve an expired sign-in/verification challenge, or change the exact-input/credit gates. Recovery runtime validation must be recorded separately from source syntax validation.

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

## Inline recovery and no-stop contract

The canonical pipeline owns safe post-generation recovery. Do not create a second salvage workflow for ordinary multipiece cleanup.

For multipiece requests, optional `multipiece.salvage` runs **inside the same Blender invocation before the same Unity multipiece gate**. Current certified-safe policy:
- `mode: disconnected_residue_only`;
- selected piece indices only;
- remove only disconnected components below the configured absolute/relative triangle threshold;
- never delete the dominant component and never attempt connected spike/protrusion surgery under this policy;
- record the salvage decision in the Blender multipiece report.

This means the normal chain is:
`source → Blender optimize/refine/split → safe inline salvage when configured → Unity gate → artifact → visual verdict → selective promotion`.

A technical substep, commit, queued run, completed run, artifact upload or chat/context boundary is **not** an owner handoff. The executing agent continues through evidence review and every zero-credit/reversible recovery step available. Legitimate stops are limited to explicit credit spend/irreversible authorization, missing owner-only input/access, a genuine external blocker after recovery attempts, or completed verified scope.

Runner-throughput rule: workflow-file maintenance must not itself wake heavy Windows publication/gates. Heavy owner WebGL publication remains request-batched via `pipeline/unity-publish-request.json`; diagnostic/legacy flows remain manual-only. Prefer extending this canonical pipeline over adding another Windows workflow.

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
