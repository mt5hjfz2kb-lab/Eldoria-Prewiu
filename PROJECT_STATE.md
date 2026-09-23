# Eldoria — PROJECT STATE
Updated: 2026-09-23

This file describes **functional project state only**. Operational HEAD/current task belongs in `SESSION_HANDOFF.md`; permanent process belongs in `AGENTS.md`.

## Active product
- Runtime/API milestone: **v0.30**.
- Development branch: `main`, the only active development line.
- Canonical editable source: `v0220/index.html` + `v0220/js/`.
- `v0220` and save/API aliases containing v022/v023 are compatibility identifiers, not competing active versions.
- Normal development URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/
- `playtest/` is generated deployment output, never editable source.
- Frozen external tester snapshot: **Eldoria Closed Playtest T1 / 0.26.5-test.2** in `tester-v0265/`; it is isolated research output and is never a development source.
- Frozen tester integration commit: `df618e86be9da399bb827d5e6cebc3f13e55ff97` (current re-frozen snapshot after the final-survey contrast hotfix).
- Exact operational HEAD must be verified live from `main`; it is not derived from CHANGELOG or historical baselines.
- Protected visual recovery: `f139968ccbfdeb3e1d37f58568187374faf6d1f2` / `stable/visual-good-f139968c`.
- Older `v019*`, `v020*`, `v0210`, old r7 material and superseded version folders are historical/reference only.

## Implemented Arc I scaffold
The active slice supports the Valoria-first progression through Bastion X: rebuilding/production, world gathering, Corrupts, Fissure/Lyra, food/hunting, Cantera, Forge/Aether equipment, Nareth/Maelis, march configuration/trial and final Breach assault. The playtest now ends explicitly with an Arc I completion screen, survey, copyable summary, JSON session export and complete local reset. World combat now uses a layered PvE scaffold: hunting teaches Power, common threats teach five combat statistics and march composition, uncommon threats add real traits/counter-play, and world bosses use semiautomatic combat with one minimal hero intervention. PvP combat remains unimplemented; the Relicario exposes only a clearly locked future Duelo PvP tab, with no active matchmaking or rewards.

Stable subsystems are separated under `v0220/js/` for economy/state, timestamp tasks/offline recovery, dialogue/tutorial, UI and gameplay helpers. `runtime-hotfix.js` remains migration/compatibility only.

## Protected product direction
The MVP question is whether the compact loop creates understanding, satisfaction and desire to continue:
**Valoria → need/meaningful choice → world → gather/fight/discover → valuable reward → return → visible growth → new ambition.**

Before scope expansion/Unity migration, the slice still needs to prove: a genuine economic trade-off, meaningful Breach gameplay, a useful relic/card reaching the Relicario with use/conserve tension, and a truthful simulated larger 4X horizon.

## Implemented baseline inherited from v0.26 (historical label; active in v0.27)
- Combat language now separates hunting PvE from threat PvE; PvP is explicitly reserved architecturally and not exposed as a fake live system.
- Hero Hall and expedition configuration expose readable attack/defense/support roles and explain that Power is a development summary rather than a single combat verdict.
- Arcón is a bottom-navigation inventory for objects/materials/equipment; cards and relics remain strictly in Códice.
- World activity now yields named upgrade materials (ancient iron brace, ash hide, stone tusk) used by production-building upgrades, connecting exploration to city progression.
- Bastion VII introduces the first relic through Orus, opens Códice and a guided Duelo de Reliquias. Indestructible remains a quality, never a rarity.
- Mobile world-map rendering has a dedicated v0.26 performance pass using transform containment/backface/will-change rules and removes transition work during touch panning.
- Macro systems (server Breach, deep talents, resonances, oracle and full PvP) remain documented future scope, not implemented.

