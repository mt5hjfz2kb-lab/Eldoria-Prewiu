# Eldoria — Surface Production Pipeline

Status: active zero-credit baseline.  
Updated: 2026-09-29.

## Purpose

Turn accepted geometry into coherent Eldoria production surfaces without regenerating the asset unless the defect is geometric.

## Canonical stages

### S0 — Geometry accepted
TECH geometry gate is green. Record raw/optimized SHA, bounds, triangles and materials.

### S1 — Surface diagnostics
Blender report records image dimensions/colorspaces, material count, Principled values and image-texture routing. In Unity inspect the candidate using Rendering Debugger material overrides.

Classify the defect before touching the asset:
- ALBEDO/VALUE
- NORMAL/FORM
- ROUGHNESS/SMOOTHNESS
- METALLIC
- AO/DEPTH
- UV/TEXEL DENSITY
- LIGHTING/EXPOSURE
- STYLE/IDENTITY.

### S2 — Baseline material calibration
Use the LookDev comparison set to solve stone/timber/roof/rock/ground against identical camera and lighting profiles. Do not tune every building independently.

### S3 — Bake/enrich only when useful
Optional Blender baking can create AO, tangent normal and other supporting maps after geometry/UV acceptance. Never bake to hide fundamentally broken geometry.

### S4 — Optional paint lane
Hero assets may enter a dedicated painting tool only when S0–S3 cannot efficiently reach the visual target. Current evaluation order: ArmorPaint low-cost pilot, then Substance Painter if its smart-material/baking workflow demonstrates enough additional value.

### S5 — Unity packing/import
Normalize maps to the URP Lit convention used by Eldoria. Preserve source maps and generate runtime-packed textures separately so authoring data is not destroyed.

### S6 — Integrated camera gate
Capture official zoom 19/12/9 plus relevant pan/mobile view. Compare COMPOSITION, SURFACE and IDENTITY independently.

## Texture budget baseline

Until mobile profiling justifies a different value:
- hero/primary architecture: authoring up to 2K per major material set;
- secondary/support assets: target 1K where quality survives official zooms;
- do not retain 4K/8K simply because the source generator produced it;
- Unity platform import/compression remains the final runtime decision.

These are starting policies, not a hard device budget. Device profiling can tighten or relax them.

## Material-family principle

Valoria should converge on a small reusable family rather than one unrelated material set per AI asset:
- Eldoria Stone
- Eldoria Rock
- Eldoria Timber
- Eldoria Slate/Roof
- Eldoria Ground/Mud
- metal/accent where needed.

Per-building uniqueness should come primarily from masks, variation, wear, decals/props and architectural form, not an entirely unrelated shading response.

## Aserradero priority

The current Aserradero geometry is accepted. Its next intervention is S1–S2, not regeneration. Diagnose imported maps/material values and lighting, establish the first reusable surface recipe, then apply that recipe to Cuartel before generating more major architecture.

## Automation target

Future `surface` configuration in each asset manifest should carry:
- material classes;
- source texture identities;
- bake configuration;
- authoring tool/version;
- packed runtime map identities;
- Unity import settings;
- LookDev profile used;
- SURFACE verdict and evidence.

No paid tool or paid generation is implicit in this pipeline.