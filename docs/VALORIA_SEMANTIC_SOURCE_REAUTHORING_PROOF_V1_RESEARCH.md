# VALORIA SEMANTIC SOURCE REAUTHORING PROOF v1 — research and plan

Date: 2026-10-04. Experimental only. Accepted visual baseline remains VQB `7a564bc5056bb7166b7704c313ae4db7d55d4530`.

## Verified source diagnosis

Existing VQB Blender evidence is reused rather than repeated:
- Hero Bastion: one 49,800-triangle mesh, one material, 2,792 loose connected components. Prior deterministic segmentation proved a low 14% height cut can remove 840 disconnected low components / 5,125 triangles (10.29%) while retaining 44,675 source triangles.
- Aserradero: one 49,800-triangle mesh, one meaningful material, 5,100 connected components. Embedded textures: basecolor 2048², RM 1024², normal 1024².
- Because the rich generated assets are single-material meshes, semantic reauthoring cannot rely on pre-existing material slots. It must classify geometry/components and assign additional face material indices while preserving UVs and source texture nodes.

## External research -> concrete decision

Blender 5.2 documentation:
- Mesh Separate supports selection, material and **loose parts**, validating disconnected-component editing as a first-class mesh operation: https://docs.blender.org/manual/en/5.2/modeling/meshes/editing/mesh/separate.html
- Multiple material slots can be assigned to selected faces, so source UV geometry can keep its mapping while semantic face groups receive related materials: https://docs.blender.org/manual/en/5.2/render/materials/assignment.html
- UV maps are mesh data and remain the texture-coordinate source unless explicitly replaced: https://docs.blender.org/manual/en/5.2/modeling/meshes/uv/uv_texture_spaces.html
- Baking is useful for game-engine base/normal/AO output and requires UVs; selected-to-active is available if later geometry edits require rebaking. It is **deferred in phase 1** because the current source maps can be preserved directly: https://docs.blender.org/manual/en/5.2/render/cycles/baking.html

Applied technique:
1. Hero: delete only the already-proven low14 disconnected component band from the actual source mesh; retain source silhouette above it; author the rock-to-masonry transition inside the same Blender source collection rather than placing a Unity wrapper in front.
2. Aserradero: retain all source geometry, UVs and texture nodes; classify disconnected components by height/form and assign stone/timber/roof variants that multiply the original basecolor rather than replacing it.
3. Wall: start from rich `HighStraightWall.glb`, preserve geometry/UVs/source maps, and assign foundation/stone/cap zones using the same material hierarchy.

Deferred:
- global retopology/decimation;
- texture repaint from scratch;
- bake/rebake unless source UV preservation fails;
- paid add-ons/Tripo;
- any other Valoria building.

## Stop rule

Capture **zoom 9 + mobile first** with real HUD and identical accepted VQB state. Mobile includes the normal home crop and a matched bounded-pan Aserradero view because the canonical portrait home does not expose all of F1. If the candidate is not a clear first-glance improvement, do not run 12/19 and do not proliferate variants.
