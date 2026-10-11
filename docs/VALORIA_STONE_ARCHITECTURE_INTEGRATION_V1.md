# Valoria — Stone Architecture v1 Integration

Date: 2026-09-30  
Status: **PRODUCTION INTEGRATION PASS / CLOSED**

## Scope

Integrate only the three already-certified Stone Architecture v1 production resources into real Valoria, with **zero new Tripo generation and zero new asset spend**:

- `CornerWallL.glb`
- `RockToWallTransition.glb`
- `HighStraightWall.glb`

Rejected Stone Architecture pieces 03/04/06/07/08 are not referenced by this integration.

## Final production placement

The first integration attempt used 7 instances and was visually rejected during camera review for excessive fortification/read as attached vertical blocks. The final refined set deliberately reduces this to **5 instances**.

### CornerWallL — 2 instances

1. `Valoria · StoneArch · corner · west work court`
   - anchor: `(-11.55, 0.30, 4.05)`
   - target span: `1.85`
   - yaw: `112°`
   - function: closes/articulates the west work-court edge and strengthens the terrace corner without obstructing the route.

2. `Valoria · StoneArch · corner · west lower court`
   - anchor: `(-15.35, 0.31, -4.25)`
   - target span: `1.70`
   - yaw: `18°`
   - function: closes a lower civilian court edge and breaks the flat/unfinished parcel boundary.

### HighStraightWall — 1 instance

1. `Valoria · StoneArch · high wall · west terrace back`
   - anchor: `(-17.55, 0.28, 0.55)`
   - target span: `2.65`
   - yaw: `88°`
   - function: one restrained rear terrace limit. The earlier military-side instance was removed after review to keep the military frontage open and avoid a repetitive fortified-city read.

### RockToWallTransition — 2 instances

1. `Valoria · StoneArch · rock wall seam · sawmill`
   - anchor: `(-6.15, 0.10, -2.55)`
   - target span: `1.95`
   - yaw: `28°`
   - function: hides the artificial Aserradero architecture/ground transition and integrates its foundation with the surrounding terrain/stone language.

2. `Valoria · StoneArch · rock wall seam · barracks`
   - anchor: `(5.85, 0.10, -3.75)`
   - target span: `1.85`
   - yaw: `205°`
   - function: softens the Cuartel foundation seam without introducing a new wall line across the readable military frontage.

## Visual result

Final visual review used a **same-scene BEFORE/AFTER** gate. Valoria is built once with Stone Architecture disabled, BEFORE is captured, then only the Stone Architecture layer is added to that exact scene and AFTER is captured. This prevents material, lighting, cache or scene-rebuild differences from contaminating the comparison.

The final pass is intentionally restrained:
- the west civilian/work district gains stronger enclosure and depth;
- wall/terrain joins read less like floating asset boundaries;
- one rear terrace edge gains architectural continuity;
- roads, stairs and important buildings remain fully readable;
- there is no repeated wall corridor or fortress-maze effect;
- the visual change remains localized to the intended integration zone at overview/mobile scale.

Measured image-difference footprint from the same-scene gate:
- zoom 19 overview: ~0.28% of pixels changed;
- zoom 12 overview: ~0.69%;
- zoom 9 overview: ~0.56%;
- mobile overview: ~0.04%;
- focused CornerWallL / HighStraightWall / RockToWallTransition views: ~1.21–1.22% each.

These figures are evidence of a localized additive pass, not a city-layout rewrite.

## Gameplay / topology

**UNCHANGED.**

- no hotspot moved;
- no certified road moved;
- no certified stair moved;
- no navigation route changed;
- no playable parcel moved;
- Stone Architecture instances live under `ProductionVisualIntegration`;
- the visual root disables all child colliders and removes accidental `WorldHotspot` components;
- final same-scene evidence reports `collider_hotspot_signature_equal: true`;
- `gameplay_topology_changed: false`.

## Existing-kit combination

The strongest integration is:
- **RockToWallTransition + existing Ground Kit / rescued seam language** around Aserradero and Cuartel;
- **CornerWallL + West Rebuilders housing/courts + StoneKit surface dressing** in the west district;
- **HighStraightWall + rear terrace/Ground Kit edge**, used only once to avoid repetition.

No working existing element was replaced merely to force use of a new module.

## Final validation

### Valoria Visual Formula
- run: **36725807356 — SUCCESS**
- source: `596a53ee415389213617699f50591bf980518cf0`
- artifact: **11103906617**
- artifact name: `valoria-visual-formula-36725807356`
- artifact digest: `sha256:151ca13173d00a1b5c9970a5b403277a52b3388ed4268e1fbc50e652f8779517`
- evidence: zoom 19 / 12 / 9 / mobile + three focused identical-camera BEFORE/AFTER pairs.

### Unity slice
- run: **36725799252 — SUCCESS**
- source: `984a1815e1b5d231b58654982954ab7ee98a3415`
- artifacts:
  - **11102592524** — `eldoria-unity-checks-984a1815e1b5d231b58654982954ab7ee98a3415`
  - **11103796270** — `eldoria-valoria-captures-984a1815e1b5d231b58654982954ab7ee98a3415`
- EditMode / PlayMode / Windows player / benchmark path completed successfully.

### World Map Visual Formula regression check
- run: **36725799121 — SUCCESS**
- artifact: **11102777443**
- source: `984a1815e1b5d231b58654982954ab7ee98a3415`

## Cost

- new Tripo generations: **0**
- Tripo credits spent by this block: **0**
- new purchased assets: **0**

## Final verdict

**PASS.**

Stone Architecture v1 is now used as a real, restrained part of Valoria's urban language rather than as a demonstration set. The final five instances improve architectural continuity, court/terrace definition and architecture↔terrain joins while preserving gameplay/topology and the city's civilian breathing space.
