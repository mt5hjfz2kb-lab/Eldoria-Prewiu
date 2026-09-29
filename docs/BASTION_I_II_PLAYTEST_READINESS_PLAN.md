# Eldoria — Bastion I–II Playtest Readiness Plan

Status: ACTIVE EXECUTION PRIORITY  
Updated: 2026-09-29

## Current execution status — 2026-09-29

Implemented in Unity:
- authoritative Bastion I–II mission counters/flags and domain-owned objective stages;
- forest + quarry resource lessons and explicit route-clear requirement;
- explicit Bastion-II March confirmation with persisted composition and Expedition Power;
- structured persistent battle report;
- full QA fresh-save I→II domain regression with offline/reload coverage;
- QA fresh-save reset control;
- atomic profile facade: runtime consumes exactly one content profile;
- QA_FAST remains active; OWNER_I_II is staged from the web contract and inactive;
- profile-scoped saves prevent QA/OWNER contamination while preserving the existing QA save path;
- OWNER_I_II values are regression-locked against the web contract;
- owner-playtest acceptance checklist: `docs/BASTION_I_II_OWNER_PLAYTEST_CHECKLIST.md`.

Latest known integrated gameplay gate before profile plumbing: run **36551685275 SUCCESS** (EditMode + PlayMode + Windows build + benchmark capture).
The newer profile-isolation/Frontier/Ground-Kit integration is awaiting its own full superseding Unity gate; do not call that newer HEAD green until the run finishes.

Current owner-candidate certification:
- integrated Unity gate **36559605933 SUCCESS** on gameplay/profile SHA **447d6e934a6fd72760cf88c97f9c889ca749f693**;
- EditMode PASS;
- PlayMode PASS;
- Windows desktop build PASS;
- official Valoria/Frontier benchmark capture PASS;
- `QA_FAST` remains the default runtime;
- `OWNER_I_II` is now explicit opt-in only through `--eldoria-profile=OWNER_I_II`;
- Windows package includes `PLAY_OWNER_I_II_CANDIDATE.bat`;
- QA and OWNER saves remain isolated;
- OWNER values remain the staged web-contract candidate and are not production-frozen.

Still required before owner-playtest GREEN:
- perform the uninterrupted human fresh-save I→II run using `PLAY_OWNER_I_II_CANDIDATE.bat`;
- record pacing/clarity issues, especially the Bastion-II resource recovery beat before recruiting 20 Archers;
- accept or revise the current Frontier I–II visual wedge from the owner playthrough; the stronger 0a07dfad experiment was intentionally reverted after visual regression;
- only after that human run, decide whether OWNER_I_II values should be promoted/frozen;
- representative physical-mobile performance remains a later readiness gate, not a blocker for the first desktop owner pass.

## Objective

Produce the first owner-playable Unity build that proves the real Eldoria promise through **Bastion I and II**:

**Valoria → need → world → gather/fight → return → visible growth → stronger army → new threat → victory/new ambition.**

This is not a broad Arc-I port. The web v0.32.0 remains the canonical product/design reference; Unity must preserve its validated intent while adapting presentation and implementation.

The build must be useful for a human product/art judgment, not merely technically reachable by automated tests.

---

## Executive decision

Until Bastion I–II owner playtest is green:

- do **not** migrate Bastion III–X as active Unity scope;
- do **not** add Códice, Relicario, Granero, Cantera, Forja or Hospital to the playable Unity path;
- do **not** reopen the validated Valoria LookDev formula;
- do **not** fully rebuild Bastion unless the bounded Hero Pass proves the current structural mass cannot reach the visual benchmark;
- do **not** perform a broad architecture refactor merely because current slice files are monolithic.

Advance only work that either:
1. makes Bastion I–II truthful to the web design,
2. makes the first owner test visually representative,
3. removes a blocker to mobile/owner testing,
4. reduces a known rewrite risk in the immediate I–II path.

---

## Current Unity baseline

Already real / green:

- persistent save and offline task completion;
- Bastion I level state;
- Aserradero build task;
- Valoria → Frontier scene travel;
- forest gathering with outbound / gather / return phases;
- Corrupt scout combat;
- Power transition and resource return;
- Bastion I → II ascent;
- Cuartel construction;
- profile-owned Archer recruitment (QA_FAST currently +12; OWNER_I_II candidate +20);
- Engendro readiness gated by trained-this-chapter + confirmed Expedition Power + prepared March;
- Engendro combat/reward;
- idempotent commands/tasks;
- dedicated Aserradero and Cuartel production art;
- certified playable Valoria topology and building hotspots;
- bounded city pan / official zoom family 9 / 12 / 19;
- first production district under frozen Visual Formula v1;
- automated EditMode, PlayMode, Windows build and visual gates.

These facts mean the project is no longer blocked on core feasibility.

---

## Mission-state implementation contract

For the I–II owner build, Unity needs a small authoritative chapter state rather than more presenter-only conditionals.

