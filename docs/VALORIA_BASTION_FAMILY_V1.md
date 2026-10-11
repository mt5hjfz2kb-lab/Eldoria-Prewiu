# VALORIA BASTION FAMILY v1

Date 2026-10-05. Workstream `valoria-bastion-family-v1`. Base main `e1093da38df1d01d7045a5e63f4f01b77bfebcd2`.

## FINAL RESULT — PRODUCTION FAMILY PASS / CLOSED

**Source08 is authoritative. HERO QUALITY BAR PASS. 0 Tripo credits.**

- Source01–05 were rejected during the isolated HERO source search; Source06 failed HERO/technical cleanliness; Source07 reached **ART SOURCE PASS** but failed the cumulative Unity HERO gate because its near-white hero masonry collapsed the upper fortress into a washed-out high-value band against the already accepted Valoria families.
- Source08 preserves Source07 geometry and all nine approved Bastion envelopes exactly, changing only the exported material/value hierarchy. The authoring pipeline was corrected so source-specific texture tints are actually embedded in the Unity-consumed GLB rather than merely existing in the spec.
- Authoritative source run: **37327905027**, artifact **11353460819**.
- Authoritative BLEND SHA: `5c7b7e0918ac2e7dc78984515991c3f3546dffc3927bd0fa23bcaf4e9b148893`.
- Authoritative GLB SHA: `3a731789c2454e27dd6e60346719b69189b7999d0ed6e5e535927c3d7163a477`.
- Source metrics: **22,690 tris / 11,727 source vertices / 9 modules / 9 materials / 0 nonmanifold edges**, UV/normals/tangents PASS.
- Cumulative Unity run: **37328467482**, artifact **11352609605** — SUCCESS. The earlier duplicate run **37328457083** was superseded/cancelled by the later identical Source08 trigger and is not authoritative.
- Unity import: **22,690 tris / 64,705 imported vertices / 9 renderers / 9 materials / 7 textures / 35 submesh draws**; colliders 0; production scene not opened or saved; focused gameplay tests PASS.
- All seven official capture poses were produced. Direct review of the authoritative 16:9 and mobile matched-camera frames passes the integrated HERO bar: the Bastion keeps the target-specific dominant Keep, blue/gold heraldry, adjacent command arch, controlled asymmetry, visible depth, scaffold/damage identity and stronger hierarchy than Lower Gate/Walls, while the warmer/lower-value Source08 stone removes Source07's pasted-white-band failure.
- Lower Gate, Bridge, Road, Stair, Rock/Terrain Source09 and Walls remain retained; camera, canonical target, global composition, platform and gameplay remain locked and unchanged.
- Final verdict: **ART SOURCE PASS / UNITY TECH PASS / MATCHED-CAMERA VISUAL PASS / HERO QUALITY BAR PASS / PRODUCTION FAMILY PASS / CLOSED**.
- No next family is opened by this closure.

## Locked context
Lower Gate, Bridge, Road, Stair, Rock/Terrain Source09 and Walls are closed **PRODUCTION FAMILY PASS** and immutable. Camera, canonical target, global composition, platform and gameplay are locked. This workstream authors **Bastion only**. Tripo is forbidden without a new explicit owner authorization.

## TARGET → BLOCKOUT
Canonical target: `docs/evidence/valoria-art-production-reset-v1/canonical-target.jpeg`. Target Bastion bbox in the canonical 1536×1024 frame is **[608,64]→[1265,270]**, anchor **[952,217]**, approximately **8.605%** of frame area. Required silhouette: **long upper curtain + dominant single keep**, with military asymmetry and a clearly stronger hierarchy than Lower Gate or peripheral Walls.

Approved Bastion ownership is exactly:
- `BastionCurtainWest`
- `BastionCurtainEast`
- `BastionWestTower`
- `BastionEastTower`
- `BastionKeep`
- `BastionLeftWing`
- `BastionRightGatePillar`
- `BastionGateLintel`
- `BastionFarWestBlock`

Each module must remain inside its approved reset blockout envelope. No camera or world compensation is permitted.

## FAMILY SPEC — HERO
### Primary forms
1. **BastionKeep** is the unique dominant vertical mass, not one tower among equals.
2. West/East curtains form a long horizontal defensive datum behind the upper terrace.
3. The gate/lintel creates the principal inhabited break and ties the stair axis into the fortress.

### Secondary forms
- East tower is deliberately taller/heavier than West tower.
- FarWestBlock acts as an asymmetric rear lantern/defensive volume.
- LeftWing is a broad supporting mass rather than a cloned keep.
- RightGatePillar is a narrow vertical counterpoint around the gate.

### Tertiary forms
- Structural foundations, belts/cornices, sparse buttresses, deep military recesses, restrained cap courses and grouped crenellation.
- Blue/gold heraldry belongs to the central keep only, reinforcing identity and focal hierarchy.
- No random damage scatter. Damage/weathering may only be added when it changes a visible structural edge or plane.

### Material hierarchy
Warm Valoria military masonry; darker structural bases/recesses; lighter cut-stone caps; blue/gold heraldry. Bastion may share family DNA with Lower Gate/Walls but must be richer in form hierarchy, silhouette and value structure. Road and Rock/Terrain remain visually distinct.

### Authoring
Mandatory `BLENDER_PROFESSIONAL_V1`. Explicit editable meshes, real volume, selective bevel, boolean recesses, metric UV and deterministic GLB. Primitive-like shapes may exist only as authored intermediate solids inside a richer semantic composition; a visible stack-of-boxes result is automatic ART SOURCE FAIL.

Planning LOD0 ceiling: **≤45k source triangles** for the nine-module hero family. Triangle count is not a quality metric.

## ART SOURCE GATE — HERO
Before Unity answer all twelve:
1. deliberate silhouette?
2. immediately dominant Valoria Bastion?
3. clear primary→secondary→tertiary hierarchy?
4. real architectural depth?
5. controlled target-consistent asymmetry?
6. towers/volumes avoid cloned modules?
7. rich material without microdetail dependence?
8. coherent with accepted Lower Gate + Walls?
9. works from real gameplay camera?
10. authored rather than procedural?
11. enough identity to avoid castle-kit genericity?
12. obvious full-frame improvement?

**1, 2, 9, 11 or 12 FAIL = ART SOURCE FAIL.** No Unity integration until the source passes visually.

## UNITY / MATCHED-CAMERA / HERO QUALITY
Cumulative BEFORE/AFTER retains Lower Gate + Bridge + Road + Stair + Rock/Terrain Source09 + Walls identically; only the nine Bastion placeholders change. All seven official cameras are mandatory, protected families must remain at **0 px**, gameplay/colliders/import sanity must pass, and the final review must explicitly compare **TARGET vs APPROVED BLOCKOUT vs STRUCTURAL FRAME WITH FINAL BASTION**.

The final gate is visual. It must answer whether the integrated Bastion is already a commercial-game hero asset, has its own personality/richness, holds at gameplay distance, makes the frame read less like improved greybox, unifies Walls+Terrain+Bastion, and genuinely approaches the premium target. Green CI, hashes, polygon/material count and technical cleanliness grant no visual credit.

Current state: **FAMILY SPEC LOCKED / PLANNER NEXT / 0 TRIPO**.
