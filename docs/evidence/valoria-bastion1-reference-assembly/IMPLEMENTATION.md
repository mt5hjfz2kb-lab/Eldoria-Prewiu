# Bastion I reference assembly — 2026-10-12

Owner request: construct the initial Valoria scene using the eight existing authored assets, then migrate its presentation into main when validated. Keep the concurrent WebGL/APK delivery work independent.

## Concrete candidate

`tools/valoria-bastion1-assembly/assemble.py` assembles a lower gate, stone approach bridge, a broad middle construction terrace, an axial paved route, stairs, a raised rear terrace and upper fortification. All eight V3 assets are used, unchanged at source. Bridge and stair models are reused from the existing committed families. The complete Hero V3 architecture is preserved at the lower gate. For the upper bastion, authored portal objects are copied as a subset, paired with the existing watchtower to match the reference single dominant tower left of the arch. No source mesh is cut or overwritten. A smaller portal sits further left; short wings and detached lower remnants preserve the open central reconstruction yard. Source SHA-256, actual placements, material changes and output hashes are recorded in the generated manifest.

Identical material groups are joined within each source copy, then reused as linked meshes across placements. Texture images are shared and limited to 1024 pixels in this candidate; the original high-resolution inputs remain intact. This is a measured size/renderer experiment, not a mobile performance certificate.

The flat terrace supports and water are labelled PROTOTYPE. Existing Holotna Mountain pine geometry and alpha atlas are reused; its species color is baked to green for portable glTF. Existing Emace bush meshes use provisional palette materials. No completed Sawmill, Barracks or Granary is introduced. This candidate does not fabricate progression, rewards or player state.

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

The owner rejected the first architecture as too unlike the reference. Revision 4 removes both distant flank towers, the repeated upper double-tower gate, the mirrored connected lower wall and unnecessary continuous wings. It retains the lower two-tower gate, creates the upper asymmetric hierarchy from existing authored parts, and leaves detached lower wall remnants around a broad open yard. Local Blender composition was visually inspected and source hashes remain exact. Revision 4 self-contained GLB is 51,903,572 bytes; structural audit has zero errors/warnings. Its 254 shared meshes include unjoined copied portal objects; this is not a mobile optimization certificate.

Unity runs 38284402710, 38285712056 and 38287194015 are rejected as visual evidence: successful scene saving and CI did not repair texture/render corruption. Revision 4 exact Unity run 38288516982 uses isolated D3D11 rendering and resets rendering debug state. Its first landscape frame still uses wrong render colors, while subsequent mobile landscape and portrait frames show correct source material colors. These genuine frames were visually inspected. This sequence identified shader readiness at the capture boundary; synchronous exact-variant compilation and discarded GPU setup renders were verified in run 38289244341. All three actual engine frames now show correct materials and were individually inspected; the exact persistent Unity scene and shared dependencies are saved in artifact 11765245348. The architectural correction is delivered as a partial existing-asset construction assembly, without claiming full environmental ART PASS. No ART PASS, physical-device PASS or gameplay/main cutover is implied.

Primary technical references consulted:
- Unity 6 GPU instancing / SRP batching: https://docs.unity.com/en-us/engine/6000.0/manual/analysis/graphics-performance-profiling/reduce-draw-calls/gpuinstancing
- Unity glTFast source/documentation: https://github.com/Unity-Technologies/com.unity.cloud.gltfast

## Safe introduction into main

The owner authorizes main as the construction source. The generator and inspection camera were introduced in main commit `f71d20e59b47726de5ec299a451866253b2559d4` under `Unity/Assets/Eldoria/ArtTests/Bastion1Assembly/`, with source staging tools and this record. This is an inactive, explicitly provisional art candidate. It does not replace the runtime city, visual routing, player camera, economy, save data or UI. Candidate artifacts remain separate from player downloads. `ART PASS` and integrated I–II regressions are required for a later active scene cutover; adding the editor construction tools is not that cutover. The same script GUIDs are preserved when moving the candidate folder. Main architecture, workflow governance, quarantine and source/editor gating workflows passed on that commit. The source/editor gate intentionally skips heavy ArtTests-only edits; actual Unity compilation/render evidence is produced on the isolated art branch with the same implementation and the Experimental folder location. This is not a main player rebuild.

To build the scene from main, run the manual **Bastion I reference assembly candidate** workflow with main selected, or stage/assemble the source locally, copy the GLB into `Unity/Assets/Eldoria/ArtTests/Bastion1Assembly/GeneratedSource/`, generate explicit defaults with `materials.py`, and use the Unity menu **Eldoria > Art > Build Bastion I reference assembly candidate**. The editor produces a persistent `Bastion1ReferenceAssembly.unity` with shared dependency assets; generated files are evidence artifacts rather than duplicated player downloads.

## Delivered correction and remaining scope

The corrected architectural hierarchy and inactive main authoring tools are delivered. Actual player startup still uses SHARP. No player publication, economy/progression change, active scene routing replacement or mobile performance claim occurred. The missing organic cliff/terrain treatment and distant mountain environment are recorded, not fabricated from unmatched defensive assets. A later active startup replacement must implement the camera/picking/state adapter and pass the existing I–II gameplay regressions.
