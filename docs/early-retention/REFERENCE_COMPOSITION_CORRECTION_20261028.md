# Reference composition correction · 28 October 2026

Owner continuation: finish PR120 against the selected original visual composition, then PR122. Reward selection/balance may remain for later evaluation. Functional systems must remain real.

## Independent review of aa2666d4

The 19 authentic Unity images and domain/interaction checks establish a technically working candidate, not visual equivalence. Comparing the original paired reference with `vigilants-stage15-engendro-horizontal.png` and `scout-mission-board-horizontal.png` shows three material differences: route milestones float independently of painted paving; the adversary reads as a small creature outside its encounter card; parchment cards have flat geometric edges and share an opaque navy backing.

## Candidate corrections

- Route positions project measured coordinates of the existing painterly-v2 paving through exactly the same RawImage UV crop. All 25 real stages remain; octagonal dark medallions, green completed checks and the current golden halo carry actual progress.
- Engendro uses the same authentic canonical creature texture inside the encounter card. Its visibility transition restores the encounter layout, including after winning stage15.
- Three live scout contracts use a new blank parchment backing with alpha. Titles, state, reward values, images and action buttons remain separate uGUI components. The flat navy shared board backing is removed.
- Existing19 image states, complete text-fit checks, >=12 physical-pixel handheld type, raycast actions, three dispatches/claims, XP and save reload remain required. The obsolete requirement that every marker be above a portrait in a different parent is replaced by checking actual world-space portrait occlusion for each of25 points. The independent information-card occlusion checks are retained.

## Blank parchment provenance

Source: builtin image generation using the original paired design as a reference (right-hand parchment only). Output `scout-parchment-v3.png`, RGBA1024x1536. No paid external API was invoked. The transparent pixels retain RGB values; Unity imports alpha explicitly. It contains no baked titles, values, icons, actions or fake rewards.

Exact prompt: Create ONE production UI asset extracted/recreated faithfully from reference: the blank aged parchment card backing seen under the three expedition cards on RIGHT side. Single tall rectangular parchment sheet only, straight-on orthographic front view, exact same warm cream fibrous paper, irregular softly torn and slightly curled browned edges, subtle golden candlelit dimensional shading. NO text, NO illustrations, NO icons, NO symbols, NO buttons, NO medallion. Flat light cream empty center suitable for real game text and a thumbnail to be overlaid by code. Card occupies 92% width and 94% height of a 2:3 portrait transparent canvas. No background, no surrounding scene, no perspective angle. Match reference paper color and edge craftsmanship precisely. This is a reusable game UI backing asset, not a mockup.

## Status

Candidate source only. ART_APPROVED=false. Inspect the exact new Unity screenshots before accepting or merging. Local .NET smoke could not start because this host's process-stat interface caused the .NET CLI to throw Win32Exception; this is not a domain test failure or a PASS. Use the existing Ubuntu/native certification routes.
