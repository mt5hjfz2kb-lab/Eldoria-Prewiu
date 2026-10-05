# VALORIA GOLDEN LOOKDEV SLICE v1 — Evidence index

Date: 2026-10-05

## Baseline / reason for pivot
- Canonical premium target: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`.
- Vegetation + Water/Shore final frame: run 37347735272 / artifact 11361740372.
- Premium Hero Test Zone iterations 01–05 all failed the premium visual gate despite technical success.
- Iteration05 authority: run 37353878139 / artifact 11364280394.
- Commit `040adeb1751feeb3a687f01621bb18f8de78db98` records the method pivot.
- A concurrent iteration06 commit/run landed immediately after the pivot but before the Golden Lookdev claim propagated. It is **superseded for method authority** and must not be used to reopen broad-scene micro-tuning.

## Golden Lookdev authority
- Spec: `docs/VALORIA_GOLDEN_LOOKDEV_SLICE_V1.md`
- Active route: `pipeline/art-production-request.json`
- Active workstream: `valoria-golden-lookdev-slice-v1`
- Tripo: 0 credits.
- Broad Props + Background and new production families remain blocked.

## Required next evidence
1. Exact Golden Slice screen-space crop/target definition.
2. Baseline capture/crop from official camera.
3. Surface recipe specification: albedo/normal/roughness/AO/masks, texture scale and memory.
4. Lighting/presentation recipe specification.
5. First integrated AFTER capture set.
6. Metric review against 4/5 gate.
7. Gameplay + performance sanity.
