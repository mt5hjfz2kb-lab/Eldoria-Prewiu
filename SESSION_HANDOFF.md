# Eldoria — SESSION HANDOFF

Updated: 2026-09-24

Permanent rules: `AGENTS.md`. Functional state: `PROJECT_STATE.md`. Detailed QA: `QA_AND_DEPLOY.md`. History: `CHANGELOG.md` only.

## Current working state
- Branch: `main`, only active development line.
- Active development version: **v0.30.4**.
- Verify the real live HEAD at session start; never infer it from chat/history.
- Canonical editable source: `v0220/index.html` + `v0220/js/`; `v0220` is a compatibility directory name.
- Generated development output: `playtest/`; never edit it directly.
- Normal development URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/
- QA Launcher URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/?qa=1&launcher=1

## Current v0.30.4 product state
- **v0.30.4 is the active development patch. v0.30.3 remains the previous certified baseline, not the active development version.**
- This patch closes the owner's mobile UX clarity block: exact version visibility, K/M/B resources, expedition-power teaching, first-class March access/exit, short world-march travel feedback, early Barracks simplification, Arcón organization, contextual speedup teaching, Forge clarity and restrained ambient audio.
- Permanent responsibility boundaries remain unchanged: Códice = knowledge; Relicario = Reliquias; Arcón = objects/materials/equipment; Cuartel = recruit/upgrade troops; Héroes = characters; Marcha = expedition composition; Poder Total ≠ Poder de expedición.
- The v0.30.4 owner UX suite passes. The full Arc I fresh-save was updated only where the player's real semantics changed (world-march animation and full-node gathering) and the pre-promotion candidate passed GitHub Actions run **1386** at `7c7289c08b12c7cd610a6971e199aa8c6c783332`.
- Final certification for the promoted version is the normal Pages workflow on the version-promotion commit; do not claim v0.30.4 certified from run 1386 alone.
- Continue to edit only `v0220/index.html` + `v0220/js/`; `playtest/` is generated and `tester-v0265/` remains frozen.

## Current v0.30.3 product state
- Códice and Relicario are now independent peer destinations in the main navigation. Códice is knowledge-only; Relicario owns Reliquia collection/use/practice. Their onboarding is separate and old cross-navigation portals were removed.
- Current stabilization block is dedicated to making the existing Arc I completable end-to-end from a clean save; no large new system was added.
- Códice and Relicario are separate first-level systems: Códice = knowledge/discovery; Relicario = transversal Reliquia collection/use/practice. No nesting or parent/back relationship remains.
- Primary navigation exposes independent CÓDICE and RELICARIO buttons once unlocked; legacy saves with Códice unlocked migrate to Relicario unlocked automatically.
- Each system has independent first-use teaching. Códice explains world knowledge; Relicario explains rarity/effect/state, use/conserve, Practice and future PvP.
- Fresh-save Arc I certification with this architecture passed GitHub Actions run 1331; final metadata/docs promotion is v0.30.3.
- VI→VII equipment ownership, Devorador modeled combat, Bastion II single-hero March, Lyra affinity consistency, world/building hitboxes and player-facing development text are hardened in v0.30.2.
- qa/e2e-v0302-arc1-stability.js covers the reported blockers and the canonical fresh-save test equips the Hoja de Éter before attempting Bastion VII.

- Chapter mission system and universal accelerators are active.
- Arcón includes utility tabs, including Aceleradores.
- Chapter II Engendro is reachable/visible in its intended sequence.
- Layered PvE remains active; only Arqueros are player-facing/recruitable. Relicario shows Duelo PvP only as a locked future tab; no PvP functionality is active.
- Hero/troop/march base domain is now active for Aldric + Lyra: roles, affinities, inherited base stats, skill-slot/rank schema, exclusive-talent seam, troop roster and explicit march composition. Paladines are reserved structurally but still hidden/unbalanced; Maelis remains for a later integration pass.

## Versioning rule now in force
- Minor `v0.X` advances only for a consolidated, validated functional/playable milestone.
- Corrections, bugs, visual polish and balance inside that milestone use `v0.X.Y`.
- Agents choose the target version when a substantial block starts and promote only after integration + validation; the owner does not need to request the bump.
- The integrated military layer promoted development to v0.28. The subsequently consolidated Códice/Relicario separation is the **v0.29** milestone.

## QA workflow now in force
Three separate levels exist:
1. **Focused** — exact system preset / dedicated test for small changes.
2. **Segment** — coherent progression block preset for medium changes.
3. **Integral** — `npm run validate:local` + uninterrupted fresh save for milestones, progression/economy/sequencing changes and release candidates.

Do not automatically replay all of Arc I for every small correction. Escalate QA according to risk, while keeping the integral gate mandatory before important stable releases.

