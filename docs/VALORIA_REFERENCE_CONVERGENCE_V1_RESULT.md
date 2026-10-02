# VALORIA REFERENCE CONVERGENCE v1 — RESULT

Date: 2026-10-02
Status: **TECH PASS / VISUAL FAIL vs approved reference**
Branch: `visual-proof/valoria-reference-convergence-v1`
Base: `visual-proof/valoria-master-visual-rebuild-v2`

## Objective

Push the certified Blank Canvas rebuild toward the approved Valoria reference using the existing library only.

## Toolchain

- profile: `environment_composition`
- Tripo spend: **0 credits**
- allow_tripo: false
- geometry_gap_proven: **true**
- official validation: zoom 19 / 12 / 9 / mobile
- gameplay collider/hotspot signature: **unchanged**

## Final evidence

- Run: `36994280995` — SUCCESS
- Artifact: `11220589620`
- Digest: `sha256:fb8f2c0596876cae10d113b296f9796090e5badfefbc465983d53f3379cb6113`

Measured scene:
- BEFORE: 1,108,222 triangles / 163 renderers / 181 material slots / 4 lights
- AFTER: 1,574,237 triangles / 358 renderers / 430 material slots / 7 lights

## What improved

- stronger vertical composition around the Hero Bastion;
- deeper foreground with the existing Granero and support structures;
- more geological framing and terrace occupation;
- additional authored cobble / retaining edges / vegetation / reconstruction props;
- denser central read without resuming lateral residential sprawl;
- 0 gameplay/collider/hotspot changes.

## Why this is still a visual fail

The approved reference relies on a large-scale architectural/environmental motif that the current library does not contain at adequate quality:

**monumental broken-arch / ancient ruin masses physically fused with vertical cliff-rock.**

Attempts to approximate the role with:
- TowerWallRock,
- Stone Gate/Tower,
- existing wall/passages,
- generic Gothic arch inventory,
- rock + terrace overlap

either read as placed modules, become too blocky, or do not create the continuous ruin-cliff silhouette of the reference.

This is now a **proven geometry gap**, not an unresolved composition problem.

## Exact next asset role

Create one reusable hero environment module:

`Valoria_AncientRuinCliffArch_v1`

Required read:
- single colossal broken stone arch / aqueduct remnant;
- asymmetric ruined crown and missing chunks;
- substantial cliff-rock fused into the base and sides;
- believable thickness, buttresses and masonry breakup;
- enough negative space through the arch to frame the distant world;
- medieval-fantasy stone language compatible with Hero Bastion;
- isolated production reference, not concept art and not a whole-scene image;
- reusable mirrored / rotated on the left and right outer frame.

No Tripo generation may start until the exact input image is persisted, staged, visible cost is known, and the owner gives fresh explicit approval.
