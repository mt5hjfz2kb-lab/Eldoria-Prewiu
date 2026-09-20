# Eldoria — QA, Test Mode and deployment

## Local build
There is no compilation/bundling step for the active game. Canonical source is `v0220/index.html`.

Equivalent local serving flow:
```bash
mkdir -p playtest
cp v0220/index.html playtest/index.html
python3 -m http.server 4173
```
Open `http://127.0.0.1:4173/playtest/?qa=1`.

CI installs `playwright@1.55.0` and Chromium. The canonical mobile emulation used by tests is 390×844, `isMobile:true`, `hasTouch:true`.

## QA/Test Mode
Runtime exposes `window.ELDORIA_V023` (and compatibility alias `ELDORIA_V022`):
- `version`
- `assert()`
- `loadState(name)`
- `qa()`
- `reset()`
- `state()`
- `setQA(patch)`

Known deterministic fixtures: `start`, `sawmill`, `world`, `lyra`, `forge`, `endgame`. They are useful for targeted regression but are **not evidence of uninterrupted reachability**.

Active save key: `eldoria-v022-consistent-loop`. It is intentionally retained for save compatibility even though the active runtime milestone is v0.24.0.

Important IDs already used include:
- `v022-core-loop`
- `building-sawmill`, `building-barracks`, `building-granary`, `building-stoneworks`, `building-keep`
- `building-action-<id>`
- `world-node-<id>`
- `world-action-<id>`

Add stable `data-testid` to every important new action/state. Forge/Hero Hall coverage is incomplete and should be normalized.

## Mandatory development loop
Run `npm install` once per working copy, then use **one command** during active development:
```bash
npm run validate:local
```
It builds the canonical preview, checks JavaScript/contracts, starts the local server, runs the targeted blocker suite, the complete regression suite, and the uninterrupted fresh-save Arc I traversal. A gameplay change is not ready to push/certify until this command is green. When it fails, fix locally and rerun it; do not use GitHub Actions as the debugger.

GitHub Pages certification is deliberately `workflow_dispatch` only. Dispatch it once, after local validation is green, then verify the deployed URL. Every player-reported regression must be added to automated QA so it cannot silently return.

## Automated tests
- `qa/e2e-smoke.js`: boot and real sawmill contextual/start/complete phases.
- `qa/e2e-core-flow.js`: loads deterministic states and runs runtime assertions; this is state/render regression, not a playthrough.
- `qa/e2e-all-buildings.js`: verifies contextual actions exist for main buildings; mostly availability, not every action to completion.
- `qa/e2e-all-nodes.js`: verifies world contextual actions and compact dimensions; forest is executed into a gather task, many other nodes are availability checks. Some paths still use DOM click rather than real touch.
- `qa/e2e-real-progression.js`: dedicated Fissure→Lyra real regression.\n- `qa/e2e-late-progression.js`: real mobile late Arc I traversal using controlled phase setup.\n- `qa/e2e-full-arc1.js`: uninterrupted fresh-save Arc I traversal. It never uses `setQA/loadState` to jump progression; `advanceEconomy(seconds)` only advances the same passive-production path so CI can audit long waits without sleeping in real time.

## Canonical full-playthrough gate
`qa/e2e-full-arc1.js` is the mandatory uninterrupted fresh-save Arc I gate. It must start from a clean save without jumping progression with `setQA/loadState`, perform the playable flow through the current end of Arc I, and assert state transitions rather than only DOM presence. It also covers persistence-sensitive paths such as gathering continuing when the player returns to Valoria. Shortened deterministic timers may be used only when they execute the same production resolution path.

For every gameplay bug: reproduce with Playwright → fix → prove the exact interaction → run regressions. Prefer Playwright `tap/click` to JS DOM clicks so pointer/pan/overlay bugs are detectable.

## Workflow / deploy
`.github/workflows/pages.yml` is manual-only (`workflow_dispatch`). It is a final clean-environment certification/deploy, not a development debugger. It runs the same `npm run validate:local` gate, uploads diagnostics, deploys Pages, then runs a small published-site smoke/behavior check. It intentionally does **not** repeat the entire fresh-save/regression matrix a second time against Pages.

Do not dispatch it for documentation-only work or every small commit. Do not call a build published until Pages deployment succeeds; do not call the public build verified until its Chromium check succeeds.

## Release procedure
Batch a coherent gameplay block locally. Iterate with targeted tests; run `npm run validate:local` once the candidate is coherent. Fix/repeat locally until green, commit/push once, then dispatch one manual Pages certification only when a playable/release build is requested. Documentation/process-only commits require consistency verification and push, not a Pages deployment.

## Architecture guardrail
Core gameplay/dialogue/economy behavior belongs in `v0220/index.html` until modules are extracted deliberately. `runtime-hotfix.js` is compatibility/migration-only and must not accumulate new core behavior. Before adding content, prefer extracting stable subsystems behind the same DOM/test contracts rather than layering another hotfix.

## Historical tools
`tools/build-r7.mjs`, `tools/postprocess-v020.mjs`, and `tools/restore-v020.py` are historical/recovery tooling. Do not use them casually to regenerate the active game; they can reintroduce old visual/runtime layers.
