# Eldoria — Changelog

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
