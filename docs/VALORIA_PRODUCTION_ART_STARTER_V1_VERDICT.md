# Valoria Production Art Starter Family v1 — Verdict

Status: **TECH PASS / VISUAL FAIL / NOT PROMOTED**  
Program: **VALORIA PRODUCTION ART SYSTEM RESET v1**

## Evidence

Source build:
- run `37195777502`
- artifact `11301510401`
- canonical source persisted at commit `a120048cfdebec9cc714dbecdd3d625042165e00`
- BLEND SHA-256 `85ccce27f6beb5087e93abc321e4991e572266eef9b3aa2ba5344295a4bce236`

Unity stop-gate sequence:
- `37196747485` — TECH PASS; visual fail due oversized/white imported family.
- `37197026374` — TECH PASS; scale improved; visual fail because imported shader ignored diagnostic property blocks.
- `37197624852` — TECH PASS with production material bridge; visual still below target.
- `37197888431` — TECH PASS; final bounded v1 evidence.

Gameplay-focused checks passed on valid runs. Paid credits: **0**.

## What v1 proved

The reset successfully replaced the Nation1 600–1600-vertex primitive family with reproducible Blender-authored geometry containing actual recesses, openings, eaves, roof thickness, arches, buttresses, trim and stronger silhouettes.

Representative triangle counts:
- Main Gate: 6,028
- Wall: 3,690
- Tower: 5,412
- Civic House: 9,800
- Workshop: 5,408

The source/export/Unity certification system works.

## Why v1 is not final art

In real zoom 9/mobile captures, the entry wall/towers/houses still read as a clean modular kit beside the far richer Hero Bastion and legacy dedicated production buildings.

The dominant remaining defect is no longer:
- CI;
- Blender access;
- import/export;
- scale alone;
- simple color;
- a missing small prop.

It is **source richness / architectural language**.

Even after bounded scale/material corrections, the family remains closer to a cleaner prototype than to the approved premium fantasy 4X reference.

## Anti-loop rule

Do not continue v1 through additional:
- braces;
- cornices;
- tint changes;
- tiny roof decorations;
- prop scatter;
- repeated wall-placement variants.

The three bounded source/presentation attempts are sufficient evidence that this technique has reached its useful ceiling.

## Reuse from v1

Keep:
- build/export automation;
- manifest/source identity system;
- source layout;
- gameplay-safe integration gate;
- useful recessed-opening/roof-thickness techniques;
- role/scale lessons.

Do not promote:
- v1 gate/wall/tower/civic/workshop GLBs to `PRODUCTION_ART_SOURCE`.

Their manifests remain `TEMPORARY`.

## Next technique

Starter Family v2 must derive its visible architectural language from certified rich donor assets (Aserradero, Cuartel, Granero and compatible production anchors), using semantic Blender source reuse/re-authoring rather than another primitive/boolean enrichment loop.

Stop gate remains zoom 9 + mobile.
