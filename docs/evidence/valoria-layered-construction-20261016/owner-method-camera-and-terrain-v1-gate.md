# Valoria — professional stage 2 and 3 gates, 17 October 2026

Scope: exclusive construction/valoria-master-plan-layers-20261016. This assessment follows the owner's order: camera/composition first, then terrain/traffic, then modular architecture. No canonical edits, paused branches or paid credits.

## Real Unity before/after inspected

- BEFORE: BridgeV1, GitHub Actions run 38977840458, bridge-v1/bridge-v1-zoom12.png and zoom19.png. Genuine continuous bridge, candidate LOCAL ART IMPROVEMENT, not canonical.
- AFTER: TerrainV1, GitHub Actions run 38979099838, terrain-v1/terrain-v1-home-12.png, home-19.png, portrait.png. Blender source has 16,164 triangles, Unity source import TECH PASS, bridge 5/5 and staircase 12/12 raycasts.
- Both use the historical VisualWorld orthographic QA camera; they are NOT actual runtime SHARP player camera captures.

## Direct visual verdict: TERRAIN V1 FULL SCENE ART FAIL

The grey mountainous band newly placed next to the staircase has visibly repeated sharply faceted sheets rather than a cohesive terrace. Riverbanks form a long near-rectangular trench, with abrupt cut-offs and a very flat far river plane. Huge expanses of uniform pale material dominate zoom 12 and 19. Rectangular reserved plot and upper platform slab edges remain prominent. The portrait crop misses much of the gate and bridge. The top Bastion remains a plain grey block, correctly GREYBOX, NOT finished architecture. The terrain has changed, but improvement of the overall environment and reference similarity is not demonstrated. Do not approve or promote TerrainV1.

Keep BridgeV1 as best local visual candidate and TerrainV1 only as reproducible EDITABLE EXPERIMENT. Neither qualifies as full scene 6/10.

## Two different camera authorities — cannot be interchanged

CANONICAL PLAYER: ValoriaParcelPresentation + SlicePresenter, SHARP visible beauty, PERSPECTIVE projection, home FOV 44.42281, camera HOME at identity. Source authority: docs/VALORIA_VISUAL_AUTHORITY_RESET_V1.md, production run 37474695980 and 45/45 semantic hotspot raycasts at historical SHARP-only projection.

LEGACY QA: VisualWorld / ValoriaMobileNavigableCityV1, ORTHOGRAPHIC position (18.2,14.6,-25.8), target (0,3.35,5.6), zoom values 9, 12, 19. Certified master-plan skeleton geometry belongs to this QA frame and does not automatically map to SHARP native 3D coordinates. The source manifest explicitly calls it LEGACY_QA_CERTIFIED_TOPOLOGY_NOT_CANONICAL_SHARP_GEOMETRY.

All previous Unity ABC/ABCD/BridgeV1/TerrainV1 screenshots are comparisons in the QA coordinate frame, NOT gameplay runtime approval.

## Gates and next authorized action

1. Stage 1: owner visual SHA-256 and 22 historical footprints = SOURCE PASS (topology reference only).
2. Stage 2: REAL PLAYER CAMERA WITH REAL MODEL/ANCHOR CORRESPONDENCE = NOT YET PASS. Before further art, recover a measured 3D-to-SHARP mapping for the same bridge, lower gate, road, stair, upper platform and west/east parcels; compare HOME, zoom/pan and mobile UI. No arbitrary scale transform or FOV hack.
3. Stage 3: original collider/traffic STRUCTURAL TECH PASS; physical stair and bridge were tested, but terrain ART FAIL. Correct visual mesh/river/stone support to match real geometry, then recapture whole frame from real player camera and QA.
4. Stage 4: current volumes GREYBOX, neither complete composition nor finalized Bastion.
5. Stage 5–6: hold expansion of upper fortification and further modular families until stages 2/3 are validated.
6. Stage 8/9: true PlayerState/LocalGateway/gameplay and mobile device profiling NOT certified by this isolated scene.

Preserve the existing good scene and all versions; correcting V1 is allowed but no silent artistic promotion. This is a recorded visual rejection, not a successful full-scene art phase.
