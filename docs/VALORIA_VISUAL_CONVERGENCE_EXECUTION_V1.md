# VALORIA VISUAL CONVERGENCE EXECUTION v1

Status: ACTIVE PRODUCTION-METHOD CONVERGENCE
Date: 2026-10-06
Owner workstream: `valoria-visual-convergence-production-v1`

## Owner decision
Broad visual-method discovery is frozen. Existing research evidence is preserved in `docs/VALORIA_VISUAL_RND_DECISION_REGISTER_V1.md`, but no new generator/view-synthesis/depth/donor family may be opened unless this convergence workstream explicitly reopens one bounded dependency.

## Goal
Produce one repeatable Valoria visual-production recipe that can scale from the Lower Gate proof to the wider city while preserving:
- canonical visual identity,
- real interaction/clickability,
- bounded camera motion,
- real 3D depth/occlusion,
- Unity 6 URP compatibility,
- mobile/runtime feasibility,
- editable production assets where gameplay requires them.

## Locked convergence hypothesis
1. Canonical target/reference.
2. Semantic isolation where needed.
3. High-fidelity visual representation only where it survives runtime gates.
4. Real 3D interactive substrate.
5. Human/professional geometry correction for architecture that generators deform.
6. Target-conditioned appearance / PBR / depth alignment.
7. Photogrammetry or other donors only for justified surface relief.
8. Official-camera Unity integration.
9. Runtime + interaction + mobile validation.
10. Scale only after the isolated representative gate passes.

## Gate 1 — SHARP → Unity
**Current gate status: 300K TECH PASS / VISUAL PASS.**

Current evidence:
- SHARP full-frame proof is the strongest visual-fidelity result.
- UnitySplats compiles and loads the sanitized SHARP PLY through decoding and Spark packing.
- Full 1.18M-splat proof originally crashed after asset packing when driven without a bounded/runtime-safe lifecycle.
- A bounded ~300k proof now renders successfully in Unity 6 URP after sanitation, RDF coordinate correction and explicit D3D11 CPU-sort initialization.
- HOME + bounded pan preserve the canonical scene; behind-geometry probe is occluded, and a front probe renders in front.
- 300k shows expected sparse/punctate quality loss; this is now a density/optimization question, not a basic compatibility question.

Current bounded test — **PASSED at ~300k**:
- original artifact bytes preserved;
- sanitized working copy;
- uniform ~300k splat sample;
- Spark load;
- explicit renderer + D3D11 CPU-sort lifecycle;
- HOME, bounded pan/zoom and occlusion captures.

Next bounded test:
- raise proof density to ~600k splats using the identical lifecycle;
- compare visual fidelity and stability against the 300k artifact;
- only then consider full-density/runtime profiling.

Decision:
- PASS: scale density upward and measure performance/quality.
- MEMORY/GPU FAIL: find largest stable density and treat Gaussian as bounded visual layer.
- RENDER/PIPELINE FAIL independent of density: demote SHARP from runtime layer; retain as visual target/reference evidence and converge on real-mesh + appearance-transfer route.

## Gate 2 — production geometry
Do not reopen generator bake-off.
Use only already-qualified evidence:
- TRELLIS semantic: base 3D donor.
- Hi3DGen seed 7: fixed-seed donor only; high variance prevents autonomous generation.
- InstantMesh: fast blockout donor.
- Human-authored modular/CC0 or professional mesh: preferred correction source where architectural identity matters.
- Hunyuan/CraftsMan: donor only.
- TripoSR: visual fail for hero art.

A geometry candidate passes only if silhouette and architectural identity meet the isolated source gate before lookdev.

## Gate 3 — appearance/depth
Allowed components:
- Human-Mesh / Geometry-Anchored appearance projection.
- Target-derived PBR.
- Marigold continuous depth for alignment/occlusion assistance.
- Photogrammetry donor only with a visually compatible medieval-stone source.
- SCoPE/SHARP only as representation layers after runtime proof.

## Stop rule
No new R&D family while Gates 1–3 can progress with existing evidence.
A new method may be opened only when a named convergence gate is blocked and the new method addresses that exact blocker without duplicating a rejected route.

