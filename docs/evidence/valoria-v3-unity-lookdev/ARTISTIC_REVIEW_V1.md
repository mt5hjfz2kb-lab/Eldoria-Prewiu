# Valoria V3 / eight-asset Unity review — 2026-10-12

This records direct inspection of actual Unity `Camera.Render()` frames. Approved original GLBs: seven from `0abf13870d21725c30acf3f3e3a8c86385470073`, plus immutable Hero V3 SHA-256 `81e63b7269bef3180648b636ad7341a4373e800afd05527e8e8aa95d88d35e2f`. The source art decision is not a certificate for the destination scene.

## Exact engine evidence

- Previous source `c01867fb` / run `38188882212` / artifact `11720770593`: 30 images, only seven standalone A/B studies; Hero V3 in assembly, not standalone. Real Unity capture; technical pass, art destination fail.
- Eight-piece run `7f668647` / `38226442712` / `11736298865`: 36 frames including Hero V3 individual; successful engine execution. Better continuous wall, but broad artificial tabletop and weak cliff.
- Cliff exposure run `1ba6ef79` / `38227085782` / `11736729885`: all frames rendered but native Windows process exited `-1073741819` after save/exit; visual fail because three rocky slabs visibly float at the lip. Never treat this red run as technical success.
- Widened terrain and buried cliffs `e94085af` / `38227971890` / `11737631556`: 36 genuine PNGs and CI success. Floating slabs fixed; the front slope is grassy and the approved cliff silhouettes disappear. Still art fail.

Frames after the first run are 1440×960, plus 780×520 mobile-scale assembly. `previous-assembled-hero.png` represents the old layout from the same camera; `baseline-assembled-hero.png` and `lookdev-assembled-hero.png` hold the newly snapped geometry/camera constant and change materials/light/environment. Individual `baseline-<name>-hero/strategic.png` versus `lookdev-<name>-hero/strategic.png` keeps camera and source mesh/placement fixed. The environment is intentionally changed in lookdev; this is not a material-only comparison.

## Three main defects per original, visible at the player camera

| Piece | Defect 1 | Defect 2 | Defect 3 | Corrective ownership |
|---|---|---|---|---|
| wall_straight | repeated regular ashlar | broad unbroken mass | rigid modular ends | Blender for form, Unity for weathering/contact |
| watchtower | plain square silhouette | conspicuous exterior ladder | flat stone read at scale | Blender for architecture, Unity for shadow/material |
| wall_corner | oversized clean square corners | obvious flat end faces | segmented crenel rhythm | Blender for bonded end details |
| wall_connections | narrow isolated pier | square uncapped cut | weak transition into gate | Blender for joints; Unity for placement |
| stone_path_platform | looks laid atop ground | abrupt ends | paving too uniform | Blender for buried edge/variation; Unity for terrain roadbed |
| cliff_rocks | slab-like rectangular mass | grass crest horizontal band | little readable angular geology | Qwen-guided Blender geometry variant and matched Unity review |
| defensive_props | banners thin and stiff | tiny stylized flames | pieces spread loosely | Blender for prop structure; Unity for placement and selective warm light |
| hero_gate_v3 | very rectangular portal | dark/flat arch opening | high part count (1,037 renderers) | preserve approved source; use Unity grading/detail and profile before promotion |

The combined scene adds three deficits: insufficient hierarchy and inhabited detail versus the 9.5 reference; sloping terrain with a single grass read; sparse trees and weak near/far compositional depth. The eight pieces are correctly imported but are not a commercial medieval environment yet. No ARTISTIC BREAKTHROUGH PASS, no mobile performance certification, no main promotion.

## Controlled next cycle

`tools/art-rd/valoria_cliff_unity_variant.py` and `.github/workflows/valoria-cliff-unity-artist-variant.yml` make a separate, zero-paid-credit variant. Qwen views the actual Unity capture with the aspirational reference, decides geological amplitudes/facets/crest/talus, and BlenderMCP applies mesh changes to the approved .blend copy. Keep inference trace, source SHA, edit proof, before/after Blender renders, and variant .blend/GLB. It remains **unreviewed** until the variant passes real Unity A/B under the same camera. If Qwen/Blender fails, preserve the run evidence; do not label procedural output artist-approved. Never overwrite the frozen original.
