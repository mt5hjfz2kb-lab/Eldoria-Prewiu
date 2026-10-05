# VALORIA STAIR FAMILY v1

Date 2026-10-05. Workstream: `valoria-stair-family-v1`. Base main `ba7468009c15491a17e8b75858c973236b8d9507`.

Lower Gate, Bridge and Road are closed **PRODUCTION FAMILY PASS** and immutable. This block replaces Stair only. Camera, target, global composition, platform, Bastion, Terrain, Walls, gameplay, hotspots and colliders remain protected.

## TARGET → BLOCKOUT authority

Canonical target: `docs/evidence/valoria-art-production-reset-v1/canonical-target.jpeg` (1536×1024, SHA-256 `8ae6fb0e...`). Approved reset blockout: run **37275659322** / artifact **11329019907**. Official camera remains orthographic yaw20 / pitch35 / span48 / distance110 plus the approved landscape and portrait variants.

Stair is the **4-unit level connection** between accepted MainRoad at the lower level and accepted UpperRoad / upper terrace at the upper level. The approved family is exactly `Stair_00..Stair_11`, `StairCheekLeft`, `StairCheekRight`.

## FAMILY SPEC

- Preserve every approved outer envelope, position, orientation, width, total rise and screen occupancy.
- Twelve broad stone step masses remain legible on mobile; no micro-treads or decorative nosing density.
- Two substantial masonry cheeks provide architectural section and prevent the stair reading as a texture/ramp. They remain inside the approved blockout envelopes and are not railings/walls from another family.
- Material language inherits accepted Road/Bridge/Lower Gate warm civic stone but keeps a darker cheek value so the whole axis does not collapse into one uniform tone.
- Lower and upper joins are solved inside Stair only. No Road, Ground, Terrain or Bastion geometry moves.
- No props, railings, terrain dressing, walls or gameplay geometry.
- Legacy `TerraceStairRock.glb` and `StreetLandingTransition.glb` remain classification C/reference-only; reuse is not authorized.
- Planning LOD0 budget: **≤5,000 triangles**. UV, normals and tangents mandatory. Zero colliders. Zero Tripo.

## Gates

1. **TECH PASS** — reproducible editable source/export, ≤5k tris, UV/normals/tangents, zero colliders, zero Tripo.
2. **ART SOURCE PASS** — isolated clay/lit/axis/approved-camera/wire views show real stair section, broad mobile-readable steps, restrained hierarchy and clean Road/upper interfaces.
3. **UNITY INTEGRATION PASS** — exact reviewed GLB at scale1; accepted Lower Gate + Bridge + Road retained identically in BEFORE/AFTER; only Stair placeholders replaced; focused gameplay PASS.
4. **MATCHED-CAMERA VISUAL PASS** — all seven official views preserve protected families at zero displacement and confirm Road → Stair → upper terrace/Bastion continuity.
5. **PRODUCTION FAMILY PASS** only after all previous gates pass.

Current status: **PRODUCTION FAMILY PASS / CLOSED / 0 TRIPO**.


## Result

**PRODUCTION FAMILY PASS / CLOSED.**

Planner run **37301748302** / artifact **11341384021** passed with zero paid/Tripo credits. Source01 is the authoritative **ART SOURCE PASS**: run **37301752932**, artifact **11340838935**, editable source SHA-256 `84b1e5f492a552c8452751fbe3d9872c827dc8f01a36cdddffeef73068c47a05`, GLB SHA-256 `442fe2d2796c0db01760bd8b13bcdf28b3e5aa214108b0095e484309f6950c88`. Source geometry is **1,360 tris / 712 authored vertices / 3 semantic modules / 2 materials** with UV/normals/tangents and 0 Tripo credits.

Cumulative Unity gate: run **37302198301**, job **111737317212**, runner **DESKTOP-R10PE55**, artifact **11340779541** — SUCCESS. Accepted Lower Gate + Bridge + Road are retained identically in BEFORE/AFTER and only the fourteen Stair placeholders are replaced by `StairSteps`, `StairCheekLeft`, `StairCheekRight`.

All seven official matched cameras pass. **72 protected elements/view remain at 0 px delta**. Integrated Stair occupies **99.787%** of approved blockout width and **99.699%** of height in the source 3:2 view; max center delta over all official views is **0.0575 px**. Direct visual review is **VISUAL PASS**: the white greybox stair is replaced by a warm civic-stone stair with broad readable tread rhythm and masonry cheeks, preserving the exact 4-unit connection and completing Road → Stair → UpperRoad/Bastion without moving Road, UpperRoad, Ground, Terrain or Bastion.

Real Unity import: **1,360 triangles / 3,756 imported vertices / 3 renderers / 2 materials / 4 textures / 3 submesh draws**, mesh bytes **397,896**, texture bytes **11,188,384**; UV/normals/tangents present; colliders **0**; production scene opened/saved **false**. Focused PlayMode **5/5 PASS**.

Camera, target, global composition, platform, Bastion, Terrain, Walls, Ground, Lower Gate, Bridge, Road and gameplay remain unchanged. Tripo: **0 credits**.

Formal evidence: `docs/evidence/valoria-stair-family-v1/{source-review,production-sanity,matched-camera-metrics,integrated-visual-review,checkpoint}.json`.

**Do not start Terrain, Walls, Bastion or another family without a new owner instruction.**
