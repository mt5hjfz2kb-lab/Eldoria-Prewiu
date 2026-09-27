# Micro-Valoria — three-family isolated composition gate

Date: 2026-09-27.

## Scope

This gate tests whether the three certified functional module families can form a coherent small Valoria district in an isolated Unity scene without touching production Valoria:

1. TowerWallRock
2. TerraceStairRock
3. GateStreetRiseRock MV1

The scene remains a geometry/composition study. It does not approve final materials, textures, LODs or mobile performance.

## Certified sources

- TowerWallRock: 50,000 tris
- TerraceStairRock: 49,800 tris
- GateStreetRiseRock MV1: 49,799 tris

GateStreetRiseRock MV1 is used at the orientation proven by the final canonical yaw-180 gate, where the functional face aligns with the official camera direction.

## Final isolated composition

Final review commit: `4c5ee31b781bd0966794bb1cd1f46bb96e4a060c`  
Unity run: **36348823965**  
Artifact: **10940749116**

The scene uses:
- 2 × TowerWallRock
- 2 × TerraceStairRock
- 1 × GateStreetRiseRock MV1

Technical totals:
- **249,399 instanced triangles**
- 5 MeshColliders
- UV0 and normals present on all three source families
- positive raycast on each family
- background miss PASS
- non-empty 19 / 12 / 9 / oblique captures
- district bounds: 25.343 × 11.742 × 35.513

The final review uses neutral diagnostic clay and lower-intensity lighting only to expose geometry and joins. No source GLB was altered.

## Real visual review

### 19 — strategic

The composition now reads as one elongated fortified rocky district rather than three unrelated pieces placed side by side. The southern gate provides a clear front, the middle terraces create elevation, and the two tower masses establish an upper skyline. The overall silhouette is recognizably vertical and urban-fortified.

Remaining weakness: the district is still narrow/linear and some rock transitions are visibly assembled rather than naturally continuous.

### 12 — city

This is the strongest evidence that the third family changes the composition qualitatively. The lower approach now feeds a readable monumental access, and the eye can continue into the elevated middle district rather than stopping at a rock pedestal. Tower repetition is reduced compared with the two-family experiment.

Remaining weakness: the transition from GateStreetRiseRock into the first terrace is visually dense and not yet a clean production-quality street junction.

### 9 — detail

The entrance, gate opening and internal rise remain readable. The middle district also shows terraces, walls and upper structures as connected architecture rather than isolated props.

Remaining weakness: seams between module rock bases and overlapping decorative detail are still visible at this distance. This is a kit-integration problem, not proof that the modular language itself fails.

### Oblique

The composition has genuine depth and a coherent low-to-high progression. The gate, rising middle and upper fortified silhouettes form a believable vertical sequence. It no longer reads primarily as repeated tower modules.

Remaining weakness: back/side massing is still heavier and more fortress-like than an inhabited city district, and the current three-family kit cannot yet provide enough street/building variety for a full Valoria neighborhood.

## Verdict

**Technical gate: PASS.**

**Three-family urban-language gate: PASS for the modular hypothesis.**

The three distinct functions now produce a readable small fortified Valoria district:
- defense / skyline;
- terrace / elevation;
- entrance / circulation.

This is the first composition that demonstrates that the modular Tripo → Blender → Unity route can create a coherent Valoria-like urban fragment rather than only isolated assets.

This is **not** a final-art or production-city PASS. Before scaling to full Valoria, the kit still needs:
- cleaner snap/overlap interfaces between rock bases;
- more non-tower architectural variety;
- smaller street/landing/filler modules;
- final material language and textures;
- LOD/instancing/mobile-performance budgets;
- a broader district layout so the city does not become a single linear fortified spine.

## Production decision

The modular route is validated strongly enough to continue.

Do not return to whole-castle generation and do not keep regenerating GateStreetRiseRock.

Next production work should expand the kit with small complementary pieces and consolidate the module pipeline/inventory before scaling into production Valoria.

`Valoria.unity`, `VisualWorld` and gameplay remain untouched.
