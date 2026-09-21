# Eldoria — PROJECT STATE
Updated: 2026-09-21

This file describes **functional project state only**. Operational HEAD/current task belongs in `SESSION_HANDOFF.md`; permanent process belongs in `AGENTS.md`.

## Active product
- Runtime/API milestone: **v0.25.1**.
- Canonical source: `v0220/index.html` + `v0220/js/`.
- Development branch: `main`.
- `v0220` and save/API aliases containing v022/v023 are compatibility identifiers, not competing active versions.
- `playtest/` is generated deployment output, never editable source.
- Current certified playable baseline: `b9d69605c650e1b54f2cff6dfc8514c27add9691` (v0.25.1 hotfix candidate, certification run 793 green).
- Protected visual recovery: `f139968ccbfdeb3e1d37f58568187374faf6d1f2` / `stable/visual-good-f139968c`.
- Older `v019*`, `v020*`, `v0210` and r7 material are historical/reference only.

## Implemented Arc I scaffold
The active slice supports the Valoria-first progression through Bastion X: rebuilding/production, world gathering, Corrupts, Fissure/Lyra, food/hunting, Cantera, Forge/Aether equipment, Nareth/Maelis, march configuration/trial and final Breach assault. The playtest now ends explicitly with an Arc I completion screen, survey, copyable summary, JSON session export and complete local reset. Normal world combat is intentionally light/automatic; later special combats remain scaffolding rather than final combat design.

Stable subsystems are separated under `v0220/js/` for economy/state, timestamp tasks/offline recovery, dialogue/tutorial, UI and gameplay helpers. `runtime-hotfix.js` remains migration/compatibility only.

## Protected product direction
The MVP question is whether the compact loop creates understanding, satisfaction and desire to continue:
**Valoria → need/meaningful choice → world → gather/fight/discover → valuable reward → return → visible growth → new ambition.**

Before scope expansion/Unity migration, the slice still needs to prove: a genuine economic trade-off, meaningful Breach gameplay, a useful relic/card reaching Codex with use/conserve tension, and a truthful simulated larger 4X horizon.

## Known functional gaps / debt
- Bastion VI–X still need human pacing validation beyond automated reachability.
- Hero Hall needs deeper hero stats/skills/equipment UX; this is intentionally not expanded in the stabilization milestone.
- Special late combats are placeholders for the intended richer semiautomatic layer.
- Economy pacing to Bastion X needs human/player-experience validation even when automated reachability is green.
- The current Códice introduces the relic mystery, but the full use/conserve card economy and Duelo de Reliquias remain deliberately outside this stabilized Arc I slice.
- Renewable/additional world nodes, hunting respawn, live timers and expanded canonical narrative/dialogue work remain implemented.

## Verification state
v0.25.1 correction block passed full local-equivalent certification, uninterrupted fresh-save Arc I, deployment and published Chromium verification in run 793. Exact current repository HEAD and certification result belong in `SESSION_HANDOFF.md`.

## Maintenance
Change this file only when implemented functionality, product scope, verified stable baseline, or known functional gaps change. Do not put transient next-task/HEAD information here.
