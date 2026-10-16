# Valoria — final terrain-reuse gate: native Unity V21 and V22, 16 October 2026

**Decision: STOP selecting existing little prefabs as a substitute for the owner-reference geological landmass. Build a new coherent authored terrain base in Blender.**

Source of truth: owner reference image 1513×1039, SHA-256 `de476e0516f744f9cd899419404971fa5e8d42bd49f2f7db7670d1cb325f8a4f`.
Zero paid credits. Pilot isolated from runtime game/release. No `ART_APPROVED`.

## Real native Unity checks

### Four original terrain prefabs — V21
- Windows Unity run [38866725171](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38866725171), genuine 1536x1024.
- [43-terrain-backbone-oblique.png](backbone-donors/43-terrain-backbone-oblique.png) and [44-terrain-backbone-high.png](backbone-donors/44-terrain-backbone-high.png).
- Assets: `SM_Cliffs_01.prefab`, `SM_Cliffs_03.prefab`, `SM_Terrain_03.prefab`, `SM_Hills_01.prefab` from `Unity/Assets/Eldoria/Resources/Valoria`.
- All import with one mesh renderer each. Their native longest bounds are approximately 100 units; studio normalized to a matching comparison span.
- **Art REJECT for hero meseta:** bright lime-green / white simplified block surfaces, without the fractured stratified gray cliffs, grounded vegetation, weathered material detail and scale coherence of the reference. Could be background prototypes only after separate retexturing, not the hero's authored cliff base.

### Original Blender source candidates — V22
- Windows Unity run [38867193841](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38867193841).
- [45-authored-terrain-family-oblique.png](terrain-family/45-authored-terrain-family-oblique.png) and [46-authored-terrain-family-high.png](terrain-family/46-authored-terrain-family-high.png).
- A `art-source/valoria/production/rock-terrain-family-v1/RockTerrainFamilyV1.glb` (5 renderers; bounds longest ~92.4), shows simple rock/cap geometry with slabs, **not** a continuous sculpted landscape.
- B `art-source/valoria/production/premium-square-metre-v1/Valoria_PremiumSquareMetre_v1.glb` (34 renderers; native longest ~1.99), reveals many separate modular blocks/components in a kit, **not** a coherent standalone geological environment. Asset size ~7.3MB does not imply visual readiness.
- **Art REJECT for primary meseta.** Source assets remain untouched and may be used individually only after acceptance.

## Production decision grounded in 2026-10-16 evidence
- V9–V15 full-scene trials show structural gates and road but overall reference-grade art remains grossly insufficient.
- V16–V19 tests demonstrate some reusable small 3D elements, and show original glTF material issues: three imports render black or white, test-only URP stone materials reveal their mesh; none replaces the meseta.
- V20 injected four attractive `RockToWallTransition` pieces into a frozen V15 camera. Changes measured in under 2% of image; no meaningful improvement in large-scale landscape or architecture. Rejected.
- V21 and V22 rule out both high-level original terrain prefab families as ready-made replacements.
- Conclusion: further adding small assets creates busywork without addressing large continuous rock mass and exact reference silhouette.

## Exact next Blender deliverable (single geo asset only, not full scene)
**Name:** `Valoria_CanonicalPlateau_Geology_v1`. Author real editable geometry in Blender, export `.blend`, `.glb`, textured ground/rock material assets, vertex/poly/UV report, two native Blender or Unity captures; never approve based on exporter PASS.

Required semantic zones: (1) bridge landing and front cliff with an intentional gate-sized opening, (2) one continuous large, navigable upper rock plateau with a broad *empty* courtyard, (3) a raised rear terrace with a genuine stair contact, (4) irregular nonrepeating escarpment with layered rocky faces at least 3 different scales, (5) material transition at the cliff lip without discontinuities, (6) naturally worn grass/soil albedo and physically reasonable rock textures, (7) source-to-photo camera silhouette frozen from approved reference.
No artificial disconnected islands, no array of repeated rocky columns masquerading as cliffs, no skyline quad standing in for mountains. Prefer actual artist sculpting/photogrammetric CC0 geological detail integrated into a continuous underlying mesh with rights documented, not another naïve extruded procedural outline.

**Quality gate:** macro silhouette and contacts first, two camera A/B versus V15 and original owner art. If no clear improvement, reject geology v1; if visually promising, pursue material/sculpt refinements. Subjective grade 8/10 must be judged using *real* images and owner review. Preserve V15 as strongest architecture/blockout baseline and V20 as negative controlled test. Do not integrate new geology into the player scene or release.

All relevant GitHub Actions are owner-authorized isolated zero-budget work. Other artists, release tasks and Unity production remain untouched.
