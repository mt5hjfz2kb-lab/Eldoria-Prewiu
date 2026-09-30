# Eldoria — Visual Convergence Pipeline

Status: active art-direction execution policy.  
Updated: 2026-09-28.

Read with `docs/ELDORIA_VISUAL_BENCHMARK.md`, the Valoria master-plan/camera documents, Art Pass records and `docs/ELDORIA_ART_PIPELINE_INDEX.md`.

## Objective

Reduce the number of expensive art iterations required to reach the intended Valoria quality. The problem is no longer whether we can generate 3D; it is convergence: choosing the right target, testing the cheapest representation that can prove it, only then spending generation/modeling effort, and validating integrated city read rather than isolated beauty.

## Visual convergence ladder

### V0 — Composition proof
Zero/near-zero cost: greybox/proxy under official camera/pan, validating route, massing, skyline, focal hierarchy, occlusion, click targets and progression space. If V0 fails, do not create final geometry.

### V1 — Look-dev proof
Cheap material/lighting study on representative geometry: palette, value hierarchy, stone/wood/roof separation, fog/atmosphere, sun direction, restrained VFX and scale cues. If the scene still reads as washed-out, board-like, toy-like or fortress-only, fix look-dev before adding more buildings.

### V2 — Hero-fragment proof
Create one small high-quality fragment containing the difficult visual language: architecture/rock seam, roof edge, window/door depth, damage/weathering, trim hierarchy and material breakup. Compare it at real gameplay distance. If detail does not survive official zooms, do not propagate it.

### V3 — Production module
Generate/model only after V0–V2 are green: exact parcel/function, camera-aware hidden surfaces, reusable interfaces, topology/material budget and source identity.

### V4 — Integrated district proof
Judge in real Valoria at zoom 19/12/9, home pose and pan extremes, real lighting and surrounding assets, with interaction hotspots. An isolated PASS cannot produce a city-level VISUAL PASS by itself.

## Three independent dimensions

Record separately:
- COMPOSITION — silhouette, hierarchy, circulation, occlusion.
- SURFACE — materials, lighting, texture, depth, weathering.
- IDENTITY — intended Eldoria/Valoria function and style.

Do not collapse these into one score. A beautiful model can fail composition; a technically good material can fail identity.

## Cheapest-fix rule

| Defect | First intervention |
| --- | --- |
| wrong mass/footprint/route | composition/transform/geometry |
| wrong building function read | silhouette/entrance/roof/prop language |
| pale/washed-out | material, exposure, lighting, tonemapping |
| repetitive fortress feeling | kit diversity + residential/civic massing |
| seams/rock islands | overlap/burial/terrain filler |
| detail invisible at zoom | improve macro shapes/value; stop micro-detail |
| too expensive on mobile | LOD/material/shader/texture/culling before abandoning art direction |

Never regenerate an expensive asset to solve a defect whose cause is downstream.

## Reference capture protocol

Maintain a small certified reference set: official home zoom 9/12/19, west/east/future pan extremes, one mobile framing, one material close read and optional hero-fragment diagnostic. For every candidate produce identical camera/exposure captures and an automatic side-by-side/contact sheet.

## Automated visual regression

Automation may reject objective regressions: missing render, camera drift, unexpected occupancy/silhouette change, hotspot occlusion, route visibility loss, excessive frame-time/triangle/material growth, missing materials/textures. Automation must not decide beauty; final art-direction acceptance remains human.

## Look-dev scene

Create/maintain one lightweight `ValoriaLookDev` scene containing representative rock, stone wall, timber, roof, vegetation, ground, hero fragment and official sun/sky/fog candidates under neutral comparison lighting. Solve palette/material/lighting questions there before running full Valoria.

## Lighting direction

For Unity 6 URP, test lighting as a system: stable directional-light profile, global/post-processing volume, baked/mixed strategy where appropriate, and only test Adaptive Probe Volumes after a representative moving/dynamic-object case proves benefit. Use Rendering Debugger for lighting/material diagnosis. Promote newer features only after mobile profiling.

## Asset-generation strategy

Use AI 3D for compact buildings, distinct hero modules, rocks/architectural fragments and bounded modules. Do not ask it to solve whole-city topology, exact street networks, guaranteed clean modular interfaces, or final optimization/material integration without downstream work.

Preserve approved source image and task identity for every generated asset so failures improve the next iteration rather than restarting from memory.

## Tripo API research gate

The official API supports asynchronous image-to-model tasks, model URLs and webhooks. This could remove browser/CDP fragility. Do not migrate production until API credit economics vs Studio, output/model-version parity, required H/P options, privacy/ownership expectations and one explicitly authorized A/B generation are verified.

If equivalent, preferred future chain is `exact input -> API task -> webhook/task result -> model URL -> SHA -> Blender -> Unity`.

## Performance-aware art budgets

Track by integrated camera cell/district, not only per asset: visible triangles, renderer/material count, texture memory, shader complexity/overdraw, transparent FX, shadow casters and draw calls/batches. The fixed camera is an advantage: optimize aggressively what can never influence reachable views.

## Decision rule before another paid generation

Answer:
1. What exact visible problem does this asset solve?
2. Can a zero-credit composition/material test disprove the idea first?
3. Is the defect actually geometry rather than lighting/material/integration?
4. At which official zoom must the improvement be visible?
5. What objective evidence will distinguish success from another iteration?

If those answers are missing, do not generate.


## Production-cell rule — validated 2026-09-30

Valoria production now uses a **small-cell proof before citywide dressing**.

Canonical order for a candidate area:
1. **COMPOSITION / circulation** — footprint, route, elevation, silhouette and camera read.
2. **SURFACE baseline** — shared stone/rock/timber/roof/ground families, value separation, roughness, texture response and local lighting.
3. **Only after SURFACE reads correctly:** props, inhabitants, banners, smoke, vegetation and other density/life cues.
4. Matched BEFORE/AFTER captures at official zooms plus mobile framing.
5. Collider/hotspot signature must remain identical for a visual-only pass.
6. Scale the recipe citywide only when the improvement is clearly visible at gameplay distance.

### 2026-09-30 density-cell experiment

The first lower-civic Production Cell deliberately tested the old temptation: add ground detail, reused props, worker silhouettes, banners, smoke and local warmth without generating new geometry.

Result:
- technical gate: PASS;
- world regression: PASS;
- gameplay topology/collider/hotspot safety: PASS;
- Tripo spend: 0;
- **visual step-change: FAIL / insufficient**.

The AFTER frame is somewhat more occupied, but the difference is small and introduces extra small-scale noise while the dominant quality gap remains the underlying **surface/material/ground/lighting coherence**. Therefore this density cell is retained as diagnostic evidence only and is disabled in production by default.

Permanent conclusion: **do not use set dressing to compensate for unresolved SURFACE.** The next production-quality proof must solve the surface/look-dev layer first on a bounded cell using existing geometry; only then add life/detail.
