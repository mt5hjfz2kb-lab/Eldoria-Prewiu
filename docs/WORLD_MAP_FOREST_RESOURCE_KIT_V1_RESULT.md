# World Map — Forest Resource Kit v1 experiment

Status: **REJECTED FOR PRODUCTION**  
Date: 2026-09-29  
Cost: **0 Tripo credits / 0 €**

## Question tested

Can the already-imported **EmaceArt Slavic World Free** nature subset become Eldoria's production-quality reusable forest language after Eldoria scale, composition and material modulation, especially for the pulled-back Frontier/world-map camera?

## Real implementation tested

A temporary reusable `WorldNatureKit` was built and integrated into the real Frontier forest resource pocket while preserving the existing independent `forest-valoria` gameplay hotspot.

The composition used the project's existing Slavic references for:
- tree / tall tree;
- bush / undergrowth;
- moss;
- firewood;
- flat rock.

No gameplay topology or rule was moved into the art. Visual colliders were disabled.

## Evidence

GitHub Actions:
- run: **36590669809**
- artifact: **11044780827**
- result: technical gate **SUCCESS**
- captures:
  - `world-overview-14.png`
  - `world-overview-mobile.png`
  - `forest-kit-10.png`
  - `forest-kit-7.png`
  - `forest-kit-mobile.png`

The experiment compiled and rendered successfully and preserved the separate forest gameplay hotspot.

## Visual verdict

**VISUAL FAIL / DO NOT PROMOTE.**

At the real game camera, and especially in the forest-focused zoom:
- foliage reads too artificial and asset-pack-specific;
- greens become conspicuously bright/neon relative to Eldoria's terrain and lighting;
- tree silhouettes are too simple and repetitive for the intended premium world-map language;
- undergrowth does not blend convincingly into the terrain;
- material modulation alone is insufficient to make the nature family visually belong to Eldoria.

This is a style/asset-source limitation, not a technical integration failure.

## What remains usable from Slavic World Free

Do **not** generalize this rejection to the whole pack. Existing evidence still supports selective use of secondary hard-surface/environment props where the source is not visually dominant:
- rocks / flat rocks / boulders;
- cobble / mud road fragments;
- stone fence / retaining pieces;
- firewood;
- sacks, crates, barrels and similar small props;
- other low-salience dressing only after camera validation.

Do **not** use the rejected Slavic tree/bush/moss family as Eldoria's final world-map vegetation language without a materially different asset/surface solution and a new visual gate.

## Production consequence

The temporary `WorldNatureKit`, dedicated gate code and workflow were removed after review, and Frontier's previous forest composition was restored. The failed visual experiment must not remain active in production.

## Next art direction

For the world map, prioritize a new **World Nature Kit** from a better vegetation source or original/adapted assets, while continuing to reuse validated Slavic hard-surface props selectively.

The next zero/low-cost investigation should therefore separate:
1. **nature source quality** (trees/bushes/ground vegetation), and
2. **world-map prop kit** (rocks, road edges, resource dressing, signs, crates, timber),

rather than treating Slavic World Free as one indivisible family.
