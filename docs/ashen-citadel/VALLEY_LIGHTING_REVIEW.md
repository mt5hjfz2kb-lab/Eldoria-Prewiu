# Ashen valley lighting continuation — 2026-11-16

Existing workstream and owner; no production merge or rule changes.

## Investigated
Prior real Unity evidence b0390aa showed flat lighting, repetitive meadow, and minimal visible contact shadows. URP asset supports main-light shadows already; enabling shadows alone is not a demonstrated root-cause fix. Local Blender composition confirms raised peripheral terrain is feasible, but is not Unity evidence.

## Implemented
Continuous 512px ground map with broad/detail noise, worn approach and restrained foundation verge replaces a repeated 5m tile. Central plots remain planar; wooded terrain rises up to roughly 3m. City tree and rock placement uses the same height function. Private lighting uses a lower warm key, cooler restrained ambient fill, longer fog interval, explicit shadow bias/cascades and finite camera far plane.

A matched on/off directional-shadow capture records affected pixels and mean channel delta so a setting change cannot be misreported as functioning shadows.

## Source verification
Source integrity (12 meshes, 18 materials, 6 source textures and 4 sprites) passed; outward solid and roof-normal regression passed. Source CI and Blender CI at dd63fd76d2b6dff76372cf85617595183e074b92 passed.

## Native result
Candidate c9d008a16e473a39c1505247c10ff33052b60df0, run 43775821050: pending inspection at time of drafting. No final artistic approval implied.

Native 43775821050 failed before rendering: URP 17.3 shadow booleans are read-only and ShadowQuality is ambiguous. Corrected using the private pipeline serialized fields (verified in actual pipeline asset) and qualified UnityEngine enum at 8034917ca7efdc20215e56067136c2be4791501c. Retry 43776052927 did not execute Unity: live lease was transferred during checkout. Preserve exact corrected candidate and retry only the native job after resource returns.

## Verified native result

Exact code 8034917ca7efdc20215e56067136c2be4791501c, native run 43776052927 attempt 2: SUCCESS. Artifact 14187055106. Actual XML: 7/7 EditMode, 4/4 PlayMode. Matched shadow on/off measurement: 271354 of 1440000 pixels exceed a summed RGB delta of 12; mean absolute channel delta 5.5713/255. This proves a visible shadow contribution, not an artistic score or mobile performance target.

Landscape V and portrait V visually reviewed: exterior roofs remain opaque, directional shadows attach trees/towers to ground, sunlit walls read against cooler shaded courtyard, no lower empty strip. Ground repetition is removed; surface detail is still sparse. Foliage remains visibly faceted and the UI is unchanged. No full game, mobile physical-device, performance, or final art approval claim.

The previous failure taught us to verify writable API members, not infer setters from property names. URP 17.3 shadow support flags are edited only on the private pipeline clone via SerializedObject; production assets are not edited.

Portrait review caught a new interaction: the raised woodland rim can exceed the old 2m lower-ray safety clearance, exposing a thin bottom cut. The height function is bounded above by 3.04m; ce3422bb raises the shared runtime/capture clearance to 6m and the capture assertion to 5.99m. This is a camera safety correction preserving orthographic composition, not a geometry repaint. Final native verification pending the next shared turn.

## Final clearance evidence and delivery

Final tested code: 486282eca21e097ffd83c74ca3bbe25600017eb4. Native run 43777338073 SUCCESS, artifact 14186822421. Actual result XML: 7/7 EditMode and 4/4 PlayMode. Final shadow on/off diagnostic: 275994 pixels affected, mean channel delta 5.5406/255. Inspected the five-stage portrait sheet, Bastion V portrait and actual landscape comparison: the new bottom cut is absent, complete architecture and shadows remain visible. The combined lighting/terrain treatment is a reviewable improvement, not global ART_APPROVED. No change to production beta, domain, combat or persistence.

Deliverables are direct PNGs and labeled/resized comparisons of native Unity captures; no painted-over lighting. Last camera translation preserves orthographic framing but changes view-depth and shadow-cascade placement, so the A/B is a visual comparison, not a numerical identical-matrix benchmark. Terrain detail, faceted foliage and UI remain unfinished; physical mobile performance is unverified.
