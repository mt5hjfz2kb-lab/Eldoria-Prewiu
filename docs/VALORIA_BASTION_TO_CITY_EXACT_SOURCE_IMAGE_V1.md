# VALORIA BASTION-TO-CITY EXACT SOURCE IMAGE v1 — RESULT

Date: 2026-10-04  
Status: **CLOSED**  
Final verdict: **TECH PASS / EXACT SOURCE IMAGE FAIL / 0 CREDITS / STOP**

## Authority

Work was reconstructed from live `main` at `0027d9ab8d1272884c02a272b0aed245a3713bfd` and the locked sources:

- `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`
- canonical `AFTER-HORIZONTAL-16x9.png` from run **37221732105**, artifact **11309549299**
- SHA-256 `ef45233a09199829c83f5cb3d5587753f5b93fa7b1e9ba726b633791b7e343c1`
- `docs/VALORIA_BASTION_TO_CITY_FRAME_DESIGN_BRIEF_V1.md`
- `docs/evidence/valoria-bastion-to-city-frame-design-brief-v1/target-spec.json`
- `docs/evidence/valoria-bastion-to-city-frame-design-brief-v1/target-overlay.svg`
- previous source-prep and Blender source-gate results.

## What was tested

The required method was treated literally: the canonical 1600×900 capture is the locked background and only the replacement envelope may change.

A direct image-edit attempt was rejected because the image generator re-rendered Valoria globally instead of performing a reliable local inpaint.

A deterministic fallback was then tested:

1. generate richer transition architecture only as donor material;
2. composite donor material only inside the locked replacement envelope;
3. restore every pixel outside the envelope from the canonical base;
4. restore the protected Hero Bastion, promoted lower city and central stair regions byte-for-byte from the canonical base;
5. derive one isolated crop from the same integrated solution.

## Technical preservation result

The fallback proves the background-lock requirement is technically solvable.

Measured max per-channel pixel delta versus canonical base:

- outside replacement envelope: **0**
- Hero Bastion protected region: **0**
- promoted lower city protected region: **0**
- central stair protected region: **0**

So the integrated candidate **does unequivocally remain Valoria** and changes only pixels legally contained by the replacement envelope.

## Visual/source result

Despite the exact pixel preservation, the candidate is **not suitable for Tripo**.

The inserted west/east material reads as composited fragments rather than one continuous authored architectural frame. The west side has useful arcade depth, but the east side loses a clear civic mass and reads primarily as an inserted rock/retaining fragment. The receiving base does not read as one coherent stepped system joining both shoulders.

The isolated derivative successfully removes most of Hero Bastion and lower-city context, reducing the risk that a 3D generator would reconstruct the entire city. However, isolation exposes the underlying problem more clearly: the source is visually fragmented and does not provide one unambiguous connected 3D object.

## Mandatory questions

**¿La imagen conserva inequívocamente Valoria y cambia solo la pieza Bastion→city, o estamos otra vez generando una ciudad distinta?**

**YES for the integrated pixel-locked candidate.** This is verified, not subjective: all pixels outside the replacement envelope and all protected regions are identical to the canonical capture.

**¿Podríamos entregar esta imagen a un generador 3D sin riesgo evidente de que intente reconstruir Hero Bastion o una nueva lower city?**

**NO.** The integrated candidate still contains the whole surrounding scene, while the isolated derivative avoids that scope problem but is not a coherent enough architectural source to justify paid generation.

## Rejected evidence

- integrated candidate JPEG
  - SHA-256 `441880829fe0d19554e0f0d11dc5d0726734a91d4ef1f99b0cafd129267198e2`
  - 229,936 bytes
- isolated same-solution candidate JPEG
  - SHA-256 `a5ce585b3a9a28d5fd68f0aafe710fbd5dd45f497a53a6ad6da17753a92376ee`
  - 82,297 bytes

These are evidence of the failed gate only. They are **not** production inputs and are deliberately not persisted under `pipeline/exact-inputs`.

## Stop gate

- Tripo opened: **NO**
- Tripo credits: **0**
- 3D generated: **NO**
- Blender opened: **NO**
- Unity touched: **NO**
- gameplay/colliders/hotspots/routes/parcels/reserves changed: **NO**
- multiview package created: **NO**
- future Tripo generation block opened: **NO**

Final status:

**TECH PASS / EXACT SOURCE IMAGE FAIL / 0 CREDITS / STOP**

There is therefore no honest sentence of the form “Esta es la única imagen que proponemos enviar a Tripo si autorizas el gasto” yet. The exact-pixel preservation problem is solved; the remaining blocker is obtaining one coherent local architectural paintover/inpaint inside the locked envelope without global scene regeneration.
