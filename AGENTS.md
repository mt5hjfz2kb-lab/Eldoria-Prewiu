# Eldoria — Agent entry point

The repository is the source of truth. Chat history is disposable.

## Source-of-truth hierarchy
1. **AGENTS.md** — permanent working rules and protocol.
2. **SESSION_HANDOFF.md** — current branch/version, current operational state, blockers and next task. Verify live `main` HEAD at session start.
3. **PROJECT_STATE.md** — current functional product state.
4. Specialized docs — read only when needed: `DESIGN_DECISIONS.md`, `QA_AND_DEPLOY.md`, `ELDORIA_CONTINUIDAD.md`, `ELDORIA_BASELINE_RULES.md`. `CHANGELOG.md` is history only.

If documents disagree, this hierarchy wins. Reconcile stale lower-level docs; never reconstruct active code from history.

Unity migration execution: start at `UNITY_MIGRATION_PLAN.md`, then `UNITY_CORE_CONTRACT.md`, `UNITY_ARCHITECTURE.md`, `UNITY_VERTICAL_SLICE.md` and `Unity/README.md`. A separate Unity source project exists at `Unity/`, fixed to Editor 6000.3.23f1; its first executable/player build has **not** been validated until a Unity Editor and license run tests and build. It does not replace the current web source, certification or frozen tester rules. Do not call source preflight a Unity test or claim screenshots/build without opening it.

## Fast start
1. Read this file.
2. Read `SESSION_HANDOFF.md`.
3. Read `PROJECT_STATE.md`.
4. Read `pipeline/active-workstreams.json` and apply the parallel-chat ownership protocol below.
5. Confirm real branch + HEAD.
6. Before substantive edits, claim one non-conflicting workstream in the registry using the current file SHA.
7. Work from the canonical source and inspect only task-relevant code/tests unless a wider audit is explicitly requested.

## Parallel-chat workstream ownership
- Multiple chats/agents may work on Eldoria concurrently, but **one coherent workstream has exactly one active owner at a time**. The canonical live registry is `pipeline/active-workstreams.json`; protocol details live in `docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md`.
- Before substantive edits, workflow dispatches, Tripo staging/generation, Unity visual production, promotion, or documentation that changes project state, an agent must fetch the current registry from `main` and claim a workstream. Read-only investigation may proceed unclaimed only when it cannot mutate repo state or consume shared runner/credits.
- A claim must declare a stable `id`, human-readable `title`, `owner`, `scope`, `resources`, `claimed_at`, `base_main_sha`, and `status=active`. Use the current registry blob SHA for the write. A stale-SHA failure is a concurrency signal: re-read the registry and reassess; never blindly retry an overlapping claim.
- Do not claim a workstream whose scope or exclusive resources overlap an existing active claim. Treat shared production surfaces such as the same canonical files, the same Valoria district/asset family, the Windows self-hosted runner for heavy jobs, Tripo credit spend, or the same promotion target as conflicts unless the existing owner explicitly narrows/releases them in the registry.
- Distinct workstreams may consume already-promoted/certified outputs from one another, but must not silently expand into another active owner's scope. If new findings reveal overlap, stop that overlapping sub-part, record the dependency, and continue only the non-conflicting scope.
- Before updating `main` after substantial work, re-read the registry and live `main` HEAD. Rebase/reconcile conceptually against newer canonical state rather than overwriting another chat's result.
- On completion, blocker, or deliberate handoff, update the registry immediately: release exclusive resources, set `status` to `completed`, `blocked`, or `handoff`, record the resulting commit/run/artifact when relevant, and move the entry from `active` to `history` when complete. Do not leave stale active claims.
- The registry is coordination state, not product truth. `AGENTS.md`, `SESSION_HANDOFF.md`, `PROJECT_STATE.md`, certified evidence, and live code remain authoritative for project/product state.
- Never solve a conflict by creating a parallel replacement pipeline or duplicate asset family. Resolve ownership first, then reuse/promote the canonical result.

