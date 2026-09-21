# Eldoria — PROJECT STATE
Updated: 2026-09-21

This file describes **functional project state only**. Operational HEAD/current task belongs in `SESSION_HANDOFF.md`; permanent process belongs in `AGENTS.md`.

## Active product
- Runtime/API milestone: **v0.26.2**.
- Canonical source: `v0220/index.html` + `v0220/js/`.
- Development branch: `main`.
- `v0220` and save/API aliases containing v022/v023 are compatibility identifiers, not competing active versions.
- `playtest/` is generated deployment output, never editable source.
- Previous certified playable baseline: `343b7bde7dd0dd75e5c0bd47984938dcd7a6f6bd` (v0.25.1). v0.26.1 is the active release candidate pending the certification run attached to the final v0.26 documentation commit.
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
v0.26.1 must only be called certified after its final Pages workflow passes the full local-equivalent gate, uninterrupted fresh-save Arc I and published Chromium verification. Exact result belongs in `SESSION_HANDOFF.md`.

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
