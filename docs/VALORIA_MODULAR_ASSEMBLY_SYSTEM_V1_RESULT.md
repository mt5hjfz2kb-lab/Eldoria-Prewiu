# Valoria Modular Assembly System v1 — final experimental result

Closed: 2026-10-01.  
Branch: `visual-proof/modular-assembly-system-v1`  
Canonical `main` checked before work: `5fc42539175b7872da4e370ea09e2b3f9478a6a7`  
Final visual code: `fe4d11d41aaafa147cbeb9fe2085fedd9e28c3ec`  
Final gate/workflow head: `00d0eb3b531e051cc2d1c3d0e684b7d97cf605c4`  
Final run: **36844327767 — SUCCESS**  
Artifact: **11152513788**  
Tripo credits: **0**

Verdict:

**TECH PASS**  
**MODULAR ASSEMBLY SYSTEM PASS**  
**VISUAL FAIL**  
**NOT PROMOTED TO PRODUCTION**

## Question answered

Can Alternative A's successful modular-composition recipe be turned into a reusable assembly grammar that creates several distinct real-parcel architectures without changing gameplay?

**Yes technically and compositionally, but the current existing architectural vocabulary does not yet scale to a whole-frame visual pass.**

The reusable system works. Five distinct assemblies were produced from one parametrized grammar and placed in real middle/lower Valoria parcels. They vary role, orientation, footprint, height, base, projection and masonry/rock integration without duplicating one exact building.

However, the final 19/12/9/mobile comparison does not show a sufficiently large category jump across the whole middle/lower district to justify sector-by-sector production rollout yet.

## Reusable grammar

Each assembly is expressed as:

`occupied core -> buried rock/terrace base -> optional masonry spine/corner -> optional rock-to-wall seam -> one functional projection -> shared material family + MPB tint -> restrained local occupied light`

Parameters include:
- core: house / shed;
- base: stepped / broad buried terrain;
- width / height;
- yaw / orientation;
- dominant side;
- optional HighStraightWall spine;
- optional CornerWallL mass;
- optional RockToWallTransition seam;
- projection: balcony / porch / civic gate / none;
- tint via MaterialPropertyBlock;
- local occupied light.

The final correction establishes the local-facing convention inherited from Alternative A: the occupied front is local +Z, retaining masonry belongs on local -Z. This prevents the masonry spine from reading as a wall pasted over the facade.

## Five real variants

1. **A1 secondary guardhouse** — front Cuartel parcel; stepped rock, occupied house core, rear masonry spine, seam, balcony.
2. **A2 terraced residence** — west lower parcel; broad buried base, occupied core, masonry/seam, porch, different yaw and larger residential footprint.
3. **A3 civic corner house** — east lower parcel; stepped base, corner masonry mass, seam, civic gate; different silhouette and civic reading.
4. **A4 workshop/residence hybrid** — west outer-middle parcel; shed core, broad base, seam and porch; intentionally omits the rear wall after visual review to avoid collage.
5. **A5 small fortified residence** — east outer-middle parcel; house core, stepped base, restrained wall/corner mass and gallery.

All assemblies are visual-only. Existing gameplay remains authoritative underneath.

## Material/scaling result

Alternative A previously needed many tint-specific adapted materials. This system changes that model:

- source-derived adapted materials are shared and cached;
- GPU instancing is enabled on adapted materials;
- per-assembly color variation is applied with MaterialPropertyBlock;
- no material is duplicated merely because a building uses a different tint.

Final measured frame:

| Metric | Baseline | Assembly system | Delta |
| --- | ---: | ---: | ---: |
| Triangles | 1,601,568 | 1,623,310 | +21,742 |
| Renderers | 1,201 | 1,110 | -91 |
| Materials | 622 | 558 | -64 |
| Lights | 24 | 29 | +5 |

Assembly system:
- active assemblies: **5**
- shared adapted surface materials cached: **107**
- Tripo credits: **0**

This demonstrates that the reusable material strategy improves scaling pressure even while five assemblies are active.

## Gameplay / technical gate

Final run **36844327767 — SUCCESS**.

Evidence:
- `collider_hotspot_signature_equal=true`
- `hero_district_preserved=true`
- `hero_bastion_bounds_equal=true`
- gameplay colliders added: **0**
- gameplay hotspots added: **0**
- Hero Bastion unchanged
- Hero District unchanged
- certified gameplay topology remains authoritative
- no Tripo spend
- no production scene promotion

A first over-broad visual suppression pass was rejected and corrected before final acceptance. The final gate scopes replacement to the selected real parcel neighborhoods instead of using broad citywide removal as a visual shortcut.

## Capture evidence

Final artifact contains deterministic:
- baseline zoom 19 / 12 / 9 / mobile;
- assembly-system zoom 19 / 12 / 9 / mobile;
- evidence JSON and Unity log.

Optional per-variant square captures were attempted. Repeated camera reconfiguration after the eight authoritative captures triggered a reproducible native Unity/URP graphics-driver crash (`Texture2D.Apply` / render submit stack) on the self-hosted runner. Because the authoritative full-frame captures had already rendered and the primary acceptance question is whole-frame quality, the unstable optional focus pass was removed from the final gate rather than weakening or falsifying the core evidence.

## Visual review

### What improved

- the front Cuartel parcel no longer reads as one isolated dedicated shell;
- several lower parcels now use the same rock / masonry / inhabited-core grammar;
- the masonry orientation correction removes the worst pasted-wall/collage failure;
- repeated tint no longer requires repeated material instances;
- the Hero Bastion remains clearly dominant;
- no five assemblies are exact clones: silhouette, orientation, base relationship, height and functional projection differ.

### Why VISUAL PASS is not awarded

At zoom 12, zoom 9 and mobile, the change is real but not large enough across the complete frame.

The dominant remaining problem is no longer assembly mechanics. It is the quality/variety ceiling of the **inhabited architectural core vocabulary** available to the grammar.

The existing Slavic house/shed cores still carry most of each mid-tier building's visible roof/body identity. Across several parcels this causes:
- repeated simple roof/body language;
- insufficient facade/corner/upper-storey richness;
- weak differentiation between residential, civic and workshop masses at gameplay scale;
- persistent visual gap from Hero District to lower city;
- a frame that still feels partly like placed pack buildings rather than one authored mountain city.

Burial, overlap, wall/rock seams, shared materials and local lights can integrate the bases, but they cannot manufacture missing mid-tier silhouette and facade capability from the current cores.

## Exact missing capability

Before any citywide rollout, the vocabulary needs a **small number of richer reusable mid-tier inhabited architectural capabilities**, not another complete monolithic building per parcel.

The missing functions are:
- a richer two-level inhabited core / upper-floor mass;
- a stronger reusable facade/entry mass;
- a roof/corner silhouette module that can vary the crown without looking like a pasted roof;
- optionally one richer workshop/civic core that is visibly distinct from the residential house.

These capabilities should be modular and shared by multiple assemblies. New geometry is not authorized by this result; this document only identifies the proven gap.

## Final decision

### Conclusion B

**Do not extend this current system sector by sector across Valoria yet.**

The **assembly framework itself is reusable and technically successful**, including shared materials and safe visual-only placement. But the **current existing asset vocabulary does not scale visually enough**: after five real-parcel applications, the full frame still fails the required category jump.

Keep the system and its grammar as the production candidate. Do not discard it and do not return to isolated prefab placement.

If a later block is authorized to add geometry, target only the exact missing mid-tier inhabited capabilities above, then feed those pieces into this same assembly system and repeat the 19/12/9/mobile frame gate.

No production integration was performed.
