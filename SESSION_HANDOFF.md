# Eldoria — SESSION HANDOFF

Updated: 2026-09-23

Permanent rules: `AGENTS.md`. Functional state: `PROJECT_STATE.md`. Detailed QA: `QA_AND_DEPLOY.md`. History: `CHANGELOG.md` only.

## Current working state
- Branch: `main`, only active development line.
- Active development version: **v0.27**.
- Verify the real live HEAD at session start; never infer it from chat/history.
- Canonical editable source: `v0220/index.html` + `v0220/js/`; `v0220` is a compatibility directory name.
- Generated development output: `playtest/`; never edit it directly.
- Normal development URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/
- QA Launcher URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/?qa=1&launcher=1

## Current v0.27 product state
- Chapter mission system and universal accelerators are active.
- Arcón includes utility tabs, including Aceleradores.
- Chapter II Engendro is reachable/visible in its intended sequence.
- Layered PvE remains active; only Arqueros are player-facing/recruitable; PvP remains unimplemented.
- Hero/troop/march base domain is now active for Aldric + Lyra: roles, affinities, inherited base stats, skill-slot/rank schema, exclusive-talent seam, troop roster and explicit march composition. Paladines are reserved structurally but still hidden/unbalanced; Maelis remains for a later integration pass.

## QA workflow now in force
Three separate levels exist:
1. **Focused** — exact system preset / dedicated test for small changes.
2. **Segment** — coherent progression block preset for medium changes.
3. **Integral** — `npm run validate:local` + uninterrupted fresh save for milestones, progression/economy/sequencing changes and release candidates.

Do not automatically replay all of Arc I for every small correction. Escalate QA according to risk, while keeping the integral gate mandatory before important stable releases.

The development-only QA Launcher is injected during `playtest/` generation and appears only with `?qa=1`. Its storage layer isolates the canonical save key to a QA-only save, so presets/fresh QA do not alter normal saves. Shared fixture definitions live in `v0220/js/qa-fixtures.js` and should be reused by Playwright where practical.

Current focal presets: Héroes + Tropas + Marcha, Engendro, Fisura/Lyra, Forja/Devorador, Códice/Reliquias/Duelo, Maelis, marcha/ataque, Heraldo semiautomático, capítulos v0.27, aceleradores and final Bastión X. Segment presets: VI→VIII and IX→X.

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
- Base Héroe + Rol + Afinidad + Estadísticas + Tropas + Marcha implemented for Aldric and Lyra without redesigning the final Hero Hall UI.
- Aldric: Tank, Paladines affinity, +3% Defense to Paladines in the march only.
- Lyra: DPS, Arqueros affinity, +3% Attack to Arqueros in the march only.
- Existing balanced values were reused; no new Paladin combat values were invented. Maelis is intentionally deferred.
- Focused preset: `hero-army-base`. Permanent regression: `qa/e2e-v027-hero-army.js`.

## Next task
Continue normal v0.27 owner testing/development using the smallest QA tier that gives meaningful evidence; preserve full fresh-save certification for important candidates.
