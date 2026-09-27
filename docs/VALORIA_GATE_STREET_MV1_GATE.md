# GateStreetRiseRock MV1 — multiview isolated module gate

Date: 2026-09-27.

The Windows self-hosted runner first inventoried recent Downloads and identified the new multiview source unambiguously:

- Exact file: `Eldoria_Module_GateStreetRiseRock_MV1.glb`
- Source path on runner: `C:\Users\crist\Downloads\Eldoria_Module_GateStreetRiseRock_MV1.glb`
- Size: 8,186,028 bytes
- Last write: 2026-09-27T21:48:11.6257811+02:00
- SHA-256: `4d1c19978302643003600b0da5ed2f0ef14f7256c85da03077d36fa9f13e294e`

Inventory run: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/36345917408

The production gate then selected MV1 by **exact name + exact SHA**. V1, V2 and V3 were explicitly excluded by their known hashes:

- V1: `345e483c961a35738eb34fdeeca217cb79ffc7324d8989e05015322cfac8e458`
- V2: `d1cfd520d07e2e311016d52fe40d11e3b0abc6e16ebc83a044e320f25ecba692`
- V3: `32fa4360d87f0bc325ebbe792fa03c2428d5efdd4d90f26f52562029c746bd73`

TowerWallRock, TerraceStairRock, Bastion and Castle families were excluded by name. No ambiguous fallback selection was used.

Validation run: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/36345950898  
Artifact: `eldoria-tripo-gate-street-b305629dccd38dba7e3d06cbbe29708317523180` (ID 10941290390).

## Blender measurements

| Stage | Objects | Vertices | Triangles | Materials | Images | UV0 | Normals | Bounds | Size |
| --- | ---: | ---: | ---: | ---: | ---: | --- | --- | --- | ---: |
| Raw MV1 | 1 | 170,408 | 341,254 | 1 | 0 | absent | present | 0.872131 × 0.979797 × 0.552399 | 8,186,028 bytes |
| Optimized | 1 | 24,678 | 49,799 | 1 | 0 | present | present | 0.872556 × 0.979619 × 0.552968 | 2,101,016 bytes |

Optimized GLB SHA-256: `9d1a97ea385d557b77779eef7a7b65f8027bfc79a56b26081abc6b62f25a8302`.

The existing Blender pipeline was reused without inventing a new production path. Target was 49,800 triangles, accepted interval 49,500–50,000. UV0 was generated because the raw Tripo export had none. Normals and the single material survived the reduction.

## Unity technical gate

Unity 6000.3.23f1 imported MV1 into the existing isolated GateStreetRiseRock review scene.

Measured result:

- 1 mesh / 1 renderer
- 1 material / 0 textures
- 56,282 imported vertices
- 49,799 triangles
- UV0 present
- normals present
- 1 MeshCollider
- positive raycast: PASS
- empty-space miss: PASS
- review bounds: 16.0328 × 10.1605 × 18.0
- mesh runtime bytes: 4,798,784
- 19 / 12 / 9 / oblique captures non-empty
- frontal diagnostic generated successfully

**Technical gate: PASS.**

## Real visual review

Required functional chain:

`entrada inferior → paso bajo el arco → ascenso visible → salida/rellano superior`

### 19 — strategic

MV1 reads as a more coherent single rocky-fortified object than V3. The upper architecture is better consolidated and the massing is easier to understand globally. However, the lower review road still terminates against the front rock mass. There is no globally readable ground-level entry feeding the gate/rise system.

### 12 — city

The architecture is clearer and the upper enclosure feels more spatially coherent than V3. The open arch is visible, but it remains part of the elevated fortified structure rather than the visible continuation of the lower road. The lower approach, gate passage and ascent cannot be followed as one route.

### 9 — detail

More openings and internal architectural depth are visible, but the key problem remains structural rather than resolution-related. There is still no exposed continuous path from the lower approach into/under the arch and upward to a visible landing. Extra detail does not recover the missing route.

### Oblique

Multiview has materially improved side/rear coherence: the asset feels less like a single-view facade and more like a genuine volume. This is the clearest improvement over V3. Nevertheless, the lower road arrives at the rock base and the elevated circulation remains disconnected visually.

### Frontal diagnostic

This is decisive. The road points directly at the lower rock face/base. No clear ground-level opening receives it. The visible arches are higher in the architecture, while the supposed rise is hidden/ambiguous behind rock and walls. The entire chain cannot be traced from bottom to top.

**Functional visual gate: FAIL.**

## Comparison with V3

Multiview **did help**, but mainly in 3D coherence:

- better side/rear volume;
- more coherent upper architecture;
- stronger sense that the same structure exists from several views;
- less single-view distortion.

It did **not** solve the actual connective requirement. The same core failure persists for a fourth iteration: the generated rocky foundation dominates the lower frontage and prevents a clearly exposed ground-to-upper circulation line.

## Next production decision

Do **not** make another full GateStreetRiseRock iteration by simply changing multiview references again. Four generations now show the same structural tendency: Tripo fuses the lower access into a decorative rock foundation and treats the upper gate/street as elevated architecture.

Recommended next move: **divide the connective function into simpler modules** with explicit interfaces, for example:

1. lower open gate / road receiver on rock;
2. separate exposed rising street or stair/ramp segment;
3. upper landing / terrace connector.

These modules can then overlap/snap in Unity or Blender while preserving a visually continuous route. This reduces the number of simultaneous spatial constraints Tripo must solve in one generation.

**Smart Mesh/P2 is not the next primary fix.** It may be useful after a source already contains the correct route, for topology/mesh quality or cleanup. It should not be expected to invent the missing circulation in a geometrically wrong whole-piece design.

A second multiview configuration is lower priority than modular decomposition because MV1 already proves that multiview improves volumetric consistency but not the required functional layout.

## Decision

- MV1 technical gate: **PASS**
- MV1 functional visual gate: **FAIL**
- third functional family certification: **NO**
- new three-family Micro-Valoria: **NOT BUILT**
- recommended next route: **split the connector into simpler explicit modules**
- Valoria.unity: untouched
- VisualWorld: untouched
- gameplay: untouched
- Tripo credits spent by this validation: 0
