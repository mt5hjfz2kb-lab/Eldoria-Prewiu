# Valoria IF1 — physical Gate → Street → Terrace decision

Date: 2026-09-28. Read `AGENTS.md`, `SESSION_HANDOFF.md`, art pipeline index, kit, v1 standard and audit, prior Micro-Valoria 2, and four dedicated Gate/Street/Terrace/Residential gates before touching source. Starting canonical `main`: `2e084f35767cdcb58c8637e8cf9e05e28ce918b1`.

## Scope and source identity

The only tested composition is a three-piece isolated control, GateStreetRiseRock MV1 → StreetLandingTransition → TerraceStairRock. Source optimized SHA-256 respectively `9d1a97ea385d557b77779eef7a7b65f8027bfc79a56b26081abc6b62f25a8302`, `ef367d9f0f671cd29e1b02e2d36a2dfdea3acd6e789087e4da5fb5e087e81b01`, `83fce93daeb5bb455ab89bb195c39f617b3ea69ff9bf29f938ae4744caff3e5e`. The isolated workflow stages these **exact certified GLBs** by artifact and hash. The originals remain untouched. No Tripo and no extra family.

**No IF1 geometry variant was created.** The prerequisite physical audit found that the required Gate outlet edit is not a small removable parapet or rock chip. Calling an unchanged copy `GateStreetRiseRock_IF1` would falsely suggest a physical fix. Attempting a blind cut through the upper architectural mass would violate the user's preservation rule. Street and Terrace were not modified after the Gate failed the mandatory first link; Residential phase was therefore deliberately not started.

## Physical audit before any edit

Source Y-up GLB dimensions and heights are in GLB units; the Unity short-chain scales longitudinal spans to Gate 15, Street 8.2 and Terrace 11.3 world units, yaw 180°. Gate scale factor is about 15.31 world units per source unit. Measurements use real certified mesh triangle/ray intersections. A near-horizontal upward-facing surface is only a *candidate* floor; a clear collision-free corridor also needs edge/headroom probes.

| Station | Floor candidate | Center headroom / obstruction | Sampled contiguous candidate strip within ±0.25 world units of Gate upper Y=.290 | Conclusion |
| --- | --- | --- | --- | --- |
| Gate source Z +.35, lower entry | Y≈.042 | Open at center at the sampled level | Lower width not limiting here | Entrance preserved |
| Gate Z −.25, proposed upper exit | Y≈.290 | No overhead hit at center in the 2.4-unit local clearance ray | X −.060…+.048 ≈ **1.65 world units** of candidate upward surface in a 0.002-source-unit grid | Marginally above the 1.6 minimum as surface *before* parapet/rock clearance; not certified clear width |
| Gate Z −.28, 0.46 world units farther | Y≈.315 at center | Other geometry intersects the upward ray only **.025 GLB ≈ .38 world units** above proposed floor | Largest contiguous candidate side patch ~**.58 world units** | Too narrow; headroom lost |
| Gate Z −.30, 0.77 world units farther | No central floor near Y=.290; center upward surface Y≈.523 | Vertical jump **.233 GLB ≈ 3.57 world units** from previous proposed floor; offset side strip at target height only ~.83 world units | X −.208…−.154 = **.83 world units** | Passage ends against rear architecture/rock; cannot join a 1.6-wide Street without rebuilding that upper region |
| Street +.35 lower / −.35 upper | Y≈.091 / .178 | Center has no sampled overhead at either end | Central candidate ~.21–.22 source X, about **1.72–1.84 world units** at 8.2 placement | Potential connector; full lateral clearance not certified |
| Terrace +.35 lower / −.35 upper | Y≈.028 / .272 | Center has no sampled overhead at either end | Lower strip approximately .22 source X at 11.3 placement, about **2.6 world units** | Potential receiving stair; full landing not certified |

The v1 scene placed Street's candidate inlet **0.25 world units beyond** the Gate socket and the Terrace inlet **0.20 beyond** the Street socket. These positions match socket centers vertically but leave an unsupported horizontal join. Moving Street closer can cover a short open gap, yet at Gate Z−.28 to −.30 the receiving corridor meets elevated geometry and loses its floor. The same source includes a separate large upper architectural region; a wide exit cut through that region or a 1.6-wide added bridge/support would alter its form substantially. Cutting just the protruding small ornament at Z−.30 exposes a hole rather than a floor. A cut through rock at a side edge is not proven to provide a continuous supported alternative.

The center probes establish a **failure**, not a certified complete map of all possible paths. A later authored redesign could test a lateral exit, but it would need its own measured landing, turn, rock support and clearance. It cannot be claimed from current socket alignment.

