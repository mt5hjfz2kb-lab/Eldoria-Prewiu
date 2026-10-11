# Valoria hero gate V3 — reproducible selective refinement

Canonical branch: `art/valoria-first-hero-asset-v1-20261011`. Protected `main` is not involved.

## Input provenance

V3 starts by opening the editable `quality-v2/ValoriaHeroGate.blend`. It does not regenerate or replace the two towers, entrance, portcullis or structural silhouette. V2 itself is reproducibly derived from the pinned V1 Blender source. The source geometry is preserved before GLB evaluation.

## Production

The existing `.github/workflows/valoria-first-hero-asset.yml` runs on a push to the allowlisted art scripts and installs Blender 4.2.5. It executes `quality_surfaces.py`, `quality_v2.py`, then `quality_v3.py`. The latter deliberately edits individual existing masonry meshes, uneven bevel treatment and returns, using a fixed seed. It saves a new editable `.blend`, unmodified original V2 evidence, new PNG hero/strategic/detail renders, GLB, and machine-readable report.

Images for honest matched comparison:
- `quality-v2/after-hero-matched.png` vs `quality-v3/after-hero-matched.png`
- `quality-v2/after-strategic.png` vs `quality-v3/after-strategic.png`

No visual acceptance is implied by the workflow's SUCCESS.

## Existing art intelligence

Read `tools/valoria-art-brain/bridge.py` and the operations-branch `RUNTIME_RECOVERY.md`. The workflow retrieves `start_local_qwen_ci.sh` from the operations branch and uses local free `qwen2.5vl:3b` to review the real reference and matched hero images; its advisory and exact input paths are preserved in `quality-v3/qwen-art-review.json` if inference completes. Qwen is NOT authorized to certify quality. BlenderMCP rock-contact recipes are not blindly applied to a gate; this V3 stage uses the existing Blender Python automation directly rather than claiming an MCP operation.

## QA gates

Open actual PNGs side-by-side; inspect both hero and strategic scales. Reject if surface changes are indistinguishable, hurt cohesion, or break stone contacts. Check `quality-report.json` for object counts and GLB export, then validate imported GLB/materials and strategic readability in an isolated Unity scene through authorized existing Windows CI only after art is accepted and runner ownership verified. Mobile device quality and Unity import remain **unverified** until real artifacts exist.

No paid models, resources or external art licenses are introduced.
