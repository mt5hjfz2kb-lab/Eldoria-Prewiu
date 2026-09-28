# VALORIA MODULE INTERFACE STANDARD v1 — measured draft before mesh edits

Date: 2026-09-28. Baseline main `008318cc8d1564ce593cc022c19e6822cbc253d4`. Six exact certified GLBs were inspected, including TowerWallRock retrieved from runner Downloads with SHA `18785f7ba607cef1e7dcee45684d65c3166dc47b0f73b4bc4fb3d295c6f53a6a` in [audit run 36389811661](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/36389811661). The other five are the optimized certified artifacts cited in `VALORIA_MICRO_CITY_INHABITED_DISTRICT_GATE.md`. This is a **design contract**, not an interface certification.

## Measurement convention

Original GLB axes are Y-up, Z longitudinal. Local bounds below are measured on the real optimized geometry in source units (the composition scales these by about 8–16). A candidate floor height is the center of upward-facing triangles in a narrow center strip near the respective Z end. Roof, parapet and rock surfaces may also be upward-facing: a floor candidate is **not** certified passable until clearance and a connected route are checked visually and by collision probes. All numeric Unity clearances below apply **after instance scaling**. `+Z` and `-Z` below describe source geometry before the certified MV1 180° yaw.

| Family | Bounds X × Y × Z | Up-face lower candidate at +Z | Up-face higher candidate at -Z | Functionally exposed end |
| --- | --- | --- | --- | --- |
| GateStreetRiseRock MV1 | 0.873 × 0.553 × 0.980 | 0.025–0.057 | mixed 0.103–0.329; roofs up to 0.53 | lower entry / inner exit, but upper landing height ambiguous |
| StreetLandingTransition | 0.470 × 0.300 × 0.979 | 0.091–0.093 | 0.153–0.178 | short lower and upper street ends |
| ResidentialTerraceRock | 0.983 × 0.619 × 0.978 | near 0.104–0.117 | mixed 0.115–0.323 | side frontage/landing, not a verified through street |
| TerraceStairRock | 0.751 × 0.469 × 0.970 | 0.014–0.033 | 0.183–0.277 | lower stair and upper terrace |
| RockTerrainSeamFiller | 0.962 × 0.453 × 0.980 | 0.116–0.138 | 0.311–0.313 | terrain edges only; no certified street |
| TowerWallRock | 0.921 × 1.000 × 0.799 | near 0.109–0.115 | mixed 0.167–0.189 | defensive rock/wall join, no street socket |

The street's narrow central walking-surface samples at its Z ends span about 0.21–0.22 GLB units in X. At its prior 8.2-unit longitudinal composition scale, this is approximately **1.72–1.84 Unity units** of *sampled horizontal surface*, not verified obstacle-free width. Street elevation changes by about 0.085 GLB units, or ~0.70 at that scale. The terrace spans ~0.24 GLB units of vertical candidate difference, ~2.7 at its prior 10.8–11.7 scale. These measurements motivate the tiers below; they do not prove that the gate's apparent elevated roof is a connected road.

## Street and landing contract

- Preferred clear street width **1.8 Unity units**; hard minimum **1.6**. These are measured against the sampled StreetLandingTransition floor width, so a placed connector smaller than ~8 units longitudinal scale cannot be assumed to meet the minimum.
- Keep a **0.20** lateral buffer clear of rock, railing and parapet on each side of the designated walk strip. Gate and building entrances need **2.4** clear vertical units above the walking surface. If collision samples cannot show this, the socket fails.
- Each join requires a **1.6 × 1.6** unobstructed landing after scaling; preferred 2.0 × 2.0 where the receiving mesh permits it. Joined walk surfaces may differ by no more than **0.25** vertically, **0.35** laterally; walk direction must deviate by no more than **15°** without an explicit turn/landing.
- Short street slopes should not exceed **1:5**; stairs may be steeper if stair treads are visible and the next landing is identifiable at zoom 9.
- A rock/parapet throat must not occlude the through-route from official front and oblique cameras. A collider/raycast hit alone is insufficient.

## Elevation tiers and pivots

Use the sampled connector change (~0.7–1.0 world units) as **L1 = L0 + 0.9 ± 0.25** and the sampled terrace rise (~2.7–3.0) as **L2 = L0 + 2.8 ± 0.35** for a typical 10–12 unit module scale. **L0** is the *measured center of the source's entrance floor after placement*, not the bottom of its rock bounds. Gate's apparent upper surface remains **unassigned** until an actual clear walkable exit is demonstrated. Any instance scale must recalculate its own socket world height; do not blindly place all bases at Y=0.

The logical pivot for traversable modules is `Street_In` at the lower walk center, facing into the module along the flow; `Street_Out` is the exit walk center. Unity's scene front is **-Z**, so a source whose lower end is +Z (MV1 and the measured street/terrace floor candidates) needs **yaw 180°** for a south-to-north ascent. `Terrace_L1`/`Terrace_L2` mark actual usable landings, not a roof or rock crest. `RockOverlap_Left`/`RockOverlap_Right` mark sacrificial outer rock shoulders, never the walk strip. A defensive TowerWallRock gets only rock/wall edge sockets unless a real passable opening is shown.

## Rock integration

- Designate a **0.5–1.2** world-unit overlap band between two existing rock shoulders; no filler may be presented as a standalone island. At least **0.5** of contiguous integration edge should remain visible at a join.
- Filler burial may hide up to **45% of its scaled height** (the previous composition buried roughly 1.7–2.0 units of a ~3.8-unit filler); bury only its geological base, never a certified usable landing.
- Blend toward ground at approximately **1:4** or gentler where a walkable terrain shoulder is intended. An irregular natural cliff may remain steeper away from circulation. The full-width flat/pedestal termination of the source GLB must be buried, trimmed or overlapped; it cannot remain a clearly exposed rectangle in 19/12/9/oblique.
- Changes may remove only local obstruction and sacrificial interface geometry. Preserve primary architecture, facade, roofline, rock silhouette and original material/UV intent. No replacement cube or rectangular wall may be used to disguise a failed join.

## Audit and certification rule

For each family record before/after source SHA, triangle count, vertex/bounds change, exact named sockets and world-transformed entry/exit position. Verify normals, UV0, material, collider and selection hit/miss. A `FUNCTION CERTIFIED` family retains its prior isolated function. `INTERFACE CERTIFIED` additionally requires measurable landing/clearance at its claimed sockets and visual before/after evidence at the actual official cameras. Families with no through-route must not be falsely certified for circulation; they may pass only a rock/defensive-edge interface. If automatic surgery cannot preserve form and demonstrate the join, keep the original functional asset and report INTERFACE FAIL.
