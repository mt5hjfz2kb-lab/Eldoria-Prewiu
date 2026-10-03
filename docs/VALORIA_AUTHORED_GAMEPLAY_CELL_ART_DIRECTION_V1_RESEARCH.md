# Valoria Authored Gameplay Cell Art Direction v1 — research and decision

2026-10-03. Source of truth: VQB accepted visual `7a564bc5056bb7166b7704c313ae4db7d55d4530`; prior Unified Visual Language proof `TECH PASS / VISUAL FAIL / NOT PROMOTED`. Fixed Flat Citadel and real interaction plots remain authoritative.

## Frame-first diagnosis / art sheet

The real 9/12/mobile pairs from run 37152713806 show a richly detailed pale Hero, darker functional source Aserradero, a repeated pale outer ring and one large, nearly featureless brown interior ground. The earlier exterior bands became technical strips; another flat ground tint barely changed the frame. The new hypothesis is physically authored, human-scale masonry between the main approach, Bastion steps and west work building, not an overall palette swap. This is a *cell proof*, not a city-wide art-family claim.

| Rule | This proof | Replacement seam |
| --- | --- | --- |
| Primary silhouette | Keep Hero Bastion source and outer ring exactly | Hero source GLB can be semantically separated in later Blender authoring; do not erase rock blindly |
| Stone | Warm-grey dressed paving, restrained dark retaining/curb | Two shared material families, replace texture/bake independently |
| Timber | Dark desaturated working lumber inside F1 | Replace work props with authored prefabs |
| Scale | Paving 0.25–0.35 world units, curbs 0.07, low returns 0.18–0.23 | Mesh dimensions are parameters, not changes to city size |
| Detail density | Stone pattern is visible on the C0 processional and Aserradero apron; distant ring remains quiet | One batched mesh per path keeps renderer cost contained |
| Inhabitation | Existing canonical occupation from VQB, one restrained lumber stack | Any future actor system remains separate |
| Camera | Compare official 19/12/9 and portrait with real HUD; 9.1 is runtime home | No forced portrait city fit or world scaling |

Reserved R4/R5/R6, F1–F3, H0, C0/C1/C2, XW/XE/XU/XS are protected. Only shallow visual stones along existing circulation, low removable Hero threshold returns and F1-owned lumber. No colliders/hotspots are introduced.

## Applied and rejected methods

- Unity's SRP Batcher guidance warns that MaterialPropertyBlocks can break batching. Use shared URP materials and **one combined paving mesh per route**, plus a few discrete low retaining pieces. [Unity Manual](https://docs.unity3d.com/6000.0/Documentation/Manual/DrawCallBatching-Properties.html).
- Blender's glTF exporter preserves mesh primitives/material assignments but UV seams/hard edges increase output vertices. A previous semantic Aserradero split proved editable geometry, yet its whole-model rebuild and shared atlas failed integrated visual review. This proof retains the richer canonical source and tests only the missing interface. [Blender Manual](https://docs.blender.org/manual/en/3.6/addons/import_export/scene_gltf2.html); [previous closure](VALORIA_AUTHORED_SECONDARY_ART_FAMILY_V1_RESULT.md).
- Trim sheets can unify modular environment pieces, but a first sheet spanning Hero/Aserradero/wall would require revisiting baked source masks and has already produced a worse palette in Eldoria. Defer that paid-production-scale decision until the played-camera interface proof wins. Specialized environment breakdown: [80.lv modular environment](https://80.lv/articles/making-of-a-modular-gothic-environment-with-procedural-systems-trim-sheets-custom-shaders).
- URP decals and vertex painting could soften stone/rock contact, but do not solve the large empty ground area or the fused source silhouette by themselves. No new renderer feature or package is justified in this bounded attempt. Shader and prior acceptance preserved.
- A free-form exterior field/crop strip was explicitly rejected by screenshots in the previous block; no retry under another name. Blender sources from the Authored Secondary block remain available as research, with no failed replacement GLB integrated here. Tripo: no geometry gap proven, zero credits.

## Reproduction

`pipeline/art-production-request.json` routes `environment_composition` to existing Unity assembly and official-camera validation. `ValoriaAuthoredGameplayCellV1.Apply` is a reversible visual-only root on top of VQB. The reused VQB capture workflow calls `ValoriaAuthoredGameplayCellGateV1.Capture` and stages the same external CC0 surface inputs as before. Gate captures BEFORE from VQB and AFTER from VQB plus candidate with real HUD at all four views; collision signature and focused HUD/progression tests must pass. Candidate remains experimental until actual images establish a significant whole-frame improvement.
