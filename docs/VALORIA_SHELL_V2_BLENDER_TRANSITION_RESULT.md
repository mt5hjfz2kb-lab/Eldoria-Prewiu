# Valoria Visual Shell v2 — Blender transition proof

Date: 2026-10-03. Experimental branch: `visual-proof/valoria-shell-v2-blender-transition`. No production visual code promoted.

## Why this proof exists

The approved benchmark expects one connected mountain, Bastion, terraces, city and deep horizon. Earlier projected matte and native extruded crags failed, and the procedural height field improved continuity but produced a bare mesa. This block tested a Blender-exported contiguous terrain surface with separate ground/rock materials, metre-scale UVs, a footprint bound to the current fixed camera, and Unity's existing Hero Bastion and Kiara 3 backplate. The GLB was staged by CI only. Gameplay, hotspots, colliders and routes were not changed.

## Runs and real images

| Source / run | Artifact | Result |
| --- | --- | --- |
| `b1612080` / [37121765626](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37121765626) | Logs | Blender export blocked by missing NumPy in hosted package; no visual evidence. |
| `a810f5dc` / [37121873960](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37121873960) | `11273955784` | Blender GLB exported; Windows PBR staging failed due to regex escaping. |
| `08ac55f2` / [37122003698](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37122003698) | `11274220503` | Unity TECH PASS, visual invalid: terrain triangles faced downward, showing strips and isolated rocks. |
| `aa9b9008` / [37122211553](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37122211553) | `11274220795` | Unity TECH PASS, visual fail: upward ground but 180° rotation put the upper terrace in the foreground; missing UV gave flat colour. |
| `014e0f56` / [37122415465](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37122415465) | `11273791699` | Unity TECH PASS / VISUAL FAIL. Corrected orientation and UV, captured 19/12/9/mobile and east/west. |

The final capture's `collider_hotspot_signature_equal` is true. The exported GLB and maps came from Blender and CC0 Poly Haven; Tripo credits spent: 0.

## Visual decision

**FAIL. Do not promote this shell.** In the 1280×720 frame the Bastion retains its strong authored silhouette, but the new surface is a wide, simple earth slope. The rock transitions cut bright wedges into it, lower functional activity is mostly absent from this strongest composite, and the photographic valley still has a different detail scale. The mobile crop is dominated by blank earth. West/east views expose the same material and composition gap. This has not crossed a category toward the approved reference.

There is a real improvement in *terrain continuity* compared with the floating island and three isolated native crags. It is narrower than the goal: a connected mass by itself does not create an integrated fortress city. One final structural correction of winding, axis and UV was warranted because earlier images were technically invalid. The resulting image is a valid rejection, so stop this generator route.

## Bottleneck and next method

The demonstrated ceiling belongs to the current automated scene authoring method: height fields or generated grids, scattered secondary prefabs, and an unrelated photographic backplate. Unity/URP and Blender successfully imported and rendered the mesh while preserving gameplay; they are not shown to be the ceiling. The missing deliverable is a deliberately composed, camera-specific **Hero-to-district environment assembly**: authored rock strata and retaining walls shaped around the Bastion, readable civic terraces and circulation, a coherent mid/lower architecture family at matched texel density, and a background graded to that geometry. Restore the functional sawmill/barracks/granary visually within the exact 19/12/9/mobile frame before judging any new shell. This requires a composition-first authored model and a complete-frame paintover/mesh design, not another noise function or prefab coordinate loop.

No production promotion and no tester build change. Preserve the branch and artifacts for evidence.
