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

## Tutorial/prologue contract — 2026-09-21
- **Bastion I–X is the complete playable prologue/tutorial.** By Bastion X a first-time player must understand Eldoria's core loop without external instructions: rebuild/upgrade → produce → explore → gather/fight/discover → obtain a meaningful reward → return → strengthen Valoria/heroes → pursue the next ambition.
- I–III = survival and discovery; IV–VI = expansion and preparation; VII–VIII = mystery/collection/hero consequences; IX = independent mastery check; X = graduation/finale that resolves the immediate threat while opening the larger Breach mystery.
- The tutorial must create curiosity, not merely explain controls. Early events should seed unanswered questions about the Breach, disappearances, anomalous materials/relics and the larger world without explaining the full truth.
- Hero recruitment is a narrative event, never a routine reward. Every major hero first appears in-context, exchanges dialogue with Aldric/other relevant characters, states a believable motive, and receives a ceremony before joining.
- **Lyra recruitment is two-phase:** investigate the Fissure perimeter → meet Lyra while she is searching for traces of the disappeared → establish her motive and alliance through Aldric↔Lyra dialogue → then assault the Fissure together.
- The Fissure assault with Lyra awards an **unknown manuscript**. It is deliberately unusable during the current prologue; it seeds a later Barracks troop unlock and must be stored as a future-purpose discovery, not explained away.
- Character/narrator story beats use the canonical typed dialogue surface. Sir Aldric dialogue always uses his canonical portrait; do not create a parallel dialogue system.
- The world should feel populated. Resource nodes and beasts/enemies are distinct categories: beasts primarily return food; Breach/corrupted enemies return combat/rare progression rewards. Higher node levels can be visible as locked/aspirational content before unlock.
- Preserve the current working loop and certified visual baseline. This is a reorganization/reinforcement of the MVP, not a rewrite.

## Canonical Bastion I–X chronology — 2026-09-21
- **Bastion I — Las Cenizas:** Aldric carries the opening with more narrative weight. Rebuild, first world trip, gather, return and improve. The first expedition exposes an unexplained corruption mark that Aldric recognizes but refuses to explain fully. Core question planted: what really happened to Valoria?
- **Bastion II — Troops and first threat:** Barracks, real troops and first Corrupt combat. Victory leaves an anomalous residue rather than a routine reward-only beat, reinforcing that the Breach is changing the world.
- **Bastion III — The world opens:** Granary, Food, hunting and a denser frontier. Higher-level resources/beasts/enemies are visible before they are reachable to create aspiration. The Fissure and Lyra are the first major epic beat; Lyra's alliance is contextual and the Fissure assault follows as phase two.
- **Bastion IV — Early autonomy:** richer map and wider decisions while explicit tutorial guidance recedes. The player should begin choosing what to improve and where to go rather than following a single hand.
- **Bastion V — Quarry and extraction:** unlock the productive Quarry and seed the idea that extraction can reveal anomalous objects/materials. Do not over-explain or lock final RNG design yet.
- **Bastion VI — Forge and closed progression loop:** Breach elite → special material → Forge → first equipment ceremony. This is the first complete kingdom → exploration → combat → material → equipment → hero strength → Power loop.
- **Bastion VII — Codex mystery:** first card/relic arrives as a special discovery, unlocks the Codex and enables future relic drops. Explain collection, not ultimate purpose. Protected question: what are these relics really for?
- **Bastion VIII — Maelis:** a bespoke world situation, dialogue, motive, consequence and hero ceremony. Maelis never joins as a generic mission-complete reward.
- **Bastion IX — Independent mastery:** stop teaching. Present a multi-system preparation problem with no step-by-step guidance; the player must apply economy, heroes, equipment, troops and march knowledge independently.
- **Bastion X — Prologue finale:** a meaningful expedition uses the learned loop, resolves the immediate frontier danger and reveals that the Breach is part of something much larger. End with **CAPÍTULO I COMPLETADO** while preserving the larger mystery.
- Narrative escalation across the prologue: **I something is wrong → II it was not an accident → III Lyra proves it extends beyond Valoria → V–VI corruption produces strange matter → VII relics introduce a second unknown → VIII Nareth proves other realms are affected → IX the player can survive independently → X the local threat is only one expression of the Breach.**

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


## Layered PvE combat and troop progression — v0.26.6
- Combat complexity is introduced in layers, not all at once: **Caza → Poder**, **enemigo común → estadísticas + composición**, **poco común → rasgo + contra-juego**, **jefe del mundo → semiautomático + una intervención mínima del héroe**.
- Current combat statistics are **ATQ / DEF / VIDA / RUPTURA / PODER**. These values must affect resolution; they are not decorative UI.
- Hunting stays intentionally simple and resolves from Power without a march-composition screen.
- Common threats introduce PREPARAR MARCHA with real archer count and hero selection, followed by a report that explains the result.
- Uncommon threats must have a real mechanical trait. Acechador de Ceniza establishes the pattern with **Emboscada**: stronger opening pressure followed by exposed defense. A different march composition must be capable of changing the outcome.
- World bosses use automatic troops/basic exchanges. The player gets at most one small role-consistent hero intervention in this scaffold; this is an architecture seam, not the final hero-skill system.
- **PvP is not implemented yet** and must not be implied by fake opponents, rankings or combat language.
- The only recruitable troop family in the current player-facing build is **Arqueros**.
- Troop progression follows **encontrar → comprender → desbloquear**. Do not preview future troop families in UI, locked cards, silhouettes or tooltips before their narrative discovery.
- Internally, the troop-family seam may reserve future families. **Paladines** are intended around Bastión 11 with narrative justification; **Brujos** are later and tied to La Brecha. Neither is currently recruitable or visible to players.
- The existing **Manuscrito de la Fisura** remains on its current acquisition/storage flow. Do not duplicate it, rename its purpose, or reveal its troop-unlock connection early.
- Mobile combat UX must preserve the world as context: compact selection, march preparation, report, timer/combat, and return flow; no oversized permanent panels.