### Bastion I counters / flags
- Aserradero rebuilt;
- wood gathered this chapter;
- stone gathered this chapter;
- corrupt route cleared;
- Bastion raised to II.

### Bastion II counters / flags
- Cuartel built;
- Archers trained this chapter;
- prepared March confirmed;
- current Expedition Power;
- Engendro defeated;
- Bastion raised to III (future boundary; do not implement Bastion III content yet).

### Rules
- counters are authoritative domain/save state, not inferred from current wallet/roster after the fact;
- rewards are granted once and idempotently;
- objective presentation consumes this state but does not own it;
- QA_FAST may use compressed thresholds, but field meaning must match the web contract;
- OWNER_I_II thresholds come from the web contract and require owner pacing validation before becoming production defaults;
- Bastion I route-clear must become explicit rather than JourneyComplete being satisfied by corruption discovery alone.


## Implementation status — 2026-09-29

### Implemented / structurally green pending latest CI consolidation
- explicit `QA_FAST` vs web-contract content profiles;
- persistent authoritative chapter progress for Bastion I–II;
- early wood and stone gathering counters;
- explicit corrupt-route-clear requirement before Bastion II;
- early quarry node in Frontier;
- persistent Cuartel / recruitment state;
- explicit Bastion-II March confirmation;
- persisted prepared hero/troop composition;
- persisted confirmed Expedition Power;
- Engendro blocked until March is prepared;
- structured persisted battle report;
- Frontier battle summary with participants / Power / rounds / remaining health / rewards / reason;
- domain-owned objective-stage keys consumed by the HUD;
- QA fresh-save reset control;
- full domain fresh-save progression now exercises wood → stone → route → Bastion II → Cuartel → recruitment → March → Engendro;
- `OWNER_I_II_CANDIDATE` is staged in `SliceContentProfiles` from the web contract, but remains deliberately inactive until human pacing validation.

### Still required before OWNER PLAYTEST candidate
- latest full Unity gate must be green after concurrent changes settle;
- inspect/accept current Bastion Hero Pass and Frontier benchmark visually at representative mobile framing;
- switch from staged `OWNER_I_II_CANDIDATE` to an owner-facing runtime only after the integrated CI/build is green; do not simply promote QA_FAST;
- run uninterrupted human fresh-save I→II without developer knowledge;
- perform at least one physical-device mobile profile once Android/target build prerequisites are available;
- fix only the issues discovered by that integrated owner run before expanding into Bastion III.

---

## Product gaps before a meaningful human I–II test

### P0 — Explicit March preparation

Current Unity gameplay can send Aldric + all available troops automatically. That proves combat logic but does **not** preserve the Bastion-II teaching contract from the web.

Bastion II must expose an explicit but compact preparation step before the Engendro:
- selected hero: Aldric;
- visible Arqueros count;
- clear March Power / combat-relevant summary;
- confirm/send action;
- ability to understand why 48 Archers matter.

Do not port the entire late-game military UI. Reuse the stable web military-domain direction so the model can later grow to multiple heroes/troop tiers, while I–II UI exposes only what is actually unlocked.

### P0 — Chapter / objective guidance

Replace prototype-style always-visible explanatory copy with the validated web principle:
- compact current objective;
- contextual destination/action;
- clear completion feedback;
- next step.

For I–II, the player must be able to complete a fresh save without developer knowledge.

### P0 — Truthful I–II economy profile

Current Unity constants are slice-validation values. Before experiential playtest:
- compare I–II costs/rewards/timers against canonical web values;
- move immediate I–II balance into an explicit content/config layer rather than letting temporary test constants silently become final design;
- if a fast QA profile is retained, label/separate it from the owner experience profile.

The owner playtest must not accidentally validate pacing that exists only because test timers are 6–8 seconds.

### P0 — Battle feedback

Bastion II needs player-readable combat feedback, not only an internal result string:
- who fought;
- key stats/composition;
- outcome;
- short reason;
- reward/consequence;
- clear return to Valoria / next action.

Use the web's two-level report principle if practical: summary first, detail optional.

---

## Visual quality wedge for the I–II test

The visual benchmark remains the target: a luminous, monumental, inhabited epic-fantasy Valoria with visible dark/corrupted threat, strong depth and premium 4X readability.

The I–II owner build does **not** require all future Valoria art. It does require every surface visible in the test path to be credible enough that the test is not dominated by placeholder perception.

### Valoria

Keep:
- frozen Visual Formula v1;
- production Aserradero;
- production Cuartel;
- ResidentialTerraceRock / seam integration;
- certified topology / official camera.

Finish before owner test:
- current bounded Bastion silhouette / roof Hero Pass;
- remove remaining obvious floating/procedural crown reads;
- strengthen early ruined/rebuilding state rather than making early Valoria look fully mature;
- add selective life/readability cues:
  - Aserradero smoke/work material;
  - Cuartel training/guard cues;
  - repair/scaffolding/stockpile cues;
  - restrained Valoria blue identity accents where appropriate;