## Layered PvE combat (introduced in v0.26.6; active in v0.27)
- **Hunting:** Lobo ceniciento and Jabalí de roca resolve automatically from expedition Power and explain the threshold in the battle report.
- **Common threat:** Engendro de la Fisura Nv.1 unlocks after the first route threat and introduces PREPARAR MARCHA, archer count, hero selection and the real ATQ / DEF / VIDA / RUP / PODER comparison.
- **Uncommon threat:** Acechador de Ceniza Nv.3 has a real Emboscada trait: extra opening damage followed by exposed defense. Changing march composition changes the simulated result.
- **World boss:** Heraldo de la Fisura Nv.5 is visually distinct, has a live timer and uses semiautomatic combat. Basic exchanges are automatic; the selected hero can intervene once with a small role-consistent ability.
- Battle reports show both sides, remaining life, round exchanges and plain-language reasons for the outcome.
- Troop recruitment currently exposes **Arqueros only**. Combat troop families are internally extensible, but later families remain undisclosed and unavailable until their narrative unlocks.
- Progression principle is **find → understand → unlock**. Future troop families and PvP are not previewed in the player UI.
- The existing Breach Manuscript flow is unchanged and its future purpose remains unrevealed.
- Permanent mobile interaction QA: `qa/e2e-v0266-pve-combat.js`.

## v0.28 — base system for heroes, troops and marches
- A dedicated domain model now defines heroes, roles, affinities, base combat statistics, troop families and march composition independently from the final Hero Hall presentation.
- **Sir Aldric** is defined as **Tank** with **Paladines** affinity: +3% Defense applies only to Paladines present in the march. The affinity grants a bonus but never restricts what troops or heroes may be combined.
- **Lyra** is defined as **DPS** with **Arqueros** affinity: +3% Attack applies only to Arqueros present in the march and does not modify other troop families.
- Aldric/Lyra base combat statistics are inherited from the already-active v0.26.6 combat model; this iteration does not introduce new balance numbers for them.
- Arqueros keep their existing v0.26.6 troop stat profile. Paladines are structurally registered for affinity/march logic but remain hidden/reserved with combat stats deliberately unbalanced/unset until their own design pass, avoiding invented values.
- March state now has explicit hero and troop-composition structures while retaining legacy compatibility fields used by the current slice. The domain model accepts mixed troop composition and applies per-family affinities before combat resolution.
- Each integrated hero has two PvE skill slots and two PvP skill slots prepared with rank bounds I–V. Existing PvE abilities occupy the first PvE slot; future ranks/effects are not fabricated. Talent progression exposes an exclusive-choice structure without inventing the number or contents of talent tiers.
- Maelis remains outside this new hero-domain integration for a later pass; her existing slice behavior is preserved.
- Permanent focused QA: `qa/e2e-v027-hero-army.js`. Focused development preset: `hero-army-base`.

## v0.28 — integrated military progression layer
- The military loop is now treated as one progression layer: **Barracks → troop inventory/tiers → Hero Hall → individual hero profile → march preparation → expedition stats/combat**.
- Barracks progression is independent from Bastion progression. Bastion only gates how far the Barracks may develop; upgrading the Barracks never grants troops automatically.
- Archer tiers are persistent and coexist: T1 unlocks at Barracks 1, T2 at Barracks 4 and T3 at Barracks 10. Older tiers remain in the roster after higher tiers unlock.
- The canonical roster is tiered (family → tier → count). Legacy saves migrate existing generic Archers into T1 without deleting troops. A pure promotion operation exists so future troop promotion can move chosen quantities upward explicitly; no automatic conversion exists.
- Archer T1/T2/T3 combat profiles reuse values already present in the canonical v0.26.6 troop progression at the corresponding historical progression points (1/4/10). No new arbitrary combat numbers were introduced. Recruitment cost/time remains the existing generic Archer recruitment contract until tier-specific balance is formally defined.
- Only **Arqueros** remain player-facing/recruitable. Paladines and Brujos stay hidden/reserved in player UI.
- The Hero Hall is now a dedicated visual collection. Selecting a hero opens an individual character screen with the hero as the visual focus, visible level/power/role/affinity, equipment slots around the figure and separate Habilidades/Talentos tabs.
- Aldric and Lyra retain the canonical role/affinity model. Affinity is a bonus only, never a composition restriction. Maelis remains visually available where progression already unlocks her, but her definitive hero-system data remains deferred.
- March preparation supports up to three heroes structurally and tier-specific Archer quantities. Expedition ATQ/DEF/VIDA/RUP/Poder are calculated from the saved composition through the shared military domain model.
- The central global Power model remains unchanged; hero power, troop/march power and global account Power are presented as distinct concepts and are not added twice to the global total.
- Focused QA: `qa/e2e-v027-hero-army.js`, `qa/e2e-v027-military-circuit.js`, and the expanded Barracks recruitment regression.

