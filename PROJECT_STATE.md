# Eldoria — PROJECT STATE
Updated: 2026-09-20

This file describes **functional project state only**. Operational HEAD/current task belongs in `SESSION_HANDOFF.md`; permanent process belongs in `AGENTS.md`.

## Active product
- Runtime/API milestone: **v0.24.0**.
- Canonical source: `v0220/index.html` + `v0220/js/`.
- Development branch: `main`.
- `v0220` and save/API aliases containing v022/v023 are compatibility identifiers, not competing active versions.
- `playtest/` is generated deployment output, never editable source.
- Last certified stable recovery baseline: `2ef3058da8b78f235dac7b6a1bcd0c0cc0d52435` / `baseline/v0.24-certified`.
- Protected visual recovery: `f139968ccbfdeb3e1d37f58568187374faf6d1f2` / `stable/visual-good-f139968c`.
- Older `v019*`, `v020*`, `v0210` and r7 material are historical/reference only.

## Implemented Arc I scaffold
The active slice supports the Valoria-first progression through Bastion X: rebuilding/production, world gathering, Corrupts, Fissure/Lyra, food/hunting, Cantera, Forge/Aether equipment, Nareth/Maelis, march configuration/trial and final Breach assault. Normal world combat is intentionally light/automatic; later special combats remain scaffolding rather than final combat design.

Stable subsystems are separated under `v0220/js/` for economy/state, timestamp tasks/offline recovery, dialogue/tutorial, UI and gameplay helpers. `runtime-hotfix.js` remains migration/compatibility only.

## Protected product direction
The MVP question is whether the compact loop creates understanding, satisfaction and desire to continue:
**Valoria → need/meaningful choice → world → gather/fight/discover → valuable reward → return → visible growth → new ambition.**

Before scope expansion/Unity migration, the slice still needs to prove: a genuine economic trade-off, meaningful Breach gameplay, a useful relic/card reaching Codex with use/conserve tension, and a truthful simulated larger 4X horizon.

## Known functional gaps / debt
- Bastion VI–X and some construction paths still need final object-local UX/progression-gate polish.
- Forge/Hero Hall need fuller stable IDs and hero stats/skills/equipment UX.
- End-of-test survey, session export/copy and full tester reset are incomplete.
- Special late combats are placeholders for the intended richer semiautomatic layer.
- Economy pacing to Bastion X needs human/player-experience validation even when automated reachability is green.
- Current post-baseline development includes renewable/additional world nodes, hunting respawn, live timers and expanded canonical narrative/dialogue work; these are not a new stable baseline until final publication certification is green.

## Verification state
The last **certified stable** build remains `2ef3058...`, which passed regressions, uninterrupted fresh-save Arc I, Pages deployment and published Chromium verification. Current `main` is ahead of it. Exact current HEAD, blockers and next task are intentionally kept only in `SESSION_HANDOFF.md`.

## Maintenance
Change this file only when implemented functionality, product scope, verified stable baseline, or known functional gaps change. Do not put transient next-task/HEAD information here.
