# Valoria Surface v1

Status: ACTIVE VALIDATION
Updated: 2026-09-29

## Evidence source

Aserradero diagnostic run: 36493273686

Certified production source:
- asset: Valoria_Aserradero_AP2_v1
- SHA-256: 1932e36c7fdf972c2ffb209ed7b3679fd7f0243d0f8e713e0f4643146fa9ad0e
- bytes: 11134796
- triangles: 49800
- vertices: 55752
- materials: 1
- UV0: present
- normals: present

Textures:
- basecolor: 2048x2048, sRGB
- normal: 1024x1024, Non-Color
- combined RM mask: 1024x1024, Non-Color

## Primary finding

The source GLB already carries a valid PBR surface structure.

The previous Unity integration path for Aserradero rebuilt a simplified material through `BenchmarkPieceIntegrated`, preserving basecolor but discarding the source normal/RM response and imposing a flat tint/smoothness profile.

This is classified as a SURFACE integration defect, not a geometry defect.

## Surface v1 rule

For dedicated production GLBs that already provide valid PBR maps:
- preserve their imported material and map routing;
- do not replace them with a basecolor-only integration material;
- do not regenerate geometry to fix a surface-only failure;
- use LookDev/environment calibration after preserving the source PBR first.

Aserradero now follows the same preservation principle as Cuartel via `BenchmarkPiece`.

## Current baseline

Aserradero:
- geometry: accepted
- source PBR: valid
- integration path: corrected to preserve authored PBR
- next gate: integrated official-camera comparison

Cuartel:
- production PBR preservation: active
- full Unity gate: success
- next role: second-building reuse proof for the formula

## Promotion criterion

Surface v1 becomes reusable baseline when:
1. Aserradero preserves its PBR and improves integrated readability;
2. Cuartel remains coherent under the same LookDev/material-family rules;
3. neither requires unrelated per-building lighting or shading logic.

If those conditions hold, this document becomes the first reusable surface layer of Valoria Visual Formula v1.

## Two production surface lanes

### Lane A — authored PBR preservation

Use for dedicated production GLBs such as Aserradero/Cuartel when diagnostics prove that useful PBR maps already exist.

Rule:
- preserve source basecolor / normal / metallic-roughness information;
- do not replace it with a flat tint or basecolor-only material;
- diagnose Unity import/adaptation before changing the source asset.

### Lane B — certified geometry surface rescue

Use for historical certified GLBs whose geometry, UV0 and normals remain useful but whose optimized artifact contains only a flat/no-texture material.

Current evidence shows this for:
- TerraceStairRock;
- GateStreetRiseRock MV1;
- ResidentialTerraceRock;
- StreetLandingTransition;
- RockTerrainSeamFiller.

TowerWallRock has an exact certified GLB on the runner and is undergoing the same non-mutating diagnostic.

Rules:
- preserve certified geometry and SHA evidence;
- add a new Valoria material layer without reopening paid generation;
- start with semantically simple SUPPORT assets (rock/terrain);
- do not assign one rock material to mixed architecture simply because it is easy;
- mixed assets require semantic material segmentation before they can become final art.

The first zero-credit Lane B proof is RockTerrainSeamFiller with the deterministic `rock` Surface v1 rescue profile.

## Hero proof reuse

GateStreetRiseRock MV1 is selected as the bounded HERO visual proof for the formula because it contains architecture + rock and already has a certified silhouette. Its historical circulation/interface failure remains authoritative and is explicitly out of scope: surface improvement must never be presented as a repaired route.

## Unity/glTFast preservation rule

The project uses `com.unity.cloud.gltfast 6.14.1` with URP. glTFast can import glTF metallic-roughness PBR, normal, occlusion and emission directly and may use its own shaders such as `Shader Graphs/glTF-pbrMetallicRoughness`.

Therefore Valoria's URP adaptation layer must treat the following as already-compatible and preserve them unchanged:
- `Universal Render Pipeline/*`
- `Shader Graphs/glTF-*`
- `glTF/*`

Only genuinely legacy/non-URP materials should pass through the fallback `AdaptForUrp` reconstruction path.

This closes a second PBR-loss route beyond the old `BenchmarkPieceIntegrated` path: converting a valid glTFast material merely because its shader name did not begin with `Universal Render Pipeline/`.

Production fix: `ValoriaKit.AdaptForUrp` now preserves glTFast shaders before any fallback conversion.