## v0.29 — Códice de Eldoria + Relicario
- **Códice de Eldoria** is now a knowledge/discovery archive rather than a card manager. Its current sections are **La Brecha, Bestiario, Mundo and Personajes**, populated progressively from real state/discoveries.
- **Relicario** is the dedicated card/Reliquia system and is entered from the Códice. It has three mobile-first tabs: **Colección**, **Práctica** and **Duelo PvP**.
- Colección shows rarity, effect and state. Use/conserve remains intact: a normal Reliquia is consumed when used; an Indestructible remains in collection, enters cooldown and remains valid for practice/duel systems.
- Tutorial phase 1 deliberately hides board language: the first Reliquia teaches only what it is, rarity/effect and the use-vs-conserve decision.
- At five discovered Reliquias, **Maestre Orin** reveals side values and directs the player into Práctica by playing rather than through a rules wall.
- Práctica uses the existing 3×3 capture mechanics with temporary cards and no permanent collection loss. After the guided pass, practice remains repeatable against Orin.
- **Duelo PvP** is visible but explicitly locked/future; no matchmaking, rewards or fake PvP functionality are active.
- Chapter 7/9 relic objectives now route to the Relicario while the Códice objective remains knowledge-oriented.
- Focused QA preset: `relicario-v029`. Permanent regression: `qa/e2e-v029-codex-relicario.js`, plus updated legacy Codex/Chest and guided-practice coverage.

## v0.30 — depth, autonomy and systemic feedback
- Bastion I–X economy now includes four explicit development-priority decisions at Bastion IV, VI, VIII and IX. Each offers multiple viable routes, grants immediate strategic resources/Power and never blocks content or relies on artificial waiting.
- Guidance is reduced progressively from Bastion VI, lighter by VIII, and Bastion IX replaces step-by-step routing with general objectives so the player chooses order and approach.
- Total Power keeps the existing central model, adds a concise first-use explanation, visible gain feedback and restrained milestone ceremonies.
- Simulated leaderboards now expose three categories: Total Power, Corrupts defeated and Reliquia collection. The player and immediate rival are always visible; profiles are explicitly simulated.
- Battle reports now have exactly two reading levels: a clear summary and expandable full detail, while keeping hero intervention feedback visible.
- Spanish/English localization is selectable from visible settings, persists locally, and v0.30 UI/system text is audited to avoid mixed-language presentation in the new layer.
- The English audit also covers dynamic Arc I surfaces such as the World Boss heading; no mixed ES/EN labels remain in the certified path.
- Bastion II/III background completion now returns coherently to the kingdom view, keeping newly unlocked structures such as the Barracks visible after offline/task-recovery completion.
- HUD includes a compact UTC server/world clock prepared for future events without expanding the central play area.
- First audio layer uses lightweight ambient music and action feedback with persistent music/SFX toggles in settings.
- Ceremonies were reduced to real milestones; routine chapter completions use compact feedback.
- Códice/Relicario separation remains intact; PvP remains future/locked; no new Breach mechanics, heroes, buildings or alliance systems were added.
- Permanent focused regression: `qa/e2e-v030-depth.js`. Final certified v0.30 candidate: `dd2641a3d20751465c6f4e4c20fd350b6f33bbb5` (GitHub Actions run 1231). Full release certification passed: fresh-save Arc I/Bastion I–X, mobile interaction, frozen tester guard, Pages deploy, published Chromium verification and frozen tester URL verification.

