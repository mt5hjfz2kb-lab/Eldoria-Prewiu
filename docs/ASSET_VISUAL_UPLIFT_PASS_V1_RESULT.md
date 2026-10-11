# Asset Visual Uplift Pass v1 — Result

Date: 2026-10-02  
Status: **TECH PASS / SELECTIVE VISUAL PASS**  
Branch: `visual-proof/asset-visual-uplift-v1`  
Validated run: **37002187636 — SUCCESS**  
Artifact: **11223898210**  
Tripo credits: **0**  
New source geometry: **0**  
Gameplay topology changes: **0**

## Objective

Improve the perceived quality of the highest-return assets already present in the canonical Valoria library, without blanket reprocessing, new geometry by default, paid generation, or gameplay changes.

The pass starts from the production result of `ASSET LIBRARY REPROCESSING PASS v1` and the canonicalized 26-GLB library. Canonical GLB bytes were not edited.

## Selection and actual result

### Hero Bastion — IMPROVED

Canonical source:
`Unity/Assets/Eldoria/Resources/Valoria/HeroBastionGenerated/Valoria_HeroBastion_v1.glb`

Why selected:
- dominant focal asset at every official zoom;
- geometry/silhouette already certified;
- remaining leverage was surface/value hierarchy, not regeneration.

Executed:
- renderer-local role treatment;
- restrained stone / rock / roof / metal / accent separation;
- low-gloss stone response and controlled normal strength;
- existing source textures/normals preserved.

Evidence:
- **1 Hero renderer touched**;
- matched BEFORE/AFTER frames show stronger focal separation and cleaner relation between the bright Bastion body and the darker supporting fortress mass, most clearly at zoom 12/9/mobile.

Verdict: **VISUAL PASS**.

### Dedicated production buildings — PARTIAL IMPROVEMENT

Canonical sources in scope:
- `Valoria_Aserradero_AP2_v1.glb`
- `Valoria_Cuartel_AP2_v1.glb`
- `Valoria_Granero_BIII_v1.glb`

Executed:
- functional palette separation for production / military / granary roles where the current canonical scene exposes matching renderer hierarchy;
- existing source materials/maps remain authoritative;
- no geometry or gameplay ownership changed.

Evidence:
- **2 dedicated renderers touched** in the validated canonical frame.

Verdict:
- the two matched in-frame dedicated renderers receive a visible but secondary surface uplift;
- the gate does **not** prove a separate Granero renderer treatment, so this pass makes no unsupported claim for that asset.

Practical ceiling for v1:
- do not force a new placement or alter composition only to manufacture evidence;
- Granero remains **not separately validated by this surface-only pass**.

### Stone Architecture — IMPROVED / HIGH RETURN

Production subset:
- `CornerWallL.glb`
- `HighStraightWall.glb`
- `RockToWallTransition.glb`

Why selected:
The previous reprocessing pass replaced these instances with one flat architectural-stone material. That improved coherence but collapsed useful source map information and value separation.

Executed:
- preserve source base map where present;
- preserve normal map where present;
- preserve occlusion/mask inputs where representable in URP Lit;
- normalize roughness/metallicity;
- role-sensitive fallback tint instead of one flat material.

Evidence:
- included within **17 imported Stone/Terrain renderers touched**;
- the AFTER frames show materially stronger retaining-wall depth and darker side masses around the Hero approach, especially at zoom 12 and 9.

Verdict: **VISUAL PASS / one of the strongest returns in the pass**.

### Terrain & Terrace — IMPROVED / HIGH RETURN

Production subset:
- `BroadRockPlatform.glb`
- `SteppedRockTerrace.glb`

Executed:
- preserve texture/normal/AO inputs;
- stronger but controlled rock normal response;
- low-gloss terrain/rock family;
- no geometry/regeneration.

Evidence:
- included within the same **17 imported Stone/Terrain renderers touched**;
- AFTER improves separation between the bright processional route, terrace stone and darker geological support without changing silhouette.

Verdict: **VISUAL PASS**.

### Mid-Tier Architecture Piece01–04 — NO CURRENT-FRAME UPLIFT CLAIM

The family remains canonical and the uplift code supports sub-material separation if these pieces are instantiated through `AddMidTierPiece`.

However, the current accepted `AssetLibraryReprocessingPassV1.Build()` calls `ReassembleMidTierCore()`, which currently uses Terrain/Stone support only; `ReassembleCompactCoreArchitecture()` is not part of the accepted active build path.

Evidence:
- **0 Mid-Tier renderers touched** in the validated run.

Verdict:
- **NO CLAIM / practical ceiling for this surface-only v1 frame**.
- Re-introducing Mid-Tier geometry solely to demonstrate the pass would be a composition change and would violate the selective, non-conflicting scope.
- The family remains available for a future composition-owned pass.

