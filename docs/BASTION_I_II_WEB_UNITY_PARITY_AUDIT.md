# Eldoria — Bastion I–II Web ↔ Unity Parity Audit

Status: ACTIVE MIGRATION CONTRACT  
Updated: 2026-09-29

## Purpose

Separate three concepts that were previously easy to confuse:

1. **canonical product contract** proven in the web vertical slice;
2. **current Unity QA slice behavior** built to prove technical feasibility quickly;
3. **owner-playtest profile** still to be finalized before human I–II evaluation.

The web remains the canonical design/reference until Unity replacement is explicitly approved. Unity may simplify implementation while proving infrastructure, but those simplifications must not silently redefine product intent.

---

## Bastion I — web product contract

Canonical chapter: **LAS CENIZAS DE VALORIA**.

Validated web missions:
- rebuild Aserradero;
- recover **600 wood**;
- recover **500 stone**;
- clear the Corrupt route;
- raise Bastion to level 2.

Relevant current web facts:
- fresh state: **230 wood / 150 stone / 36 Archer T1**;
- Aserradero reconstruction: **80 wood**, 6 s current web task duration;
- Bastion II cost: **450 wood / 300 stone**;
- forest and quarry are both part of the early-world economy;
- first corruption mark / route threat are part of the discovery arc;
- chapter/mission UI owns progress and next-action guidance.

### Current Unity I behavior

Current Unity starts:
- **30 wood / 150 stone / 36 Archer T1**;
- Aserradero cost **80 wood**, 6 s;
- one forest node supplies the primary gather loop;
- CorruptionDiscovered is set by starting the forest gather;
- JourneyComplete becomes true once:
  - Aserradero is built;
  - corruption was discovered;
  - the March has returned idle.

Important gap:
- current Unity can complete the Bastion-I journey **without requiring ScoutDefeated**;
- no canonical 600-wood / 500-stone mission counters exist;
- no equivalent early quarry requirement exists;
- Bastion II resource cost is not currently enforced on AdvanceBastion.

Verdict: **TECHNICAL SLICE PASS / PRODUCT PARITY NOT YET PASS**.

---

## Bastion II — web product contract

Canonical chapter: **ALGO QUE DEFENDER**.

Validated web missions:
- build Cuartel;
- train **20 Archers**;
- reach **2,250 Expedition Power**;
- defeat one Engendro de la Fisura;
- raise Bastion to level 3.

Relevant web facts:
- Cuartel construction: **180 wood / 120 stone**, 8 s current web task duration;
- Archer recruitment supports 5 / 10 / 20 quantities;
- recruitment cost is currently **20 wood + 12 stone per Archer**;
- recruitment time is derived from quantity, minimum 7 s in the current web implementation;
- March is an explicit first-class surface once Cuartel exists;
- player sees hero + troop composition + March Power before relevant combat;
- battle reporting shows participants, stats, damage/outcome and reasons.

### Current Unity II behavior

Current Unity:
- Cuartel cost **140 wood / 90 stone**;
- recruit action adds **12 Archers** for **50 wood**, no stone;
- starts with 36 Archers, so the gate is **48 total**;
- explicit mission target is therefore functionally “recruit 12” rather than web “train 20”;
- Engendro requires Bastion II + Cuartel + 48 available Archers;
- Aldric + **all available troops are auto-selected** when departing;
- no explicit March configuration confirmation exists;
- Expedition Power is displayed but is not a Bastion-II mission gate;
- battle reason exists as a string, but there is no product-grade two-level report surface.

Verdict: **LOGIC / PERSISTENCE PASS / TEACHING + PARITY NOT YET PASS**.

---

## What is truly canonical vs what needs judgment

### Canonical product intent — preserve
- Bastion I teaches kingdom → world → gather/fight → return → visible growth.
- Bastion I includes both economy recovery and a real route/corruption threat.
- Bastion II introduces Cuartel, recruitment, March understanding and Engendro combat.
- March must become an explicit player concept in Bastion II.
- Expedition Power is distinct from Total Power.
- building/recruitment tasks are persistent timestamp tasks.
- costs are paid when tasks start.
- one construction/building-upgrade queue remains the current rule.
- player guidance follows Discoverability → Comprehension → Interaction → Feedback → Next step.

### Values that should be treated as current web implementation until owner pacing confirms them
- exact chapter resource targets (600 wood / 500 stone);
- exact Bastion II/III resource costs;
- exact troop recruitment cost per unit;
- current short task seconds.

These values have stronger authority than Unity QA constants, but the owner playtest should validate pacing before they are frozen as final launch balance.

### Unity QA_FAST values — do not promote silently
- initial wood 30;
- Cuartel 140/90;
- recruit +12 for 50 wood;
- 48-Archer Engendro gate as a shortcut to prove recruitment/combat;
- 2 s travel / 5 s gather;
- JourneyComplete without explicit route-clear requirement.

---

## Migration decision

Do **not** immediately copy every web number into Unity and call parity complete.

Instead:

1. preserve current Unity QA behavior under an explicit `QA_FAST` profile;
2. implement the missing **structure** first:
   - mission counters/objectives;
   - explicit March preparation;
   - route-clear requirement;
   - Expedition Power milestone;
   - battle report;
3. add an `OWNER_I_II` content profile based on the web contract;
4. validate I–II pacing in a human owner playtest;
5. only after that promote the chosen owner values to production/default.

This protects development speed without letting test shortcuts redefine the game.

---

## Immediate P0 parity backlog

1. **Bastion-I mission state**
   - gather wood counter;
   - gather stone counter / quarry path;
   - route-clear flag required;
   - Bastion-II ascent cost and mission gate.

2. **Bastion-II recruitment parity**
   - quantity-select or a bounded I–II equivalent that can satisfy 20 trained;
   - resource cost model that can later match web;
   - trained counter separate from total owned troops.

3. **Explicit March preparation**
   - Aldric;
   - Archer quantity;
   - Expedition Power / key stats;
   - confirm/send.

4. **Engendro report**
   - participants;
   - key stats;
   - result/reason;
   - reward/consequence.

5. **OWNER_I_II profile**
   - values proposed from web;
   - clearly separate from QA_FAST;
   - human pacing acceptance before production freeze.
