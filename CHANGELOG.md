# Eldoria — Changelog

> History only. Current operational state is defined by `SESSION_HANDOFF.md`; current functional state is defined by `PROJECT_STATE.md`.

## v0.30.4 — 2026-09-24

- Completed the owner mobile UX clarity block without expanding Arc I content scope.
- Added a discreet exact build label and K/M/B formatting for large resources.
- Clarified Poder de expedición versus Poder Total and made March a discoverable first-class destination with a clear exit path.
- Added short visible world-march travel feedback before expedition actions resolve.
- Simplified the early Cuartel experience and fixed mobile stacking/hitbox conflicts that could block recruitment.
- Reorganized Arcón presentation while preserving strict boundaries with Códice knowledge and Relicario Reliquias.
- Improved contextual accelerator teaching and Forge locked/ready/material/output/equipment explanations.
- Reworked ambient audio toward restrained dark-fantasy ambience.
- Preserved the independent Códice / Relicario architecture introduced in v0.30.3.
- Added/extended v0.30.4 owner UX, mobile and interaction regression coverage.
- Synchronized uninterrupted fresh-save QA with the real march-animation and full-node gathering semantics; pre-promotion full certification passed GitHub Actions run 1386.

## v0.30.3 — 2026-09-24

- Promoted Códice and Relicario to fully independent peer systems in primary navigation instead of exposing Relicario from inside Códice.
- Removed the Códice → Relicario portal and the Relicario → Códice parent/back affordance.
- Added explicit in-screen purpose explanations: Códice is world knowledge/discovery; Relicario is the transversal Reliquia collection/use/practice system.
- Added separate first-use onboarding for Códice and Relicario while preserving existing saves and the current three-Reliquia Practice threshold.
- Added backward-compatible `relicarioUnlocked` state so existing Bastion VII+ saves gain the independent destination without reset.
- Updated mobile navigation and all affected QA to enforce the new separation, including uninterrupted fresh-save Arc I progression through Chapter VII.
- Corrected stale canonical documentation that still described cards/relics as belonging to Códice.

## v0.30.2 — 2026-09-23

- Fixed the Bastion VI → VII softlock by making forged equipment remain canonically owned when equipped; mission, counter and gate now agree.
- Converted the Ether Devourer from the legacy instant combat readout to the shared modeled PvE preparation, forecast, trait and battle-report flow.
- Removed the visible `APO undefined` failure and strengthened combat reports with participants, damage and matchup reasoning.
- Allowed valid Aldric-only March composition before Lyra exists; retained the two-hero requirement for the later March Trial.
- Routed expedition objectives to the March screen and clarified Power versus matchup forecast.
- Unified Lyra's +3% Archer Attack affinity from the canonical hero/troop domain and removed the leaked internal `archer` label.
- Prevented decorative world labels from stealing taps and raised active city action hitboxes, covering the Bastion III Boar and Bastion VI Forge cases.
- Replaced audited player-visible development placeholders/implementation language with in-world or locked-state copy and tightened the Bastion IV/Devourer narrative cadence.
- Added exact forge → equip → 1/1 mission → Bastion VII regression plus mobile checks for March, Boar, Forge, Lyra and Devourer.
- Full release certification remains governed by `npm run validate:local`, uninterrupted fresh-save Arc I and the published Pages browser check.

## v0.30.1 — 2026-09-23

- Separated Barracks, Troops, Heroes and March into clear player-facing responsibilities.
- Rebuilt Hero Hall as hero-only collection and moved expedition composition to a dedicated March screen.
- Added short contextual onboarding for Barracks, Troops, Hero Hall and March, including concrete affinity explanations.
- Improved mission-panel discoverability, touch targets and compact mobile behavior.
- Added first-acquisition Speedup education and an explicit ACCELERATE picker on compatible timed processes.
- Normalized city building labels to name + level only.
- Upgraded the Breach Manuscript into a meaningful narrative discovery ceremony without adding new Breach mechanics.
- Strengthened proportional reward ceremonies, contrast, readability and English coverage across affected surfaces.
- Added permanent novice-player QA acceptance: Discoverability → Comprehension → Interaction → Feedback → Next step.
- Added `qa/e2e-v0301-ux-clarity.js` and updated military/chapter/fresh-save regressions.
- Full integral release gate passed on the correction candidate before patch metadata promotion.

## v0.30 — 2026-09-23

- Added four meaningful development-priority choices across Bastion IV, VI, VIII and IX, replacing forced-wait style progression pressure with immediate strategic trade-offs.
- Reduced tutorial guidance progressively from Bastion VI–VIII and converted Bastion IX to general objectives rather than step-by-step routing.
- Kept the existing Total Power model and added a concise first-use lesson, visible gain feedback and restrained milestone moments.
- Expanded simulated rankings to Total Power, Corrupts defeated and Reliquia collection, always showing the player and immediate rival.
- Reworked combat reports into exactly two reading levels: clear summary and full details.
- Added persistent Spanish/English settings for the new/systemic layer, a compact UTC server clock and first lightweight ambient/SFX audio controls.
- Audited ceremonies so routine progress uses compact feedback and major presentation remains reserved for meaningful milestones.
- Preserved Códice/Relicario v0.29 behavior, locked/future PvP and the existing Breach system; added no alliances, new heroes, building proliferation or Breach expansion.
- Added `qa/e2e-v030-depth.js`, updated legacy QA compatibility, and certified the complete release gate including Arc I, mobile interaction, frozen tester guard, Pages deployment and published Chromium verification.

## v0.29 — 2026-09-23