## Active line and versions
- Development branch: `main` only.
- Canonical editable runtime: `v0220/index.html` + `v0220/js/`.
- `v0220` is a compatibility directory name, not the product version.
- Active development version: **v0.32.0**.
- Generated development build: `playtest/`; never edit it as source.
- Frozen external tester snapshot: **Eldoria Closed Playtest T1 / 0.26.5-test.2** at `/tester-v0265/`; never use it as a development baseline.
- Protected recovery baselines remain `baseline/v0.24-certified` and `stable/visual-good-f139968c`.

## Permanent versioning policy
- A **minor milestone** `v0.X` increases only when a relevant functional block or new playable milestone has been consolidated and validated.
- Corrections, bug fixes, visual adjustments and balance changes inside the same milestone use **patch versions** `v0.X.Y`.
- Do not increment versions for individual development commits or arbitrary intermediate states.
- Do not wait for the owner to request a version bump. When starting a substantial block, determine the next target version as part of planning; keep the current active version during implementation and promote to the target version only after the block is integrated and validated.
- When a milestone or patch version changes, synchronize at minimum `PROJECT_STATE.md`, `CHANGELOG.md`, `package.json`, `package-lock.json`, `SESSION_HANDOFF.md`, `README.md`, runtime version metadata/API and any version-sensitive QA assertion.
- Historical headings/test filenames may retain the version in which a subsystem was introduced; do not rename them merely because the active version advanced.
- Version promotion is release metadata, not a gameplay change. It must not alter balance, progression or frozen tester output.

## Development speed / migration / visual convergence
- Development workflow optimization and CI profiles: `docs/ELDORIA_DEVELOPMENT_PIPELINE_V2.md`.
- Unity capability migration execution: `docs/UNITY_MIGRATION_ACCELERATION_PLAN.md` in addition to `UNITY_MIGRATION_PLAN.md`.
- Valoria art decisions must follow `docs/ELDORIA_VISUAL_CONVERGENCE_PIPELINE.md`: prove composition/look-dev cheaply before paid/final geometry, classify defects before regenerating, and judge final quality integrated at official cameras.
- Graphics-tool decisions and adoption/defer/reject rationale live in `docs/ELDORIA_GRAPHICS_TOOLCHAIN_AUDIT_2026.md`; do not add a tool merely because it is capable or fashionable.
- Accepted geometry enters `docs/ELDORIA_SURFACE_PIPELINE.md` before any regeneration caused by a visual defect. Diagnose SURFACE separately from COMPOSITION and IDENTITY.
- Valoria production art must inherit `docs/VALORIA_VISUAL_FORMULA_v1.md`; deviations require explicit evidence and documentation. The formula remains provisional until Aserradero + Cuartel + hero-fragment gates are green.
- `.github/workflows/unity-cache-probe.yml` is an isolated measurement workflow only. Do not promote persistent Library reuse into canonical CI until cold/warm results are deterministic and source contamination is excluded.

## Canonical Toolchain Automation v2 entry rule
- Every new Valoria visual/art production block must begin with `docs/ELDORIA_TOOLCHAIN_AUTOMATION_V2.md` and a route represented by `pipeline/art-production-request.json`; run/validate it with `tools/plan-art-production.mjs` (or the lightweight `.github/workflows/art-production-plan.yml`) before waking heavy tooling or requesting/generated geometry.
- The planner is the mandatory routing gate, not a replacement for the execution engines. Its selected route must dispatch into the existing canonical Unity, Blender and Tripo workflows/tools rather than creating a parallel per-asset pipeline.
- Routing is authoritative: **composition -> Unity first; surface/detail -> Unity then Blender when justified; new geometry -> only after a proven geometry gap; animated assets -> separate character route.**
- Tripo must never be the default first step. Any credit-consuming Tripo operation remains blocked until the exact input/reference and visible cost have been shown and the owner gives fresh explicit authorization.
- Tool/capability availability is not production certification. LOD/bake/material/lighting/decals/probes/terrain/Tripo transforms must still pass the relevant technical gate and real 19/12/9/mobile visual validation before promotion.
- Experimental proof requests and proof-only Unity classes are evidence/reproduction infrastructure, not required production dependencies. Do not merge stale experimental branch history merely to recreate already-promoted canonical behavior.
- If a future chat proposes the old blanket `image -> Tripo -> decimate -> Unity` flow without first passing the routing gate, treat that as a process regression and correct it before execution.

