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

## Native evidence recovered and actually inspected (16 November 2026)

Recovered exact Unity native run **43866203492**, artifact **14225881950**, source commit `4a3dcc8d893eb021296ce61f7f113d39d6501313`. Artifact SHA256 `57de0435322e107e28336947e21f6e62aaa797ef4ba98e0a78050d28b3c90d46`. The ten I–V landscape/portrait PNG hashes match `native-proof.json`; EditMode **7/7** and PlayMode **4/4** pass.

**Observed on genuine 1600x900 Unity Bastion V frame:** fortress and all four secondary structures are distinct, support contact is acceptable, courtyard/approach has more actual flagstones. However the approach still forms parallel small rectangular stone tracks, recognizably repetitive. The broader grass remains flat, with disconnected rock/foliage clusters and harsh long shadows. The footprint of the courtyard is less visually rich than the buildings; this is **not** a premium-art acceptance.

**Observed on 390x844 Unity frame:** all four secondary structures appear in view, but city and labels/details appear small; no performance or physical iPhone claim is supported.

This native evidence predates the separately implemented and source-checked *normalized wear mask* correction. The masks should improve the dirt/stone transition mathematically, but **the corrected C# source has not yet been rendered or visually approved in Unity**. Shared Windows runner remained held by a different workstream when inspected, so no conflicting native job was dispatched. Next isolated test should compare the same 1600x900 frame and 390x844 frame with identical lighting and camera, explicitly reviewing whether the approach still resembles laid tiles.

## Second native pass — normalized wear masks (verified)

Native Unity run `43870533193` finished **SUCCESS**, exact head `310cee1f41999a0aec5eee4b88304a55f45a6d5d`, artifact `14228156458`, ZIP SHA256 `76de9b5cb444a74182a25639702130f05a3a67ee6cf33d39fcc0b3da5eec021c`. All ten Bastion I–V landscape/mobile screenshot SHA256 values verified; **EditMode 7/7 and PlayMode 4/4** passed; source compilation and full Unity rendering succeeded.

The new capture was compared to the previous same-camera source `4a3dcc8d893eb021296ce61f7f113d39d6501313`. Technical falloff correction is real but the visual improvement is marginal: in Bastion V landscape, only about **1% of pixels** changed by more than 10 RGB units, with mean absolute RGB changes approximately **0.36/0.21/0.16 on an 8-bit scale**. The approach still reads as a grid of small rectangular pavers, secondary work areas remain too empty and the grass is largely flat. On mobile, details are still small, despite all structures being inside the frame.

**Quality decision:** keep the corrected math and reproducible tests, but reject this pass as the final premium artistic solution. The next independent pass must change the **design of the geometry and scene composition** (less regular paving, better material-ground integration, landscape vegetation masses), not repeat global brightness or color tweaks. Shared Windows lease released after terminal native success; gameplay/main art remain untouched. Physical iPhone, WebGL, FPS and memory profiling remain unverified.
