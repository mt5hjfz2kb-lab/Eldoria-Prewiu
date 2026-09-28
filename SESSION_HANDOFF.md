# Eldoria — SESSION HANDOFF

Updated: 2026-09-28

This file is intentionally **operational and short**. Historical execution detail belongs in git history, specialist docs and `CHANGELOG.md`. Permanent working rules live in `AGENTS.md`; current functional product state lives in `PROJECT_STATE.md`; durable design decisions live in `DESIGN_DECISIONS.md`.

## Live development line
- Repository: `mt5hjfz2kb-lab/Eldoria-Prewiu`.
- Active branch: **`main` only**.
- Verify real `main` HEAD at the start of every execution block; do not infer it from chat.
- Active web milestone/reference: **v0.32.0**.
- Active Unity source: `Unity/`, Editor **6000.3.23f1**.
- Web remains the canonical playable design/reference until Unity replacement/parity is explicitly approved.

## Latest certified Valoria state
### Playable District v1 — ART PASS 1 CERTIFIED
- Certified playable/art checkpoint: `26cec966b2495c0dc1d0f925ffd5aa6e7793f9a3`.
- Unity Actions run: **36441851841**.
- Full Unity checks artifact: **10978669163**.
- Official Valoria captures artifact: **10979357857**.
- Result: **TECH PASS / INTERACTION PASS / VISUAL PASS — ART PASS 1**.
- Frozen kernel topology, gameplay coordinates, 12-step route, official isometric orientation, zoom 9/12/19, bounded pan and Master Envelope reservations remain protected.
- Art Pass 1 removes the dominant rectangular board/plinth read and integrates terrain/support/circulation visually; it is **not final architecture**.
- Record: `docs/VALORIA_PLAYABLE_DISTRICT_ART_PASS_1.md`.

### Master Envelope + Camera Gate v1 CERTIFIED
- Certification checkpoint: `c8135a8c162e7849d852472d67e2492cd8a88af4`.
- Unity Actions run: **36432979557**.
- Capture artifact: **10974741738**.
- Camera: fixed authored isometric orientation, zoom 9..19, bounded panning, progression-aware bounds and recenter.
- Tap-vs-drag and real Aserradero/Cuartel/Bastion hotspot selection after pan are validated.
- The Playable District Skeleton is the **kernel**, not the whole city. Long-range planning preserves headroom toward roughly Bastion 25–35.
- Record: `docs/VALORIA_MASTER_ENVELOPE_CAMERA_GATE_V1.md` plus the three master-plan documents.

## Latest published preview status
- `Publish Eldoria Preview` run **36443320468** completed successfully on commit `13a3a39069fdb47fb2d65b88d5446834c1e135ca`.
- Full local-equivalent certification gate: PASS.
- GitHub Pages deploy: PASS.
- Published Chromium verification: PASS.
- Frozen tester URL guard: PASS.

## 2026-09-28 canonical documentation cleanup
- `DESIGN_DECISIONS.md` was rewritten as a concise **current canonical decision set**, removing historical contradictions.
- Added `docs/BASTION_I_X_MASTER_TABLE.md` as the canonical Bastion I–X audit/table.
- Permanent preservation rule is explicit: **validated vertical-slice decisions remain valid in Unity unless a later explicit decision supersedes them**.
- Códice / Relicario / Arcón responsibilities are reconciled to current product state:
  - Códice = world knowledge/discovery;
  - Relicario = Reliquia/card collection/use/Practice/future Duel;
  - Arcón = objects/materials/equipment.
- Códice and Relicario are independent peer systems.
- Cantera role is closed: **stone production**.
- Relicario teaching is staged from Bastion VII onward instead of dumping the whole card system at first contact.
- Platform decision is explicit: **mobile-first, tablet-supported, PC-ready as a future option but not committed**.
- Bastion ascent contract is explicit: every level communicates **what changed/was learned + visible Valoria growth + new action/capability**.
- Bastion IX = independent mastery; Bastion X = tutorial/prologue graduation, not game completion.

## Canonical Bastion I–X status
Read `docs/BASTION_I_X_MASTER_TABLE.md` before changing Arc I progression.

