# VALORIA ROAD FAMILY v1

Date 2026-10-05. Workstream: `valoria-road-family-v1`. Base main `42fff830351a6c07384a37cb42c575bda6cb88ca`.

Lower Gate and Bridge are closed **PRODUCTION FAMILY PASS** and immutable. This block replaces Road only. Camera, target, global composition, platform, Bastion, Stair, Terrain, Walls, gameplay, hotspots and colliders remain protected.

## TARGET → BLOCKOUT authority

Canonical target: `docs/evidence/valoria-art-production-reset-v1/canonical-target.jpeg`, SHA-256 `8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689`.
Approved reset blockout: run **37275659322** / artifact **11329019907**. Official camera remains orthographic yaw20 / pitch35 / span48 / distance110, with the already-defined landscape and portrait variants.

The target requirement is a **diagonal continuous bridge-road-stair-gate spine** and the Road family function is a **continuous legible spine**. The approved blockout Road family is exactly four placeholders:
- `ForegroundRoad`
- `GateThreshold`
- `MainRoad`
- `UpperRoad`

The road family does not own Bridge, Lower Gate, Stair, Ground or Terrain. It must meet those interfaces without moving or reauthoring them.

## FAMILY SPEC

### 1. Route and silhouette
- Preserve each placeholder's exact X/Y footprint and its original top/bottom Z envelope.
- No widening, narrowing, bending or camera compensation.
- `GateThreshold` remains the broad receiving area immediately behind accepted Lower Gate.
- `MainRoad` remains the long middle spine toward the locked Stair.
- `UpperRoad` remains the receiving strip beyond Stair toward Bastion; it is authored now only because it is already part of the approved Road family, not because Stair is being produced.
- `ForegroundRoad` remains the lower approach surface before Bridge and may not change Bridge placement.

### 2. Visual hierarchy
Road is circulation infrastructure, not a hero asset. It must be clearly readable from the official camera while staying subordinate to accepted Lower Gate and Bridge. Surface value sits darker/earthier than Bridge cut-stone and lighter/more organized than raw Ground/Terrain so the route reads without bleaching the entire scene into one stone value.

### 3. Geometry and surface
The Road must not read as a flat texture pasted onto terrain. Each road strip therefore keeps a real shallow structural core plus:
- broad central paving skin;
- real shallow edge bands contained inside the original blockout thickness;
- a 10 mm center recess relative to the approved outer edge top, entirely inside the locked envelope;
- selective structural bevels small enough not to change screen occupancy materially.

No raised curb, prop, wall, rubble, drainage, vegetation or terrain dressing may escape the original road bounds. Surface language uses large staggered civic paving / stone-grain scale that remains legible in mobile views. No micro-cobble noise.

### 4. Interfaces
- Bridge and Lower Gate remain retained accepted production families in Unity BEFORE and AFTER.
- GateThreshold meets the accepted Lower Gate at its locked receiver; no Lower Gate changes.
- MainRoad meets the locked Stair approach; no stair geometry is added, hidden or moved.
- UpperRoad prepares the continuation after Stair but does not manufacture a stair transition.
- Ground/Terrain are untouched; any visible future terrain seam belongs to Terrain/Ground family review, not Road scope.

### 5. Legacy review
`StreetLandingTransition.glb` is **C — REFERENCE ONLY** in `asset-families.json`; no as-is fit is approved. The reset specifically routes Road as **surface mesh + tile/decals** with a planning LOD0 budget of **3,000 tris**. Geometry gap is proven for target-specific exact-footprint Road; no legacy geometry is authorized.

### 6. Mobile / production constraints
Planning LOD0 budget: **≤3,000 triangles**. Source must carry UV, normals and tangents. Paving joints must stay coarse enough to survive mobile landscape and portrait. LOD1/LOD2 are deferred until matched-camera screen-space review; no device FPS claim is made by this isolated family proof.

## Gates

1. **TECH PASS** — reproducible editable source/export, ≤3k tris, valid UV/normals/tangents, zero colliders, zero Tripo.
2. **ART SOURCE PASS** — clay/lit/axis views/approved-camera proxy/wireframe directly show a real circulation surface, readable hierarchy and no microdetail failure.
3. **UNITY INTEGRATION PASS** — exact reviewed GLB, scale1, replace only the four Road placeholders, retain accepted Bridge + Lower Gate unchanged in both phases, focused gameplay PASS.
4. **MATCHED-CAMERA VISUAL PASS** — exact camera A/B in all official views; protected projection invariant; Road retains target/blockout route weight and continuity through Bridge → Lower Gate → Road → locked Stair → Bastion.
5. **PRODUCTION FAMILY PASS** only after all previous gates pass.

Current state: **FAMILY SPEC LOCKED / SOURCE AUTHORING NEXT**. Tripo: **0 credits**.
