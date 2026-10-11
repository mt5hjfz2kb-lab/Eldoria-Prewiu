# Valoria hero gate — surface quality V2

This is an in-place reauthoring of the gate preserved at `4d9ac9ebfb713898c37572604b2c03238afec4e5`. It opens that exact editable `.blend`; it does not create another gate or alter the architectural footprint. The baseline stays in `art-source/valoria/production/first-hero-gate-v1/` and the revised editable source/export live under `quality-v2/`.

## Reproduce in a normal chat or local shell

Check out `art/valoria-first-hero-asset-v1-20261011` with the large art-source path included in sparse checkouts. Use Blender **4.2.5 LTS** (the workflow pins the official archive SHA-256 `cd28ab8164d88237c0c45a188ef7e4974bcb8c651de15fcbc3635166e584e4b3`), Python 3 with NumPy and Pillow, and no paid endpoint. Then:

```bash
python3 tools/valoria-art-brain/bridge.py --check
python3 tools/art-rd/first-hero-support/quality_surfaces.py
blender -b -t 8 --python tools/art-rd/first-hero-support/quality_v2.py
ELDORIA_HERO_VERIFY_OUTPUT="$PWD/art-source/valoria/production/first-hero-gate-v1/quality-v2" \
  blender -b -t 8 --python tools/art-rd/first-hero-support/verify_export.py
python3 tools/art-rd/first-hero-support/quality_board.py
```

`quality_v2.py` verifies the input SHA, renders the original and modified gate from the **same scene, camera, lights, exposure, samples and resolution**, saves editable `.blend`, and exports `.glb`. The verifier imports that exact GLB back into Blender, checks UVs/material slots, captures it and exports one conservative LOD1. Compare `before-hero-matched.png`, `after-hero-matched.png`, `after-glb-roundtrip.png`, `after-lod1.png` and `after-strategic.png` with your own eyes. The original `detail-refine.png` and new `after-detail.png` use the same camera settings. A tested LOD2 collapse broke masonry seams and was rejected; do not call the V1 LOD2 an export of this revision. The GitHub Actions workflow repeats these commands on an isolated free Linux runner and preserves the result on the task branch.

For material changes, edit `quality_surfaces.py` and rerun the commands. For shaped stone changes, edit the bounded transforms in `quality_v2.py`, keeping the same baseline and cameras. The renderer, bitmap generation, mesh export and hashes are automatic; the choice to accept or reject a surface treatment is an **artistic decision**, never a result inferred from a test exit code. Keep rejected experiments out of the preserved `quality-v2/` result.

The existing art brain bridge inventories `tools/art-studio-recovery/` and `tools/art-studio-reuse/` at pinned historical source `e137aaf713c682509cacc62cd1b3279c140a811c`. The source vision adapter is rock-specific and requires local Ollama/Qwen3-VL; adapt it to the gate only when that inference service actually exists. The old BlenderMCP correction was executed for V1 and is preserved in its report. V2 uses direct Blender mesh editing because the bounded edits are testable in the source itself; do not label the V2 pass an MCP execution. The PBR bitmaps here are deterministic original authoring; there is no CC0 donor to license or download.

## Unity route and mobile limits

When the Windows Unity runner's active owner releases it, reuse the historical isolated importer/capturer on `integration/valoria-gatehouse-unity-v1-20261011`, targeting `quality-v2/ValoriaHeroGate.glb` and its exact SHA. Capture strategic, mobile and detail camera views, save the scene, close the editor and reopen to verify persisted materials. Unity **6000.3.23f1** must be the execution version. The Blender renders and GLB roundtrip are not Unity captures or Android/iOS performance measurements. Select and measure the single safe LOD1 on physical targets before shipping; a lower-cost LOD2 must be reauthored without breaking the stone silhouette.
