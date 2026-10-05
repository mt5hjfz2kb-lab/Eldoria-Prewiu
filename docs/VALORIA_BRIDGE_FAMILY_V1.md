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

Current state: **FAMILY SPEC LOCKED / SOURCE AUTHORING NEXT**. Tripo credits: **0**.
