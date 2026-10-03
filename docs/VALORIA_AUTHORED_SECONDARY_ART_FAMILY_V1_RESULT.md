# VALORIA AUTHORED SECONDARY ART FAMILY v1 — RESULT

Date: 2026-10-03  
Status: **TECH PASS / VISUAL FAIL / NOT PROMOTED**  
Branch: `visual-proof/valoria-authored-secondary-art-family-v1`

## Purpose

Test whether source-level art authoring can close the visual-quality gap between the Hero Bastion and Valoria's secondary architecture without reopening the locked Flat Citadel, changing gameplay topology, or spending paid-generation credits.

The proof was deliberately limited to:
- Aserradero;
- one wall-support vocabulary;
- one gate-support vocabulary.

Expansion to Cuartel / Granero was forbidden unless the first proof produced a clear integrated benchmark jump.

## Authoritative visual evidence

Final valid integrated comparison:
- run **37151178579 — SUCCESS**
- artifact **11283369532**
- source SHA: `be2c806a83fb7692311d489b2d0d810e76055de7`
- captures: baseline / authored-complete / source-preserving / source-atlas-v2 at **19 / 12 / 9 / mobile**
- gameplay collider/hotspot signature: preserved
- macrocomposition: unchanged
- F1 Aserradero envelope: preserved
- front gate route: preserved
- Tripo / paid credits: **0**

A later rerun **37151277094** failed before Unity capture because its staging assertion still expected eight GLBs after obsolete proof candidates had been cleaned from the branch. That infrastructure-only failure does not supersede the successful integrated comparison above.

## What was tested

### A. Full authored rebuild

A fully authored Blender Aserradero plus wall/gate modules tested a coherent warm-stone / timber / blue-slate language with beveled hard-surface forms.

Result: **rejected**.

At 12/9 the new Aserradero loses too much of the canonical source's fine silhouette and reads as a simplified dark-roof mass. The family is internally coherent, but coherence gained by lowering source richness is not an improvement.

### B. Source-preserving semantic authoring

The canonical Aserradero geometry was preserved at **49,800 triangles** while its single material source was segmented into semantic Stone / Timber / Roof regions.

Technical result: **PASS**.

This proves Blender can re-author the source non-destructively and make semantic material ownership explicit without changing the mesh.

Visual result: **insufficient**.

The geometry remains strong, but the first shared response still reads overly orange/brown relative to the Hero Bastion and does not create a benchmark-level whole-frame improvement.

### C. Source-preserving + shared authored atlas + canonical support v2

A deterministic shared material set was authored:
- warm ashlar stone;
- dark timber;
- blue slate;
- normal maps for all three.

The proof also created wall/gate support candidates derived from canonical source meshes rather than primitive replacement geometry.

Result: **technically valid, visually insufficient**.

At 12/9 the blue-slate roof creates stronger family identity, but it becomes too saturated against the rest of Valoria. The wall/gate support surfaces read as light horizontal bands added onto the existing defensive language. Mobile gains are negligible because the changed Aserradero is not a dominant part of the frame.

This is not the clear benchmark jump required by the stop rule.

## Decisions

Retain as technical knowledge:
- source-preserving semantic segmentation is viable;
- shared authored material atlases are viable;
- canonical support sources can be re-authored without paid generation;
- Blender is now proven as an actual source-authoring tool, not merely an auditor;
- integrated A/B proof in deterministic Unity cameras is the correct acceptance method.

Do **not** promote:
- the full authored Aserradero rebuild;
- the source-preserving Aserradero variants;
- the authored wall/gate support variants;
- the first shared Stone/Timber/Slate atlas.

Do **not** expand this family to Cuartel or Granero.

## Why the block fails visually

The proof isolates a deeper issue: replacing or re-surfacing one secondary family does not materially change the whole-frame hierarchy because the dominant mismatch is scene-wide.

Valoria still combines:
- one very rich Hero landmark;
- secondary sources with different visual densities;
- large intentionally reserved areas;
- a defensive ring and ground treatment with their own visual language;
- a broad, simple surrounding frame.

A local Aserradero + support family can improve one district but cannot by itself make the full 19/12/9 frame read like the approved commercial benchmark.

## Anti-loop additions

Do not silently retry:
1. lowering source detail in exchange for stylistic consistency;
2. recoloring the canonical Aserradero repeatedly;
3. expanding this first atlas to Cuartel/Granero without a new whole-frame hypothesis;
4. adding more authored wall/gate support pieces around the existing ring to make the family appear more prevalent;
5. treating isolated asset coherence as proof of full-frame improvement.

## Next decision boundary

The remaining gap is no longer proven to be solvable by another isolated asset-family pass.

The next meaningful step requires a **visual execution decision at whole-frame level**: keep the current semi-realistic mixed-source language and accept its ceiling, or deliberately rebase the player-facing visual shell toward a more unified authored/stylized language while preserving all gameplay, parcels, geography and systems.

That is a materially different art-direction choice and must not be smuggled in as another technical iteration.

No production promotion or owner publication is performed by this closeout.
