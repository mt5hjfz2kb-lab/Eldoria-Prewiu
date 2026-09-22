# Eldoria — SESSION HANDOFF

Updated: 2026-09-22

Permanent rules: `AGENTS.md`. Functional state: `PROJECT_STATE.md`.

## Current working state
- Branch: `main`.
- Active normal-development milestone: **v0.26.5**.
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


## v0.26.4 resource HUD readability correction — 2026-09-22
- Restored the previously clear resource pictograms: 🌲 wood, 🪨 stone, 🍖 food and ⚔️ Power.
- Mobile top HUD is reduced to 50px and uses compact icon/value chips; the realm label is hidden on mobile to preserve playfield width.
- Resource and Power groups flex within the available width instead of pushing/covering HUD controls. Ranking remains a compact top control; duplicate Arcón HUD access stays hidden because Arcón already exists in primary navigation.
- Permanent mobile QA now checks the pictograms, horizontal HUD overflow and overlap between the profile/resources/tool groups.
- No economy, gameplay, progression or resource values changed.


## v0.26.4 Bastion contextual-anchor correction — 2026-09-22
- Bastion now follows the exact same object-local interaction rule as the other city buildings on mobile.
- Its interactive visual box was tightened to the visible Bastion footprint, so MEJORAR/CONSTRUIR appears immediately below the Bastion instead of being displaced by the old oversized invisible button area.
- Construction timers remain directly above the selected building.
- Permanent QA now asserts both Aserradero and Bastion contextual actions are attached within 10px beneath their actual building hitbox.
- No gameplay, cost, timer or progression logic changed.


## v0.26.5 unified owner-improvement block — 2026-09-22
- Initial guidance now explains the next action and adds contextual shortage guidance with exact missing resource quantities, source locations and a direct destination action.
- World navigation preserves the player's pan position across node selection, gathering and World ↔ Valoria transitions; mobile pan remains free during active tasks. The pannable frontier and node distribution were expanded to create more exploration horizon without introducing a fake final 4X map.
- Valoria building positions were rebalanced for mobile spacing while retaining object-local actions and timers.
- Barracks now separates building upgrades from troop recruitment. Recruitment has its own quantity/cost/time interface and timestamp task, including offline completion; Barracks level is a real infrastructure level.
- Lyra's recruitment explicitly unlocks and highlights Heroes, explaining hero stats, equipment, Power and expedition relevance.
- Bastion IV now requires Aserradero, Cuartel and Granero at level 2 in addition to resources and the Fissure gate; missing infrastructure guides the player to the correct building.
- Defeating the Devourer now triggers a canonical narrative consequence before the Aether Ember reward is resolved.
- Arcón items use distinct pictograms and stronger type/use/origin presentation; basic resources remain outside inventory and Reliquias remain in Códice.
- Forge, Códice, Arcón, Heroes, Power and related dialogs now share a mobile vertical-scroll contract for 390×844.
- Códice received a purpose/collection/decision UX pass with clearer rarity, N/E/S/O, effects, status and Echo presentation.
- First Duelo access now launches a real guided 3×3 tutorial using five temporary loan cards. The tutorial teaches placement, directional comparisons, captures and control progressively; loan cards never enter the permanent collection and are removed on completion.
- Permanent QA: `qa/e2e-v0265-owner-block.js`, included in targeted and regression suites. Fresh-save Arc I now includes timed recruitment and Barracks infrastructure before Bastion IV.


## Frozen external tester build — 2026-09-22
- Name: **Eldoria Closed Playtest T1**.
- Tester version: **0.26.5-test.1**.
- Frozen game source: `67f6ff8fd10a01a428d29b22c678f51e5a7225f1`.
- Certified integration commit: `84c0a4071f3be639154f021689109719e9ba6cf7`.
- Frozen reference branch: `frozen/testers-v0265-t1`.
- Public tester URL: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/tester-v0265/
- **Do not edit `tester-v0265/` during T1 except for a critical test-invalidating defect.**
- `main` is the only active development line. Normal product work continues from `v0220/`; no visible tester intro/reporting/survey layer belongs in the active runtime.
- Pages now enforces a drift guard against certified commit `84c0a4071f3be639154f021689109719e9ba6cf7`; a main push cannot silently change tester bytes.
- Main publication only checks that the frozen tester URL remains available; it no longer reruns tester research instrumentation or inserts QA feedback on every normal deployment.
- Feedback persistence: Supabase project **Eldoria Playtest Feedback**, raw evidence in `public.eldoria_feedback`.
- Grouped evidence/tracing: `public.eldoria_feedback_findings` + `public.eldoria_feedback_finding_links`.
- Aggregated binary metrics: admin-only view `public.eldoria_feedback_quick_stats`.
- Analysis/prioritization rules: `TESTER_FEEDBACK_PROTOCOL.md`.

## Current operating rule after T1 freeze
- Continue design, systems, bugfixes, UX, content and vertical-slice work normally on `main`.
- Feedback never auto-implements. First group equivalent evidence, classify, prioritize and record the decision/result with traceability.
- At the end of T1, compare consolidated evidence with the vertical-slice questions before deciding on major redesign or Unity migration.