## Known functional gaps / debt
- Bastion VI–X still need human pacing validation beyond automated reachability.
- Hero Hall still needs the eventual full hero skill/talent system; v0.26.6 only establishes a minimal combat-ability seam for world-boss intervention.
- Existing late-story combats outside the new layered PvE examples remain scaffolding and can migrate onto the same combat model later.
- Economy pacing to Bastion X needs human/player-experience validation even when automated reachability is green.
- The Relicario implements use/conserve decisions, Indestructible cooldown behavior, consumed-card discovery/Echo consequences and a 3×3 Practice board against Maestre Orin. PvP remains visibly future/locked. A wider relic economy and broader card pool remain future scope.
- Renewable/additional world nodes, hunting respawn, live timers and expanded canonical narrative/dialogue work remain implemented.

## Verification state
Normal development continues exclusively on `main`. The frozen external tester cohort is **Eldoria Closed Playtest T1 / 0.26.5-test.2** and is not an active product line. Its immutable reference and feedback protocol are documented in `SESSION_HANDOFF.md` and `TESTER_FEEDBACK_PROTOCOL.md`.

## Maintenance
Change this file only when implemented functionality, product scope, verified stable baseline, or known functional gaps change. Do not put transient next-task/HEAD information here.


## Historical implementation notes retained for context
These headings record when features entered the product. They are **not** active-version markers; the active development version is v0.30.

### v0.26.1 — coherence pass
- World nodes respawn by timer and relocate instead of reappearing at a fixed coordinate.
- Combat and expedition expose ATQ / DEF / APO for heroes, troops and the active march; Power remains a summary.
- Hero Hall and Codex receive high-contrast presentation.
- Bastion IX attack interaction is part of release acceptance; real player path must remain actionable.
- Codex / Orus / Relic Duel preserve the v0.21 clarity reference while staying integrated with current Arc I lore.


### v0.26.2 — Códice / Reliquias / Arcón
- Arcón is a dedicated full-screen view and no longer opens as a floating modal over the current map.
- Códice is a dedicated full-screen archive view with stronger hierarchy and card presentation.
- The v0.21 card principle is restored: using a normal relic consumes it for its immediate effect and leaves a weak Echo 1/1/1/1 for Duelo; conserving preserves its full N/E/S/O values and does not prevent later use.
- Indestructible is a quality, not a rarity. Indestructible relics survive activation and enter cooldown.
- The first Sello de Ceniza has a concrete outside-Duelo effect and a visible use/conserve decision.
- Orus offers guided N/E/S/O training before the formal Duelo milestone.
- At 5 discovered relics, the formal Duelo opens as a playable 3×3 board: select a card, choose a cell, compare touching N/E/S/O values, capture weaker adjacent cards, and gain +1 N/E/S/O in the central Breach-resonance cell.
- Formal Duelo unlock progress is based on 5 discovered relics; consumed discoveries still count but enter the hand as Echoes 1/1/1/1.
- qa/e2e-codex-chest.js protects dedicated views, use/conserve explanation, Indestructible cooldown semantics and Orus orientation training.


### v0.26.3 — guidance, Hero Hall and sequencing
- Salón de Héroes is a dedicated full-screen management surface. It combines hero role/stat reading, equipment and expedition formation instead of splitting them across modal flows.
- Bastion IX has a device-independent persistent attack objective once a valid march exists; the action sits outside the pannable world transform.
- Tutorial guidance extends beyond the opening chapters into Forge/Devorador, Códice/Orus, Nareth and the Bastion IX march trial.
- The first Códice visit teaches the relic economy through Orus: outside-Duelo effects, N/E/S/O, use vs conserve, consumed Echoes and Indestructible cooldown.
- Narrative reveals now respect discovery order: the Forge does not explain Aether before the material is found, and Maelis is not introduced before reaching Nareth. Duplicate explanatory beats were reduced.
- Timed construction/upgrade costs are deducted when work starts. Completion applies the result only; legacy unpaid in-flight tasks remain migration-safe.
- v0.26.3 acceptance adds mobile 390×844 checks for march/attack reachability and start-time resource spending while preserving the full Arc I regression.


