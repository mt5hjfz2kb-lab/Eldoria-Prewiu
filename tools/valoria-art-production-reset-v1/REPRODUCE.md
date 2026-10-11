# Valoria reset — reproducible greybox

This is isolated PREPRODUCTION, not production art. The canonical target bytes must remain unchanged. No Tripo, final asset sources, runtime scene edits, or spend are involved.

## CPU geometry and camera reproduction
From repository root, install NumPy, SciPy and Pillow in a temporary Python environment; `requirements.txt` records compatible versions. DejaVuSans.ttf is used for annotations (`/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf`; adapt the font path on another OS).

```sh
python tools/valoria-art-production-reset-v1/blockout.py
python tools/valoria-art-production-reset-v1/compare.py
```

`blockout.py` rebuilds the complete scene JSON, Unity mesh input, OBJ and 9 views from primitives. `compare.py` uses saved real Unity captures/bounds when available; otherwise it explicitly falls back to CPU captures. The original target is never rasterized into the greybox. PNG annotations are derived and may be regenerated from committed JPG previews; the exact canonical JPEG and authoritative Unity PNGs are preserved.

## Authoritative Unity replay
Editor 6000.3.23f1, project `Unity/`, licensed runner. At captured commit `73d0bfee2692faa3c9590b65486057080031ba9a`, the existing workflow ran `Eldoria.EditorTools.PreproductionSceneCapture.Capture` with the mesh input and view request, and executed five focused PlayMode tests. Consult the workflow for its exact batch invocation/output directory. The replay creates an empty ephemeral scene, without loading or saving the gameplay scene. Captured run: 37275659322; artifact: 11329019907.

The current request is intentionally **disabled** following capture. A future recapture must claim the runner/workstream, then set `enabled: true` with a new nonce under the same isolated scope; restore `enabled: false` after evidence. Never replay the legacy final-art mode for this scene. Screenshots in `unity-final/` and its `evidence.json`/test XML are the authority, not the CPU render.

## Evidence integrity
`package-manifest.json` lists SHA-256 for committed package files, excluding itself. `preservation-proof.json` compares 1000 protected baseline files by Git blob identity. `projection-crosscheck.json` compares camera/bounds transfer; `target-blockout-comparison.json` measures macro landmark alignment with manual uncertainty. None of these is final-art approval. Owner blockout review remains pending.
