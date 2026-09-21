# Eldoria — SESSION HANDOFF

Updated: 2026-09-21

Permanent rules: `AGENTS.md`. Functional state: `PROJECT_STATE.md`.

## Current working state
- Branch: `main`.
- Active release candidate: **v0.26.2**.
- Previous certified public baseline: v0.25.1 at `343b7bde7dd0dd75e5c0bd47984938dcd7a6f6bd`.
- Canonical source: `v0220/index.html` + `v0220/js/`.
- Generated output: `playtest/`; never edit it directly.
- Public URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/playtest/
- Protected recovery baselines remain `baseline/v0.24-certified` and `stable/visual-good-f139968c`.

## v0.26.2 delivery block
Built on the certified v0.26.1 baseline. The Códice/Relic layer now recovers the useful v0.21 rules: clear use-vs-conserve trade-off, consumed cards become weak 1/1/1/1 Echoes for Duelo, Indestructible remains a quality and survives use by entering cooldown, and conserving is not irreversible. Orus provides guided orientation training. At 5 discovered relics, the formal Duelo opens as a playable 3×3 board with N/E/S/O edge comparisons, adjacent captures, a +1 central resonance cell and consumed-card Echoes 1/1/1/1.

Arcón and Códice are now dedicated full-screen game views, like Mundo/Valoria, rather than modal overlays on top of the map. Arcón remains strictly objects/materials/equipment; Códice remains strictly cards/relics.

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


v0.26.2 acceptance: Arcón and Códice are full-screen views; first relic clearly explains USAR vs CONSERVAR; normal-card consumption/Echo rule is visible; Indestructible survives use and enters cooldown; conservation permits later use; Orus training teaches N/E/S/O orientation; formal Duelo milestone counts discovered relics including consumed Echoes; the playable 3×3 board must support real card selection, placement, capture and CPU response; uninterrupted Arc I and prior v0.26.1 regression suite remain green.