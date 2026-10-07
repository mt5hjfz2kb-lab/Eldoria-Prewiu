# Eldoria — Valoria Bastion I → II — First Complete City Progression Loop v1

Status: **CLOSED — ALL REQUIRED GATES PASS / PUBLISHED WEBGL PASS**
Workstream: `valoria-bastion-i-ii-progression-v1`

## Scope
This block turns the certified real-state substrate into the first complete playable city progression segment. It does not reopen visual R&D, create a parallel game architecture, or authorize Bastion III.

Target:
`NEW GAME → BASTION I → economy/world decisions → Aserradero → resources → route threat → Bastion upgrade → visible Bastion II → Cuartel → recruitment → March preparation → Engendro → save/reload`.

World Region 1 remains a separate 4X screen and is consumed through its certified `LocalGateway` contract.

## Vertical-slice / historical audit

| Surface | Decision | Reason |
| --- | --- | --- |
| `PlayerState`, `LocalGateway`, persistence | REUSE | Certified authoritative state, timestamps, save/reload and idempotency. |
| Parcel progression + SHARP variants | REUSE | Certified real construction presentation. |
| World Region 1 forest/quarry/scout/Engendro | REUSE | Closed separate 4X screen and macroloop contract. |
| Historical web I–II economy | ADAPT | Preserved as `OWNER_I_II`; gameplay evidence, not visual authority. |
| Web mission/March/Expedition Power teaching | ADAPT | Consumed by current state-aware Unity objective flow. |
| Old HTML/CSS/prototype visuals | SUPERSEDED | Current certified Valoria/World own player-facing art. |
| Bastion III+, Granero, Forge, Hospital, Relicario/Códice depth | NOT YET | Outside this closure. |
| Wider heroes, cards, PvP/alliance breadth | NOT YET | Not needed for the I–II proof. |

## Bastion I contract
Owner-facing WebGL already selects `OWNER_I_II` at boot.

Fresh state:
- Bastion I;
- Aserradero unbuilt; its parcel is the first reconstruction target;
- Cuartel unbuilt and locked until Bastion II;
- 230 wood / 150 stone / 36 Archer T1;
- Aldric is the current expedition hero.

Chapter I requires:
- Aserradero built;
- 600 gathered wood;
- 500 gathered stone;
- corrupt route cleared;
- 450 wood / 300 stone available for Bastion II.

The player cannot build everything immediately. Costs are paid at authoritative task start; resource recovery is through the World screen; mission counters track gathered resources rather than wallet balance.

## Buildings in I → II

### Bastion
Function: city progression authority and unlock gate.

I → II:
- previous level 1;
- requires Chapter-I journey complete;
- uses active-profile wood/stone cost;
- consumes cost once through idempotent command execution;
- increases canonical Kingdom/Total Power;
- unlocks Cuartel, Chapter II, March preparation and Engendro progression;
- persists in `PlayerState.BastionLevel`.

`BastionProgressionCatalog` owns requirements/costs/unlocks so later levels extend progression content rather than adding UI-only rules.

### Aserradero
- starts unbuilt;
- 80 wood;
- persistent shared construction queue;
- completed SHARP state appears only after authoritative level 1;
- contributes canonical Power.
No passive-income system is invented here; wood recovery remains the real World-node loop.

### Cuartel
- locked during Bastion I;
- becomes available at Bastion II;
- owner profile: 180 wood / 120 stone;
- persistent construction;
- unlocks Archer recruitment;
- owner profile requires 20 trained this chapter;
- bounded 20-Archer batch costs 400 wood / 240 stone;
- recruitment completion is persistent/idempotent.

No future building is exposed.

## Economy / progression

Chapter I:
1. rebuild Aserradero;
2. recover wood;
3. recover stone;
4. clear corrupt route;
5. return to Valoria;
6. satisfy Bastion-II wallet cost;
7. ascend.

Chapter II:
1. build Cuartel;
2. return to World if recruitment resources are missing;
3. train Archers;
4. explicitly prepare/confirm Aldric's March;
5. meet Expedition Power;
6. defeat Engendro;
7. return with one-time reward.

Objective selection is state-derived. Already-completed conditions are recognized automatically.

## Bastion II visual evolution
The certified SHARP Valoria remains authority. Bastion II is the same Bastion grown.

`ValoriaParcelPresentation` now binds `BastionLevelTwoVisuals` directly to persisted `PlayerState.BastionLevel`.

The final bounded II layer adds:
- two restrained heraldic pennants on the upper side towers;
- a small crenellation/authority lift on the upper keep.

This is deliberately an incremental level change, not a replacement castle.

