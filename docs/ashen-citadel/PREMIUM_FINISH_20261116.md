# Premium finish — material separation and contact depth

Owner request: improve premium finish of existing structural identity candidate. Same workstream, architecture, cameras, authoritative gameplay and isolated branch. Baseline exact native source 8897cbac2572fd746018041754813908f1082514 / run43861251720.

## Visual diagnosis and decision before authoring
The reviewed image has muted material separation, uniformly lit joints and roofs that read as flat colored panels. Preserve the new continuous roofs and recognizable silhouettes. Avoid another geometry micro-pass: this iteration targets material response and the missing contact shading.

- Derive subtle tangent-space normal maps from the already authored stone, plaster, slate and wood tiles; do not add external art or pretend this is sculpted geometry.
- Deepen cool slate and aged timber; retain warm stone and restrained brass. Give slate/metal separate smoothness.
- Clone the URP renderer as well as the pipeline; add a bounded, downsampled SSAO pass for intersections. No production renderer mutation.
- Reduce unshadowed fill and ambient flatness, increase key/shadow separation and modest contrast. Preserve camera, fog and architecture for actual A/B.
- Blender material preview includes matching tint and height relief. It does not simulate URP SSAO or certify Unity.

## Technical research
Reviewed actual Unity-Technologies Graphics 6000.3/staging ScreenSpaceAmbientOcclusion.cs and Unity 6000.3 SSAO documentation before implementation. Serialized settings verified against source; depth-normals source, medium samples, interleaved noise, downsample, intensity1.35 radius0.65 direct0.18. This is an isolated look-development candidate, not a mobile budget claim.

## Gates
Source integrity; Blender preview; actual native Unity compile/render/test; same-camera A/B; artifact hashes. Physical mobile profiling and final artistic approval remain separate.
