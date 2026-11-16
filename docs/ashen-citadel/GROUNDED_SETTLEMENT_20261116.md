# Grounded settlement refinement

Owner request: separate secondary buildings, remove platform appearance, address stone/ground repetition and add shared identity. Baseline real Unity run 43837348051, source 6080de8c8630da95c33cca59e79cb71bbba9cd0e.

Diagnosis: contourPlate generates explicit basalt side walls beneath all auxiliaries and a rim around the fortress. This is geometry, not a shadow issue. Remove those plates and extend structural footings into the continuous terrain. Preserve the granary's functional stone feet, not a display podium.

Directed changes: spread four auxiliaries to an irregular wider arrangement, connect entrances by soft worn paths outside fortress walls; avoid isolated circular plots. Keep strategic light and landscape capture camera unchanged; fit 46 metres of horizontal world extent in portrait to include the separated settlement. Visible workshop saw/sign, barracks chimney/shields, granary roof vent/barrels, mason workbench. Same slate, burgundy and brass family. Unequal elongated stone courses with lower mortar contrast replace square tiles.

Reuse search returned production sawmill/cuartel/granary. Continue adapting owner-reviewed Ashen family rather than substituting a mismatched kit. Original directed geometry/texture code, actual Blender import/render/editable sources and actual Unity evidence; no paid or third-party art. No gameplay changes; only presentation camera framing in runtime file.

Verification: geometry/UV/winding, source syntax, local Blender I/V, then real Unity all stages/landscape/portrait and existing gameplay tests. Physical-device validation remains separate. Capture first-frame anomaly workaround retained with diagnostic frames; no root-cause claim.

Local verification: all twelve source meshes, six material textures and four UI sprites passed integrity checks; all solids/roofs passed winding checks. Both modified C# files parse without syntax errors. Actual Blender I/V renders and editable .blend/GLB produced; no platform rims or structural floating visible. Auxiliary geometry clears wall envelope by approximately 5.6–7.0 metres. These are local structural observations, not native acceptance.

First native run 43839861481 succeeded (7 EditMode + 4 PlayMode), artifact 14211744255, source 1119feb9154756050986d70f2c2b07b8afa56d1a. Landscape confirms removed rims and separation. Portrait 43 still cropped left/right auxiliaries: rejected that framing, changed to 23/aspect orthographic size. Architecture/materials unchanged for final camera-only validation.
