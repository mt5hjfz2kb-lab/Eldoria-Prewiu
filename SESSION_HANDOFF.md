# Eldoria — SESSION HANDOFF
Updated: 2026-09-21

This file contains transient operational state only. Permanent rules: `AGENTS.md`. Functional state: `PROJECT_STATE.md`.

## Current working state
- Branch: `main`.
- Active certified milestone: **v0.24.1**.
- Certified gameplay/deployment HEAD: `445661a318195315a6a8f56ade3bfa6642e6af58`.
- Current repository HEAD is documentation/workflow-only ahead of that certified gameplay commit.
- Canonical editable source: `v0220/index.html` + `v0220/js/`.
- Generated deployment output: `playtest/`; never edit it as source.
- Pages workflow: manual-only (`workflow_dispatch`).
- Protected visual recovery: `f139968ccbfdeb3e1d37f58568187374faf6d1f2` / `stable/visual-good-f139968c`.

## Last completed work block
v0.24.1 stabilization and certification completed. Mobile world-node QA now tolerates canonical runtime rerenders while still using real Playwright touch. The uninterrupted fresh-save Arc I route uses independent renewable quarry nodes for Bastion VIII–X rather than depending on one depleted node respawning inside the same sweep.

Certification run **749** passed the complete `npm run validate:local` gate, uploaded the Pages artifact, deployed successfully, and passed the published-build Chromium verification.

## Current status
**v0.24.1 is the current certified playable build.** No stabilization blocker remains from the v0.24.1 certification cycle.

## Next task
Resume normal product development from v0.24.1. Before changing gameplay, read the relevant specialized design document only for the subsystem being changed. Keep the local-first loop: targeted test → `npm run validate:local` → coherent commit/push → one manual Pages certification for the next playable milestone.

## Minimal commands
- Iteration: relevant `qa/e2e-*.js` test.
- Pre-push gameplay gate: `npm run validate:local`.
- Release candidate: manually dispatch `.github/workflows/pages.yml` once after local green.

Do not reread historical directories/docs during normal startup.
