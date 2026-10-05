# VALORIA BRIDGE FAMILY v1

Date 2026-10-05. Workstream: `valoria-bridge-family-v1`. Base main `c706b488c0b1c9ed0582b65ae3d504d8f43c9a58`. Lower Gate is closed **PRODUCTION FAMILY PASS** and immutable. This block replaces Bridge only; camera, target, global composition, Lower Gate, Bastion, Road, Stair, Terrain, gameplay, hotspots and colliders are protected.

## Authority and screen-space role

Canonical target: `docs/evidence/valoria-art-production-reset-v1/canonical-target.jpeg`, SHA-256 `8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689`. Approved blockout: `blockout-scene.json` / `unity-blockout-input.json`, certified by reset run **37275659322** / artifact **11329019907**. Official source camera remains orthographic yaw20/pitch35/span48/center0/distance110. Target Bridge annotation is bbox **[421,688,751,885]**, anchor **[589,787]**. No camera or composition compensation is permitted.

The approved Bridge family consists of exactly four placeholders: `BridgeDeck`, `BridgeParapetL`, `BridgeParapetR`, `BridgeSupport`. Primary target read is the diagonal foreground crossing that lands at the already-accepted Lower Gate threshold. The family must not move or resize Lower Gate to fit.

## Family specification

### A. Silhouette and interfaces
- Preserve the blockout deck trapezoid and its rising grade exactly: foreground top **3.20 m**, gate receiver **7.03 m**.
- Preserve both parapet outer footprints and terminal screen occupancy. Continuous walls may be internally lowered only if target-like terminal piers retain the approved outer bbox.
- Bridge entry remains broad and readable in foreground; gate end narrows naturally into Lower Gate.
- Preserve exact bridge→gate receiver and the existing threshold/road axis. No new threshold, road segment or terrain wedge.
- `BridgeSupport` remains inside its approved **2×2×4.6 m** envelope; it may become architectural support/arch language but may not widen into cliff/terrain scope.

### B. Visual language
Warm structural limestone and lighter cut-stone trim inherit the already-accepted Lower Gate palette and metric masonry/stone-grain response. The bridge should read as the same civic-defensive construction campaign, not a generic castle-kit insert. Target identity is restrained: stone crossing, low parapets, stronger square end posts and a readable masonry support; no lamps, banners, vegetation, rubble dressing, props or ornamental sculpture in this family.

### C. Source authoring
Method: **direct editable Blender mesh specification + generic professional authoring engine**, zero Tripo. Primary deck, parapet and support envelopes are explicit designed meshes derived from the approved blockout. Surface helpers may provide selective bevel, metric UV and shared tileable masonry normal/color. Pavers are shallow authored bands contained inside the approved outer silhouette. Support uses a two-leg opening plus stepped arch shoulders contained inside the original support envelope. No legacy geometry is imported.

### D. Legacy review
`GateStreetRiseRock_MV1.glb` remains **C — REFERENCE ONLY** per `asset-families.json`; as-is fit is unproven and reuse is not authorized. No A/B legacy candidate exists. Geometry gap is therefore proven for this target-specific bridge. No sunk-cost reuse.

### E. Performance and mobile
Planning LOD0 budget from preproduction: **≤12,000 triangles**. Two shared stone material roles are expected. Source must retain UVs/normals/tangents after Unity import. LOD1/LOD2 are future optimizations only after matched-camera silhouette review. Required visual cameras: source 3:2, 16:9, mobile landscape and portrait-entry; the full seven official captures remain the technical non-regression set. No device FPS claim from the isolated proof.

## Gates

1. **TECH PASS**: reproducible source/export, bounds, topology/material/UV evidence and zero paid credits.
2. **ART SOURCE PASS**: direct review of clay, lit 3/4, frontal/lateral, approved-camera proxy and wireframe. Must read as the target Bridge and as the same stone language as Lower Gate.
3. **UNITY INTEGRATION PASS**: reviewed GLB only, explicit scale1/pivot, replace only the four Bridge placeholders, no colliders, no production scene save, focused gameplay tests PASS.
4. **VISUAL PASS**: matched-camera TARGET vs APPROVED BLOCKOUT vs BRIDGE INTEGRATED. Must preserve screen occupancy and structural continuity into Lower Gate in landscape and portrait-entry.
5. **PRODUCTION FAMILY PASS** only if all four gates above pass. Failure stays inside Bridge; do not open Road/Stair/Terrain/other families.

## Result

**PRODUCTION FAMILY PASS / CLOSED.**

Source progression was deliberately gated rather than promoted on technical success alone. Source01 (run **37296116400**, artifact **11338688062**) was TECH PASS but **ART SOURCE FAIL** because its deck created an unintended zipper/chevron seam and its support read as a technical block assembly. Source02 (run **37296800850**, artifact **11340060969**) fixed the geometry but exposed a surface gap. Source03 (run **37297309856**, artifact **11339267765**) retained the accepted geometry and added metric civic paving; it is the authoritative **ART SOURCE PASS**. Reviewed source SHA-256: `801fd7d393c8c151c3036e793e62ff1aad88cd22b636598194b7c47e1ba2eed4`; GLB SHA-256: `92a2c8ef00621948aaef7f93ae364fb87428e4c54b05bdad62313f8de9942a9c`.

The cumulative Unity gate retained the already-approved Lower Gate identically in BEFORE and AFTER and replaced only Bridge. Authoritative run **37298002864**, job **111723747578**, runner **DESKTOP-R10PE55**, artifact **11339213616** — SUCCESS. Focused gameplay tests: **5/5 PASS**.

Matched-camera technical alignment passes in all seven official views. The Bridge replacement union is ~**99.891%** of approved blockout width and **99.632%** of height with only **0.108 px** center delta in source 3:2; maximum center delta over all official views is **0.119 px**. All **82 protected non-Bridge projected elements remain unchanged at 0 px**. Against the canonical target's BridgeDeck/BridgeParapet bbox, the integrated primary bridge is **99.988%** of target width and **104.915%** of target annotated height, with **11.42 px / 0.619% of frame diagonal** center error.

Direct integrated review is **VISUAL PASS**: the foreground crossing, stone paving, restrained parapets, terminal posts, small masonry arch support and accepted Lower Gate read as one coherent Valoria defensive-civic structure. Bridge→Lower Gate continuity survives 3:2, 16:9, mobile landscape and portrait-entry without moving Lower Gate, Road, Terrain or camera. Final cliff/vegetation/water/lighting belong to other families and were not altered or falsely claimed as Bridge completion.

Real Unity import: **2,592 triangles, 7,076 imported vertices, 4 renderers, 3 materials, 6 textures, 8 submesh draws**, mesh bytes **748,128**, texture bytes **16,782,576**; UV/normals/tangents present; colliders **0**; production scene opened/saved **false**. Camera, composition, platform, Bastion, Road, Terrain, Lower Gate and gameplay remain intact. Tripo credits: **0**.

Formal evidence: `docs/evidence/valoria-bridge-family-v1/source-review.json`, `production-sanity.json`, `matched-camera-metrics.json`, `integrated-visual-review.json`, and `checkpoint.json`.

**Do not start Road, Stair, Terrain or another family without a new owner instruction.**
