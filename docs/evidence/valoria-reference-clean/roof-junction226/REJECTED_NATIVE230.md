# Native230 — rejected hip shaping

Blender authoring and all six Unity before/after captures completed. The overall workflow failed because the trial log marker omitted the existing REFERENCE_CLEAN prefix; this is corrected without weakening the workflow gate.

Independent direct visual review of all six after views rejects this candidate: the tower interiors are cleared, but reshaping individual slate geometry creates jagged/fanned end tiles and an implausible rear lip, visible in detail and portrait. The canonical playable scene remains unchanged at b6bc04b8c7b70acdc1fb11042b8fd26148cbadb9b0ecffb6f2abca2f3ae96940.

Evidence: run39488180353, immutable compact artifact12294402736. Rejected candidate SHA256 7baa44987f01729b3fd7df2653d3d8e15ff4e95080dc1d1387b757589c9292c5; editable Blender SHA256 ec9ca3c1471903782b4316abd1e4fa9b93a2d387148ba923c197a31af635529b. Preserve rejected editable source once if present in retained worktree.

Changed strategy: retain the original supported gable roof and all slate slopes. Limit mesh clipping to intersections within existing tower walls and retain the fitted stone terrace. No new architecture or progression. This requires a new visual trial because geometry changes materially.
