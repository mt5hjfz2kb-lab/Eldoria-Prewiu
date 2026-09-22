# Eldoria — PROJECT STATE
Updated: 2026-09-22

This file describes **functional project state only**. Operational HEAD/current task belongs in `SESSION_HANDOFF.md`; permanent process belongs in `AGENTS.md`.

## Active product
- Runtime/API milestone: **v0.26.5**.
- Canonical source: `v0220/index.html` + `v0220/js/`.
- Development branch: `main`.
- `v0220` and save/API aliases containing v022/v023 are compatibility identifiers, not competing active versions.
- `playtest/` is generated deployment output, never editable source.
- Certified normal-development baseline: v0.26.5 systems on `main`; exact operational HEAD belongs in `SESSION_HANDOFF.md`.
- Protected visual recovery: `f139968ccbfdeb3e1d37f58568187374faf6d1f2` / `stable/visual-good-f139968c`.
- Older `v019*`, `v020*`, `v0210` and r7 material are historical/reference only.

## Implemented Arc I scaffold
The active slice supports the Valoria-first progression through Bastion X: rebuilding/production, world gathering, Corrupts, Fissure/Lyra, food/hunting, Cantera, Forge/Aether equipment, Nareth/Maelis, march configuration/trial and final Breach assault. The playtest now ends explicitly with an Arc I completion screen, survey, copyable summary, JSON session export and complete local reset. Normal world combat is intentionally light/automatic; later special combats remain scaffolding rather than final combat design.

Stable subsystems are separated under `v0220/js/` for economy/state, timestamp tasks/offline recovery, dialogue/tutorial, UI and gameplay helpers. `runtime-hotfix.js` remains migration/compatibility only.

## Protected product direction
The MVP question is whether the compact loop creates understanding, satisfaction and desire to continue:
**Valoria → need/meaningful choice → world → gather/fight/discover → valuable reward → return → visible growth → new ambition.**

Before scope expansion/Unity migration, the slice still needs to prove: a genuine economic trade-off, meaningful Breach gameplay, a useful relic/card reaching Codex with use/conserve tension, and a truthful simulated larger 4X horizon.

## v0.26 implemented product block
- Combat language now separates hunting PvE from threat PvE; PvP is explicitly reserved architecturally and not exposed as a fake live system.
- Hero Hall and expedition configuration expose readable attack/defense/support roles and explain that Power is a development summary rather than a single combat verdict.
- Arcón is a bottom-navigation inventory for objects/materials/equipment; cards and relics remain strictly in Códice.
- World activity now yields named upgrade materials (ancient iron brace, ash hide, stone tusk) used by production-building upgrades, connecting exploration to city progression.
- Bastion VII introduces the first relic through Orus, opens Códice and a guided Duelo de Reliquias. Indestructible remains a quality, never a rarity.
- Mobile world-map rendering has a dedicated v0.26 performance pass using transform containment/backface/will-change rules and removes transition work during touch panning.
- Macro systems (server Breach, deep talents, resonances, oracle and full PvP) remain documented future scope, not implemented.

## Known functional gaps / debt
- Bastion VI–X still need human pacing validation beyond automated reachability.
- Hero Hall needs deeper hero stats/skills/equipment UX; this is intentionally not expanded in the stabilization milestone.
- Special late combats are placeholders for the intended richer semiautomatic layer.
- Economy pacing to Bastion X needs human/player-experience validation even when automated reachability is green.
- The Códice implements the use/conserve decision, Indestructible cooldown behavior, consumed-card Echo consequences, Orus orientation training and a playable 3×3 formal Duelo de Reliquias board. A wider relic economy and broader card pool remain future scope.
- Renewable/additional world nodes, hunting respawn, live timers and expanded canonical narrative/dialogue work remain implemented.

## Verification state
Normal development continues exclusively on `main`. The frozen external tester cohort is **Eldoria Closed Playtest T1 / 0.26.5-test.1** and is not an active product line. Its immutable reference and feedback protocol are documented in `SESSION_HANDOFF.md` and `TESTER_FEEDBACK_PROTOCOL.md`.

## Maintenance
Change this file only when implemented functionality, product scope, verified stable baseline, or known functional gaps change. Do not put transient next-task/HEAD information here.


## v0.26.1 — coherence pass
- World nodes respawn by timer and relocate instead of reappearing at a fixed coordinate.
- Combat and expedition expose ATQ / DEF / APO for heroes, troops and the active march; Power remains a summary.
- Hero Hall and Codex receive high-contrast presentation.
- Bastion IX attack interaction is part of release acceptance; real player path must remain actionable.
- Codex / Orus / Relic Duel preserve the v0.21 clarity reference while staying integrated with current Arc I lore.


