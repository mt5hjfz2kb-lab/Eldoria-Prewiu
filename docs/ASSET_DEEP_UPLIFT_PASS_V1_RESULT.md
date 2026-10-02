# Asset Deep Uplift Pass v1 — Result

Date: 2026-10-02  
Status: **TECH PASS / DEEP AUDIT CLOSED / 2 PBR CANDIDATES PARKED**  
Branch: `visual-proof/asset-deep-uplift-v1-r3`  
Tripo credits: **0**  
Canonical production GLBs mutated: **0**  
Gameplay topology changes: **0**

## Objective

Continue the previous Asset Visual Uplift work by testing whether the remaining visual ceiling is actually caused by weak geometry. The rule for this pass was to diagnose the canonical GLBs first, perform mesh surgery only when evidence proves a geometry gap, and otherwise preserve the accepted geometry.

## Main finding

The initial A/B/C/D classification was intentionally provisional. Real Blender diagnostics changed the conclusion.

**None of the tested C candidates produced evidence that geometry surgery is the right next intervention.**

The apparent weak points divide into two groups:

1. already healthy, relatively light geometry with complete PBR inputs, where the remaining issue is composition / identity / production placement;
2. historical 49.8K-triangle modules with valid UVs/normals but a genuinely flat, zero-texture material, where the correct intervention is a surface rescue rather than remeshing.

The canonical audit is updated in `pipeline/asset-deep-uplift-v1-audit.json`.

## Geometry diagnostics

### SteppedRockTerrace

Run **37004870013 — SUCCESS**  
Artifact **11225760232**

Canonical:
- SHA-256: `b2d11106fd517f51e0d76ffb733c622db33b045bb12f2ecbe5c6ae043efe05fe`
- 1 object
- **6,379 triangles**
- 1 material
- **3 source images**: 2K basecolor, 2K normal, 1K mask
- UVs: present
- normals: present

Verdict: **KEEP GEOMETRY / PBR COMPLETE**.

The prior visual weakness is not evidence of an overbuilt or broken mesh. No deep geometry edit was executed.

### BroadRockPlatform

Run **37005128307 — SUCCESS**  
Artifact **11225234064**

- **8,650 triangles**
- 3 PBR images
- UVs/normals valid

Verdict: **KEEP GEOMETRY / PBR COMPLETE**.

### MidTier Piece01–04

Same diagnostic run **37005128307**.

| Piece | Triangles | PBR images | Decision |
|---|---:|---:|---|
| Piece01 | 11,997 | 3 | keep geometry |
| Piece02 | 13,201 | 3 | keep geometry |
| Piece03 | 16,650 | 3 | keep geometry |
| Piece04 | 6,324 | 3 | keep geometry |

All retain valid UVs and normals.

Asset Visual Uplift v1 had zero active renderer coverage for this family, but that is a production composition/coverage question, not proof of bad geometry. No mesh surgery is justified from the current evidence.

### TerraceStairRock

Diagnostic:
- run **37005128307**
- artifact **11225234064**
- **49,800 triangles**
- valid UVs/normals
- **0 images**
- one flat material

Verdict: **SURFACE GAP, NOT PROVEN GEOMETRY GAP**.

### StreetLandingTransition

Diagnostic:
- run **37005128307**
- artifact **11225234064**
- **49,800 triangles**
- valid UVs/normals
- **0 images**
- one flat material

Verdict: **SURFACE GAP + HISTORICAL INTERFACE LIMIT**.

Its documented traversal/interface failure is unchanged. This asset remains a visual overlay only; adding a better surface does not turn it into a certified route.

## PBR candidates actually produced

### TerraceStairRock PBR Rescue

Run **37005340145 — SUCCESS**  
Artifact **11225483548**

Source:
- SHA-256: `83fce93daeb5bb455ab89bb195c39f617b3ea69ff9bf29f938ae4744caff3e5e`
- 49,800 triangles
- 0 source images

