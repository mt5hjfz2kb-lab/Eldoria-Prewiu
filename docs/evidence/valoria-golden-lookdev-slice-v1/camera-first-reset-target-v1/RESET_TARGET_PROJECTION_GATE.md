# VALORIA CAMERA-FIRST RESET TARGET v1 — SOURCE/PROJECTION GATE

Status: **TECH PASS / CORE VISUAL PASS / BOUNDED EDGE-INTEGRATION UNKNOWN / ISOLATED UNITY PROOF AUTHORIZED**
Date: 2026-10-06

## Authority

Current artistic authority:
`docs/evidence/valoria-art-production-reset-v1/canonical-target.jpeg`

SHA-256:
`8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689`

The earlier proof based on `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg` remains technology evidence only and is explicitly non-canonical.

## Authoritative corrected proof

Workflow:
`[PROOF] Valoria Reset-Target Orthographic Layers v1`

Corrected run:
- run: **37427102468**
- artifact: **11395715446**
- artifact digest: `sha256:d6a854a92e3f6f689efc898b720ecb12e3c2db96147b34c254606ac3d6044999`
- credits: **0**
- Unity touched: **false**

Golden crop:
- source box: `x=300..1000, y=420..960`
- raster: **700×540**
- contents: Lower Gate + Bridge + adjacent cliff/ground/water
- HUD excluded

## Corrections versus run 37426291260

The first canonical run proved the technique but had two implementation defects:
1. Blender orthographic framing interpreted scale without the render aspect, producing an over-tight crop.
2. transparent Mix Shader layers did not provide the desired depth-write behavior for dynamic occlusion.

Run 37427102468 corrects both:
- aspect-correct orthographic framing;
- alpha-clipped, depth-writing layer materials.

## Visual review

### Canonical base

**PASS.**

The corrected base reproduces the source crop with no geometry reinterpretation and no visible framing drift.

Measured against the exact 700×540 canonical crop:
- mean absolute pixel error: **2.48 / 255**
- RMSE: **4.27 / 255**
- median absolute channel error: **1 / 255**
- 90th percentile: **6 / 255**
- 99th percentile: **18 / 255**

The remaining delta is render/color sampling, not a structural visual mismatch.

### Zoom-in

**PASS.**

The fixed-orientation orthographic zoom preserves the target art without perspective tearing or geometric reinterpretation.

### Depth decomposition

**PASS FOR PROOF.**

Depth Anything 3 BASE produces a coherent large-scale ordering for:
- bridge/foreground;
- Lower Gate;
- plateau/mid-ground;
- background.

Four deterministic bands are sufficient to demonstrate the visual-shell architecture. Production masks may later be semantically refined; no such refinement is required to validate the core method.

### Dynamic depth behavior

**PASS FOR ENGINE-PROOF ENTRY.**

After switching the layer material to alpha clip/depth write, a probe placed behind nearer depth layers is correctly occluded.

This resolves the prior transparent-layer failure.

### Bounded pan / zoom-out

**NOT A VISUAL-METHOD FAILURE; EDGE INTEGRATION REMAINS UNPROVEN.**

The isolated Blender proof contains only the 700×540 Golden crop. Panning or zooming beyond its finite rectangle reveals the world background.

That does not indicate target degradation inside the shell. In the game the shell will sit over the existing world. The remaining question is whether the shell boundary can be integrated without a visible rectangular seam.

That must be tested in Unity against the real visual world rather than hidden with synthetic padding or inpaint.

## Decision

The camera-first hybrid method has now proved the core properties required to justify an engine test:

1. exact current owner target used;
2. canonical crop preserved visually;
3. fixed orthographic camera contract respected;
4. zoom does not destroy the target;
5. depth ordering can coexist with dynamic depth;
6. no paid generation;
7. no Tripo/Meshy dependency.

Therefore:

**ISOLATED UNITY VISUAL-SHELL PROOF IS AUTHORIZED.**

This is not production promotion.

## Unity proof requirements

The Unity proof must:
- use the exact canonical reset-target crop/layers;
- use the locked orthographic camera family (yaw 20°, pitch 35°);
- preserve the existing collider/hotspot signature exactly;
- add no gameplay colliders or hotspots to the shell;
- capture an integrated full target view so shell boundaries are visible;
- capture Golden close / zoom behavior;
- validate a dynamic probe in front of and behind shell layers;
- report whether the local rectangular boundary is acceptable, needs feather/semantic masking, or invalidates the method;
- spend 0 credits.

No broad production adoption is authorized until this integrated proof is visually reviewed.
