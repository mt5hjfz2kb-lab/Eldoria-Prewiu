# Valoria Surface v1

Status: ACTIVE VALIDATION
Updated: 2026-09-29

## Evidence source

Aserradero diagnostic run: 36493273686

Certified production source:
- asset: Valoria_Aserradero_AP2_v1
- SHA-256: 1932e36c7fdf972c2ffb209ed7b3679fd7f0243d0f8e713e0f4643146fa9ad0e
- bytes: 11134796
- triangles: 49800
- vertices: 55752
- materials: 1
- UV0: present
- normals: present

Textures:
- basecolor: 2048x2048, sRGB
- normal: 1024x1024, Non-Color
- combined RM mask: 1024x1024, Non-Color

## Primary finding

The source GLB already carries a valid PBR surface structure.

The previous Unity integration path for Aserradero rebuilt a simplified material through `BenchmarkPieceIntegrated`, preserving basecolor but discarding the source normal/RM response and imposing a flat tint/smoothness profile.

This is classified as a SURFACE integration defect, not a geometry defect.

## Surface v1 rule

For dedicated production GLBs that already provide valid PBR maps:
- preserve their imported material and map routing;
- do not replace them with a basecolor-only integration material;
- do not regenerate geometry to fix a surface-only failure;
- use LookDev/environment calibration after preserving the source PBR first.

Aserradero now follows the same preservation principle as Cuartel via `BenchmarkPiece`.

## Current baseline

Aserradero:
- geometry: accepted
- source PBR: valid
- integration path: corrected to preserve authored PBR
- next gate: integrated official-camera comparison

Cuartel:
- production PBR preservation: active
- full Unity gate: success
- next role: second-building reuse proof for the formula

## Promotion criterion

Surface v1 becomes reusable baseline when:
1. Aserradero preserves its PBR and improves integrated readability;
2. Cuartel remains coherent under the same LookDev/material-family rules;
3. neither requires unrelated per-building lighting or shading logic.

If those conditions hold, this document becomes the first reusable surface layer of Valoria Visual Formula v1.
