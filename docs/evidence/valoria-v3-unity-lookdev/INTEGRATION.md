# Valoria V3 — isolated Unity lookdev experiment

Status: **implementation started, NOT validated in Unity, NOT artistic completion**.
Branch: `integration/valoria-v3-unity-lookdev-20261012`.

## Approved exact sources
The Work delivery `docs/evidence/valoria-asset-family-v2/QUALITY_RELEASE_RESULT.json` on `art/valoria-asset-family-v2-ai-authorship-20261011` records `FAMILY_SOURCE_ART_PASS_AT_FROZEN_HERO_V3_MINIMUM`. Source run 38184974032, approved commit `0abf13870d21725c30acf3f3e3a8c86385470073`, delivery artifact `11719219312`. Seven assets and LOD0/1/2 GLBs; **no Unity pass**. SHA-pin and verify GLBs when staging them; do not use older v1 assets.

Source folder: `art-source/valoria/production/asset-family-v2/quality-v3/<piece>/<piece>.glb`.
Names: wall_straight, watchtower, wall_corner, wall_connections, stone_path_platform, cliff_rocks, defensive_props.

## Implemented preliminary engineering
`Unity/Assets/Eldoria/Scripts/Experimental/ValoriaFamilyLookdev.cs` is a **source-only experimental component** for loading GLBs from `Assets/StreamingAssets/ValoriaFamilyV3/` through the existing glTFast package into a staging hierarchy; it applies a basic alternative light/material tint. This is NOT a lookdev quality result. It must be compiled and iterated on in the actual Unity editor before being used as evidence. The code does not yet provide an official capture harness or guarantee exact A/B switching; do not present it as finished.

## Required steps for the owning Unity integration
1. Confirm actual repository ownership, acquire Windows runner lease only when free. Stage seven exact GLBs from approved source branch into an isolated test working copy (do not overwrite canonical assets); verify stored SHA256. Since sources reside in a separate branch, checking out `main` alone is insufficient.
2. Compile and fix importer/API issues under Unity 6000.3.23f1 / URP 17.3 / glTFast 6.14.1. Use the project import configuration and correct Android file URI handling if packaging for mobile.
3. Create an isolated persisted test scene and matched cameras. Integrate frozen Hero V3 unchanged. Compare *same model, camera, composition and placement* in neutral base versus material/light/environment lookdev.
4. Improve **each of seven**: wall body and stone joints; tower cornices and heraldry; 90-degree corners; straight/gate joining; paving roughness and terrain support; scanned cliff stratification, silhouette and seams; props cloth/metal/torch emissive. Use URP Lit with texture/normal/metallic/roughness maps validated against actual GLB material channel packing.
5. Drive changes with before/after actual engine screenshots (hero, strategic/mobile, seam/contact views) and commit scene settings, lighting, shader/material presets and exact source SHA. Do not change geometry to claim a material A/B improvement.
6. Test mobile frame time, GPU bottleneck, memory, batches, overdraw, shadows and texture tiers on hardware; document tradeoffs. Validate no gameplay or camera regressions.
7. Escalate structural fixes to Blender artist with precise measurements. Approve only per-asset ART PASS and Unity IMPORT PASS on independently inspected evidence, then performance PASS on genuine device. Keep `main` and published scene unchanged until promotion gate.

## Caveats
A source PASS in Blender is not a Unity pass. This branch is a development artifact without engine captures or proven runtime improvement. Do not assign arbitrary visual scores. Preserve provenance and zero paid credits.
