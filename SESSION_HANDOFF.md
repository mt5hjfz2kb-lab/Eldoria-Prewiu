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

## QA workflow now in force
Three separate levels exist:
1. **Focused** — exact system preset / dedicated test for small changes.
2. **Segment** — coherent progression block preset for medium changes.
3. **Integral** — `npm run validate:local` + uninterrupted fresh save for milestones, progression/economy/sequencing changes and release candidates.

Do not automatically replay all of Arc I for every small correction. Escalate QA according to risk, while keeping the integral gate mandatory before important stable releases.

The development-only QA Launcher is injected during `playtest/` generation and appears only with `?qa=1`. Its storage layer isolates the canonical save key to a QA-only save, so presets/fresh QA do not alter normal saves. Shared fixture definitions live in `v0220/js/qa-fixtures.js` and should be reused by Playwright where practical.

Current focal presets: Engendro, Fisura/Lyra, Forja/Devorador, Códice/Reliquias/Duelo, Maelis, marcha/ataque, Heraldo semiautomático, capítulos v0.27, aceleradores and final Bastión X. Segment presets: VI→VIII and IX→X.

## Frozen tester isolation
- **Eldoria Closed Playtest T1 / 0.26.5-test.2**.
- Frozen integration commit: `e3b47bf05ad9b68703bc47e78c3eb1c1ca542535`.
- URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/tester-v0265/
- Frozen research output only; never use as source and never inject the QA Launcher into it.

## Next task
Continue normal v0.27 owner testing/development using the smallest QA tier that gives meaningful evidence; preserve full fresh-save certification for important candidates.
