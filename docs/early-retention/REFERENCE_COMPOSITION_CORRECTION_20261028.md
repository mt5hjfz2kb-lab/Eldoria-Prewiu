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

## Native31 review and next correction

Run40728708845, source7251fcf11bdbe9640ef453a68889217adab7da89: Unity13 domain tests pass,13 genuine Vigilants PNGs. Route-to-paving placement visibly improves. Screenshot capture stops before Scouts because the prior test requires the disabled vector paper surface to remain enabled. Next candidate retains that component as an active heraldic emblem over the actual sliced parchment, and adds explicit checks for all3newpaper Images, texture identity, visibility and touch transparency. The observed adversary stretch is corrected by preserving actual trimmed-image aspect. Reference-style serif headings/buttons use bundled DejaVuSerif.ttf and its full redistribution notice. The font is limited to this destination; body text uses the existing HUD font. Parchment edges use9-slice rendering to survive wide/stacked card reflow. No ART_APPROVED claim.

The legacy Codex fixture is now installed by Playwright addInitScript before the next document boots, preventing old-document autosave during reload from replacing its unlock flags. Its post-reload assertion checks all3flags; navigation waits for a real actionable button. No production web source changed.

The corrected Codex/Relicario QA passed three consecutive fresh Chromium runs with the actual frozen web code. No assertions were removed. Native31's old owner-dependent shared UI gate failed before downloading/running Unity; its two lease checks now explicitly allow the current retention owner/revision while retaining the previous UI owner's separate claim rule. Queue cleanup likewise checks current owner and revision6 before touching any unstarted obsolete run.

## Native32 full interaction proof and visual diagnosis

Run40731691933, sourcebd0b48a6be86bbb8849b1b9fa8d4b7283da9d3c2, artifact12836117013, ZIP SHA25687980dbb7a7516f335f2c27f6cef6b8e73cc123a30d50158831ab84180034f9d. All19actual Unity states were captured; domain13/13 and genuine combat/send/claim/XP/return/reload checks passed. The final layout collector correctly rejects one title: Suministros extraviados needs56units and received33. All original assertions remain required.

Visual inspection also rejects the short-row parchment margins:90source-pixel corners consumed almost all row height, leaving status text on navy outside the opaque paper. Next candidate uses3x pixels-per-unit to preserve fine curls while filling each real card. A small browser9-slice preview was used only as a cheap material/layout diagnostic, never as Unity proof. Title box now allows62units/two lines; actual state/action text remains separate. The redundant Scouts section heading is removed, cards use the freed height, and the real duration from ScoutDurationSeconds is visible before sending. Landscape subtitle no longer sits beneath the Back target.

## Reference-derived encounter illustration and panel

The approved paired concept is the visual authority. The prior gray-studio3D thumbnail does not have its painterly appearance. Two built-in image edits now prepare separate decorative UI assets from that original reference, with live Unity controls and data layered above. The3D spawnling model, battle rules and enemy IDs are unchanged; these are2D presentation candidates, not new3D assets or a Unity render.

- `Resources/EldoriaRetention/engendro-reference-v3.png`:1254x1254RGBA, SHA256889e026b580b30c64b5b8966a68d67e81910fd15118c44c807433dd17d7139d4. Imported at maximum1024, with genuine input alpha and CPU readability disabled. Used directly; no duplicate keyed texture or removal of pale skeletal details. The previous real-model render remains as fallback.
- `Resources/EldoriaRetention/encounter-panel-v3.png`:1774x887RGBA, SHA2568b1122679358b8120efa67a00cb251d0a6df719d09b52da92464b85dee21d4d9. Only its transparent outer padding is excluded by a Sprite rect(42,130,1690,627); original file bytes are preserved.128pixel sliced borders with2.5density preserve actual gold corner ornaments. Text, creature, army, XP and action controls are not baked into this skin.

