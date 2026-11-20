# Corona de la Brecha — real-player browser readiness when economy shares the top ribbon

**Context:** Unity 6.3 exact-SHA source `ae04e9a5fbf57e71b432b2d5824c16a50c311200` (frozen QA). New 46-point, nearly screen-wide dark artist HUD begins at safe top+2 and extends to x≈3..387 in a 390px portrait browser. Previously, the Chrome smoke tool interpreted dark resource material as a faded or clipped world.

## Causal diagnosis BEFORE the new test
Original browser readiness used edge columns **2.5%** wide (for390 x0..9) and rows0..6, insisting on edge luminance>=78. The legitimate painted ribbon covers x3..387 and starts at y2, so 3 out of the 5 sampled left "edge" columns are dark on y2..6. That can drive edge average below78, regardless of Unity rendering. The second heuristic `edgeMinusCenter>19` is also invalid because the intended center is dark. This is a false QA failure, not a game or asset defect. Unity's actual world is visually exposed at the physical outsidemost x0..2 and x388..389.

## Implemented in independent BROWSER-only QA branch
Use the real exterior **0.4%** edge strip, at least2px, which remains outside the 46pt ribbon at 390/1170 resolution; keep the same2px row spacing and>=78 stable brightness criterion. Replace old light-edge-minus-dark-center clipping heuristic with a test of exterior bright grass plus the separate existing **topPinnedResources.darkFraction>=.18** pixel coverage. This still fails closed if gameplay is dark, cropped, absent, or the real economy is not painted at the top. The command must still deduct 230→150, auto-dismiss the panel, prove browser reload, and open/close the user-requested left task drawer by real mouse clicks.

**Verification still required:** run the updated Chrome script on the immutable REAL Unity WebGL build artifact generated in [44833570082](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44833570082) after Unity completes. Do not substitute a mocked screenshot, assert success from source inspection, or alter Unity/gameplay to fix this browser test. No iPhone device claim.
