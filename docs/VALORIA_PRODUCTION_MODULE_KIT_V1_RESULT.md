# Valoria Production Module Kit v1 — Final Result

Date: 2026-10-01  
Base main HEAD: `9da483e4ea1b9780b1afc22ad610c6c0289ed0f9`  
Experimental branch: `visual-proof/valoria-production-module-kit-v1`

## Final verdict

**MODULE KIT PROOF PASS / NO MERGE.**

This experiment converts the Golden Cell visual recipe into a renderer-efficient modular implementation inside the real Valoria scene.

It proves that the main visual gain can be reproduced without the +389 renderer overhead of the first transfer experiment.

## Canonical evidence

Validated source HEAD: `c369d64d23ee6ae3bff0b1b505845bc7b6f25330`  
Successful run: **36788537850 — SUCCESS**  
Artifact: **11129969846**

Official captures:
- orthographic 19;
- orthographic 12;
- orthographic 9;
- mobile 390×844.

Gameplay:
- same-scene BEFORE / AFTER: **true**
- collider + hotspot signature equal: **true**
- gameplay topology changed: **false**
- Tripo credits: **0**
- paid assets: **0**

## Structural metrics

| Metric | BEFORE | AFTER | Delta |
| --- | ---: | ---: | ---: |
| triangles | 1,574,646 | 1,596,689 | **+22,043** |
| active renderers | 1,199 | 1,208 | **+9** |
| unique materials | 629 | 620 | **-9** |
| lights | 22 | 25 | **+3** |

Hard proof budgets:
- renderer delta max: **+20**
- triangle delta max: **+60,000**

Both gates pass comfortably.

## What changed versus the first transfer

The first Golden transfer reproduced the visual result but added **+389 active renderers** because the scene was built from many helper primitives.

Production Module Kit v1 instead collapses the same visual logic into reusable combined-mesh modules:

1. terrace / retaining structure;
2. processional stair;
3. west gate wing;
4. east gate wing;
5. central gate / arch / portcullis;
6. west rock-wall seam;
7. east rock-wall seam;
8. authored residence collapsed to one renderer;
9. authored workshop collapsed to one renderer.

The existing Bastion renderer family is re-materialized in place and therefore adds no renderer count.

## Visual result

The module kit preserves the important Golden Cell read:

- Bastion remains the primary focal point;
- processional stair is clear at mobile distance;
- stone / timber / roof separation is readable;
- gate hierarchy is stronger than main;
- rock / retaining transitions frame the upper terrace;
- support architecture creates human scale;
- warm gate light against cooler environment survives the official cameras.

The v4 tuning deliberately reduces the mass of the gate wings and stair cheeks so the Bastion remains visible instead of being boxed in.

## Rejected alternatives

The following were reviewed and rejected:

- complete CC0 hero-fort glTF: fragmented silhouette / weak hierarchy;
- generic Hero Fortress Replacement: blocky, low-detail focal mass;
- helper-heavy Golden transfer as production implementation: excessive renderer count;
- replacing the current Bastion with unrelated authored families: wrong architectural language.

## Production implications

Validated production principles:

1. keep gameplay topology independent from visuals;
2. use one renderer per logical architectural module where practical;
3. reuse shared PBR material families;
4. combine authored support prefabs into renderer-efficient meshes;
5. keep official 19 / 12 / 9 / mobile gates;
6. require same-scene collider/hotspot equality;
7. optimize renderer structure before citywide propagation;
8. reject technically-valid assets when they weaken silhouette/hierarchy.

## Why this is still NO MERGE

This proof is strong enough to validate the production approach, but it is not yet approved for direct `main` promotion because:

- final visual acceptance by the owner is still required;
- Android device profiling has not been performed;
- the cell remains experimental and must be converted into clean production-facing APIs/assets before merge;
- citywide propagation has not been validated.

## Next step if visually approved

1. freeze the v4 composition and metrics;
2. extract the combined module builder and material setup into production-owned code/assets;
3. profile the cell on the Android target;
4. integrate only this bounded cell into a production candidate branch;
5. re-run Unity tests/build + Valoria visual gates;
6. only then consider merge to `main`.

## Final disposition

- visual direction: **PASS**
- renderer-efficiency proof: **PASS**
- gameplay safety: **PASS**
- production architecture direction: **PASS**
- Android performance certification: **PENDING**
- merge to main: **NO**
- citywide propagation: **NO**
