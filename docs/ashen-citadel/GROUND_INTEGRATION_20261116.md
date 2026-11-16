# Inhabited ground and courtyard integration

Owner continues premium refinement. Same isolated Ashen workstream and branch. Baseline source3b9c246eb5f0a611873dfa89aa9ea3997aaa7238 / native43863284139.

## Decision and reuse
Last Unity images show a grass-filled courtyard, sparse disconnected approach stones, weak craft-yard soil transitions, and small footing gaps. Lighting/material improvements cannot fix these composition/contact issues. Reuse current original AshenLandscape and existing stone palette; do not change architecture or camera.
Ran find-reuse.py against curated, current, historical indexes and read gallery entry: RockTerrainSeamFiller and RockTerrainFamilyV1 are conditional seam/rock families, not a fitted courtyard for this settlement. Adapt the current owned landscape, avoiding imports from unrelated scenes.

## Authored changes
Broad 19cm soil crown across settlement, feathered over14m outside core; no individual plot platform or rim. Match vegetation placement height in Unity. Individually chipped near-flush paving stones in courtyard, eroded approach, small craft thresholds. Path/yard earth masks strengthened, irregular edges kept. Random seed restored after paving so unrelated boulders retain exact positions.

## Gates
Deterministic OBJ regeneration and upward paving winding; source/art validation; Blender geometry review; actual same-camera Unity I/V and portrait, gameplay tests. Soil texture is Unity-authored and requires native visual review. No final art approval or mobile-performance claim.
