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
