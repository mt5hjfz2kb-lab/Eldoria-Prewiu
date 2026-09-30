# Eldoria — Canonical design decisions

Updated: 2026-09-28

This file contains **current design decisions that must survive chat changes**. Historical discussion belongs in git history/CHANGELOG, not here. Repository state wins over chat. If an older statement conflicts with `AGENTS.md`, `SESSION_HANDOFF.md`, `PROJECT_STATE.md` or a later explicit decision here, the newer/higher-level source wins.

## Decision-preservation rule
- A system, chronology or product rule already validated in the web vertical slice remains valid when moving to Unity **unless a later explicit decision supersedes it**.
- Unity migration is an implementation/adaptation effort, not permission to redesign validated product decisions by default.
- When Unity needs a different presentation, controls, camera, performance solution or visual embodiment, preserve the validated gameplay/narrative intent unless the owner explicitly changes it.
- Never reopen a closed decision merely because a newer implementation layer does not yet contain it.

## Product and platform direction
- Eldoria is a **mobile-first** dark-fantasy city-builder / 4X / RPG.
- Phone is the primary validation target; **tablet is supported from the start** with responsive use of the additional space.
- **PC is a future viable expansion, not a committed launch platform.** Architecture, camera and UI decisions should avoid unnecessary mobile-only lock-in so PC can later adapt presentation and controls without rebuilding the core game.
- Current active production uses Unity 6000.3.23f1. The web v0.32.0 slice remains the canonical playable design/reference until Unity replacement is explicitly approved.
- Budget remains 0 € unless the owner explicitly changes it. Do not spend Tripo credits or other paid resources without authorization.

## Core loop
Canonical desire loop:
**Valoria → need/meaningful choice → world → gather/fight/discover → valuable reward → return → visible growth/stronger heroes → new ambition.**

Progression quality is not measured by system count. Every major addition should strengthen this loop or the long-term 4X promise.

## Bastion I–X contract
- **Bastion I–X is the complete playable prologue/tutorial**, not the whole game.
- I–III = survival/discovery; IV–VI = expansion/preparation; VII–VIII = mystery/collection/hero consequences; IX = independent mastery; X = graduation/finale and opening of the larger game.
- Bastion X must feel like **graduation**, not credits/end-of-game.
- After X, the game must stop stretching tutorial logic. Long-term Valoria planning currently preserves structural headroom toward roughly Bastion 25–35; the exact ultimate cap remains provisional.
- Canonical per-level detail and current gaps live in `docs/BASTION_I_X_MASTER_TABLE.md`.

### Per-Bastion presentation rule
Every Bastion ascent should communicate three things clearly:
1. **What changed / what was learned** — one short, legible message.
2. **What changed physically in Valoria** — visible growth, occupation, repair, district/building evolution or atmosphere.
3. **What the player can now do** — one concrete new capability or meaningful expansion of an existing one.

A Bastion level should not read as merely “two more menus unlocked”. From VII onward explicit hand-holding must reduce; IX is the mastery check and X the graduation.

