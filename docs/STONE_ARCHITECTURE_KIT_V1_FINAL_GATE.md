# Stone Architecture Kit v1 — Final Gate

Date: 2026-09-30

## Scope
Final certification of the owner-approved Stone Architecture multipiece generation. No new Tripo generation was performed in this closing block.

## Source identity
- Exact approved input PNG: 1254x1254, 2,139,894 bytes.
- Input SHA-256: `5a7a5353e7a8b1a914cb968d856a738868ac548311aee0be4df69b4eeac76258`.
- Tripo generation: owner-authorized 55 credits, clicked exactly once.
- Exported GLB: 74,616,872 bytes.
- Exported GLB SHA-256: `da593271a03170ce00475a64ee1754c4b4a40621644d5a30cfb75803ccae6422`.

## Canonical gate evidence
- First complete multipiece gate: run `36704168246` — SUCCESS — artifact `11091836294`.
- Per-piece visual gate: run `36705459251` — SUCCESS — artifact `11091584269`.
- Source was reduced from 1,951,506 raw triangles to 49,800 triangles.
- Multipiece extraction produced 8 groups / 49,792 total triangles in Unity.
- All 8 Unity entries have mesh, material, collider, UV0 and normals.

## Final verdict
- TECHNICAL: PASS
- EXACT SOURCE IDENTITY: PASS
- BLENDER OPTIMIZATION: PASS
- UNITY IMPORT / COLLIDER / UV / NORMALS: PASS
- MULTIPIECE COUNT: PASS (8 groups)
- VISUAL / MODULAR PRODUCTION ACCEPTANCE: **FAIL**
- PRODUCTION PROMOTION: **NO**

The visual failure is not caused by camera framing. The dedicated per-piece views demonstrate incorrect grouping and residual geometry. Therefore the kit must not be promoted as eight production-ready Valoria module families.

## Piece-by-piece visual review
1. **Piece 01 — corner wall:** strongest architectural result. Main form is coherent and readable, but a small detached residual fragment remains. Candidate for cleanup, not certified as-is.
2. **Piece 02 — rock-to-wall transition:** coherent silhouette and the strongest clean source candidate. Suitable for a cleanup/pivot pass, but not automatically promoted in this gate.
3. **Piece 03:** visually contains two spatially separate architectural masses in one exported group. Fails one-family/one-module separability.
4. **Piece 04:** contains disconnected masonry masses plus curved/thin residual geometry. Fails clean reusable module requirement.
5. **Piece 05 — high wall candidate:** main wall is strong, but a detached vertical fragment remains. Candidate for cleanup, not certified as-is.
6. **Piece 06 — low wall candidate:** main wall is readable, but long thin protrusions/spikes remain. Fails clean side/rear geometry requirement.
7. **Piece 07 — pillar/buttress candidate:** two separate pillar-like masses are grouped together and one contains damaged/protruding geometry. Fails one-module separability.
8. **Piece 08 — arch candidate:** recognizable arch, but includes a detached upper/side mass and protruding residual geometry. Fails clean opening/module requirement.

## What this proves
The single-sheet Tripo approach can generate useful visual source material and the automated spatial clustering can recover eight numerical groups, but **numerical group count is not equivalent to eight production modules**. The exact source remains valuable for a zero-credit cleanup/salvage pass using the already exported GLB; no second Tripo generation is justified by this gate alone.

## Protected surfaces
No production promotion was performed. Valoria.unity, VisualWorld and gameplay remain untouched.

## Closure
This Stone Architecture v1 generation/certification block is closed. Any future work must start as a separate cleanup/salvage task from the existing GLB or as a newly authorized generation; it must not be described as already production-certified.
