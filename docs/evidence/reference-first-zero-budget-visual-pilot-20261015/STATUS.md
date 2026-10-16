# Valoria reference-first 0 EUR graphical pilot — CURRENT 2026-10-16

**ART_APPROVED=false.** This pilot is a technical test harness + unapproved lookdev. It has NOT produced 8/10 art, does NOT alter the user-facing game, and has NOT been published.

## Canonical owner reference
1513x1039 JPEG, SHA256 `de476e0516f744f9cd899419404971fa5e8d42bd49f2f7db7670d1cb325f8a4f`. Two fortifications/doors, central road and intermediate stair, one connected cliff-top plateau, sparse side walls, trees around perimeter, blue banners and amber torches, stylized well-finished stone.

## Completed authentic native Unity evidence (all 1536x1024)

- Architectural blockouts V1–V15: reference-first source exists, real run #38860369047 (V15 final baseline). No art approval; plateau, skyline, grass, cliff and detailing visibly below owner art.
- Detailed donor comparison: #38861107452, originals BroadRockPlatform and SteppedRockTerrace. Modest small pieces; not whole terrain.
- Four transition donor GLBs original import #38864541121. A near black, B/C blown white, D RockToWallTransition textured. #38864994482 neutral diagnostic reveals intact geometry; all use Shader Graphs/glTF-pbrMetallicRoughness. #38865388028 stone material trial; source GLBs unchanged. #38865787439 high-resolution close-up of A and D; only D coherent as a standalone donor, neither approved.
- Real scene V20 #38866182984: exactly 4 D junctions injected into identical V15 camera, native images 41/42. **REJECT V20 visually**: changed pixels ~1.82% at channel intensity>2, 0.90% at >8, without meaningful repair of the landscape, and creates mixed gray/beige stone. Preserve V15, do not integrate V20.
- Nature base family V21 #38866725171: SM_Cliffs_01, SM_Cliffs_03, SM_Terrain_03, SM_Hills_01. Native images 43/44 show neon lime/white stylized simple terrain squares. **REJECT as canonical cliff.**
- Original Blender geology family studio V22 #38867193841: RockTerrainFamilyV1.glb and Valoria_PremiumSquareMetre_v1.glb, native 45/46. First is simple slab/outline, second an exploded kit of separate primitives, **not coherent ready-made terrain; REJECT.** Manifest reports 5 renderers and 34 renderers respectively. The Windows render AND Ubuntu proof archive both completed successfully.

## Rigorous conclusion
Unity pipeline, Blender GLB staging, fixed-camera capture and GitHub archive **work**. High-quality continuous 3D plateau art **does not yet exist** as a readily reusable candidate. 8/10 has not been demonstrated. STOP blind collage/terrain-prefab substitutions.

**Next task: design and author ONE actual, continuous `Valoria_CanonicalPlateau_Geology_v1` in Blender with real sculpted cliffs and physically believable UV/PBR, compared as an isolated single asset to the reference BEFORE another castle composition.** Do not repeat naive extruded low-poly terrain/sky planes; the main repository already contains historic failures `tools/blender_lower_city_cliff_v1.py`, and the 63-asset art branch has a genuine CC0 scanned cliff candidate but it is still UNAPPROVED. Reuse validated rights/provenance only after the independent quality gate. See full contract: [V21_V22_TERRAIN_SOURCE_DECISION_20261016.md](V21_V22_TERRAIN_SOURCE_DECISION_20261016.md).

## Safeguards
- Isolated branch `prototype/reference-first-zero-budget-visual-pilot-20261015`.
- Actual source manifests and original PNGs in this evidence folder; no invented screenshots.
- Preserve main/production and published player scenes, global pause, 63 individual asset approval branch and running independent workstreams.
- No paid service; Blender/Unity installed on owner PC reachable through authorized self-hosted Actions.
- Technically successful workflow does not imply art approval. Independent eyes and owner review required for 8/10.
