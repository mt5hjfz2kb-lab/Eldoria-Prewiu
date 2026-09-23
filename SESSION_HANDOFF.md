# Eldoria — SESSION HANDOFF

Updated: 2026-09-23

Permanent rules: `AGENTS.md`. Functional state: `PROJECT_STATE.md`. Detailed QA: `QA_AND_DEPLOY.md`. History: `CHANGELOG.md` only.

## Current working state
- Branch: `main`, only active development line.
- Active development version: **v0.30**.
- Verify the real live HEAD at session start; never infer it from chat/history.
- Canonical editable source: `v0220/index.html` + `v0220/js/`; `v0220` is a compatibility directory name.
- Generated development output: `playtest/`; never edit it directly.
- Normal development URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/
- QA Launcher URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/?qa=1&launcher=1

## Current v0.30 product state
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
- Ceremonies are reserved for real milestones.
- Códice/Relicario v0.29 architecture is preserved. PvP remains locked/future. No new Breach mechanics, alliances, heroes or building proliferation were added.
- Focused regression: `qa/e2e-v030-depth.js`.
- Certification: full local-equivalent gate, uninterrupted Arc I, mobile UX, frozen tester snapshot guard, GitHub Pages deployment, published Chromium verification and frozen tester URL verification all passed on the v0.30 candidate.

## Next task
Continue normal v0.30 owner testing/development using the smallest QA tier that gives meaningful evidence; preserve full fresh-save certification for important candidates.