- ensure Bastion stays the orientation/focal anchor.

Do not fully rebuild Bastion yet unless the Hero Pass still fails the official-camera benchmark after the current silhouette corrections.

### Frontier / I–II world corridor

This is a major visual risk. A beautiful Valoria followed by a prototype-looking Frontier will invalidate the visual impression of the first test.

Create one bounded high-quality world corridor sufficient for I–II:
- Valoria exit/approach;
- forest resource identity;
- route depth;
- Corrupt scout;
- Engendro presentation;
- environmental Breach/corruption cue in the distance or terrain;
- readable march path / travel feedback.

Do not build the entire world map. Build the exact I–II path to benchmark quality first.

### Whole-frame acceptance

At representative mobile framing, no dominant visible element on the I–II path should read primarily as:
- developer primitive;
- isolated asset-store prefab;
- white/flat placeholder;
- floating architecture;
- generic empty terrain;
- debug UI.

---

## UI / UX scope for the first test

Required:
- safe-area/mobile-resilient HUD;
- resources and Total Power;
- compact objective/mission surface;
- contextual building interactions;
- construction/recruitment timers;
- March preparation;
- battle result;
- clear navigation Valoria ↔ Frontier;
- Bastion ascent moment;
- chapter I / II completion feedback;
- local reset / fresh-save path for repeated owner testing.

Not required yet:
- Códice;
- Relicario;
- Arcón depth;
- Granero;
- Cantera;
- Forge;
- Hospital;
- Maelis;
- full Hero Hall;
- PvP;
- ranking;
- later troop families;
- Bastion III+ progression.

Aldric can remain the only exposed hero for I–II.

---

## Technical policy

### Preserve
- current command/revision/idempotency model;
- persistence/offline completion;
- independent gameplay hotspots vs visual meshes;
- certified Valoria topology;
- Visual Formula v1;
- construction queue = one building/Bastion construction at a time.

### Do not over-refactor before test

`SlicePresenter.cs`, `VisualWorld.cs` and related files are growing and should be decomposed progressively, but a large rewrite before the I–II test creates more risk than value.

Allow only extraction directly needed by I–II, e.g.:
- March preparation UI/controller;
- objective/chapter presentation;
- combat-report presentation;
- content/balance config.

After I–II owner validation, schedule structural decomposition before porting the full Arc I breadth.

---

## Device / performance gate

Mobile-first means desktop editor/build success is insufficient.

Before calling I–II **mobile ready**:
- run on at least one representative physical phone;
- capture FPS/frame time;
- record peak memory and resolution;
- test pan/taps/safe areas;
- test Valoria overview, dense production district and Frontier combat;
- record thermal/sustained notes where possible.

The current ~460k-triangle Valoria scene is a measured editor geometry baseline, not a mobile budget.

Do not reduce visual quality pre-emptively. Optimize only measured bottlenecks.

---

## Execution order

### Track A — Playability (critical path)
1. canonical I–II balance/config audit vs web;
2. explicit March preparation;
3. objective/chapter presentation;
4. combat report + completion feedback;
5. fresh-save uninterrupted I→II owner-flow automated regression;
6. owner-build packaging/reset.

### Track B — Visual quality (parallel)
1. close current Bastion silhouette/roof Hero Pass;
2. polish early-Valoria life/reconstruction cues;
3. produce benchmark-quality I–II Frontier corridor;
4. official 19/12/9 + mobile visual review;
5. actual device performance profile.

### Track C — Next-content preproduction (non-blocking, no broad implementation)
While A/B execute:
- reserve Granero maximum envelope / district role;
- prepare Granero art brief from Bastion III contract;
- preserve Cantera / Forja / Hospital requirements in the building production inventory;
- do not generate/pay for them until I–II test readiness is no longer threatened by critical-path work.

---

## Owner playtest acceptance

A fresh player should be able to start from zero and, without developer intervention:

1. understand damaged Valoria / immediate need;
2. leave Valoria for the world;
3. gather wood and return;
4. rebuild Aserradero;
5. perceive visible Valoria recovery;
6. ascend Bastion to II;
7. understand the Cuartel role;
8. build Cuartel;
9. recruit the required Archers;
10. explicitly prepare a March;
11. understand the Engendro threat and why the force is ready/not ready;
12. fight and understand the result;
13. return to a visibly stronger Valoria;
14. receive a clear Bastion-II completion/new-ambition beat.

The build is not accepted merely because automated tests can reach the same state.

---

## After owner I–II validation

Only then:
1. fix owner feedback;
2. lock the I–II Unity product/UX pattern;
3. decide whether Bastion requires full hero rebuild or the existing Hero Pass is sufficient for the next production phase;
4. produce Granero / Bastion III district in canonical chronology;
5. progressively migrate the stable web systems instead of porting them all at once;
6. begin controlled code decomposition before system breadth grows substantially.
