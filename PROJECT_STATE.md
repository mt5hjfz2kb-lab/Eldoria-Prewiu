# Eldoria — PROJECT STATE
Updated: 2026-09-19\nRelease candidate: v0.23.17 fresh-save Arc I gate

## Current version
- Runtime/API: **v0.23.24**.
- Canonical source: `v0220/index.html`.
- Development branch: `development/v0.23-clean`.
- Public deployment source: the workflow copies `v0220/index.html` to `playtest/index.html`.
- Public URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/
- Protected visual recovery point: `f139968ccbfdeb3e1d37f58568187374faf6d1f2`, branch `stable/visual-good-f139968c`.
- Pre-documentation development HEAD: `feee035322acc58108bed0e41a748cbbf87539cf`.

## Last functional milestone
v0.23 moved routine building/world interaction to object-local contextual controls. Node/building actions are intended to appear below the tapped object and timers above it. The first Fissure interaction is being changed so attacking the Fissure when Lyra is absent triggers the Lyra recruitment event; after recruitment the Fissure can be attacked/defeated normally.

## Currently verified
Workflow run `35458031436` completed successfully after adding `qa/e2e-late-progression.js`: real mobile early progression through Fissure→Lyra and the late Arc I chain Bastion VI→Forge→Devorador→Ascua→forja de equipo→Bastion VIII→Nareth/Maelis→Bastion IX→March Trial→Bastion X→final assault all pass, followed by successful Pages deployment and published-build Chromium verification.

This is strong segmented end-to-end coverage, but it still uses QA state setup between major phases; a single uninterrupted fresh-save run from the opening through Arc I remains the next verification target.

The standard CI has previously passed boot, sawmill contextual/start/complete, core state fixtures, building action availability, world-node action availability, recovery checks, deploy and published-build Chromium checks. These checks do **not** constitute a complete fresh-save Chapter I playthrough.

v0.23.16 makes QA-mode overlay suppression deterministic. Workflow run `35457570522` completed successfully: the real mobile Fissure→Lyra regression passed locally, all existing regressions passed, Pages deployed, and the selected Chromium checks including the Fissure→Lyra flow passed against the published URL. The **full uninterrupted fresh-save Arc I traversal is still not yet implemented**, so later progression remains to be certified.

## Open bugs / risks
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

## Work now under verification\n- Added `qa/e2e-full-arc1.js`, an uninterrupted fresh-save mobile traversal from Bastion I to the Arc I finale. It uses real taps/clicks for gameplay and only accelerates passive production through `advanceEconomy()`; it does not use `setQA/loadState` to jump progression.\n- Early economy blocker found before Bastion V: the old Quarry reserve/load could not fund the required stone before Stoneworks unlocked. Quarry reserve is now 2,800 with 700-stone loads; this change is pending full CI verification.\n\n## Product course correction — 2026-09-20
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