## Canonical Valoria modular-art pipeline
- Start Valoria art work at `docs/ELDORIA_ART_PIPELINE_INDEX.md` and `docs/VALORIA_MODULE_KIT.md`; these distinguish active production tooling, support infrastructure, historical experiments and real certified families.
- For every new Tripo module, read and follow `docs/TRIPO_MODULE_PIPELINE.md`.
- The canonical pipeline supports two Tripo entry routes: (1) owner manual generation/export to runner Downloads, and (2) remote Tripo CLI generation on the Windows self-hosted runner from text, one image, or 2–4 multiview images declared in the canonical request. After source creation/selection, the agent owns Blender normalization, Unity gate, evidence review and documentation through the same canonical request/workflow path.
- Do **not** ask the owner to upload a GLB manually when repo write access and the Windows self-hosted runner are available. Manual upload is fallback only after a demonstrated runner/workflow failure.
- New module gates must use `pipeline/tripo-module-request.json` + `.github/workflows/tripo-module-pipeline.yml` unless a documented technical exception requires otherwise.
- **Approved-image persistence rule:** when the owner approves a ChatGPT image for Tripo, the same execution block must persist the unchanged bytes under `pipeline/exact-inputs/<asset_name>/part_*.b64` (or another canonical exact-byte repository representation), record SHA-256 + byte size + real format/dimensions, and point `pipeline/tripo-studio-request.json` at that repository exact-input before staging. Do not leave an approved image only in a chat-local `/mnt/data` path and expect a later chat/runner to recover it. Downloads/visual matching are fallback/manual routes, not the canonical path for chat-approved inputs.
- Before any Tripo action that spends credits, the agent must stop and show the owner the **exact image/reference that will be submitted**, not a contact sheet, moodboard, multi-view presentation board or collage. The owner-facing handoff must include: the final isolated input image, asset name/function, parcel/dimensions if applicable, and visible credit cost. Credit spending requires explicit owner approval after seeing that exact input.
 Old module-specific workflows are historical, not the default.
- Do not create another per-module Blender script, Unity capturer or workflow merely for a new asset. Extend the generic pipeline only when a demonstrated requirement cannot be represented by the current request/configuration.
- A new family exists only when it fills a distinct functional role and has source identity + technical evidence + visual acceptance. Version suffixes of one family never count as new families.
- Owner local inspection of the certified three-family composition is available in Unity at `Eldoria > Art Gate > Micro-Valoria > Rebuild and Open Certified Review`; this convenience view never replaces CI certification.