Current genuine design gaps:
1. **Bastion IV identity** — exact autonomy challenge + visible Valoria consequence.
2. **Bastion V tuning** — which upgrades begin requiring stone and ratios; Cantera’s fundamental purpose is already closed.
3. **Bastion VII teaching cadence** — stage Relicario onboarding across VII–IX/post-X.
4. **Bastion X finale feel** — graduation/ceremony pacing so it opens desire for the larger game.
5. **XI+ progression** — separate post-prologue structure; do not repeat tutorial beats.

## Aserradero AP2 dedicated asset checkpoint
- Owner authorized **55 Tripo credits** for the exact approved image; generation/export completed successfully.
- Source GLB SHA-256: `874e3344d7c09da153fcfeaf012d3d542fdf4d55ca8eb198078646660c4ad37b`.
- Canonical Tripo->Blender->Unity pipeline run **36461987549**, artifact **10987829034**: raw **1,796,171 tris** -> optimized **49,800 tris**, UV/normals/material present.
- Dedicated GLB is integrated in real Valoria at `Unity/Assets/Eldoria/Resources/Valoria/Valoria_Aserradero_AP2_v1.glb`; parcel/click/street-clear regression is covered by PlayMode.
- Latest integrated checkpoint: `d633a5482a321a75cc4cf11a4a0e548e6cf8f1bd`.
- Final Unity run **36472860786**: TECH PASS / INTERACTION PASS; full checks artifact **10992473730**, official captures **10991679902**.
- Visual verdict: dedicated architecture/function read **PASS**, but integrated material remains too pale/washed out, so final visual **FAIL**. Do not regenerate geometry or spend more Tripo credits yet; fix Unity material/lighting integration first.
- Detailed record: `docs/VALORIA_PLAYABLE_DISTRICT_ART_PASS_2_ARCHITECTURE.md`.

## Immediate production priority
The next **Unity/art execution** remains Art Pass 2 inside the certified/frozen envelopes:
1. first audit the six certified Valoria families for **selective fragment reuse** (facades, rock, parapets, terraces, skyline pieces); they may support the art layer but must not dictate topology;
2. dedicated **Aserradero architecture** only where existing/reusable art cannot reach the target quality;
3. dedicated **Cuartel architecture**;
4. stronger **Bastion silhouette / roof hierarchy**;
5. increased inhabited density while preserving interaction, circulation and camera readability.

Do **not** move gameplay coordinates, the certified 12-step route, camera family or Master Envelope reservations merely to fit art. Keep execution prompts concise and task-focused; permanent rules belong in canonical docs rather than being repeated in every handoff.

## Product-direction work that can proceed in parallel
- Close Bastion IV’s exact autonomy beat.
- Tune the first stone requirements at Bastion V.
- Convert the staged Relicario teaching cadence into exact mission/tutorial beats.
- Later map the Bastion-by-Bastion visible Valoria changes onto the certified long-term city plan.

## Platform / delivery direction
- Primary product validation: phone/mobile.
- Tablet is supported from the beginning with responsive layout/use of extra space.
- PC remains a viable future expansion; preserve architecture/controls/UI seams that avoid unnecessary mobile-only lock-in.
- Continue the short owner-review loop used in the web slice: **build a small coherent change → certify it → let the owner see/play it → adjust before stacking more work**.
- Unity delivery can use Windows builds now; mobile build/distribution infrastructure remains to be prepared for regular owner device testing.

## Permanent safety/source rules
- Repo + canonical docs beat chat memory.
- Never spend Tripo credits or paid resources without explicit authorization.
- Do not fabricate multiplayer, PvP, alliance/rally participants, rewards or state.
- Do not call a change certified without the relevant gate/evidence.
- Frozen tester snapshot remains immutable research output.
- For new Valoria art work, start at `docs/ELDORIA_ART_PIPELINE_INDEX.md`, `docs/VALORIA_MODULE_KIT.md` and the master-plan/camera documents.

## Next handoff instruction
Before execution: read `AGENTS.md`, this file, `PROJECT_STATE.md`, `DESIGN_DECISIONS.md`, `docs/BASTION_I_X_MASTER_TABLE.md` when Arc I is relevant, and the specific Valoria specialist docs for art work; then verify live `main` HEAD and current workflow state.