Candidate:
- SHA-256: `4173b794b875c872aee1cb663e67a516bf37c96273c752f8ef4b6fb9225727b9`
- 49,800 triangles
- identical bounds
- geometry modified: **false**
- deterministic 512 PBR set:
  - basecolor
  - roughness
  - tangent normal

Persisted candidate:
`pipeline/candidates/asset-deep-uplift-v1/TerraceStairRock-PBRRescue.glb`

### StreetLandingTransition PBR Rescue

Run **37005568784 — SUCCESS**  
Artifact **11224644144**

Source:
- SHA-256: `ef367d9f0f671cd29e1b02e2d36a2dfdea3acd6e789087e4da5fb5e087e81b01`
- 49,800 triangles
- 0 source images

Candidate:
- SHA-256: `16b090b4f0fbd54284919887877e527d932c7366e3b78d970c546ae30837c28f`
- 49,800 triangles
- identical bounds
- geometry modified: **false**
- deterministic 512 PBR basecolor / roughness / normal

Persisted candidate:
`pipeline/candidates/asset-deep-uplift-v1/StreetLandingTransition-PBRRescue.glb`

The two candidates were rebuilt from exact canonical source SHAs and persisted on run **37006224604 — SUCCESS**, artifact **11226280027**. The persistence gate re-verified the expected candidate SHA before committing them.

## Rejected experiments

A broader surface-rescue batch was deliberately tested against:
- ResidentialTerraceRock
- RockTerrainSeamFiller
- TowerWallRock

Run **37005849399 — SUCCESS**  
Artifact **11225747180**

That experiment was **rejected for promotion** after inspecting the current canonical sources:

- ResidentialTerraceRock: **4 materials / 12 images**
- RockTerrainSeamFiller: **1 material / 3 images**
- TowerWallRock: **2 materials / 6 images**

The current canonical files already contain authored texture information. Replacing them with one generic rock material would discard useful data and risk a regression. The generated experimental candidates are not persisted as approved candidates and must not replace the canonical assets.

This supersedes older historical assumptions that these current canonical files were still flat/no-texture.

## What this pass did not do

- no Tripo generation;
- no paid credits;
- no new asset family;
- no canonical production GLB replacement;
- no gameplay collider/hotspot/topology changes;
- no geometry surgery;
- no forced MidTier placement merely to create evidence.

The first hosted Blender diagnostic also exposed an infrastructure issue: Blender 4.0 on the GitHub-hosted environment lacked NumPy and could return exit code 0 despite the Python traceback. The workflow was hardened by installing `python3-numpy` and requiring a non-empty report before accepting a diagnostic.

## Production-placement decision

Neither TerraceStairRock nor StreetLandingTransition is currently used by the accepted Valoria production composition. Therefore this pass deliberately does **not** force either candidate into the city merely to manufacture integrated-camera evidence.

The candidates remain parked outside Unity Resources. If a future real production placement selects either asset, that placement must compare canonical vs candidate at official **19 / 12 / 9 / mobile** cameras and verify an unchanged collider/hotspot signature before any canonical replacement.

No immediate Unity promotion gate is required for the current production frame because these assets are not currently contributing to it.

## Final verdict

**TECH PASS / DEEP AUDIT CLOSED / 2 VALID PBR CANDIDATES PARKED / NO GEOMETRY SURGERY JUSTIFIED.**

The deeper audit did produce additional real asset work, but not in the originally assumed form:

- SteppedRockTerrace, BroadRockPlatform and MidTier Piece01–04 should **not** be remodeled on current evidence.
- TerraceStairRock and StreetLandingTransition were the genuine surface-deficit assets; both now have deterministic, SHA-pinned PBR candidates with unchanged geometry.
- ResidentialTerraceRock, RockTerrainSeamFiller and TowerWallRock already carry PBR data in their current canonical versions; generic replacement candidates were rejected.
- GateStreetRiseRock MV1 remains unsuitable as a certified traversable connector.
- No production promotion is currently warranted because neither candidate is used in the accepted production composition. Validate only when a real placement selects one.