They are OFF at I, ON at II, non-interactive, additive, tiny in payload, and use the existing Unity composition route. The production capture includes `runtime-bastion-ii` and asserts visual state follows authoritative level.

## Reward feedback
Successful ascent produces:
`BASTIÓN II · MI REINO HA CRECIDO · +X PODER · Cuartel y Capítulo II desbloqueados`.

`+X PODER` is calculated from canonical power before/after the real transition. No parallel metric exists.

## Tutorial / mobile UX
The existing 390×844 UI grammar remains:
- touch-first, no hover dependency;
- resource HUD;
- one current-objective card;
- one contextual primary CTA;
- separate CIUDAD/MUNDO navigation;
- requirement-aware building/world panels;
- future systems disabled.

The Bastion panel now tells the player what is required, what is missing, and what II unlocks.

## Persistence / idempotency
No new save architecture:
- command IDs prevent duplicate spending;
- task IDs prevent duplicate construction/recruit completion;
- World return IDs prevent duplicate rewards;
- timestamps survive reload/offline;
- chapter counters, prepared March, report, Bastion/building levels persist;
- presentation is reapplied from persisted state.

## World contract / first macroloop
Already-real contract:
`VALORIA → MUNDO → resource/PvE → March → reward → REINO/Valoria → use reward → progress`.

Bastion I consumes forest/quarry/scout. Bastion II consumes the existing Engendro route and prepared March. No second World implementation is created.

## Data-driven seams
- `SliceContentProfiles`: atomic content/balance profile;
- `BastionProgressionCatalog`: Bastion requirements/costs/unlocks;
- `SliceRules.CurrentObjectiveKey`: state-aware mission progression;
- `ChapterProgressState`: persistent mission counters/flags;
- parcel manifest/bindings: state-driven construction;
- Region 1 data/runtime: separate World screen;
- `ValoriaParcelPresentation`: state-driven visual variants.

Bastion III is deliberately not authored.

## Performance / payload
No duplicate SHARP scene per Bastion level. II uses tiny additive runtime cues plus existing parcel variants. No new paid model route and no paid credits.

## Mandatory gates
Closure requires:
1. SOURCE / ARCHITECTURE
2. TECH
3. INITIAL GAME STATE
4. CONSTRUCTION
5. ECONOMY
6. PROGRESSION
7. BASTION VISUAL EVOLUTION
8. INTERACTION
9. SAVE / RELOAD
10. IDEMPOTENCY
11. VISUAL REGRESSION
12. MOBILE READABILITY
13. PERFORMANCE / PAYLOAD SANITY
14. CITY ↔ WORLD CONTRACT
15. FIRST COMPLETE MACROLOOP

Automated gates may accelerate elapsed time, but the accepted progression starts from fresh authoritative state and uses normal game commands—no injected Bastion/building completion or wallet cheats.

Paid credits: **0**.


## Final certification — CLOSED

Closure commit chain:
- final production implementation: `725035ded5d003b20582f8e3ca5d43e5ee967a15`
- final publication request: `b5046f98c0fef689c109190611271ecce6d001ea`
- workstream closure: `3de462440278d2cf16bfd34e95e0cc0850c3d245`

Final evidence:
- production run `37656607306`: SUCCESS
- Unity source/editor gate `37656607356`: SUCCESS
- visual-authority routing `37656607230`: SUCCESS
- Pages/WebGL publish `37657298925`: SUCCESS
- published mobile playable probe `37660181336`: SUCCESS
- published probe artifact `11499884413`
- published URL: `https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/unity-owner/`

Published mobile probe verified HOME, real horizontal and vertical touch pan, MUNDO, Region 1 gather/reward, REINO/Valoria return, browser reload persistence, landscape and portrait presentation. The probe reported `playablePass=true` and restored revision 4 with wood 590 and gatheredWood 360 after reload.

The full Bastion I→II progression contract, construction, economy, Bastion II state binding, interaction, idempotency and save/reload are covered by the successful production/EditMode/PlayMode gates. The published probe is a transport/mobile regression gate and does not replace those deeper progression tests.

Final verdict: **SOURCE PASS / TECH PASS / INITIAL GAME STATE PASS / CONSTRUCTION PASS / ECONOMY PASS / PROGRESSION PASS / BASTION VISUAL EVOLUTION PASS / INTERACTION PASS / SAVE-RELOAD PASS / IDEMPOTENCY PASS / VISUAL REGRESSION PASS / MOBILE READABILITY PASS / PERFORMANCE/PAYLOAD SANITY PASS / CITY↔WORLD PASS / FIRST COMPLETE MACROLOOP PASS.**

Paid credits: **0**.

Bastion III is **not opened** by this closure.
