# Eldoria — PROJECT STATE
Updated: 2026-09-24

This file describes **functional project state only**. Operational HEAD/current task belongs in `SESSION_HANDOFF.md`; permanent process belongs in `AGENTS.md`.

## Active product
- A **separate Unity source slice** is staged in `Unity/` (Editor 6000.3.23f1): Valoria → frontera → bosque/optional corrupt scout → return → sawmill; core state, scenes, procedure-built visual study and tests/build entry points. On 2026-09-25 the Windows Editor and standalone player were compiled and played successfully through the first slice, including persistence and the 2452→2622 Power transition. The initial standalone magenta-render issue was corrected with a player-safe URP runtime material. Automated EditMode/PlayMode certification, mobile validation and the remote Unity-node workflow remain pending. `UNITY_CORE_CONTRACT.md` records the deliberate Power/March decisions. Web v0.32.0 continues as the playable canonical product.
- Runtime/API milestone: **v0.32.0**.
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

## v0.32.0 — Hospital, wounded troops and March reconciliation
- Bastion X now introduces a real Hospital loop after the Herald battle: PvE can create wounded troops, while permanent PvE deaths remain explicitly **0**.
- Wounded troops remain owned by Valoria but are removed from the available military roster until treatment completes; Troops and March explain this distinction directly.
- Hospital treatment is a canonical timed task, persists across reload/offline time, and restores recovered troops to the available roster when complete.
- March composition is reconciled against the actually available roster only when military state changes, preventing wounded troops from leaking back into expeditions without destabilizing unrelated UI renders.
- The Hospital is first revealed through the Bastion X Herald flow and the chapter/finale path requires recovery before the final assault.
- Future multiplayer semantics stay separated from the current slice: solo March exposes an operation-contribution contract, PvP casualty policy is reserved, and no fake rally/join UI is introduced.
- Old saves migrate medical state safely without altering existing resources, progression or owned troops.
- Dedicated regression: `qa/e2e-v032-hospital.js`, included in targeted/regression certification; the uninterrupted Arc I sweep now covers Herald wounds → Hospital treatment → recovered expedition readiness.
- Frozen tester output remains unchanged.

## v0.31.1 — Relicario Practice visual polish
- Práctica now preserves a real portrait-card silhouette in both hand and board instead of flattening placed cards into square or text-block representations.
- N/S/E/O are anchored consistently to the top, bottom, right and left card edges, including mobile layouts.
- Practice reuses the current Relicario rarity language: Common gray, Rare blue, Epic purple and Legendary orange, with restrained medieval-dark framing rather than neon treatment.
- Hand selection, board placement and capture states have clearer feedback while preserving the underlying Practice rules and collection state.
- The historical v0.21 card direction was used only as a visual reference for proportion, framed presence and perimeter values; current v0.31 Relicario data, rarity and reveal semantics remain canonical.
- Dedicated mobile and desktop Practice coverage validates portrait geometry, four-rarity legibility and N/S/E/O alignment. The pre-promotion candidate passed full local-equivalent certification, uninterrupted fresh-save Arc I, frozen tester guard, Pages deployment and published Chromium verification in GitHub Actions run **1433** at `d73cf809353864dfe05e5bfd4052383bcf191d2d`.
- This patch changes presentation/UX only; card catalog, acquisition probabilities, effects, economy, progression and frozen tester output are unchanged.

## v0.31.0 — Relicario collection, rarity, acquisition and card-use system
- Relicario is a first-class system fully separate from Códice. Códice remains knowledge/discovery only; Relicario owns Reliquia collection, use/conserve decisions, reveal ceremony, Practice and future Duel behavior.
- The initial collection contains 15 Eldoria-native cards: 6 Common, 4 Rare, 3 Epic and 2 Legendary, each with fixed N/S/E/O values, exact 4X use effects and optional card-specific Duel abilities.
- Ordinary world-enemy victories perform exactly one Relicario rarity roll: Common 0.10%, Rare 0.05%, no card 99.85%. Hunting is explicitly 0%. Epic and Legendary cards are excluded from ordinary drop.
- Card pools unlock only after the represented content has actually been discovered. Arc I progression no longer depends on rare random drops.
- Epic random-reward support exists structurally for a future special reward source; no current ordinary source grants Epics. Legendary cards exist in data/design but have no active acquisition source.
- Indestructible is an additional property, not a rarity. Its rarity-specific chance is evaluated after card selection; a player can own at most one Indestructible copy of a concrete card, while normal duplicates remain repeatable. Duplicate Indestructible outcomes reroute to another eligible card of the same rarity when possible.
- Every obtained card uses a dedicated reveal ceremony with rarity-colored treatment, N/S/E/O, exact 4X effect, Duel ability when present and a second-stage INDESTRUCTIBLE reveal when applicable.
- Normal cards consume one copy when used; Indestructible cards keep the card and apply cooldown/effect state. Temporary effects persist with timestamps and continue correctly across reload/offline time.
- The first Relicario experience is integrated into the real Bastion VII flow: first card, reveal, progressive explanation of rarity/use/conserve/Indestructible and later N/S/E/O + Practice teaching. QA mode does not overwrite tutorial state.
- Duel support now preserves card-specific abilities used by this collection, including Guardia, Distorsión and Corrupción, without making abilities automatic by rarity.
- Dedicated Relicario coverage validates catalog/rarities, ordinary-drop probabilities, hunt exclusion, one-roll semantics, discovery gating, normal duplicates, Indestructible uniqueness/rerouting, use persistence, reveal UI, mobile/desktop surfaces and Códice separation.
- The pre-promotion functional candidate passed the full local-equivalent certification, uninterrupted fresh-save Arc I, frozen tester guard, Pages deploy and published Chromium verification in GitHub Actions run **1418** at `665690a17e20397725e51354693eecea56ae893e`.

