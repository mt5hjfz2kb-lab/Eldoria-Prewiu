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

## 2026-10-10 late continuation — revised real full-size proof

- Reviewed artifact 11674930485 (run 38064294083), full-resolution 1152×768 PNG and GLB. At this point the front gate and bridge approach were visually disconnected and terrain looked like a suspended slab.
- Created physical bridge parapets, abutment, front joining curtain wall and crenellations. Result 38064664786 technically SUCCESS; reviewed captured PNG: clear entry improvement, still simplified art.
- Added bedrock strata and visible keep detailing. Result 38064828771 technically SUCCESS; reviewed captured PNG: details improved, but quality still not approved.
- Routed existing locally stored masonry/wood/paving albedo textures into exportable glTF materials. Final reviewed run 38065087220 technically SUCCESS, artifact 11675261425, worldspace GLB included; source gate 38065087257 SUCCESS. No paid credits.
- **FINAL VISUAL VERDICT FOR THIS METHOD: FAIL**. Full-size reference comparison: angular cliff, straight toy-like ramparts, sparse settlement, crude foliage and missing atmospheric environment. Asset reuse and geometric bridging solved basic composition but the visual still does not meet Eldoria's commercial reference. Additional tiny prop tweaks on this generated-box method are not a sound route to visual parity.
- **Unity integration/QA: NOT RUN**. Main's live `world-region-1-visual-convergence-v3-20261010` still owns `windows-self-hosted-unity-6000-3-23f1`; new Publish Eldoria Preview run 38065203733 is in progress. Do not seize resources. The isolated `valoria-mesh-only-unity-qa.yml` remains manual-only and must wait for legitimate release **and** a visually approved source.
- **Next method**: authored high-detail mesh terrain and joined architectural forms with verified textures/materials, not compositing stock GLBs with cuboids; independently reproduce matched strategic-camera captures; only then stage this GLB in Unity through the prepared manual QA workflow and visually review the actual Unity screenshot. No change to `main` or published build.

## Owner-authorized two-track review — final inspected source proof

- Source commit: `683ff91bb188c39a06e3eb99c7221a6c52eef282`.
- Reproducible Blender run [38079353952](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38079353952): **SUCCESS**. Exact artifact [11679846513](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38079353952/artifacts/11679846513) contains full PNG, Blender source, GLB and independent structural audit.
- Implemented continuous hinterland, physical cottage roofs/facades, exportable authored surface textures and UVs, efficient direct-mesh trees and static mesh batching. Preserved the intervening stone-arch and heraldic changes from the existing prototype.
- After rejecting the simple keep mass, changed the architectural source by reusing the previously authorized Hero Bastion GLB. Original source is untouched; experimental import uses a derived 1024px texture cap. **No new generation, purchase or paid credits.**
- Exact audited export: **16,477,940 bytes; 27 meshes; 46 primitives; approximately 233,212 triangles; 34 materials; 20 embedded images; zero structural errors and warnings**. These are source metrics, not a mobile performance certification.
- **FINAL VISUAL REVIEW: FAIL against the approved Valoria reference.** The reused central bastion is substantially more detailed, but perimeter walls, repeated cottages, angular bedrock and geometric foliage remain visibly inconsistent and model-like. A successful export does not approve the city.
- **Unity import/render: NOT RUN for this revision.** The source still fails the required art gate; no claim of Unity visual parity, mobile performance or production readiness is made. The importer camera correction is prepared but unverified.
- **Iteration closed as a rejected feasibility candidate; project remains visually unfinished.** Do not promote this experimental GLB or continue incremental primitive decoration as a route to the approved target. Next meaningful work requires an authored coherent environment/architecture source and a matched-camera acceptance proof.
- Canonical SHARP city and published production remain protected.