Creature exact prompt (built-in image tool; no external API): Edit target: supplied original approved paired game UI concept. Extract ONLY the purple Breach Spawnling monster illustration from the LOWER LEFT encounter card of the LEFT screen as a standalone transparent cutout. Reproduce that SAME visible creature faithfully: the same three-quarter view facing right, feral crouching posture, four clawed limbs, bony white/grey horned skull and spine/branchlike spikes, black and violet sinewy armored body and purple magical corruption energy along its back. This is the exact creature from this picture, not a redesigned animal, not the map, not the gold frame, not the enemy text. Preserve the reference's painterly polished fantasy game illustration quality, lighting, silhouette, anatomy and rich violet/glowing accents. Complete only the small parts of its feet hidden by the old UI, unobtrusively. Full body inside canvas with5 percent clear padding, no clipping. NO text, NO icons, NO interface controls, NO scene background, NO ground plane, NO frame, NO numbers, NO rewards. Genuine transparent alpha around creature and sparse purple energy wisps; opaque creature. Portrait or square composition optimized as a reusable live game UI portrait, approximately 1024 pixels. Do not include any part of the right screen or the unrelated scenery.

Panel exact prompt (built-in image tool; no external API): Edit target: supplied approved paired medieval game UI. Produce ONE blank reusable encounter-panel skin matching the black/slate encounter information panel on LOWER LEFT screen. Only a single rectangular panel: fine richly crafted antique-gold double outline, small delicate medieval engraved curlicue corner flourishes like the reference, very dark charcoal/slate-blue softly textured interior with restrained violet smoky ambience, fully opaque dark center so real overlaid white text stays readable. Whole rectangle entirely visible with tiny 2 percent transparent outer padding. Wide landscape approximately 2:1 aspect; border widths thin and consistent, suitable for 9-slice stretching. Reproduce this reference's fine gold craftsmanship and dark material; do not add a different theme. NO text, NO numbers, NO creature, NO reward icons, NO button, NO dividing lines, NO picture, NO illustration window, NO UI mockup, NO scene outside the panel. Genuine transparent alpha outside the clean ornamental panel edges. This is a blank background asset behind live Unity encounter controls.

Native33 must render these exact source identities and19states, without relaxing text, visibility, action, save or resource-source checks. The portrait card gains scenic height by placing real XP/combat actions side by side at touch-safe sizes. Rewards remain provisional for later owner evaluation. No ART_APPROVED until the new exact screenshots are inspected.

## Shared certification refreshed without repeating changed room proof

Shared UI run40731703310 passed47EditMode and72PlayMode cases with zero failures. The actual compiled PR merge is fd0967e7fb0b761aa333e8a9c928e4bce95f24d4, whose Git tree20d1cfaf48df8077778b1870b69daa17e5770038 exactly equals immutable branch sourcebd0b48a6be86bbb8849b1b9fa8d4b7283da9d3c2. The existing strict scope check now pins this completed certificate; the entire certify job must remain byte-identical and shared production inputs unchanged. Its native-workflow comparison is now strict byte equality (both versions require19views), removing the old17/19normalization. The current room's own19-view native33proof is still separately mandatory. No shared or native test was disabled.


## Candidate34 — distinct illustrated contracts and actual guardian signal

Native33 (`443cf71829d705eb8f75c45b334a7fd11041d450`, run40740007470)
passed13domain tests, all19 actual captures and real battle/three dispatches/
three claims/XP/save reload. Its actual screenshot comparison still showed
repeated map crops in contracts, flat vector button faces and a missing skull
standard. Technical success did not constitute reference acceptance.

Four retained PNGs were authored with the built-in image generation tool from
the same selected paired original reference, without external API spending.
The following describes the briefs (not a verbatim transcript): isolate a blank
red enamel/gold battle button; isolate its blue expedition counterpart; create
one2x2 painterly atlas of patrol tower, lost travellers at ruins, supply wagon in
emerald forest and coastal ground fissure; isolate the red cloth/gold skull
standard. Buttons/banner have true alpha; no numbers, rewards, captions or
interactive controls are baked into any asset.

