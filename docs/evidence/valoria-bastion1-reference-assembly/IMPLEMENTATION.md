# Bastion I reference assembly — 2026-10-12

Owner request: construct the initial Valoria scene using the eight existing authored assets, then migrate its presentation into main when validated. Keep the concurrent WebGL/APK delivery work independent.

## Concrete candidate

`tools/valoria-bastion1-assembly/assemble.py` assembles a lower gate, stone approach bridge, a broad middle construction terrace, an axial paved route, stairs, a raised rear terrace and upper fortification. All eight V3 assets are used, unchanged at source. Bridge and stair models are reused from the existing committed families. The Hero V3 architecture is preserved. Source SHA-256, actual placements, material changes and output hashes are recorded in the generated manifest.

Identical material groups are joined within each source copy, then reused as linked meshes across placements. Texture images are shared and limited to 1024 pixels in this candidate; the original high-resolution inputs remain intact. This is a measured size/renderer experiment, not a mobile performance certificate.

The flat terrace supports and water are labelled PROTOTYPE. Existing Emace vegetation meshes are reused with preview palette materials; their original production texture treatment must be restored during Unity finishing. No completed Sawmill, Barracks or Granary is introduced. This candidate does not fabricate progression, rewards or player state.

The Unity editor generator imports the exact assembled GLB, captures landscape/mobile landscape/portrait, saves one persistent candidate scene with shared dependency assets and reports renderer/mesh/material/texture/triangle counts. It does not alter canonical scenes, build settings or public deployments. Desktop comparative pipeline settings are restored after the run.

## SHARP decision and main migration

**Choose staged replacement by complete scene, not wholesale deletion.** SHARP currently supplies state-dependent city imagery through `ValoriaParcelPresentation`, with native `GsplatRenderer` and the WebGL frame/state loader. Its camera uses image-plane coordinates and screen-projected parcel targeting. Dropping a world-unit mesh scene into those camera/selection methods would break Home, pan, picking and construction presentation.

Promote only the bounded new implementation files, not the entire integration branch. The integration branch also contains concurrent transport/build changes; its ancestry is not permission to merge those changes.

1. Review exact Unity output against the owner reference. Resolve terraced cliff joins, road/stair contacts, flag materials, vegetation and the main route; external artistic direction must be executed and recorded before approving new terrain artwork.
2. Add an explicit mesh presentation mode to the existing `ValoriaParcelPresentation` facade, while retaining its `Apply(PlayerState)`, parcel `Binding`s and current economy/persistence ownership. Introduce a world-camera adapter for Home/pan/zoom and colliders tied to canonical BuildingId, not decorative module indices. Keep the SHARP route as rollback until the replacement passes.
3. In the mesh mode, do not attach/request SHARP/WebGL state loaders. Do not load both full environments. Existing visibility/interaction progression gates still drive empty/construction/built states; the eight fortification pieces do not supply every future building.
4. Focused Unity tests: fresh Bastion I exclusions; Sawmill construction completion; Bastion II Barracks unlock/completion; resource integrity; exact reload; reset; full four-way pan; landscape/portrait camera; world return; actual building picking. Compare before/after at matched cameras. Profile actual Android/iOS separately.
5. Reconcile live main and the registry, cherry-pick this bounded candidate and the tested adapter changes, then update visual-authority routing to the exact approved successor. Publish only through the existing authorized release process. Remove unused SHARP runtime dependencies from builds only after that cutover; preserve source/history for recovery.

## Reproduction

Stage inputs without changing another branch:

```sh
python tools/valoria-bastion1-assembly/stage.py /tmp/bastion1-inputs
blender -b --threads 6 --python-exit-code 1 --python tools/valoria-bastion1-assembly/assemble.py -- --inputs /tmp/bastion1-inputs --output /tmp/bastion1-source --samples 24
```

Copy the generated GLB into `Unity/Assets/Eldoria/Experimental/Bastion1Assembly/GeneratedSource/`. Run `Eldoria.EditorTools.Bastion1ReferenceCapture.Capture` in the existing Unity 6000.3.23f1 project. The branch-specific workflow performs both steps on the existing Windows runner, after jobs already occupying that runner; it never cancels another job or publishes.

## Evidence status

Source composition implementation is present. No ART PASS, external Qwen inference, gameplay regression PASS, physical-device PASS or main cutover is claimed by this document. Final status must be updated from actual inspected outputs, not workflow success alone.

Primary technical references consulted:
- Unity 6 GPU instancing / SRP batching: https://docs.unity.com/en-us/engine/6000.0/manual/analysis/graphics-performance-profiling/reduce-draw-calls/gpuinstancing
- Unity glTFast source/documentation: https://github.com/Unity-Technologies/com.unity.cloud.gltfast
