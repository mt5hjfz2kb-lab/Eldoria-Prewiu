# Eldoria — Changelog

## v0.27 — 2026-09-22

- Replaced the persistent guided-step overlay with a compact chapter mission system and mobile-safe mission drawer.
- Added chapter objectives with numeric progress, contextual destinations and one-time chapter rewards.
- Integrated Códice objectives into the main progression path.
- Introduced universal accelerators for eligible construction, upgrade and recruitment timestamp tasks; gathering remains non-accelerable.
- Added persistent mission/chapter analytics and idempotent reward handling.
- Retuned late Bastion economy costs and aligned the uninterrupted Arc I sweep with v0.27 pacing.
- Added permanent chapter/speedup coverage in `qa/e2e-v027-chapters.js` while preserving all prior regression suites.
- Frozen tester build remains unchanged.


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

Latest development milestone: **v0.27**. Certification status for the current `main` HEAD is tracked in `SESSION_HANDOFF.md` / the Pages workflow.


## v0.27 owner-test hotfix — Spawnling / Arcón
- Fixed Chapter II progression so the Engendro de la Fisura is available before Bastion III.
- Added Arcón category tabs and made Universal Speedups visible there with quantities and purpose.
- Added regression coverage for the exact owner-reported flow.
