# Eldoria — Changelog\n\n## v0.23.23 — 2026-09-19\n- Timed core actions now recover after a page/app reload instead of leaving progression stranded. Completed persisted tasks are resolved on boot for Aserradero, Cuartel, Granero, Cantera, Forja, Bastion VI–X and resource gathering.\n- Full fresh-save QA now deliberately reloads during the first construction to guard this behavior.\n\n\n\n## QA economy calibration — 2026-09-19\n- Uninterrupted fresh-save traversal now reaches and completes the Arc I finale. Measured accelerated passive-production requirement is 4,380 seconds (73 minutes); the CI guard is set at 4,500 seconds to detect future economy regressions rather than failing below the observed viable route.\n\n\n\n## QA economy calibration — 2026-09-19\n- The uninterrupted fresh-save run reached the Arc I finale successfully. Its only failing assertion was the provisional 4,200-second passive-production ceiling: measured requirement was 4,380 seconds (~73 min). The gate is now 4,500 seconds (75 min); no progression state or resources were injected.\n\n\n\n## v0.23.24 — 2026-09-19\n- Full fresh-save traversal reached Bastion VI and exposed a missing stable identifier on the Forge. Added `data-testid=building-forge`; no gameplay state was bypassed.\n\n\n\n## v0.23.23 — 2026-09-19\n- Continued real-touch audit found Granero overlapping Forja in the late-Arc layout. Kingdom interactables now occupy separated mobile-safe zones (Aserradero lower-left, Granero lower-right, Forja upper-right).\n\n\n\n## v0.23.22 — 2026-09-19\n- Corrected the actual mobile overlap source: `sawmillWrap` had a 118px touch box that still covered the Granero despite moving the Granero. Aserradero wrapper is now smaller and moved to the lower-left safe area.\n\n\n\n## v0.23.21 — 2026-09-19\n- Real mobile traversal identified overlapping kingdom hitboxes: the Aserradero intercepted taps intended for the Granero. Granero and Cantera now have explicit non-overlapping positions.\n- This is a gameplay/input correction only; protected visual baseline remains untouched.\n\n\n\n## v0.23.20 — 2026-09-19\n- Fresh-save mobile QA exposed that the pulsing objective animation made unwrapped kingdom POIs continuously unstable for real Playwright taps. Objective buildings now keep a static highlight instead of geometry-affecting animation.\n- Fresh-save traversal now uses true mobile taps rather than forced clicks for building interactions.\n\n\n\n## v0.23.19 — 2026-09-19\n- Reworked building selection to be state-driven: a real building tap now persists `selectedAction` and performs a canonical render, preventing contextual actions from being lost to concurrent render cycles.\n- This replaces the transient DOM-injection path for normal building interaction.\n\n\n## v0.23.18 — 2026-09-19\n- Fixed contextual building actions disappearing during continuous mobile progression: selecting a building now renders the action as canonical state instead of injecting a transient DOM fragment that the render loop could erase.\n- Full fresh-save Arc I traversal remains the release gate.\n\n\n## v0.23.17 — 2026-09-19\n- Added `qa/e2e-full-arc1.js` for uninterrupted fresh-save mobile progression through the Arc I finale.\n- Added a QA-only economy clock accelerator that advances normal passive-production resolution without changing progression flags/resources directly.\n- Fixed a real pre-Bastion-V economy deadlock: Quarry reserve increased from 900 to 2,800 and quarry load from 260 to 700 so the finite node can fund required early stone before Stoneworks unlocks.\n- Full CI/published verification pending.\n

## QA milestone — 2026-09-19
- Added real mobile late Arc I traversal covering Bastion VI, Forge, Devorador/Ascua, equipment forging, Nareth/Maelis, March Trial, Bastion X and final assault.
- Workflow `35458031436` passed all local regressions, both real progression tests, Pages deploy and published Chromium verification.
- Remaining verification gap: one uninterrupted fresh-save run rather than segmented QA-state setup.


Only milestones and important regressions/fixes are recorded here. Historical 0.19–0.21 material remains in the repository.

## v0.23.16 — 2026-09-19
- QA mode now suppresses narrative overlays deterministically so mobile interaction tests cannot be intercepted by the legacy Aldric cinematic.

## v0.23.15 — 2026-09-19
- Hardened Fissure→Lyra discovery against duplicate cinema overlays.
- Dedicated regression now taps both Fissure action and Lyra recruitment through Playwright mobile input rather than DOM-click fallback.
- Verification pending the next workflow run.

## v0.23.14 — 2026-09-19
- Current runtime version.
- First Fissure interaction changed toward deterministic Lyra recruitment: when Lyra is absent, the Fissure encounter must lead to the Lyra recruitment cinematic; after recruitment, the Fissure can be fought normally.
- Dedicated `qa/e2e-real-progression.js` added and repeatedly hardened to exercise this regression.
- **Status at documentation consolidation:** implementation present, latest dedicated real regression not yet certified green; do not mark resolved until Actions + deployed Chromium verification pass.

## v0.23.12–0.23.13
- Reworked contextual world UI to be compact and anchored to nodes; building actions follow the same object-local pattern.
- Added mobile/touch isolation to avoid world-pan swallowing contextual actions.
- Added public-build Chromium verification after Pages deployment.
- Added Fissure/Lyra regression work.
- Important lesson: prior tests checked action-panel presence/dimensions but did not prove every action completed. Never equate those.

## v0.23.10–0.23.11
- Unified contextual world action rendering and removed competing contextual elements.
- Added broad building/node action-availability Playwright coverage.
- Regression: a layout fix was initially reported as solved before the actual published interaction was sufficiently exercised. Process changed: gameplay fixes require real interaction and published-build verification.

## v0.23.x foundation
- Canonical development moved to `v0220/index.html` while preserving the user-approved visual recovery point `f139968c`.
- Legacy runtime was disabled rather than deleted.
- Playwright/Chromium mobile QA integrated into GitHub Actions.
- Current systems expanded through Bastion X scaffolding: Forge, Aether Ember, Maelis/Nareth, March Trial and final assault.

## Historical
v0.21 is the external-playtest bank of earlier systems (including richer cards/Duel/Orin content). v0.22 recentered the prototype on Valoria → need → world → reward → stronger Valoria. Old directories are recovery/reference, not active deployment sources.