## Promotion rule
Production method is locked only after:
- isolated Lower Gate visual comparison,
- multi-angle/camera-envelope stability,
- real Unity interaction substrate,
- occlusion/depth proof,
- runtime feasibility,
- official-camera integrated evidence.

Green CI, polygon count, single frontal beauty shot, or one lucky seed never count as production certification.


## Gate 1 result — SHARP in Unity
**Verdict: RUNTIME REPRESENTATION PASS / VISUAL PARTIAL / PRODUCTION CLEANUP REQUIRED**

Evidence:
- Run: `37455323684`
- Artifact: `11409152206`
- Bounded proof: ~294,912 splats sampled from 1,179,648 source splats.
- UnitySplats runtime PLY load: PASS.
- Renderer initialization / CPU-sort lifecycle in batchmode: PASS.
- HOME capture: PASS — visible Valoria scene.
- bounded pan-left / pan-right: PASS — scene remains visible and preserves nearby-view parallax.
- zoom captures: produced.
- front raster probe: appears in front of Gaussian layer.
- behind probe: occluded by Gaussian scene in captured evidence.
- paid credits: 0.

What this proves:
- SHARP is no longer only an offline visual-reference technique.
- A SHARP Gaussian representation can be loaded and rendered in Unity 6 URP using the current project runtime.
- Real raster/3D gameplay substrate can coexist with the Gaussian layer in the same camera/render pipeline.

What this does **not** yet prove:
- production-ready image cleanliness,
- final mobile performance,
- final full-density quality,
- free camera movement outside the bounded nearby-view envelope,
- per-building click/selection from Gaussian data itself.

Visible production issues:
- the current SHARP source was generated from a canonical screenshot containing UI/HUD regions, so those UI elements are baked into the Gaussian representation;
- the 300k uniform proof sample visibly aliases/sparsifies detail compared with the offline SHARP synthesis;
- the source/render still needs exposure/color calibration and a clean production framing.

**Production consequence:** SHARP is approved as a candidate high-fidelity visual representation layer inside the hybrid Valoria pipeline. It is not approved as the sole gameplay geometry or as final art in its current source form.

### Gate 1A — clean visual source
Next production action:
1. use a canonical Valoria target with UI/HUD removed before SHARP generation;
2. preserve the current contaminated artifact only as technical evidence;
3. regenerate SHARP from the clean target;
4. repeat the bounded Unity capture at 300k, then progressively test 450k / 600k / highest stable density;
5. choose the best quality/performance density before moving to integrated interactive geometry.

No new visual-method R&D is authorized for this gate.


## Gate 1B — 600k convergence density
**Verdict: TECH PASS / VISUAL PASS / NEW PROVISIONAL BASELINE**

Evidence:
- Run: `37455916168`
- Artifact: `11409767660`
- Working density: ~600k splats via the same bounded/sanitized source path and UnitySplats lifecycle proven at ~300k.
- HOME: PASS.
- bounded pan-left / pan-right: PASS.
- zoom captures: PASS.
- occlusion-front / occlusion-behind: PASS.
- paid credits: 0.

Direct visual comparison versus ~300k:
- 300k shows severe punctate/sparse breakup and loses architectural readability.
- 600k materially restores the Lower Gate, walls, stair, road, vegetation and camp silhouettes into a coherent scene.
- Bounded pan remains visually stable enough for the current camera envelope.
- Occlusion proof remains functional.

Decision:
- **Adopt ~600k as the provisional SHARP Unity convergence baseline.**
- Do not scale blindly beyond 600k until the clean-source gate is complete.
- The current source still contains baked HUD/UI and is therefore evidence-only, not production-clean art.

### Gate 1A — clean production source
Next action:
1. materialize/reuse the already-located canonical clean candidate if valid:
   `art-source/valoria/lookdev/golden-slice-v1/camera-first-reset-target-v1/canonical-golden-crop.png`
   and compare against `semantic-base.png`;
2. verify that the chosen source contains no baked gameplay HUD/UI and preserves the approved visual frame;
3. regenerate SHARP from that clean source;
4. repeat the ~600k Unity proof using the exact Gate 1B lifecycle;
5. only after this PASS may SHARP be considered a production visual layer.

