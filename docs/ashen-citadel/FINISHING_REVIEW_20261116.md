# Ashen finishing candidate — 2026-11-16

User request: refine finish of the reviewed Ashen Citadel scene. Same independent branch, no gameplay or main art integration.

## Observed baseline
Actual native Unity artifact 14207886409, run 43828204694, source 019a24510e62dff53679d034874f51fd890199f1. Inspected Bastion V 1600x900 and Bastion I 390x844. Fortress is legible but dark, terrain visually empty, conifers evenly dispersed, road has a hard ribbon silhouette. Mobile city occupies limited screen area; preserve current camera for an honest first comparison.

## Prepared candidate
- Raise warm key from 1.15 to 1.60, less orange tint, increase cool ambient; reduce shadow strength .72 to .64. Keep light direction/camera/architecture.
- Ground: olive/sage macro variation, soft dirt/mineral wear, two worn road tracks. Apply the same spatial ground texture to road ribbon and terrain to remove the abrupt material edge.
- Seven asymmetrical groves, 78 candidates instead of 86; varied scale .52–1.18 and height. Clear building/approach exclusion zones. Existing pine mesh and texture reused.
- Grade: exposure .22, contrast 5, saturation 2; bloom unchanged.
- Ground texture 1024 square instead of 512; estimated RGBA32 mip memory increases from ~1.33 MiB to ~5.33 MiB. Profile on device before adoption. Author-time generation shared between the two materials.

## Verified result
Owner explicitly authorized the main coordination update. Native Unity run 43832430172 passed on source 41f3e984dd0cb41c9004a455321b56af21751925, artifact 14208894317. All ten native capture hashes verified; 7/7 EditMode and 4/4 PlayMode, zero failures or skipped tests. Source syntax CI also passed.

Actual V landscape and I portrait captures inspected against the prior run: masonry and slate read more clearly; road no longer reads as a hard separate ribbon; forest masses have clearings and broader scale variety. Retain as a useful finish increment, not ART_APPROVED. Ground still smooth in large areas; rocks remain generic, auxiliary plinths read as hard outlines, UI remains rough and mobile architecture still small. These need separate deliberate refinement, not another global brightness increase. No physical iPhone/WebGL/performance certification.

Camera and architecture unchanged; no gameplay changes or production publication. Windows released after successful terminal native job. Evidence remains linked to the exact source SHA, not the later documentation commit.
