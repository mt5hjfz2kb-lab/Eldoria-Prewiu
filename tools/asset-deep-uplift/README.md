# Asset Deep Uplift v1

This folder records the deterministic execution lane for **existing canonical GLBs only**. It does not authorize new assets or Tripo spend.

## Priority order

1. SteppedRockTerrace
2. TerraceStairRock
3. BroadRockPlatform
4. StreetLandingTransition
5. MidTier Piece01–04 isolated audit

## Required Blender sequence

For each C candidate:

1. run the canonical `tools/tripo_module_blender.py` in diagnostic-only mode and record objects, triangles, bounds, UVs, material/image routing and connected components;
2. compare against the current canonical SHA from `pipeline/asset-library-canonicalization-audit.json`;
3. apply only reversible conservative cleanup first: duplicate-vertex merge at tiny tolerance, outward normal consistency and material sanity;
4. geometry edits are allowed only where the diagnostic + official camera evidence proves a form problem. Preserve occupied footprint and all gameplay topology; these GLBs are visual skins, never gameplay authority;
5. do not use blanket decimation as an uplift. Triangle reduction is an optimization decision, not a visual-improvement decision;
6. export to a candidate path, never overwrite the canonical GLB before Unity proof;
7. validate isolated plus integrated 19/12/9/mobile before promotion;
8. if the result is not clearly better, keep the current canonical GLB.

## Candidate-specific guardrails

- **SteppedRockTerrace:** prioritize readable landings, cleaner silhouette and authored rock/terrace transition. Do not alter the approved placement footprint just to make the mesh look dramatic.
- **TerraceStairRock:** trim only sacrificial rock shoulders/occluding noise. It must remain a visual overlay on independent traversal.
- **BroadRockPlatform:** attack pedestal/amorphous rock read, not the usable platform envelope.
- **StreetLandingTransition:** visual overlay only. Its historical interface failure is not repaired by cosmetic geometry work.
- **MidTier Piece01–04:** audit first; 0 renderers were touched in Asset Visual Uplift v1, so no production-frame geometry claim is currently justified.

## Stop conditions

- any collider/hotspot/gameplay topology change;
- any request to use the currently owned Windows runner before release;
- any Tripo credit spend;
- any output that changes silhouette/footprint without matched-camera evidence.