## Canonical Bastion chronology
- **I — Las Cenizas:** reconstruction, first world trip, gather → return → improve; Valoria/Breach mystery is planted.
- **II — Troops and first threat:** Cuartel, real troops, March and first Corrupt combat.
- **III — The world opens:** Granero/Food, hunting, denser frontier, Fissure and two-phase Lyra recruitment/assault.
- **IV — Early autonomy:** **CLOSED.** Aldric states that Valoria cannot rebuild everything at once and the player chooses the first kingdom priority: **Production/Works, Defense, or Shelter/Population**. The choice changes immediate resource emphasis and produces a visible Valoria consequence (work activity, reinforced entry/guard presence, or inhabited hearth/home cues) without permanently locking content. This behavior is implemented in the canonical web vertical slice and is a preserved migration requirement for Unity.
- **V — Cantera and stone economy:** Cantera is the stone-production building, analogous to Aserradero = wood and Granero = food. Extraction can seed anomalous materials/discoveries without over-explaining them.
- **VI — Forge and closed progression loop:** Breach elite / Devorador → special material → Forja → Hoja de Éter / first equipment ceremony → stronger hero/power loop.
- **VII — Knowledge + Relicario onboarding:** Códice and Relicario unlock as **independent peer systems**. First deterministic Reliquia/card reaches Relicario. Teaching is progressive rather than dumping the full system at once.
- **VIII — Wider-world consequence / Maelis:** bespoke world situation, Nareth/wider-realm consequence where applicable, dialogue, motive and Maelis recruitment ceremony. Hero recruitment is never a generic mission-complete reward.
- **IX — Independent mastery:** no step-by-step teaching. Player solves a multi-system preparation problem and uses the two-hero March requirement/trial as an autonomy check.
- **X — Prologue finale / graduation:** learned loop is applied in a meaningful expedition; Herald/Hospital loop teaches combat wear/recovery; immediate frontier danger resolves while the larger Breach mystery opens. End state communicates **CAPÍTULO I COMPLETADO**, not game completion.

## Resource/building responsibility
- **Aserradero = wood production.**
- **Granero = food economy.**
- **Cantera de Valoria = stone production.**
- **Cuartel = recruit/upgrade troops.**
- **Forja = materials → equipment progression.**
- **Salón de Héroes = hero/equipment management.**
- **Hospital = wounded-troop recovery; it never restores permanent casualties.**
- Routine building interaction remains object-local where practical: tap object → requirement/cost + action → timer/feedback.
- **Construction queue v1:** for now Valoria allows **one active building construction or building/Bastion upgrade at a time**. Gathering, troop recruitment and Hospital treatment are separate task categories and do not consume the building-construction slot. This limit is part of the gameplay contract to preserve in Unity unless explicitly redesigned later.
- **Construction queue:** for the current vertical slice, Valoria permits **one active construction/building upgrade at a time**. Gathering, troop training and medical treatment are separate task families and do not consume this construction slot. This is a deliberate current rule, not a permanent monetization/queue commitment.

## Códice, Relicario, Arcón
These responsibilities are permanent unless explicitly redesigned:
- **Códice = world knowledge/discovery only:** La Brecha, Bestiario, Mundo, Personajes and related lore/discoveries.
- **Relicario = Reliquia/card system:** collection, reveal ceremony, rarity, use/conserve, N/S/E/O, Practice and future Duel behavior.
- **Arcón = objects/materials/equipment.** No Reliquias/cards live there.
- Códice and Relicario are independent first-level peer destinations; neither is parent/child of the other.
- **Códice and Relicario do not require dedicated physical buildings in Valoria.** They are first-class interface/systems destinations. Bastion VII may add environmental/civic/mystic dressing that signals cultural recovery, but no plot or production building is reserved for either system unless a later explicit design decision changes this.
- Bastion VII introduces both, but **Relicario teaching is deliberately staged**: first card/reveal and basic use/conserve first; rarity/Indestructible/N-S-E-O/Practice deepen progressively through VII–IX and can continue after X. Do not overwhelm the player with the entire card ruleset at first contact.
- Indestructible is a property/quality, never a rarity.
- Ordinary eligible world-enemy victories may roll Common/Rare cards at very low rates; hunting grants none; Epic/Legendary acquisition stays special rather than ordinary-drop progression.

## Narrative and heroes
- Arc I is **Las Cenizas de Valoria**; La Brecha is the persistent threat and mystery.
- Opening order: Narrator → brief Aldric → play. Aldric is host/narrative guide, not a constant tutorial commentator.
- Hero recruitment is a narrative event. Major heroes appear in context, exchange dialogue, have a believable motive and receive a ceremony before joining.
- Lyra is two-phase: Fissure investigation/contact → motive/alliance dialogue → Fissure assault together.
- The Manuscrito de la Fisura remains a future-purpose discovery and must not reveal its later troop-family function early.
- Maelis joins through a bespoke narrative/world situation, not a generic reward.
- Curiosity escalation matters: I something is wrong → II not an accident → III extends beyond Valoria → V–VI corruption creates strange matter → VII Reliquias create a second unknown → VIII other realms/people are affected → IX player survives independently → X local threat is one expression of something larger.

