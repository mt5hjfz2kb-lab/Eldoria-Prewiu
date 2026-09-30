# Valoria Production Cell — Golden Transfer v1 — Final Result

Date: 2026-10-01  
Base main HEAD at branch creation: `9da483e4ea1b9780b1afc22ad610c6c0289ed0f9`  
Experimental branch: `visual-proof/valoria-production-cell-golden-transfer-v1`

## Final verdict

**VISUAL FAIL / NO MERGE / NO PRODUCTION**

The transfer mechanics are technically valid, but the production-zone result does **not** retain the category jump required by the benchmark.

## Exact production cell tested

- Bastion;
- processional access / stairs;
- lower plaza;
- immediate civic terrace;
- rock ↔ architecture seams.

The gameplay layer remained authoritative. Visual renderers were replaced/suppressed only inside the selected cell; colliders, hotspots, routes and navigation ownership were not transferred to the art layer.

## Canonical final evidence

Final successful Unity validation:

- proof HEAD: `9a66ceff4ac0bed3dca86e7335763fee5ef1576a`
- run: **36787683142 — SUCCESS**
- artifact: **11130876302**
- same-scene BEFORE / AFTER: **true**
- collider + hotspot signature equal: **true**
- Tripo credits: **0**
- paid assets: **0**
- official captures: **zoom 19 / 12 / 9 / mobile 390×844**

## Render metrics — final attempt

| Metric | BEFORE | AFTER | Delta |
| --- | ---: | ---: | ---: |
| triangles | 1,574,646 | 1,171,138 | **-403,508 (-25.6%)** |
| active renderers | 1,199 | 1,038 | **-161 (-13.4%)** |
| unique material instances | 629 | 576 | **-53 (-8.4%)** |
| lights | 22 | 25 | +3 |

The final attempt therefore solved the structural cost problem that existed in the first transfer attempt. The fail is visual, not technical/performance-by-count.

## Attempt history

### Attempt 1 — literal Golden transfer

Run **36786910345**, artifact **11129683214**.

Technical safety passed, but the frame still read as **“Eldoria improved”** and the implementation added too many small renderers:

- triangles: 1,574,646 → 1,594,962;
- renderers: 1,199 → 1,588;
- materials: 629 → 627;
- lights: 22 → 29.

It was rejected.

### Attempt 2 — complete visual-cell replacement

Run **36787683142**, artifact **11130876302**.

Changes:

- legacy renderers in the selected cell are suppressed while gameplay remains;
- one coherent PBR family drives stone / ground / wood / roof / metal / plaster / moss;
- Bastion/access/plaza/terrace are rebuilt as one visual composition;
- static generated meshes are combined by shared material;
- instancing is enabled on shared materials;
- local light count is reduced;
- atmospheric separation remains restrained.

This version substantially improves the render structure, but the hero architecture still reads as procedural/blockout geometry. At zoom 12 and mobile, the silhouette, roof construction, openings and secondary forms do not reach premium authored-architecture quality. PBR surfaces cannot compensate for insufficient hero source geometry.

## Unity systems evaluated / used

### Useful and retained as principles

- **URP/Lit shared PBR material family** — meaningful surface/cohesion gain.
- **Normal + AO + diffuse maps** — meaningful upgrade from synthetic flat materials.
- **Shared materials + GPU instancing** — correct production structure.
- **Static mesh combination by material** — reduced renderer count materially.
- **Fog / atmospheric perspective** — useful for depth, secondary to architecture.
- **Restrained warm local light** — hierarchy support only.
- **Visual-layer replacement independent of gameplay layer** — technically validated and essential.

### Not used because they do not solve the dominant blocker yet

- Shader Graph;
- decals;
- baked lightmaps;
- Light Probes;
- Reflection Probes;
- LODGroup;
- Terrain replacement for the city cell.

Those remain valid tools, but adding them now would polish geometry that still fails the hero-architecture bar.

## What actually changed quality

The positive changes were:

1. coherent PBR surfaces instead of flat/synthetic materials;
2. a clearer Bastion → stair → plaza/terrace hierarchy;
3. continuous rock ↔ retaining architecture;
4. removal of mixed legacy visual shells inside the converted cell;
5. less dependence on point lights;
6. a renderer-efficient visual replacement strategy.

The remaining dominant blocker is **hero architecture source quality**.

## Pipeline conclusions / invalidations

Rejected for hero production architecture:

- procedural cubes/beveled primitives as the final Bastion shell;
- expecting a PBR pass to create a category jump on insufficient geometry;
- adding density/props before the hero architecture itself is premium;
- layering new art over legacy visual shells inside a converted cell;
- treating a green technical gate as a visual PASS.

Still valid:

- metric/procedural construction for circulation, stairs, terraces and connectivity;
- shared material families;
- visual/gameplay layer separation;
- BEFORE/AFTER at 19/12/9/mobile;
- collider/hotspot signature gate;
- measured renderer/material/triangle gates.

## Main disposition

**Nothing from this branch is approved for main.**

The branch is an experimental record only. The Golden Cell branch is also not merged wholesale.

## Next concrete step

Do **not** extend the recipe across Valoria.

Keep the validated production-cell replacement mechanics, PBR family, mesh compaction and gameplay-safety gate. Replace only the **hero Bastion/access source geometry** with genuinely authored geometry that survives zoom 12 and mobile.

Order:

`hero source geometry → real production cell → 19/12/9/mobile → collider/hotspot signature → render metrics → visual verdict`

Use a free/local authored source first. Tripo remains limited to hero architecture and requires explicit authorization before any credit spend.

Only after this exact cell reaches **VISUAL PASS** should the system be industrialized for the rest of Valoria.
