# GateStreetRiseRock V3 — isolated module gate

Date: 2026-09-27.

Source identified automatically in the Windows runner Downloads before processing:

- Exact file: `Eldoria_Model_GateStreetRiseRock_V3.glb`
- Size: 7,997,192 bytes
- Last write: 2026-09-27T21:20:05.2524772+02:00
- SHA-256: `32fa4360d87f0bc325ebbe792fa03c2428d5efdd4d90f26f52562029c746bd73`

The workflow explicitly excluded the previous GateStreetRiseRock source SHA `345e483c961a35738eb34fdeeca217cb79ffc7324d8989e05015322cfac8e458` and V2 SHA `d1cfd520d07e2e311016d52fe40d11e3b0abc6e16ebc83a044e320f25ecba692`, and excluded TowerWallRock / TerraceStairRock / Bastion / Castle candidates.

Validation run: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/36344152482  
Artifact: `eldoria-tripo-gate-street-642842530eef9de3768ff97fcd60c4ff249985fe` (ID 10939503397).

## Blender measurements

| Stage | Objects | Vertices | Triangles | Materials | Images | UV0 | Normals | Bounds | Size |
| --- | ---: | ---: | ---: | ---: | ---: | --- | --- | --- | ---: |
| Raw source | 1 | 166,448 | 333,439 | 1 | 0 | absent | present | 0.859802 × 0.980927 × 0.543701 | 7,997,192 bytes |
| Optimized | 1 | 24,628 | 49,799 | 1 | 0 | present | present | 0.859648 × 0.981748 × 0.543780 | 2,199,160 bytes |

Optimized GLB SHA-256: `27cc221f006328c968b6e161e5e4ea21be1744647fa8846344973070bf9f001b`.

The existing automatic Blender path was reused unchanged: target 49,800 tris with accepted range 49,500–50,000. UV0 was generated because the raw source had no UV layer.

## Unity technical gate

Unity 6000.3.23f1 imported:

- 1 mesh / 1 renderer / 1 material / 0 textures
- 59,349 imported vertices
- 49,799 triangles
- UV0 present
- normals present
- 1 MeshCollider
- positive mesh raycast: PASS
- empty-space selection miss: PASS
- 19 / 12 / 9 / oblique captures: non-empty
- review bounds: 15.7613 × 9.9700 × 18.0
- reported Editor mesh runtime memory: 4,995,080 bytes

**Technical gate: PASS.**

## Real visual review

The decisive acceptance criterion remained:

`entrada inferior → paso por el arco → ascenso visible → salida/rellano superior`.

### 19
The module reads as a coherent fortified rocky mass and the monumental arch is visible. However, the lower approach road reaches the front rock mass rather than visibly feeding into the arch. The ascent is not readable as one continuous route.

### 12
The open arch is clearer than in V1/V2, but it still reads as an elevated architectural feature. The lower road and the upper gate level are spatially separated by foreground rock/fortification mass, and no exposed climb connects them in the capture.

### 9
Additional inner geometry becomes visible, yet the required circulation chain is still not traceable. There is no clear sequence from the lower entry through the arch into an exposed ramp/stair and then onto an upper landing.

### Oblique
The module shows genuine depth and coherent architecture-rock integration, but the side/rear view does not reveal the missing circulation. The lower approach still reads as arriving at the rock base while the upper fortified level remains disconnected visually.

### Frontal diagnostic
This is the clearest failure evidence: the review road terminates at the front rock face/base. The large gate and upper structures sit behind/above that mass. A visible lower opening and continuous exposed ascent to the upper landing are not demonstrated.

**Functional visual gate: FAIL.**

This is deliberately stricter than “the arch is open.” The gate tests readable urban circulation at the official views. Hidden, internal, ambiguous or camera-dependent continuity does not satisfy the connective role required for Micro-Valoria.

## Decision

GateStreetRiseRock V3 is technically sound and is visually more coherent as architecture than the prior iterations, but it still does **not** prove the required continuous circulation:

`entrada inferior → arco → subida visible → rellano superior`.

Therefore:

- do **not** certify GateStreetRiseRock V3 as the third functional modular family;
- do **not** build the requested new three-family Micro-Valoria iteration from V3;
- retain the two-family Micro-Valoria result as the latest composition gate;
- Valoria.unity, VisualWorld and gameplay remain untouched;
- no Tripo credits were used during validation.

The next acceptable source must make the lower entry itself part of the visible gate/ramp system: the approach should visibly enter an opening at ground level, the climb must remain exposed through or immediately beyond the arch, and the upper landing must be visually contiguous from at least the 12 and 9 views, with 19 still preserving the overall lower-to-upper reading.
