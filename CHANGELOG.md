# Eldoria — Changelog

> History only. Current operational state is defined by `SESSION_HANDOFF.md`; current functional state is defined by `PROJECT_STATE.md`.

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
