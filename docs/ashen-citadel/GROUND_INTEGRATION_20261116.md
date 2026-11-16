# Inhabited ground and courtyard integration

Owner continues premium refinement. Same isolated Ashen workstream and branch. Baseline source3b9c246eb5f0a611873dfa89aa9ea3997aaa7238 / native43863284139.

## Decision and reuse
Last Unity images show a grass-filled courtyard, sparse disconnected approach stones, weak craft-yard soil transitions, and small footing gaps. Lighting/material improvements cannot fix these composition/contact issues. Reuse current original AshenLandscape and existing stone palette; do not change architecture or camera.
Ran find-reuse.py against curated, current, historical indexes and read gallery entry: RockTerrainSeamFiller and RockTerrainFamilyV1 are conditional seam/rock families, not a fitted courtyard for this settlement. Adapt the current owned landscape, avoiding imports from unrelated scenes.

## Authored changes
Broad 19cm soil crown across settlement, feathered over14m outside core; no individual plot platform or rim. Match vegetation placement height in Unity. Individually chipped near-flush paving stones in courtyard, eroded approach, small craft thresholds. Path/yard earth masks strengthened, irregular edges kept. Random seed restored after paving so unrelated boulders retain exact positions.

## Gates
Deterministic OBJ regeneration and upward paving winding; source/art validation; Blender geometry review; actual same-camera Unity I/V and portrait, gameplay tests. Soil texture is Unity-authored and requires native visual review. No final art approval or mobile-performance claim.

## First native review and correction
Run43865170576/sourceee98973 completed success; actual V landscape/portrait inspected. Soil contact improved, but the approach looked like disconnected checkered tiles. Reject that path treatment as final: reduce colour alternation, narrow joints, stagger courses and concentrate loss on verge/distant end. Preserve all other layout/material/camera changes for a focused corrective A/B.

## Continued review — corrected wear masks (16 November 2026)

The staggered, low-contrast paving source is committed and the last native capture of that correction remains **pending independent visual verification**. A separate code inspection identified a definite Unity math error: `Mathf.SmoothStep(from, to, t)` was being given the desired distance thresholds as *output values*, rather than normalizing the distance into `t`. That could produce incorrect or negative wear weights and uneven transitions.

Implemented in isolated source commit `8fae11c43a671e20a8e8650ab5c1c29f935315db`:
- `InverseLerp(near, far, distance)` first, then `SmoothStep(0,1,t)` and invert, for paths and courtyard masks.
- Bounded courtyard soil opacity at 72% to avoid replacing the entire ground with a flat brown surface.
- Reused the four activity-centre coordinates outside the one-million-pixel texture-generation loop to reduce temporary allocations.

Regression source check `tools/ashen-citadel/verify-ground-falloff.mjs` is wired into isolated source QA. The independently checked boundary/monotonicity model passed 1,000 sampled transitions across five procedural detail values; this verifies the *mathematical intent*, not the Unity render. The actual Unity compile, same-camera screenshots, 7 EditMode + 4 PlayMode tests and iPhone performance are still separate gates. Do not mark ART_APPROVED or claim final visual quality until those are inspected.

Next verification order: obtain terminal result/artifacts for the pending paving proof if available, reserve the shared Windows runner through the canonical main registry when free, capture the new source SHA with identical I–V cameras, then inspect path softness, stone regularity, building-to-ground transitions and mobile framing. Do not bypass the main runner lease or disturb unrelated jobs.
