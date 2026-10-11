# Micro-Valoria: isolated two-family composition gate

Date: 2026-09-27. Unity 6000.3.23f1. Source commit `c1d51b536888d0da6a4117ebe983a439c8fa6d3e`. [Certified technical run and four captures](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/36338298285), artifact `eldoria-micro-valoria-c1d51b536888d0da6a4117ebe983a439c8fa6d3e` (ID 10937928530).

## Scope and sources

There are **two confirmed distinct module families**, not three. The earlier gate/wall/rock claim could not be matched to any GLB in the Windows runner's Downloads, and the V2/V3/V4/V5 TowerWallRock files are revisions of the first family. This experiment uses exactly:

| Family | Exact staged source | Instances | Imported tris per instance |
| --- | --- | ---: | ---: |
| TowerWallRock | `Eldoria_Module_TowerWallRock_50K.glb`, SHA-256 `18785f7ba607cef1e7dcee45684d65c3166dc47b0f73b4bc4fb3d295c6f53a6a` | 4 | 50,000 |
| TerraceStairRock | `Eldoria_Module_TerraceStairRock_50K.glb`, SHA-256 `83fce93daeb5bb455ab89bb195c39f617b3ea69ff9bf29f938ae4744caff3e5e` from the certified Module 3 run 36334704739 | 2 | 49,800 |

`MicroValoriaReview.Capture` constructs and saves only `Assets/Eldoria/ArtTests/ImageTo3D/MicroValoriaReview.unity` in the Windows runner workspace. The scene and four PNGs are preserved in the linked workflow artifact. The two GLBs are staged transiently with exact SHA checks and are not committed to the repo. Four rotations/placements of TowerWallRock frame an open southern threshold, western high shoulder and rear wall; two TerraceStairRock instances suggest a central climb and upper east platform. A neutral ground plane, short road, directional light and cameras are the only other visual elements. No Tripo generation, old castle asset or gameplay asset was used. `Valoria.unity` and `VisualWorld` were not touched.

## Technical gate

The real Unity run succeeded. Imported meshes: one per family, UV0 and normals present; 54,760 Unity vertices for TowerWallRock, 55,063 for TerraceStairRock. The six instances total **299,600 rendered instance triangles**. Each instance received a MeshCollider; raycast on actual module geometry passed for both families, and all four views missed the district when aimed at an empty corner. Four 1280×720 PNGs were confirmed non-flat/non-empty. The scene's measured renderer bounds were approximately **28.02 × 13.82 × 32.99** Unity units. The Editor reported source mesh memory of 4,706,200 and 4,720,800 bytes respectively; the sum for six copies without sharing would be 28,266,400 bytes, which is **not** an actual GPU/device or city-scale memory measurement. These full-mesh colliders are review probes, not production collision design. No draw-call, LOD, mobile frame-time or whole-city budget gate has been passed.

Two composition passes were rendered. The first passed technically but showed detached rock islands, an ambiguous entrance and strong cropping at 12/9. The second compacted the modules, overlapped their rocky bases and reframed the official zooms. Only the second pass is the final visual evidence.

## Actual visual review

| View | Observation |
| --- | --- |
| 19 (strategic) | A compact, stepped fortified silhouette appears. The collection can read as one small castle compound from a distance. Repeated tower shapes remain evident; there is no discernible street or urban quarter. |
| 12 (city) | Foreground walls and roofs merge into a dense white mass. The central rise is visible as height, but its route is not legible. No clear entrance arch, portal or walkable passage connects the approach to the inner terrace. |
| 9 (detail) | Individual openings, towers and crenellations survive geometrically. The intended stair is occluded by adjacent fused meshes and does not show a continuous lower-to-upper pedestrian/vehicle connection. Cropping at this zoom is intentional focus on the entrance, not full-scene framing. |
| Oblique | The rock-backed geometry has genuine depth and a better integrated overall silhouette than the first arrangement. Rocky pedestals and overlapping wall ends still reveal prefab boundaries, with gaps/occlusion at junctions. A ground-level approach remains a road terminating against fortified mass. |

The bright monochrome appearance is the actual neutral/untextured import of these geometry-gate sources; material/style continuity is **not** approved. Neither the technical success nor the compact silhouette proves a usable 4X city district. These are still one fused mesh each, without semantic parts, snap sockets, shared production materials or LODs.

## Decision and next asset

**Technical gate PASS; visual urban-composition gate FAIL.** Two families can make a coherent *small fortification silhouette* at 19, but they do not make a believable connected vertical district at 12/9. Simply adding more copies would compound repeated towers and rock pedestals without creating a legible traversable route. Do not scale this combination into Valoria.

The next distinct family should be an **open gate plus connective ascending street on rock**, rather than another tower or a complete castle. It must include a visibly passable ground-level arch/portal, short flanking curtain-wall ends without attached tall towers, a continuous road/stepped ramp that rises to a usable upper landing, and two deliberate rock/terrain junction edges that can meet the current terrace and wall foundations. Its path and upper landing must read at 19/12/9 from front and oblique; the opening must remain visibly clear rather than being filled by generated rock. Provide simple human-scale cues, useful pivots/snap points at lower road and upper landing, and preferably separate semantic gate, wall, stair/road and rock meshes. First validate this **single small module** through the same external optimization/UV/material/import/raycast gate; then repeat the isolated Micro-Valoria composition. This is a specification for a future module, **not** a request to spend Tripo credits now.
