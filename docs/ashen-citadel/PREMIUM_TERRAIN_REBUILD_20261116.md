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