| Certified family | Original → post-audit triangles | Original → post-audit local bounds X×Y×Z | Exact geometry edit | IF1 variant |
| --- | ---: | --- | --- | --- |
| GateStreetRiseRock MV1 | 49,799 → 49,799 | .873×.553×.980 → identical | **None**; outlet requires removal/reconstruction of a larger architectural region or added support floor | None |
| StreetLandingTransition | 49,800 → 49,800 | .470×.300×.979 → identical | None; stopped at first Gate link | None |
| TerraceStairRock | 49,800 → 49,800 | .751×.469×.970 → identical | None; stopped at first Gate link | None |
| ResidentialTerraceRock | 49,800 → 49,800 | .983×.619×.978 → identical | None; conditional phase not entered | None |

No after-surgery clearance improvement is claimed. The original certified source geometries themselves constitute the no-cut control; all IF1 “after” columns remain unchanged because the permissible surgery ended at the safety boundary.

## Short-chain Unity control and visual review

[Final run 36395383244](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/36395383244), artifact **10957743662**, SUCCESS. It repeats the preliminary physical run 36394526658 and additionally asserts imported source materials and source bounds. `ValoriaInterfaceShortChainReview.unity` is isolated and contains only one instance of each of the exact three certified originals, neutral floor/light/camera, no Residential, filler or defense. The 3 certified source hashes matched. Unity 6000.3.23f1 measured **49,799 + 49,800 + 49,800 = 149,399 instanced triangles**, 56,282 / 65,314 / 55,063 imported vertices, three source materials present, three MeshColliders, UV0 and normals on all three, mesh selection hits on all three, four nonempty 1280×720 captures and four empty-background selection misses. Source bounds X×Y×Z from Unity: Gate **.87256×.55297×.97962**, Street **.47024×.30010×.97924**, Terrace **.75083×.46871×.96991**. The scene uses neutral URP clay to keep geometric joins visible. Bounds of the placed chain: approximately **13.36 × 10.23 × 27.37** Unity units. The two selected floor-center placements match numerically at <0.001 units. This is a *no-cut control*, not IF1 certification.

The actual Unity `metrics.json` shows Gate center floor errors from the nominated upper-plaza level: source Z−.22 **.084 m**, −.24 **.075 m**, −.25 **.001 m**, −.26 **.007 m**; at Z−.28 it abruptly becomes **1.113 m** and at −.30 and −.32 **3.566–3.567 m**. The first four probes have a 2.4-unit empty upward ray; the last three fail the floor tolerance. A raycast hit on elevated architecture is not a walkable floor.

**Actual four-view image review:**

- **19:** all three identities are legible, but the chain is a narrow sequence of distinct rocky masses, not a continuous supported route.
- **12:** Street begins against/beyond Gate's raised enclosed rear plaza. Its visual beginning is partly hidden by sculptural architecture; the ground/rock break remains.
- **9:** Gate's upper courtyard terminates at a decorated wall/rock cluster. The connector appears behind it, with no visible 1.6-wide exit or continuous walking surface. The stair itself remains recognizable.
- **Oblique:** the street and stair form a coherent pair on their side, but their rocky undersides stand apart from Gate's rock pedestal. Moving their sockets until the center points coincide did not fuse a geological base.

## Decision and certification

- Technical isolated scene: **PASS** for certified identity/import/UV0/normals/material presence/collider/raycast/captures. **No physical adaptation was claimed or technically gated.**
- Physical corridor interface: **FAIL** from Gate's measured outlet and the actual zoom-9 view.
- Visual chain: **FAIL**, actual 19/12/9/oblique reviewed; entrance→street→stair is not a continuous route.
- Gate, Street, Terrace and Residential keep their existing **FUNCTION CERTIFIED** classification; **zero INTERFACE CERTIFIED**.
- No `GateStreetRiseRock_IF1`, `StreetLandingTransition_IF1`, `TerraceStairRock_IF1` or `ResidentialTerraceRock_IF1` delivered. No physical geometry was changed; tris and bounds are **identical before/after** for each source. This is a deliberate stop under the rule against cuts that damage the asset's identity.
- A viable modular route needs an **authored passable end** to Gate's high plaza with a 1.6-unit continuous floor, 2.4-unit height and side rock shoulder designed to receive Street. The generator/design brief must separate architecture, road floor, parapets and sacrificial rock into editable semantic regions with measured connection sockets. This describes a change in asset authoring/part separation for future work, **not authorization to generate a seventh family or remodel now**.

`Valoria.unity`, `VisualWorld`, gameplay and web runtime remain untouched. Existing six-family Micro-Valoria 2 remains VISUAL/INTERFACE FAIL. Residential is out of scope until this first chain passes.
