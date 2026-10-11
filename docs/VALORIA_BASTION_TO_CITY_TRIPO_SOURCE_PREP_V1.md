# VALORIA BASTION-TO-CITY TRIPO SOURCE PREP v1 — RESULT

Date: 2026-10-04  
Status: **CLOSED**  
Final verdict: **TECH PASS / TRIPO INPUT PREP FAIL / 0 CREDITS / STOP**

## Authority used

The block was reconstructed from live `main`, not chat state. The exact authority chain was:

- `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`
- canonical base capture `AFTER-HORIZONTAL-16x9.png`
  - run **37221732105**
  - artifact **11309549299**
  - SHA-256 `ef45233a09199829c83f5cb3d5587753f5b93fa7b1e9ba726b633791b7e343c1`
- `docs/VALORIA_BASTION_TO_CITY_FRAME_DESIGN_BRIEF_V1.md`
- `docs/evidence/valoria-bastion-to-city-frame-design-brief-v1/target-spec.json`
- `docs/evidence/valoria-bastion-to-city-frame-design-brief-v1/target-overlay.svg`
- previous ART SOURCE failures in:
  - `docs/VALORIA_BASTION_TO_CITY_ARCHITECTURAL_FRAME_V1_RESULT.md`
  - `docs/VALORIA_BASTION_TO_CITY_BLENDER_AUTHORING_V1_RESULT.md`

The approved visual target is already represented canonically by the Valoria visual reference plus the locked frame brief/spec. There is no separate authoritative target-image binary in the design-brief evidence directory.

## Intended package design

The correct future package should be **multi-view with exactly three images**:

1. one primary 3/4 source image;
2. one opposite-side auxiliary to clarify lateral depth, wall returns and the taller civic/east mass;
3. one tighter transition auxiliary to clarify deep arcades, stepped receiver, real wall thickness and stone/rock interlock.

Two auxiliaries are justified because the previous ART SOURCE failures were specifically about invisible depth/returns and weak rock/architecture interlock. A single frontal image cannot resolve both hidden lateral depth and the transition seam without encouraging hallucinated geometry.

No optional fourth/fifth image is justified at this stage.

## Candidate-generation result

Multiple zero-Tripo visual-prep attempts were made only to create the reviewable 2D input set. They were **not** submitted to Tripo and spent **0 Tripo credits**.

Every generated attempt failed the same hard requirement: it drifted from the canonical Hero Bastion / locked Valoria frame and generated a different fortress-city composition. The imagery had richer architecture, but richness alone is not enough; using it would teach Tripo the wrong object.

The final rejected crops were hashed locally for audit:

- `candidate-01-main-source-REJECTED.jpg` — `dfbb0141860cab0c527475d527d91647eb856b493791c157e23aad904ef36a57`
- `candidate-02-aux-opposite-REJECTED.jpg` — `78bf1f78f5b6383d02c39fdbee7cef6bc81cf1c01a976869bd7a603d48734543`
- `candidate-03-aux-transition-REJECTED.jpg` — `b6f3a5f4bb9d18fbd01bb4bebde8d37b214d6f10d88717a05bf8c9c8fed07316`

They are deliberately **not** declared as Tripo inputs and are not copied into `pipeline/exact-inputs`.

## Why this is a FAIL rather than “good enough”

The package must increase the probability of obtaining a structurally better 3D source **for the approved piece**. The candidates instead trade the previous blockout problem for identity/design drift:

- the Hero Bastion silhouette changes;
- the west/east relationship no longer follows the locked frame;
- the central stair/receiver relationship changes;
- lower-city continuity becomes a different city layout;
- the arcades/rock interlock are visually rich but belong to newly invented architecture.

Submitting these would be a process regression because it would spend credits before the exact approved input exists.

## Mandatory consistency question

**¿Estas vistas describen esencialmente la misma pieza y ayudarían a una reconstrucción 3D coherente, o en realidad se contradicen?**

**FAIL.** They are internally related as generated imagery, but they do **not** describe the same canonical Valoria piece locked by the repository. Therefore they are not a coherent multiview reconstruction package for the approved target.

## Stop gate

No Tripo session was opened.  
No Tripo credits were spent.  
No final source 3D was generated.  
No Blender authoring was opened.  
No Unity integration or gameplay change occurred.  
No `VALORIA BASTION-TO-CITY TRIPO SOURCE GENERATION v1` block was opened.

Final status:

**TECH PASS / TRIPO INPUT PREP FAIL / 0 CREDITS / STOP**

There are currently **no exact images that should be sent to Tripo**. The correct next action is to obtain an image-generation/editing route that can preserve the canonical Hero Bastion and locked frame while only reauthoring the Bastion→city envelope; only then should this prep block be reopened or superseded with a new approved input set.
