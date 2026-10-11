# VALORIA SECONDARY / SUPPORT FORM REPLACEMENT v1 — RESULT

Date: 2026-10-04
Status: **CLOSED / NOT PROMOTED**
Final verdict: **TECH PASS / VISUAL PARTIAL-FAIL**

## Scope

Bounded to HOME mobile, PAN Granero, PAN intermediate, PAN Cuartel and 16:9. Hero Bastion, accepted defense, Granero, Cuartel, Flat Citadel, orthographic camera, promoted mobile HOME, panning/recenter, 16:9 framing, Final Look, gameplay, hotspots, colliders, routes, parcels and future reserves were preserved. Forja/Hospital/Cantera were not opened.

Cost: **0 Tripo / 0 paid credits**.

## Planner

- run **37219017523 — SUCCESS**
- route: existing assets / Unity composition
- Tripo and paid generation disabled

## Rejected first technique

The first composition used MasonryWall/MasonryTower plus SlavicHouse/SlavicShed and Slavic rock sources.

- run 37219001205 — compile-only FAIL
- tech-only fix: c8080d530dade1c49cbac8865a2c801ab337974d
- matched run **37219171093 — SUCCESS**
- artifact **11310056089**
- verdict: **TECH PASS / VISUAL FAIL / TECHNIQUE DISCARDED**

Reason: large brown masonry faces and new brown-roof Slavic houses broke the current stone/roof family and added obvious placed masses, especially in PAN Cuartel. Per anti-loop, this was not color/scale-tuned further.

## Final technique

The replacement technique used only certified/rescued structural sources:

- Valoria/StoneArchitectureKit_v1/HighStraightWall
- Valoria/StoneArchitectureKit_v1/CornerWallL
- Valoria/StoneArchitectureKit_v1/RockToWallTransition
- Valoria/Rescued/ResidentialTerraceRock
- existing lower-city authored houses already present in baseline

Housing treatment was subtractive: duplicate procedural underlays beneath the west rebuilding houses were hidden, two immediate authored neighbours were removed from the compressed west group, and four existing authored houses were retained. No new replacement-house donor family was added.

Support treatment: two larger stair-flanking structural bays, two terrace returns, two architecture-to-rock seams and one broad buried west-quarter rock mass. No ground-decor overlay, microprops, camera change, layout rewrite or new functional building.

## Authoritative matched gate

- run **37219568955 — SUCCESS**
- artifact **11309203816**
- captured head **a82d508a4d614749fb3b641927e26aae9a900c02**
- gameplay collider/hotspot signature preserved: **true**
- camera policy preserved: **true**
- replacement pieces built: **7**
- renderers suppressed: **81**
- housing renderers suppressed: **6**
- Granero/Cuartel moved: **false**
- Hero Bastion changed: **false**
- Final Look changed: **false**
- Tripo / paid credits: **0 / 0**

Matched BEFORE/AFTER exists for all five required views.

## Visual review

Improvements: the mismatched brown technique is gone; the final candidate stays inside the certified Valoria stone vocabulary; architecture-rock seams are more authored; PAN Granero/intermediate gain clearer structural termination; the west housing subset has less hidden double-stacking; Hero Bastion stays dominant.

Why this is not a visual pass: HOME changes too little; several new stone pieces still read as repeated defensive modules rather than one authored urban retaining system; PAN Cuartel still exposes placed-module logic; the dominant orange-roof lower-city concentration remains too compressed; lower-city height/silhouette hierarchy remains weak. The frame still reads closer to **Hero Bastion + secondary kit** than to one premium authored city.

## House-cluster conclusion

The user's observation is confirmed: density is being produced too much by immediate repetition rather than articulated urban structure. The correct direction is not more houses. The final pass improved a west subset but did not solve the dominant visible roof/frontage band.

A future solution should form 2–3 differentiated frontage groups with clear height hierarchy, courts/lanes, fewer immediate roof repeats and integrated stone/rock bases. Do not solve it by duplicating another donor house family.

## Canonical reference caveat

Repo documentation points to references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg, but that binary is not present in the audited Git tree. Therefore this block does not claim literal pixel-to-pixel scoring against the missing file. Evaluation used the canonical written hierarchy/density/cohesion rules in the visual bible and benchmark. Restore the reference binary before a future block claims direct image-to-image scoring.

## Promotion

**NOT PROMOTED.** ValoriaSecondarySupportFormReplacementV1.Enabled remains false and no production runtime hook was added.

## Direct answer

**No — not yet.** Valoria has not made a sufficiently large visible jump in secondary/support forms to say it has escaped the “Hero Bastion + filler/prototype” read. The final technique is directionally cleaner and technically safe, but the frame-level gain is only partial.

## Recommended next block

**VALORIA LOWER-CITY URBAN MASSING REAUTHORING v1** — one screen-first intervention on the dominant orange-roof/frontage band: recompose it into 2–3 authored groups with height/silhouette variation, breathing courts and integrated stone/rock support bases. No new primary functional buildings, no camera work, no density-by-duplication. Promote only if the five matched views clearly lose the prefab/kit-bashed read.

## Closeout confirmation

Closing the request retriggered the same matched gate. Confirmation run **37219883054 — SUCCESS**, artifact **11309782754**. This is a technical confirmation only; the visual verdict remains **PARTIAL-FAIL / NOT PROMOTED**.
