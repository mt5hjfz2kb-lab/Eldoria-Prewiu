# Valoria LookDev — preliminary controlled result

Status: preliminary; superseded only by the current 5-profile Aserradero+Cuartel comparison.  
Date: 2026-09-29.

Evidence:
- partial artifact from run 36494143895: 11004071366
- four profiles successfully rendered before the legacy no-timeout process was cancelled:
  baseline, neutral-pbr, ruins-contrast, neutral-overcast
- views: integrated overview zoom 12 + Aserradero zoom 9.

## Visual result

**baseline — REJECT direction**
- highest wash/ambient lift;
- weak separation between terrain, rock and architecture;
- Aserradero/Cuartel-like bright surfaces become false focal points.

**neutral-overcast — REJECT direction**
- removes some warm cast but remains pale/grey and atmosphere still compresses the city;
- insufficient dark-fantasy depth.

**neutral-pbr — KEEP as useful lower-contrast bound**
- materially better value separation than baseline;
- cooler/cleaner PBR read;
- preserves readability, but can still feel somewhat flat/cool.

**ruins-contrast — KEEP as useful high-contrast bound**
- strongest hierarchy, ground/architecture separation and depth of the first four profiles;
- best direction for recovering Valoria's dark-fantasy mass;
- slightly too dark/warm to promote without a balanced comparison against both Aserradero and Cuartel.

## Quantitative support

Approximate full-frame luminance statistics from the actual 1280x720 PNGs:

| Profile | Overview mean | Sawmill mean | Overview >90% luma | Sawmill >90% luma |
| --- | ---: | ---: | ---: | ---: |
| baseline | 0.436 | 0.403 | 0.388% | 0.663% |
| neutral-pbr | 0.337 | 0.302 | 0.013% | 0.021% |
| ruins-contrast | 0.284 | 0.255 | 0.011% | 0.018% |
| neutral-overcast | 0.385 | 0.344 | 0.012% | 0.019% |

These statistics support the wash diagnosis but do not select art direction by themselves.

## Current decision

The production candidate should live **between neutral-pbr and ruins-contrast**, not near baseline/overcast.

The current `valoria-v1-candidate` is intentionally in that range:
- ambient: (0.54, 0.56, 0.57)
- fog: (0.46, 0.49, 0.51), 31–68
- sun: (1.00, 0.84, 0.68), intensity 1.45
- shadow strength: 0.68
- sun rotation: (52, -34, 0)

Promotion still requires the new 5-profile / 3-view comparison to prove that Aserradero **and Cuartel** improve together.
