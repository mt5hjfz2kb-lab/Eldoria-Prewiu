# VALORIA REFERENCE CONVERGENCE v1 — RESULT

Date: 2026-10-02
Status: **TECH PASS / VISUAL FAIL against approved reference**

## Approved target

The owner supplied a whole-frame Valoria target showing:
- a compact inhabited mountain city;
- Hero Bastion / citadel as the dominant focal point;
- colossal broken arches and ancient masonry framing the settlement;
- steep cliff falloff and layered terraces;
- dense but controlled vegetation;
- warm construction/activity against cooler distant mountains;
- strong depth and a world that continues well beyond the active settlement.

The target is a quality/composition benchmark, not a literal requirement to copy every prop.

## Zero-credit experiments

All experiments used the existing library and spent **0 Tripo credits**.

Relevant successful evidence runs:
- 36990598874 — first reference convergence attempt.
- 36991648582 — Mega Fantasy ruin/bridge exposure attempt.
- 36992765452 — embedded-ruin / foreground-depth iteration.
- 36992883332 — broken-arch geology iteration.
- 36993521992 — large ruin-frame iteration.

All successful runs preserved the gameplay collider/hotspot signature.

## What existing assets can solve

Existing library assets are sufficient for:
- Hero Bastion focal point;
- functional Aserradero / Cuartel;
- compact vertical terrace structure;
- rock/terrain shelves and seam treatment;
- foreground cliff mass;
- pines/bushes and reconstruction occupation;
- warm/cool lighting and atmospheric separation;
- future growth plots.

## Proven geometry gap

The library does **not** contain a monumental ruin element that survives the target scale.

Tested families included:
- WorldInventory Arch_Gothic;
- Wall_Broken / Column_Round;
- Mega Fantasy Props destroyed tower;
- wall passage;
- detailed wall;
- stone bridge.

At the size required by the approved reference these pieces read as one or more of:
- detached wall slabs;
- oversized blockout gates;
- boxed castle modules;
- repeated modular props rather than ancient geology-integrated structure.

Further scaling/recomposition of these sources reduced visual quality instead of increasing it.

## Decision

Do not create more houses and do not resume lateral residential expansion.

The next justified new geometry is one reusable environmental hero asset:

**Valoria Monumental Broken Arch / Cliff Frame v1**

Requirements:
- one large open arch with a strong negative-space opening;
- asymmetric broken crown and side mass;
- aged stone integrated into a natural rock base;
- designed to be partially buried into Valoria terrain;
- visually convincing from the fixed official camera;
- mirrorable / rotatable so one generated source can frame both sides of the city;
- no gameplay ownership;
- no scaffolding baked into the asset unless later proven necessary (existing props can supply occupation);
- no house or complete district attached.

This is a narrow environment-geometry gap, not a new architectural family.

## Spend gate

Toolchain profile: `environment_new_geometry`.
`geometry_gap_proven=true`.

Tripo remains blocked until:
1. the exact isolated input image is persisted;
2. the exact image is shown to the owner;
3. visible Tripo cost is known;
4. the owner gives fresh explicit authorization.

