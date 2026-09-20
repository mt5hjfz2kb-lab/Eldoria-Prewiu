# Eldoria — SESSION HANDOFF
Updated: 2026-09-20

This file contains transient operational state only. Permanent rules: `AGENTS.md`. Functional state: `PROJECT_STATE.md`.

## Current working state
- Branch: `main`.
- Active runtime milestone: **v0.24.0**; do not label v0.24.1 until a later candidate is fully certified.
- Canonical editable source: `v0220/index.html` + `v0220/js/`.
- Generated deployment output: `playtest/`; never edit it as source.
- Last certified stable baseline: `2ef3058da8b78f235dac7b6a1bcd0c0cc0d52435` / `baseline/v0.24-certified`.
- Protected visual recovery: `f139968ccbfdeb3e1d37f58568187374faf6d1f2` / `stable/visual-good-f139968c`.

## Last completed work block
Repository/workflow coherence audit completed. Canonical hierarchy is now:
`AGENTS.md` → `SESSION_HANDOFF.md` → `PROJECT_STATE.md` → specialized docs on demand.
Redundant legacy handoff/status pointer files were removed. README/baseline/design/continuity/QA docs were aligned so old branch/version/protocol claims cannot compete with the active state. Historical version directories remain recovery-only.

The development protocol is now local-first: targeted iteration → `npm run validate:local` → one coherent commit/push → manual Pages certification only for a playable/release candidate. The Pages workflow no longer repeats separate copies of the full regression/fresh-save matrix after the canonical validation command; published verification is intentionally smaller.

## Current gameplay candidate status
Post-baseline main contains the recent world/narrative/regression fixes (hunting visibility/respawn, live gather countdown, Fissure inspection/Lyra flow, additional renewable nodes and dialogue work). The previous publication run for pre-audit commit `be138290...` passed all pre-deploy/local-equivalent QA and deployed Pages, but its published verification failed on stale test expectations/intermittent public interaction checks. Therefore **the last certified stable baseline remains `2ef3058...`** and current main must not yet be called v0.24.1/stable.

## Next task
Resume stabilization of the current gameplay candidate using the new local-first protocol: reconcile the published-test expectations with the canonical dialogue/node behavior, run `npm run validate:local` until green, then perform one manual Pages certification. If the final published build is green, consolidate the milestone as v0.24.1 and update this handoff + `PROJECT_STATE.md` + `CHANGELOG.md`.

## Open operational blockers
- Published `e2e-real-progression.js` still expected the obsolete `aldric-state-dialogue` selector after dialogue unification.
- One published all-nodes attempt observed a zero-size meat contextual action; verify/reproduce locally before changing gameplay.
- Do not expand gameplay while these stabilization checks remain open.

## Minimal commands
- Iteration: run only the relevant `qa/e2e-*.js` test.
- Pre-push gameplay gate: `npm run validate:local`.
- Final release: manually dispatch `.github/workflows/pages.yml` once, only after local green.

Do not reread historical directories/docs during normal startup. Read specialized documentation only when the next task actually needs it.