### v0.26.4 — unified visual / UX system
- Functional rules are unchanged; this milestone is a presentation/navigation pass with explicit parity protection.
- Bottom navigation is normalized to Ciudad, Mundo, Héroes, Arcón and Códice; Ranking remains available in the HUD.
- Routine city/world interactions use a shared contextual bottom-sheet pattern while deep management remains full-screen.
- Forge joins Héroes, Arcón and Códice as a full-screen management view.
- Arcón presents an inventory grid plus non-destructive in-screen item details.
- World node categories are visually differentiated without changing node behavior or rewards.
- Mobile map movement uses direct pointer tracking, restrained inertia and dynamic limits while preserving object taps.
- UI scale, spacing, safe areas, touch targets, panel hierarchy and restrained color semantics are centralized in the v0.26.4 visual layer.
- `UI_PARITY_0264.md` and `qa/e2e-v0264-ui-parity.js` define and protect the before/after contract.


### v0.26.5 — guidance, infrastructure and collection onboarding
- Contextual progression guidance now names missing resources, quantities and where to obtain them, and can direct the player to the relevant world node or city building.
- Barracks has two distinct loops: infrastructure upgrades and timestamp-based troop recruitment. Recruitment persists through view changes/offline time.
- Bastion IV requires level-2 Aserradero, Cuartel and Granero, establishing the infrastructure gate before further fortification growth.
- Lyra recruitment is the explicit onboarding moment for the Heroes layer.
- The Devourer has a post-combat narrative consequence before the Aether material/Forge loop.
- Arcón uses object-specific visual identity; Códice remains the exclusive Reliquia/card surface.
- All deep management surfaces follow the mobile vertical-scroll contract.
- Códice now frames discovery, conservation and activation as its core loop, and its first Duelo is a guided 3×3 training match with five temporary loan cards that never mutate the permanent collection.
- The world frontier remains one pannable map but uses a larger spatial footprint and wider node distribution to communicate future 4X scale without pretending the final shared-world system exists.


## Canonical MVP visual target — 2026-09-22
- Art direction is now explicitly part of the MVP product definition: **stylized semi-realistic dark fantasy**, premium/solemn but mobile-readable.
- Canonical city target: monumental vertical Valoria rebuilt over imperial ruins, with distinct interactive landmarks and visible reconstruction.
- Canonical world target: readable 4X territory with routes, settlements, resources, marches, threats and integrated Breach corruption.
- La Brecha uses a violet/magenta dimensional-corruption language and is a world phenomenon, not a generic portal.
- Final-production visuals belong primarily to the future Unity phase; the web slice only needs enough presentation fidelity to validate gameplay/UX against this direction.
- Source: `docs/MVP_ART_DIRECTION.md`.


## v0.27 — chapter missions, rewards and accelerators
- The old persistent guided-step block has been replaced by a compact chapter mission system that preserves guidance without covering the playfield.
- Progression is organized into chapter objectives with numeric progress where appropriate, contextual destinations and chapter-completion rewards.
- The mission panel is available from a compact control and opens as a mobile-budget drawer rather than a full-screen obstruction.
- Resource collection objectives require meaningful quantities rather than a single interaction.
- The Códice is integrated into the main progression instead of being treated as optional side content.
- Universal accelerators are introduced as a real inventory/currency for eligible construction, upgrade and recruitment timestamp tasks. Gathering cannot be accelerated.
- Mission/chapter rewards are idempotent and persist across reloads; analytics record chapter starts, mission completion, chapter completion, rewards and accelerator use.
- Bastion VI–X economy costs were retuned to preserve resource pressure while remaining reachable through production, gathering and chapter rewards.
- Permanent QA: `qa/e2e-v027-chapters.js`, plus the uninterrupted Arc I sweep with v0.27 economy pacing.
- Frozen tester build remains untouched and is not a source for v0.27 development.


## v0.27 owner-test correction — 2026-09-22
- Chapter II's **Engendro de la Fisura** is reachable at Bastion II, matching the mission sequence.
- Arcón classifies stored content by utility with dedicated tabs for Aceleradores, Materiales, Equipo and Especiales.
- Universal accelerators awarded by chapters/missions are visible in Arcón as counted consumables; applying them still requires a compatible active timed task.
- The correction is protected by mobile Playwright regression in `qa/e2e-v027-owner-feedback.js`.