## Economy, world and Power
- Resource nodes have finite reserves and per-trip loads smaller than total reserves; depleted nodes leave the active map and later return/respawn according to current implementation/balance.
- Enemy victories grant loot/XP/items, not magical Power.
- **Poder Total** describes development/possessions; **Poder de expedición / March Power** describes the deployed force. They are not interchangeable.
- At least one meaningful opportunity-cost/trade-off must exist; progression cannot collapse into following one highlighted next button.
- The world should feel populated and aspirational: visible locked higher-level nodes/threats may foreshadow growth without exposing future troop families prematurely.

## Combat and troop progression
- Combat complexity layers progressively: **Caza → Poder**, **enemy common → stats + composition**, **uncommon → trait/counter-play**, **world boss → semiautomatic + minimal hero intervention**.
- Current combat stats are ATQ / DEF / VIDA / RUPTURA / PODER and must affect resolution.
- Routine combat should remain light/automatic enough for mobile; special encounters carry the richer layer.
- Current player-facing recruitable troop family is Arqueros. Future families follow **discover → understand → unlock** and must not be previewed before narrative discovery.
- PvP is not implemented. Never fabricate opponents, rankings, alliances, rally participants, rewards or ownership.

## Wounded troops / Hospital / future operations
- PvE may wound troops but creates **0 permanent troop deaths**.
- Wounded troops remain owned but unavailable for March composition until treatment completes.
- Hospital restores wounded troops through persistent timed treatment and never restores permanent casualties.
- Future PvP may later distinguish available / wounded / permanently lost units, but those percentages/rules are not implemented now.
- March data may reserve future joint-operation identity, but no fake multiplayer UI is allowed before real multiplayer exists.

## Visual and Valoria direction
- Art target: **stylized semi-realistic dark fantasy 4X**, not photorealism and not bright/cartoon fantasy.
- Valoria is a persistent vertical bastion/city rebuilding inside monumental imperial ruins, not a generic field of detached buildings.
- Progression should visibly repair, occupy, densify and elevate Valoria. Major eras carry strong transformations; individual Bastion levels still need lighter visible feedback.
- Camera is authored isometric with zoom 9..19 and bounded panning/progression-aware bounds; free 360° orbit is out of scope.
- The certified Playable District Skeleton is the kernel, not the final city footprint. Preserve Master Envelope expansion reservations and future districts.
- Final acceptance of city art is from official camera/zoom/pan views with interaction/circulation preserved, not from isolated asset beauty alone.

## UX and teaching
- Player must understand **what to do, why, where to touch, what happened and what to do next**.
- One new mechanic = one brief contextual explanation when it first matters; deeper rules arrive when they become actionable.
- Mobile/iPhone is primary. Essential controls stay inside safe viewport and world context remains visible where practical.
- Large unlocks deserve ceremony; routine actions do not.
- From Bastion VII onward guidance tapers. Bastion IX should require independent application rather than instructions.

## Engineering / source-of-truth
- `main` is the only active development line; verify live HEAD before execution.
- Unity is an active source line, not a hypothetical future migration. Preserve approved web behavior/design as reference until Unity parity/replacement is explicitly approved.
- Make surgical changes; preserve stable IDs and validated gameplay contracts.
- Do not fake certification: changed ≠ verified; tests green ≠ published; deployed ≠ published interaction verified.
- Important new product decisions belong here or in the linked canonical specialist document; operational runs/HEAD belong in `SESSION_HANDOFF.md`; current functional implementation belongs in `PROJECT_STATE.md`.
