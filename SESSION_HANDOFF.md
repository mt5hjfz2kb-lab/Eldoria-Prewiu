- **WORLD MAP 4X VISUAL BENCHMARK v1 LOCKED (2026-09-29):** world-map art is now judged as a fixed-direction mobile 4X map, not as isolated asset beauty shots. Canonical gate requires far/normal/near/detail zooms at orthographic 18/14/10/7 plus 390x844 mobile; assets must preserve silhouette, biome massing, route/node readability, repetition resistance and low gameplay-noise. New benchmark: `docs/WORLD_MAP_VISUAL_BENCHMARK_V1.md`. Slavic foliage remains rejected; Slavic hard-surface remains conditional. Immediate comparison baseline is current Frontier vs existing NatureStarterKit2 vs external free Holotna Mountain vs Jermesa Hill Rock/Mountain before any production promotion. No paid asset purchase or Tripo spend authorized by this block.

- **WORLD MAP FOREST RESOURCE KIT v1 REJECTED (2026-09-29):** real zero-cost Frontier experiment tested EmaceArt Slavic World Free tree/tall-tree/bush/moss/firewood/flat-rock assets as a reusable forest composition with Eldoria scaling/material modulation and independent gameplay hotspot. Gate **36590669809 SUCCESS**, artifact **11044780827** proved technical integration, but official/focused captures were **VISUAL FAIL**: foliage reads too artificial/asset-pack-specific, greens become conspicuously neon, silhouettes repeat and undergrowth does not blend into terrain. The temporary kit/integration/gate were removed and Frontier's previous forest restored. Slavic remains selectively usable for low-salience hard-surface props (rocks, roads, fences, firewood, sacks/crates/barrels) only after camera validation. Record: `docs/WORLD_MAP_FOREST_RESOURCE_KIT_V1_RESULT.md`.

- **GRANERO BIII DEDICATED ASSET CLOSED (2026-09-29):** exact approved input `Valoria_Granero_BIII_v1_APPROVED_EXACT.jpeg` = 1254×1254, 602,970 bytes, SHA-256 `2d58bfe83064601223ad0027963c2977acbde10b84fdefe528db4549215c7c7a`. Tripo used the single owner-authorized Generate at visible cost **55 credits**; exported raw GLB **69,308,356 bytes**, SHA-256 `b14cfbf85d608aac4a3b551a3804df57f8dc866264c786565e496cfac7fe647b`. Canonical Blender reduced **1,826,722 -> 49,800 tris** with UV/normals/PBR preserved; certified production GLB = **12,951,524 bytes**, SHA-256 `d968bf51ba730007dd34d90c344c27467a768a36d79049abaf9ae3e174d87c67`. Oriented isolated gate run **36572780639 SUCCESS**, artifact **11036129405** = TECH PASS / VISUAL PASS; yaw 180 establishes south/front. Certified production GLB promoted by run **36575066762** to `Unity/Assets/Eldoria/Resources/Valoria/Valoria_Granero_BIII_v1.glb`. Runtime integration is visual-only on the locked west-growth Bastion III plot near (-17.2,0.34,-3.0), replacing only a non-authoritative placeholder house; imported colliders remain disabled. Integrated Visual Formula run **36581806788 SUCCESS**, artifact **11040631044**, includes dedicated Granero captures at zoom 12/9/mobile and confirms the asset reads correctly in Valoria without blocking the west route. Full Unity slice **36581806847 SUCCESS**: EditMode + PlayMode + Windows build + Valoria benchmark PASS; artifacts **11040582911** (checks) and **11041280293** (Valoria captures). `Valoria.unity` was not modified. Do not regenerate this asset unless a later explicit redesign supersedes certification.

# Eldoria — SESSION HANDOFF
- **TRIPO UPLOAD HARDENING (2026-09-29):** canonical chat-approved input flow is now fail-fast and resumable. `tools/tripo-studio-bridge.mjs` performs Tripo-session preflight, bounded upload retries, waits for stable UI receipt, verifies expected dimensions + visible Generate cost, records before/after screenshots, `tripo-upload-verification.json` and `tripo-flow-state.json`, and keeps `stage_upload` at zero spend. `tools/validate-tripo-exact-input.mjs` + per-asset `manifest.json` now validate repo exact-input SHA/bytes before staging; `AGENTS.md` requires every newly approved ChatGPT image to be persisted to repo exact-input in the same execution block. Downloads/visual matching are fallback only. Granero remains safely parked with `enabled=false` because its complete approved JPEG bytes are not yet present in the repo; do not substitute the Library PNG or search Downloads. No Tripo credits were spent by this hardening work.

