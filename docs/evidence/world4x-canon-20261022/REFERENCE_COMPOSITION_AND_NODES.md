# Reference composition and geographic render development

## Scope
The immutable original panorama is a design target, never a background plate. Coastline, nine river polylines, two lakes, fifteen paths, thirty-six groves and three low ridge positions are separately authored in world coordinates. Existing gameplay targets and camera positions are retained. Geometry is reversible for real same-camera before/after captures.

## Evidence and rejected alternatives
- Other-chat P0: real Unity run 39850201753, rejected for water/forest mask contamination, duplicated river/roads and erased Breach; useful principle of independent geography, pigment and distribution retained.
- Squeezed original atlas: run 39863502597, rejected for distortion. Explicit vector geography replaces that approach.
- Procedural ground camouflage: runs 39864636702 and 39865078502, rejected. Existing painted landcover retained.
- Directed geography blockout cf96ab6: run 39869039848, 7/7, good composition proof but buried Breach and schematic rivers. Not art approved.
- Geographic development 709e5bb: run 39869835824, 7/7, restores ground datum -4 and curved water/paths. Too elongated and flat.
- Reference proportions 47f251b: run 39870743739, 7/7, 24.0949814 seconds; revised vector proportions and actual-anchor Breach. Still too schematic.
- Guided terrain paintover: three built-in edit calls failed HTTP400 Unknown parameter reasoning. No output exists and no paid API fallback was used.

## Reference node art
Built-in image generation created a transparent three-cell atlas for the blue-roof fortress, pale angular quarry and neutral ruined watchtower. Source: generated_images/exec-27fa7113-2b9b-4d0f-929e-d45d03824e48.png, 2172x724. Prompt intent: isolated hand-painted isometric strategic game markers, blue-roof fortress, angular limestone quarry, ruined round watchtower, warm upper-left lighting, no labels, transparent background. Original alpha bounds retained through UVs, no destructive image cropping. Project blob 35a704ca91dceb697829d5c927fda92295aad002.

Runtime replacements attach to actual existing city, quarry, ruin and wood destinations; no targets, colliders or save fields are created. The existing animated Spawnling remains. No food/hunt target is invented. Full city construction scene is outside this art adapter. This is a 2.5D marker representation, not new fully modeled buildings.

## New geographic dressing trial
A separately rendered warm faceted cliff ribbon follows the authored shore. Three restrained rock shoulders and clustered ecological ground cover provide scale across the open terrain. A sparse small-tree transition layer connects the main grove groups. These are scene meshes, not a retouched screenshot. All are parented under the candidate geographic root and removed from the before control. The central gameplay and Breach clearance stay open.

## Acceptance
Technical test success is not artistic acceptance. No ART_APPROVED, merge, release or physical-device claim. The following native render must be inspected against the original reference and exact before/after camera metadata before retaining this trial.

## Completed native developments
- 22ea203: native 39872369256, 7/7, 23.6348886 s, artifact 12467131384; SHA256 1056926c8564bc11a61d2047b74cf0439c2d6289337c8b80b88568e9cd6da37d. All four A/B camera metadata pairs identical. City sprite visible, but uncovered legacy rock/ruin donors remained: incomplete replacement.
- 315944c: compile failure CS0819 at cliff vertex declaration. Fixed explicit Vector3 declarations; no test weakened.
- a063db9: native 39873035679, 7/7, 24.099133 s; artifact 12467570823, ZIP SHA256 946c20ad4df196d654b02f3155f3eac281f3426d16f4ac53255250c5743e92b0. Actual faceted coast and ecological transitions visible. Old quarry scarp still covered its replacement; addressed by exact renderer inventory.
- b961bd4: native 39873467132, 7/7, 23.7117504 s; artifact 12467253123, ZIP SHA256 223e457ea03c378462958b359563c0c9fe66e4b9cc92fe7c2204e80aa1f78082. Cloud banks replace straight fog boundary and broad pigment varies the ground. Revealed remaining extraction apron and near-ground overlap; neither is treated as acceptable final art.

