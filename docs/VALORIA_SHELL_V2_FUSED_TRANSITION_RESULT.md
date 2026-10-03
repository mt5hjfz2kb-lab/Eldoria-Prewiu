# Valoria Visual Shell v2 — Fused Authored Transition Result

Date: 2026-10-03  
Status: **TECH PASS / VISUAL FAIL / NOT PROMOTED**

## Scope

This experiment tested a materially distinct Hero-to-city transition after repeated failure of:

- procedural TerrainData / native generated terrain;
- generated Blender heightfield/grid terrain;
- native extruded crags;
- rectangular terrace/deck compositions;
- loose low-detail cliff prefabs;
- repeated support-rock placement with only surface changes.

The candidate was built on experimental branch `visual-proof/valoria-shell-v2-fused-transition`.
No production gameplay code or canonical scene was promoted.

## Directed method

The operational problem was:

> Can the certified Valoria support rocks/terraces be fused into one manifold, camera-authored geological foundation so the lower city stops reading as separate islands/platforms?

The chosen method used Blender overlap + voxel remesh:

1. import already-certified `ResidentialTerraceRock`, `RockTerrainSeamFiller` and `TowerWallRock`;
2. overlap them in the real fixed-camera Hero-to-city footprint;
3. add only connective rock volumes and narrow civic road beds;
4. join and voxel-remesh the entire support mass into one continuous manifold shell;
5. decimate for fixed-camera/mobile proof;
6. import the resulting GLB as visual-only geometry;
7. apply the existing Valoria triplanar rock/ground shader in Unity;
8. retain the architecture-first city arrangement above the shell;
9. preserve the gameplay collision/hotspot signature.

This was not a terrain heightfield or another rectangular substrate.

## Technical evidence

Final candidate commit: `e88d821e1e00dce5249fc6ee445f51ee7a267e1d`

Final workflow:

- run: **37126944625**
- conclusion: **SUCCESS**
- capture artifact: **11275712744**
- Blender source artifact: **11274812687**

The pipeline proved:

- Blender imported the certified support sources successfully;
- voxel-remeshed fused geometry exported to GLB;
- Unity imported and rendered the fused shell;
- 19 / 12 / 9 / mobile capture set completed;
- collider/hotspot signature remained equal;
- gameplay ownership was not transferred to the visual shell;
- Tripo credits: **0**;
- paid assets: **0**.

Two earlier runs were technical setup failures only and are not visual attempts:

- `37126515629`: Ubuntu Blender glTF import lacked NumPy;
- `37126661052`: Unity compile failed because the composition helper did not receive `PlayerState`;
- `37126776522`: staging omitted the stone PBR maps required by the inherited retaining-wall path.

Those defects were corrected without changing the visual candidate.

## Visual review

The final full-frame evidence is a **VISUAL FAIL** against `docs/ELDORIA_VISUAL_BENCHMARK.md` and the approved reference.

What improved:

- the city base no longer reads as three rectangular deck slabs;
- more of the lower/middle district sits on a physically continuous support mass;
- the transition is less obviously a collection of disconnected terrain platforms.

What still fails:

- the complete composition still reads as a floating island in front of the photographic distant world;
- the fused geology has a single large blob/island silhouette instead of becoming a believable mountain-to-valley midground;
- Hero Bastion quality/material fidelity remains substantially above the surrounding city;
- lower architecture still reads as mixed asset families rather than one authored environment;
- retaining/circulation pieces remain visually separable instead of reading as infrastructure cut into the mountain;
- frame-edge/world continuity is not solved by merely making the support geometry manifold;
- residual disconnected presentation pieces remain visible in the wider frame.

Therefore manifold fusion solved a topology/continuity symptom, **not the actual full-frame environment-art problem**.

## Stop decision

Do **not** make a fused-transition v2 by:

- moving the same support lobes;
- changing voxel size;
- smoothing the edge;
- scaling the same rocks;
- adding more Mid-Tier prefabs to the fused blob.

That would be another micro-iteration of a failed family.

The problem is reclassified as:

> Build a complete coherent midground/world shell whose geology, frame edges, architecture seating and material quality belong to the Hero Bastion, rather than trying to repair an island-shaped substrate.

Historical Slavic proofs were also reviewed after this result. Their architecture is internally coherent but visibly lower-detail/stylized versus the Hero Bastion, so it is not a credible primary midground replacement.

## Next distinct method

Directed research identified zero-cost CC0 photogrammetric cliff/rock models as the next materially different method: hero-quality scanned geology forming the full midground, with existing Valoria architecture buried into it and the photographic image restricted to distant background only.

The next proof must use a small set of distinct high-quality scanned cliff/mountainside modules, preserve their PBR materials, extend the real 3D midground beyond the visible frame edge, and be judged in 19 / 12 / 9 / mobile before any material/lighting polish or production promotion.
