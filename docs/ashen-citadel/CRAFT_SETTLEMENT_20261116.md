# Craft settlement — macro redesign after owner rejection

Owner feedback: secondaries remain basic and read as appendages of the keep; enrich both architecture and ground. Baseline native source 86bccf9d708a0d75681ddb632ac5a7dad677ddeb / run 43840861529, branch experiment/ashen-citadel-i-v-20261115. Preserve castle I–V, spacing, camera and light for direct comparison. No gameplay, production merge or credits.

## Change of approach
The last pass improved distance and removed platforms, but small props did not overcome four similar roofed boxes. Stop adding trim to that template. Replace the secondary massing and material proportions by function:
- Sawmill: long open structural bay, two unequal weathered timber roof planes, exposed braces, horizontal trunks and sawing bench. No castle-blue main roof.
- Granary: low plaster/timber loading store plus a tall banded timber silo with conical wooden cap, ladder and barrels. Agriculture is recognizable in silhouette.
- Barracks: inhabited longhouse, stone lower wall and exposed timber/plaster upper wall, offset porch and wing, chimney and training target. No battlement tower.
- Stoneworks: an open canvas-covered workbench and braced derrick with suspended block and stone stock. No miniature stone house.

Retain restrained burgundy/brass insignia as cultural link; do not force every roof to match the keep. Ground: stronger meadow/earth tonal structure, specific sawdust/straw/chalk wear at entrances, grouped low shrubs and dry grasses, broken approach paving. No raised pads or rectangular plots. Collider coverage grows with the new meshes; click IDs and authoritative systems are unchanged.

## Authorship and reuse
Reuse query found current production Aserradero_AP2/Cuartel_AP2/Granero_BIII. Continue transforming the owner-reviewed Ashen assets instead of mixing art families. Geometry is actively directed and authored through generate.mjs, imported/rendered/exported in actual Blender, then checked in Unity. No claim of interactive BlenderMCP sculpting or externally generated original art. Existing material palette/textures reused.

## Acceptance evidence
Macro silhouette and actual Unity same-camera A/B, supports on ground, no cut-off auxiliaries at 390x844, all five castle stages preserved, source/UV/winding and existing 7+4 Unity tests. Blender is structural preview; physical mobile remains unverified. No self-issued ART_APPROVED.

Local verification: original fortress meshes unchanged; four new auxiliaries total 1,266 polygons (prior 915). Source integrity, winding and C# syntax passed. Blender V assembled scene rendered and editable source saved. Projected secondary extents stay within the existing 46m portrait width. Static inspection found paving initially below the 0.045m road ribbon: raise paving to 0.072m before final evidence; this changes only small ground faces.

First Unity run 43845247032 / c5a95d4026f5bcdd4b752fdab488730e90e66bf9 / artifact 14214833753: secondary massing reads distinctly and supports are grounded. Native image exposed shrub domes looking like glossy green rocks, so that treatment was visually rejected. Replace domes with overlapping individual leaf geometry and explicit matte, non-metallic, two-sided foliage material. Preserve building changes.

## Final native review
Source b7646dad75ecdb53b2a4c0792968c141cc630e6a; Unity run 43846509441 SUCCESS; artifact 14214989589. Seven EditMode and four PlayMode tests passed. All ten screenshot SHA256 values match native-proof.json. Final native I/IV/V landscapes and V portrait inspected: distinct craft silhouettes, no plinths, all secondaries within portrait, leaves read as matte foliage rather than green rocks, approach paving visible. Existing castle geometry/camera/light preserved. Blender regeneration/editable-source workflow 43846509550 SUCCESS and C# source contract 43846509519 SUCCESS.

Remaining artistic limits: distant heraldry and equipment remain small at 390px; ground colour still broad and smooth between activity areas; timber joinery and material surface detail can be refined. This is a substantial secondary-building redesign, not a claim of final commercial art quality. Physical iPhone/WebGL performance untested; no production merge or publication. Windows released on terminal completion; source owner remains active for owner review.

Accumulated lesson: details cannot overcome repeated primary massing. Different structural systems, roof planes and material proportions made functions readable. Closed low-poly foliage domes plus unspecified smoothness read as rocks in URP; open leaf geometry plus explicit matte material removed that failure in the final native frame. Ground overlays must sit above the authored road ribbon, not merely above the terrain.
