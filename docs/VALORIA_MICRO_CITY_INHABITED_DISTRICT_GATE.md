# Micro-Valoria 2 — inhabited district, six-family isolated gate

Date: 2026-09-28. Baseline `main` HEAD at start: `8f8f3f2ff3785d1168d8a008221bfa6d54a79080`. Isolated implementation: `MicroValoria2Review.cs` and `micro-valoria-2-gate.yml`; the existing three-family scene/gate remains intact.

## Certified inputs and layout

The Windows runner staged six previously certified optimized GLBs and checked the exact SHA-256 of each before launching Unity. The only architectural/rock geometry is from these six families. No Tripo request or new family was created.

| Family | Instances | Tris each | Source of certified optimized GLB |
| --- | ---: | ---: | --- |
| TowerWallRock | 1 | 50,000 | Runner Downloads, SHA `18785f7ba607cef1e7dcee45684d65c3166dc47b0f73b4bc4fb3d295c6f53a6a` |
| TerraceStairRock | 2 | 49,800 | Certified run 36334704739, SHA `83fce93daeb5bb455ab89bb195c39f617b3ea69ff9bf29f938ae4744caff3e5e` |
| GateStreetRiseRock MV1 | 1 | 49,799 | Certified run 36346961014, SHA `9d1a97ea385d557b77779eef7a7b65f8027bfc79a56b26081abc6b62f25a8302` |
| ResidentialTerraceRock | 2 | 49,800 | Certified run 36353236508, SHA `2ca694f547b54989305328045b60d28b53da45da94cffbd63a27b9fc698dc458` |
| StreetLandingTransition | 1 | 49,800 | Certified run 36355160385, SHA `ef367d9f0f671cd29e1b02e2d36a2dfdea3acd6e789087e4da5fb5e087e81b01` |
| RockTerrainSeamFiller | 3 | 49,800 | Certified run 36356423039, SHA `2676e11fbfde6781996117b191a8a88b534b36ad6901409283bfdea22a51af6d` |

Ten instances total, **498,199 instanced triangles**. MV1 faces the certified 180° direction. The entrance leads into one short connector; the two residential masses flank the middle, two terraces provide stepped architecture, one tower accents the rear skyline, and three seam fillers sit partially buried beneath the foundations. A neutral ground plane, short approach strip, lighting and camera are context only. Unity saves only `Assets/Eldoria/ArtTests/ImageTo3D/MicroValoria2Review.unity` inside the runner artifact.

## Iterations and evidence

1. [First Unity run 36387613724](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/36387613724), artifact **10955108050**: the strategic silhouette was promising but lateral housing and rear tower separated from the core; the central route disappeared among parapets and rock. District bounds 27.70 × 11.28 × 41.07 Unity units.
2. [Revised Unity run 36388243909](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/36388243909), artifact **10955426116**, composition commit `bca4db9ec17f787df51cb6908a87b8c8f60a6e48`: moved both residences and tower inward, shrank the connector, overlapped and buried fillers more deeply, and reframed the city/detail cameras. Final measured bounds **22.01 × 10.86 × 37.04** Unity units. All four requested PNGs are in the artifact.

## Technical gate

**TECH PASS.** Unity 6000.3.23f1 imported one mesh/material for each family, with UV0 and normals present. Ten MeshColliders were present. An actual-mesh raycast hit succeeded for every family, and empty-space rays missed the district in all four views. Four 1280×720 captures at 19/12/9/oblique were nonempty. The combined source-mesh memory *without sharing* is 48,066,872 Editor-reported bytes; this is not a measured runtime mobile memory or frame-time budget. Material is neutral diagnostic clay and is not approved final art. The procedural road/ground are minimal viewing context, not additional architectural families. Production `Valoria.unity`, `VisualWorld`, gameplay and web runtime were not modified.

## Actual visual gate of the final pass

| View | Real observation |
| --- | --- |
| 19 | One vertical fortified/inhabited silhouette is present. The gate, roofs and upper tower form a coherent distant mass; it is still narrow and reads more as an elongated stronghold than a district spread across a mountainside. |
| 12 | The two residential roof groups introduce a convincing civil layer absent in the three-family version. However the central raised connector and battlements form a large uninterrupted parapet band. The eye cannot follow a clean entrance → landing → residential street → stair chain. The eastern residential rock still has a legible outer pedestal edge. |
| 9 | The gateway and a lower rise are visible, and houses have genuine facade volume. At the middle join, roofs, parapets and irregular rocks occlude the path. The upper terrace and residential access cannot be visually verified as one traversable route. The tight framing is a street/detail view rather than a whole-district view. |
| Oblique | The district has genuine depth and fewer detached masses than the first pass, but several rocky foundation shoulders remain readable as separate prefabs. The rear wall/tower still projects an isolated defensive end; the filler is not a sufficiently controlled common mountain surface. |

**VISUAL / URBAN FAIL for the requested inhabited, connected district.** The second pass clearly improves the first, yet it does not satisfy all of the required 19/12/9 circulation, continuous rock base and civil-first reading conditions. A green Unity job and nonempty PNGs are explicitly insufficient for visual acceptance. Do not promote this scene into production Valoria or claim that the six-piece kit is city-scale-ready.

## Comparison with three-family Micro-Valoria

The prior [three-family gate](VALORIA_MICRO_CITY_THREE_FAMILY_GATE.md) passed as a **small connected fortification**: 5 instances, 249,399 tris, bounds 25.34 × 11.74 × 35.51. The present test doubles the placed geometry to 10 instances and 498,199 tris, adds two civilian building groups, a short street/landing and buried seam support, and reduces the repeated tower count from two to one. It visibly gains inhabited roofs and a denser urban silhouette. It still fails the higher bar: a complete legible route through the new middle connector and a single continuous mountain under all families. It should not be interpreted as reversing the earlier, narrower three-family result.

## Exact failure and production decision

The missing capability is **a reliable shared interface**, not simply a seventh visual motif. Existing modules have fused ornamental parapets, stairs, buildings and large irregular rock pedestals with no agreed lower-road/upper-landing elevations, passable join widths, pivots/sockets or controlled mountain cut lines. Overlap hides some joins but can also occlude the road; the seam filler rounds edges without removing the incompatible pedestal shoulders. The tower/fortified parapets remain disproportionate relative to only two inhabited masses. Further tiny moves of the same ten fused assets are unlikely to prove a traversable neighborhood.

Keep the modular experiment and the certified six-family kit for isolated studies. **Do not scale this exact placement/GLB-interface language into full Valoria yet.** The next decision should define shared street/landing elevations, snap/cut regions and continuous rock-base boundaries, and then adjust the *existing* certified families or composition method against those rules before another city-scale build. Final surface materials, LODs, instancing and mobile performance remain separate ungated work. No new asset family was generated in this block.
