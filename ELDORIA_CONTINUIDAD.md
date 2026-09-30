# Eldoria — Continuity and system specification
Updated: 2026-09-19. Read on demand when the current task needs product/system continuity beyond the short operational state.

## Purpose
This file captures the project knowledge needed to continue without chat history. Code remains authoritative for exact current behavior; this document records intended behavior, implemented scope and gaps.

## Product
Eldoria is a mobile-first dark medieval fantasy city-builder/4X/RPG prototype. The slice tests whether rebuilding **Valoria** creates enough attachment and ambition to pull the player into exploration, combat, heroes and collection.

Guiding loop: **build → produce → spend → improve → explore → gather/fight → obtain resources/heroes/items → strengthen → return to build**.

## Current Arc I player flow
Intended current sequence:
1. Narrator establishes Valoria/La Brecha; brief Aldric introduction.
2. Rebuild Aserradero.
3. Enter World; gather finite Forest and Quarry resources.
4. Defeat Corrupts/camp and return with loot.
5. Rebuild Bastion II; Cuartel becomes available; build/recruit troops.
6. Fissure III presents a need Aldric cannot solve alone.
7. First Fissure attack/event reveals Lyra; recruit her.
8. Attack Fissure with Lyra; receive stone/XP; Fissure marked defeated.
9. Raise Bastion III.
10. Build Granero; expanded pannable frontier exposes food/hunting.
11. Raise Bastion IV, then V; build Cantera de Valoria.
12. Progress to VI; reconstruct Forge.
13. At VII defeat Devorador de Éter → obtain Ascua de Éter material.
14. Forge equipment from the Ascua.
15. At VIII investigate/fight Nareth → rescue/recruit Maelis.
16. At IX configure a march and complete March Trial.
17. At X perform final assault on Corazón de la Brecha.
18. Arc I completion: immediate threat falls, Breach remains open.

**Verification warning:** this whole chain is not yet proven as one uninterrupted fresh-save E2E. Later steps are scaffolding and targeted-state reachable; see `PROJECT_STATE.md`.

## State/resources
Active save key: `eldoria-v022-consistent-loop`. State includes wood, stone, food, power, troops, node reserves, Bastion/building flags, hero flags/XP/levels, tasks, view, chapter, selected action, Forge/Aether flags, inventory, equipped gear, march configuration, late encounter flags and Codex.

Starting state currently includes 230 wood, 150 stone, 0 food, 1,800 base power and 36 troops. World reserves: forest 1,250, quarry 900, meat 1,100. Gather loads: wood 360, stone 260, food 300.

Current Bastion resource costs in code:
- II 450 wood / 300 stone + Corrupts cleared.
- III 650 / 500 + Lyra + Fissure defeated.
- IV 850 / 700 / 300 food.
- V 1,100 / 900 / 500.
- VI 1,300 / 1,050 / 650.
- VII 1,500 / 1,200 / 800.
- VIII 1,750 / 1,400 / 950.
- IX 2,050 / 1,650 / 1,100.
- X 2,400 / 1,950 / 1,300.
VI–X currently need stronger content gates.

Passive rates currently derive from Bastion level when the relevant building exists. This is an implementation shortcut; intended future design is explicit building levels capped by Bastion.

## Missions/tutorial
`goal()` drives the current objective card from game state. Tutorial philosophy is contextual and minimal: the world teaches actions; characters add meaning. Do not add external instructions to make a playtest completable. The test is whether in-game guidance works.

Important onboarding requirement: routine actions are local to their object. A selected building/node exposes cost/load/march info and its action immediately below it. An active timer appears above the same object.

## Buildings
- Bastion: progression spine.
- Aserradero: initial reconstruction, wood passive production.
- Cuartel: construction then recruitment of 5 guards for resources; troops contribute Power/March Power.
- Granero: unlocked after Bastion III; food passive economy.
- Cantera de Valoria / stoneworks: unlocked around Bastion V; stone passive economy + ranking flag.
- Forja: Bastion VI+; consumes Ascua de Éter to create Hoja de Éter equipment.
- Salón de Héroes: hero/equipment UI; incomplete versus intended final UX.

Routine construction should use contextual action + timer, not confirmation modals. Stoneworks and higher Bastion still violate this in places.

