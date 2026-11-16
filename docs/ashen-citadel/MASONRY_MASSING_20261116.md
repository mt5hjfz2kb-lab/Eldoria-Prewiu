# Masonry and structural massing

Owner requests correction of repeated masonry and rectangular wall masses observed in native source 16761f13eaf6ba12059ccae73b50852f3ddc3aa1, run 43851110708. Continue adapting the existing reviewed Ashen family, not importing the previously inventoried Aserradero_AP2/Cuartel_AP2/Granero_BIII or rejected legacy Bastion. Same scene camera, lighting, spacing, mechanics and I–V progression.

Cause: plaster incorrectly shares the stone block texture in both Blender and Unity. Stone uses equal-height rows in a short repeating tile. Main walls are almost cuboids despite 13cm corner chamfers. Small decoration does not address either defect.

Directed changes: separate quiet lime plaster; larger stone repeat with unequal row heights and individual block widths, subdued mortar and mineral shading. Grounded masonry masses gain broad chamfered corners and a continuous battered lower wall (no separate base/platform). Secondary timber buildings gain coherent side framing and diagonal braces; keep roof footprints and selection bounds. Preserve prior recognizable silhouettes.

Validation: deterministic regeneration, source/UV/normals, Blender structural/render inspection, same-camera Unity I–V and portrait comparison, existing gameplay tests. No final art approval or physical-phone claim from code checks.
