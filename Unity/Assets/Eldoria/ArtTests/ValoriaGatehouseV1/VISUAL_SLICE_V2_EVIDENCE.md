# Valoria Bastion I — Visual Quality Breakthrough V2 evidence
Date: 2026-10-11. Isolated from playable main.
## Scope and source
- Unity 6000.3.23f1; Blender-authored environment source `tools/art-rd/valoria_visual_slice_environment_v2.py`.
- Main asset: `valoria_visual_slice_environment_v2.glb`; exact original gate and coherent wall GLBs reused.
- Natural geological outcrop, fractured cliff ring, ascending paved access and descending causeway are geometry in Unity, not painted into the backdrop.
- Existing Holotna Mountain Tree01A/B prefabs reused, with authored LODGroups. Licensing/provenance must be verified for commercial distribution.
- Existing Eldoria valley-backdrop.png used as a 2D atmosphere plane. This is not continuous world geometry.
- Selected existing PBR rock family retained; brown ground/foliage variants and visually incompatible historical GLB rejected.
## Reproducible verification
- Baseline: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38094510147
- Latest V2 full renderer and fresh-process scene reload: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38126449936
- Saved scene: `Assets/Eldoria/ArtTests/ValoriaGatehouseV1/ValoriaVisualSliceV1.unity` (under Unity/)
- Mandatory fresh Unity editor reload: 306 non-null mesh filters; 0 missing meshes; 306 enabled renderers; 175,415 triangle indices/3 counted across meshes.
- Environmental hierarchy: 127,962 triangle indices/3, 276 renderers, 456 material slots (including LOD levels and duplicates); values are not in-game draw calls or mobile benchmark.
- Review shots: `unity-valoria-slice-landscape-1280x720.png`, `unity-valoria-slice-portrait-390x844.png`, `unity-valoria-slice-closeup-1280x720.png`, `unity-valoria-slice-overview-1280x720.png`, `unity-reloaded-scene-1280x720.png`.
- No Android/iOS device profiling; no measured runtime FPS, GPU timing or draw-call budget.
## Artistic approval
NOT APPROVED as target-reference-quality / commercial indie final.
Substantial advances over V1: natural irregular geometry replacing smooth plateau, existing tree mesh LODs replacing cone placeholders, world backdrop and two-tier strategic framing, path and bridge geometrically connected.
Remaining gaps: image-reference stone material richness, distinct masonry detail, realistic terrain/grass blends, warm visible torches/contact shadows, environmental integration with distant valley, and on-device visual performance.
Keep the V2 branch isolated; do not publish to main without approval.
