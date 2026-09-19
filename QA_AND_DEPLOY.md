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

Active save key: `eldoria-v022-consistent-loop`. It is intentionally retained for compatibility even though runtime version is v0.23.x.

Important IDs already used include:
- `v022-core-loop`
- `building-sawmill`, `building-barracks`, `building-granary`, `building-stoneworks`, `building-keep`
- `building-action-<id>`
- `world-node-<id>`
- `world-action-<id>`

Add stable `data-testid` to every important new action/state. Forge/Hero Hall coverage is incomplete and should be normalized.

## Automated tests
- `qa/e2e-smoke.js`: boot and real sawmill contextual/start/complete phases.
- `qa/e2e-core-flow.js`: loads deterministic states and runs runtime assertions; this is state/render regression, not a playthrough.
- `qa/e2e-all-buildings.js`: verifies contextual actions exist for main buildings; mostly availability, not every action to completion.
- `qa/e2e-all-nodes.js`: verifies world contextual actions and compact dimensions; forest is executed into a gather task, many other nodes are availability checks. Some paths still use DOM click rather than real touch.
- `qa/e2e-real-progression.js`: dedicated Fissure→Lyra real regression under active development at v0.23.14.

## Required full-playthrough target
A future canonical E2E must start from a clean save without jumping progression with `setQA/loadState`, and perform the playable flow through the current end of Arc I. It must assert each state transition, not only DOM presence. Shortened deterministic timers may be introduced in QA mode if they use the same production resolution path.

For every gameplay bug: reproduce with Playwright → fix → prove the exact interaction → run regressions. Prefer Playwright `tap/click` to JS DOM clicks so pointer/pan/overlay bugs are detectable.

## Workflow / deploy
`.github/workflows/pages.yml` runs on pushes to `main`:
1. checkout;
2. copy canonical `v0220/index.html` to `playtest/index.html`;
3. syntax-check inline JS;
4. install Playwright + Chromium;
5. serve local build;
6. run E2E/regression jobs;
7. recovery greps;
8. upload Pages artifact;
9. deploy Pages;
10. run selected Chromium checks against the public Pages URL.

If a pre-deploy test fails, Pages is not deployed. Do not call a commit “published” unless Deploy to GitHub Pages completed. Do not call gameplay “verified” unless the relevant real interaction test completed; green syntax/static checks are insufficient.

## Release procedure
Work on `development/v0.23-clean` → commit surgical change → run/review tests → fast-forward `main` only when appropriate → wait for Actions → inspect each step → if successful, test the deployed URL → update `PROJECT_STATE.md` and `CHANGELOG.md`. Use a cache-busting query when sharing a playable URL.

## Historical tools
`tools/build-r7.mjs`, `tools/postprocess-v020.mjs`, and `tools/restore-v020.py` are historical/recovery tooling. Do not use them casually to regenerate the active game; they can reintroduce old visual/runtime layers.