- Split the former mixed card/Codex surface into two distinct systems: **Códice de Eldoria** for world knowledge and **Relicario** for Reliquia/card management.
- Added Códice sections for La Brecha, Bestiario, Mundo and Personajes, populated from actual progression flags.
- Added Relicario tabs for Colección, Práctica and visibly locked/future Duelo PvP.
- Reworked relic onboarding into two phases: first relic teaches rarity/effect/use-vs-conserve only; five discoveries unlock side values and Maestre Orin's board teaching.
- Preserved normal-card consumption and Indestructible cooldown/non-consumption semantics.
- Converted the existing 3×3 board into lossless Practice against Orin; temporary teaching cards never enter or mutate the permanent collection.
- Routed chapter relic decisions/training toward Relicario while preserving Códice as the knowledge destination.
- Added focused `relicario-v029` QA and permanent `qa/e2e-v029-codex-relicario.js` coverage, then certified regression, mobile fresh-save, frozen tester guard and published Chromium verification.


## v0.28 — 2026-09-23

- Consolidated the military progression layer as one playable system: Barracks → persistent Archer tiers → army inventory → Hero Hall → individual hero profile → tier-aware march preparation → expedition stats/Power → combat.
- Barracks progression now independently unlocks Archer T1/T2/T3 at levels 1/4/10; older tiers persist and Barracks upgrades never generate or auto-convert troops.
- Added canonical tier-aware troop roster and explicit promotion seam without inventing promotion costs or future troop-family balance.
- Reworked Hero Hall into a visual collection with individual Aldric/Lyra character profiles, visible hero Power/role/affinity, equipment slots and Habilidades/Talentos surfaces.
- March composition now supports explicit tier quantities and up to three heroes structurally, with shared affinity and expedition-stat calculation as the single source of truth.
- Preserved hidden/unbalanced future troop families and deferred Maelis' definitive hero-system data.
- Certified focused military QA, regression, mobile fresh-save Arc I, frozen tester guard, deployment and published Chromium verification before promoting the milestone from v0.27 to v0.28.


## v0.27 — 2026-09-22

- Replaced the persistent guided-step overlay with a compact chapter mission system and mobile-safe mission drawer.
- Added chapter objectives with numeric progress, contextual destinations and one-time chapter rewards.
- Integrated Códice objectives into the main progression path.
- Introduced universal accelerators for eligible construction, upgrade and recruitment timestamp tasks; gathering remains non-accelerable.
- Added persistent mission/chapter analytics and idempotent reward handling.
- Retuned late Bastion economy costs and aligned the uninterrupted Arc I sweep with v0.27 pacing.
- Added permanent chapter/speedup coverage in `qa/e2e-v027-chapters.js` while preserving all prior regression suites.
- Frozen tester build remains unchanged.
- Owner-test correction: Chapter II's Engendro de la Fisura is available before Bastion III; Arcón now groups stored content by utility and exposes Universal Speedups with quantity/purpose; regression coverage protects the flow.


## v0.26.6 — 2026-09-22

- Added layered PvE progression: Power-only hunting, common five-stat march preparation, uncommon trait/counter-play and a timed semiautomatic world boss.
- Added Engendro de la Fisura Nv.1, Acechador de Ceniza Nv.3 with real Emboscada, and Heraldo de la Fisura Nv.5 with one hero intervention.
- Added real ATQ / DEF / VIDA / RUP / PODER combat resolution plus readable post-battle explanations and round logs.
- Recruitment now presents Arqueros as the only currently available troop family; future families remain hidden while the combat architecture is extensible.
- Preserved the existing Breach Manuscript flow and kept PvP unimplemented.
- Added permanent 390×844 interaction/regression coverage in `qa/e2e-v0266-pve-combat.js`.


## v0.25.1 — 2026-09-21

- Corrected the owner playtest block: canonical Aldric portrait/dialogues, HUD Arcón de Valoria and manuscript inventory, Hero Hall close control, contextual Bastion upgrades, correct building levels, visible costs and timed upgrades, Power explanation, denser 4X horizon cues and explicit elite visibility.
- Added permanent real-touch regression coverage for the corrected inventory, building and elite flows.
- Updated the uninterrupted fresh-save Arc I certification for timed production upgrades; 16/16 checkpoints pass.
- Certified and deployed the public build in GitHub Pages run 793, including published Chromium verification.


## v0.25.0 — 2026-09-21

- Closed the external-playtest loop with an unmistakable Arc I completion screen, compact survey, copyable session summary, JSON export and full local reset.
- Added a structured local session record and stable QA access to the end-state summary.
- Fixed a real mobile late-progression blocker where the Granary intercepted the Forge action; also kept the first Sawmill action inside the mobile viewport.
- Added stable selectors for primary navigation, profile, Power and playtest closeout actions.
- Made QA setup reproducible with a lockfile, `npm run qa:setup`, generated-output ignores and a single `npm run validate:local` gate.
- Established the large owner-feedback delivery protocol: reproduce → coherent batch → targeted QA → full fresh-save gate → one push → published verification.
- Validation target: complete Arc I from a clean save on 390×844 touch, submit the final survey and verify the exported final state.

## v0.24.1 — 2026-09-21

- Certified the Bastion I–X Arc I scaffold, renewable late-game resource route, mobile world-node interactions and published Chromium verification in run 749.

## v0.23.x — 2026-09-19

- Consolidated object-local kingdom/world actions, offline-safe timestamp tasks, independent production-building levels, equipment replacement safety and the uninterrupted fresh-save Arc I regression gate.