- **GROUND KIT v1 CERTIFIED / NEXT = GRANERO EXACT INPUT (2026-09-29):** reusable visual-only Ground Kit (StreetStraight, StreetBlendWidening, TerraceFloor, RetainingEdge, GroundSeam) is now PRODUCTION BASE / PASS. Evidence: Visual Formula **36554941017 SUCCESS**, artifact **11027775838**; LookDev **36554945846 SUCCESS**; Unity slice **36554920243 SUCCESS** with EditMode + PlayMode + Windows build + Valoria benchmark all green. The kit preserves certified topology/hotspots and uses zero Tripo credits. Next step is **Valoria_Granero_BIII_v1** exact isolated 3/4 image → repo exact-input identity → Tripo stage_upload → visible credit cost → STOP before Generate for owner approval.

- **GROUND KIT v1 IMPLEMENTED / VISUAL VALIDATION ACTIVE (2026-09-29):** reusable visual-only ground library now exists in `ValoriaGroundKit.cs` with StreetStraight, StreetBlendWidening, TerraceFloor, RetainingEdge and GroundSeam, and is assembled into the real Valoria kernel. Commits `49e8548...` + `ff34e38...`. No Tripo credits used. The first Unity slice run was superseded by an unrelated concurrent `SliceBoot.cs` change; do not interpret the cancellation as a kit failure. Do not begin paid Granero generation until Ground Kit visual evidence at official cameras is reviewed.

- **VALORIA REUSABLE CONSTRUCTION LIBRARY v1 (2026-09-29):** canonical plan added at `docs/VALORIA_LIBRARY_PRODUCTION_PLAN_V1.md`. Production must now treat Valoria as an assembly library: approved topology → reusable Ground Kit → reusable Support architecture → Dedicated/Hero building → progression dressing. **Immediate art priority is Ground Kit v1 before Granero**, because streets/terraces/retaining edges are shared by every current/future district and the visual benchmark rejects flat board/test-platform reads. Ground Kit is visual-only and must not own gameplay collision/topology. After Ground Kit passes, next dedicated building remains **Granero** (Bastion III), developed alongside Residential/Food Support Kit v1. No paid generation is authorized by this decision.

- **MOBILE DEVICE PROFILING BLOCKER IDENTIFIED (2026-09-29):** existing `unity-slice.yml` now performs a non-blocking mobile-readiness audit on the Windows runner. Real runner audit in run **36549512711**, job **109344102250** completed the audit step successfully and reported **Unity Android Build Support = false**, **adb = absent**, therefore no physical Android device can be profiled from CI yet. This is now recorded in `pipeline/mobile-visual-performance-gate.json`. Next prerequisite is to install Android Build Support for Unity **6000.3.23f1** on `DESKTOP-R10PE55`; after installation, rerun the existing workflow, verify SDK/NDK/JDK/adb, connect/authorize a representative phone, then measure FPS/frame time/memory/batches/thermal data. Do not reduce Valoria quality before that device evidence exists.

- **VALORIA WEST REBUILDERS QUARTER v1 CLOSED (2026-09-29):** west Master Envelope reserve now contains a real inhabited/reconstruction production extension inside `VisualWorld`: staggered housing, two work courts, six route fragments, two upper dwellings and two visual-only `RockTerrainSeamFiller` geology instances. Bastion Hero Pass also removed the last detached/floating crown read. Visual Formula **36546445386 SUCCESS**, artifact **11022647974**; LookDev **36546445341 SUCCESS**; measured scene **567,460 tris / 644 renderers / 80 materials / 15 lights**. Unity slice **36546445337** first attempt completed EditMode + PlayMode + Windows build + Valoria benchmark successfully before the workflow was superseded/cancelled by newer concurrent main work; treat that as concurrency, not a product-test failure. Mobile baseline is updated in `pipeline/mobile-visual-performance-gate.json`; no quality reduction before physical-phone profiling. Record: `docs/VALORIA_WEST_REBUILDERS_QUARTER_V1.md`.

