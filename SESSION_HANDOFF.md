# Eldoria — SESSION HANDOFF
Updated: 2026-09-21

This file contains transient operational state only. Permanent rules: `AGENTS.md`. Functional state: `PROJECT_STATE.md`.

## Current working state
- Repository HEAD entering the next executable QA block: `7b2b59fc9e10e682cafea7f3a66e1c04d5262483` (manual-only Pages workflow restored after the mobile-node QA fix).
- Branch: `main`.
- Active runtime milestone: **v0.24.0**; do not label v0.24.1 until a later candidate is fully certified.
- Canonical editable source: `v0220/index.html` + `v0220/js/`.
- Generated deployment output: `playtest/`; never edit it as source.
- Last certified stable baseline: `2ef3058da8b78f235dac7b6a1bcd0c0cc0d52435` / `baseline/v0.24-certified`.
- Protected visual recovery: `f139968ccbfdeb3e1d37f58568187374faf6d1f2` / `stable/visual-good-f139968c`.

## Last completed work block
Workflow consistency restored. The accidental `push: main` trigger was removed from `.github/workflows/pages.yml`; Pages certification is again manual-only as required by `AGENTS.md` and `QA_AND_DEPLOY.md`.

The known mobile all-nodes blocker was also patched in `qa/e2e-all-nodes.js`: world nodes are scrolled into the mobile viewport before the real Playwright tap, so offscreen nodes such as `forest2` can be tested as actual touch interactions rather than failing only because they start outside the viewport.

## Current gameplay candidate status
Post-baseline main contains the recent world/narrative/regression fixes (hunting visibility/respawn, live gather countdown, Fissure inspection/Lyra flow, additional renewable nodes and dialogue work). The previous publication run for pre-audit commit `be138290...` passed all pre-deploy/local-equivalent QA and deployed Pages, but its published verification failed on stale test expectations/intermittent public interaction checks. Therefore **the last certified stable baseline remains `2ef3058...`** and current main must not yet be called v0.24.1/stable.

## Next task
In an execution-capable session, start from this exact HEAD and run `npm run validate:local`. Fix every discovered regression in the same technical loop until green. Only then commit/push the coherent candidate and manually dispatch `.github/workflows/pages.yml` once. Inspect the certification run/logs, fix/repeat if needed, and finish only after the published Chromium verification is green. Then consolidate v0.24.1 in runtime/package/docs and record the certified HEAD.

Do not use ordinary pushes to trigger Pages and do not use Actions as the normal debugger.

## Open operational blockers
- No known code blocker should be assumed resolved solely by this handoff; the full local gate must establish the real remaining set.
- This chat session could edit GitHub but did not expose a local checkout/terminal/Playwright runner or a fresh workflow-dispatch operation. That is an execution-environment limitation, not a repository/gameplay blocker.
- Do not expand gameplay while stabilization/certification remains open.

## Minimal commands
- Iteration: run only the relevant `qa/e2e-*.js` test.
- Pre-push gameplay gate: `npm run validate:local`.
- Final release: manually dispatch `.github/workflows/pages.yml` once, only after local green.

Do not reread historical directories/docs during normal startup. Read specialized documentation only when the next task actually needs it.