| Source PNG | SHA256 |
| --- | --- |
| battle-action-skin-v4.png | 4e7df7e1fbcafda1e14382f39255eae2a63036bad975a63a0f205a31e10838f3 |
| scout-action-skin-v4.png | 2796b59784bf05ca9bca7c3cbf3d88d7e34ed9098e0b798b760dc47ab38410f6 |
| scout-mission-vignettes-v4.png | 9f28dabb20fc8b8740f2839b4632811cba24917a6ed4e1d43a023e0bc937121f |
| guardian-banner-v4.png | 5d00ec6311edbb1a547226f4e95eaea3bb32e9197850bea9cc95e921c253057f |

The same RoyalButton Graphic draws the nine-sliced red/blue texture and remains
the genuine raycast target. The actual persisted mission Kind chooses its atlas
quadrant, inset to prevent bleeding; a cover crop preserves aspect. All three
portraits now retain the painted mission detail too. The skull banner is purely
decorative and follows only the actual current guardian using StagePosition,
clamped inside its existing illustrated scene. No extra Canvas, EventSystem,
mission rules or values are introduced. Existing domain data, UTC clocks,
combat and persistence remain authoritative. Rewards balance is provisional
under the owner's latest instruction.

The next native proof retains all19 capture states, >=12px handheld text and
all real interaction checks. Additional assertions verify the actual source
textures, correct kind-to-quadrant mapping, undistorted previews in both
orientations and a wholly visible current guardian standard. Candidate34
requires its own real Unity output and independent comparison before acceptance;
ART_APPROVED remains false at source preparation.


## Macro correction after reviewing the regular landscape composition

The source33 1600x900 image exposed a structural layout defect: a 1536px-wide
encounter card displayed only about132px of creature and a large unused right
area; the shallow scenic strip also cropped out the destination castle. New
button textures cannot correct that hierarchy. Replace the stacked wide strips
with two balanced full-height regions: the winding campaign on the left and a
portrait-proportioned encounter on the right, using the already readable portrait
creature/data/action proportions. The image remains on its actual mapped road,
all25 milestones remain clear of the separate card, and the creature receives a
360-unit square instead of a160-unit cap. The real battle button also shows the
existing crossed-swords icon beside its live caption. This is a composition
change requiring a new exact native capture, not a claim of acceptance.


### Exact retained medallion prompt — built-in image_gen

Use case: stylized-concept. Asset type: ONE transparent 2x2 sprite atlas of four medieval fantasy UI medallions for the existing Eldoria Scout Board. Input image: the selected paired Camino de los Vigilantes / Mesa de Exploradores reference, used ONLY for matching its exact ornate gold circular mission badges, painterly polished game UI craftsmanship and scale. Produce four separate circular badges, perfectly centred in four equal square quadrants of one square transparent image, matching diameters and consistent gold relief rims. Top left: gold compass rose on warm amber enamel (patrol). Top right: luminous green leaf sprig on deep emerald enamel, matching the forest badge in the reference (rescue). Bottom left: small wooden supply chest with a golden latch on warm gold/brown enamel (supplies). Bottom right: ivory sailing ship over deep blue enamel, matching the coastal badge in the reference (Breach traces near coast). Each badge fills 80% of its quadrant, 10% transparent margin on all sides, fully isolated, no overlaps, no shadows spilling into adjacent quadrants. True alpha transparent background outside the four circles. No text, letters, numerals, currencies, rewards, labels, frames or complete UI. These are decorative badges only; no baked gameplay. Preserve original reference's rich gold bevel, cream highlights, dimensional illustration, no flat vector substitutes. Deliver only this one 2x2 atlas.

Source: scout-medallions-v5.png; SHA256 is recorded in the art request. One512px GPU atlas, source retained at original resolution/alpha; no text or baked reward values.
