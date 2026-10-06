# VALORIA FIRST PLAYABLE CITY SLICE v1

Status: CORRECT-REFERENCE SLICE PROOF PASS / CLOSED
Date: 2026-10-06

The previous vertical-reference VISUAL PASS is revoked as VISUAL_AUTHORITY_MISMATCH. This correction re-runs the same approved technical recipe against the exact owner open-Valoria target.

## Authority and result
- Reference: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg` (exact owner bytes).
- Source run/artifact: **37472982237 / 11418315301**.
- Final Unity run/artifact: **37474695980 / 11418118004**.
- Capture commit: `d912f89d0957aa5c49937736840070b1de6a753b`.
- **TECH PASS / VISUAL PASS / REFERENCE MATCH PASS / COMPOSITION PASS / INTERACTION PASS / BOUNDED CAMERA PASS**.
- Manual review: `docs/evidence/valoria-visual-authority-reset-v1/review.json`.
- Unmodified technical report: `docs/evidence/valoria-visual-authority-reset-v1/production-slice-evidence.json`. Its gate_pass=false intentionally requires the separate direct visual review.

## Correct route and composition
Bridge → Lower Gate → Main Road → Central Stair → contained Upper Gate / Walls / Bastion. Broad central esplanade, cabin plot left and blue-tent camp plot right remain readable at HOME and both accepted pans. No vertical monumental citadel is current authority.

## Real geometry / semantics
Seven approved GLBs remain imported/editable: Bridge, Lower Gate, Road, Stair, Walls, Bastion and Rock/Terrain. All imports, bounds and reference anchors pass. Real GLBs remain hidden support geometry in beauty; there is no promoted visible-mesh override. SHARP provides the visible whole frame.

Nine standard Unity proxy anchors cover those seven regions plus LeftCabinParcel and RightCampParcel. HOME, pan-left, pan-right, zoom-in and zoom-out give **45/45** semantic raycasts and five selection-counter events per proxy.

## Corrected camera / appearance
Camera comes from SHARP PLY intrinsics: 1230×845, FOV 44.42281°, identity extrinsic. Pan is x±0.5; bounded zoom FOV multipliers 0.9/1.1. The inherited ±1.75 envelope was rejected because it clipped the right parcel and exposed edge stretch. GammaToLinear is disabled in the actual Gamma project; the unnecessarily dark initial capture was rejected.

## Scope
PASS certifies this visual-authority / bounded hybrid slice correction. It does not certify a new full-game release, visible full-mesh replacement, free camera or mobile-device performance. Unity 6 URP, SHARP, existing projection/PBR path, colliders and approved source families are retained. No purchases, paid credits, new generator or research lane. No next production block started; wait for owner instruction.
