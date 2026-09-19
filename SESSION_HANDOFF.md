# Eldoria — SESSION HANDOFF
Updated: 2026-09-19

- Active development branch: `development/v0.23-clean`.
- HEAD before this organization-only documentation block: `f9d57b687b70b973eb381bebe563b15c5a98bec0`; after this block, use GitHub HEAD as authoritative and verify it at session start.
- Runtime: **v0.23.24**.
- Canonical editable game: `v0220/index.html`; active fresh-save traversal: `qa/e2e-full-arc1.js`.
- Last verified point: targeted early + late Arc I tests pass; uninterrupted fresh-save Arc I currently reaches Bastion III/Granary gate. Latest run was recalibrated from 180s to 210s passive economy after arriving with 219/240 wood.
- Work in progress: certify one uninterrupted fresh-save mobile Arc I traversal after independent production-building levels/economy changes.
- Next task: inspect the latest Actions run for `f9d57b...`; if full Arc I fails, fix the exact blocker, rerun; if green, verify deploy/published Chromium before declaring test-ready.
- Open bugs: full fresh-save Arc I not yet green after latest calibration; Bastion VI–X and Stoneworks still have routine modal UX debt; Forge/Hero Hall stable IDs/UX incomplete; survey/export/reset incomplete; special combats remain placeholders; legacy runtime remains embedded.

## Critical rules
1. Repository is the only source of truth; never reconstruct from chat or an old build.
2. Never modify `stable/visual-good-f139968c` / `f139968ccbfdeb3e1d37f58568187374faf6d1f2`.
3. Make surgical changes to `v0220/index.html`; preserve the approved visual baseline.
4. Routine build/upgrade/gather/attack is object-local: action below, timer above; avoid confirmation modals.
5. Cards/relics → Codex; equipment/materials → Chest/inventory.
6. Use stable test IDs and real tap/click paths for important interactions.
7. Fixture QA is not proof of fresh-save reachability.
8. Do not claim gameplay fixed/published until the relevant real QA/deploy verification passes.
9. Update this handoff after every important work block; update PROJECT_STATE/CHANGELOG for milestones.
10. Read extended docs only when the current task requires them.

## Working method
- Primary iteration: local canonical build + Playwright; batch blockers before pushing.
- Failure output: step + state/resources + screenshot where possible.
- Separate regression, uninterrupted fresh-save, and player-like experiential passes.
- GitHub Actions is final certification/deployment, not the normal debugger.

## Minimum verification
- Syntax/workflow: push to `main` and inspect `.github/workflows/pages.yml`.
- Gameplay change: targeted Playwright test first; for progression/release run `node qa/e2e-full-arc1.js` plus workflow regressions.
- Release/test-ready: Pages deploy + published Chromium verification must pass.
- Details when needed: `PROJECT_STATE.md`, `QA_AND_DEPLOY.md`, `DESIGN_DECISIONS.md`, `ELDORIA_CONTINUIDAD.md`, `ELDORIA_BASELINE_RULES.md`, `CHANGELOG.md`.
