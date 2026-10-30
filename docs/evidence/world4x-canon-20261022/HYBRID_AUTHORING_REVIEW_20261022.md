# Hybrid authoring continuation — 2026-10-22

## Retained runtime correction
PR100 retains the regional forest visibility correction: native 39826768500, 7/7 PlayMode tests, 12 actual Unity captures. Near and panorama matched-camera captures unchanged; regional visibility restored. Details: REGIONAL_LOD_CONTINUATION_20261022.md.

## Actual author pipeline
Source commit 1132e559 exports 6,561 measured Unity vertices and 12,800 triangles, verified by native 39827508494. Blender freezes this source without artistic deformation, preserves UV and vertex indices, converts Unity Y-up to Blender Z-up, and scales ten Unity metres per Blender unit. The existing memory selector and typed author are reused. Installed Ollama and Blender Python are recovered without downloads, installation, cloud inference or spending.

Artist retries recover this exact snapshot only after checking Unity source identity against 1132e559. They do not rerun unchanged Unity or claim a fresh Unity test pass.

## Real outcomes
- Run 39828569968, artifact 12446819628: actual Qwen3-VL 4B memory consumption and Blender execution. Rejected: lateral displacement at x=-90 outside camera, zero significant pixel difference. No runtime integration.
- Run 39829192368, artifact 12448555620: actual Qwen3-VL 8B generated numeric operation IDs on both attempts. Existing typed validator rejected them. No Blender execution.
- Run 39829726922, artifact 12448800512: string IDs corrected and exact memory consumption verified. Rejected before Blender because ridge_center at [-1,0,0], radius12 overlapped protected central gameplay circle.
- Run 39830118330, artifact 12448222697, source 5916307: actual Qwen3-VL 8B produced three bounded Z-up peripheral edits; actual Blender executed them (43,43,46 vertices changed). Source and result mesh hashes differ; original remains preserved. Matched clay render review shows only small edge hills, not a convincing correction of the panorama composition. **Do not promote this variant.**

Final plan hash: 8203eabe7ca2bfa0ba16534c7420065b384b34c6fd424fd33fd0fae471f1bdfe.
Final output GLB hash: cc47d3e414aab8362e0e4673793da1e07eea8aeab06b5b754bdc50afb52ce29b.
Final memory inventory hash: c81e057104b6264414bdd445f68befe4ffd493021b722f6dc6c01c9c9505a791.
Matched final clay comparison: 3.126844618% of pixels changed by >5 RGB levels; max channel change16; mean per-pixel max change0.459619. This is a visibility measurement, not a quality score. Original and final renders inspected.

## Next artistic decision
Do not repeat generic peripheral-bump prompts or weaken central protection. Actual existing ridge anchors in Unity are (-330,190), (326,215), (-320,-220), (267,-170); Blender equivalents divide by10. Supply actual geographic anchors and water exclusions before another authoring cycle. Current snapshot lacks painted land/water context in Blender, so a neutral clay pass cannot establish reference fidelity. A future candidate needs reference-linked landform composition and genuine full-scene Unity comparison.

The reference still differs materially in grove density, coherent branching water, geological breach and coastal silhouette. P0 art remains open. No new artist geometry enters runtime, no main merge/publication, no art approval, no physical mobile/iPhone pass, and no WebGL release inference.
