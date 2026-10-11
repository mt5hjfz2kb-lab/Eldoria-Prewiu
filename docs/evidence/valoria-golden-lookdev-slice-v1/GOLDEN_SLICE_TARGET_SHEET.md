# VALORIA GOLDEN LOOKDEV SLICE v1 — GOLDEN SLICE TARGET SHEET

Status: **CANONICAL TARGET TRANSLATION**
Date: 2026-10-05

Authority chain:
`references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg` → this target sheet → bounded Golden Slice implementation.

This sheet translates the canonical premium fantasy 4X reference into measurable screen-space goals for the real Lower Gate slice. It does not redesign the camera, macro composition, family positions, gameplay or global platform.

## Judged slice

Lower Gate front/edge + adjacent rock/cliff contact + one ground contact field + bridge/water/shore contact + existing vegetation framing. Official orthographic yaw20 / pitch35 camera family remains locked. Primary diagnostic view is the explicit `UNITY-golden-close` crop; 16:9, mobile landscape, 3:2 and relevant portrait views remain regression views.

## Value hierarchy
- Gate sun-facing stone: **0.58–0.76 perceived value**; readable as warm limestone, not near-white.
- Gate recesses/joints/contact occlusion: **0.22–0.42**.
- Ground: **0.38–0.58**, always lower than hero stone.
- Rock: **0.30–0.52**, cooler/darker than architectural stone.
- Vegetation canopy: **0.25–0.58** with trunk at **0.18–0.34**.
- Water: shallow **0.38–0.52**, deep **0.22–0.38**.
- No dominant screen-space surface may sit at one nearly uniform value.

## Warm / cool relationship
- Architecture + dry soil: warm neutral / muted ochre.
- Rock: slightly cooler taupe/grey than architecture.
- Water + distant atmosphere: cool cyan/blue-grey.
- Vegetation: muted green with controlled warm light-facing tips; no mint-uniform canopy.
- Warm foreground against cooler water/distance is the principal depth separation.

## Material hue families
- **Stone:** warm limestone / sandstone; low chroma beige-gold, never orange, never chalk-white.
- **Rock:** cooler grey-brown/taupe with localized warm reflected light.
- **Ground:** muted earth/olive-brown with restrained dry dust variation.
- **Water:** desaturated teal/cyan; deeper water cooler/darker.
- **Wet contact:** same local material hue, 12–22% darker value and visibly smoother response; not a black outline.

## Shadow density / softness
- Key shadows must be clearly readable without becoming hard polygon bands.
- Contact regions target 18–32% darker than adjacent lit planes.
- Large cast shadows retain visible material information.
- Penumbra should read soft enough for stylized realism; no razor-edged greybox look.
- Ambient fill must preserve underside readability without flattening form.

## Ambient / indirect strategy
- Warm-neutral fill on foreground architecture/ground.
- Cooler/quieter environment contribution toward water/distance.
- Contact AO only reinforces real interfaces; it may not draw artificial dark seams.

## Edge wear policy
- Wear is selective and scale-aware: corners, threshold traffic, lower plinths and exposed rock ridges.
- Edge wear value lift: typically 4–10%, not a white outline.
- No uniform edge highlight on every block.
- At gameplay distance, wear must collapse into mid-frequency breakup rather than noise.

## Dirt / contact policy
- Dirt accumulates at wall bases, road edges, creases and protected concavities.
- Contact masks must be irregular and material-aware.
- No broad flat “dirt polygons”; no visible patch perimeter.
- Contact darkening width should remain screen-space subordinate to the underlying form.

## Wetness policy
- Shore/rock wetness: localized darkening + reduced roughness.
- Wet transition must follow water contact/topography and remain irregular.
- No continuous painted ring around the shore.
- Architecture receives no wetness unless physically connected to water/splash zone.

## Vegetation hierarchy
- Trunks materially darker and warmer than canopy.
- Canopy has at least three readable value/hue bands under the key.
- Tree grounding must have contact shadow / local occlusion.
- Existing instances only; density is not a quality lever in this gate.

## Roughness hierarchy
- Dry ground: **0.78–0.94 perceptual roughness**.
- Rock: **0.72–0.90** with localized ridge variation.
- Architectural stone: **0.58–0.80**.
- Worn stone edges: **0.46–0.64**.
- Wet rock/shore: **0.30–0.52**.
- Water: controlled smooth response; no mirror sheet.

## Specular hierarchy
- Water/wet contacts carry the strongest restrained specular.
- Worn stone edges may catch the key subtly.
- Dry earth/rock should not sparkle.
- Specular must separate materials without becoming a highlight effect.

## Atmospheric depth
- Golden close crop: minimal fog influence; material truth dominates.
- 16:9 / 3:2: cooler/quieter distance layer with restrained atmospheric compression.
- Fog may never veil the hero slice or erase local contrast.
- Background is subordinate and cooler than foreground.

## Detail-frequency target
- **Macro:** Gate/bridge/rock silhouettes and broad terrain planes.
- **Mid-frequency:** 0.25–1.5 m scale variation: stone blocks, rock breaks, soil patches, foundation transitions, shoreline irregularity. This frequency MUST be visible at mobile landscape.
- **Micro:** 2–12 cm scale normal/roughness grain visible only in close crop and high-resolution landscape; it must not shimmer at gameplay distance.
- No surface may jump directly from macro shape to a uniform flat color.

## Mobile contrast / readability
- Lower Gate opening remains the darkest focal recess.
- Gate silhouette and road/bridge route remain immediately legible.
- Fine textures must not create moiré or noisy high-frequency contrast.
- Material boundaries must remain readable after downscale to 1280×720.
- Mobile core metric target: >=4/5.

## Golden acceptance thresholds
The slice cannot pass unless:
- material richness >=4/5
- lighting depth >=4/5
- contact quality >=4/5
- terrain/architecture integration >=4/5
- water/shore integration >=4/5
- natural integration >=4/5
- premium perception >=4/5
- mobile readability >=4/5
- gameplay regression PASS
- performance sanity PASS

Green CI is not visual evidence.