## v0.26.2 — Códice / Reliquias / Arcón
- Arcón is a dedicated full-screen view and no longer opens as a floating modal over the current map.
- Códice is a dedicated full-screen archive view with stronger hierarchy and card presentation.
- The v0.21 card principle is restored: using a normal relic consumes it for its immediate effect and leaves a weak Echo 1/1/1/1 for Duelo; conserving preserves its full N/E/S/O values and does not prevent later use.
- Indestructible is a quality, not a rarity. Indestructible relics survive activation and enter cooldown.
- The first Sello de Ceniza has a concrete outside-Duelo effect and a visible use/conserve decision.
- Orus offers guided N/E/S/O training before the formal Duelo milestone.
- At 5 discovered relics, the formal Duelo opens as a playable 3×3 board: select a card, choose a cell, compare touching N/E/S/O values, capture weaker adjacent cards, and gain +1 N/E/S/O in the central Breach-resonance cell.
- Formal Duelo unlock progress is based on 5 discovered relics; consumed discoveries still count but enter the hand as Echoes 1/1/1/1.
- qa/e2e-codex-chest.js protects dedicated views, use/conserve explanation, Indestructible cooldown semantics and Orus orientation training.


## v0.26.3 — guidance, Hero Hall and sequencing
- Salón de Héroes is a dedicated full-screen management surface. It combines hero role/stat reading, equipment and expedition formation instead of splitting them across modal flows.
- Bastion IX has a device-independent persistent attack objective once a valid march exists; the action sits outside the pannable world transform.
- Tutorial guidance extends beyond the opening chapters into Forge/Devorador, Códice/Orus, Nareth and the Bastion IX march trial.
- The first Códice visit teaches the relic economy through Orus: outside-Duelo effects, N/E/S/O, use vs conserve, consumed Echoes and Indestructible cooldown.
- Narrative reveals now respect discovery order: the Forge does not explain Aether before the material is found, and Maelis is not introduced before reaching Nareth. Duplicate explanatory beats were reduced.
- Timed construction/upgrade costs are deducted when work starts. Completion applies the result only; legacy unpaid in-flight tasks remain migration-safe.
- v0.26.3 acceptance adds mobile 390×844 checks for march/attack reachability and start-time resource spending while preserving the full Arc I regression.


## v0.26.4 — unified visual / UX system
- Functional rules are unchanged; this milestone is a presentation/navigation pass with explicit parity protection.
- Bottom navigation is normalized to Ciudad, Mundo, Héroes, Arcón and Códice; Ranking remains available in the HUD.
- Routine city/world interactions use a shared contextual bottom-sheet pattern while deep management remains full-screen.
- Forge joins Héroes, Arcón and Códice as a full-screen management view.
- Arcón presents an inventory grid plus non-destructive in-screen item details.
- World node categories are visually differentiated without changing node behavior or rewards.
- Mobile map movement uses direct pointer tracking, restrained inertia and dynamic limits while preserving object taps.
- UI scale, spacing, safe areas, touch targets, panel hierarchy and restrained color semantics are centralized in the v0.26.4 visual layer.
- `UI_PARITY_0264.md` and `qa/e2e-v0264-ui-parity.js` define and protect the before/after contract.


## v0.26.5 — guidance, infrastructure and collection onboarding
- Contextual progression guidance now names missing resources, quantities and where to obtain them, and can direct the player to the relevant world node or city building.
- Barracks has two distinct loops: infrastructure upgrades and timestamp-based troop recruitment. Recruitment persists through view changes/offline time.
- Bastion IV requires level-2 Aserradero, Cuartel and Granero, establishing the infrastructure gate before further fortification growth.
- Lyra recruitment is the explicit onboarding moment for the Heroes layer.
- The Devourer has a post-combat narrative consequence before the Aether material/Forge loop.
- Arcón uses object-specific visual identity; Códice remains the exclusive Reliquia/card surface.
- All deep management surfaces follow the mobile vertical-scroll contract.
- Códice now frames discovery, conservation and activation as its core loop, and its first Duelo is a guided 3×3 training match with five temporary loan cards that never mutate the permanent collection.
- The world frontier remains one pannable map but uses a larger spatial footprint and wider node distribution to communicate future 4X scale without pretending the final shared-world system exists.