- **BASTION I–II OWNER PLAYTEST READINESS (2026-09-29):** active priority is the first human-playable Unity I–II quality wedge, not broad Arc-I migration. Canonical plan: `docs/BASTION_I_II_PLAYTEST_READINESS_PLAN.md`; parity audit: `docs/BASTION_I_II_WEB_UNITY_PARITY_AUDIT.md`. Unity now has authoritative mission progress, early forest + quarry lessons, explicit route-clear before Bastion II, persisted March confirmation + Expedition Power, structured battle reporting, and one fresh-save domain regression covering the whole I→II loop through the Engendro. `QA_FAST` remains active; `OWNER_I_II_CANDIDATE` is staged from the web contract but deliberately inactive until integrated CI/build + human pacing validation. Do not duplicate these systems.

- **VALORIA BUILDING PRODUCTION INVENTORY v1 (2026-09-29):** new canonical art-production map: `docs/VALORIA_BUILDING_PRODUCTION_INVENTORY_V1.md`. Arc-I dedicated functional art still known missing: **Granero, Cantera, Forja, Hospital**; Bastion remains a separate HERO line. **Códice and Relicario are UI/meta systems and do not require dedicated Valoria buildings or plots.** Before any paid generation/purchase, identify the canonical need, plot/envelope and asset class; the city dictates assets, not the reverse.

- **VALORIA FIRST PRODUCTION DISTRICT v1 CLOSED (2026-09-29):** `VALORIA_VISUAL_FORMULA_v1` is **VALIDATED / FROZEN** and the first real production district now consumes it in `VisualWorld`. Production uses dedicated Aserradero + Cuartel, rescued `ResidentialTerraceRock` on the certified upper civil plot and `RockTerrainSeamFiller` x3; rescued visual meshes remain independent from gameplay/colliders/hotspots. `TerraceStairRock` was recovered and fit-tested but intentionally omitted here because it was redundant/occluded against the certified 12-step route. Official district gate **36543056731 SUCCESS**, artifact **11021850361**, zoom **19/12/9**, real topology `REAL_VISUALWORLD_PRODUCTION_DISTRICT`; district-focused metrics: **428 active renderers / 55 unique materials / 460,236 scene tris / 8 lights**. Production formula overview **36541695034 SUCCESS**, artifact **11021096851**; full Unity slice **36541695042 SUCCESS** (EditMode + PlayMode + Windows build + capture); LookDev recheck **36541695060 SUCCESS**. Historical rescue/integration spent **0 Tripo credits**. Mobile gate now records a measured editor baseline; no visual-quality reduction is allowed before representative target-device profiling. Record: `docs/VALORIA_FIRST_PRODUCTION_DISTRICT_V1.md`.

- **VALORIA AP2 — Cuartel generation/integration + Valoria v1 LookDev CLOSED (2026-09-29):** exact approved Cuartel input remains JPEG 1254×1254, 580,151 bytes, SHA-256 `24e88ca7a615769fcb72bcda9726e28d733d6b79ea9cc6d894f7686f19c2c5f9`. Owner authorization was used exactly once: Tripo run **36485397606** clicked Generate once and spent exactly **55 credits** (2685 → 2630), task `6d102e13-3f8d-403c-936a-f4dba2eb81c0`. Exported raw GLB: **68,162,660 bytes**, SHA-256 `d9ebd70a2993db449314f1025ca2f589741bc1771fa69a4cee899385b30045f4`; canonical Blender result **49,800 tris**; promoted production GLB SHA-256 `948a79e8f14be5ba4df1b98ddcb01d5312a3806f9fbdc4ecd8541960e0a5b980`. Cuartel keeps ~3.72×3.08×3.62 envelope, west/central street flank clear, visual colliders disabled and hotspot separate. PBR preservation is certified. The remaining overbright/washed LookDev issue was resolved by the zero-credit **valoria-v1-candidate** environment profile and applied to production without geometry regeneration or Tripo spend. Validation: LookDev run **36529204672 SUCCESS**, Unity slice **36529204570 SUCCESS**, Visual Formula **36529730848 SUCCESS**, artifact **11015563918**. Aserradero + Cuartel surface/LookDev gate is therefore closed. Full `VALORIA_VISUAL_FORMULA_v1` remains active only for later gates such as the bounded hero fragment; do not reopen Cuartel geometry or Tripo generation for this issue.

