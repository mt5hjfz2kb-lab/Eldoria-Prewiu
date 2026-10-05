# VALORIA ROCK / TERRAIN FAMILY v1

Date 2026-10-05. Workstream: `valoria-rock-terrain-family-v1`. Base main `18774d4e51c8136f0079b7a76595e8c9307a34b1`.

Lower Gate, Bridge, Road and Stair are closed **PRODUCTION FAMILY PASS** and immutable. Camera, target, global composition, platform envelope, Bastion, Walls and gameplay remain locked. This block replaces structural Rock/Terrain only.

## TARGET → BLOCKOUT authority

Canonical target: `docs/evidence/valoria-art-production-reset-v1/canonical-target.jpeg`, 1536×1024, SHA-256 `8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689`. Approved reset blockout: run **37275659322** / artifact **11329019907**. Camera remains orthographic yaw20 / pitch35 / span48 / distance110 plus the approved landscape and portrait variants.

Target silhouette requires a **broad continuous cliff platform** with **two levels and a clear 4-unit rise**. Target family occupancy estimates are ~30–38% playable ground and ~12–18% cliff/upper rise. The family must therefore improve a large percentage of the frame without redefining its composition.

## Bounded family ownership

This family owns exactly:
- `ForegroundBank`
- `MainPlatform`
- `PlayableGround`
- `UpperTerraceCliff`
- `UpperTerraceGround`
- `CliffShoulder_0..6`

Explicitly excluded:
- `BackgroundTerrain` — Background family, locked for this block.
- `BridgeSupport` — accepted Bridge family, immutable.
- Water/shore, vegetation, Walls, Bastion, Cabin, Camp, props, buildings and lighting.

## FAMILY SPEC

### Macro silhouette and levels
- Preserve the exact approved outer boundary at foreground/main/upper levels.
- Preserve world height anchors: foreground 3, main platform 7, upper terrace 11.
- No camera compensation, widening, raising/lowering or element movement.
- MainPlatform and UpperTerraceCliff retain their exact top/bottom perimeter rings; all additional shaping is inset **inside** those envelopes.

### Rock authoring
The greybox vertical cliff walls are replaced by four broad faceted bands between locked top/bottom rings. Intermediate bands move only inward and use deliberately nonuniform medium-scale offsets. This creates large authored planes, breaks and shadow rhythm while keeping the approved silhouette exact.

No micro-rock scatter, no pebble noise, no repeated modular cliff stamps, no external boulders, no displacement that escapes the blockout envelope. `CliffShoulder_0..6` already encode the approved irregular shoulder rhythm and therefore keep their exact envelopes, gaining coherent rock material rather than being used to redesign the plateau.

### Ground authoring
`PlayableGround` and `UpperTerraceGround` preserve their exact geometry/elevation so Road, Stair, Gate and Bastion interfaces cannot be disturbed. Their production contribution is a distinct earth/ground material family, separate from both the retained civic paving and the cliff rock.

### Material hierarchy
- rock_shadow: darkest large recessed band;
- rock_mid: main cliff mass;
- rock_face: selective exposed planes;
- earth_bank / earth_top / earth_upper: warmer, quieter ground family.

Rock is darker/cooler and more faceted than ground. Ground is warmer/more matte and remains visibly different from accepted Road paving. Materials must support architecture rather than compete with Lower Gate/Bastion.

### Legacy
All reset legacy candidates remain non-authoritative. `RockToWallTransition.glb`, `BroadRockPlatform.glb` and `SteppedRockTerrace.glb` are classification B only and **reuse remains unauthorized**; rescued rock sources are classification C. Source01 therefore uses new target-bound explicit geometry from the approved blockout rather than letting legacy modules redefine the terrain.

### Production/performance
Combined reset planning budgets: Cliff/Rock 45k + Ground/Terrain 15k LOD0. Source01 intentionally targets far below the 60k combined ceiling while retaining broad silhouette; actual budget is measured at TECH gate. UV/normals/tangents mandatory. Zero gameplay colliders. Zero Tripo.

## Quality bar / gates

**TECH PASS** is necessary but insufficient.

**ART SOURCE PASS** requires explicit YES to:
1. Frame no longer reads as greybox around accepted architecture.
2. Large rock masses approach target rhythm/reading.
3. Plateau macro silhouette remains approved.
4. Real depth without noisy surface chatter.
5. Authored rather than procedural/repetitive read.
6. Detail scale survives mobile landscape and portrait.
7. Rock/Terrain visually receives Lower Gate/Bridge/Road/Stair without altering them.
8. Rock, earth and paving are materially distinct.
9. Source promises a real frame-level quality jump.

If any hard question fails, source remains at ART SOURCE and must be reauthored inside this family.

**UNITY INTEGRATION PASS** must retain accepted Lower Gate + Bridge + Road + Stair identically in BEFORE/AFTER and replace only the 12 Rock/Terrain placeholders above.

**MATCHED-CAMERA VISUAL PASS** requires all seven official cameras plus a dedicated **TARGET vs APPROVED BLOCKOUT vs STRUCTURAL FRAME WITH ROCK/TERRAIN** comparison, zero protected-family movement, preserved macro silhouette, clean interfaces, mobile readability, material/import sanity, zero unauthorized colliders and gameplay regression PASS.

Current status: **SOURCE AUTHORING AUTHORIZED / 0 TRIPO**.
