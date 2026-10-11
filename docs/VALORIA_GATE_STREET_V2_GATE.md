# GateStreetRiseRock V2 — isolated module gate

Date: 2026-09-27.

Source identified automatically in the Windows runner Downloads before processing:

- Exact file: `Eldoria_Module_GateStreetRiseRockV2.glb`
- Size: 7,806,592 bytes
- Last write: 2026-09-27T20:53:41.1688662+02:00
- SHA-256: `d1cfd520d07e2e311016d52fe40d11e3b0abc6e16ebc83a044e320f25ecba692`

The previous GateStreetRiseRock source SHA `345e483c961a35738eb34fdeeca217cb79ffc7324d8989e05015322cfac8e458` was explicitly excluded, as were TowerWallRock / TerraceStairRock / Bastion / Castle candidates.

Validation run: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/36342620782  
Artifact: `eldoria-tripo-gate-street-f0a3926a751d20d3f26dce320ea59d663756b031` (ID 10940120314).

The workflow's overall conclusion is red only because an auxiliary post-validation step attempted to commit evidence from the self-hosted runner and Git rejected the checkout as an unsafe directory. Identification, Blender, Unity import/gate and artifact upload all completed successfully.

## Blender measurements

| Stage | Objects | Vertices | Triangles | Materials | Images | UV0 | Normals | Bounds | Size |
| --- | ---: | ---: | ---: | ---: | ---: | --- | --- | --- | ---: |
| Raw source | 1 | 162,581 | 325,290 | 1 | 0 | absent | present | 0.666748 × 0.981293 × 0.546356 | 7,806,592 bytes |
| Optimized | 1 | 24,836 | 49,800 | 1 | 0 | present | present | 0.667597 × 0.981339 × 0.546468 | 2,194,524 bytes |

Optimized GLB SHA-256: `8df7cb42ab945e923332dfc087db98c49ea231b75b962c30654c8dc5c08203fa`.

The existing automatic Blender path was reused unchanged: target 49,800 tris with accepted range 49,500–50,000. UV0 was generated because the raw source had no UV layer.

## Unity technical gate

Unity 6000.3.23f1 imported:

- 1 mesh / 1 renderer / 1 material / 0 textures
- 59,204 imported vertices
- exactly 49,800 triangles
- UV0 present
- normals present
- 1 MeshCollider
- positive mesh raycast: PASS
- empty-space selection miss: PASS
- 19 / 12 / 9 / oblique captures: non-empty
- bounds after review scaling: 12.2453 × 10.0235 × 18.0
- reported Editor mesh runtime memory: 4,985,816 bytes

**Technical gate: PASS.**

## Real visual review

The acceptance criterion was not merely an open arch. The route had to be traceable as:

`lower approach → passage through arch → clearly visible ascent → upper landing/terrace`.

### 19
A compact rocky fortified module reads at strategic distance. The large arch is visible as an architectural element, but the lower approach does not visually enter it and no continuous ascending route can be followed.

### 12
The approach road terminates against the front rock/fortification mass. The large arch remains elevated/recessed behind the foreground mass. The path from ground level through the arch to a climb is not readable.

### 9
More geometry is visible, including the arch and inner structures, but the decisive connection still fails: the lower road, arch passage, visible climb and upper landing do not form one legible sequence. The supposed ascent is occluded/ambiguous.

### Oblique
The module has coherent depth and rock integration, but the road still meets the front mass rather than demonstrating a readable lower-to-upper circulation line. The elevated architecture reads as a separate fortified level rather than a connected street route.

### Frontal diagnostic
The frontal view shows a small central stair-like element and the large arch behind/above it, but the lower entry is blocked/ambiguous and the complete circulation chain cannot be visually traced.

**Functional visual gate: FAIL.**

This is a capture-based production judgment, not a claim that no conceivable internal surface exists. Under the project gate, hidden or ambiguous continuity is insufficient: if the ascent cannot be read in the official views, the module does not satisfy the connective function.

## Decision

GateStreetRiseRock V2 improves the architectural form and retains a strong open arch, but it still does **not** prove the required continuous circulation:

`entrada inferior → arco → subida visible → rellano superior`.

Therefore:

- do **not** certify GateStreetRiseRock V2 as the third modular family;
- do **not** build the requested three-family Micro-Valoria iteration from this V2;
- retain the existing two-family Micro-Valoria result as the latest composition gate;
- Valoria.unity, VisualWorld and gameplay remain untouched;
- no Tripo credits were used during validation.

The next acceptable source must expose the entire lower-to-upper route in the geometry itself, with the lower road visibly entering the gate and the climb remaining exposed enough to read at 19/12/9 and oblique without relying on hidden internal traversal.
