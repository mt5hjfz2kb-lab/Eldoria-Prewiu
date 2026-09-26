# Bastion_Optimized_v1 — source receipt

Date: 2026-09-26

- Derived from owner-supplied `fantasy castle 3d model(1).glb`.
- Optimized candidate: `Bastion_Optimized_v1.glb`.
- Verified geometry: 1 mesh, 39,251 vertices, 81,506 triangles.
- File size: 12,794,888 bytes (~12.2 MiB).
- Original inspection: ~501k triangles, one mesh, three 4K textures.
- Optimization target for this pass: ~80k triangles and 2K textures.
- Purpose: isolated Unity art/technical validation only; not production-final.
- Do not replace or edit `Valoria.unity`, `VisualWorld`, or gameplay.

Required Unity validation:
1. Import GLB in isolated ImageTo3D test scene.
2. Verify materials, UVs, normals, scale and orientation.
3. Capture orthographic zooms 19 / 12 / 9 at 1280x720.
4. Capture one oblique/orbit view to prove real volume.
5. Test collider/raycast only in the isolated scene.
6. Compare visual loss versus source/reference and record runtime cost.
