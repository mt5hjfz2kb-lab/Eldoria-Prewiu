# GateStreetRiseRock: isolated module gate

Date: 2026-09-27. Source in Windows runner Downloads: `Eldoria_Module_GateStreetRiseRock.glb`, 7,966,260 bytes, SHA-256 `345e483c961a35738eb34fdeeca217cb79ffc7324d8989e05015322cfac8e458`. Exact name and checksum were verified before staging; neither TowerWallRock revision nor an old bastion was substituted.

[Original automated Blender and Unity run](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/36340507171) and [diagnostic Unity run with four requested captures plus frontal inspection](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/36340990104), artifact ID `10939280373`. The workflows stage the source from Downloads by exact name/checksum. Source and optimized GLB are transient workflow artifacts, not committed Unity assets. The saved test scene is `Assets/Eldoria/ArtTests/ImageTo3D/TripoGateStreetModuleReview.unity` inside the artifact only. No Tripo credits were used.

## Measurements and technical gate

| Stage | Blender vertices | Triangles | UV | Normals | Materials / images | Size |
| --- | ---: | ---: | --- | --- | --- | ---: |
| Raw GLB | 165,833 | 332,091 | absent | present | 1 / 0 | 7,966,260 bytes |
| Automatically optimized GLB | 24,686 | 49,799 | present | present | 1 / 0 | 2,155,384 bytes |

The optimized `Eldoria_Module_GateStreetRiseRock_50K.glb` has SHA-256 `8478b21aafe8202b07bdc2fe2d182174f705c1e3a17b4555862dfbff0a713257`. Its dimensions stayed close to the original; Blender bounds changed from 0.717804 × 0.981659 × 0.464584 to approximately 0.718098 × 0.981712 × 0.464674. UV coordinates were generated automatically because the original had none. This is a single fused mesh with one untextured neutral material, not approved final art.

Unity 6000.3.23f1 imported 1 mesh, 1 renderer, 1 material, 0 textures, 57,981 imported vertices and exactly 49,799 triangles. UV0 and normals passed. One MeshCollider, direct hit on the mesh and background raycast miss passed. Reported mesh runtime memory was 4,907,528 bytes; this is an Editor mesh figure, not a mobile budget. Four 1280×720 captures at zoom 19/12/9/oblique were nonempty. A fifth frontal diagnostic capture and neutral gray diagnostic material clarified the silhouette. **Technical gate PASS.**

## Actual visual review

| View | Observation |
| --- | --- |
| 19 | A compact fortified rocky mass with battlements; the entrance and ascending route are not legible at strategic distance. |
| 12 | A large elevated rectangular court dominates. The ground approach goes toward the front arch, but a route to the upper level cannot be followed visually. |
| 9 | The frontal arch is visibly open. It carries an upper deck overhead; the lower road continues beneath it without a visible ramp or stairs onto the upper deck. |
| Oblique | The same upper slab and a rear arched bridge read as a separate elevated structure. Rock flanks frame the gate, but no continuous lower-to-upper street is evident. |
| Frontal diagnostic | The opening is visible above the road, though parapets partly mask it; it does not establish a rising route. |

**Functional visual gate FAIL.** The generated geometry supplies a believable open gate and rocky fortification, but does not establish the specified ascending street through the gate to an upper landing. A generic mesh raycast proves selection/collision on some geometry, not passage or connection. The neutral material and missing textures also prevent approval of finished style. This is a visual judgment from rendered views, not a numerical proof that every possible traversal ray is blocked.

The condition for inserting this family in a new Micro-Valoria iteration is unmet. The two-family scene and production world remain untouched. Do not claim a three-family urban-composition pass from this geometry.

## Required corrected third family

Regenerate or remodel a **small connective GateStreetRiseRock**, with a clear lower road entering an open ground-level portal, then an exposed ascending ramp or broad stair with a visible grade, ending in an accessible upper landing at a compatible height with TerraceStairRock. Keep rocks and short wall ends outside the walkable strip. Expose the entire lower-to-upper route in front and oblique views at 19/12/9, and provide distinct lower/upper join edges or markers. Maintain the scale of one modular junction rather than a self-contained castle court. Export with material/UV intent where possible. Once this revised source exists, rerun the isolated gate, then compose Micro-Valoria with all three actual families.
