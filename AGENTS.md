# Eldoria — Agent entry point

The repository is the source of truth. Chat history is disposable.

## Source-of-truth hierarchy
1. **AGENTS.md** — permanent working rules and protocol.
2. **SESSION_HANDOFF.md** — current branch/HEAD, current operational state, blockers and next task.
3. **PROJECT_STATE.md** — current functional product state: implemented systems, verified scope, known gaps.
4. Specialized docs — read only when the task needs them:
   - `DESIGN_DECISIONS.md`: product/design decisions.
   - `QA_AND_DEPLOY.md`: QA/test/deploy details.
   - `ELDORIA_CONTINUIDAD.md`: deeper system/narrative continuity.
   - `ELDORIA_BASELINE_RULES.md`: protected regression/recovery constraints.
   - `CHANGELOG.md`: history only; never current-state authority.
   - `docs/`, old version directories and historical tools: recovery/reference only.

If documents disagree, this hierarchy wins. Reconcile the stale lower-level document as part of the task; never reconstruct active code from history.

## Fast start — every new session
1. Read this file.
2. Read `SESSION_HANDOFF.md`.
3. Read `PROJECT_STATE.md`.
4. Confirm real branch + HEAD.
5. If they match the handoff, work immediately. Read specialized docs only for the concrete task.
6. Inspect only the code/tests related to the task. Do not audit/re-read the whole repository unless explicitly requested.

## Active line and versions
- Development branch: `main`.
- Canonical editable runtime: `v0220/index.html` plus `v0220/js/`.
- `v0220` is a compatibility directory name, **not** the active product version.
- Active runtime/API milestone: **v0.26.3**.
- Generated public build: `playtest/`; never edit it as source.
- Last certified stable recovery baseline: branch `baseline/v0.24-certified`, commit `2ef3058da8b78f235dac7b6a1bcd0c0cc0d52435`.
- Protected visual recovery: `stable/visual-good-f139968c`, commit `f139968ccbfdeb3e1d37f58568187374faf6d1f2`.
- `v019*`, `v020*`, `v0210`, old r7 assets/tools and `docs/R7*` are historical only. Never use them in normal development or deployment.

## Permanent working rules
- Make surgical changes to the canonical runtime; never rebuild from an old version.
- Preserve approved art unless the owner explicitly requests visual redesign.
- `runtime-hotfix.js` is compatibility/migration-only; no new gameplay/UI/dialogue belongs there.
- Stable important interactions need `data-testid` and real Playwright tap/click coverage.
- Fixture/state QA proves a targeted state, not uninterrupted player reachability.
- Routine build/upgrade/gather/attack is object-local: action below the object, timer above; avoid routine confirmation modals.
- Cards/relics → Codex. Equipment/materials → Chest/inventory.
- Keep claims exact: code changed ≠ verified; local green ≠ published; deployed ≠ published interaction verified.
- Update `SESSION_HANDOFF.md` after every important work block. Update `PROJECT_STATE.md` only when functional state/gaps change. Update `CHANGELOG.md` for meaningful milestones/fixes, not every tiny commit.

## Development / QA / release protocol
For ordinary development, work in one session and do not use GitHub Actions as the debugger:
1. Inspect/reproduce the task.
2. Implement the whole coherent batch locally.
3. Run the smallest relevant targeted test while iterating.
4. Before pushing a gameplay/release candidate, run `npm run validate:local`.
5. If it fails, fix locally and rerun; do not push defect-by-defect.
6. Commit/push the coherent green block once.
7. The push to `main` runs the clean-environment GitHub Pages certification/deployment. `workflow_dispatch` remains available for an intentional rerun.
8. Verify the deployed URL with Chromium before calling it playable/verified.

Every player-reported regression becomes a permanent automated assertion when practical.

### QA tiers
- **During iteration:** relevant targeted test(s) only.
- **Pre-push gameplay gate:** `npm run validate:local` (build + contracts + targeted regression + regression suite + uninterrupted fresh-save Arc I).
- **Final publication:** one coherent green push to `main`; its Pages workflow certifies and publishes that exact commit. Avoid piecemeal pushes.

### Owner-feedback block protocol
When the owner sends corrections or improvements, treat them as one delivery block:
1. Reproduce and group all feasible items before editing.
2. Implement the whole coherent block without asking for intermediate confirmation unless a real product decision is unavoidable.
3. Run targeted real-touch tests while iterating, then the full local gate.
4. Correct every failure found by that gate, including regressions outside the originally reported symptom.
5. Push once, wait for the clean Pages certification, verify the public build, and only then return a stable build for owner testing.

The next owner-facing message should therefore be either a verified stable build or a precise blocker that genuinely requires owner action—not a stream of partial patches.

## Definition of done
- Documentation/process-only change: docs/workflow/package consistency checked, commit/push complete; no gameplay publication required.
- Gameplay change: implementation + targeted real interaction + `npm run validate:local` green + coherent commit/push.
- Playable/release delivery: gameplay definition above + successful Pages certification/deploy + Chromium check of the published build.

When the owner says **hazlo / sigue / adelante / continúa**, execute the largest safe block in the same turn. Do not send bug-by-bug status messages. Return only with a useful verified result, a required design decision, or a genuine tool/access blocker.