The development-only QA Launcher is injected during `playtest/` generation and appears only with `?qa=1`. Its storage layer isolates the canonical save key to a QA-only save, so presets/fresh QA do not alter normal saves. Shared fixture definitions live in `v0220/js/qa-fixtures.js` and should be reused by Playwright where practical.

Current focal presets: Héroes + Tropas + Marcha, Engendro, Fisura/Lyra, Forja/Devorador, Códice/Relicario, Relicario v0.29, Maelis, marcha/ataque, Heraldo semiautomático, capítulos v0.27, aceleradores and final Bastión X. Segment presets: VI→VIII and IX→X.

## Permanent owner delivery rule
Every future implementation delivery inherits the repository-level owner link contract from `AGENTS.md` / `QA_AND_DEPLOY.md`:
- include **🎯 Probar esta mejora** with the appropriate focused development QA state whenever reasonable;
- include **🧩 Probar tramo** when the change spans a meaningful progression/system block;
- always include **🎮 Jugar completo** with the normal development build URL;
- if a reasonable focused preset does not yet exist for a new feature, creating/adapting one is part of that feature's implementation;
- these links never replace automated QA and must never point at the frozen tester snapshot.

This is permanent cross-session behavior, not a one-session preference.

## Frozen tester isolation
- **Eldoria Closed Playtest T1 / 0.26.5-test.2**.
- Frozen integration commit: `df618e86be9da399bb827d5e6cebc3f13e55ff97` (current re-frozen snapshot after the final-survey contrast hotfix).
- URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/tester-v0265/
- Frozen research output only; never use as source and never inject the QA Launcher into it.

## Latest development block
- v0.30 deepens existing systems without expanding La Brecha.
- Four real development-priority choices are active at Bastion IV, VI, VIII and IX; they are immediate strategic trade-offs, not wait gates.
- Guidance tapers from VI–VIII and Bastion IX uses general objectives rather than step-by-step instructions.
- Existing Total Power remains canonical; v0.30 adds first-use explanation, gain feedback and restrained milestone feedback.
- Simulated rankings now cover Total Power, Corrupts defeated and Reliquia collection, always showing the player and immediate rival.
- Battle reports expose summary + full detail only.
- Persistent ES/EN selector, server UTC clock and first ambient/SFX audio layer are active in Settings.
- English-mode audit includes dynamic Arc I text (including World Boss labels) and is clean on the certified path.
- Bastion II/III background/offline completion is aligned with the normal flow and returns to Kingdom so newly unlocked structures remain visible; fresh-save regression covers this explicitly.
- Ceremonies are reserved for real milestones.
- Códice/Relicario v0.29 architecture is preserved. PvP remains locked/future. No new Breach mechanics, alliances, heroes or building proliferation were added.
- Focused regression: `qa/e2e-v030-depth.js`.
- Certification: final certified runtime candidate `dd2641a3d20751465c6f4e4c20fd350b6f33bbb5`, GitHub Actions run 1231. Full local-equivalent gate, uninterrupted fresh-save Arc I/Bastion I–X, mobile UX, frozen tester snapshot guard, GitHub Pages deployment, published Chromium verification and frozen tester URL verification all passed; the final fresh-save includes the Bastion II render stabilization.

## Latest v0.30.3 correction block
- Military UX responsibilities are now explicit and separated: Barracks → recruit/upgrade; Troops → owned army; Heroes → characters; March → expedition composition.
- Hero Hall is hero-only. March is a dedicated screen with Heroes / Troops / Composition / March Power / Confirm hierarchy.
- Contextual first-use explanations cover Barracks, Troops, Hero Hall and March.
- Mission drawer discoverability and mobile touch targets were improved without increasing HUD footprint.
- Speedups now teach themselves on first acquisition and expose an explicit ACCELERATE picker for compatible timed tasks.
- Building labels are normalized to name + Level only.
- Manuscript acquisition now triggers a short narrative ceremony and consequence; important rewards use proportional ceremony.
- Contrast/legibility and English coverage were audited across the modified surfaces.
- Permanent QA policy now requires **Discoverability → Comprehension → Interaction → Feedback → Next step** in addition to technical correctness.
- Focused regression: `qa/e2e-v0301-ux-clarity.js`.
- Final certified v0.30.2 release candidate: `24855ee2a862676f36f9185be5ede8fc42dfa860`, GitHub Actions run 1275. Integral fresh-save Bastion I–X, novice-player/mobile UX, frozen tester guard, Pages deployment, published Chromium verification and frozen tester URL verification all green.

## Next task
Continue normal v0.30 owner testing/development using the smallest QA tier that gives meaningful evidence; preserve full fresh-save certification for important candidates.