## Permanent Valoria camera-first composition rule
- Valoria is authored for a **fixed gameplay camera orientation with defined zoom levels, an approved home pose and bounded translation across the city**, not for free 360° exploration. The official orientation/zoom family and approved camera envelope are the visual source of truth for composition.
- Before approving, adapting or requesting any Valoria asset/system, validate it against three constraints first: **official camera**, **player interaction/clickability**, and **spatial progression/circulation**.
- Prioritize silhouette, route readability, occlusion, click targets and hierarchy from the official zooms **across the approved pan envelope**, not only from the home pose. Do not spend production effort making surfaces outside every reachable player camera universally beautiful unless they affect gameplay, shadows, collisions or future explicitly approved camera states.
- It is acceptable to use controlled overlap, burial and hidden joins outside the player-visible camera envelope when they preserve clean geometry, collisions and visual quality from official views. Do not use camera constraints to justify visibly broken geometry or interaction defects.
- City construction should be planned **bottom-up by spatial layers** before proliferating more complete diorama modules: terrain/base → ground-level circulation → vertical connections → upper levels/plots → buildings → props/detail.
- New Valoria assets must justify where they live in that layered plan, what gameplay/urban function they serve, and what interfaces they connect to before generation. If those answers are missing, do not generate the asset.
- A technically valid isolated asset is not sufficient evidence for city readiness. Final acceptance must come from integrated review in the official camera/zoom set.
- Long-term Valoria planning must use `docs/VALORIA_MASTER_PLAN_V1.md`, `docs/VALORIA_PROGRESSION_MAP_V1.md` and `docs/VALORIA_CAMERA_EXPANSION_PLAN_V1.md`. Bastion I–X is the prologue/first arc, not the structural city ceiling; hard-to-reverse terrain, road, district, plot or camera-bound decisions must preserve headroom for the current long-range planning target of roughly Bastion 25–35.
- The certified Playable District Skeleton is the city **kernel**, not the whole production footprint. Before broad final-art dressing, validate a larger-than-one-mobile-viewport master-envelope graybox with bounded panning, reserved expansion districts and progression-aware camera bounds.

## GitHub access stability rule
- Use the connected GitHub app/connector as the canonical repository access path for reads, commits, workflow inspection, artifacts and ordinary repository writes.
- Do **not** open github.com in the Work cloud browser, start a separate GitHub OAuth/login flow, or request a fresh GitHub authorization when connector access is already working.
- Do **not** switch to a separate CLI authentication path merely for convenience. Use CLI/browser GitHub authentication only when a required operation is genuinely unsupported by the connected GitHub tool, and document that exception first.
- After a Work resume/restart/context change, test the existing GitHub connector with one read call before asking the owner for access again. If that call succeeds, reuse the existing connection and continue without another authorization prompt.
- Repeated GitHub approval prompts are an operational defect to avoid, not a normal step of the Eldoria workflow.

## Tripo exact-input ingress rule
- A ChatGPT-generated or user-approved image intended for Tripo is not production-ready until its exact bytes are persisted under `pipeline/exact-inputs/<asset_name>/` with a manifest containing SHA-256 and byte size.
- Persist those bytes in the **same chat/work block where the image is generated or approved**, before dispatching any Windows/Tripo workflow. Use `tools/persist-tripo-exact-input.mjs` when a local/materialized image path is available.
- Canonical Tripo staging uses `input_transport=repo_exact_input`. Do not use a runner browser session to retrieve ChatGPT images, do not wait for ChatGPT login, and do not recursively hunt Downloads/iCloud for an approved chat image.
- If exact bytes are absent, fail immediately before occupying the Windows runner. Never wait minutes hoping a browser/session transfer becomes available.
- Once persisted, all later Tripo retries must reconstruct the input from repo chunks and verify SHA-256 + byte size before upload.
- Generated reference creation and exact-input persistence are one atomic production block: do not hand off between them.

## Unity publication batching rule
- Unity/UI iteration must not publish WebGL on every source commit. Dedicated Unity/UI gates certify iteration; owner publication is a separate coherent-block step.
- The canonical trigger for a Unity WebGL publication is `pipeline/unity-publish-request.json`. Update it only after the current Unity/UI block is coherent and ready for owner review.
- Do not use unrelated Unity source edits to wake Pages. This keeps the single Windows runner available for short Tripo bridge/module work and other active production lanes.
- During parallel chats, prefer short runner-critical jobs (for example Tripo staging) to clear between heavy Unity certification/publication jobs; do not create redundant heavy gates for the same source change.

