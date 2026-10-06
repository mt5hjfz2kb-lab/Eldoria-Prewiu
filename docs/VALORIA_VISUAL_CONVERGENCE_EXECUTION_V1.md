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
Current evidence:
- SHARP full-frame proof is the strongest visual-fidelity result.
- UnitySplats compiles and loads the sanitized SHARP PLY through decoding and Spark packing.
- Full 1.18M-splat proof crashes after asset packing completes.
- This is not a visual rejection.

Current bounded test:
- preserve original artifact bytes,
- create sanitized working copy,
- uniformly sample to ~300k splats,
- load as Spark,
- instrument renderer creation / assignment / refresh / camera render,
- capture HOME, bounded pan/zoom and occlusion probes.

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
