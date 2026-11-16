# Ashen Citadel — recovered direction and pictorial pass

Owner 2026-11-16 accepted the composition and requested an attractive graphic identity with freedom of technique. This is the existing independent Ashen experiment, not Visual Reboot's separate three-state pilot.

## Recovered plan
Dark stylized medieval 2.5D; readable modular silhouettes, pictorial stone/wood/slate, warm side lighting against cool shadows, violet reserved for corruption. Validate original Bastion I–V and auxiliary buildings in actual Unity cameras, then coherent modular UI and mobile performance. Preserve authoritative mechanics and saves. No main integration or publication.

## This pass
An inhabited fortress at dusk: aged masonry, blue slate, muted conifers, warm window emission. Original composition, building geometry and game integration retained. Blender-authored bent foliage cards replace geometric pine masses; genuine transparent spruce-needle texture becomes a project material with alpha-test/depth/shadows. Separate opaque ground grass avoids applying tree alpha to terrain. Private Unity volume adds restrained bloom, split tone and neutral tonemapping.

Two local foliage approaches were rejected: separated curved volumes looked like spikes; smooth overlapping ellipsoids looked gummy. Changed technique to actual three-dimensional trunk plus bent textured foliage cards, inspected in Blender before native Unity.

## Authorship and reproducibility
`author_painterly_pine.py` authors the editable Blender source and exports OBJ/GLB, preserving explicit UVs and normals. Blender 4.3 used for authoring. Existing generator now preserves this owned mesh, and unwrap skips it. Source validators check closed trunk winding and upward foliage normals (open cards have no signed volume). Source/Blender CI can regenerate remaining assets without replacing authored pine.

`AshenNeedles.png` was generated using built-in image generation, no paid API, as an original isolated texture, not a scene or gameplay screenshot. Prompt: dense spruce branch spray, top-down orthographic, true transparent background, central brown twig bottom to top, tapered fan of overlapping needles, muted blue-green/teal, sage tips, painterly adult dark-medieval treatment, neutral diffuse albedo, no baked shadows, outline, glow or text. Original copied unmodified into the project; Unity import limited to 512 pixels.

## Verification boundaries
Local mesh/UV/PNG and winding validation passed. Blender study verifies forms/materials, not Unity lighting or mobile performance. Native Unity screenshots and gameplay tests must be inspected before claiming engine success. Artistic acceptance remains distinct from CI. Physical iPhone performance and full UI finishing remain separate gates.