### Rescued support — IMPROVED WHERE ALREADY VISIBLE

Canonical family:
- ResidentialTerraceRock
- RockTerrainSeamFiller
- StreetLandingTransition
- TerraceStairRock
- TowerWallRock

Executed:
- only already-visible rescued renderers receive support-family surface normalization;
- no new placement for the sake of coverage.

Evidence:
- **8 rescued renderers touched**.

Verdict:
- **VISUAL PASS for the visible rescued support subset**;
- StreetLandingTransition / TerraceStairRock and any non-active member receive no fabricated claim.

### GateStreetRiseRock_MV1 — EXPLICITLY EXCLUDED

No primary treatment.
Its historical traversal/interface failure remains unchanged; it stays visual-only and is not promoted by this pass.

## Implementation

Runtime surface layer:
- `Unity/Assets/Eldoria/Scripts/Presentation/AssetVisualUpliftPassV1.cs`

Integration:
- `Unity/Assets/Eldoria/Scripts/Presentation/AssetLibraryReprocessingPassV1.cs`
  - existing Stone/Terrain instances route through texture/normal-preserving uplift materials;
  - existing scene Hero/dedicated/rescued assets receive renderer-local treatment;
  - no collider/hotspot ownership is added.

Validation:
- `Unity/Assets/Eldoria/Scripts/Editor/AssetVisualUpliftGateV1.cs`
- `.github/workflows/asset-visual-uplift-v1.yml`

The stable gate uses D3D11 on the Windows Unity runner and avoids global shader-pass precompilation after the first D3D12 capture attempt crashed inside Unity culling.

## Validation evidence

Validated:
- run **37002187636 — SUCCESS**
- artifact **11223898210**

The artifact contains matched:
- `before-19.png` / `after-19.png`
- `before-12.png` / `after-12.png`
- `before-9.png` / `after-9.png`
- `before-mobile.png` / `after-mobile.png`
- BEFORE/AFTER metrics
- evidence JSON
- Unity log

Technical invariants from evidence:
- camera matched: **true**
- deterministic rebuild BEFORE/AFTER: **true**
- collider/hotspot signature equal: **true**
- gameplay topology changed: **false**
- new geometry generated: **false**
- Tripo credits: **0**

Coverage:
- Hero renderers touched: **1**
- dedicated renderers touched: **2**
- rescued renderers touched: **8**
- imported Stone/Terrain renderers touched: **17**
- Mid-Tier renderers touched: **0**

Scene metrics:

| Metric | BEFORE | AFTER |
|---|---:|---:|
| active renderers | 781 | 781 |
| unique materials | 74 | 76 |
| scene triangles | 1,647,618 | 1,647,618 |
| active lights | 25 | 25 |

Interpretation:
- renderer count unchanged;
- triangle count unchanged;
- light count unchanged;
- only +2 unique runtime materials, consistent with a surface/detail uplift rather than geometry inflation.

## Visual review

Official camera review:

- **Zoom 19:** improvement is intentionally restrained; full-frame composition is unchanged, with slightly better mass separation around the Hero fortress.
- **Zoom 12:** clear improvement in retaining-wall / terrace depth and Hero support mass readability.
- **Zoom 9:** strongest desktop evidence; darker architectural supports create better stone hierarchy and edge separation without stealing focus from the Bastion.
- **Mobile:** uplift remains perceptible despite reduced screen area; Hero/support contrast is clearer without increasing scene density.

The pass does **not** solve the larger benchmark gap by itself. It improves the quality of existing assets; it does not replace the composition/verticality/environment work owned by separate Valoria convergence passes.

## Cost / toolchain verdict

- Tripo: **not used**
- Blender: **not used**
- new GLB generation: **none**
- canonical GLB mutation: **none**
- gameplay topology/colliders/hotspots: **unchanged**

This validates the intended Toolchain Automation v2 decision rule: surface/detail gaps were handled in Unity rather than escalating to new geometry or paid generation.

## Final verdict

**TECH PASS / SELECTIVE VISUAL PASS.**

Promote:
- Hero Bastion surface hierarchy;
- Stone Architecture surface preservation/separation;
- Terrain & Terrace surface preservation/separation;
- visible rescued support treatment;
- matched dedicated production treatment.

Do not overclaim:
- Mid-Tier has 0 active renderer coverage in this accepted frame;
- Granero is not separately proven by the gate;
- GateStreetRiseRock_MV1 remains outside the primary uplift target.

The block meets the v1 success criterion: visible quality increases on the highest-return active assets, gameplay signature is unchanged, scene geometry is unchanged, and no Tripo credits were spent.
