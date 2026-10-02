# Asset Deep Uplift v1

Deterministic zero-credit lane for improving **existing canonical GLBs** only.

## Evidence-driven result

The deep audit found **no current asset that justifies blind geometry surgery**.

Blender diagnostics showed:
- SteppedRockTerrace — 6,379 tris / full 3-image PBR → keep geometry.
- BroadRockPlatform — 8,650 tris / full 3-image PBR → keep geometry.
- MidTier Piece01–04 — 6,324–16,650 tris / full 3-image PBR → keep geometry; solve production placement/coverage separately.
- TerraceStairRock — 49,800 tris / valid UV+normals / 0 images → surface rescue candidate produced.
- StreetLandingTransition — 49,800 tris / valid UV+normals / 0 images → surface rescue candidate produced; route/interface failure remains unchanged.

Current canonical ResidentialTerraceRock, RockTerrainSeamFiller and TowerWallRock already contain PBR images. Generic one-material rescue outputs for those assets were rejected.

## Persisted candidates

Only two candidates survive this pass:

- `pipeline/candidates/asset-deep-uplift-v1/TerraceStairRock-PBRRescue.glb`
  - SHA-256 `4173b794b875c872aee1cb663e67a516bf37c96273c752f8ef4b6fb9225727b9`
  - exact geometry/bounds preserved
  - 512 basecolor + roughness + normal

- `pipeline/candidates/asset-deep-uplift-v1/StreetLandingTransition-PBRRescue.glb`
  - SHA-256 `16b090b4f0fbd54284919887877e527d932c7366e3b78d970c546ae30837c28f`
  - exact geometry/bounds preserved
  - 512 basecolor + roughness + normal
  - visual-overlay role only; never infer traversal certification

Candidate GLBs live outside Unity Resources intentionally. They must not become production merely because they exist in the repository.

## Promotion sequence

1. verify candidate SHA and matching canonical source SHA;
2. import candidate into an isolated Unity validation path;
3. compare canonical vs candidate at official 19 / 12 / 9 / mobile cameras;
4. verify collider/hotspot signature is unchanged;
5. promote only if the candidate is clearly better in the integrated frame;
6. otherwise retain the current canonical GLB.

## Geometry-edit rule

A future geometry edit needs direct evidence of a form defect that surface/composition cannot solve. Acceptable evidence includes:
- destructive silhouette noise visible at official camera;
- impossible/incorrect physical visual envelope for the intended visual-only role;
- proven occlusion caused by removable sacrificial geometry;
- severe normals/topology defect not fixable without mesh editing.

Triangle count alone is not a reason to edit a mesh. Decimation is optimization, not visual uplift.

## Surface preservation rule

Before any generic material rescue, inspect the **current canonical file**, not only historical reports. If it already contains useful basecolor/normal/mask/material information, preserve it. Never replace authored PBR with one generic rock material merely because an older source version was flat.

## Tooling

The GitHub-hosted Blender lane is recorded by:
- `pipeline/asset-deep-uplift-run-request.json`
- `.github/workflows/asset-deep-uplift-blender.yml`

The workflow is manual-only after this pass. It must remain zero-credit and must not use the Windows Unity runner.

## Stop conditions

- Tripo or other paid generation without fresh explicit authorization;
- canonical GLB overwrite before official-camera proof;
- gameplay collider/hotspot/topology change;
- claiming an interface/traversal repair from a surface-only change;
- replacing a current PBR source with a generic material without a matched-camera proof.