## Tripo approved-image persistence rule
- When an image is approved for Tripo in ChatGPT, persist its exact original bytes into `pipeline/exact-inputs/<asset_name>/` during the same work block before triggering any Tripo staging workflow.
- Production staging uses `input_transport: repo_exact_input`. Do not depend on a logged-in ChatGPT browser session on the Windows runner.
- `chatgpt_exact_input` is deprecated for production staging and must fail fast; never wait minutes for ChatGPT login as part of normal Tripo transport.
- If exact approved bytes cannot be obtained, keep the request disabled and report the missing bytes as the real blocker. Never substitute regenerated, recompressed, screenshot, Downloads, or visually similar bytes.
- Validate SHA-256 and byte count before enabling `stage_upload`; staging remains zero-spend and stops before Generate.

## Workflow governance rule
- Automatic CI is reserved for canonical production/certification paths. Historical, diagnostic, comparison, inventory, rescue and cache-probe workflows must be `workflow_dispatch` only unless they are explicitly promoted back to production.
- `[LEGACY]` workflows are evidence/repro tools and must never have `push`, `pull_request`, `schedule` or `workflow_run` triggers.
- Do not keep one-off dispatch/cancellation workflows after the incident they solved. Hard-coded workflow-run IDs are forbidden.
- Prefer one canonical generic workflow with request/configuration over per-asset/per-experiment workflows.
- On the single Windows runner, avoid overlapping automatic diagnostic gates when an authoritative production gate already covers the same change. LookDev is opt-in; Visual Formula / Unity slice remain the production evidence paths.
- Any workflow-governance change must pass `tools/check-workflow-governance.mjs`; the lightweight `workflow-governance.yml` enforces this without waking the Windows runner.

## Permanent no-stop execution rule
- A work block must continue automatically through every routine reversible technical step available: edits, commits, CI transitions, polling, artifact retrieval, evidence inspection, zero-credit repair/retry, documentation and safe promotion already authorized by the gate.
- A queued/in-progress workflow, completed commit, uploaded artifact, chat/context boundary or obvious next technical step is **not** a stopping point and must not be handed back to the owner as unfinished work.
- Stop only for: explicit new credit spend/purchase or irreversible authorization; a genuine product/design decision with materially different outcomes; missing owner-only input/access; an external blocker after reasonable recovery attempts; or fully completed and verified scope.
- On resume/restart, reconstruct from live `main` + active workflow state and immediately continue the unfinished block. Never require the owner to type “continúa” to advance routine pipeline work.
- Prefer one canonical chained workflow over multiple sequential Windows workflows. Safe repair/salvage belongs inside the canonical Blender/Unity gate when representable by configuration.
- Runner throughput is part of correctness: workflow-maintenance commits must not wake heavy Windows jobs, publications remain explicitly batched, and redundant diagnostic gates must not compete with the authoritative production gate.

## Permanent working rules
- Make surgical changes to the canonical runtime; never rebuild from an old version.
- **Progression-visibility contract:** the canonical vertical slice defines when player-facing content exists. Buildings, units, districts, world nodes, narrative props and UI entry points may be authored/certified early, but completed art and gameplay interaction must remain hidden/disabled until their canonical unlock. Unity must reconstruct the correct visible/interactable state from PlayerState before the first rendered frame and after scene/state refresh. See `docs/PROGRESSION_VISUAL_CONTRACT.md`.
- **Approved UI reference is binding:** the owner-approved mobile HUD reference is an implementation contract, not optional inspiration. Preserve its player-facing visual grammar, mobile budgets, one-clear-next-action hierarchy and progression correctness. See `docs/UI_REFERENCE_CONTRACT.md`.
- Preserve approved art unless the owner requests visual redesign.
- `runtime-hotfix.js` is migration/compatibility-only; no new gameplay/UI/dialogue belongs there.
- Stable interactions need `data-testid` and real Playwright tap/click coverage.
- Fixture QA proves the targeted state, not uninterrupted player reachability.
- World knowledge/discoveries → Códice. Cards/Reliquias → Relicario. Equipment/materials → Arcón/inventory.
- Keep claims exact: changed ≠ verified; local green ≠ published; deployed ≠ published interaction verified.
- Update `SESSION_HANDOFF.md` after important work blocks. Update `PROJECT_STATE.md` only when product functionality/scope changes.

