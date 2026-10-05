# VALORIA GOLDEN LOOKDEV SLICE v1 — SURFACE AUDIT + RECIPE v1

Status: **ACTIVE SURFACE AUTHORING METHOD**
Date: 2026-10-05

Authority: canonical Golden Slice target sheet + artifact 11365386999 close crop + repository source state. Camera, macro composition, gameplay and closed family positions remain locked.

## Audit scope
Lower Gate edge, local contact rock/boulders, local ground/contact shell, water/shore contact and existing vegetation inside the Golden hero bounds only.

## Current audit

| Surface | Current state before this pass | UV / texel state | Material channels visible | Classification |
|---|---|---|---|---|
| Lower Gate stone | Real authored GLB material exists, but repeated beige response dominates at gameplay camera; detail response is too uniform | UVs present on retained family; scale coherence acceptable for source textures, not enough visible variation | base/albedo + normal where source provides it; scalar smoothness; weak/no spatial AO/roughness/wear hierarchy in integrated frame | **BLOCKING PREMIUM** |
| Local rock/contact | Geometry has UV/normals/tangents, but faceted silhouettes + uniform face response read low-poly | UVs present on authored source03; local correction allowed if missing | base/normal from source; insufficient spatial roughness/crevice response | **BLOCKING PREMIUM** |
| Ground/contact shell | Source03 avoids source02 broad plates, but the remaining local ground/berm response is flat and color-led | UVs present where authored; generated/proxy surfaces may require planar metric UV fallback | flat/scalar response, little mid-frequency soil information | **BLOCKING PREMIUM** |
| Water | Runtime mesh has tiled UV, authored wave normal/albedo and smoothness | 9x tiling on generated grid | albedo + normal + scalar smoothness; insufficient shallow/deep/contact hierarchy | **WEAK** |
| Shore | Mesh strip has UV; current material is dark flat tint + scalar smoothness | UV present | no real AO/roughness/wet spatial variation | **BLOCKING PREMIUM** |
| Vegetation | Authored source03 geometry/materials are accepted structurally, but integrated canopy reads faceted/value-banded | UVs present on retained source | source albedo/normal; insufficient restrained material variation and grounding in Golden close crop | **WEAK** |

## Reproducible surface recipe

The first valid surface method uses deterministic **procedural + authored analytic masks** inside Unity as a zero-license proof of the complete PBR channel stack. It is not a tint-only or MaterialPropertyBlock solution. Each family receives real per-pixel albedo, tangent-space normal, AO and smoothness information, generated from fixed authored pattern rules so the exact result is reproducible from the repository.

### STONE
- warm limestone/sandstone albedo family;
- staggered masonry mask with real mortar/value breakup;
- face bowing + mineral variation for mid-frequency response;
- tangent normal from the authored height field;
- AO concentrated in mortar/contact recesses;
- spatial smoothness variation;
- restrained dirt accumulation; no white uniform edge outline.

### ROCK
- cooler grey-brown/taupe albedo;
- directional strata and intersecting crevice masks;
- tangent normal from layered height;
- crevice AO;
- drier rougher ridges / darker crevices;
- local ground/wet response handled by shore family, not by global recolor.

### GROUND
- muted earth/olive-brown family;
- authored macro patches;
- compacted path streaks;
- pebble-scale mid-frequency relief;
- high roughness with spatial variation;
- AO tied to relief, not a painted border.

### WATER / SHORE
- water keeps its generated grid and wave normal;
- shore gets a dedicated darker wet family;
- silt bands + shallow ripple channels;
- visibly higher smoothness than dry ground/rock;
- wet contact remains localized and irregular.

### VEGETATION
- existing geometry only;
- restrained albedo/value breakup and leaf/branch response;
- darker trunk/canopy separation inherited from source plus local surface response;
- no added density.

## UV / texel policy
- Preserve authored UVs when present.
- If a Golden Slice renderer lacks complete UVs, clone only that render mesh for the isolated capture and apply metric-ish planar UVs; no topology/silhouette change.
- Ground/rock/shore target approximately consistent visible texel scale at the official close crop.
- No stretching accepted in the Golden close crop.

## Material response hierarchy
stone != rock != ground != wet shore != water != vegetation through:
- base color family;
- normal response;
- AO;
- spatial smoothness;
- wetness hierarchy;
- specular hierarchy.

## Performance budget — iteration01
- 5 reusable surface families.
- 4 maps/family: albedo, normal, AO, metallic/smoothness.
- 256x256 RGBA32 with mipmaps for the proof.
- Estimated uncompressed texture footprint with mip overhead: ~6.7 MB.
- No new transparent material family.
- Existing instancing retained.
- No geometry expansion in this surface iteration.

## Anti-loop
This is **surface method iteration01**. A failure may be followed by at most two technically valid iterations of the same method. After three, change method. Do not add new geometry to hide a surface failure.

## Gate
This method is not accepted unless material richness, surface readability, ground richness, rock richness and premium perception each reach >=4/5, with mobile/gameplay/performance PASS.
