# Eldoria — SESSION HANDOFF

Updated: 2026-09-22

Permanent rules: `AGENTS.md`. Functional state: `PROJECT_STATE.md`. History: `CHANGELOG.md` only.

## Current working state
- Branch: `main` — the only active development line.
- Active development version: **v0.27**.
- Live HEAD: verify the real `main` HEAD at session start; do not infer it from historical docs or chat.
- Canonical editable source: `v0220/index.html` + `v0220/js/`.
- `v0220` is a compatibility directory name, not the product version.
- Generated public output: `playtest/`; never edit it directly.
- Normal development URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/
- Protected recovery baselines: `baseline/v0.24-certified` and `stable/visual-good-f139968c`.

## Frozen tester isolation
- Build: **Eldoria Closed Playtest T1 / 0.26.5-test.2**.
- Frozen integration commit: `e3b47bf05ad9b68703bc47e78c3eb1c1ca542535`.
- URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/tester-v0265/
- `tester-v0265/` is frozen research output, not a development baseline.
- Normal work on `main` must not edit or inherit the tester layer.
- Pages drift guard must keep the tester bytes identical to the frozen integration snapshot.

## Current v0.27 state
- Chapter mission system replaces the old persistent guided-step block.
- Mission/chapter rewards and universal accelerators are active.
- Arcón organizes stored content by utility, including an Aceleradores tab.
- Chapter II's Engendro de la Fisura is reachable in the intended mission sequence.
- Layered PvE from v0.26.6 remains part of the active v0.27 runtime.
- Current player-facing recruitable troop family remains Arqueros only.
- PvP remains unimplemented.

## Verification / workflow
- Documentation-only changes: check hierarchy/version/source/tester isolation, commit and push; no gameplay publication is required.
- Gameplay/release work: `npm run validate:local` must pass before the coherent push to `main`.
- A build is only called published/verified after the corresponding Pages workflow and published Chromium verification pass.

## Next task
Continue owner testing and v0.27 development from the normal development URL. Do not base fixes on the frozen tester snapshot.

## Minimal commands
```bash
npm install
npm run qa:setup
npm run validate:local
```
