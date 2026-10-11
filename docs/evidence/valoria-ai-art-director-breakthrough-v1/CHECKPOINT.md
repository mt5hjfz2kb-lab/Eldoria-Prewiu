# VALORIA — AI ART DIRECTOR BREAKTHROUGH V1 / CHECKPOINT

## Status
CODE-CANDIDATE — NOT VISUAL PASS. No Unity acceptance, no art library promotion.
Branch: `art/valoria-ai-director-breakthrough-v1-20261011`.
Historical run 38090973491 (SUCCESS), artifact 11684236177 (not expired as inspected on 2026-10-11).
Historical Blender, GLB, camera images: `art-source/valoria/production/gatehouse-reference/`.
The user-specified 6.5/10 is historical subjective guidance, not a new verified score.

## Actual artistic choice
Problem hypothesis: disconnected geometric silhouette and weak structural reading from game camera.
Prioritize substantial silhouette/geometry over surface noise.
Execution in `tools/art-rd/valoria_gatehouse_reference.py`:
- Full-height tapered stone buttresses, distinct actual meshes, closed faces, beveled edges.
- Recessed defensive bridge and strengthened load-bearing outer segmented arch.
- Continuous defensive crown with readable parapet silhouette.
- Existing doors, tower structures and materials retained as baseline.
- Metadata inside GLB manifest records operation and REVIEW_REQUIRED status.

## Strict verification still required
1. Run `.github/workflows/valoria-gatehouse-reference.yml` at this exact branch HEAD on existing free Linux Blender runner.
2. Confirm Blender exit 0, the expected marker, .blend and GLB existence, texture/material and mesh stats.
3. Inspect real historical/new render pairs **at matched view**, owner reference image, and the contemporary library variant. No pass without the actual images.
4. If still toy-like, REWORK/REJECT; do not generate dependent library pieces.
5. Local Qwen + BlenderMCP review must remain advisory, use existing `tools/art-studio-recovery/` policies; arbitrary generated scripts not permitted.
6. Only then independent isolated Unity candidate import; not authorized to interfere with other Unity/Windows work.
7. No mobile performance conclusion until profiling on Android/iOS devices.

## Execution limitation
GitHub connected tools permitted source commits but did not expose an Actions workflow_dispatch mutation. A workflow run was not visible after initial code and workflow commits. GitHub app-originated commits may not trigger push workflows; this is an unverified cause, not an established diagnosis. No installed Blender executable in the current container; direct rendering unavailable. Do not claim art complete.

## Reuse
Reuse geometric operations and art-review log only after side-by-side approval. Scale adapted to tower/wall proportions and shared materials, avoid simply replicating gate mass across library. Keep main protected.
