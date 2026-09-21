# Eldoria — SESSION HANDOFF

Updated: 2026-09-21

Permanent rules: `AGENTS.md`. Functional state: `PROJECT_STATE.md`.

## Current working state

- Branch: `main`.
- Active release candidate: **v0.25.0**.
- Gameplay/release commit: `687f2247045f0df49290c647dd44179416966f7b`.
- Canonical source: `v0220/index.html` + `v0220/js/`.
- Generated output: `playtest/`; never edit it directly.
- Public URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/
- Pages certification runs on a coherent push to `main`; `workflow_dispatch` is available only for an intentional rerun.
- Protected recovery baselines remain `baseline/v0.24-certified` and `stable/visual-good-f139968c`.

## Last completed block

v0.25.0 audited the current repository and complete Arc I flow. It fixes the mobile Forge/Granary pointer collision, keeps the first Sawmill action on-screen, delays the Bastion III panel until the required troop recruitment is complete, normalizes important QA selectors and raises key mobile actions to a 44 px touch target.

The external-playtest closeout is now complete: explicit end screen, contextual survey, copyable summary, JSON export, structured local session data and full reset. Documentation and setup were consolidated around one reproducible local gate.

## Verification

- `npm run validate:local`: **PASS**.
- Mobile touch viewport: **390×844**.
- Fresh-save Arc I: **16/16 checkpoints PASS**.
- Automated UX/console observations: **0**.
- Accelerated equivalent economy time: **4,368 s**.
- Final survey and session summary: **PASS**.
- GitHub Pages/published Chromium result: check the workflow attached to the v0.25.0 push before calling the public URL certified.

## Next task

If the v0.25.0 Pages workflow is green, hand the public build to the owner for human playtest. New feedback must follow the block protocol in `AGENTS.md`: reproduce all items, implement one coherent batch, targeted QA, full local gate, one push, published verification, then return the stable build.

## Minimal commands

```bash
npm install
npm run qa:setup
npm run validate:local
```

No known automated progression blocker remains. Honest product limitations are recorded in `PROJECT_STATE.md`.
