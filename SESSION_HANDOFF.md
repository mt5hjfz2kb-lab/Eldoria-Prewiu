# Eldoria — SESSION HANDOFF

Updated: 2026-09-21

Permanent rules: `AGENTS.md`. Functional state: `PROJECT_STATE.md`.

## Current working state

- Branch: `main`.
- Active release candidate: **v0.25.0**.
- Current owner-feedback candidate: `443d70161e54fbad928aaae6687bb8c03223d9d0`.
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

## Current owner-feedback block

Implemented the reported early/mid-Arc blockers: canonical Aldric portrait, Corrupt combat dialogue surface, HUD Arcón unlocked from the early strange find, Fissure manuscript stored in inventory, Hero Hall close control, contextual-only Bastion upgrades, correct Sawmill level, building upgrade costs/timers, stronger Power/Ranking explanation, additional 4X horizon cues, and explicit Bastion VI elite visibility. Permanent mobile regression coverage was added. Candidate is awaiting the Pages certification/deployment workflow before owner handoff.

## Next task

Do not hand this candidate to the owner until the workflow for `443d70161e54fbad928aaae6687bb8c03223d9d0` is green and the published Chromium verification passes.

## Minimal commands

```bash
npm install
npm run qa:setup
npm run validate:local
```

No known automated progression blocker remains. Honest product limitations are recorded in `PROJECT_STATE.md`.