## QA model — focus, segment, integral
Eldoria has three deliberately separate QA layers. Do not run the entire game after every small iteration unless the change can affect global progression.

### 1. Focused QA — “probar solo el cambio”
Use a deterministic QA Launcher preset or the related Playwright fixture for the exact system changed. The preset must start from a coherent reachable state and preserve relevant dependencies. Typical command: `npm run qa:focus` or the specific regression file.

### 2. Segment QA — “probar un tramo”
Use a segment preset when a change may affect a progression block, e.g. Bastion VI–VIII or IX–X. Typical command: `npm run qa:segment`, plus the related tests.

### 3. Integral QA — fresh save
Use `npm run validate:local` for milestones, progression/economy/sequencing changes, release candidates and any change whose risk crosses multiple systems. It retains the uninterrupted fresh-save Arc I traversal.

Decision rule:
- small/local change → focused test;
- medium/system block change → focused + segment;
- milestone, progression/economy/sequencing, release candidate → `npm run validate:local` + fresh save;
- before declaring an important build stable → integral gate remains mandatory.

## Development-only QA Launcher
- Manual launcher exists only in the generated development build when opened with `?qa=1`.
- QA storage is isolated from the normal save key before runtime boot. Presets and QA fresh saves must never overwrite the player's normal save.
- Browser fixture catalog lives in `v0220/js/qa-fixtures.js`; Playwright should reuse these presets where practical instead of inventing unrelated ad-hoc states.
- Development injection is performed by `tools/build-preview.mjs`; do not add QA launcher UI to the normal canonical runtime or tester snapshot.
- Adding a preset should mean adding one coherent fixture entry, a stable target interaction, and automated coverage when useful.

## Permanent owner-delivery links
After any correction, improvement or new feature with a reasonable isolated QA path, the final delivery to the owner must automatically include the relevant development-build links. The owner must not need to ask for them.

Required delivery surfaces:
- **🎯 Probar esta mejora** — always include when a focused QA state is reasonable. It must point to the development build opened directly in the relevant QA preset/state for the change just delivered.
- **🧩 Probar tramo** — include when the change spans multiple related systems, phases or a progression block. It must point directly to the coherent segment preset/state that starts before the affected block.
- **🎮 Jugar completo** — always include the general development-build URL so the owner can play normally or begin a fresh save when desired.

The agent chooses the preset automatically from the work performed. Examples: Códice → Códice preset; boss → boss preset; Cuartel/recruitment → Cuartel/recruitment-ready preset; Bastion VI–VIII progression → focused target plus VI–VIII segment when applicable. Cross-cutting work may require more than one focused/segment link.

If a new feature has no suitable preset but can reasonably be isolated, creating or adapting a coherent preset/deep-link is part of implementing that feature. Do not force the owner to replay the whole game merely because a focused fixture was missing.

These links are an owner-review convenience and never replace internal QA. Run the appropriate focused, segment or integral validation first. Links must always target the current development build, never the frozen tester build or a historical snapshot. Preserve the normal/fresh-save path and keep using integral QA internally whenever the risk/release protocol requires it.

A delivery is incomplete if a reasonable focused owner test exists but the final response omits **🎯 Probar esta mejora**, or if it omits **🎮 Jugar completo**. **🧩 Probar tramo** is conditional on scope.

## Owner-feedback block protocol
When the owner sends corrections/improvements: reproduce and group the coherent block, implement it without piecemeal handoffs, run the smallest valuable QA while iterating, escalate to segment/integral based on risk, correct regressions found, push a coherent green result, verify the published development build, then return the build. Avoid bug-by-bug status messages.

## Permanent UX acceptance rule
Technical correctness is necessary but not sufficient. Every new or modified player-facing system must also pass a novice-player UX review:

**DISCOVERABILITY → COMPREHENSION → INTERACTION → FEEDBACK → NEXT STEP**

