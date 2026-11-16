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

## Verification and blocker
Local diff inspected. No runtime/gameplay/test files changed. This candidate has NOT been rendered in Unity and must not be described as a proven visual improvement.

Automatic approval review rejected the attempted main coordination update (same canonical workstream handoff) because current user authorization covers isolated-branch continuation, not a main write. No workaround attempted; no native job dispatched. Registry update requires explicit owner approval before the shared Windows run. Proposed scope: only pipeline/active-workstreams.json, changing the Ashen entry's owner/revision/continuation state, preserving all other entries. Then synchronize the existing native workflow's owner checks to that new claimed identity, without relaxing guards.

Next: authorized coordination update, publish isolated candidate, run existing source/native QA, inspect same-camera I/V PNGs and all gameplay test results, retain only visibly better outcome. Release Windows after terminal job. No physical iPhone, WebGL or ART_APPROVED claim.
