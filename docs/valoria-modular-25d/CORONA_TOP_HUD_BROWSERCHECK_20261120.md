# Corona de la Brecha — exact-source HUD reflow and real Chrome pixel QA

Date: 2026-11-20. Current experiment branch: experiment/valoria-corona-de-la-brecha-ui-20261120.

## Observed (real Unity WebGL)
The genuine browser run 44816471118 at 390x844 places the old resource row at approximately y=113..165. Pixel differencing two genuine PNGs of the sawmill panel, immediately before and after Build, found exactly 90 changed pixels in the wood amount glyph bounding box x=97..114, y=133..144. No unrelated pixels in the cropped wood region changed. These are actual original screenshot pixels.

## Implemented (candidate until new build)
The row now touches the TOP safe-area line, removing the previous 105 reference-point vertical displacement, while the identity crest occupies the next compact row in portrait. In the 390x844 Chrome reference, the revised wood digits are consequently predicted near x=97..114, y=28..39. The browser ROI is adjusted to x=21.5%-31.3%, y=2.8%-5.5% on the real screenshot. The test must demonstrate actual changed numbers 230 -> 150 and reload persistence again: the prior image is NOT accepted as evidence of the new code.

The OBRAS tap now targets live Unity NavigationCell(5): x~352/390, 56/844 above the bottom; the CIUDAD home tap targets NavigationCell(4), x~292/390. These are real hit targets, not synthetic state changes.

## Grid and intended commercial finish
Resource materials and number remain original painted UI, identity directly below in portrait or left in landscape. NavigationCell is a six-slot responsive geometry with minimum 44 points. Slots 0-2 have no GameObjects or false menu functions; slots 3-5 keep the actual status/camera/building actions. A compact, art-preserving quest plaque sits just above the nav row. Only a successful LocalGateway Build closes the details panel and leaves a countdown anchored to the Aserradero.

## Still unverified
Exact-SHA Unity/C# compiler, PlayMode, genuine Chrome screenshots 390x844/1170x2532/844x390, physical iPhone memory/FPS/gestures, human visual quality and art acceptance. No merge into main. Do not claim the ROI estimate is a completed browser test.
