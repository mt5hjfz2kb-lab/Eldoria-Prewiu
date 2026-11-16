# Masonry and structural massing

Owner requests correction of repeated masonry and rectangular wall masses observed in native source 16761f13eaf6ba12059ccae73b50852f3ddc3aa1, run 43851110708. Continue adapting the existing reviewed Ashen family, not importing the previously inventoried Aserradero_AP2/Cuartel_AP2/Granero_BIII or rejected legacy Bastion. Same scene camera, lighting, spacing, mechanics and I–V progression.

Cause: plaster incorrectly shares the stone block texture in both Blender and Unity. Stone uses equal-height rows in a short repeating tile. Main walls are almost cuboids despite 13cm corner chamfers. Small decoration does not address either defect.

Directed changes: separate quiet lime plaster; larger stone repeat with unequal row heights and individual block widths, subdued mortar and mineral shading. Grounded masonry masses gain broad chamfered corners and a continuous battered lower wall (no separate base/platform). Secondary timber buildings gain coherent side framing and diagonal braces; keep roof footprints and selection bounds. Preserve prior recognizable silhouettes.

Validation: deterministic regeneration, source/UV/normals, Blender structural/render inspection, same-camera Unity I–V and portrait comparison, existing gameplay tests. No final art approval or physical-phone claim from code checks.

## Implemented and tested; native visual inspection interrupted

Geometry/material source 259bfa78037216cdc91da41c739ef7ceda9d6491. Deterministic local regeneration, source/UV/winding checks and Blender I/V rendering passed; inspected Blender V. Full five-level Blender workflow43854084876 SUCCESS, artifact14221420491. C# source check43854084863 SUCCESS.

Exact native source bd69a2657c6bc32b5df5641824c7aa9380838243, run43854590011 attempt2 SUCCESS. Attempt1 failed before Windows scheduling during concurrent lease release; it made no Unity execution. Attempt2 logs confirm editor exit0, 7/7 EditMode and4/4 PlayMode, zero failures/skips. Artifact14220747003 holds real PNGs, scene and XML; advertised archive digest4d8438d0452d1f14afbf2aac52529fc5f7ef3588ab566c2af45ded4dd90b5b28 has NOT been locally verified because the execution environment became unavailable during download.

NEXT: download artifact14220747003, verify digest and native-proof image hashes, compare against native43851110708 at identical cameras. Do not claim final Unity visual improvement until inspected. No physical mobile validation or production merge/publication. Windows released after terminal run success. Cleanup emitted Remove-Item errors on run-local PackageCache; full scratch deletion not certified.
