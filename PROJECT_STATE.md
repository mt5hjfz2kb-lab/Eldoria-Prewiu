# Eldoria — PROJECT STATE
Updated: 2026-09-19

## Current version
- Runtime/API: **v0.23.14**.
- Canonical source: `v0220/index.html`.
- Development branch: `development/v0.23-clean`.
- Public deployment source: the workflow copies `v0220/index.html` to `playtest/index.html`.
- Public URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/
- Protected visual recovery point: `f139968ccbfdeb3e1d37f58568187374faf6d1f2`, branch `stable/visual-good-f139968c`.
- Pre-documentation development HEAD: `feee035322acc58108bed0e41a748cbbf87539cf`.

## Last functional milestone
v0.23 moved routine building/world interaction to object-local contextual controls. Node/building actions are intended to appear below the tapped object and timers above it. The first Fissure interaction is being changed so attacking the Fissure when Lyra is absent triggers the Lyra recruitment event; after recruitment the Fissure can be attacked/defeated normally.

## Currently verified
The standard CI has previously passed boot, sawmill contextual/start/complete, core state fixtures, building action availability, world-node action availability, recovery checks, deploy and published-build Chromium checks. These checks do **not** constitute a complete fresh-save Chapter I playthrough.

The newest dedicated real regression `qa/e2e-real-progression.js` targets Fissure → Lyra recruitment. At the pre-documentation HEAD, workflow run `35456645132` for commit `feee035322acc58108bed0e41a748cbbf87539cf` **completed with failure**. Therefore **Fissure→Lyra and the complete progression are not certified as passing**. This is the exact verification state to resume from after the documentation-only consolidation.

## Open bugs / risks
- Finish and obtain a green real mobile Fissure→Lyra regression, then verify on the deployed URL.
- Build a true uninterrupted fresh-save progression test. Current tests use QA fixtures for substantial portions.
- Verify camp and boss actions through real pointer/touch paths, not merely action-panel presence.
- Timed actions store task metadata but completion callbacks are in-memory; reload during a task can strand progression. Replace with serializable/resumable task resolution.
- Bastion VI–X still use confirmation-modal behavior and need explicit progression gates, not only resource costs.
- Stoneworks construction still uses an old confirmation popup; routine actions should be object-local.
- Resource buildings display Bastion-derived levels/rates but do not yet have true independent upgrade levels capped by Bastion.
- Equipping a replacement item can discard the previously equipped item instead of returning it to inventory.
- Forge and Hero Hall need complete stable test IDs/wrappers; Hero Hall needs fuller hero switching/stats/skills/XP/equipment UX.
- End-of-test survey, session export/copy and full tester reset requirements are incomplete.
- Later special combats are functional progression placeholders, not the intended polished semiautomatic combat.
- Economy from early game to Bastion X has not been proven deadlock/grind-free by an uninterrupted playthrough.
- Dead legacy runtime remains physically inside the monolithic HTML, although disabled. Do not remove blindly.

## Next work
1. Complete the Fissure→Lyra fix and make its real mobile regression green locally and on Pages.
2. Extend QA into a real uninterrupted progression traversal and fix every blocker found.
3. Then address task persistence, Bastion gates/modal removal, true building levels, equipment swap, Hero Hall/test IDs, end-of-test flow and deeper combat.
4. Keep visual baseline protected throughout.

## Maintenance rule
This file is the short operational truth. Update it whenever version, verified traversal, open blocker, next work, canonical path or stable recovery commit changes.