The geographic reconstruction is useful: full silhouette, continuous river graph, correctly placed lakes, groves and actual-anchor scar fit the panorama without stretching the old atlas. However the old control still has richer painted water/terrain detail. The candidate improves composition and geometric continuity, not every aspect of finish. Further art approval must assess water treatment, richer terrain painting and atmosphere; these cannot be inferred from technical PASS.

- b54d58a: native 39874010630, 6/7. The only failure was the older test requiring the imported rectangular prairie to reappear near the camera. That is now the source of the visible overlap. The updated test requires the new supported continuous terrain to be visible, the old rectangle to be hidden, and the original to be fully restored by before mode, then hidden again by candidate mode. Collider preservation assertions remain. No gate was removed.
- The old near-only geometric fracture lines were also revealed when removing the raised old ground. The candidate now consistently uses its painted geological scar at all scales; before mode still selects the original behavior. Exact A/B camera metadata is rechecked after this change.

- 3c76c35: native 39874427278, 6/7. A new hierarchy-active assertion ran in the structural test before the actual world was entered (Boot starts in the city). It now checks enabled local candidate hierarchy there; the full activeInHierarchy check is additionally enforced in the capture test after genuine gateway progression and world entry. The unchanged real-world capture path remains mandatory. The 210m panorama scar was too prominent close to the city; its depiction now transitions smoothly from a compact 100m sheet to 210m between zoom80 and152, retaining the exact anchor and city-approach clearance. This is a visual representation change, not a new gameplay area or collider.

## Retained verified candidate — 23 October 2026 Europe/Madrid
Source 038c9f513a415c4dab282f632302a50c8d930241, tree 01bc0c6957aa2e8c5181b5371cb3a965cced360d.
Native Unity run https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39874832548 completed SUCCESS. Actual XML: 7/7 passed, 26.6555606 seconds. Artifact 12468311395, 15,933,517 bytes, ZIP SHA256 1a8d65c35f4f6aafa9de1fea366d7562aedb42b5d8e422e9c6899587ee012db5; full ZIP CRC checked.

Twelve genuine Unity PNGs were produced. All four regional/panoramic A/B pairs have identical camera xyz, quaternion, target, orthographic zoom, dimensions and aspect; only label and PNG hash differ. The comparison report embeds the original bytes, verifies eight paired PNG SHA256s and keeps the immutable original panorama as a separate artistic reference, never as a same-camera image. Its JavaScript syntax and embedded PNGs were validated; browser rendering was not automated because this runtime has no Chromium executable.

Visual review: retained as a useful composition candidate, NOT ART_APPROVED. Portrait now includes the authored region silhouette, two lakes, narrow river graph, grove groups and central scar. The coast has actual faceted relief. The old city/wood/quarry/ruin presentation is replaced without changing destination IDs; old quarry apron and the overlapping near-ground rectangle are hidden, and before mode restores the imported ground. Near scar lines no longer revert to the old geometric fallback. The compact near scar remains at the same actual anchor and grows smoothly only as a visual representation for the wide view.

Remaining major limitations: flat/diagrammatic inland water, insufficient painted ground richness compared with the immutable reference and parts of the old atlas control, simple ground-space cloud banks, lack of integrated contact lighting/shadows, and no new food/hunt gameplay targets. Landscape H244 deliberately retains the established camera and still crops the north/south ends of the elongated region; portrait456 contains the full silhouette. No claim of identical geographic visibility across orientations. Native tests are scoped structural/selection/camera evidence, not complete combat/save/UI regression or device profiling. No merge, external paid usage, publication or mobile certification.

Reproduce the standalone comparison with build_comparison_review.py <artifact_directory> 39874832548 038c9f513a415c4dab282f632302a50c8d930241 from the workspace holding the immutable originals. The HTML contains unretouched capture bytes and four native before/after sliders.
