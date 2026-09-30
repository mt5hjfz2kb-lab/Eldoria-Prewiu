# Terrain & Terrace Kit v1 — final production gate (2026-09-30)

## Decision

**Technical kit: partial pass. West Rebuilders Quarter visual pilot: FAIL. No Terrain & Terrace instances remain in production Valoria.** The exact approved JPEG produced two salvageable isolated modules, but two sparse pilot placements did not deliver a credible mountain-built city at zoom 12/9. The first placement was largely occluded; the second made a detached-looking pedestal in front of housing. Keeping that placement would lower the quality bar. Do not represent CI success as visual acceptance.

## Exact source and paid generation

- Approved input: `pipeline/exact-inputs/TerrainTerraceKit_v1/`, 1448×1086 JPEG, 597,320 bytes, SHA-256 `47d4db0a40fec121f86c7c871bfd197314635976abcd87e4ad211eb098d79c71`. The stage preview reconstructed to that exact SHA.
- Stage run `36733759312` SUCCESS, artifact `11105714637`, no credit spend.
- One Tripo generation, run `36734206526` SUCCESS, artifact `11106786039`, task `9a34385e-db18-485e-b6ac-9b58ac953bc6`, **55 credits total**. Export run `36734554068` SUCCESS, artifact `11106142634`: 77,291,792-byte `TerrainTerraceKit_v1.glb`, SHA-256 `63d74d19455a92b0e134ada49bde41a3b050bdb8bfe19102eac632bf1d15264e`. Generation request is disabled; no second generation was attempted.
- Blender canonical XY-sheet correction run `36736179242` SUCCESS, artifact `11107497320`; bounded disconnected-residue cleanup run `36737671375` SUCCESS, artifact `11107953070`. Raw 1,958,300 triangles → 49,799 optimized; 7 isolated groups / 48,113 Unity triangles after cleanup. Base color and normal maps 2048², mask 1024²; UV0, normals, materials, collider raycasts and empty-space misses verified in the isolated Unity gate. The first XZ split `36734868497` was a technical run but visual failure and is not a production source.

## Strict piece review

| Group | Isolated verdict | Production GLB | Reason |
| --- | --- | --- | --- |
| 01, 6,379 tris | usable standalone | `SteppedRockTerrace.glb` | Compact stepped masonry/rock support; visible side and rear profile. |
| 02, 7,671 tris | reject | removed | Remote disconnected cliff fragment still visible in rear capture. |
| 03, 4,419 tris | reject | removed | Torn projecting deck lip in rear view. |
| 04, 7,127 tris | reject | not promoted | Fragmented retaining edge. |
| 05, 8,862 tris | reject | not promoted | Fused wall/rock silhouette with disconnected protrusions. |
| 06, 5,005 tris | reject | not promoted | Incomplete narrow connector profile. |
| 07, 8,650 tris | usable standalone | `BroadRockPlatform.glb` | Continuous broad deck and exposed rock/masonry face. |

The existing selective promotion workflow ran `36741557542` SUCCESS. A strict multi-angle review then rejected 02/03 and removed those two copied GLBs. Only 01/07 remain at `Unity/Assets/Eldoria/Resources/Valoria/TerrainTerraceKit_v1/`. The initial malformed promotion request `36741113209` failed identity verification before copying anything; this did not consume Tripo credits.

## West Rebuilders Quarter pilot and rejection

Two visual-only instances were tried; final production count is **zero**:

| Candidate | Trial anchor / span / yaw | Intended problem | Actual review |
| --- | --- | --- | --- |
| SteppedRockTerrace | `(-18.0, .22, 2.0)`, 4.30, 94° | Upper homes retaining edge / rock-to-architecture base | Becomes a conspicuous isolated plinth in front of the ornate house; not a credible terrain transition. |
| BroadRockPlatform | `(-16.4, .12, -4.5)`, 5.00, 100° | Lower homes platform / terrace edge | Mostly occluded or not readable as a distinct support at 12/9. |

First same-scene before/after gate `36741899930` SUCCESS, artifact `11109889097`; first trial was essentially invisible. Revised same-scene gate `36742519478` SUCCESS, artifact `11111064562` contains official overview and west-focus before/after at **19, 12, 9 and mobile**, plus `terrain-terrace-evidence.json`. Its camera and scene are identical within each pair; collision/hotspot signatures matched, and all trial colliders were disabled. That technical PASS does not override the visual FAIL. The after-9 west image shows the isolated foreground pedestal particularly clearly. The final city deliberately retains the before state.

The revision also passed full Unity slice `36742519517` (EditMode, PlayMode, desktop build, benchmark) and World Map Visual Formula `36742519509`; both SUCCESS. The post-rejection cleanup removes the runtime integration and pilot capture code, preserving the independently tested source modules for a future composition only. It touches no Valoria scene, `VisualWorld`, roads, stairs, routes, lots, interactive buildings, hotspots or gameplay. The original certified topology remains authoritative. Do not reintroduce these two trial placements as a shortcut.

**Verdict for the requested visual block: FAIL.** No claim of a mountain-city leap is supported by the zoom-12/9 evidence. The isolated two-piece asset inventory is retained for possible future use, but this pilot is closed without production instances.
