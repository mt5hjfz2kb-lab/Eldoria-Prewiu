# Eldoria — Agent entry point

The repository is the source of truth. Chat history is disposable.

## Source-of-truth hierarchy
1. **AGENTS.md** — permanent working rules and protocol.
2. **SESSION_HANDOFF.md** — current branch/version, current operational state, blockers and next task. Verify live `main` HEAD at session start.
3. **PROJECT_STATE.md** — current functional product state.
4. Specialized docs — read only when needed: `DESIGN_DECISIONS.md`, `QA_AND_DEPLOY.md`, `ELDORIA_CONTINUIDAD.md`, `ELDORIA_BASELINE_RULES.md`. `CHANGELOG.md` is history only.

If documents disagree, this hierarchy wins. Reconcile stale lower-level docs; never reconstruct active code from history.

## Fast start
1. Read this file.
2. Read `SESSION_HANDOFF.md`.
3. Read `PROJECT_STATE.md`.
4. Confirm real branch + HEAD.
5. Work from the canonical source and inspect only task-relevant code/tests unless a wider audit is explicitly requested.

## Active line and versions
- Development branch: `main` only.
- Canonical editable runtime: `v0220/index.html` + `v0220/js/`.
- `v0220` is a compatibility directory name, not the product version.
- Active development version: **v0.27**.
- Generated development build: `playtest/`; never edit it as source.
- Frozen external tester snapshot: **Eldoria Closed Playtest T1 / 0.26.5-test.2** at `/tester-v0265/`; never use it as a development baseline.
- Protected recovery baselines remain `baseline/v0.24-certified` and `stable/visual-good-f139968c`.

## Permanent working rules
- Make surgical changes to the canonical runtime; never rebuild from an old version.
- Preserve approved art unless the owner requests visual redesign.
- `runtime-hotfix.js` is migration/compatibility-only; no new gameplay/UI/dialogue belongs there.
- Stable interactions need `data-testid` and real Playwright tap/click coverage.
- Fixture QA proves the targeted state, not uninterrupted player reachability.
- Cards/relics → Códice. Equipment/materials → Arcón/inventory.
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

## Definition of done
- Documentation/process-only: consistency checked + commit/push.
- Small gameplay/UI correction: focused real interaction green; add segment if cross-system risk exists.
- Medium block: focused + related segment green.
- Important/release delivery: `npm run validate:local` green + push + Pages certification + published Chromium verification.

When the owner says **hazlo / sigue / adelante / continúa**, execute the largest safe block in the same turn and return only with a verified result, a required product decision, or a genuine blocker.

## Frozen tester isolation
- **Eldoria Closed Playtest T1** is immutable research: `0.26.5-test.2`, frozen integration commit `df618e86be9da399bb827d5e6cebc3f13e55ff97` (re-frozen after the approved final-survey contrast hotfix), path `/tester-v0265/`.
- Do not edit `tester-v0265/` during the test window except for an explicitly versioned critical tester defect.
- Main development must not inherit tester intro/report/survey layers unless explicitly promoted into product.
- Pages must guard tester bytes against drift from the frozen integration commit.
- Feedback is evidence, not an automatic backlog; use `TESTER_FEEDBACK_PROTOCOL.md`.
