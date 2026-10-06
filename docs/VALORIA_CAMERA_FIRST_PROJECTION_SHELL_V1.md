# VALORIA CAMERA-FIRST PROJECTION SHELL v1

Status: ACTIVE — ZERO-SPEND TECHNOLOGY PROOF
Date: 2026-10-06

## Objective

Determine whether Eldoria can preserve the canonical visual reference through a hybrid 2.5D/3D camera-first presentation layer instead of reconstructing all visible hero art as conventional full 3D.

This proof does NOT redesign Valoria and does NOT integrate into Unity.

## Canonical source

Exact bytes:
`references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`

Materialization evidence:
- workflow run: 37422337426
- artifact: 11393816276
- no image regeneration
- no paid generation

## Core hypothesis

A depth-derived receiver mesh textured/projected with the canonical reference can preserve reference-class pixels at the canonical view while allowing bounded parallax across Eldoria's restricted camera envelope.

The first proof intentionally uses the complete canonical reference rather than an invented derived asset.

## Proof sequence

1. Estimate monocular depth from the exact canonical reference using **Depth Anything 3 BASE**.
   - model: `depth-anything/DA3-BASE`
   - license: Apache-2.0
   - commercial-use-compatible checkpoint selected intentionally
   - no paid API

2. Build a camera-space depth mesh.
   - deterministic grid
   - UVs map directly to exact reference pixels
   - reject triangles that cross large depth discontinuities
   - preserve the original image as emission/unlit projected color

3. Render:
   - canonical/base camera
   - small left-envelope offset
   - small right-envelope offset
   - optional stronger diagnostic offsets

4. Measure:
   - base-view pixel reconstruction error
   - visible hole/stretch rate on offset views
   - screen-space silhouette stability
   - qualitative parallax credibility

5. Decision:
   - PASS: base is visually near-identical and bounded offsets remain coherent enough to justify multi-projection development;
   - PARTIAL: base passes but offset views expose holes/stretching; proceed only to layered/multi-projection proof;
   - FAIL: base cannot preserve the canonical image or depth topology is unusable even at minimal offsets.

## Hard rules

- No Tripo.
- No Meshy.
- No paid generation.
- No Unity integration during this proof.
- No regenerated/invented replacement reference.
- Exact canonical reference pixels remain authority.
- Depth model predicts geometry only; it does not redesign architecture.
- Golden Surface V2 remains available later for physical normal/roughness/AO response, but is out of scope until the projection proof is visually credible.

## Adoption path if proof succeeds

`canonical reference -> segmentation/layers -> depth geometry -> projection receiver -> multi-camera projection blend -> Golden Surface V2 physical response -> Unity official-camera gate`

Dynamic gameplay elements remain genuine 3D:
- characters
- moving props
- water
- construction/upgrade effects
- gameplay colliders/hotspots

The projection shell is a visual presentation layer, never gameplay authority.
