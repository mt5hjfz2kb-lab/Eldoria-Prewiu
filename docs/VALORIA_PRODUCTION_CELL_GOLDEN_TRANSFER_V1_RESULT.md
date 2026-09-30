# Valoria Production Cell — Golden Transfer v1 — Final Result

Date: 2026-10-01
Base main HEAD: `9da483e4ea1b9780b1afc22ad610c6c0289ed0f9`
Experimental branch: `visual-proof/valoria-production-cell-golden-transfer-v1`

## Scope
Real production Valoria cell only:
- Bastion visual shell / main access;
- processional stair and immediate plaza;
- upper civic terrace;
- rock ↔ retaining architecture transitions;
- small residential / productive support frontage.

Gameplay topology, authoritative stairs, colliders and hotspots remain owned by the existing production scene.

## Canonical evidence
Final successful transfer run: **36786910345 — SUCCESS**
Artifact: **11129683214**
Validated source HEAD: **ad32df3609f0162c2384c9494b0650dd02d2d74a**

The run produces matched same-scene BEFORE / AFTER captures at:
- orthographic 19;
- orthographic 12;
- orthographic 9;
- mobile 390×844.

The Golden Cell reference used for direct fidelity comparison is:
- visual checkpoint: `3b5764ea786af415fb918e98da83dd7f893d715e`;
- run: **36783595411**;
- artifact: **11129220413**.

Direct Golden AFTER ↔ production AFTER review shows the transferred cell is effectively the same visual result at all four official framings. The experiment therefore proves that the Golden Cell recipe can technically be recreated on the real production Valoria scene without changing gameplay authority.

## Gameplay / topology gate
- same-scene before / after: **true**
- collider + hotspot signature equal: **true**
- gameplay topology changed: **false**
- Tripo credits: **0**
- paid assets: **0**

## Render metrics

| Metric | BEFORE | AFTER | Delta |
| --- | ---: | ---: | ---: |
| triangles | 1,574,646 | 1,594,962 | +20,316 |
| active renderers | 1,199 | 1,588 | +389 |
| unique materials | 629 | 627 | -2 |
| lights | 22 | 29 | +7 |

These are editor scene counts, not physical Android profiling. No device-performance conclusion is inferred.

The renderer increase is structurally undesirable for a final production implementation and comes mainly from primitive/bevel helper construction. This must not be propagated citywide.

## Unity systems actually used
- URP/Lit material base;
- shared PBR material families for stone, cobble/ground, wood, roof, metal, plaster and moss-rock;
- imported CC0 texture maps with mipmaps / trilinear filtering and normal-map import;
- existing real production camera and scene;
- visual-only renderer replacement while preserving gameplay objects;
- directional lighting adjustment;
- restrained warm point lights / emissive-like focal treatment;
- linear fog / atmospheric separation;
- shared materials rather than one material per generated piece.

Not adopted in this proof because they were not needed to answer the transfer question:
- new Unity Terrain ownership;
- Shader Graph;
- decals;
- baked lightmaps / Light Probes / Reflection Probes;
- LODGroup;
- GPU-instancing-specific implementation;
- MaterialPropertyBlock variation;
- occlusion culling.

Those remain candidates only where a measured production problem justifies them.

## Visual verdict

**VISUAL FAIL — do not promote to main.**

Two separate conclusions must not be confused:

1. **TRANSFER FIDELITY: PASS.** The real production cell reproduces the best Golden Cell visual checkpoint essentially 1:1 while preserving colliders/hotspots.
2. **PRODUCTION VISUAL ACCEPTANCE: FAIL.** Under the current owner gate, the result still reads as “Eldoria improved” rather than the requested category change. The hero architecture is stronger than main, but the surrounding generation, coarse architectural language and prototype-like support construction still expose the same ceiling.

The canonical Golden Cell documentation also records that checkpoint as diagnostic / below the final category-jump threshold. Therefore this branch must not silently reinterpret it as a production visual certification.

## What actually improved quality
The visible gain comes from the combination, not from density:
- replacing the old Bastion visual representation instead of protecting it;
- one coherent PBR surface family;
- lighter, more readable masonry;
- a stronger gateway / fortress hierarchy;
- continuous stair → terrace → Bastion composition;
- rock / wall transitions;
- restrained blue / gold identity and warm focal light.

The gain did **not** come from adding more props, more lights or Tripo geometry.

## Pipeline conclusions

Keep:
- gameplay/topology and art as independent layers;
- blockout / frame proof before final fabrication;
- matched 19 / 12 / 9 / mobile review;
- same-scene collider/hotspot signature gate;
- coherent shared PBR material families;
- permission to replace legacy hero visuals;
- bounded production-cell proof before citywide propagation.

Invalidated for final production:
- primitive/bevel-heavy helper geometry as final architecture;
- treating the Golden Cell checkpoint itself as sufficient proof of a new visual category;
- citywide propagation before the hero architecture reaches the benchmark;
- solving the remaining gap through density or extra lights;
- preserving the old Bastion shell because of sunk cost.

## Next concrete step
Do **not** rebuild the rest of Valoria.

The next proof should keep the same real production cell and replace only the remaining weak architectural layer with a genuinely higher-grade fortress-specific hero/access family, while reusing the now-proven PBR / camera / gameplay-safe transfer system.

Before any paid/credit generation, test zero-cost local or already-owned authored fortress candidates. Tripo remains a last escalation requiring explicit owner authorization.

## Main disposition
Nothing from this experimental branch is promoted to `main`.
Production `main` remains unchanged by this proof.
