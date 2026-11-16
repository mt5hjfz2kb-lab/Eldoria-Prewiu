# Ashen Citadel — substantial premium terrain reconstruction

**Scope:** completely isolated art experiment `experiment/ashen-citadel-i-v-20261115`. Owner instruction: reject repeated micro-corrections; reconstruct premium-quality courtyard, path, environmental vegetation, and building–ground integration. No changes to main/gameplay, combat, persistence, or the separate World landscape.

## Why the former approach failed

The original city field was an almost-flat 3.6-m grid, with hundreds of small, regularly spaced rectangular cut stones. Changing their tint, joints, and falloff (native runs 43866203492 and 43870533193) did not make the environment read as a coherent inhabited settlement. That was verified by genuine same-camera Unity captures. The 11 passing tests showed only functionality, not premium artistic quality.

## Structural replacement — implemented, not just proposed

`AshenPremiumTerrainBuilder.cs` is an entirely new deterministic, editor-run geometry author. During isolated city scene construction only, it disables the legacy city terrain instance (without changing/deleting its source or the separate World view) and builds an **editable Unity mesh asset** named `AshenPremiumTerrain.asset`. It reuses the existing licensed/author-owned Ashen palette, textures, lighting, camera, fortress I–V and interactive building proxies.

The new scene comprises:

1. **Continuous, sculpted meadow:** a dense 120×120 quad terrain field, preserving Ashen's established macro heights, adding small-scale erosion only outside the load-bearing footprint and paths. One shared mesh avoids raised square platforms.
2. **A continuous compacted-earth approach:** curved ground ribbon near the gate, embedded into the common landscape, with a changing width rather than discrete rectangular steps.
3. **Genuinely fitted irregular flagstones:** seeded local Voronoi cells clipped against neighbours, varied and contracted to form uneven tight joints. Every stone has its own shallow top surface and beveled sides; density falls away from the gate toward the outskirts. Smaller courtyard and workshop entrance paving groups share the method. This replaces the old tiled rows.
4. **Purposeful ecological vegetation:** low grass/leaf silhouettes grouped in clearings, avoiding the main road, castle and work-area entrances. Rock outcrops are clustered at the landscape edge.
5. **Work-area traces:** scattered rough timber at the sawmill and chiselled stones outside the mason's yard, both resting on local sculpted soil rather than floating on artificial pads.

The source generator is seeded, preserves five structural Bastion variants, saves the resulting native Unity Mesh in the isolated project, and uses 8 submeshes sharing existing URP materials. It creates no paid dependency or third-party imagery.

## Quality and safety verification

- Code and geometry source are **implemented in branch**; the previous paved-ground proof is captured and documented.
- **Pending separately:** native C# compile and actual Unity run for this newly reconstructed ground; exact 1600×900 and 390×844 captures; 7 EditMode and 4 PlayMode tests; structural review for stone overlap, silhouette, shadow response, terrain edges, material repetition.
- **Unverified:** iPhone/WebGL GPU memory, FPS, battery consumption and final artistic approval.
- Protect the shared Windows runner: claim it via `pipeline/active-workstreams.json` only when free, release it when native execution finishes; do not cancel or hijack other experiments.
- Reject the rebuild if real captures reveal a worse result. Unity compilation alone is not artistic acceptance.

## Reconstruction invariants

Only the independent City view uses this mesh. Keep the old `AshenLandscape.obj` source unmodified because the separate World view still uses it. No legacy Bastion or main repository art is imported. Geometry is generated in Unity at edit-time and saved as an asset, not a mock-up, reference painting or dynamically generated image.

## Genuine Unity V2–V3 review and acceptance decision

- **V2** `c16b00a30c05fe69a468aa9229379b346eb1cad5`; native run **43883041688**, artifact **14232794149**: 7/7 EditMode and 4/4 PlayMode; ten image hashes verified. Sculpted/smoothed continuous terrain and blue-noise fitted stone paths replaced the wide repeated tiled carpet. However the ground-cover silhouettes were excessively black and read like bird-shaped spikes. **Visual regression rejected**, despite green tests.
- **V3** `d561872156bf0bc845f9ab585d01f0b2c02bfdce`; native run **43884660555**, artifact **14234376960** (ZIP digest `sha256:91e5120f56baecfe8e480f17b70d31849d383992e95eb1098ca01792ea00faa8`). 7/7 EditMode and 4/4 PlayMode passed, all ten I–V / 1600×900 and 390×844 screenshot hashes verified. Real same-camera Bastion V views independently reviewed.
- **Validated visual changes:** smooth terrain shading rather than faceted foreground ground triangles; organic tighter footpath with dirt shoulder; fewer rectangular pavers; removed oversimplified smooth-gray city pebble objects; separated vegetation into its own lightweight mesh with no shadow casting, soft natural colours and shorter blades; and reinforced activity-ground around workshops. The former stark black spiky plants no longer dominate the scene. Secondary architectural identity and Bastion evolution I–V preserved.
- **Quality judgement:** V3 is a substantial upgrade over the original repeating path and the failed V2 foliage. It is **not final premium-art approval**: meadow and distant scenery remain quite sparse, sunlight is still hard, buildings have some repeated roof/surface modules, and actual screen-space richness on portrait is limited. Do not confuse the 11 passed mechanics/tests with artistic quality.
- **Scope and environment:** the source lives only in `experiment/ashen-citadel-i-v-20261115`; City-only terrain edit-time mesh; separate World, production `main` and authoritative rules unchanged. Physical iPhone/WebGL, GPU and battery/performance profiling unverified. Shared Windows reservation released after V3 completed, no ongoing background job or promise.

### Lessons captured

1. Flat-shaded, triangle-duplicated meadow geometry caused obvious ground facets: explicit continuous finite-difference terrain normals solved it.
2. Even irregular polygon meshes can look like a carpet if row-and-column seeds and broad packed-earth rectangles dictate their silhouette: random rejection-sampled seeds, clipped octagonal Voronoi, and a narrower road work better.
3. Plant silhouette and material/shadow behaviour have to be evaluated as one system. Dark double-sided triangular grasses with mesh-wide shadow casting produced near-black 'bird' clumps; simply increasing saturation did not solve the cause. A separate, shadow-free material + smaller grass shapes yielded the clearer native image.
4. Rendering and automated tests are necessary but not sufficient. V2 technically passed but was visually rejected, then iterated to V3 without disturbing gameplay.
