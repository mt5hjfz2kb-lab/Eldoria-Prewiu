# Eldoria — Bastion I–II Owner Playtest Checklist

Status: ACTIVE ACCEPTANCE CHECKLIST  
Updated: 2026-09-29

## Purpose

Define the first meaningful human acceptance test for Unity Eldoria.

This checklist is deliberately stricter than CI. A run can be technically green and still fail owner playtest if the player does not understand what to do, a visible area reads as placeholder, or progression only works through developer knowledge.

The test always starts from a **fresh local save**.

---

## Build profile

- Runtime balance profile: QA_FAST until OWNER_I_II pacing is explicitly promoted.
- Product meaning: must match the canonical web Bastion I–II contract even when QA thresholds are compressed.
- Visual direction: frozen Valoria Visual Formula v1.
- Target device: desktop/Windows build first for iteration; representative physical mobile required before declaring mobile-ready.

---

## Fresh-save starting expectations

Player sees:
- damaged/recovering Valoria;
- Bastion I;
- Aldric as current hero;
- initial resource HUD;
- current objective that explains the first need;
- no Bastion III+ systems exposed.

Failure if:
- developer/QA wording dominates the frame;
- objective is ambiguous;
- player can access systems that should not exist yet;
- dominant visual element reads as primitive/placeholder.

---

## Bastion I — Las Cenizas de Valoria

### 1. Understand the need
Expected:
- player understands Valoria needs rebuilding;
- Aserradero is identified as the first functional restoration target.

Failure if the player must guess why the Aserradero matters.

### 2. Enter the Frontier
Expected:
- navigation Valoria → Frontier is obvious;
- world corridor preserves visual quality sufficiently to feel like the same game.

Failure if Frontier looks like a technical test scene compared with Valoria.

### 3. Recover wood
QA_FAST target:
- gather enough wood to satisfy the explicit Bastion-I wood objective.

Expected:
- departure / gather / return is readable;
- mission counter increments on return;
- resource is not double-granted after reload.

### 4. Recover stone
QA_FAST target:
- gather enough stone from the early quarry to satisfy the Bastion-I stone objective.

Expected:
- forest and quarry have distinct identities;
- mission counter tracks gathered stone, not wallet balance.

### 5. Rebuild Aserradero
Expected:
- building cost is visible;
- cost is paid once at task start;
- task survives app/reload;
- completion visibly changes Valoria.

### 6. Clear the corrupt route
Expected:
- player understands this is a combat/threat objective, not merely another resource node;
- Explorer battle resolves;
- route-clear mission flag only completes after victory;
- battle summary explains participants, result, rounds/reason and reward.

Failure if merely discovering corruption completes the chapter.

### 7. Complete Bastion I
Expected:
- chapter cannot complete without:
  - Aserradero;
  - wood objective;
  - stone objective;
  - route cleared.
- player receives clear completion / next-step feedback.

### 8. Raise Bastion to II
Expected:
- Bastion ascent is an intentional moment;
- city progression is visually perceptible;
- no Bastion III content is introduced.

---

## Bastion II — Algo que Defender

### 9. Build Cuartel
Expected:
- player understands Cuartel = troop recruitment / military infrastructure;
- construction queue rules remain clear.

### 10. Train Archers
QA_FAST target:
- train the current compressed QA quantity.

Expected:
- objective tracks **trained this chapter**, not total roster;
- recruitment task survives reload;
- repeated/idempotent resolution cannot duplicate troops.

### 11. Prepare March
Expected:
- explicit PREPARAR MARCHA action;
- player sees:
  - Sir Aldric;
  - available/prepared Archer count;
  - Attack;
  - Defense;
  - Health;
  - Break;
  - Expedition Power;
- player explicitly confirms March.

Failure if Engendro can be attacked without March confirmation.

### 12. Understand readiness
Expected:
- objective distinguishes Total Power from Expedition Power;
- confirmed Expedition Power is stored in authoritative mission progress.

Failure if the player has no way to understand why a force is or is not ready.

### 13. Fight Engendro
Expected:
- Engendro cannot launch if:
  - Cuartel missing;
  - required troops missing;
  - March not confirmed;
  - prepared troops are no longer available.
- combat uses prepared March, not silently all troops.

### 14. Understand battle result
Expected summary:
- target;
- victory/defeat;
- hero;
- troop count;
- player Expedition Power;
- rounds;
- remaining health;
- reward;
- human-readable reason.

Do not invent enemy power until combat exposes it authoritatively.

### 15. Complete Bastion II
Expected:
- Engendro mission flag persists;
- player returns to a visibly stronger Valoria;
- clear "chapter complete / new ambition" beat;
- Bastion III content remains outside the current playable scope.

---

## Persistence / reload interruptions

Repeat the test with reload/exit during:
- gathering outbound/gathering/returning;
- Aserradero construction;
- Cuartel construction;
- Archer recruitment;
- prepared March before attack;
- combat return/reward.

Acceptance:
- no duplicate costs;
- no duplicate rewards;
- no lost mission counters;
- no lost March confirmation;
- no lost battle report;
- state resumes from authoritative timestamps.

---

## UX acceptance

During the complete I→II run, the owner should never need:
- repository knowledge;
- a QA menu;
- a console;
- a developer explanation;
- knowledge of hidden thresholds.

If the player asks “what do I do now?”, record a UX failure at that state.

---

## Visual acceptance

Every dominant visible element on the owner path must avoid reading primarily as:
- primitive;
- white/flat placeholder;
- floating architecture;
- empty generic terrain;
- unrelated asset-store prefab;
- debug surface.

Specific I–II visual gates:
- Bastion silhouette / roof Hero Pass;
- Aserradero / Cuartel production PBR;
- inhabited/reconstruction cues in Valoria;
- quality Frontier corridor;
- forest identity;
- quarry identity;
- corrupt route identity;
- Engendro presentation;
- visible distant Breach/corruption cue where composition allows.

The reference benchmark remains the destination; I–II is accepted when this bounded path is representative enough to judge the final game direction honestly.

---

## Performance capture

Before calling I–II mobile-ready, record on a representative physical phone:
- resolution;
- quality profile;
- average FPS / frame time;
- peak memory;
- Valoria overview;
- dense district pan;
- Frontier travel/combat;
- sustained/thermal notes.

Do not pre-emptively reduce art quality without measured evidence.

---

## Exit criteria

Bastion I–II is owner-playtest GREEN only when:

1. fresh save reaches end of II without developer intervention;
2. all domain/CI regression tests pass;
3. objectives and March are explicit;
4. battle feedback is understandable;
5. persistence interruptions are safe;
6. bounded visual path is representative of Eldoria;
7. no critical UI/safe-area blocker exists.

After GREEN:
- collect owner feedback;
- fix the I–II pattern;
- decide whether Bastion needs full rebuild;
- begin Granero / Bastion III production in canonical chronology;
- schedule controlled architecture decomposition before breadth expands.
