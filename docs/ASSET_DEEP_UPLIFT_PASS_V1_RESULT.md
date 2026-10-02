# Asset Deep Uplift Pass v1 — Classification / execution state

Date: 2026-10-02  
Status: **CLASSIFICATION PASS / GEOMETRY EXECUTION BLOCKED BY ACTIVE OWNERSHIP**  
Branch: `visual-proof/asset-deep-uplift-v1`  
Tripo credits: **0**  
Canonical GLBs mutated: **0**

## Purpose

Follow Asset Visual Uplift v1 with an asset-by-asset decision that separates assets already good enough from those limited by surface or geometry. The goal is to spend deep Blender work only where geometry is genuinely the bottleneck.

## Classification result

The canonical 26-GLB library is classified in `pipeline/asset-deep-uplift-v1-audit.json`.

- **A — keep:** the six StoneKit pieces plus RockTerrainSeamFiller. Their current production/support function does not justify mesh surgery.
- **B — surface/integration:** Hero Bastion, ResidentialTerraceRock, TowerWallRock, Stone Architecture production subset, Aserradero, Cuartel, Granero and PlayerCity. Their geometry is accepted/useful; the remaining leverage is authored surface, bake, identity and grounding.
- **C — geometry-limited / prove before editing:** MidTier Piece01–04, StreetLandingTransition, TerraceStairRock, BroadRockPlatform and SteppedRockTerrace.
- **D — do not deep-uplift for failed role:** GateStreetRiseRock MV1 as a traversable connector. It may remain a landmark fragment, but its documented physical/interface failure should not receive repeated polish in an attempt to turn it into a route.

## Highest-return C queue

1. **SteppedRockTerrace** — strongest first candidate because terrace/vertical readability contributes directly to the mountain-city image and its earlier pilot failed to create enough visual lift.
2. **TerraceStairRock** — useful vertical-city silhouette, but only as visual skin over independent gameplay traversal.
3. **BroadRockPlatform** — target the generic pedestal read while preserving its usable envelope.
4. **StreetLandingTransition** — reduce heavy parapet/occlusion only for visual-overlay use; do not claim interface repair.
5. **MidTier Piece01–04** — isolated audit before deciding whether they deserve cleanup or replacement. Asset Visual Uplift v1 had zero active renderer coverage for this family.

## What was deliberately not done

No canonical GLB was edited in this block. That is not an artistic hesitation; it is a repository ownership constraint.

The live workstream `valoria-reference-convergence-v2` currently owns both:
- `valoria-production-composition`
- `windows-runner-heavy`

The parallel-workstream protocol forbids this pass from taking that runner or mutating the production composition while the owner remains active. Blender binary changes without the official Unity camera proof would also violate the camera-first acceptance rule.

## Prepared execution lane

`tools/asset-deep-uplift/README.md` records the exact zero-credit sequence and candidate-specific guardrails. The first binary execution after the runner is released is **SteppedRockTerrace**, followed by **TerraceStairRock**. Each output must remain a candidate until isolated + integrated 19/12/9/mobile evidence shows a clear improvement with unchanged gameplay signature.

## Current verdict

The useful conclusion is narrower than “rebuild everything”:

- most of the library should **not** be remodeled;
- the best geometry-uplift opportunity is concentrated in a small transition/terrace subset;
- GateStreetRiseRock should not absorb more route-repair effort;
- paid generation remains unjustified at this point.

The classification and execution recipe are complete. Binary geometry execution is intentionally pending release of the shared production runner/composition ownership; no false visual-pass claim is made before that evidence exists.
