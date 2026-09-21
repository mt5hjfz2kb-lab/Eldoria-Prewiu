# Eldoria — SESSION HANDOFF

Updated: 2026-09-21

Permanent rules: `AGENTS.md`. Functional state: `PROJECT_STATE.md`.

## Current working state
- Branch: `main`.
- Active release candidate: **v0.26.0**.
- Previous certified public baseline: v0.25.1 at `343b7bde7dd0dd75e5c0bd47984938dcd7a6f6bd`.
- Canonical source: `v0220/index.html` + `v0220/js/`.
- Generated output: `playtest/`; never edit it directly.
- Public URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/
- Protected recovery baselines remain `baseline/v0.24-certified` and `stable/visual-good-f139968c`.

## v0.26 delivery block
Implemented as an evolution of v0.25.1: hunting PvE vs threat PvE combat language; reserved PvP architecture; readable hero attack/defense/support stats; clearer expedition composition; Power reframed as a summary; Arcón moved into bottom navigation with object cards; strict Arcón/Códice separation; world-earned building-upgrade materials; Bastion VII first relic; Orus introduction; guided Códice/Duelo de Reliquias; Indestructible as a quality; mobile world-pan rendering performance pass; unified v0.26 visual hierarchy.

Macro Breach-server, deep talents, resonances, oracle and complete PvP remain future scope.

## Verification
Final v0.26 certification is the Pages workflow attached to this documentation push. Do not call the build certified until the workflow's full gate and published Chromium verification are green.

## Next task
If certification is green, owner human playtest of v0.26. Any feedback becomes one coherent owner-feedback block.

## Minimal commands
```bash
npm install
npm run qa:setup
npm run validate:local
```