## v0.30.4 — mobile UX clarity, continuity and system comprehension
- The owner mobile-feedback block is integrated without expanding Arc I scope: the focus is clarity, navigation, feedback and continuity of actions.
- A discreet in-game build label exposes the exact version without consuming meaningful HUD space.
- Large resource values use readable K / M / B abbreviations while internal resource arithmetic remains unchanged.
- Poder de expedición is explained as a separate concept from Poder Total, with contextual guidance toward troops, heroes, March composition and equipment.
- March is a first-class discoverable destination with an explicit exit/back path. World actions that send an expedition now communicate travel with a short visible march animation before resolution.
- Cuartel remains exclusively responsible for recruiting/upgrading troops. Its early-game presentation is simplified and mobile stacking/hitboxes were corrected so neighboring buildings cannot intercept recruitment actions.
- Arcón keeps objects/materials/equipment distinct from Códice knowledge and Relicario Reliquias, with clearer grouping and contextual accelerator teaching.
- Forge now explains its purpose, locked state, required anomalous material, output and relationship to hero equipment without adding unrelated systems.
- Ambient audio was reduced/reworked so it reads as restrained dark-fantasy ambience rather than continuous noise.
- Códice and Relicario remain fully independent peer systems; no v0.30.4 correction re-nests one inside the other.
- Dedicated owner UX coverage is qa/e2e-v0304-owner-ux.js. The uninterrupted Arc I fresh-save and late-progression suites were synchronized with the new march-animation and full-node gathering semantics rather than bypassing them.
- Pre-promotion functional certification passed GitHub Actions run **1386** on commit `7c7289c08b12c7cd610a6971e199aa8c6c783332`, including the full local-equivalent gate, frozen tester guard, Pages deploy and published Chromium verification. The version-promotion commit must pass the same workflow before final release claim.

## v0.30.3 Códice / Relicario independence hardening
- Códice and Relicario are now peer destinations in primary navigation, not parent/child surfaces.
- Códice contains only world knowledge and discovery: La Brecha, Bestiario, Mundo and Personajes.
- Relicario remains the transversal Reliquia system for Colección, Práctica and the visibly locked future Duelo PvP.
- Both systems now explain their purpose independently on first use and in-screen, while preserving current saves and Chapter VII progression.
- Mobile QA and uninterrupted fresh-save Arc I regression enforce that no Relicario portal remains inside Códice and no Códice back-link remains inside Relicario.

## v0.30.3 — independent Códice / Relicario correction
- **Códice and Relicario are now completely independent peer destinations in primary navigation.** Neither contains a portal/back-link that presents the other as a child or parent.
- Códice is knowledge-only: La Brecha, Bestiario, Mundo and Personajes, with a first-open explanation focused on discovery and world understanding.
- Relicario is the transversal Reliquia system: Colección, Práctica and future/locked Duelo PvP, with a separate first-open explanation of rarity, effect, state and use/conserve decisions.
- Existing saves are migration-safe: players who had Códice unlocked automatically receive the independent Relicario destination.
- Mobile navigation now supports the additional first-level Relicario destination and its touch/overflow contract is covered by QA.
- Chapter VII and the uninterrupted fresh-save Arc I route use the independent Relicario entry point; no gameplay/reward semantics were removed.
- Integral certification passed on GitHub Actions run 1331 before metadata/documentation promotion.

