# Eldoria — Decisions that must survive chat changes

## MVP course correction — 2026-09-20
- Stop expanding breadth after the current Arc I technical blockers are closed. Bastion X is a scaffold/ceiling, not a reason to add Bastion XI or more systems.
- MVP success is not system count. It must prove a compact desire loop that makes the player want to continue.
- Canonical validation loop: **Valoria → need/meaningful choice → world → gather/fight/discover → valuable reward → return → reward changes kingdom/hero → new ambition**.
- Before MVP closure the active flow must demonstrate four protected promises: **La Brecha changes gameplay**, **economy creates a real trade-off**, **a relic/card has real utility and reaches Codex**, **the player sees evidence of a larger shared 4X world**.
- Cards/Codex return as a small meaningful layer, not a large early tutorial. First relic should create a clear use-now vs conserve-for-later choice. Orin owns this explanation.
- World 4X must be a truthful prototype: other realms/territorial conflict may be simulated and explicitly labelled; never imply real multiplayer.
- At least one progression choice must be mutually exclusive or opportunity-cost based. Following a single highlighted next button to Bastion X is not sufficient proof of strategy.
- Do not migrate to Unity until this compact MVP loop is playable, fresh-save certified and human-playtested.

## Product / loop
- Mobile-first dark medieval fantasy city-builder/4X/RPG vertical slice.
- Core loop: **Valoria → need/decision → world → gather/fight → reward → return → visible growth → new ambition**.
- The web build is a low-cost design laboratory. Long-term direction may be Unity, but do not migrate/rebuild until the validated slice is stable.
- Budget remains 0 € unless explicitly changed by the owner.

## UX
- Player must know what to do, why, and where to touch.
- Routine building/upgrade/gather/attack interaction is object-local: tap object → compact cost/requirement + action directly below → active timer above. Avoid routine confirmation popups.
- One new mechanic = one brief contextual explanation at the moment it matters.
- Mobile/iPhone is primary. Avoid overlays/buttons outside safe viewport.
- World remains pannable; essential actions must still work with normal tap/click and be automatable.

## Narrative
- Arc I: **Las Cenizas de Valoria**. Valoria is damaged; La Brecha is the persistent threat.
- Opening order: Narrator, then short Aldric, then play.
- Canonical opening idea: “La Brecha apareció sobre Valoria antes del amanecer…”; Aldric follows briefly: “Valoria sobrevivió a la noche…”.
- Never restore the obsolete long Aldric intro beginning “Mi señor… esto es Valoria. O lo que queda de ella…”.
- Aldric is host/narrative guide, not a constant tutorial commentator.
- Lyra should be desired because the Fissure creates a gameplay need; the current intended event is first Fissure attack → discover/recruit Lyra → return to Fissure empowered.
- Orin is reserved as diegetic guide for Codex/cards/Breach/Duel when those systems return to the active flow.
- Large unlocks deserve ceremony; routine actions do not.

## World, economy and power
- Resource nodes have finite reserves and per-trip loads smaller than total reserves. Depleted nodes leave the active map and later respawn/reappear; exact current tuning belongs to code, not this design document.
- Enemy victories give loot/XP/items, not magical Power.
- Total Power represents development/possessions (buildings, troops, heroes, relevant equipment/collection). March Power represents the deployed force.
- Construction/recollection use visible compressed timers in the slice. Future production can use longer queues, but waiting must represent real activity.
- Resource buildings have independent persistent levels capped by Bastion; upgrades improve their production.

## Buildings / progression
- Bastion is the main progression spine, currently scaffolded to level X.
- Aserradero restores wood production; Cuartel recruits real troops; Granero unlocks food economy; Cantera de Valoria provides stone production; Forja introduces equipment/material progression; Salón de Héroes is the hero/equipment management destination.
- Higher Bastion levels must be gated by meaningful preceding content, not resources alone. Some VI–X gates remain to implement.

## Combat
- Normal world combat should be light/automatic but visible on the map: march → brief impact → result.
- Semiautomatic combat remains the intended richer layer for special encounters; current later fights are placeholders/scaffolding, not final combat design.
- Do not make routine combat a heavy minigame.

## Heroes / equipment
- Heroes are introduced because the player needs them. Aldric leads; Lyra specializes in Breach/rupture/damage; Maelis is support/survival.
- Hero Hall should allow switching heroes and show level/XP, stats, skills and individual equipment.
- Equipment/materials live in Chest/inventory. Replacing equipment must return old equipment to inventory.

## Cards / Codex / Duel
- Cards/relics go directly to the **Codex**, never Chest/inventory.
- Intended concepts preserved from earlier builds: rarity including Legendary, Indestructible as a quality, use/conserve decisions, effects outside Duel, Relic Duel values N/S/E/W.
- Do not force full Cards/Duel back into the early active loop until core Valoria/world progression is solid. Earlier v0.21 is the system bank/reference.

## Engineering / production
- One canonical development line; versions only for real milestones/hotfixes.
- Protected visual recovery commit: `f139968ccbfdeb3e1d37f58568187374faf6d1f2`.
- Preserve stable IDs and move logic gradually toward data-driven state/costs/unlocks/rewards/actions/serializable tasks/narrative events so later Unity migration is feasible.
- Do not fake multiplayer, ownership, rewards or state.
- Repository is the source of truth; update state/changelog with important work.
