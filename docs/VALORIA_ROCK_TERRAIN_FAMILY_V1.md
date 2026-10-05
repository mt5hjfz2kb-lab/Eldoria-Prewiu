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

Current status: **PRODUCTION FAMILY PASS / CLOSED / SOURCE09 AUTHORITATIVE / 0 TRIPO**.


## Result

**PRODUCTION FAMILY PASS / CLOSED.**

Authoritative source is **Source08**, run **37308344056**, artifact **11344638101**. Editable source SHA-256 `559c32476c4df3f590ab9a5377f449aa44ec4d2956f2d18204d5716a0b0ad64b`; GLB SHA-256 `f9e66881cfcbf61d135ec1a8ddf13027e6dac06228dfaef745f13fa2450b02ed`.

Source08 is **ART SOURCE PASS** after seven deliberately rejected/iterated attempts. Source01 failed for striped horizontal banding; Source02/03/04/05/06/07 successively removed procedural cadence, pillar reads and one-to-one shoulder proxy repetition. Source08 resolves the root issue by absorbing `CliffShoulder_0..6` into a continuous authored geological mass rather than reproducing them as separate production rocks.

Authoritative source metrics: **1,909 tris / 3,516 authored source vertices / 5 semantic modules / 7 materials**, UV/normals/tangents present, 0 Tripo credits. The five production modules are `ForegroundBank`, `MainPlatform`, `PlayableGround`, `UpperTerraceCliff`, `UpperTerraceGround`.

Cumulative Unity gate: run **37309039558**, job **111759642435**, runner **DESKTOP-R10PE55**, artifact **11344907445** — SUCCESS. Accepted Lower Gate + Bridge + Road + Stair remain identical in BEFORE/AFTER; only structural Rock/Terrain changes. The seven shoulder placeholders are suppressed as obsolete greybox proxies.

Unity import sanity: **1,909 triangles / 5,298 imported vertices / 5 renderers / 7 materials / 0 texture payload / 17 submesh draws**, mesh bytes **563,376**; UV/normals/tangents present; colliders **0**; production scene opened/saved **false**. Focused PlayMode **5/5 PASS**.

All seven official matched cameras pass. **63 protected non-terrain elements per view remain at exactly 0 px projection delta.** The Rock/Terrain family macro envelope remains effectively identical to the approved blockout in every view: width/height ratios are ~**1.0000000**, with a maximum family-center delta below **0.0001 px**.

### TARGET vs APPROVED BLOCKOUT vs STRUCTURAL FRAME WITH ROCK/TERRAIN

- **TARGET:** broad continuous two-level cliff platform, clear 4-unit rise, large/medium controlled rock facets, subdued earth/rock hierarchy supporting architecture.
- **APPROVED BLOCKOUT:** correct composition, levels and occupancy, but terrain reads as flat technical slabs/dark bands plus seven explicit shoulder proxies.
- **STRUCTURAL FRAME / SOURCE08:** same macro silhouette and anchors, but visible plateau/upper rise now read as continuous faceted geological masses with separate earth planes; shoulder proxy cadence is gone.

Conclusion: **REAL QUALITY JUMP PASS.** Rock/Terrain materially reduces the greybox character of the whole frame without moving accepted architecture, camera or composition. The frame is not yet final-target complete because Walls, Bastion, Cabin/Camp, Vegetation, Props, Water/Shore and Background remain intentionally outside this family.

Formal evidence:
- `docs/evidence/valoria-rock-terrain-family-v1/source08-review.json`
- `docs/evidence/valoria-rock-terrain-family-v1/source-review.json`
- `docs/evidence/valoria-rock-terrain-family-v1/production-sanity.json`
- `docs/evidence/valoria-rock-terrain-family-v1/matched-camera-metrics.json`
- `docs/evidence/valoria-rock-terrain-family-v1/target-blockout-structural-comparison.json`
- `docs/evidence/valoria-rock-terrain-family-v1/integrated-visual-review.json`
- `docs/evidence/valoria-rock-terrain-family-v1/checkpoint.json`

Camera, target, global composition, Bastion, Walls, BackgroundTerrain, accepted Lower Gate/Bridge/Road/Stair and gameplay remain unchanged. **Tripo: 0 credits.**

