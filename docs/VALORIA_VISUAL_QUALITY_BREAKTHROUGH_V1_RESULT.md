# VALORIA VISUAL QUALITY BREAKTHROUGH v1 — RESULT

Date: 2026-10-03  
Branch: `visual-proof/valoria-visual-quality-breakthrough-v1`

## Verdict

**TECH PASS / VISUAL FAIL**

This block produces a real first-glance improvement over the Asset Coherence baseline, but it does **not** yet satisfy the benchmark threshold for a convincing commercial-quality Valoria. The improvement is retained as the strongest current experimental presentation, but is not described as a finished visual target.

## Authoritative final evidence

- Accepted implementation commit: `7a564bc5056bb7166b7704c313ae4db7d55d4530`
- Final validation run: **37149992067**
- Final artifact: **11284220007**
- Baseline block: Asset Coherence implementation `1071454bd1bd27ec1f5a0c754805d61a7a77e829`
- Baseline run/artifact: **37143754581 / 11281586517**
- Final artifact contains matched real captures:
  - `before-19.png` / `after-19.png`
  - `before-12.png` / `after-12.png`
  - `before-9.png` / `after-9.png`
  - `before-mobile.png` / `after-mobile.png`
  - real Unity HUD captures `game-19/12/9/mobile.png`
- Focused live HUD/progression tests: PASS
- Tripo / paid credits: **0**

## What improved and is retained

### 1. Secondary architecture coherence
The canonical Aserradero, Cuartel and Granero remain in place after a full library lineup and Blender source audit showed no clearly superior replacement family. Their existing PBR maps are now interpreted with a semantic response for roof/foundation/timber hierarchy. This improves value separation without changing geometry or identity.

### 2. Wall rhythm and hierarchy
The continuous accepted wall base is retained. Repetitive primitive merlons are replaced by controlled non-uniform Stone_Wall cap spans with unequal lengths/heights and CornerWallL treatment. This removes much of the machine-like repeated-block reading while keeping gate, towers and expansion seams legible.

### 3. Hero Bastion / city interface
The canonical Hero GLB remains intact. Blender proved it contains 2,792 disconnected components but only one material. Low14/20/26 height-segmentation candidates were captured and rejected. The retained solution uses architectural wrapping/interface treatment rather than subtracting the Hero source.

### 4. Exterior world reading
Opaque field wedges and later low-relief berm/mound geometry were both rejected. The retained solution modifies the existing contained natural surround continuously in world space outside the city envelope and adds restrained occupation cues. The result is less board-like without introducing visible technical patches or a new Terrain system.

### 5. Game presentation / HUD
The real Unity HUD receives a responsive hierarchy/contrast pass. Canonical mobile interaction budgets are preserved: top/nav remain 68 px and the primary action remains >=44 px. No fake overlay is used.

## Rejected technique families

1. Wholesale authored-wall replacement by separated curtain pieces — fragmentation/regression.
2. Flat exterior field wedges — obvious brown patches.
3. MidTier annexes as the main secondary-architecture solution — too small/marginal.
4. Slavic/Mega wholesale functional replacement — weaker source coherence/identity.
5. Hero height-only segmentation — marginal at low thresholds, destructive at high thresholds.
6. Low-relief exterior ribbons/mounds — read as dark technical polygons.
7. Repeated tint-only changes — explicitly closed by anti-loop rule.
8. New terrain/foliage plugins or paid authoring tools — no demonstrated advantage sufficient to justify dependency/cost.

## Benchmark comparison

### Clear improvement
- wall repetition / silhouette rhythm;
- secondary-building value hierarchy;
- Hero-to-city interface;
- exterior uniformity;
- lived-in cues;
- HUD hierarchy and mobile-safe interaction sizing;
- overall coherence versus the immediate baseline.

### Still below benchmark
- the secondary architecture still does not match the Hero Bastion's authored richness;
- the Hero remains visibly more detailed and materially sophisticated than the rest of Valoria;
- large reserved/open areas still read sparse at 19/12, even though they must remain growth-safe;
- the natural surround still lacks the depth and authored world-frame quality of the visual benchmark;
- material families are more coherent but not yet at premium authored-atlas quality;
- the city still reads as an advanced prototype / early commercial production scene rather than a polished final-game capture.

Therefore the block is not promoted as **VISUAL PASS**.

## Parcel reservation / gameplay safety

The Flat Citadel macrocomposition remains locked. No mountain, terrain-island or territorial redesign was introduced.

The implementation preserves the reserved progression/growth interfaces:
- active Aserradero / Cuartel / Granero;
- future Cantera / Forja / Hospital;
- C0 / C1 / C2;
- XW / XE / XU / XS;
- future accesses and growth space.

All VQB additions are presentation-only / reversible. Gameplay colliders/hotspots remain authoritative and focused progression tests pass in the final run.

## Toolchain evolution

### Better use of existing tools
- Unity/URP shader semantics are now used for structure-aware material response rather than tint-only grading.
- Canonical asset-library selection is evidence-led through normalized lineups.
- Blender is now a practical source-diagnostic tool for component/material/bounds audits and deterministic candidate authoring.
- GitHub Actions produces matched 19/12/9/mobile evidence and separates visual capture from gameplay/HUD validation.

### New tools incorporated
None. No plugin demonstrated a clear enough production or visual advantage.

### Tools evaluated / deferred
- URP screen-space decals: technically valid for sparse later wear/seam work, deferred.
- Unity Terrain/foliage tooling: unnecessary for the locked contained Flat Citadel.
- Material Maker / Substance / ArmorPaint: not a demonstrated bottleneck.
- Tripo: no spend required.

## Scalability answer

**Yes — another graphical level can be pursued in six months without rebuilding Valoria.**

The macro layout, parcel map and gameplay authority remain separated from presentation. The retained VQB changes are modular:
- wall cap family can be replaced independently;
- semantic material response can be replaced with authored atlases/masks;
- functional GLBs can be swapped one by one;
- Hero interface treatment is independent of Hero gameplay/layout;
- exterior response/foliage is replaceable;
- HUD presentation is independent of world topology.

The main limitation is not structural lock-in. It is the remaining quality gap in source art coherence.

## Recommended next block

**VALORIA AUTHORED SECONDARY ART FAMILY v1**

Do not reopen macrocomposition and do not continue VQB micro-polish.

Goal: create a coherent, reusable authored secondary-art language for functional buildings + wall/gate/tower support that can genuinely approach the Hero Bastion's quality while preserving each functional identity and the parcel map.

Required emphasis:
1. authored modular roof / stone / timber / foundation vocabulary;
2. reusable trims and material atlas/masks;
3. source-level semantic separation rather than runtime tint inference;
4. one proof family first (Aserradero + one wall/gate support pair), then expand only if the proof produces a clear benchmark jump;
5. Blender/source authoring first; no paid generation unless a proven geometry gap remains;
6. retain the current Flat Citadel and current VQB implementation as BEFORE.

This is a larger art-authoring problem, not another placement/material-tuning pass.
