# VALORIA GOLDEN LOOKDEV SLICE v1 — TEXTURE AUTHORING TOOL DECISION

Status: **DECISION LOCKED FOR GOLDEN PROOF**

## Evaluated routes

### Substance 3D Painter / Designer
Strengths: excellent authored masks, smart materials, channel packing and iterative paint workflow.  
Decision: **DEFER for Golden proof**. It would add a new paid/license dependency and runner reproducibility burden before we have proved that the present geometry + a full PBR channel stack can hit the target. Adoption remains possible later if manual authored masking becomes the bottleneck.

### Blender texture painting
Strengths: direct local paint correction, no new paid dependency.  
Decision: **SUPPORTED FALLBACK**, useful for selective hand masks but inefficient as the sole repeatable family pipeline.

### Blender procedural bake + authored masks
Strengths: deterministic, versionable, runner reproducible, zero new license cost, already in the canonical DCC path, suitable for albedo/normal/roughness/AO/contact mask generation.  
Decision: **SELECTED FOR GOLDEN PROOF**.

## Problem solved
The previous authoring path generated mostly albedo + normal with scalar roughness, which leaves stone/rock/ground too uniform under Unity lighting. The Golden proof needs channel-level response: macro color breakup, mid-frequency surface height/normal, spatial roughness variation and explicit contact/wear masks.

## Required outputs
Per hero surface family where useful:
- base color / albedo
- tangent-space normal
- perceptual roughness map
- AO/contact mask
- optional wear/wetness mask
- deterministic metric UV / texel density

## Output format
- PNG source maps in the authored source directory.
- GLB embeds/references the authored base/normal/roughness response for isolated and Unity review.
- Masks remain separately versioned where Unity-specific blending requires them.

## Mobile budget for Golden Slice
- 512×512 per reusable stone/rock/ground base set for proof.
- 256×256 masks where the signal is broad/contact-driven.
- Avoid 2K/4K proof textures.
- Target <= 3 material families added to the close slice and <= 8 MB uncompressed source texture footprint before engine compression.

## Reproducibility
All maps must be deterministically generated from repo source + fixed parameters/seeds. Manual masks, if added, are checked into the repo as canonical source bytes.

## Fallback
If deterministic Blender maps cannot reach 4/5 material richness after an integrated test, the next method is a tightly scoped Painter/Designer proof on the same slice—not another procedural micro-tuning loop.

## Purchased content
No asset packs or third-party visual identity are authorized.
