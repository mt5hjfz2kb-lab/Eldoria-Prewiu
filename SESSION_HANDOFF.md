# Eldoria — SESSION HANDOFF
- **VALORIA AP2 — Cuartel generation/integration + Valoria v1 LookDev CLOSED (2026-09-29):** exact approved Cuartel input remains JPEG 1254×1254, 580,151 bytes, SHA-256 `24e88ca7a615769fcb72bcda9726e28d733d6b79ea9cc6d894f7686f19c2c5f9`. Owner authorization was used exactly once: Tripo run **36485397606** clicked Generate once and spent exactly **55 credits** (2685 → 2630), task `6d102e13-3f8d-403c-936a-f4dba2eb81c0`. Exported raw GLB: **68,162,660 bytes**, SHA-256 `d9ebd70a2993db449314f1025ca2f589741bc1771fa69a4cee899385b30045f4`; canonical Blender result **49,800 tris**; promoted production GLB SHA-256 `948a79e8f14be5ba4df1b98ddcb01d5312a3806f9fbdc4ecd8541960e0a5b980`. Cuartel keeps ~3.72×3.08×3.62 envelope, west/central street flank clear, visual colliders disabled and hotspot separate. PBR preservation is certified. The remaining overbright/washed LookDev issue was resolved by the zero-credit **valoria-v1-candidate** environment profile and applied to production without geometry regeneration or Tripo spend. Validation: LookDev run **36529204672 SUCCESS**, Unity slice **36529204570 SUCCESS**, Visual Formula **36529730848 SUCCESS**, artifact **11015563918**. Aserradero + Cuartel surface/LookDev gate is therefore closed. Full `VALORIA_VISUAL_FORMULA_v1` remains active only for later gates such as the bounded hero fragment; do not reopen Cuartel geometry or Tripo generation for this issue.

- **Blender post-Tripo refine proof (2026-09-28):** zero-credit Aserradero experiment completed on run **36483088548**, artifact **10997686762**. Existing canonical Blender pipeline now supports safe repository GLB staging plus optional refinement diagnostics/material cleanup. Aserradero stayed at **49,800 tris**; conservative cleanup reduced vertices **55,752 -> 50,447** and connected components **5,100 -> 2,636**, while UV/normals/3 textures remained present. Isolated Unity review PASS. This proves Blender can add useful cleanup/material control after Tripo, but does **not** yet prove autonomous high-quality sculpt/model redesign. Production Aserradero was not replaced. Use this stage selectively on future hero assets after Tripo.

Updated: 2026-09-29

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
1. keep the dedicated Aserradero geometry; fix only its pale/washed-out Unity material integration with a bounded pass;
2. audit the six certified Valoria families for **selective fragment reuse** (facades, rock, parapets, terraces, skyline pieces); they may support the art layer but must not dictate topology;
3. proceed directly to dedicated **Cuartel** and stronger **Bastion** architecture in the same larger execution block where practical;
4. increase inhabited density only after the three primary architecture reads are materially stronger;
5. close the block with official zoom 19/12/9 captures plus real click/panel regressions.

Avoid another chain of micro-passes. The next art block must create an obvious city-level visual jump. Do **not** move gameplay coordinates, the certified 12-step route, camera family or Master Envelope reservations merely to fit art. Keep execution prompts concise and task-focused; permanent rules belong in canonical docs rather than being repeated in every handoff.

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

## Recommended urgent security hardening — OPEN / non-blocking
- **Priority class:** recommended urgent, but **not the current production priority** and not a blocker for the active Valoria art block.
- As verified on **2026-09-29**, the canonical GitHub repository is currently **public**; unauthorised users cannot write to it, but public contents can be viewed/copied.
- Before external testers, broader public exposure, store/marketing preparation or any similar project-value milestone, perform the hardening in conservative stages:
  1. change the repository to **private**;
  2. immediately verify GitHub Actions, the self-hosted runner and required integrations still work;
  3. add appropriate protection/rules for `main` without blocking the canonical CI workflow;
  4. audit repository history/current tree for secrets, API keys, credentials and unnecessary sensitive material; rotate anything exposed where appropriate;
  5. verify a recoverable backup/recovery path before considering the hardening closed.
- **Reminder triggers:** explicitly surface this OPEN action again when preparing the first owner Unity playtest, before external testers, and before any deliberate public/store/marketing exposure.
- Do not perform a large one-shot security migration. Apply and validate each step separately so development continuity is preserved.

## Next handoff instruction
Before execution: read `AGENTS.md`, this file, `PROJECT_STATE.md`, `DESIGN_DECISIONS.md`, `docs/BASTION_I_X_MASTER_TABLE.md` when Arc I is relevant, and the specific Valoria specialist docs for art work; then verify live `main` HEAD and current workflow state.