Current tooling note:
- GitHub text/blob connector cannot materialize these binary PNG/JPG blobs directly in the present session (`fetch_blob` UTF-8 decode limitation).
- This is a tooling/materialization blocker for Gate 1A source inspection, not a visual-method blocker.


## Gate 1A/1B lock — clean SHARP 600k
**Verdict: PRODUCTION BASELINE PASS for bounded high-fidelity visual representation**

Canonical evidence:
- Clean SHARP source run: `37456084187`
- Clean SHARP source artifact: `11408853949`
- Unity clean 600k run: `37456541286`
- Unity clean 600k artifact: `11409359136`
- Source: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`
- proof_splat_count: 589,824
- source_splat_count: 1,179,648
- paid credits: 0

Visual/runtime result:
- HOME: PASS — HUD-free Valoria identity is retained in Unity 6 URP.
- bounded pan left/right: PASS — nearby-view parallax remains coherent enough for the locked camera envelope.
- zoom in/out: PASS.
- raster foreground probe: PASS — real Unity geometry can render in front of the Gaussian layer.
- raster behind probe: PASS — behind-probe remains occluded by the Gaussian scene.
- runtime load / Spark packing / D3D11 CPU sort: PASS.

Known limitations:
- top-of-frame has uncovered black area outside useful Gaussian coverage; production camera/background framing must avoid or cover it.
- 600k is a visual baseline, not yet a mobile performance certification.
- Gaussian scene is a visual representation layer, not gameplay geometry.
- wider free-camera motion is not authorized by this evidence.

**LOCK:** use clean approved-reference SHARP at 600k as the current high-fidelity visual-layer baseline. Do not reopen visual-generator discovery while this convergence path can progress.

## Gate 2 — interactive 3D substrate alignment
Goal:
- keep the locked SHARP visual layer;
- add real Unity 3D proxy geometry/colliders for a representative architectural interaction region;
- prove screen-space selection/raycast mapping, depth coexistence and bounded-camera stability;
- keep gameplay interaction independent from Gaussian visual data.

Gate 2 PASS requires:
1. at least three named interactive proxy regions aligned to visible architecture;
2. deterministic camera-ray hit evidence from representative screen points;
3. no visual break of SHARP HOME/pan envelope;
4. front/behind depth ordering remains valid;
5. proxy geometry can be hidden from beauty render while remaining selectable/collidable;
6. evidence JSON maps screen point → proxy ID → world hit position.


## Gate 2 — interactive substrate HOME
**Verdict: TECH PASS / INTERACTION PASS AT HOME / CAMERA-STABILITY UNPROVEN**

Evidence:
- Run: `37457903922`
- Artifact: `11410576612`
- Clean SHARP source: artifact `11408853949`
- Unity visual layer: 589,824 splats
- Invisible interaction proxies: WestTower, CentralKeep, EastTower, LowerGate
- Deterministic HOME raycasts: **4/4 PASS**
- beauty-home: PASS; proxies remain invisible in production beauty
- paid credits: 0

Raycast evidence:
- WestTower → WestTower
- CentralKeep → CentralKeep
- EastTower → EastTower
- LowerGate → LowerGate

What this proves:
- a clean SHARP Gaussian visual layer can coexist with standard Unity colliders/raycast interaction;
- gameplay selection does not need to come from Gaussian data itself;
- the high-fidelity representation can remain purely visual while Unity owns interaction/gameplay semantics.

What this does **not** yet prove:
- proxy-to-visible-architecture alignment across bounded camera movement;
- stable selection through pan/zoom;
- gameplay callbacks/state changes;
- mobile/runtime budget.

Important evidence caveat:
- debug proxy solids are depth-occluded by the Gaussian layer, so `proxy-debug.png` is not sufficient visual alignment evidence by itself.
- Gate 2 is therefore promoted only as a HOME interaction substrate pass.

### Gate 2B — bounded camera interaction stability
Next production gate:
1. keep the same four world-space proxies;
2. test HOME, bounded pan-left, pan-right, zoom-in and zoom-out;
3. project each proxy into each camera view and raycast back through its projected screen point;
4. require the same semantic ID on every visible view;
5. record projected viewport coordinates, pass/fail and selection state;
6. preserve a clean SHARP beauty capture for every camera state;
7. no new visual method or generator is authorized.