## World nodes
Current nodes include Forest, Quarry, Corrupts camp, Fissure/boss, Meat hunting ground, Wolves, Boar, Devorador de Éter, Nareth, March Trial and final Breach node. The world is a single pannable surface; later nodes extend beyond the initial viewport.

Forest/Quarry/Meat are finite. Wolf/Boar reward food. Camp rewards wood/stone. Fissure with Lyra rewards stone + Lyra XP. Devorador yields Aether Ember. Nareth yields Maelis. Trial validates configured march. Final closes Arc I.

## Combat
Normal early combat is map-visible automatic movement/impact/result. Later `specialCombat` encounters are lightweight placeholders. The intended richer direction is semiautomatic combat for meaningful special encounters, not every routine world fight.

## Heroes
- Sir Aldric: starting leader/guardian.
- Lyra, Arquera de Ceniza: rupture/damage/Breach specialist; recruited through Fissure need/event.
- Maelis, Custodia de Nareth: support/survival; rescued later at Nareth.
March configuration pairs Aldric with an available specialist.

Hero Hall currently shows available heroes and equipment count/March Power and can open individual equipment handling. Intended expansion: internal hero switching, clear level/XP/stats/skills, individual equipment slots and stable test IDs.

## Inventory/equipment
Inventory/Chest is for equipment and materials. Aether Ember is a material dropped by Devorador; Forge can consume it to create Hoja de Éter. Equipped items contribute where implemented. Known bug: replacing an equipped item can lose the old item instead of returning it to inventory.

## Cards / Codex / Duel
Current v0.23 active flow keeps these secondary while the core loop is repaired. `codex` remains in state. Design to preserve from earlier validated work:
- Cards/relics go directly to Codex, never Chest.
- Rarities including Legendary.
- Indestructible is a quality.
- Meaningful consume/conserve choice and effects outside Duel.
- Relic Duel uses N/S/E/W values.
- Orin teaches Codex/cards/Breach/Duel diegetically.
Earlier v0.21 files are a reference bank, not a source to overwrite v0.23.

## Power
Total Power is development, not loot: buildings/troops/heroes/relevant collection. March Power is the force sent. Enemy kills themselves should not grant magical Power; their loot/XP can enable development that raises Power.

## Time/tasks
`timedAction()` creates visible tasks with start/end and callbacks. Slice durations are seconds. **Known architectural defect:** callback completion is not serializable, so reloading during an active task can lose completion behavior. Future task records must encode a resolvable action type/payload and resume on load.

## End-of-test / playtest requirements
The target playtest experience must eventually support: fresh start, full content completion, unmistakable end, final survey, copy/export session summary, and full reset for another tester. Survey/export are not complete in active v0.23.

## Visual/art direction
Dark medieval fantasy; Valoria should read as a kingdom the player wants to grow, not an app dashboard. Current CSS/emoji art is provisional but the owner approved the overall visual baseline at `f139968c`. Preserve it during logic work. Large unlocks get ceremony. Mobile layout first.

## Architecture
Active game is still a large monolithic `v0220/index.html` containing CSS, HTML and JS. Canonical root is `#eldoria-core-loop`; active body hides older shell. A legacy runtime remains physically present but is disabled with an early return. Do not delete until dependency analysis and regression testing prove safe.

Gradual architecture target: separate state/data (costs, unlocks, rewards), commands/actions, serializable tasks, narrative events and renderer. This is preparation for a possible Unity production slice, not permission to rewrite now.

## Historical material
`v019*`, `v020*`, `v0210`, `docs/R7_CONSOLIDATION.md`, `docs/QA_R7.md`, and old build scripts document earlier phases. They can explain recovered systems/decisions but are not current operational truth. When they conflict with `AGENTS.md`, `PROJECT_STATE.md`, this file, current code or latest commits, treat them as historical.

## Current work queue
See `PROJECT_STATE.md` for the short list. Major known work: certify Fissure→Lyra; full fresh-save E2E; serializable tasks; higher Bastion gates and object-local interactions; true building levels; equipment swap; Hero Hall; late combat; end survey/export/reset; economy/deadlock pass; eventually clean dead runtime without visual regression.
