# Eldoria — SESSION HANDOFF

Updated: 2026-09-21

Permanent rules: `AGENTS.md`. Functional state: `PROJECT_STATE.md`.

## Current working state
- Branch: `main`.
- Active release candidate: **v0.26.4**.
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

## v0.26.3 owner-feedback block
- Bastion IX mobile reliability: configured marches expose a persistent ATACAR objective outside the pannable map, so the action cannot disappear with device-specific pan/layout state.
- Salón de Héroes is now a dedicated full-screen view with hero roles, ATQ/DEF/APO explanations, equipment and march formation in one place.
- March setup explicitly explains Aldric as leader, companion roles, troop contribution and the difference between Power and combat composition.
- Códice first-visit education now includes an Orus-guided tutorial for cards, N/E/S/O, USAR vs CONSERVAR, consumed Echo 1/1/1/1 and Indestructible cooldown semantics.
- Bastion VI/Forge and Bastion VIII/Nareth narrative ordering was corrected so materials and Maelis are not explained before the player discovers them; repeated post-discovery dialogue was reduced.
- Contextual tutorial guidance now continues through Forge, Devorador, Códice, Nareth and Bastion IX march/trial milestones.
- Construction and upgrade resources are committed when the timer begins; completion no longer charges twice. Old in-flight saves retain a compatibility fallback.
- Permanent QA covers 390x844 touch layout, full-screen Hero Hall, march persistence, Bastion IX CTA visibility and immediate resource spending.


## v0.26.4 visual / UX parity block
- No gameplay/economy/progression/narrative rule changes.
- Primary bottom navigation is Ciudad · Mundo · Héroes · Arcón · Códice, preserving progression gates.
- Ranking remains available from the HUD instead of occupying a sixth primary-nav slot.
- City/world quick actions use a common bottom contextual sheet while the scene remains visible.
- Hero Hall, Arcón, Códice and Forge use full-screen management surfaces.
- Arcón adds in-screen object inspection without changing inventory semantics.
- World nodes expose consistent visual categories for resources, fauna, threats, elite/location and Breach.
- Mobile world pan uses direct pointer tracking plus restrained inertia and dynamic bounds.
- A single v0.26.4 scale/token layer harmonizes typography, spacing, panels, buttons, safe areas and touch targets.
- Functional parity inventory: `UI_PARITY_0264.md`; permanent QA: `qa/e2e-v0264-ui-parity.js`.

## v0.26.4 acceptance
100% v0.26.3 functional parity, full fresh-save Bastion I→X, mobile 390×844 navigation/context/map checks, all legacy regression suites green, Pages deployment green and published Chromium verification green.

## v0.26.4 mobile structural layout correction — 2026-09-22
- Owner rejected the previous oversized mobile composition; the mobile layout now follows hard viewport budgets.
- HUD: 56px. Bottom navigation: 58px. Quest/tutorial: <=58px / <=8vh. Normal contextual sheet: <=19vh, with secondary information under INFO.
- World nodes recover relative visual presence while tutorial emphasis and Breach decoration are restrained.
- Permanent QA now enforces HUD <=12% vh, tutorial <=8% vh, navigation <=10% vh, context <=22% vh and >=65% visible world share at 390x844.
- Gameplay, logic, economy, narrative, progression and action semantics are unchanged.
