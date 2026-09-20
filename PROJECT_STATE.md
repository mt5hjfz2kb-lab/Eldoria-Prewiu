# Eldoria — PROJECT STATE
Updated: 2026-09-20
Release candidate: v0.24.0 clean playtest milestone

## Current version
- Runtime/API: **v0.24.0**.
- Canonical source: `v0220/index.html`.
- Development branch: `main`.
- Public deployment source: the workflow copies `v0220/index.html` to `playtest/index.html`.
- Public URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/
- Certified functional recovery point: `2ef3058da8b78f235dac7b6a1bcd0c0cc0d52435`, branch `baseline/v0.24-certified` (full fresh-save Arc I + regressions + deployed-site Chromium verification green).
- Protected visual recovery point: `f139968ccbfdeb3e1d37f58568187374faf6d1f2`, branch `stable/visual-good-f139968c`.
- Pre-documentation development HEAD: `feee035322acc58108bed0e41a748cbbf87539cf`.

## Last functional milestone
v0.23 moved routine building/world interaction to object-local contextual controls. Node/building actions are intended to appear below the tapped object and timers above it. The first Fissure interaction is being changed so attacking the Fissure when Lyra is absent triggers the Lyra recruitment event; after recruitment the Fissure can be attacked/defeated normally.

## Currently verified
The v0.24.0 baseline at `2ef3058da8b78f235dac7b6a1bcd0c0cc0d52435` is fully certified. Workflow run `35527395192` passed syntax, boot, contextual sawmill construction, core regression, all building actions, all world-node actions, real early progression, real late Arc I progression, the uninterrupted fresh-save Arc I traversal, recovery checks, GitHub Pages deployment, and Chromium verification against the published build.

This commit is protected by branch `baseline/v0.24-certified`. Structural cleanup must preserve its observable gameplay, approved art and stable QA contracts. If cleanup regresses behavior, compare/recover from this branch rather than reconstructing from older v0.22/v0.21 code.

`runtime-hotfix.js` is migration-only (623 bytes at baseline); core gameplay/dialogue/UI belongs in the canonical runtime.

## Open bugs / risks
- Verify camp and boss actions through real pointer/touch paths, not merely action-panel presence.
- Timed actions are serializable/resumable through absolute timestamps and `recoverFinishedTasks()`; keep regression coverage for view changes and reload/offline completion.
- Bastion VI–X still use confirmation-modal behavior and need explicit progression gates, not only resource costs.
- Stoneworks construction still uses an old confirmation popup; routine actions should be object-local.
- Resource buildings display Bastion-derived levels/rates but do not yet have true independent upgrade levels capped by Bastion.
- Equipment replacement returns the previous item to inventory; preserve this behavior in future inventory work.
- Forge and Hero Hall need complete stable test IDs/wrappers; Hero Hall needs fuller hero switching/stats/skills/XP/equipment UX.
- End-of-test survey, session export/copy and full tester reset requirements are incomplete.
- Later special combats are functional progression placeholders, not the intended polished semiautomatic combat.
- Economy from early game to Bastion X has not been proven deadlock/grind-free by an uninterrupted playthrough.
- Dead legacy runtime remains physically inside the monolithic HTML, although disabled. Do not remove blindly.

## Structural consolidation now active
- Certified v0.24 is the new functional baseline; the historical v0.22 filename/save/API aliases remain only for compatibility.
- Do not rename/migrate those compatibility identifiers during stabilization.
- Consolidate duplicate CSS/runtime layers progressively and test each block.
- Dead legacy runtime may be removed only after proving equivalent behavior through the existing real-interaction and fresh-save gates.
- Do not add gameplay breadth until this consolidation is stable.

## Product course correction — 2026-09-20
The MVP is being refocused on the original product question: does the compact Eldoria loop create understanding, satisfaction and desire to continue? Do not add more breadth beyond the current Arc I scaffold until that is demonstrated.

Required MVP proof:
1. Rebuild/upgrade Valoria with a real economic trade-off.
2. Leave because the kingdom needs something; encounter La Brecha/world danger.
3. Fight/gather/discover and obtain a meaningful reward.
4. Return and visibly use that reward to change kingdom/hero progression.
5. Introduce one relic into Codex with a meaningful use-now vs conserve choice, explained diegetically by Orin.
6. Show a truthful, explicitly simulated 4X horizon: another realm, a safe node, a contested node and Breach influence/territory.
7. End with a new ambition/mystery, not merely a higher Bastion number.

Existing v0.21 Codex/Duel/world-4X code is a system bank. Reconnect only the smallest useful slice; do not restore the old long tutorial chain wholesale.

## Next work
1. Repair the current final-assault QA regression caused by the new pre-assault Aldric dialogue, then certify the uninterrupted fresh-save Arc I traversal.
2. Reconnect the smallest meaningful Codex/relic choice to the current v0.23 flow.
3. Add/restore a concise truthful 4X horizon and one genuine economic opportunity-cost choice.
4. Add QA assertions for the MVP desire loop itself, not only reachability to Bastion X.
5. Run a player-like mobile pass and human playtest before any Unity migration or scope expansion.
6. Keep visual baseline protected throughout.

## Maintenance rule
This file is the short operational truth. Update it whenever version, verified traversal, open blocker, next work, canonical path or stable recovery commit changes.


## Consolidación arquitectónica v0.24 — completada
- Baseline de recuperación: `baseline/v0.24-certified`.
- `v0220/index.html` conserva la composición/render del vertical slice, pero las primitivas estables están separadas en `v0220/js/state-economy.js`, `tasks-offline.js`, `dialogue-tutorial.js`, `ui.js` y `gameplay.js`.
- Eliminado del HTML canónico el runtime legacy deshabilitado (~164 KB de código inalcanzable). La recuperación histórica queda en la rama baseline.
- `runtime-hotfix.js` queda exclusivamente para compatibilidad/migraciones, no gameplay/UI/diálogo.
- `qa/e2e-behavior-regression.js` valida límites de módulos, economía/offline, tareas timestamp y gates de Bastión además de la regresión de recorrido existente.
- El workflow de release es manual (`workflow_dispatch`) para certificar bloques coherentes, no cada commit.