**Do not start Walls, Bastion or another family without a new owner instruction.**

## Source09 material correction — authoritative newer repo state

After the Source08 Unity A/B, a newer repository commit (`9eab04e64c983e0232065c6d4a942670d28cc9c0`) re-opened this family for a **material-only** correction. Source08 geometry remains locked; Source09 changes only the palette: darker warmer rock, compressed facet contrast and darker earth to avoid the integrated washed-out read. The previous Source08 closure is therefore superseded until Source09 passes ART SOURCE + cumulative Unity + matched-camera visual gates.


## Final result — Source09 authoritative

**PRODUCTION FAMILY PASS / CLOSED.**

The previous Source08 closure is superseded. Source08 proved the structural geometry and Unity alignment, but its pale/high-contrast material hierarchy failed the later strict integrated visual review due to washout / low-poly facet dominance. **Source09 is the authoritative production source.**

Source09 changes **materials only**; Source08 geometry, interfaces, 3/7/11 anchors, macro silhouette and placement remain locked unchanged.

Authoritative Source09:
- Source run **37310029846** — SUCCESS
- Source artifact **11345358292**
- Editable BLEND SHA-256 `309a4cabac72e16b6bc6b13d66b8649b675ef668ad3a1ab367a355de0525d4cd`
- GLB SHA-256 `e842a7c64dc71b66d00cf1fbe3d31101927313a0f73111e0875f53ed3248863f`
- **1,909 tris / 3,516 authored vertices / 5 modules / 7 materials**
- UV / normals / tangents PASS
- Tripo **0 credits**

Authoritative cumulative Unity gate:
- Run **37310582556** — SUCCESS
- Job **111764702509**
- Runner **DESKTOP-R10PE55**
- Artifact **11345546113**
- Import: **1,909 tris / 5,298 imported vertices / 5 renderers / 7 materials / 17 submesh draws**
- Colliders: **0**
- Focused PlayMode: **5/5 PASS**
- Production scene opened/saved: **false**

Matched-camera:
- All seven official cameras PASS.
- **63 protected non-terrain elements/view remain at exactly 0 px projection delta.**
- Rock/Terrain macro width/height remains within approximately **0.00002%** of approved blockout across all official views.
- Maximum family-center drift remains below **0.0001 px**.

Visual quality bar:
- Greybox reduction: PASS.
- Target rock rhythm: PASS.
- Plateau silhouette: PASS.
- Depth without noise: PASS.
- Authored / non-procedural read: PASS.
- Mobile landscape + portrait detail scale: PASS.
- Lower Gate / Bridge / Road / Stair integration: PASS with zero movement.
- Rock / earth / paving material separation: PASS.
- **REAL QUALITY JUMP: PASS.**

TARGET vs APPROVED BLOCKOUT vs STRUCTURAL FRAME WITH ROCK/TERRAIN:
- target demands a broad continuous two-level cliff platform with controlled medium/large rock facets and subdued warm terrain;
- blockout preserved composition but read as technical slabs plus explicit shoulder proxies;
- Source09 preserves the exact approved macro frame while replacing that read with one continuous authored geological mass and a darker, warmer, compressed material hierarchy that keeps architecture dominant.

The remaining distance to final target quality belongs to intentionally unopened families: Walls, Bastion, Cabin/Camp, Vegetation, Props, Water/Shore and Background. They are not part of this gate.

Formal authority:
- `docs/evidence/valoria-rock-terrain-family-v1/source-review.json`
- `docs/evidence/valoria-rock-terrain-family-v1/source09-review.json`
- `docs/evidence/valoria-rock-terrain-family-v1/production-sanity.json`
- `docs/evidence/valoria-rock-terrain-family-v1/matched-camera-metrics.json`
- `docs/evidence/valoria-rock-terrain-family-v1/target-blockout-structural-comparison.json`
- `docs/evidence/valoria-rock-terrain-family-v1/integrated-visual-review.json`
- `docs/evidence/valoria-rock-terrain-family-v1/checkpoint.json`

Camera, target, global composition, Bastion, Walls, BackgroundTerrain, BridgeSupport, Lower Gate, Bridge, Road, Stair and gameplay remain unchanged.

**Do not start Walls, Bastion or another family without explicit owner instruction.**