## v0.30.2 Arc I completion hardening
- Bastion VI → VII now uses one canonical equipment-ownership rule across mission progress, equipped gear and the Bastion gate; equipping the forged Aether Blade cannot revert the mission counter.
- Ether Devourer now uses modeled PvE preparation, matchup forecast, trait resolution and the shared post-combat report instead of the legacy instant readout.
- March accepts Aldric as a valid single-hero expedition before the late-game March Trial; the two-hero requirement remains reserved for the trial stage.
- Lyra's affinity is rendered from the canonical hero/troop domain as +3% Archer Attack across Hero and March surfaces.
- Decorative world labels no longer intercept node taps; active building actions are raised above neighboring city hitboxes, including mobile.
- Player-facing development placeholders in the audited Arc I surfaces were replaced with in-world/locked-state language.
- Permanent regression coverage includes the exact forge → equip → mission 1/1 → Bastion VII sequence plus mobile hitbox/combat checks.

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
- Arcón is a bottom-navigation inventory for objects/materials/equipment; cards and relics are managed strictly in the independent Relicario; Códice is knowledge-only.
- World activity now yields named upgrade materials (ancient iron brace, ash hide, stone tusk) used by production-building upgrades, connecting exploration to city progression.
- Bastion VII unlocks Códice and Relicario as separate peer systems. The first Reliquia is managed in Relicario; the Códice remains knowledge-only. Indestructible remains a quality, never a rarity.
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
- The Breach Manuscript still preserves its unrevealed future purpose, but v0.30.1 now presents its acquisition as a short narrative ceremony with Aldric's reaction and a restrained Valoria/Breach mystery hook.
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
- **Relicario** is the dedicated card/Reliquia system and is a first-level destination independent from the Códice. It has three mobile-first tabs: **Colección**, **Práctica** and **Duelo PvP**.
- Colección shows rarity, effect and state. Use/conserve remains intact: a normal Reliquia is consumed when used; an Indestructible remains in collection, enters cooldown and remains valid for practice/duel systems.
- Tutorial phase 1 deliberately hides board language: the first Reliquia teaches only what it is, rarity/effect and the use-vs-conserve decision.
- At three discovered Reliquias, **Maestre Orin** reveals side values and directs the player into Práctica by playing rather than through a rules wall.
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

## v0.30.1 — UX clarity, responsibility boundaries and novice-player QA
- Barracks, Troops, Heroes and March are now separated by responsibility: Barracks recruits/upgrades, Troops shows owned army, Hero Hall manages characters only, and March is a dedicated expedition-composition screen.
- First-entry micro-explanations were added for Barracks, Troops, Hero Hall and March without long text walls.
- Hero Hall initial view is hero-only, with portrait collection, power/level summary and visible undiscovered slots; troop inventory and march composition no longer appear there.
- Individual hero profiles keep full-character focus, Power, level, role, affinity, Skills/Talents tabs and prepared equipment slots.
- March preparation now uses explicit hierarchy: Heroes → Troops → Composition → March Power → Confirm. Affinity copy explains concrete march bonuses where applicable.
- Building labels are normalized to building name + Level X only; production/timers/resources moved out of city labels.
- Mission panel discoverability now includes a compact first-use affordance, chevron state and touch-safe close control without growing the mobile HUD.
- Universal Speedups now teach their purpose on first acquisition and expose a clearly visible ACCELERATE picker on compatible timed tasks; gathering remains non-accelerable.
- Breach Manuscript acquisition is promoted to a short narrative ceremony with item presentation, Aldric reaction and a restrained mystery hook linking Valoria and the Breach without expanding Breach mechanics.
- Important reward presentation was audited and strengthened proportionally for manuscript/Aether-style rare milestones while routine rewards remain compact.
- Contrast/readability was hardened across dialogs, tutorials, missions, heroes, troops, march, Codex, Reliquary, Chest and Settings; touch targets were raised where the mobile pass found weak discoverability.
- English localization was extended across the new UX clarity surfaces.
- Permanent novice-player QA rule added to AGENTS.md and QA_AND_DEPLOY.md: **Discoverability → Comprehension → Interaction → Feedback → Next step**. Technical function alone is no longer sufficient for acceptance.
- Permanent focused regression: `qa/e2e-v0301-ux-clarity.js`, plus updated military/chapter/fresh-save regressions.
- Final certified v0.30.1 release candidate: `24855ee2a862676f36f9185be5ede8fc42dfa860`, GitHub Actions run 1275. Full integral gate green including fresh-save Bastion I–X, novice-player/mobile UX, frozen tester guard, Pages deployment, published Chromium verification and frozen tester URL verification.

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
These headings record when features entered the product. They are **not** active-version markers; the active development version is v0.32.0.

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