- **Blender post-Tripo refine proof (2026-09-28):** zero-credit Aserradero experiment completed on run **36483088548**, artifact **10997686762**. Existing canonical Blender pipeline now supports safe repository GLB staging plus optional refinement diagnostics/material cleanup. Aserradero stayed at **49,800 tris**; conservative cleanup reduced vertices **55,752 -> 50,447** and connected components **5,100 -> 2,636**, while UV/normals/3 textures remained present. Isolated Unity review PASS. This proves Blender can add useful cleanup/material control after Tripo, but does **not** yet prove autonomous high-quality sculpt/model redesign. Production Aserradero was not replaced. Use this stage selectively on future hero assets after Tripo.

- **BASTION IV + construction queue CLOSED (2026-09-29):** web vertical slice now makes Bastion IV the first explicit governance choice: Production/Works vs Defense vs Shelter/Population, with a visible Valoria consequence and no permanent content lock. Canonical mission copy/target is updated and the decision is required before Bastion V. Current construction rule is also locked to **one simultaneous building construction/upgrade**; gathering, troop training and Hospital treatment remain separate task families. Preserve both decisions when the corresponding flow migrates to Unity.

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
1. **Bastion V tuning** — which upgrades begin requiring stone and ratios; Cantera’s fundamental purpose is already closed.
2. **Bastion VII teaching cadence** — stage Relicario onboarding across VII–IX/post-X.
3. **Bastion X finale feel** — graduation/ceremony pacing so it opens desire for the larger game.
4. **XI+ progression** — separate post-prologue structure; do not repeat tutorial beats.

**Bastion IV is now CLOSED in the web vertical slice:** the player must choose Production/Works, Defense, or Shelter/Population; the choice has an immediate resource emphasis plus a visible Valoria cue and is required before Bastion V. Preserve this contract when the corresponding Unity progression is migrated. Web construction queue v1 also allows only **one active building/Bastion construction or upgrade at a time**; gathering, recruitment and Hospital treatment remain separate queues.

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
Valoria has moved from visual-formula validation into production scaling.

1. Keep `VALORIA_VISUAL_FORMULA_v1` frozen; do not reopen broad tool/LookDev investigation without new integrated evidence.
2. Expand from the certified first production district into the next adjacent city section, reusing certified geometry before requesting paid generation.
3. Use `ResidentialTerraceRock` and `RockTerrainSeamFiller` as proven production reuse precedents; `TerraceStairRock` remains available for a later transition where it genuinely improves composition.
4. Continue strengthening Bastion/hero architecture and inhabited density without moving frozen gameplay topology, the 12-step route, camera family or Master Envelope reservations.
5. Before any graphics downgrade, profile the current measured production scene on a representative target phone using `pipeline/mobile-visual-performance-gate.json`; derive SUPPORT / PRIMARY / HERO budgets from actual device evidence.

Avoid returning to isolated-module experimentation unless a specific production defect requires it. The next visual block should extend a real district, not rebuild the factory.

## Product-direction work that can proceed in parallel
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

## 2026-09-29 Bastion I-II OWNER candidate checkpoint
- Gameplay/profile certification run **36559605933**: SUCCESS.
- Certified gameplay/profile SHA: **447d6e934a6fd72760cf88c97f9c889ca749f693**.
- EditMode, PlayMode, Windows build and official benchmark capture all passed.
- `QA_FAST` remains the normal/default runtime.
- `OWNER_I_II` is explicit opt-in with `--eldoria-profile=OWNER_I_II`; it does not silently replace QA.
- Windows artifact now includes `PLAY_OWNER_I_II_CANDIDATE.bat` and OWNER uses a profile-scoped save.
- Bastion-II UX now tells the player to return to the world for missing recruitment resources rather than leaving an unexplained blocked recruit state.
- Current Frontier baseline is the `4a282d32...` readability pass. The later stronger production experiment `0a07dfad...` was deliberately reverted by `c962f0f...` after visual regression; do not resurrect it blindly.
- Next product gate is no longer another automated reachability pass: run the owner candidate from fresh save end-to-end without developer knowledge and capture pacing/clarity/visual feedback before promoting OWNER values or expanding to Bastion III.
