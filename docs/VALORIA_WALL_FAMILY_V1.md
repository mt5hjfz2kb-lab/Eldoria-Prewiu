# VALORIA WALL FAMILY v1

Date 2026-10-05. Workstream: `valoria-wall-family-v1`. Base main `56d3ccd7c8e9f32455a3c28b1068c6eb515412fd`.

Lower Gate, Bridge, Road, Stair and Rock/Terrain Source09 are closed **PRODUCTION FAMILY PASS** and immutable. Camera, target, global composition, platform, Bastion and gameplay remain locked. This block replaces **Wall only**.

## TARGET → BLOCKOUT authority

Canonical target: `docs/evidence/valoria-art-production-reset-v1/canonical-target.jpeg`, 1536×1024, SHA-256 `8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689`. Approved reset blockout: run **37275659322** / artifact **11329019907**. Official camera remains orthographic yaw20 / pitch35 / span48 / distance110 plus the six approved aspect/pan variants.

Wall ownership is exactly the approved peripheral defensive set:
- `WestPartialWall`
- `WestWallTowerA`
- `WestWallTowerB`
- `EastPartialWall`
- `EastWallTowerA`
- `EastWallTowerB`

These are peripheral wall/end-cap pieces, **not Bastion hero towers**. Their approved world envelopes, positions, heights and screen occupancy are immutable.

## FAMILY SPEC

### Defensive reading
- Walls close the defensive perimeter without competing with Lower Gate or Bastion.
- Main runs retain real thickness and are divided into wall body → parapet/cap → crenellation rhythm.
- Merlons are intentionally unequal in spacing/width across west/east runs; no visible uniform array.
- End/corner pieces use heavier plinth/body/cap hierarchy and intentionally varied merlon proportions.
- Buttresses are sparse and structural, placed where they visually seat the wall rather than as ornamental repetition.
- No random damage pass; no chips/noise used as design substitute.

### Interfaces
- West and east wall run endpoints remain exactly inside the approved blockout envelopes.
- Lower Gate, Rock/Terrain Source09 and all other accepted families remain untouched.
- Wall base/plinth shading is darker than wall faces so the family reads seated into the terrain rather than pasted on top.
- Wall material is warm Valoria masonry, distinct from darker Rock/Terrain and from Road paving.

### Hierarchy
Required hierarchy: **Bastion > Lower Gate > Walls**. Peripheral tower/end-cap silhouette may articulate the wall but may not become a hero skyline element.

### Technical
- New target-bound Blender authored source; legacy wall kits remain reference/classification only and are not imported as production.
- Mesh authoring uses explicit faces/solid thickness/selective bevel and grouped semantic modules; no paid generation.
- Planning LOD0 ceiling: **≤18,000 triangles** for the complete six-module family; source should target far below this.
- UVs, normals and tangents mandatory; deterministic GLB; source .blend reproducible; zero colliders; zero Tripo.

## ART SOURCE GATE — hard questions

All must be explicit PASS before Unity:
1. Does the wall look designed rather than assembled?
2. Does it have real depth and thickness?
3. Are visible repetitions controlled?
4. Does crenellation rhythm work at mobile scale?
5. Is the union with Lower Gate natural?
6. Does Rock/Terrain seating read structural rather than pasted?
7. Is **Bastion > Lower Gate > Walls** preserved?
8. Is the material inside the Valoria family while distinct from Road/Rock?
9. Does it clearly improve the frame over the approved blockout?

Any hard-stop failure blocks Unity integration.

## UNITY / MATCHED-CAMERA GATE

Integration is cumulative. BEFORE and AFTER must retain **Lower Gate + Bridge + Road + Stair + Rock/Terrain Source09 identically**. Only the six Wall placeholders above may change.

Seven official cameras are mandatory. Required evidence:
- protected closed families: **0 px displacement**;
- Wall envelope/silhouette preserved;
- clean Lower Gate and Rock/Terrain continuity;
- mobile readability;
- no clipping/seams;
- UV/normals/tangents/import sanity;
- **0 new colliders**;
- focused gameplay regression PASS;
- performance/import sanity;
- explicit **TARGET vs APPROVED BLOCKOUT vs STRUCTURAL FRAME WITH WALLS** comparison;
- explicit conclusion that Walls improve defensive reading without stealing Bastion hierarchy.

Current state: **FAMILY SPEC LOCKED / PLANNER NEXT / 0 TRIPO**.
