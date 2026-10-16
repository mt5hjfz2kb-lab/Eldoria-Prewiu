# Valoria — water V10 and last lime-mortar V11, source-verified continuation

**Date:** 2026-10-16. **Scope:** the 63 independently reused Valoria assets, no scene assembly, no Unity writes, no paid services.

## One actual improvement: river_water_tile

- Previous model [run 38897442996](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38897442996) (stochastic V9 water), subjective **5.7/10**. Avoid promoting banded V8.
- Real new model and fixed-camera before/after generated in [run 38907369609](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38907369609) (workflow returned overall FAILURE **only because mortar V10 crashed**), artifact **12050225774**, nested `valoria-water-transmission-v10/`.
- Genuine Blender output GLB `river_water_tile.glb` SHA256 **d9baee5247c1641415daef580c3d08972f85f746e8ad022a2201692a8c5009bf** (1,312,456 bytes). Same-camera PNG SHA before **541d599460182ce8d23f2148d4c20b446bc2d50825976af98f082667ed488f29**, after **e22d9bcb8c67af89bd41025339579ce2776357432c0ab6ed5325cbabfd67cf91**.
- Independently opened the two real PNGs. After has darker believable river-blue water and more coherent surface highlights; still slightly concentrated/specular. Subjective **6.1/10 provisional, not ART_APPROVED and no final scene/user approval**. The score is not from the CI result.
- Examined **actual embedded binary GLB JSON**: `KHR_materials_transmission` factor 0.699999988, `KHR_materials_ior` 1.33299994, roughness 0.17, alphaMode BLEND; 1 mesh, 2 images. The neutral brown bed exists **only in the studio**, excluded from export. Thus physical glTF transmission genuinely exists; Unity URP compatibility, water contact, animation and reflections remain unvalidated. Keep V9 as fallback.

## Last deficient model: mortar_joint_recess

- Actual V9 thin joint `mortar_joint_recess` subjective **5.7/10**, with physically plausible ~26mm height but white pebble blobs and material too uniform. Do not mistake narrow appearance in a wide camera for failure; require macro.
- Composite test [38907369609](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38907369609) **failed to finish** because `true_lime_mortar_v10_staged.py` wrongly assumed the V9 glTF had embedded its original Blender-only CC0 diffuse node. Error: `Original source CC0 photo was not embedded`. The water half of the composite is valid independently.
- New code `tools/valoria-modular-fortress/true_lime_mortar_v11_explicit_cc0.py` fixes source dependency: if no imported color-image, load original verified photographed CC0 lime atlas **from run 38894488570**, artifact **12043122730**, `eldoria-mortar-lime-v8/atlas/real_photo_lime_mortar_aged_muted_1k.png`; preserve source V9 `mortar_joint_recess.glb` from run **38895731036**, artifact **12044250637**.
- Actual running/queued producer: [run 38910917796](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38910917796), separate Windows Blender A/B and macro. Do not assign score or mark 6 until checking new PNGs. Preserves old and new SHA, original photographed source hash and finite geometry.
- Shared Windows resource at 15:42 local was occupied by independent Bastión I run **38910601788**. New mortar run waits in queue rather than killing/competing with the Bastión work.

## Quality gate

Previous evidence handoff: 61/63 in the **visual neighborhood of >=6**, two below (water and lime). Incorporating water V10 **provisionally** gives **62/63**, lime unresolved. Neither threshold count nor a CI PASS equals a certified game-ready asset. The eventual **63/63 >=6** is only a provisional Blender studio floor until explicit user review; **0 ART_APPROVED**, Unity A/B still required for final 8/10 decisions under `docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md`.