For each system, verify that a player with no prior Eldoria design knowledge:
- discovers that the system or control exists;
- understands what it is and what it is for;
- understands how to interact with it;
- receives clear, legible feedback about the consequence;
- understands the next useful step;
- can distinguish interactive elements from decoration;
- sees the system in the correct responsibility area without unrelated information being mixed in.

If a feature technically works but requires external developer explanation to understand, it is not done and must be treated as a UX defect before release. Integral regressions must include a specific discoverability/comprehension pass in addition to functional reachability.

## Definition of done
- Documentation/process-only: consistency checked + commit/push.
- Small gameplay/UI correction: focused real interaction green; add segment if cross-system risk exists.
- Medium block: focused + related segment green.
- Important/release delivery: `npm run validate:local` green + push + Pages certification + published Chromium verification.

## Continuation / no-premature-stop rule
- The default behavior is to continue through the entire coherent work block without waiting for the owner between routine substeps.
- Do **not** stop merely because a commit was made, one workflow completed, a new workflow started, evidence was generated, a document was updated, a chat/context was resumed, or the next technical step is obvious.
- Do **not** convert intermediate checkpoints into owner handoffs. Intermediate status is informational only; continue automatically.
- Waiting for CI/runner results is not a reason to return control to the owner if the same turn can inspect the result and continue.
- Stop only for: (1) a real product/design choice with materially different outcomes, (2) explicit authorization required for spend/credits/irreversible action, (3) missing access/input only the owner can provide, (4) a genuine technical blocker after reasonable recovery attempts, or (5) the requested block is fully completed and verified.
- If a substep fails, diagnose and attempt the safe recovery path before escalating. Do not ask the owner to choose between routine technical recovery options.
- After a chat/resume/context boundary, reconstruct state from the repository and continue from the real next unfinished step; do not treat the boundary itself as a stopping point.
- When the owner says **hazlo / sigue / adelante / continúa**, execute the largest safe block in the same turn and return only with a verified result, a required product decision, or a genuine blocker.

## Frozen tester isolation
- **Eldoria Closed Playtest T1** is immutable research: `0.26.5-test.2`, frozen integration commit `df618e86be9da399bb827d5e6cebc3f13e55ff97` (re-frozen after the approved final-survey contrast hotfix), path `/tester-v0265/`.
- Do not edit `tester-v0265/` during the test window except for an explicitly versioned critical tester defect.
- Main development must not inherit tester intro/report/survey layers unless explicitly promoted into product.
- Pages must guard tester bytes against drift from the frozen integration commit.
- Feedback is evidence, not an automatic backlog; use `TESTER_FEEDBACK_PROTOCOL.md`.


## Canonical Valoria full-frame convergence rule
- Valoria visual production is now governed by `docs/VALORIA_FULL_FRAME_CONVERGENCE_LOOP_V1.md`.
- The terminal unit of progress is the **integrated official gameplay frame**, not an isolated asset or defect.
- Every visual iteration must inventory all significant benchmark gaps, batch every currently actionable zero-credit correction by dependency, capture matched BEFORE/AFTER at zoom 19/12/9/mobile, and continue until a FULL-FRAME VISUAL PASS or a documented blocker.
- **TECH PASS is safety evidence, not visual completion. LOCAL VISUAL PASS is bounded evidence, not visual completion. Only FULL-FRAME VISUAL PASS may close a Valoria visual iteration.**
- Do not create one workstream per stair/wall/material/prop defect unless a real exclusive-resource dependency requires isolation. Bounded proofs must immediately re-enter the same full-frame iteration.
- After a promoted FULL-FRAME VISUAL PASS, refresh `pipeline/unity-publish-request.json` to the promoted `main` SHA so owner review never relies on a stale WebGL build.
- The machine-readable contract is `pipeline/valoria-full-frame-convergence-v1.json` and is guarded by `tools/validate-valoria-full-frame-convergence.mjs`.
