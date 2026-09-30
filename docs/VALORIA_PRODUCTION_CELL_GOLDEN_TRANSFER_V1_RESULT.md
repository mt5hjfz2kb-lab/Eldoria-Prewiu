# Valoria Production Cell — Golden Transfer v1 — Final Result

Date: 2026-10-01  
Base main HEAD: `9da483e4ea1b9780b1afc22ad610c6c0289ed0f9`  
Experimental branch: `visual-proof/valoria-production-cell-golden-transfer-v1`

## Final verdict

**TRANSFER PASS / NO MERGE.**

This experiment answers the transfer question successfully:

> Can the visual language proven in the Golden Cell be recreated on the real production Valoria scene without changing gameplay authority?

**Yes.**

It does **not** answer the production-promotion question:

> Is this exact implementation ready to be merged and propagated citywide?

**No.**

## Canonical evidence

Successful transfer run: **36786910345 — SUCCESS**  
Artifact: **11129683214**  
Validated source HEAD: `ad32df3609f0162c2384c9494b0650dd02d2d74a`

Golden Cell reference used for fidelity comparison:

- validated visual checkpoint: `3b5764ea786af415fb918e98da83dd7f893d715e`
- run: **36783595411 — SUCCESS**
- artifact: **11129220413**

The production AFTER reproduces the useful Golden Cell result at the official framings:

- orthographic 19;
- orthographic 12;
- orthographic 9;
- mobile 390×844.

## Gameplay / topology safety

- same-scene BEFORE / AFTER: **true**
- collider + hotspot signature equal: **true**
- gameplay topology changed: **false**
- Tripo credits: **0**
- paid assets: **0**

The experiment therefore proves that Eldoria can replace or overlay the visual layer of a real production cell while leaving authoritative gameplay intact.

## Render metrics

| Metric | BEFORE | AFTER | Delta |
| --- | ---: | ---: | ---: |
| triangles | 1,574,646 | 1,594,962 | +20,316 |
| active renderers | 1,199 | 1,588 | +389 |
| unique materials | 629 | 627 | -2 |
| lights | 22 | 29 | +7 |

Interpretation:

- triangle growth is modest for the local visual gain;
- unique materials do not increase;
- **renderer growth is too high for a citywide production pattern**;
- editor counts are not a substitute for Android device profiling.

So the visual recipe transfers, but the current helper-heavy implementation must be rebuilt more efficiently.

## What transferred successfully

The useful visual gain comes from the combination of:

1. coherent PBR stone / ground / wood / roof / metal / plaster / moss families;
2. processional stair → terrace → fortified focal hierarchy;
3. rock ↔ architecture transition instead of flat placement;
4. human-scale residential/productive support;
5. restrained blue/gold identity;
6. controlled warm local light against cooler ambience;
7. freedom to change the visual shell while preserving gameplay objects underneath.

This is the production lesson.

## What must NOT be copied into production

Do not propagate:

- primitive/bevel helper construction as final architecture;
- one GameObject/renderer per small edge/detail;
- the experimental point-light count;
- hand-authored one-off placement as a citywide authoring model;
- the current visual implementation merely because transfer fidelity passed.

The +389 active renderer delta is the clearest warning.

## Production conversion required

Before any merge, rebuild the Golden/Transfer recipe as reusable production systems:

### A. Surface families
Create shared production materials for:

- fortress stone;
- retaining / dark foundation stone;
- cobble / paved ground;
- packed earth;
- timber;
- slate / roof;
- metal;
- plaster;
- moss / rock transition.

Use shared materials, atlas/trim approaches where appropriate, and measured variation rather than unique material proliferation.

### B. Architecture modules
Replace helper blocks with reusable authored modules for:

- Bastion/access façade;
- gate/arch;
- retaining wall;
- stair/landing;
- terrace edge;
- rock ↔ wall seam;
- residential frontage;
- productive frontage.

The modules must be composable under the existing gameplay topology.

### C. Renderer reduction
The production proof must substantially reduce renderer count versus this transfer while preserving the visual read. Candidate techniques include:

- combined modular meshes where logical;
- shared material batching;
- GPU instancing for repeated props/vegetation where beneficial;
- fewer helper edge objects;
- authored mesh detail instead of many tiny primitives.

Do not adopt any optimization blindly; measure the actual result.

### D. Same acceptance discipline
Every production conversion still requires:

- official cameras 19 / 12 / 9 / mobile;
- same-scene BEFORE / AFTER;
- unchanged collider/hotspot signature;
- visual review before citywide propagation;
- performance profiling only after the visual bar is retained.

## Rejected escalation

The later complete CC0 hero-fort glTF experiment on the Golden branch was technically valid but visually worse:

- head: `7936001ed2862990dd88724b0aadda0f6143ddaf`
- run: **36786986339 — SUCCESS**
- artifact: **11130336527**

It fragmented the focal architecture and weakened silhouette/hierarchy.

Therefore:

> More authored geometry is not automatically better. Production modules must preserve the composition proven by the best Golden Cell.

## Main disposition

Nothing from this branch is promoted to `main`.

- transfer fidelity: **PASS**
- gameplay safety: **PASS**
- exact implementation production readiness: **FAIL**
- merge: **NO**
- citywide propagation: **NO**
- next step: **rebuild the proven recipe as reusable, renderer-efficient production modules**

This branch is now a transfer reference, not production content.
