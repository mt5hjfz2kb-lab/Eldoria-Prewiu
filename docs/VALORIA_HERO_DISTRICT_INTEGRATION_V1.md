# Valoria Hero District Integration v1 — final experimental proof

Closed: 2026-10-01.  
Branch: `visual-proof/hero-district-integration-v1`  
Final visual code: `c3f8a1261b36fedd60e88dc90646e85af5c71d50`  
Final run: **36829935987 — SUCCESS**  
Artifact: **11146672957**

Verdict: **TECH PASS / VISUAL PASS (scoped Hero District frame) / NOT PROMOTED TO PRODUCTION**.

## Scope and source identity

This proof keeps the already-approved Valoria Hero Bastion unchanged and improves only its immediate visible district.

Canonical optimized Hero Bastion:
- SHA-256: `afb6cee6ae572b0879650f18285b32798e263c17359a158ffe2bdd03fb62ad5c`
- 49,800 triangles
- canonical review orientation: yaw 180°
- Tripo credits in this block: **0**
- no regeneration, retopology or paid generation

The Hero Bastion is recovered from the certified artifact and verified by SHA before Unity capture.

## What changed

Zero-credit, visual-only integration around the existing gameplay topology:

- unified visible material response of the certified central stair and L1 landing;
- buried certified `BroadRockPlatform` and `SteppedRockTerrace` modules under the existing upper district to turn the former platform read into a mountain/terrace mass;
- certified `RockToWallTransition` modules at the Bastion/rock seams;
- restrained `HighStraightWall` retaining fragments immediately flanking the upper approach;
- two authored rock breaks at the stair foot;
- replaced only the immediate east procedural upper dwelling with an existing authored house visual;
- two restrained local warm lights; no global exposure trick.

No production scene, gameplay rule, route, hotspot or gameplay collider was moved or rewritten.

## Deterministic evidence

Matched same-scene BEFORE/AFTER captures exist for:
- zoom 19;
- zoom 12;
- zoom 9;
- 390×844 mobile.

Gameplay safety:
- `same_scene_before_after=true`
- `collider_hotspot_signature_equal=true`
- generated Hero Bastion colliders enabled: **false**
- generated Hero Bastion hotspots added: **false**
- legacy Bastion visual renderers suppressed: **19**

Hero bounds remained:
- centre: `(0, 7.14, 8.75)`
- size: `(12.8, 9.239, 10.962)`

## Metrics

BEFORE:
- 1,529,895 triangles
- 1,181 renderers
- 609 materials
- 22 lights

AFTER:
- 1,604,280 triangles
- 1,207 renderers
- 628 materials
- 24 lights

Delta:
- +74,385 triangles
- +26 renderers
- +19 materials
- +2 restrained local lights

The increase comes from already-certified existing geometry, not newly generated or paid assets.

## Visual verdict

**VISUAL PASS for the bounded Hero District frame.**

The improvement is larger than a cosmetic polish:
- the Hero Bastion no longer reads as a high-quality object sitting on a flat old platform;
- its rock base now continues into visible terrace/support masses;
- the lower physical stair, L1 landing, generated Bastion stair and gate form a substantially clearer vertical approach;
- retaining masonry and rock transitions create a continuous built-into-the-mountain read;
- the immediate right-side procedural building no longer competes with the Hero focal point;
- the improvement survives zoom 9 and the mobile crop and remains visible at zoom 12/19.

This does **not** mean the whole of Valoria is now at Hero Bastion quality. The lower/middle city remains the next visible category gap, outside this bounded proof.

## Decision

**A) This recipe can be extended to the next sector of Valoria.**

The repeatable zero-credit recipe is:

`hero-quality anchor -> preserve certified gameplay topology -> bury certified terrace/rock support -> explicit rock/masonry seam -> shared surface response -> replace only directly competing immediate architecture -> restrained local warmth -> matched official-camera gate`.

Do not promote this branch automatically. Owner visual review is required first.

`main` remained at `bfa38d7453fbcbed1439f96cb05238eb1820b593` at closeout.
