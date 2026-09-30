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
- FULL-KIT PRODUCTION PROMOTION: **NO**
- SELECTIVE PROMOTION: **YES — piece 02 only (`RockToWallTransition`)**

The visual failure is not caused by camera framing. The dedicated per-piece views demonstrate incorrect grouping and residual geometry. Therefore the kit must not be promoted as eight production-ready Valoria module families.

## Piece-by-piece visual review
1. **Piece 01 — corner wall:** strong architectural result, but its own front evidence shows a detached residual fragment. Cleanup candidate, not certified as-is.
2. **Piece 02 — rock-to-wall transition:** **individual visual PASS**. Front/side/rear/oblique evidence is coherent and free of detached residual geometry. It is the only piece accepted visually as-is and was promoted separately to `Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/RockToWallTransition.glb`.
3. **Piece 03:** visually contains two spatially separate architectural masses in one exported group. Fails one-family/one-module separability.
4. **Piece 04:** contains disconnected masonry masses plus curved/thin residual geometry. Fails clean reusable module requirement.
5. **Piece 05 — high wall candidate:** main wall is strong, but a detached vertical fragment remains. Candidate for cleanup, not certified as-is.
6. **Piece 06 — low wall candidate:** main wall is readable, but long thin protrusions/spikes remain. Fails clean side/rear geometry requirement.
7. **Piece 07 — pillar/buttress candidate:** two separate pillar-like masses are grouped together and one contains damaged/protruding geometry. Fails one-module separability.
8. **Piece 08 — arch candidate:** recognizable arch, but includes a detached upper/side mass and protruding residual geometry. Fails clean opening/module requirement.

## What this proves
The single-sheet Tripo approach can generate useful visual source material and the automated spatial clustering can recover eight numerical groups, but **numerical group count is not equivalent to eight production modules**. The exact source remains valuable for a zero-credit cleanup/salvage pass using the already exported GLB; no second Tripo generation is justified by this gate alone. Piece 02 is individually clean, while pieces 01/05/06 are the strongest cleanup candidates.

## Selective promotion result
A dedicated strict-pass promotion was executed after the visual gate:
- workflow run `36707553542` — SUCCESS;
- source artifact: run `36705459251`, artifact `11091584269`;
- promoted file: `Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/RockToWallTransition.glb`;
- promoted piece identity: `piece_02_7270tris.glb`, 12,414,676 bytes, SHA-256 `116adcc98a676c249cf4ff0f4a33b5a7fc59f2f0469ba8a10e0513cb021326e0`;
- the workflow verified exact byte size + SHA before copying.

The promotion was moved to GitHub-hosted Ubuntu and consumes no Windows runner time. It downloads the certified gate artifact instead of regenerating or re-splitting on the owner PC.

## Protected surfaces
Only the single certified Resource GLB above was added. `Valoria.unity`, `VisualWorld` and gameplay remain untouched.

## Closure
This Stone Architecture v1 generation/certification block is closed. The 8-family kit itself is not production-certified; only `RockToWallTransition` is individually certified/promoted. Any future work on pieces 01/05/06 is a separate zero-credit cleanup/salvage task, while 03/04/07/08 require reconstruction or a newly authorized generation.

## Selective production promotion
After the kit-level closeout, the strict per-piece evidence was used for a separate zero-credit selective promotion. Only **piece 02 / RockToWallTransition** met the visual gate as-is.

- Promotion workflow: **36707553542 SUCCESS**
- Promotion evidence artifact: **11092856576**
- Certified source artifact: **11091584269**
- Production path: `Unity/Assets/Eldoria/Resources/Valoria/StoneArchitectureKit_v1/RockToWallTransition.glb`
- Exact promoted bytes: **12,414,676**
- Exact promoted SHA-256: `116adcc98a676c249cf4ff0f4a33b5a7fc59f2f0469ba8a10e0513cb021326e0`
- Promotion commit: `f5b9f9ecdd426a17166d7051500f3c8b6e43193a`
- Files changed by that commit: **only the production GLB above**.

The overall eight-family kit verdict remains **VISUAL FAIL**. Piece 01, 05 and 06 are cleanup candidates; pieces 03, 04, 07 and 08 remain rejected. This selective promotion did not alter Valoria.unity, VisualWorld, gameplay, hotspots or topology and consumed **0 additional Tripo credits** beyond the already-authorized 55-credit generation.
