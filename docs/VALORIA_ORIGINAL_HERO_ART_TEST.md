# Valoria: original architectural hero study

Date: 2026-09-26. Status: **rejected for promotion**. The study is deliberately isolated from Valoria and the Windows player scenes. The visual acceptance reference remains `docs/ELDORIA_VISUAL_BENCHMARK.md`.

## Tool audit and reproducibility

- Linux authoring workspace: official Blender 4.5.14 LTS downloaded for this study, plus its bundled Python and NumPy; the exported FBX and five authored 512 px texture studies are committed. SHA-256 of the official Linux archive: `9ba871ff2ecd36526b77432745980b7e6664ecd0c7ca11c48849073dcfe06da3`.
- Windows self-hosted Unity runner: Unity 6000.3.23f1 and Python available; `blender.exe` and `openscad.exe` not found in PATH or Blender Foundation program directory (workflow run `36232507332`). The runner consumes the **baked FBX**, so capture/build do not require Blender. ProBuilder is not declared by this Unity project.
- Run the source on a workstation with Blender 4.5 using `blender -b --factory-startup --python scripts/build_valoria_hero.py`. Override `ELDORIA_HERO_OUTPUT`, `ELDORIA_HERO_SAMPLES`, and `ELDORIA_HERO_RESOLUTION` if desired. The source and all five textures are original. No external model or licensed texture is used.
- `Unity/Assets/Eldoria/Scripts/Editor/ValoriaOriginalHeroCapture.cs` loads this model into a new, unsaved empty editor scene with URP Lit materials and produces three fixed 1280 × 720 review angles. Its menu item and CI method never write to `VisualWorld` or the playable scenes.

## What the actual art test contains

One interconnected upper hall, primary tower, two secondary towers, a gate with an open arch and interior doors, damaged ancestral gallery, masonry wall, buttresses, staggered slate roofs, deep window recesses, balcony, lower attached house, stairs, and a rocky terrace. The test includes independent limestone, aged masonry, slate, oak, and cliff texture studies; restrained heraldic blue and warmer gate lighting. Preview-only valley, canopy, and distant ridges are excluded from the FBX. The source and first Blender renders are in `scripts/build_valoria_hero.py` and `Unity/Assets/Eldoria/ArtTests/OriginalHero/`.

## Visual decision

**Fail. Do not substitute this for the Bastión or build a city around it.** Establishing, approach, and opposite-side reviews show a more vertical, distinct silhouette than the previous modular fortress, but the main faces are still broad flat cuboids. The secondary towers and gables remain too elementary, the front gate and buttress rhythm feel systematically generated, and the rock terrace reads as a separate presentation platform. The distant preview-only trees/hills have toy-like simplification and do not establish a credible world. It could not plausibly appear inside the owner's reference image without revealing itself as prototype geometry. Technical import or green tests do not overturn this judgment.

## What is actually missing

Blender itself is sufficient and free. The missing ingredient is **high-quality authored architecture**, not another procedural layout pass: sculpted or carefully retopologized broken ashlar and eroded cliff transitions; handmade Gothic/Romanesque openings and tracery with recess depth; deliberately asymmetrical roof junctions and turret transitions; wear/dirt/moss masks and proper high-quality normal/roughness maps; and an environment artist's manual silhouette, roof, and facade pass. A suitable alternative is a cohesive, licensed, zero-cost high-detail fortress family containing these elements and allowing modification; the current imported modular packs and this generated model do not meet that bar. Before any new approach is promoted, compare a small imported/rendered fragment at game-camera distance against all three angles and the official benchmark.

The original source/model/captures are retained as a reproducible **negative result** to avoid resuming micro-polish on the current Valoria scene. No art approval has been recorded.
