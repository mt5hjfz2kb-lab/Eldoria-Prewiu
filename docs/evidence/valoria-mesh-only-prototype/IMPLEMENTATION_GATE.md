# Valoria Bastion I – alternative to SHARP – proof gate (2026-10-10)

- Separate branch: `prototype/valoria-bastion1-no-sharp-20261010`. Canonical `main` and published game remain unchanged.
- Existing Blender-authored production GLBs used: Bridge, Lower Gate, Road, Stair, Wall, Bastion, RockTerrain.
- `Unity/Assets/Eldoria/Scripts/Editor/ValoriaMeshOnlyBastionOneProof.cs` stages the tracked GLBs and defines a mesh-only experimental Unity scene and a PNG camera capture; the Unity editor entry point has **not** been executed or compiled.
- `tools/valoria_mesh_only_preview.py` imports source GLBs into free Blender and renders a camera-space staging proof with NO SHARP PLY, splats, or source projection.
- Blender run [38057660737](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38057660737) completed SUCCESS **technical execution only**, and committed a small inspectable thumbnail to `preview.jpg.base64.txt`.
- Direct visual inspection of that thumbnail: **VISUAL FAIL**. Isolated components float, lack continuous terrain/building contact, and do not resemble a coherent playable city. The component stage is not production quality.
- Previous runtime's SHARP presentation is still canonical. Never replace/disable it on `main` based on these checks.
- Independent next work: abandon viewport-proxy placement as primary integration; create physically consistent Blender-authored plateau/fortress scene from approved original geometry and materials, with reliable world-space anchors, terrain/architecture contact, a coherent matched-camera render and an executable Unity scene.
- Unity/Windows certification cannot be claimed or scheduled against another owner's exclusive runner reservation. The registry must be reread on `main` and the actual workflow queue verified immediately before any shared-runner use.
- Zero paid credits. Neither Unity compilation, visual approval, commercial asset provenance certification nor release has been achieved.

## Continued implementation and objective art gate — 2026-10-10

- Verified completed Blender run **38064294083** / artifact **11674930485**; inspected the full 1152×768 rendered PNG rather than treating CI success as quality.
- Following commits implemented a physically enclosed lower gate/stone causeway, road-to-gate continuity checks, architectural supports, natural rock strata and occupied-wall detailing.
- Independently reproduced renders: **38064664786 SUCCESS** and **38064828771 SUCCESS** with full-size PNG, JSON and GLB artifacts. The current low-res image in `preview.jpg.base64.txt` was inspected directly.
- **ART GATE: FAIL**. Geometry is no longer just free-floating image-space fragments; the through-route is visibly more coherent. However large slab-like terrace geometry, angular rocks, simple roof volumes, shallow details, sparse vegetation and toy-block proportions remain far below the owner's Valoria image reference. Do **not** certify visual parity or promote to canonical Unity.
- This method (original family GLBs plus procedurally assembled blockout cuboids) is **not** the final art solution. Stop trying to add tiny material or prop patches to reach the target. A new authored high-detail architectural scene, controlled PBR texture pipeline, and terrain sculpt are required before re-entering the visual acceptance gate.
- Isolated Unity importer and manual QA workflow exist on the experiment branch, but **no Unity import or render was executed**. The Windows runner remains listed under the Region 1 owner's resource claim on current `main`; do not dispatch heavy Unity until that owner releases it.
- Zero paid credits; published game untouched; commercial rights of every included historical GLB still require individual provenance verification.
