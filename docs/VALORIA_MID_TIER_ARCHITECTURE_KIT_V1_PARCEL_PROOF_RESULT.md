# Valoria Mid-Tier Architecture Kit v1 — Parcel Proof Final Result

Status: CLOSED — TECH PASS / VISUAL PASS
Date: 2026-10-01
Branch: `visual-proof/mid-tier-kit-parcel-proof-v1`
Certified run: `36865285702`
Evidence artifact: `11164212111`
Certified source commit: `744db9f0c8a2d1061c9b2e5d7d20aa3ea7cc66a6`

## Source integrity
- No new Tripo generation.
- Additional Tripo credits: **0**.
- Exact raw GLB reused: **68,231,740 bytes**.
- Raw SHA-256: `5e5432b78151a837eea1a77c2ea5783c543da10da747add208adfdc21cbf284a`.
- Reconstruction plane: `cluster_axes = yz`.

## Phase 1 — Piece 01 cleanup
Result: **CLEAN PASS**.

Conservative cleanup was limited to Piece 01. Pieces 02/03/04 were not cleaned or clipped.
The final accepted cleanup removed disconnected residue and used the already-supported post-join cleanup plus a controlled lower-side axis clip on Piece 01 only.

Final isolated pieces:
- Piece 01: 11,997 tris — arched entry / porch — CLEAN PASS.
- Piece 02: 13,201 tris — two-storey residential mass — VALID.
- Piece 03: 16,650 tris — workshop / facade mass — VALID.
- Piece 04: 6,324 tris — roof / upper-structure module — VALID.
- Total: 48,172 tris.

All four isolated Unity reviews retained UVs, normals, renderer/material, collider raycast hit and empty-space miss. Official front/rear/side/oblique captures exist for every piece.

## Phase 2 — real Mid/Lower parcel proof
One existing Mid/Lower front parcel was used. No production scene was modified and no production promotion occurred.

The recovered pieces were assembled as one architecture:
- cleaned Piece 01 as entrance/porch,
- Piece 02 as inhabited two-storey core,
- Piece 03 as secondary workshop mass,
- Piece 04 as upper/roof articulation,
- certified terrain/terrace and stone transition vocabulary,
- shared surface materials,
- one contained local light.

The proof replaced only the parcel visual layer. Gameplay topology, routes and interaction authority remained unchanged.

## Technical invariants
- collider/hotspot signature equal: **true**
- gameplay colliders added: **0**
- gameplay hotspots added: **0**
- Hero District preserved: **true**
- Hero Bastion bounds equal: **true**
- Tripo credits: **0**

## Metrics
| Metric | Baseline | After | Delta |
|---|---:|---:|---:|
| Triangles | 1,601,568 | 1,628,679 | +27,111 |
| Renderers | 1,201 | 1,208 | +7 |
| Materials | 622 | 625 | +3 |
| Lights | 24 | 25 | +1 |

## Official evidence
Baseline and after exist at:
- zoom 19
- zoom 12
- zoom 9
- mobile

The mobile and zoom-9 comparisons show the intended result clearly: the previous single Slavic-style block is replaced by a materially richer, asymmetric, terraced red-tile composition with stronger depth, roof silhouette, entrance hierarchy and rock/architecture relationship. It reads as a built urban parcel and is visibly closer to the Hero District / Hero Bastion language rather than merely being a technical substitution.

## Verdict
**TECH PASS**

**VISUAL PASS**

This is not a claim that all Mid/Lower Valoria is finished. It demonstrates that the recovered Mid-Tier Architecture Kit supplies the missing richer architectural core that the Modular Assembly System v1 lacked.

## Conclusion
**A) The recovered Mid-Tier Architecture Kit provides the required visual jump at parcel scale and is suitable to extend sector by sector, with each future sector still requiring its own visual gate.**

No automatic merge/promotion to `main` was performed.
