# VALORIA IN-CITY SURFACE & LIGHTING PASS v1

Status: CLOSED — TECH PASS / VISUAL PARTIAL / NOT PROMOTED  
Date: 2026-10-02

## Objective

Improve the promoted Valoria production frame from inside the compact city without touching World Frame v1, city footprint, gameplay topology, colliders/hotspots or adding geometry.

Planner route:
- profile: `environment_surface`
- planner run: **37044611044 — SUCCESS**
- `geometry_gap_proven=false`
- `allow_tripo=false`
- Tripo credits: **0**

## Iteration 1

Run: **37044505972 — SUCCESS**  
Artifact: **11243404622**

TECH PASS:
- matched 19/12/9/mobile;
- same scene BEFORE/AFTER;
- collider/hotspot signature unchanged;
- geometry added: false;
- World Frame mutated: false;
- Tripo credits: 0.

Surface scope:
- earth renderers touched: 2;
- stone renderers touched: 58;
- retaining renderers touched: 82;
- lights added: 4.

VISUAL FAIL:
- broad retaining-wall MaterialPropertyBlock tint washed pillars/retaining architecture toward white;
- stone/stair treatment became too uniform.

## Iteration 2

Run: **37045539351 — SUCCESS**  
Artifact: **11243349211**

TECH PASS:
- collider/hotspot signature unchanged;
- geometry unchanged;
- World Frame unchanged;
- active renderers: 806 -> 806;
- triangles: 1,659,618 -> 1,659,618;
- unique materials: 76 -> 70;
- lights: 25 -> 29;
- retaining renderers touched: **0**.

VISUAL PARTIAL:
- retaining washout fixed;
- main route and stairs gain a slightly warmer/coherent surface response;
- no obvious regression at 19/12/9/mobile;
- improvement is too small to justify production promotion or claim a benchmark step-change.

## Decision

Do **not** integrate `ValoriaInCitySurfacePassV1` into production.

The proof confirms that broad surface recolouring alone is now low-return. The next in-city gap should target the physical/visual seating of the central stair/landing/terraces using existing certified geometry before adding more dressing.

Recommended next proof:
- real placement of existing `TerraceStairRock` / `StreetLandingTransition` vocabulary around the central stair/landing;
- existing assets only;
- zero Tripo credits;
- preserve canonical Bastion stair circulation/colliders/hotspots;
- matched 19/12/9/mobile acceptance.
